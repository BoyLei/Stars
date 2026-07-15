using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtoMsg;
using SGF.Network;
using SGF.Time;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// 游戏网络适配编排器：把 V2 核心网络层（NetworkHub / NetworkConnection / PingHealthMonitor /
    /// TimeSyncClock / GlobalUiArbiter）与游戏实际协议（验证 / 心跳 / 时间同步 / RPC / 会话恢复）粘起来。
    ///
    /// 职责：
    ///   1. 注册游戏 socket，注入 VerifyPacketFactory / HeartbeatPacketFactory / RpcDecoder / RecoveryPolicy；
    ///   2. 订阅验证成功(2)/失败(3) 消息，驱动 MarkVerified / MarkVerifyFailed；
    ///   3. 服务器时间(4843) 标记为 Immediate（TimeSyncClock 用 Interlocked，线程安全）接收线程内直接同步；
    ///      心跳回包(4) 走主线程分发（PingHealthMonitor 非线程安全，避免与 Tick 内 RecordPong/State 访问竞争）；
    ///   4. 每帧 Tick 驱动心跳发送、Ping 健康度、事件泵、UI 意图仲裁；
    ///   5. 通过 SocketViewStateChanged 暴露旧网络同构的 UI 状态，供未来 AppMain 接入。
    ///
    /// 注意：本类只“准备”集成，不修改 AppMain 与旧网络层（按计划约定）。
    /// </summary>
    public sealed class GameNetworkAdapter : IDisposable
    {
        public const string DefaultSocketName = "game";

        private readonly string _socketName;
        private readonly NetworkHub _hub;
        private readonly NetworkConnection _connection;
        private readonly ConnectionRecoveryCoordinator _coordinator;
        private readonly GlobalUiArbiter _arbiter;
        private readonly PingHealthMonitor _ping = new PingHealthMonitor();
        private readonly TimeSyncClock _timeSync = new TimeSyncClock();

        private long _lastHeartbeatSentMs;
        private ConnectionUiIntent _lastIntent = ConnectionUiIntent.None;
        private bool _disposed;
        private readonly Queue<int> _pingSamples = new Queue<int>();
        private PingHealthState _lastPingState = PingHealthState.Healthy;
        private const int MaxPingSamples = 5;

        public event Action<SocketViewState> SocketViewStateChanged;
        public event Action<bool> OnSocketClose;
        public event Action<RealConnectionState> OnConnectionStateChanged;
        public event Action<PingHealthState> OnPingHealthChanged;

        /// <summary>
        /// 测试缝：transportFactory 允许注入 FakeTransport 驱动连接而不触网；recoveryService 允许注入
        /// Fake 恢复服务覆盖“重连前 HTTP 重新登录”的集成逻辑。生产代码保持默认（真实 TCP + 真实恢复服务）。
        /// </summary>
        public GameNetworkAdapter(
            string socketName = DefaultSocketName,
            Func<INetworkTransport> transportFactory = null,
            ISessionRecoveryService recoveryService = null)
        {
            _socketName = socketName;
            _coordinator = new ConnectionRecoveryCoordinator(recoveryService ?? new GameSessionRecoveryService());
            _arbiter = new GlobalUiArbiter();
            _hub = new NetworkHub();
            _connection = _hub.Register(_socketName, transportFactory ?? (() => new TcpNetworkTransport()), BuildConfig());
            _arbiter.Register(_socketName, SocketUiVisibility.AlwaysVisible, UiContext.Lobby);
            SubscribeHandlers();
            _connection.StateChanged += OnStateChanged;
        }

        public NetworkConnection Connection => _connection;
        public NetworkHub Hub => _hub;
        public PingHealthMonitor Ping => _ping;
        public TimeSyncClock TimeSync => _timeSync;

        private static NetworkConnectionConfig BuildConfig()
        {
            return new NetworkConnectionConfig
            {
                MaxReconnectAttempts = 3,
                ConnectTimeout = TimeSpan.FromSeconds(5),
                VerifyTimeout = TimeSpan.FromSeconds(5),
                ReconnectDelay = TimeSpan.FromSeconds(1),
                HeartbeatInterval = TimeSpan.FromSeconds(1),
                VerifyPacketFactory = GameVerifyPacketFactory.Create,
                HeartbeatPacketFactory = GameHeartbeatPacketFactory.Create,
                RpcCommand = V2GameCommandIds.RpcMsgID,
                RpcDecoder = GameRpcDecoder.Decode,
                // 仅时间同步走 Immediate（TimeSyncClock 用 Interlocked，线程安全）；
                // 心跳回包走主线程分发，避免与 PingHealthMonitor（非线程安全）的 Tick 访问竞争
                ImmediateCommands = new HashSet<int>
                {
                    V2GameCommandIds.ServerTimeRetID
                },
                RecoveryPolicy = RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect,
                DynamicHeartbeatInterval = state => state == PingHealthState.Healthy ? TimeSpan.FromSeconds(1) : TimeSpan.FromMilliseconds(50)
            };
        }

        private void SubscribeHandlers()
        {
            // 验证成功 / 失败：走主线程事件泵（PacketReceived），由 hub.Tick 驱动
            _hub.Subscribe(_socketName, V2GameCommandIds.ClientVerifySucceedRet, OnVerifySucceed);
            _hub.Subscribe(_socketName, V2GameCommandIds.ClientVerifyFailedRet, OnVerifyFailed);
            // 心跳回包：走主线程分发（PingHealthMonitor 非线程安全）
            _hub.Subscribe(_socketName, V2GameCommandIds.HeartBeat, OnHeartBeat);
            // 服务器时间：Immediate（TimeSyncClock 线程安全），接收线程内直接同步
            _hub.SubscribeImmediate(_socketName, V2GameCommandIds.ServerTimeRetID, OnServerTime);
        }

        private void OnVerifySucceed(NetworkPacket packet)
        {
            Interlocked.Exchange(ref _lastHeartbeatSentMs, 0);
            _ping.Reset();
            _connection.MarkVerified();
        }

        private void OnVerifyFailed(NetworkPacket packet)
        {
            _connection.MarkVerifyFailed();
        }

        private void OnStateChanged(string socketName, RealConnectionState state)
        {
            OnConnectionStateChanged?.Invoke(state);
            if (state == RealConnectionState.Closed || state == RealConnectionState.Kicked || state == RealConnectionState.ReconnectFailed)
                OnSocketClose?.Invoke(state == RealConnectionState.Kicked);
        }

        private void OnHeartBeat(NetworkPacket packet)
        {
            // 延迟 = 收到回包时刻 - 上次发送时刻（与旧网络 Ping.OnNewPingRet 计算一致）
            long sent = Interlocked.Read(ref _lastHeartbeatSentMs);
            if (sent > 0)
            {
                int latency = (int)(TimeUtils.ClientNowStampMilli - sent);
                _ping.RecordPong(latency);
                _connection.RecordHeartbeatPong(latency);
                // B5: 滚动平均 ping 样本，更新全局 TimeUtils.AveragePing（与原始 Ping.RefreshPing 一致）
                _pingSamples.Enqueue(latency);
                if (_pingSamples.Count > MaxPingSamples) _pingSamples.Dequeue();
                TimeUtils.AveragePing = (int)Math.Ceiling(_pingSamples.Average());
            }
        }

        private void OnServerTime(NetworkPacket packet)
        {
            try
            {
                var ret = ServerTimeRet.Parser.ParseFrom(packet.Payload);
                _timeSync.Update(ret.TimeStamp, TimeUtils.ClientNowStampMilli);
            }
            catch (Exception ex)
            {
                SGF.Debuger.LogError($"[V2Adapter] 解析 ServerTimeRet 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动连接（首次登录）。session 来自 GameLoginInfo（地址 / PID / Token）。
        /// </summary>
        public System.Threading.Tasks.Task StartAsync(ReconnectSessionData session, CancellationToken cancellationToken = default)
        {
            return _hub.Start(_socketName, session, cancellationToken);
        }

        /// <summary>
        /// 触发一次会话恢复 + 重连（RecoveryPolicy=RefreshSessionBeforeReconnect 时走 HTTP 重新登录）。
        /// </summary>
        public System.Threading.Tasks.Task RecoverAndReconnectAsync(ReconnectSessionData current, CancellationToken cancellationToken = default)
        {
            return _hub.RecoverAndReconnect(_socketName, current, _coordinator, cancellationToken);
        }

        /// <summary>
        /// 每帧调用（主线程），驱动心跳、Ping 健康度、事件泵与 UI 意图仲裁。
        /// </summary>
        public void Tick(long nowMilliseconds)
        {
            if (_disposed) return;

            // 1) 心跳：发送并异步记录发送时刻（Interlocked 保证跨线程读写安全），用于回包延迟计算
            _connection.SendHeartbeatIfDue(nowMilliseconds).ContinueWith(t =>
            {
                try { if (t.Status == TaskStatus.RanToCompletion && t.Result) Interlocked.Exchange(ref _lastHeartbeatSentMs, nowMilliseconds); }
                catch { }
            }, TaskScheduler.Default);

            // 2) 心跳老化检测（未收到回包过久 -> HeartbeatLost）
            long sent = Interlocked.Read(ref _lastHeartbeatSentMs);
            if (sent > 0)
                _ping.UpdateHeartbeatAge(nowMilliseconds - sent);

            // 3) 事件泵：把接收线程投递的消息在主线程分发（含心跳回包 -> OnHeartBeat -> RecordPong）
            _hub.Tick();

            // 4) 弱网状态变化回调（C3）
            if (_ping.State != _lastPingState)
            {
                _lastPingState = _ping.State;
                OnPingHealthChanged?.Invoke(_ping.State);
            }

            // 5) UI 意图仲裁
            ConnectionUiIntent intent = ConnectionUiPolicy.Evaluate(_connection.State, _ping.State);
            _arbiter.UpdateIntent(_socketName, intent);
            if (_arbiter.CurrentIntent.Intent != _lastIntent)
            {
                _lastIntent = _arbiter.CurrentIntent.Intent;
                SocketViewStateChanged?.Invoke(GameUiIntentBridge.ToSocketViewState(_arbiter.CurrentIntent.Intent));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _hub.Stop(_socketName);
            _hub.Dispose();
        }
    }
}
