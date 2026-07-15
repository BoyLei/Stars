#if NETWORK_REFACTOR_V2_STANDALONE
using System;
using System.Collections.Generic;
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
            byte[] first = PacketCodec.Encode(10, new byte[] { 1 }, false); byte[] second = PacketCodec.Encode(11, new byte[] { 2, 3 }, false); byte[] sticky = new byte[first.Length + second.Length]; Buffer.BlockCopy(first, 0, sticky, 0, first.Length); Buffer.BlockCopy(second, 0, sticky, first.Length, second.Length); var stickyReader = new PacketReader(1024); stickyReader.Append(sticky, 0, sticky.Length); Check(stickyReader.TryRead(out NetworkPacket firstPacket) && firstPacket.Command == 10 && stickyReader.TryRead(out NetworkPacket secondPacket) && secondPacket.Command == 11 && !stickyReader.TryRead(out _), "sticky multiple packets");
            var shortLengthReader = new PacketReader(16); shortLengthReader.Append(new byte[] { 0, 0, 0, 0, 1, 0 }, 0, 6); Check(shortLengthReader.IsCorrupt && !shortLengthReader.TryRead(out _), "short declared length is corrupt without throw");
            byte[] oneChunkOversize = PacketCodec.Encode(1, new byte[32], false); var oneChunkOversizeReader = new PacketReader(16); oneChunkOversizeReader.Append(oneChunkOversize, 0, oneChunkOversize.Length); Check(oneChunkOversizeReader.IsCorrupt && !oneChunkOversizeReader.TryRead(out _), "one chunk oversize marks corrupt without throw");
            byte[] maxStickyFirst = PacketCodec.Encode(10, new byte[] { 1, 2, 3, 4 }, false); byte[] maxStickySecond = PacketCodec.Encode(11, new byte[] { 5, 6, 7, 8 }, false); byte[] maxSticky = new byte[maxStickyFirst.Length + maxStickySecond.Length]; Buffer.BlockCopy(maxStickyFirst, 0, maxSticky, 0, maxStickyFirst.Length); Buffer.BlockCopy(maxStickySecond, 0, maxSticky, maxStickyFirst.Length, maxStickySecond.Length); var maxStickyReader = new PacketReader(12); maxStickyReader.Append(maxSticky, 0, maxSticky.Length); Check(!maxStickyReader.IsCorrupt && maxStickyReader.TryRead(out NetworkPacket maxStickyPacket1) && maxStickyPacket1.Command == 10 && maxStickyReader.TryRead(out NetworkPacket maxStickyPacket2) && maxStickyPacket2.Command == 11, "sticky packet total may exceed max packet size");
            Check(PacketCodec.Encode(0x1234, new byte[] { 9, 8, 7 }, false)[4] == 0x34, "legacy header");
            Check(RpcCodec.Encode(0x1234, new byte[] { 7, 8 })[0] == 14, "rpc envelope");
            Check(ConnectionUiPolicy.Evaluate(RealConnectionState.Connected, PingHealthState.CriticalWeak) == ConnectionUiIntent.FakeReconnectDialog, "ui policy");
            var clock = new TimeSyncClock(); clock.Update(1000, 900); TimeSyncSnapshot snapshot = clock.Current; Check(snapshot.ServerTimeStamp == 1000 && snapshot.ClientReceiveTimeStamp == 900 && snapshot.Offset == 100, "time sync snapshot");
            var ping = new PingHealthMonitor(); for (int i = 0; i < 5; i++) ping.RecordPong(920); Check(ping.State == PingHealthState.Weak, "legacy ping threshold");
            ping.Reset(); for (int i = 0; i < PingHealthMonitor.TotalWeakLimit; i++) { ping.RecordPong(920); ping.RecordPong(1); } Check(ping.State == PingHealthState.ReturnLogin, "legacy total weak limit");

            var arbiter = new GlobalUiArbiter(); arbiter.Register("game", SocketUiVisibility.VisibleWhenFocused, UiContext.Lobby); arbiter.SetContext(UiContext.Battle);
            arbiter.UpdateIntent("game", ConnectionUiIntent.ReturnLoginDialog); Check(arbiter.CurrentIntent.Intent == ConnectionUiIntent.None, "background hidden");
            arbiter.SetContext(UiContext.Lobby); Check(arbiter.CurrentIntent.Intent == ConnectionUiIntent.ReturnLoginDialog, "focused visible");
            var focusedArbiter = new GlobalUiArbiter(); focusedArbiter.Register("game", SocketUiVisibility.AlwaysVisible, UiContext.Lobby); focusedArbiter.Register("battle", SocketUiVisibility.VisibleWhenFocused, UiContext.Battle); focusedArbiter.SetContext(UiContext.Battle);
            focusedArbiter.UpdateIntent("game", ConnectionUiIntent.ReturnLoginDialog); focusedArbiter.UpdateIntent("battle", ConnectionUiIntent.RealReconnectDialog); Check(focusedArbiter.CurrentIntent.SocketName == "battle" && focusedArbiter.CurrentIntent.Intent == ConnectionUiIntent.RealReconnectDialog, "focused socket wins over always visible");
            focusedArbiter.SetContext(UiContext.Lobby); Check(focusedArbiter.CurrentIntent.SocketName == "game", "cached always visible re-evaluates after focus change");

            var reuseService = new FakeRecoveryService();
            var reuseCoordinator = new ConnectionRecoveryCoordinator(reuseService);
            var reuseCurrent = new ReconnectSessionData("127.0.0.1:1", 1, "old", false);
            Check(object.ReferenceEquals(await reuseCoordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.ReuseCurrentCredential, reuseCurrent, CancellationToken.None), reuseCurrent) && reuseService.Calls == 0, "reuse current credential skips recovery service");
            bool manualOnlyFailed = false; try { await reuseCoordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.ManualOnly, reuseCurrent, CancellationToken.None); } catch (InvalidOperationException) { manualOnlyFailed = true; }
            Check(manualOnlyFailed && reuseService.Calls == 0, "manual recovery policy skips recovery service");
            var blockingRecovery = new BlockingRecoveryService();
            var blockingCoordinator = new ConnectionRecoveryCoordinator(blockingRecovery);
            Task<ReconnectSessionData> pendingRecovery = blockingCoordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, reuseCurrent, CancellationToken.None);
            Task<ReconnectSessionData> canceledRecovery = blockingCoordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, reuseCurrent, new CancellationToken(true));
            Check(canceledRecovery.IsCanceled && !pendingRecovery.IsCompleted, "recovery honors canceled caller while pending");
            blockingRecovery.Complete();
            var flakyRecovery = new FlakyRecoveryService();
            var flakyCoordinator = new ConnectionRecoveryCoordinator(flakyRecovery);
            bool flakyFailed = false; try { await flakyCoordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, reuseCurrent, CancellationToken.None); } catch (InvalidOperationException) { flakyFailed = true; }
            ReconnectSessionData flakyRecovered = await flakyCoordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, reuseCurrent, CancellationToken.None);
            Check(flakyFailed && flakyRecovery.Calls == 2 && flakyRecovered.Token == "new", "recovery releases pending after failure");
            var multiSocketRecovery = new FakeRecoveryService();
            var multiSocketCoordinator = new ConnectionRecoveryCoordinator(multiSocketRecovery);
            await Task.WhenAll(multiSocketCoordinator.RecoverAsync("game", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, reuseCurrent, CancellationToken.None), multiSocketCoordinator.RecoverAsync("battle", RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, reuseCurrent, CancellationToken.None));
            Check(multiSocketRecovery.Calls == 2, "recovery does not deduplicate different sockets");

            var recoverTransport = new CountingTransport();
            var recoverHub = new NetworkHub();
            NetworkConnection recoverConnection = recoverHub.Register("game", () => recoverTransport, new NetworkConnectionConfig { RecoveryPolicy = RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, ReconnectDelay = TimeSpan.Zero });
            await recoverConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            recoverConnection.MarkVerified();
            var recoverCoordinator = new ConnectionRecoveryCoordinator(new FakeRecoveryService());
            var recoverCurrent = new ReconnectSessionData("127.0.0.1:1", 1, "old", true);
            await Task.WhenAll(recoverHub.RecoverAndReconnect("game", recoverCurrent, recoverCoordinator, CancellationToken.None), recoverHub.RecoverAndReconnect("game", recoverCurrent, recoverCoordinator, CancellationToken.None));
            Check(recoverTransport.ConnectCalls == 2, "hub deduplicates concurrent recover and reconnect");
            var hubCanceledTransport = new CountingTransport();
            var hubCanceledHub = new NetworkHub();
            NetworkConnection hubCanceledConnection = hubCanceledHub.Register("game", () => hubCanceledTransport, new NetworkConnectionConfig { RecoveryPolicy = RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, ReconnectDelay = TimeSpan.Zero });
            await hubCanceledConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            hubCanceledConnection.MarkVerified();
            var hubBlockingService = new BlockingRecoveryService();
            var hubBlockingCoordinator = new ConnectionRecoveryCoordinator(hubBlockingService);
            Task hubPendingRecovery = hubCanceledHub.RecoverAndReconnect("game", recoverCurrent, hubBlockingCoordinator, CancellationToken.None);
            Task hubCanceledRecovery = hubCanceledHub.RecoverAndReconnect("game", recoverCurrent, hubBlockingCoordinator, new CancellationToken(true));
            Check(hubCanceledRecovery.IsCanceled && !hubPendingRecovery.IsCompleted, "hub recovery honors canceled caller while pending");
            hubBlockingService.Complete();
            await hubPendingRecovery;
            var failRecoverTransport = new CountingTransport();
            var failRecoverHub = new NetworkHub();
            NetworkConnection failRecoverConnection = failRecoverHub.Register("game", () => failRecoverTransport, new NetworkConnectionConfig { RecoveryPolicy = RecoveryAuthenticationPolicy.RefreshSessionBeforeReconnect, ReconnectDelay = TimeSpan.Zero });
            await failRecoverConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            failRecoverConnection.MarkVerified();
            bool recoverFailed = false;
            try { await failRecoverHub.RecoverAndReconnect("game", recoverCurrent, new ConnectionRecoveryCoordinator(new FailingRecoveryService()), CancellationToken.None); }
            catch (InvalidOperationException) { recoverFailed = true; }
            Check(recoverFailed && failRecoverConnection.State == RealConnectionState.ReconnectFailed && !failRecoverTransport.Connected, "recovery failure marks reconnect failed");
            var kickedTransport = new CountingTransport();
            var kickedHub = new NetworkHub();
            NetworkConnection kickedConnection = kickedHub.Register("game", () => kickedTransport, new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero });
            await kickedConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            kickedConnection.MarkVerifyFailed();
            bool kickedRecoverFailed = false; try { await kickedHub.RecoverAndReconnect("game", recoverCurrent, new ConnectionRecoveryCoordinator(new FakeRecoveryService()), CancellationToken.None); } catch (InvalidOperationException) { kickedRecoverFailed = true; }
            Check(kickedRecoverFailed && kickedConnection.State == RealConnectionState.Kicked && kickedTransport.ConnectCalls == 1, "kicked connection does not recover and reconnect");
            var canceledReconnectTransport = new FakeTransport();
            var canceledReconnectConnection = new NetworkConnection("canceled-reconnect", canceledReconnectTransport, new MainThreadEventPump(), new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero });
            await canceledReconnectConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            canceledReconnectConnection.MarkVerified();
            bool reconnectCanceled = false; try { await canceledReconnectConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), new CancellationToken(true)); } catch (OperationCanceledException) { reconnectCanceled = true; }
            Check(reconnectCanceled && canceledReconnectConnection.State == RealConnectionState.Connected && canceledReconnectTransport.Connected, "reconnect with canceled token does not mutate connected connection");
            var failedPump = new MainThreadEventPump();
            var failedConnection = new NetworkConnection("failed", new FakeTransport { ReceiveData = PacketCodec.Encode(203, new byte[] { 1 }, false) }, failedPump, new NetworkConnectionConfig());
            int failedCalls = 0; failedConnection.PacketReceived += (_, __) => failedCalls++;
            await failedConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            failedConnection.MarkVerified();
            await Task.Yield();
            failedConnection.MarkReconnectFailed();
            failedPump.Tick();
            Check(failedConnection.State == RealConnectionState.ReconnectFailed && failedCalls == 0, "reconnect failed drops old receive loop packet");
            var reconnectFailTransport = new FakeTransport();
            var reconnectFailConnection = new NetworkConnection("reconnect-connect-fail", reconnectFailTransport, new MainThreadEventPump(), new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero });
            await reconnectFailConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            reconnectFailConnection.MarkVerified();
            reconnectFailTransport.ThrowOnConnect = true;
            bool reconnectConnectFailed = false; try { await reconnectFailConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None); } catch (InvalidOperationException) { reconnectConnectFailed = true; }
            Check(reconnectConnectFailed && reconnectFailConnection.State == RealConnectionState.ReconnectFailed && !reconnectFailTransport.Connected, "reconnect connect failure marks reconnect failed");

            var sender = new PartialSender(); await TransportSend.SendAllAsync(sender, new byte[7], CancellationToken.None); Check(sender.Total == 7 && sender.Calls > 1, "partial sends");
            var slowSend = new SlowSendTransport();
            var serializedConnection = new NetworkConnection("serialized", slowSend, new MainThreadEventPump(), new NetworkConnectionConfig());
            await serializedConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            serializedConnection.MarkVerified();
            await Task.WhenAll(serializedConnection.Send(101, new byte[] { 1 }), serializedConnection.Send(102, new byte[] { 2 }));
            Check(slowSend.MaxConcurrentSends == 1 && slowSend.Commands.Count == 2 && slowSend.Commands[0] == 101 && slowSend.Commands[1] == 102, "connection serializes concurrent sends");
            var hub = new NetworkHub(); int calls = 0; hub.Subscribe("*", 9, _ => calls++); hub.EnqueueReceived("battle", new NetworkPacket(9, 0, new byte[0])); Check(calls == 0, "main thread gate"); hub.Tick(); Check(calls == 1, "main thread dispatch");
            var nestedHub = new NetworkHub(); var nested = new List<int>(); nestedHub.Subscribe("game", 9, _ => { nested.Add(9); nestedHub.EnqueueReceived("game", new NetworkPacket(10, 0, new byte[0])); }); nestedHub.Subscribe("game", 10, _ => nested.Add(10)); nestedHub.EnqueueReceived("game", new NetworkPacket(9, 0, new byte[0])); nestedHub.Tick(); Check(nested.Count == 2 && nested[0] == 9 && nested[1] == 10, "nested message dispatches in same tick");
            var locked = new NetworkHub(); int lockCalls = 0; locked.Subscribe("game", 9, _ => lockCalls++); locked.LockMessages(true);
            locked.EnqueueReceived("game", new NetworkPacket(9, 0, new byte[0])); locked.Tick(); Check(lockCalls == 0 && locked.IsMessageLocked, "message lock blocks normal dispatch");
            locked.LockMessages(false); locked.Tick(); Check(lockCalls == 1 && !locked.IsMessageLocked, "message unlock flushes normal dispatch");
            var throwingHub = new NetworkHub(); int flushedAfterThrow = 0; throwingHub.Subscribe("game", 1, _ => throw new InvalidOperationException("callback failed")); throwingHub.Subscribe("game", 2, _ => flushedAfterThrow++); throwingHub.LockMessages(true); throwingHub.EnqueueReceived("game", new NetworkPacket(2, 0, new byte[0])); throwingHub.Tick(); throwingHub.LockMessages(false); throwingHub.EnqueueReceived("game", new NetworkPacket(1, 0, new byte[0])); bool tickFailed = false; try { throwingHub.Tick(); } catch (InvalidOperationException) { tickFailed = true; } Check(!tickFailed && flushedAfterThrow == 1, "tick exception is isolated and still flushes unlocked messages");
            var luaHub = new NetworkHub(); var luaApi = new LuaNetworkApi(luaHub); object luaOwner = new object(); int luaHandleCalls = 0; int luaOwnerCalls = 0; long luaHandle = luaApi.Subscribe("game", 9, _ => luaHandleCalls++); luaApi.Subscribe("game", 9, _ => luaOwnerCalls++, luaOwner); luaApi.Unsubscribe(luaHandle); luaApi.UnsubscribeOwner(luaOwner); luaHub.EnqueueReceived("game", new NetworkPacket(9, 0, new byte[0])); luaHub.Tick(); Check(luaHandleCalls == 0 && luaOwnerCalls == 0, "lua unsubscribe by handle and owner");
            var luaHandles = (System.Collections.IDictionary)typeof(LuaNetworkApi).GetField("_handles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(luaApi); Check(luaHandles.Count == 0, "lua owner release clears handles");
            var luaIdempotentHub = new NetworkHub(); var luaIdempotentApi = new LuaNetworkApi(luaIdempotentHub); object luaIdempotentOwner = new object(); int luaIdempotentCalls = 0; long luaIdempotentHandle = luaIdempotentApi.Subscribe("game", 9, _ => luaIdempotentCalls++, luaIdempotentOwner); luaIdempotentApi.UnsubscribeOwner(luaIdempotentOwner); luaIdempotentApi.UnsubscribeOwner(luaIdempotentOwner); luaIdempotentApi.Unsubscribe(luaIdempotentHandle); luaIdempotentApi.Unsubscribe(luaIdempotentHandle); luaIdempotentHub.EnqueueReceived("game", new NetworkPacket(9, 0, new byte[0])); luaIdempotentHub.Tick(); Check(luaIdempotentCalls == 0, "lua release is idempotent across owner and handle");
            var connection = hub.Register("game", () => new FakeTransport(), new NetworkConnectionConfig()); hub.Subscribe("game", 10, _ => calls++); connection.InjectReceivedForTests(new NetworkPacket(10, 0, new byte[0])); hub.Tick(); Check(calls == 2, "one tick dispatch");
            var luaSendTransport = new FakeTransport(); var luaSendHub = new NetworkHub(); NetworkConnection luaSendConnection = luaSendHub.Register("game", () => luaSendTransport, new NetworkConnectionConfig()); await luaSendConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None); luaSendConnection.MarkVerified(); await new LuaNetworkApi(luaSendHub).SendRpc("game", 203, 0x1234, new byte[] { 9 }); Check(luaSendTransport.SentPackets.Count == 1 && luaSendTransport.SentPackets[0].Command == 203 && luaSendTransport.SentPackets[0].Payload[0] == 14 && luaSendTransport.SentPackets[0].Payload[1] == 0x34, "lua rpc send envelope");
            var csharpHub = new NetworkHub(); var csharpApi = new CSharpNetworkApi(csharpHub); object csharpOwner = new object(); int csharpNormal = 0; int csharpRpcEntity = 0; int csharpRpcGroup = 0; IDisposable csharpNormalToken = csharpApi.Subscribe("game", 9, _ => csharpNormal++); csharpApi.SubscribeRpc("game", 14, 1001, 7, _ => csharpRpcEntity++, csharpOwner); csharpApi.SubscribeRpcGroup("game", 1001, _ => csharpRpcGroup++, csharpOwner); csharpNormalToken.Dispose(); csharpApi.UnsubscribeOwner(csharpOwner); csharpHub.EnqueueReceived("game", new NetworkPacket(9, 0, new byte[0])); csharpHub.EnqueueReceived("game", new NetworkPacket(14, 0, new byte[0], 1001, 7)); csharpHub.Tick(); Check(csharpNormal == 0 && csharpRpcEntity == 0 && csharpRpcGroup == 0, "csharp unsubscribe by token and owner");
            var csharpIdempotentHub = new NetworkHub(); var csharpIdempotentApi = new CSharpNetworkApi(csharpIdempotentHub); object csharpIdempotentOwner = new object(); int csharpIdempotentCalls = 0; IDisposable csharpIdempotentToken = csharpIdempotentApi.Subscribe("game", 9, _ => csharpIdempotentCalls++, csharpIdempotentOwner); csharpIdempotentApi.UnsubscribeOwner(csharpIdempotentOwner); csharpIdempotentApi.UnsubscribeOwner(csharpIdempotentOwner); csharpIdempotentToken.Dispose(); csharpIdempotentToken.Dispose(); csharpIdempotentHub.EnqueueReceived("game", new NetworkPacket(9, 0, new byte[0])); csharpIdempotentHub.Tick(); Check(csharpIdempotentCalls == 0, "csharp release is idempotent across owner and token");
            int entityCalls = 0; int groupCalls = 0; hub.SubscribeRpc("game", 14, 1001, 9, _ => entityCalls++); hub.SubscribeRpcGroup("game", 1001, _ => groupCalls++);
            connection.InjectReceivedForTests(new NetworkPacket(14, 0, new byte[0], 1001, 8)); hub.Tick(); Check(entityCalls == 0 && groupCalls == 1, "rpc group without entity");
            connection.InjectReceivedForTests(new NetworkPacket(14, 0, new byte[0], 1001, 9)); hub.Tick(); Check(entityCalls == 1 && groupCalls == 2, "rpc entity and group");

            var transport = new FakeTransport();
            var config = new NetworkConnectionConfig { VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false), HeartbeatPacketFactory = () => new NetworkOutboundPacket(200, new byte[] { 2 }, false), HeartbeatInterval = TimeSpan.FromMilliseconds(100) };
            var verifiedConnection = new NetworkConnection("verify", transport, new MainThreadEventPump(), config);
            await verifiedConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            Check(transport.Sent == 1, "verify packet sent");
            await verifiedConnection.Send(101, new byte[] { 3 }); await verifiedConnection.Send(102, new byte[] { 4 }); Check(transport.Sent == 1, "business cached before verify");
            verifiedConnection.MarkVerified(); await Task.Yield(); Check(transport.SentPackets.Count == 3 && transport.SentPackets[1].Command == 101 && transport.SentPackets[2].Command == 102, "cached business flushes in order after verify");
            await verifiedConnection.Send(103, new byte[] { 5 }); Check(transport.Sent == 4, "business after verify");
            var apiTransport = new FakeTransport(); var apiHub = new NetworkHub(); NetworkConnection apiConnection = apiHub.Register("game", () => apiTransport, new NetworkConnectionConfig()); await apiConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None); apiConnection.MarkVerified(); var api = new CSharpNetworkApi(apiHub); await api.SendCustom("game", 201, 7, value => new[] { (byte)value }); await api.SendProtobuf("game", 202, 8, value => new[] { (byte)value }); await api.SendRpc("game", 203, 0x1234, new byte[] { 9 }); Check(apiTransport.SentPackets.Count == 3 && apiTransport.SentPackets[0].Command == 201 && apiTransport.SentPackets[0].Payload[0] == 7 && apiTransport.SentPackets[1].Command == 202 && apiTransport.SentPackets[1].Payload[0] == 8 && apiTransport.SentPackets[2].Command == 203 && apiTransport.SentPackets[2].Payload[0] == 14 && apiTransport.SentPackets[2].Payload[1] == 0x34, "csharp api custom protobuf rpc send");
            Check(await verifiedConnection.SendHeartbeatIfDue(100), "heartbeat sent"); Check(!await verifiedConnection.SendHeartbeatIfDue(150), "heartbeat interval");
            var heartbeatFailTransport = new FakeTransport();
            var heartbeatFailConnection = new NetworkConnection("heartbeat-fail", heartbeatFailTransport, new MainThreadEventPump(), config);
            await heartbeatFailConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            heartbeatFailConnection.MarkVerified();
            heartbeatFailTransport.ThrowOnSend = true;
            bool heartbeatFailed = false; try { await heartbeatFailConnection.SendHeartbeatIfDue(100); } catch (InvalidOperationException) { heartbeatFailed = true; }
            heartbeatFailTransport.ThrowOnSend = false;
            int heartbeatRetryBefore = heartbeatFailTransport.Sent;
            Check(heartbeatFailed && await heartbeatFailConnection.SendHeartbeatIfDue(100) && heartbeatFailTransport.Sent == heartbeatRetryBefore + 1, "heartbeat send failure stays visible and retryable");
            var pongPump = new MainThreadEventPump();
            var pongConfig = new NetworkConnectionConfig { HeartbeatPongDecoder = pkt => pkt.Command == 200 ? (int?)PingHealthMonitor.WeakLatencyMilliseconds : null };
            var pongConnection = new NetworkConnection("pong", new FakeTransport { ReceiveData = PacketCodec.Encode(200, new byte[] { 1 }, false) }, pongPump, pongConfig);
            int pongNormalCalls = 0; pongConnection.PacketReceived += (_, __) => pongNormalCalls++;
            await pongConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            pongConnection.MarkVerified(); await Task.Yield(); pongPump.Tick();
            for (int i = 0; i < PingHealthMonitor.WeakSampleCount; i++) pongConnection.RecordHeartbeatPong(PingHealthMonitor.WeakLatencyMilliseconds);
            Check(pongConnection.PingHealth.State == PingHealthState.Weak && pongNormalCalls == 0, "heartbeat pong updates ping without normal dispatch");
            pongConnection.RecordHeartbeatPong(10, 1000);
            Check(pongConnection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds - 1) == PingHealthState.Healthy && pongConnection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds) == PingHealthState.HeartbeatLost, "heartbeat pong refreshes last pong time");
            var heartbeatLostConnection = new NetworkConnection("heartbeat-lost", new FakeTransport(), new MainThreadEventPump(), new NetworkConnectionConfig());
            heartbeatLostConnection.RecordHeartbeatPong(10, 1000);
            Check(heartbeatLostConnection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds - 1) == PingHealthState.Healthy, "heartbeat age stays healthy before threshold");
            Check(heartbeatLostConnection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds) == PingHealthState.HeartbeatLost, "heartbeat age detects lost");
            var resetPingConnection = new NetworkConnection("reset-ping", new FakeTransport(), new MainThreadEventPump(), new NetworkConnectionConfig());
            await resetPingConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            resetPingConnection.MarkVerified();
            resetPingConnection.RecordHeartbeatPong(10, 1000);
            Check(resetPingConnection.CheckHeartbeatAge(1000 + PingHealthMonitor.HeartbeatLostMilliseconds) == PingHealthState.HeartbeatLost, "ping state can become heartbeat lost before verify reset");
            resetPingConnection.MarkVerified();
            Check(resetPingConnection.PingHealth.State == PingHealthState.Healthy, "mark verified resets ping health");
            Check(resetPingConnection.CheckHeartbeatAge(long.MaxValue) == PingHealthState.Healthy, "mark verified resets heartbeat pong age");
            var verifyPump = new MainThreadEventPump();
            var verifySuccessConfig = new NetworkConnectionConfig { VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false), VerifyResultDecoder = pkt => pkt.Command == 2 ? (bool?)true : null };
            var verifySuccessConnection = new NetworkConnection("verify-success", new FakeTransport { ReceiveData = PacketCodec.Encode(2, new byte[] { 1 }, false) }, verifyPump, verifySuccessConfig);
            int verifyNormalCalls = 0; verifySuccessConnection.PacketReceived += (_, __) => verifyNormalCalls++;
            await verifySuccessConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield(); await Task.Yield(); verifyPump.Tick(); Check(verifySuccessConnection.State == RealConnectionState.Connected && verifyNormalCalls == 0, "verify success packet connects without normal dispatch");
            var verifyFailurePump = new MainThreadEventPump();
            var verifyFailureConfig = new NetworkConnectionConfig { VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false), VerifyResultDecoder = pkt => pkt.Command == 3 ? (bool?)false : null };
            var verifyFailureTransport = new FakeTransport { ReceiveData = PacketCodec.Encode(3, new byte[] { 0 }, false) };
            var verifyFailureConnection = new NetworkConnection("verify-failure", verifyFailureTransport, verifyFailurePump, verifyFailureConfig);
            await verifyFailureConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield(); await Task.Yield(); verifyFailurePump.Tick(); Check(verifyFailureConnection.State == RealConnectionState.Kicked && !verifyFailureTransport.Connected, "verify failure packet kicks and closes transport");
            var verifyTimeoutPump = new MainThreadEventPump();
            var verifyTimeoutTransport = new FakeTransport();
            var verifyTimeoutConfig = new NetworkConnectionConfig { VerifyTimeout = TimeSpan.FromMilliseconds(20), VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false) };
            var verifyTimeoutConnection = new NetworkConnection("verify-timeout", verifyTimeoutTransport, verifyTimeoutPump, verifyTimeoutConfig);
            await verifyTimeoutConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Delay(60); verifyTimeoutPump.Tick(); Check(verifyTimeoutConnection.State == RealConnectionState.Kicked && !verifyTimeoutTransport.Connected, "verify timeout kicks and closes transport");
            var staleVerifyTimeoutPump = new MainThreadEventPump();
            var staleVerifyTimeoutTransport = new FakeTransport();
            var staleVerifyTimeoutConfig = new NetworkConnectionConfig { VerifyTimeout = TimeSpan.FromMilliseconds(20), ReconnectDelay = TimeSpan.Zero, VerifyPacketFactory = _ => new NetworkOutboundPacket(100, new byte[] { 1 }, false) };
            var staleVerifyTimeoutConnection = new NetworkConnection("stale-verify-timeout", staleVerifyTimeoutTransport, staleVerifyTimeoutPump, staleVerifyTimeoutConfig);
            await staleVerifyTimeoutConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            await Task.Delay(60);
            await staleVerifyTimeoutConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "new", true), CancellationToken.None);
            staleVerifyTimeoutPump.Tick(); Check(staleVerifyTimeoutConnection.State == RealConnectionState.Verifying && staleVerifyTimeoutTransport.Connected, "old verify timeout does not kick new reconnect generation");
            var verifySendFailTransport = new FakeTransport { ThrowOnSend = true };
            var verifySendFailConnection = new NetworkConnection("verify-send-fail", verifySendFailTransport, new MainThreadEventPump(), verifyTimeoutConfig);
            bool verifySendFailed = false; try { await verifySendFailConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None); } catch (InvalidOperationException) { verifySendFailed = true; }
            Check(verifySendFailed && verifySendFailConnection.State == RealConnectionState.Kicked && !verifySendFailTransport.Connected, "verify packet send failure kicks and closes transport");
            var closeDuringVerifyTransport = new BlockingSendTransport();
            var closeDuringVerifyPump = new MainThreadEventPump();
            var closeDuringVerifyConnection = new NetworkConnection("close-during-verify", closeDuringVerifyTransport, closeDuringVerifyPump, verifyTimeoutConfig);
            var closeDuringVerifyStates = new List<RealConnectionState>();
            closeDuringVerifyConnection.StateChanged += (_, nextState) => closeDuringVerifyStates.Add(nextState);
            Task closeDuringVerifyStart = closeDuringVerifyConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await closeDuringVerifyTransport.SendStarted.Task;
            closeDuringVerifyConnection.Close();
            closeDuringVerifyPump.Tick();
            bool closeDuringVerifyCanceled = false; try { await closeDuringVerifyStart; } catch (OperationCanceledException) { closeDuringVerifyCanceled = true; }
            Check(closeDuringVerifyCanceled && closeDuringVerifyConnection.State == RealConnectionState.Closed && !closeDuringVerifyStates.Contains(RealConnectionState.Kicked) && !closeDuringVerifyTransport.Connected, "close during verify send keeps closed state");
            var initialConnectFailTransport = new FakeTransport { ThrowOnConnect = true };
            var initialConnectFailConnection = new NetworkConnection("initial-connect-fail", initialConnectFailTransport, new MainThreadEventPump(), new NetworkConnectionConfig());
            bool initialConnectFailed = false; try { await initialConnectFailConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None); } catch (InvalidOperationException) { initialConnectFailed = true; }
            Check(initialConnectFailed && initialConnectFailConnection.State == RealConnectionState.ReconnectFailed && !initialConnectFailTransport.Connected, "initial connect failure marks reconnect failed");
            var canceledStartTransport = new FakeTransport();
            var canceledStartConnection = new NetworkConnection("canceled-start", canceledStartTransport, new MainThreadEventPump(), new NetworkConnectionConfig());
            bool startCanceled = false; try { await canceledStartConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), new CancellationToken(true)); } catch (OperationCanceledException) { startCanceled = true; }
            Check(startCanceled && canceledStartConnection.State == RealConnectionState.Disconnected && !canceledStartTransport.Connected, "start with canceled token does not mutate disconnected connection");

            byte[] immediateBytes = PacketCodec.Encode(300, new byte[] { 4 }, false);
            var immediateTransport = new FakeTransport { ReceiveData = immediateBytes };
            var immediateConfig = new NetworkConnectionConfig(); immediateConfig.ImmediateCommands.Add(300);
            var immediateConnection = new NetworkConnection("immediate", immediateTransport, new MainThreadEventPump(), immediateConfig);
            int immediateCalls = 0; immediateConnection.ImmediatePacketReceived += (_, __) => immediateCalls++;
            await immediateConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield(); await Task.Yield(); Check(immediateCalls == 1, "immediate command bypasses tick");
            var immediateHub = new NetworkHub(); int normalImmediateCalls = 0; int hubImmediateCalls = 0;
            immediateHub.Subscribe("game", 300, _ => normalImmediateCalls++); immediateHub.SubscribeImmediate("game", 300, _ => hubImmediateCalls++);
            NetworkConnection immediateHubConnection = immediateHub.Register("game", () => new FakeTransport { ReceiveData = immediateBytes }, immediateConfig);
            await immediateHubConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield(); await Task.Yield(); Check(hubImmediateCalls == 1 && normalImmediateCalls == 0, "immediate handler without normal dispatch");
            immediateHub.Tick(); Check(normalImmediateCalls == 0, "immediate command does not enter normal tick queue");

            int rpcCmd = 14; int msgCmd = 2002; byte[] rpcPayload = RpcCodec.Encode(msgCmd, new byte[] { 5 });
            byte[] rpcWire = PacketCodec.Encode(rpcCmd, rpcPayload, false);
            var rpcHub = new NetworkHub(); int rpcEntity = 0; int rpcGroup = 0;
            rpcHub.SubscribeRpc("rpc", rpcCmd, msgCmd, 0, _ => rpcEntity++); rpcHub.SubscribeRpcGroup("rpc", msgCmd, _ => rpcGroup++);
            var rpcConfig = new NetworkConnectionConfig { RpcCommand = rpcCmd };
            NetworkConnection rpcConn = rpcHub.Register("rpc", () => new FakeTransport { ReceiveData = rpcWire }, rpcConfig);
            await rpcConn.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield(); rpcHub.Tick();
            Check(rpcEntity == 1 && rpcGroup == 1, "rpc decode from receive loop");

            var stalePump = new MainThreadEventPump();
            var staleTransport = new FakeTransport { ReceiveData = PacketCodec.Encode(201, new byte[] { 1 }, false) };
            var staleConnection = new NetworkConnection("stale", staleTransport, stalePump, new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero });
            int staleCalls = 0; staleConnection.PacketReceived += (_, __) => staleCalls++;
            await staleConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield();
            await staleConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            stalePump.Tick(); Check(staleCalls == 0, "reconnect drops old receive loop packet");
            var stalePongTransport = new DelayedReceiveTransport(PacketCodec.Encode(200, new byte[] { 1 }, false));
            var stalePongConnection = new NetworkConnection("stale-pong", stalePongTransport, new MainThreadEventPump(), new NetworkConnectionConfig { ReconnectDelay = TimeSpan.Zero, HeartbeatPongDecoder = pkt => pkt.Command == 200 ? (int?)10 : null });
            await stalePongConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "old", false), CancellationToken.None);
            stalePongConnection.MarkVerified(); await Task.Yield();
            await stalePongConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "new", true), CancellationToken.None);
            stalePongTransport.ReleaseFirstReceive(); await Task.Yield();
            Check(stalePongConnection.CheckHeartbeatAge(long.MaxValue) == PingHealthState.Healthy, "reconnect drops heartbeat pong returned by old receive loop after cancel");
            var exhaustedPump = new MainThreadEventPump();
            var exhaustedConnection = new NetworkConnection("exhausted", new FakeTransport { ReceiveData = PacketCodec.Encode(204, new byte[] { 1 }, false) }, exhaustedPump, new NetworkConnectionConfig { MaxReconnectAttempts = 0, MaxTotalReconnectAttempts = 1, ReconnectDelay = TimeSpan.Zero });
            int exhaustedCalls = 0; exhaustedConnection.PacketReceived += (_, __) => exhaustedCalls++;
            await exhaustedConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            exhaustedConnection.MarkVerified(); await Task.Yield();
            await exhaustedConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            exhaustedPump.Tick(); Check(exhaustedConnection.State == RealConnectionState.ReconnectFailed && exhaustedCalls == 0, "reconnect exhaustion drops old receive loop packet");
            var retryableConnection = new NetworkConnection("retryable", new FakeTransport(), new MainThreadEventPump(), new NetworkConnectionConfig { MaxReconnectAttempts = 0, MaxTotalReconnectAttempts = 2, ReconnectDelay = TimeSpan.Zero });
            await retryableConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            retryableConnection.MarkVerified();
            await retryableConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            Check(retryableConnection.State == RealConnectionState.ReconnectRetryable, "single reconnect exhaustion moves to retryable before total limit");
            await retryableConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            Check(retryableConnection.State == RealConnectionState.ReconnectFailed, "total reconnect exhaustion moves to reconnect failed");
            var clearCacheConnection = new NetworkConnection("clear-cache", new FakeTransport(), new MainThreadEventPump());
            await clearCacheConnection.Send(101, new byte[] { 1 });
            clearCacheConnection.Close();
            Check(CachedSendCount(clearCacheConnection) == 0, "close clears cached business sends");
            var verifyFailCacheConnection = new NetworkConnection("verify-fail-cache", new FakeTransport(), new MainThreadEventPump());
            await verifyFailCacheConnection.Send(102, new byte[] { 2 });
            await verifyFailCacheConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            verifyFailCacheConnection.MarkVerifyFailed();
            Check(CachedSendCount(verifyFailCacheConnection) == 0, "verify failed clears cached business sends");
            var closePump = new MainThreadEventPump();
            var closeConnection = new NetworkConnection("close", new FakeTransport { ReceiveData = PacketCodec.Encode(202, new byte[] { 1 }, false) }, closePump, new NetworkConnectionConfig());
            int closeCalls = 0; closeConnection.PacketReceived += (_, __) => closeCalls++;
            await closeConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield(); closeConnection.Close(); closePump.Tick();
            Check(closeConnection.State == RealConnectionState.Closed && closeCalls == 0, "close drops old receive loop packet");
            var corruptTransport = new FakeTransport { ReceiveData = new byte[] { 100, 0, 0, 0, 1, 0 } };
            var corruptPump = new MainThreadEventPump();
            var corruptConnection = new NetworkConnection("corrupt", corruptTransport, corruptPump, new NetworkConnectionConfig { MaxPacketSize = 64, ReconnectDelay = TimeSpan.Zero });
            int corruptCalls = 0; corruptConnection.PacketReceived += (_, receivedPacket) => { if (receivedPacket.Command == 202) corruptCalls++; };
            await corruptConnection.Start(new ReconnectSessionData("127.0.0.1:1", 1, "token", false), CancellationToken.None);
            await Task.Yield(); corruptPump.Tick(); Check(corruptConnection.State == RealConnectionState.Reconnecting, "corrupt packet moves reconnecting before reset");
            corruptTransport.SetReceiveData(PacketCodec.Encode(202, new byte[] { 1 }, false));
            await corruptConnection.ReconnectWith(new ReconnectSessionData("127.0.0.1:1", 1, "token", true), CancellationToken.None);
            await Task.Yield(); corruptPump.Tick(); Check(corruptCalls == 1, "reconnect resets packet reader after corrupt packet");
        }

        private static void Check(bool condition, string name) { if (!condition) throw new Exception("Failed: " + name); _checks++; }
        private static void ExpectThrows<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new Exception("Expected " + typeof(T).Name); }
        private static int CachedSendCount(NetworkConnection connection)
        {
            var field = typeof(NetworkConnection).GetField("_sendCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return ((System.Collections.ICollection)field.GetValue(connection)).Count;
        }

        private sealed class PartialSender : IAsyncByteSender
        {
            public int Calls; public int Total;
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken token) { Calls++; int sent = Math.Min(2, count); Total += sent; return Task.FromResult(sent); }
        }

        private sealed class FakeRecoveryService : ISessionRecoveryService
        {
            public int Calls;
            public async Task<ReconnectSessionData> RecoverAsync(string socketName, ReconnectSessionData current, CancellationToken cancellationToken)
            {
                Calls++;
                await Task.Yield();
                return new ReconnectSessionData(current.SocketAddress, current.Pid, "new", true);
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
            public void Complete() { _source.TrySetResult(new ReconnectSessionData("127.0.0.1:1", 1, "new", true)); }
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

        private sealed class FakeTransport : INetworkTransport
        {
            public int Sent;
            public readonly List<SentPacket> SentPackets = new List<SentPacket>();
            public bool ThrowOnConnect;
            public bool ThrowOnSend;
            public byte[] ReceiveData;
            private bool _received;
            public bool Connected { get; private set; }
            public void SetReceiveData(byte[] data) { ReceiveData = data; _received = false; }
            public Task ConnectAsync(string host, int port, CancellationToken token) { if (ThrowOnConnect) throw new InvalidOperationException("connect failed"); Connected = true; return Task.CompletedTask; }
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken token)
            {
                if (ThrowOnSend) throw new InvalidOperationException("send failed");
                var reader = new PacketReader(1024);
                reader.Append(data, offset, count);
                if (!reader.TryRead(out NetworkPacket packet)) throw new Exception("send packet decode failed");
                Sent++;
                SentPackets.Add(new SentPacket { Command = packet.Command, Payload = packet.Payload });
                return Task.FromResult(count);
            }
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

        private sealed class SentPacket
        {
            public int Command;
            public byte[] Payload;
        }

        private sealed class CountingTransport : INetworkTransport
        {
            public int ConnectCalls;
            public bool Connected { get; private set; }
            public Task ConnectAsync(string host, int port, CancellationToken token) { ConnectCalls++; Connected = true; return Task.CompletedTask; }
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken token) { return Task.FromResult(count); }
            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken token)
            {
                var source = new TaskCompletionSource<int>(); token.Register(() => source.TrySetCanceled()); return source.Task;
            }
            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }

        private sealed class BlockingSendTransport : INetworkTransport
        {
            public readonly TaskCompletionSource<bool> SendStarted = new TaskCompletionSource<bool>();
            public bool Connected { get; private set; }
            public Task ConnectAsync(string host, int port, CancellationToken token) { Connected = true; return Task.CompletedTask; }
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken token)
            {
                SendStarted.TrySetResult(true);
                var source = new TaskCompletionSource<int>();
                token.Register(() => source.TrySetCanceled());
                return source.Task;
            }
            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken token)
            {
                var source = new TaskCompletionSource<int>();
                token.Register(() => source.TrySetCanceled());
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
            public Task ConnectAsync(string host, int port, CancellationToken token) { Connected = true; return Task.CompletedTask; }
            public Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken token) { return Task.FromResult(count); }
            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken token)
            {
                if (!_firstStarted)
                {
                    _firstStarted = true;
                    _firstBuffer = buffer;
                    return _firstReceive.Task;
                }
                var source = new TaskCompletionSource<int>(); token.Register(() => source.TrySetCanceled()); return source.Task;
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
            public readonly System.Collections.Generic.List<int> Commands = new System.Collections.Generic.List<int>();
            public int MaxConcurrentSends;
            public bool Connected { get; private set; }
            public Task ConnectAsync(string host, int port, CancellationToken token) { Connected = true; return Task.CompletedTask; }
            public async Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken token)
            {
                int active = Interlocked.Increment(ref _activeSends);
                MaxConcurrentSends = Math.Max(MaxConcurrentSends, active);
                try
                {
                    var reader = new PacketReader(1024);
                    reader.Append(data, offset, count);
                    if (!reader.TryRead(out NetworkPacket packet)) throw new Exception("send packet decode failed");
                    Commands.Add(packet.Command);
                    await Task.Delay(20, token);
                    return count;
                }
                finally
                {
                    Interlocked.Decrement(ref _activeSends);
                }
            }
            public Task<int> ReceiveAsync(byte[] buffer, CancellationToken token)
            {
                var source = new TaskCompletionSource<int>(); token.Register(() => source.TrySetCanceled()); return source.Task;
            }
            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }
    }
}
#endif
