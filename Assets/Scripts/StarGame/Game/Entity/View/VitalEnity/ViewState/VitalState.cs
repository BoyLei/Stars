using Animancer;
using Animancer.FSM;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Entity.View.VitalSign.State
{
    public delegate void VitalStateChanged(VitalState vitalState, VitalStateEnum state);

    public enum VitalStateEnum
    {
        Play,
        End,
    }
    public enum ChangeStateConditions
    {
        DefToIdle,
        StayOnEnd

    }

    public class VitalState : StateBehaviour
    {
        public ChangeStateConditions ChangeCond = ChangeStateConditions.DefToIdle;

        [SerializeField] private Priority _Priority;
        public enum Priority
        {
            Low,// Could specify "Low = 0," if we want to be explicit.
            Medium,// Medium = 1,
            High,// High = 2,
        }

        public VitalStateChanged OnVitalStateChanged;   // 动作状态改变的委托

        /// <summary>
        /// Only allows a new state to be entered if it has equal or higher <see cref="Priority"/> to this state.
        /// </summary>
        public override bool CanExitState
        {
            get
            {
                var nextState = (VitalState)StateChange<VitalState>.NextState;
                return nextState._Priority >= _Priority;
            }
        }
        // 进入Idle状态开始的委托
        public Action<int> IdleState;
        // 状态结束委托
        public Action ActionEndState;

        [SerializeField]
        public AnimationClip _Animation;    // 默认绑定的动作文件
        // 
        /// <summary>
        /// 过渡动画, 是一种 对 AnimationClip 的封装, 可以用来设置动画在什么时候点 开始 / 结束 等 
        /// note:
        /// 
        /// </summary>
        [SerializeField] private ClipTransition _clipTransition;

        /// <summary>
        /// 状态机 在 进入状态时, 会根据 ClipTransition.key 去注册
        /// </summary>
        public object Key => _clipTransition == null ? null : _clipTransition.Key;

        public AnimationClip Clip => _clipTransition == null ? null : _clipTransition.Clip;

        public float ClipDuration => _clipTransition == null ? 0 : _clipTransition.Clip.length;

        public float NormalizedStartTime => _clipTransition == null ? 0 : _clipTransition.NormalizedStartTime;

        // 播放速度
        public float speed = 1;
        // 是否需要重头播放
        public bool isNeedPlayFromStart = false;

        /// <summary>
        /// 动画融合 过渡的时间;
        /// note:
        ///     1.默认的值是 -1, 是为了 能够 外部传0时,能够区分默认值;
        ///     2.传过来的 动画融合 过渡时间,单位为 豪秒/ms
        /// </summary>
        public float FadeTimeMilSeconds = -1;

        // 是否融合[是否帮美术动作节省动画（融合天然具备帮忙做动画的功能）]
        // >0是默认true=融合 ；==0 就是不融合
        //意义：下一个动画的百分之多少来做上一个的融合；上一个会播放完毕，下一个idle15%时间做融合
        //填写：比如idle 10秒，需要3秒融合，3/10=0.3
        private float DefFadeTime = 0.15f;

        private I_VVitalAnim _Character;
        public I_VVitalAnim Character { get => _Character; set => _Character = value; }
        //=====我的Character的属性======
        //public AnimancerComponent M_CharacterAnimancer;
        //==============================

        public AnimancerState CurState;

        // 动画的状态
        private E_ULayerSubState _AnimState;
        public E_ULayerSubState AnimState { get => _AnimState; set => _AnimState = value; }

        // 是否回归默认状态
        private bool _IsBacktrackDefault = true;
        public bool IsBacktrackDefault { get => _IsBacktrackDefault; set => _IsBacktrackDefault = value; }

        public List<UnityEngine.AnimationEvent> AnimEvents = new();

        //上一次执行 动画update的时间. 增加这个变量的目的是为了通过 上一次upadte 的时间 跟当前帧动画时间的比较，确定动画是否进入下一次Loop
        public float LastAnimTime = 0;

        /// <summary>
        /// 禁止一次 状态结束还原到 idle 状态
        /// </summary>
        public bool ForbidRset2IdleOnEnd = false;

        public void SetClip(AnimationClip clip, float startTime = 0)
        {
            _clipTransition.Clip = clip;
            SetStartTimeMs(startTime);

            if (startTime > 0)
            {
                //SGF.Debuger.LogError($"[xx-xx] , vitalState : {this.name} , setClip : {clip.name} , startTime : {startTime}");
            }

            // _clipTransition.State.Time
        }

        /// <summary>
        /// 设置动画的 开始时间, 单位毫秒
        /// </summary>
        /// <param name="startTime"> ms </param>
        public void SetStartTimeMs(float startTime)
        {
            SetCurNormalizedStartTime(startTime / 1000f / _clipTransition.MaximumDuration);
        }

        /// <summary>
        /// 设置动画的 开始时间, 单位 s
        /// </summary>
        /// <param name="startTime"> s </param>
        public void SetStartTime(float startTime)
        {
            SetCurNormalizedStartTime(startTime / _clipTransition.MaximumDuration);
        }

        /// <summary>
        /// 设置 动画 标准单位化的 时间
        /// </summary>
        /// <param name="startTime"> 0~1 </param>
        public void SetCurNormalizedStartTime(float startTime)
        {
            _clipTransition.NormalizedStartTime = startTime;
        }

        public float GetCurStateTime()
        {
            if (CurState != null)
            {
                return CurState.Time;
            }
            return 0;
        }

        public override void OnEnterState()
        {
            if (Clip == null && _Animation != null)
            {
                SetClip(_Animation);
            }
            //SGF.Debuger.Log($"[VitalState] OnEnterState T :  {this.name}  ");

            base.OnEnterState();//状态先不做二次继承
            if (transform.name == "Idle")
            {
                IdleState?.Invoke(0);
            }

            // 注册动画事件
            // RegisterAnimationEvent(_clipTransition.Clip.events);
            OnVitalStateChanged?.Invoke(this, VitalStateEnum.Play);
        }
        // public override void OnExitState()
        // {
        //     SGF.Debuger.Log($"[VitalState] OnExitState T :  {transform.name}  ");

        //     base.OnExitState();//状态先不做二次继承

        // }


        // Start is called before the first frame update
        private void Awake()
        {
            //_Character = gameObject.GetComponent<VVitalAnim>();
        }

        /// <summary>
        /// Plays the animation and if it is not looping it returns the <see cref="Character"/> to Idle afterwards.
        /// </summary>
        ///
        private void OnEnable()
        {
            if (_clipTransition == null || _clipTransition.Clip == null)
            {
                SGF.Debuger.Log($"VitalState.OnEnable() name={transform.name} _clipTransition == null || _clipTransition.Clip == null err!!!");
                return;
            }
            if (Character == null || Character.Animancer == null)
            {
                SGF.Debuger.Log($"VitalState.OnEnable() name={transform.name} Character == null || Character.Animancer== null err!!!");
                return;
            }
            // 是否回归默认状态
            if (!IsBacktrackDefault)
            {
                ChangeCond = ChangeStateConditions.StayOnEnd;
            }
            //
            //看引用哪一个吧
            /// <summary>
            /// 我们逻辑层不会允许相同逻辑进去，不会逻辑维护表现，所以逻辑驱动表现一定是要换状态换动画，当前的设置就是有新的动画立刻暂停来播放新的
            /// </summary>
            /// <returns></returns>
            /// -----------------------------------------------------------------
            /// 新动画相对 0.15倍长度的时间 完成切换，提前调用时下一个权重都是0
            /// 这里权重是 动画播放优先级pri 和 【动态权重】；像如同状态就是0就是立刻播放；0.15下一个动画用来融合
            /// NormalizedSpeed在乎权重，同状态切换时可以享受weight的影响
            /// 切换过程中：设计意义是，权重越接近1越不需要时间发生切换或者融合【算出的时间就是进行下一次切换的融合时间（动画百分比）】【越接近下一个动画，如切换到下一个状态就越不需要，花时间去切换到此状态】；；【跟多layer分层没关系 ，这就是一个层次里面的，mix只包含两个动画的关系】
            /// 必须0.15系数 * 下一个的动画时间 * 【剩余需要融合的百分比】  ；比如当前权重已经是0.9，说明有0.1没切换完毕，算法上  = 0.1 * 下一个动画长度 * 0.15（程序客户需求）；在给你补0.1时间去融合
            /// 目的是永远保持；任何情况同状态切换都可以并且只取剩余时间：=整体为=【 必须0.15系数 * 下一个的动画时间 * 【整体是1】】
            /// -------------------------------------------------------------
            /// 根据枚举时间，第二个的参数意义可能都不同，比如绝对时间，比如动画长度的百分比；
            ///--------------------------------------------------------------
            bool isStateReStart = false;
            if (isNeedPlayFromStart && CurState != null)
            {
                //Rewind播放指针..
                CurState.Stop();
                //CurState.NormalizedTime = 0;
                //Character.Animancer.Play(_Animation, 0.01f).Time = 0;
                isNeedPlayFromStart = false;
                isStateReStart = true;
            }
            if (CurState != null)
            {
                // 先清空
                CurState.Events.OnEnd = null;
            }
            //上一个收尾-----------------------------------------
            float fadeDuration = FadeTimeMilSeconds / (_clipTransition.Clip.length * 1000f);

            fadeDuration = _clipTransition.NormalizedStartTime > 0 ? 0 : fadeDuration;

            //融合时间..
            if (fadeDuration == 0)
            {

                // SGF.Debuger.LogError($"state : {this.name} , animation: {Clip.name} , fadeDuration : {fadeDuration} , mode:  FixedDuration");
                CurState = Character.Animancer.Play(_clipTransition, 0, FadeMode.FixedDuration);//段磊描述的问题：1无CD，2反复点，3动作在融合过程中脚本检测他在相同状态下【不会重新播放-导致动作不播放】【暂定不存在此问题】
            }
            else
            {
                // 如果 fadeDuration == -1 ,那就是默认的 过渡时间, 此时采用 DefFadeTime 默认过渡时间
                fadeDuration = fadeDuration > 0 ? fadeDuration : DefFadeTime;

                /// 2023/9/5
                /// 高磊强烈要求 同一个动作连续播,也需要融合,所以 对于需要重新播的状态动作, 使用NormalizedFromStart, 它会创建一个新的同样文件的状态.
                FadeMode fadeMode = isStateReStart ? FadeMode.NormalizedFromStart : FadeMode.NormalizedSpeed;
                // SGF.Debuger.LogError($"state : {this.name} , animation.length : {Clip.name} , fadeDuration : {fadeDuration} , mode:  {fadeMode}");
                CurState = Character.Animancer.Play(_clipTransition, fadeDuration, fadeMode);//段磊描述的问题：1无CD，2反复点，3动作在融合过程中脚本检测他在相同状态下【不会重新播放-导致动作不播放】【暂定不存在此问题】//0.1//0.25f 美术做到了过度，不用融合，容易出问题在融合过程中本身状态并未退出导致再有同状态过来屏蔽掉了不会rewind就导致不播了

            }

            CurState.Time = _clipTransition.NormalizedStartTime > 0 ? CurState.Time : 0;
            // SGF.Debuger.Log($"[VitalState] OnEnable T :  Play animation {_Animation.name}  ");
            Character.Animancer.Playable.Speed = speed;

            // 2023/09/04
            // 如果 状态自己切自己, 那么 这个状态自己的融合 weight 就直接设置为1.
            // note:
            // 目前是 只考虑 2 种 状态相互切的情况， 如果更多的状态切换， 此处的wight 就不能如此粗暴的设置.
            // 可能整个 状态的 融合 切换逻辑 都需要做些调整, 否则 还是会出现 wight!=1 的情况
            // if (isStateReStart)
            // {
            //     CurState.Weight = 1;
            // }

            //SGF.Debuger.LogError($"[VitalState] state : {CurState.Clip.name}");
            if (!_clipTransition.IsLooping)
            {
                switch (ChangeCond)
                {
                    //[控制状态之间的跳转情况:回Idle/暂停在此刻]
                    case ChangeStateConditions.DefToIdle:
                        {
                            CurState.Events.OnEnd = () =>
                            {
                                ActionEndState?.Invoke();
                                ActionEndState = null;

                                // 禁止一次 返回默认 idle, 同一个状态 但是用两个 state 做切换的时候，防止在帧尾出现 刚切换到 下一个相同状态, 就被上一个状态的 onEnd 还原到idle 的bug
                                if (ForbidRset2IdleOnEnd)
                                {
                                    // SGF.Debuger.Log($"xxxx OnEnable : {CurState} 禁止 超时回到 idle");
                                    return;
                                }
                                Character.StateMachine.ForceSetDefaultState?.Invoke();
                            };
                            //CurState.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;
                        }
                        break;
                    case ChangeStateConditions.StayOnEnd:
                        {
                            CurState.Events.OnEnd = () =>
                            {
                                ActionEndState?.Invoke();
                                ActionEndState = null;
                            };
                        }
                        break;
                    default:
                        {
                            CurState.Events.OnEnd = () =>
                            {
                                ActionEndState?.Invoke();
                                ActionEndState = null;
                            };
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 设置 状态默认的 融合时间
        /// </summary>
        /// <param name="layerState"></param>
        public void SetStateDefaultFadeTime(E_ULayerSubState layerState)
        {
            FadeTimeMilSeconds = GetStateEnterTime(layerState);
            //SGF.Debuger.LogError($"动画 融合时间 layerState: {layerState} , {FadeTimeMilSeconds}ms");
        }
        private Type typeOfState = typeof(E_ULayerSubState);

        private float GetStateEnterTime(E_ULayerSubState layerState)
        {
            // 如果 状态的 过渡时间已经设置了, 那就直接返回它设置的过渡时间
            if (FadeTimeMilSeconds != -1)
            {
                //SGF.Debuger.LogError($"[VitalState] state {this.name} , 设置默认状态 {layerState} 时 存在 FadeTimeMilSeconds: {FadeTimeMilSeconds} ");
                return FadeTimeMilSeconds;
            }

            string stateName = name;
            switch (layerState)
            {
                case E_ULayerSubState.Idle:
                case E_ULayerSubState.BattleIdle:
                case E_ULayerSubState.WeaponRetractionIdle:
                case E_ULayerSubState.WeakNess:
                case E_ULayerSubState.Hurt:
                case E_ULayerSubState.WanderMoving:
                case E_ULayerSubState.SingleMoving:
                case E_ULayerSubState.BattleMoving:
                case E_ULayerSubState.WeaponRetractionMoving:
                    {
                        // 上面几种都是 配置的通用状态
                        stateName = Enum.GetName(typeOfState, layerState);
                    }
                    break;
                case E_ULayerSubState.InterAction1:
                case E_ULayerSubState.InterAction2:
                    {
                        // 交换物 也是两个状态相互切,但是 不走 配置中的状态融合时间,所以就用默认的时间就可以了
                        stateName = Enum.GetName(typeOfState, layerState);
                    }
                    break;
                case E_ULayerSubState.BeingControl:
                case E_ULayerSubState.BegineKnockDown:
                case E_ULayerSubState.KnockDown:
                case E_ULayerSubState.EndKnockDown:
                case E_ULayerSubState.Deading:
                    {
                        // 倒地/死亡 目前都是 倒地状态配置
                        stateName = "Deading";
                    }
                    break;
                case E_ULayerSubState.IsSonscious:

                case E_ULayerSubState.Skill_Common1:
                case E_ULayerSubState.Skill_Common2:
                    {
                        stateName = "Skill";
                    }
                    break;
                default:
                    break;
            }

            AnimancerEnterTimeDataCell dataCell = LocalDataManager.Instance.GetAnimancerEnterTimeDataCell(stateName);
            // 如果 这个状态的配置 找不到, 就直接返回默认的动画过渡融合时间 -1
            if (dataCell == null)
            {
                return -1;
            }
            //SGF.Debuger.LogError($"[VitalState] state {this.name} , 设置默认状态 {layerState} 配置 FadeTimeMilSeconds: {dataCell.GetStatrEnterTime()} ");

            return dataCell.GetStatrEnterTime();
        }

        public override void OnExitState()
        {
            base.OnExitState();
            speed = 1;
            IsBacktrackDefault = true;
            ChangeCond = ChangeStateConditions.DefToIdle;
            OnVitalStateChanged?.Invoke(this, VitalStateEnum.End);
            ActionEndState?.Invoke();
            ActionEndState = null;

            ForbidRset2IdleOnEnd = false;
        }

        public void PlayWwise(string path)
        {
            var wwisePath = path.Split("|");
            if (wwisePath.Length < 2)
            {
                SGF.Debuger.LogWarning($"[VitalState] PlayWwise: {path} 路径报错");
                return;
            }
            string soundBank = wwisePath[0];
            string wwiseName = wwisePath[1];

            Service.Sound.SoundManager.Instance.PostSoundBankEvent(soundBank, wwiseName, this.gameObject, this.gameObject);
        }

        // private void RegisterAnimationEvent(AnimationEvent[] events)
        // {
        //     var curTime = _clipTransition.State.Time;
        //     AnimEvents.Clear();
        //     LastAnimTime = curTime;
        //     for (int i = 0; i < events.Length; i++)
        //     {
        //         animEvent = events[0];

        //         // 动画帧不过滤，因为存在 loop 动画， 需要循环播放.
        //         AnimEvents.Add(animEvent);
        //     }
        //     animCount = AnimEvents.Count;
        // }

        public void PlayAnimEvent(AnimationEvent animationEvent)
        {
            switch (animationEvent.functionName)
            {
                case "PlayWwise":
                    {
                        PlayWwise(animationEvent.stringParameter);
                    }
                    break;

                default: break;

            }
        }

        AnimationEvent animEvent;
        int animCount = 0;
        private void Update()
        {
            // 先屏蔽, 等博哥 拍板
            return;
            // 考虑到 动画 speed 可能还会变, 所以放入 update 中去取 动画当前时间会比较准确
            float curTime = _clipTransition.State.Time;
            // 如果当前时间 小于 上一次的 动画执行时间，那说明 动画执行完进入了下一轮
            bool hasLoop = curTime < LastAnimTime;

            for (int i = 0; i < animCount; i++)
            {
                animEvent = AnimEvents[i];

                // 如果没有 loop 循环动画, 那就是 正常 按 curTime 来播放动画事件帧
                if (!hasLoop)
                {
                    // 如果 动画事件的 时间 > 当前动画时间, 就表明 没有后续事件需要执行, 跳出循环
                    if (animEvent.time > curTime)
                    {
                        break;
                    }
                    // 如果动画时间  小于 当前动画时间，表明 这个动画 已经执行过了, 那就跳过
                    if (animEvent.time <= LastAnimTime)
                    {
                        continue;
                    }
                }
                else
                {
                    // 如果有了loop, 说明动画 执行过了 尾帧， 从新开始了新一轮动画, 那就需要 1. 执行 LastAnimTime ---> 动画尾帧的 事件帧; 2: 执行0 ---> curTime 的动画帧
                    if (animEvent.time > curTime && animEvent.time <= LastAnimTime)
                    {
                        continue;
                    }
                }

                PlayAnimEvent(animEvent);
            }

            LastAnimTime = curTime;
        }

    }
}
