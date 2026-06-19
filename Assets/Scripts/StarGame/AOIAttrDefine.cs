namespace StarProjectDef
{
    [XLua.LuaCallCSharp]
    public class AOIAttrDefine
    {
         // Name:1001:string:名称
         public const string Name = "Name";

         // RoleModel:1002:uint32:角色头像id
         public const string RoleModel = "RoleModel";

         // LoginTime:1004:int64:登录时间
         public const string LoginTime = "LoginTime";

         // LogoutTime:1005:int64:登出时间
         public const string LogoutTime = "LogoutTime";

         // TodayOnlineTime:1006:int64:今天在线时长
         public const string TodayOnlineTime = "TodayOnlineTime";

         // OnlineTime:1007:int64:总在线时长
         public const string OnlineTime = "OnlineTime";

         // AccessToken:1008:string:accessToken
         public const string AccessToken = "AccessToken";

         // RoleModelFrame:1009:int32:头像框
         public const string RoleModelFrame = "RoleModelFrame";

         // Coin:1010:int64:金币
         public const string Coin = "Coin";

         // Silver:1011:int64:银币
         public const string Silver = "Silver";

         // ChargeDiamond:1012:int64:钻石：充值获得
         public const string ChargeDiamond = "ChargeDiamond";

         // BindDiamond:1013:int64:绑钻：游戏内获得
         public const string BindDiamond = "BindDiamond";

         // SceneFlag:1049:uint64:按照位运算,第0位为1表示进入过scene服
         public const string SceneFlag = "SceneFlag";

         // Position:1050:*linmath.Vector3:坐标
         public const string Position = "Position";

         // Rot:1051:int32:旋转角度
         public const string Rot = "Rot";

         // MoveState:1052:int64:在黑洞状态下最后一次客户端发来的移动消息的时间（毫秒）
         public const string MoveState = "MoveState";

         // RobotState:1053:bool:是否是机器人
         public const string RobotState = "RobotState";

         // Rocker:1054:int32:摇杆角度
         public const string Rocker = "Rocker";

         // RiderState:1055:bool:骑马的状态
         public const string RiderState = "RiderState";

         // RegionID:1056:uint16:当前的SpaceID
         public const string RegionID = "RegionID";

         // OfflineMapID:1057:int32:上下线的SpaceID
         public const string OfflineMapID = "OfflineMapID";

         // OfflinePosition:1058:*linmath.Vector3:上下线的Space坐标
         public const string OfflinePosition = "OfflinePosition";

         // CurrParIndex:1059:int32:当前伙伴Index,默认0
         public const string CurrParIndex = "CurrParIndex";

         // StorageHP:1060:int64:存储的血量
         public const string StorageHP = "StorageHP";

         // StorageMP:1061:int64:存储的蓝量
         public const string StorageMP = "StorageMP";

         // ReplyHpPre:1062:int32:战斗状态低于百分多少自动回复
         public const string ReplyHpPre = "ReplyHpPre";

         // StorageDrugs:1063:*protoMsg.StorageDrugsMd:战斗状态存储的药品
         public const string StorageDrugs = "StorageDrugs";

         // ReviveMapID:1070:int32:出生/复活点Space
         public const string ReviveMapID = "ReviveMapID";

         // RevivePosition:1071:*linmath.Vector3:出生/复活的Space坐标
         public const string RevivePosition = "RevivePosition";

         // CurMapID:1072:uint64:当前地图ID，仅在进入地图时由scene更新到lobby
         public const string CurMapID = "CurMapID";

         // OfflineRot:1073:int32:上下线的Space角度
         public const string OfflineRot = "OfflineRot";

         // TeamID:1074:uint64:队伍ID,为0则未组队
         public const string TeamID = "TeamID";

         // ExAmuletInfo:1075:*protoMsg.ExtractaAmuletMD:最后一次护符萃取信息
         public const string ExAmuletInfo = "ExAmuletInfo";

         // OutGuildTime:1077:int64:退出公会时间
         public const string OutGuildTime = "OutGuildTime";

         // PlayerGuildInfo:1079:*protoMsg.PlayerGuildInfo:玩家的公会信息
         public const string PlayerGuildInfo = "PlayerGuildInfo";

         // CreateTime:1102:int64:实体创建时间（只在战斗服使用）
         public const string CreateTime = "CreateTime";

         // PlayerLevel:1103:int32:等级
         public const string PlayerLevel = "PlayerLevel";

         // PlayerExp:1104:int32:队伍经验值
         public const string PlayerExp = "PlayerExp";

         // RiskInfo:1105:*protoMsg.RiskLevelMD:冒险等级相关
         public const string RiskInfo = "RiskInfo";

         // WantTaskInfo:1106:*protoMsg.WantTaskMD:通缉任务
         public const string WantTaskInfo = "WantTaskInfo";

         // ReceMaxGlobalMailID:1200:int64:已经收取的全服邮件编号
         public const string ReceMaxGlobalMailID = "ReceMaxGlobalMailID";

         // RecoverTime:1201:int64:一些活动的恢复时间
         public const string RecoverTime = "RecoverTime";

         // InteractID:1203:uint64:交互物件ID
         public const string InteractID = "InteractID";

         // SkillPoint:1204:int32:技能点
         public const string SkillPoint = "SkillPoint";

         // DialogID:1205:int32:对话ID
         public const string DialogID = "DialogID";

         // CurState:1206:uint8:场景服传过来的状态,独占状态
         public const string CurState = "CurState";

         // UserID:1207:uint64:玩家数据库ID,客户端AOI用
         public const string UserID = "UserID";

         // SkillPointUsed:1208:int32:已消耗的技能点
         public const string SkillPointUsed = "SkillPointUsed";

         // CurTreasure:1209:*protoMsg.TreasureData:当前宝图的相关信息
         public const string CurTreasure = "CurTreasure";

         // GVEBonus:1211:*protoMsg.GVEBonus:GVE个人积分
         public const string GVEBonus = "GVEBonus";

         // TreasureMonid:1212:*protoMsg.TreMonID:挖宝出来的专属怪ID集合
         public const string TreasureMonid = "TreasureMonid";

         // GNGData:1213:*protoMsg.GNGUserData:GNG个人数据
         public const string GNGData = "GNGData";

         // Tweeter1:1214:int32:当前已装备鸣器1
         public const string Tweeter1 = "Tweeter1";

         // Tweeter2:1215:int32:当前已装备鸣器2
         public const string Tweeter2 = "Tweeter2";

         // PVPState:1216:int32:pvp连杀状态
         public const string PVPState = "PVPState";

         // PayData:1217:*protoMsg.UserPayData:充值个人数据:充值次数,累充,首充签到
         public const string PayData = "PayData";

         // CollectEnergy:1218:int32:采集活力值
         public const string CollectEnergy = "CollectEnergy";

         // CollectEnergyTime:1219:int64:采集活力值上次恢复时间戳
         public const string CollectEnergyTime = "CollectEnergyTime";

         // LastParIndex:1220:int64:上一个伙伴的配置ID
         public const string LastParIndex = "LastParIndex";

         // BaseID:11001:int64:表格ID
         public const string BaseID = "BaseID";

         // DBID:11002:uint64:数据库ID
         public const string DBID = "DBID";

         // Num:11003:int64:堆叠
         public const string Num = "Num";

         // SpaceId:11004:int32:所属背包ID
         public const string SpaceId = "SpaceId";

         // Level:11006:uint32:加成等级
         public const string Level = "Level";

         // Quality:11007:uint32:品阶
         public const string Quality = "Quality";

         // UpdatePoint:11008:int32:强化点数
         public const string UpdatePoint = "UpdatePoint";

         // CDTime:11009:int64:使用CD
         public const string CDTime = "CDTime";

         // GetTime:11010:int64:获得道具时间
         public const string GetTime = "GetTime";

         // EquipProps:11011:*protoMsg.EquipPropList:装备属性
         public const string EquipProps = "EquipProps";

         // IsBind:11012:bool:道具是否绑定
         public const string IsBind = "IsBind";

         // Durability:11015:int32:耐久度
         public const string Durability = "Durability";

         // Heroid:11016:uint64:当前使用该武器的英雄
         public const string Heroid = "Heroid";

         // CombatSkills:11017:*protoMsg.CombatSkillList:护符战技
         public const string CombatSkills = "CombatSkills";

         // GemSlots:11018:*protoMsg.GemSlotList:护符纹章槽
         public const string GemSlots = "GemSlots";

         // BelongEquipID:11019:uint64:归属于的装备ID
         public const string BelongEquipID = "BelongEquipID";

         // ExtractNum:11020:int32:萃取次数
         public const string ExtractNum = "ExtractNum";

         // TreasureMapID:11021:int32:宝藏所在地图
         public const string TreasureMapID = "TreasureMapID";

         // TreasureInterID:11022:int32:宝藏关联交互物
         public const string TreasureInterID = "TreasureInterID";

         // TreasurePos:11023:int32:宝藏中心坐标
         public const string TreasurePos = "TreasurePos";

         // Atk:3001:int64:攻击
         public const string Atk = "Atk";

         // Defence:3002:int64:防御
         public const string Defence = "Defence";

         // MDefence:3003:int64:魔抗
         public const string MDefence = "MDefence";

         // Hp:3004:int64:生命
         public const string Hp = "Hp";

         // Mp:3005:int64:法力
         public const string Mp = "Mp";

         // RecHp:3006:int64:生命恢复值
         public const string RecHp = "RecHp";

         // RecMp:3007:int64:法力恢复值
         public const string RecMp = "RecMp";

         // AtkRate:3008:int64:百分比攻击加成
         public const string AtkRate = "AtkRate";

         // DefRate:3009:int64:百分比防御加成
         public const string DefRate = "DefRate";

         // MDefRate:3010:int64:百分比魔抗加成
         public const string MDefRate = "MDefRate";

         // HpRate:3011:int64:百分比生命加成
         public const string HpRate = "HpRate";

         // HitLv:3012:int64:命中等级
         public const string HitLv = "HitLv";

         // DodgeLv:3013:int64:闪避等级
         public const string DodgeLv = "DodgeLv";

         // CriLv:3014:int64:暴击等级
         public const string CriLv = "CriLv";

         // CriDefLv:3015:int64:抗暴击等级
         public const string CriDefLv = "CriDefLv";

         // CriDamLv:3016:int64:爆伤等级
         public const string CriDamLv = "CriDamLv";

         // ExHit:3017:int64:额外命中率
         public const string ExHit = "ExHit";

         // ExDodge:3018:int64:额外闪避率
         public const string ExDodge = "ExDodge";

         // ExCri:3019:int64:额外暴击率
         public const string ExCri = "ExCri";

         // ExDefCri:3020:int64:额外抗暴击率
         public const string ExDefCri = "ExDefCri";

         // ExCriDam:3021:int64:额外爆伤率
         public const string ExCriDam = "ExCriDam";

         // Pierce:3022:int64:破甲
         public const string Pierce = "Pierce";

         // PierceRate:3023:int64:百分比破甲
         public const string PierceRate = "PierceRate";

         // MPierce:3024:int64:法穿
         public const string MPierce = "MPierce";

         // MPierceRate:3025:int64:百分比法穿
         public const string MPierceRate = "MPierceRate";

         // Speed:3026:int64:移动速度
         public const string Speed = "Speed";

         // SpeedAdd:3027:int64:移动速度加成
         public const string SpeedAdd = "SpeedAdd";

         // AttackSpeed:3028:int64:攻击速度
         public const string AttackSpeed = "AttackSpeed";

         // EnergySpeed:3029:int64:蓄力速度
         public const string EnergySpeed = "EnergySpeed";

         // Spectral1:3030:int64:量谱槽1值
         public const string Spectral1 = "Spectral1";

         // Spectral2:3031:int64:量谱槽2值
         public const string Spectral2 = "Spectral2";

         // Spectral3:3032:int64:量谱槽3值
         public const string Spectral3 = "Spectral3";

         // SpectralRate1:3033:int64:量谱槽1万分比加成
         public const string SpectralRate1 = "SpectralRate1";

         // SpectralRate2:3034:int64:量谱槽2万分比加成
         public const string SpectralRate2 = "SpectralRate2";

         // SpectralRate3:3035:int64:量谱槽3万分比加成
         public const string SpectralRate3 = "SpectralRate3";

         // Recovery:3036:int64:恢复效率万分比
         public const string Recovery = "Recovery";

         // Injury:3037:int64:重伤万分比
         public const string Injury = "Injury";

         // PhyDamAdd:3051:int64:物理伤害加成
         public const string PhyDamAdd = "PhyDamAdd";

         // PhyDamDec:3052:int64:物理伤害削弱
         public const string PhyDamDec = "PhyDamDec";

         // ElemDamAdd:3053:int64:元素伤害加成
         public const string ElemDamAdd = "ElemDamAdd";

         // ElemDamDec:3054:int64:元素伤害削弱
         public const string ElemDamDec = "ElemDamDec";

         // FinDamAdd:3055:int64:最终伤害加成
         public const string FinDamAdd = "FinDamAdd";

         // FinDamDec:3056:int64:最终伤害减免
         public const string FinDamDec = "FinDamDec";

         // Suck:3057:int64:吸血
         public const string Suck = "Suck";

         // Thorns:3058:int64:反伤-荆棘
         public const string Thorns = "Thorns";

         // BuffAddDamage:3059:int64:buff增伤万分比
         public const string BuffAddDamage = "BuffAddDamage";

         // BuffVulnerable:3060:int64:buff易伤万分比
         public const string BuffVulnerable = "BuffVulnerable";

         // ElemEarth:3200:int64:大地元素
         public const string ElemEarth = "ElemEarth";

         // ElemFire:3201:int64:烈焰元素
         public const string ElemFire = "ElemFire";

         // ElemIce:3202:int64:寒冰元素
         public const string ElemIce = "ElemIce";

         // ElemNatura:3203:int64:自然元素
         public const string ElemNatura = "ElemNatura";

         // ElemToxic:3204:int64:剧毒元素
         public const string ElemToxic = "ElemToxic";

         // ElemLight:3205:int64:光明元素
         public const string ElemLight = "ElemLight";

         // ElemDark:3206:int64:黑暗元素
         public const string ElemDark = "ElemDark";

         // ElemAll:3220:int64:全元素攻击
         public const string ElemAll = "ElemAll";

         // ResiEarth:3221:int64:大地抗性
         public const string ResiEarth = "ResiEarth";

         // ResiFire:3222:int64:烈焰抗性
         public const string ResiFire = "ResiFire";

         // ResiIce:3223:int64:寒冰抗性
         public const string ResiIce = "ResiIce";

         // ResiNatura:3224:int64:自然抗性
         public const string ResiNatura = "ResiNatura";

         // ResiToxic:3225:int64:剧毒抗性
         public const string ResiToxic = "ResiToxic";

         // ResiLight:3226:int64:光明抗性
         public const string ResiLight = "ResiLight";

         // ResiDark:3227:int64:黑暗抗性
         public const string ResiDark = "ResiDark";

         // ElemEarthRate:3241:int64:大地元素百分比
         public const string ElemEarthRate = "ElemEarthRate";

         // ElemFireRate:3242:int64:烈焰元素百分比
         public const string ElemFireRate = "ElemFireRate";

         // ElemIceRate:3243:int64:寒冰元素百分比
         public const string ElemIceRate = "ElemIceRate";

         // ElemNaturaRate:3244:int64:自然元素百分比
         public const string ElemNaturaRate = "ElemNaturaRate";

         // ElemToxicRate:3245:int64:剧毒元素百分比
         public const string ElemToxicRate = "ElemToxicRate";

         // ElemLightRate:3246:int64:光明元素百分比
         public const string ElemLightRate = "ElemLightRate";

         // ElemDarkRate:3247:int64:黑暗元素百分比
         public const string ElemDarkRate = "ElemDarkRate";

         // ElemAllRate:3260:int64:全元素百分比
         public const string ElemAllRate = "ElemAllRate";

         // ResiDizz:3350:int64:抵抗眩晕
         public const string ResiDizz = "ResiDizz";

         // ResiChaos:3351:int64:抵抗混乱
         public const string ResiChaos = "ResiChaos";

         // ResiRetard:3352:int64:抵抗减速
         public const string ResiRetard = "ResiRetard";

         // ResiFreeze:3353:int64:抵抗冰冻
         public const string ResiFreeze = "ResiFreeze";

         // ResiSlience:3354:int64:抵抗沉默
         public const string ResiSlience = "ResiSlience";

         // ResiBlind:3355:int64:抵抗致盲
         public const string ResiBlind = "ResiBlind";

         // ResiFear:3356:int64:抵抗恐惧
         public const string ResiFear = "ResiFear";

         // ResiNumb:3357:int64:抵抗麻痹
         public const string ResiNumb = "ResiNumb";

         // ResiSuppress:3358:int64:抵抗压制
         public const string ResiSuppress = "ResiSuppress";

         // ResiTodeter:3359:int64:抵抗震慑
         public const string ResiTodeter = "ResiTodeter";

         // ResiOverheat:3360:int64:抵抗过热
         public const string ResiOverheat = "ResiOverheat";

         // ResiStiff:3361:int64:抵抗僵直
         public const string ResiStiff = "ResiStiff";

         // ResiTwine:3362:int64:抵抗缠绕
         public const string ResiTwine = "ResiTwine";

         // ResiAbsent:3363:int64:抵抗失神
         public const string ResiAbsent = "ResiAbsent";

         // ResiControl:3399:int64:控制抵抗
         public const string ResiControl = "ResiControl";

         // EnhDizz:3300:int64:增强眩晕
         public const string EnhDizz = "EnhDizz";

         // EnhChaos:3301:int64:增强混乱
         public const string EnhChaos = "EnhChaos";

         // EnhRetard:3302:int64:增强减速
         public const string EnhRetard = "EnhRetard";

         // EnhFreeze:3303:int64:增强冰冻
         public const string EnhFreeze = "EnhFreeze";

         // EnhSlience:3304:int64:增强沉默
         public const string EnhSlience = "EnhSlience";

         // EnhBlind:3305:int64:增强致盲
         public const string EnhBlind = "EnhBlind";

         // EnhFear:3306:int64:增强恐惧
         public const string EnhFear = "EnhFear";

         // EnhNumb:3307:int64:增强麻痹
         public const string EnhNumb = "EnhNumb";

         // EnhSuppress:3308:int64:增强压制
         public const string EnhSuppress = "EnhSuppress";

         // EnhTodeter:3309:int64:增强震慑
         public const string EnhTodeter = "EnhTodeter";

         // EnhOverheat:3310:int64:增强过热
         public const string EnhOverheat = "EnhOverheat";

         // EnhStiff:3311:int64:增强僵直
         public const string EnhStiff = "EnhStiff";

         // EnhTwine:3312:int64:增强缠绕
         public const string EnhTwine = "EnhTwine";

         // EnhAbsent:3313:int64:增强失神
         public const string EnhAbsent = "EnhAbsent";

         // EnhControl:3349:int64:控制增强
         public const string EnhControl = "EnhControl";

         // TruthAtk:3400:int64:实际攻击力
         public const string TruthAtk = "TruthAtk";

         // TruthHp:3401:int64:实际生命
         public const string TruthHp = "TruthHp";

         // TruthDef:3402:int64:实际防御
         public const string TruthDef = "TruthDef";

         // TruthMDef:3403:int64:实际魔抗
         public const string TruthMDef = "TruthMDef";

         // TruthElemEarthRate:3404:int64:实际大地元素百分比
         public const string TruthElemEarthRate = "TruthElemEarthRate";

         // TruthElemFireRate:3405:int64:实际烈焰元素百分比
         public const string TruthElemFireRate = "TruthElemFireRate";

         // TruthElemIceRate:3406:int64:实际寒冰元素百分比
         public const string TruthElemIceRate = "TruthElemIceRate";

         // TruthElemNaturaRate:3407:int64:实际自然元素百分比
         public const string TruthElemNaturaRate = "TruthElemNaturaRate";

         // TruthElemToxicRate:3408:int64:实际剧毒元素百分比
         public const string TruthElemToxicRate = "TruthElemToxicRate";

         // TruthElemLightRate:3409:int64:实际光明元素百分比
         public const string TruthElemLightRate = "TruthElemLightRate";

         // TruthElemDarkRate:3410:int64:实际黑暗元素百分比
         public const string TruthElemDarkRate = "TruthElemDarkRate";

         // TruthElemEarth:3411:int64:实际大地元素
         public const string TruthElemEarth = "TruthElemEarth";

         // TruthElemFire:3412:int64:实际烈焰元素
         public const string TruthElemFire = "TruthElemFire";

         // TruthElemIce:3413:int64:实际寒冰元素
         public const string TruthElemIce = "TruthElemIce";

         // TruthElemNatura:3414:int64:实际自然元素
         public const string TruthElemNatura = "TruthElemNatura";

         // TruthElemToxic:3415:int64:实际剧毒元素
         public const string TruthElemToxic = "TruthElemToxic";

         // TruthElemLight:3416:int64:实际光明元素
         public const string TruthElemLight = "TruthElemLight";

         // TruthElemDark:3417:int64:实际黑暗元素
         public const string TruthElemDark = "TruthElemDark";

         // TruthSpeed:3418:int64:实际移动速度
         public const string TruthSpeed = "TruthSpeed";

         // TruthHitRate:3419:int64:实际命中率
         public const string TruthHitRate = "TruthHitRate";

         // TruthDodgeRate:3420:int64:实际闪避率
         public const string TruthDodgeRate = "TruthDodgeRate";

         // TruthCriRate:3421:int64:实际暴击率
         public const string TruthCriRate = "TruthCriRate";

         // TruthDefCirRate:3422:int64:实际抗暴击率
         public const string TruthDefCirRate = "TruthDefCirRate";

         // TruthCirDamRate:3423:int64:实际爆伤率
         public const string TruthCirDamRate = "TruthCirDamRate";

         // TruthMP:3424:int64:实际法力
         public const string TruthMP = "TruthMP";

         // TruthAttackSpeed:3425:int64:实际攻速
         public const string TruthAttackSpeed = "TruthAttackSpeed";

         // TruthEnergySpeed:3426:int64:实际蓄力速度
         public const string TruthEnergySpeed = "TruthEnergySpeed";

         // TruthSpectral1:3427:int64:实际量谱槽1
         public const string TruthSpectral1 = "TruthSpectral1";

         // TruthSpectral2:3428:int64:实际量谱槽2
         public const string TruthSpectral2 = "TruthSpectral2";

         // TruthSpectral3:3429:int64:实际量谱槽3
         public const string TruthSpectral3 = "TruthSpectral3";

         // TruthRecovery:3430:int64:实际恢复效率万分比
         public const string TruthRecovery = "TruthRecovery";

         // TruthInjury:3431:int64:实际重伤万分比
         public const string TruthInjury = "TruthInjury";

         // curHp:3450:int64:当前生命
         public const string curHp = "curHp";

         // curMp:3451:int64:当前法力
         public const string curMp = "curMp";

         // curSpectral1:3452:int64:当前量谱槽1
         public const string curSpectral1 = "curSpectral1";

         // curSpectral2:3453:int64:当前量谱槽2
         public const string curSpectral2 = "curSpectral2";

         // curSpectral3:3454:int64:当前量谱槽3
         public const string curSpectral3 = "curSpectral3";

         // HeroId:3501:int64:战斗单位的表格ID
         public const string HeroId = "HeroId";

         // TargetBattleEntityID:3502:uint64:当前盯着目标的编号
         public const string TargetBattleEntityID = "TargetBattleEntityID";

         // BattleSubState:3503:uint32:战斗状态下随机子状态
         public const string BattleSubState = "BattleSubState";

         // State:3901:uint32:战斗状态
         public const string State = "State";

         // Job:3902:uint32:职业
         public const string Job = "Job";

         // Faction:3903:uint8:玩家阵营
         public const string Faction = "Faction";

         // PathPoses:3904:*protoMsg.ArrayVector3:寻路的坐标集合
         public const string PathPoses = "PathPoses";

         // CurrPathIndex:3905:int32:寻路的坐标点下标，无效时为-1
         public const string CurrPathIndex = "CurrPathIndex";

         // BelongID:3907:uint64:从属ID，上级继承关系
         public const string BelongID = "BelongID";

         // ReviveCount:3908:int32:复活次数（目前只在副本内用到）
         public const string ReviveCount = "ReviveCount";

         // Index:2001:uint32:怪物的索引
         public const string Index = "Index";

         // AIState:2007:uint32:同步AI的状态给客户端
         public const string AIState = "AIState";

         // ShowLimit:2009:int32:客户端显示上限
         public const string ShowLimit = "ShowLimit";

         // TargetId:2011:uint64:同步目标ID给客户端
         public const string TargetId = "TargetId";

         // InfluenceID:2012:int32:实体ID
         public const string InfluenceID = "InfluenceID";

         // TinyEntityFlag:2014:uint32:protoMsg.PVP_MonsterFlag 位操作
         public const string TinyEntityFlag = "TinyEntityFlag";

         // DropCount:2015:uint32:掉落數量
         public const string DropCount = "DropCount";

         // SpawnInfluenceID:2019:int32:创建时所在实体InfluenceID
         public const string SpawnInfluenceID = "SpawnInfluenceID";

         // MonsterAIState:2028:uint32:怪物服务器状态
         public const string MonsterAIState = "MonsterAIState";

         // IsInteract:2030:bool:是否处于交互
         public const string IsInteract = "IsInteract";

         // InteractUseList:2031:*protoMsg.AllInterState:交互列表
         public const string InteractUseList = "InteractUseList";

         // SummonHostID:2032:uint64:召唤物主人ID
         public const string SummonHostID = "SummonHostID";

         // OffestRota:2033:float32:召唤物和主人的偏移角度
         public const string OffestRota = "OffestRota";

         // BelongTeamID:2034:uint64:归属的teamid
         public const string BelongTeamID = "BelongTeamID";

         // Alias:2035:string:data.json的Alias
         public const string Alias = "Alias";

         // AvatarID:2036:int32:对应的AvatarID
         public const string AvatarID = "AvatarID";

         // EntityLevel:2037:int32:等级
         public const string EntityLevel = "EntityLevel";

         // SpaceIndex:2038:int32:data.json 里的 index
         public const string SpaceIndex = "SpaceIndex";

         // Title:2039:string:data.json 里的 title
         public const string Title = "Title";

         // ControlID:2040:int32:data.json 里的 控制器id
         public const string ControlID = "ControlID";

         // ActMark:2041:uint32:各种活动、玩法中的标记
         public const string ActMark = "ActMark";

         // ShowLevel:2042:int32:显示等级
         public const string ShowLevel = "ShowLevel";

         // LevelTarget:4001:int16:关卡目标
         public const string LevelTarget = "LevelTarget";

        public const string WorldLineRewardData = "WorldLineRewardData";

         public const string Error = "Error";
     }
}