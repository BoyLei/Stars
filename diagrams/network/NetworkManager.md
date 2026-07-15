# NetworkManager

**命名空间：** `SGF.Network`
**继承：** `ServiceModule<NetworkManager>` → `Module`
**文件：** `Assets/Scripts/StarFramework/Network/NetworkManager.cs`

## 职责

网络层入口，管理两个Socket连接，消息订阅/反序列化/主线程分帧派发。

## 核心字段

- `gameSocket` (SocketBase) — 系统消息连接
- `battleSocket` (SocketBase) — 战斗消息连接 (已废弃)
- `messageQueue` — 待处理消息队列，每帧从 Update 中取出分发

## 核心方法

| 方法 | 说明 |
|------|------|
| `Init()` | 初始化，检查本机IP，注册Update回调 |
| `Connect(ip, port, isBattle)` | 连接服务器 |
| `SendMessage(cmd, data, callback?)` | 发送消息，支持回调 |
| `OnMessageCmd(cmd, handler)` | 注册消息监听(按cmd) |
| `OnMessageEnum(enumType, handler)` | 注册消息监听(按枚举) |
| `OffTargetMessage(cmd)` | 取消消息监听 |
| `Update()` | 每帧从messageQueue取出消息，分发给FixMessageManager/MsgRetManager |
| `LockMessage(flag)` | 消息锁定，锁定期间缓存消息 |

## 数据流

```
发送: SendMessage → SocketUtils.FormateData → SocketBase.Send → SocketItem → TCP
接收: SocketItem → SocketBase.OnDataReceived → DataBuffer → SocketUtils.Deserialize
  → 入messageQueue → Update → FixMessageManager.ProcessMessage / MsgRetManager.ProcessRet
```

## 关键设计

- 双Socket分离业务消息和战斗消息
- 消息分帧处理，避免阻塞主线程
- 支持消息回调机制(SendMsgData.callback)
- 消息锁定用于场景切换等关键时刻
