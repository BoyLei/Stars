using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class LightmapBaker : EditorWindow
{
    private List<GameObject> affectedObjects = new List<GameObject>();
    private Dictionary<GameObject, ShadowCastingMode> originalShadowSettings = new Dictionary<GameObject, ShadowCastingMode>();
    
    [MenuItem("自动化工具/TA工具/光照贴图烘焙工具")]
    public static void ShowWindow()
    {
        GetWindow<LightmapBaker>("Lightmap Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("此工具会记录场景内所有受点光源和面光源影响的物件");
        GUILayout.Label("将他们的ShadowCaster值设置为ON,并开始烘焙,在完成后还原");
        if (GUILayout.Button("Bake Lightmap"))
        {
            BakeLightmap();
        }

        GUILayout.Label("如果在烘焙完之前中断烘焙,则ShadowCaster设置不会还原,需要手动点击此按钮");
        if (GUILayout.Button("还原场景中所有Prefab中的ShadowCast设置"))
        {
            RestoreShadowCastSettings();
        }
    }

    private void BakeLightmap()
    {
        // 查找所有点光源和区域光源
        Light[] allLights = FindObjectsOfType<Light>();
        List<Light> pointLights = new List<Light>();
        List<Light> areaLights = new List<Light>();

        foreach (Light light in allLights)
        {
            if (light.type == LightType.Point)
            {
                pointLights.Add(light);
            }
            else if (light.type == LightType.Rectangle)
            {
                areaLights.Add(light);
            }
        }

        // 查找所有受影响的物体
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        affectedObjects.Clear();
        originalShadowSettings.Clear();

        foreach (GameObject obj in allObjects)
        {
            if (obj.activeInHierarchy && obj.GetComponent<Renderer>() != null && !obj.name.EndsWith("_Shadow"))
            {
                foreach (Light pointLight in pointLights)
                {
                    if (Vector3.Distance(pointLight.transform.position, obj.transform.position) <= pointLight.range)
                    {
                        affectedObjects.Add(obj);
                        break;
                    }
                }

                foreach (Light areaLight in areaLights)
                {
                    Vector3 toObject = obj.transform.position - areaLight.transform.position;
                    Vector3 lightRight = areaLight.transform.right * areaLight.areaSize.x / 2;
                    Vector3 lightUp = areaLight.transform.up * areaLight.areaSize.y / 2;
                    float dotRight = Vector3.Dot(toObject, lightRight.normalized);
                    float dotUp = Vector3.Dot(toObject, lightUp.normalized);
                
                    if (Mathf.Abs(dotRight) <= lightRight.magnitude && Mathf.Abs(dotUp) <= lightUp.magnitude)
                    {
                        affectedObjects.Add(obj);
                        break;
                    }
                }
            }
        }

        // 记录原始的CastShadow参数，并设置为On
        foreach (GameObject obj in affectedObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalShadowSettings[obj] = renderer.shadowCastingMode;
                
            }
        }
        foreach (GameObject obj in affectedObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        // 开始烘焙
        Lightmapping.completed += OnLightmappingCompleted;
        Lightmapping.BakeAsync();
    }

    private void OnLightmappingCompleted()
    {
        // 还原CastShadow参数
        foreach (KeyValuePair<GameObject, ShadowCastingMode> kvp in originalShadowSettings)
        {
            Renderer renderer = kvp.Key.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = kvp.Value;
            }
        }

        // 取消注册事件
        Lightmapping.completed -= OnLightmappingCompleted;

        Debug.Log("Lightmap baking completed and original shadow settings restored.");
    }
    //还原回项目中Prefab的ShadowCast的设置
    public static void RestoreShadowCastSettings()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        List<GameObject> prefabsRestored = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj);
            if (prefab != null)
            {
                Renderer[] sceneRenderers = obj.GetComponentsInChildren<Renderer>();
                Renderer[] prefabRenderers = prefab.GetComponentsInChildren<Renderer>();

                if (sceneRenderers.Length == prefabRenderers.Length)
                {
                    for (int i = 0; i < sceneRenderers.Length; i++)
                    {
                        if (sceneRenderers[i].name == prefabRenderers[i].name)
                        {
                            SerializedObject prefabRendererSO = new SerializedObject(prefabRenderers[i]);
                            SerializedProperty shadowCastingModeProp = prefabRendererSO.FindProperty("m_CastShadows");
                            if (shadowCastingModeProp != null)
                            {
                                sceneRenderers[i].shadowCastingMode = (ShadowCastingMode)shadowCastingModeProp.intValue;
                            }
                        }
                        
                    }
                    prefabsRestored.Add(obj);
                }
            }
        }

        Debug.Log($"Restored shadow casting settings for {prefabsRestored.Count} Prefab instances.");
    }
}