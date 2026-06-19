#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.XR;

public class RefreshAirShadowTools
{
#if UNITY_EDITOR
    [MenuItem("自动化工具/TA工具/RefreshAirShadow", false, 90)]
    static void RefreshAirShadow()
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.Contains("/Res/Map"))
                continue;

            var curScene = EditorSceneManager.OpenScene(assetPath);
            var customShadow = Object.FindObjectsByType<CustomDirectionalShadow>(FindObjectsSortMode.None);
            if (customShadow.Length > 0)
            {
                var airShadow = GameObject.Find("AirShadow");
                if (airShadow != null)
                {
                    var root = GameObject.Find(curScene.name);
                    var renderers = airShadow.GetComponentsInChildren<Renderer>();
                    foreach(var renderer in renderers)
                    {                        
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; 
                    }
                    PrefabUtility.ApplyPrefabInstance(root, InteractionMode.AutomatedAction);
                }
                Debug.LogWarning("=== change scene:" + assetPath + " ===");
            }
        }

        AssetDatabase.Refresh();
    }
#endif
}

