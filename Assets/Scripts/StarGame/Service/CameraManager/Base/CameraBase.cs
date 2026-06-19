using SGF;
using SGF.Unity;
using StarProject.Game;
using System.Collections.Generic;
//using StarProject.Game.Player;
using UnityEngine;
using StarProjectDef;


namespace StarProject.Service.Cam.Data
{

    public abstract class CameraBase : MonoBehaviour, IRegistrable
    {
        public Camera Camera;
        [SerializeField]
        public int DefCullingMask;
        private void Awake()
        {

            Camera = transform.GetComponent<Camera>();
            ///只有实例才能，实现IColl，的Add接口，不然上下文中给定是无效的
            RegToICollection<E_CameraType, CameraBase>(CameraManager.Instance, GetCameraType(), this);

            OnAwake();



        }
        /// <summary>
        /// 子类在这里写兄弟们，不然子类Awake会覆盖注册的
        /// </summary>
        protected abstract void OnAwake();
        protected abstract E_CameraType GetCameraType();
        //TODO：3D转2D的公共接口
        public static void Create()
        {

        }

        public static void Release()
        {

        }




        void Start()
        {

        }



        void OnDestroy()
        {

        }


        ///必须选择一种调用注册
        ///
        /// 
        /// 
        /// <summary>
        /// 1，对方类型，必须实现了，并且带有这个集合
        /// 2，我用此接口（基类），作为标准（内存锚点）去寻找（并非强转压缩空间），其他匹配者
        /// </summary>
        /// <param name="list"></param>
        /// <param name="cell"></param>
        public void RegToICollection(ICollection<IRegistrable> list, IRegistrable cell)
        {

        }
        /// <summary>
        /// 抽象T适合反射去找，在找个连通器，联通两个接口
        /// 查找耗性能，这里先具体
        /// 虽然通用但是参数还是你自己写
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="IRegistrable">往哪里面加，通常是ServiceModule._cache</typeparam>
        /// <param name="dic"></param>
        /// <param name="cell"></param>
        public void RegToICollection<K, IRegistrable>(ICollection<KeyValuePair<K, IRegistrable>> list, K kkk, IRegistrable cell)
        {
            list.Add(new KeyValuePair<K, IRegistrable>(kkk, cell));

        }

        /// <summary>
        /// camera 执行各种行为的基方法
        /// </summary>
        /// <param name="cameraFxParam"></param>
        /// <returns></returns>
        public abstract void DoCameraAction(List<CameraFxParam> cameraFxParams, int cameraEventID);


        /// <summary>
        /// camera 停止摄像机效果
        /// </summary>
        /// <param name="cameraEvent"></param>
        /// <returns></returns>
        public abstract void StopCameraAction(CameraEvent cameraEvent, int cameraEventID);
    }
}
