# LuaModule (C# ↔ Lua 桥接)

**命名空间：** `SGF.Module.Framework`
**继承：** `LuaModule : BusinessModule`
**文件：** `Assets/Scripts/StarFramework/Module/Framework/LuaModule.cs`
**Lua 基类：** `Assets/Res/LuaScripts/LuaModule/Core/BaseLuaModule.lua.txt`

## 职责

C# 与 Lua 业务模块的桥梁，所有 enum > 5000 的模块通过 LuaModule 创建。

## 核心字段

- `luaTable` (LuaTable) — 模块对应的 Lua 环境表
- `mSourceLuaFile` (LuaTable) — Lua 原始文件表

## Lua 侧回调委托

| C# 委托 | Lua 函数 | 触发时机 |
|---------|----------|---------|
| `OnModuleCreateEvent` | `OnModuleCreate(self, arg)` | 模块创建时 |
| `OnModuleShowEvent` | `OnModuleShow(self, arg)` | 模块显示时 |
| `OnModuleHideEvent` | `OnModuleHide(self, arg)` | 模块隐藏时 |
| `OnModuleReleaseEvent` | `OnModuleRelease(self)` | 模块释放时 |
| `OnModuleMessageEvent` | `OnModuleMessage(self, msg, args)` | 收到消息时 |

## 创建流程

```
ModuleManager.CreateModule("ChatModule")
→ Type.GetType 失败(C#无此类)
→ LuaManager.Instance.GetLuaModule("ChatModule")
→ new LuaModule("ChatModule")
→ LuaManager.Instance.CreateLuaModuleScriptAsync("ChatModule", this)
  → 加载 LuaScripts/LuaModule/ChatModule.lua.txt
  → BindLuaCall(luaTable)
    → 绑定 OnModuleCreate/OnShow/Hide/Release/Message 回调
    → 设置 self / LuaEventTable / Event 到 Lua 环境
```

## Lua 调用 C#

Lua 侧可通过以下方式调用 C#：
- `Event(target, type)` — 模块事件通信
- `LuaEventTable` — 模块事件表
- `LuaCallModuleManager` — Lua 调用 ModuleManager 接口
- `GlobalEvent` — 全局事件广播
