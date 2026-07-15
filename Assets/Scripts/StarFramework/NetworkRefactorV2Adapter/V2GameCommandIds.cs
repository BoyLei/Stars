using SGF.Network;

namespace SGF.NetworkRefactorV2.Adapter
{
    /// <summary>
    /// V2 网络层与游戏实际协议之间的命令号映射。
    /// 客户端发送侧与服务器推送侧的命令号不同（见 NetworkRefactorV2Plan.md 协议对照）：
    ///   - 验证 / 心跳 / 自定义消息走 CustomMsgID（客户端自定义结构体 + ByteStream 序列化）
    ///   - RPC 发送走 CustomMsgID.RPCMsg(58)，RPC 接收走 MsgIDEnum.RpcMsgID(1001, protobuf 封装)
    /// </summary>
    public static class V2GameCommandIds
    {
        // 客户端 -> 服务器 自定义消息（CustomMsgID）
        public const int ClientVerifyReq = (int)CustomMsgID.ClientVerifyReq;          // 1  验证请求
        public const int ClientVerifySucceedRet = (int)CustomMsgID.ClientVerifySucceedRet; // 2  验证成功回包
        public const int ClientVerifyFailedRet = (int)CustomMsgID.ClientVerifyFailedRet;   // 3  验证失败回包（被踢）
        public const int HeartBeat = (int)CustomMsgID.HeartBeat;                        // 4  心跳
        public const int RPCMsg = (int)CustomMsgID.RPCMsg;                              // 58 客户端发送 RPC

        // 服务器 -> 客户端 protobuf 消息（MsgIDEnum）
        public const int RpcMsgID = (int)MsgIDEnum.RpcMsgID;                            // 1001 RPC 接收（protobuf 封装）
        public const int ServerTimeRetID = (int)MsgIDEnum.ServerTimeRetID;             // 4843 服务器时间同步
    }
}
