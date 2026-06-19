using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering.Universal;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;
using static UnityEngine.UI.Image;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
//[RequireComponent(typeof(Camera))]
public partial class CustomShadowBase : MonoBehaviour
{
    public enum ShadowMapSize 
    {
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
    }
    public ShadowMapSize _DynamicShadowMapSize = ShadowMapSize._1024;
    public ShadowMapSize _StaticShadowMapSize = ShadowMapSize._1024;

    [SerializeField]
    protected LayerMask m_StaticLayer = ~(1 << 10 | 1 << 31);
    [SerializeField]
    protected LayerMask m_DynamicLayer = 1 << 10 | 1 << 31;
    public int GetDynamicShadowMapSize() { return (int)_DynamicShadowMapSize; }
    public int GetStaticShadowMapSize() { return (int)_StaticShadowMapSize; }

    public void SetShadowMapSize(int dynamicSize, int staticSize)
    {
        _DynamicShadowMapSize = (ShadowMapSize)dynamicSize;
        _StaticShadowMapSize = (ShadowMapSize)staticSize;
        if (null != _renderer)
            _SyncAttr_DepthMapDimens();
    }

    [Range(0f, 3.0f)]
    public float _shadowStrength = 0.8f;
    
    protected Camera _projector;
    protected Light _light;
    /*public*/
    protected ScriptableRendererData MainCameraRendererData;
    protected UniversalRenderer _renderer;
    protected Matrix4x4 _preCasterMatrix;
    [HideInInspector]
    protected bool _inited = false;

    public bool IsInited()
    {
        return _inited;
    }

    public void SyncStaticShadowLayer(int faceID)
    {
        CustomShadowMgr.Instance.SyncStaticLayer(_light, faceID, m_StaticLayer);
    }

    public void SyncDynamicShadowLayer(int faceID)
    {
        CustomShadowMgr.Instance.SyncDynamicLayer(_light, faceID, m_DynamicLayer);
    }

    protected void _BuildDirectionalShadowProjectionMatrix(float orthSize,float near,float far,ref Matrix4x4 proj)
    {
        float halfSize = orthSize/2;// ShadowDistance;
        Matrix4x4 matProjDynamic = Matrix4x4.Ortho(
            -halfSize,
            halfSize,
            -halfSize,
            halfSize,
            near,
            far);
        //if (SystemInfo.usesReversedZBuffer)
        //{
        //    matProjDynamic.m20 = -matProjDynamic.m20;
        //    matProjDynamic.m21 = -matProjDynamic.m21;
        //    matProjDynamic.m22 = -matProjDynamic.m22;
        //    matProjDynamic.m23 = -matProjDynamic.m23;
        //}
        proj = GL.GetGPUProjectionMatrix(matProjDynamic, true);
    }
    protected void _BuildSpotShadowProjectionMatrix(ref Matrix4x4 proj)
    {
        //proj = GL.GetGPUProjectionMatrix(_projector.projectionMatrix, true);
        //return;
        Matrix4x4 matProjDynamic = Matrix4x4.Perspective(
        _light.spotAngle,
        1.0f,
        0.01f,
        _light.range);
        proj = GL.GetGPUProjectionMatrix(matProjDynamic, true);
    }
    protected void _BuildPointShadowProjectionMatrix(ref Matrix4x4 proj)
    {
        //proj = GL.GetGPUProjectionMatrix(_projector.projectionMatrix, true);
        //return;
        Matrix4x4 matProjDynamic = Matrix4x4.Perspective(
            90.0f,
            1.0f,
            0.01f,
            _light.range);
        proj = GL.GetGPUProjectionMatrix(matProjDynamic, true);
    }
    protected void _BuildViewMatrix(Vector3 pos, Quaternion rot, ref Matrix4x4 mat)
    {
        //mat = _projector.worldToCameraMatrix;
        //return;
        mat = Matrix4x4.TRS(pos, rot, Vector3.one).inverse;
        //if (SystemInfo.usesReversedZBuffer)
        {
            mat.m20 = -mat.m20;
            mat.m21 = -mat.m21;
            mat.m22 = -mat.m22;
            mat.m23 = -mat.m23;
        }
    }
    protected virtual void _SyncAttr_ShadowCamera()
    {
        CustomShadowMgr.Instance.SetShadowCamera(_light, 0, _projector);
    }

    protected void DetermineRenderer(int index)
    {
        ScriptableRendererData[] rendererDataList =
            (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField(
                "m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(UniversalRenderPipeline.asset);
        //int defaultRendererIndex = 
        //    (int)typeof(UniversalRenderPipelineAsset).GetField(
        //        "m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(UniversalRenderPipeline.asset);
        //ScriptableRendererData forwardRenderer = rendererDataList[defaultRendererIndex];
        MainCameraRendererData = rendererDataList[0];
        //
        //MainCamera = Camera.main;
        //var _addMainCamData = MainCamera.GetUniversalAdditionalCameraData();

        //_renderer = _addMainCamData.scriptableRenderer as ForwardRenderer;
        //
        if (null == _projector)
        {
            //GameObject go = new GameObject("__SpotCamera", typeof(Camera));
            _projector = GetComponent<Camera>();
        }
        //
        var _addCamData = _projector.GetUniversalAdditionalCameraData();
        _addCamData.renderPostProcessing = false;
        _addCamData.antialiasing = AntialiasingMode.None;
        _addCamData.stopNaN = false;
        _addCamData.dithering = false;
        _addCamData.renderShadows = false;
        _addCamData.requiresColorTexture = false;
        _addCamData.requiresDepthTexture = false;
        _addCamData.volumeLayerMask = LayerMask.GetMask("Nothing");
        _addCamData.SetRenderer(0);
        _renderer = _addCamData.scriptableRenderer as UniversalRenderer;
#if ZGame
        _addCamData.SetRenderer(4);//No Feature Renderer;// UniversalRenderPipeline.asset.render);
#endif
        //
        if (null == _light)
        {
            _light = GetComponent<Light>();
            //_light.type = LightType.Spot;
        }
        _light.shadows = LightShadows.None;
        //
        // Apply texture scale and offset to save a MAD in shader.
        var textureScaleAndBias = Matrix4x4.identity;
        if(SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore|| 
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2|| 
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
        {
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
        }
        else
        {
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            //textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            //textureScaleAndBias.m23 = 0.5f;
        }

        var flipY = Matrix4x4.identity;
        if (SystemInfo.graphicsUVStartsAtTop)
        {
            flipY.m00 = 1.0f;
            flipY.m11 = -1.0f;
            flipY.m22 = 1.0f;
            flipY.m03 = 0.0f;
            flipY.m13 = 1.0f;
            flipY.m23 = 0.0f;
        }
        _preCasterMatrix = flipY * textureScaleAndBias;
    }
    protected virtual void _AddShadow(int faceID = -1)
    {
        CustomShadowMgr.Instance.AddShadow(_light, faceID, null);
    }

    protected virtual void _RmvShadow()
    {
        if (_light != null)
        {
            CustomShadowMgr.Instance.RmvShadow(_light);
        }
    }
    protected virtual void OnEnable()
    {
        InitProjector();
        StartCoroutine(_DelayInit());
    }
    protected virtual void OnDisable()
    {
        _RmvShadow();
        _inited = false;
#if UNITY_EDITOR
        DisableEditor();
#endif
    }
    //protected virtual void OnDestroy()
    //{
    //    _RmvShadow();
    //    _inited = false;
    //}
    /// <summary>
    /// 不存在SceneLightingComponent时进行的初始化，因此要等两帧检查是否已经被SceneLightingComponent初始化了
    /// </summary>
    /// <returns></returns>
    private IEnumerator _DelayInit()
    {
        // 等待多场景Enable/Disable完成
        yield return null;
        // 场景有SceneLightingComponent的话，会在此时调用Init
        //yield return null;
        // 所以在这帧之后阴影没有被初始化，说明场景没有场景有SceneLightingComponent的话，则自己进行Init
        //yield return null;
        
        Init();
    }
    /// <summary>
    /// 初始化调用接口
    /// </summary>
    public void Init()
    {
        if (_inited)
            return;
        
        /*
        if (PerformanceManager.Instance != null && !PerformanceManager.Instance.GetEnableShadow())
            return;
        */
        _DoInit();
        _inited = true;
    }
    /// <summary>
    /// 初始化具体实现
    /// </summary>
    protected virtual void _DoInit()
    {
    }
    public virtual void CaptureStaticLayer()
    {
        if (null != _renderer)
            _SyncAttr_CaptureStaticLayer();
    }
    public virtual void _SyncAttr_CaptureStaticLayer()
    {
        Debug.AssertFormat(false , "Need Impl _SyncAttr_CaptureStaticLayer!!!");
    }
    public void _SyncAttr_Inited()
    {
        CustomShadowMgr.Instance.InitedShadow(_light, 0);
    }
    public virtual void CopyShadowData(CustomShadowBase shadow)
    {
#if ZGame
        if (PerformanceManager.Instance != null)
        {
            _DynamicShadowMapSize = (ShadowMapSize)Mathf.Min((int)shadow._DynamicShadowMapSize, PerformanceManager.Instance.GetShadowMapDynamicSize());
            _StaticShadowMapSize = (ShadowMapSize)Mathf.Min((int)shadow._StaticShadowMapSize, PerformanceManager.Instance.GetShadowMapStaticSize());            
        }
        else
        {
            _DynamicShadowMapSize = shadow._DynamicShadowMapSize;
            _StaticShadowMapSize = shadow._StaticShadowMapSize;            
        }

        _shadowStrength = shadow._shadowStrength;
        //protected Camera _projector;
        //protected Light _light;
        /*public*/
        //protected ScriptableRendererData MainCameraRendererData;
        //protected ForwardRenderer _renderer;
        //_preCasterMatrix = shadow._preCasterMatrix;
        m_StaticLayer = shadow.m_StaticLayer;
        m_DynamicLayer = shadow.m_DynamicLayer;
#endif
    }

    protected virtual void _SyncAttr_DepthMapDimens() { }

    public virtual void SetShadowOn(bool b) { }

    void InitProjector()
    {
        if (_projector != null)
            return;

        _projector = GetComponentInChildren<Camera>();
        if (_projector != null)
            return;

        var son = new GameObject("shadowCamera");
        _projector = son.AddComponent<Camera>();
        son.transform.parent = this.transform;
        son.transform.localRotation = Quaternion.identity;
        son.transform.localPosition = Vector3.zero;
    }
}
