using ProtoMsg;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// RPC 解码器：V2 接收循环把原始包（command=RpcMsgID=1001, payload 为 protobuf RpcMsg）交给本解码器。
    /// 注意：游戏 RPC 接收端是 protobuf 封装（ProtoMsg.RpcMsg），不是客户端发送端的 [14] 格式，
    /// 因此必须用 ProtoMsg.RpcMsg.Parser 解析，而不能用 RpcCodec.TryDecode。
    /// 解析后构造带 IsRpc / MessageCommand / EntityId 的 NetworkPacket，payload 替换为内层 MsgData。
    /// RPC 时实体 ID 取自 rpcMsg.SrcEntityID（非 RPC 消息实体 ID 默认 0，见 ProtoUtils.DeserializePbMsg）。
    /// </summary>
    public static class GameRpcDecoder
    {
        public static NetworkPacket Decode(NetworkPacket packet)
        {
            if (packet.Command != V2GameCommandIds.RpcMsgID) return null;

            RpcMsg rpcMsg;
            try
            {
                rpcMsg = RpcMsg.Parser.ParseFrom(packet.Payload);
            }
            catch (System.Exception ex)
            {
                SGF.Debuger.LogError($"[V2Adapter] GameRpcDecoder 解析 RpcMsg 失败: {ex.Message}");
                return null;
            }

            return new NetworkPacket(
                (ushort)V2GameCommandIds.RpcMsgID,
                packet.SerializeType,
                rpcMsg.MsgData.ToByteArray(),
                (int)rpcMsg.MsgID,
                rpcMsg.SrcEntityID);
        }
    }
}
