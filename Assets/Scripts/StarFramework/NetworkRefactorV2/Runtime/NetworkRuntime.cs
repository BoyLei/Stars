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
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan VerifyTimeout { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(0.5);
        public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(1);
        public Func<ReconnectSessionData, NetworkOutboundPacket> VerifyPacketFactory { get; set; }
        public Func<NetworkOutboundPacket> HeartbeatPacketFactory { get; set; }
        public ISet<int> ImmediateCommands { get; set; } = new HashSet<int>();
        public RecoveryAuthenticationPolicy RecoveryPolicy { get; set; } = RecoveryAuthenticationPolicy.ReuseCurrentCredential;
    }

    public sealed class NetworkConnection : IDisposable
    {
        private readonly INetworkTransport _transport;
        private readonly MainThreadEventPump _events;
        private readonly PacketReader _reader;
        private readonly ConnectionStateMachine _state = new ConnectionStateMachine();
        private CancellationTokenSource _lifetime;
        private int _reconnectAttempts;
        private long _lastHeartbeatMilliseconds = long.MinValue;
        public string Name { get; }
        public NetworkConnectionConfig Config { get; }
        public RealConnectionState State => _state.State;
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
            ParseAddress(session.SocketAddress, out string host, out int port);
            Move(RealConnectionState.Connecting);
            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token))
            {
                timeout.CancelAfter(Config.ConnectTimeout);
                await _transport.ConnectAsync(host, port, timeout.Token);
            }
            Move(RealConnectionState.Verifying);
            await SendVerifyPacket(session, _lifetime.Token);
            _ = WatchVerifyTimeout(_lifetime.Token);
            _ = ReceiveLoop(_lifetime.Token);
        }

        public async Task ReconnectWith(ReconnectSessionData session, CancellationToken cancellationToken = default(CancellationToken))
        {
            _lifetime?.Cancel();
            _transport.Close();
            _reconnectAttempts++;
            if (_reconnectAttempts > Config.MaxReconnectAttempts)
            {
                Move(RealConnectionState.ReconnectFailed);
                return;
            }
            if (State != RealConnectionState.Reconnecting && State != RealConnectionState.ReconnectFailed) Move(RealConnectionState.Reconnecting);
            if (Config.ReconnectDelay > TimeSpan.Zero) await Task.Delay(Config.ReconnectDelay, cancellationToken);
            Move(RealConnectionState.Connecting);
            await StartFromConnecting(session, cancellationToken);
        }

        private async Task StartFromConnecting(ReconnectSessionData session, CancellationToken cancellationToken)
        {
            ParseAddress(session.SocketAddress, out string host, out int port);
            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token))
            {
                timeout.CancelAfter(Config.ConnectTimeout);
                await _transport.ConnectAsync(host, port, timeout.Token);
            }
            Move(RealConnectionState.Verifying);
            await SendVerifyPacket(session, _lifetime.Token);
            _ = WatchVerifyTimeout(_lifetime.Token);
            _ = ReceiveLoop(_lifetime.Token);
        }

        public void MarkVerified() { _reconnectAttempts = 0; Move(RealConnectionState.Connected); }
        public void MarkVerifyFailed() { Move(RealConnectionState.Kicked); }
        public Task Send(int command, byte[] payload, bool encrypt = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (State != RealConnectionState.Connected) throw new InvalidOperationException("Business messages can only be sent after verify succeeds.");
            return SendRaw(command, payload, encrypt, cancellationToken);
        }

        public Task<bool> SendHeartbeatIfDue(long nowMilliseconds, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (State != RealConnectionState.Connected || Config.HeartbeatPacketFactory == null) return Task.FromResult(false);
            if (_lastHeartbeatMilliseconds != long.MinValue && nowMilliseconds - _lastHeartbeatMilliseconds < Config.HeartbeatInterval.TotalMilliseconds) return Task.FromResult(false);
            _lastHeartbeatMilliseconds = nowMilliseconds;
            NetworkOutboundPacket packet = Config.HeartbeatPacketFactory();
            return SendRaw(packet.Command, packet.Payload, packet.Encrypt, cancellationToken).ContinueWith(_ => true, cancellationToken);
        }

        public void InjectReceivedForTests(NetworkPacket packet) { _events.Post(() => PacketReceived?.Invoke(Name, packet)); }

        public void Close()
        {
            _lifetime?.Cancel(); _transport.Close();
            if (State != RealConnectionState.Closed) Move(RealConnectionState.Closed);
        }

        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int count = await _transport.ReceiveAsync(buffer, cancellationToken);
                    if (count == 0) throw new SocketException((int)SocketError.ConnectionReset);
                    _reader.Append(buffer, 0, count);
                    while (_reader.TryRead(out NetworkPacket packet))
                    {
                        if (Config.ImmediateCommands != null && Config.ImmediateCommands.Contains(packet.Command)) ImmediatePacketReceived?.Invoke(Name, packet);
                        else _events.Post(() => PacketReceived?.Invoke(Name, packet));
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

        private async Task WatchVerifyTimeout(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Config.VerifyTimeout, cancellationToken);
                if (!cancellationToken.IsCancellationRequested && State == RealConnectionState.Verifying) _events.Post(() => { if (State == RealConnectionState.Verifying) Move(RealConnectionState.Reconnecting); });
            }
            catch (OperationCanceledException) { }
        }

        private Task SendRaw(int command, byte[] payload, bool encrypt, CancellationToken cancellationToken)
        { return TransportSend.SendAllAsync(_transport, PacketCodec.Encode(command, payload, encrypt), cancellationToken); }
        private void Move(RealConnectionState state) { _state.MoveTo(state); _events.Post(() => StateChanged?.Invoke(Name, state)); }
        private static void ParseAddress(string address, out string host, out int port)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Socket address is required.", nameof(address));
            int separator = address.LastIndexOf(':');
            if (separator <= 0 || !int.TryParse(address.Substring(separator + 1), out port) || port < 1 || port > 65535) throw new FormatException("Socket address must be host:port.");
            host = address.Substring(0, separator);
        }
        public void Dispose() { Close(); _transport.Dispose(); _lifetime?.Dispose(); }
    }

    public sealed class NetworkHub : IDisposable
    {
        private readonly Dictionary<string, NetworkConnection> _connections = new Dictionary<string, NetworkConnection>(StringComparer.Ordinal);
        private readonly MainThreadEventPump _events = new MainThreadEventPump();
        private readonly SubscriptionRegistry<NetworkPacket> _subscriptions = new SubscriptionRegistry<NetworkPacket>();
        private readonly RpcSubscriptionRegistry<NetworkPacket> _rpcSubscriptions = new RpcSubscriptionRegistry<NetworkPacket>();

        public NetworkConnection Register(string name, Func<INetworkTransport> transportFactory = null, NetworkConnectionConfig config = null)
        {
            if (_connections.ContainsKey(name)) throw new InvalidOperationException($"Connection '{name}' is already registered.");
            var connection = new NetworkConnection(name, (transportFactory ?? (() => new TcpNetworkTransport()))(), _events, config);
            connection.PacketReceived += EnqueueReceived; connection.ImmediatePacketReceived += DispatchReceived; _connections.Add(name, connection); return connection;
        }
        public Task Start(string name, ReconnectSessionData session, CancellationToken cancellationToken = default(CancellationToken)) { return Get(name).Start(session, cancellationToken); }
        public async Task RecoverAndReconnect(string name, ReconnectSessionData current, ConnectionRecoveryCoordinator coordinator, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (coordinator == null) throw new ArgumentNullException(nameof(coordinator));
            NetworkConnection connection = Get(name);
            ReconnectSessionData recovered = await coordinator.RecoverAsync(name, connection.Config.RecoveryPolicy, current, cancellationToken);
            await connection.ReconnectWith(recovered, cancellationToken);
        }
        public void Stop(string name) { Get(name).Close(); }
        public Task Send(string name, int command, byte[] payload, bool encrypt = false, CancellationToken cancellationToken = default(CancellationToken)) { return Get(name).Send(command, payload, encrypt, cancellationToken); }
        public IDisposable Subscribe(string socketName, int command, Action<NetworkPacket> callback, object owner = null) { return _subscriptions.Subscribe(socketName, command, callback, owner); }
        public IDisposable SubscribeRpc(string socketName, int rpcCommand, int messageCommand, ulong entityId, Action<NetworkPacket> callback, object owner = null) { return _rpcSubscriptions.Subscribe(socketName, rpcCommand, messageCommand, entityId, callback, owner); }
        public IDisposable SubscribeRpcGroup(string socketName, int messageCommand, Action<NetworkPacket> callback, object owner = null) { return _rpcSubscriptions.SubscribeGroup(socketName, messageCommand, callback, owner); }
        public void UnsubscribeOwner(object owner) { _subscriptions.UnsubscribeOwner(owner); _rpcSubscriptions.UnsubscribeOwner(owner); }
        public void EnqueueReceived(string socketName, NetworkPacket packet)
        {
            if (_events.IsTicking) DispatchReceived(socketName, packet);
            else _events.Post(() => DispatchReceived(socketName, packet));
        }
        private void DispatchReceived(string socketName, NetworkPacket packet)
        {
            _subscriptions.Dispatch(socketName, packet.Command, packet);
            if (packet.IsRpc) _rpcSubscriptions.Dispatch(socketName, packet.Command, packet.MessageCommand, packet.EntityId, packet);
        }
        public void Tick() { _events.Tick(); }
        private NetworkConnection Get(string name) { if (!_connections.TryGetValue(name, out NetworkConnection connection)) throw new KeyNotFoundException(name); return connection; }
        public void Dispose() { foreach (NetworkConnection connection in _connections.Values) connection.Dispose(); _connections.Clear(); }
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
        public IDisposable SubscribeRpc(string socket, int rpcCommand, int messageCommand, ulong entityId, Action<NetworkPacket> callback, object owner = null) { return _hub.SubscribeRpc(socket, rpcCommand, messageCommand, entityId, callback, owner); }
        public IDisposable SubscribeRpcGroup(string socket, int messageCommand, Action<NetworkPacket> callback, object owner = null) { return _hub.SubscribeRpcGroup(socket, messageCommand, callback, owner); }
        public void UnsubscribeOwner(object owner) { _hub.UnsubscribeOwner(owner); }
    }

    public sealed class LuaNetworkApi
    {
        private readonly NetworkHub _hub; private readonly Dictionary<long, IDisposable> _handles = new Dictionary<long, IDisposable>(); private long _next;
        public LuaNetworkApi(NetworkHub hub) { _hub = hub ?? throw new ArgumentNullException(nameof(hub)); }
        public long Subscribe(string socket, int command, Action<NetworkPacket> callback, object owner = null) { long handle = Interlocked.Increment(ref _next); _handles.Add(handle, _hub.Subscribe(socket, command, callback, owner)); return handle; }
        public long SubscribeRpc(string socket, int rpcCommand, int messageCommand, ulong entityId, Action<NetworkPacket> callback, object owner = null) { long handle = Interlocked.Increment(ref _next); _handles.Add(handle, _hub.SubscribeRpc(socket, rpcCommand, messageCommand, entityId, callback, owner)); return handle; }
        public long SubscribeRpcGroup(string socket, int messageCommand, Action<NetworkPacket> callback, object owner = null) { long handle = Interlocked.Increment(ref _next); _handles.Add(handle, _hub.SubscribeRpcGroup(socket, messageCommand, callback, owner)); return handle; }
        public void Unsubscribe(long handle) { if (_handles.TryGetValue(handle, out IDisposable token)) { _handles.Remove(handle); token.Dispose(); } }
        public void UnsubscribeOwner(object owner) { _hub.UnsubscribeOwner(owner); }
        public Task Send(string socket, int command, byte[] payload, bool encrypt = false) { return _hub.Send(socket, command, payload, encrypt); }
        public Task SendRpc(string socket, int outerCommand, int methodCommand, byte[] payload, bool encrypt = false) { return Send(socket, outerCommand, RpcCodec.Encode(methodCommand, payload), encrypt); }
    }
}
