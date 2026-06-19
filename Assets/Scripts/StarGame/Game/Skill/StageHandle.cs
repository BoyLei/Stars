using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using SkillEditor;
using StarProjectDef;
using StarProject.CustomDataStruct;
using SGF.Unity;
using StarProject.Game.Data;
using EffectData = SkillEditor.EffectData;
using System.Text;


namespace StarProject.Game.Skill
{
    /// <summary>
    /// 处理阶段事件的中间层,对下用来连接 skillStage 层,对上用来连接 skillEntity/skillBuff 等.
    /// 是一个通用的效果的中间层
    /// </summary>
    public abstract class StageHandle : EntityRemoteStatic
    {
        public virtual string TagFlag
        {
            get => $"[StageHandle]";
        }
        /// <summary>
        /// stageHandle层持有的黑板,如果StageHandle是 skill, 那 handleBlackBoard 就是技能黑板
        /// 如果是 buff， 那 handleBlackBoard 就是 buff 黑板
        /// </summary>
        private BaseBlackBoard _handleBlackBoard;
        private static StringBuilder sb = new StringBuilder(32);

        private ulong _builderID = 0;
        public ulong BuilderID => _builderID;

        private ulong _ownerEntityID = 0;
        public ulong OwnerEntityID => _ownerEntityID;


        private ulong _runtimeID;

        public ulong RuntimeID => _runtimeID;

        protected E_StageType playStageType = E_StageType.None;

        /// <summary>
        /// 阶段播放的类型,根据子类不同的类型,再构建函数中自己去设置它的 PlayStageType
        /// </summary>
        public E_StageType PlayStageType => playStageType;

        private HashSet<string> histroyKeySet = new HashSet<string>();


        public Action<string, List<int>, bool> ActionOnRefreshStates;
        /// <summary>
        /// 技能 尝试设置自己为 主动技能的 Func
        /// <SkillStage, bool, bool> -----> <this , isMain , result>
        /// </summary>
        public Func<SkillStage, bool, bool, bool> FuncOnSkillTrySetActiveSkill;


        /// <summary>
        /// 技能播放动画接口
        /// <I_AnimParam, bool>  -----><SkillEntity, I_AnimParam, isFrameStart>
        /// </summary>
        public Action<SkillStage, I_AnimParam, bool> ActionOnPlayAnim;

        public Action<I_FxParam> ActionOnPlayFx;

        public Action<I_AudioParam> ActionOnPlayAudio;
        public Action<I_AudioParam> ActionOnStopAudio;

        public Action<I_FxParam> ActionOnStopFx;

        public Action<SkillEntity, int, E_CameraEffectType> ActionOnPlayCamera;

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


        /// <summary>
        ///  DelayInvoke 播放effect 的唯一 关键key的生成接口
        /// </summary>
        /// <param name="effectID"></param>
        /// <returns></returns>
        public abstract string FormatEffectKey(int effectID);

        /// <summary>
        /// 技能阶段的尝试 设置自己为 主动阶段 时调用的接口
        /// </summary>
        /// <param name="isActiveMain">此技能阶段 需要设置 主动 还是取消主动</param>
        /// <returns></returns>
        public abstract bool OnActionStageTrySetActiveMain(SkillStage skillStage, double enterStageTime, bool isActiveMain, bool force);

        /// <summary>
        /// 退出这个阶段 执行的Action
        /// <exitStageTime ,E_SkillStageExitType> : <退出阶段的时间 , 退出阶段的类型>
        /// </summary>
        public Action<SkillStage, double, E_SkillStageExitType> ActionOnExitStage;

        /// <summary>
        /// 判断是否是 人物摇杆移动的接口 
        /// 是否是摇杆移动,影响 主角自身 阶段 非活跃动画的播放逻辑
        /// 如果阶段 不抢占活跃,而此时 主角又是摇杆移动,那么人物的移动,在不同类型的阶段,可能移动打断阶段的动画
        /// </summary>
        public abstract bool IsJoySitckMoving();

        /// <summary>
        /// 跟随玩家 阶段循环的 特效/动画 key  的记录， 用来 标识 这个特效/动画 已经播放过. 一般是  阶段进入时就 播放的自己循环的动画
        /// </summary>
        private HashSet<string> _followLoopKeyRecord = new HashSet<string>();

        public void Init(BaseBlackBoard baseBlackBoard, ulong runtimeID, ulong builder, ulong owener)
        {
            OnCreate();

            _runtimeID = runtimeID;
            _builderID = builder;
            _ownerEntityID = owener;
            _handleBlackBoard = baseBlackBoard;

            // Debug.Log($"[stageHandle] create runtimeID: {runtimeID}, playStageType: {playStageType} hash: {this.GetHashCode()} ");
        }

        public virtual void OnCreate()
        {

        }

        public void RegisterStage(SkillStage skillStage)
        {
            skillStage.ActionOnExitStage += OnActionExitStage;
            skillStage.ActionOnStageStartCD += OnActionStageStartCD;
            skillStage.ActionOnRefreshStageStates += OnActionRefreshStageStates;

            skillStage.FuncOnStageTrySetActiveMain += OnActionStageTrySetActiveMain;

            skillStage.FuncOnStageTryPlayAnim += OnFuncStageTryPlayAnim;
            skillStage.ActionOnStageTryPlayFx += OnActionStageTryPlayFx;
            skillStage.ActionOnStageTryPlaySound += OnActionStageTryPlaySound;
            skillStage.ActionOnStageTryStopSound += OnActionStageTryStopSound;

            skillStage.ActionOnStageTryStopFx += OnActionStageTryStopFx;

            skillStage.ActionOnStageTryPlayEffect += OnActionStageTryPlayEffect;
            skillStage.ActionOnStageRegisterServerEffect += OnActionStageRegisterServerEffect;
            skillStage.FuncOnTryPlayRegistedServerEffect += TryPlayRegistedServerEffect;

            // 暂时屏蔽,老的 摄像机震屏 等逻辑的接口
            // skillStage.ActionOnStageTryPlayCamera += OnActionStageTryPlayCamera;

            skillStage.ActionOnStageTryPlayCameraShake += OnActionOnStageTryPlayCameraShake;

            skillStage.ActionOnStageWriteSkillEffect += OnActionStageWriteSkillEffect;
        }

        public void UnRegisterStage(SkillStage skillStage)
        {
            // stage release 的时候会自己曲 将action=null。 所以此处应该不需要
            return;
            skillStage.ActionOnExitStage -= OnActionExitStage;
            skillStage.ActionOnStageStartCD -= OnActionStageStartCD;
            skillStage.ActionOnRefreshStageStates -= OnActionRefreshStageStates;

            skillStage.FuncOnStageTrySetActiveMain -= OnActionStageTrySetActiveMain;

            skillStage.FuncOnStageTryPlayAnim -= OnFuncStageTryPlayAnim;
            skillStage.ActionOnStageTryPlayFx -= OnActionStageTryPlayFx;
            skillStage.ActionOnStageTryPlaySound -= OnActionStageTryPlaySound;
            skillStage.ActionOnStageTryStopFx -= OnActionStageTryStopFx;

            skillStage.ActionOnStageTryPlayEffect -= OnActionStageTryPlayEffect;
            skillStage.ActionOnStageRegisterServerEffect -= OnActionStageRegisterServerEffect;
            skillStage.FuncOnTryPlayRegistedServerEffect -= TryPlayRegistedServerEffect;

            // 暂时屏蔽,老的 摄像机震屏 等逻辑的接口
            // skillStage.ActionOnStageTryPlayCamera -= OnActionStageTryPlayCamera;

            skillStage.ActionOnStageTryPlayCameraShake -= OnActionOnStageTryPlayCameraShake;

            skillStage.ActionOnStageWriteSkillEffect -= OnActionStageWriteSkillEffect;
        }


        /// <summary>
        /// 退出阶段的逻辑,子类可以 Override
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="exitStageTime"></param>
        /// <param name="skillStageExitType"></param>
        public virtual void OnActionExitStage(SkillStage skillStage, double exitStageTime, E_SkillStageExitType skillStageExitType)
        {
            bool canReleaseStage = CheckSkillStageCanRelease(skillStageExitType);
            // SGF.Debuger.LogError($"{TagFlag} OnActionExitStage : stage[{skillStage.StageIDStr}] , exitStageTime : {exitStageTime} , skillStageExitType : {skillStageExitType} , canReleaseStage : [{canReleaseStage}]");
            if (!CheckSkillStageCanRelease(skillStageExitType))
            {
                return;
            }

            ActionOnExitStage?.Invoke(skillStage, exitStageTime, skillStageExitType);
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


        public virtual void OnActionStageStartCD(SkillStage skillStage)
        {
            //DB_Close       SGF.Debuger.Log($"{TagFlag} OnActionStageStartCD : stage[{skillStage.StageID}] ");
        }


        /// <summary>
        /// 阶段刷新 原子状态的接口，skill/buff 都是直接对人处理，
        /// 但是 对于 子弹bullet，它是独立的 实体, 所以 对子弹的处理逻辑单独分离。
        /// </summary>
        /// <param name="stageID"></param>
        /// <param name="states"></param>
        /// <param name="regist"></param>
        public void OnActionRefreshStageStates(string stageID, List<int> states, bool regist)
        {
            ActionOnRefreshStates.Invoke(stageID, states, regist);
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
            // 如果是开始帧,就是正常的播放动画的逻辑
            // 就需要有单独自己的动画播放逻辑判断
            if (isFrameStart)
            {
                if (!CheckNeedExecuteStageFrame(skillStage))
                {
                    return false;
                }
            }

            animParam.InitAnimationJson(animationJson, PlayStageType);
            // 动画的结束帧 可能会超过阶段,导致 结束帧 对不上 动画的开始帧
            animParam.SetExtraKey(RuntimeID.ToString());
            animParam.SetStartTime(startTime);
            ActionOnPlayAnim?.Invoke(skillStage, animParam, isFrameStart);
            return true;
        }

        private bool CheckNeedExecuteStageFrame(SkillStage skillStage)
        {
            //note:
            //  如果自己是需要抢位技能,即active为true
            //  由于抢位逻辑的存在,当 收到阶段 playAni 的Action的时候,抢位一定是成功的
            //  抢位失败的 阶段 会被 打断阶段


            // 当技能阶段为 非主位阶段,且当前角色正在 摇杆移动的时候,不需要播放动作
            if (!skillStage.NeedActive && IsJoySitckMoving())
            {
                return false;
            }

            return true;
        }

        FxParam fxParam = new FxParam();

        private void OnActionStageTryPlayFx(SkillStage skillStage, FXJson fXJson, int fxStartTime)
        {

            // 特效：如果不跟随阶段循环。第一次执行，后面的直接return
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

            fxParam.InitWithFxJson(fXJson, BuilderID, OwnerEntityID);
            fxParam.SetExtralKey($"{RuntimeID.ToString()}_[{skillStage.StageIDStr}]");
            fxParam.SetFxStartTime(fxStartTime);

            ActionOnPlayFx?.Invoke(fxParam);
        }


        private void OnActionStageTryStopFx(SkillStage skillStage, FXJson fXJson)
        {
            fxParam.InitWithFxJson(fXJson, BuilderID, OwnerEntityID);
            fxParam.SetExtralKey($"{RuntimeID.ToString()}_[{skillStage.StageIDStr}]");


            ActionOnStopFx?.Invoke(fxParam);
        }

        AudioParam audioParam = new AudioParam();

        //层次开放太多了，层层可以多个
        private void OnActionStageTryPlaySound(SkillStage skillStage, SoundJson soundJson, int startTime)
        {
            audioParam.Init(soundJson, startTime, 0);

            ActionOnPlayAudio?.Invoke(audioParam);
        }

        private void OnActionStageTryStopSound(SkillStage skillStage, SoundJson soundJson)
        {
            audioParam.Init(soundJson, 0, 0);
            ActionOnStopAudio?.Invoke(audioParam);
        }

        // EffectParam effectParam = new EffectParam();
        private void OnActionStageTryPlayEffect(SkillStage skillStage, EffectData effectData, bool isRecover, bool isFrameStart, int stageRecoverTime)
        {
            PlayStageEffect(skillStage, effectData, isRecover, isFrameStart);
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
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="effectData"></param>
        /// <param name="isRecover"> 是否是阶段复原的播放效果</param>
        /// <param name="isFrameStart"> 是否是播放效果的首帧</param>
        private void PlayStageEffect(SkillStage skillStage, EffectData effectData, bool isRecover, bool isFrameStart)
        {
            EffectParam newEffectParam = new EffectParam();
            newEffectParam.Init(effectData, BuilderID, OwnerEntityID, PlayStageType, $"{RuntimeID}_{skillStage.StageIDStr}");
            newEffectParam.SetExtraData(skillStage);

            TryPlayEffect(skillStage, newEffectParam, isRecover, isFrameStart);

            // 效果的结束帧 一般都是走 特殊的结束逻辑 (比如输入轴的结束帧,有单独的处理逻辑)
            // 对于Next的效果,目前 是放在 效果的首帧处理
            if (!isFrameStart)
            {
                return;
            }

            PreRegPlayEffectNext(skillStage, newEffectParam, isRecover, false);
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
            // if (effectParam.SkillEffectType == E_SkillEffect.IsNotEmpty)
            // {
            //     SGF.Debuger.LogError($"{TagFlag} [x-d] effectID : {effectParam.EffectID} CheckNeedPreRgeNextEffect isWaitServerRegNext : {isWaitServerRegNext} , isServerLine : {isServerLine} , result : {result}");
            // }

            return result;
        }

        /// <summary>
        /// 提前注册 Next效果 
        /// note: 
        ///     2022/12/26 增加 一个判断类 效果 需要等待服务器 消息回来后 执行后面的逻辑
        ///     所以 PreRegPlayEffectNext 扩展为 不仅在客户端线 执行,同时 在 服务器线 也可以触发 next, 
        ///     通过 提前注册 IsWaitServerRegNext  来处理
        /// </summary>
        /// <param name="skillStage"></param>
        /// <param name="i_EffectParam"></param>
        /// <param name="isRecover"></param>
        /// <param name="isServerLine">是否是 服务器线触发</param>
        private void PreRegPlayEffectNext(SkillStage skillStage, I_EffectParam i_EffectParam, bool isRecover, bool isServerLine)
        {
            List<int> next = i_EffectParam.Next;
            if (next == null) return;
            BaseBlackBoard baseBlackBoard = i_EffectParam.SaveSkill ? (BaseBlackBoard)_handleBlackBoard : skillStage.StageBlackBoard;

            bool needPreRegNextEffect = CheckNeedPreRgeNextEffect(baseBlackBoard, i_EffectParam, isServerLine);
            if (!needPreRegNextEffect)
            {
                return;
            }


            // // 策划配置的效果的执行时间
            // int nextEffectStartTime = effectData.EffectEndTime - effectData.ClinetExecuteTime;

            // 新约定, 夏哥说 为了服务器效果能够提前计算, 所以 通过 ServerExecuteTime 和 ClinetExecuteTime 来区分效果的开始时间,
            // ServerNextDelayTime 约定为 效果的 执行时长.
            // 所以 客户端 的下个效果执行时间 是 基于当前时间 + ServerNextDelayTime
            int nextEffectStartTime = i_EffectParam.ServerNextDelayTime;
            //if (effectData.EffectArgs.MoveWithRot != null)
            //{
            //    SGF.Debuger.Log($"黑板位移 PreRegPlayEffectNext nextEffectStartTime={nextEffectStartTime},DurningTime={effectData.EffectArgs.MoveWithRot.DurningTime}");
            //}
            // NEXT_TRUE 的效果分支
            bool IsNeedRegNextTrue = EffectResultUtils.IsRegNext(baseBlackBoard, i_EffectParam, true);
            if (IsNeedRegNextTrue)
            {
                if (next.Count > 0)
                {
                    int trueEffectID = next[0];
                    EffectData trueEffectData = skillStage.GetEffectData(trueEffectID);
                    if (trueEffectData != null)
                    {
                        //SGF.Debuger.Log($"{TagFlag}  PreRegPlayEffectNext trueEffectData  {trueEffectID} ");
                        PlayNextEffect(skillStage, trueEffectData, isRecover, nextEffectStartTime);
                    }
                    else
                    {
                        //SGF.Debuger.LogError($"{TagFlag}  PreRegPlayEffectNext trueEffectData  {trueEffectID} not find!!! ");
                    }
                }
            }

            // NEXT_FALSE 的效果分支
            bool IsNeedRegNextFalse = EffectResultUtils.IsRegNext(baseBlackBoard, i_EffectParam, false);
            if (IsNeedRegNextFalse)
            {
                if (next.Count > 1)
                {
                    int falseEffectID = next[1];
                    EffectData falseEffectData = skillStage.GetEffectData(falseEffectID);
                    //DB_Close       SGF.Debuger.Log($"{TagFlag}  PreRegPlayEffectNext falseEffectID  {falseEffectID} ");

                    if (falseEffectData != null)
                    {
                        PlayNextEffect(skillStage, falseEffectData, isRecover, nextEffectStartTime);
                    }
                    else
                    {
                        //SGF.Debuger.LogError($"{TagFlag}  PreRegPlayEffectNext falseEffectID  {falseEffectID} not find!!! ");
                    }
                }
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
        private bool TryPlayEffect(SkillStage skillStage, I_EffectParam i_EffectParam, bool isRecover, bool isFrameStart)
        {
            BaseBlackBoard baseBlackBoard = i_EffectParam.SaveSkill ? _handleBlackBoard : skillStage.StageBlackBoard;

            // 如果是 效果的结束帧,不需要取执行 服务器效果
            if (!isFrameStart)
            {
                //dont do TryPlayServerEffect
            }
            else
            {
                // 先在黑板中注册 当前 服务器效果配置 
                // 服务器效果 在每次 效果执行的时候,先注册
                // 注册的时候,用 clone 的数据, 因为 可能在 某一时刻才执行
                RegisterServerEffect(skillStage, i_EffectParam.EffectData);

                TryPlayServerEffect(i_EffectParam, baseBlackBoard);
            }

            // 如果是复原阶段,不需要播放 客户端效果
            if (isRecover)
            {
                return true;
            }

            bool result = FuncOnTryPlayClientEffect.Invoke(PlayStageType, RuntimeID, i_EffectParam, baseBlackBoard, isFrameStart, BuilderID, OwnerEntityID);

            // 客户端执行完效果后,刷新 客户端 效果黑板的 executeResult
            EffectResultUtils.UpdateEffectExecuteResult(baseBlackBoard, i_EffectParam, true, result);

            return result;
        }

        /// <summary>
        /// 客户端尝试 播放服务器效果线的逻辑
        /// </summary>
        /// <param name="i_EffectParam"></param>
        /// <param name="baseBlackBoard"></param>
        private void TryPlayServerEffect(I_EffectParam i_EffectParam, BaseBlackBoard baseBlackBoard)
        {
            string outputKey = i_EffectParam.OutputKey;

            // 客户端检查 服务器的 效果数据是否已经全部同步过来
            // 查看阶段中是否含有这个效果数据  (服务器黑板)
            object v = baseBlackBoard.Get(outputKey, E_BlackBoardTag.Server);
            // 如果 服务器黑板中 找不到 key 关联的数据,那结束
            if (v == null)
            {
                //DB_Close       SGF.Debuger.Log($"{TagFlag}  [client] TryPlayServerEffect : {i_EffectParam.EffectID} no data !!!");
                return;
            }

            // 如果找到了服务器黑板数据,那就执行这个效果,如果效果执行失败,那就结束
            // 如果 服务器效果没播放成功,那就注册这个效果 到黑板数据中
            bool result = TryPlayRegistedServerEffect((CustomBlackBoardNode)v, baseBlackBoard);

            // 执行完服务器效果后,刷新 服务器 效果黑板的 executeResult
            EffectResultUtils.UpdateEffectExecuteResult(baseBlackBoard, i_EffectParam, false, result);


            if (!result)
            {
                //DB_Close       SGF.Debuger.Log($"{TagFlag}  [client] TryPlayServerEffect : {i_EffectParam.EffectID} result : {result}");
                return;
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
        private void PlayNextEffect(SkillStage skillStage, EffectData effectData, bool isRecover, int effectStartTime)
        {
            string key = FormatEffectKey(effectData.EffectID);
            histroyKeySet.Add(key);

            float delayTime = (effectStartTime + effectData.ClinetExecuteTime) / 1000.0f;

            object[] Os = new object[] { skillStage, effectData, isRecover, this.GetHashCode() };
            string stageIDStr = skillStage.StageIDStr;
            DelayInvoker.DelayInvoke(key, delayTime,
                (object[] args) =>
                {
                    SkillStage skillStage1 = (SkillStage)Os[0];
                    EffectData effectData1 = (EffectData)Os[1];
                    bool isRecover1 = (bool)Os[2];
                    // SGF.Debuger.LogError($"{TagFlag} [stagehandle]  PlayNextEffect stage {stageIDStr}  effectID {effectData1.EffectID} , stageHandle: {Os[3]} ");
                    if (skillStage1.StageID == 0)
                    {
                        SGF.Debuger.Log($"{TagFlag}  PlayNextEffect stage {stageIDStr}  effectID {effectData1.EffectID}  阶段 被销毁 ");
#if UNITY_EDITOR
                        //Debug.Break();
#endif
                        return;
                    }
                    PlayStageEffect(skillStage1, effectData1, isRecover1, true);
                }
            , Os);
        }


        private void RegisterServerEffect(SkillStage skillStage, EffectData effectData)
        {
            // Note:
            // 注册服务器 的效果参数可能 存在黑板中延迟处理
            // 所以此处 effectParam 是 每次都重新 new 一个
            // 后面考虑 EffectParam 弄一个工厂模式
            EffectParam effectParam = new EffectParam();
            effectParam.Init(effectData, BuilderID, OwnerEntityID, PlayStageType, $"{RuntimeID}_{skillStage.StageIDStr}");

            // TODO: DL
            // 临时方案,目前服务器 发给客户端的 黑板数据 是效果的 outputkey.
            // note:
            //     1.如果策划配置了效果写入技能黑板,服务器 黑板数据会通过技能运行时同步过来.
            //     2.当黑板数据通过技能同步过来时,客户端需要定位到具体效果在什么阶段.
            //       例如输入轴这种效果,收到黑板数据后,客户端需要关闭输入轴.
            //       因此,客户端需要根据黑板数据定位到 这个效果 到底在哪.
            //       所以目前会在客户端的注册黑板中,将 skillStage 也存下来,等收到服务器数据的时候,不需要反向定位
            //
            effectParam.SetExtraData(skillStage);

            // 根据数据 是SaveSkill,知道 要操作哪个黑板
            // BaseBlackBoard regBlackBoard = effectData.SaveSkill ? (BaseBlackBoard)skillBlackBoard : skillStage.StageBlackBoard;
            // TODO : DL
            // 要看看怎么搞
            BaseBlackBoard regBlackBoard = skillStage.StageBlackBoard;

            string outputKey = effectParam.OutputKey;
            //DB_Close       SGF.Debuger.Log($"{TagFlag}  [client]  stage : {skillStage.StageIDStr}  RegisterServerEffect : {effectParam.EffectID} , outputKey : {outputKey} , isSkill : {effectData.SaveSkill} ");

            EffectUtils.RegisterOutputKeyEffect(regBlackBoard, effectParam);

        }


        /// <summary>
        /// 技能尝试播放服务器效果
        /// note：
        ///     1.每个效果的 outPutkey 都可能是List<string> 格式.
        ///     2.对于客户端来说，每次 收到一个服务器的 BlackBoardNode的时候,都需要 TryPlayRegistedServerEffect 尝试播放这个效果.
        ///     3.只有这个效果依赖 的 所有的 outPutkey 数据都齐全的时候, TryPlayRegistedServerEffect 才会成功;
        ///     4.所以 FuncOnTryPlayServerEffect 播放效果的时候，需要检查它 依赖的 所有的 outPutkey。
        /// </summary>
        /// <param name="serverCustomBlackBoardNode">单个黑板数据</param>
        /// <param name="baseBlackBoard">阶段黑板</param>
        /// <returns>技能播放服务器效果结果</returns>
        public bool TryPlayRegistedServerEffect(CustomBlackBoardNode serverCustomBlackBoardNode, BaseBlackBoard baseBlackBoard)
        {
            string key = serverCustomBlackBoardNode.Key;
            if (baseBlackBoard == null)
            {
                //SGF.Debuger.LogError($"{TagFlag}  TryPlayRegistedServerEffect : key={key},baseBlackBoard is null !!!");
                return false;
            }
            /// <summary>
            /// 先从 注册黑板中取对于的效果配置数据(I_EffectParam)
            /// </summary>
            object v = baseBlackBoard.Get(key, E_BlackBoardTag.Reg);

            if (v == null)
            {
                //DB_Close       SGF.Debuger.Log($"{TagFlag}  TryPlayRegistedServerEffect : key = {key} , reg baseBlackBoard is null !!!");
                return false;
            }
            CusListQueue<I_EffectParam> regEffectParams = (CusListQueue<I_EffectParam>)v;
            if (regEffectParams.Count == 0)
            {
                //DB_Close       SGF.Debuger.Log($"{TagFlag} TryPlayRegistedServerEffect : key = {key} , reg baseBlackBoard is null !!!");
                return false;
            }

            bool result = false;
            // 找到 reg 黑板中 outputkey 注册的第一个 效果参数
            I_EffectParam i_EffectParam = regEffectParams.Peek();


            //先 更新服务器黑板数据的result
            EffectResultUtils.UpdateServerEffectResult(i_EffectParam, serverCustomBlackBoardNode);

            //如果阶段收到了 效果黑板数据,那 一定从 阶段黑板中 查找数据
            result = FuncOnTryPlayServerEffect.Invoke(PlayStageType, RuntimeID, i_EffectParam, serverCustomBlackBoardNode, baseBlackBoard);


            //DB_Close       SGF.Debuger.Log($"{TagFlag}  TryPlayRegistedServerEffect : key = {key} , effectID : {i_EffectParam.EffectID} , result {result}");

            //如果效果执行成功 , 删除黑板中 存在的 注册的 outPutKey
            if (result)
            {
                // 只有在服务器效果 执行为 true的时候,才需要去 检查是否需要 触发这个效果Next的注册
                {
                    SkillStage skillStage = (SkillStage)i_EffectParam.ExtraData;
                    if (skillStage != null)
                    {
                        PreRegPlayEffectNext(skillStage, i_EffectParam, false, true);
                    }
                }
                EffectUtils.UnRegisterOutputKeyEffect(baseBlackBoard, key, i_EffectParam);
            }
            return result;
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

        private void OnActionOnStageTryPlayCameraShake(SkillStage skillStage, CameraShakeJson cameraShake, bool start)
        {
            GlobalEvent.OnVirtualCameraShakeEvent.Invoke(start, cameraShake);
        }


        private void OnActionStageWriteSkillEffect(SkillStage skillStage, string key, object v, E_BlackBoardTag blackBoardTag)
        {
            //DB_Close       SGF.Debuger.Log($"{TagFlag}  OnActionStageWriteSkillEffect : key = {key} ,  blackBoardTag {blackBoardTag}");
            _handleBlackBoard.Set(key, v, blackBoardTag);
        }

        public void ReleaseAction()
        {
            ActionOnRefreshStates = null;
            FuncOnSkillTrySetActiveSkill = null;
            ActionOnPlayAnim = null;
            ActionOnPlayFx = null;
            ActionOnPlayAudio = null;
            ActionOnStopAudio = null;
            ActionOnStopFx = null;
            ActionOnPlayCamera = null;
            FuncOnTryPlayClientEffect = null;
            FuncOnTryPlayServerEffect = null;
        }

        public virtual void Reset()
        {
            _handleBlackBoard = null;

            _builderID = 0;
            _ownerEntityID = 0;
            _runtimeID = 0;
            playStageType = E_StageType.None;

            foreach (string item in histroyKeySet)
            {
                DelayInvoker.CancelInvoke(item);
            }


            _followLoopKeyRecord.Clear();
        }

        protected override void Release()
        {
            // Debug.Log($"[stageHandle] Release runtimeID: {_runtimeID}, playStageType: {playStageType}  hash: {this.GetHashCode()} ");

            base.Release();

            Reset();

            ReleaseAction();

        }
    }


}
