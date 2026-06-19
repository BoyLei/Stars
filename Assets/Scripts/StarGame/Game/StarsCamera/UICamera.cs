using DG.Tweening;
using StarProject.Service.Cam.Data;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.StarsCamera
{

    public class UICamera : CameraBase
    {
        //public static GameCamera Current;
        //public static Camera MainCamera;
        private GameContext m_context;
        [SerializeField]
        private Transform M_myParentRoot;
        ///public Canvas M_Canvas;
        //DynamicCamRoot是放一堆 相机的
        //BCameraRoot 才是控制节点（段磊控制这个）TODO
        public const float SEC_PER_FRAME = 1f / GameConfig.FIX_TIME_PER_SEC; //每帧 0.03333秒

        private const string flagKey = "[UICamera]";

        public GameCameraFeel gameCameraFeel;

        protected override void OnAwake()
        {
            //AwakeTodo
            if (M_myParentRoot == null)
            {
                M_myParentRoot = transform.parent;//（段磊控制这个）TODO
            }

            EnsureCameraFeel();

            Camera.allowDynamicResolution = false;

            //m_CameraOffset = m_Camera.transform.localPosition;

        }


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
            return E_CameraType.UICam;//--
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

        public override void StopCameraAction(CameraEvent cameraEvent, int cameraEventID)
        {
            //gameCameraFeel.StopCameraAction(cameraEvent, cameraEventID);
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
            //gameCameraFeel.DoCameraAction(cameraFxParams, cameraEventID);
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

        public void UpdateCamera(Vector3 targetPos)
        {

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





        //玩家 通过 CameraManager 到 这里
        //事件，比如看NPC 通过 CameraManager 到 这里


        void OnDestroy()
        {
            m_context = null;
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {


        }











    }
}
