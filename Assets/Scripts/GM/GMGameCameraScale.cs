using Cinemachine;
using SGF.Unity;
using Sirenix.OdinInspector;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 缩放摄像机
/// </summary>
public class GMGameCameraScale : MonoBehaviour
{
    //private string LOG_TAG = "[GameCameraScale]";

    ///---------------------------------- 
    [LabelText("是否使用鼠标缩放")]
    public bool IsUseMouseScale = false;
    [LabelText("是否使用双指缩放")]
    public bool IsUseDoubleFingersScale = false;
    [LabelText("缩放系数")]
    public float ScaleFactor = 1f;

    [LabelText("默认距离")]
    [Range(GameConfig.MIN_DISTANCE, GameConfig.MAX_DISTANCE)]
    public float DefaultDistance = 10f;
    [LabelText("最大距离")]
    public float MaxDistance = 17f;
    [LabelText("阀值距离")]
    public float ThresholdDistance = 7f;
    [LabelText("最小距离")]
    public float MinDistance = 3f;
    [LabelText("当前距离")]
    [Range(GameConfig.MIN_DISTANCE, GameConfig.MAX_DISTANCE)]
    public float CurDistance = 10f;

    [LabelText("最大俯视角")]
    public float MaxAngleOfPitch = 43f;
    [LabelText("最小俯视角")]
    public float MinAngleOfPitch = 25f;
    ///---------------------------------- 

    public CinemachineVirtualCamera VirtualCamera;

    private CinemachineFramingTransposer m_Transposer;
    private CinemachineFramingTransposer M_Transposer
    {
        get
        {
            if (VirtualCamera != null && m_Transposer == null)
            {
                m_Transposer = VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
            return m_Transposer;
        }
        set
        {
            m_Transposer = value;
        }
    }

    ///---------------------------------- 

    // 是否默认相机类型
    // true:默认视角 角度 41
    // false:观察者视角 角度 41-25
    private bool m_IsDefauleCameraType = true;
    private bool M_IsDefauleCameraType
    {
        get
        {
            return m_IsDefauleCameraType;
        }
        set
        {
            if (m_IsDefauleCameraType != value)
            {
                m_IsDefauleCameraType = value;
            }
        }
    }

    // 角度、距离 配置差
    private float distanceConfDiffer = 1f;
    private float angleConfDiffer = 1f;

    //记录上一次手机触摸位置判断用户是在左放大还是缩小手势
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;


    /// /------------------------
    private Vector3 PlayerDefaultOffsetPos = Vector3.zero;

    //private int maxDistance = 100;
    private float mHoldingTime;
    private bool isDown = false;
    private bool isDownScrollWheel = false;
    private float mLastTapTime = 0;
    private bool isMouseRightDown = false;

    /// /------------------------



    protected void Awake()
    {
        ScaleFactor = GameConfig.SCALE_FACTOR;
        DefaultDistance = GameConfig.DEFAULT_DISTANCE;
        MaxDistance = GameConfig.MAX_DISTANCE;
        ThresholdDistance = GameConfig.THRESHOLD_DISTANCE;
        MinDistance = GameConfig.MIN_DISTANCE;
        MaxAngleOfPitch = GameConfig.MAX_ANGLE_OF_PITCH;
        MinAngleOfPitch = GameConfig.MIN_ANGLE_OF_PITCH;

        distanceConfDiffer = ThresholdDistance - MinDistance;
        angleConfDiffer = MaxAngleOfPitch - MinAngleOfPitch;
    }

    private void Start()
    {
        M_Transposer = VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (M_Transposer != null)
        {
            M_Transposer.m_MinimumDistance = GameConfig.THRESHOLD_DISTANCE;
            M_Transposer.m_MaximumDistance = GameConfig.MAX_DISTANCE;
            M_Transposer.m_CameraDistance = CurDistance;
        }

        SetPlayerCameraOffect(new Vector3(0, 1.01f, 0));
    }

    protected void Update()
    {
        WindowsPlatforms();
    }

    private void WindowsPlatforms()
    {
        Vector3 mousePosition = UnityEngine.Input.mousePosition;
        if (UnityEngine.Input.GetMouseButtonUp(0))
        {
            isDown = false;
        }

        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            if (isDown)
            {
                mHoldingTime += UnityEngine.Time.deltaTime;
            }
            else
            {
                if (CheckGuiRaycastObjects())
                {
                    return;
                }

                isDown = true;
                mHoldingTime = 0;
                if (UnityEngine.Time.realtimeSinceStartup - mLastTapTime <= 0.5f)
                {
                    
                }
                mLastTapTime = UnityEngine.Time.realtimeSinceStartup;
            }
        }

        if (UnityEngine.Input.GetMouseButtonUp(2))
        {
            isDownScrollWheel = false;
        }
        if (UnityEngine.Input.GetMouseButtonDown(2))
        {
            if (!isDownScrollWheel)
            {
                isDownScrollWheel = true;
            }
        }

        if (isDownScrollWheel)
        {
            On_ScrollWheel(UnityEngine.Input.GetAxis("Mouse ScrollWheel"));
        }

        // 监听--鼠标右键
        if (UnityEngine.Input.GetMouseButtonUp(1))
        {
            isMouseRightDown = false;
        }
        if (UnityEngine.Input.GetMouseButtonDown(1))
        {
            if (!isMouseRightDown)
            {
                isMouseRightDown = true;
            }
        }
    }

    public bool CheckGuiRaycastObjects()
    {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
if (EventSystem.current.IsPointerOverGameObject(UnityEngine.Input.GetTouch(0).fingerId))
            {
            return true;
            }
#else
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
#endif

        return false;
    }

    private void On_ScrollWheel(float axis)
    {
        if (IsUseMouseScale)
        {
            CurDistance -= axis * ScaleFactor;
            CurDistance = Mathf.Clamp(CurDistance, MinDistance, MaxDistance);
            SetPlayerCameraParam();
        }
    }

    private void SetPlayerCameraParam()
    {
        if (CurDistance < ThresholdDistance)
        {
            // 进入观察摄像机参数
            M_IsDefauleCameraType = false;

            float ratio = 0f;
            if (CurDistance != MinDistance)
            {
                float differ = ThresholdDistance - CurDistance;
                ratio = 1 - (differ / distanceConfDiffer);
            }
            float angle = MinAngleOfPitch + angleConfDiffer * ratio;

            SetPlayerCameraAngle(angle);
        }
        else if (!M_IsDefauleCameraType)
        {
            // 进入默认摄像机参数
            M_IsDefauleCameraType = true;
            SetPlayerCameraAngle(MaxAngleOfPitch);
        }
        SetPlayerCameraDistance(CurDistance);
    }

    /// <summary>
    /// 设置虚拟相机的俯视角【X轴】
    /// </summary>
    /// <param name="distance"></param>
    public void SetPlayerCameraAngle(float angle)
    {
        if (VirtualCamera != null)
        {
            // float t= transposer.m_CameraDistance +offset;
            Vector3 localEulerAngles = Vector3.zero;
            localEulerAngles.x = angle;
            VirtualCamera.transform.localEulerAngles = localEulerAngles;
        }
    }

    /// <summary>
    /// 设置虚拟相机的距离【高度】
    /// </summary>
    /// <param name="distance"></param>
    public void SetPlayerCameraDistance(float distance)
    {
        if (M_Transposer != null)
        {
            CurDistance = distance;
            // float t= transposer.m_CameraDistance +offset;
            M_Transposer.m_CameraDistance = CurDistance;
        }
    }

    /// <summary>
    /// 设置虚拟相机的中心点偏移量【Y轴】
    /// </summary>
    /// <param name="offsetPos"></param>
    public void SetPlayerCameraOffect(Vector3 offsetPos)
    {
        PlayerDefaultOffsetPos = offsetPos;
        UpdateVirtualCameraOffset(Vector3.zero);
    }

    /// <summary>
    /// 更新 当前 虚拟相机的 偏移
    /// </summary>
    /// <param name="offsetPos"></param>
    public void UpdateVirtualCameraOffset(Vector3 offsetPos)
    {
        if (VirtualCamera == null)
        {
            return;
        }
        CinemachineCameraOffset offset = GameObjectUtils.EnsureComponent<CinemachineCameraOffset>(VirtualCamera.gameObject);
        if (offset == null)
        {
            return;
        }
        offset.m_Offset = offsetPos + PlayerDefaultOffsetPos;
    }
}

