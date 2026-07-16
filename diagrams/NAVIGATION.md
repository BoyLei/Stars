# 模块导航总表

> Level 1 — 顶层索引。进入某模块前先读对应 `_Nav__*.md`。
> 然后根据需要再深入 Level 3 子模块详情。
> 
> **AI Agent 入口**: 项目根目录 `AGENTS.md` — 包含完整技术栈/架构图/开发约束/路径速查

## 系统模块一览

| 编号 | 模块 | Level 2 导航 | Level 3 详情 | 说明 |
|------|------|-------------|------|------|
| A | **网络模块** | `network/_Nav__Network.md` | Socket 协议等 | Socket/协议编解码/消息分发/重连 |
| B | **战斗模块** | `battle/_Nav_Battle.md` | Skill/Buff 等 | BattleManager/技能/Buff/Bullet/被动/副本 |
| C | **UI 框架** | `ui/_Nav__UI.md` | Page/Window/Widget | UIManager/UIPage/UIWindow/UIWidget |
| D | **Entity 系统** | `entity/_Nav__Entity.md` | 工厂/继承/View | 实体工厂/继承体系/View显示层 |
| E | **Lua 脚本模块** | `lua/_Nav__Lua.md` | 6域+公共库 | Lua业务模块按6域分组详解/LuaView/公共库/数据结构 |
| F | **模块系统** | `module/_Nav__Module.md` | 基类/通信 | ModuleManager/C#模块详情/通信机制 |
| G | **常驻服务** | `services/_Nav__Service.md` | 核心+补充 | ServiceModule 单例服务 |
| H | **项目总览** | `architecture/_Nav__Arch.md` | StarProjectDef/InitFlow | 三层架构/初始化全链路/StarProjectDef |
| W | **工作流** | `workflows/sync-md-on-pull.md` | 自动化 | AI 工作流: git pull 后自动同步 Lua MD |

## 新增/更新文件 (2026-07-05 第二次更新)

### 本轮修正（基于 codegraph 实际源码验证）
| 文件 | 修正内容 |
|------|----------|
| `architecture/InitFlow.md` | InitServices 调用序列修正，增加编号 |
| `architecture/StarProjectDef.md` | UIDef.cs 路径修正 `StarGame/UI/UIDef.cs` |
| `battle/L4_COMBAT_EFFECT.md` | ServerControlStageEntityBase 源文件路径修正 `Game/Skill/Base/` |

### 第三轮：Lua 模块详情补充（基于实际源码阅读）

| 文件 | 变更 | 说明 |
|------|------|------|
| `lua/module/SocialModules.md` | **新增** | Chat/Friend/Guild/Mail/Team 等社交模块代码分析 |
| `lua/module/EconomyModules.md` | **新增** | Shop/Exchange/Recharge 等经济模块 + 交易行搜索器 |
| `lua/module/EventModules.md` | **新增** | 活动模块 + gameID 时间计算规则 + 抽卡 3D Timeline |
| `lua/module/CombatModules.md` | **新增** | Pvp/Arena/SecretArea 等战斗模块 |
| `lua/module/CharacterModules.md` | **新增** | 天赋/符文/铭文/战力 等养成模块 |
| `lua/module/SystemModules.md` | **新增** | Task/Wanted/LivingSkills 等系统模块 |
| `lua/_Nav__Lua.md` | **更新** | Level 3 索引更新 |
| `lua/module/LuaModuleList.md` | **重写** | 全量重构 |

### 第一轮新增 (2026-07-05 上午)

| 文件 | 变更 | 说明 |
|------|------|------|
| `../AGENTS.md` | **新增** | AI Agent 项目入口 (AGENTS.md 规范) |
| `architecture/StarProjectDef.md` | **新增** | GameEnums/GameConfig/ModuleDef/UIDef/WrapData |
| `architecture/InitFlow.md` | **新增** | 完整初始化链路 (Prepare→AppMain→Login) |
| `battle/BATTLE_OVERVIEW.md` | **新增** | SkillDispatcher/Controller/Entity/Stage/Buff/Bullet/Passive 分层总览 |
| `services/ServicesSupplement.md` | **新增** | 16个未文档化服务 (SDK/User/Business/Trigger等) |
| `module/CsBusinessModules.md` | **新增** | 10+ C# 业务模块详情 |
| `lua/module/LuaModuleList.md` | **重写** | Lua模块按6域分组+绑定机制 |
| `architecture/_Nav__Arch.md` | **更新** | 增加子模块索引 |
| `battle/_Nav_Battle.md` | **更新** | 增加 SkillSystem 引用 |
| `services/_Nav__Service.md` | **更新** | 增加 G15~G30 服务 + 补充服务 |
| `module/_Nav__Module.md` | **更新** | 增加 CsBusinessModules 引用 |

## 文件组织

```
diagrams/
├── NAVIGATION.md            ← Level 1 (本文件)
├── network/         Socket/协议/编码/加密/重连
├── battle/          BattleManager/技能/Buff/Bullet/被动/副本
├── ui/              UIManager/UIPage/UIWindow/UIWidget/队列
├── entity/           工厂/继承体系/View显示层/数据对象池
├── lua/              6域模块详解 + LuaView + 公共库 + 数据结构
├── module/           ModuleManager/基类/通信/C#业务模块
├── services/         14 核心服务 + ServicesSupplement
└── architecture/     StarProjectDef/初始化全链路
```

## 使用规则

1. 修改某模块前，先读对应 `_Nav__*.md` 了解子模块划分
2. 需要细节时再读 Level 3 详情文档
3. 修改代码后，**更新对应 Level 3 文档**保持同步
4. 新增子模块时，更新 `_Nav__*.md` 和对应的 Level 3 文档
5. AI Agent 使用本项目的 **入口文件是 `AGENTS.md`**
