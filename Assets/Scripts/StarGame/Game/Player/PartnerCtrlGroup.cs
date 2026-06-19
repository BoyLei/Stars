using ProtoMsg;
using SGF.Network;
using SGF.Unity;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player.Component;
using StarProject.Service.Battle;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Player
{
    /// <summary>
    /// 伙伴的实体控制层，类似于playerCtrGroup
    /// 目前每个怪物 还是跟 主角 / 怪物 的设计类似.
    /// 而对于 playerCtrlGroup 拥有 的 伙伴, 通过 在 playerCtrlGroup 中 持有 runtimeID的方式间接持有.
    /// 这样,对于每个 伙伴 而言,它都是 独立的 由 服务器AOI 控制的 唯一实体.
    /// </summary>
    public class PartnerCtrlGroup : EntityCtrlBase
    {
        protected new string LOG_TAG = "PartnerCtrlGroup";

        //======================================================================

        private PartnerEntityBase m_CurrentCtrledVitBase;              //P，被控制的Enity：目前设计只有自己，skPlayer
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

        //======================================================================


        //public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        //{
        //    LOG_TAG = $"[PartnerCtrlGroup_{data.M_EntityID}]";
        //    m_data = (VitalSignData)data;
        //    base.Create(data, pos);
        //    //先创建逻辑层
        //    //然后创建显示层
        //    //指定数据层：数据创建【任意玩家】
        //    m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<PartnerEntityBase>();
        //    m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform);
        //    m_data.myOwnerNtt = m_CurrentCtrledVitBase;
        //    m_data.myOwnerNttGroup = this;

        //    //【我，其他人】放置在出生坐标
        //    SyncBornPos(pos);

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

        //    CreateSkillComponent();

        //    m_data.ActionOnBattleStateDead += OnDeadDisPlay;

        //    if (m_CurrentCtrledVitBase.M_IsMainPlayerSummon)
        //    {
        //        GlobalEvent.OnLeaveSpace.AddListener(OnLeaveSpace);
        //    }
        //}

        public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        {
            LOG_TAG = $"[PartnerCtrlGroup_{data.M_EntityID}]";
            m_data = (VitalSignData)data;
            m_data.myOwnerNttGroup = this;
            m_data.ActionOnBattleStateDead += OnDeadDisPlay;

            base.Create(data, pos);

            m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<PartnerEntityBase>();
            m_data.myOwnerNtt = m_CurrentCtrledVitBase;
            m_CurrentCtrledVitBase.ActionOnViewCreateFinish += OnActionOnViewCreateFinifh;
            m_CurrentCtrledVitBase.ActionOnSyncBorthPos += SyncBornPos;

            // 显隐组件
            m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;

            m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform, OnTemplateCreateFinifh);
        }

        /// <summary>
        /// 壳子加载完成
        /// </summary>
        protected override void OnTemplateCreateFinifh(GameObject gob)
        {
            base.OnTemplateCreateFinifh(gob);

            // 创建头顶信息面板
            UnitPendant unitPendant = new(this);
            m_listCompoent.Add(unitPendant);
            m_CurrentCtrledVitBase.ActionOnMainPlayerHurt = unitPendant.OnActionMainPlayerHurt;
            m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;
            // 创建选中指示器组件
            CheckIndicator checkIndicator = new(this);
            m_listCompoent.Add(checkIndicator);
            m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

            CreateSkillComponent();

            M_Curr.M_IsAlive = !m_CurrentCtrledVitBase.Data.IsDead;
            if (m_CurrentCtrledVitBase.M_IsMainPlayerSummon)
            {
                GlobalEvent.OnLeaveSpace.AddListener(OnLeaveSpace);
            }
        }

        /// <summary>
        /// 加载模型完成
        /// </summary>
        protected override void OnActionOnViewCreateFinifh()
        {
            base.OnActionOnViewCreateFinifh();

            if (!M_Curr.M_IsAlive)
            {
                OnDeadDisPlay();
            }
        }

        protected override void CreateMContainer(EntityBaseData data)
        {
            //创建用来显示视图的容器
            m_container = new GameObject(LOG_TAG);
            GameObject root = EntityRoot.Instance.RemoveRoot;
            ulong SummonHostID = m_data.Attrs.GetAoiValue<ulong>(EnumAOIType.Ulong, AOIAttrDefine.SummonHostID);
            bool M_IsMainPlayerSummon = GameManager.Instance.mainPlayerId == SummonHostID;
            if (M_IsMainPlayerSummon)
            {
                root = EntityRoot.Instance.DotRemoveRoot;
            }
            m_container.transform.SetParent(root.transform);
        }

        /// <summary>
        ///  创建技能相关的组件{先不删除}
        /// </summary>
        private void CreateSkillComponent()
        {
            // TODO : 曲
            /// 伙伴的技能按钮组件
            /// note:
            ///     曲的按钮(SkillComponent) 跟 EntityCtrlGroup 耦合的太死.
            ///     而对于 伙伴的 技能按钮而言, 玩家在create的时候, 伙伴的AOI实体并不一定创建。
            ///     而伙伴的技能 按钮, 是在 playerCtrlGroup 中创建, 由多个伙伴共同刷新.
            ///     好的做法, 是将 技能按钮 跟 EntityCtrlGroup 分离 , 里面只需要关系 ui的显示 和 点击相关逻辑;
            ///     对于 伙伴技能 这种 1 对 N 的 形式, 提供 update 刷新接口即可
            if (m_CurrentCtrledVitBase.M_IsMainPlayerSummon)
            {
                // 目前 先在 伙伴创建的时候，将自己通知给 主角(没办法, 技能按钮现在依赖EntityCtrlGroup, 所以现在只能先绕一下)
                // 等后面 曲 要是能够 把技能按钮 改成 只通过 数据(技能list) 刷新显示, 那本地就不需要这么写
                GlobalEvent.OnPartnerCreate.Invoke(this);
            }
        }

        // {先不删除}
        private void RomveSkillComponent()
        {
            if (m_CurrentCtrledVitBase.M_IsMainPlayerSummon)
            {
                // 目前 先在 伙伴创建的时候，将自己通知给 主角(没办法, 技能按钮现在依赖EntityCtrlGroup, 所以现在只能先绕一下)
                // 等后面 曲 要是能够 把技能按钮 改成 只通过 数据(技能list) 刷新显示, 那本地就不需要这么写
                GlobalEvent.OnPartnerLeave.Invoke(this);
            }
        }

        public uint GetCfgId()
        {
            return m_CurrentCtrledVitBase.ConfigIndex;
        }

        public override void Release()
        {
            m_data.ActionOnBattleStateDead -= OnDeadDisPlay;
            RomveSkillComponent();
            if (m_CurrentCtrledVitBase.M_IsMainPlayerSummon)
            {
                GlobalEvent.OnLeaveSpace.RemoveListener(OnLeaveSpace);
            }
            //Release Component!
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].Release();
            }
            m_listCompoent.Clear();

            //释放所有伙伴
            PartnerEntityBase node = m_CurrentCtrledVitBase;
            while (node != null)
            {
                PartnerEntityBase next = node.NextFriend;
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

        /// <summary>
        /// 离开上一个地图时，重置一些逻辑
        /// </summary>
        private void OnLeaveSpace(bool res)
        {
            // 切地图重启技能
            m_CurrentCtrledVitBase.skillDispatcher.Reset();
        }

        protected override void SyncBornPos(UnityEngine.Vector3 pos)
        {
            m_CurrentCtrledVitBase.MoveByServerNew(pos, true, true);
        }

        //----------------------------------------------------------------------

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

            SGF.Debuger.Log($"{LOG_TAG}: OnBuffCreateRet  player : {buffCreateRet.OwnerEntityID} , buffId : {buffCreateRet.BuffID}");

            m_CurrentCtrledVitBase.skillDispatcher.SkillController.OnBuffCreateRet(buffCreateRet);
        }

        /// <summary>
        /// 服务器同步运行时数据,用于同步runtime数据的消息，包括skill，buff，被动技
        /// </summary>
        public override void OnRuntimeSyncRet(MessageHandleData data)
        {
            SGF.Debuger.Log($"{LOG_TAG}: OnRuntimeSyncRet");
            RuntimeSyncRet runtimeSyncRet = (RuntimeSyncRet)data.data;

            // 技能控制器 同步
            m_CurrentCtrledVitBase.OnSkillRuntimeSyncRet(runtimeSyncRet);
        }

        /// <summary>
        /// 服务器通知的buf结束，此时应该立即干掉所有的buf效果
        /// </summary>
        public override void OnBuffEndRet(MessageHandleData data)
        {
            SGF.Debuger.Log($"{LOG_TAG}: OnBuffEndRet");

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
            //m_CurrentCtrledVitBase.skillDispatcher.Reset();
            //M_Curr.M_IsAlive = false;
            //I_AnimParam animParam = m_CurrentCtrledVitBase.GetAnimParamByState(E_ULayerSubState.Deading);
            //m_CurrentCtrledVitBase.SetSubState(E_ULayerSubState.Deading, animParam, OnDelayDelete);
            m_CurrentCtrledVitBase.skillDispatcher.Reset();
            I_AnimParam animParam = m_CurrentCtrledVitBase.GetAnimParamByState(E_ULayerSubState.Deading);
            m_CurrentCtrledVitBase.SetSubState(E_ULayerSubState.Deading, animParam);
            M_Curr.M_IsAlive = false;
            DelayInvoker.DelayInvoke(GameConfig.System_Die_Count_Down, OnDelayDelete);
        }

        private void OnDelayDelete(object[] args)
        {
            if (m_CurrentCtrledVitBase == null)
            {
                return;
            }
            GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, currentCtrlNttId, null);
            gameCommand.isServerAOI = false;
            GameManager.Instance.EntityDataCommand(gameCommand);
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

        public override void OnCDUpdateNotice(MessageHandleData data)
        {
            CDUpdateNotice cDUpdateNotice = (CDUpdateNotice)data.data;
            // GlobalEvent.OnPartnerCDUpdateNotice.Invoke(cDUpdateNotice);
            //SGF.Debuger.LogError($"{LOG_TAG}: CDUpdateNotice");

            m_CurrentCtrledVitBase.OnCDUpdateNotice(cDUpdateNotice);
        }


    }
}