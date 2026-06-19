using ProtoMsg;
using SGF.Network;
using StarProject.Game.Entity.Factory;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace StarProject.Game.Data
{
    public class BuffEventData
    {
        public int BuffID;
        public string BuffEvent;

        public object Value;

        public BuffEventData(int buffID, string buffEvent, object value)
        {
            BuffID = buffID;
            BuffEvent = buffEvent;
            Value = value;
        }
    }

    [XLua.CSharpCallLua]
    public delegate void OnAttributeSyncDelegate(string key, object value);

    /// <summary>
    /// 当 通用数据 变化时, 注册 通知的 委托
    /// </summary>
    public delegate void OnDatatChangeDelegate(object v);

    //public class MoveRotateCache
    //{
    //    //public int 
    //}
    /// <summary>
    /// 每个人本身
    /// 一帧数据处理结束消息
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    ///[XLua.CSharpCallLua]
    //public delegate void OneFrameAttrDataSyncEnd();

    //自定义属性比较器
    public class SyncBaseInfoComparor : IComparer<SyncBaseInfo>
    {
        public int Compare(SyncBaseInfo x, SyncBaseInfo y)
        {
            return x.Index.CompareTo(y.Index);
        }
    }

    //【即时数据】：对应生命体非生命体基类EntityObject：
    //那为啥数据多一层基层，因为数据有两种，一个是构建他，一个是围绕他过程服务的
    [XLua.LuaCallCSharp]
    ///基于REQ/RESP的数据基类
    public abstract class EntityBaseData : DynamicDataObject
    {
        //还是两个并行的queue；是集合还是散列，集合好理解空间分配会比较耗还是集合吧  用Snap
        //public void queue<MoveRotateCache> MoveRotQ = new qu
        private string TagFlag => $"[{M_EntityID}] [EntityBaseData]";
        public E_EntityType EntityType;
        public bool IsServerAOI = true;

        /// <summary>
        /// 所有ntt都应具有位置属性 ：  public virtual Vector3 Position()
        /// </summary>
        public VitalSignAttrData Attrs;
        public VitalSignAttrData OldAttrs;

        protected GameContext m_context;

        //protected OneFrameAttrDataSyncEnd frameAttrDataSyncEnd;
        /// <summary>
        /// 服务器控制玩家的原子状态
        /// 服务器的原子状态基于整条时间线的设计。就是一个阶段配置了300ms的锁，但是这个阶段有个1000ms的其它效果，
        /// 那服务器锁的解开时间为 1300ms。
        /// </summary>
        private E_BattleStateType serverBattleStateType = E_BattleStateType.BattleStateType;

        /// <summary>
        /// 客户端控制的原子状态
        /// 服务器的锁为当前一个阶段的锁，当前这个阶段300ms结束之后，锁就解开。目前并没有完全跟服务器一致，服务器也说不需要
        /// </summary>
        private E_BattleStateType clientBattleStateType = E_BattleStateType.BattleStateType;

        /// <summary>
        /// 融合 双端的原子状态
        /// </summary>
        private E_BattleStateType mixBattleStateType = E_BattleStateType.BattleStateType;

        /// <summary>
        /// 客户端控制的 每个原子状态 对应锁 的次数，如果次数>0，表示该状态 处于锁住的状态
        /// 每次 单个状态发生变化的时候，都需要去同步 clientBattleStateType 客户端的原子锁综合状态
        /// </summary>
        public DictionaryEx<E_BattleStateType, int> clientBattleStateRegisterCountDic = new();

        /// <summary>
        /// 玩家原子状态发生改变时的Action
        /// 参数为：
        /// curBattleStateType ; changeBattleStateType
        /// 当前的原子状态 和 后来的原子状态
        /// </summary>
        public Action<E_BattleStateType, E_BattleStateType> ActionOnBattleStateChange;

        public Action ActionOnBattleStateDead;
        public UnityEngine.Vector3 Pos = new(-999, 0, 0);
        public ProtoMsg.Vector3 pbPos = new();

        /// <summary>
        /// 将需要更新的属性数据先用list 存起来
        /// </summary>
        private List<SyncBaseInfo> PropList = new();
        /// <summary>
        /// 原子状态缓存的属性cache，缓存只需要缓存一个最新的属性数据就可以了，没必要要一个queue
        /// </summary>
        private DictionaryEx<string, SyncBaseInfo> PropCache = new();

        private SyncBaseInfoComparor m_AttrComparor = new();

        private readonly List<string> m_AttrWhiteList = new()
        {
            AOIAttrDefine.StorageDrugs,
            AOIAttrDefine.PathPoses,
            AOIAttrDefine.CurrPathIndex,
            AOIAttrDefine.ExAmuletInfo,
            AOIAttrDefine.PlayerGuildInfo,
            AOIAttrDefine.RiskInfo,
            AOIAttrDefine.WorldLineRewardData,
            AOIAttrDefine.GVEBonus,
            AOIAttrDefine.CurTreasure,
            AOIAttrDefine.TreasureMonid,
            AOIAttrDefine.WantTaskInfo,
            AOIAttrDefine.GNGData
        };

        #region 暴露出来的属性数值

        /// <summary> 是否禁止移动 </summary>
        public bool Is___ForbidMove { get => CheckStateIsForbid(E_BattleStateType.BattleState_ForbidMove); }

        /// <summary> 是否禁止转向 </summary>
        public bool Is___ForbidDir { get => CheckStateIsForbid(E_BattleStateType.BattleState_ForbidDir); }

        /// <summary> 是否禁止普通攻击 </summary>
        public bool IsForbidAttack { get => CheckStateIsForbid(E_BattleStateType.BattleState_ForbidAttack); }

        /// <summary> 是否禁止技能 </summary>
        public bool IsForbidSkill { get => CheckStateIsForbid(E_BattleStateType.BattleState_ForbidSkill); }

        /// <summary> [服务器]是否是战斗状态 </summary>
        public bool IsBattleStateServer { get => CheckStateIsForbid(E_BattleStateType.BattleState_BattleState); }

        /// <summary> 是否是普攻状态 </summary>
        public bool IsBattleStateNormal { get => CheckStateIsForbid(E_BattleStateType.BattleState_NormalState); }

        /// <summary> 是否是死亡状态 </summary>
        public bool IsDead { get => CheckStateIsForbid(E_BattleStateType.BattleState_Dead); }

        /// <summary> 当前攻速属性 </summary>
        public int CurAttackSpeed { get => Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.TruthAttackSpeed); }

        /// <summary> 当前的蓄力速度</summary>
        public int CurEnergySpeed { get => Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.TruthEnergySpeed); }

        /// <summary> 禁被选择器选中</summary>
        public bool IsForbidSelect { get => CheckStateIsForbid(E_BattleStateType.BattleState_ForbidSelect); }

        /// </summary> 当前是否是黑洞牵引状态<summary>
        public bool IsBattleState_BlackHole { get => CheckStateIsForbid(E_BattleStateType.BattleState_BlackHole); }

        /// <summary> 各种活动、玩法中的标记</summary>
        public uint ActMark { get => Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.ActMark); }

        #endregion

        #region 属性变更监听
        // C#层属性变更委托
        private Dictionary<string, List<OnAttributeSyncDelegate>> OnAttributeHandlers = new();
        // Lua层属性变更委托
        private Dictionary<string, Dictionary<string, OnAttributeSyncDelegate>> AttributeChangeLuaActionDic = new();

        #region C#层委托

        public void RegisterAttribute(string attributeName, OnAttributeSyncDelegate callBack)
        {
            if (OnAttributeHandlers.ContainsKey(attributeName))
            {
                OnAttributeHandlers[attributeName].Add(callBack);
                //SGF.Debuger.LogError($"AllReady Register callBack,attributeName= {attributeName}");
            }
            else
            {
                OnAttributeHandlers.Add(attributeName, new List<OnAttributeSyncDelegate>() { callBack });
            }
        }

        public void UnRegisterAttribute(string attributeName, OnAttributeSyncDelegate callBack)
        {
            if (OnAttributeHandlers.ContainsKey(attributeName))
            {
                OnAttributeHandlers[attributeName].Remove(callBack);
                // SGF.Debuger.LogError($"AllReady Register callBack,attributeName= {attributeName}");
            }
        }

        public void OnAttributeChange(string key, object value)
        {
            if (key == "Position")
            {
                // SGF.Debuger.LogWarning($"[{M_EntityID}] OnAttributeChange 准备执行: {key} , value: {value}");
                pbPos = ProtoUtils.DeserializePbPos(value);
                ProtoUtils.CopyPbPos2V3(out Pos, pbPos);
            }

            if (OnAttributeHandlers.TryGetValue(key, out var callBacks))
            {
                callBacks.ForEach((callBack) =>
                {
                    callBack?.Invoke(key, value);
                });
            }
        }

        protected void ClearAttribute()
        {
            if (OnAttributeHandlers != null)
            {
                OnAttributeHandlers.Clear();
            }
        }

        #endregion

        #region LUA层委托

        public void AddAttributeChangeLuaAction(string attributeName, string key, OnAttributeSyncDelegate callBack)
        {
            if (AttributeChangeLuaActionDic.ContainsKey(attributeName))
            {
                if (AttributeChangeLuaActionDic[attributeName].ContainsKey(key))
                {
                    AttributeChangeLuaActionDic[attributeName][key] = callBack;
                }
                else
                {
                    AttributeChangeLuaActionDic[attributeName].Add(key, callBack);
                }
            }
            else
            {
                Dictionary<string, OnAttributeSyncDelegate> keyValuePairs = new();
                keyValuePairs.Add(key, callBack);
                AttributeChangeLuaActionDic.Add(attributeName, keyValuePairs);
            }
        }

        public void DelAttributeChangeLuaAction(string attributeName, string key)
        {
            if (AttributeChangeLuaActionDic.ContainsKey(attributeName))
            {
                if (AttributeChangeLuaActionDic[attributeName].ContainsKey(key))
                {
                    AttributeChangeLuaActionDic[attributeName].Remove(key);
                }
            }
        }

        public void OnAttributeChangeLua(string key, object value)
        {
            if (AttributeChangeLuaActionDic.TryGetValue(key, out var dic))
            {
                try
                {
                    foreach (var item in dic)
                    {
                        item.Value?.Invoke(key, value);
                    }
                }
                catch (Exception e)
                {
                    SGF.Debuger.LogError($"只是日志 属性同步LUA 报错 key={key},value={value},e={e}");
                }
            }
        }

        protected void ClearAttributeLua()
        {
            if (AttributeChangeLuaActionDic != null)
            {
                AttributeChangeLuaActionDic.Clear();
            }
        }


        #endregion

        #endregion

        protected override void Create(ulong entityId)
        {
            base.M_EntityID = entityId;
            m_context = GameManager.Instance.Context;
            Attrs = new VitalSignAttrData();
            Attrs.LOG_TAG += $"_{entityId}";

            OldAttrs = new VitalSignAttrData();
            OldAttrs.LOG_TAG += $"_old_{entityId}";
        }

        public virtual void Create(ulong entityId, E_EntityType entityType, bool isServerAOI)
        {
            base.M_EntityID = entityId;
            m_context = GameManager.Instance.Context;
            Attrs = new VitalSignAttrData();
            Attrs.LOG_TAG += $"_{entityId}";
            OldAttrs = new VitalSignAttrData();
            OldAttrs.LOG_TAG += $"_old_{entityId}";
            EntityType = entityType;
            IsServerAOI = isServerAOI;
        }

        protected override void Release()
        {
            ClearAttribute();
            ClearAttributeLua();

            base.M_EntityID = 0;
            Attrs = null;
            OldAttrs = null;
            m_context = null;
            Pos = UnityEngine.Vector3.zero;
            PropList.Clear();
            PropCache.Clear();

            changeAnimsDatas.Clear();

            ActionStateChange = null;
            ActionRefreshCurChangeAnims = null;
            ActionRefreshCurAnims = null;
            base.Release();
        }

        //網絡，set
        //1，存比取次数多，2未必需要数值因为客户端不计算，3更新时机很特殊是值标记的时候 
        //4, 伤害不用客户端取（属性对客户端逻辑曾未必有用），5未必打开界面显示数值，6打开了未必要求立刻更新  
        //7，所以Get次数相对少，8那反射在取的时候，9现在需要通过映射【获取】【结果数值】
        //TODO枚舉映射所得數值，
        ////remoteIndex动态Index = 本地Index + 服务器Json工具生成运行时加载一次（StarIndex）
        private void SetValueById(VitalSignAOIClientAttrs attrsType, object value)
        {
            OnAttributeChangeLua(attrsType.Name, value);

            OnAttributeChange(attrsType.Name, value);

            // 触发 属性 变化的 通用接口
            TriggerChange(attrsType.Name, value);
        }

        public void UpdateWithAttr(PropBaseSyncList pbsAttrList, bool isContrast = true)
        {
            if (Attrs == null)
            {
                Attrs = new VitalSignAttrData();
            }
            if (OldAttrs == null)
            {
                OldAttrs = new VitalSignAttrData();
            }
            if (pbsAttrList != null && pbsAttrList.Prop.Count > 0)
            {
                UpdatePropList(pbsAttrList, isContrast);
            }
        }

        private void UpdatePropList(PropBaseSyncList pbsAttrList, bool isContrast)
        {
            //解-比-存-通知（比）
            ///---------
            //if (M_EntityID == GameManager.Instance.mainPlayerId)
            //{
            //    SGF.Debuger.LogError($"属性同步创建 UpdatePropList--开始--长度={pbsAttrList.Prop.Count},时间={SGF.Time.TimeUtils.TimeLogString()}");
            //    SGF.Debuger.LogError($"属性同步创建 1.先对比排除相同--开始--长度={pbsAttrList.Prop.Count},时间={SGF.Time.TimeUtils.TimeLogString()}");
            //}

            PropList.Clear();


            // 全量同步：判断差异，无改变的就下发不通知改变
            //---1.先对比排除相同  (长度30个消耗1毫秒)
            if (isContrast)
            {
                for (int i = pbsAttrList.Prop.Count - 1; i >= 0; i--)
                {
                    bool isChanage = GetAttrIsChanage(pbsAttrList.Prop[i]);
                    if (!isChanage)
                    {
                        // 如果没改变--就删除
                        pbsAttrList.Prop.RemoveAt(i);
                    }
                }
            }
            //if (M_EntityID == GameManager.Instance.mainPlayerId)
            //{
            //    SGF.Debuger.LogError($"属性同步创建 1.先对比排除相同--结束--长度={pbsAttrList.Prop.Count},时间={SGF.Time.TimeUtils.TimeLogString()}");
            //    SGF.Debuger.LogError($"属性同步创建 2.排序--开始-- 时间={SGF.Time.TimeUtils.TimeLogString()}");
            //}

            //---2.排序（原子锁优先-》路点组-》路点组下标-》其他）
            bool isHaveState = false;
            //场景 && 人
            for (int i = 0; i < pbsAttrList.Prop.Count; i++)
            {
                SyncBaseInfo syncBaseInfo = pbsAttrList.Prop[i];
                try
                {
                    VitalSignAOIClientAttrs vitalSignAOIClientAttrs = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)syncBaseInfo.Index);

                    if (vitalSignAOIClientAttrs == null)
                    {
                        continue;
                    }

                    //if (vitalSignAOIClientAttrs.Name == AOIAttrDefine.curHp)
                    //{
                    //    SGF.Debuger.LogWarning($"主角血条 当前的 1111 ={syncBaseInfo.Int64Value}");
                    //}
                    //if (vitalSignAOIClientAttrs.Name == AOIAttrDefine.TruthHp)
                    //{
                    //    SGF.Debuger.LogWarning($"主角血条 最大值的 1111 ={syncBaseInfo.Int64Value}");
                    //}

                    if (vitalSignAOIClientAttrs.Name == AOIAttrDefine.State)
                    {
                        PropList.Insert(0, syncBaseInfo);   // 3901
                        isHaveState = true;
                    }
                    else
                    {
                        PropList.Add(syncBaseInfo);
                    }
                }
                catch (System.Exception)
                {
                    SGF.Debuger.LogWarning($" {TagFlag} UpdatePropList() id=>{M_EntityID},属性id={syncBaseInfo.Index},值={syncBaseInfo} error!!! 属性id客户端不认识");
                }
            }
            //服务器是无序的，1.坐标(1050)要先P2.athPoses(3904)后3.CurrPathIndex(3905)，，【主要处理怪物移动的属性顺序】
            if (isHaveState)
            {
                int index = 1;  //算自己一共有几个（步长）
                PropList.Sort(index, PropList.Count - index, m_AttrComparor);
            }
            else
            {
                PropList.Sort((x, y) => x.Index.CompareTo(y.Index));
            }
            //if (M_EntityID == GameManager.Instance.mainPlayerId)
            //{
            //    SGF.Debuger.LogError($"属性同步创建 2.排序--结束-- 时间={SGF.Time.TimeUtils.TimeLogString()}");
            //    SGF.Debuger.LogError($"属性同步创建 3.缓存--开始-- 时间={SGF.Time.TimeUtils.TimeLogString()}");
            //}
            //---3.缓存
            int len = PropList.Count;
            for (int i = 0; i < len; i++)
            {
                HandleProperty(PropList[i]);
            }
            //if (M_EntityID == GameManager.Instance.mainPlayerId)
            //{
            //    SGF.Debuger.LogError($"属性同步创建 3.缓存--结束-- 时间={SGF.Time.TimeUtils.TimeLogString()}");
            //    SGF.Debuger.LogError($"属性同步创建 4.通知--开始-- 时间={SGF.Time.TimeUtils.TimeLogString()}");
            //}
            //---4.通知
            for (int i = 0; i < len; i++)
            {
                InvokeAttrChange(PropList[i]);
            }
            //if (M_EntityID == GameManager.Instance.mainPlayerId)
            //{
            //    SGF.Debuger.LogError($"属性同步创建 4.缓存--结束-- 时间={SGF.Time.TimeUtils.TimeLogString()}");
            //    SGF.Debuger.LogError($"属性同步创建 UpdatePropList--结束--长度={pbsAttrList.Prop.Count},时间={SGF.Time.TimeUtils.TimeLogString()}");
            //}
        }

        private bool GetAttrIsChanage(SyncBaseInfo syncBaseInfo)
        {
            bool isChanage = true;
            VitalSignAOIClientAttrs attrType = GetVitalSignAOIClientAttrsByIndex((ushort)syncBaseInfo.Index);

            if (attrType == null)
            {
                SGF.Debuger.LogWarning($" {TagFlag} GetAttrIsChanage() id=>{M_EntityID},属性id={syncBaseInfo.Index}],值={syncBaseInfo} error!!! 属性id客户端不认识");
                isChanage = false;
                return isChanage;
            }
            ////原子锁
            //if (attrType.Name == AOIAttrDefine.State)
            //{
            //    return isChanage;
            //}

            if (m_AttrWhiteList.Contains(attrType.Name))
            {
                return isChanage;
            }
            //新数据大多数是第一次
            bool isHaveAttr = Attrs.CheckCacheAttrName(attrType);
            if (isHaveAttr)
            {
                switch (syncBaseInfo.PropValueCase)
                {
                    case SyncBaseInfo.PropValueOneofCase.None:
                        break;
                    case SyncBaseInfo.PropValueOneofCase.Int32Value:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, syncBaseInfo.Int32Value);
                        break;
                    case SyncBaseInfo.PropValueOneofCase.Uint32Value:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, syncBaseInfo.Uint32Value);
                        break;
                    case SyncBaseInfo.PropValueOneofCase.Int64Value:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, syncBaseInfo.Int64Value);
                        break;
                    case SyncBaseInfo.PropValueOneofCase.Uint64Value:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, syncBaseInfo.Uint64Value);
                        break;
                    case SyncBaseInfo.PropValueOneofCase.FloatValue:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, syncBaseInfo.FloatValue);
                        break;
                    case SyncBaseInfo.PropValueOneofCase.DoubleValue:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, (float)syncBaseInfo.DoubleValue);
                        break;
                    case SyncBaseInfo.PropValueOneofCase.StringValue:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, syncBaseInfo.StringValue);
                        break;
                    case SyncBaseInfo.PropValueOneofCase.BoolValue:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, syncBaseInfo.BoolValue);
                        break;
                    case SyncBaseInfo.PropValueOneofCase.MsgValue:
                        isChanage = Attrs.GetValIsChanageByAttr(attrType, syncBaseInfo.MsgValue);
                        break;
                    default:
                        break;
                }
                //object newVal = syncBaseInfo.GetType().GetProperty(syncBaseInfo.PropValueCase.ToString()).GetValue(syncBaseInfo, null);
                //isChanage = Attrs.GetValIsChanageByAttr(attrType, newVal);
                //if (!isChanage)
                //{
                //    //SGF.Debuger.LogError($" {TagFlag} 属性同步创建 GetAttrIsChanage id=>{M_EntityID},属性id={syncBaseInfo.Index},属性名={attrType.Name},属性描述=[{attrType.desc}],值={syncBaseInfo} error!!! 值相同，不改变");
                //}
            }
            //if (M_EntityID == GameManager.Instance.mainPlayerId)
            //{
            //    SGF.Debuger.LogError($"属性同步创建 先对比排除相同--对比--属性id={syncBaseInfo.Index},属性名={attrType.Name},属性描述=[{attrType.desc}],值={syncBaseInfo}--时间={SGF.Time.TimeUtils.TimeLogString()}");
            //}
            return isChanage;
        }

        // 获取原子锁锁住的对应属性名
        private string GetBattleState2Attr(E_BattleStateType state)
        {
            string attrName = AOIAttrDefine.Error;
            switch (state)
            {
                case E_BattleStateType.BattleState_ForbidMove:
                    {
                        attrName = AOIAttrDefine.Position;
                    }
                    break;
                case E_BattleStateType.BattleState_ForbidDir:
                    {
                        attrName = AOIAttrDefine.Rot;
                    }
                    break;
                default:
                    {
                        //TODO: dl
                        //需要的时候再添加
                    }
                    break;
            }
            return attrName;
        }

        // 检查属性名是否服务器禁止
        private bool CheckAttrIsServerForbid(string attr)
        {
            bool isForbid = false;
            //如果是坐标属性同步，需要判断是否
            switch (attr)
            {
                case AOIAttrDefine.Position:
                    {
                        // isForbid = CheckStateIsServerForbid(E_BattleStateType.BattleState_ForbidMove);
                    }
                    break;
                case AOIAttrDefine.Rot:
                    {
                        //判断是否禁止转向
                        // isForbid = CheckStateIsServerForbid(E_BattleStateType.BattleState_ForbidDir);
                    }
                    break;
                default:
                    {
                        //TODO: dl
                        //需要的时候再加
                    }
                    break;
            }
            return isForbid;
        }

        // 检查状态是否服务器禁止
        public bool CheckStateIsServerForbid(E_BattleStateType state)
        {
            bool isForbid = false;
            isForbid = CheckState(serverBattleStateType, state, true);
            ////UnityEngine.Debug.Log($" {TagFlag} CheckStateIsServerForbid id => {M_EntityID}  battleStateType ===>{serverBattleStateType} , checkState ==> {state}  isForbid {isForbid}");

            return isForbid;
        }

        // 检查状态是否被禁止
        private bool CheckState(E_BattleStateType state, E_BattleStateType checkState, bool equal)
        {
            if (equal)
            {
                return (state & checkState) == checkState;
            }
            return (state & checkState) != checkState;
        }

        /// <summary>
        /// 更新状态【原子锁】
        /// </summary>
        /// <param name="state"></param>
        private void HandleOnStateChange(E_BattleStateType state)
        {
            E_BattleStateType changeState = serverBattleStateType ^ state; //异或，不同位置是：1
            if (changeState == E_BattleStateType.BattleStateType)
            {
                return;
            }
            // SGF.Debuger.LogError($"  [xx] {TagFlag} 原子状态  HandleOnStateChange id => {M_EntityID} stage: {(int)state}  battleStateType ===>{serverBattleStateType} , new state ==> {state}  , changeState ==> {changeState}");
            ReadAllChangePropCache(changeState, state);

            serverBattleStateType = state;
            UpdateMixBattleStates(true);


            #region 临时代码-》通知主角的状态在HUD界面显示

            if (M_EntityID == GameManager.Instance.mainPlayerId)
            {
                CheckServerStateTest();
            }

            #endregion
        }

        #region 原子锁修改后，读取通知之前锁住的原子锁信息

        private Array stateTypes = Enum.GetValues(typeof(E_BattleStateType));

        private E_BattleStateType tempCheckState;   // 临时缓存要检查的状态
        private void ReadAllChangePropCache(E_BattleStateType changeState, E_BattleStateType newState)
        {
            foreach (var item in stateTypes)
            {
                tempCheckState = (E_BattleStateType)item;

                //首先，检查是否该状态已经发生 改变了 
                bool changeForbidPosSync = CheckState(changeState, tempCheckState, true);
                if (changeForbidPosSync)
                {
                    //然后，检查的状态 不在新的原子状态中
                    bool unForbidPosSync = CheckState(newState, tempCheckState, false);
                    //那么，说明坐标的属性同步 由 禁止 ---> 不禁止
                    //那么，读取缓存
                    if (unForbidPosSync)
                    {
                        //UnityEngine.Debug.Log($" {TagFlag}  ReadAllChangePropCache id => {M_EntityID}  checkState ===>{checkState} , newState ==> {newState}  ,  unForbidPosSync ----> {unForbidPosSync}");
                        ReadPropCache(tempCheckState);
                    }
                }
            }
        }

        private void ReadPropCache(E_BattleStateType state)
        {
            string attrName = GetBattleState2Attr(state);
            if (attrName == AOIAttrDefine.Error)
            {
                return;
            }

            SyncBaseInfo syncBaseInfo;

            if (PropCache.TryGetValue(attrName, out syncBaseInfo))
            {
                PropCache.Remove(attrName);

                VitalSignAOIClientAttrs attrType = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)syncBaseInfo.Index);
                if (attrType != null)
                {
                    //object value = syncBaseInfo.GetType().GetProperty(syncBaseInfo.PropValueCase.ToString()).GetValue(syncBaseInfo, null);
                    //// 缓存
                    //CacheAttrValueByVitalSignAOIClientAttrs(attrType, value);
                    CacheAttrValueByVitalSignAOIClientAttrs(attrType, syncBaseInfo);
                    // 通知
                    object value = null;
                    string key = attrType.Name;
                    switch (syncBaseInfo.PropValueCase)
                    {
                        case SyncBaseInfo.PropValueOneofCase.None:
                            break;
                        case SyncBaseInfo.PropValueOneofCase.Int32Value:
                            {
                                value = Attrs.GetIntValue(key);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.Uint32Value:
                            {
                                value = Attrs.GetUIntValue(key);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.Int64Value:
                            {
                                value = Attrs.GetLongValue(key);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.Uint64Value:
                            {
                                value = Attrs.GetULongValue(key);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.FloatValue:
                            {
                                value = Attrs.GetFloatValue(key);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.DoubleValue:
                            {
                                value = Attrs.GetFloatValue(key);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.StringValue:
                            {
                                value = Attrs.GetStrValue(key);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.BoolValue:
                            {
                                value = Attrs.GetBoolValue(key);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.MsgValue:
                            {
                                value = Attrs.GetProtoValue(key);
                            }
                            break;
                        default:
                            break;
                    }
                    SetValueById(attrType, value);
                }
                //UnityEngine.Debug.Log($" {TagFlag} ReadPropCache: {state}  attrName {attrName} id=>{M_EntityID}  属性===》{syncBaseInfo.Index} 值===》 {syncBaseInfo}  ");
            }
        }

        #endregion

        // 缓存AOI中的属性数据
        private void HandleProperty(SyncBaseInfo syncBaseInfo)
        {
            VitalSignAOIClientAttrs attrType = GetVitalSignAOIClientAttrsByIndex((ushort)syncBaseInfo.Index);

            if (attrType == null)
            {
                SGF.Debuger.LogWarning($" {TagFlag} HandleProperty() id=>{M_EntityID},属性id={syncBaseInfo.Index},值={syncBaseInfo} error!!! 属性id客户端不认识");
                return;
            }

            //object val = syncBaseInfo.GetType().GetProperty(syncBaseInfo.PropValueCase.ToString()).GetValue(syncBaseInfo, null);

            /// 2023/6/28
            /// 对于 朝向/坐标 的 aoi属性变化, 不再 根据是否被原子锁锁住,就放入 缓存队列。
            /// 目前 因为 做了 客户端数据和 服务器数据的分离, 那原子锁锁住之后,只需要设置到服务器数据不推送view表现就可以了
            {
                //判断属性行否需要禁止，如果禁止，就禁止
                // bool isForbid = CheckAttrIsServerForbid(attrType.Name);
                //if (attrType.Name == AOIAttrDefine.State && M_EntityID != GameManager.Instance.mainPlayerId)
                //{
                //    SGF.Debuger.LogError($"死亡测试 数据层 缓存 ---------={val}");
                //    //SGF.Debuger.LogError($" {TagFlag} HandleProperty id=>{M_EntityID}  属性===》{syncBaseInfo.Index} 名=》{attrType.Name} 值===》 {val}");
                //}
                //if (M_EntityID == GameManager.Instance.mainPlayerId)
                //{
                //    SGF.Debuger.LogError($"属性同步创建 3.缓存-------属性id={syncBaseInfo.Index},值={syncBaseInfo}----------- 时间={SGF.Time.TimeUtils.TimeLogString()}");
                //}
                // 夏哥过来看了下，指点说，一组属性过来，就算有锁了移动，坐标还是要下发
                //----- 
                // if (isForbid)
                // {
                //     PropCache[attrType.Name] = syncBaseInfo;
                //     return;
                // }
            }

            // 临时打印伙伴属性值
            {
                if (EntityType == E_EntityType.Partner)
                {
                    if (attrType.Name == AOIAttrDefine.Atk)
                    {
                        SGF.Debuger.LogWarning($"伙伴属性 id={M_EntityID} 攻击力={syncBaseInfo.Int64Value}");
                    }
                    if (attrType.Name == AOIAttrDefine.TruthHp)
                    {
                        SGF.Debuger.LogWarning($"伙伴属性 id={M_EntityID} 最大生命值={syncBaseInfo.Int64Value}");
                    }
                }

            }



            //CacheAttrValueByVitalSignAOIClientAttrs(attrType, val);
            CacheAttrValueByVitalSignAOIClientAttrs(attrType, syncBaseInfo);

        }

        private void CacheAttrValueByVitalSignAOIClientAttrs(VitalSignAOIClientAttrs vitalSignAOIClientAttrs, object val)
        {
            if (!m_AttrWhiteList.Contains(vitalSignAOIClientAttrs.Name))
            {
                // 缓存的时候
                // 1.先查找当前的里面有没有指
                // 2.1【有，就取出来放到旧的里面】
                // 2.2【无，就不关了】
                if (Attrs.CheckCacheAttrName(vitalSignAOIClientAttrs))
                {
                    var oldVal = Attrs.GetLuaAoiValue(vitalSignAOIClientAttrs);
                    OldAttrs.CacheValueByKey(vitalSignAOIClientAttrs, ref oldVal);
                }
            }
            //【最终】都是同步属性
            Attrs.CacheValueByKey(vitalSignAOIClientAttrs, ref val);
        }

        private void CacheAttrValueByVitalSignAOIClientAttrs(VitalSignAOIClientAttrs vitalSignAOIClientAttrs, SyncBaseInfo syncBaseInfo)
        {
            string key = vitalSignAOIClientAttrs.Name;
            if (!m_AttrWhiteList.Contains(vitalSignAOIClientAttrs.Name))
            {
                // 缓存的时候
                // 1.先查找当前的里面有没有指
                // 2.1【有，就取出来放到旧的里面】
                // 2.2【无，就不关了】
                bool isHaveAttr = Attrs.CheckCacheAttrName(vitalSignAOIClientAttrs);
                if (isHaveAttr)
                {
                    switch (syncBaseInfo.PropValueCase)
                    {
                        case SyncBaseInfo.PropValueOneofCase.None:
                            break;
                        case SyncBaseInfo.PropValueOneofCase.Int32Value:
                            {
                                var oldVal = Attrs.GetIntValue(key);
                                OldAttrs.CacheIntMap(key, oldVal);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.Uint32Value:
                            {
                                var oldVal = Attrs.GetUIntValue(key);
                                OldAttrs.CacheUIntMap(key, oldVal);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.Int64Value:
                            {
                                var oldVal = Attrs.GetLongValue(key);
                                OldAttrs.CacheLongMap(key, oldVal);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.Uint64Value:
                            {
                                var oldVal = Attrs.GetULongValue(key);
                                OldAttrs.CacheULongMap(key, oldVal);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.FloatValue:
                            {
                                var oldVal = Attrs.GetFloatValue(key);
                                OldAttrs.CacheFloatMap(key, oldVal);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.DoubleValue:
                            {
                                var oldVal = Attrs.GetFloatValue(key);
                                OldAttrs.CacheFloatMap(key, oldVal);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.StringValue:
                            {
                                var oldVal = Attrs.GetStrValue(key);
                                OldAttrs.CacheStrMap(key, oldVal);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.BoolValue:
                            {
                                var oldVal = Attrs.GetBoolValue(key);
                                OldAttrs.CacheBoolMap(key, oldVal);
                            }
                            break;
                        case SyncBaseInfo.PropValueOneofCase.MsgValue:
                            {
                                var oldVal = Attrs.GetProtoValue(key);
                                OldAttrs.CacheProtoMap(key, ref oldVal);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            //【最终】都是同步属性
            switch (syncBaseInfo.PropValueCase)
            {
                case SyncBaseInfo.PropValueOneofCase.None:
                    break;
                case SyncBaseInfo.PropValueOneofCase.Int32Value:
                    {
                        Attrs.CacheIntMap(key, syncBaseInfo.Int32Value);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Uint32Value:
                    {
                        Attrs.CacheUIntMap(key, syncBaseInfo.Uint32Value);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Int64Value:
                    {
                        Attrs.CacheLongMap(key, syncBaseInfo.Int64Value);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Uint64Value:
                    {
                        Attrs.CacheULongMap(key, syncBaseInfo.Uint64Value);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.FloatValue:
                    {
                        Attrs.CacheFloatMap(key, syncBaseInfo.FloatValue);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.DoubleValue:
                    {
                        Attrs.CacheFloatMap(key, (float)syncBaseInfo.DoubleValue);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.StringValue:
                    {
                        Attrs.CacheStrMap(key, syncBaseInfo.StringValue);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.BoolValue:
                    {
                        Attrs.CacheBoolMap(key, syncBaseInfo.BoolValue);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.MsgValue:
                    {
                        Attrs.CacheProtoMap(key, syncBaseInfo.MsgValue);
                    }
                    break;
                default:
                    break;
            }
            //Attrs.CacheValueByKey(vitalSignAOIClientAttrs, ref val);
        }

        // 根据属性ID获取属性类型
        private VitalSignAOIClientAttrs GetVitalSignAOIClientAttrsByIndex(ushort index)
        {
            VitalSignAOIClientAttrs attrType = LocalDataManager.Instance.GetAttrPropByAttrIdx(index);

            return attrType;
        }

        // 通知AOI中的属性数据
        private void InvokeAttrChange(SyncBaseInfo syncBaseInfo)
        {
            VitalSignAOIClientAttrs attrType = GetVitalSignAOIClientAttrsByIndex((ushort)syncBaseInfo.Index);

            if (attrType == null)
            {
                SGF.Debuger.LogWarning($" {TagFlag} InvokeAttrChange() id=>{M_EntityID},属性id={syncBaseInfo.Index},值={syncBaseInfo} error!!! 属性id客户端不认识");
                return;
            }

            //判断属性行否需要禁止，如果禁止，就禁止
            //bool isForbid = CheckAttrIsServerForbid(attrType.Name);
            //if (isForbid)
            //{
            //    return;
            //}

            object val = null;
            string key = attrType.Name;
            switch (syncBaseInfo.PropValueCase)
            {
                case SyncBaseInfo.PropValueOneofCase.None:
                    break;
                case SyncBaseInfo.PropValueOneofCase.Int32Value:
                    {
                        val = Attrs.GetIntValue(key);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Uint32Value:
                    {
                        val = Attrs.GetUIntValue(key);
                        //状态发生变化的时候，才会收到服务器是状态属性同步
                        if (attrType.Name == AOIAttrDefine.State)
                        {
                            //UnityEngine.Debug.Log($" {TagFlag} InvokeAttrChange: {attr} id=>{M_EntityID}  属性===》{syncBaseInfo.Index} 值===》 {syncBaseInfo}  ");
                            //状态的值为 unit32 
                            HandleOnStateChange((E_BattleStateType)Convert.ToUInt32(val));
                            //SGF.Debuger.LogError($"主角属性同步 数据层 通知 ---------={val}");
                        }
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Int64Value:
                    {
                        val = Attrs.GetLongValue(key);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.Uint64Value:
                    {
                        val = Attrs.GetULongValue(key);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.FloatValue:
                    {
                        val = Attrs.GetFloatValue(key);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.DoubleValue:
                    {
                        val = Attrs.GetFloatValue(key);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.StringValue:
                    {
                        val = Attrs.GetStrValue(key);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.BoolValue:
                    {
                        val = Attrs.GetBoolValue(key);
                    }
                    break;
                case SyncBaseInfo.PropValueOneofCase.MsgValue:
                    {
                        val = Attrs.GetProtoValue(key);
                    }
                    break;
                default:
                    break;
            }

            //if (attrType.Name == AOIAttrDefine.curHp)
            //{
            //    SGF.Debuger.LogWarning($"主角血条 当前的 2222 ={val}");
            //}
            //if (attrType.Name == AOIAttrDefine.TruthHp)
            //{
            //    SGF.Debuger.LogWarning($"主角血条 最大值的 2222 ={val}");
            //}
            //if (attrType.Name == AOIAttrDefine.StorageDrugs)
            //{
            //    Attrs.CacheValueByKey(attrType, ref val);
            //}
            #region 过程

            //if (attrType.Name == AOIAttrDefine.Position && M_EntityID != GameManager.Instance.mainPlayerId)
            //{
            //    UnityEngine.Debug.Log($"属性同步坐标检查 InvokeAttrChange id=>{M_EntityID} ");
            //}
            //同步前面Handel已经缓存了最终的数值，这里设置直接拦截，这里缓存是此时时刻最新的值（哪怕是过去时）
            //路店会继续保持

            //if (attrType.Name == AOIAttrDefine.Position)
            //{
            //    //UnityEngine.Debug.Log($" {TagFlag} HandleProperty id => {M_EntityID}  battleStateType ===>{serverBattleStateType} , Update Position ==>  isForbid {isForbid}");

            //    //一个人，一帧，属性同步，位置信息====[只能是其他人的]
            //    //强同步，和技能都不在这里，主角也不应该用:主角会发但是不会用：其他包括其他人和物件子弹
            //    //if (isMainPlayer)
            //    //技能
            //    //Attrs.自己用的(attrType, ref val);
            //    return;//缓存再这里中断
            //}
            //if (attrType.Name == AOIAttrDefine.Rot)
            //{
            //    //Attrs.自己用的(attrType, ref val);
            //    return;//缓存再这里中断
            //}
            #endregion

            SetValueById(attrType, val);
        }


        /// <summary>
        /// 注册客户端本地的原子状态
        /// </summary>
        /// <param name="states">策划配置表中填的原子状态</param>
        public void HandleClientBattleStates(List<int> states, bool isRegister)
        {
            // if (M_EntityID == GameManager.Instance.mainPlayerId)
            // {
            //     if (isRegister)
            //     {

            //         SGF.Debuger.LogError($"[EntityBaseData] [states] 禁止 客户端 原子锁: {states.KJoin("| ")} ");
            //     }
            //     else
            //     {
            //         SGF.Debuger.LogError($"EntityBaseData] [states] 解开 客户端 原子锁: {states.KJoin("| ")} ");

            //     }
            // }
            E_BattleStateType clientState;
            for (int i = 0; i < states.Count; i++)
            {
                clientState = BattleStateUtils.GetBattleStateTypeByCfgState(states[i]);
                //if (EntityType == E_EntityDataType.Partner.ToString())
                //{
                //    SGF.Debuger.LogError($"技能测试 技能阶段原子锁11111 isRegister={isRegister},clientState={clientState},states={states[i]}");
                //}
                HandleItemClientBattleState(clientState, isRegister);
                //if (EntityType == E_EntityDataType.Partner.ToString())
                //{
                //    SGF.Debuger.LogError($"技能测试 技能阶段原子锁222222 isRegister={isRegister},clientState={clientState},states={states[i]}");
                //}
                //UnityEngine.Debug.Log($"技能阶段原子锁 isRegister={isRegister},clientState={clientState}，states={states}");
            }
            UpdateClientBattleStates();

            // 用来检测服务器的 原子锁是否解开的问题,可以不删
            //{
            // bool forbidMove = CheckStateIsClientForbid(E_BattleStateType.BattleState_ForbidMove);
            // string str = states.KJoin<int>("_");
            // bool isServerForbid = CheckStateIsServerForbid(E_BattleStateType.BattleState_ForbidMove);

            // UnityEngine.Debug.Log($"[SkillStage]  ExecuteStageStates  regist {isRegister} states {str} , forbidMove {forbidMove} , isServerForbid {isServerForbid} ");
            //}

            #region 临时代码

            if (M_EntityID == GameManager.Instance.mainPlayerId)
            {
                CheckClientStateTest();
            }

            #endregion
        }

        public void HandleClientBattleStates(List<E_BattleStateType> states, bool isRegister)
        {
            for (int i = 0; i < states.Count; i++)
            {
                HandleItemClientBattleState(states[i], isRegister);
            }

            // if (M_EntityID == GameManager.Instance.mainPlayerId)
            // {
            //     if (isRegister)
            //     {

            //         SGF.Debuger.LogError($"[EntityBaseData] [states] 禁止 原子锁: {states.KJoin("| ")} ");
            //     }
            //     else
            //     {
            //         SGF.Debuger.LogError($"EntityBaseData] [states] 解开 原子锁: {states.KJoin("| ")} ");

            //     }
            // }

            UpdateClientBattleStates();

            #region 临时代码

            if (M_EntityID == GameManager.Instance.mainPlayerId)
            {
                CheckClientStateTest();
            }

            #endregion
        }

        public void ClearClientBattleStates(List<E_BattleStateType> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                E_BattleStateType clientState = states[i];
                if (clientBattleStateRegisterCountDic.ContainsKey(clientState))
                {
                    clientBattleStateRegisterCountDic.Remove(clientState);
                }
            }

            UpdateClientBattleStates();
        }

        // 缓存客户端的原子锁状态
        private void HandleItemClientBattleState(E_BattleStateType clientState, bool isRegister)
        {
            int count = 0;
            if (clientBattleStateRegisterCountDic.ContainsKey(clientState))
            {
                count = clientBattleStateRegisterCountDic[clientState];
            }
            count = isRegister ? (count + 1) : (count - 1);

            if (count > 0)
            {
                clientBattleStateRegisterCountDic[clientState] = count;
            }
            else
            {
                clientBattleStateRegisterCountDic.Remove(clientState);
            }

            //SGF.Debuger.LogError($"  [状态切换] {TagFlag}  HandleItemClientBattleState id => {M_EntityID}  clientState ===>{clientState} , count ==> {count}  isRegister {isRegister}");
        }

        private void UpdateMixBattleStates(bool isServerChanage)
        {
            var lastE_BattleStateType = mixBattleStateType;

            mixBattleStateType = serverBattleStateType | clientBattleStateType;

            ActionOnBattleStateChange?.Invoke(lastE_BattleStateType, mixBattleStateType);

            // 死亡状态通知
            if (IsDead && isServerChanage)
            {
                GameManager.Instance.TriggerEvent(TriggerEventType.HeroDie, M_EntityID);
                ActionOnBattleStateDead?.Invoke();
            }
        }

        #region 临时代码

        public Action<E_BattleStateType, bool, int> ActionStateChange;

        private bool isClientForbidMove = false;
        private bool isClientForbidDir = false;

        private bool isServerForbidMove = false;
        private bool isServerForbidDir = false;
        private void CheckClientStateTest()
        {
            bool isForbidMove = CheckStateIsClientForbid(E_BattleStateType.BattleState_ForbidMove);
            if (isClientForbidMove != isForbidMove)
            {
                isClientForbidMove = isForbidMove;
                ActionStateChange?.Invoke(E_BattleStateType.BattleState_ForbidMove, isClientForbidMove, 0);
            }

            bool isForbidDir = CheckStateIsClientForbid(E_BattleStateType.BattleState_ForbidDir);
            if (isClientForbidDir != isForbidDir)
            {
                isClientForbidDir = isForbidDir;
                ActionStateChange?.Invoke(E_BattleStateType.BattleState_ForbidDir, isClientForbidDir, 0);
            }
        }

        private void CheckServerStateTest()
        {
            bool isForbidMove = CheckStateIsServerForbid(E_BattleStateType.BattleState_ForbidMove);
            if (isServerForbidMove != isForbidMove)
            {
                isServerForbidMove = isForbidMove;
                ActionStateChange?.Invoke(E_BattleStateType.BattleState_ForbidMove, isServerForbidMove, 1);
            }

            bool isForbidDir = CheckStateIsServerForbid(E_BattleStateType.BattleState_ForbidDir);
            if (isServerForbidDir != isForbidDir)
            {
                isServerForbidDir = isForbidDir;
                ActionStateChange?.Invoke(E_BattleStateType.BattleState_ForbidDir, isServerForbidDir, 1);
            }
        }

        #endregion

        // 更新客户端原子锁
        private void UpdateClientBattleStates()
        {
            E_BattleStateType clientState = E_BattleStateType.BattleStateType;
            foreach (KeyValuePair<E_BattleStateType, int> item in clientBattleStateRegisterCountDic)
            {
                clientState = clientState | item.Key;
            }
            // UnityEngine.Debug.Log($" {TagFlag} #1  UpdateClientBattleStates id => {M_EntityID} clientBattleStateType {clientBattleStateType} --->  {clientState}");
            clientBattleStateType = clientState;
            UpdateMixBattleStates(false);
        }

        // 检查客户端【状态】是否禁止
        private bool CheckStateIsClientForbid(E_BattleStateType state)
        {
            bool isForbid = false;
            isForbid = CheckState(clientBattleStateType, state, true);
            return isForbid;
        }

        // 检查【状态】是否禁止
        private bool CheckStateIsMixForbid(E_BattleStateType state)
        {
            return CheckState(mixBattleStateType, state, true);
        }

        /// <summary>
        /// 检查【状态】是否禁止
        /// 客户端、服务器 一个被禁止 即为禁止 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool CheckStateIsForbid(E_BattleStateType state)
        {
            //bool isClientForbid = CheckStateIsClientForbid(state);
            //bool isServerForbid = CheckStateIsServerForbid(state);
            //bool isForbid = isClientForbid || isServerForbid;
            bool isForbid = CheckStateIsMixForbid(state);

            //if (isForbid)
            //{
            //    if (M_EntityID == GameManager.Instance.mainPlayerId)
            //    {
            //        UnityEngine.Debug.Log($"{TagFlag}  CheckStateIsForbid id=>{M_EntityID},IsClientForbid={isClientForbid},isServerForbid={isServerForbid},state={state},isForbid={isForbid}");
            //    }
            //}
            return isForbid;
        }



        #region 改变玩家 不同状态对应的动画 数据list.
        /// <summary>
        /// 注册的 改变玩家 不同状态对应的动画 数据list.
        /// note: 
        ///     1.目前改变 玩家 不同状态动画 采用buff 来实现;
        ///     2.目前 跟 郑亮 约定 如下:
        ///         01: 不同 的buff 动作替换 是 相互替换的关系,后面buff的动作 [整租] 替换前面buff,注意,是整租,而不是单个状态的动画;
        ///         02: buff 的动作替换 有 时间顺序, 后面buff 替换前面buff的动作. 如果最后的buff 提前取消了, 就采用取消了的前一个.
        /// 
        ///     3.基于上面的约定, 动画替换的数据采用 List 结构, 保证时间的前后顺序. 当前的动画替换数据,永远采用数据最后一个.
        ///       同时, 如果有后续 的需求: 动作替换不再是 整租替换, 而是单个状态的替换, 这种设计 也好修改.
        /// </summary>
        private List<ChangeAnimsData> changeAnimsDatas = new();

        public Action<ChangeAnimsData> ActionRefreshCurChangeAnims;

        /// <summary>
        /// 刷新当前的 动画
        /// </summary>
        public Action ActionRefreshCurAnims;

        private int FindChangeAnimsData(string tag)
        {
            int index = changeAnimsDatas.FindIndex((ChangeAnimsData changeAnimsData) =>
            {
                return changeAnimsData.Tag == tag;
            });
            return index;
        }
        /// <summary>
        /// 注册动作替换数组
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="changeAnims"></param>
        public void RegisterChangeAnim(string tag, List<SkillEditor.ChangeAnim> changeAnims)
        {
            int idx = FindChangeAnimsData(tag);
            // 如果找到了,那就是重复注册了
            if (-1 != idx)
            {
                return;
            }
            ChangeAnimsData changeAnimsData = new();
            changeAnimsData.Init(tag, changeAnims);
            changeAnimsDatas.Add(changeAnimsData);
            ActionRefreshCurChangeAnims.Invoke(CurChangeAnim());
        }

        /// <summary>
        /// 取消动作替换数组
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="changeAnims"></param>
        public void UnRegisterChangeAnim(string tag, List<SkillEditor.ChangeAnim> changeAnims)
        {
            int idx = FindChangeAnimsData(tag);
            // 如果找不到了,那就万事大吉
            if (-1 == idx)
            {
                return;
            }
            changeAnimsDatas.RemoveAt(idx);
            ActionRefreshCurChangeAnims.Invoke(CurChangeAnim());
        }

        /// <summary>
        /// 当前的 动画状态 数据
        /// </summary>
        public ChangeAnimsData CurChangeAnim()
        {
            if (changeAnimsDatas.Count == 0)
            {
                return null;
            }
            return changeAnimsDatas[changeAnimsDatas.Count - 1];
        }
        #endregion

        #region 增加一个 玩家身上 数据变化 监听的通用接口
        private Dictionary<string, Dictionary<string, List<OnDatatChangeDelegate>>> dataChangeHandles = new();

        /// <summary>
        /// 注册 监听的接口
        /// </summary>
        /// <param name="regKey"></param>
        /// <param name="listener"></param>
        /// <param name="tag">唯一的 key, 可以用来批量删除的 主键</param>
        public void RegChangeListener(string regKey, OnDatatChangeDelegate listener, string tag)
        {
            Dictionary<string, List<OnDatatChangeDelegate>> tag2Listeners;

            if (!dataChangeHandles.ContainsKey(regKey))
            {
                tag2Listeners = new Dictionary<string, List<OnDatatChangeDelegate>>();
            }
            else
            {
                tag2Listeners = dataChangeHandles[regKey];
            }

            List<OnDatatChangeDelegate> listeners;
            if (!tag2Listeners.ContainsKey(tag))
            {
                listeners = new List<OnDatatChangeDelegate>();
            }
            else
            {
                listeners = tag2Listeners[tag];
            }

            // 如果 有重复的 注册的话, 就直接 return 不处理
            if (listeners.Contains(listener))
            {
                return;
            }
            listeners.Add(listener);

            tag2Listeners[tag] = listeners;

            dataChangeHandles[regKey] = tag2Listeners;
        }

        private void UnRegChangeListener(string regKey, OnDatatChangeDelegate listener, string tag)
        {
            if (!dataChangeHandles.ContainsKey(regKey))
            {
                return;
            }
            Dictionary<string, List<OnDatatChangeDelegate>> tag2Listeners = dataChangeHandles[regKey];

            if (!tag2Listeners.ContainsKey(tag))
            {
                return;
            }

            List<OnDatatChangeDelegate> listeners = tag2Listeners[tag];
            listeners.Remove(listener);

            if (listeners.Count == 0)
            {
                tag2Listeners.Remove(tag);
            }

            if (tag2Listeners.Count == 0)
            {
                dataChangeHandles.Remove(regKey);
            }
        }

        /// <summary>
        /// 取消 注册 key 下面 与 所有 tag 相关的 事件监听
        /// </summary>
        /// <param name="regKey"></param>
        /// <param name="tag"></param>
        public void UnRegChangeListeners(string regKey, string tag)
        {
            if (!dataChangeHandles.ContainsKey(regKey))
            {
                return;
            }
            Dictionary<string, List<OnDatatChangeDelegate>> tag2Listeners = dataChangeHandles[regKey];

            if (!tag2Listeners.ContainsKey(tag))
            {
                return;
            }

            tag2Listeners.Remove(tag);

            if (tag2Listeners.Count == 0)
            {
                dataChangeHandles.Remove(regKey);
            }
        }

        /// <summary>
        /// 直接 取消 regKey 相关的 所有监听
        /// </summary>
        /// <param name="regKey"></param>
        public void UnRegChangeListenersByRegKey(string regKey)
        {
            dataChangeHandles.Remove(regKey);
        }

        public void UnRegAllTagChangeListeners(string tag)
        {
            List<string> tempKeys = new();
            foreach (KeyValuePair<string, Dictionary<string, List<OnDatatChangeDelegate>>> item in dataChangeHandles)
            {
                Dictionary<string, List<OnDatatChangeDelegate>> tag2Listeners = item.Value;
                if (tag2Listeners.ContainsKey(tag))
                {
                    tag2Listeners.Remove(tag);
                }

                if (tag2Listeners.Count == 0)
                {
                    tempKeys.Add(item.Key);
                }
            }
            tempKeys.ForEach((key) =>
            {
                dataChangeHandles.Remove(key);
            });

        }

        /// <summary>
        /// 提取的 通用的 触发 数据变化的 接口
        /// </summary>
        /// <param name="regKey"></param>
        /// <param name="tag"></param>
        /// <param name="triggerData"></param>
        public void TriggerChange(string regKey, object triggerData)
        {
            if (!dataChangeHandles.ContainsKey(regKey))
            {
                return;
            }
            Dictionary<string, List<OnDatatChangeDelegate>> tag2Listeners = dataChangeHandles[regKey];

            List<KeyValuePair<string, List<OnDatatChangeDelegate>>> tempTag2Listeners = tag2Listeners.KToList();

            foreach (KeyValuePair<string, List<OnDatatChangeDelegate>> item in tempTag2Listeners)
            {
                List<OnDatatChangeDelegate> handles = item.Value;

                // note: 此处 不用 forEach 是为了防止 handle 在执行的 时候 会去取消 监听,导致 foreach报错
                for (int i = 0; i < handles.Count; i++)
                {
                    handles[i].Invoke(triggerData);
                }
            }

            tempTag2Listeners.Clear();
            //
            GlobalEvent.OnPropChange?.Invoke(M_EntityID, regKey, triggerData);
        }

        /// <summary>
        /// 注册 针对 buff 的监听
        /// </summary>
        /// <param name="buffID"></param>
        /// <param name="tag"></param>
        public void RegBuffListener(int buffID, OnDatatChangeDelegate listener, string tag)
        {
            RegChangeListener($"buff_{buffID}", listener, tag);
        }

        public void UnRegBuffListeners(int buffID, string tag)
        {
            UnRegChangeListeners($"buff_{buffID}", tag);
        }

        public void TriggerBuffChange(int buffID, string buffEvent, object value)
        {
            string regKey = $"buff_{buffID}";
            //SGF.Debuger.LogError($"buff_{buffID} CheckConditions buffEvent: {buffEvent}");
            if (!dataChangeHandles.ContainsKey(regKey))
            {
                return;
            }
            BuffEventData buffEventData = new(buffID, buffEvent, value);

            TriggerChange(regKey, buffEventData);
        }


        private Dictionary<string, int> uiConfigHiddenCount = new();

        private void RefreshUIHiddenCount(HashSet<string> uiConfigs, bool onEnter)
        {
            foreach (var item in uiConfigs)
            {
                if (!uiConfigHiddenCount.ContainsKey(item))
                {
                    uiConfigHiddenCount.Add(item, 0);
                }
                var count = uiConfigHiddenCount[item];

                uiConfigHiddenCount[item] = onEnter ? count++ : count--;

                if ((onEnter && count > 0) || (!onEnter && count == 0))
                {
                    TriggerChange(item, new TriggerTypeEffectData(GameConfig.HIDDEN_UI_EVENT, onEnter));
                }
            }
        }

        /// <summary>
        /// 触发 隐藏 ui 效果改变
        /// </summary>
        /// <param name="uiConfigs"></param>
        /// <param name="onEnter"></param>
        public void TriggerHiddenUIChange(HashSet<string> uiConfigs, bool onEnter)
        {
            RefreshUIHiddenCount(uiConfigs, onEnter);
        }


        #endregion

    }

    public static class BattleStateUtils
    {
        public static E_BattleStateType GetBattleStateTypeByCfgState(int state)
        {
            return (E_BattleStateType)(1 << state);
        }
    }
}