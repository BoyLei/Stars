using CSObjectWrapEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Build.Player;
using UnityEngine;
using GameEditor.Tools.Statistics;
using System.Security.Cryptography;
using static BuildMenu;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using UnityEngine.TextCore.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net;
using StarProjectDef;
using UnityEditor.Build;
using Codice.Client.BaseCommands.BranchExplorer;
using static BuildWindow;

public class BuildPack : Editor
{
    public static Dictionary<string, string> CommadLines = new Dictionary<string, string>();
    public static StringBuilder stringBuilder = new StringBuilder();
    public static bool IsSendXiaoShan = false;
    public  static bool  WriteStreamAssest=false;

  

    public static string SceneParent = "map_scenes_map";

    static string[] symbols =
    {
        "GM",
        "STAR_DEV",
        "STAR_SDK",
    };

    static string[] jenkinsconsoles =
    {
        "IsSendXiaoShan",
        "HFS",
        "BuildRes",
        "PackDir",
        "datetime"
    };

    static string[] buildoptions = null;

    public static List<string> BuildScene()
    {
        //环境设置
        List<string> scenes = new List<string>();
        foreach (var item in EditorBuildSettings.scenes)
        {
            if (item.enabled)
            {
                scenes.Add(item.path);
            }
        }

        return scenes;
    }

    public static void BuildAddresable()
    {
        Generator.GenAll();
        BuildRes.ClearAddressablesGroup();
        BuildRes.FreshAssetGroup();
    }

    public static void Log(string message)
    {
        stringBuilder.Append(message);
        stringBuilder.Append("\n");
    }

    public static void Save()
    {
        string filePath = Application.dataPath + "/../BuildLog.txt";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        File.WriteAllText(filePath, stringBuilder.ToString());

    }

    private static void ReadCommadLineArgs()
    {
        Log("读取命令行参数");
        CommadLines.Clear();
        Type buildoptionstype = typeof(BuildOptions);
        buildoptions = buildoptionstype.GetEnumNames();

        List<string> lines = System.Environment.GetCommandLineArgs().KToList();
        if (lines != null && lines.Count > 0)
        {
            int index1 = 0;//lines.IndexOf("-datetime");
            for (int i = 0; i < jenkinsconsoles.Length; i++)
            {
                var name = jenkinsconsoles[i];
                index1 = lines.IndexOf("-" + name);
                if (index1 > -1 && index1 < lines.Count)
                {
                    string jenkinsvalue = string.Empty;
                    if (index1 < lines.Count - 1)
                    {
                        jenkinsvalue = lines[index1 + 1];
                    }
                    CommadLines.Add(name, jenkinsvalue);
                    if (name.Equals("IsSendXiaoShan"))
                    {
                        IsSendXiaoShan = jenkinsvalue == "true";
                    }
                    Log(name + jenkinsvalue);
                }
            }

            index1 = lines.IndexOf("-LanguageType");
            if (index1 > -1 && index1 < lines.Count)
            {
                string languageType_value = string.Empty;
                if (index1 < lines.Count - 1)
                {
                    languageType_value = lines[index1 + 1];
                }
                CommadLines.Add("LanguageType", languageType_value);
                Log("LanguageType:" + languageType_value);
                if (languageType_value.Equals("Chinese"))
                {
                    GameApp.Instance.languageType = LanguageType.Chinese;
                }
                else if (languageType_value.Equals("EngLish"))
                {
                    GameApp.Instance.languageType = LanguageType.English;
                }
            }

            for (int i = 0; i < symbols.Length; i++)
            {
                string symbolname = symbols[i];
                index1 = lines.IndexOf("-" + symbolname);
                bool haschanged = false;
                if (index1 > -1 && index1 < lines.Count)
                {
                    string symbolvalue = string.Empty;
                    if (index1 < lines.Count - 1)
                    {
                        symbolvalue = lines[index1 + 1];
                    }
                    if (!string.IsNullOrEmpty(symbolvalue))
                    {
                        haschanged = true;
                        var target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
                        var group = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup;
                        var name = NamedBuildTarget.FromBuildTargetGroup(group);
                        string scriptDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(name);
                        UnityEngine.Debug.LogError("symbols == " + scriptDefineSymbols);
                        int gmIndex = scriptDefineSymbols.IndexOf(symbolname);
                        if (symbolvalue.Equals("True"))
                        {
                            if (gmIndex == -1)
                            {
                                scriptDefineSymbols = scriptDefineSymbols + ";" + symbolname;
                                PlayerSettings.SetScriptingDefineSymbols(name, scriptDefineSymbols);
                            }
                        }
                        else
                        {
                            if (gmIndex != -1)
                            {
                                scriptDefineSymbols = scriptDefineSymbols.Replace(symbolname, "");
                                PlayerSettings.SetScriptingDefineSymbols(name, scriptDefineSymbols);
                            }
                        }
                        CommadLines.Add(symbolname, symbolvalue);
                    }
                }
                if (haschanged)
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                }
            }
            for (int i = 0; i < buildoptions.Length; i++)
            {
                string optionnames = buildoptions[i];
                index1 = lines.IndexOf("-" + optionnames);
                string optionvalue;
                if (index1 > -1 && index1 < lines.Count)
                {
                    optionvalue = lines[index1 + 1];
                    CommadLines.Add(optionnames, optionvalue);
                }
                else
                {
                    CommadLines.Add(optionnames, "False");
                }
            }
        }

        for (int i = 0; i < buildoptions.Length; i++)
        {
            string optionnames = buildoptions[i];
            if (!CommadLines.ContainsKey(optionnames))
            {
                CommadLines.Add(optionnames, "False");
            }
        }

        for (int i = 0; i < symbols.Length; i++)
        {
            var symbolsname = symbols[i];
            if (!CommadLines.ContainsKey(symbolsname))
            {
                var target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
                var group = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup;
                var name = NamedBuildTarget.FromBuildTargetGroup(group);
                string scriptDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(name);
                int gmIndex = scriptDefineSymbols.IndexOf(symbolsname);
                if (gmIndex != -1)
                {
                    CommadLines.Add(symbolsname, "True");
                }
                else
                {
                    CommadLines.Add(symbolsname, "False");
                }
            }
        }
        if (!CommadLines.ContainsKey("datatime"))
        {
            string data = System.DateTime.Now.ToString("yyyyMMddHHmm");
            CommadLines.Add("datatime", data);
        }
        if (!CommadLines.ContainsKey("PackDir"))
        {
            CommadLines.Add("PackDir", GameApp.Instance.PackDir);
        }
        if (!CommadLines.ContainsKey("HFS"))
        {
            CommadLines.Add("HFS", "http://10.191.72.172:280/jenkinspack/");
        }
        if (!CommadLines.ContainsKey("BuildRes"))
        {
            CommadLines.Add("BuildRes", "True");
        }
    }

    public static BuildOptions CalCommondLineBuildOptions(BuildOptions option)
    {
        for (int i = 0; i < buildoptions.Length; i++)
        {
            string optionnames = buildoptions[i];
            if (CommadLines[optionnames] == "True")
            {
                BuildOptions options = BuildOptions.None;
                if (Enum.TryParse<BuildOptions>(optionnames, true, out options))
                {
                    option |= options;
                }
            }
        }
        return option;
    }

    /// <summary>
    /// 本地包
    /// </summary>
    /// <param name="Options"></param>
    public static void InnerBuildLocalPack(BuildOptions Options = BuildOptions.None,bool ismanual = true,bool buildres = true)
    {
        AssestConfig.Instance.IsUseUpdate = false;//不需要热更
        GameApp.Instance.IsUseUpdate = false;//不需要热更
        
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);//等待工程加载完成
        stringBuilder.Clear();
        Log("xluawrap begin");
        HandleXLuaWrapFile(true);
        Log("xluawrap end");
        BuildPack.Log("MsgPackGenerateCode begin");
        BuildMenu.MsgPackGenerateCode();
        BuildPack.Log("MsgPackGenerateCode end");
        Log("addressable groups begin");
        ReadCommadLineArgs();
        Options = CalCommondLineBuildOptions(Options);
        if ((CommadLines["BuildRes"] == "True"&&!ismanual)||(ismanual&& buildres))
        {
            BuildRes.ClearAddressablesGroup();//清理addresable分组
            AssestConfig.Instance.FreshAssetGroup();//刷新分组
            AssestConfig.Instance.SetAddressableLocal();//设置addresableseting
            BuildRes.BuildAddressables();//打包ab
        }
        UnityEditor.EditorUtility.SetDirty(AssestConfig.Instance);//刷新
        UnityEditor.EditorUtility.SetDirty(GameApp.Instance);//刷新
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
        BuildCurrent(Options);//打包流程
    }

    static void HandleXLuaWrapFile(bool isgen)
    {
        Generator.ClearAll();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
        if (isgen)
        {
            Generator.GenAll();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
    }

    public static void BuildLocalPack()
    {
        InnerBuildLocalPack(BuildOptions.None,false);
    } 

    public static void BuildDeBuggerPack(BuildOptions options)
    {
        AssestConfig.Instance.IsUseUpdate = false;
        GameApp.Instance.IsUseUpdate = false;
        stringBuilder.Clear();
        AssestConfig.Instance.SetAddressableLocal();
        UnityEditor.EditorUtility.SetDirty(AssestConfig.Instance);

        UnityEditor.EditorUtility.SetDirty(GameApp.Instance);
        AssetDatabase.SaveAssets();
        BuildCurrent(options);
    }

    public static void BuildRemotePack()
    {
        InnerBuildRemotePack(BuildOptions.None);
    }

    /// <summary>
    /// 远程包
    /// </summary>
    /// <param name="Options"></param>
    public static void InnerBuildRemotePack(BuildOptions Options = BuildOptions.None)
    {
        AssestConfig.Instance.IsUseUpdate = true;
        AssestConfig.Instance.IsFirstPack = true;
        ReadCommadLineArgs();
        stringBuilder.Clear();
        GameApp.Instance.IsUseUpdate = true;
        BuildRes.ClearAddressablesGroup();
        AssestConfig.Instance.FreshAssetGroup();
        AssestConfig.Instance.SetAddressableRemote();
        UnityEditor.EditorUtility.SetDirty(GameApp.Instance);
        UnityEditor.EditorUtility.SetDirty(AssestConfig.Instance);
        AssetDatabase.SaveAssets();
        BuildRes.BuildAddressables();
       GameApp.Instance.SaveVesion(GameApp.Instance.GetGameVersion());
       BuildCurrent(Options);
    }

    public static void BuildResource()
    {
        //压缩资源
        string sourecPath =GameApp.Instance.BuildDir;
        string destDir = $"{GameApp.Instance.BuildDir}/../OldRes";
        string destPath = $"{destDir}/{GameApp.Instance.Version}.zip";
        
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        if (File.Exists(destPath))
        {
            File.Delete(destPath);
        }
        
        ZipFile.CreateFromDirectory(sourecPath,destPath);

        
        GameApp.Instance.UpdateResVersion();
        if (AssestConfig.Instance.IsFirstPack)
        {
            AssestConfig.Instance.IsFirstPack = false;
            UnityEditor.EditorUtility.SetDirty(AssestConfig.Instance);
            AssetDatabase.SaveAssets();
        }

        var path  = ContentUpdateScript.GetContentStateDataPath(true);

        if (string.IsNullOrEmpty(path))
        {
            UnityEngine.Debug.LogWarning("No path specified for Content State Data file.");
        }
        
        if (!File.Exists(path))
        {
            UnityEngine.Debug.LogWarningFormat("No Content State Data file exists at path: {0}",path);
        }
        else
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
           // settings.OverridePlayerVersion = GameApp.Instance.Version;
            var modifiedEntries = ContentUpdateScript.GatherModifiedEntries(settings, path);

            StringBuilder sbuider = new StringBuilder();

            foreach (var item in modifiedEntries)
            {
                sbuider.AppendLine(item.address);

            }
            Debug.Log(sbuider.ToString());

            if (modifiedEntries.Count > 0)
            {

                var groupName = $"Patch_{GameApp.Instance.Version}";
                ContentUpdateScript.CreateContentUpdateGroup(settings, modifiedEntries
                    ,groupName );

                if (!string.IsNullOrEmpty(path))
                    ContentUpdateScript.BuildContentUpdate(settings, path);
            }
        }
    }

    public static void BuildCode()
    {
        GameApp.Instance.UpdateCodeVersion();
        AssestConfig.Instance.IsFirstPack = false;
        UnityEditor.EditorUtility.SetDirty(AssestConfig.Instance);
        AssetDatabase.SaveAssets();
        BuildScript();
    }
    public static void BuildScript()
    {
        string buildDir = string.Format("{0}/{1}/{2}/{3}/code",
                                         GameApp.Instance.BuildDir,
                                          GameApp.Instance.channel.ToString(),
                                         GameApp.Instance.publish.ToString(),
                                         GameApp.Instance.buildtarget.ToString());

        var files = System.IO.Directory.GetFiles(buildDir);
        foreach (var file in files)
        {
            System.IO.File.Delete(file);
        }

        var target = EditorUserBuildSettings.activeBuildTarget;
        var group = BuildPipeline.GetBuildTargetGroup(target);
        ScriptCompilationSettings scriptCompilationSettings = default;
        scriptCompilationSettings.group = group;
        scriptCompilationSettings.target = target;
        ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildDir);
    }

    public static void BuildCurrent(BuildOptions options= BuildOptions.None,BuildTarget buildTarget = BuildTarget.Android)
    {
        ClearBuildEnvRes();//清理环境
        ClearPlatformUnuseRes();//清理资源
   //     ClearUnuseArtScene();//清理场景
       // AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        DeleteTerrainTextures();
      //  AutoCollectShaderVariantTool.CallDupliateEmbedMaterials();
        var target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        if (target == BuildTarget.Android)
        {
            BuildAndroid(options);
        }
        else if (target == BuildTarget.iOS)
        {
            //BuildIOS(options);
        }
        else if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        {
            BuildWindows(options);
        }
    }
    private static void DeleteTerrainTextures() 
    {
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ArtWorkSpace/Scenes/Scene" });

        foreach (string sceneGUID in sceneGUIDs)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            UnityEngine.Object[] loadedAssets = AssetDatabase.LoadAllAssetsAtPath(scenePath);

            foreach (UnityEngine.Object asset in loadedAssets)
            {
                if (asset is GameObject gameObject)
                {
                    Terrain terrain = gameObject.GetComponent<Terrain>();
                    if (terrain != null)
                    {
                        terrain.terrainData.alphamapResolution = 16;
                    }
                }
            }
        }
        AssetDatabase.Refresh();
    }
    public static string GetBuildTarget()
    {
        if (GameApp.Instance.IsUseUpdate)
        {
            return "remote";
        }
        return "local";
    }

    static string targetShader = "Universal Render Pipeline/Terrain/Lit";
    private static void ClearBuildEnvRes()
    {
        var sd = Shader.Find(targetShader);
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.EndsWith(".mat"))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && mat.shader == sd)
                {
                    Debug.Log("发现资源："+ path);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.Refresh();
                }
            }
        }
        Debug.Log("已清理完毕所有打包无用资源");
    }

    private static void ClearPlatformUnuseRes()
    {
        var target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        string path = "Assets/StreamingAssets/Audio/GeneratedSoundBanks";

        // 只有在目标路径存在时，才进行清理
        if (System.IO.Directory.Exists(path))
        {
            string[] platformsToRemove;

            if (target == BuildTarget.Android)
            {
                platformsToRemove = new[] { "iOS", "Windows" };
            }
            else if (target == BuildTarget.iOS)
            {
                platformsToRemove = new[] { "Android", "Windows" };
            }
            else if (target == BuildTarget.StandaloneWindows)
            {
                platformsToRemove = new[] { "Android", "iOS" };
            }
            else
            {
                // 其他平台不处理
                Debug.Log("当前平台不需要清理资源: " + target);
                return;
            }

            // 循环清理不需要的文件夹
            foreach (string platform in platformsToRemove)
            {
                string platformPath = System.IO.Path.Combine(path, platform);
                if (System.IO.Directory.Exists(platformPath))
                {
                    System.IO.Directory.Delete(platformPath, true);
                    Debug.Log($"{platform} 文件夹已清理");
                }
            }
        }
        else
        {
            Debug.LogWarning("资源路径不存在: " + path);
        }

        Debug.Log("已清理完毕所有打包无用资源");
    }

   

    private static void ClearUnuseArtScene()
    {
        UnuseMapsThisVersion unuseMapsThisVersion;
        string path = "Assets/Res/Map";
        unuseMapsThisVersion = Resources.Load<UnuseMapsThisVersion>("UnuseMapsThisVersion");
        // 只有在目标路径存在时，才进行清理
        if (Directory.Exists(path))
        {
            // 从 ScriptableObject 读取平台名称
            string[] platformsToRemove = unuseMapsThisVersion.removeName;

            foreach (string platform in platformsToRemove)
            {
                string platformPath = Path.Combine(path, platform);

                // 删除文件夹
                if (Directory.Exists(platformPath))
                {
                    Directory.Delete(platformPath, true);
                    Debug.Log($"{platform} 文件夹已清理");
                }

                // 删除对应的.prefab文件（如果存在）
                string prefabPath = Path.Combine(path, platform + ".prefab");
                if (File.Exists(prefabPath))
                {
                    File.Delete(prefabPath);
                    Debug.Log($"{platform}.prefab 文件已删除");
                }

                // 删除对应的.unity场景文件（如果存在）
                string scenePath = Path.Combine(path, platform + ".unity");
                if (File.Exists(scenePath))
                {
                    File.Delete(scenePath);
                    Debug.Log($"{platform}.unity 文件已删除");
                }
            }
        }
        else
        {
            Debug.LogWarning("资源路径不存在: " + path);
        }
        // 刷新编辑器窗口，确保资源状态更新
        AssetDatabase.Refresh();
        Debug.Log("已清理完毕所有打包无用资源");
    }

    public static void ClearHideFlags()
    {
        Shader shader = Shader.Find("Hidden/InternalErrorShader");
        shader.hideFlags = HideFlags.None;
        EditorUtility.SetDirty(shader);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 安卓
    /// </summary>
    //[MenuItem("Build/Android")]
    public static void BuildAndroid(BuildOptions options= BuildOptions.None)
    {
        var scenes = new List<string>() {"Assets/Res/Map/Prepare.unity"}; //BuildScene();

        
      //  ClearDefaultFontRes();
        GameApp.Instance.EditorInitVersion();//先刷新一下version，联合打包起码同步一下别的打包机的版本号。
        //PlayerSettings.productName = "七王书";
        PlayerSettings.companyName = "Yoka";
        GameApp.Instance.PlayerSettingVersion();//然后再设置
        /*PlayerSettings.bundleVersion = GameApp.Instance.Version;*/
        /*PlayerSettings.Android.bundleVersionCode = GameApp.Instance.BundleCode;//覆盖包体作用，等于代码自增*/
/*        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; 
        PlayerSettings.Android.targetSdkVersion = 33; */
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = Application.dataPath + "/../stargame.keystore";
        PlayerSettings.Android.keystorePass = "ykgame123";
        PlayerSettings.Android.keyaliasName = "stargame";
        PlayerSettings.Android.keyaliasPass = "ykgame123";
        
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 1);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 1);
        //PlayerSettings(BuildTargetGroup.Android, "PUBLIC");
        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;


        string FileDir =CommadLines["PackDir"] + "/" + CommadLines["datatime"];

        var GMValue = CommadLines["GM"];
        var STAR_DEVValue = CommadLines["STAR_DEV"];
        var DATATimeValue = CommadLines["datatime"];
        var BUILDPATHDIR = CommadLines["PackDir"];
        if (!Directory.Exists(FileDir))
        {
            Directory.CreateDirectory(FileDir);
        }
        string Filepath = FileDir + ".apk";
        Log("===开始打包===");
        Log("打包路径" + Filepath);
        Log("===打包中===");
        // 打包之前 先 执行一些 文件的生成
        RunBeforBuildCommand();
        //打包
        ClearHideFlags();

        BuildPlayerOptions bpo = new BuildPlayerOptions()
        {
            scenes = scenes.ToArray(),
            locationPathName = Filepath,
            target = BuildTarget.Android,
        };
        bpo.options = options | BuildOptions.CleanBuildCache;
        Log("scenes == " + bpo.scenes[0] + " locationPathName == "+ bpo.locationPathName+ " assetBundleManifestPath==" + bpo.assetBundleManifestPath+ " targetGroup==" + bpo.targetGroup + " target==" + bpo.target + "subtarget" + bpo.subtarget + " options== " + bpo.options + " extraScriptingDefines== " + bpo.extraScriptingDefines);
        var builder = BuildPipeline.BuildPlayer(bpo);
        Log("===打包完成===");
       

        StringBuilder _stringBuilder = new StringBuilder();
        foreach (var item in builder.steps)
        {
            _stringBuilder.Append("stepName" + item.name + "\n");
            foreach (var message in item.messages)
            {
                _stringBuilder.Append("stepNameMessage" + message.type + "==" + message.content + "\n");

            }
        }

        _stringBuilder.Append("summary.result" + builder.summary.result.ToString() + "\n");
        _stringBuilder.Append("summary.totalErrors" + builder.summary.totalErrors + "\n");
        _stringBuilder.Append("summary.totalWarnings" + builder.summary.totalWarnings + "\n");
        foreach (var item in builder.GetFiles())
        {
            _stringBuilder.Append(item.path + "\n");
        }
        Log(_stringBuilder.ToString());
        // BuildProcess.RunBat("E:/Tools/hfs.exe", string.Format($"/command:upload {Filepath}"));

        try
        {

            if (builder.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Log("打包成功");

                if (IsSendXiaoShan)
                {
                    string uploadpath = "http://10.191.72.172:280/jenkinspack/";
                    if (!CommadLines.ContainsKey("HFS"))
                    {
                        uploadpath = CommadLines["HFS"];
                    }
                    string filesvnPath = uploadpath + CommadLines["datatime"] + ".apk";
                    string uploadPath = Filepath.Replace('/', '\\');
                    Log("上传HFS" + uploadPath);

                    XiaoShanHelper.SendLink("安卓Dev", "Dev打包成功", "打包", filesvnPath);
                    Log("发消息到小闪" + filesvnPath);
                }
                else
                {
                    Log("未发送小闪");
                }


            }
            else
            {
                Log("打包失败");
                if (IsSendXiaoShan)
                {
                   XiaoShanHelper.SendTextMsg("打包失败", "打包");
                }
            }
            HandleXLuaWrapFile(false);//还原luagen文件
                                      //上传符号表
#if !GM && !STAR_DEV  
   //         UpLoadBuglySo.EditAndRunCommand(DATATimeValue, BUILDPATHDIR);
#endif
        }
        catch (Exception e)
        {
            Log("打包失败" + e.Message);
            Debug.LogException(e);
        }
        Save();
        IsSendXiaoShan = false;
    }

    //ApkProfiler

    private static void ClearDefaultFontRes()
    {
        
    }
    
    static void testMachine(string filepaths,string filename)
    {
      //  filepaths = "F:/Proj/StarsProject_Client/GamePack/AndroidPack/TrunkPack/202410161931.apk";
        //string hashfile = "";
        //byte[] fileBytes = File.ReadAllBytes(filepaths);
        //using (MD5 md5 = MD5.Create())
        //{
        //    byte[] hashBytes = md5.ComputeHash(fileBytes);
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < hashBytes.Length; i++)
        //    {
        //        sb.Append(hashBytes[i].ToString("X2"));
        //    }
        //    hashfile = sb.ToString();
        //}
        ////上传到云存储
        //FileInfo info = new FileInfo(filepaths);
        //float length = info.Length;

        //var commandlines = $"../uploader.exe {filepaths} {hashfile}{filename}"; //$"-X PUT  http://package.test.com/cloud_file/{hashfile} -H 'Content-Type: application/octet-stream' --data-binary {filepaths}";
        //MsgProcessHelper.InvokeProcessStartAsync("curl", commandlines);
        //commandlines = "curl -X POST  http://package.test.com/api/pm/package/ -H \"Content-Type: application/json\" -d ";
        //var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"version_name\":\"2.2\",\"app_id\":10200210,\"package_array\":[{\"download_url\":\"15ed53aa4db674cc105fcfd293d1307f202410161931.apk\",\"file_md5\":\"15ed53aa4db674cc105fcfd293d1307f\",\"file_size\":1653033661,\"icon\":\"\",\"bundle_name\":\"七王书\",\"bundle_version\":\"4.2.6\",\"bundle_identifier\":\"com.Yoka.Stars\",\"sdk_version\":\"30\",\"original_file_name\":\"202410161931.apk\",\"platform\":\"安卓端\",\"channel\":\"游卡\",\"sub_channel\":\"游卡-安卓\",\"sub_channel_id\":310003,\"environment\":\"develop\",\"repository_version\":\"\",\"msg\":\"\"}]}"); //MiniJSON.Json .Deserialize(jsonString) as Dictionary<string, object>;
        //Debug.LogError("type == " + dict["package_array"].GetType());
        //Newtonsoft.Json.Linq.JArray arrary = dict["package_array"] as Newtonsoft.Json.Linq.JArray;
        // Debug.LogError("dict == " + dict);
        //arrary["download_url"] = $"{hashfile}{filename}";
        //arrary["file_md5"] = hashfile;
        //arrary["file_size"] = length;
        //arrary["original_file_name"] = filename;
        //dict["package_array"] = Newtonsoft.Json.JsonConvert.SerializeObject(arrary);
        BuildMenu.MsgPackGenerateCode();
        //  XiaoShanHelper.SendTextMsg("打包成功:包svn路径" + "http://10.191.72.172:280/jenkinspack/202410161244.apk", "打包");
    }
    //[MenuItem("Build/testmachine")]
    //public static void CurlApk()
    //{
    //    // testMachine("F:/Proj/StarsProject_Client/GamePack/AndroidPack/TrunkPack/202410161931.apk", "202410161931.apk");
    //    BuildRes.SetDownLoadedAssets(includeinbuild, SceneParent);
    //}

   

    //[MenuItem("Build/BuildWindows")]
    public static void BuildWindows(BuildOptions options= BuildOptions.None)
    {

        //PlayerSettings.productName = "七王书";
        PlayerSettings.companyName = "Yoka";
        var scenes = BuildScene();
        string FileDir = CommadLines["PackDir"] + "/" + CommadLines["datatime"];
        if (!Directory.Exists(FileDir))
        {
            Directory.CreateDirectory(FileDir);
        }

        string Filepath = FileDir + "/Star.exe";
        Log("===开始打包===");
        Log("打包路径" + Filepath);
        Log("===打包中===");
        // 打包之前 先 执行一些 文件的生成
        RunBeforBuildCommand();

        var bpo = new BuildPlayerOptions()
        {
            scenes = scenes.ToArray(),
            locationPathName = Filepath,
            target = BuildTarget.StandaloneWindows64,
        };
        bpo.options = options;
        //打包
        var builder = BuildPipeline.BuildPlayer(bpo);
        

        Log("===打包完成===");

        //BuildProcess.RunBat("E:/Tools/hfs.exe", string.Format($"/command:upload {Filepath}") );
        Save();
        if (builder.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            if (IsSendXiaoShan)
            {
                string filesvnPath = "https://10.191.80.176/svn/Stars_Project/StarsProject_Client/GamePack/WinPack" + CommadLines["datatime"] + "/Star.exe";

                /*RunBat("C:/Program Files/7-Zip/7zG.exe", string.Format($"a {FileDir}"));*/

                XiaoShanHelper.SendTextMsg("打包成功:包svn路径" + filesvnPath, "打包");
            }
        }
        else
        {
            if (IsSendXiaoShan)
            {
                XiaoShanHelper.SendTextMsg("打包失败", "打包");
            }
            Log("打包失败");
        }
        IsSendXiaoShan = false;
    }

    public static void RunBat(string program, string parm)
    {
        try
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = program;
            process.StartInfo.Arguments = string.Format(parm);//this is argument
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.Log("Info:" + e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.LogError("Error:" + e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            process.Close();
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
        }
    }

    /// <summary>
    /// 执行一些 build 之前 需要执行的指令
    /// </summary>
    public static void RunBeforBuildCommand()
    {
        Log("开始 生成 fxDetails.json : ");
        // 生成 一份 所有特效 参数 配置细节 的json
        SkillEditor.FxGenera.GeneraFxDetails();
        Log(" 生成 fxDetails.json 成功");
        Log(" ===================================== ");
        //Log(" 开始 生成 SkillJsonList : ");
        //// 生成 技能配置 依赖的 jsonList, 它同时也 会依赖 FxDetails
        //Json2List.GenerateSkillJsonList();
        //Log(" 生成 SkillJsonList  成功 ");
        Log(" 开始 生成 AssetsToDefine : ");
        AssetsToDefine.GenerateAssetsToDefine();
        Log(" 生成 AssetsToDefine  成功 ");
        Log(" 开始 生成 UIQueueToDefine : ");
        UIQueueToDefine.GenerateUIQueueToDefine();
        Log(" 生成 UIQueueToDefine  成功 ");
        Log(" ===================================== ");

    }
}


/*public class BuildProcessReport:IPostprocessBuildWithReport,IPreprocessBuildWithReport
{
    public int callbackOrder
    {
        get { return 0; }
    }
    public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
       //Buidl前
       Debug.Log("打包开始");
       BuildPack.WriteStreamAssest = true;
       if (BuildPack.WriteStreamAssest)
       {
           Debug.Log("拷贝开始");
           string destdir = $"{Application.streamingAssetsPath}/aa/{EditorUserBuildSettings.activeBuildTarget}";
           if (!Directory.Exists(destdir))
           {
               Directory.CreateDirectory(destdir);
           }
          var  files = Directory.GetFiles(GameApp.Instance.BuildDir);
          foreach (var file in files)
          {
              if (file.EndsWith(".bundle"))
              {
                  string fileName = Path.GetFileName(file);
                  string destFileName = $"{destdir}/{fileName}"; //Application.streamingAssetsPath + "/aa/" + fileName;
                  Debug.Log($"拷贝文件 sourceFileName={file} destFileName={destFileName}");
                  File.Copy(file,destFileName);
              }
          }
          Debug.Log("拷贝结束");
       }
    }

    public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
        //Build 后
        Debug.Log("打包结束");
        
        
    }
}*/