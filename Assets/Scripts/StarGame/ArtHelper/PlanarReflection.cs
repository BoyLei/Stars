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
using StarProject;



[ExecuteInEditMode]
public class PlanarReflection : MonoBehaviour {
    [LabelText("哪些Layer需要被反射")]
    public LayerMask _reflectionMask = -1;
    [LabelText("是否反射天空盒")]
    public bool _reflectSkybox = false;
    [LabelText("反射物件的纵向位移")]
    public float _clipPlaneOffset = 0.00F;
    //反射图属性名
    const string _reflectionTex = "_ReflectionTex";
    const string _reflectionTex2 = "_ReflectionTex2";
    Camera _reflectionCamera;
    Camera _reflectionCamera2;
    private UniversalAdditionalCameraData _reflectionCameraRendererData;
    Vector3 _oldpos;
    RenderTexture _bluredReflectionTexture;
    Material _sharedMaterial;
    //模糊效果相关参数
    [LabelText("是否开启模糊(开启增加性能损耗)")]
    public bool _blurOn = true;
    [LabelText("模糊大小")]
    [Range(0.0f, 5.0f)]
    public float _blurSize = 1;
    [LabelText("模糊迭代次数 值越小性能损失越大")]
    [Range(0, 3)]
    public int _blurIterations = 1;
    [LabelText("模糊贴图的降采样程度 值越大性能损耗越小")]
    [Range(1.0f, 5.0f)]
    public int _downsample = 2;
    
    [Title("第二层反射图")]
    [LabelText("SecondLayer")]
    [ToggleLeft]
    public bool SecondLayer = false;

    
    [ShowIf("SecondLayer")] 
    [LabelText("哪些Layer需要被反射")]
    public LayerMask _reflectionMask2 = -1;
    //记录上述模糊参数，用于判断参数是否发生变化   
    private bool _oldBlurOn;
    private float _oldBlurSize;
    private int _oldBlurIterations;
    private float _oldDownsample;
    //模糊shader
    private Shader _blurShader;
    private Material _blurMaterial;
    //用来判断当前是否正在渲染反射图
    private static bool _insideRendering;
    //当前场景内主相机
    //private static string mainCameraTag = "MainCamera";
    private Camera mainCamera;
    //存储主相机
    private Camera mainCameraTemp;
    //反射的RendererData的Index
    private static SGRenderingPaths planarReflectionRenderDataIndex = SGRenderingPaths.PlanarReflectionRenderData;
    //CameraName
    private static string reflectionCameraName = "SGAMEReflectionPlaneCamera";
    private static string reflectionCameraTag = "PlanarReflectionCamera";
    //ReflectionCamObj
    GameObject reflectCamObj;
    //定义第一层反射和第二层反射的标记
    private static bool FIRSTLAYER = false;
    private static bool SECONDLAYER = true;

    Material BlurMaterial {
        get {
            if (_blurMaterial == null) {
                _blurMaterial = new Material(_blurShader);
                return _blurMaterial;
            }
            return _blurMaterial;
        }
    }

    
    private void OnEnable()
    {
        //mainCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;

        // GameObject[] mainCameraTagObjs = GameObject.FindGameObjectsWithTag(mainCameraTag);
        // foreach (var VARIABLE in mainCameraTagObjs)
        // {
        //     mainCamera =  VARIABLE.GetComponent<Camera>();
        // }

        //GlobalEvent.OnCameraAwake.AddListener(MainCameraAwakeInvoke);
        
        if(CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam)!=null)
        {
            mainCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;
        }
        if (mainCamera == null)
        {
            if (Camera.main!=null && Camera.main.tag == "MainCamera" && Camera.main.name != "UICamera")
            {
                mainCamera = Camera.main;
            }   
            //mainCameraTemp = Camera.main;
        }
        
        if(Camera.main!=null && Camera.main.tag== "MainCamera" && Camera.main.name != "UICamera")
        {
            mainCameraTemp = Camera.main;
        }
       
       
        if (mainCameraTemp == null || mainCamera == null)
        {
            Debug.Log("场景内没有MainCamera, 请检查MainCamera的Tag是否为MainCamera");
            
        }
        else
        {
            //初始化相机设置
                  //  Camera currentCam = Camera.current;
#if UNITY_EDITOR
        //在编辑器模式下, 需要区分当前的相机是主相机还是Scene相机
        if (Camera.current == Camera.main)
        {
           mainCamera = mainCameraTemp;
           //Debug.Log("相同!Camera.current的名字叫:"+Camera.current.name + "Camera.main的名字叫"+ Camera.main.name);
        }
        else if (Camera.current!=null && Camera.current.name == "Preview Camera")
        {
            return;
        }
        else
        {
            mainCamera = mainCameraTemp;
            //Preview Camera  SceneCamera
            if (Camera.current != null)
            {
                //在渲染场景camera
                mainCamera = SceneView.lastActiveSceneView.camera;
                //Debug.Log("不同!Camera.current的名字叫:"+Camera.current.name + "Camera.main的名字叫"+ Camera.main.name);
            }
           
        }
#endif
        
        Camera currentCam = mainCameraTemp;
        if (currentCam == null) {
            return;
        }

// #if !UNITY_EDITOR
//         if (!currentCam.gameObject.CompareTag("MainCamera"))
//             return;
// #endif

        // if (_insideRendering) {
        //     return;
        // }
        _insideRendering = true;

        if (_reflectionCamera == null)
        {
           // _reflectionCamera = CreateReflectionCamera(currentCam);
            reflectCamObj = GameObject.FindWithTag(reflectionCameraTag);
             if (reflectCamObj == null)
             {
                 _reflectionCamera = CreateReflectionCamera(mainCameraTemp,FIRSTLAYER);
                 if(SecondLayer)
                    _reflectionCamera2 = CreateReflectionCamera(mainCameraTemp,SECONDLAYER);
                 //Debug.Log("找不到反射相机");
             }
            else 
             {
                 //Debug.Log("找到相机,名字为: "+ reflectCamObj.name);
                 if (reflectCamObj.GetComponent<Camera>() != null)
                 {
                    reflectCamObj.GetComponent<Camera>().targetTexture.DiscardContents(); 
                 } 
                 DestroyImmediate(reflectCamObj);
                 _reflectionCamera = CreateReflectionCamera(mainCameraTemp,FIRSTLAYER);
                 if(SecondLayer)
                     _reflectionCamera2 = CreateReflectionCamera(mainCameraTemp,SECONDLAYER);
            }
        }
            //相机渲染加入队列
            RenderPipelineManager.beginCameraRendering += ReflectionCameraRendering;
        }
       
    }


    bool IsObjectInViewFrustum(Camera cam, GameObject obj)
    {
        // 获取相机的视锥平面
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        // 获取物体的包围盒
        Bounds bounds = obj.GetComponent<Renderer>().bounds;

        // 检查包围盒是否在视锥内
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    void MainCameraAwakeInvoke(StarProjectDef.E_CameraType evID)
    {
        if (evID == StarProjectDef.E_CameraType.StarWorldCam)
        {
            Debug.Log("获取到主相机");
        }
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= ReflectionCameraRendering;
        
    }

    private void OnDestroy()
    {
        //销毁时释放
        RenderPipelineManager.beginCameraRendering -= ReflectionCameraRendering;
        if (_reflectionCamera != null && _reflectionCamera.targetTexture!=null)
        {
           _reflectionCamera.targetTexture.Release();
           DestroyImmediate(_reflectionCamera.targetTexture);
           _reflectionCamera.targetTexture = null;
           Destroy(_reflectionCamera.transform.gameObject);
           _reflectionCamera = null;
        }
        if (_reflectionCamera2 != null && _reflectionCamera2.targetTexture!=null)
        {
            _reflectionCamera2.targetTexture.Release();
            DestroyImmediate(_reflectionCamera2.targetTexture);
            _reflectionCamera2.targetTexture = null;
            Destroy(_reflectionCamera2.transform.gameObject);
            _reflectionCamera2 = null;
        }
    }

    private void OnValidate()
    {
        //设置模糊RenderFeature的参数
        setReflectionCameraRenderFeature(_reflectionCamera);
        if (SecondLayer)
            setReflectionCameraRenderFeature(_reflectionCamera2);
    }

    void Awake() {
        _oldBlurOn = _blurOn;
        _oldBlurSize = _blurSize;
        _oldBlurIterations = _blurIterations;
        _oldDownsample = _downsample;

        //设置相机的分辨率
       // setReflectionCameraRenderResolution(_reflectionCamera);
        //设置模糊RenderFeature的参数
        setReflectionCameraRenderFeature(_reflectionCamera);
        if (SecondLayer)
            setReflectionCameraRenderFeature(_reflectionCamera2);
        
    }

    void Start() {
        _sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        _blurShader = Shader.Find("Hidden/KawaseBlur");
        if (_blurShader == null)
            Debug.LogError("缺少Hidden/KawaseBlur Shader");
       // mainCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;

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

        if (_blurOn != _oldBlurOn || _blurSize != _oldBlurSize || _blurIterations != _oldBlurIterations || _downsample!= _oldDownsample)
        {
            _blurParamChanged = true;
        }
    }

    //创建反射用的摄像机
    Camera CreateReflectionCamera(Camera mainCam, bool ifSecondLayer) {
        //生成Camera
        String reflName = String.Empty;
        if(!ifSecondLayer)
            reflName = gameObject.name + "Reflection" + mainCam.name;
        else
            reflName = gameObject.name + "Reflection" + mainCam.name + "_Second Layer";
        GameObject go = new GameObject(reflName);
        go.hideFlags = HideFlags.DontSave;
        //go.hideFlags = HideFlags.HideAndDontSave;
        //go.hideFlags = HideFlags.HideAndDontSave;
        Camera reflectCamera = go.AddComponent<Camera>();
        //设置反射相机的参数
        HoldCameraSettings(reflectCamera,ifSecondLayer);
        
        //创建RT并绑定Camera
        if (!reflectCamera.targetTexture) {
            reflectCamera.targetTexture = CreateTexture(mainCam, ifSecondLayer);
        }

        return reflectCamera;
    }
    //设置反射相机的参数
    void HoldCameraSettings(Camera heplerCam,bool ifSecondLayer)
    {
        heplerCam.backgroundColor = ifSecondLayer ? Color.green : Color.black;
        heplerCam.clearFlags = ifSecondLayer ? CameraClearFlags.SolidColor : (_reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor);
        heplerCam.renderingPath = RenderingPath.Forward;
        heplerCam.cullingMask = ifSecondLayer ?_reflectionMask2 : _reflectionMask;
        heplerCam.allowMSAA = false;
        heplerCam.enabled = false;
        heplerCam.tag = reflectionCameraTag;
        //heplerCam.GetComponent<UniversalAdditionalCameraData>().SetRenderer(planarReflectionRenderDataIndex);
    }
    //创建RT 
    RenderTexture CreateTexture(Camera sourceCam, bool ifSecondLayer)
    {
        // int width = Mathf.RoundToInt(Screen.width / _downsample);
        // int height = Mathf.RoundToInt(Screen.height / _downsample);
        int width = Mathf.RoundToInt(sourceCam.pixelWidth / _downsample / 3f);
        int height = Mathf.RoundToInt(sourceCam.pixelHeight / _downsample / 3f);
        //Debug.Log("Name: "+ sourceCam.name + " Width: " + width + " Height: " + height );
        RenderTextureFormat formatRT = sourceCam.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        RenderTexture rt = new RenderTexture(width, height, 24, formatRT);
        rt.name = ifSecondLayer ? "ReflectionPlaneRT2" : "ReflectionPlaneRT";
        rt.hideFlags = HideFlags.DontSave;
        return rt;
    }
    


    void ReflectionCameraRendering(ScriptableRenderContext context, Camera camera) {
      //  Camera currentCam = Camera.current;
#if UNITY_EDITOR
        //在编辑器模式下, 需要区分当前的相机是主相机还是Scene相机
        if (Camera.current == Camera.main)
        {
           mainCamera = mainCameraTemp;
           //Debug.Log("相同!Camera.current的名字叫:"+Camera.current.name + "Camera.main的名字叫"+ Camera.main.name);
        }
        else if (Camera.current!=null && Camera.current.name == "Preview Camera")
        {
            return;
        }
        else
        {
            mainCamera = mainCameraTemp;
            //Preview Camera  SceneCamera
            if (Camera.current != null)
            {
                //在渲染场景camera
                mainCamera = SceneView.lastActiveSceneView.camera;
                //Debug.Log("不同!Camera.current的名字叫:"+Camera.current.name + "Camera.main的名字叫"+ Camera.main.name);
            }
           
        }
#endif
        
        Camera currentCam = mainCameraTemp;
        if (currentCam == null) {
            return;
        }
// #if !UNITY_EDITOR
//         if (!currentCam.gameObject.CompareTag("MainCamera"))
//             return;
// #endif
//
//         // if (_insideRendering) {
//         //     return;
//         // }
//         _insideRendering = true;
//
//         if (_reflectionCamera == null)
//         {
//            // _reflectionCamera = CreateReflectionCamera(currentCam);
//             reflectCamObj = GameObject.FindWithTag(reflectionCameraTag);
//              if (reflectCamObj == null)
//              {
//                  _reflectionCamera = CreateReflectionCamera(mainCameraTemp,FIRSTLAYER);
//                  if(SecondLayer)
//                     _reflectionCamera2 = CreateReflectionCamera(mainCameraTemp,SECONDLAYER);
//                  //Debug.Log("找不到反射相机");
//              }
//             // else 
//             //  {
//             //      //Debug.Log("找到相机,名字为: "+ reflectCamObj.name);
//             //      if (reflectCamObj.GetComponent<Camera>() != null)
//             //      {
//             //         reflectCamObj.GetComponent<Camera>().targetTexture.DiscardContents(); 
//             //      } 
//             //      DestroyImmediate(reflectCamObj);
//             //      _reflectionCamera = CreateReflectionCamera(mainCameraTemp,FIRSTLAYER);
//             //      if(SecondLayer)
//             //          _reflectionCamera2 = CreateReflectionCamera(mainCameraTemp,SECONDLAYER);
//             // }
//         }
        if (_reflectionCamera != null)
        {
            //设置Renderer
            SetCameraData(_reflectionCameraRendererData);
            // if (_reflectionCameraRendererData!=null)
            // {
            //     _reflectionCameraRendererData.SetRenderer(planarReflectionRenderDataIndex);
            // }
            // else
            // {
            //     _reflectionCameraRendererData = _reflectionCamera.transform.GetComponent<UniversalAdditionalCameraData>();
            //     if (_reflectionCameraRendererData != null)
            //     {
            //         _reflectionCameraRendererData.SetRenderer(planarReflectionRenderDataIndex);
            //         _reflectionCameraRendererData.requiresColorOption = CameraOverrideOption.Off;
            //         _reflectionCameraRendererData.requiresDepthOption =  CameraOverrideOption.Off;
            //         _reflectionCameraRendererData.renderShadows = false;
            //     }
            // }
            
    
            //渲染反射图
            //RenderReflection(context, currentCam, _reflectionCamera);
    
            //是否对反射图进行模糊
            if (_reflectionCamera && _sharedMaterial) {
                if (_blurOn) {
                    // if (_bluredReflectionTexture == null)
                    //     _bluredReflectionTexture = CreateTexture(currentCam);
                    //PostProcessTexture(context, currentCam, _reflectionCamera.targetTexture, _bluredReflectionTexture);
                  //  _reflectionCamera.SetRenderer(); 
                    //渲染反射图
                    RenderReflection(context, currentCam, _reflectionCamera);
                    _sharedMaterial.SetTexture(_reflectionTex,  _reflectionCamera.targetTexture);
                    if (SecondLayer && _reflectionCamera2!=null)
                    {
                        RenderReflection(context, currentCam, _reflectionCamera2);
                        _sharedMaterial.SetTexture(_reflectionTex2,  _reflectionCamera2.targetTexture);
                    }
                }
                else {
                    //渲染反射图
                    RenderReflection(context, currentCam, _reflectionCamera);
                    _sharedMaterial.SetTexture(_reflectionTex, _reflectionCamera.targetTexture);
                    if (SecondLayer && _reflectionCamera2!=null)
                    {
                        RenderReflection(context, currentCam, _reflectionCamera2);
                        _sharedMaterial.SetTexture(_reflectionTex2,  _reflectionCamera2.targetTexture);
                    }
                }
                
            }
            
    
            _insideRendering = false;
        }
        
    }
    //设置Camera的AdditionalCameraData
    void SetCameraData(UniversalAdditionalCameraData reflectionCameraRendererData)
    {
        if (reflectionCameraRendererData!=null)
        {
            reflectionCameraRendererData.SetRenderer((int)planarReflectionRenderDataIndex);
            reflectionCameraRendererData.requiresColorOption = CameraOverrideOption.Off;
            reflectionCameraRendererData.requiresDepthOption =  CameraOverrideOption.Off;
            reflectionCameraRendererData.renderShadows = false;
        }
        else
        {
            reflectionCameraRendererData = _reflectionCamera.GetUniversalAdditionalCameraData();
            if (reflectionCameraRendererData != null)
            {
                reflectionCameraRendererData.SetRenderer((int)planarReflectionRenderDataIndex);
                reflectionCameraRendererData.requiresColorOption = CameraOverrideOption.Off;
                reflectionCameraRendererData.requiresDepthOption =  CameraOverrideOption.Off;
                reflectionCameraRendererData.renderShadows = false;
            }
        }
    }
    //调用反射相机，渲染反射图
    void RenderReflection(ScriptableRenderContext context, Camera currentCam, Camera reflectCamera) {
        if (reflectCamera == null) {
            Debug.LogError("反射Camera无效");
            return;
        }
        if (_sharedMaterial && !_sharedMaterial.HasProperty(_reflectionTex))
        {
            Debug.LogError("Shader中缺少_ReflectionTex属性");
            return;
        }
        //保持反射相机的参数
        if(!reflectCamera.name.Contains("Second Layer"))
            HoldCameraSettings(reflectCamera, FIRSTLAYER);
        else
            HoldCameraSettings(reflectCamera, SECONDLAYER);

        if (_reflectSkybox) {
            if (currentCam.gameObject.GetComponent(typeof(Skybox))) {
                Skybox sb = (Skybox)reflectCamera.gameObject.GetComponent(typeof(Skybox));
                if (!sb) {
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

    static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane) {
        Vector4 q = projection.inverse * new Vector4(
            Mathf.Sign(clipPlane.x),
            Mathf.Sign(clipPlane.y),
            1.0F,
            1.0F
            );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        // third row = clip plane - fourth row
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];

        return projection;
    }
    //设置反射相机的分辨率
    void setReflectionCameraRenderResolution(Camera ReflectionCamera)
    {
        int width = ReflectionCamera.targetTexture.width / _downsample;
        int height = ReflectionCamera.targetTexture.height / _downsample;
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        ReflectionCamera.targetTexture = renderTexture;
    }
    //设置反射相机的RenderFeature相关参数
    void setReflectionCameraRenderFeature(Camera ReflectionCamera)
    {
        if (ReflectionCamera != null)
        {
            // UniversalAdditionalCameraData addtionalData = ReflectionCamera.GetComponent<UniversalAdditionalCameraData>();
            // ScriptableRenderer reflectCameraRenderer = addtionalData.scriptableRenderer;
            // ScriptableRenderer.RenderingFeatures reflectCameraRFs = reflectCameraRenderer.supportedRenderingFeatures;
            // UniversalRendererData reflectCameraRenderData;
            
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

            // List<ScriptableRendererFeature> features = (List<ScriptableRendererFeature>)reflectCameraRFs;
            // foreach (var feature in reflectCameraRFs)
            // {
            //     
            // }
            //ScriptableRendererFeature reflectCameraRF = reflectCameraRenderer.supportedRenderingFeatures.LogError();
            //ScriptableRenderer reflectCameraRenderer = ReflectionCamera.get;
        }
    }

    static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane) {
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

    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign) {
        Vector3 offsetPos = pos + normal * _clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    // //对反射图进行图像处理(利用command buffer实现)
    // private Dictionary<Camera, CommandBuffer> _cameras = new Dictionary<Camera, CommandBuffer>();
    // void PostProcessTexture(ScriptableRenderContext context,Camera cam, RenderTexture source, RenderTexture dest)
    // {
    //     //参数有变化需要刷新commandbuffer
    //     if (_blurParamChanged)
    //     {
    //         if (_cameras.ContainsKey(cam))
    //             cam.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _cameras[cam]);
    //         _cameras.Remove(cam);
    //     }
    //     //已经设置了commandbuffer就不用再执行了
    //     if (_cameras.ContainsKey(cam))
    //         return;
    //
    //     CommandBuffer buf = CommandBufferPool.Get("Blur Reflection Texture");//new CommandBuffer();
    //     //buf.name = "Blur Reflection Texture";
    //     _cameras[cam] = buf;
    //     float width = source.width;
    //     float height = source.height;
    //     int rtW = Mathf.RoundToInt(width / _downsample);
    //     int rtH = Mathf.RoundToInt(height / _downsample);
    //
    //     int blurredID = Shader.PropertyToID("_Temp1");
    //     int blurredID2 = Shader.PropertyToID("_Temp2");
    //     buf.GetTemporaryRT(blurredID, rtW, rtH, 0, FilterMode.Bilinear, source.format);
    //     buf.GetTemporaryRT(blurredID2, rtW, rtH, 0, FilterMode.Bilinear, source.format);
    //
    //     buf.Blit((Texture)source, blurredID);
    //     for (int i = 0; i < _blurIterations; i++)
    //     {
    //         float iterationOffs = (i * 1.0f);
    //         buf.SetGlobalFloat("_Offset", iterationOffs / _downsample + _blurSize);
    //         buf.Blit(blurredID, blurredID2, BlurMaterial, 0);
    //         buf.Blit(blurredID2, blurredID, BlurMaterial, 0);
    //     }
    //     buf.Blit(blurredID, dest);
    //
    //     buf.ReleaseTemporaryRT(blurredID);
    //     buf.ReleaseTemporaryRT(blurredID2);
    //     
    //     
    //     //context.ExecuteCommandBuffer(buf);
    //     cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
    //     
    //     CommandBufferPool.Release(buf);
    // }

}