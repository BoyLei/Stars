using ProtoMsg;
using SGF.Module.Framework;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Module;
using StarProject.Service.Battle;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// 绝对静态：石头
/// 不具备生命体征：子弹（或者说的空体）
/// *这里是具备生命体征的实体
/// //逻辑实体层
/// 
/// ///资源划分：1,细胞【【人：多职业，变身_S：时装_T】【怪物：敌人：Boss_B：伙伴，宝宝】】 :{} 
///2：【武器】
///3：【子弹】【无生命空Gameobject】
///{fx可以包含在上面的任一个Perfab中，但不单独分组}
///{Data包含的无非【从属者子集：【细胞】/【子弹】:【细胞】【武器】}
///@@这里是生命体征基础：一定是管理细胞加载
/// </summary>
/// <summary>
///  【通用逻辑层：基类】【（可）创建表现层：驱动于Mono基类】
///  可以理解为（要循环利用的），载具系统马车驱动人1人2，也可以理解为火车的每个车厢，很多伙伴设计的每个伙伴节点，
///  也可以理解为，一个人身上的所有零部件的跟随，一堆宠物跟着自己， Slg群组的每一个士兵，或者nakeNode
/// </summary>
namespace StarProject.Game.Entity.VitalSigns
{
    //playerC具体的某一个人 ： 比如控制蜀国五虎将组，中的赵云
    public class MonsterEntityBase : NPCEntityBase
    {
        private string TagFlag => $"[{EntityId}] [MonsterEntityBase]";

        //==================================================================
        protected new MonsterEntityBase m_nextMyControl;       //（不包含我的召唤物），【我的伙伴，我的熊宝宝】

        internal MonsterEntityBase NextFriend { get { return m_nextMyControl; } }

        private StarWorldModule swm;
        public StarWorldModule Swm
        {
            get
            {
                //if (swm == null)
                //{
                //    swm = ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule) as StarWorldModule;
                //}
                //return swm;
                return ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule) as StarWorldModule;
            }
            set => swm = value;
        }

        public bool IsBoss = false;
        public int MonType = 1;

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
        //    m_playerData = playerData;
        //    m_entityBaseWalkSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_WALK_SPEED;
        //    m_entityBaseRunSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_RUN_SPEED;
        //    m_playerData.ActionOnBattleStateChange += OnBattleStateChange;
        //    m_data = playerData.viewEnityData;
        //    m_index = index;
        //    IsBoss = false;
        //    IgnoreGravity = false;
        //    //每个Player都将创建一个独立的SkillController
        //    CreateSkillDispatcher(playerData, container);

        //    ///就算没有资源，我也可以分池的设定，有View的Enity，被Enity驱动View
        //    ///资源划分：1,细胞【【人：多职业，变身_S：时装_T】【怪物：敌人：Boss_B：伙伴，宝宝】】 :{其他职业，怪物，都用id区分} 
        //    ///这里是生命体征基础：一定是管理细胞加载
        //    ///规划任意角色都具备（变身，和时装）
        //    ///优先级如下，1默认基础状态，2有时装显示时装，3有变身显示变身（因为没有项目组给变身在做一套时装那么情怀（LangFeiQian））
        //    // 加载模型
        //    uint indexValue = ConfigIndex;
        //    if (m_playerData.EntityType == E_EntityType.Monster)
        //    {
        //        if (m_playerData.IsRobot)
        //        {
        //            RobotDataCell robotDataCell = LocalDataManager.Instance.GetRobotDataCell(indexValue);
        //            ModelCfg = robotDataCell;

        //            if (robotDataCell != null)
        //            {
        //                M_Name = robotDataCell.Name;
        //                BoxScaleRatio = Vector3.one;
        //                ModelRadius = (float)robotDataCell.GetModelRadius() / 100f;
        //                BoxCfgScale = Vector3.one * ModelRadius;

        //                CreateDefaultAvatar(robotDataCell.GetAvatarID());
        //            }
        //            else
        //            {
        //                SGF.Debuger.LogError($"{TagFlag} Create PlayerEnityId=>{EntityId},AOIindexValue={indexValue},type={m_playerData.EntityType},IsRobot={m_playerData.IsRobot},robotDataCell=null");
        //            }
        //        }
        //        else
        //        {
        //            MonsterDataCell monsterDataCell = LocalDataManager.Instance.GetMonsterDataCell(indexValue);
        //            ModelCfg = monsterDataCell;
        //            if (monsterDataCell != null)
        //            {
        //                M_Name = monsterDataCell.Name;
        //                BoxScaleRatio = Vector3.one;
        //                ModelRadius = (float)monsterDataCell.GetModelRadius() / 100f;
        //                BoxCfgScale = Vector3.one * ModelRadius;

        //                CreateDefaultAvatar(monsterDataCell.GetAvatarID());
        //                IsBoss = monsterDataCell.GetMonType() == 3;
        //            }
        //            else
        //            {
        //                SGF.Debuger.LogError($"{TagFlag} Create PlayerEnityId=>{EntityId},AOIindexValue={indexValue},type={m_playerData.EntityType},monsterDataCell=null");
        //            }

        //            // 在地图配置里面查找是否忽略重力
        //            IgnoreGravity = GameManager.Instance.GetEntityIDIsIgnoreGravity(E_EntityType.Monster, indexValue);
        //        }
        //    }
        //    ViewFactory.CreateViewAddressables("Roles/Template/Monster_Model2", "Roles/Template/Monster_Model2", this, container);
        //    InitRegisterAttribute();
        //}

        public void Create(int index, VitalSignData playerData, Transform container, Action<GameObject> cb)
        {
            base.Init(playerData.M_EntityID, playerData.EntityType);
            m_playerData = playerData;
            m_entityBaseWalkSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_WALK_SPEED;
            m_entityBaseRunSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_RUN_SPEED;
            m_playerData.ActionOnBattleStateChange += OnBattleStateChange;
            m_data = playerData.viewEnityData;
            m_index = index;
            IsBoss = false;
            MonType = 1;
            IgnoreGravity = false;
            //每个Player都将创建一个独立的SkillController
            CreateSkillDispatcher(playerData, container);

            uint indexValue = ConfigIndex;
            if (m_playerData.EntityType == E_EntityType.Monster)
            {
                if (m_playerData.IsRobot)
                {
                    RobotDataCell robotDataCell = LocalDataManager.Instance.GetRobotDataCell(indexValue);
                    ModelCfg = robotDataCell;

                    if (robotDataCell != null)
                    {
                        M_Name = robotDataCell.Name;
                        BoxScaleRatio = Vector3.one;
                        ModelRadius = (float)robotDataCell.GetModelRadius() / 100f;
                        BoxCfgScale = Vector3.one * ModelRadius;

                        CreateDefaultAvatar(robotDataCell.GetAvatarID());
                    }
                    else
                    {
                        SGF.Debuger.LogWarning($"{TagFlag} Create PlayerEnityId=>{EntityId},AOIindexValue={indexValue},type={m_playerData.EntityType},IsRobot={m_playerData.IsRobot},robotDataCell=null");
                    }
                }
                else
                {
                    MonsterDataCell monsterDataCell = LocalDataManager.Instance.GetMonsterDataCell(indexValue);
                    ModelCfg = monsterDataCell;
                    if (monsterDataCell != null)
                    {
                        M_Name = monsterDataCell.Name;
                        BoxScaleRatio = Vector3.one;
                        ModelRadius = (float)monsterDataCell.GetModelRadius() / 100f;
                        BoxCfgScale = Vector3.one * ModelRadius;

                        CreateDefaultAvatar(monsterDataCell.GetAvatarID());
                        MonType = monsterDataCell.GetMonType();
                        IsBoss = MonType == 3;
                    }
                    else
                    {
                        SGF.Debuger.LogWarning($"{TagFlag} Create PlayerEnityId=>{EntityId},AOIindexValue={indexValue},type={m_playerData.EntityType},monsterDataCell=null");
                    }

                    // 在地图配置里面查找是否忽略重力
                    IgnoreGravity = GameManager.Instance.GetEntityIDIsIgnoreGravity(E_EntityType.Monster, indexValue);
                }
            }

            InitRegisterAttribute();
            // 先同步一次 服务器出生点坐标
            ActionOnSyncBorthPos?.Invoke();

            ViewFactory.CreateViewAsync("Roles/Template/Monster_Model2", this, container, cb);
        }

        protected override void Reset()
        {
            base.Reset();
            IsBoss = false;
            MonType = 1;
        }

        protected override void Release()
        {
            Swm = null;
            base.Release();
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

        //[首次判断] + 范围更正/[频率服务器控制寻路期间：5次同步一次/追人每次都有]
        //==================================================================

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
            // 客户端校验用的：自己根据自己的策略强拉，但是同步太多了
            if (CurrentPos.z == pos.z && CurrentPos.x == pos.x && isBornOrForceSet == false)
            {
            }
            else
            {
                if (isBornOrForceSet)
                {
                    //朝向：Dir就是x，z没有Y
                    M_EntityMoveDir = pos - CurrentPos;
                    ActionOnBirthPos?.Invoke(pos);

                    if (Swm != null && (Faction == 5 || Faction == 6) && Data != null)
                    {
                        Swm.OnPvpPlayerMove(Data.M_EntityID, pos);
                    }
                    else if (Swm != null && GameManager.Instance.GetCurMapType() == SpaceType.SpaceArena && Data.IsRobot)
                    {
                        Swm.OnSpaceArenaMove(Data.M_EntityID, pos);
                    }
                }
                else
                {
                    Vector3 len = pos - CurrentPos;
                    len.y = 0;
                    lenF = len.magnitude;
                    if (lenF >= GetForceSyncValue())
                    {
                        //自己缓存的向量，算Y的
                        M_EntityMoveDir = pos - CurrentPos;
                        DoForceMove?.Invoke(pos);
                    }
                }
                if (IsBoss)
                {
                    // 如果是boss根据距离显示通知显示血条
                    BattleManager.Instance.DistanceSetEnemy(this);
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
                    //朝向：Dir就是x，z没有Y
                    M_EntityMoveDir = pos - CurrentPos;
                    ActionOnBirthPos?.Invoke(pos);

                    if (Swm != null && (Faction == 5 || Faction == 6) && Data != null)
                    {
                        Swm.OnPvpPlayerMove(Data.M_EntityID, pos);
                    }
                }
                else if (ServerForce)
                {
                    Vector3 len = pos - CurrentPos;
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
                if (IsBoss)
                {
                    // 如果是boss根据距离显示通知显示血条
                    BattleManager.Instance.DistanceSetEnemy(this);
                }
            }
        }

        #region 小地图通知

        public override void OnFinalPosChange(UnityEngine.Vector3 newVector3)
        {
            //底层之间直接发送，不用action
            if (Swm != null && Data != null)
            {
                Swm.OnMonsterMove(Data.M_EntityID, newVector3);

                if (Faction == 5 || Faction == 6)
                {
                    Swm.OnPvpPlayerMove(Data.M_EntityID, newVector3);
                }
                else if (Swm != null && GameManager.Instance.GetCurMapType() == SpaceType.SpaceArena && Data.IsRobot)
                {
                    Swm.OnSpaceArenaMove(Data.M_EntityID, newVector3);
                }
            }
            base.OnFinalPosChange(newVector3);
        }

        #endregion

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
            if (!string.IsNullOrEmpty(M_Appellation) && !string.IsNullOrWhiteSpace(M_Appellation) && M_Appellation.Contains("|"))
            {
                var splite = M_Appellation.Split("|");
                if (splite.Length > 0)
                {
                    string languageKey = splite[0];
                    string appellation = splite[1];
                    M_Appellation = string.Format(LanguageManager.Instance.GetLanguageByKey(languageKey), appellation);
                }
            }
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
