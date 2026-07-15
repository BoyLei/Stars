# ModuleManager

**命名空间：** `SGF.Module.Framework`
**继承：** `ServiceModule<ModuleManager>` → `Module`
**文件：** `Assets/Scripts/StarFramework/Module/Framework/ModuleManager.cs`

## 职责

业务模块的统一管理器，负责模块的创建、获取、释放、消息发送和事件管理。

## 核心字段

- `m_mapModules` — 已创建的模块字典
- `m_mapCacheMessage` — 消息缓存(目标模块未创建时暂存)
- `bModuleStack` — 模块堆栈

## 核心方法

| 方法 | 说明 |
|------|------|
| `Init(domain)` | 初始化模块管理器 |
| `CreateModule(enumDef)` | 创建模块(自动判断C#/Lua) |
| `GetModule(enumDef)` | 获取模块实例 |
| `ReleaseModule(enumDef)` | 释放模块 |
| `SendMessage(target, func, args)` | 模块间消息通信 |
| `Event(target, type)` | 获取事件表 |
| `ShowModule(enumDef, arg)` | 显示模块 |
| `HideModule(enumDef, arg)` | 隐藏模块 |

## 创建逻辑

```
CreateModule("Xxx"):
  1. Type.GetType("StarProject.Module.XxxModule")
     → 找到: new XxxModule()  (C# 模块)
     → 未找到: 走 Lua 路径
  2. LuaManager.GetLuaModule("XxxModule")
     → new LuaModule() → BindLuaCall(LuaTable)
```

## 通信机制

> 详见 `_Nav__Module.md` 通信机制小节。TODO: 这里补 ModuleManager 实现细节（如消息缓存队列算法、反射调用性能策略等）。
