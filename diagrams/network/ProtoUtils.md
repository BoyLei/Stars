# ProtoUtils / PBSerializer

**文件：** `Assets/Scripts/StarFramework/Network/ProtoUtils.cs`

## 职责

Protobuf 消息的工具处理和序列化。

## ProtoUtils (static)

| 方法 | 说明 |
|------|------|
| `DeserializePbMsg(data, type)` | Protobuf 反序列化 |
| `ParseRpcHeader(data)` | 解析 RPC 头部 |

## PBSerializer

| 方法 | 说明 |
|------|------|
| `Serialize<T>(obj)` | 序列化 |
| `Deserialize<T>(data)` | 反序列化 |
