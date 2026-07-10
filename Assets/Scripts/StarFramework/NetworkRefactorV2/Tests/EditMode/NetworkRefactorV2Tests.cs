using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SGF.NetworkRefactorV2.Tests
{
    public sealed class NetworkRefactorV2Tests
    {
        [Test] public void StateMachineRejectsInvalidTransition()
        {
            var state = new ConnectionStateMachine();
            state.MoveTo(RealConnectionState.Connecting);
            Assert.Throws<InvalidOperationException>(() => state.MoveTo(RealConnectionState.Connected));
        }

        [Test] public void StateMachineAllowsVerifyFailureToReturnLoginPath()
        {
            var state = new ConnectionStateMachine();
            state.MoveTo(RealConnectionState.Connecting);
            state.MoveTo(RealConnectionState.Verifying);
            state.MoveTo(RealConnectionState.Kicked);
            Assert.AreEqual(RealConnectionState.Kicked, state.State);
        }

        [Test] public void PacketCodecRoundTripsEncryptedFragmentedPacket()
        {
            byte[] encoded = PacketCodec.Encode(513, new byte[] { 1, 2, 3 }, true);
            Assert.AreNotEqual(1, encoded[6]);
            var reader = new PacketReader(1024);
            reader.Append(encoded, 0, 2);
            Assert.IsFalse(reader.TryRead(out _));
            reader.Append(encoded, 2, encoded.Length - 2);
            Assert.IsTrue(reader.TryRead(out NetworkPacket packet));
            Assert.AreEqual(513, packet.Command);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, packet.Payload);
        }

        [Test] public void PacketCodecMatchesLegacyPlainHeaderAndRejectsOversize()
        {
            CollectionAssert.AreEqual(new byte[] { 5, 0, 0, 0, 0x34, 0x12, 9, 8, 7 }, PacketCodec.Encode(0x1234, new byte[] { 9, 8, 7 }, false));
            var reader = new PacketReader(8);
            Assert.Throws<InvalidOperationException>(() => reader.Append(new byte[] { 100, 0, 0, 0, 1, 0 }, 0, 6));
        }

        [Test] public void PacketCodecMatchesLegacyEncryptedGoldenBytes()
        {
            CollectionAssert.AreEqual(new byte[] { 5, 0, 0, 2, 1, 2, 255, 9, 32 }, PacketCodec.Encode(513, new byte[] { 1, 2, 3 }, true));
        }

        [Test] public void RpcCodecUsesLegacyEnvelope()
        {
            CollectionAssert.AreEqual(new byte[] { 14, 0x34, 0x12, 2, 0, 7, 8 }, RpcCodec.Encode(0x1234, new byte[] { 7, 8 }));
        }

        [TestCase(RealConnectionState.Connected, PingHealthState.Healthy, ConnectionUiIntent.None)]
        [TestCase(RealConnectionState.Connected, PingHealthState.Weak, ConnectionUiIntent.WeakLoading)]
        [TestCase(RealConnectionState.Connected, PingHealthState.CriticalWeak, ConnectionUiIntent.FakeReconnectDialog)]
        [TestCase(RealConnectionState.Connected, PingHealthState.ReturnLogin, ConnectionUiIntent.ReturnLoginDialog)]
        [TestCase(RealConnectionState.Reconnecting, PingHealthState.Healthy, ConnectionUiIntent.RealReconnectDialog)]
        [TestCase(RealConnectionState.Kicked, PingHealthState.Healthy, ConnectionUiIntent.ReturnLoginDialog)]
        public void UiPolicyMapsStates(RealConnectionState connection, PingHealthState ping, ConnectionUiIntent expected)
        {
            Assert.AreEqual(expected, ConnectionUiPolicy.Evaluate(connection, ping));
        }

        [Test] public void ArbiterDelaysBackgroundSocketUntilFocused()
        {
            var arbiter = new GlobalUiArbiter();
            arbiter.Register("game", SocketUiVisibility.VisibleWhenFocused, UiContext.Lobby);
            arbiter.Register("battle", SocketUiVisibility.VisibleWhenFocused, UiContext.Battle);
            arbiter.SetContext(UiContext.Battle);
            arbiter.UpdateIntent("game", ConnectionUiIntent.ReturnLoginDialog);
            Assert.AreEqual(ConnectionUiIntent.None, arbiter.CurrentIntent.Intent);
            arbiter.SetContext(UiContext.Lobby);
            Assert.AreEqual(ConnectionUiIntent.ReturnLoginDialog, arbiter.CurrentIntent.Intent);
        }

        [Test] public void EventPumpDispatchesOnlyWhenTicked()
        {
            var pump = new MainThreadEventPump();
            int calls = 0;
            pump.Post(() => calls++);
            Assert.AreEqual(0, calls);
            pump.Tick();
            Assert.AreEqual(1, calls);
        }

        [Test] public void SubscriptionRegistryScopesAndUnsubscribes()
        {
            var registry = new SubscriptionRegistry<byte[]>();
            int calls = 0;
            IDisposable token = registry.Subscribe("game", 1, _ => calls++);
            registry.Subscribe(SubscriptionRegistry<byte[]>.AnySocket, 1, _ => calls++);
            registry.Dispatch("battle", 1, Array.Empty<byte>());
            registry.Dispatch("game", 1, Array.Empty<byte>());
            token.Dispose();
            registry.Dispatch("game", 1, Array.Empty<byte>());
            Assert.AreEqual(4, calls);
        }

        [Test] public async Task RecoveryCoordinatorRefreshesOnceForConcurrentRequests()
        {
            var service = new FakeRecoveryService();
            var coordinator = new ConnectionRecoveryCoordinator(service);
            var current = new ReconnectSessionData("x", 1, "old", false);
            Task<ReconnectSessionData> first = coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, current, CancellationToken.None);
            Task<ReconnectSessionData> second = coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, current, CancellationToken.None);
            await Task.WhenAll(first, second);
            Assert.AreEqual(1, service.Calls);
            Assert.AreEqual("new", first.Result.Token);
        }

        [Test] public void TimeSyncClockReplacesWholeSnapshot()
        {
            var clock = new TimeSyncClock();
            clock.Update(1000, 900);
            TimeSyncSnapshot snapshot = clock.Current;
            Assert.AreEqual(1000, snapshot.ServerTimeStamp);
            Assert.AreEqual(900, snapshot.ClientReceiveTimeStamp);
            Assert.AreEqual(100, snapshot.Offset);
        }

        [Test] public void PingMonitorUsesLegacyThresholdsWithoutDrivingUi()
        {
            var monitor = new PingHealthMonitor();
            for (int i = 0; i < 5; i++) monitor.RecordPong(920);
            Assert.AreEqual(PingHealthState.Weak, monitor.State);
            for (int i = 5; i < 12; i++) monitor.RecordPong(920);
            Assert.AreEqual(PingHealthState.CriticalWeak, monitor.State);
            monitor.UpdateHeartbeatAge(5000);
            Assert.AreEqual(PingHealthState.HeartbeatLost, monitor.State);
        }

        [Test] public void PingMonitorUsesLegacyTotalWeakLimit()
        {
            var monitor = new PingHealthMonitor();
            for (int i = 0; i < PingHealthMonitor.TotalWeakLimit; i++)
            {
                monitor.RecordPong(920);
                monitor.RecordPong(1);
            }
            Assert.AreEqual(PingHealthState.ReturnLogin, monitor.State);
        }

        [Test] public async Task TransportSendAllHandlesPartialWrites()
        {
            var sender = new PartialSender();
            await TransportSend.SendAllAsync(sender, new byte[7], CancellationToken.None);
            Assert.AreEqual(7, sender.Total);
            Assert.Greater(sender.Calls, 1);
        }

        [Test] public void HubRoutesNamedAndWildcardSubscriptionsOnTick()
        {
            var hub = new NetworkHub();
            int calls = 0;
            hub.Subscribe("game", 9, _ => calls++);
            hub.Subscribe(SubscriptionRegistry<NetworkPacket>.AnySocket, 9, _ => calls++);
            hub.EnqueueReceived("battle", new NetworkPacket(9, 0, Array.Empty<byte>()));
            Assert.AreEqual(0, calls);
            hub.Tick();
            Assert.AreEqual(1, calls);
            hub.EnqueueReceived("game", new NetworkPacket(9, 0, Array.Empty<byte>()));
            hub.Tick();
            Assert.AreEqual(3, calls);
        }

        [Test] public void HubDispatchesReceivedPacketInOneTick()
        {
            var hub = new NetworkHub();
            int calls = 0;
            hub.Subscribe("game", 9, _ => calls++);
            NetworkConnection connection = hub.Register("game", () => new FakeTransport(), new NetworkConnectionConfig());
            connection.InjectReceivedForTests(new NetworkPacket(9, 0, Array.Empty<byte>()));
            hub.Tick();
            Assert.AreEqual(1, calls);
        }

        [Test] public void HubRoutesRpcByEntityAndGroup()
        {
            var hub = new NetworkHub();
            int entityCalls = 0;
            int groupCalls = 0;
            hub.SubscribeRpc("game", 14, 1001, 9, _ => entityCalls++);
            hub.SubscribeRpcGroup("game", 1001, _ => groupCalls++);
            NetworkConnection connection = hub.Register("game", () => new FakeTransport(), new NetworkConnectionConfig());
            connection.InjectReceivedForTests(new NetworkPacket(14, 0, Array.Empty<byte>(), 1001, 8));
            hub.Tick();
            Assert.AreEqual(0, entityCalls);
            Assert.AreEqual(1, groupCalls);
            connection.InjectReceivedForTests(new NetworkPacket(14, 0, Array.Empty<byte>(), 1001, 9));
            hub.Tick();
            Assert.AreEqual(1, entityCalls);
            Assert.AreEqual(2, groupCalls);
        }

        [Test] public async Task ImmediateCommandsBypassTickQueue()
        {
            byte[] packet = PacketCodec.Encode(300, new byte[] { 1 }, false);
            var transport = new FakeTransport { ReceiveData = packet };
            var config = new NetworkConnectionConfig();
            config.ImmediateCommands.Add(300);
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), config);
            int calls = 0;
            connection.ImmediatePacketReceived += (_, __) => calls++;
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            Assert.AreEqual(1, calls);
        }

        [Test] public async Task ConnectionSendsVerifyPacketAndBlocksBusinessUntilVerified()
        {
            var transport = new FakeTransport();
            var config = new NetworkConnectionConfig
            {
                VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1, 2 }, false)
            };
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            Assert.AreEqual(1, transport.Sent.Count);
            Assert.AreEqual(100, transport.Sent[0].Command);
            try
            {
                await connection.Send(101, new byte[] { 3 });
                Assert.Fail("Business send before verify should fail.");
            }
            catch (InvalidOperationException)
            {
            }
            connection.MarkVerified();
            await connection.Send(101, new byte[] { 3 });
            Assert.AreEqual(2, transport.Sent.Count);
        }

        [Test] public async Task VerifyingReceiveFailureMovesToKickedInsteadOfReconnect()
        {
            var pump = new MainThreadEventPump();
            var transport = new FakeTransport { ThrowOnReceive = true };
            var connection = new NetworkConnection("game", transport, pump, new NetworkConnectionConfig());
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            pump.Tick();
            Assert.AreEqual(RealConnectionState.Kicked, connection.State);
        }

        [Test] public async Task HeartbeatSenderUsesConfiguredPacketAfterVerified()
        {
            var transport = new FakeTransport();
            var config = new NetworkConnectionConfig
            {
                HeartbeatInterval = TimeSpan.FromMilliseconds(100),
                HeartbeatPacketFactory = () => new NetworkOutboundPacket(200, new byte[] { 7 }, false)
            };
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            Assert.IsTrue(await connection.SendHeartbeatIfDue(100));
            Assert.IsFalse(await connection.SendHeartbeatIfDue(150));
            Assert.IsTrue(await connection.SendHeartbeatIfDue(200));
            Assert.AreEqual(2, transport.Sent.Count);
            Assert.AreEqual(200, transport.Sent[0].Command);
        }

        private sealed class FakeRecoveryService : ISessionRecoveryService
        {
            public int Calls;
            public async Task<ReconnectSessionData> RecoverAsync(string socketName, ReconnectSessionData current, CancellationToken cancellationToken)
            {
                Calls++;
                await Task.Yield();
                return new ReconnectSessionData("x", 1, "new", true);
            }
        }


        private sealed class PartialSender : IAsyncByteSender
        {
            public int Calls;
            public int Total;
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
            {
                Calls++;
                int sent = Math.Min(2, count);
                Total += sent;
                return Task.FromResult(sent);
            }
        }

        private sealed class FakeTransport : INetworkTransport
        {
            public sealed class SentPacket
            {
                public int Command;
                public byte[] Payload;
            }

            public readonly System.Collections.Generic.List<SentPacket> Sent = new System.Collections.Generic.List<SentPacket>();
            public bool ThrowOnReceive;
            public byte[] ReceiveData;
            private bool _received;
            public bool Connected { get; private set; }

            public Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
            {
                Connected = true;
                return Task.CompletedTask;
            }

            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
            {
                var reader = new PacketReader(1024);
                reader.Append(data, offset, count);
                Assert.IsTrue(reader.TryRead(out NetworkPacket packet));
                Sent.Add(new SentPacket { Command = packet.Command, Payload = packet.Payload });
                return Task.FromResult(count);
            }

            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
            {
                if (ThrowOnReceive) throw new InvalidOperationException("receive failed");
                if (ReceiveData != null && !_received)
                {
                    _received = true;
                    Buffer.BlockCopy(ReceiveData, 0, buffer, 0, ReceiveData.Length);
                    return Task.FromResult(ReceiveData.Length);
                }
                var source = new TaskCompletionSource<int>();
                cancellationToken.Register(() => source.TrySetCanceled());
                return source.Task;
            }

            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }
    }
}
