using UnityEngine;
using System;
using UnityEngine.Rendering.Universal.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class CustomSpotShadow : CustomShadowBase
{

    //public Material SSProjectorShadow;
    bool CaptureStaticLayeEveryFrame = false;
    /*public */Color ShadowColor = Color.black;
    //
    [Range(0.0f, 10.0f)]
    public float DepthBias_Depth = 0.5f; 
    [Range(0.0f, 3.0f)]
    public float DepthBias_Normal = 0.0f;

#if UNITY_EDITOR

    float _LastZBias_Depth = -0.02f;
    float _LastZBias_Normal = 0.0f;

    //float _LastZBias = 0.125f;
    Color _LastShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.666667f);

    Vector3 _LastCamPosition;
    Quaternion _LastCamRoate;
    float _LastNear = .0f;
    float _LastFar = .0f;
    float _LastFov = .0f;
    ShadowMapSize _LastDynamicShadowMapSize = ShadowMapSize._1024;
    ShadowMapSize _LastStaticShadowMapSize = ShadowMapSize._1024;
    float _LastShadowStrength = 1;

    LayerMask m_LastStaticLayer;
    LayerMask m_LastDynamicLayer;
#endif

    [HideInInspector]
    //bool _inited = false;

    const int SPOT_RENDERINDEX = 9;
    //public int a = 0;
    // Start is called before the first frame update
    void Start()
    {
        //DetermineRenderer(SPOT_RENDERINDEX);
        //_initCamera();
    }
    void _initCamera()
    {
        if (null != _light)
        {
            _light.type = LightType.Spot;
        }

        {
            _projector.aspect = 1.0f;
            _projector.nearClipPlane = 0.01f;
            _projector.farClipPlane = _light.range;
            _projector.fieldOfView = _light.spotAngle;
            _projector.cullingMask = LayerMask.GetMask("Scene") + LayerMask.GetMask("Character");
            
        }
        //}
        _projector.enabled = false;
        _projector.orthographic = false;
        _projector.useOcclusionCulling = true;
        //_projector.backgroundColor = Color.blue;

#if UNITY_EDITOR

        _LastNear = _projector.nearClipPlane;
        _LastFar = _projector.farClipPlane = _light.range;
        _LastFov = _projector.fieldOfView = _light.spotAngle;
#endif

    }

    protected override void _DoInit()
    {
        DetermineRenderer(SPOT_RENDERINDEX);
        _AddShadow();
        _initCamera();
        _SyncAttr_DepthMapDimens();
        _SyncAttr_ShadowCamera();
        //_SyncAttr_CaptureStaticLayer();
        _SyncAttr_ShadowMatrix();
        _SyncAttr_ShadowStrength();
        _SyncAttr_Inited();

        SyncDynamicShadowLayer(0);
        SyncStaticShadowLayer(0);
    }
    protected override void _SyncAttr_DepthMapDimens()
    {
        CustomShadowMgr.Instance.SetShadowSize(_light, 0, (int)_StaticShadowMapSize, (int)_DynamicShadowMapSize);
    }
    void _SyncAttr_ShadowStrength()
    {
        CustomShadowMgr.Instance.SetShadowStrength(_light, 0, _shadowStrength);
    }

    // ScriptRenderer.cs SetPerCameraShaderVariables �������?
    void _SyncAttr_ShadowMatrix()
    {
        Matrix4x4 gpuProj = new Matrix4x4(); 
        _BuildSpotShadowProjectionMatrix(ref gpuProj); 
        //Matrix4x4 mat = gpuProj * _projector.worldToCameraMatrix;
        Matrix4x4 viewMatrix = new Matrix4x4();
        //_BuildViewMatrix(transform.position, transform.rotation, ref viewMatrix);
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(1, 1, -1));
        viewMatrix = scale * transform.worldToLocalMatrix;
        
        Matrix4x4 mat = gpuProj * viewMatrix;
        //_light.shadows.
        ///

        CustomShadowMgr.Instance.SetStaticShadowMatrix(_light, 0, mat, _preCasterMatrix * mat, gpuProj, DepthBias_Depth, DepthBias_Normal, _shadowStrength, DepthBias_Depth, DepthBias_Normal);
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
        if (_LastDynamicShadowMapSize != _DynamicShadowMapSize || _LastStaticShadowMapSize != _StaticShadowMapSize)
        {
            _SyncAttr_DepthMapDimens();
            _LastDynamicShadowMapSize = _DynamicShadowMapSize;
            _LastStaticShadowMapSize = _StaticShadowMapSize;
        }

        if (_LastShadowStrength != _shadowStrength)
        {
            _SyncAttr_ShadowStrength();
            _LastShadowStrength = _shadowStrength;
        }

        if (_LastCamPosition != gameObject.transform.position ||
            _LastCamRoate != gameObject.transform.rotation ||
            _LastNear != _projector.nearClipPlane ||
            _LastFar != _light.range ||
            _LastFov != _light.spotAngle ||
            //
            _projector.farClipPlane != _light.range ||
            _projector.fieldOfView != _light.spotAngle ||
            DepthBias_Depth != _LastZBias_Depth ||
            DepthBias_Normal != _LastZBias_Normal ||
            ShadowColor != _LastShadowColor)
        {
            _SyncAttr_ShadowMatrix();
            //
            _LastCamPosition = gameObject.transform.position;
            _LastCamRoate = gameObject.transform.rotation;
            _LastNear = _projector.nearClipPlane;
            _LastFar = _projector.farClipPlane = _light.range;
            _LastFov = _projector.fieldOfView = _light.spotAngle;
            //
            _LastZBias_Depth = DepthBias_Depth;
            _LastZBias_Normal = DepthBias_Normal;
            _LastShadowColor = ShadowColor;
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
#endif
        if (CaptureStaticLayeEveryFrame )
        {
            //CreateCaptureCamera();
            _SyncAttr_CaptureStaticLayer();
        }
    }
    private void OnDrawGizmos()
    {
        return;
#region
        if (enabled && gameObject.activeInHierarchy)
        {
            float halfAngle1 = 0.5f * _projector.fieldOfView;
            float halfAngle2 = 0.5f * _projector.fieldOfView;
            Vector3 ptRectCenter = transform.position + transform.forward * _projector.farClipPlane;
            Vector3 ptRightForward = ptRectCenter +
                Mathf.Tan(halfAngle1 * Mathf.Deg2Rad) * _projector.farClipPlane * transform.right + Mathf.Tan(halfAngle2 * Mathf.Deg2Rad) * _projector.farClipPlane * transform.up;
            Vector3 ptRightBack = ptRectCenter +
                Mathf.Tan(halfAngle1 * Mathf.Deg2Rad) * _projector.farClipPlane * transform.right - Mathf.Tan(halfAngle2 * Mathf.Deg2Rad) * _projector.farClipPlane * transform.up;
            Vector3 ptLeftBack = ptRectCenter -
                Mathf.Tan(halfAngle1 * Mathf.Deg2Rad) * _projector.farClipPlane * transform.right - Mathf.Tan(halfAngle2 * Mathf.Deg2Rad) * _projector.farClipPlane * transform.up;
            Vector3 ptLeftForward = ptRectCenter -
                Mathf.Tan(halfAngle1 * Mathf.Deg2Rad) * _projector.farClipPlane * transform.right + Mathf.Tan(halfAngle2 * Mathf.Deg2Rad) * _projector.farClipPlane * transform.up;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, ptRightForward);
            Gizmos.DrawLine(transform.position, ptRightBack);
            Gizmos.DrawLine(transform.position, ptLeftBack);
            Gizmos.DrawLine(transform.position, ptLeftForward);
            Gizmos.DrawLine(ptRightForward, ptRightBack);
            Gizmos.DrawLine(ptRightBack, ptLeftBack);
            Gizmos.DrawLine(ptLeftBack, ptLeftForward);
            Gizmos.DrawLine(ptLeftForward, ptRightForward);
            //Gizmos.DrawLine(transform.position, ShadowRange);

        }
#endregion
    }
    override public void CopyShadowData(CustomShadowBase shadow)
    {
        base.CopyShadowData(shadow);
        CustomSpotShadow sShadow = (CustomSpotShadow)shadow;
        //
        //bool CaptureStaticLayeEveryFrame = false;
        ShadowColor = sShadow.ShadowColor;
        DepthBias_Depth = sShadow.DepthBias_Depth;
        DepthBias_Normal = sShadow.DepthBias_Normal;
    #if UNITY_EDITOR
        _LastZBias_Depth = sShadow._LastZBias_Depth;
        _LastZBias_Normal = sShadow._LastZBias_Normal;
        _LastShadowColor  = sShadow._LastShadowColor;
        //_LastCamPosition = sShadow._LastCamPosition;
        //_LastCamRoate = sShadow._LastCamRoate;
        //_LastNear = sShadow._LastNear;
        //_LastFar =  sShadow._LastFar;
        //_LastFov = sShadow._LastFov;
        _LastDynamicShadowMapSize = sShadow._LastDynamicShadowMapSize;
        _LastStaticShadowMapSize = sShadow._LastStaticShadowMapSize;
        _LastShadowStrength = sShadow._LastShadowStrength;
        m_LastStaticLayer = sShadow.m_StaticLayer;
        m_LastDynamicLayer = sShadow.m_DynamicLayer;
#endif
        //[HideInInspector]
        //bool _inited = false;
        //const int SPOT_RENDERINDEX = 9;

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
