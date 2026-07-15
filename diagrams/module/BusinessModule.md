# BusinessModule

**继承：** `BusinessModule : Module`
**文件：** `Assets/Scripts/StarFramework/Module/Framework/BusinessModule.cs`

## 职责

业务模块基类，提供消息处理/事件/显示生命周期。

## 核心方法

| 方法 | 说明 |
|------|------|
| `HandleMessage(msg, args)` | 通过反射调用处理方法 |
| `Show(arg)` / `Hide(arg)` | 显示/隐藏 |
| `Save()` / `Load()` | 数据持久化 |
| `GetEventTable()` | 获取事件表 |
| `Event(type)` | 注册/获取事件 |

## 特性

- 自动通过反射获取模块名称
- 支持生命周期管理（Create → Show → Hide → Release）
- 消息处理通过 `OnModuleMessage_Prefix` 方法名约定
