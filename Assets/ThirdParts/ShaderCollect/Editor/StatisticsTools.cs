using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameDLL;
using System.IO;
using System.Text.RegularExpressions;

//using GameEditor.Build;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using Unity.EditorCoroutines.Editor;
using GameExtensions;

namespace GameEditor.Tools
{
    public interface IPrepareGameObjectAsset
    {
        bool PrepareGameObjectAsset(GameObject root);
    }

    /// <summary>
    /// 统计工具的菜单
    /// </summary>
    public class StatisticsTools
    {
        const string STATISTICS_LOGGING_VERBOSE_MODE_KEY = "ResourceStatistics_LoggingVerboseMode";
        const string STATISTICS_LOGGING_FILE_DIR = "ResourceStatistics_LoggingFileDir";
        const string STATISTICS_REPORT_FILE_DIR = "ResourceStatistics_ReportFileDir";
        const string STATISTICS_SOURCE_FILE_DIR = "ResourceStatistics_SourceFileDir";
        const string STATISTICS_PACKAGE_DICTIONARY = "ResourceStatistics_PackageDictionary";

        public static bool LogginVerboseMode
        {
            get
            {
                return GameDLL.Resource.Statistics.StatisticsFacade.LogVerboseMode;
            }

            set
            {
                if (GameDLL.Resource.Statistics.StatisticsFacade.LogVerboseMode != value)
                {
                    GameDLL.Resource.Statistics.StatisticsFacade.LogVerboseMode = value;
                    EditorPrefs.SetBool(STATISTICS_LOGGING_VERBOSE_MODE_KEY, value);
                }
            }
        }


        public static IPrepareGameObjectAsset CustomPrepareGameObjectAsset { get; set; }

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            LogginVerboseMode = EditorPrefs.GetBool(STATISTICS_LOGGING_VERBOSE_MODE_KEY, false);
        }

        [MenuItem("Tools/StatiticsTools/Switch Verbose Mode/Enabled", false, 11)]
        static void EnableVerboseLogging()
        {
            LogginVerboseMode = true;
        }

        [MenuItem("Tools/StatiticsTools/Switch Verbose Mode/Enabled", true)]
        static bool CheckVerboseLoggingEnabled()
        {
            Menu.SetChecked("Tools/StatiticsTools/Switch Verbose Mode/Enabled", LogginVerboseMode);
            return !LogginVerboseMode;
        }

        [MenuItem("Tools/StatiticsTools/Switch Verbose Mode/Disabled", false, 12)]
        static void DisableVerboseLogging()
        {
            LogginVerboseMode = false;
        }

        [MenuItem("Tools/StatiticsTools/Switch Verbose Mode/Disabled", true)]
        static bool CheckVerboseLoggingDisabled()
        {
            Menu.SetChecked("Tools/StatiticsTools/Switch Verbose Mode/Disabled", !LogginVerboseMode);
            return LogginVerboseMode;
        }


        [MenuItem("Tools/StatiticsTools/DumpResourceUsage", false, 13)]
        static void DumpResourceUsage()
        {
            string defaultLogDir = Application.dataPath;
            string parentDir = System.IO.Path.GetDirectoryName(defaultLogDir);
            defaultLogDir = System.IO.Path.Combine(parentDir, "Log");
            if (!System.IO.Directory.Exists(defaultLogDir))
            {
                System.IO.Directory.CreateDirectory(defaultLogDir);
            }
            string cachedLogDir = EditorPrefs.GetString(STATISTICS_LOGGING_FILE_DIR, string.Empty);
            string logDir;
            if (!string.IsNullOrEmpty(cachedLogDir) && System.IO.Directory.Exists(cachedLogDir))
            {
                logDir = cachedLogDir;
            }
            else
            {
                logDir = defaultLogDir;
            }
            string defaultFileName = string.Format("runtime_resource_dump_{0}", DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss_ffff"));
            string saveFilePath = EditorUtility.SaveFilePanel("Dump usage", logDir, defaultFileName, "log");
            if (string.IsNullOrEmpty(saveFilePath))
            {
                GLog.Log("user canceled");
                return;
            }
            cachedLogDir = System.IO.Path.GetDirectoryName(saveFilePath);
            EditorPrefs.SetString(STATISTICS_LOGGING_FILE_DIR, cachedLogDir);
            using (System.IO.FileStream fs = new System.IO.FileStream(saveFilePath, System.IO.FileMode.OpenOrCreate))
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fs))
                {
                    //ResourceManager.Instance.StatisticsFacade.DumpUsage(writer);

                    writer.Close();
                }
                fs.Close();
            }
        }

        [MenuItem("Tools/StatiticsTools/DumpResourceUsage", true)]
        static bool CheckResourceDumpping()
        {
            return Application.isPlaying;
        }



        static List<Action<bool>> s_savingPrefabEvents = new List<Action<bool>>();

        public static void RegisterSavingPrefabEvent(Action<bool> callback)
        {
            s_savingPrefabEvents.Add(callback);
        }

        public static void OnSavingPrefab(bool enter)
        {
            foreach (var callback in s_savingPrefabEvents)
            {
                callback.Invoke(enter);
            }
        }



        delegate void VisitAsset(string assetPath, UnityEngine.Object o, StreamWriter sw);

        delegate void VisitProgress(string assetPath, int current, int total);

        static void ReportError(StreamWriter sw, string assetPath, string aspect, string detail)
        {
            string line = string.Format("Error aspect={0}\tassetPath={1}\tdetail={2}", aspect, assetPath, detail);
            sw.WriteLine(line);
        }

        public static void ProgressBar(string title, string assetPath, int current, int total)
        {
            EditorUtility.DisplayProgressBar(title, string.Format("{1}/{2}\t{3}", title, current, total, assetPath), (float)current / total);
        }

        public static string ConvertSystemPathToAssetPath(string systemPath)
        {
            string projectPath = Application.dataPath;
            if (systemPath.StartsWith(projectPath))
            {
                string temp = systemPath.Substring(projectPath.Length);
                temp = "Assets" + temp;
                temp = temp.Replace('\\', '/');
                return temp;
            }
            else
            {
                return null;
            }
        }

        public static string ConvertSystemPathToPackagePath(string systemPath, string packagePath)
        {
            string absPath = Path.GetFullPath(packagePath);
            if (systemPath.StartsWith(absPath))
            {
                string temp = systemPath.Substring(absPath.Length);
                temp = packagePath + temp;
                temp = temp.Replace('\\', '/');
                return temp;
            }
            else if (systemPath.StartsWith(packagePath))
            {
                string temp = systemPath.Replace('\\', '/');
                return temp;
            }
            else
            {
                return null;
            }
        }

        static void CheckDetfaultMaterail(string assetPath, GameObject go, StreamWriter sw)
        {
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (r.sharedMaterials != null)
                {
                    foreach (var m in r.sharedMaterials)
                    {
                        if (m == null)
                            continue;
                        var materialPath = AssetDatabase.GetAssetPath(m);
                        var assetId = AssetDatabase.AssetPathToGUID(materialPath);
                        string reason = string.Empty;
                        string matchingRegex = @"^Assets/(.+).mat$";
                        if (!Regex.IsMatch(materialPath, matchingRegex, RegexOptions.IgnoreCase))
                            reason = "Default Material";
                        else if (m.shader.name == "Default")
                            reason = "Default Shader";
                        if (!string.IsNullOrEmpty(reason))
                        {
                            try
                            {
                                ReportError(sw, assetPath, reason, string.Format("component={0},material={1},materialPath={2},materialId={3}", r.name, m.name, materialPath, assetId));
                            }
                            catch (Exception e)
                            {
                                GLog.ErrorFormat("CheckDetfaultMaterail exception asset={0}, message={1}", assetPath, e.Message);
                            }
                        }
                    }
                }
            }
        }

        static void TraveresBuildingAssets(VisitAsset visitor, StreamWriter sw, VisitProgress scanningProgressCallback, VisitProgress checkingProgressCallback)
        {
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            HashSet<string> assetsWithAssetbundleMark = new HashSet<string>();

            int count = allAssets.Length;
            int current = 0;
            foreach (var assetPath in allAssets)
            {
                ++current;
                if (scanningProgressCallback != null)
                {
                    scanningProgressCallback(assetPath, current, count);
                }
                var importer = AssetImporter.GetAtPath(assetPath);
                if (!string.IsNullOrEmpty(importer.assetBundleName))
                {
                    assetsWithAssetbundleMark.Add(assetPath);
                }
            }
            string[] buildingAssets = AssetDatabase.GetDependencies(assetsWithAssetbundleMark.ToArray());
            count = buildingAssets.Length;
            current = 0;
            foreach (var buildingAsset in buildingAssets)
            {
                ++current;
                if (checkingProgressCallback != null)
                {
                    checkingProgressCallback(buildingAsset, current, count);
                }
                UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(buildingAsset);
                visitor(buildingAsset, o, sw);
            }
        }

        [MenuItem("Tools/StatiticsTools/CheckErrorAsset/CheckBuildingAssets", false, 16)]
        static void CheckBuildingAssets()
        {
            EditorUtility.ClearProgressBar();

            string dateString = DateTime.Now.ToString("yyyy-MM-dd-hh-mm");
            string saveFilePath = EditorUtility.SaveFilePanel("Save report", string.Empty, string.Format("buildingAssetReport-{0}", dateString), "txt");
            if (string.IsNullOrEmpty(saveFilePath))
                return;
            using (FileStream fs = new FileStream(saveFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    TraveresBuildingAssets((assetPath, obj, streamWriter) =>
                    {
                        if (obj is GameObject)
                        {
                            CheckDetfaultMaterail(assetPath, obj as GameObject, streamWriter);
                        }
                    }, sw, (assetPath, current, total) => {
                        ProgressBar("Scanning", assetPath, current, total);
                    }, (assetPath, current, total) => {
                        ProgressBar("Checking", assetPath, current, total);
                    });
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Finish", "CheckBuildingAssets process finished", "Ok");
        }

        private static string ParseArgsToLogDir()
        {
            Debug.Log("ParseArgsToLogDir triggered");
            string[] args = System.Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                string[] values = arg.Split('=');
                if (values.Length == 2)
                {
                    string envKey = values[0];
                    string envValue = values[1];
                    if(envKey.ToLower()=="reportpath")
                    {
                        Debug.Log($"ParseArgsToLogDir found reportPath={envValue}");
                        return envValue;
                    }
                }
            }
            return string.Empty;
        }


        public static string OpenSaveFileDialog(string title)
        {
            Debug.Log($"OpenSaveFileDialog enter, title={title}");
            string defaultLogDir = Application.dataPath;
            string parentDir = System.IO.Path.GetDirectoryName(defaultLogDir);
            defaultLogDir = System.IO.Path.Combine(parentDir, "Log");
            if (!System.IO.Directory.Exists(defaultLogDir))
            {
                System.IO.Directory.CreateDirectory(defaultLogDir);
            }
            string cachedLogDir = EditorPrefs.GetString(STATISTICS_REPORT_FILE_DIR, string.Empty);
            string logDir;
            if (!string.IsNullOrEmpty(cachedLogDir) && System.IO.Directory.Exists(cachedLogDir))
            {
                logDir = cachedLogDir;
            }
            else
            {
                logDir = defaultLogDir;
            }
            string defaultFileName = string.Format("{0}-{1}", title, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));
            if(Application.isBatchMode)
            {
                var pathFromEnv = ParseArgsToLogDir();
                if(!string.IsNullOrEmpty(pathFromEnv))
                {
                    Debug.Log($"pathFromEnv={pathFromEnv}");
                    return pathFromEnv+".txt";
                }
                return string.Format($"{logDir}/{defaultFileName}.txt");
            }
            string saveFilePath = EditorUtility.SaveFilePanel(title, logDir, defaultFileName, "txt");
            if (string.IsNullOrEmpty(saveFilePath))
            {
                GLog.Log("user canceled");
                return null;
            }
            cachedLogDir = System.IO.Path.GetDirectoryName(saveFilePath);
            EditorPrefs.SetString(STATISTICS_REPORT_FILE_DIR, cachedLogDir);
            return saveFilePath;
        }

        public static string OpenSaveFolderDialog(string title)
        {
            string defaultLogDir = Application.dataPath;
            string parentDir = System.IO.Path.GetDirectoryName(defaultLogDir);
            defaultLogDir = System.IO.Path.Combine(parentDir, "Log");
            if (!System.IO.Directory.Exists(defaultLogDir))
            {
                System.IO.Directory.CreateDirectory(defaultLogDir);
            }
            string cachedLogDir = EditorPrefs.GetString(STATISTICS_REPORT_FILE_DIR, string.Empty);
            string logDir;
            if (!string.IsNullOrEmpty(cachedLogDir) && System.IO.Directory.Exists(cachedLogDir))
            {
                logDir = cachedLogDir;
            }
            else
            {
                logDir = defaultLogDir;
            }
            string defaultFileName = string.Format("{0}-{1}", title, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));
            string saveFolderPath = EditorUtility.SaveFolderPanel(title, logDir, string.Empty);
            if (string.IsNullOrEmpty(saveFolderPath))
            {
                GLog.Log("user canceled");
                return null;
            }
            cachedLogDir = saveFolderPath;
            EditorPrefs.SetString(STATISTICS_REPORT_FILE_DIR, cachedLogDir);
            string destFolder = Path.Combine(saveFolderPath, defaultFileName);
            GLog.LogFormat("ReportABRelation to {0}", destFolder);
            if (Directory.Exists(destFolder))
            {
                Directory.Delete(destFolder, true);
            }
            Directory.CreateDirectory(destFolder);
            return destFolder;
        }

        public static string OpenReadSourceDialog(string title)
        {
            string defaultLogDir = Application.dataPath;
            string parentDir = System.IO.Path.GetDirectoryName(defaultLogDir);
            string cachedSrcDir = EditorPrefs.GetString(STATISTICS_SOURCE_FILE_DIR, string.Empty);
            string srcDir = null;
            if (!string.IsNullOrEmpty(cachedSrcDir) && System.IO.Directory.Exists(cachedSrcDir))
            {
                srcDir = cachedSrcDir;
            }
            else
            {
                srcDir = parentDir;
            }
            string srcPath = EditorUtility.OpenFilePanel(title, srcDir, string.Empty);
            if (string.IsNullOrEmpty(srcPath))
                return null;

            cachedSrcDir = System.IO.Path.GetDirectoryName(srcPath);
            EditorPrefs.SetString(STATISTICS_SOURCE_FILE_DIR, cachedSrcDir);
            return srcPath;
        }

        public delegate void ReportTxtSavedCallback(string txtFilePath);

        private static ReportTxtSavedCallback s_reportTxtSavedCallback = null;

        public static void SetReportTxtSavedCallback(ReportTxtSavedCallback callback)
        {
            s_reportTxtSavedCallback = callback;
        }

        public static void ClearReportTxtSavedCallback()
        {
            s_reportTxtSavedCallback = null;
        }

        public static void OnReportTxtSaved(string txtFilePath)
        {
            if (s_reportTxtSavedCallback != null)
            {
                s_reportTxtSavedCallback(txtFilePath);
            }
        }


        /*
        [MenuItem("Tools/StatiticsTools/CheckUnbundledBuildingAssets", false, 17)]
        static void CheckUnbundledBuildingAssets()
        {
            EditorUtility.ClearProgressBar();

            string dateString = DateTime.Now.ToString("yyyy-MM-dd-hh-mm");
            string saveFilePath = EditorUtility.SaveFilePanel("Save report", string.Empty, string.Format("unbundledBuildingAssetReport-{0}", dateString), "txt");
            if (string.IsNullOrEmpty(saveFilePath))
                return;
            using (FileStream fs = new FileStream(saveFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    Dictionary<string, string> assetToBundleTable = new Dictionary<string, string>();
                    Dictionary<string, HashSet<string>> assetToDependedBy = new Dictionary<string, HashSet<string>>();
                    string[] allAssets = AssetDatabase.GetAllAssetPaths();
                    int count = allAssets.Length;
                    int current = 0;
                    //寻找出所有的依赖关系
                    foreach(var assetPath in allAssets)
                    {
                        ProgressBar("Scanning", assetPath, current, count);

                        var importer = AssetImporter.GetAtPath(assetPath);
                        string bundleName = importer.assetBundleName;
                        if (!string.IsNullOrEmpty(bundleName))
                        {
                            assetToBundleTable.Add(assetPath, bundleName);
                            string[] depends = AssetDatabase.GetDependencies(assetPath);
                            foreach(var depend in depends)
                            {
                                switch(Path.GetExtension(depend))
                                {
                                    case ".cs":
                                        continue;
                                }
                                HashSet<string> dependedBySet = null;
                                if(!assetToDependedBy.TryGetValue(depend, out dependedBySet))
                                {
                                    dependedBySet = new HashSet<string>();
                                    assetToDependedBy.Add(depend, dependedBySet);
                                }
                                dependedBySet.Add(assetPath);
                            }
                        }
                        current++;
                    }
                    count = assetToDependedBy.Count;
                    current = 0;
                    //检测每个被依赖的资源，是否有AB设置，如果没有则检测是否被多个包依赖
                    foreach (var dependKV in assetToDependedBy)
                    {
                        string depend = dependKV.Key;
                        ProgressBar("Checking", depend, current, count);
                        HashSet<string> dependedBy = dependKV.Value;
                        if(assetToBundleTable.ContainsKey(depend))
                        {
                            //说明被依赖资源有分配AB，是合法的
                        }
                        else
                        {
                            //没有分配AB则是看运气的，分属两个AB就是非法
                            HashSet<string> dependedByAB = new HashSet<string>();
                            List<string> cachedLine = new List<string>();
                            foreach (var by in dependedBy)
                            {
                                string byAB = null;
                                if(assetToBundleTable.TryGetValue(by, out byAB))
                                {
                                    dependedByAB.Add(byAB);
                                }
                                else
                                {
                                    GLog.ErrorFormat("byAsset not found ab: {0}, while checking {1}", by, depend);
                                }
                                cachedLine.Add(string.Format("{0}==>({1})", by, byAB));
                            }
                            bool error = dependedByAB.Count>1;
                            if(error)
                            {
                                sw.WriteLine(string.Format("error asset: {0}", depend));
                                foreach(var cached in cachedLine)
                                {
                                    sw.WriteLine(cached);
                                }
                            }
                            else
                            {
                                sw.WriteLine(string.Format("risky asset: {0}", depend));
                            }
                            sw.WriteLine(string.Empty);
                        }

                        current++;
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Finish", "CheckUnbundledBuildingAssets process finished", "Ok");
        }
        */

        public static void CheckWastedMaterailTexture(ShaderAnalyseToolsConfig config, string assetPath, Material material, bool deepClearProperties, StreamWriter sw)
        {
            Shader shader = material.shader;
            bool changed = false;
            if (shader == null)
            {
                string log = string.Format("Material error, no shader assigned: {0}", assetPath);
                if (sw != null)
                {
                    sw.WriteLine(log);
                }
                else
                {
                    GLog.Log(log);
                }
                return;
            }

            HashSet<string> textureNamesInShader = new HashSet<string>();
            for (int index = 0; index < shader.GetPropertyCount(); ++index)
            {
                int propId = shader.GetPropertyNameId(index);
                switch (shader.GetPropertyType(index))
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        string texName = shader.GetPropertyName(index);
                        //Debug.LogFormat("property {0}, name {1}", propId, texName);
                        textureNamesInShader.Add(texName);
                        break;
                }
            }
            string[] textureNames = material.GetTexturePropertyNames();
            foreach (var textureName in textureNames)
            {
                Texture texture = material.GetTexture(textureName);
                if (texture)
                {
                    if (!textureNamesInShader.Contains(textureName))
                    {
                        string wasted = AssetDatabase.GetAssetPath(texture);
                        string log = string.Format("Texture wasted: {0} of material {1}, name {2}. It is automatically removed", wasted, assetPath, textureName);
                        if (sw != null)
                        {
                            sw.WriteLine(log);
                        }
                        else
                        {
                            GLog.Log(log);
                        }
                        material.SetTexture(textureName, null);
                        changed = true;
                    }
                }
            }

            string[] keywords = ShaderAnalyseTools.GetShaderKeywords(shader, ShaderAnalyseTools.KeywordType.Both);
            List<string> vaildKeywordList = new List<string>();
            HashSet<string> keywordsByShader = new HashSet<string>(keywords);
            bool combinationCare = false;
            int[] fixedCombinationFoundIndex = null;
            int[] flexibleCombinationFoundIndex = null;
            foreach (var caredCombinationName in config.combinationPattern.shaderNames)
            {
                if (caredCombinationName.Equals(shader.name))
                {
                    combinationCare = true;
                    fixedCombinationFoundIndex = new int[config.combinationPattern.fixedCombination.Length];
                    for (int i = 0; i < fixedCombinationFoundIndex.Length; ++i)
                    {
                        fixedCombinationFoundIndex[i] = -1;
                    }
                    flexibleCombinationFoundIndex = new int[config.combinationPattern.flexibleCombination.Length];
                    for (int i = 0; i < flexibleCombinationFoundIndex.Length; ++i)
                    {
                        flexibleCombinationFoundIndex[i] = -1;
                    }
                    break;
                }
            }
            foreach (var k in material.shaderKeywords)
            {
                if (keywordsByShader.Contains(k))
                {
                    bool shouldWipeOff = false;
                    foreach (var pattern in config.keywordWipeOffPattern)
                    {
                        Regex regex = new Regex(pattern);
                        if (regex.IsMatch(k))
                        {
                            shouldWipeOff = true;
                            break;
                        }
                    }
                    if (!shouldWipeOff)
                    {
                        vaildKeywordList.Add(k);
                    }
                    if (combinationCare)
                    {
                        for (int fixedGroupIndex = 0; fixedGroupIndex < config.combinationPattern.fixedCombination.Length; ++fixedGroupIndex)
                        {
                            for (int keywordIndex = 0; keywordIndex < config.combinationPattern.fixedCombination[fixedGroupIndex].keywords.Length; ++keywordIndex)
                            {
                                if (config.combinationPattern.fixedCombination[fixedGroupIndex].keywords[keywordIndex].Equals(k))
                                {
                                    fixedCombinationFoundIndex[fixedGroupIndex] = keywordIndex; //找到组合号
                                    break;
                                }
                            }
                        }
                        for (int flexibleGroupIndex = 0; flexibleGroupIndex < config.combinationPattern.flexibleCombination.Length; ++flexibleGroupIndex)
                        {
                            for (int keywordIndex = 0; keywordIndex < config.combinationPattern.flexibleCombination[flexibleGroupIndex].keywords.Length; ++keywordIndex)
                            {
                                if (config.combinationPattern.flexibleCombination[flexibleGroupIndex].keywords[keywordIndex].Equals(k))
                                {
                                    flexibleCombinationFoundIndex[flexibleGroupIndex] = keywordIndex; //找到组合号
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    string log = string.Format("keyword {0} in material {1} is no longer supported by shader {2}. It is automatically removed", k, material.name, shader.name);
                    if (sw != null)
                    {
                        sw.WriteLine(log);
                    }
                    else
                    {
                        GLog.Log(log);
                    }
                    changed = true;
                }
            }
            if (combinationCare)
            {
                int priorIndex = -1;
                for (int fixedGroupIndex = 0; fixedGroupIndex < fixedCombinationFoundIndex.Length; ++fixedGroupIndex)
                {
                    if (fixedCombinationFoundIndex[fixedGroupIndex] >= 0)
                    {
                        int currentIndex = fixedCombinationFoundIndex[fixedGroupIndex];   //找到一个已配置的
                        if (priorIndex < 0)
                        {
                            priorIndex = currentIndex;
                        }
                        else if (priorIndex != currentIndex)
                        {
                            string log = string.Format("Combination mismatch: {0}", assetPath);
                            if (sw != null)
                            {
                                sw.WriteLine(log);
                            }
                            else
                            {
                                GLog.Error(log);
                            }
                        }
                    }
                }
                if (priorIndex < 0)    //一个都没找到则选中第0组
                {
                    priorIndex = 0;
                }
                else if (config.combinationPattern.fixedCombinationIndexLimit <= priorIndex)
                {
                    string log = string.Format("Combination priorIndex out of range: {0}, priorIndex={1}", assetPath, priorIndex);
                    if (sw != null)
                    {
                        sw.WriteLine(log);
                    }
                    else
                    {
                        GLog.Error(log);
                    }
                    priorIndex = -1;
                }
                if (priorIndex >= 0)
                {
                    for (int fixedGroupIndex = 0; fixedGroupIndex < fixedCombinationFoundIndex.Length; ++fixedGroupIndex)
                    {
                        if (fixedCombinationFoundIndex[fixedGroupIndex] < 0)
                        {
                            var group = config.combinationPattern.fixedCombination[fixedGroupIndex].keywords;
                            if (group.Length <= priorIndex)
                            {
                                string log = string.Format("Combination out of range: {0}, fixedGroupIndex={1}, priorIndex={2}", assetPath, fixedGroupIndex, priorIndex);
                                if (sw != null)
                                {
                                    sw.WriteLine(log);
                                }
                                else
                                {
                                    GLog.Error(log);
                                }
                            }
                            else
                            {
                                string addFixedCombinationKeyword = group[priorIndex];
                                string log = string.Format("Fixed combination add keyword: {0}, fixedGroupIndex={1}, priorIndex={2}, addFixedCombinationKeyword={3}", assetPath, fixedGroupIndex, priorIndex, addFixedCombinationKeyword);
                                if (sw != null)
                                {
                                    sw.WriteLine(log);
                                }
                                else
                                {
                                    GLog.Log(log);
                                }
                                vaildKeywordList.Add(addFixedCombinationKeyword);
                                changed = true;
                            }
                        }
                    }
                }
                for (int flexibleGroupIndex = 0; flexibleGroupIndex < flexibleCombinationFoundIndex.Length; ++flexibleGroupIndex)
                {
                    if (flexibleCombinationFoundIndex[flexibleGroupIndex] < 0)
                    {
                        var group = config.combinationPattern.flexibleCombination[flexibleGroupIndex].keywords;
                        string addflexibleCombinationKeyword = group[0];    //没有就加第一个
                        string log = string.Format("Fixed combination add keyword: {0}, fixedGroupIndex={1}, addflexibleCombinationKeyword={2}", assetPath, flexibleGroupIndex, addflexibleCombinationKeyword);
                        if (sw != null)
                        {
                            sw.WriteLine(log);
                        }
                        else
                        {
                            GLog.Log(log);
                        }
                        vaildKeywordList.Add(addflexibleCombinationKeyword);
                        changed = true;
                    }
                }
            }
            try
            {
                var array = vaildKeywordList.ToArray();
                if (array == null)
                {
                    array = new string[0] { };
                }
                GLog.LogFormat("material.shaderKeywords {0}", assetPath);
                material.shaderKeywords = array;

                //if(deepClearProperties)
                {
                    SerializedObject so = new SerializedObject(material);
                    SerializedProperty m_SavedProperties = so.FindProperty("m_SavedProperties");
                    RemoveElement(material, "m_TexEnvs", m_SavedProperties, deepClearProperties);
                    RemoveElement(material, "m_Floats", m_SavedProperties, deepClearProperties);
                    RemoveElement(material, "m_Colors", m_SavedProperties, deepClearProperties);
                    so.ApplyModifiedProperties();
                }

            }
            catch (Exception e)
            {
                GLog.ErrorFormat("material.shaderKeywords error: {0}", e.Message);
            }
            if (changed)
            {
                EditorUtility.SetDirty(material);
            }

            /*
            HashSet<string> allUsedTexture = new HashSet<string>();
            foreach (var textureName in textureNamesInShader)
            {
                Texture texture = material.GetTexture(textureName);
                if(!texture)
                {
                    //有些贴图使用是有条件的，这里就不报了
                    //sw.WriteLine(string.Format("Texture is null for name: {0} of material {1}", textureName, assetPath));
                }
                else
                {
                    string texturePath = AssetDatabase.GetAssetPath(texture);
                    allUsedTexture.Add(texturePath);
                }
            }

            string[] dependencies = AssetDatabase.GetDependencies(assetPath);
            HashSet<string> dependedTextures = new HashSet<string>();
            foreach (string depend in dependencies)
            {
                switch (Path.GetExtension(depend))
                {
                    case ".shader":
                    case ".mat":
                        break;
                    default:
                        dependedTextures.Add(depend);
                        break;
                }
            }
            dependedTextures.ExceptWith(allUsedTexture);
            if (dependedTextures.Count > 0)
            {
                foreach (var wasted in dependedTextures)
                {
                    sw.WriteLine(string.Format("Texture wasted: {0} of material {1}", wasted, assetPath));
                }
            }
            */
        }


        private static void RemoveElement(Material mat, string spName, SerializedProperty saveProperty, bool deepClearProperties)
        {
            HashSet<string> shaderProperies = new HashSet<string>();
            for (int i = 0; i < mat.shader.GetPropertyCount(); ++i)
            {
                var p = mat.shader.GetPropertyName(i);
                shaderProperies.Add(p);
            }
            SerializedProperty property = saveProperty.FindPropertyRelative(spName);
            for (int i = property.arraySize - 1; i >= 0; i--)
            {
                var prop = property.GetArrayElementAtIndex(i);
                string propertyName = prop.displayName;
                if (!mat.HasProperty(propertyName))
                {
                    property.DeleteArrayElementAtIndex(i);
                    Debug.Log("移除属性名称" + propertyName);
                }
                else if (deepClearProperties && !shaderProperies.Contains(propertyName))
                {
                    property.DeleteArrayElementAtIndex(i);
                    Debug.Log("深度移除属性名称" + propertyName);
                }
            }
        }

        [MenuItem("Tools/StatiticsTools/CheckErrorAsset/CheckWastedTextureInMaterial", false, 18)]
        static void CheckWastedAssets()
        {
            EditorUtility.ClearProgressBar();

            string dateString = DateTime.Now.ToString("yyyy-MM-dd-hh-mm");
            string saveFilePath = EditorUtility.SaveFilePanel("Save report", string.Empty, string.Format("wastedAssetReport-{0}", dateString), "txt");
            if (string.IsNullOrEmpty(saveFilePath))
                return;
            using (FileStream fs = new FileStream(saveFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    ShaderAnalyseToolsConfig config = ShaderAnalyseTools.GetConfig();

                    TraveresBuildingAssets((assetPath, obj, streamWriter) =>
                    {
                        if (obj is Material)
                        {
                            CheckWastedMaterailTexture(config, assetPath, obj as Material, false, streamWriter);
                        }
                    }, sw, (assetPath, current, total) => {
                        ProgressBar("Scanning", assetPath, current, total);
                    }, (assetPath, current, total) => {
                        ProgressBar("Checking", assetPath, current, total);
                    });
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Finish", "CheckBuildingAssets process finished", "Ok");
        }


        [MenuItem("Tools/StatiticsTools/CheckErrorAsset/CheckSelectedMaterial", false, 19)]
        [MenuItem("Assets/CheckSelectedMaterial")]
        static void CheckSelectedMaterial()
        {
            EditorUtility.ClearProgressBar();
            int index = 0;
            var selection = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            ShaderAnalyseToolsConfig config = ShaderAnalyseTools.GetConfig();
            foreach (UnityEngine.Object o in selection)
            {
                ++index;
                string assetPath = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(assetPath))
                    continue;       //只有工程资源才检查
                if (System.IO.Path.GetExtension(assetPath) != ".mat")
                    continue;       //只有材质球才需要检查
                ProgressBar("Scanning", assetPath, index, selection.Length);
                CheckWastedMaterailTexture(config, assetPath, o as Material, false, null);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Finish", "CheckSelectedMaterial process finished", "Ok");
        }

        public static HashSet<string> s_modifiedFileSet = null;

        public static void BeginMarkModifiedFileList()
        {
            s_modifiedFileSet = new HashSet<string>();
        }

        public static void AppendModifiedFileList(string[] appendLine, string parentDir, string listFilePath)
        {
            List<string> fullPathList = new List<string>();
            foreach(var filePath in appendLine)
            {
                string absFilePath;
                if (string.IsNullOrEmpty(parentDir))
                {
                    absFilePath = filePath;
                }
                else
                {
                    absFilePath = parentDir + "/" + filePath;
                }
                fullPathList.Add(absFilePath);
            }
            File.AppendAllLines(listFilePath, fullPathList);
        }

        public static void MarkModifiedFile(string filePath, bool isMeta)
        {
            if (s_modifiedFileSet != null)
            {
                //if(filePath.Contains(' '))
                //{
                //    GLog.ErrorFormat("MarkModifiedFile failed, filePath contains whitespace, it cannot be commit by svn commandline: {0}", filePath);
                //}
                //else if(isMeta)
                if (isMeta)
                {
                    s_modifiedFileSet.Add(filePath + ".meta");
                }
                else
                {
                    s_modifiedFileSet.Add(filePath);
                }
            }
        }

        static Dictionary<string, string> allPackagePath = null;

        [Serializable]
        public struct DumpPackageItem
        {
            public string packageName;
            public string packagePath;
        };

        [Serializable]
        public class DumpPackageObject
        {
            public DumpPackageItem[] itemarray;
        }

        [InitializeOnLoadMethod]
        static void StartInitializeOnLoadMethod()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //运行时也会调用到这个函数，需要屏蔽
            }
            else
            {
                ShowInstalledPackages();
            }
        }

        [MenuItem("Tools/StatiticsTools/ShowInstalledPackages")]
        static void ShowInstalledPackages()
        {
            allPackagePath = null;
            EditorCoroutineUtility.StartCoroutineOwnerless(QueryAllPackages());
        }

        static IEnumerator QueryAllPackages()
        {
            ListRequest LRequest = Client.List();
            yield return LRequest;
            while (!LRequest.IsCompleted)
                yield return null;
            if (LRequest.Status == StatusCode.Success)
            {
                allPackagePath = new Dictionary<string, string>();
                foreach (var package in LRequest.Result)
                {
                    // Only retrieve packages that are currently installed in the
                    // project (and are neither Built-In nor already Embedded)
                    if (package.isDirectDependency && package.source
                        != PackageSource.BuiltIn && package.source
                        != PackageSource.Embedded)
                    {
                        GLog.LogFormat("Installed package: {0}, path: {1}", package.name, package.assetPath);
                        allPackagePath.Add(package.name, package.assetPath);
                    }
                }

                var tempObject = new DumpPackageObject();
                var temp = new DumpPackageItem[allPackagePath.Count];
                int index = 0;
                foreach (var kv in allPackagePath)
                {
                    temp[index] = new DumpPackageItem();
                    temp[index].packageName = kv.Key;
                    temp[index].packagePath = kv.Value;
                    ++index;
                }
                tempObject.itemarray = temp;
                string dump = JsonUtility.ToJson(tempObject);
                GLog.LogFormat("DumpPackageDictionary: {0}", dump);
                EditorPrefs.SetString(STATISTICS_PACKAGE_DICTIONARY, dump);
            }
            else
                Debug.Log(LRequest.Error.message);
        }

        private static void RecoverDictionaryDump()
        {
            string dump = EditorPrefs.GetString(STATISTICS_PACKAGE_DICTIONARY);
            GLog.LogFormat("RecoverDictionaryDump: {0}", dump);
            DumpPackageObject tempObject = JsonUtility.FromJson<DumpPackageObject>(dump);
            if(allPackagePath==null)
            {
                allPackagePath = new Dictionary<string, string>();
            }
            else
            {
                allPackagePath.Clear();
            }
            foreach (var tempItem in tempObject.itemarray)
            {
                GLog.LogFormat("RecoverDictionaryDump item: {0}: {1}", tempItem.packageName, tempItem.packagePath);
                allPackagePath.Add(tempItem.packageName, tempItem.packagePath);
            }
        }

        public static string[] GetAllPackageName()
        {
            RecoverDictionaryDump();
            if (allPackagePath!=null)
            {
                return allPackagePath.Keys.ToArray();
            }
            return null;
        }

        public static string[] GetAllPackagePath()
        {
            RecoverDictionaryDump();
            if (allPackagePath != null)
            {
                return allPackagePath.Values.ToArray();
            }
            return null;
        }
    }
}
