# NetworkRefactorV2 隔离重构计划

## Summary

- 新增 `Assets/Scripts/StarFramework/NetworkRefactorV2/`，命名空间统一为 `SGF.NetworkRefactorV2`。
- 保留旧网络层和现有 `NetworkRefactor`，不修改 `AppMain`、业务模块、Lua 脚本或 UI。
- 独立实现多 Socket、协议编解码、恢复协调、Ping 状态、UI 策略仲裁及 C#/Lua API。
- 使用 Unity EditMode 单元测试验证后，才允许规划旧网络层接入。

## Implementation Changes

- `Core`：实现 `RealConnectionState`、`PingHealthState`、`ConnectionUiIntent`、连接状态机、线程安全事件队列和不可变时间同步快照。
- `Protocol`：独立实现现有 6 字节包头、自定义协议、Protobuf、RPC、加解密、分包/粘包及最大包长校验。
- `Transport`：实现可注入 TCP 传输、完整发送循环、接收线程解析；后台线程只更新基础状态和投递事件，不调用 Unity API。
- `Recovery`：实现 `ReconnectSessionData`、`ISessionRecoveryService` 和 `ConnectionRecoveryCoordinator`，支持：
  - `ReuseCurrentCredential`
  - `RefreshSessionBeforeReconnect`
  - `ManualOnly`
- `Runtime`：实现动态命名的 `NetworkHub`、独立 `NetworkConnection`、共享默认配置加单连接覆盖，以及主线程 `Tick()` 事件泵。
- `Policy`：实现纯函数式 `ConnectionUiPolicy` 和 `GlobalUiArbiter`；按 `Login/Lobby/Battle/Loading`、Focused Socket、可见性策略仲裁，并缓存后台 Socket 的待处理意图。
- Ping、重连次数和超时默认值沿用旧网络层当前配置；全部允许通过每连接配置覆盖，不增加额外配置系统。
- 删除新目录中的临时探针和脚手架；文件使用 UTF-8 无 BOM、CRLF。

## Public Interfaces

- `NetworkHub.Register/Start/Stop/Send/Tick`
- `NetworkHub.Subscribe(socketName, command, callback)`，支持 `AnySocket`
- C# 订阅返回 `IDisposable`；Lua 订阅返回数值句柄，并支持按句柄或 owner 释放。
- `NetworkConnection.Start(session)`、`ReconnectWith(session)`、`Close()`
- `ISessionRecoveryService.RecoverAsync(...)`
- `GlobalUiArbiter.SetContext(...)`、`UpdateIntent(...)`、`CurrentIntent`
- 新代码不直接引用具体登录 HTTP Handler 或 UI 控件，均通过接口/事件注入。

## Test Plan

- 状态机：合法迁移、非法迁移、主动关闭、验证失败、重连耗尽、被踢。
- 协议：黄金字节、分包、粘包、多包、异常长度、最大包限制、自定义/PB/RPC、加密往返。
- 对照测试：未加密数据与旧实现逐字节一致；旧实现已知的“加密标志配明文”只作为缺陷回归样本，V2 必须发送真实密文。
- 传输：模拟 partial send、接收异常、重复关闭、取消和并发队列。
- 恢复：三种认证策略、刷新失败、刷新成功后重新验证、并发恢复去重。
- UI：文档中的所有单 Socket 映射；战斗中 `gameSocket` 断开不抢占 `battleSocket`；返回 Lobby 后重新评估缓存意图。
- 线程：接收回调不直接执行 Unity/UI 逻辑，事件只在 `Tick()` 后派发。
- 时间同步：快照原子替换，不出现字段混合状态。
- 使用 `SGF.NetworkRefactorV2.Tests` EditMode asmdef 执行测试，不连接真实服务器。
- 若 Unity 批处理超过 30 秒，不持续轮询；提供完整测试命令由用户执行。若仍出现许可证错误，明确记录为环境阻塞，不宣称测试通过或交付完成。

## Assumptions

- 不新增第三方依赖，使用现有 Unity Test Framework 1.1.33。
- 不创建 `.meta` 之外的旧层适配或业务接入代码。
- 不修改当前工作区已有的无关变更。
- 新目录若执行时已存在，则先停止并核对，禁止覆盖未知文件。
