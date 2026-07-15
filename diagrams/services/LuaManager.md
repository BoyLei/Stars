# LuaManager

**继承：** `ServiceModule<LuaManager>` → `Module`
**文件：** `Assets/Scripts/StarGame/Service/LuaManager/LuaManager.cs`

## 职责

XLua 虚拟机管理，Lua 模块加载/桥接/定时更新。

## 核心功能

- XLua 虚拟机初始化与生命周期管理
- Lua 模块创建：`CreateLuaModuleScriptAsync(name, luaModule)`
- Lua 全局环境管理：`LuaManager.Global`
- Lua 模块定时更新
- Lua ↔ C# 桥接绑定
