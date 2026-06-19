//在ProjectScanDef.cs中添加新的模块类型 (如果需要添加新模块的话，否则直接添加检查项即可)

//扫描类型唯一标识
//public enum EnumScanModes
//{
//    通用设置 = 0,
//    基本资源检查,
//    贴图资源检查,
//    音频资源检查,
//    动效资源检查,
//    场景检查,
//    eExampleCheck
//}

//自定义某个检测项
using Codice.Utils;
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

namespace CasualEngine.ProjectScanTool
{
    [Serializable] 
    public class Texture_ResourceNoReferenceCheckDetail : CustomCheckDetail
    {
       
    }

    [Serializable]
    //注册特性，并绑定对应模块类型
    [CustomScanType(EnumScanModes.基本资源检查)]
    public class Texture_ResourceNoReference : CustomRule
    {
        static string[] allAssetPaths;
        void OnEnable()
        {
            ruleTitle = "资源是否有引用";
            ruleDescription = "[说 明]:检查资源是否有引用";
        }

        //定义检查规则列表
        [ListDrawerSettings(Expanded = true), LabelText("检查资源是否有引用")]
        public List<Texture_ResourceNoReferenceCheckDetail> checkDetailList = new List<Texture_ResourceNoReferenceCheckDetail>() { new Texture_ResourceNoReferenceCheckDetail() };

        //注册扫描的执行逻辑
        [CustomScanAction]
        public void Do_CustomExampleCheck()
        {
            //添加逻辑
            ProjectScanHelper.DoCustomRuleCheck(Do_ResourceNoReferenceCheck, this);
        }
        #region 贴图是否重复统计
        public static bool Do_ResourceNoReferenceCheck(Texture_ResourceNoReference customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] guids = AssetDatabase.FindAssets("t:Object", null);
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
                foreach (var path in paths)
                {
                    bool find = false;
                    if (!string.IsNullOrEmpty(path))
                    {
                        foreach (string guid in guids)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);

                            if (ArrayUtility.Contains(dependencies, path))
                            {
                                find = true;
                                break;
                            }
                        }
                    }
                    if (!find) 
                    {
                        customRule.Record(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), methodName, "资源引用为0 ", path);
                    }
                }
            }
            return true;
        }
        //#region 贴图是否重复统计
        //public static bool Do_ResourceNoReferenceCheck(Texture_ResourceNoReference customRule)
        //{
        //    string methodName = MethodBase.GetCurrentMethod().Name;
        //    foreach (var checkDetail in customRule.checkDetailList)
        //    {
        //        string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
        //        foreach (var path in paths)
        //        {
        //            if (!string.IsNullOrEmpty(path))
        //            {
        //                string guid = AssetDatabase.AssetPathToGUID(path);
        //                var withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        //                string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
        //                    .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        //                int startIndex = 0;
        //                EditorApplication.update = delegate ()
        //                {
        //                    string file = files[startIndex];

        //                    bool isCancel = EditorUtility.DisplayCancelableProgressBar($"匹配资源中{path}", file, (float)startIndex / (float)files.Length);
        //                    if (Regex.IsMatch(File.ReadAllText(file), guid))
        //                    {
        //                        Debug.Log(file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(file)));
        //                        customRule.Record(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(file)), methodName, "资源引用为0 ", path);
        //                    }

        //                    startIndex++;
        //                    if (isCancel || startIndex >= files.Length)
        //                    {
        //                        EditorUtility.ClearProgressBar();
        //                        EditorApplication.update = null;
        //                        startIndex = 0;
        //                        Debug.Log("匹配结束");
        //                    }

        //                };
        //            }
        //        }
        //    }
        //    return true;
        //}
        //static private string GetRelativeAssetsPath(string path)
        //{
        //    return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        //}

        //public static bool Do_ResourceNoReferenceCheck(Texture_ResourceNoReference customRule)
        //{
        //    allAssetPaths = AssetDatabase.GetAllAssetPaths();

        //    string methodName = MethodBase.GetCurrentMethod().Name;
        //    Dictionary<string,List<string>> textureHashes = new Dictionary<string, List<string>>();
        //    foreach (var checkDetail in customRule.checkDetailList)
        //    {
        //        string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
        //        foreach (var path in paths)
        //        {
        //            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        //            if (asset == null)
        //                continue;
        //            string guid = AssetDatabase.AssetPathToGUID(path);
        //            bool hasReference = false;
        //            foreach (var assetPath in allAssetPaths)
        //            {
        //                if (assetPath.EndsWith(".prefab") || assetPath.EndsWith(".unity") || assetPath.EndsWith(".mat") || assetPath.EndsWith(".shader") || assetPath.EndsWith(".lightmap") || assetPath.EndsWith(".particle") || assetPath.EndsWith(".flare") || assetPath.EndsWith(".anim") || assetPath.EndsWith(".physicMaterial") || assetPath.EndsWith(".spriteatlas"))
        //                { 
        //                    string content = File.ReadAllText(assetPath);
        //                    if (content == null)
        //                    {
        //                        continue;
        //                    }

        //                    if (content.IndexOf(guid) > 0)
        //                    {
        //                        hasReference = true;
        //                        break;
        //                    }
        //                }
        //            }
        //            //if (!hasReference) 
        //            //{
        //            //    string[] fileExtensions = { "*.cs", "*.lua", "*.lua.txt", "*.json" }; // Add or remove based on your needs

        //            //    foreach (string extension in fileExtensions)
        //            //    {
        //            //        // Get all files with the current extension in the folder and subfolders
        //            //        var codeFiles = System.IO.Directory.GetFiles(Application.dataPath, extension, System.IO.SearchOption.AllDirectories);

        //            //        foreach (var file in codeFiles)
        //            //        {
        //            //            string fileContent = System.IO.File.ReadAllText(file);
        //            //            if (fileContent.Contains(asset.name))
        //            //            {
        //            //                hasReference = true;
        //            //                break;
        //            //            }
        //            //        }
        //            //        if (hasReference) break;
        //            //    }
        //            //}
        //            if (!hasReference) { customRule.Record(asset, methodName, "资源引用为0 ", path); }
        //            Resources.UnloadAsset(asset);
        //        }
        //    }
        //    return true;
        //}
        #endregion
    }
}