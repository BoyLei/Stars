using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GameDLL.Hdg;
using SGF;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using StarProject.Service.Cam;
using Sirenix.OdinInspector;


public class PlanarReflectionDC : MonoBehaviour
{
    [LabelText("哪些Layer需要被反射")]
    public LayerMask _reflectionMask = -1;
    [LabelText("是否反射天空盒")]
    public bool _reflectSkybox = false;
    [LabelText("反射物件的纵向位移")]
    public float _clipPlaneOffset = 0.07F;
    //反射图属性名
    const string _reflectionTex = "_ReflectionTex";
    Camera _reflectionCamera;
    private UniversalAdditionalCameraData _reflectionCameraRendererData;
    Vector3 _oldpos;
    RenderTexture _bluredReflectionTexture;
    Material _sharedMaterial;
    [LabelText("是否开启模糊(开启增加性能损耗)")]
    public bool _blurOn = true;
    [LabelText("模糊大小")]
    [Range(0.0f, 5.0f)]
    public float _blurSize = 1;
    [LabelText("模糊迭代次数")]
    [Range(0, 3)]
    public int _blurIterations = 1;
    [LabelText("模糊贴图的降采样程度")]
    [Range(1.0f, 4.0f)]
    public float _downsample = 1.5f;
    private bool _oldBlurOn;
    private float _oldBlurSize;
    private int _oldBlurIterations;
    private float _oldDownsample;
    private Shader _blurShader;
    private Material _blurMaterial;
    private static bool _insideRendering;
    public Camera mainCamera;
    private static SGRenderingPaths planarReflectionRenderDataIndex = SGRenderingPaths.PlanarReflectionRenderData;
    private static string reflectionCameraName = "SGAMEReflectionPlaneCamera";
    private static string reflectionCameraTag = "PlanarReflectionCamera";
    GameObject reflectCamObj;

    Material BlurMaterial
    {
        get
        {
            if (_blurMaterial == null)
            {
                _blurMaterial = new Material(_blurShader);
                return _blurMaterial;
            }
            return _blurMaterial;
        }
    }


    private void OnEnable()
    {
        if (mainCamera == null)
        {
            Debug.LogError("相机为空，请检查相机是否被正确赋值");
            return;
        }
        RenderPipelineManager.beginCameraRendering += ReflectionCameraRendering;
    }

    // private void OnDisable()
    // {
    //     RenderPipelineManager.beginCameraRendering -= ReflectionCameraRendering;
    // }

    private void OnValidate()
    {
        setReflectionCameraRenderFeature(_reflectionCamera);
    }

    void Awake()
    {
        _oldBlurOn = _blurOn;
        _oldBlurSize = _blurSize;
        _oldBlurIterations = _blurIterations;
        _oldDownsample = _downsample;

        setReflectionCameraRenderFeature(_reflectionCamera);

    }

    void Start()
    {
        _sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        _blurShader = Shader.Find("Hidden/KawaseBlur");
        if (_blurShader == null)
            Debug.LogError("缺少Hidden/KawaseBlur Shader");
    }

    bool _blurParamChanged;
    void Update()
    {
        if (_blurParamChanged)
        {
            _oldBlurOn = _blurOn;
            _oldBlurSize = _blurSize;
            _oldBlurIterations = _blurIterations;
            _oldDownsample = _downsample;
        }

        if (_blurOn != _oldBlurOn || _blurSize != _oldBlurSize || _blurIterations != _oldBlurIterations || _downsample != _oldDownsample)
        {
            _blurParamChanged = true;
        }
    }
    Camera CreateReflectionCamera(Camera cam)
    {
        String reflName = gameObject.name + "Reflection" + cam.name;
        GameObject go = new GameObject(reflName);
        go.hideFlags = HideFlags.DontSave;
        Camera reflectCamera = go.AddComponent<Camera>();
        HoldCameraSettings(reflectCamera);
        if (!reflectCamera.targetTexture)
        {
            reflectCamera.targetTexture = CreateTexture(cam);
        }

        return reflectCamera;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= ReflectionCameraRendering;

        if (_reflectionCamera != null)
        {
            var rt = _reflectionCamera.targetTexture;
            if (rt != null)
            {
                rt.Release();
                Destroy(rt);
                rt = null;
            }
            // var cameraData = _reflectionCamera.GetUniversalAdditionalCameraData();
            // if(cameraData != null)
            // {
            //     Destroy(cameraData);
            //     cameraData= null;
            // }
            // Destroy(_reflectionCamera);
            // _reflectionCamera = null;

            GameObject.DestroyImmediate(_reflectionCamera.gameObject);
        }
    }

    void HoldCameraSettings(Camera heplerCam)
    {
        heplerCam.backgroundColor = Color.black;
        heplerCam.clearFlags = _reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        heplerCam.renderingPath = RenderingPath.Forward;
        heplerCam.cullingMask = _reflectionMask;
        heplerCam.allowMSAA = false;
        heplerCam.enabled = false;
        heplerCam.tag = reflectionCameraTag;
    }
    RenderTexture CreateTexture(Camera sourceCam)
    {
        int width = Mathf.RoundToInt(sourceCam.pixelWidth / _downsample);
        int height = Mathf.RoundToInt(sourceCam.pixelHeight / _downsample);
        RenderTextureFormat formatRT = sourceCam.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        RenderTexture rt = new RenderTexture(width, height, 24, formatRT);
        rt.name = "ReflectionPlaneRT";
        rt.hideFlags = HideFlags.DontSave;
        return rt;
    }

    void ReflectionCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        Camera currentCam = mainCamera;
        if (camera.name == "SceneCamera")
        {
            currentCam = camera;
        }

        _insideRendering = true;
        if (_reflectionCamera == null)
        {
            reflectCamObj = GameObject.FindWithTag(reflectionCameraTag);
            if (reflectCamObj == null)
            {
                _reflectionCamera = CreateReflectionCamera(currentCam);
            }
            else
            {
                if (reflectCamObj.GetComponent<Camera>() != null)
                {
                    reflectCamObj.GetComponent<Camera>().targetTexture.DiscardContents();
                }
                DestroyImmediate(reflectCamObj);
                _reflectionCamera = CreateReflectionCamera(currentCam);
            }
        }

        if (_reflectionCamera != null)
        {
            SetCameraData(_reflectionCameraRendererData);
            if (_reflectionCamera && _sharedMaterial)
            {
                if (_blurOn)
                {
                    RenderReflection(context, currentCam, _reflectionCamera);
                    _sharedMaterial.SetTexture(_reflectionTex, _reflectionCamera.targetTexture);
                }
                else
                {
                    RenderReflection(context, currentCam, _reflectionCamera);
                    _sharedMaterial.SetTexture(_reflectionTex, _reflectionCamera.targetTexture);
                }

            }
            _insideRendering = false;
        }

    }
    void SetCameraData(UniversalAdditionalCameraData reflectionCameraRendererData)
    {
        if (reflectionCameraRendererData != null)
        {
            reflectionCameraRendererData.SetRenderer((int)planarReflectionRenderDataIndex);
            reflectionCameraRendererData.requiresColorOption = CameraOverrideOption.Off;
            reflectionCameraRendererData.requiresDepthOption = CameraOverrideOption.Off;
            reflectionCameraRendererData.renderShadows = false;
        }
        else
        {
            reflectionCameraRendererData = _reflectionCamera.GetUniversalAdditionalCameraData();
            if (reflectionCameraRendererData != null)
            {
                reflectionCameraRendererData.SetRenderer((int)planarReflectionRenderDataIndex);
                reflectionCameraRendererData.requiresColorOption = CameraOverrideOption.Off;
                reflectionCameraRendererData.requiresDepthOption = CameraOverrideOption.Off;
                reflectionCameraRendererData.renderShadows = false;
            }
        }
    }

    void RenderReflection(ScriptableRenderContext context, Camera currentCam, Camera reflectCamera)
    {
        if (reflectCamera == null)
        {
            Debug.LogError("反射Camera无效");
            return;
        }
        if (_sharedMaterial && !_sharedMaterial.HasProperty(_reflectionTex))
        {
            Debug.LogError("Shader中缺少_ReflectionTex属性");
            return;
        }
        HoldCameraSettings(reflectCamera);

        if (_reflectSkybox)
        {
            if (currentCam.gameObject.GetComponent(typeof(Skybox)))
            {
                Skybox sb = (Skybox)reflectCamera.gameObject.GetComponent(typeof(Skybox));
                if (!sb)
                {
                    sb = (Skybox)reflectCamera.gameObject.AddComponent(typeof(Skybox));
                }
                sb.material = ((Skybox)currentCam.GetComponent(typeof(Skybox))).material;
            }
        }

        bool isInvertCulling = GL.invertCulling;
        GL.invertCulling = true;

        Transform reflectiveSurface = this.transform; //waterHeight;

        Vector3 eulerA = currentCam.transform.eulerAngles;

        reflectCamera.transform.eulerAngles = new Vector3(-eulerA.x, eulerA.y, eulerA.z);
        reflectCamera.transform.position = currentCam.transform.position;

        Vector3 pos = reflectiveSurface.transform.position;
        pos.y = reflectiveSurface.position.y;
        Vector3 normal = reflectiveSurface.transform.up;
        float d = -Vector3.Dot(normal, pos) - _clipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        reflection = CalculateReflectionMatrix(reflection, reflectionPlane);
        _oldpos = currentCam.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(_oldpos);

        reflectCamera.worldToCameraMatrix = currentCam.worldToCameraMatrix * reflection;

        Vector4 clipPlane = CameraSpacePlane(reflectCamera, pos, normal, 1.0f);

        Matrix4x4 projection = currentCam.projectionMatrix;
        projection = CalculateObliqueMatrix(projection, clipPlane);
        reflectCamera.projectionMatrix = projection;

        reflectCamera.transform.position = newpos;
        Vector3 euler = currentCam.transform.eulerAngles;
        reflectCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

        //reflectCamera.Render();
        UniversalRenderPipeline.RenderSingleCamera(context, reflectCamera);
        GL.invertCulling = isInvertCulling;
    }

    static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4(
            Mathf.Sign(clipPlane.x),
            Mathf.Sign(clipPlane.y),
            1.0F,
            1.0F
            );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];

        return projection;
    }

    void setReflectionCameraRenderFeature(Camera ReflectionCamera)
    {
        if (ReflectionCamera != null)
        {
            var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer((int)planarReflectionRenderDataIndex);
            var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
            List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

            foreach (var feature in features)
            {
                if (feature.GetType() == typeof(SGameBlurRF))
                {
                    (feature as SGameBlurRF)._blurIterations = _blurIterations;
                    (feature as SGameBlurRF)._blurSize = _blurSize;
                    (feature as SGameBlurRF)._downsample = _downsample;
                    (feature as SGameBlurRF)._blurOn = _blurOn;
                }
            }
        }
    }

    static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1.0F - 2.0F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2.0F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2.0F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2.0F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2.0F * plane[1] * plane[0]);
        reflectionMat.m11 = (1.0F - 2.0F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2.0F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2.0F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2.0F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2.0F * plane[2] * plane[1]);
        reflectionMat.m22 = (1.0F - 2.0F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2.0F * plane[3] * plane[2]);

        reflectionMat.m30 = 0.0F;
        reflectionMat.m31 = 0.0F;
        reflectionMat.m32 = 0.0F;
        reflectionMat.m33 = 1.0F;

        return reflectionMat;
    }

    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * _clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private Dictionary<Camera, CommandBuffer> _cameras = new Dictionary<Camera, CommandBuffer>();
    void PostProcessTexture(ScriptableRenderContext context, Camera cam, RenderTexture source, RenderTexture dest)
    {
        if (_blurParamChanged)
        {
            if (_cameras.ContainsKey(cam))
                cam.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _cameras[cam]);
            _cameras.Remove(cam);
        }
        if (_cameras.ContainsKey(cam))
            return;

        CommandBuffer buf = CommandBufferPool.Get("Blur Reflection Texture");
        _cameras[cam] = buf;
        float width = source.width;
        float height = source.height;
        int rtW = Mathf.RoundToInt(width / _downsample);
        int rtH = Mathf.RoundToInt(height / _downsample);

        int blurredID = Shader.PropertyToID("_Temp1");
        int blurredID2 = Shader.PropertyToID("_Temp2");
        buf.GetTemporaryRT(blurredID, rtW, rtH, 0, FilterMode.Bilinear, source.format);
        buf.GetTemporaryRT(blurredID2, rtW, rtH, 0, FilterMode.Bilinear, source.format);

        buf.Blit((Texture)source, blurredID);
        for (int i = 0; i < _blurIterations; i++)
        {
            float iterationOffs = (i * 1.0f);
            buf.SetGlobalFloat("_Offset", iterationOffs / _downsample + _blurSize);
            buf.Blit(blurredID, blurredID2, BlurMaterial, 0);
            buf.Blit(blurredID2, blurredID, BlurMaterial, 0);
        }
        buf.Blit(blurredID, dest);

        buf.ReleaseTemporaryRT(blurredID);
        buf.ReleaseTemporaryRT(blurredID2);

        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);

        CommandBufferPool.Release(buf);
    }

}