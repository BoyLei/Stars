using Cinemachine;
using SGF;
using SGF.Module.Framework;
using SGF.Unity;
using SGF.Utlis;
using StarProject.Game;
using StarProject.Game.StarsCamera;
using StarProject.Service.Cam.Data;
using StarProject.Service.Input;
using StarProjectDef;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Cinemachine.CinemachineBlenderSettings;
using UnityEngine.UI;

namespace StarProject.Service.Cam
{
    /// <summary>
    /// 注册和管理移动节点
    /// 规划只会出现三个摄像机
    /// </summary>
    public class CameraManager : ServiceModule<CameraManager>, ICollection<KeyValuePair<E_CameraType, CameraBase>>//这里不是IRegistrable，而是本类要的具体实现了；我只要这种具体类型的，并且实现了具体类型的add，来限制;抽象部分查看IRegistrable
    {
        private string flagKey = "[CameraManager]";
        //非UI
        public CameraBase CurrentPlayCamera;
        /// <summary>
        /// Import result object caching
        /// </summary>
        public Dictionary<E_CameraType, CameraBase> M_CameraMap = new();

        public int Count => throw new System.NotImplementedException();

        public bool IsReadOnly => throw new System.NotImplementedException();

        private int cameraEventID = 0;

        #region 虚拟相机配置

        private string VirtualCameraPath = "Properties/VirtualCamera/PlayerCamera";
        private CinemachineVirtualCamera VirtualCamera;
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

        public static float CurDistance = 10f;   // 当前距离

        #endregion

        private Vector3 PlayerDefaultOffsetPos = Vector3.zero;



        public int GetCameraEventID()
        {
            return ++cameraEventID;
        }

        public void Init()
        {
            CheckSingleton();

            GlobalEvent.OnCameraEvent.AddListener(OnCameraEvent);
            GlobalEvent.OnCameraStopEvent.AddListener(StopCamFxSync);
            GlobalEvent.OnModifyCameraHeight.AddListener(OnOnModifyCameraHeight);

            InputManager.Instance.On_Swipe_Start += OnSwipeStartHandle;
            InputManager.Instance.On_Swipe += OnSwipeHandle;
            InputManager.Instance.On_Swipe_End += OnSwipeEndHandle;

            CurDistance = GameConfig.DEFAULT_DISTANCE;
            CreateVirtualCamera();
        }

        #region 主虚拟相机

        // 创建虚拟主相机
        private void CreateVirtualCamera()
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(VirtualCameraPath,
            (GameObject go) =>
            {
                if (go == null)
                {
                    return;
                }
                var gob = GameObject.Instantiate<GameObject>(go);
                VirtualCamera = gob.GetComponent<CinemachineVirtualCamera>();
                VirtualCamera.gameObject.name = "PlayerCamera";
                VirtualCamera.gameObject.SetActive(false);
                VirtualCamera.transform.SetParent(EntityRoot.Instance.DotRemoveRoot.transform);
                M_Transposer = VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (M_Transposer != null)
                {
                    M_Transposer.m_MinimumDistance = GameConfig.THRESHOLD_DISTANCE;
                    M_Transposer.m_MaximumDistance = GameConfig.MAX_DISTANCE;
                    M_Transposer.m_CameraDistance = CurDistance;
                }
                GlobalEvent.OnVirtualCameraCreate?.Invoke(null);
            });
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

        public void SetPlayerCameraAngle(float angleX, float angleY = 0, float angleZ = 0)
        {
            if (VirtualCamera != null)
            {
                // float t= transposer.m_CameraDistance +offset;
                Vector3 localEulerAngles = Vector3.zero;
                localEulerAngles.Set(angleX, angleY, angleZ);
                VirtualCamera.transform.localEulerAngles = localEulerAngles;
            }
        }

        public void SetPlayerCameraAngleX(float angleX, bool isFroce = false)
        {
            if (VirtualCamera != null)
            {
                if (CinemachineCore.Instance.IsLive(VirtualCamera) || isFroce)
                {
                    // float t= transposer.m_CameraDistance +offset;
                    Vector3 localEulerAngles = Vector3.zero;
                    localEulerAngles.Set(angleX, VirtualCamera.transform.localEulerAngles.y, VirtualCamera.transform.localEulerAngles.z);
                    VirtualCamera.transform.localEulerAngles = localEulerAngles;
                }
            }
        }

        public void SetPlayerCameraAngleY(float angleY, bool isFroce = false)
        {
            if (VirtualCamera != null)
            {
                if (CinemachineCore.Instance.IsLive(VirtualCamera) || isFroce)
                {
                    // float t= transposer.m_CameraDistance +offset;
                    Vector3 localEulerAngles = Vector3.zero;
                    localEulerAngles.Set(VirtualCamera.transform.localEulerAngles.x, /*VirtualCamera.transform.localEulerAngles.y +*/ angleY, VirtualCamera.transform.localEulerAngles.z);
                    VirtualCamera.transform.localEulerAngles = localEulerAngles;
                    //SGF.Debuger.LogError($"点击滑动 y={VirtualCamera.transform.localEulerAngles.y}");
                }
            }
        }

        public Vector3 GetPlayerCameraAnglesY()
        {
            Vector3 dir = Vector3.zero;
            if (VirtualCamera != null && CinemachineCore.Instance.IsLive(VirtualCamera))
            {
                dir.y = VirtualCamera.transform.rotation.eulerAngles.y;
            }
            return dir;
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

        public Vector3 GetVirtualCameraOffset()
        {
            if (VirtualCamera == null)
            {
                return Vector3.zero;
            }
            CinemachineCameraOffset offset = GameObjectUtils.EnsureComponent<CinemachineCameraOffset>(VirtualCamera.gameObject);
            if (offset == null)
            {
                return Vector3.zero;
            }
            return offset.m_Offset - PlayerDefaultOffsetPos;
        }

        /// <summary>
        /// 设置虚拟相机跟随目标
        /// </summary>
        /// <param name="transform"></param>
        public void SetPlayerFlowTarget(UnityEngine.Transform transform)
        {
            if (VirtualCamera != null)
            {
                VirtualCamera.Follow = transform;
                GameManager.Instance.GetGameCameraComponent().M_CinemachineBrain.m_DefaultBlend.m_Time = 0;
                VirtualCamera.gameObject.SetActive(true);
                GameManager.Instance.GetGameCameraComponent().M_CinemachineBrain.m_DefaultBlend.m_Time = 2;
            }
        }

        public void ReleasePlayerFlowTarget()
        {
            if (VirtualCamera != null)
            {
                VirtualCamera.Follow = null;
                VirtualCamera.gameObject.SetActive(false);
            }
        }

        private void OnOnModifyCameraHeight(float height)
        {
            if (M_Transposer != null)
            {
                //CurDistance = height;
                M_Transposer.m_CameraDistance = height;
            }
        }


        #region 主相机旋转

        private float targetX = 0f;
        private float targetY = 0f;
        private float MoveXSpeed = 0.1f;

        private Vector3 LastPos;    // 手指初始点击坐标
        private bool isStartDrag = false;

        private void OnSwipeStartHandle(Vector3 position)
        {
            if (!GameManager.Instance.GetIsCanRotateCamera())
            {
                return;
            }
            if (InputManager.Instance.IsForbidInpitTouch(true))
            {
                return;
            }
            LastPos = position;
            isStartDrag = true;
        }

        private void OnSwipeHandle(Vector3 position, float holdtime)
        {
            if (!GameManager.Instance.GetIsCanRotateCamera())
            {
                return;
            }
            if (!isStartDrag)
            {
                return;
            }
            if (InputManager.Instance.IsForbidInpitTouch(true))
            {
                isStartDrag = false;
                return;
            }
            float x = targetX + ((position.x - LastPos.x) * MoveXSpeed);
            if (x != targetX)
            {
                targetX = x;
                SetPlayerCameraAngleY(targetX);
            }

            float y = targetY - ((position.y - LastPos.y) * MoveXSpeed);
            y = y <= GameConfig.MIN_MANUAL_ANGLE_OF_PITCH ? GameConfig.MIN_MANUAL_ANGLE_OF_PITCH : y;
            y = y >= GameConfig.MAX_MANUAL_ANGLE_OF_PITCH ? GameConfig.MAX_MANUAL_ANGLE_OF_PITCH : y;
            if (y != targetY)
            {
                targetY = y;
                SetPlayerCameraAngleX(targetY);
            }

            LastPos = position;
        }

        private void OnSwipeEndHandle(Vector3 position)
        {
            LastPos = Vector3.zero;
            isStartDrag = false;
        }

        public void CloseCameraRotate()
        {
            LastPos = Vector3.zero;
            isStartDrag = false;
        }

        #endregion

        #endregion

        public CameraBase GetCamera(E_CameraType e_CameraType)
        {
            CameraBase CamGob = null;
            if (M_CameraMap.TryGetValue(e_CameraType, out CamGob))
            {
                //未找到相机
            }
            return CamGob;
        }

        /// <summary>
        /// UI必须开启，渲染相机也要开启
        /// 同组切换的目前只有BattleCamera
        /// </summary>
        /// <param name="openCamera"></param>
        /// <returns></returns>
        public GameObject SwitchCamera(E_CameraType openCamera)
        {
            GameObject cam = new();
            foreach (var item in M_CameraMap)
            {
                if (item.Key == E_CameraType.UICam)//--
                {
                    continue;
                    //UICam保持开放
                }
                if (item.Key == E_CameraType.SpecialCam)
                {
                    continue;
                    //UICam保持开放
                }
                //---------------------------------------------
                cam = item.Value.gameObject;
                cam.SetActive(item.Key == openCamera);
                if (item.Key == openCamera)
                {
                    CurrentPlayCamera = item.Value;
                }

            }
            return cam;
        }

        public void OnCameraEvent(int cameraEventID, int cameraEffectId, E_CameraEffectType cameraEffectType, UnityEngine.Vector3 pos)
        {
            SGF.Debuger.Log($"{flagKey} OnCameraEvent cameraEffectId {cameraEffectId} cameraType {cameraEffectType}");

            E_CameraType cameraType = E_CameraType.StarWorldCam;

            // //1.判断是否在屏幕范围内 或者 是否在视野范围内
            CameraBase cameraBase = GetCamera(cameraType);
            if (!cameraBase)
            {
                return;
            }
            bool isInCameraView = CommonFunction.Is3DPointInsideCameraView(cameraBase.Camera, pos);
            if (!isInCameraView)
            {
                SGF.Debuger.LogError($"{flagKey} OnCameraEvent cameraEffectId {cameraEffectId} cameraType {cameraEffectType} , Camera {cameraBase.Camera.transform.position.ToString()} , pos : {pos.ToString()}");
                return;
            }


            //2.生成对应的camera效果参数，传递给对应的摄像机
            CameraFxParam cameraFxParam = new();
            cameraFxParam.Init(cameraEffectId, cameraEffectType);
            DoCamFxSync(new List<CameraFxParam>() { cameraFxParam }, cameraType, cameraEventID);
        }

        public bool IsCameraPoint(E_CameraType cameraType, Vector3 pos)
        {
            CameraBase cameraBase = GetCamera(cameraType);
            if (!cameraBase)
            {
                return false;
            }
            bool isInCameraView = CommonFunction.Is3DPointInsideCameraView(cameraBase.Camera, pos);
            if (!isInCameraView)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 这里不会有多种游戏类型：根据不同得场景，不同得游戏模式，选取不同得相机；所以我们简单点直接指定相机
        /// </summary>
        /// <param name="cameraFxParams"></param>
        /// <param name=""></param>
        public void DoCamFxSync(List<CameraFxParam> cameraFxParams, E_CameraType e_CameraType, int cameraEventID)
        {
            //同步处理多个效果
            //这里不用栈，也不用Queue，更不必池，也不必处理空引用得列表
            //全部转交给DG和Fell来调用
            CameraBase cameraBase = GetCamera(e_CameraType);
            if (!cameraBase)
            {
                return;
            }

            cameraBase.DoCameraAction(cameraFxParams, cameraEventID);
        }

        public void StopCamFxSync(int cameraEventID, CameraEvent cameraEvent, E_CameraType e_CameraType)
        {
            //同步处理多个效果
            //这里不用栈，也不用Queue，更不必池，也不必处理空引用得列表
            //全部转交给DG和Fell来调用
            CameraBase cameraBase = GetCamera(e_CameraType);
            if (!cameraBase)
            {
                return;
            }
            cameraBase.StopCameraAction(cameraEvent, cameraEventID);
        }

        public void DoCameraFxStep()
        {

            //Camera.MM/DG
        }

        public void Add(KeyValuePair<E_CameraType, CameraBase> item)
        {
            if (item.Key != null && item.Value != null)
            {
                M_CameraMap.Add(item.Key, item.Value);
                this.Log("RegCam:" + item.Key + "___" + item.Value.name);
            }
        }
        public bool Remove(KeyValuePair<E_CameraType, CameraBase> item)
        {
            bool rem = true;
            if (M_CameraMap.ContainsKey(item.Key))
            {
                rem = M_CameraMap.Remove(item.Key);

            }
            return rem;
        }
        public void Clear()
        {
            M_CameraMap.Clear();
        }
        public bool Contains(KeyValuePair<E_CameraType, CameraBase> item)
        {
            return M_CameraMap.ContainsKey(item.Key);
        }
        public void CopyTo(KeyValuePair<E_CameraType, CameraBase>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<E_CameraType, CameraBase>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }


        #region 设置RawCamera

        private int m_ShowRawCameraCount = 0;
        public void SetRawCamrea(bool isShow)
        {
            if (isShow)
            {
                ShowRawCamera();
            }
            else
            {
                HideRawCamera();
            }
        }
        private void HideRawCamera()
        {
            m_ShowRawCameraCount--;
            if (m_ShowRawCameraCount == 0)
            {
                CameraBase cameraBase = GetCamera(E_CameraType.SpecialCam);
                if (cameraBase != null)
                {
                    if (cameraBase is RawCamera)
                    {
                        RawCamera rc = (RawCamera)cameraBase;
                        rc.SetState(false);
                    }


                }
            }
        }

        private void ShowRawCamera()
        {
            m_ShowRawCameraCount++;
            if (m_ShowRawCameraCount == 1)
            {
                CameraBase cameraBase = GetCamera(E_CameraType.SpecialCam);
                if (cameraBase != null)
                {
                    if (cameraBase is RawCamera)
                    {
                        RawCamera rc = (RawCamera)cameraBase;
                        rc.SetState(true);
                    }

                };
            }
        }

        #endregion

        #region 设置BattleCamera

        public void SetBattlePos(Vector3 localRotate, Vector3 localPos)
        {
            CameraBase cameraBase = GetCamera(E_CameraType.StarWorldCam);
            if (cameraBase != null)
            {
                cameraBase.transform.localEulerAngles = localRotate;
                cameraBase.transform.localPosition = localPos;
            }
        }


        //统一接口调我：静帧ui调我屏蔽关闭相机的时候内存下降
        //其他关闭可能还需要下降呢
        public void SetBattleCameraShowType(bool active, bool callMethodFromStaticFrameUI)
        {
            CameraBase cameraBase = GetCamera(E_CameraType.StarWorldCam);
            if (cameraBase != null)
            {
                //cameraBase.Camera.enabled = active;//其他组件保证运行
                cameraBase.Camera.cullingMask = active ? cameraBase.DefCullingMask : 0;
                if (callMethodFromStaticFrameUI)
                {
                    //关闭相机，是否可以降低内存预算,还是说不想降低保持内存预算
                    RenderManager.Instance.AllowMemoryDecline = active;
                }
            }
        }

        public float GetBlendForVirtualCamerasOutTime(string fromCameraName, string toCameraName)
        {
            float outTime = 0f;
            Camera camera = GetCamera(E_CameraType.StarWorldCam).Camera;
            if (camera != null)
            {
                CinemachineBrain cinemachineBrain = camera.GetComponent<CinemachineBrain>();
                if (cinemachineBrain != null)
                {
                    bool isFind = false;
                    if (cinemachineBrain.m_CustomBlends != null)
                    {
                        for (int i = 0; i < cinemachineBrain.m_CustomBlends.m_CustomBlends.Length; ++i)
                        {
                            CustomBlend blendParams = cinemachineBrain.m_CustomBlends.m_CustomBlends[i];
                            if (blendParams.m_From == fromCameraName && (blendParams.m_To == toCameraName || blendParams.m_To == kBlendFromAnyCameraLabel))
                            {
                                outTime = blendParams.m_Blend.BlendTime;
                                isFind = true;
                                break;
                            }
                        }
                    }
                    if (!isFind)
                    {
                        outTime = cinemachineBrain.m_DefaultBlend.m_Time;
                    }
                }
            }
            return outTime;
        }

        private void SetUICameraRenderType(bool active)
        {
            //设置UI相机的renderType
            CameraBase uiCam = GetCamera(E_CameraType.UICam);//--
            if (uiCam != null)
            {
                if (active)
                {
                    uiCam.Camera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
                }
                else
                {
                    //之前这么改是因为缺少一个主相机嘛。因为隐藏了主相机，现在不能隐藏（性能？通过其他方式了就要）
                    uiCam.Camera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
                }
            }
        }


        public void SwitchBattleCameraShowState(bool active, bool callMethodFromStaticFrameUI)
        {
            SetBattleCameraShowType(active, callMethodFromStaticFrameUI);
            //现在控制他没用了，因为没有ui相机，主相机不能隐藏
            //SetUICameraRenderType(active);

        }
        #endregion

    }









}
