using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarProject.Service.SystemOpen;
using StarProjectDef;
using UnityEditor;
using UnityEngine;

public class AutoFixedSystemOpenType
{
    [MenuItem("Tools/Fix/FixSystemOpenTypePrefab")]
    private static void ProcessPrefabs()
    {
        // Assets/Res/UI/HudMenu/Prefab/
        string res = "Assets/Res";
        string resources = "Assets/Resources";
        string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new string[] { $"{res}/UI", $"{resources}/Perfab" });

        foreach (string prefabPath in prefabPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabPath);
            // GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            GameObject tempPrefab = null;
            try
            {
                tempPrefab = PrefabUtility.LoadPrefabContents(path);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"prefab load 有问题, path: {path} , error: {e.Data} ");


                continue;
            }

            if (tempPrefab == null)
            {
                continue;
            }

            SystemItem[] systemItems = tempPrefab.GetComponentsInChildren<SystemItem>(true);

            if (systemItems.Length == 0)
            {
                continue;
            }

            foreach (var systemItem in systemItems)
            {
                systemItem.systemTypeName = systemItem.systemType.ToString();

                // try
                // {
                //     SystemOpenType1 s1 = (SystemOpenType1)((int)systemItem.systemType);
                //     // 如果 SystemOpenType1 和  SystemOpenType 的 值对应的 key 相同, 皆大欢喜
                //     if (s1.ToString() == systemItem.systemTypeName)
                //     {
                //         Debug.Log($"prefab: {tempPrefab.name}, node: {systemItem.name}, 系统类型相同: {systemItem.systemTypeName}");
                //     }
                //     else
                //     {
                //         Debug.LogError($"prefab: {tempPrefab.name}, node: {systemItem.name}, 系统类型不同: {systemItem.systemTypeName}, cfgEnum类型: {s1.ToString()}, path: {path}");
                //     }

                // }
                // catch (System.Exception)
                // {
                //     Debug.LogError($"prefab: {tempPrefab.name}, node: {systemItem.name}, 强转失败, 系统类型不同: {systemItem.systemTypeName}, path: {path} ");
                // }
            }


            try
            {

                PrefabUtility.SaveAsPrefabAsset(tempPrefab, path);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"prefab: {tempPrefab.name}, prefab 保存失败 ");
            }

            PrefabUtility.UnloadPrefabContents(tempPrefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}
