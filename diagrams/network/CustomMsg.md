# CustomMsg

**类型：** `Singleton`
**文件：** `Assets/Scripts/StarFramework/Network/CustomMsg.cs`

## 职责

自定义消息的协议号 ↔ 类型映射，支持运行时反射。

## 核心字段

- `cmdTypeMap` — `Dictionary<cmd, Type>` 命令码到类型的映射
- `cmdIdMap` — 反向映射

## 核心方法

| 方法 | 说明 |
|------|------|
| `Register(cmd, type)` | 注册协议号与类型的绑定 |
| `GetType(cmd)` | 根据命令码获取消息类型 |

## CustomMsgID (enum)

| 枚举值 | 说明 |
|--------|------|
| `ClientVerifyReq = 1` | 客户端验证请求 |
| `ClientVerifySucceedRet = 2` | 验证成功 |
| `ClientVerifyFailedRet = 3` | 验证失败 |
| `HeartBeat = 4` | 心跳 |
| `RPCMsg = 58` | RPC 消息 |
