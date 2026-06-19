using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProjectDef;
using System;
using UnityEngine;

namespace StarProject.Game.Entity.VitalSigns
{
    public class PartnerEntityBase : NPCEntityBase
    {
        private string TagFlag => $"[{EntityId}] [PartnerEntityBase]";

        //==================================================================
        protected new PartnerEntityBase m_nextMyControl;       //（不包含我的召唤物），【我的伙伴，我的熊宝宝】

        internal PartnerEntityBase NextFriend { get { return m_nextMyControl; } }

        //-----------------------------------------------------------------------

        /// <summary>
        /// 创建逻辑数据必要的，需要玩家数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="playerData"></param>
        /// <param name="container">角色们找个通用的角色挂点就行了</param>
        //public void Create(int index, VitalSignData playerData, Transform container)
        //{
        //    base.Init(playerData.M_EntityID, playerData.EntityType);
        //    m_playerData = playerData;
        //    m_entityBaseWalkSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_WALK_SPEED;
        //    m_entityBaseRunSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_RUN_SPEED;
        //    M_IsMainPlayerSummon = GameManager.Instance.mainPlayerId == SummonHostID;
        //    m_playerData.ActionOnBattleStateChange += OnBattleStateChange;
        //    m_data = playerData.viewEnityData;
        //    m_index = index;
        //    //每个Player都将创建一个独立的SkillController
        //    CreateSkillDispatcher(playerData, container);

        //    ///就算没有资源，我也可以分池的设定，有View的Enity，被Enity驱动View
        //    ///资源划分：1,细胞【【人：多职业，变身_S：时装_T】【怪物：敌人：Boss_B：伙伴，宝宝】】 :{其他职业，怪物，都用id区分} 
        //    ///这里是生命体征基础：一定是管理细胞加载
        //    ///规划任意角色都具备（变身，和时装）
        //    ///优先级如下，1默认基础状态，2有时装显示时装，3有变身显示变身（因为没有项目组给变身在做一套时装那么情怀（LangFeiQian））
        //    // 加载模型
        //    PartnerDataCell partnerDataCell = LocalDataManager.Instance.GetPartnerDataCell((int)ConfigIndex);
        //    ModelCfg = partnerDataCell;
        //    if (partnerDataCell == null)
        //    {
        //        SGF.Debuger.LogError($"{TagFlag} Create PartnerEntityId=>{EntityId},AOIindexValue={ConfigIndex},partnerDataCell=null");
        //        return;
        //    }

        //    M_Name = partnerDataCell.Name;
        //    m_data.size = 1;
        //    CreateDefaultAvatar(partnerDataCell.GetAvatarID());

        //    ViewFactory.CreateViewAddressables("Roles/Template/Partner_Model2", "Roles/Template/Partner_Model2", this, container);
        //    InitRegisterAttribute();
        //    InformDeadChange();
        //}

        public void Create(int index, VitalSignData playerData, Transform container, Action<GameObject> cb)
        {
            base.Init(playerData.M_EntityID, playerData.EntityType);
            m_playerData = playerData;
            m_entityBaseWalkSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_WALK_SPEED;
            m_entityBaseRunSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_RUN_SPEED;
            M_IsMainPlayerSummon = GameManager.Instance.mainPlayerId == SummonHostID;
            m_playerData.ActionOnBattleStateChange += OnBattleStateChange;
            m_data = playerData.viewEnityData;
            m_index = index;
            //每个Player都将创建一个独立的SkillController
            CreateSkillDispatcher(playerData, container);

            PartnerDataCell partnerDataCell = LocalDataManager.Instance.GetPartnerDataCell(ConfigIndex);
            ModelCfg = partnerDataCell;
            if (partnerDataCell == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} Create PartnerEntityId=>{EntityId},AOIindexValue={ConfigIndex},partnerDataCell=null");
                return;
            }

            M_Name = partnerDataCell.Name;
            CreateDefaultAvatar(partnerDataCell.GetAvatarID());

            InitRegisterAttribute();
            InformDeadChange();

            // 先同步一次 服务器出生点坐标
            ActionOnSyncBorthPos?.Invoke();

            ViewFactory.CreateViewAsync("Roles/Template/Partner_Model2", this, container, cb);
        }

        protected override void Release()
        {
            base.Release();
        }

        protected override void Reset()
        {
            isBorn = true;
            base.Reset();
        }

        internal override void EnterFrame()
        {
            base.EnterFrame();
        }


        /// <summary>
        /// 1,先暂停pos发送，校验。
        /// 2,技能停止特殊移动。
        /// 3,技能攻击引发的位移*
        /// 弧形可由路点处理拓展endpos[]
        /// </summary>
        protected override void SkillViewSmooth(Vector3 endPos, float durningTime, MoveLabel moveLabel, MoveType moveType, string key, Action<bool> action)
        {
            DoSkillPathMove?.Invoke(endPos, durningTime, moveLabel, moveType, key, action);
        }

        /// <summary>
        /// 服务器来消息，控制其他玩家
        /// 出生控制【全部人员】；非出生控制【只能非主角】
        /// 怪[组数据]，和主角[自己算]，会强拉。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isComeFromServer">服务器设置，听服务器的，那是客户端自己算的</param>
        internal override void MoveByServer(Vector3 pos, bool isBornOrForceSet)
        {
            float lenF = 0;
            if (CurrentPos.z == pos.z && CurrentPos.x == pos.x && isBornOrForceSet == false)
            {
            }
            else
            {
                if (isBornOrForceSet)
                {
                    M_EntityMoveDir = pos - CurrentPos;
                    ActionOnBirthPos?.Invoke(pos);
                    // 伙伴增加 创建的时候播放出场特效
                    {
                        FxParam fxParam = new();
                        fxParam.InitWithLogic("Fx_Parter_change_common");
                        ActionOnPlaySpecialEffects?.Invoke(fxParam, "Debut", "Effects/Roles/cm");
                    }
                }
                else
                {
                    //朝向：Dir就是x，z没有Y
                    Vector3 len = pos - CurrentPos;
                    len.y = 0;
                    lenF = len.magnitude;
                    if (lenF >= GetForceSyncValue())
                    {
                        //自己缓存的向量，算Y的
                        M_EntityMoveDir = pos - CurrentPos;
                        DoForceMove?.Invoke(pos);
                    }
                    else
                    {
                        //SGF.Debuger.Log($"伙伴坐标 没有触发强拉 CurrentPos={CurrentPos},pos={pos},lenF={lenF},force={GetForceSyncValue()}");
                    }
                }
            }
        }

        internal override void MoveByServerNew(Vector3 pos, bool ServerForce, bool isBorn)
        {
            float lenF = 0;
            if (CurrentPos.z == pos.z && CurrentPos.x == pos.x && (ServerForce && isBorn) == false)
            {
            }
            else
            {
                if (isBorn)
                {
                    M_EntityMoveDir = pos - CurrentPos;
                    ActionOnBirthPos?.Invoke(pos);
                    // 伙伴增加 创建的时候播放出场特效
                    {
                        FxParam fxParam = new();
                        fxParam.InitWithLogic("Fx_Parter_change_common");
                        ActionOnPlaySpecialEffects?.Invoke(fxParam, "Debut", "Effects/Roles/cm");
                    }
                }
                else if (ServerForce)
                {
                    //朝向：Dir就是x，z没有Y
                    Vector3 len = pos - CurrentPos;
                    len.y = 0;
                    lenF = len.magnitude;
                    if (lenF >= GetForceSyncValue())
                    {
                        //自己缓存的向量，算Y的
                        M_EntityMoveDir = pos - CurrentPos;
                        DoForceMove?.Invoke(pos);
                    }
                    else
                    {
                        //SGF.Debuger.Log($"伙伴坐标 没有触发强拉 CurrentPos={CurrentPos},pos={pos},lenF={lenF},force={GetForceSyncValue()}");
                    }
                }
                ServerPosition = pos;
            }
        }

        #region 原子锁改变

        protected override void OnBattleStateChange(E_BattleStateType curState, E_BattleStateType newState)
        {
            base.OnBattleStateChange(curState, newState);

            if (M_IsMainPlayerSummon)
            {
                //检测再对应位是否有变化需求
                //电路设计：我要知道新数据，枚举中其他位为0，我检测位为1
                //关键位置我是1你是0就是0我要知你，其他位置我是0你是什么都排除
                //采用或运算
                E_BattleStateType theCurStateIn____BattlePoint = curState | E_BattleStateType.BattleState_Dead;
                //其他位必然是0，关键位用对方的
                E_BattleStateType theNewerStateIn____BattlePoint = newState | E_BattleStateType.BattleState_Dead;
                // ^ 检测不同
                if (theCurStateIn____BattlePoint != theNewerStateIn____BattlePoint)
                {
                    InformDeadChange();
                }
            }
        }

        // 伙伴死亡状态通知
        private void InformDeadChange()
        {
            GlobalEvent.OnPartnerDeadUpdate?.Invoke(EntityId, Data.IsDead);
        }

        #endregion

        #region 小地图通知

        public override void OnFinalPosChange(UnityEngine.Vector3 newVector3)
        {
            //底层之间直接发送，不用action
            //Swm.OnMonsterMove(Data.M_EntityID,newVector3);
            base.OnFinalPosChange(newVector3);
        }

        #endregion

        #region 属性同步

        private long m_TruthHp;

        private long m_CurHp;

        private bool isBorn = true;

        protected override void InitRegisterAttribute()
        {
            base.InitRegisterAttribute();

            Data.RegisterAttribute(AOIAttrDefine.TruthHp, OnAOITruthHpChange);
            Data.RegisterAttribute(AOIAttrDefine.curHp, OnAOIHpChange);
        }


        protected override void OnAOIPositionChange(string key, object val)
        {
            base.OnAOIPositionChange(key, val);
            // 伙伴创建成功通知HUD
            if (isBorn && M_IsMainPlayerSummon)
            {
                GlobalEvent.OnPartnerEntityCreateFinish?.Invoke(EntityId);
            }
            isBorn = false;
        }

        private void OnAOITruthHpChange(string key, object val)
        {
            m_TruthHp = m_playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
            InformHpChange();
        }

        private void OnAOIHpChange(string key, object val)
        {
            m_CurHp = m_playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
            InformHpChange();
        }

        private void InformHpChange()
        {
            if (M_IsMainPlayerSummon)
            {
                GlobalEvent.OnPartnerBloodUpdate?.Invoke(ConfigIndex, m_CurHp, m_TruthHp); ;
            }
        }

        #endregion

        #region 技能、BUFF、被动效果执行

        protected override void OnActionOnSkillPlayAnim(E_ULayerSubState stage, I_AnimParam i_AnimParam)
        {
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillPlayAnim PartnerEntityId=>{EnityId},stage={stage},i_AnimParam={i_AnimParam}");
            ForceSetState(stage, i_AnimParam);
        }

        protected override void OnActionOnSkillAnimation(E_ULayerSubState stage)
        {
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillAnimation PartnerEntityId=>{EnityId},stage={stage}");
            ForceSetState(stage, null);
        }

        protected override void OnActionOnSkillPlayFx(I_FxParam i_FxParam)
        {
            ulong buildId = i_FxParam.BuilderID;
            //ulong ownerId = i_FxParam.OwnerID;
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillPlayFx PartnerEntityId=>{EnityId},i_FxParam={i_FxParam}");
            AvatarDataCell avatarData = avatarDataCell;
            if (buildId != Data.M_EntityID)
            {
                AvatarDataCell model = GameManager.Instance.GetEntityAvatarById(buildId);
                if (model != null)
                {
                    avatarData = model;
                }
            }
            if (avatarData == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} OnActionOnSkillPlayFx PartnerEntityId=>{EntityId},i_FxParam={i_FxParam},avatarData=null");
                return;
            }
            ActionOnPlaySpecialEffects?.Invoke(i_FxParam, "", avatarData.EffectsPath);
        }

        protected override void OnActionOnSkillStopFx(I_FxParam i_FxParam)
        {
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillStopFx PartnerEntityId=>{EnityId},i_FxParam={i_FxParam}");
            ActionOnStopSpecialEffects?.Invoke(i_FxParam, "");
        }

        protected override void OnActionOnBulletOffectY(float offectY)
        {
            ActionOnViewOffectY?.Invoke(offectY);
        }

        #endregion

        #region 特效模块：人资深的触发器：特效只是一种表现挂载方式
        internal void AddFxTriggerForPersion(EnumEnityListKey entityFx, Transform curCtrlPlayerFxRoot)
        {
            EntityRemoteStatic fx = EntityFactory.InstanceEntity<EntityRemoteStatic>();
            fxList = GetEntityList(entityFx);
            fxList.Add(fx);
            //现在没有非实体数据
            fx.Create(E_WithOuLifeResType.FxUnit, 0, curCtrlPlayerFxRoot);
        }
        #endregion

        // 伙伴的技能CD是由服务器通知主角实体再客户端中转到伙伴实体
        public void OnCDUpdateNotice(ProtoMsg.CDUpdateNotice cDUpdateNotice)
        {
            m_playerData.ActionOnCDUpdateNotice?.Invoke(cDUpdateNotice.CDInfo);
        }

    }
}