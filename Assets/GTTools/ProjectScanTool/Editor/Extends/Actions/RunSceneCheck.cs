/*
 * @Description: 场景资源检查
 */
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Linq;
using GameTechTools.CommonLibs.CommonExtends;

namespace CasualEngine.ProjectScanTool
{
    public class RunSceneCheck : MethodHelper
    {
        private static GameObject[] GetSceneObjects()
        {
            // Use this method since GameObject.FindObjectsOfType will not return disabled objects.
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
                       && go.hideFlags == HideFlags.None).ToArray();
        }

        public static bool Do_SceneRealtimeGICheck(SceneRealtimeGICheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.scene);
                foreach (var path in paths)
                {
                    var sceneName = Path.GetFileName(path);
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    if (Lightmapping.realtimeGI != checkDetail.realtimeLighting)
                    {
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            Lightmapping.realtimeGI = checkDetail.realtimeLighting;
                            EditorSceneManager.MarkSceneDirty(scene);
                            EditorSceneManager.SaveScene(scene);
                        }

                        customRule.Record(null, methodName, $"{sceneName}RealtimeGI设置和要求不符，要求是{checkDetail.realtimeLighting}", path, true);
                    }
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            return true;
        }

        #region 场景Missing Prefab检查
        public static bool Do_SceneMissPrefabCheck(SceneMissPrefabCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.scene);
            foreach (var path in paths)
            {
                var sceneName = Path.GetFileName(path);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                //拿到场景中所有的物体
                var sceneObjects = GetSceneObjects();
                foreach (var go in sceneObjects)
                {
                    string nodePath = ProjectScanHelper.GetNodePath(go.transform);
                    if (go.name.Contains("Missing Prefab"))
                    {
                        customRule.Record(go, methodName, $"{sceneName} has missing prefab {nodePath}", path);
                        continue;
                    }

                    if (PrefabUtility.IsPrefabAssetMissing(go))
                    {
                        customRule.Record(go, methodName, $"{sceneName} has missing prefab {nodePath}", path);
                    }
                }
                EditorSceneManager.CloseScene(scene, true);
            }
            return true;
        }
        #endregion

        #region 场景Missing Script检查
        public static bool Do_SceneMissScriptCheck(SceneMissScriptCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.scene);
            foreach (var path in paths)
            {
                var sceneName = Path.GetFileName(path);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                //拿到场景中所有的物体
                var sceneObjects = GetSceneObjects();
                foreach (var go in sceneObjects)
                {
                    string nodePath = ProjectScanHelper.GetNodePath(go.transform);
                    var components = go.GetComponentsInChildren<Component>(true);
                    foreach (var component in components)
                    {
                        if (!component)
                        {
                            customRule.Record(go, methodName, $"{sceneName}中{nodePath}节点存在组件引用丢失", path);
                            continue;
                        }
                    }
                }
                EditorSceneManager.CloseScene(scene, true);
            }
            return true;
        }
        #endregion
    }
}