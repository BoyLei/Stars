using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Time;
using SkillEditor;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.TypeEffect;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 由 服务器 控制阶段创建 的entity的base
    /// </summary>
    public class ServerControlStageEntityBase : EntityRemoteStatic
    {
        protected string TagFlag = "";

        public ulong RuntimeID;

        public E_EntityState state = E_EntityState.None;

        /// <summary>
        /// 此处 阶段的创建和销毁完全依赖服务器控制,所以 IsRunning 由 是否 ServerClose 决定.
        /// 同时, IsRunning 是 virtual 类型, 可以根据不同子类 自定义 IsRunning 的实现.
        /// </summary>
        public virtual bool IsRunning => (state != E_EntityState.None) && (state & E_EntityState.ServerClose) != E_EntityState.ServerClose;

        protected BaseConfigInfo baseConfigInfo;

        protected ulong _builderID = 0;
        public ulong BuilderID => _builderID;

        protected ulong _ownerID = 0;
        public ulong OwnerID => _ownerID;


        /// <summary>
        /// 原子状态 是否已经注册的记录
        /// note:
        ///     防止被多次注册或者多次取消
        /// </summary>
        private bool isRegisterStates = false;

        private List<SkillStage> waitRemoveSkillStages = new List<SkillStage>();


        public BaseBlackBoard EntityBlackBoard = new BaseBlackBoard();

        protected StageHandle stageHandle;

        public StageHandle StageHandle => stageHandle;

        private SkillStage curStage;
        public SkillStage CurStage => curStage;

        private List<SkillStage> normalStages = new List<SkillStage>();
        private List<SkillStage> otherStages = new List<SkillStage>();
        private List<SkillStage> bulletStages = new List<SkillStage>();

        public Action<string, List<int>, bool> ActionOnRefreshStates;

        /// <summary>
        /// 技能播放动画接口
        /// <I_AnimParam, bool>  -----><SkillEntity, I_AnimParam, isFrameStart>
        /// </summary>
        public Action<SkillStage, I_AnimParam, bool> ActionOnPlayAnim;

        public Action<I_FxParam> ActionOnPlayFx;

        public Action<I_AudioParam> ActionOnPlayAudio;

        public Action<I_FxParam> ActionOnStopFx;

        /// <summary>
        /// 技能播放 客户端计算的效果线逻辑,每个效果在执行的时候,需要返回这个客户端效果自己执行是否成功
        /// <E_PlayEffectType,runtimeID, I_EffectParam, BaseBlackBoard, bool, bool ,ulong, ulong> 
        /// 对应:
        /// <E_PlayEffectType,runtimeID, I_EffectParam, BaseBlackBoard, isFrameStart, result, builder,owener>
        /// </summary>
        public Func<E_StageType, ulong, I_EffectParam, BaseBlackBoard, bool, ulong, ulong, bool> FuncOnTryPlayClientEffect;

        /// <summary>
        /// 技能注册 服务器效果线 的action，对于技能中的所有效果(包含所有Next效果),此处都会注册
        /// note:
        ///     由于服务器和客户端的计算可能不一致,所以客户端 会注册所有的服务器效果,等待服务器的效果数据.
        ///     从而兼容服务器和客户端计算不一致的问题
        /// </summary>
        public Func<E_StageType, ulong, I_EffectParam, CustomBlackBoardNode, BaseBlackBoard, bool> FuncOnTryPlayServerEffect;

        public void Create(ulong runtimeID, ulong ownerID, ulong builderID)
        {
            RuntimeID = runtimeID;
            _ownerID = ownerID;
            _builderID = builderID;

            state = E_EntityState.Running;

            WriteOwner(ownerID, builderID);
        }

        public void WriteOwner(ulong ownerID, ulong buildID)
        {
            BlackBoardNode Owner = new BlackBoardNode();
            Owner.Key = "Owner";
            Owner.Uint64Value = ownerID;
            WriteServerBlackBoard(Owner);

            BlackBoardNode Builder = new BlackBoardNode();
            Builder.Key = "Builder";
            Builder.Uint64Value = buildID;
            WriteServerBlackBoard(Builder);

        }


        protected void SetStageHandle(StageHandle handle)
        {
            stageHandle = handle;
            stageHandle.Init(EntityBlackBoard, RuntimeID, BuilderID, OwnerID);
            RegisterAction();
        }

        private void RegisterAction()
        {
            stageHandle.ActionOnPlayAnim = ActionOnPlayAnim;
            stageHandle.ActionOnPlayFx = ActionOnPlayFx;
            stageHandle.ActionOnPlayAudio = ActionOnPlayAudio;      //目前假设音频一个事件对应多个音频的制作方式
            stageHandle.ActionOnRefreshStates = ActionOnRefreshStates;

            stageHandle.FuncOnTryPlayClientEffect = FuncOnTryPlayClientEffect;
            stageHandle.FuncOnTryPlayServerEffect = FuncOnTryPlayServerEffect;
            stageHandle.ActionOnStopFx = ActionOnStopFx;
            stageHandle.ActionOnExitStage = OnAtionExitStage;
        }

        protected void ExecuteStageStates(List<int> states, bool regist)
        {
            if (states == null || states.Count == 0)
            {
                // SGF.Debuger.Log($"{TagFlag} HandleStageStates  stage {stage}  states null");
                return;
            }
            //为了防止多次注册或者多次取消,采用了异或的方式,决定是否需要执行 注册/取消
            //具体的逻辑如下:
            //      regist    isRegisterStates      是否需要执行
            //         T            T                   F
            //         T            F                   T
            //         F            T                   T
            //         F            F                   F
            bool needExecuteRegist = isRegisterStates ^ regist;
            // SGF.Debuger.Log($"{TagFlag} {StageIDStr} ExecuteStageStates  regist {regist}  needExecuteRegist {needExecuteRegist}");

            if (needExecuteRegist)
            {
                isRegisterStates = regist;
                ActionOnRefreshStates?.Invoke(TagFlag, states, regist);
            }
        }

        internal override void EnterFrame()
        {
            base.EnterFrame();

            EnterFrameStages(normalStages);
            EnterFrameStages(otherStages);
            EnterFrameStages(bulletStages);
        }

        private void EnterFrameStages(List<SkillStage> stages)
        {
            ReleaseWaitRemoveSkillStages(stages);

            stages.ForEach((SkillStage stage) =>
            {
                stage.OnUpdate();
            });
        }

        /// <summary>
        /// 当收到服务器阶段 同步时,去创建对应的阶段.
        /// 服务器目前保证 stageCreate时，也会给客户端发送 RunStageRet。
        /// </summary>
        /// <param name="runStageRet"></param>
        public void OnRunStageRet(RunStageRet runStageRet)
        {
            int stageId = runStageRet.StageID;
            int loopIdx = runStageRet.StageLoop;

            // string stageIdStr = SkillStage.FormatStageIDStr(stageId, loopIdx);
            //DB_Close       SGF.Debuger.Log($"[server] {TagFlag} [---xxx--] OnRunStageRet  [{stageIdStr}]");

            SkillStage stage = GetSkillStage(stageId, loopIdx);

            if (stage == null)
            {
                //DB_Close    SGF.Debuger.LogError($"[server] {TagFlag}  OnRunStageRet new Create stage [{stageIdStr}]  error!!!]");
                return;
            }
            // 每次服务器同步一个新的阶段阶段过来的时候, 客户端记录一下当前的阶段是哪个
            // 对客户端来说, 客户端会同时跑多个阶段(服务器由于网络快慢，客户端需要保存多个阶段以应付 网络来的过快 上个阶段还没关闭的问题)
            curStage = stage;

            // 阶段的同步和创建
            stage.OnServerRunStageRet(runStageRet);
        }

        public void OnRuntimeSync(RuntimeSyncRet runtimeSyncRet)
        {
            RepeatedField<BlackBoardNode> blackList = runtimeSyncRet.BlackList;
            HandleBlackList(blackList, "RuntimeSyncRet", false);
        }

        protected long GetStartTime()
        {
            string key = "StartTime";
            bool containStartTime = EntityBlackBoard.Contain(key, E_BlackBoardTag.Server);
            if (!containStartTime)
            {
                //DB_Close       SGF.Debuger.Log($"{TagFlag} : [StartTime] not find error!!!");
                return 0;
            }
            CustomBlackBoardNode customBlackBoardNode = EntityBlackBoard.GetKey<CustomBlackBoardNode>(key);

            return (long)customBlackBoardNode.Value;

        }

        protected void PlayCreateLoopEffects(List<FXJson> loopEffectFxs, bool isCreate)
        {

            for (int i = 0; i < loopEffectFxs.Count; i++)
            {
                FXJson loopEffectFx = loopEffectFxs[i];
                PlayCreateLoopEffect(loopEffectFx, isCreate, i);
            }

        }

        protected void PlayCreateLoopEffect(FXJson loopEffectFx, bool isCreate, int idx)
        {
            if (loopEffectFx == null)
            {
                return;
            }

            FxParam fxParam = new FxParam();
            fxParam.InitWithFxJson(loopEffectFx, BuilderID, OwnerID);
            fxParam.SetExtralKey($"{RuntimeID.ToString()}_loopEffectFx_{idx}");

            if (isCreate)
            {
                // SGF.Debuger.LogError($"{TagFlag} PlayCreateLoopEffect play : {loopEffectFx.EffectName}");

                ActionOnPlayFx.Invoke(fxParam);
            }
            else
            {
                // SGF.Debuger.LogError($"{TagFlag} PlayCreateLoopEffect stop : {loopEffectFx.EffectName}");

                ActionOnStopFx?.Invoke(fxParam);
            }
        }
        private CustomBlackBoardNode WriteServerBlackBoard(BlackBoardNode item)
        {
            CustomBlackBoardNode customBlackBoardNode = BaseBlackBoard.InitServerCustomBlackBoardNode(item.Key, item, BuilderID, OwnerID);

            if (customBlackBoardNode != null)
            {
                string key = item.Key;
                // 写入技能黑板数据(此处是服务器黑板数据同步过来,所以直接存入阶段黑板)
                // 同时,服务器同步的数据,写入服务器黑板数据块
                if (EntityBlackBoard == null)
                {
                    //DB_Close     SGF.Debuger.LogError($"{TagFlag} WriteServerBlackBoard key={item.Key} , buffBlackBoard=null");
                }
                EntityBlackBoard?.Set(key, customBlackBoardNode, E_BlackBoardTag.Server);
            }

            return customBlackBoardNode;
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
                CustomBlackBoardNode customBlackBoardNode = WriteServerBlackBoard(item);
                if (customBlackBoardNode != null)
                {
                    tempCustomBlackBoardNodeList.Add(customBlackBoardNode);
                }
            }

            EffectUtils.SortServerCustomBlackList(tempCustomBlackBoardNodeList, EntityBlackBoard);

            tempCustomBlackBoardNodeList.ForEach((CustomBlackBoardNode customBlackBoardNode) =>
            {
                HandleItemBlackBoard(customBlackBoardNode, isRecover, tag);
            });

            tempCustomBlackBoardNodeList.Clear();
        }

        public virtual void HandleItemBlackBoard(CustomBlackBoardNode customBlackBoardNode, bool isRecover, string tag)
        {
            string key = customBlackBoardNode.Key;
            //DB_Close       SGF.Debuger.Log($"{TagFlag}[server] {tag} HandleItemBlackBoard key : {key} , isRecover {isRecover}");


            EntityBlackBoard.Set(key, customBlackBoardNode, E_BlackBoardTag.Server);
            if (!isRecover)
            {
                stageHandle.TryPlayRegistedServerEffect(customBlackBoardNode, EntityBlackBoard);
            }
        }

        private SkillStage GetSkillStage(int stageId, int loopIdx)
        {
            SkillStage stage = FindStage(stageId, loopIdx);
            // 如果找不到,由于客户端是提前创建 otherStage,
            // 所以找不到的一定是 由服务器开启的 normalStage
            if (stage == null)
            {
                stage = CreateStage(stageId, loopIdx);
            }
            return stage;
        }

        private SkillStage FindStageInTarget(List<SkillStage> target, int stageId, int loopIdx)
        {
            SkillStage skillStage = target.Find(
                (SkillStage stage) =>
                {
                    return stage.EqualStageID(stageId, loopIdx);
                });

            return skillStage;
        }

        private SkillStage FindStage(int stageId, int loopIdx)
        {
            SkillStage skillStage = FindStageInTarget(normalStages, stageId, loopIdx);

            if (skillStage != null)
            {
                return skillStage;
            }

            skillStage = FindStageInTarget(otherStages, stageId, loopIdx);

            if (skillStage != null)
            {
                return skillStage;
            }

            skillStage = FindStageInTarget(bulletStages, stageId, loopIdx);

            return skillStage;
        }

        private SkillStage CreateStage(int stageId, int loopIdx)
        {
            if (baseConfigInfo == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} stageId={stageId},loopIdx={loopIdx}");
                return null;
            }
            StageJson stageJson = baseConfigInfo.GetStageJson(stageId);
            if (stageJson == null)
            {
                //DB_Close    ($"{TagFlag} CreateStage: [{stageId}_{loopIdx}] not find error!!!");
                return null;
            }
            return CreateStage(stageJson, loopIdx);
        }

        private SkillStage CreateStage(StageJson stageJson, int loopIdx)
        {
            SkillStage stage = null;
            TimeLineStage timeLineStage = baseConfigInfo.GetTimeLineStage(stageJson);
            if (timeLineStage != null)
            {
                StageInfo stageInfo = new StageInfo(stageJson, timeLineStage, 0, stageHandle.PlayStageType);

                stage = InitSkillStage(stageInfo, loopIdx);
                switch (stageJson.StageType)
                {
                    case StageType.NormalStage:
                        {
                            AddSkillStage(normalStages, stage);
                        }
                        break;
                    case StageType.BulletStage:
                        {
                            AddSkillStage(bulletStages, stage);
                        }
                        break;
                    case StageType.AddBuffStage:
                        {
                            AddSkillStage(otherStages, stage);
                        }
                        break;
                    case StageType.EndBuffStage:
                        {
                            // SGF.Debuger.LogError($"[BuffEndRet] 收到结束阶段通知");

                            AddSkillStage(otherStages, stage);
                        }
                        break;
                    case StageType.TriggerStage:
                        {
                            AddSkillStage(otherStages, stage);
                        }
                        break;
                    default:
                        {
                            //DB_Close    SGF.Debuger.LogError($"{TagFlag} CreateStage [{stage.StageIDStr}] StageType : {stageJson.StageType} no handle!!! ");
                        }
                        break;
                }
                // SGF.Debuger.LogError($"{TagFlag} CreateStage [{stage.StageIDStr}] StageType : {stageJson.StageType} ");
            }
            return stage;
        }

        private void AddSkillStage(List<SkillStage> stages, SkillStage stage)
        {
            int idx = stages.FindIndex((SkillStage skillStage) =>
                     {
                         return skillStage.StageIDStr == stage.StageIDStr;
                     });
            // 如果存在相同的 阶段, 那说明服加阶段重复了
            if (idx != -1)
            {
                //SGF.Debuger.LogError($"{TagFlag}  add stage[{stage.StageIDStr}] , type : {stage.curStageInfo.StageInfoType} repeate error!!!");
            }
            else
            {
                // SGF.Debuger.LogError($"[---xxxx--] Add new {stage.StageIDStr} ,idx {stages.Count} , code {stage.GetHashCode()}");
                stages.Add(stage);
            }

        }

        private SkillStage InitSkillStage(StageInfo stageInfo, int loopIdx)
        {
            SkillStage skillStage = EntityFactory.InstanceEntity<SkillStage>();

            string tagStr = TagFlag;
            skillStage.Init(stageInfo, loopIdx, tagStr, BuilderID, OwnerID, EntityBlackBoard);

            stageHandle.RegisterStage(skillStage);

            return skillStage;
        }


        /// <summary>

        /// 2022/12/13
        /// 目前跟 夏哥沟通, 对于buff/被动/ 子弹这种 由服务器控制 阶段开始的 实体, 阶段的结束由客户端自己控制.
        /// 
        /// note:
        ///     1.原本的阶段思路是 客户端阶段结束,但不销毁, 这样，哪怕阶段结束了, 服务器通知效果后,也可以执行;
        ///     2.改为 阶段结束即 release 后，发现在阶段结束后，还是会经常收到 阶段同步， 立即销毁此种方式不可行.
        ///     
        ///     3.最终思路改为 阶段结束,只修改 阶段状态, 将阶段的状态标记为 ReadyRelease 状态，在阶段的数量超过一定次数的时候,
        ///       释放掉 其中 ReadyRelease 的阶段
        /// 
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="exitStageTime"></param>
        /// <param name="skillStageExitType"></param>
        private void OnAtionExitStage(SkillStage skillStage, double exitStageTime, E_SkillStageExitType skillStageExitType)
        {
            bool canReleaseStage = CheckSkillStageCanRelease(skillStageExitType);
            // SGF.Debuger.LogError($"{TagFlag} OnActionExitStage : stage[{skillStage.StageIDStr}] , exitStageTime : {exitStageTime} , skillStageExitType : {skillStageExitType} , canReleaseStage : [{canReleaseStage}]");
            if (!CheckSkillStageCanRelease(skillStageExitType))
            {
                return;
            }

            AddWaitRemoveSkillStage(skillStage);
        }

        private bool CheckSkillStageCanRelease(E_SkillStageExitType skillStageExitType)
        {
            bool canReleaseStage = false;
            switch (skillStageExitType)
            {
                case E_SkillStageExitType.Default:
                    {
                        // 如果是默认的 阶段时间走完的退出逻辑，阶段只是不需要再去执行它的事件帧,但是串行的效果线还需要执行.
                        // 所以此时, 不需要删除这个阶段
                        canReleaseStage = false;
                    }
                    break;
                case E_SkillStageExitType.Recover:
                    {
                        // 如果是 技能阶段恢复创建的阶段, 阶段 可以依旧保留，只是 不执行它的阶段帧
                        // note:
                        //     阶段的恢复应该只有技能才需要完整的从头往后跑一边,而其它的子弹/buff/被动,都是由服务器控制 阶段的创建,
                        //     阶段 效果同步的时候，创建对应的阶段.  阶段的效果 依据的是 是否超过阶段的最大时间
                        canReleaseStage = false;
                    }
                    break;
                case E_SkillStageExitType.Broken:
                case E_SkillStageExitType.FailedSetMainSkill:
                case E_SkillStageExitType.OverMaxStageTime:
                    {
                        // 如果是 阶段被打断/ 阶段抢占失败而结束/ 阶段超过最大的时间, 
                        // 此时, 阶段 可以直接结束
                        canReleaseStage = true;
                    }
                    break;

                default:
                    {
                        // 默认可以 释放 阶段
                        canReleaseStage = true;
                    }
                    break;
            }
            return canReleaseStage;
        }

        private void AddWaitRemoveSkillStage(SkillStage releaseStage)
        {
            // 如果是 endBuff 阶段， endbuff阶段 服务器是瞬间执行，而客户端时一个持续过程. 
            // 如果 如果时buff的时候, buff的实体释放 需要等到 endbuff 阶段结束才可以释放
            if (releaseStage.curStageInfo.StageInfoType == StageType.EndBuffStage)
            {
                state |= E_EntityState.ClientClose;
            }
            waitRemoveSkillStages.Add(releaseStage);
            releaseStage.ReadyRelease = true;
            // SGF.Debuger.LogError($"{TagFlag} Add remove {releaseStage.StageIDStr} , code {releaseStage.GetHashCode()}");
        }



        private void ReleaseWaitRemoveSkillStages(List<SkillStage> stages)
        {
            if (stages.Count == 0)
            {
                return;
            }
            for (int i = waitRemoveSkillStages.Count - 1; i >= 0; i--)
            {
                SkillStage stage = waitRemoveSkillStages[i];
                int idx = stages.IndexOf(stage);
                if (-1 != idx)
                {
                    stages.Remove(stage);
                    waitRemoveSkillStages.Remove(stage);

                    ReleaseSkillStage(stage);
                }
            }
        }

        private void ReleaseSkillStage(SkillStage stage)
        {
            // SGF.Debuger.LogError($"{TagFlag} release {stage.StageIDStr} , type : {stage.curStageInfo.StageInfoType}");
            // 当前阶段
            if (curStage == stage)
            {
                curStage = null;
            }
            stageHandle.UnRegisterStage(stage);
            EntityFactory.ReleaseEntity(stage);
        }

        private void ReleaseSkillStages(List<SkillStage> stages)
        {
            // 先释放 stages中已经在 等待删除队列中的阶段
            ReleaseWaitRemoveSkillStages(stages);

            // 然后将 stages中 剩余的阶段一起释放
            stages.ForEach((SkillStage stage) =>
            {
                ReleaseSkillStage(stage);
            });
            stages.Clear();


        }

        /// <summary>
        /// 策划配置的 标签效果, 以前是只有buff 独有, 后面改为 子弹/被动/buff 均需要有
        /// </summary>
        private List<BaseTypeEffect> _globalEffects = new List<BaseTypeEffect>();

        /// <summary>
        /// 播放 配置的效果标签 特效: 比如冰冻特效的冰冻、影子buff的 影子等
        /// </summary>
        public void PlayEffects(List<GlobalShowSerialize> effectTypes, int CfgID)
        {
            _globalEffects.Clear();


            effectTypes.ForEach((GlobalShowSerialize effectType) =>
            {
                BaseTypeEffect effect = TypeEffectFactory.Create(effectType);
                if (effect != null)
                {
                    _globalEffects.Add(effect);
                }
            });

            if (_globalEffects != null && _globalEffects.Count > 0)
            {
                foreach (BaseTypeEffect effect in _globalEffects)
                {
                    effect.InitBlackBoard(RuntimeID, BuilderID, EntityBlackBoard);
                    effect.OnEnter(_ownerID, CfgID);
                }
            }
        }

        public void StopEffects()
        {
            if (_globalEffects != null && _globalEffects.Count > 0)
            {
                foreach (BaseTypeEffect effect in _globalEffects)
                {
                    effect.OnExit();
                }
                _globalEffects.Clear();
            }
        }

        protected virtual void ResetAction()
        {
            ActionOnPlayAnim = null;
            ActionOnPlayFx = null;
            ActionOnPlayAudio = null;
            ActionOnStopFx = null;

            FuncOnTryPlayClientEffect = null;
            FuncOnTryPlayServerEffect = null;
        }

        protected virtual void Reset()
        {
            RuntimeID = 0;
            baseConfigInfo = null;
            _builderID = 0;
            _ownerID = 0;

            isRegisterStates = false;

            state = E_EntityState.None;

            EntityBlackBoard.Clear();

            stageHandle?.Reset();


        }

        protected override void Release()
        {
            base.Release();

            StopEffects();
            // SGF.Debuger.LogError($"{TagFlag} release !!!");

            // 先释放 对应的阶段, 阶段释放过程中, 会处理对应的事件帧逻辑, 所以 在reset 之前执行
            {
                ReleaseSkillStages(normalStages);
                ReleaseSkillStages(otherStages);
                ReleaseSkillStages(bulletStages);
            }



            Reset();

            waitRemoveSkillStages.Clear();
            _globalEffects.Clear();

            EntityFactory.ReleaseEntity(stageHandle);

            ResetAction();
        }

    }
}
