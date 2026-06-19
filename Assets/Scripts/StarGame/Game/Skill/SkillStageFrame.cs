using SkillEditor;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using EffectData = SkillEditor.EffectData;


namespace StarProject.Game.Skill
{
    /// <summary>
    /// 统一帧事件处理器
    /// </summary>
    public class SkillStageFrame
    {

        private string TagFlag => $"[SkillStage_{StageIDStr}] [SkillStageFrame],";

        public string StageIDStr;

        /// <summary>
        ///  在timeLine上的时间
        /// </summary>
        public int TimeLineTime;

        /// <summary>
        /// 基于阶段的帧事件的时间
        /// </summary>
        public int FrameTime;

        public int FrameEndTime;

        /// <summary>
        /// 事件帧的 长度。 
        /// note:
        ///     1. 对于 动作/音频这种 事件帧,事件帧的长度 就是 它的实际长度;
        ///     2. 对于 策划配置的效果 事件帧, 时间帧的长度 只是 用来标识 下一个 效果需要间隔多久执行.
        ///        所以 对于 效果帧, 效果恢复 的时候, 需要 根据 每个效果的实际 表现, 去判断是否需要
        ///        执行它的 特效/动作的 恢复.
        /// </summary>
        public int Duration;

        /// <summary>
        /// 是否是 用来server恢复的 拷贝的开始帧.
        /// note:
        ///     如果一个 动画 横跨 ABC 3个阶段,那么 在 B/C 的开始阶段, 会自动生成一个 copy的 动画开始帧.
        ///     这样, 当我们做 阶段恢复的时候, 不管是在 B 还是 C, 都能够 只依赖 这一个阶段,恢复这个动画
        /// </summary>
        public bool IsRecoverCopyStartFrame = false;

        /// <summary>
        /// 生成的恢复帧 在timeLine轴上 实际跳过了 多久
        /// </summary>
        public int RecoverJumpTime = 0;

        public E_StageFrameEventType FrameType;

        public object Value;

        /// <summary>
        /// 是否是开始帧,False 是客户端自己构建的结束帧
        /// 动画轨帧事件中，小曲为了处理结束帧，状态机回不去的逻辑
        /// </summary>
        public bool isFrameStart = true;

        /// <summary>
        /// 播放 动画的接口 <AnimationJson, bool, float>  ----> <AnimationJson, isFrameStart,float>
        /// </summary>
        public Action<AnimationJson, bool, float> ActionOnPlayAnimation;

        /// <summary>
        /// 动画帧结束的事件
        /// </summary>
        public Action<AnimationJson> ActionOnEndAnimation;


        /// <summary>
        /// 播放 特效的接口
        /// note: 参数 传入是 毫秒
        /// </summary>
        public Action<FXJson, int> ActionOnPlayFx;
        /// <summary>
        /// 结束 特效的接口
        /// </summary>
        public Action<FXJson> ActionOnStopFx;

        /// <summary>
        /// 播放 声音
        /// </summary>
        public Action<SoundJson, int> ActionOnPlaySound;

        /// <summary>
        /// 暂停 音频 播放
        /// </summary>
        public Action<SoundJson> ActionOnStopSound;


        /// <summary>
        /// 播放效果  <EffectData, bool, bool, int>  ----> <EffectData, isRecover , isStart, stageRecoverTime>
        /// </summary>
        public Action<EffectData, bool, bool, int> ActionOnPlayEffect;

        /// <summary>
        /// 结束效果
        /// /// </summary>
        public Action<EffectData> ActionOnStopEffect;


        public Action<CameraJson, bool> ActionOnPlayCamera;

        public Action<CameraShakeJson, bool> ActionOnPlayCameraShake;

        public SkillStageFrame(TimeLineFrameEvent timeLineFrameEvent)
        {
            TimeLineTime = timeLineFrameEvent.TimeLineTime;
            FrameTime = timeLineFrameEvent.StageFrameTime;
            Duration = timeLineFrameEvent.Duration;
            FrameEndTime = FrameTime + Duration;
            FrameType = timeLineFrameEvent.StageFrameEventType;
            Value = timeLineFrameEvent.frameData;
            isFrameStart = timeLineFrameEvent.isFrameStart;
            StageIDStr = timeLineFrameEvent.StageIdStr;
            IsRecoverCopyStartFrame = timeLineFrameEvent.IsRecoverCopyStartFrame;
            RecoverJumpTime = timeLineFrameEvent.RecoverJumpTime;

        }

        /// <summary>
        /// 根据 传入的 的 阶段 恢复时间, 去判定 这个事件帧 是否需要 执行.
        ///     1.对于 效果帧 和 非 恢复阶段的事件帧, 直接执行;
        ///     2.对于 恢复阶段的非效果帧, 需要判定 这个效果帧是否在 恢复时间点内. 
        ///         如果在内部,说明这个 效果已经执行完毕了,也就不需要再去 执行事件帧的恢复.
        /// </summary>
        /// <param name="isRecover"></param>
        /// <param name="stageRecoverTime">阶段总的 恢复时间, 有可能 大于整个阶段</param>
        /// <returns></returns>
        public bool CheckIsPlayFrameEvent(bool isRecover, double stageRecoverTime = 0)
        {
            bool isEffectFrame = FrameType == E_StageFrameEventType.Effect;

            // 事件帧 是否可以播放 分为以下两种情况:
            //     1.如果是 效果 类型事件帧, 则 可以直接播放效果帧;
            //     2.如果是 动画/特效 类型的事件帧, 需要检查 事件帧的 长度是否 在阶段的恢复时间内,
            //          如果 事件帧 在阶段的恢复时间内, 那表明 在恢复的时候，这个 事件帧 已经执行完毕,不需要再去执行恢复逻辑
            //          如果 不在, 那就执行这个事件帧
            bool result = isEffectFrame;
            bool isRecoverPlay = isRecover ? FrameEndTime > stageRecoverTime : !IsRecoverCopyStartFrame;

            result = result || isRecoverPlay;
            if (!result)
            {
                //Debug.LogError($"{TagFlag} [xx-xx] check : FrameType : {FrameType} , isFrameStart : {isFrameStart}, isRecover : {isRecover} , startTime : {FrameTime}, duration : {Duration}, frameEndTime : {FrameEndTime}, stageRecoverTime : {stageRecoverTime} ,IsRecoverCopyStartFrame: {IsRecoverCopyStartFrame}, isRecoverPlay: {isRecoverPlay}");
                //Debug.LogError($"{TagFlag} [xx-xx] CheckIsPlayFrameEvent result: {result}");
            }


            return result;
        }

        public void Play(bool isRecover, double stageRecoverTime = 0)
        {
            /// 实际阶段帧 播放时 需要执行的恢复时间, 如果不是恢复阶段的时候, 就不需要执行恢复时间逻辑
            /// note:
            ///     1. 如果要执行 恢复, 那么 说明 事件帧的 时间长度 要大于 阶段的恢复时间.
            ///        所以恢复的时间点 = 事件帧的长度 - (事件帧的结束点 - 阶段的恢复时间点);
            /// 
            ///     2. 对于 效果的恢复, 不遵循上面的计算方式, 效果 恢复需要单独处理
            double starTime = !isRecover ? 0 : (Duration - (FrameEndTime - stageRecoverTime));
            bool recoverLog = isRecover && starTime > 0 || starTime < 0;
            // if (recoverLog)
            // {
            // }
            if (starTime > 0)
            {

                //SGF.Debuger.LogError($"{TagFlag} [xx-xx] Play stageRecoverTime : {stageRecoverTime}, Duration: {Duration}, FrameEndTime :{FrameEndTime}, starTime : {starTime}");
            }

            switch (FrameType)
            {
                case E_StageFrameEventType.Animation://动画轴
                    {
                        //play animation
                        AnimationJson animationJson = (AnimationJson)Value;
                        ActionOnPlayAnimation?.Invoke(animationJson, isFrameStart, (int)starTime);

                        // if (starTime > 500)
                        // {
                        //     Debug.Break();
                        // }
                        if (recoverLog)
                        {
                            //SGF.Debuger.LogError($"{TagFlag} [xx-xx] Play Animation = {animationJson.ClipName} , isFrameStart : {isFrameStart} , isRecover : {isRecover} , starTime : {starTime}");
                        }
                    }
                    break;
                case E_StageFrameEventType.SpecialEffects://特效轴
                    {
                        FXJson fXJson = (FXJson)Value;
                        // 效果 
                        // 只需要处理开始帧
                        // 后续看情况是否需要处理 结束帧
                        if (isFrameStart)
                        {
                            ActionOnPlayFx?.Invoke(fXJson, (int)starTime);
                        }
                    }
                    break;
                case E_StageFrameEventType.Audio://音频轴
                    {
                        SoundJson soundJson = (SoundJson)Value;
                        // 如果 网络 延迟超过100ms, 就认为 音频 是走的恢复逻辑,
                        // 音频 目前没找到 中间点播放的接口, 所以此处 直接不播放
                        if (isFrameStart && starTime <= 100)
                        {
                            ActionOnPlaySound?.Invoke(soundJson, (int)starTime);
                        }
                    }
                    break;
                case E_StageFrameEventType.Effect://效果类型
                    {
                        EffectData effectData = (EffectData)Value;
                        // SGF.Debuger.LogError($"Play effectData {effectData.EffectID} ");
                        /// 效果的 恢复 需要根据 特定的 效果去 单独处理
                        /// note:
                        ///     1.很多效果 配置的 是一个 时间点, 在效果里面开启 一段特效或者动画;
                        ///     2.这种方式,需要根据 实际要求,看是否需要 恢复 特效或者动画;
                        ///     3.在阶段帧 播放的时候,此处并不清楚 特效是否需要恢复, 所以将阶段恢复时间传入效果,由他们自己处理.
                        ActionOnPlayEffect?.Invoke(effectData, isRecover, isFrameStart, (int)stageRecoverTime);
                    }
                    break;
                case E_StageFrameEventType.States://不在帧事件，而在阶段里处理
                    {
                        // SGF.Debuger.LogError($"[SkillStageFrame] time {FrameTime}  play {FrameType} error , no handle with States !!!");

                    }
                    break;
                case E_StageFrameEventType.Camera:
                    {
                        CameraJson cameraJson = (CameraJson)Value;
                        if (isFrameStart)
                        {
                            ActionOnPlayCamera?.Invoke(cameraJson, isFrameStart);
                        }
                    }
                    break;
                case E_StageFrameEventType.CameraShake:
                    {
                        CameraShakeJson cameraJson = (CameraShakeJson)Value;
                        if (isFrameStart && cameraJson != null)
                        {
                            ActionOnPlayCameraShake?.Invoke(cameraJson, isFrameStart);
                        }
                    }
                    break;
                default:
                    {
                        //DB_Close    SGF.Debuger.LogError($"[SkillStageFrame] time {FrameTime}  play {FrameType} error , no handle!!!");
                    }
                    break;
            }
        }

        /// <summary>
        /// 事件帧结束的接口
        /// 根据不同的事件帧类型,做对应的取消逻辑
        /// </summary>
        public void Stop(E_SkillStageExitType type)
        {
            switch (FrameType)
            {
                case E_StageFrameEventType.Animation:
                    {
                        /// 动画帧可能由于其它的技能/动作打断
                        /// 所以进入stop流程的时候,此处并不需要单独去结束动画流程

                        //dont do anything
                        // 如果是被打断,那就结束这个动画
                        if (type == E_SkillStageExitType.Broken)
                        {
                            AnimationJson animationJson = (AnimationJson)Value;
                            ActionOnPlayAnimation?.Invoke(animationJson, false, 0);
                        }
                    }
                    break;
                case E_StageFrameEventType.SpecialEffects:
                    {
                        FXJson fXJson = (FXJson)Value;

                        // 如果阶段 是 执行恢复的 阶段退出, 就不需要结束当前阶段的特效(因为recover本身也不播放特效)
                        if (type == E_SkillStageExitType.Recover)
                        {
                            return;
                        }
                        // type == E_SkillStageExitType.Default || 
                        // 如果是跟随阶段结束, 那 任何情况,理论上 阶段结束,都需要结束这个特效
                        if (fXJson.config.IsChangeCancel)
                        {
                            ActionOnStopFx?.Invoke(fXJson);
                            return;
                        }

                        /// 如果是跟随技能结束,技能被 打断时, 取消特效
                        /// TODO DL
                        /// 后面需要区分 阶段 打断 是 只打断阶段还是 技能的整体打断
                        /// 目前 阶段 被打断 和 技能被打断, type 都是 E_SkillStageExitType.Broken
                        /// note:
                        ///     2022/12/15
                        ///     刚发现一个问题,技能 结束了,特效关闭了,但是 viewAoi中还保留这这个特效的引用.
                        ///     原因就是 此处 特效 的释放条件 是 技能被打断才能释放. 
                        ///     目前跟策划gl 沟通, 设计上这个参数 就是 技能被打断时，才取消这个 特效.
                        ///     所以 此处 不做更改, 在 viewAoi 中 增加 一个 特效关闭时，自己移除引用的机制来处理 特效引用的问题。
                        if (fXJson.config.IsSyncBreak && type == E_SkillStageExitType.Broken)
                        {
                            ActionOnStopFx?.Invoke(fXJson);
                            return;
                        }
                    }
                    break;
                case E_StageFrameEventType.Audio:
                    {
                        /// 2023/3/22
                        /// 需要新增 阶段被打断的时候,音频也被打断的逻辑(目前 主要是蓄力结束,音频也要结束)
                        /// 对于 大部分(非打断)的阶段结束 情况, 音频 不需要关注 结束帧, 正常播放就行.

                        if (type == E_SkillStageExitType.Broken)
                        {
                            SoundJson soundJson = (SoundJson)Value;
                            ActionOnStopSound?.Invoke(soundJson);
                        }

                    }
                    break;
                case E_StageFrameEventType.Effect:
                    {
                        // 效果 如果配置了 IsChangeCancel , 那表明这个效果 需要跟随阶段结束.
                        EffectData effectData = (EffectData)Value;
                        if (effectData.IsStageCancel)
                        {
                            ActionOnStopEffect?.Invoke(effectData);
                        }
                    }
                    break;
                case E_StageFrameEventType.CameraShake:
                    {
                        CameraShakeJson cameraJson = (CameraShakeJson)Value;
                        if (!isFrameStart && cameraJson != null)
                        {
                            ActionOnPlayCameraShake?.Invoke(cameraJson, isFrameStart);
                        }
                    }
                    break;
                case E_StageFrameEventType.States:
                    {
                        // SGF.Debuger.LogError($"[SkillStageFrame] time {FrameTime}  stop {FrameType} error , no handle with States !!!");
                    }
                    break;
                case E_StageFrameEventType.Camera:
                    {
                        //dont do anything
                    }
                    break;
                default:
                    {
                        // DB_Close SGF.Debuger.LogError($"[SkillStageFrame] time {FrameTime}  stop {FrameType} error , no handle!!!");
                    }
                    break;
            }
            // SGF.Debuger.Log($"[SkillStageFrame] time {FrameTime}  stop {FrameType} end");
        }



        /// <summary>
        /// 阶段退出接口
        ///     阶段结束取消根据对于的阶段退出类型，取消对应的阶段效果
        /// </summary>
        /// <param name="type"></param>
        public void OnStageExit(E_SkillStageExitType type)
        {
            switch (type)
            {
                case E_SkillStageExitType.Default:
                    {
                        //正常技能阶段的结束
                        //1.正常阶段结束,不需要做任何事，注册的效果依旧注册等待
                        //dont do anything

                        // 2022/11/30 
                        // 特效参数中也配置了 阶段正常结束时,结束特效的逻辑
                        Stop(type);
                    }
                    break;

                case E_SkillStageExitType.FailedSetMainSkill:
                    {
                        //抢占主位 技能失败，那理论上这个阶段执行不了任何效果
                        // dont do anything
                    }
                    break;
                case E_SkillStageExitType.Broken:
                    {
                        //技能打断
                        // 1. 取消所有的 后续效果队列
                        // 2. 取消之前注册的源自状态等
                        // 3. 取消阶段中配置了随阶段销毁的特效
                        Stop(type);
                    }
                    break;
                case E_SkillStageExitType.Recover:
                    {
                        //技能恢复后的阶段退出，技能恢复阶段就是为了注册一些效果事件
                        //所以技能恢复阶段的退出，并不需要取消 效果消息注册
                        //dont do anything

                        /// 2023/3/22
                        /// 目前 阶段的恢复逻辑存在一些修改:
                        ///     1.阶段执行恢复逻辑的时候,是一帧跑完所有的 阶段逻辑, 此时就需要再play的时候,检测哪些
                        ///       事件帧 需要播放(主要根据 恢复时间 来判断) .
                        ///     2.阶段 只有在 进入时 发现 恢复时间 远超 当前阶段的时候,才会 走 恢复阶段的退出流程.
                        /// 
                        /// 基于上面的情况, 有如下情况:
                        ///     1.恢复阶段 现如今不再仅仅 只是 注册 效果监听, 还会按恢复时间去 播放 动作/特效/音频/效果等;
                        ///     2.阶段 执行 一个事件帧 恢复逻辑的时候, 已经 根据 事件帧 在时间轴上的长度 做了筛选, 只有满足 恢复 时间长度的 才会播放;
                        ///     
                        /// 基于上, 恢复 阶段的 退出 也就不需要 再去 终结 事件帧 的执行(超过恢复时间的事件帧才需要执行)
                        /// 
                        /// 所以 此处依旧是 啥都不用干;
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// 技能结束调用的 结束接口,
        ///     根据技能结束的类型，取消对应的阶段效果
        /// </summary>
        /// <param name="skillExitType"></param>
        public void OnExit(E_SkillExitType skillExitType)
        {
            //技能运行时结束,结束阶段取消所有的效果
            //不管是主动的技能运行时结束 还是被动的技能运行时结束

            switch (skillExitType)
            {
                case E_SkillExitType.Default:
                case E_SkillExitType.ServerDefault:
                    {
                        //正常技能阶段的结束
                        //1.正常阶段结束,不需要做任何事，注册的效果依旧注册等待
                        //dont do anything
                    }
                    break;

                case E_SkillExitType.FailedSetMainSkill:
                    {
                        //抢占主位 技能失败，那理论上这个阶段执行不了任何效果
                        // dont do anything
                    }
                    break;
                case E_SkillExitType.ServerBreakSkill:
                case E_SkillExitType.ClientBreakSkill:
                    {
                        //技能打断
                        // 1. 取消所有的 后续效果队列
                        // 2. 取消之前注册的源自状态等
                        // 3. 取消阶段中配置了随阶段销毁的特效
                        // 技能被打断结束,那阶段就是走 Broken
                        Stop(E_SkillStageExitType.Broken);
                    }
                    break;
                case E_SkillExitType.Reset:
                    {
                        Stop(E_SkillStageExitType.Broken);
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }

        public void Release()
        {
            FrameTime = 0;
            FrameType = E_StageFrameEventType.Default;
            Value = null;
            isFrameStart = true;

            ActionOnPlayAnimation = null;
            ActionOnEndAnimation = null;
            ActionOnPlayFx = null;
            ActionOnStopFx = null;
            ActionOnPlaySound = null;
            ActionOnStopSound = null;
            ActionOnPlayEffect = null;
            ActionOnStopEffect = null;
            ActionOnPlayCamera = null;
        }
    }


}
