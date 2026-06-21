using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using GameDLL;
using UnityEditor.SceneManagement;
using Unity.EditorCoroutines.Editor;


namespace GameEditor.Tools.Statistics
{
    public class AutoCollectShaderVariantTool
    {
        [Serializable]
        public class Config
        {
            [Serializable]
            public class QualityConfig
            {
                public int qualityLevel;
                public int globalLODLevel;
                public string[] keywords;
            }

            public string exportPath;
            public QualityConfig[] qualityConfig;
            public string assetbundleName;
            public string assetbundleVariant;
            public string openScene;
            public float cameraDistance;
            public string runner;
            public string exportEmbedMaterialJsonPath;
        }

        //public static string[] GetAllRenderAssets()
        //{
        //    string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        //    foreach (var assetBundleName in assetBundleNames)
        //    {
        //        if (assetBundleName.StartsWith(dependentConfig.autoPrefix))
        //            continue;   //自动收集的不考虑，因为FBX会带入default，而且不会实例化

        //        var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);

        //        foreach (string a in assetPaths)
        //        {
        //            var ext = Path.GetExtension(a);
        //            if (ext == ".mat")
        //            {
        //                UnityEngine.Material mat = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(a);
        //                abAssetList.Add(mat);
        //            }
        //        }
        //    }

        //}
        //打包已经集成这个是editor点的
        [MenuItem("Build/打包已经集成-这个是主动点的-editor/Shader/AutoCollectShaderVariant", false, 302)]
        static void Do()
        {
            //EditorCoroutineUtility.StartCoroutineOwnerless(CoDo2(false, null));
            CoDo(false);
        }

        private static bool s_doExternFinished = false;
        private static Action<string[]> s_finishCallback = null;
        private static bool s_silent = false;
        public static void DoExtern()
        {
            GameDLL.GLog.Log("AutoCollectShaderVariantTool.DoExtern called");
            s_doExternFinished = false;
            CoDo(false);
            //EditorCoroutineUtility.StartCoroutineOwnerless(CoDo2(false, action));
        }

        public static void RegisterFinishCallback(Action<string[]> callback)
        {
            s_finishCallback = callback;
        }

        public static bool IsExternFinished()
        {
            return s_doExternFinished;
        }

        //[MenuItem("Tools/StatiticsTools/Shader/AutoCollectShaderVariantBD", false, 306)]
        //static void DoBD()
        //{
        //    string[] filterExt = new string[] { ".shader", ".mat" };
        //    List<string> allRenderAssets = AutoAssignDependentABTools.GetAllBuildingAssets(filterExt, true, ABAssetPass.All);
        //    ShaderCollection.GenShaderVariant(allRenderAssets);
        //}


        //[MenuItem("Tools/StatiticsTools/Shader/Test", false, 305)]
        static void Test()
        {
            EditorUtility.ClearProgressBar();
            AutoAssignDependentABConfig dependentConfig = null;
            const string dependentJsonFilePath = "Assets/Editor Default Resources/AutoAssignDependentABTools.json";
            using (FileStream fs = new FileStream(dependentJsonFilePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string content = sr.ReadToEnd();
                    dependentConfig = JsonUtility.FromJson<AutoAssignDependentABConfig>(content);
                }
            }
            string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            EditorUtility.ClearProgressBar();
            int assetBundleIndex = 0;
            List<Material> abAssetList = new List<Material>();
            HashSet<string> loadedShaderNameSet = new HashSet<string>();
            EditorUtility.ClearProgressBar();
            HashSet<string> allAssets = new HashSet<string>();
            foreach (var assetBundleName in assetBundleNames)
            {
                Tools.StatisticsTools.ProgressBar("collecting", assetBundleName, assetBundleIndex, assetBundleNames.Length);
                ++assetBundleIndex;
                if (assetBundleName.StartsWith(dependentConfig.autoPrefix))
                    continue;   //自动收集的不考虑，因为FBX会带入default，而且不会实例化

                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);

                foreach (string a in assetPaths)
                {
                    allAssets.Add(a);
                }
            }
            string[] firstPass = allAssets.ToArray();
            Tools.StatisticsTools.ProgressBar("dependent", "please wait", 0, 1);
            string[] secondPass = AssetDatabase.GetDependencies(firstPass);
            foreach (var s in secondPass)
            {
                allAssets.Add(s);
            }
            HashSet<string> collected = new HashSet<string>();
            foreach (var assetPath in allAssets)
            {
                var ext = Path.GetExtension(assetPath);
                if (ext == ".mat")
                {
                    UnityEngine.Material mat = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(assetPath);
                    abAssetList.Add(mat);

                    collected.Add(assetPath);
                }
            }
            string test = @"Assets/Arts/Models/Scenes/HerosPreView_Wild/Materials/Graveyard_plant02.mat";
            if (collected.Contains(test))
            {
                GLog.Log("exist");
            }
            else
            {
                GLog.Log("not exist");
            }
            EditorUtility.ClearProgressBar();
        }

        static Config LoadConfig(bool silent)
        {
            Config s_shaderVariantConfig = null;
            const string jsonFilePath = "Assets/Editor Default Resources/AutoCollectShaderVariantTool.json";
            using (FileStream fs = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string content = sr.ReadToEnd();
                    s_shaderVariantConfig = JsonUtility.FromJson<Config>(content);
                }
            }
            if (s_shaderVariantConfig == null)
            {
                if (!silent)
                {
                    EditorUtility.DisplayDialog("Finish", "config file not exist", "Ok");
                }
                s_doExternFinished = true;
            }
            return s_shaderVariantConfig;
        }

        const string EDITOR_PREF_SILIENT = "EDIOR_PREF_SILIENT";
        const string EDITOR_PREF_COLLECT_VARIANT = "EDITOR_PREF_COLLECT_VARIANT";
        const string EDITOR_PREF_COLLECT_VARIANT_REMAIN = "EDITOR_PREF_COLLECT_VARIANT_REMAIN";
        const string EDITOR_PREF_COLLECT_VARIANT_CURRENT = "EDITOR_PREF_COLLECT_VARIANT_CURRENT";
        const string EditorPrefSeperator = ",";
        const int defaultQualityLevel = -1;
        const string EDITOR_PREF_COLLECT_VARIANT_PRODUCTION = "EDITOR_PREF_COLLECT_VARIANT_PRODUCTION";
        static void CoDo(bool silent)
        {
            EditorPrefs.SetBool(EDITOR_PREF_SILIENT, silent);
            s_silent = silent;
            GameDLL.GLog.Log("AutoCollectShaderVariantTool.CoDo2 called");
            EditorUtility.ClearProgressBar();
            AutoAssignDependentABConfig dependentConfig = null;
            const string dependentJsonFilePath = "Assets/Editor Default Resources/AutoAssignDependentABTools.json";
            using (FileStream fs = new FileStream(dependentJsonFilePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string content = sr.ReadToEnd();
                    dependentConfig = JsonUtility.FromJson<AutoAssignDependentABConfig>(content);
                }
            }
            if (dependentConfig == null)
            {
                if (!silent)
                {
                    EditorUtility.DisplayDialog("Finish", "config file not exist", "Ok");
                }
                s_doExternFinished = true;
                return;
            }

            Config s_shaderVariantConfig = LoadConfig(silent);

            //初始化目录
            if (Directory.Exists(s_shaderVariantConfig.exportPath))
            {
                Directory.Delete(s_shaderVariantConfig.exportPath, true);
            }
            Directory.CreateDirectory(s_shaderVariantConfig.exportPath);

            List<string> remainList = new List<string>();
            foreach(var qualityConfig in s_shaderVariantConfig.qualityConfig)
            {
                remainList.Add(qualityConfig.qualityLevel.ToString());
            }
            string remain = string.Join(EditorPrefSeperator, remainList.ToArray());
            EditorPrefs.SetString(EDITOR_PREF_COLLECT_VARIANT_REMAIN, remain);
            EditorPrefs.SetString(EDITOR_PREF_COLLECT_VARIANT_PRODUCTION, string.Empty);

            CollectShaderVariantBegin();
        }

        static string[] ignoreShaders = null, ignoreShadersBatchs = null;

        static bool IsCanParseShader(string shaderName)
        {
#if STAR_CHECK
            if (ignoreShaders == null)
            {
                var globalSettings = AssetDatabase.LoadAssetAtPath<ShaderCheckerDefine>("Assets/ShaderCheckerDefine.asset");
                if (globalSettings == null)
                {
                    throw new Exception("初始化全局配置失败，请检查再执行！");
                }
                ignoreShaders = globalSettings.IgnoreShaders;
                ignoreShadersBatchs = globalSettings.IgnoreShaderBatchs;
            }
            foreach (var item in ignoreShaders)
            {
                if (shaderName.Equals(item, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            foreach(var item in ignoreShadersBatchs)
            {
                if (shaderName.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
#else
            return true;
#endif
        }

        static void CollectShaderVariantBegin()
        {
            s_silent = EditorPrefs.GetBool(EDITOR_PREF_SILIENT, true);
            Config s_shaderVariantConfig = LoadConfig(s_silent);

            //打开场景
            var tempScene = EditorSceneManager.OpenScene(s_shaderVariantConfig.openScene);
            //打开窗口，不然Shader收集不到
            GameExtensions.GameViewHelper.OpenGameView();

            string remain = EditorPrefs.GetString(EDITOR_PREF_COLLECT_VARIANT_REMAIN, string.Empty);
            var stringArray = remain.Split(EditorPrefSeperator.ToCharArray());
            if(stringArray.Length==0)
            {
                GLog.ErrorFormat("EDITOR_PREF_COLLECT_VARIANT_REMAIN parses empty: {0}", remain);
                return;
            }
            string first = stringArray[0];
            int qualityLevel = defaultQualityLevel; 
            if(!int.TryParse(first, out qualityLevel))
            {
                GLog.ErrorFormat("EDITOR_PREF_COLLECT_VARIANT_REMAIN[0] parses int failed: {0}", first);
                return;
            }
            
            EditorPrefs.SetInt(EDITOR_PREF_COLLECT_VARIANT_CURRENT, qualityLevel);
            if (stringArray.Length == 1)
            {
                EditorPrefs.SetString(EDITOR_PREF_COLLECT_VARIANT_REMAIN, string.Empty);
                LogStage("CollectShaderVariantBegin 2 empty");
            }
            else
            {
                List<string> remainList = new List<string>();
                for (int i = 1; i < stringArray.Length; ++i)
                {
                    remainList.Add(stringArray[i]);
                }
                remain = string.Join(EditorPrefSeperator, remainList.ToArray());
                EditorPrefs.SetString(EDITOR_PREF_COLLECT_VARIANT_REMAIN, remain);
                LogStage("CollectShaderVariantBegin 2 : " + remain);
            }

            //反射对象
            var runtimeAssembly = typeof(ShaderUtil).Assembly;
            Type shaderUtilType = typeof(ShaderUtil);

            //先清空
            object obj1 = shaderUtilType.InvokeMember("ClearCurrentShaderVariantCollection",
            System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, null, null);

            QualitySettings.SetQualityLevel(qualityLevel);
            HashSet<string> allKeywords = new HashSet<string>();
            HashSet<string> thisKeywords = new HashSet<string>();
            foreach (var qualityConfig in s_shaderVariantConfig.qualityConfig)
            {
                foreach(var kw in qualityConfig.keywords)
                {
                    allKeywords.Add(kw);
                }
                if (qualityConfig.qualityLevel == qualityLevel)
                {
                    Shader.globalMaximumLOD = qualityConfig.globalLODLevel;
                    foreach (var kw in qualityConfig.keywords)
                    {
                        thisKeywords.Add(kw);
                    }
                }
            }
            foreach(var kw in allKeywords)
            {
                if(thisKeywords.Contains(kw))
                {
                    Shader.EnableKeyword(kw);
                }
                else
                {
                    Shader.DisableKeyword(kw);
                }
            }

            GameObject collector = new GameObject("collector");
            //ShaderVariantRunner runner = collector.AddComponent<ShaderVariantRunner>();
            var type = System.Type.GetType(s_shaderVariantConfig.runner);
            ShaderVariantRunner runner = collector.AddComponent(type) as ShaderVariantRunner;
            Camera camera = collector.AddComponent<Camera>();
            runner.renderingCamera = camera;
            runner.distance = s_shaderVariantConfig.cameraDistance;
            LogStage("CollectShaderVariantBegin 3 " + qualityLevel);

            string[] filterExt = new string[] { ".mat" };
            runner.targetMaterials = new List<Material>();
            List<string> allRenderAssets = AutoAssignDependentABTools.GetAllBuildingAssets(filterExt, false, ABAssetPass.All);

            EmbedMaterial embedObj = new EmbedMaterial();
            List<string> instancingPathList = new List<string>();
            foreach (var assetPath in allRenderAssets)
            {
                if (string.IsNullOrEmpty(assetPath))
                    continue;
                UnityEngine.Material mat = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(assetPath);
                if (!mat)
                    continue;

                if (mat.shader != null && !IsCanParseShader(mat.shader.name))
                    continue;

                runner.targetMaterials.Add(mat);
                //把有GPU Instancing属性的材质球复制一份出来和Shader一起打包，保证不会被傻X的Unity给Strip掉，by Rick
                if (mat.enableInstancing)
                {
                    instancingPathList.Add(assetPath);
                }
            }
            string embedPath = s_shaderVariantConfig.exportEmbedMaterialJsonPath;
            string embedDir = System.IO.Path.GetDirectoryName(embedPath);
            if (!string.IsNullOrEmpty(embedPath))
            {
                if (!Directory.Exists(embedDir))
                {
                    Directory.CreateDirectory(embedDir);
                }
                embedObj.instancingMaterials = instancingPathList.ToArray();
                string json = JsonUtility.ToJson(embedObj, true);
                File.WriteAllText(embedPath, json);
                //StatisticsTools.MarkModifiedFile(embedPath, false);   //这里权且认为都会改变，svn会过滤掉没改变的提交，这几个文件只会被这段代码改，不用担心和其他修改混在一起提交
                //StatisticsTools.MarkModifiedFile(embedPath, true);
            }
            else
            {
                GameDLL.GLog.Error("embedPath is empty, export failed");
            }

            //runner.callback += OnShaderVariantCollectFinish;
            LogStage("CollectShaderVariantBegin 4 runner.targetMaterials.Count=" + runner.targetMaterials.Count);

            EditorPrefs.SetBool(EDITOR_PREF_COLLECT_VARIANT, true);
            EditorApplication.isPlaying = true;
        }

        static void CollectShaderVariantEnd()
        {
            EditorSceneManager.UnloadSceneAsync(EditorSceneManager.GetActiveScene(), UnityEngine.SceneManagement.UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            LogStage("CollectShaderVariantEnd 1");
            EditorApplication.playModeStateChanged -= LogPlayModeState;
            s_silent = EditorPrefs.GetBool(EDITOR_PREF_SILIENT, true);
            Config s_shaderVariantConfig = LoadConfig(s_silent);

            LogStage("CollectShaderVariantEnd 2");
            Type shaderUtilType = typeof(ShaderUtil);
            int qualityLevel = EditorPrefs.GetInt(EDITOR_PREF_COLLECT_VARIANT_CURRENT, defaultQualityLevel);
            if(qualityLevel == defaultQualityLevel)
            {
                GLog.Error("EDITOR_PREF_COLLECT_VARIANT_CURRENT is empty");
                return;
            }
            string outputPath = string.Format("{0}/shadervariants_{1}.shadervariants", s_shaderVariantConfig.exportPath, qualityLevel);
            object obj2 = shaderUtilType.InvokeMember("SaveCurrentShaderVariantCollection",
            System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, null, new object[] { outputPath });

            AssetDatabase.Refresh();
            ShaderVariantCollection svAsset = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(outputPath);
            if (svAsset == null)
            {
                GLog.ErrorFormat("load CollectShaderVariantEnd fail, {0}", outputPath);
            }
            LogStage("CollectShaderVariantEnd 3");
            AssetImporter ai = AssetImporter.GetAtPath(outputPath);
            ai.assetBundleName = s_shaderVariantConfig.assetbundleName;
            ai.assetBundleVariant = s_shaderVariantConfig.assetbundleVariant;
            EditorUtility.SetDirty(svAsset);
            string production = EditorPrefs.GetString(EDITOR_PREF_COLLECT_VARIANT_PRODUCTION, string.Empty);
            if(string.IsNullOrEmpty(production))
            {
                production = outputPath;
            }
            else
            {
                production = string.Join(EditorPrefSeperator, new string[] { production, outputPath });
            }
            EditorPrefs.SetString(EDITOR_PREF_COLLECT_VARIANT_PRODUCTION, production);
            LogStage("CollectShaderVariantEnd 4 " + production);

            string remain = EditorPrefs.GetString(EDITOR_PREF_COLLECT_VARIANT_REMAIN, string.Empty);
            var stringArray = remain.Split(EditorPrefSeperator.ToCharArray());
            LogStage("CollectShaderVariantEnd 5 " + EDITOR_PREF_COLLECT_VARIANT_REMAIN + " stringArray.Length=" + stringArray.Length);
            if (stringArray.Length == 0)
            {
                LogStage("CollectShaderVariantEnd 6 a");
                OnShaderVariantCollectFinish();
            }
            else
            {
                string first = stringArray[0];
                if(string.IsNullOrEmpty(first))
                {
                    LogStage("CollectShaderVariantEnd 6 b");
                    OnShaderVariantCollectFinish();
                }
                else
                {
                    LogStage("CollectShaderVariantEnd 6");
                    EditorCoroutineUtility.StartCoroutineOwnerless(CoroutineCollectShaderVariantBegin());
                }
            }

            //StatisticsTools.MarkModifiedFile(outputPath, false);   //这里权且认为都会改变，svn会过滤掉没改变的提交，这几个文件只会被这段代码改，不用担心和其他修改混在一起提交
            //StatisticsTools.MarkModifiedFile(outputPath, true);
        }

        static IEnumerator CoroutineCollectShaderVariantBegin()
        {
            yield return new WaitForSecondsRealtime(2f);
            CollectShaderVariantBegin();
        }

        static IEnumerator CoroutineOnShaderVariantCollectFinish()
        {
            LogStage("CoroutineOnShaderVariantCollectFinish 1");
            yield return new WaitForSecondsRealtime(2f);
            LogStage("CoroutineOnShaderVariantCollectFinish 2");
            s_silent = EditorPrefs.GetBool(EDITOR_PREF_SILIENT, true);
            Config s_shaderVariantConfig = LoadConfig(s_silent);
            string embedPath = s_shaderVariantConfig.exportEmbedMaterialJsonPath;
            string procution = EditorPrefs.GetString(EDITOR_PREF_COLLECT_VARIANT_PRODUCTION, string.Empty);
            var outputPath = procution.Split(EditorPrefSeperator.ToCharArray());
            LogStage("CoroutineOnShaderVariantCollectFinish 3 "+ procution);
            if(s_finishCallback==null)
            {
                LogStage("CoroutineOnShaderVariantCollectFinish 4 a");
            }
            else
            {
                LogStage("CoroutineOnShaderVariantCollectFinish 4 b");
            }
            s_finishCallback?.Invoke(outputPath.Append(embedPath).ToArray());
            s_doExternFinished = true;
            AssetDatabase.SaveAssets();

            ShaderKeywordPatcher.ProcShaderKeywordPatch();

            if (!s_silent)
            {
                EditorUtility.DisplayDialog("Finish", "AutoCollectShaderVariant process finished", "Ok");
            }
            LogStage("OnShaderVariantCollectFinish 5");
        }

        [InitializeOnLoadMethod]
        static void RunInit()
        {
            LogStage("RunInit 1");
            bool collecting = EditorPrefs.GetBool(EDITOR_PREF_COLLECT_VARIANT, false);
            if (collecting)
            {
                EditorApplication.playModeStateChanged += LogPlayModeState;
                EditorPrefs.SetBool(EDITOR_PREF_COLLECT_VARIANT, false);
                LogStage("RunInit 2");
            }
        }

        static void LogPlayModeState(PlayModeStateChange mode)
        {
            LogStage("LogPlayModeState 1");
            switch (mode)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    LogStage("LogPlayModeState 2");
                    CollectShaderVariantEnd();
                    break;
            }
        }

        static void OnShaderVariantCollectFinish()
        {
            LogStage("OnShaderVariantCollectFinish 1");
            EditorCoroutineUtility.StartCoroutineOwnerless(CoroutineOnShaderVariantCollectFinish());
            LogStage("OnShaderVariantCollectFinish 2");
        }

        public class EmbedMaterial
        {
            public string[] instancingMaterials;
        }
       
        public static void CallDupliateEmbedMaterials()
        {
            bool silent = true;
            Config s_shaderVariantConfig = LoadConfig(silent);
            var json = File.ReadAllText(s_shaderVariantConfig.exportEmbedMaterialJsonPath);
            var embedObj = JsonUtility.FromJson<EmbedMaterial>(json);
            List<Material> materialList = new List<Material>();
            foreach(var assetPath in embedObj.instancingMaterials)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if(mat)
                {
                    materialList.Add(mat);
                }
                else
                {
                    GLog.ErrorFormat("embeded material does not exist: {0}", assetPath);
                }    
            }
            DuplicateEmbedMaterials(materialList.ToArray(), silent);
        }

        static void DuplicateEmbedMaterials(Material[] sourceMaterialArray, bool silent)
        {
            Config s_shaderVariantConfig = LoadConfig(silent);
            //先清空原来的，防止旧版本残留的
            string[] guids = AssetDatabase.FindAssets("t:Material", new string[] { s_shaderVariantConfig.exportPath });
            HashSet<string> oldPathSet = new HashSet<string>();
            if (guids != null)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    var matPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if(string.IsNullOrEmpty(matPath))
                    {
                        GLog.ErrorFormat("matPath is empty for guid: {0}", guids[i]);
                    }
                    else
                    {
                        //AssetDatabase.DeleteAsset(matPath);
                        oldPathSet.Add(matPath);
                    }
                }
            }

            //然后依次复制
            foreach (var mat in sourceMaterialArray)
            {
                var matDuplicated = UnityEngine.Object.Instantiate(mat);
                var textureNameArray = matDuplicated.GetTexturePropertyNames();
                foreach(var name in textureNameArray)
                {
                    matDuplicated.SetTexture(name, null);  //不去掉的话shader包会依赖外部ab
                }
                string assetPath = AssetDatabase.GetAssetPath(mat);
                string assetGUID = string.Empty;
                if (!string.IsNullOrEmpty(assetPath))
                {
                    assetGUID = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
                }                
                string duplicatedMatPath = string.Format("{0}/dup_inst_mat_{1}_{2}.mat", s_shaderVariantConfig.exportPath, mat.name.Replace("/", "-"), assetGUID);
                if(oldPathSet.Contains(duplicatedMatPath))
                {
                    //复制属性到已存在的
                    Material oldMaterial = AssetDatabase.LoadAssetAtPath<Material>(duplicatedMatPath);
                    oldPathSet.Remove(duplicatedMatPath);
                    oldMaterial.CopyPropertiesFromMaterial(matDuplicated);
                }
                else
                {
                    //不存在的新建
                    AssetDatabase.CreateAsset(matDuplicated, duplicatedMatPath);
                }
                //AssetDatabase.Refresh();
                AssetImporter ai = AssetImporter.GetAtPath(duplicatedMatPath);
                ai.assetBundleName = s_shaderVariantConfig.assetbundleName;
                ai.assetBundleVariant = s_shaderVariantConfig.assetbundleVariant;
            }

            //删除废弃的
            foreach(var wastedPath in oldPathSet)
            {
                AssetDatabase.DeleteAsset(wastedPath);
            }

            //呼叫存盘
            AssetDatabase.SaveAssets();
        }

        public static void LogStage(string pattern)
        {
            GameDLL.GLog.LogFormat("DebugAutoCollectShader {0}", pattern);
        }
    }
}
