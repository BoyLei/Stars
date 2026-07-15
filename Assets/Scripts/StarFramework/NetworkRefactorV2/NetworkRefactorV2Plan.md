# NetworkRefactorV2 Plan

## Current priority

1. Match the original `SGF.Network` behavior first.
2. Keep the implementation isolated under `NetworkRefactorV2`.
3. Treat multi-socket support as a capability of the new runtime, not as an integration step yet.
4. Do not modify `AppMain`, Lua, UI, or the old network layer until V2 behavior is proven.

## Original network behavior now covered

- 6-byte packet header: 3-byte little-endian payload-plus-command length, 1-byte serialize type, 2-byte little-endian command.
- Legacy encryption key and rotate/xor algorithm.
- RPC envelope `[14][cmd:2][len:2][payload]`.
- Initial TCP connect failure is surfaced to the caller and marks the connection as `ReconnectFailed` while closing the transport.
- `Start(...)` honors an already-canceled caller before mutating connection state, so a canceled initial start cannot move a disconnected connection to `Connecting` or connect the transport.
- Verify packet is sent immediately after TCP connect when a verify packet factory is provided.
- Verify result packets are decoded by injected `VerifyResultDecoder`; success moves to `Connected`, failure moves to `Kicked`, and verify result packets are not dispatched as normal business messages.
- Verify failure cancels the connection lifetime and closes the underlying transport while preserving `Kicked` as the externally visible state.
- Verify packet send failure uses the same `Kicked` + transport-close path and rethrows the send exception to the caller.
- Active `Close()` during verify-packet send is treated as cancellation and keeps `Closed`; it must not emit a transient `Kicked` state event that could drive return-login UI.
- Business sends before verify succeeds are cached, not sent to the transport, and flushed in order after `MarkVerified()`.
- Verify failure maps to `Kicked` and returns login UI intent.
- Receive failure before verify maps to `Kicked`, not reconnect.
- Verify timeout is separate from TCP connect timeout and maps to the same `Kicked` + transport-close path as verify failure.
- Heartbeat packets are sent by injected factory and interval gate.
- Heartbeat send failures are not swallowed; failed sends do not advance the heartbeat interval and remain retryable.
- Heartbeat pong packets are decoded by injected `HeartbeatPongDecoder`, update the connection-owned `PingHealthMonitor`, and are swallowed before normal business dispatch.
- Heartbeat pong packets received through the receive loop refresh the last-pong timestamp used by heartbeat-lost checks.
- Connections record the last pong timestamp through `RecordHeartbeatPong(latency, now)` and expose `CheckHeartbeatAge(now)` for Tick-driven heartbeat-lost detection.
- `MarkVerified()` is idempotent for an already connected socket and resets reconnect attempts, heartbeat send/receive baselines, and ping health for the newly verified session.
- Outbound packets are serialized through a per-connection send gate so concurrent business/heartbeat/verify sends cannot interleave on the transport.
- Weak ping thresholds match the original values, including total weak limit `1000` -> return login.
- Reconnect cancels the old receive loop, applies attempt limit, and delays by default `0.5s`.
- `ReconnectWith(...)` honors an already-canceled caller before mutating connection state, so a canceled reconnect request cannot close a still-connected socket.
- Reconnect TCP connect failure is surfaced to the caller and marks the connection as `ReconnectFailed` while closing the transport.
- Recovery policies are covered: `ReuseCurrentCredential` returns the current session without calling the refresh service, `RefreshSessionBeforeReconnect` deduplicates concurrent refreshes, honors already-canceled callers, releases failed pending tasks, and `ManualOnly` fails without calling the refresh service.
- Recovery deduplication is per socket; different socket names recover independently.
- `NetworkHub.RecoverAndReconnect(...)` deduplicates concurrent recovery/reconnect requests per socket, so one socket cannot refresh/reconnect multiple times in parallel, while already-canceled callers still receive a canceled task instead of being attached to another caller's running recovery.
- Recovery refresh failure is surfaced to the caller and marks the connection as `ReconnectFailed` while closing the transport.
- `Kicked` connections reject recovery/reconnect and keep the externally visible `Kicked` state for return-login handling.
- Receive-loop events carry a connection generation guard so packets posted by an old loop before reconnect are dropped instead of being dispatched after the new connection starts.
- `ReconnectFailed` increments the same receive generation, so packets already posted by the old receive loop cannot dispatch after recovery failure or reconnect-attempt exhaustion.
- Active `Close()` increments the same receive generation, so already-posted receive-loop events cannot dispatch packets or mutate state after the connection is closed.
- Verify timeout events also carry the connection generation guard, so a timeout posted by an old verifying session cannot kick a newer reconnect generation.
- Receive-loop data returned after cancellation is checked against the connection generation before parsing, so stale heartbeat pong / immediate packets cannot mutate the new connection.
- Reconnect resets the per-connection `PacketReader`, so a corrupt packet on the old socket cannot poison the next receive loop.
- RPC subscriptions support entity-specific dispatch and group dispatch.
- Immediate command hook supports original-style receive-thread handling for time sync without referencing old concrete handlers; heartbeat pong stays on the main-thread path and updates both adapter and connection ping health.
- `TimeSyncClock` replaces the whole immutable snapshot atomically, preserving consistent server/client/offset fields.
- Immediate handlers are isolated from normal subscriptions: receive-loop immediate dispatch calls `SubscribeImmediate(...)` handlers only; normal `Subscribe(...)` callbacks still require the main-thread `Tick()` path and are not invoked for immediate packets.
- `NetworkHub.LockMessages(bool)` matches the original `NetworkManager.LockMessage(bool)` queue gate: normal messages are held while locked and flushed in order after unlock on `Tick()`.
- `NetworkHub.Tick()` flushes unlocked pending messages in a `finally` block, so one callback exception cannot leave previously locked messages stuck forever.
- `NetworkHub.EnqueueReceived(...)` dispatches messages enqueued while `Tick()` is already draining in the same Tick, avoiding an extra-frame delay for nested dispatch.
- `GlobalUiArbiter` prefers the currently focused socket over `AlwaysVisible` sockets and keeps background socket intents cached for context re-evaluation.
- `CSharpNetworkApi.SendCustom/SendProtobuf/SendRpc` route through the hub and preserve the legacy RPC envelope.
- `CSharpNetworkApi` subscription tokens and owner-based unsubscribe remove normal and RPC callbacks from dispatch.
- `LuaNetworkApi` numeric handles and owner-based unsubscribe both remove callbacks from dispatch, clear owned handle entries, and `SendRpc` preserves the legacy RPC envelope.
- PacketReader uses a byte buffer with cursors instead of `List<byte>.RemoveRange`, reads sticky multiple packets from one buffer, treats `MaxPacketSize` as a per-packet limit rather than a receive-chunk limit, and marks oversize / too-short declared lengths corrupt without throwing.

## Verification status

- V2 EditMode test assembly compile: passed with `EDITMODE_COMPILE_EXIT=0`.
- Standalone self-check: passed with `NetworkRefactorV2 self-check: 85 passed, 0 failed` after the `OriginalNetworkFeatures` repair pass.
- Unity Editor batch command did not finish inside the 30-second automation window after this repair pass. The log no longer showed V2/Adapter `error CS` lines in the captured tail, but Unity emitted licensing token errors and the EditMode test result was not produced. Do not claim EditMode passed until the command below completes with a test summary/XML.
- Encoding: V2 `.cs` and `.asmdef` files are UTF-8 without BOM and CRLF.
- Boundary check: V2 core does not directly reference `SGF.Network`, `SocketItem`, `SocketUtils`, `MsgEncode`, `NetworkManager`, `GameManager`, or UI classes; `NetworkRefactorV2Adapter` may reference legacy protocol enums/types as the compatibility seam.

## Unity test blocker

Unity Editor batch test was previously blocked because another Unity instance had the same project open:

```text
Multiple Unity instances cannot open the same project.
Project: H:/Star/Stars_Project/StarsProject_Client/trunk/Stars
```

After closing the open Unity Editor instance, the batch command starts and exits with code `0`, but no XML result file is produced. Re-run with this command when investigating Unity Test Runner output:

```powershell
& 'H:/DevelopFold/Unity Editor/2022.3.42f1c1/Editor/Unity.exe' -batchmode -nographics -quit -projectPath 'H:/Star/Stars_Project/StarsProject_Client/trunk/Stars' -runTests -testPlatform EditMode -assemblyNames SGF.NetworkRefactorV2.Tests -logFile 'H:/Star/Stars_Project/StarsProject_Client/trunk/Stars/Temp/NetworkRefactorV2-EditMode.log'
```

## Adapter layer draft (NetworkRefactorV2Adapter)

Status: experimental draft, not accepted as part of the current V2 completion gate.

Reason:

- The current objective is to make V2 match the original network-layer behavior first.
- Adapter code references game/old-layer concrete types (`SGF.Network`, `ProtoMsg`, `StarProjectDef`, `HttpUtils`, `GameLoginInfo`), so it belongs to a later integration phase.
- Adapter EditMode tests are currently kept as `.disabled` files because a standalone test asmdef cannot reliably reference Adapter sources that live in Assembly-CSharp. Re-enable only after the adapter gets a proper assembly boundary or a different Unity test setup.

集成适配层位于 `Assets/Scripts/StarFramework/NetworkRefactorV2Adapter/`，**无 asmdef，属于 Assembly-CSharp**，可同时引用 V2 的 `SGF.NetworkRefactorV2` 程序集与旧网络层类型（`SGF.Network` / `ProtoMsg` / `StarProjectDef`），但不修改旧网络层与 `AppMain`（按计划约定）。

| 文件 | 职责 |
|------|------|
| `V2GameCommandIds.cs` | 命令号映射：客户端自定义消息 `CustomMsgID`（1/2/3/4/58）与服务器 protobuf 消息 `MsgIDEnum`（RpcMsgID=1001 / ServerTimeRetID=4843）。发送/接收 RPC 命令号不同。 |
| `GameVerifyPacketFactory.cs` | 由 `ReconnectSessionData` 构造 `ClientVerifyReq`（`Source=0`, `PID`, `Token`, `SessState` 1=新/2=重连），`ByteStream` 序列化，与旧 `SocketItem` 一致。 |
| `GameHeartbeatPacketFactory.cs` | 构造 `HeartBeat{ Delay=0 }` 并 `ByteStream` 序列化。 |
| `GameRpcDecoder.cs` | RPC 接收端用 `ProtoMsg.RpcMsg.Parser` 解析 protobuf 封装，提取真实 `SrcEntityID` 作为 `EntityId`、内层 `MsgData` 作为 payload（`enityId` 默认 0、RPC 时由协议内 `SrcEntityID` 决定，**非 bug**）。 |
| `GameSessionRecoveryService.cs` | 实现 `ISessionRecoveryService`：重连前经 `HttpUtils.SendRetryLoginHeaderHttpReq` + `CHOOSE_HERO_HANDLER` 重新登录刷新 Token（对应 `RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect`）。 |
| `GameUiIntentBridge.cs` | 静态映射 `ConnectionUiIntent` → 旧 `SocketViewState`（None/Connecting/PingReconnectBox/ReconnectBox/RetrunLoginBox）。 |
| `GameNetworkAdapter.cs` | 主编排器：注册 game socket、注入工厂/RpcDecoder/RecoveryPolicy、订阅验证(2/3)与即时心跳(4)/时间(4843)、每帧 `Tick` 驱动心跳+Ping+事件泵+UI 仲裁，暴露 `SocketViewStateChanged`。 |
| `GameAdapterSelfCheck.cs` | 纯逻辑自检（命令号/RPC解码/验证包/心跳包/UI映射），编辑器菜单 `Network/V2 Adapter SelfCheck` 手动运行。 |

**RPC 命令号差异（关键）**：客户端发 RPC 用 `CustomMsgID.RPCMsg=58` + `[14]` 信封（V2 `RpcCodec.Encode`）；服务器推 RPC 用 `MsgIDEnum.RpcMsgID=1001` + protobuf `RpcMsg` 封装（V2 `GameRpcDecoder` 解析）。两者不可混用。

## Unit tests（2026-07-10 新增）

目标：覆盖 V2 的 4 个内部 bug 修复，并为 `GameNetworkAdapter` / `GameSessionRecoveryService` 提供可测缝 + Fake 集成测试（用户确认范围：「全部 + 给 GameNetworkAdapter/恢复服务加可测缝并用 Fake 覆盖集成逻辑」）。

### 测试缝（seam）
- `GameNetworkAdapter` 构造函数新增可选参数 `transportFactory`（`Func<INetworkTransport>`）与 `recoveryService`（`ISessionRecoveryService`）。默认 `null` → 生产用真实 `TcpNetworkTransport` + `GameSessionRecoveryService`；测试可注入 `FakeTransport` 与 `FakeRecoveryService` 覆盖集成逻辑。
- `GameSessionRecoveryService` 新增 `ChooseHeroRequester` 委托（`(ChooseHeroReq, Action<ChossHeroAck,bool>) => void`）。默认 `DefaultChooseHero` 走真实 `HttpUtils.SendRetryLoginHeaderHttpReq`；测试注入委托即可用 Fake 替代 HTTP 重新登录。

### V2 核心测试（SGF.NetworkRefactorV2.Tests，已存在）
- 4 个 bug 回归测试：
  - BUG C：`StateMachineRejectsKickedToReconnect` / `...ToConnectingAndReconnectFailed` — 验证失败/被踢后的 `Kicked` 状态拒绝自动重连，保持返回登录路径。
  - BUG A：`MarkVerifiedResetsHeartbeatBaseline` — `MarkVerified` 重置心跳基线后首个心跳立即到期。
  - `MarkVerifiedResetsPingHealth` — 验证成功重置 Ping 健康与 last-pong 基线，避免重连后沿用旧弱网/心跳丢失状态。
  - BUG B：`ReconnectWithDelayIsInterruptedByClose` — `ReconnectWith` 延迟期间 `Close()` 必须中断（`_lifetime` 先行建立）。
  - `StartWithAlreadyCanceledTokenDoesNotMutateDisconnectedConnection` — 已取消调用方不能让 `Disconnected` 连接进入 `Connecting` 或连接 transport。
  - `ReconnectWithAlreadyCanceledTokenDoesNotMutateConnectedConnection` — 已取消调用方不能让仍 connected 的连接进入 `Reconnecting` 或关闭 transport。
  - BUG D：`PacketReaderMarksCorruptOnOversize` + `PacketReaderReturnsFalseOnShortDeclaredLength` + `CorruptPacketMovesConnectionToReconnecting` — 损坏包置 `IsCorrupt`（不抛异常），连接迁移到 `Reconnecting`。
  - `PacketReaderMarksCorruptWhenOversizePacketArrivesInOneChunk` — 超最大包长 header 与 body 同块到达时不抛 `Invalid packet length`，只置 `IsCorrupt`。
  - `PacketReaderAllowsStickyPacketsWhoseTotalExceedsMaxPacketSize` — 多个合法小包同块粘包时，即使总接收块超过 `MaxPacketSize`，也不能误判为超长单包。
  - `ReconnectAttemptExhaustionDropsOldReceiveLoopPacket` — 重连次数耗尽进入 `ReconnectFailed` 后，旧接收循环已投递事件不能继续派发。
  - `NetworkApiReleaseIsIdempotentAcrossOwnerAndHandle` — Lua handle / owner、C# token / owner 交叉重复释放必须幂等且不再派发消息。
  - `HubRecoverAndReconnectHonorsCanceledCallerWhileRecoveryIsPending` — Hub 层重连去重时，已取消调用方不能复用别人的 pending recovery。
  - `OldVerifyTimeoutDoesNotKickNewReconnectGeneration` — 旧验证超时已投递但未 Tick 时，重连生成的新 `Verifying` 连接不能被旧超时误踢成 `Kicked`。
  - `CloseDuringVerifySendKeepsClosedState` — 主动关闭打断验证包发送时，保持 `Closed` 且不派发 `Kicked` 状态事件。
  - `ReconnectDropsHeartbeatPongReturnedByOldReceiveLoopAfterCancel` — 旧接收循环取消后才返回的心跳包不能更新新连接的 Ping/心跳年龄状态。

### 适配层测试（草案，当前禁用）
- 文件保留为 `.disabled`：`NetworkRefactorV2Adapter/Tests/EditMode/AdapterTests.cs.disabled` 与 `SGF.NetworkRefactorV2Adapter.Tests.asmdef.disabled`。
- 禁用原因：当前 Adapter 源码位于 `Assembly-CSharp`，测试 asmdef 不能稳定引用它；启用会破坏当前 V2 验证门禁。
- 纯逻辑测试：命令号映射、 `GameRpcDecoder` 解码 round-trip、验证/心跳工厂输出、`GameUiIntentBridge` 映射、`GameAdapterSelfCheck.Run()` 零失败。
- Fake 集成测试（本地 `FakeTransport : INetworkTransport`）：
  - `AdapterSendsVerifyOnStartAndConnectsOnVerifySuccess` — `StartAsync` 即发验证包（cmd=1），注入验证成功（cmd=2）经 `Tick` 触发 `MarkVerified` → `Connected`。
  - `AdapterSendsHeartbeatAfterVerifiedAndHandlesPong` — 验证后 `Tick` 触发心跳（cmd=4），注入心跳回包（cmd=4）经 `Tick` 驱动 `OnHeartBeat`→`RecordPong` 不抛异常。
  - `RecoveryServiceRefreshesTokenViaInjectedDelegate` — 注入 `ChooseHeroRequester` 委托，返回新 Token → 构造 `ReconnectSessionData`（PID=请求PID、Token=新、IsReconnectSession=true）并写回 `GameLoginInfo.M_ChossHeroAck`。
  - `RecoveryServiceFaultsOnHttpFailure` / `RecoveryServiceCancelsWithToken` — 失败与取消路径。

### 运行
Unity Editor Test Runner（EditMode）选择 `SGF.NetworkRefactorV2.Tests` 与 `SGF.NetworkRefactorV2Adapter.Tests`。CLI 批量（注意 XML 输出缺口，见上）：
```powershell
& 'H:/DevelopFold/Unity Editor/2022.3.42f1c1/Editor/Unity.exe' -batchmode -nographics -quit -projectPath 'H:/Star/Stars_Project/StarsProject_Client/trunk/Stars' -runTests -testPlatform EditMode -assemblyNames SGF.NetworkRefactorV2.Tests -logFile 'H:/Star/Stars_Project/StarsProject_Client/trunk/Stars/Temp/NetworkRefactorV2-EditMode.log'
```

## Remaining before old-layer integration

- [x] **3. Adapter boundary for `ClientVerifyReq` / heartbeat payload** — 已完成：`GameVerifyPacketFactory` / `GameHeartbeatPacketFactory`（基于 `GameLoginInfo` 会话数据）。
- [x] **4. Command IDs for immediate heartbeat/time-sync** — 已完成：`V2GameCommandIds` + `GameNetworkAdapter`。**线程安全决策**：仅 `ServerTimeRetID(4843)` 标 `ImmediateCommands`（`TimeSyncClock` 用 `Interlocked`，线程安全，接收线程内直接同步，与旧 `PreHandleMessageHandleData` 一致）；`HeartBeat(4)` 改走主线程分发——`PingHealthMonitor` 无锁（旧 `Ping` 用 `lock`），并发 `RecordPong`/`UpdateHeartbeatAge` 会撕裂状态。`_lastHeartbeatSentMs` 用 `Interlocked` 跨线程读写。
- [x] **6. Unit tests (V2 bug-fix)** — 已完成：V2 核心回归测试保留在 `SGF.NetworkRefactorV2.Tests`；Adapter Fake 集成测试仍是草案，当前禁用。
- [ ] **1. Unity CLI `-testResults` XML 缺失** — 环境问题（Unity Editor CLI 输出缺口），与适配类无关，需另行排查。
- [ ] **2. Old-layer comparison harness** — 若需编译进 V2 测试程序集会拉入旧游戏/UI 依赖，违反隔离约束，暂不实现；适配层逻辑已由 `GameAdapterSelfCheck` 在 Assembly-CSharp 内自测。
- [ ] **5. Multi-socket registration (game + battle)** — V2 运行时已支持（`NetworkHub.Register(name, ...)` 多实例）。`GameNetworkAdapter` 当前构建 `game` socket；未来 `battle` socket 可再用一个 `Register("battle", ...)` + 独立 config（验证/心跳/RPC 命令号可能不同）实现，_battle socket 的协议常量与工厂待 battle 服协议确认后补充。属运行时能力，非当前集成步骤。

## Out of current V2 core scope

- `FixMessage` / `MsgRet` / `Client2Third` / `QASDK` are old business compatibility handlers and belong in the adapter/legacy compatibility layer, not the isolated V2 TCP core.
- `KCP` / `RPCLite` / `StateSync` are separate legacy subsystems and are not part of this V2 TCP repair pass.
