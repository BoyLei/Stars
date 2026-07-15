# SocketDataPools

**类型：** `Singleton`
**文件：** `Assets/Scripts/StarFramework/Network/SocketDataPools.cs`

## 职责

`SendMsgData` 对象池，减少 GC 分配。

## 核心方法

| 方法 | 说明 |
|------|------|
| `GetMsgData()` | 从池中获取 SendMsgData |
| `FormateRetMsgData(cmd, data)` | 格式化回调消息 |
