using Animancer.FSM;
using SGF.Unity;
using SGF.Utlis;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.Sound;
using StarProject.Service.Time;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalAnim : ViewVitalHeroNormal, I_VVitalAnim
    {
        private string LogFlag = "[ViewVitalAnim]";

        public AudioSource audioSource;//有动画都有声音[主角自己播放声音]，其他人通过aoi到这里播放声音

        private readonly StateMachine<VitalState>.WithDefault _StateMachine = new();
        public StateMachine<VitalState>.WithDefault StateMachine => _StateMachine;

        public Action<VitalState> OnStateChange;    // 通知影子改变状态
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
        private VitalState _nextVitalState;
        public VitalState M_NextVitalState
        {
            get => _nextVitalState;
            set
            {

                _nextVitalState = value;
                //SGF.Debuger.LogError($"[anim] set nextVitalstate: {_nextVitalState.name}, {_nextVitalState.Clip?.name} , {_nextVitalState.GetHashCode()}");
                OnStateChange?.Invoke(_nextVitalState);
            }
        }
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

            Stage_Idle.IdleState += OnIdleStateAction;
            StateMachine.DefaultState = Stage_Idle;
            CurrentState = E_ULayerSubState.Idle;
            m_IsInit = false;

            //audioSource = GetComponent<AudioSource>();
        }

        protected override void Release()
        {
            Stage_Idle.IdleState -= OnIdleStateAction;
            CommonVitalStates.Clear();

            m_IsInit = true;

            DelayInvoker.CancelInvoke(StandAnimDelayKey);
            StandAnimDelayKey = string.Empty;

            base.Release();
        }

        protected override void OnEventListener()
        {
            base.OnEventListener();

            OnRefreshCurChangeAnims(null);

            if (M_EntityBase != null)
            {
                M_EntityBase.AnimStatePlay += PlayAnim;
                M_EntityBase.ViewEnterFrameAction += OnEnterFrame;

                M_EntityBase.ActionOnPlayAudio += OnPlayAudio;
                M_EntityBase.SpeedChangeAction += OnSpeedChangeAction;
                M_EntityBase.Data.ActionRefreshCurChangeAnims += OnRefreshCurChangeAnims;
                M_EntityBase.Data.ActionRefreshCurAnims += RefreshCurStateAnim;

                M_EntityBase.ActionOnPlaySpecialEffects += PlayFx;
                M_EntityBase.ActionOnStopSpecialEffects += BaseStopFx;
            }
        }

        protected override void OffEventListener()
        {
            base.OffEventListener();

            if (M_EntityBase != null)
            {
                M_EntityBase.AnimStatePlay -= PlayAnim;
                M_EntityBase.ViewEnterFrameAction -= OnEnterFrame;

                M_EntityBase.ActionOnPlayAudio -= OnPlayAudio;
                M_EntityBase.SpeedChangeAction -= OnSpeedChangeAction;
                M_EntityBase.Data.ActionRefreshCurChangeAnims -= OnRefreshCurChangeAnims;
                M_EntityBase.Data.ActionRefreshCurAnims -= RefreshCurStateAnim;

                M_EntityBase.ActionOnPlaySpecialEffects -= PlayFx;
                M_EntityBase.ActionOnStopSpecialEffects -= BaseStopFx;
            }
        }

        private void EnterNewState(VitalState newState)
        {
            //SGF.Debuger.Log($" [anim] CurrentState: {CurrentState} --->  进入新状态 {newState}");

            if (StateMachine.CurrentState != null && StateMachine.CurrentState.Clip != null)
            {
                bool isSkillState = newState.AnimState == E_ULayerSubState.Skill_Common1 || newState.AnimState == E_ULayerSubState.Skill_Common2;


                if (isSkillState)
                {
                    /// Fix Bug: 人物站立不动 播特效的bug
                    /// --By DL
                    /// 技能状态会出现 不同状态 但是相同的动作文件相互切的问题
                    /// 此时,逻辑层的状态切换融合后切换过去,但是动作 并没有重新开始播
                    /// 从而导致状态剩余1s,但是 动画剩余0.5s这种情况.
                    /// 此时动画先结束,进入超时Idle状态,导致 人物动画在技能 快速切换,
                    /// 站立不动播特效的情况.
                    /// 修复方案就是 对于技能状态,做特殊处理
                    /// note : 
                    /// 1.bug产生的原因是: 
                    ///   a.技能使用的是 fixedUpdate, 动画使用的是update，那么就存在一定的几率，动画状态机提前一帧超时进入idle;
                    ///   b.如果 此时恰好 之前的技能动作和 新的技能动作一致, 超时的技能 AnimancerState 的时间并没有清空;
                    /// 
                    /// 2.此处使用 新的技能动作 去TryGet 当前的nowState , 因为 Animancer.States.Current 可能是 Idle

                    // if (StateMachine.CurrentState.Character.Animancer.States.TryGet(newState.Key, out AnimancerState nowState))
                    // {
                    //     newState.isNeedPlayFromStart = true;
                    //     // newState.FadeTimeMilSeconds = 0f;
                    // }

                    bool isSameClip = CommonVitalStates[0].Clip == CommonVitalStates[1].Clip;
                    bool curStateIsSkill = StateMachine.CurrentState.CurState != null && StateMachine.CurrentState.CurState.Clip == newState.Clip;

                    if (isSameClip && curStateIsSkill)
                    {
                        newState.isNeedPlayFromStart = true;
                    }



                }
            }
            newState.SetStateDefaultFadeTime(currentState);
            if (newState.isNeedPlayFromStart)
            {
                //SGF.Debuger.LogError($"[anim] playerFromStart: newState[{newState.name}_{newState.GetHashCode()}]: {newState.CurState.Weight} , curState[{newState.name}_{newState.GetHashCode()}]: {StateMachine.CurrentState.CurState.Weight} , clip: {newState.Clip.name} ----> isFramStart");
                // if (newState.CurState.Weight + StateMachine.CurrentState.CurState.Weight != 1)
                // {
                //     newState.CurState.Weight = 1 - StateMachine.CurrentState.CurState.Weight;
                // }
                StateMachine.TryResetState(newState);
            }
            else
            {
                // SGF.Debuger.LogError($"[Anim] play: newState: {newState.Clip.name} , curState: {StateMachine.CurrentState.CurState.Clip.name} ");

                StateMachine.TrySetState(newState);
            }
        }

        private void OnEnterFrame(UnityEngine.Vector3 V)
        {
            if (dirtyState)
            {
                if (M_NextVitalState != null)
                {
                    dirtyState = false;
                    EnterNewState(M_NextVitalState);
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
            //SGF.Debuger.LogWarning($"状态修改 currentState={currentState},subState={subState},animName={animParam.AnimationPath},time={SGF.Time.TimeUtils.TimeLogString()}");
            long curTimeStamp = TimeManager.Instance.GetServerTimeStamp();
            bool changeState = true;
            switch (subState)
            {
                case E_ULayerSubState.StandBy:
                case E_ULayerSubState.Idle:
                case E_ULayerSubState.BattleIdle:
                case E_ULayerSubState.WeaponRetractionIdle:
                case E_ULayerSubState.WanderMoving:
                case E_ULayerSubState.SingleMoving:
                case E_ULayerSubState.WeaponRetractionMoving:
                case E_ULayerSubState.BattleMoving:
                case E_ULayerSubState.Deading:
                case E_ULayerSubState.Hurt:
                    {

                        string animName = GetAnimNameByState(subState, animParam);
                        Action<AnimationClip> cb = (animationClip) =>
                        {
                            VitalState vitalState = GetFreeVitalState(animParam.AnimationPath);
                            if (vitalState != null)
                            {
                                AsyncLoadAnimCallBack(curTimeStamp, subState, vitalState, animationClip, animParam, endStateCB, false, animParam.AnimationPath);
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
                                AsyncLoadAnimCallBack(curTimeStamp, subState, vitalState, animationClip, animParam, endStateCB, true, animParam.AnimationPath);
                            }
                        };
                        RefreshStateAnim(animName, cb);
                    }
                    break;
                case E_ULayerSubState.InterAction1:
                case E_ULayerSubState.InterAction2:
                case E_ULayerSubState.Skill_Common1:
                case E_ULayerSubState.Skill_Common2:
                    {

                        string path = animParam.GetCurAnimationPath(M_EntityBase.avatarDataCell);
                        StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation,
                        (AnimationClip animationClip) =>
                        {

                            VitalState vitalState = GetFreeVitalState(animParam.AnimationPath);
                            if (vitalState != null)
                            {
                                // SGF.Debuger.LogError($"[anim] subState: {subState} ,curVState{M_NextVitalState.name} -->  : {vitalState.name} ,animationClip: {animationClip}"); ;
                                AsyncLoadAnimCallBack(curTimeStamp, subState, vitalState, animationClip, animParam, endStateCB, false, animParam.AnimationPath);
                            }
                        });
                    }
                    break;
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

        private void AsyncLoadAnimCallBack(long chanageTimeStamp, E_ULayerSubState subState, VitalState curLayserState, AnimationClip animationClip, I_AnimParam animParam, Action endStateCB, bool isNeedPlayFromStart, string path)
        {
            /// TODO: 曲
            /// 可能存在逻辑bug, 同一帧可能会 切换多次动画状态, 此时 的时间戳相同.
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
                SGF.Debuger.LogError($" [anim] CurrentState: {CurrentState} --->  准备切换到 {subState} , cant find path: {path} ");

                return;
            }
            //SGF.Debuger.Log($" [anim] CurrentState: {CurrentState} --->  准备切换到 {subState} , anim: {animationClip.name} ");

            curLayserState.AnimState = subState;
            curLayserState.ActionEndState = endStateCB;
            curLayserState.SetClip(animationClip, animParam.StartTime);
            curLayserState.speed = animParam.Speed;
            // 技能状态 特殊规则判断
            if (subState == E_ULayerSubState.Skill_Common1 || subState == E_ULayerSubState.Skill_Common2)
            {
                // 如果上一个状态是受击状态
                // 那当前的这个技能动作，就需要重头播放
                curLayserState.isNeedPlayFromStart = CurrentState == E_ULayerSubState.Hurt;
                if (animParam.UseLastStateTimeAsStartTime && StateMachine.CurrentState != null && CurrentState != E_ULayerSubState.Skill_Common1 && CurrentState != E_ULayerSubState.Skill_Common2)
                {
                    float lastStateTime = StateMachine.CurrentState.GetCurStateTime();
                    curLayserState.SetStartTime(lastStateTime);

                    // SGF.Debuger.Log($" [anim] CurrentState: {CurrentState} 准备切 技能状态 , 设置 startTime: {lastStateTime} ");
                }
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
                if (CheckNeedResetStandCount(subState))
                {
                    standCount = 0;
                }

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
            //SGF.Debuger.Log($" [anim] 加载完动画后 设置 M_NextVitalState CurrentState: {CurrentState} --->  准备切换到 {NextVitalState}  ");
            CurrentState = subState;
            M_NextVitalState = curLayserState;

            dirtyState = true;
        }

        private void AsyncRefreshCurAnimCallBack(VitalState curLayserState, AnimationClip animationClip)
        {
            curLayserState.SetClip(animationClip);
            curLayserState.isNeedPlayFromStart = true;
            // 标记这个状态 为 dirtyState, 同时 设置 M_NextVitalState(只有 两个同时设置,才会刷新显示层)
            // 标记 这个状态动画需要重新播放, 否则同一个状态 的状态进入会 被内部return.
            //SGF.Debuger.Log($" [anim] 刷新当前动画 cb CurrentState: {CurrentState} --->  准备切换到 {NextVitalState} , anim: {animationClip.name} ");

            M_NextVitalState = curLayserState;

            dirtyState = true;
        }

        private void OnIdleStateAction(int num)
        {
            //要回到Idle，走正常流程，里面的BattleIdle/Idle自己会处理
            M_EntityBase?.SetMovmentState();
        }

        private void PlayFx(I_FxParam fxParam, string key, string effectPathPrefix)
        {
            SafePlayEffect(fxParam, key, effectPathPrefix);
        }

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

        /// <summary>
        /// 根据状态获取动画名
        /// </summary>
        /// <returns></returns>
        private string GetAnimNameByState(E_ULayerSubState state, I_AnimParam animParam = null)
        {
            string animName = "";

            switch (state)
            {
                case E_ULayerSubState.Default:
                case E_ULayerSubState.Idle:
                    {
                        animName = curIsDefaultAnim ? animCfg.Idle : curChangeAnimData.GetAnim(SkillEditor.AnimState.Idle);
                        animName = animName == null ? animCfg.Idle : animName;
                    }
                    break;
                case E_ULayerSubState.BattleIdle:
                    {
                        animName = curIsDefaultAnim ? animCfg.BattleIdle : curChangeAnimData.GetAnim(SkillEditor.AnimState.BattleIdle);
                        animName = animName == null ? animCfg.BattleIdle : animName;
                    }
                    break;
                case E_ULayerSubState.WeaponRetractionIdle:
                    {
                        animName = curIsDefaultAnim ? animCfg.WeaponRetractionIdle : curChangeAnimData.GetAnim(SkillEditor.AnimState.WeaponRetractionIdle);
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
                        animName = curIsDefaultAnim ? animCfg.Hurt : curChangeAnimData.GetAnim(SkillEditor.AnimState.Hurt);
                        animName = animName == null ? animCfg.Hurt : animName;
                    }
                    break;
                case E_ULayerSubState.WanderMoving:
                    break;
                case E_ULayerSubState.SingleMoving:
                    {
                        animName = curIsDefaultAnim ? animCfg.SingleMoving : curChangeAnimData.GetAnim(SkillEditor.AnimState.SingleMoving);
                        animName = animName == null ? animCfg.SingleMoving : animName;
                    }
                    break;
                case E_ULayerSubState.WeaponRetractionMoving:
                    {
                        animName = curIsDefaultAnim ? animCfg.WeaponRetractionMoving : curChangeAnimData.GetAnim(SkillEditor.AnimState.SingleMoving);
                        animName = animName == null ? animCfg.WeaponRetractionMoving : animName;
                    }
                    break;
                case E_ULayerSubState.BattleMoving:
                    {
                        animName = curIsDefaultAnim ? animCfg.BattleMoving : curChangeAnimData.GetAnim(SkillEditor.AnimState.BattleMoving);
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
                        animName = curIsDefaultAnim ? animCfg.Deading : curChangeAnimData.GetAnim(SkillEditor.AnimState.Deading);
                        animName = animName == null ? animCfg.Deading : animName;
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

        #region 播放声音

        private void OnPlayAudio(AudioClip obj, bool needLoop)
        {
            if (audioSource == null)
            {
                return;
            }
            //ac有，且有关联
            audioSource.clip = obj;
            audioSource.loop = needLoop;//结束回调
            if (needLoop)
            {
                audioSource.Play();
            }
            else
            {
                //Shot后的回调
                audioSource.PlayOneShot(obj);
            }
        }

        private void DelayPlaySound(float delayTime, string soundName)
        {
            object[] Os = new object[] { soundName };
            DelayInvoker.DelayInvoke(delayTime,
                   //延迟处理--------------------
                   (object[] args) =>
                   {
                       string soundName1 = (string)args[0];
                       SkillTriSound(soundName1);
                   }
           , Os);
        }

        //wwise曲
        private void SkillTriSound(string soundEnum)
        {
            if (!String.IsNullOrEmpty(soundEnum))
            {
                SoundManager.Instance.PlayAudioSourceEvent(
               soundEnum.ToEnum<AudioMethodEvent>()
               , OnPlayAudio, needLoop: false);

                //akEvent.
            }
        }

        #endregion


        #endregion
    }
}



