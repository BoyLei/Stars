using ProtoMsg;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Skill;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProjectDef;
using System;
using UnityEngine;

/// <summary>
///  【通用逻辑层：基类】【（可）创建表现层：驱动于Mono基类】
///  可以理解为（要循环利用的），载具系统马车驱动人1人2，也可以理解为火车的每个车厢，很多伙伴设计的每个伙伴节点，
///  也可以理解为，一个人身上的所有零部件的跟随，一堆宠物跟着自己， Slg群组的每一个士兵，或者nakeNode
/// </summary>
namespace StarProject.Game.Entity.VitalSigns
{
    public class SummonEntityBase : NPCEntityBase
    {
        private string TagFlag => $"[{EntityId}] [SummonEntityBase]";

        //==================================================================
        protected new SummonEntityBase m_nextMyControl;       //（不包含我的召唤物），【我的伙伴，我的熊宝宝】
        internal SummonEntityBase NextFriend { get { return m_nextMyControl; } }

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
        //    M_IsMainPlayerSummon = GameManager.Instance.mainPlayerId == SummonHostID;
        //    //每个Player都将创建一个独立的SkillController
        //    CreateSkillDispatcher(playerData, container);

        //    // 如果是子弹模型
        //    if (m_playerData.dataType == E_EntityDataType.Summon)
        //    {
        //        SummonAttrDataCell summonAttrDataCell = LocalDataManager.Instance.GetSummonAttrDataCell((int)ConfigIndex);
        //        ModelCfg = summonAttrDataCell;
        //        if (summonAttrDataCell != null)
        //        {
        //            M_Name = summonAttrDataCell.Name;
        //            SummonDataCell summonDataCell = LocalDataManager.Instance.GetSummonDataCell(summonAttrDataCell.GetUseTemplate());

        //            if (summonDataCell != null)
        //            {
        //                BoxScaleRatio = UnityEngine.Vector3.one;
        //                ModelRadius = (float)summonDataCell.GetModelRadius() / 100.0f;
        //                BoxCfgScale = UnityEngine.Vector3.one * ModelRadius;

        //                CreateDefaultAvatar(summonDataCell.GetAvatarID());
        //            }
        //            else
        //            {
        //                SGF.Debuger.LogError($"{TagFlag} Create PlayerEnityId=>{EntityId},AOIindexValue={ConfigIndex},id={summonAttrDataCell.GetUseTemplate()},type={m_playerData.dataType},summonDataCell=null");
        //            }
        //        }
        //        else
        //        {
        //            SGF.Debuger.LogError($"{TagFlag} Create PlayerEnityId=>{EntityId},AOIindexValue={ConfigIndex},type={m_playerData.dataType},summonAttrDataCell=null");
        //        }
        //        ViewFactory.CreateViewAddressables("Roles/Template/Summon_Model2", "Roles/Template/Summon_Model2", this, container);
        //    }
        //    else
        //    {
        //        // 如果 是子弹的时候, gl 需要 在子弹创建的时候，客户端就立即判断是否执行子弹的模拟
        //        if (m_playerData.dataType == E_EntityDataType.Bullet)
        //        {
        //            m_playerData.hasModel = false;

        //            LocalDataManager.Instance.GetBulletJson((int)ConfigIndex, (bulletJson) =>
        //            {
        //                ModelCfg = bulletJson;

        //                SetIsSimulateMove(bulletJson.config.IsClientMove);

        //                int modelId = bulletJson.config.AvatarID;

        //                if (modelId != 0)
        //                {
        //                    m_playerData.hasModel = true;
        //                    CreateDefaultAvatar(modelId);
        //                }
        //                else
        //                {
        //                    // 子弹召唤物就加载一个空模型
        //                    if (SummonHostID != 0)
        //                    {
        //                        AvatarDataCell _avatarDataCell = GameManager.Instance.GetEntityAvatarById(SummonHostID);
        //                        if (_avatarDataCell != null)
        //                        {
        //                            avatarDataCell = _avatarDataCell;
        //                        }
        //                    }
        //                }
        //            });
        //        }

        //        ViewFactory.CreateViewAddressables("Roles/Template/Bullet_Model2", "Roles/Template/Bullet_Model2", this, container);
        //    }

        //    if (avatarDataCell != null)
        //    {
        //        SoundManager.Instance.LoadBank(avatarDataCell.SoundBank);
        //    }

        //    InitRegisterAttribute();
        //}

        public void Create(int index, VitalSignData playerData, Transform container, Action<GameObject> cb)
        {
            base.Init(playerData.M_EntityID, playerData.EntityType);
            m_entityBaseWalkSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_WALK_SPEED;
            m_entityBaseRunSpeed = GameConfig.CLIENT_MONSTER_ANIM_BASE_RUN_SPEED;
            m_playerData = playerData;
            Data.ActionOnBattleStateChange += OnBattleStateChange;
            m_data = playerData.viewEnityData;
            m_index = index;
            M_IsMainPlayerSummon = GameManager.Instance.mainPlayerId == SummonHostID;
            //每个Player都将创建一个独立的SkillController
            CreateSkillDispatcher(playerData, container);

            InitRegisterAttribute();

            // 先同步一次 服务器出生点坐标
            ActionOnSyncBorthPos?.Invoke();

            // 如果是
            if (m_playerData.EntityType == E_EntityType.Summon)
            {
                SummonAttrDataCell summonAttrDataCell = LocalDataManager.Instance.GetSummonAttrDataCell((int)ConfigIndex);
                ModelCfg = summonAttrDataCell;
                if (summonAttrDataCell != null)
                {
                    M_Name = summonAttrDataCell.Name;
                    SummonDataCell summonDataCell = LocalDataManager.Instance.GetSummonDataCell(summonAttrDataCell.GetUseTemplate());
                    if (summonDataCell != null)
                    {
                        BoxScaleRatio = UnityEngine.Vector3.one;
                        ModelRadius = (float)summonDataCell.GetModelRadius() / 100.0f;
                        BoxCfgScale = UnityEngine.Vector3.one * ModelRadius;

                        CreateDefaultAvatar(summonDataCell.GetAvatarID());
                    }
                    else
                    {
                        SGF.Debuger.LogWarning($"{TagFlag} Create PlayerEnityId=>{EntityId},AOIindexValue={ConfigIndex},id={summonAttrDataCell.GetUseTemplate()},type={m_playerData.EntityType},summonDataCell=null");
                    }
                }
                else
                {
                    SGF.Debuger.LogWarning($"{TagFlag} Create PlayerEnityId=>{EntityId},AOIindexValue={ConfigIndex},type={m_playerData.EntityType},summonAttrDataCell=null");
                }
                ViewFactory.CreateViewAsync("Roles/Template/Summon_Model2", this, container, cb);
            }
            else
            {
                // 如果 是子弹的时候, gl 需要 在子弹创建的时候，客户端就立即判断是否执行子弹的模拟
                if (m_playerData.EntityType == E_EntityType.BulletEntity)
                {
                    m_playerData.hasModel = false;

                    LocalDataManager.Instance.GetBulletJson((int)ConfigIndex, (bulletJson) =>
                    {
                        ModelCfg = bulletJson;
                        SetIsSimulateMove(bulletJson.config.IsClientMove);
                        int modelId = bulletJson.config.AvatarID;
                        if (modelId != 0)
                        {
                            m_playerData.hasModel = true;
                            CreateDefaultAvatar(modelId);
                        }
                        else
                        {
                            // 子弹召唤物就加载一个空模型
                            if (SummonHostID != 0)
                            {
                                AvatarDataCell _avatarDataCell = GameManager.Instance.GetEntityAvatarById(SummonHostID);
                                if (_avatarDataCell != null)
                                {
                                    avatarDataCell = _avatarDataCell;
                                }
                                if (avatarDataCell != null)
                                {
                                    SoundManager.Instance.LoadBank(avatarDataCell.SoundBank);
                                }
                            }
                        }
                    });
                }
                ViewFactory.CreateViewAsync("Roles/Template/Bullet_Model2", this, container, cb);
            }
        }

        internal override void EnterFrame()
        {
            base.EnterFrame();
        }

        protected override void Release()
        {
            base.Release();
        }

        /// <summary>
        /// 1,先暂停pos发送，校验。
        /// 2,技能停止特殊移动。
        /// 3,技能攻击引发的位移*
        /// 弧形可由路点处理拓展endpos[]
        /// </summary>
        /// <param name="endPos"></param>
        /// <param name="durningTime"></param>
        protected override void SkillViewSmooth(UnityEngine.Vector3 endPos, float durningTime, MoveLabel moveLabel, MoveType moveType, string key, Action<bool> action)
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
                    //朝向：Dir就是x，z没有Y
                    M_EntityMoveDir = pos - CurrentPos;
                    if (m_playerData != null && m_playerData.EntityType == E_EntityType.BulletEntity)
                    {
                        //SGF.Debuger.LogError($"子弹移动调试 MoveByServer111111 id={EntityId},curposition={Position()},nextpos={pos}");
                        DoForceMove?.Invoke(pos);//对于怪来说，验证设定一下，然后继续走（下一个点-最终点）：强制设定然后立刻走下一个
                    }
                    else
                    {
                        ActionOnBirthPos?.Invoke(pos);
                    }
                }
                else
                {
                    //朝向：Dir就是x，z没有Y
                    UnityEngine.Vector3 len = pos - CurrentPos;
                    len.y = 0;
                    lenF = len.magnitude;

                    if (lenF >= GetForceSyncValue())
                    {
                        //自己缓存的向量，算Y的
                        M_EntityMoveDir = pos - CurrentPos;
                        //SGF.Debuger.LogError($"子弹移动调试 MoveByServer 222222222 id={EntityId},curposition={Position()},nextpos={pos},lenF={lenF}");
                        DoForceMove?.Invoke(pos);
                    }
                    else if (Is_MainPlayer_FindingPath == false)
                    {
                        // 如果是在 黑洞状态下, 是否处于移动动画状态 根据服务器发过来的  MoveState 时间戳判断.
                        // 如果时间戳 过大, 表明服务器没收到 客户端的移动状态, 相应的 就不需要播动画
                        if (Data.IsBattleState_BlackHole)
                        {
                            bool stopAnim = (long)SGF.Time.TimeUtils.ServerNowStampMilli - MoveState > 100 && M_eSubState == E_ULayerSubState.SingleMoving;
                            if (stopAnim)
                            {
                                I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.Idle);
                                ChangeState((GameKeyCommand)E_ULayerSubState.Idle, animParam, false);
                            }
                        }
                        else
                        {
                            I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.SingleMoving);
                            ChangeState((GameKeyCommand)E_ULayerSubState.SingleMoving, animParam, false);
                        }
                        DoThdPsnMove?.Invoke(pos);//停止加过程
                    }
                }
            }
        }

        internal override void MoveByServerNew(UnityEngine.Vector3 pos, bool ServerForce, bool isBorn)
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
                }
                else if (ServerForce)
                {
                    //朝向：Dir就是x，z没有Y
                    UnityEngine.Vector3 len = pos - CurrentPos;
                    len.y = 0;
                    lenF = len.magnitude;

                    if (lenF >= GetForceSyncValue())
                    {
                        //自己缓存的向量，算Y的
                        M_EntityMoveDir = pos - CurrentPos;
                        //SGF.Debuger.LogError($"子弹移动调试 MoveByServer 222222222 id={EntityId},curposition={Position()},nextpos={pos},lenF={lenF}");
                        DoForceMove?.Invoke(pos);
                    }
                    else if (Is_MainPlayer_FindingPath == false)
                    {
                        // 如果是在 黑洞状态下, 是否处于移动动画状态 根据服务器发过来的  MoveState 时间戳判断.
                        // 如果时间戳 过大, 表明服务器没收到 客户端的移动状态, 相应的 就不需要播动画
                        if (Data.IsBattleState_BlackHole)
                        {
                            bool stopAnim = (long)SGF.Time.TimeUtils.ServerNowStampMilli - MoveState > 100 && M_eSubState == E_ULayerSubState.SingleMoving;
                            if (stopAnim)
                            {
                                I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.Idle);
                                ChangeState((GameKeyCommand)E_ULayerSubState.Idle, animParam, false);
                            }
                        }
                        else
                        {
                            I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.SingleMoving);
                            ChangeState((GameKeyCommand)E_ULayerSubState.SingleMoving, animParam, false);
                        }
                        DoThdPsnMove?.Invoke(pos);//停止加过程
                    }
                }

                ServerPosition = pos;
            }
        }

        #region AOI属性同步

        protected override void InitRegisterAttribute()
        {
            base.InitRegisterAttribute();
        }

        #endregion

        #region 技能、BUFF、被动效果执行

        protected override void OnActionOnSkillPlayAnim(E_ULayerSubState stage, I_AnimParam i_AnimParam)
        {
            //SGF.Debuger.Log($"{TagFlag} OnActionOnSkillPlayAnim PlayerEnityId=>{EntityId},stage={stage},i_AnimParam={i_AnimParam}");
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

        #region 子弹相关逻辑
        public void OnBulletCreateRet(BulletCreateRet bulletCreateRet)
        {
            skillDispatcher.SkillController.OnBulletCreateRet(bulletCreateRet);
        }

        public void OnBulletEndRet(BulletEndRet bulletEndRet)
        {
            skillDispatcher.SkillController.OnBulletEndRet(bulletEndRet);
        }

        public void OnBulletCreatCusBlackBoard(BaseBlackBoard baseBlackBoard)
        {
            skillDispatcher.SkillController.OnBulletCreatCusBlackBoard(baseBlackBoard);
        }

        #endregion
    }
}
