using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarProjectDef;
using StarProject.CustomDataStruct;
using SkillEditor;
using SGF.Unity;
using ProtoMsg;

using StarProject.Game.Entity.View.VitalSign.State;
using SGF.Network;
using SGF.Time;
using EffectData = SkillEditor.EffectData;


namespace StarProject.Game.Skill
{

    /// <summary>
    /// SkillEntity 的 Action 部分
    /// </summary>
    public partial class SkillEntity
    {


        /// <summary>
        /// 技能状态发生改变时,通知外面,参数为 curStage, lastStage,是否强制取消
        /// </summary>
        public Action<E_ULayerSubState, E_ULayerSubState, SkillEntity, bool, bool, E_ClientSkillEndType> ActionOnStageChange;


        /// <summary>
        /// 当技能的黑板数据下发时,执行的Action
        /// </summary>
        public Action<string, object> ActionOnBlackBord;

        /// <summary>
        /// 当蓄能开始,通知外面
        /// </summary>
        public Action<SkillEntity> ActionOnEnergyStart;

        /// <summary>
        /// 当蓄力层数发生改变时,通知外面(runnintTime)
        /// </summary>
        public Action<int, double> ActionOnEnergyCountChange;

        /// <summary>
        /// 当蓄能满了之后,通知外面
        /// </summary>
        public Action<SkillEntity> ActionOnEnergyFull;

        /// <summary>
        /// 当蓄力结束之后,通知外层。
        /// </summary>
        public Action<SkillEntity> ActionOnEnergyEnd;

        /// <summary>
        /// 技能 尝试设置自己为 主动技能的 Func 
        /// <SkillEntity, bool, bool> -----> <this , isMain , ifForce , result>
        /// </summary>
        public Func<SkillEntity, double, bool, bool, bool> FuncOnSkillTrySetActiveSkill;

        /// <summary>
        /// 技能结束的Action
        /// note:
        ///     技能skillExit 并不会直接恢复角色动画。
        ///     角色动画的控制 由技能的 主动阶段来控制,不像之前 由技能skillEntity控制
        /// </summary>
        public Action<SkillEntity, E_SkillExitType> ActionOnSkillExit;


        /// <summary>
        /// 原子状态刷新的接口
        /// </summary>
        public Action<string, List<int>, bool> ActionOnRefreshStates;


        /// <summary>
        /// 技能播放动画接口
        /// <I_AnimParam, bool>  -----><SkillEntity, I_AnimParam, isFrameStart>
        /// </summary>
        public Action<SkillStage, I_AnimParam, bool> ActionOnPlayAnim;

        public Action<I_FxParam> ActionOnPlayFx;

        public Action<I_AudioParam> ActionOnPlayAudio;

        public Action<I_AudioParam> ActionOnStopAudio;
        /// <summary>
        /// 技能播放 客户端计算的效果线逻辑,每个效果在执行的时候,需要返回这个客户端效果自己执行是否成功
        /// <E_PlayEffectType,runtimeID, I_EffectParam, BaseBlackBoard, builder,owener, isFrameStart,stageRecoverTime,isRecover, result>
        /// </summary>
        public Func<E_StageType, ulong, I_EffectParam, BaseBlackBoard, bool, ulong, ulong, int, bool, bool> FuncOnTryPlayClientEffect;

        /// <summary>
        ///  效果的 结束 逻辑
        /// <E_PlayEffectType,runtimeID, I_EffectParam, BaseBlackBoard, builder,owener,result>
        /// </summary>
        public Func<E_StageType, ulong, I_EffectParam, BaseBlackBoard, ulong, ulong, bool> FuncOnTryStopEffect;

        /// <summary>
        /// 技能注册 服务器效果线 的action，对于技能中的所有效果(包含所有Next效果),此处都会注册
        /// note:
        ///     由于服务器和客户端的计算可能不一致,所以客户端 会注册所有的服务器效果,等待服务器的效果数据.
        ///     从而兼容服务器和客户端计算不一致的问题
        /// </summary>
        public Func<E_StageType, ulong, I_EffectParam, CustomBlackBoardNode, BaseBlackBoard, int, bool> FuncOnTryPlayServerEffect;


        public Action<I_FxParam> ActionOnStopFx;

        public Action<SkillEntity, int, E_CameraEffectType> ActionOnPlayCamera;

        public Action<bool, CameraShakeJson> ActionOnPlayCameraShake;


        /// <summary>
        /// 同步技能使用时的 数据信息
        /// </summary>
        public Action<SyncSkillUseType> ActionOnSyncSkillUse;

        public Action<SkillEntity, SkillStage> ActionOnServerCreateStage;

        /// <summary>
        /// <int, bool, bool, bool, float>  ----> <朝向,同步客户端数值,同步服务器数值,同步view,最大转向时间>
        /// </summary>
        public Action<int, bool, bool, bool, float> ActionOnSyncPlayerViewRota;

        private void ReleaseAction()
        {
            ActionOnStageChange = null;
            ActionOnBlackBord = null;

            ActionOnEnergyStart = null;
            ActionOnEnergyCountChange = null;
            ActionOnEnergyFull = null;
            ActionOnEnergyEnd = null;

            FuncOnSkillTrySetActiveSkill = null;
            ActionOnSkillExit = null;
            ActionOnRefreshStates = null;
            ActionOnPlayAnim = null;
            ActionOnPlayFx = null;
            ActionOnPlayAudio = null;
            ActionOnStopAudio = null;
            FuncOnTryPlayClientEffect = null;
            FuncOnTryPlayServerEffect = null;
            FuncOnTryStopEffect = null;
            ActionOnStopFx = null;

            ActionOnPlayCamera = null;
            ActionOnPlayCameraShake = null;

            ActionOnSyncSkillUse = null;
            ActionOnServerCreateStage = null;

            ActionOnSyncPlayerViewRota = null;
        }

        private void OnExitOtherStage(SkillStage skillStage)
        {
            {
                // note: 不需要 立即从 serverCreateOtherStages 中移除, 在技能release的时候, 会统一 release 所有阶段
                // serverCreateOtherStages.Remove(skillStage);
            }

            executedStages.Enqueue(skillStage);
        }

        /// <summary>
        /// 技能阶段退出接口
        /// </summary>
        /// <param name="exitStageTime"></param>
        /// <param name="skillStageExitType"></param>
        public void OnActionExitStage(SkillStage skillStage, double exitStageTime, E_SkillStageExitType skillStageExitType)
        {
            // SGF.Debuger.LogError($"{TagFlag} [skillStage] 阶段退出: {skillStage.StageIDStr} , IsNormalStage: {skillStage.IsNormalStage} , skillStageExitType {skillStageExitType}");

            if (!skillStage.IsNormalStage)
            {
                OnExitOtherStage(skillStage);
                return;
            }

            // //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} OnActionExitStage skillStage:[{skillStage.StageIDStr}] skillStageExitType : {skillStageExitType}  ");

            switch (skillStageExitType)
            {
                case E_SkillStageExitType.Default:
                    {
                        //正常的时间结束,那就正常的进入下一阶段
                        EnterNextStage(skillStage, exitStageTime, E_SkillStageEnterType.Default);
                    }
                    break;
                case E_SkillStageExitType.FailedSetMainSkill:
                    {
                        //抢位技能失败,结束技能运行时
                        OnExit(E_SkillExitType.FailedSetMainSkill);
                    }
                    break;
                case E_SkillStageExitType.Recover:
                    {
                        //技能恢复过程中的阶段结束
                        //note:
                        //  只需要跳转到下一个阶段即可,OnEnter 交给上面 Recover的时候自己去处理
                        EndCurStageUpdate(true);
                    }
                    break;
                case E_SkillStageExitType.Broken:
                    {
                        //阶段被打断的结束逻辑
                        OnStageBroken(skillStage, exitStageTime);
                    }
                    break;
                case E_SkillStageExitType.OverMaxStageTime:
                    {
                        // 如果是 阶段 运行 超过了最大的运行线的时间, 那其实由外面去释放这个阶段
                        // 本地 暂时可以啥都不用干
                        //SGF.Debuger.LogError($"{TagFlag} OnActionExitStage skillStage:[{skillStage.StageIDStr}] skillStageExitType : {skillStageExitType} not release ");
                    }
                    break;
                default:
                    {
                        //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} OnActionExitStage skillStage:[{skillStage.StageIDStr}] skillStageExitType : {skillStageExitType} error!!!");
                    }
                    break;
            }
        }

        /// <summary>
        /// 阶段的打断接口,
        ///     需要区分 阶段配置的打断类型,是打断 当前技能运行时,还是 只打断当前阶段,跳转到对应的阶段
        /// note:
        ///     此处只是 打断阶段, 将当前的阶段 执行退出逻辑, 所以 此处并不会 进入下个阶段
        /// </summary>
        public void OnBreakStage(E_SkillStageExitType type)
        {
            CancelInvokeExecuteUserInput();
            BreakStage(CurSkillStage, type);
        }

        /// <summary>
        /// 设置 活跃 被打断的标志, 并不会 真正的打断阶段
        /// </summary>
        public void SetBreakActiveTag()
        {
            CancelInvokeExecuteUserInput();
            if (CurSkillStage == null)
            {
                return;
            }
            CurSkillStage.SetBreakActiveTag();
        }

        /// <summary>
        /// 打断 一个阶段, 只打断, 不做其它任何事
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="type"></param>
        public void BreakStage(SkillStage stage, E_SkillStageExitType type)
        {
            if (stage == null)
            {
                return;
            }
            ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} BreakStage {stage.StageIDStr} , type : {type} , needActive: {stage.NeedActive}");
            stage.ExitStage(type);

        }

        /// <summary>
        /// 打断当前阶段,进入下一个阶段
        /// </summary>
        /// <param name="type"></param>
        public void BreakCurStageToNext(E_SkillStageExitType type)
        {
            if (CurSkillStage == null)
            {
                return;
            }
            BreakStage(CurSkillStage, type);
            EndCurStageUpdate(true);
        }

        /// <summary>
        /// 技能阶段的尝试 设置自己为 主动阶段 时调用的接口
        /// </summary>
        /// <param name="isActiveMain">此技能阶段 需要设置 主动 还是取消主动</param>
        /// <param name="force">是否强制 设置活跃, 只有 服务器同步技能时,才会 出现强制设置 的情况</param>
        /// <returns></returns>
        public bool OnActionStageTrySetActiveMain(SkillStage skillStage, double enterStageTime, bool isActiveMain, bool force)
        {
            bool result = FuncOnSkillTrySetActiveSkill.Invoke(this, enterStageTime, isActiveMain, force);
            return result;
        }

        /// <summary>
        /// 阶段刷新 原子状态的接口
        /// </summary>
        /// <param name="stageID"></param>
        /// <param name="states"></param>
        /// <param name="regist"></param>
        public void OnActionRefreshStageStates(string stageID, List<int> states, bool regist)
        {
            ActionOnRefreshStates.Invoke(stageID, states, regist);
        }

        private void OnActionExitStartCD(E_SkillExitType exitType)
        {
            if (exitType == E_SkillExitType.Reset)
            {
                return;
            }
            skillContainer.ClientStartCD(cfg);
        }

        private void OnActionStageStartCD(SkillStage skillStage)
        {
            skillContainer.ClientStartCD(cfg);
        }

        /// <summary>
        /// 客户端 技能预播的时候, 开启技能配置的 技能槽的 CD
        /// </summary>
        private void StartContainerCD()
        {
            skillContainer.ClientStartContainerCD(cfg);
        }

        private bool CheckNeedExecuteStageFrame(SkillStage skillStage)
        {
            //note:
            //  如果自己是需要抢位技能,即active为true
            //  由于抢位逻辑的存在,当 收到阶段 playAni 的Action的时候,抢位一定是成功的
            //  抢位失败的 阶段 会被 打断阶段


            // 当技能阶段为 非抢占活跃阶段,且当前角色正在 摇杆移动的时候,不需要播放动作
            if (!skillStage.NeedActive && playerData.M_Is_Moveing)
            {
                return false;
            }

            return true;
        }

        AnimParam animParam = new AnimParam();

        /// <summary>
        /// 阶段动画能不能播,做逻辑判断
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="animationJson"></param>
        /// <param name="isFrameStart">是开始帧还是结束帧</param>
        private bool OnFuncStageTryPlayAnim(SkillStage skillStage, AnimationJson animationJson, bool isFrameStart, float startTime)
        {
            bool result = StagePlayAnim(skillStage, animationJson, isFrameStart, startTime);

            // 播放成功后，更新当前动画记录
            if (result)
            {
                UpdateCurAnimtionRecord(skillStage, animationJson, isFrameStart);
            }

            return result;
        }

        private bool StagePlayAnim(SkillStage skillStage, AnimationJson animationJson, bool isFrameStart, float startTime)
        {
            // 如果是开始帧,就是正常的播放动画的逻辑
            // 就需要有单独自己的动画播放逻辑判断
            if (isFrameStart)
            {
                if (!CheckNeedExecuteStageFrame(skillStage))
                {
                    return false;
                }
            }
#if (UNITY_EDITOR && BATTLE_DEBUG)
            string animUID = $"{animationJson.ClipName}_{skillStage.StageIDStr}";
            DebugAnimationData(animUID, isFrameStart, $"{animUID}_{isFrameStart}", new List<string>() { $"startTime: {startTime}" });
#endif
            animParam.InitAnimationJson(animationJson, E_StageType.Skill);
            // 动画的结束帧 可能会超过阶段,导致 结束帧 对不上 动画的开始帧
            animParam.SetExtraKey(RuntimeID.ToString());
            animParam.SetStartTime(startTime);
            ActionOnPlayAnim.Invoke(skillStage, animParam, isFrameStart);
            return true;
        }

        private void UpdateCurAnimtionRecord(SkillStage skillStage, AnimationJson animationJson, bool isFrameStart)
        {
            // SGF.Debuger.Log($"xxxx-- update record : {animationJson.ClipName} , isFrameStart {isFrameStart}");
            // 如果存在一个 动画的 首帧 记录
            if (curAnimationRecord != null && IsCurAnimationRecordFramStart)
            {
                // 如果 新的动画帧是首帧, 那直接更新 缓存帧记录

                // 如果 是尾帧, 需要比较尾帧 与首帧是否配对(因为存在动画直接有交错).
                //      只有首尾帧完全匹配,才需要清空这个记录
                if (!isFrameStart)
                {
                    if (curAnimationRecord == animationJson)
                    {
                        CleanAnimationRecord();
                    }
                    else
                    {
                        return;
                    }
                }
            }

            curAnimationRecordStage = skillStage;
            curAnimationRecord = animationJson;
            IsCurAnimationRecordFramStart = isFrameStart;
        }

        FxParam fxParam = new FxParam();

        private void OnActionStageTryPlayFx(SkillStage skillStage, FXJson fXJson, int fxStartTime)
        {
            // 特效：如果不跟随阶段循环。第一次执行，后面的直接return
            if (skillStage.curStageInfo.IsStageLoop && !fXJson.config.IsFollowLoop)
            {
                sb.Clear();
                var key = sb.Append(skillStage.StageID).Append("_").Append(fXJson.EffectName).ToString();
                if (_followLoopKeyRecord.Contains(key))
                {
                    return;
                }
                else
                {
                    _followLoopKeyRecord.Add(key);
                }
            }
#if (UNITY_EDITOR && BATTLE_DEBUG)
            string fxUID = $"{fXJson.EffectName}_{skillStage.StageIDStr}";
            DebugFxData(fxUID, true, $"{fxUID}_{true}", new List<string>() { $"fxStartTime: {fxStartTime}" });
#endif
            fxParam.InitWithFxJson(fXJson, BuilderID, OwnerEntityID);
            fxParam.SetExtralKey($"{RuntimeID.ToString()}_[{skillStage.StageIDStr}]");
            fxParam.SetFxStartTime(fxStartTime);

            ActionOnPlayFx.Invoke(fxParam);
        }

        AudioParam audioParam = new AudioParam();

        //层次开放太多了，层层可以多个
        private void OnActionStageTryPlaySound(SkillStage skillStage, SoundJson soundJson, int startTime)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            string audioUID = $"{soundJson.EventName}_{skillStage.StageIDStr}";
            DebugAudioData(audioUID, true, $"{audioUID}_{true}", new List<string>() { $"play: {soundJson.EventName}" });
#endif
            audioParam.Init(soundJson, startTime, 0);

            ActionOnPlayAudio.Invoke(audioParam);
        }

        private void OnActionStageTryStopSound(SkillStage skillStage, SoundJson soundJson)
        {
            audioParam.Init(soundJson, 0, 0);
            ActionOnStopAudio.Invoke(audioParam);
        }

        // EffectParam effectParam = new EffectParam();
        private void OnActionStageTryPlayEffect(SkillStage skillStage, EffectData effectData, bool isRecover, bool isFrameStart, int stageRecoverTime)
        {
            PlayStageEffect(skillStage, effectData, isRecover, isFrameStart, stageRecoverTime);
        }

        private void OnActionStageTryStopEffect(SkillStage skillStage, EffectData effectData)
        {
            StopStageEffect(skillStage, effectData);
        }

        /// <summary>
        /// 执行阶段播放效果的接口
        /// note:
        ///     客户端的效果处理逻辑理论上比服务器的效果逻辑要复杂一点(不包括服务器数据计算部分的话),
        ///     对于客户端的效果数据,客户端需要考虑以下几点:
        ///     1.对每个效果 需要区分 客户端表现的效果线 和 展示服务器效果数据的服务器效果线;
        ///     2.由于存在客户端提前计算的逻辑,如果兼容 服务器和客户端本地计算数据不一致的问题.
        ///         即如何处理 本地提前计算数据不一致,导致的 Next效果 不一致?
        ///     3.如何对已经执行的技能的效果线 恢复的问题.
        /// 
        /// note2:
        ///     此处需要注意：
        ///     Q:  为什么服务器效果线的 逻辑在SkillEntity中注册？
        ///     A:  因为效果中 可以配置 黑板数据是否写入技能黑板中,所以阶段的黑板数据,可以写入技能黑板同步给客户端;
        ///         如果将 服务器效果线注册逻辑 写入阶段中, 上述的服务器通知过来的技能黑板数据无法处理
        /// 
        /// note3:
        ///     Q: 为什么 此处 传入的是 stageRecoverTime 阶段的恢复时间, 而不是这个效果 它自己 需要 恢复的时间?
        ///     A: 因为 如果传入 的是 效果他自己的 恢复时间, 那么它 的Next 效果执行的时候, 恢复时间 就不知道 如何去计算, 所以传入 stageRecoverTime
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="effectData"></param>
        /// <param name="isRecover"> 是否是阶段复原的播放效果</param>
        /// <param name="isFrameStart"> 是否是播放效果的首帧</param>
        /// <param name="stageRecoverTime"> 阶段恢复的时间</param>
        private void PlayStageEffect(SkillStage skillStage, EffectData effectData, bool isRecover, bool isFrameStart, int stageRecoverTime)
        {
            EffectParam newEffectParam = CreateEffectParam(skillStage, effectData);

            TryPlayEffect(skillStage, newEffectParam, isRecover, isFrameStart, stageRecoverTime);

            // 效果的结束帧 一般都是走 特殊的结束逻辑 (比如输入轴的结束帧,有单独的处理逻辑)
            // 对于Next的效果,目前 是放在 效果的首帧处理
            if (!isFrameStart)
            {
                return;
            }

            StagePrePlayEffectNext(skillStage, newEffectParam, isRecover, stageRecoverTime);
        }

        private void StopStageEffect(SkillStage skillStage, EffectData effectData)
        {

            EffectParam newEffectParam = CreateEffectParam(skillStage, effectData);


            BaseBlackBoard baseBlackBoard = effectData.SaveSkill ? skillBlackBoard : skillStage.StageBlackBoard;

            FuncOnTryStopEffect?.Invoke(E_StageType.Skill, RuntimeID, newEffectParam, baseBlackBoard, BuilderID, OwnerEntityID);
        }

        /// <summary>
        /// 检查 是否 需要提前注册 Next 效果.
        /// note:
        ///     1.如果 是服务器线, 检查 是否 注册了 isWaitServerRegNext. 
        ///     2.如果 是客户端线, 目前 isWaitServerRegNext 默认 为false, 默认情况下，需要注册.
        ///       如果 isWaitServerRegNext 为true, 此时 客户端线不需要注册, 交给 服务器线 注册.
        ///       所以,他们之间的关系如下;
        ///            isServerLine | isWaitServerRegNext   PreRegPlayEffectNext
        ///                 True    |          True                 True
        ///                 True    |          False                False 
        ///                 False   |          True                 False 
        ///                 False   |          False                True 
        /// /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isServerLine">是否是 服务器线</param>
        private bool CheckNeedPreRgeNextEffect(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam, bool isServerLine)
        {

#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                // 对于 本地服来说, 客户端线都是先注册效果. 
                // 对于 IsNotEmpty 这种类型的判断效果,都会在执行的时候 即生成 客户端黑板数据.
                // 然后 根据黑板数据, 后面再去判断 走 Next_True 还是 Next_False 效果分支.
                return true;
            }
#endif

            bool isWaitServerRegNext = EffectResultUtils.IsWaitServerRegNext(baseBlackBoard, i_EffectParam);
            bool result = (isServerLine && isWaitServerRegNext) || (!isServerLine && !isWaitServerRegNext);

            return result;
        }

        /// <summary>
        /// 提前注册并播放 Next效果 
        /// note: 
        ///     2022/12/26 增加 一个判断类 效果 需要等待服务器 消息回来后 执行后面的逻辑
        ///     所以 PreRegPlayEffectNext 扩展为 不仅在客户端线 执行,同时 在 服务器线 也可以触发 next, 
        ///     通过 提前注册 IsWaitServerRegNext  来处理
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isRecover"></param>
        /// <param name="isServerLine">是否是 服务器线触发</param>
        /// <param name="stageRecoverTime">阶段的恢复时间</param>
        private void StagePrePlayEffectNext(SkillStage skillStage, I_EffectParam i_EffectParam, bool isRecover, int stageRecoverTime)
        {
            List<int> next = i_EffectParam.Next;
            if (next == null) return;
            BaseBlackBoard baseBlackBoard = i_EffectParam.SaveSkill ? (BaseBlackBoard)skillBlackBoard : skillStage.StageBlackBoard;

            bool needPreRegNextEffect = CheckNeedPreRgeNextEffect(baseBlackBoard, i_EffectParam, false);
            if (!needPreRegNextEffect)
            {
                return;
            }

            PrePlayNextEffect(skillStage, i_EffectParam, isRecover, stageRecoverTime);
        }

        /// <summary>
        /// 提前 播放 next效果
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isRecover"></param>
        /// <param name="stageRecoverTime"></param>
        private void PrePlayNextEffect(SkillStage skillStage, I_EffectParam i_EffectParam, bool isRecover, int stageRecoverTime)
        {
            List<int> next = i_EffectParam.Next;
            if (next == null || next.Count == 0) return;
            BaseBlackBoard baseBlackBoard = i_EffectParam.SaveSkill ? (BaseBlackBoard)skillBlackBoard : skillStage.StageBlackBoard;

            /// 2023/2/15
            ///     需要判定 当前效果 是否属于 执行时间点 固定的效果, 如果是 固定时间点执行的效果, 可以执行 stageRecoverTime 的效果恢复
            ///     但对于 效果线长度 不确定的 效果, 此处 无法恢复, 需要 跟 大佬们 沟通一个方案才行, 目前是 先不处理
            /// 
            /// 后续:
            ///     已经跟夏哥确认, 对于变长的效果线,由于数据的缺失,客户端 无法效果恢复

            // NEXT_TRUE 的效果分支
            bool IsNeedRegNextTrue = EffectResultUtils.IsRegNext(baseBlackBoard, i_EffectParam, true);
            if (IsNeedRegNextTrue && next.Count > 0)
            {
                int trueEffectID = next[0];
                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"提前 播放 next true effectID: {trueEffectID}");

                skillStage.PlayNextEffect(trueEffectID, isRecover, stageRecoverTime);
            }

            // NEXT_FALSE 的效果分支
            bool IsNeedRegNextFalse = EffectResultUtils.IsRegNext(baseBlackBoard, i_EffectParam, false);
            if (IsNeedRegNextFalse && next.Count > 1)
            {
                int falseEffectID = next[1];
                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"提前 播放 next false effectID: {falseEffectID}");

                skillStage.PlayNextEffect(falseEffectID, isRecover, stageRecoverTime);
            }
        }

        /// <summary>
        ///  服务器 触发 next 效果的执行, 需要考虑 两件事 :
        ///     1. 如果是 需要等待 服务器线 的数据
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isRecover"></param>
        /// <param name="isServerLine"></param>
        /// <param name="stageRecoverTime"></param>
        private void ServerPreRegPlayEffectNext(SkillStage skillStage, I_EffectParam i_EffectParam, bool isRecover, int stageRecoverTime)
        {
            List<int> next = i_EffectParam.Next;
            if (next == null) return;
            BaseBlackBoard baseBlackBoard = i_EffectParam.SaveSkill ? (BaseBlackBoard)skillBlackBoard : skillStage.StageBlackBoard;

            bool needPreRegNextEffect = CheckNeedPreRgeNextEffect(baseBlackBoard, i_EffectParam, true);
            // 当不需要注册 next效果 的时候, 可以 去 看看 能否 直接触发 next 的服务器效果
            if (!needPreRegNextEffect)
            {
                /// 2023/3/13
                /// bug 描述 : 技能 串行效果 顺序 Enemy1 ----> Move ---->  IsEmpty ---> xx;
                ///     收到 服务器的 黑板顺序 如下 Enemy1 ---->  IsEmpty---> xx----> Move ;
                ///     客户端 先处理 了 IsEmpty, 发现 前置 效果 Move 未执行, IsEmpty 效果执行失败;
                ///     待 服务器线 触发 Move 效果 的时候, move 的 next  并不需要 服务器线 注册, 触发,
                ///     在 此处 效果线 执行 被 中断, 后续效果 都没法执行.
                /// 
                /// fix:
                ///     修复方式 就是 在 服务器线 执行 PreRegPlayEffectNext 发现不需要 注册next 的时候, 尝试 再次 触发next 的服务器线.

                /// note:
                ///     进入此处, 表明此时 不需要 服务器 提前注册, 即 此效果 不是 判定效果(一般来说,非判定效果的next 最多数量只有1).
                ///     对照上面的 例子来说, 此处 即是 服务器 触发 效果 Move , 此处 需要 客户端 修正 执行效果 顺序, 去执行 next 效果 IsEmpty;
                PrePlaySeverRegEffectNext(baseBlackBoard, i_EffectParam, isRecover, stageRecoverTime);
                return;
            }

            // 提前播放 next 效果
            PrePlayNextEffect(skillStage, i_EffectParam, isRecover, stageRecoverTime);
        }

        private void PrePlaySeverRegEffectNext(BaseBlackBoard baseBlackBoard, I_EffectParam i_EffectParam, bool isRecover, int stageRecoverTime)
        {

            // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"在 服务器线 不需要注册等待效果 时, 效果: {i_EffectParam.EffectID}, outputKey: {i_EffectParam.OutputKey} 准备触发 Next");
            I_EffectParam nextEffect = GetNormalNextEffect(i_EffectParam, baseBlackBoard);

            if (nextEffect != null)
            {
                PreTriggerPlayServerEffect(nextEffect, baseBlackBoard, 0);
            }

        }

        private string FormatEffectKey(int effectID)
        {
            return $"{TagFlag} effectID[{effectID}]";
        }

        /// <summary>
        /// 立即播放 effectData的下一个效果
        /// 会取消 之前注册的next效果
        /// note:
        ///     1.对于服务器来说,效果是个点的概念.所以，服务器认为 发给客户端的效果数据的时候,效果就已经结束了.
        ///     2.对于客户端来说,效果 是个线段的概念.有效果的开始和效果的结束.
        ///     3.对于客户端的服务器线来说,客户端 是收到了 黑板数据后,执行对应的服务器效果线逻辑.
        ///     4.对于一些 延时的效果(比如：用户输入效果 和 蓄力效果),服务器数据如果过来了,表明的是这个效果结束
        /// /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="effectData"></param>
        /// <param name="nextValue"></param>
        public void ImmediatelyPlayEffectNext(SkillStage skillStage, EffectData effectData, bool nextValue, int stageRecoverTime)
        {
            string key = FormatEffectKey(effectData.EffectID);
            DelayInvoker.CancelInvoke(key);

            // SGF.Debuger.Log($"{TagFlag} ImmediatelyPlayEffectNext stage {skillStage.StageIDStr}  effectID {effectData.EffectID} , CancelInvoke {key}");

            int[] next = effectData.Next;
            if (next == null) return;
            // NEXT_TRUE 的效果分支
            {
                if (next.Length > 0 && nextValue)
                {
                    int trueEffectID = next[0];
                    // EffectData trueEffectData = skillStage.GetEffectData(trueEffectID);
                    // PlayStageEffect(skillStage, trueEffectData, false, true, stageRecoverTime);
                    skillStage.PlayNextEffect(trueEffectID, false, stageRecoverTime);
                }
            }

            // NEXT_FALSE 的效果分支
            {
                if (next.Length > 1 && nextValue)
                {
                    int falseEffectID = next[1];
                    // EffectData falseEffectData = skillStage.GetEffectData(falseEffectID);
                    // PlayStageEffect(skillStage, falseEffectData, false, true, stageRecoverTime);
                    skillStage.PlayNextEffect(falseEffectID, false, stageRecoverTime);

                }
            }
        }

        /// <summary>
        /// 播放下一个效果的接口
        /// note:
        ///     效果的 执行实践延迟都在 技能中 已经处理,外部不需要任何的延迟
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="effectData"></param>
        /// <param name="isRecover"></param>
        /// <param name="stageRecoverTime">阶段的恢复时间</param>
        private void PlayNextEffect(SkillStage skillStage, EffectData effectData, bool isRecover, int effectStartTime, int stageRecoverTime)
        {
            string key = FormatEffectKey(effectData.EffectID);
            histroyKeySet.Add(key);
            float delayTime = (effectStartTime) / 1000.0f;

            int nextEffectRecoverTime = stageRecoverTime - (int)delayTime;
            nextEffectRecoverTime = nextEffectRecoverTime < 0 ? 0 : nextEffectRecoverTime;

            if (delayTime > 0)
            {
                object[] Os = new object[] { skillStage, effectData, isRecover, nextEffectRecoverTime };
                //DB_Close       SGF.Debuger.Log($"{TagFlag} reg PlayNextEffect stage {skillStage.StageIDStr}  effectID {effectData.EffectID} , delayTime {delayTime}");

                DelayInvoker.DelayInvoke(key, delayTime,
                    (object[] args) =>
                    {
                        SkillStage skillStage1 = (SkillStage)Os[0];
                        EffectData effectData1 = (EffectData)Os[1];
                        bool isRecover1 = (bool)Os[2];
                        int nextEffectRecoverTime1 = (int)Os[3];
                        //DB_Close       SGF.Debuger.Log($"{TagFlag} run  PlayNextEffect stage {skillStage1.StageIDStr}  effectID {effectData1.EffectID}, stageRecoverTime {stageRecoverTime}, nextEffectRecoverTime1 {nextEffectRecoverTime1} ");

                        /// 2023/2/15
                        /// 目前 stageInfo 中 已经具备所有效果 在 skillStage 上的实际 执行时间,
                        /// 所以在 执行next 效果的恢复的时候, 可以统一 通过 stageRecoverTime - 效果在阶段上的执行时间 ,
                        /// 即可以得到 这个效果 实际 的 恢复时间

                        PlayStageEffect(skillStage1, effectData1, isRecover1, true, stageRecoverTime);
                    }
                , Os);
            }
            else
            {
                // delay ==0 的时候 立即执行这个效果, delayInvoke 会延迟一帧 
                PlayStageEffect(skillStage, effectData, isRecover, true, stageRecoverTime);
            }

        }

        /// <summary>
        /// 技能播放效果的接口

        /// note：
        ///     效果的执行 此处分为了 服务器效果线/客户端效果线
        ///     服务器效果线 : 不论这个效果,客户端本地计算为 可不可以 播放, 本地都注册一个服务器效果,等待服务器数据
        ///     客户端效果线 : 客户端效果 根据自己本地计算,决定这个效果是否可以.
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isRecover">是否是复原阶段</param>
        /// <param name="isFrameStart">是否是效果首帧</param>
        /// <returns>效果执行后的结果，True 表明效果 执行 成功了, False表示效果不满足执行条件</returns>
        private bool TryPlayEffect(SkillStage skillStage, I_EffectParam i_EffectParam, bool isRecover, bool isFrameStart, int stageRecoverTime)
        {
            BaseBlackBoard baseBlackBoard = i_EffectParam.SaveSkill ? (BaseBlackBoard)skillBlackBoard : skillStage.StageBlackBoard;
#if (UNITY_EDITOR && BATTLE_DEBUG)
            LogUtils.Log(LogUtils.LogEnum.Skill, $" {TagFlag} 阶段 stage_{skillStage.StageIDStr} [TryPlayEffect] : {i_EffectParam.OutputKey}, {i_EffectParam.EffectID}  ", playerData.isMainPlayer);
#endif
            // 如果是 效果的结束帧,不需要取执行 服务器效果
            if (!isFrameStart)
            {
                //dont do TryPlayServerEffect
            }
            else
            {
#if (UNITY_EDITOR && BATTLE_DEBUG)
                DebugClientEffectData($"reg ID: {i_EffectParam.EffectID}", new List<string>() { $"outPutKey: {i_EffectParam.OutputKey}" });
#endif
                // 先在黑板中注册 当前 服务器效果配置 
                // 服务器效果 在每次 效果执行的时候,先注册
                // 注册的时候,用 clone 的数据, 因为 可能在 某一时刻才执行
                // note:
                //      注册服务器效果监听, 目前 只会在 阶段 执行效果的时候 注册一遍
                RegisterServerEffect(skillStage, i_EffectParam.EffectData);
#if (UNITY_EDITOR && BATTLE_DEBUG)
                DebugClientEffectData($"trigger ID: {i_EffectParam.EffectID}", new List<string>() { $"outPutKey: {i_EffectParam.OutputKey}" });
#endif
                StageTryPlayServerEffect(i_EffectParam, baseBlackBoard, stageRecoverTime);
            }

            /// 2023/2/13
            /// 阶段效果恢复的时候,需要根据 stageRecoverTime 时间,去恢复之前的效果, 所以也需要执行 之前的 客户端效果(比如隐身这种客户端表现的效果)
            /// 客户端 需要根据 恢复时间,执行的时候,判定 当前 隐身是否还在,如果在的话,就需要恢复.
            {
                // 如果是复原阶段,不需要播放 客户端效果
                // if (isRecover)
                // {
                //     return true;
                // }
            }
#if (UNITY_EDITOR && BATTLE_DEBUG)
            DebugClientEffectData($"play ID: {i_EffectParam.EffectID}", new List<string>() { $"outPutKey: {i_EffectParam.OutputKey}", $"isFrameStart: {isFrameStart}" });

            LogUtils.Log(LogUtils.LogEnum.Skill, $" {TagFlag} 阶段 stage_{skillStage.StageIDStr} [TryPlayEffect] 播放客户端效果 : {i_EffectParam.OutputKey}, {i_EffectParam.EffectID}  ", playerData.isMainPlayer);
#endif
            bool result = FuncOnTryPlayClientEffect.Invoke(E_StageType.Skill, RuntimeID, i_EffectParam, baseBlackBoard, isFrameStart, BuilderID, OwnerEntityID, stageRecoverTime, isRecover);

            // 客户端执行完效果后,刷新 客户端 效果黑板的 executeResult
            EffectResultUtils.UpdateEffectExecuteResult(baseBlackBoard, i_EffectParam, true, result);

            return result;
        }



        /// <summary>
        /// 客户端尝试 播放服务器效果线的逻辑
        /// </summary>
        /// <param name="regEffectParam"></param>
        /// <param name="baseBlackBoard"></param>
        private void StageTryPlayServerEffect(I_EffectParam regEffectParam, BaseBlackBoard baseBlackBoard, int stageRecoverTime)
        {
            string outputKey = regEffectParam.OutputKey;

            // 客户端检查 服务器的 效果数据是否已经全部同步过来
            // 查看阶段中是否含有这个效果数据  (服务器黑板)
            object v = baseBlackBoard.Get(outputKey, E_BlackBoardTag.Server);
            // 如果 服务器黑板中 找不到 key 关联的数据,那结束
            if (v == null)
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  [client] TryPlayServerEffect : {i_EffectParam.EffectID} , {outputKey} no data !!!");
                return;
            }
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  [client] TryPlayServerEffect : {i_EffectParam.EffectID} , {outputKey} , {stageRecoverTime} ");

            // 如果找到了服务器黑板数据,那就执行这个效果,如果效果执行失败,那就结束
            // 客户端 阶段 触发 服务器效果的时候(表明)
            bool result = TryPlayServerRegEffect((CustomBlackBoardNode)v, regEffectParam, baseBlackBoard, stageRecoverTime);
            // TryPlayRegistedServerEffect((CustomBlackBoardNode)v, baseBlackBoard, stageRecoverTime);
#if (UNITY_EDITOR && BATTLE_DEBUG)
            DebugClientEffectData($"play ID: {regEffectParam.EffectID}", new List<string>() { $"outPutKey: {regEffectParam.OutputKey}", $"result: {result}" });
#endif
            // 执行完服务器效果后,刷新 服务器 效果黑板的 executeResult
            EffectResultUtils.UpdateEffectExecuteResult(baseBlackBoard, regEffectParam, false, result);

            if (!result)
            {
                //DB_Close       SGF.Debuger.Log($"{TagFlag}  [client] TryPlayServerEffect : {regEffectParam.EffectID} result : {result}");
                return;
            }
        }

        /// <summary>
        /// 提前去 触发 注册效果regEffectParam 能够 播放的效果
        /// </summary>
        /// <param name="regEffectParam"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="stageRecoverTime"></param>
        private void PreTriggerPlayServerEffect(I_EffectParam regEffectParam, BaseBlackBoard baseBlackBoard, int stageRecoverTime)
        {
            // 检查 阶段的这个效果的服务器线 能否 触发, 目前 需要判断 outputKey 或者 inputKeys 是否满足
            bool canTrigger = CheckCanTriggerRegEffect(regEffectParam, baseBlackBoard, out object v);

            // 如果数据 都没到, 那就不能触发, 也就不需要执行后续逻辑
            if (!canTrigger)
            {
                return;
            }

            ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}   TryPlayServerEffect : {i_EffectParam.EffectID} , {outputKey} , {stageRecoverTime} ");
            CustomBlackBoardNode triggleValue = (CustomBlackBoardNode)v;

            // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 阶段尝试 取执行服务器效果线, 采用的 key : {triggleValue.Key}");

            // 如果找到了服务器黑板数据,那就执行这个效果,如果效果执行失败,那就结束
            // 如果 服务器效果没播放成功,那就注册这个效果 到黑板数据中
            bool result = TryPlayCanTriggerRegistedServerEffect(regEffectParam, triggleValue, baseBlackBoard, stageRecoverTime);

            if (!result)
            {
                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}   TryPlayServerEffect : {regEffectParam.EffectID} result : {result}");
                return;
            }
        }

        /// <summary>
        /// 检查 注册的 效果 能否 触发 , 条件 是 这个效果 是否 满足 所有的 inputKeys 或者 这个效果的 outputKey 是否到达
        /// </summary>
        /// <param name="regEffectParam">注册 发 效果监听</param>
        /// <param name="baseBlackBoard">效果 存在的黑板</param>
        /// <param name="triggleServerBoardNode">服务器触发这个 注册效果 黑板数据, 可能是 outputkey 的结果, 也可能是 inputkeys 的最后一个结果, 不能触发的时候, 返回为 null </param>
        /// <returns></returns>
        private bool CheckCanTriggerRegEffect(I_EffectParam regEffectParam, BaseBlackBoard baseBlackBoard, out object triggleServerBoardNode)
        {
            triggleServerBoardNode = null;

            List<string> inputKeys = regEffectParam.InputKeys;
            string outputKey = regEffectParam.OutputKey;

            bool hasRegOutPutKey = outputKey.Length == 0 ? true : baseBlackBoard.Contain(outputKey, E_BlackBoardTag.Reg);

            bool hasInputkeys = EffectUtils.CheckIsBlackBoardHasKeys(baseBlackBoard, regEffectParam.InputKeys, E_BlackBoardTag.Server);

            bool hasOutputkey = EffectUtils.CheckIsBlackBoardHasKey(baseBlackBoard, outputKey, E_BlackBoardTag.Server);

            // 如果 inputKeys 的数据 没到,并且 outputKey的数据也没到,那铁定不能执行
            if (!hasRegOutPutKey || !hasOutputkey && !hasInputkeys)
            {
                return false;
            }

            SkillStage skillStage = (SkillStage)regEffectParam.ExtraData;
            if (skillStage == null)
            {
                return false;
            }

            // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  CheckCanTriggleRegEffect : {regEffectParam.EffectID} , {outputKey}, hasOutputkey: {hasOutputkey}, hasInputkeys: {hasInputkeys} ");


            // 如果 有 outputKey的 值, 直接 使用 outputKey的 值就可以了
            if (hasOutputkey)
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  拥有outputKey的黑板数据,取黑板数据 ");
                bool hasServerExecuted = EffectResultUtils.CheckEffectExecuteResult(regEffectParam, baseBlackBoard, E_BlackBoardTag.Server);
                if (hasServerExecuted)
                {
                    // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 效果 id: {regEffectParam.EffectID} , outputKey: {outputKey} 是 [串行] 效果, 它本身已经被服务器线执行, 所以 这个效果不用 触发");
                    return false;
                }

                // 客户端检查 服务器的 效果数据是否已经全部同步过来
                // 查看阶段中是否含有这个效果数据  (服务器黑板)
                triggleServerBoardNode = baseBlackBoard.Get(outputKey, E_BlackBoardTag.Server);
            }
            else
            {
                /// 2023/3/8
                /// 如果 存在 并行效果 [1,2,3] , 此时 客户端线跑到 效果 3 , 但是 服务器线 只 同步到 1.
                /// 效果 3 inputKeys 为[A] , 此时 黑板中 存在 A 数据, 满足 3 的 执行条件.
                /// 
                /// 跟gl 确定需求 如下:
                ///     1.并行的效果线没有强依赖关系，如果有强依赖关系的两个效果却配成了并行就是策划配错了!!!
                ///       所以 并行的效果, 只需要满足条件 即可以 触发, 不需要关心 同在 时间轴上的前面的效果.
                ///     
                ///     2.对于 串行效果, 执行顺序 需要满足 : 1--->2--->3 ;
                ///       当 满足 了 3 的执行条件后, 但是 2 又 没有触发, 此时 3 并不执行, 需要等待 2 执行完成后,再去触发 3;
                /// 
                ///     3.不同的效果,不可能 有相同的 next, 这点 gl 保证！！！
                /// 
                /// 所以 判定 一个效果 能不能 触发, 就需要判定 这个效果 是并行效果 还是 串行效果.

                int effectID = regEffectParam.EffectID;

                bool isNextFrame = skillStage.CheckIsNextEffectFrame(effectID);

                // 如果 是 并行线上的效果, 那就 不管 ,直接检查 这个注册效果能不能触发即可
                if (!isNextFrame)
                {
                    // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 效果 id: {effectID} , outputKey: {outputKey} 是并行效果, 可以直接检查 inputKeys 是否满足触发");
                }
                else
                {
                    int parentEffectID = skillStage.GetNextIDParent(effectID);
                    EffectData parentEffectData = skillStage.GetEffectData(parentEffectID);

                    EffectParam parentEffectParam = CreateEffectParam(skillStage, parentEffectData);

                    // 如果是 串行 的效果, 就要去检查 它的 父 效果 有没有执行, 如果没有执行,那就不能执行
                    bool hasParentServerExecuted = EffectResultUtils.CheckEffectExecuteResult(parentEffectParam, baseBlackBoard, E_BlackBoardTag.Server);

                    // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 效果 id: {effectID} , outputKey: {outputKey} 是 [串行] 效果, 父效果: {parentEffectID}, {parentEffectParam.OutputKey} 是否已经执行服务器线: {hasParentServerExecuted}");

                    // 如果 这个next 的效果 的 父效果 没有执行, 那么 这个 next效果就 不执行
                    if (!hasParentServerExecuted)
                    {
                        // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 效果 id: {effectID} , outputKey: {outputKey} 是 [串行] 效果, 父效果 未执行, 所以 这个效果暂时不先 触发");
                        return false;
                    }

                    bool hasServerExecuted = EffectResultUtils.CheckEffectExecuteResult(regEffectParam, baseBlackBoard, E_BlackBoardTag.Server);
                    if (hasServerExecuted)
                    {
                        // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 效果 id: {effectID} , outputKey: {outputKey} 是 [串行] 效果, 它本身已经被服务器线执行, 所以 这个效果不用 触发");
                        return false;
                    }
                }

                // 如果没有 outputkey ,但是 存在 inputKeys 的数据，
                // 那说明 客户端 已经具备 能够执行 的数据了, 所以 此时 客户端 直接拿 服务器黑板中 inputkeys 需要的数据执行即可
                // 看看 是否能够执行成功
                if (inputKeys.Count > 0)
                {
                    // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  客户端已经 拥有 inputKeys 的黑板数据,先取黑板: {inputKeys[inputKeys.Count - 1]} 的数据 去执行 效果线");

                    triggleServerBoardNode = baseBlackBoard.Get(inputKeys[inputKeys.Count - 1], E_BlackBoardTag.Server);
                }
            }

            // 如果 服务器黑板中 找不到 key 关联的数据,那结束
            if (triggleServerBoardNode == null)
            {
                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  取的数据为空 ");

                // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  [client] TryPlayServerEffect : {i_EffectParam.EffectID} , {outputKey} no data !!!");
                return false;
            }

            return true;
        }


        /// <summary>
        /// 阶段注册 服务器效果线 效果的接口
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="effectData"></param>
        private void OnActionStageRegisterServerEffect(SkillStage skillStage, EffectData effectData)
        {
            RegisterServerEffect(skillStage, effectData);
        }

        private EffectParam CreateEffectParam(SkillStage skillStage, EffectData effectData)
        {
            EffectParam newEffectParem = new EffectParam();
            newEffectParem.Init(effectData, BuilderID, OwnerEntityID, E_StageType.Skill, $"{RuntimeID}_{skillStage.StageIDStr}");

            // 注册黑板的时候,同时把阶段的 skillStage 存入 效果参数中
            {
                // 临时方案,目前服务器 发给客户端的 黑板数据 是效果的 outputkey.
                // note:
                //     1.如果策划配置了效果写入技能黑板,服务器 黑板数据会通过技能运行时同步过来.
                //     2.当黑板数据通过技能同步过来时,客户端需要定位到具体效果在什么阶段.
                //       例如输入轴这种效果,收到黑板数据后,客户端需要关闭输入轴.
                //       因此,客户端需要根据黑板数据定位到 这个效果 到底在哪.
                //       所以目前会在客户端的注册黑板中,将 skillStage 也存下来,等收到服务器数据的时候,不需要反向定位
                //
                newEffectParem.SetExtraData(skillStage);
            }

            return newEffectParem;
        }

        /// <summary>
        /// 注册服务器 效果
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="effectData"></param>
        private void RegisterServerEffect(SkillStage skillStage, EffectData effectData)
        {
            // Note:
            // 注册服务器 的效果参数可能 存在黑板中 延迟处理, 所以此处需要由自己 独立的 一份数据
            // 所以此处 effectParam 是 每次都重新 new 一个
            // 后面考虑 EffectParam 弄一个工厂模式
            EffectParam newServerEffectParam = CreateEffectParam(skillStage, effectData);

            // 根据数据 是SaveSkill,知道 要操作哪个黑板
            BaseBlackBoard regBlackBoard = effectData.SaveSkill ? (BaseBlackBoard)skillBlackBoard : skillStage.StageBlackBoard;

            string outputKey = newServerEffectParam.OutputKey;
            ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [client]  stage : {skillStage.StageIDStr}  RegisterServerEffect : {newServerEffectParam.EffectID} , outputKey : {outputKey} , isSkill : {effectData.SaveSkill} ");
            // 先注册 inputkeys
            EffectUtils.RegisterInputKeysEffect(regBlackBoard, newServerEffectParam);

            EffectUtils.RegisterOutputKeyEffect(regBlackBoard, newServerEffectParam);
        }

        /// <summary>
        /// 尝试 去播放 服务器同步 的 黑板效果
        /// 此时 是服务器 通过阶段 同步过来的 效果说句,所以此时 执行黑板的时候,不需要执行恢复逻辑,直接 recoverStageTime = 0 即可
        /// </summary>
        /// <param name="serverCustomBlackBoardNode"></param>
        /// <param name="baseBlackBoard"></param>
        /// <returns></returns>
        private bool StageTryPlayResitedServerEffect(CustomBlackBoardNode serverCustomBlackBoardNode, BaseBlackBoard baseBlackBoard)
        {

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} , 尝试 播放 服务器黑板数据key: {serverCustomBlackBoardNode.Key} 的效果");
            return TryPlayRegistedServerEffect(serverCustomBlackBoardNode, baseBlackBoard, 0);
        }


        /// <summary>
        /// 服务器 尝试 播放服务器效果线
        /// note：
        ///     1.每个效果的 outPutkey 都可能是List<string> 格式.
        ///     2.对于客户端来说，每次 收到一个服务器的 BlackBoardNode的时候,都需要 TryPlayRegistedServerEffect 尝试播放这个效果.
        ///     3.只有这个效果依赖 的 所有的 outPutkey 数据都齐全的时候, TryPlayRegistedServerEffect 才会成功;
        ///     4.所以 FuncOnTryPlayServerEffect 播放效果的时候，需要检查它 依赖的 所有的 outPutkey。
        /// </summary>
        /// <param name="serverCustomBlackBoardNode">尝试 播放 注册效果的 黑板数据, 一个 黑板数据 可能对应多份 效果监听, 这个黑板效果 会依次触发每个监听</param>
        /// <param name="baseBlackBoard">阶段黑板</param>
        /// <returns>技能播放服务器效果结果</returns>
        private bool TryPlayRegistedServerEffect(CustomBlackBoardNode serverCustomBlackBoardNode, BaseBlackBoard baseBlackBoard, int stageRecoverTime)
        {

            bool checkCanTriggle = CheckServerDataCanTriggerRegEffects(serverCustomBlackBoardNode, baseBlackBoard, out List<I_EffectParam> canTriggleRegEffects);

            if (!checkCanTriggle)
            {
                return false;
            }

            // 如果 可以触发的 效果 数量为 0, 则返回 触发失败
            if (canTriggleRegEffects.Count == 0)
            {
                return false;
            }

            bool result = false;
            string key = serverCustomBlackBoardNode.Key;

            List<I_EffectParam> triggleSuccessRegEffectParam = new List<I_EffectParam>();

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 黑板key: {key} ,开始 播放 监听效果:");

            canTriggleRegEffects.ForEach((I_EffectParam regEffectParam) =>
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"------{TagFlag} 服务器 黑板key: {key} trigger regEffect: {regEffectParam.OutputKey} ------------- start");

                bool triggleResult = TryPlayCanTriggerRegistedServerEffect(regEffectParam, serverCustomBlackBoardNode, baseBlackBoard, stageRecoverTime);
                if (triggleResult)
                {
                    if (regEffectParam.OutputKey == key)
                    {
                        result = true;
                    }
                }
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"------{TagFlag} 服务器 黑板key: {key} trigger regEffect: {regEffectParam.OutputKey} ------------- end");

            });
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 服务器 黑板key: {key} 执行 result: {result}");

            return result;
        }

        /// <summary>
        /// 播放能够触发的服务器效果
        /// </summary>
        /// <param name="serverCustomBlackBoardNode"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="stageRecoverTime"></param>
        /// <returns></returns>
        private bool TryPlayCanTriggerRegistedServerEffect(I_EffectParam regEffectParam, CustomBlackBoardNode serverCustomBlackBoardNode, BaseBlackBoard baseBlackBoard, int stageRecoverTime)
        {
            string sreverKey = serverCustomBlackBoardNode.Key;
            string outputKey = regEffectParam.OutputKey;

            // 首先 更新 一下黑板数据 的 result
            EffectResultUtils.UpdateServerEffectResult(regEffectParam, serverCustomBlackBoardNode);

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 黑板key: {sreverKey}, 尝试触发 outputKey: {outputKey} , regEffectParam : {regEffectParam.EffectID}");
            bool triggleResult = TryPlayServerRegEffect(serverCustomBlackBoardNode, regEffectParam, baseBlackBoard, stageRecoverTime);




            return triggleResult;
        }

        /// <summary>
        /// 检查 服务器 发过来的黑板 数据 是否能够 触发 注册 效果.
        /// 只有 注册效果 满足 inputKeys 或者 满足 outputKey 之后，这个效果 才能触发.
        /// 
        /// 2023/3/8
        ///     note: 效果的 触发 后面还需要考虑  后续的效果 如果执行, 前面的效果 就 不需要再 执行的问题.  比如 时间轴上 存在 效果 1 | 2 | 3 , 
        ///     eg1:    
        ///         现在来了 一个 黑板 数据 A ， 它 可以 触发 [1,2] , 此时 触发的顺序 就应该 是 [1 --> 2];
        ///     eg2:
        ///         存在 一个黑板数据 B , 它 满足 了效果 3, 但是 [1,2] 未 触发, 此时 3 是否应该执行??? 理论上 是不应该才对;
        ///         
        ///     跟 gl 确定 , 服务器 的效果 一定 是  1 ---> 2 ---> 3 , 所以 不会出现 [1,2] 未触发, 但是 3 能触发的情况.
        /// </summary>
        /// <param name="serverCustomBlackBoardNode"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="canTriggleRegEffectParam"></param>
        /// <returns></returns>
        private bool CheckServerDataCanTriggerRegEffects(CustomBlackBoardNode serverCustomBlackBoardNode, BaseBlackBoard baseBlackBoard, out List<I_EffectParam> canTriggleRegEffectParam)
        {
            canTriggleRegEffectParam = new List<I_EffectParam>();

            string key = serverCustomBlackBoardNode.Key;
            if (baseBlackBoard == null)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} CheckServerDataCanTriggerRegEffects : key={key},baseBlackBoard is null !!!");
                return false;
            }

            return CheckRegKeyEffectCanTriggle(key, baseBlackBoard, canTriggleRegEffectParam);
        }

        /// <summary>
        /// 检查 reg 的 key 的效果 是否可以 触发 服务器线
        /// </summary>
        /// <param name="regKey"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="canTriggleRegEffectParam"></param>
        /// <returns></returns>
        private bool CheckRegKeyEffectCanTriggle(string regKey, BaseBlackBoard baseBlackBoard, List<I_EffectParam> canTriggleRegEffectParam)
        {
            object v = null;
            /// 2023/3/13
            /// 效果 的 outputKey 可能 为 空, 如果 存在一个 效果 1 ---> 2 --->3 , 其中 2 没有 outPutKey ， 1 outputKey A (A 是 需要由服务器注册的效果), 3 outputKey C;
            /// 
            /// note:
            ///     对于上面 的情况, 只有 A 是 标记了 由 服务器 注册 的效果才行. 如果 1 是 普通的 服务器效果, 此时 1 在 客户端线 提前 预播的时候 会注册 效果2,
            ///     由于 2  只有 客户端线(不需要 outputkey的服务器同步数据), 所以 2 会由客户端线 提前播放.
            ///     
            ///     而如果 1 是 需要由服务器 注册的效果(类似于 isEmpty 判定效果),此时 效果 1 由服务器 黑板数据同步过来的时候 触发 下个效果 2 的注册 .
            ///     
            ///     所以此处 判断 reg 效果能否执行, 也不需要 判断 key是否为空


            /// <summary>
            /// 先从 注册黑板中取对于的效果配置数据(I_EffectParam)
            /// </summary>
            v = baseBlackBoard.Get(regKey, E_BlackBoardTag.Reg);

            if (v == null)
            {
                //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} CheckServerDataCanTriggerRegEffects : key = {key} , reg is null !!!");
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 检查黑板: key = {regKey} 没有注册监听, 啥都不干");

                return false;
            }


            CusListQueue<I_EffectParam> regEffectParams = (CusListQueue<I_EffectParam>)v;
            if (regEffectParams.Count == 0)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} CheckServerDataCanTriggerRegEffects : key = {regKey} 存在过 注册,但时 监听的效果regEffectParams is null !!!");
                return false;
            }

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 开始检查 黑板 regKey {regKey} 注册的 效果 能否 触发:");

            foreach (I_EffectParam regEffectParam in regEffectParams)
            {
                // 收到 了服务器 黑板 数据 后, 去触发 这个 黑板 注册的效果之前, 先检查一次 这个效果 是否 能够触发(满足 所有 inputKeys 或者 满足 outputkey).
                // 只有 满足 触发条件之后, 这个 注册 效果 才需要 去 触发执行.
                bool canTriggle = CheckCanTriggerRegEffect(regEffectParam, baseBlackBoard, out object v1);
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 服务器黑板key: {regKey} ,检查是否可以触发效果 outputKey: {regEffectParam.OutputKey} ,canTriggle: {canTriggle}");
                if (canTriggle)
                {
                    canTriggleRegEffectParam.Add(regEffectParam);
                }
            }

            return true;
        }

        /// <summary>
        /// 检查 服务器 线 能否 触发 普通注册效果的next 效果, 普通效果的 next.length <=1; 
        /// note:
        ///     1. 此处 是检查 普通效果的 next 效果是否可以触发, 不是 判断效果的next, 所以 一般情况应该只运行 next 只有一个, 如果有多个, 此处需要拓展;
        ///     2. 会存在 效果 [1-->2-->3] ， 1 outputKey: A , 2 outputKey 空, 3 outputKey: B 的情况, 此时 效果 1 执行成功, 需要触发 1 的next, 
        ///        只要 2 满足 inputKey 的条件, 2 即可执行.  如果 2 不满足情况, 那 3 也就不需要 提前触发.
        /// </summary>
        /// <param name="regEffect">注册效果</param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="nextEffectParam">注册效果的 next 效果</param>
        /// <returns></returns>
        private bool CheckServerCanTriggerNormalNextEffect(I_EffectParam regEffect, BaseBlackBoard baseBlackBoard, out I_EffectParam nextEffectParam)
        {
            nextEffectParam = null;

            if (baseBlackBoard == null)
            {
                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} CheckServerDataCanTriggerRegEffects : key={regEffect.OutputKey}, baseBlackBoard is null !!!");
                return false;
            }
            SkillStage skillStage = (SkillStage)regEffect.ExtraData;

            List<int> next = regEffect.Next;
            if (next.Count == 0)
            {
                return false;
            }
            if (next.Count > 1)
            {
#if (UNITY_EDITOR && BATTLE_DEBUG)
                LogUtils.LogError(LogUtils.LogEnum.Skill, $"skill_{skillId}, skillStage_{skillStage.StageIDStr}, regEffect: {regEffect.EffectID} 应该只是个普通效果, 不应该有多个 next效果id ");
#endif
                SGF.Debuger.LogWarning($"[SkillEntityActionPartial] skill_{skillId}, skillStage_{skillStage.StageIDStr}, regEffect: {regEffect.EffectID} 应该只是个普通效果, 不应该有多个 next效果id ");

#if UNITY_EDITOR
                UnityEngine.Debug.Break();
#endif
                return false;
            }

            int nextTrue = regEffect.Next[0];
            EffectData effectData = skillStage.GetEffectData(nextTrue);
            skillStage.GetEffectDataOutputKey(effectData, out string nextTrueKey, out E_SkillEffect skillEffect);

            nextEffectParam = CreateEffectParam(skillStage, effectData);

            bool canTriggle = CheckCanTriggerRegEffect(nextEffectParam, baseBlackBoard, out object v1);
            // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} CheckServerDataCanTriggerRegEffects : key={regEffect.OutputKey} nextEffect: [{nextEffectParam.EffectID}] , key: {nextEffectParam.OutputKey} canTriggle: {canTriggle}");

            return canTriggle;
        }

        private I_EffectParam GetNormalNextEffect(I_EffectParam normalRegEffect, BaseBlackBoard baseBlackBoard)
        {
            if (baseBlackBoard == null)
            {
                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} CheckServerDataCanTriggerRegEffects : key={normalRegEffect.OutputKey}, baseBlackBoard is null !!!");
                return null;
            }
            SkillStage skillStage = (SkillStage)normalRegEffect.ExtraData;

            List<int> next = normalRegEffect.Next;
            if (next.Count == 0)
            {
                return null;
            }
            if (next.Count > 1)
            {

                SGF.Debuger.LogWarning($"[SkillEntityActionPartial] skill_{skillId}, skillStage_{skillStage.StageIDStr}, regEffect: {normalRegEffect.EffectID} 应该只是个普通效果, 不应该有多个 next效果id ");
#if UNITY_EDITOR
                UnityEngine.Debug.Break();
#endif
                return null;
            }

            int nextTrue = normalRegEffect.Next[0];
            EffectData effectData = skillStage.GetEffectData(nextTrue);
            skillStage.GetEffectDataOutputKey(effectData, out string nextTrueKey, out E_SkillEffect skillEffect);

            return CreateEffectParam(skillStage, effectData);
        }

        /// <summary>
        /// 尝试 用 黑板数据 serverCustomBlackBoardNode  执行 regEffectParam 注册的效果
        /// </summary>
        /// <param name="serverCustomBlackBoardNode">服务器的黑板数据</param>
        /// <param name="regEffectParam">实际触发的 效果</param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="stageRecoverTime"></param>
        /// <returns></returns>
        private bool TryPlayServerRegEffect(CustomBlackBoardNode serverCustomBlackBoardNode, I_EffectParam regEffectParam, BaseBlackBoard baseBlackBoard, int stageRecoverTime)
        {

            string key = serverCustomBlackBoardNode.Key;

            SkillStage skillStage = (SkillStage)regEffectParam.ExtraData;

            /// 2023/2/15
            /// note:
            ///     当执行 服务器线 TryPlayRegistedServerEffect 时, 目前的设计是必须 客户端线先跑;
            ///     当执行 客户端线 TryPlayRegistedServerEffect 时, skillStage 一定存在.
            ///     所以,
            ///         理论上 skillStage == null 应该不会不存在.
            /// note2：
            ///     后续目前不确定 是否需要 修改 服务器 执行 必须依赖 客户端线 先跑的设计.
            ///     如果 改了设计， skillStage == null 就可能存在。
            if (skillStage == null)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} TryPlayRegistedServerEffect : key = {key} , effectID : {regEffectParam.EffectID} , not find skillStage !!!");
                return false;
            }
            bool canTriggle = CheckCanTriggerRegEffect(regEffectParam, baseBlackBoard, out object v1);
            if (!canTriggle)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} TryPlayRegistedServerEffect : key = {key} , effectID : {regEffectParam.EffectID} , 已经不能再次触发 !!!");
                return false;
            }
            string outputKey = regEffectParam.OutputKey;

            int effectStageRuntime = skillStage.GetEffectStageTime(regEffectParam.EffectID);
            int effectRecoverTime = stageRecoverTime - effectStageRuntime;
            effectRecoverTime = effectRecoverTime < 0 ? 0 : effectRecoverTime;
#if (UNITY_EDITOR && BATTLE_DEBUG)
            LogUtils.Log(LogUtils.LogEnum.Skill, $"{TagFlag} 尝试播放服务器效果: [TryPlayRegistedServerEffect] serverkey = {key} , regOutputKey: {outputKey}, regEffectID: {regEffectParam.EffectID} ,stageRecoverTime {stageRecoverTime}, stageRuntime {effectStageRuntime}, effectRecoverTime : {effectRecoverTime}", playerData.isMainPlayer);
#endif
            //如果阶段收到了 效果黑板数据,那 一定从 阶段黑板中 查找数据
            bool result = FuncOnTryPlayServerEffect.Invoke(E_StageType.Skill, RuntimeID, regEffectParam, serverCustomBlackBoardNode, baseBlackBoard, effectRecoverTime);

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} TryPlayRegistedServerEffect : key = {key} , effectID : {i_EffectParam.EffectID} , result {result}");

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 黑板key: {key} 播放效果监听 outputKey: {outputKey}, effectID: {regEffectParam.EffectID} ,stageRecoverTime {stageRecoverTime}, stageRuntime {effectStageRuntime}, effectRecoverTime : {effectRecoverTime} result: {result}");

            /// 执行完服务器效果后,刷新 服务器 效果黑板的 executeResult
            EffectResultUtils.UpdateEffectExecuteResult(baseBlackBoard, regEffectParam, false, result);
            // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 黑板key: {key} 更新 执行结果 效果outputKey: {outputKey}, effectID: {regEffectParam.EffectID} ,stageRecoverTime {stageRecoverTime}, stageRuntime {effectStageRuntime}, effectRecoverTime : {effectRecoverTime} result: {result}");


            //如果效果执行成功 , 删除黑板中 存在的 注册的 outPutKey
            if (result)
            {
                // 如果 有效果 触发成功, 那么 就通知 外面 这个效果执行成功了, 同时, 需要移除所有 这个 注册相关的数据
                if (result)
                {
                    // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 黑板key: {key} 触发 outputKey: {outputKey} 成功,  取消 {outputKey} 监听.");
                    EffectUtils.UnRegisterRegEffect(baseBlackBoard, regEffectParam);
                }
                else
                {
                    // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 黑板key: {key} 触发 outputKey: {outputKey} 失败");
                }

                // 只有在服务器效果 执行为 true的时候,才需要去 检查是否需要 触发这个效果Next的注册
                {
                    /// 2023/2/15
                    /// TODO: DL
                    ///     需要跟 博哥 和夏哥 沟通, 该 如何 恢复 不定时长效果线 的效果
                    /// note:
                    ///     1. 有些 判定效果, 需要 等到服务器 确定后,才会执行 next;
                    ///     2. 对于这种判定 的效果, 因为不确定 到底 等了多久, 其实 也就意味 效果线的 后续效果的执行时机 的时间长度并不确定;
                    ///     3. 目前 效果的 恢复, 是通过 在stageInfo 中 提前 生成一份 确定的 效果执行 时间 dic，来映射所有效果的执行 时间,
                    ///        从而, 根据 阶段的恢复时间 - 效果在阶段上的执行时间 , 就能够完整的 恢复 效果 执行了多久.
                    ///     4. 因此, 对于 不定长 的效果线, 目前 无法 执行恢复
                    ServerPreRegPlayEffectNext(skillStage, regEffectParam, false, stageRecoverTime);
                }
            }
            return result;
        }

        private void OnActionStageTryStopFx(SkillStage skillStage, FXJson fXJson)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            string fxUID = $"{fXJson.EffectName}_{skillStage.StageIDStr}";
            DebugFxData(fxUID, false, $"{fxUID}_{false}", new List<string>() { $"stop: {fXJson.EffectName}" });
#endif
            fxParam.InitWithFxJson(fXJson, BuilderID, OwnerEntityID);
            fxParam.SetExtralKey($"{RuntimeID.ToString()}_[{skillStage.StageIDStr}]");

            ActionOnStopFx.Invoke(fxParam);
        }


        private void OnActionOnStageTryPlayCameraShake(SkillStage skillStage, CameraShakeJson cameraShake, bool start)
        {
            ActionOnPlayCameraShake?.Invoke(start, cameraShake);
        }

        private void OnActionStageWriteSkillEffect(SkillStage skillStage, string key, object v, E_BlackBoardTag blackBoardTag)
        {
            //DB_Close       SGF.Debuger.Log($"{TagFlag}  OnActionStageWriteSkillEffect : key = {key} ,  blackBoardTag {blackBoardTag}");
            skillBlackBoard.Set(key, v, blackBoardTag);
        }

        private void OnActionStageTryPlayCamera(SkillStage skillStage, CameraJson cameraJson, bool isFrameStart)
        {
            int cameraEffectID = cameraJson.CameraCustomData.CameraName;
            E_CameraEffectType e_CameraEffectType = (E_CameraEffectType)cameraJson.CameraCustomData.CameraType;

            ActionOnPlayCamera.Invoke(this, cameraEffectID, e_CameraEffectType);
        }

        /// <summary>
        /// 服务器 通知 阶段创建
        /// </summary>
        /// <param name="skillStage"></param>
        private void OnActionServerStageCreate(SkillStage skillStage)
        {
            if (CurSkillStage != null && CurSkillStage == skillStage)
            {
                ActionOnServerCreateStage?.Invoke(this, skillStage);
            }
        }

    }
}
