using ProtoMsg;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using StarProject.Service.Battle;
using SGF.Unity;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 2022/10/24
    ///     1.与夏哥的沟通,客户端的buff由于策划配置了动画,需要本地预演动画;
    ///     2.与gl的交流,客户端buff的实现需要基于 stage阶段 的设计;
    /// 
    /// 2022/10/25 (下午)
    ///     1.与夏哥的沟通如下:
    ///         a. 对于buff/子弹/被动, 客户端 应该只需要完全听从服务器的通知，不需要的本地预演;
    ///         b. buff的动画和特效, [限制] 策划只能配置在 buff的开始阶段，类似眩晕这种buff,
    ///            在buff开始的时候客户端播放动画,一直到buff结束;
    ///         c. 基于b,通过限制策划的配置, 客户端即不需要 本地实现buff阶段的过程,
    ///            实现类似于之前buff 冰冻,在buff开始即播放冰冻.
    ///     
    ///     2.沟通之后,通过衡量,buff的设计略作修改:
    ///         a. buff 依旧采用阶段的设计, [不限制] 策划的配置;
    ///         b. buff 阶段的创建 采用 服务器的通知, 而不是本地计时创建,减少工作量;
    ///         c. 效果 的实现依旧基于阶段, 跟技能的 区别 只是 buff只有服务器通知部分,
    ///            不需要本地预演.
    ///       
    ///     以上的设计,综合了 夏哥的建议以及本地当前的工作进度(沟通时,buff已经基于阶段设计)情况.
    ///     总体上 比夏哥推荐的方式重新写一份要更加友好
    /// 
    /// 2022/10/25 (晚上)
    ///     1.与博哥的沟通:
    ///         客户端还是不要限制策划的配置,按技能那种方式,客户端全量来做,这样的话,最准确
    /// 
    /// 2022/10/26(10:30)
    ///    通过跟博哥和夏哥沟通,确认采用简单处理的方式. 本地不做预演,由服务器通知buff阶段开启
    /// </summary>
    public class SkillBuff : ServerControlStageEntityBase
    {

        public int BuffID;
        public int RuntimeLv;

        private E_EntityState readyReleaseState = E_EntityState.ClientClose | E_EntityState.ServerClose;

        /// <summary>
        /// buff的创建和销毁完全依赖服务器,所以对于buff而言,
        /// 决定buff是否 IsRunning 的关键因素是 ServerClose
        /// </summary>
        public override bool IsRunning => (state != E_EntityState.None) && (state & readyReleaseState) != readyReleaseState;
        public bool isServerClose = false;

        private BuffInfo buffInfo;
        public BuffInfo BuffInfo => buffInfo;

        private VitalSignData playerData;

        private ulong EntityID => playerData == null ? 0 : playerData.M_EntityID;

        // 开始时间
        private long startTime = 0;
        public long StartTime => startTime;

        // 存在时间（剩余时间）
        private long liveTime = 0;
        public long LiveTime => liveTime;


        private int stackCount = 0;
        /// <summary>
        /// buff层数
        /// </summary>
        public int StackCount
        {
            get
            {
                return GetBuffStackCount();
            }
        }

        private int shieldVal = 0;
        /// <summary>
        /// buff护盾值
        /// </summary>
        public int ShieldVal => shieldVal;

        /// <summary>
        /// buff 的最大时间
        /// </summary>
        public int MaxTime => buffInfo.Time;

        public bool IsTimeShow => buffInfo.Cfg.IsTimeShow;

        private BuffDescDataCell buffDescDataCell;
        public BuffDescDataCell BuffDescDataCell
        {
            get
            {
                if (buffDescDataCell == null)
                {
                    buffDescDataCell = LocalDataManager.Instance.GetBuffDescDataCellBySkillIdAndLevel(BuffID, RuntimeLv);
                }
                return buffDescDataCell;
            }
        }

        public Action<SkillBuff> ActionOnUpdateBuff;

        public bool dirtyUpdateBuff = false;

        public void Create(BuffCreateRet buffCreateRet, VitalSignData vitalSignData)
        {
            Create(buffCreateRet.RuntimeID, buffCreateRet.OwnerEntityID, buffCreateRet.BuilderID);
            state = E_EntityState.Running;

            playerData = vitalSignData;

            BuffID = (int)buffCreateRet.BuffID;
            RuntimeLv = buffCreateRet.RuntimeLv > 0 ? buffCreateRet.RuntimeLv : 1;
            TagFlag = $"[{EntityID}] [Buff_{BuffID}] runtimeID[{RuntimeID}]";

            // SGF.Debuger.LogError($"{TagFlag} [Create] ");
            EntityBlackBoard.Set(BaseBlackBoard.KEY_CFG_ID, BuffID, E_BlackBoardTag.Client);

            CreateStageHandle();

            InitBuff(buffCreateRet);

            buffDescDataCell = null;
            playerData.TriggerBuffChange(BuffID, "buffCreate", stackCount);
        }

        private void CreateStageHandle()
        {
            BuffStageHandle handle = EntityFactory.InstanceEntity<BuffStageHandle>();
            SetStageHandle(handle);
            // buff 的 stageHandle 在 init 的时候 会 执行 reset. 所以 SetVitalSignData 在init 之后设置才生效
            handle.SetVitalSignData(playerData);
        }

        private bool needExecuteFrameInit = false;
        private void InitBuff(BuffCreateRet buffCreateRet)
        {

            buffInfo = EntityFactory.InstanceEntity<BuffInfo>();


            buffInfo.Init(BuffID, () =>
            {
                // 先处理buff 的黑板数据,服务器发过来的 黑板数据应该是都只需要存入黑板, 所以 isRecover = true;
                HandleBlackList(buffCreateRet.BlackList, "buffCreateRet", true);

                InitBuffStartTime();

                needExecuteFrameInit = true;

                PlayBuffDamageText();
            });
            baseConfigInfo = buffInfo;
        }

        /// <summary>
        /// buff 的释放是在下一帧 EnterFrame 执行.
        /// 所以 如果是 替换类型的buff, 服务器下发 是 先移除buff--->创建buff;
        /// 客户端是 移除buff --->标记状态 ---> 创建buff ---> 移除buff.
        /// 此时 会存在 创建的buff 效果被 移除的buff 释放的问题.
        /// 
        /// 好的方案是 每个buff 对应的都是自己的效果. 这样相互不影响。
        /// 但对于表现层的 tween 来说，一般都是公用一个 tween。 此时 就不太好处理.
        /// 所以把 buff 创建的效果 移动到下一帧的 EnterFrame 中执行.
        /// 避免顺序问题
        /// </summary>
        private void OnFrameInit()
        {
            if (buffInfo == null)
            {
                return;
            }
            if (!needExecuteFrameInit)
            {
                return;
            }
            PlayBuffEffects();

            PlayCreateLoopEffects(buffInfo.LoopEffectFxs, true);

            ExecuteStageStates(buffInfo.States, true);

            needExecuteFrameInit = false;
        }

        private void InitBuffStartTime()
        {
            startTime = GetStartTime();
            long now = SGF.Time.TimeUtils.ServerNowStampMilli;
            if (startTime == 0)
            {
                //SGF.Debuger.LogError($"{TagFlag} [server]  Create  startTime {startTime} eroor!!! , set time Now {now}:");
                startTime = now;
            }

            //long costTime = now - startTime;
            //int enterTime = Mathf.Abs((int)costTime);

            //enterTime = enterTime <= 100 ? 0 : enterTime;
            // SGF.Debuger.LogError($"{TagFlag} [server]  Create  startTime {startTime} , costTime {costTime} , enterTime {enterTime}");
        }

        internal override void EnterFrame()
        {
            base.EnterFrame();
            OnFrameInit();

            if (dirtyUpdateBuff)
            {
                dirtyUpdateBuff = false;
                ActionOnUpdateBuff.Invoke(this);
            }
        }

        #region LUA层使用

        // LUA层BUFFUI回去BUFF剩余时间
        public long GetEndime()
        {
            return (startTime + LiveTime) - SGF.Time.TimeUtils.ServerNowStampMilli;
        }

        public string GetBuffIconPath()
        {
            string path = "";

            if (BuffDescDataCell != null)
            {
                path = BuffDescDataCell.IconPath;
            }

            return path;
        }

        public string GetBuffIcon2Path()
        {
            string path = "";

            if (BuffDescDataCell != null)
            {
                path = BuffDescDataCell.IconPath2;
            }

            return path;
        }

        public string GetBuffName()
        {
            string name = "";

            if (BuffDescDataCell != null)
            {
                name = BuffDescDataCell.SkillName;
            }

            return name;
        }

        public string GetBuffDecs()
        {
            string decs = "";

            if (BuffDescDataCell != null)
            {
                decs = BuffDescDataCell.Decs;
            }

            //if (string.IsNullOrEmpty(decs) || string.IsNullOrWhiteSpace(decs))
            //{
            //    if (BuffInfo != null && BuffInfo.Cfg != null)
            //    {
            //        decs = BuffInfo.Cfg.BuffDesc;
            //    }
            //}

            return decs;
        }

        public int GetBuffLevel()
        {
            int level = 1;
            if (BuffDescDataCell != null)
            {
                level = BuffDescDataCell.GetLevel();
            }
            return level;
        }

        #endregion

        // 获取buff层数
        public int GetBuffStackCount()
        {
            int _stackCount = 0;
            if (IsRunning && !isServerClose)
            {
                _stackCount = stackCount;
            }
            return _stackCount;
        }

        /// <summary>
        /// 播放buff 配置的 特效: 比如冰冻特效的冰冻、影子buff的 影子等
        /// </summary>
        private void PlayBuffEffects()
        {
            PlayEffects(buffInfo.Cfg.GlobalShows, BuffID);
        }

        private void StopBuffEffects()
        {
            StopEffects();
        }

        private void PlayBuffDamageText()
        {
            if (playerData != null && playerData.IsDead)
            {
                //现在女枪每次死亡后会播放一次换弹。
                //改为：死了之后关闭BUFF的飘字了
                return;
            }
            if (buffInfo.Cfg.AddEffectFlys.Count > 0)
            {
                for (int i = 0; i < buffInfo.Cfg.AddEffectFlys.Count; i++)
                {
                    var data = buffInfo.Cfg.AddEffectFlys[i];
                    BattleManager.Instance.PlayDamageText(EntityID, BuilderID, OwnerID, data.Value, data.Type);
                }
            }
        }

        public override void HandleItemBlackBoard(CustomBlackBoardNode customBlackBoardNode, bool isRecover, string tag)
        {
            // SGF.Debuger.Log($"{TagFlag} buff HandleBuffBlackBoardNode: Key [{customBlackBoardNode.Key}] , tag: {tag}");

            base.HandleItemBlackBoard(customBlackBoardNode, isRecover, tag);
            HandleBuffBlackBoardNode(customBlackBoardNode);
        }

        /// <summary>
        /// buff 有几个需要单独自己处理的黑板数据
        /// </summary>
        /// <param name="blackBoardNode"></param>
        private void HandleBuffBlackBoardNode(CustomBlackBoardNode customBlackBoardNode)
        {
            bool emitBuffUpdate = true;
            string key = customBlackBoardNode.Key;
            switch (key)
            {
                case "StartTime":
                    {
                        startTime = Convert.ToInt64(customBlackBoardNode.Value);
                        //DB_Close       SGF.Debuger.Log($"{TagFlag} HandleBuffBlackBoardNode: Key [{key}] value : {startTime}");
                    }
                    break;
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
                    break;
                case "LiveTime":
                    {
                        // 存在时间
                        // 以上几种情况不需要发送 更新buff ui 事件
                        liveTime = Convert.ToInt64(customBlackBoardNode.Value);
                        //DB_Close       SGF.Debuger.Log($"{TagFlag} HandleBuffBlackBoardNode: Key [{key}] value : {liveTime}");
                    }
                    break;
                //buff层数
                case "StackCount":
                    {
                        stackCount = Convert.ToInt32(customBlackBoardNode.Value);
                        playerData.TriggerBuffChange(BuffID, "StackCount", stackCount);
                        //DB_Close       SGF.Debuger.Log($"{TagFlag} HandleBuffBlackBoardNode: Key [{key}] value : {stackCount}");
                    }
                    break;
                //buff护盾值
                case "ShieldVal":
                    {
                        //emitBuffUpdate = false;
                        shieldVal = Convert.ToInt32(customBlackBoardNode.Value);
                        playerData.TriggerBuffChange(BuffID, "ShieldVal", stackCount);

                        //DB_Close       SGF.Debuger.Log($"{TagFlag} HandleBuffBlackBoardNode: Key [{key}] value : {shieldVal}");
                    }
                    break;
                default:
                    {
                        emitBuffUpdate = false;
                        //DB_Close       SGF.Debuger.Log($"{TagFlag} HandleBuffBlackBoardNode: Key [{key}] no handle error!!!");
                    }
                    break;
            }
            if (emitBuffUpdate)
            {
                dirtyUpdateBuff = true;
            }
        }

        /// <summary>
        /// buff 运行时结束时，由服务器通知buff结束.
        /// note1:
        ///     对于 服务器而言,buffEnd 阶段是 瞬间执行的阶段, 而客户端 需要执行完buffEnd 才算完全结束.
        ///     所以, 客户端的buff 在收到 buffEndRet 之后, 只是标记 它的状态 为 ServerClose。 
        /// 
        /// note2:
        ///     只有 buff 同时被标记为 ClientClose&&ServerClose 后, buff实体才会关闭.
        /// 
        /// note3:
        ///     buff 的 clientClose 结束 要分为两种情况:
        ///         1.存在    endStage 的时候, 在 阶段执行 ReleaseStage 的时候, 对于 EndBuffStage 类型的阶段,
        ///           会单独设置 为 clientClose;
        ///         2.不存在  endStage 的时候, 在 收到buffEndRet 的时候, 就可以 同时 标记 状态为 clientClose;
        /// </summary>
        /// <param name="buffEndRet"></param>
        public void OnBuffEndRet(BuffEndRet buffEndRet)
        {

            HandleBlackList(buffEndRet.BlackList, "BuffEndRet", true);
            // 先标记 buff 状态为 : 服务器关闭了
            state |= E_EntityState.ServerClose;
            //SGF.Debuger.LogError($"[shadow]  {TagFlag} [server]  BuffEndRet BuffID[{buffEndRet.BuffID}] , EndType : {buffEndRet.EndType}");
            //SGF.Debuger.LogError($"{TagFlag} OnBuffEndRet : state ===> {state}");

            // 如果 没有 endStage, 客户端其实也可以直接设置 buff状态为 客户端关闭
            SKillEndType sKillEndType = buffEndRet.EndType;
            if (sKillEndType == SKillEndType.Break)
            {
                state |= E_EntityState.ClientClose;
            }
            if (!buffInfo.CheckHasTypeStage(StageType.EndBuffStage) || (CurStage != null && CurStage.curStageInfo.StageInfoType != StageType.EndBuffStage))
            {
                DelayInvoker.DelayInvoke(0.5f, (args) =>
                {
                    // 如果没有结束阶段，buff的释放不需要等到 endStage结束,直接标记 客户端结束状态
                    state |= E_EntityState.ClientClose;
                });

                //SGF.Debuger.LogError($"{TagFlag} OnBuffEndRet no endBuffState ,set state ===> {state}");

            }
            else

            // 服务器保证如果 服务器下发 子弹结束阶段给客户端, 那一定是先 发Buff结束阶段, 然后再下发 buffendRet;
            // 对于客户端来说, 客户端 也是收到服务器新的阶段，就会立即 设置CurStage。
            //     基于上, 如果服务器 不下发 子弹结束阶段给客户端, 那 客户端收到 EndBuffRet 就可以立即结束buff;
            if (CurStage == null)
            {
                state |= E_EntityState.ClientClose;
            }


            isServerClose = true;

            playerData.TriggerBuffChange(BuffID, "BuffEnd", null);
        }

        private void ReleaseAction()
        {
            ActionOnUpdateBuff = null;
        }

        protected override void Reset()
        {
            //SGF.Debuger.LogError($"{TagFlag} Reset ");
            StopBuffEffects();
            if (buffInfo != null)
            {
                PlayCreateLoopEffects(buffInfo.LoopEffectFxs, false);

                ExecuteStageStates(buffInfo.States, false);
                EntityFactory.ReleaseEntity(buffInfo);
            }

            isServerClose = false;

            needExecuteFrameInit = false;

            base.Reset();
        }

        protected override void Release()
        {

            base.Release();
            //SGF.Debuger.LogError($"{TagFlag} Release ");

            buffInfo = null;

            BuffID = 0;
            shieldVal = 0;
            RuntimeLv = 0;

            buffDescDataCell = null;

            ReleaseAction();
        }

    }
}
