using System;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using StarProjectDef;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
[ExecuteAlways]
#endif
public class CustomDirectionalShadow : CustomShadowBase
{
    public CustomShadowMgr.ShadowType m_StaticShadowType = CustomShadowMgr.ShadowType.SSM;
    [Min(1)]
    public float ShadowDistance = 20.0f;
    [Min(1)]
    public float DynamicShadowDistance = 35.0f;
    [Min(1)]
    public float DynamicCenterLength = 20.0f;
    [Range(0.0f, 10.0f)]
    public float DepthBias_Depth = 1.5f;
    [Range(-1.0f, 3.0f)]
    public float DepthBias_Normal = -0.02f;
    [Range(-90.0f, 90.0f)]
    public float m_CharaLightXAxisOffset = 0;
    [Range(0.0f, 10.0f)]
    public float DepthBias_Depth_Dynamic = 3.0f;
    [Range(-1.0f, 3.0f)]
    public float DepthBias_Normal_Dynamic = 0.75f;
    [Range(1, 500)]
    public int m_ESMConst = 80;
    [Range(0, 0.2f)] 
    public float m_ESMBlurDelta = 0.035f;
    [SerializeField]
    bool m_AllowPlayingDebug = false;

    public enum DynamicCenterMode
    {
        Constant,
        CameraRelated,
    }

    Transform _MainCameraTransform;
    DynamicCenterMode m_DynamicCenterMode = DynamicCenterMode.Constant;

    float m_DynamicShadowDistanceBackup;
    Transform m_MainCameraTransformOverride;
    bool m_FollowCameraBackup;
    float m_CharaXOffsetBackup;
    float m_DynamicCenterLengthBackup;
    DynamicCenterMode m_DynamicCenterModeBackup;
    //
    Matrix4x4 _gpuProjDynmic;
    Matrix4x4 _matViewDynamic = new Matrix4x4();

    //[SerializeField]
    bool CaptureStaticLayeEveryFrame = false;

    /*public */
    Color ShadowColor = Color.black;
    //

    bool m_FollowCamera = true;

    float m_FarStatic = 120;
    float m_NearStatic = -280;
    float m_FarDynamic = 50;
    float m_NearDynamic = -50;

    Vector3 _RendererCenter = Vector3.zero;

    Transform mainCameraTransform
    {
        get { return m_MainCameraTransformOverride ? m_MainCameraTransformOverride : _MainCameraTransform; }
    }

#if UNITY_EDITOR
    float _LastSize = .0f;

    float _LastZBias_Depth = -0.02f;
    float _LastZBias_Normal = 0.0f;
    float _LastZBias_Depth_Dynamic = -0.02f;
    float _LastZBias_Normal_Dynamic = 0.0f;
    Color _LastShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.666667f);
    ShadowMapSize _LastDynamicShadowMapSize = ShadowMapSize._1024;
    ShadowMapSize _LastStaticShadowMapSize = ShadowMapSize._1024;

    float _LastShadowStrength = 1;
    LayerMask m_LastStaticLayer;
    LayerMask m_LastDynamicLayer;

    RenderTexture m_ShadowCameraTexture;
    public RenderTexture shadowCameraTexture { get { return m_ShadowCameraTexture; } }
    Vector3 m_LastPosition;
    Quaternion m_LastRotation;
    CustomShadowMgr.ShadowType m_LastStaticShadowType;
    int m_LastESMConst;
    float m_LastESMBlurDelta = 0.2f;

#endif

    float _LastDynamicShadowDistance = 0.0f;

    const int DIR_RENDERINDEX = 7;
    //public int a = 0;
    // Start is called before the first frame update
    void Start()
    {
    }
    public static Bounds TransformBounds( Transform _transform, Bounds _localBounds)
    {
        var center = _transform.InverseTransformPoint(_localBounds.center);

        // transform the local extents' axes
        var extents = _localBounds.extents;
        var axisX = _transform.InverseTransformVector(extents.x, 0, 0);
        var axisY = _transform.InverseTransformVector(0, extents.y, 0);
        var axisZ = _transform.InverseTransformVector(0, 0, extents.z);

        // sum their absolute value to get the world extents
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }

    void _calculateRendererCenter()
    {
        if (!m_FollowCamera)
            return;

        if (mainCameraTransform == null)
            return;

        if (m_DynamicCenterMode == DynamicCenterMode.Constant)
        {
            _RendererCenter = mainCameraTransform.position + mainCameraTransform.forward * DynamicCenterLength;
        }
        else if (m_DynamicCenterMode == DynamicCenterMode.CameraRelated)
        {
            var camForward = new Vector3(mainCameraTransform.forward.x, 0, mainCameraTransform.forward.z);
            var lightForward = new Vector3(transform.forward.x, 0, transform.forward.z);
            var dot = Vector3.Dot(camForward, lightForward);

            var dynamicCenterLength = DynamicCenterLength;
            if (dot > 0.5f && dot < 1)
            {
                dynamicCenterLength = 1;
            }
            else if (dot > 0 && dot < 0.5)
            {
                dynamicCenterLength = Mathf.Lerp(DynamicCenterLength * 0.5f, 1, dot * 2);
            }
            else
            {
                dynamicCenterLength = Mathf.Lerp(DynamicCenterLength * 0.5f, DynamicCenterLength, -dot);
            }

            var dir = new Vector3(camForward.x, lightForward.y, camForward.z);
            _RendererCenter = mainCameraTransform.position + dir * dynamicCenterLength;
        }
    }
   
    public void SetRendererCenter(Transform target, float distance, float charaXOffset, bool followCamera, float dynamicCenterLength, DynamicCenterMode mode)
    {
        m_MainCameraTransformOverride = target;

        m_DynamicShadowDistanceBackup = DynamicShadowDistance;
        m_FollowCameraBackup = m_FollowCamera;
        m_CharaXOffsetBackup = m_CharaLightXAxisOffset;
        m_DynamicCenterLengthBackup = DynamicCenterLength;
        m_DynamicCenterModeBackup = m_DynamicCenterMode; 

        DynamicShadowDistance = distance;
        m_FollowCamera = followCamera;
        m_CharaLightXAxisOffset = charaXOffset;
        DynamicCenterLength = dynamicCenterLength;
        m_DynamicCenterMode = mode;
    }

    public void SetRendererCenter(Transform target, float distance, float charaXOffset, bool followCamera)
    {
        m_MainCameraTransformOverride = target;

        m_DynamicShadowDistanceBackup = DynamicShadowDistance;
        m_FollowCameraBackup = m_FollowCamera;
        m_CharaXOffsetBackup = m_CharaLightXAxisOffset;
        m_DynamicCenterLengthBackup = DynamicCenterLength;
        m_DynamicCenterModeBackup = m_DynamicCenterMode;

        DynamicShadowDistance = distance;
        m_FollowCamera = followCamera;
        m_CharaLightXAxisOffset = charaXOffset;
    }
#if UNITY_EDITOR
    internal void UpdateRendererCenter(Transform target, float distance, float charaXOffset, bool followCamera, float dynamicCenterLength, DynamicCenterMode mode)
    {
        m_MainCameraTransformOverride = target;
        DynamicShadowDistance = distance;
        m_FollowCamera = followCamera;
        m_CharaLightXAxisOffset = charaXOffset;
        DynamicCenterLength = dynamicCenterLength;
        m_DynamicCenterMode = mode;
    }
#endif
    public void ResetRendererCenter()
    {
        m_MainCameraTransformOverride = null;

        DynamicShadowDistance = m_DynamicShadowDistanceBackup;
        m_FollowCamera = m_FollowCameraBackup;
        m_CharaLightXAxisOffset = m_CharaXOffsetBackup;
        DynamicCenterLength = m_DynamicCenterLengthBackup;
        m_DynamicCenterMode = m_DynamicCenterModeBackup;
    }

    void _initCamera()
    {

        if (_light)
        {
            _light.type = LightType.Directional;
        }

        _projector.enabled = false;
        _projector.cullingMask = -1;
        _projector.orthographic = true;
        _projector.useOcclusionCulling = true;
        _projector.backgroundColor = Color.blue;
        _projector.clearFlags = CameraClearFlags.SolidColor;

        _projector.farClipPlane = m_FarStatic;
        _projector.nearClipPlane = m_NearStatic;
        _projector.aspect = 1.0f;
        _projector.orthographicSize = ShadowDistance*0.5f;

        // By Wuzhongjie, 获取Camera先从CameraManager拿
        if (_MainCameraTransform == null)
        {
            Camera mainCam = null;
            if (Application.isPlaying && StarProject.Service.Cam.CameraManager.Instance != null)
            {
                if (StarProject.Service.Cam.CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam) != null)
                    mainCam = StarProject.Service.Cam.CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).Camera;
                }

            // CameraManager不存在的情况再从UnityEngine.Camera里找
            if (mainCam == null)
            {
                if (Camera.main)
                    mainCam = Camera.main;
                else
                {
                    Camera[] all = Camera.allCameras;
                    foreach (var one_cam in all)
                    {
                        if (one_cam.tag == "MainCamera")
                        {
                            mainCam = one_cam;
                        }
                    }
                }
            }

            if (mainCam != null)
            {
                _MainCameraTransform = mainCam.transform;
            }
            else
            {
                Debug.LogWarning("没有主相机");
                _MainCameraTransform = this.transform;
            }
        }
        if(mainCameraTransform != null)
        {
            Debug.Log("------------------拾取到相机: " + mainCameraTransform.gameObject.name);
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    private void OnMainCameraChanged(GameObject cameraObj)
    {
        // 摄像机为空或阴影未初始化跳过，初始化时会自动寻找合适的摄像机
        if (cameraObj == null || !_inited || mainCameraTransform == cameraObj.transform)
            return;

        _MainCameraTransform = cameraObj.transform;
        _initCamera();
    }
    protected override void _DoInit()
    {
        DetermineRenderer(DIR_RENDERINDEX);
        _AddShadow();
        _initCamera();
        _SyncAttr_DepthMapDimens();
        _SyncAttr_StaticCameraInfo();
        _SyncAttr_ShadowCamera();
        //_SyncAttr_CaptureStaticLayer();
        _SyncAttr_ShadowMatrix();
        //
        //_collectRenderers();
        _UpdateDynamicProjMatrix();
        _SyncAttr_ShadowMatrix_D();
        _SyncAttr_ShadowStrength();

        SyncDynamicShadowLayer(0);
        SyncStaticShadowLayer(0);

        _SyncAttr_Inited();

        CustomShadowMgr.Instance.staticShadowType = m_StaticShadowType;
        CustomShadowMgr.Instance.esmConst = m_ESMConst;
        CustomShadowMgr.Instance.esmBlurDelta = m_ESMBlurDelta;
    }
    protected override void _SyncAttr_DepthMapDimens()
    {
        CustomShadowMgr.Instance.SetShadowSize(_light, 0 ,(int) _StaticShadowMapSize, (int)_DynamicShadowMapSize);
    }
    void _SyncAttr_ShadowStrength()
    {
        CustomShadowMgr.Instance.SetShadowStrength(_light, 0, _shadowStrength);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _MainCameraTransform = null;
        CustomShadowMgr.Instance.staticShadowType = CustomShadowMgr.ShadowType.SSM;
    }

    // ScriptRenderer.cs SetPerCameraShaderVariables �������
    void _SyncAttr_ShadowMatrix() 
    {
        if (_renderer == null)
            return;
            
        Matrix4x4 gpuProj = new Matrix4x4();
        _BuildDirectionalShadowProjectionMatrix(ShadowDistance, m_NearStatic, m_FarStatic, ref gpuProj);

        Matrix4x4 viewMatrix = new Matrix4x4();
        Vector3 cameraPos = transform.position;// + transform.forward * _near * 0.5f;
        _BuildViewMatrix(cameraPos, transform.rotation, ref viewMatrix);
        //viewMatrix = _projector.worldToCameraMatrix;
        Matrix4x4 mat = gpuProj * viewMatrix;

        CustomShadowMgr.Instance.SetStaticShadowMatrix(_light,0, mat, _preCasterMatrix * mat, gpuProj, DepthBias_Depth, DepthBias_Normal, 
                                                        _shadowStrength, DepthBias_Depth_Dynamic, DepthBias_Normal_Dynamic);
    }
    void _UpdateDynamicProjMatrix()
    {
        var far = Mathf.Max(m_FarDynamic, DynamicShadowDistance);
        var near = Mathf.Min(m_NearDynamic, -DynamicShadowDistance);
        _BuildDirectionalShadowProjectionMatrix(DynamicShadowDistance, near, far, ref _gpuProjDynmic);
        //
        // float halfSize = DynamicShadowDistance;// ShadowDistance;
        // Matrix4x4 matProjDynamic = Matrix4x4.Ortho(
        //     -halfSize,
        //     halfSize,
        //     -halfSize,
        //     halfSize,
        //     _near,
        //     _far);
        // if (SystemInfo.usesReversedZBuffer)
        // {
        //     matProjDynamic.m20 = -matProjDynamic.m20;
        //     matProjDynamic.m21 = -matProjDynamic.m21;
        //     matProjDynamic.m22 = -matProjDynamic.m22;
        //     matProjDynamic.m23 = -matProjDynamic.m23;
        // }
        // _gpuProjDynmic = GL.GetGPUProjectionMatrix(matProjDynamic, true);
    }
    void _SyncAttr_StaticCameraInfo()
    {
        CustomShadowMgr.Instance.SetStaticCameraInfo(_light, 0, transform.position, ShadowDistance);
    }
    void _SyncAttr_DynamicCameraInfo(Vector3 position)
    {
        CustomShadowMgr.Instance.SetDynamicCameraInfo(_light, 0, position, DynamicShadowDistance);
    }
    void _SyncAttr_ShadowMatrix_D()
    {
        Vector3 dynamicPosition = _RendererCenter;// - transform.forward * _halfLength;// * 0.5f;
        //Matrix4x4 matViewDynamic = Matrix4x4.TRS(dynamicPosition, transform.rotation, transform.localScale).inverse;
        var dynamicRot = Quaternion.Euler(m_CharaLightXAxisOffset + transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
        _BuildViewMatrix(dynamicPosition, dynamicRot, ref _matViewDynamic);
        // transform.localToWorldMatrix.inverse;
        Matrix4x4 matViewProjDynamic = _gpuProjDynmic * _matViewDynamic; //_projector.worldToCameraMatrix;// 


        //_renderer.m_DirShadowCasterPass.ShadowMatrix_D = matViewProjDynamic; //matViewProj;// 
        CustomShadowMgr.Instance.SetDynamicShadowMatrix(_light, 0, matViewProjDynamic, _preCasterMatrix * matViewProjDynamic, _gpuProjDynmic, DepthBias_Depth_Dynamic, DepthBias_Normal_Dynamic);
        _SyncAttr_DynamicCameraInfo(dynamicPosition);
    }
    public override void _SyncAttr_CaptureStaticLayer()
    {
        CustomShadowMgr.Instance.renderStaticLayer = true;
    }
    private void Update()
    {
        if (!_inited)
            return;
#if UNITY_EDITOR
        if (!Application.isPlaying || (Application.isPlaying && m_AllowPlayingDebug))
        {
            if (_LastDynamicShadowMapSize != _DynamicShadowMapSize ||
           _LastSize != ShadowDistance ||
           _LastStaticShadowMapSize != _StaticShadowMapSize||
           m_LastPosition != this.transform.position ||
           m_LastRotation != this.transform.rotation)
           {
                if (_LastSize != ShadowDistance &&
                    _projector != null)
                {
                    _projector.orthographicSize = ShadowDistance * 0.5f;
                    //_NeedCaptureStaticFrame = true;
                }
                _SyncAttr_ShadowMatrix();
                _SyncAttr_DepthMapDimens();
                _SyncAttr_StaticCameraInfo();
                ///
                _LastDynamicShadowMapSize = _DynamicShadowMapSize;
                _LastStaticShadowMapSize = _StaticShadowMapSize;
                _LastSize = ShadowDistance;
                m_LastPosition = this.transform.position;
                m_LastRotation = this.transform.rotation;
            }
            if (_LastShadowStrength != _shadowStrength)
            {
                _SyncAttr_ShadowStrength();
                _LastShadowStrength = _shadowStrength;
            }

            if (DepthBias_Depth != _LastZBias_Depth ||
                DepthBias_Normal != _LastZBias_Normal ||
                DepthBias_Depth_Dynamic != _LastZBias_Depth_Dynamic ||
                DepthBias_Normal_Dynamic != _LastZBias_Normal_Dynamic ||
                ShadowColor != _LastShadowColor ||
                m_LastESMConst != m_ESMConst ||
                m_LastESMBlurDelta != m_ESMBlurDelta)
            {
                _SyncAttr_ShadowMatrix();
                _SyncAttr_StaticCameraInfo();
                CustomShadowMgr.Instance.esmConst = m_ESMConst;
                CustomShadowMgr.Instance.esmBlurDelta = m_ESMBlurDelta;

                _LastZBias_Depth = DepthBias_Depth;
                _LastZBias_Normal = DepthBias_Normal;
                _LastShadowColor = ShadowColor;
                _LastZBias_Normal_Dynamic = DepthBias_Normal_Dynamic;
                _LastZBias_Depth_Dynamic = DepthBias_Depth_Dynamic;
                m_LastESMConst = m_ESMConst;
                m_LastESMBlurDelta = m_ESMBlurDelta;
            }

            if (m_LastDynamicLayer != m_DynamicLayer)
            {
                SyncDynamicShadowLayer(0);
                m_LastDynamicLayer = m_DynamicLayer;
            }

            if (m_LastStaticLayer != m_StaticLayer)
            {
                SyncStaticShadowLayer(0);
                m_LastStaticLayer = m_StaticLayer;
            }

            if (m_LastStaticShadowType != m_StaticShadowType)
            {
                CustomShadowMgr.Instance.staticShadowType = m_StaticShadowType;
                m_LastStaticShadowType = m_StaticShadowType;
            }
        }
#endif
        if (DynamicShadowDistance != _LastDynamicShadowDistance)
            _UpdateDynamicProjMatrix();

        if (CaptureStaticLayeEveryFrame)
            _SyncAttr_CaptureStaticLayer();

        _calculateRendererCenter();
        _SyncAttr_ShadowMatrix_D();
    }
    
    private void OnDrawGizmos()
    {
        #region
        if (enabled && gameObject.activeInHierarchy)
        {
            if (null == mainCameraTransform)
                return;
            //
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_RendererCenter, 1.0f);
            Gizmos.DrawLine(mainCameraTransform.position, _RendererCenter);
        }
        #endregion
#if UNITY_EDITOR
        Camera sceneCamera = SceneView.lastActiveSceneView.camera;
        Vector3 pos = sceneCamera.ViewportToScreenPoint(new Vector3(0, 0, 0));
        //Rect rect = new Rect(pos, new Vector2(200, 200));

        //Gizmos.DrawGUITexture(rect, m_ShadowCameraTexture);
        //GUI.DrawTexture(new Rect(0, 0, 200, 200), m_ShadowCameraTexture);
#endif
    }


    override public void CopyShadowData(CustomShadowBase shadow)
    {
        base.CopyShadowData(shadow);
        //
        CustomDirectionalShadow dShadow = (CustomDirectionalShadow)shadow;
        ShadowDistance = dShadow.ShadowDistance;
        DynamicShadowDistance = dShadow.DynamicShadowDistance;
        DynamicCenterLength = dShadow.DynamicCenterLength;
        //
        //_gpuProjDynmic = dShadow._gpuProjDynmic; 
        //_matViewDynamic = dShadow._matViewDynamic;
        ShadowColor = dShadow.ShadowColor;
        //
        DepthBias_Depth = dShadow.DepthBias_Depth;
        DepthBias_Normal = dShadow.DepthBias_Normal;
        //Transform _MainCameraTransform;
        //_far= dShadow._far;
        //_near= dShadow._near;
        //
        //List<SkinnedMeshRenderer> _lstSkinnedMeshRenderer;
        //List<MeshRenderer> _lstMeshRenderer;
        //struct _ObjInfo
        //{
        //    public GameObject obj;
        //    public List<SkinnedMeshRenderer> skinnedmeshes;
        //    public List<MeshRenderer> meshes;
        //}
        //Dictionary<GameObject,_ObjInfo> _DynamicObjs = new Dictionary<GameObject, _ObjInfo>();
        //Vector3 _RendererCenter = Vector3.zero;

#if UNITY_EDITOR
        //_LastCamRotate = dShadow._LastCamRotate;
        //_LastCamPosition = dShadow._LastCamPosition;
        //_LastCamScale = dShadow._LastCamScale;
        //_LastNear = dShadow._LastNear;
        //_LastFar =  dShadow._LastFar;
        _LastSize = dShadow._LastSize;
        //
        _LastZBias_Depth = dShadow._LastZBias_Depth;
        _LastZBias_Normal = dShadow._LastZBias_Normal;
        _LastShadowColor  = dShadow._LastShadowColor;
        _LastDynamicShadowMapSize = dShadow._LastDynamicShadowMapSize;
        _LastStaticShadowMapSize = dShadow._LastStaticShadowMapSize;
        _LastDynamicShadowDistance = dShadow._LastDynamicShadowDistance;
        _LastShadowStrength = dShadow._LastShadowStrength;
        m_LastStaticLayer = dShadow.m_StaticLayer;
        m_LastDynamicLayer = dShadow.m_DynamicLayer;
#endif
        //bool _inited = false;

        // TODO：尝试在设置完成后重新Init，看看还有没有阴影出错问题
        Init();
    }

    override public void SetShadowOn(bool b)
    {
        if (_inited == b)
            return;

        if (b) 
        { 
            if (isActiveAndEnabled)
            {
                Init();
            }
        }
        else if (_inited)
        {
            _RmvShadow();
            _inited = false;
        }
    }
}
