# Lua 系统/信息域模块

> Level 3 — 11 个系统模块，覆盖任务/排行榜/通缉/藏宝图/生活技能/剧情演出

---

## 1. TaskModule — 任务系统（核心）
主线/支线/日常任务的生命周期管理（接受→追踪→提交→奖励）。内含剧情播放功能（Type=1 对话演出 / Type=2 图片 / Type=3 分镜）。
UI: `TaskWindow/Prefab/TaskWindow` / `HudTaskWidget` / `PlotWidget` / `PlotImageWindow` 等 6 个
特点: 通过 GlobalEvent.OnRoleCreateComplete 触发任务初始化。

**系统关系（6 View + 6 跨模块）**:
Module: `LuaModule/TaskModule.lua.txt` (~542行)
  ↓
View: `View/UIWindow/TaskWindow/HudTaskWidget.lua.txt` (~339行, HUD任务追踪)
  ├── `TaskWindow.lua.txt` (~543行, 任务面板)
  ├── `PlotWidget.lua.txt` (~128行, 对话演出)
  ├── `PlotImageWindow.lua.txt` (~71行, 图片演出)
  ├── `BlockWidget.lua.txt` (~114行, 黑屏遮罩)
  └── `View/UIWidget/EctypeTargetWidget.lua.txt` (~186行, 副本目标)
子面板: TeamWidget / TaskWidget / TaskGroupItem / TaskTargetItem / EctypeTargetItem / GVEWidget
Module→View: OnModuleShow → HudTaskWidget异步打开 / UpdateTaskData / UpdateTeamData / PlayPlot
View→Module: TaskWindow:OnClickGotoBtn → 通缉寻路 / BtnWorldLine → 世界线跳转
跨模块: TeamModule (队伍同步) / SecretAreaModule (副本警告) / WorldLineModule / WantedModule / ChatModule (任务共享) / InterActionModule

## 2. WantedModule — 通缉玩法
通缉 Boss 挑战（接取→追踪→击杀→奖励）。与组队系统联动（E_TeamPlayType.PLAY_WTASK=1001）。
UI: 通缉窗口
特点: 系统域最大模块，作为组队子玩法运行。

**系统关系**:
Module: `LuaModule/WantedModule.lua.txt` (~752行, 系统域最大)
  ↓
View: `View/UIWindow/Wanted/WantedWindow.lua.txt` (~249行, 通缉主窗口)
  ├── `WantedChallengeSuccessWindow.lua.txt` (~169行, 成功结算)
  ├── `WantedRankWidget.lua.txt` (~99行, 排行挂件)
  └── `WantedFailWindow.lua.txt` (~183行, 失败/复活)
子面板: ItemWantedTarget / ItemWantedSuc / ItemWantedRank
Module→View: OnOpenWantedWindow → BindModule + InitData / OpenFailWindow / OpenSuccessWindow
View→Module: SendLeaveInstanceReq / SendReviveReq / GetAllTarNumInfo (数据查询)
跨模块: TeamModule (队伍状态检查/队长判断/组队大厅)

## 3. LivingSkillsModule — 生活技能
采集/制造/材料仓库完整循环。支持跨地图自动寻路到最近矿点。主角创建成功时自动请求采集物信息。
UI: `LivingSkills/Prefab/LivingSkillsWindow` / `LivingWarehouseWindow` / `LifeSkillCreatWidget`
特点: 跨地图矿点寻路 — 本地图无可用矿点时自动搜索其他地图。

## 4. AvgLuaModule — AVG 剧情演出
AVG 文字冒险/对话演出/分支选择。
UI: AVG 剧情窗口
特点: 支持分支选择和跳过功能。

## 5. RankModule — 排行榜
战力/等级/竞技场/活动等多维度排行榜。
UI: 排行榜窗口
特点: 多 Tab 切换，按类型请求不同排行数据。

## 6. BeginnerTargetModule — 新手目标
新手/回归玩家的阶段性目标指引和奖励引导。
UI: 新手目标窗口
特点: 分阶段解锁，逐步引导。

## 7. TreasureModule — 藏宝图
藏宝图使用/寻宝/挖掘奖励。
UI: 藏宝图窗口
特点: 使用道具触发，按坐标寻宝。

## 8. WorldLineModule — 世界线管理
世界线查看/切换。
UI: 世界线窗口
特点: 分线管理，用于分流玩家。

**系统关系**:
Module: `LuaModule/WorldLineModule.lua.txt` (~196行)
  ↓
View: `View/UIWindow/WorldLineWindow.lua.txt` (~656行, 超大View, 对象池模式)
  └── `View/UIWidget/WorldLinePlotWidget.lua.txt` (~97行, 剧情Widget)
子面板: 6个 (ObjectPool动态加载): WorldLineItem / WorldLinePopMenu / WorldLineDot / WorldLineLine / SelectBtn
Module→View: OnOpenWorldLine → BindModule + InitItems + Refesh / TaskStageChange → PlotWidget打开
View→Module: MakeWorldLineRewardReq → 领取奖励 / OnClickCheckBtn → 剧情跳转
跨模块: ChatModule (领奖后模拟聊天消息广播)

## 9. ObjectInteractiveModule — 物体交互
场景内可交互物列表展示（宝箱/采集/传送门）。
UI: 交互列表
特点: 支持 3 种本地实体类型（宝箱/传送门/通缉实体）。

## 10. SystemOpenTipsModule — 系统解锁提示
系统功能解锁弹窗，等级到达或任务完成时触发。
UI: 解锁提示窗口
特点: 被动触发，由 SystemOpenManager 控制。

## 11. LoginModule — 登录模块
空壳登录模块，仅完成模块注册，无实际消息处理。
特点: 预留扩展点，当前无业务逻辑。
