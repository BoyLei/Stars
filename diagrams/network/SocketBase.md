# SocketBase

**命名空间：** `SGF.Network`
**文件：** `Assets/Scripts/StarFramework/Network/SocketBase.cs`

## 职责

Socket 连接封装，管理连接状态、发送队列、重连机制和数据处理。

## 核心字段

- `m_SocketItem` (SocketItem) — 底层 TCP Socket
- `m_SendQueue` — 发送消息队列
- `m_DataBuffer` (DataBuffer) — 接收数据缓冲
- `m_SocketState` (SocketState) — 连接状态枚举
- `reconnectCount` — 当前重连次数 (MAX_RECONNECT=3)
- `totalReconnectCount` — 总重连次数 (MAX_TOTAL_RECONNECT=5)
- `CONNECT_TIMEOUT` — 连接超时 (3秒)

## 核心方法

| 方法 | 说明 |
|------|------|
| `Connect(ip, port)` | 建立连接 |
| `Disconnect()` | 断开连接 |
| `Send(data)` | 发送数据(入sendQueue) |
| `OnDataReceived(data)` | 接收数据回调 → DataBuffer |
| `CheckHeartBeat()` | 心跳检测 (CustomMsgID.HeartBeat=4) |
| `Reconnect()` | 自动重连逻辑 |

## 生命期

```
Connect → 连接中 → 已连接 → [心跳保活]
  ↕ 断开/超时 ↕
重连(最多3次/总计5次) → 成功恢复 / 失败断开
```

## 关键设计

- 两级重连限制：单次最多重试3次，整个生命周期最多5次
- 3秒连接超时
- 心跳包定期检测，超时自动触发重连
