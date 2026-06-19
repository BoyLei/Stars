/*
 * @Description: 美术资源检测
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameTechTools.CommonLibs.CommonExtends;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace CasualEngine.ProjectScanTool
{
    public class RunBasicAssetsCheck : MethodHelper
    {
        #region 检测资源名称是否含有中文或空格
        public static bool Do_PathEmptyOrChineseCheck(PathEmptyOrChineseCheck customRule)
        {
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAllAssetPaths(customRule, checkDetail);
                foreach (var path in paths)
                {
                    string lowerPath = path.ToLower();

                    if (ProjectScanHelper.isResHasChineseOrSpace(path, checkDetail.checkChild))
                    {
                        customRule.Record(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), "Do_PathEmptyOrChineseCheck", "资源名包含空格或中文", path);
                    }
                }
            }

            return true;
        }
        #endregion

        #region Missing Prefab检查
        public static bool Do_MissPrefabCheck(MissPrefabCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.prefab);
            foreach (var path in paths)
            {
                Object o = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (o == null)
                {
                    continue;
                }
                FindMissingPrefabInGO(customRule, methodName, path, o as GameObject, o.name, true);
            }
            return true;
        }

        public static void FindMissingPrefabInGO<T>(T customRule, string methodName, string prefabPath, GameObject go, string prefabName, bool isRoot) where T : CustomRule
        {
            string nodePath = ProjectScanHelper.GetNodePath(go.transform);
            if (go.name.Contains("Missing Prefab"))
            {
                customRule.Record(go, methodName, $"{prefabName} has missing prefab {nodePath}", prefabPath);
                return;
            }

            if (PrefabUtility.IsPrefabAssetMissing(go))
            {
                customRule.Record(go, methodName, $"{prefabName} has missing prefab {nodePath}", prefabPath);
                return;
            }

            if (PrefabUtility.IsDisconnectedFromPrefabAsset(go))
            {
                customRule.Record(go, methodName, $"{prefabName} has missing prefab {nodePath}", prefabPath);
                return;
            }
            var components = go.GetComponents<Component>();

            foreach (var component in components)
            {
                // Missing components will be null, we can't find their type, etc.
                if (!component)
                {
                    //Debug.LogErrorFormat(go, $"Missing Component {0} in GameObject: {1}", component.GetType().FullName, GetFullPath(go));
                    customRule.Record(go, methodName, $"{prefabName} has missing prefab {nodePath}", prefabPath);
                    continue;
                }

                SerializedObject so = new SerializedObject(component);
                var sp = so.GetIterator();

                var objRefValueMethod = typeof(SerializedProperty).GetProperty("objectReferenceStringValue",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                // Iterate over the components' properties.
                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        string objectReferenceStringValue = string.Empty;

                        if (objRefValueMethod != null)
                        {
                            objectReferenceStringValue = (string)objRefValueMethod.GetGetMethod(true).Invoke(sp, new object[] { });
                        }

                        if (sp.objectReferenceValue == null
                            && (sp.objectReferenceInstanceIDValue != 0 || objectReferenceStringValue.StartsWith("Missing")))
                        {
                            customRule.Record(go, methodName, $"{prefabName} has missing prefab {nodePath} reason :{objectReferenceStringValue}", prefabPath);
                        }
                    }
                }
            }
            if (!isRoot)
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    return;
                }
                GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(go);
                if (root == go)
                {
                    return;
                }
            }

            foreach (Transform childT in go.transform)
            {
                FindMissingPrefabInGO(customRule, methodName, prefabPath, childT.gameObject, prefabName, false);
            }
        }
        #endregion

        #region Missing Script检查
        public static bool Do_MissScriptCheck(MissScriptCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.prefab);
            foreach (var path in paths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }
                Transform[] childs = prefab.GetComponentsInChildren<Transform>(true);
                foreach (var child in childs)
                {
                    string childPath = ProjectScanHelper.GetNodePath(child.transform);
                    Component[] scripts = child.gameObject.GetComponents(typeof(MonoBehaviour));
                    foreach (var ob in scripts)
                    {
                        if (ob == null)
                        {
                            customRule.Record(prefab, methodName, $"{prefab.name}中节点{childPath}存在组件引用丢失", path);
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        #region UI子节点数量检查
        public static bool Do_UIChildsCountCheck(UIChildsCountCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab);
                foreach (var path in paths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                    {
                        continue;
                    }
                    Transform[] childs = prefab.GetComponentsInChildren<Transform>(true);
                    if (childs.Length > checkDetail.numChildren)
                    {
                        customRule.Record(prefab, methodName, $"{prefab.name}中子节点数量过多({childs.Length})", path);
                    }
                }
            }

            return true;
        }
        #endregion

        #region 检查材质是否误引用图集
        public static bool Do_MaterialRefAtlasCheck(MaterialRefAtlasCheck customRule)
        {
            // Func<string, MaterialRefAtlasCheckDetail, bool> InAtlasDir = (texpath, detail) =>
            // {
            //     foreach (var atlasPath in detail.spriteAtlasDirs)
            //     {
            //         if (texpath.IndexOf(atlasPath) >= 0)
            //         {
            //             return true;
            //         }
            //     }
            //     return false;
            // };

            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.material);
                foreach (var path in paths)
                {
                    Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                    if (mat == null)
                        continue;

                    //GetTexture无法找到所有的材质，需要利用资源依赖项
                    // Texture tex = mat.GetTexture("_MainTex");
                    // if (!tex)
                    //     continue;

                    Object[] roots = new Object[] { mat };
                    Object[] dependObjs = EditorUtility.CollectDependencies(roots);
                    foreach (Object dependObj in dependObjs)
                    {
                        if (dependObj.GetType() == typeof(Texture2D))
                        {
                            string texpath = AssetDatabase.GetAssetPath(dependObj.GetInstanceID());
                            TextureImporter texImporter = AssetImporter.GetAtPath(texpath) as TextureImporter;
                            if (texImporter != null && texImporter.textureType == TextureImporterType.Sprite)
                            {
                                customRule.Record(mat, methodName, "材质误引用了图集  " + texpath, path);
                            }
                            // if (!string.IsNullOrEmpty(texpath) && (InAtlasDir(texpath, checkDetail) || Path.GetExtension("texpath") == ".spriteatlas"))
                            // {
                            //     customRule.Record(mat, methodName, path + "  误引用了图集  " + texpath, path);
                            // }
                        }
                    }
                    Resources.UnloadAsset(mat);
                }
            }
            return true;
        }
        #endregion

        #region 检查材质纹理引用丢失 guid无法定位资源
        public static bool Do_MaterialRefMissingCheck(MaterialRefMissingCheck customRule)
        {

            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.material);
            foreach (var path in paths)
            {
                ProjectScanHelper.BeginReceiveEditorLog();
                //当guid无法定位资源时，loadAsset则会抛出Broken text PPtr. GUID相关异常日志，通过捕获日志来确认资源存在异常
                Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                ProjectScanHelper.EndReceiveEditorLog();
                List<string> editorLogs = ProjectScanHelper.GetEditorLog();
                if (editorLogs.Count > 0)
                {
                    foreach (var info in editorLogs)
                    {
                        if (info.IndexOf("Broken text PPtr. GUID") > -1)
                        {
                            customRule.Record(mat, methodName, path + " 存在纹理引用丢失", path);
                        }
                    }
                }
                Resources.UnloadAsset(mat);

            }
            ProjectScanHelper.GetEditorLog().Clear();
            return true;
        }
        #endregion
    }
}