/*
 * @Description: 项目检测工具配置
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System.Linq;

namespace CasualEngine.ProjectScanTool
{
    /// <summary>
    /// 工具工作台，日志显示级别
    /// </summary>
    public enum LogDisplayLevel
    {
        eNormal = 0, //#ffffff 默认的输出，白色文字
        eCorrect = 1, //#00ff00  资源扫描正常，绿色显示
        eWarning = 2, //#ffa500 资源扫描警告，黄色警示
        eError = 3, //#ff0000 //资源异常，红色提示
    }

    /// <summary>
    /// 开启或关闭当前扫描种类下的所有子类
    /// </summary>
    public enum EnumScanEnable
    {
        eClose = 0,
        eOpen = 1,
    }

    /// <summary>
    /// 音频文件命名规范匹配方式
    /// </summary>
    public enum EnumAudioNameMatchRule
    {
        前缀匹配 = 1,
        后缀匹配 = 2,
        包含即可 = 3
    }

    /// <summary>
    /// 文件磁盘大小单位
    /// </summary>
    public enum EnumStorageMemoryType
    {
        B = 1,
        KB = 1024,
        M = 1024 * 1024,
        G = 1024 * 1024 * 1024
    }

    /// <summary>
    /// 贴图大小扫描类型
    /// </summary>
    public enum EnumImgSizeScanType
    {
        eScan_w_h = 0,  //宽高扫描
        eScan_fixed_mul, //固定倍数扫描，如贴图设置Android是etc2会检测尺寸是4的倍数
        eScan_square,   //正方形扫描   ios pvrtc是正方形
    }

    /// <summary>
    /// 开启或关闭扫描的模块
    /// </summary>
    public class CustomScanModeData
    {
        public EnumScanModes scanMode;
        public Type assetType;
        public string description = "";
        //注册的扫描规则列表
        public List<CustomScanRuleData> scanRules = new List<CustomScanRuleData>();
    }

    /// <summary>
    /// 开启或关闭扫描的规则
    /// </summary>
    public class CustomScanRuleData
    {
        public EnumScanModes bindScanMode;
        public Type assetType;
    }

    [InitializeOnLoad]
    public class ProjectScanGlobalConfig
    {
        public static string res_dir = "Assets/GTTools/ProjectScanTool/Editor/Sketchmap/";

        /// <summary>
        /// 项目扫描工程路径
        /// </summary>
        public static string scan_tool_dir = "Assets/GTTools/ProjectScanTool/Editor/";

        /// <summary>
        /// 项目扫描模块配置存储路径
        /// </summary>
        public static string scanTypeConfig_dir = scan_tool_dir + "ScanModeData/";
        public static string EditorLogDir = Directory.GetParent(Application.dataPath).FullName + "/Library/ProjectScanLog";

        /// <summary>
        /// 项目扫描测试场景
        /// </summary>
        public static string test_scene_path = scan_tool_dir + "TestRes/Scene/TestScene.unity";

        /// <summary>
        /// 所有的检查模块定义,在GolbalConfig.InitCustomData中会根据特性反射获取所有自定义的检查模块
        /// </summary>
        public static List<CustomScanModeData> allScanModes = new List<CustomScanModeData>();

        /// <summary>
        /// 所有的检查规则定义,在GolbalConfig.InitCustomData中会根据特性反射获取所有自定义的检查规则
        /// </summary>
        public static List<CustomScanRuleData> allScanChildRules = new List<CustomScanRuleData>();

        /// <summary>
        /// 缓存的开放应用到后处理的规则
        /// </summary>
        public static List<CustomRule> autoPostProcessorRules = new List<CustomRule>();

        /// <summary>
        /// 全局的通用设置
        /// </summary>
        public static GeneralSetting generalSetting;

        /// <summary>
        /// 自定义类的特性togglebox绘制guistyle
        /// </summary>
        public static GUIStyle customToggleTitleStyle;

        /// <summary>
        /// 自定义Lable属性，区别Editor的灰色，显眼一点
        /// </summary>
        public static GUIStyle customLableStyle;

        /// <summary>
        /// 是否开放自动修正权限，默认开放，但是当jenkins等后台走ProjectScanner.RunAll()接口时，权限关闭，避免后台自动处理资源，理论上后台只是检查给出日志，保证安全性
        /// </summary>
        public static bool openAutoCorrection = true;

        /// <summary>
        /// 是否开放后处理权限，除了批处理启动的默认关闭，这里则默认开启，如果是手动点击检测，则关闭后处理，避免部分refresh多次触发扫描
        /// </summary>
        public static bool openCustomPostProccesser = true;

        static ProjectScanGlobalConfig()
        {
            //后台命令行启动的unity禁用自动修正和后处理
            if (Environment.CommandLine.IndexOf("-batchmode") >= 0)
            {
                ProjectScanGlobalConfig.openAutoCorrection = false;
            }
            if (!Directory.Exists(EditorLogDir))
            {
                Directory.CreateDirectory(EditorLogDir);
            }
            InitCustomData();
            InitRes();
        }

        /// <summary>
        /// 初始化自定义的序列化数据
        /// </summary>
        public static void InitCustomData()
        {
            //获取所有自定义检测模块
            ProjectScanGlobalConfig.allScanModes.Clear();
            ProjectScanGlobalConfig.allScanChildRules.Clear();
            Type[] typeList = Assembly.GetExecutingAssembly().GetTypes();
            if (typeList != null)
            {
                foreach (Type type in typeList)
                {
                    //过滤出被[CustomScanTypeAttribute]修饰的类
                    if (type.IsDefined(typeof(CustomScanTypeAttribute), true))
                    {
                        CustomScanTypeAttribute attrib = (CustomScanTypeAttribute)Attribute.GetCustomAttribute((System.Reflection.MemberInfo)type, typeof(CustomScanTypeAttribute));
                        CustomScanModeData modeData = null;
                        foreach (var mode in ProjectScanGlobalConfig.allScanModes)
                        {
                            if (mode.scanMode == attrib.scanMode)
                            {
                                modeData = mode;
                                break;
                            }
                        }
                        if (modeData == null)
                        {
                            modeData = new CustomScanModeData();
                            modeData.scanMode = attrib.scanMode;
                            modeData.description = attrib.scanMode.ToString();
                            ProjectScanGlobalConfig.allScanModes.Add(modeData);
                        }

                        CustomScanRuleData ruleChild = new CustomScanRuleData()
                        {
                            bindScanMode = attrib.scanMode,
                            assetType = type,
                        };
                        modeData.scanRules.Add(ruleChild);
                        ProjectScanGlobalConfig.allScanChildRules.Add(ruleChild);

                    }
                }
            }

            ProjectScanGlobalConfig.allScanModes.Sort((x, y) =>
            {
                return x.scanMode - y.scanMode;
            });

            //初始化assetData，所有检测设置的规则数据序列化assetData
            foreach (var item in ProjectScanGlobalConfig.allScanChildRules)
            {
                if (!File.Exists(ProjectScanGlobalConfig.scanTypeConfig_dir + item.assetType.Name + ".asset"))
                {
                    ProjectScanHelper.CreateScriptableObjectAsset(ProjectScanGlobalConfig.scanTypeConfig_dir, item.assetType, item.assetType.Name);
                }
            }

            generalSetting = AssetDatabase.LoadAssetAtPath<GeneralSetting>(ProjectScanGlobalConfig.scanTypeConfig_dir + typeof(GeneralSetting).Name + ".asset");
            if (generalSetting == null)
            {
                generalSetting = ProjectScanHelper.CreateAsset<GeneralSetting>(ProjectScanGlobalConfig.scanTypeConfig_dir, "GeneralSetting");
            }
            InitOrUpdateAutoPostProcessorRule();
            InitTextureFormatValues();
            InitAllShaderInfos();
        }

        /// <summary>
        /// 缓存的后处理规则
        /// </summary>
        public static void InitOrUpdateAutoPostProcessorRule(bool showLog = false)
        {
            autoPostProcessorRules.Clear();
            foreach (var ruleData in allScanChildRules)
            {
                if (generalSetting.CheckCustomModeIsEnable(ruleData.bindScanMode))
                {
                    var customRuleObj = AssetDatabase.LoadAssetAtPath(ProjectScanGlobalConfig.scanTypeConfig_dir + ruleData.assetType.Name + ".asset", ruleData.assetType);
                    if (customRuleObj == null)
                    {
                        continue;
                    }
                    if (customRuleObj.GetType().BaseType != typeof(CustomRule))
                    {
                        return;
                    }
                    if (customRuleObj.GetType().IsDefined<CustomScanTypeAttribute>())
                    {
                        CustomRule rule = (CustomRule)customRuleObj;
                        if (rule.enable && rule.autoPostProcessor.enable)
                        {
                            var flag = true;
                            //开启了后处理，检查下逻辑函数有没有透传参数
                            var methods = rule.GetType().GetMethods();
                            foreach (var method in methods)
                            {
                                if (method.IsDefined<CustomScanActionAttribute>())
                                {
                                    if (method.GetParameters().Length == 0)
                                    {
                                        Debug.LogErrorFormat("{0}开启了将规则应用到后处理，但是{1}函数并没有实现assetPostprocessorPath的传参，可能当前规则不适合进行后处理。", rule.GetType().Name, method.Name);
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                            if (flag)
                            {
                                autoPostProcessorRules.Add(rule);
                                if (showLog)
                                {
                                    Debug.LogFormat("{0}开启了将规则应用到后处理", rule.GetType().Name);
                                }
                            }
                        }
                    }
                }
            }
            if (showLog)
            {
                Debug.LogFormat("开启应用到后处理的规则共{0}条", autoPostProcessorRules.Count);
            }

        }

        /// <summary>
        /// 初始化工具需要使用的一些资源
        /// </summary>
        public static void InitRes()
        {
        }

        // [UnityEditor.Callbacks.DidReloadScripts]
        // [InitializeOnLoadMethod]
        public static void InitCustomStyle()
        {
            if (customToggleTitleStyle == null)
            {
                customToggleTitleStyle = new GUIStyle();
                // customToggleTitleStyle = (GUIStyle)ProjectScanHelper.DeepCopy(SirenixGUIStyles.ToggleGroupTitleBg);
                customToggleTitleStyle.fixedHeight = 26; //高度重写
                customToggleTitleStyle.fixedWidth = 0;
                customToggleTitleStyle.fontStyle = FontStyle.Normal;
                customToggleTitleStyle.fontSize = 14;
                customToggleTitleStyle.richText = true;
                customToggleTitleStyle.alignment = TextAnchor.MiddleLeft;
                customToggleTitleStyle.border = new RectOffset(15, 7, 4, 4);
                customToggleTitleStyle.clipping = TextClipping.Overflow;
                customToggleTitleStyle.contentOffset = new Vector2(2, -2f); //20->2
                customToggleTitleStyle.focused.textColor = new Color(0, 0, 0, 1);
                customToggleTitleStyle.focused.scaledBackgrounds = SirenixGUIStyles.ToggleGroupTitleBg.focused.scaledBackgrounds;
                customToggleTitleStyle.hover.textColor = new Color(0, 0, 0, 1);
                customToggleTitleStyle.hover.scaledBackgrounds = SirenixGUIStyles.ToggleGroupTitleBg.hover.scaledBackgrounds;
                customToggleTitleStyle.imagePosition = SirenixGUIStyles.ToggleGroupTitleBg.imagePosition;
                customToggleTitleStyle.margin = SirenixGUIStyles.ToggleGroupTitleBg.margin;

                customToggleTitleStyle.normal.background = SirenixGUIStyles.ToggleGroupTitleBg.normal.background;
                customToggleTitleStyle.normal.scaledBackgrounds = SirenixGUIStyles.ToggleGroupTitleBg.normal.scaledBackgrounds;
                customToggleTitleStyle.normal.textColor = Color.white; //颜色重写

                customToggleTitleStyle.onActive.background = SirenixGUIStyles.ToggleGroupTitleBg.onActive.background;
                customToggleTitleStyle.onActive.scaledBackgrounds = SirenixGUIStyles.ToggleGroupTitleBg.onActive.scaledBackgrounds;
                customToggleTitleStyle.onActive.textColor = Color.white; //颜色重写

                customToggleTitleStyle.onFocused = SirenixGUIStyles.ToggleGroupTitleBg.onFocused;
                customToggleTitleStyle.onHover = SirenixGUIStyles.ToggleGroupTitleBg.onHover;
                customToggleTitleStyle.onNormal = SirenixGUIStyles.ToggleGroupTitleBg.onNormal;
                customToggleTitleStyle.overflow = SirenixGUIStyles.ToggleGroupTitleBg.overflow;
                customToggleTitleStyle.padding = SirenixGUIStyles.ToggleGroupTitleBg.overflow;
                customToggleTitleStyle.stretchHeight = SirenixGUIStyles.ToggleGroupTitleBg.stretchHeight;
                customToggleTitleStyle.stretchWidth = SirenixGUIStyles.ToggleGroupTitleBg.stretchWidth;
                customToggleTitleStyle.wordWrap = SirenixGUIStyles.ToggleGroupTitleBg.wordWrap;
            }
            if (customLableStyle == null)
            {
                customLableStyle = new GUIStyle();
                customLableStyle.fixedHeight = ProjectScanGlobalConfig.customToggleTitleStyle.fixedHeight - 4;
                customLableStyle.fontSize = 14;
                customLableStyle.alignment = TextAnchor.MiddleLeft;
                customLableStyle.normal.textColor = Color.white;
            }
        }

        /// <summary>
        /// ProjectScanWindow 打开时的初始化
        /// </summary>
        public static void Init()
        {
            ProjectScanGlobalConfig.InitCustomStyle();
            if (!Directory.Exists(EditorLogDir))
            {
                Directory.CreateDirectory(EditorLogDir);
            }
            InitCustomData();
        }

        public static ValueDropdownList<int> default_formats = new ValueDropdownList<int>() { };  //默认支持的纹理压缩格式
        public static ValueDropdownList<int> win_formats = new ValueDropdownList<int>() { };  //win支持的纹理压缩格式
        public static ValueDropdownList<int> iOS_formats = new ValueDropdownList<int>() { };  //ios支持的纹理压缩格式
        public static ValueDropdownList<int> android_formats = new ValueDropdownList<int>() { };  //android支持的纹理压缩格式
        public static ValueDropdownList<int> all_formats = new ValueDropdownList<int>() { };  //以上3个汇总
        /// <summary>
        /// 初始化不同平台纹理压缩格式
        /// </summary>
        public static void InitTextureFormatValues()
        {
            if (default_formats.Count > 0)
            {
                return;
            }
            //利用反射获取defalut、ios、android实际支持的纹理压缩格式，如果不这样处理，直接TextureImporterFormat作为枚举选择，会有很多非当前平台的压缩参数
            TextureUtils.GetDefaultTextureFormatValuesAndStrings(TextureImporterType.Default, ref default_formats);
            TextureUtils.GetPlatformTextureFormatValuesAndStrings(TextureImporterType.Default, BuildTarget.StandaloneWindows, ref win_formats);
            TextureUtils.GetPlatformTextureFormatValuesAndStrings(TextureImporterType.Default, BuildTarget.iOS, ref iOS_formats);
            TextureUtils.GetPlatformTextureFormatValuesAndStrings(TextureImporterType.Default, BuildTarget.Android, ref android_formats);
            all_formats.Clear();
            all_formats.AddRange(default_formats);
            all_formats.AddRange(win_formats);
            all_formats.AddRange(iOS_formats);
            all_formats.AddRange(android_formats);
        }


        public static ValueDropdownList<string> allShaders = new ValueDropdownList<string>() { };
        /// <summary>
        /// 初始化支持的shader列表，参考源码https://github.com/Unity-Technologies/UnityCsReference/blob/3f0dae724475e51dab2c924c4fa470cfd0269280/Editor/Mono/Inspector/MaterialEditor.cs
        /// 中class ShaderSelectionDropdown 
        /// </summary>   
        public static void InitAllShaderInfos()
        {
            var shaders = ShaderUtil.GetAllShaderInfo();
            var shaderList = new List<string>();

            foreach (var shader in shaders)
            {
                if (shader.name.StartsWith("Deprecated") || shader.name.StartsWith("Hidden"))
                {
                    continue;
                }
                if (shader.hasErrors)
                {
                    continue;
                }
                if (!shader.supported)
                {
                    continue;
                }
                if (shader.name.StartsWith("Legacy Shaders/"))
                {
                    shaderList.Add(shader.name);
                    continue;
                }
                shaderList.Add(shader.name);
            }

            shaderList.Sort();
            var unnestedList = shaderList.Where(s => s.Count(c => c == '/') == 0).ToList();
            shaderList = shaderList.Where(s => s.Count(c => c == '/') > 0).ToList();
            shaderList.AddRange(unnestedList);
            allShaders.Clear();
            foreach (var item in shaderList)
            {
                allShaders.Add(item, item);
            }
        }

    }

}
