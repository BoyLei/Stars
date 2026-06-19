using Animancer;
using Animancer.FSM;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.Time;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalSummonAnim : ViewVitalSummonNormal, I_VVitalAnim
    {
        private string LogFlag = "[ViewVitalSummonAnim]";

        private readonly StateMachine<VitalState>.WithDefault _StateMachine = new();
        public StateMachine<VitalState>.WithDefault StateMachine => _StateMachine;

        private E_ULayerSubState currentState = E_ULayerSubState.Default;
        public E_ULayerSubState CurrentState
        {
            get => currentState;
            set
            {
                currentState = value;
            }
        }

        private VitalState Stage_Idle;
        private List<VitalState> CommonVitalStates = new();

        private VitalState NextVitalState;
        private bool m_IsInit = true;
        // 标识脏状态-》下一帧切换
        private bool dirtyState = false;
        private long m_LastChanageTime = 0;

        private string StandAnimDelayKey = string.Empty;

        protected override void Create(EntityObject entity)
        {
            //find，手绑定editor，工具
            BindState();
            StandAnimDelayKey = FxUtils.GetDelayStandFlag(this);
            base.Create(entity);

            m_IsInit = false;
        }

        protected override void Release()
        {
            CommonVitalStates.Clear();

            m_IsInit = true;

            DelayInvoker.CancelInvoke(StandAnimDelayKey);
            StandAnimDelayKey = string.Empty;

            base.Release();
        }

        protected override void OnEventListener()
        {
            base.OnEventListener();

            if (M_EntityBase != null)
            {
                M_EntityBase.ActionOnPlaySpecialEffects += PlayFx;
                M_EntityBase.ActionOnStopSpecialEffects += BaseStopFx;

                if (M_EntityBase.Data.hasModel)
                {
                    OnRefreshCurChangeAnims(null);
                    Stage_Idle.IdleState += OnIdleStateAction;
                    StateMachine.DefaultState = Stage_Idle;
                    CurrentState = E_ULayerSubState.Idle;

                    M_EntityBase.AnimStatePlay += PlayAnim;
                    M_EntityBase.ViewEnterFrameAction += OnEnterFrame;

                    M_EntityBase.SpeedChangeAction += OnSpeedChangeAction;
                    M_EntityBase.Data.ActionRefreshCurChangeAnims += OnRefreshCurChangeAnims;
                    M_EntityBase.Data.ActionRefreshCurAnims += RefreshCurStateAnim;
                }
            }
        }

        protected override void OffEventListener()
        {
            base.OffEventListener();

            if (M_EntityBase != null)
            {
                M_EntityBase.ActionOnPlaySpecialEffects -= PlayFx;
                M_EntityBase.ActionOnStopSpecialEffects -= BaseStopFx;

                if (M_EntityBase.Data.hasModel)
                {
                    OnRefreshCurChangeAnims(null);
                    Stage_Idle.IdleState -= OnIdleStateAction;

                    M_EntityBase.AnimStatePlay -= PlayAnim;
                    M_EntityBase.ViewEnterFrameAction -= OnEnterFrame;

                    M_EntityBase.SpeedChangeAction -= OnSpeedChangeAction;
                    M_EntityBase.Data.ActionRefreshCurChangeAnims -= OnRefreshCurChangeAnims;
                    M_EntityBase.Data.ActionRefreshCurAnims -= RefreshCurStateAnim;
                }
            }
        }

        private void EnterNewState(VitalState newState)
        {
            if (StateMachine.CurrentState != null && StateMachine.CurrentState.Clip != null)
            {
                bool isSkillState = newState.AnimState == E_ULayerSubState.Skill_Common1 || newState.AnimState == E_ULayerSubState.Skill_Common2;

                if (isSkillState && CommonVitalStates[0].Clip == CommonVitalStates[1].Clip)
                {
                    if (StateMachine.CurrentState.Character.Animancer.States.TryGet(newState.Key, out AnimancerState nowState))
                    {
                        newState.isNeedPlayFromStart = true;
                        // newState.FadeTimeMilSeconds = 0f;
                    }
                }
            }
            newState.SetStateDefaultFadeTime(currentState);
            if (newState.isNeedPlayFromStart)
            {
                StateMachine.TryResetState(newState);
            }
            else
            {
                StateMachine.TrySetState(newState);
            }
        }

        private void OnEnterFrame(Vector3 V)
        {
            if (dirtyState)
            {
                if (NextVitalState != null)
                {
                    dirtyState = false;
                    EnterNewState(NextVitalState);
                }
            }
        }

        private void BindState()
        {
            Stage_Idle = transform.Find("ModelOffset/StateMachines/Idle").GetComponent<VitalState>();
            Stage_Idle.Character = this;

            var Common_1 = transform.Find("ModelOffset/StateMachines/Common_1").GetComponent<VitalState>();
            var Common_2 = transform.Find("ModelOffset/StateMachines/Common_2").GetComponent<VitalState>();
            Common_1.Character = this;
            Common_2.Character = this;
            CommonVitalStates.Add(Common_1);
            CommonVitalStates.Add(Common_2);
        }

        #region 【委托回调】

        public void PlayAnim(E_ULayerSubState subState, I_AnimParam animParam, Action endStateCB)
        {
            //SGF.Debuger.LogError($"状态切换 PlayAnim() currentState={currentState},,subState={subState}");
            long curTimeStamp = TimeManager.Instance.GetServerTimeStamp();
            bool changeState = true;
            switch (subState)
            {
                case E_ULayerSubState.StandBy:
                case E_ULayerSubState.Idle:
                case E_ULayerSubState.BattleIdle:
                case E_ULayerSubState.WanderMoving:
                case E_ULayerSubState.SingleMoving:
                case E_ULayerSubState.BattleMoving:
                case E_ULayerSubState.Deading:
                case E_ULayerSubState.Hurt:
                case E_ULayerSubState.InterAction1:
                case E_ULayerSubState.InterAction2:
                case E_ULayerSubState.Skill_Common1:
                case E_ULayerSubState.Skill_Common2:
                    {

                        string animName = GetAnimNameByState(subState, animParam);
                        Action<AnimationClip> cb = (animationClip) =>
                        {
                            VitalState vitalState = GetFreeVitalState(animParam.AnimationPath);
                            if (vitalState != null)
                            {
                                AsyncLoadAnimCallBack(curTimeStamp, subState, vitalState, animationClip, animParam, endStateCB, false);
                            }
                        };
                        RefreshStateAnim(animName, cb);
                    }
                    break;
                case E_ULayerSubState.Performance:
                    {

                        string animName = GetAnimNameByState(subState, animParam);
                        Action<AnimationClip> cb = (animationClip) =>
                        {
                            VitalState vitalState = GetFreeVitalState(animParam.AnimationPath);
                            if (vitalState != null)
                            {
                                AsyncLoadAnimCallBack(curTimeStamp, subState, vitalState, animationClip, animParam, endStateCB, true);
                            }
                        };
                        RefreshStateAnim(animName, cb);
                    }
                    break;
                case E_ULayerSubState.WeaponRetractionIdle: // idle收刀动作只有主角有
                case E_ULayerSubState.WeaponRetractionMoving: // move收刀动作只有主角有
                case E_ULayerSubState.BegineKnockDown:
                case E_ULayerSubState.KnockDown:
                case E_ULayerSubState.EndKnockDown:
                case E_ULayerSubState.IsSonscious:
                case E_ULayerSubState.WeakNess:
                case E_ULayerSubState.BeingControl:
                    {
                        // 目前没动作 
                        changeState = false;
                    }
                    break;
                default:
                    changeState = false;
                    break;
            }
            if (!changeState)
            {
                endStateCB?.Invoke();
            }
        }

        private void AsyncLoadAnimCallBack(long chanageTimeStamp, E_ULayerSubState subState, VitalState curLayserState, AnimationClip animationClip, I_AnimParam animParam, Action endStateCB, bool isNeedPlayFromStart)
        {
            if (chanageTimeStamp < m_LastChanageTime)
            {
                // 异步加载到动画文件时间太旧了，之后已经又切换其他了
                return;
            }
            // 这里但凡上层推下来切换的状态，这里就好改
            m_LastChanageTime = chanageTimeStamp;
            endStateCB?.Invoke();
            if (curLayserState == null)
            {
                // 状态机没有就算了
                return;
            }
            // 是否刷新动画, 只有 切换状态了 并且找到动画资源了,才需要切换状态动画
            if (animationClip == null)
            {
                // 动画文件找不到就算了
                return;
            }

            curLayserState.AnimState = subState;
            curLayserState.ActionEndState = endStateCB;
            curLayserState.SetClip(animationClip, animParam.StartTime);
            curLayserState.speed = animParam.Speed;
            // 技能状态 特殊规则判断
            if (subState == E_ULayerSubState.Skill_Common1 || subState == E_ULayerSubState.Skill_Common2)
            {
                if (CheckNeedResetStandCount(subState))
                {
                    standCount = 0;
                }

                // 如果上一个状态是受击状态
                // 那当前的这个技能动作，就需要重头播放
                curLayserState.isNeedPlayFromStart = CurrentState == E_ULayerSubState.Hurt;
                if (animParam.UseLastStateTimeAsStartTime && StateMachine.CurrentState != null && CurrentState != E_ULayerSubState.Skill_Common1 && CurrentState != E_ULayerSubState.Skill_Common2)
                {
                    float lastStateTime = StateMachine.CurrentState.GetCurStateTime();
                    curLayserState.SetStartTime(lastStateTime);
                }
            }
            else if (subState == E_ULayerSubState.Hurt)
            {
                curLayserState.isNeedPlayFromStart = CurrentState != E_ULayerSubState.Skill_Common1 && CurrentState != E_ULayerSubState.Skill_Common2;
            }
            else
            {
                curLayserState.isNeedPlayFromStart = isNeedPlayFromStart;
            }
            // 死亡状态一直保持死亡的最后一帧
            if (subState == E_ULayerSubState.Deading)
            {
                curLayserState.IsBacktrackDefault = false;
            }
            // 当这个状态 不为空的时候， 设置这个状态 的 融合时间
            curLayserState.FadeTimeMilSeconds = animParam.FadeInTime;
            // SGF.Debuger.LogError($"{LogFlag} originState: {originState}_[{originState.GetHashCode()}], curState: {CurrentState}_[{CurrentState.GetHashCode()}] , refreshStateAnimRes: {refreshStateAnimRes}, dirtyState: {dirtyState}  ");
            // 如果需要设置的状态 不是 站立状态, 才需要检查 是否 延迟播放 standBy anim 动画
            if (subState != E_ULayerSubState.StandBy)
            {
                bool needPlayStandAnim = CheckIsNeedDelayPlayStandAnim(subState);
                if (!needPlayStandAnim)
                {
                    if (DelayInvoker.ContainInvoke(StandAnimDelayKey))
                    {
                        DelayInvoker.CancelInvoke(StandAnimDelayKey);
                    }
                }
                else
                {
                    // SGF.Debuger.Log($"[stand] 准备 延迟播放 待机动画, CurrentState: {CurrentState}, dirtyState {dirtyState}");
                    DelayPlayStandAnim();
                }
            }
            // 只有 动画状态 的资源 刷新成功, 才需要 去切换这个状态的动画. 而所有的动画状态由最后一个状态决定
            // 否则 就只设置当前的状态,并不切换 动画(防止切换到 新状态 ,但是因为没有新状态的动画,而播放了新状态中的旧动画的bug)
            CurrentState = subState;
            NextVitalState = curLayserState;
            dirtyState = true;
        }

        private void AsyncRefreshCurAnimCallBack(VitalState curLayserState, AnimationClip animationClip)
        {
            curLayserState.SetClip(animationClip);
            curLayserState.isNeedPlayFromStart = true;
            // 标记这个状态 为 dirtyState, 同时 设置 M_NextVitalState(只有 两个同时设置,才会刷新显示层)
            // 标记 这个状态动画需要重新播放, 否则同一个状态 的状态进入会 被内部return.
            NextVitalState = curLayserState;
            dirtyState = true;
        }

        /// <summary>
        /// 站立状态回调
        /// </summary>
        /// <param name="num"></param>
        private void OnIdleStateAction(int num)
        {
            //要回到Idle，走正常流程，里面的BattleIdle/Idle自己会处理
            M_EntityBase?.ForceSetState(E_ULayerSubState.Idle, null);
        }

        /// <summary>
        /// 播放特效
        /// </summary>
        /// <param name="fxParam"></param>
        private void PlayFx(I_FxParam fxParam, string key, string effectPathPrefix)
        {
            SafePlayEffect(fxParam, key, effectPathPrefix);
        }

        /// <summary>
        /// 速度改变
        /// </summary>
        /// <param name="speed"></param>
        private void OnSpeedChangeAction(float speed)
        {
            if (M_EntityBase == null)
            {
                return;
            }
            // 判断现在的状态是不是移动状态
            VitalState vitalState = GetVitalStataByState(CurrentState);
            if (vitalState != null)
            {
                switch (CurrentState)
                {
                    case E_ULayerSubState.WanderMoving:
                        {
                            vitalState.speed = speed / M_EntityBase.m_entityBaseWalkSpeed;
                        }
                        break;
                    case E_ULayerSubState.SingleMoving:
                        {
                            vitalState.speed = speed / M_EntityBase.m_entityBaseRunSpeed;
                        }
                        break;
                    case E_ULayerSubState.BattleMoving:
                        {
                            vitalState.speed = speed / M_EntityBase.m_entityBaseRunSpeed;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        //TODO 曲 : 后面要把这些通用逻辑放在 基类中
        #region 模型 动作 模块的逻辑, 本来想放基类的,但是发现不太好放,所以目前是 每个子类拷贝一份

        /// <summary>
        /// 当前是否是默认动画
        /// </summary>
        private bool curIsDefaultAnim = true;
        /// <summary>
        /// 当前 玩家身上的 改变动画 数据, 目前一般由buff 来赋予玩家 不同的 基础动作数据.
        /// 此处存的是 玩家身上 当前 会显示的 动画数据
        /// </summary>
        private ChangeAnimsData curChangeAnimData = null;

        /// <summary>
        /// 当 玩家身上的 状态动作数据改变时,通知 显示层
        /// </summary>
        /// <param name="changeAnimsData"></param>
        private void OnRefreshCurChangeAnims(ChangeAnimsData changeAnimsData)
        {
            // 刷新当前是否是 默认动作状态
            curIsDefaultAnim = changeAnimsData == null;

            curChangeAnimData = changeAnimsData;

            // 同时 ，刷新当前状态 的动画
            RefreshCurStateAnim();
        }

        /// <summary>
        /// 只刷新 当前状态的动作,没必要 状态动作一改变，就刷新所有状态的动画
        /// note:
        ///     目前 策划的设计是 动画文件 被替换之后,当前的动作继续跑完, 等到下一次播放这个动作的时候,换成新的.
        ///     所以此处 目前是 只替换了动画,但是并没有直接重播. 目前还不确定这个是否会有问题,以及 目前的动画状态机是否会有问题
        /// </summary>
        private void RefreshCurStateAnim()
        {
            VitalState curVitalState = GetCurVitalState();
            if (curVitalState != null)
            {
                // 刷新完状态动画文件后-》下发给影子
                Action<AnimationClip> cb = (animationClip) =>
                {
                    AsyncRefreshCurAnimCallBack(curVitalState, animationClip);
                };
                string animName = GetAnimNameByState(curVitalState.AnimState);
                RefreshStateAnim(animName, cb);
            }
        }

        private string GetAnimNameByState(E_ULayerSubState state, I_AnimParam animParam = null)
        {
            string animName = "";
            bool isReadAnimData = curIsDefaultAnim || curChangeAnimData == null;

            switch (state)
            {
                case E_ULayerSubState.Default:
                case E_ULayerSubState.Idle:
                    {
                        animName = isReadAnimData ? animCfg.Idle : curChangeAnimData.GetAnim(SkillEditor.AnimState.Idle);
                        animName = animName == null ? animCfg.Idle : animName;
                    }
                    break;
                case E_ULayerSubState.BattleIdle:
                    {
                        animName = isReadAnimData ? animCfg.BattleIdle : curChangeAnimData.GetAnim(SkillEditor.AnimState.BattleIdle);
                        animName = animName == null ? animCfg.BattleIdle : animName;
                    }
                    break;
                case E_ULayerSubState.WeaponRetractionIdle:
                    {
                        animName = isReadAnimData ? animCfg.WeaponRetractionIdle : curChangeAnimData.GetAnim(SkillEditor.AnimState.WeaponRetractionIdle);
                        animName = animName == null ? animCfg.WeaponRetractionIdle : animName;
                    }
                    break;
                case E_ULayerSubState.StandBy:
                    {
                        // 待机动作 应该没什么 改变动画的 幺蛾子，因为待机动画就是 
                        animName = animCfg.Stand;
                    }
                    break;
                case E_ULayerSubState.WeakNess:
                    break;
                case E_ULayerSubState.InterAction1:
                case E_ULayerSubState.InterAction2:
                case E_ULayerSubState.Skill:
                case E_ULayerSubState.Skill_Common1:
                case E_ULayerSubState.Skill_Common2:
                case E_ULayerSubState.Skill_Common:
                    {
                        if (animParam != null)
                        {
                            animName = animParam.GetCurAnimationName();
                        }
                    }
                    break;
                case E_ULayerSubState.Hurt:
                    {
                        animName = isReadAnimData ? animCfg.Hurt : curChangeAnimData.GetAnim(SkillEditor.AnimState.Hurt);
                        animName = animName == null ? animCfg.Hurt : animName;
                    }
                    break;
                case E_ULayerSubState.WanderMoving:
                    break;
                case E_ULayerSubState.SingleMoving:
                    {
                        animName = isReadAnimData ? animCfg.SingleMoving : curChangeAnimData.GetAnim(SkillEditor.AnimState.SingleMoving);
                        animName = animName == null ? animCfg.SingleMoving : animName;
                    }
                    break;
                case E_ULayerSubState.WeaponRetractionMoving:
                    {
                        animName = isReadAnimData ? animCfg.WeaponRetractionMoving : curChangeAnimData.GetAnim(SkillEditor.AnimState.SingleMoving);
                        animName = animName == null ? animCfg.WeaponRetractionMoving : animName;
                    }
                    break;
                case E_ULayerSubState.BattleMoving:
                    {
                        animName = isReadAnimData ? animCfg.BattleMoving : curChangeAnimData.GetAnim(SkillEditor.AnimState.BattleMoving);
                        animName = animName == null ? animCfg.BattleMoving : animName;
                    }
                    break;
                case E_ULayerSubState.BeingControl:
                    break;
                case E_ULayerSubState.BegineKnockDown:
                    break;
                case E_ULayerSubState.KnockDown:
                    break;
                case E_ULayerSubState.EndKnockDown:
                    break;
                case E_ULayerSubState.IsSonscious:
                    break;
                case E_ULayerSubState.Deading:
                    {
                        animName = isReadAnimData ? animCfg.Deading : curChangeAnimData.GetAnim(SkillEditor.AnimState.Deading);
                        animName = animName == null ? animCfg.Deading : animName;
                        // Deading 死亡状态 目前 时先采用 KnockDown 击倒状态
                    }
                    break;
                default:
                    {
                        SGF.Debuger.LogWarning($"{LogFlag} RefreshCurStateAnim CurrentState {CurrentState} no handle!!!");
                    }
                    break;
            }

            return animName;
        }

        #endregion

        #region 播放随机待机动画

        private float GetStandByRandomTime()
        {
            if (M_EntityBase == null || M_EntityBase.avatarDataCell == null)
            {
                return 0;
            }
            var randomTime = M_EntityBase.avatarDataCell.StandRandTime;
            if (randomTime.Count == 0)
            {
                return 0;
            }
            float delayTime = randomTime[(int)UnityEngine.Random.Range(0, randomTime.Count)] / 1000f;
            return delayTime;
        }

        /// <summary>
        /// 检查是否需要 重置 待机播放休闲动作次数
        /// </summary>
        private bool CheckNeedResetStandCount(E_ULayerSubState newState)
        {
            // 首先,只有 idle状态时, 才会切换 待机 动画. 所以一旦新的状态不是idle,就需要 重置次数
            if (newState != E_ULayerSubState.Idle)
            {
                return true;
            }
            // 检查 是否 改变了 idle 的动画, 被动 buff 等等 都可能改变idle 状态的动画, 如果动画改变,  
            if (!curIsDefaultAnim)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// 检查是否需要 延迟播放 待机动画. 如果当前状态 不是Idle  或者当前的动画被改变(就需要改变为idle动画) 
        /// 或者当前 没有配置这个 动画, 就需要 退出这个 stand 状态
        /// </summary>
        /// <returns></returns>
        private bool CheckIsNeedDelayPlayStandAnim(E_ULayerSubState newState)
        {
            // 首先,只有 idle状态时, 才会切换 待机 动画
            if (newState != E_ULayerSubState.Idle)
            {
                return false;
            }

            // 如果是 idle 状态, 就需要检查 是否 改变了 idle 的动画, 被动 buff 等等 都可能改变idle 状态的动画
            if (!curIsDefaultAnim)
            {
                return false;
            }

            // 如果设置了 保持初始的idle 不进入 stand 状态, 那就不切换到 stand
            if (KeepStartIdleNotStand)
            {
                return false;
            }

            if (animCfg == null)
            {
                return false;
            }

            // 如果 待机动画 配置的是 空,那就 直接返回false
            if (string.IsNullOrEmpty(animCfg.Stand))
            {
                return false;
            }

            if (-1 != CfgStandCount && standCount > 0 && standCount >= CfgStandCount)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// idle 状态下,如果 长时间没有动作(一直在idle 状态, 就切换到 待机动作)
        /// gl 2023/5/16 提的需求
        /// </summary>
        private void DelayPlayStandAnim()
        {
            // 如果有 延迟播放 待机动画的 定时器,那就 return
            if (DelayInvoker.ContainInvoke(StandAnimDelayKey))
            {
                return;
            }

            if (M_EntityBase == null || M_EntityBase.avatarDataCell == null)
            {
                return;
            }

            float delayTime = GetStandByRandomTime();

            DelayInvoker.DelayInvoke(StandAnimDelayKey, delayTime, PlayStandAnim);
        }

        /// <summary>
        /// 2024/3/13
        /// gl 提的需求 :
        ///     在待机期间会根据时间判断是否播放休闲动作，新增次数限制，保证在同一次待机期间不会在尾部判断休闲动作
        /// </summary>
        private int standCount = 0;

        /// <summary>
        /// 配置表 配置的 播放休闲动作 次数
        /// </summary>
        private int CfgStandCount
        {
            get
            {
                if (M_EntityBase != null && M_EntityBase.avatarDataCell != null)
                {
                    return M_EntityBase.avatarDataCell.GetStandCount();
                }
                return 0;
            }
        }
        private void PlayStandAnim(object[] args)
        {
            // 检查是否需要重置待机播放次数
            if (CheckNeedResetStandCount(CurrentState))
            {
                standCount = 0;
            }

            // 在真正播放 这个 待机动画的时候, 再去检查一边 是否能够播放
            if (!CheckIsNeedDelayPlayStandAnim(CurrentState))
            {
                return;
            }

            if (M_EntityBase == null || M_EntityBase.avatarDataCell == null)
            {
                standCount = 0;
                return;
            }

            float rate = UnityEngine.Random.Range(0, 1f);
            if (rate * 10000 <= M_EntityBase.avatarDataCell.GetStandProbability())
            {
                // 待机动作播放次数 ++
                standCount++;

                // SGF.Debuger.Log($"[stand] 准备 切换 待机状态, CurrentState: {CurrentState}, ready SetStandByState ");
                // 设置 standBy
                M_EntityBase.SetStandByState();
            }
            else
            {
                // SGF.Debuger.Log($"[stand] 准备 切换 待机状态, 概率不足, DelayPlayStandAnim");
                // 如果概率不满足, 那就重新去执行 延迟播放 这个 站立动画
                DelayPlayStandAnim();
            }
        }

        #endregion


        #region 获取状态机

        /// <summary>
        /// 获取VitalState  获取空闲的VitalState
        /// </summary>
        /// <param name="VitalStateName"></param>
        /// <returns></returns>
        private VitalState GetFreeVitalState(string VitalStateName)
        {
            VitalState vitalState = null;
            if (m_entity == null)
            {
                return vitalState;
            }
            if (!string.IsNullOrEmpty(VitalStateName) && !string.IsNullOrWhiteSpace(VitalStateName))
            {
                string stateName = VitalStateName.ToLower();
                if (stateName.IndexOf("idle") != -1)
                {
                    vitalState = Stage_Idle;
                }
            }

            if (vitalState == null)
            {
                // 使用通用的
                if (CommonVitalStates != null)
                {
                    foreach (var item in CommonVitalStates)
                    {
                        if (item != null && !item.enabled)
                        {
                            vitalState = item;
                            break;
                        }
                    }
                }
            }
            return vitalState;
        }

        /// <summary>
        /// 获得当前的状态机
        /// </summary>
        /// <returns></returns>
        private VitalState GetCurVitalState()
        {
            VitalState vitalState = null;

            if (Stage_Idle.enabled || m_IsInit)
            {
                vitalState = Stage_Idle;
            }

            if (vitalState == null)
            {
                if (CommonVitalStates != null)
                {
                    foreach (var item in CommonVitalStates)
                    {
                        if (item.enabled)
                        {
                            vitalState = item;
                            break;
                        }
                    }
                }
            }

            return vitalState;
        }

        /// <summary>
        /// 获取该状态的状态机
        /// </summary>
        /// <param name="e_ULayerSubState"></param>
        /// <returns></returns>
        private VitalState GetVitalStataByState(E_ULayerSubState e_ULayerSubState)
        {
            VitalState vitalState = null;

            if (CommonVitalStates != null)
            {
                foreach (var item in CommonVitalStates)
                {
                    if (item.enabled)
                    {
                        if (item.AnimState == e_ULayerSubState)
                        {
                            vitalState = item;
                            break;
                        }
                    }
                }
            }
            return vitalState;
        }

        #endregion

    }
}



