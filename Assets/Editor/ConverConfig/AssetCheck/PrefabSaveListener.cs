using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using StarProject.OffLine;

public class PrefabSaveListener : UnityEditor.AssetModificationProcessor
{
    private static bool isSavingPrefab = false;

    private static string[] OnWillSaveAssets(string[] paths)
    {
        foreach (string path in paths)
        {
            if (path.EndsWith(".prefab"))
            {
                isSavingPrefab = true;

                // 处理 Prefab 保存事件
                Debug.Log("Prefab saved: " + path);

                // 获取Prefab的Asset路径
                string prefabAssetPath = path.Replace(Application.dataPath, "Assets");

                // 选中Prefab
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);

                // 执行Prefab上的脚本
                if (prefab != null && prefab.GetComponent<PanelOffLineData>() != null)
                {
                    prefab.GetComponent<PanelOffLineData>().GenerateNodesData();
                    Debug.Log("检测你修改引导相关界面，请手动Shift + Alt + W 更新数据【我已经帮你选中当前节点了】");
                    Debug.LogWarning("检测你修改引导相关界面，请手动Shift + Alt + W 更新数据【我已经帮你选中当前节点了】");
                    Debug.LogError("检测你修改引导相关界面，请手动Shift + Alt + W 更新数据【我已经帮你选中当前节点了】");

                    Debug.Log("然后更新下 【TutorialConfig.json】");
                    Debug.LogWarning("然后更新下 【TutorialConfig.json】");
                    Debug.LogError("然后更新下 【TutorialConfig.json】");

                }

                isSavingPrefab = false; // 重置标志

                break; // 只处理第一个Prefab
            }
        }

        return paths;
    }
}