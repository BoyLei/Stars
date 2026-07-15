# Lua 脚本模块导航

> Level 2 — Lua 系统子模块划分。修改 Lua 代码前先读本文件。

## 子模块列表

| 编号 | 子模块 | Level 3 详情 | 职责 |
|------|--------|-------------|------|
| E1 | LuaModule桥接 | `LuaModuleBridge.md` | C# LuaModule ↔ LuaTable 映射 |
| E2 | Lua业务模块列表 | `module/LuaModuleList.md` | 74个模块按域分组(社交/经济/活动等) |
| E3 | Lua 社交域 | `module/SocialModules.md` | 8个社交模块 (Chat/Friend/Guild/Mail/Team等) |
| E4 | Lua 经济域 | `module/EconomyModules.md` | 17个经济模块 (Shop/Bag/Equip/Exchange/Recharge等) |
| E5 | Lua 活动域 | `module/EventModules.md` | 14个活动模块 (Activity/DrawCard/Event/Announcement等) |
| E6 | Lua 战斗域 | `module/CombatModules.md` | 17个战斗模块 (Pvp/Arena/SecretArea/SkillWindow等) |
| E7 | Lua 角色域 | `module/CharacterModules.md` | 8个角色模块 (Talent/Inscription/Amulet/FightPower等) |
| E8 | Lua 系统域 | `module/SystemModules.md` | 11个系统模块 (Task/Wanted/Rank/LivingSkills等) |
| E9 | Lua View 层 | `view/LuaView.md` | UIWindow/UIWidget/UIPage |
| E10 | Lua 公共库 | `common/LuaCommon.md` | Class/Config/Define/Helper/Global |
| E11 | Lua 数据结构 | `struct/LuaStruct.md` | 13个结构体 |
| E12 | Lua 系统总览 | `LuaSystem.md` | 架构/位置/关键机制 |

## 模块创建流程

```
ModuleManager.CreateModule("XxxModule")
→ C#反射查找失败
→ LuaManager.GetLuaModule("XxxModule")
→ new LuaModule() → BindLuaCall(LuaTable)
→ Lua侧: LuaScripts/LuaModule/XxxModule.lua.txt
```
