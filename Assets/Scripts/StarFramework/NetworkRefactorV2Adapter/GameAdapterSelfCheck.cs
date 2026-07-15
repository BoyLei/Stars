using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using Google.Protobuf;
using ProtoMsg;
using SGF.Network;
using SGF.Time;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// 适配层自检：纯逻辑验证（命令号映射 / RPC 解码 / 验证包 / 心跳包），不依赖 Unity 运行时与网络。
    /// 可在编辑器通过菜单 Network/V2 Adapter SelfCheck 手动运行；也可由集成测试直接调用 Run()。
    /// </summary>
    public static class GameAdapterSelfCheck
    {
        public static string Run()
        {
            var report = new StringBuilder();
            int passed = 0; int failed = 0;

            // 1) 命令号映射与游戏枚举一致
            Check(V2GameCommandIds.ClientVerifyReq == (int)CustomMsgID.ClientVerifyReq, "ClientVerifyReq 映射", report, ref passed, ref failed);
            Check(V2GameCommandIds.ClientVerifySucceedRet == (int)CustomMsgID.ClientVerifySucceedRet, "ClientVerifySucceedRet 映射", report, ref passed, ref failed);
            Check(V2GameCommandIds.ClientVerifyFailedRet == (int)CustomMsgID.ClientVerifyFailedRet, "ClientVerifyFailedRet 映射", report, ref passed, ref failed);
            Check(V2GameCommandIds.HeartBeat == (int)CustomMsgID.HeartBeat, "HeartBeat 映射", report, ref passed, ref failed);
            Check(V2GameCommandIds.RPCMsg == (int)CustomMsgID.RPCMsg, "RPCMsg 映射", report, ref passed, ref failed);
            Check(V2GameCommandIds.RpcMsgID == (int)MsgIDEnum.RpcMsgID, "RpcMsgID 映射", report, ref passed, ref failed);
            Check(V2GameCommandIds.ServerTimeRetID == (int)MsgIDEnum.ServerTimeRetID, "ServerTimeRetID 映射", report, ref passed, ref failed);
            Check(GameNetworkAdapter.DefaultSocketName == "game", "DefaultSocketName=game", report, ref passed, ref failed);

            // 2) RPC 解码：构造 protobuf RpcMsg，模拟服务器推送（command=1001, payload=RpcMsg 字节）
            var rpcMsg = new RpcMsg
            {
                SrcEntityID = 123456,
                MsgID = 1001,
                MsgData = Google.Protobuf.ByteString.CopyFrom(new byte[] { 11, 22, 33 })
            };
            byte[] rpcBytes = rpcMsg.ToByteArray();
            var rawPacket = new NetworkPacket((ushort)V2GameCommandIds.RpcMsgID, 0, rpcBytes);
            NetworkPacket decoded = GameRpcDecoder.Decode(rawPacket);
            Check(decoded != null, "RpcDecoder 非空", report, ref passed, ref failed);
            Check(decoded != null && decoded.IsRpc, "RpcDecoder 标记 IsRpc", report, ref passed, ref failed);
            Check(decoded != null && decoded.MessageCommand == (int)rpcMsg.MsgID, "RpcDecoder MessageCommand", report, ref passed, ref failed);
            Check(decoded != null && decoded.EntityId == rpcMsg.SrcEntityID, "RpcDecoder EntityId(SrcEntityID)", report, ref passed, ref failed);
            Check(decoded != null && decoded.Payload != null && decoded.Payload.Length == 3 && decoded.Payload[2] == 33, "RpcDecoder 内层 Payload", report, ref passed, ref failed);
            Check(GameRpcDecoder.Decode(new NetworkPacket(999, 0, new byte[1])) == null, "RpcDecoder 非 RPC 命令返回 null", report, ref passed, ref failed);

            // 3) 验证包工厂
            var session = new ReconnectSessionData("127.0.0.1:8080", 987654321, "abc-token", false);
            NetworkOutboundPacket verify = GameVerifyPacketFactory.Create(session);
            Check(verify != null, "VerifyPacket 非空", report, ref passed, ref failed);
            Check(verify != null && verify.Command == V2GameCommandIds.ClientVerifyReq, "VerifyPacket 命令号=1", report, ref passed, ref failed);
            Check(verify != null && verify.Payload != null && verify.Payload.Length > 0, "VerifyPacket 有负载", report, ref passed, ref failed);
            Check(GameVerifyPacketFactory.Create(new ReconnectSessionData("h:1", 1, null, true)) != null, "VerifyPacket Token 为 null 不崩", report, ref passed, ref failed);

            // 4) 心跳包工厂
            NetworkOutboundPacket hb = GameHeartbeatPacketFactory.Create();
            Check(hb != null, "HeartBeatPacket 非空", report, ref passed, ref failed);
            Check(hb != null && hb.Command == V2GameCommandIds.HeartBeat, "HeartBeatPacket 命令号=4", report, ref passed, ref failed);
            Check(hb != null && hb.Payload != null && hb.Payload.Length > 0, "HeartBeatPacket 有负载", report, ref passed, ref failed);

            // 5) UI 意图映射
            Check(GameUiIntentBridge.ToSocketViewState(ConnectionUiIntent.None) == SocketViewState.None, "UI None", report, ref passed, ref failed);
            Check(GameUiIntentBridge.ToSocketViewState(ConnectionUiIntent.WeakLoading) == SocketViewState.Connecting, "UI WeakLoading", report, ref passed, ref failed);
            Check(GameUiIntentBridge.ToSocketViewState(ConnectionUiIntent.FakeReconnectDialog) == SocketViewState.PingReconnectBox, "UI FakeReconnectDialog", report, ref passed, ref failed);
            Check(GameUiIntentBridge.ToSocketViewState(ConnectionUiIntent.RealReconnectDialog) == SocketViewState.ReconnectBox, "UI RealReconnectDialog", report, ref passed, ref failed);
            Check(GameUiIntentBridge.ToSocketViewState(ConnectionUiIntent.ReturnLoginDialog) == SocketViewState.RetrunLoginBox, "UI ReturnLoginDialog", report, ref passed, ref failed);

            using (var adapter = new GameNetworkAdapter(transportFactory: () => new NullTransport()))
            using (var compat = new LegacyNetworkManagerCompat(adapter))
            {
                Check(adapter.Connection.Config.MaxReconnectAttempts == 3 && adapter.Connection.Config.MaxTotalReconnectAttempts == 5, "Adapter 重连次数=3/5", report, ref passed, ref failed);
                compat.LockMessage(true);
                Check(compat.IsMessageLock, "LegacyCompat IsMessageLock=true", report, ref passed, ref failed);
                compat.LockMessage(false);
                Check(!compat.IsMessageLock, "LegacyCompat IsMessageLock=false", report, ref passed, ref failed);
                Check(true, "LegacyCompat 构造/释放/锁消息", report, ref passed, ref failed);
            }

            var timeRet = new ServerTimeRet { TimeStamp = 123456789 };
            byte[] serverTimePacket = PacketCodec.Encode(V2GameCommandIds.ServerTimeRetID, timeRet.ToByteArray(), false);
            using (var adapter = new GameNetworkAdapter(transportFactory: () => new ScriptedReceiveTransport(serverTimePacket)))
            using (var compat = new LegacyNetworkManagerCompat(adapter))
            {
                int preStaticCount = 0;
                compat.OnPreStaticMessage(V2GameCommandIds.ServerTimeRetID, data => preStaticCount++);
                adapter.StartAsync(new ReconnectSessionData("127.0.0.1:1", 1, "token", false)).Wait();
                Thread.Sleep(50);
                Check(adapter.TimeSync.Current.ServerTimeStamp == timeRet.TimeStamp, "ServerTime 使用 ImmediateSubscription", report, ref passed, ref failed);
                Check(preStaticCount == 1, "LegacyCompat OnPreStaticMessage 接入 Immediate", report, ref passed, ref failed);
            }

            using (var adapter = new GameNetworkAdapter(transportFactory: () => new NullTransport()))
            {
                FieldInfo sentField = typeof(GameNetworkAdapter).GetField("_lastHeartbeatSentMs", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo heartbeat = typeof(GameNetworkAdapter).GetMethod("OnHeartBeat", BindingFlags.Instance | BindingFlags.NonPublic);
                var packet = new NetworkPacket((ushort)V2GameCommandIds.HeartBeat, 0, new byte[0]);
                for (int i = 0; i < PingHealthMonitor.WeakSampleCount; i++)
                {
                    sentField.SetValue(adapter, TimeUtils.ClientNowStampMilli - PingHealthMonitor.WeakLatencyMilliseconds);
                    heartbeat.Invoke(adapter, new object[] { packet });
                }
                Check(adapter.Connection.PingHealth.State == PingHealthState.Weak, "Heartbeat 更新 Connection PingHealth", report, ref passed, ref failed);
            }

            using (var adapter = new GameNetworkAdapter(transportFactory: () => new NullTransport()))
            {
                int legacySideEffects = 0;
                int qaReports = 0;
                int callbackA = 0;
                int callbackB = 0;
                using (var compat = new LegacyNetworkManagerCompat(adapter, _ => legacySideEffects++, _ => qaReports++))
                {
                    MessageHandle handlerA = _ => callbackA++;
                    MessageHandle handlerB = _ => callbackB++;
                    compat.OnMessageCmd(V2GameCommandIds.HeartBeat, handlerA, compat);
                    compat.OnMessageCmd(V2GameCommandIds.HeartBeat, handlerB, null);
                    adapter.Hub.EnqueueReceived(GameNetworkAdapter.DefaultSocketName, ToPacket(GameHeartbeatPacketFactory.Create()));
                    adapter.Hub.Tick();
                    Check(callbackA == 1 && callbackB == 1, "LegacyCompat 多订阅均收到消息", report, ref passed, ref failed);
                    Check(legacySideEffects == 1 && qaReports == 1, "LegacyCompat 每包副作用/QASDK 只执行一次", report, ref passed, ref failed);

                    compat.LockMessage(true);
                    adapter.Hub.EnqueueReceived(GameNetworkAdapter.DefaultSocketName, ToPacket(GameHeartbeatPacketFactory.Create()));
                    adapter.Hub.Tick();
                    Check(callbackA == 1 && callbackB == 1 && legacySideEffects == 1 && qaReports == 1, "LegacyCompat LockMessage(true) 锁住普通消息", report, ref passed, ref failed);
                    compat.LockMessage(false);
                    adapter.Hub.Tick();
                    Check(callbackA == 2 && callbackB == 2 && legacySideEffects == 2 && qaReports == 2, "LegacyCompat LockMessage(false) 按序 flush", report, ref passed, ref failed);

                    compat.OffMessageCmd(V2GameCommandIds.HeartBeat, handlerB, null);
                    adapter.Hub.EnqueueReceived(GameNetworkAdapter.DefaultSocketName, ToPacket(GameHeartbeatPacketFactory.Create()));
                    adapter.Hub.Tick();
                    Check(callbackA == 3 && callbackB == 2, "LegacyCompat OffMessageCmd 后不再回调", report, ref passed, ref failed);

                    compat.OffTargetMessage(compat);
                    compat.OffTargetMessage(compat);
                    adapter.Hub.EnqueueReceived(GameNetworkAdapter.DefaultSocketName, ToPacket(GameHeartbeatPacketFactory.Create()));
                    adapter.Hub.Tick();
                    Check(callbackA == 3 && callbackB == 2, "LegacyCompat OffTargetMessage 后不再回调", report, ref passed, ref failed);
                    Check(legacySideEffects == 4 && qaReports == 4, "LegacyCompat 无业务订阅也执行每包副作用/QASDK", report, ref passed, ref failed);
                }
            }

            ProtoInfo? serverTimeInfo = ProtoDic.Instance.GetProtoInfoByName(nameof(ServerTimeRet));
            if (serverTimeInfo.HasValue)
            {
                var capture = new CaptureTransport();
                using (var adapter = new GameNetworkAdapter(transportFactory: () => capture))
                using (var compat = new LegacyNetworkManagerCompat(adapter))
                {
                    adapter.Connection.MarkVerified();
                    compat.SendRPCMsg(ServerType.ServerTypeScene, timeRet, false, 77);
                    Thread.Sleep(20);
                    Check(TryReadFirstPacket(capture.Sent, out NetworkPacket sent) && sent.Command == V2GameCommandIds.RPCMsg, "LegacyCompat SendRPCMsg 外层 cmd=58", report, ref passed, ref failed);
                    Check(sent != null && TryDecodeLegacyRpcPayload(sent.Payload, out RPCMsg rpc) && rpc.ServerType == (byte)ServerType.ServerTypeSpace && rpc.SrcEntityID == 77 && rpc.MethodName == serverTimeInfo.Value.Name, "LegacyCompat SendRPCMsg 使用旧 RPCMsg struct", report, ref passed, ref failed);
                    Check(rpc.Data != null && rpc.Data.Length >= 5 && rpc.Data[0] == 14 && rpc.Data[1] == (byte)(serverTimeInfo.Value.cmd & 0xff) && rpc.Data[2] == (byte)((serverTimeInfo.Value.cmd >> 8) & 0xff), "LegacyCompat SendRPCMsg 内层 [14][cmd][len]", report, ref passed, ref failed);
                }
            }

            var centerCapture = new CaptureTransport();
            using (var adapter = new GameNetworkAdapter(transportFactory: () => centerCapture))
            using (var compat = new LegacyNetworkManagerCompat(adapter))
            {
                adapter.Connection.MarkVerified();
                compat.SendClient2CenterMsg(timeRet);
                Thread.Sleep(20);
                Check(TryReadFirstPacket(centerCapture.Sent, out NetworkPacket sent) && sent.Command == (int)MsgIDEnum.Client2CenterReqID, "LegacyCompat SendClient2CenterMsg cmd", report, ref passed, ref failed);
                Check(sent != null && TryDecodeClient2Center(sent.Payload, out Client2CenterReq req) && req.MsgName == nameof(ServerTimeRet) && req.MsgData.Length == timeRet.ToByteArray().Length, "LegacyCompat SendClient2CenterMsg 包装 MsgName/MsgData", report, ref passed, ref failed);
            }

            report.Insert(0, $"V2 Adapter SelfCheck: {passed} passed, {failed} failed\n");
            return report.ToString();
        }

        private static void Check(bool ok, string name, StringBuilder report, ref int passed, ref int failed)
        {
            if (ok) { passed++; }
            else { failed++; report.AppendLine("  [FAIL] " + name); }
        }

        private static bool TryReadFirstPacket(List<byte[]> sent, out NetworkPacket packet)
        {
            packet = null;
            if (sent.Count == 0) return false;
            var reader = new PacketReader(NetworkConnectionConfig.DefaultMaxPacketSize);
            reader.Append(sent[0], 0, sent[0].Length);
            return reader.TryRead(out packet);
        }

        private static NetworkPacket ToPacket(NetworkOutboundPacket packet)
        {
            return new NetworkPacket((ushort)packet.Command, 0, packet.Payload);
        }

        private static bool TryDecodeLegacyRpcPayload(byte[] payload, out RPCMsg rpc)
        {
            bool ok;
            object value = new ByteStream().DeSerializeType(typeof(RPCMsg), payload, out ok);
            rpc = ok && value is RPCMsg msg ? msg : default(RPCMsg);
            return ok && value is RPCMsg;
        }

        private static bool TryDecodeClient2Center(byte[] payload, out Client2CenterReq req)
        {
            req = null;
            try
            {
                req = Client2CenterReq.Parser.ParseFrom(payload);
                return req != null;
            }
            catch
            {
                return false;
            }
        }

        private sealed class NullTransport : INetworkTransport
        {
            public bool Connected => false;
            public System.Threading.Tasks.Task ConnectAsync(string host, int port, System.Threading.CancellationToken cancellationToken) { return System.Threading.Tasks.Task.FromResult(0); }
            public System.Threading.Tasks.Task<int> ReceiveAsync(byte[] buffer, System.Threading.CancellationToken cancellationToken) { return System.Threading.Tasks.Task.FromResult(0); }
            public System.Threading.Tasks.Task<int> SendAsync(byte[] data, int offset, int count, System.Threading.CancellationToken cancellationToken) { return System.Threading.Tasks.Task.FromResult(count); }
            public void Close() { }
            public void Dispose() { }
        }

        private sealed class CaptureTransport : INetworkTransport
        {
            public readonly List<byte[]> Sent = new List<byte[]>();
            public bool Connected => true;
            public System.Threading.Tasks.Task ConnectAsync(string host, int port, CancellationToken cancellationToken) { return System.Threading.Tasks.Task.FromResult(0); }
            public System.Threading.Tasks.Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken) { return System.Threading.Tasks.Task.FromResult(0); }
            public System.Threading.Tasks.Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
            {
                byte[] copy = new byte[count];
                Buffer.BlockCopy(data, offset, copy, 0, count);
                Sent.Add(copy);
                return System.Threading.Tasks.Task.FromResult(count);
            }
            public void Close() { }
            public void Dispose() { }
        }

        private sealed class ScriptedReceiveTransport : INetworkTransport
        {
            private readonly byte[] _data;
            private bool _received;
            public bool Connected { get; private set; }

            public ScriptedReceiveTransport(byte[] data)
            {
                _data = data;
            }

            public System.Threading.Tasks.Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
            {
                Connected = true;
                return System.Threading.Tasks.Task.FromResult(0);
            }

            public System.Threading.Tasks.Task<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
            {
                if (!_received)
                {
                    _received = true;
                    Buffer.BlockCopy(_data, 0, buffer, 0, _data.Length);
                    return System.Threading.Tasks.Task.FromResult(_data.Length);
                }
                var source = new System.Threading.Tasks.TaskCompletionSource<int>();
                cancellationToken.Register(() => source.TrySetCanceled());
                return source.Task;
            }

            public System.Threading.Tasks.Task<int> SendAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
            {
                return System.Threading.Tasks.Task.FromResult(count);
            }

            public void Close() { Connected = false; }
            public void Dispose() { Close(); }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Network/V2 Adapter SelfCheck")]
        private static void RunFromMenu()
        {
            UnityEngine.Debug.Log(Run());
        }
#endif
    }
}
