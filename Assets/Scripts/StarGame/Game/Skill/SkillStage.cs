using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Time;
using SkillEditor;
using StarProject.CustomDataStruct;
using StarProject.Game.Entity;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using EffectData = SkillEditor.EffectData;

namespace StarProject.Game.Skill
{
    public class SkillStage : EntityLocalDynamic
    {
        /// <summary>
        /// 阶段的 拼接是 stageID_loopIdx ， 目前预留  32 位, 基本可以满足. 后续有需求可以加
        /// StageID 一般 不会 超过 10位 长度;
        /// LoopIdx 一般 不会 超过 5位 长度;
        /// </summary>
        /// <returns></returns>
        private static StringBuilder sb = new StringBuilder(32);

        /// <summary>
        /// 一份 FormatStageIDStr 缓存的 StageIDStr 记录的 cache。主要是为了防止 频繁执行 FormatStageIDStr 造成的字符串 gc
        /// </summary>
        private static Dictionary<int, string> StageIDCacheDic = new();

        private static int tmpValue = 0;
        /// <summary>
        /// 阶段 StageIDStr 的获取 接口。
        /// </summary>
        private static string GetFormateStageIDStr(int stageID, int loopIdx)
        {
            // loopIdx 从1 开始, 一般不会很多. 目前对 策划 stageID 的限制 在 1-9999 之间, 所以 将 loop 设置位 10w+
            // 这样能够保证 stageID 和 loopIdx 永远不重复
            // 10000*100000 = 1000000000 < intMax
            loopIdx += 100000;

            // 生成 唯一的 StageIDCacheDic 的 key值
            // 此处 没用 stageID 和 loopIdx 的 2层 dictionary 嵌套是为了 减少 一层 dic的 创建和查找.
            tmpValue = stageID * loopIdx;

            if (!StageIDCacheDic.ContainsKey(tmpValue))
            {
                sb.Clear();
                StageIDCacheDic.Add(tmpValue, sb.Append(stageID).Append("_").Append(loopIdx).ToString());
            }
            return StageIDCacheDic[tmpValue];
        }

        public static string FormatStageIDStr(int stageID, int loopIdx)
        {
            return GetFormateStageIDStr(stageID, loopIdx);
        }

        /// <summary>
        /// 进入阶段的 帧 补充修复时间. 
        /// 那 [阶段的 总运行时间]  = [阶段的初始的 帧 补充时间] + [阶段的实际运行时间]
        /// </summary>
        private double enterStageFixTime = 0;

        /// <summary>
        /// 阶段的实际运行时间;
        /// </summary>
        private double stageRunningTime = 0;
        /// <summary>
        /// 阶段的总运行时间,其实 就是 阶段进入的 补帧时间 + 阶段时机的运行时间
        /// </summary>
        private double stageTotalRunningTime => enterStageFixTime + stageRunningTime;         //当前自己逻辑阶段，已运行时间

        public StageInfo curStageInfo;

        /// <summary>
        /// 退出这个阶段 执行的Action, 正常阶段超时 或者 外部结束阶段都会走
        /// <exitStageTime ,E_SkillStageExitType> : <退出阶段的时间 , 退出阶段的类型>
        /// </summary>
        public Action<SkillStage, double, E_SkillStageExitType> ActionOnExitStage;


        /// <summary>
        /// 阶段开启CD
        /// </summary>
        public Action<SkillStage> ActionOnStageStartCD;


        /// <summary>
        /// 刷新阶段原子状态的 Action
        /// <StageID, states , 注册/取消> 
        /// </summary>
        public Action<string, List<int>, bool> ActionOnRefreshStageStates;

        /// <summary>
        /// 阶段尝试播放动画的Func
        /// </summary>
        public Func<SkillStage, AnimationJson, bool, float, bool> FuncOnStageTryPlayAnim;
        public Action<SkillStage, FXJson, int> ActionOnStageTryPlayFx;
        public Action<SkillStage, SoundJson, int> ActionOnStageTryPlaySound;
        public Action<SkillStage, SoundJson> ActionOnStageTryStopSound;
        public Action<SkillStage, FXJson> ActionOnStageTryStopFx;

        /// <summary>
        /// <SkillStage, EffectData, bool, bool, int>  ----> <SkillStage, EffectData, isRecover, isFramStart, stageRecoverTime>
        /// </summary>
        public Action<SkillStage, EffectData, bool, bool, int> ActionOnStageTryPlayEffect;
        public Action<SkillStage, EffectData> ActionOnStageTryStopEffect;

        public Action<SkillStage, CameraJson, bool> ActionOnStageTryPlayCamera;

        /// <summary>
        /// 阶段注册 服务器效果线 效果的接口
        /// </summary>
        public Action<SkillStage, EffectData> ActionOnStageRegisterServerEffect;

        public Action<SkillStage, CameraShakeJson, bool> ActionOnStageTryPlayCameraShake;

        /// <summary>
        /// 阶段 写入技能黑板
        /// note :
        ///     1. 阶段是通用的逻辑,它与技能不一定相关;
        ///     2. action.Invoke 的时候, action 可能 为null;
        /// </summary>
        public Action<SkillStage, string, object, E_BlackBoardTag> ActionOnStageWriteSkillEffect;

        /// <summary>
        /// 服务器定义的 特殊的黑板 key的 处理action
        /// </summary>
        public Action<SkillStage, BlackBoardNode> ActionOnSpecialServerBlackBoardNode;

        /// <summary>
        /// 尝试执行注册了的 服务器效果Func
        /// note1：
        ///     当阶段尝试 执行服务器发过来的效果 数据(BlackBoardNode)的时候,
        ///     如果不解开黑板数据,其实就无法知道 这个数据到底需要存在于哪个黑板中,
        ///     而对于阶段来说,阶段执行效果,其实并不需要知道 效果的细节.
        /// noet2:
        ///     Q: 如上所述,此处应该传skillStage而不是BaseBlackBoard才对啊(因为服务器发过来的数据是outPutkey,此处确实不知道效果黑板存在那个黑板里面)?
        ///     A: 那此处就要分情况处理
        ///         1.对于阶段正常的 OnStageRet 来说,阶段的效果 服务器会通过阶段运行时同步,
        ///           所以 正常运行的OnStageRet收到的效果,一定存在于阶段运行时;
        ///         2.对于技能 全同步的 阶段恢复来说, 如果是恢复阶段,那有两条:
        ///             a.阶段恢复,只恢复阶段自己的数据.
        ///               所以阶段恢复的只是阶段的数据,并不需要执行FuncOnTryPlayRegistedServerEffect,
        ///             b.且数据均是阶段数据.
        ///             
        /// </summary>
        public Func<CustomBlackBoardNode, BaseBlackBoard, bool> FuncOnTryPlayRegistedServerEffect;

        /// <summary>
        /// 阶段尝试 执行 抢占逻辑的 Func
        /// 只有需要设置 抢占成功 或者 取消抢占才会设置，
        /// 阶段抢占 失败这种逻辑不需要设置
        /// </summary>
        public Func<SkillStage, double, bool, bool, bool> FuncOnStageTrySetActiveMain;

        /// <summary>
        /// 阶段 事件帧 的队列
        /// </summary>
        /// <typeparam name="FrameEvent"></typeparam>
        /// <returns></returns>
        public QueueExtends<SkillStageFrame> frameEventsQueue = new QueueExtends<SkillStageFrame>();

        /// <summary>
        /// 已经执行的帧事件的队列
        /// </summary>
        /// <typeparam name="SkillStageFrame"></typeparam>
        /// <returns></returns>
        public QueueExtends<SkillStageFrame> executedFrameEventsQueue = new QueueExtends<SkillStageFrame>();


        public List<SkillStageFrame> frameEventsList = new List<SkillStageFrame>();

        /// <summary>
        /// next 效果对应的 事件帧的 map
        /// </summary>
        /// <typeparam name="int">效果id</typeparam>
        /// <typeparam name="SkillStageFrame"></typeparam>
        public Dictionary<int, SkillStageFrame> nextEffectFrameEventMap = new Dictionary<int, SkillStageFrame>();

        public Dictionary<int, int> Next2ParentEffectMap = new Dictionary<int, int>();

        private int _stageID = 0;

        public int StageID => _stageID;

        private string _stageIDStr = "";
        public string StageIDStr => _stageIDStr;

        /// <summary>
        /// 阶段的 UID, 在 runStageRet 中 服务器通知后客户端才能确定 UID,
        /// 因此 目前主要 跟服务器 同步的方式 是 采用 StageIDStr 的方式.
        /// 对于 阶段运行时强制结束消息 RunStageForceEndRet 这种, 服务器 想用 UID 直接确定,那也可以
        /// </summary>
        public ulong StageUID = 0;

        public int StageIdx => curStageInfo.StageIdx;

        public int Time => curStageInfo.Time;

        // 阶段的打断 改为 由 服务器处理,客户端 接收服务器 打断跳转到哪个阶段,所以此处没有引用
        public E_SkillStageBreakType StageBreakType => curStageInfo.StageBreakType;

        // public int SkipStageID => curStageInfo.JumpStageID;
        /// <summary>
        /// 阶段的攻速
        /// </summary>
        public float Speed = 1;

        /// <summary>
        /// 在 循环阶段中的第几阶段
        /// </summary>
        private int _loopIdx = 0;

        public int LoopIdx => _loopIdx;

        private long serverCreteTime = 0;

        /// <summary>
        /// 当前阶段 的服务器创建时间, 用来做 同步时的阶段同步。
        /// note:
        ///     注意: 对于 循环阶段,服务器的 CreateTime 是第一个循环阶段的创建时间.
        ///           而客户端 此处的创建时间 是实际的 当前阶段的创建时间, 外部直接用就可以了
        /// </summary>
        public long ServerCreteTime => serverCreteTime;

        /// <summary>
        /// 技能阶段的黑板
        /// </summary>
        private StageBlackBoard _stageBlackBoard = new StageBlackBoard();

        public StageBlackBoard StageBlackBoard => _stageBlackBoard;

        /// <summary>
        /// 原子状态 是否已经注册的记录
        /// note:
        ///     防止被多次注册或者多次取消
        /// </summary>
        private bool isRegisterStates = false;


        private bool IsEntryCD => curStageInfo.IsEntryCD;

        /// <summary>
        /// 是否是需要抢位 的阶段
        /// </summary>
        public bool NeedActive => curStageInfo.NeedActive;

        /// <summary>
        /// 保持 active 的标志, 阶段 配置了 抢占 活跃,但是 可能被 打断, 此时需要等待服务器同步,
        /// 在等待期间,  这个阶段的后续 动画 应该 不打断 当前活跃阶段的 动画.  所以 当这个阶段
        /// 被 打断的时候, 将 keepActive 设置 为false
        /// 
        /// </summary>
        private bool keepActive = true;

        /// <summary>
        /// 阶段 是否 保持 Active, 阶段虽然 需要抢占, 但是 可能因为 被抢占失败, 而被设置为 非活跃阶段.
        /// note:
        ///     目前 阶段 如果 是 被 自己的 技能抢占 而打断, 此时 客户端 不做 阶段打断的预演, 而完全依赖服务器
        ///     阶段 强制打断的通知. 
        /// note2:
        ///     此时 就会有个 网络延迟问题, 这个阶段 明明 客户端 可以预演被打断了,但是 服务器通知还没到,从而 这个阶段
        ///     的 一些 其它 事件帧 也开始 执行. 
        ///     所以 此处 增加 一个 标识 KeepActive， 用来 标识 阶段是否 任然保持 活跃
        /// </summary>
        public bool Active => keepActive && NeedActive;

        /// <summary>
        /// 阶段是否启动, enter 后 为 true, exit 后为 false
        /// 增加这个变量,主要是 防止 阶段退出了,但是还响应了 update的逻辑
        /// </summary>
        private bool stageEnable = false;

        private bool stageInit = false;

        private bool stageClose = false;


        /// <summary>
        /// [Enter][Update][onExit]持续运行中的“标识”
        /// 与[被关掉]无关
        /// </summary>
        public bool StageEnable
        {
            get => stageEnable;
            set
            {
                stageEnable = value;
                // // SGF.Debuger.Log($"{TagFlag} {this.StageIDStr} stageTotalRunningTime {stageTotalRunningTime} stageEnable : {stageEnable}  ");
            }
        }

        /// <summary>
        /// 阶段是否初始化
        /// </summary>
        /// <value></value>
        public bool StageInit
        {
            get => stageInit;
        }

        /// <summary>
        /// 等待下一帧 才执行 update 的标记.
        /// 技能 执行预播的时候,目前是在 当前帧 就添加进入 技能实体队列的.
        /// 那么 此时就需要标记 这个技能 waitNextFrameUpdate = true;
        /// 这样,这个技能实体 在当前这一次 的update中 不执行, 而是在下一个update中执行.
        /// </summary>
        private bool waitNextFrameUpdate = false;

        /// <summary>
        /// 是否执行了 OnExit逻辑. 阶段执行了 OnExit后 就会标记为 ReadyRelease. 
        /// 
        /// </summary>
        public bool ReadyRelease = false;

        private string _tagStr = "";

        private ulong _builderID = 0;
        public ulong BuilderID => _builderID;

        private ulong _ownerEntityID = 0;
        public ulong OwnerEntityID => _ownerEntityID;

        /// <summary>
        /// 阶段的最大 时间，取的是串行效果 和 阶段时间的最大值
        /// </summary>
        public int MaxStageTime;

        /// <summary>
        /// 是否跳过 循环阶段
        /// </summary>
        private bool _isSkipLoopStage = false;

        /// <summary>
        /// 是否跳过 循环阶段, 当 当前阶段 是 循环阶段 && 设置了_isSkipLoopStage = true 的时候,才需要跳过后续 循环阶段
        /// </summary>
        public bool IsSkipLoopStage => _isSkipLoopStage && curStageInfo.IsStageLoop;

        /// <summary>
        /// 当前阶段 是否是 循环阶段
        /// </summary>
        public bool IsStageLoop => curStageInfo.IsStageLoop;

        public bool IsNormalStage => curStageInfo.IsNormalStage;

        /// <summary>
        /// 服务器创建阶段, 目前 阶段被服务器创建的时候, 会向外通知. 比如 技能的预输入,需要在阶段被服务器创建的时候,
        /// 主动触发.
        /// </summary>
        public bool ServerCreateStage = false;

        /// <summary>
        /// 服务器创建的 阶段通知
        /// </summary>
        public Action<SkillStage> ActionOnServerStageCreate;

        public int _leastTriggerCount = 1;

        /// <summary>
        /// 剩余的 阶段 可以被触发的次数. 
        /// 对于 非触发器阶段来说，阶段只能被触发一次。
        /// 对于 触发器 阶段来说, 阶段上的效果的 可触发次数 为 配置次数
        /// </summary>
        public int LeastTriggerCount => _leastTriggerCount;


        /// <summary>
        /// 阶段 
        /// </summary>
        /// <param name="stageInfo"></param>
        /// <param name="loopIdx">阶段处于 循环阶段中的第几阶段</param>
        /// <param name="stageIdx">阶段处于 配置表中的idx,只是为了根据 stageIdx 反向定位在配置表中的位置</param>
        /// 
        public void Init(StageInfo stageInfo, int loopIdx, string tagStr, ulong builderID, ulong ownerEntityID, BaseBlackBoard parentBlackBoard)
        {
            curStageInfo = stageInfo;
            _loopIdx = loopIdx;

            _stageID = stageInfo.StageID;
            _stageIDStr = SkillStage.FormatStageIDStr(_stageID, _loopIdx);

            _tagStr = tagStr;

            TagFlag = $"[SkillStage_{StageIDStr}], {_tagStr} ,";

            _builderID = builderID;
            _ownerEntityID = ownerEntityID;

            MaxStageTime = stageInfo.MaxStageTime;

            // 在阶段创建的时候,即将黑板 赋予阶段,这样就不会出现 因为阶段没进onEnter，黑板不存在的问题
            _stageBlackBoard.parent = parentBlackBoard;
            InitFrameQueue();

            // 初始化 阶段的 剩余 可触发次数.
            _leastTriggerCount = stageInfo.TriggerCount;

        }

        private void InitFrameQueue()
        {
            List<SkillStageFrame> frames = curStageInfo.TimeLineFrames;
            // List<SkillStageFrame> frames = curStageInfo.frames;

            for (int i = 0; i < frames.Count; i++)
            {
                RegFrameAction(frames[i]);
            }
            frameEventsQueue.Clear();
            frameEventsQueue.HeadEnqueue(frames.KToQueue<SkillStageFrame>());
            frameEventsList = frameEventsQueue.KToList<SkillStageFrame>();

            Dictionary<int, SkillStageFrame> nextFrames = curStageInfo.NextEffectFrameMap;
            nextEffectFrameEventMap.Clear();
            foreach (KeyValuePair<int, SkillStageFrame> item in nextFrames)
            {
                RegFrameAction(item.Value);
                nextEffectFrameEventMap.Add(item.Key, item.Value);
            }

            Dictionary<int, int> next2ParentEffectMap = curStageInfo.Next2ParentEffectMap;
            Next2ParentEffectMap.Clear();
            foreach (KeyValuePair<int, int> item in next2ParentEffectMap)
            {
                Next2ParentEffectMap.Add(item.Key, item.Value);
            }
        }


        private void RegFrameAction(SkillStageFrame frame)
        {
            frame.ActionOnPlayAnimation += OnActionPlayAnimation;
            frame.ActionOnPlayFx += OnActionPlayFx;
            frame.ActionOnPlaySound += OnActionPlaySound;
            frame.ActionOnStopSound += OnActionStopSound;
            frame.ActionOnPlayEffect += OnActionPlayEffect;
            frame.ActionOnStopEffect += OnActionStopEffect;
            frame.ActionOnPlayCamera += OnActionPlayCamera;
            frame.ActionOnPlayCameraShake += OnActionPlayCameraShake;
            frame.ActionOnStopFx += OnActionStopFx;
        }

        private void UnregFrameAction(SkillStageFrame frame)
        {
            frame.ActionOnPlayAnimation -= OnActionPlayAnimation;
            frame.ActionOnPlayFx -= OnActionPlayFx;
            frame.ActionOnPlaySound -= OnActionPlaySound;
            frame.ActionOnStopSound -= OnActionStopSound;
            frame.ActionOnPlayEffect -= OnActionPlayEffect;
            frame.ActionOnStopEffect -= OnActionStopEffect;
            frame.ActionOnPlayCamera -= OnActionPlayCamera;
            frame.ActionOnPlayCameraShake -= OnActionPlayCameraShake;
            frame.ActionOnStopFx -= OnActionStopFx;
        }
        public EffectData GetEffectData(int effectID)
        {
            EffectData effectData = null;
            if (curStageInfo.EffectDatasDic.TryGetValue(effectID, out effectData))
            {
                return effectData;
            }
            return null;
        }

        public EffectData GetNextEffectDataWhileOutputkeyNotNull(int effectID)
        {
            EffectData effectData = GetEffectData(effectID);
            while (effectData != null)
            {
                GetEffectDataOutputKey(effectData, out string outputKey, out E_SkillEffect skillEffect);
                // 如果找到了 outputKey 的next 效果, 那就 直接返回这个 effectData
                if (outputKey != "")
                {
                    break;
                }
                if (effectData.Next == null)
                {
                    return null;
                }
                // 如果此时的 outputKey 为 "", 那理论上这个 效果的 next 要么为空, 要么 只有1个 (多个的话,就是选择效果,那一定要有 outputkey)
                if (effectData.Next.Length > 1)
                {

                    SGF.Debuger.LogWarning($"[SkillStage] 效果 effectID: {effectData.EffectID}, desc: {effectData.Desc} 存在多个next 效果， 但是缺少outputKey, error!!!");
#if UNITY_EDITOR
                    UnityEngine.Debug.Break();
#endif
                    break;
                }
                // 如果 没有后续的 next 效果了,那就是找不到后续 拥有outputKey的效果
                if (effectData.Next.Length == 0)
                {
                    return null;
                }
                // 如果存在 一个 next 效果, 那就递归去找 这个next 效果
                effectData = GetNextEffectDataWhileOutputkeyNotNull(effectData.Next[0]);
            }
            return effectData;
        }

        /// <summary>
        /// 得到效果 在 阶段上的运行时间
        /// </summary>
        /// <param name="effectID"></param>
        /// <returns></returns>
        public int GetEffectStageTime(int effectID)
        {
            return curStageInfo.GetEffectStageTime(effectID);
        }



        /// <summary>
        /// 每个阶段进入执行的流程
        /// </summary>
        /// <param name="enterStageTime">进入阶段携带的已经消耗的时长</param>
        /// <param name="enterType">进入技能阶段时的类型</param>
        /// <param name="force"> 是否强制 设置活跃, 只有服务器 同步的技能,才需要强制设置 活跃技能</param>
        public void OnEnter(double enterStageTime, E_SkillStageEnterType enterType, bool force)
        {
            ReadyRelease = false;
            StageEnable = true;

            {
                /// 2023/4/12
                /// waitNextFrameUpdate 用来控制 当前的阶段执行 update的时候 ,是否需要跳过当前帧,等到下一帧执行.
                /// 如果是 第一个技能的 第一个阶段, 那么技能 和阶段的 补帧 时间 都是 0, 此时
                /// 阶段进入后, 当前帧已经 根据  enterStageTime 补齐了,所以当前帧 不执行 onUpdate
                if (enterStageTime > 0 && enterStageTime <= TimeUtils.FixedDeltaTime)
                {
                    waitNextFrameUpdate = true;
                }
            }



            // 阶段进入的时候,记录一个 阶段的 帧补充时间 
            //     如果是 客户端 提前预播的 话, 阶段 进入时,传入的 enterStageTime 就是 阶段 ---> 阶段间 帧的补充时间
            //     (因为 阶段结束是在帧尾结束, 但阶段长度并不一定刚好落在帧尾,可能在帧的中间,所以需要在进入下个阶段的时候,补充这个间隔时间)
            //     如果是 服务器恢复阶段逻辑,那就是  阶段的 恢复时间
            enterStageFixTime = enterStageTime;

            //先执行抢位逻辑,不管enterStageTime 花了多久,正常流程
            //recover恢复阶段 会直接 返回 result = true;
            //DB_Close   LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [v-v] OnEnter type {enterType} , enterStageTime {enterStageTime} ,maxStageTime {MaxStageTime},Time {curStageInfo.Time}ms, StageEnable {StageEnable}");

            bool result = ExcuteActiveMain(enterStageTime, enterType, force);
            if (!result)
            {
                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} OnEnter ExcuteActiveMain result {result}");
            }
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 阶段进入 OnEnter type {enterType} , stageTotalRunningTime {stageTotalRunningTime} ,maxStageTime {MaxStageTime},Time {curStageInfo.Time}ms, StageEnable {StageEnable} ,active: {curStageInfo.NeedActive}, result: {result}");

            // ExcuteActiveMain , 设置活跃/非活跃阶段,可以触发 用户输入轴效果,从而继续切阶段
            // 导致这个阶段在进入的时候即 退出, 所以此时 StageEnable 可能在 执行完ExcuteActiveMain 后
            // 就变为false， 从而退出阶段
            {
                // 如果 执行完 ExcuteActiveMain 后, StageEnable 还是false,那 说明这个阶段被打断了
                if (!StageEnable)
                {
                    return;
                }
            }



            if (!result)
            {
                switch (enterType)
                {
                    case E_SkillStageEnterType.Default:
                        {
                            //如果是正常的阶段类型,阶段的抢位失败,直接结束技能运行时
                            OnExit(enterStageTime, E_SkillStageExitType.FailedSetMainSkill);
                        }
                        break;
                    case E_SkillStageEnterType.SkillBreak:
                        {
                            //如果是技能被打断时，进入的技能阶段，这个时候这个阶段不能够抢位
                            //note:
                            //      技能打断类型的阶段进入时，目前跟 服务器和策划的 约定是不需要抢位,
                            //      如果配置中此技能阶段仍需要抢位, 那就跳到下一个阶段，
                            //      直到找到一个不需要抢位的阶段或者 技能阶段结束, 技能结束
                            OnExit(enterStageTime, E_SkillStageExitType.Broken);
                        }
                        break;

                    default:
                        {
                            //如果时正常的阶段类型,阶段的抢位失败,直接结束技能运行时
                            OnExit(enterStageTime, E_SkillStageExitType.FailedSetMainSkill);
                        }
                        break;
                }
                return;
            }


            // 说明 客户端先跑了,服务器后面通过 skillUseRet进来, 所以 enterStageTime < stageTotalRunningTime
            if (enterStageTime < stageTotalRunningTime)
            {
                return;
            }



            //1.如果进入阶段后,enterStageTime > Time,那说明 阶段 已经过了最大时长，走的是服务器 的效果恢复流程
            if (curStageInfo.Time <= enterStageTime)
            {
                // 此时, 应该设置 stageTotalRunningTime 为 阶段的配置时间 curStageInfo.Time
                stageRunningTime = curStageInfo.Time - enterStageTime;
                ExecuteEnter();
                //执行 这个阶段上 所有事件帧 的 恢复逻辑
                ExecuteFrameEvents(true, false, enterStageTime);

                RecoverStageLines();

                // 目前阶段的恢复逻辑 是根据 服务器发过来的 阶段创建时间执行的恢复.
                // 所以当这个阶段执行完恢复逻辑后,进入后续阶段,会先判断是否是恢复阶段,
                // 如果不是,由于 阶段是 在时间轴 上连续的 , 所以不会出现 stage1 客户端预播, stage2需要走恢复,这种情况(除非效果线结束,服务器这个阶段都不会发过来).
                // 所以从这个阶段的 恢复逻辑 退出的时候, 后续阶段的  补充时间 为0 , 后续阶段也不会使用这个时间.
                OnExit(0, E_SkillStageExitType.Recover);

                return;
            }


            ExecuteEnter();

            bool isRecover = false;

            if (enterType == E_SkillStageEnterType.Recover)
            {
                isRecover = true;
            }
            else if (enterType == E_SkillStageEnterType.RecoverCurStage)
            {
                // 如果是恢复当前阶段, 就有几种情况:
                // 1.怪物使用技能, 玩家看到,此时 去掉网络延迟后,  enterStageTime 在 0 ~ 100ms 之间, 此时 可以当作这个阶段 不走恢复;
                if (enterStageTime <= 100)
                {
                    isRecover = false;

                }
                else
                {
                    isRecover = true;
                }
            }


            // 2023/4/11
            // 感觉 不应该根据 阶段的补帧时间 判断是否走 回复阶段逻辑, 因为 卡帧的时候,阶段进入的补帧时间 确实可能大于 1帧
            {
                /// 2023/2/8
                ///     如果 想要根据 enterStageTime 的大小来判定是否是 执行恢复逻辑, 则需要修改
                ///     当前 效果的恢复逻辑, 恢复逻辑 也需要执行 客户端效果线, 同时,根据 恢复 传入的 恢复时间,
                ///     去 恢复 对应的效果.
                ///     这个可能要 不同的效果, 分别来处理。
                ///  如果 阶段的进入时间 小于 1帧 , 一般都是 阶段过渡 时，从上一个阶段带来的过渡时间.
                ///  对于过渡时间,这种 就直接按默认方式 进入阶段即可.
                ///  但如果大于 1帧 , 那 基本上 是由于服务器 同步而进入的阶段, 这个时候, 就是直接用 恢复的方式进入阶段
                // bool less1Frame = enterStageTime <= TimeUtils.FixedDeltaTime;
                // bool isServerRecover = !less1Frame || isRecover;
            }
            // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} OnEnter type = {enterType} , stageTotalRunningTime {stageTotalRunningTime}ms , Time {curStageInfo.Time}ms, MaxStageTime {MaxStageTime}ms,");
            // 阶段进入时, 执行对于的事件帧, 对于 技能的 恢复类型进入, 也是按 恢复的方式执行事件帧
            ExecuteFrameEvents(isRecover, false, enterStageTime);

            RecoverStageLines();
            //执行进入阶段的CD逻辑
            ExecuteStageCD();

            // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} OnEnter type = {enterType} , stageTotalRunningTime {stageTotalRunningTime} , Time {curStageInfo.Time} , MaxStageTime {MaxStageTime}");
        }

        /// <summary>
        /// 执行 enter相关的 阶段逻辑
        /// </summary>
        private void ExecuteEnter()
        {
            StageEnable = true;

            stageInit = true;

            stageClose = false;

            ExecuteStageStates(true);

        }

        /// <summary>
        /// 每个阶段的退出执行流程
        ///     阶段的退出接口 由阶段自己调用
        /// </summary>
        /// <param name="exitStageTime"></param>
        /// <param name="type"></param>
        private void OnExit(double exitStageTime, E_SkillStageExitType type)
        {
            /// 防止多次退出
            /// 正常的 ExecuteFrameEvents 可能会退出阶段,
            /// 而如果恰好 ExecuteFrameEvents 后 执行阶段超时逻辑,
            /// 可能导致多次执行OnExit
            if (StageEnable == false)
            {
                return;
            }
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 阶段退出 OnExit type {type} , stageTotalRunningTime {stageTotalRunningTime} , exitStageTime: {exitStageTime}");

            // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [input] OnExit   type = {type} , stageTotalRunningTime {stageTotalRunningTime}");
            ExecuteExit(type);

            ActionOnExitStage?.Invoke(this, exitStageTime, type);
        }

        /// <summary>
        /// 执行 阶段 各种状态的退出, 是为了 阶段退出/ 技能退出 时,能够公用一个接口
        /// </summary>
        private void ExecuteExitStates()
        {
            StageEnable = false;
            ReadyRelease = true;
            ExecuteStageStates(false);
        }

        /// <summary>
        /// 执行阶段的退出流程
        /// </summary>
        /// <param name="type"></param>
        private void ExecuteExit(E_SkillStageExitType type)
        {
            ExecuteExitStates();
            ExecuteExitStageFrameEvent(type);
        }

        /// <summary>
        /// TODO : DL
        ///  E_SkillStageExitType 和 E_SkillExitType 后面看看能不能统一类型
        /// </summary>
        /// <param name="type"></param>
        private void ExecuteExit(E_SkillExitType type)
        {
            //LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} ExecuteExit  type {type}  stageTotalRunningTime {stageTotalRunningTime}");
            ExecuteExitStates();
            ExecuteExitStageFrameEvent(type);
        }

        /// <summary>
        /// 主动 结束阶段的接口
        /// </summary>
        /// <param name="type"></param>
        public void ExitStage(E_SkillStageExitType type)
        {
            /// 2023/4/12
            /// 主动结束阶段,需要计算出 当前帧结束这个技能 是否需要给后续技能补帧
            /// 1.如果 打断的时候, 当前帧尾 在阶段的中间点, 即 并不是 阶段结束才打断, 那么对于 阶段中间的 打断，
            ///   可以认为 是在这个帧尾打断即可, 后续的 补帧 时间为 0.
            /// 2.如果 打断的时候, 帧尾 在阶段的 后面,表明 阶段结束 在这一帧的中间点,此时需要为
            ///   后续的 阶段/技能 补充 时间.
            /// 3.如果 runningStageTime == 0, 说明阶段是 进入即被打断,此时退出的时候,后续阶段的 帧补充时间就是 
            ///   进入这个阶段 的 帧补充个时间

            double exitStageTime = 0;
            if (stageTotalRunningTime > curStageInfo.Time)
            {
                exitStageTime = stageTotalRunningTime - curStageInfo.Time;
            }
            else if (stageRunningTime == 0)
            {
                exitStageTime = enterStageFixTime;
            }
            // LogUtils.Log(LogUtils.LogEnum.Skill, $"{TagFlag} [v-v] 结束阶段 ExitStage ,Time: {curStageInfo.Time}, stageTotalRunningTime {stageTotalRunningTime}, 补充时间: {exitStageTime}");

            OnExit(exitStageTime, type);
        }

        /// <summary>
        /// 技能结束,退出阶段的入口
        /// </summary>
        /// <param name="skillExitType"></param>
        public void OnExitSkill(E_SkillExitType skillExitType)
        {
            //技能运行时结束,需要依次退出所有的阶段效果,
            //不需要走ActionOnExitStage 通知外面,只需要执行阶段退出逻辑即可
            ExecuteExit(skillExitType);
        }

        public void OnUpdate()
        {
            // 如果 需要等待 一帧,那就 本帧 跳过
            if (waitNextFrameUpdate)
            {
                waitNextFrameUpdate = false;
                return;
            }
            if (!stageInit)
            {
                return;
            }

            // 如果阶段 完全关闭了,那就 不需要执行 update
            if (stageClose)
            {
                return;
            }
            //TODO:[技能速度修改]
            {
                /// TODO: DL
                /// 2023/4/12
                /// 此处 可能 会存在 时间点 不准的 问题:
                /// FixedUpdate 经测试 并没有真正的 按照 TimeUtils.FixedDeltaTime 的时间运行.
                /// 那么 此时 客户端 每次 FixedUpdate 累加的时间 就不一定是  技能的实际时间长度.
                /// 
                /// 不过误差范围暂时看还比较小.后面 看情况 是否需要 改为实际的时间长度
                stageRunningTime += TimeUtils.FixedDeltaTime * Speed;
            }

            if (stageTotalRunningTime < curStageInfo.Time + 80)
            {
                // LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [v-v] OnUpdate stageRunningTime {stageRunningTime} ,stageTotalRunningTime: {stageTotalRunningTime}");

            }

            // 如果阶段时间超过 阶段时间线的 最大时间, 那 其实就应该关闭 整个阶段
            // 此处需要 增加 ping的 延迟，策划配在效果线的尾部 
            // note:
            //      MaxStageTime 要 大于 curStageInfo.Time , 如果相等, 可能提前结束导致 没有进入下个阶段
            // LogUtils.LogError(LogUtils.LogEnum.Skill,$"TimeUtils.AveragePing :{TimeUtils.AveragePing}");
            if (stageTotalRunningTime > (MaxStageTime + 100))
            {
                stageClose = true;
                // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  OnExit stageTotalRunningTime {stageTotalRunningTime}");
                ActionOnExitStage?.Invoke(this, stageTotalRunningTime, E_SkillStageExitType.OverMaxStageTime);
                return;
            }

            // 阶段只要超过 阶段的Time, 阶段即可认为 StageEnable== false;
            // 此时,不需要去 执行 阶段中 播放事件帧的逻辑
            if (!StageEnable)
            {
                return;
            }

            ExecuteFrameEvents(false);
            // SGF.Debuger.Log($"{TagFlag} [input] OnUpdate stageTotalRunningTime {stageTotalRunningTime}");


            // 阶段超时退出时,执行完剩余所有的帧
            // note:
            //    1.一帧的时间是33ms,所以对于阶段进入或者阶段退出,都可能存在33ms以内的误差,导致漏帧
            //    2.如果时间 超过当前阶段时间，需要执行超时退出 这个阶段.
            //    3.如果ExecuteFrameEvents 中存在退出阶段的逻辑, 需要防止 多次 OnExit 
            if (stageTotalRunningTime > curStageInfo.Time)
            {
                ExecuteFrameEvents(false, true);     // [循环执行] 阶段超时结束了,强制执行阶段剩余的所有帧
                double exitStageTime = stageTotalRunningTime - curStageInfo.Time;
                //DB_Close   LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [v-v] OnUpdate ready Exit ,Time: {curStageInfo.Time}, stageTotalRunningTime {stageTotalRunningTime} ");
                OnExit(exitStageTime, E_SkillStageExitType.Default);    //[超时约束：只是结束当前阶段update][回归到技能控制]：*阶段结束阶段不会释放，可能有效果。*阶段之间必须相连。
            }
        }

        /// <summary>
        /// try 设置 active 活跃技能
        /// </summary>
        /// <param name="isActiveMain"></param>
        /// <param name="force"> 是否强制 设置 活跃技能, 只有服务器 同步才需要强制 设置</param>
        /// <returns></returns>
        public bool TrySetActiveMain(double enterStageTime, bool isActiveMain, bool force)
        {
            // 非 normal 阶段 不参与 活跃抢占
            if (!curStageInfo.IsNormalStage)
            {
                return true;
            }
            bool result = FuncOnStageTrySetActiveMain.Invoke(this, enterStageTime, isActiveMain, force); ;
            return result;
        }

        /// <summary>
        /// 执行 阶段抢占 技能的逻辑
        /// 抢占失败分两种情况:
        ///     1. 普通阶段的抢占失败, 需要直接结束技能运行时;
        ///     2. 被技能抢占后而进入的技能阶段,此时 跟策划和服务器的 约定是 此时技能跳转的阶段不 抢占主阶段,
        ///        如过 配置了主阶段，则继续跳转技能阶段 直到 下一个阶段为 非主动阶段 或者 技能运行时结束；
        /// </summary>
        /// <param name="enterType"></param>
        /// <returns></returns>
        public bool ExcuteActiveMain(double enterStageTime, E_SkillStageEnterType enterType, bool force)
        {
            bool needActiveMain = curStageInfo.NeedActive;
            // SGF.Debuger.Log($"{TagFlag}  ExcuteActiveMain enterType  {enterType} , needActiveMain {needActiveMain} ");
            //阶段不抢占,那直接执行成功
            if (!needActiveMain)
            {
                TrySetActiveMain(enterStageTime, false, false);
                return true;
            }
            switch (enterType)
            {
                case E_SkillStageEnterType.SkillBreak:
                    {
                        //如果 enterType 是技能打断,那么需要判断这个技能是否需要设置主动,如果需要设置,
                        //直接 执行 main 失败
                        return false;
                    }
                case E_SkillStageEnterType.Recover:
                    {
                        //如果 enterType 是技能恢复状态进入的阶段,那不需要执行抢位
                        //直接 返回true
                        return true;
                    }
                case E_SkillStageEnterType.RecoverCurStage:
                    {
                        // 恢复技能的当前阶段, 需要 根据当前阶段 是否抢占活跃来设置 活跃
                        // note:
                        //     如果服务器 在 AOI 同步第三人 技能的时候, 同步了多个 当前阶段是活跃的技能的时候,
                        //     说明 服务器 有了 bug.  目前 技能应该只允许存在 一个 活跃技能
                        //     服务器 能够保证 当前只有一个 活跃技能, 而客户端 只需要收到技能后 同步即可
                        return TrySetActiveMain(enterStageTime, true, force);
                    }
                default:
                    {
                        //其它的进入类型,那就按正常的设置流程
                        return TrySetActiveMain(enterStageTime, true, force);
                    }
            }
        }


        /// <summary>
        /// 帧事件执行
        /// </summary>
        /// <param name="isRecover"></param>
        public void PlayFrameEvent(bool isRecover, double stageRecoverTime = 0)
        {
            SkillStageFrame frameEvent = null;
            if (frameEventsQueue.Count > 0)
            {
                frameEvent = frameEventsQueue.Dequeue();
            }
            if (frameEvent == null)
            {
                return;
            }

            // 先检查一遍 这个 事件帧 是否在 阶段的恢复时间内, 如果在恢复的时间内,
            // 那其实就不需要去 播放这个事件帧了
            // 而对于 效果帧而言, 目前 CheckIsPlayFrameEvent 的设定是 都可以直接播, 
            // 在具体的效果 里面 去执行 它的 特效/动作/或者其它 的恢复
            bool isPlayFrameEvent = frameEvent.CheckIsPlayFrameEvent(isRecover, stageRecoverTime);

            // 打日志
            //{
            //    if (frameEvent.FrameType == E_StageFrameEventType.Animation)
            //    {
            //        AnimationJson animationJson = (AnimationJson)frameEvent.Value;
            //        LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} play Anim: name={animationJson.ClipName},TimeLineTime {frameEvent.TimeLineTime} ,FrameTime {frameEvent.FrameTime}, isFrameStart: {frameEvent.isFrameStart}, isRecoverFrame {frameEvent.IsRecoverCopyStartFrame}");
            //    }
            //    else if (frameEvent.FrameType == E_StageFrameEventType.SpecialEffects)
            //    {
            //        FXJson fXJson = (FXJson)frameEvent.Value;
            //        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} play fx: name={fXJson.EffectName},TimeLineTime {frameEvent.TimeLineTime} ,FrameTime {frameEvent.FrameTime}, isFrameStart: {frameEvent.isFrameStart}, isRecoverFrame {frameEvent.IsRecoverCopyStartFrame}");
            //    }
            //    else if (frameEvent.FrameType == E_StageFrameEventType.Effect)
            //    {
            //        EffectData effectData = (EffectData)frameEvent.Value;
            //        //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} play effect: EffectID={effectData.EffectID},TimeLineTime {frameEvent.TimeLineTime} ,FrameTime {frameEvent.FrameTime}, isFrameStart: {frameEvent.isFrameStart}, isRecoverFrame {frameEvent.IsRecoverCopyStartFrame}");
            //    }
            //}


            if (isPlayFrameEvent)
            {
                frameEvent.Play(isRecover, stageRecoverTime);
                executedFrameEventsQueue.Enqueue(frameEvent);
            }
        }

        /// <summary>
        /// 播放 nextFrameEvent id为effectID 的 接口.
        /// note:
        ///     由于next 效果并没有 直接生成 事件帧排布在 阶段上, 所以之前的next 效果都是 由技能skillEntity直接播放.
        ///     但是 这样对于 效果的 终止就很难处理(效果的终止,本质上是调用 已经执行过的事件帧 的 stop).
        ///     所以 对于next效果, skillStage中 先提供对应的直接播放 effectData的接口,在播放之前,先生成
        ///     对应的事件帧,这样 就可以 通过阶段 统一管理 所有的next 效果事件帧了
        /// </summary>
        /// <param name="effectID"></param>
        /// <param name="isRecover"></param>
        public void PlayNextEffect(int effectID, bool isRecover, int stageRecoverTime)
        {
            if (nextEffectFrameEventMap.TryGetValue(effectID, out SkillStageFrame frame))
            {
                if (frame.CheckIsPlayFrameEvent(isRecover, stageRecoverTime))
                {
                    frame.Play(isRecover, stageRecoverTime);
                    executedFrameEventsQueue.Enqueue(frame);
                }
            }
        }

        /// <summary>
        /// 检查 效果 是否是 next 效果
        /// </summary>
        /// <param name="effectID"></param>
        public bool CheckIsNextEffectFrame(int effectID)
        {
            return nextEffectFrameEventMap.ContainsKey(effectID);
        }

        /// <summary>
        /// 获取 效果 对应的 outputKey, 后续  需要给 EffectData 拓展 一个 GetOutputKey, 要不然很麻烦
        /// </summary>
        /// <param name="effectID"></param>
        /// <param name="outputKey"></param>
        /// <param name="skillEffectType"></param>
        public void GetEffectDataOutputKey(EffectData effectData, out string outputKey, out E_SkillEffect skillEffectType)
        {
            outputKey = "";
            skillEffectType = E_SkillEffect.None;
            if (effectData == null)
            {
                return;
            }
            outputKey = EffectParam.GetEffectDataOutPutKey(effectData);
            skillEffectType = EffectParam.GetEffectDataESkillEffect(effectData);
        }

        /// <summary>
        /// 获取 next效果 的 父效果的 id
        /// </summary>
        /// <param name="nextID"></param>
        /// <returns></returns>
        public int GetNextIDParent(int nextID)
        {
            if (Next2ParentEffectMap.ContainsKey(nextID))
            {
                return Next2ParentEffectMap[nextID];
            }
            return 0;
        }

        // public 

        /// <summary>
        /// 执行帧事件
        /// </summary>
        /// <param name="isRecover">是否是阶段恢复执行的事件帧</param>
        /// <param name="forceAll">是否强制执行所有的剩余帧,用来特殊处理阶段超时结束了,
        ///      但是阶段最后的结束帧可能超过阶段时间的情况(一般针对最后一个阶段,因为最后一个阶段会将所有帧放入其中)</param>
        /// <param name="stageRecoverTime">执行事件帧 时, 这个阶段的 恢复 时间 . 
        ///      通过判断 (阶段的恢复时间 - 事件帧的开始时间 - 事件帧的 持续时间) > 0  来判断 这个事件帧 是否还在运行.
        ///      对于动画/ 特效等 这种持续的 效果, 可以 通过上面的公式 判定是否 在恢复的时候 需要展示出来。
        /// </param>
        public void ExecuteFrameEvents(bool isRecover, bool forceAll = false, double stageRecoverTime = 0)
        {
            //帧事件处理逻辑
            {
                bool tryFind = true;
                do
                {
                    if (frameEventsQueue.Count > 0 && (stageTotalRunningTime >= frameEventsQueue.Peek().FrameTime || forceAll))
                    {
                        PlayFrameEvent(isRecover, stageRecoverTime);
                        tryFind = true;
                    }
                    else
                    {
                        tryFind = false;
                    }
                } while (tryFind);
            }
        }

        /// <summary>
        /// 执行阶段退出时的 帧处理逻辑
        /// </summary>
        public void ExecuteExitStageFrameEvent(E_SkillStageExitType type)
        {
            while (executedFrameEventsQueue.Count > 0)
            {
                SkillStageFrame frameEvent = executedFrameEventsQueue.Dequeue();
                frameEvent.OnStageExit(type);
            }
        }

        /// <summary>
        /// 技能运行时结束后，阶段退出时的 帧处理逻辑
        /// </summary>
        public void ExecuteExitStageFrameEvent(E_SkillExitType type)
        {
            while (executedFrameEventsQueue.Count > 0)
            {
                SkillStageFrame frameEvent = executedFrameEventsQueue.Dequeue();
                frameEvent.OnExit(type);
            }

        }

        public bool EqualStageID(int stageID, int loopIdx = 0)
        {
            bool equal = stageID == _stageID && loopIdx == _loopIdx;
            return equal;
        }

        /// <summary>
        /// 处理不同的技能阶段,需要设置的原子状态
        /// </summary>
        private void ExecuteStageStates(bool regist)
        {
            List<int> states = curStageInfo.States;
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
                ActionOnRefreshStageStates?.Invoke(StageIDStr, states, regist);
            }
        }

        private void ExecuteStageCD()
        {
            if (IsEntryCD)
            {
                ActionOnStageStartCD?.Invoke(this);
            }
        }

        public void OnActionPlayAnimation(AnimationJson animationJson, bool isFrameStart, float startTime)
        {
            bool result = FuncOnStageTryPlayAnim.Invoke(this, animationJson, isFrameStart, startTime);
        }

        /// <summary>
        /// 阶段播放特效的接口
        /// </summary>
        /// <param name="fXJson"></param>
        /// <param name="fxStartTime">特效 开始的时间点,单位 ms</param>
        public void OnActionPlayFx(FXJson fXJson, int fxStartTime)
        {
            bool isPlayOnStart = fxStartTime == 0;
            if (fXJson.IsCameraImpulse)
            {
                if (!isPlayOnStart)
                {
                    GlobalEvent.OnCameraImpluseEvent?.Invoke(BuilderID, fXJson.EffectName, false);
                    return;
                }
                GlobalEvent.OnCameraImpluseEvent?.Invoke(BuilderID, fXJson.EffectName, true);
            }
            else if (fXJson.IsCamaeraOffset)
            {
                if (!isPlayOnStart)
                {
                    return;
                }
                GlobalEvent.OnCameraOffsetEvent?.Invoke(BuilderID, fXJson.EffectName);
            }
            else
            {

                ActionOnStageTryPlayFx.Invoke(this, fXJson, fxStartTime);
            }
        }

        public void OnActionPlaySound(SoundJson soundJson, int startTime)
        {
            ActionOnStageTryPlaySound.Invoke(this, soundJson, startTime);
        }

        public void OnActionStopSound(SoundJson soundJson)
        {
            ActionOnStageTryStopSound.Invoke(this, soundJson);
        }

        public void OnActionPlayEffect(EffectData effectData, bool isRecover, bool isFrameStart, int stageRecoverTime)
        {
            // 目前Server设计,技能结束,效果结束
            // 客户端这边 效果 由阶段 处理。
            // 处理过程与之前类型,分为 两条方式,客户端表现的处理和 服务器数据的处理
            // 需要处理的一个问题，数据的存储问题,需要存在技能黑板还是 阶段黑板内.
            // 同时，需要将阶段 与 服务器的协议挂钩，让阶段能够自己处理 服务器发送的黑板 和 客户端自己产生的黑板数据
            // 效果的执行，依赖于黑板数据，客户端的黑板 需要 区分 客户端黑板数据  和服务器数据

            // 如果是轴上的时间,直接根据轴上的时间,播放效果

            /// note:
            ///     1.阶段skillStage是通用逻辑,技能/buff/后面被动之类的,可能都采用 阶段的方式实现;
            ///     2.不同类型的阶段,执行动画、效果 等,逻辑可能不相同,比如动画,技能的动画优先级最高,而buff的优先级仅大于idle;
            ///     3.所以,目前阶段 事件帧并没有处理细节逻辑,细节的实现,都基于上层 如 skillEntity/skillBuff  中处理.
            /// 
            ActionOnStageTryPlayEffect.Invoke(this, effectData, isRecover, isFrameStart, stageRecoverTime);
        }

        public void OnActionStopEffect(EffectData effectData)
        {

            ActionOnStageTryStopEffect.Invoke(this, effectData);
        }
        public void OnActionPlayCamera(CameraJson cameraJson, bool isFramStart)
        {
            ActionOnStageTryPlayCamera?.Invoke(this, cameraJson, isFramStart);
        }

        public void OnActionPlayCameraShake(CameraShakeJson cameraShake, bool isFramStart)
        {
            ActionOnStageTryPlayCameraShake.Invoke(this, cameraShake, isFramStart);
        }

        public void OnActionStopFx(FXJson fXJson)
        {
            if (fXJson.IsCameraImpulse)
            {
                GlobalEvent.OnCameraImpluseEvent?.Invoke(BuilderID, fXJson.EffectName, false);
            }
            else
            {
                ActionOnStageTryStopFx?.Invoke(this, fXJson);
            }
        }

        /// <summary>
        /// 阶段写入黑板数据的逻辑, isWriteClient = true 存入客户端黑板
        /// </summary>
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <param name="isWriteSkillBlackBoard">是否写入技能黑板</param>
        /// <param name="isWriteClient">数据写入客户端黑板还是服务器黑板</param>
        private void WriteBlackBoard(string key, CustomBlackBoardNode v, bool isWriteSkillBlackBoard, E_BlackBoardTag tag)
        {

            if (isWriteSkillBlackBoard)
            {
                ActionOnStageWriteSkillEffect?.Invoke(this, key, v, tag);
                return;
            }
            if (_stageBlackBoard == null)
            {
                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} WriteBlackBoard  stage {StageIDStr} , _stageBlackBoard null error!!!  ");

                return;
            }
            // LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [x-x] WriteBlackBoard[{StageIDStr}] hash[{_stageBlackBoard.GetHashCode()}] key {key} , tag {tag} ");

            _stageBlackBoard.Set(key, v, tag);
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

        public CustomBlackBoardNode WriteBlackBoard(BlackBoardNode blackBoardNode, E_BlackBoardTag tag)
        {
            string key = blackBoardNode.Key;
            CustomBlackBoardNode customBlackBoardNode = BaseBlackBoard.InitServerCustomBlackBoardNode(key, blackBoardNode, BuilderID, OwnerEntityID);
            if (customBlackBoardNode != null)
            {
                //// SGF.Debuger.Log($"黑板阶段同步 key={key}");
                // bool isWriteBlackBoard = false;
                // 写入阶段黑板数据(此处是服务器阶段黑板数据同步过来,所以直接存入阶段黑板)
                // 同时,服务器同步的数据,写入服务器黑板数据块
                WriteBlackBoard(key, customBlackBoardNode, false, tag);
            }

            return customBlackBoardNode;
        }

        /// <summary>
        /// 阶段处理每个黑板数据的接口
        /// </summary>
        /// <param name="blackBoardNode"></param>
        /// <param name="isRecover"></param>
        private void HandleItemBlackBoard(CustomBlackBoardNode customBlackBoardNode, bool isRecover)
        {

            if (isRecover)
            {
                // 如果是 恢复数据,那就不需要执行 这个服务器效果数据,直接存入黑板
                // isWriteBlackBoard = true;
                // do nothing
            }
            else
            {
                // 如果是正常阶段的效果数据同步,那就尝试执行效果
                // 如果服务器数据先到,客户端还没执行到此处,那执行失败,

                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}========  处理效果 key:{customBlackBoardNode.Key} ================= start ");

                if (customBlackBoardNode.Key.Contains("Move"))
                {
                    // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 移动调试 准备移动 key:{customBlackBoardNode.Key}");
                }
                bool result = FuncOnTryPlayRegistedServerEffect.Invoke(customBlackBoardNode, (BaseBlackBoard)StageBlackBoard);

                // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}========  客户端处理服务器黑板 key:{customBlackBoardNode.Key}, result : {result} =================end ");
                // 客户端将数据写入服务器黑板中(服务器的数据应该都要写入黑板数据中)
                // isWriteBlackBoard = true;
            }
        }

        /// <summary>
        /// 收到服务器阶段同步后的 处理逻辑，主要是子弹/被动/buff 这些纯由服务器控制的 阶段
        /// </summary>
        /// <param name="runStageRet"></param> <summary>
        /// 
        /// </summary>
        /// <param name="runStageRet"></param>
        public void OnServerRunStageRet(RunStageRet runStageRet)
        {

            // 服务器同步阶段数据过来,不管有没有创建这个阶段, 都可以把数据先存到黑板里面去
            {
                tempCustomBlackBoardNodeList.Clear();
                WriteBlackList(runStageRet.BlackList, E_BlackBoardTag.Server, ref tempCustomBlackBoardNodeList);
            }

            bool isRecover = false;

            // 如果阶段收到通知时还未创建, 那就要先走阶段的创建逻辑
            if (!stageInit)
            {
                // 同步阶段的时间 : 阶段当前时间 = 客户端本地服务器时间 - 服务器阶段创建时间CurStageTime 
                long curStageRealyTime = TimeUtils.ServerNowStampMilli - runStageRet.CreateTime;
                // SGF.Debuger.LogError($"[server] {TagFlag}  OnRunStageRet  curStageRealyTime: {curStageRealyTime}");

                //curStageRealyTime = Math.Abs(curStageRealyTime);
                curStageRealyTime = curStageRealyTime < 0 ? 0 : curStageRealyTime;
                // 处理阶段误差, 目前发现 误差时间 可能大于 100ms, 所以 目前 如果 延迟>100 , 先 -100
                double recoverTime = curStageRealyTime <= GameConfig.SKILL_LAG_TIME ? 0 : Math.Abs(curStageRealyTime - GameConfig.SKILL_LAG_TIME);

                isRecover = recoverTime > GameConfig.SKILL_LAG_TIME;
                // isRecover = false;

                //  SGF.Debuger.LogError($"[server] {TagFlag}  OnRunStageRet new Create stage [{stageIdStr}]  CurClientServerTime={TimeUtils.ServerNowStampMilli},ServerCreateTime={runStageRet.CreateTime}, curStageRealyTime : {curStageRealyTime} , recoverTime : [{recoverTime}] , isRecover=[{isRecover}]");
                E_SkillStageEnterType enterType = isRecover ? E_SkillStageEnterType.Recover : E_SkillStageEnterType.Default;
                OnEnter(recoverTime, enterType, false);
            }

            OnRunStageRet(runStageRet, isRecover);
        }



        private List<CustomBlackBoardNode> tempCustomBlackBoardNodeList = new List<CustomBlackBoardNode>();
        /// <summary>
        /// 黑板效果 运行时同步
        /// </summary>
        /// <param name="runStageRet"></param>
        /// <param name="isRecover">是否是恢复数据的调用</param>
        public void OnRunStageRet(RunStageRet runStageRet, bool isRecover)
        {
            if (!ServerCreateStage)
            {
                ServerCreateStage = true;
                ActionOnServerStageCreate?.Invoke(this);
            }

            StageUID = runStageRet.UID;

            UpdateCurStageCreteTime(runStageRet);

            // 阶段黑板 如何关联,目前想到的是通过 StageID + StageLoop

            //如果收到了服务器效果数据,就需要:
            //  1.检查阶段是否注册了 这个效果的服务器数据等待逻辑
            //  2.如果有注册,直接执行注册逻辑
            //  3.如果没有注册,将数据存入阶段中
            RepeatedField<BlackBoardNode> blackList = runStageRet.BlackList;
            tempCustomBlackBoardNodeList.Clear();
            if (blackList.Count > 0)
            {
                foreach (BlackBoardNode item in blackList)
                {
                    //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [Server] OnRunStageRet StageEnable : {StageEnable} , key {item.Key} , stageTotalRunningTime {stageTotalRunningTime} , isRecover {isRecover}");
                    switch (item.Key)
                    {
                        case "InputCoord":
                        case "InputRota":
                        case "InputTarget":
                        //蓄力的层数
                        case "BBEnergy":
                        //蓄力的时间
                        case "BBEnergyTime":
                        case "Builder":
                        case "Owner":
                        case "Victim":
                        case "SkillTarget":
                        case "LiveTime":
                        case "StartTime":
                        //buff护盾值
                        case "ShieldVal":
                        //buff层数
                        case "StackCount":
                            {
                                ActionOnSpecialServerBlackBoardNode?.Invoke(this, item);
                            }
                            break;
                        default:
                            {
                                CustomBlackBoardNode customBlackBoardNode = WriteBlackBoard(item, E_BlackBoardTag.Server);
                                if (customBlackBoardNode != null)
                                {
                                    tempCustomBlackBoardNodeList.Add(customBlackBoardNode);
                                }
                            }
                            break;
                    }
                }
            }
            // tempCustomBlackBoardNodeList.ForEach((CustomBlackBoardNode customBlackBoardNode) =>
            // {
            // // gg    LogUtils.LogError(LogUtils.LogEnum.Skill,$"black key : {customBlackBoardNode.Key}");
            // });
            // LogUtils.LogError(LogUtils.LogEnum.Skill,$"black key --------------");

            EffectUtils.SortServerCustomBlackList(tempCustomBlackBoardNodeList, StageBlackBoard);
            // Debug.Break();

            tempCustomBlackBoardNodeList.ForEach((CustomBlackBoardNode customBlackBoardNode) =>
            {
#if (UNITY_EDITOR && BATTLE_DEBUG)
#endif
                bool isMain = OwnerEntityID == GameManager.Instance.mainPlayerId;
                //LogUtils.Log(LogUtils.LogEnum.Skill, $"{TagFlag} black key : {customBlackBoardNode.Key}", isMain);
                HandleItemBlackBoard(customBlackBoardNode, isRecover);
            });

            tempCustomBlackBoardNodeList.Clear();

            // TODO: DL
            // 销毁的恢复逻辑
            // ServerRefreshRegEffects(runStageRet.StageLines, (int)stageTotalRunningTime);
        }

        public void UpdateCurStageCreteTime(RunStageRet runStageRet)
        {
            if (runStageRet.CreateTime == 0)
            {
                return;
            }
            serverCreteTime = (runStageRet.CreateTime + runStageRet.StageLoop * Time);
        }


        /// <summary>
        /// 由服务器 消息通知,来更新 需要注册的 服务器效果线
        /// note：
        ///     1.正常效果 在客户端执行的时候,由 客户端检查是否有服务器黑板数据,来执行或者注册;
        ///     2.当服务器 效果数据同步过来的时候, 根据服务器 发送的效果线的数据,可以计算出 
        ///         当前 客户端还需要等待服务器 哪些效果,从而 更新 注册数据.
        ///     3.阶段 恢复 的时候,根据阶段的 效果线数据,决定 哪些服务器效果线需要执行
        /// </summary>
        /// <param name="stageLines"></param>
        /// <param name="stageTime"></param>
        private void ServerRefreshRegEffects(RepeatedField<RunLineData> stageLines, int stageTime)
        {
            List<EffectData> effectDatas = curStageInfo.GetCurNeedRegEffects(stageLines, stageTime);


            Dictionary<string, object> stageRegBlackBord = _stageBlackBoard.GetBlackBoard(E_BlackBoardTag.Reg);

            // 每次服务器 同步 当前效果记录过来的时候,客户端 可以先干掉所有的注册逻辑,只需要关心 服务器告诉客户端 需要注册的那些效果
            {

                stageRegBlackBord.Clear();
            }

            // TODO: DL
            // 1.对于技能黑板的恢复,目前没处理
            // 2.对于输入轴效果这种存在延时的效果,目前基于curEffectID（当前效果id）的方式,没办法恢复,没想好如何处理

            //更新每一个 注册的 效果节点
            effectDatas.ForEach((EffectData effectData) =>
            {
                ActionOnStageRegisterServerEffect.Invoke(this, effectData);
            });
        }

        private void HandleStageLineDataCache(RepeatedField<RunLineData> stageLines)
        {
            ServerRefreshRegEffects(stageLines, (int)stageTotalRunningTime);
        }

        /// <summary>
        /// 恢复阶段的效果线数据
        /// </summary>
        private void RecoverStageLines()
        {
            StageBlackBoard.ReadStageLinesCache(HandleStageLineDataCache);
        }

        /// <summary>
        /// 是否是在阶段的 尾部, 目前考虑到延迟的问题, 如果这个阶段 存在 用户输入轴效果, 
        /// 当前阶段 是活跃 并且 在阶段的尾部()
        /// </summary>
        public bool IsAtEndOfStage()
        {
            double leastTime = curStageInfo.Time - stageTotalRunningTime;

            if (stageTotalRunningTime <= curStageInfo.Time && SkillEntity.CheckInForbidenUseSkillTime(leastTime))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置跳过 循环阶段的标识
        /// </summary>
        public void SetSkipLoopStage()
        {
            _isSkipLoopStage = true;
        }

        /// <summary>
        /// 设置 活跃 被打断的标签
        /// </summary>
        public void SetBreakActiveTag()
        {
            keepActive = false;
        }

        protected override void Release()
        {
            base.Release();
            Reset();
            ReleaseFrameEvents();
            ReleaseAction();
        }

        /// <summary>
        /// 回溯到 init 初始状态,
        /// </summary>
        public void Backtrack2Init()
        {
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 阶段执行 回溯流程");
            // 先对所有 已经执行的 事件帧 执行 打断逻辑
            ExecuteExit(E_SkillExitType.Reset);

            // 回溯 已经执行 过的 事件帧到 frameEvent中去
            BacktrackexEcutedFrameEvents();

            // 还原 阶段中的 默认 值
            ResetStageValue();
        }

        /// <summary>
        /// 回溯 已经执行 过的事件帧 到 frameEventsQueue 中
        /// </summary>
        private void BacktrackexEcutedFrameEvents()
        {
            // 回溯 并且 清除 executedFrameEventsQueue
            frameEventsQueue.HeadEnqueue(executedFrameEventsQueue);
            executedFrameEventsQueue.Clear();
        }

        /// <summary>
        /// 还原 阶段里面 定义 的一些 属性 默认值
        /// </summary>
        private void ResetStageValue()
        {
            enterStageFixTime = 0;
            stageRunningTime = 0;

            // 清除 阶段黑板的数据 
            _stageBlackBoard.Clear();

            // 清除 外面设置的 跳过阶段循环 标志
            _isSkipLoopStage = false;

            stageEnable = false;
            ReadyRelease = false;

            stageInit = false;
            stageClose = false;

            keepActive = true;

            waitNextFrameUpdate = false;

        }

        /// <summary>
        /// 阶段的 Rest 接口, 调用Reset 应该将阶段 还原到 init 之前的状态
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
            // 不管如何,Release的时候，都先执行阶段阶段的退出
            ExecuteExit(E_SkillExitType.Reset);
            //SGF.Debuger.Log($"[SkillStage] 准备 释放 阶段: {StageIDStr}");

            ResetStageValue();

            ServerCreateStage = false;

            StageUID = 0;

            curStageInfo = null;
            _stageID = 0;
            _stageIDStr = "";
            _tagStr = "";

            _builderID = 0;
            _ownerEntityID = 0;
            MaxStageTime = 0;

            Speed = 1;

            _loopIdx = 0;
            _stageBlackBoard.Clear();

        }

        private void ReleaseFrameEvents()
        {
            // 清理事件帧
            {
                foreach (KeyValuePair<int, SkillStageFrame> item in nextEffectFrameEventMap)
                {
                    SkillStageFrame frame = item.Value;
                    UnregFrameAction(frame);
                    frame.Release();
                }
                nextEffectFrameEventMap.Clear();

                Next2ParentEffectMap.Clear();


                frameEventsList.ForEach((SkillStageFrame frame) =>
                {
                    UnregFrameAction(frame);
                    frame.Release();
                });
                frameEventsList.Clear();
                frameEventsQueue.Clear();
                executedFrameEventsQueue.Clear();
            }
        }



        public void ReleaseAction()
        {
            ActionOnExitStage = null;
            ActionOnStageStartCD = null;
            ActionOnRefreshStageStates = null;
            FuncOnStageTryPlayAnim = null;
            ActionOnStageTryPlayFx = null;
            ActionOnStageTryPlaySound = null;
            ActionOnStageTryStopSound = null;
            ActionOnStageTryStopFx = null;
            ActionOnStageTryPlayEffect = null;
            ActionOnStageTryStopEffect = null;
            ActionOnStageTryPlayCamera = null;
            ActionOnStageRegisterServerEffect = null;
            FuncOnTryPlayRegistedServerEffect = null;
            FuncOnStageTrySetActiveMain = null;
            ActionOnStageTryPlayCameraShake = null;
            ActionOnSpecialServerBlackBoardNode = null;
            ActionOnServerStageCreate = null;
        }

    }


}
