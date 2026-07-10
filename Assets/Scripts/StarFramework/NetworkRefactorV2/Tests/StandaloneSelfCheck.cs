#if NETWORK_REFACTOR_V2_STANDALONE
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SGF.NetworkRefactorV2.Tests
{
    internal static class StandaloneSelfCheck
    {
        private static int _checks;

        private static int Main()
        {
            try { Run().GetAwaiter().GetResult(); Console.WriteLine("NetworkRefactorV2 self-check: " + _checks + " passed, 0 failed"); return 0; }
            catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
        }

        private static async Task Run()
        {
            var state = new ConnectionStateMachine(); state.MoveTo(RealConnectionState.Connecting);
            ExpectThrows<InvalidOperationException>(() => state.MoveTo(RealConnectionState.Connected));
            state.MoveTo(RealConnectionState.Verifying); state.MoveTo(RealConnectionState.Kicked); Check(state.State == RealConnectionState.Kicked, "verify failure path");

            byte[] bytes = PacketCodec.Encode(513, new byte[] { 1, 2, 3 }, true);
            Check(bytes[6] != 1, "encrypted payload");
            Check(bytes[0] == 5 && bytes[3] == 2 && bytes[6] == 255 && bytes[7] == 9 && bytes[8] == 32, "legacy encrypted golden bytes");
            var reader = new PacketReader(1024); reader.Append(bytes, 0, 2); Check(!reader.TryRead(out _), "fragment wait");
            reader.Append(bytes, 2, bytes.Length - 2); Check(reader.TryRead(out NetworkPacket packet) && packet.Command == 513 && packet.Payload[2] == 3, "packet round trip");
            Check(PacketCodec.Encode(0x1234, new byte[] { 9, 8, 7 }, false)[4] == 0x34, "legacy header");
            Check(RpcCodec.Encode(0x1234, new byte[] { 7, 8 })[0] == 14, "rpc envelope");
            Check(ConnectionUiPolicy.Evaluate(RealConnectionState.Connected, PingHealthState.CriticalWeak) == ConnectionUiIntent.FakeReconnectDialog, "ui policy");
            var ping = new PingHealthMonitor(); for (int i = 0; i < 5; i++) ping.RecordPong(920); Check(ping.State == PingHealthState.Weak, "legacy ping threshold");
            ping.Reset(); for (int i = 0; i < PingHealthMonitor.TotalWeakLimit; i++) { ping.RecordPong(920); ping.RecordPong(1); } Check(ping.State == PingHealthState.ReturnLogin, "legacy total weak limit");

            var arbiter = new GlobalUiArbiter(); arbiter.Register("game", SocketUiVisibility.VisibleWhenFocused, UiContext.Lobby); arbiter.SetContext(UiContext.Battle);
            arbiter.UpdateIntent("game", ConnectionUiIntent.ReturnLoginDialog); Check(arbiter.CurrentIntent.Intent == ConnectionUiIntent.None, "background hidden");
            arbiter.SetContext(UiContext.Lobby); Check(arbiter.CurrentIntent.Intent == ConnectionUiIntent.ReturnLoginDialog, "focused visible");

            var sender = new PartialSender(); await TransportSend.SendAllAsync(sender, new byte[7], CancellationToken.None); Check(sender.Total == 7 && sender.Calls > 1, "partial sends");
            var hub = new NetworkHub(); int calls = 0; hub.Subscribe("*", 9, _ => calls++); hub.EnqueueReceived("battle", new NetworkPacket(9, 0, new byte[0])); Check(calls == 0, "main thread gate"); hub.Tick(); Check(calls == 1, "main thread dispatch");
            var connection = hub.Register("game", () => new FakeTransport(), new NetworkConnectionConfig()); hub.Subscribe("game", 10, _ => calls++); connection.InjectReceivedForTests(new NetworkPacket(10, 0, new byte[0])); hub.Tick(); Check(calls == 2, "one tick dispatch");
            int entityCalls = 0; int groupCalls = 0; hub.SubscribeRpc("game", 14, 1001, 9, _ => entityCalls++); hub.SubscribeRpcGroup("game", 1001, _ => groupCalls++);
            connection.InjectReceivedForTests(new NetworkPacket(14, 0, new byte[0], 1001, 8)); hub.Tick(); Check(entityCalls == 0 && groupCalls == 1, "rpc group without entity");
            connection.InjectReceivedForTests(new NetworkPacket(14, 0, new byte[0], 1001, 9)); hub.Tick(); Check(entityCalls == 1 && groupCalls == 2, "rpc entity and group");

            var transport = new FakeTransport();
            var config = new NetworkConnectionConfig { VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false), HeartbeatPacketFactory = () => new NetworkOutboundPacket(200, new byte[] { 2 }, false), HeartbeatInterval = TimeSpan.FromMilliseconds(100) };
            var verifiedConnection = new NetworkConnection("verify", transport, new MainThreadEventPump(), config);
            await verifiedConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            Check(transport.Sent == 1, "verify packet sent");
            bool blocked = false; try { await verifiedConnection.Send(101, new byte[] { 3 }); } catch (InvalidOperationException) { blocked = true; } Check(blocked, "business blocked before verify");
            verifiedConnection.MarkVerified(); await verifiedConnection.Send(101, new byte[] { 3 }); Check(transport.Sent == 2, "business after verify");
            Check(await verifiedConnection.SendHeartbeatIfDue(100), "heartbeat sent"); Check(!await verifiedConnection.SendHeartbeatIfDue(150), "heartbeat interval");

            byte[] immediateBytes = PacketCodec.Encode(300, new byte[] { 4 }, false);
            var immediateTransport = new FakeTransport { ReceiveData = immediateBytes };
            var immediateConfig = new NetworkConnectionConfig(); immediateConfig.ImmediateCommands.Add(300);
            var immediateConnection = new NetworkConnection("immediate", immediateTransport, new MainThreadEventPump(), immediateConfig);
            int immediateCalls = 0; immediateConnection.ImmediatePacketReceived += (_, __) => immediateCalls++;
            await immediateConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield(); await Task.Yield(); Check(immediateCalls == 1, "immediate command bypasses tick");
        }

        private static void Check(bool condition, string name) { if (!condition) throw new Exception("Failed: " + name); _checks++; }
        private static void ExpectThrows<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new Exception("Expected " + typeof(T).Name); }

        private sealed class PartialSender : IAsyncByteSender
        {
            public int Calls; public int Total;
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken token) { Calls++; int sent = Math.Min(2, count); Total += sent; return Task.FromResult(sent); }
        }

        private sealed class FakeTransport : INetworkTransport
        {
            public int Sent;
            public byte[] ReceiveData;
            private bool _received;
            public bool Connected { get; private set; }
            public Task ConnectAsync(string host, int port, CancellationToken token) { Connected = true; return Task.CompletedTask; }
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken token) { Sent++; return Task.FromResult(count); }
            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken token)
            {
                if (ReceiveData != null && !_received)
                {
                    _received = true;
                    Buffer.BlockCopy(ReceiveData, 0, buffer, 0, ReceiveData.Length);
                    return Task.FromResult(ReceiveData.Length);
                }
                var source = new TaskCompletionSource<int>(); token.Register(() => source.TrySetCanceled()); return source.Task;
            }
            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }
    }
}
#endif
