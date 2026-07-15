# SocketUtils

**类型：** `static class`
**文件：** `Assets/Scripts/StarFramework/Network/SocketUtils.cs`

## 职责

静态工具类，数据格式化/解析/加密/解密的总入口。

## 核心方法

| 方法 | 说明 |
|------|------|
| `FormateData(cmd, data, dataType)` | 打包消息 |
| `DeserializeMessageData(data)` | 解析消息 → sSocketData |
| `Encrypt(data)` | 加密 |
| `Decrypt(data)` | 解密 |

## 分流逻辑

收到数据后根据 `dataType` 分流：
- `CustomMsg` → `ByteStream` 反射反序列化
- `Protobuf` → `ProtoUtils.DeserializePbMsg` / `PBSerializer`
