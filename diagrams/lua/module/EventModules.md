# Lua 活动/运营域模块

> Level 3 — 14 个活动模块，覆盖签到/活跃度/限时活动/抽卡/公告

---

## 1. EventModule — 活动系统总控
限时活动管理中心，根据 gameID（1-5）区分活动类型，计算各活动的准备时间和开启时间，按需派发到具体玩法模块。
UI: `Event/EventWindow` / `EventPopWindow`
特点: gameID 约定 — 1=侧边入口 / 2=梦境入侵 / 3=GVG / 4=野外Boss / 5=特殊活动；不同 gameID 有不同前置准备时间（5或15分钟）。

**系统关系（活动总控中枢）**:
Module: `LuaModule/EventModule.lua.txt` (~482行)
  ↓
View: `View/UIWindow/EventWindow.lua.txt` (~320行, 活动列表, 横向滚动+对象池)
  ├── `EventPopWindow.lua.txt` (~368行, 单个活动详情, 5种玩法分支)
  └── `EventSuccessWindow.lua.txt` (~166行, GVE结算)
子面板: EventItem (对象池动态创建) / EventSuccessItem / ItemGrid
Module→View: OnCalEventOpenTime → 时间窗口计算 → EventWindow:OpenEvent
View→Module: 按 PlayMode 分支 → GVE切地图 / GNG寻路NPC / PVP寻路 / PartyTime寻路
跨模块: 8个! WildBossModule / AfternoonGveModule / PvpModule / RechargeModule / RunHorseModule / DailyActModule / GamePlayCalendarModule / GamePlayChoiceModule / ItemTipsModule

## 2. DrawCardModule — 抽卡系统
抽卡主界面/十连抽/单抽，品质判定后播放 Timeline 特效和金/紫品质动画。加载独立 3D 抽卡场景展示角色模型。
UI: `DrawCard/Prefab/DrawCardWindow` / `DrawCard/Prefab/TenTimesWindow`
特点: 抽卡 3D 场景 + Timeline 动画控制器 + 品质分色特效。

**系统关系（3D场景+Timeline特效）**:
Module: `LuaModule/DrawCardModule.lua.txt` (~647行)
  ↓
View: `View/UIWindow/DrawCard/DrawCardWindow.lua.txt` (~619行, 卡池选择)
  ├── `DrawCardDetailWindow.lua.txt` (~436行, 3D模型+Timeline)
  ├── `TenTimesWindow.lua.txt` (~254行, 十连抽结果)
  ├── `DetailInfoWindow.lua.txt` (~115行, 概率+历史)
  └── `TenTimesItems.lua.txt` (~60行, 3D卡片展示)
子面板: 6个卡池Tab (DrawCardHD/XS/YQ/CZ) + RateInfo / RecordInfo / CardItem
Module→View: LoadCardScene(19) 加载独立3D场景 → DrawCardDetailWindow → PlayTimeline
View→Module: SingleDC / TenDC → GachaReq → OnGachaRet 回调
跨模块: ChatModule (抽卡结果广播聊天) / ItemTipsModule (物品/伙伴Tips) / MutiScenesMergeManager (3D场景管理)

## 3. ActivityModule — 活动签到
签到奖励领取，玩法预告（FunctionPreview）数据管理。监听 FixMessage（CheckIn/UserSundry）更新签到状态和系统开放奖励。
UI: `Activity/Prefab/SignIn/SignInWindow`
特点: 双重数据源 — 网络协议（签到领取）+ FixMessage（状态同步）。

**系统关系（多Tab聚合窗口）**:
Module: `LuaModule/ActivityModule.lua.txt` (~531行)
  ↓
View: `View/UIWindow/SignInWindow/SignInWindow.lua.txt` (~675行, 5Tab总窗口)
  ├── DailyRewardItem / AccumulateItem (签到奖励)
  ├── SevenDayPanel (七日目标)
  ├── BeginnerTaskPanel (新手任务)
  ├── GamePlayPreviewPanel (玩法预告)
  └── PartnerTaskPanel (伙伴目标)
Module→View: OnShowSignInWindow → BindModule + InitControls + Init(chekinMD)
View→Module: GetRedPoint / GetGamePlayPreviewRedPoint / IsReceivedAllPreview
跨模块: BeginnerTargetModule (新手目标红点) / ItemTipsModule (道具Tips)

## 4. AnnouncementModule — 公告系统
系统公告列表展示（更新/活动/维护）。
UI: 公告窗口
特点: 登录时主动拉取公告列表。

## 5. RunHorseModule — 跑马灯
公告滚动跑马灯显示。
UI: 跑马灯挂件
特点: 与 AnnouncementModule 配合，公告文字横向滚动。

## 6. DailyActModule — 每日活跃度
今日活跃度进度和宝箱奖励领取。
UI: 活跃度窗口
特点: 进度条驱动的多档位奖励。

## 7. OnlineRewardModule — 在线奖励
在线时长挂机奖励。
UI: 在线奖励窗口
特点: 累计在线时间解锁奖励档位。

## 8. GamePlayCalendarModule — 玩法日历
所有玩法日程总览，显示开启时间和奖励预览。
UI: 日历窗口
特点: 时间驱动，依赖服务器时间 GameManager.GetServerTimeStamp。

## 9. GamePlayChoiceModule — 玩法选择
多分支玩法选择（如选择难度/模式）。
UI: 选择窗口
特点: 玩法前置选择器。

## 10. AfternoonGveModule — 午间 GVE
公会午间 GVE 玩法，组队打 Boss 排名奖励。
UI: 午间 GVE 界面
特点: 公会玩法，与组队系统联动。

**系统关系**:
Module: `LuaModule/AfternoonGveModule.lua.txt` (~369行)
  ↓
View: `View/UIWindow/AfternoonGvePopWindow.lua.txt` (~352行, 主弹窗)
  ├── `AfternoonGveSuccessWindow.lua.txt` (结算窗口)
  ├── `View/UIWidget/EctypeAfternoonGveWidget.lua.txt` (~179行, 副本HUD)
  └── `View/UIWidget/AfternoonGveFinishWidget.lua.txt` (~122行, 奖励结算)
子面板: GngEffectItem / AfternoonGveRankPart / AfternoonGveRewardInfo
Module→View: OnGNGStartNtf / OnGNGRunNtf / OnGNGEndNtf → 按阶段打开对应窗口
View→Module: 网络消息驱动，异步加载View
跨模块: ChatModule (切换聊天根节点)

## 11. EventBossRankModule — 活动 Boss 排行
限时活动 Boss 的伤害排行榜。
UI: 排名界面
特点: 与 EventModule 时间联动，仅在活动期间有效。

## 12. RewardsPopModule — 奖励弹窗
通用奖励获得弹窗，展示道具/货币/经验。
UI: 奖励弹窗
特点: 跨模块复用，传入奖励列表即可展示。

## 13. EventTipsModule — 活动提示挂件
GVE 活动内的提示挂件，监听 AOI 属性 GVEBonus 变化实时刷新。
UI: `Event/EventTipsWidget`
特点: 通过 EntityBase.Data 的 AddAttributeChangeLuaAction 监听属性同步。

## 14. SevenDaysSignModule — 七日签到
七日签到测试模块，含大量测试代码和事件通讯验证。
特点: 半成品，目前主要用于 Event/LuaModuleDispatcher 通讯机制验证。
