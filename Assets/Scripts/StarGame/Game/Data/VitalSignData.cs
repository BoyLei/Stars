using Google.Protobuf.Collections;
using ProtoMsg;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProjectDef;
using System;
using System.Collections.Generic;
/// <summary>
/// GetType().GetProperty(); 属性
/// GetType().GetMethod();方法
/// GetType().GetFields();字段
/// </summary>

namespace StarProject.Game.Data
{
    ///【玩家数据属性了】
    /// <summary>
    /// AOI中动态分配的生命体征数据（也就是说动物的父类，和物体的子类，相当于动物植物，具备细胞的生物）
    /// 物体
    /// 1，非生命体征（子弹，显示的召唤物，不显示的召唤物），【归属者：他是谁的东西】
    /// （mix：团队，积分或者击杀连杀，（从属者），AI，名字、（驱动器，轨迹，发射器），Id），
    /// 2有生命体征：伙伴，宝宝，技能，【归属者】
    /// 
    /// 1生命体
    /// 2[模型]
    /// 3[武器]
    /// 4[管理子生命体集合]
    /// 5其他必要显示数据
    /// 6其他必要逻辑属性据
    /// </summary>
    //[ProtoContract]
    [XLua.LuaCallCSharp]
    public class VitalSignData : EntityBaseData//PlayerData的父级//数据层：包含显示层，和实体逻辑层的必要数据
    {
        public bool isMainPlayer = false;//当前控制组，当前控制的人。

        public bool IsGVEBoss = false;// 是否GVE怪物
        public bool IsRobot = false;// 是否机器人

        public bool hasModel = true; // 是否有模型
        public NPCEntityBase myOwnerNtt;
        public EntityCtrlBase myOwnerNttGroup;

        /// <summary> 是否遥感移动动作（只是摇杆移动,不判断原子锁） </summary>
        public bool M_is_JoySitckMoving { get; set; }

        /// <summary> 是否 是正在移动</summary>
        public bool M_Is_Moveing { get; set; }

        /// <summary>
        /// 这个玩家对应的用户的ID 
        /// </summary>
        //[ProtoMember(2)] 
        public uint userId;//accountID//账号id没用

        #region 技能相关变量
        // 单技能信息
        ///     message SingleSkillInfo
        ///     {
        ///         required int32 SkillDBID = 1; // 职业技能编号
        ///         required int32 SkillLevel = 2; // 技能等级
        ///         required int32 TalentType = 3; // 天赋类型
        ///         required int32 TalentID = 4; //天赋编号
        ///     }
        ///     
        /// <summary>
        /// 人物的技能信息列表
        /// note:
        ///     SkillDBID + TalentID 共同决定一个SingleSkillInfo。
        /// note2：
        ///     目前策划和服务器的设计是 单符文页的设计，那就只有唯一一份天赋。
        ///     即Skilllist 中只存在一份 唯一的 SkillDBID(职业技能ID)
        /// </summary>
        public DictionaryEx<int, ProtoMsg.SingleSkillInfo> skillListMap = new DictionaryEx<int, ProtoMsg.SingleSkillInfo>();
        // 服务器目前是单符文页 单天赋的设计，所以只有唯一天赋，所以list 方式数据结构先注释
        // private List<ProtoMsg.SingleSkillInfo> skillList = new List<ProtoMsg.SingleSkillInfo>();

        /// <summary>
        /// 人物技能位信息列表
        /// </summary>
        public List<ProtoMsg.SkillPos> skillPosList = new List<ProtoMsg.SkillPos>();

        #endregion


        /// <summary>
        /// 玩家/伙伴/宝宝 的名字
        /// </summary>
        //[ProtoMember(3)] 
        public string name;

        /// <summary>
        /// 【模型，和Avatar】数据
        /// </summary>
        //[ProtoMember(4)] 
        public VitalSignViewShowData viewEnityData = new VitalSignViewShowData();

        /// <summary>
        /// 玩家的组队ID，如果是单人，则等于id
        /// 通常分成ABCDEFG，和中立Z
        /// </summary>
        //[ProtoMember(5)] 
        public int teamId = 0;

        /// <summary>
        /// 玩家在局中的得分成就/或者击杀数据/或要记录的某些特殊数据
        /// </summary>
        //[ProtoMember(6)] 
        //public int score = 0;

        /// <summary>
        /// 如果这个玩家挂机，或者是AI玩家的话，其AI的ID，如果0则是不使用AI
        /// AI是这个生命体征的生物是否具备的电脑驱动能力/或者玩家控制者驱动的
        /// 多说一点子弹也具备Ai
        /// </summary>
        //[ProtoMember(7)] 
        public int ai = 0;

        /// <summary>
        /// 归属者，会延迟创建，注册给主人，消失开始读注册给主人，用IRegable
        /// 吸血，自己反补自己，等都需要
        /// 就别类了，或者你可以设计成类，服务器传递给你id，你看这个id能不能注册进去
        /// 如自己就等于自己id，如果有就给主人id
        /// </summary>
        //[ProtoMember(8)] 
        //public int Ownid = 0;


        //【跟随者ID】：也可能服务器帮你注册好了
        //[ProtoMember(9)] public PartnerData otherData = new OtherData();


        //【零部件数据】拥有的其他基本数据信息：根据类似还是外观数据，但是会根据职业，根据part拆分，例如武器，头饰，服装，披风，鞋子，（时装：当然这个在人物显示数据中）
        //[ProtoMember(10)] public EquipPartData otherData = new OtherData();

        /// <summary>
        /// 所有ntt都应具有位置属性 ：  public virtual Vector3 Position()
        /// </summary>
        //[ProtoMember(11)] 


        protected override void Create(ulong entityId)
        {
            base.Create(entityId);
            viewEnityData = new VitalSignViewShowData();
        }

        public override void Create(ulong entityId, E_EntityType entityType, bool isServerAOI)
        {
            base.Create(entityId, entityType, isServerAOI);
            viewEnityData = new VitalSignViewShowData();
        }

        protected override void Release()
        {
            base.Release();
            viewEnityData = null;
            IsGVEBoss = false;
            IsRobot = false;
            hasModel = true;
            hasChangeSkillSlot = false;
            skillListMap.Clear();
            skillPosList.Clear();
            changeSkillSlots.Clear();
            ActionOnAllCDNotice = null;
            ActionOnRoleAllCDListNtf = null;
            ActionOnAllSkillNotice = null;
            ActionOnCDUpdateNotice = null;
            ActionOnChangeSkillSlot = null;
        }

        #region 小曲新寫的
        //private DictionaryEx<string, Func<object, bool>> m_attrFuncData = new DictionaryEx<string, Func<object, bool>>();


        /// <summary>
        /// 属性同步执行方法
        /// </summary>
        /// <param name="attrName">属性名</param>
        /// <param name="value">值</param>
        //public void CallFuncName(string attrName, object value)
        //{
        //    // 查找类下方法名
        //    // 委托Func调用方法传参
        //    if (m_attrFuncData.ContainsKey(attrName))
        //    {
        //        Func<object, bool> func = m_attrFuncData[attrName];
        //        func(value);
        //    }
        //    else
        //    {
        //        MethodInfo method = GetType().GetMethod("Set" + attrName);
        //        if (method != null)
        //        {
        //            // Func<类型,最后一个类型>  最后一个类型始终是方法的返回值类型
        //            Func<object, bool> func = (Func<object, bool>)Delegate.CreateDelegate(typeof(Func<object, bool>), this, method);
        //            func(value);
        //            m_attrFuncData.Add(attrName, func);
        //        }
        //    }
        //}

        #endregion



        #region 老的我幫忙改的
        /// <summary>
        /// 属性同步执行方法
        /// </summary>
        /// <param name="attrName">属性名</param>
        /// <param name="value">值</param>
        //public void CallFuncName(string attrName, object value)
        //{
        //    // 查找类下方法名
        //    // 委托Func调用方法传参
        //    //反射“字段的”Set属性方法
        //    MethodInfo method = GetType().GetMethod("Set"+attrName);
        //    if (method != null)
        //    {
        //        //方法1
        //        // Func<类型,最后一个类型>  最后一个类型始终是方法的返回值类型
        //        //映射在：本类实例中，“服务器类型名Str”映射的Set方法
        //        //Func<object, bool> func = (Func<object, bool>)Delegate.CreateDelegate(typeof(Func<object, bool>), this, method);
        //        //func(value);

        //        //方法2獲取屬性，直接賦值屬性  

        //        //方法3直接給
        //        method.Invoke(this, new object[] { value });

        //    }
        //}


        #endregion
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="localValue"></param>
        /// <returns></returns>
        //public T GetConfValueByName(E_AttrName localValue)
        //{
        //    //65535（1000开始）
        //    var value = GetValueByName(localValue);
        //    int remoteAttrIndex = (int)localValue + LocalDataManager.Instance.M_PlayerInfoData.startIndex;
        //    string strType = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)remoteAttrIndex).type;
        //    Type.GetType(strType)
        //    Convert.
        //}

        /// <summary>
        /// Set/反射用
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        //public bool SetName(object v)
        //{
        //    Attrs.Name = (string)v;
        //    return true;
        //}

        //public bool SetRoleModel(object v)
        //{
        //    Attrs.RoleModel = (UInt32)v;
        //    return true;
        //}

        //public bool SetCurHeroId(object v)
        //{
        //    Attrs.CurHeroId = (UInt32)v;
        //    return true;
        //}

        //public bool SetPlayerLevel(object v)
        //{
        //    Attrs.PlayerLevel = (Int32)v;
        //    return true;
        //}


        #region 技能跟数据相关接口

        /// <summary>
        /// 当所有技能刷新的时候通知的action，一般是刚进入游戏 或者换职业技能的时候，需要刷新所有技能
        /// 才会执行这个action
        /// </summary>
        public Action<RepeatedField<ProtoMsg.CDData>> ActionOnAllCDNotice;
        public Action<RepeatedField<ProtoMsg.CDData>> ActionOnRoleAllCDListNtf;

        public Action<object> ActionOnAllSkillNotice;

        /// <summary>
        /// 通知 技能槽 发生技能变化的通知.
        /// note:
        ///     目前跟 gl 确定的是 变身切换技能是一套技能统一替换.
        ///     如果 有槽位没有配置技能,则表示 这个槽位需要隐藏;
        ///     所以有一下几种情况:
        ///         1.如果 ActionOnChangeSkillSlot.invoke([]), 表示 所有槽位 都需要隐藏;
        ///         2.如果 ActionOnChangeSkillSlot.invoke(null), 表示取消技能槽的替换, 所有槽位 都需要显示;
        ///         3.如果 ActionOnChangeSkillSlot.invoke([1]), 表示槽位1 替换技能, 其它槽位 都需要隐藏;
        /// </summary>
        public Action<List<SkillEditor.TalentAndSlot>> ActionOnChangeSkillSlot;

        /// <summary>
        /// CD刷新的Action
        /// </summary>
        public Action<ProtoMsg.CDData> ActionOnCDUpdateNotice;

        /// <summary>
        /// 需要变身的 技能槽位
        /// </summary>
        public List<SkillEditor.TalentAndSlot> changeSkillSlots = new();

        private bool hasChangeSkillSlot = false;
        /// <summary>
        /// 是否 有变身技能效果标签. 
        /// note:
        ///     changeSkillSlots 如果为 [], 存在两种情况:
        ///         1.变身技能效果标签里面 配的就是[],表示 隐藏所有的 技能槽;
        ///         2.变身效果结束, changeSkillSlots =[] ; 表示已经没有 变身技能;
        /// 
        ///     基于上,  增加一个  HasChangeSkillSlot 来区分上面的情况
        /// </summary>
        public bool HasChangeSkillSlot => hasChangeSkillSlot;

        /// <summary>
        /// 检查 玩家身上 soltID 槽位是否 有变身的 技能
        /// </summary>
        /// <param name="slotID"></param>
        public bool IsChangeSkillSlot(int slotID)
        {
            if (!HasChangeSkillSlot)
            {
                return false;
            }

            return changeSkillSlots.KContains((SkillEditor.TalentAndSlot item) =>
            {
                return item.SlotID == slotID;
            });
        }

        public void UpdateChangeSlots(bool hasSkillSlot, List<SkillEditor.TalentAndSlot> skillSlots)
        {
            hasChangeSkillSlot = hasSkillSlot;
            changeSkillSlots.Clear();

            if (HasChangeSkillSlot)
            {
                changeSkillSlots.AddRange(skillSlots);
                ActionOnChangeSkillSlot?.Invoke(changeSkillSlots);
            }
            else
            {
                ActionOnChangeSkillSlot?.Invoke(null);
            }

        }

        public SingleSkillInfo GetSingleSkillInfo(int jobSkillID)
        {
            SingleSkillInfo singleSkillInfo = null;

            if (skillListMap.TryGetValue(jobSkillID, out singleSkillInfo))
            {
                return singleSkillInfo;
            }
            return null;
        }

        protected override void Create()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
