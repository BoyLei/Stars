using System;
using System.Collections.Generic;

namespace SGF.Network
{
    /// <summary>
    /// 同时具有，静态，弱（唯一，被动式）性，【√弱（服务性）性】
    /// TODO：应该对结构进行整合,静态 或 整合的服务类
    /// </summary>
    public partial class CustomMsg : Singleton<CustomMsg>
    {
        public Dictionary<int, Type> cmd2Struct = new Dictionary<int, Type>();
        public Dictionary<int, CustomMsgID> cmd2MsgEnum = new();

        public override void Init()
        {
            base.Init();
            Array arrays = Enum.GetValues(typeof(CustomMsgID));
            string[] names = Enum.GetNames(typeof(CustomMsgID));
            for (int i = 0; i < arrays.LongLength; i++)
            {
                int value = (int)arrays.GetValue(i);

                // 0 是 默认的 枚举,没有任何含义, 所以也没有结构
                if (value != 0)
                {
                    Type t = Type.GetType("SGF.Network." + names[i]);
                    if (t == null)
                    {
                        Debuger.LogError("SGF.Network." + names[i] + "Is null.请初始化模板");
                    }
                    cmd2Struct.Add(value, t);
                }

                cmd2MsgEnum.Add(value, Enum.Parse<CustomMsgID>(names[i]));
            }


        }

        public Type GetStructByCmd(int cmd)
        {
            Type structType;
            if (cmd2Struct.ContainsKey(cmd))
            {
                cmd2Struct.TryGetValue(cmd, out structType);
                return structType;
            }
            return null;
        }

        public bool Contain(int cmd)
        {
            return cmd2Struct.ContainsKey(cmd);
        }

        public CustomMsgID GetEnumByCmd(int cmd)
        {
            if (cmd2MsgEnum.TryGetValue(cmd, out CustomMsgID cmdEnum))
            {
                return cmdEnum;
            }
            return CustomMsgID.None;
        }


    }

    public enum CustomMsgID : int
    {
        None = 0,

        ClientVerifyReq = 1,  //ClientVerifyReqMsgID 验证消息的ID号
        ClientVerifySucceedRet = 2,    //ClientVerifySucceedRetMsgID 验证结果成功返回的ID号
        ClientVerifyFailedRet = 3,  //ClientVerifyFailedRetMsgID 验证结果失败返回的ID号
        HeartBeat = 4, // HeartBeatMsgID 心跳消息ID

        Hangup = 11,        // 让服务器 关闭socket的通知, 客户端主动发送
        UserDuplicateLoginNotify = 24, //  UserDuplicateLoginNotifyMsgID 玩家重复登录的消息

        RPCMsg = 58, // RPCMsgID RPC消息

        BattleClientVerifyReq = 103,     //场景服的验证协议
        BattleClientVerifyRet = 104,  //场景服的验证回包协议      (跟服务器一起看过了，不用了)

        //#region AOI
        //EnterAOI = 3006,
        //LeaveAOI = 3007,
        //UpdateAOI = 3008,
        //AOIMsg = 3009,
        //#endregion 


    }
}
