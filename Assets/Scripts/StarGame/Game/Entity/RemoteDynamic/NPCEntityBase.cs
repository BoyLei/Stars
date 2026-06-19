using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Network;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Service.Battle;
using StarProject.Service.FindPath;
using StarProject.Service.LocalData;
using StarProject.Service.LocalDynamic;
using StarProject.Service.Sound;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProVec3 = ProtoMsg.Vector3;
using Utils = Fire.Utils;
using Vector3 = UnityEngine.Vector3;

//挂点层有挂点的npc，其实可以hero继承小兵，这里就相当于小兵
//所有有挂点特性的
namespace StarProject.Game.Entity.VitalSigns
{
    [XLua.CSharpCallLua]
    public delegate void EnergyStart(SkillEntity skillEntity);
    [XLua.CSharpCallLua]
    public delegate void EnergyEnd(SkillEntity skillEntity);
    [XLua.CSharpCallLua]
    public delegate void StartSkillStage(SkillEntity skillEntity);
    [XLua.CSharpCallLua]
    public delegate void EndSkillStage(SkillEntity skillEntity, E_SkillExitType e_SkillExitType);

    [XLua.CSharpCallLua]
    public delegate void BuffCreate(SkillBuff skillBuff, BuffCreateRet buffCreateRet);
    [XLua.CSharpCallLua]
    public delegate void BuffEnd(SkillBuff skillBuff, BuffEndRet buffEndRet);
    [XLua.CSharpCallLua]
    public delegate void BuffUpdate(SkillBuff skillBuff);
    [XLua.CSharpCallLua]
    public delegate void SpectralSkillBuff(bool isEnter);

    [XLua.CSharpCallLua]
    public delegate void PassiveCreate(PassiveSkillEntity passiveSkillEntity, PassiveSkillUseRet passiveSkillUseRet);
    [XLua.CSharpCallLua]
    public delegate void PassiveEnd(PassiveSkillEntity passiveSkillEntity, PassiveSkillEndRet passiveSkillEndRet);

    public class SkillMovePara
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="triSec">【触发总体毫秒】：因为用多少个帧做平滑是算出来的：策划这里一定注意，攻击频率小于推送频率，不然多从推/param>
        /// <param name="disVecPerFrame">【每帧多少米（向量）】</param>
        /// <param name="moveEndPos"></param>
        public void Begin(float triSec, Vector3 disVecPerFrame, Vector3 moveEndPos)
        {
            //触发总体毫秒
            TriSec = triSec;
            //【每帧多少米（向量）】
            M_TotleMoveDisVec = disVecPerFrame;
            //结尾坐标
            MoveEndPos = moveEndPos;
            SkillMoveDone = false;
        }

        public bool SkillMoveDone
        {
            get => readyDone;
            set => readyDone = value;
        }

        public float TriSec
        {
            get => rriTimes;
            set
            {
                rriTimes = value;
                if (rriTimes <= 0)
                {
                    SkillMoveDone = true;
                }
            }
        }

        //根据类型
        //每次距离
        //多少次
        //技能运行时触发器只有一个，之后用触发器实现
        private float rriTimes;
        public Vector3 M_TotleMoveDisVec = Vector3.zero;
        public Vector3 MoveEndPos = Vector3.zero;
        private bool readyDone = true;
    }

    [XLua.LuaCallCSharp]
    public abstract class NPCEntityBase : AOIEntityObject
    {
        // 实体坐标发生变化通知小地图
        public override void OnFinalPosChange(UnityEngine.Vector3 newVector3)
        {
            base.OnFinalPosChange(newVector3);
        }
        private string TagFlag
        {
            get { return $"[{EntityId}] [VitalSignsBase]"; }
        }

        public Action<Vector3> ViewEnterFrameAction;

        public const float SEC_PER_FRAME = 1f / GameConfig.FIX_TIME_PER_SEC; //没帧 0.03333秒
        //*帧率限制，不可能0~60；平滑度限制小于多少没必要发
        //TODO：Fix也60：？update可控吗

        public Action<Vector3, float, MoveLabel, MoveType, string, Action<bool>> OffsetMoveAction; // 位移委托  float：传（秒）

        protected NPCEntityBase m_nextMyControl;

        //==================================================================
        protected VitalSignViewShowData m_data; //玩家模型数据：基础,avatarSnakke

        //==================================================================
        /// <summary>
        /// 在 EntityCtrlGroup 中的 index, 不是它自己属性的 Index
        /// </summary>
        protected int m_index;
        public int Index
        {
            get { return m_index; }
        }

        public Action<E_ULayerSubState, I_AnimParam, Action> AnimStatePlay;

        public VitalSignViewShowData ShowData
        {
            get { return m_data; }
        }

        protected override VitalSignAttrData AttrData => Data.Attrs;

        private E_ULayerSubState m_eSubState = E_ULayerSubState.Idle; //当前状态
        public override E_ULayerSubState M_eSubState
        {
            get => m_eSubState;
            //set
            //{
            //    //可以强制同步：因为有部分逻辑是服务器发来的，所以并不排除相同的变更
            //    //优化，设置状态机那边也要加一个逻辑， 相同的状态设置不重复播放动画。 因为技能前摇的阶段，我本地使用技能的时候，会进入一次。 服务器返回后，如果时间很快，也会进入一次
            //    //if (m_eSubState == value)
            //    //{
            //    //    return;
            //    //}
            //    m_eSubState = value;
            //    //SGF.Debuger.Log("m_eSubState" + m_eSubState);
            //    //if (AnimStatePlay != null)
            //    //{
            //    //    //倍率来源不同
            //    //    //最小刷新阶段是，技能阶段
            //    //    //statemix之后作为拓展
            //    //    //【逻辑通知到表现层的速率】

            //    //    if (skillRuntimeParaCache != null)
            //    //    {
            //    //        // 【技能攻速 和 技能位移】
            //    //        AnimStatePlay.Invoke(m_eSubState, skillRuntimeParaCache, m_isSuspendByServer, GetAnimSpeed(skillRuntimeParaCache));
            //    //    }
            //    //    else
            //    //    {
            //    //        // 【常规】
            //    //        AnimStatePlay.Invoke(m_eSubState, null, m_isSuspendByServer, GetAnimSpeed());
            //    //    }
            //    //}
            //    SetSubState(m_eSubState, animParam);
            //}
        }

        //=================================================================
        //TODO：Y:【会中转AOI】【会记录】【但是服务器不提供Nav运算】需要碰撞，找到逻辑层位置了，服务器要，甚至自己也要更新正确
        //服务器是2D数据，战斗的时候Y就无效，服务器会记录Y，但是他不会通过navMesh算Y
        //我们上线是在固定位置出生，还是野外也能存位置？@下线坐标
        //目前单主城记录点，之后不排除多记录点
        //private Vector3 mergePos = Vector3.zero;

        //=================================================================
        protected bool m_IsJoyStickStop = true; //【本实体的是否停止状态，他只能是我/其他人中的一种】就算停止GetDir保留最后方向
        protected bool m_IsDriveByServerStop = false;
        protected bool m_IsSkillJoyStick = false; // 技能遥感

        //----------------------------------------------------------------------
        //=================================================================
        protected List<EntityObject> fxList;

        public SkillMovePara skillMovePara = new();

        //public bool SkillDamageTriggerTag = false;
        //public List<AOIEntityObject> currentTriggetNtts = new List<AOIEntityObject>();


        //public GetEnemyList [持续持有要求的]创建维护攻击者：数学库辅助查找+内封装查找方法
        //
        //游戏上下文/GameMgr[一次性持有]维护一些共有的查找方法，当作BattleManager强同步信息，当作服务器维护AOI
        //=================================================================

        public Action<Vector3, Action> ActionOnForceSyncPosPerSkill;

        /// <summary> 选中目标委托 </summary>
        public Action<bool> ActionOnCheckTarget;
        public bool IsCheckedEntity = false;
        /// <summary> 选中目标移动委托 </summary>
        public Action ActionOnCheckTargetMove;

        /// <summary>
        ///  按实体朝向 播放 模拟子弹抛物线的接口, 纯 表现层 表现
        /// float time, float  maxHight, float recoverTime ,Vector3 direction
        /// </summary>
        public Action<float, float, float, Vector3, Action> ActionOnPlayThrowBullet;

        /// <summary>
        ///  模拟的 贝塞尔曲线
        ///  ulong targetID , vector3 targetPos , EffectTypeBezierBullet 
        /// </summary>
        public Action<ulong, Vector3, EffectTypeBezierBullet, Action> ActionOnPlayBezierBullet;

        /// <summary>
        /// 播放追踪弹
        /// ulong targetEntityID, HangPoint hangPoint, float speed, bool isTargetDie2Stop, Action onComplete
        /// </summary>
        public Action<ulong, HangPoint, float, bool, Action> ActionOnPlayTrackingBullet;

        /// <summary>
        /// 播放 lineRender的 效果 action
        /// EffectTypePlayEffectLineRenderer effect, List<ulong> targets, int startTime, string tagKey
        /// </summary>
        public Action<LineRendererConfig, List<ulong>, int, string> ActionOnPlayEffectLineRenderer;
        public Action<LineRendererConfig, string> ActionOnStopEffectLineRenderer;

        /// <summary>
        /// 刷新 隐藏的 技能槽
        /// </summary>
        public Action<List<int>> ActionOnRefreshHiddenSkillSlot;

        protected override void Init(ulong entityID, E_EntityType entityType)
        {
            base.Init(entityID, entityType);
            if (Data != null)
            {
                ServerPosition = Data.Pos;
            }
            OffsetMoveAction += OnSkillMove;

            if (EntityType == E_EntityType.BulletEntity)
            {
                ActionOnPathMoveEnd += OnActionPathMoveEnd;
            }

            ActionOnCheckTarget += OnActionOnCheckTarget;

            GlobalEvent.onPlayerShowHPUI.AddListener(OnPlayerShowHPUI);
        }

        private void OnPlayerShowHPUI(ulong arg0)
        {
            if (arg0 == EntityId)
            {
                return;
            }
            if (EntityType != E_EntityType.Partner)
            {
                return;
            }
            if (SummonHostID != arg0)
            {
                return;
            }
            ActionOnMainPlayerHurt?.Invoke();   // 被主角打的实体，显示头顶血条
        }

        protected override void Release()
        {
            //==========0===========
            ForceBackToDefaultState();
            //======================
            ViewFactory.ReleaseView(this); //时装，正常模型，变身都是 [View 对应 logic]都是一对一卸载的 
            //释放技能controller
            m_clientTag = E_ClentMainPlayerState.Normal;
            m_BattleMixTag = E_ClentMainPlayerState.Normal;
            ReleaseSkillAction();
            ReleaseBuffAction();
            ReleasePassiveAction();
            ReleaseSkillController();
            //==============1=================
            OffsetMoveAction = null;
            ActionOnCheckTarget = null;
            ActionOnMainPlayerHurt = null;
            //===============================

            ActionOnPlayThrowBullet = null;
            ActionOnPlayTrackingBullet = null;
            ActionOnPlayBezierBullet = null;
            ActionOnPlayEffectLineRenderer = null;
            ActionOnStopEffectLineRenderer = null;
            ActionOnRefreshHiddenSkillSlot = null;
            ActionOnRefreshSpectralChange = null;
            //--------------N----------
            base.Release();
            m_nextMyControl = null;
            m_data = null;
            IsCheckedEntity = false;
            //-------------------------
            hiddenSkillSlots.Clear();
            hasSpectralChangeEffect = false;

            isSimulateMove = false;


            GlobalEvent.onPlayerShowHPUI.RemoveListener(OnPlayerShowHPUI);

            Reset();
        }

        protected override void Reset()
        {
            base.Reset();
        }

        internal override void EnterFrame()
        {
            base.EnterFrame();
            skillDispatcher?.EnterFrame();
            FxNttListEnterFrame();
            ViewEnterFrameAction?.Invoke(Vector3.zero);
        }

        private void OnActionOnCheckTarget(bool check)
        {
            IsCheckedEntity = check;
        }

        public void CreateShadowView(Transform container, string path)
        {
            bool? hasShadowView = CheckHasControllerShow?.Invoke(EntityShowHidenTag.FollwShadow);
            if (hasShadowView == null || hasShadowView == false)
            {
                //Ctrl Group
                //ViewFactory.CreateViewModelAddressables(containerPath, "", path, this, container, "", false);
                ViewFactory.CreateViewAsync("Roles/Template/Character_Model_Shadow", this, container, null, path);
            }

            // 创建影子 是 同步， 所以此处 在创建完成后，发送对应的事件给影子,让它计数为 1
            ControlShowHide?.Invoke(EntityShowHidenTag.FollwShadow, true);
            // SGF.Debuger.LogError("[shadow]  create}");
        }

        public void HiddenShadowView()
        {
            ControlShowHide?.Invoke(EntityShowHidenTag.FollwShadow, false);
            // SGF.Debuger.LogError("[shadow]  hidden");
        }

        public void CreateMirrorShadowView(Transform container, string path)
        {
            bool? hasShadowView = CheckHasControllerShow?.Invoke(EntityShowHidenTag.MirrorShadow);
            if (hasShadowView == null || hasShadowView == false)
            {
                //Ctrl Group
                //ViewFactory.CreateViewModelAddressables("Roles/Template/Character_Model_Shadow", "Model/Roles/Character/JianS_F/cm/JianS_F_Model", path, this, container, "", false);
                ViewFactory.CreateViewAsync("Roles/Template/Character_Model_Shadow", this, container, null, path);
            }
            else
            {
                ControlShowHide?.Invoke(EntityShowHidenTag.MirrorShadow, true);
            }
        }

        public void HiddenMirrorShadowView()
        {
            ControlShowHide?.Invoke(EntityShowHidenTag.MirrorShadow, false);
        }

        /// <summary>
        /// 原子锁改变
        /// </summary>
        /// <param name="curState">当前的状态</param>
        /// <param name="newState">改变了的状态(新的状态)</param>
        protected virtual void OnBattleStateChange(E_BattleStateType curState, E_BattleStateType newState)
        {
            //检测再对应位是否有变化需求
            //电路设计：我要知道新数据，枚举中其他位为0，我检测位为1
            //关键位置我是1你是0就是0我要知你，其他位置我是0你是什么都排除
            //采用或运算
            E_BattleStateType theCurStateIn____BattlePoint = curState | E_BattleStateType.BattleState_BattleState;
            //其他位必然是0，关键位用对方的
            E_BattleStateType theNewerStateIn____BattlePoint = newState | E_BattleStateType.BattleState_BattleState;
            //其他位必然是0，关键位用对方的
            //E_BattleStateType theNewerStateIn____OutWarPoint = newState | E_BattleStateType.BattleState_OutWarState;

            //bool isLastStateInNormal = E_BattleStateType.BattleState_NormalState == (curState & E_BattleStateType.BattleState_NormalState);

            //Debug.Log($"怪物原子锁  curState={curState}=={Convert.ToUInt32(curState)}  --- newState={newState}=={Convert.ToUInt32(newState)}");
            // ^ 检测不同
            if (theCurStateIn____BattlePoint != theNewerStateIn____BattlePoint /*|| theCurStateIn____BattlePoint != theNewerStateIn____OutWarPoint*/)
            {
                //1 == 相同是0，不同是1 ； 说明不同 ；同时也说明现在是false;说明是normal;
                bool isServerBattle = E_BattleStateType.BattleState_BattleState == (newState & E_BattleStateType.BattleState_BattleState);

                bool isServerOutWar = E_BattleStateType.BattleState_OutWarState == (newState & E_BattleStateType.BattleState_OutWarState);

                //bool isServerNormal = E_BattleStateType.BattleState_NormalState == (newState & E_BattleStateType.BattleState_NormalState);
                if (Data != null && Data.isMainPlayer)
                {
                    //普攻状态,这里能配合声音播放，比如冰冻有声音实现则用不实现则不用，现在发现了吧，这里不是状态什么都往里面塞就有问题了吧！！！战斗内的状态和别的混合就这个问题
                    GlobalEvent.onMainPlayerChanageBattleState?.Invoke(isServerBattle ? E_PlayerStateForMusic.Battle : E_PlayerStateForMusic.Normal);
                }
                //客户端融合服务器，有一个是战斗就是战斗
                bool needSetBattleMixState = (M_clientTag == E_ClentMainPlayerState.Battle) || isServerBattle || isServerOutWar /*&& !isServerNormal*/;
                if (needSetBattleMixState)
                {
                    M_BattleMixTag = E_ClentMainPlayerState.Battle;
                    GlobalEvent.OnBattleStateChange.Invoke(true);
                }
                else
                {
                    M_BattleMixTag = E_ClentMainPlayerState.Normal;
                    GlobalEvent.OnBattleStateChange.Invoke(false);
                }
            }

            //--------------- 下面是判断原子锁位移然后解开后的如果之前有缓存路点就继续走
            {
                E_BattleStateType theCurStateIn____Move = curState | E_BattleStateType.BattleState_ForbidDisplacement;
                //其他位必然是0，关键位用对方的
                E_BattleStateType theNewerStateIn____Move = newState | E_BattleStateType.BattleState_ForbidDisplacement;
                if (theCurStateIn____Move != theNewerStateIn____Move)
                {
                    bool isInNewBattle = E_BattleStateType.BattleState_ForbidDisplacement == (newState & E_BattleStateType.BattleState_ForbidDisplacement);
                    if (!isInNewBattle)
                    {
                        // 禁止位移【解除了】
                        // 1.判断是否有路点要移动
                        if (Is_MainPlayer_FindingPath && FindPathPoints.Count > 0)
                        {
                            //SGF.Debuger.LogError("禁止移动  解开了继续移动");
                            ClientExecuteWayPoint(false, false);//服务器破坏更新循环
                        }
                        else
                        {
                            // 2.刷新下当前的状态
                            SetMovmentState();
                        }
                    }

                }
            }
            //BattleState_NormalState战斗和普通是互相对立的，检测一个即可
            //--------------- 如果原子锁不能选中了，那就关闭调主角对目标的选中状态
            bool isForbidSelect = E_BattleStateType.BattleState_ForbidSelect == (newState & E_BattleStateType.BattleState_ForbidSelect);
            if (Data != null && !Data.isMainPlayer && isForbidSelect)
            {
                if (BattleManager.Instance.CurAtkEntity != null && BattleManager.Instance.CurAtkEntity.EntityId == Data.M_EntityID)
                {
                    // BattleManager.Instance.SetCurAtkEntity(null);
                    // 如果怪物被原子 设置了禁止选中. 那 重新索敌. 重新索敌的时候, 内部会考虑这个 原子锁的状态。
                    BattleManager.Instance.ClearSerarchTargets();
                }
            }
        }

        internal override void RegisterAction()
        {
            base.RegisterAction();
            ActionOnHurtNodeMsg += OnHurtNodeMsg;
            ActionOnOffsetDataMsg += OnOffsetData;
        }

        //---------------------------------------------------------------------------------------------------


        /// <summary>
        /// 默认的技能播放阶段
        /// note: 
        ///     加个备注,此处单独保存了一个curSkillState,是为了在任何情况下,技能阶段都是 在1-2之间依次切换;
        ///     不用M_eSubState，是因为使用M_eSubState 的切换可能为 idle --> skill_common1 --> idle --> skill_common1。
        ///     这样并不是 skill_common1 、Skill_Common1 依次切;
        ///     不能被小曲给带偏了！！
        /// </summary>
        private E_ULayerSubState curSkillState = E_ULayerSubState.Default;

        private E_ULayerSubState GetNextSkillState()
        {
            if (curSkillState == E_ULayerSubState.Default)
            {
                return E_ULayerSubState.Skill_Common1;
            }
            else if (curSkillState == E_ULayerSubState.Skill_Common1)
            {
                return E_ULayerSubState.Skill_Common2;
            }
            else
            {
                return E_ULayerSubState.Skill_Common1;
            }
        }

        /// <summary>
        /// 优先级的 比较 其实可以 放在 SetSubState 中处理, 也可以放在 外层处理.
        /// 如果优先级比较 放在外层, 此处就做 特殊的阶段 变化(如skill 的阶段)和对于的动画层通知逻辑
        /// </summary>
        /// <param name="state"></param>
        /// <param name="animParam"></param>
        public void SetSubState(E_ULayerSubState state, I_AnimParam animParam, Action endStateAction = null)
        {
            //if (state == E_ULayerSubState.BattleIdle && m_eSubState == state)
            //{
            //    return;
            //}
            //if (EntityType == E_EntityType.Monster)
            //{
            //    Debug.LogError($"怪物状态 逻辑层  旧的={m_eSubState}，新的={state}");
            //}
            // if (EntityType == E_EntityType.Monster)
            // {
            //     SGF.Debuger.LogError($"xxxx--[动作] anim: {animParam.AnimationPath}, curState: {m_eSubState} ---> {state} "); ;
            //     SGF.Debuger.LogError($"[状态切换] xxxx--[动作] anim: {animParam.AnimationPath}, curState: {m_eSubState} ---> {state} "); ;
            // }



            m_eSubState = state;
            switch (m_eSubState)
            {
                // 技能单独判断
                case E_ULayerSubState.Skill_Common:
                    {
                        E_ULayerSubState subState = GetNextSkillState();
                        m_eSubState = subState;
                        curSkillState = subState;
                        AnimStatePlay?.Invoke(m_eSubState, animParam, endStateAction);
                    }
                    break;
                default:
                    {
                        AnimStatePlay?.Invoke(m_eSubState, animParam, endStateAction);
                    }
                    break;
            }
        }

        public bool Is___JoySitckStop
        {
            get { return m_IsJoyStickStop; }
            set
            {
                if (m_IsJoyStickStop != value)
                {
                    m_IsJoyStickStop = value;
                    if (m_IsJoyStickStop)
                    {
                        // 遥感停止就-清空移动的朝向
                        M_EntityMoveDir = UnityEngine.Vector3.zero;
                    }
                    //else
                    //{

                    //    // 主角如果有遥感输入，就暂停主角的自动寻路
                    //    ActionStopMoveDotween?.Invoke("Is___JoySitckStop");
                    //}
                    SetMovmentState();

                    // 更新 是否正在 移动的 状态
                    m_playerData.M_Is_Moveing = !value;
                    /// 2023/3/30
                    /// gl 说 技能的移动 条件替换的 动画,是 在 技能活跃阶段,切换的 另外一套动作,
                    ///     所以判断条件 不能根据玩家的 move/battleMove 状态, 而是 在技能状态 判断玩家是否移动
                    //SGF.Debuger.LogError($"[状态切换] 移动状态 发生变化, 此时 摇杆状态 Is___JoySitckStop: {m_IsJoyStickStop} , 移动状态 M_Is_Moveing: {m_playerData.M_Is_Moveing}");

                    // SGF.Debuger.LogError($"[动作] 刷新移动状态: M_Is_Moveing {m_playerData.M_Is_Moveing} , Is___JoySitckStop: {m_IsJoyStickStop} , M_eSubState: {M_eSubState}"); ;

                    // 如果 开始移动/ 结束移动, 就广播 出去当前 开始移动,由技能控制器 处理 是否需要切换 移动时的技能动画
                    m_playerData.TriggerChange("event_on_Move", m_IsJoyStickStop);
                }
            }
        }

        public void SetMovmentState()
        {
            // SGF.Debuger.LogError($"[状态切换] 设置移动状态, m_IsJoyStickStop: {m_IsJoyStickStop}");

            if (m_IsJoyStickStop && !Is_MainPlayer_FindingPath)
            {
                //是否是客户战斗状态
                //要回到Idle，走正常流程，里面的BattleIdle/Idle自己会处理
                I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.Idle);
                ChangeState((GameKeyCommand)E_ULayerSubState.Idle, animParam, false);
            }
            else
            {
                I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.SingleMoving);
                ChangeState((GameKeyCommand)E_ULayerSubState.SingleMoving, animParam, false);
            }
        }

        public void SetMovmentForceState()
        {
            if (m_IsJoyStickStop)
            {
                //是否是客户战斗状态
                //要回到Idle，走正常流程，里面的BattleIdle/Idle自己会处理
                I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.Idle);
                ForceSetState(E_ULayerSubState.Idle, animParam);
            }
            else
            {
                I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.SingleMoving);
                ForceSetState(E_ULayerSubState.SingleMoving, animParam);
            }
        }

        public bool Is___SkillJoyStick
        {
            get { return m_IsSkillJoyStick; }
            set
            {
                if (m_IsSkillJoyStick != value)
                {
                    m_IsSkillJoyStick = value;
                }
            }
        }

        //==================================================================

        ///// <summary>
        ///// Rect：先用unity的，或者场景遍历，或者碰撞（unity不准确，卡，逻辑要分离）
        ///// TODO：GameManager维护数据的八叉树玩家列表:就是玩家的关系你都知道了
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public bool TryHitBound(GameContext context)
        //{
        //    Rect rect = new Rect(0, 0, context.mapSize.x, context.mapSize.y);
        //    if (!rect.Contains(Position()))
        //    {
        //        Blast();
        //        return true;
        //    }
        //    return false;
        //}
        ////疾风
        //internal void Blast()
        //{
        //    if (OnBlast != null)
        //    {
        //        OnBlast();
        //    }
        //}
        //==================================================================

        protected void FxNttListEnterFrame()
        {
            if (fxList != null && fxList.Count != 0)
            {
                for (int i = 0; i < fxList.Count; i++)
                {
                    EntityRemoteStatic woleb = (EntityRemoteStatic)fxList[i];
                    woleb.EnterFrame();
                }
            }
        }

        //==================================================================

        #region 实体技能分发器


        public bool HasBuff(int buffID)
        {
            return skillController.GetSkillBuffWithBuffID(buffID) != null;
        }

        /// <summary>
        /// 每个player都有一个skillDispatcher，能够通过它获取所有技能信息
        /// </summary>
        public SkillDispatcher skillDispatcher;
        protected SkillController skillController;

        /// <summary>
        /// player初始化时，需要给玩家创建一个skillDispatcher,每个player都需要根据玩家的配置信息，
        /// 在skillControll中，动态的生成所有的技能数据。
        /// </summary>
        /// <param name="playerData"></param> 
        /// <param name="container"></param>
        protected void CreateSkillDispatcher(VitalSignData playerData, Transform container)
        {
            skillDispatcher = new SkillDispatcher();
            skillDispatcher.Create(playerData, container);
            skillController = skillDispatcher.SkillController;

            skillController.ActionOnSkillPlayAnim += OnActionOnSkillPlayAnim;
            skillController.ActionOnSkillAnimation += OnActionOnSkillAnimation;

            skillController.ActionOnSkillPlayFx += OnActionOnSkillPlayFx;
            skillController.ActionOnSkillStopFx += OnActionOnSkillStopFx;

            skillController.FuncOnPlayClientSkillEffect += PlayClientSkillEffect;
            skillController.FuncOnTryPlayServerSkillEffect += OnFuncTryPlayServerSkillEffect;

            skillController.ActionOnSkillStopEffect += OnActionStopEffect;


            skillController.ActionOnPlayCamera += OnActionPlayCamera;

            skillController.ActionOnSkillUpdateRotate += OnActionSkillUpdateRotate;

            skillController.ActionOnSyncServerRotate += OnActionSyncServerRotate;

            skillController.ActionOnBulletOffectY += OnActionOnBulletOffectY;
            skillController.ActionOnHidden += OnActionHidden;

            BindSkillAction();
            BindBuffAction();
            BindPassiveAction();
        }

        private void ReleaseSkillController()
        {
            if (skillDispatcher == null)
            {
                return;
            }

            skillDispatcher.Release();
            skillDispatcher = null;
        }

        #endregion


        /// <summary>
        /// 获取玩家的所有技能槽 dic
        /// </summary>
        public DictionaryEx<int, SkillContainer> GetSkillContainerDic()
        {
            return skillDispatcher.SkillUnitController.SkillContainerDic;
        }

        #region 【弃用的】声音方法

        //public AkEvent akEvent;
        //private static readonly System.Collections.Generic.List<AkEvent> akEvents = new System.Collections.Generic.List<AkEvent>();
        /* private void OnActionPlayAudio(I_AudioParam aPara)
         {
             *//* PlayEvent(aPara.soundName);*/
        /*akEvent.data.SetupReference("Battle_skill_bigsword_hitground", new Guid("8CB82A5D-D6D1-4602-BF36-27528FB84A09"));
        akEvent.data.Post(new GameObject());*//*
        AkSoundEngine.PostEvent(aPara.soundName, new GameObject());
    }
    private void OnPlayAKAudio()
    {
        //akEvent.name = "Battle_skill_bigsword_hitground";
        //PlayEvent(akEvent);
    }*/


        /* public void PlayEvent(string name)
         {
             //akEvent.data.ObjectReference.GetWwiseObjectForMigration()
             *//*akEvent.data.SetupReference(name,Guid.Empty);*//*
             //AkWwiseInitializationSettings.ActivePlatformSettings.
             //防重复Gob，哈希id
             var playingID = akEvent.data.Post(akEvent.gameObject, (uint)AkCallbackType.AK_EndOfEvent, CallbackHandler, akEvent);
             if (playingID != AkSoundEngine.AK_INVALID_PLAYING_ID)
             {
                 akEvents.Add(akEvent);

                 // In the case where objects are being placed in edit mode and then previewed, their positions won't yet be updated so we ensure they're updated here.
                 AkSoundEngine.SetObjectPosition(akEvent.gameObject, akEvent.transform);
             }
         }*/

        //public /*static*/ void PlayEvent(AkEvent akEvent)
        //{
        //    if (akEvents.Contains(akEvent))
        //    {
        //        return;
        //    }
        //    //防重复Gob，哈希id
        //    var playingID = akEvent.data.Post(akEvent.gameObject, (uint)AkCallbackType.AK_EndOfEvent, CallbackHandler, akEvent);
        //    if (playingID != AkSoundEngine.AK_INVALID_PLAYING_ID)
        //    {
        //        akEvents.Add(akEvent);

        //        // In the case where objects are being placed in edit mode and then previewed, their positions won't yet be updated so we ensure they're updated here.
        //        AkSoundEngine.SetObjectPosition(akEvent.gameObject, akEvent.transform);
        //    }
        //}

        //public static void StopEvent(AkEvent akEvent)
        //{
        //    if (akEvents.Remove(akEvent))
        //    {
        //        akEvent.data.Stop(akEvent.gameObject);
        //    }
        //}

        //public static bool IsEventPlaying(AkEvent akEvent)
        //{
        //    return akEvents.Contains(akEvent);
        //}

        //private static void CallbackHandler(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
        //{
        //    if (in_type == AkCallbackType.AK_EndOfEvent)
        //    {
        //        akEvents.Remove(in_cookie as AkEvent);

        //        //var refreshGUI = RefreshGUI;
        //        //if (refreshGUI != null)
        //        //{
        //        //    refreshGUI.Invoke();
        //        //}
        //    }
        //}
        #endregion

        #region 技能、BUFF、被动效果执行

        protected abstract void OnActionOnSkillPlayAnim(E_ULayerSubState e_ULayerSubState, I_AnimParam i_AnimParam);

        protected abstract void OnActionOnSkillAnimation(E_ULayerSubState e_ULayerSubState);

        protected abstract void OnActionOnSkillPlayFx(I_FxParam i_FxParam);

        protected abstract void OnActionOnSkillStopFx(I_FxParam fxParam);

        protected abstract void OnActionOnBulletOffectY(float offectY);

        #endregion

        /// <summary>
        /// 路点 位移结束 的 回调, 目前 主要是针对 子弹, 后续 可以所有的 实体类型都去增加
        /// </summary>
        /// <param name="isBreak"></param>
        public void OnActionPathMoveEnd(bool isBreak)
        {
#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                // 通知 本地服 这个实体 路点移动结束, 请求响应触发器
                GlobalEvent.OnClientReqLocalServerEvent.Invoke(ClientEventReq.TriggerEvent, new object[] { EntityId, TriggerEvent.PathMoveEnd, Position() });
            }
#endif
        }

        internal void SetFriend(VitalSignViewShowData node)
        {
            //用MM管控伙伴移动跟随
            //m_myControl = node;
            //m_myControl.MoveTo(m_pos);
        }

        public Action ActionOnMainPlayerHurt;   // 受到主角的伤害显示头顶血条
        private void OnHurtNodeMsg(HurtData hurtData, ulong builderId, ulong ownerId, int flutteringWordsID, E_StageType e_StageType)
        {
            // SGF.Debuger.Log($"{TagFlag} [xxxx] OnHurtNodeMsg2 TargetID={hurtData.TargetID},builderID={builderId},ownerId={ownerId},flutteringWordsID={flutteringWordsID}");

            BattleManager.Instance.OnHurtData(hurtData, null, builderId, ownerId, flutteringWordsID, e_StageType);

            //if (hurtData.Ishit && hurtData.Hurt > 0)
            {
                // 被动选中目标怪物
                if (Data != null)
                {
                    // 如果是主角自己，没有目标怪物时，那个怪物第一个攻击，那个就是目标怪物
                    if (Data.isMainPlayer)
                    {
                        if (BattleManager.Instance.CurAtkEntity == null && Data.M_EntityID != builderId && Data.M_EntityID != ownerId)
                        {
                            EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(ownerId);
                            if (entityCtrlBase != null && entityCtrlBase.M_Curr != null)
                            {
                                // BattleManager.Instance.SetCurAtkEntity(entityCtrlBase.M_Curr as NPCEntityBase);
                                // 怪物 攻击 的时候, 并且 当前没有索敌目标的时候, 设置攻击我的目标 就是 索敌的怪物
                                BattleManager.Instance.Switch2TargetEnemy(ownerId);
                                //怪物攻击主角
                                if (AppConfig.IsDev())
                                {
                                    GMBattleInfo.Instance?.SetTarget(entityCtrlBase.M_Curr as NPCEntityBase);
                                }
                            }
                        }
                    }
                    else if (Data.EntityType == E_EntityType.Monster || Data.EntityType == E_EntityType.Summon || Data.EntityType == E_EntityType.Partner)
                    {
                        // TODO: 曲
                        // 实体收到了主角/主角的召唤物/主角的伙伴的伤害时
                        bool isBuilderEntityByMainPlayerSummon = BattleManager.Instance.GetEntityIdIsMainPlayerSummon(builderId);
                        bool isOwnerEntityByMainPlayerSummon = BattleManager.Instance.GetEntityIdIsMainPlayerSummon(ownerId);
                        if (builderId == GameManager.Instance.mainPlayerId || isBuilderEntityByMainPlayerSummon || isOwnerEntityByMainPlayerSummon)
                        {
                            ActionOnMainPlayerHurt?.Invoke();   // 被主角打的实体，显示头顶血条
                            //主角攻击怪物
                            if (AppConfig.IsDev())
                            {
                                EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(ownerId);
                                if (entityCtrlBase != null && entityCtrlBase.M_Curr != null)
                                {
                                    GMBattleInfo.Instance?.SetTarget(entityCtrlBase.M_Curr as NPCEntityBase);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnCureNodeMsg(CureData cureData, ulong builderId, ulong ownerId)
        {
            BattleManager.Instance.OnHurtData(cureData, builderId, ownerId);
        }

        private void OnOffsetData(OffsetData offseData, float durningTime, MoveLabel moveLabel, MoveType moveType, string key)
        {
            UnityEngine.Vector3 v3 = UnityEngine.Vector3.zero;
            v3.x = offseData.Pos.X;
            v3.y = offseData.Pos.Y;
            v3.z = offseData.Pos.Z;
            // SGF.Debuger.LogError($"移动调试 [c-c] OnSkillMove :{v3.x},{v3.y},{v3.z} start");
            OffsetMoveAction?.Invoke(v3, durningTime, moveLabel, moveType, key, null);

            // GM
            if (EntityType == E_EntityType.Monster && Faction == 2)
            {
                AddSkillMoveGobCache(v3);
            }
        }

        #region 状态机

        /// <summary>
        /// 矩阵
        /// </summary>
        /// <param name="gameKeyCommand">纵向命令处理集合</param>
        public override void ChangeState(GameKeyCommand gameKeyCommand /*bool canSetSameState = false强制刷新*/, I_AnimParam i_AnimParam, bool isCanEqual)
        {
            base.ChangeState(gameKeyCommand, i_AnimParam, isCanEqual);
            SetStateByCondition(CommandChange(gameKeyCommand), m_BattleMixTag, i_AnimParam, isCanEqual);
        }

        /// <summary>
        /// 条件转换
        /// 里面是原子锁其实
        /// </summary>
        /// <param name="gameKeyCommand"></param>
        /// <returns></returns>
        public GameKeyCommand CommandChange(GameKeyCommand gameKeyCommand) //服务器强制拉回状态
        {
            switch (gameKeyCommand)
            {
                //case GameKeyCommand.None:
                //    break;
                case GameKeyCommand.MoveCommand:
                case GameKeyCommand.BattleMoveCommand:
                    //来源于轮盘的特殊转换:目前ui可设置轮盘无效不发送命令即可
                    //左侧就控制移动和朝向，右侧技能时控制朝向+左侧是否可以辅助移动
                    //如不移动
                    if (IsActionSkillCantMove() || IsDebuffStateCantMove())
                    {
                        if (GetSkillCanSetFace() == false)
                        {
                            //固定方向模式
                            gameKeyCommand = GameKeyCommand.None;
                        }
                        else
                        {
                            //原地 + 方向选择模式
                            gameKeyCommand = GameKeyCommand.FaceTo;
                        }
                    }

                    //移动 + （方向选择模式 或 不可移动）TODO：右侧技能如果是有轮盘那么移动
                    //此时此刻不附带面向或者技能轮盘更正面向
                    {
                    }

                    break;
                case GameKeyCommand.FaceTo: //技能按钮的方向对齐
                    break;
            }

            return gameKeyCommand;
        }


        /// <summary>
        /// 设置状态：响应式，消息来源于，服务器，客户端响应，客户端监听
        /// </summary>
        /// <param name="gameKeyCommand"></param>
        /// <param name="e_ClentMainPlayerState"></param>
        /// <param name="isCanEqual">相同状态是否可以切换</param>
        /// =============
        public void SetStateByCondition(GameKeyCommand gameKeyCommand, E_ClentMainPlayerState e_ClentMainPlayerState, I_AnimParam i_AnimParam, bool isCanEqual)
        {
            E_ULayerSubState newState = M_eSubState;

            //处理融合形动作，而不处理状态
            switch (gameKeyCommand)
            {
                case GameKeyCommand.None:
                    break;
                //组内========================================================================================================================================
                case GameKeyCommand.StandBy:
                    // 如果收到了 播放待机动画的 指令,且当前阶段 为 idle 阶段, 那么 就可以直接切换到 待机播放动画的 阶段
                    if (M_eSubState == E_ULayerSubState.Idle)
                    {
                        newState = E_ULayerSubState.StandBy;
                    }
                    else if ((int)M_eSubState < (int)gameKeyCommand)
                    {
                        // 否则的话,就需要按照 状态的优先级 顺序比较来 决定是否切换待机状态
                        newState = (E_ULayerSubState)(int)gameKeyCommand;
                    }
                    break;
                //【目标】：目标是idle，但是当前是移动，那一定是idle
                case GameKeyCommand.Idle:
                case GameKeyCommand.BattleIdle:
                    if (M_eSubState == E_ULayerSubState.Idle || M_eSubState == E_ULayerSubState.BattleIdle //【当前】组内随意切
                        || M_eSubState == E_ULayerSubState.SingleMoving || M_eSubState == E_ULayerSubState.BattleMoving
                        || M_eSubState == E_ULayerSubState.Hurt /* hurt回归到idle情况；状态机回归到Default，但逻辑状态机卡在hurt*/
                        || M_eSubState == E_ULayerSubState.WeaponRetractionIdle || M_eSubState == E_ULayerSubState.WeaponRetractionMoving// 如果 旧状态 是 武器 的收刀 状态 或者 旧状态 是 武器的 移动收刀状态, 此时收到了 新的Idle指令, 就根据 移动指令规则, 去判断是否切到 battleIdle 状态
                        ) /* 技能后摇阶段可以被打断 */
                    {
                        //E_ULayerSubState tempState = E_ULayerSubState.Idle + (int)e_ClentMainPlayerState;
                        //if (M_eSubState == E_ULayerSubState.WeaponRetractionIdle || M_eSubState == E_ULayerSubState.WeaponRetractionMoving)
                        //{

                        // //   SGF.Debuger.LogError($"[状态] 收到 [{gameKeyCommand}] 指令 , 当前状态 M_eSubState ---> [{M_eSubState}] , 准备切到 : [{tempState}] ");
                        //}
                        //hurt指向回归idle，后进行左右切换
                        newState = E_ULayerSubState.Idle + (int)e_ClentMainPlayerState;
                    }
                    else if ((int)M_eSubState < (int)gameKeyCommand) //其他要遵循准则
                    {
                        newState = (E_ULayerSubState)(int)gameKeyCommand;
                    }
                    break;
                case GameKeyCommand.WanderCommand:
                    if (M_eSubState == E_ULayerSubState.Idle || M_eSubState == E_ULayerSubState.BattleIdle //【当前】组内随意切
                        || M_eSubState == E_ULayerSubState.SingleMoving || M_eSubState == E_ULayerSubState.BattleMoving
                        )
                    {
                        newState = E_ULayerSubState.WanderMoving;
                    }
                    else if ((int)M_eSubState < (int)gameKeyCommand) //其他要遵循准则
                    {
                        newState = (E_ULayerSubState)(int)gameKeyCommand;
                    }
                    break;
                case GameKeyCommand.MoveCommand:
                case GameKeyCommand.BattleMoveCommand:
                    //if (EntityType == E_EntityType.Monster)
                    //{
                    //    if (M_eSubState == E_ULayerSubState.Skill || M_eSubState == E_ULayerSubState.Skill_Common1 || M_eSubState == E_ULayerSubState.Skill_Common2|| M_eSubState == E_ULayerSubState.Skill_Common)
                    //    {
                    //        SGF.Debuger.LogError($"路点移动的时候，发现上次居然是技能状态  怪物id={EntityId},上次的状态={M_eSubState}");
                    //        UnityEngine.Debug.Break();
                    //    }
                    //}
                    if (M_eSubState == E_ULayerSubState.Idle || M_eSubState == E_ULayerSubState.BattleIdle //【当前】组内随意切
                        || M_eSubState == E_ULayerSubState.SingleMoving ||
                            M_eSubState == E_ULayerSubState.BattleMoving
                        // 如果 是 idle/move 的 收刀动作 过程中 收到了 移动指令, 那就遵循 移动 跟随 战斗状态 去切换 对应的 移动或者 战斗移动状态
                        || (M_eSubState == E_ULayerSubState.WeaponRetractionIdle)
                        || (M_eSubState == E_ULayerSubState.WeaponRetractionMoving)
                        )
                    {
                        // 
                        // 如果当前状态 是 收刀的移动,这个时候 收到了移动状态,需要区分 当前 是否是 战斗状态, 如果是战斗状态, 那其实 就换位 战斗移动
                        // 如果不是战斗状态, 就 需要等 收到 移动动作 结束后, 再去 切换 对应的移动状态

                        //if (M_eSubState == E_ULayerSubState.WeaponRetractionMoving)
                        //{
                        //    if (e_ClentMainPlayerState == E_ClentMainPlayerState.Battle)
                        //    {

                        //        E_ULayerSubState tempState = E_ULayerSubState.SingleMoving + (int)e_ClentMainPlayerState;
                        //        newState = E_ULayerSubState.SingleMoving + (int)e_ClentMainPlayerState;
                        //     //   SGF.Debuger.LogError($"[状态] 收到 [{gameKeyCommand}] 指令 , 当前状态 M_eSubState ---> [{M_eSubState}] , 准备切到 : [{tempState}] ");
                        //    }
                        //    else
                        //    {
                        //        return;
                        //    }
                        //}


                        newState = E_ULayerSubState.SingleMoving + (int)e_ClentMainPlayerState;
                    }
                    else if ((int)M_eSubState < (int)gameKeyCommand) //其他要遵循准则
                    {
                        newState = (E_ULayerSubState)(int)gameKeyCommand;
                    }
                    break;
                //组内=========================================================================================================================================
                case GameKeyCommand.FaceTo:
                    //只是融合状态，并不会修改
                    break;
                //Hurt设定比较特殊：他属于临时状态，他不属于[走/停]
                case GameKeyCommand.Hurt:
                    if ((int)M_eSubState < (int)gameKeyCommand)
                    {
                        //命令控制力
                        newState = (E_ULayerSubState)(int)gameKeyCommand;
                        //Set Single Move
                    }
                    break;
                case GameKeyCommand.SkillCancelCommand:
                    //局部控制状态
                    //技能取消时根据是否是战斗状态，
                    if (SkillCtrlByClient())
                    {
                        newState = GetForceDefaultState();
                    }
                    break;
                default:
                    //如果当前在技能阶段是可以直接到技能【客户端】后摇的
                    //尾巴可以允许特殊进入到技能结尾
                    //[规划内的特殊情况]
                    //if ((M_eSubState >= E_ULayerSubState.Stage_SkillPro &&
                    //    M_eSubState <= E_ULayerSubState.Stage_Builded
                    //    && gameKeyCommand == GameKeyCommand.Stage_Builded_Tail)
                    //    || (M_eSubState == E_ULayerSubState.EndKnockDown))  //击倒起身可以被低优先级的覆盖
                    //{
                    //    newState = (E_ULayerSubState)(int)gameKeyCommand;
                    //}
                    if (M_eSubState == E_ULayerSubState.EndKnockDown)  //击倒起身可以被低优先级的覆盖
                    {
                        newState = (E_ULayerSubState)(int)gameKeyCommand;
                    }
                    else
                    {
                        //arg1.skillInfo.skillId + cur
                        //读取动画名字：
                        if ((int)M_eSubState < (int)gameKeyCommand)
                        {
                            //命令控制力
                            newState = (E_ULayerSubState)(int)gameKeyCommand;
                            //Set Single Move
                        }
                    }
                    break;
            }
            // 不相同的状态 -- 切换
            bool isChange = M_eSubState != newState;
            // if (EntityId == GameManager.Instance.mainPlayerId)
            // {
            //     SGF.Debuger.LogError($"[状态切换] SetStateByCondition 当前状态 M_eSubState={M_eSubState}, gameKeyCommand={newState}, isChange={isChange}");
            // }
            // 相同状态 -- 【可以相同参数:true】 切换
            if (M_eSubState == (E_ULayerSubState)(int)gameKeyCommand && isCanEqual)
            {
                newState = (E_ULayerSubState)(int)gameKeyCommand;
                isChange = true;
            }
            // if (EntityType == E_EntityType.Monster)
            //if (EntityId != GameManager.Instance.mainPlayerId)
            // {
            // SGF.Debuger.LogError($" xxxx 状态切换 SetStateByCondition  M_eSubState={M_eSubState},gameKeyCommand={newState},isCanEqual={isCanEqual},isChange={isChange}");

            //   SGF.Debuger.LogError($"状态切换 SetStateByCondition---11111 M_eSubState={M_eSubState},gameKeyCommand={newState},isCanEqual={isCanEqual},isChange={isChange}");
            // }
            if (isChange)
            {
                SetSubState(newState, i_AnimParam);
            }
            // if (EntityId == GameManager.Instance.mainPlayerId)
            // {
            //     SGF.Debuger.LogError($"状态切换 SetStateByCondition---22222 M_eSubState={M_eSubState},gameKeyCommand={newState},isCanEqual={isCanEqual},isChange={isChange}");
            // }
        }

        /// <summary> 客户端状态倒计时 </summary>
        public float M_clientStateCountdown = 0f;

        private E_ClentMainPlayerState m_clientTag = E_ClentMainPlayerState.Normal; //默认别人状态

        /// <summary> [客户端]主角自己维护的状态【也是服务器融合】 </summary>
        ///融合解释：因为逻辑是有一个是battle就battle，全不是Battla就解开
        ///主角符合，第三方没有自己的战斗状态全部听服务器的
        ///主动摇杆，和服务器变化都会推送给处理
        public E_ClentMainPlayerState M_clientTag
        {
            get { return m_clientTag; }
            set
            {
                if (value != m_clientTag)
                {
                    m_clientTag = value;
                    GlobalEvent.OnClientBattleStateChange.Invoke(m_clientTag == E_ClentMainPlayerState.Battle);
                    if ((m_clientTag == E_ClentMainPlayerState.Battle) || Data.IsBattleStateServer)
                    {
                        M_BattleMixTag = E_ClentMainPlayerState.Battle;
                    }
                    else
                    {
                        M_BattleMixTag = E_ClentMainPlayerState.Normal;
                    }
                }
            }
        }

        private E_ClentMainPlayerState m_BattleMixTag;

        public E_ClentMainPlayerState M_BattleMixTag
        {
            get { return m_BattleMixTag; }
            set
            {
                if (value != m_BattleMixTag)
                {
                    I_AnimParam animParam = null;
                    var originTag = m_BattleMixTag;
                    //SGF.Debuger.LogError($"属性同步 设置客户端战斗状态 m_BattleMixTag={m_BattleMixTag}val={value}");
                    m_BattleMixTag = value;

                    /// 2023/3/16
                    /// gl需求: 新增 战斗状态 切换到 非战斗状态 时的 中间 收刀状态.

                    // 如果 是 由战斗状态切到 非战斗状态, 判断 当前的 状态 是否是 battleIdle 或者 battleMove, 如果是
                    // 上述两个状态, 那就 切换 为 收刀的过渡状态
                    if (originTag == E_ClentMainPlayerState.Battle && value != E_ClentMainPlayerState.Battle)
                    {
                        if (M_eSubState == E_ULayerSubState.BattleIdle)
                        {
                            animParam = GetAnimParamByState(E_ULayerSubState.WeaponRetractionIdle);
                            //SGF.Debuger.LogError($"[状态] 战斗状态----> 非战斗状态 , 当前状态 BattleIdle ---> [WeaponRetractionIdle] ,animParam: {animParam.AnimationPath} ");
                            ChangeState(GameKeyCommand.WeaponRetractionIdle, animParam, false); //用当前状态刷新以下，纵向指针不变，横向指针刷新（战斗状态）

                        }
                        else if (M_eSubState == E_ULayerSubState.BattleMoving)
                        {
                            animParam = GetAnimParamByState(E_ULayerSubState.WeaponRetractionMoving);
                            //SGF.Debuger.LogError($"[状态] 战斗状态----> 非战斗状态 , 当前状态 BattleMoving ---> [WeaponRetractionMoving] ,animParam: {animParam.AnimationPath} ");
                            ChangeState(GameKeyCommand.WeaponRetractionMoving, animParam, false); //用当前状态刷新以下，纵向指针不变，横向指针刷新（战斗状态）
                        }
                        else
                        {
                            animParam = GetAnimParamByState(M_eSubState);
                            //SGF.Debuger.LogError($"[状态] 战斗状态----> 非战斗状态 , 当前状态 M_eSubState ---> [{M_eSubState}] ,animParam: {animParam.AnimationPath} ");
                            ChangeState((GameKeyCommand)M_eSubState, animParam, false); //用当前状态刷新以下，纵向指针不变，横向指针刷新（战斗状态）
                        }
                    }
                    else
                    {
                        animParam = GetAnimParamByState(M_eSubState);
                        ChangeState((GameKeyCommand)M_eSubState, animParam, false); //用当前状态刷新以下，纵向指针不变，横向指针刷新（战斗状态）
                    }

                    //SoundManager.Instance.
                    /*SetMovmentState();*/
                }
            }
        }

        /// <summary>
        /// 恢复对应的idle或battleIdle
        /// </summary>
        /// <returns></returns>
        public E_ULayerSubState GetForceDefaultState()
        {
            if (M_BattleMixTag == E_ClentMainPlayerState.Battle)
            {
                return E_ULayerSubState.BattleIdle;
            }
            else
            {
                return E_ULayerSubState.Idle;
            }
        }

        /// <summary>
        /// 受伤表现层回调需要接入这个
        /// dl的技能需要接这个
        /// </summary>
        /// <param name="isForce">isForce = true是时强制设置，应该是比较重要的状态机回归：如技能【打断】</param>
        public void ForceSetState(E_ULayerSubState forceToAny, I_AnimParam i_AnimParam)
        {
            // 【主角】要多判断 自己维护的状态
            //if (Data.isMainPlayer)
            //{
            //   //跟主角没关系，你打别人别人也会变成battleIdle通过服务器告诉你，另外别人更简单别人不存储客户端状态
            //}
            /* 静止，移动|战斗，平和
                 设置我都允许，4个里面的切换
                 并且我也允许自然切换，主动推送*/

            if (Data.IsDead)
            {
                forceToAny = E_ULayerSubState.Deading;
            }

            if (forceToAny == E_ULayerSubState.Idle || forceToAny == E_ULayerSubState.SingleMoving)
            {
                E_ULayerSubState forceState = forceToAny + (int)m_BattleMixTag;
                I_AnimParam animParam = GetAnimParamByState(forceState);
                SetSubState(forceState, animParam);
            }
            else
            {
                //大的会走推送表现层没问题（且会看情况发送技能），且需要直接切换逻辑层不通过判断
                if (i_AnimParam == null)
                {
                    i_AnimParam = GetAnimParamByState(forceToAny);
                }
                SetSubState(forceToAny, i_AnimParam);
            }
        }

        protected void ForceBackToDefaultState()
        {
            I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.Default);
            SetSubState(E_ULayerSubState.Default, animParam);
        }

        /// <summary>
        /// 技能取消，只能在：技能阶段-客户端可控制的状态
        /// </summary>
        /// <returns></returns>
        private bool SkillCtrlByClient()
        {
            return false;
            // return M_eSubState == E_ULayerSubState.Stage_SkillPro || M_eSubState == E_ULayerSubState.Stage_SkillPro;
        }

        /// <summary>
        /// 当前技能状态，就设计成不能移动的
        /// </summary>
        /// <returns></returns>
        private bool IsActionSkillCantMove()
        {
            return false;
            //return M_eSubState >= E_ULayerSubState.Stage_Building_CannotMove && M_eSubState < E_ULayerSubState.Stage_Building_CannotMove;
        }

        /// <summary>
        /// 当前被动技能，不能移动的
        /// </summary>
        /// <returns></returns>
        private bool IsDebuffStateCantMove()
        {
            return false;
            //return M_eSubState >= E_ULayerSubState.IsSonscious && M_eSubState < E_ULayerSubState.Deading;
        }

        private bool GetSkillCanSetFace()
        {
            return true; //默认朝向世界，角色，其他玩家等固定方向|不能操作轮盘+技能也没有轮盘
        }

        private bool lock_syncPreSkillPos = false;

        public E_ModelShowState GetModelShowState()
        {
            return m_data.e_ModelShowState;
        }

        #endregion

        #region RPC协议

        #region 【技能】RPC协议

        public void OnUseSkill(ProtoMsg.SkillUseRet skillUseRet)
        {
            // SGF.Debuger.Log($"VitalSignsBase OnUseSkill t :  ");

            lock_syncPreSkillPos = true;
            //SGF.Debuger.LogError($"VitalSignsBase OnUseSkill lock_syncPreSkillPos {lock_syncPreSkillPos}");
            // 技能回调的时候，先执行角度的改变
            //ServerSetRotation(skillUseRet.Rot, false, false);
            //ClientSetRotation(skillUseRet.Rot, false);
            // SGF.Debuger.LogError($"收到服务器的使用技能回复:  {skillUseRet}");
            PreHandleSkillPostion(skillUseRet, () =>
            {
                lock_syncPreSkillPos = false;
                // SGF.Debuger.Log($"VitalSignsBase OnUseSkill t :  lock_syncPreSkillPos {lock_syncPreSkillPos}");
                // ServerSetRotation(skillUseRet.Rot, true, false);
                skillController.OnServerUseSkill(skillUseRet);
            });

            // if (EntityType == E_EntityType.Monster)
            // {
            //     SGF.Debuger.Log("清空路点 使用技能--------------------");

            //     ProtoMsg.Vector3 skillPos = skillUseRet.Pos;
            //     UnityEngine.Vector3 pos = UnityEngine.Vector3.zero;
            //     pos.x = skillPos.X;
            //     pos.y = CurrentPos.y;
            //     pos.z = skillPos.Z;
            //     AddSkillUserGobCache(pos);
            // }
        }

        public void OnEndSkill(ProtoMsg.SkillEndRet skillEndRet)
        {
            skillController.OnServerEndSkill(skillEndRet);
        }

        #endregion

        #region 【被动技能】RPC协议

        /// <summary>
        /// 被动技能创建
        /// </summary>
        /// <param name="passiveSkillUseRet"></param>
        public void OnPassiveSkillUseRet(ProtoMsg.PassiveSkillUseRet passiveSkillUseRet)
        {
            skillController.OnPassiveSkillUseRet(passiveSkillUseRet);
        }

        /// <summary>
        /// 被动技能莫得了
        /// 
        /// </summary>
        /// <param name="passiveSkillEndRet"></param>
        public void OnPassiveSkillEndRet(ProtoMsg.PassiveSkillEndRet passiveSkillEndRet)
        {
            skillController.OnPassiveSkillEndRet(passiveSkillEndRet);
        }

        #endregion

        public void OnRunStageRet(ProtoMsg.RunStageRet runStageRet)
        {
            // 技能相关的逻辑 放 技能控制器中处理
            skillController.OnServerRunStage(runStageRet);

            // 非技能实体方面的逻辑(类似于buff ui的 实体)，单独处理
            switch (runStageRet.RuntimeType)
            {
                case RuntimeEnumType.DefaultRuntime:
                case RuntimeEnumType.PassiveSkill:
                case RuntimeEnumType.ActiveSkill:
                case RuntimeEnumType.Bullet:
                    break;
                case RuntimeEnumType.Buff:
                    {
                        //RunBlackKeyBuff(EnumEnityListKey.Env_Buff, runStageRet);
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnRunStageForceEndRet(ProtoMsg.RunStageForceEndRet runStageForceEndRet)
        {
            skillController.OnServerRunStageForceEndRet(runStageForceEndRet);
        }

        public void OnSkillRuntimeSyncRet(ProtoMsg.RuntimeSyncRet runtimeSyncRet)
        {
            skillController.OnSkillRuntimeSync(runtimeSyncRet);
        }

        public void OnPreSkillUseInputRet(ProtoMsg.PreSkillUseInputRet preSkillUseInputRet)
        {
            skillController.OnPreSkillUseInputRet(preSkillUseInputRet);
        }

        #endregion

        public void OnActionSkillUpdateRotate(int rota, bool isUpdateServer, bool isUpdateClient, bool isSyncView, float maxLerpTime)
        {
            //!Is___JoySitckStop || 
            if (Is_MainPlayer_FindingPath)
            {
                SGF.Debuger.LogWarning("在主角路点位移的时候, 收到了 技能的 朝向的同步");
                // Debug.Break();
                return;
            }
            //maxLerpTime = 1;
            //SGF.Debuger.LogError($"[Skill_Rotate] rota: {rota}, isUpdateServer: {isUpdateServer}, isUpdateClient: {isUpdateClient}, isSyncView: {isSyncView}, maxLerpTime: {maxLerpTime}");
            bool hasSyncView = false;
            bool hasLerp = false;
            if (isUpdateClient)
            {
                // 客户端会同步view
                hasSyncView = true;
                hasLerp = true;
                ClientSetRotation(rota, true, maxLerpTime, true);
            }

            if (isUpdateServer)
            {
                // 如果 客户端和 服务器 同时 同步了 view, 那么 服务器 lerp = false 会打断 表现层的lerp . 所以 此处 判断一下  客户端之前是否同步了 view hasSyncView
                bool syncView = hasSyncView ? true : isSyncView;
                bool needLerp = hasLerp ? true : maxLerpTime > 0;
                ServerSetRotation(rota, needLerp, syncView, maxLerpTime, true);
            }
        }

        /// <summary>
        /// 同步 一次 服务器朝向 的 view 表现
        /// </summary>
        public void OnActionSyncServerRotate()
        {
            if (Data.Is___ForbidDir)
            {
                return;
            }

            SyncClientRotaWithServerRota();
        }

        #region 技能黑板:串行、并行 数据和特效处理接口

        /// <summary>
        /// 播放客户端效果线的效果
        /// note:
        ///     1.每个效果的播放,都需要判定自己这个效果是否能够执行,返回的result是这个效果是否能够执行
        ///     2.对于客户端效果线来说,执行的时候,不确定服务器的数据是否已经来到.
        ///       所以对于客户端来说,需要检测服务器的数据在黑板中是否存在,如果存在，采用的应该是服务器数据
        /// </summary>
        /// <param name="runtimeID">运行时runtimeID, 此处的 runtimeID 是子弹、技能、被动、buff 这种运行时id,不是阶段的 runtimeID</param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns>执行效果之后的结果,根据结果可以知道这个效果是否执行成功</returns>
        private bool PlayClientSkillEffect(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart, ulong builderID, ulong ownerEntityID, int stageRecoverTime, bool isRecover)
        {
            bool result = true;
            //TODO DL
            //直接播放 客户端的 特效 和动作

            /// <summary>
            /// 效果逻辑 实现过程中的思考:
            ///     效果执行的逻辑 包含Next[True,False],由此,有两种实现方式
            ///     1.每个效果执行时,输出自己这个效果的执行结果,根据True/False决定不同的分支;
            ///     2.每个效果执行时,检查这个效果是否可以执行,根据是否执行返回True/Fase。
            ///        Next的两个效果均参与上述方式(本地的效果采用这种方式实现)
            /// 
            /// 优劣:
            ///     方案一:
            ///     优点:
            ///         1.思路清晰,符合逻辑,效果执行之后，根据当前效果执行的结果,就可以知道接下来
            ///           需要执行那个 Next效果;
            ///     问题:
            ///         1.符合直觉，但只适合立即执行的效果.对于等待输入这种 可能延时触发的效果,无法
            ///           在效果执行的瞬间,就知道结果.
            ///     方案二:
            ///     优点:
            ///         1.在每个效果执行的时候,检查自己是否可以执行这个效果,反向依赖的方式,能够处理
            ///           延时效果的逻辑;
            ///     缺点:
            ///         1.需要将所有分支的效果都注册执行逻辑;
            ///         2.执行效果的返回 不再时效果执行分支的 True/False,而是 效果执行成功的True/False，
            ///           有理解成本
            ///         3.需要对每个类型的效果执行之前,都做一次 CheckCanPlay 的条件检查
            ///     
            /// </summary>

            //result = CheckCanPlay(skillEntity, skillStage, effectParam);

            SkillStage skillStage = (SkillStage)effectParam.ExtraData;
            int effectStageRuntime = skillStage.GetEffectStageTime(effectParam.EffectID);

            // 阶段恢复时间 - 效果在阶段上的时间 =  效果 恢复的时间(效果执行了多久)
            int effectRecoverTime = stageRecoverTime - effectStageRuntime;
            effectRecoverTime = effectRecoverTime < 0 ? 0 : effectRecoverTime;

            // 旋转效果 是个瞬间执行的效果, 所以 客户端在做 旋转效果的恢复的时候, 客户端的角度已经通过 AOI 属性同步过来了,
            // 此时，就不需要 单独恢复
            bool needRecover = isRecover && effectRecoverTime > 0;

            //SGF.Debuger.LogError($"{TagFlag} stage_[{skillStage.StageIDStr}] PlayClientSkillEffect : key = {effectParam.OutputKey} , effectID: {effectParam.EffectID} ,SkillEffectType {effectParam.SkillEffectType},stageRecoverTime {stageRecoverTime}, stageRuntime {effectStageRuntime}, effectRecoverTime : {effectRecoverTime}");

            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.OffsetNodeMsg:
                    break;
                case E_SkillEffect.AddMagic:
                    break;
                case E_SkillEffect.DecMagic:
                    break;
                case E_SkillEffect.FixCDAbs:
                    break;
                case E_SkillEffect.FixCDPercent:
                    break;
                case E_SkillEffect.SetSkillCD:
                    break;
                case E_SkillEffect.SetSkillCDPercent:
                    break;
                case E_SkillEffect.ChangeToward:
                    {
                        result = HandleClientEffectChangeToward(effectParam, blackBoard, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.ChangeToAbsoluteToward:
                    {
                        result = HandleEffectChangeToAbsoluteToward(effectParam, blackBoard, effectRecoverTime, E_BlackBoardTag.Client);
                    }
                    break;

                case E_SkillEffect.Empty:
                    break;
                case E_SkillEffect.CollisionBox:
                    {
                        result = HandleClientEffectCollisionBox(effectParam, blackBoard, builderID, ownerEntityID, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.TarGroup:
                    break;
                case E_SkillEffect.WaitInput:
                    break;
                case E_SkillEffect.IsInput:
                    break;
                case E_SkillEffect.IsStage:
                    break;
                case E_SkillEffect.Treat:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleClientEffectTreat(effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.Damage:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleClientEffectDamage(effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.MoveWithPos:
                    {
                        result = HandleClientEffectOffset(effectParam, blackBoard, false, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.MoveWithRot:
                    {
                        result = HandleClientEffectOffset(effectParam, blackBoard, true, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.ChangeMana:
                    break;
                case E_SkillEffect.ChangeCD:
                    break;
                case E_SkillEffect.BreakCurRuntime:
                    break;
                case E_SkillEffect.ChangeProp:
                    break;
                case E_SkillEffect.AddBuff:
                    break;
                case E_SkillEffect.RemoveBuff:
                    break;
                case E_SkillEffect.CreateBullet:
                    {
#if UNITY_EDITOR
                        if (EditorModeTest.EditorMode.IsEditorMode)
                        {
                            result = HandleClientCreateBullet(effectParam, blackBoard, effectRecoverTime);
                        }
#endif
                    }
                    break;
                case E_SkillEffect.DestoryBullet:
                    break;
                case E_SkillEffect.SetBulletTargetPos:
                    {
#if UNITY_EDITOR
                        if (EditorModeTest.EditorMode.IsEditorMode)
                        {
                            result = HandleClientSetBulletTargetPos(runtimeID, effectParam, blackBoard, effectRecoverTime);
                        }
#endif
                    }
                    break;
                case E_SkillEffect.BreakCurRuntimeInBullet:
                    {
#if UNITY_EDITOR
                        if (EditorModeTest.EditorMode.IsEditorMode)
                        {
                            result = HandleClientBreakCurRuntimeInBullet(runtimeID, effectParam, blackBoard);
                        }
#endif
                    }
                    break;
                case E_SkillEffect.PlayEffectAtPoint:
                    result = HandleClientPlayEffectAtPoint(effectParam, blackBoard, effectRecoverTime);
                    break;
                case E_SkillEffect.PlayEffectAtTarget:
                    result = HandleClientPlayEffectAtTarget(effectParam, blackBoard, effectRecoverTime);
                    break;
                case E_SkillEffect.PlayEffectBetweenPoints:
                    {
                        result = HandleClientPlayEffectBetweenTargets(effectParam, blackBoard, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.Stealth:
                    result = HandleClientStealth(effectParam, blackBoard, effectRecoverTime);
                    break;
                case E_SkillEffect.DamageSecond:
                    if (needRecover)
                    {
                        return true;
                    }
                    result = HandleClientDamageSecond(effectParam, blackBoard);
                    break;
                case E_SkillEffect.SelectHitFromKey:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleClientSelectHitFromKey(effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.IsNotEmpty:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleClientIsNotEmpty(effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.CheckIntKey:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleClientIsIntKey(effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.CheckToward:
                case E_SkillEffect.CheckPassive:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = EffectResultUtils.HandleClientRegEffect(effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.ThrowBullet:
                    {
#if UNITY_EDITOR
                        if (EditorModeTest.EditorMode.IsEditorMode)
                        {
                            // 本地服 模拟抛射一个子弹
                            result = HandleClientThrowBullet(effectParam, blackBoard);
                        }
#endif

                    }
                    break;
                case E_SkillEffect.ClientSummonAnim:
                    {
                        result = HandleClientSummonAnim(runtimeID, effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.ClientSummonEffect:
                    {
                        result = HandleClientSummonEffect(runtimeID, effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.ClientSummonTurnTo:
                    {
                        // result = HandleServerSummonTurnTo(runtimeID, effectParam, blackBoard);
                    }
                    break;
                case E_SkillEffect.ClientSummonRemove:
                    {
                        result = HandlSummonRemove(runtimeID, effectParam, blackBoard);
                    }
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 注册服务器效果线
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="customBlackBoardNode">触发的服务器 黑板数据, 注意, 也有可能是 inputKey 的黑板数据, 所以 服务器效果线处理的时候,不应该直接 将数据强转</param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="effectRecoverTime">[效果它自己] 的 恢复时间</param>
        private bool OnFuncTryPlayServerSkillEffect(ulong runtimeID, I_EffectParam effectParam, CustomBlackBoardNode customBlackBoardNode, BaseBlackBoard baseBlackBoard, int effectRecoverTime)
        {
            // note:
            // 需要跟服务器确认, 阶段的效果数据，是否会通过技能传给 客户端???
            // 如果 可能通过技能传入的话，客户端就需要 在技能中 执行效果逻辑(目前是直接在技能中增加了一套技能播放服务器效果的逻辑)
            // 但是，走的都是统一的 player.OnActionPlayServerSkillEffect 接口, 所以 技能播放的时候，传过来的 skillStage 为null
            // 
            // 跟服务器确认之后，如果服务器还是可能有的话，那就只能在 技能收到效果数据的时候，客户端自己根据某种方式，找到对应的阶段
            // 来处理服务器效果了

            // 如果 效果的恢复时间 > 0 , 那就 需要 执行 这个效果的恢复逻辑
            bool needRecover = effectRecoverTime > 0;

            bool result = true;

            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.OffsetNodeMsg:
                    break;
                case E_SkillEffect.AddMagic:
                    break;
                case E_SkillEffect.DecMagic:
                    break;
                case E_SkillEffect.FixCDAbs:
                    break;
                case E_SkillEffect.FixCDPercent:
                    break;
                case E_SkillEffect.SetSkillCD:
                    break;
                case E_SkillEffect.SetSkillCDPercent:
                    break;
                case E_SkillEffect.ChangeToward:
                    {
                        // 旋转效果 是个瞬间执行的效果, 所以 客户端在做 旋转效果的恢复的时候, 客户端的角度已经通过 AOI 属性同步过来了,
                        // 此时，就不需要 单独恢复
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleServerEffectChangeToward(effectParam, customBlackBoardNode, baseBlackBoard, effectRecoverTime);
                    }
                    break;

                case E_SkillEffect.ChangeToAbsoluteToward:
                    {
                        result = HandleEffectChangeToAbsoluteToward(effectParam, baseBlackBoard, effectRecoverTime, E_BlackBoardTag.Server);
                    }
                    break;
                case E_SkillEffect.Empty:
                    break;
                case E_SkillEffect.CollisionBox:
                    {
                        result = HandleServerEffectCollisionBox(effectParam, baseBlackBoard, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.TarGroup:
                    break;
                case E_SkillEffect.WaitInput:
                    break;
                case E_SkillEffect.IsInput:
                    break;
                case E_SkillEffect.IsStage:
                    break;
                case E_SkillEffect.Treat:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleServerEffectTreat(effectParam, baseBlackBoard);
                    }
                    break;
                case E_SkillEffect.Damage:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleServerEffectDamage(effectParam, baseBlackBoard);
                    }
                    break;
                case E_SkillEffect.MoveWithPos:
                    {
                        result = HandleServerEffectOffset(effectParam, baseBlackBoard, false, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.MoveWithRot:
                    {
                        result = HandleServerEffectOffset(effectParam, baseBlackBoard, true, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.ChangeMana:
                    break;
                case E_SkillEffect.ChangeCD:
                    break;
                case E_SkillEffect.BreakCurRuntime:
                    break;
                case E_SkillEffect.ChangeProp:
                    break;
                case E_SkillEffect.AddBuff:
                    break;
                case E_SkillEffect.RemoveBuff:
                    break;
                case E_SkillEffect.CreateBullet:
                    break;
                case E_SkillEffect.DestoryBullet:
                    break;
                case E_SkillEffect.PlayEffectAtPoint:
                    result = HandleServerEffectAtPoint(effectParam, baseBlackBoard, effectRecoverTime);
                    break;
                case E_SkillEffect.PlayEffectAtTarget:
                    result = HandleServerPlayEffectAtTarget(effectParam, baseBlackBoard, effectRecoverTime);
                    break;
                case E_SkillEffect.PlayEffectBetweenPoints:
                    result = HandleServerPlayEffectBetweenTargets(effectParam, baseBlackBoard, effectRecoverTime);
                    break;
                case E_SkillEffect.Stealth:
                    result = HandleServerStealth(effectParam, baseBlackBoard, effectRecoverTime);
                    break;
                case E_SkillEffect.DamageSecond:
                    if (needRecover)
                    {
                        return true;
                    }
                    result = HandleServerDamageSecond(effectParam, baseBlackBoard, effectRecoverTime);
                    break;
                case E_SkillEffect.SelectHitFromKey:
                    // SGF.Debuger.LogError($"{TagFlag} [x-x] [server] SelectHitFromKey  ");

                    result = true;
                    break;
                case E_SkillEffect.IsNotEmpty:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleServerIsNotEmpty(effectParam, baseBlackBoard, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.CheckIntKey:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = HandleServerIsIntKey(effectParam, baseBlackBoard, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.CheckToward:
                case E_SkillEffect.CheckPassive:
                    {
                        if (needRecover)
                        {
                            return true;
                        }
                        result = EffectResultUtils.HandleServerRegEffect(effectParam, baseBlackBoard, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.ThrowBullet:
                    {
                        /// 2023/17/18
                        /// gl需求: 子弹抛物线
                        /// 约定如下:
                        /// 1.客户端 在收到服务器 ThrowBullet 效果的 outputKey 后 触发这个抛物线效果;
                        /// 2.这个效果的 outputKey 是 抛物线的起点,TargetPoint 是 目标点.
                        /// 3.目前 约定 抛物线的 初始点 和 目标点在统一 水平面, 所以 他们的y 需要校准 相同.
                        /// 4.第三人 同步 看到的子弹, 可以根据 初始点 + 目标点 + effectRecoverTime恢复时间 恢复具体的抛物线.

                        result = HandleServerThrowBullet(effectParam, baseBlackBoard, effectRecoverTime);

                    }
                    break;
                case E_SkillEffect.LaunchBullet:
                    {
                        result = HandleServerPlayLaunchBullet(effectParam, baseBlackBoard);
                    }
                    break;
                case E_SkillEffect.BezierBullet:
                    {
                        result = HandleServerBezierBullet(effectParam, baseBlackBoard, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.PlayEffectLineRenderer:
                    {
                        result = HandleServerPlayEffectLineRender(effectParam, baseBlackBoard, effectRecoverTime);
                    }
                    break;

                case E_SkillEffect.EffectTypeRegister:
                    {
                        result = HandleServerEffectTypeRegister(effectParam, baseBlackBoard, effectRecoverTime);
                    }
                    break;
                case E_SkillEffect.ClientSummonTurnTo:
                    {
                        result = HandleServerSummonTurnTo(runtimeID, effectParam, baseBlackBoard);
                    }
                    break;
                case E_SkillEffect.ClientSummonRemove:
                    {
                        result = HandlSummonRemove(runtimeID, effectParam, baseBlackBoard);
                    }
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 停止 效果 的接口,一般是在 效果中 配置了 跟随阶段关闭 而停止 效果
        /// note:
        ///     目前 是 只实现了 隐身效果的 跟随阶段打断, 后续 看 需求 一个个加
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="builderID"></param>
        /// <param name="ownerID"></param>
        private void OnActionStopEffect(I_EffectParam effectParam, BaseBlackBoard baseBlackBoard, ulong builderID, ulong ownerID)
        {
            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.Stealth:
                    StopStealth(effectParam, baseBlackBoard);
                    break;
                default:
                    break;
            }
        }

        #region 【碰撞盒】效果
        private bool HandleClientEffectCollisionBox(I_EffectParam effectParam, BaseBlackBoard blackBoard, ulong builderID, ulong ownerEntityID, int effectRecoverTime)
        {
            //先检查能不能播放效果,所以就有一下几步

            //1.检查服务器黑板中是否有本效果输出outputKey的数据

            // 如果效果有输出key,那就查找 黑板中是否有完整的 服务器输出的黑板数据
            // 比如一个效果, 输出 hurtTarget 10个人， 治疗 cureTarget 2个队友,那黑板就会有 hurtTarget和cureTarget 两个
            // 少了任何一个,客户端都用本地计算(服务器黑板数据不会减,理论上 key不会删掉)

            string outPutKey = effectParam.OutputKey;

            bool find = EffectUtils.CheckIsBlackBoardHasKey(blackBoard, outPutKey, E_BlackBoardTag.Server);

            var collisionBox = effectParam.BaseEffect as EffectTypeCollisionBox;
            // 如果能找到服务器数据,直接播放服务器的预警圈
            if (find)
            {

                // 如果不播 预警圈, 那就不播
                if (!collisionBox.IsForewarn)
                {
                    return true;
                }

                // 客户端执行时，先判断下服务器线有没有执行
                bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);
                if (clientExecuteResult)
                {
                    return true;
                }

                // 标识 客户端 提前执行了 服务器线的数据 
                EffectResultUtils.UpdateEffectExecuteResultState(blackBoard, effectParam, E_BlackBoardTag.Client, E_EffectExecuteResultState.ClientRunServer);

                // 如果 builder 不是 自己的时候, 也不播
                if (effectParam.Builder != EntityId)
                {
                    return true;
                }
                //SGF.Debuger.LogError($"[Rotate] 预警圈  客户端 播放预警圈 1111 key={effectParam.OutputKey}");

                // 服务器 数据在客户端线提前执行,那不管 服务器的数据能不能播,客户端线都是返回true,标识 客户端线执行成功
                bool result = PlayEffectWarning(effectParam, blackBoard, effectRecoverTime);

                //SGF.Debuger.LogError($"[测试] 预警圈 {EntityType} {EntityId}  客户端 播放 预警圈 result: {result} , 更新结果: true ");
                return true;
            }
            else
            {
#if UNITY_EDITOR
                if (EditorModeTest.EditorMode.IsEditorMode)
                {

                    // 如果找不到服务器数据,就需要客户端自己取计算碰撞盒 数据,客户端线提前 执行.
                    RepeatedField<SkillTarData> skillTarDatas = new();


                    ShapeSerialize shape = collisionBox.Shape;   // 碰撞盒形状
                    int MaxTar = collisionBox.MaxTar;   // 结算最大人数

                    // 生成释放技能的坐标点 数组
                    RepeatedField<ProVec3> posArr = EffectUtils.GetCollisionPosArray(effectParam, blackBoard);
                    if (posArr.Count == 0)
                    {
                        return false;
                    }
                    // 生成释放技能的朝向 数组
                    RepeatedField<int> angleArr = EffectUtils.GetCollisionTowardArr(effectParam, blackBoard);
                    if (angleArr.Count == 0)
                    {
                        return false;
                    }

                    // 如果找不到服务器数据,那客户端自己计算,并将这个数据存入 客户端 黑板中
                    // 计算命中了 哪些
                    SelectType CollisionTarget = collisionBox.CollisionTarget;  // 目标类型
                    bool IsSettleHit = collisionBox.IsSettleHit;    // 是否结算命中
                                                                    // 先留着，测试好了，确定不用，再删除
                    Vector3 skillDir = EffectUtils.GetSkillDirection(collisionBox.CenterPosArray, EulerAngles.y);

                    for (int i = 0; i < posArr.Count; i++)
                    {
                        ProVec3 centerPos = posArr[i];
                        for (int j = 0; j < angleArr.Count; j++)
                        {
                            int angle = angleArr[j];
                            //得到的碰撞合命中者
                            RepeatedField<SkillTarData> targes = EffectUtils.GetCollisionHitTargets(EntityId, Faction, centerPos, skillDir, angle, shape, CollisionTarget, IsSettleHit, MaxTar);

                            skillTarDatas = skillTarDatas.MergeProtoArray<SkillTarData>(targes,
                            (SkillTarData target1, SkillTarData target2) =>
                            {
                                return target1.TarID == target2.TarID;
                            });
                        }
                    }
                    SkillTarsMsg skillTarsMsg = new();
                    skillTarsMsg.SkillTars.Append(skillTarDatas);
                    CustomBlackBoardNode customBlackBoardNode = BaseBlackBoard.InitClientCustomBlackBoardNode(outPutKey, skillTarsMsg, builderID, ownerEntityID);

                    // 写入客户端黑板中
                    blackBoard.Set(outPutKey, customBlackBoardNode, E_BlackBoardTag.Client);
                    {
                        // 显示 预警圈
                        if (collisionBox.IsForewarn && posArr.Count > 0)
                        {
                            //SGF.Debuger.Log($"预警圈  客户端 播放 22222 key={effectParam.OutputKey}");
                            //for (int i = 0; i < posArr.Count; i++)
                            //{
                            //    ProVec3 centerPos = posArr[i];
                            //    for (int j = 0; j < angleArr.Count; j++)
                            //    {
                            //        //SGF.Debuger.Log($"预警圈  客户端 播放 22222 key={effectParam.OutputKey},坐标={centerPos},角度={angleArr[j]}");
                            //    }
                            //}
                            EffectUtils.HandleEffectWarning(posArr, angleArr, effectParam, effectRecoverTime);
                            //PlayEffectWarning(effectParam, blackBoard, effectRecoverTime);
                        }
                    }

                    return true;
                }
#endif
                return false;
            }


            //if (false)
            //{
            //    // 如果找不到服务器数据,就需要客户端自己取计算碰撞盒 数据,客户端线提前 执行.
            //    RepeatedField<SkillTarData> skillTarDatas = new();


            //    ShapeSerialize shape = collisionBox.Shape;   // 碰撞盒形状
            //    int MaxTar = collisionBox.MaxTar;   // 结算最大人数

            //    // 生成释放技能的坐标点 数组
            //    RepeatedField<ProVec3> posArr = EffectUtils.GetCollisionPosArray(effectParam, blackBoard);
            //    if (posArr.Count == 0)
            //    {
            //        return false;
            //    }
            //    // 生成释放技能的朝向 数组
            //    RepeatedField<int> angleArr = EffectUtils.GetCollisionTowardArr(effectParam, blackBoard);
            //    if (angleArr.Count == 0)
            //    {
            //        return false;
            //    }

            //    // 如果找不到服务器数据,那客户端自己计算,并将这个数据存入 客户端 黑板中
            //    // 计算命中了 哪些
            //    SelectType CollisionTarget = collisionBox.CollisionTarget;  // 目标类型
            //    bool IsSettleHit = collisionBox.IsSettleHit;    // 是否结算命中
            //                                                    // 先留着，测试好了，确定不用，再删除
            //    Vector3 skillDir = EffectUtils.GetSkillDirection(collisionBox.CenterPosArray, EulerAngles.y);

            //    for (int i = 0; i < posArr.Count; i++)
            //    {
            //        ProVec3 centerPos = posArr[i];
            //        for (int j = 0; j < angleArr.Count; j++)
            //        {
            //            int angle = angleArr[j];
            //            //得到的碰撞合命中者
            //            RepeatedField<SkillTarData> targes = EffectUtils.GetCollisionHitTargets(EntityId, Faction, centerPos, skillDir, angle, shape, CollisionTarget, IsSettleHit, MaxTar);
            //            skillTarDatas = skillTarDatas.MergeProtoArray<SkillTarData>(targes,
            //            (SkillTarData target1, SkillTarData target2) =>
            //            {
            //                return target1.TarID == target2.TarID;
            //            });
            //        }
            //    }
            //    SkillTarsMsg skillTarsMsg = new();
            //    skillTarsMsg.SkillTars.Append(skillTarDatas);
            //    CustomBlackBoardNode customBlackBoardNode = BaseBlackBoard.InitClientCustomBlackBoardNode(outPutKey, skillTarsMsg, builderID, ownerEntityID);

            //    // 写入客户端黑板中
            //    blackBoard.Set(outPutKey, customBlackBoardNode, E_BlackBoardTag.Client);
            //    {
            //        // 显示 预警圈
            //        if (collisionBox.IsForewarn && posArr.Count > 0)
            //        {
            //            //SGF.Debuger.Log($"预警圈  客户端 播放 22222 key={effectParam.OutputKey}");
            //            //for (int i = 0; i < posArr.Count; i++)
            //            //{
            //            //    ProVec3 centerPos = posArr[i];
            //            //    for (int j = 0; j < angleArr.Count; j++)
            //            //    {
            //            //        //SGF.Debuger.Log($"预警圈  客户端 播放 22222 key={effectParam.OutputKey},坐标={centerPos},角度={angleArr[j]}");
            //            //    }
            //            //}
            //            EffectUtils.HandleEffectWarning(posArr, angleArr, effectParam, effectRecoverTime);
            //            //PlayEffectWarning(effectParam, blackBoard, effectRecoverTime);
            //        }
            //    }

            //    return true;
            //}

        }

        /// <summary>
        /// 播放预警圈
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="effectRecoverTime">效果的恢复时间</param>
        /// <returns></returns>
        private bool PlayEffectWarning(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            // 生成释放技能的坐标点 数组
            RepeatedField<ProVec3> posArr = EffectUtils.GetCollisionPosArray(effectParam, blackBoard);
            if (posArr.Count == 0)
            {
                return false;
            }
            // 生成释放技能的朝向 数组
            RepeatedField<int> angleArr = EffectUtils.GetCollisionTowardArr(effectParam, blackBoard);
            if (angleArr.Count == 0)
            {
                return false;
            }
            //SGF.Debuger.LogError($"[Rotate] 预警圈  服务器 播放预警圈 1111 key={effectParam.OutputKey}");

            //for (int i = 0; i < posArr.Count; i++)
            //{
            //    ProVec3 centerPos = posArr[i];
            //    for (int j = 0; j < angleArr.Count; j++)
            //    {
            //        Debug.Log($"预警圈  key={effectParam.OutputKey},坐标={centerPos},角度={angleArr[j]},开始时间={effectRecoverTime}");
            //    }
            //}

            /// 2023/2/15
            /// TODO: 曲
            ///     note：
            ///         目前 播放 预警圈 会传入 一个效果的 恢复时间, 因为 目前预警圈 并不属于 普通特效(只是在最上层的根节点套了一个粒子组件的壳而已),
            ///         因此 在效果执行 恢复的时候, 并不能根据 通用的效果 恢复 接口, 确定 预警圈 是否需要播放和恢复(它自己有配置),
            ///         所以 此处 传如 effectRecoverTime 作为 开始时间给 预警圈, 预警圈 需要根据 这个时间, 定点 播放 或者 超时 不播放.
            PlayEffectWarning(posArr, angleArr, effectParam, effectRecoverTime);
            return true;
        }

        private bool PlayEffectWarning(RepeatedField<ProVec3> posArr, RepeatedField<int> angleArr, I_EffectParam effectParam, int startTime = 0)
        {
            if (posArr.Count == 0)
            {
                return false;
            }

            if (angleArr.Count == 0)
            {
                return false;
            }

            EffectUtils.HandleEffectWarning(posArr, angleArr, effectParam, startTime);
            return true;
        }

        /// <summary>
        /// TODO: 曲
        ///     碰撞盒的 预警圈 不能只依赖 服务器线 来播放, 内网网络延迟不大的时候,问题不大.
        ///     但是 一旦网络延迟变大，问题就会很大.
        ///     好点的做法,是 客户端线 提前播放 预警圈, 等到 服务器线 碰撞盒 数据来了之后, 再去
        ///     更正.
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="effectRecoverTime">效果的 恢复时间</param>
        /// <returns></returns>
        private bool HandleServerEffectCollisionBox(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            // 如果不播 预警圈, 那就不播
            var collisionBox = effectParam.BaseEffect as EffectTypeCollisionBox;
            if (!collisionBox.IsForewarn)
            {
                return true;
            }

            // 如果 builder 不是 自己的时候, 也不播
            // 高磊删的，如果有问题再说
            //if (effectParam.Builder != EntityId)
            //{
            //    return true;
            //}
            //SGF.Debuger.Log($"预警圈  服务器 播放  key={effectParam.OutputKey}");
            // 预警圈 直接由 服务器
            //return PlayEffectWarning(effectParam, blackBoard, effectRecoverTime);

            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);
            // 客户端线 有没有执行成功, 如果执行失败,说明 客户端线 没找到数据, 并且自己也 没计算出 数据, 此时就纯粹 依赖服务器线来执行
            if (!clientExecuteResult)
            {
                bool result = PlayEffectWarning(effectParam, blackBoard, effectRecoverTime);
                //SGF.Debuger.LogError($"[测试] {EntityType} {EntityId}  服务器 播放 预警圈 result: {result} , 更新结果: {result} ");

                //SGF.Debuger.LogWarning($"预警圈  服务器 播放 1111111 key={effectParam.OutputKey}");
                return result;
            }

            bool clientHasRunServer = EffectResultUtils.CheckEffectHasExecuteState(blackBoard, effectParam, E_BlackBoardTag.Client, E_EffectExecuteResultState.ClientRunServer);
            // 如果客户端线 执行 成功了, 然后需要检查 客户端线 有没有 提前 执行 服务器线 的数据, 如果 提前执行了 服务器线的数据, 那服务器线就不需要干啥.
            if (clientHasRunServer)
            {
                //SGF.Debuger.LogError($"[测试] {EntityType} {EntityId}  服务器 播放 预警圈 客户端已经执行, 直接返回 true ");

                return true;
            }

            string outPutKey = effectParam.OutputKey;

            // 如果 客户端没有提前执行服务器,而是简单的客户端 自己提前计算的碰撞盒数据, 那就需要比对 客户端计算的数据 和服务器计算的数据的差别,
            // 然后 将缺失 的 数据 对应的碰撞盒  补齐 ，同时，根据 客户端/ 服务器线执行的时间, 追赶上去

            object clientBlackBoardNode = blackBoard.Get(outPutKey, E_BlackBoardTag.Client);

            if (clientBlackBoardNode == null)
            {
                // 如果此时查不到 客户端线 存的数据，理论上不存在才对, 测试 加个 error 日志, 要定位这个问题
                Debug.Assert(clientBlackBoardNode == null, "HandleServerEffectCollisionBox client none data!!!");
                return true;
            }
            CustomBlackBoardNode customClientBlackBoardNode = (CustomBlackBoardNode)clientBlackBoardNode;

            // 服务器数据上面已经做了处理,所以此处一定有效
            object serverBlackBoardNode = blackBoard.Get(outPutKey, E_BlackBoardTag.Server);
            CustomBlackBoardNode customServerBlackBoardNode = (CustomBlackBoardNode)serverBlackBoardNode;

            SkillTarsMsg clientSkillTarsMsg = (SkillTarsMsg)customClientBlackBoardNode.Value;
            SkillTarsMsg serverSkillTarsMsg = (SkillTarsMsg)customServerBlackBoardNode.Value;

            // 服务器需要补齐的
            RepeatedField<SkillTarData> serverNeedSkillTarDatas = ProtoUtils.Subtract(serverSkillTarsMsg.SkillTars, clientSkillTarsMsg.SkillTars,
                (SkillTarData a, SkillTarData b) =>
                {
                    return a.TarID == b.TarID;
                });

            // 客户端关闭的
            //RepeatedField<SkillTarData> serverNeedSkillTarDatas2 = ProtoUtils.Subtract(clientSkillTarsMsg.SkillTars, serverSkillTarsMsg.SkillTars,
            //    (SkillTarData a, SkillTarData b) =>
            //    {
            //        return a.TarID == b.TarID;
            //    });
            //SGF.Debuger.LogError($"[测试] {EntityType} {EntityId}  服务器 播放 预警圈  找到服务器数据, 返回 result: true");

            if (serverNeedSkillTarDatas.Count > 0)
            {
                RepeatedField<ProVec3> posTargetPos = EffectUtils.GetSkillTargetsCenterPos(effectParam, collisionBox.CenterPosArray, serverNeedSkillTarDatas);

                RepeatedField<int> angleArr = EffectUtils.GetTargetsTowardArr(serverNeedSkillTarDatas, collisionBox.TowardArray.TowardOffset, collisionBox.TowardArray.TransTargetIsHit);

                if (posTargetPos.Count == 0 || angleArr.Count == 0)
                {
                    return true;
                }
                long useTime = 0;
                long clientRuntTime = EffectResultUtils.GetEffectExecuteTime(blackBoard, effectParam, E_BlackBoardTag.Client);
                if (clientRuntTime > 0)
                {
                    useTime = SGF.Time.TimeUtils.ClientNowStampMilli - clientRuntTime;

                    //TODO DL
                    //计算 客户端线 和 服务器的时间间隔, 然后将 时间间隔放入特效中 

                    //SGF.Debuger.LogError($"[xx--xx] , {useTime}");
                }
                //SGF.Debuger.LogWarning($"预警圈  服务器 播放 222222 key={effectParam.OutputKey}");
                //for (int i = 0; i < posTargetPos.Count; i++)
                //{
                //    ProVec3 centerPos = posTargetPos[i];
                //    for (int j = 0; j < angleArr.Count; j++)
                //    {
                //        SGF.Debuger.LogWarning($"预警圈  服务器 播放 222222 key={effectParam.OutputKey},坐标={centerPos},角度={angleArr[j]}");
                //    }
                //}
                return PlayEffectWarning(posTargetPos, angleArr, effectParam, (int)useTime);
            }

            return true;
        }

        #endregion

        #region  命中目标的筛选
        private bool HandleClientSelectHitFromKey(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            var selectHitFromKey = effectParam.BaseEffect as EffectTypeSelectHitFromKey;
            if (selectHitFromKey == null)
            {
                return false;
            }
            string inputKey = effectParam.InputKeys[0];
            string outPutKey = effectParam.OutputKey;

            bool find = EffectUtils.CheckIsBlackBoardHasKey(blackBoard, outPutKey, E_BlackBoardTag.Server);

            // find了 ，服务器数据存在，不用写
            if (find)
            {
                // SGF.Debuger.LogError($"{TagFlag} [x-x] [client] find outputkey {outPutKey}");

                return true;
            }

            // 先直接取黑板数据, 服务器取不到,会取客户端黑板数据(先服务器数据/ 后客户端数据).
            CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(inputKey);

            if (customBlackBoardNode != null)
            {
                blackBoard.Set(outPutKey, customBlackBoardNode, E_BlackBoardTag.Client);
                // SGF.Debuger.LogError($"{TagFlag} [x-x] [client] find inputKey {inputKey}");

                return true;
            }
            // SGF.Debuger.LogError($"{TagFlag} [x-x] [client] find inputKey {inputKey} error");

            return false;
        }
        #endregion

        #region 【治疗】效果
        private bool HandleEffectTreat(I_EffectParam effectParam, BaseBlackBoard blackBoard, E_BlackBoardTag tag)
        {
            bool executeResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, tag);
            if (executeResult)
            {
                return true;
            }

            string outPutKey = effectParam.OutputKey;
            CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(outPutKey);
            if (customBlackBoardNode == null)
            {
                return false;
            }

            CureNodeMsg cureNodeMsg = (CureNodeMsg)customBlackBoardNode.Value;
            AvatarDataCell avatarData = avatarDataCell;
            if (Data != null && effectParam.Builder != Data.M_EntityID)
            {
                AvatarDataCell model = GameManager.Instance.GetEntityAvatarById(effectParam.Builder);
                if (model != null)
                {
                    avatarData = model;
                }
            }
            if (avatarData == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} HandleEffectTreat PartnerEntityId=>{EntityId},Builder={effectParam.Builder},avatarData=null");
            }
            EffectUtils.HandleEffectCure(cureNodeMsg, effectParam, avatarData.EffectsPath);

            return true;
        }

        private bool HandleClientEffectTreat(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            bool result = HandleEffectTreat(effectParam, blackBoard, E_BlackBoardTag.Server);
            // 客户端执行时，先判断下服务器线有没有执行

            if (result)
            {
                //   SGF.Debuger.Log($"黑板阶段同步 【客户端】 治疗效果 effectid={effectParam.EffectID}");
                EffectResultUtils.UpdateEffectExecuteResultState(blackBoard, effectParam, E_BlackBoardTag.Client, E_EffectExecuteResultState.ClientRunServer);
            }
            return result;
        }

        private bool HandleServerEffectTreat(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            bool result = HandleEffectTreat(effectParam, blackBoard, E_BlackBoardTag.Client);
            // 客户端执行时，先判断下服务器线有没有执行
            if (result)
            {
                //   SGF.Debuger.Log($"黑板阶段同步 【服务器】 治疗效果 effectid={effectParam.EffectID} , result: {result}");
            }
            return result;
        }
        #endregion

        #region 【伤害】效果
        private bool HandleEffectDamage(I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isPlayEffect)
        {
            string outPutKey = effectParam.OutputKey;

            CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(outPutKey);
            if (customBlackBoardNode != null)
            {
                HurtNodeMsg hurtMsg = (HurtNodeMsg)customBlackBoardNode.Value;
                if (avatarDataCell != null)
                {
                    var cfgID = blackBoard.Get(BaseBlackBoard.KEY_CFG_ID, E_BlackBoardTag.Client);


                    EffectUtils.HandleEffectDamage(hurtMsg, effectParam, avatarDataCell.EffectsPath, isPlayEffect, cfgID == null ? 0 : (int)cfgID);
                }
                return true;
            }

            return false;
        }

        private bool HandleEffectDamageTargetArr(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            var damage = effectParam.BaseEffect as EffectTypeDamage;

            string effectTargetKey = damage.EffectTargetArray.Result;
            RepeatedField<SkillTarData> skillTarDatas = EffectUtils.GetKeySkillTargetArray(effectTargetKey, effectParam, blackBoard);
            if (skillTarDatas != null && skillTarDatas.Count > 0)
            {
                if (avatarDataCell != null)
                {
                    EffectUtils.HandleEffectDamageTar(skillTarDatas, effectParam, avatarDataCell.EffectsPath);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HandleClientEffectDamage(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            // 客户端执行时，先判断下服务器线有没有执行
            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);
            if (clientExecuteResult)
            {
                //   LogUtils.LogError(LogEnum.SkillEffect, $"客户端播放 伤害 效果key: {effectParam.OutputKey} , 服务器已经 播放, return");
                return true;
            }

            // 1.先找->服务器效果线【输出】【key】 有没有->黑板数据
            // 2.后找->输入【key】中的命中列表 只播放：被击特效
            bool result = HandleEffectDamage(effectParam, blackBoard, true);

            //SGF.Debuger.LogError($"客户端播放 伤害 效果key: {effectParam.OutputKey} , 播放伤害 和 受击 结果: result: {result}");
            if (result)
            {
                EffectResultUtils.UpdateEffectExecuteResultState(blackBoard, effectParam, E_BlackBoardTag.Client, E_EffectExecuteResultState.ClientRunServer);
                return true;
            }
            // 客户端 只有在 完全没有伤害目标的时候,才会 返回 false
            result = HandleEffectDamageTargetArr(effectParam, blackBoard);

            //SGF.Debuger.LogError($"客户端播放 伤害 效果key: {effectParam.OutputKey} , 只播受击: result: {result}");

            return result;
        }

        private bool HandleServerEffectDamage(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            // 检查 客户端有没有执行 服务器的数据,如果已经执行了, 那服务器就不需要执行后续逻辑
            bool clientHasRunServer = EffectResultUtils.CheckEffectHasExecuteState(blackBoard, effectParam, E_BlackBoardTag.Client, E_EffectExecuteResultState.ClientRunServer);
            if (clientHasRunServer)
            {
                //   LogUtils.LogError(LogEnum.SkillEffect, $"服务器播放 伤害 效果key: {effectParam.OutputKey} , 客户端已经 播放 服务器效果, return.");
                return true;
            }

            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);

            // 判断 客户端 线的执行 结果, 如果 为 false, 说明 客户端 没执行 或者 客户端 执行的 skillTargeArr 为 空
            // 此时 就可以 完全的执行 这个 服务器效果线
            if (!clientExecuteResult)
            {
                //   LogUtils.LogError(LogEnum.SkillEffect, $"服务器播放 伤害 效果key: {effectParam.OutputKey} , 客户端 客户端 执行结果 false, 处理伤害和 动作");
                return HandleEffectDamage(effectParam, blackBoard, true);
            }
            //   LogUtils.LogError(LogEnum.SkillEffect, $"服务器播放 伤害 效果key: {effectParam.OutputKey} , 只播伤害");

            // 只跑飘字
            return HandleEffectDamage(effectParam, blackBoard, false);
        }

        #endregion

        #region 【坐标、朝向位移】效果
        private bool HandleEffectOffset(I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isRotMove, int effectRecoverTime)
        {
            // 1.先找->服务器效果线【输出】【key】 有没有->黑板数据
            string outPutKey = "";
            float durningTime = 0;
            MoveLabel moveLabel = MoveLabel.Normal;
            MoveType moveType = MoveType.Rush;
            if (isRotMove)
            {
                var moveWithRot = effectParam.BaseEffect as EffectTypeMoveWithRot;
                outPutKey = moveWithRot.OutputKey.Result;
                durningTime = moveWithRot.DurningTime / 1000f; // 配置表是毫秒，，这里要转成秒
                moveLabel = moveWithRot.MoveLabel;
                moveType = moveWithRot.MoveType;
            }
            else
            {
                EffectTypeMoveWithPos moveWithPos = effectParam.BaseEffect as EffectTypeMoveWithPos;
                outPutKey = moveWithPos.OutputKey.Result;
                durningTime = moveWithPos.DurningTime / 1000f; // 配置表是毫秒，，这里要转成秒
                moveLabel = moveWithPos.MoveLabel;
                moveType = moveWithPos.MoveType;
            }
            CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(outPutKey);
            if (customBlackBoardNode != null)
            {
                OffsetNodeMsg offsetNodeMsg = (OffsetNodeMsg)customBlackBoardNode.Value;

                // 效果 剩余需要执行的时间 =  效果总时间 - 效果恢复的时间
                durningTime -= effectRecoverTime / 1000f;
                EffectUtils.HandleEffectOffset(effectParam, offsetNodeMsg, durningTime, moveLabel, moveType);
                return true;
            }

            return false;
        }

        private bool HandleClientEffectOffset(I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isRotMove, int effectRecoverTime)
        {
            /// 如果执行 位移的效果的恢复的时候, 就需要考虑一个问题: 恢复的时间点 是否在 这个效果 的执行时间内 (效果 可以配置 一个时间段)?
            ///     如果 在 这个效果的时间内不的话, 就需要正常的 播放位移 动作(怪物的坐标 应该通过aoi 提前同步, 所以 表现是 怪物会有一个位移的动作)
            ///     如果 不在这个效果 时间内的话, 那 其实不需要恢复这个效果
            // if (effectRecoverTime > 0 && effectParam.EffectRuntTime <= effectRecoverTime)
            // {
            //     return true;
            // }

            // 如果 服务器已经 位移了, 客户端 就不需要位移
            bool executeResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);
            if (executeResult)
            {
                //   LogUtils.LogError(LogEnum.SkillEffect, $"黑板阶段同步 【客户端】 位移效果 entityid={Data.M_EntityID},des={effectParam.EffectData.Desc} {effectParam.OutputKey} , 服务器已经位移, return");
                return true;
            }

            // 判断能不能找到【输出】【key】的黑板数据
            bool result = HandleEffectOffset(effectParam, blackBoard, isRotMove, effectRecoverTime);

            if (result)
            {
                //   LogUtils.LogError(LogEnum.SkillEffect, $"黑板阶段同步 【客户端】 位移效果 entityid={Data.M_EntityID},des={effectParam.EffectData.Desc} {effectParam.OutputKey}");
            }
            return result;
        }

        private bool HandleServerEffectOffset(I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isRotMove, int effectRecoverTime)
        {
            /// 如果执行 位移的效果的恢复的时候, 就需要考虑一个问题: 恢复的时间点 是否在 这个效果 的执行时间内 (效果 可以配置 一个时间段)?
            ///     如果 在 这个效果的时间内不的话, 就需要正常的 播放位移 动作(怪物的坐标 应该通过aoi 提前同步, 所以 表现是 怪物会有一个位移的动作)
            ///     如果 不在这个效果 时间内的话, 那 其实不需要恢复这个效果
            // if (effectRecoverTime > 0 && effectParam.EffectRuntTime <= effectRecoverTime)
            // {
            //     return true;
            // }

            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);
            if (clientExecuteResult)
            {
                //   LogUtils.LogError(LogEnum.SkillEffect, $"黑板阶段同步 【服务器】 位移效果 entityid={Data.M_EntityID},des={effectParam.EffectData.Desc} {effectParam.OutputKey} , 客户端已经位移, return");
                return true;
            }
            bool result = HandleEffectOffset(effectParam, blackBoard, isRotMove, effectRecoverTime);
            if (result)
            {
                //   LogUtils.LogError(LogEnum.SkillEffect, $"黑板阶段同步 【服务器】 位移效果 entityid={Data.M_EntityID},des={effectParam.EffectData.Desc},OutputKey={effectParam.OutputKey},effectRecoverTime={effectRecoverTime}");
            }
            return result;
        }

        #endregion

        #region 【旋转】效果

        private bool HandleTagEffectRotate(I_EffectParam effectParam, BaseBlackBoard blackBoard, E_BlackBoardTag tag, int rotateTime)
        {
            // string outPutKey = effectParam.OutputKey;
            bool cliExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);
            bool serExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);

            bool ExecuteResult = cliExecuteResult || serExecuteResult;

            // Debug.Log($"[Rotate] 开始执行 旋转效果 tag: {tag}, cliExecuteResult: {cliExecuteResult}, serExecuteResult: {serExecuteResult}, totalResult: {ExecuteResult}, tag: {tag} , rotateTime: {rotateTime} ");
            if (ExecuteResult)
            {
                return true;
            }

            return HandleEffectRotate(effectParam, blackBoard, rotateTime);

        }
        private bool HandleEffectRotate(I_EffectParam effectParam, BaseBlackBoard blackBoard, int rotateTime)
        {
            string outPutKey = effectParam.OutputKey;

            CustomBlackBoardNode customBlackBoardNode = (CustomBlackBoardNode)blackBoard.Get(outPutKey, E_BlackBoardTag.Server);
            if (customBlackBoardNode == null)
            {
                // Debug.Log($"[Rotate] 执行 旋转效果: false");
                return false;
            }

            // Debug.LogError($"[Rotate] 执行 旋转效果: true");
            UpRotaNodeMsg upRotaNodeMsg = (UpRotaNodeMsg)customBlackBoardNode.Value;
            EffectUtils.HandleEffectRotate(upRotaNodeMsg, rotateTime);

            return true;
        }

        private bool HandleClientEffectChangeToward(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {

            // 效果的持续时间
            int durningTime = (effectParam.BaseEffect as EffectTypeChangeToward).DurningTime;

            // 如果配置了效果时间, 才需要走恢复逻辑，否则的话 瞬间 执行
            if (durningTime > 0)
            {
                if (durningTime <= effectRecoverTime && effectRecoverTime > 0)
                {
                    return true;
                }
            }

            int leastTime = durningTime - effectRecoverTime;
            // 1.先找->服务器效果线【输出】【key】 有没有->黑板数据
            bool result = HandleTagEffectRotate(effectParam, blackBoard, E_BlackBoardTag.Client, leastTime);
            //   SGF.Debuger.Log($"黑板阶段同步 【客户端】 旋转效果 entityid={Data.M_EntityID},des={effectParam.EffectData.Desc} , result: {result}");
            return result;
        }

        private bool HandleServerEffectChangeToward(I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            // 效果的持续时间
            int durningTime = (effectParam.BaseEffect as EffectTypeChangeToward).DurningTime;
            // 如果配置了效果时间, 才需要走恢复逻辑，否则的话 瞬间 执行
            if (durningTime > 0)
            {
                if (durningTime <= effectRecoverTime && effectRecoverTime > 0)
                {
                    return true;
                }
            }
            int leastTime = durningTime - effectRecoverTime;
            // 1.先找->服务器效果线【输出】【key】 有没有->黑板数据
            bool result = HandleTagEffectRotate(effectParam, blackBoard, E_BlackBoardTag.Server, leastTime);
            //   SGF.Debuger.Log($"黑板阶段同步 【服务器】 旋转效果 entityid={Data.M_EntityID},des={effectParam.EffectData.Desc} , result: {result}");
            return result;
        }

        private bool HandleEffectChangeToAbsoluteToward(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime, E_BlackBoardTag tag)
        {

            var effect = effectParam.BaseEffect as EffectTypeChangeToAbsoluteToward;

            string ChangeTimeKey = effect.ChangeTime.Result;
            var blackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(ChangeTimeKey, false);
            if (blackBoardNode == null)
            {
                return false;
            }
            var changeTime = (int)blackBoardNode.Value;
            int leastTime = changeTime - effectRecoverTime;
            // 1.先找->服务器效果线【输出】【key】 有没有->黑板数据
            //   SGF.Debuger.Log($"黑板阶段同步 【客户端】 旋转效果 entityid={Data.M_EntityID},des={effectParam.EffectData.Desc} , result: {result}");
            bool result = HandleTagEffectRotate(effectParam, blackBoard, tag, leastTime);
            return result;
        }

        #endregion

        #region 【在目标点播放特效】效果

        /// <summary>
        /// 在 多个点，播放不同朝向的 特效的 接口,后续看看要不要 作为通用逻辑
        /// note: 
        ///     1.在 指定的点上,按指定的朝向播放特效,所以 不需要人物挂点
        /// </summary>
        /// <param name="posArr"></param>
        /// <param name="towardArr"></param>
        private bool PlayEffectFxsAtPoint(RepeatedField<ProVec3> posArr, RepeatedField<int> towardArr, FXJson fXJson, ulong builderEntityID, ulong ownerEntityID, int startTime = 0)
        {
            if (posArr == null || posArr.Count == 0)
            {
                return false;
            }
            if (towardArr == null || towardArr.Count == 0)
            {
                return false;
            }

            if (!LocalFxManager.Instance.CheckEffectNeedPlay(fXJson, startTime))
            {
                SGF.Debuger.Log($"[NPCEntityBase] PlayEffectFxsAtPoint fxJson: {fXJson.EffectName} 播放时间: {startTime} 大于 {fXJson.Duration}, 跳过播放!!!");
                return true;
            }
            for (int i = 0; i < posArr.Count; i++)
            {
                ProVec3 pos = posArr[i];

                for (int j = 0; j < towardArr.Count; j++)
                {
                    int angle = towardArr[j];
                    FxParam fxParam = new();
                    fxParam.InitWithFxJson(fXJson, builderEntityID, ownerEntityID);
                    fxParam.SetFxStartTime(startTime);
                    EffectUtils.PlayFxAtPoint(fxParam, pos, angle);
                }
            }

            return true;
        }

        private bool PlayEffectAtPoint(I_EffectParam effectParam, BaseBlackBoard blackBoard, int startTime = 0)
        {
            EffectTypePlayEffectAtPoint playEffectAtPoint = effectParam.BaseEffect as EffectTypePlayEffectAtPoint;
            if (playEffectAtPoint == null)
            {
                return false;
            }

            FXJson fXJson = playEffectAtPoint.PrefabPathInEditor.fxJson;

            if (fXJson == null)
            {
                return false;
            }

            RepeatedField<ProVec3> posArr = EffectUtils.GetCenterPosArray(effectParam, playEffectAtPoint.CenterPosArray, blackBoard);

            RepeatedField<int> towardArr = EffectUtils.GetTowardArray(effectParam, playEffectAtPoint.TowardArray, blackBoard);

            bool result = PlayEffectFxsAtPoint(posArr, towardArr, fXJson, effectParam.Builder, effectParam.Owner, startTime);
            // SGF.Debuger.LogError($"[x-x] PlayEffectAtPoint {effectParam.EffectID} , outputkey {effectParam.OutputKey} , result {result}");
            return result;
        }

        private bool HandleClientPlayEffectAtPoint(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            bool serverExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);

            // 如服务器数据已经到了, 那执行客户端线前会先 执行服务器线.如果服务器线已经执行了,那客户端线就不需要执行
            if (serverExecuteResult)
            {
                return true;
            }
            // 效果的 恢复事件 其实就是 这个效果的 开始事件
            int startTime = effectRecoverTime;

            bool result = PlayEffectAtPoint(effectParam, blackBoard, startTime);
            return result;
        }

        /// <summary>
        /// 服务器效果线 在 某个点 播放对应的效果
        /// note:
        ///     如果客户端 效果线 已经播放了这个效果(需要根据 outPutKey 记录客户端 这个效果的执行结果),
        ///     服务器就不需要执行这个效果
        /// </summary>
        /// <param name="effectParam"></param>
        /// <returns></returns>
        private bool HandleServerEffectAtPoint(I_EffectParam effectParam, BaseBlackBoard baseBlackBoard, int effectRecoverTime)
        {
            // // 服务器 说 客户端表现效果 不应该走服务器线
            // return true;
            // check 客户端是否执行 了这个效果,如果执行了这个效果 且 result == false ,那就需要执行 这个效果,返回true
            //      如果客户端 已经执行过,且 result == true, 那就不需要执行这个效果,返回true
            //      如果 客户端 都还没执行这个效果,那服务器 不需要执行这个效果,返回false
            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, baseBlackBoard, E_BlackBoardTag.Client);

            // 如果客户端已经提前播放了 特效,那服务器线就不需要执行
            if (clientExecuteResult)
            {
                return true;
            }

            int startTime = effectRecoverTime;

            // 如果客户端没有提前执行成功(一般是缺乏数据),那服务器线直接执行这个效果,且返回 True
            return PlayEffectAtPoint(effectParam, baseBlackBoard, startTime);
        }

        #endregion

        #region 目标身上播放特效

        private bool HandleClientPlayEffectAtTarget(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            bool serverExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);

            // 如服务器数据已经到了, 那执行客户端线前会先 执行服务器线.如果服务器线已经执行了,那客户端线就不需要执行
            if (serverExecuteResult)
            {
                return true;
            }
            int startTime = effectRecoverTime;
            bool result = PlayEffectAtTarget(effectParam, blackBoard, startTime);
            return result;
        }

        private bool HandleServerPlayEffectAtTarget(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            // // 服务器 说 客户端表现效果 不应该走服务器线
            // return true;
            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);

            // 如果客户端已经提前播放了 特效,那服务器线就不需要执行
            if (clientExecuteResult)
            {
                return true;
            }
            int startTime = effectRecoverTime;
            return PlayEffectAtTarget(effectParam, blackBoard, startTime);
            // return true;
        }

        private bool PlayTargetsEffect(RepeatedField<ulong> targets, List<FXJson> fXJsons, ulong builderEntityID, ulong ownerEntityID, int startTime = 0)
        {
            if (targets.Count == 0)
            {
                return false;
            }

            if (fXJsons.Count == 0)
            {
                return false;
            }

            bool success = false;
            for (int i = 0; i < targets.Count; i++)
            {
                ulong targe = targets[i];

                // 特效
                for (int j = 0; j < fXJsons.Count; j++)
                {
                    FXJson fXJson = fXJsons[i];
                    if (!LocalFxManager.Instance.CheckEffectNeedPlay(fXJson, startTime))
                    {
                        SGF.Debuger.Log($"[NPCEntityBase] PlayTargetsEffect fXJson: {fXJson.EffectName} starTime: {startTime} 大于 {fXJson.Duration}, 跳过播放");
                        // 播放一个即认为成功
                        success = true;
                        continue;
                    }

                    FxParam fxParam = new();
                    fxParam.InitWithFxJson(fXJson, builderEntityID, ownerEntityID);
                    fxParam.SetFxStartTime(startTime);
                    EffectUtils.TargetPlayEffect(targe, fxParam);
                    // 播放一个即认为成功
                    success = true;
                }
            }
            return success;
        }

        private bool PlayEffectAtTarget(I_EffectParam effectParam, BaseBlackBoard blackBoard, int startTime = 0)
        {
            EffectTypePlayEffectAtTarget playEffectAtTarget = effectParam.BaseEffect as EffectTypePlayEffectAtTarget;
            string targetKey = playEffectAtTarget.CheckKey.Result;
            if (targetKey == "")
            {
                return false;
            }
            RepeatedField<ulong> targets = EffectUtils.GetKeyTargetArray(targetKey, effectParam, blackBoard);

            EffectTypeHitEffect[] hitEffects = playEffectAtTarget.HitEffect;
            if (hitEffects.Length == 0)
            {
                return false;
            }

            List<FXJson> fXJsons = new();
            foreach (EffectTypeHitEffect item in hitEffects)
            {
                fXJsons.Add(item.fxJson);
            }

            return PlayTargetsEffect(targets, fXJsons, effectParam.Builder, effectParam.Owner, startTime);
        }

        #endregion

        #region 在两点间播放特效

        private bool PlayEffectBetweenPoints(I_EffectParam effectParam, BaseBlackBoard blackBoard, int startTime = 0)
        {
            EffectTypePlayEffectBetweenPoints playEffectBetweenPoints = effectParam.BaseEffect as EffectTypePlayEffectBetweenPoints;

            RepeatedField<ProVec3> fromCenterPosArray = EffectUtils.GetSkillTargetsCenterPos(effectParam, playEffectBetweenPoints.FromCenterPosArray, blackBoard);
            if (fromCenterPosArray.Count == 0)
            {
                return false;
            }

            RepeatedField<ProVec3> toCenterPosArray = EffectUtils.GetSkillTargetsCenterPos(effectParam, playEffectBetweenPoints.ToCenterPosArray, blackBoard);
            if (toCenterPosArray.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < fromCenterPosArray.Count; i++)
            {
                ProVec3 startPoint = fromCenterPosArray[i];
                for (int j = 0; j < toCenterPosArray.Count; j++)
                {
                    ProVec3 endPoint = toCenterPosArray[i];

                    Vector3 dir = new(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z);

                    // 先只考虑 xz 屏幕的朝向,暂不考虑 空间 y轴
                    Vector3 dirXZ = new(dir.x, 0, dir.z);

                    // 计算出 朝向 与 x 轴的夹角 , 此夹角 即是 沿着 y 轴的 角度
                    float angle = Utils.GetAngle(dirXZ, Vector3.right, Vector3.up);

                    // 计算出长度
                    float length = dir.magnitude * 100;

                    float scale = length / playEffectBetweenPoints.UnitLength;

                    Vector3 scaleXYZ = new(1, 1, 1);
                    if (playEffectBetweenPoints.IsChangeX)
                    {
                        scaleXYZ.x = scale;
                    }
                    if (playEffectBetweenPoints.IsChangeY)
                    {
                        scaleXYZ.y = scale;
                    }
                    if (playEffectBetweenPoints.IsChangeZ)
                    {
                        scaleXYZ.z = scale;
                    }

                    EffectTypeHitEffect effectTypeHitEffect = playEffectBetweenPoints.PrefabPathInEditor;
                    FXJson fXJson = effectTypeHitEffect.fxJson;

                    if (!LocalFxManager.Instance.CheckEffectNeedPlay(fXJson, startTime))
                    {
                        SGF.Debuger.Log($"[NPCEntityBase] PlayEffectBetweenPoints fXJson: {fXJson.EffectName} startTime: {startTime} 小于 {fXJson.Duration}, 跳过播放");
                        continue;
                    }

                    FxParam fxParam = new();
                    fxParam.InitWithFxJson(fXJson, effectParam.Builder, effectParam.Owner);
                    fxParam.SetScale(scaleXYZ.x, scaleXYZ.y, scaleXYZ.z);
                    fxParam.SetFxStartTime(startTime);
                    EffectUtils.PlayFxAtPoint(fxParam, startPoint, (int)angle);
                }
            }

            return true;
        }

        private bool HandleClientPlayEffectBetweenTargets(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            bool serverExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);

            // 如服务器数据已经到了, 那执行客户端线前会先 执行服务器线.如果服务器线已经执行了,那客户端线就不需要执行
            if (serverExecuteResult)
            {
                return true;
            }
            int startTime = effectRecoverTime;

            bool result = PlayEffectBetweenPoints(effectParam, blackBoard, startTime);
            return result;
        }

        private bool HandleServerPlayEffectBetweenTargets(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);

            // 如果客户端已经提前播放了 特效,那服务器线就不需要执行
            if (clientExecuteResult)
            {
                return true;
            }
            int startTime = effectRecoverTime;

            return PlayEffectBetweenPoints(effectParam, blackBoard, startTime);
        }

        #endregion

        #region 隐身

        private void OnActionHidden(bool isHidden)
        {
            m_playerData.myOwnerNttGroup.SetModelVisiable(!isHidden);

        }
        private void StartHidden(int time, string key)
        {
            if (time <= 0)
            {
                //   LogUtils.LogError(LogEnum.SkillEffect, $"recover : StartHidden time: {time}, skip !!!");
                return;
            }
            // ActionModelVisiableTime?.Invoke(false, false, time / 1000f, key);
            ActionOnStartTimeHidden?.Invoke(time / 1000f, key);
        }

        private void StopHidden(string key)
        {
            ActionOnStopTimeHidden?.Invoke(key);
        }

        private bool PlayEffectStealth(I_EffectParam effectParam, BaseBlackBoard blackBoard, int startTime = 0)
        {
            EffectTypeStealth effectTypeStealth = effectParam.BaseEffect as EffectTypeStealth;
            if (effectTypeStealth == null)
            {
                return false;
            }
            int leastTime = effectTypeStealth.DurningTime - startTime;
            StartHidden(leastTime, effectParam.UniqueKey);
            return true;
        }

        private bool HandleClientStealth(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            bool serverExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);

            // 如服务器数据已经到了, 那执行客户端线前会先 执行服务器线.如果服务器线已经执行了,那客户端线就不需要执行
            if (serverExecuteResult)
            {
                return true;
            }
            int startTime = effectRecoverTime;
            return PlayEffectStealth(effectParam, blackBoard, startTime);
        }

        private bool HandleServerStealth(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
            // // 服务器 说 客户端表现效果 不应该走服务器线
            // return true;
            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);

            // 如果客户端已经提前播放了 特效,那服务器线就不需要执行
            if (clientExecuteResult)
            {
                return true;
            }
            int startTime = effectRecoverTime;
            return PlayEffectStealth(effectParam, blackBoard, startTime);
        }

        private void StopStealth(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            string key = effectParam.UniqueKey;
            StopHidden(key);
        }
        #endregion

        private bool HandleUserInput(I_EffectParam effectParam, BaseBlackBoard blackBoard, bool isFrameStart)
        {
            bool result = false;

            // 先处理结束帧逻辑
            // 结束帧只是客户端独有的逻辑,如果执行到结束帧,说明 玩家没有任何输入,服务器也没有返回任何 的 输入轴对应的key
            // 所以对于客户端来说,就是执行 配置表中 配置的 用户输入 结束逻辑

            return result;
        }

        #region 二次触发器伤害
        private bool HandleEffectDamage(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            string outPutKey = effectParam.OutputKey;
            CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(outPutKey);
            if (customBlackBoardNode == null)
            {
                return false;

            }
            HurtNodeMsg hurtMsg = (HurtNodeMsg)customBlackBoardNode.Value;
            if (hurtMsg == null)
            {
                return false;
            }
            EffectUtils.HandleEffectDamageSecond(hurtMsg, effectParam);
            return true;
        }

        private bool HandleClientDamageSecond(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            bool serverExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Server);
            // 如果服务器端已经提前播放了 特效,那客户端线就不需要执行
            if (serverExecuteResult)
            {
                return true;
            }
            bool result = HandleEffectDamage(effectParam, blackBoard);

            if (result)
            {
                //   SGF.Debuger.Log($"黑板阶段同步 【客户端】 二次伤害效果 effectid={effectParam.EffectID} result: {result}");
            }
            return result;
        }

        private bool HandleServerDamageSecond(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);
            // 如果客户端已经提前播放了 特效,那服务器线就不需要执行
            if (clientExecuteResult)
            {
                return true;
            }

            bool result = HandleEffectDamage(effectParam, blackBoard);
            if (result)
            {
                //   SGF.Debuger.Log($"黑板阶段同步 【服务器】 二次伤害效果 effectid={effectParam.EffectID},result={result}");
            }
            return result;
        }

        #endregion

        #region 判断目标效果是否为空
        /// <summary>
        /// 处理 客户端线 判断 目标key 是否存在. 
        /// note:
        ///     1.目前 策划gl的要求, 判定效果由 服务器判定, 客户端 的客户端线并不提前计算;
        ///     2.客户端 线在执行的时候,存在 服务器线 数据 先到/ 未到 两种情况;
        ///         如果服务器 数据 先到, 由服务器 触发 这个效果是否执行注册 Next;
        ///         如果服务器 数据 未到, 客户端线 只需要打个 等待服务器注册的标签,标识自己不需要注册Next 效果即可。 
        ///     
        ///     3.对于客户端线, 客户端线 执行 结果返回 true/false 差别不大,默认就 返回true,
        ///       对于客户线来说, 目前这个效果 并没有其它的特殊逻辑,所以就返回true.
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        private bool HandleClientIsNotEmpty(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                /// 2023/10/20
                /// IsNotEmpty: 判断目标Key是否不为空
                /// 原始的 设计, 对于这种 判定效果, 先由客户端线执行这个效果的注册逻辑.
                /// 再由 服务器线 触发这个效果的Next 效果 注册.
                /// 
                /// 对于本地服而言，并没有在本地服模拟技能的流程。技能的表现还是依赖于
                /// 客户端线 的逻辑. 
                /// 所以对于 这种判定效果, 如果是本地服的状态下, 客户端线在执行的时候, 可以
                /// 直接执行. 并 输出对应的 outPutKey
                var isNotEmpty = effectParam.BaseEffect as EffectTypeIsNotEmpty;

                string inputKey = isNotEmpty.CheckKey.Result;
                string outputKey = isNotEmpty.OutputKey.Result;

                var resultBlackBoard = EffectUtils.GetIsEffectResult(blackBoard, inputKey, outputKey, E_BlackBoardTag.Client);

                // 判断效果的数据 依旧存入 客户端黑板中. 
                blackBoard.Set(outputKey, resultBlackBoard, E_BlackBoardTag.Client, effectParam.SaveSkill);

                if ((bool)resultBlackBoard.Value == true)
                {
                    SGF.Debuger.LogWarning($"{TagFlag} effectID : {effectParam.EffectID} IsNotEmpty 判空效果 先存入 黑板的结果数据 ");
                }


            }
#endif
            EffectResultUtils.RegIsWaitServerRegNext(blackBoard, effectParam, true);
            // SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} HandleClientIsNotEmpty  RegIsWaitServerRegNext : true");
            return true;
        }

        /// <summary>
        /// 处理 服务器线 判定目标key是否 存在.
        /// note:
        ///     1.服务器线 的判定结果 由 服务器直接下发,所以 服务器
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        private bool HandleServerIsNotEmpty(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            EffectResultUtils.RegIsWaitServerRegNext(blackBoard, effectParam, true);
            if (!blackBoard.Contain(effectParam.OutputKey, E_BlackBoardTag.Server))
            {
                return false;
            }
            // SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} HandleServerIsNotEmpty RegIsWaitServerRegNext : true");
            return true;
        }
        #endregion

        #region  判断 职业的 效果节点
        /// <summary>
        /// 判断是否是 某个职业的 效果节点, gl 目前有个需求是 不同的职业，新手会有不同的 变身被动.
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        private bool HandleClientIsJob(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {

            EffectResultUtils.RegIsWaitServerRegNext(blackBoard, effectParam, true);
            // SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} HandleClientIsNotEmpty  RegIsWaitServerRegNext : true");
            return true;
        }

        private bool HandleServerIsJob(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            EffectResultUtils.RegIsWaitServerRegNext(blackBoard, effectParam, true);
            if (!blackBoard.Contain(effectParam.OutputKey, E_BlackBoardTag.Server))
            {
                return false;
            }
            // SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} HandleServerIsNotEmpty RegIsWaitServerRegNext : true");
            return true;
        }

        #endregion

        #region 
        private bool HandleClientIsIntKey(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {

            EffectResultUtils.RegIsWaitServerRegNext(blackBoard, effectParam, true);
            // SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} HandleClientIsNotEmpty  RegIsWaitServerRegNext : true");
            return true;
        }

        /// <summary>
        /// 判断效果 isIntKey
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        private bool HandleServerIsIntKey(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            EffectResultUtils.RegIsWaitServerRegNext(blackBoard, effectParam, true);
            if (!blackBoard.Contain(effectParam.OutputKey, E_BlackBoardTag.Server))
            {
                return false;
            }
            // SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} HandleServerIsNotEmpty RegIsWaitServerRegNext : true");
            return true;
        }

        #endregion



        #region 处理抛物线效果
        /// <summary>
        /// 处理 子弹抛物线 效果, 目前 约定 抛物线的 起始点和落点 需要在同一个水平面
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="stageRecoverTime"></param>
        /// <returns></returns>
        private bool HandleServerThrowBullet(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {


            // 这个效果 完全依赖服务器数据,所以 可以完全依赖服务器线执行这个效果
            EffectTypeThrowBullet throwBullet = effectParam.BaseEffect as EffectTypeThrowBullet;

            string inputKey = throwBullet.TargetPoint.Result;

            RepeatedField<ProVec3> targetPoints = EffectUtils.GetKeyPosArray(inputKey, effectParam, blackBoard);

            // 此时黑板中 抛物线的 目标点 没有找到, 所以就不抛了
            if (targetPoints.Count == 0)
            {

                return true;
            }

            string outKey = throwBullet.OutputKey.Result;

            RepeatedField<ProVec3> startPoints = EffectUtils.GetKeyPosArray(outKey, effectParam, blackBoard);

            // 黑板中 抛物线的 开始点找不到, 那也不抛了
            if (startPoints.Count == 0)
            {
                return true;
            }

            Vector3 startPoint = ProtoUtils.ConvertProtoVec3ToUnityVec3(startPoints[0]);
            Vector3 endPoint = ProtoUtils.ConvertProtoVec3ToUnityVec3(targetPoints[0]);
            // 将 开始点 和 结束点 投影到 XY 平面上, 策划的要求 抛物线 水平方向移动 都是 XY 平面上 按速度v 移动
            {
                startPoint.y = 0;
                endPoint.y = 0;
            }

            int maxHight = throwBullet.MaxHight;

            StartThrowBullet(startPoint, endPoint, maxHight / 100f, stageRecoverTime / 1000f);

            return true;
        }

        private bool HandleClientThrowBullet(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {

            // 这个效果 完全依赖服务器数据,所以 可以完全依赖服务器线执行这个效果
            EffectTypeThrowBullet throwBullet = effectParam.BaseEffect as EffectTypeThrowBullet;

            string inputKey = throwBullet.TargetPoint.Result;

            RepeatedField<ProVec3> targetPoints = EffectUtils.GetKeyPosArray(inputKey, effectParam, blackBoard);

            // 此时黑板中 抛物线的 目标点 没有找到, 所以就不抛了
            if (targetPoints.Count == 0)
            {
                return true;
            }

            string outKey = throwBullet.OutputKey.Result;

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                ProVec3 vec3 = PbPosition();
                CustomBlackBoardNode customBlackBoardNode = BaseBlackBoard.InitClientCustomBlackBoardNode(outKey, vec3, effectParam.Builder, effectParam.Owner);

                // 写入客户端黑板中
                blackBoard.Set(outKey, customBlackBoardNode, E_BlackBoardTag.Client);
            }
#endif

            RepeatedField<ProVec3> startPoints = EffectUtils.GetKeyPosArray(outKey, effectParam, blackBoard);

            // 黑板中 抛物线的 开始点找不到, 那也不抛了
            if (startPoints.Count == 0)
            {
                return true;
            }

            Vector3 startPoint = ProtoUtils.ConvertProtoVec3ToUnityVec3(startPoints[0]);
            Vector3 endPoint = ProtoUtils.ConvertProtoVec3ToUnityVec3(targetPoints[0]);
            // 将 开始点 和 结束点 投影到 XY 平面上, 策划的要求 抛物线 水平方向移动 都是 XY 平面上 按速度v 移动
            {
                startPoint.y = 0;
                endPoint.y = 0;
            }

            int maxHight = throwBullet.MaxHight;

            StartThrowBullet(startPoint, endPoint, maxHight / 100f, 0);


            return true;
        }


        /// <summary>
        /// 开启一个 子弹抛物线的效果,
        /// 
        /// 2023/7/8
        ///  note: 
        /// 抛物线如果 是走 恢复的逻辑, 可能需要考虑2种恢复方式:
        ///     1.根据 恢复时间 执行恢复
        ///     2.根据 玩家的当前 属性同步坐标恢复.  
        /// 整体来说, 恢复时间 对于客户端来说 有个容错 的时间, 所以其实不太准确。
        /// 即:
        ///     子弹可能出现 AOI属性 同步的坐标 在 后面,但是 根据时间恢复的坐标还在前面的情况.  
        ///     目前 效果的恢复 统一 走的 是按时间 恢复的流程, 先按统一 的流程处理,
        ///     后续如果有 更加细致的要求, 在按当前坐标 在曲线种的 水平面进度 做更加细致的 表现恢复
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="maxHight"></param>
        /// <param name="startTime"></param>
        private void StartThrowBullet(Vector3 startPoint, Vector3 endPoint, float maxHight, float startTime)
        {
            float distance = Vector3.Distance(startPoint, endPoint);
            float totalTime = distance / Speed;

            // 如果 抛物线的开始时间 要大于 总的运动时间, 那就不抛了
            if (startTime >= totalTime)
            {
                return;
            }
            // 高磊大哥让加的变量
            Vector3 directionXZ = endPoint - startPoint;
            directionXZ.y = 0;

            // 开启模拟运动后, 先禁止 服务器的路点位移
            SetIsSimulateMove(true);
            SetIsSimulateRotation(true);

            ActionOnPlayThrowBullet?.Invoke(totalTime, maxHight, startTime, directionXZ / totalTime, () =>
            {
                // SetIsSimulateMove(false);
                // 抛物线动画模拟结束后, 将这个实体 设置为服务器 同步的结束点
                endPoint.y = Position().y;
                // MoveByServer(endPoint, true);

                //TODO DL 临时屏蔽 ，好像有bug
                {
                    // MoveByServerNew(endPoint, true, true);
                }
            });
        }

        /// <summary>
        /// 服务器播放追踪弹的效果
        /// 2023/9/4
        /// note1:
        ///     追踪弹的恢复 不太好用 时间来恢复. 对于一个正在追踪过程中的追踪弹来说， 直接按照当前的子弹位置开启追踪即可.
        ///     但如果 是追踪结束了的 子弹， 那它执行这个效果的瞬间就应该结束了. 
        /// note2:
        ///     如果是 一个已经追踪结束的子弹, 后续 还有自己的 位移逻辑， 那此处的 追踪逻辑应该被 其它的 位移效果打断， 从而保证恢复的正确性
        /// </summary>
        /// <returns></returns>
        public bool HandleServerPlayLaunchBullet(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            EffectTypeLaunchBullet launchBullet = effectParam.BaseEffect as EffectTypeLaunchBullet;
            string inputKey = launchBullet.ToTarget.Result;

            var targetIDs = EffectUtils.GetKeyTargetArray(inputKey, effectParam, blackBoard);
            // 没有数据的时候，服务器没有目标节点， 那这个效果就不管
            if (targetIDs.Count == 0)
            {
                return true;
            }
            // 取第一个 为目标
            ulong targetEntityID = targetIDs[0];
            HangPoint hangPoint = launchBullet.ToTargetHangPoint;

            bool isTargetDie2Stop = false;

            StartPlayLaunchBullet(targetEntityID, hangPoint, Speed, isTargetDie2Stop);
            return true;
        }


        private void StartPlayLaunchBullet(ulong targetEntityID, HangPoint hangPoint, float speed, bool isTargetDie2Stop)
        {
            // 开启模拟运动后, 先禁止 服务器的路点位移
            SetIsSimulateMove(true);
            SetIsSimulateRotation(true);

            ActionOnPlayTrackingBullet?.Invoke(targetEntityID, hangPoint, speed, isTargetDie2Stop, () =>
            {
                // SetIsSimulateMove(false);
                SGF.Debuger.Log($"[Track-Bullet] 模拟运动结束后, position: {Position()} , serverPos: {ServerPosition}");
                // 抛物线动画模拟结束后, 将这个实体 设置为服务器 同步的结束点
                // MoveByServer(ServerPosition, true);
                MoveByServerNew(ServerPosition, true, false);
            });
        }

        #endregion

        #region 播放 贝塞尔子弹
        private bool HandleServerBezierBullet(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            EffectTypeBezierBullet bezierBullet = effectParam.BaseEffect as EffectTypeBezierBullet;

            // 曲线播放的总时间
            float totalTime = 1f;

            // 曲线当前时间
            float curTime = stageRecoverTime / 1000f;

            RepeatedField<ulong> targetIDs = EffectUtils.GetKeyTargetArray(bezierBullet.InputKey.Result, effectParam, blackBoard);

            // 目标的 实体id
            ulong targetID = targetIDs.Count > 0 ? targetIDs[0] : 0;


            Vector3 targetPos = Vector3.zero;

            // 如果目标id 存在, 那就采用目标id 的 坐标
            if (targetID != 0)
            {
                var targeEntity = GameManager.Instance.GetEntityByEntityID(targetID);
                targetPos = targeEntity.Position();
            }
            else
            {
                var targetCenterPos = EffectUtils.GetSingleCenterPos(effectParam, bezierBullet.SingleCenterPos, blackBoard);
                if (targetCenterPos != null)
                {
                    ProtoUtils.CopyPbPos2V3(out targetPos, targetCenterPos);
                }
            }




            // 开启模拟运动的标志
            SetIsSimulateMove(true);
            SetIsSimulateRotation(true);

            /// 表现层开启 贝塞尔曲线的模拟 
            /// note1:
            ///     此处开启 贝塞尔曲线 不考虑恢复时间.由于服务器没有记录贝塞尔曲线开始的第一个点(释放技能时的点), 
            ///     所以过程恢复 是没办法进行的. 
            ///     所以此处直接按当前的位置 做抛物线表现即可
            /// note2:
            ///     targetID 提前获取, 在表现层 直接根据targetID 是否为0 来看是否是跟随玩家的贝塞尔曲线.
            /// note3:
            ///     如果 targetID 存在但是过程中怪物死亡, 那贝塞尔曲线 结束.
            ActionOnPlayBezierBullet?.Invoke(targetID, targetPos, bezierBullet, () =>
            {

                // 结束模拟运动的标志
                // SetIsSimulateMove(false);
                // SetIsSimulateRotation(false);

                MoveByServerNew(ServerPosition, true, false);
            });

            return true;
        }

        #endregion

        #region 播放 LineRender 效果
        private bool HandleServerPlayEffectLineRender(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            EffectTypePlayEffectLineRenderer lineRenderEffect = effectParam.BaseEffect as EffectTypePlayEffectLineRenderer;
            LineRendererConfig lineRenderCfg = lineRenderEffect.LineRendererConfig;

            // 如果 恢复时间 超过了 效果时间, 那就 不需要恢复
            if (stageRecoverTime >= lineRenderCfg.EndTime)
            {
                return true;
            }

            string outPutKey = effectParam.OutputKey;

            // 当 服务器黑板中 不包含 这个效果的outputkey， 就表示 这个效果由 它的 前置效果提前触发.
            bool isServerPreTrigger = !EffectUtils.CheckIsBlackBoardHasKey(blackBoard, outPutKey, E_BlackBoardTag.Server);


            string fromeKey = lineRenderCfg.FromTarget.Result;

            var fromPoints = EffectUtils.GetKeyTargetArray(fromeKey, effectParam, blackBoard).KToList();
            if (fromPoints.Count == 0)
            {
                // 如果没找到数据，并且是 其它效果提前触发， 那其实就需要返回 false
                return !isServerPreTrigger;
            }

            string toTargetKey = lineRenderCfg.ToTarget.Result;

            var targetPoints = EffectUtils.GetKeyTargetArray(toTargetKey, effectParam, blackBoard).KToList();
            if (targetPoints.Count == 0)
            {
                return !isServerPreTrigger;
            }

            Data.myOwnerNttGroup.ControlPlayLineRender(lineRenderCfg, fromPoints, targetPoints, stageRecoverTime, blackBoard.GetHashCode().ToString());
            return true;
        }

        /// <summary>
        /// 播放 line Render 的接口。 不管 gl 如何定义 配置表, 但基础接口不变
        /// <param name="lineRenderCfg"></param>
        /// <param name="startPoints"></param>
        /// <param name="targetPoints"></param>
        /// <param name="startTime"></param>
        /// <param name="tagKey"></param>
        public void PlayLineRender(LineRendererConfig lineRenderCfg, List<ulong> targets, int startTime, string tagKey)
        {
            ActionOnPlayEffectLineRenderer?.Invoke(lineRenderCfg, targets, startTime, tagKey);
        }

        public void StopLineRenders(LineRendererConfig lineRenderCfg, string tagKey)
        {
            ActionOnStopEffectLineRenderer?.Invoke(lineRenderCfg, tagKey);
        }

        #endregion

        #region 黑洞效果
        /// <summary>
        /// 客户端 处理 黑洞效果, 如果能够 找到服务器下发的数据, 那客户端就 直接播放 这个效果;
        /// 如果 找不到, 那就返回 false
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="stageRecoverTime"></param>
        /// <returns></returns>
        public bool HandleClientEffectTypeRegister(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            // 如果 客户端 已经执行了 这个效果（目前 只是播放特效）
            bool clientExecuteResult = EffectResultUtils.CheckEffectExecuteResult(effectParam, blackBoard, E_BlackBoardTag.Client);
            if (clientExecuteResult)
            {
                return true;
            }

            // 如果没有执行这个黑洞的客户端效果, 那就是先检查是否有这个效果的 服务器黑板数据, 如果 数据没有的话, 那就 不需要执行客户端黑板效果, 
            // 等服务器收到 这个黑板效果的 outputKey 后再去触发
            EffectTypeRegister effectTypeRegister = effectParam.BaseEffect as EffectTypeRegister;
            string outPutKey = effectParam.OutputKey;

            // 检查是否有这个黑板数据
            bool isHasOutPutKey = EffectUtils.CheckIsBlackBoardHasKey(blackBoard, outPutKey, E_BlackBoardTag.Server);

            if (!isHasOutPutKey)
            {
                return false;
            }

            // 如果 存在黑板数据， 那就 拿到这个黑板数据, 然后 去播这个 黑洞效果
            object serverBlackBoardNode = blackBoard.Get(outPutKey, E_BlackBoardTag.Server);
            CustomBlackBoardNode customServerBlackBoardNode = (CustomBlackBoardNode)serverBlackBoardNode;

            // 拿到对应的 黑洞效果数据
            BlackHoleNodeMsg blackHoleNodeMsg = (BlackHoleNodeMsg)customServerBlackBoardNode.Value;

            ProtoUtils.CopyPbPos2V3(out Vector3 v3, blackHoleNodeMsg.Singularities);
            SGF.Debuger.LogWarning($"{TagFlag} 客户端播放 [黑洞] 效果, 坐标: [{v3}]");

            return true;
        }

        /// <summary>
        /// 黑洞效果, 由服务器 告知 在哪个点开始播. 
        /// 2023/9/25
        ///     目前跟夏哥的沟通思路如下:
        ///     1.在玩家黑洞状态下,客户端 发送给服务器 的是指令信息.此状态下客户端 不做坐标提前计算;
        ///     2.客户端收到 服务器的 属性坐标同步和, 往那个坐标移动.(此时速度计算 是个问题，延迟状态下感觉手感会有问题).
        ///     3.在完全依赖服务器 坐标控制去情况下,  服务器同步过来的 黑洞效果, 客户端关注的只需要 一个 黑洞中心点的问题.
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="stageRecoverTime"></param>
        public bool HandleServerEffectTypeRegister(I_EffectParam effectParam, BaseBlackBoard blackBoard, int stageRecoverTime)
        {
            SGF.Debuger.LogWarning($"{TagFlag} 收到服务器 [黑洞] 效果");
            // 每次收到服务器 的 黑洞效果之后, 都去触发一次 客户端的 黑洞效果, 内部会做 重复播放的过滤
            HandleClientEffectTypeRegister(effectParam, blackBoard, stageRecoverTime);

            EffectTypeRegister effectTypeRegister = effectParam.BaseEffect as EffectTypeRegister;


            // 这个效果服务器会 通过黑板不停的 触发, 所以 每次 收到 这个效果的时候, 都去刷新 服务器黑板的数据, 并且标记执行结果为false.
            // 这样 服务器黑板的效果的监听就不会被取消
            return false;
        }

        #endregion

        #region 客户端创建子弹的效果
        public bool HandleClientCreateBullet(I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                // 客户都创建子弹 先只放在编译器模式使用
                GlobalEvent.OnClientReqLocalServerEvent.Invoke(ClientEventReq.CreateBullet, new object[] { effectParam, blackBoard });
            }
#endif
            return true;
        }
        #endregion

        #region 设置 子弹目标点的效果
        /// <summary>
        /// 设置子弹 目标点的效果
        /// 跟服务器确认这个效果的逻辑如下:
        ///     1. 目标点 存入 运行时黑板中(非阶段) 发给客户端。
        ///     2. 子弹的移动 交给 移动节点移动
        /// note:
        ///     此处阶段处理这个效果的时候,采用的 是 stageUID.此处需要注意以下几点:
        ///     1.对于 技能来说, 阶段由本地创建。所以 stageUID 在客户端预播时 可能还没收到服务器的 runStageRet.
        ///       此时 stageID 为0. 
        ///     2.对于 子弹/被动/buff 而言, 阶段统一由服务器 runStageRet 创建.
        ///       所以 此处子弹的 stageUID 一定存在. 本地服 通过 UID 确定唯一的 阶段. 从而模拟 服务器 阶段效果 推动给客户端的逻辑
        /// 
        /// </summary>
        /// <param name="runtimeID">运行时ID</param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="effectRecoverTime"></param>
        /// <returns></returns>
        public bool HandleClientSetBulletTargetPos(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard, int effectRecoverTime)
        {
#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                EffectTypeSetBulletTargetPos effectTypeSetBulletTargetPos = effectParam.BaseEffect as EffectTypeSetBulletTargetPos;

                ProVec3 centerProVec3 = EffectUtils.GetSingleCenterTargetKeyPos(effectParam, effectTypeSetBulletTargetPos.SingleCenterPos, blackBoard);
                if (centerProVec3 == null)
                {
                    SGF.Debuger.Log($"{TagFlag} 子弹 SetBulletTargetPos 时 子弹实体[{EntityId}] 找不到 SingleCenterPos 目标点");
                    return true;
                }

                Vector3 centerPosV3 = ProtoUtils.ConvertProtoVec3ToUnityVec3(centerProVec3);

                // 目标点 和 子弹的当前 坐标点 决定了子弹的 当前朝向. 
                var distance = effectTypeSetBulletTargetPos.Distance / 100f;

                // 基于实体的 客户端朝向, 移动到目标节点 需要转角 多少度.
                // 对于 转角来说， 不管服务器的 角度 跟客户端是否 镜像, 客户端 角度右转 +50， 服务器角度也一样才对。 
                int clientOffsetAngle = EffectUtils.GetTargetEntity2PosAngle(EntityId, centerPosV3, out bool result);

                // 计算出 客户端的角度
                var clientAngle = (int)EulerAngles.y + clientOffsetAngle;

                Vector3 targetPos = Vector3.zero;

                switch (effectTypeSetBulletTargetPos.BuilderOrPos)
                {
                    case BuilderOrPos.Builder:
                        {

                            // 如果是基于 自身(注意, 高磊此处的 自身 指的是 子弹运行时的 owner)，
                            EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(effectParam.Owner);

                            // 那子弹的目标点就是子弹当前坐标到 centerPos 目标点方向  Distance 的坐标
                            targetPos = Fire.Utils.PosMoveByClientRota(entityCtrlBase.M_Curr.Position(), clientAngle, distance);

                        }
                        break;
                    case BuilderOrPos.Pos:
                        {
                            // 如果是 基于坐标， 那子弹的目标节点就是 centerPos 往前 Distance 的坐标
                            targetPos = Fire.Utils.PosMoveByClientRota(centerPosV3, clientAngle, distance);
                        }
                        break;

                    default: break;
                }

                int serverRot = (int)Utils.GetRoata2ServerAngle(clientAngle);

                // 此时， 计算出了 子弹的目标点 targetPos 和 当前的朝向

                // 对于子弹实体来说, 设置 子弹目标的这个 会产生2种行为:
                // 1.将 子弹目标 的输入存入 子弹黑板运行时(这个在客户端线自己执行);
                // 2.将 子弹目标 转换为 路点数据 ，跟随 当时的速度/朝向 作为属性一起同步给 客户端

                SGF.Debuger.LogWarning($"子弹效果 设置 目标坐标: {targetPos} ");
                Vector3 startPos = Position();
                if (!FindPathManager.Instance.CheckWalkable(startPos, targetPos))
                {
                    if (!FindPathManager.Instance.FindValidPointNearby(targetPos, out targetPos))
                    {
                        SGF.Debuger.LogWarning($"子弹效果 设置 目标坐标: {targetPos} , 找不到 附件有用的 路点");
                        return true;
                    }
                    SGF.Debuger.LogWarning($"子弹效果 设置 目标坐标 变更---> {targetPos} ");
                }

                // 找到对应的 路点数据
                Vector3[] path;
                if (targetPos.Equals(startPos))
                {
                    path = new Vector3[0];
                }
                else
                {
                    Service.FindPath.FindPathManager.Instance.FindPath(Position(), targetPos, 0, out path, 0);
                }

                CustomBlackBoardNode customBlackBoardNode = BaseBlackBoard.InitClientCustomBlackBoardNode(effectParam.OutputKey, ProtoUtils.ConvertUnityVec3ToProtoVec3(targetPos), effectParam.Builder, effectParam.Owner);


                blackBoard.Set(effectParam.OutputKey, customBlackBoardNode, E_BlackBoardTag.Client, effectParam.SaveSkill);
                // 将 目标点设置进入 子弹黑板中 而不是 阶段黑板
                // if (effectParam.SaveSkill)
                // {
                //     blackBoard.Set(effectParam.OutputKey, ProtoUtils.ConvertUnityVec3ToProtoVec3(targetPos), E_BlackBoardTag.Client, false);
                // }
                // else
                // {
                //     // 这个效果 策划跟服务器 是单独约定的, 没有走 saveSkill,所以此处单独设置
                //     blackBoard.Set(effectParam.OutputKey, ProtoUtils.ConvertUnityVec3ToProtoVec3(targetPos), E_BlackBoardTag.Client, true);
                // }
                //path.Shift(Position());
                // path.Add(Position());
                SGF.Debuger.LogWarning($"子弹效果 设置 目标坐标: {targetPos} , rota: {serverRot} , 路点: {path}");

                // 如果 目标点 就是 当前原点, 高磊的意思应该是立即触发 

                // 客户都创建子弹 先只放在编译器模式使用
                GlobalEvent.OnClientReqLocalServerEvent.Invoke(ClientEventReq.SetBulletTargetPos, new object[] { EntityId, runtimeID, targetPos, serverRot, path });
            }
#endif

            return true;
        }
        #endregion

        #region 打断子弹 当前阶段的效果
        public bool HandleClientBreakCurRuntimeInBullet(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                var breakCurRuntimeInBullet = effectParam.BaseEffect as EffectTypeBreakCurRuntimeInBullet;

                GlobalEvent.OnClientReqLocalServerEvent.Invoke(ClientEventReq.BreakCurRuntimeInBullet, new object[] { EntityId, runtimeID, breakCurRuntimeInBullet });

            }
#endif
            return true;
        }
        #endregion

        #endregion



        #region 坐标设置逻辑

        /// <summary>
        /// 服务器强制刷新位置同步：怪-直接拉就行了
        /// </summary>
        /// <param name="v"></param>
        /// <param name="hasInit"></param>
        public override void ServerSetPosition(object v, bool hasInit, bool isSyncView)
        {


            if (lock_syncPreSkillPos)
            {
                //   SGF.Debuger.Log($"Monster  SetPosition isBornOrS_Ctrl {hasInit} lock_syncPreSkillPos {lock_syncPreSkillPos} return!!!");
                isSyncView = false;
            }


            if (Data.Is___ForbidMove)
            {
                //   SGF.Debuger.Log($"Monster  SetPosition IsForbidMove {Is___ForbidMove} return!!!");
                isSyncView = false;
            }

            base.ServerSetPosition(v, hasInit, isSyncView);
        }

        private Vector3 PreSkillPosTmp = Vector3.zero;  // 技能临时缓存的坐标

        ///客户端/服务器==设置逻辑坐标，通知
        /// <summary> 设置自己的坐标点 </summary>
        public override void OnSetCurrentInvoke(bool isInvoke = false)
        {
            base.OnSetCurrentInvoke();
            if (isInvoke)
            {
                TargetPosDisInform();
            }
        }

        /// <summary>
        /// 目标坐标距离通知
        /// </summary>
        public void TargetPosDisInform()
        {
            if (Data == null)
            {
                return;
            }
            // 判断目标id与主角的距离
            // -- 主角自己每次都发
            if (Data.isMainPlayer)
            {
                ActionOnCheckTargetMove?.Invoke();
            }
            // -- 如果是目标实体
            else if (BattleManager.Instance.CurAtkEntity != null && BattleManager.Instance.CurAtkEntity.EntityId == EntityId)
            {
                ActionOnCheckTargetMove?.Invoke();
            }
        }

        private GameObject skillTestPoint;
        /// <summary>
        /// 提前处理使用（技能会快于aoi）
        /// 技能时的起始坐标（和当前坐标做平滑移动到终点后，开始释放技能）
        /// </summary>
        /// <param name="skillUseRet"></param>
        public void PreHandleSkillPostion(ProtoMsg.SkillUseRet skillUseRet, Action skillMoveEndAction)
        {
            ///执行SkillUseRet前处理坐标的问题:
            /// 需要区分是否是 客户端主动释放的技能
            /// 

            ///释放技能,需要区分几种情况:
            /// 1.其他人:
            ///     其他人坐标走的属性同步,在使用技能前,必定先走属性同步,所以可以认为客户端收到skillUseRet后,直接先设置坐标,然后使用技能
            /// 2.怪物:
            ///     怪物坐标也是走属性同步,是用路点的方式,使用技能时,服务器会先停路点,然后再给客户端发送skillUseRet,所以客户端收到skillUseRet后,
            ///     处理方式也是 先设置坐标,然后使用技能
            /// 3.主角自己(主动释放):
            ///     由于技能存在预施法的前摇阶段,所以需要区分 技能预施法阶段 是否执行了坐标原子锁(可能存在技能边走边释放):
            ///     a. 如果预施法阶段存在原子锁,那理论上skillUseRet 返回的坐标与 客户端一致
            ///     b. 如果不存在原子锁,那客户端应该 再合理范围内,采用客户端的坐标(因为此时服务器返回的坐标一定落后客户端)
            /// 4.主角自己(服务器释放)
            ///     此时跟怪物一样,理论上需要收到服务器的协议后,立即释放技能.
            ///     但主角的移动跟 怪物存在区别, 怪物的坐标 是 跟随AOI属性同步,所以在使用技能前,怪物的坐标理论上已经达到技能点的附件,
            ///     但主角的移动 是自己控制,所以在收到技能使用 的消息的时候,如果不是客户端 自己主动发起的技能, 服务器下发的技能坐标和客户端
            ///     大概率会存在偏差.
            ///     但既然此时是服务器主动推的技能, 那就直接使用服务器的技能坐标.
            if (Data == null)
            {
                return;
            }
            ProtoMsg.Vector3 targetPos = null;
            ProtoMsg.Vector3 skillPos = skillUseRet.Pos;

            Vector3 curPos = Position();

            //  SGF.Debuger.LogError($"[pos-x-{EnityId}] , xxxxxxxxx [skillUseRet] : skillPos [{skillPos.X},{skillPos.Y},{skillPos.Z}] , curPos [{curPos.x},{curPos.y},{curPos.z}]");

            switch (Data.EntityType)
            {
                case E_EntityType.None:
                    break;
                case E_EntityType.RoomSpace:
                    break;
                case E_EntityType.Player:
                    {
                        if (Data.isMainPlayer)
                        {
                            bool isClientUseSkill = skillController.CheckIsClientUseSkill(skillUseRet);
                            //如果不是客户端自己释放的技能,而是服务器主动释放
                            if (!isClientUseSkill)
                            {
                                targetPos = skillPos;
                            }
                            else
                            {
                                //如果是客户端自己使用技能,就需要区分该技能前摇阶段是否有位移的原子锁
                                // bool isForbidMove = skillController.CheckSkillStageHasState(skillUseRet.SkillID, E_ULayerSubState.Stage_SkillPro, E_BattleStateType.BattleState_ForbidMove);
                                // if (isForbidMove)
                                // {
                                //     targetPos = skillPos;
                                // }
                                // else
                                // {
                                //     targetPos = ProtoPosition();
                                // }


                                //TODO DL
                                // note：
                                //      之前需要提前判断 抬手阶段是否有原子锁,如果有原子锁,使用服务器的坐标,没有原子锁，使用本地坐标
                                //      现在 阶段没有抬手 阶段概念，预施法使用技能 也可以直接使用本地坐标
                                targetPos = PbPosition();

                            }
                        }
                        else
                        {
                            targetPos = skillPos;
                        }
                    }
                    break;
                case E_EntityType.Monster:
                case E_EntityType.Summon:
                case E_EntityType.Partner:
                    targetPos = skillPos;
                    break;
                default:
                    {
#if UNITY_EDITOR && BATTLE_DEBUG
                        LogUtils.LogError(LogEnum.SkillEffect, $"{TagFlag} PreHandleSkillPostion type : {Data.dataType} , no handle!!!");
#endif
                    }
                    break;
            }

            //if (skillTestPoint != null)
            //{
            ////    skillTestPoint = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/4") as GameObject);
            //    skillTestPoint = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/4");
            //}

            {
                //SGF.Debuger.LogError($"{TagFlag} 使用技能时收到的服务器坐标: CurrentPos: {CurrentPos} ----> targetPos: {targetPos} ");
                PreSkillPosTmp.x = targetPos.X;
                PreSkillPosTmp.y = CurrentPos.y;
                PreSkillPosTmp.z = targetPos.Z;



                //I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.SingleMoving);
                //ChangeState((GameKeyCommand)E_ULayerSubState.SingleMoving, animParam, false); //1，服务器提前关闭了路点暂停了给我，2技能接管移动如果网络延后播放跑动，3提前更需要播放跑动了：即一起延迟我让他跑（顺序不变），一起不延迟我也让他跑啊
                ActionOnForceSyncPosPerSkill?.Invoke(PreSkillPosTmp, skillMoveEndAction);
                //skillMoveEndAction?.Invoke();
                // Vector3 distXZ = _v3 - Position(); //移动速度是表面移动速度，类似周长，我假设模拟的是XZ平面的，爬坡移动速度变快了其实，先用服务器设定同步就没问题
                // distXZ.y = 0;
                // float moveTimeSec = (distXZ).magnitude / Speed;

                // SGF.Debuger.LogError($"LockRPCMessage , {true}");
                // m_playerData.myOwnerNttGroup.LockRPCMessage(true);
                // OnSkillMove(_v3, moveTimeSec, MoveLabel.Normal, MoveType.DefaultOffset, (bool result) =>
                // {
                //     skillMoveEndAction?.Invoke();
                //  //   SGF.Debuger.LogError($"LockRPCMessage , {false}");
                //     m_playerData.myOwnerNttGroup.LockRPCMessage(false);

                // });

                // DoSkillPathMove2?.Invoke(_v3,)
                //if (Data.isMainPlayer)
                //{
                // //   SGF.Debuger.Log($"{TagFlag} 技试试能  发送  角度={skillUseRet.Rot} [client] [input] SendPreUseSkillReq SendRPCMsg skillID {skillUseRet.SkillID} , runtimeID {skillUseRet.RuntimeID}  ");
                //}

                //skillTestPoint.name = "SkillPoint";
                //skillTestPoint.transform.position = _v3;
            }
        }

        //2服务器先关闭路点，【然后发技能消息】，都比较提前【隐患处理ForceSynvPosPerSkill】


        /// <summary>
        /// 技能位移回调
        /// </summary>
        protected void OnSkillMove(Vector3 endPos, float durningTime, MoveLabel moveLabel, MoveType moveType, string key, Action<bool> action)
        {
            if (!M_IsAlive)
            {
                return;
            }
            SkillViewSmooth(endPos, durningTime, moveLabel, moveType, key, action); //表现驱动平滑
        }

        protected virtual void SkillViewSmooth(Vector3 endPos, float durningTime, MoveLabel moveLabel, MoveType moveType, string key, Action<bool> action)
        {
            //人(我/别人)和怪物不同

        }

        /// <summary>
        /// 逻辑层平滑===
        /// </summary>
        /// <param name="endPos"></param>
        /// <param name="durningTime"></param>
        protected void FrameLogicSmooth(Vector3 endPos, int durningTime)
        {
            //朝向：Dir就是x，z没有Y
            Vector3 dirInterpolation = endPos - CurrentPos;

            dirInterpolation.y = 0;
            //float speed = Speed / (100 * GameConfig.FIX_TIME_PER_SEC);  // 600 / (3000) = 0.2f 米/帧 ； 每秒600cm = 每秒6m = 每帧0.2米
            float sec = durningTime / 1000f; //N个物理帧里面做平滑，按照次数即可

            float skillSpeed = (float)dirInterpolation.magnitude / sec; //距离多少米 / 多少秒  =  每秒多少米

            float speedMeterPerFrame = skillSpeed / GameConfig.FIX_TIME_PER_SEC; //每帧多少米

            skillMovePara.Begin(sec, dirInterpolation.normalized * speedMeterPerFrame, endPos);
        }

        #endregion

        // --------- 服务器来消息，控制其他玩家,后续属性通知不必强制设置主角=

        /// <summary>
        /// 复活
        /// </summary>
        public void ReliveDisplay()
        {
            M_clientStateCountdown = 0f;
            m_clientTag = E_ClentMainPlayerState.Normal;
            m_BattleMixTag = E_ClentMainPlayerState.Normal;
            ReSetBorn();
            ForceSetState(E_ULayerSubState.Idle, null);
            TargetPosDisInform();
        }

        #region 路点相关逻辑
        public override void OnFreshToNextPath()
        {
            base.OnFreshToNextPath();

            if (Data != null && skillController != null)
            {
                skillController.OnClientMove();
            }
        }

        #endregion

        #region 模拟召唤物相关逻辑
        private bool HandleClientSummonAnim(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            EffectTypeClientSummonAnim effect = effectParam.BaseEffect as EffectTypeClientSummonAnim;

            HandleClientSummons(runtimeID, effect.Index, (entity) =>
            {
                entity.PlayAnimation(effect.Anim);
            });

            return true;
        }

        private bool HandleClientSummonEffect(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            ClientSummonEffect effect = effectParam.BaseEffect as ClientSummonEffect;

            if (effect.HitEffect.Count == 0)
            {
                return true;
            }

            List<FXJson> fxJsons = new List<FXJson>();
            ConverFx.LoopEffects2FxJsons(effect.HitEffect, fxJsons);

            List<FxParam> fxParams = new List<FxParam>();
            for (int i = 0; i < fxJsons.Count; i++)
            {
                FxParam fxParam = new FxParam();
                fxParam.InitWithFxJson(fxJsons[i], effectParam.Builder, effectParam.Owner);

                fxParams.Add(fxParam);
            }

            HandleClientSummons(runtimeID, effect.Index, (entity) =>
            {
                for (int i = 0; i < fxParams.Count; i++)
                {
                    entity.PlaySpecialEffect(fxParams[i], true);
                }
            });

            return true;
        }

        private bool HandleServerSummonTurnTo(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            EffectTypeClientSummonTurnTo effect = effectParam.BaseEffect as EffectTypeClientSummonTurnTo;


            string outPutKey = effectParam.OutputKey;

            // 检查是否有这个黑板数据
            bool isHasOutPutKey = EffectUtils.CheckIsBlackBoardHasKey(blackBoard, outPutKey, E_BlackBoardTag.Server);

            if (!isHasOutPutKey)
            {
                return false;
            }

            string inputKey = effectParam.InputKeys[0];
            // 如果 存在黑板数据， 那就 拿到这个黑板数据, 然后 去播这个 黑洞效果
            object serverBlackBoardNode = blackBoard.Get(inputKey, E_BlackBoardTag.Server);
            CustomBlackBoardNode customServerBlackBoardNode = (CustomBlackBoardNode)serverBlackBoardNode;

            // 拿到对应的 黑洞效果数据
            SkillTarsMsg skillTarsMsg = (SkillTarsMsg)customServerBlackBoardNode.Value;

            // 没有目标对象, 那就不看
            if (skillTarsMsg.SkillTars.Count == 0)
            {
                return true;
            }

            SkillTarData turn2Target = skillTarsMsg.SkillTars[0];
            float trunLerpTime = effect.TurnTime / 100f;
            float totalTime = effect.LookTime / 100f;

            HandleClientSummons(runtimeID, effect.Index, (entity) =>
            {
                entity.Turn2Target(turn2Target.TarID, trunLerpTime, totalTime);
            });

            return true;
        }

        private bool HandlSummonRemove(ulong runtimeID, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            EffectTypeClientSummonRemove effect = effectParam.BaseEffect as EffectTypeClientSummonRemove;

            for (int i = 0; i < effect.Index.Count; i++)
            {
                int index = effect.Index[i];

                string key = GameManager.Instance.FormateLocalEntityKey(runtimeID, index);
                // 删除 key 对应的 entity
                GameManager.Instance.RemoveFormateKey2Entity(key);
            }

            return true;
        }


        private void HandleClientSummons(ulong runtimeID, List<int> indexs, Action<EntityLocalStatic> ac)
        {
            for (int i = 0; i < indexs.Count; i++)
            {
                int index = indexs[i];

                HandleClientSummon(runtimeID, index, ac);
            }
        }

        private void HandleClientSummon(ulong runtimeID, int index, Action<EntityLocalStatic> ac)
        {
            string key = GameManager.Instance.FormateLocalEntityKey(runtimeID, index);
            ulong entityID = GameManager.Instance.GetFormateKey2EntityID(key);

            if (entityID == 0)
            {
                return;
            }

            EntityLocalStatic entity = GameManager.Instance.GetLocalEntity(entityID);
            ac.Invoke(entity);
        }

        #endregion

        #region 【NPC实体 提取 【技能】的方法和委托】

        private SkillEntity m_CurSkillEntity;

        private EndSkillStage ActionOnEndSkillStage;

        private EnergyStart ActionOnEnergyStart;

        private EnergyEnd ActionOnEnergyEnd;


        private void BindSkillAction()
        {
            if (skillDispatcher != null && skillDispatcher.SkillController != null)
            {
                skillDispatcher.SkillController.ActionOnEnergyStart += OnActionOnEnergyStart;
                skillDispatcher.SkillController.ActionOnEnergyEnd += OnActionOnEnergyEnd;
                skillDispatcher.SkillController.ActionOnEndSkillStage += OnActionOnEndSkillStage;
            }
        }

        private void ReleaseSkillAction()
        {
            if (skillDispatcher != null && skillDispatcher.SkillController != null)
            {
                skillDispatcher.SkillController.ActionOnEnergyStart -= OnActionOnEnergyStart;
                skillDispatcher.SkillController.ActionOnEnergyEnd -= OnActionOnEnergyEnd;
                skillDispatcher.SkillController.ActionOnEndSkillStage -= OnActionOnEndSkillStage;
                m_CurSkillEntity = null;
            }
        }

        private void OnActionOnEnergyStart(SkillEntity skillEntity)
        {
            m_CurSkillEntity = skillEntity;
            ActionOnEnergyStart?.Invoke(m_CurSkillEntity);
        }
        private void OnActionOnEnergyEnd(SkillEntity skillEntity)
        {
            ActionOnEnergyEnd?.Invoke(skillEntity);
            m_CurSkillEntity = null;
        }
        private void OnActionOnEndSkillStage(SkillEntity skillEntity, E_SkillExitType e_SkillExitType)
        {
            ActionOnEndSkillStage?.Invoke(skillEntity, e_SkillExitType);
        }

        public int GetCurEnergyCount(bool ignoreInitCount = false)
        {
            int count = 0;
            if (m_CurSkillEntity != null)
            {
                count = m_CurSkillEntity.GetCurEnergyCount(ignoreInitCount);
            }
            return count;
        }

        public double GetCurEnergyTime(bool ignoreInitCount = false)
        {
            double energyTime = 0;
            if (m_CurSkillEntity != null)
            {
                energyTime = m_CurSkillEntity.GetCurEnergyTime(ignoreInitCount);
            }
            return energyTime;
        }

        #region LUA 层绑定技能事件
        public void AddEnergyStartLuaAction(EnergyStart action)
        {
            ActionOnEnergyStart += action;
        }
        public void DelEnergyStartLuaAction(EnergyStart action)
        {
            ActionOnEnergyStart -= action;
        }

        public void AddEnergyEndLuaAction(EnergyEnd action)
        {
            ActionOnEnergyEnd += action;
        }
        public void DelEnergyEndLuaAction(EnergyEnd action)
        {
            ActionOnEnergyEnd -= action;
        }


        public void AddEndSkillStageLuaAction(EndSkillStage action)
        {
            ActionOnEndSkillStage += action;
        }
        public void DelEndSkillStageLuaAction(EndSkillStage action)
        {
            ActionOnEndSkillStage -= action;
        }

        #endregion

        #endregion

        #region 【NPC实体 提取 【BUFF】的方法和委托】

        private BuffCreate ActionOnBuffCreate;

        private BuffEnd ActionOnBuffEnd;

        private BuffUpdate ActionOnBuffUpdate;

        public SpectralSkillBuff ActionOnSpectralSkillBuff;

        private void BindBuffAction()
        {
            if (skillDispatcher != null && skillDispatcher.SkillController != null)
            {
                skillDispatcher.SkillController.ActionOnBuffCreate += OnActionOnBuffCreate;
                skillDispatcher.SkillController.ActionOnBuffEnd += OnActionOnBuffEnd;
                skillDispatcher.SkillController.ActionOnBuffUpdate += OnActionOnBuffUpdate;
            }
        }

        private void ReleaseBuffAction()
        {
            if (skillDispatcher != null && skillDispatcher.SkillController != null)
            {
                skillDispatcher.SkillController.ActionOnBuffCreate -= OnActionOnBuffCreate;
                skillDispatcher.SkillController.ActionOnBuffEnd -= OnActionOnBuffEnd;
                skillDispatcher.SkillController.ActionOnBuffUpdate -= OnActionOnBuffUpdate;
            }
            ActionOnSpectralSkillBuff = null;
        }

        private void OnActionOnBuffCreate(SkillBuff skillBuff, BuffCreateRet buffCreateRet)
        {
            ActionOnBuffCreate?.Invoke(skillBuff, buffCreateRet);
        }
        private void OnActionOnBuffEnd(SkillBuff skillBuff, BuffEndRet buffEndRet)
        {
            ActionOnBuffEnd?.Invoke(skillBuff, buffEndRet);
        }
        private void OnActionOnBuffUpdate(SkillBuff skillBuff)
        {
            ActionOnBuffUpdate?.Invoke(skillBuff);
        }

        public List<SkillBuff> GetSkillBuffs()
        {
            List<SkillBuff> buffLists = new();
            if (skillDispatcher != null && skillDispatcher.SkillController != null)
            {
                Dictionary<ulong, SkillBuff> buffDic = skillDispatcher.SkillController.GetSkillBuffs();
                if (buffDic != null && buffDic.Count > 0)
                {
                    foreach (var item in buffDic)
                    {
                        if (item.Value != null && !item.Value.IsReleased)
                        {
                            buffLists.Add(item.Value);
                        }
                    }
                }
            }
            return buffLists;
        }

        #region LUA 层绑定BUFF事件

        //private int ActionOnBuffCreateCount = 0;

        public void AddBuffCreateLuaAction(BuffCreate action)
        {
            ActionOnBuffCreate += action;
            //ActionOnBuffCreateCount++;
            //Debug.Log($"霸服 LUA 层绑定BUFF事件+++++++ count={ActionOnBuffCreateCount}");
        }
        public void DelBuffCreateLuaAction(BuffCreate action)
        {
            ActionOnBuffCreate -= action;
            //ActionOnBuffCreateCount--;
            //Debug.Log($"霸服 LUA 层绑定BUFF事件------ count={ActionOnBuffCreateCount}");
        }

        public void AddBuffEndLuaAction(BuffEnd action)
        {
            ActionOnBuffEnd += action;
        }
        public void DelBuffEndLuaAction(BuffEnd action)
        {
            ActionOnBuffEnd -= action;
        }

        public void AddBuffUpdateLuaAction(BuffUpdate action)
        {
            ActionOnBuffUpdate += action;
        }
        public void DelBuffUpdateLuaAction(BuffUpdate action)
        {
            ActionOnBuffUpdate -= action;
        }

        public void AddSpectralSkillLuaAction(SpectralSkillBuff action)
        {
            ActionOnSpectralSkillBuff += action;
        }
        public void DelSpectralSkillLuaAction(SpectralSkillBuff action)
        {
            ActionOnSpectralSkillBuff -= action;
        }


        #endregion

        #endregion

        #region 【NPC实体 提取 【Passive】的方法和委托】

        private PassiveCreate ActionOnPassiveCreate;

        private PassiveEnd ActionOnPassiveEnd;

        private void BindPassiveAction()
        {
            if (skillDispatcher != null && skillDispatcher.SkillController != null)
            {
                skillDispatcher.SkillController.ActionOnPassiveCreate += OnActionOnPassiveCreate;
                skillDispatcher.SkillController.ActionOnPassiveEnd += OnActionOnPassiveEnd;
            }
        }
        private void ReleasePassiveAction()
        {
            if (skillDispatcher != null && skillDispatcher.SkillController != null)
            {
                skillDispatcher.SkillController.ActionOnPassiveCreate -= OnActionOnPassiveCreate;
                skillDispatcher.SkillController.ActionOnPassiveEnd -= OnActionOnPassiveEnd;
            }
        }

        private void OnActionOnPassiveCreate(PassiveSkillEntity passiveSkillEntity, PassiveSkillUseRet passiveSkillUseRet)
        {
            ActionOnPassiveCreate?.Invoke(passiveSkillEntity, passiveSkillUseRet);
        }
        private void OnActionOnPassiveEnd(PassiveSkillEntity passiveSkillEntity, PassiveSkillEndRet passiveSkillEndRet)
        {
            ActionOnPassiveEnd?.Invoke(passiveSkillEntity, passiveSkillEndRet);
        }

        public List<PassiveSkillEntity> GetSkillPassives()
        {
            List<PassiveSkillEntity> PassiveSkillLists = new();
            if (skillDispatcher != null && skillDispatcher.SkillController != null)
            {
                Dictionary<ulong, PassiveSkillEntity> passiveSkillListsDic = skillDispatcher.SkillController.GetPassiveSkills();
                if (passiveSkillListsDic != null && passiveSkillListsDic.Count > 0)
                {
                    PassiveSkillLists = passiveSkillListsDic.Values.KToList();
                }
            }
            return PassiveSkillLists;
        }

        #region LUA 层绑定BUFF事件

        public void AddPassiveCreateLuaAction(PassiveCreate action)
        {
            ActionOnPassiveCreate += action;
            //Debug.Log($"霸服 LUA 层绑定BUFF事件+++++++ count={ActionOnBuffCreateCount}");
        }
        public void DelPassiveCreateLuaAction(PassiveCreate action)
        {
            ActionOnPassiveCreate -= action;
            //ActionOnBuffCreateCount--;
            //Debug.Log($"霸服 LUA 层绑定BUFF事件------ count={ActionOnBuffCreateCount}");
        }

        public void AddPassiveEndLuaAction(PassiveEnd action)
        {
            ActionOnPassiveEnd += action;
        }
        public void DelPassiveEndLuaAction(PassiveEnd action)
        {
            ActionOnPassiveEnd -= action;
        }

        #endregion

        #endregion


        #region 设置待机状态

        public void SetStandByState()
        {
            I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.StandBy);
            ChangeState((GameKeyCommand)E_ULayerSubState.StandBy, animParam, false);
        }

        #endregion

        #region 是否可以被选中

        public virtual void OnSelected()
        {
            if (EntityId == GameManager.Instance.mainPlayerId)
            {
                return;
            }
            if (EntityType == E_EntityType.Npc)
            {
                /*if (Data.myOwnerNttGroup != null) 策划取消点击model寻路和标记
                {
                    var ctrl = Data.myOwnerNttGroup as GameNPCCtrlGroup;
                    if (ctrl != null && ctrl.IsShow)
                    {
                        if (ctrl.M_NpcDataCell != null && ctrl.M_NpcDataCell.GetIsInteractive())
                        {
                            //位移过去
                            BusinessManager.Instance.MoveToEntity(EntityId);

                            BattleManager.Instance.SetCurAtkEntity(this);

                        }
                    }
                }*/
            }
            else
            {
                // BattleManager.Instance.SetCurAtkEntity(this);
                // 怪物被选中的时候, 切换到指定的 怪物 未索敌目标
                BattleManager.Instance.Switch2TargetEnemy(EntityId);
                BattleManager.Instance.SetShowBossPanelEntity(this);

                // 选中怪物后，重新记录一次 自动战斗的索敌中心点
                BattleManager.Instance.RecordAutoBattleSpecialStartPos(Position());
            }
        }

        #endregion

        private int defaultAvatarID = 0;
        public void CreateDefaultAvatar(int avatarID)
        {
            defaultAvatarID = avatarID;
            RefreshAvatarID(avatarID, true);
        }

        /// <summary>
        /// 刷新 avatarID
        /// </summary>
        /// <param name="avatarID"></param>
        public void RefreshAvatarID(int avatarID, bool resetModel = false)
        {
            // 刷新 模型的时候，如果之前的模型配置了 音频bank, 那就先卸载之前的 音频bank
            if (avatarDataCell != null && avatarDataCell.SoundBank != null)
            {
                SoundManager.Instance.UnLoadBank(avatarDataCell.SoundBank);
            }

            bool chanageAvatar = m_data.id != avatarID;
            // 重新 刷新新的 模型数据
            m_data.id = avatarID;
            avatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(avatarID);

            if (avatarDataCell != null)
            {
                modelDataCell = LocalDataManager.Instance.GetModelDataCell(avatarDataCell.GetModelId());
                ModleScale = avatarDataCell.GetModelScaling() / 100f;
                m_data.size = avatarDataCell.GetModelScaling();
                SoundManager.Instance.LoadBank(avatarDataCell.SoundBank);
            }
            else
            {
                SGF.Debuger.LogWarning($"{TagFlag} Create() avatarId={m_data.id},avatarDataCell=null  err!!!");
            }
            if (modelDataCell == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} Create [{EntityType}] EnityId=>{EntityId},AOIindexValue={ConfigIndex},APpearanceID={avatarID},modelDataCell=null");
            }
            else
            {
                m_data.name = modelDataCell.Desc;
            }

            if (chanageAvatar)
            {
                // 数据层切换 模型通知表现层
                ActionOnSwitchModel?.Invoke(avatarID, resetModel);
            }
        }

        /// <summary>
        /// 切换 模型 id
        /// </summary>
        /// <param name="avatarID"></param>
        public void SwitchModel(int avatarID)
        {
            // 刷新模型 数据
            RefreshAvatarID(avatarID, false);

        }

        public void RefreshAvatarEffect(AvatarChangeEffect avatarChangeEffect)
        {
            if (avatarChangeEffect == null)
            {
                SwitchModel(defaultAvatarID);
            }
            else
            {
                SwitchModel(avatarChangeEffect.EffectTypeSerialize.BUFF_AvatarChange.AvatarID);
            }
        }

        /// <summary>
        /// 隐藏的技能槽
        /// </summary>
        private List<int> hiddenSkillSlots = new();

        public List<int> GetHiddenSkillSlots()
        {
            return hiddenSkillSlots;
        }

        /// <summary>
        /// 刷新隐藏的 技能槽
        /// </summary>
        public void RefreshHiddenSkillSlotEffect(HiddenSkillSlotEffect hiddenSkillSlotEffect)
        {
            // 非主角不需要处理 隐藏技能槽位的效果
            if (!Data.isMainPlayer)
            {
                return;
            }
            // 如果没有影藏技能槽的效果了,那就把 hiddenSkillSlots 清空
            if (hiddenSkillSlotEffect == null)
            {
                hiddenSkillSlots.Clear();
            }
            else
            {
                hiddenSkillSlots.AddRange(hiddenSkillSlotEffect.EffectTypeSerialize.BUFF_SkillSlotHide.HideIndex);
            }

            ActionOnRefreshHiddenSkillSlot?.Invoke(hiddenSkillSlots);
        }


        /// <summary>
        /// 刷新技能槽位 
        /// </summary>
        public void RefreshChangeSkillSlots(ChangeSkillEffect changeSkillEffect)
        {
            // 非主角不需要处理 技能槽位的效果
            if (!Data.isMainPlayer)
            {
                return;
            }

            // 刷新 变身技能槽的数据
            if (changeSkillEffect != null)
            {
                Data.UpdateChangeSlots(true, changeSkillEffect.EffectTypeSerialize.BUFF_JobSkillChange.TalentAndSlot);
            }
            else
            {
                Data.UpdateChangeSlots(false, null);
            }
        }


        private bool hasSpectralChangeEffect = false;

        /// <summary>
        /// 是否拥有 改变量普类型的显示标签
        /// </summary>
        public bool HasSpectralChangeEffect => hasSpectralChangeEffect;

        /// <summary>
        /// 刷新 量普效果标签的 通知
        /// </summary>
        public Action<SpectralChangeEffect> ActionOnRefreshSpectralChange;

        /// <summary>
        /// 刷新量普变化的效果
        /// </summary>
        /// <param name="spectralChangeEffect"></param>
        public void RefreshSpectralChangeEffect(SpectralChangeEffect spectralChangeEffect)
        {
            // 非主角不需要处理 技能槽位的效果
            if (!Data.isMainPlayer)
            {
                return;
            }

            // 刷新 变身技能槽的数据
            ActionOnRefreshSpectralChange?.Invoke(spectralChangeEffect);
        }


        #region  半透效果
        public void OnTranslucentEffect(TranslucentEffect translucentEffect, bool onEnter)
        {
            ActionOnTranslucentEffect?.Invoke(translucentEffect, onEnter);

        }

        #endregion

        #region  创建本地召唤物逻辑
        public void CreateSimulateSummon(ulong runtimeID, string key, Vector3 pos, int avatarID, bool isFollowRotate)
        {
            ActionOnCreateSimulateSummon?.Invoke(runtimeID, key, pos, avatarID, isFollowRotate);
        }

        public void TurnSimulateSummon(ulong runtimeID, int angle, bool isFollowRotate, bool isPlay)
        {
            ActionOnTurnSimulateSummon?.Invoke(runtimeID, angle, isFollowRotate, isPlay);
        }
        #endregion


    }
}
