# MsgRetManager

**继承：** `ServiceModule<MsgRetManager>` → `Module`
**文件：** `Assets/Scripts/StarFramework/Network/MsgRetManager.cs`

## 职责

消息回调管理器，注册回调并在收到响应时执行。

## 核心方法

| 方法 | 说明 |
|------|------|
| `RegisterRet(cmd, callback)` | 注册消息回调 |
| `ProcessRet(cmd, data)` | 处理回调 |
