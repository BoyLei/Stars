using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class TextureReferenceViewerWindow : EditorWindow
{
    private Texture2D texture;
    private List<GameObject> referencedPrefabs = new List<GameObject>();

    [MenuItem("自动化工具/Texture Reference Viewer")]
    public static void ShowWindow()
    {
        TextureReferenceViewerWindow window = GetWindow<TextureReferenceViewerWindow>();
        window.titleContent = new GUIContent("Texture Reference Viewer");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Select Texture to Check References", EditorStyles.boldLabel);

        texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false);

        if (GUILayout.Button("Find Prefabs Using Texture"))
        {
            FindPrefabReferences();
        }

        GUILayout.Space(10);

        GUILayout.Label("Prefab References", EditorStyles.boldLabel);

        if (referencedPrefabs.Count == 0)
        {
            EditorGUILayout.HelpBox("No prefab references found.", MessageType.Info);
        }
        else
        {
            foreach (GameObject prefab in referencedPrefabs)
            {
                if (GUILayout.Button(prefab.name))
                {
                    Selection.activeObject = prefab;
                    EditorGUIUtility.PingObject(prefab);
                }
            }
        }
    }

    private void FindPrefabReferences()
    {
        if (texture == null)
        {
            return;
        }

        referencedPrefabs.Clear();

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset is GameObject)
            {
                GameObject prefab = (GameObject)asset;
                if (IsTextureReferencedInPrefab(prefab, texture))
                {
                    referencedPrefabs.Add(prefab);
                }
            }
        }

        // 使用GUID进行查找
        string textureGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(texture));
        foreach (string assetPath in allAssetPaths)
        {
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);
            foreach (string dependency in dependencies)
            {
                string dependencyGuid = AssetDatabase.AssetPathToGUID(dependency);
                if (dependencyGuid == textureGuid)
                {
                    UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (asset is GameObject && !referencedPrefabs.Contains((GameObject)asset))
                    {
                        referencedPrefabs.Add((GameObject)asset);
                    }
                }
            }
        }
    }

    private bool IsTextureReferencedInPrefab(GameObject prefab, Texture2D texture)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material != null && material.mainTexture == texture)
                {
                    return true;
                }
            }
        }

        return false;
    }
}