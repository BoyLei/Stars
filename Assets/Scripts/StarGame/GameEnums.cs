using Sirenix.OdinInspector;
using System;
using XLua;

/// <summary>
/// 首先放在Service脚本，其次是单例子（mono和其他单例），；如果能确定一定被某个系统用就放在对应Module，如不能确定就放GameEnums
/// </summary>
namespace StarProjectDef
{

    #region 表现相关

    //在下拉菜单序列化
    //程序维护，他们选择，Ta新作一个效果以后我们维护一下
    [System.Serializable]
    public enum E_ShaderShowType
    {
        [LabelText("溶解")]
        _Clip,//定义两个是防止有两个效果在一个人身上配置但是重复
        [LabelText("Alpha渐变")]
        _Alpha //同时_分割符前面有用到当成key
    }

    public enum LoadingState
    {
        GameUpdate,
        Common

    }
    public enum E_OutlineEntityType : byte
    {
        StaticItem,


        DynamicItem,//交互物件


        MainPlayer,

        Hero,
        Monster,
        Summon,
        NPC,

    }


    public enum E_MusicTriggerType
    {
        State,//业务层的临时状态：具体表现是，出去后【如果是6秒内，战斗音乐也会恢复，但是安全区会快速切换：上层，临时，强制就是state】
        Event
    }

    //OverLayType情况下获取你的类型获sortinglayer大小进行唯一组进行排序
    //头顶信息就是一组通用4000
    public enum E_OverLayTypeOrderBase
    {
        SceneMessage = 2000,
        ModelMessage = 3000,
        FXBackest = 3100/*~3200*/,
        Pendant = 4000,

        //stateMessage = 5000,

        //HealthMsg = 6000,


        MainPlayerPendant = 9999,
        //SpecSelectPendant = 10000
        SpecSelectPendant = 10000,

        // doto:伤害飘字新规则
        // ****高磊说: 改成UI的伤害飘字，离相机最近的一定渲染到了最前面，就算离相机远的位置后出来了，也有可能会看不见
        // ****之前是后出来的一定会压先出来的
        //1伤害飘字最高优先级，2目前包含公式的意义：飘字也有Z轴关系大组；3，需代码控制同z轴的后出来的在后面
        //每次排序需依赖，第一次循环就是先按照orderLayer先排序：嵌套循环（谁是后产生放在后面）
        /*      2DamageRoot 创建一个中间节点名字叫 sortingOrder值，然后吧新创建的伤害丢在这个这个节点的下，然后设置最后一个
         *      1首先看damageRoot里面有  就放在最后一个，没有就创建，并且名字是这个orderlayer
         *      //不用删除这个节点，子节点也有池--- 也不必处理刷新，位置不会动，
         *      //设计现象：不会出现节点换父节点（10101），因为产生伤害就放在对应位置上，不跟跟着走
                //orderLayer,index
        都是整数，场景200*200，而且结果是相机距离超过200看不到了，算法还限制了
        设计选择题：忽略距离用后出现就很乱，分组的话后出现的可能被档上 ||近大远小是不需要的设计
        可以有特殊关注的， 比如+一个数值
        */
        BattleMesg = 10100,
        //SPECBattleMesg = BattleMesg + 1000,
        /*这个数量也挺大
        NextMessage = 11000,*/

    }
    #endregion




    #region 界面相关

    // UI异步加载状态
    public enum UIAsyncLoadState
    {
        None,
        InLoaded,       // 加载中
        Remove,         // 删除
        LoadFinish,     // 加载完成
    }

    [XLua.LuaCallCSharp]
    public enum ThreeDModelPos
    {
        None,
        Front,
        Back,
        Raw,
    }

    [CSharpCallLua]
    public enum ChangePageType
    {
        CommonForceChange,
        MeekFromFb
    }

    /// <summary>
    /// 分区块隐藏
    /// </summary>
    [LuaCallCSharp]
    public enum MainPageCommond
    {
        HideNone = 0,//全是0，什么都不处理，全是1全部隐藏
        //组1:这个位如果值是1（bin），那就是关
        //-----------------------------
        HideMoveWidgetCommond = 1 << 0,//1
        HideSkillWidgetCommond = 1 << 1,//2
        WidgetHide = (1 << 2) - 1,  //3
        //-----------------------------------

        //组2:这个位如果值是1（bin），那就是关
        //-------------------------------
        HidePageTopLeft = 1 << 2,   // 4
        HidePageTopCentre = 1 << 3, // 8
        HidePageTopRight = 1 << 4, // 16
        HidePageModdleLeft = 1 << 5, // 32
        HidePageModdleCentre = 1 << 6,  // 64
        HidePageModdleRight = 1 << 7,   // 128
        HidePageDownLeft = 1 << 8,  // 256
        HidePageDownCentre = 1 << 9,    // 512 
        HidePageDownRight = 1 << 10,    // 1024 --2044
        PageHide = HideBoth - WidgetHide, //--2044
        //-------------------------------------

        //--------特殊的
        ChatHide = HidePageModdleRight + WidgetHide,
        HudHideRight = HideSkillWidgetCommond + HidePageTopRight + HidePageModdleRight + HidePageDownRight,
        //--------特殊的

        HideBoth = (1 << 11) - 1,    // WidgetHide + PageHide
    }

    #region UI队列

    public enum UISeatType
    {
        None,
        Top,
        Down,
        Left,
        Right,
        Full,
    }

    public enum UIQueuePriorityType
    {
        None = 0,
        NotBlockingPlot = 10,
        PermanentTips = 20,
        GetTips = 30,
        ShortMsg = 40,
        PlayGmae = 50,
        Button = 60,
        Full = 70,
        BlockingPlot = 80,
    }

    #endregion



    #endregion

    #region 同步相关
    [XLua.LuaCallCSharp]

    public enum EnumAOIType
    {
        Int,
        Long,
        Uint,
        Ulong,
        Float,
        Bool,
        String,
        Proto,
    }



    [Flags]
    public enum E_BattleStateType
    {
        //[原子他们说的本身--不是状态，而是开关，是32把锁头：所以必须【源】所有是组合出现的都不用浪费位置]
        //[封禁组][状态组]是两个东西


        //2基于开关，空必须控一位，移动有开就有关闭，所以0和1都是default
        //3基于值的话，读表的时候要处理下位运算（服务器），之后的客户端
        //4本质来说0和1不能分开default和移动，因为是两个东西了，但是最后确定应该是不留default第0位也能用因为，这个设计不是状态，而是限定，限定是附加的没必要留

        //状态枚举（0位）不表示什么
        BattleStateType = 0,

        //-----------------------------------------------------------
        IDonnotKnowWhyServerNotUse = 1 << 0, //1如果按照指数来枚举的话，永远少一位，相当于0和1都会呗忽略//这不是标识状态，而是禁锢【状态有default】【禁锢所有位都要用】【【乱用，不分离的代表】】

        //Default分设计，max分情况算最后，设计要分离，位数要节省
        //-----------------------------------------------------------
        //禁移动
        BattleState_ForbidMove = 1 << 1,

        //禁转向
        BattleState_ForbidDir = 1 << 2,

        //禁伤害
        BattleState_ForbidHurt = 1 << 3,

        //禁治疗
        BattleState_ForbidCure = 1 << 4,

        //禁攻击 (只有普工)
        BattleState_ForbidAttack = 1 << 5,

        //禁技能 (除普工之外的所有技能)
        BattleState_ForbidSkill = 1 << 6,

        //禁位移
        BattleState_ForbidDisplacement = 1 << 7,

        //禁被选择器选中
        BattleState_ForbidSelect = 1 << 8,

        //禁打断技能
        BattleState_ForbidBreakSkill = 1 << 9,
        // 被黑洞牵引的状态
        BattleState_BlackHole = 1 << 10,

        //倒地状态，此状态下才可以回超必杀能量
        BattleState_FallDown = 1 << 11,

        //禁坐标同步，此状态用于位移效果运行时不进行坐标强拉的标记
        BattleState_ForbidPosSync = 1 << 12,

        //禁自己使用其他技能
        BattleState_ForbidUseOtherSkill = 1 << 13,

        //动作保持
        BattleState_ActionKeep = 1 << 14,


        //最后一个状态
        BattleStateOverlayMax = 1 << 15,

        // 怪物脱战状态 怪物需要脱战逃跑动作，
        BattleState_OutWarState = 1 << 28,
        /// <summary>
        /// BattleStateOverlayMax 之前的枚举状态为 共存的
        /// BattleStateOverlayMax 之后的枚举状态为 互斥的
        /// </summary>/
        //普攻状态,这里能配合声音播放，比如冰冻有声音实现则用不实现则不用，现在发现了吧，这里不是状态什么都往里面塞就有问题了吧！！！战斗内的状态和别的混合就这个问题
        BattleState_NormalState = 1 << 29,

        //战斗状态
        BattleState_BattleState = 1 << 30,

        //死亡:并非并集，还有复活，和禁止复活
        //BattleState_Dead = (1 << 15) - 1,
        //他是一个【状态】通过限定（1|2|4）来达成控制，有别于【限制】应该分离，但目前不应该删除因为不告诉就是不知道，不代表124变成000就是挂了，也不代表反过来就是活了
        //我们不做并集，并且这里要处理在状态中【乱用，不分离的代表】
        BattleState_Dead = 1 << 31, //我觉得这个位用的也不合理，死亡应该是 前面多个东西的并集，如果A|B|D（C是是否可以【加血复活】）

        //全封禁
        //BattleState_FobiddenUpAll = (1 << 32) - 1,
    }

    #endregion

    #region 怪物相关


    public enum E_MonsterState
    {
        None,
        Peace, // 非战斗状态
        Battle, // 战斗状态
        OutWar, // 脱战状态
        Getaway, // 逃离状态
    }

    #endregion

    /// <summary>
    /// 实体首次到空中到地面状态
    /// </summary>
    public enum E_EntityGState : byte
    {
        Default = 0,
        nonTouchGroundDrop = 1,
        readyTouchGroundOnceStop = 2
    }

    /// <summary>
    /// 实体类型
    /// 目前用标志位枚举，直接E_EntityType.Player.toString() 也是 "Player"，不影响目前的正常判断
    /// 唯一的不同是，可以用枚举标志位，来表示多种枚举集合
    /// </summary>
    [Flags]
    [XLua.LuaCallCSharp]
    public enum E_EntityType
    {
        None = 0,
        RoomSpace = 1,
        Player = 2,//主角包含自己和他人
        Npc = 4,
        Monster = 8,
        BulletEntity = 16,
        Interact = 32,
        /// <summary> 召唤物类型 </summary>
        Summon = 64,
        Partner = 128,
        GVEBoss = 256,    // 跟怪物一样
        Robot = 512,

        /// <summary>
        /// 客户端自己定义的召唤物
        /// </summary>
        ClientSummon = 4096
    }


    // 实体关系类型
    public enum E_EntityRelationType
    {
        None,                           // - 异常
        MonsterNormal,                  // - 【怪物】普通
        MonsterElite,                   // - 【怪物】精英
        MonsterBoss,                    // - 【怪物】BOSS
        MonsterOtherFriendly,           // - 【怪物】其他友善的
        NPC,                            // - NPC
        PlayerMain,                     // - 【人】主角
        PlayerOtherFriendly,            // - 【人】其他友善的
        PlayerOtherHostility,           // - 【人】其他敌对的
        SummonMainPlayer,               // - 【召唤物】主角的
        SummonOtherFriendly,            // - 【召唤物】其他友善的
        SummonOtherHostility,           // - 【召唤物】其他敌对的
        PartnerMainPlayer,              // - 【伙伴】主角的
        PartnerOtherFriendly,           // - 【伙伴】其他友善的
        PartnerOtherHostility,          // - 【伙伴】其他敌对的
    }


    public enum E_Command
    {
        Create,
        Destroy,
        Update,
        Reg,
    }

    #region 设计相关
    public enum GameRenderType
    {
        ThreeD,//全3D
        TwoD//全2D
    }

    public enum E_ModelShowState
    {
        Normal = 0, //常规
        Super, //变身
        FashionModel, //时装
    }

    public enum E_AirState
    {
        VerySmallDownSpeed, //地面无重力-有腿支撑呢-重力有弹簧对抗-下降速度不会改变（当然是享受的重力的，这里是优化设计）：当character.isGround有替代者的时候
        GriDownSpeedOnGround, //保持-9.8
        StayInAir, //无重力
        GriDownSpeed, //速度持续增加
        InitSpeed//为了确定地面的
    }

    public enum EnumEnityListKey
    {
        /// <summary>
        /// 人物头顶的buff
        /// </summary>
        Env_Buff,

        /// <summary>
        /// 人物UI的buff，常驻，且有一个一定会加一个（主角就是这样）
        /// </summary>
        UI_Buff_MainPlayer,

        /// <summary>
        /// 其他人有一个加一个的（去副本我们小队头像都会显示，也是肯定不唯一的）
        /// </summary>
        UI_Buff_FB_Team,

        //-----下面是异步
        /// <summary>
        /// 唯一的（例如boss位就那一个）
        /// </summary>
        Union_Buff_Boss, //4，

        //-------------------------------------
        Env_EntityFx, //*被生命体征拥有的非生命体Fx，如非生命体征的创建就要放在FxMgr

        //----------------------------------------------------------------------------------------
        UI_Fx,

        Env_Bullet,
    }

    public enum E_WithOuLifeResType
    {
        NullEffect,
        Bullet, //子弹

        //----------战斗特效-------------
        //DamagePerfab,//伤害效果
        //DamageEffectPerfab,//伤害特效效果
        //DamageSuckBloodPerfab,//吸血
        //DamageBackAttackSpecial,//特殊被刺
        //DamageBackAttack,//單獨被刺
        //DamageLightAttack,
        FxUnit, //直接通过id来查找目前
        //-------------------------------
        //CommonBower,//士兵
        //-------------------------------
        //Coin,//金币
        //Diamond,//钻石
        //Box,//宝箱道具
        //Exp,//exp

        ////----场景资源----------
        //ENV_HOLE,//场景深坑
        //         //-----角色辅助，伤害
        UnitPendant,
        UI,
        Max,
    }

    #region 【技能相关枚举】


    /// <summary> 目标选择类型 </summary>
    public enum E_SkillBlackBoardKey
    {
        InputRota, // 朝向输入
        InputCoord, // 坐标输入
        InputTarget // 目标输入
    }

    /// <summary>
    /// 客户端自己定义的技能结束的类型
    /// </summary>
    public enum E_ClientSkillEndType
    {
        /// <summary>
        /// 默认状态,一般这种状态,不做特殊处理
        /// </summary>
        Default = 0,
        /// <summary>
        /// 服务器默认结束
        /// </summary>
        ServerDefault,

        /// <summary>
        /// 服务器结束技能
        /// 不管是通常的服务器一个技能自己的 skillEndRet
        /// 还是技能被服务器其它技能的skillEndRet 主动打断，都是ServerEnd类型
        /// </summary>
        ServerBreak,

        /// <summary>
        /// 客户度正常的技能阶段结束类型
        /// </summary>
        ClientNormalEnd,

        /// <summary>
        /// 客户端主动的打断技能导致的技能结束。
        /// 例如释放技能1的某个阶段，客户度使用请求技能2，
        /// 从而打断技能1
        /// </summary>
        UseSkill,

        /// <summary>
        /// 移动打断技能
        /// </summary>
        Move,
        /// <summary>
        /// 客户端主动取消技能
        /// </summary>
        ClientQuit,

        /// <summary>
        /// 客户端 执行 reset 接口的时候, 会将技能实体 按 reset 的方式退出
        /// </summary>
        Reset,
        /// <summary>
        /// 通用的打断技能类型, 意味这 就是打断这个技能, 而不需要关心 怎么被打断的
        /// </summary>
        CommonBreakSkill,
    }

    /// <summary>
    /// 打断技能的类型
    /// </summary>
    public enum E_BreakSkillType
    {
        /// <summary>
        /// 使用新技能的打断旧技能类型
        /// </summary>
        UseSkill,

        /// <summary>
        /// 移动打断技能类型
        /// </summary>
        Move,
    }

    /// <summary>
    /// 客户端自己定义的技能CD类型
    /// </summary>
    public enum E_SkillClientCDType
    {
        /// <summary>
        /// 由不同的技能阶段控制的CD。
        /// 目前的技能分别在引导阶段前后，开启CD。
        /// 这种方式，都是由技能运行时相应的状态来开启CD的。
        /// </summary>
        StageControl,

        /// <summary>
        /// 单机状态
        /// 使用技能（收到服务器协议）立即触发CD。
        /// 这种方式，在技能开始阶段之前，可以有SkillUnit来控制CD开启。
        /// </summary>
        OnUseSkill,

        /// <summary>
        /// 单机状态
        /// 使用技能后，等待摇杆间隔时间，超过间隔时间后，开启CD。
        /// 一般用于单个技能位 多技能的连招处理。
        /// 技能的CD由 SkillUnit 来控制开启。
        /// </summary>
        OnWaitJoyInterval
    }

    /// <summary>
    /// 技能阶段的进入类型
    /// </summary>
    public enum E_SkillStageEnterType
    {
        Default,
        /// <summary>
        /// 被技能打断类型,
        /// note:
        ///     目前约定阶段进入类型为 SkillBreak时,不能抢占活跃技能
        /// </summary>
        SkillBreak,
        /// <summary>
        /// 技能恢复阶段时阶段进入类型,
        /// </summary>
        Recover,
        /// <summary>
        /// 技能恢复阶段时 恢复当前阶段的类型,此处阶段类型与 Recover 区分,
        /// 是因为 Recover 走的普通阶段的恢复, 而 RecoverCurStage 是 服务器当前
        /// 技能的 恢复
        /// </summary>
        RecoverCurStage,
    }

    /// <summary>
    /// 技能阶段的退出类型
    /// </summary>
    public enum E_SkillStageExitType
    {
        Default,
        /// <summary>
        /// 被打断类型
        /// </summary>
        Broken,
        /// <summary>
        /// 抢占主动技能失败,需要结束整个技能运行时
        /// </summary>
        FailedSetMainSkill,
        /// <summary>
        /// 技能恢复阶段时阶段退出类型,
        /// </summary>
        Recover,
        /// <summary>
        /// 阶段 运行的时间 超过了最大的时间线的时间， 那客户端可以主动关闭这个阶段运行时
        /// </summary>
        OverMaxStageTime,
    }

    /// <summary>
    /// 技能阶段的打断类型
    /// (对应配置表中 阶段的 打断类型)
    /// </summary>
    public enum E_SkillStageBreakType
    {
        Default,
        /// <summary>
        /// 跳到指定阶段
        /// </summary>
        SkipStage,
        /// <summary>
        /// 打断技能运行时
        /// </summary>
        BreakSkill,
    }

    /// <summary>
    /// 技能的结束类型
    /// </summary>
    public enum E_SkillExitType
    {
        /// <summary>
        /// 正常时间结束类型
        /// </summary>
        Default,
        /// <summary>
        /// 服务器正常时间结束类型
        /// </summary>
        ServerDefault,
        /// <summary>
        /// 打断技能类型，任何技能中途的结束，都可以是打断技能
        /// </summary>
        ClientBreakSkill,
        ServerBreakSkill,
        /// <summary>
        /// 抢占主动技能失败,需要结束整个技能运行时
        /// </summary>
        FailedSetMainSkill,
        /// <summary>
        /// 技能重置导致的技能退出
        /// </summary>
        Reset,
    }

    /// <summary>
    /// 播放效果的类型
    /// </summary>
    public enum E_StageType
    {
        /// <summary>
        /// 未定义
        /// </summary>
        None,
        /// <summary>
        /// 技能播放效果
        /// </summary>
        Skill,
        /// <summary>
        /// buff播放效果
        /// </summary>
        Buff,
        Bullet,
        Passive,
    }

    /// <summary>
    /// 跳转到指定阶段的类型(根据跳转阶段的时机)
    /// </summary>
    public enum E_SkipToStageType
    {
        None,
        /// <summary>
        /// 当前阶段退出的时候,跳转到指定阶段
        /// </summary>
        OnCurStageExit,
        /// <summary>
        /// 当阶段进入且 没有活跃阶段的时候,目前服务器的
        /// 阶段处理逻辑统一放在了 阶段进入时处理的
        /// </summary>
        OnNoneActiveStageEnter,
    }

    /// <summary>
    /// 阶段事件帧导出规则类型
    /// </summary>
    public enum E_StageFrameExportType
    {
        /// <summary>
        /// 左开右闭(]
        /// </summary>
        LeftOpenRightClose,
        /// <summary>
        /// 左闭右开[)
        /// </summary>
        LeftCloseRightOpen,
        /// <summary>
        /// 左右闭合[]
        /// </summary>
        LeftCloseRightClose,
    }

    /// <summary>
    /// 客户端执行结果的状态, 目前的效果 执行分为 客户端线和 服务期线两条线分别执行.
    /// 所以执行 结果 会包含  ClientRun/ ServerRun。
    /// 同时，对于客户端线 的效果来说, 它 可能 有些数据,需要依赖 服务器, 有些数据 只需要客户端表现,
    /// 所以 客户端执行效果 状态 可以 继续细分:  clientRunServer(客户端执行了服务器线数据) / clientRun
    /// </summary>
    [Flags]
    public enum E_EffectExecuteResultState
    {
        /// <summary>
        /// 客户端线执行， 一般来说,只要客户端线执行了，不管执行结果,都会更新 它的状态为 ClientRun
        /// </summary>
        ClientRun = 0,
        /// <summary>
        /// 客户端线执行 了服务器线 数据 , 如果客户端线 额外执行了 服务器线的数据, 那就需要手动同步 它的状态为 ClientRunServer
        /// </summary>
        ClientRunServer = 1 << 0,
        /// <summary>
        /// 服务器线执行
        /// </summary>
        ServerRun = 1 << 1,
    }

    #endregion

    public enum EntityOutLifeType
    {
        FollowRole, //跟随主角；服务器部管移动
        MoveStandAlong, //独自需要呗驱动
    }

    public enum LocalEffectTags
    {
        UI,
        Env,
        ActorFx,
        Weapon,
    }

    /// <summary>
    /// 客户端可以控制的一切命令组：
    /// 0命令获取器，转换器
    /// 1服务器命令优先ServerLock组
    /// 2客户端自由组
    /// 3命令能力值
    /// 4Command也合并进去
    /// 【大小是控制【到】那一步,数值是直接可以设置，】
    /// </summary>
    public enum GameKeyCommand
    {
        None = 0,

        StandBy = E_ULayerSubState.StandBy,  // 待机指令
        Idle = E_ULayerSubState.Idle,
        BattleIdle = E_ULayerSubState.BattleIdle,
        WeaponRetractionIdle = E_ULayerSubState.WeaponRetractionIdle, // 战斗idle 切换到 普通 idle 过程中 的 中间 收刀动作的状态
        Performance = E_ULayerSubState.Performance,                       // 用于动作表演
        //客户端全命令:3选1
        //MoveTap = E_ULayerSubState.Moving, //idle+取消技能后摇（但是肯定不能取消技能）他包含了特殊实现的IsStop？          
        FaceTo = 120000, //E_ULayerSubState.TurnAround, //【可融合状态】                            // = E_SubState.Stage_Building_CannotMove, // 【大于本值，小于下一个，就会转换到本命令】Stage_Building.Stage_Building || BeingControl中的特殊几种如冰封 会让MoveConmand变成FaceTo
        Hurt = E_ULayerSubState.Hurt,

        WanderCommand = E_ULayerSubState.WanderMoving,  // 漫步状态
        MoveCommand = E_ULayerSubState.SingleMoving, //移动持续滑动
        BattleMoveCommand = E_ULayerSubState.BattleMoving, //移动持续滑动

        WeaponRetractionMoving = E_ULayerSubState.WeaponRetractionMoving, // battleMove 切换 到 singleMoving 中间的 过渡 收刀动作的状态

        //FaceTo2 = E_SubState.IsSonscious, //【大于本值，小于下一个，就会转换到本命令】Stage_Building.Stage_Building || BeingControl中的特殊几种如冰封 会让MoveConmand变成FaceTo
        //点击一下摇杆||或者松开事件，取消一切移动，


        Stage_SkillPro = E_ULayerSubState.Stage_SkillPro, //技能按钮按下，
        Stage_SkillEnergy = E_ULayerSubState.Stage_SkillEnergy, //持续选取方向，或者持续憋能量
        SkillCancelCommand = 120001, //取消一切技能


        //服务器命令  ----服务器命令属于强命令，客户端发了，再移动，也要听服务器的      
        Stage_BuildPro = E_ULayerSubState.Stage_BuildPro,

        //施法进行中[s]
        Stage_Building = E_ULayerSubState.Stage_Building,

        //施法后[s]
        Stage_Builded = E_ULayerSubState.Stage_Builded,

        Stage_Builded_Tail = E_ULayerSubState.Stage_Builded_Tail, //持续选取方向，或者持续憋能量


        Skill_Common1 = E_ULayerSubState.Skill_Common1,
        Skill_Common2 = E_ULayerSubState.Skill_Common2,
        Skill_Common = E_ULayerSubState.Skill_Common,

        //交互1
        InterAction1 = E_ULayerSubState.InterAction1,

        //交互2
        InterAction2 = E_ULayerSubState.InterAction2,

        //开始倒下
        BegineKnockDown = E_ULayerSubState.BegineKnockDown,

        //倒下过程中
        KnockDown = E_ULayerSubState.KnockDown,

        //倒下起身
        EndKnockDown = E_ULayerSubState.EndKnockDown,
    }


    /// <summary> 使用技能 蓄力阶段影响状态效果 </summary>
    public enum E_SkillEnergyState
    {
        NoOwnMove = 1, // 1：不可移动
        NoRotate, // 2：不可转向
        NoBreak, // 3：不可被打断
        NoDizziness, // 4：不可被昏迷
        NoDisplacement, // 5：不可被位移
        NoHurt, // 6：不受到伤害
        NoChecked, // 7：不可被目标选中（包括客户端和技能结算）
        // [1,2,3,4]
    }

    /// <summary> 客户端主角状态 </summary>
    /// 客户端触发条件是：空放技能持续6秒循环
    /// 服务器是：具体产生伤害
    /// 两个的并集：是设定战斗状态标签
    public enum E_ClentMainPlayerState
    {
        Normal = 0, // 正常状态
        Battle = 1, // 战斗状态
        //不用bool的原因是可能有休闲状态比如
    }

    /// <summary>
    /// 真正的角色状态
    /// </summary>
    public enum E_PlayerStateForMusic
    {
        Normal = 0, // 正常状态
        Battle = 1, // 战斗状态
    }

    public enum ObjectSubState
    {
        //-===============================
        Default = 0,

        ///状态级---------------
        Idle = 10000, //待机状态,交互前

        EnterRange = 10001,
        StayRange = 10002,
        ExitRange = 10003,

        Intering = 20000, //交互中   拿出背包

        InterFinish = 30000, //交互结束 开心

        //-===============================

        Dead = 40000, //死亡，销毁
    }


    /// <summary>
    /// [玩家角色层控制]---游戏和主角之间的关系
    /// 【1级定义】
    /// 综合状态控制组 command = 【动画播放表现在逻辑层次优先级】或者说【是策划被打断的指令集合】
    /// 【大小是，重要程度】
    /// </summary>
    public enum E_ULayerSubState
    {
        Default = 0,

        StandBy = 500,    // 待机播放 待机动作的状态,目前 gl 的需求是  idle 随机 10-15s 切换到待机动作, 而任何动作 都可以打断待机

        ///状态级---------------
        Idle = 10000, //[非战斗状态]有动画表现
        BattleIdle = 10001, //[战斗状态]有动画表现 初始战斗待机
        WeaponRetractionIdle = 10002, //由 battleIdle 切换到 普通 idle 状态的时候,新增 的一个 收刀过渡 动作的状态

        Performance = 10100,      //用于动作表演

        ///状态限制级
        WeakNess = 30000, //没有血的jugg虚弱状态，或者弱控制，例如减速状态

        //交互--------------------
        InterAction1 = 30100, //交互动作1
        InterAction2 = 30200, //交互动作2

        ///被动级---------------
        Hurt = 40000, //有动画表现 优先级为此，不会单独成为一个强切换状态

        //还有能否强制的概念可以特殊写
        ///主动级-----------------
        WanderMoving = 50000,   // 漫步状态
        //Searching,                //【服务器Ai状态】查找中，查找公共组：人物目标，建筑目标：分支状态-
        SingleMoving = 50001, //移动中,中切换状态。【组2】（非战斗状态的瞎走）【注意这个SingleMove是玩家自己控制的简单移动】
        BattleMoving = E_ULayerSubState.SingleMoving + 1, //移动中,中切换状态。【组2】（战斗状态的瞎走）

        WeaponRetractionMoving = E_ULayerSubState.BattleMoving + 1, // 由 battleMove 切换 到 singleMoving 的过程中,新增的 一个 移动 收刀动作

        Skill = 60000, //[S]                      //所有技能，强切换状态，【组1】

        Skill_Common1 = E_ULayerSubState.Skill + 11000,
        Skill_Common2 = E_ULayerSubState.Skill + 12000,
        Skill_Common = E_ULayerSubState.Skill + 15000,




        //6k~6500
        //技能前摇[s]
        Stage_SkillPro = E_ULayerSubState.Skill + 1000,

        //蓄力阶段[s]
        Stage_SkillEnergy = E_ULayerSubState.Skill + 2000, //可以被Cancel命令打断

        //服务器命令中---------------------------------------------------
        //施法前摇[s]
        Stage_BuildPro = E_ULayerSubState.Skill + 3000,

        //施法进行中[s]--移动--2000类移动类持续释放技能
        Stage_Building = E_ULayerSubState.Skill + 5000,

        //施法进行中[s]--不能移动--2000类不能移动类持续释放技能
        Stage_Building_CannotMove = E_ULayerSubState.Skill + 7000,

        //施法后[s]
        Stage_Builded = E_ULayerSubState.Skill + 9000,
        //服务器命令中---------------------------------------------------

        //施法后摇[客户端状态][特殊！]
        Stage_Builded_Tail = E_ULayerSubState.Hurt - 100, //【3900】可以被动Hurt以上打断


        ///控制级[S]-----------------
        BeingControl = 70000, //被控制，超强控制切换状态

        //开始倒下
        BegineKnockDown = 70001,

        //倒下的过程中
        KnockDown = 70002,

        //倒下起身
        EndKnockDown = 70003,

        IsSonscious = 70500, //大于这个数值就是意识清醒
                             //嘲讽,恐惧,冰冻                                      （服务器靠原子锁，客户端枚举是控制动画状态的切换条件）
                             //Chase,                                                //追着打，【组1】,Ai或者主角狂暴必须干死



        //失控级[S]----------------
        Deading = 80000, //吃鸡持续死亡，尸体不复活状态
        //Lock =    90000
    }


    //逻辑状态上 移动可以和任何逻辑状态融合 动画不能容和就是动画优先级来播放切换；动画能融合就融合播放；所以移动和自动战斗是一个单独的调配器；但是其指令可以放在stateCommond中（尽量）
    //两个调配器互斥，一个开放别的不行，记录当前状态变化，发送状态指令，根据状态循环触发；service类似写外挂；路点的不用备份没有暂停恢复就行了；不行就重新开始
    /*   自动战斗，
       互斥
       自动寻路不同

    放成一个状态合并到 E_ULayerSubState 尽量
     *  
     *  public enum E_AutoFindPathCommand
       {
           发起
           执行中
           能不能被开启/暂停
           终止
       }

       public enum E_AutoBattleCommand
       {
           发起
           执行中
           能不能被开启/暂停
           终止
       }
   */





    /// <summary>
    /// 技能位类型
    /// </summary>
    public enum E_SkillSetType
    {
        /// <summary>
        /// 普工技能类型
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 常规技能
        /// </summary>
        Conventional = 2,

        /// <summary>
        /// 辅助技能-------（弃用）
        /// </summary>
        Assistant = 3,

        /// <summary>
        /// 终极技能
        /// </summary>
        Ultimate = 4,

        /// <summary>
        /// 冲刺技能
        /// </summary>
        sprint = 5,

        /// <summary>
        /// 宠物技能-------（弃用）
        /// </summary>
        Pet = 6,
    }

    public enum E_SkillEffect
    {
        #region TODO Delete 等待删除的老效果
        /// <summary>
        /// 位移 注明效果子表--------------------
        /// </summary>
        OffsetNodeMsg, //--
        /// <summary>
        /// 回蓝
        /// </summary>
        AddMagic,

        /// <summary>
        /// 扣蓝
        /// </summary>
        DecMagic,
        /// <summary>
        /// 按照绝对值修改技能cd
        /// </summary>
        FixCDAbs,
        /// <summary>
        /// 按照百分比修改技能cd
        /// </summary>
        FixCDPercent,
        /// <summary>
        /// 设置技能模块固有cd时间
        /// </summary>
        SetSkillCD,
        /// <summary>
        /// 按照百分比设置技能模块固有cd时间
        /// </summary>
        SetSkillCDPercent,

        #endregion


        //-----------------------------------------
        /// <summary>
        /// 空效果 注明效果子表
        /// </summary>
        Empty = 1000,
        /// <summary>
        /// 空效果 ,有些效果只有服务器 需要执行,客户端不需要执行,就将这个效果设置为None
        /// </summary>
        None,

        // 以下是 编译配置的一些新的 效果类型
        /// <summary>
        /// 增加碰撞盒
        /// </summary>
        CollisionBox,
        /// <summary>
        /// 编译器配置的目标集合类型
        /// </summary>
        TarGroup,
        /// <summary>
        /// 编译器配置的等待用户输入类型
        /// </summary>
        WaitInput,
        /// <summary>
        /// 判断用户操作
        /// </summary>
        IsInput,
        /// <summary>
        /// 判断阶段
        /// </summary>
        IsStage,
        /// <summary>
        /// 治疗
        /// </summary>
        Treat,
        /// <summary>
        /// 造成伤害
        /// </summary>
        Damage,
        /// <summary>
        /// 根据坐标位移
        /// </summary>
        MoveWithPos,
        /// <summary>
        /// 根据朝向位移
        /// </summary>
        MoveWithRot,
        /// <summary>
        /// 修改蓝量
        /// </summary>
        ChangeMana,
        /// <summary>
        /// 修改CD
        /// </summary>
        ChangeCD,
        /// <summary>
        /// 打断目标当前技能
        /// </summary>
        BreakCurRuntime,
        /// <summary>
        /// 修改属性
        /// </summary>
        ChangeProp,
        /// <summary>
        /// 施加BUFF 
        /// </summary>
        AddBuff,
        /// <summary>
        /// 按标签类型去除BUFF--------------------
        /// </summary>
        RemoveBuff,
        /// <summary>
        /// 创建子弹  
        /// </summary>
        CreateBullet,
        /// <summary>
        /// 创建子弹  销毁子弹
        /// </summary>
        DestoryBullet,
        /// <summary>
        /// 设置子弹 目标点
        /// </summary>
        SetBulletTargetPos,
        /// <summary>
        /// 打断子弹对应的阶段
        /// </summary>
        BreakCurRuntimeInBullet,
        /// <summary>
        /// 用户输入
        /// </summary>
        UserInput,
        /// <summary>
        /// 蓄力
        /// </summary>
        Energy,

        /// <summary>
        /// 在目标点播放效果
        /// </summary>
        PlayEffectAtPoint,
        /// <summary>
        /// 在目标 target身上播放效果
        /// </summary>
        PlayEffectAtTarget,
        /// <summary>
        /// 在两点间播放特效
        /// </summary>
        PlayEffectBetweenPoints,
        /// <summary>
        /// 隐身效果
        /// </summary>
        Stealth,
        SingleRandomPoint,
        DamageSecond,
        /// <summary>
        /// 判断目标key是否为空
        /// </summary>
        IsNotEmpty,
        SelectHitFromKey,
        SpSelectTarget,

        /// <summary>
        /// 抛出一个子弹
        /// </summary>
        ThrowBullet,
        /// <summary>
        /// 追踪弹
        /// </summary>
        LaunchBullet,
        /// <summary>
        /// 贝塞尔追踪弹
        /// </summary>
        BezierBullet,
        /// <summary>
        /// 播放 lineRender 效果
        /// </summary>
        PlayEffectLineRenderer,
        /// <summary>
        /// 黑洞效果
        /// </summary>
        EffectTypeRegister,
        //---------------------------------------


        /// <summary>
        /// 转向
        /// </summary>
        ChangeToward,

        /// <summary>
        /// 检查目标Key的int值
        /// </summary>
        CheckIntKey,
        ClientSummonAnim,                 //客户端召唤物播放动作
        ClientSummonEffect,                 //客户端召唤物播放特效
        ClientSummonTurnTo,                 //客户端召唤物朝向目标
        ClientSummonRemove,                 //移除客户端召唤物

        /// <summary>
        /// 目标集合修改至绝对朝向
        /// </summary>
        ChangeToAbsoluteToward,

        CheckToward,
        CheckPassive,

    }

    /// <summary>
    /// 客户端 移动特效 的类型枚举
    /// </summary>
    public enum E_ClientMoveFxType
    {
        /// <summary>
        /// 移动到 目标的pos
        /// </summary>
        Move2TargetPos,
        /// <summary>
        /// 移动到 跟随目标
        /// </summary>
        Move2FollowTarget,
    }

    /// <summary>
    /// 播放效果的模式
    /// </summary>
    public enum E_PlayEffectModel
    {
        /// <summary>
        /// 时间轴 的效果播放模式
        /// </summary>
        TimeLine,
        /// <summary>
        /// Next 为true
        /// </summary>
        Next_Ture,
        /// <summary>
        /// Next 为false
        /// </summary>
        Next_False,

    }

    /// <summary>
    /// 技能释放类型
    /// </summary>
    public enum E_UseSkillResult
    {
        //正常
        Succeed = 0,
        // CD没冷却
        CD_Not_Enough = 1,
        //状态不对
        Status_Error = 2,
        //蓝量不足
        MP_Not_Enough = 3,
        //优先级不够
        Priority_Not_Enough = 4,

        //技能正在运行,将会CD
        CD_Will_Running = 8,

        // 用户输入未开启
        User_Input_Not_Open = 10,
        // 在非活跃时立即执行用户输入
        Run_User_Input_On_No_Active = 15,
        // 在非活跃时,执行 立即用户 输入
        Run_User_Immediate_Input = 16,

        // 打断活跃技能然后立即执行用户输入
        Break_Active_Run_User_input,
        // 缓存用户输入
        Cache_User_Input = 30,
        //在用户输入在 技能阶段的尾部,此时需要屏蔽用户输入
        Input_At_StageEnd = 31,
        // 用户输入轴的 禁止输入范围
        Input_At_Forbid = 32,


        // 没有需要执行的技能的效果
        No_Skill_Effect = 40,
        // 技能槽中当前技能不是需要使用的技能
        Cur_Not_UseSkill = 50,

        /// <summary>
        /// 使用技能时,存在技能且遥感 与 主角角度 过大时，gl 说不能使用这个技能
        /// </summary>
        Cur_JoyStick_Driction_Error = 60,

        /// <summary>
        /// 延迟 使用 执行这个技能, 用于 蓄力秒放,但是又需要 执行一个最小蓄力周期这种,
        /// 它的 使用技能操作 属于成功,但是 需要延迟 去 执行
        /// </summary>
        DealyInvokeSkill = 70,

        /// <summary>
        /// 技能预播 导致的 技能槽按钮cd, 每个技能 单独配置
        /// </summary>
        Button_CD_Not_Enough = 80,

        /// <summary>
        /// 预输入操作 开始 提前预播的时候, 服务器还没有返回 预输入操作的 结果. 目前跟 服务器约定的是 预输入操作,
        /// 客户端 需要等待 服务求先返回预输入操作的结果。 如果一个输入轴效果中 触发了多次 预输入操作,那只要返回
        /// 一次,客户端 都认为 这个 预输入轴 的 预输入操作可以 提前预播.
        /// </summary> 
        Server_Not_Ret_Input_Result = 90,

        /// <summary>
        /// 原子锁 禁止使用技能
        /// </summary>
        ForbidAttack = 100,

        /// <summary>
        /// 直接使用技能
        /// </summary>
        Send_Use_Skill = 200,

        // 使用技能失败
        Failed = 6000,

        //// 有什么后面继续加
        /// 
        /// 
    }

    /// <summary>
    /// 同步 技能 使用类型
    /// </summary>
    public enum SyncSkillUseType
    {
        /// <summary>
        /// 客户端 预播 技能使用
        /// </summary>
        ClientPreUse,
        /// <summary>
        /// 服务器 预播 技能使用
        /// </summary>
        ServerUse,
        /// <summary>
        /// 客户端 执行 预输入操作
        /// </summary>
        ClientPreUserInput,
        ServerUserInput,
    }

    /// <summary>
    /// 用户输入轴 非法状态
    /// </summary>
    [Flags]
    public enum E_UserInputInvalidState
    {
        None = 0,
        /// <summary>
        /// 被标记为 非法状态
        /// </summary>
        MarkInvalid = 1 << 0,
        /// <summary>
        /// 被标记为 提前执行
        /// </summary>
        MarkPreExecute = 1 << 1,
        /// <summary>
        /// 被标记为 关闭状态
        /// </summary>
        MarkClose = 1 << 2,

    }

    /// <summary>
    /// 技能槽 等待时间的 类型
    /// </summary>
    public enum E_SkillContainerWaitIntervalType
    {
        None,
        /// <summary>
        /// 用户输入
        /// </summary>
        UserInput,
        /// <summary>
        /// 蓄力的输入
        /// </summary>
        ChargeInput,
        /// <summary>
        /// buff 开启的等待时间 类型
        /// </summary>
        Buff,
    }

    /// <summary>
    /// 技能/buff等实体的状态
    /// </summary>
    [Flags]
    public enum E_EntityState
    {
        None = 0,
        /// <summary>
        /// 正在运行的状态
        /// </summary>
        Running = 1 << 0,
        /// <summary>
        /// 客户端关闭技能
        /// </summary>
        ClientClose = 1 << 1,
        /// <summary>
        /// 服务器关闭技能
        /// </summary>
        ServerClose = 1 << 2,
    }

    /// <summary>
    /// Buff 标签
    /// </summary>
    public enum E_BuffeTag
    {
        Debuffs = 1,            //减益
        Bleed = 2,              //流血
        Shield = 3,             //护盾
        Decelerate = 4,         //减速
        Freeze = 5,             //冰冻
        KnockDown = 6,          //击倒
        ShadowFollow = 7,       //跟随的影子
        ChangeAnims = 8,        //改变状态动画
        Invisible = 9,                 //隐身
        Paralysis = 10,          //麻痹(类似冰冻效果，但没有shader表现，只是动作暂停)
    }

    #endregion

    #region 摄像机相关

    /// <summary>
    /// 摄像机震动方向
    /// </summary>
    public enum E_CameraShakeType
    {
        /// <summary>
        /// X轴方向
        /// </summary>
        X = 1,

        /// <summary>
        /// Y轴方向
        /// </summary>
        Y,

        /// <summary>
        /// Z轴方向
        /// </summary>
        Z,

        /// <summary>
        /// 随机的X/Y/Z的某一个方向
        /// </summary>
        Random
    }

    [XLua.LuaCallCSharp]
    /// <summary>
    /// 摄像机效果类型
    /// </summary>
    public enum E_CameraEffectType
    {
        /// <summary>
        /// 屏幕震动
        /// </summary>
        Shake = 0,

        /// <summary>
        /// 屏幕视野缩放
        /// </summary>
        Zoom = 1,

    }

    /// <summary>
    /// 摄像机效果广播类型
    /// </summary>
    public enum E_CameraEffectBroadCastType
    {
        /// <summary>
        /// 不广播的特效 只 针对自己播放 技能效果: 比如 砸地
        /// </summary>
        NotBroadCast = 0,

        /// <summary>
        /// 广播类型，一般是在屏幕内，都可以广播，后面再具体细分
        /// </summary>
        BroadCast = 1,
    }

    [XLua.LuaCallCSharp]
    public enum E_CameraType
    {
        UICam,
        LoginCam,
        StarWorldCam,
        SpecialCam,//不确定是什么，可能做Raw，可能分离做NPC对焦点
        SpecialDepthCam//Raw的深度Camera
    }

    [XLua.LuaCallCSharp]
    /// <summary>
    /// 效果，类型，参数
    /// 每个都具备参数，需打包成类，不能用二进制按位的操作来
    /// </summary>
    public enum CameraEvent
    {
        ShakeCam,
        FlashCam,//是屏幕效果，并非策划说的人物闪白
        CameraZoom,
        CameraTrail,//轨迹
        CameraShaderFx,
        CameraPost,
        CameraFx,
        WidgetAnim,
        //TimeScale,时间暂停不是摄像机效果，但是记录在这里
    }

    [XLua.LuaCallCSharp]
    /// <summary>
    /// 屏幕视频效果事件类型枚举
    /// </summary>
    public enum E_AVType
    {
        AvUI,
        AvCG
    }

    #endregion

    /// <summary>
    /// 通用的帧事件类型
    /// </summary>
    public enum E_FrameEventType
    {
        /// <summary>
        /// 播放视频的帧事件类型
        /// </summary>
        Video = 100,
    }

    /// <summary>
    /// 按钮输入类型
    /// </summary>
    public enum E_BtnInputType
    {
        PointerDown,    // 按下
        PointerUp,      // 抬起
    }

    /// <summary>
    /// 使用技能类型
    /// </summary>
    public enum E_UseSkillType
    {
        /// <summary>
        /// 不干任何事
        /// </summary>
        None,
        /// <summary>
        /// 使用技能类型
        /// </summary>
        UsingSkill,
        /// <summary>
        /// 执行技能效果
        /// </summary>
        ExecuteEffect,
    }


    /// <summary>
    /// 通用的帧事件类型
    /// </summary>
    public enum E_StageFrameEventType
    {
        Default,
        Animation,
        SpecialEffects,
        Audio,
        Effect,
        Camera,
        CameraShake,
        /// <summary>
        /// 原子状态
        /// </summary>
        States,
    }

    #region Tag相关

    public enum E_TagType
    {
        Untagged,
        Respawn,
        Finish,
        EditorOnly,
        MainCamera,
        Player,
        GameController,
        NPC,
        MainPlayer,
        TeamMate,
        Neutral,
        Enemy,
        Boss,
        Collections, //策划的交互物件，更趋近于收集物，NPC不属于
    }

    public enum E_SortingLayerType
    {
        SceneMsg_3D,
        ModelMsg_3D,
        Fx_Backest_3D,
        PendantMsg_3D,
        StateMsg_3D,
        HealthMsg_3D,
        BattleMsg_3D,
        UI_BackGround_2D,
        Fx_BackGround_3D,
        UI_Cell_2D,
        Fx_Cell_3D,
        UI_Page_2D,
        Fx_Page_3D,
        UI_Window_2D,
        Fx_Window_3D,
        UI_Widget_2D,
        Fx_Widget_3D,
        UI_Tips_2D,
        Fx_Tips_3D,
        UI_Top_2D,
        Fx_Top_3D,
        UI_Guid_Top_2D,
        Scene_Topest_3D
    }

    /// <summary>
    /// 这里定义的String名字，之后就不用因为工程修改来改这个了
    /// </summary>
    public enum E_LayerType
    {
        //1-7 不可以
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        //3
        Water = 4,
        UI = 5,
        //6
        //7

        //8-31 自定义
        Walkable = 8,
        Obstacle = 9,
        Entity = 10,
        Ground = 11,
        Wall = 12,
        BlockObstacle = 13,
        CrossEntity = 14,
        RealDynamicAirWall = 15,
        ThreeDUIMessage = 16,
        Bullet = 17,
        AkEvents = 18,
        //19
        //20
        //21
        //22
        //23
        //24
        Bloomed = 25,
        //CartoonCast = 26,
        //CartoonReceive = 27,
        //28
        //29
        DarkEnvEntityLight = 30,
        DynamicObject = 31
    }

    #endregion


    #region 伤害飘字类型

    public enum E_HurtType
    {
        // 总伤害
        Hurt_Null = 0,
        // 物理伤害
        Hurt_Physic = 1,
        // 魔法伤害
        Hurt_Magic = 2,
        // 自然伤害
        Hurt_Elem1 = 3,
        // 元素伤害
        Hurt_Elem3 = 4,
        // 意志伤害
        Hurt_Elem2 = 5,
        // 本源伤害
        Hurt_Elem4 = 6,
        // 星次伤害   
        Hurt_Elem5 = 7,
        // 虚无伤害
        Hurt_Elem6 = 8,

        // 吸血
        SuckBlood = 100,
        // 反伤
        ThornsBlood = 101,
        //护盾抵挡伤害
        ShieldValue = 102,
    }

    public enum E_TextMoveType
    {
        None,
        LeftParabola = 10,          // 左抛物线
        LeftParabolaCrit = 11,      // 左抛物线暴击
        LeftParabolaNew = 12,      // 新左抛物线暴击
        RightParabola = 20,         // 右抛物线
        RightParabolaCrit = 21,     // 右抛物线暴击
        Bounce = 30,                // 弹跳
        Floating = 40,              // 上浮
        FloatingNew = 41,              // 新上浮
        Emerge = 50,                // 浮现
        CommonDamage = 60,                // 普通伤害
        Crit = 70,                // 暴击

        Common = 100,                // 普通
        Crit2 = 110,                // 暴击
    }
    #endregion

    // 技能按钮状态
    public enum E_SkillBtnState
    {
        None,
        Active = 1 << 0,        // 正常可用状态
        Pressed = 1 << 1,       // 按下状态
        Inactive = 1 << 2,      // 无法使用状态
        OnCooldown = 1 << 3,    // CD中
    }

    #region LoginUI

    /// <summary>
    /// 区服列表类型
    /// </summary>
    public enum E_AreaTabType
    {
        MyArea,
        RecommendArea,
        OtherArea,
    }

    public enum E_RecordType
    {
        RecordLoginIP,      // 登录IP
        RecordLoginID,      // 登录账号ID
        RecordGroupInfo,    // 区服信息
        RecordLoginPID,     // 登录角色ID
    }

    #endregion


    public enum E_AssetType
    {
        Animation,
        Effects,
        Roles,
    }

    /// <summary>
    /// 实体显影 的 tag 类型标签
    /// </summary>
    [Flags]
    public enum EntityShowHidenTag
    {
        None = 0,
        // 自己。逻辑的自己：类似关卡NPC，和任务NPC，开始不需要的表现
        Self = 1 << 0,
        // 跟随的影子
        FollwShadow = 1 << 1,
        // 镜像影子
        MirrorShadow = 1 << 2,


        //=====================================以战斗的方式隐藏自己===================================
        //战斗的自己：媚光隐身的闪烁
        BattleSelf = 1 << 3,//另一个维度，把self一维变成二维的
    }

    /// <summary>
    /// 移动类型
    /// </summary>
    public enum EntityMoveType
    {
        None,
        /// <summary>
        /// 路点位移
        /// </summary>
        Path,
        /// <summary>
        /// 技能位移
        /// </summary>
        SkillMove,
        /// <summary>
        /// 技能位移
        /// </summary>
        PreSkillUseMove,
        /// <summary>
        /// 其他人的路点位移
        /// </summary>
        ThirdPersonMove,
    }

    public enum CamerOffsetStage
    {
        None = 0,
        In = 1,
        Stage = 2,
        Out = 3,
    }

    [XLua.LuaCallCSharp]
    public enum E_ChatMsgTrackState : byte
    {
        Free,           // 游离状态，一旦进入不会自动滚动
        LockToNew       // 有新消息进入会，自动滚动到最新消息
    }


    /// <summary>
    /// 加载技能配置表的类型
    /// </summary>
    public enum SkillCfgType
    {
        Skill,
        Buff,
        Passive,
        Bullet,
        FxDetail
    }

    /// <summary>
    /// 本地实体类型
    /// </summary>
    public enum E_LocalEntityType
    {
        None = 0,
        TreasureBox,        // 宝箱
        Gateway,            // 传送门
        WantedEnity,        // 通缉实体

        Summon,             // 召唤物
    }

    /// <summary>
    /// 本地实体创建来源
    /// </summary>
    public enum E_LocalEntitySource
    {
        None = 0,
        SecretArea,         // 秘境副本
        DailyEctype,        // 每日副本
        DailyTeamEctype,    // 每日组队副本
        WantedGameplay,     // 通缉玩法
        WildBoss            // 野外BOSS
    }


    /// <summary>
    /// 副本类型
    /// </summary>
    public enum SpaceStrType : byte
    {
        Normal,         // 正常副本
        PersonDaily,    // 个人日常
        PersonSecret    // 个人秘境
    }


    #region 音频相关
    [LuaCallCSharp]
    public enum E_SoundNTFtype : byte
    {

        //别人耳朵，现在耳朵不在相机，耳朵在相机和别人之间
        //自己地点
        MySelf,
        //目标地点
        Target,
        //全局音效
        Global,
        //自己耳朵
        MyListener_SystemSound,
    }
    #endregion


    /// <summary>
    /// 一些通用 触发事件的类型枚举
    /// /// </summary>
    public enum TriggerEventType
    {

        /// <summary>
        /// hero 实体 死亡的事件通知
        /// </summary>
        HeroDie,
        /// <summary>
        /// AOI 实体离开
        /// </summary>
        AOILeave,
        /// <summary>
        /// AOI 实体进入
        /// </summary>
        AOIEnter,
        /// <summary>
        /// 进入/离开副本
        /// </summary>
        Ectype,
        /// <summary>
        /// 切换地图之前的 协议通知
        /// </summary>
        MapPreloadNotice,

        LeaveSpace,

        /// <summary>
        /// 场景地图 切换
        /// </summary>
        MapChangeRet,

        /// <summary>
        /// 副本战斗结束
        /// </summary>
        EctypeModuleGameEnd,

        /// <summary>
        /// 主角没有 活跃技能
        /// </summary>
        M_NoneActiveSkill,
        /// <summary>
        /// 主角的 技能 cd 结束
        /// </summary>
        M_SkillEndCD,
        /// <summary>
        /// 主角的 开启了新的用户输入
        /// </summary>
        StartUserInput,

        /// <summary>
        /// 结束主角的 ButtonCD
        /// </summary>
        EndButtonCD,
        // Break
    }


    public enum LoadSceneType
    {
        Default,
        PreLoadPV,
        PreLoadFadeWhite,
        PreLoadFadeBlack,

    }


    /// <summary>
    /// [用户操作层控制] - 人和手机的关系
    /// 自动战斗的状态
    /// </summary>
    [Flags]
    public enum AutoBattleState
    {
        Close = 0,
        Open = 1 << 0,
        /// <summary>
        /// 挂起状态, 
        /// </summary>
        HoldOn = 1 << 1,
        /// <summary>
        /// 禁止状态
        /// </summary>
        Forbid = 1 << 2,
        /// <summary>
        /// 自动战斗进入其它寻路状态
        /// </summary>
        OtherFindingPath = 1 << 3,
        HoldOn_AVG = 1 << 4,
        /// 自动战斗等待 秘境 服务器下发挂点状态
        HoldOn_Screat_WaitPoint = 1 << 5,

    }

    /// <summary>
    /// 自动战斗中 设置 寻路目标点的类型.
    /// </summary>
    public enum AutoBattleSetTargetPosType
    {
        /// <summary>
        /// 副本类型 设置的 [自动战斗] 寻路目标点
        /// </summary>
        Ectype,
        /// <summary>
        /// 单人 日常本 副本设置的 [自动战斗] 寻路目标点
        /// </summary>
        PersonDailySpace,
    }


    #region 模块继承业务相关，哪些系统用了格子系统
    public enum E_SlotModule
    {
        BagModule = ModuleDef.Name.BagModule,                     //背包
        EquipModule = ModuleDef.Name.EquipModule,                 //装备
        AmuletModule = ModuleDef.Name.AmuletModule,               //护符
        MedicineModule = ModuleDef.Name.MedicineModule,           //药品
    }

    #endregion

    /// <summary>
    /// 地图显示页签类型[根据配置表一起修改]
    /// </summary>
    public enum MapShowTabType
    {
        None,
        Common, // 常用
        GamePlay, // 玩法
        Function, // 功能
        Transfer, // 传送
        Task, // 任务
        Monster,    // 怪物
    }

    /// <summary>
    /// 地图国度类型[根据配置表一起修改]
    /// </summary>
    public enum MapNationType
    {
        None,
        FenDe,     // 芬德
        HeiTieZhiSen,    // 黑铁之森 
        SiBanSai    // 斯班赛

    }




    /// <summary>
    /// 动画的 event 枚举
    /// </summary>
    public enum AnimationEventType
    {
        /// <summary>
        /// 动画播放 wwise 音频的 类型枚举
        /// </summary>
        Wwise
    }


    #region 玩家特效 scale 是否跟随玩家 的枚举
    public enum E_FxScale
    {
        None,
        /// <summary>
        /// 不跟随 玩家 尺寸变化，创建时 根据 玩家尺寸 创建，之后玩家尺寸变化， 
        /// </summary>
        NoFollow,
        /// <summary>
        /// 跟随 玩家尺寸 一起变化
        /// </summary>
        Follow,
        /// <summary>
        /// 原始特效尺寸, 不跟随 玩家尺寸 而变化。
        /// </summary>
        Origin,
    }

    #endregion

    // #if UNITY_EDITOR
    #region  离线技能编辑器 相关事件的枚举
    public enum LocalServerEventRsp
    {
        /// <summary>
        /// 切换怪物
        /// </summary>
        SwitchMonster,
        /// <summary>
        /// AOI同步
        /// </summary>
        AOIMsg,
        /// <summary>
        /// 主角的 属性同步, 曲 说目前 主角的属性同步走的是单独的协议通知
        /// </summary>
        PropSyncList,
        /// <summary>
        /// 子弹运行时的创建
        /// </summary>
        BulletCreateRet,
        /// <summary>
        /// 子弹运行结束
        /// </summary>
        BulletEndRet,
        /// <summary>
        /// 阶段的 消息同步
        /// </summary>
        RunStageRet,
        /// <summary>
        /// 阶段运行时强制结束消息, 本地服为了简单(没有打断逻辑), 阶段结束就发送这个通知 给客户端
        /// </summary>
        RunStageForceEndRet,

        /// <summary>
        /// 自定义的黑板 的消息同步, 对于 黑板数据, 目前客户端存的是 转换后的 customBlackBoard。
        /// 不太好 确定 它的原始数据 是哪个, 所以 就 先用 客户端 黑板数据 做同步.
        /// </summary>
        UpdateCusBlackBoard,
    }

    /// <summary>
    /// 同步自定义黑板的的类型
    /// </summary>
    public enum CusUpdateBlackType
    {
        BulletCraete
    }

    /// <summary>
    /// 客户端发送的请求本地服 执行指令
    /// </summary>
    public enum ClientEventReq
    {
        CreateMainPlayer,

        /// <summary>
        /// 主角的移动
        /// </summary>
        MoveMsg2,
        /// <summary>
        /// 创建一个 怪物
        /// </summary>
        CreateMonster,

        /// <summary>
        /// 请求本地服创建一个子弹
        /// </summary>
        CreateBullet,
        /// <summary>
        /// 设置子弹目标点的坐标
        /// </summary>
        SetBulletTargetPos,
        /// <summary>
        /// 打断 子弹 当前的运行时
        /// </summary>
        BreakCurRuntimeInBullet,
        /// <summary>
        /// 请求触发 触发器事件
        /// </summary>
        TriggerEvent,
        /// <summary>
        /// 同步坐标和朝向。 目前主要是在使用技能时 同步坐标和朝向:
        /// 
        /// 客户端使用技能. 使用技能时本地服本不做模拟逻辑，交由客户端线自己模拟整个流程。
        /// 但是 本地服需要知道 客户端线使用技能时的 坐标/角度 等信息.
        /// </summary>
        UpdatePosAndRot,
    }

    /// <summary>
    /// 触发器事件类型 枚举
    /// </summary>
    public enum TriggerEvent
    {
        /// <summary>
        /// 路点移动结束
        /// </summary>
        PathMoveEnd,

    }
    #endregion
    // #endif


    #region 红点类型
    /// <summary>
    /// 红点类型
    /// </summary>
    [LuaCallCSharp]
    [Hotfix]
    public enum RedPointType
    {
        /// <summary>
        /// 默认啥都没有的类型
        /// </summary>
        None,
        /// <summary>
        /// 红点组容器类型
        /// </summary>
        Group,

        ///Npc商店
        NpcShop = 10,
        /// 药品设置
        Medicine1 = 15, //战斗恢复
        Medicine2 = 16, //生命储备为空
        Medicine3 = 17, //法力储备为空
        /// 拍卖——工会
        Auction_Union = 20,
        /// 拍卖——世界
        Auction_World = 21,
        // 拍卖——待拍
        Auction_New = 22,
        // 组队
        TeamGroup = 25,
        // 工会
        TradeUnion = 30,

        //公会收到申请请求
        Union_Apply = 31,

        //公会更新了公告
        Union_Notice = 32,

        //公会功能的状态发生了变更
        Union_ChangeFunc = 33,


        #region 好友的类型
        /// 好友类型 目前 策划文案中的红点分为三种:
        /// 1.收到了新申请请求			交互按钮	    普通	    关闭界面后清除
        /// 2.收到了新消息			    信息栏	        强调-新	    关闭界面后清除
        /// 3.收到了新消息			    入口按钮	    数字	    关闭界面后清除
        /// 
        /// 红点系统 我的想法应该只针对 最外层的红点显示即可(如上3的红点).
        /// 但策划想要将 系统内部的 ui状态(如上1，2都是系统内部的UI状态)也纳入红点系统.
        /// 所以此处 采用好友的红点功能, 做一个示例.
        /// 
        /// 
        /// 
        /// <summary>
        /// 好友入口处的条件
        /// </summary>
        Friend = 35,
        /// <summary>
        /// 好友申请
        /// </summary>
        FriendRequest = 36,
        /// <summary>
        /// 好友通知
        /// </summary>
        FriendNotice = 37,

        #endregion


        #region 邮箱的红点 类型
        /// 2023/10/30
        /// 红点系统 我的想法应该只针对 最外层的红点显示即可. 但策划 想着 将系统内部的 ui状态也纳入红点系统.
        /// 所以 基于 邮箱 的功能, 采用两种方式分别实现 以作示例

        //-----------------------------------------------------------
        /// 基于最外层的实现:
        /// 使用方法:
        /// 1.在主界面 邮箱icon上 挂载 RedPoint;
        /// 2.配置表中 配置 Email 类型的红点条件 未 Email > 0
        /// <summary>
        /// 邮箱 类型， 只关心 邮箱最外层的红点是否显示.依赖于邮箱系统刷新的数据变化
        /// </summary>
        Email = 50,
        Mail_New = 51,
        Mail_Red = 52,
        //-----------------------------------------------------------


        //-----------------------------------------------------------
        //  复合模式暂不考虑. 
        //  深入思考后, 每个 邮件的 是否已读和 是否领取依赖于邮件自己的数据.
        //  没必要 将 ui 状态的逻辑 提出来。而且 这种条件也不好 作为红点系统的 通用条件独立出来

        /// 基于 内部状态也纳入红点系统的实现
        /// 邮箱的复合类型. 使用方法:
        /// 1.在主界面 邮箱icon上 挂载 RedPointContainer
        /// 2.在 RedPointTypeGroup 类型组中 拖入 复合邮件类型关心的 Email_UnRead 和 Email_UnReceived
        /// 3.在邮箱 的item 新和 未领取状态节点上 分别 挂上 RedPoint 组件，并设置 对应的类型
        /// 4.配置表中 配置 
        ///     a. Email_Complex 类型的红点条件 为 Email > 0
        ///     b. Email_UnRead 类型的 红点条件 为 Email_UnRead > 0
        ///     c. Email_UnReceived 类型的 红点条件 为 Email_UnReceived > 0
        /// 
        /// <summary>
        /// 邮箱的复合类型. 复合邮件类型 = 未读邮件类型 + 未领取邮件类型
        /// </summary>
        // Email_Complex,
        // Email_UnRead,
        // Email_UnReceived,
        //---------------------------------------------------------
        #endregion

        #region 聊天类型
        Chat = 60,

        #endregion

        /// 抽卡
        Gacha1 = 65,
        Gacha2 = 66,
        Gacha3 = 67,
        Gacha4 = 68,
        // 装备
        Equip = 70,
        // 伙伴
        Partner = 75,


        // 符文
        Rune = 80,
        // 冒险等级
        Adventure = 85,
        // 角色天赋
        Talent = 90,
        // 转职
        TransJob = 95,

        SkillBook = 100,//技能书

        Rank = 101,     // 排行榜
        DailyAct = 102, // 每日活动

        #region 商业化相关

        FirstCharge = 120,//首充奖励

        MoonCard,//月卡

        BattlePass,//战斗通行证

        BattlePass_Task,//战斗通行证任务
        #endregion

        #region 测试 类型, 用来测试 
        //入口
        Entrance = 10000,
        // 托入引用的 节点1
        SubType_Node1,
        // 托入引用的 节点2
        SubType_Node2,
        // 关联的类型 3

        SubType3,
        // 关联的类型 4
        SubType4,
        #endregion

        #region 伙伴
        //新增伙伴
        Partner_New = 20000,
        Partner_Team,
        //伙伴可以上阵  有空槽位 +有未上阵 未助阵伙伴
        Partner_CanInBattle,
        //伙伴可助战   有空槽位 +有未上阵 未助阵伙伴
        Partner_CanAssist,
        //伙伴可进阶
        Partner_Level,
        //伙伴装备
        Partner_Equip,
        //构筑
        Partner_Construct,
        //伙伴升级
        Partner_Upgrade,
        #endregion

        #region 公告红点
        /// <summary>
        /// 最外层的公告红点
        /// </summary>
        Announcement_Total = 30000,
        /// <summary>
        /// 公告活动按钮红点
        /// </summary>
        Announcement_Activity,
        /// <summary>
        /// 公告按钮
        /// </summary>
        Announcement,

        #endregion

        #region 技能界面
        /// <summary>
        /// 技能点的 红点
        /// </summary>
        SkillRedPoint = 40000,
        /// <summary>
        /// 技能界面 "新" 的标记
        /// </summary>
        SkillNewRedPoint = 40010,

        #endregion
        #region 玩法界面
        /// <summary>
        /// 个人爬塔领奖
        /// </summary>
        PlayRedPointPersonTowerAward = 50000,
        /// <summary>
        /// 冒险等级满足挑战等级
        /// </summary>
        PlayRedPointPersonTowerLevel = 50001,


        /// <summary>
        /// 环任务每天没有接取
        /// </summary>
        PlayRedPointRingTaskNotReceive = 50003,


        #region 个人秘境

        /// <summary>
        /// 个人秘境
        /// </summary>
        Secretarea = 50100,

        /// <summary>
        /// 秘境领取每日奖励
        /// </summary>
        SecretareaDailyReward = 50110,
        /// <summary>
        /// 秘境领取每日任务
        /// </summary>
        SecretareaDailyTask = 50111,

        /// <summary>
        /// 秘境成就奖励
        /// </summary>
        SecretareaAchieve = 50120,
        /// <summary>
        /// 秘境成就挑战层数奖励
        /// </summary>
        SecretareaAchieveFloor = 50121,
        /// <summary>
        /// 秘境成就活跃奖励
        /// </summary>
        SecretareaAchieveActive = 50122,
        /// <summary>
        /// 秘境成就挑战数奖励
        /// </summary>
        SecretareaAchieveChallenge = 50123,

        #endregion

        #endregion

        #region 装备相关
        /// <summary>
        /// 装备可以强化
        /// </summary>
        PlayRedPointEquipCanUpgrade = 60001,

        /// <summary>
        /// 有可以精炼的装备
        /// </summary>
        PlayRedPointEquipCanRefine = 60002,

        /// <summary>
        /// 有可以重构的装备
        /// </summary>
        PlayRedPointEquipCanRecast = 60003,
        #endregion

        #region 伙伴目标
        /// <summary>
        /// 可以领取的伙伴目标奖励
        /// </summary>
        PartnerTask = 70001,
        #endregion


        #region

        /// <summary>
        /// 有部位未穿戴装备且获取了新装备；
        /// </summary>
        Bag_Uequiped = 80001,

        #endregion

        #region 符文

        /// <summary>
        /// 护符未装备且有可装备护符
        /// </summary>
        Amulet_Uequiped = 90001,

        /// <summary>
        /// 装备护符上未装备纹章，且有可装备纹章
        /// </summary>
        Emb_Uequiped = 90002,

        /// <summary>
        /// 已装备护符满足打磨消耗需求
        /// </summary>
        Amulet_Polish = 90003,
        #endregion

        /// <summary>
        /// 战力红点
        /// </summary>
        Power = 100000,
        /// <summary>
        /// 新手目标
        /// </summary>
        BeginnerTarget = 100001,
    }

    /// <summary>
    /// 红点 的条件类型
    /// </summary>
    [LuaCallCSharp]
    [Hotfix]
    public enum RedPointConditionType
    {
        None,

        #region 拍卖相关的红点 条件类型
        /// 拍卖——工会
        Auction_Union = 20,
        /// 拍卖——世界
        Auction_World = 21,
        // 拍卖——待拍
        Auction_New = 22,
        #endregion


        #region 公会
        //公会收到申请请求
        Union_Apply = 31,

        //公会更新了公告
        Union_Notice = 32,

        //公会功能的状态发生了变更
        Union_ChangeFunc = 33,

        #endregion

        #region  好友相关的条件
        // 好友入口
        Friend = 35,

        /// <summary>
        /// 好友申请
        /// </summary>
        FriendRequest = 36,

        /// <summary>
        /// 好友通知
        /// </summary>
        FriendNotice = 37,

        #endregion


        #region 邮箱类型 对应的红点条件
        /// 邮箱类型 示例:
        /// 1. 邮箱Icon 上显示 包含数字的 红点;
        /// 2. 每次收到 新邮件时, 信息栏 显示 "新",阅读后清楚;
        /// 3. 未领取的邮件, 也显示 普通红点;
        /// 
        /// 综上:
        ///    1.邮箱 红点的显示条件 : Email_UnRead(未读邮箱) >0 或 Email_UnReceived(未领取 邮箱) > 0
        ///      红点数量 = Email_UnRead + Email_UnReceived ; 存入 类型为 RedPointCountType 的 RedPointCount 中
        ///    2.新 邮箱标签 的显示: Email_UnRead > 0;
        ///    3.未领取 邮箱标签: Email_UnReceived
        /// 
        /// note:
        ///     红点 系统应该只需要关心 最上层的 红点显示.
        ///     目前 策划 老想 红点系统 也处理 系统内部的 状态显示。 如邮件 未读和 未领取状态 的显示.
        ///     
        /// <summary>
        /// 邮箱 的条件类型,判断的是 未读 + 未领取 数量 > 0
        /// </summary>
        Email = 50,
        Mail_New = 51,
        Mail_Red = 52,
        // /// <summary>
        // /// 未读 邮箱 的条件类型
        // /// </summary>
        // Email_UnRead = 51,
        // /// <summary>
        // /// 未领取 邮箱 的条件类型
        // /// </summary>
        // Email_UnReceived = 52,

        #endregion

        #region 聊天类型
        Chat = 60,

        #endregion

        #region 抽卡类型
        Gacha1 = 70,
        Gacha2 = 71,
        Gacha3 = 72,
        Gacha4 = 73,
        #endregion

        #region 药品类型
        Medicine1 = 80, //战斗恢复
        Medicine2 = 81, //生命储备为空
        Medicine3 = 82, //法力储备为空
        #endregion

        // 冒险等级
        Adventure = 85,

        TeamGroup = 90, //队伍
        // 转职
        TransJob = 95,
        SkillBook = 100,//技能书

        Rank = 111,     // 排行榜
        DailyAct = 112,  // 日常活跃度

        #region 商业化相关

        FirstCharge = 120,//首充奖励

        MoonCard,//月卡

        BattlePass,//战斗通行证

        BattlePass_Task,//战斗通行证有未领取的任务


        #endregion

        #region 测试 类型 的条件, 用来测试 
        //入口
        Entrance = 10000,

        // 托入引用的 节点1
        SubType_Node1,
        // 托入引用的 节点2
        SubType_Node2,
        // 关联的类型 3

        SubType3,
        // 关联的类型 4
        SubType4,

        // SubType_Node2 节点 关联的 第二个类型
        SubType5,

        #endregion

        #region 伙伴红点
        Partner = 20000,
        //新伙伴
        Partner_New,
        Partner_Team,
        //伙伴可以上阵
        Partner_CanInBattle,
        //伙伴可助战
        Partner_CanAssist,
        //伙伴可进阶
        Partner_Level,
        //伙伴装备
        Partner_Equip,

        //伙伴升级
        Partner_UpGrade,

        //伙伴构筑，突破
        Partner_Construct,
        #endregion


        #region 伙伴红点
        Announcement_Total = 30000,
        /// <summary>
        /// 公告活动按钮红点
        /// </summary>
        Announcement_Activity,
        /// <summary>
        /// 公告按钮
        /// </summary>
        Announcement,

        #endregion

        #region 个人爬塔
        /// <summary>
        /// 是否可以领奖
        /// </summary>
        PersonTowerAward = 50000,
        /// <summary>
        /// 冒险等级满足挑战等级
        /// </summary>
        PersonTowerLevel = 50001,
        #endregion

        #region 环任务
        /// <summary>
        /// 环任务每天没有领取
        /// </summary>
        RingTaskNotReceive = 50002,
        #endregion

        #region 装备相关
        /// <summary>
        /// 有可以强化的装备
        /// </summary>
        EquipCanUpgrade = 60001,

        /// <summary>
        /// 有可以精炼的装备
        /// </summary>
        EquipCanRefine = 60002,
        #endregion

        #region 伙伴目标
        /// <summary>
        /// 可以领取的伙伴目标奖励
        /// </summary>
        PartnerTask = 70001,
        #endregion

        #region 背包

        /// <summary>
        /// 有部位未穿戴装备且获取了新装备；
        /// </summary>
        Bag_Uequiped = 80001,

        #endregion

        #region 符文

        /// <summary>
        /// 护符未装备且有可装备护符
        /// </summary>
        Amulet_Uequiped = 90001,

        /// <summary>
        /// 装备护符上未装备纹章，且有可装备纹章
        /// </summary>
        Emb_Uequiped = 90002,

        /// <summary>
        /// 已装备护符满足打磨消耗需求
        /// </summary>
        Amulet_Polish = 90003,
        #endregion
        /// <summary>
        /// 新手目标
        /// </summary>
        BeginnerTarget = 100001,
    }



    #endregion

    #region 重新登录状态
    [XLua.LuaCallCSharp]
    public enum AgainLoginType
    {
        None = 0,                   // 重新登录
        AgainLogin = 1,             // 重新登录
        BacktrackSelectRole = 2,    // 选角界面
        ChangeLanguage = 3,         // 改变语言
    }

    #endregion


    #region 系统开放 枚举
    // [LuaCallCSharp]
    // [Hotfix]
    // public enum SystemOpenType1
    // {
    //     None = 0,
    //     /// <summary>
    //     /// 解锁药品补给
    //     /// </summary>
    //     Potion = 1,
    //     /// <summary>
    //     /// 解锁玩家技能1
    //     /// </summary>
    //     SkillOne,
    //     /// <summary>
    //     /// 解锁玩家技能2
    //     /// </summary>
    //     SkillTwo,
    //     /// <summary>
    //     /// 开启伙伴-详情/阵容-出战
    //     /// </summary>
    //     Partner_Common,
    //     /// <summary>
    //     /// 开启 自动战斗
    //     /// </summary>
    //     AutoFight,
    //     /// <summary>
    //     /// 解锁玩家技能3
    //     /// </summary>
    //     SkillThree,
    //     /// <summary>
    //     /// 解锁玩家技能4
    //     /// </summary>
    //     SkillFour,

    //     /// <summary>
    //     /// 开启装备-强化
    //     /// </summary>
    //     Equip_Common,
    //     /// <summary>
    //     /// 开启首充
    //     /// </summary>
    //     FirstTopUp,
    //     /// <summary>
    //     /// 开启交易行
    //     /// </summary>
    //     Trade,
    //     /// <summary>
    //     /// 开启聊天功能
    //     /// </summary>
    //     Chat,
    //     /// <summary>
    //     /// 开启好友功能
    //     /// </summary>
    //     Friend,
    //     /// <summary>
    //     /// 开启邮件功能
    //     /// </summary>
    //     Mail,

    //     /// <summary>
    //     /// 开启玩法-日常
    //     /// </summary>
    //     Playmode_Daily,
    //     /// <summary>
    //     /// 开启冒险等级
    //     /// </summary>
    //     AdventureLV,
    //     /// <summary>
    //     /// 开启活动
    //     /// </summary>
    //     Activity,
    //     /// <summary>
    //     /// 开启商城功能及通用商店
    //     /// </summary>
    //     Shop_Common,
    //     /// <summary>
    //     /// 开启抽卡
    //     /// </summary>
    //     Gacha,
    //     /// <summary>
    //     /// 开启伙伴-阵容-助战
    //     /// </summary>
    //     Partner_Assist,
    //     /// <summary>
    //     /// 开启伙伴-进阶
    //     /// </summary>
    //     Partner_StarUP,
    //     /// <summary>
    //     /// 开启组队大厅
    //     /// </summary>
    //     TeamFind,

    //     /// <summary>
    //     /// 开启公会
    //     /// </summary>
    //     Guild,
    //     /// <summary>
    //     /// 开启玩法-周常
    //     /// </summary>
    //     Playmode_Weekly,
    //     /// <summary>
    //     /// 开启拍卖行
    //     /// </summary>
    //     Auction,

    //     /// <summary>
    //     /// 开启被动天赋
    //     /// </summary>
    //     ATTTree,
    //     /// <summary>
    //     /// 开启伙伴-装备
    //     /// </summary>
    //     Partner_Equip,
    //     /// <summary>
    //     /// 开启装备-精炼
    //     /// </summary>
    //     Equip_Refine,
    //     /// <summary>
    //     /// 开启装备-重构
    //     /// </summary>
    //     Equip_Reconst,
    //     /// <summary>
    //     /// 开启护符-装备/打磨
    //     /// </summary>
    //     Amulet_Common,
    //     /// <summary>
    //     /// 开启纹章
    //     /// </summary>
    //     Emblem,
    //     /// <summary>
    //     /// 开启护符-萃取
    //     /// </summary>
    //     Amulet_Extract,
    //     /// <summary>
    //     /// 解锁转职
    //     /// </summary>
    //     JobTransfer,

    //     /// <summary>
    //     /// 解锁奥义技能
    //     /// </summary>
    //     Ult,
    //     /// <summary>
    //     /// 开启个人秘境
    //     /// </summary>
    //     PersonSecret,
    //     /// <summary>
    //     /// 开启组队日常本 
    //     /// </summary>
    //     TeamDailyCopy,
    //     /// <summary>
    //     /// 开启通缉任务  
    //     /// </summary>
    //     TeamWanted,
    //     /// <summary>
    //     /// 开启单人日常本 
    //     /// </summary>
    //     DailyCopy,
    //     /// <summary>
    //     /// 开启个人爬塔  
    //     /// </summary>
    //     PersonalTower,
    //     /// <summary>
    //     /// 开启异步竞技场 
    //     /// </summary>
    //     Arena,
    //     /// <summary>
    //     /// 开启战场活动  
    //     /// </summary>
    //     BattleField,
    //     /// <summary>
    //     /// 开启梦境入侵
    //     /// </summary>
    //     PlayModeGVE,
    //     /// <summary>
    //     /// 开启鸣器
    //     /// </summary>
    //     Etch,
    //     /// <summary>
    //     /// 开启藏宝图
    //     /// </summary>
    //     Treasure,
    //     /// <summary>
    //     /// 开启午间活动
    //     /// </summary>
    //     GuildNoonActiv,
    //     /// <summary>
    //     /// 开启环任务-待配置
    //     /// </summary>
    //     RingTask,
    //     /// <summary>
    //     /// 开启野外BOSS
    //     /// </summary>
    //     FieldBoss,
    //     /// <summary>
    //     /// 转职
    //     /// </summary>
    //     TransferJob,
    //     /// <summary>
    //     /// 开启生活技能
    //     /// </summary>
    //     LifeSkill,
    //     /// <summary>
    //     /// 开启公会捐献
    //     /// </summary>
    //     Playmode_GuildDonate,

    // }


    [LuaCallCSharp]
    [Hotfix]
    public enum SystemOpenCondition
    {
        /// <summary>
        /// 角色等级
        /// </summary>
        RoleLevel = 1,
        /// <summary>
        /// 冒险等级
        /// </summary>
        AdvanceLevel,
        /// <summary>
        /// 任务等级
        /// </summary>
        TaskLevel,
    }

    #endregion

    #region 系统优化设置相关
    public enum GameObjectPoolType
    {
        ActiveType,//显示隐藏
        CanvasGroupType,//通常性能最优先
        PosType,//挪动很远
        ScaleType//变成0

    }

    public enum E_Render_PRI
    {
        //【这里有优先级】【高优先出现低优先会被立刻中断】【上限一共就50个】
        PlayerATTACK_FX,           // 玩家攻击效果
        PlayerHIT_FX,               // 玩家受到攻击效果
        TeammateATTACK_FX,         // 玩家伙伴攻击效果
        TeammateHIT_FX,             // 玩家伙伴受到攻击效果



        PartyATTACK_FX,             // 队友攻击效果
        PartyHIT_FX,                // 队友受到攻击效果
        GuildATTACK_FX,             // 工会攻击效果
        GuildHIT_FX,                 // 工会受到攻击效果

        Other_Player_FX,            // 其他玩家(不是一个队伍也不是一个工会的玩家)
        Other                       // 其他
    }

    #endregion

    #region 多语言

    public enum LanguageType
    {
        None,
        Chinese,
        English
    }


    #endregion

    /// <summary>
    /// Loading 菊花类型, 不同的类型显示不一样的菊花
    /// </summary>
    [Hotfix]
    public enum LoadingWidgetTypeEnum
    {
        /// <summary>
        /// 默认菊花状态
        /// </summary>
        Default,
        /// <summary>
        /// 消息回复超时
        /// </summary>
        MsgOverTime,
        /// <summary>
        /// 弱网状态1 (转点)
        /// </summary>
        WeakConnect1,
        /// <summary>
        /// 弱网状态2 (转圈)
        /// </summary>
        WeakConnect2,
        /// <summary>
        /// 等待响应
        /// </summary>
        AwaitResponse,

    }

    #region  本地缓存类型的枚举
    [XLua.LuaCallCSharp]
    public enum CacheType
    {
        /// <summary>
        /// 普通的 缓存类型, 默认下一帧 存入本地
        /// </summary>
        Normal,
        /// <summary>
        /// 立即 存储的缓存类型, 一般不需要, 只有特别重要的 需要立即存储的类型 才需要立即落盘
        /// </summary>
        Immediately,
        /// <summary>
        /// 延迟 存储的数据类型, 目前 默认 100ms 落盘
        /// </summary>
        Delay,
    }
    #endregion

    /// <summary>
    /// 资源释放的类型.
    /// 默认 的资源 在 map 和 scene 变化的时候，都会去触发释放逻辑.
    /// 特殊的 有些资源 只有 切场景的时候 才释放 ，但 切换 map 不是放.
    /// 
    /// 另:
    ///     主角的类似 特效资源， 不管且地图 或者切场景, 都不需要释放
    ///     只有返回 登录界面的时候, 才需要被释放
    /// </summary>
    public enum ResoruceReleaseType
    {
        /// <summary>
        /// 场景地图 和 服务器ID 切换的时候，都需要触发释放逻辑
        /// </summary>
        MapSceneAndServerID,
        /// <summary>
        /// 地图 和 场景切换的时候，都需要触发释放逻辑。 目前来说 类似于 地图的 prefab 就是 MapAndScene 资源类型.
        /// </summary>
        MapAndScene,
        /// <summary>
        /// 只有切换场景的时候, 才需要释放，用于 跨地图的 资源
        /// </summary>
        Scene,

        /// <summary>
        /// 强制释放才能释放的资源. 强制释放 目前应该只有返回登录才会这样.
        /// 对于 主角的 特效而言, 且场景和 地图都不需要释放, 所以可以指定为这个类型
        /// </summary>
        Force,
    }

    /// <summary>
    /// 尝试 释放资源的 类型
    /// </summary>
    public enum TryReleaseResouceType
    {
        /// <summary>
        /// 尝试切线, 只需要释放 AOI 相关的资源, 但是地图 这种可以不需要释放
        /// </summary>
        ServerID,
        /// <summary>
        /// 尝试释放 地图相关的 资源类型
        /// </summary>
        Map,
        /// <summary>
        /// 尝试释放 scene 相关的 资源类型
        /// </summary>
        Scene,
        /// <summary>
        /// 强制释放才能释放的资源. 强制释放 目前应该只有返回登录才会这样.
        /// 对于 主角的 特效而言, 且场景和 地图都不需要释放, 所以可以指定为这个类型
        /// </summary>
        Force,


    }

    /// <summary>
    /// 显示效果的 刷新 类型
    /// </summary>
    public enum TypeEffectUpdateType
    {
        /// <summary>
        /// 效果进入
        /// </summary>
        OnEnter,
        /// <summary>
        /// 效果更新
        /// </summary>
        OnUpdate,
        /// <summary>
        /// 效果退出
        /// </summary>
        OnExit,

    }

    /// <summary>
    /// 战力模块 枚举
    /// </summary>
    [XLua.LuaCallCSharp]
    public enum FightPowerModuleEnum
    {
        Role = 100, // 角色
        Equip = 200, // 装备

        Skill = 300, // 技能
        Partner = 400,// 伙伴

        Role_GrowUp = 101, // 冒险成长    AdvGradeExp	LevelExp
        Role_AttriTT = 102, // 星灵     AttriTTNode
        //  = #103,饰品主属性
        //  = #104,饰品副属性
        //  = #105,饰品套装属性

        Equip_Slot = 201, //  强化槽位+共鸣战力      EquipSlotIntensify
        Equip_Sub = 202, // 主属性战力      EquipSub	SubCurve
        Equip_Rebuild = 203, // 装备重构       EquipSub
        Equip_Heraldry = 204, // 日月纹章      HeraldryEquip

        Skill_Level = 301, // 角色技能      SkillLevel
        Skill_Passive = 302, // 鸣魄战技      PassiveDesc

        Partner_Playing = 401, // 伙伴出战      PartnerNature	ParConversion
        Partner_Assist = 402, // 伙伴助战      Assist
        Partner_Star = 403, // 伙伴醒灵      StarExpend	PartnerSkillDesc
        Partner_Equip = 404, // 定影魂石      PartnerEquip



    }

    /// <summary>
    /// 登录配置相关枚举,不同的 登录渠道 对应的不同的枚举
    /// </summary>
    public enum E_LoginCfgEnum
    {
        Star_Dev = 1,       // 内网开发环境
        OutSide_Test,   // 外网测试
        Version_Test,   // 版署,
        Romania,        // 罗马尼亚
        Philippines,    // 菲律宾


    }

    public enum E_FindPathType
    {
        None,           // 空类型
        Normal,         // 通用的寻路类型
        AutoBattle,     // 自动战斗寻路类型
        AutoBattle_Spawner,     // 自动战斗寻路到副本目标类型, 
        AutoBattle_MonsterPoint,     // 自动战斗寻路到副本怪物点
        AutoBattle_Move2Pos,     // 自动战斗 设置的 移动到指定点坐标
        Follow,         // 跟随目标寻路类型

    }

    /// <summary>
    /// 活动 倒计时类型
    /// </summary>
    public enum E_EventTimeType
    {

    }
}