using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor.Rendering;
using UnityEditor;
using UnityEngine;
using System.Linq;
using AssetChecker.Interface;
using AssetChecker.Define;

namespace AssetChecker.ShaderAssetCheck
{
    public class ShaderAssetChecker : AssetBaseCheck, IAssetChecker
    {
        static string reportDirName = string.Empty;
        public static ShaderCheckerDefine globalSettings;
        static List<ShaderCompiledInfo> shaderCompiledInfos = new List<ShaderCompiledInfo>();
        static Dictionary<string, List<string>> shaderLibFuncs = new Dictionary<string, List<string>>();


        [MenuItem("Tools/AssetChecker/ShaderAssetChecker")]
        public static void StartCheck()
        {
            var guid = EditorUserSettings.GetConfigValue(AssetCheckDataDefine.ShaderCheckCfgFile);
            var scfigFile = AssetDatabase.GUIDToAssetPath(guid);
            if (!CheckAndInitRunEnv(scfigFile))
            {
                return;
            }
            reportDirName = DateTime.Now.ToString("yyyyMMdd_hhmmss");
            var (webroot, template) = PreprocAssets();
            webroot += reportDirName + "/";
            if (!Directory.Exists(webroot))
            {
                Directory.CreateDirectory(webroot);
            }
            var tempAssetPaths = AssetDatabase.GetAllAssetPaths();
            var assetChecker = new ShaderAssetChecker();
            assetChecker.DoAssetCheck(template, webroot, tempAssetPaths);
        }

        static bool CheckAndInitRunEnv(string scfigFile)
        {
            if (!File.Exists(scfigFile))
            {
                UnityEngine.Debug.LogError("!!!ShaderCheckerDefine文件未找到，如果还未创建，请右键选择Create/ShaderCheckerDefine创建后，再执行！");
                return false;
            }
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                UnityEngine.Debug.LogError("!!!需要在安卓平台编译shader，请先切换到安卓平台，再执行！");
                return false;
            }
            if (!string.IsNullOrEmpty(EditorPrefs.GetString("kScriptsDefaultApp", "")))
            {
                UnityEngine.Debug.LogError("!!!功能使用了OpenCompiledShader函数打开代码关联的IDE，为了节省内存与时间，请先到Preferences/External Tools设置成Open by file extension再执行！");
                return false;
            }
            globalSettings = AssetDatabase.LoadAssetAtPath<ShaderCheckerDefine>(scfigFile);
            if (globalSettings == null)
            {
                UnityEngine.Debug.LogError("初始化全局配置失败，请检查再执行！");
                return false;
            }
            if (string.IsNullOrEmpty(globalSettings.maliGPUCompiler))
            {
                UnityEngine.Debug.LogError("请先在ShaderCheckerDefine上设置malioc的路径，若未安装，下载地址：https://developer.arm.com/Tools%20and%20Software/Mali%20Offline%20Compiler");
                return false;
            }
            else
            {
                string extension = string.Empty;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    extension = ".exe";
                }
                var compilerPath = Path.Combine(globalSettings.maliGPUCompiler, "malioc" + extension);
                if (!File.Exists(compilerPath))
                {
                    UnityEngine.Debug.LogError("malioc的文件路径未找到，请检查再执行！");
                    return false;
                }
            }
            return true;
        }

        [MenuItem("Assets/Create/ShaderCheckerDefine")]
        public static void CreateShaderCheckerData()
        {
            var currDir = GetSelectedPathOrFallback();
            var savePath = Path.Combine(currDir, AssetCheckDataDefine.ShaderCheckCfgFile);
            if (!string.IsNullOrEmpty(savePath))
            {
                savePath = savePath.Replace(Application.dataPath, "Assets");

                var asset = ScriptableObject.CreateInstance<ShaderCheckerDefine>();
                AssetDatabase.CreateAsset(asset, savePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var guid = AssetDatabase.GUIDFromAssetPath(savePath);
                EditorUserSettings.SetConfigValue(AssetCheckDataDefine.ShaderCheckCfgFile, guid.ToString());
            }
        }

        public void DoAssetCheck(string template, string webroot, string[] tempAssetPaths)
        {
            InitAssetCheck();
            //CheckUseIllegalShader(template, webroot, tempAssetPaths);
            CheckShaderComplexity(template, webroot, tempAssetPaths);
            CheckShaderSlowFunc(template, webroot, tempAssetPaths);
            SaveOverview(webroot, true);
        }

        private void InitAssetCheck()
        {
            shaderCompiledInfos.Clear();
        }

        private bool IsNeedAnalysePath(string path)
        {
            return true;
            //return AssetDatabase.GetImplicitAssetBundleName(path) != "";
        }

        /// <summary>
        /// 检查Shader复杂度
        /// </summary>
        private void CheckShaderComplexity(string template, string webroot, string[] tempAssetPaths)
        {
            var maliGpuInfo = globalSettings.maliGpuInfo;
            var gpuType = maliGpuInfo._type.ToString().Insert(4, "-");
            var gpuComplexity = maliGpuInfo._complexity;

            var result = new List<ShaderComplexityInfo>();
            GetShaderComplexityReport(tempAssetPaths, gpuType, ref result);

            result.Sort((a, b) =>
            {
                return float.Parse(b._complexity).CompareTo(float.Parse(a._complexity));
            });

            var index = 0;
            var values = new List<List<string>>();
            var reportShaders = new StringBuilder();

            var htmlTitles = new List<TableTitle>()
            {
                new TableTitle("index"),
                new TableTitle("Shader Path", "center"),
                new TableTitle("GlobalKeywords"),
                new TableTitle("LocalKeywords"),
                new TableTitle("Keywords"),
                new TableTitle("WorkRegNum↓"),
                new TableTitle("UniformRegNum↓"),
                new TableTitle("StackSpilling↓"),
                new TableTitle("16Arithmetic↑"),
                new TableTitle("A↓"),
                new TableTitle("LS↓"),
                new TableTitle("V↓"),
                new TableTitle("T↓"),
                new TableTitle("Bound"),
                new TableTitle("Complexity↓"),
                new TableTitle("Specification"),
            };
            reportShaders.AppendLine(GetReportTitle(htmlTitles));
            var rowFormat = GetRowFormat(htmlTitles.Count);

            var shaderComplexity = globalSettings.maliGpuInfo._complexity;
            foreach (var item in result)
            {
                var assetPath = item._trunkFile;
                var complexity = float.Parse(item._complexity);
                var globalWords = item._globalKeywords;
                var localWords = item._localKeywords;
                var keywords = item._keywords;
                var complexityStr = complexity > gpuComplexity ? FormatFocusText(item._complexity) : item._complexity;

                if (!IsShowByKeyword(item._globalKeywords))
                {
                    continue;
                }
                var workRegInfo = item._workRegInfo;
                var uniformRegCount = item._uniformRegCount;
                var stackSpilling = item._stackSpilling;
                var _16Arithmetic = item._16Arithmetic;
                var _A = item._A;
                var _LS = item._LS;
                var _V = item._V;
                var _T = item._T;
                var _Bound = item._bound;

                var strMsg = string.Format(rowFormat, index, assetPath, globalWords, localWords, keywords,
                    workRegInfo, uniformRegCount, stackSpilling, _16Arithmetic, _A, _LS, _V, _T, _Bound, complexityStr, shaderComplexity);
                reportShaders.AppendLine(strMsg);

                index++;
                values.Add(new List<string>()
                {
                    index.ToString(), assetPath, globalWords, localWords, keywords, workRegInfo, uniformRegCount, stackSpilling, _16Arithmetic, item._complexity, shaderComplexity.ToString()
                });
            }
            WriteReportFile(template, reportShaders.ToString(), "Shader计算复杂度", webroot, "report_shader_complexity", GetExcelLink("report_shader_complexity"));

            var titles = new List<ExcelTitle>()
            {
                new ExcelTitle("index", 10),
                new ExcelTitle("Shader Path", 100),
                new ExcelTitle("GlobalKeywords", 100),
                new ExcelTitle("LocalKeywords", 100),
                new ExcelTitle("Keywords", 100),
                new ExcelTitle("WorkRegNum", 10),
                new ExcelTitle("UniformRegNum", 10),
                new ExcelTitle("StackSpilling", 10),
                new ExcelTitle("16Arithmetic", 10),
                new ExcelTitle("A↓", 10),
                new ExcelTitle("LS↓", 10),
                new ExcelTitle("V↓", 10),
                new ExcelTitle("T↓", 10),
                new ExcelTitle("Bound", 10),
                new ExcelTitle("Complexity", 10),
            };
            ExportExcel(webroot + "report_shader_complexity.xlsx", titles, values);
        }

        private bool IsShowByKeyword(string keyword)
        {
            foreach (var item in globalSettings.excludeShaderKeywords)
            {
                if (keyword == item)
                {
                    return false;
                }
                else if (keyword.Contains(string.Format(" {0} ", item)))
                {
                    return false;
                }
                else if (keyword.StartsWith(string.Format("{0} ", item)))
                {
                    return false;
                }
                else if (keyword.EndsWith(string.Format(" {0}", item)))
                {
                    return false;
                }
            }
            return true;
        }

        [MenuItem("Assets/Print Shader Complexity")]
        private static void DisplayShaderComplexity()
        {
            if (Selection.activeObject == null)
                return;

            var shaderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!shaderPath.EndsWith(".shader"))
            {
                return;
            }
            globalSettings = AssetDatabase.LoadAssetAtPath<ShaderCheckerDefine>("Assets/" + AssetCheckDataDefine.ShaderCheckCfgFile);
            if (globalSettings == null)
            {
                throw new Exception("初始化全局配置失败，请检查再执行！");
            }
            UnityEngine.Debug.LogError(shaderPath);

            var maliGpuInfo = globalSettings.maliGpuInfo;
            var gpuType = maliGpuInfo._type.ToString().Insert(4, "-");

            var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);

            var dic = new List<ShaderComplexityInfo>();

            new ShaderAssetChecker().GetAnShaderComplexityInternal(shader, shaderPath, gpuType, ref dic);

            foreach (var item in dic)
            {
                var text = item.ToString() + " Specification:>" + 
                    maliGpuInfo._complexity;
                UnityEngine.Debug.Log(text);
            }
        }

        private void GetAnShaderComplexityInternal(Shader shader, string shaderPath, string gpuType, ref List<ShaderComplexityInfo> dic)
        {
            int defaultMask = (1 << (int)ShaderCompilerPlatform.GLES3x);
            var tempPath = AssetPath.Substring(0, AssetPath.LastIndexOf('/')) + "/Temp/";

            OpenCompiledShader(shader, 1, defaultMask, false, false, false);

            var filePath = tempPath + "Compiled-" + shader.name.Replace('/', '-');
            var (trunkList, shaderCodes) = ParseShaderCompiledCode(filePath, shaderPath, gpuType, dic);

            var dataPath = Application.dataPath;
            foreach (var item in trunkList)
            {
                var variant = GetVariantKeywordName(shaderCodes, item._index);
                var (complexity, workRegInfo, uniformRegCount, stackSpilling, _16Arithmetic,
                    _A, _LS, _V, _T, _bound) = CalcShaderComplexity(item._trunkFile, gpuType, dataPath);
                dic.Add(new ShaderComplexityInfo()
                {
                    _globalKeywords = variant.Item1,
                    _localKeywords = variant.Item2,
                    _keywords = variant.Item3,
                    _trunkFile = item._trunkFile,
                    _workRegInfo = workRegInfo,
                    _uniformRegCount = uniformRegCount,
                    _stackSpilling = stackSpilling,
                    _16Arithmetic = _16Arithmetic,
                    _A = _A,
                    _LS = _LS,
                    _T = _T,
                    _V = _V,
                    _bound = _bound,
                    _complexity = complexity.ToString(),
                });
            }
        }

        private void GetShaderComplexityReport(string[] tempAssetPaths, string gpuType, ref List<ShaderComplexityInfo> dic)
        {
            foreach (var shaderPath in tempAssetPaths)
            {
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);

                if (shaderPath.EndsWith(".shader") && IsCanParseShader(shader.name) && IsNeedAnalysePath(shaderPath))
                {
                    GetAnShaderComplexityInternal(shader, shaderPath, gpuType, ref dic);
                }
            }
        }

        private bool IsCanParseShader(string shaderName)
        {
            foreach (var item in globalSettings.IgnoreShaders)
            {
                if (shaderName.Equals(item))
                {
                    return false;
                }
            }
            foreach (var item in globalSettings.IgnoreShaderBatchs)
            {
                if (shaderName.StartsWith(item))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 分析shader编译后代码
        /// </summary>
        (List<ShaderTrunkInfo>, string[]) ParseShaderCompiledCode(string filePath, string shaderPath, string gpuType, List<ShaderComplexityInfo> dic)
        {
            var vertIndex = 0;
            var fragIndex = 0;
            var stateType = ShaderTrunkType.None;
            var lastState = ShaderTrunkType.None;
            var codeSnippets = new List<string>();
            var trunkList = new List<ShaderTrunkInfo>();

            var shaderCompInfo = new ShaderCompiledInfo()
            {
                _assetPath = shaderPath,
            };
            shaderCompInfo._codeLines = new List<string>();
            shaderCompInfo._minLines = new List<string>();
            shaderCompInfo._maxLines = new List<string>();
            shaderCompiledInfos.Add(shaderCompInfo);

            var shaderCodes = File.ReadAllLines(filePath + ".shader");
            for (int i = 0; i < shaderCodes.Length; i++)
            {
                var line = shaderCodes[i];
                if (line.StartsWith("#ifdef VERTEX"))
                {
                    stateType = ShaderTrunkType.Vertex;
                    codeSnippets.Clear();
                    continue;
                }
                if (line.StartsWith("#ifdef FRAGMENT"))
                {
                    stateType = ShaderTrunkType.Fragment;
                    codeSnippets.Clear();
                    continue;
                }
                if (line.StartsWith("void main()") && stateType != ShaderTrunkType.None)
                {
                    lastState = stateType;
                    stateType = ShaderTrunkType.Main;
                }
                if (stateType == ShaderTrunkType.Main && line.StartsWith("}"))
                {
                    codeSnippets.Add(line);
                    if (lastState == ShaderTrunkType.Vertex)
                    {
                        var vertFile = filePath + "_" + (vertIndex++) + ".vert";
                        File.WriteAllLines(vertFile, codeSnippets.ToArray());

                        trunkList.Add(new ShaderTrunkInfo()
                        {
                            _trunkFile = vertFile,
                            _index = i,
                        });
                    }
                    else if (lastState == ShaderTrunkType.Fragment)
                    {
                        var fragFile = filePath + "_" + (fragIndex++) + ".frag";
                        File.WriteAllLines(fragFile, codeSnippets.ToArray());

                        trunkList.Add(new ShaderTrunkInfo()
                        {
                            _trunkFile = fragFile,
                            _index = i,
                        });

                        if (shaderCompInfo._minLines.Count == 0)
                        {
                            foreach (var item in shaderCompInfo._codeLines)
                                shaderCompInfo._minLines.Add(item);
                        }
                        else
                        {
                            if (shaderCompInfo._codeLines.Count > 0 && shaderCompInfo._minLines.Count > shaderCompInfo._codeLines.Count)
                            {
                                shaderCompInfo._minLines.Clear();
                                foreach (var item in shaderCompInfo._codeLines)
                                    shaderCompInfo._minLines.Add(item);
                            }
                        }

                        if (shaderCompInfo._maxLines.Count == 0)
                        {
                            foreach (var item in shaderCompInfo._codeLines)
                                shaderCompInfo._maxLines.Add(item);
                        }
                        else
                        {
                            if (shaderCompInfo._maxLines.Count < shaderCompInfo._codeLines.Count)
                            {
                                shaderCompInfo._maxLines.Clear();

                                foreach (var item in shaderCompInfo._codeLines)
                                    shaderCompInfo._maxLines.Add(item);
                            }
                        }
                        shaderCompInfo._codeLines.Clear();
                    }
                    stateType = ShaderTrunkType.None;
                    lastState = ShaderTrunkType.None;
                }
                else
                {
                    if (stateType == ShaderTrunkType.Vertex || stateType == ShaderTrunkType.Fragment || stateType == ShaderTrunkType.Main)
                    {
                        if (line.Contains("#version 300 es"))
                        {
                            codeSnippets.Add("#version 310 es");    //unity bug
                        }
                        else
                        {
                            codeSnippets.Add(line);
                            ParseShaderCompCode(line, shaderCompInfo);
                        }
                    }
                }
            }
            shaderCompInfo._variantNum = fragIndex;

            return (trunkList, shaderCodes);
        }

        /// <summary>
        /// 分析shader编译后代码
        /// </summary>
        void ParseShaderCompCode(string line, ShaderCompiledInfo shaderCompInfo)
        {
            var funcNames = TryGetContainsShaderFunc(line, globalSettings.compiledShaderFuncs);
            if (funcNames.Count > 0)
            {
                shaderCompInfo._codeLines.Add(line.Trim());
            }
        }

        (string, string, string) GetVariantKeywordName(string[] shaderCodes, int index)
        {
            string global = "none";
            string local = "none";
            string keywords = "none";
            for (int i = index; i >= 0; i--)
            {
                var line = shaderCodes[i];
                if (line.StartsWith("Local Keywords:"))
                {
                    var strs = line.Split(':');
                    local = strs[1].Trim();
                    continue;
                }
                else if (line.StartsWith("Global Keywords:"))
                {
                    var strs = line.Split(':');
                    global = strs[1].Trim();
                    break;
                }
                else if (line.StartsWith("Keywords:"))
                {
                    var strs = line.Split(':');
                    keywords = strs[1].Trim();
                    break;
                }
            }
            return (global, local, keywords);
        }

        (float, string, string, string, string, string, string, string, string, string) CalcShaderComplexity(string destFile, string gpuType, string dataPath)
        {
            float cycles = 0;                   //Shader生成的所有指令的累积执行周期数
            string workRegInfo = "";            //该shader工作使用的寄存器数量，减少提升性能
            string uniformRegCount = "";        //存储着色器可能需要的常量，所有线程都有共享uniform register
            string stackSpilling = "";          //是否有变量被放置到栈内存中，有的话GPU读取是性能消耗较大
            string _16Arithmetic = "";          //以16位或更低精度执行的算术运算的百分比。数值越高代表shader性能越好
            string _A = "", _LS = "", _V = "", _T = "", _bound = "";
            if (File.Exists(destFile))
            {
                var toolPath = Path.GetFullPath(Application.dataPath + "/../Tools/mali_offline_compiler/");
                var compilerPath = Directory.Exists(toolPath) ? toolPath : globalSettings.maliGPUCompiler;

                var proc = Path.Combine(compilerPath, "malioc");
                var args = string.Format("{0} -c {1}", "\"" + destFile + "\"", gpuType);
                var info = GetCommandLine(proc, args);
                using (var process = Process.Start(info))
                {
                    process.WaitForExit();
                    string msg = process.StandardOutput.ReadToEnd();
                    if (!string.IsNullOrEmpty(msg))
                    {
                        if (msg.Contains("Compilation failed."))
                        {
                            UnityEngine.Debug.LogError(destFile);
                            UnityEngine.Debug.LogError(msg);
                            UnityEngine.Debug.LogError("---------------------------------------------------------------------->>>>>");
                        }
                        else
                        {
                            var lines = msg.Split('\n');
                            var lastLine = string.Empty;
                            var splitStr = new string[] { "  " };
                            var constTotalCycles = "Total instruction cycles:";
                            foreach (var item in lines)
                            {
                                if (item.StartsWith(constTotalCycles))
                                {
                                    var line = item.Replace(constTotalCycles, string.Empty);
                                    var strs = line.Split(splitStr, StringSplitOptions.RemoveEmptyEntries);

                                    _A = strs[0].Trim();
                                    _LS = strs[1].Trim();
                                    //UnityEngine.Debug.LogError(item + " " + lastLine + " " + strs.Length);

                                    if (strs.Length == 4)
                                    {
                                        _T = strs[2].Trim();
                                        _bound = strs[3].Trim();

                                        cycles = float.Parse(_A) + float.Parse(_LS) + float.Parse(_T);
                                    }
                                    else if (strs.Length == 5)
                                    {
                                        _V = strs[2].Trim();
                                        _T = strs[3].Trim();
                                        _bound = strs[4].Trim();

                                        cycles = float.Parse(_A) + float.Parse(_LS) + float.Parse(_V) + float.Parse(_T);
                                    }
                                }
                                if (item.StartsWith("Work registers"))
                                {
                                    var strs = item.Split(':');
                                    workRegInfo = strs[1];
                                }
                                if (item.StartsWith("Uniform registers"))
                                {
                                    var strs = item.Split(':');
                                    uniformRegCount = strs[1];
                                }
                                if (item.StartsWith("Stack spilling"))
                                {
                                    var strs = item.Split(':');
                                    stackSpilling = strs[1].Trim();
                                }
                                if (item.StartsWith("16-bit arithmetic"))
                                {
                                    var strs = item.Split(':');
                                    _16Arithmetic = strs[1];
                                }
                                lastLine = item;
                            }
                        }
                    }
                    process.Close();
                }
            }
            return (cycles, workRegInfo, uniformRegCount, stackSpilling, _16Arithmetic, _A, _LS, _V, _T, _bound);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 检查违法的shader
        /// </summary>
        void CheckUseIllegalShader(string template, string webroot, string[] tempAssetPaths)
        {
            var keyValues = new Dictionary<string, List<string>>();
            foreach (var item in tempAssetPaths)
            {
                if (!item.StartsWith(AssetPath) || !item.EndsWith(".prefab")) continue;

                var gameObj = AssetDatabase.LoadAssetAtPath<GameObject>(item);
                var meshRenders = gameObj.GetComponentsInChildren<MeshRenderer>();
                foreach (var render in meshRenders)
                {
                    AddIllegalShaderKeyValues(item, render.sharedMaterials, ref keyValues);
                }
                var skinRenders = gameObj.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var render in skinRenders)
                {
                    AddIllegalShaderKeyValues(item, render.sharedMaterials, ref keyValues);
                }
                var particleRenders = gameObj.GetComponentsInChildren<ParticleSystemRenderer>();
                foreach (var render in particleRenders)
                {
                    AddIllegalShaderKeyValues(item, render.sharedMaterials, ref keyValues);
                }
            }
            var index = 0;
            var values = new List<List<string>>();
            var reportShaders = new StringBuilder();

            var htmlTitles = new List<TableTitle>()
            {
                new TableTitle("index"),
                new TableTitle("Asset Path", "center"),
                new TableTitle("Shader Path"),
                new TableTitle("author"),
                new TableTitle("orderid"),
            };
            reportShaders.AppendLine(GetReportTitle(htmlTitles));
            var rowFormat = GetRowFormat(htmlTitles.Count);

            foreach (var item in keyValues)
            {
                index++;
                var assetPath = item.Key;
                var shaderPaths = string.Join("<br>", item.Value);
                var fileVerInfo = GetFileVerInfo(assetPath);
                var strMsg = string.Format(rowFormat, index, assetPath, shaderPaths, fileVerInfo._author, fileVerInfo._orderid);
                reportShaders.AppendLine(strMsg);

                var exShaderPaths = string.Join("\n", item.Value);
                values.Add(new List<string>()
                {
                    index.ToString(), assetPath, exShaderPaths, fileVerInfo._author, fileVerInfo._orderid
                });
            }
            WriteReportFile(template, reportShaders.ToString(), "使用非法的shader", webroot, "report_shaders", GetExcelLink("report_shaders"));

            var titles = new List<ExcelTitle>()
            {
                new ExcelTitle("index", 10),
                new ExcelTitle("Asset Path", 100),
                new ExcelTitle("Shader Path", 100),
                new ExcelTitle("author", 10),
                new ExcelTitle("orderid", 10),
            };
            ExportExcel(webroot + "report_shaders.xlsx", titles, values);
        }

        void AddIllegalShaderKeyValues(string assetPath, Material[] mates, ref Dictionary<string, List<string>> keyValues)
        {
            foreach (var mate in mates)
            {
                if (mate == null || mate.shader == null) continue;
                var shaderPath = AssetDatabase.GetAssetPath(mate.shader);

                if (!IsValiedShaderPath(shaderPath))
                {
                    keyValues.TryGetValue(assetPath, out List<string> list);
                    if (list == null)
                    {
                        list = new List<string>();
                        keyValues.Add(assetPath, list);
                    }
                    if (!string.IsNullOrEmpty(shaderPath))
                    {
                        if (shaderPath == "Resources/unity_builtin_extra" || shaderPath == "Library/unity default resources")
                        {
                            shaderPath = "【" + mate.name + "】 " + mate.shader.name;
                        }
                        list.Add(shaderPath);
                    }
                }
            }
        }

        bool IsValiedShaderPath(string path)
        {
            var ShaderCustomPaths = globalSettings.shaderPaths;
            if (ShaderCustomPaths.Count == 0)
            {
                throw new Exception("Shader 存放路径没有设置！！！");
            }
            bool isValied = false;
            foreach (var item in ShaderCustomPaths)
            {
                if (path.StartsWith(item))
                {
                    isValied = true;
                    break;
                }
            }
            return isValied;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 检查shader函数
        /// </summary>
        void CheckShaderSlowFunc(string template, string webroot, string[] tempAssetPaths)
        {
            var dic = new Dictionary<string, ShaderFuncInfo>();
            foreach (var item in tempAssetPaths)
            {
                if (!item.EndsWith(".shader") || !IsNeedAnalysePath(item))
                {
                    continue;
                }
                var stateType = ShaderTrunkType.None;
                var lines = File.ReadAllLines(item);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();

                    ////检查Properties部分代码///
                    if (line.StartsWith("Properties"))
                    {
                        stateType = ShaderTrunkType.Properties;
                    }
                    if (stateType == ShaderTrunkType.Properties)
                    {
                        if (line.StartsWith("}"))
                        {
                            stateType = ShaderTrunkType.Main;
                        }
                        else
                        {
                            ParseShaderProperties(item, line, dic);
                        }
                    }
                    ///检查代码里函数部分
                    ParseShaderFuncName(item, i + 1, line, dic);

                    ///检查代码里#include xxx.hlsl文件///
                    if (line.StartsWith("#include") && line.EndsWith(".hlsl\""))
                    {
                        var hlslPath = line.Replace("#include", "").Replace("\"", "").Trim();
                        ParseHlslInShader(item, hlslPath, dic, tempAssetPaths);
                    }
                }
                ParseMateList(item, dic, tempAssetPaths);
            }
            var sortedDic = dic.OrderByDescending(x => x.Value._lineCodes.Count).ToDictionary(x => x.Key, x => x.Value);

            var index = 0;
            var values = new List<List<string>>();
            var reportShaders = new StringBuilder();

            var htmlTitles = new List<TableTitle>()
            {
                new TableTitle("index"),
                new TableTitle("Asset Path", "center"),
                new TableTitle("Line Num"),
                new TableTitle("Shader Functions"),
                new TableTitle("Tex2D VarNum"),
                new TableTitle("Tex2D Vars"),
                new TableTitle("Mate Num"),
                new TableTitle("Mate List"),
                new TableTitle("MinCompiled FuncNum"),
                new TableTitle("MinCompiled Code"),
                new TableTitle("MaxCompiled FuncNum"),
                new TableTitle("MaxCompiled Code"),
            };
            reportShaders.AppendLine(GetReportTitle(htmlTitles));
            var rowFormat = GetRowFormat(htmlTitles.Count);

            var pageroot = webroot + "/subpage/";
            if (!Directory.Exists(pageroot))
            {
                Directory.CreateDirectory(pageroot);
            }
            foreach (var item in sortedDic)
            {
                if (item.Value._passCount == 0 && item.Value._propVars.Count == 0 && item.Value._mateList.Count == 0)
                {
                    continue;
                }
                index++;
                var assetPath = item.Key;
                var lineNum = item.Value._lineCodes.Count.ToString();
                var hrefUrl1 = "subpage/linecode_" + index + ".html";
                var shaderFuncFile = webroot + "/" + hrefUrl1;
                var shaderFuncs = CreateMutiLineLink(item.Value._lineCodes, shaderFuncFile, hrefUrl1);

                var propVarNum = item.Value._propVars.Count.ToString();
                var hrefUrl2 = "subpage/propsvar_" + index + ".html";
                var shaderPropsFile = webroot + "/" + hrefUrl2;
                var shaderProps = CreateMutiLineLink(item.Value._propVars, shaderPropsFile, hrefUrl2);

                var mateNum = item.Value._mateList.Count.ToString();
                var hrefUrl3 = "subpage/matelist_" + index + ".html";
                var mateFile = webroot + "/" + hrefUrl3;
                var mateList = CreateMutiLineLink(item.Value._mateList, mateFile, hrefUrl3);

                var minCompCodeNum = "";
                var minCompCodeLine = "";
                var maxCompCodeNum = "";
                var maxCompCodeLine = "";
                var compCodeFuncs = TryGetCompiledShaderObjInfo(item.Key);
                if (compCodeFuncs != null)
                {
                    minCompCodeNum = compCodeFuncs._minLines.Count.ToString();
                    var hrefUrl4 = "subpage/mincompcode_" + index + ".html";
                    var minCompCodeFile = webroot + "/" + hrefUrl4;
                    minCompCodeLine = CreateMutiLineLink(compCodeFuncs._minLines, minCompCodeFile, hrefUrl4);

                    maxCompCodeNum = compCodeFuncs._maxLines.Count.ToString();
                    var hrefUrl5 = "subpage/maxcompcode_" + index + ".html";
                    var maxCompCodeFile = webroot + "/" + hrefUrl5;
                    maxCompCodeLine = CreateMutiLineLink(compCodeFuncs._maxLines, maxCompCodeFile, hrefUrl5);
                }
                var strMsg = string.Format(rowFormat, index, assetPath, lineNum, shaderFuncs, propVarNum, shaderProps, mateNum, mateList, minCompCodeNum, minCompCodeLine, maxCompCodeNum, maxCompCodeLine);
                reportShaders.AppendLine(strMsg);

                var exShaderFuncs = ClearFocusText(shaderFuncs);
                values.Add(new List<string>()
                {
                    index.ToString(), assetPath, lineNum.ToString(), exShaderFuncs, propVarNum, shaderProps, mateNum, mateList, minCompCodeNum, minCompCodeLine, maxCompCodeNum, maxCompCodeLine
                });
            }
            WriteReportFile(template, reportShaders.ToString(), "耗时shader函数分析", webroot, "report_shaderfuncs", GetExcelLink("report_shaderfuncs"));

            var titles = new List<ExcelTitle>()
            {
                new ExcelTitle("index", 10),
                new ExcelTitle("Asset Path", 100),
                new ExcelTitle("Line Num", 10),
                new ExcelTitle("Shader Functions", 100),
                new ExcelTitle("Tex2D VarNum", 10),
                new ExcelTitle("Tex2D Vars", 10),
                new ExcelTitle("Mate Num", 10),
                new ExcelTitle("Mate List", 10),
                new ExcelTitle("MinCompiled FuncNum", 10),
                new ExcelTitle("MinCompiled Code", 10),
                new ExcelTitle("MaxCompiled FuncNum", 10),
                new ExcelTitle("MaxCompiled Code", 10),
            };
            ExportExcel(webroot + "report_shaderfuncs.xlsx", titles, values);
        }

        private void ParseHlslInShader(string assetPath, string line, Dictionary<string, ShaderFuncInfo> dic, string[] tempAssetPaths)
        {
            var funcInfo = TryAddOrGetShaderFunc(assetPath, dic);
            var realPath = GetHlslAssetFilePath(line, tempAssetPaths);
            if (!string.IsNullOrEmpty(realPath))
            {
                shaderLibFuncs.TryGetValue(realPath, out var funcs);
                if (funcs == null)
                {
                    funcs = new List<string>();
                    shaderLibFuncs.Add(realPath, funcs);

                    if (!File.Exists(realPath))
                    {
                        UnityEngine.Debug.LogError("[EX] hlsl file not exist!!");
                        return;
                    }
                    var lines = File.ReadAllLines(realPath);
                    var index = 0;
                    foreach (var item in lines)
                    {
                        index++;
                        var newItem = item.Trim();
                        if (newItem.StartsWith("//") || newItem.StartsWith("#define"))
                        {
                            continue;
                        }
                        var funcNames = TryGetContainsShaderFunc(newItem, globalSettings.shaderFuncs);
                        if (funcNames.Count > 0)
                        {
                            foreach (var funcName in funcNames)
                            {
                                newItem = FormatFuncName(newItem, funcName);
                            }
                            var filename = System.IO.Path.GetFileName(realPath);
                            funcs.Add(string.Format("[{0}] {1}: {2}", filename, index, newItem));
                        }
                    }
                }
                foreach (var func in funcs)
                    funcInfo._lineCodes.Add(func);
            }
        }

        /// <summary>
        /// 获取HLSL真实路径
        /// </summary>
        private string GetHlslAssetFilePath(string path, string[] tempAssetPaths)
        {
            if (path.StartsWith("Packages/"))
            {
                var pathInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
                var packStrs = new List<string>(path.Split('/'));
                if (packStrs[1] == "com.unity.render-pipelines.universal" ||
                    packStrs[1] == "com.unity.render-pipelines.core")
                {
                    return string.Empty;
                }
                packStrs.RemoveRange(0, 2);
                return Path.Combine(pathInfo.resolvedPath, string.Join("/", packStrs));
            }
            if (path.StartsWith("./"))
            {
                path = path.Remove(0, 1);
            }
            foreach (var item in tempAssetPaths)
            {
                if (item.EndsWith(path))
                {
                    return Path.Combine(AssetPath.Replace("/Assets", "/"), item);
                }
            }
            return path;
        }

        public ShaderCompiledInfo TryGetCompiledShaderObjInfo(string assetPath)
        {
            foreach (var item in shaderCompiledInfos)
            {
                if (item._assetPath == assetPath)
                {
                    return item;
                }
            }
            return null;
        }

        public List<string> TryGetContainsShaderFunc(string str, List<string> funcList)
        {
            var result = new List<string>();
            foreach (var item in funcList)
            {
                if (str.Contains(item + "("))   //匹配带上(，否则会不准
                {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// 分析shader属性列表
        /// </summary>
        void ParseShaderProperties(string assetPath, string line, Dictionary<string, ShaderFuncInfo> dic)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) return;
            var funcInfo = TryAddOrGetShaderFunc(assetPath, dic);
            if (line.Contains("2D)") || line.Contains("2DArray)") || line.Contains("CUBE)"))
            {
                funcInfo._propVars.Add(line);
            }
        }

        void ParseShaderFuncName(string assetPath, int v, string line, Dictionary<string, ShaderFuncInfo> dic)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) return;
            var funcNames = TryGetContainsShaderFunc(line, globalSettings.shaderFuncs);
            if (funcNames.Count > 0)
            {
                if (line.Contains("//"))    //排除 float4 col = 0;// tex2D(_MainTex, i.uv);
                {
                    var strs = line.Split(new string[] { "//" }, StringSplitOptions.None);
                    var isExistFunc = false;

                    foreach (var funcName in funcNames)
                    {
                        if (strs[0].Contains(funcName))
                        {
                            isExistFunc = true;
                            break;
                        }
                    }
                    if (!isExistFunc)
                    {
                        return;
                    }
                }
                var funcInfo = TryAddOrGetShaderFunc(assetPath, dic);
                foreach (var funcName in funcNames)
                {
                    line = FormatFuncName(line, funcName);
                }
                line = v + ": " + line;

                var shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);

                funcInfo._passCount = shader.passCount;
                funcInfo._lineCodes.Add(line);
            }
        }

        ShaderFuncInfo TryAddOrGetShaderFunc(string assetPath, Dictionary<string, ShaderFuncInfo> dic)
        {
            dic.TryGetValue(assetPath, out ShaderFuncInfo funcInfo);
            if (funcInfo == null)
            {
                funcInfo = new ShaderFuncInfo();
                funcInfo._lineCodes = new List<string>();
                funcInfo._propVars = new List<string>();
                funcInfo._mateList = new List<string>();
                dic.Add(assetPath, funcInfo);
            }
            return funcInfo;
        }

        public void ParseMateList(string shaderPath, Dictionary<string, ShaderFuncInfo> dic, string[] tempAssetPaths)
        {
            var shaderFuncInfo = TryAddOrGetShaderFunc(shaderPath, dic);
            foreach (var assetPath in tempAssetPaths)
            {
                if (!assetPath.EndsWith(".mat"))
                {
                    continue;
                }
                var mateObj = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (mateObj != null && mateObj.shader != null)
                {
                    var mateShaderPath = AssetDatabase.GetAssetPath(mateObj.shader);
                    if (shaderPath == mateShaderPath)
                    {
                        shaderFuncInfo._mateList.Add(assetPath);
                    }
                }
            }
        }
    }
}

