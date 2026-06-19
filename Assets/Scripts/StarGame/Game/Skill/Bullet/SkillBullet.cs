using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Time;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProjectDef;
using UnityEngine;
using System;
using StarProject.Game.Entity.View.VitalSign.State;
using Sirenix.Utilities;
using StarProject.Game.TypeEffect;

namespace StarProject.Game.Skill
{
    /// 2023/07/19
    /// 1.跟 高磊 沟通, 子弹由 之前的 简单子弹 改为召唤物实现的原因:
    ///     子弹需要有 能够播放技能的能力. 所以 子弹其实就是策划认为的召唤物.
    /// 
    /// 2.子弹抛物线 等客户端模拟的纯客户端 表现, 需要采用 模型的高度. 
    ///   同时, gl 还想实现类似 炸弹人 q 的效果, 那么 就需要表现层 能够记住上一次 抛物线弹射的落点y=0 的情况.
    /// 
    /// 
    /// 
    /// ==================================================================
    /// <summary>
    /// 子弹的 实体层. 技能的阶段处理跟buff 类似,阶段的创建由服务器控制，
    /// 客户端只需要处理 每个阶段中间的事件帧的处理
    /// 
    /// note1: 
    ///     子弹的流程分为子弹 阶段流程 和 一般阶段流程;
    ///     一般阶段流程,由子弹阶段 进入时指定stageID 来开启。
    ///     子弹流程 中会配置 子弹目标效果
    /// 
    /// note2:
    ///     子弹的特效 一般是从第一个阶段就开始播, 那么其他人AOI同步时,
    ///     就会有个问题: 特效要不要从头开始播????
    ///     
    ///     目前的处理方案是 对于子弹的特效,从头开始播特效,如果不从头播,那其他人会看不到子弹
    /// </summary>
    public class SkillBullet : EntityRemoteStatic
    {
        private string TagFlag
        {
            get => $"[{OwnerEntityID}] [Bullet_{BulletID}] runtimeID : {RuntimeID} ";
        }
        public ulong RuntimeID;
        public int BulletID;
        public int RuntimeLv;

        private BaseBlackBoard BulletBlackBoard = new BaseBlackBoard();

        public E_EntityState state = E_EntityState.None;

        // 子弹的关闭完全依赖服务器,不想普通技能那样 还需要依赖客户端
        public bool IsRunning => (state != E_EntityState.None) && (state & E_EntityState.ServerClose) != E_EntityState.ServerClose;

        private BulletInfo bulletInfo;

        /// <summary>
        /// bullet普通阶段
        /// </summary>
        private List<SkillStage> normalStages = new List<SkillStage>();
        private List<SkillStage> otherStages = new List<SkillStage>();
        private List<SkillStage> bulletStages = new List<SkillStage>();

        private ulong _builderID = 0;
        public ulong BuilderID => _builderID;

        private ulong _ownerEntityID = 0;
        public ulong OwnerEntityID => _ownerEntityID;

        // private ulong _originalMaster = 0;

        // public ulong OriginalMaster => _originalMaster;

        // 开始时间
        private long startTime = 0;

        private StageHandle stageHandle;

        public StageHandle StageHandle => stageHandle;

        /// <summary>
        /// 技能播放动画接口
        /// <I_AnimParam, bool>  -----><SkillEntity, I_AnimParam, isFrameStart>
        /// </summary>
        public Action<SkillStage, I_AnimParam, bool> ActionOnPlayAnim;

        public Action<I_FxParam> ActionOnPlayFx;

        public Action<I_AudioParam> ActionOnPlayAudio;

        public Action<I_FxParam> ActionOnStopFx;

        public Action<float> ActionOnBulletOffectY;

        public Action<bool> ActionOnHidden;


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

        private bool hasCreate = false;
        public void Create(BulletCreateRet bulletCreateRet)
        {
            /// 2023/3/16
            /// 子弹 创建 协议 服务器 目前会 发两次, 一次 子弹 运行时 的创建
            /// 一次 是 子弹 进入 AOI 的 创建通知.
            /// 此处 增加一个 变量 来 避免 客户端 重复创建两次 子弹特效

            if (hasCreate)
            {
                return;
            }
            // SGF.Debuger.LogWarning($"[server] {TagFlag}  bulletCreateRet {bulletCreateRet}");

            hasCreate = true;
            state = E_EntityState.Running;
            RuntimeID = bulletCreateRet.RuntimeID;
            BulletID = bulletCreateRet.BulletID;
            RuntimeLv = bulletCreateRet.RuntimeLv > 0 ? bulletCreateRet.RuntimeLv : 1;
            _ownerEntityID = bulletCreateRet.OwnerEntityID;
            _builderID = bulletCreateRet.BuilderID;
            WriteOwner(OwnerEntityID, BuilderID);

            CreateStageHandle();

            CreateBulletInfo();

            startTime = GetStartTime(bulletCreateRet);
            long now = SGF.Time.TimeUtils.ServerNowStampMilli;
            if (startTime == 0)
            {
                //DB_Close    SGF.Debuger.LogError($"[server] {TagFlag}   Create , not find startTime , set startTime {startTime} , eroor!!! ");
                startTime = now;
            }
            long costTime = now - startTime;
            int enterTime = Mathf.Abs((int)costTime);

            // 延迟的误差 目前设置的范围是 100ms, 超过100ms,即走阶段恢复逻辑
            bool isRecover = enterTime > GameConfig.SKILL_LAG_TIME ? true : false;

            // 如果是 子弹的 recover 恢复(第三方看到的子弹的恢复), 那 enterTime 就是计算出来的 时间
            // 如果是自己的 子弹, 那就是从 0开始创建
            enterTime = enterTime <= GameConfig.SKILL_LAG_TIME ? 0 : enterTime;

            //DB_Close       SGF.Debuger.Log($"[server] {TagFlag}   Create  startTime {startTime} , costTime {costTime} , enterTime {enterTime} , isRecover : {isRecover}");

            HandleBlackList(bulletCreateRet.BlackList, "bulletCreateRet", isRecover);

            PlayCreateLoopEffects(bulletInfo.LoopEffectFxs, true);
            ActionOnHidden?.Invoke(false);

            PlayEffects(bulletInfo.Cfg.GlobalShows, BulletID);
        }


        public void WriteOwner(ulong ownerID, ulong buildID)
        {
            BlackBoardNode Owner = new BlackBoardNode();
            Owner.Key = "Owner";
            Owner.Uint64Value = ownerID;
            HandleItemBlackBoard(Owner, true);

            BlackBoardNode Builder = new BlackBoardNode();
            Builder.Key = "Builder";
            Builder.Uint64Value = buildID;
            HandleItemBlackBoard(Builder, true);

        }

        public void OnBulletCreatCusBlackBoard(BaseBlackBoard baseBlackBoard)
        {

            baseBlackBoard.clientBackBord.ForEach(item =>
            {
                BulletBlackBoard?.Set(item.Key, item.Value, E_BlackBoardTag.Client, false, false);
            });


        }
        private void CreateStageHandle()
        {
            stageHandle = EntityFactory.InstanceEntity<BulletStageHandle>();
            stageHandle.Init(BulletBlackBoard, RuntimeID, BuilderID, OwnerEntityID);
            RegisterAction();
        }

        private void CreateBulletInfo()
        {
            bulletInfo = EntityFactory.InstanceEntity<BulletInfo>();
            bulletInfo.Init(BulletID, OnBulletLoadFinishCb);


        }

        private void OnBulletLoadFinishCb(BulletInfo bulletInfo)
        {
            // 设置子弹 的模型 高度
            ActionOnBulletOffectY?.Invoke(((float)bulletInfo.Cfg.Hight / 100.0f));
        }

        private void RegisterAction()
        {
            stageHandle.ActionOnPlayAnim = ActionOnPlayAnim;
            stageHandle.ActionOnPlayFx = ActionOnPlayFx;
            stageHandle.ActionOnPlayAudio = ActionOnPlayAudio;      //目前假设音频一个事件对应多个音频的制作方式

            stageHandle.FuncOnTryPlayClientEffect = FuncOnTryPlayClientEffect;
            stageHandle.FuncOnTryPlayServerEffect = FuncOnTryPlayServerEffect;
            stageHandle.ActionOnStopFx = ActionOnStopFx;
            stageHandle.ActionOnExitStage = OnAtionExitStage;
        }

        protected void PlayCreateLoopEffects(List<FXJson> loopEffectFxs, bool isCreate)
        {
            loopEffectFxs.ForEach((FXJson loopEffectFx) =>
            {
                PlayLoopEffect(loopEffectFx, isCreate);
            });
        }

        private void PlayLoopEffect(FXJson loopEffectFx, bool isCreate)
        {
            if (loopEffectFx == null)
            {
                return;
            }

            FxParam fxParam = new FxParam();
            fxParam.InitWithFxJson(loopEffectFx, BuilderID, OwnerEntityID);
            fxParam.SetExtralKey($"{RuntimeID.ToString()}_loopEffectFx");
            // fxParam.SetOriginalMaster(OriginalMaster);

            if (isCreate)
            {
                ActionOnPlayFx.Invoke(fxParam);
            }
            else
            {
                ActionOnStopFx?.Invoke(fxParam);
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

        private long GetStartTime(BulletCreateRet bulletCreateRet)
        {
            RepeatedField<BlackBoardNode> blackList = bulletCreateRet.BlackList;

            for (int i = 0; i < blackList.Count; i++)
            {
                if (blackList[i].Key == "StartTime")
                {
                    return blackList[i].Int64Value;
                }
            }
            //DB_Close       SGF.Debuger.Log($"{TagFlag} BulletCreateRet: [StartTime] not find error!!!");
            return 0;
        }

        public void OnBulletEndRet(BulletEndRet bulletEndRet)
        {
            // SGF.Debuger.LogWarning($"[server] {TagFlag}  OnBulletEndRet new {bulletEndRet}");

            RepeatedField<BlackBoardNode> blackList = bulletEndRet.BlackList;
            HandleBlackList(blackList, "BulletEndRet", false);
            StopEffects();
        }

        private void HandleBlackList(RepeatedField<BlackBoardNode> blackList, string tag, bool isRecover)
        {
            if (blackList != null && blackList.Count > 0)
            {

                foreach (BlackBoardNode item in blackList)
                {
                    //DB_Close       SGF.Debuger.Log($"[server] {TagFlag}  {tag} , key : {item.Key}");

                    HandleItemBlackBoard(item, isRecover);

                    // if (item.Key == "OriginalMaster")
                    // {
                    //     _originalMaster = (ulong)item.Int64Value;
                    //     SGF.Debuger.Log($"[server] 设置子弹 _originalMaster : {_originalMaster} , key : {item.Key}");
                    // }
                }
            }
        }


        public void OnBuffRuntimeSync(RuntimeSyncRet runtimeSyncRet)
        {
            RepeatedField<BlackBoardNode> blackList = runtimeSyncRet.BlackList;
            HandleBlackList(blackList, "RuntimeSyncRet", false);
        }

        public void OnRunStageRet(RunStageRet runStageRet)
        {
            int stageId = runStageRet.StageID;
            int loopIdx = runStageRet.StageLoop;

            // SGF.Debuger.LogWarning($"[server] {TagFlag}  OnRunStageRet {runStageRet}");

            SkillStage stage = GetSkillStage(stageId, loopIdx);

            if (stage == null)
            {
                SGF.Debuger.LogWarning($"[server] {TagFlag}  OnRunStageRet new Create stage [{SkillStage.FormatStageIDStr(stageId, loopIdx)}]  error!!!]");
                return;
            }
            stage.OnServerRunStageRet(runStageRet);
        }

        private void HandleItemBlackBoard(BlackBoardNode item, bool isRecover)
        {
            CustomBlackBoardNode customBlackBoardNode = BaseBlackBoard.InitServerCustomBlackBoardNode(item.Key, item, BuilderID, OwnerEntityID);
            //DB_Close       SGF.Debuger.Log($"[server] {TagFlag}  HandleItemBlackBoard key={item.Key} , isRecover {isRecover}");
            if (customBlackBoardNode == null)
            {
                return;
            }
            string key = item.Key;
            // 写入技能黑板数据(此处是服务器黑板数据同步过来,所以直接存入阶段黑板)
            // 同时,服务器同步的数据,写入服务器黑板数据块
            if (BulletBlackBoard == null)
            {
                //SGF.Debuger.LogError($"{TagFlag} HandleItemBlackBoard key={item.Key} , buffBlackBoard=null");
            }
            BulletBlackBoard?.Set(key, customBlackBoardNode, E_BlackBoardTag.Server);
            if (!isRecover)
            {
                stageHandle.TryPlayRegistedServerEffect(customBlackBoardNode, BulletBlackBoard);
            }
        }

        private SkillStage GetSkillStage(int stageId, int loopIdx)
        {
            SkillStage stage = FindStage(stageId, loopIdx);
            // 如果找不到,由于客户端是提前创建 otherStage,
            // 所以找不到的一定是 由服务器开启的 normalStage
            if (stage == null)
            {
                StageJson stageJson = bulletInfo.GetStageJson(stageId);
                if (stageJson == null)
                {
                    SGF.Debuger.LogWarning($"{TagFlag} GetSkillStage: [{stageId}_{loopIdx}] not find error!!!");
                    return null;
                }
                stage = CreateStage(stageJson, loopIdx);
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

            skillStage = FindStageInTarget(bulletStages, stageId, loopIdx);

            if (skillStage != null)
            {
                return skillStage;
            }


            skillStage = FindStageInTarget(otherStages, stageId, loopIdx);

            return skillStage;
        }

        private int tmpIdx = 0;
        private void AddSkillStage(List<SkillStage> stages, SkillStage stage)
        {
            tmpIdx = stages.FindIndex((SkillStage skillStage) =>
                     {
                         return skillStage.StageIDStr == stage.StageIDStr;
                     });
            // 如果存在相同的 阶段, 那说明服加阶段重复了
            if (tmpIdx != -1)
            {
                //SGF.Debuger.LogError($"{TagFlag} add stage: [{stage.StageIDStr}] , type : {stage.curStageInfo.StageInfoType} repeate error!!!");

                ReleaseSkillStage(stages[tmpIdx]);
                stages.RemoveAt(tmpIdx);
            }
            stages.Add(stage);
        }
        private SkillStage CreateStage(StageJson stageJson, int loopIdx)
        {
            SkillStage stage = null;
            TimeLineStage timeLineStage = bulletInfo.GetTimeLineStage(stageJson);
            if (timeLineStage != null)
            {
                StageInfo stageInfo = new StageInfo(stageJson, timeLineStage, 0, E_StageType.Bullet);

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
                    case StageType.TriggerStage:
                        {
                            AddSkillStage(otherStages, stage);
                        }
                        break;
                    default: break;
                }
                //DB_Close       SGF.Debuger.Log($"{TagFlag} CreateStage [{stage.StageIDStr}] StageType : {stageJson.StageType} ");
            }
            return stage;
        }

        private SkillStage InitSkillStage(StageInfo stageInfo, int loopIdx)
        {
            SkillStage skillStage = EntityFactory.InstanceEntity<SkillStage>();

            string tagStr = TagFlag;
            skillStage.Init(stageInfo, loopIdx, tagStr, BuilderID, OwnerEntityID, BulletBlackBoard);

            stageHandle.RegisterStage(skillStage);

            return skillStage;
        }
        private void ReleaseSkillStage(SkillStage stage)
        {
            stageHandle.UnRegisterStage(stage);
            EntityFactory.ReleaseEntity(stage);
        }

        private void ReleaseSkillStages(List<SkillStage> stages)
        {
            stages.ForEach((SkillStage stage) =>
            {
                ReleaseSkillStage(stage);
            });
            stages.Clear();

            ReleaseWaitRemoveSkillStages(stages);
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
            // AddWaitRemoveSkillStage(skillStage);
            // SGF.Debuger.LogError($"{TagFlag} [m-m] OnAtionExitStage stage[{skillStage.GetHashCode()}]: [{skillStage.StageIDStr}]");

            ExecuteReleaseStage(normalStages, skillStage);
            ExecuteReleaseStage(otherStages, skillStage);
            ExecuteReleaseStage(bulletStages, skillStage);

        }
        int MaxCount = 10;
        // 先 写死每次释放3个， 如果一次把所有的释放, 没必要，不释放其实就是为了等服务器
        int releaseCount = 3;

        List<SkillStage> releaseStageTemp = new List<SkillStage>();

        private void ExecuteReleaseStage(List<SkillStage> stages, SkillStage ignoreStage)
        {
            int stageCount = stages.Count;
            if (stageCount > MaxCount)
            {
                releaseStageTemp.Clear();
                for (int i = 0; i < stageCount; i++)
                {
                    SkillStage stage = stages[i];
                    if (stage.ReadyRelease && stage != ignoreStage && releaseStageTemp.Count <= releaseCount)
                    {
                        releaseStageTemp.Add(stage);
                        // AddWaitRemoveSkillStage(stage, stages.IndexOf(stage));
                    }
                }
                if (releaseStageTemp.Count > 0)
                {
                    releaseStageTemp.ForEach((SkillStage stage) =>
                    {
                        AddWaitRemoveSkillStage(stage, -1);
                    });
                    releaseStageTemp.Clear();
                }
            }
        }

        private List<SkillStage> waitRemoveSkillStages = new List<SkillStage>();

        private void AddWaitRemoveSkillStage(SkillStage skillStage, int idx)
        {
            waitRemoveSkillStages.Add(skillStage);
            // SGF.Debuger.LogError($"[---xxxx--] Add remove {skillStage.StageIDStr} ,idx {idx} , code {skillStage.GetHashCode()}");
            skillStage.ReadyRelease = true;
        }



        private void ReleaseWaitRemoveSkillStages(List<SkillStage> stages)
        {
            for (int i = waitRemoveSkillStages.Count - 1; i >= 0; i--)
            {
                SkillStage stage = waitRemoveSkillStages[i];
                int idx = stages.IndexOf(stage);
                if (-1 != idx)
                {
                    stages.Remove(stage);
                    waitRemoveSkillStages.Remove(stage);
                    // SGF.Debuger.LogError($"[---xxxx--] release {stage.StageIDStr} ,idx {idx} , code {stage.GetHashCode()}");
                }
            }
        }

        /// <summary>
        /// 策划配置的 标签效果, 以前是只有buff 独有, 后面改为 子弹/被动/buff 均需要有
        /// </summary>
        private List<BaseTypeEffect> _globalEffects = new List<BaseTypeEffect>();

        /// <summary>
        /// 播放buff 配置的 特效: 比如冰冻特效的冰冻、影子buff的 影子等
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
                    effect.InitBlackBoard(RuntimeID, BuilderID, BulletBlackBoard);
                    effect.OnEnter(OwnerEntityID, CfgID);
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

        protected override void Release()
        {


            PlayCreateLoopEffects(bulletInfo.LoopEffectFxs, false);

            hasCreate = false;
            RuntimeID = 0;
            BulletID = 0;
            RuntimeLv = 0;
            BulletBlackBoard.Clear();
            state = E_EntityState.None;

            EntityFactory.ReleaseEntity(bulletInfo);
            bulletInfo = null;

            ReleaseSkillStages(normalStages);
            ReleaseSkillStages(otherStages);
            ReleaseSkillStages(bulletStages);

            waitRemoveSkillStages.Clear();

            _builderID = 0;
            _ownerEntityID = 0;
            _globalEffects.Clear();

            EntityFactory.ReleaseEntity(stageHandle);

            base.Release();
        }

    }
}
