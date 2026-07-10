using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SGF.NetworkRefactorV2
{
    public interface IAsyncByteSender
    {
        Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken);
    }

    public static class TransportSend
    {
        public static async Task SendAllAsync(IAsyncByteSender sender, byte[] data, CancellationToken cancellationToken)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (data == null) throw new ArgumentNullException(nameof(data));
            int offset = 0;
            while (offset < data.Length)
            {
                int sent = await sender.SendAsync(data, offset, data.Length - offset, cancellationToken);
                if (sent <= 0) throw new SocketException((int)SocketError.ConnectionReset);
                offset += sent;
            }
        }
    }

    public interface INetworkTransport : IAsyncByteSender, IDisposable
    {
        bool Connected { get; }
        Task ConnectAsync(string host, int port, CancellationToken cancellationToken);
        Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken);
        void Close();
    }

    public sealed class TcpNetworkTransport : INetworkTransport
    {
        private TcpClient _client;
        public bool Connected => _client != null && _client.Connected;

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            Close();
            _client = new TcpClient { NoDelay = true };
            using (cancellationToken.Register(Close)) await _client.ConnectAsync(host, port);
            cancellationToken.ThrowIfCancellationRequested();
        }

        public async Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
        {
            if (!Connected) throw new InvalidOperationException("Transport is not connected.");
            await _client.GetStream().WriteAsync(data, offset, count, cancellationToken);
            return count;
        }

        public Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            if (!Connected) throw new InvalidOperationException("Transport is not connected.");
            return _client.GetStream().ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        }

        public void Close() { TcpClient client = Interlocked.Exchange(ref _client, null); client?.Close(); }
        public void Dispose() { Close(); }
    }
}
