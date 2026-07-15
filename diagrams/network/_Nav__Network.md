# 网络模块导航

> Level 2 — 网络层子模块划分。修改网络代码前先读本文件，再深入 Level 3。

## 子模块列表

| 编号 | 子模块 | Level 3 详情 | 职责 |
|------|--------|-------------|------|
| A1 | NetworkManager | `NetworkManager.md` | 入口，双Socket管理，消息订阅，主线程分帧 |
| A2 | SocketBase | `SocketBase.md` | Socket封装，连接/重连/心跳/发送队列 |
| A3 | SocketItem | `SocketItem.md` | 原生TCP Socket，connect/receive线程 |
| A4 | DataBuffer | `DataBuffer.md` | TCP流式数据→完整消息包组装 |
| A5 | SocketUtils | `SocketUtils.md` | 格式化/解析/加密/解密，双协议分流 |
| A6 | ProtoUtils/PBSerializer | `ProtoUtils.md` | Protobuf 消息序列化/反序列化 |
| A7 | ByteStream | `ByteStream.md` | 二进制流读写，自定义消息序列化 |
| A8 | MsgEncode | `MsgEncode.md` | 消息加密/解密 |
| A9 | FixMessageManager | `FixMessageManager.md` | 消息分发，管理MDMgr |
| A10 | MsgRetManager | `MsgRetManager.md` | 消息回调注册与处理 |
| A11 | CustomMsg | `CustomMsg.md` | 协议号↔类型映射 |
| A12 | ProtoDic | `ProtoDic.md` | Protobuf协议字典 |
| A13 | SocketDataPools | `SocketDataPools.md` | SendMsgData对象池 |
| A14 | NetworkUiPolicy | `NetworkUiPolicy.md` | 单Socket Ping/UI分层 + 多Socket UI仲裁 |
| A15 | NetworkRefactorV2Adapter | `NetworkRefactorV2Adapter.md` | V2 主 TCP 适配层、兼容层、架构图 |

## 数据流

```
发送: 业务层 → NetworkManager → SocketUtils格式化 → SocketBase → SocketItem → TCP
接收: TCP → SocketItem → DataBuffer → SocketUtils解析
  → [CustomMsg/Protobuf分流] → NetworkManager.Update → 业务模块
```
