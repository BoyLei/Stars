/// </summary>
///      是一个特殊的管理：（EnityDataS--EnityView）
///      【具体封装组：玩家控制的核心】一个游戏玩家的，手下控制的一队人，的这个玩家
///      例如：玩家A，玩家B，机器人A：手下控制的一队人马，这一组按照某个指令进行操作
///      本类是这一组 ：游戏名词定义：组：玩家控制组；队：下副本一堆玩家控制组：；阵营：我方，敌人，中立，或环境可摧毁物
/// </summary>

// 账号ID，角色ID，角色组（本类），每一个角色（赵云，关羽，白马，熊宝宝）*【普装，时装，变身三个状态】
// 流程：gameMGR，创建玩家组Group（本类），
// [fixUpdater]然后再创建子enity(logic)【逻辑状态机】-
// factory-
// [update(和相机同）]CreateView(:Mono——VVitalNormal)
// VVitalNormal【显示状态机--动画驱动器--动画管理--动画播放】

using ProtoMsg;
using SGF.Network;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Map;
using StarProject.Game.Player.Component;
using StarProject.Game.Skill;
using StarProject.Service.Battle;
using StarProject.Service.Cam;
using StarProjectDef;
using System;
using System.Collections.Generic;
using SGF.Unity;
using SkillEditor;
using StarProject.Service.LocalData;
using Task;
using Trigger;
using UnityEngine;
using SkillType = Task.SkillType;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.Game.Player
{
    /// <summary>
    /// 比如控制蜀国五虎将组
    /// 【任意玩家】的控制组
    /// PS：被组管理的 实体层 可以有
    /// 一个数据对应多个  ，实体，一个实体对应1个View
    ///     但是不同【类型】的View要不同的实体
    /// 为什么被组管理，实体组啊有相同功能，但是组并不需要这里自己是实体
    ///     延迟显示怎么办：延迟再创建被
    /// </summary>
    public class PlayerCtrlGroup : EntityCtrlBase
    {
        protected new string LOG_TAG = "PlayerCtrlGroup";

        //======================================================================
        private HeroEntityBase m_CurrentCtrledVitBase; //P，被控制的Enity：目前设计只有自己，skPlayer[我和比如雷家那]

        ///-----------------------------------------------------------------
        //private VitalSignsBase m_CurrentCtrledTeamLeader;         //队长,第0个元素[一组跟随者]，之后要拓展成3个人一起战斗 + 召唤宝宝
        //private VitalSignsBase[] m_CurrentCtrledVitBases;         //第一个人攻击，其他人都跟着攻击
        ///-----------------------------------------------------------------
        //private VitalSignsBase[] m_CurrentCtrledHead;             //头尾跟随移动[收尾跟随者]
        //private VitalSignsBase[] m_CurrentCtrledTail;
        ///---------------------------------------------------------------------------------
        ///只有表现层的一组：特殊做
        ///1，三位一体是伙伴，2，这个也没有逻辑层，3，这种表现肯定每个人都独立且之后通过某种方式组合
        ///所以要独立出来

        //跟随动态目标组件，例如跟随 怪，行走的npc 第三方 玩家等
        public FollowDynamicTarget FollowDynamic = new();

        private Transform CurCtrlPlayerFxRoot;

        //======================================================================
        //目前改成主角了//Id目前是恰好对：如果三位一体，就是三个Nttid了
        public ulong currentCtrlNttId
        {
            get
            {
                if (m_CurrentCtrledVitBase != null)
                {
                    return m_CurrentCtrledVitBase.EntityId;
                }
                else
                {
                    return 0;
                }
            }
        } // { get { return m_data.m_EnityId; } }

        public override AOIEntityObject M_Curr
        {
            get { return m_CurrentCtrledVitBase; }
        } //逻辑层Enity

        public bool IsPlayingSkill
        {
            get
            {
                if (m_CurrentCtrledVitBase != null)
                {
                    return m_CurrentCtrledVitBase.skillDispatcher.SkillController.HasRunningSkill();
                }

                return false;
            }
        }

        public SkillComponent g_SkillComponent = null;

        /// <summary>
        /// 伙伴的 技能槽
        /// </summary>
        private SkillComponent partner_SkillComponent = null;

        private List<MainPlayerTrigger> m_MainPlayerTrigger;
        //======================================================================

        /// <summary>
        /// Create starPlayer with the specified data and pos.
        /// </summary>
        /// <returns>void</returns>
        /// <param name="data">Data.</param>
        /// <param name="pos">Position.</param>
        //public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        //{
        //    LOG_TAG = $"[PlayerCtrlGroup_{data.M_EntityID}]";

        //    //3位1体的时候，是1份数据3个id，或者3份数据：
        //    //所以数据在这里没必要，应该分发到每个角色实体上，才对
        //    m_data = (VitalSignData)data;

        //    base.Create(data, pos);

        //    //先创建逻辑层
        //    //然后创建显示层
        //    //指定数据层：数据创建【任意玩家】
        //    m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<HeroEntityBase>();
        //    m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform);//Index是给伙伴设计的
        //    m_data.myOwnerNtt = m_CurrentCtrledVitBase;
        //    m_data.myOwnerNttGroup = this;

        //    //目前只有一个角色
        //    CurCtrlPlayerFxRoot = m_container.transform.Find("Character_Model/ModelOffset/FxRoot");

        //    //TODO：这里需要再子类中具体添加面板和组件m_CurrentCtrledVitBase
        //    //创建AI
        //    if (m_data.ai > 0)
        //    {
        //        var ai = new PCEnityAI(this);
        //        m_listCompoent.Add(ai);
        //    }
        //    // 创建技能指示器组件 异步加载完后执行
        //    if (Data.isMainPlayer)
        //    {
        //        SkillIndicator skillIndicator = new(this);
        //        m_listCompoent.Add(skillIndicator);
        //        SkillPointerDownActions = skillIndicator.OnPointerDown;
        //        SkillDirChangeActions = skillIndicator.OnSkillDirChange;
        //        SkillPointerUpActions = skillIndicator.OnPointerUp;
        //        SkillEndDragActions = skillIndicator.OnEndDrag;
        //        SkillnCancelActions = skillIndicator.OnCancel;
        //        m_data.ActionOnBattleStateDead += skillIndicator.OnDeadDisPlay;
        //    }
        //    // 创建头顶信息面板组件
        //    UnitPendant unitPendant = new(this);
        //    m_listCompoent.Add(unitPendant);
        //    m_CurrentCtrledVitBase.ActionOnMainPlayerHurt = unitPendant.OnActionMainPlayerHurt;
        //    m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;
        //    // 10V10玩法改变玩家的头顶名字颜色
        //    m_CurrentCtrledVitBase.ActionSetNameAndCor = unitPendant.OnActionSetNameAndCor;
        //    m_CurrentCtrledVitBase.ActionSetNameOrgCor = unitPendant.OnActionSetNameOrgCor;
        //    //m_CurrentCtrledVitBase.ActionOnViewDel += unitPendant.OnActionOnViewDel;
        //    //m_CurrentCtrledVitBase.ActionOnViewCreateFinifh += unitPendant.OnActionOnViewCreateFinifh;
        //    // 创建选中指示器组件
        //    CheckIndicator checkIndicator = new(this);
        //    m_listCompoent.Add(checkIndicator);
        //    m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

        //    // 显隐组件
        //    m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;

        //    OnEventListener();
        //    AddEventListener();

        //    // 判断主角是不是已经死亡了
        //    M_Curr.M_IsAlive = !m_CurrentCtrledVitBase.Data.IsDead;

        //    // 主角自己的话，绑定面板上的技能
        //    if (m_data.isMainPlayer)
        //    {
        //        // 技能组件
        //        m_CurrentCtrledVitBase.skillDispatcher.SkillUnitController.ActionOnRefreshAllSkill = UpdatePlayerSkill; // 添加技能位刷新的委托

        //        if (BattleManager.Instance.allSkillNotice != null)
        //        {
        //            OnAllSkillNotice(BattleManager.Instance.allSkillNotice);
        //        }

        //        if (!M_Curr.M_IsAlive)
        //        {
        //            // 放在异步前后：这个有风险，要试一试
        //            GlobalEvent.onMainPlayerDie.Invoke(!M_Curr.M_IsAlive);//初始化
        //        }
        //    }

        //    m_data.ActionOnBattleStateChange += OnBattleStateChange;

        //    // 异步加载完后执行
        //    if (!M_Curr.M_IsAlive)
        //    {
        //        OnBattleStateDead();
        //    }
        //    //【我，其他人】放置在出生坐标 异步加载完后执行
        //    if (pos.x != -999)//AOI的Default，不能进
        //    {
        //        SyncBornPos(pos);//客户端本地创建的，默认负数（000有用），则不进
        //    }
        //}
        public override void Create(EntityBaseData data, Vector3 pos)
        {
            LOG_TAG = $"[PlayerCtrlGroup_{data.M_EntityID}]";

            //3位1体的时候，是1份数据3个id，或者3份数据：
            //所以数据在这里没必要，应该分发到每个角色实体上，才对
            m_data = (VitalSignData)data;
            m_data.myOwnerNttGroup = this;
            m_data.ActionOnBattleStateChange += OnBattleStateChange;

            base.Create(data, pos);

            //先创建逻辑层
            //然后创建显示层
            //指定数据层：数据创建【任意玩家】
            m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<HeroEntityBase>();
            m_data.myOwnerNtt = m_CurrentCtrledVitBase;
            m_CurrentCtrledVitBase.ActionOnViewCreateFinish += OnActionOnViewCreateFinifh;
            m_CurrentCtrledVitBase.ActionOnSyncBorthPos += SyncBornPos;

            // 显隐组件
            m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;

            OnEventListener();
            AddEventListener();

            m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform, OnTemplateCreateFinifh); //Index是给伙伴设计的

            // 主角自己的话，绑定面板上的技能
            if (m_data.isMainPlayer)
            {
                // 狗曲的事件监听, 怎么能 直接用 = 号呢
                {
                    // 技能组件
                    m_CurrentCtrledVitBase.skillDispatcher.SkillUnitController.ActionOnRefreshAllSkill -=
                        UpdatePlayerSkill; // 添加技能位刷新的委托

                    m_CurrentCtrledVitBase.skillDispatcher.SkillUnitController.ActionOnRefreshAllSkill +=
                    UpdatePlayerSkill; // 添加技能位刷新的委托
                }


                if (BattleManager.Instance.allSkillNotice != null)
                {
                    OnAllSkillNotice(BattleManager.Instance.allSkillNotice);
                }
            }
        }

        /// <summary> 壳子加载完成 </summary>
        protected override void OnTemplateCreateFinifh(GameObject gob)
        {
            base.OnTemplateCreateFinifh(gob);

            // 创建头顶信息面板组件
            UnitPendant unitPendant = new(this);
            m_listCompoent.Add(unitPendant);
            m_CurrentCtrledVitBase.ActionOnMainPlayerHurt = unitPendant.OnActionMainPlayerHurt;
            m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;
            // 10V10玩法改变玩家的头顶名字颜色
            m_CurrentCtrledVitBase.ActionSetNameAndCor = unitPendant.OnActionSetNameAndCor;
            m_CurrentCtrledVitBase.ActionSetNameOrgCor = unitPendant.OnActionSetNameOrgCor;
            ActionPlayMvpAnim += unitPendant.SetMvpAnimShow;
            // 创建选中指示器组件
            CheckIndicator checkIndicator = new(this);
            m_listCompoent.Add(checkIndicator);
            m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

            // 判断主角是不是已经死亡了
            M_Curr.M_IsAlive = !m_CurrentCtrledVitBase.Data.IsDead;

            // 主角自己的话，绑定面板上的技能
            if (m_data.isMainPlayer)
            {
                if (!M_Curr.M_IsAlive)
                {
                    // 放在异步前后：这个有风险，要试一试
                    GlobalEvent.onMainPlayerDie.Invoke(!M_Curr.M_IsAlive); //初始化
                }
            }
        }

        /// <summary> 加载模型完成 </summary>
        protected override void OnActionOnViewCreateFinifh()
        {
            // 创建技能指示器组件 异步加载完后执行
            if (Data.isMainPlayer)
            {
                SkillIndicator skillIndicator = new(this);
                m_listCompoent.Add(skillIndicator);
                SkillPointerDownActions = skillIndicator.OnPointerDown;
                SkillDirChangeActions = skillIndicator.OnSkillDirChange;
                SkillPointerUpActions = skillIndicator.OnPointerUp;
                SkillEndDragActions = skillIndicator.OnEndDrag;
                SkillnCancelActions = skillIndicator.OnCancel;
                m_data.ActionOnBattleStateDead += skillIndicator.OnDeadDisPlay;
            }

            base.OnActionOnViewCreateFinifh();

            // 异步加载完后执行
            if (!M_Curr.M_IsAlive)
            {
                OnBattleStateDead();
            }
        }

        private void AddEventListener()
        {
            if (m_data.isMainPlayer)
            {
                NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PreSkillUseInputRetID, OnPreSkillUseInputRet, this);
                NetworkManager.Instance.OnMessageEnum(MsgIDEnum.SO_CDDatasID, OnSO_CDDatas, this, false);
                NetworkManager.Instance.OnMessageEnum(MsgIDEnum.RoleAllCDListNtfID, OnRoleAllCDListNtf, this, false);
            }
        }

        private void RemoveEventListenre()
        {
            if (m_data.isMainPlayer)
            {
                NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PreSkillUseInputRetID, OnPreSkillUseInputRet, this);
                NetworkManager.Instance.OffMessageEnum(MsgIDEnum.SO_CDDatasID, OnSO_CDDatas, this, false);
                NetworkManager.Instance.OffMessageEnum(MsgIDEnum.RoleAllCDListNtfID, OnRoleAllCDListNtf, this, false);
            }
        }

        private void OnBattleStateChange(E_BattleStateType curState, E_BattleStateType newState)
        {
            E_BattleStateType theCurStateIn____DeadPoint = curState & E_BattleStateType.BattleState_Dead;
            E_BattleStateType theNewerStateIn____DeadPoint = newState & E_BattleStateType.BattleState_Dead;
            if (theCurStateIn____DeadPoint != theNewerStateIn____DeadPoint)
            {
                //1 == 相同是0，不同是1 ； 说明不同 ；同时也说明现在是false;说明是normal;
                bool isServerDead = E_BattleStateType.BattleState_Dead == theNewerStateIn____DeadPoint;

                if (isServerDead)
                {
                    // 死亡
                    if (!m_data.isMainPlayer)
                    {
                        OnBattleStateDead();
                    }
                }
                else
                {
                    // 复活了
                    OnReliveDisplay();
                }

                if (m_data.isMainPlayer)
                {
                    GlobalEvent.onMainPlayerDie.Invoke(isServerDead); //RPC=PB=正常同步死亡逻辑
                }
            }
        }

        //gm:创建模型
        public void AddPlayerView()
        {
            m_CurrentCtrledVitBase?.AddMainPlayerView(m_container.transform, m_CurrentCtrledVitBase.Position());
        }

        public void AddPlayerViewNew(UnityEngine.Vector3 pos)
        {
            m_CurrentCtrledVitBase?.AddMainPlayerView(m_container.transform, pos);
        }

        public void ClearPlayerView()
        {
            m_CurrentCtrledVitBase?.ClearNpc();
        }

        private void OnEventListener()
        {
            if (m_data.isMainPlayer)
            {
                GlobalEvent.OnAllSkillNoticeEvent.AddListener(OnAllSkillNotice);
                GlobalEvent.OnSwitchSkillPosRetEvent.AddListener(OnSwitchSkillPosRet);
                GlobalEvent.OnSwitchTalentRetEvent.AddListener(OnSwitchTalentRet);


                /// 伙伴的协议是 系统层协议,不走rpc
                /// ==========================================
                //GlobalEvent.OnPartnerUpdate.AddListener(UpdatePartnerSkill);
                //GlobalEvent.OnPartnerLeave.AddListener(RomvePartnerSkill);
                /// ===========================================
            }

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                GlobalEvent.OnLocalServerEvent.AddListener(OnLocalServerEvent);
                return;
            }
#endif
        }

        private void OffEventListener()
        {
            if (m_data.isMainPlayer)
            {
                GlobalEvent.OnAllSkillNoticeEvent.RemoveListener(OnAllSkillNotice);
                GlobalEvent.OnSwitchSkillPosRetEvent.RemoveListener(OnSwitchSkillPosRet);
                GlobalEvent.OnSwitchTalentRetEvent.RemoveListener(OnSwitchTalentRet);

                //GlobalEvent.OnPartnerUpdate.RemoveListener(UpdatePartnerSkill);
                //GlobalEvent.OnPartnerLeave.RemoveListener(RomvePartnerSkill);
            }

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                GlobalEvent.OnLocalServerEvent.RemoveListener(OnLocalServerEvent);
                return;
            }
#endif
        }

        private void UpdatePlayerSkill()
        {
            // 技能组件
            if (g_SkillComponent == null)
            {
                g_SkillComponent = new SkillComponent();
                g_SkillComponent.Init(this);
                m_data.ActionOnBattleStateDead += g_SkillComponent.OnDeadDisPlay;
            }
            else
            {
                if (GameInput.Instance != null)
                {
                    g_SkillComponent.OnActionOnRefreshAllSkill();
                    //SGF.Debuger.Log($"{LOG_TAG} 创建刷新技能按钮 gameinput不存在，先添加委托");
                    //GameInput.GameInputFinishAction += g_SkillComponent.OnActionOnRefreshAllSkill;
                }
            }
        }

        protected override void CreateMContainer(EntityBaseData data)
        {
            //创建用来显示视图的容器
            m_container = new GameObject(LOG_TAG);
            GameObject root = EntityRoot.Instance.RemoveRoot;
            if (Data.isMainPlayer)
            {
                root = EntityRoot.Instance.DotRemoveRoot;
            }

            m_container.transform.SetParent(root.transform);
        }

        public override void EnterFrame(int frameIndex)
        {
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].EnterFrame(frameIndex);
            }

            g_SkillComponent?.EnterFrame(frameIndex);
            //partner_SkillComponent?.EnterFrame(frameIndex);

            ClientDoMove(); //我处理逻辑限定在这里30fps，不论摇杆变更多频繁不影响我
            ClientGroupSync();
            FollowDynamic.OnEnterFrame(frameIndex);
            if (Data != null && Data.isMainPlayer)
            {
                // 技能切遥感位移的倒计时
                if (M_MainPlayerChangeMoveForceTime > 0)
                {
                    M_MainPlayerChangeMoveForceTime -= Time.fixedDeltaTime;
                    //SGF.Debuger.Log($"{LOG_TAG} 强制时间 EnterFrame nowTime={TimeUtils.ServerNowStampMilli}ms,m_ForceMoveTime={m_ForceMoveTime}");
                }

                if (!Data.M_is_JoySitckMoving && M_MainPlayerChangeMoveForceChatTime > 0)
                {
                    M_MainPlayerChangeMoveForceChatTime -= Time.fixedDeltaTime;
                }
            }

            if (m_MainPlayerTrigger != null)
            {
                foreach (var trigger in m_MainPlayerTrigger)
                {
                    trigger.EnterFrame(frameIndex);
                }
            }
        }

        /// <summary>
        /// Release this instance.
        /// </summary>
        public override void Release()
        {
            RemoveEventListenre();

            // 清空寻路状态
            cur_FindPathType = E_FindPathType.None;

            Data.ActionOnBattleStateChange -= OnBattleStateChange;

            //Release Component!
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].Release();
            }

            m_listCompoent.Clear();

            g_SkillComponent?.Release();
            //partner_SkillComponent?.Release();
            //释放所有伙伴
            HeroEntityBase node = m_CurrentCtrledVitBase;
            while (node != null)
            {
                HeroEntityBase next = node.NextFriend;
                EntityFactory.ReleaseEntity(node);
                node = next;
            }

            //m_tailFriend = null;
            OffEventListener();

            m_CurrentCtrledVitBase = null;
            //curBattlePartnerEttBase = null;

            base.Release();
            if (m_container != null)
            {
                GameObject.Destroy(m_container);
                m_container = null;
            }
        }

        //----------------------------------------------------------------------

        /// <summary>
        ///  切换地图
        /// </summary>
        /// <param name="pos">坐标</param>
        /// <param name="rot">角度</param>
        public void EnterSpace(UnityEngine.Vector3 pos, int rot)
        {
            //由于延迟处理的镜像副本要保持效果连续
            // 切换地图强拉
            ForceSyncPos(pos);
            ForceSyncRot(rot);
            CreateMainRoleTrigger();
        }

        /// <summary>
        /// 离开上一个地图时，重置一些逻辑
        /// </summary>
        public void LeaveSpace()
        {
            // 切地图重启技能
            m_CurrentCtrledVitBase.skillDispatcher.Reset();
            //寻路时，同步坐标，本质是镜像触发的End点
            //主角常驻寻路需要清理，坐标/场景/同步，在场景切换时[不同副本/镜像副本]时要处理，清理寻路信息
            BreakFindPath();
            BreakFollowDynamicEnity();
            DestroyMainRoleTrigger();
        }

        private void CreateMainRoleTrigger()
        {
            if (m_MainPlayerTrigger == null)
            {
                m_MainPlayerTrigger = new List<MainPlayerTrigger>();
            }

            m_MainPlayerTrigger.Clear();
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.MainPlayer != null)
            {
                var triggerGroups = GameMap.sceneJsonData.MainPlayer.TriggerGroups;
                if (triggerGroups != null && triggerGroups.Count > 0)
                {
                    foreach (var item in triggerGroups)
                    {
                        if (GameMap.sceneJsonData.Triggers.TryGetValue(item.TriggerID, out TriggerJsonData jsonData) &&
                            jsonData != null)
                        {
                            foreach (var effect in item.Effects)
                            {
                                var trigger = new MainPlayerTrigger(jsonData, effect);
                                m_MainPlayerTrigger.Add(trigger);
                            }
                        }
                    }
                }
            }
        }

        private void DestroyMainRoleTrigger()
        {
            if (m_MainPlayerTrigger != null)
            {
                m_MainPlayerTrigger.Clear();
            }
        }

        /// <summary> 强制同步角度（服务器角度） </summary>
        public override void ForceSyncRot(int rot)
        {
            base.ForceSyncRot(rot);
            // SGF.Debuger.LogError($"[Rotate] EnterSpace，强同步坐标: {rot}");
            m_CurrentCtrledVitBase.ServerSetRotation(rot, false, true, 0);
        }

        /// <summary> 强制同步坐标 </summary>
        public override void ForceSyncPos(UnityEngine.Vector3 pos)
        {
            base.ForceSyncPos(pos);
            // m_CurrentCtrledVitBase.MoveByServer(pos, true);
            m_CurrentCtrledVitBase.MoveByServerNew(pos, true, false);
        }

        protected override void SyncBornPos(UnityEngine.Vector3 pos)
        {
            m_CurrentCtrledVitBase.MoveByServerNew(pos, true, true);
        }

        /// <summary>
        /// 群体控制
        /// </summary>
        /// <param name="changeVector3">fix帧的变化【向】量</param>
        /// <param name="changeVector3">fix帧 玩家操作杆的 方向向量</param>
        private void ClientGroupMoveTo(UnityEngine.Vector3 targetVector3, UnityEngine.Vector3 joyMoveDirction)
        {
            //玩家的其他3位1体，本地ai驱动TODO
            //个体控制
            m_CurrentCtrledVitBase.ClientSetMoveByFixFrame(targetVector3, joyMoveDirction);
        }

        /// <summary>
        /// 客户端自己控制
        /// </summary>
        /// <param name="pos"></param>
        private void ClientGroupSync()
        {
            //玩家的其他3位1体，本地ai驱动TODO
            m_CurrentCtrledVitBase.EnterFrame();
        }

        /// <summary>
        /// 在人身上创建特效
        /// </summary>
        /// <param name="data"></param>
        public void OnFxCreateRet(MessageHandleData data)
        {
            //特效一般就是挂载通常不移动
            m_CurrentCtrledVitBase.AddFxTriggerForPersion(EnumEnityListKey.Env_EntityFx, CurCtrlPlayerFxRoot);
        }

        public override bool CheckCanSkillBtn(SkillContainer skillContainer)
        {
            //if (Data != null && Data.isMainPlayer)
            //{
            //    if (M_MainPlayerChangeMoveForceTime > 0)
            //    {
            //        SGF.Debuger.Log($"{LOG_TAG} skillid={skillContainer.CurSkillId},m_MainPlayerChangeMoveForceTime>0 result=false");
            //        return false;
            //    }
            //}

            // 修改判断技能槽能否使用技能的接口， 跟自动战斗 的判定接口统一
            if (!skillContainer.CheckCanUse())
            {
                SGF.Debuger.LogWarning(
                    $"{LOG_TAG} 检查是否可以使用技能 skillid={skillContainer.CurSkillId},IsCanUserSkill=false");
                return false;
            }

            return true;
        }


        #region Move

        private bool moveLen_____IsZero = true;

        private bool DoVKey_Move(int vkey, float args)
        {
            //SGF.Debuger.LogError($"物理按键 DoVKey_Move vkey={vkey},args={args}");
            //寻路中 按键盘移动则终止寻路
            BreakFindPath();
            BreakFollowDynamicEnity();
            //客户端预演交互则终止交互
            if (m_CurrentCtrledVitBase.IsClientPrepareInter && !IsLockInter)
            {
                InterFail(0);
            }
            switch (vkey)
            {
                case GameVKey.MoveX_CMD:
                    {
                        M_InputMoveDirection.x = args;
                    }
                    break;
                case GameVKey.MoveZ_CMD:
                    {
                        M_InputMoveDirection.z = args;
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }

        public void InputVKey(int vkey, float arg)
        {
            //SGF.Debuger.Log(vkey + "：" + arg);
            bool hasHandled = false;
            hasHandled = hasHandled || DoVKey_Move(vkey, arg);
        }

        /// <summary>
        /// 主动打断制造
        /// </summary>
        public override void BreakCreate()
        {
            m_CurrentCtrledVitBase?.BreakCreate();
        }

        public void OnBeginCreating(int job, string animation)
        {
            m_CurrentCtrledVitBase?.OnBeginCreating(job, animation);
        }

        public void OnEndCreating()
        {
            m_CurrentCtrledVitBase?.OnEndCreating();
        }

        public bool IsCreating()
        {
            return m_CurrentCtrledVitBase.IsCreating();
        }

        /// <summary>
        /// 主动打断交互
        /// </summary>
        public override void BreakInteract()
        {
            SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            BreakInterReq interEeq = new();
            battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, interEeq);
            //GlobalEvent.OnStopCreate.Invoke(1);
        }

        /// <summary>
        /// 主动打断寻路
        /// </summary>
        public override void BreakFindPath()
        {
            if (m_CurrentCtrledVitBase != null && m_CurrentCtrledVitBase.Is_MainPlayer_FindingPath)
            {
                m_CurrentCtrledVitBase.BreakFindPath();
                // 如果仅打断寻路，动画切不过去会卡在原地踏步
                //var stage = m_CurrentCtrledVitBase.GetForceDefaultState();
                //m_CurrentCtrledVitBase.ForceSetState(stage, null);
                // 这里还是要判断下优先级
                I_AnimParam animParam = m_CurrentCtrledVitBase.GetAnimParamByState(E_ULayerSubState.Idle);
                m_CurrentCtrledVitBase.ChangeState((GameKeyCommand)E_ULayerSubState.Idle, animParam, false);
            }
        }

        public bool FollowDynamicEnity(string key, ulong enityid, float range = 1,
            System.Action<bool, string> action = null, bool tips = true)
        {
            Action<bool, string> callBack = (result, stringKey) =>
            {
                var findPathType = cur_FindPathType;
                cur_FindPathType = E_FindPathType.None;

                action?.Invoke(result, stringKey);
            };
            if (FollowDynamic != null)
            {
                return FollowDynamic.OnStart(key, enityid, range, callBack, tips);
            }

            return false;
        }

        public void BreakFollowDynamicEnity()
        {
            if (FollowDynamic != null)
            {
                cur_FindPathType = E_FindPathType.None;
                FollowDynamic.Break();
            }
        }
        public E_FindPathType Cur_FindPathType => cur_FindPathType;
        private E_FindPathType cur_FindPathType = E_FindPathType.None;

        /// <summary>
        /// 当前是否在寻路状态
        /// </summary>
        public bool Is_FindPathing => Cur_FindPathType != E_FindPathType.None;

        public bool ClientNavFindPath(UnityEngine.Vector3 position, float maxDistance = 1,
            System.Action<bool> CallBack = null, bool isChanageFindPath = false, bool recordOnStateForbid = false)
        {
            return TryFindPath(position, maxDistance, CallBack, isChanageFindPath, recordOnStateForbid, E_FindPathType.Normal);
        }

        /// <summary>
        /// 尝试寻路的接口
        /// </summary>
        /// <param name="position"></param>
        /// <param name="maxDistance"></param>
        /// <param name="ac"> 由 寻路类型1 ----> 寻路类型2 的回调 </param>
        /// <param name="isChanageFindPath"></param>
        /// <param name="recordOnStateForbid"></param>
        /// <param name="findPathType"></param>
        /// <returns></returns>
        public bool TryFindPath(UnityEngine.Vector3 position, float maxDistance = 1, System.Action<bool> ac = null, bool isChanageFindPath = false, bool recordOnStateForbid = false, E_FindPathType findPathType = E_FindPathType.Normal)
        {

            BreakFindPath();
            BreakFollowDynamicEnity();


            // 先设置状态, 那么 上一个寻路的 回调中， 就可以知道是 由什么类型的寻路打断
            cur_FindPathType = findPathType;

            Action<bool> callBack = (result) =>
            {
                SGF.Debuger.Log($"[FindPath] player TryFindPath result: {result}, 寻路类型: {cur_FindPathType}  ---> end");
                cur_FindPathType = E_FindPathType.None;

                ac?.Invoke(result);
            };

            if (m_CurrentCtrledVitBase != null)
            {
                return m_CurrentCtrledVitBase.ClientNavFindPath(position, maxDistance, callBack, isChanageFindPath, recordOnStateForbid);
            }

            return false;
        }

        private bool CheckNeedRecordOnFindPathFinish(bool result)
        {
            if (Data == null)
            {
                return false;
            }
            if (!Data.isMainPlayer)
            {
                return false;
            }
            if (!result)
            {
                return false;
            }

            // 不是自动战斗的 都要返回 true, 执行修复逻辑
            if (cur_FindPathType == E_FindPathType.None || cur_FindPathType == E_FindPathType.Normal || cur_FindPathType == E_FindPathType.Follow)
            {
                return true;
            }

            return false;
        }

        public bool IsIntering()
        {
            ulong value = m_CurrentCtrledVitBase.CurInteractID;
            ulong uid = Convert.ToUInt64(value);
            if (uid > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsLockInter { get; set; }

        public void PrepareInteract(ulong entityID)
        {
            IsLockInter = false;
            var entity = (ObjectCtrlGroup)GameManager.Instance.GetEntityCtr(entityID);
            if (entity != null)
            {
                InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(entity.ConfigID);
                if (interactDataCell != null)
                {

                    m_CurrentCtrledVitBase.PlayInteractAnimation(interactDataCell);
                    m_CurrentCtrledVitBase.PlayInteractProcessBar(entityID, interactDataCell);
                    if (!interactDataCell.GetCanBreak())
                    {
                        SetInterLock(true);
                    }
                }
            }

            m_CurrentCtrledVitBase.IsClientPrepareInter = true;
        }


        public void SetInterLock(bool locked)
        {
            if (IsLockInter != locked)
            {
                IsLockInter = locked;
                if (IsLockInter)
                {
                    GameManager.Instance.RegisterMainPlayerClientBattleStates();
                }
                else
                {
                    GameManager.Instance.UnRegisterMainPlayerClientBattleStates();
                }
            }
        }

        /// <summary>
        /// 触发实体交互失败事件并将当前控制的虚拟对象状态更改为空闲
        /// </summary>
        /// <param name="entityID">要交互的实体的 ID</param>
        public void InterFail(ulong entityID)
        {
            m_CurrentCtrledVitBase.IsClientPrepareInter = false;
            if (GlobalEvent.InterEvent != null)
            {
                GlobalEvent.InterEvent.Invoke(entityID, 0);
            }
            //交互结束
            m_CurrentCtrledVitBase.ForceSetState(E_ULayerSubState.Idle, null);
            if (IsLockInter)
            {
                SetInterLock(false);
            }

        }


        /// <summary>
        /// Update检测
        /// Fix里面去处理
        /// 客户端逻辑
        /// </summary>
        /// 
        /// <summary> 【主角自己】技能切遥感位移的【强制时间（毫秒）】 </summary>
        public float m_MainPlayerChangeMoveForceChatTime = 0;

        public float M_MainPlayerChangeMoveForceChatTime
        {
            get { return m_MainPlayerChangeMoveForceChatTime; }
            set
            {
                if (m_MainPlayerChangeMoveForceChatTime <= 0 && value > 0)
                {
                    // HUD聊天 - 隐藏
                    GlobalEvent.OnChatHudShowEvent?.Invoke(false);
                }
                else if (Data != null && Data.M_is_JoySitckMoving == false && value <= 0)
                {
                    // HUD聊天 - 显示
                    GlobalEvent.OnChatHudShowEvent?.Invoke(true);
                }

                m_MainPlayerChangeMoveForceChatTime = value;
            }
        }

        public override bool GetChatShow()
        {
            return M_MainPlayerChangeMoveForceChatTime > 0;
        }

        private void ClientDoMove()
        {
            MoveDirection = M_InputMoveDirection;

            if (MoveDirection.magnitude > 0)
            {
                Vector3 VirtualCameraForward = CameraManager.Instance.GetPlayerCameraAnglesY();
                MoveDirection = Quaternion.Euler(0, VirtualCameraForward.y, 0) * MoveDirection;

                //脚步可以放在动画代码附近，循环播放===
                //脚步不要考虑别人的，1，脚步根据地面材质横向拓展，2，大家都跑声音多奇怪，3，脚步是第三层环境层
                //wwise曲!!!!这里加脚步声
                //SoundManager.Instance.PlayEvent(AudioMethodEvent.FOOT_STEP, false);//Demo没做别人的，怪物的，那需要池化，3D声音//Y都是0

                //主角刚出生不应该走这里
                if (moveLen_____IsZero == true)
                {
                    //长度不足，就直接暂停，且不重复控制
                    moveLen_____IsZero = StopTouchJoyStick(false);

                    // 当左侧方向摇杆处于未操作状态5秒后，左下角的区域会显示聊天内容
                    M_MainPlayerChangeMoveForceChatTime = GameConfig.System_ChangeMoveForceTimeChat;
                }

                // 【主角自己】客户端技能按钮每次点击的间隔
                if (M_MainPlayerChangeMoveForceTime <= 0 && !M_UserSkillTag)
                {
                    M_MainPlayerChangeMoveForceTime = GameConfig.System_ChangeMoveForceTime;
                    M_UserSkillTag = true;
                    //SGF.Debuger.Log($"{LOG_TAG} 强制时间 ClientDoMove nowTime={TimeUtils.ServerNowStampMilli}ms,m_ForceMoveTime={m_ForceMoveTime}");
                }

                //逻辑层逻辑帧处理，显示层逻辑帧处理；如果用DG的话改变的话会一直打破DG性能损耗；这个是摇杆为了细化操作所以不用DG；推送频率也是按照帧处理而不能用秒；
                UnityEngine.Vector3 temp = m_CurrentCtrledVitBase.Position() + (MoveDirection.normalized *
                    m_CurrentCtrledVitBase.Speed / GameConfig.FIX_TIME_PER_SEC);

                ClientGroupMoveTo(temp, MoveDirection.normalized);
            }
            else //==0
            {
                //主角刚出生不应该走这里
                if (moveLen_____IsZero == false)
                {
                    //长度不足，就直接暂停，且不重复控制
                    moveLen_____IsZero = StopTouchJoyStick(true);

                    // 当左侧方向摇杆处于未操作状态5秒后，左下角的区域会显示聊天内容
                    M_MainPlayerChangeMoveForceChatTime = GameConfig.System_ChangeMoveForceTimeChat;

                    m_CurrentCtrledVitBase.OnJoyStickMove(UnityEngine.Vector3.zero);
                }

                // 技能切遥感位移的【强制时间（毫秒）】
                if (M_MainPlayerChangeMoveForceTime > 0)
                {
                    M_MainPlayerChangeMoveForceTime = 0;
                }
            }
        }

        private bool StopTouchJoyStick(bool needStop)
        {
            // M_is_JoySitckMoving 不判断原子锁
            Data.M_is_JoySitckMoving = !needStop;
            // 主角的遥感是否可以移动是在原子锁判断结束后赋值
            if (needStop)
            {
                m_CurrentCtrledVitBase.Is___JoySitckStop = needStop; //手柄控制
            }

            return needStop;
        }

        #endregion

        #region 对应的RPC消息处理函数模块

        #region Buff模块

        /// <summary>
        /// 服务器创建buff
        /// </summary>
        public override void OnBuffCreateRet(MessageHandleData data)
        {
            BuffCreateRet buffCreateRet = (BuffCreateRet)data.data;

            //SGF.Debuger.LogError($"霸服霸服 {LOG_TAG}: OnBuffCreateRet  player : {buffCreateRet.OwnerEntityID} , buffId : {buffCreateRet.BuffID}");

            m_CurrentCtrledVitBase.skillDispatcher.SkillController.OnBuffCreateRet(buffCreateRet);
        }

        /// <summary>
        /// 服务器同步运行时数据,用于同步runtime数据的消息，包括skill，buff，被动技
        /// </summary>
        public override void OnRuntimeSyncRet(MessageHandleData data)
        {
            //SGF.Debuger.Log($"{LOG_TAG}: OnRuntimeSyncRet");
            RuntimeSyncRet runtimeSyncRet = (RuntimeSyncRet)data.data;

            // 技能控制器 同步
            m_CurrentCtrledVitBase.OnSkillRuntimeSyncRet(runtimeSyncRet);
        }

        public override void OnRunStageRet(MessageHandleData data)
        {
            //SGF.Debuger.Log($"{LOG_TAG}: OnRunStageRet");
            RunStageRet runStageRet = (RunStageRet)data.data;

            m_CurrentCtrledVitBase.OnRunStageRet(runStageRet);
        }

        public override void OnRunStageForceEndRet(MessageHandleData data)
        {
            RunStageForceEndRet runStageForceEndRet = (RunStageForceEndRet)data.data;

            m_CurrentCtrledVitBase.OnRunStageForceEndRet(runStageForceEndRet);
        }

        void OnPreSkillUseInputRet(MessageHandleData data)
        {
            PreSkillUseInputRet preSkillUseInputRet = (PreSkillUseInputRet)data.data;

            m_CurrentCtrledVitBase.OnPreSkillUseInputRet(preSkillUseInputRet);
        }

        /// <summary>
        /// 服务器通知的buf结束，此时应该立即干掉所有的buf效果
        /// </summary>
        public override void OnBuffEndRet(MessageHandleData data)
        {
            BuffEndRet buffEndRet = (BuffEndRet)data.data;

            //SGF.Debuger.LogError($"霸服霸服 {LOG_TAG}: OnBuffEndRet  player : {buffEndRet.OwnerEntityID} , buffId : {buffEndRet.BuffID}");

            m_CurrentCtrledVitBase.skillDispatcher.SkillController.OnBuffEndRet(buffEndRet);
        }

        public override void OnServerSetPosNTF(MessageHandleData data)
        {
            ProtoMsg.ServerSetPosNTF serverSetPosMsg = (ProtoMsg.ServerSetPosNTF)data.data;
            UnityEngine.Vector3 v3 = UnityEngine.Vector3.zero;
            v3.x = serverSetPosMsg.Pos.X;
            v3.y = serverSetPosMsg.Pos.Y;
            v3.z = serverSetPosMsg.Pos.Z;
            ForceSyncPos(v3);
        }

        public override void OnReadyDeadNtf(MessageHandleData data)
        {
            base.OnReadyDeadNtf(data);

            M_Curr.IsReadyDead = true;
        }

        #endregion

        #region 技能相关逻辑

        public void OnAllSkillNotice(AllSkillNotice allSkillNotice)
        {
            m_CurrentCtrledVitBase.OnAllSkillNotice(allSkillNotice);
        }

        public void OnSwitchSkillPosRet(SwitchSkillPosRet switchSkillPosRet)
        {
            m_CurrentCtrledVitBase.OnSwitchSkillPosRet(switchSkillPosRet);
        }

        public void OnSwitchTalentRet(SwitchTalentRet switchTalentRet)
        {
            //TODO: dl
            m_CurrentCtrledVitBase.OnSwitchTalentRet(switchTalentRet);
        }

        /// <summary>
        /// rpc 的 技能cd 同步
        /// </summary>
        /// <param name="data"></param>
        public override void OnCDUpdateNotice(MessageHandleData data)
        {
            CDUpdateNotice cDUpdateNotice = (CDUpdateNotice)data.data;
            //SGF.Debuger.LogError($"{LOG_TAG}: CDUpdateNotice");
            m_CurrentCtrledVitBase.OnCDUpdateNotice(cDUpdateNotice);
            // 技能CD改版，主角的
            if (m_CurrentCtrledVitBase.Data.isMainPlayer)
            {
                /*bool isMySkill =
                    m_CurrentCtrledVitBase.skillDispatcher.SkillUnitController.CheckIsMySkillById(cDUpdateNotice.CDInfo
                        .JobSkillID);*/
                //if (!isMySkill && curBattlePartnerEttBase != null)
                //{
                //    // note:
                //    // 夏哥墙裂，俩个伙伴技能id【绝对不可能公用一个技能id】
                //    // 如果不是主角的技能，说明是伙伴的技能CD信息-》发送给伙伴
                //    curBattlePartnerEttBase.OnCDUpdateNotice(cDUpdateNotice);
                //}

                var config = LocalDataManager.Instance.GetSkillConfig(cDUpdateNotice.CDInfo.BattleSkillID);

                if (config == null)
                {
                    return;
                }

                var isPartnerSkill = config.SkillTag == SkillTag.PetSkill;

                if (isPartnerSkill)
                {
                    GlobalEvent.OnPartnerCDUpdateNotice?.Invoke(cDUpdateNotice.CDInfo.BattleSkillID);
                }
            }
        }

        public void OnSO_CDDatas(MessageHandleData data)
        {
            SO_CDDatas so_CDDatas = (SO_CDDatas)data.data;
            m_CurrentCtrledVitBase.OnAllSkillCDNotice(so_CDDatas.CDList);
        }

        public void OnRoleAllCDListNtf(MessageHandleData data)
        {
            RoleAllCDListNtf roleAllCDListNtf = (RoleAllCDListNtf)data.data;
            // SGF.Debuger.Log($"[CD_Refresh]: OnRoleAllCDListNtf: {roleAllCDListNtf}");
            m_CurrentCtrledVitBase.OnRoleAllCDListNtf(roleAllCDListNtf.List);
        }

        #endregion

        #region 技能处理模块

        public override void OnSkillUseRet(MessageHandleData data)
        {
            //SGF.Debuger.Log($"{LOG_TAG}: OnSkillUseRet time=");
            SkillUseRet skillUseRet = (SkillUseRet)data.data;

            m_CurrentCtrledVitBase.OnUseSkill(skillUseRet);


            if (AppConfig.IsDev())
            {
                GMBattleInfo.Instance?.AddSkillRecord(skillUseRet.OwnerEntityID, skillUseRet.SkillID);
            }
        }

        public override void OnSkillEndRet(MessageHandleData data)
        {
            SkillEndRet skillEndRet = (SkillEndRet)data.data;

            m_CurrentCtrledVitBase.OnEndSkill(skillEndRet);
        }

        #endregion

        #region 主角【死亡】

        public override void OnChangeDeadState(MessageHandleData data)
        {
            if (data == null)
            {
                //SGF.Debuger.LogError($"OpenWaitToBornWindow  OnChangeDeadState 000 currentCtrlNttId={currentCtrlNttId} data is null");
                return;
            }

            //SGF.Debuger.LogWarning($"OpenWaitToBornWindow  OnChangeDeadState 111111111111111 currentCtrlNttId={currentCtrlNttId} data.enityId{data.enityId}");
            ChangeDeadState change = (ChangeDeadState)data.data;
            if (change != null)
            {
                if (data.enityId == currentCtrlNttId)
                {
                    M_Curr.M_IsAlive = !change.State;
                    if (change.State)
                    {
                        OnDeadDisPlay();
                    }
                    else
                    {
                        UnityEngine.Vector3 v3 = UnityEngine.Vector3.zero;
                        v3.x = change.Pos.X;
                        v3.y = change.Pos.Y;
                        v3.z = change.Pos.Z;
                        ForceSyncPos(v3); //重生拉取
                        //OnReliveDisplay();    // 通过原子判断合理
                        HandleOnRevive();
                    }
                    //SGF.Debuger.LogError($"OpenWaitToBornWindow  OnChangeDeadState IsDead={change.State}");
                }
            }
            else
            {
                SGF.Debuger.LogWarning($"OpenWaitToBornWindow  OnChangeDeadState change is null");
            }
        }

        ////【主角】复活 暂时不用了，协议保留
        //public void OnRoleReviveRet(MessageHandleData data)
        //{
        //    RoleReviveRet roleReviveRetMsg = (RoleReviveRet)data.data;
        //    if (roleReviveRetMsg.RetValue == 0)
        //    {
        //        // 成功
        //        UnityEngine.Vector3 v3 = UnityEngine.Vector3.zero;
        //        v3.x = roleReviveRetMsg.Pos.X;
        //        v3.y = roleReviveRetMsg.Pos.Y;
        //        v3.z = roleReviveRetMsg.Pos.Z;
        //        ForceSyncPos(v3); //【主角】复活
        //        OnReliveDisplay();
        //        GlobalEvent.onMainPlayerDie.Invoke(false);//不用了=RPC=PB=（是如何复活的消息-只是暂时不用）
        //        SGF.Debuger.Log($"{LOG_TAG} OnRoleReviveRet succeed");
        //    }
        //    else
        //    {
        //        SGF.Debuger.LogWarning($"{LOG_TAG} OnRoleReviveRet failure");
        //    }
        //}

        /// <summary>
        /// 死亡逻辑
        /// note:
        ///     只有主角死亡的时候，才会走 aoi 通知的 死亡逻辑， 进入下面的 接口
        /// </summary>
        private void OnDeadDisPlay()
        {
            // BattleManager.Instance.SetCurAtkEntity(null);
            // 索敌目标如果死亡,那就清空索敌队列
            BattleManager.Instance.ClearSerarchTargets();
            //m_CurrentCtrledVitBase.skillDispatcher.Reset();
            OnBattleStateDead();

            // 死亡的时候清空 当前的寻路状态
            cur_FindPathType = E_FindPathType.None;
        }

        private void OnBattleStateDead()
        {
            I_AnimParam animParam = m_CurrentCtrledVitBase.GetAnimParamByState(E_ULayerSubState.Deading);
            m_CurrentCtrledVitBase.SetSubState(E_ULayerSubState.Deading, animParam);
            m_CurrentCtrledVitBase.M_EntityMoveDir = UnityEngine.Vector3.zero;

            m_CurrentCtrledVitBase.M_IsAlive = false;
        }

        /// <summary>
        /// 复活逻辑
        /// </summary>
        private void OnReliveDisplay()
        {
            m_CurrentCtrledVitBase.M_IsAlive = true;
            m_CurrentCtrledVitBase.ReliveDisplay();
            //SGF.Debuger.LogError($"OpenWaitToBornWindow  OnReliveDisplay");
        }

        #endregion

        #endregion

        #region 被动技能

        /// <summary>
        /// 被动技能创建
        /// </summary>
        /// <param name="passiveSkillUseRet"></param>
        public override void OnPassiveSkillUseRet(MessageHandleData data)
        {
            PassiveSkillUseRet passiveSkillUseRet = (PassiveSkillUseRet)data.data;
            //SGF.Debuger.LogError($"{LOG_TAG}  OnPassiveSkillUseRet  runtimeID {passiveSkillUseRet.RuntimeID} , skillID {passiveSkillUseRet.SkillID} ");
            m_CurrentCtrledVitBase.OnPassiveSkillUseRet(passiveSkillUseRet);
        }

        /// <summary>
        /// 被动技能莫得了
        /// 
        /// </summary>
        /// <param name="passiveSkillEndRet"></param>
        public override void OnPassiveSkillEndRet(MessageHandleData data)
        {
            PassiveSkillEndRet passiveSkillEndRet = (PassiveSkillEndRet)data.data;

            //SGF.Debuger.LogError($"{LOG_TAG}  OnPassiveSkillEndRet  runtimeID {passiveSkillEndRet.RuntimeID} , skillID {passiveSkillEndRet.SkillID} ");
            m_CurrentCtrledVitBase.OnPassiveSkillEndRet(passiveSkillEndRet);
        }

        #endregion

        #region 伙伴相关协议

        //private PartnerEntityBase curBattlePartnerEttBase = null; // 缓存当前伙伴的控制器

        /// <summary>
        /// 刷新伙伴技能槽 的技能
        /// </summary>
        /// <param name="partnerCtrlGroup"></param>
        private void UpdatePartnerSkill(EntityCtrlBase entityCtrlBase, int starNum)
        {
            //if (partner_SkillComponent == null)
            //{
            //    partner_SkillComponent = new SkillComponent();
            //}
            //curBattlePartnerEttBase = entityCtrlBase.M_Curr as PartnerEntityBase;

            //// 得到配置后,对于伙伴 单独创建伙伴自己的技能位
            //int partnerId = (int)curBattlePartnerEttBase.ConfigIndex;
            //{
            //    ParSkillDataCell parSkillDataCell = LocalDataManager.Instance.GetParSkillDataCell(partnerId, starNum);
            //    if (parSkillDataCell != null)
            //    {
            //        List<int> partnerSkills = new List<int>() { parSkillDataCell.GetParSkill() };
            //        KeyValuePair<int, List<int>> posId2Skills = new KeyValuePair<int, List<int>>(GameConfig.SKILL_PARTNER_POS_ID, partnerSkills);
            //        List<KeyValuePair<int, List<int>>> partnerSkillLists = new List<KeyValuePair<int, List<int>>> { posId2Skills };

            //        curBattlePartnerEttBase.skillDispatcher.SkillUnitController.CreateWithSkillLists(partnerSkillLists);
            //    }
            //}

            //// 伙伴技能创建的时候，问一下主角的技能控制器里面的CD，目前是多少了
            //foreach (KeyValuePair<int, SkillContainer> item in curBattlePartnerEttBase.GetSkillContainerDic())
            //{
            //    SkillContainer skillContainer = item.Value;
            //    if (!skillContainer.isPadding)
            //    {
            //        SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnRefreshAllSkill skillBtn.SkillContainer.isPadding=false ");
            //        continue;
            //    }

            //    int skillId = skillContainer.CurSkillId;
            //    SkillContainer mainPlayerSkillContainer = m_CurrentCtrledVitBase.skillDispatcher.SkillUnitController.GetSkillUnit(skillId);
            //    if (mainPlayerSkillContainer != null)
            //    {
            //        skillContainer.SetSkillCD(mainPlayerSkillContainer.LeastCD);
            //    }
            //}

            //partner_SkillComponent.Init(entityCtrlBase, this);
        }

        private void RomvePartnerSkill(EntityCtrlBase entityCtrlBase)
        {
            // 如果伙伴下战了，判断删除的伙伴是不是自己
            //if (curBattlePartnerEttBase != null)
            //{
            //    if (curBattlePartnerEttBase.EntityId == entityCtrlBase.Data.M_EntityID)
            //    {
            //        if (partner_SkillComponent != null)
            //        {
            //            partner_SkillComponent.RomoveSkill(entityCtrlBase);
            //        }
            //        curBattlePartnerEttBase = null;
            //    }
            //}
        }

        #endregion

        #region 可删除的

        ///// <summary>
        ///// 1,先加载;2，再控制显示隐藏
        ///// ;//这里可以控制主角的隐藏，伙伴隐藏，以及影子隐藏，接入就隐藏呗
        ///// </summary>
        //public void ControlShowHide(bool isShow)
        //{
        //    m_CurrentCtrledVitBase.ControlShowHide(isShow);
        //}

        //public bool TryEatFood(EntityObject entity)
        //{
        //    //这里应该有一个公式来决定吃一个Food生成多少个Node
        //    if (HitTest(entity, HitDistance))
        //    {
        //        AddNodes(m_data.viewEnityData.size / m_data.viewEnityData.keyStep);
        //        return true;
        //    }
        //    return false;
        //}

        //public bool TryHitEnemies(List<EntityCtrlBase> entityCtrlBase)
        //{
        //    //for (int j = 0; j < entityCtrlBase.Count; j++)
        //    //{
        //    //    EntityCtrlBase other = entityCtrlBase[j];
        //    //    bool isTargetNtt = EntityFactoryUtils.CheckIsTriggleAOIEntity(other.M_Curr, currentCtrlNttId, M_Curr.Faction);
        //    //    if (!isTargetNtt)
        //    //    {
        //    //        continue;
        //    //    }

        //    //    if (HitTest(other, HitDistance))
        //    //    {
        //    //        Blast();
        //    //        return true;
        //    //    }
        //    //}

        //    return false;
        //}


        ///// <summary>
        ///// 地图边界
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public bool TryHitBound(GameContext context)
        //{
        //    Rect rect = new Rect(0, 0, context.mapSize.x, context.mapSize.y);
        //    if (!rect.Contains(m_CurrentCtrledVitBase.Position()))
        //    {
        //        Blast();
        //        return true;
        //    }
        //    return false;
        //}

        //public bool HitTest(EntityCtrlBase entityCtrlBase, float testDistance)
        //{
        //    HeroEntityBase node = (HeroEntityBase)entityCtrlBase.M_Curr;
        //    while (node != null)
        //    {
        //        //if (node.IsKeyNode())//关键碰撞者
        //        {
        //            float distance = UnityEngine.Vector3.Distance(m_CurrentCtrledVitBase.Position(), node.Position());
        //            if (distance < testDistance)
        //            {
        //                return true;
        //            }
        //        }

        //        node = node.NextFriend;
        //    }

        //    return false;
        //}

        //public bool HitTest(EntityObject entity, float testDistance)
        //{
        //    float distance = UnityEngine.Vector3.Distance(m_CurrentCtrledVitBase.Position(), entity.Position());
        //    if (distance < testDistance)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// 死亡爆装备
        ///// </summary>
        //private void Blast()
        //{
        //    HeroEntityBase node = m_CurrentCtrledVitBase;
        //    while (node != null)
        //    {
        //        //if (node.IsKeyNode())
        //        {
        //            UnityEngine.Vector3 pos = GetRandomPosition(node.Position(), 8);

        //            node.Blast();

        //            if (GameManager.Instance.Context.random.Rnd() > 0.5)
        //            {
        //                //GameManager.Instance.CreateItem(pos, m_data.teamId);
        //            }
        //        }

        //        node = node.NextFriend;
        //    }
        //}

        //private UnityEngine.Vector3 GetRandomPosition(UnityEngine.Vector3 center, int r)
        //{
        //    int dx = m_context.random.Range(-r, r);
        //    int dy = m_context.random.Range(-r, r);
        //    center.x += dx;
        //    center.y += dy;
        //    return center;

        //}

        #endregion

        public SkillDispatcher GetSkillDispather()
        {
            return m_CurrentCtrledVitBase.skillDispatcher;
        }


        #region GM命令

        public void ChanageJob(uint JobID)
        {
            m_CurrentCtrledVitBase.JobChange(JobID);
        }

        #endregion

#if UNITY_EDITOR
        public void GetSkillIds(ref List<int> ids)
        {
            foreach (KeyValuePair<int, SkillContainer> item in m_CurrentCtrledVitBase.skillDispatcher
                         .SkillUnitController.SkillContainerDic)
            {
                foreach (SkillInfo info in item.Value.ShowSkillInfos)
                {
                    ids.Add(info.cfg.ID);
                }
            }
        }

        public void RefreshSkill()
        {
            g_SkillComponent?.RefreshSkill();
        }

        public void OnLocalServerEvent(LocalServerEventRsp localServerEvent, object data)
        {
            switch (localServerEvent)
            {
                case LocalServerEventRsp.SwitchMonster:
                    {
                        int avatarID = EditorModeTest.EditorMode.Instance.localServer.AvatarID;
                        (M_Curr as NPCEntityBase).SwitchModel(avatarID);
                        var allSkillNotice = EditorModeTest.EditorMode.Instance.localServer.CurAllSkillNotice;
                        GlobalEvent.OnRefreshAllSkillNoticeEvent.Invoke(allSkillNotice);
                    }
                    break;
                default: break;
            }
        }

#endif
    }
}