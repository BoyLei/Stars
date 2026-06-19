using Fire;
using SGF.Network;
using StarProject.Game.Entity.Factory;
using StarProjectDef;
using System;
using UnityEngine;


namespace StarProject.Game.Entity
{

    //【服务器动态掌管，进入-更新-离开】 
    public abstract class EntityRemoteDynamic : EntityObject
    {
        public bool IsMainPlayer = false;

        /// <summary>
        /// <float, bool, float,bool > ----> 角度/是否需要lerp/lerp的最大时间 / 是否直接使用 maxLerpTime
        /// </summary>
        public Action<float, bool, float, bool> DoAngelChange;
        //public Queue<> s = new Queue<>();
        //public  ;
        //服务器，客户端控制，都会同步的
        //逻辑层中心
        private Vector3 currentPos;

        public Vector3 CurrentPos
        {
            get => currentPos;
        }

        public override Vector3 Position()
        {
            return CurrentPos;
        }

        public ProtoMsg.Vector3 PbPosition()
        {
            tempV3 = Position();
            ProtoUtils.CopyV3ToPbPos(pbPos, tempV3);
            return pbPos;
        }

        private Vector3 serverPosition = Vector3.zero;

        /// <summary>
        /// 服务器坐标
        /// </summary>
        public Vector3 ServerPosition
        {
            get => serverPosition;
            set
            {
                serverPosition = value;
            }
        }

        public void SetCurrentPos(Vector3 value)
        {
            currentPos = value;
            OnFinalPosChange(currentPos);
        }

        // 模型半径
        public float ModelRadius = 0.5f;

        /// <summary>
        /// 通知小地图实体的位置
        /// </summary>
        /// <param name="newVector3"></param>
        public virtual void OnFinalPosChange(Vector3 newVector3)
        {

        }

        private ProtoMsg.Vector3 pbPos = new();

        private Vector3 tempV3 = Vector3.zero;

        protected Vector3 m_clientViewAngle; //缓存当前朝向

        /// <summary>
        /// 客户端 表现层的 方向向量
        /// </summary>
        /// <value></value>
        public Vector3 EulerAngles { get { return m_clientViewAngle; } }

        public int ClientRotToServerRot;    // 客户端角度转换服务器角度

        protected int m_ServerAngles;
        public int ServerAngles { get { return m_ServerAngles; } }

        public ulong EntityId;
        public E_EntityType EntityType;


        //// -----如果原子锁了角度,那以下俩个朝向，会不一样
        //// 实体-》【移动的朝向】手柄会持续改动；是上一次的和当前目标点差的默认移动方向；如果服务器强拉后我保持移动方向一直走的设定（掉线），我会一直撞墙；如果我修改了我会反向移动   
        //角度用Dir/Angel，留下用于记录最后一帧的朝向
        public Vector3 m_EntityMoveDir;

        public Vector3 M_EntityMoveDir
        {
            get
            {
                return m_EntityMoveDir;
            }
            set
            {
                m_EntityMoveDir = value;
            }
        }
        private Smooth.SmoothMoveV3 m_EntitySmoothMoveV3;
        /// <summary>
        /// 实体平滑运动变量, 通过 向 SmoothMoveV3 中传入当前帧的移动向量,可以计算出 当前帧的平滑移动变量
        /// </summary>
        public Smooth.SmoothMoveV3 M_EntitySmoothMoveV3
        {
            get
            {
                if (m_EntitySmoothMoveV3 == null)
                {
                    m_EntitySmoothMoveV3 = new Smooth.SmoothMoveV3();
                }
                return m_EntitySmoothMoveV3;
            }
        }
        //// 实体-》【角度的朝向】是面部朝向：风行者，嘲讽技能
        public Vector3 m_EntityAnglesDir;
        /// <summary>
        /// 实体的 面部 朝向
        /// </summary>
        /// <value></value>
        public Vector3 M_EntityAnglesDir
        {
            get
            {
                return m_EntityAnglesDir;
            }
            set
            {
                m_EntityAnglesDir = value;
            }
        }
        /// <summary>
        /// 移动时候的摇杆指令.
        /// 2023/9/25
        /// 目前 跟夏哥的 约定是在受到黑洞牵引的时候,客户端会给服务器发送摇杆指令.
        /// 此时服务器 并不会处理 客户端发的坐标,只使用 对应的朝向
        /// </summary>
        public Vector3 m_EntityJoyMoveDir;
        public Vector3 M_EntityJoyMoveDir
        {
            get
            {
                return m_EntityAnglesDir;
            }
        }
        /// <summary>
        /// 移动时 摇杆指令的 方向.
        /// 目前 主要是 在黑洞效果里面设置
        /// </summary>
        public int m_EntityJoyMoveSendServerAngle;
        public int M_EntityJoyMoveSendServerAngle
        {
            get
            {
                return m_EntityJoyMoveSendServerAngle;
            }
        }

        /// <summary> 
        /// 服务器 下发的 每帧移动指令
        /// 目前 主要是 在黑洞效果里面设置
        /// </summary>
        public Vector3 m_EntityServerFrameMoveDir;
        public Vector3 M_EntityServerFrameMoveDir
        {
            get
            {
                return m_EntityServerFrameMoveDir;
            }
        }

        //====================================================

        /// <summary>
        /// 2023/7/18
        /// 客户端模拟运动的标识
        /// 客户端 想做 子弹模拟 运动. 服务器 子弹 依旧会采用路点的方式 同步给客户端.
        /// 所以 需要客户端 在模拟运动的时候，禁止子弹的服务器 路点位移. 
        /// 目前 先在 路点位移 doPathMove 同步 view 的时候屏蔽
        /// </summary>
        public bool isSimulateMove = false;

        public bool IsSimulateMove => isSimulateMove;

        public void SetIsSimulateMove(bool isSimulate)
        {
            isSimulateMove = isSimulate;
        }

        public bool GetIsClientSimulateMove()
        {
            return isSimulateMove;
        }

        // -------------------------------------------
        /// <summary>
        /// 服务器 设置 坐标
        /// </summary>
        /// <param name="v"></param>
        /// <param name="hasInit"></param>
        public virtual void ServerSetPosition(object v, bool hasInit, bool isSyncView)
        {
            //优化：属性同步要用Pb来解:这耗啊时刻在解析
            pbPos = ProtoUtils.DeserializePbPos(v);
            //bool isCurrentNoInit = !hasInit;//isCurrentNoInit = 没成功init
            ProtoUtils.CopyPbPos2V3(out serverPosition, pbPos);
            // 如果开启了 客户端的 模拟运动, 那服务器设置坐标 不同步到表现层
            isSyncView = !IsSimulateMove && isSyncView;

            if (!isSyncView)
            {
                return;
            }

            /// TODO: DL
            /// 临时 测试其他人 移动朝向采用下个移动点的朝向,而不是服务器属性同步的朝向.
            /// 表现上 会跟主角移动 一样流程.
            /// 
            /// 但细节和方案需要后续确定.目前 测试时候 只是屏蔽了 属性同步表现层朝向。
            /// 这个后面需要考虑一下 类似恐惧技能造成的朝向变化情况.这个时候需要 采用服务器的AOI属性朝向.
            {
                // Vector3 moveDir = new Vector3(serverPosition.x - CurrentPos.x, 0, serverPosition.z - CurrentPos.z);
                // if (moveDir.magnitude > 0.1)
                // {
                //     ClientSetRotationByDir(moveDir, true, 0);
                // }

            }
            MoveByServerNew(serverPosition, true, hasInit);
            // MoveByServer(serverPosition, hasInit);
        }

        /// <summary>
        /// 当 执行 SetCurrentPos 时,通知子类节点 
        /// </summary>
        public virtual void OnSetCurrentInvoke(bool isInvoke = false)
        {
            //dont do anything
        }

        // 影子坐标改变
        public Action<Vector3> OnPosChange;
        // 控制影子的显隐
        public Action<EntityShowHidenTag, bool> ControlShowHide;
        /// <summary>
        /// 检查是否有 对应类型的 view显示曾 实体
        /// </summary>
        public Func<EntityShowHidenTag, bool> CheckHasControllerShow;

        private Vector3 ___posCache;
        public virtual void SetCurrentPos(float posX, float posY, float posZ, bool isInvoke = false)
        {
            ___posCache.x = posX;
            /*if (posY > 0) */
            ___posCache.y = posY;
            ___posCache.z = posZ;
            //111
            OnPosChange?.Invoke(___posCache);
            //if (___posCache == Vector3.zero)
            //{
            //    SGF.Debuger.LogError($"实体角度调试 --------设置坐标有问题 currentPos={currentPos},value={___posCache}");
            //}

            SetCurrentPos(___posCache);
            //-------------------------------通知--------------------------------------------------------
            OnSetCurrentInvoke(isInvoke);
            //---------------------------------------------------------------------------------------
            //稳定后做0.1距离外再同步一次位置，目前是每次update调用他
            //SGF.Debuger.Log($"名字测试 主角创建 主角位置 EntityId={EntityId},isMainPlayer={IsMainPlayer},pos={___posCache}");
            Service.WorldToUI.WorldItemChecker.Instance.NeedRefUICommand(EntityId, IsMainPlayer);
        }

        /// <summary>
        /// 提取的公共 的 服务器驱动 的移动接口，不同子类会有不同的实现方式
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isBornOrServerForce"></param>
        internal abstract void MoveByServer(Vector3 pos, bool isBornOrServerForce);


        internal abstract void MoveByServerNew(Vector3 pos, bool ServerForce, bool isBorn);

        #region 角度 的模拟运动
        private bool isSimulateRotation = false;

        /// <summary>
        /// 判定是否是 角度模拟朝向变化.  如果是角度模拟的话, 不需要走 服务器同步角度朝向这一套逻辑. 
        /// 服务器的角度朝向只会改变 m_ServerAngles 的值, 但不会同步到 表现层.
        /// </summary>
        public bool IsSimulateRotation => isSimulateRotation;

        public void SetIsSimulateRotation(bool isRotationSimulate)
        {
            isSimulateRotation = isRotationSimulate;
        }

        #endregion

        #region 设置角度

        /// <summary>
        /// 同步 view 层的 角度
        /// </summary>
        /// <param name="y"></param>
        /// <param name="neepLerp"></param>
        /// <param name="maxLerpTime"></param>
        private void SyncViewAngle(float y, bool neepLerp, float maxLerpTime = 0, bool IsServer = false, bool isUseMaxLerpTime = false)
        {
            // if (EntityId == GameManager.Instance.mainPlayerId)
            // {
            //     SGF.Debuger.LogError($"IsServer={IsServer}[Rotate] 推送 viewRota : {y} ,  curAngle: {EulerAngles.y} needLerp: {neepLerp} ");
            // }
            //if (EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.Log($"实体角度调试 OnAngelChangeMove id={EntityId},y={y},neepLerp={neepLerp}");
            //}
            DoAngelChange?.Invoke(y, neepLerp, maxLerpTime, isUseMaxLerpTime); //服务器设置角度
        }

        #region 设置客户端角度(m_clientViewAngle)
        ///------------------------------客户端角度-----------------------------------------
        /// 2023/6/26
        ///     角度 分为 m_clientViewAngle 和 m_ServerAngles.  他们通过 DoAngelChange 事件 共同驱动 view表现层 的角度
        ///     对于 客户端角度 m_clientViewAngle 来说, 客户端 可以自己 调整它的大小(摇杆操作或其它方式), 同时 发送给 服务器;
        ///     对于 服务器角度 m_ServerAngles 来说, 类似于 角度的属性同步, 自己或者 其他人的 表现并不一致; 对于主角来说, 
        ///          属性的角度同步只会 设置 数值, 而不像其他人一样,  服务器属性同步过来的时候, 就需要同步 给表现层
        public void ClientSetRotation(float rotate, bool neepLerp, float maxLerpTime, bool isUseMaxLerpTime = false)
        {
            Vector3 dir = Utils.ServerRota2Vector(rotate);
            ClientSetRotationByDir(dir, neepLerp, maxLerpTime, false, isUseMaxLerpTime);
        }

        public void ClientSetRotationByDir(Vector3 dir, bool neepLerp, float maxLerpTime, bool IsServer = false, bool isUseMaxLerpTime = false)
        {
            M_EntityAnglesDir = dir;
            SetClientAngle(dir);

            SyncViewAngle(m_clientViewAngle.y, neepLerp, maxLerpTime, IsServer, isUseMaxLerpTime);
        }

        public void SetClientAngle(Vector3 dir)
        {
            //客户端服务器是反的，另一种标识方法是客户端=90-服务器=一样的
            //客户端角度
            float viewAngle = (float)(Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg) % 360;
            double serverRot = Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg % 360;
            //if (EntityId == GameManager.Instance.mainPlayerId)
            //{
            //    Debug.LogError($"dir={dir} viewAngle={viewAngle}    serverRot={serverRot} ");
            //}
            SetClientAngle(viewAngle, (int)serverRot);
        }

        private void SetClientAngle(float viewAngle, int serverRot)
        {
            m_clientViewAngle.y = viewAngle;
            ClientRotToServerRot = serverRot;
        }

        /// <summary>
        /// 使用 服务器 朝向 同步 客户端朝向
        /// </summary>
        public void SyncClientRotaWithServerRota()
        {
            if (ClientRotToServerRot == ServerAngles)
            {
                return;
            }
            // SGF.Debuger.LogError($"[Rotate] 同步一次 服务器朝向 到 view ClientRotToServerRot: {ClientRotToServerRot},  ServerAngles: {ServerAngles}");
            ClientSetRotation(ServerAngles, false, 0);
        }

        //-----------------------------------------------------------------------

        #endregion

        #region 设置服务器角度

        /// --------- 服务器来消息，控制其他玩家,后续属性通知不必强制设置主角 --------------------------
        /// note:
        ///     1. 同步服务器角度可以 只同步数值, 比如 主角自己的属性同步,角度的同步只设置数值，但不同步 view层;
        ///     2. 同步服务器角度的时候 如果 需要同步view层(isSyncView = true), 那默认的 会同时 设置 客户端角度(m_clientViewAngle) 
        ///        这样, 在服务器 同步角度后，能够保证 客户端/服务求 角度数据的一致.
        ///     3. 角度是 数据 顺序如下:
        ///         m_clientViewAngle 发生变化 ------> 驱动 view 层变化
        ///         服务器角度 发生变化 ,判断 是否需要 驱动 view 层变化:
        ///             不需要 ----> 只是数值发生变化;
        ///             需要   ----> 驱动 m_clientViewAngle 客户端角度 发生变化 ---> 驱动view 层变化

        /// <summary>
        /// 服务器设置角度
        /// </summary>
        /// <param name="rotate"></param>
        /// <param name="neepLerp"></param>
        /// <param name="isSyncView">如果 服务器角度需要同步 view, 那会同时 设置客户端角度数据,从而保证数据的一致</param>
        /// <param name="maxLerpTime"></param>
        /// <param name="isUseMaxLerpTime">是否 使用 传入的 maxLerpTime 作为lerp 时间</param>
        public void ServerSetRotation(float rotate, bool neepLerp = true, bool isSyncView = true, float maxLerpTime = 0, bool isUseMaxLerpTime = false)
        {
            // 服务器算的是X轴方向的夹角
            // 需要客户端拿到值后，转换成方向向量
            // 再计算Y轴的夹角，然后赋值
            //【m_dirInterpolation/自己的m_clientAngle不用自己算】这个用服务器m_clientAngle.y
            Vector3 dir = Utils.ServerRota2Vector(rotate);
            //float m_clientAngleY = (float)Math.Round((Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg) % 360,1);
            // SGF.Debuger.LogError($"[Rotate] 服务器 setRota : {rotate} , needLerp: {neepLerp} , isSyncView: {isSyncView}");

            // 如果是模拟角度运动的话, 服务器的朝向将不再同步到表现层
            isSyncView = !IsSimulateRotation && isSyncView;

            ServerSetRotationByDir(dir, neepLerp, isSyncView, maxLerpTime, isUseMaxLerpTime);
        }

        private void ServerSetRotationByDir(Vector3 dir, bool neepLerp, bool isSyncView, float maxLerpTime, bool isUseMaxLerpTime = false)
        {
            //m_entityMoveDir = dir;//外面没有Get都是Set；因为面部朝向不关心移动朝向（移动缓存perv）
            M_EntityAnglesDir = dir;
            SetServerAngle(dir);

            // 如果 需要同步view, 那 服务器数据会同时 设置 客户端角度数据,从而保证 数据的一致
            if (!isSyncView)
            {
                return;
            }


            ClientSetRotationByDir(dir, neepLerp, maxLerpTime, true, isUseMaxLerpTime);
        }

        public void SetServerAngle(Vector3 dir)
        {
            m_ServerAngles = (int)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg % 360);
        }

        //-----------------------------------------------------------------------
        #endregion

        #endregion

        public virtual void ClearDefineVector() { }
        protected override void Release()
        {
            pbPos = new ProtoMsg.Vector3();
            m_clientViewAngle = Vector3.zero;
            ClientRotToServerRot = 0;
            m_ServerAngles = 0;
            M_EntityMoveDir = Vector3.zero;
            M_EntityAnglesDir = Vector3.zero;//之后要拓展到每一个子类中
            currentPos = Vector3.zero;

            isSimulateRotation = false;
            base.Release();
        }

        protected virtual void Reset()
        {
            IsMainPlayer = false;
        }

    }


}