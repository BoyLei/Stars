using SGF.Utlis;
using Sirenix.OdinInspector;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

///--------------------------------------------------------------------
/// 文件名   :   TaskEnumDefine.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/10 09:35:59
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace Task
{
    public enum TaskType
    {
        /// <summary>
        /// 对话
        /// </summary>
        Dialogue = 1, //

        /// <summary>
        /// 关卡
        /// </summary>
        Level = 2, //

        /// <summary>
        /// 杀怪
        /// </summary>
        KillMonster = 3, //

        /// <summary>
        /// 收集
        /// </summary>
        Collect = 4, //

        /// <summary>
        /// 使用(道具) 
        /// </summary>
        UseItem = 5, //

        /// <summary>
        /// 使用(技能) 6
        /// </summary>
        UseSkill = 6,

        /// <summary>
        /// 跑腿/到达 7
        /// </summary>
        Arrive = 7, //

        /// <summary>
        /// 护送 8
        /// </summary>
        Escort = 8, //

        /// <summary>
        /// 交互 9
        /// </summary>
        InterAction = 9, //

        /// <summary>
        /// 序章 10
        /// </summary>
        Prologue = 10, //

        /// <summary>
        /// 黑幕 11
        /// </summary>
        Shady = 11, //

        /// <summary>
        /// 杀怪获得道具
        /// </summary>
        KillMonsterGetItem = 12,

        /// <summary>
        /// 背包中有指定道具
        /// </summary>
        BagItems = 13,

        /// <summary>
        /// 交互获得道具
        /// </summary>
        InterGetItem = 14,

        /// <summary>
        /// 引导使用道具
        /// </summary>
        GuideUseItem = 15,

        /// <summary>
        /// 引导上阵伙伴
        /// </summary>
        GuidePartnerInBattle = 16,

        /// <summary>
        /// 玩法
        /// </summary>
        Plays = 17,
        /// <summary>
        /// 玩家等级
        /// </summary>
        PlayerLevel = 18,
        /// <summary>
        /// 玩家冒险等级
        /// </summary>
        AdventureLevel = 19,
        /// <summary>
        /// 系统交互
        /// </summary>
        SystemInteract = 20,
        /// <summary>
        /// 今日登录
        /// </summary>
        LoginToday = 21,
        /// <summary>
        /// x商店购买道具x次
        /// </summary>
        ShopBuyGoods = 22,
        /// <summary>
        /// 参与x阶通缉x次
        /// </summary>
        WantTaskCountStage = 23,
        /// <summary>
        /// 玩家技能升级
        /// </summary>
        PlayerSkillLevelUp = 24,
        /// <summary>
        /// 击杀怪物来源
        /// </summary>
        SkillMonsterSource = 25,
        /// <summary>
        /// 签到领奖次数
        /// </summary>
        SignInAward = 26,
        /// <summary>
        /// 完成指定id的任务
        /// </summary>
        FinishTask = 27,
        /// <summary>
        /// 总计强化等级X
        /// </summary>
        TotalIntensify = 28,
        /// <summary>
        /// N个X级以上佣兵
        /// </summary>
        PartnerCount = 29,
        /// <summary>
        ///秘境X层通关
        /// </summary>
        SecretAreaLevel = 30,
        /// <summary>
        ///X类型技能X级
        /// </summary>
        SkillLevel = 31,
        /// <summary>
        ///完成副本ID(蚀梦之影)
        /// </summary>
        ShadowofDreamErosionPass = 32,
        /// <summary>
        ///激活N个X级以上Y品质以上护符
        /// </summary>
        TalismanQualityCount = 33,
        /// <summary>
        ///获取N个X级纹章
        /// </summary>
        HerldryCount = 34,
        /// <summary>
        ///爬塔X层
        /// </summary>
        PersonTowerLevel = 35,
        /// <summary>
        ///累计获取N件X类型道具
        /// </summary>
        PartnerEquipCount = 36,
        /// <summary>
        ///N件X级装备洗练度达到Y
        /// </summary>
        EquipmentTrainingLevel = 37,
        /// <summary>
        ///点亮天赋点X个
        /// </summary>
        TalentCount = 38,
        /// <summary>
        ///某位置(1，2，3代表出战，助战，任意位置)伙伴任意资质超过X
        /// </summary>
        PartnerQualification = 39,
        /// <summary>
        ///X模块信标评分达到(X需要支持总模块)
        /// </summary>
        ModuleRating = 40,
        /// <summary>
        ///任意呜器达到x级
        /// </summary>
        BuzzerLevel = 41,
        /// <summary>
        ///z频道发送r条消息
        /// </summary>
        ChannelMessages = 42,
        /// <summary>
        ///获取z级以上品质以上装备I件
        /// </summary>
        GetEquipQuality = 43,
        /// <summary>
        ///穿戴x级以上品质以上装备件
        /// </summary>
        WearingEquipQuality = 44,
        /// <summary>
        ///X商店累计消耗货币y数量N
        /// </summary>
        ShopSpendCurrency = 45,
        /// <summary>
        ///激活I个z级战技
        /// </summary>
        ActivationWar = 46,
        /// <summary>
        ///指定邮件标题领奖次数
        /// </summary>
        MailRewarded = 47,
        /// <summary>
        ///所有装备强化至XXXX级
        /// </summary>
        TotalEquipIntensify = 48,
        /// <summary>
        ///穿戴指定ID护符
        /// </summary>
        WearingQualityAmulet = 49,
        /// <summary>
        ///装配指定ID鸣器
        /// </summary>
        WearingTweeter = 50,
        /// <summary>
        ///活跃度
        /// </summary> 
        Activitylevel = 51,
        /// <summary>
        ///进入指定地图
        /// </summary> 
        EnterMap = 52,
        /// <summary>
        ///创建或加入帮会
        /// </summary> 
        CreateOrJoinGuild = 53,
        /// <summary>
        ///养成/指定部位强化等级达到X级
        /// </summary> 
        PartsIntensifyLevel = 54,
        /// <summary>
        ///养成/切换X槽位技能天赋
        /// </summary> 
        ChangeSkillTalent = 55,
        /// <summary>
        ///养成/指定位置装配数量X伙伴
        /// </summary> 
        PosEquipPartner = 56,
        /// <summary>
        ///养成/Z类型纹章镶嵌X星次石
        /// </summary> 
        EmblemSetStarStone = 57,
        /// <summary>
        /// 玩法/通关X组Y难度时序残响
        /// </summary>
        PersonDailyPass = 58
    }

    public enum TaskClassifyType
    {
        MainLine = 1, //主线
        SubbranchLine = 2, //支线
        Biography = 3, // [传记]
        Play = 4,
        Guide = 5,
        Other = 6,
        Challenge = 7,
    }

    public enum TaskPeriod
    {
        TaskPeriodForever = 0, //永久
        TaskPeriodDay = 1, //每日
        TaskPeriodWeek = 2 //周长
    }


    public enum E_FindType
    {
        FindPath = 0,
        Fly = 1
    }

    public enum E_FindPath
    {
        None = 0, //没有寻路
        Spawer = 1, //Spawer
        NPC = 2, //指定位置
        InterAction = 3, //交互物
        Area = 4, //区域
        Position = 5,
        Wanted = 6, // 通缉目标
        ChanageMapPosition = 7, // 切地图寻路
        JumpTo = 10,//跳转
        DoGuide=11, //引导
    }

    public enum FunctionType
    {
        None = 0,
        ReleaseSkill = 1, //释放技能                    [skillID,skilllevel]
        AddBuff = 2, //添加Buff                    [buffid,buffcnt]
        RemoveBuff = 3, //移除Buff                    [buffid]
        ReciveTask = 4, // 接收任务                  [TaskID]
        FinishTask = 5, //完成任务                    [TaskID]
        EntityLevel = 6, //进入副本                    [LevelID]
        Dialogue = 7, //触发对话                    [Dialogue]
        PlayBlack = 8, //播放黑幕                     [黑幕ID]
        TaskProcess = 9, //任务进度增加
        PlayTimeline = 10, //播放Timeline
        PlayPlot = 11, //播放章节效果
        PlayImage = 12, //播放剧情图片
        OpenNpcShop = 13, //打开NPC商店   [商店id]
        SendCustomEvent = 14, //发送事件      [key string]
        SceneControlFlag = 15, //大场景标记位控制  1,2(1开启2关闭)  标记位具体意义，1是滤镜
        SameSceneTranslate = 16, //同场景TP        [坐标X，坐标Y，坐标Z，角度
        ChangeNpcState = 17, //切换NPC状态机          【NPCID,状态】
        ClientSendModuleMsg = 18, //客户端发送事件   【ModuleName, EventName] 
        TeamEctypeChallenge = 19, //发起组队日常本挑战
        TreasureFind = 20, //藏宝图挖宝   
        SendBattleOpenEvent = 21, //发送关卡开启事件 
        ActiveSceneCam = 22, //激活场景虚拟相机
        RiskLevelBreak = 23, //冒险等级突破效果
        UpItem = 24, // 加、减道具进背包
        ModifyEntityTempVisibility = 25, //修改对象临时显隐
        PlayAnimation = 26, //播放指定动画
        PlayFx = 27, //播放指定特效
        ChanageHide = 28, //切换显隐【设置服务器标记】
        AddPassiveSkill = 29, //添加被动
        TransJob = 30, //转职
        TriggerGuide = 31, //触发引导    
        ShowMessage = 32, //显示飘字
        ChangeSceneObjState = 33, //改变场景对象状态
        NpcMove = 34, //NPC移动
        Translate = 35, //场景传送
        Wait = 36, //等待
        PassiveRemove = 37, //移除被动效果
        ScreenImpulse = 38, //屏幕震动
        PlayPv = 39,    // 播放视频
        ExitCurMap=40,  //离开本地图（副本）
        ServerModifyEntityVisible = 41,       //服务器修改对象的显隐
        CheckBlackBoard = 42,       //副本黑板
        SystemGuide = 43,//系统引导//GuideID；int
        PartnerDialog= 44,//伙伴喊话//多语言Key
        ChanageSceneTransitionData = 45,//切图过渡信息配置 1.过渡类型 2.地图基础信息id 3.PV表ID
        FakePartnerUI = 46, //假伙伴UI
        Transition = 47, //转场过渡
        //48 服务器派对时刻 通知答题选项给 Lobby
        //49 服务器派派对时刻  领取公会积分奖励
        //50 服务器派派对时刻   重置挂机计数
        XinSGShowUI = 51, // 新手关显隐UI  -1：遥感和其他UI  其他数值是技能槽位ID
        PartnerJoin = 52, // 伙伴加入效果
        PartnerLevel = 53, // 伙伴离开效果
        ShowMessageBox =54,//系统弹窗
        EndGuide=55,        //结束引导
        PlayEffectFormPlayerToPoint=56,     //从玩家身上播放特效到固定点
        ScenePlaysControl = 57, //场景玩法开关控制
        ShowEnterDungeonMenu = 58, //副本进入面板弹出
        ShowMindRepair = 59, //打开心灵修复小游戏
    }

    /// <summary>
    /// 移动类型
    /// </summary>
    public enum MoveType
    {
        Spawner = 0,

        Path = 1,
    }

    public enum TaskEntityType
    {
        None = 0,
        Npc = 1,
        Obj = 2,
        Mon = 3
    }

    public enum TaskEntityHideType
    {
        None = 0, // 未定义
        NotLookOtherPlayer = 1, // 不能看见别人
        OtherPlayerNotLookSelf = 2, // 别人看不见我
        FinishGuideCopy2 = 3,//通知服务器新手关2完成
        FinishGuideCopy1 = 4,//通知服务器新手关1完成
        FinishGuideCopy3 = 5,//通知服务器新手关3完成
        RefreshRingTask = 6,//刷新环任务
    }

    public enum TaskPassiveSkillSourceType
    {
        Default = 0, // 默认
        SecretArea = 1, // 秘境
    }

    public enum ConditionType
    {
        None = 0,
        LevelLimit = 1, //等级限制
        TaskIsFinish = 2, //任务是否完成
        TaskIsRunning = 3, // 任务是否进行中
        TaskEventIsFinish = 4, //任务事件是否完成
        TaskIsCommit = 5, //任务是否已提交
        ItemLimit = 6, //道具限制
        TeamCheck = 7, //队伍检查
        GuildLevelLimit = 8, //工会等级限制
        TreasureActived = 9, // 是否在挖宝
        ServerOpendDay = 10, //开服天数
        FinishLevel = 11, //通关个人秘境指定层数
        RiskLevelLimit = 12, //冒险等级
        PlayerJob = 13, //玩家职业
        CheckBuff = 14, //检查玩家身上具有某个buff
        TeamLevelLimt = 15, //队伍等级检查
        GuidExisted = 16, //是否存在公会
        WantedAccepted = 17, //通缉任务是否接取
        EquipIntensify = 18, //装备强化
        PartnerCount = 19, //伙伴数量
        AmuletPolishing = 20, //护符打磨
        EquipQualityCount = 21, //穿戴了X个品质为Y的装备 参数1:品质 参数2:数量
        JobSkillLevelCount = 22, // 指定技能升级到x级  参数1:技能id 参数2：技能等级
        PersonDailyPass = 23, // 通关指定单人日常本 参数1:组id,参数2:难度
        TeamDailyPass = 24, // // 通关指定组队日常本 参数1:组队本系统表主id
        AppointSystemOpen = 25, // // 指定系统解锁 参数1:系统id
        CheckBlackBoard = 26,//参数1-黑板key：string 参数2-黑板值：int32
        PowerModuleCheck = 27,//战力模块检查  参数1 战力模块（0总，1装备，2伙伴，3角色，4技能） 参数2 战力下限（需求的战力数字）参数3 历史还是当前（0历史，1当前）
        SkillDotTotal = 28, //角色累计获得技能点数 参数1 获得技能点数的数量
        AdventureCheck = 29, //养成/冒险旅程阶段检 参数1 阶段ID
    }

    public enum TrrigerType
    {
        TimerTrriger = 1,
        PositionTrriger = 2,
        PropertyTrriger = 3,
    }

    public enum CompareEnum
    {
        Equal = 0,
        Greater = 1,
        Less = 2,
        GreaterEqual = 3,
        LessEqual = 4,
        UnEqual = 5,
    }

    public enum EffectTargetType
    {
        NPC = 0, //NPC
        InterAction = 1, // 交互物
        Area = 2, //区域
        FinishTarget = 3, //任务目标完成时
    }

    public enum PlayEnums
    {
        None = 0, //无
        PersonSecret = 1, //开启个人秘境
        TeamDailyCopy = 2, //开启组队日常本
        TeamWanted = 3, //开启通缉任务
        DailyCopy = 4, //开启单人日常本
        PersonalTower = 5, //开启个人爬塔
        Arena = 6, //开启异步竞技场
        BattleField = 7, //开启战场活动
        PlayModeGVE = 8, //开启梦境入侵
        GuildDonate = 9,//工会捐献
        TreasureDig = 10,//挖宝
        WildBoss = 11,//野外boss
        RingTask = 12,  //环任务
        GuildBoss = 13, //工会boss
        PartyReward = 14, //派对时刻领取挂机奖励（20次记一次完成）
        ResidentGameplay = 15, //常驻玩法
        LimitedTimeGameplay = 16, //限时玩法
    }
    public enum ItemType {
        Item_Type_Prop = 0,//属性类道具
        Item_Type_Normal = 1,
        Item_Type_Equip = 2,
        Item_Type_Use = 3,
        Item_Type_ParEquip = 4,
        Item_Type_Amulet = 5,
        Item_Type_Heraldry = 6,//纹章
        Item_Type_LifeGatherItem_10=10,//生活技能 相关道具
        Item_Type_Treasure = 20,//藏宝图
        Item_Type_Treasure_chip = 21,//藏宝图碎片
        Item_Type_Bos = 22,// 宝箱类型占位
        Item_Type_Virtual = 101,//资产数值类道具
    }
    public enum SkillType
    {
        Anything = 0,//任意
        BattleSeat_NormalAttack = 1,//普攻
        BattleSeat_Common = 2,//常规
        Battleseat_Assist = 3,//辅助
        Battleseat_Ultimate = 4,//大招
        BattleSeat_Rush = 5,//冲刺
    }
    public enum MonsterSourceEnum
    {
        Scene = 0, //场景
        Copy = 1, //副本
    }
    public enum SystemInteract
    {
        None = 0, //无
        EquipEnhance = 1, //装备强化
        EquipRefine = 2, //装备洗练
        PartnerLevelUp = 3, //伙伴升级
        PartnerChristen = 4, //伙伴洗练
        AmuletPolish = 5, //护符打磨
        EquipRecast = 6, //装备重铸
        ShopBuyGoods = 7, //购买商品
        TweeterLevelUp = 8, //鸣器升级
        AmuletExtract = 9, //护符萃取
        PartnerEquipUp = 10,//伙伴装备进阶
        HerldryLevelUp = 11,//纹章升级
        DailySignIn = 12, //签到次数 
        DrawCard = 13, //抽卡 
        PartnerStar = 14, //伙伴升星
        AddFriends = 15, //添加X位好友
        WearPartnerEquipment = 16, //穿戴N件伙伴装备
        TradingBankCostCoin = 17,//交易行累计消耗X金币
        BankRedeemBoundDiamonds = 18,//银行累计兑换绑钻X
        DreamRealmReward = 19,//领取一次梦之境奖励
        ProductAndMake = 20,//生产制造
        Shop = 21,//商城购买商品（次）
        TradeBuyGoods = 22,//交易行购买商品
    }
    public static class TaskEnumUtils
    {
        static public IEnumerable _taskclassifytypes = new ValueDropdownList<TaskClassifyType>()
        {
            { "主线", TaskClassifyType.MainLine },
            { "支线", TaskClassifyType.SubbranchLine },
            { "传记", TaskClassifyType.Biography },
            { "日常", TaskClassifyType.Play },
            { "引导", TaskClassifyType.Guide },
            { "其他", TaskClassifyType.Other },
            { "挑战", TaskClassifyType.Challenge },
        };


        static public IEnumerable _taskperiods = new ValueDropdownList<TaskPeriod>()
        {
            { "永久", TaskPeriod.TaskPeriodForever },
            { "每日", TaskPeriod.TaskPeriodDay },
            { "每周", TaskPeriod.TaskPeriodWeek },
        };


        static public IEnumerable _conditiontypetypes = new ValueDropdownList<ConditionType>()
        {
            { "等级限制", ConditionType.LevelLimit },
            { "玩家职业", ConditionType.PlayerJob },
            { "开服天数", ConditionType.ServerOpendDay },
            { "任务/任务是否完成", ConditionType.TaskIsFinish },
            { "任务/任务是否进行中", ConditionType.TaskIsRunning },
            { "任务/任务事件是否完成", ConditionType.TaskEventIsFinish },
            { "任务/任务是否已提交", ConditionType.TaskIsCommit },
            { "道具限制", ConditionType.ItemLimit },
            { "组队/是否在队伍中", ConditionType.TeamCheck },
            { "组队/队伍等级限制", ConditionType.TeamLevelLimt },
            { "公会/公会等级检查", ConditionType.GuildLevelLimit },
            { "玩法/是否在挖宝", ConditionType.TreasureActived },
            { "玩法/通关拟态梦境层数", ConditionType.FinishLevel },
            { "玩法/侵蚀裂隙任务是否接取", ConditionType.WantedAccepted },
            { "玩法/通关指定时序残响关卡", ConditionType.PersonDailyPass },
            { "玩法/通关指定蚀梦之影关卡", ConditionType.TeamDailyPass },
            { "养成/冒险等级", ConditionType.RiskLevelLimit },
            { "养成/装备强化", ConditionType.EquipIntensify },
            { "养成/伙伴等级", ConditionType.PartnerCount },
            { "养成/打磨完成纹章数量", ConditionType.AmuletPolishing },
            { "养成/穿戴了X个品质为Y的装备", ConditionType.EquipQualityCount },
            { "养成/职业技能总等级", ConditionType.JobSkillLevelCount },
            { "养成/战力信标检查", ConditionType.PowerModuleCheck },
            { "养成/累计获得技能点数", ConditionType.SkillDotTotal },
            { "养成/冒险旅程阶段检", ConditionType.AdventureCheck },
            { "检查身上Buff", ConditionType.CheckBuff },
            { "指定系统解锁", ConditionType.AppointSystemOpen },
            { "副本黑板", ConditionType.CheckBlackBoard },
        };

        static public IEnumerable _tasktypes = new ValueDropdownList<TaskType>()
        {
            { "与任务对象产生交互", TaskType.Dialogue },
            { "完成副本", TaskType.Level },
            { "杀怪/杀怪", TaskType.KillMonster },
            { "杀怪/击杀怪物来源", TaskType.SkillMonsterSource },
            { "杀怪/杀怪获得道具", TaskType.KillMonsterGetItem },
            { "场景/进入地图", TaskType.EnterMap },
            { "交互/交互", TaskType.InterAction },
            { "交互/交互获得道具", TaskType.InterGetItem },  
            { "道具/收集道具", TaskType.Collect },
            { "道具/背包中有指定道具", TaskType.BagItems },
            { "道具/使用道具", TaskType.UseItem },
            { "道具/消耗道具", TaskType.GuideUseItem },
            { "使用技能", TaskType.UseSkill },
            { "完成指定任务", TaskType.FinishTask },
            { "养成/玩家等级", TaskType.PlayerLevel },
            { "养成/玩家冒险等级", TaskType.AdventureLevel },
            { "养成/玩家技能升级", TaskType.PlayerSkillLevelUp },
            { "养成/X技能X级", TaskType.SkillLevel },
            { "养成/打磨完成X个Y等阶Z品质纹章", TaskType.TalismanQualityCount },
            { "养成/获取N个X级星次石", TaskType.HerldryCount },
            { "养成/累计获取N件X类型道具", TaskType.PartnerEquipCount },
            { "养成/获取z级以上品质以上装备X件", TaskType.GetEquipQuality },
            { "养成/穿戴z级以上品质以上装备X件", TaskType.WearingEquipQuality },
            { "养成/X商店累计消耗货币y数量N", TaskType.ShopSpendCurrency },
            { "养成/激活X个z级战技", TaskType.ActivationWar },
            { "养成/所有装备强化至XXXX级", TaskType.TotalEquipIntensify },
            { "养成/穿戴指定ID纹章", TaskType.WearingQualityAmulet },
            { "养成/装配指定ID鸣器", TaskType.WearingTweeter },
            { "养成/总强化等级X", TaskType.TotalIntensify },
            { "养成/N件X级装备完美度达到Y", TaskType.EquipmentTrainingLevel },
            { "养成/点亮天赋点X个", TaskType.TalentCount },
            { "养成/某位置(1，2，3代表出战，助战，任意位置)伙伴任意资质超过X", TaskType.PartnerQualification },
            { "养成/X模块信标评分达到(X需要支持总模块)", TaskType.ModuleRating },
            { "养成/任意呜器达到x级", TaskType.BuzzerLevel },
            { "养成/伙伴上阵", TaskType.GuidePartnerInBattle },
            { "养成/N个X级以上伙伴", TaskType.PartnerCount },
            { "养成/指定部位强化等级达到X级", TaskType.PartsIntensifyLevel },
            { "养成/切换X槽位技能天赋", TaskType.ChangeSkillTalent },
            { "养成/指定位置装配数量X伙伴", TaskType.PosEquipPartner },
            { "养成/Z类型纹章镶嵌X星次石", TaskType.EmblemSetStarStone },
            { "系统/系统交互", TaskType.SystemInteract },
            { "系统/今日登录", TaskType.LoginToday },
            { "系统/x商店购买道具x次", TaskType.ShopBuyGoods },
            { "系统/签到领奖", TaskType.SignInAward },
            { "系统/z频道发送r条消息", TaskType.ChannelMessages },
            { "系统/指定邮件标题领奖X次", TaskType.MailRewarded },
            { "系统/创建或加入公会", TaskType.CreateOrJoinGuild },
            { "玩法/玩法", TaskType.Plays },
            { "玩法/拟态梦境X层通关", TaskType.SecretAreaLevel },
            { "玩法/日常活跃度X", TaskType.Activitylevel },
            { "玩法/参与x阶侵蚀裂隙x次", TaskType.WantTaskCountStage },            
            { "玩法/完成蚀梦之影X副本ID", TaskType.ShadowofDreamErosionPass },
            { "玩法/空想之环到达X层", TaskType.PersonTowerLevel },
            { "玩法/通关X组Y难度时序残响", TaskType.PersonDailyPass },
        };

        static public IEnumerable _effecttypetypes = new ValueDropdownList<FunctionType>()
        {
            { "任务进度增加", FunctionType.TaskProcess },
            { "接取任务", FunctionType.ReciveTask },
            { "完成任务", FunctionType.FinishTask },
            { "等待", FunctionType.Wait },                  
            { "场景/进入副本", FunctionType.EntityLevel },
            { "场景/离开本地图（副本）", FunctionType.ExitCurMap },
            { "场景/同场景传送", FunctionType.SameSceneTranslate },
            { "场景/传送", FunctionType.Translate },
            { "演出/触发对话", FunctionType.Dialogue },
            { "演出/播放黑幕", FunctionType.PlayBlack },
            { "演出/播放Timeline", FunctionType.PlayTimeline },
            { "演出/播放章节效果", FunctionType.PlayPlot },
            { "演出/播放剧情图片", FunctionType.PlayImage },
            { "演出/激活场景虚拟相机", FunctionType.ActiveSceneCam },
            { "演出/屏幕震动", FunctionType.ScreenImpulse },
            { "演出/播放视频", FunctionType.PlayPv },
            { "演出/播放动画", FunctionType.PlayAnimation },
            { "演出/播放特效", FunctionType.PlayFx },
            { "演出/伙伴喊话", FunctionType.PartnerDialog },
            { "演出/切图过渡信息配置", FunctionType.ChanageSceneTransitionData },
            { "演出/假伙伴UI", FunctionType.FakePartnerUI },
            { "演出/转场过渡", FunctionType.Transition },   
            { "NPC/切换NPC状态机", FunctionType.ChangeNpcState },
            { "NPC/切换场景物件状态", FunctionType.ChangeSceneObjState },
            { "NPC/NPC移动", FunctionType.NpcMove },
            { "NPC/修改对象临时显隐", FunctionType.ModifyEntityTempVisibility },
            { "NPC/服务器修改对象的显隐", FunctionType.ServerModifyEntityVisible },
            { "主角/释放技能", FunctionType.ReleaseSkill },
            { "主角/添加Buff", FunctionType.AddBuff },
            { "主角/移除Buff", FunctionType.RemoveBuff },  
            { "主角/添加被动", FunctionType.AddPassiveSkill },
            { "主角/移除被动", FunctionType.PassiveRemove },
            { "事件/客户端发送事件", FunctionType.ClientSendModuleMsg },
            { "事件/发送自定义事件", FunctionType.SendCustomEvent },
            { "事件/设置服务器标记", FunctionType.ChanageHide },
            { "事件/大场景标记位控制", FunctionType.SceneControlFlag },
            { "事件/发送关卡开启事件", FunctionType.SendBattleOpenEvent },
            { "系统/打开NPC商店", FunctionType.OpenNpcShop },
            { "系统/藏宝图挖宝", FunctionType.TreasureFind },
            { "系统/冒险等级突破效果", FunctionType.RiskLevelBreak },
            { "系统/修改道具", FunctionType.UpItem },  
            { "系统/转职", FunctionType.TransJob },
            { "系统/系统引导", FunctionType.SystemGuide },
            { "系统/触发引导", FunctionType.TriggerGuide },
            { "系统/发起组队日常本挑战", FunctionType.TeamEctypeChallenge },
            { "系统/飘字显示", FunctionType.ShowMessage },
            { "副本黑板", FunctionType.CheckBlackBoard },
            { "系统弹窗", FunctionType.ShowMessageBox } ,
            { "结束引导", FunctionType.EndGuide } ,
            { "新手关显隐UI", FunctionType.XinSGShowUI } ,
            { "伙伴加入效果", FunctionType.PartnerJoin } ,
            { "伙伴离开效果", FunctionType.PartnerLevel } ,
            { "从玩家身上播放特效到固定点", FunctionType.PlayEffectFormPlayerToPoint },
            {"场景玩法开关控制", FunctionType.ScenePlaysControl },
            {"演出/进入副本弹窗", FunctionType.ShowEnterDungeonMenu },
            {"打开心灵修复小游戏", FunctionType.ShowMindRepair },
        };

        static public IEnumerable _e_findpath = new ValueDropdownList<E_FindPath>()
        {
            { "没有寻路", E_FindPath.None },
            { "场景编辑器配置的Spanwer位置", E_FindPath.Spawer },
            { "到达具体NPC位置", E_FindPath.NPC },
            { "到达具体交互物", E_FindPath.InterAction },
            { "到达具体区域位置", E_FindPath.Area },
            { "跳转",E_FindPath.JumpTo },
            { "引导",E_FindPath.DoGuide }
        };

        static public IEnumerable _e_findtype = new ValueDropdownList<E_FindType>()
        {
            { "寻路", E_FindType.FindPath },
            { "传送(暂不支持)", E_FindType.Fly },
        };


        static public Dictionary<ConditionType, Type> ConditionFactory = new Dictionary<ConditionType, Type>()
        {
            { ConditionType.LevelLimit, typeof(LevelCondition) },
            { ConditionType.TaskIsFinish, typeof(TaskFinishCondition) },
            { ConditionType.TaskIsRunning, typeof(TaskRunningCondition) },
            { ConditionType.TaskEventIsFinish, typeof(TaskEventFinishCondition) },
            { ConditionType.TaskIsCommit, typeof(TaskCommitCondition) },
            { ConditionType.ItemLimit, typeof(ItemLimitCondition) },
        };

        static public Dictionary<TaskType, Type> TaskTypeFactory = new Dictionary<TaskType, Type>()
        {
            { TaskType.Dialogue, typeof(TaskTypeDialogue) },
            { TaskType.Level, typeof(TaskTypeLevel) },
            { TaskType.KillMonster, typeof(TaskTypeKillMonster) },
            { TaskType.Collect, typeof(TaskTypeCollect) },
            { TaskType.UseItem, typeof(TaskTypeUseItem) },
            { TaskType.UseSkill, typeof(TaskTypeUseSkill) },
            { TaskType.Arrive, typeof(TaskTypeArrive) },
            { TaskType.Escort, typeof(TaskTypeEscort) },
            { TaskType.InterAction, typeof(TaskTypeInterAction) },
            { TaskType.Prologue, typeof(TaskTypePrologue) },
            { TaskType.Shady, typeof(TaskTypeShady) },
            { TaskType.KillMonsterGetItem, typeof(TaskTypeKillMonsterGetItem) },
            { TaskType.BagItems, typeof(TaskTypeCollect) },
            { TaskType.InterGetItem, typeof(TaskTypeInterGetItem) },
            { TaskType.GuideUseItem, typeof(TaskTypeGuideUseItem) },
            { TaskType.GuidePartnerInBattle, typeof(TaskTypeGuidePartnerInBattle) },
            { TaskType.Plays, typeof(TaskTypePlays) },
            { TaskType.PlayerLevel, typeof(TaskTypePlayerLevel) },
            { TaskType.AdventureLevel, typeof(TaskTypeAdventureLevel) },
            { TaskType.SystemInteract, typeof(TaskTypeSystemInteract) },
            { TaskType.LoginToday, typeof(TaskTypeLoginToday) },
            { TaskType.ShopBuyGoods, typeof(TaskTypeShopBuyGoods) },
            { TaskType.WantTaskCountStage, typeof(TaskTypeWantTaskCountStage)},
            { TaskType.PlayerSkillLevelUp, typeof(TaskTypePlayerSkillLevelUp) },
            { TaskType.SignInAward, typeof(TaskTypePlayerSignInAward) },
            { TaskType.SkillMonsterSource, typeof(TaskTypeSkillAppointMonster) },
            { TaskType.FinishTask, typeof(TaskTypeFinishTask) },
            { TaskType.TotalIntensify, typeof(TaskTotalIntensify) },
            { TaskType.PartnerCount, typeof(TaskPartnerCount) },
            { TaskType.SecretAreaLevel, typeof(TaskSecretAreaLevel) },
            { TaskType.SkillLevel, typeof(TaskSkillLevel) },
            { TaskType.ShadowofDreamErosionPass, typeof(TaskShadowofDreamErosionPass) },
            { TaskType.TalismanQualityCount, typeof(TaskTalismanQualityCount) },
            { TaskType.HerldryCount, typeof(TaskHerldryCount) },
            { TaskType.PersonTowerLevel, typeof(TaskPersonTowerLevel) },
            { TaskType.PartnerEquipCount, typeof(TaskPartnerEquipCount) },
            { TaskType.EquipmentTrainingLevel, typeof(TaskEquipmentTrainingLevel) },
            { TaskType.TalentCount, typeof(TaskTalentCount) },
            { TaskType.PartnerQualification, typeof(TaskPartnerQualification) },
            { TaskType.ModuleRating, typeof(TaskModuleRating) },
            { TaskType.BuzzerLevel, typeof(TaskBuzzerLevel) },
            { TaskType.ChannelMessages, typeof(TaskChannelMessages) },
            { TaskType.GetEquipQuality, typeof(TaskGetEquipQuality) },
            { TaskType.WearingEquipQuality, typeof(TaskWearingEquipQuality) },
            { TaskType.ShopSpendCurrency, typeof(TaskShopSpendCurrency) },
            { TaskType.ActivationWar, typeof(TaskActivationWar) },
            { TaskType.MailRewarded, typeof(TaskMailRewarded) },
            { TaskType.TotalEquipIntensify, typeof(TaskTotalEquipIntensify) },
            { TaskType.WearingQualityAmulet, typeof(TaskWearingQualityAmulet) },
            { TaskType.WearingTweeter, typeof(TaskWearingTweeter) },
            { TaskType.Activitylevel, typeof(TaskActivitylevel) },
            { TaskType.EnterMap, typeof(TaskEnterMap) },
            { TaskType.CreateOrJoinGuild, typeof(CreateOrJoinGuild) },
            { TaskType.PartsIntensifyLevel, typeof(PartsIntensifyLevel) },
            { TaskType.ChangeSkillTalent, typeof(ChangeSkillTalent) },
            { TaskType.PosEquipPartner, typeof(PosEquipPartner) },
            { TaskType.EmblemSetStarStone, typeof(EmblemSetStarStone) },
            { TaskType.PersonDailyPass, typeof(PersonDailyPass) },

        };
        static public Dictionary<FunctionType, Type>EffectTypeFactory = new Dictionary<FunctionType, Type>()
        {
           { FunctionType.ReleaseSkill, typeof(EffectReleaseSkill) },
            { FunctionType.AddBuff, typeof(EffectAddBuff) },
            { FunctionType.RemoveBuff, typeof(EffectRemoveBuff) },
            { FunctionType.ReciveTask, typeof(EffectReciveTask) }, // 注意：Recive可能是Receive的拼写错误  
            { FunctionType.FinishTask, typeof(EffectFinishTask) },
            { FunctionType.EntityLevel, typeof(EffectEntityLevel) },
            { FunctionType.Dialogue, typeof(EffectDialogue) },
            { FunctionType.PlayBlack, typeof(EffectPlayBlack) },
            { FunctionType.TaskProcess, typeof(EffectTaskProcess) },
            { FunctionType.PlayTimeline, typeof(EffectPlayTimeline) },
            { FunctionType.PlayPlot, typeof(EffectPlayPlot) },
            { FunctionType.PlayImage, typeof(EffectPlayImage) },
            { FunctionType.OpenNpcShop, typeof(EffectOpenNpcShop) },
            { FunctionType.SendCustomEvent, typeof(EffectSendCustomEvent) },
            { FunctionType.SceneControlFlag, typeof(EffectSceneControlFlag) },
            { FunctionType.SameSceneTranslate, typeof(EffectSameSceneTranslate) },
            { FunctionType.ChangeNpcState, typeof(EffectChangeNpcState) },
            { FunctionType.ClientSendModuleMsg, typeof(EffectClientSendModuleMsg) },
            { FunctionType.TeamEctypeChallenge, typeof(EffectTeamEctypeChallenge) },
            { FunctionType.TreasureFind, typeof(EffectTreasureFind) },
            //{ FunctionType.SendBattleOpenEvent, typeof(EffectSendBattleOpenEvent) },
            { FunctionType.ActiveSceneCam, typeof(EffectActiveSceneCam) },
            { FunctionType.RiskLevelBreak, typeof(EffectRiskLevelBreak) },
            { FunctionType.UpItem, typeof(EffectUpItem) },
            { FunctionType.ModifyEntityTempVisibility, typeof(ModifyEntityTempVisibility) },
            { FunctionType.PlayAnimation, typeof(EffectPlayAnimation) },
            { FunctionType.PlayFx, typeof(EffectPlayFx) },
            { FunctionType.ChanageHide, typeof(EffectChanageHide) }, // 注意：这里假设了Chanage是Change的拼写错误  
            { FunctionType.AddPassiveSkill, typeof(EffectAddPassiveSkill) },
            { FunctionType.TransJob, typeof(EffectTransJob) },
            { FunctionType.TriggerGuide, typeof(EffectTriggerGuide) },
            { FunctionType.ShowMessage, typeof(EffectShowMessage) },
            { FunctionType.ChangeSceneObjState, typeof(EffectChangeSceneObjState) },
            { FunctionType.NpcMove, typeof(EffectNpcMove) },
            { FunctionType.Translate, typeof(EffectTranslate) }, // 注意：这里假设Translate指的是移动或转换位置  
            { FunctionType.Wait, typeof(EffectWait) }, // 可能没有具体的“效果”类型，只是一个等待操作  
            { FunctionType.PassiveRemove, typeof(EffectPassiveRemove) },
            { FunctionType.ScreenImpulse, typeof(EffectScreenImpulse) },
            { FunctionType.PlayPv, typeof(EffectPlayPv) }, // 假设Pv指的是某种特定的播放内容  
            { FunctionType.ExitCurMap, typeof(EffectExitCurMap) },
            { FunctionType.ServerModifyEntityVisible, typeof(EffectServerModifyEntityVisible) },
            { FunctionType.CheckBlackBoard, typeof(EffectCheckBlackBoard) }, // 假设BlackBoard指的是某种信息板或状态板  
            { FunctionType.SystemGuide, typeof(EffectSystemGuide) },
            { FunctionType.PartnerDialog, typeof(EffectPartnerDialog) },
            { FunctionType.ChanageSceneTransitionData, typeof(EffectChanageSceneTransitionData) }, // 假设Chanage是Change的拼写错误  
            { FunctionType.FakePartnerUI, typeof(EffectFakePartnerUI) },
            { FunctionType.Transition, typeof(EffectTransition) },
            //{ FunctionType.XinSGShowUI, typeof(EffectXinSGShowUI) }, // 假设XinSG是某种特定UI的标识符  
            //{ FunctionType.PartnerJoin, typeof(EffectPartnerJoin) },
            //{ FunctionType.PartnerLevel, typeof(EffectPartnerLevel) }, // 假设这与提升或改变伙伴等级有关  
            { FunctionType.ShowMessageBox, typeof(EffectShowMessageBox) },
            { FunctionType.EndGuide, typeof(EffectEndGuide) },
            { FunctionType.PlayEffectFormPlayerToPoint, typeof(EffectPlayEffectFormPlayerToPoint) }, // 修正了Form为From 
            {FunctionType.ScenePlaysControl, typeof(EffectScenePlaysControl) },
             {FunctionType.ShowEnterDungeonMenu, typeof(EffectShowEnterDungeonMenu) },
             {FunctionType.ShowMindRepair, typeof(EffectShowMindRepair) },

        };
        static public List<FunctionType> ClientTimeOutList = new List<FunctionType>()
        {
            { FunctionType.PlayAnimation},
            { FunctionType.Dialogue},
            { FunctionType.PlayBlack},
            { FunctionType.PlayTimeline},
            { FunctionType.PlayPlot},
            { FunctionType.PlayImage},
            { FunctionType.OpenNpcShop},
            { FunctionType.ScreenImpulse},
            { FunctionType.PlayPv},
            { FunctionType.PartnerDialog},
            { FunctionType.ShowMessage},
            { FunctionType.Wait},
            { FunctionType.SystemGuide},
            { FunctionType.ClientSendModuleMsg},
            { FunctionType.ChanageSceneTransitionData},
            { FunctionType.Transition},
            { FunctionType.ChangeNpcState},
            { FunctionType.ModifyEntityTempVisibility},
            { FunctionType.ChangeSceneObjState},
            { FunctionType.NpcMove},
            { FunctionType.PlayFx},
            { FunctionType.FakePartnerUI},
            { FunctionType.XinSGShowUI},
            { FunctionType.PartnerJoin},
            { FunctionType.PartnerLevel},
            { FunctionType.ShowMessageBox},
            { FunctionType.EndGuide},
            { FunctionType.PlayEffectFormPlayerToPoint},
            {FunctionType.ScenePlaysControl },
            {FunctionType.ActiveSceneCam },

        };
        static public IEnumerable _trrigertype = new ValueDropdownList<TrrigerType>()
        {
            { "定时触发器", TrrigerType.TimerTrriger },
            { "位置触发器", TrrigerType.PositionTrriger },
            { "属性触发器", TrrigerType.PropertyTrriger },
        };

        static public IEnumerable _compareenum = new ValueDropdownList<CompareEnum>()
        {
            { "==", CompareEnum.Equal },
            { ">", CompareEnum.Greater },
            { "<", CompareEnum.Less },
            { ">=", CompareEnum.GreaterEqual },
            { "<=", CompareEnum.LessEqual },
            { "!=", CompareEnum.UnEqual },
        };

        // 
        static public IEnumerable _tasktag = new List<string>()
        {
            "",
            "经验",
            "装备",
            "金钱",
            "帮贡"
        };

        static public IEnumerable _effecttargettype = new ValueDropdownList<EffectTargetType>()
        {
            { "NPC", EffectTargetType.NPC },
            { "交互物", EffectTargetType.InterAction },
            { "区域", EffectTargetType.Area },
            { "任务目标完成", EffectTargetType.FinishTarget },
        };

        static public IEnumerable _taskentitytypes = new ValueDropdownList<TaskEntityType>()
        {
            { "无", TaskEntityType.None },
            { "Npc", TaskEntityType.Npc },
            { "物件", TaskEntityType.Obj },
            { "怪物", TaskEntityType.Mon },
        };

        static public IEnumerable _taskentityhidetypes = new ValueDropdownList<TaskEntityHideType>()
        {
            { "无", TaskEntityHideType.None },
            { "不能看见别人", TaskEntityHideType.NotLookOtherPlayer },
            { "别人看不见我", TaskEntityHideType.OtherPlayerNotLookSelf },
            { "通知服务器新手关2完成", TaskEntityHideType.FinishGuideCopy2 },
            { "通知服务器新手关1完成", TaskEntityHideType.FinishGuideCopy1 },
            { "通知服务器新手关3完成", TaskEntityHideType.FinishGuideCopy3 },
            { "刷新环任务", TaskEntityHideType.RefreshRingTask },
        };

        static public IEnumerable _taskpassiveskillsourcetypes = new ValueDropdownList<TaskPassiveSkillSourceType>()
        {
            { "默认", TaskPassiveSkillSourceType.Default },
            { "秘境", TaskPassiveSkillSourceType.SecretArea },
        };

        static public IEnumerable _taskplayenums = new ValueDropdownList<PlayEnums>()
        {
            { "无", PlayEnums.None },
            { "拟态梦境", PlayEnums.PersonSecret },
            { "蚀梦之影", PlayEnums.TeamDailyCopy },
            { "侵蚀裂隙", PlayEnums.TeamWanted },
            { "时序残响", PlayEnums.DailyCopy },
            { "空想之环", PlayEnums.PersonalTower },
            { "胜者为王", PlayEnums.Arena },
            { "风暴对抗", PlayEnums.BattleField },
            { "梦境入侵获得奖励", PlayEnums.PlayModeGVE },
            { "协会募集", PlayEnums.GuildDonate },
            { "藏宝图", PlayEnums.TreasureDig },
            { "协会讨伐", PlayEnums.WildBoss },
            { "研究手册", PlayEnums.RingTask },
            { "无畏之战", PlayEnums.GuildBoss },
            { "派对时刻领取挂机奖励（20次记一次完成）", PlayEnums.PartyReward },
            { "常驻玩法", PlayEnums.ResidentGameplay },
            { "限时玩法", PlayEnums.LimitedTimeGameplay },
        };
        static public IEnumerable _itemtypeenums = new ValueDropdownList<ItemType>()
        {
            { "属性类道具", ItemType.Item_Type_Prop },
            { "常规道具", ItemType.Item_Type_Normal },
            { "装备", ItemType.Item_Type_Equip },
            { "使用类道具", ItemType.Item_Type_Use },
            { "伙伴装备", ItemType.Item_Type_ParEquip },
            { "纹章", ItemType.Item_Type_Amulet },
            { "星次石", ItemType.Item_Type_Heraldry },
            { "生活技能相关道具", ItemType.Item_Type_LifeGatherItem_10 },
            { "藏宝图", ItemType.Item_Type_Treasure },
            { "藏宝图碎片", ItemType.Item_Type_Treasure_chip },
            { "宝箱类型占位", ItemType.Item_Type_Bos },
            { "资产数值类道具", ItemType.Item_Type_Virtual },
        };
        static public IEnumerable _skilltypeenums = new ValueDropdownList<SkillType>()
        {
            { "任意", SkillType.Anything },
            { "普攻", SkillType.BattleSeat_NormalAttack },
            { "常规", SkillType.BattleSeat_Common },
            { "辅助", SkillType.Battleseat_Assist },
            { "大招", SkillType.Battleseat_Ultimate },
            { "冲刺", SkillType.BattleSeat_Rush },
        };
        
        static public IEnumerable _monstersourceenums = new ValueDropdownList<MonsterSourceEnum>()
        {
            { "场景", MonsterSourceEnum.Scene },
            { "副本", MonsterSourceEnum.Copy },
        };
        static public IEnumerable _systemInteractenums = new ValueDropdownList<SystemInteract>()
        {
            { "无", SystemInteract.None },
            { "装备强化", SystemInteract.EquipEnhance },
            { "装备洗练", SystemInteract.EquipRefine },
            { "伙伴升级", SystemInteract.PartnerLevelUp },
            { "伙伴洗练", SystemInteract.PartnerChristen },
            { "护符打磨", SystemInteract.AmuletPolish },
            { "装备重铸", SystemInteract.EquipRecast },
            { "购买商品", SystemInteract.ShopBuyGoods },
            { "鸣器升级", SystemInteract.TweeterLevelUp },
            { "护符萃取", SystemInteract.AmuletExtract },
            { "伙伴装备进阶", SystemInteract.PartnerEquipUp },
            { "星次石合成", SystemInteract.HerldryLevelUp },
            { "每日签到", SystemInteract.DailySignIn },
            { "抽卡", SystemInteract.DrawCard },
            { "伙伴升星", SystemInteract.PartnerStar },
            { "添加X位好友", SystemInteract.AddFriends },
            { "穿戴N件伙伴装备", SystemInteract.WearPartnerEquipment },
            { "交易行累计消耗X金币", SystemInteract.TradingBankCostCoin },
            { "银行累计兑换绑钻X", SystemInteract.BankRedeemBoundDiamonds },
            { "领取一次梦之境奖励", SystemInteract.DreamRealmReward },
            { "生产制造", SystemInteract.ProductAndMake },
            { "商城购买商品（次）", SystemInteract.Shop },
            { "交易行购买商品", SystemInteract.TradeBuyGoods },
        };

        static public IEnumerable _movetypes = new ValueDropdownList<MoveType>()
        {
            { "Spawner", MoveType.Spawner },
            { "路径", MoveType.Path },
        };
        static public IEnumerable e_systemopentypes = new ValueDropdownList<SystemOpenType>()
      {
        { "不显示", SystemOpenType.None },
        { "解锁药品补给", SystemOpenType.Potion },
        { "解锁玩家技能1", SystemOpenType.SkillOne },
        { "解锁玩家技能2", SystemOpenType.SkillTwo },
        { "开启伙伴-详情-阵容-出战", SystemOpenType.Partner_Common },
        { "解锁自动战斗", SystemOpenType.AutoFight },
        { "解锁玩家技能3", SystemOpenType.SkillThree },
        { "解锁玩家技能4", SystemOpenType.SkillFour },
        { "开启装备-强化", SystemOpenType.Equip_Common },
        { "开启首充", SystemOpenType.FirstTopUp },
        { "开启交易及交易行", SystemOpenType.Trade },
        { "开启商城功能及通用商店", SystemOpenType.Shop_Common },
        { "开启聊天功能及聊天框", SystemOpenType.Chat },
        { "开启好友功能", SystemOpenType.Friend },
        { "开启邮件功能", SystemOpenType.Mail },
        { "开启组队功能", SystemOpenType.TeamFind },
        { "开启玩法-日常", SystemOpenType.Playmode_Daily },
        { "开启冒险等级", SystemOpenType.AdventureLV },
        { "开启活动", SystemOpenType.Activity },
        { "开启抽卡", SystemOpenType.Gacha },
        { "开启伙伴-阵容-助战", SystemOpenType.Partner_Assist },
        { "开启伙伴-进阶", SystemOpenType.Partner_StarUP },
        { "开启公会", SystemOpenType.Guild },
        { "开启玩法-周常", SystemOpenType.Playmode_Weekly },
        { "开启拍卖行", SystemOpenType.Auction },
        { "开启被动天赋", SystemOpenType.ATTTree },
        { "开启伙伴-装备", SystemOpenType.Partner_Equip },
        { "开启装备-精炼", SystemOpenType.Equip_Refine },
        { "开启装备-重构", SystemOpenType.Equip_Reconst },
        { "开启护符-装备-打磨", SystemOpenType.Amulet_Common },
        { "开启纹章", SystemOpenType.Emblem },
        { "开启护符-萃取", SystemOpenType.Amulet_Extract },
        { "解锁奥义技能", SystemOpenType.Ult },
        { "开启拟态梦境", SystemOpenType.PersonSecret },
        { "开启蚀梦之影", SystemOpenType.TeamDailyCopy },
        { "开启侵蚀裂隙", SystemOpenType.TeamWanted },
        { "开启时序残响", SystemOpenType.DailyCopy },
        { "开启空想之环", SystemOpenType.PersonalTower },
        { "开启胜者为王", SystemOpenType.Arena },
        { "开启风暴对抗", SystemOpenType.BattleField },
        { "开启梦境入侵", SystemOpenType.PlayModeGVE },
        { "开启鸣器", SystemOpenType.Etch },
        { "开启藏宝图", SystemOpenType.Treasure },
        { "开启无畏之战", SystemOpenType.GuildNoonActiv },
        { "开启研究手册", SystemOpenType.RingTask },
        { "开启协会讨伐", SystemOpenType.FieldBoss },
        { "转职", SystemOpenType.TransferJob },
        { "开启生活技能", SystemOpenType.LifeSkill },
        { "开启协会募集", SystemOpenType.Playmode_GuildDonate },
        { "解锁目标选择", SystemOpenType.TargetSelect },
        { "开启技能", SystemOpenType.Skill },
        { "开启月卡", SystemOpenType.Activ_MonthCard },
        { "开启冒险企划", SystemOpenType.Activ_BattlePass },
        { "开启签到", SystemOpenType.Activ_SignIn },
        { "开启七日目标", SystemOpenType.Activ_SevenDay },
        { "开启道具合成", SystemOpenType.Item_Compose },
        { "开启梦之镜", SystemOpenType.WorldLine },
        { "开启玩法预告", SystemOpenType.FunctionPreview },
        { "在线奖励", SystemOpenType.TimeReward },
        { "伙伴目标", SystemOpenType.PartnerTarget },
        { "排行榜", SystemOpenType.Rank },
        { "战力", SystemOpenType.Power },
        { "日常活跃度", SystemOpenType.DailyPlayActiv },
        { "派对时刻", SystemOpenType.PartyTime },
        { "跑马灯", SystemOpenType.Marquee },
        { "技能天赋", SystemOpenType.SkillTalent },
        { "冒险旅程", SystemOpenType.BeginnerTarget },
        { "开启研究手册-正式指引", SystemOpenType.RingTask_Guidance },
        { "开启伙伴-资质", SystemOpenType.Partner_Qualification },
        { "开启伙伴-升级", SystemOpenType.Partner_LevelUp },
    };

#if UNITY_EDITOR
        public static void Init()
        {
            var serverJsonDir = PlayerPrefs.GetString("serverJsonDir", Application.dataPath);
            //加载外部配置
            MapEditor.EditorConfigUtils.Init();
            _SceneList.Clear();
            _maplist.Clear();
            SystemJumps.Clear();
            var mapli = MapEditor.EditorConfigUtils.SceneCfgs.Scenes;
            {
                foreach (var mapmd in mapli)
                {
                    _maplist.Add($"{mapmd.Key} {mapmd.Value.MapName}", mapmd.Key);
                    var scenecfg = new SceneCfg(mapmd.Value);
                    scenecfg.Init(serverJsonDir);
                    _SceneList.Add(mapmd.Key, scenecfg);
                }
            }
            string path = "Assets/Res/Config/ExcelBytes/SystemJump.bytes";
           var jsonAsset =UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (jsonAsset != null)
            {
                var config = MessagePack.MessagePackSerializer.Deserialize<SystemJumpData>(jsonAsset.bytes);
                if (config != null)
                {
                    foreach (var item in config.StaticSystemJumpDatas)
                    {
                        SystemJumps.Add($"{item.Key}.{item.Value.Comment}", item.Key);
                    }
                }
            }
        }

        public static void Destroy()
        {
            _maplist.Clear();
            _SceneList.Clear();
            SystemJumps.Clear();
        }

        public static ValueDropdownList<int> _maplist = new ValueDropdownList<int>();

        public static Dictionary<int, SceneCfg> _SceneList = new Dictionary<int, SceneCfg>();
        
        
        public static ValueDropdownList<int> SystemJumps = new ValueDropdownList<int>();
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// 地图配置
    /// </summary>
    public class SceneCfg
    {
        public SceneCfg(MapEditor.SceneData data)
        {
            this.data = data;
        }

        public SceneCfg(MapEditor.LevelData data)
        {
            this.data = new MapEditor.SceneData(data.MapID, data.MapName, data.SceneName);
        }

        
        public MapEditor.SceneData data;
        private GameObject mRootObj;
        private Transform mArea;
        private Transform mMine;
        private Transform mSpawner;
        private Transform mNpc;
        private Transform mMonster;
        public ValueDropdownList<long> _arealist = new ValueDropdownList<long>();
        public ValueDropdownList<long> _interlist = new ValueDropdownList<long>();
        public ValueDropdownList<long> _spawner = new ValueDropdownList<long>();
        public ValueDropdownList<long> _npclist = new ValueDropdownList<long>();
        public ValueDropdownList<long> _monster = new ValueDropdownList<long>();

        private static string SaveDir = "Assets/DevTools/MapEditor/Prefabs/";

        private SceneJsonData CreateMapObject(string path)
        {
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(text);
                return json;
            }

            return null;
        }

        public void Init(string serverJsonDir)
        {
            string path = $"{serverJsonDir}/{data.MapID}/data.json";
            var mapJson = CreateMapObject(path);
            if (mapJson != null)
            {
                _arealist.Clear();
                if (mapJson.Areas != null && mapJson.Areas.Count > 0)
                {
                    foreach (var item in mapJson.Areas)
                    {
                        _arealist.Add($"{item.Value.AreaName} {item.Value.AreaID}", item.Value.AreaID);
                    }
                }

                _npclist.Clear();
                foreach (var item in mapJson.Npcs)
                {
                    if (MapEditor.EditorConfigUtils.NpcCfgs.Npcs.TryGetValue(item.Value.NpcID, out var nPCData))
                    {
                        _npclist.Add($"{item.Value.NpcID}_{nPCData.Name}", item.Value.NpcID);
                    }
                }

                _interlist.Clear();
                foreach (var item in mapJson.Mines)
                {
                    if (MapEditor.EditorConfigUtils.MineCfgs.Mines.TryGetValue(item.Value.MineID, out var cfgv))
                    {
                        _interlist.Add($"{item.Value.MineID} {cfgv.ObjectName}", item.Value.MineID);
                    }
                }

                _spawner.Clear();
                foreach (var item in mapJson.Spawners)
                {
                    _spawner.Add($"{item.Value.SpawnerID}  {item.Value.Desc}", item.Value.SpawnerID);
                }


                _monster.Clear();
                foreach (var item in mapJson.Monsters)
                {
                    _monster.Add($"{item.Value.MonsterID} {item.Value.Desc}", item.Value.MonsterID);
                }
            }
        }
    }
#endif
}