using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class CustomOmniShadow : CustomShadowBase
{
    //Cubemap face.
    public enum CubemapFaceMask
    {
        PositiveX = 1,
        NegativeX = 2,
        PositiveY = 4,
        NegativeY = 8,
        PositiveZ = 16,
        NegativeZ = 32
    }

    public CubemapFaceMask ShadowDirection = CubemapFaceMask.NegativeY;
    //
    //public Material SSProjectorShadow;
    bool CaptureStaticLayeEveryFrame = false;
    ///*public*/ bool RenderDynamicCaster = true;
    //public float ShadowRange = 50.0f;

    /*public */
    Color ShadowColor = Color.black;
    //
    [Range(0.0f, 10.0f)]
    public float DepthBias_Depth = 0.5f; 
    [Range(0.0f, 3.0f)]
    public float DepthBias_Normal = 0.0f;

    Matrix4x4[] _ShadowMatrices = new Matrix4x4[6];
    Matrix4x4 _gpuProj;
    /// <summary>
    ///Cache锟斤拷锟斤拷,editor锟斤拷
    /// </summary>
#if UNITY_EDITOR
    float _LastZBias_Depth = -0.02f;
    float _LastZBias_Normal = 0.0f;
    Color _LastShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.666667f);

    //影锟斤拷depthtexture锟斤拷染锟斤拷锟斤拷锟斤拷锟斤拷锟?
    Vector3 _LastCamPosition;
    float _LastOmniRange;
    CubemapFaceMask _LastShadowDirection;
    ShadowMapSize _LastDynamicShadowMapSize = ShadowMapSize._1024;
    ShadowMapSize _LastStaticShadowMapSize = ShadowMapSize._1024;
    float _LastShadowStrength = 1;

    LayerMask m_LastStaticLayer;
    LayerMask m_LastDynamicLayer;
#endif
    //Light _OmniLight;
    //
    protected Matrix4x4 _CacheProjMatrix;
    [HideInInspector]
    bool _inited = false;

    const int OMNI_RENDERINDEX = 8;
    //public int a = 0;
    // Start is called before the first frame update
    protected override void _SyncAttr_ShadowCamera()
    {
        CustomShadowMgr.Instance.SetShadowCamera(_light, 0, _projector);
    }
    protected override void _AddShadow(int faceID = -1)
    {
        for (int i = 0; i < 6; i++)
        {
            base._AddShadow(i);
        }
    }
    void _initCamera()
    {
        if (null == _projector)
        {
            //GameObject go = new GameObject("__OmniCamera", typeof(Camera));
            _projector = GetComponent<Camera>();
        }
        if( null == _light)
        {
            _light = GetComponent<Light>();
        }
        //模锟斤拷锟斤拷未锟斤拷锟?
        //if (!_inited)
        {
            _projector.nearClipPlane = 0.01f;
            _projector.fieldOfView = 90.0f;
            _projector.cullingMask = LayerMask.GetMask("Scene") + LayerMask.GetMask("Character");
            //}
        }
        _projector.enabled = false;
        _projector.orthographic = false;
        _projector.useOcclusionCulling = true;
        //_projector.backgroundColor = Color.blue;

        //_OmniLight = GetComponent<Light>();
        _projector.farClipPlane = _light.range;// _OmniLight.range;
#if UNITY_EDITOR
        _LastOmniRange = _projector.farClipPlane = _light.range;
        _LastShadowDirection = ShadowDirection;
#endif

    }

    protected override void _DoInit()
    {
        DetermineRenderer(OMNI_RENDERINDEX);
        ///
        _AddShadow();
        _initCamera();
        _SyncAttr_DepthMapDimens();
        _SyncAttr_ShadowCamera();
        _UpdateShadowMatrices();
        _SyncAttr_ShadowMatrix();
        _SyncAttr_ShadowStrength();
        _SyncAttr_Inited();

        SyncDynamicShadowLayer(0);
        SyncStaticShadowLayer(0);
    }
    protected override void _SyncAttr_DepthMapDimens()
    {
        //
        if (_renderer == null)
        {
            return;
        }

        int size = (int)_DynamicShadowMapSize;
        for (int i = 0; i < 6; i++)
        {
            CustomShadowMgr.Instance.SetShadowSize(_light, i, (int)_StaticShadowMapSize, size);
        }
    }
    void _SyncAttr_ShadowStrength()
    {
        CustomShadowMgr.Instance.SetShadowStrength(_light, 0, _shadowStrength);
    }
    void _UpdateShadowMatrices()
    {
        _gpuProj = new Matrix4x4();
        _BuildPointShadowProjectionMatrix(ref _gpuProj);

        Matrix4x4 viewMatrix = new Matrix4x4();

        //_projector.transform.forward = Vector3.right;
        _BuildViewMatrix(transform.position, Quaternion.Euler(0,90,0),ref viewMatrix);
        _ShadowMatrices[0] = _gpuProj * viewMatrix;
        //
        //_projector.transform.forward = Vector3.left;
        _BuildViewMatrix(transform.position, Quaternion.Euler(0, -90, 0), ref viewMatrix);
        _ShadowMatrices[1] = _gpuProj * viewMatrix;
        //
        //_projector.transform.forward = Vector3.up;
        _BuildViewMatrix(transform.position, Quaternion.Euler(-90, 0, 0), ref viewMatrix);
        _ShadowMatrices[2] = _gpuProj * viewMatrix;
        //
        //_projector.transform.forward = Vector3.down;
        _BuildViewMatrix(transform.position, Quaternion.Euler(90, 0, 0), ref viewMatrix);
        _ShadowMatrices[3] = _gpuProj * viewMatrix;
        //
        //_projector.transform.forward = Vector3.forward;
        _BuildViewMatrix(transform.position, Quaternion.Euler(0, 0, 0), ref viewMatrix);
        _ShadowMatrices[4] = _gpuProj * viewMatrix;
        //
        //_projector.transform.forward = Vector3.back;
        _BuildViewMatrix(transform.position, Quaternion.Euler(0, 180, 0), ref viewMatrix);
        _ShadowMatrices[5] = _gpuProj * viewMatrix; 
        
        //_projector.transform.forward = Vector3.right;
        //_ShadowMatrices[0] = gpuProj * _projector.worldToCameraMatrix;
        //
        //_projector.transform.forward = Vector3.left;
        //_ShadowMatrices[1] = gpuProj * _projector.worldToCameraMatrix;
        //
        //_projector.transform.forward = Vector3.up;
        //_ShadowMatrices[2] = gpuProj * _projector.worldToCameraMatrix;
        //
        //_projector.transform.forward = Vector3.down;
        //_ShadowMatrices[3] = gpuProj * _projector.worldToCameraMatrix;
        //
        //_projector.transform.forward = Vector3.forward;
        //_ShadowMatrices[4] = gpuProj * _projector.worldToCameraMatrix;
        //
        //_projector.transform.forward = Vector3.back;
        //_ShadowMatrices[5] = gpuProj * _projector.worldToCameraMatrix;
    }
    void _SyncAttr_ShadowMatrix()
    {
        for(int i = 0;i < 6;i++)
        {
            CustomShadowMgr.Instance.SetStaticShadowMatrix(_light,i,_ShadowMatrices[i], _ShadowMatrices[i], _gpuProj, DepthBias_Depth, DepthBias_Normal,
                                                            _shadowStrength, DepthBias_Depth, DepthBias_Normal);
        }
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
            _LastShadowDirection != ShadowDirection ||
            DepthBias_Depth != _LastZBias_Depth ||
            DepthBias_Normal != _LastZBias_Normal ||
            ShadowColor != _LastShadowColor ||
            (_LastOmniRange != _light.range ||
            _projector.farClipPlane != _light.range))
        {
            _UpdateShadowMatrices();
            _SyncAttr_ShadowMatrix();
            //
            _LastCamPosition = gameObject.transform.position;
            _LastShadowDirection = ShadowDirection;
            _LastZBias_Depth = DepthBias_Depth;
            _LastZBias_Normal = DepthBias_Normal;
            _LastShadowColor = ShadowColor;
            _LastOmniRange = _projector.farClipPlane = _light.range;
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
        if (CaptureStaticLayeEveryFrame)
        {
            _SyncAttr_CaptureStaticLayer();
        }


    }
    private void OnDrawGizmos()
    {
        return;
        #region
        if (enabled && gameObject.activeInHierarchy)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _light.range);
            Gizmos.DrawSphere(transform.position, _light.range * 0.01f);

            float lineLength = _light.range * 0.1f;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * lineLength);
            Gizmos.DrawLine(transform.position, transform.position - Vector3.up * lineLength);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * lineLength);
            Gizmos.DrawLine(transform.position, transform.position - Vector3.forward * lineLength);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * lineLength);
            Gizmos.DrawLine(transform.position, transform.position - Vector3.right * lineLength);

        }
        #endregion
    }
    override public void CopyShadowData(CustomShadowBase shadow)
    {
        base.CopyShadowData(shadow);
        CustomOmniShadow oShadow = (CustomOmniShadow)shadow;
        //
        ShadowDirection = oShadow.ShadowDirection;
        //bool CaptureStaticLayeEveryFrame = false;
        ShadowColor = oShadow.ShadowColor;
        DepthBias_Depth = oShadow.DepthBias_Depth;
        DepthBias_Normal = oShadow.DepthBias_Normal;
        //for(int  i = 0; i < 6; i++)
        //{
            //_ShadowMatrices[i] = oShadow._ShadowMatrices[i];
        //}
        //_gpuProj = oShadow._gpuProj;
        /// <summary>
        ///Cache锟斤拷锟斤拷,editor锟斤拷
        /// </summary>
    #if UNITY_EDITOR
        _LastZBias_Depth = oShadow._LastZBias_Depth;
        _LastZBias_Normal = oShadow._LastZBias_Normal;
        _LastShadowColor  = oShadow._LastShadowColor;
        //影锟斤拷depthtexture锟斤拷染锟斤拷锟斤拷锟斤拷锟斤拷锟?
        //_LastCamPosition = oShadow._LastCamPosition;
        //_LastOmniRange = oShadow._LastOmniRange;
        _LastShadowDirection = oShadow._LastShadowDirection;
        _LastDynamicShadowMapSize = oShadow._LastDynamicShadowMapSize;
        _LastStaticShadowMapSize = oShadow._LastStaticShadowMapSize;
        _LastShadowStrength = oShadow._LastShadowStrength;
        m_LastStaticLayer = oShadow.m_StaticLayer;
        m_LastDynamicLayer = oShadow.m_DynamicLayer;
#endif
        //Light _OmniLight;
        //
        //_CacheProjMatrix = oShadow._CacheProjMatrix;
        //[HideInInspector]
        //bool _inited = false;
        //const int OMNI_RENDERINDEX = 8;

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
