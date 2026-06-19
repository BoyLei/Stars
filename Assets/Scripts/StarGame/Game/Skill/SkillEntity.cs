using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Time;
using SGF.Unity;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProjectDef;
using System;
using System.Collections.Generic;

using BattleDebug;
using StarProject.CustomDataStruct;
using System.Text;
using StarProject.Game.Skill.Utils;

namespace StarProject.Game.Skill
{


    /// <summary>
    /// 每一个技能创建时,都会创建一个SkillEntity。
    /// SkillEntity包含技能的配置信息,以及根据技能配置和玩家数据,综合计算出的 如 技能的CD等等数据
    /// </summary>
    public partial class SkillEntity : EntityRemoteStatic
    {
        private static StringBuilder sb = new StringBuilder(32);
        private string TagFlag
        {
            get => sb.Clear().Append("[").Append(EntityID).Append("] [SkillEntity_").Append(skillId).Append("], runtimeID : ").Append(RuntimeID).ToString();
        }

        private VitalSignData playerData;

        public ulong EntityID => playerData == null ? 0 : playerData.M_EntityID;

        public int Faction => playerData == null ? 0 : playerData.myOwnerNtt.Faction;

        public UnityEngine.Vector3 Pos => playerData == null ? UnityEngine.Vector3.zero : playerData.myOwnerNtt.Position();


        public int skillId = 0;

        public SkillContainer skillContainer;

        private E_EntityState skillEntityState = E_EntityState.None;

        private E_EntityState readyReleaseState = E_EntityState.ClientClose | E_EntityState.ServerClose;

        /// <summary>
        /// 技能是否正在运行:
        /// 技能的结束，主要区分为 客户端结束 和服务器结束两种。
        /// 目前只有 客户端关闭,才 会将 IsRunning 设置为 false;
        /// 
        /// 只有 ClientClose && serverClose, 技能实体才会销毁
        /// </summary>
        public bool IsClientRunning => (skillEntityState != E_EntityState.None) && (skillEntityState & E_EntityState.ClientClose) != E_EntityState.ClientClose;

        private bool readyRelease = false;
        public bool ReadyRelease => readyRelease;

        private ulong _builderID = 0;
        public ulong BuilderID => _builderID;

        private ulong _ownerEntityID = 0;
        public ulong OwnerEntityID => _ownerEntityID;

        /// <summary>
        /// 效果执行的key的历史记录, 对于一个 next的效果来说, 可能存在 Next_True/Next_False 两种分支的效果key, 
        /// 而他们都是 采用 delayInvoke group= key 组的方式调用。 对于取消next效果来说, 直接取消 key对应的所有效果即可
        /// </summary>
        /// <typeparam name="string"></typeparam>
        private HashSet<string> histroyKeySet = new HashSet<string>();

        /// <summary>
        /// 当前的动画帧,用来做动作状态的恢复
        /// (技能被阶段中打断,如果动作跨阶段,会丢失结束帧,这个记录可以用来恢复动画结束帧)
        /// </summary>
        private AnimationJson curAnimationRecord;

        private bool IsCurAnimationRecordFramStart = false;

        private SkillStage curAnimationRecordStage = null;

        private int uniqueID = 0;

        private SkillController skillController = null;

        /// <summary>
        /// 等待下一帧 才执行 update 的标记.
        /// 技能 执行预播的时候,目前是在 当前帧 就添加进入 技能实体队列的.
        /// 那么 此时就需要标记 这个技能 waitNextFrameUpdate = true;
        /// 这样,这个技能实体 在当前这一次 的update中 不执行, 而是在下一个update中执行.
        /// </summary>
        private bool waitNextFrameUpdate = false;


        /// <summary>
        /// 跟随玩家 阶段循环的 特效/动画 key  的记录， 用来 标识 这个特效/动画 已经播放过. 一般是  阶段进入时就 播放的自己循环的动画
        /// </summary>
        private HashSet<string> _followLoopKeyRecord = new HashSet<string>();

        public bool IsNormalSkill => curSkillInfo.IsNormalSkill();


        private int GetUniqueID()
        {
            return uniqueID++;
        }

        public void Create(SkillInfo _skillInfo, SkillController _skillController, ulong runtimeID)
        {
            playerData = _skillController.PlayerData;
            skillController = _skillController;

            curSkillInfo = _skillInfo;
            skillId = _skillInfo.skillId;
            skillContainer = _skillInfo.skillContainer;

            cfg = _skillInfo.cfg;

            isAutoTurnToTarget = _skillInfo.IsAutoTurnToTarget;

            skillBlackBoard = new SkillBlackBoard();

            // 技能创建的时候, 在黑板中存入这个技能的 id
            skillBlackBoard.Set(BaseBlackBoard.KEY_CFG_ID, skillId, E_BlackBoardTag.Client);

            _builderID = playerData.M_EntityID;
            _ownerEntityID = playerData.M_EntityID;

            RuntimeID = runtimeID;

            InitStages(_skillInfo);

        }


        public ulong RuntimeID;

        /// <summary>
        /// 开始蓄力的标志位
        /// </summary>
        private bool Flag_StartEnergy = false;


        /// <summary>
        /// 是否是客户端模拟阶段
        /// 客户端模拟阶段 只会在 技能前摇阶段,需要等到服务器反馈后,
        /// 才会进入后续阶段
        /// </summary>
        private bool isClientSimulating = false;

        public bool IsClientSimulating
        {
            get { return isClientSimulating; }
        }

        private bool isOnlyServerUseSkill = false;

        public SkillUseReq clientSkillUseReq;

        private Dictionary<string, SkillStage> recoverStageDic = new Dictionary<string, SkillStage>();


        /// <summary>
        /// 技能黑板
        /// </summary>
        private SkillBlackBoard skillBlackBoard;

        private DateTime now;

        private long clientPreUseTime = 0;

        /// <summary>
        /// 技能的朝向
        /// </summary>
        private int playerRota = 0;

        /// <summary>
        /// 客户度使用技能,优先进入技能前摇阶段
        /// 得到服务器使用协议返回后,进入后续技能阶段
        /// </summary>
        /// <param name="_skillUseReq"></param>
        public void ClientUseSkill(SkillUseReq _skillUseReq, int enterTime)
        {
            RuntimeID = _skillUseReq.RuntimeID;
            //SGF.Debuger.Log($"{TagFlag} on [client] UseSkill ");

            isClientSimulating = true;
            isOnlyServerUseSkill = false;
            clientSkillUseReq = _skillUseReq;

            HandleBlackList(_skillUseReq.BlackList, "SkillUseReq", true);

            //TODO 
            //DELETE | MOVE
            {
                curAttackSpeed = _skillUseReq.CAttackSpeed;
                curSkillProTime = GetRealSkillProTime(curAttackSpeed);

                curEnergySpeed = playerData.CurEnergySpeed;

                now = DateTime.UtcNow;
            }
            clientPreUseTime = TimeUtils.ServerNowStampMilli;

            // 更新 技能的朝向
            UpdateSkillReqRot(_skillUseReq);

            OnClientPreEnter(enterTime);

            // 技能 开始 预播 的时候, 开启 技能槽的 cd
            StartContainerCD();

            ActionOnSyncSkillUse.Invoke(SyncSkillUseType.ClientPreUse);
            //SGF.Debuger.LogError($"客户端 使用技能 预施法: {_skillUseReq}");
        }


        public void ServerUseSkill(SkillUseRet skillUseRet)
        {
            // // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} on ServerUseSkill start cost {(DateTime.UtcNow - now).TotalMilliseconds} ms , now {TimeUtils.ClientNowStampMilli}");
            //TODO 
            //DELETE | MOVE
            {
                curAttackSpeed = skillUseRet.SAttackSpeed;

                int serverSkillProTime = GetRealSkillProTime(curAttackSpeed);

                //// SGF.Debuger.Log($"{TagFlag} [ServerCreate] SAttackSpeed {curAttackSpeed} , serverSkillProTime {serverSkillProTime} , clientSkillProTime {curSkillProTime}");

                curSkillProTime = serverSkillProTime;

                curEnergySpeed = skillUseRet.SEnergySpeed;
            }

            RuntimeID = skillUseRet.RuntimeID;

            /// SkillUseRet 存在两种情况
            /// 1.怪物 使用技能/ 其他人使用技能 :
            ///     收到之后，直接播放技能
            /// 2.主角自己使用技能:
            ///     区分是否存在客户端模拟逻辑,
            ///         存在，继续按客户端模拟逻辑继续进行(如果网速延迟太高,直接不处理后续)
            ///         不存在，按怪物相似逻辑处理

            //1.计算出技能运行了多久
            if (isClientSimulating)
            {
                /// 如果是客户端运行时收到了服务器的回复,
                /// 由于目前改为客户端不等服务器,所以客户端会提前进入后续阶段(不会再停留在抬手阶段)
                isClientSimulating = false;
            }

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  [ServerCreate]  server.CreateTime {skillUseRet.CreateTime} , localServerTime {TimeUtils.ServerNowStampMilli}");

            /// 阶段恢复逻辑
            /// 阶段的恢复可以分为:
            ///     1.自己使用技能的状态恢复,此时 基本都是采用自己的阶段，还是按自己的技能流程往下走
            ///     2.对于 自己(断线重连或者 服务器使用技能) 和其他人的 技能恢复, 需要找到对应的阶段 和对应阶段的时间.
            ///       如果 直接根据服务器返回的 createTime,由于客户端不知道服务器
            ///       发过来的技能阶段中间是否有阶段 被 加速||跳过，所以根据createTime
            ///       计算出来的时间差 无法直接定位。
            ///       所以我让服务器额外返回 CurStageID 、CurStageLoop 、和  CurStageTime。
            ///       客户端根据 上面3个字段综合起来可以恢复到 相对应的阶段。
            /// 
            /// note: 2022/09/30
            ///     1.夏哥 将 CurStageTime 的定义,由 对应阶段的阶段时间 改为 阶段循环loop为0 的创建时间.
            ///     2.对应的 循环阶段loo为3,  CurStageTime为 0时,具体阶段的时间计算为 :
            ///         阶段当前时间 = 客户端本地服务器时间 - 服务器阶段创建时间CurStageTime - 循环次数loop X 单个循环阶段的时间.
            ///     3.考虑到 延时导致的误差, 阶段当前时间 < 0 的时候 ,设置为 0
            /// 
            //计算恢复这个阶段所需要的时间
            // double recoverTime = GetServerUseRecoverTime(skillUseRet);

            // 先提前更新一下恢复 阶段的数据
            PreHandleSeverRecoverBlackBoard(skillUseRet);
            isOnlyServerUseSkill = clientSkillUseReq == null;
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [server] ServerUseSkill CurStageTime {skillUseRet.CurStageTime} , clientNowServerTime : {TimeUtils.ServerNowStampMilli} ,  clientPreUseTime : {clientPreUseTime} , server recoverTime {recoverTime} ms , client cost time :  {(DateTime.UtcNow - now).TotalMilliseconds} ms ");
            // 如果是服务器主动释放的技能, 就走服务器 下发的 时间恢复技能;
            // 如果是客户端主动释放的技能, 不需要走 server的useSkill,直接走本地的模拟即可
            if (isOnlyServerUseSkill)
            {

                try
                {
                    OnServerEnter(skillUseRet);
                }
                catch (System.Exception e)
                {

                    SGF.Debuger.LogError($"{TagFlag} 收到服务器使用技能报错: {skillUseRet}");
                    SGF.Debuger.LogError($"错误:  {e.Message} , {e.StackTrace}");

                }
            }
            else
            {
                // 客户端已经存在,就不处理    
            }
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [server] [xx-xx] OnEnter ");
#if (UNITY_EDITOR && BATTLE_DEBUG)
            DebugSkillEventData(true, false, "ServerUseSkill", new List<string>() { $"服务器使用技能" });
#endif
            UpdateBlackBoard(skillUseRet);
            //SGF.Debuger.LogWarning($"{TagFlag} 技能 自动转向 ServerUseSkill skillUseReq.Rot={skillUseReq.Rot},,,skillUseRet.Rot={skillUseRet.Rot}");

            UpdateSkillRetRot(skillUseRet); // 同步客户端、服务器角度、强制同步到显示层

            // SkillDebugData debugData = InitSkillDebugData(RuntimeID.ToString(), onlyServerUserSkill, "ServerUseSkill", new List<string>() { $"now:[{now}]" });
            // BattleDebugHelper.Debug(debugData);
            ActionOnSyncSkillUse.Invoke(SyncSkillUseType.ServerUse);

            // SGF.Debuger.Log($"{TagFlag} ServerUseSkill cost {(DateTime.UtcNow - now).TotalMilliseconds} ms ");
        }

        /// <summary>
        /// 同步节能的朝向, 技能的朝向 
        /// </summary>
        /// <param name="rot"></param>
        /// <param name="isSyncView"></param>
        public void UpdateSkillRota(int rot, float time, bool isSyncView, bool updateClient, bool updateServer)
        {
            playerRota = rot;

            ActionOnSyncPlayerViewRota.Invoke(rot, updateClient, updateServer, isSyncView, time);
        }

        public void UpdateSkillReqRot(SkillUseReq skillUseReq)
        {
            // IsAutoTurnToTarget
            int skillRota = SkillUtils.GetSkillReqRot(skillUseReq);
            bool syncPlayerAngle = SkillUtils.GetIsAutoRotBySkillId(skillUseReq.SkillID);

            int rot = syncPlayerAngle ? skillRota : skillUseReq.Rot;

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                // 玩家坐标 先转换为 服务器坐标
                var curPos = SGF.Network.ProtoUtils.ConvertUnityVec3ToProtoVec3(Pos);
                // 玩家朝向
                // rot 先转换为服务器朝向,此处本来就是服务器角度
                int serverRot = rot;

                // 更新技能使用请求的 本地服数据
                GlobalEvent.OnClientReqLocalServerEvent.Invoke(ClientEventReq.UpdatePosAndRot, new object[] { EntityID, curPos, serverRot });
            }
#endif

            float time = curSkillInfo.SkillTurnAroundTime / 1000f;
            // time = 0.5f;
            // SGF.Debuger.LogError($"[Rotate] 客户端预播 技能 使用 skillUseReq 朝向: {rot}");
            UpdateSkillRota(rot, time, true, true, false);
        }

        public void UpdateSkillRetRot(SkillUseRet skillUseRet)
        {
            int rot = skillUseRet.Rot;
            float time = curSkillInfo.SkillTurnAroundTime / 1000f;
            // time = 0.5f;

            // SGF.Debuger.LogError($"[Rotate] 技能回复 skillUseRet 朝向: {rot}");
            UpdateSkillRota(rot, time, true, true, true);
        }

        /// <summary>
        /// 更新 用户预输入操作的 技能朝向, 目前 是 完全信赖  
        /// </summary>
        public void UpdateSkillUserInput(I_EffectParam effectParam, PreUserInputData preUserInputData)
        {
            // SGF.Debuger.LogError($"[Rotate] 服务器技能预输入 朝向 : {preUserInputData.Rot}");
            float time = curSkillInfo.SkillTurnAroundTime / 1000f;
            // time = 0.5f;
            UpdateSkillRota(preUserInputData.Rot, time, true, true, true);
        }


        /// <summary>
        /// 计算 恢复到 服务器返回的时间点 的 总的技能 运行时间
        /// </summary>
        private double GetServerUseRecoverTime(SkillUseRet skillUseRet)
        {
            int curstageID = skillUseRet.CurStageID;
            int loopCount = skillUseRet.CurStageLoop;
            string stageIDStr = SkillStage.FormatStageIDStr(curstageID, loopCount);

            if (!stageDic.ContainsKey(stageIDStr))
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} GetServerUseRecoverTime skillUseRet.stageIDStr: {stageIDStr} not find In stageDic");
                return 0;
            }

            int lastStageTime = GetRunToStageMaxTime(curstageID, loopCount);

            int itemStageTime = stageDic[stageIDStr].Time;
            // 当前阶段的时间 : 阶段当前时间 = 客户端本地服务器时间 - 服务器阶段创建时间CurStageTime - 循环次数loop X 单个循环阶段的时间.
            double curStageRealyTime = TimeUtils.ServerNowStampMilli - skillUseRet.CurStageTime - loopCount * itemStageTime;
            curStageRealyTime = curStageRealyTime < 0 ? 0 : curStageRealyTime;

            double recoverTime = lastStageTime + curStageRealyTime;
            //DB_Close       SGF.Debuger.Log($"{TagFlag} GetServerUseRecoverTime stageIDStr: {stageIDStr} lastStageTime {lastStageTime} , recoverTime : {recoverTime}");
            return recoverTime;
        }

        /// <summary>
        /// 收到技能 同步时，创建相应的黑板
        /// note:
        ///     1.如果是正常的技能回复(不需要技能恢复),只需要创建 默认的技能黑板
        ///     2.如果是需要 复原的技能恢复, 需要创建 技能黑板 和当前正在运行的阶段效果黑板
        /// </summary>
        /// <param name="skillUseRet"></param>
        private void UpdateBlackBoard(SkillUseRet skillUseRet)
        {
            //技能黑板部分
            {
                HandleBlackList(skillUseRet.BlackList, "SkillUseRet", true);
            }

            //阶段黑板部分
            {
                RepeatedField<RunStageRet> stageList = skillUseRet.StageList;
                //说明有需要恢复的阶段效果
                if (stageList.Count > 0)
                {
                    for (int i = 0; i < stageList.Count; i++)
                    {
                        RunStageRet runStageRet = stageList[i];
                        OnServerRunStage(runStageRet, true);
                    }
                }
            }
        }

        /// <summary>
        /// 提前处理技能 的
        /// </summary>
        /// <param name="skillUseRet"></param>
        private void PreHandleSeverRecoverBlackBoard(SkillUseRet skillUseRet)
        {
            WriteOwner(skillUseRet.OwnerEntityID, skillUseRet.OwnerEntityID);

            RepeatedField<BlackBoardNode> skillBlackList = skillUseRet.BlackList;

            // 技能的黑板同步数据 先存入
            {
                tempCustomBlackBoardNodeList.Clear();
                WriteBlackList(skillBlackList, E_BlackBoardTag.Server, ref tempCustomBlackBoardNodeList);
                tempCustomBlackBoardNodeList.Clear();
            }

            RepeatedField<RunStageRet> runStageRets = skillUseRet.StageList;

            foreach (RunStageRet runStageRet in runStageRets)
            {
                SkillStage skillStage = GetSkillStageOnRunStageRet(runStageRet);
                if (skillStage == null)
                {
                    //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [server] PreUpdateSeverRecoverStage StageID : {runStageRet.StageID} , StageLoop : {runStageRet.StageLoop} , not find stage!!!!");
                }
                else
                {
                    {
                        // 先将 黑板数据 写入黑板
                        tempCustomBlackBoardNodeList.Clear();
                        skillStage.WriteBlackList(runStageRet.BlackList, E_BlackBoardTag.Server, ref tempCustomBlackBoardNodeList);
                        tempCustomBlackBoardNodeList.Clear();

                        skillStage.UpdateCurStageCreteTime(runStageRet);
                    }

                }
            }
        }



        internal override void EnterFrame()
        {
            base.EnterFrame();
            OnUpdate();
        }

        private SkillStage GetSkillStageOnRunStageRet(RunStageRet runStageRet)
        {
            int stageID = runStageRet.StageID;
            int loopIdx = runStageRet.StageLoop;

            SkillStage stage = GetSkillStage(stageID, loopIdx);

            if (stage != null)
            {
                return stage;
            }

            // 如果 正常的阶段找不到的时候,那就看是不是 otherStage, 对于触发器等这种策划配置的
            // 其它阶段，技能里面并不会提前创建这个阶段
            StageJson stageJson = curSkillInfo.GetStageJson(stageID);
            if (stageJson == null)
            {
                return null;
            }

            TimeLineStage timeLineStag = curSkillInfo.GetOtherTimeLineStage(stageID);
            StageInfo stageInfo = new StageInfo(stageJson, timeLineStag, loopIdx, E_StageType.Skill);
            stage = CteateSkillStage(stageInfo, loopIdx);

            // SGF.Debuger.Log($"[SkillStage] 技能创建 服务器阶段: {stage.StageIDStr}");

            long curStageRealyTime = TimeUtils.ServerNowStampMilli - runStageRet.CreateTime;

            curStageRealyTime = Math.Abs(curStageRealyTime);
            // 处理阶段误差, 目前发现 误差时间 可能大于 100ms, 所以 目前 如果 延迟>100 , 先 -100
            double recoverTime = curStageRealyTime <= GameConfig.SKILL_LAG_TIME ? 0 : Math.Abs(curStageRealyTime - GameConfig.SKILL_LAG_TIME);

            bool isRecover = recoverTime > GameConfig.SKILL_LAG_TIME;


            //SGF.Debuger.Log($"[server] {TagFlag}  OnRunStageRet new Create stage [{stage.StageIDStr}]  , curStageRealyTime : {curStageRealyTime} , recoverTime : [{recoverTime}]");
            E_SkillStageEnterType enterType = isRecover ? E_SkillStageEnterType.Recover : E_SkillStageEnterType.Default;
            stage.OnEnter(recoverTime, enterType, false);

            serverCreateOtherStages.Add(stage);
            return stage;
        }

        private SkillStage GetSkillStage(int stageID, int loopIdx)
        {
            string key = SkillStage.FormatStageIDStr(stageID, loopIdx);
            SkillStage skillStage = null;
            if (stageDic.TryGetValue(key, out skillStage))
            {
                skillStage = stageDic[key];
            }

            return skillStage;
        }


        private SkillStage GetUIDSkillStage(ulong uid)
        {
            foreach (KeyValuePair<string, SkillStage> item in stageDic)
            {
                if (item.Value.StageUID == uid)
                {
                    return item.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 阶段运行时 同步黑板 数据的接口.
        /// 目前先通过技能找到相应的黑板的方式 进行同步
        /// </summary>
        public void OnServerRunStage(RunStageRet runStageRet, bool isRecover)
        {
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [server] OnServerRunStage StageID : {runStageRet.StageID} , StageLoop : {runStageRet.StageLoop} start");

            SkillStage skillStage = GetSkillStageOnRunStageRet(runStageRet);
            if (skillStage == null)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [server] OnServerRunStage StageID : {runStageRet.StageID} , StageLoop : {runStageRet.StageLoop} , not find stage!!!!");
                return;
            }

            List<string> keys = FormatDebugKeys(runStageRet.BlackList);
#if (UNITY_EDITOR && BATTLE_DEBUG)
            DebugServerEffectData($"runStageRet", keys);
#endif
            skillStage.OnRunStageRet(runStageRet, isRecover);

            // 如果走的是阶段恢复的逻辑, 那就将阶段恢复的 阶段记录下来
            if (isRecover)
            {
                recoverStageDic[skillStage.StageIDStr] = skillStage;
            }
        }

        /// <summary>
        /// 2024-6-13
        ///     1.结束所有的循环阶段；
        ///     2.立即结束这个阶段的所有效果线
        /// </summary>
        /// <param name="runStageForceEndRet"></param>
        public void OnServerRunStageForceEndRet(RunStageForceEndRet runStageForceEndRet)
        {
            ulong uid = runStageForceEndRet.ID;
            SkillStage skillStage = GetUIDSkillStage(uid);
            if (skillStage == null)
            {
                return;
            }

            ///  2023/2/28
            ///  跟夏哥沟通 阶段被强制结束,服务器如何通知 客户端的问题如下:
            ///     增加 一个 RunStageForceEndRet 阶段被强制 结束通知客户端的协议;
            /// 
            ///  分析如下：
            ///  如果 确实找到了 这个阶段, 就判断 当前阶段是不是 服务器强制同步的阶段
            ///      如果是, 那直接 结束当前阶段即可
            ///      如果不是,就需要 去区分 当前阶段 是在 这个阶段的前面还是 后面.
            ///         如果当前阶段是 后面阶段,表明 强制结束的阶段 本地已经结束,那就不用管(自己的主动技能,
            ///             客户端的阶段跑的可能比 服务器要快)
            ///         如果当前阶段是 前面阶段,说明 服务器 跑的比客户端快(其他人的技能),此时 应结束
            ///             当前阶段, 同时追到 强制结束的阶段,结束它

            if (!skillStage.NeedActive)
            {
                // 目前约定 非活跃阶段 不允许打断, 而技能的打断, 服务器 也不应该把阶段 打断 告诉 客户端
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 收到 服务器 打断 非活跃阶段 skillStage: {skillStage.StageIDStr} , 不允许！！！");
                return;
            }

            if (skillStage == CurSkillStage)
            {
                /// 2023/3/2 15:18
                /// 打断 这块 目前 有几个问题:
                ///     服务器:
                ///         1. 目前 服务器 循环阶段的打断, 是打断所有的循环阶段,而不是 最后的一个循环阶段,跟策划还是有所区别(目前夏哥正在改);
                ///         2. 服务器 用户输入轴 是挂在 技能身上, 所以 输入轴 中的 阶段被打断的时候, 并不会 影响 输入轴效果,
                ///            从而导致 后续的阶段 由于前面阶段的打断而落在 用户输入轴 上, 这样其实是 错误的.
                ///     客户端:
                ///         1. 跟服务器一样,阶段被打断也不会影响 输入轴的效果, 这个也是个问题;
                ///         2. 客户端收到了 阶段打断后,会 打断 这个阶段. 同时,由于 
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 收到 服务器 打断 当前阶段 CurSkillStage: {CurSkillStage.StageIDStr} active: {CurSkillStage.NeedActive}");

                /// 2023/3/2
                /// 跟夏哥 沟通如下:
                ///     1.阶段被打断,输入轴 并不应该被打断,而是 减去被打断阶段的事件;
                ///     2.阶段被打断,输入轴 也不应该走超时逻辑, 超时 一般配置的是 超时结束技能(普工)；
                ///     3.对于 三段斩 这种技能,砍第一刀中途被其它技能打断,此时 如果想 打断输入轴,那就无法挥出第二刀;
                ///     4.非活跃阶段 不允许被打断, 如果收到了 服务器 非活跃阶段被打断, 就说明 服务器发错了.
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [xcxc] 收到 服务器 打断阶段 就是当前阶段 CurSkillStage: {CurSkillStage.StageIDStr} active: {CurSkillStage.NeedActive}");

                BreakCurStageToNext(E_SkillStageExitType.Broken);
                return;
            }

            if (CurSkillStage == null)
            {

                return;
            }

            int stageID = skillStage.StageID;
            int loopIdx = skillStage.LoopIdx;

            bool isNextStage = CheckStageIsCurNextStage(stageID, loopIdx);
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [xcxc] 收到 服务器 打断阶段 [{stageID}_{loopIdx}] CurSkillStage: {CurSkillStage.StageIDStr} active: {CurSkillStage.NeedActive}, 是NextStage: {isNextStage}");
            // 对于主动技能来说, 阶段是 按顺序的,所以：
            //     如果服务器 结束的是 后续的阶段, 那就很简单, 客户端  依次 释放阶段,一直到 服务器同步的阶段
            if (isNextStage)
            {
                // 一直找到 当前的阶段 就等于 服务器强制 关闭的阶段
                while (CurSkillStage != null && !CurSkillStage.EqualStageID(stageID, loopIdx))
                {
                    //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [xcxc] 往后 打断阶段 CurSkillStage: {CurSkillStage.StageIDStr} active: {CurSkillStage.NeedActive}");
                    BreakCurStageToNext(E_SkillStageExitType.Broken);
                }
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [xcxc] 往后 打断阶段 CurSkillStage: {CurSkillStage.StageIDStr} active: {CurSkillStage.NeedActive}");
                // 结束 当前阶段,跳转到 下一个阶段
                BreakCurStageToNext(E_SkillStageExitType.Broken);
            }
            else
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [xcxc] 往前 打断阶段  [{stageID}_{loopIdx}], CurSkillStage: {CurSkillStage.StageIDStr} active: {CurSkillStage.NeedActive}");

                // 如果是 之前的阶段, 那就看 之前的阶段 有没有 走过 onExit,
                // 目前 skillStage 内部会 处理 只走依次 onExit, 所以 直接再次打断 一次之前的阶段就可以了
                BreakStage(skillStage, E_SkillStageExitType.Broken);
            }
        }

        /// <summary>
        /// 同步到 指定的阶段
        /// </summary>
        /// <param name="stageID"></param>
        /// <param name="loopIdx"></param>
        /// <param name="serverStageCreateTime">服务器 同步阶段的创建时间</param>
        private void Sync2Stage(int stageID, int loopIdx, long serverStageCreateTime)
        {
            // 补丁
            if (CurSkillStage == null)
            {
                SGF.Debuger.LogError($"{TagFlag} 阶段同步的时候, 本地阶段为 null , 异常！！！");
                return;
            }
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 准备同步阶段, curStage {CurSkillStage.StageIDStr} --> {stageID}_{loopIdx}");
            //SGF.Debuger.Log($"{TagFlag} [SkillStage] 准备同步阶段, curStage {CurSkillStage.StageIDStr} --> {stageID}_{loopIdx}");
            if (CurSkillStage.EqualStageID(stageID, loopIdx))
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 准备同步阶段 未变, 跳过");
                return;
            }

            bool sync2Next = CheckStageIsCurNextStage(stageID, loopIdx);

            // 如果 服务器 同步 到的是  后面的阶段,那客户端 执行的 是跳转到 后续阶段的逻辑
            if (sync2Next)
            {
                Sync2NextStage(stageID, loopIdx, serverStageCreateTime);
            }
            else
            {
                // 否则的话, 执行的是 回溯到 之前 阶段的逻辑
                Backtrack2Stage(stageID, loopIdx);
            }
        }

        /// <summary>
        /// 同步 到 后续 的 阶段
        /// </summary>
        /// <param name="stageID"></param>
        /// <param name="loopIdx"></param>
        /// <param name="serverStageCreateTime">服务器 同步阶段的创建时间</param>
        private void Sync2NextStage(int stageID, int loopIdx, long serverStageCreateTime)
        {
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 准备同步到后续阶段, curStage {CurSkillStage.StageIDStr} --> {stageID}_{loopIdx} , serverStageCreateTime: {serverStageCreateTime}");

            // 进入当前阶段
            SkipToStage(0, E_SkillStageEnterType.RecoverCurStage, stageID, loopIdx);

        }

        /// <summary>
        /// 将 技能 回溯到 指定的阶段
        /// </summary>
        private void Backtrack2Stage(int stageID, int loopIdx)
        {
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 准备回溯, curStage {CurSkillStage.StageIDStr} --> {stageID}_{loopIdx}");

            List<SkillStage> backtrackStages = GetStageChangeList(CurSkillStage.StageID, CurSkillStage.LoopIdx, stageID, loopIdx);

            // 阶段的回溯 需要 按照阶段的执行 顺序 倒序 回溯, 所以 当执行的是回溯的时候,
            // backtrackStages 返回的是 执行顺序 相反的阶段list 
            backtrackStages.ForEach((SkillStage stage) =>
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 开始回溯 stage {stage.StageIDStr}");
                stage.Backtrack2Init();
            });
            // 先 从 已经执行的queue中 删除 需要回溯的 阶段
            executedStages.Remove(backtrackStages);

            // 将回溯队列 反转, 此时 得到的是 执行的 阶段 顺序
            backtrackStages.Reverse();

            // 如果回溯的阶段队列中包含当前阶段, 则先将当前阶段出栈
            if (backtrackStages[backtrackStages.Count - 1].StageIDStr == CurSkillStage.StageIDStr)
            {
                stagesQueue.Dequeue();
            }

            // 按 阶段的执行顺序,从 queue 的 头部 插入这个 阶段队列
            stagesQueue.HeadEnqueue(backtrackStages);

            backtrackStages.Clear();

            // 进入当前阶段
            EnterCurStage(0, E_SkillStageEnterType.Recover, true);
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 回溯后 curStage {CurSkillStage.StageIDStr}");
        }

        public void WriteBlackList(RepeatedField<BlackBoardNode> blackLists, E_BlackBoardTag tag, ref List<CustomBlackBoardNode> customBlackBoardNodes)
        {
            foreach (BlackBoardNode item in blackLists)
            {
                CustomBlackBoardNode customBlackBoardNode = WriteBlackBoard(item, tag);
                if (customBlackBoardNode != null)
                {
                    customBlackBoardNodes.Add(customBlackBoardNode);
                }
            }


        }

        public void WriteOwner(ulong ownerID, ulong buildID)
        {
            BlackBoardNode Owner = new BlackBoardNode();
            Owner.Key = "Owner";
            Owner.Uint64Value = ownerID;
            WriteBlackBoard(Owner, E_BlackBoardTag.Server);

            BlackBoardNode Builder = new BlackBoardNode();
            Builder.Key = "Builder";
            Builder.Uint64Value = buildID;
            WriteBlackBoard(Owner, E_BlackBoardTag.Server);

        }

        private CustomBlackBoardNode WriteBlackBoard(BlackBoardNode item, E_BlackBoardTag tag)
        {
            CustomBlackBoardNode customBlackBoardNode = BaseBlackBoard.InitServerCustomBlackBoardNode(item.Key, item, _builderID, _ownerEntityID);
            if (customBlackBoardNode != null)
            {
                string key = item.Key;
                // 写入技能黑板数据(此处是服务器黑板数据同步过来,所以直接存入阶段黑板)
                // 同时,服务器同步的数据,写入服务器黑板数据块
                if (skillBlackBoard == null)
                {
                    // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} WriteBlackBoard key={item.Key} , skillBlackBoard=null");
                }
                skillBlackBoard?.Set(key, customBlackBoardNode, tag);
            }

            return customBlackBoardNode;
        }

        private void HandleItemBlackBoard(CustomBlackBoardNode customBlackBoardNode, bool isRecover, string tag)
        {
            string key = customBlackBoardNode.Key;

            // LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag}[server] {tag} HandleItemBlackBoard key : {key} , isRecover {isRecover}", key.Contains("input", StringComparison.CurrentCultureIgnoreCase));
            // 写入技能黑板数据(此处是服务器黑板数据同步过来,所以直接存入阶段黑板)
            // 同时,服务器同步的数据,写入服务器黑板数据块
            if (!isRecover)
            {
                switch (key)
                {
                    case "CurStageID":
                        {
                            /// 2023/3/1
                            /// 目前 约定 , 服务器 的 RuntimeSyncRet 中 如果有 下面3个字段, 表明服务器 阶段 发生了 跳转.
                            ///     目前 客户端 先不做 阶段 打断的 提前预演
                            ///     当前阶段ID : "CurStageID"
                            ///     当前阶段loop次数 : "CurStageLoop"
                            ///     当前阶段开始时间 : "CurStageCreate"

                            /// note: 
                            ///     收到服务器 RuntimeSynceRet 同步的时候, 需要注意以下几点:
                            ///     1.只有 主动技能 才会有 当前 阶段 被恶意打断变化后的同步数据;
                            ///     2.客户端 收到了 服务器同步的阶段数据后, 需要考虑 收到的 阶段是 当前的阶段 后面 还是 前面;
                            ///     3.如果是 后面, 那把当前阶段 追赶到 服务器 同步的 阶段 即可;
                            ///     4.如果是 前面, 那就需要把 当前 阶段 往前 回溯到 服务器同步的阶段, 有点麻烦;

                            // Backtrack2Stage(int stageID, int loopIdx)

                            // 走到服务器 恶意打断 阶段 强同步 到客户端的时候, 此时 服务器以下3个数据 都会同步过来

                            int curStageID = GetBlackValue<int>("CurStageID", E_BlackBoardTag.Server);
                            int curStageLoop = GetBlackValue<int>("CurStageLoop", E_BlackBoardTag.Server);
                            long curStageCreateTime = GetBlackValue<long>("CurStageCreate", E_BlackBoardTag.Server);
                            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [xcxc] 收到黑板同步 强制同步到: {curStageID}_{curStageLoop}, CurSkillStage: {CurSkillStage.StageIDStr} active: {CurSkillStage.NeedActive}");

                            Sync2Stage(curStageID, curStageLoop, curStageCreateTime);

                        }
                        break;
                    default:
                        {
                            StageTryPlayResitedServerEffect(customBlackBoardNode, skillBlackBoard);
                        }
                        break;
                }
            }
        }

        private T GetBlackValue<T>(string key, E_BlackBoardTag tag)
        {
            CustomBlackBoardNode customBlackBoardNode = (CustomBlackBoardNode)skillBlackBoard.Get(key, tag);
            if (customBlackBoardNode != null)
            {
                return (T)customBlackBoardNode.Value;
            }
            return default(T);
        }
        private List<CustomBlackBoardNode> tempCustomBlackBoardNodeList = new List<CustomBlackBoardNode>();

        protected void HandleBlackList(RepeatedField<BlackBoardNode> blackList, string tag, bool isRecover)
        {
            if (blackList == null || blackList.Count == 0)
            {
                return;
            }

            tempCustomBlackBoardNodeList.Clear();

            foreach (BlackBoardNode item in blackList)
            {
                CustomBlackBoardNode customBlackBoardNode = WriteBlackBoard(item, E_BlackBoardTag.Server);
                if (customBlackBoardNode != null)
                {
                    tempCustomBlackBoardNodeList.Add(customBlackBoardNode);
                }
            }
#if (UNITY_EDITOR && BATTLE_DEBUG)
            List<string> keys = FormatDebugKeys(blackList);

            DebugServerEffectData($"{tag}", keys);
#endif
            // 对 收到的 黑板数据blackList 按照服务器注册的 regIndex 顺序排序, 用来解决服务器 数据 发过来乱序的问题
            EffectUtils.SortServerCustomBlackList(tempCustomBlackBoardNodeList, skillBlackBoard);

            tempCustomBlackBoardNodeList.ForEach((CustomBlackBoardNode customBlackBoardNode) =>
            {
                // SGF.Debuger.Log($"[server] {TagFlag} {tag} ,[blackBoard]  key : {customBlackBoardNode.Key} , isRecover : {isRecover}");
                HandleItemBlackBoard(customBlackBoardNode, isRecover, tag);
            });

            tempCustomBlackBoardNodeList.Clear();
        }

        /// <summary>
        /// 技能运行时同步 数据的 接口
        /// </summary>
        /// <param name="runtimeSyncRet"></param>
        public void OnRuntimeSyncRet(RuntimeSyncRet runtimeSyncRet)
        {
            //技能收到效果数据的时候,跟阶段一样,也需要执行 服务器效果线的逻辑
            RepeatedField<BlackBoardNode> blackList = runtimeSyncRet.BlackList;

            HandleBlackList(blackList, "runtimeSyncRet", false);
        }

        private void ResetCurAnimation()
        {
            if (curAnimationRecord != null && IsCurAnimationRecordFramStart)
            {
                StagePlayAnim(curAnimationRecordStage, curAnimationRecord, false, 0);
            }
            CleanAnimationRecord();
        }

        private void CleanAnimationRecord()
        {
            curAnimationRecordStage = null;
            curAnimationRecord = null;
            IsCurAnimationRecordFramStart = false;
        }

        private void ReleaseStages()
        {

            // 阶段在 技能 OnExit的时候,应该已经执行完stage.OnExit了
            // 所以在 技能 Release 的时候, 不需要 OnExit.
            {
                stagesQueue.Clear();
                executedStages.Clear();
            }

            stagesListCoyp.ForEach((SkillStage stage) =>
            {
                EntityFactory.ReleaseEntity(stage);
            });

            stagesListCoyp.Clear();


            stageDic.Clear();

            serverCreateOtherStages.ForEach((SkillStage stage) =>
            {
                EntityFactory.ReleaseEntity(stage);
            });
            serverCreateOtherStages.Clear();

        }



        protected override void Release()
        {
            //SGF.Debuger.Log($"{TagFlag} [memory]  Release ");
            // SerSnapshotSeqManager.Instance.RecordRuntime(runtimeID, playerData.M_EntityID, RunTimeState.Destroy);
#if (UNITY_EDITOR && BATTLE_DEBUG)
            DebugSkillDebugData(false, "ReleaseSkill", new List<string>() { $"BuilderID: {BuilderID}" });

#endif
            base.Release();
            // 先还原当前节能动画帧
            ResetCurAnimation();

            foreach (string item in histroyKeySet)
            {
                DelayInvoker.CancelInvoke(item);
            }

            histroyKeySet.Clear();

            playerData = null;
            curSkillInfo = null;

            skillId = 0;
            cfg = null;

            uniqueID = 0;

            RuntimeID = 0;
            Flag_StartEnergy = false;
            energyItemTime = 0;
            energyMinCount = 0;
            energyMaxCount = 0;

            energyedCount = 0;
            energyTotalTime = 0;
            energyUITotalTime = 0;
            isClientSimulating = false;

            isOnlyServerUseSkill = false;

            waitNextFrameUpdate = false;

            clientSkillUseReq = null;

            skillController = null;

            skillBlackBoard.Clear();
            skillBlackBoard = null;

            skillEntityState = E_EntityState.None;
            readyRelease = false;

            hasPreForbidDir = false;

            _followLoopKeyRecord.Clear();
            ReleaseStages();


            ReleaseAction();

            // TODO Delete中的变量,后续看是否有必要 删除
            ReleaseSkillPro();
        }


        #region 新版的技能实现方式

        /// <summary>
        /// 技能阶段 的队列, 会在技能初始化的时候就生成它的所有阶段。
        /// note:
        ///     目前技能的 阶段 是一开始就直接全部创建完成的.
        ///     这样,就能根据 服务器返回的技能时间,依次遍历阶段，定位到当前技能所属的阶段时间点
        /// </summary>
        /// <typeparam name="SkillStage"></typeparam>
        /// <returns></returns>
        private QueueExtends<SkillStage> stagesQueue = new QueueExtends<SkillStage>();

        /// <summary>
        /// 阶段 stages 的 copy list,用来做遍历用
        /// </summary>
        private List<SkillStage> stagesListCoyp = null;

        /// <summary>
        /// 服务器创建的 其它阶段, 目前 技能的 普通阶段,由客户端 本地创建,服务器同步过来数据.
        /// 而对于 类似 技能的 触发器 这种 other 阶段, 则由 服务器同步过来创建
        /// </summary>
        /// <typeparam name="SkillStage"></typeparam>
        /// <returns></returns>
        private List<SkillStage> serverCreateOtherStages = new List<SkillStage>();

        /// <summary>
        /// 已经执行完 阶段OnExit 逻辑的 阶段队列
        /// </summary>
        /// <typeparam name="SkillStage"></typeparam>
        /// <returns></returns>
        private QueueExtends<SkillStage> executedStages = new QueueExtends<SkillStage>();


        /// <summary>
        /// 阶段 dic, 包含所有的 阶段的 映射.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="SkillStage"></typeparam>
        /// <returns></returns>
        private DictionaryEx<string, SkillStage> stageDic = new DictionaryEx<string, SkillStage>();

        /// <summary>
        /// 当前的阶段
        /// </summary>
        /// <value>stage 或者 null</value>
        public SkillStage CurSkillStage
        {
            get
            {
                if (stagesQueue.Count > 0)
                {
                    return stagesQueue.Peek();
                }
                return null;
            }
        }

        /// <summary>
        /// 初始化阶段数据
        /// </summary>
        public void InitStages(SkillInfo skillInfo)
        {
            cfgStageJsons = skillInfo.stageJsons;
            if (cfgStageJsons.Count == 0)
            {
                SGF.Debuger.LogError($"{TagFlag} , InitStages : {skillInfo.skillId} none stages , error!!!");
            }
            stageDic.Clear();
            for (int i = 0; i < cfgStageJsons.Count; i++)
            {
                StageJson stageJson = cfgStageJsons[i];

                for (int l = 0; l < stageJson.StageNormal.LoopCount; l++)
                {
                    TimeLineStage timeLineStag = skillInfo.GetTimeLineStage(stageJson.StageID, l);
                    StageInfo stageInfo = new StageInfo(stageJson, timeLineStag, i, E_StageType.Skill);

                    SkillStage stage = CteateSkillStage(stageInfo, l);

                    //如果有循环阶段,循环创建多个阶段
                    stagesQueue.Enqueue(stage);
                }
            }

            stagesListCoyp = stagesQueue.KToList<SkillStage>();
        }

        /// <summary>
        /// 计算 当前阶段 的 next 阶段 的 阶段id 和 loopIdx
        /// </summary>
        /// <param name="containLoop"></param>
        /// <param name="stageID"></param>
        /// <param name="loopIdx"></param>
        private void GetNextStageIDStr(bool containLoop, out int stageID, out int loopIdx)
        {
            stageID = -1;
            loopIdx = -1;

            if (CurSkillStage == null)
            {
                return;
            }
            // 先找到当前阶段对应的 idx
            int idx = stagesListCoyp.FindIndex((SkillStage stage) =>
            {
                return stage.EqualStageID(CurSkillStage.StageID, CurSkillStage.LoopIdx);
            });

            for (int i = idx + 1; i <= stagesListCoyp.Count - 1; i++)
            {
                SkillStage stage = stagesListCoyp[i];
                // 如果包含 循环阶段,那就直接用下一个 阶段就可以了
                if (containLoop)
                {
                    stageID = stage.StageID;
                    loopIdx = stage.LoopIdx;
                    return;
                }
                // 如果不包含 循环阶段, 就需要跳过循环阶段,直到 下一个非循环阶段 为止
                if (!stage.IsStageLoop)
                {
                    stageID = stage.StageID;
                    loopIdx = stage.LoopIdx;
                    return;
                }
            }
            // 如果后续 没有 非循环阶段, 那就 返回默认的 阶段ID 为 -1
            return;
        }

        private SkillStage CteateSkillStage(StageInfo stageInfo, int loopIdx)
        {
            SkillStage skillStage = EntityFactory.InstanceEntity<SkillStage>();

            string tagStr = TagFlag;
            skillStage.Init(stageInfo, loopIdx, tagStr, BuilderID, OwnerEntityID, skillBlackBoard);

            // SGF.Debuger.Log($"{TagFlag} [skillStage] 创建阶段: {skillStage.StageIDStr} , IsNormalStage: {skillStage.IsNormalStage}");


            skillStage.ActionOnExitStage += OnActionExitStage;
            skillStage.ActionOnRefreshStageStates += OnActionRefreshStageStates;
            skillStage.ActionOnStageStartCD += OnActionStageStartCD;

            skillStage.FuncOnStageTrySetActiveMain += OnActionStageTrySetActiveMain;

            skillStage.FuncOnStageTryPlayAnim += OnFuncStageTryPlayAnim;
            skillStage.ActionOnStageTryPlayFx += OnActionStageTryPlayFx;
            skillStage.ActionOnStageTryPlaySound += OnActionStageTryPlaySound;
            skillStage.ActionOnStageTryStopSound += OnActionStageTryStopSound;
            skillStage.ActionOnStageTryStopFx += OnActionStageTryStopFx;

            skillStage.ActionOnStageTryPlayEffect += OnActionStageTryPlayEffect;
            skillStage.ActionOnStageTryStopEffect += OnActionStageTryStopEffect;


            skillStage.ActionOnStageRegisterServerEffect += OnActionStageRegisterServerEffect;
            skillStage.FuncOnTryPlayRegistedServerEffect += StageTryPlayResitedServerEffect;

            skillStage.ActionOnStageTryPlayCamera += OnActionStageTryPlayCamera;

            skillStage.ActionOnStageTryPlayCameraShake += OnActionOnStageTryPlayCameraShake;

            skillStage.ActionOnStageWriteSkillEffect += OnActionStageWriteSkillEffect;

            skillStage.ActionOnServerStageCreate += OnActionServerStageCreate;

            stageDic[skillStage.StageIDStr] = skillStage;

            return skillStage;
        }

        /// <summary>
        /// 计算 技能从开始  运行到对应stageID阶段,且该阶段循环loopNum次的总时间
        /// </summary>
        /// <param name="stageID"></param>
        /// <param name="loopNum"></param>
        /// <returns></returns>
        private int GetRunToStageMaxTime(int stageID, int loopNum = 0)
        {
            int time = 0;

            List<SkillStage> stagesCopy = stagesQueue.KToList<SkillStage>();
            int idx = stagesCopy.FindIndex((SkillStage value) =>
            {
                return value.EqualStageID(stageID, loopNum);
            });

            if (-1 != idx)
            {
                for (int i = 0; i < idx; i++)
                {
                    time += stagesCopy[i].Time;
                }
            }

            return time;
        }

        /// <summary>
        /// 客户端 预播 的时候 进入的接口. 既然是 预播
        /// </summary>
        /// <param name="enterSkillTime">技能预播时,在帧尾 补 执行这个技能 的 补偿 时间.</param>
        public void OnClientPreEnter(double enterSkillTime)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            DebugSkillDebugData(true, "ClientUseSkill", new List<string>() { $"BuilderID: {BuilderID}" });

            DebugSkillEventData(true, true, "ClientUseSkill", new List<string>() { $"客户端使用技能" });
#endif
            skillEntityState = E_EntityState.Running;
            readyRelease = false;
            //DB_Close       SGF.Debuger.Log($"{TagFlag} [client] OnEnter Time {enterSkillTime} ");
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [client] [v-v] OnClientPreEnter enterSkillTime: {enterSkillTime}");


            /// 2023/4/12
            /// waitNextFrameUpdate 用来控制 当前的阶段执行 update的时候 ,是否需要跳过当前帧,等到下一帧执行.
            /// 如果是 第一个技能的 第一个阶段, 那么技能 和阶段的 补帧 时间 都是 0, 此时
            /// // 阶段进入后, 当前帧已经 根据  enterStageTime 补齐了,所以当前帧 不执行 onUpdate
            if (enterSkillTime > 0 && enterSkillTime <= TimeUtils.FixedDeltaTime)
            {
                waitNextFrameUpdate = true;
            }

            /// 2023/4/11
            /// 发现 普攻 连点 多轮后,客户端服务器之间的 误差被放大.
            /// 检查发现主要的原因 可能是客户端 提前预播的时候,将技能时间提前了.
            /// 客户端 在每次 Update的时候, 检查阶段 是否结束. 如果结束,就跳入下一个阶段. 如果此时触发了 
            /// 预输入技能的逻辑, 则会在 进入非活跃 阶段的时候, 提前预播 进入下一个技能.
            /// 但此时 存在2个问题:
            ///     1.目前技能实体 是一旦创建，就在当前帧执行了 update. 这样其实会将这个技能的执行时间提前.
            ///     2.进入预播的时候,上个技能的结束 其实发生在帧 尾, 那这个技能实际上的执行时间被延后,
            ///       所以需要将上个技能 结束时到这个帧尾 的时间 补偿给预播技能即可. 
            EnterCurStage(enterSkillTime, E_SkillStageEnterType.Default, false);
        }

        public void OnServerEnter(SkillUseRet skillUseRet)
        {

            skillEntityState = E_EntityState.Running;
            readyRelease = false;

            /// 2023/3/10
            ///     之前 跟 夏哥的约定, 服务器返回 的 技能使用中的 CurStageLoop 指的是 服务器 下一个创建的阶段(当前阶段服务器没有额外保存)
            ///     所以 客户端 需要 在收到  CurStageLoop 后, 额外 - 1 处理
            ///     1000 or 时间
            int serverLoopIdx = skillUseRet.CurStageLoop - 1;
            serverLoopIdx = serverLoopIdx < 0 ? 0 : serverLoopIdx;

            string stageIDStr = SkillStage.FormatStageIDStr(skillUseRet.CurStageID, serverLoopIdx);
            // do recover stage
            do
            {
                long enterSkillTime = 0;
                // 当 recoverStageDic 还有 阶段 未恢复的时候,那就先执行 阶段的恢复逻辑
                if (recoverStageDic.Count != 0 && CurSkillStage.StageIDStr != stageIDStr)
                {
                    if (recoverStageDic.ContainsKey(CurSkillStage.StageIDStr))
                    {
                        SkillStage recoverStage = recoverStageDic[CurSkillStage.StageIDStr];

                        recoverStageDic.Remove(CurSkillStage.StageIDStr);

                        enterSkillTime = TimeUtils.ServerNowStampMilli - recoverStage.ServerCreteTime;
                        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [xx-xx] server stage[{CurSkillStage.StageIDStr}] ServerCreteTime {recoverStage.ServerCreteTime} , ServerNowStampMilli: {TimeUtils.ServerNowStampMilli} , enterSkillTime {enterSkillTime}   ");

                        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [xx-xx] server stage[{recoverStage.StageIDStr}] OnEnter Time {enterSkillTime} , ready EnterCurStage {E_SkillStageEnterType.Recover} ");
                        EnterCurStage(enterSkillTime, E_SkillStageEnterType.Recover, false);
                    }
                    else
                    {
                        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [xx-xx] OnServerEnter skip stage[{CurSkillStage.StageIDStr}] ");

                        // 如果 没有 这个阶段的恢复数据,且存在 阶段未恢复,那就 继续往后续阶段跳
                        EndCurStageUpdate();
                    }
                }
                else
                {
                    if (CurSkillStage == null)
                    {

                        SGF.Debuger.LogWarning($"{TagFlag} 收到服务器使用技能时, CurSkillStage=null, 直接跳过");
#if UNITY_EDITOR
                        // UnityEngine.Debug.Break();
#endif
                        break;
                    }

                    // 如果 没有这个阶段的恢复数据, 且当且阶段又不是服务器同步的阶段,那就继续往后跳
                    if (CurSkillStage.StageIDStr != stageIDStr)
                    {
                        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [xx-xx] OnServerEnter skip stage[{CurSkillStage.StageIDStr}] server stageIDStr : {stageIDStr}");
                        EndCurStageUpdate();
                        continue;
                    }
                    if (recoverStageDic.ContainsKey(CurSkillStage.StageIDStr))
                    {
                        recoverStageDic.Remove(CurSkillStage.StageIDStr);
                    }
                    int itemStageTime = CurSkillStage.Time;
                    // 当前阶段的时间 : 阶段当前时间 = 客户端本地服务器时间 - 服务器阶段创建时间CurStageTime - 循环次数loop X 单个循环阶段的时间.
                    double curStageRealyTime = TimeUtils.ServerNowStampMilli - skillUseRet.CurStageTime - serverLoopIdx * itemStageTime;
                    curStageRealyTime = curStageRealyTime < 0 ? 0 : curStageRealyTime;

                    double recoverTime = curStageRealyTime < 500 ? 0 : curStageRealyTime;
                    //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [xx-xx] server stage[{CurSkillStage.StageIDStr}] CurStageTime {skillUseRet.CurStageTime} , ServerNowStampMilli: {TimeUtils.ServerNowStampMilli} , curStageRealyTime {curStageRealyTime} ----> recoverTime: {recoverTime}");

                    //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [xx-xx] server stage[{CurSkillStage.StageIDStr}] OnEnter Time {curStageRealyTime} , ready EnterCurStage {E_SkillStageEnterType.Recover} ");

                    EnterCurStage(recoverTime, E_SkillStageEnterType.RecoverCurStage, true);
                    break;
                }

            } while (CurSkillStage != null);
        }

        /// <summary>
        /// 技能运行时结束
        /// </summary>
        public void OnExit(E_SkillExitType exitType)
        {
            // SkillDebugData debugData = InitSkillDebugData(RuntimeID.ToString(), false, $"SkillExit_{exitType}", new List<string>() { $"now:[{now}]" });
            // BattleDebugHelper.Debug(debugData);
#if (UNITY_EDITOR && BATTLE_DEBUG)
            DebugSkillEventData(false, false, "SkillExit", new List<string>() { $"技能结束类型: {exitType}" });
#endif
            CancelInvokeExecuteUserInput();
            // LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} OnExit CurSkillStage[{CurSkillStage?.StageIDStr}] ,技能结束类型: {exitType} ");

            // 如果是 服务器正常结束,只更新技能状态,但不执行正常的技能退出逻辑
            if (exitType == E_SkillExitType.ServerDefault)
            {
                UpdateSkillEntityState(exitType);

                // 如果是服务器正常结束技能,且流程由服务器判断，那么 收到 ServerDefault 的时候,状态可能就已经
                // 变为 准备释放了. 此时,应该通知外面 这个技能 实体结束了.至于 是否释放,可以放下一帧 update执行
                if (readyRelease)
                {
                    // 通知外面结束,同时,清除 activeSkill
                    ActionOnSkillExit?.Invoke(this, exitType);
                }

                return;
            }

            while (executedStages.Count > 0)
            {
                SkillStage skillStage = executedStages.Dequeue();
                skillStage.OnExitSkill(exitType);
            }

            if (CurSkillStage != null)
            {
                CurSkillStage.OnExitSkill(exitType);
            }

            UpdateSkillEntityState(exitType);

            OnStageChange(0);
            OnActionExitStartCD(exitType);
            ActionOnSkillExit?.Invoke(this, exitType);
        }

        private void UpdateSkillEntityState(E_SkillExitType exitType)
        {
            E_EntityState newState = E_EntityState.None;
            switch (exitType)
            {
                case E_SkillExitType.Default:
                case E_SkillExitType.ClientBreakSkill:
                case E_SkillExitType.FailedSetMainSkill:
                    {
                        // 如果 客户端结束的时候,服务器还未回复,那就是客户端预播被打断,此时就直接结束这个技能
                        if (isClientSimulating)
                        {
                            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [memory] UpdateSkillEntityState exitType {exitType}, 技能没收到服务器回复 就被打断, 预播被打断,结束这个技能!!! ");

                            newState = readyReleaseState;
                        }
                        else
                        {
                            newState = E_EntityState.ClientClose;
                        }

                    }
                    break;
                case E_SkillExitType.ServerDefault:
                    {
                        // 如果是 纯服务器 使用技能, 那服务器通知技能结束,就结束这个技能.
                        if (isOnlyServerUseSkill)
                        {
                            newState = readyReleaseState;
                        }
                        else
                        {
                            newState = E_EntityState.ServerClose;
                        }
                    }
                    break;
                case E_SkillExitType.ServerBreakSkill:
                case E_SkillExitType.Reset:
                    {
                        newState = readyReleaseState;
                    }
                    break;
                default:
                    {
                        // 如果新增了一个 不知道 类型的打断, 那么认为这个类型 能够直接打断这个技能
                        newState = readyReleaseState;
                        //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [memory] UpdateSkillEntityState exitType {exitType} , 没有处理这个类型, 默认设置这个 技能结束类型 可以直接释放这个 技能!!!");

                    }
                    break;
            }

            // 刷新新的状态
            skillEntityState = skillEntityState | newState;

            readyRelease = (skillEntityState & readyReleaseState) == readyReleaseState;

            //if (IsNormalSkill)
            //{
            //    SGF.Debuger.LogError($"{TagFlag} UpdateSkillEntityState exitType {exitType} , readyRelease : {readyRelease} , IsClientRunning {IsClientRunning} , , skillEntityState {skillEntityState}");
            //}
        }

        public bool IsServerEndSkill()
        {
            return (skillEntityState & E_EntityState.ServerClose) == E_EntityState.ServerClose;
        }

        public bool IsClientEndSkill()
        {
            return (skillEntityState & E_EntityState.ClientClose) == E_EntityState.ClientClose;
        }

        /// <summary>
        /// 逻辑帧：30次每秒
        /// </summary>
        public void OnUpdate()
        {
            // 如果 需要等待 一帧,那就 本帧 跳过
            if (waitNextFrameUpdate)
            {
                waitNextFrameUpdate = false;
                return;
            }

            // 服务器创建的 otherStage update
            serverCreateOtherStages.ForEach((stage) =>
            {
                stage.OnUpdate();
            });

            // 技能 让它的每个阶段 都执行 onUpdate,而不是 只对CurSkillStage 执行update
            // 因为 阶段的时间线的执行时间 可能会超过 阶段的时间Time
            stagesListCoyp.ForEach((SkillStage stage) =>
            {
                stage.OnUpdate();
            });

            if (CurSkillStage != null)
            {
                // CurSkillStage.OnUpdate();
                OnEnergyUpdate();
            }
            else
            {
                OnExit(E_SkillExitType.Default);
            }
        }

        /// <summary>
        /// 进入当前阶段
        /// </summary>
        /// <param name="enterSkillTime"></param>
        /// <param name="skillStageEnterType"></param>
        /// <param name="force">是否需要 强制 设置活跃技能, 只有 服务器技能同步时,才需要如此设置</param>
        public void EnterCurStage(double enterSkillTime, E_SkillStageEnterType skillStageEnterType, bool force = false)
        {
            // 如果 服务器已经结束了,那客户端就不跳转下个阶段了,当前阶段结束后就结束
            if (CurSkillStage != null && !IsServerEndSkill())
            {
                //SGF.Debuger.LogError($"{TagFlag} [client] [skillStage] EnterCurStage CurStageTime {CurSkillStage.StageIDStr} skillStageEnterType {skillStageEnterType} ");

                OnStageChange(CurSkillStage.StageIdx);
                CurSkillStage.OnEnter(enterSkillTime, skillStageEnterType, force);
            }
            else
            {
                OnExit(E_SkillExitType.Default);
            }
        }

        /// <summary>
        /// 结束当前阶段的update
        /// note:
        ///     阶段结束,效果线并不结束.阶段 运行时结束了,但是依旧可以收到服务器的效果数据.
        ///     所以,阶段结束后,只是停止阶段的update,并不立即销毁阶段.
        ///     所有阶段的清理,在技能结束之后统一处理(目前技能结束,由服务器统一通知).
        /// </summary>
        /// <param name="executedStage">默认为执行了当前阶段，执行了当前阶段后，就会放入已经执行队列，在技能结束的时候，用于取消技能阶段注册的 效果</param>
        private SkillStage EndCurStageUpdate(bool executedStage = true)
        {
            if (stagesQueue.Count == 0)
            {
                return null;
            }

            SkillStage stage = stagesQueue.Dequeue();

            if (stage == null)
            {
                return null;
            }
            // SGF.Debuger.Log($"{TagFlag} [client] [skillStage] EndCurStageUpdate stage {stage.StageIDStr}  ");
            executedStages.Enqueue(stage);
            return stage;
        }

        /// <summary>
        /// 进入下一个阶段
        /// </summary>
        /// <param name="lastStage">上一个阶段是谁, 只有上一个阶段== curSkillStage 的时候, EnterNextStage 才是生效的 </param>
        /// <param name="enterSkillTime"></param>
        /// <param name="skillStageEnterType"></param>
        public void EnterNextStage(SkillStage lastStage, double enterSkillTime, E_SkillStageEnterType skillStageEnterType)
        {
            // 有可能是之前的阶段 也走了 阶段的退出流程, 进入了这个接口,所以 此处需要将 退出的阶段 和 当前阶段 做一个对比,
            // 只有 是 当前阶段的时候,
            if (lastStage != CurSkillStage)
            {
                return;
            }

            // 如果 需要跳过 后续的循环阶段
            if (CurSkillStage.IsSkipLoopStage)
            {

                int skipStageID = CurSkillStage.StageID;

                // 首先是要 结束 当前阶段
                EndCurStageUpdate(true);

                while (CurSkillStage != null && CurSkillStage.IsStageLoop && CurSkillStage.StageID == skipStageID)
                {
                    EndCurStageUpdate(true);
                }
            }
            else
            {
                EndCurStageUpdate(true);
            }

            if (CurSkillStage != null)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} EnterNextStage 进入下个阶段 {CurSkillStage.StageIDStr}, enterSkillTime: {enterSkillTime}, skillStageEnterType: {skillStageEnterType}");
            }
            else
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} EnterNextStage 阶段结束,没有 后续阶段.");
            }



            EnterCurStage(enterSkillTime, skillStageEnterType);
        }

        /// <summary>
        /// 跳转到 指定的 阶段, 包含循环阶段的跳转
        /// </summary>
        /// <param name="enterSkillTime"></param>
        /// <param name="skillStageEnterType"></param>
        /// <param name="stageID"></param>
        /// <param name="loopIdx"></param>
        public void SkipToStage(double enterSkillTime, E_SkillStageEnterType skillStageEnterType, int stageID, int loopIdx)
        {
            if (CurSkillStage == null)
            {
                return;
            }
            // 如果要跳转的阶段 就是 当前阶段,那就 不管
            if (CurSkillStage.EqualStageID(stageID, loopIdx))
            {
                return;
            }

            // 如果不是当前的阶段, 那就往接下来的阶段跳,一直 跳到 设定的阶段位置
            while (CurSkillStage != null && !CurSkillStage.EqualStageID(stageID, loopIdx))
            {
                BreakCurStageToNext(E_SkillStageExitType.Broken);
            }

            EnterCurStage(enterSkillTime, skillStageEnterType);
        }

        /// <summary>
        /// 跳转到下一个阶段，不包含循环阶段
        /// </summary>
        /// <param name="enterSkillTime"></param>
        /// <param name="skillStageEnterType"></param>
        public void SkipToNextStage(double enterSkillTime, E_SkillStageEnterType skillStageEnterType)
        {
            int lastStageID = CurSkillStage.StageID;
            //先将当前的阶段 放入已经执行队列
            EndCurStageUpdate(true);

            //直接跳到下一个 阶段(循环阶段也跳过)
            while (CurSkillStage != null && CurSkillStage.StageID == lastStageID)
            {
                SkillStage stage = EndCurStageUpdate(false);
                // SGF.Debuger.Log($"{TagFlag} SkipToNextStage skip : {stage.StageIDStr} , lastStageID : {lastStageID} ");
            }
            EnterCurStage(enterSkillTime, skillStageEnterType);
        }


        #region  设置 阶段 跳转 数据的逻辑

        /// <summary>
        /// 设置 下一次跳转的 阶段 数据
        /// </summary>
        /// <param name="skipStageID"></param>
        /// <param name="loopIdx"></param>
        /// <param name="blackBoardTag">是 客户端 跳转 还是 服务器跳转, 如果服务器跳转 和 客户端跳转 同时存在, 那就 执行 服务器跳转, 同时,清除所有的跳转数据 </param>
        /// <param name="skipToStageType">跳转的 类型, 是 阶段结束跳转 还是 怎样跳转, 这个参数是为了 后面 方便拓展</param>
        private void SetSkipToStage(int skipStageID, int loopIdx, E_BlackBoardTag blackBoardTag, E_SkipToStageType skipToStageType)
        {
            if (-1 == skipStageID)
            {
                return;
            }
            StageSkipData stageSkipData = new StageSkipData(skipStageID, loopIdx, skipToStageType);
            skillBlackBoard.Set(BaseBlackBoard.KEY_SKIP_TO_STAGEID, stageSkipData, blackBoardTag);
        }

        /// <summary>
        /// 检查 是否 设置 了 参数 类型的 跳转阶段 数据
        /// </summary>
        /// <param name="skipToStageType"></param>
        /// <returns></returns>
        private bool CheckHasSetSkipStage(E_SkipToStageType skipToStageType)
        {
            string key = BaseBlackBoard.KEY_SKIP_TO_STAGEID;

            if (!skillBlackBoard.ContainKey(key, true))
            {
                return false;
            }
            StageSkipData stageSkipData = skillBlackBoard.GetKey<StageSkipData>(key, false);

            return stageSkipData.SkipToStageType == skipToStageType;
        }

        /// <summary>
        /// 直接跳转到对设置的 对应阶段
        /// </summary>
        public void SkipToSetSkipStage(E_SkipToStageType skipToStageType, double enterStageTime)
        {
            string key = BaseBlackBoard.KEY_SKIP_TO_STAGEID;

            if (!CheckHasSetSkipStage(skipToStageType))
            {
                return;
            }

            StageSkipData stageSkipData = skillBlackBoard.GetKey<StageSkipData>(key, false);

            int skipStageID = stageSkipData.StageID;
            int loopIdx = stageSkipData.LoopIdx;
            skillBlackBoard.RemoveKey(key, true);

            SkipToStage(enterStageTime, E_SkillStageEnterType.Default, skipStageID, loopIdx);
        }

        #endregion

        /// <summary>
        /// 当阶段被打断的接口, 此处 处理的 是 善意打断(阶段由于 技能自己的 效果 而被打断)
        /// </summary>
        private void OnStageBroken(SkillStage skillStage, double enterStageTime)
        {
            // 当阶段被打断的时候,首先检查 服务器有没有强制 设置需要跳转到什么阶段
            if (CheckHasSetSkipStage(E_SkipToStageType.OnCurStageExit))
            {
                SkipToSetSkipStage(E_SkipToStageType.OnCurStageExit, enterStageTime);
                return;
            }

            /// 2023/3/9
            /// 此处 是为了 处理 阶段打断时, 没有设置 跳转阶段,导致 技能 暂停执行的问题.
            /// 
            /// note:
            ///     目前的 阶段打断 可能分为以下几种情况:
            ///     1.技能自身 的 效果打断, 这种打断 是客户端 能够推演的打断, 在阶段被打断时,
            ///       设置 跳转到对 应的目标阶段.
            ///     
            ///     2.服务器通知的 被其他人打断的 恶意打断, 由于 网络延迟,可能出现 客户端 收到
            ///       这个协议的时候, 本地 这个阶段 已经 结束的情况.
            ///       如果 这个阶段 已经结束, 那 本地 就不需要 跳转, 啥都不用干.

            /// Fix 不太可能出现的异常bug:
            ///     如果 收到了一个阶段被打断, 但是 又没有设置 打断阶段的时候,跳转 到 哪个阶段,
            ///     同时 这个阶段 又是 当前阶段, 此时 其实 就是 一个异常 bug 状态.
            if (CurSkillStage != null && CurSkillStage == skillStage)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} OnStageBroken stage[{skillStage.StageIDStr}] 未设置 后续阶段, 此处加个容错处理 error!!!!");
                EndCurStageUpdate();
                EnterCurStage(enterStageTime, E_SkillStageEnterType.Default, true);
            }

            return;

        }

        /// <summary>
        /// 阶段发生改变 通知外层的接口
        /// </summary>
        /// <param name="stageIdx"></param>
        private void OnStageChange(int stageIdx)
        {
            // 更新当前技能  基于配置的idx
            _curStageIdx = stageIdx;
        }

        #endregion


        #region 技能处理效果的接口

        private CustomBlackBoardNode InitEffectBoardNode(string key, I_EffectParam effectParam)
        {
            CustomBlackBoardNode effectBlackBoardNode = SkillBlackBoard.InitClientCustomBlackBoardNode(key, effectParam.Clone(), BuilderID, OwnerEntityID);
            return effectBlackBoardNode;
        }



        /// <summary>
        /// 处理效果的结束帧逻辑
        /// </summary>
        /// <param name="key">自定义存在技能黑板中的key</param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <param name="isFrameStart"></param>
        /// <param name="findAll"></param>
        /// <param name="skipToStageType">跳转阶段的类型,是阶段退出时立即跳转还是 阶段进入时跳转</param>
        /// <param name="state">执行效果节点的状态</param>
        private void ExecuteEndEffect(string key, I_EffectParam effectParam, StageEvent stageEvent, bool nextValue, E_SkipToStageType skipToStageType, E_BlackBoardNodeState state)
        {
            SkillStage skillStage = (SkillStage)effectParam.ExtraData;

            // 效果的结束帧 只执行一次
            if (skillBlackBoard.ContainKey(key))
            {
                CustomBlackBoardNode blackBoardNode = skillBlackBoard.GetKey<CustomBlackBoardNode>(key, false);
                //DB_Close       SGF.Debuger.Log($"{TagFlag} [Input] ExecuteEndEffect  key {key} , effectID {effectParam.EffectID} , state {state} , IsOpen {blackBoardNode.IsOpen}");

                if (!blackBoardNode.IsOpen)
                {
                    return;
                }

                // 更新黑板数据的状态
                blackBoardNode.state = state;


                //立即执行下一个效果
                ImmediatelyPlayEffectNext(skillStage, effectParam.EffectData, nextValue, 0);


                //阶段超时的逻辑
                HandleStageEvent(stageEvent, skipToStageType);

                //执行 结束效果的一些额外逻辑
                OnEndEffect(effectParam, nextValue);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="nextValue"></param>
        private void OnEndEffect(I_EffectParam effectParam, bool nextValue)
        {
            switch (effectParam.SkillEffectType)
            {
                case E_SkillEffect.UserInput:
                    {
                        // TODO : 曲
                        // 看看是否有ui刷新
                        // ui ???
                    }
                    break;
                case E_SkillEffect.Energy:
                    {
                        StopEnergy();
                    }
                    break;
            }
        }

        public bool IsAtEndOfStage()
        {
            if (CurSkillStage != null)
            {
                return CurSkillStage.IsAtEndOfStage();
            }
            return false;
        }

        /// <summary>
        /// 检查 传入阶段 是否是 当前 的 后续阶段
        /// </summary>
        /// <returns></returns>
        public bool CheckStageIsCurNextStage(int stageID, int loopIdx)
        {
            int curIdx = stagesListCoyp.FindIndex((stage) =>
            {
                return stage.EqualStageID(CurSkillStage.StageID, CurSkillStage.LoopIdx);
            });

            int nextStage = stagesListCoyp.FindIndex((stage) =>
            {
                return stage.EqualStageID(stageID, loopIdx);
            });
            return nextStage > curIdx;
        }

        public List<SkillStage> GetStageChangeList(int fromStageID, int fromLoopIdx, int toStageID, int toLoopIdx)
        {
            List<SkillStage> stages = new List<SkillStage>();

            int fromIdx = stagesListCoyp.FindIndex((stage) =>
            {
                return stage.EqualStageID(fromStageID, fromLoopIdx);
            });

            int toIdx = stagesListCoyp.FindIndex((stage) =>
            {
                return stage.EqualStageID(toStageID, toLoopIdx);
            });

            int count = toIdx - fromIdx;

            // from 阶段 在 to 阶段的前面, 表示  从前到后 的阶段 过渡
            if (fromIdx < toIdx)
            {
                for (int i = fromIdx; i <= toIdx; i++)
                {
                    stages.Add(stagesListCoyp[i]);
                }
            }
            else if (fromIdx > toIdx)
            {
                // from 在 to 阶段的 后面, 表示 从后往前 的 阶段 回溯
                for (int i = fromIdx; i >= toIdx; i--)
                {
                    stages.Add(stagesListCoyp[i]);
                }
            }
            // 如果是 相等的阶段, 那其实啥都不用干

            // stages.KJoin

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 计算恢复的阶段 start: {fromStageID}_{fromLoopIdx} ---> {toStageID}_{toLoopIdx}");
            return stages;
        }

        #endregion



        #region 黑洞效果
        /// 2023/9/21 
        /// 1. 摇杆输入 采用 还是之前的 坐标同步协议， 里面传坐标和朝向.  同时 设置 模拟表示 为 true 给服务器
        ///
        /// 2. 服务器 开启黑洞效果后， 会将 黑板效果的目标存入 效果黑板中。同时，将玩家的 原子状态 设置为 黑洞状态.
        ///
        /// 3. 客户端 在收到这个 黑洞效果后，立即同步一次坐标(到时候看表现), 本地记录 这个 黑洞的 作用目标， 从而确定 实体 被 哪些黑洞影响；
        ///
        /// 4.服务器 黑洞效果开启后，如果收到了 客户端 普通的坐标同步， 那么服务器会做补偿，认为这些坐标 是 在黑洞效果期间收到的。
        ///   客户端 也是上面的逻辑.
        /// 
        /// 5.第三人同步 后面再说


        #endregion

    }


}
