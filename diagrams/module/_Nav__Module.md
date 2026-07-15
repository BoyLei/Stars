# 模块系统导航

> Level 2 — 模块管理系统子模块划分。

## 子模块列表

| 编号 | 子模块 | Level 3 详情 | 职责 |
|------|--------|-------------|------|
| F1 | ModuleManager | `ModuleManager.md` | 模块生命周期管理(创建/获取/释放/通信) |
| F2 | Module基类 | `Module.md` | 抽象基类 |
| F3 | BusinessModule | `BusinessModule.md` | 业务模块基类(消息/事件/显示) |
| F4 | ServiceModule | `ServiceModule.md` | 泛型单例服务基类 |
| F5 | LuaModule | `LuaModule.md` | C#↔Lua桥接适配器 |
| F6 | ModuleDef | `ModuleDef.md` | 模块枚举定义(5000分界线，C# + Lua 模块) |
| F7 | C# 业务模块 | `CsBusinessModules.md` | C# 模块详情 (Login/Tutorial/Trigger/Item等) |

## 通信机制

- **SendMessage**: 模块间反射调用，支持消息缓存
- **Event**: 事件订阅，支持预监听(PreListen)
- **GlobalEvent**: 全局广播(跨模块/跨层)

## 创建规则

enum < 5000 → C# 反射实例化
enum > 5000 → LuaManager.GetLuaModule → LuaModule绑LuaTable
