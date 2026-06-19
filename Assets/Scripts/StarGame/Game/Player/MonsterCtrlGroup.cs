using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Unity;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Map;
using StarProject.Game.Player.Component;
using StarProject.Service.Battle;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Player
{
    /// <summary>
    /// TODO : 小曲
    /// 怪物的实体控制层，类似于playerCtrGroup
    /// </summary>
    public class MonsterCtrlGroup : EntityCtrlBase
    {
        protected new string LOG_TAG = "MonsterCtrlGroup";

        //======================================================================

        private MonsterEntityBase m_CurrentCtrledVitBase;              //P，被控制的Enity：目前设计只有自己，skPlayer
        ///-----------------------------------------------------------------
        //private VitalSignsBase m_CurrentCtrledTeamLeader;         //队长,第0个元素[一组跟随者]，之后要拓展成3个人一起战斗 + 召唤宝宝
        //private VitalSignsBase[] m_CurrentCtrledVitBases;         //第一个人攻击，其他人都跟着攻击
        ///-----------------------------------------------------------------
        //private VitalSignsBase[] m_CurrentCtrledHead;             //头尾跟随移动[收尾跟随者]
        //private VitalSignsBase[] m_CurrentCtrledTail;

        private Transform CurCtrlPlayerFxRoot;

        //======================================================================
        //目前改成主角了//Id目前是恰好对：如果三位一体，就是三个Nttid了
        public ulong currentCtrlNttId { get { return m_CurrentCtrledVitBase.EntityId; } }// { get { return m_data.m_EnityId; } }
        public override AOIEntityObject M_Curr { get { return m_CurrentCtrledVitBase; } }//逻辑层Enity
        public int Index
        {
            get
            {
                if (Data != null && Data.Attrs != null)
                {
                    return Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.SpaceIndex);
                }

                return 0;
            }
        }
        //======================================================================

        //public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        //{
        //    LOG_TAG = $"[MonsterCtrlGroup_{data.M_EntityID}]";
        //    m_data = (VitalSignData)data;
        //    base.Create(data, pos);
        //    //先创建逻辑层
        //    //然后创建显示层
        //    //指定数据层：数据创建【任意玩家】
        //    m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<MonsterEntityBase>();
        //    m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform);
        //    m_data.myOwnerNtt = m_CurrentCtrledVitBase;
        //    m_data.myOwnerNttGroup = this;

        //    //【我，其他人】放置在出生坐标
        //    SyncBornPos(pos);

        //    //TODO：这里需要再子类中具体添加面板和组件m_CurrentCtrledVitBase
        //    // 创建头顶信息面板
        //    UnitPendant unitPendant = new UnitPendant(this);
        //    m_listCompoent.Add(unitPendant);
        //    m_CurrentCtrledVitBase.ActionOnMainPlayerHurt = unitPendant.OnActionMainPlayerHurt;
        //    m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;
        //    // 创建选中指示器组件
        //    CheckIndicator checkIndicator = new CheckIndicator(this);
        //    m_listCompoent.Add(checkIndicator);
        //    m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

        //    m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;

        //    //m_data.ActionOnBattleStateDead += OnDeadDisPlay;
        //    m_data.ActionOnBattleStateChange += OnBattleStateChange;

        //    if (!M_Curr.M_IsAlive)
        //    {
        //        OnDeadDisPlay();
        //    }

        //    OnModelCreateSuccess();
        //}

        public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        {
            LOG_TAG = $"[MonsterCtrlGroup_{data.M_EntityID}]";
            m_data = (VitalSignData)data;
            m_data.myOwnerNttGroup = this;
            m_data.ActionOnBattleStateChange += OnBattleStateChange;

            base.Create(data, pos);

            m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<MonsterEntityBase>();
            m_data.myOwnerNtt = m_CurrentCtrledVitBase;
            m_CurrentCtrledVitBase.ActionOnViewCreateFinish += OnActionOnViewCreateFinifh;
            m_CurrentCtrledVitBase.ActionOnSyncBorthPos += SyncBornPos;

            // 显隐组件
            m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;

            m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform, OnTemplateCreateFinifh);
        }

        /// <summary> 壳子加载完成 </summary>
        protected override void OnTemplateCreateFinifh(GameObject gob)
        {
            base.OnTemplateCreateFinifh(gob);

            // 创建头顶信息面板
            UnitPendant unitPendant = new(this);
            m_listCompoent.Add(unitPendant);
            //if (m_CurrentCtrledVitBase == null)
            //{
            //    SGF.Debuger.LogError("m_CurrentCtrledVitBase=null");
            //    Debug.Break();
            //}
            //if (unitPendant == null)
            //{
            //    SGF.Debuger.LogError("unitPendant=null");
            //    Debug.Break();
            //}
            m_CurrentCtrledVitBase.ActionOnMainPlayerHurt = unitPendant.OnActionMainPlayerHurt;
            m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;
            ActionPlayMvpAnim += unitPendant.SetMvpAnimShow;

            // 创建选中指示器组件
            CheckIndicator checkIndicator = new(this);
            m_listCompoent.Add(checkIndicator);
            m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

            M_Curr.M_IsAlive = !m_CurrentCtrledVitBase.Data.IsDead;
        }

        /// <summary> 加载模型完成 </summary>
        protected override void OnActionOnViewCreateFinifh()
        {
            base.OnActionOnViewCreateFinifh();

            if (!M_Curr.M_IsAlive)
            {
                OnDeadDisPlay();
            }

            SetSceneJsonRot();
        }

        private void SetSceneJsonRot()
        {
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.Monsters != null)
            {
                if (GameMap.sceneJsonData.Monsters.TryGetValue(Index, out var data) && data != null)
                {
                    if (data.ClientRot != null)
                    {
                        if (data.ClientRot.x != 0 || data.ClientRot.z != 0)
                        {
                            Animator animator = m_container.GetComponentInChildren<Animator>();
                            if (animator != null)
                            {
                                animator.transform.localRotation = Quaternion.Euler(data.ClientRot.Convert());
                            }
                        }
                    }
                }
            }
        }

        protected override void CreateMContainer(EntityBaseData data)
        {
            //创建用来显示视图的容器
            m_container = new GameObject(LOG_TAG);
            m_container.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
        }

        public override void Release()
        {
            //m_data.ActionOnBattleStateDead -= OnDeadDisPlay;
            Data.ActionOnBattleStateChange -= OnBattleStateChange;

            //Release Component!
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].Release();
            }
            m_listCompoent.Clear();

            //释放所有伙伴
            MonsterEntityBase node = m_CurrentCtrledVitBase;
            while (node != null)
            {
                MonsterEntityBase next = node.NextFriend;
                EntityFactory.ReleaseEntity(node);
                node = next;
            }
            //m_tailFriend = null;
            m_CurrentCtrledVitBase = null;
            m_data = null;
            base.Release();
            if (m_container != null)
            {
                GameObject.Destroy(m_container);
                m_container = null;
            }
        }

        public override void EnterFrame(int frameIndex)
        {
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].EnterFrame(frameIndex);
            }

            //ClientDoMove();
            ClientGroupSync();
        }

        //----------------------------------------------------------------------

        protected override void SyncBornPos(UnityEngine.Vector3 pos)
        {
            m_CurrentCtrledVitBase.MoveByServerNew(pos, true, true);
        }

        /// <summary>
        /// 客户端自己控制
        /// </summary>
        /// <param name="pos"></param>
        public void ClientGroupSync()
        {    //玩家的其他3位1体，本地ai驱动TODO
            m_CurrentCtrledVitBase.EnterFrame();

        }

        #region Buff模块
        /// <summary>
        /// 服务器创建buff
        /// </summary>
        public override void OnBuffCreateRet(MessageHandleData data)
        {
            BuffCreateRet buffCreateRet = (BuffCreateRet)data.data;

            // SGF.Debuger.Log($"{LOG_TAG}: OnBuffCreateRet  player : {buffCreateRet.OwnerEntityID} , buffId : {buffCreateRet.BuffID}");

            m_CurrentCtrledVitBase.skillDispatcher.SkillController.OnBuffCreateRet(buffCreateRet);
        }

        /// <summary>
        /// 服务器同步运行时数据,用于同步runtime数据的消息，包括skill，buff，被动技
        /// </summary>
        public override void OnRuntimeSyncRet(MessageHandleData data)
        {
            // SGF.Debuger.Log($"{LOG_TAG}: OnRuntimeSyncRet");
            RuntimeSyncRet runtimeSyncRet = (RuntimeSyncRet)data.data;

            // 技能控制器 同步
            m_CurrentCtrledVitBase.OnSkillRuntimeSyncRet(runtimeSyncRet);
        }

        /// <summary>
        /// 服务器通知的buf结束，此时应该立即干掉所有的buf效果
        /// </summary>
        public override void OnBuffEndRet(MessageHandleData data)
        {
            // SGF.Debuger.Log($"{LOG_TAG}: OnBuffEndRet");

            BuffEndRet buffEndRet = (BuffEndRet)data.data;

            m_CurrentCtrledVitBase.skillDispatcher.SkillController.OnBuffEndRet(buffEndRet);
        }

        #endregion

        #region 技能处理模块

        public override void OnSkillUseRet(MessageHandleData data)
        {
            //SGF.Debuger.Log($"{LOG_TAG}: OnSkillUseRet");
            SkillUseRet skillUseRet = (SkillUseRet)data.data;

            m_CurrentCtrledVitBase.OnUseSkill(skillUseRet);

            //if (skillUseRet != null)
            //{
            //    SGF.Debuger.LogWarning($"实体角度调试 技能id={skillUseRet.SkillID}");
            //}
            if (AppConfig.IsDev())
            {
                GMBattleInfo.Instance?.AddSkillRecord(skillUseRet.OwnerEntityID, skillUseRet.SkillID);
            }
        }
        public override void OnSkillEndRet(MessageHandleData data)
        {
            //SGF.Debuger.LogError($"{LOG_TAG}: OnSkillEndRet");
            SkillEndRet skillEndRet = (SkillEndRet)data.data;
            m_CurrentCtrledVitBase.OnEndSkill(skillEndRet);
        }

        public override void OnRunStageRet(MessageHandleData data)
        {
            RunStageRet runStageRet = (RunStageRet)data.data;
            m_CurrentCtrledVitBase.OnRunStageRet(runStageRet);
        }

        public override void OnRunStageForceEndRet(MessageHandleData data)
        {
            RunStageForceEndRet runStageForceEndRet = (RunStageForceEndRet)data.data;

            m_CurrentCtrledVitBase.OnRunStageForceEndRet(runStageForceEndRet);
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

        #region 死亡复活

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
                    OnDeadDisPlay();
                }
                else
                {
                    // 复活了
                    OnReliveDisplay();
                }
            }
        }

        /// <summary>
        /// 死亡逻辑
        /// </summary>
        private void OnDeadDisPlay()
        {
            if (BattleManager.Instance.CurAtkEntity != null && BattleManager.Instance.CurAtkEntity.EntityId == Data.M_EntityID)
            {
                // BattleManager.Instance.SetCurAtkEntity(null);
                // 索敌目标如果死亡,那就切换索敌目标
                BattleManager.Instance.ClearSerarchTargets();
            }
            if (BattleManager.Instance.ShowBossPanelEntity != null && BattleManager.Instance.ShowBossPanelEntity.EntityId == Data.M_EntityID)
            {
                BattleManager.Instance.SetShowBossPanelEntity(null);
            }
            //m_CurrentCtrledVitBase.skillDispatcher.Reset();
            //I_AnimParam animParam = m_CurrentCtrledVitBase.GetAnimParamByState(E_ULayerSubState.Deading);
            //m_CurrentCtrledVitBase.SetSubState(E_ULayerSubState.Deading, animParam, OnDelayDelete);
            //M_Curr.M_IsAlive = false;
            // 个人秘境怪物死亡模拟进度掉落物
            {
                if (GameManager.Instance.GetCurMapType() == SpaceType.SpaceSercet && m_CurrentCtrledVitBase.IsBoss == false)
                {
                    int itemid = m_CurrentCtrledVitBase.MonType == 1 ? 121 : 122;
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.LootControllerModule, "OnAddLoot", new object[] { itemid, m_CurrentCtrledVitBase.Position(), 1, GameManager.Instance.mainPlayerId, m_CurrentCtrledVitBase.ConfigIndex });
                }
            }
            {
                /// 2024/9/3
                /// fixed: 
                ///     修复怪物 自爆buff 没法显示伤害的bug:
                /// 
                /// note:
                ///    死亡的时候 可能还会继续收到 黑板数据过来, 所以 此事 skillDispatcher 的运行时不能干掉。

                // m_CurrentCtrledVitBase.skillDispatcher.Reset();

            }

            I_AnimParam animParam = m_CurrentCtrledVitBase.GetAnimParamByState(E_ULayerSubState.Deading);
            m_CurrentCtrledVitBase.SetSubState(E_ULayerSubState.Deading, animParam);
            M_Curr.M_IsAlive = false;
            if (Data.IsRobot)
            {
                //机器人可以复活,这里只播放死亡动作
            }
            else
            {
                if (m_CurrentCtrledVitBase.IsBoss && GameManager.Instance.GetMapSubType() == GameConfig.INSTANCE_PLOT_SECOND)
                {

                    OnDelayDelete();
                }
                else
                {
                    DelayInvoker.DelayInvoke(GameConfig.System_Die_Count_Down, OnDelayDelete);
                }
            }
        }

        private void OnDelayDelete(object[] args)
        {
            if (m_CurrentCtrledVitBase == null)
            {
                return;
            }

            {
                GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, currentCtrlNttId, null);
                gameCommand.isServerAOI = false;
                GameManager.Instance.EntityDataCommand(gameCommand);
            }



        }

        private void OnDelayDelete()
        {
            if (m_CurrentCtrledVitBase == null)
            {
                return;
            }
            GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, currentCtrlNttId, null);
            gameCommand.isServerAOI = false;
            GameManager.Instance.EntityDataCommand(gameCommand);
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
    }
}