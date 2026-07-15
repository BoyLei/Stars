using SGF.Network;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// 心跳包工厂：构造游戏自定义的 HeartBeat 自定义消息（Delay 字段客户端固定为 0），
    /// 并用 ByteStream 序列化。对应旧网络 Ping.SendHeartBeatMsg 中的 heartBeat 结构。
    /// </summary>
    public static class GameHeartbeatPacketFactory
    {
        public static NetworkOutboundPacket Create()
        {
            var hb = new HeartBeat { Delay = 0 };

            var bs = new ByteStream();
            if (!bs.Serialize(hb))
            {
                SGF.Debuger.LogError("[V2Adapter] GameHeartbeatPacketFactory 序列化 HeartBeat 失败");
                return null;
            }

            return new NetworkOutboundPacket(V2GameCommandIds.HeartBeat, bs.data, false);
        }
    }
}
