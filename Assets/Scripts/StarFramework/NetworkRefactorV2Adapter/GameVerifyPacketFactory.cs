using SGF.Network;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// 验证包工厂：将 V2 的 ReconnectSessionData（address/pid/token/reconnect）转换为游戏自定义的
    /// ClientVerifyReq 自定义消息，并用 ByteStream 序列化（与旧网络层 SendCustomMsg 完全一致）。
    /// 对应旧网络 SocketItem 构造函数中的 clientVerifyReq 赋值。
    /// </summary>
    public static class GameVerifyPacketFactory
    {
        /// <summary>
        /// 构建验证包。Source 固定为 0（客户端来源）；SessState 与 BattleClientVerifyRet.IsNew 语义一致：
        /// 1 = 新会话，2 = 断线重连。
        /// </summary>
        public static NetworkOutboundPacket Create(ReconnectSessionData session)
        {
            var req = new ClientVerifyReq
            {
                Source = 0,
                PID = session.Pid,
                Token = session.Token ?? string.Empty,
                SessState = session.IsReconnectSession ? (byte)2 : (byte)1
            };

            var bs = new ByteStream();
            if (!bs.Serialize(req))
            {
                SGF.Debuger.LogError("[V2Adapter] GameVerifyPacketFactory 序列化 ClientVerifyReq 失败");
                return null;
            }

            return new NetworkOutboundPacket(V2GameCommandIds.ClientVerifyReq, bs.data, false);
        }
    }
}
