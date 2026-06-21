using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameDLL;
using System.IO;
using System.Text.RegularExpressions;
using Unity.EditorCoroutines.Editor;
using System.Text;


namespace GameEditor.Tools.Statistics
{
    [Serializable]
    public class AutoAssignDependentABConfig
    {
        [Serializable]
        public class DirecotyRegex
        {
            public string pathPattern;
            public int matchIndex;
            public string replacement;
            public int subMatchIndex;
            public int variantMatchIndex;
        }

        public string autoPrefix;
        public string autoVariant;
        public string playerSuffix;
        public string sharePattern;
        public DirecotyRegex defaultCategory;
        public DirecotyRegex[] categories;
        public string[] exceptions;
        public string editorLuaBaseScenePath;
        public string luaScriptReporterWastedAssetWithAB;
        public string manualVariant;

        public string langABPattern;
        public string[] langVariants;

    }

    public enum ABAssetPass
    {
        First = 1,
        Second = 2,
        All = First | Second,
    }

    class AutoAssignDependentABTools
    {
        /// <summary>
        /// 排除掉不关心的资源，代码都是在首包内，不关心依赖
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public bool IsCaredAsset(string assetPath)
        {
            string ext = Path.GetExtension(assetPath).ToLower();
            switch (ext)
            {
                case ".cs":
                case ".dll":
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// 实现类，统一自动手动方法
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="config"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        static string GetABNameImp(string assetPath, AutoAssignDependentABConfig config, string prefix, out string variant)
        {
            string ext = Path.GetExtension(assetPath).ToLower();
            bool isScene = false;
            variant = string.Empty;
            if (!IsCaredAsset(assetPath))
            {
                return null;
            }
            switch (ext)
            {
                case ".unity":
                    isScene = true;
                    break;
                default:
                    break;
            }
            string assignedToAbName = null;
            foreach (var oneCategory in config.categories)
            {
                Regex regex = new Regex(oneCategory.pathPattern);
                if (regex.IsMatch(assetPath))
                {
                    Match m = regex.Match(assetPath);
                    if (m.Groups.Count <= oneCategory.matchIndex)
                    {
                        GameDLL.GLog.ErrorFormat("wrong matching index: dependAssetPath={0}, pathPattern={1}, matchIndex={2}, groupCount={3}", assetPath, oneCategory.pathPattern, oneCategory.matchIndex, m.Groups.Count);
                    }
                    else
                    {
                        string extraction = m.Groups[oneCategory.matchIndex].Value;
                        if(oneCategory.subMatchIndex > oneCategory.matchIndex)
                        {
                            string secondExtraction = m.Groups[oneCategory.subMatchIndex].Value;
                            assignedToAbName = string.Format(oneCategory.replacement, extraction, secondExtraction);
                            //UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);                            
                            //int hashCode = obj.GetHashCode();
                            //int split = hashCode % oneCategory.splitCount;
                            //assignedToAbName = string.Format(oneCategory.replacement, extraction, split);
                        }
                        else
                        {
                            assignedToAbName = string.Format(oneCategory.replacement, extraction);
                        }
                        if(oneCategory.variantMatchIndex > 0)
                        {
                            variant = m.Groups[oneCategory.variantMatchIndex].Value.ToLower();
                        }
                        if (isScene)
                        {
                            assignedToAbName += config.playerSuffix;  //在代码层面就把player资源和其他分开，这个是Unity打包限制，不需要考虑应用层需要
                        }
                    }
                    break;
                }
            }
            //if (string.IsNullOrEmpty(assignedToAbName))
            if(assignedToAbName==null)
            {
                assignedToAbName = (prefix + config.defaultCategory.replacement).ToLower(); //规则以为的，需要注意的资源
            }
            else if(assignedToAbName == string.Empty)
            {
                //规则以内故意不单独打包的资源
            }
            else
            {
                assignedToAbName = (prefix + assignedToAbName).ToLower();   //规则以内的资源
            }
            return assignedToAbName;
        }

        /// <summary>
        /// 根据正则找到对应的自动AB
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        static public string GetAutoABName(string assetPath, AutoAssignDependentABConfig config, out string variant)
        {
            return GetABNameImp(assetPath, config, config.autoPrefix, out variant);
        }

        /// <summary>
        /// 根据正则找到对应的手动AB
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string GetABName(string assetPath, AutoAssignDependentABConfig config, bool needAutoPrefix, out string variant)
        {
            return GetABNameImp(assetPath, config, needAutoPrefix?config.autoPrefix:string.Empty, out variant);
        }

        public static AutoAssignDependentABConfig GetAutoAssignDependentABConfig()
        {
            AutoAssignDependentABConfig config = null;
            const string jsonFilePath = "Assets/Editor Default Resources/AutoAssignDependentABTools.json";
            using (FileStream fs = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string content = sr.ReadToEnd();
                    config = JsonUtility.FromJson<AutoAssignDependentABConfig>(content);
                }
            }
            foreach (var oneCategory in config.categories)
            {
                Regex regex = new Regex(oneCategory.pathPattern);   //在这里先生成下，看看正则是否合规，免得等分析很久才报错
            }

            return config;
        }

        /// <summary>
        /// 对外部暴露的接口，共享字段
        /// </summary>
        /// <returns></returns>
        public static string GetSharePattern()
        {
            AutoAssignDependentABConfig config = GetAutoAssignDependentABConfig();
            if (config != null)
            {
                return config.sharePattern;
            }
            return null;
        }

        public static bool IsBuildingAsset(string path, ABAssetPass pass)
        {
            AssetImporter ai = AssetImporter.GetAtPath(path);
            string assetbundleName = ai.assetBundleName;
            if(string.IsNullOrEmpty(assetbundleName))
            {
                return false;
            }
            AutoAssignDependentABConfig config = GetAutoAssignDependentABConfig();
            if(assetbundleName.StartsWith(config.autoPrefix))
            {
                if((pass& ABAssetPass.Second)>0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if((pass & ABAssetPass.First) >0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 自动分配AB
        /// </summary>
        /// <param name="freeOnly"></param>
        public static void AutoDependentABImp(bool freeOnly)
        {
            EditorUtility.ClearProgressBar();
            var config = GetAutoAssignDependentABConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("Finish", "config file not exist", "Ok");
                return;
            }
            Dictionary<string, string> oldDict = new Dictionary<string, string>();
            Dictionary<string, string> newDict = new Dictionary<string, string>();

            string logFilePath = Path.Combine(Application.dataPath, "../AutoDependentAB.log");
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            using (FileStream fs = new FileStream(logFilePath, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    HashSet<string> firstPath = new HashSet<string>();
                    int assetBundleIndex = 0;
                    var existingAbNames = AssetDatabase.GetAllAssetBundleNames();
                    foreach (var existingAbName in existingAbNames)
                    {
                        Tools.StatisticsTools.ProgressBar("analysing", existingAbName, assetBundleIndex, existingAbNames.Length);
                        string[] caredAssetPath = AssetDatabase.GetAssetPathsFromAssetBundle(existingAbName);
                        if (existingAbName.StartsWith(config.autoPrefix))
                        {
                            //是自动的先解散，以后会重新分配
                            foreach (var assetPath in caredAssetPath)
                            {
                                AssetImporter ai = AssetImporter.GetAtPath(assetPath);
                                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                                if (obj == null)
                                {
                                    sw.WriteLine(string.Format("free error\t{0}", assetPath));
                                }
                                else
                                {
                                    string old = ai.assetBundleName;
                                    oldDict.Add(assetPath, old);
                                    ai.assetBundleVariant = string.Empty;   //要先释放variant，不然会报错
                                    ai.assetBundleName = string.Empty;
                                    EditorUtility.SetDirty(obj);
                                    sw.WriteLine(string.Format("free\t{0}\t{1}", assetPath, old));
                                }
                            }
                        }
                        else
                        {
                            //不是自动的加入依赖检查
                            foreach (var assetPath in caredAssetPath)
                            {
                                firstPath.Add(assetPath);
                            }
                        }
                        assetBundleIndex++;
                    }
                    if (!freeOnly)
                    {
                        HashSet<string> exceptions = new HashSet<string>(config.exceptions);
                        Tools.StatisticsTools.ProgressBar("please wait", "fetching dependency", 0, 1);
                        string[] allDepends = AssetDatabase.GetDependencies(firstPath.ToArray());
                        int depedingIndex = 0;
                        foreach (var dependAssetPath in allDepends)
                        {
                            Tools.StatisticsTools.ProgressBar("assigning", dependAssetPath, depedingIndex, allDepends.Length);
                            ++depedingIndex;
                            if (firstPath.Contains(dependAssetPath))
                                continue;       //在手动指定的包中，不关心
                            string variant;
                            string assignedToAbName = GetAutoABName(dependAssetPath, config, out variant);
                            if (string.IsNullOrEmpty(assignedToAbName))
                                continue;       //不关心的
                            if (exceptions.Contains(dependAssetPath))
                                continue;       //不进行AB归档的，让Shader自动收集进其他AB，临时解决那些奇怪的变体问题
                            AssetImporter ai = AssetImporter.GetAtPath(dependAssetPath);
                            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dependAssetPath);
                            if (obj == null)
                            {
                                sw.WriteLine(string.Format("assign error\t{0}\tnull", dependAssetPath));
                            }
                            else
                            {
                                EditorUtility.SetDirty(obj);
                                ai.assetBundleName = assignedToAbName;
                                //ai.assetBundleVariant = config.autoVariant;   --带变体后缀的包不能收集同名资源，所以不能加
                                ai.assetBundleVariant = variant;
                                sw.WriteLine(string.Format("assign\t{0}\t{1}", dependAssetPath, assignedToAbName));

                                newDict.Add(dependAssetPath, assignedToAbName);
                            }
                        }
                    }
                }
            }

            HashSet<string> diffSet = new HashSet<string>();
            foreach(var kvOld in oldDict)
            {
                string oldPath = kvOld.Key;
                string oldAb = kvOld.Value;
                string newAb = null;
                if(newDict.TryGetValue(oldPath, out newAb))
                {
                    if(newAb != oldAb)
                    {
                        //说明AB已更改，一般不会发生，除非是修改了规则
                        diffSet.Add(oldPath);
                    }
                    else
                    {
                        //说明没有更改
                    }
                }
                else
                {
                    //说明已经不再需要打包，需要更新svn
                    diffSet.Add(oldPath);
                }
            }
            foreach(var kvNew in newDict)
            {
                string newPath = kvNew.Key;
                if(oldDict.ContainsKey(newPath))
                {
                    //在oldDict遍历中已经处理过了
                }
                else
                {
                    //这些是新增的，需要提交svn
                    diffSet.Add(newPath);
                }
            }
            foreach(var diff in diffSet)
            {
                StatisticsTools.MarkModifiedFile(diff, true);
            }

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            //因为ShaderVariant必须和Shader打在一个包里，free之后被解除打包了，收集结束后必须重算一下，因此到这里并没有结束
            if(freeOnly)
            {
                EditorUtility.DisplayDialog("Finish", "process finished", "Ok");
            }
        }

        [MenuItem("Tools/StatiticsTools/AutoDependent/AutoAssignDependentAB", false, 201)]
        public static void AutoAssignDependentAB()
        {
            AutoDependentABImp(false);
            AutoCollectShaderVariantTool.DoExtern();
        }

        [MenuItem("Tools/StatiticsTools/AutoDependent/AutoFreeDependentAB", false, 202)]
        public static void AutoFreeDependentAB()
        {
            AutoDependentABImp(true);
        }

        //[MenuItem("Tools/StatiticsTools/TestRegexReplace", false, 202)]
        static void TestRegexReplace()
        {
            //string testInput = @"Assets/Arts/Models/Scenes/Agora/texture/senlin_1.tga";
            //Regex pattern = new Regex(@"Assets/Arts/Models/Scenes/([^/\\]+)?([/\\]*)");
            //string testInput = @"Assets/Arts/Shaders/item/worldmap_text.shader";
            //Regex pattern = new Regex(@"Assets/([\s\S]+)\.shader$");
            //string testInput = @"Assets/Arts/Materials/mtl_none.mat";
            string testInput = @"Assets/Arts/Models/Scenes/Common/Props/Material/treasure_chest01.mat";
            //Regex pattern = new Regex(@"Assets/Arts/Materials/(.+)");
            Regex pattern = new Regex(@"Assets/Arts/Models/Scenes/Common/Props/([^/\\\\]+)?([/\\\\]*)");
            bool found = pattern.IsMatch(testInput);
            int index = 1;
            string replacement = @"arts/scene/common/props/{0}";
            if (found)
            {
                Match m = pattern.Match(testInput);
                int count = m.Groups.Count;
                for (int expIndex = 0; expIndex < count; ++expIndex)
                {
                    string s1 = m.Groups[expIndex].Value;
                    GameDLL.GLog.LogFormat("s{0}={1}", expIndex, s1);
                }
                string extraction = m.Groups[index].Value;
                string assignedToAbName = string.Format(replacement, extraction);
                GLog.LogFormat("assignedToAbName={0}", assignedToAbName);
            }
            else
            {
                GameDLL.GLog.Log("not found");
            }
        }

        static IEnumerator TestCircularCo()
        {
            string abCommon = "__auto/arts/scene/common111";
            string abChapter = "__auto/arts/scene/chapter one111";
            Dictionary<string, string> assetToNewAB = new Dictionary<string, string>();
            assetToNewAB.Add("Assets/Arts/Models/Scenes/Common/VFX/Materials/Lightbeam_Rays 5.mat", abCommon);
            assetToNewAB.Add("Assets/Arts/Models/Scenes/Battle/Chapter One/VFX/Textures/Lightbeam.tga", abChapter);
            assetToNewAB.Add("Assets/Arts/Models/Scenes/Battle/Chapter One/Materials/leaf11.mat", abChapter);
            assetToNewAB.Add("Assets/Arts/Models/Scenes/Common/Textures/Forest_combine_D_002.tga", abCommon);

            /*
            foreach (var kv in assetToNewAB)
            {
                string assetPath = kv.Key;
                AssetImporter ai = AssetImporter.GetAtPath(assetPath);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                ai.assetBundleName = kv.Value;
                ai.assetBundleVariant = "z";
                EditorUtility.SetDirty(obj);
            }

            AssetDatabase.Refresh();
            yield return null;

            string[] depCommon = AssetDatabase.GetAssetBundleDependencies(abCommon, false);
            GameDLL.GLog.LogFormat("GetAssetBundleDependencies begin {0}", abCommon);
            foreach (var dep in depCommon)
            {
                GameDLL.GLog.Log(dep);
            }
            GameDLL.GLog.LogFormat("GetAssetBundleDependencies end {0}", abCommon);
            string[] depChapter = AssetDatabase.GetAssetBundleDependencies(abCommon, false);
            GameDLL.GLog.LogFormat("GetAssetBundleDependencies begin {0}", abChapter);
            foreach (var dep in depChapter)
            {
                GameDLL.GLog.Log(dep);
            }
            GameDLL.GLog.LogFormat("GetAssetBundleDependencies end {0}", abChapter);
            */
            AssetDependencyRegistry reg = new AssetDependencyRegistry();
            reg.Clear();
            foreach (var kv in assetToNewAB)
            {
                reg.AddAssetToAB(kv.Key, kv.Value);
            }

            GameDLL.GLog.LogFormat("GetAssetBundleDependencies begin {0}", abCommon);
            var link = reg.getAllABDependency(abCommon);
            foreach (var dep in link)
            {
                GameDLL.GLog.Log(dep);
            }
            GameDLL.GLog.LogFormat("GetAssetBundleDependencies end {0}", abCommon);

            //foreach (var kv in assetToNewAB)
            //{
            //    string assetPath = kv.Key;
            //    AssetImporter ai = AssetImporter.GetAtPath(assetPath);
            //    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            //    ai.assetBundleVariant = string.Empty;
            //    ai.assetBundleName = string.Empty;
            //    EditorUtility.SetDirty(obj);
            //}

            AssetDatabase.Refresh();
            yield return null;
        }

        class AssetDependencyRegistry
        {
            Dictionary<string, HashSet<string>> abToAssets = new Dictionary<string, HashSet<string>>();
            Dictionary<string, string> assetToAB = new Dictionary<string, string>();

            public void Clear()
            {
                abToAssets.Clear();
                assetToAB.Clear();
            }

            public void AddAssetToAB(string assetPath, string abName)
            {
                HashSet<string> set = null;
                if(!abToAssets.TryGetValue(abName, out set))
                {
                    set = new HashSet<string>();
                    abToAssets.Add(abName, set);
                }
                set.Add(assetPath);
                assetToAB.Add(assetPath, abName);
            }

            public string[] getAllABDependency(string abName)
            {
                HashSet<string> set = abToAssets[abName];
                string[] assets = AssetDatabase.GetDependencies(set.ToArray());
                HashSet<string> abSet = new HashSet<string>();
                foreach(var asset in assets)
                {
                    string ab = null;
                    if(!assetToAB.TryGetValue(asset, out ab))
                    {
                        ab = "unknown";
                    }
                    abSet.Add(ab);
                }

                return abSet.ToArray();
            }
        }


        //[MenuItem("Tools/StatiticsTools/TestCircularCheck", false, 203)]
        static void TestCircularCheck()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(TestCircularCo());
        }

        [MenuItem("Tools/StatiticsTools/CheckBundle/ReportABByDatabase", false, 23)]
        static void ReportABByDatabase()
        {
            EditorUtility.ClearProgressBar();
            var config = GetAutoAssignDependentABConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("Finish", "config file not exist", "Ok");
                return;
            }

            //string dateString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            //string reportFolderName = string.Format("ReportABByDatabase-{0}", dateString);
            //string destDirectory = EditorUtility.SaveFolderPanel("Save Report To", string.Empty, string.Empty);
            //if (string.IsNullOrEmpty(destDirectory))
            //    return;

            //string destFolder = Path.Combine(destDirectory, reportFolderName);
            //GLog.LogFormat("ReportABRelation to {0}", destFolder);
            //if(Directory.Exists(destFolder))
            //{
            //    Directory.Delete(destFolder, true);
            //}
            //Directory.CreateDirectory(destFolder);
            string destFolder = StatisticsTools.OpenSaveFolderDialog("ReportABByDatabase");
            if(destFolder==null)
            {
                return;
            }

            Dictionary<string, string> assetToAB = new Dictionary<string, string>();    //资源到AB的查找表 writen
            Dictionary<string, HashSet<string>> abToAssets = new Dictionary<string, HashSet<string>>(); //AB到资源的查找表 writen
            Dictionary<string, HashSet<string>> assetToDependent = new Dictionary<string, HashSet<string>>();   //资源到依赖的查找表 writen
            Dictionary<string, HashSet<string>> assetToDepender = new Dictionary<string, HashSet<string>>();    //资源到依赖者的查找表 writen
            Dictionary<string, HashSet<string>> abToDependent = new Dictionary<string, HashSet<string>>();  //AB到依赖AB的查找表
            Dictionary<string, HashSet<string>> abToDepender = new Dictionary<string, HashSet<string>>();   //AB到依赖者AB的查找表

            //收集手动指定的AB
            HashSet<string> manualAssets = new HashSet<string>();   //手动指定的AB
            HashSet<string> externAssets = new HashSet<string>();   //自动收集的AB
            var existingAbNames = AssetDatabase.GetAllAssetBundleNames();
            int assetBundleIndex = 0;
            foreach (var existingAbName in existingAbNames)
            {
                Tools.StatisticsTools.ProgressBar("collecting", existingAbName, assetBundleIndex, existingAbNames.Length);
                ++assetBundleIndex;
                if (existingAbName.StartsWith(config.autoPrefix))
                    continue;   //自动打包的跳过
                string[] caredAssetPath = AssetDatabase.GetAssetPathsFromAssetBundle(existingAbName);
                HashSet<string> containingAssets = new HashSet<string>();
                abToAssets.Add(existingAbName, containingAssets);
                foreach (var assetPath in caredAssetPath)
                {
                    manualAssets.Add(assetPath);    //防止手动指定资源之间有相互依赖关系，这里先放进表里
                    assetToAB.Add(assetPath, existingAbName);   //添加资源到AB查找表
                    containingAssets.Add(assetPath);    //添加资源到AB查找表
                }
            }

            int manualIndex = 0;
            foreach (var manualAsset in manualAssets)
            {
                Tools.StatisticsTools.ProgressBar("dependence", manualAsset, manualIndex, manualAssets.Count);
                manualIndex++;
                string[] dependencies = AssetDatabase.GetDependencies(manualAsset);
                foreach (var dependentAsset in dependencies)
                {
                    if (manualAssets.Contains(dependentAsset))
                        continue;
                    externAssets.Add(dependentAsset);
                }
            }
            manualAssets.UnionWith(externAssets);   //此时manualAssets含义已经变成所有资源


            //收集外部引用的AB
            manualIndex = 0;
            foreach(var manualAsset in manualAssets)
            {
                Tools.StatisticsTools.ProgressBar("analysing", manualAsset, manualIndex, manualAssets.Count);
                manualIndex++;

                string[] dependencies = AssetDatabase.GetDependencies(manualAsset);
                HashSet<string> dependent = new HashSet<string>();
                assetToDependent.Add(manualAsset, dependent);
                foreach (var dependentAsset in dependencies)
                {
                    if (dependentAsset == manualAsset)
                        continue;   //跳过自己
                    if (!IsCaredAsset(dependentAsset))
                        continue;   //跳过代码
                    dependent.Add(dependentAsset);  //资源到依赖的查找表

                    HashSet<string> depender = null;
                    if(!assetToDepender.TryGetValue(dependentAsset, out depender))
                    {
                        depender = new HashSet<string>();
                        assetToDepender.Add(dependentAsset, depender);
                    }
                    depender.Add(manualAsset);  //资源到依赖者的查找表

                    //外部自动收集资源
                    string variant;
                    string externAbName = GetAutoABName(dependentAsset, config, out variant);
                    if (!assetToAB.ContainsKey(dependentAsset))
                    {
                        assetToAB.Add(dependentAsset, externAbName);
                    }
                    HashSet<string> containingAssets = null;
                    if(!abToAssets.TryGetValue(externAbName, out containingAssets))
                    {
                        containingAssets = new HashSet<string>();
                        abToAssets.Add(externAbName, containingAssets);
                    }
                    containingAssets.Add(dependentAsset);
                }
            }

            string summaryFilePath = Path.Combine(destFolder, "ABRelation.txt");
            string detailPath = Path.Combine(destFolder, "detail");
            Directory.CreateDirectory(detailPath);
            using (FileStream fs = new FileStream(summaryFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string title = "name\tin\tout\tinAB\toutAB";
                    sw.WriteLine(title);
                    var ignoringName = "LightingData.asset";

                    //遍历所有AB，查找引用和被引用关系
                    int abIndex = 0;
                    foreach (var abKV in abToAssets)
                    {
                        string abName = abKV.Key;
                        HashSet<string> containingAssets = abKV.Value;
                        Tools.StatisticsTools.ProgressBar("saving", abName, abIndex, abToAssets.Count);
                        manualIndex++;

                        Dictionary<string, List<string>> outABSet = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> inABSet = new Dictionary<string, List<string>>();
                        int inCount = 0;
                        int outCount = 0;
                        string inList = string.Empty;
                        string outList = string.Empty;

                        string detailFilePath = Path.Combine(detailPath, abName) + ".txt";
                        string detailFolderPath = Path.GetDirectoryName(detailFilePath);
                        if(!Directory.Exists(detailFolderPath))
                        {
                            Directory.CreateDirectory(detailFolderPath);
                        }
                        using (FileStream fsd = new FileStream(detailFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            using (StreamWriter swd = new StreamWriter(fsd))
                            {
                                swd.WriteLine("==================================================================");
                                swd.WriteLine("=======资源列表=======");
                                foreach (var assetInAb in containingAssets)
                                {
                                    swd.WriteLine(assetInAb);
                                }
                                swd.WriteLine(string.Empty);

                                //引用别人
                                swd.WriteLine("==================================================================");
                                swd.WriteLine("=======引用别人=======");
                                foreach (var assetInAb in containingAssets)
                                {
                                    var fileName = Path.GetFileName(assetInAb);
                                    //if (fileName == ignoringName)
                                    //    continue;   //这种资源的依赖关系不会被打进包里
                                    HashSet<string> dependentSet = null;
                                    if (assetToDependent.TryGetValue(assetInAb, out dependentSet))
                                    {
                                        foreach(var dependentAsset in dependentSet)
                                        {
                                            string dependentAB = null;
                                            if(!assetToAB.TryGetValue(dependentAsset, out dependentAB))
                                            {
                                                string error = string.Format("can't find ab for asset {0}", dependentAsset);
                                                GLog.Error(error);
                                                swd.WriteLine(error);
                                            }
                                            else if(dependentAB == abName)
                                            {
                                                //自己依赖自己就不用统计了
                                            }
                                            else
                                            {
                                                List<string> linkList = null;
                                                if(!outABSet.TryGetValue(dependentAB, out linkList))
                                                {
                                                    linkList = new List<string>();
                                                    outABSet.Add(dependentAB, linkList);
                                                }
                                                linkList.Add(string.Format("{0}\t->\t{1}", assetInAb, dependentAsset));
                                            }
                                        }
                                    }
                                }
                                foreach(var outABKV in outABSet)
                                {
                                    string outAbName = outABKV.Key;
                                    List<string> linkList = outABKV.Value;
                                    swd.WriteLine(outAbName);
                                    foreach(var link in linkList)
                                    {
                                        swd.WriteLine("\t" + link);
                                    }
                                }
                                outCount = outABSet.Count;
                                outList = string.Join(",", outABSet.Keys.ToArray());

                                swd.WriteLine(string.Empty);
                                //被别人引用
                                swd.WriteLine("==================================================================");
                                swd.WriteLine("=======被别人引用=======");
                                foreach (var assetInAb in containingAssets)
                                {
                                    HashSet<string> dependerSet = null;
                                    if (assetToDepender.TryGetValue(assetInAb, out dependerSet))
                                    {
                                        foreach (var dependerAsset in dependerSet)
                                        {
                                            string dependerAB = null;
                                            if (!assetToAB.TryGetValue(dependerAsset, out dependerAB))
                                            {
                                                string error = string.Format("can't find ab for asset {0}", dependerAsset);
                                                GLog.Error(error);
                                                swd.WriteLine(error);
                                            }
                                            else if (dependerAB == abName)
                                            {
                                                //自己依赖自己就不用统计了
                                            }
                                            else
                                            {
                                                List<string> linkList = null;
                                                if (!inABSet.TryGetValue(dependerAB, out linkList))
                                                {
                                                    linkList = new List<string>();
                                                    inABSet.Add(dependerAB, linkList);
                                                }
                                                linkList.Add(string.Format("{0}\t->\t{1}", dependerAsset, assetInAb));
                                            }
                                        }
                                    }
                                }
                                foreach (var inABKV in inABSet)
                                {
                                    string inAbName = inABKV.Key;
                                    List<string> linkList = inABKV.Value;
                                    swd.WriteLine(inAbName);
                                    foreach (var link in linkList)
                                    {
                                        swd.WriteLine("\t" + link);
                                    }
                                }
                                inCount = inABSet.Count;
                                inList = string.Join(",", inABSet.Keys.ToArray());

                                //写入统计
                                string summaryLine = string.Format("{0}\t{1}\t{2}\t{3}\t{4}", abName, inCount, outCount, inList, outList);
                                sw.WriteLine(summaryLine);
                            }
                        }

                    }
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Finish", "ReportABByDatabase process finished", "Ok");
        }

        public static List<string> GetAllBuildingAssets(string[] filterExt, bool toGUID, ABAssetPass pass)
        {
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
                EditorUtility.DisplayDialog("Finish", "config file not exist", "Ok");
                return null;
            }

            HashSet<string> allAssets = new HashSet<string>();
            string[] assetNames = AssetDatabase.GetAllAssetPaths();
            foreach (var assetName in assetNames)
            {
                if (!assetName.StartsWith(dependentConfig.autoPrefix))
                    continue;   //自动收集的不考虑，因为FBX会带入default，而且不会实例化

                //var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetName);

                //foreach (string a in assetPaths)
                {
                    allAssets.Add(assetName);
                }
            }

            List<string> output = new List<string>();
            string[] firstPass = allAssets.ToArray();
            if((pass & ABAssetPass.Second)>0)
            {
                string[] secondPass = AssetDatabase.GetDependencies(firstPass);
                foreach (var s in secondPass)
                {
                    allAssets.Add(s);
                }
            }
            if((pass & ABAssetPass.First) == 0)
            {
                HashSet<string> firstSet = new HashSet<string>(firstPass);
                allAssets.ExceptWith(firstSet);
            }
            foreach (var assetPath in allAssets)
            {
                var ext = Path.GetExtension(assetPath);
                if (filterExt == null)  //传空表示所有
                {
                    if (toGUID)
                    {
                        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
                        output.Add(assetGUID);
                    }
                    else
                    {
                        output.Add(assetPath);
                    }
                }
                else
                {
                    foreach (var filter in filterExt)
                    {
                        if (ext.ToLower() == filter)
                        {
                            if (toGUID)
                            {
                                string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
                                output.Add(assetGUID);
                            }
                            else
                            {
                                output.Add(assetPath);
                            }
                            break;
                        }
                    }
                }
            }
            return output;
        }
    }
}
