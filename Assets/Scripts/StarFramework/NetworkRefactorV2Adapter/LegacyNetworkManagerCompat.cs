using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using ProtoMsg;
using SGF.Network;
using StarProject.Game;
using StarProjectDef;
using UnityEngine;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// V2 兼容旧 NetworkManager / SocketBase 调用面的薄门面。
    /// 只准备移植入口，不接管 AppMain，也不替换旧 NetworkManager.Instance。
    /// </summary>
    [XLua.LuaCallCSharp]
    public sealed class LegacyNetworkManagerCompat : IDisposable
    {
        private readonly GameNetworkAdapter _adapter;
        private readonly CSharpNetworkApi _api;
        private readonly Dictionary<SubscriptionKey, IDisposable> _tokens = new Dictionary<SubscriptionKey, IDisposable>();
        private readonly Dictionary<object, List<SubscriptionKey>> _ownerKeys = new Dictionary<object, List<SubscriptionKey>>();
        private readonly ProtoMsg.Client2CenterReq _client2CenterReq = new ProtoMsg.Client2CenterReq();
        private readonly Dictionary<int, MessageHandle> _preStaticHandlers = new Dictionary<int, MessageHandle>();
        private readonly Dictionary<int, IDisposable> _preStaticTokens = new Dictionary<int, IDisposable>();
        private readonly Action<MessageHandleData> _legacySideEffects;
        private readonly Action<MessageHandleData> _serverMessageReporter;
        private ReconnectSessionData _lastSession;
        private bool _disposed;

        public bool IsMessageLock => _adapter.Hub.IsMessageLocked;
        public event Action<bool> OnSocketClose;
        public event Action<SocketViewState> SocketViewStateChanged;

        public LegacyNetworkManagerCompat(GameNetworkAdapter adapter)
            : this(adapter, null, null)
        {
        }

        internal LegacyNetworkManagerCompat(
            GameNetworkAdapter adapter,
            Action<MessageHandleData> legacySideEffects,
            Action<MessageHandleData> serverMessageReporter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _api = new CSharpNetworkApi(adapter.Hub);
            _legacySideEffects = legacySideEffects ?? RunLegacySideEffects;
            _serverMessageReporter = serverMessageReporter ?? ReportServerMessageToQASDK;
            _adapter.Hub.PacketDispatching += OnPacketDispatching;
            _adapter.OnSocketClose += HandleSocketClose;
            _adapter.SocketViewStateChanged += HandleSocketViewStateChanged;
        }

        public void Init()
        {
        }

        public Task StartAsync(ReconnectSessionData session, CancellationToken cancellationToken = default)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            _lastSession = session;
            return _adapter.StartAsync(session, cancellationToken);
        }

        public void Start(string ip, int port, ulong pid, string token, ProtocolType protocolType = ProtocolType.Tcp)
        {
            if (protocolType != ProtocolType.Tcp)
            {
                Debug.LogWarning($"[V2Compat] protocolType {protocolType} is not supported by main TCP adapter.");
                return;
            }
            _ = StartAsync(new ReconnectSessionData($"{ip}:{port}", pid, token, false));
        }

        public void Start(string url, ulong pid, string token, ProtocolType protocolType = ProtocolType.Tcp)
        {
            if (protocolType != ProtocolType.Tcp)
            {
                Debug.LogWarning($"[V2Compat] protocolType {protocolType} is not supported by main TCP adapter.");
                return;
            }
            _ = StartAsync(new ReconnectSessionData(url, pid, token, false));
        }

        public void Close()
        {
            End();
        }

        public void Release()
        {
            Dispose();
        }

        public void End()
        {
            Dispose();
        }

        public void ReStart()
        {
            if (_disposed || _lastSession == null) return;
            Task task = _adapter.RecoverAndReconnectAsync(_lastSession);
            task.ContinueWith(t => Debug.LogWarning($"[V2Compat] ReStart failed: {t.Exception?.GetBaseException().Message}"), TaskContinuationOptions.OnlyOnFaulted);
        }

        public void TestReconnect()
        {
            ReStart();
        }

        public void ReStartPing()
        {
            ResetPingState();
        }

        public void ReSetPing()
        {
            ResetPingState();
        }

        public void TestPing()
        {
            Debug.Log("[V2Compat] TestPing is a legacy debug toggle; no-op in V2 compat.");
        }

        public void OnMessageCmd(int cmd, MessageHandle callback, object target, bool isGroup = false)
        {
            if (callback == null) return;
            SubscriptionKey key = new SubscriptionKey(cmd, callback, target, isGroup);
            if (_tokens.ContainsKey(key)) return;

            IDisposable token = isGroup
                ? _api.SubscribeRpcGroup(GameNetworkAdapter.DefaultSocketName, cmd, p => DispatchPacket(p, callback), target)
                : _api.Subscribe(GameNetworkAdapter.DefaultSocketName, cmd, p => DispatchPacket(p, callback), target);
            _tokens.Add(key, token);
            AddOwnerKey(target, key);
        }

        public void OnMessageEnum(MsgIDEnum messageEnum, MessageHandle callback, object target, bool isGroup = false)
        {
            OnMessageCmd((int)messageEnum, callback, target, isGroup);
        }

        public void OffMessageCmd(int cmd, MessageHandle callback, object target, bool isGroup = false)
        {
            SubscriptionKey key = new SubscriptionKey(cmd, callback, target, isGroup);
            if (!_tokens.TryGetValue(key, out IDisposable token)) return;

            token.Dispose();
            _tokens.Remove(key);
            RemoveOwnerKey(target, key);
        }

        public void OffMessageEnum(MsgIDEnum messageEnum, MessageHandle callback, object target, bool isGroup = false)
        {
            OffMessageCmd((int)messageEnum, callback, target, isGroup);
        }

        public void OffTargetMessage(object target)
        {
            if (target == null || !_ownerKeys.TryGetValue(target, out List<SubscriptionKey> keys)) return;

            foreach (SubscriptionKey key in keys.ToArray())
            {
                if (_tokens.TryGetValue(key, out IDisposable token)) token.Dispose();
                _tokens.Remove(key);
            }
            _ownerKeys.Remove(target);
            _api.UnsubscribeOwner(target);
        }

        public void LockMessage(bool isLock)
        {
            _api.LockMessages(isLock);
        }

        public void OnPreStaticMessage(int cmd, MessageHandle callback)
        {
            if (callback == null) return;
            if (_preStaticHandlers.ContainsKey(cmd)) _preStaticHandlers[cmd] += callback;
            else _preStaticHandlers.Add(cmd, callback);
            if (!_preStaticTokens.ContainsKey(cmd))
            {
                IDisposable token = _api.SubscribeImmediate(
                    GameNetworkAdapter.DefaultSocketName,
                    cmd,
                    p => PreHandleMessageHandleData(ToMessageHandleData(p)),
                    this);
                _preStaticTokens.Add(cmd, token);
            }
        }

        public void PreHandleMessageHandleData(MessageHandleData messageHandleData)
        {
            if (messageHandleData == null) return;
            if (_preStaticHandlers.TryGetValue(messageHandleData.messageCmd, out MessageHandle callback))
            {
                callback?.Invoke(messageHandleData);
            }
        }

        public void HandleMessageHandleData(MessageHandleData messageHandleData, bool immediately)
        {
            if (messageHandleData == null) return;
            _legacySideEffects(messageHandleData);
        }

        public void SendEnumPbByteMsg(MsgIDEnum enumCmd, IMessage message, bool isEncrypt = false)
        {
            if (message == null) return;
            _ = _api.SendProtobuf(GameNetworkAdapter.DefaultSocketName, (int)enumCmd, message, m => m.ToByteArray(), isEncrypt);
        }

        public void SendClient2CenterMsg(IMessage message)
        {
            if (message == null) return;
            _client2CenterReq.MsgName = message.GetType().Name;
            _client2CenterReq.MsgData = message.ToByteString();
            SendEnumPbByteMsg(MsgIDEnum.Client2CenterReqID, _client2CenterReq);
        }

        public void SendRPCMsg(ServerType serverType, IMessage message, bool isEncrypt = false, ulong oneOfCtrlEntityId = 0, bool isAutoChangeMsgTarget = true, bool isSend2QASDK = true)
        {
            if (message == null) return;
            if (isAutoChangeMsgTarget && (serverType == ServerType.ServerTypeInstance || serverType == ServerType.ServerTypeScene))
            {
                serverType = ServerType.ServerTypeSpace;
            }

            string messageName = message.GetType().Name;
            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByName(messageName);
            if (!protoInfo.HasValue)
            {
                Debug.LogError($"[V2Compat] SendRPCMsg name : {messageName} has no protoinfo");
                return;
            }

            // 构造 RPCMsg struct 并 ByteStream 序列化，与原始 SocketBase.SendRPCMsg 线格式一致：
            // RPCMsg{ServerType:byte, SrcEntityID:ulong, MethodName:string, Data:byte[]}
            // Data = SocketUtils.RPCMsgSerializer = [14][cmd:2][len:2][payload]
            byte[] rpcInnerData = SocketUtils.RPCMsgSerializer(message.ToByteArray(), protoInfo.Value.cmd);
            RPCMsg rpcMsg = new RPCMsg
            {
                ServerType = (byte)serverType,
                SrcEntityID = oneOfCtrlEntityId,
                MethodName = protoInfo.Value.Name,
                Data = rpcInnerData
            };
            ByteStream rpcStream = new ByteStream();
            if (!rpcStream.Serialize(rpcMsg))
            {
                Debug.LogError($"[V2Compat] SendRPCMsg ByteStream.Serialize failed for {messageName}");
                return;
            }
            _ = _api.Send(GameNetworkAdapter.DefaultSocketName, V2GameCommandIds.RPCMsg, rpcStream.data, isEncrypt);

            if (isSend2QASDK)
            {
                QASDKMsgUtils.SendClientRpcPb2QASDK(serverType, protoInfo.Value.cmd, message, isEncrypt, oneOfCtrlEntityId, isAutoChangeMsgTarget);
            }
        }

        public void SendRPCMsgLua(int cmd, byte[] data, bool isEncrypt = false)
        {
            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(cmd);
            if (!protoInfo.HasValue) return;

            // Lua RPC 同样需要 RPCMsg struct 封装，ServerType 沿用 ProtoInfo.ServerType。
            byte[] rpcInnerData = SocketUtils.RPCMsgSerializer(data, protoInfo.Value.cmd);
            RPCMsg rpcMsg = new RPCMsg
            {
                ServerType = protoInfo.Value.ServerType,
                SrcEntityID = 0,
                MethodName = protoInfo.Value.Name,
                Data = rpcInnerData
            };
            ByteStream rpcStream = new ByteStream();
            if (!rpcStream.Serialize(rpcMsg)) return;
            _ = _api.Send(GameNetworkAdapter.DefaultSocketName, V2GameCommandIds.RPCMsg, rpcStream.data, isEncrypt);
        }

        public void SendCustomMsg(int cmd, object msg, bool isEncrypt = false, int messageCmd = 0)
        {
            if (msg == null) return;
            ByteStream stream = new ByteStream();
            if (!stream.Serialize(msg)) return;
            _ = _api.Send(GameNetworkAdapter.DefaultSocketName, cmd, stream.data, isEncrypt);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _adapter.Hub.PacketDispatching -= OnPacketDispatching;
            _adapter.OnSocketClose -= HandleSocketClose;
            _adapter.SocketViewStateChanged -= HandleSocketViewStateChanged;
            foreach (IDisposable token in _tokens.Values) token.Dispose();
            foreach (IDisposable token in _preStaticTokens.Values) token.Dispose();
            _tokens.Clear();
            _ownerKeys.Clear();
            _preStaticTokens.Clear();
            _preStaticHandlers.Clear();
            _adapter.Dispose();
        }

        private static MessageHandleData ToMessageHandleData(NetworkPacket packet)
        {
            MessageData messageData = new MessageData
            {
                cmd = packet.Command,
                data = packet.Payload,
                serializeType = packet.SerializeType,
                socketName = GameNetworkAdapter.DefaultSocketName
            };

            MessageHandleData data = new MessageHandleData { socketName = messageData.socketName };
            if (packet.IsRpc)
            {
                IMessage pb = ProtoUtils.Deserialize(packet.MessageCommand, packet.Payload);
                data.isRPC = true;
                data.enityId = packet.EntityId;
                data.messageCmd = packet.MessageCommand;
                data.data = pb;
                data.DeserializedData = pb;
                data.messageName = pb != null ? pb.Descriptor.Name : null;
                return data;
            }

            if (CustomMsg.Instance.GetEnumByCmd(packet.Command) != CustomMsgID.None)
            {
                Type type = CustomMsg.Instance.GetStructByCmd(packet.Command);
                bool ok;
                object custom = new ByteStream().DeSerializeType(type, packet.Payload, out ok);
                if (ok)
                {
                    data.data = custom;
                    data.DeserializedData = custom;
                    data.messageCmd = packet.Command;
                    data.messageName = type != null ? type.Name : null;
                    return data;
                }
            }

            ProtoUtils.DeserializePbMsg(messageData, data);
            return data;
        }

        private void DispatchPacket(NetworkPacket packet, MessageHandle callback)
        {
            MessageHandleData data = ToMessageHandleData(packet);
            callback(data);
        }

        private void OnPacketDispatching(string socketName, NetworkPacket packet)
        {
            MessageHandleData data = ToMessageHandleData(packet);
            InvokeSafely(_legacySideEffects, data, "legacy side effects");
            InvokeSafely(_serverMessageReporter, data, "server QASDK report");
        }

        private void HandleSocketClose(bool forceClose)
        {
            OnSocketClose?.Invoke(forceClose);
        }

        private void HandleSocketViewStateChanged(SocketViewState state)
        {
            SocketViewStateChanged?.Invoke(state);
        }

        private void ResetPingState()
        {
            if (_disposed) return;
            _adapter.Ping.Reset();
            _adapter.Connection.PingHealth.Reset();
        }

        private static void RunLegacySideEffects(MessageHandleData messageHandleData)
        {
            if (messageHandleData.messageCmd == (int)MsgIDEnum.DBUpUserDatasReqID)
            {
                FixMessageManager.Instance.HandleFixMessageCS(messageHandleData.data as DBUpUserDatasReq);
            }

            if (messageHandleData.messageCmd == (int)MsgIDEnum.Client2ThirdRetID)
            {
                Client2ThirdMsgManager.Instance.HandleMessageCS(messageHandleData.data as Client2ThirdRet);
            }

            if (messageHandleData.isRPC)
            {
                GameManager.Instance.HandleRPCMsg(messageHandleData);
            }

            GameManager.Instance.OnRetMsg(messageHandleData.messageCmd);
        }

        private static void ReportServerMessageToQASDK(MessageHandleData messageHandleData)
        {
            IMessage pbMsg = messageHandleData.data as IMessage;
            if (pbMsg == null) pbMsg = messageHandleData.DeserializedData as IMessage;
            if (pbMsg == null) return;
            QASDKMsgUtils.SendServerMessage2QASDK(messageHandleData.isRPC, true, false, messageHandleData.messageCmd, pbMsg);
        }

        private static void InvokeSafely(Action<MessageHandleData> action, MessageHandleData data, string name)
        {
            try
            {
                action(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[V2Compat] {name} failed for cmd {data.messageCmd}: {ex.Message}");
            }
        }

        private void AddOwnerKey(object owner, SubscriptionKey key)
        {
            if (owner == null) return;
            if (!_ownerKeys.TryGetValue(owner, out List<SubscriptionKey> keys))
            {
                keys = new List<SubscriptionKey>();
                _ownerKeys.Add(owner, keys);
            }
            keys.Add(key);
        }

        private void RemoveOwnerKey(object owner, SubscriptionKey key)
        {
            if (owner == null || !_ownerKeys.TryGetValue(owner, out List<SubscriptionKey> keys)) return;
            keys.Remove(key);
            if (keys.Count == 0) _ownerKeys.Remove(owner);
        }

        private struct SubscriptionKey : IEquatable<SubscriptionKey>
        {
            private readonly int _cmd;
            private readonly MessageHandle _callback;
            private readonly object _target;
            private readonly bool _isGroup;

            public SubscriptionKey(int cmd, MessageHandle callback, object target, bool isGroup)
            {
                _cmd = cmd;
                _callback = callback;
                _target = target;
                _isGroup = isGroup;
            }

            public bool Equals(SubscriptionKey other)
            {
                return _cmd == other._cmd && Equals(_callback, other._callback) && ReferenceEquals(_target, other._target) && _isGroup == other._isGroup;
            }

            public override bool Equals(object obj)
            {
                return obj is SubscriptionKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = _cmd;
                    hash = (hash * 397) ^ (_callback != null ? _callback.GetHashCode() : 0);
                    hash = (hash * 397) ^ (_target != null ? _target.GetHashCode() : 0);
                    hash = (hash * 397) ^ _isGroup.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
