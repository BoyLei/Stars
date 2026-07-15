# ModuleDef

**命名空间：** `StarProjectDef`
**文件：** `Assets/Scripts/StarGame/Module/ModuleDef.cs`

## 职责

模块枚举定义，C# 模块和 Lua 模块的分界线。

## 枚举结构（部分）

```csharp
enum Name {
    None,
    LoginModule,       // C# 模块
    StarWorldModule,   // C# 模块
    // ... 更多 < 5000 的 C# 模块
    LuaModuleType = 5000,  // C#/Lua 分界线
    AvgLuaModule,     // > 5000 = Lua 模块
    TaskModule,
    ChatModule,
    // ... 更多 Lua 模块
}
```

## 关键规则

- `enum < 5000` → C# 模块（`CsModuleDef`），通过反射实例化
- `enum > 5000` → Lua 模块（`LuaModuleDef`），通过 LuaManager 加载
- `GetModuleDefState(name)` — 获取模块定义状态
- `GetModuleName(def)` — 获取模块名称字符串
