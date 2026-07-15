using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SGF.NetworkRefactorV2
{
    public enum RealConnectionState { Disconnected, Connecting, Verifying, Connected, Reconnecting, ReconnectRetryable, ReconnectFailed, Kicked, Closed }
    public enum PingHealthState { Healthy, Weak, CriticalWeak, HeartbeatLost, ReturnLogin }
    public enum ConnectionUiIntent { None, WeakLoading, FakeReconnectDialog, RealReconnectDialog, ReconnectRetryDialog, ReturnLoginDialog }
    public enum UiContext { Login, Lobby, Battle, Loading }
    public enum SocketUiVisibility { AlwaysVisible, VisibleWhenFocused, Silent }
    public enum RecoveryAuthenticationPolicy { ReuseCurrentCredential, RefreshSessionBeforeReconnect, ManualOnly }

    public sealed class ConnectionStateMachine
    {
        public RealConnectionState State { get; private set; } = RealConnectionState.Disconnected;

        public void MoveTo(RealConnectionState next)
        {
            if (!CanMove(State, next)) throw new InvalidOperationException($"Invalid connection state transition: {State} -> {next}");
            State = next;
        }

        private static bool CanMove(RealConnectionState current, RealConnectionState next)
        {
            if (next == RealConnectionState.Closed) return current != RealConnectionState.Closed;
            if (next == RealConnectionState.Kicked) return current != RealConnectionState.Closed;
            switch (current)
            {
                case RealConnectionState.Disconnected: return next == RealConnectionState.Connecting;
                case RealConnectionState.Connecting: return next == RealConnectionState.Verifying || next == RealConnectionState.Reconnecting || next == RealConnectionState.ReconnectFailed;
                case RealConnectionState.Verifying: return next == RealConnectionState.Connected || next == RealConnectionState.Reconnecting || next == RealConnectionState.ReconnectFailed;
                case RealConnectionState.Connected: return next == RealConnectionState.Reconnecting || next == RealConnectionState.Disconnected;
                case RealConnectionState.Reconnecting: return next == RealConnectionState.Connecting || next == RealConnectionState.ReconnectFailed || next == RealConnectionState.ReconnectRetryable;
                case RealConnectionState.ReconnectRetryable: return next == RealConnectionState.Connecting || next == RealConnectionState.Reconnecting || next == RealConnectionState.ReconnectFailed;
                case RealConnectionState.ReconnectFailed: return next == RealConnectionState.Connecting;
                case RealConnectionState.Kicked: return false;
                default: return false;
            }
        }
    }

    public sealed class MainThreadEventPump
    {
        private readonly ConcurrentQueue<Action> _events = new ConcurrentQueue<Action>();
        public bool IsTicking { get; private set; }
        public void Post(Action action) { if (action == null) throw new ArgumentNullException(nameof(action)); _events.Enqueue(action); }
        public void Tick()
        {
            IsTicking = true;
            try
            {
                while (_events.TryDequeue(out Action action))
                {
                    try { action(); }
                    catch (Exception) { /* 单个回调异常不中断后续消息处理，与原始 NetworkManager.OnMyUpdate 行为一致 */ }
                }
            }
            finally { IsTicking = false; }
        }
    }

    public sealed class PingHealthMonitor
    {
        public const int WeakLatencyMilliseconds = 460 * 2;
        public const int WeakSampleCount = 5;
        public const int CriticalWeakSampleCount = 12;
        public const int HeartbeatLostMilliseconds = 5000;
        public const int TotalWeakLimit = 1000;
        private int _consecutiveWeak;
        private int _totalWeak;
        public PingHealthState State { get; private set; } = PingHealthState.Healthy;

        public PingHealthState RecordPong(int latencyMilliseconds)
        {
            if (latencyMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(latencyMilliseconds));
            if (State == PingHealthState.ReturnLogin) return State;
            if (latencyMilliseconds >= WeakLatencyMilliseconds) { _consecutiveWeak++; _totalWeak++; }
            else _consecutiveWeak = 0;
            if (_totalWeak >= TotalWeakLimit) { State = PingHealthState.ReturnLogin; return State; }
            State = _consecutiveWeak >= CriticalWeakSampleCount ? PingHealthState.CriticalWeak : _consecutiveWeak >= WeakSampleCount ? PingHealthState.Weak : PingHealthState.Healthy;
            return State;
        }

        public PingHealthState UpdateHeartbeatAge(long milliseconds)
        {
            if (milliseconds < 0) throw new ArgumentOutOfRangeException(nameof(milliseconds));
            if (State == PingHealthState.ReturnLogin) return State;
            if (milliseconds >= HeartbeatLostMilliseconds) State = PingHealthState.HeartbeatLost;
            return State;
        }

        public void Reset() { _consecutiveWeak = 0; _totalWeak = 0; State = PingHealthState.Healthy; }
    }

    public sealed class TimeSyncSnapshot
    {
        public long ServerTimeStamp { get; }
        public long ClientReceiveTimeStamp { get; }
        public long Offset { get; }
        public TimeSyncSnapshot(long server, long client) { ServerTimeStamp = server; ClientReceiveTimeStamp = client; Offset = server - client; }
    }

    public sealed class TimeSyncClock
    {
        private TimeSyncSnapshot _current = new TimeSyncSnapshot(0, 0);
        public TimeSyncSnapshot Current => Volatile.Read(ref _current);
        public void Update(long server, long client) { Interlocked.Exchange(ref _current, new TimeSyncSnapshot(server, client)); }
    }

    public sealed class SubscriptionRegistry<T>
    {
        public const string AnySocket = "*";
        private sealed class Entry { public long Id; public string Socket; public int Command; public Action<T> Callback; public object Owner; }
        private sealed class Token : IDisposable
        {
            private SubscriptionRegistry<T> _owner; private readonly long _id;
            public Token(SubscriptionRegistry<T> owner, long id) { _owner = owner; _id = id; }
            public void Dispose() { Interlocked.Exchange(ref _owner, null)?.Remove(_id); }
        }
        private readonly object _gate = new object();
        private readonly List<Entry> _entries = new List<Entry>();
        private long _nextId;

        public IDisposable Subscribe(string socket, int command, Action<T> callback, object owner = null)
        {
            if (string.IsNullOrWhiteSpace(socket)) throw new ArgumentException("Socket name is required.", nameof(socket));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            var entry = new Entry { Id = Interlocked.Increment(ref _nextId), Socket = socket, Command = command, Callback = callback, Owner = owner };
            lock (_gate) _entries.Add(entry);
            return new Token(this, entry.Id);
        }

        public void Dispatch(string socket, int command, T message)
        {
            Entry[] snapshot;
            lock (_gate) snapshot = _entries.FindAll(x => x.Command == command && (x.Socket == socket || x.Socket == AnySocket)).ToArray();
            foreach (Entry entry in snapshot) entry.Callback(message);
        }

        public void UnsubscribeOwner(object owner) { if (owner == null) return; lock (_gate) _entries.RemoveAll(x => ReferenceEquals(x.Owner, owner)); }
        public void UnsubscribeSocket(string socket) { lock (_gate) _entries.RemoveAll(x => x.Socket == socket); }
        private void Remove(long id) { lock (_gate) _entries.RemoveAll(x => x.Id == id); }
    }

    public sealed class RpcSubscriptionRegistry<T>
    {
        private sealed class Entry { public long Id; public string Socket; public int RpcCommand; public int MessageCommand; public ulong EntityId; public bool IsGroup; public Action<T> Callback; public object Owner; }
        private sealed class Token : IDisposable
        {
            private RpcSubscriptionRegistry<T> _owner; private readonly long _id;
            public Token(RpcSubscriptionRegistry<T> owner, long id) { _owner = owner; _id = id; }
            public void Dispose() { Interlocked.Exchange(ref _owner, null)?.Remove(_id); }
        }
        private readonly object _gate = new object();
        private readonly List<Entry> _entries = new List<Entry>();
        private long _nextId;

        public IDisposable Subscribe(string socket, int rpcCommand, int messageCommand, ulong entityId, Action<T> callback, object owner = null)
        { return Add(socket, rpcCommand, messageCommand, entityId, false, callback, owner); }

        public IDisposable SubscribeGroup(string socket, int messageCommand, Action<T> callback, object owner = null)
        { return Add(socket, 0, messageCommand, 0, true, callback, owner); }

        public void Dispatch(string socket, int rpcCommand, int messageCommand, ulong entityId, T message)
        {
            Entry[] snapshot;
            lock (_gate)
                snapshot = _entries.FindAll(x =>
                    (x.Socket == socket || x.Socket == SubscriptionRegistry<T>.AnySocket) &&
                    x.MessageCommand == messageCommand &&
                    (x.IsGroup || (x.RpcCommand == rpcCommand && x.EntityId == entityId))).ToArray();
            foreach (Entry entry in snapshot) entry.Callback(message);
        }

        public void UnsubscribeOwner(object owner) { if (owner == null) return; lock (_gate) _entries.RemoveAll(x => ReferenceEquals(x.Owner, owner)); }
        private IDisposable Add(string socket, int rpcCommand, int messageCommand, ulong entityId, bool group, Action<T> callback, object owner)
        {
            if (string.IsNullOrWhiteSpace(socket)) throw new ArgumentException("Socket name is required.", nameof(socket));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            var entry = new Entry { Id = Interlocked.Increment(ref _nextId), Socket = socket, RpcCommand = rpcCommand, MessageCommand = messageCommand, EntityId = entityId, IsGroup = group, Callback = callback, Owner = owner };
            lock (_gate) _entries.Add(entry);
            return new Token(this, entry.Id);
        }
        private void Remove(long id) { lock (_gate) _entries.RemoveAll(x => x.Id == id); }
    }
}
