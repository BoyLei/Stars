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
    /// 召唤物的实体控制层，类似于playerCtrGroup
    /// </summary>
    public class SummonCtrlGroup : EntityCtrlBase
    {
        protected new string LOG_TAG = "SummonCtrlGroup";

        //======================================================================

        private SummonEntityBase m_CurrentCtrledVitBase;              //P，被控制的Enity：目前设计只有自己，skPlayer

        //目前改成主角了//Id目前是恰好对：如果三位一体，就是三个Nttid了
        public ulong currentCtrlNttId { get { return m_CurrentCtrledVitBase.EntityId; } }// { get { return m_data.m_EnityId; } }
        public override AOIEntityObject M_Curr { get { return m_CurrentCtrledVitBase; } }//逻辑层Enity

        //======================================================================

        //public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        //{
        //    LOG_TAG = $"[SummonCtrlGroup_{data.M_EntityID}]";
        //    m_data = (VitalSignData)data;
        //    base.Create(data, pos);
        //    m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<SummonEntityBase>();
        //    m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform);
        //    m_data.myOwnerNtt = m_CurrentCtrledVitBase;
        //    m_data.myOwnerNttGroup = this;

        //    //【我，其他人】放置在出生坐标
        //    SyncBornPos(pos);

        //    // 创建技能指示器
        //    if (m_data.dataType == E_EntityDataType.Summon)
        //    {
        //        // 创建头顶信息面板
        //        UnitPendant unitPendant = new(this);
        //        m_listCompoent.Add(unitPendant);
        //        m_CurrentCtrledVitBase.ActionOnMainPlayerHurt = unitPendant.OnActionMainPlayerHurt;
        //        m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;
        //        // 创建选中指示器组件
        //        CheckIndicator checkIndicator = new(this);
        //        m_listCompoent.Add(checkIndicator);
        //        m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

        //        m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;
        //    }

        //    m_data.ActionOnBattleStateDead += OnDeadDisPlay;
        //    ActionOnUpdateCusBlackBoard += OnActionUpdateCusBlackBoard;
        //}

        public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        {
            LOG_TAG = $"[SummonCtrlGroup_{data.M_EntityID}]";
            m_data = (VitalSignData)data;
            m_data.myOwnerNttGroup = this;
            m_data.ActionOnBattleStateDead += OnDeadDisPlay;

            base.Create(data, pos);

            m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<SummonEntityBase>();
            m_data.myOwnerNtt = m_CurrentCtrledVitBase;
            m_CurrentCtrledVitBase.ActionOnViewCreateFinish += OnActionOnViewCreateFinifh;
            m_CurrentCtrledVitBase.ActionOnSyncBorthPos += SyncBornPos;

            if (m_data.EntityType == E_EntityType.Summon)
            {
                m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;  // 显隐组件
            }

            m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform, OnTemplateCreateFinifh);
        }

        /// <summary> 壳子加载完成 </summary>
        protected override void OnTemplateCreateFinifh(GameObject gob)
        {
            base.OnTemplateCreateFinifh(gob);

            ActionOnUpdateCusBlackBoard += OnActionUpdateCusBlackBoard;

            // 如果是子弹就不创建这些组件
            if (m_data.EntityType == E_EntityType.Summon)
            {
                // 创建头顶信息面板
                UnitPendant unitPendant = new(this);
                m_listCompoent.Add(unitPendant);
                m_CurrentCtrledVitBase.ActionOnMainPlayerHurt = unitPendant.OnActionMainPlayerHurt;
                m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;
                // 创建选中指示器组件
                CheckIndicator checkIndicator = new(this);
                m_listCompoent.Add(checkIndicator);
                m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

                m_CurrentCtrledVitBase.Data.myOwnerNttGroup.SetModelVisiable(false);
            }
        }

        /// <summary> 加载模型完成 </summary>
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
            if (M_IsMainPlayerSummon && m_data.EntityType != E_EntityType.BulletEntity)
            {
                root = EntityRoot.Instance.DotRemoveRoot;
            }
            m_container.transform.SetParent(root.transform);
        }

        public override void Release()
        {
            m_data.ActionOnBattleStateDead -= OnDeadDisPlay;
            ActionOnUpdateCusBlackBoard -= OnActionUpdateCusBlackBoard;

            //Release Component!
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].Release();
            }
            m_listCompoent.Clear();

            //释放所有伙伴
            SummonEntityBase node = m_CurrentCtrledVitBase;
            while (node != null)
            {
                SummonEntityBase next = node.NextFriend;
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
        private void ClientGroupSync()
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

            //SGF.Debuger.Log($"{LOG_TAG}: OnBuffCreateRet  player : {buffCreateRet.OwnerEntityID} , buffId : {buffCreateRet.BuffID}");

            m_CurrentCtrledVitBase.skillDispatcher.SkillController.OnBuffCreateRet(buffCreateRet);
        }

        /// <summary>
        /// 服务器同步运行时数据,用于同步runtime数据的消息，包括skill，buff，被动技
        /// </summary>
        public override void OnRuntimeSyncRet(MessageHandleData data)
        {
            //SGF.Debuger.Log($"{LOG_TAG}: OnRuntimeSyncRet");
            RuntimeSyncRet runtimeSyncRet = (RuntimeSyncRet)data.data;

            m_CurrentCtrledVitBase.OnSkillRuntimeSyncRet(runtimeSyncRet);
        }

        /// <summary>
        /// 服务器通知的buf结束，此时应该立即干掉所有的buf效果
        /// </summary>
        public override void OnBuffEndRet(MessageHandleData data)
        {
            //SGF.Debuger.Log($"{LOG_TAG}: OnBuffEndRet");

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
        #endregion

        #region 死亡复活

        public override void OnReadyDeadNtf(MessageHandleData data)
        {
            base.OnReadyDeadNtf(data);

            M_Curr.IsReadyDead = true;
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
            //m_CurrentCtrledVitBase.skillDispatcher.Reset();
            //m_CurrentCtrledVitBase.M_IsAlive = false;
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

        #region 子弹召唤物处理逻辑
        /// <summary>
        /// 按服务器的意思，子弹的创建由AOI同步来 告知
        /// 而BulletCreateRet 通知的是子弹 运行时的创建
        /// </summary>
        /// <param name="data"></param>
        public override void OnBulletCreateRet(MessageHandleData data)
        {
            BulletCreateRet bulletCreateRet = (BulletCreateRet)data.data;
            //SGF.Debuger.Log($"{LOG_TAG} OnBulletCreateRet  id = {bulletCreateRet.RuntimeID} ");
            m_CurrentCtrledVitBase.OnBulletCreateRet(bulletCreateRet);
        }

        public override void OnBulletEndRet(MessageHandleData data)
        {
            BulletEndRet bulletEndRet = (BulletEndRet)data.data;
            m_CurrentCtrledVitBase.OnBulletEndRet(bulletEndRet);

            {
                //OwnerEntityID 就是子弹的entityID
                ulong enityId = bulletEndRet.OwnerEntityID;
                GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, enityId, null);
                gameCommand.isServerAOI = false;
                DelayInvoker.DelayInvokerOnEndOfFrame(delegate
                {
                    GameManager.Instance.EntityDataCommand(gameCommand);
                }, null);
                // SnapShotUtils.StopEffects(bulletEndRet.RuntimeID);
                //SGF.Debuger.Log($"{LOG_TAG}: OnBulletEndRet M_EntityID {enityId} ,StopEffects RuntimeID {bulletEndRet.RuntimeID}  ");
            }
        }

        #endregion

        public void OnActionUpdateCusBlackBoard(EditorModeTest.CusBlackBoardData cusBlackBoardData)
        {
            switch (cusBlackBoardData.CusBlackType)
            {
                case CusUpdateBlackType.BulletCraete:
                    {
                        // 子弹 运行时创建的黑板同步
                        m_CurrentCtrledVitBase.OnBulletCreatCusBlackBoard(cusBlackBoardData.blackBoard);
                    }
                    break;

                default: break;
            }
        }
    }
}