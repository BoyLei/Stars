using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarProjectDef;
using UnityEngine;

[XLua.LuaCallCSharp]
public static class TutorialDefine
{
    // static public List<Vector2Int> ArrowDirection = new List<Vector2Int>();
    public static Vector2Int center = new Vector2Int(0, 0);
    public static Vector2Int right = new Vector2Int(1, 0);
    public static Vector2Int right_down = new Vector2Int(1, -1);
    public static Vector2Int down = new Vector2Int(0, -1);
    public static Vector2Int left_down = new Vector2Int(-1, -1);
    public static Vector2Int left = new Vector2Int(-1, 0);
    public static Vector2Int left_up = new Vector2Int(-1, 1);
    public static Vector2Int up = new Vector2Int(0, 1);
    public static Vector2Int right_up = new Vector2Int(1, 1);


    static public IEnumerable arrowdirections = new ValueDropdownList<Vector2Int>()
    {
        { "中心", center },
        { "右", right },
        { "右下", right_down },
        { "下", down },
        { "左下", left_down },
        { "左", left },
        { "左上", left_up },
        { "上", up },
        { "右上", right_up },
    };

    static public IEnumerable e_tutorialconditiontypes = new ValueDropdownList<E_TutorialConditionType>()
    {
        { "界面开启", E_TutorialConditionType.OpenUI },
        { "完成引导", E_TutorialConditionType.FinishTutorial },
        { "完成任务", E_TutorialConditionType.FinishTask },
        { "系统开启", E_TutorialConditionType.SystemOpen },
        { "进入场景", E_TutorialConditionType.EntryScene },
        { "退出场景", E_TutorialConditionType.ExitScene }
    };

    static public IEnumerable e_tutorialtypes = new ValueDropdownList<E_TutorialType>()
    {
        { "强引导", E_TutorialType.ForceGuide },
        { "弱引导-侧边弹窗", E_TutorialType.SidePopWindow },
        { "弱引导-飘字", E_TutorialType.FloatingText },
        { "弱引导-轮播图", E_TutorialType.LoopImage },
    };

    static public IEnumerable e_fingertypes = new ValueDropdownList<E_FingerType>()
    {
        { "手指", E_FingerType.Finger },
        { "拖动", E_FingerType.Drag },
        { "长按", E_FingerType.LongPress },
    };

    static public IEnumerable e_tutorialtargettypes = new ValueDropdownList<E_TutorialTargetType>()
    {
        { "无", E_TutorialTargetType.None },
        { "UI对象", E_TutorialTargetType.UI },
        { "场景对象", E_TutorialTargetType.Entity },
    };

    static public IEnumerable e_tutorialcompletetypetypes = new ValueDropdownList<E_TutorialCompleteTypeType>()
    {
        { "点击", E_TutorialCompleteTypeType.Click },
        { "接收事件", E_TutorialCompleteTypeType.ReciveEvent },
        { "长按", E_TutorialCompleteTypeType.LongPress },
        { "点击屏幕", E_TutorialCompleteTypeType.ClickScreen },
        { "延迟自动完成(单位毫秒)", E_TutorialCompleteTypeType.Delay },
    };

    static public IEnumerable e_tutorialpointtypes = new ValueDropdownList<E_TutorialPointType>()
    {
        { "非断点", E_TutorialPointType.None },
        { "中断回到起始位置", E_TutorialPointType.PointBreak },
        { "中断继续", E_TutorialPointType.PointContinue },
    };

    static public IEnumerable e_maskshapetypes = new ValueDropdownList<E_MaskShapeType>()
    {
        { "不显示", E_MaskShapeType.None },
        { "四边形", E_MaskShapeType.Square },
        { "圆形", E_MaskShapeType.Circle }
    };

    static public IEnumerable e_textstyles = new ValueDropdownList<E_TextStyle>()
    {
        { "不显示", E_TextStyle.None },
        { "带箭头文本框", E_TextStyle.WithArrow },
        { "侧滑栏", E_TextStyle.SidePop },
        { "飘字", E_TextStyle.FloatingText }
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

    static public IEnumerable e_eventdefines = new ValueDropdownList<E_EventDefine>()
    {
        { "释放技能(参数技能槽位)", E_EventDefine.ReleaseSkill },
        { "引导图片(参数为配置表的Event)", E_EventDefine.GuideImage },
        { "拖动摇杆", E_EventDefine.DragStick },
        { "点击世界地图", E_EventDefine.ClickWorldMap }
    };
}


public enum E_TutorialConditionType
{
    OpenUI = 0, //---界面开启
    FinishTutorial = 1, //---完成引导
    FinishTask = 2, //---完成任务
    SystemOpen = 3, //系统开放
    EntryScene = 4, //进入场景
    ExitScene = 5, //退出场景
}

public enum E_TutorialType
{
    ForceGuide = 0, //---强引导
    SidePopWindow = 1, //---弱引导-侧边弹窗
    FloatingText = 2, //---弱引导-飘字
    LoopImage = 3, //---弱引导-轮播图
}

public enum E_FingerType
{
    Finger = 0, //手指
    Drag = 1, //拖动
    LongPress = 2, //长按
}

public enum E_TutorialTargetType
{
    None = 0, //无
    UI = 1, //UI
    Entity = 2, //场景对象
}

public enum E_TutorialCompleteTypeType
{
    Click = 0, //点击
    ReciveEvent = 1, //接收事件
    LongPress = 2, //长按
    ClickScreen = 3, //点击屏幕
    Delay = 4, //延迟
}


public enum E_TutorialPointType
{
    None = 0, //非断点
    PointBreak = 1, //中断回到起始位置
    PointContinue = 2, //中断继续
}

public enum E_MaskShapeType
{
    None = 0, //没有形状
    Square = 1, //四边形
    Circle = 2 //圆形
}

/// <summary>
/// 文本框样式
/// </summary>
public enum E_TextStyle
{
    None = 0, //无文字
    WithArrow = 1, //带箭头
    SidePop = 2, //侧滑栏
    FloatingText = 3, //飘字
}

public enum E_EventDefine
{
    ReleaseSkill = 0, //释放技能
    GuideImage = 1, //引导图片
    DragStick = 2, //拖动摇杆
    ClickWorldMap = 3, //点击世界地图
}