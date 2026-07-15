# 原始网络层完整功能清单（SGF.Network）

> **用途**：可复用参考文档，记录原始网络层（`Assets/Scripts/StarFramework/Network/`）的全部功能特性。
> 后续重构/对比时直接查阅此文档，无需重复 review 原始代码。
> **生成日期**：2026-07-12
> **覆盖范围**：NetworkManager + Socket/* + KCP/ + RPCLite/ + StateSync/ + MDManager/ + 顶层管理器

---

## 1. NetworkManager（核心网络管理器）

**文件**：`Network/NetworkManager.cs`
**职责**：多 socket 管理、消息订阅/分发、消息队列与主线程派发、特殊消息路由。

### 1.1 多 Socket 支持
- 双 Socket 实例：`gameSocket`（"game"，系统/大厅连接）和 `battleSocket`（"battle"，战斗连接，当前已废弃但保留结构）
- 每个 SocketBase 独立管理连接、验证、心跳、重连

### 1.2 消息订阅/分发系统
- `OnMessageCmd(cmd, callback, target, isGroup)` — 按 cmd 注册
- `OnMessageEnum(MsgIDEnum, callback, target, isGroup)` — 按枚举注册
- `OffMessageCmd` / `OffMessageEnum` — 按 cmd+callback 取消
- `OffTargetMessage(target)` — 按 target 批量取消所有监听
- RPC 消息组监听：`isGroup=true` 时忽略 entityId，监听同一 messageCmd 的所有 RPC 消息
- 消息 Key 格式：普通 `cmd_false`；RPC `rpcCmd_messageCmd_entityId`
- target 映射表 `target2KeyMessage`：按 target 统一移除所有监听
- 委托链：同一 key 支持多 callback 通过 `+=` 追加

### 1.3 消息队列与主线程派发
- `messageHandleDataQueue` — 线程安全队列（`lock(mLock)`）
- `OnMyUpdate` → `ProcessMessageHandleQueue()` 消费队列
- `LockMessage(bool)` — 锁定后暂停消息派发（资源未准备好时）
- `IsMessageLock` 属性 — 外部可查询
- **异常恢复**：OnMyUpdate 中 catch 异常后自动 `LockMessage(false)` 解锁并清队列

### 1.4 收线程立即处理（PreHandle）
- `PreHandleMessageHandleData` — 特定消息（ServerTimeRet、HeartBeat）在收线程立即处理，不进主线程队列
- `OnPreStaticMessage(cmd, callback)` — 注册静态消息监听，不需取消，收线程立即执行
- `preStaticKey2MessageHandleDic` — 静态消息监听字典

### 1.5 消息反序列化
- 解密：`serializeType & 0x02` 判断是否加密 → `MsgEncode.DecryptData`
- 自定义消息：`compressFlag == 0` 且 cmd 在 CustomMsg 中 → ByteStream 反序列化
- PB 消息：`compressFlag == 0` 且非自定义 → `ProtoUtils.DeserializePbMsg`
- RPC 嵌套解析：解析出 RpcMsg 后二次解析内层真实消息（messageCmd、entityId）
- QASDK 上报：PB 消息解析成功后上报 QASDK（**接收侧**）

### 1.6 快照数据缓存与分帧处理
- `CacheMessageSnapData(messageHandleData)` — 消息存入 `DynamicDataFactory`
- `InvokeMessageSnapDataCache()` — Update 中按 `GetDealCountByAddSpeed<SerMessageSnapData>` 分帧处理
- `IsFullMessage` 标记 — AOI 消息完整标记（当前始终 true）
- 目的：避免单帧处理大量消息卡顿

### 1.7 特殊消息路由
- **FixMessage 差量更新**：cmd == `DBUpUserDatasReqID` → `FixMessageManager.Instance.HandleFixMessageCS`
- **ThirdMessage 第三方消息**：cmd == `Client2ThirdRetID` → `Client2ThirdMsgManager.Instance.HandleMessageCS`
- **RPC 消息组**：RPC 消息同时触发 groupKey 监听和 `GameManager.Instance.HandleRPCMsg`
- **RetMsg 通知**：所有消息处理后通知 `GameManager.Instance.OnRetMsg(messageCmd)`

### 1.8 Socket 关闭回调
- `OnSocketClose` 事件：`Action<bool>` — 外部注册 socket 关闭通知
- `OnGameSocketClose` — 内部转发方法
- `OnGameSocketWeak` — 弱网回调（当前空实现）

### 1.9 生命周期
- `Init` — 检查本机 IP、注册 Update 监听、注册 SocketClose 回调、注册 quitting
- `Close` — 关闭 gameSocket 和 battleSocket
- `Release` — 移除 Update 监听

---

## 2. SocketBase（Socket 中间层 — 当前使用版本）

**文件**：`Network/Socket/SocketBase.cs`（652 行）
**职责**：连接管理、状态机、UI 状态、验证、重连、发送、Ping 管理。

### 2.1 状态机
- `E_SocketState`：None / VerifyFailed / NormalConnect / TryReConnecting / ConnnectFail / ReConnnectFail
- `OnSocketStateChange` 事件：`Action<E_SocketState>`

### 2.2 UI 状态
- `SocketViewState`：None / Connecting / ReconnectBox / PingReconnectBox / RetrunLoginBox
- `OnSocketViewStateChange` / `SwithSocketViewState` — UI 状态切换

### 2.3 连接 API
- `Start(ip, port, pid, token, protocolType)` / `Start(url, pid, token, protocolType)`
- `ReStart()` — 重启连接
- `TestReconnect()` — 测试重连
- `End()` — 主动关闭，不重连

### 2.4 Ping 管理
- 内置 Ping 实例
- `ReStartPing()` / `ReSetPing()` / `TestPing()`

### 2.5 发送缓存队列
- `Queue<SendMsgData> sendMsgCache` — 线程安全 `lock(sendMsgCache)`
- `SendMsgData()` 只入队，不管连接状态
- SocketItem 发线程通过 `OnTryGetSendMsgData` 委托 dequeue 发送
- **断线时消息残留在队列**，重连验证后继续发送（隐式补发）

### 2.6 发送 API
- `SendRPCMsg(serverType, message, isEncrypt, oneOfCtrlEntityId, isAutoChangeMsgTarget, isSend2QASDK)` — RPC 消息
- `SendClient2CenterMsg(message)` — 中台消息（Client2CenterReq 包装）
- `SendEnumPbByteMsg(enumCmd, message, isEncrypt)` — 直接发送 byte[] PB 消息
- `SendCustomMsg(cmd, msg, isEncrypt, messageCmd)` — 自定义消息（ByteStream 序列化）

### 2.7 ServerType 自动转换
- `isAutoChangeMsgTarget && (Instance|Scene)` → `Space`

### 2.8 RPC 封装（关键）
- 构造 `RPCMsg` struct：`{ServerType:byte, SrcEntityID:ulong, MethodName:string, Data:byte[]}`
- `rpcMsg.Data = SocketUtils.RPCMsgSerializer(message.ToByteArray(), messageCmd)` = `[14][cmd:2][len:2][msg]`
- `rpcMsg.ServerType = (byte)serverType`
- `rpcMsg.MethodName = protoInfo.Value.Name`
- `rpcMsg.SrcEntityID = 0`（当前固定 0）
- `SendCustomMsg(CustomMsgID.RPCMsg=58, rpcMsg)` → **ByteStream 序列化整个 RPCMsg struct** 发送

### 2.9 QASDK 上报（发送侧）
- `isSend2QASDK` 为 true 时调用 `QASDKMsgUtils.SendClientRpcPb2QASDK`

### 2.10 重连机制
- `MAX_RECONNECT_TIMES = 3` — 单轮重连最大次数
- `MAX_TOTAL_RECONNECT_TIMES = 5` — 总重连最大次数（**定义但 SocketBase.cs 中未实际比较 — 原始 bug**）
- 重连间隔 0.5s
- **重连中间状态**：3 次失败 → `ConnnectFail` → `ReconnectBox`（重连弹窗，用户可手动重试）→ `OnReConnectFailed()` 重置 `_reconnectTimes=0` 允许再次重连
- 总失败 → `ReConnnectFail` → `RetrunLoginBox`

### 2.11 Token 重连
- `TryReconnectSocketWithToken()` — 通过 HTTP 重新获取 token 后重连
- `OnChoseHeroAckCallBack` — 检查 `GameLoginInfo.M_ChossHeroAck.Result > 1` 为失败（**原始 HTTP 调用已注释，此回调为死代码**）

### 2.12 验证流程
- 连接成功后自动发送 `ClientVerifyReq`（PID、Token、SessState）
- 验证成功 → `NormalConnect`
- 验证失败 → `VerifyFailed`（被踢下线，不重连）

### 2.13 调试 API
- `ReStart()` / `ReStartPing()` / `ReSetPing()` / `TestReconnect()` / `TestPing()` / `End()`

---

## 3. SocketItem（底层 Socket 项 — 当前使用版本）

**文件**：`Network/Socket/SocketItem.cs`（559 行）
**职责**：底层 TCP 连接、收发线程、验证、连接超时。

### 3.1 多线程模型
- **主线程**：创建 SocketItem、设置超时定时器、接收回调通知（Loom.QueueOnMainThread）
- **socket_thread**（`OnSocketMainThread`）：连接 + 发送循环（`TrySendMsgData`），单线程
- **socket_ReceiveThread**（`OnReceiveThread`）：纯接收循环（`TryReceiveMsgData`），阻塞式 Receive
- **收线程和发线程分离**

### 3.2 收线程循环
- 阻塞式 `curSocket.Receive(_tmpReceiveBuff)`，4096 字节固定接收缓冲区
- `DataBuffer.AddBuffer` 追加，`while (ContainData())` 循环提取完整消息
- `SocketUtils.GetMsgHandleData` 解析为 `MessageHandleData`
- `PreHandleMessage` — 拦截验证成功/失败消息设置 `verifyFalg` 标志
- `OnReceiveMessage?.Invoke()` 回调到 SocketBase（**收线程执行，非主线程**）
- Receive 返回 0 或异常 → `OnExceptionCloseConnect` 关闭连接

### 3.3 发线程循环
- `TrySendMsgData` — 从 `sendMsgCache` dequeue 发送
- 未验证时只发验证消息，验证后才从队列取业务消息

### 3.4 连接超时
- `DelayInvoker.DelayInvoke(3秒, OnConnectingOverTime)` — 3 秒超时
- 超时检查 `verifyFalg`：已验证成功则忽略；否则 `overTimeFlag=true` 并关闭 socket 触发重连
- 连接回调检查 `overTimeFlag`：已超时则不启动收线程

### 3.5 验证机制
- 连接后第一步发送 `ClientVerifyReq`（PID、Token）
- 收到 `ClientVerifySucceedRet` → `verifyFalg = true` → 开始发送业务消息
- 收到 `ClientVerifyFailedRet` → 被踢下线，不重连

### 3.6 对象池使用
- `SocketDataPools.Instance.FormateRetMsgData(msg)` — 格式化发送消息
- `SendMsgData` 继承 `SocketData : SimpleDataObject`，通过 `SimpleDataFactory.InstanceData<T>()` 获取，用完 `Return(msg)` 释放

---

## 4. Ping（Ping 健康 / 弱网检测）

**文件**：`Network/Socket/Ping.cs`（682 行）
**职责**：心跳发送、Ping 延迟采样、弱网状态机、动态心跳频率。

### 4.1 滚动样本窗口
- `LinkedList<int> pingsList` — 最多 20 样本（`PING_COUNT=20`）
- `GetCurAveragePing` — 取最新 5 个样本算平均（`PING_VALID_COUNT=5`）
- `sumPing` / `pingCount` — 实时累加

### 4.2 动态心跳频率
- `refreshPingInterval` — 动态调整：
  - 样本不足 5 或 ping 波动 > 50ms（`PING_WAVE_LIMIT`）→ 50ms 快速刷新（`PING_REFRESH_INTERVAL_NET_CHANG`）
  - 稳定后 → 1000ms（`PING_REFRESH_INTERVAL`）

### 4.3 dirtyFlag 心跳调度
- `dirtyFlag` 位标志：`0x1`=已发送, `0x2`=已收回复, `0x4`=定时器触发
- 只有 `dirtyFlag == 0` 或 `dirtyFlag == 0x7`（全部完成）才发下一条
- **等上一条回复后才发下一条**，防止心跳洪泛

### 4.4 AveragePing 全局
- `RefreshPing()` → `TimeUtils.AveragePing = AveragePing` — 全局静态更新
- `GetCurAveragePing()` 含在途心跳的实时估算（等待期间也计算当前 ping）

### 4.5 等待期间弱网计数累积
- `tempWaitPinMsgTime` — 当前等待 ping 的时间
- `tempWaitPinCount = tempWaitPinMsgTime / 920`（`PING_PONG_WEAK_NET_TIME`）— 等待时间换算为弱网次数
- `preContinueWeakCount + tempWaitPinCount` — 累积弱网计数
- 每帧检查（FixedUpdate `OnPingUpdate`）

### 4.6 恢复 Healthy 条件
- `PING_ENTER_NORMAL_COUNT = 3` — 连续 3 次正常 ping 才恢复 Healthy

### 4.7 弱网状态机
- `E_SocketPingState`：None / Weaking / TryReconnect / ReturnLogin
- `SwitchPingState` — 状态切换 + UI 状态映射
- 阈值：
  - `PING_PONG_WEAK_NET_TIME = 920` — 弱网定义
  - `PING_PONG_WAIT_TIME = 5000` — 超时 5s 弹断线重连
  - `PING_WAVE_MAX_COUNT = 1000` — 总弱网次数限制 → 返回登录
  - `PING_ENTER_CONNECTING_COUNT = 5` — 连续 5 次弱网 → 菊花
  - `PING_ENTER_DISCONNECT_COUNT = 12` — 连续 12 次弱网 → 断线重连

### 4.8 FixedUpdate 驱动
- `MonoHelper.AddFixedUpdateListener(OnPingUpdate)` — 固定帧驱动

### 4.9 TestPing 开关
- `TestPing` 布尔 — 测试时关闭心跳回复处理

### 4.10 公共 API
- `Start()` / `End()` / `Reset()` / `StartSendPing()`
- `ResetTotalWeakPingCount()` / `ResetWeakPingCount()`
- `OnHeartBeatMsgID(data, tag)` — 收到心跳回复
- `CurTotalWeakPingCount` / `PreContinueWeakCount` / `PreContinueNomalPingCount` / `CurWaitTime` / `CurWaitWeakPingTimes`

---

## 5. 包编解码 / 加密

**文件**：`Network/Socket/DataBuff.cs`、`Network/Socket/MsgEncode.cs`、`Network/Socket/SocketUtils.cs`

### 5.1 包头格式
- 6 字节包头：3 字节小端 payload+command 长度 + 1 字节 serializeType + 2 字节小端 command
- `DataBuffer` — 粘包处理

### 5.2 加密
- `serializeType & 0x02` 判断是否加密
- `MsgEncode.DecryptData` / `EncryptData` — rotate/xor 算法
- Key：`{ 253, 1, 56, 52, 62, 176, 42, 138 }`

### 5.3 RPC 序列化
- `SocketUtils.RPCMsgSerializer(data, cmd)` — `[14][cmd:2][len:2][data]`（14 = proto 类型标记）

### 5.4 消息解析
- `SocketUtils.GetMsgData(DataBuffer)` — 从 DataBuffer 取完整 MessageData

---

## 6. 对象池 / GC 优化

**文件**：`Network/Socket/SocketDataPools.cs`、`Network/Socket/ShareArrayPool.cs`

### 6.1 SocketDataPools
- `FormateRetMsgData(msg)` — 格式化发送消息
- `GetMsgData(...)` — 获取消息数据
- `SendMsgData` / `MessageHandleData` / `SocketData` 池化
- `SimpleDataFactory.InstanceData<T>()` — 获取池化对象
- `Return(msg)` — 释放回池

### 6.2 ShareArrayPool
- `byte[]` 共享数组池
- 减少 GC 压力

---

## 7. MsgRetManager（消息回复管理）

**文件**：`Network/MsgRetManager.cs`
**职责**：处理服务器返回的 `MsgRet` 消息（RetMsgID + RetCode），显示错误提示。

### 7.1 API
- `OnMessageEnum(callback, params MsgIDEnum[])` — 注册回调
- `OffMessageEnum(callback, params MsgIDEnum[])` — 取消回调
- `OnMsgRet(MessageHandleData)` — 处理 MsgRet 消息

### 7.2 行为
- 解析 `MsgRet.RetMsgID` + `RetCode`
- 触发注册的回调（`MsgRetArgs`）
- 通知 `GameManager.Instance.OnRetMsg(messageCmd)`
- `RetCode != 0` 且 `!IsDialog` → `Frame.Util.ShowMessageByMsgRet(msgRet)` 显示错误提示

---

## 8. FixMessageManager（差量消息 + MDManager）

**文件**：`Network/FixMessageManager.cs`、`Network/MDManager/`（22 个文件）
**职责**：协议数据差量更新，21 个业务模块数据管理器统一管理。

### 8.1 差量更新
- `HandleFixMessageCS(DBUpUserDatasReq)` — 处理差量更新消息
- `DBDataModel.IsPartial` — true=差量（MapModel 合并），false=全量（Add）
- 增改：`mgr.Add(keyName, dbDataModel)` / `mgr.GetMsg(keyName)` + `UpPackData` + `mgr.Update(keyName)`
- 删除：`mgr.Del(keyName)`
- 通知：`FixMessageNotifyData{TableName, AllDatas, Delets, Changes}` → 注册的回调
- 背包刷新：`IsUpdateItem` → `OnItemChangeToRefeshBag.Invoke`

### 8.2 API
- `OnMessage(key, callback)` / `OffMessage(key, callback)` — 按 key 注册/取消
- `ClearAllDatas()` — 清空所有模块数据
- `GetMDMgr(keyname)` — 获取指定模块管理器

### 8.3 21 个业务模块数据管理器
| key | 管理器 | 说明 |
|-----|--------|------|
| comcount | ComCountMDMgr | 通用次数 |
| items | ItemMDMgr | 道具 |
| eqrecasts | EqRecastMDMgr | 装备熔铸 |
| gacha | GachaMDMgr | 抽卡 |
| hero | HeroMDMgr | 主角信息 |
| partner | PartnerMDMgr | 伙伴数据 |
| bankplayer | BankPlayerMDMgr | 交易行玩家 |
| tweeter | TweeterMDMgr | 鸣器数据 |
| usersundry | UserSundryMDMgr | 用户设置 |
| b10register | B10registerMDMgr | PVP |
| personsecret | PersonSecretMDMgr | 个人秘境 |
| persontower | PersonTowerMDMgr | 个人爬塔 |
| guilddonate | GuildDonateMDMgr | 公会捐献 |
| userLifeSkills | LifeSkillUserDataMDMgr | 生活技能 |
| userSceneLogicData | UserSceneLogicDataMDMgr | 玩家场景数据 |
| checkin | CheckInMDMgr | 签到 |
| dailyactivity | DailyActivityMDMgr | 每日活跃 |
| persondaily | PDinfoMDMgr | 单人本 |
| sevendaygoal | SevenDayGoalMDMgr | 七日目标 |
| fight | FightMDMgr | 战力数据 |
| lobbyGamePlay | LobbyGamePlayMDMgr | 新手目标 |

### 8.4 MDMgrInterface 接口
- `Add(keyName, dbDataModel)` / `Del(keyName)` / `Update(keyName)` / `GetMsg(keyName)` / `ClearAllData()` / `Notify(data)`

---

## 9. Client2ThirdMsgManager（第三方消息）

**文件**：`Network/Client2ThirdMsgManager.cs`
**职责**：服务器中转第三方协议统一处理。

### 9.1 API
- `OnMessageEnum(MsgIDEnum, callback)` / `OffMessageEnum(MsgIDEnum, callback)`
- `HandleMessageCS(Client2ThirdRet)` — 处理第三方消息

### 9.2 行为
- 按 `MsgName` 查找 `ProtoDic.GetCMDByName` 获取 msgid
- `ProtoUtils.Deserialize(msgid, MsgData)` 反序列化
- 触发 `ThirdMessageDelegate(msgName, message)` 回调

---

## 10. KCP 传输（独立子系统）

**文件**：`Network/KCP/`（kcp.cs + KCPSocket.cs + switch_queue.cs）
**职责**：可靠 UDP 传输层，**仅 RPCLite 使用，主网络层不用**。

### 10.1 kcp.cs（996 行）
- KCP 协议 C# 完整移植
- ARQ、快速重传、拥塞控制、滑动窗口

### 10.2 KCPSocket.cs（534 行）
- UDP Socket + KCPProxy 管理
- 每个远端 IPEndPoint 对应一个 KCPProxy
- `SwitchQueue<byte[]>` 接收线程到主线程无锁切换
- `AddReceiveListener` / `SendTo` / `EnableBroadcast` / `Update` / `SelfEndPoint` / `SelfPort` / `SelfIP`

### 10.3 switch_queue.cs（77 行）
- 双队列线程安全 SwitchQueue

### 10.4 使用情况
- **主网络层（SocketItem/SocketBase/NetworkManager）不用 KCP**，始终 TCP
- KCP 仅被 `RPCLite/RPCService.cs` 使用（P2P 副本通信）
- RPCService.cs 有 TODO：`"Socket修改成KcpSocket（优先），或，替换成Socket；在做战场陪陪/副本模式的时候做"`

---

## 11. RPCLite（P2P RPC — 独立子系统）

**文件**：`Network/RPCLite/`（RPCService.cs + RPCMessage.cs + RPCMethodHelper.cs + Example/）
**职责**：C2C/S2S 的 P2P RPC 框架，用于战场陪练/副本模式。

### 11.1 RPCService
- 构造：`new KCPSocket(port, 1)` — KCP 传输
- `RPCTick()` — 主线程驱动 `m_Socket.Update()`
- 接收：`OnReceive` → `PBSerializer.NDeserialize<RPCMessage>` → `HandleRPCMessage`
- 反射调用：`MethodInfo.Invoke` 按方法名调用
- 绑定调用：`m_MapRPCBind[name]` → `RPCMethodHelper.Invoke`

### 11.2 RPC 调用方式
- `RPC(IPEndPoint, name, args)` — 1 对 1 调用
- `RPC(List<IPEndPoint>, name, args)` — 1 对多调用
- `RPC(beginPort, endPort, name, args)` — 端口广播
- `RPC(RPCService, name, args)` — 本地不通过网络（C2C/S2S）

### 11.3 绑定 API
- `Bind(name, rpc)` / `Bind<T0>(name, rpc)` / ... `Bind<T0-T9>(name, rpc)` — 泛型方法绑定

### 11.4 RPCMessage
- `methodName` + `args`（object[]）

---

## 12. StateSync（状态同步 — 独立子系统，未完成）

**文件**：`Network/StateSync/Client/StateSyncManager.cs` + `SSPLiteData.cs`
**职责**：帧锁定/非锁定同步、时间对齐、帧映射。

### 12.1 当前状态
- **大量代码被注释**，当前未完成
- `SSPManager.Start(param, playerId)` / `Stop()` / `EnterFrame()` — 基本框架
- 设计意图：时间对齐、帧驱动 Logic/View、追帧

---

## 13. 其他目录

### 13.1 Base/
- 基础类定义

### 13.2 Interface/
- 接口定义

### 13.3 Utils/
- 工具类

### 13.4 ProtocalData/
- 协议数据定义

### 13.5 ProtoPatial/
- proto 部分定义

### 13.6 proto/（53 个 .cs 文件）
- protobuf 消息定义（由 .proto 生成）

---

## 附录：关键常量速查

| 常量 | 值 | 位置 | 说明 |
|------|-----|------|------|
| MAX_RECONNECT_TIMES | 3 | SocketItem.cs:24 | 单轮重连最大次数 |
| MAX_TOTAL_RECONNECT_TIMES | 5 | SocketItem.cs:30 | 总重连最大次数（SocketBase.cs 未比较） |
| PING_COUNT | 20 | Ping.cs:23 | ping 样本最大数 |
| PING_VALID_COUNT | 5 | Ping.cs:28 | 计算 ping 用的有效样本数 |
| PING_REFRESH_INTERVAL | 1000 | Ping.cs:33 | 标准 ping 刷新间隔 |
| PING_REFRESH_INTERVAL_NET_CHANG | 50 | Ping.cs:37 | 网络波动时 ping 刷新间隔 |
| PING_WAVE_LIMIT | 50 | Ping.cs:42 | ping 波动改变刷新频率的阈值 |
| PING_WAVE_MAX_COUNT | 1000 | Ping.cs:47 | 总弱网次数限制 → 返回登录 |
| PING_ENTER_NORMAL_COUNT | 3 | Ping.cs:52 | 恢复正常的连续次数 |
| PING_ENTER_CONNECTING_COUNT | 5 | Ping.cs:57 | 进入弱网菊花状态的次数 |
| PING_ENTER_DISCONNECT_COUNT | 12 | Ping.cs:62 | 进入断线重连的次数 |
| PING_PONG_WEAK_NET_TIME | 920 | Ping.cs:68 | 弱网 ping-pong 基础值 |
| PING_PONG_WAIT_TIME | 5000 | Ping.cs:73 | 单次等待超时 → 重连 |
| 连接超时 | 3 秒 | SocketItem.cs | TCP 连接超时 |
| 重连间隔 | 0.5 秒 | SocketBase.cs | 重连延迟 |
| 接收缓冲区 | 4096 | SocketItem.cs | _tmpReceiveBuff |
| CustomMsgID.RPCMsg | 58 | CustomMsg.cs | 客户端 RPC 命令号 |
| CustomMsgID.HeartBeat | 4 | CustomMsg.cs | 心跳命令号 |
| CustomMsgID.ClientVerifyReq | 1 | CustomMsg.cs | 验证请求 |
| CustomMsgID.ClientVerifySucceedRet | 2 | CustomMsg.cs | 验证成功 |
| CustomMsgID.ClientVerifyFailedRet | 3 | CustomMsg.cs | 验证失败 |
| MsgIDEnum.RpcMsgID | 1001 | proto/ | 服务器推 RPC 命令号 |
| ServerTimeRetID | 4843 | proto/ | 服务器时间返回 |
| 加密 Key | {253,1,56,52,62,176,42,138} | MsgEncode.cs | rotate/xor 密钥 |
