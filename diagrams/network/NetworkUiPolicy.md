# Network UI Policy

## 目标

在保留策划需要的弱网 UI 表现前提下，将以下 3 件事拆开：

1. 真实连接状态
2. Ping/弱网质量状态
3. 最终 UI 表现

多 Socket 时，再额外增加一层全局 UI 仲裁，避免后台 Socket 抢前台界面的网络弹窗。

---

## 单 Socket 分层

### 1. RealConnectionState

只表达真实网络事实，不直接表达 UI。

建议状态：

- `Disconnected`
- `Connecting`
- `Verifying`
- `Connected`
- `Reconnecting`
- `ReconnectFailed`
- `Kicked`
- `Closed`

典型触发：

- `Connect` 开始 -> `Connecting`
- 收到验证成功 -> `Connected`
- `Send/Receive/Connect` 异常 -> `Reconnecting` 或 `ReconnectFailed`
- 服务器踢下线/验证失败 -> `Kicked`
- 主动关闭 -> `Closed`

### 2. PingHealthState

只表达链路质量，不直接关 Socket，不直接弹 UI。

建议状态：

- `Healthy`
- `Weak`
- `CriticalWeak`
- `HeartbeatLost`

典型含义：

- `Healthy`：延迟和抖动正常
- `Weak`：已有明显弱网，需要弱提示
- `CriticalWeak`：玩家体验已经接近断线，需要假断线提示
- `HeartbeatLost`：长时间未收到 Pong，允许升级为真实断线处理

### 3. ConnectionUiIntent

这是单个 Socket 根据真实状态 + Ping 状态产出的 UI 意图，不代表最终一定显示。

建议状态：

- `None`
- `WeakLoading`
- `FakeReconnectDialog`
- `RealReconnectDialog`
- `ReturnLoginDialog`

说明：

- `FakeReconnectDialog`：策划要的“弱网伪装断线”
- `RealReconnectDialog`：真实连接异常后的重连弹窗
- `ReturnLoginDialog`：连接已不可恢复，或者被踢下线

---

## 单 Socket 推荐规则

推荐默认规则：

- `Connected + Healthy` -> `None`
- `Connected + Weak` -> `WeakLoading`
- `Connected + CriticalWeak` -> `FakeReconnectDialog`
- `Connected + HeartbeatLost` -> 先维持 `FakeReconnectDialog`
- `Connected + HeartbeatLost` 持续超过二级阈值 -> 升级为真实断线
- `Reconnecting` -> `RealReconnectDialog`
- `ReconnectFailed` -> `ReturnLoginDialog`
- `Kicked` -> `ReturnLoginDialog`

### 为什么不直接用 Ping 触发重连

当前项目存在明确产品需求：弱网时先让玩家感知“像断线”，但不一定立刻重建 Socket。

因此推荐：

- Ping 先决定体验层表现
- 真实断线仍由 Socket 异常主导
- Ping 长时间失联时，再升级成真实断线

这样既保留策划表现，也不会把短时抖动直接打成断线。

---

## 多 Socket 设计

多 Socket 时，每个 Socket 都有自己的：

- `RealConnectionState`
- `PingHealthState`
- `ConnectionUiIntent`

但不是每个 Socket 的 UI 意图都应该直接显示到前台。

所以增加：

## GlobalUiArbiter

职责：

- 收集所有 Socket 的 `ConnectionUiIntent`
- 根据当前界面上下文，决定哪个 Socket 可以影响前台 UI
- 对不该打断当前界面的 Socket，只记录状态，不立刻弹窗

---

## 界面上下文

建议增加一个轻量上下文：

- `Login`
- `Lobby`
- `Battle`
- `Loading`

上下文只回答一个问题：

当前前台界面，允许哪个 Socket 的网络状态影响 UI。

---

## Socket 可见性策略

每个 Socket 增加一个 UI 可见性策略：

- `AlwaysVisible`
  - 可以参与全局 UI 评估
- `VisibleWhenFocused`
  - 只有当前相关界面激活时才允许显示
- `Silent`
  - 永不主动弹全局 UI，只保留状态给业务层查询

### 典型建议

- `gameSocket`
  - `AlwaysVisible` 或 `VisibleWhenFocused`
- `battleSocket`
  - `VisibleWhenFocused`
- 下载/聊天/埋点类 Socket
  - `Silent`

## Socket 恢复认证策略

多 Socket 下，不是所有连接都应该使用同一种重连认证流程。

建议每个 Socket 增加一个恢复认证策略：

- `ReuseCurrentCredential`
  - 直接复用当前内存中的认证信息重连
- `RefreshSessionBeforeReconnect`
  - 重连前先刷新会话，再使用新的认证信息建链
- `ManualOnly`
  - 不自动恢复，只上报状态，等待外层业务决定

典型建议：

- `gameSocket`
  - `RefreshSessionBeforeReconnect`
- `battleSocket`
  - 由服务端协议决定
  - 如果依赖主连接恢复结果，则不单独刷新
- 后台辅助连接
  - `ReuseCurrentCredential` 或 `ManualOnly`

---

## 多 Socket 仲裁规则

推荐先用最小规则，不做复杂优先级系统。

### 规则 1

当前界面有一个 `FocusedSocketGroup`。

例如：

- `Lobby` -> `gameSocket`
- `Battle` -> `battleSocket`

### 规则 2

只有 `FocusedSocketGroup` 对应的 Socket，允许直接把 `ConnectionUiIntent` 显示到前台。

### 规则 3

非当前前台的 Socket：

- 可以继续维护自己的真实连接状态
- 可以继续维护自己的 Ping 状态
- 可以继续产出自己的 UI 意图
- 但默认不允许抢前台弹窗

### 规则 4

如果某个后台 Socket 出现异常：

- 先缓存“待处理 UI 意图”
- 等切回它所属界面时再重新评估

---

## 需求例子：战斗中主界面 Socket 断开

场景：

- 当前在战斗界面
- `battleSocket` 正常
- `gameSocket` 断开

期望行为：

- 战斗界面不被 `gameSocket` 的断线弹窗打断
- 战斗继续使用 `battleSocket` 的状态显示
- 战斗结束切回主界面后，再处理 `gameSocket` 的断线 UI

推荐处理：

1. `gameSocket` 更新自己的 `RealConnectionState`
2. `gameSocket` 产出 `RealReconnectDialog` 或 `ReturnLoginDialog`
3. `GlobalUiArbiter` 判断当前上下文为 `Battle`
4. `gameSocket` 不是当前前台允许打断的 Socket
5. 本次不弹窗，只记录为“待处理网络 UI”
6. 退出战斗切回 `Lobby`
7. `GlobalUiArbiter` 重新评估 `gameSocket`
8. 若它仍异常，再显示对应弹窗

---

## 对当前项目的落地建议

### Ping.cs

只负责：

- 发心跳
- 收心跳
- 计算弱网状态
- 输出 `PingHealthState`

不再直接操作 UI 状态。

### SocketBase.cs

只负责：

- 真实连接生命周期
- 重连流程
- 输出 `RealConnectionState`

不再直接决定“假断线 / 真断线 / 返回登录”这种产品表现。
不再直接发 HTTP，也不再直接承担“刷新 token / 刷新登录会话”的职责。

### 连接恢复模型

当前项目原始设计中：

- 初次进入世界时，先通过选角接口获得 `LobbyAddr + Token`
- 客户端再使用这些数据建立 Socket，并自动发送验证消息
- 断线重连时，原始设计要求重新拿一次新的 `Token`

这套“重新拿 token”的逻辑不应继续写在 `SocketBase` 内部。

推荐拆成 3 个角色：

1. `NetworkConnection`
   - 只负责 Socket 建链、验证、收发、心跳、真实断线检测
   - 不负责发 HTTP，不负责刷新 token

2. `SessionRecoveryService`
   - 只负责调用恢复接口
   - 例如重新请求 `ChooseHeroHandler`
   - 返回新的会话恢复数据

3. `ConnectionRecoveryCoordinator`
   - 负责串联“连接断开 -> 刷新会话 -> 重新建链”
   - 根据连接的恢复认证策略决定是否先刷新 token

### 推荐恢复数据结构

```csharp
class ReconnectSessionData
{
    public string SocketAddress;
    public ulong Pid;
    public string Token;
    public bool IsReconnectSession;
}
```

说明：

- `SocketAddress`：当前连接目标地址，例如 `LobbyAddr`
- `Pid`：用于生成验证消息
- `Token`：本次重连使用的新 token
- `IsReconnectSession`：表明这是恢复会话，不是首次登录

### 推荐重连流程

#### 初次登录

1. 登录/选角模块调用登录后端
2. 获得 `LobbyAddr + Token`
3. 组装 `ReconnectSessionData`
4. 交给 `NetworkConnection.Start(sessionData)`
5. `NetworkConnection` 内部自动构建并发送验证消息

#### 断线重连

1. `NetworkConnection` 检测到真实断线
2. 上报“需要恢复连接”
3. `ConnectionRecoveryCoordinator` 读取当前连接的恢复认证策略
4. 如果策略为 `RefreshSessionBeforeReconnect`
5. 调用 `SessionRecoveryService`
6. 获得新的 `ReconnectSessionData`
7. 再驱动 `NetworkConnection.ReconnectWith(sessionData)`
8. 连接内部自动发送新的验证消息

### 为什么不放回 SocketBase

如果把“刷新 token / 重新调登录恢复接口”继续写在 `SocketBase` 内部，会导致：

- 传输层直接依赖 HTTP / 登录恢复逻辑
- 单 Socket 时代残留的临时逻辑继续扩散到多 Socket
- 无法给不同 Socket 设定不同恢复认证策略
- 收线程、主线程、HTTP 回调继续相互缠绕

因此在重构中应明确：

- `Socket` 管连接
- `RecoveryService` 管恢复凭据
- `Coordinator` 管重连编排

### 当前项目的补回方向

当前代码中，断线后“重新请求 `ChooseHeroHandler` 获取新 token”的逻辑原本存在设计意图，但实现被注释掉。

重构时应补回，但补回到：

- `SessionRecoveryService`
- `ConnectionRecoveryCoordinator`

而不是重新塞回 `SocketBase.TryReconnectSocketWithToken()`

### 新增策略层

增加两个轻量对象：

1. `ConnectionUiPolicy`
   - 输入：单个 Socket 的 `RealConnectionState + PingHealthState`
   - 输出：单个 Socket 的 `ConnectionUiIntent`

2. `GlobalUiArbiter`
   - 输入：所有 Socket 的 `ConnectionUiIntent + 当前界面上下文`
   - 输出：当前前台真正显示的 UI

### StarWorldSocketMsg.cs

只做前台表现适配：

- 显示菊花
- 显示假断线弹窗
- 显示真重连弹窗
- 显示返回登录弹窗

不要再把“状态判定”写在 UI 层。

---

## 控制协议线程模型

对连接级控制协议，推荐采用：

- 收线程内解析
- 只更新线程安全基础状态
- 不直接驱动业务对象和 Unity 表现

适合放在收线程里的协议：

- 验证成功 / 验证失败
- 心跳 Ping / Pong
- 时间同步
- 连接保活 / 纯控制 Ack

不适合放在收线程里的协议：

- 会修改 GameManager / Module / Entity 的业务消息
- 会调用 Unity API 的逻辑
- 需要依赖当前场景、页面、业务上下文判断的协议

### 时间同步采用方案 B

时间同步默认采用：

- 子线程解析 `ServerTimeRet`
- 生成新的时间同步快照
- 原子替换当前快照引用
- 主线程与业务层统一只读快照

推荐快照结构：

```csharp
class TimeSyncSnapshot
{
    public long ServerTimeStamp;
    public long ClientReceiveTimeStamp;
    public long Offset;
}
```

推荐流程：

1. 收线程收到时间同步消息
2. 读取当前客户端接收时间
3. 计算新的 `Offset`
4. 构造新的 `TimeSyncSnapshot`
5. 一次性替换当前快照引用
6. 业务层后续只读取最新快照

### 为什么采用方案 B

目的不是让子线程直接参与业务，而是避免两类问题：

1. 主线程卡顿导致时间同步处理延后
2. 多个时间字段分别写入时，被其它线程读到“半新半旧”的组合

方案 B 的边界更清楚：

- 子线程只负责生成快照
- 共享层只暴露一个原子替换的引用
- 业务层永远读完整快照，不读散落字段

### 约束

时间同步快照更新后：

- 不直接弹 UI
- 不直接调用 Unity API
- 不直接触发复杂业务逻辑

它只负责更新“本地时钟基准”。

如果业务需要感知“刚完成一次时间同步”，应通过主线程安全事件或拉模式读取快照，而不是在收线程里直接驱动后续逻辑。

---

## 推荐结论

当前项目推荐采用：

- 单 Socket：`真实状态 / Ping状态 / UI意图` 三层分离
- 多 Socket：再增加 `全局 UI 仲裁层`

这样可以同时满足：

- 策划需要的弱网 UI 表现
- 真断线与假断线分离
- 多 Socket 独立维护状态
- 非前台 Socket 不打断当前界面

这是后续继续做多 Socket 网络层的默认前提。
