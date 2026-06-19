using Sirenix.OdinInspector;
using StarProject.Service.Cam;
using StarProject.Service.Input;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace StarProject.Game.StarsCamera
{
    [System.Serializable]
    public struct GameCameraParam
    {
        public Vector3 AngleV3;
        public float Distance;
        public GameCameraParam(Vector3 angleV3, float distance)
        {
            AngleV3 = angleV3;
            Distance = distance;
        }
    }

    /// <summary>
    /// 缩放摄像机
    /// </summary>
    public class GameCameraScale : MonoBehaviour
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

        //private Vector2 lastSingleTouchPosition;
        //private Vector3 m_CameraOffset;
        //private Camera m_Camera;
        ////定义摄像机可以活动的范围
        //public float xMin = -100;
        //public float xMax = 100;
        //public float zMin = -100;
        //public float zMax = 100;

        public int index = 0;


        public List<GameCameraParam> GameCameraParams = new(){
            new GameCameraParam(new Vector3(41, 0, 0), 12)
            // new GameCameraParam(new Vector3(40, 0, 0), 12),
        };

        private bool CanFingers = false;

        protected void Awake()
        {
            InputManager.Instance.On_ScrollWheel += On_ScrollWheel;
            InputManager.Instance.On_DoubleFingersStart += On_DoubleFingersStart;
            InputManager.Instance.On_DoubleFingersMove += On_DoubleFingersMove;
            InputManager.Instance.On_DoubleFingersEnd += On_DoubleFingersEnd;

            GlobalEvent.OnVirtualCameraCreate.AddListener(OnVirtualCameraCreate);
            GlobalEvent.OnSwitchBattleCameraDefaultParam.AddListener(SwitchCameraDefalultParam);



            ScaleFactor = GameConfig.SCALE_FACTOR;
            DefaultDistance = GameConfig.DEFAULT_DISTANCE;
            MaxDistance = GameConfig.MAX_DISTANCE;
            ThresholdDistance = GameConfig.THRESHOLD_DISTANCE;
            MinDistance = GameConfig.MIN_DISTANCE;
            MaxAngleOfPitch = GameConfig.MAX_ANGLE_OF_PITCH;
            MinAngleOfPitch = GameConfig.MIN_ANGLE_OF_PITCH;

            distanceConfDiffer = ThresholdDistance - MinDistance;
            angleConfDiffer = MaxAngleOfPitch - MinAngleOfPitch;


            GameCameraParams.Insert(0, new GameCameraParam(new Vector3(MaxAngleOfPitch, 0, 0), DefaultDistance));
            index = 0;
        }

        //private void Start()
        //{
        //    //m_Camera = CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).Camera;
        //    //m_CameraOffset = Vector3.zero;
        //}

        //private void LateUpdate()
        //{
        //    var position = m_CameraOffset + m_Camera.transform.forward * -distance;
        //    transform.localPosition = position;
        //}

        private void OnDestroy()
        {
            InputManager.Instance.On_ScrollWheel -= On_ScrollWheel;
            InputManager.Instance.On_DoubleFingersStart -= On_DoubleFingersStart;
            InputManager.Instance.On_DoubleFingersMove -= On_DoubleFingersMove;
            InputManager.Instance.On_DoubleFingersEnd -= On_DoubleFingersEnd;

            GlobalEvent.OnVirtualCameraCreate.RemoveListener(OnVirtualCameraCreate);
            GlobalEvent.OnSwitchBattleCameraDefaultParam.RemoveListener(SwitchCameraDefalultParam);
        }

        private void OnVirtualCameraCreate(object arg0)
        {
            StarProject.Service.Cam.CameraManager.Instance.SetPlayerCameraAngleX(MaxAngleOfPitch);
            StarProject.Service.Cam.CameraManager.Instance.SetPlayerCameraDistance(DefaultDistance);
        }


        public void SwitchCameraDefalultParam(object arg)
        {
            index++;
            index = index < GameCameraParams.Count ? index : (index - GameCameraParams.Count);

            var cameraParam = GameCameraParams[index];

            StarProject.Service.Cam.CameraManager.Instance.SetPlayerCameraAngle(cameraParam.AngleV3.x, cameraParam.AngleV3.y, cameraParam.AngleV3.z);
            StarProject.Service.Cam.CameraManager.Instance.SetPlayerCameraDistance(cameraParam.Distance);
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

        private void On_DoubleFingersStart(Vector3 Finger1Position, Vector3 Finger2Position)
        {
            if (!IsUseDoubleFingersScale)
            {
                return;
            }

            if (GameInput.GetIsTouchDown)
            {
                return;
            }

            CanFingers = true;

            oldPosition1 = Finger1Position;
            oldPosition2 = Finger2Position;
        }

        private void On_DoubleFingersMove(Vector3 Finger1Position, Vector3 Finger2Position, float time)
        {
            if (!IsUseDoubleFingersScale)
            {
                return;
            }

            if (GameInput.GetIsTouchDown)
            {
                return;
            }

            if (!CanFingers)
            {
                return;
            }

            DoubleFingersScaleCamera(Finger1Position, Finger2Position);
        }

        private void On_DoubleFingersEnd(Vector3 Finger1Position, Vector3 Finger2Position)
        {
            CanFingers = false;
        }

        /// <summary> 双指触摸缩放摄像头 </summary>
        private void DoubleFingersScaleCamera(Vector3 tempPosition1, Vector3 tempPosition2)
        {
            //计算出当前两点触摸点的位置
            float currentTouchDistance = Vector3.Distance(tempPosition1, tempPosition2);
            float lastTouchDistance = Vector3.Distance(oldPosition1, oldPosition2);

            //计算上次和这次双指触摸之间的距离差距
            //然后去更改摄像机的距离
            CurDistance -= (currentTouchDistance - lastTouchDistance) * ScaleFactor * Time.deltaTime;
            //把距离限制住在min和max之间
            CurDistance = Mathf.Clamp(CurDistance, MinDistance, MaxDistance);
            SetPlayerCameraParam();

            //备份上一次触摸点的位置，用于对比
            oldPosition1 = tempPosition1;
            oldPosition2 = tempPosition2;
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

                StarProject.Service.Cam.CameraManager.Instance.SetPlayerCameraAngleX(angle);
            }
            else if (!M_IsDefauleCameraType)
            {
                // 进入默认摄像机参数
                M_IsDefauleCameraType = true;
                StarProject.Service.Cam.CameraManager.Instance.SetPlayerCameraAngleX(MaxAngleOfPitch);
            }
            StarProject.Service.Cam.CameraManager.Instance.SetPlayerCameraDistance(CurDistance);
        }

        //private void MoveCamera(Vector3 scenePos)
        //{
        //    // TODO: 曲 目前没有移动摄像的需求
        //    // 主角一定在摄像机的最中间
        //    return;
        //    Vector3 lastTouchPostion = m_Camera.ScreenToWorldPoint(new Vector3(lastSingleTouchPosition.x, lastSingleTouchPosition.y, -1));
        //    Vector3 currentTouchPosition = m_Camera.ScreenToWorldPoint(new Vector3(scenePos.x, scenePos.y, -1));

        //    Vector3 v = currentTouchPosition - lastTouchPostion;
        //    m_CameraOffset += new Vector3(v.x, 0, v.z) * transform.localPosition.y;

        //    //把摄像机的位置控制在范围内
        //    m_CameraOffset = new Vector3(Mathf.Clamp(m_CameraOffset.x, xMin, xMax), m_CameraOffset.y, Mathf.Clamp(m_CameraOffset.z, zMin, zMax));
        //    //SGF.Debuger.Log(lastTouchPostion + "|" + currentTouchPosition + "|" + v);
        //    lastSingleTouchPosition = scenePos;
        //}
    }
}

