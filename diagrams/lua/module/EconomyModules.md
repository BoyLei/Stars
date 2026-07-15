# Lua 经济/交易域模块

> Level 3 — 17 个经济模块，覆盖商店/背包/装备/交易行/拍卖/充值全链路

---

## 1. ShopModule — 商店系统
普通商店和 NPC 商店，支持购买掉落预览。
UI: `Shop/ShopWindow` / `Shop/NpcShopWindow`
特点: 购买前可预览购买产出的掉落物。

**系统关系**:
Module: `LuaModule/ShopModule.lua.txt` (~488行)
  ↓
View: `View/UIWindow/ShopWindow.lua.txt` (~472行, 普通商店) + `View/UIWindow/NpcShopWindow.lua.txt` (~306行, NPC商店)
子面板: ShopItem / SelectBtn / CurrencyBar (共用)
Module→View: shopWindowView:InitItems / OpenShop / RefeshShop / SearchItem
View→Module: OpenBuyMenu / CheckBuyLimit / SendShopBuyReq
跨模块: ItemTipsModule (道具Tips) / ItemBuyModule (购买弹窗) / BankModule (兑换跳转)

## 2. ExchangeModule — 交易行（拍卖行）
玩家间自由交易，支持 5 种搜索方式（模糊搜索/类型查询/商品查询/SubType 查询/指定道具）。买卖日志最多保留 24 小时。
UI: 拍卖行窗口
特点: 经济域最大模块，模糊搜索通过 `LuaModuleDispatcher` 跨模块调用。

**系统关系**:
Module: `LuaModule/ExchangeModule.lua.txt` (~1166行)
  ↓ 嵌入 RechargeWindow 内显示:
View: `View/UIWindow/Exchange/ExchangeUI.lua.txt` (~244行, 购买/出售/关注三tab)
  ├── Purchase (购买) / Sell (出售) / Attention (关注)
  └── 独立入口: `View/UIWindow/BankWindow/BankWindow.lua.txt` (金币兑换, 由BankModule管理)
Module→View: ExchangeUI:SetFinderState / ExchangeUI:Close
View→Module: 15+ 接口 (模糊搜索/类型查询/商品查询/关注/日志/排序/翻页)
跨模块: SelectorModule (道具品类筛选器)

## 3. RechargeModule — 充值/竞拍
充值购买钻石/礼包，同时实现拍卖竞拍功能（名称有历史包袱）。竞拍支持普通拍卖和公会竞拍两种类型。
UI: `Recharge/Prefab/RechargeWindow`
特点: 同时承载充值和竞拍两个独立业务。

**系统关系**:
Module: `LuaModule/RechargeModule.lua.txt` (~861行)
  ↓
View: `View/UIWindow/Auction/RechargeWindow.lua.txt` (~270行)
  ├── 子面板: AuctionUI (`View/UIWindow/Auction/AuctionUI`) — 竞拍页
  │   └── ItemTypeGroupBtn / AuctionGoodsItem / UnionWarehouse
  └── 子面板: ExchangeUI (`View/UIWindow/Exchange/ExchangeUI`) — 交易行页
      └── Purchase / Sell / Attention
  共用: CurrencyBar (货币栏)
Module→View: RechargeWindow:OnShow / UpdateData / UpdateItemData / UpdateDivvyCount
View→Module: SendAuctionReq / SendAuctionBidReq / GetCurDefaultAuctionType / IsTypeActive
跨模块: ExchangeModule (道具跳转模糊搜索) / BankModule (兑换跳转)

## 4. ItemTipsModule — 道具 Tips
道具详情弹窗，显示属性/来源/功能按钮。
UI: 道具 Tips 弹窗
特点: 被多个模块引用（Bag/Equip/Partner 等），是最常用的公共 UI。

## 5. BagModule — 背包
道具查看/整理/拆分/扩展。
UI: 背包窗口
特点: 基础道具管理入口。

**系统关系**:
Module: `LuaModule/BagModule.lua.txt` (~201行)
  ↓
View: `View/UIWindow/BagWindow.lua.txt` (~1517行, 超大型View)
子面板: ItemGrid / SelectBtn / ObjectPool
Module→View: bagWindowView:InitItems / RefeshBag / OnMenuBtnClick
View→Module: OpenItemTips / CloseItemTips / OpenEquip / CloseEquip
跨模块: EquipModule (联动打开装备) / ItemTipsModule (道具Tips) / DropInfoListModule (掉落)

## 6. EquipModule — 装备
装备穿戴/卸下/属性查看。
UI: `Equip/Prefab/EquipWindow` / `EquipTipsWidget`
特点: 与 PartnerModule 协作 — 伙伴通过此模块打开装备界面。

## 7. EquipUpgradeModule — 装备强化
装备强化/进阶/升星。
UI: 装备强化界面
特点: 多层强化链路（强化→进阶→升星）。

## 8. QuickEquipModule — 快速穿戴
一键快速穿戴最佳装备，自动计算最优装备组合。
UI: 快速穿戴窗口
特点: 自动最优化选择，无需手动逐件比较。

## 9. ItemBuyModule — 道具购买
道具购买弹框，支持数量/价格/货币选择。
UI: 购买确认弹窗
特点: 多货币购买选择。

## 10. ItemUseModule — 道具使用
道具使用确认弹框，支持批量使用。
UI: 使用确认弹窗
特点: 支持单次和批量两种使用模式。

## 11. ItemResolveModule — 道具分解
道具分解为材料。
UI: 分解窗口
特点: 道具回收/材料产出。

## 12. BankModule — 银行/仓库
个人仓库扩展/存取。
UI: 仓库窗口
特点: 背包扩展存储。

## 13. CommercializationModule — 商业化
限时礼包/首充/特惠活动。
UI: 商业化窗口
特点: 红点检测驱动弹窗。

**系统关系**:
Module: `LuaModule/CommercializationModule.lua.txt` (~167行)
  ↓ 不持有View引用，通过 OpenWidgetAsync 按 OpenType 路由:
View: `View/UIWidget/FirstPayWidget.lua.txt` (首充, ~365行)
  ├── `View/UIWidget/MoonCardWidget.lua.txt` (月卡, ~315行)
  └── `View/UIWidget/BattlePassWidget.lua.txt` (通行证, ~365行)
      └── BattlePassRewards / BattlePassTasks (子面板)
Module→View: OpenType 路由: 1→FirstPay / 2→MoonCard / 3→BattlePass
View→Module: 三个Widget间通过 LuaModuleDispatcher 互跳
跨模块: ItemTipsModule (月卡奖励图标Tips)

## 14. FuncTipsModule — 功能提示
通用提示弹窗，确认/取消/信息展示。
UI: 通用提示窗口
特点: 跨模块复用，所有需要确认弹窗的地方均可调用。

## 15. AnnounceModule — 掉落公告
道具获得时的掉落公告队列，按先进先出逐个弹出展示。
UI: `Announce/AnnounceWidget`
特点: 队列模式确保公告不重叠。

## 16. ItemManagerModule — 道具管理器
监听金币变化（PropSyncListID 协议）同步货币数据。
特点: 底层数据监听模块，不直接提供 UI。

## 17. LootControllerModule — 战利品特效
掉落物飞向角色的特效控制器，按品质（1-5）分 5 个对象池。场景切换时自动回收所有特效。
特点: 对象池管理，场景切换时全局回收。
