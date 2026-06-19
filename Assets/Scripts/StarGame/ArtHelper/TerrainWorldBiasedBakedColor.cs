using UnityEngine;

[ExecuteInEditMode]
public class TerrainWorldBiasedBakedColor : MonoBehaviour
{
    [Header("全局光照贴图")]
    public Texture2D WorldBiasedLightMap = null;
    [Header("地表模型")]
    public Renderer TheMeshRenderer = null;
    //public Material GrassMat =null;

    [Header("光照增强参数")]
    [Tooltip("光照整体强度(默认1)")]
    public float GlobalStrength = 1f;
    [Tooltip("光照增强的阈值(默认0.45)")]
    public float SaturateThreshold = 0.45f;
    [Tooltip("光照增强的强度(默认0.6)")]
    public float SaturationIntensity = 0.6f;
    [Tooltip("光照阈值")]
    private static float Threshold = 0.6f;
    [Tooltip("光照阈值膝点")]
    private static float ThresholdKnee = 0.3f;


    private static string Keyword = "TERRAIN_LIGHTMAP_SAMPLE";

    private static string addtionLKeyWord = "_ADDITIONAL_LIGHTS";

    void Awake()
    {
        Flush();
    }

    private void OnEnable()
    {
        Flush();
    }

    private void OnDisable()
    {
        Shader.DisableKeyword(Keyword);
        Shader.EnableKeyword(addtionLKeyWord);
    }

    private void OnValidate()
    {
        Flush();
    }

    void Flush()
    {
        if (TheMeshRenderer)
        {
            Bounds bounds = TheMeshRenderer.bounds;

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            float minWorldX = min.x;
            float maxWorldX = max.x;
            float minWorldZ = min.z;
            float maxWorldZ = max.z;


            // 传递边界的最小和最大值而不是中心和大小
            Shader.SetGlobalVector("_TerrainWorldInfoMin", new Vector4(minWorldX, minWorldZ, 0, 0));
            Shader.SetGlobalVector("_TerrainWorldInfoMax", new Vector4(maxWorldX, maxWorldZ, 0, 0));
            Shader.SetGlobalTexture("_TerrainWorldLightmap", WorldBiasedLightMap);

            Shader.SetGlobalVector("_TerrainWorldInfo", new Vector4(bounds.center.x, bounds.center.z, 0.5f * bounds.size.x, 0.5f * bounds.size.z));

            // GrassMat.SetTexture("_TerrainWorldLightmap", WorldBiasedLightMap);
            // GrassMat.SetVector("_TerrainWorldInfo", new Vector4(bounds.center.x, bounds.center.z, 0.5f * bounds.size.x, 0.5f * bounds.size.z));
            Shader.EnableKeyword(Keyword);

            RefreshBloom();

            /* shader里面采样用这个.
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float2 horizontalMapping = (worldPos.xz - _TerrainWorldInfo.xy) / _TerrainWorldInfo.zw;
            */
        }
    }

    //刷新lightmap光照强化参数
    void RefreshBloom()
    {
        //设置LightMap强化相关函数
        Shader.SetGlobalFloat("_TerrainLM_SaturateThreshold", SaturateThreshold);
        Shader.SetGlobalFloat("_TerrainLM_SaturationIntensity", SaturationIntensity);
        Shader.SetGlobalFloat("_TerrainLM_Threshold", Threshold);
        Shader.SetGlobalFloat("_TerrainLM_ThresholdKnee", ThresholdKnee);
        Shader.SetGlobalFloat("_TerrainLM_Strength", GlobalStrength);
    }

#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        Flush();
    }
#endif
}
