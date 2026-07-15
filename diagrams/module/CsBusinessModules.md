# C# 业务模块详情

> Level 3 — StarGame/Module/ 下 C# 业务模块的详细信息。Lua 模块见 `diagrams/lua/module/LuaModuleList.md`。

---

## 模块判定规则

```csharp
ModuleDef.GetModuleDefState(enum):
  enum < LuaModuleType(5000) → CsModuleDef  → C# 反射实例化 new BusinessModule()
  enum > LuaModuleType(5000) → LuaModuleDef → LuaManager.GetLuaModule() + LuaTable 绑定
```

> **⚠️ 同名模块注意**: C# 和 Lua 有同名模块（如 ChatModule、TaskModule），它们职责不同：
> - **C# ChatModule**（12文件, Module/ChatModule/）— 战斗域通用聊天控件管理（频道切换底层框架）
> - **Lua ChatModule** — 社交域具体聊天业务逻辑（发送/接收/频道UI/弹幕），见 `../lua/module/SocialModules.md`
> - **C# TaskModule**（19文件, 9cs）— 任务系统底层框架（协议/实体/任务数据结构）
> - **Lua TaskModule** — 任务系统 UI/流程层（接受→追踪→提交→剧情），见 `../lua/module/SystemModules.md`
> - C# 模块提供底层框架支撑，Lua 模块实现具体 UI 和业务流程，两者通过 ModuleManager 协作

---

## C# 模块一览（按 AppMain.InitBusiness 注册顺序）

| 编号 | 模块名 | 枚举值 | 文件位置 | 职责 |
|------|--------|--------|----------|------|
| M1 | **LoginModule** | <5000 | `Module/Login/` | 登录流程管理（账号验证/创建角色/进入游戏） |
| M2 | **TutorialModule** | <5000 | `Module/TutorialModule/` | 新手引导系统（分步引导/状态机） |
| M3 | **TriggerModule** | <5000 | `Module/TriggerModule/` | 触发器调度（NPC/区域/事件触发） |
| M4 | **InterActionModule** | <5000 | `Module/InterActionModule/` | 交互系统（NPC交互/物件交互） |
| M5 | **ItemControllerModule** | <5000 | `Module/ItemController/` | 道具管理（C#端，含装备/符文等） |
| M6 | **WorldMapModule** | <5000 | `Module/WorldMapModule/` | 世界大地图（导航/传送/标记） |
| M7 | **RayCheckModule** | <5000 | `Module/RayCheckModule/` | 射线检测（遮挡半透/点击选择） |
| M8 | **RingTaskModule** | <5000 | `Module/RingTaskModule/` | 环任务系统 |
| M9 | **ScenePlayModule** | <5000 | `Module/ScenePlayModule/` | 场景玩法管理 |
| M10 | **PlayerLocalCache** | <5000 | `Module/PlayerLocalCache/` | 玩家本地缓存数据模块 |

---

## 模块详细说明

### M1. LoginModule — 登录模块

**文件**: `Module/Login/`  
**类型**: C# (enum < 5000)  
**注册**: `InitBusiness` 第一个创建  
**显示**: `TravelToScene → ShowModule(LoginModule)`

**职责**: 管理从登录到进入游戏的完整流程：
- SDK 登录结果验证 → 发送登录协议 → 服务器验证
- 创建角色流程（创角→选角→确认）
- 进入游戏后的场景加载和玩家数据初始化

**关键交互**:
- `SDKManager` → 获取登录 Token
- `UserManager.UpdateMainUserData()` → 设置用户数据
- `NetworkManager` → 发送/接收登录协议
- `UIManager` → 打开登录/创角 UI 页面

---

### M2. TutorialModule — 新手引导

**文件**: `Module/TutorialModule/` (10 文件, 5 cs)  
**类型**: C#  
**注册**: 仅在非 Editor 模式下创建 (`!isEditorMode`)

**职责**: 管理新手引导的步骤状态机：
- 引导步骤配置加载
- 遮罩/高亮/点击拦截
- 步骤推进与状态保存
- 强制引导与跳过逻辑

**设计要点**:
- 引导步骤分为强制性（不可跳过）和可跳过两种
- 引导状态通过 LocalDataManager 持久化
- 引导期间会通过 `MainPageCommond` 位掩码控制 UI 显示

---

### M3. TriggerModule — 触发器模块

**关键词**: `触发区域`, `NPC检测`, `环境触发器`

**职责**: 调度所有场景触发器：
- NPC 交互触发（对话/任务/商店）
- 区域进入/离开触发
- 事件触发（剧情/特效）
- 与 `TriggerEntityManager` 和 `TriggerEntityManager` 配合

---

### M4. InterActionModule — 交互模块

**职责**: 管理玩家与游戏世界的交互：
- NPC 交互：对话/接任务/交易
- 物件交互：宝箱/机关/传送门
- 交互 UI 弹窗管理
- 交互状态机

---

### M5. ItemControllerModule — 道具管理

**文件**: `Module/ItemController/` (18 文件, 8 cs)  
**职责**: C# 端道具核心管理：
- 装备系统（EquipModule / EquipSlotController / EquipUpgrade）
- 符文系统（AmuletModule）
- 鸣器系统（InscriptionModule）
- 药品系统（MedicineModule）
- 道具使用/购买/分解 流程

**关联模块**: 与大量 Lua 模块配合（BagModule / ItemTipsModule / QuickEquipModule 等）

---

### M6. WorldMapModule — 世界地图

**文件**: `Module/WorldMapModule/` (9 文件, 4 cs)  
**职责**: 世界大地图系统：
- 地图加载与显示
- 传送点管理
- 地图标记（任务/玩家/NPC）
- 小地图更新

**关联**: `GameConfig.MAP_COMMON_CACHE` / `MAP_COMMON_MAP_CACHE`

---

### M7. RayCheckModule — 遮挡检测

**职责**: 场景点击和遮挡处理：
- 角色被遮挡时半透处理
- 场景物件点击选择
- 射线检测优化

---

### M8. RingTaskModule — 环任务

**职责**: 环任务系统（连续任务链），管理多轮任务的进度、奖励和重置。

---

### M9. ScenePlayModule — 场景玩法

**职责**: 场景内玩法管理，如机关解谜、环境互动等场景内特定玩法。

---

### M10. PlayerLocalCache — 玩家本地缓存

**职责**: 玩家相关的本地数据缓存模块，管理离线可用数据。

---

## 大型 C# 模块（代码较多）

| 模块 | 文件数 | 说明 |
|------|--------|------|
| **StarWorldModule** | 9 文件, 4 cs | 场景战斗模块，管理世界场景中的战斗逻辑 |
| **TaskModule** | 19 文件, 9 cs | 任务系统（大量业务逻辑） |
| **TutorialModule** | 10 文件, 5 cs | 新手引导 |
| **ChatModule** | 12 文件, 6 cs | 聊天系统 |
| **ItemController** | 18 文件, 8 cs | 道具管理 |

---

## 模块命名约定

- C# 模块文件在 `Module/[ModuleName]/` 目录下
- 主类命名为 `[ModuleName]` 如 `LoginModule : BusinessModule`
- 每个模块通过 `ModuleManager.CreateModule(ModuleDef.Name.XXX)` 注册
- C# 模块内可调用 Lua 模块通过 `ModuleManager.SendMessage(luaModuleName, method, args)`

> **通信机制**: 详见 `_Nav__Module.md`（SendMessage/Event/GlobalEvent 三种机制）。
