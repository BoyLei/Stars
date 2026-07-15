using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SGF.NetworkRefactorV2
{
    public sealed class ReconnectSessionData
    {
        public string SocketAddress { get; }
        public ulong Pid { get; }
        public string Token { get; }
        public bool IsReconnectSession { get; }
        public ReconnectSessionData(string address, ulong pid, string token, bool reconnect) { SocketAddress = address; Pid = pid; Token = token; IsReconnectSession = reconnect; }
    }

    public interface ISessionRecoveryService
    {
        Task<ReconnectSessionData> RecoverAsync(string socketName, ReconnectSessionData current, CancellationToken cancellationToken);
    }

    public sealed class ConnectionRecoveryCoordinator
    {
        private readonly ISessionRecoveryService _service;
        private readonly object _gate = new object();
        private readonly Dictionary<string, Task<ReconnectSessionData>> _pending = new Dictionary<string, Task<ReconnectSessionData>>(StringComparer.Ordinal);
        public ConnectionRecoveryCoordinator(ISessionRecoveryService service) { _service = service ?? throw new ArgumentNullException(nameof(service)); }

        public Task<ReconnectSessionData> RecoverAsync(string socket, RecoveryAuthenticationPolicy policy, ReconnectSessionData current, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Canceled(cancellationToken);
            if (policy == RecoveryAuthenticationPolicy.ReuseCurrentCredential) return Task.FromResult(current);
            if (policy == RecoveryAuthenticationPolicy.ManualOnly) throw new InvalidOperationException("Connection recovery is manual.");
            lock (_gate)
            {
                if (_pending.TryGetValue(socket, out Task<ReconnectSessionData> task)) return task;
                task = RecoverOnce(socket, current, cancellationToken); _pending.Add(socket, task); return task;
            }
        }

        private static Task<ReconnectSessionData> Canceled(CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<ReconnectSessionData>();
            source.SetCanceled();
            return source.Task;
        }

        private async Task<ReconnectSessionData> RecoverOnce(string socket, ReconnectSessionData current, CancellationToken cancellationToken)
        {
            await Task.Yield();
            try { return await _service.RecoverAsync(socket, current, cancellationToken); }
            finally { lock (_gate) _pending.Remove(socket); }
        }
    }
}
