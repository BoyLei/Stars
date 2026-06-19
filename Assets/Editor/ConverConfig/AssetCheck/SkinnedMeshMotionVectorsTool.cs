using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class SkinnedMeshMotionVectorsTool : EditorWindow
{
    private string[] prefabPaths;
    private int currentPrefabIndex;
    private bool isModifying;

    [MenuItem("自动化工具/Skinned Mesh Motion Vectors Tool")]
    private static void Init()
    {
        SkinnedMeshMotionVectorsTool window = (SkinnedMeshMotionVectorsTool)EditorWindow.GetWindow(typeof(SkinnedMeshMotionVectorsTool));
        window.Show();
    }
    private string prefabsInput;
    private void OnGUI()
    {
        GUILayout.Label("Input Prefab Paths (One per line)");
        prefabsInput = EditorGUILayout.TextArea(prefabsInput, GUILayout.Height(200));

        if (GUILayout.Button("Apply Changes"))
        {
            ApplyChanges(prefabsInput);
        }
    }

    private void ApplyChanges(string prefabsInput)
    {
        Debug.LogError(prefabsInput);
        string input = prefabsInput;
        string pattern = @"Assets.*?\.prefab";

        MatchCollection matches = Regex.Matches(input, pattern);
        int index = 0;
        prefabPaths = new string[matches.Count];
        foreach (Match match in matches)
        {
            prefabPaths[index] = match.Value;
            index++;
            Debug.Log(match.Value);
            ProcessPrefabs(match.Value);
        }



      /*  isModifying = true;
        EditorApplication.update += ProcessPrefabs;*/
    }

    private void ProcessPrefabs(string value)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(value);

        if (prefab != null)
        {
            SkinnedMeshRenderer[] skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
            {
                renderer.skinnedMotionVectors = false;
            }

            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
        }
    }
}

