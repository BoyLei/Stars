# Lua 业务模块 — 完整分类列表

> Level 3 — 74 个 Lua 业务模块按 6 个功能域分组（不含测试文件）。  
> 位置: `Assets/Res/LuaScripts/LuaModule/`  
> 基类: `Core/BaseLuaModule.lua.txt`  
> 创建: `ModuleManager.CreateModule → LuaManager.GetLuaModule → BindLuaCall(LuaTable)`

---

## 社交域 (8 个) — 详见 `SocialModules.md`

| 模块 | 职责 |
|------|------|
| **ChatModule** | 多频道聊天（世界/私聊/公会/队伍/喇叭），HUD 挂件 + 弹幕 |
| **FriendModule** | 好友添加/删除/黑名单/在线状态，Client2Third 协议中转 |
| **GuildModule** | 公会创建/加入/管理/公会战入口 |
| **MailModule** | 邮件查看/附件领取/一键删除已读/红点刷新 |
| **TeamModule** | 队伍创建/邀请/踢人/队长转让/跟随切场景 |
| **DailyTeamModule** | 日常副本自动组队匹配，与 TeamModule 独立解耦 |
| **DonateModule** | 公会捐献 |
| **PartyTimeModule** | 公会篝火挂机奖励 |

## 经济/交易域 (17 个) — 详见 `EconomyModules.md`

| 模块 | 职责 |
|------|------|
| **ShopModule** | 商店（普通/NPC 商店），购买掉落预览 |
| **BagModule** | 背包查看/整理/拆分/扩展 |
| **EquipModule** | 装备穿戴/卸下/属性查看 |
| **EquipUpgradeModule** | 装备强化/进阶/升星 |
| **QuickEquipModule** | 一键快速穿戴最佳装备 |
| **ItemTipsModule** | 道具详情弹窗（属性/来源/功能按钮），被多模块引用 |
| **ItemUseModule** | 道具使用确认弹框，支持批量 |
| **ItemBuyModule** | 道具购买弹框（数量/货币选择） |
| **ItemResolveModule** | 道具分解为材料 |
| **ExchangeModule** | 交易行/拍卖行，5 种搜索方式，买卖日志 |
| **BankModule** | 银行/仓库存取 |
| **RechargeModule** | 充值/拍卖竞拍（名称含历史包袱） |
| **CommercializationModule** | 限时礼包/首充 |
| **FuncTipsModule** | 通用提示弹窗（确认/取消/信息） |
| **AnnounceModule** | 掉落公告队列，道具获得时逐个弹出 |
| **ItemManagerModule** | 道具管理器，监听金币变化同步 |
| **LootControllerModule** | 战利品掉落特效（5 品质对象池），场景切换回收 |

## 活动/运营域 (14 个) — 详见 `EventModules.md`

| 模块 | 职责 |
|------|------|
| **ActivityModule** | 活动签到/玩法预告，FixMessage 双重数据源 |
| **DailyActModule** | 每日活跃度进度和宝箱奖励 |
| **EventModule** | 活动总控，gameID 时间计算和玩法子模块派发 |
| **EventBossRankModule** | 活动 Boss 伤害排行榜 |
| **AfternoonGveModule** | 公会午间 GVE 组队玩法 |
| **GamePlayCalendarModule** | 玩法日历总览 |
| **GamePlayChoiceModule** | 玩法前置选择（难度/模式） |
| **OnlineRewardModule** | 在线时长挂机奖励 |
| **DrawCardModule** | 抽卡（3D 场景 + Timeline 特效 + 品质分色） |
| **RewardsPopModule** | 通用奖励获得弹窗 |
| **AnnouncementModule** | 系统公告列表 |
| **RunHorseModule** | 公告滚动跑马灯 |
| **EventTipsModule** | GVE 活动提示挂件，监听 GVEBonus 属性变化 |
| **SevenDaysSignModule** | 七日签到测试模块（半成品） |

## 战斗/副本域 (17 个) — 详见 `CombatModules.md`

| 模块 | 职责 |
|------|------|
| **PvpModule** | PVP 10v10（报名/倒计时/战斗 HUD/结算） |
| **ArenaModule** | 异步竞技场 1v1（排名/挑战/次数管理） |
| **WildBossModule** | 野外 Boss（列表/伤害/宝箱，Space 服协议） |
| **PersonalTowerModule** | 个人爬塔（逐层挑战） |
| **AdventureLevelModule** | 冒险等级/经验/进度 |
| **SecretAreaModule** | 个人秘境（11 个 UI，最大 Lua 模块） |
| **EctypeEntranceModule** | 副本入口选择（难度/组队） |
| **EctypeBagModule** | 副本内临时背包 |
| **EctypePopModule** | 副本内提示弹窗 |
| **EctypeSettleModule** | 副本结算（评分/奖励） |
| **DropControllerModule** | 副本掉落拾取/分配/Roll 点 |
| **DropInfoListModule** | 掉落信息列表展示 |
| **SkillWindowModule** | 技能查看/装备/升级 |
| **SkillUnlockTipsModule** | 技能解锁特效提示 |
| **BattleTeamModule** | 伙伴阵容配置/出战/升星 |
| **SkillSwitchModule** | 技能方案切换/改名（多套配置） |
| **PartnerModule** | 伙伴装备窗口，通过 EquipModule 操作 |

## 角色/养成域 (8 个) — 详见 `CharacterModules.md`

| 模块 | 职责 |
|------|------|
| **TalentModule** | 天赋树加点/重置 |
| **InscriptionModule** | 铭文装备/升级/套装 |
| **AmuletModule** | 符文镶嵌/合成/升级 |
| **FightPowerModule** | 战力计算与展示 |
| **MedicineModule** | 药品制作/使用/快捷栏 |
| **MindRepairModule** | 心灵修复小游戏（休闲） |
| **PlayerPreviewModule** | 玩家信息预览（6 种预览类型） |
| **SelectorModule** | 通用选择器（角色/技能/物品） |

## 系统/信息域 (11 个) — 详见 `SystemModules.md`

| 模块 | 职责 |
|------|------|
| **TaskModule** | 任务系统（接受→追踪→提交→奖励），含剧情播放 |
| **WantedModule** | 通缉 Boss（接取→追踪→击杀→奖励），组队子玩法 |
| **LivingSkillsModule** | 生活技能（采集/制造/跨地图矿点寻路） |
| **AvgLuaModule** | AVG 文字冒险/对话演出/分支选择 |
| **RankModule** | 多维度排行榜 |
| **BeginnerTargetModule** | 新手/回归阶段性目标引导 |
| **TreasureModule** | 藏宝图寻宝 |
| **WorldLineModule** | 世界线查看/切换 |
| **ObjectInteractiveModule** | 场景交互物列表（宝箱/传送门） |
| **SystemOpenTipsModule** | 系统功能解锁提示 |
| **LoginModule** | 空壳登录模块（预留扩展点） |

---

## 总计

共 **74 个** Lua 模块（不含测试文件 `LuaModuleTest.lua.txt`），按 6 个功能域分组：
- 社交: 8 个
- 经济/交易: 17 个
- 活动/运营: 14 个
- 战斗/副本: 17 个
- 角色/养成: 8 个
- 系统/信息: 11 个
