using Fire;
using Google.Protobuf;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Network;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.TypeEffect;
using StarProject.Service.Cam;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace StarProject.Game.Entity.RemoteDynamic
{
    public abstract class AOIEntityObject : EntityRemoteDynamic
    {
        //private string LOG_TAG = "[AOIEntityObject]";

        protected VitalSignData m_playerData; //玩家基础数据,playerD

        public VitalSignData Data
        {
            get { return m_playerData; }
        }
        protected virtual VitalSignAttrData AttrData => null;

        ///// <summary> 是否禁止客户端摇杆朝向 </summary>
        public bool IsForbidJoyStickDir = false;

        /// <summary>
        /// 阵营 目前 1：人  0：怪
        /// </summary>
        protected int m_faction = -1;
        public int Faction
        {
            get { return m_faction; }
            set
            {
                if (m_faction != value)
                {
                    //Debug.LogError($"{ EntityId} 阵营变更: {m_faction}->{value}");
                    m_faction = value;

                    CacheFactionData();
                    FactionDirty = true;

                }
            }
        }

        public bool FactionDirty { get; set; }

        public List<int> FriendlyFaction;   // 同盟阵营
        public List<int> NeutralFaction;    // 中立阵营
        public List<int> OpposingFaction;   // 敌对阵营

        public ModelDataCell modelDataCell;
        public AvatarDataCell avatarDataCell;

        public bool IsNeedListenPathUpdate { get; protected set; }

        public string m_Name = "";
        public string M_Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        public string m_Appellation = "";
        public string M_Appellation
        {
            get
            {
                return m_Appellation;
            }
            set
            {
                m_Appellation = value;
            }
        }

        // 是否忽略重力
        public bool IgnoreGravity = false;

        public float ModleScale = 1.0f;

        /// <summary>
        /// 2023/8/24
        /// 碰撞盒 缩放因子.
        /// gl 要求 通过改变碰撞盒参数, 从而 改变 模型大小.同时,  改变 相应的 特效 大小.
        /// 
        /// note:
        ///     模型大小 = 模型配置大小cfgScale * 碰撞盒 缩放因子;
        ///     特效大小 = 碰撞盒配置cfgScale * 碰撞盒 缩放因子;
        /// </summary>
        public UnityEngine.Vector3 BoxScaleRatio = UnityEngine.Vector3.one;

        /// <summary>
        /// 碰撞盒的 碰撞盒大小
        /// </summary>
        public UnityEngine.Vector3 BoxCfgScale = UnityEngine.Vector3.one;

        private UnityEngine.Vector3 v3 = UnityEngine.Vector3.one;
        /// <summary>
        /// 模型 半径 总的 缩放尺寸 = 缩放因子(BoxScaleRatio) * 配置缩放半径(BoxCfgScale)
        /// </summary>
        public UnityEngine.Vector3 GetBoxScale()
        {
            v3.Set(BoxScaleRatio.x * BoxCfgScale.x, BoxScaleRatio.y * BoxCfgScale.y, BoxScaleRatio.z * BoxCfgScale.z);
            return v3;
        }

        /// <summary>
        /// 实体 对应配置表的 id
        /// </summary>
        /// <typeparam name="uint"></typeparam>
        public uint ConfigIndex => AttrData.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Index);

        /// <summary>
        /// 召唤物主人ID
        /// </summary>
        public ulong SummonHostID => AttrData.GetAoiValue<ulong>(EnumAOIType.Ulong, AOIAttrDefine.SummonHostID);

        protected object ModelCfg = null;
        /// <summary>
        /// 实体 模型 对应的配置, 此处存储的原因是 gl 新加了一个需求,需要 子弹 实时 跟随 主角的高度. 
        /// 与其让 子弹每次 都去 通过 ConfigIndex 获取配置, 还不如 直接在实体身上存储 相应的模型 配置, 这样也不需要每次都去读配置
        /// </summary>
        /// <value></value>
        public object EntityModelConfig
        {
            get
            {
                return ModelCfg;
            }
        }
        public int SpaceIndex => AttrData.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.SpaceIndex);

        /// <summary>
        /// 实体 当前交互的 id
        /// </summary>
        public ulong CurInteractID => AttrData.GetAoiValue<ulong>(EnumAOIType.Ulong, AOIAttrDefine.InteractID);

        private void CacheFactionData()
        {
            FactionRelationDataCell factionRelationDataCell = LocalDataManager.Instance.GetFactionRelationDataCell(Faction);
            if (factionRelationDataCell != null)
            {
                FriendlyFaction = factionRelationDataCell.FriendlyFaction;
                NeutralFaction = factionRelationDataCell.NeutralFaction;
                OpposingFaction = factionRelationDataCell.OpposingFaction;
            }
        }

        private bool m_IsAlive = false;

        /// <summary>
        /// 是否活着
        /// </summary>
        public bool M_IsAlive
        {
            get
            {
                return m_IsAlive;
            }
            set
            {
                if (value == m_IsAlive)
                {
                    return;
                }
                m_IsAlive = value;
                if (!m_IsAlive)
                {
                    // 清空数据层的东西
                    DieRelease();
                    ActionDie?.Invoke();
                }
            }
        }

        public Action ActionOnBeAttackFlashColor;   // 受击闪白
        protected Action<HurtData, ulong, ulong, int, E_StageType> ActionOnHurtNodeMsg;  // 伤害通知
        protected Action<OffsetData, float, MoveLabel, MoveType, string> ActionOnOffsetDataMsg; // 位移通知
        public Action<I_FxParam, string, string> ActionOnPlaySpecialEffects;    // 播放特效
        public Action<I_FxParam, string> ActionOnStopSpecialEffects;    // 关闭特效
        public Action<bool> CompoentShowAction; // 组件显示委托

        public Action<AudioClip, bool> ActionOnPlayAudio;//NeedLoop
        public Action<string, E_SoundNTFtype> ActionOnPlayWwiseAudio;   // 播放动画插件声音
        public Action<bool> PauseAnimation;  //动画暂停
        public Action<bool> FreezModel;

        /// <summary>
        /// 角色 shader 状态变化的通知
        /// </summary>
        public Action<CharStateController.CharacterState, bool> OnCharStateChange;

        /// <summary>
        /// globalShowEffect 效果发生变化时的通知. <刷新类型, 变化的 BaseTypeEffect, 当前的 BaseTypeEffect>
        /// </summary>
        public Action<TypeEffectUpdateType, BaseTypeEffect, BaseTypeEffect> ActionOnStackShowTypeChange;

        /// <summary>
        /// 隐身效果
        /// </summary>
        public Action<TranslucentEffect, bool> ActionOnTranslucentEffect;
        /// <summary>
        /// 创建本地模拟召唤物
        /// </summary>
        public Action<ulong, string, UnityEngine.Vector3, int, bool> ActionOnCreateSimulateSummon;
        /// <summary>
        /// 本地召唤物 的旋转
        /// note:
        ///     以玩家为中心，对召唤物的根节点做一定角度的旋转
        /// </summary>
        public Action<ulong, int, bool, bool> ActionOnTurnSimulateSummon;

        public Action<ShaderEnum, bool> ShaderChange;
        public Action<bool, TypeEffect.BaseTypeEffect> ShaderChange2;

        public Action<string> SetIdleAnimation; // 设置实体模型idle动作文件名
        public Action<float, string> ActionOnStartTimeHidden;   // 开启一段时间的隐身
        public Action<string> ActionOnStopTimeHidden;       // 中止 key 对应的一段时间的隐身
        public Action<bool> ActionOnStartHidden;   // 模型隐藏
        public Action<bool> ActionOnStopHidden;   // 模型显示
        public Action<UnityEngine.Vector3> ActionOnUpdateBoxScale;   // 碰撞盒 半径发生变化的通知

        public Action<float> ActionOnViewOffectY;    // 显示层Y轴偏移量

        public Action ActionOnViewCreateFinish;   // 显示层模型完成

        /// <summary>
        /// note:
        ///     此处添加的 同步出生点坐标是为了 临时修复子弹 再viewCreate 后 第一次设置坐标位置不对的问题.
        ///     原因是 多次释放子弹后，子弹的 创建由异步变成了同步， 此时 Create设置的 ServerPos 并没有采用
        ///     实体创建时下发的坐标，导致坐标设置不对
        /// </summary>
        public Action ActionOnSyncBorthPos;   // 同步一次出生点坐标的action
        /// <summary>
        /// 切换模型的 acton
        /// </summary>
        public Action<int, bool> ActionOnSwitchModel;
        public Action ActionDie;    // 死亡委托

        public Action ActionOnMoveStateChange;

        public Action<string, int> ActionSetNameAndCor;
        public Action ActionSetNameOrgCor;

        //=================================================================
        //public bool M_CtrlByServer = false;//M_mainPlayerSetByServer
        //public bool IsDriveByServerStop { get { return m_IsDriveByServerStop; } set { m_IsDriveByServerStop = value; } }
        protected float m_speed = 6.01f; // 当前速度 600 => 6 ；600cm 改成 0.6m

        public float Speed
        {
            get { return m_speed; }
            set
            {
                if (m_speed != value)
                {
                    // 变化差小于0.1（10是服务器的）
                    if (Mathf.Abs(m_speed - value) >= 0.1)
                    {
                        SpeedChangeAction?.Invoke(value);
                    }
                    //没有负数，没有0
                    //在阀值之内的都允许
                    m_speed = value;//速度记录
                    InvokeNextPointTarget(false);//服务器加速，客户端更新到下一个点的时间。
                    //if (EntityId == GameManager.Instance.mainPlayerId)
                    //{
                    //    SGF.Debuger.LogError($"主角当前的速度={m_speed}");
                    //}
                }
            }
        } //人只有客户端用：600  ||| 怪是（服务器/客户端都用）：应该300（目前是3）|||是float

        public Action<float> SpeedChangeAction;    // 移动速度改变委托

        /// <summary> 动画参数 </summary>
        public AnimParam m_animParam = new();
        // ------------------- 生命实体移动时 基础移速与动画的比例
        public float m_entityBaseWalkSpeed;
        public float m_entityBaseRunSpeed;
        // --------------------------------------------------------

        public System.Action<string> ActionStopMoveDotween;

        private bool m_IsMainPlayerSummon = false;
        public bool M_IsMainPlayerSummon
        {
            get
            {
                return m_IsMainPlayerSummon;
            }
            set
            {
                m_IsMainPlayerSummon = value;
            }
        }

        /// <summary>
        /// 在 AOI实体上 加一个 当前实体状态， 可以用来区分 当前是在 移动/释放技能等等
        /// </summary>
        /// <value></value>
        public virtual E_ULayerSubState M_eSubState
        {
            get => E_ULayerSubState.Idle;
        }

        public bool IsSimpleMovingState()
        {
            return M_eSubState != E_ULayerSubState.BattleIdle && (int)M_eSubState >= (int)E_ULayerSubState.WanderMoving && (int)M_eSubState <= (int)E_ULayerSubState.WeaponRetractionMoving;
        }

        //============= 从NpcEntityBase中挪过来的代码===========
        //两个问题：
        //1,出生和销毁要有生命周期控制
        //2,子类型要重写
        protected bool IsFirstPos = true; //这里有隐患，重生呢？没事根据距离拉
        protected bool IsFirstRot = true;
        protected bool IsFirstAOI = true;
        /// <summary>
        /// 移动状态，目前跟夏哥约定 是 服务器的时间戳。 客户端只在 黑洞效果下处理这个状态.
        /// 如果超过 一段时间，就认为 动画取消
        /// </summary>
        public long MoveState = 0;

        /// <summary>
        /// 准备死亡的状态
        /// 2024/3/13
        ///     gl 的需求: 怪物死亡后, 被动/buff 中的shader 效果不跟随 运行时而 移除.
        /// </summary>
        public bool IsReadyDead = false;

        //死亡重置初始状态
        protected virtual void ReSetBorn()
        {
            IsFirstPos = true;
            IsFirstRot = true;
            IsFirstAOI = true;
            IsReadyDead = false;
        }


        protected virtual void Init(ulong entityID, E_EntityType entityType)
        {
            EntityId = entityID;
            EntityType = entityType;
            M_IsAlive = true;
            IsNeedListenPathUpdate = false;
            RegisterAction();
        }


        public virtual void DieRelease()
        {
            Clear_Current_FollowUp_Target(); //挂
            ClearServerV3WayPoints();
            nextMovePoint = UnityEngine.Vector3.zero;

            DoPathMove?.Invoke(nextMovePoint); //角色挂掉
            //--- 删除GM节点
            DestroyPosGobsCache();
            DestroyRotPointGobsCache();
            DestroySkillMoveGobsCache();
            DestroySkillUserGobsCache();
            DestroySkillUserRPCGobsCache();
            DestroyWayPointsCache();
        }

        #region 属性变更监听

        protected virtual void InitRegisterAttribute()
        {
            // 初始化属性值
            OnAOIRotChange(AOIAttrDefine.Rot, null);
            OnAOITruthSpeedChange(AOIAttrDefine.TruthSpeed, null);
            OnAOIFactionChange(AOIAttrDefine.Faction, null);

            if (Data == null)
            {
                return;
            }

            //共用的属性回调可以在此处注册
            Data.RegisterAttribute(AOIAttrDefine.Position, OnAOIPositionChange);
            Data.RegisterAttribute(AOIAttrDefine.PathPoses, OnAOIWaypointsChange);
            Data.RegisterAttribute(AOIAttrDefine.CurrPathIndex, OnAOICurrPathIndexChange);
            Data.RegisterAttribute(AOIAttrDefine.Rot, OnAOIRotChange);
            Data.RegisterAttribute(AOIAttrDefine.TruthSpeed, OnAOITruthSpeedChange);
            Data.RegisterAttribute(AOIAttrDefine.Faction, OnAOIFactionChange);
            Data.RegisterAttribute(AOIAttrDefine.OfflineRot, OnAOIRotChange);
            Data.RegisterAttribute(AOIAttrDefine.MoveState, OnMoveStateChange);
            //Data.RegisterAttribute(AOIAttrDefine.PVPState, OnPvpStateChange);

        }

        protected virtual void UnRegisterAttribute()
        {
            if (Data == null)
            {
                return;
            }
            Data.UnRegisterAttribute(AOIAttrDefine.Position, OnAOIPositionChange);
            Data.UnRegisterAttribute(AOIAttrDefine.PathPoses, OnAOIWaypointsChange);
            Data.UnRegisterAttribute(AOIAttrDefine.CurrPathIndex, OnAOICurrPathIndexChange);
            Data.UnRegisterAttribute(AOIAttrDefine.Rot, OnAOIRotChange);
            Data.UnRegisterAttribute(AOIAttrDefine.TruthSpeed, OnAOITruthSpeedChange);
            Data.UnRegisterAttribute(AOIAttrDefine.Faction, OnAOIFactionChange);
            Data.UnRegisterAttribute(AOIAttrDefine.OfflineRot, OnAOIRotChange);
            Data.RegisterAttribute(AOIAttrDefine.MoveState, OnMoveStateChange);

        }

        #endregion
        protected void ResetToIdleState()
        {
            //SGF.Debuger.Log("关闭了");//【1服务器先关闭路点】，然后发技能消息，都比较提前【隐患处理ForceSynvPosPerSkill】
            //要回到Idle，走正常流程，里面的BattleIdle/Idle自己会处理
            BreakFindPath();

            I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.Idle);
            ChangeState((GameKeyCommand)E_ULayerSubState.Idle, animParam, false);

            IsFirstAOI = true; //不论服务器是否驱动我，我走完了所有路点，循环已经破裂，回归初始状态（服务器数据迟到了玩家都走完了，还要再次启动或驱动)【走完的停止，和技能断裂的停止】
        }
        #region 移动处理细则
        ///全局设定提醒：无论如何都会逻辑停一下当调用MoveLerper（0.0001秒，上面刷一下会继续走）
        ///上层处理决定[zero当前无目标/保持之前目标/Dequeue请求新的目标/Clear如不同步永远无目标]
        ///没消息时走完就结束，有消息时会持续更新
        ///逻辑要拆分的比较细，逻辑都分开
        ///网络延迟和提前到有细分处理

        /// <summary>
        /// （逻辑停一下。）取消当前目标，如不同步永远无目标
        /// //清理数据，下次是0，停止当前
        /// </summary>
        public void Clear_Current_FollowUp_Target()
        {
            ClearFindPathPoints();
            //nextMovePoint = UnityEngine.Vector3.zero;
            ResetToIdleState();//通知
        }

        /// <summary>
        /// 下次是0，停止当前
        /// </summary>
        public void Clear_FollowUp_Target()
        {
            ClearFindPathPoints();
            //serverV3WayPointsCache.Clear();
            nextMovePoint = UnityEngine.Vector3.zero;
        }

        /// <summary>
        /// 下次是0，停止当前
        /// </summary>
        public void MainPlayer_Clear_FollowUp_Target()
        {
            ClearFindPathPoints();
            ClearServerV3WayPoints();
            nextMovePoint = UnityEngine.Vector3.zero;
        }

        private void ClearFindPathPoints()
        {
            FindPathPoints.Clear();
            GameManager.Instance.TriggerEvent("FindPathPoints_Clear", EntityId);
        }

        private void ClearServerV3WayPoints()
        {
            serverV3WayPointsCache.Clear();
            GameManager.Instance.TriggerEvent("ServerV3WayPoints_Clear", EntityId);
        }

        //有具体情况，具体分析，具体处理组合
        #endregion

        protected override void Release()
        {
            UnRegisterAttribute();
            EntityId = 0;
            M_IsAlive = false;
            EntityType = E_EntityType.None;
            ///////////////11111111
            ActionOnBeAttackFlashColor = null;
            ActionOnHurtNodeMsg = null;
            ActionOnOffsetDataMsg = null;
            ActionOnPlaySpecialEffects = null;
            ActionOnStopSpecialEffects = null;
            CompoentShowAction = null;
            ActionOnPlayAudio = null;
            ActionOnPlayWwiseAudio = null;
            PauseAnimation = null;
            FreezModel = null;
            OnCharStateChange = null;
            ActionOnStackShowTypeChange = null;
            ActionOnTranslucentEffect = null;
            ActionOnCreateSimulateSummon = null;
            ActionOnTurnSimulateSummon = null;
            ShaderChange = null;
            ShaderChange2 = null;
            SetIdleAnimation = null;
            ActionOnStartTimeHidden = null;
            ActionOnStopTimeHidden = null;
            ActionOnStartHidden = null;
            ActionOnStopHidden = null;
            ActionOnUpdateBoxScale = null;
            ActionOnViewOffectY = null;
            ActionOnViewCreateFinish = null;
            ActionOnSyncBorthPos = null;
            ActionOnSwitchModel = null;
            ActionDie = null;
            ActionOnMoveStateChange = null;
            ActionSetNameAndCor = null;
            ActionSetNameOrgCor = null;
            SpeedChangeAction = null;
            ActionStopMoveDotween = null;
            ActionOnPathMoveEnd = null;
            OnFindPathCallBack = null;
            /////////////// 22222222

            if (avatarDataCell != null)
            {
                SoundManager.Instance.UnLoadBank(avatarDataCell.SoundBank);
            }
            m_playerData = null;
            ModelCfg = null;

            base.Release();
        }

        protected override void Reset()
        {
            m_IsMainPlayerSummon = false;
            m_faction = -1;
            FriendlyFaction = null;
            NeutralFaction = null;
            OpposingFaction = null;

            BoxScaleRatio = UnityEngine.Vector3.one;
            BoxCfgScale = UnityEngine.Vector3.one;
            ModelRadius = 0.5f;

            IsReadyDead = false;

            base.Reset();

            ReSetBorn();
            DieRelease();
        }

        internal virtual void RegisterAction() { }

        internal virtual void EnterFrame() { }

        public void HandleHurtNodeMsg(HurtData hurtData, ulong builderID, ulong ownerId, int flutteringWordsID, E_StageType e_StageType)
        {
            ActionOnHurtNodeMsg?.Invoke(hurtData, builderID, ownerId, flutteringWordsID, e_StageType);
        }

        public void HandleActionOnOffsetDataMsg(OffsetData offsetData, float durningTime, MoveLabel moveLabe, MoveType moveType, string key)
        {
            ActionOnOffsetDataMsg?.Invoke(offsetData, durningTime, moveLabe, moveType, key);
        }

        public void OnActionPlayCamera(int cameraEffectID, E_CameraEffectType e_CameraEffectType)
        {
            int cameraEventID = CameraManager.Instance.GetCameraEventID();
            GlobalEvent.OnCameraEvent.Invoke(cameraEventID, cameraEffectID, e_CameraEffectType, Position());
        }

        // 通知小地图实体坐标发生变化
        public override void OnFinalPosChange(UnityEngine.Vector3 newVector3)
        {
            base.OnFinalPosChange(newVector3);
        }

        /// <summary>
        /// 基础的位移坐标,不同的子类可以重写对应的 MoveByServer 方法
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isBornOrServerForce"></param>
        internal override void MoveByServer(UnityEngine.Vector3 pos, bool isBornOrServerForce)
        {
            SetCurrentPos(pos);
        }

        internal override void MoveByServerNew(UnityEngine.Vector3 pos, bool ServerForce, bool isBorn)
        {
            SetCurrentPos(pos);
        }
        #region 路点相关

        public Queue<UnityEngine.Vector3> FindPathPoints = new();    // 这是：根据服务器通知的路点下标裁剪的客户端用到的路点[b,c,d]
        public List<UnityEngine.Vector3> serverV3WayPointsCache = new();  // 服务器每次下发的路点[a,b,c,d]
        public UnityEngine.Vector3 nextMovePoint = UnityEngine.Vector3.zero;    // 下一个路点

        public Action<UnityEngine.Vector3> DoPathMove, DoForceMove, DoThdPsnMove;   // 循环路点，强同步坐标，寻路坐标
        public Action<UnityEngine.Vector3> ActionOnBirthPos;    // 出生坐标委托
        public Action<UnityEngine.Vector3, float, MoveLabel, MoveType, string, Action<bool>> DoSkillPathMove; // 技能移动

        /// ------------------------------  测试坐标点
        protected Queue<GameObject> RotPointGobsCache = new();    // 角度属性同步 朝向和坐标
        protected Queue<GameObject> PosGobsCache = new();         // 坐标属性同步 坐标
        protected Queue<GameObject> SkillMoveGobsCache = new();   // 技能位移坐标
        protected Queue<GameObject> SkillUserGobsCache = new();   // 技能使用协议每帧坐标
        protected Queue<GameObject> SkillUserRPCGobsCache = new();// 技能使用协议立即坐标
        protected List<GameObject> WayPointsGobsCache = new();     // 路组属性同步 坐标

        #region GM节点处理

        #region 添加

        protected void AddPosGobCache(UnityEngine.Vector3 pos)
        {
            return;
            GameObject god = null;
            if (PosGobsCache.Count < 5)
            {
                // god = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/PosGobCache") as GameObject);
                god = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/PosGobCache");
            }
            else
            {
                god = PosGobsCache.Dequeue();
            }
            //float sjs = UnityEngine.Random.Range(0.0f, 1.0f);
            //int sjs2 = UnityEngine.Random.Range(0, 3);
            //Color color = new Color(sjs2 == 0 ? sjs : 1, sjs2 == 1 ? sjs : 1, sjs2 == 2 ? sjs : 1, sjs2 == 3 ? sjs : 1);
            //MeshRenderer meshRenderer = nextPointBox.GetComponent<MeshRenderer>();
            //meshRenderer.material.SetColor("_BaseColor", color);
            god.name = $"坐标同步_{PosGobsCache.Count + 1}";
            god.transform.position = pos;
            //god.transform.localScale = UnityEngine.Vector3.one;
            PosGobsCache.Enqueue(god);
        }

        protected void AddRotPointGobCache(int value)
        {
            return;
            GameObject Arror = null;
            if (RotPointGobsCache.Count > 10)
            {
                Arror = RotPointGobsCache.Dequeue();
            }
            else
            {
                //Arror = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/RotPointGobCache") as GameObject);
                Arror = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/RotPointGobCache");
            }
            Arror.name = "角度同步_" + value;
            Arror.transform.position = CurrentPos;
            UnityEngine.Vector3 dir = Utils.ServerRota2Vector(value);
            float _y = (float)(Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg) % 360;
            UnityEngine.Vector3 dir2 = UnityEngine.Vector3.zero;
            dir2.y = _y;
            Arror.transform.localEulerAngles = dir2;
            RotPointGobsCache.Enqueue(Arror);
            //SGF.Debuger.LogError($"属性同步 角度tag 坐标={CurrentPos},原角度={m_angles.y},{ServerAngles},现在的={_y},{value}");
        }

        protected void AddSkillMoveGobCache(UnityEngine.Vector3 pos)
        {
            return;
            GameObject god = null;
            if (SkillMoveGobsCache.Count < 5)
            {
                god = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/SkillMoveGobCache");
                //god = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/SkillMoveGobCache") as GameObject);
            }
            else
            {
                god = SkillMoveGobsCache.Dequeue();
            }
            //float sjs = UnityEngine.Random.Range(0.0f, 1.0f);
            //int sjs2 = UnityEngine.Random.Range(0, 3);
            //Color color = new Color(sjs2 == 0 ? sjs : 1, sjs2 == 1 ? sjs : 1, sjs2 == 2 ? sjs : 1, sjs2 == 3 ? sjs : 1);
            //MeshRenderer meshRenderer = nextPointBox.GetComponent<MeshRenderer>();
            //meshRenderer.material.SetColor("_BaseColor", color);
            god.name = $"技能位移_{SkillMoveGobsCache.Count + 1}";
            god.transform.position = pos;
            god.transform.localScale = UnityEngine.Vector3.one;
            SkillMoveGobsCache.Enqueue(god);
        }

        protected void AddSkillUserGobCache(UnityEngine.Vector3 pos)
        {
            return;
            GameObject god = null;
            if (SkillUserGobsCache.Count < 2)
            {
                //god = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/SkillUserGobCache") as GameObject);
                god = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/SkillUserGobCache");
            }
            else
            {
                god = SkillUserGobsCache.Dequeue();
            }
            //float sjs = UnityEngine.Random.Range(0.0f, 1.0f);
            //int sjs2 = UnityEngine.Random.Range(0, 3);
            //Color color = new Color(sjs2 == 0 ? sjs : 1, sjs2 == 1 ? sjs : 1, sjs2 == 2 ? sjs : 1, sjs2 == 3 ? sjs : 1);
            //MeshRenderer meshRenderer = nextPointBox.GetComponent<MeshRenderer>();
            //meshRenderer.material.SetColor("_BaseColor", color);
            god.name = $"使用技能坐标_{SkillUserGobsCache.Count + 1}";
            god.transform.position = pos;
            god.transform.localScale = UnityEngine.Vector3.one;
            SkillUserGobsCache.Enqueue(god);
        }

        protected void AddSkillUserRPCGobCache(UnityEngine.Vector3 pos)
        {
            return;
            GameObject god = null;
            if (SkillUserRPCGobsCache.Count < 2)
            {
                //god = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/SkillUserRPCGobCache") as GameObject);
                god = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/SkillUserRPCGobCache");
            }
            else
            {
                god = SkillUserRPCGobsCache.Dequeue();
            }
            //float sjs = UnityEngine.Random.Range(0.0f, 1.0f);
            //int sjs2 = UnityEngine.Random.Range(0, 3);
            //Color color = new Color(sjs2 == 0 ? sjs : 1, sjs2 == 1 ? sjs : 1, sjs2 == 2 ? sjs : 1, sjs2 == 3 ? sjs : 1);
            //MeshRenderer meshRenderer = nextPointBox.GetComponent<MeshRenderer>();
            //meshRenderer.material.SetColor("_BaseColor", color);
            god.name = $"使用技能坐标_{SkillUserRPCGobsCache.Count + 1}";
            god.transform.position = pos;
            SkillUserRPCGobsCache.Enqueue(god);
        }

        protected void AddWayPointsGobCache(UnityEngine.Vector3 pos, Color color, int i)
        {
            return;
            //GameObject nextPointBox = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/WayPointsGobCache") as GameObject);
            GameObject nextPointBox = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/WayPointsGobCache");

            MeshRenderer meshRenderer = nextPointBox.GetComponent<MeshRenderer>();
            meshRenderer.material.SetColor("_BaseColor", color);
            nextPointBox.name = $"路点_{i}";
            nextPointBox.transform.position = pos;
            nextPointBox.transform.localScale = UnityEngine.Vector3.one * (1f + (i * 0.2f));
            WayPointsGobsCache.Add(nextPointBox);
        }

        #endregion

        #region 销毁
        private void DestroyPosGobsCache()
        {
            int count = PosGobsCache.Count;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.GameObject.Destroy(PosGobsCache.Dequeue());
            }
            PosGobsCache.Clear();
        }

        private void DestroyRotPointGobsCache()
        {
            int count = RotPointGobsCache.Count;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.GameObject.Destroy(RotPointGobsCache.Dequeue());
            }
            RotPointGobsCache.Clear();
        }

        private void DestroySkillMoveGobsCache()
        {
            int count = SkillMoveGobsCache.Count;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.GameObject.Destroy(SkillMoveGobsCache.Dequeue());
            }
            SkillMoveGobsCache.Clear();
        }

        private void DestroySkillUserGobsCache()
        {
            int count = SkillUserGobsCache.Count;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.GameObject.Destroy(SkillUserGobsCache.Dequeue());
            }
            SkillUserGobsCache.Clear();
        }

        private void DestroySkillUserRPCGobsCache()
        {
            int count = SkillUserRPCGobsCache.Count;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.GameObject.Destroy(SkillUserRPCGobsCache.Dequeue());
            }
            SkillUserRPCGobsCache.Clear();
        }

        private void DestroyWayPointsCache()
        {
            for (int i = WayPointsGobsCache.Count - 1; i >= 0; i--)
            {
                UnityEngine.GameObject.Destroy(WayPointsGobsCache[i]);
            }
            WayPointsGobsCache.Clear();
        }
        #endregion

        #endregion

        #region 主角寻路
        /// <summary>
        /// 每次 路点 移动结束的通知.  服务器 路点有多个, 那么 当移动到最后一个点的时候 触发这个 Action
        /// </summary>
        public Action<bool> ActionOnPathMoveEnd = null;
        public System.Action<bool> OnFindPathCallBack = null;   // 主角寻路委托
        public System.Action OnFindPathIndexCallBack = null;   // 主角寻路委托

        private bool m_FindPath;
        public bool Is_MainPlayer_FindingPath
        {
            get { return m_FindPath; }
            set
            {
                if (m_FindPath != value)
                {
                    m_FindPath = value;
                }
            }
        }

        #endregion

        public virtual bool CheckClientMainPlayerFindingPath()
        {
            return false;
        }

        public virtual void ChangeState(GameKeyCommand gameKeyCommand /*bool canSetSameState = false强制刷新*/ , I_AnimParam i_AnimParam, bool isCanEqual)
        {
            //AOI dont do anything
        }


        #region 路点移动逻辑


        /// <summary>
        /// 设置路点移动的信息
        /// 2024/4/24
        /// note:
        ///     1. 路点位移 只是一个移动方式, 理论上可以提供给 任何调用方调用, 所以 不存在所谓的 任务可以寻路,但是自动战斗 不应该寻路;
        ///     2. 路点位移 是一个持续的 位移状态, 基于上, 其实可以 添加相应的 路点位移类型,  比如任务寻路开始/结束、 自动战斗寻路、
        ///     3. 状态的维护，遵循谁调用，谁 维护。所以应该 提供统一上层接口， 统一维护状态.
        /// 
        /// note: 
        ///     目前 路点调用 接口在 TaskHelp 中, 在接口设计上 不太一致. 
        ///     所以 自动战斗那边 的寻路 结束的时候，依旧没办法 区分 是任务寻路结束、小地图寻路结束 还是 自动战斗自己的寻路.
        ///     而自动战斗 需要区分是 小地图寻路 还是 任务寻路。 小地图寻路结束后 需要 记录自动战斗的起始点.
        /// </summary>
        /// <param name="potions">路点</param>
        /// <param name="action">回调</param>
        public void SetWayPointData(UnityEngine.Vector3[] potions, System.Action<bool> action, Action indexCallBack = null)
        {
            Is_MainPlayer_FindingPath = true;//循环

            AddServerV3WayPointsCache(potions);
            ContinuousEnterWaypointDataByIndex();

            Action<bool> completeCb = action;
            // 如果是主角, 将主角开始/结束的事件通知给外部
            if (IsMainPlayer)
            {
                GameManager.Instance.TriggerEvent("FindingPath", true);

                completeCb = (result) =>
                {
                    GameManager.Instance.TriggerEvent("FindingPath", false);
                    action?.Invoke(result);
                };
            }
            OnFindPathCallBack += completeCb;
            if (indexCallBack != null)
            {
                OnFindPathIndexCallBack += indexCallBack;
                //SGF.Debuger.LogWarning($"寻路看看 id={EntityId} ++++++++++++++----------");

            }

            ClientExecuteWayPoint(true, false);   //客户端自我Nav寻路开启
        }

        public void BreakFindPath()
        {
            if (!Is_MainPlayer_FindingPath) { return; }
            MainPlayer_Clear_FollowUp_Target(); // 清空路点
            //ActionStopMoveDotween?.Invoke("BreakFindPath");
            nextMovePoint = UnityEngine.Vector3.zero;

            DoPathMove?.Invoke(nextMovePoint);
            OnFindPathCallBack?.Invoke(false);  // 路点打断通知
            OnFindPathCallBack = null;
            OnFindPathIndexCallBack = null;
            //SGF.Debuger.LogWarning($"寻路看看 id={EntityId} 情空----------11111111");

            ActionOnPathMoveEnd?.Invoke(false);
            if (Data.isMainPlayer)
            {
                GlobalEvent.OnHitEndEvent?.Invoke(null);    // 清空轻提示
            }
            Is_MainPlayer_FindingPath = false;
            //SGF.Debuger.LogWarning("清空路点 333333333333333333");
        }

        /// 同时被客户端/服务器更新取得的：逻辑层（服务器可以刷新认为我已经到终点，我也可以自己走完=处理网络）。
        /// 主角和怪都有路点组（客户端/服务器的Nav），主角有特殊手柄移动。 
        /// 是否需要强制走新的路店，或者保持老的终点
        public void ClientExecuteWayPoint(bool needInvokeNewPoint, bool isCBIndexPath)
        {
            if (Data.isMainPlayer)
            {
                //SGF.Debuger.Log($"[Move] ClientExecuteWayPoint count: {FindPathPoints.Count}");
            }
            if (!M_IsAlive)
            {
                return;
            }
            //[客户端是已经都走完了-闲置]不论强制请求与否，首先过滤一层是否能推出新数据，不能则立刻暂停
            if (FindPathPoints.Count == 0)
            {
                if (isCBIndexPath)
                {
                    OnFindPathIndexCallBack?.Invoke();
                }
                OnFindPathCallBack?.Invoke(true);  // 路点打断通知
                OnFindPathCallBack = null;
                OnFindPathIndexCallBack = null;
                //SGF.Debuger.LogWarning($"寻路看看 id={EntityId} 情空----------");

                ActionOnPathMoveEnd?.Invoke(true);

                ResetToIdleState();
                if (Data != null && Data.isMainPlayer)
                {
                    GlobalEvent.OnHitEndEvent?.Invoke(null);
                }
            }
            else//有数据才继续决定
            {
                //ChangeState(GameKeyCommand.MoveCommand);//怪还是应该保持默认Idle，因为还有攻击，攻击之后就可以不处理了
                bool isFindNextPoint = InvokeNextPointTarget(needInvokeNewPoint);
                if (Data != null && Data.isMainPlayer)
                {
                    if (isCBIndexPath)
                    {
                        OnFindPathIndexCallBack?.Invoke();
                    }
                    if (isFindNextPoint)
                    {
                        // TODO: 曲
                        // 根据曲爷的提示 临时增加的逻辑. 
                        // 后续 需要 曲 将此处 tips 的逻辑 提出路点寻路 的流程中. 此处是临时增加的 tips 逻辑
                        //string tipsStr = Service.Battle.BattleManager.Instance.IsAutoBattling ? "自动战斗中" : "正在前往";
                        // TODO: 曲
                        // 2024/3/5 [主HUD优化单策划提]
                        // 自动战斗不显示文字提示了，因为自动战斗的图标上会做动效
                        //string tipsStr = GameConfig.LocalStr["AutoPathfinding"];
                        string tipsStr = LanguageManager.Instance.GetLanguageByKey("AutoPathfinding");
                        GlobalEvent.OnHitStarEvent?.Invoke(tipsStr);
                    }
                    else
                    {
                        GlobalEvent.OnHitEndEvent?.Invoke(null);
                    }
                }
                if (!isFindNextPoint)
                {
                    ResetToIdleState();
                }
                else
                {
                    if (isCBIndexPath)
                    {
                        OnFindPathIndexCallBack?.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// 刷新下一个路点执行的逻辑
        /// 由子类去决定做不同的事
        /// </summary>
        public virtual void OnFreshToNextPath()
        {
            //OnFreshToNextPath do anything
        }

        /// DeqNewPoint：默认永远找新路点：为速度变更而设计。
        public bool InvokeNextPointTarget(bool needDequeueNewData = true)
        {
            if (!M_IsAlive)
            {
                return false;
            }

            UnityEngine.Vector3 pos = UnityEngine.Vector3.one * 1000;

            if (needDequeueNewData)
            {
                bool find = false;
                do
                {
                    pos = FindPathPoints.Dequeue(); //只要被表现层请求路店的时候会给设置一下，因为两点之间都是直线设置一次，开始设置就都对
                    if (pos != nextMovePoint)
                    {
                        //取得新数据
                        find = true;
                        break;
                    }
                } while (FindPathPoints.Count != 0);

                if (!find)
                {
                    return false;
                }
            }
            else
            {
                //保持原来数据
                if (nextMovePoint == UnityEngine.Vector3.zero)
                {
                    //不改变当前目标，如果是Zero就Return
                    return false;
                }
                else
                {
                    pos = nextMovePoint;
                }
            }
            nextMovePoint = pos;
            nextMovePoint.y = CurrentPos.y;
            OnFreshToNextPath();    // 这里有疑点
            I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.SingleMoving);
            ChangeState((GameKeyCommand)E_ULayerSubState.SingleMoving, animParam, false);
            UnityEngine.Vector3 dir = nextMovePoint - Position();
            //长度==0的时候：1不修改位置，2不修改朝向
            if (dir.magnitude > 0.01f)
            {
                //if (EntityId == GameManager.Instance.mainPlayerId)
                //{
                //    SGF.Debuger.LogError($"实体角度调试 111111 [InvokeNextPointTarget] EulerAngles={EulerAngles.y},ServerAngles={ServerAngles},nextMovePoint={nextMovePoint},Position={Position()},magnitude={dir.magnitude}");
                //}
                M_EntityMoveDir = dir;
                //if (EntityType == E_EntityType.BulletEntity)
                //{
                //    float clientRot = (float)(Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg) % 360;
                //    float ServerRot = (int)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg % 360);
                //    SGF.Debuger.Log($"实体角度调试 [FreshToNextPath] id={EnityId},EulerAngles={EulerAngles.y},ServerAngles={ServerAngles},clientRot={clientRot},ServerRot={ServerRot},nextMovePoint={nextMovePoint},Position={Position()}");
                //}
                ClientSetRotationByDir(dir, true, 0);
            }
            else
            {
                //SGF.Debuger.LogError($"实体角度调试 [FreshToNextPath222] id={EnityId},EulerAngles={EulerAngles.y},ServerAngles={ServerAngles},nextMovePoint={nextMovePoint},Position={Position()}");
            }

            //if (EntityType == E_EntityType.BulletEntity)
            //{
            //    float clientRot = (float)(Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg) % 360;
            //    float ServerRot = (int)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg % 360);
            //    SGF.Debuger.LogError($"子弹移动调试 [FreshToNextPath] id={EntityId},EulerAngles={EulerAngles.y},ServerAngles={ServerAngles},clientRot={clientRot},ServerRot={ServerRot},nextMovePoint={nextMovePoint},Position={Position()}");
            //}

            // 当没有禁止 路点移动的时候, 将需要位移的路点 推送到表现层
            DoPathMove?.Invoke(nextMovePoint);
            return true;

            //if (EntityId == GameManager.Instance.mainPlayerId)
            //{
            //    SGF.Debuger.LogError($"实体角度调试 222222 [InvokeNextPointTarget] EulerAngles={EulerAngles.y},ServerAngles={ServerAngles},nextMovePoint={nextMovePoint},Position={Position()},magnitude={dir.magnitude}");
            //}
        }

        public void CacheServerWayPointsData(object v)
        {
            ByteString vector3Msg = (ByteString)v;
            byte[] msgData = vector3Msg.ToByteArray();
            IMessage pbMessage = ProtoUtils.Deserialize((int)MsgIDEnum.ArrayVector3ID, msgData);
            ProtoMsg.ArrayVector3 arrayVector3 = (ProtoMsg.ArrayVector3)pbMessage;
            AddServerV3WayPointsCache(arrayVector3.ArrayVector3_);
            // 如果 服务器 清空了路点, 那就是服务器认为客户端已经到了目标节点
            // 而此时 客户端可能运行到一半, 所以, 客户端需要根据服务求 下发的 坐标,
            // 去比较是否移动到了目标节点, 如果没有, 那就用 服务器下发的玩家坐标, 去设置最终的路点坐标
            // if (vector3PosesCache.Count == 0)
            // {
            //     if (CurrentPos.x != ServerPosition.x || CurrentPos.z != ServerPosition.z)
            //     {
            //         vector3PosesCache.Add(ServerPosition);
            //     }
            // }
        }

        protected void AddServerV3WayPointsCache(RepeatedField<global::ProtoMsg.Vector3> vector3s)
        {
            ClearServerV3WayPoints();

            #region TestWaypoint
            Color color = Color.white;
            //if (EntityType == E_EntityType.Partner && vector3s.Count > 0)
            if (EntityType == E_EntityType.Monster)
            {
                DestroyWayPointsCache();
                float sjs = UnityEngine.Random.Range(0.0f, 1.0f);
                int sjs2 = UnityEngine.Random.Range(0, 3);
                color = new Color(sjs2 == 0 ? sjs : 1, sjs2 == 1 ? sjs : 1, sjs2 == 2 ? sjs : 1, sjs2 == 3 ? sjs : 1);
            }
            #endregion

            UnityEngine.Vector3 initPos = UnityEngine.Vector3.zero;
            for (int i = 0, len = vector3s.Count; i < len; i++)
            {
                ProtoMsg.Vector3 v3 = vector3s[i];
                initPos.x = v3.X;
                initPos.y = v3.Y;
                initPos.z = v3.Z;
                serverV3WayPointsCache.Add(initPos);
                //SGF.Debuger.LogWarning($"{TagFlag} SetPathPoses EnityId=>{EnityId},i={i},initPos={initPos}");

                #region TestWaypoint
                if (EntityType == E_EntityType.Monster)
                {
                    AddWayPointsGobCache(initPos, color, i);
                }
                #endregion
            }
        }

        public void AddServerV3WayPointsCache(UnityEngine.Vector3[] vector3s)
        {
            ClearServerV3WayPoints();

            #region TestWaypoint
            Color color = Color.white;
            //if (EntityType == E_EntityType.Player && vector3s.Length > 0)
            if (EntityType == E_EntityType.Monster)
            {
                DestroyWayPointsCache();
                float sjs = UnityEngine.Random.Range(0.0f, 1.0f);
                int sjs2 = UnityEngine.Random.Range(0, 3);
                color = new Color(sjs2 == 0 ? sjs : 1, sjs2 == 1 ? sjs : 1, sjs2 == 2 ? sjs : 1, sjs2 == 3 ? sjs : 1);
            }
            #endregion

            for (int i = 0, len = vector3s.Length; i < len; i++)
            {
                serverV3WayPointsCache.Add(vector3s[i]);
                //SGF.Debuger.LogWarning($"{TagFlag} SetPathPoses EnityId=>{EnityId},i={i},initPos={initPos}");

                #region TestWaypoint
                if (EntityType == E_EntityType.Monster)
                {
                    AddWayPointsGobCache(vector3s[i], color, i);
                }
                #endregion
            }
        }

        /// <summary>
        /// 持续录入有效数据[在后续叠加数据]
        /// 返回是空/全数据/部分数据（因AOI需要帮服务器裁剪处理的）
        /// </summary>
        /// <param name="v">有效数据索引,默认都是0</param>
        public void ContinuousEnterWaypointDataByIndex(int validDataIndex = 0)
        {
            if (serverV3WayPointsCache == null || serverV3WayPointsCache.Count == 0)
            {
                //空数据：检测服务器是否漏发了路点集合 或 路点集合与下标消息顺序倒置了。
                return;
            }
            //怪物寻路过程中:其他玩家B进入怪物AOI，服务器路点集合没裁剪，只是告诉我B玩家的index。

            //裁剪无效数据，只录入有效数据。
            for (int i = validDataIndex; i < serverV3WayPointsCache.Count; i++)
            {
                FindPathPoints.Enqueue(serverV3WayPointsCache[i]);
            }
            //SGF.Debuger.LogError(EnityId + "___" + FindPathPoints.Count);
        }

        #endregion

        #endregion

        #region 动画参数
        public I_AnimParam GetAnimParamByState(E_ULayerSubState e_ULayerSubState, string animName = "")
        {
            AnimParam animParam = new();
            animParam.Reset();
            animParam.SetFadeInTime(100);
            animParam.SetBaseAnimSpeed(GetAnimSpeed(e_ULayerSubState));
            if (!string.IsNullOrEmpty(animName))
            {
                animParam.SetBaseAnimName(animName);
            }
            return animParam;
        }

        private float GetAnimSpeed(E_ULayerSubState e_ULayerSubState)
        {
            float speed = 1.0f;
            switch (e_ULayerSubState)
            {
                case E_ULayerSubState.WanderMoving:
                    {
                        speed = Speed / m_entityBaseWalkSpeed;
                    }
                    break;
                case E_ULayerSubState.SingleMoving:
                case E_ULayerSubState.BattleMoving:
                case E_ULayerSubState.WeaponRetractionMoving:
                    {
                        speed = Speed / m_entityBaseRunSpeed;
                    }
                    break;
                default:
                    break;
            }
            speed = 1.0f; // TODO: 2023/04/10 策划要再讨论【移速】对【移动动作播放速度】的规则
            return speed;
        }

        #endregion

        #region 播放特效的接口
        /// <summary>
        /// 播放 特效的接口
        /// key : 预留的一个 参数key
        /// </summary>
        public void PlaySpecialEffect(I_FxParam fxParam, string key = "")
        {
            AvatarDataCell avatarData = avatarDataCell;
            if (fxParam.BuilderID != EntityId)
            {
                AvatarDataCell model = GameManager.Instance.GetEntityAvatarById(fxParam.BuilderID);
                if (model != null)
                {
                    avatarData = model;
                }
            }
            if (avatarData == null)
            {
                SGF.Debuger.LogWarning($"PlaySpecialEffect() id={EntityId},type={EntityType},avatarData=null");
                return;
            }
            ActionOnPlaySpecialEffects?.Invoke(fxParam, key, avatarData.EffectsPath);
        }

        #endregion

        #region 通用属性相关处理
        bool isBattleState_BlackHole = false;
        /// <summary>
        /// 是否是 第一次 进入 黑洞效果
        /// </summary>
        bool isFirstEnterBlachHole = false;

        /// <summary> AOI [坐标] 变化 </summary>
        /// 常规处理，就是直接设置位置，直接拉
        protected virtual void OnAOIPositionChange(string key, object val)
        {
            // 主角自己不需要同步坐标和角度，是客户端自己控制
            // 主角自己控制/RPC强拉/移动不需要路店（除了任务和寻路）
            object value = AttrData.GetProtoValue(key);
            if (Data == null)
            {
                ServerSetPosition(value, false, false);
                return;
            }
            isBattleState_BlackHole = Data.IsBattleState_BlackHole;

            if (!isBattleState_BlackHole)
            {
                isFirstEnterBlachHole = false;
            }

            /// 2023/9/25
            /// 属性同步增加了 黑洞效果. 在黑洞效果作用下, 主角也需要走属性同步(客户端完全信赖服务器数据)
            /// note:
            ///     在客户端播放技能的时候,如果此时存在位移,那该如何处理？
            if (!Data.isMainPlayer || isBattleState_BlackHole)
            {
                if (!Data.Is___ForbidMove)
                {
                    if (EntityType == E_EntityType.Monster || isBattleState_BlackHole)
                    {
                        ByteString vector3Msg = (ByteString)value;
                        byte[] msgData = vector3Msg.ToByteArray();
                        IMessage pbMessage = ProtoUtils.Deserialize((int)MsgIDEnum.Vector3ID, msgData);
                        ProtoMsg.Vector3 protoPos = (ProtoMsg.Vector3)pbMessage;
                        UnityEngine.Vector3 tempV3 = UnityEngine.Vector3.zero;
                        tempV3.x = protoPos.X;
                        tempV3.y = protoPos.Y;
                        tempV3.z = protoPos.Z;
                        AddPosGobCache(tempV3);
                        if (isBattleState_BlackHole)
                        {
                            // 如果是 第一次进入黑洞, 此时客户端 可能 跑在服务器的前面去，而且还没给服务器发
                            // 那么 此时 第一次的移动向量 就 是 服务器坐标 - 客户端当前坐标.
                            if (isFirstEnterBlachHole)
                            {
                                isFirstEnterBlachHole = true;
                                m_EntityServerFrameMoveDir = tempV3 - CurrentPos;
                            }
                            else
                            {
                                // 如果进入了 黑洞效果后, 后续的移动 由服务器每帧 驱动.
                                // 那此时的移动向量就是 服务器推送的 每帧间隔
                                // 此处 采用 += 是因为 在一帧之内, 可能受到多次的 
                                m_EntityServerFrameMoveDir += tempV3 - ServerPosition;
                            }

                            UnityEngine.Vector3 v3 = tempV3 - ServerPosition;
                            SGF.Debuger.LogWarning($"[OnAOIPositionChange] [黑洞] 坐标同步: curPos: {Position()} ---> {tempV3}, m_EntityServerFrameMoveDir: {m_EntityServerFrameMoveDir}  need MoveDir: {M_EntityMoveDir},serverMovePos: {v3} forbidMove: {Data.Is___ForbidMove}, forbidDir: {Data.Is___ForbidDir} ");
                        }

                    }

                    ServerSetPosition(value, IsFirstPos, true);
                    if (IsFirstPos)
                    {
                        IsFirstPos = false;
                    }
                    //if (EntityType == E_EntityType.BulletEntity)
                    //{
                    //    SGF.Debuger.LogError($"子弹移动调试 [OnAOIPositionChange] 2222222 id={EntityId},curposition={Position()},IsFirstPos={IsFirstPos}");
                    //}
                }
                else
                {
                    ServerSetPosition(value, false, false);
                }

                // 朝向移动的设置 挪到了 玩家真正move到一个点后 设置,此处屏蔽
                {
                    /// 2023/06/29
                    /// 如果 其他人的原子锁 没有锁住,并且是 简单的移动,那就使用 玩家到下一个点的 移动朝向
                    // if (!Data.Is___ForbidDir && IsSimpleMovingState())
                    // {
                    //     UnityEngine.Vector3 moveDir = new UnityEngine.Vector3(ServerPosition.x - CurrentPos.x, 0, ServerPosition.z - CurrentPos.z);
                    //     if (moveDir.magnitude > 0.1)
                    //     {
                    //         SGF.Debuger.LogError($"[Rotate] entityID: {EntityId} 服务器 [AOI] 坐标 , IsSimpleMovingState: {IsSimpleMovingState()}, 设置移动朝向: {moveDir} ");
                    //         ClientSetRotationByDir(moveDir, true, 0);
                    //     }
                    // }
                }
            }
            else
            {
                // 主角并且 没有受到 黑洞效果时,客户端会采用自己的摇杆坐标,所以只设置 服务器 坐标，但是不 同步 view
                ServerSetPosition(value, false, false);
            }
        }

        //同步延迟次数
        //public int 
        /// <summary>
        /// 既然采用了持续录入机制，所以服务器发空数/零值，都不可代表立刻停下
        /// 因为机制是当前目标走完且所有节点走完，如果想通过强同步来停止目前移动，或立刻过度到其他地点需新增协议或通过参数或强同步
        ///【机制要健全，谨防踩10年所有的坑，还不如自己设计，别被代偏有自己的设计】
        /// </summary>
        public void foo()
        {

        }

        #region 怪物:【寻路/追杀】：怪物有路点是因为怪物的行动是服务器推算出来的;人是自己走出来的
        ///########################################################################服务器消息顺序已经排序过##########################################################################
        ///#######################################第一条消息一定先到#######################################跟海星重新整理过AOI，现在先3904后3905
        ///################################################################################################消息是point—index-p-index-p-index,
        /// <summary> AOI [路点] 变化 </summary>
        /// 1，坐标集合
        /// 2，寻路坐标发生变化会直接同步给我一套
        /// 3，[此是NuLL处决定 停/走]
        /// 4，[vector3PosesCache:][全数据/count == 0]
        /// 策略：AOI数据组来了以后，[继续录入]，然后该走的要走完
        /// 服务器延迟发送了，我会停下；提前发送了我会继续走
        protected virtual void OnAOIWaypointsChange(string key, object val)
        {
            // 如果走的是 客户端的模拟运动, 那 属性的路店同步 也不需要处理
            if (IsSimulateMove)
            {
                return;
            }
            object value = AttrData.GetProtoValue(key);
            //准备数据
            CacheServerWayPointsData(value);

            //更新：20230214，停止也是在目标点；
            //FindPathPoints.Clear();
            //持续录入有效数据
            //ContinuousEnterWaypointDataByIndex();
            //ServerUpdatePath(false);
        }

        ///#######################################第二条消息一定后到[处理决策]#######################################

        /// <summary>
        /// 服务器寻路同步信息元素：1，寻路全路点；2，索引下标[（过程不给:作用于区分后加入AOI的人）]；3，同步验证
        /// <summary> AOI [路点下标] 变化 
        /// 同步设定1：首次可能告诉我不同的index，二次告诉我都是相同的index=
        /// 同步设定2：通常服务器给了路径然后才会给index
        /// 设定3：服务器在下标变更的时候通知给客户端，客户端需自己模拟
        /// 设定4：下表作用：共【1，2，3，4，5】个路点，玩家A持续在AOI中收到【1~5】（ idx = 0 ）；
        /// 玩家B后进来收到【1~5】（idx = 3）
        /// 此消息同步时机：新路点组过来，或我AOI进入已经寻路的其他怪物组内
        /// 方法作用：全程知识持续录入有效信息
        /// 信息：同步首先是基于网络不波动的情况，所以基础不需要考虑网络突然好了的先到，和突然卡了的后到；在（平滑网络）的情况下收到就意味着消息需要处理；
        /// 【我收到就意味着立刻改变目标；空是立刻清理目标】
        /// 提前和延后的定义：约定延迟为X每次固定Tick时间通过X传递过来，原来大于X后续小于X或者网速比预计的快都是提前，后续延迟就是滞后
        /// 所以：服务器告诉我们，就是中断，不论后来有值还是没有
        /// 1：服务器是没走的时候，提前算一下路点发过来
        /// 2：然后会模拟移动，按照逻辑帧不连续的点进行闪烁
        /// 3：发送空这时候代表着他此时此刻没有目标（此时会告诉我同步同步我会强拉），有数据就是改变了
        /// 4：所有情况基础基于不波动考虑
        /// 5：本质就是延迟固定时间处理
        /// 6：决定曾就是index帮忙处理拆分数据
        /// 7：index清除 目标为0 停下来
        /// 8：所以才会有校验机制。快速同步作为客户端处理手段
        /// 9：然后分组按照1.5的N次幂来处理加速，变更在单个点，或者路点组变化的时候进行N的变更
        /// </summary>
        /// <param name="key"></param>
        protected virtual void OnAOICurrPathIndexChange(string key, object val)
        {
            int value = AttrData.GetAoiValue<int>(EnumAOIType.Int, key);
            //1,"-1"是立刻停下；index索引越界即为不合法不处理
            if (serverV3WayPointsCache.Count == 0 || value == -1)
            {
                //路点清空(或jineng立刻打断)
                //3服务器空就是别的技能要停下来，走完停下来；有数据在走就会一直发非空
                //3.05空就是清理缓存，清理当前，停下来；新的就立刻修改，怪物不必走完
                //3.1  路店下标一起发，想做事情的时候就发下表
                //3.2  怪物无所谓，停就是停止，怪物也没有组的组；thd的pos单点才有组，thd必须走完
                //3.3  所以怪物停下来就是空对应下标是-1，机制复杂可能出现各种情况，停止（到地方了，放技能了）通过空；              
                //3.4  机制上我是基于下标执行的，路店是一组数据我是帮忙处理，下表是处理机制处理正确数据
                //3.5  所以空就停下来，清理目标，立刻打断，目标是0，执行技能
                //3.6  所以如果是空或者下表-1一起出现或者出现一个，就清理；              
                //3.7  发给我别的呢，我也要立刻中断巡逻到追杀，鑫目标
                //3.8  所以任意情况都是清空
                //3.9  永远执行新目标，或者停下来
                //3.10 服务器发怪物是绝对服从 空停，新目标立刻改变
                Clear_Current_FollowUp_Target();
                return;
            }

            if (value >= serverV3WayPointsCache.Count)
            {
                //数据count == 1；索引是1就是错误
                return;
            }

            //怪物推送新的立刻执行，因为怪物寻路和处理逻辑在服务器，情况比较多====thd其他人是玩家走出来的所以要模拟，怪物是逻辑推算出来的，所以路点就是决定走和暂停的一切
            Clear_FollowUp_Target();
            // 更新：20230214，停止也是在目标点；
            //FindPathPoints.Clear();

            //持续录入有效数据
            ContinuousEnterWaypointDataByIndex(value);

            Is_MainPlayer_FindingPath = true; // 开始路点移动
            // 判断下原子锁移动
            if (Data.Is___ForbidMove)
            {
                // 如果锁了就不走，等下次解开的时候再移动
                //SGF.Debuger.LogWarning("禁止移动  如果锁了就不走，等下次解开的时候再移动");
                return;
            }

            //queue空怎么处理 - Zero - 停止
            //中断重刷情况，服务器首次(新）/后续驱动（新）/玩家走完（新）/数据到了
            if (IsFirstAOI)
            {
                //首次驱动是通过服务器来的；还有一个目的是服务器数据迟到了玩家都走完了，还要再次启动或驱动
                ClientExecuteWayPoint(true, false);//服务器破坏更新循环
                IsFirstAOI = false;
            }
            else //数据到了
            {
                /// 网络不波动的情况下：持续走动过程中 处理如下
                ///  【服务器发送空】：没数据就是停下来不继续走；清理数据目标是0；就不走跟T/F没关系 有数据决定-想停下来-
                ///  【服务器发送值】：一定是新的数据目标；（清理-停下来zero），取得新目标；一定是T，
                ClientExecuteWayPoint(true, false);
            }
            //Debug.LogError(EnityId + "___Index");
        }

        #endregion

        /// <summary> AOI [角度] 变化 </summary>
        protected virtual void OnAOIRotChange(string key, object val)
        {
            //第一次服务器设定转向（todo）
            //1，移动主角角度需过滤
            //2, 技能转向不需要处理（服务器也没发，移动是到后面，所以我特殊写的移动不改变当前对怪物的朝向（逻辑层不改变rotate））
            //3，主角是自己控制转向
            //小曲发给服务器，但是自己并未转向,实际上告诉服务器移动方向和转向，自己移动，但并未转向
            //其实这段时间属于移动，本来就可以打断后摇，也可以走
            //移动------锁定，转向，位移
            //策划想------移动太短，想长一点，不然看起来抖一下（111111后摇短一点时间，或者后摇不可被打断），所以普通攻击连击（想加Cd，想加一个暗锁，【22222不如明锁】，之后就算加队列（也应该基于明锁））【333必须要有秒（0.1）】
            int value = AttrData.GetAoiValue<int>(EnumAOIType.Int, key);
            //if (EntityType == E_EntityType.BulletEntity)
            //{
            //    UnityEngine.Vector3 dir = Utils.Rota2Vector(value);
            //    float clientRot = (float)(Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg) % 360;
            //    float ServerRot = (int)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg % 360);
            //    SGF.Debuger.Log($"实体角度调试 OnAOIRotChange id={EnityId},EulerAngles={EulerAngles.y},ServerAngles={ServerAngles},clientRot={clientRot},ServerRot={ServerRot},isRotFirstAoi={isRotFirstAoi}");
            //}

            if (EntityType == E_EntityType.Monster)
            {
                AddRotPointGobCache(value);
            }

            //网络情况分为刚进入卡顿，和卡顿拥挤刚结束
            //原设定是首次直接设定，后续慢慢转换
            //不可依赖队列的空和否，数据空间换表现，还是要存就要new一个对象，虽然是局部new，还是需要避免
            //策略上，不记录也可以：1首次就是直接设置是兼容的，2数据是连续的中断后第一个数据立刻设置，立即设定如果转角很慢首次就很奇怪，但是整体策略就是可能出现光法的飘的问题
            //就算移动速度很快，依然策略上不能出错，因为这并不是选择是可以兼容的
            //所以取得设定就好，节省数据，不影响设定必定要缓存的
            bool isSyncView = true;

            if (Data != null && Data.isMainPlayer && IsFirstRot)
            {
                ClientSetRotation(value, false, 0);
            }

            // 朝向的 属性同步 如果被原子锁锁住了,此时 只同步 服务器朝向数据, 但是不同步 view 层;
            if (Data != null)
            {
                bool isForbidDir = m_playerData.Is___ForbidDir;

                if (Data.isMainPlayer)
                {
                    isSyncView = false;
                }
                else
                {
                    /// 2023/06/29
                    /// 如果是 其他人的 简单移动(排除 战斗移动), 就需要考虑 是否采用 服务器的 属性同步朝向问题
                    /// 如果 禁止原子锁的 朝向, 那表现上 其实 就该使用 服务器同步过来的朝向
                    ///      没禁止， 就应该采用 移动的 下一个点的朝向
                    if (IsSimpleMovingState())
                    {
                        isSyncView = isForbidDir ? true : false;
                    }
                    else
                    {
                        isSyncView = isForbidDir && !IsFirstRot ? false : isSyncView;
                    }

                    //不是 主角的时候时候, 如果是 黑洞状态 ， 朝向直接采用服务器的朝向
                    if (Data.IsBattleState_BlackHole)
                    {
                        isSyncView = true;
                    }
                }
            }

            //SGF.Debuger.LogError($"[Rotate] entityID: {EntityId} 服务器 [AOI] 朝向 : {value} , IsSimpleMovingState: {IsSimpleMovingState()}, isSyncView: {isSyncView} ");

            ServerSetRotation(value, !IsFirstRot, isSyncView, 0);
            if (IsFirstRot)
            {
                IsFirstRot = false;
            }
        }

        /// <summary>
        /// 当玩家 动画状态改变的时候 的通知。
        /// 目前 跟夏哥的约定 这个 状态 专门用来同步 在黑洞状态下 第三方的 移动状态。
        /// 对于怪物来说， 在黑洞状态下 一般 是静止 的被 黑洞拉扯.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        protected void OnMoveStateChange(string key, object val)
        {
            // 目前 这个状态 服务器发过来的是 一个 时间戳。
            // 如果 超过这个时间戳一定的时间范围， 客户端认为动画状态 取消
            if (!Data.IsBattleState_BlackHole)
            {
                return;
            }
            long value = AttrData.GetAoiValue<long>(EnumAOIType.Long, key);

            MoveState = value;

            ActionOnMoveStateChange?.Invoke();



        }

        // protected void OnPvpStateChange(string key, object val)
        // {
        //     PvpState = (int)AttrData.GetAoiValue<uint>(EnumAOIType.Int, key);

        //     UpdatePvpIcon();
        // }

        /// <summary> AOI [实际速度（结果速度）] 变化 </summary>
        protected virtual void OnAOITruthSpeedChange(string key, object val)
        {
            // 目前是客户端做逻辑：限定：服务器也要有这个逻辑
            long value = AttrData.GetAoiValue<long>(EnumAOIType.Long, key);

            //SGF.Debuger.Log($" -{EnityId} : TruthSpeed : {value}");
            if (EntityId == GameManager.Instance.mainPlayerId)
            {
                // 主角现在的速度限制改为读表
                Speed = Mathf.Clamp(value, LocalDataManager.Instance.GetSystemDataCell(42).GetValue() * 1.0f, LocalDataManager.Instance.GetSystemDataCell(43).GetValue() * 1.0f) / 100f;
            }
            else
            {
                Speed = value / 100f;
            }
        }

        bool IsPvpFaction()
        {
            return Faction == 5 || Faction == 6;
        }

        bool IsSameFaction()
        {
            var myFaction = GameManager.Instance.M_MainPlayerCtrlBase.Data.myOwnerNtt.Faction;
            return myFaction == Faction;
        }

        /// <summary> AOI [阵营] 变化 </summary>
        protected virtual void OnAOIFactionChange(string key, object val)
        {
            if (Data == null)
            {
                return;
            }
            Faction = (int)AttrData.GetAoiValue<uint>(EnumAOIType.Uint, key);


            if (Data.EntityType != E_EntityType.Robot
                && Data.EntityType != E_EntityType.Player
                && Data.EntityType != E_EntityType.Monster)
            {
                return;
            }

            if (IsPvpFaction())
            {
                if (!Data.isMainPlayer)
                {
                    SGF.Module.Framework.ModuleManager.Instance.SendMessage(ModuleDef.Name.StarWorldModule, "OnSetPvpFaction", Data.M_EntityID, Faction);
                    if (IsSameFaction())
                    {
                        ActionSetNameAndCor?.Invoke(M_Name, 1003);
                    }
                    else
                    {
                        ActionSetNameAndCor?.Invoke(/*"红方_" + */M_Name, 1103);
                    }
                }
                else
                {
                    ActionSetNameAndCor?.Invoke(M_Name, 1003);
                }
            }
            else
            {
                ActionSetNameOrgCor?.Invoke();
            }
        }

        /// <summary>
        /// 上线前的TODO（Done）：当客户端服务器一切移动协议都完全稳定时候，客户端做保底【距离小于0.01就不处理】
        /// 但是移动过程中预处理后续移动路点的时候很容易出错
        /// </summary>
        /// <returns></returns>
        protected float GetForceSyncValue()
        {
            return 0.166f * Speed;    // 高磊需求的0.5f
            //return 0.5f;    // 高磊需求的0.5f
            //return Speed * GameConfig.S2C_MINI_MOVE_SYNC_TIME;
        }

        #endregion

        #region 模型 缩放因子变化处理
        public void UpdateBoxScaleRatio(UnityEngine.Vector3 boxScaleRatio)
        {
            if (boxScaleRatio != BoxScaleRatio)
            {
                return;
            }

            BoxScaleRatio = boxScaleRatio;

            ActionOnUpdateBoxScale?.Invoke(BoxScaleRatio);
        }

        #endregion


    }


    // 移动补偿的 基础逻辑, 先放这，之后会改成向量补偿的方式


}