using ProtoMsg;
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
///包含：主角，和其他玩家
/// </summary>
using SGF.Module.Framework;
using SGF.Network;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Skill;
using StarProject.Module;
using StarProject.Service.Battle;
using StarProject.Service.Business;
using StarProject.Service.FindPath;
using StarProject.Service.Input;
using StarProject.Service.LocalData;
using StarProject.Service.SDK;
using StarProject.Service.User;
using StarProjectDef;
using System;
using System.Collections.Generic;
using SGF.Time;
using SGF.Unity;
using StarProject.Game.Player;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
///  【通用逻辑层：基类】【（可）创建表现层：驱动于Mono基类】
///  可以理解为（要循环利用的），载具系统马车驱动人1/人2，也可以理解为火车的每个车厢，很多伙伴设计的每个伙伴节点，
///  也可以理解为，一个人身上的所有零部件的跟随，一堆宠物跟着自己， Slg群组的每一个士兵，或者nakeNode
/// </summary>
namespace StarProject.Game.Entity.VitalSigns
{
    //playerC具体的某一个人 ： 比如控制蜀国五虎将组，中的赵云
    public class HeroEntityBase : NPCEntityBase
    {
        private string TagFlag => $"[{EntityId}] [HeroEntityBase]";

        //==================================================================

        protected new HeroEntityBase m_nextMyControl; //（不包含我的召唤物），【我的伙伴，我的熊宝宝】||伙伴再召唤的宝宝

        internal HeroEntityBase NextFriend
        {
            get { return m_nextMyControl; }
        }

        public int TeamId
        {
            get { return m_playerData.teamId; }
        }
        public bool IsClientPrepareInter { get; set; }
        #region 主角发送坐标和角度

        private SocketBase M_BattleSocket;
        private ProtoMsg.MoveMsg2 moveMsg2 = new();
        private ProtoMsg.Vector3 vector3 = new();
        private DateTimeOffset dateTimeOffset = new(DateTime.UtcNow);
        private Vector3 m_toServerPos; //客户端本地操作平滑，服务器不必，其他客户端自己弥补
        private bool m_PostTag; //发送标签

        /// <summary>
        /// 制造的职业
        /// </summary>
        private int CreateJob = 0;

        // 发送移动坐标的上一次数据
        private Vector3 m_oldPos;
        private int m_oldServerRot;

        #endregion

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


        private bool IsRobot = false; // 是否机器人

        /// <summary>
        /// 创建逻辑数据必要的，需要玩家数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="playerData"></param>
        /// <param name="container">角色们找个通用的角色挂点就行了</param>
        //        public void Create(int index, VitalSignData playerData, Transform container)
        //        {
        //            base.Init(playerData.M_EntityID, playerData.EntityType);
        //            IsMainPlayer = playerData.isMainPlayer;
        //            IsNeedListenPathUpdate = true;
        //            m_entityBaseWalkSpeed = GameConfig.CLIENT_HERO_ANIM_BASE_WALK_SPEED;
        //            m_entityBaseRunSpeed = GameConfig.CLIENT_HERO_ANIM_BASE_RUN_SPEED;
        //            m_playerData = playerData;
        //            m_playerData.ActionOnBattleStateChange += OnBattleStateChange;
        //            if (IsMainPlayer)
        //            {
        //                M_BattleSocket = NetworkManager.Instance.gameSocket;
        //            }
        //            //--------------
        //            m_data = playerData.viewEnityData;
        //            m_index = index;

        //            //每个Player都将创建一个独立的skillDispatcher
        //            CreateSkillDispatcher(playerData, container);
        //            skillDispatcher.SkillController.ActionOnStartSkillStage += OnActionOnStartSkillStage;
        //            skillDispatcher.SkillController.ActionOnEndSkillStage += OnActionOnEndSkillStage;

        //            ///就算没有资源，我也可以分池的设定，有View的Enity，被Enity驱动View
        //            ///资源划分：1,细胞【【人：多职业，变身_S：时装_T】【怪物：敌人：Boss_B：伙伴，宝宝】】 :{其他职业，怪物，都用id区分} 
        //            ///这里是生命体征基础：一定是管理细胞加载
        //            ///规划任意角色都具备（变身，和时装）
        //            ///优先级如下，1默认基础状态，2有时装显示时装，3有变身显示变身（因为没有项目组给变身在做一套时装那么情怀（LangFeiQian））
        //            m_JobID = m_playerData.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
        //            JobDataCell jobDataCell = LocalDataManager.Instance.GetJobDataCell((int)m_JobID);
        //            if (jobDataCell != null)
        //            {
        //                int model = jobDataCell.GetAvatarID();
        //#if UNITY_EDITOR
        //                if (SkillEditorGM.g_modelID != -1)
        //                {
        //                    model = SkillEditorGM.g_modelID;
        //                    SkillEditorGM.g_modelID = -1;
        //                }
        //#endif
        //                CreateDefaultAvatar(model);

        //            }
        //            ViewFactory.CreateViewAddressables("Roles/Template/Character_Model2", "Roles/Template/Character_Model2", this, container);

        //            InitRegisterAttribute();
        //        }
        public void Create(int index, VitalSignData playerData, Transform container, Action<GameObject> cb = null)
        {
            base.Init(playerData.M_EntityID, playerData.EntityType);
            IsMainPlayer = playerData.isMainPlayer;
            IsNeedListenPathUpdate = true;
            m_entityBaseWalkSpeed = GameConfig.CLIENT_HERO_ANIM_BASE_WALK_SPEED;
            m_entityBaseRunSpeed = GameConfig.CLIENT_HERO_ANIM_BASE_RUN_SPEED;
            m_playerData = playerData;
            m_playerData.ActionOnBattleStateChange += OnBattleStateChange;
            if (IsMainPlayer)
            {
                M_BattleSocket = NetworkManager.Instance.gameSocket;
            }

            //--------------
            m_data = playerData.viewEnityData;
            m_index = index;

            //每个Player都将创建一个独立的skillDispatcher
            CreateSkillDispatcher(playerData, container);
            skillDispatcher.SkillController.ActionOnStartSkillStage += OnActionOnStartSkillStage;
            skillDispatcher.SkillController.ActionOnEndSkillStage += OnActionOnEndSkillStage;

            ///就算没有资源，我也可以分池的设定，有View的Enity，被Enity驱动View
            ///资源划分：1,细胞【【人：多职业，变身_S：时装_T】【怪物：敌人：Boss_B：伙伴，宝宝】】 :{其他职业，怪物，都用id区分} 
            ///这里是生命体征基础：一定是管理细胞加载
            ///规划任意角色都具备（变身，和时装）
            ///优先级如下，1默认基础状态，2有时装显示时装，3有变身显示变身（因为没有项目组给变身在做一套时装那么情怀（LangFeiQian））
            m_JobID = m_playerData.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
            JobDataCell jobDataCell = LocalDataManager.Instance.GetJobDataCell((int)m_JobID);
            if (jobDataCell != null)
            {
                int model = jobDataCell.GetAvatarID();
#if UNITY_EDITOR
                if (SkillEditorGM.g_modelID != -1)
                {
                    model = SkillEditorGM.g_modelID;
                    SkillEditorGM.g_modelID = -1;
                }
#endif
                CreateDefaultAvatar(model);
            }

            InitRegisterAttribute();
            // 先同步一次 服务器出生点坐标
            ActionOnSyncBorthPos?.Invoke();
            ViewFactory.CreateViewAsync("Roles/Template/Character_Model2", this, container, cb);
        }


        protected override void Release()
        {
            skillDispatcher.SkillController.ActionOnStartSkillStage -= OnActionOnStartSkillStage;
            skillDispatcher.SkillController.ActionOnEndSkillStage -= OnActionOnEndSkillStage;
            Swm = null;
            IsRobot = false;
            M_BattleSocket = null;
            base.Release();
        }

        internal override void EnterFrame()
        {
            base.EnterFrame();
            // 过去式，用不到了
            //技能特殊移动
            //if (skillMovePara.SkillMoveDone == false)
            //{
            //    skillMovePara.TriSec -= SEC_PER_FRAME;
            //    ViewEnterFrameAction?.Invoke(skillMovePara.M_TotleMoveDisVec);
            //}
            //else
            //{
            //    //执行完毕
            //    ViewEnterFrameAction?.Invoke(Vector3.zero);
            //}

            //1中间会拦截信息是否推送
            //2就算推送也在下面写如拦截会空个N帧数，3但是最大不会超过30帧和1米的最小约束条件
            if (m_PostTag)
            {
                SendSyncMoveMsg();
                m_PostTag = false;
            }

            // 【主角客户端状态】
            // 客户端自己的维护的状态，如果小于0，就回归普通状态
            //if (Data.isMainPlayer)
            {
                if (M_clientTag == E_ClentMainPlayerState.Battle)
                {
                    if (M_clientStateCountdown > 0)
                    {
                        M_clientStateCountdown -= Time.fixedDeltaTime;
                        if (M_clientStateCountdown <= 0)
                        {
                            BattleManager.Instance.TextRenderQueue = 0;
                            M_clientTag = E_ClentMainPlayerState.Normal;
                        }
                    }
                }
            }
        }

        protected override void SkillViewSmooth(Vector3 endPos, float durningTime, MoveLabel moveLabel,
            MoveType moveType, string key, Action<bool> action)
        {
            //FrameLogicSmooth(endPos, durningTime);//逻辑驱动表现平滑，也不用逻辑驱动了
            DoSkillPathMove?.Invoke(endPos, durningTime, moveLabel, moveType, key, action);
        }

        // --------- 服务器来消息，控制其他玩家,后续属性通知不必强制设置主角
        /// <summary>
        /// 服务器来消息，控制其他玩家
        /// 出生控制【全部人员】；非出生控制【只能非主角】
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isComeFromServer">服务器设置，听服务器的，那是客户端自己算的</param>
        internal override void MoveByServer(Vector3 pos, bool isBornOrForceSet)
        {
            //===============不包含主角================
            float lenF = 0;
            if (CurrentPos.z == pos.z && CurrentPos.x == pos.x && isBornOrForceSet == false)
            {
            }
            else
            {
                if (isBornOrForceSet) //首次，强制
                {
                    //SGF.Debuger.Log($"{TagFlag} 坐标-- 强拉 MoveByServer PlayerEnityId={EnityId},CurrentPos={CurrentPos},pos={pos},anglesY=》{m_angles.y}");
                    M_EntityMoveDir = pos - CurrentPos;
                    ActionOnBirthPos?.Invoke(pos);

                    if (Swm != null && (Faction == 5 || Faction == 6) && Data != null && !Data.isMainPlayer)
                    {
                        Swm.OnPvpPlayerMove(Data.M_EntityID, pos);
                    }
                    else if (Swm != null && !Data.isMainPlayer && GameManager.Instance.GetCurMapType() == SpaceType.SpaceArena)
                    {
                        Swm.OnSpaceArenaMove(Data.M_EntityID, pos);
                    }
                }
                else
                {
                    if (Data.isMainPlayer)
                    {
                        Vector3 len = pos - CurrentPos;
                        len.y = 0;
                        lenF = len.magnitude;
                        // 如果是黑洞的话，目前服务器下发的属性坐标同步可能回 发生强拉,所以屏蔽掉黑洞装填的强拉
                        if (lenF >= GetForceSyncValue() && !Data.IsBattleState_BlackHole)
                        {
                            //自己缓存的向量，算Y的
                            M_EntityMoveDir = pos - CurrentPos;
                            //主角不必验证，但是也啦下
                            // SGF.Debuger.LogError($"{TagFlag} , 发生了 强制位移同步 pos : {pos}");
                            DoForceMove.Invoke(pos); //不影响过程，直接啦
                        }
                    }
                    else if (IsRobot)
                    {
                        // 如果是机器人，坐标同步只是矫正位置
                        Vector3 len = pos - CurrentPos;
                        len.y = 0;
                        lenF = len.magnitude;
                        if (lenF >= GetForceSyncValue() && !Data.IsBattleState_BlackHole)
                        {
                            //自己缓存的向量，算Y的
                            M_EntityMoveDir = pos - CurrentPos;
                            DoForceMove?.Invoke(pos);
                        }
                    }
                    else
                    {
                        // 如果是在 黑洞状态下, 是否处于移动动画状态 根据服务器发过来的  MoveState 时间戳判断.
                        // 如果时间戳 过大, 表明服务器没收到 客户端的移动状态, 相应的 就不需要播动画
                        if (Data.IsBattleState_BlackHole)
                        {
                            bool stopAnim = (long)SGF.Time.TimeUtils.ServerNowStampMilli - MoveState > 100 &&
                                            M_eSubState == E_ULayerSubState.SingleMoving;
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

                        DoThdPsnMove?.Invoke(pos); //停止加过程
                    }
                }
            }
        }

        internal override void MoveByServerNew(Vector3 pos, bool ServerForce, bool isBorn)
        {
            //===============不包含主角================
            float lenF = 0;
            if (CurrentPos.z == pos.z && CurrentPos.x == pos.x && (ServerForce && isBorn) == false)
            {
            }
            else
            {
                if (isBorn) //首次，强制
                {
                    //SGF.Debuger.Log($"{TagFlag} 坐标-- 强拉 MoveByServer PlayerEnityId={EnityId},CurrentPos={CurrentPos},pos={pos},anglesY=》{m_angles.y}");
                    M_EntityMoveDir = pos - CurrentPos;
                    //SetCurrentPos(pos);
                    ActionOnBirthPos?.Invoke(pos);

                    if (Swm != null && (Faction == 5 || Faction == 6) && Data != null && !Data.isMainPlayer)
                    {
                        Swm.OnPvpPlayerMove(Data.M_EntityID, pos);
                    }
                }
                else if (ServerForce)
                {
                    if (Data.isMainPlayer)
                    {
                        Vector3 len = pos - CurrentPos;
                        len.y = 0;
                        lenF = len.magnitude;
                        // 如果是黑洞的话，目前服务器下发的属性坐标同步可能回 发生强拉,所以屏蔽掉黑洞装填的强拉
                        if (lenF >= GetForceSyncValue() && !Data.IsBattleState_BlackHole)
                        {
                            //自己缓存的向量，算Y的
                            M_EntityMoveDir = pos - CurrentPos;
                            //主角不必验证，但是也啦下
                            // SGF.Debuger.LogError($"{TagFlag} , 发生了 强制位移同步 pos : {pos}");
                            DoForceMove.Invoke(pos); //不影响过程，直接啦
                        }
                    }
                    else if (IsRobot)
                    {
                        // 如果是机器人，坐标同步只是矫正位置
                        Vector3 len = pos - CurrentPos;
                        len.y = 0;
                        lenF = len.magnitude;
                        if (lenF >= GetForceSyncValue() && !Data.IsBattleState_BlackHole)
                        {
                            //自己缓存的向量，算Y的
                            M_EntityMoveDir = pos - CurrentPos;
                            DoForceMove?.Invoke(pos);
                        }
                    }
                    else
                    {
                        // 如果是在 黑洞状态下, 是否处于移动动画状态 根据服务器发过来的  MoveState 时间戳判断.
                        // 如果时间戳 过大, 表明服务器没收到 客户端的移动状态, 相应的 就不需要播动画
                        if (Data.IsBattleState_BlackHole)
                        {
                            bool stopAnim = (long)SGF.Time.TimeUtils.ServerNowStampMilli - MoveState > 100 &&
                                            M_eSubState == E_ULayerSubState.SingleMoving;
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

                        DoThdPsnMove?.Invoke(pos); //停止加过程
                    }
                }

                if (ServerForce)
                {
                    ServerPosition = pos;
                }
            }
        }

        #region 主角遥感控制的移动

        private Vector3 tempDirction = Vector3.zero; // 主角遥感控制缓存的临时遍历

        /// <summary>
        /// 【主角】客户端自我控制
        /// </summary>
        /// <param name="targetPos">目标点：主角的</param>
        internal void ClientSetMoveByFixFrame(Vector3 targetPos, UnityEngine.Vector3 joyMoveDirction)
        {
            Vector3 oldPos = CurrentPos;

            bool isBattleState_BlackHole = Data.IsBattleState_BlackHole;
            bool isForbidMove = Data.Is___ForbidMove;


            tempDirction = targetPos - CurrentPos;

            //处理【位移】并
            if (!isForbidMove) //可移动
            {
                // 当玩家没有收到 黑洞牵引状态的时候, 就是按摇杆移动的方式移动
                if (!isBattleState_BlackHole)
                {
                    //等显示层取
                    M_EntityMoveDir = tempDirction;
                }
                else
                {
                    // 如果 受到了黑洞牵引,那客户端摇杆移动 不做移动,而是将移动的朝向指令发送给服务器
                    //M_EntityMoveDir = Vector3.zero;
                }

                skillDispatcher.SkillController.OnClientMove();

                // 主角移动时遮挡剔除镂空
                Shader.SetGlobalVector("_PlayerPos", CurrentPos);
            }

            OnJoyStickMove(targetPos);

            float oldAngle = EulerAngles.y;
            float newAngle = 0;
            /// 2023/9/25
            /// 收到客户端摇杆后对玩家朝向的提前 本地处理.
            /// note:
            ///     1.如果没有 黑洞效果, 那就是直接采用本地计算 直接设置玩家朝向;
            ///     2.如果  有 黑洞效果, 那本地不应该设置朝向,而改由 服务器同步坐标和朝向才对;
            ///       但是:
            ///           目前移动 同步的协议, 夏哥的说法是 客户端通过 rot 发送客户端的移动指令(先复用之前的协议,不拓展字段),
            ///           此时，服务器通过 rot这个字段 当作客户端的 操作指令,摒弃 里面的坐标, 做跟黑洞偏移的运算.
            ///           所以如果 客户端不做朝向的 预播, 可能就没办法同步朝向问题.
            ///           所以 此处 哪怕受到了 黑洞效果, 客户端依旧先 设置自己的朝向
            {
                //处理【（移动/技能_摇杆朝向）】  
                if (!IsForbidJoyStickDir && !Data.Is___ForbidDir /*|| !Is___SkillJoyStick*/) //没锁朝向 并且 技能摇杆处理方向
                {
                    //朝向：Dir就是x，z没有Y
                    //M_EntityAnglesDir = targetPos - oldPos;//因为要么别人，要么自己，所以公用|同时存在时服务器信息更新客户端
                    //SetAngle(M_EntityAnglesDir);
                    ClientSetRotationByDir(tempDirction, true, 0);
                    newAngle = EulerAngles.y;

                    float interpolation = oldAngle - newAngle;
                    interpolation %= 360; //-360 ~ 360

                    if (Math.Abs(interpolation) > GameConfig.C2S_MINI_SYNC_ANGLE)
                    {
                        m_PostTag |= true;
                    }
                    //SGF.Debuger.LogError($"{TagFlag} 技能 自动转向 ----遥感转向------- m_angles={m_angles.y},,,m_ServerAngles={m_ServerAngles}");
                }
            }

            m_EntityJoyMoveDir = Vector3.zero;
            m_EntityJoyMoveSendServerAngle = 0;

            // 如果 收到了 黑洞效果的影响, 那么只要摇杆有操作,都需要给服务器发送摇杆指令
            if (isBattleState_BlackHole)
            {
                // 设置玩家当前的 摇杆指令
                m_EntityJoyMoveDir = joyMoveDirction;

                // 不管玩家角度 有没有变化, 此时都需要 发送 这个 移动摇杆指令 给服务器
                m_PostTag |= true;

                // 1.如果 此时 朝向的原子锁 锁住了,但是 受到 黑洞效果影响, 比如 原子锁锁住玩家向右，但是摇杆向左,并且玩家向左移动.
                //   此时 仍旧需要将 摇杆操作的朝向发给服务器，但此时 不是 玩家自己的朝向
                //   !IsForbidJoyStickDir && Data.Is___ForbidDir && !isForbidMove
                //
                // 2.如果此时没有被朝向的原子锁锁住,那基于上面客户端朝向的预播角度逻辑,客户端会 先本地预播朝向.
                //   同时需要向服务器 发送客户端此时的摇杆朝向角度指令.
                // 
                // 由此:
                //   只要客户端处于黑洞状态的影响执行,客户端都需要给服务器发送摇杆指令
                m_EntityJoyMoveSendServerAngle =
                    (int)(Math.Atan2(tempDirction.z, tempDirction.x) * Mathf.Rad2Deg % 360);

                // 处于黑洞控制下,摇杆操作朝向预期的 移动到的目标角度
                newAngle = (float)(Math.Atan2(tempDirction.x, tempDirction.z) * Mathf.Rad2Deg) % 360;
                // SGF.Debuger.LogWarning(
                //     $"{TagFlag} [黑洞] 摇杆移动: Pos: {CurrentPos} ---> {tempDirction},Rotate: {oldAngle} ---> {newAngle}, server angle: {ClientRotToServerRot} ---> {m_EntityJoyMoveSendServerAngle} ");
            }

            // 不用的数据还原
            tempDirction = Vector3.zero;
            Is___JoySitckStop =
                Data.Is___ForbidMove && Data.Is___ForbidDir; //普通移动事件【L】：既禁止移动：又禁止朝向就等于禁止摇杆；省性能的思路，但是要在摇杆里面控制

            /// 2023/12/5
            /// 修一个 移动 途中方技能， 因为没有原子锁变化， 导致 移动状态 没办法刷新 动画的 bug。 
            /// 目前 会跟 Is___JoySitckStop 重复 多刷一次 SetMovmentState 移动状态. 但是 没看到太大问题
            /// 
            /// TODO: 曲
            /// 这块需要 想一下 如何优化 移动发送状态 变化 指令
            SetMovmentState();

            if (!Is___JoySitckStop)
            {
                // 主角如果有遥感输入，就暂停主角的自动寻路
                BreakFindPath();
            }
        }

        public void OnBeginCreating(int job, string animation)
        {
            CreateJob = job;

            GameKeyCommand command = GetNextInterState();
            I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.InterAction1, animation);
            ChangeState(command, animParam, false);
        }

        public void OnEndCreating()
        {
            CreateJob = 0;
            ForceSetState(E_ULayerSubState.Idle, null);
        }

        public bool IsCreating()
        {
            return CreateJob > 0;
        }


        public void BreakCreate()
        {
            GlobalEvent.OnStopCreate.Invoke(2);
        }


        public void OnJoyStickMove(UnityEngine.Vector3 targetPos)
        {
            skillDispatcher.SkillController.OnClientJoyStickChange(targetPos);
        }

        /// <summary>
        /// [客户端对逻辑位置的变更]
        /// unity物理，表现层同步到逻辑层，在状态同步到服务器
        /// </summary>Y是之后的
        /// <returns></returns>
        public void SetCollY_FilterSSPData( /*float Y*/ Vector3 v3, bool isSkillMove = false)
        {
            SetCurrentPos(v3.x, v3.y, v3.z, true); //显示层推送逻辑层
            //SGF.Debuger.LogError($"[状态切换] SetCollY_FilterSSPData() 摇杆状态={Is___JoySitckStop},禁移动={Is___ForbidMove},禁朝向={Is___ForbidDir},向量={M_EntityMoveDir}");

            //-------------------------------------------------------- 
            // 如果是【技能位移】就【不同步给服务器】
            // 因为技能位移服务器不需要知道每帧的坐标点
            // 发送距离的间隔限制
            if ((v3 - m_toServerPos).magnitude > GameConfig.C2S_MINI_MOVE_SYNC_DISTANCE && !isSkillMove)
            {
                m_toServerPos = v3;
                /// 2023/9/26
                /// 如果 是 黑洞效果状态下, 客户端的摇杆指令不会对玩家产生 实际移动, 而是将 摇杆指令 发送给服务器.
                /// 等到 服务器返回属性 坐标同步后,将属性坐标同步的 坐标偏移量 作为摇杆指令 输入给玩家摇杆.
                /// 此时 ， 主角 在每帧 执行 偏移量运动后， 就不需要再次 推送给服务器.
                if (Data != null && Data.isMainPlayer && !Data.IsBattleState_BlackHole)
                {
                    m_PostTag |= true;
                }
            }

            // if (Data != null && Data.IsBattleState_BlackHole)
            // {
            //     SGF.Debuger.LogWarning($"{TagFlag} [黑洞] move之后: Pos: {CurrentPos} ");
            // }


            //-------------------------------------------------------- 
        }

        /// <summary>
        /// 发送移动同步消息
        /// </summary>
        private void SendSyncMoveMsg()
        {
            // 是否处于黑洞牵引状态
            bool isBattleState_BlackHole = Data.IsBattleState_BlackHole;

            // 判断属性是否有变化, 如果没有变化但是 属于 黑洞牵引状态, 那么此时 还是需要给服务器发送 摇杆指令
            if (m_oldPos == CurrentPos && m_oldServerRot == ClientRotToServerRot && !Data.IsBattleState_BlackHole)
            {
                SGF.Debuger.Log($"{TagFlag} SendSyncMoveMsg no change ----- no SendSyncMoveMsg");
                return;
            }

            // 判断原子状态是否被禁用了, 如果 被原子锁 禁止朝向和移动, 那 黑洞牵引 时候 的 摇杆指令 也就无效, 不需要给服务器发送
            bool isForbidMove = Data.Is___ForbidMove;
            bool isForbidDir = Data.Is___ForbidDir;
            if (isForbidMove && isForbidDir)
            {
                SGF.Debuger.Log(
                    $"{TagFlag} SendSyncMoveMsg isForbidPos={isForbidMove},isForbidDir={isForbidDir} ----- no SendSyncMoveMsg");
                return;
            }

            // 判断是否禁止了坐标
            Vector3 sendPos = CurrentPos;
            if (!isForbidMove)
            {
                m_oldPos = CurrentPos;
            }
            else
            {
                sendPos = m_oldPos;
            }

            // 判断是否禁止了朝向
            int sendServerRot = ClientRotToServerRot;
            if (!isForbidDir)
            {
                m_oldServerRot = sendServerRot;
            }
            else
            {
                sendServerRot = m_oldServerRot;
            }

            if (isBattleState_BlackHole)
            {
                sendServerRot = M_EntityJoyMoveSendServerAngle;
            }

            vector3.X = sendPos.x;
            vector3.Y = sendPos.y;
            vector3.Z = sendPos.z;
            moveMsg2.IsStart = false; // 服务器要求一直传false就行
            moveMsg2.Rot = sendServerRot;
            moveMsg2.Pos = vector3;
            moveMsg2.TimeStamp = dateTimeOffset.ToUnixTimeSeconds();
            // if (!isBattleState_BlackHole)
            // {
            //     SGF.Debuger.LogWarning($"{TagFlag} [黑洞] 发送属性同步: Pos: {CurrentPos} ---> {sendPos},Rotate: {ClientRotToServerRot} ---> {sendServerRot}");

            // }
            M_BattleSocket.SendRPCMsg(ServerType.ServerTypeScene, moveMsg2, false);

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                // 移动消息, 通过 本地服 模拟发送 移动逻辑
                GlobalEvent.OnClientReqLocalServerEvent.Invoke(ClientEventReq.MoveMsg2, new object[] { moveMsg2 });
            }
#endif

            /// 2023/9/25
            /// 跟夏哥沟通如下:
            /// 1.MoveMsg2.rot 原本的设计 是 同步玩家的朝向, 目前在原子锁的状态下, 将这个玩家朝向 设置为 摇杆朝向, 发送给服务器;
            /// 2.在原本的设计上,朝向 跟 移动 是分离的. 如玩家朝向向左,但是 移动向右。
            /// TODO: 夏
            /// 3.夏哥的意思,在黑洞效果的情况下, 兼容以前的协议,临时将 这个rot 设置为 摇杆朝向, 等到后续 需要再拓展字段
            /// 4.在
            // if (isBattleState_BlackHole)
            // {
            //     SGF.Debuger.LogWarning(
            //         $"{TagFlag} [黑洞] 发送属性同步: Pos: {CurrentPos} ---> {sendPos},Rotate: {ClientRotToServerRot} ---> {sendServerRot}");
            // }
        }

        #endregion

        #region 这边只是改变客户端维护的技能结束后的过渡状态（战斗状态）

        private void OnActionOnStartSkillStage(SkillEntity skillEntity)
        {
            //if (Data.isMainPlayer)
            {
                M_clientTag = E_ClentMainPlayerState.Battle;
                M_clientStateCountdown = GameConfig.System_BattleStateCountDown;
            }
        }

        private void OnActionOnEndSkillStage(SkillEntity skillEntity, E_SkillExitType e_SkillExitType)
        {
            //if (Data.isMainPlayer)
            {
                M_clientTag = E_ClentMainPlayerState.Battle;
                M_clientStateCountdown = GameConfig.System_BattleStateCountDown;
            }
        }

        #endregion

        #region 小地图通知

        public override void OnFinalPosChange(UnityEngine.Vector3 newVector3)
        {
            //底层之间直接发送，不用action
            if (Swm != null && Data != null && Data.isMainPlayer)
            {
                Swm.OnMainPlayerMove(newVector3);
            }
            else if (Swm != null && (Faction == 5 || Faction == 6) && Data != null && !Data.isMainPlayer)
            {
                Swm.OnPvpPlayerMove(Data.M_EntityID, newVector3);
            }
            else if (Swm != null && !Data.isMainPlayer && GameManager.Instance.GetCurMapType() == SpaceType.SpaceArena)
            {
                Swm.OnSpaceArenaMove(Data.M_EntityID, newVector3);
            }

            base.OnFinalPosChange(newVector3);
        }

        #endregion

        #region 属性同步

        private ulong SceneFlag = 0;
        private int m_CurLevel = 0;
        private long m_CurHp = 0;
        private uint m_JobID = 0;

        protected override void InitRegisterAttribute()
        {
            base.InitRegisterAttribute();

            M_Name = m_playerData.Attrs.GetAoiValue<string>(EnumAOIType.String, AOIAttrDefine.Name);

            Data.RegisterAttribute(AOIAttrDefine.InteractID, OnAOIInteractIDChange);

            Data.RegisterAttribute(AOIAttrDefine.Job, OnAOIJobChange);

            Data.RegisterAttribute(AOIAttrDefine.RobotState, OnAOIRobotStateChange);

            if (Data.isMainPlayer)
            {
                m_CurLevel = m_playerData.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerLevel);
                GameManager.Instance.SetReportNewPlayerLog(true);
                Data.RegisterAttribute(AOIAttrDefine.PlayerLevel, OnAOILevelChange);

                SceneFlag = AttrData.GetAoiValue<ulong>(EnumAOIType.Ulong, AOIAttrDefine.SceneFlag);
                Data.RegisterAttribute(AOIAttrDefine.SceneFlag, OnAOISceneFlagChange);

                m_CurHp = m_playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp);
                Data.RegisterAttribute(AOIAttrDefine.curHp, OnAOICurHpChange);

                Data.RegisterAttribute(AOIAttrDefine.WantTaskInfo, OnAOIWantTaskInfoChanage);
            }
        }

        private void OnAOISceneFlagChange(string key, object val)
        {
            ulong newSceneFlag = AttrData.GetAoiValue<ulong>(EnumAOIType.Ulong, AOIAttrDefine.SceneFlag);

            if (newSceneFlag != SceneFlag)
            {
                GameManager.Instance.ChangeSceneFlag((uint)newSceneFlag);
                SceneFlag = newSceneFlag;
            }
        }

        private GameKeyCommand curInterState = GameKeyCommand.None;

        /// <summary> AOI [交互ID] 变化 </summary>
        private GameKeyCommand GetNextInterState()
        {
            if (curInterState == GameKeyCommand.None)
            {
                return GameKeyCommand.InterAction1;
            }
            else if (curInterState == GameKeyCommand.InterAction1)
            {
                return GameKeyCommand.InterAction2;
            }
            else
            {
                return GameKeyCommand.InterAction1;
            }
        }

        public void PlayInteractAnimation(InteractDataCell interactDataCell)
        {
            GameKeyCommand command = GetNextInterState();

            if (interactDataCell != null)
            {
                //交互进行中
                I_AnimParam animParam = GetAnimParamByState(E_ULayerSubState.InterAction1, interactDataCell.Action);
                ChangeState(command, animParam, false);
            }
        }

        public void PlayInteractProcessBar(ulong entityid, InteractDataCell interactDataCell)
        {
            long waitTime = 86400000;
            bool showBar = true;

            if (interactDataCell != null)
            {
                waitTime = interactDataCell.GetInterTime();
                showBar = interactDataCell.GetIsShowbar();
            }


            if (showBar)
            {
                if (GlobalEvent.InterEvent != null)
                {
                    GlobalEvent.InterEvent.Invoke(entityid, TimeUtils.ServerNowStampMilli + waitTime);
                }
            }
        }

        private void OnAOIInteractIDChange(string key, object val)
        {
            ulong uid = m_playerData.Attrs.GetAoiValue<ulong>(EnumAOIType.Ulong, key);
            //ulong uid = Convert.ToUInt64(value);
            if (uid > 0)
            {
                GameKeyCommand command = GetNextInterState();
                var entity = (ObjectCtrlGroup)GameManager.Instance.GetEntityCtr(uid);
                if (entity != null)
                {
                    InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(entity.ConfigID);
                    if (interactDataCell != null)
                    {
                        PlayInteractAnimation(interactDataCell);
                        IsClientPrepareInter = false;
                    }
                }
            }
            else
            {
                PlayerCtrlGroup player = GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup;
                if (player != null)
                {
                    player.SetInterLock(false);
                }

                //交互结束
                ForceSetState(E_ULayerSubState.Idle, null);
            }
        }

        /// <summary> AOI [坐标] 变化 </summary>
        /// 其他人直接设置位置坐标即可
        protected override void OnAOIPositionChange(string key, object val)
        {
            base.OnAOIPositionChange(key, val);

            //object value = m_playerData.Attrs.GetProtoValue(key);
            //// 主角自己不需要同步坐标和角度，是客户端自己控制
            //// 主角自己控制/RPC强拉/移动不需要路店（除了任务和寻路）
            //if (m_playerData.isMainPlayer == false)
            //{

            //    if (IsFirstPos)
            //    {
            //        SetPosition(value, false);  //如果是true是强制啦
            //        IsFirstPos = false;
            //    }
            //    else
            //    {
            //        SetPosition(value, true);//过程移动
            //    }
            //}
            //else
            //{
            //    /// 2023/4/14
            //    /// 取巧的做法, 目前 属性的 服务器数据 和 本地数据没有做拆分.
            //    /// 同时, 发现 属性 的原子锁 目前并不会 锁住 OnAOIPositionChange 的消息通知, 而是 会将属性值 锁住.
            //    /// 所以 上面 GetProtoValue(key) 获取的 是 锁住的 旧值, 但是  val 是 服务器发过来的新值
            //    /// 
            //    /// TODO: 曲
            //    /// @ 曲, 后面如果可以的话可以把属性 区分出 纯服务器数据 和 客户端 控制的数据.
            //    var vv3 = SetServerPosition(val);
            //    // SGF.Debuger.LogError($"属性同步 [c-c] positionChange {vv3} ");

            //}
        }

        /// <summary> AOI [等级] 变化 </summary>
        private void OnAOILevelChange(string key, object val)
        {
            int curLevel = m_playerData.Attrs.GetAoiValue<int>(EnumAOIType.Int, key);
            bool isUpgrade = curLevel > m_CurLevel;
            if (isUpgrade)
            {
                // ----主角升级
                // 播放特效
                {
                    FxParam fxParam = new();
                    fxParam.InitWithLogic("Fx_role_up_common");
                    ActionOnPlaySpecialEffects?.Invoke(fxParam, "LevelUp", "Effects/Roles/Character/cm");
                }
                // 通知NPC解锁
                GameManager.Instance.SetAllNpcIsVisiable();

                //刷新环任务红点
                GameManager.Instance.CheckRingTaskGetInfo();

                if (UserManager.Instance.MainUserData != null)
                {
                    //SDKManager.Instance.UpLoadRoleData(curLevel.ToString(), GSSDK.GSRole.RoleType.TYPE_LEVEL_UP);

                    SDKManager.Instance.ReportSDKLevelUp(
                        ServerId: BusinessManager.Instance.GetAreaID().ToString(),
                        ServerName: BusinessManager.Instance.GetAreaName(),
                        RoleId: BusinessManager.Instance.GetUserPID().ToString(),
                        RoleName: M_Name,
                        RoleLevel: curLevel.ToString(),
                       CreateTime: GameLoginInfo.GetCreateTime().ToString()
                    );
                }
            }

            m_CurLevel = curLevel;
            GameManager.Instance.SetReportNewPlayerLog(true);
        }

        private void OnAOICurHpChange(string key, object val)
        {
            // 判断如果不是战斗状态
            // 血量有变化就显示飘字
            //long curHp = m_playerData.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
            //long cha = curHp - m_CurHp;
            //if (cha > 0 && !IsBattleStateServer)
            //{
            //    SGF.Debuger.Log($"系统加血 值={cha}");
            //    //GameManager.Instance.OnCureFloatingText(cha, EntityId);
            //}
            //m_CurHp = curHp;
        }

        private void OnAOIWantTaskInfoChanage(string key, object val)
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.WantedModule, "InformWantedWindow", new object[] { 2 });
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "InformWantedWindow", new object[] { 2 });
        }

        /// <summary>
        /// 第三方人才有角度变化属性同步
        /// </summary>
        /// <param name="key"></param>
        protected override void OnAOIRotChange(string key, object val)
        {
            base.OnAOIRotChange(key, val);

            //只有第三方人才有角度变化属性同步
            if (m_playerData.isMainPlayer)
            {
                //主角只有通过，【通过技能/强制更正协议（非属性同步）】拉取转角
                //所以这里不会控制主角角度的
                //这里是小地图的
                Swm.OnMainPlayerRot(ServerAngles);
            }
        }

        private void OnAOIRobotStateChange(string key, object value)
        {
            IsRobot = m_playerData.Attrs.GetAoiValue<bool>(EnumAOIType.Bool, key);
        }

        private void OnAOIJobChange(string key, object value)
        {
            uint jobId = m_playerData.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, key);
            if (jobId != 0 && m_JobID != jobId)
            {
                uint tempJobID = m_JobID;
                JobChange(jobId);
                if (Data.isMainPlayer)
                {

                    JobDataCell jobDataCell = LocalDataManager.Instance.GetJobDataCell((int)jobId);
                    if (jobDataCell.GetTransLevel() != 0 && jobDataCell.GetBaseJob() == tempJobID)
                    {
                        // 播放转职特效
                        {
                            FxParam fxParam = new();
                            fxParam.InitWithLogic("FX_UI_Com_ZhanZhi");
                            ActionOnPlaySpecialEffects?.Invoke(fxParam, "LevelUp", "Effects/Roles/Character/cm");
                        }
                    }
                    GlobalEvent.OnPlayerDataChange?.Invoke(AOIAttrDefine.Job, jobId);
                }
            }
        }

        // 暴露出来是为了GM命令使用
        public void JobChange(uint JobID)
        {
            // 职业发生了变化，【转职了】
            m_JobID = JobID;
            JobDataCell jobDataCell = LocalDataManager.Instance.GetJobDataCell((int)m_JobID);
            if (jobDataCell != null)
            {
                int model = jobDataCell.GetAvatarID();
#if UNITY_EDITOR
                if (SkillEditorGM.g_modelID != -1)
                {
                    model = SkillEditorGM.g_modelID;
                    SkillEditorGM.g_modelID = -1;
                }
#endif
                CreateDefaultAvatar(model);
            }
        }

        #endregion

        #region 技能、BUFF、被动效果执行

        protected override void OnActionOnSkillPlayAnim(E_ULayerSubState stage, I_AnimParam i_AnimParam)
        {
            //if (i_AnimParam != null)
            //{
            //    SGF.Debuger.LogError($"{TagFlag} 属性同步 OnActionOnSkillPlayAnim PlayerEnityId=>{EntityId},stage={stage},i_AnimParam={i_AnimParam.AnimationPath}");
            //}
            //SGF.Debuger.LogError($"{TagFlag} 属性同步 OnActionOnSkillPlayAnim PlayerEnityId=>{EntityId},stage={stage}");
            ForceSetState(stage, i_AnimParam);
        }

        protected override void OnActionOnSkillAnimation(E_ULayerSubState stage)
        {
            //SGF.Debuger.LogError($"{TagFlag} 属性同步 OnActionOnSkillAnimation PlayerEnityId=>{EntityId},stage={stage}");
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
                SGF.Debuger.LogWarning($"{TagFlag} OnActionOnSkillPlayFx PlayerEnityId=>{EntityId},avatarData=null");
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

        //人身上的触发体：非生命体被人拥有的实体：类似服务器的子弹，【不同于时间逻辑触发器，场景关卡触发物，物理触发器】
        internal void AddFxTriggerForPersion(EnumEnityListKey entityFx, Transform curCtrlPlayerFxRoot)
        {
            EntityRemoteStatic fx = EntityFactory.InstanceEntity<EntityRemoteStatic>();
            fx.GetyOutLifeType = EntityOutLifeType.FollowRole;
            fxList = GetEntityList(entityFx);
            fxList.Add(fx);
            //现在没有非实体数据
            fx.Create(E_WithOuLifeResType.FxUnit, 0, curCtrlPlayerFxRoot);
        }

        #endregion

        #region 职业技能相关逻辑,因为怪物没有职业技能,所以先放在HeroEntityBase中

        public void OnAllSkillNotice(ProtoMsg.AllSkillNotice allSkillNotice)
        {
            m_playerData.skillListMap.Clear();
            int count = allSkillNotice.Skilllist.Count;
            for (int i = 0; i < count; i++)
            {
                ProtoMsg.SingleSkillInfo singleSkillInfo = allSkillNotice.Skilllist[i];
                // skillList.Add(singleSkillInfo);
                m_playerData.skillListMap.Add(singleSkillInfo.SkillDBID, singleSkillInfo);

                //SGF.Debuger.LogError($"技能信息 skilllist遍历 SkillDBID={singleSkillInfo.SkillDBID},SkillLevel={singleSkillInfo.SkillLevel},TalentID={singleSkillInfo.TalentID}");
            }

            m_playerData.skillPosList.Clear();
            count = allSkillNotice.Skillposlist.Count;
            for (int i = 0; i < count; i++)
            {
                ProtoMsg.SkillPos skillPos = allSkillNotice.Skillposlist[i];
                m_playerData.skillPosList.Add(skillPos);
                // SGF.Debuger.LogError($"技能信息 Skillposlist 遍历 PosID={skillPos.PosID},JobSkillID={skillPos.JobSkillID},Islocked={skillPos.Islocked}");
            }

            m_playerData.ActionOnAllSkillNotice?.Invoke(allSkillNotice);
        }

        public void OnAllSkillCDNotice(Google.Protobuf.Collections.RepeatedField<ProtoMsg.CDData> cdList)
        {
            m_playerData.ActionOnAllCDNotice?.Invoke(cdList);
        }

        public void OnRoleAllCDListNtf(Google.Protobuf.Collections.RepeatedField<ProtoMsg.CDData> cdList)
        {
            m_playerData.ActionOnRoleAllCDListNtf?.Invoke(cdList);
        }

        /// <summary>
        /// 技能位切换技能返回(技能槽切换职业技能)
        /// </summary>
        /// <param name="switchSkillPosRet"></param>
        public void OnSwitchSkillPosRet(ProtoMsg.SwitchSkillPosRet switchSkillPosRet)
        {
            //TODO: dl
        }

        /// <summary>
        ///  切换天赋返回
        /// </summary>
        /// <param name="switchTalentRet"></param>
        public void OnSwitchTalentRet(ProtoMsg.SwitchTalentRet switchTalentRet)
        {
            //TODO: dl
        }

        public void OnCDUpdateNotice(ProtoMsg.CDUpdateNotice cDUpdateNotice)
        {
            m_playerData.ActionOnCDUpdateNotice?.Invoke(cDUpdateNotice.CDInfo);
        }

        #endregion

        #region 主角自己寻路

        /// <summary>
        /// 主角重写
        /// 寻路是客户端自己寻的
        /// </summary>
        /// <returns></returns>
        public override bool CheckClientMainPlayerFindingPath()
        {
            if (Is_MainPlayer_FindingPath && FindTarget != NoneVector3 &&
                Vector3.Distance(CurrentPos, FindTarget) <= MaxDistance)
            {
                MainPlayer_Clear_FollowUp_Target();
                return true;
            }

            return false;
        }

        private Vector3 NoneVector3 = new(-999, -999, -999);
        private Vector3 FindTarget = new(-999, -999, -999);
        private float MaxDistance = 0.1f;

        private bool CanFindPath()
        {
            if (M_eSubState == E_ULayerSubState.InterAction1)
            {
                return false;
            }

            if (M_eSubState == E_ULayerSubState.InterAction2)
            {
                return false;
            }

            return true;
        }

        protected override void OnBattleStateChange(E_BattleStateType curState, E_BattleStateType newState)
        {
            base.OnBattleStateChange(curState, newState);

            // 状态没发烧变化
            if (curState == newState)
            {
                return;
            }

            E_BattleStateType theCurStateIn____Move = curState | E_BattleStateType.BattleState_ForbidMove;
            //其他位必然是0，关键位用对方的
            E_BattleStateType theNewerStateIn____Move = newState | E_BattleStateType.BattleState_ForbidMove;

            // 如果移动的原子锁发生变化 且 由 禁止移动 ----> 可以移动
            if (theCurStateIn____Move != theNewerStateIn____Move && !Data.Is___ForbidMove)
            {
                ResumeRecordClientNaveFindPath();
            }


            E_BattleStateType theCurStateIn____Dir = curState | E_BattleStateType.BattleState_ForbidDir;
            //其他位必然是0，关键位用对方的
            E_BattleStateType theNewerStateIn____Dir = newState | E_BattleStateType.BattleState_ForbidDir;

            // 如果移动的原子锁发生变化 且 由 禁止转向 ----> 可以转向
            if (theCurStateIn____Dir != theNewerStateIn____Dir && !Data.Is___ForbidDir)
            {
                ResumeRecordClientNaveFindPath();
            }
        }

        // public List<GameObject> mPaths = new List<GameObject>();

        internal class NaveFindPathRecord
        {
            public Vector3 Position;
            public float Maxdistance;
            public Action<bool> Action;
            public bool IsChanageFindPath;
            public E_FindPathType FindPathType;

            public void Init(Vector3 position, float maxdistance, Action<bool> action, bool isChanageFindPath, E_FindPathType findPathType)
            {
                Position = position;
                Maxdistance = maxdistance;
                Action = action;
                IsChanageFindPath = isChanageFindPath;
                FindPathType = findPathType;
            }
        }

        private NaveFindPathRecord naveFindPathRecord;

        private bool CheckNeedRecordNaveFindPath(E_FindPathType findPathType)
        {
            if (!Data.isMainPlayer)
            {
                return false;
            }

            // 自动战斗中的 寻路，不需要考虑记录寻路
            switch (findPathType)
            {
                case E_FindPathType.AutoBattle:
                case E_FindPathType.AutoBattle_MonsterPoint:
                case E_FindPathType.AutoBattle_Move2Pos:
                case E_FindPathType.AutoBattle_Spawner:
                    {
                        return false;
                    }

                default: return true;
            }

            return true;
        }

        public void RecordClientNaveFindPath(Vector3 position, float Maxdistance, System.Action<bool> action,
            bool isChanageFindPath, E_FindPathType findPathType)
        {
            if (!Data.isMainPlayer)
            {
                return;
            }
            // 不是主角,  不需要关心寻路的状态恢复
            if (naveFindPathRecord == null)
            {
                naveFindPathRecord = new NaveFindPathRecord();
            }

            naveFindPathRecord.Init(position, Maxdistance, action, isChanageFindPath, findPathType);
        }

        /// <summary>
        /// 恢复一次 寻路操作
        /// </summary>
        private void ResumeRecordClientNaveFindPath()
        {
            // 不是主角,  不需要关心寻路的状态恢复
            if (!Data.isMainPlayer)
            {
                return;
            }

            if (naveFindPathRecord == null)
            {
                return;
            }
            BattleManager.Instance.ResetCoolDown();

            ClientNavFindPath(naveFindPathRecord.Position, naveFindPathRecord.Maxdistance, naveFindPathRecord.Action,
                naveFindPathRecord.IsChanageFindPath, false, naveFindPathRecord.FindPathType);
            naveFindPathRecord = null;
        }

        private void ClearRecordClientNaveFindPath()
        {
            naveFindPathRecord = null;
        }

        /// <summary>
        /// 开启自动寻路的接口
        /// </summary>
        /// <param name="position"></param>
        /// <param name="Maxdistance"></param>
        /// <param name="action"></param>
        /// <param name="isChanageFindPath"></param>
        /// <param name="recordOnStateForbid">是否在原子锁锁住无法寻路的时候 记录此处操作</param>
        public bool ClientNavFindPath(UnityEngine.Vector3 position, float Maxdistance, System.Action<bool> action,
            bool isChanageFindPath = false, bool recordOnStateForbid = false, E_FindPathType findPathType = E_FindPathType.None)
        {
            //if (mPaths.Count > 0)
            //{
            //    while (mPaths.Count > 0)
            //    {
            //        GameObject.DestroyImmediate(mPaths[0]);
            //        mPaths.RemoveAt(0);
            //    }
            //}
            if (!M_IsAlive)
            {
                action?.Invoke(false);
                StarDebug.LogError("非存活状态", M_eSubState.ToString());
                return false;
            }

            if (InputManager.Instance.IskeyDown)
            {
                action?.Invoke(false);
                StarDebug.LogError("有按键输入不能寻路");
                return false;
            }

            if (InputManager.Instance.IsJoystickMove)
            {
                action?.Invoke(false);
                StarDebug.LogError("有摇杆输入不能寻路");
                return false;
            }

            if (!CanFindPath())
            {
                action?.Invoke(false);
                StarDebug.LogError("当前状态不能寻路", M_eSubState.ToString());
                return false;
            }

            if (Data.Is___ForbidMove || Data.Is___ForbidDir)
            {
                // 如果被原子锁锁住导致此处操作没法进行, 那就记住此处操作， 等到原子锁解开后, 再次出发
                if (recordOnStateForbid && CheckNeedRecordNaveFindPath(findPathType))
                {

                    if (Data.isMainPlayer)
                    {
                        BattleManager.Instance.CoolDown();
                    }
                    RecordClientNaveFindPath(position, Maxdistance, action, isChanageFindPath, findPathType);
                }
                else
                {
                    ClearRecordClientNaveFindPath();
                }

                action?.Invoke(false);
                return false;
            }

            MaxDistance = Maxdistance;
            if (isChanageFindPath)
            {
                float y = BusinessManager.Instance.GetGroundHeight(position.x, position.z);
                position.y = y > 0 ? y : CurrentPos.y;
            }

            if (FindPathManager.Instance.FindPath(CurrentPos, position, Maxdistance,
                    out UnityEngine.Vector3[] potions) && potions != null && potions.Length > 0)
            {
                //打断上次寻路
                if (Is_MainPlayer_FindingPath)
                {
                    //比较上一个一个寻路目标点和本次目标点是否同一个位置，距离小于0.5单位就替换callback
                    var lastPoint = potions[potions.Length - 1];
                    if (Vector3.Distance(lastPoint, FindTarget) <= 0.5f)
                    {
                        //OnFindPathCallBack?.Invoke(false);
                        //OnFindPathCallBack = action;
                        return true;
                    }
                    else
                    {
                        BreakFindPath();
                    }
                }

                FindTarget = potions[potions.Length - 1];

                if (Vector3.Distance(CurrentPos, FindTarget) <= MaxDistance)
                {
                    action?.Invoke(true);
                    OnFindPathCallBack = null;
                    BreakFindPath();
                    //MainPlayer_Clear_FollowUp_Target();
                    //Is_MainPlayer_FindingPath = false;
                    return true;
                }

                SetWayPointData(potions, action);

                ///
                return true;
            }
            else
            {
                action?.Invoke(false);
            }

            return false;
        }

        #endregion

        #region GM 按钮，添加主角模型 不移动

        public static bool g_testModel = false;

        List<ViewObject> testModels = new();

        //gm:创建模型view
        public void AddMainPlayerView(Transform container, Vector3 pos)
        {
            //modelDataCell = LocalDataManager.Instance.GetModelDataCell(2001);
            if (modelDataCell != null)
            {
                g_testModel = true;
                //string modlePath = modelDataCell.ModelsPath;
                //var obj = ViewFactory.CreateViewAddressables(modlePath, "", this, container, "", false);
                var obj = ViewFactory.CreateViewAddressables("Roles/Template/Character_Model2",
                    "Roles/Template/Character_Model2", this, container);
                //var obj = ViewFactory.CreateViewAddressables("", "Roles/Template/Character_Model", this, container, "", false);
                //obj.transform.position = pos;
                obj.GetComponent<ViewVitalAnim>().OnForceMove(pos);
                obj.GetComponent<ViewVitalAnim>().SetIsSyncPos(false);

                testModels.Add(obj);
                g_testModel = false;
            }
        }

        public void ClearNpc()
        {
            foreach (var item in testModels)
            {
                item.gameObject.SetActive(false);
            }

            testModels.Clear();
        }

        #endregion
    }
}