//AutoGenerate from: 新手引导表_PlayerGuidance.xlsm.xlsx  sheet:GuidSysOpen
namespace StarProjectDef
{
	[XLua.LuaCallCSharp]
	public enum SystemOpenType
	{
		None = 0,
		/// <summary>
		/// 解锁药品补给
		/// </summary>
		Potion = 1,
		/// <summary>
		/// 解锁玩家技能1
		/// </summary>
		SkillOne = 2,
		/// <summary>
		/// 解锁玩家技能2
		/// </summary>
		SkillTwo = 3,
		/// <summary>
		/// 开启伙伴-详情-阵容-出战
		/// </summary>
		Partner_Common = 4,
		/// <summary>
		/// 解锁自动战斗
		/// </summary>
		AutoFight = 5,
		/// <summary>
		/// 解锁玩家技能3
		/// </summary>
		SkillThree = 6,
		/// <summary>
		/// 解锁玩家技能4
		/// </summary>
		SkillFour = 7,
		/// <summary>
		/// 开启装备-强化
		/// </summary>
		Equip_Common = 8,
		/// <summary>
		/// 开启首充
		/// </summary>
		FirstTopUp = 9,
		/// <summary>
		/// 开启交易及交易行
		/// </summary>
		Trade = 10,
		/// <summary>
		/// 开启商城功能及通用商店
		/// </summary>
		Shop_Common = 11,
		/// <summary>
		/// 开启聊天功能及聊天框
		/// </summary>
		Chat = 12,
		/// <summary>
		/// 开启好友功能
		/// </summary>
		Friend = 13,
		/// <summary>
		/// 开启邮件功能
		/// </summary>
		Mail = 14,
		/// <summary>
		/// 开启组队功能
		/// </summary>
		TeamFind = 21,
		/// <summary>
		/// 开启玩法-日常
		/// </summary>
		Playmode_Daily = 15,
		/// <summary>
		/// 开启冒险等级
		/// </summary>
		AdventureLV = 16,
		/// <summary>
		/// 开启活动
		/// </summary>
		Activity = 17,
		/// <summary>
		/// 开启抽卡
		/// </summary>
		Gacha = 18,
		/// <summary>
		/// 开启伙伴-阵容-助战
		/// </summary>
		Partner_Assist = 19,
		/// <summary>
		/// 开启伙伴-进阶
		/// </summary>
		Partner_StarUP = 20,
		/// <summary>
		/// 开启公会
		/// </summary>
		Guild = 22,
		/// <summary>
		/// 开启玩法-周常
		/// </summary>
		Playmode_Weekly = 23,
		/// <summary>
		/// 开启拍卖行
		/// </summary>
		Auction = 24,
		/// <summary>
		/// 开启被动天赋
		/// </summary>
		ATTTree = 25,
		/// <summary>
		/// 开启伙伴-装备
		/// </summary>
		Partner_Equip = 26,
		/// <summary>
		/// 开启装备-精炼
		/// </summary>
		Equip_Refine = 27,
		/// <summary>
		/// 开启装备-重构
		/// </summary>
		Equip_Reconst = 28,
		/// <summary>
		/// 开启纹章（护符）-装备-打磨
		/// </summary>
		Amulet_Common = 29,
		/// <summary>
		/// 开启星次石
		/// </summary>
		Emblem = 30,
		/// <summary>
		/// 开启护符-萃取
		/// </summary>
		Amulet_Extract = 31,
		/// <summary>
		/// 解锁奥义技能
		/// </summary>
		Ult = 33,
		/// <summary>
		/// 开启拟态梦境
		/// </summary>
		PersonSecret = 34,
		/// <summary>
		/// 开启蚀梦之影
		/// </summary>
		TeamDailyCopy = 35,
		/// <summary>
		/// 开启侵蚀裂隙
		/// </summary>
		TeamWanted = 36,
		/// <summary>
		/// 开启时序残响
		/// </summary>
		DailyCopy = 37,
		/// <summary>
		/// 开启空想之环
		/// </summary>
		PersonalTower = 38,
		/// <summary>
		/// 开启胜者为王
		/// </summary>
		Arena = 39,
		/// <summary>
		/// 开启风暴对抗
		/// </summary>
		BattleField = 40,
		/// <summary>
		/// 开启梦境入侵
		/// </summary>
		PlayModeGVE = 41,
		/// <summary>
		/// 开启鸣器
		/// </summary>
		Etch = 42,
		/// <summary>
		/// 开启藏宝图
		/// </summary>
		Treasure = 43,
		/// <summary>
		/// 开启无畏之战
		/// </summary>
		GuildNoonActiv = 44,
		/// <summary>
		/// 开启研究手册
		/// </summary>
		RingTask = 45,
		/// <summary>
		/// 开启协会讨伐
		/// </summary>
		FieldBoss = 46,
		/// <summary>
		/// 转职
		/// </summary>
		TransferJob = 47,
		/// <summary>
		/// 开启生活技能
		/// </summary>
		LifeSkill = 48,
		/// <summary>
		/// 开启协会募集
		/// </summary>
		Playmode_GuildDonate = 49,
		/// <summary>
		/// 解锁目标选择
		/// </summary>
		TargetSelect = 50,
		/// <summary>
		/// 开启技能
		/// </summary>
		Skill = 51,
		/// <summary>
		/// 开启月卡
		/// </summary>
		Activ_MonthCard = 52,
		/// <summary>
		/// 开启冒险企划
		/// </summary>
		Activ_BattlePass = 53,
		/// <summary>
		/// 开启签到
		/// </summary>
		Activ_SignIn = 54,
		/// <summary>
		/// 开启七日目标
		/// </summary>
		Activ_SevenDay = 55,
		/// <summary>
		/// 开启道具合成
		/// </summary>
		Item_Compose = 56,
		/// <summary>
		/// 开启梦之镜
		/// </summary>
		WorldLine = 57,
		/// <summary>
		/// 开启玩法预告
		/// </summary>
		FunctionPreview = 58,
		/// <summary>
		/// 在线奖励
		/// </summary>
		TimeReward = 59,
		/// <summary>
		/// 伙伴目标
		/// </summary>
		PartnerTarget = 60,
		/// <summary>
		/// 排行榜
		/// </summary>
		Rank = 61,
		/// <summary>
		/// 战力
		/// </summary>
		Power = 62,
		/// <summary>
		/// 日常活跃度
		/// </summary>
		DailyPlayActiv = 63,
		/// <summary>
		/// 派对时刻
		/// </summary>
		PartyTime = 64,
		/// <summary>
		/// 跑马灯
		/// </summary>
		Marquee = 65,
		/// <summary>
		/// 技能天赋
		/// </summary>
		SkillTalent = 66,
		/// <summary>
		/// 冒险旅程
		/// </summary>
		BeginnerTarget = 67,
		/// <summary>
		/// 开启研究手册-正式指引
		/// </summary>
		RingTask_Guidance = 68,
		/// <summary>
		/// 开启伙伴-资质
		/// </summary>
		Partner_Qualification = 69,
		/// <summary>
		/// 开启伙伴-升级
		/// </summary>
		Partner_LevelUp = 70,
	}
}
/**
static public IEnumerable e_systemopentypes = new ValueDropdownList<SystemOpenType>()
{
	{ "不显示", SystemOpenType.None }},
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
	{ "开启纹章（护符）-装备-打磨", SystemOpenType.Amulet_Common },
	{ "开启星次石", SystemOpenType.Emblem },
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
}
**/