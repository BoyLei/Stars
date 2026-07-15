using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SGF.NetworkRefactorV2
{
    public sealed class NetworkConnectionConfig
    {
        public const int DefaultMaxPacketSize = 4 * 1024 * 1024;
        public int MaxPacketSize { get; set; } = DefaultMaxPacketSize;
        public int MaxReconnectAttempts { get; set; } = 3;
        public int MaxTotalReconnectAttempts { get; set; } = 5;
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan VerifyTimeout { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(0.5);
        public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(1);
        public Func<ReconnectSessionData, NetworkOutboundPacket> VerifyPacketFactory { get; set; }
        public Func<NetworkPacket, bool?> VerifyResultDecoder { get; set; }
        public Func<NetworkOutboundPacket> HeartbeatPacketFactory { get; set; }
        public Func<NetworkPacket, int?> HeartbeatPongDecoder { get; set; }
        public ISet<int> ImmediateCommands { get; set; } = new HashSet<int>();
        /// <summary>
        /// RPC 命令号。配置后 ReceiveLoop 会自动用 RpcCodec.TryDecode 解析 RPC 封装，
        /// 构造带 IsRpc/MessageCommand 的 NetworkPacket。默认解码 entityId=0。
        /// </summary>
        public int? RpcCommand { get; set; }
        /// <summary>
        /// 自定义 RPC 解码器（可选）。接收原始 NetworkPacket，返回解码后的 NetworkPacket（IsRpc=true）。
        /// 返回 null 表示不是 RPC 包。设置后覆盖 RpcCommand 的默认解码行为，
        /// 集成层可在此解析 RpcMsg protobuf 提取完整 entityId。
        /// </summary>
        public Func<NetworkPacket, NetworkPacket> RpcDecoder { get; set; }
        public RecoveryAuthenticationPolicy RecoveryPolicy { get; set; } = RecoveryAuthenticationPolicy.ReuseCurrentCredential;
        public Func<PingHealthState, TimeSpan> DynamicHeartbeatInterval { get; set; }
    }

    public sealed class NetworkConnection : IDisposable
    {
        private readonly INetworkTransport _transport;
        private readonly MainThreadEventPump _events;
        private readonly PacketReader _reader;
        private readonly ConnectionStateMachine _state = new ConnectionStateMachine();
        private readonly SemaphoreSlim _sendGate = new SemaphoreSlim(1, 1);
        private readonly PingHealthMonitor _pingHealth = new PingHealthMonitor();
        private CancellationTokenSource _lifetime;
        private int _reconnectAttempts;
        private int _totalReconnectAttempts;
        private int _receiveGeneration;
        private long _lastHeartbeatMilliseconds = long.MinValue;
        private long _lastPongMilliseconds = long.MinValue;
        private readonly object _sendCacheGate = new object();
        private readonly Queue<PendingSend> _sendCache = new Queue<PendingSend>();
        public string Name { get; }
        public NetworkConnectionConfig Config { get; }
        public RealConnectionState State => _state.State;
        public PingHealthMonitor PingHealth => _pingHealth;
        public event Action<string, NetworkPacket> PacketReceived;
        public event Action<string, NetworkPacket> ImmediatePacketReceived;
        public event Action<string, RealConnectionState> StateChanged;

        public NetworkConnection(string name, INetworkTransport transport, MainThreadEventPump events, NetworkConnectionConfig config = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Connection name is required.", nameof(name));
            Name = name; _transport = transport ?? throw new ArgumentNullException(nameof(transport)); _events = events ?? throw new ArgumentNullException(nameof(events));
            Config = config ?? new NetworkConnectionConfig(); _reader = new PacketReader(Config.MaxPacketSize);
        }

        public async Task Start(ReconnectSessionData session, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            cancellationToken.ThrowIfCancellationRequested();
            ParseAddress(session.SocketAddress, out string host, out int port);
            _reader.Reset();
            Move(RealConnectionState.Connecting);
            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            try
            {
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token))
                {
                    timeout.CancelAfter(Config.ConnectTimeout);
                    await _transport.ConnectAsync(host, port, timeout.Token);
                }
                Move(RealConnectionState.Verifying);
                await SendVerifyPacketOrFail(session, _lifetime.Token);
                int generation = Interlocked.Increment(ref _receiveGeneration);
                _ = WatchVerifyTimeout(_lifetime.Token, generation);
                _ = ReceiveLoop(_lifetime.Token, generation);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                if (State == RealConnectionState.Connecting) MarkReconnectFailed();
                throw;
            }
        }

        public async Task ReconnectWith(ReconnectSessionData session, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (State == RealConnectionState.Kicked || State == RealConnectionState.Closed)
                throw new InvalidOperationException("Cannot reconnect a kicked/closed connection.");
            _lifetime?.Cancel();
            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _transport.Close();
            _reconnectAttempts++;
            _totalReconnectAttempts++;
            if (_reconnectAttempts > Config.MaxReconnectAttempts)
            {
                if (_totalReconnectAttempts >= Config.MaxTotalReconnectAttempts)
                {
                    MarkReconnectFailed();
                }
                else
                {
                    _reconnectAttempts = 0;
                    Interlocked.Increment(ref _receiveGeneration);
                    _lifetime?.Cancel();
                    _transport.Close();
                    if (State == RealConnectionState.Connected) Move(RealConnectionState.Reconnecting);
                    Move(RealConnectionState.ReconnectRetryable);
                }
                return;
            }
            if (State != RealConnectionState.Reconnecting && State != RealConnectionState.ReconnectFailed) Move(RealConnectionState.Reconnecting);
            try
            {
                if (Config.ReconnectDelay > TimeSpan.Zero) await Task.Delay(Config.ReconnectDelay, _lifetime.Token);
                Move(RealConnectionState.Connecting);
                await StartFromConnecting(session, _lifetime.Token);
            }
            catch (OperationCanceledException) when (State == RealConnectionState.Closed || cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                MarkReconnectFailed();
                throw;
            }
        }

        private async Task StartFromConnecting(ReconnectSessionData session, CancellationToken cancellationToken)
        {
            ParseAddress(session.SocketAddress, out string host, out int port);
            _reader.Reset();
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token))
            {
                timeout.CancelAfter(Config.ConnectTimeout);
                await _transport.ConnectAsync(host, port, timeout.Token);
            }
            Move(RealConnectionState.Verifying);
            await SendVerifyPacketOrFail(session, _lifetime.Token);
            int generation = Interlocked.Increment(ref _receiveGeneration);
            _ = WatchVerifyTimeout(_lifetime.Token, generation);
            _ = ReceiveLoop(_lifetime.Token, generation);
        }

        public void MarkVerified()
        {
            _reconnectAttempts = 0;
            _totalReconnectAttempts = 0;
            _lastHeartbeatMilliseconds = long.MinValue;
            _lastPongMilliseconds = long.MinValue;
            _pingHealth.Reset();
            if (State != RealConnectionState.Connected) Move(RealConnectionState.Connected);
            _ = FlushSendCache();
        }

        private async Task FlushSendCache()
        {
            while (true)
            {
                PendingSend pending;
                lock (_sendCacheGate)
                {
                    if (_sendCache.Count == 0) return;
                    pending = _sendCache.Dequeue();
                }
                await SendRaw(pending.Command, pending.Payload, pending.Encrypt, _lifetime?.Token ?? CancellationToken.None);
            }
        }
        public void MarkVerifyFailed()
        {
            _lifetime?.Cancel();
            _transport.Close();
            ClearSendCache();
            Move(RealConnectionState.Kicked);
        }
        public void MarkReconnectFailed()
        {
            Interlocked.Increment(ref _receiveGeneration);
            _lifetime?.Cancel();
            _transport.Close();
            RealConnectionState state = State;
            if (state == RealConnectionState.Closed || state == RealConnectionState.Kicked || state == RealConnectionState.ReconnectFailed) return;
            if (state == RealConnectionState.Connected) Move(RealConnectionState.Reconnecting);
            Move(RealConnectionState.ReconnectFailed);
        }
        public Task Send(int command, byte[] payload, bool encrypt = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (State == RealConnectionState.Kicked || State == RealConnectionState.Closed)
                throw new InvalidOperationException("Cannot send on a kicked/closed connection.");
            if (State != RealConnectionState.Connected)
            {
                lock (_sendCacheGate) _sendCache.Enqueue(new PendingSend(command, payload, encrypt));
                return Task.CompletedTask;
            }
            return SendRaw(command, payload, encrypt, cancellationToken);
        }

        public async Task<bool> SendHeartbeatIfDue(long nowMilliseconds, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (State != RealConnectionState.Connected || Config.HeartbeatPacketFactory == null) return false;
            TimeSpan interval = Config.HeartbeatInterval;
            if (Config.DynamicHeartbeatInterval != null)
            {
                TimeSpan dynamic = Config.DynamicHeartbeatInterval(_pingHealth.State);
                if (dynamic > TimeSpan.Zero) interval = dynamic;
            }
            if (_lastHeartbeatMilliseconds != long.MinValue && nowMilliseconds - _lastHeartbeatMilliseconds < interval.TotalMilliseconds) return false;
            NetworkOutboundPacket packet = Config.HeartbeatPacketFactory();
            await SendRaw(packet.Command, packet.Payload, packet.Encrypt, cancellationToken);
            _lastHeartbeatMilliseconds = nowMilliseconds;
            return true;
        }

        public PingHealthState RecordHeartbeatPong(int latencyMilliseconds) { return _pingHealth.RecordPong(latencyMilliseconds); }
        public PingHealthState RecordHeartbeatPong(int latencyMilliseconds, long nowMilliseconds)
        {
            _lastPongMilliseconds = nowMilliseconds;
            return RecordHeartbeatPong(latencyMilliseconds);
        }
        public PingHealthState CheckHeartbeatAge(long nowMilliseconds)
        {
            if (_lastPongMilliseconds == long.MinValue) return _pingHealth.State;
            return _pingHealth.UpdateHeartbeatAge(nowMilliseconds - _lastPongMilliseconds);
        }

        public void InjectReceivedForTests(NetworkPacket packet) { _events.Post(() => PacketReceived?.Invoke(Name, packet)); }

        public void Close()
        {
            Interlocked.Increment(ref _receiveGeneration);
            _lifetime?.Cancel(); _transport.Close();
            ClearSendCache();
            if (State != RealConnectionState.Closed) Move(RealConnectionState.Closed);
        }

        private void ClearSendCache()
        {
            lock (_sendCacheGate) _sendCache.Clear();
        }

        private async Task ReceiveLoop(CancellationToken cancellationToken, int generation)
        {
            var buffer = new byte[8192];
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int count = await _transport.ReceiveAsync(buffer, cancellationToken);
                    if (cancellationToken.IsCancellationRequested || generation != _receiveGeneration) return;
                    if (count == 0) throw new SocketException((int)SocketError.ConnectionReset);
                    _reader.Append(buffer, 0, count);
                    while (_reader.TryRead(out NetworkPacket packet))
                    {
                        NetworkPacket effective = DecodeRpcIfNeeded(packet);
                        if (HandleVerifyResultIfNeeded(effective, generation)) continue;
                        if (HandleHeartbeatPongIfNeeded(effective)) continue;
                        if (Config.ImmediateCommands != null && Config.ImmediateCommands.Contains(effective.Command)) ImmediatePacketReceived?.Invoke(Name, effective);
                        else _events.Post(() => { if (generation == _receiveGeneration) PacketReceived?.Invoke(Name, effective); });
                    }
                    if (_reader.IsCorrupt)
                    {
                        if (State != RealConnectionState.Closed) _events.Post(() => { if (generation == _receiveGeneration && State != RealConnectionState.Closed) Move(RealConnectionState.Reconnecting); });
                        return;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception)
            {
                if (cancellationToken.IsCancellationRequested || State == RealConnectionState.Closed) return;
                RealConnectionState failedState = State;
                _events.Post(() =>
                {
                    if (State == RealConnectionState.Closed) return;
                    if (generation != _receiveGeneration) return;
                    Move(failedState == RealConnectionState.Verifying ? RealConnectionState.Kicked : RealConnectionState.Reconnecting);
                });
            }
        }

        private Task SendVerifyPacket(ReconnectSessionData session, CancellationToken cancellationToken)
        {
            if (Config.VerifyPacketFactory == null) return Task.CompletedTask;
            NetworkOutboundPacket packet = Config.VerifyPacketFactory(session);
            return SendRaw(packet.Command, packet.Payload, packet.Encrypt, cancellationToken);
        }

        private async Task SendVerifyPacketOrFail(ReconnectSessionData session, CancellationToken cancellationToken)
        {
            try
            {
                await SendVerifyPacket(session, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || State == RealConnectionState.Closed)
            {
                throw;
            }
            catch
            {
                MarkVerifyFailed();
                throw;
            }
        }

        private async Task WatchVerifyTimeout(CancellationToken cancellationToken, int generation)
        {
            try
            {
                await Task.Delay(Config.VerifyTimeout, cancellationToken);
                if (!cancellationToken.IsCancellationRequested && State == RealConnectionState.Verifying) _events.Post(() => { if (generation == _receiveGeneration && State == RealConnectionState.Verifying) MarkVerifyFailed(); });
            }
            catch (OperationCanceledException) { }
        }

        private async Task SendRaw(int command, byte[] payload, bool encrypt, CancellationToken cancellationToken)
        {
            await _sendGate.WaitAsync(cancellationToken);
            try
            {
                await TransportSend.SendAllAsync(_transport, PacketCodec.Encode(command, payload, encrypt), cancellationToken);
            }
            finally
            {
                _sendGate.Release();
            }
        }

        private bool HandleVerifyResultIfNeeded(NetworkPacket packet, int generation)
        {
            if (State != RealConnectionState.Verifying || Config.VerifyResultDecoder == null) return false;
            bool? result = Config.VerifyResultDecoder(packet);
            if (!result.HasValue) return false;
            _events.Post(() =>
            {
                if (generation != _receiveGeneration) return;
                if (State != RealConnectionState.Verifying) return;
                if (result.Value) MarkVerified();
                else MarkVerifyFailed();
            });
            return true;
        }

        private bool HandleHeartbeatPongIfNeeded(NetworkPacket packet)
        {
            if (Config.HeartbeatPongDecoder == null) return false;
            int? latency = Config.HeartbeatPongDecoder(packet);
            if (!latency.HasValue) return false;
            RecordHeartbeatPong(latency.Value, CurrentMilliseconds());
            return true;
        }

        /// <summary>
        /// 如果是 RPC 包，解析 RPC 封装构造带 IsRpc/MessageCommand 的 NetworkPacket。
        /// 优先使用 RpcDecoder（集成层注入），否则用 RpcCodec.TryDecode 默认解码。
        /// </summary>
        private NetworkPacket DecodeRpcIfNeeded(NetworkPacket packet)
        {
            if (Config.RpcDecoder != null) return Config.RpcDecoder(packet) ?? packet;
            if (Config.RpcCommand.HasValue && packet.Command == Config.RpcCommand.Value
                && RpcCodec.TryDecode(packet.Payload, out int msgCmd, out byte[] innerPayload))
                return new NetworkPacket(packet.Command, packet.SerializeType, innerPayload, msgCmd, 0);
            return packet;
        }
        private void Move(RealConnectionState state) { _state.MoveTo(state); _events.Post(() => StateChanged?.Invoke(Name, state)); }
        private static long CurrentMilliseconds() { return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds; }
        private static void ParseAddress(string address, out string host, out int port)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Socket address is required.", nameof(address));
            int separator = address.LastIndexOf(':');
            if (separator <= 0 || !int.TryParse(address.Substring(separator + 1), out port) || port < 1 || port > 65535) throw new FormatException("Socket address must be host:port.");
            host = address.Substring(0, separator);
        }
        public void Dispose() { Close(); _transport.Dispose(); _lifetime?.Dispose(); _sendGate.Dispose(); }

        private readonly struct PendingSend
        {
            public readonly int Command;
            public readonly byte[] Payload;
            public readonly bool Encrypt;
            public PendingSend(int command, byte[] payload, bool encrypt) { Command = command; Payload = payload; Encrypt = encrypt; }
        }
    }

    public sealed class NetworkHub : IDisposable
    {
        private readonly Dictionary<string, NetworkConnection> _connections = new Dictionary<string, NetworkConnection>(StringComparer.Ordinal);
        private readonly MainThreadEventPump _events = new MainThreadEventPump();
        private readonly SubscriptionRegistry<NetworkPacket> _subscriptions = new SubscriptionRegistry<NetworkPacket>();
        private readonly RpcSubscriptionRegistry<NetworkPacket> _rpcSubscriptions = new RpcSubscriptionRegistry<NetworkPacket>();
        private readonly SubscriptionRegistry<NetworkPacket> _immediateSubscriptions = new SubscriptionRegistry<NetworkPacket>();
        private readonly Queue<QueuedPacket> _lockedMessages = new Queue<QueuedPacket>();
        private readonly Dictionary<string, Task> _recoveries = new Dictionary<string, Task>(StringComparer.Ordinal);
        private readonly object _recoveryGate = new object();
        private bool _messageLocked;

        public bool IsMessageLocked => _messageLocked;
        public event Action<string, NetworkPacket> PacketDispatching;

        public NetworkConnection Register(string name, Func<INetworkTransport> transportFactory = null, NetworkConnectionConfig config = null)
        {
            if (_connections.ContainsKey(name)) throw new InvalidOperationException($"Connection '{name}' is already registered.");
            var connection = new NetworkConnection(name, (transportFactory ?? (() => new TcpNetworkTransport()))(), _events, config);
            connection.PacketReceived += EnqueueReceived; connection.ImmediatePacketReceived += DispatchImmediate; _connections.Add(name, connection); return connection;
        }
        public Task Start(string name, ReconnectSessionData session, CancellationToken cancellationToken = default(CancellationToken)) { return Get(name).Start(session, cancellationToken); }
        public Task RecoverAndReconnect(string name, ReconnectSessionData current, ConnectionRecoveryCoordinator coordinator, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (coordinator == null) throw new ArgumentNullException(nameof(coordinator));
            if (cancellationToken.IsCancellationRequested) return CanceledTask();
            lock (_recoveryGate)
            {
                if (_recoveries.TryGetValue(name, out Task running)) return running;
                Task task = RecoverAndReconnectOnce(name, current, coordinator, cancellationToken);
                _recoveries.Add(name, task);
                return task;
            }
        }
        private async Task RecoverAndReconnectOnce(string name, ReconnectSessionData current, ConnectionRecoveryCoordinator coordinator, CancellationToken cancellationToken)
        {
            NetworkConnection connection = Get(name);
            try
            {
                ReconnectSessionData recovered = await coordinator.RecoverAsync(name, connection.Config.RecoveryPolicy, current, cancellationToken);
                await connection.ReconnectWith(recovered, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                connection.MarkReconnectFailed();
                throw;
            }
            finally
            {
                lock (_recoveryGate) _recoveries.Remove(name);
            }
        }
        public void Stop(string name) { Get(name).Close(); }
        public Task Send(string name, int command, byte[] payload, bool encrypt = false, CancellationToken cancellationToken = default(CancellationToken)) { return Get(name).Send(command, payload, encrypt, cancellationToken); }
        public IDisposable Subscribe(string socketName, int command, Action<NetworkPacket> callback, object owner = null) { return _subscriptions.Subscribe(socketName, command, callback, owner); }
        public IDisposable SubscribeImmediate(string socketName, int command, Action<NetworkPacket> callback, object owner = null) { return _immediateSubscriptions.Subscribe(socketName, command, callback, owner); }
        public IDisposable SubscribeRpc(string socketName, int rpcCommand, int messageCommand, ulong entityId, Action<NetworkPacket> callback, object owner = null) { return _rpcSubscriptions.Subscribe(socketName, rpcCommand, messageCommand, entityId, callback, owner); }
        public IDisposable SubscribeRpcGroup(string socketName, int messageCommand, Action<NetworkPacket> callback, object owner = null) { return _rpcSubscriptions.SubscribeGroup(socketName, messageCommand, callback, owner); }
        public void UnsubscribeOwner(object owner) { _subscriptions.UnsubscribeOwner(owner); _rpcSubscriptions.UnsubscribeOwner(owner); _immediateSubscriptions.UnsubscribeOwner(owner); }
        public void LockMessages(bool locked) { _messageLocked = locked; }
        public void EnqueueReceived(string socketName, NetworkPacket packet)
        {
            if (_events.IsTicking) DispatchOrQueue(socketName, packet);
            else _events.Post(() => DispatchOrQueue(socketName, packet));
        }
        private void DispatchOrQueue(string socketName, NetworkPacket packet)
        {
            if (_messageLocked) _lockedMessages.Enqueue(new QueuedPacket(socketName, packet));
            else DispatchReceived(socketName, packet);
        }
        private void DispatchReceived(string socketName, NetworkPacket packet)
        {
            NotifyPacketDispatching(socketName, packet);
            _subscriptions.Dispatch(socketName, packet.Command, packet);
            if (packet.IsRpc) _rpcSubscriptions.Dispatch(socketName, packet.Command, packet.MessageCommand, packet.EntityId, packet);
        }
        private void NotifyPacketDispatching(string socketName, NetworkPacket packet)
        {
            Action<string, NetworkPacket> handlers = PacketDispatching;
            if (handlers == null) return;
            foreach (Action<string, NetworkPacket> handler in handlers.GetInvocationList())
            {
                try { handler(socketName, packet); }
                catch (Exception) { }
            }
        }
        private void DispatchImmediate(string socketName, NetworkPacket packet)
        {
            _immediateSubscriptions.Dispatch(socketName, packet.Command, packet);
        }
        public void Tick()
        {
            try { _events.Tick(); }
            catch (Exception) { LockMessages(false); }
            finally { FlushLockedMessages(); }
        }
        private void FlushLockedMessages()
        {
            while (!_messageLocked && _lockedMessages.Count > 0)
            {
                QueuedPacket message = _lockedMessages.Dequeue();
                try { DispatchReceived(message.SocketName, message.Packet); }
                catch (Exception) { }
            }
        }
        private NetworkConnection Get(string name) { if (!_connections.TryGetValue(name, out NetworkConnection connection)) throw new KeyNotFoundException(name); return connection; }
        public void Dispose() { foreach (NetworkConnection connection in _connections.Values) connection.Dispose(); _connections.Clear(); }
        private static Task CanceledTask() { var source = new TaskCompletionSource<object>(); source.SetCanceled(); return source.Task; }

        private readonly struct QueuedPacket
        {
            public readonly string SocketName;
            public readonly NetworkPacket Packet;
            public QueuedPacket(string socketName, NetworkPacket packet) { SocketName = socketName; Packet = packet; }
        }
    }

    public sealed class CSharpNetworkApi
    {
        private readonly NetworkHub _hub;
        public CSharpNetworkApi(NetworkHub hub) { _hub = hub ?? throw new ArgumentNullException(nameof(hub)); }
        public Task Send(string socket, int command, byte[] payload, bool encrypt = false) { return _hub.Send(socket, command, payload, encrypt); }
        public Task SendCustom<T>(string socket, int command, T message, Func<T, byte[]> serialize, bool encrypt = false)
        { if (serialize == null) throw new ArgumentNullException(nameof(serialize)); return Send(socket, command, serialize(message), encrypt); }
        public Task SendProtobuf<T>(string socket, int command, T message, Func<T, byte[]> serialize, bool encrypt = false)
        { return SendCustom(socket, command, message, serialize, encrypt); }
        public Task SendRpc(string socket, int outerCommand, int methodCommand, byte[] protobufPayload, bool encrypt = false)
        { return Send(socket, outerCommand, RpcCodec.Encode(methodCommand, protobufPayload), encrypt); }
        public IDisposable Subscribe(string socket, int command, Action<NetworkPacket> callback, object owner = null) { return _hub.Subscribe(socket, command, callback, owner); }
        public IDisposable SubscribeImmediate(string socket, int command, Action<NetworkPacket> callback, object owner = null) { return _hub.SubscribeImmediate(socket, command, callback, owner); }
        public IDisposable SubscribeRpc(string socket, int rpcCommand, int messageCommand, ulong entityId, Action<NetworkPacket> callback, object owner = null) { return _hub.SubscribeRpc(socket, rpcCommand, messageCommand, entityId, callback, owner); }
        public IDisposable SubscribeRpcGroup(string socket, int messageCommand, Action<NetworkPacket> callback, object owner = null) { return _hub.SubscribeRpcGroup(socket, messageCommand, callback, owner); }
        public void UnsubscribeOwner(object owner) { _hub.UnsubscribeOwner(owner); }
        public void LockMessages(bool locked) { _hub.LockMessages(locked); }
    }

    public sealed class LuaNetworkApi
    {
        private sealed class HandleEntry { public IDisposable Token; public object Owner; }
        private readonly NetworkHub _hub; private readonly Dictionary<long, HandleEntry> _handles = new Dictionary<long, HandleEntry>(); private long _next;
        public LuaNetworkApi(NetworkHub hub) { _hub = hub ?? throw new ArgumentNullException(nameof(hub)); }
        public long Subscribe(string socket, int command, Action<NetworkPacket> callback, object owner = null) { return Add(_hub.Subscribe(socket, command, callback, owner), owner); }
        public long SubscribeImmediate(string socket, int command, Action<NetworkPacket> callback, object owner = null) { return Add(_hub.SubscribeImmediate(socket, command, callback, owner), owner); }
        public long SubscribeRpc(string socket, int rpcCommand, int messageCommand, ulong entityId, Action<NetworkPacket> callback, object owner = null) { return Add(_hub.SubscribeRpc(socket, rpcCommand, messageCommand, entityId, callback, owner), owner); }
        public long SubscribeRpcGroup(string socket, int messageCommand, Action<NetworkPacket> callback, object owner = null) { return Add(_hub.SubscribeRpcGroup(socket, messageCommand, callback, owner), owner); }
        public void Unsubscribe(long handle) { if (_handles.TryGetValue(handle, out HandleEntry entry)) { _handles.Remove(handle); entry.Token.Dispose(); } }
        public void UnsubscribeOwner(object owner)
        {
            _hub.UnsubscribeOwner(owner);
            if (owner == null) return;
            var handles = new List<long>();
            foreach (KeyValuePair<long, HandleEntry> pair in _handles) if (ReferenceEquals(pair.Value.Owner, owner)) handles.Add(pair.Key);
            foreach (long handle in handles) Unsubscribe(handle);
        }
        public void LockMessages(bool locked) { _hub.LockMessages(locked); }
        public Task Send(string socket, int command, byte[] payload, bool encrypt = false) { return _hub.Send(socket, command, payload, encrypt); }
        public Task SendRpc(string socket, int outerCommand, int methodCommand, byte[] payload, bool encrypt = false) { return Send(socket, outerCommand, RpcCodec.Encode(methodCommand, payload), encrypt); }
        private long Add(IDisposable token, object owner) { long handle = Interlocked.Increment(ref _next); _handles.Add(handle, new HandleEntry { Token = token, Owner = owner }); return handle; }
    }
}
