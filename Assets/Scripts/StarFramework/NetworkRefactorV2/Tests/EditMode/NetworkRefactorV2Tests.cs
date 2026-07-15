using System;
using System.Collections.Generic;
using System.Reflection;
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

        [Test] public void PacketReaderReadsStickyMultiplePackets()
        {
            byte[] first = PacketCodec.Encode(10, new byte[] { 1 }, false);
            byte[] second = PacketCodec.Encode(11, new byte[] { 2, 3 }, false);
            var sticky = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, sticky, 0, first.Length);
            Buffer.BlockCopy(second, 0, sticky, first.Length, second.Length);
            var reader = new PacketReader(1024);

            reader.Append(sticky, 0, sticky.Length);

            Assert.IsTrue(reader.TryRead(out NetworkPacket firstPacket));
            Assert.AreEqual(10, firstPacket.Command);
            CollectionAssert.AreEqual(new byte[] { 1 }, firstPacket.Payload);
            Assert.IsTrue(reader.TryRead(out NetworkPacket secondPacket));
            Assert.AreEqual(11, secondPacket.Command);
            CollectionAssert.AreEqual(new byte[] { 2, 3 }, secondPacket.Payload);
            Assert.IsFalse(reader.TryRead(out _));
        }

        [Test] public void PacketCodecMatchesLegacyPlainHeaderAndRejectsOversize()
        {
            CollectionAssert.AreEqual(new byte[] { 5, 0, 0, 0, 0x34, 0x12, 9, 8, 7 }, PacketCodec.Encode(0x1234, new byte[] { 9, 8, 7 }, false));
            var reader = new PacketReader(8);
            reader.Append(new byte[] { 100, 0, 0, 0, 1, 0 }, 0, 6);
            Assert.IsTrue(reader.IsCorrupt);
        }

        [Test] public void PacketReaderMarksCorruptWhenOversizePacketArrivesInOneChunk()
        {
            byte[] oversize = PacketCodec.Encode(1, new byte[32], false);
            var reader = new PacketReader(16);

            reader.Append(oversize, 0, oversize.Length);

            Assert.IsTrue(reader.IsCorrupt);
            Assert.IsFalse(reader.TryRead(out _));
        }

        [Test] public void PacketReaderAllowsStickyPacketsWhoseTotalExceedsMaxPacketSize()
        {
            byte[] first = PacketCodec.Encode(10, new byte[] { 1, 2, 3, 4 }, false);
            byte[] second = PacketCodec.Encode(11, new byte[] { 5, 6, 7, 8 }, false);
            var sticky = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, sticky, 0, first.Length);
            Buffer.BlockCopy(second, 0, sticky, first.Length, second.Length);
            var reader = new PacketReader(12);

            reader.Append(sticky, 0, sticky.Length);

            Assert.IsFalse(reader.IsCorrupt);
            Assert.IsTrue(reader.TryRead(out NetworkPacket firstPacket));
            Assert.AreEqual(10, firstPacket.Command);
            Assert.IsTrue(reader.TryRead(out NetworkPacket secondPacket));
            Assert.AreEqual(11, secondPacket.Command);
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

        [Test] public void ArbiterPrefersFocusedSocketOverAlwaysVisibleSocket()
        {
            var arbiter = new GlobalUiArbiter();
            arbiter.Register("game", SocketUiVisibility.AlwaysVisible, UiContext.Lobby);
            arbiter.Register("battle", SocketUiVisibility.VisibleWhenFocused, UiContext.Battle);
            arbiter.SetContext(UiContext.Battle);

            arbiter.UpdateIntent("game", ConnectionUiIntent.ReturnLoginDialog);
            arbiter.UpdateIntent("battle", ConnectionUiIntent.RealReconnectDialog);

            Assert.AreEqual("battle", arbiter.CurrentIntent.SocketName);
            Assert.AreEqual(ConnectionUiIntent.RealReconnectDialog, arbiter.CurrentIntent.Intent);
            arbiter.SetContext(UiContext.Lobby);
            Assert.AreEqual("game", arbiter.CurrentIntent.SocketName);
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

        [Test] public void RecoveryCoordinatorHonorsCanceledCallerWhileRecoveryIsPending()
        {
            var service = new BlockingRecoveryService();
            var coordinator = new ConnectionRecoveryCoordinator(service);
            var current = new ReconnectSessionData("x", 1, "old", false);
            Task<ReconnectSessionData> pending = coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, current, CancellationToken.None);
            var canceled = new CancellationToken(true);

            Task<ReconnectSessionData> canceledRequest = coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, current, canceled);

            Assert.IsTrue(canceledRequest.IsCanceled);
            Assert.IsFalse(pending.IsCompleted);
            service.Complete();
        }

        [Test] public async Task RecoveryCoordinatorReleasesPendingAfterFailure()
        {
            var service = new FlakyRecoveryService();
            var coordinator = new ConnectionRecoveryCoordinator(service);
            var current = new ReconnectSessionData("x", 1, "old", false);

            await ThrowsAsync<InvalidOperationException>(async () =>
                await coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, current, CancellationToken.None));
            ReconnectSessionData recovered = await coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, current, CancellationToken.None);

            Assert.AreEqual(2, service.Calls);
            Assert.AreEqual("new", recovered.Token);
        }

        [Test] public async Task RecoveryCoordinatorDoesNotDeduplicateDifferentSockets()
        {
            var service = new FakeRecoveryService();
            var coordinator = new ConnectionRecoveryCoordinator(service);
            var current = new ReconnectSessionData("x", 1, "old", false);

            await Task.WhenAll(
                coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, current, CancellationToken.None),
                coordinator.RecoverAsync("battle", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, current, CancellationToken.None));

            Assert.AreEqual(2, service.Calls);
        }

        [Test] public async Task RecoveryCoordinatorReusesCurrentCredentialWithoutCallingService()
        {
            var service = new FakeRecoveryService();
            var coordinator = new ConnectionRecoveryCoordinator(service);
            var current = new ReconnectSessionData("127.0.0.1:1", 1, "old", false);

            ReconnectSessionData recovered = await coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.ReuseCurrentCredential, current, CancellationToken.None);

            Assert.AreSame(current, recovered);
            Assert.AreEqual(0, service.Calls);
        }

        [Test] public async Task RecoveryCoordinatorManualOnlyDoesNotCallService()
        {
            var service = new FakeRecoveryService();
            var coordinator = new ConnectionRecoveryCoordinator(service);
            var current = new ReconnectSessionData("127.0.0.1:1", 1, "old", false);

            await ThrowsAsync<InvalidOperationException>(async () =>
                await coordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.ManualOnly, current, CancellationToken.None));
            Assert.AreEqual(0, service.Calls);
        }

        [Test] public async Task HubDeduplicatesConcurrentRecoverAndReconnect()
        {
            var transport = new CountingTransport();
            var hub = new NetworkHub();
            var config = new NetworkConnectionConfig
            {
                RecoveryPolicy = RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect,
                ReconnectDelay = TimeSpan.Zero
            };
            NetworkConnection connection = hub.Register("game", () => transport, config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            connection.MarkVerified();
            var coordinator = new ConnectionRecoveryCoordinator(new FakeRecoveryService());
            var current = new ReconnectSessionData("127.0.0.1:1", 1, "old", true);

            await Task.WhenAll(
                hub.RecoverAndReconnect("game", current, coordinator, CancellationToken.None),
                hub.RecoverAndReconnect("game", current, coordinator, CancellationToken.None));

            Assert.AreEqual(2, transport.ConnectCalls);
        }

        [Test] public async Task HubRecoverAndReconnectHonorsCanceledCallerWhileRecoveryIsPending()
        {
            var transport = new CountingTransport();
            var hub = new NetworkHub();
            NetworkConnection connection = hub.Register("game", () => transport, new NetworkConnectionConfig
            {
                RecoveryPolicy = RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect,
                ReconnectDelay = TimeSpan.Zero
            });
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            connection.MarkVerified();
            var service = new BlockingRecoveryService();
            var coordinator = new ConnectionRecoveryCoordinator(service);
            var current = new ReconnectSessionData("127.0.0.1:1", 1, "old", true);
            Task pending = hub.RecoverAndReconnect("game", current, coordinator, CancellationToken.None);

            Task canceled = hub.RecoverAndReconnect("game", current, coordinator, new CancellationToken(true));

            Assert.IsTrue(canceled.IsCanceled);
            Assert.IsFalse(pending.IsCompleted);
            service.Complete();
            await pending;
        }

        [Test] public async Task HubMarksReconnectFailedWhenRecoveryFails()
        {
            var transport = new CountingTransport();
            var hub = new NetworkHub();
            var config = new NetworkConnectionConfig
            {
                RecoveryPolicy = RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect,
                ReconnectDelay = TimeSpan.Zero
            };
            NetworkConnection connection = hub.Register("game", () => transport, config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            connection.MarkVerified();

            var coordinator = new ConnectionRecoveryCoordinator(new FailingRecoveryService());
            var current = new ReconnectSessionData("127.0.0.1:1", 1, "old", true);
            try
            {
                await hub.RecoverAndReconnect("game", current, coordinator, CancellationToken.None);
                Assert.Fail("Recovery failure should be visible to caller.");
            }
            catch (InvalidOperationException)
            {
            }

            Assert.AreEqual(RealConnectionState.ReconnectFailed, connection.State);
            Assert.IsFalse(transport.Connected);
        }

        [Test] public async Task ReconnectFailedDropsOldReceiveLoopPacket()
        {
            var pump = new MainThreadEventPump();
            var connection = new NetworkConnection(
                "game",
                new FakeTransport { ReceiveData = PacketCodec.Encode(203, new byte[] { 1 }, false) },
                pump,
                new NetworkConnectionConfig());
            int calls = 0;
            connection.PacketReceived += (_, __) => calls++;

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            await Task.Yield();
            connection.MarkReconnectFailed();
            pump.Tick();

            Assert.AreEqual(RealConnectionState.ReconnectFailed, connection.State);
            Assert.AreEqual(0, calls);
        }

        [Test] public async Task ReconnectAttemptExhaustionDropsOldReceiveLoopPacket()
        {
            var pump = new MainThreadEventPump();
            var connection = new NetworkConnection(
                "game",
                new FakeTransport { ReceiveData = PacketCodec.Encode(204, new byte[] { 1 }, false) },
                pump,
                new NetworkConnectionConfig { MaxReconnectAttempts = 0, MaxTotalReconnectAttempts = 1, ReconnectDelay = TimeSpan.Zero });
            int calls = 0;
            connection.PacketReceived += (_, __) => calls++;

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            await Task.Yield();
            await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            pump.Tick();

            Assert.AreEqual(RealConnectionState.ReconnectFailed, connection.State);
            Assert.AreEqual(0, calls);
        }

        [Test] public async Task KickedConnectionDoesNotRecoverAndReconnect()
        {
            var transport = new CountingTransport();
            var hub = new NetworkHub();
            NetworkConnection connection = hub.Register("game", () => transport, new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero });
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerifyFailed();

            await ThrowsAsync<InvalidOperationException>(async () =>
                await hub.RecoverAndReconnect("game", new ReconnectSessionData("127.0.0.1:1", 1, "token", true), new ConnectionRecoveryCoordinator(new FakeRecoveryService()), CancellationToken.None));
            Assert.AreEqual(RealConnectionState.Kicked, connection.State);
            Assert.AreEqual(1, transport.ConnectCalls);
        }

        [Test] public async Task ReconnectWithAlreadyCanceledTokenDoesNotMutateConnectedConnection()
        {
            var transport = new FakeTransport();
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero });
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();

            await ThrowsAsync<OperationCanceledException>(async () =>
                await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), new CancellationToken(true)));

            Assert.AreEqual(RealConnectionState.Connected, connection.State);
            Assert.IsTrue(transport.Connected);
        }

        [Test] public async Task ReconnectConnectFailureMovesToReconnectFailedAndClosesTransport()
        {
            var transport = new FakeTransport();
            var connection = new NetworkConnection(
                "game",
                transport,
                new MainThreadEventPump(),
                new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero });
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            transport.ThrowOnConnect = true;

            await ThrowsAsync<InvalidOperationException>(async () =>
                await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None));

            Assert.AreEqual(RealConnectionState.ReconnectFailed, connection.State);
            Assert.IsFalse(transport.Connected);
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

        [Test] public async Task ConnectionSerializesConcurrentSends()
        {
            var transport = new SlowSendTransport();
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), new NetworkConnectionConfig());
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();

            await Task.WhenAll(
                connection.Send(101, new byte[] { 1 }),
                connection.Send(102, new byte[] { 2 }));

            Assert.AreEqual(1, transport.MaxConcurrentSends);
            CollectionAssert.AreEqual(new[] { 101, 102 }, transport.Commands);
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

        [Test] public void HubDispatchesNestedReceivedPacketInSameTick()
        {
            var hub = new NetworkHub();
            var received = new List<int>();
            hub.Subscribe("game", 9, _ =>
            {
                received.Add(9);
                hub.EnqueueReceived("game", new NetworkPacket(10, 0, Array.Empty<byte>()));
            });
            hub.Subscribe("game", 10, _ => received.Add(10));

            hub.EnqueueReceived("game", new NetworkPacket(9, 0, Array.Empty<byte>()));
            hub.Tick();

            CollectionAssert.AreEqual(new[] { 9, 10 }, received);
        }

        [Test] public void HubLocksNormalMessagesUntilUnlocked()
        {
            var hub = new NetworkHub();
            var received = new List<int>();
            hub.Subscribe("game", 9, packet => received.Add(packet.Payload[0]));
            hub.LockMessages(true);

            hub.EnqueueReceived("game", new NetworkPacket(9, 0, new byte[] { 1 }));
            hub.EnqueueReceived("game", new NetworkPacket(9, 0, new byte[] { 2 }));
            hub.Tick();
            Assert.AreEqual(0, received.Count);
            Assert.IsTrue(hub.IsMessageLocked);

            hub.LockMessages(false);
            hub.Tick();
            CollectionAssert.AreEqual(new[] { 1, 2 }, received);
            Assert.IsFalse(hub.IsMessageLocked);
        }

        [Test] public void HubFlushesUnlockedMessagesEvenWhenTickCallbackThrows()
        {
            var hub = new NetworkHub();
            int flushed = 0;
            hub.Subscribe("game", 1, _ => throw new InvalidOperationException("callback failed"));
            hub.Subscribe("game", 2, _ => flushed++);
            hub.LockMessages(true);
            hub.EnqueueReceived("game", new NetworkPacket(2, 0, Array.Empty<byte>()));
            hub.Tick();
            hub.LockMessages(false);

            hub.EnqueueReceived("game", new NetworkPacket(1, 0, Array.Empty<byte>()));

            Assert.Throws<InvalidOperationException>(() => hub.Tick());
            Assert.AreEqual(1, flushed);
        }

        [Test] public void LuaApiUnsubscribesByHandleAndOwner()
        {
            var hub = new NetworkHub();
            var api = new LuaNetworkApi(hub);
            object owner = new object();
            int handleCalls = 0;
            int ownerCalls = 0;
            long handle = api.Subscribe("game", 9, _ => handleCalls++);
            api.Subscribe("game", 9, _ => ownerCalls++, owner);

            api.Unsubscribe(handle);
            api.UnsubscribeOwner(owner);
            hub.EnqueueReceived("game", new NetworkPacket(9, 0, Array.Empty<byte>()));
            hub.Tick();

            Assert.AreEqual(0, handleCalls);
            Assert.AreEqual(0, ownerCalls);
        }

        [Test] public void LuaApiUnsubscribeOwnerClearsOwnedHandles()
        {
            var hub = new NetworkHub();
            var api = new LuaNetworkApi(hub);
            object owner = new object();
            api.Subscribe("game", 9, _ => { }, owner);

            api.UnsubscribeOwner(owner);

            var handles = (System.Collections.IDictionary)typeof(LuaNetworkApi)
                .GetField("_handles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(api);
            Assert.AreEqual(0, handles.Count);
        }

        [Test] public void NetworkApiReleaseIsIdempotentAcrossOwnerAndHandle()
        {
            var luaHub = new NetworkHub();
            var luaApi = new LuaNetworkApi(luaHub);
            object luaOwner = new object();
            int luaCalls = 0;
            long luaHandle = luaApi.Subscribe("game", 9, _ => luaCalls++, luaOwner);

            luaApi.UnsubscribeOwner(luaOwner);
            luaApi.UnsubscribeOwner(luaOwner);
            luaApi.Unsubscribe(luaHandle);
            luaApi.Unsubscribe(luaHandle);
            luaHub.EnqueueReceived("game", new NetworkPacket(9, 0, Array.Empty<byte>()));
            luaHub.Tick();

            var csharpHub = new NetworkHub();
            var csharpApi = new CSharpNetworkApi(csharpHub);
            object csharpOwner = new object();
            int csharpCalls = 0;
            IDisposable token = csharpApi.Subscribe("game", 9, _ => csharpCalls++, csharpOwner);

            csharpApi.UnsubscribeOwner(csharpOwner);
            csharpApi.UnsubscribeOwner(csharpOwner);
            token.Dispose();
            token.Dispose();
            csharpHub.EnqueueReceived("game", new NetworkPacket(9, 0, Array.Empty<byte>()));
            csharpHub.Tick();

            Assert.AreEqual(0, luaCalls);
            Assert.AreEqual(0, csharpCalls);
        }

        [Test] public async Task LuaApiSendsRpcEnvelope()
        {
            var transport = new FakeTransport();
            var hub = new NetworkHub();
            NetworkConnection connection = hub.Register("game", () => transport, new NetworkConnectionConfig());
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            var api = new LuaNetworkApi(hub);

            await api.SendRpc("game", 203, 0x1234, new byte[] { 9 });

            Assert.AreEqual(1, transport.Sent.Count);
            Assert.AreEqual(203, transport.Sent[0].Command);
            CollectionAssert.AreEqual(new byte[] { 14, 0x34, 0x12, 1, 0, 9 }, transport.Sent[0].Payload);
        }

        [Test] public void CSharpSubscriptionsUnsubscribeByTokenAndOwner()
        {
            var hub = new NetworkHub();
            var api = new CSharpNetworkApi(hub);
            object owner = new object();
            int normalCalls = 0;
            int immediateCalls = 0;
            int rpcEntityCalls = 0;
            int rpcGroupCalls = 0;
            IDisposable normal = api.Subscribe("game", 9, _ => normalCalls++);
            IDisposable immediate = api.SubscribeImmediate("game", 10, _ => immediateCalls++);
            api.SubscribeRpc("game", 14, 1001, 7, _ => rpcEntityCalls++, owner);
            api.SubscribeRpcGroup("game", 1001, _ => rpcGroupCalls++, owner);

            normal.Dispose();
            immediate.Dispose();
            api.UnsubscribeOwner(owner);
            hub.EnqueueReceived("game", new NetworkPacket(9, 0, Array.Empty<byte>()));
            hub.EnqueueReceived("game", new NetworkPacket(14, 0, Array.Empty<byte>(), 1001, 7));
            hub.Tick();

            Assert.AreEqual(0, normalCalls);
            Assert.AreEqual(0, rpcEntityCalls);
            Assert.AreEqual(0, rpcGroupCalls);
            Assert.AreEqual(0, immediateCalls);
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

        [Test] public async Task HubImmediateCommandsUseImmediateHandlersWithoutNormalDispatch()
        {
            byte[] packet = PacketCodec.Encode(300, new byte[] { 1 }, false);
            var transport = new FakeTransport { ReceiveData = packet };
            var config = new NetworkConnectionConfig();
            config.ImmediateCommands.Add(300);
            var hub = new NetworkHub();
            int normalCalls = 0;
            int immediateCalls = 0;
            hub.Subscribe("game", 300, _ => normalCalls++);
            hub.SubscribeImmediate("game", 300, _ => immediateCalls++);
            NetworkConnection connection = hub.Register("game", () => transport, config);

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            Assert.AreEqual(1, immediateCalls);
            Assert.AreEqual(0, normalCalls);

            hub.Tick();
            Assert.AreEqual(0, normalCalls);
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

        [Test] public async Task CSharpApiSendsCustomProtobufAndRpcEnvelope()
        {
            var transport = new FakeTransport();
            var hub = new NetworkHub();
            NetworkConnection connection = hub.Register("game", () => transport, new NetworkConnectionConfig());
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            var api = new CSharpNetworkApi(hub);

            await api.SendCustom("game", 201, 7, value => new[] { (byte)value });
            await api.SendProtobuf("game", 202, 8, value => new[] { (byte)value });
            await api.SendRpc("game", 203, 0x1234, new byte[] { 9 });

            Assert.AreEqual(3, transport.Sent.Count);
            Assert.AreEqual(201, transport.Sent[0].Command);
            CollectionAssert.AreEqual(new byte[] { 7 }, transport.Sent[0].Payload);
            Assert.AreEqual(202, transport.Sent[1].Command);
            CollectionAssert.AreEqual(new byte[] { 8 }, transport.Sent[1].Payload);
            Assert.AreEqual(203, transport.Sent[2].Command);
            CollectionAssert.AreEqual(new byte[] { 14, 0x34, 0x12, 1, 0, 9 }, transport.Sent[2].Payload);
        }

        [Test] public async Task VerifySuccessPacketMovesConnectionToConnectedWithoutNormalDispatch()
        {
            byte[] verifySuccess = PacketCodec.Encode(2, new byte[] { 1 }, false);
            var transport = new FakeTransport { ReceiveData = verifySuccess };
            var config = new NetworkConnectionConfig
            {
                VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false),
                VerifyResultDecoder = packet => packet.Command == 2 ? (bool?)true : null
            };
            var pump = new MainThreadEventPump();
            var connection = new NetworkConnection("game", transport, pump, config);
            int normalCalls = 0;
            connection.PacketReceived += (_, __) => normalCalls++;

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            pump.Tick();

            Assert.AreEqual(RealConnectionState.Connected, connection.State);
            Assert.AreEqual(0, normalCalls);
        }

        [Test] public async Task VerifyFailurePacketMovesConnectionToKickedWithoutNormalDispatch()
        {
            byte[] verifyFailure = PacketCodec.Encode(3, new byte[] { 0 }, false);
            var transport = new FakeTransport { ReceiveData = verifyFailure };
            var config = new NetworkConnectionConfig
            {
                VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false),
                VerifyResultDecoder = packet => packet.Command == 3 ? (bool?)false : null
            };
            var pump = new MainThreadEventPump();
            var connection = new NetworkConnection("game", transport, pump, config);
            int normalCalls = 0;
            connection.PacketReceived += (_, __) => normalCalls++;

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            pump.Tick();

            Assert.AreEqual(RealConnectionState.Kicked, connection.State);
            Assert.IsFalse(transport.Connected);
            Assert.AreEqual(0, normalCalls);
        }

        [Test] public async Task VerifyTimeoutMovesToKickedAndClosesTransport()
        {
            var transport = new FakeTransport();
            var config = new NetworkConnectionConfig
            {
                VerifyTimeout = TimeSpan.FromMilliseconds(20),
                VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false)
            };
            var pump = new MainThreadEventPump();
            var connection = new NetworkConnection("game", transport, pump, config);

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Delay(60);
            pump.Tick();

            Assert.AreEqual(RealConnectionState.Kicked, connection.State);
            Assert.IsFalse(transport.Connected);
        }

        [Test] public async Task OldVerifyTimeoutDoesNotKickNewReconnectGeneration()
        {
            var transport = new FakeTransport();
            var config = new NetworkConnectionConfig
            {
                VerifyTimeout = TimeSpan.FromMilliseconds(20),
                ReconnectDelay = TimeSpan.Zero,
                VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false)
            };
            var pump = new MainThreadEventPump();
            var connection = new NetworkConnection("game", transport, pump, config);

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            await Task.Delay(60);
            await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "new", true), CancellationToken.None);
            pump.Tick();

            Assert.AreEqual(RealConnectionState.Verifying, connection.State);
            Assert.IsTrue(transport.Connected);
        }

        [Test] public async Task VerifyPacketSendFailureMovesToKickedAndClosesTransport()
        {
            var transport = new FakeTransport { ThrowOnSend = true };
            var config = new NetworkConnectionConfig
            {
                VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false)
            };
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), config);

            await ThrowsAsync<InvalidOperationException>(async () =>
                await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None));

            Assert.AreEqual(RealConnectionState.Kicked, connection.State);
            Assert.IsFalse(transport.Connected);
        }

        [Test] public async Task CloseDuringVerifySendKeepsClosedState()
        {
            var transport = new BlockingSendTransport();
            var pump = new MainThreadEventPump();
            var config = new NetworkConnectionConfig
            {
                VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false)
            };
            var connection = new NetworkConnection("game", transport, pump, config);
            var states = new List<RealConnectionState>();
            connection.StateChanged += (_, state) => states.Add(state);

            Task startTask = connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await transport.SendStarted.Task;
            connection.Close();
            pump.Tick();

            await ThrowsAsync<OperationCanceledException>(async () => await startTask);
            Assert.AreEqual(RealConnectionState.Closed, connection.State);
            CollectionAssert.DoesNotContain(states, RealConnectionState.Kicked);
            Assert.IsFalse(transport.Connected);
        }

        [Test] public async Task InitialConnectFailureMovesToReconnectFailedAndClosesTransport()
        {
            var transport = new FakeTransport { ThrowOnConnect = true };
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), new NetworkConnectionConfig());

            await ThrowsAsync<InvalidOperationException>(async () =>
                await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None));

            Assert.AreEqual(RealConnectionState.ReconnectFailed, connection.State);
            Assert.IsFalse(transport.Connected);
        }

        [Test] public async Task StartWithAlreadyCanceledTokenDoesNotMutateDisconnectedConnection()
        {
            var transport = new FakeTransport();
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), new NetworkConnectionConfig());

            await ThrowsAsync<OperationCanceledException>(async () =>
                await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), new CancellationToken(true)));

            Assert.AreEqual(RealConnectionState.Disconnected, connection.State);
            Assert.IsFalse(transport.Connected);
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

        [Test] public async Task HeartbeatSendFailureIsVisibleAndDoesNotAdvanceInterval()
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
            transport.ThrowOnSend = true;

            await ThrowsAsync<InvalidOperationException>(async () => await connection.SendHeartbeatIfDue(100));

            transport.ThrowOnSend = false;
            int beforeRetry = transport.Sent.Count;
            Assert.IsTrue(await connection.SendHeartbeatIfDue(100));
            Assert.AreEqual(beforeRetry + 1, transport.Sent.Count);
        }

        [Test] public async Task HeartbeatPongUpdatesConnectionPingWithoutNormalDispatch()
        {
            byte[] pong = PacketCodec.Encode(200, new byte[] { 1 }, false);
            var transport = new FakeTransport { ReceiveData = pong };
            var pump = new MainThreadEventPump();
            var config = new NetworkConnectionConfig
            {
                HeartbeatPongDecoder = packet => packet.Command == 200 ? (int?)PingHealthMonitor.WeakLatencyMilliseconds : null
            };
            var connection = new NetworkConnection("game", transport, pump, config);
            int normalCalls = 0;
            connection.PacketReceived += (_, __) => normalCalls++;

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            await Task.Yield();
            pump.Tick();

            Assert.AreEqual(PingHealthState.Healthy, connection.PingHealth.State);
            Assert.AreEqual(0, normalCalls);
            for (int i = 0; i < PingHealthMonitor.WeakSampleCount; i++)
            {
                connection.RecordHeartbeatPong(PingHealthMonitor.WeakLatencyMilliseconds);
            }
            Assert.AreEqual(PingHealthState.Weak, connection.PingHealth.State);
        }

        [Test] public async Task HeartbeatPongRefreshesLastPongTimeForAgeChecks()
        {
            var connection = new NetworkConnection(
                "pong-age",
                new FakeTransport(),
                new MainThreadEventPump(),
                new NetworkConnectionConfig { HeartbeatPongDecoder = packet => packet.Command == 200 ? (int?)10 : null });

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            connection.RecordHeartbeatPong(10, 1000);

            Assert.AreEqual(PingHealthState.Healthy, connection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds - 1));
            Assert.AreEqual(PingHealthState.HeartbeatLost, connection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds));
        }

        [Test] public void ConnectionDetectsHeartbeatLostFromLastPongTime()
        {
            var connection = new NetworkConnection("game", new FakeTransport(), new MainThreadEventPump(), new NetworkConnectionConfig());
            connection.RecordHeartbeatPong(10, 1000);
            Assert.AreEqual(PingHealthState.Healthy, connection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds - 1));
            Assert.AreEqual(PingHealthState.HeartbeatLost, connection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds));
        }

        [Test] public async Task ReceiveLoopDecodesRpcPacketForEntityAndGroupDispatch()
        {
            int rpcCommand = 14;
            int messageCommand = 1001;
            byte[] rpcPayload = RpcCodec.Encode(messageCommand, new byte[] { 1, 2, 3 });
            byte[] wireBytes = PacketCodec.Encode(rpcCommand, rpcPayload, false);

            var hub = new NetworkHub();
            int entityCalls = 0;
            int groupCalls = 0;
            hub.SubscribeRpc("game", rpcCommand, messageCommand, 0, _ => entityCalls++);
            hub.SubscribeRpcGroup("game", messageCommand, _ => groupCalls++);

            var config = new NetworkConnectionConfig { RpcCommand = rpcCommand };
            NetworkConnection connection = hub.Register("game", () => new FakeTransport { ReceiveData = wireBytes }, config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            hub.Tick();
            Assert.AreEqual(1, entityCalls, "entity-specific RPC dispatch");
            Assert.AreEqual(1, groupCalls, "group RPC dispatch");
        }

        [Test] public async Task RpcDecoderOverrideExtractsEntityId()
        {
            int rpcCommand = 14;
            int messageCommand = 1002;
            ulong entityId = 42;
            byte[] innerPayload = new byte[] { 9 };
            byte[] rpcPayload = RpcCodec.Encode(messageCommand, innerPayload);
            byte[] wireBytes = PacketCodec.Encode(rpcCommand, rpcPayload, false);

            var hub = new NetworkHub();
            int entityCalls = 0;
            hub.SubscribeRpc("game", rpcCommand, messageCommand, entityId, _ => entityCalls++);

            var config = new NetworkConnectionConfig
            {
                RpcCommand = rpcCommand,
                RpcDecoder = packet => packet.Command == rpcCommand
                    ? new NetworkPacket(packet.Command, packet.SerializeType, innerPayload, messageCommand, entityId)
                    : null
            };
            NetworkConnection connection = hub.Register("game", () => new FakeTransport { ReceiveData = wireBytes }, config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            hub.Tick();
            Assert.AreEqual(1, entityCalls, "RpcDecoder extracts entityId for entity-specific dispatch");
        }

        #region Bug-fix 回归测试（NetworkRefactorV2 计划：4 个内部 bug）

        // 原始网络层语义：验证失败/被踢直接返回登录，不进入自动重连流程。
        [Test] public void StateMachineRejectsKickedToReconnect()
        {
            var state = new ConnectionStateMachine();
            state.MoveTo(RealConnectionState.Connecting);
            state.MoveTo(RealConnectionState.Verifying);
            state.MoveTo(RealConnectionState.Kicked);
            Assert.Throws<InvalidOperationException>(() => state.MoveTo(RealConnectionState.Reconnecting));
        }

        [Test] public void StateMachineRejectsKickedToConnectingAndReconnectFailed()
        {
            var state = new ConnectionStateMachine();
            state.MoveTo(RealConnectionState.Connecting);
            state.MoveTo(RealConnectionState.Verifying);
            state.MoveTo(RealConnectionState.Kicked);
            Assert.Throws<InvalidOperationException>(() => state.MoveTo(RealConnectionState.Connecting));
            Assert.Throws<InvalidOperationException>(() => state.MoveTo(RealConnectionState.ReconnectFailed));
        }

        // BUG A：MarkVerified 必须重置心跳基线（_lastHeartbeatMilliseconds = long.MinValue），
        // 否则重连成功后会跳过首个心跳，Ping 健康度异常。
        [Test] public async Task MarkVerifiedResetsHeartbeatBaseline()
        {
            var transport = new FakeTransport();
            var config = new NetworkConnectionConfig
            {
                HeartbeatInterval = TimeSpan.FromMilliseconds(1000),
                HeartbeatPacketFactory = () => new NetworkOutboundPacket(200, new byte[] { 7 }, false)
            };
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            Assert.IsTrue(await connection.SendHeartbeatIfDue(100));   // 首次心跳到期
            Assert.IsFalse(await connection.SendHeartbeatIfDue(150)); // 间隔内未到期
            connection.MarkVerified();                                // 重置基线
            Assert.IsTrue(await connection.SendHeartbeatIfDue(200));   // 重置后再次到期
        }

        [Test] public async Task MarkVerifiedResetsPingHealth()
        {
            var connection = new NetworkConnection("game", new FakeTransport(), new MainThreadEventPump(), new NetworkConnectionConfig());
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();

            connection.RecordHeartbeatPong(10, 1000);
            Assert.AreEqual(PingHealthState.HeartbeatLost, connection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds));

            connection.MarkVerified();

            Assert.AreEqual(PingHealthState.Healthy, connection.PingHealth.State);
            Assert.AreEqual(PingHealthState.Healthy, connection.CheckHeartbeatAge(long.MaxValue));
        }

        // BUG B：ReconnectWith 必须先建 _lifetime 并用其 token 驱动延迟，Close() 才能可靠中断重连延迟，
        // 否则延迟期间 Close 无法取消，重连会空等 ReconnectDelay。
        [Test] public async Task ReconnectWithDelayIsInterruptedByClose()
        {
            var transport = new FakeTransport();
            var config = new NetworkConnectionConfig
            {
                ReconnectDelay = TimeSpan.FromSeconds(5),
                VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1, 2 }, false),
                HeartbeatPacketFactory = () => new NetworkOutboundPacket(200, new byte[] { 7 }, false)
            };
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            Task reconnect = connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            await Task.Yield();
            // 延迟期间关闭：Close() 取消 _lifetime，应立即中断重连延迟（而非阻塞 5 秒）
            connection.Close();
            await Task.WhenAny(reconnect, Task.Delay(TimeSpan.FromSeconds(1)));
            AggregateException ignoredException = reconnect.Exception; // 观察可能的取消异常，避免未观测任务警告
            Assert.IsTrue(reconnect.IsCompleted, "Close 必须中断 ReconnectWith 的延迟等待");
        }

        [Test] public async Task ReconnectDropsPacketsPostedByOldReceiveLoop()
        {
            var transport = new FakeTransport { ReceiveData = PacketCodec.Encode(201, new byte[] { 1 }, false) };
            var pump = new MainThreadEventPump();
            var config = new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero };
            var connection = new NetworkConnection("game", transport, pump, config);
            int calls = 0;
            connection.PacketReceived += (_, __) => calls++;

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            pump.Tick();

            Assert.AreEqual(0, calls);
        }

        [Test] public async Task ReconnectDropsHeartbeatPongReturnedByOldReceiveLoopAfterCancel()
        {
            var transport = new DelayedReceiveTransport(PacketCodec.Encode(200, new byte[] { 1 }, false));
            var config = new NetworkConnectionConfig
            {
                ReconnectDelay = TimeSpan.Zero,
                HeartbeatPongDecoder = packet => packet.Command == 200 ? (int?)10 : null
            };
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), config);

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            connection.MarkVerified();
            await Task.Yield();
            await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "new", true), CancellationToken.None);
            transport.ReleaseFirstReceive();
            await Task.Yield();

            Assert.AreEqual(PingHealthState.Healthy, connection.CheckHeartbeatAge(long.MaxValue));
        }

        [Test] public async Task CloseDropsPacketsPostedByReceiveLoop()
        {
            var transport = new FakeTransport { ReceiveData = PacketCodec.Encode(202, new byte[] { 1 }, false) };
            var pump = new MainThreadEventPump();
            var connection = new NetworkConnection("game", transport, pump, new NetworkConnectionConfig());
            int calls = 0;
            connection.PacketReceived += (_, __) => calls++;

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            connection.Close();
            pump.Tick();

            Assert.AreEqual(RealConnectionState.Closed, connection.State);
            Assert.AreEqual(0, calls);
        }

        // BUG D：PacketReader 损坏（声明长度超过 maxPacketSize）应置 IsCorrupt 而非抛异常；
        // ReceiveLoop 据此迁移到 Reconnecting 而非崩溃。
        [Test] public void PacketReaderMarksCorruptOnOversize()
        {
            var reader = new PacketReader(16);
            reader.Append(new byte[] { 0x00, 0x01, 0x00, 0, 0, 0 }, 0, 6); // 长度 260 > 16 -> 损坏
            Assert.IsTrue(reader.IsCorrupt);
            Assert.IsFalse(reader.TryRead(out _), "损坏时 TryRead 不应抛异常，仅返回 false");
        }

        [Test] public void PacketReaderReturnsFalseOnShortDeclaredLength()
        {
            var reader = new PacketReader(16);
            reader.Append(new byte[] { 0, 0, 0, 0, 1, 0 }, 0, 6); // 声明长度 4 < 包头 6 -> 损坏
            Assert.IsTrue(reader.IsCorrupt);
            Assert.IsFalse(reader.TryRead(out _), "损坏时 TryRead 不应抛异常，仅返回 false");
        }

        [Test] public async Task CorruptPacketMovesConnectionToReconnecting()
        {
            byte[] corrupt = { 0x00, 0x01, 0x00, 0, 0, 0 }; // 长度 260 > maxPacketSize 16 -> 损坏
            var transport = new FakeTransport { ReceiveData = corrupt };
            var pump = new MainThreadEventPump();
            var config = new NetworkConnectionConfig { MaxPacketSize = 16 };
            var connection = new NetworkConnection("game", transport, pump, config);
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            pump.Tick();
            Assert.AreEqual(RealConnectionState.Reconnecting, connection.State);
        }

        [Test] public async Task ReconnectResetsPacketReaderAfterCorruptPacket()
        {
            var transport = new FakeTransport { ReceiveData = new byte[] { 100, 0, 0, 0, 1, 0 } };
            var pump = new MainThreadEventPump();
            var connection = new NetworkConnection("game", transport, pump, new NetworkConnectionConfig { MaxPacketSize = 64, ReconnectDelay = TimeSpan.Zero });
            int calls = 0;
            connection.PacketReceived += (_, packet) => { if (packet.Command == 202) calls++; };

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            pump.Tick();
            Assert.AreEqual(RealConnectionState.Reconnecting, connection.State);

            transport.SetReceiveData(PacketCodec.Encode(202, new byte[] { 1 }, false));
            await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            await Task.Yield();
            pump.Tick();

            Assert.AreEqual(1, calls);
        }

        #endregion

        #region 阶段2/3 回归测试（B1 发送缓存 / B2 异常解锁 / B4 动态心跳 / B6 重连中间状态 / Phase3 编码优化）

        [Test] public void B1_SendCachesWhenNotConnected()
        {
            var transport = new FakeTransport();
            var events = new MainThreadEventPump();
            var conn = new NetworkConnection("test", transport, events);
            // Disconnected 状态发送应缓存不抛异常
            Assert.DoesNotThrow(() => conn.Send(100, new byte[] { 1, 2 }, false).Wait());
            Assert.AreEqual(0, transport.Sent.Count);
        }

        [Test] public void B1_SendThrowsOnKicked()
        {
            var transport = new FakeTransport();
            var events = new MainThreadEventPump();
            var conn = new NetworkConnection("test", transport, events);
            conn.Start(new ReconnectSessionData("host:1", 1, "t", false)).Wait();
            events.Tick();
            conn.MarkVerifyFailed();
            Assert.Throws<InvalidOperationException>(() => conn.Send(100, new byte[] { 1 }, false));
        }

        [Test] public void B2_EventPumpIsolatesCallbackException()
        {
            var pump = new MainThreadEventPump();
            int call1 = 0, call2 = 0;
            pump.Post(() => { call1++; throw new InvalidOperationException("boom"); });
            pump.Post(() => { call2++; });
            Assert.DoesNotThrow(() => pump.Tick());
            Assert.AreEqual(1, call1);
            Assert.AreEqual(1, call2);
        }

        [Test] public void B6_ReconnectRetryableStateExistsInStateMachine()
        {
            var sm = new ConnectionStateMachine();
            sm.MoveTo(RealConnectionState.Connecting);
            sm.MoveTo(RealConnectionState.Reconnecting);
            sm.MoveTo(RealConnectionState.ReconnectRetryable);
            Assert.AreEqual(RealConnectionState.ReconnectRetryable, sm.State);
            sm.MoveTo(RealConnectionState.Connecting);
            Assert.AreEqual(RealConnectionState.Connecting, sm.State);
        }

        [Test] public void B6_ReconnectRetryableCanTransitionToReconnectFailed()
        {
            var sm = new ConnectionStateMachine();
            sm.MoveTo(RealConnectionState.Connecting);
            sm.MoveTo(RealConnectionState.Reconnecting);
            sm.MoveTo(RealConnectionState.ReconnectRetryable);
            sm.MoveTo(RealConnectionState.ReconnectFailed);
            Assert.AreEqual(RealConnectionState.ReconnectFailed, sm.State);
        }

        [Test] public void B6_UiPolicyMapsReconnectRetryableToRetryDialog()
        {
            ConnectionUiIntent intent = ConnectionUiPolicy.Evaluate(RealConnectionState.ReconnectRetryable, PingHealthState.Healthy);
            Assert.AreEqual(ConnectionUiIntent.ReconnectRetryDialog, intent);
        }

        [Test] public void B4_DynamicHeartbeatIntervalConfigSupported()
        {
            var config = new NetworkConnectionConfig();
            Assert.IsNull(config.DynamicHeartbeatInterval);
            config.DynamicHeartbeatInterval = state => state == PingHealthState.Healthy ? TimeSpan.FromSeconds(1) : TimeSpan.FromMilliseconds(50);
            Assert.IsNotNull(config.DynamicHeartbeatInterval);
            Assert.AreEqual(TimeSpan.FromMilliseconds(50), config.DynamicHeartbeatInterval(PingHealthState.Weak));
        }

        [Test] public void B6_MaxTotalReconnectAttemptsConfigExists()
        {
            var config = new NetworkConnectionConfig();
            Assert.AreEqual(5, config.MaxTotalReconnectAttempts);
            config.MaxTotalReconnectAttempts = 10;
            Assert.AreEqual(10, config.MaxTotalReconnectAttempts);
        }

        [Test] public async Task ReconnectAttemptExhaustionMovesToRetryableBeforeTotalLimit()
        {
            var transport = new FakeTransport();
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), new NetworkConnectionConfig
            {
                MaxReconnectAttempts = 0,
                MaxTotalReconnectAttempts = 2,
                ReconnectDelay = TimeSpan.Zero
            });
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();

            await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);

            Assert.AreEqual(RealConnectionState.ReconnectRetryable, connection.State);
        }

        [Test] public async Task TotalReconnectAttemptExhaustionMovesToReconnectFailed()
        {
            var transport = new FakeTransport();
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump(), new NetworkConnectionConfig
            {
                MaxReconnectAttempts = 0,
                MaxTotalReconnectAttempts = 2,
                ReconnectDelay = TimeSpan.Zero
            });
            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);

            await connection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);

            Assert.AreEqual(RealConnectionState.ReconnectFailed, connection.State);
        }

        [Test] public async Task CachedBusinessSendsFlushInOrderAfterVerify()
        {
            var transport = new FakeTransport();
            var connection = new NetworkConnection("game", transport, new MainThreadEventPump());
            await connection.Send(101, new byte[] { 1 }, false);
            await connection.Send(102, new byte[] { 2 }, false);

            await connection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            connection.MarkVerified();
            await Task.Yield();

            Assert.AreEqual(2, transport.Sent.Count);
            Assert.AreEqual(101, transport.Sent[0].Command);
            Assert.AreEqual(102, transport.Sent[1].Command);
        }

        [Test] public void CloseOrVerifyFailedClearsCachedBusinessSends()
        {
            var closeConnection = new NetworkConnection("close", new FakeTransport(), new MainThreadEventPump());
            closeConnection.Send(101, new byte[] { 1 }, false).Wait();
            closeConnection.Close();
            Assert.AreEqual(0, CachedSendCount(closeConnection));

            var verifyFailedConnection = new NetworkConnection("verify-failed", new FakeTransport(), new MainThreadEventPump());
            verifyFailedConnection.Send(102, new byte[] { 2 }, false).Wait();
            verifyFailedConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false)).Wait();
            verifyFailedConnection.MarkVerifyFailed();
            Assert.AreEqual(0, CachedSendCount(verifyFailedConnection));
        }

        [Test] public void Phase3_PacketCodecEncodeNoCloneWhenNotEncrypted()
        {
            byte[] payload = { 10, 20, 30 };
            byte[] encoded = PacketCodec.Encode(100, payload, false);
            Assert.AreEqual(PacketCodec.HeaderSize + payload.Length, encoded.Length);
            Assert.AreEqual(10, payload[0]);
            Assert.AreEqual(20, payload[1]);
            Assert.AreEqual(30, payload[2]);
        }

        [Test] public void Phase3_PacketCodecEncodeEncryptStillWorks()
        {
            byte[] payload = { 10, 20, 30 };
            byte[] encoded = PacketCodec.Encode(100, payload, true);
            Assert.AreEqual(PacketCodec.HeaderSize + payload.Length, encoded.Length);
            Assert.AreEqual(2, encoded[3]); // serializeType = 2 (encrypted)
            // 原始 payload 不应被修改
            Assert.AreEqual(10, payload[0]);
        }

        #endregion

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

        private sealed class BlockingRecoveryService : ISessionRecoveryService
        {
            private readonly TaskCompletionSource<ReconnectSessionData> _source = new TaskCompletionSource<ReconnectSessionData>();
            public Task<ReconnectSessionData> RecoverAsync(string socketName, ReconnectSessionData current, CancellationToken cancellationToken)
            {
                cancellationToken.Register(() => _source.TrySetCanceled());
                return _source.Task;
            }
            public void Complete() { _source.TrySetResult(new ReconnectSessionData("x", 1, "new", true)); }
        }

        private sealed class FlakyRecoveryService : ISessionRecoveryService
        {
            public int Calls;
            public Task<ReconnectSessionData> RecoverAsync(string socketName, ReconnectSessionData current, CancellationToken cancellationToken)
            {
                Calls++;
                if (Calls == 1) throw new InvalidOperationException("refresh failed");
                return Task.FromResult(new ReconnectSessionData(current.SocketAddress, current.Pid, "new", true));
            }
        }

        private sealed class FailingRecoveryService : ISessionRecoveryService
        {
            public async Task<ReconnectSessionData> RecoverAsync(string socketName, ReconnectSessionData current, CancellationToken cancellationToken)
            {
                await Task.Yield();
                throw new InvalidOperationException("refresh failed");
            }
        }

        private static int CachedSendCount(NetworkConnection connection)
        {
            var field = typeof(NetworkConnection).GetField("_sendCache", BindingFlags.Instance | BindingFlags.NonPublic);
            var queue = (System.Collections.ICollection)field.GetValue(connection);
            return queue.Count;
        }

        private static async Task ThrowsAsync<T>(Func<Task> action) where T : Exception
        {
            try
            {
                await action();
            }
            catch (T)
            {
                return;
            }
            catch (Exception exception)
            {
                Assert.Fail("Expected " + typeof(T).Name + " but got " + exception.GetType().Name);
                return;
            }
            Assert.Fail("Expected " + typeof(T).Name);
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
            public bool ThrowOnConnect;
            public bool ThrowOnSend;
            public bool ThrowOnReceive;
            public byte[] ReceiveData;
            private bool _received;
            public bool Connected { get; private set; }
            public void SetReceiveData(byte[] data) { ReceiveData = data; _received = false; }

            public Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
            {
                if (ThrowOnConnect) throw new InvalidOperationException("connect failed");
                Connected = true;
                return Task.CompletedTask;
            }

            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
            {
                if (ThrowOnSend) throw new InvalidOperationException("send failed");
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

        private sealed class CountingTransport : INetworkTransport
        {
            public int ConnectCalls;
            public bool Connected { get; private set; }

            public Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
            {
                ConnectCalls++;
                Connected = true;
                return Task.CompletedTask;
            }

            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
            {
                return Task.FromResult(count);
            }

            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
            {
                var source = new TaskCompletionSource<int>();
                cancellationToken.Register(() => source.TrySetCanceled());
                return source.Task;
            }

            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }

        private sealed class BlockingSendTransport : INetworkTransport
        {
            public readonly TaskCompletionSource<bool> SendStarted = new TaskCompletionSource<bool>();
            public bool Connected { get; private set; }
            public Task ConnectAsync(string host, int port, CancellationToken cancellationToken) { Connected = true; return Task.CompletedTask; }
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
            {
                SendStarted.TrySetResult(true);
                var source = new TaskCompletionSource<int>();
                cancellationToken.Register(() => source.TrySetCanceled());
                return source.Task;
            }
            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
            {
                var source = new TaskCompletionSource<int>();
                cancellationToken.Register(() => source.TrySetCanceled());
                return source.Task;
            }
            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }

        private sealed class DelayedReceiveTransport : INetworkTransport
        {
            private readonly byte[] _data;
            private readonly TaskCompletionSource<int> _firstReceive = new TaskCompletionSource<int>();
            private byte[] _firstBuffer;
            private bool _firstStarted;
            public bool Connected { get; private set; }

            public DelayedReceiveTransport(byte[] data) { _data = data; }
            public Task ConnectAsync(string host, int port, CancellationToken cancellationToken) { Connected = true; return Task.CompletedTask; }
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken) { return Task.FromResult(count); }
            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
            {
                if (!_firstStarted)
                {
                    _firstStarted = true;
                    _firstBuffer = buffer;
                    return _firstReceive.Task;
                }
                var source = new TaskCompletionSource<int>();
                cancellationToken.Register(() => source.TrySetCanceled());
                return source.Task;
            }
            public void ReleaseFirstReceive()
            {
                Buffer.BlockCopy(_data, 0, _firstBuffer, 0, _data.Length);
                _firstReceive.TrySetResult(_data.Length);
            }
            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }

        private sealed class SlowSendTransport : INetworkTransport
        {
            private int _activeSends;
            public readonly List<int> Commands = new List<int>();
            public int MaxConcurrentSends;
            public bool Connected { get; private set; }

            public Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
            {
                Connected = true;
                return Task.CompletedTask;
            }

            public async Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
            {
                int active = Interlocked.Increment(ref _activeSends);
                MaxConcurrentSends = Math.Max(MaxConcurrentSends, active);
                try
                {
                    var reader = new PacketReader(1024);
                    reader.Append(data, offset, count);
                    Assert.IsTrue(reader.TryRead(out NetworkPacket packet));
                    Commands.Add(packet.Command);
                    await Task.Delay(20, cancellationToken);
                    return count;
                }
                finally
                {
                    Interlocked.Decrement(ref _activeSends);
                }
            }

            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
            {
                var source = new TaskCompletionSource<int>();
                cancellationToken.Register(() => source.TrySetCanceled());
                return source.Task;
            }

            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }
    }
}
