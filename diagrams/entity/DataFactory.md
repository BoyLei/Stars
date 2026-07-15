# SimpleDataFactory / DynamicDataFactory

**类型：** `static class`
**文件：** `Assets/Scripts/StarGame/Game/Entity/Factory/`

## SimpleDataFactory

简单数据对象池。

| 方法 | 说明 |
|------|------|
| `InstanceData<T>()` | 创建数据实例 |
| `ReleaseData(data)` | 释放数据（回池） |

## DynamicDataFactory

动态数据对象池，支持队列管理。

| 方法 | 说明 |
|------|------|
| `InstanceData<T>()` | 创建动态数据实例 |
| `ReleaseData(data)` | 释放动态数据（回池） |
