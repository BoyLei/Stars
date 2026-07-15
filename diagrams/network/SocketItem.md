# SocketItem

**文件：** `Assets/Scripts/StarFramework/Network/SocketItem.cs`

## 职责

原生 TCP Socket 封装，提供异步连接与收发。

## 核心字段

- `tcpSocket` — 原生 `System.Net.Sockets.Socket`
- `connectThread` — 连接线程
- `receiveThread` — 接收线程

## 核心方法

| 方法 | 说明 |
|------|------|
| `ConnectAsync(ip, port)` | 异步连接 |
| `SendAsync(data)` | 异步发送 |
| `ReceiveLoop()` | 线程循环接收数据 |
| `Close()` | 关闭 Socket |
