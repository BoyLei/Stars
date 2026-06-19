using System;
using System.Collections.Generic;

namespace SGF.Network
{
    /// <summary>
    /// 自定义消息：节省流量
    /// </summary>
    public struct ClientVerifyReq
    {
        public byte Source;
        public ulong PID;

        public string Token;

        public byte SessState;
    }

    public struct ClientVerifySucceedRet
    {
        public byte Source;
        public ulong PID;

        public ulong SourceID;

        public byte Type;
    }
    public struct ClientVerifyFailedRet
    {

    }

    public struct HeartBeat
    {
        public byte Delay;
    }

    public struct UserDuplicateLoginNotify
    {
    }

    public struct BattleClientVerifyReq
    {
        public ulong PID;
        public string Token;
        public byte SessState;
    }

    public struct RPCMsg
    {
        public byte ServerType;
        public ulong SrcEntityID;
        public string MethodName;
        public byte[] Data;
    }

    /// <summary>
    /// 这个只是verify协议验证
    /// </summary>
    public struct BattleClientVerifyRet
    {
        public ulong SpaceID; //地图ID spaceid是场景唯一id ，相当于每次创建新场景给他的一个 不重复id ；这个id是用来方便管理场景的，但我现在要找一个例子，说客户端用这个字段
        public byte IsNew;  //isnew 是代表是新建session，还是断线重连 ；1是新建，2是断线重连的
        //资产MapId 例如地图 0
    }


    public struct BattleEnityAOINoti
    {
        public ulong PID;
        public string Token;
        public byte SessState;
    }

    /// <summary>
    /// 让服务器给的 客户端主动通知服务器 关闭 socket 的消息, 服务器不走正常的踢下线，而是直接关闭 socket
    /// </summary>
    public struct Hangup
    {

    }


}
