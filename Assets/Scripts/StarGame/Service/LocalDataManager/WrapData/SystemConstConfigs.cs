//AutoGenerate from: System.xlsx  sheet:System
public static class SystemConstConfigs
{
	//好友人数上限
	public const int System_FriendMAx = 80;
	//好友申请列表过期时间（秒）
	public const int System_FriendRequestOutTime = 864000;
	//好友申请列表上限
	public const int System_FriendRequestMax = 20;
	//怪物通用技能（自杀），怪物加载时自动学习此技能，生存时间到期后自杀
	public const int System_MonsterSuicideSkills = 6004;
	//原Monster表中的LoopInterval字段；决定了怪物的除了AI之外的Update,同步AOI.
	public const int System_Sync_AOI = 200;
	//进入镜像延迟行为树开始时间
	public const int System_MirrorLevel_Delay = 2000;
	//创建角色进入场景
	public const int System_FirstEnterSpaceID = 9;
	//创建角色进入场景区域位置
	public const int System_FirstEnterAreaID = 7;
	//新手关
	public const int FirstLevel = 10006;
	//战斗状态恢复时间
	public const int System_BattleStateCountDown = 2;
	//移动速度最小值（厘米）
	public const int SYSTEM_MOVE_SPEED_MIN = 60;
	//移动速度最大值（厘米）
	public const int SYSTEM_MOVE_SPEED_MAX = 1500;
	//伙伴默认召唤与人的角度
	public const int System_PartnerBornAngle = 120;
	//伙伴默认召唤与人的距离
	public const int System_PartnerBornDistance = 150;
	//角色气血药品存储上限
	public const int System_Drug_MxpHP = 10000000;
	//角色法力药品存储上限
	public const int System_Drug_MxpMP = 10000000;
	//脱战后，每秒回复自身%生命上限的生命
	public const int System_Recover_MxpHP = 10;
	//脱战后，每秒回复自身%法力上限的法力
	public const int System_Recover_MxpMP = 10;
	//伙伴死亡后，自动复活时间
	public const int System_PartnerRebornTime = 60000;
	//每日生成任务数量（环任务）
	public const int RingTask_TaskNum = 3;
	//环任务领奖任务
	public const int RingTask_AwardTask = 1073741824;
	//每日刷新时间
	public const int System_DailyReset = 18000;
	//日常副本结束延迟秒数
	public const int DailyCopy_Delay = 15;
	//初始进度值
	public const int DefaultProgress = 0;
	//满进度值
	public const int FullProgress = 140;
	//每日额外宝箱次数
	public const int DailyExtraBox = 0;
	//午间公会副本，个人日常本，个人爬塔副本限制时间
	public const int SecretCopy_TimeLimit = 900;
	//公会拍卖竞拍时间
	public const int GuildAuctionTime = 900;
	//世界拍卖竞拍时间
	public const int WorldAuctionTime = 900;
	//单次加价百分比%
	public const int MarksUp = 5;
	//主动刷新CD
	public const int RefreshCD = 30;
	//分红总额比例%
	public const int Divvy = 100;
	//日常副本倒计时
	public const int DailyCopy_TimeLimit = 900;
	//个人秘境结算后自动退出倒计时（秒）
	public const int SecretCopy_ExitTime = 120;
	//一次通知队伍数量
	public const int NoticeTeamNum = 10;
	//最大邮件存储数量
	public const int MailLimit = 300;
	//组队日常本奖励怪出现概率
	public const int TeamDailyCopy_RewardOdds = 2;
	//组队日常本单服单日奖励怪出现上限
	public const int TeamDailyCopy_RewardNumLimit = 10;
	//组队日常本复活等待时间
	public const int TeamDailyCopy_RiseTime = 15;
	//组队日常本副本时间限制
	public const int TeamDailyCopy_CopyTimeLimit = 360;
	//公会事件数量
	public const int GuildRecordLimit = 30;
	//重新加入公会CD
	public const int ChangeGuildCD = 3600;
	//申请加入公会的个数上限
	public const int GuildApplyLimit = 30;
	//组队日常本入口npc（通用）
	public const int TeamDailCopy_EntryNPC = 200001;
	//组队日常本召集等待超时限制时间
	public const int TeamDailCopy_TeamConfirm = 15;
	//公会改名CD
	public const int GuildChangeNameCD = 3600;
	//公会改公告CD
	public const int GuildChangeNoticeCD = 3600;
	//入会申请接收数量上限
	public const int GuildApplyReceiveLimit = 20;
	//入会申请生效时间
	public const int GuildApplyEffectTime = 3600;
	//重构消耗道具（待废弃）
	public const int RebuildItem = 1;
	//重构消耗道具数量（待废弃）
	public const int RebuildItemNum = 50;
	//重构高等级出现概率
	public const int HigherLevelWeight = 40;
	//重构高品质出现概率
	public const int HigherQualityWeight = 60;
	//重构普通属性池概率
	public const int CommonWeight = 40;
	//重构填充属性池概率
	public const int RareWeight = 60;
	//重构记录保存数量
	public const int RebuildSaveLimit = 30;
	//冒险等级经验暂存数量上限
	public const int AdvExpSaveLimit = 10000;
	//冒险等级当前版本开放上限
	public const int AdvLevelCap = 48;
	//通缉挑战时队员范围限制
	public const int TeamWantedChallengeDistance = 15;
	//通缉每日领奖次数
	public const int TeamWantedAwardLimits = 3;
	//通缉怪物挑战时间
	public const int TeamWantedTimeLimits = 10;
	//通缉挑战申请倒计时
	public const int TeamWantedApplyCountDown = 10;
	//单区域天赋重置银币消耗（待废弃）
	public const int ATTTZoneResetConsume = 5000;
	//整树天赋重置银币消耗（待废弃）
	public const int ATTTWholeResetConsume = 12000;
	//自动加点需要的最小剩余天赋点量
	public const int ATTTMinAddPointConsume = 20;
	//拍卖行开启后等待期
	public const int AuctionStartDelay = 300;
	//无畏之战战斗时间限制
	public const int GNActivBoss_BattleLimit = 60;
	//无畏之战BUFF选择次数
	public const int GNActivBoss_BUFFCount = 2;
	//无畏之战排行榜刷新间隔
	public const int GNActivBoss_RankRefresh = 2;
	//无畏之战挑战间隔
	public const int GNActivBoss_BattleCD = 10;
	//玩家关注道具列表数
	public const int TradeFollowListNum = 20;
	//玩家关注道具实例列表数
	public const int TradeFollowInstanceNum = 20;
	//刷新CD（秒）
	public const int TradeRefreshCD = 10;
	//购买及出售记录最大保存数
	public const int TradeMaxRecordNum = 10;
	//买方上架银币税率（/100）
	public const int TradePutawayTax = 4;
	//买方交易金币税率（/100）
	public const int TradeSuccessTax = 5;
	//公示期时长（分钟）
	public const int TradePublicPeriod = 5;
	//上架时长（分钟）
	public const int TradePutawayPeriod = 1440;
	//交易锁定时长（小时）
	public const int TradeLockPeriod = 36;
	//兑换税率刷新时长（分钟)
	public const int CurrExRateRefreshPeriod = 60;
	//玩家日兑换绑钻映射
	public const int MaxBindingExNum = 16;
	//邮件发送延迟基础值（秒）
	public const int CurrExMailSendBaseTime = 180;
	//邮件发送延迟浮动值（随机然后与基础值相加）
	public const int CurrExMailSendFloatTime = 240;
	//初始钻比金汇率（10:X）
	public const int CurrExInitialRate = 5000;
	//触发汇率变动的钱钻比例（/1000）
	public const int CurrExRateChangeTrigger = 10;
	//每次变动比例（/1000）
	public const int CurrExRateChangePerTime = 10;
	//每周期上涨上限变动值
	public const int CurrExPerCycleRiseLimit = 40;
	//每周期下跌上限变动值
	public const int CurrExPerCycleFallLimit = 50;
	//日汇率上涨变动比率（/100）（1+X）
	public const int CurrExDailyRiseLimit = 12;
	//日汇率下跌变动比率（/100）(1-X)
	public const int CurrExDailyFallLimit = 15;
	//初始解锁货架数量
	public const int TradePutawayInitialNum = 18;
	//货架解锁消耗货币数（待废弃）
	public const int TradeUnlockShelfConsume = 50000;
	//付费可增加货架数量
	public const int TradePutawayMaxNum = 0;
	//无畏之战通用次数映射
	public const int GNActivBoss_CommonCount = 15;
	//X天未成交则使用道具价值算指导价
	public const int TradeUseItemValuePeriod = 5;
	//大地图最小缩放比率(/1000)
	public const int LMapMinZoomRatio = 1000;
	//大地图最大缩放比率(/1000)
	public const int LMapMaxZoomRatio = 3200;
	//世界地图最小缩放比率(/1000)
	public const int WMapMinZoomRatio = 400;
	//世界地图最大缩放比率(/1000)
	public const int WMapMaxZoomRatio = 950;
	//战场阵营人数限制
	public const int BattleField_CampLimit = 10;
	//战场报名时间
	public const int BattleField_EnrollTime = 300;
	//战场准备时间
	public const int BattleField_PrepareTime = 30;
	//战场副本限时
	public const int BattleField_BattleLimit = 10;
	//战场BUFF获取距离判定
	public const int BattleField_BUFFDistance = 1;
	//战场复活等待时间
	public const int BattleField_RebirthTime = 3;
	//重构消耗（关联Cost表）
	public const int RebuildItem_Cost = 10006;
	//单区域天赋重置消耗（关联Cost表）
	public const int ATTTZoneResetConsume_Cost = 10007;
	//整树天赋重置消耗（关联Cost表）
	public const int ATTTWholeResetConsume_Cost = 10008;
	//货架解锁消耗货币数（关联Cost表）
	public const int TradeUnlockShelfConsume_Cost = 10009;
	//通缉目标3阶副本复活等待时间
	public const int TeamWantedCopy_RiseTime = 10;
	//心灵传讯-基础发送时延(ms)
	public const int Tele_Dialogue_Send_Delay = 2500;
	//战场BOSS
	public const int BattleField_BOSSID = 1030101;
	//战场BOSS第1次生成时间
	public const int BattleField_BOSSTime1 = 180;
	//战场BOSS第2次生成时间
	public const int BattleField_BOSSTime2 = 360;
	//战场BOSS生成提示（BOSS生成XX秒前提示）
	public const int BattleField_BOSSWarnTime = 10;
	//战场结束提示1（战场结束XX秒前提示）
	public const int BattleField_EndWarnTime1 = 180;
	//战场结束提示2（战场结束XX秒前提示）
	public const int BattleField_EndWarnTime2 = 60;
	//战场结束提示3（战场结束XX秒前提示）
	public const int BattleField_EndWarnTime3 = 30;
	//竞技场_刷新对手CD
	public const int Arena_RefreshRivalCD = 60;
	//竞技场_付费购买道具
	public const int Arena_BuyCostItem = 3;
	//竞技场_准备时间
	public const int Arena_PrepareTime = 3;
	//竞技场_单局限时
	public const int Arena_RoundTime = 90;
	//竞技场_初始排名
	public const int Arena_DefaultRank = 3000;
	//竞技场_结算时间
	public const int Arena_SettlementTime = 79200;
	//竞技场_战报保存数量
	public const int Arena_ReportNum = 50;
	//组队日常本入口自动销毁时间
	public const int TeamDailCopy_EntryNPCDelete = 600;
	//战场结束倒计时提示4（战场结束3秒前倒计时动效）
	public const int BattleField_EndWarnTime4 = 3;
	//月卡每次购买增加有效期
	public const int MonthCardPlusValidity = 30;
	//战场结算自动退出副本倒计时
	public const int BattleField_ExitTime = 60;
	//每周经验获取数量上限
	public const int BP_WeeklyExpLimit = 4000;
	//通行证等级购买消耗（关联Cost表）
	public const int BP_GradeUnitPrice = 10010;
	//通行证付费-高级档位赠送等级
	public const int BP_PresentGrade = 10;
	//入队申请数量上限
	public const int TeamApplyReceiveLimit = 10;
	//单账号创建角色上限
	public const int CreateCharacterLimit = 5;
	//野外BOSS宝箱自动开启时间
	public const int FieldBoss_BoxAutoOpen = 8;
	//野外BOSS宝箱消失时间
	public const int FieldBoss_BoxDisappear = 8;
	//野外BOSS稀有奖励提示要求
	public const int FieldBoss_ItemQualityNotice = 4;
	//野外BOSSMVP动效持续时间
	public const int FieldBoss_MVPEffectTime = 5;
	//公会每日踢人人数限制
	public const int Guild_DailyKickPlayerLimit = 10;
	//免费刷新次数
	public const int Donate_FreeRefreshCount = 2;
	//付费刷新单次消耗绑钻数
	public const int Donate_RefreshCost = 30;
	//补签消耗（关联Cost表）
	public const int SignIn_MakeUp = 10011;
	//每日抽卡次数限制（关联次数表）
	public const int DailyGachaTimesLimit = 19;
	//玩家活力上限
	public const int LifeSkill_EnergyUpperLimit = 1500;
	//恢复活力频率（秒）
	public const int LifeSkill_EnergyRefreshInterval = 432;
	//系统解锁初始活力
	public const int LifeSkill_EnergySystemUnlockGet = 200;
	//活力转换经验比例（经验=活力*x）
	public const int LifeSkill_EnergyTransEXRatio = 10;
	//制造读条时长（秒）
	public const int LifeSkill_CreateLoadingTime = 2;
	//采集读条时长（秒）
	public const int LifeSkill_MineLoadingTime = 3;
	//公会邀请加入间隔CD（秒）
	public const int Guild_JoinInviteInterval = 300;
	//新手无输入触发任务高亮间隔（秒）
	public const int Guidance_NoInputTaskHLInterval = 40;
	//监听无输入高亮结束监听任务
	public const int Guidance_NoInputHLEndTask = 268632077;
	//野外boss掉落宝箱缩放大小（配置值=缩放大小*100）
	public const int FieldBoss_BoxSize = 300;
	//七日目标持续时间(可领奖日期、天数)
	public const int SevenDaysTarget_Days = 15;
	//转职特效
	public const int TransJobPassive = 1151;
	//切换伙伴上阵CD（秒）
	public const int Chanage_Partner_SeatCD = 5;
	//攻击护符槽位解锁条件（等级）
	public const int AtkAmulet_Unlock = 1;
	//防御护符槽位解锁条件（等级）
	public const int DefAmulet_Unlock = 38;
	//轮询拉去区服消息（秒）
	public const int Loop_Get_GroupList_Req = 20;
	//抽卡绑钻购买商品ID
	public const int Gacha_BuyGoodsID = 18;
	//钻石购买绑钻汇率（1：X)
	public const int Currency_DiamondBuyBindDiaRate = 1;
	//绑钻购买银币汇率（1：X)
	public const int Currency_BindDiamondBuySilverRate = 1000;
	//新手未完成主线触发任务高亮间隔（秒）
	public const int Guidance_NoCompTaskHLInterval = 20;
	//新手不用登场技触发技能高亮间隔（秒）
	public const int Guidance_NoParSkillHLInterval = 10;
	//个人秘境基础Buff时间（秒）
	public const int SecretBuffTime_Basic = 60;
	//个人秘境额外Buff时间（秒）
	public const int SecretBuffTime_Extra = 240;
	//聊天单文字消息最长发送字节数
	public const int Chat_TextMaxByteCount = 100;
	//抽卡展示动作切换下限时长
	public const int Gacha_ShowSwitchMinTime = 7;
	//抽卡展示动作切换上限时长
	public const int Gacha_ShowSwitchMaxTime = 15;
	//拟态梦境奖励限时
	public const int PersonSecret_TimeLimit = 300;
	//派对时刻挂机奖励间隔(秒)
	public const int PartyTime_QA_HangUp_Interval = 10;
	//派对时刻挂机奖励（对应cost表id）
	public const int PartyTime_QA_HangUp_Reward = 10012;
	//派对时刻挂机奖励领取次数（对应通用次数表ID）
	public const int PartyTime_QA_HangUp_LimitNum = 20;
	//派对时刻每轮答题间隔（秒）
	public const int PartyTime_QA_Round_Interval = 30;
	//派对时刻每轮答案公布到下一轮答题开始间隔（秒）
	public const int PartyTime_QA_Round_Interval2 = 10;
	//派对时刻预热阶段时间
	public const int PartyTime_HeraldPhase = 300;
	//派对时刻准备阶段时间
	public const int PartyTime_PreparatoryPhase = 300;
	//派对时刻玩法阶段时间
	public const int PartyTime_GamePlayPhase = 300;
	//派对时刻结束阶段时间
	public const int PartyTime_EndPhase = 300;
	//排行榜前三名轮播时间
	public const int Rank_ShowCarouselTime = 8000;
	//登录各类超时等待时长(s)（创/删/选/进入游戏/请求服务器）
	public const int Login_ReqWaitTimeout = 20;
	//上架检测间隔（分钟）
	public const int Trade_SysGoodCheckInterval = 60;
	//定时检测补货触发（百分比）
	public const int Trade_SysGoodTimingReplenishTrigger = 0;
	//实时检测补货触发（百分比）
	public const int Trade_SysGoodRealTimeReplenishTrigger = 20;
	//上架道具价格波动上限（百分比）
	public const int Trade_SysGoodPriceRandomUpperLimit = 200;
	//上架道具价格波动下限（百分比）
	public const int Trade_SysGoodPriceRandomLowerLimit = 150;
	//单次补货组数波动上限（百分比）
	public const int Trade_SysGoodShelfRandomUpperLimit = 120;
	//单次补货组数波动下限（百分比）
	public const int Trade_SysGoodShelfRandomLowerLimit = 70;
	//单组内数量波动上限（百分比）
	public const int Trade_SysGoodItemRandomUpperLimit = 120;
	//单组内数量波动下限（百分比）
	public const int Trade_SysGoodItemRandomLowerLimit = 70;
	//强引导遮罩透明度（百分比/unity非线性）
	public const int Guidance_ForceGuideMaskAlpha = 65;
	//全版本钻比金汇率最高值限制（10:X）
	public const int ExInitialRate_All_Max = 15000;
	//全版本钻比金汇率最低值限制（10:X）
	public const int ExInitialRate_All_Min = 500;
	//AVG-无语音对话自动播放延迟
	public const int Dialogue_AutoPlayDelay_NoVoice = 2000;
	//AVG-有语音对话自动播放延迟
	public const int Dialogue_AutoPlayDelay_HaveVoice = 500;
	//强引导遮罩透明度-图片组（百分比/unity非线性）
	public const int Guidance_ForceGuideMaskAlpha2 = 95;
	//强引导转弱开始变化次数
	public const int Guidance_ForceGuideMaskFadeStaTime = 3;
	//强引导转弱最终变化次数
	public const int Guidance_ForceGuideMaskFadeEndTime = 8;
	//强引导转弱降低透明度（百分比/unity非线性）
	public const int Guidance_ForceGuideMaskFadeAlpha = 5;
	//命中系数1
	public const int Att_Hit1 = 1;
	//命中系数2
	public const int Att_Hit2 = 4;
	//闪避系数1
	public const int Att_Evasion1 = 1;
	//闪避系数2
	public const int Att_Evasion2 = 4;
	//暴击系数1
	public const int Att_Critical1 = 1;
	//暴击系数2
	public const int Att_Critical2 = 7;
	//抗暴系数1
	public const int Att_Recritical1 = 1;
	//抗暴系数2
	public const int Att_Recritical2 = 7;
	//暴伤系数1
	public const int Att_Criticaldamage1 = 2;
	//暴伤系数2
	public const int Att_Criticaldamage2 = 7;
	//元素免伤系数1
	public const int Arr_Elementimmunity1 = 1;
	//元素免伤系数2
	public const int Arr_Elementimmunity2 = 7;
	//BUFF时长系数1
	public const int Arr_BUFFtime1 = 1;
	//BUFF时长系数2
	public const int Arr_BUFFtime2 = 1;
	//BUFF时长上限
	public const int Arr_BUFFtimelimit1 = 160;
	//BUFF时长下限
	public const int Arr_BUFFtimelimit2 = 40;
	//区分目标增伤系数1
	public const int Arr_Targetincrement1 = 1;
	//区分目标增伤系数2
	public const int Arr_Targetincrement2 = 4;
	//是否开启充值
	public const int RechargePaySystem = 0;
	//跑马灯滚动速度
	public const int MarqueeDuration = 120;
	//拟态梦境助力Buff
	public const int SecretAssistanceBuffID = 3451;
	//虚拟相机隐藏hud额外持续时间
	public const int BackToPlayerCFV = 2000;
	//特殊环任务领奖
	public const int RingTask_SpecialAwardTask = 1073741833;
	//拟态梦境（个人秘境）首次通关特殊掉落
	public const int SpecialDrop_PersonSecret = 450010;
	//蚀梦之影（组队日常本）首次通关特殊掉落
	public const int SpecialDrop_TeamDaily = 220200;
	//时序残响（个人日常本）首次通关特殊掉落
	public const int SpecialDrop_PersonDaily = 110150;
	//传送读条时间（毫秒）
	public const int TransferWaitingTime = 2000;
	//拟态梦境关卡时间
	public const int PersonSecret_LevelTimeLimit = 300;
	//拟态梦境退出不显示主HUD
	public const int PersonSecret_ExitWithoutMainHud = 5;
	//前期任务移速BUFF
	public const int Task_FastFast = 97;
	//前期任务移速buff附加需求寻路距离（米）
	public const int Guidance_MoveFastFindPathDIST = 15;
	//前期新手加成结束等级（移速buff附加）
	public const int Guidance_MoveFastEndLv = 39;
}