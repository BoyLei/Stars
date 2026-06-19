using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;

namespace StarProject.Game.Entity.VitalSigns
{
    public class GameNPCEntityBase : NPCEntityBase
    {
        private string TagFlag => $"[{EntityId}] [GameNPCEntityBase]";

        //==================================================================
        protected new GameNPCEntityBase m_nextMyControl;       //（不包含我的召唤物），【我的伙伴，我的熊宝宝】

        internal GameNPCEntityBase NextFriend { get { return m_nextMyControl; } }

        //------------------------------------------------------------------------

        /// <summary>
        /// 创建逻辑数据必要的，需要玩家数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="playerData"></param>
        /// <param name="container">角色们找个通用的角色挂点就行了</param>
        //public void Create(int index, VitalSignData playerData, Transform container)
        //{
        //    base.Init(playerData.M_EntityID, playerData.EntityType);
        //    m_entityBaseWalkSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_WALK_SPEED;
        //    m_entityBaseRunSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_RUN_SPEED;
        //    m_playerData = playerData;
        //    Data.ActionOnBattleStateChange += OnBattleStateChange;
        //    m_data = playerData.viewEnityData;
        //    m_index = index;
        //    IgnoreGravity = false;

        //    //每个Player都将创建一个独立的SkillController
        //    CreateSkillDispatcher(playerData, container);

        //    // 加载模型
        //    NpcDataCell npcDataCell = LocalDataManager.Instance.GetNPCDataCell(ConfigIndex);
        //    ModelCfg = npcDataCell;
        //    if (npcDataCell != null)
        //    {
        //        M_Name = npcDataCell.Name;

        //        int avatarID = npcDataCell.GetAvatarID();
        //        // 服务器化身ID是根据选择的副本不同，会修改ID，创建的时候如果有值，客户端就不读取配置表的id
        //        int teampID = AttrData.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.AvatarID);
        //        if (teampID != 0)
        //        {
        //            avatarID = teampID;
        //        }

        //        // 刷新 avatarID
        //        CreateDefaultAvatar(avatarID);
        //        // 在地图配置里面查找是否忽略重力
        //        IgnoreGravity = GameManager.Instance.GetEntityIDIsIgnoreGravity(E_EntityDataType.Npc, ConfigIndex);
        //    }
        //    else
        //    {
        //        SGF.Debuger.LogError($"{TagFlag} Create NpcEnityId=>{EntityId},AOIindexValue={ConfigIndex},monsterDataCell=null");
        //    }
        //    ViewFactory.CreateViewAddressables("Roles/Template/NPC_Model2", "Roles/Template/NPC_Model2", this, container);

        //    InitRegisterAttribute();
        //}

        public void Create(int index, VitalSignData playerData)
        {
            base.Init(playerData.M_EntityID, playerData.EntityType);
            m_entityBaseWalkSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_WALK_SPEED;
            m_entityBaseRunSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_RUN_SPEED;
            m_playerData = playerData;
            Data.ActionOnBattleStateChange += OnBattleStateChange;
            m_data = playerData.viewEnityData;
            m_index = index;
            IgnoreGravity = false;

            //每个Player都将创建一个独立的SkillController
            //CreateSkillDispatcher(playerData, container);

            // 加载模型
            NpcDataCell npcDataCell = LocalDataManager.Instance.GetNPCDataCell(ConfigIndex);
            ModelCfg = npcDataCell;
            if (npcDataCell != null)
            {
                M_Name = npcDataCell.Name;

                int avatarID = npcDataCell.GetAvatarID();
                // 服务器化身ID是根据选择的副本不同，会修改ID，创建的时候如果有值，客户端就不读取配置表的id
                int teampID = AttrData.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.AvatarID);
                if (teampID != 0)
                {
                    avatarID = teampID;
                }

                // 刷新 avatarID
                CreateDefaultAvatar(avatarID);
                // 在地图配置里面查找是否忽略重力
                IgnoreGravity = GameManager.Instance.GetEntityIDIsIgnoreGravity(E_EntityType.Npc, ConfigIndex);
            }
            else
            {
                SGF.Debuger.LogError($"{TagFlag} Create NpcEnityId=>{EntityId},AOIindexValue={ConfigIndex},monsterDataCell=null");
            }

            InitRegisterAttribute();
        }

        public void CreateModel(Transform container, Action<GameObject> cb)
        {
            // 先同步一次 服务器出生点坐标
            ActionOnSyncBorthPos?.Invoke();
            ViewFactory.CreateViewAsync("Roles/Template/NPC_Model2", this, container, cb);
        }

        protected override void Release()
        {
            base.Release();
        }

        internal override void EnterFrame()
        {
            base.EnterFrame();
        }

        /// <summary> AOI [坐标] 变化 </summary>
        //[首次判断] + 范围更正/[频率服务器控制寻路期间：5次同步一次/追人每次都有]
        //==================================================================
        /// <summary>
        /// 服务器来消息，控制其他玩家
        /// 出生控制【全部人员】；非出生控制【只能非主角】
        /// 怪[组数据]，和主角[自己算]，会强拉。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isComeFromServer">服务器设置，听服务器的，那是客户端自己算的</param>
        internal override void MoveByServer(UnityEngine.Vector3 pos, bool isBornOrForceSet)
        {
            float lenF = 0;
            // 客户端校验用的：自己根据自己的策略强拉，但是同步太多了
            if (CurrentPos.z == pos.z && CurrentPos.x == pos.x && isBornOrForceSet == false)
            {
            }
            else
            {
                if (isBornOrForceSet)
                {
                    M_EntityMoveDir = pos - CurrentPos;
                    ActionOnBirthPos?.Invoke(pos);
                    int indexValue = m_playerData.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.Rot);
                    ServerSetRotation(indexValue, false, true, 0);
                }
                else
                {
                    ////拉也要走完，你先别拉
                    //朝向：Dir就是x，z没有Y
                    UnityEngine.Vector3 len = pos - CurrentPos;
                    len.y = 0;
                    lenF = len.magnitude;
                    if (lenF >= GetForceSyncValue())
                    {
                        //自己缓存的向量，算Y的
                        M_EntityMoveDir = pos - CurrentPos;
                        DoForceMove?.Invoke(pos);
                    }
                }
            }
        }

        internal override void MoveByServerNew(Vector3 pos, bool ServerForce, bool isBorn)
        {
            float lenF = 0;
            // 客户端校验用的：自己根据自己的策略强拉，但是同步太多了
            if (CurrentPos.z == pos.z && CurrentPos.x == pos.x && (ServerForce && isBorn) == false)
            {
            }
            else
            {
                if (isBorn)
                {
                    M_EntityMoveDir = pos - CurrentPos;
                    ActionOnBirthPos?.Invoke(pos);
                    int indexValue = m_playerData.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.Rot);
                    ServerSetRotation(indexValue, false, true, 0);
                }
                else if (ServerForce)
                {
                    ////拉也要走完，你先别拉
                    //朝向：Dir就是x，z没有Y
                    UnityEngine.Vector3 len = pos - CurrentPos;
                    len.y = 0;
                    lenF = len.magnitude;
                    if (lenF >= GetForceSyncValue())
                    {
                        //自己缓存的向量，算Y的
                        M_EntityMoveDir = pos - CurrentPos;
                        DoForceMove?.Invoke(pos);
                    }
                }
                ServerPosition = pos;
            }
        }

        public void LookAtMainPlayer()
        {
            float y = Skill.Utils.SkillUtils.GetOrientationBuilderRotate(GameManager.Instance.MainPlayerEnityId, EntityId);
            DoAngelChange?.Invoke(y, true, 0, false); //服务器设置角度
        }

        public void RecoverCfgAngel()
        {
            DoAngelChange?.Invoke(m_clientViewAngle.y, true, 0, false);
        }

        #region AOI属性同步

        protected override void InitRegisterAttribute()
        {
            base.InitRegisterAttribute();

            OnAOINameChange(AOIAttrDefine.Alias, null);
            OnAOITitleChange(AOIAttrDefine.Title, null);

            //Data.RegisterAttribute(AOIAttrDefine.Name, OnAOINameChange);
            //Data.RegisterAttribute(AOIAttrDefine.Title, OnAOITitleChange);
        }

        private void OnAOINameChange(string key, object value)
        {
            string name = m_playerData.Attrs.GetAoiValue<string>(EnumAOIType.String, key);
            if (!string.IsNullOrEmpty(name))
            {
                string[] names = name.Split('@');
                if (names.Length > 1)
                {
                    M_Name = string.Format(LanguageManager.Instance.GetLanguageByKey(names[1]),names[0]);
                }
                else
                {
                    M_Name = LanguageManager.Instance.GetLanguageByKey(name);
                }
            }
        }

        private void OnAOITitleChange(string key, object value)
        {
            M_Appellation = m_playerData.Attrs.GetAoiValue<string>(EnumAOIType.String, key);
        }

        #endregion

        #region 技能、BUFF、被动效果执行

        protected override void OnActionOnSkillPlayAnim(E_ULayerSubState stage, I_AnimParam i_AnimParam)
        {
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillPlayAnim PlayerEnityId=>{EnityId},stage={stage},i_AnimParam={i_AnimParam}");
            ForceSetState(stage, i_AnimParam);
        }

        protected override void OnActionOnSkillAnimation(E_ULayerSubState stage)
        {
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillAnimation PlayerEnityId=>{EnityId},stage={stage}");
            ForceSetState(stage, null);
        }

        protected override void OnActionOnSkillPlayFx(I_FxParam i_FxParam)
        {
            ulong buildId = i_FxParam.BuilderID;
            //ulong ownerId = i_FxParam.OwnerID;
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillPlayFx PlayerEnityId=>{EnityId},i_FxParam={i_FxParam}");
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
                SGF.Debuger.LogWarning($"{TagFlag} OnActionOnSkillPlayFx PlayerEnityId=>{EntityId},i_FxParam={i_FxParam},avatarData=null");
                return;
            }
            ActionOnPlaySpecialEffects?.Invoke(i_FxParam, "", avatarData.EffectsPath);
        }

        protected override void OnActionOnSkillStopFx(I_FxParam i_FxParam)
        {
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillStopFx PlayerEnityId=>{EnityId},i_FxParam={i_FxParam}");
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



    }
}
