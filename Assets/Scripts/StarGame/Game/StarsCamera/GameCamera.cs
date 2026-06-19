using Cinemachine;
using DG.Tweening;
using Koenigz.PerfectCulling;
using SGF.Unity;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Service.Battle;
using StarProject.Service.Cam;
using StarProject.Service.Cam.Data;
using StarProject.Service.WorldToUI;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using static StarProject.Service.Battle.BattleManager;

namespace StarProject.Game.StarsCamera
{

    public class GameCamera : CameraBase
    {

        private const string flagKey = "[GameCamera]";
        //public static GameCamera Current;
        //public static Camera MainCamera;

        public static ulong FocusPlayerId = 0;
        private GameContext m_context;
        [SerializeField]
        private Transform M_myParentRoot;
        //DynamicCamRoot是放一堆相机的
        //BCameraRoot 才是控制节点（段磊控制这个）TODO
        public const float SEC_PER_FRAME = 1f / GameConfig.FIX_TIME_PER_SEC; //每帧 0.03333秒

        public GameCameraFeel gameCameraFeel;

        private CinemachineBrain m_CinemachineBrain;
        public CinemachineBrain M_CinemachineBrain => m_CinemachineBrain;

        private StreamingController sc;
        private PerfectCullingCamera perfectCullingCamera;

        private Vector3 CurCameraPos = Vector3.zero;

        protected override void OnAwake()
        {
            //AwakeTodo
            if (M_myParentRoot == null)
            {
                M_myParentRoot = transform.parent;//（段磊控制这个）TODO
            }

            SetCameraHeight(GameConfig.DEFAULT_DISTANCE);
            EnsureCameraFeel();
            sc = GetComponent<StreamingController>();
            perfectCullingCamera = GetComponent<PerfectCullingCamera>();
            //无效且基于全局
            //sc.streamingMipmapBias = 2;
            GlobalEvent.OnCameraAwake?.Invoke(GetCameraType());
            //取消预加载 中止预加载。
            //是预加载 用于查明流处理器当前是否正在预加载纹理 mipmap。
            //设置预加载 启动此摄像机的流数据的预加载。

            /*            超时秒 停止预加载前的可选超时。无需超时时设置为 0.0f。
            激活相机开启超时 设置为 True 可在超时到期时激活连接的摄像机组件。
            禁用摄像机剪切自 相机在超时时停用（如果 Camera.activateCameraOnTime 为 True）。此参数可以为空。*/
            //AOI不在视觉范围内影响也不大
            perfectCullingCamera.NeighborCellIncludeRadius = GameConfig.QualityForCameraClipNeighbor;
            sc.SetPreloading(GameConfig.SEC_RATE, true);
            //m_CameraOffset = m_Camera.transform.localPosition;

            Camera.allowDynamicResolution = GameConfig.AllowDynamicResolution;

            DefCullingMask = Camera.cullingMask;
            m_CinemachineBrain = transform.GetComponent<CinemachineBrain>();
        }

        // 当一个VirtualCamera变为Live状态时触发，若带有混合，则触发在混合开始的第一帧。
        //第一个参数为新的Live状态的VirtualCamera，第二个参数为上一个Live状态的VirtualCamera
        //public void CameraActivatedEvent(ICinemachineCamera liveCamera, ICinemachineCamera lastCamera)
        //{
        //    float blendTime = 0;    // 俩个相机切换的移动时间（秒）
        //    bool isChanageOther = liveCamera.Name != "PlayerCamera";
        //    if (m_CinemachineBrain != null)
        //    {
        //        blendTime = m_CinemachineBrain.m_DefaultBlend.BlendTime;
        //        //Debug.Log($"切换时间=》{m_CinemachineBrain.m_DefaultBlend.BlendTime}");
        //    }
        //    // 通知头顶信息节点要一直刷新了
        //    if (isChanageOther)
        //    {
        //        // 通知一直刷新
        //        WorldItemChecker.Instance.SetTimeLineSate(isChanageOther);
        //    }
        //    else
        //    {
        //        // 通知关闭了吧
        //        if (DelayInvoker.ContainInvoke(flagKey))
        //        {
        //            DelayInvoker.CancelInvoke(flagKey);
        //        }
        //        DelayInvoker.DelayInvoke(flagKey, blendTime, DelayShowModel, new object[] { });
        //    }
        //}

        //public void DelayShowModel(object[] args)
        //{
        //    WorldItemChecker.Instance.SetTimeLineSate(false);
        //}

        // 当一个VirtualCamera变为Live状态，并且其混合方式为Cut的情况下触发
        public void CameraCutEvent(CinemachineBrain brain)
        {
            Debug.Log(brain);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            return;
            if (!BattleManager.Instance.IsSercetSpace())
            {
                return;
            }
            var mapId = GameManager.Instance.M_Map.GetMapId();

            if (!BattleManager.Instance.secretMap2MonsterPathPoint.ContainsKey(mapId))
            {
                return;
            }

            // 前面已经做了判断，所以此处 地图对应的 怪物生成点一定能够找到
            List<MonsterPathPoint> secretMonsterPoint = BattleManager.Instance.secretMap2MonsterPathPoint[mapId];

            Gizmos.color = Color.red;
            for (int i = 1; i < secretMonsterPoint.Count; i++)
            {
                Gizmos.DrawLine(secretMonsterPoint[i - 1].Point, secretMonsterPoint[i].Point);
            }
        }

#endif

        private void EnsureCameraFeel()
        {
            gameCameraFeel = M_myParentRoot.AddComp<GameCameraFeel>();

            // GameObjectUtils.EnsureComponent<MMWiggle>(gameObject);

            // GameObjectUtils.EnsureComponent<MMCameraShaker>(gameObject);
            // GameObjectUtils.EnsureComponent<MMCameraZoom>(gameObject);

            // MMWiggle mMWiggle = gameObject.GetComponent<MMWiggle>();
            // mMWiggle.PositionActive = true;
            // mMWiggle.RotationActive = true;
            // mMWiggle.ScaleActive = true;

        }

        protected override E_CameraType GetCameraType()
        {
            return E_CameraType.StarWorldCam;
        }

        public static void Create()
        {
            //Camera c = GameObject.FindObjectOfType<Camera>();
            //if (c != null)
            //{
            //	GameObjectUtils.EnsureComponent<GameCamera>(c.gameObject);
            //}
            //else
            //{
            //	Debuger.LogError("GameCamera", "Create() Cannot Find Camera In Scene!");
            //}

        }

        public static void Release()
        {
            //if (Current != null)
            //{
            //	GameObject.Destroy(Current);
            //	Current = null;
            //}
        }
        private Vector3 _v3lpos;
        public void SetCameraHeight(float height)
        {
            //角2弧
            //向下看时，角度越小，距离/高度接近无限大，
            //所以才是CoTan，要不就是Tan （90-Angel）
            float z = Mathf.Abs(Mathf.Tan((90 - transform.eulerAngles.x) * Mathf.Deg2Rad) * height) * -1f;
            _v3lpos.x = 0f;
            _v3lpos.y = height;
            _v3lpos.z = z;
            //初始设置一次，有动画设置一次，没必要运动设置
            transform.localPosition = _v3lpos;

        }

        public override void StopCameraAction(CameraEvent cameraEvent, int cameraEventID)
        {
            /*gameCameraFeel.StopCameraAction(cameraEvent, cameraEventID);*/
        }


        /// <summary>
        /// 通用的Camera行为接口
        /// 通过cameraFxParam 的参数，去执行不同的行为
        /// note :
        ///     因为通过CameraFxParam 将多个camera的行为统一归类为 float[] paras中，
        ///     所以就需要约定好 不同类型数据 各个位置 对应数据 的生成和解析方式
        /// </summary>
        /// <param name="cameraFxParams"></param>
        /// <returns></returns>
        public override void DoCameraAction(List<CameraFxParam> cameraFxParams, int cameraEventID)
        {
            /*gameCameraFeel.DoCameraAction(cameraFxParams, cameraEventID);*/
        }

        void Start()
        {
            //Current = this;
            //MainCamera = this.GetComponent<Camera>();
            m_context = GameManager.Instance.Context;
        }
        /// <summary>
        /// 方法1:驱动之源
        /// </summary>
        //void Update()
        //{
        //    if (GameManager.Instance.IsRunning)
        //    {
        //        PlayerCtrlGroup player = GameManager.Instance.GetPlayer(FocusPlayerId);
        //        if (player != null)
        //        {
        //            //逻辑位置，是服务器位置
        //            //控制相机关注的一定是：1客户端中，2任意玩家的主角，3并未其他
        //            Vector3 targetPos = player.M_Curr.Position();
        //            //targetPos = m_context.EntityToViewPoint(targetPos);

        //            //Vector3 pos = this.transform.position;
        //            //pos.x = targetPos.x;
        //            //pos.y = targetPos.y;
        //            //this.transform.position = pos;
        //            SetCameraPos(targetPos);
        //        }
        //    }
        //}

        public void UpdateCameraPos(Vector3 targetPos)
        {
            //if (GameManager.Instance.IsRunning)
            {
                //PlayerCtrlGroup player = GameManager.Instance.GetPlayer(FocusPlayerId);
                //if (player != null)
                {
                    //逻辑位置，是服务器位置
                    //控制相机关注的一定是：1客户端中，2任意玩家的主角，3并未其他
                    //Vector3 targetPos = player.M_Curr.Position();
                    //targetPos = m_context.EntityToViewPoint(targetPos);

                    //Vector3 pos = this.transform.position;
                    //pos.x = targetPos.x;
                    //pos.y = targetPos.y;
                    //this.transform.position = pos;
                    // SetCameraPos(targetPos, false);
                }
            }
        }


        public Tweener CameraTrLerp;
        /// <summary>
        /// 方法2
        /// 其实可以先这么些，但是也要写另一个（玩家移动（网络来消息了）
        /// TODO
        /// 手柄操作--->通过数据给服务器
        ///--->网络（驱动玩家移动）
        ///--->驱动玩家（Event.invoke（玩家发生位移操过0.1就移动摄像机控制相机刷新频率：默认实时更新先）【现在没有玩家随便弄一个】）
        ///---?驱动摄像机(设置摄像机)
        ///!!!!控制  来源于CameraManager哦
        //Player给他 玩家锚点在脚下，直接给camera
        /// </summary>
        public void SetCameraPos(Vector3 targetV3, bool isLerp = false)//可以是3维坐标，可以是坐标,是否平滑
        {
            //平滑上下坡
            if (isLerp)
            {
                if (CameraTrLerp != null && CameraTrLerp.IsPlaying())
                {
                    CameraTrLerp.Complete();
                }
                CameraTrLerp = M_myParentRoot.transform.DOMove(targetV3, SEC_PER_FRAME).SetAutoKill(true).SetEase(Ease.Linear);


            }
            else
            {
                //优先这个
                M_myParentRoot.transform.position = targetV3;
            }





            //玩家 通过 CameraManager 到 这里
            //事件，比如看NPC 通过 CameraManager 到 这里
            //为啥通过CameraManager 因为有很多动画呀，因为有很多相机啊
        }
        public void SetCameraPos(Vector2 targetV2, bool isLerp = false)//可以是2维坐标/方向，可以是坐标,是否平滑
        {

        }

        void OnDestroy()
        {
            m_context = null;
        }

        /// <summary> 目标箭头上一次移动的距离 </summary>
        private Vector3 m_targetPreDis = Vector3.zero;
        /// <summary> 目标箭头是否显示 </summary>
        private bool m_targetIsShow = false;

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {
            Test();

            #region 设置头顶信息和伤害飘字位置信息

            Vector3 curPos = Camera.transform.position;
            if (Vector3.Distance(CurCameraPos, curPos) > 0.001f)
            {
                Service.WorldToUI.WorldItemChecker.Instance.UpdatePendGroupsPos();
            }
            CurCameraPos = curPos;
            #endregion
        }

        private void LateUpdate()
        {
            #region 判断目标箭头是否显示
            NPCEntityBase targetEntityBase = BattleManager.Instance.ShowBossPanelEntity;
            if (targetEntityBase != null && targetEntityBase.EntityId != 0 && targetEntityBase.M_IsAlive)
            {
                var pos = Camera.WorldToScreenPoint(targetEntityBase.Position());
                var distance = Vector3.Distance(m_targetPreDis, pos);
                if (distance < 1) {/* SGF.Debuger.Log("沒動")*/; return; };
                m_targetPreDis = pos;
                if (0 < pos.x && pos.x < Screen.width && pos.y > 0 && pos.y < Screen.height && pos.z > 0)
                {
                    // 在屏幕内了
                    if (m_targetIsShow)
                    {
                        m_targetIsShow = false;
                        GlobalEvent.onTargetArrowShow?.Invoke(m_targetIsShow);
                    }
                    return;
                }
                else if (!m_targetIsShow)
                {
                    m_targetIsShow = true;
                    GlobalEvent.onTargetArrowShow?.Invoke(m_targetIsShow);
                }
                if (pos.z < 0)
                {
                    pos = pos * -1;
                }
                GlobalEvent.onTargetArrowPos?.Invoke(pos);
            }
            else if (m_targetIsShow)
            {
                m_targetIsShow = false;
                GlobalEvent.onTargetArrowShow?.Invoke(m_targetIsShow);
            }
            #endregion
        }

        public bool CheckIsInCameraPos(Vector3 vector3)
        {
            var pos = Camera.WorldToScreenPoint(vector3);

            if (0 < pos.x && pos.x < Screen.width && pos.y > 0 && pos.y < Screen.height && pos.z > 0)
            {
                return true;
            }
            return false;
        }

        private void Test()
        {
            return;
            ////测试代码
            //if (Input.GetMouseButtonDown(0))
            //{
            //    // return;
            //    TestCamera();
            //}

            //if (Input.GetMouseButtonDown(1))
            //{
            //    // return;
            //    count--;
            //    i -= 0.1f;
            //    SGF.Debuger.LogError($"{flagKey} i reset --> {i}  ");
            //}

            //if (Input.GetMouseButtonDown(2))
            //{
            //    count = 0;
            //    i = 0.1f;
            //    // isShake = !isShake;
            //    SGF.Debuger.LogError($"{flagKey} i reset --> {i} isShake {isShake} ");
            //    TestStopCameraEffect();
            //}
        }

        private int count = 0;
        private float i = 0.1f;

        private bool isShake = false;

        private void TestCamera()
        {
            count++;
            if (count % 3 == 0)
            {
                i += 0.1f;
            }

            List<CameraFxParam> cameraFxParams = new();

            if (isShake)
            {
                CameraFxParam cameraFxParam = new();
                SGF.Debuger.Log($"{flagKey} i {i}  ");

                cameraFxParam.InitCameraShakeParam(0.5f, i, 10, 0.6f);
                cameraFxParams.Add(cameraFxParam);
            }

            if (!isShake)
            {
                CameraFxParam cameraFxParam = new();
                cameraFxParam.InitCameraZoomParam(0.5f, 30, 5f);
                cameraFxParams.Add(cameraFxParam);
            }

            CameraManager.Instance.DoCamFxSync(cameraFxParams, E_CameraType.StarWorldCam, 0);

        }

        private void TestStopCameraEffect()
        {
            CameraEvent cameraEvent = CameraEvent.ShakeCam;
            if (isShake)
            {
                cameraEvent = CameraEvent.ShakeCam;
            }
            else
            {
                cameraEvent = CameraEvent.CameraZoom;
            }


            CameraManager.Instance.StopCamFxSync(0, cameraEvent, E_CameraType.StarWorldCam);
        }







    }
}
