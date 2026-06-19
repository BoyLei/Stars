using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Diagnostics;



#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
#endif

[Serializable]
public class SGameMeshRenderLightData
{
    public Vector4 LightmapOffsetScale = Vector4.zero;
    public int LightmapIndex = -1;
    public MeshRenderer MeshRenderer;

}


[ExecuteInEditMode]
public class SGameSceneLightmapData : MonoBehaviour
{
#if UNITY_EDITOR
    //public LightingDataAsset LightingDataAsset;
#endif

    public string SceneName = string.Empty;
    public List<SGameMeshRenderLightData> MeshRenderLightDatas = new List<SGameMeshRenderLightData>();
    public RenderSettings SavedRenderSettings;

    public bool Fog;
    public float FogStartDistance;
    public float FogEndDistance;
    public UnityEngine.FogMode FogMode;
    public Color FogColor;
    public float FogDensity;

    public AmbientMode AmbientMode;
    public Color AmbientSkyColor;
    public Color AmbientEquatorColor;
    public Color AmbientGroundColor;
    public float AmbientIntensity;
    public Color AmbientLight;

    public Color SubtractiveShadowColor;
    public Material Skybox;
    public Light Sun;
    public SphericalHarmonicsL2 AmbientProbe;

    public Texture CustomReflection;
    public float ReflectionIntensity;
    public int ReflectionBounces;
    public Cubemap DefaultReflection;
    public DefaultReflectionMode DefaultReflectionMode;
    public int DefaultReflectionResolution;

    private string AdditionalLightKeyword = "_ADDITIONAL_LIGHTS";

    public string Json_SourceLightmapSettings = string.Empty;
    public string Json_SourceRenderSettings = string.Empty;


    ///////////////////////////////////////////////////////////////    
#if UNITY_EDITOR

    void CopyInternal(SerializedObject source, SerializedObject dest)
    {
        var prop = source.GetIterator();
        while (prop.Next(true))
        {
            var copyProperty = true;
            foreach (var propertyName in new[] { "m_Sun", "m_FileID", "m_PathID", "m_ObjectHideFlags" })
            {
                if (string.Equals(prop.name, propertyName, System.StringComparison.Ordinal))
                {
                    copyProperty = false;
                    break;
                }
            }

            if (copyProperty)
                dest.CopyFromSerializedProperty(prop);
        }

        dest.ApplyModifiedProperties();
    }

    static bool TryGetSettings(System.Type type, string methodName, out UnityEngine.Object settings)
    {
        settings = null;

        var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
        if (method == null)
        {
            UnityEngine.Debug.LogErrorFormat("CopyLightingSettings: Could not find {0}.{1}", type.Name, methodName);
            return false;
        }

        var value = method.Invoke(null, null) as UnityEngine.Object;
        if (value == null)
        {
            UnityEngine.Debug.LogErrorFormat("CopyLightingSettings: Could get data from {0}.{1}", type.Name, methodName);
            return false;
        }

        settings = value;
        return true;
    }
#endif
    ///////////////////////////////////////////////////////////////    


#if UNITY_EDITOR
    public static void Export(SGameSceneLightmapData root, string sceneName)
    {
        if (!root)
            return;

        UnityEngine.Object lightmapSettings;
        if (!TryGetSettings(typeof(LightmapEditorSettings), "GetLightmapSettings", out lightmapSettings))
            return;

        UnityEngine.Object renderSettings;
        if (!TryGetSettings(typeof(RenderSettings), "GetRenderSettings", out renderSettings))
            return;

        root.Json_SourceLightmapSettings = EditorJsonUtility.ToJson(lightmapSettings);
        root.Json_SourceRenderSettings = EditorJsonUtility.ToJson(renderSettings);



        root.SceneName = sceneName;
        root.MeshRenderLightDatas.Clear();

        var renderers = root.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.lightmapIndex != -1 && renderer.lightmapIndex != 65534)
            {
                var info = new SGameMeshRenderLightData();
                info.MeshRenderer = renderer;
                info.LightmapOffsetScale = renderer.lightmapScaleOffset;
                info.LightmapIndex = renderer.lightmapIndex;
                root.MeshRenderLightDatas.Add(info);
            }
        }

        if (root.MeshRenderLightDatas.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有检查到LightMap的信息,请确认场景是否烘焙了LightMap", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("恭喜", "LightMap信息导出完成", "确定");
        }
    }
#endif

    void Start()
    {
        ApplyToMeshRenderersRunTime();

    }


#if UNITY_EDITOR
    //编辑器下运行时加载LightMap
    public void ApplyToMeshRenderers()
    {
        UnityEngine.Object lightmapSettings;
        if (!TryGetSettings(typeof(LightmapEditorSettings), "GetLightmapSettings", out lightmapSettings))
            return;

        UnityEngine.Object renderSettings;
        if (!TryGetSettings(typeof(RenderSettings), "GetRenderSettings", out renderSettings))
            return;

        EditorJsonUtility.FromJsonOverwrite(Json_SourceLightmapSettings, lightmapSettings);
        EditorJsonUtility.FromJsonOverwrite(Json_SourceRenderSettings, renderSettings);

        CopyInternal(new SerializedObject(lightmapSettings), new SerializedObject(lightmapSettings));

        //var targetRenderSettings = new SerializedObject(renderSettings);
        CopyInternal(new SerializedObject(renderSettings), new SerializedObject(renderSettings));

        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();


        // 创建一个 Stopwatch 实例
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Scene currentScene = SceneManager.GetActiveScene();
        // 获取场景的名字
        string sceneName = currentScene.name;
        // 获取场景的路径
        string scenePath = currentScene.path;

        //有光照贴图的信息 且不为美术场景
        if (!sceneName.StartsWith("Art") && !scenePath.Contains("ArtWorkSpace"))
        {
            if (MeshRenderLightDatas.Count != 0)
            {

                foreach (var lightmapData in MeshRenderLightDatas)
                {
                    if (lightmapData.MeshRenderer
                        && lightmapData.LightmapIndex != -1
                        && lightmapData.LightmapIndex != 65534)
                    {
                        lightmapData.MeshRenderer.lightmapIndex = lightmapData.LightmapIndex;
                        lightmapData.MeshRenderer.lightmapScaleOffset = lightmapData.LightmapOffsetScale;
                        //Debug.Log(lightmapData.MeshRenderer.name + " " + lightmapData.MeshRenderer.lightmapIndex + " " + lightmapData.MeshRenderer.lightmapScaleOffset);
                    }
                }

                //关闭场景中所有point light和area light
                Shader.DisableKeyword(AdditionalLightKeyword);
                // 查找场景中所有的灯光对象
                Light[] allLights = FindObjectsOfType<Light>();

                // 遍历每个灯光对象
                foreach (Light light in allLights)
                {
                    // 检查灯光类型
                    if (light.type == LightType.Point || light.type == LightType.Area)
                    {
                        // 关闭灯光
                        light.gameObject.SetActive(false);
                    }
                }
                // 保存当前场景
                if (EditorSceneManager.SaveScene(currentScene))
                {
                    // 重新加载当前场景
                    EditorSceneManager.OpenScene(currentScene.path);
                }
                // EditorUtility.DisplayDialog("恭喜", "LightMap信息加载完成", "确定");
            }
            else
            {
                // EditorUtility.DisplayDialog("导入失败", "Prefab中没有光照贴图数据,请检查场景Prefab是否为最新,地编是否有把光照贴图信息导入至Prefab中!", "中！");
            }
        }
        stopwatch.Stop();
        // 输出执行时间
        UnityEngine.Debug.Log(sceneName + "设置Lightmap数据执行时间：" + stopwatch.ElapsedMilliseconds + " 毫秒");
    }
#endif


    //游戏运行时加载LightMap
    public void ApplyToMeshRenderersRunTime()
    {
        if (LightmapSettings.lightmaps != null && LightmapSettings.lightmaps.Length > 0)
        {
            // 创建一个 Stopwatch 实例
            //Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();
            float startTime = Time.realtimeSinceStartup;


            foreach (var lightmapData in MeshRenderLightDatas)
            {
                if (lightmapData.MeshRenderer
                    && lightmapData.LightmapIndex != -1
                    && lightmapData.LightmapIndex != 65534)
                {
                    lightmapData.MeshRenderer.lightmapIndex = lightmapData.LightmapIndex;
                    lightmapData.MeshRenderer.lightmapScaleOffset = lightmapData.LightmapOffsetScale;
                    //Debug.Log(lightmapData.MeshRenderer.name + " " + lightmapData.MeshRenderer.lightmapIndex + " " + lightmapData.MeshRenderer.lightmapScaleOffset);
                }
            }
            float endTime = Time.realtimeSinceStartup;
            float deltaTime = endTime - startTime;
            // 输出执行时间
            UnityEngine.Debug.Log("当前场景设置Lightmap数据执行时间：" + deltaTime + " 秒");
        }

    }
}

