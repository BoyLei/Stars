# Lua 战斗/副本域模块

> Level 3 — 17 个战斗模块，覆盖 PVP/竞技场/野外Boss/秘境/副本/技能/伙伴出战

---

## 1. SecretAreaModule — 个人秘境（最大 Lua 模块）
个人秘境挑战（逐层爬塔），含 11 个 UI 组件。支持断线重连缓存、赛季排名/成就/日常奖励。Boss 死亡位置生成宝箱。
UI: `SecretArea/Prefab/SecretAreaWindow` / `TaskWidget` / `BuffTipsWidget` / `RankWidget` / `AchieveWidget` 等 11 个
特点: 特殊交互物件常量（宝箱 9999991/额外宝箱 9999997/传送门 9999993）。

**系统关系**:
Module: `LuaModule/SecretAreaModule.lua.txt` (~1477行)
  ↓
View: `View/UIWindow/SecretArea/SecretAreaWindow.lua.txt` (~35KB, 主界面)
  ├── `SecretAreaTaskWidget.lua.txt` (战斗HUD挂件)
  ├── `SecretAreaFailWindow.lua.txt` (失败复活) / `SecretAreaSuccessWindow.lua.txt` (胜利结算)
  ├── `SecretAreaDailyAwardWidget.lua.txt` (每日奖励)
  ├── `SecretAreaRankWidget.lua.txt` (赛季排行) / `SecretAreaAchieveWidget.lua.txt` (成就)
  └── `RankRewardPartWidget.lua.txt` / `EctypeWarningWidget.lua.txt`
子面板: 5个（SecretAreaFloorItem / ItemTickets / AchieveItem / DailyTaskItem / DailyAwardItem + ItemGrid）
Module→View: OpenSecretAreaWindow / OpenSecretAreaTaskWidget / 共 11 个 Open/Close/Refresh 接口
View→Module: SendEnterPersonSercetReq / SendPersonSecretReviveReq / SendLeaveInstanceReq
跨模块: ChatModule (宝箱开箱聊天) / RewardsPopModule (领奖弹窗)

## 2. PvpModule — PVP 对战（10v10）
10v10 大战场，含报名/倒计时/战斗HUD/结算完整流程。
UI: `PVP/Prefab/SignUpWindow` / `InfoWindow` / `PvpWidget` / `CountDownTips` 等 8 个
特点: OnModuleShow 时主动请求对战数据和报名数据。

**系统关系**:
Module: `LuaModule/PvpModule.lua.txt` (~703行)
  ↓
View: `View/UIWindow/PvpWindow/` 下 9 个文件
  ├── SignUpWindow (报名) / BattleInfoWindow (战场信息)
  ├── PvpWidget (战斗HUD) / CountDownTips (倒计时)
  ├── BattleTips (连杀/首杀提示) / MarqueeWidget (跑马灯)
  └── ResultWindow (结算) / PvpEntryWindow (入口) / RewardsWindow (奖励)
子面板: 4个（SignUpItem / BattleInfoItem / RewardInfoTips / ResultItem）
Module→View: OnShowSignUp / OnShowBattleInfo / 按协议逐项打开各窗口
View→Module: 通过 self.module 发协议和状态查询

## 3. ArenaModule — 异步竞技场（1v1）
1v1 异步竞技场，通过 FixMessageManager 读取 ComCount 通用次数（ID=17）。挑战次数含免费+购买两种，对手列表固定 5 个。
UI: `Arena/Prefab/ArenaWindow` / `BattleRecordWidget` / `SettleWindow`
特点: 竞技场购买次数消耗从 ArenaBuy 配表读取递进价格。

**系统关系**:
Module: `LuaModule/ArenaModule.lua.txt` (~80+行)
  ↓
View: `View/UIWindow/Arena/ArenaWindow.lua.txt` (~17KB) + `ArenaSettleWindow.lua.txt` + `ArenaBattleRecordWidget.lua.txt`
  └── 配套Widget: ArenaBattleRecordItem / ArenaRankItem / ArenaRankRewardWidget
Module→View: OnOpenArenaWindow → ArenaWindow:BindModule + Init / OpenSettleWindow / OpenBattleRecord
View→Module: SendChallengeReq / SendBuyCount / GetChallengeCount
跨模块: ChatModule (竞技场结果广播聊天)

## 4. BattleTeamModule — 伙伴出战
伙伴阵容配置/出战/升星管理。
UI: 伙伴出战窗口
特点: 第二大 Lua 模块，伙伴系统核心。

**系统关系**:
Module: `LuaModule/BattleTeamModule.lua.txt` (伙伴系统核心)
  ↓
View: `View/UIWindow/Partner/` 下 15+ 个文件
  ├── PartnerWindow (主窗口) / PartnerDetailWidget
  ├── PartnerStarRiseWidget (升星) / PartnerAblutionWidget (洗练)
  ├── PartnerEquipWidget (装备) / PartnersWidget (列表)
  ├── BattleTeamWidget / AssistsWidget / PartnerAssistWidget
  ├── CertificationToppedWidget / PartnerTeamAttributeWidget
  └── LevelUpWindow / EquipSelWindow
Module→View: 15+ 个 Open/Close/Refresh 接口
View→Module: 通过 self.module 发起协议请求和状态查询

## 5. SkillWindowModule — 技能窗口
技能查看/装备/升级操作。
UI: 技能窗口
特点: 技能成长链路入口。

## 6. WildBossModule — 野外 Boss
野外 Boss 列表/伤害统计/宝箱领取。协议发往 Space 服（非 Lobby）。MonID + Map 双键索引配表。
UI: 野外 Boss 主界面 / 伤害进度挂件
特点: 唯一的 Space 服协议模块，与 EventModule 联动获取下次刷新时间。

**系统关系**:
Module: `LuaModule/WildBossModule.lua.txt`
  ↓
View: `WildBoss/Prefab/WildBossWindow` + `WildBoss/Prefab/WildBossDamageWidget`
Module→View: OpenWindowAsync / OpenWidgetAsync (异步加载)
View→Module: Boss数据查询 / 伤害提交
跨模块: EventModule (获取下次刷新时间) / ChatModule (Boss击杀公告)

## 7. PersonalTowerModule — 个人爬塔
无限/限时爬塔玩法，逐层挑战。
UI: 爬塔窗口
特点: 逐层解锁，难度递增。

## 8. SkillUnlockTipsModule — 技能解锁表现
技能解锁时的特效和提示展示。
UI: 技能解锁提示
特点: 纯表现层，不涉及技能数据操作。

## 9. SkillSwitchModule — 技能方案切换
多套技能配置方案切换和方案改名。
UI: `Skill/Prefab/SkillSwitchNew`
特点: 支持战斗中禁止切换（IsSeverBattleState 判定），多种失败原因提示。

## 10. AdventureLevelModule — 冒险等级
冒险等级/经验/任务进度展示。
UI: 冒险等级窗口
特点: 进度型系统。

## 11. DropControllerModule — 掉落管理
副本内掉落拾取/分配/Roll 点。
UI: 掉落管理界面
特点: 组队掉落分配逻辑。

## 12. DropInfoListModule — 掉落信息列表
掉落物品信息列表展示。
UI: 掉落列表
特点: 与 DropControllerModule 配套使用。

## 13. EctypeEntranceModule — 副本入口
副本入口选择，含难度/组队/匹配选项。
UI: 副本入口窗口
特点: 副本统一入口。

## 14. EctypeBagModule — 副本临时背包
副本内临时背包，存放副本中拾取的物品。
UI: 副本背包
特点: 副本结束即清空。

## 15. EctypeSettleModule — 副本结算
副本完成后的结算界面，含评分/奖励/统计。
UI: 结算窗口
特点: 副本结束触发。

## 16. EctypePopModule — 副本弹窗
副本内人性化提示弹窗（确认/提示）。
UI: 提示弹窗
特点: 副本内通用确认框。

## 17. PartnerModule — 伙伴装备
伙伴装备窗口，通过 EquipModule 打开装备界面进行伙伴装备操作。支持道具 Tips 查看。
UI: `Partner/Prefab/PartnerWindow`
特点: 与 EquipModule/ItemTipsModule 协作完成伙伴装备管理。
