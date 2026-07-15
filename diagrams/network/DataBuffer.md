# DataBuffer

**命名空间：** `SGF.Network`
**文件：** `Assets/Scripts/StarFramework/Network/DataBuffer.cs`

## 职责

TCP 流式数据的帧缓冲层，将不定长度的 TCP 数据流组装为完整消息包。

## 核心方法

| 方法 | 说明 |
|------|------|
| `AddBuffer(data, offset, length)` | 追加接收到的数据到缓冲区 |
| `ContainData()` | 检查缓冲区是否包含一个完整的数据包 |
| `GetData()` | 获取一个完整的数据包(sSocketData) |

## sSocketData 结构

| 字段 | 说明 |
|------|------|
| `dataLen` | 数据包长度 |
| `dataType` | 消息类型(CustomMsg/Protobuf) |
| `cmd` | 命令码 |
| `body` | 消息体 |

## 数据流

```
TCP接收 → SocketItem.ReceiveLoop → SocketBase.OnDataReceived
→ DataBuffer.AddBuffer (追加到缓冲区)
→ DataBuffer.ContainData? (检查完整性)
  → 不完整: 等待更多数据
  → 完整: DataBuffer.GetData → sSocketData → SocketUtils.Deserialize
```

## 关键设计

- 解决TCP粘包/半包问题
- dataType字段用于双协议分流(CustomMsg 或 Protobuf)
- 缓冲区自动扩展，支持大数据包
