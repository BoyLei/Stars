using System;
using System.Collections.Generic;

namespace SGF.NetworkRefactorV2
{
    public static class ConnectionUiPolicy
    {
        public static ConnectionUiIntent Evaluate(RealConnectionState connection, PingHealthState ping)
        {
            if (connection == RealConnectionState.Kicked || connection == RealConnectionState.ReconnectFailed) return ConnectionUiIntent.ReturnLoginDialog;
            if (connection == RealConnectionState.Reconnecting) return ConnectionUiIntent.RealReconnectDialog;
            if (connection != RealConnectionState.Connected) return ConnectionUiIntent.None;
            if (ping == PingHealthState.ReturnLogin) return ConnectionUiIntent.ReturnLoginDialog;
            if (ping == PingHealthState.Weak) return ConnectionUiIntent.WeakLoading;
            if (ping == PingHealthState.CriticalWeak || ping == PingHealthState.HeartbeatLost) return ConnectionUiIntent.FakeReconnectDialog;
            return ConnectionUiIntent.None;
        }
    }

    public sealed class UiIntentResult
    {
        public string SocketName { get; }
        public ConnectionUiIntent Intent { get; }
        public UiIntentResult(string socketName, ConnectionUiIntent intent) { SocketName = socketName; Intent = intent; }
    }

    public sealed class GlobalUiArbiter
    {
        private sealed class Entry { public SocketUiVisibility Visibility; public UiContext Focus; public ConnectionUiIntent Intent; }
        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private UiContext _context;
        public UiIntentResult CurrentIntent { get; private set; } = new UiIntentResult(null, ConnectionUiIntent.None);

        public void Register(string socketName, SocketUiVisibility visibility, UiContext focus)
        {
            if (string.IsNullOrWhiteSpace(socketName)) throw new ArgumentException("Socket name is required.", nameof(socketName));
            _entries[socketName] = new Entry { Visibility = visibility, Focus = focus };
            Evaluate();
        }
        public void SetContext(UiContext context) { _context = context; Evaluate(); }
        public void UpdateIntent(string socketName, ConnectionUiIntent intent) { if (!_entries.TryGetValue(socketName, out Entry entry)) throw new KeyNotFoundException(socketName); entry.Intent = intent; Evaluate(); }
        private void Evaluate()
        {
            foreach (KeyValuePair<string, Entry> pair in _entries)
                if (pair.Value.Visibility != SocketUiVisibility.Silent && (pair.Value.Visibility == SocketUiVisibility.AlwaysVisible || pair.Value.Focus == _context) && pair.Value.Intent != ConnectionUiIntent.None)
                { CurrentIntent = new UiIntentResult(pair.Key, pair.Value.Intent); return; }
            CurrentIntent = new UiIntentResult(null, ConnectionUiIntent.None);
        }
    }
}
