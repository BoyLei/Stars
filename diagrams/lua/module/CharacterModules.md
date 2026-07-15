# Lua 角色/养成域模块

> Level 3 — 8 个角色养成模块，覆盖天赋/符文/铭文/战力/药品

---

## 1. TalentModule — 天赋系统
天赋树加点/重置/属性预览。
UI: 天赋窗口
特点: 树形加点结构，支持重置。

## 2. AmuletModule — 符文系统
符文镶嵌/合成/升级。
UI: 符文窗口
特点: 镶嵌→合成→升级 三层操作链路。

**系统关系（最深View嵌套）**:
Module: `LuaModule/AmuletModule.lua.txt` (~315行)
  ↓
View: `View/UIWindow/AmuletWindow.lua.txt` (~265行)
  └── 7个子面板: DefAmulet / PolishAmulet / EmblemMenu / ExtractAmulet / EmblemSkillInfo / SelectBtn / CurrencyBar
Module→View: OnOpenAmuletMsg → BindModule + InitItems + OpenAmulet
View→Module: 镶嵌/合成/升级请求 + 通过 SelectBtn 切换子标签
跨模块: ItemTipsModule (道具Tips打开/关闭)

## 3. InscriptionModule — 铭文系统
铭文装备/升级/套装效果。
UI: 铭文窗口
特点: 套装收集驱动。

## 4. MedicineModule — 药品/消耗品
药品制作/使用/快捷栏设置。
UI: 药品窗口
特点: 制作+使用双功能。

## 5. FightPowerModule — 战力计算
战力值计算与界面展示。
特点: 汇总所有养成系统的最终战力，纯计算/展示模块。

## 6. PlayerPreviewModule — 玩家预览
玩家详细信息预览（装备/属性/伙伴），支持 6 种预览类型：玩家信息/加好友/组队邀请/公会邀请/禁言/恢复发言。
UI: 玩家预览窗口
特点: 同一个窗口支持 6 种不同业务场景。

**系统关系（6种业务场景复用）**:
Module: `LuaModule/PlayerPreviewModule.lua.txt` (~203行)
  ↓
View: `View/UIWindow/PlayerPreviewWindow.lua.txt` (~565行, 玩家装备/属性预览)
  ├── `View/UIWindow/PartnerPreviewWindow.lua.txt` (~265行, 伙伴预览)
  └── `View/UIWidget/PlayerFuncTips.lua.txt` (~118行, 功能菜单)
共用子面板: EquipItem
Module→View: OpenPlayerPreviewWindow / OpenPartnerPreviewWindow / OpenPlayerFuncTips
View→Module: 按场景类型 → 发好友申请/组队邀请/公会邀请/禁言操作
跨模块: ItemTipsModule / BagModule / BattleTeamModule / RankModule / FriendModule (5个!)

## 7. SelectorModule — 选择器
通用选择器，可用于角色/技能/物品选择。
UI: 选择器窗口
特点: 通用组件，多场景复用。

**系统关系（回调模式，零跨模块依赖）**:
Module: `LuaModule/SelectorModule.lua.txt` (~158行)
  ↓
View: `View/UIWindow/ComSelector/SelectorWindow.lua.txt` (~600行, 主窗口)
  └── `View/UIWindow/ComSelector/SelectorWords.lua.txt` (~343行, 文字选择器)
子面板: SortList / EquipRecastSelectedBoxItem
Module→View: OnOpenSelectorByType / OnOpenSelectorByItemID → BindModule + InitControls
View→Module: 纯回调模式 — OnClickOkEvent(data) / OnClickCancleEvent()
跨模块: 无（通用组件，调用方通过回调获取结果）

## 8. MindRepairModule — 心灵修复
休闲小游戏玩法，挂机奖励获取。
UI: 心灵修复窗口
特点: 轻量化休闲玩法。
