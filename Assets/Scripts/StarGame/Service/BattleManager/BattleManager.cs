using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Time;
using SGF.UI.Framework;
using SGF.Unity;
using SkillEditor;
using StarProject.Game;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Entity.WithOutLife.InfoEntity;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Game.Skill.Utils;
using StarProject.Service.Business;
using StarProject.Service.Cam;
using StarProject.Service.FindPath;
using StarProject.Service.Input;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.SystemOpen;
using StarProjectDef;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;


namespace StarProject.Service.Battle
{
    public class DamageData
    {
        public ulong TargetID;    //受伤目标
        public bool IsCrit;   //是否爆击
        public bool Ishit;    //是否命中
        public long Hurt;      //伤害数据
        public string damageText;   // 飘字文本
        public E_HurtType HurtType = E_HurtType.Hurt_Null;  // 伤害类型

        public DamageData(ulong targetID, bool isCrit, bool ishit, long hurt, E_HurtType hurtType, string text = "")
        {
            TargetID = targetID;
            IsCrit = isCrit;
            Ishit = ishit;
            Hurt = hurt;
            damageText = text;
            HurtType = hurtType;
        }
    }
    public class BattleManager : ServiceModule<BattleManager>
    {
        public void Init()
        {
            InitOpenAutoBattleSceneData();
            InitEntityRelationIconPath();

            OnEventMessage();

            GlobalEvent.AutoBattleEvent.RemoveListener(OnAutoBattlePermantEvent);
            GlobalEvent.AutoBattleEvent.AddListener(OnAutoBattlePermantEvent);
            GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreate);

            InitAutoBattleData();

            InitLoadCurLanguageBattleSprite();
            InitLoadCurrencySprite();

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {

                GlobalEvent.OnRefreshAllSkillNoticeEvent.AddListener(OnTestAllSkillNoticeEvent);

                return;
            }
#endif
        }

        public override void Release()
        {
            OffEventMessage();
            ReleaseGame();

            base.Release();

        }

        public void ReleaseGame()
        {
            ClearSerarchTargets();
            SetShowBossPanelEntity(null);
            // 清除 自动战斗的 状态 并且 不设置缓存
            UpdateBateState(AutoBattleState.Close, false, true, false);
            CurCheckNPC = null;

            ClearPersonSecreteData();
        }

        private void OnEventListener()
        {
            OffEventListener();

            GlobalEvent.AutoBattleEvent.AddListener(OnAutoBattleEvent);
            GlobalEvent.OnLoadingViewEvent.AddListener(OnOnLoadingViewEvent);
        }

        private void OffEventListener()
        {
            GlobalEvent.AutoBattleEvent.RemoveListener(OnAutoBattleEvent);
            GlobalEvent.OnLoadingViewEvent.RemoveListener(OnOnLoadingViewEvent);
        }

        private void OnEventMessage()
        {
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TransJobRetID, OnTransJobRet, this);


            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.AllSkillNoticeID, OnAllSkillNotice, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.SwitchSkillPosRetID, OnSwitchSkillPosRet, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.SwitchTalentRetID, OnSwitchTalentRet, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.CDUpdateNoticeID, OnCDUpdateNotice, this);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.InstanceDataRetID, OnInstanceDataRetID, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GameEndID, OnGameEndID, this);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PersonSecretLevelDataRetID, OnPersonSecretLevelDataRetID, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.EnterPersonSercetRetID, OnEnterPersonSercetRet, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GameEndPersonSecretRetID, OnGameEndPersonSecretRet, this);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GNGEndNtfID, OnGameEndID, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TreasureEndNtfID, OnGameEndID, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GameEndPersonDailyRetID, OnGameEndID, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TDGameEndNtfID, OnGameEndID, this);
        }

        private void OffEventMessage()
        {
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.TransJobRetID, OnTransJobRet, this);

            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.AllSkillNoticeID, OnAllSkillNotice, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.SwitchSkillPosRetID, OnSwitchSkillPosRet, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.SwitchTalentRetID, OnSwitchTalentRet, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.CDUpdateNoticeID, OnCDUpdateNotice, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.InstanceDataRetID, OnInstanceDataRetID, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GameEndID, OnGameEndID, this);

            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PersonSecretLevelDataRetID, OnPersonSecretLevelDataRetID, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.EnterPersonSercetRetID, OnEnterPersonSercetRet, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GameEndPersonSecretRetID, OnGameEndPersonSecretRet, this);

            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GNGEndNtfID, OnGameEndID, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.TreasureEndNtfID, OnGameEndID, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GameEndPersonDailyRetID, OnGameEndID, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.TDGameEndNtfID, OnGameEndID, this);

        }



        public EntityCtrlBase M_MainPlayerCtrlBase { get { return GameManager.Instance.M_MainPlayerCtrlBase; } }
        public PlayerCtrlGroup M_MainPlayerCtrl { get { return GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup; } }

        public bool IsSeverBattleState
        {
            get
            {
                if (M_MainPlayerCtrlBase != null)
                {
                    return M_MainPlayerCtrlBase.M_Curr.Data.IsBattleStateServer;
                }
                return false;
            }
        }

        /// <summary>
        /// 玩家是否处于交战状态
        /// </summary>
        /// <returns></returns>
        public bool IsPlayerInBattle()
        {
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.M_Curr == null)
            {
                return false;
            }
            NPCEntityBase neb = M_MainPlayerCtrlBase.M_Curr as NPCEntityBase;
            if (neb != null)
            {
                if (neb.M_BattleMixTag == E_ClentMainPlayerState.Battle)
                {
                    return true;
                }
            }

            return false;
        }


        public void EnterFrame(int frameIndex)
        {
            // 延迟关闭敌方面板
            {
                if (IsOpenDelayCloseEnemyPanel && DelayCloseCountdown > 0)
                {
                    DelayCloseCountdown -= UnityEngine.Time.fixedDeltaTime;
                    if (DelayCloseCountdown <= 0)
                    {
                        SetShowBossPanelEntity(null);
                    }
                }
            }
        }

        #region 小表情+货币

        public static TMPro.TMP_SpriteAsset CurrencySprite = null;

        private void InitLoadCurrencySprite()
        {
            string path = "UI/Icons/Emoji/IconEmo";
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<TMPro.TMP_SpriteAsset>(path,
            (TMPro.TMP_SpriteAsset asset) =>
            {
                if (asset != null)
                {
                    CurrencySprite = asset;
                }
            });
        }

        public int GetCurrencySpriteSpriteIndexFromName(string name)
        {
            int index = -1;
            if (CurrencySprite != null)
            {
                index = CurrencySprite.GetSpriteIndexFromName(name);
            }
            return index;
        }

        #endregion

        #region 伤害飘字

        public TMPro.TMP_SpriteAsset CurLanguageBattleSprite = null;

        private void InitLoadCurLanguageBattleSprite()
        {
            string path = "Fonts/Text/DamageTextNum_cn";
            switch (LanguageManager.Instance.CurLanguageType)
            {
                case LanguageType.None:
                    break;
                case LanguageType.Chinese:
                    break;
                case LanguageType.English:
                    path = "Fonts/Text/DamageTextNum_en";
                    break;
                default:
                    break;
            }
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<TMPro.TMP_SpriteAsset>(path,
            (TMPro.TMP_SpriteAsset asset) =>
            {
                if (asset != null)
                {
                    CurLanguageBattleSprite = asset;
                }
            });
        }

        //伤害飘字的RenderQueue偏移，离开战斗后重置
        public int TextRenderQueue = 0;

        private ulong DamageIndex = 0;

        private void AddDamageIndex()
        {
            if (DamageIndex + 1 >= ulong.MaxValue)
            {
                DamageIndex = 0;
            }
            else
            {
                DamageIndex++;
            }
        }

        /// <summary>
        /// 伤害飘字 【攻击者是主角、被攻击者是主角、攻击者的主人是主角、被攻击者的主人是主角】
        /// </summary>
        /// <param name="hurtData">伤害节点</param>
        /// <param name="addEffectFly">Buff飘字配置</param>
        /// <param name="builderId">施法者ID</param>
        /// <param name="ownerId">拥有者ID</param>
        /// <param name="damageTextId">飘字ID</param>
        public void OnHurtData(ProtoMsg.HurtData hurtData, List<string> addEffectFly, ulong builderId, ulong ownerId, int damageTextId, E_StageType e_StageType)
        {
            if (hurtData == null || hurtData.TargetID == 0)
            {
                return;
            }
#if UNITY_EDITOR_WIN
            //SGF.Debuger.Log($"收到伤害数据 hurtData: {hurtData} , builderID: {builderId}, ownerId: {ownerId}"); ;
#endif
            bool isPlay = GetPlayDamage(hurtData.TargetID, builderId, ownerId);
            if (isPlay)
            {
                EntityCtrlBase attackedCtrlBase = GameManager.Instance.GetEntityCtr(hurtData.TargetID);  // 被攻击者
                EntityCtrlBase attackerCtrlBase = GameManager.Instance.GetEntityCtr(ownerId);    // 攻击者
                EntityCtrlBase buildCtrlBase = GameManager.Instance.GetEntityCtr(builderId); // 施法者
                if (hurtData.Ishit)
                {
                    E_HurtType e_HurtType = GetHurtType(hurtData.TypeHurtList);
                    long hurt = hurtData.TypeHurtList[0];
                    if (hurt > 0)
                    {
                        DamageData damageData = new(hurtData.TargetID, hurtData.IsCrit, hurtData.Ishit, hurt, e_HurtType);
                        PlayDamageText(damageData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
                    }
                    //for (int i = 1; i < hurtData.TypeHurtList.Count; i++)
                    //{
                    //    long hurt = hurtData.TypeHurtList[i];
                    //    if (hurt > 0)
                    //    {
                    //        DamageData damageData = new(hurtData.TargetID, hurtData.IsCrit, hurtData.Ishit, hurt, e_HurtType);
                    //        PlayDamageText(damageData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
                    //    }
                    //}
                    //// 播放【伤害】【护盾抵挡伤害】
                    if (hurtData.ShieldValue > 0)
                    {
                        DamageData damageData = new(hurtData.TargetID, hurtData.IsCrit, hurtData.Ishit, hurtData.ShieldValue, E_HurtType.ShieldValue);
                        PlayDamageText(damageData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
                    }
                    //if (hurtData.Hurt > 0 || hurtData.ShieldValue > 0)
                    //{
                    //    PlayDamageText(hurtData.Hurt, hurtData.ShieldValue, hurtData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
                    //}
                    // 播放【吸血】
                    if (hurtData.SuckBlood > 0)
                    {
                        DamageData damageData = new(ownerId, hurtData.IsCrit, hurtData.Ishit, hurtData.SuckBlood, E_HurtType.SuckBlood);
                        EntityCtrlBase attackedCtrlBase1 = GameManager.Instance.GetEntityCtr(ownerId);
                        EntityCtrlBase attackerCtrlBase1 = GameManager.Instance.GetEntityCtr(ownerId);
                        PlayDamageText(damageData, addEffectFly, attackedCtrlBase1, attackerCtrlBase1, buildCtrlBase, damageTextId, e_StageType);
                    }
                    //if (hurtData.SuckBlood > 0)
                    //{
                    //    OnCureFloatingText(hurtData.SuckBlood, ownerId);
                    //}
                    // 播放【反伤】
                    if (hurtData.ThornsBlood > 0)
                    {
                        DamageData damageData = new(hurtData.TargetID, hurtData.IsCrit, hurtData.Ishit, hurtData.ThornsBlood, E_HurtType.ThornsBlood);
                        PlayDamageText(damageData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
                    }
                    //if (hurtData.ThornsBlood > 0)
                    //{
                    //    PlayDamageText(hurtData.ThornsBlood, 0, hurtData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
                    //}
                }
                else
                {
                    // 播放【伤害】miss
                    DamageData damageData = new(hurtData.TargetID, hurtData.IsCrit, hurtData.Ishit, 0, E_HurtType.Hurt_Physic);
                    PlayDamageText(damageData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
                    //PlayDamageText(0, 0, hurtData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
                }
            }
        }

        private E_HurtType GetHurtType(RepeatedField<long> typeHurtList)
        {
            E_HurtType type = E_HurtType.Hurt_Physic;
            {
                long maxHurt = 0;
                if (typeHurtList.Count >= 3)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        long hurt = typeHurtList[i];
                        if (hurt > maxHurt)
                        {
                            maxHurt = hurt;
                            type = (E_HurtType)i;
                        }
                    }
                }
            }
            {
                long maxHurt = 0;
                if (typeHurtList.Count >= 3)
                {
                    for (int i = 3; i < typeHurtList.Count; i++)
                    {
                        long hurt = typeHurtList[i];
                        if (hurt > maxHurt)
                        {
                            maxHurt = hurt;
                            type = (E_HurtType)i;
                        }
                    }
                }
            }
            return type;
        }

        private bool GetPlayDamage(ulong attackedId, ulong builderId, ulong ownerId)
        {
            // 被攻击者是主角的召唤物
            bool isAttackedEntityByMainPlayerSummon = GetEntityIdIsMainPlayerSummon(attackedId);
            // 施法者实体是主角的召唤物
            bool isBuilderEntityByMainPlayerSummon = GetEntityIdIsMainPlayerSummon(builderId);
            // 攻击者是主角的召唤物
            bool isOwnerEntityByMainPlayerSummon = GetEntityIdIsMainPlayerSummon(ownerId);

            // 播放逻辑=》
            // 受击者是我
            // 受击者是我的召唤物（召唤物、伙伴）
            // 攻击者是我
            // 攻击者是我的召唤物（召唤物、伙伴）
            if (
                attackedId != GameManager.Instance.mainPlayerId &&
                builderId != GameManager.Instance.mainPlayerId &&
                ownerId != GameManager.Instance.mainPlayerId &&
                !isAttackedEntityByMainPlayerSummon &&
                !isOwnerEntityByMainPlayerSummon &&
                !isBuilderEntityByMainPlayerSummon
                )
            {
                return false;
            }
            return true;
        }

        public bool GetEntityIdIsMainPlayerSummon(ulong entityId)
        {
            bool isMainPlayerSummon = false;
            // 攻击者实体
            EntityCtrlBase attackedCtrlBase = GameManager.Instance.GetEntityCtr(entityId);
            if (attackedCtrlBase != null)
            {
                isMainPlayerSummon = attackedCtrlBase.M_Curr.M_IsMainPlayerSummon;
            }
            return isMainPlayerSummon;
        }

        /// <summary>
        /// 播放伤害文字
        /// </summary>
        /// <param name="attackedId">被攻击者id</param>
        /// <param name="builderId">施法者id</param>
        /// <param name="ownerId">攻击者id</param>
        /// <param name="damageText">伤害文本</param>
        /// <param name="damageTextId">伤害飘字配置id</param>
        public void PlayDamageText(ulong attackedId, ulong builderId, ulong ownerId, string damageText, int damageTextId)
        {
            bool isPlay = GetPlayDamage(attackedId, builderId, ownerId);
            if (isPlay)
            {
                EntityCtrlBase attackedCtrlBase = GameManager.Instance.GetEntityCtr(attackedId);  // 被攻击者
                EntityCtrlBase attackerCtrlBase = GameManager.Instance.GetEntityCtr(ownerId);    // 攻击者
                EntityCtrlBase buildCtrlBase = GameManager.Instance.GetEntityCtr(builderId); // 施法者

                E_HurtType e_HurtType = E_HurtType.Hurt_Physic;
                DamageData damageData = new(0, false, true, 0, e_HurtType, damageText);
                PlayDamageText(damageData, null, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, E_StageType.None);

                //DamageEntity damageEntity = EntityFactory.InstanceEntity<DamageEntity>();
                //damageEntity.InitIndex(DamageIndex);
                //damageEntity.Create(damageText, damageTextId, attackedCtrlBase, attackerCtrlBase, buildCtrlBase);
                //AddDamageIndex();
            }
        }

        private void PlayDamageText(long hurt, long shieldValue, ProtoMsg.HurtData _hurtData, List<string> addEffectFly, EntityCtrlBase attackedCtrlBase, EntityCtrlBase attackerCtrlBase, EntityCtrlBase buildCtrlBase, int damageTextId, E_StageType e_StageType)
        {
            //ProtoMsg.HurtData hurtData = new();
            //hurtData.TargetID = _hurtData.TargetID;
            //hurtData.Hurt = hurt;
            //hurtData.IsCrit = _hurtData.IsCrit;
            //hurtData.Ishit = _hurtData.Ishit;
            //hurtData.ShieldValue = shieldValue;

            //DamageEntity damageEntity = EntityFactory.InstanceEntity<DamageEntity>();
            //damageEntity.InitIndex(DamageIndex);
            //damageEntity.Create(hurtData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
            //AddDamageIndex();
        }

        private void PlayDamageText(
            DamageData damageData,
            List<string> addEffectFly,
            EntityCtrlBase attackedCtrlBase,
            EntityCtrlBase attackerCtrlBase,
            EntityCtrlBase buildCtrlBase,
            int damageTextId,
            E_StageType e_StageType
            )
        {
            DamageEntity damageEntity = EntityFactory.InstanceEntity<DamageEntity>();
            damageEntity.InitIndex(DamageIndex);
            damageEntity.Create(damageData, addEffectFly, attackedCtrlBase, attackerCtrlBase, buildCtrlBase, damageTextId, e_StageType);
            AddDamageIndex();
        }

        #endregion

        #region 治疗飘字
        public void OnHurtData(ProtoMsg.CureData cureData, ulong builderId, ulong ownerId)
        {
            if (cureData == null || cureData.TargetID == 0)
            {
                return;
            }
            bool isPlay = GetPlayDamage(cureData.TargetID, builderId, ownerId);
            if (isPlay)
            {
                EntityCtrlBase attackedCtrlBase = GameManager.Instance.GetEntityCtr(cureData.TargetID);
                EntityCtrlBase attackerCtrlBase = GameManager.Instance.GetEntityCtr(ownerId);
                E_HurtType e_HurtType = E_HurtType.SuckBlood;
                DamageData damageData = new(cureData.TargetID, cureData.IsCrit, true, cureData.Cure, e_HurtType);
                PlayDamageText(damageData, null, attackedCtrlBase, attackerCtrlBase, null, 0, E_StageType.None);
                //DamageEntity damageEntity = EntityFactory.InstanceEntity<DamageEntity>();
                //    ProtoMsg.HurtData hurtData = new();
                //    hurtData.TargetID = cureData.TargetID;
                //    hurtData.SuckBlood = cureData.Cure;
                //    hurtData.IsCrit = cureData.IsCrit;
                //    hurtData.Ishit = true;
                //    damageEntity.InitIndex(DamageIndex);
                //    damageEntity.Create(hurtData, null, attackedCtrlBase, attackerCtrlBase, null, 3, E_StageType.None);
                //    AddDamageIndex();
            }
        }

        public void OnCureFloatingText(long hp, ulong ownerId)
        {
            EntityCtrlBase attackedCtrlBase = GameManager.Instance.GetEntityCtr(ownerId);
            EntityCtrlBase attackerCtrlBase = GameManager.Instance.GetEntityCtr(ownerId);
            DamageEntity damageEntity = EntityFactory.InstanceEntity<DamageEntity>();
            E_HurtType e_HurtType = E_HurtType.SuckBlood;
            DamageData damageData = new(ownerId, false, true, hp, e_HurtType);
            PlayDamageText(damageData, null, attackedCtrlBase, attackerCtrlBase, null, 0, E_StageType.None);
            //ProtoMsg.HurtData hurtData = new();
            //hurtData.TargetID = ownerId;
            //hurtData.SuckBlood = hp;
            //hurtData.IsCrit = false;
            //hurtData.Ishit = true;
            //damageEntity.InitIndex(DamageIndex);
            //damageEntity.Create(hurtData, null, attackedCtrlBase, attackerCtrlBase, null, 3, E_StageType.None);
            //AddDamageIndex();
        }

        #endregion

        #region 当前攻击的实体

        private NPCEntityBase m_CurAtkEntity;

        public NPCEntityBase CurAtkEntity
        {
            get
            {
                return m_CurAtkEntity;
            }
        }

        private void SetCurAtkEntity(NPCEntityBase curAtkEntity)
        {
            if (curAtkEntity != null && curAtkEntity.Data.IsForbidSelect)
            {
                // SGF.Debuger.Log("当前物体禁止被选中");
                return;
            }
            if (curAtkEntity != m_CurAtkEntity)
            {
                // 关闭目标实体脚上的选中圈指示器
                if (m_CurAtkEntity != null)
                {
                    // 目标选中委托--取消掉
                    m_CurAtkEntity.ActionOnCheckTarget?.Invoke(false);
                    m_CurAtkEntity.ActionOnCheckTargetMove -= OnActionOnCheckTargetMove;
                }
                if (curAtkEntity != null)
                {
                    curAtkEntity.ActionOnCheckTarget?.Invoke(true);
                    curAtkEntity.ActionOnCheckTargetMove += OnActionOnCheckTargetMove;
                }
            }
            m_CurAtkEntity = curAtkEntity;

            if (IsAutoBattling)
            {
                curAutoBattleAtkEntity = curAtkEntity == null ? 0 : curAtkEntity.EntityId;
            }
        }

        private void SetCurAtkEntityID(ulong entityID)
        {
            // SGF.Debuger.Log($"[Switch-Search] SetCurAtkEntityID 设置索敌怪物ID: {CurAtkEntityId} ---> {entityID}");
            if (entityID == 0)
            {
                SetCurAtkEntity(null);
            }
            else
            {
                var entity = GameManager.Instance.GetEntityByEntityID(entityID);
                if (entity == null)
                {
                    SetCurAtkEntity(null);
                }
                else
                {
                    SetCurAtkEntity(entity);
                }
            }
        }

        public ulong CurAtkEntityId
        {
            get
            {
                if (CurAtkEntity != null && CurAtkEntity.Data != null)
                {
                    return CurAtkEntity.Data.M_EntityID;
                }
                return 0;
            }
        }

        private Camera mainCamera;
        public Camera MainCamera
        {
            get
            {
                if (mainCamera == null)
                {
                    mainCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;
                }
                return mainCamera;
            }

        }

        /// <summary>
        /// 目标与主角移动回调
        /// </summary>
        public void OnActionOnCheckTargetMove()
        {
            if (m_CurAtkEntity != null && M_MainPlayerCtrlBase.M_Curr != null)
            {
                UnityEngine.Vector3 mainPlayerPos = MainCamera.WorldToScreenPoint(M_MainPlayerCtrlBase.M_Curr.Position());
                UnityEngine.Vector3 targetPos = MainCamera.WorldToScreenPoint(m_CurAtkEntity.Position());
                float distance = UnityEngine.Vector3.Distance(mainPlayerPos, targetPos);
                if (distance > Screen.width * 2)
                {
                    // 已经超过了俩个屏幕高度
                    // 脱离目标id
                    // SetCurAtkEntity(null);
                    // 脱离之后, 重新索敌
                    BattleManager.Instance.ClearSerarchTargets();
                }
            }
        }

        public bool GetEntityIdIsAlive(ulong entityId)
        {
            EntityCtrlBase entityCtrl = GameManager.Instance.GetEntityCtr(entityId);
            if (entityCtrl != null && entityCtrl.M_Curr != null)
            {
                return entityCtrl.M_Curr.M_IsAlive;
            }
            return false;
        }

        #endregion

        #region 当前显示的BOSS面板

        private bool IsOpenDelayCloseEnemyPanel = false;   // 是否开启延迟关闭
        private float DelayCloseCountdown = 3f;  // 延迟关闭倒计时

        private NPCEntityBase m_ShowBossPanelEntity;

        public NPCEntityBase ShowBossPanelEntity
        {
            get
            {
                return m_ShowBossPanelEntity;
            }
        }

        public void SetShowBossPanelEntity(NPCEntityBase entityBase)
        {
            if (entityBase != null && entityBase.Data != null && entityBase.Data.IsForbidSelect)
            {
                return;
            }

            if (entityBase == m_ShowBossPanelEntity)
            {
                return;
            }

            if (entityBase != null && entityBase.Data != null)
            {
                E_EntityType entityType = entityBase.Data.EntityType;
                switch (entityType)
                {
                    case E_EntityType.None:
                    case E_EntityType.RoomSpace:
                    case E_EntityType.Npc:
                    case E_EntityType.BulletEntity:
                    case E_EntityType.Interact:
                    case E_EntityType.Summon:
                    case E_EntityType.Partner:
                    case E_EntityType.ClientSummon:
                    case E_EntityType.Player:
                        {
                            bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(entityBase);
                            if (!isTargetNtt)
                            {
                                return;
                            }
                        }
                        break;
                    case E_EntityType.Monster:
                    case E_EntityType.GVEBoss:
                    case E_EntityType.Robot:
                        {
                            MonsterDataCell monsterAttrDataCell = LocalDataManager.Instance.GetMonsterDataCell((int)entityBase.ConfigIndex);
                            if (monsterAttrDataCell != null)
                            {
                                int monsterType = monsterAttrDataCell.GetMonType();
                                var isWildBoss = (entityBase.Data.ActMark & (1 << (int)GamePlay.GpWildBoss)) > 0;
                                bool isShow = monsterType == 3 || entityBase.Data.IsGVEBoss || isWildBoss;
                                if (!isShow)
                                {
                                    return;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            GlobalEvent.onEnemyInfo?.Invoke(entityBase);
            m_ShowBossPanelEntity = entityBase;
            IsOpenDelayCloseEnemyPanel = false;
        }

        public void OnMainPlayMove()
        {
            if (GameManager.Instance.GetCurMapType() == SpaceType.SpaceSercet)
            {
                // 如果是秘境的话，就不限制距离
                return;
            }
            if (M_MainPlayerCtrlBase != null && M_MainPlayerCtrlBase.M_Curr != null)
            {
                UnityEngine.Vector3 mainPlayerPos = M_MainPlayerCtrlBase.M_Curr.Position();
                float configDis = GameMap.SceneJsonGridSize - 2.0f;
                if (ShowBossPanelEntity != null)
                {
                    UnityEngine.Vector3 targetPos = ShowBossPanelEntity.Position();
                    float distance = UnityEngine.Vector3.Distance(mainPlayerPos, targetPos);
                    if (distance > configDis)
                    {
                        // 打开延迟关闭
                        DelayCloseCountdown = 3.0f;
                        IsOpenDelayCloseEnemyPanel = true;
                    }
                }
                else
                {
                    // 找一个近一点的
                    NPCEntityBase nPCEntityBase = GameManager.Instance.GetCloestBoss();
                    if (nPCEntityBase != null)
                    {
                        UnityEngine.Vector3 targetPos = nPCEntityBase.Position();
                        float distance = UnityEngine.Vector3.Distance(mainPlayerPos, targetPos);
                        if (distance < configDis)
                        {
                            SetShowBossPanelEntity(nPCEntityBase);
                        }
                    }
                }
            }
        }

        //BOSS血条显示逻辑优化
        //1.当BOSS进入玩家视野范围时，则直接同步BOSS数据
        //2.当BOSS出现在玩家屏幕范围内时，则开始显示BOSS血条
        //若有多个BOSS，则按照与玩家当前距离来进行显示，显示近的
        //3.当玩家通过移动等手段，使BOSS不在屏幕范围内时，则延迟3秒后，清除BOSS血条
        //【AOI范围大，这边屏幕范围指的是屏幕大小+3米，超过之后延迟关闭BOSS血条显示】
        //4.所有血条显示逻辑的最高优先级，为玩家通过操作点击锁定的怪物（包括切换按钮）
        //5.以上所有逻辑，为玩家未进入战斗状态时的血条显示逻辑
        //且BOSS血条显示逻辑，不会与小怪逻辑互相冲突
        public void DistanceSetEnemy(NPCEntityBase curAtkEntity)
        {
            bool isLimitDis = true;
            float configDis = GameMap.SceneJsonGridSize - 2.0f;
            if (GameManager.Instance.GetCurMapType() == SpaceType.SpaceSercet)
            {
                // 如果是秘境的话，就不限制距离
                isLimitDis = false;
            }
            if (ShowBossPanelEntity != null && curAtkEntity != null && isLimitDis)
            {
                if (ShowBossPanelEntity.EntityId == curAtkEntity.EntityId)
                {
                    // 判断距离，超过一定范围就清除血条面板显示
                    if (M_MainPlayerCtrlBase != null && M_MainPlayerCtrlBase.M_Curr != null)
                    {
                        UnityEngine.Vector3 mainPlayerPos = M_MainPlayerCtrlBase.M_Curr.Position();
                        UnityEngine.Vector3 targetPos = ShowBossPanelEntity.Position();
                        float distance = UnityEngine.Vector3.Distance(mainPlayerPos, targetPos);
                        if (distance > configDis)
                        {
                            // 打开延迟关闭
                            DelayCloseCountdown = 3.0f;
                            IsOpenDelayCloseEnemyPanel = true;
                        }
                    }
                }
                else if (ShowBossPanelEntity.IsCheckedEntity)
                {
                    // 上一个是点击选中的，那就算了
                }
                else
                {
                    // 2.当BOSS出现在玩家屏幕范围内时，则开始显示BOSS血条
                    // 若有多个BOSS，则按照与玩家当前距离来进行显示，显示近的
                    if (M_MainPlayerCtrlBase != null && M_MainPlayerCtrlBase.M_Curr != null)
                    {
                        UnityEngine.Vector3 mainPlayerPos = M_MainPlayerCtrlBase.M_Curr.Position();
                        UnityEngine.Vector3 targetPos = curAtkEntity.Position();
                        float distance = UnityEngine.Vector3.Distance(mainPlayerPos, targetPos);
                        if (distance < configDis)
                        {
                            // 传进来的这个符合要求，然后对比和上一个的距离
                            UnityEngine.Vector3 lastEntityScreenPos = ShowBossPanelEntity.Position();
                            float distanceLast = UnityEngine.Vector3.Distance(mainPlayerPos, lastEntityScreenPos);
                            if (distanceLast < distance)
                            {
                                SetShowBossPanelEntity(curAtkEntity);
                            }
                        }
                    }
                }
            }
            else if (ShowBossPanelEntity == null && curAtkEntity != null)
            {
                // 判断距离，超过一定范围就清除血条面板显示
                if (M_MainPlayerCtrlBase != null && M_MainPlayerCtrlBase.M_Curr != null)
                {
                    UnityEngine.Vector3 mainPlayerPos = M_MainPlayerCtrlBase.M_Curr.Position();
                    UnityEngine.Vector3 targetPos = curAtkEntity.Position();
                    float distance = UnityEngine.Vector3.Distance(mainPlayerPos, targetPos);
                    if (distance < configDis || !isLimitDis)
                    {
                        // 在距离内了
                        SetShowBossPanelEntity(curAtkEntity);
                    }
                }
            }
        }

        #endregion

        #region 当前选中的NPC

        private NPCEntityBase m_CurCheckNPC;

        public NPCEntityBase CurCheckNPC
        {
            get
            {
                return m_CurCheckNPC;
            }
            set
            {
                if (m_CurCheckNPC != null && value != m_CurCheckNPC)
                {
                    m_CurCheckNPC.ActionOnCheckTarget?.Invoke(false);
                }
                m_CurCheckNPC = value;
                if (m_CurCheckNPC != null)
                {
                    m_CurCheckNPC.ActionOnCheckTarget?.Invoke(true);
                }
            }
        }

        #endregion

        #region 技能相关逻辑

        public ProtoMsg.AllSkillNotice allSkillNotice;

#if UNITY_EDITOR
        public void OnTestAllSkillNoticeEvent(AllSkillNotice allSkillNotice)
        {
            MessageHandleData messageData = new();
            messageData.data = allSkillNotice;
            OnAllSkillNotice(messageData);

            (GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup)?.OnAllSkillNotice(allSkillNotice);
        }
#endif

        public void OnAllSkillNotice(MessageHandleData data)
        {
            ProtoMsg.AllSkillNotice _allSkillNotice = (ProtoMsg.AllSkillNotice)data.data;

            allSkillNotice = _allSkillNotice;

            GlobalEvent.OnAllSkillNoticeEvent?.Invoke(allSkillNotice);

            RefreshCacheSkillPosContainers();
        }

        public void OnSwitchSkillPosRet(MessageHandleData data)
        {
            ProtoMsg.SwitchSkillPosRet switchSkillPosRet = (ProtoMsg.SwitchSkillPosRet)data.data;
            GlobalEvent.OnSwitchSkillPosRetEvent?.Invoke(switchSkillPosRet);
        }

        public void OnSwitchTalentRet(MessageHandleData data)
        {
            ProtoMsg.SwitchTalentRet switchTalentRet = (ProtoMsg.SwitchTalentRet)data.data;
            GlobalEvent.OnSwitchTalentRetEvent?.Invoke(switchTalentRet);
        }

        public void OnCDUpdateNotice(MessageHandleData data)
        {
            ProtoMsg.CDUpdateNotice cDUpdateNotice = (ProtoMsg.CDUpdateNotice)data.data;
            GlobalEvent.OnCDUpdateNotice?.Invoke(cDUpdateNotice);

        }
        #endregion

        #region 转职

        private void OnTransJobRet(MessageHandleData data)
        {
            ProtoMsg.TransJobRet transJobRet = (ProtoMsg.TransJobRet)data.data;
            if (transJobRet.Success)
            {
                //UIManager.Instance.OpenWindow(UIDef.TransJobFinishWindow, transJobRet.Job, MainPageCommond.HideBoth, false, true);

                object[] Os = new object[] { transJobRet.Job };
                DelayInvoker.DelayInvoke("TransJobRet", 1.5f,
                      //延迟处理--------------------
                      (object[] args) =>
                      {
                          int jobID = (int)args[0]; ;
                          UIManager.Instance.OpenWindowAsync(UIDef.TransJobFinishWindow, null, jobID, MainPageCommond.HideBoth);
                      }
                  , Os);
            }
        }

        #endregion

        #region 自动战斗
        public const string AUTOBATTLEKEY = "[AutoSkill]";
        public static string AUTOBATTLE_OPEN = "AutoSkill_OPEN";
        private Dictionary<int, KeyValuePair<KeyCode, int>> pos2KeyCode = new();

        private ulong curAutoBattleAtkEntity;

        /// <summary>
        /// 寻找怪物的 半径 (cm)
        /// 曲爷 寻怪的接口 参数是 cm
        /// </summary>
        private int searchEnemyRadio = 1000;

        /// <summary>
        /// 是否 正在向怪物 移动
        /// </summary>
        private bool isMovingToEnemy = false;

        private bool dirtyAutoSkill = false;

        /// <summary>
        /// 自动战斗 思考的 频率, 目前是 THINKING_FREQUENCY*FixedUpdateTime 触发一次 Ontick
        /// </summary>  
        const int THINKING_FREQUENCY = 20;
        private int thinkingTimes = 0;

        /// <summary>
        /// 伙伴多久一次 触发自动战斗
        /// </summary>
        const int PARTNER_THINKING_TIME = 5000;
        /// <summary>
        /// 伙伴 思考的时间
        /// </summary>
        private long partnerThinkingTime = 0;

        /// <summary>
        /// 技能 槽 的数组
        /// </summary>
        private List<int> skillPoss = new() { 7, 3, 4, 5, 6, 2 };
        // private List<int> skillPoss = new() { 6, 2 };
        // private List<int> skillPoss = new() { 2 };
        // private List<int> skillPoss = new() { 4 };
        // private List<int> skillPoss = new() { 6, 2 };

        /// <summary>
        /// 缓存的节能位 对应的技能槽
        /// </summary>
        private List<SkillContainer> cacheSkillPosContainers = new();

        private string mainKey = "";

        private string MainKey
        {
            get
            {
                if (mainKey == "")
                {
                    mainKey = this.GetHashCode().ToString();
                }
                return mainKey;
            }
        }

        /// <summary>
        /// 是否 开启了 自动战斗.  
        /// 自动战斗是个符合状态 ， 但是只要有 AutoBattleState.Open, 就认为自动战斗开启, 
        /// 不管是否 被禁止 还是说 被挂起
        /// </summary>
        public bool IsOpenAutoBattle => CheckHasState(AutoBattleState.Open);

        /// <summary>
        /// 是否禁止
        /// </summary>
        public bool IsForbid => CheckHasState(AutoBattleState.Forbid);

        /// <summary>
        /// 是否自动战斗的 状态 : 非禁止且 Open状态开启即可。
        /// 所以 open 和 hold  都是属于 战斗中的 状态
        /// Hold 状态 也是 open|hold 的复合集, forbid 是单独的禁止状态
        /// </summary>
        public bool IsAutoBattling => !IsForbid && CheckHasState(AutoBattleState.Open);

        public bool IsAutoBattleHoldOn => !IsForbid && (CheckHasState(AutoBattleState.HoldOn) || CheckHasState(AutoBattleState.HoldOn_AVG));

        /// <summary>
        /// 是否是 正在寻路状态
        /// findPath 状态 可能有多种情况产生:
        /// 1. 自动战斗 自己的寻路, 寻路结束后, 开始战斗;
        /// 2. 自动战斗 途中, 任务或者其它东西 出发的 寻路, 寻路结束后, 依旧出发 自动战斗
        /// </summary>
        public bool IsAutoBattleFindingPath => !IsForbid && CheckHasState(AutoBattleState.OtherFindingPath);


        /// <summary>
        /// 自动战斗的状态
        /// </summary>
        public AutoBattleState autoBattleState = AutoBattleState.Close;

        public UnityEngine.Vector3 recordStartPos = UnityEngine.Vector3.zero;

        /// <summary>
        /// 场景 对应 自动战斗 开启 的配置
        /// </summary>
        private Dictionary<int, bool> scene2AutoBattleMap = new();
        /// <summary>
        /// 初始化 场景 开启自动战斗 的配置数据, 有些场景 进入即禁止自动战斗
        /// </summary>
        private void InitOpenAutoBattleSceneData()
        {
            // 1.日常副本
            var levelDatas = LocalDataManager.Instance.M_DailyLevel.StaticDailyLevelDatas;
            foreach (var item in levelDatas)
            {
                var v = item.Value;
                scene2AutoBattleMap[v.GetID()] = v.GetAutoBattle();
            }

            // 2.SecretLevel
            var secretDatas = LocalDataManager.Instance.M_SecretLevel.StaticSecretLevelDatas;
            foreach (var item in secretDatas)
            {
                var v = item.Value;
                scene2AutoBattleMap[v.GetID()] = v.GetAutoBattle();
            }
            // 3.MirrorLevel
            var mirrorDatas = LocalDataManager.Instance.M_MirrorLevel.StaticMirrorLevelDatas;
            foreach (var item in mirrorDatas)
            {
                var v = item.Value;
                scene2AutoBattleMap[v.GetID()] = v.GetAutoBattle();
            }
            // 4.PlotLevel
            var plotDatas = LocalDataManager.Instance.M_PlotLevel.StaticPlotLevelDatas;
            foreach (var item in plotDatas)
            {
                var v = item.Value;
                scene2AutoBattleMap[v.GetID()] = v.GetAutoBattle();
            }
            // 5.TeamDailyLevel
            var teamDailyDatas = LocalDataManager.Instance.M_TeamDailyLevel.StaticTeamDailyLevelDatas;
            foreach (var item in teamDailyDatas)
            {
                var v = item.Value;
                scene2AutoBattleMap[v.GetID()] = v.GetAutoBattle();
            }
        }

        public void InitForbiddeAutoPlaySkills()
        {
            string battleLocalKey = $"{GameConfig.SETTING_BATTLE}_{StarProject.Service.User.UserManager.Instance.MainUserData.playerRoleId}";
            bool isExists = SaveManager.Instance.KeyExists(battleLocalKey, "Setting");
            if (isExists)
            {
                ForbidSkillPoss = SaveManager.Instance.Load<List<int>>(battleLocalKey, "Setting");
            }
            ForbidSkillPoss ??= new();


        }

        private void InitAutoBattleData()
        {
            LocalDataManager.Instance.M_SkillPosSetDataList.ForEach((KeyValuePair<int, SkillPosSetDataCell> posSetData) =>
            {
                int m_KeyCode = 0;
                if (Enum.TryParse<KeyCode>(posSetData.Value.KeyCode, out KeyCode keyCode))
                {
                    m_KeyCode = (int)keyCode;
                }

                if (keyCode == 0)
                {
                    return;
                }
                // 生成 技能槽 对应的 按键输入指令
                pos2KeyCode[posSetData.Key] = new KeyValuePair<KeyCode, int>(keyCode, m_KeyCode);
            });

        }

        private void OnRoleCreate(object v)
        {
            // 刷新伙伴自动战斗的 缓存
            bool isPartnerAutoBattleOn = GetPartnerAutoBattleCache();
            SwitchPartnerAutoBattle(isPartnerAutoBattleOn);

            (M_MainPlayerCtrlBase as PlayerCtrlGroup).BreakFindPath();
            (M_MainPlayerCtrlBase as PlayerCtrlGroup).BreakFollowDynamicEnity();

            // 注册一个技能槽位的刷新逻辑
            {
                (M_MainPlayerCtrlBase.M_Curr as HeroEntityBase).skillDispatcher.SkillUnitController.ActionOnRefreshAllSkill -= RefreshCacheSkillPosContainers;
                (M_MainPlayerCtrlBase.M_Curr as HeroEntityBase).skillDispatcher.SkillUnitController.ActionOnRefreshAllSkill += RefreshCacheSkillPosContainers;
            }

            // 开启自动自动战斗的时候 记录起始点
            RecordAutoBattleStartPos(true);
            // 自动战斗开始前 同步一下 技能槽数据
            RefreshCacheSkillPosContainers();
        }

        public bool GetPartnerAutoBattleCache()
        {
            if (!LocalCacheManager.Instance.Has(GameConfig.SETTING_BATTLE_PARTNER))
            {
                return false;
            }
            else
            {
                return (bool)LocalCacheManager.Instance.Get(GameConfig.SETTING_BATTLE_PARTNER);
            }
        }

        /// <summary>
        /// 自动战斗的 常驻 事件监听, 例如 进出副本的 禁止状态 设置的 监听. 它 需要独立于 自动战斗 的 运行时状态
        /// </summary>
        private void OnAutoBattlePermantEvent(string eventType, object v)
        {
            try
            {
                if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.Ectype))
                {
                    OnEctype(v);
                }

                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.MapPreloadNotice))
                {
                    OnMapPreloadNotice(v);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.MapChangeRet))
                {
                    OnMapChangeRet(v);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.LeaveSpace))
                {
                    OnLeaveSpace(v);
                }
                else if (eventType == "FindingPath")
                {
                    OnFindPath(v);
                }
            }
            catch (System.Exception e)
            {
                //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnAutoBattlePermantEvent : eventType {eventType} ,v: {v} error {e.Message} ");

            }
        }

        private void OnAutoBattleEvent(string eventType, object v)
        {
            try
            {
                // 对不同的 事件类型 做处理, switch 的case 没办法 做 枚举的 .ToString 运算, 所以用的 if/else
                if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.HeroDie))
                {
                    OnHeroDie(v);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.AOILeave))
                {
                    OnLeaveAoi(v);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.AOIEnter))
                {
                    OnEnterAOI(v);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.M_NoneActiveSkill))
                {
                    OnNoneActiveSkill(v);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.EctypeModuleGameEnd))
                {
                    OnEctypeModuleGameEnd(v);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.M_SkillEndCD))
                {
                    OnSkillEndCD(v);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.StartUserInput))
                {
                    // SGF.Debuger.LogError("[dirty] [开启用户输入] ");
                    MarkDirtyState(true);
                }
                else if (eventType == GameManager.Instance.GetTriggerEventStr(TriggerEventType.EndButtonCD))
                {
                    // SGF.Debuger.LogError("[dirty] 结束了 buttonCD ");
                    MarkDirtyState(true);
                }

                else if (eventType == "ClickTask")
                {
                    OnClickTask(v);
                }

                else if (eventType == "AnalogStick_Up")
                {
                    OnAnalogStickUp(v);
                }
                else if (eventType == "AnalogStick_Scrolling")
                {
                    OnAnalogStickScrolling(v);
                }
                else if (eventType == "On_ReLogin")
                {
                    OnReLogin(v);
                }
                else if (eventType == "TaskItem_Select")
                {
                    OnTaskItemSelect(v);
                }
                else if (eventType == "FindPathByEctype")
                {
                    OnFindPathByEctype(v);
                }
                else if (eventType == "AvgStart")
                {
                    OnAvgStart(v);
                }


                // else if (eventType == "FindingPath")
                // {
                //     OnFindPath(v);
                // }

            }
            catch (System.Exception e)
            {
                //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnAutoBattleEvent : eventType {eventType} ,v: {v} error {e.Message} ");

            }
        }

        private bool IsExecuteAutoBattle()
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase == null || GameManager.Instance.M_MainPlayerCtrlBase.Data == null)
            {
                return false;
            }

            if (!IsAutoBattling)
            {
                return false;
            }

            // 挂起状态
            if (IsAutoBattleHoldOn)
            {
                return false;
            }

            // 寻路状态
            if (IsAutoBattleFindingPath)
            {
                return false;
            }

            // 主角死亡的时候， 自动战斗不关闭, 但是 后续逻辑不执行
            if (GameManager.Instance.M_MainPlayerCtrlBase.Data.IsDead)
            {
                return false;
            }

            return true;
        }
        private void OnFixedUpdate()
        {

            // 全局的自动战斗技能cd ,独立于 冷却cd 之外
            if (globalSkillCD > 0)
            {
                globalSkillCD -= TimeUtils.FixedDeltaTime;
                if (globalSkillCD <= 0)
                {
                    globalSkillCD = 0;
                    MarkDirtyState(true);
                }
            }

            // 加个冷却时间,点击 任务的时候,可能正在释放技能, 此时 给他加个冷却, 让技能结束 能够去跑任务
            if (coolTime > 0)
            {
                coolTime -= TimeUtils.FixedDeltaTime;
                if (coolTime <= 0)
                {
                    coolTime = 0;
                    MarkDirtyState(true);
                }
                return;
            }

            if (!IsExecuteAutoBattle())
            {
                return;
            }

            // 如果当前帧 被标脏, 那就表明当前 帧 正在位移 或者释放技能 过程中 触发了 需要释放技能.
            if (dirtyAutoSkill)
            {
                dirtyAutoSkill = false;
                // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] OnTick false");

                OnTick();
                return;
            }

            thinkingTimes++;

            if (thinkingTimes >= THINKING_FREQUENCY)
            {
                thinkingTimes = 0;

                // SGF.Debuger.LogError("[dirty] update ");

                MarkDirtyState(true);
            }

            // 高磊要求 伙伴的自动战斗每 5s 检查一次是否可以释放， 而不关心伙伴的技能cd 是否已经结束
            if (isPartnerAutoBattleOpen && TimeUtils.ClientNowStampMilli - partnerThinkingTime > PARTNER_THINKING_TIME)
            {
                partnerThinkingTime = TimeUtils.ClientNowStampMilli;
                TriggerPartnerSkill();
            }
        }

        private void OnTick()
        {
            //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnTick  IsAutoBattling: {IsAutoBattling} , IsAutoBattleHoldOn: {IsAutoBattleHoldOn}");
            if (IsAutoBattleHoldOn)
            {
                return;
            }
            SearchEnemyFight();
        }

        public void SwitchAutoBattle()
        {
            if (IsForbid)
            {
                return;
            }
            if (!IsAutoBattling)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        /// <summary>
        /// 伙伴的自动战斗是否开放
        /// </summary>
        private bool isPartnerAutoBattleOpen = false;

        public void SwitchPartnerAutoBattle(bool isOpen)
        {
            isPartnerAutoBattleOpen = isOpen;
        }


        /// <summary>
        /// 自动战斗的 冷却CD
        /// </summary>
        private float coolTime = 0;
        public void CoolDown(int time = 1000)
        {
            if (!IsAutoBattling)
            {
                return;
            }
            coolTime = time;
        }
        public void ResetCoolDown()
        {

            coolTime = 0;
            MarkDirtyState(true);
        }

        /// <summary>
        /// 自动战斗的全局的 技能cd
        /// </summary>
        private float globalSkillCD = 0;

        private void StartGlobalSkillCD(int time = 250)
        {
            globalSkillCD = time;
        }

        private bool IsGlobalSkillCDIng()
        {
            return globalSkillCD > 0;
        }

        private AutoBattleState tempState = AutoBattleState.Close;
        private void UpdateBateState(AutoBattleState state, bool setState, bool forceSetState = false, bool saveCache = true)
        {
            if (!forceSetState)
            {
                if (setState)
                {
                    tempState = autoBattleState | state;
                }
                else
                {
                    tempState = autoBattleState & ~state;
                }

                if (tempState == autoBattleState)
                {
                    return;
                }
            }
            else
            {
                // 如果强行设置状态
                tempState = state;
            }

            // SGF.Debuger.Log($"{AUTOBATTLEKEY} , UpdateBateState: {state}  ---> {tempState}");

            autoBattleState = tempState;
            // 通知外面 自动战斗开始
            GameManager.Instance.TriggerEvent(AUTOBATTLEKEY, autoBattleState);

            if (saveCache)
            {
                // 刷新 自动战斗对应的 缓存
                // SGF.Debuger.Log($"{AUTOBATTLEKEY} , UpdateBateState: {state}  ---> {tempState} , IsOpenAutoBattle: {IsOpenAutoBattle} ");
                LocalCacheManager.Instance.SetValueType(AUTOBATTLE_OPEN, IsOpenAutoBattle);
            }
        }

        private bool CheckHasState(AutoBattleState state)
        {
            return (state & autoBattleState) == state;
        }

        public void Start()
        {
            if (isForbid)
            {
                return;
            }

            //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} , Start ***********************");

            UpdateBateState(AutoBattleState.Open, true);

            OnEventListener();

            MonoHelper.AddFixedUpdateListener(OnFixedUpdate, MonoHelper.E_ModuleType.Battle);

            // // 自动战斗开始的时候, 关闭当前的 自动寻路
            if (M_MainPlayerCtrlBase != null)
            {
                // 开启自动自动战斗的时候 记录起始点
                RecordAutoBattleStartPos(true);
                (M_MainPlayerCtrlBase as PlayerCtrlGroup).BreakFindPath();
                (M_MainPlayerCtrlBase as PlayerCtrlGroup).BreakFollowDynamicEnity();
                // 自动战斗开始前 同步一下 技能槽数据
                RefreshCacheSkillPosContainers();
            }

            MarkDirtyState(true);
        }

        public void Stop()
        {
            //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , end =========================== ");
            // 如果没有 被禁止, 那 stop 就只是 关闭 stop 的状态.
            if (!IsForbid)
            {
                UpdateBateState(AutoBattleState.Close, false, true);
            }
            else
            {
                // 如果禁止了, 那就强行设置 禁止状态
                UpdateBateState(AutoBattleState.Forbid, false, true);
            }
            coolTime = 0;

            // 通知外面 自动战斗结束

            OffEventListener();

            MonoHelper.RemoveFixedUpdateListener(OnFixedUpdate, MonoHelper.E_ModuleType.Battle);

            BreakCurGotoFightEnemy(true);
        }

        private void OnOnLoadingViewEvent(bool isLoading)
        {
            // loading 的时候, 先上个长一点事件 自动战斗的冷却时间
            if (isLoading)
            {
                coolTime = 10000;
            }
            else
            {
                // 进去后 过一会 开启自动战斗
                coolTime = 800;
            }
        }

        /// <summary>
        /// 打断本次 打怪的过程
        /// </summary>
        private void BreakCurGotoFightEnemy(bool isStop = false)
        {
            // 先将 一些状态还原
            {
                isMovingToEnemy = false;
                movingToTargetPos = UnityEngine.Vector3.zero;

                // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] BreakCurGotoFightEnemy false");

                dirtyAutoSkill = false;
                thinkingTimes = 0;
                curAutoBattleAtkEntity = 0;
            }

            DelayInvoker.CancelInvoke(MainKey);


            if (M_MainPlayerCtrl != null)
            {
                if (!isStop)
                {
                    // 打断当前的 寻路
                    M_MainPlayerCtrl.BreakFindPath();
                    M_MainPlayerCtrl.BreakFollowDynamicEnity();
                }
                else
                {
                    switch (M_MainPlayerCtrl.Cur_FindPathType)
                    {
                        case E_FindPathType.AutoBattle:
                        case E_FindPathType.AutoBattle_MonsterPoint:
                        case E_FindPathType.AutoBattle_Move2Pos:
                        case E_FindPathType.AutoBattle_Spawner:
                            {
                                M_MainPlayerCtrl.BreakFindPath();
                                M_MainPlayerCtrl.BreakFollowDynamicEnity();
                                break;
                            }
                        default:
                            {
                                SGF.Debuger.Log($"[AutoBattle] BreakCurGotoFightEnemy 结束自动战斗 FindPathType ---> {M_MainPlayerCtrl.Cur_FindPathType} 不是自动战斗类型,不需要打断寻路");
                            }
                            break;
                    }

                }

            }



        }

        private bool testGotoStart = false;
        public void TestGotoStart()
        {
            //testGotoStart = !testGotoStart;
            TestAutoBattle();
        }

        private bool isForbid = false;
        private void TestAutoBattle()
        {
            isForbid = !isForbid;
            UpdateBateState(AutoBattleState.Forbid, isForbid);
        }

        // 查找怪物找不到的时间
        private long searchEmptyTime = 0;
        private void SearchEnemyFight()
        {
            bool searchResult = SearchEnemy();

            //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} , SearchEnemyFight 索敌 searchResult: {searchResult} , curAtkEntity: {curAutoBattleAtkEntity}");

            if (searchResult)
            {
                searchEmptyTime = 0;
                GotoFightEnemy();
            }
            else
            {
                // 怪物查找不到 的时间  大于 30s
                if ((TimeUtils.ClientNowStampMilli - searchEmptyTime) > 30 * 1000)
                {

                    // 第一次找不到, 只是 赋值 , 然后 记录当前时间
                    if (searchEmptyTime == 0)
                    {
                        searchEmptyTime = TimeUtils.ClientNowStampMilli;
                    }
                    else
                    {
                        searchEmptyTime = 0;
                        // SGF.Debuger.LogError($"{AUTOBATTLEKEY} ,过了 很久 找不到怪物, 准备 回到起始点 : {startPos}");
                    }
                }

                // SGF.Debuger.LogError($"{AUTOBATTLEKEY} , 找不到怪物, 准备 回到目标点 : {startPos}");
                GotoTargetPos();
            }
        }

        #region  
        /// 2023/09/18
        /// 本想 实现设计 [多个目标点] 逻辑, 后面发现 在 设置 目标点 的时候, 其实并不好区分 这个
        ///     目标点 是 [新增] 的 还是 说 是对之前 这种类型 目标点的 [更新].
        ///     这个设计 的麻烦之处在于 需要 外部接口 在调用的时候, 明确的告知 此次设置目标点是 [新增] 
        ///     还是对之前相同类型目标点的 [更新] 操作. 同时还需要告知 此次的更新 是否属于 [新增] 从而需要立即执行
        /// 
        ///     这设计反而增加了使用复杂 度, 还不如 按优先级 对 目标点的顺序 排序, 依次查找. 

        /// <summary>
        /// 设置的目标点的 数据
        /// </summary>
        // Dictionary<AutoBattleSetTargetPosType, UnityEngine.Vector3> type2TargetPos = new();

        /// <summary>
        /// 设置 目标点 类型的 list. 
        /// 此处 设计上 考虑 支持 设置多个 类型 目标点的拓展. 
        /// 所以 采用 List 存储 设置目标点的类型 + Dictionary 存储目标点 的方式.
        /// note:
        ///     此处 没有使用 Stack 的 原因是 为了支持 取消 之前设置的 目标点数据 .
        /// eg:
        ///     如 依次设置了 目标点类型 A,B,C. 
        ///     那么 目标点的 执行 顺序 为 C--->B--->A。
        ///     在C ---> B 的过程中, 可能需要 更新 或者终止A 的目标点.  所以 此处不适合 直接采用 stack.
        ///     而是 使用 list + dictionary 处理.
        /// </summary>
        // List<AutoBattleSetTargetPosType> setTargetTypeList = new List<AutoBattleSetTargetPosType>();

        // private void SetFindTargeet(AutoBattleSetTargetPosType posType, UnityEngine.Vector3 pos)
        // {

        // }
        #endregion

        private bool IsGotoRecordStartPos()
        {
            return !CheckInPlayerArea(recordStartPos, 0.5f);
        }

        private void GotoRecordStartPos()
        {
            SGF.Debuger.Log($"[FindPath] GotoRecordStartPos : goto ---> {recordStartPos}");

            TryGoToTargetPos((result) =>
                {
                    SGF.Debuger.Log($"[FindPath] GotoRecordStartPos : {recordStartPos},  result ---> {result}");

                }, E_FindPathType.AutoBattle, recordStartPos);
        }


        private bool OverDistance(float distance)
        {
            return (recordStartPos - M_MainPlayerCtrlBase.M_Curr.Position()).sqrMagnitude > distance * distance;
        }


        private UnityEngine.Vector3 movingToTargetPos = UnityEngine.Vector3.zero;




        /// <summary>
        /// 去到指定 的目标点.
        /// 目前的 目标点 有 2种:
        /// 1. 摇杆拖动时 的 落点.
        /// 2. 副本或者 其它 事件 设置的 目标点.
        /// 
        /// 基础逻辑 是 怪物找不到的时候, 检测 是否有 目标点, 如果没有， 就 找摇杆的 最近一次落点.
        /// 如果 在移动 到 目标点 的过程中 拖动了 摇杆, 那 摇杆结束后, 需要检测到 能否打怪.  
        /// 不能打怪,  移动 到  目标点.
        /// 
        /// 如果在 打怪过程中, 刷新 了新的 目标点, 打完怪物后 再移动到 新的目标点
        /// </summary>
        private void GotoTargetPos()
        {
            // 如果是正在自动寻路, 那回到目标点的 这个逻辑先停止
            if ((GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup).Is_FindPathing)
            {
                return;
            }

            // 如果有正在播放的 技能实体, 那就先不走
            if ((GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup).IsPlayingSkill)
            {
                return;
            }

            /// 2024/3/28
            /// fix: 
            ///     用来修复 自动战斗找不到怪物的时候会去一个很远的点 战斗的问题.
            /// note:
            ///     bug 未复现
            /// 方案:
            ///     增加一条规则2. 如果自动战斗的目标点大于当前位置 15m, 就将当前点设置为 摇杆位置的记录点
            if (OverDistance(15f))
            {
                // SGF.Debuger.LogError($"[BattleManager] starPos: {recordStartPos} , 太远, 准备将当前点 {M_MainPlayerCtrlBase.M_Curr.Position()} 设置为记录点 ");
                RecordAutoBattleStartPos(true);
                return;
            }

            if (IsGotoSecretAreaPoint(out Vector3 point))
            {
                GotoSecretAreaPoint(point);

                return;
            }


            // 首先判断是否 需要到达副本目标点
            // if (IsGotoInstanceGoalID())
            // {
            //     GotoSpawner();
            //     return;
            // }

            if (IsGotoRecordMove2Pos())
            {
                GotoRecordMove2Pos();
                return;
            }

            if (IsGotoRecordStartPos())
            {
                GotoRecordStartPos();
                return;
            }

        }

        private void TryGoToTargetPos(Action<bool> ac, E_FindPathType findPathType, Vector3 targetPos, float maxDistance = 0.1f)
        {
            Vector3 curPoint = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

            if (CheckInArea(curPoint, targetPos, 0.5f))
            {
                return;
            }
            movingToTargetPos = targetPos;

            // SGF.Debuger.Log($"[FindPath] [{findPathType}] goto ---> {movingToTargetPos}");
            if (movingToTargetPos == Vector3.zero)
            {
                return;
            }


            // ShowPoint(1, movingToTargetPos);
            bool findResult = (M_MainPlayerCtrlBase as PlayerCtrlGroup).TryFindPath(movingToTargetPos, maxDistance, (result) =>
            {
                movingToTargetPos = UnityEngine.Vector3.zero;
                ac?.Invoke(result);

            }, false, false, findPathType);


            if (!findResult)
            {
                movingToTargetPos = UnityEngine.Vector3.zero;
                ac?.Invoke(false);
            }
        }

        private Vector3 GetSearchStartPoint()
        {
            // 如果是个人秘境的自动战斗, 他的索敌 点 就采用 玩家啊坐标点
            if (IsSercetSpace())
            {
                return M_MainPlayerCtrlBase.M_Curr.Position();
            }

            return recordStartPos;
        }

        public bool SearchAutoBattleClosetTarget(out ulong entityId)
        {
            // 如果这个怪物 还能够被打, 那就一直 打这个怪物
            if (IsValidEnemyEntity(curAutoBattleAtkEntity))
            {
                entityId = curAutoBattleAtkEntity;
                return true;
            }

            // 如果没有怪物了或者怪物 不能被打了, 那就 重新索敌
            EntityCtrlBase mainPlayer = M_MainPlayerCtrlBase;

            Vector3 startPoint = GetSearchStartPoint();

            // 取的搜索中心点 不再使用 玩家半径, 而改为 startPos
            NPCEntityBase findEnemy = SkillUtils.GetClosestTargetByStartPos(GameManager.Instance.mainPlayerId, mainPlayer.M_Curr.Faction,
                startPoint, mainPlayer.M_Curr.Position(), searchEnemyRadio, SelectType.Enemy, true,
                    (entity) =>
                    {
                        if (IsValidEnemyEntity(entity.EntityId))
                        {
                            return true;
                        }
                        return false;
                    });

            // 如果 找不到 怪物了,直接 返回 结果为 false
            if (findEnemy == null)
            {
                entityId = 0;
                return false;
            }

            // 如果找到了 怪物,因为 find 的时候已经做了 IsValidEntity 的判定,所以此处就认为已经找到了
            entityId = findEnemy.EntityId;
            return true;
        }
        private bool SearchEnemy()
        {
            // 如果这个怪物 还能够被打, 那就一直 打这个怪物
            if (IsValidEnemyEntity(curAutoBattleAtkEntity))
            {
                return true;
            }

            // 如果没有怪物了或者怪物 不能被打了, 那就 重新索敌
            EntityCtrlBase mainPlayer = M_MainPlayerCtrlBase;

            Vector3 startPoint = GetSearchStartPoint();

            // 取的搜索中心点 不再使用 玩家半径, 而改为 startPos
            NPCEntityBase findEnemy = SkillUtils.GetClosestTargetByStartPos(GameManager.Instance.mainPlayerId, mainPlayer.M_Curr.Faction,
                startPoint, mainPlayer.M_Curr.Position(), searchEnemyRadio, SelectType.Enemy, true,
                    (entity) =>
                    {
                        if (IsValidEnemyEntity(entity.EntityId))
                        {
                            return true;
                        }
                        return false;
                    });

            // 如果 找不到 怪物了,直接 返回 结果为 false
            if (findEnemy == null)
            {
                curAutoBattleAtkEntity = 0;
                return false;
            }

            // 如果找到了 怪物,因为 find 的时候已经做了 IsValidEntity 的判定,所以此处就认为已经找到了
            curAutoBattleAtkEntity = findEnemy.EntityId;
            return true;
        }

        /// <summary>
        /// 检查 怪物 是否有效
        /// 如果这个怪物 死亡了 或者 走不到这个怪物的面前, 都认为 怪物不可被打
        /// </summary>
        /// <returns></returns>
        private bool IsValidEnemyEntity(ulong entityID)
        {
            EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(entityID);

            // 如果找不到这个怪物,直接就 不能 fight
            if (entityCtrlBase == null)
            {
                return false;
            }

            // 如果 怪物 死亡的话, 那就换下一个
            if (!entityCtrlBase.M_Curr.M_IsAlive)
            {
                return false;
            }

            var curPos = M_MainPlayerCtrlBase.M_Curr.Position();
            var targetPos = entityCtrlBase.M_Curr.Position();

            // 检查 能不能走到这个 怪物身边
            if (!FindPathManager.Instance.CheckWalkable(curPos, targetPos))
            {
                /// 2024/3/19
                /// 发现有些怪物策划会配置在一个不可到达区域，但是需要可以攻击到
                /// 所以:
                ///     增加 一个攻击距离判定, 如果满足当前的攻击距离, 就认为怪物可以被攻击,怪物有效
                if (CheckTargetPosIsInSkillArea(curPos, targetPos, entityCtrlBase.M_Curr.ModelRadius))
                {
                    return true;
                }
                return false;
            }

            // 判定阵营
            bool isTargetNtt = EntityFactoryUtils.CheckIsTriggleAOIEntity(entityCtrlBase.M_Curr, M_MainPlayerCtrlBase.M_Curr.EntityId, M_MainPlayerCtrlBase.M_Curr.Faction, SelectType.Enemy);

            if (!isTargetNtt)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 走到怪物旁边 砍怪
        /// </summary>
        private void GotoFightEnemy()
        {
            movingToTargetPos = UnityEngine.Vector3.zero;

            ignoreSkillPoss.Clear();




            // 先找到 当前可以释放的技能槽, 如果能够找到 这个技能槽, 就按技能 释放范围砍 砍怪
            SkillContainer skillContainer = GetAutoBattleCanUseSkillContainer();

            // 如果 一个技能 都无法释放, 说明当前 玩家当前 存在 活跃技能. 此时 等到玩家进入非活跃后,
            // 播放下个技能
            if (skillContainer == null)
            {
                return;
            }

            var curPos = M_MainPlayerCtrlBase.M_Curr.Position();

            var atkEnemy = GameManager.Instance.GetEntityCtr(curAutoBattleAtkEntity);

            // atkEnemy.
            var direction = atkEnemy.M_Curr.Position() - curPos;
            direction.y = 0;


            // 检查的技能槽重新赋值
            SkillContainer checkSkillContainer = skillContainer;

            /// 首先判断 是否 需要向怪物移动, 检查 一下 技能的距离 和 玩家的距离
            /// note:
            ///     1.此处 汉华 增加了 一个 摇杆拖拽 逻辑, 可以用来调整 自动战斗的 站位;
            ///     2.需要 判断如下:
            ///         (1). 先判断 当前释放的技能 是否释放, 能够 释放, 就直接释放.
            ///         (2). 本达到距离, 就走到 能够释放的 位置处释放.
            /// 
            ///     3.如果 没有找到 合适的怪物, 就回到 一开始摇杆的 拖拽点.
            /// 
            do
            {
                // 如果找到了 技能
                int skillPos = checkSkillContainer.PosID;
                //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , GotoFightEnemy 准备走到怪物面前 释放 技能槽: {skillPos}");



                bool isInSkillArea = checkSkillContainer.CurShowSkillInfo.CheckIsSkillArea(direction.sqrMagnitude, atkEnemy.M_Curr.ModelRadius);

                // 如果不再一个技能区域内, 那就结束 这个循环, 走后续逻辑
                if (!isInSkillArea)
                {
                    break;
                }

                // 如果在区域内, 并且释放成功, 那就不走后续逻辑
                if (AutoBattleUseSkill(checkSkillContainer))
                {
                    // 开启 一次全局技能cd
                    StartGlobalSkillCD();

                    ignoreSkillPoss.Clear();
                    return;
                }

                // 如果释放失败, 那就将 技能放入忽视队列
                ignoreSkillPoss.Add(skillPos);
                checkSkillContainer = GetAutoBattleCanUseSkillContainer(ignoreList: ignoreSkillPoss);

                // 如果 早不到后续需要释放的技能了, 那就 结束循环, 走后续逻辑
                if (checkSkillContainer == null)
                {
                    break;
                }

                // SGF.Debuger.LogError($" {checkSkillContainer.CurSkillId} 释放失败, 走到一个新的技能区域");
            } while (checkSkillContainer != null);

            ignoreSkillPoss.Clear();



            // 如果 正在向怪物 移动, 那就直接return
            if (isMovingToEnemy)
            {
                return;
            }

            // 如果不在 技能范围内, 那就走到 技能的 区域的释放中心点
            float autoSkillDis = skillContainer.CurShowSkillInfo.SkillAutoUseDistance / 100f;
            //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , GotoFightEnemy 选择释放技能槽 : {skillPos} , 技能释放距离: {autoSkillDis} , 准备 寻路去干 curAtkEntity: {curAtkEntity}");

            ShowTargetPoint(0, atkEnemy.M_Curr.Position());

            Vector3 dir = atkEnemy.M_Curr.Position() - curPos;
            Vector3 dirNor = dir.normalized;
            Vector3 skill = curPos + dir - (autoSkillDis * dirNor);

            FindPathManager.Instance.FindDistanceValidPoint(skill, autoSkillDis, out Vector3 targetPosition);
            ShowTargetPoint(1, targetPosition);

            isMovingToEnemy = true;
            bool findResult = (M_MainPlayerCtrlBase as PlayerCtrlGroup).TryFindPath(targetPosition, autoSkillDis, OnCompeleteFindPath, false, false, E_FindPathType.AutoBattle);


            // 如果寻路失败, 就设置寻路状态为 false
            if (!findResult)
            {
                isMovingToEnemy = false;
            }

        }

        private void ShowTargetPoint(int i, Vector3 pos)
        {
            ShowRecordPoint(pos, $"targetPoint_{i}", i == 0 ? Color.red : Color.green);
        }



        /// <summary>
        /// 检查 怪物坐标是否 在技能的 范围之内. 此时不关心这个技能槽是否可以释放
        /// <param name="targetPos"></param>
        /// <param name="targetRadius">目标的半径</param>
        /// <returns></returns>
        bool CheckTargetPosIsInSkillArea(Vector3 curPos, Vector3 targetPos, float targetRadius)
        {
            Vector3 direction = targetPos - curPos;
            direction.y = 0;

            float sqrMagnitude = direction.sqrMagnitude;

            for (int i = 0; i < skillPoss.Count; i++)
            {
                var skillPos = skillPoss[i];

                SkillContainer skillContainer = GetSkillPosContainer(skillPos);

                // 技能槽 可能没有技能
                if (skillContainer == null || skillContainer.CurShowSkillInfo == null)
                {
                    continue;
                }

                // 检查 sqrMagnitude 是否在 技能范围内, 如果在, 就说明这个怪物 在玩家的技能释放范围内
                // note:
                //      有时候 自动战斗会出现 在原地放了一个技能,但是 朝向没有转过去的现象.
                //      原因是 技能能否释放的 判定范围是 AICastDistance , 但是自动转向的判定范围是 轮盘范围 WheelRange.MaxRadius。
                //      理论上 轮盘范围 配置要小于 技能范围的配置. 
                // 如果出现上面的 现象，就表明策划配置错了,需要策划改配置
                bool isInSkillArea = skillContainer.CurShowSkillInfo.CheckIsSkillArea(sqrMagnitude, targetRadius);

                if (isInSkillArea)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 寻路后的回调
        /// </summary>
        /// <param name="moveResult">到达目标后的结果,目前看到达目标点的附近结束会返回 false</param>
        private void OnCompeleteFindPath(bool moveResult)
        {
            isMovingToEnemy = false;

            // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] 移动到 目标点");

            MarkDirtyState(true);
        }

        /// <summary>
        /// 状体 标脏. 在 自动寻路或者 释放技能过程中, 依旧会有 事件通知过来 触发 自动战斗.
        /// 这个时候 将状态标脏. 
        /// 等下一次 自动战斗 OnTick的时候 再去触发 索敌
        /// </summary>
        /// <param name="isDirty"></param>
        private void MarkDirtyState(bool isDirty)
        {
            // 标脏之后就清除 thinkingTimes 次数
            if (isDirty)
            {
                // thinkingTimes = 0;
            }
            dirtyAutoSkill = isDirty;
        }

        /// <summary>
        /// 释放技能的接口, 一个是 走到一个目标节点后 使用技能, 一个 是 技能 释放完毕后 使用技能
        /// </summary>
        private bool AutoBattleUseSkill(SkillContainer skillContainer)
        {
            // 准备释放技能, 先取消标脏
            MarkDirtyState(false);
            // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] AutoBattleUseSkill false");


            // 如果没有活跃技能的清空下, 就正常的释放 技能
            // 尝试 释放技能
            bool useSkillResult = TryUseSkill(skillContainer);

            // 如果使用技能失败了, 设置一段时间后 重试
            if (!useSkillResult)
            {

            }
            else
            {
                // (M_MainPlayerCtrlBase as PlayerCtrlGroup)?.BreakFindPath();
                // (M_MainPlayerCtrlBase as PlayerCtrlGroup)?.BreakFollowDynamicEnity();
                // isddddddddddddddddddMovingToEnemy = false;
                //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} 使用 技能成功");
            }

            // 不是普攻才标脏
            //SGF.Debuger.Log($"{AUTOBATTLEKEY} , 使用技能 {skillContainer.CurSkillId}, useSkillResult: {useSkillResult}, 标脏 ");
            skillContainer.MarkAutoBattleCD();

            return useSkillResult;
        }

        private HashSet<int> ignoreSkillPoss = new();

        private List<int> ForbidSkillPoss = new();

        public List<int> GetForbidSkillPoss()
        {
            return ForbidSkillPoss;
        }

        public void ForbidSkillPos(int pos, bool isForbid)
        {
            if (isForbid)
            {
                if (!ForbidSkillPoss.Contains(pos))
                {
                    ForbidSkillPoss.Add(pos);
                }
            }
            else
            {
                if (ForbidSkillPoss.Contains(pos))
                {
                    ForbidSkillPoss.Remove(pos);
                }
            }
        }

        public bool GetIsForbiddeSkillPos(int skillPos)
        {
            return ForbidSkillPoss.Contains(skillPos);
        }
        // public bool IsPrePlayClientSkill => !GMTestData.CheckGmIsOpen("ClosePrePlaySkill");
        /// <summary>
        /// 是否 开启 客户端的 预播放
        /// </summary>
        public bool IsPrePlayClientSkill => true;


        /// <summary>
        /// 尝试 使用技能.
        /// 1. 如果 
        /// note:
        ///     1.如果 不考虑技能释放的距离问题, 那 只有在 玩家技能的 非活跃阶段才能释放技能, 否则 返回的都是false
        ///     
        /// /// </summary>
        /// <returns></returns>
        private bool TryUseSkill(SkillContainer skillContainer)
        {

            // ignoreSkillPosList.Clear();


            // do
            // {
            // 当前使用技能的技能槽 posID
            int skillPos = skillContainer.PosID;

            bool useResult = UseSkill(skillContainer);

            // 如果未开启预播, 那只是发送协议, 检查永远是失败的, 所以 返回 !IsPrePlayClientSkill
            bool checkTryUseResult = !IsPrePlayClientSkill || CheckTryUseSkillResult(skillContainer);


            //SGF.Debuger.Log($"{AUTOBATTLEKEY} , try useSkill ====> {skillPos} 结果 useResult: {useResult}, checkTryUseResult: {checkTryUseResult}.");

            // 如果 useResult 认为释放成功, 并且 checkTryUseResult 检查 释放也成功, 那就认为此次技能释放是成功的
            if (useResult && checkTryUseResult)
            {
                //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , try useSkill ====> {skillPos} success!!!");
                return true;
            }

            // 如果释放失败了, 那就 继续查找下一个能够释放的技能
            // ignoreSkillPosList.Add(skillPos);
            // 捕获到了 曲爷的按钮把自动战斗中 技能使用的指令给 吞了， 释放技能失败， 执行了 debug.break。  目前可以屏蔽 这个异常, 流程会自动 进行到下个技能;
            // SGF.Debuger.LogWarning($"{AUTOBATTLEKEY} , try useSkill ====> {skillPos} 失败,结果 useResult: {useResult}, checkTryUseResult: {checkTryUseResult}, 准备释放下一个技能槽的技能. 十有八九 技能没怪物, 曲爷 按钮把技能拦了.");

            // skillContainer = GetAutoBattleCanUseSkillContainer(ignoreList: ignoreSkillPosList);

            // // 如果已经找不到可以释放的 自动战斗的技能了, 那就 释放失败
            // if (skillContainer == null)
            // {
            //     return false;
            // }

            // } while (true);

            return false;
        }

        private SkillContainer GetSkillPosContainer(int skillPos)
        {
            SkillUnitController skillUnitController = (M_MainPlayerCtrlBase.M_Curr as HeroEntityBase).skillDispatcher.SkillUnitController;

            if (skillUnitController.SkillContainerDic.TryGetValue(skillPos, out SkillContainer skillContainer))
            {
                return skillContainer;
            }

            return null;
        }

        /// <summary>
        /// 刷新 技能位对应 技能的 缓存
        /// </summary>
        public void RefreshCacheSkillPosContainers()
        {
            if (!IsOpenAutoBattle)
            {
                return;
            }
            cacheSkillPosContainers.Clear();

            skillPoss.ForEach((posID) =>
            {
                var skill = GetSkillPosContainer(posID);
                if (skill != null)
                {
                    cacheSkillPosContainers.Add(skill);
                }
            });
        }

        private List<SkillContainer> tempSkillContainerLists = new();
        /// <summary>
        /// 获得自动战斗可以使用的技能槽
        /// </summary>
        /// <param name="ignoreList"></param>
        private SkillContainer GetAutoBattleCanUseSkillContainer(HashSet<int> ignoreList = null)
        {
            SkillContainer skillContainer = null;

            // 自动战斗一个技能能否释放, 需要首先判定当前是否有活跃的技能.
            // 如果没有活跃技能, 那就可以释放;
            // 如果存在活跃技能:
            //     1.首先判定当前活跃的技能是否是 普攻/蓄力
            //          如果有这种输入轴的技能,那就可以是用 普攻/蓄力;
            //     2.如果只是普攻的 活跃技能
            //          需要等技能 由 活跃--->非活跃时 出发再次检查是否能够使用技能

            // 如果当前技能活跃
            bool isCurSkillActive = CheckCurIsActiveSkill();

            if (isCurSkillActive)
            {
                // 如果当前是 活跃,但又不是普攻, 那此时没法释放技能
                if (!CheckCurIsNormalActiveSkill())
                {
                    return null;
                }
            }

            var posID = 0;
            bool isIgnore = false;

            bool isForbid = false;

            tempSkillContainerLists.Clear();

            for (int i = 0; i < cacheSkillPosContainers.Count; i++)
            {
                skillContainer = cacheSkillPosContainers[i];
                posID = skillContainer.PosID;

                isIgnore = CheckIsIgnorePosID(posID, ignoreList);

                // 如果在忽视队列, 那就跳过这个技能槽
                if (isIgnore)
                {
                    continue;
                }

                // 判定是否在禁止队列
                isForbid = GetIsForbiddeSkillPos(posID);

                if (isForbid)
                {
                    continue;
                }
                // 判定一下技能按钮是否可以点击
                if (!skillContainer.IsSkillCanClick())
                {
                    continue;
                }
                if (CheckCanUseSkill(skillContainer) && CheckCanPostKeyCode(skillContainer.PosID))
                {

                    bool isUserInputNormalSkill = skillContainer.IsCanUseUserInputNormalSkill();

                    if (isUserInputNormalSkill)
                    {
                        tempSkillContainerLists.Insert(0, skillContainer);
                    }
                    else
                    {
                        tempSkillContainerLists.Add(skillContainer);
                    }
                }
            }

            var count = tempSkillContainerLists.Count;
            // 如果找到了 可以释放的 技能
            if (count > 0)
            {
                skillContainer = tempSkillContainerLists[0];
                tempSkillContainerLists.Clear();

                /// 1.如果当前最高级 的技能是 普攻 并且有 用户输入, 那就直接用这个 普攻
                /// 2.如果是普攻第一段, 普攻优先级最低, 现在是第一个,说明 其它技能都在cd, 直接放
                /// 3.不是普攻, 判定是否在 全局cd, 如果全局cding, 那直接renturn
                if (IsGlobalSkillCDIng() && !skillContainer.IsNormalSkill())
                {
                    return null;
                }

                return skillContainer;
            }

            return null;
        }

        private bool CheckIsIgnorePosID(int checkPosID, HashSet<int> ignoreList = null)
        {
            // 没有忽视列表数据,那就不需要忽视
            if (ignoreList == null || ignoreList.Count == 0)
            {
                return false;
            }

            return ignoreList.Contains(checkPosID);

        }

        /// <summary>
        /// 检查 使用 技能的 结果 到底是否成功
        /// 
        ///     曲: 不愿意 增加按钮执行指令的结果，所以这是 临时增加的方案:
        /// note1: 
        ///     技能按钮存在 自己拦截技能点击的逻辑,所以 可能存在 自动战斗 判断技能能够使用,但是被技能按钮拦截的情况. 
        ///     此时，自动战斗并不清楚 技能是否 真正的释放成功. 
        ///     所以 此处增加一个 接口 用来在  TryUseSkill 后再次检查技能释放 是否成功.   
        /// 
        /// note2:
        ///     由于需要释放的技能 可能有 非活跃技能, 所以此处技能释放的 成功与否 只考虑 活跃技能
        /// </summary>
        private bool CheckTryUseSkillResult(SkillContainer skillContainer)
        {
            // 普工默认为 释放即成功
            if (skillContainer.PosID == 2)
            {
                return true;
            }
            // 如果 这个技能 是 非活跃技能,那 在TryUseSkill 的时候 是可以释放成功的
            if (!skillContainer.IsNeedActive())
            {
                return true;
            }

            // 如果 这个技能 已经释放了，但是发现 还是可以 再释放一次，那说明之前的释放应该是被 曲爷 的按钮拦截了,所以此处认为之前释放失败
            if (CheckCanUseSkill(skillContainer))
            {
                return false;
            }

            return true;
        }



        private bool CheckCanUseSkill(SkillContainer skillContainer)
        {
            SkillController skillController = (M_MainPlayerCtrlBase.M_Curr as HeroEntityBase).skillDispatcher.SkillController;


            return skillContainer != null && skillController.CheckCanAutoBattleUseSkill(skillContainer);
        }

        /// <summary>
        /// 检查当前是否是 活跃技能状态
        /// </summary>
        /// <returns></returns>
        private bool CheckCurIsActiveSkill()
        {
            SkillController skillController = (M_MainPlayerCtrlBase.M_Curr as HeroEntityBase).skillDispatcher.SkillController;

            return skillController.IsSkillActive;
        }

        /// <summary>
        /// 当前是否是活跃的 非普攻技能
        /// </summary>
        private bool CheckCurIsNormalActiveSkill()
        {
            SkillController skillController = (M_MainPlayerCtrlBase.M_Curr as HeroEntityBase).skillDispatcher.SkillController;

            return skillController.IsSkillActive && skillController.ActiveSkillEntity.IsNormalSkill;
        }

        /// <summary>
        /// 检查是否可以 发送 技能槽 对应点击事件的 keyCode
        /// </summary>
        /// <param name="skillPos"></param>
        /// <returns></returns>
        private bool CheckCanPostKeyCode(int skillPos)
        {
            // 配置中是否 有这个玩意
            return pos2KeyCode.ContainsKey(skillPos);
        }

        /// <summary>
        /// 使用 对应的技能槽
        /// </summary>
        /// <param name="skillContainer"></param>
        private bool UseSkill(SkillContainer skillContainer)
        {
            SkillPos skillPos = skillContainer.skillPos;

            var posID = skillPos.PosID;
            if (pos2KeyCode.TryGetValue(posID, out KeyValuePair<KeyCode, int> keyCodePair))
            {
                // SGF.Debuger.LogError($"{AUTOBATTLEKEY} , UseSkill posID: {posID} , skillID: {skillContainer.CurSkillId}, keyCode: {keyCodePair.Key} , code: {keyCodePair.Value}");

                // 模拟点击 keyCode 对应的 技能按钮
                {
                    InputManager.Instance.DispatchVKey(keyCodePair.Value, 2, true);


                }

                return true;
            }

            return false;
        }

        private void OnHeroDie(object data)
        {
            ulong entityID = (ulong)data;

            if (entityID == GameManager.Instance.mainPlayerId)
            {
                //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnHeroDie entityID: {entityID} 主角死亡,执行 Stop ");
                // 主角死亡的时候， 自动战斗不关闭, 但是 后续逻辑不执行
                // Stop();
            }
            else
            {
                if (entityID == curAutoBattleAtkEntity)
                {
                    curAutoBattleAtkEntity = 0;
                    //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnHeroDie entityID: {entityID} 索敌怪物死亡,设置 curAtkEntity=0, 标脏 ");
                }
                MarkDirtyState(true);
                // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] OnHeroDie ");

                // boss 单独处理
            }
        }

        private void OnLeaveAoi(object data)
        {
            ulong entityID = (ulong)data;
            if (entityID == GameManager.Instance.mainPlayerId)
            {
                //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnLeaveAoi entityID: {entityID} 主角离开 AOI ,执行 Stop ");

                // Stop();
            }
            else
            {
                if (entityID == curAutoBattleAtkEntity)
                {
                    //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnLeaveAoi entityID: {entityID} 索敌怪物离开 AOI, BreakCurGotoFightEnemy ");
                    BreakCurGotoFightEnemy();
                    MarkDirtyState(true);
                    // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] OnLeaveAoi ");

                }
            }
        }

        private void OnEnterAOI(object data)
        {
            ulong entityID = (ulong)data;
            if (entityID != GameManager.Instance.mainPlayerId)
            {
                //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnEnterAOI entityID: {entityID} 进入 AOI , 只执行标脏, 等待下一次 tick ");

                MarkDirtyState(true);
                // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] OnEnterAOI ");

            }
        }

        private void OnEctype(object data)
        {

            EnterSpaceNtf enterSpace = (EnterSpaceNtf)data;

            //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnEctype 进入/退出副本 isEnterEctype: {isEnterEctype} ,执行 Stop ");
            UpdateMapFrobidState(enterSpace.SpType, enterSpace.MapID);

            // Stop();
        }

        private void OnEctypeModuleGameEnd(object data)
        {
            // bool isEnterEctype = (bool)data;

            //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnEctypeModuleGameEnd 进入/退出副本 isEnterEctype: {isEnterEctype} ,执行 Stop ");

            // Stop();
        }

        private void OnMapPreloadNotice(object data)
        {
            //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnMapPreloadNotice 进入/退出地图,执行 Stop ");

            MapPreloadNotice mapPreloadNotice = (MapPreloadNotice)data;

            UpdateMapFrobidState(mapPreloadNotice.SpType, mapPreloadNotice.MapID);

            // Stop();
        }

        private void OnLeaveSpace(object data)
        {
            //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnLeaveSpace 进入/退出地图,执行 Stop ");

            // 离开地图的时候 不停止自动战斗
            // Stop();
        }

        /// <summary>
        /// 当点击任务按钮 开启任务的时候, 停止自动战斗
        /// </summary>
        /// <param name="data"></param>
        private void OnClickTask(object data)
        {
            // SGF.Debuger.Log($"[FindPath] battleManager OnClickTask 点击了任务");

            // Stop();
        }

        private void OnNoneActiveSkill(object data)
        {
            //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnNoneActiveSkill 主角 进入 非活跃 阶段 ,执行  SearchEnemyFight(); ");
            MarkDirtyState(true);
            // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] OnNoneActiveSkill ");

        }

        private void OnSkillEndCD(object data)
        {
            int skillID = (int)data;
            //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnSkillEndCD 主角 技能 skillID: {skillID} 结束CD ,执行  SearchEnemyFight(); ");

            MarkDirtyState(true);
            // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] OnSkillEndCD ");

        }

        /// <summary>
        /// 记录一下自动战斗计算的 中心点
        /// </summary>
        /// <param name="clearAtkTarget"></param> 
        public void RecordAutoBattleStartPos(bool clearAtkTarget)
        {
            // 如果清除当前的攻击目标
            if (clearAtkTarget)
            {
                curAutoBattleAtkEntity = 0;
            }
            recordStartPos = M_MainPlayerCtrlBase.M_Curr.Position();
            //SGF.Debuger.LogError($"{AUTOBATTLEKEY} , RecordAutoBattleStartPos 记录自动战斗的起始坐标: {recordStartPos}");
        }

        public void RecordAutoBattleSpecialStartPos(Vector3 pos)
        {
            recordStartPos = pos;
        }

        private void OnAnalogStickUp(object data)
        {
            /// 如果 正在自动战斗， 此时 就用摇杆抬起的坐标. 
            /// note:
            ///     如果此时 还在释放技能过程中, 如果原子锁锁住了, 那移动摇杆的操作 不会对坐标产生影响.
            ///     相对于的 此处 也不需要记录 摇杆拖动的 坐标。
            ///     所以 此处通过 原子锁 判断 是否需要记录 按钮抬起的 逻辑
            // if (IsAutoBattling && !M_MainPlayerCtrlBase.M_Curr.Data.Is___ForbidMove)


            // 当摇杆抬起的时候,记录 起始点
            RecordAutoBattleStartPos(true);

            // 自动战斗
            if (IsAutoBattling)
            {
                //    SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnAnalogStickUp [stick] 抬起滑杆: {autoBattleState}");


                UpdateBateState(AutoBattleState.HoldOn, false);
                MarkDirtyState(true);


                // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] OnAnalogStickUp ");
                // 摇杆抬起的时候, 记录摇杆附近的点
                RecordCurMonsterPoint();
            }
        }

        /// <summary>
        /// 左摇杆 拖动时，左摇杆的优先级 要 大于 右摇杆 自动战斗的技能. 
        /// 所以 左摇杆 移动时， 如果 自动战斗 在释放技能, 那就 释放技能结束后 自动战斗 索敌逻辑 中止.
        /// 等到 摇杆 抬起后 重新索敌
        /// </summary>
        /// <param name="data"></param>
        private void OnAnalogStickScrolling(object data)
        {
            if (IsAutoBattling && !M_MainPlayerCtrlBase.M_Curr.Data.Is___ForbidMove)
            {
                BreakCurGotoFightEnemy();


                //  SGF.Debuger.LogError($"{AUTOBATTLEKEY} , OnAnalogStickScrolling [stick] 移动滑杆: 关闭自动战斗");
            }

            // 只要摇杆一拖动, 状态就设置位 抬起状态, 此时并不需要关心玩家是否被 原子锁锁住移动
            UpdateBateState(AutoBattleState.HoldOn, true);



        }

        /// <summary>
        /// socket 断开后,  重新进入登录界面,此时 mainPlayer 找不到, 所以需要停止 自动战斗
        /// </summary>
        /// <param name="data"></param>
        private void OnReLogin(object data)
        {
            // gl 提的 下线之后重新登录 也需要保留自动战斗的状态
            // Stop();
        }

        private void OnTaskItemSelect(object data)
        {
            // int taskItem = (int)data;
            SGF.Debuger.Log($"[FindPath] battleManager OnTaskItemSelect 点击了任务");

            // 先做的 需求是 自动战斗点击任务时 停止自动战斗, 后面 版本可能时 先完成任务再继续自动战斗
            // Stop();
        }

        /// <summary>
        /// 主角 再副本里做任务的 寻路, 打断 自动寻路
        /// </summary>
        /// <param name="data"></param>
        private void OnFindPathByEctype(object data)
        {
            // SGF.Debuger.Log($"[FindPath] battleManager OnFindPathByEctype 副本任务寻路");

            // Stop();
        }

        private void OnFindPath(object data)
        {
            bool result = (bool)data;
            SGF.Debuger.Log($"[FindPath] battleManager OnFindPath result: {result}, 寻路类型: {(M_MainPlayerCtrlBase as PlayerCtrlGroup).Cur_FindPathType} ");

            if ((M_MainPlayerCtrlBase as PlayerCtrlGroup).Cur_FindPathType != E_FindPathType.AutoBattle)
            {
                RecordAutoBattleStartPos(true);

                UpdateBateState(AutoBattleState.OtherFindingPath, result);

                // 清除 当前自动战斗 的索敌目标
                ClearSerarchTargets();
            }

        }

        private void OnAvgStart(object data)
        {
            bool result = (bool)data;
            // SGF.Debuger.LogError($"[AutoBattle] battleManager OnAvgStart  : {result}");

            // Stop();
            UpdateBateState(AutoBattleState.HoldOn_AVG, result);

        }


        private void OnMapChangeRet(object data)
        {
            MapChangeRet mapChangeRet = (MapChangeRet)data;

            UpdateMapFrobidState(mapChangeRet.SpType, mapChangeRet.MapID);

            // 清理之前自动战斗的一些 记录
            ClearAutoBattleRecord();
        }

        private void UpdateMapFrobidState(SpaceType spaceType, int mapID)
        {
            // if (spaceType == SpaceType.SpaceDefault || spaceType == SpaceType.SpaceScene)
            // {
            //     UpdateBateState(AutoBattleState.Forbid, false);
            //     return;
            // }

            bool isForbid = !GetSceneOpenAutoBattle(mapID);
            UpdateBateState(AutoBattleState.Forbid, isForbid);
        }

        private bool GetSceneOpenAutoBattle(int sceneID)
        {
            if (scene2AutoBattleMap.ContainsKey(sceneID))
            {
                return scene2AutoBattleMap[sceneID];
            }

            return true;
        }

        #endregion

        #region 实体关系头顶信息图标

        public class EntityRelationIconPath
        {
            public string _hpBg = "";
            public string _hpBgCheck = "";
            public string _hp = "";
            public string _checkArrow = "";
            public string _checkCircle = "";

            public EntityRelationIconPath(string hpBg, string hpBgCheck, string hp, string arrow, string circle)
            {
                _hpBg = hpBg;
                _hpBgCheck = hpBgCheck;
                _hp = hp;
                _checkArrow = arrow;
                _checkCircle = circle;
            }
        }

        private Dictionary<E_EntityRelationType, EntityRelationIconPath> EntityRelationIconDic = new();

        public EntityRelationIconPath GetEntityRelationIconPath(E_EntityRelationType type)
        {
            EntityRelationIconPath data = null;
            if (EntityRelationIconDic.TryGetValue(type, out data))
            {

            }
            return data;
        }

        private void InitEntityRelationIconPath()
        {
            EntityRelationIconDic.Clear();

            // - 默认异常
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_06", "Hud_Locking_Arrow03", "Hud_Locking_bg03");
                EntityRelationIconDic.Add(E_EntityRelationType.None, value);
            }
            // - 【怪物】普通
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_06", "Hud_Locking_Arrow03", "Hud_Locking_bg03");
                EntityRelationIconDic.Add(E_EntityRelationType.MonsterNormal, value);
            }
            // - 【怪物】精英
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg3", "Hud_Progress_Bg3s", "Hud_Progress_02", "Hud_Locking_Arrow03", "Hud_Locking_bg03");
                EntityRelationIconDic.Add(E_EntityRelationType.MonsterElite, value);
            }
            // - 【怪物】BOSS
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg3", "Hud_Progress_Bg3s", "Hud_Progress_02", "Hud_Locking_Arrow03", "Hud_Locking_bg03");
                EntityRelationIconDic.Add(E_EntityRelationType.MonsterBoss, value);
            }
            // - 怪物】其他友善的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_04", "Hud_Locking_Arrow04", "Hud_Locking_bg04");
                EntityRelationIconDic.Add(E_EntityRelationType.MonsterOtherFriendly, value);
            }
            // - NPC
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_04", "Hud_Locking_Arrow04", "Hud_Locking_bg04");
                EntityRelationIconDic.Add(E_EntityRelationType.NPC, value);
            }
            // - 【人】主角
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_05", "Hud_Locking_Arrow01", "Hud_Locking_bg01");
                EntityRelationIconDic.Add(E_EntityRelationType.PlayerMain, value);
            }
            // - 【人】其他友善的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_04", "Hud_Locking_Arrow04", "Hud_Locking_bg04");
                EntityRelationIconDic.Add(E_EntityRelationType.PlayerOtherFriendly, value);
            }
            // - 【人】其他敌对的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_06", "Hud_Locking_Arrow03", "Hud_Locking_bg03");
                EntityRelationIconDic.Add(E_EntityRelationType.PlayerOtherHostility, value);
            }
            // - 【召唤物】主角的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_05", "Hud_Locking_Arrow01", "Hud_Locking_bg01");
                EntityRelationIconDic.Add(E_EntityRelationType.SummonMainPlayer, value);
            }
            // - 【召唤物】其他友善的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_04", "Hud_Locking_Arrow04", "Hud_Locking_bg04");
                EntityRelationIconDic.Add(E_EntityRelationType.SummonOtherFriendly, value);
            }
            // - 【召唤物】其他敌对的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_06", "Hud_Locking_Arrow03", "Hud_Locking_bg03");
                EntityRelationIconDic.Add(E_EntityRelationType.SummonOtherHostility, value);
            }
            // - 【伙伴】主角的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_05", "Hud_Locking_Arrow01", "Hud_Locking_bg01");
                EntityRelationIconDic.Add(E_EntityRelationType.PartnerMainPlayer, value);
            }
            // - 【伙伴】其他友善的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_04", "Hud_Locking_Arrow04", "Hud_Locking_bg04");
                EntityRelationIconDic.Add(E_EntityRelationType.PartnerOtherFriendly, value);
            }
            // - 【伙伴】其他敌对的
            {
                EntityRelationIconPath value = new("Hud_Progress_Bg1", "Hud_Progress_Bg1s", "Hud_Progress_06", "Hud_Locking_Arrow03", "Hud_Locking_bg03");
                EntityRelationIconDic.Add(E_EntityRelationType.PartnerOtherHostility, value);
            }
        }

        #endregion

        #region 设置 自动和战斗 目标点
        bool findTargetPosFlag = false;

        /// <summary>
        /// 设置 查找目标点标志. 相当于开/关 移动到目标点的逻辑
        /// </summary>
        /// <param name="needCheck"></param>
        public void SetFindTargetPosFlag(bool needCheck)
        {
            findTargetPosFlag = needCheck;
            // SGF.Debuger.LogError($"{AUTOBATTLEKEY} , SetFindTargetPosFlag 设置 目标点状态 {needCheck}");

            {
                // 如果 产生了新的目标点  或者 取消了这个目标点, 
                movingToTargetPos = UnityEngine.Vector3.zero;
                recordMove2Pos = UnityEngine.Vector3.zero;
            }


        }

        /// <summary>
        /// 自动战斗移动过去的点
        /// </summary>
        UnityEngine.Vector3 recordMove2Pos = UnityEngine.Vector3.zero;
        /// <summary>
        /// 设置自动战斗的目标点
        /// </summary>
        /// <param name="pos"></param>
        public void SetAutoBattleMovePoint(UnityEngine.Vector3 pos)
        {
            recordMove2Pos = pos;
            // SGF.Debuger.LogError($"{AUTOBATTLEKEY} , SetFindTargetPoint 设置 目标点: {pos}");

        }

        private bool IsGotoRecordMove2Pos()
        {
            return findTargetPosFlag && recordMove2Pos != Vector3.zero;
        }

        private void GotoRecordMove2Pos()
        {

            movingToTargetPos = recordMove2Pos;

            SGF.Debuger.Log($"[FindPath] GotoRecordMove2Pos :  goto ---> {recordMove2Pos}");

            TryGoToTargetPos((result) =>
            {

                SGF.Debuger.Log($"[FindPath] GotoRecordMove2Pos : {recordMove2Pos},  result ---> {result}");

            }, E_FindPathType.AutoBattle_Move2Pos, recordMove2Pos);
        }
        #endregion

        #region 切换索敌怪物逻辑(汉华需求)

        /// <summary>
        /// 索敌的目标
        /// </summary>
        internal class SearchEnemyTarget
        {
            public ulong EntityID;
            /// <summary>
            /// 是否被排除
            /// </summary>
            public bool IsExclude;

            public SearchEnemyTarget(ulong entityID)
            {
                EntityID = entityID;
            }

            public SearchEnemyTarget(ulong entityID, bool isExclude)
            {
                EntityID = entityID;
                IsExclude = isExclude;
            }

            public override string ToString()
            {
                return $"{EntityID}__: {IsExclude}";
            }
        }

        /// <summary>
        /// 索敌队列. 怪物 死亡/离开/ 或者 主动切换目标, 都会重新索敌.
        /// </summary>
        private List<SearchEnemyTarget> curSearchTargets = new();
        private List<Game.Entity.RemoteDynamic.AOIEntityObject> tempSearchList = new();
        private List<SearchEnemyTarget> tmpSearchTarget = new();

        /// <summary>
        /// 当前的 索敌队列 目标.  如果索敌队列的 怪物 都被 禁止了, 那就返回 0
        /// </summary>
        public ulong CurSearchEnemyTarget
        {
            get
            {
                if (curSearchTargets != null && curSearchTargets.Count > 0 && curSearchTargets[0] != null && !curSearchTargets[0].IsExclude)
                // if (curSearchTargets != null && curSearchTargets.Count > 0)
                {
                    return curSearchTargets[0].EntityID;
                }
                return 0;
            }
        }

        /// <summary>
        /// 切换当前的 索敌目标
        /// </summary>
        public bool SwitchCurSearchTarget()
        {
            // 记录一下切换当前 索敌目标前的 目标id
            ulong curTargetId = CurSearchEnemyTarget;

            // SGF.Debuger.Log($"[Switch-Search] SwitchCurSearchTarget 当前索敌 curTargetId: {curTargetId}");

            // 需要切换当前的索敌目标, 首先判断当前的 攻击怪物在不在
            // 如果当前的 攻击的怪物都不在, 那不管是 一开始就没有 这个攻击的怪物还是 说 攻击的怪物死亡.
            // 此处都是直接 重新索敌，生成 对应的索敌队列
            if (CurAtkEntity == null)
            {
                // 当前的 攻击的怪物 不存在, 所以 curTargetId 不需要设置.就用 CurSearchEnemyTarget 即可.
                // 如果是 因为 怪物死亡导致的 CurAtkEntity 为null, 那 SearchCurEnemyTargets 也会重新生成新的 索敌怪物队列.
                // 之后的 CurSearchEnemyTarget 也会发生变化
                // SGF.Debuger.Log($"[Switch-Search] SwitchCurSearchTarget 当前索敌怪物不存在, 直接生成 索敌队列");

                // 生成索敌队列
                // SearchCurEnemyTargets();
            }
            else
            {
                // 如果当前的 攻击的怪物存在, 那就记录 curTargetId 为当前攻击的怪物.
                // 当后面重新 索敌后,  根据 curTargetId 与 重新索敌后的 CurSearchEnemyTarget 比较是否会发生变化. 
                // SGF.Debuger.Log($"[Switch-Search] SwitchCurSearchTarget 当前索敌怪物存在, curTargetId: {curTargetId} ---> 变成怪物id: {CurAtkEntity.EntityId}");
                curTargetId = CurAtkEntity.EntityId;

                // 如果当前的 攻击的怪物已经存在, 那就需要需要判断 当前的 索敌队列 是否有这个怪物.
                var findTargetIdx = curSearchTargets.FindIndex((target) =>
                {
                    return target.EntityID == CurAtkEntity.EntityId;
                });

                // 如果 索敌队列中找到了这个怪物, 那就将这个怪物设置 未 排除状态
                if (-1 != findTargetIdx)
                {
                    curSearchTargets[findTargetIdx].IsExclude = true;

                    // 将0号位置 与当前选中的 怪物idx 位置交换， 保证当前选中的怪物永远在 第一个0号位置
                    curSearchTargets.Exchange(0, findTargetIdx);
                }
                else
                {
                    // 如果没有找到, 那就 将这个 排除状态的怪物数据 传入索敌队列中
                    curSearchTargets.Add(new SearchEnemyTarget(curTargetId, true));
                }
                // SGF.Debuger.Log($"[Switch-Search] SwitchCurSearchTarget 将怪物设置为 排除状态, 索敌队列为: {SearchTargetToString(curSearchTargets)}");


                // 根据当前的 索敌队列禁止数据, 重新索敌
                // SearchCurEnemyTargets();
            }

            // 根据当前的 索敌队列禁止数据, 重新索敌
            SearchCurEnemyTargets(true);
            // SGF.Debuger.Log($"[Switch-Search] SwitchCurSearchTarget 重新生成 索敌队列为: {SearchTargetToString(curSearchTargets)}");

            // SGF.Debuger.Log($"[Switch-Search] SwitchCurSearchTarget 原始索敌怪物: {curTargetId} ---> {CurSearchEnemyTarget}");


            // 索敌的数据发生了变化,推送索敌数据.
            SetCurAtkEntityID(CurSearchEnemyTarget);

            // 切换索敌目标的时候, 采用索敌的怪物为起始点, 同时标脏
            if (IsAutoBattling)
            {
                // 选中怪物后，重新记录一次 自动战斗的索敌中心点
                RecordAutoBattleStartPos(false);
                MarkDirtyState(true);
                // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] SwitchCurSearchTarget ");

            }

            // 
            return CurAtkEntity != null;
        }

        private string SearchTargetToString(List<SearchEnemyTarget> targets)
        {
            return targets.KJoin(" | ");
        }

        /// <summary>
        /// 切换指定的 entityID 为 索敌队列的 当前索敌目标.
        /// </summary>
        /// <param name="entityID"></param>
        public void Switch2TargetEnemy(ulong entityID)
        {
            // SGF.Debuger.Log($"[Switch-Search] Switch2TargetEnemy CurSearchEnemyTarget: {CurSearchEnemyTarget} -----> 切换到 指定的怪物: {entityID} ");

            if (entityID == CurSearchEnemyTarget)
            {
                return;
            }
            // 先直接 索敌一次
            // SGF.Debuger.Log($"[Switch-Search] Switch2TargetEnemy 当前的 索敌队列为: {SearchTargetToString(curSearchTargets)}");

            SearchCurEnemyTargets(false);
            // SGF.Debuger.Log($"[Switch-Search] Switch2TargetEnemy 重新生成 索敌队列为: {SearchTargetToString(curSearchTargets)}");

            int findIdx = -1;
            for (int i = 0; i < curSearchTargets.Count; i++)
            {
                var target = curSearchTargets[i];

                if (target == null)
                {
                    curSearchTargets.RemoveAt(i);
                    i--;
                    continue;
                }

                if (target.EntityID != entityID)
                {
                    target.IsExclude = true;
                }
                else
                {
                    // 如果找到了指定的 entityID,那就 将其设为唯一的 非锁定 目标.
                    target.IsExclude = false;
                    findIdx = i;
                }
            }
            // SGF.Debuger.Log($"[Switch-Search] Switch2TargetEnemy 指定目标怪物后 索敌队列为: {SearchTargetToString(curSearchTargets)}");


            // 如果在当前的 索敌目标中找到了 指定的怪物, 那就直接 重新排序, 推送 改变怪物
            if (-1 != findIdx)
            {
                SortSearchTargets(curSearchTargets);
                // SGF.Debuger.Log($"[Switch-Search] Switch2TargetEnemy 排序 索敌队列为: {SearchTargetToString(curSearchTargets)}");

            }
            else
            {
                // 如果找不到 点中的怪物, 那就是说 索敌范围跟 点中的 范围不一致. 将点中的怪物 直接插入0号位置
                curSearchTargets.Insert(0, new SearchEnemyTarget(entityID));
                // SGF.Debuger.Log($"[Switch-Search] Switch2TargetEnemy 插入指定怪物后 索敌队列为: {SearchTargetToString(curSearchTargets)}");

            }

            SetCurAtkEntityID(CurSearchEnemyTarget);

        }

        /// <summary>
        /// 清空 索敌队列
        /// </summary>
        public void ClearSerarchTargets()
        {
            curSearchTargets.Clear();
            SGF.Debuger.Log($"[Switch-Search] ClearSerarchTargets 清空索敌队列");

            SetCurAtkEntityID(CurSearchEnemyTarget);

        }

        /// <summary>
        /// 查找当前的 索敌队列
        /// </summary>
        /// <param name="filterCurSearchTarget">索敌队列 是否过需要滤掉(删除) 当前索敌的目标</param>
        private void SearchCurEnemyTargets(bool filterCurSearchTarget)
        {
            // 如果 在 SearchCurEnemyTargets 当前选中的目标 就已经被排除了, 那 curSearchTargetID 永远拿到的都是 0
            ulong curSearchTargetID = CurSearchEnemyTarget;
            if (CurAtkEntity != null)
            {
                // SGF.Debuger.Log($"[Switch-Search] SearchCurEnemyTargets curSearchTargetID : {curSearchTargetID} ---> CurAtkEntity: {CurAtkEntity.EntityId}");

                curSearchTargetID = CurAtkEntity.EntityId;
            }

            // 1. 首先 找到附件 范围内 最新的 索敌队列.
            curSearchTargets = SearchEnemyWithCurTargets();
            // SGF.Debuger.Log($"[Switch-Search] SearchCurEnemyTargets 查找玩家范围内的 索敌队列: {SearchTargetToString(curSearchTargets)}");

            // 2. 过滤掉 所有被禁止的 怪物
            // note:
            //      此处采用的是 tmpSearchTarget接收, 如果直接 使用curSearchTargets 接收, 那么 过滤的时候,
            //      就会丢失掉之前 切换需要禁止的怪物. 
            //      目前只有 切换到完全 没有可用怪物的时候,才需要清除 禁止怪物数据. 即 下面的 3.
            tmpSearchTarget = FilterSearchTarget(curSearchTargets);
            // SGF.Debuger.Log($"[Switch-Search] SearchCurEnemyTargets 过滤后 索敌队列: {SearchTargetToString(tmpSearchTarget)}");

            // 3.判断 过滤后的 索敌队列数量. 如果索敌数量为0, 那就先清除所有的 索敌队列数据(主要是清除禁止的怪物数据), 重新索敌.
            if (tmpSearchTarget.Count == 0)
            {
                // 0. 首先清空 之前的索敌队列 数据
                curSearchTargets.Clear();
                // 1. 重新索敌
                curSearchTargets = SearchEnemyWithCurTargets();
                // SGF.Debuger.Log($"[Switch-Search] SearchCurEnemyTargets 过滤后 数量为0, 重新索敌后  索敌队列: {SearchTargetToString(curSearchTargets)}");


            }
            else
            {
                // 如果 tmpSearchTarget 存在数据, 那说明可以 锁到敌人 
            }
            // 清除缓存数据
            tmpSearchTarget.Clear();


            // 对索敌队列排序
            SortSearchTargets(curSearchTargets);

            // 如果上一次 索敌怪物 存在, 那此次 重新索敌需要把上次的怪物 放后面
            if (curSearchTargetID != 0 && CurSearchEnemyTarget == curSearchTargetID && curSearchTargets.Count > 1)
            {
                if (!curSearchTargets[1].IsExclude)
                {
                    // SGF.Debuger.Log($"[Switch-Search] SearchCurEnemyTargets 0号位与上次选中的相同, 交换0/1号, 索敌队列: {SearchTargetToString(curSearchTargets)}");

                    curSearchTargets.Exchange(0, 1);
                }
            }


            // SGF.Debuger.Log($"[Switch-Search] SearchCurEnemyTargets 排序后 索敌队列: {SearchTargetToString(curSearchTargets)}");


        }

        /// <summary>
        /// 查找周围的 敌人(考虑当前的 索敌队列)
        /// </summary>
        private List<SearchEnemyTarget> SearchEnemyWithCurTargets()
        {
            List<SearchEnemyTarget> currentTriggetNtts = new();

            // 先取周围20m 的范围
            int maxRadius = 2000;

            tempSearchList.Clear();
            tempSearchList.AddRange(SkillUtils.GetTargetInMaxRadius(GameManager.Instance.mainPlayerId, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Faction, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position(), maxRadius, SelectType.Enemy));


            tempSearchList.ForEach((searchItem) =>
            {
                if (searchItem == null)
                {
                    return;
                }

                // 如果怪物不再屏幕内， 过滤这个怪物
                if (!GameManager.Instance.GetGameCameraComponent().CheckIsInCameraPos(searchItem.Position()))
                {
                    return;
                }

                // 1.检查 新的索敌怪物 是否已经存在于 之前的 索敌队列中
                var searchTarget = curSearchTargets.Find((target) =>
                {
                    return searchItem.EntityId == target?.EntityID;
                });

                // 2.如果存在于 索敌队列中, 不管之前是否是 已经是否被排除, 都加入 当前新生成的索敌队列中, 后面统一排序
                if (searchTarget != null)
                {
                    currentTriggetNtts.Add(searchTarget);
                }
                else
                {
                    // 3.如果不存在, 表明新增的 怪物, 需要进入索敌队列
                    currentTriggetNtts.Add(new SearchEnemyTarget(searchItem.EntityId));
                }
            });

            return currentTriggetNtts;
        }



        private List<SearchEnemyTarget> FilterSearchTarget(List<SearchEnemyTarget> targets)
        {
            tmpSearchTarget.Clear();

            targets.ForEach((target) =>
            {
                if (!target.IsExclude)
                {
                    tmpSearchTarget.Add(target);
                }
            });
            return tmpSearchTarget;
        }

        private void SortSearchTargets(List<SearchEnemyTarget> targets)
        {
            // 1.如果索敌队列数量为0, 那就不需要排序
            if (targets.Count <= 1)
            {
                return;
            }

            try
            {
                // 理论上 不会有 null 的情况, 但是 怪物一多的时候旧有异常, 不知道null 如何被传入进来了
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null || targets[i].EntityID == 0)
                    {
                        SGF.Debuger.LogWarning($"[Switch-Search] SortSearchTargets remove : {targets[i]}");
                        targets.RemoveAt(i);
                        i--;
                    }
                }

                targets.Sort((targetA, targetB) =>
                {

                    // 不知道sort 的过程中 为啥会有null, 但是有null 旧先放后面
                    if (targetA == null || targetB == null)
                    {
                        SGF.Debuger.LogWarning($"[Switch-Search] 干, null A: {targetA} , B: {targetB} ");

                        return 1;
                    }
                    // 如果 A/B 都是被 排除/ 未排除
                    if (targetA.IsExclude == targetB.IsExclude)
                    {
                        // 如果 都被排除, 那 A/B 其实不用再去排序. 因为 A/B 都不会进入选择队列
                        if (targetA.IsExclude)
                        {
                            // 默认从小到大的排序。 此处其实未做任何排序逻辑
                            return -1;
                        }
                        else
                        {
                            // 如果都未排除, 那就要按 汉化的 排序规则进行排序.
                            return ComparaWithRule(targetA, targetB);
                        }
                    }
                    else
                    {
                        // 如果 A/B 不相等, 那么 被排除的 永远 排在后面
                        if (targetA.IsExclude)
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                });
            }
            catch (System.Exception e)
            {
                SGF.Debuger.LogError($"[Switch-Search] SortSearchTargets sort 报错: {e.Message}");
                targets.Clear();
            }

        }

        /// <summary>
        /// 排序规则. 
        /// 汉华的意思, 后续会有 三个排序规则:
        /// 1.距离就近筛选
        /// 2.当前生命绝对值最低
        /// 3.当前生命百分比最低
        /// 
        /// 当前默认 就先按距离就近 排序.  跟王者一样，后面可以通过设置面板,
        /// 设置 排序的 规则.
        /// </summary>
        private int sortRuler = 1;
        private int ComparaWithRule(SearchEnemyTarget targetA, SearchEnemyTarget targetB)
        {
            var entityA = GameManager.Instance.GetEntityByEntityID(targetA.EntityID);
            var entityB = GameManager.Instance.GetEntityByEntityID(targetB.EntityID);

            bool isARobat = entityA.Data.IsRobot;
            bool isBRobat = entityB.Data.IsRobot;
            // 1.优先级 : Boss>精英怪>普通怪物
            // 如果 configIndex 不相等, 那就说明不是同一种怪物类型. 此时 就按照 怪物类型的优先级判断 
            //    如果 ab 都是机器人, 那就直接 走 后面其它的排序规则
            if (entityA.ConfigIndex != entityB.ConfigIndex && (!isARobat || !isBRobat))
            {
                // 如果 a 是机器人,  b 是monster, 那就 a 在前面
                if (isARobat)
                {
                    return -1;
                }

                // 如果 b 是机器人,  a 是monster, 那就 b 在前面
                if (isBRobat)
                {
                    return 1;
                }

                // a b 都是 怪物的时候, 按 怪物类型排序
                MonsterDataCell monsterAAttrDataCell = LocalDataManager.Instance.GetMonsterDataCell((int)entityA.ConfigIndex);
                MonsterDataCell monsterBAttrDataCell = LocalDataManager.Instance.GetMonsterDataCell((int)entityB.ConfigIndex);

                if (monsterAAttrDataCell == null)
                {
                    SGF.Debuger.LogError($"[Switch-Search] ComparaWithRule  ConfigIndex: {entityA.ConfigIndex} 缺少 配置!!!");

                    return 1;
                }
                if (monsterBAttrDataCell == null)
                {
                    SGF.Debuger.LogError($"[Switch-Search] ComparaWithRule  ConfigIndex: {entityB.ConfigIndex} 缺少 配置!!!");

                    return 1;
                }
                // 根据怪物类型,按 大--->小 排序
                SGF.Debuger.LogWarning($"[Switch-Search] ComparaWithRule A type: {monsterBAttrDataCell.GetMonType()} , B type: {monsterAAttrDataCell.GetMonType()}");

                return monsterBAttrDataCell.GetMonType() - monsterAAttrDataCell.GetMonType();
            }
            else
            {
                switch (sortRuler)
                {
                    case 1:
                        {
                            return SortWithDistance(entityA, entityB);
                        }
                    case 2:
                        {
                            return SortWithAbsHp(entityA, entityB);
                        }
                    case 3:
                        {
                            return SortWithPercentHp(entityA, entityB);
                        }
                    default: break;
                }
            }
            // 理论上不应该到这一步
            Debug.LogError($"[Switch-Search] ComparaWithRule 没找到 合适的排序 规则: {sortRuler} 对应的排序方法");
            return -1;
        }

        /// <summary>
        /// 根据离 主角的距离排序
        /// </summary>
        private int SortWithDistance(NPCEntityBase entityA, NPCEntityBase entityB)
        {
            // 如果相等, 那就是同样的 怪物类型. 
            // 2. 根据距离排序
            var mainPlayerPos = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
            var dirA = entityA.Position() - mainPlayerPos;
            var dirB = entityB.Position() - mainPlayerPos;

            dirA.Set(dirA.x, 0, dirA.z);
            dirB.Set(dirB.x, 0, dirB.z);

            var disA = dirA.sqrMagnitude;
            var disB = dirB.sqrMagnitude;
            SGF.Debuger.LogWarning($"[Switch-Search] SortWithDistance disA: {disA} , disB: {disB}");

            // 如果距离 不相等, 那就按 距离远近排序
            if (disA != disB)
            {
                // 距离 从小到到 排序
                return disA.CompareTo(disB);
            }
            else
            {
                return entityA.EntityId.CompareTo(entityB.EntityId);
            }
        }


        /// <summary>
        /// 根据生命值的 绝对值 由低到高 排序
        /// </summary>
        private int SortWithAbsHp(NPCEntityBase entityA, NPCEntityBase entityB)
        {
            long curAHp = entityA.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);
            long curBHp = entityB.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);

            if (curAHp != curBHp)
            {
                return curAHp.CompareTo(curBHp);
            }
            else
            {
                // 如果血条都一样了, 那就按距离从小到大排序
                return SortWithDistance(entityA, entityB);
            }
        }

        /// <summary>
        /// 根据 生命值的  百分比进行排序
        /// </summary>
        private int SortWithPercentHp(NPCEntityBase entityA, NPCEntityBase entityB)
        {
            long curAHp = entityA.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);
            long curATruthHp = entityA.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthHp);

            long curBHp = entityB.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);
            long curBTruthHp = entityB.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.TruthHp);

            var aPerHp = curAHp * 1f / curATruthHp;
            var bPerHp = curBHp * 1f / curBTruthHp;


            if (aPerHp != bPerHp)
            {
                return aPerHp.CompareTo(bPerHp);
            }
            else
            {
                // 如果生命值 百分比相同, 那就按照绝对值hp 排序
                return SortWithAbsHp(entityA, entityB);
            }

        }
        #endregion

        #region 伙伴的自动战斗
        private PartnerConcretizeReq m_PartnerConcretizeReq = new();

        private void TriggerPartnerSkill()
        {
            // SGF.Debuger.Log($"{AUTOBATTLEKEY} ,准备触发伙伴技能: TriggerPartnerSkill: {IsAutoBattling} , IsAutoBattleHoldOn: {IsAutoBattleHoldOn}");
            var partnerMD = GetCanUsePartner();

            if (partnerMD == null)
            {
                return;
            }
            // SGF.Debuger.Log($"{AUTOBATTLEKEY} ,伙伴 {partnerMD.Index} 准备释放技能");

            // 
            m_PartnerConcretizeReq.Index = partnerMD.Index;
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, m_PartnerConcretizeReq, false);
        }

        private PartnerMD GetCanUsePartner()
        {

            List<PartnerMD> partnerMDs = PartnerManager.Instance.GetInPlayedPartners();
            // 1. 如果没有伙伴出战,且有怪物的时候, 要 出战;
            // 2. 如果 没有伙伴 有战斗状态, 要 return
            if (!GameManager.Instance.M_MainPlayerCtrlBase.Data.IsBattleStateServer)
            {
                return null;
            }

            // 有伙伴出战
            bool hasPartnerConcretization = false;
            // 有伙伴在战斗状态
            bool inBattleState = false;
            // 第一次遍历, 先找登场的伙伴,如果已经登场, 那就判断 战斗状态， 只有在战斗状态的情况下
            for (int i = 0; i < partnerMDs.Count; i++)
            {
                var partnerMD = partnerMDs[i];
                if (partnerMD == null)
                {
                    continue;
                }
                // 如果已经出战 释放过登场技能, 那就 不管，目前 伙伴只有登场技能,客户端根据 战斗状态判断能不能 自动战斗放技能
                if (partnerMD.State == PartnerState.Concretization)
                {

                    var entityBase = GameManager.Instance.GetEntityByEntityID(partnerMD.ID);

                    if (entityBase == null)
                    {
                        continue;
                    }

                    hasPartnerConcretization = true;

                    // 如果 伙伴能够找到 并且是 战斗状态, 那就 可以执行后续的 伙伴自动战斗
                    if (entityBase.Data.IsBattleStateServer)
                    {
                        inBattleState = true;
                    }
                }
            }

            // 没有伙伴出战时, 就要看 主角是否是战斗状态, 如果主角都不是战斗状态, 那伙伴也就不用战斗了
            if (hasPartnerConcretization && !inBattleState)
            {
                return null;
            }

            for (int i = 0; i < partnerMDs.Count; i++)
            {
                var partnerMD = partnerMDs[i];

                if (partnerMD == null)
                {
                    continue;
                }

                // 如果已经出战 释放过登场技能, 那就 不管，目前 伙伴只有登场技能
                if (partnerMD.State == PartnerState.Concretization || partnerMD.CurHp == 0)
                {
                    continue;
                }
                var entityBase = GameManager.Instance.GetEntityByEntityID(partnerMD.ID);

                // 如果 伙伴能够找到 并且是 战斗状态, 那就 可以执行后续的 伙伴自动战斗
                if (entityBase != null && entityBase.Data.IsBattleStateServer)
                {
                    continue;
                }
                var parSkillDataCell = LocalDataManager.Instance.GetParSkillDataCell((int)partnerMD.Index, partnerMD.CurStar);

                if (parSkillDataCell == null)
                {
                    SGF.Debuger.LogError($"{AUTOBATTLEKEY} ,准备触发伙伴_{partnerMD.ID} index: {partnerMD.Index} , star: {partnerMD.CurStar} 技能 找不到配置!!!");
                    continue;
                }

                int curSkillId = parSkillDataCell.GetParSkill();
                // 先检查 伙伴技能槽位的 CD
                GameManager.Instance.GetPartnerSkillCDBySkillID(curSkillId, out var skillCD, out var skillMaxCD);
                if (skillCD > 0)
                {
                    continue;
                }

                // 再检查伙伴的 技能CD 是否满足, 只有 技能曹CD 和 伙伴技能CD 都结束, 才需要 自动释放
                var cds = partnerMD.CDMD;
                // 伙伴技能 没有 CD
                if (cds.CDList.Count == 0)
                {

                    return partnerMD;
                }

                // 如果伙伴cd 存在, 判断 cd 是否已经结束
                for (int j = 0; j < cds.CDList.Count; j++)
                {
                    CDData cdInfo = cds.CDList[j];
                    var LeastSeatCD = Fire.Utils.CalculateCD(cdInfo.StartCDTime, cdInfo.EndCDTime);
                    // 如果 cd 已经结束, 那就触发这个
                    if (LeastSeatCD == 0)
                    {
                        return partnerMD;
                    }
                }

                return partnerMD;
            }

            return null;
        }

        #endregion

        #region 副本任务目标相关的逻辑

        private InstanceDataRet cur_InstanceData = null;
        private int cur_instance_goalID = 0;
        /// <summary>
        /// 当副本目标刷新的时候, 标脏
        /// </summary>
        /// <param name="data">
        private void OnInstanceDataRetID(MessageHandleData data)
        {
            InstanceDataRet instacne = (InstanceDataRet)data.data;

            cur_InstanceData = instacne;
            if (instacne == null)
            {
                return;
            }

            if (cur_instance_goalID != instacne.Goal)
            {
                return;
            }

            cur_instance_goalID = instacne.Goal;

            MarkDirtyState(true);
            // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] OnInstanceDataRetID ");


        }

        /// <summary>
        /// 已经到达的 副本目标， 玩家达到副本目标后， 如果托摇杆移动，
        /// 那就不回到之前的副本目标
        /// </summary>
        private int arrive_instance_goalID = 0;

        /// <summary>
        /// 没有怪物的时候 才需要检查 是否需要去对应的副本目标
        /// </summary>
        /// <returns></returns>
        private bool IsGotoInstanceGoalID()
        {
            // if (IsInstanceEnd())
            // {
            //     return false;
            // }

            var curMapType = GameManager.Instance.GetCurMapType();

            // 只有副本类型的时候，才需要去判断是否执行 副本目标
            if (curMapType == SpaceType.SpaceScene || curMapType == SpaceType.SpaceDefault)
            {
                return false;
            }

            if (arrive_instance_goalID == cur_instance_goalID)
            {
                return false;
            }

            if (cur_instance_goalID == 0)
            {
                return false;
            }

            // 地图数据找不到
            if (GameMap.sceneJsonData == null || GameMap.sceneJsonData.Spawners == null)
            {
                return false;
            }

            var cell = LocalDataManager.Instance.GetEctypeTargetDataCell(cur_instance_goalID);
            if (cell == null)
            {
                return false;
            }

            if (!GameMap.sceneJsonData.Spawners.TryGetValue(cell.Spid, out var data))
            {
                return false;
            }

            if (data == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 得到副本目标对应的坐标
        /// note:
        ///     上面已经做了判断, 所以此处的配置认为是一定可以获取的.
        /// </summary>
        private Vector3 GetSpawneridPos()
        {

            var cell = LocalDataManager.Instance.GetEctypeTargetDataCell(cur_instance_goalID);

            var spid = cell.Spid;
            if (GameMap.sceneJsonData.Spawners.TryGetValue(spid, out var data) && data != null)
            {
                return data.Position.Convert();
            }

            // 实际上这种情况应该不存在
            return Vector3.zero;
        }

        private void GotoSpawner()
        {
            // 记录一个 当前去的 副本目标id
            var gotoInstanceGoalID = cur_instance_goalID;

            // 获得当前副本目标对应的 目标点坐标
            var spawnPos = GetSpawneridPos();
            SGF.Debuger.Log($"[FindPath] GotoSpawner : {cur_instance_goalID},  goto ---> {spawnPos}");

            TryGoToTargetPos(
            (result) =>
            {
                if (result)
                {
                    arrive_instance_goalID = gotoInstanceGoalID;
                }
                SGF.Debuger.Log($"[FindPath] GotoSpawner : {gotoInstanceGoalID},  result ---> {result}");

            }, E_FindPathType.AutoBattle_Spawner, spawnPos);
        }

        #endregion

        #region 拟态梦境自动战斗需要单独写

        /// <summary>
        /// 是否去一个 个人秘境 需要去的点.
        /// </summary>
        /// <param name="point"></param>
        private bool IsGotoSecretAreaPoint(out Vector3 point)
        {
            point = Vector3.zero;

            if (!CheckCurIsSpaceType(SpaceType.SpaceSercet))
            {
                return false;
            }

            // 不能在协议的回复的时候刷, 协议回复的时候, mapId 还没切过来
            {
                // 刷新 这个地图的刷怪点
                RefreshSercetPoints();
            }

            switch (GetCurPersonSecretLevel())
            {
                case 0:
                    {
                        // 走到交互物
                        return BusinessManager.Instance.GetSecretAreaBuffPos(out point);
                    }
                case 1:
                    {
                        // 刷怪
                        return IsGotoMonsterPoint(out point);
                    }
                case 2:
                    {
                        // 刷 boss
                        return IsGotoBossBornPoint(out point);
                    }
                case 3:
                    {
                        // 走到宝箱
                        return IsGotoBossDeadPoint(out point);
                    }
                case -1:
                    {
                        // -1 表示结束后续自动战斗控制, 哪个地方也不需要去, 所以 return true, 但是不返回具体坐标
                        return true;
                    }

                default: break;
            }

            return false;
        }

        private void GotoSecretAreaPoint(Vector3 point)
        {
            switch (GetCurPersonSecretLevel())
            {
                case 0:
                    {
                        // 等待交互物的状态
                        TryGoToTargetPos((result) =>
                        {
                            var entityID = GameManager.Instance.GetCloestInteractive();
                            if (entityID == 0)
                            {
                                SGF.Debuger.LogWarning($"[AutoBattle] 到了 交互物点后 找不到 对应的 交互物");
                                return;
                            }
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "QueryInter", entityID);

                        }, E_FindPathType.AutoBattle_Move2Pos, point, 1.2f);
                    }
                    break;
                case 1:
                    {
                        GotoMonsterPoint(point, 1f);
                    }
                    break;
                case 2:
                    {
                        GotoMonsterPoint(point, 2f);
                    }
                    break;
                case 3:
                    {
                        // 等待交互物的状态
                        TryGoToTargetPos((result) =>
                        {
                            // SGF.Debuger.Log($"[AutoBattle] 到了 boss 死亡的点");
                            rewardBoxFlag = true;

                        }, E_FindPathType.AutoBattle_Move2Pos, point);
                    }
                    break;
                case -1:
                    {
                        // 结束自动战斗的控制, 啥都不干
                    }
                    break;

                default: break;
            }

        }

        /// <summary>
        /// 副本 mpaID 对应的 怪物点 寻路路径点（包含出生点）
        /// note:
        ///     此处包含出生点，就可以通过当前的坐标到 怪物点的 寻路最小距离，算出最近的一个怪物点。 
        ///     然后 寻找 它的下一个 怪物点即可
        /// </summary>
        public Dictionary<int, List<MonsterPathPoint>> secretMap2MonsterPathPoint = new();
        private bool IsGotoMonsterPoint(out Vector3 point)
        {
            point = Vector3.zero;

            // 如果没找到怪物点, 就找要给 可以到的有怪物的点
            if (FindCanArriveMonsterPoint(out point))
            {
                return true;
            }

            return FindMonsterPoint(out point);

        }

        // 到 boss 出生点
        private bool IsGotoBossBornPoint(out Vector3 point)
        {
            return BusinessManager.Instance.GetSecretBossPos(out point);
        }

        private bool IsGotoBossDeadPoint(out Vector3 point)
        {
            point = Vector3.zero;

            if (cur_gameEndPersonSecretRet == null)
            {
                return false;
            }

            ProtoUtils.CopyPbPos2V3(out point, cur_gameEndPersonSecretRet.BossDeadPos);

            // 曲 宝箱 生成位置
            point.z = point.z + 2f;

            return true;
        }

        public class MonsterPathPoint
        {
            public float Distance;
            public Vector3 Point = Vector3.zero;

            public MonsterPathPoint(Vector3 point, float distance)
            {
                Distance = distance;
                Point = point;
            }

            public MonsterPathPoint(MonsterPathPoint monsterPathPoint = null)
            {
                Update(monsterPathPoint);
            }

            public void Update(MonsterPathPoint monsterPathPoint = null)
            {
                if (monsterPathPoint != null)
                {
                    Distance = monsterPathPoint.Distance;
                    Point = monsterPathPoint.Point;
                }
            }
            private static Queue<MonsterPathPoint> pools = new();
            public static MonsterPathPoint Get(MonsterPathPoint monsterPathPoint = null)
            {
                MonsterPathPoint m_point = null;
                if (pools.Count == 0)
                {
                    m_point = new();
                }
                else
                {
                    m_point = pools.Dequeue();
                }
                m_point.Update(monsterPathPoint);

                return m_point;
            }


            public static void Put(MonsterPathPoint monsterPathPoint)
            {
                pools.Enqueue(monsterPathPoint);
            }
        }

        /// <summary>
        /// 根据 出生点 到 所有随机点的 距离排序生成一个 随机点位的
        /// </summary>
        private void RefreshSercetPoints()
        {
            var mapId = GameManager.Instance.M_Map.GetMapId();
            if (mapId == -1)
            {
                return;
            }
#if UNITY_EDITOR
            // 测试逻辑
            // secretMap2MonsterPathPoint.Clear();
#endif

            // 如果 没有这个 秘境怪物路径点, 那就 return
            if (secretMap2MonsterPathPoint.ContainsKey(mapId))
            {
                return;
            }

            if (GameMap.sceneJsonData == null || GameMap.sceneJsonData.Areas == null)
            {
                return;
            }


            //1.找到 出生点.
            var areas = GameMap.sceneJsonData.Areas;

            Vector3 startPos = Vector3.zero;
            foreach (var item in areas)
            {
                // 如果是出生点类型
                if (item.Value.areaType == 2)
                {
                    startPos = item.Value.Position.Convert();
                    break;
                }
            }
            ShowPoint(startPos, "starPoint", Color.yellow);

            //2.计算出 出生点到寻路点 直接的 距离
            var randomMonsters = GameMap.sceneJsonData.RandomMonsters;


            List<MonsterPathPoint> listMonsterPoints = new();

            List<MonsterPathPoint> bossPoints = new();
            // 拿到所有怪物的随机点
            foreach (var item in randomMonsters)
            {
                RandomMonsterJsonData monsterJson = item.Value;

                if (monsterJson.MonsterType == 3)
                {
                    bossPoints.Add(new MonsterPathPoint(monsterJson.Position.Convert(), 0));
                }
                else
                {
                    listMonsterPoints.Add(new MonsterPathPoint(monsterJson.Position.Convert(), 0));
                }

            }

            // 从开始点 寻路到 怪物的随机点
            listMonsterPoints.ForEach((item) =>
            {
                var monsterPoint = item.Point;

                bool findResult = FindPathManager.Instance.FindPath(startPos, monsterPoint, out Vector3[] corners, out float distance);
                item.Distance = distance;
                if (!findResult)
                {
                    SGF.Debuger.Log($"[FindPath]  monsterPoint: {monsterPoint} 不可到达");
                    item.Distance = 99999;
                }
                else
                {

                    // item.Distance = GetDistance(startPos, corners);
                    // SGF.Debuger.Log($"[FindPath]  monsterPoint: {monsterPoint} , startPos: {startPos}, distance: {item.Distance} , corners: {corners.KJoin(", ")}  ");
                }
            });

            //3.根据距离从小到大排序
            listMonsterPoints.Sort((a, b) =>
            {
                return a.Distance.CompareTo(b.Distance);
            });

            // 从开始点 寻路到 怪物的随机点
            bossPoints.ForEach((item) =>
            {
                var monsterPoint = item.Point;
                bool findResult = FindPathManager.Instance.FindPath(startPos, monsterPoint, 0, out Vector3[] corners);
                if (!findResult)
                {
                    SGF.Debuger.Log($"[FindPath]  monsterPoint: {monsterPoint} 不可到达");
                    item.Distance = 99999;
                }
                else
                {
                    item.Distance = GetDistance(startPos, corners);
                }
            });

            //3.根据距离从小到大排序
            bossPoints.Sort((a, b) =>
            {
                return a.Distance.CompareTo(b.Distance);
            });

            List<MonsterPathPoint> secretMonsterPoint = new();

            //4.生成一份随机点位的路径队列    
            int i = 0;
            listMonsterPoints.ForEach((item) =>
                {
                    secretMonsterPoint.Add(item);
                    ShowPoint(item.Point, $"monster_point_{i++}_{item.Distance}", Color.white);
                });

            bossPoints.ForEach((item) =>
            {
                // secretMonsterPoint.Add(item);
                ShowPoint(item.Point, $"monster_point_{i++}", Color.red);
            });

            secretMap2MonsterPathPoint.Add(mapId, secretMonsterPoint);
        }

        public void TestDistance()
        {
            GameObject.DestroyImmediate(testGoParent);
            secretMap2MonsterPathPoint.Clear();

        }

        private GameObject testGoParent;
        private void ShowPoint(Vector3 pos, string name, Color color, float x = 1, float y = 1)
        {
            return;
            if (testGoParent == null)
            {
                testGoParent = new GameObject();
            }
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.GetComponent<BoxCollider>().enabled = false;
            go.transform.parent = testGoParent.transform;
            go.transform.position = pos;
            go.name = name;
            go.GetComponent<MeshRenderer>().material.SetColor("_MainColor", color);
            go.GetComponent<Transform>().SetScaleXY(x, y);
            // return go;
        }

        private void ShowRecordPoint(Vector3 pos, string name, Color color)
        {
            return;

            if (testGoParent == null)
            {
                testGoParent = new GameObject();
            }

            var pointT = testGoParent.transform.Find(name);
            if (pointT != null)
            {
                pointT.position = pos;
                return;
            }

            ShowPoint(pos, name, color);

        }

        private List<MonsterPathPoint> FindMonsterPoints()
        {
            var mapId = GameManager.Instance.M_Map.GetMapId();

            if (!secretMap2MonsterPathPoint.ContainsKey(mapId))
            {
                return null;
            }

            List<MonsterPathPoint> secretMonsterPoints = secretMap2MonsterPathPoint[mapId];

            return secretMonsterPoints;
        }

        /// <summary>
        /// 找到 当前位置 距离最近的
        /// </summary>
        private bool FindMonsterPoint(out Vector3 findPoint)
        {
            findPoint = Vector3.zero;

            Vector3 curPoint = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

            // 前面已经做了判断，所以此处 地图对应的 怪物生成点一定能够找到
            List<MonsterPathPoint> secretMonsterPoint = FindMonsterPoints();

            // 个人秘境不会走到一般 被空气墙锁住
            // 如果 已经找遍了所以的点 还是没有怪, 那 清除缓存重新找
            if (secretMonsterPoint == null || secretMonsterPoint.Count == arriveMonsterRecord.Count)
            {
                arriveMonsterRecord.Clear();
                return false;
            }



            // 可以到达的怪物点
            List<MonsterPathPoint> walkableMonsterPoints = new();

            //1. 首先找一个 寻路距离最近的点
            for (int i = 0; i < secretMonsterPoint.Count; i++)
            {
                var monsterPoint = secretMonsterPoint[i].Point;

                // 如果之前已经到达了这个点
                if (arriveMonsterRecord.Contains(monsterPoint))
                {
                    continue;
                }

                // 空气墙 我担心 可能在前面 也可能在后面， 如果 走过的区域加上了空气墙，那就不能往回走，但是可以往前走
                if (!FindPathManager.Instance.CanArrive(curPoint, monsterPoint))
                {
                    continue;
                }

                bool findResult = FindPathManager.Instance.FindPath(curPoint, monsterPoint, out Vector3[] corners, out float distance);

                if (!findResult)
                {
                    SGF.Debuger.Log($"[FindPath]  monsterPoint: {monsterPoint} 不可到达");
                    continue;
                }
                else
                {
                    var m_point = MonsterPathPoint.Get(secretMonsterPoint[i]);
                    // 计算当前位置 到这个怪物点的 距离
                    m_point.Distance = distance;

                    walkableMonsterPoints.Add(m_point);
                }
            }

            var walkablePointCount = walkableMonsterPoints.Count;

            // 如果没有可以到达的点, 那就站着不动，不用干啥
            if (walkablePointCount == 0)
            {
                return false;
            }

            // 根据当前的点 的距离,重新生成一个 新的 点
            walkableMonsterPoints.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            // 直接找最近的 刷怪点
            findPoint = walkableMonsterPoints[0].Point;

            walkableMonsterPoints.ForEach((item) =>
            {
                MonsterPathPoint.Put(item);
            });

            walkableMonsterPoints.Clear();
            return true;

            // 玩家可以拖动摇杆，所以可能是从怪物的点位托摇杆 到了一个还未产生怪物的出生点， 
            // 此时如果只判断 距离出生点的距离，那可能去的位置没有怪物.
            // 所以就按照 最近的点 往前找

            // 先找距离最小的点, 判断 之前的点有没有怪物
            {
                // note:
                //      由于玩家可以拖动摇杆，所以玩家可能在寻路的中心点, 那么最小可达的点可以有多个
                float minDis = 0;
                for (int i = 0; i < walkablePointCount; i++)
                {
                    if (i == 0)
                    {
                        minDis = walkableMonsterPoints[i].Distance;
                    }
                    else
                    {

                        minDis = math.min(minDis, walkableMonsterPoints[i].Distance);
                    }
                }

                List<int> minPointIdx = new();
                for (int i = 0; i < walkablePointCount; i++)
                {
                    if (minDis == walkableMonsterPoints[i].Distance)
                    {
                        minPointIdx.Add(i);
                    }
                }

                int lastMinPointIdx = minPointIdx[minPointIdx.Count - 1];

                // 从最后 一个最小点，往前找怪物出生点，一旦发现怪物, 那就返回这个怪物点
                // 怪物刷怪 是从前往后的点 一个个刷, 所以先从最近点 往前找, 如果前面的点有怪物, 那就说明是 玩家托遥感拖到后面去了，
                // 先把前面的怪物刷完再说
                {
                    for (int i = lastMinPointIdx; i >= 0; i--)
                    {
                        if (SearchMonsterInPoint(walkableMonsterPoints[i].Point, 350))
                        {
                            findPoint = walkableMonsterPoints[i].Point;
                            return true;
                        }
                    }

                    // 先让它不漏点, 一个点一个点的往后走
                    {
                        // 如果之前的点 都没找到怪物, 那就不再去找之前的点, 并且把之前的点 放入 record.
                        // for (int i = lastMinPointIdx; i >= 0; i--)
                        // {
                        //     RecordArrivedMonsterPoint(walkableMonsterPoints[i].Point);
                        // }
                    }

                }

                var lastMinPoint = walkableMonsterPoints[lastMinPointIdx].Point;

                // 如果前面的点 没有怪物了, 那就往后面的点找
                // 如果 当前的位置 在 最近路店的 1m 范围内, 且还没有怪物, 那就往下一个点找
                // note:
                //      寻路是走到指定点的一定范围内结束, 所以 可鞥没走到指定的点 就结束了.
                //      那当前位置可能永远在 目标点的前方, 所有要加个 范围判断, 否咋 永远都是找到这个点，然后做到这个点 
                bool inMinPointArea = CheckInArea(curPoint, lastMinPoint, 1.5f);



                // 如果最后一个 可到达的刷怪点， 那就需要 找到这个 点 相邻的点(不管当前是否可达), 来确定它本来的方向， 从而知道 当前位置点是在前面还是后面
                if (lastMinPointIdx == walkablePointCount - 1)
                {
                    // 如果是最后一个点, 并且在范围之内, 那就不继续往后找点
                    if (inMinPointArea)
                    {
                        return false;
                    }

                    var nextPoint = Vector3.zero;
                    bool isSameDirection = false;
                    // 任何一个点的寻路， 都包含 了出生点 到目标点, 所以 secretMonsterPoint 的长度一定 >=2 
                    var idx = secretMonsterPoint.FindIndex((item) =>
                    {
                        return item.Point == lastMinPoint;
                    });

                    // 如果 最近点是最后一个 点, 那就找 前一个点
                    if (idx == secretMonsterPoint.Count - 1)
                    {
                        nextPoint = secretMonsterPoint[idx - 1].Point;

                        isSameDirection = IsSameDirection(lastMinPoint - nextPoint, curPoint - lastMinPoint);
                    }
                    else
                    {
                        // 否则 找的是 下一个点
                        nextPoint = secretMonsterPoint[idx + 1].Point;


                        isSameDirection = IsSameDirection(nextPoint - lastMinPoint, curPoint - lastMinPoint);
                    }

                    // 如果方向相同, 说明当前点 在最近刷怪点的 后面， 此时 最近点 没有怪物 而且已经走到后面， 那就不需要再回到这个点。
                    if (isSameDirection)
                    {
                        return false;
                    }
                    else
                    {
                        // 如果方向相反, 说明当前点在 刷怪点的 前面， 此时就可以直接走到这个刷怪点
                        findPoint = lastMinPoint;
                        return true;
                    }
                }
                else
                {
                    // 如果不是最后一个可行点
                    var lastMinNextPoint = walkableMonsterPoints[lastMinPointIdx + 1].Point;
                    // 如果从最小点往前找都找不到怪物,那就从最后一个最小点往后找。
                    // 怪物的刷怪逻辑 是从前往后 的点刷的， 所以就要看 最小点 是在当前位置之前的点还是之后的点.


                    if (inMinPointArea)
                    {
                        findPoint = lastMinNextPoint;
                        return true;
                    }


                    // 如果方向不同, 那说明 最小点在后面， 那就返回这个 最小点
                    if (!IsSameDirection(lastMinPoint, lastMinNextPoint, curPoint))
                    {
                        findPoint = lastMinPoint;
                        return true;
                    }
                    else
                    {
                        // 如果方向相同, 表明 最小点在 当前点的前面， 已经走过并且之前的点没有怪物，那就往后走
                        findPoint = lastMinNextPoint;
                        return true;
                    }
                }
            }

        }

        /// <summary>
        /// 查找可以到达的怪物点
        /// </summary>
        /// <param name="findPoint"></param>
        private bool FindCanArriveMonsterPoint(out Vector3 findPoint)
        {
            findPoint = Vector3.zero;

            Vector3 curPoint = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

            // 前面已经做了判断，所以此处 地图对应的 怪物生成点一定能够找到
            List<MonsterPathPoint> secretMonsterPoint = FindMonsterPoints();

            if (secretMonsterPoint == null)
            {
                return false;
            }

            // 可以到达的怪物点
            List<MonsterPathPoint> walkableMonsterPoints = new();

            //1. 首先找一个 寻路距离最近的点
            for (int i = 0; i < secretMonsterPoint.Count; i++)
            {
                var monsterPoint = secretMonsterPoint[i].Point;


                // 空气墙 我担心 可能在前面 也可能在后面， 如果 走过的区域加上了空气墙，那就不能往回走，但是可以往前走
                if (!FindPathManager.Instance.CanArrive(curPoint, monsterPoint))
                {
                    continue;
                }

                bool findResult = FindPathManager.Instance.FindPath(curPoint, monsterPoint, 1, out Vector3[] corners);

                if (!findResult)
                {
                    continue;
                }
                else
                {
                    // 计算当前位置 到这个怪物点的 距离
                    secretMonsterPoint[i].Distance = GetDistance(curPoint, corners);

                    walkableMonsterPoints.Add(secretMonsterPoint[i]);
                }
            }

            var walkablePointCount = walkableMonsterPoints.Count;

            // 如果没有可以到达的点, 那就站着不动，不用干啥
            if (walkablePointCount == 0)
            {
                return false;
            }

            walkableMonsterPoints.Sort((a, b) =>
            {
                return a.Distance.CompareTo(b.Distance);
            });


            for (int i = 0; i < walkablePointCount; i++)
            {
                if (SearchMonsterInPoint(walkableMonsterPoints[i].Point, 500))
                {
                    findPoint = walkableMonsterPoints[i].Point;
                    return true;
                }
            }

            return false;



            // 玩家可以拖动摇杆，所以可能是从怪物的点位托摇杆 到了一个还未产生怪物的出生点， 
            // 此时如果只判断 距离出生点的距离，那可能去的位置没有怪物.
            // 所以就按照 最近的点 往前找

            // 先找距离最小的点, 判断 之前的点有没有怪物
            {
                // note:
                //      由于玩家可以拖动摇杆，所以玩家可能在寻路的中心点, 那么最小可达的点可以有多个
                float minDis = 0;
                for (int i = 0; i < walkablePointCount; i++)
                {
                    if (i == 0)
                    {
                        minDis = walkableMonsterPoints[i].Distance;
                    }
                    else
                    {

                        minDis = math.min(minDis, walkableMonsterPoints[i].Distance);
                    }
                }

                List<int> minPointIdx = new();
                for (int i = 0; i < walkablePointCount; i++)
                {
                    if (minDis == walkableMonsterPoints[i].Distance)
                    {
                        minPointIdx.Add(i);
                    }
                }

                int lastMinPointIdx = minPointIdx[minPointIdx.Count - 1];

                // 从最后 一个最小点，往前找怪物出生点，一旦发现怪物, 那就返回这个怪物点
                // 怪物刷怪 是从前往后的点 一个个刷, 所以先从最近点 往前找, 如果前面的点有怪物, 那就说明是 玩家托遥感拖到后面去了，
                // 先把前面的怪物刷完再说
                {
                    for (int i = lastMinPointIdx; i >= 0; i--)
                    {
                        if (SearchMonsterInPoint(walkableMonsterPoints[i].Point))
                        {
                            findPoint = walkableMonsterPoints[i].Point;
                            return true;
                        }
                    }
                }

                var lastMinPoint = walkableMonsterPoints[lastMinPointIdx].Point;

                // 如果前面的点 没有怪物了, 那就往后面的点找
                // 如果 当前的位置 在 最近路店的 1m 范围内, 且还没有怪物, 那就往下一个点找
                // note:
                //      寻路是走到指定点的一定范围内结束, 所以 可鞥没走到指定的点 就结束了.
                //      那当前位置可能永远在 目标点的前方, 所有要加个 范围判断, 否咋 永远都是找到这个点，然后做到这个点 


                // 如果最后一个 可到达的刷怪点，
                if (lastMinPointIdx == walkablePointCount - 1)
                {
                    return false;
                }
                else
                {
                    for (int i = lastMinPointIdx + 1; i < walkablePointCount; i++)
                    {
                        if (SearchMonsterInPoint(walkableMonsterPoints[i].Point))
                        {
                            findPoint = walkableMonsterPoints[i].Point;
                            return true;
                        }
                    }
                }

                return false;
            }

        }


        private bool CheckInArea(Vector3 point, Vector3 targetPoint, float dis)
        {
            if (targetPoint == point)
            {
                return true;
            }
            var moveOffset = targetPoint - point;
            moveOffset.y = 0;
            return moveOffset.sqrMagnitude <= dis * dis;
        }

        private bool CheckInPlayerArea(Vector3 targetPoint, float dis)
        {
            Vector3 curPoint = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
            return CheckInArea(curPoint, targetPoint, dis);

        }
        private float GetDistance(Vector3 start, Vector3[] pathPoint)
        {
            float distance = 0;
            var startPoint = start;
            var offsetMove = Vector3.zero;
            for (int i = 0; i < pathPoint.Length; i++)
            {
                var point = pathPoint[i];
                offsetMove = point - startPoint;
                offsetMove.y = 0;
                distance += offsetMove.sqrMagnitude;
                startPoint = point;
            }

            return distance;
        }



        bool IsSameDirection(Vector3 start, Vector3 end, Vector3 curPoint)
        {
            Vector3 vA2Cur = curPoint - start;
            Vector3 vA2B = end - start;

            return IsSameDirection(vA2Cur, vA2B);
        }

        bool IsSameDirection(Vector3 dirA, Vector3 dirB)
        {
            var _dirA = dirA;
            var _dirB = dirB;
            _dirA.y = 0;
            _dirB.y = 0;
            if (Vector3.Dot(_dirA, _dirB) < 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 查找一个点 范围内是否有怪物
        /// </summary>
        /// <param name="point"></param>
        /// <param name="area">范围</param>
        private bool SearchMonsterInPoint(Vector3 point, int area = 250)
        {
            // 如果没有怪物了或者怪物 不能被打了, 那就 重新索敌
            var M_Curr = M_MainPlayerCtrlBase.M_Curr;

            // 取的搜索中心点 不再使用 玩家半径, 而改为 startPos
            NPCEntityBase findEnemy = SkillUtils.GetClosestTargetIdByTargets(M_Curr.EntityId, M_Curr.Faction,
                point, area, SelectType.Enemy, true,
                    (entity) =>
                    {
                        if (IsValidEnemyEntity(entity.EntityId))
                        {
                            return true;
                        }
                        return false;
                    });

            // 如果 找不到 怪物了,直接 返回 结果为 false
            if (findEnemy == null)
            {
                return false;
            }
            return true;
        }

        private HashSet<Vector3> arriveMonsterRecord = new();

        private void RecordArrivedMonsterPoint(Vector3 point)
        {
            if (!arriveMonsterRecord.Contains(point))
            {
                arriveMonsterRecord.Add(point);
                ShowPoint(point, $"record_point_{arriveMonsterRecord.Count}", Color.black, 0.8f, 1.5f);

            }
        }

        // 记录当前区域点
        private void RecordCurMonsterPoint(float distance = 10f)
        {
            if (!IsSercetSpace())
            {
                return;
            }
            List<MonsterPathPoint> secretMonsterPoint = FindMonsterPoints();

            if (secretMonsterPoint == null)
            {
                return;
            }

            Vector3 curPoint = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
            secretMonsterPoint.ForEach((m_point) =>
            {
                var checkPoint = m_point.Point;
                var offsetV = (checkPoint - curPoint);
                offsetV.y = 0;
                if (CheckInArea(curPoint, checkPoint, distance))
                {
                    // SGF.Debuger.LogError($"[FindPath] RecordCurMonsterPoint checkPoint {checkPoint} 在 {curPoint} distance: {offsetV.magnitude}");

                    RecordArrivedMonsterPoint(checkPoint);
                }
            });
        }

        private void GotoMonsterPoint(Vector3 point, float maxDistance = 0.1f)
        {

            Vector3 curPoint = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
            if (CheckInArea(curPoint, point, 1.5f))
            {
                RecordCurMonsterPoint(1.5f);
            }

            // 获得当前副本目标对应的 目标点坐标
            // SGF.Debuger.Log($"[FindPath] GotoMonsterPoint goto ---> {point}");

            TryGoToTargetPos((result) =>
            {
                if (GameManager.Instance.M_MainPlayerCtrlBase == null)
                {
                    return;
                }


                // 每到一个点, 标记周围 10m 范围内的刷怪点
                // 如果到了一个怪物点 但是没怪物, 那就 记录这个怪物点,下次不 去找
                RecordCurMonsterPoint();



                // SGF.Debuger.Log($"[FindPath] GotoMonsterPoint result ---> {result}");
                if (result)
                {
                    MarkDirtyState(true);
                    // SGF.Debuger.LogError($"{AUTOBATTLEKEY} [dirty] GotoMonsterPoint ");
                }

            }, E_FindPathType.AutoBattle_MonsterPoint, point, maxDistance);
        }
        #endregion

        #region 地图刷新时 一些数据的处理
        private void ClearAutoBattleRecord()
        {
            arriveMonsterRecord.Clear();
        }

        /// <summary>
        /// 副本 结束的时候 ， 关闭自动战斗
        /// </summary>
        private void OnGameEndID(MessageHandleData data)
        {
            // Stop();
            // 副本结束的时候， 要清除副本相关的所有数据
            ClearPersonSecreteData();
        }

        private void ClearPersonSecreteData()
        {
            cur_gameEndPersonSecretRet = null;
            cur_personSecretLevelDataRet = null;
            // cur_EnterPersonSercetRet = null;
            rewardBoxFlag = false;
        }
        // private EnterPersonSercetRet cur_EnterPersonSercetRet;
        private GameEndPersonSecretRet cur_gameEndPersonSecretRet;
        private PersonSecretLevelDataRet cur_personSecretLevelDataRet;

        /// <summary>
        /// 奖励宝箱的 flag. true 表明已经到了 宝箱地方
        /// </summary>
        private bool rewardBoxFlag = false;

        private void OnEnterPersonSercetRet(MessageHandleData data)
        {
            // 新的 个人秘境数据进来后, 先清除数据
            ClearPersonSecreteData();



            // 个人秘境 进入的数据
            // cur_EnterPersonSercetRet = data.data as EnterPersonSercetRet;
        }

        private void OnGameEndPersonSecretRet(MessageHandleData data)
        {
            cur_gameEndPersonSecretRet = data.data as GameEndPersonSecretRet;

            // Debug.LogError($"[secret]  GameEndPersonSecretRet ---> {cur_gameEndPersonSecretRet} ");
        }

        private void OnPersonSecretLevelDataRetID(MessageHandleData data)
        {
            cur_personSecretLevelDataRet = data.data as PersonSecretLevelDataRet;

            // Debug.LogError($"[secret]  curStage ---> {cur_personSecretLevelDataRet?.Stage} ");
        }

        private int GetCurPersonSecretLevel()
        {
            // 已经走到宝箱地方了, 那就啥都不用干了
            if (rewardBoxFlag)
            {
                return -1;
            }

            // 服务器 现在 默认为 进入 个人秘境一定发送
            if (cur_personSecretLevelDataRet == null)
            {
                return -1;
            }

            // 个人秘境已经 结算了, 并且boss 已经 死亡
            if (cur_gameEndPersonSecretRet != null && cur_personSecretLevelDataRet.Stage == 2)
            {
                return 3;
            }

            // 当 当前阶段 为3,表明 已经杀完  boss
            if (cur_personSecretLevelDataRet.Stage == 3)
            {
                // bose 杀完，但是没有奖励 , 要么已经领取完奖励， 要么断线重连回来。 这个时候 都不需要 走到宝箱
                if (cur_gameEndPersonSecretRet == null)
                {
                    return -1;
                }
                else
                {
                    return 3;
                }

            }

            return cur_personSecretLevelDataRet.Stage;
        }

        private bool CheckCurIsSpaceType(SpaceType checkSpaceType)
        {
            SpaceType curMapType = GameManager.Instance.GetCurMapType();
            return curMapType == checkSpaceType;
        }

        public bool IsSercetSpace()
        {
            return CheckCurIsSpaceType(SpaceType.SpaceSercet);
        }
        #endregion
    }
}

