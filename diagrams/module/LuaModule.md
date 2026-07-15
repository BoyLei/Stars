# LuaModule

**继承：** `LuaModule : BusinessModule`
**文件：** `Assets/Scripts/StarFramework/Module/Framework/LuaModule.cs`

## 职责

C# ↔ Lua 桥接适配器，所有 enum > 5000 的模块通过此桥接创建。

## 核心字段

- `luaTable` — 模块对应的 LuaTable 环境

## 生命周期与绑定

C#↔Lua 生命周期映射和 `BindLuaCall(tb)` 绑定流程 → 详见 `../lua/LuaModuleBridge.md`（包含完整创建流程、回调映射表、Lua 调用 C# 接口）。
