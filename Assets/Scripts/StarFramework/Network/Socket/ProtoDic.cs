using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System;
using Google.Protobuf.Reflection;
using System.Reflection;

namespace SGF.Network
{

    [XLua.LuaCallCSharp]
    public enum ServerType
    {
        // ServerTypeGateway 网关服
        ServerTypeGateway = 1,
        // ServerTypeClient 客户端
        ServerTypeClient,
        // ServerTypeSpace  带场景的服务器
        ServerTypeSpace,

        // ServerTypeLogin 登录服务器
        ServerTypeLogin = 5,
        // ServerTypeLobby 大厅服务器
        ServerTypeLobby,
        // ServerTypeDB 数据保存服务器
        ServerTypeDB,
        // ServerTypeCenter 中心服务器
        ServerTypeCenter,
        // ServerTypeMatch 匹配服务器
        ServerTypeMatch,
        // ServerTypeScene 主城服务器
        ServerTypeScene,
        // ServerTypeLinker 链接服务器(与外部服务通信)
        ServerTypeLinker,
        // ServerTypeInstance 副本服务器
        ServerTypeInstance,
        // ServerTypeInter 交互服务器(组队,排行榜啥的)
        ServerTypeInter
    }

    public struct ProtoInfo
    {
        // public string FullName;
        public string Name;
        public int cmd;
        public MessageParser parse;

        public byte ServerType;

    }
    /// <summary>
    /// 同时具有，静态，弱（唯一，被动式）性，【√弱（服务性）性】
    /// TODO：应该对结构进行整合,静态 或 整合的服务类
    /// </summary>
    public class ProtoDic : Singleton<ProtoDic>
    {
        private Dictionary<int, ProtoInfo> cmd2protoDic = new Dictionary<int, ProtoInfo>();


        private Dictionary<string, ProtoInfo> name2protoDic = new Dictionary<string, ProtoInfo>();


        public override void Init()
        {
            base.Init();
            InitDic();
        }

        public ProtoInfo? GetProtoInfoByCmd(int cmd)
        {
            ProtoInfo protoInfo;
            if (cmd2protoDic.TryGetValue(cmd, out protoInfo))
            {
                return protoInfo;
            }
            return null;
        }

        public ProtoInfo? GetProtoInfoByName(string name)
        {
            ProtoInfo protoInfo;
            if (name2protoDic.TryGetValue(name, out protoInfo))
            {
                return protoInfo;
            }
            return null;

        }

        public int GetCMDByName(string name)
        {
            ProtoInfo protoInfo;
            if (name2protoDic.TryGetValue(name, out protoInfo))
            {
                return protoInfo.cmd;
            }
            return 0;
        }

        //TODO:dl
        //根据协议的cmd得到cmd对应的gameServer，需要服务器优先提供一个配置，目前服务器缺失这个配置
        private ServerType GetServerType(int cmd)
        {
            //TODO:dl
            //需要根据配置获取对应的cm服务器类型
            return ServerType.ServerTypeLobby;
        }

        private void InitDic()
        {
            ProtoMap protoMap = new ProtoMap();
            protoMap.Init();

            var dic = protoMap.ProtoMapDic;
            foreach (KeyValuePair<int, System.Type> kvp in dic)
            {
                ProtoInfo protoInfo = new ProtoInfo();
                protoInfo.cmd = kvp.Key;
                // protoInfo.FullName = LowercaseFirstLetter(kvp.Value.FullName);
                protoInfo.Name = kvp.Value.Name;
                var parseInfo = kvp.Value.GetField("_parser", BindingFlags.Static | BindingFlags.NonPublic);
                protoInfo.parse = (MessageParser)parseInfo.GetValue(null);

                protoInfo.ServerType = (byte)GetServerType(protoInfo.cmd);

                cmd2protoDic.Add(protoInfo.cmd, protoInfo);
                name2protoDic.Add(protoInfo.Name, protoInfo);

            }
        }

        // public static string LowercaseFirstLetter(string input)
        // {
        //     if (string.IsNullOrEmpty(input))
        //     {
        //         return input;
        //     }

        //     // 获取第一个字符并转为小写
        //     char firstChar = char.ToLower(input[0]);

        //     // 如果字符串长度为1，直接返回小写字符
        //     if (input.Length == 1)
        //     {
        //         return firstChar.ToString();
        //     }

        //     // 创建字符数组以提高性能
        //     char[] chars = input.ToCharArray();
        //     chars[0] = firstChar;

        //     // 使用字符串构造函数来避免不必要的字符串拼接
        //     return new string(chars);
        // }
    }

    //TODO: all
    //看到缺的就补上，HandleRpcMsg中会有对应的warning日志
    public enum EnumPBMsgName
    {
        BuffCreateRet,
        BuffEndRet,

        /// <summary>
        /// 同步运行时数据,用于同步runtime数据的消息，包括skill，buff，被动技
        /// </summary>
        RuntimeSyncRet,
        /// <summary>
        /// 使用技能通知
        /// </summary>
        SkillUseRet,

        /// <summary>
        /// 运行时黑板
        /// </summary>
        RunBlackRet,
        /// <summary>
        /// 技能运行结束了
        /// </summary>
        SkillEndRet,
        /// <summary>
        /// 技能主运退出
        /// </summary>
        SkillQuitReq,
        /// <summary>
        /// 子弹创建
        /// </summary>
        BulletCreateRet,
        /// <summary>
        /// 自动结束
        /// </summary>
        BulletEndRet,

        /// <summary>
        /// 死亡/复活
        /// </summary>
        ChangeDeadState,
        /// <summary>
        /// 角色复活
        /// </summary>
        RoleReviveRet,
        /// <summary>
        /// 被动技能通知 (使用被动技能的通知)
        /// 服务器创建新的运行时回复,或者断线重连同步一次
        /// </summary>
        PassiveSkillUseRet,
        /// <summary>
        /// //被动技能运行结束了,(技能结束会发，技能无法释放也会)s->c 
        /// </summary>
        PassiveSkillEndRet,

        /// <summary>
        /// 物件交互
        /// </summary>
        InterRet,

        /// <summary>
        /// 技能阶段
        /// </summary>
        RunStageRet,

        CDUpdateNotice,
        RunStageForceEndRet,
    }
}
