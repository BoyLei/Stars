using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Text;
using ProtoMsg;
using System.Reflection;
using System.Collections.Generic;

namespace SGF.Network
{
    public static class ProtoUtils
    {
        private static StringBuilder sb = new StringBuilder();

        #region 【测试字节流】

        // 测试服务器发送的 解密后的，信息字节流
        private static byte[] m_testData = new byte[] { 8, 185, 4, 16, 0, 32, 0, 40, 128, 155, 12, 48, 130, 128, 224, 176, 231, 233, 216, 218, 40, 56, 2, 64, 1, 80, 3, 122, 51, 10, 14, 76, 111, 111, 112, 70, 105, 114, 115, 116, 69, 110, 101, 109, 121, 82, 33, 8, 239, 54, 18, 28, 10, 12, 8, 220, 128, 228, 176, 231, 233, 216, 243, 40, 16, 1, 10, 12, 8, 219, 128, 228, 176, 231, 233, 216, 243, 40, 16, 1, 122, 84, 10, 15, 76, 111, 111, 112, 68, 97, 109, 97, 103, 101, 69, 110, 101, 109, 121, 82, 65, 8, 233, 54, 18, 60, 10, 28, 8, 220, 128, 228, 176, 231, 233, 216, 243, 40, 16, 133, 1, 24, 0, 32, 0, 45, 0, 0, 0, 0, 48, 0, 56, 1, 64, 0, 10, 28, 8, 219, 128, 228, 176, 231, 233, 216, 243, 40, 16, 159, 1, 24, 0, 32, 0, 45, 0, 0, 0, 0, 48, 0, 56, 1, 64, 0 };

        public static string TestDeserialize()
        {
            // 7009是对于的字节流的 协议号ID
            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(7009);
            if (!protoInfo.HasValue)
            {
                return null;
            }
            MessageParser parser = protoInfo.Value.parse;
            IMessage message = parser.ParseFrom(m_testData);
            return message.ToString();
        }

        public static void TestDeserialize2()
        {
            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(7009);
            if (!protoInfo.HasValue)
            {
                return;
            }
            MessageParser parser = protoInfo.Value.parse;
            IMessage message = parser.ParseFrom(m_testData);

            RunStageRet runStageRet = (RunStageRet)message;

            RepeatedField<BlackBoardNode> blackList = runStageRet.BlackList;
            if (blackList.Count > 0)
            {
                foreach (BlackBoardNode item in blackList)
                {
                    object res = DeserializeBlackBoardCommon<object>(item);
                }
            }
        }

        #endregion

        public static MessageParser GetCMDParser(int cmd)
        {
            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(cmd);
            if (!protoInfo.HasValue)
            {
                return null;
            }

            return protoInfo.Value.parse;
        }



        public static IMessage Deserialize(int cmd, byte[] data)
        {
            MessageParser parser = GetCMDParser(cmd);

            if (parser == null)
            {
                return null;
            }

            try
            {
                return parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                sb.Clear();
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append($"{data[i]},");
                }
                Debuger.LogError($"[Deserialize] : CMD {cmd} , error!!!!");
                Debuger.LogError($"[Deserialize] : CMD {cmd} , data : {sb.ToString()} , e: {e.Message}");
                sb.Clear();
                return null;
            }
        }

        public static IMessage Deserialize(MessageData messageData)
        {
            MessageParser parser = GetCMDParser(messageData.cmd);

            if (parser == null)
            {
                return null;
            }

            try
            {
                return parser.ParseFrom(messageData.data);
            }
            catch (System.Exception e)
            {
                sb.Clear();
                for (int i = 0; i < messageData.data.Length; i++)
                {
                    sb.Append($"{messageData.data[i]},");
                }
                Debuger.LogError($"[Deserialize] : CMD {messageData.cmd} ,e={e} error!!!!");
                Debuger.LogError($"[Deserialize] : CMD {messageData.cmd} , data : {sb.ToString()}");
                Debuger.LogError($"[Deserialize] : CMD {messageData.cmd} , socket:{messageData.socketName},AddMessageDataCMD:{messageData.cmd},serializeType:{messageData.serializeType},buffLength:{messageData.buffLength},dataLength:{messageData.dataLength},curBuffPosition:{messageData.curBuffPosition},minBuffLen:{messageData.minBuffLen},allData:{string.Join(",", messageData.allData)}");
                sb.Clear();
                return null;
            }
        }

        public static IMessage Deserialize(int cmd, string json)
        {
            MessageParser parser = GetCMDParser(cmd);

            if (parser == null)
            {
                return null;
            }

            try
            {
                return parser.ParseJson(json);
            }
            catch (System.Exception e)
            {
                Debuger.LogError($"[Deserialize] : CMD {cmd} ,e={e} error!!!!");
                Debuger.LogError($"[Deserialize] : CMD {cmd} , data : {json}");
                return null;
            }
        }

        public static IMessage DeserializeBlackBoard(ProtoMsg.BlackBoardNode bbNode)
        {
            object val = bbNode.GetType().GetProperty(bbNode.PropValueCase.ToString()).GetValue(bbNode, null);
            ProtoMsg.RawMsg rawValue = (ProtoMsg.RawMsg)val;
            byte[] msgData = rawValue.MsgValue.ToByteArray();
            IMessage pbMessage = ProtoUtils.Deserialize((int)rawValue.MsgID, msgData);
            return pbMessage;
        }

        public static T DeserializeBlackBoard<T>(ProtoMsg.BlackBoardNode bbNode)
        {
            object val = bbNode.GetType().GetProperty(bbNode.PropValueCase.ToString()).GetValue(bbNode, null);
            ProtoMsg.RawMsg rawValue = (ProtoMsg.RawMsg)val;
            byte[] msgData = rawValue.MsgValue.ToByteArray();
            IMessage pbMessage = ProtoUtils.Deserialize((int)rawValue.MsgID, msgData);
            return (T)pbMessage;
        }

        public static bool isRPCCMD(int cmd)
        {
            return (int)MsgIDEnum.RpcMsgID == cmd;
        }

        public static bool DeserializePbMsg(MessageData messageData, MessageHandleData messageHandleData)
        {
            int cmd = messageData.cmd;
            IMessage pbMessage = Deserialize(messageData);
            if (pbMessage == null)
            {
                SGF.Debuger.LogError($"[ProtoUtils] DeserializePbMsg cmd : {cmd} data={messageData.data}  error!!");
                return false;
            }
            messageHandleData.messageCmd = cmd;




            bool isRpc = isRPCCMD(cmd);

            if (isRpc)
            {
                ProtoMsg.RpcMsg rpcMsg = (ProtoMsg.RpcMsg)pbMessage;
                int messageCmd = (int)rpcMsg.MsgID;
                ulong enityId = rpcMsg.SrcEntityID;
                byte[] msgData = rpcMsg.MsgData.ToByteArray();

                //将rpc的pbMessage替换为 实际data的 pbMessage
                pbMessage = ProtoUtils.Deserialize(messageCmd, msgData);

                if (pbMessage == null)
                {
                    SGF.Debuger.LogError($"[ProtoUtils] DeserializePbMsg rpc messageCmd : {messageCmd},msgData={msgData},enityId={enityId} error!!");
                    return false;
                }
                else
                {
                    //Debug.Log($"{flagKey} DeserializePbMsg isRpc: {isRpc} cmd : {cmd} , messageCmd : {messageCmd}, {pbMessage.Descriptor.Name}  , enityId : {enityId} time {TimeUtils.ServerNowStampMilli}");
                }

                //给rpc消息赋予额外的数据
                messageHandleData.isRPC = isRpc;
                messageHandleData.enityId = enityId;
                messageHandleData.messageCmd = messageCmd;


            }
            else
            {
                // 增加默认值
                messageHandleData.enityId = 0;
            }

            var protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(messageHandleData.messageCmd);
            if (!protoInfo.HasValue)
            {
                messageHandleData.messageName = pbMessage.Descriptor.Name;
                SGF.Debuger.LogError($"[ProtoUtils] DeserializePbMsg rpc messageCmd : {messageHandleData.messageCmd}, 没有对应的 protoInfo , 绝对有问题！！！");
            }
            else
            {
                messageHandleData.messageName = protoInfo.Value.Name;
            }




            messageHandleData.data = pbMessage;


            return true;
        }

        // public

        public static T DeserializeBlackBoardCommon<T>(ProtoMsg.BlackBoardNode bbNode)
        {
            // 如果是 none 类型, 表明服务器没有给
            if (bbNode.PropValueCase == BlackBoardNode.PropValueOneofCase.None)
            {
                SGF.Debuger.LogError($"[DeserializeBlackBoardCommon] BlackBoardNode key : {bbNode.Key} 服务器给的类型为 None, 通知夏哥要过滤掉!!!");
                throw new Exception("解析 黑板 key : {bbNode.Key} 的类型为 null");
            }

            PropertyInfo v = bbNode.GetType().GetProperty(bbNode.PropValueCase.ToString());

            try
            {
                object val = v.GetValue(bbNode);

                // val = null;
                if (bbNode.PropValueCase == BlackBoardNode.PropValueOneofCase.RawValue)
                {
                    ProtoMsg.RawMsg rawValue = (ProtoMsg.RawMsg)val;
                    byte[] msgData = rawValue.MsgValue.ToByteArray();
                    IMessage pbMessage = ProtoUtils.Deserialize((int)rawValue.MsgID, msgData);
                    return (T)pbMessage;
                }
                else
                {
                    return (T)val;
                }
            }
            catch (System.Exception e)
            {
                SGF.Debuger.LogError($"[DeserializeBlackBoardCommon] BlackBoardNode key : {bbNode.Key} ,e={e} value error!!!");
                throw e;
            }
        }

        public static ProtoMsg.Vector3 DeserializePbPos(object v)
        {
            ByteString vector3Msg = (ByteString)v;
            byte[] msgData = vector3Msg.ToByteArray();
            IMessage pbMessage = ProtoUtils.Deserialize((int)MsgIDEnum.Vector3ID, msgData);
            ProtoMsg.Vector3 pbV3 = (ProtoMsg.Vector3)pbMessage;
            return pbV3;
        }


        /// <summary>
        ///  convert unity V3 to proto V3
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns>Proto V3</returns>
        public static Vector3 ConvertUnityVec3ToProtoVec3(UnityEngine.Vector3 vector3)
        {
            Vector3 v3 = new Vector3();
            v3.X = vector3.x;
            v3.Y = vector3.y;
            v3.Z = vector3.z;
            return v3;
        }

        /// <summary>
        ///  convert proto V3 to unity V3
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns>Proto V3</returns>
        public static UnityEngine.Vector3 ConvertProtoVec3ToUnityVec3(Vector3 vector3)
        {
            UnityEngine.Vector3 v3 = new UnityEngine.Vector3();
            v3.x = vector3.X;
            v3.y = vector3.Y;
            v3.z = vector3.Z;
            return v3;
        }

        public static void CopyV3ToPbPos(ProtoMsg.Vector3 protoV3, UnityEngine.Vector3 v3)
        {
            protoV3.X = v3.x;
            protoV3.Y = v3.y;
            protoV3.Z = v3.z;
        }

        /// <summary>
        /// 将 pbPos  赋值给 v3 中, v3 是个 struct , 所以 采用 out
        /// </summary>
        /// <param name="v3"></param>
        /// <param name="pbPos"></param>
        public static void CopyPbPos2V3(out UnityEngine.Vector3 v3, ProtoMsg.Vector3 pbPos)
        {
            v3.x = pbPos.X;
            v3.y = pbPos.Y;
            v3.z = pbPos.Z;
        }



        #region 对 Proto 中的 类做拓展
        /// <summary>
        /// 对 proto 的 RepeatedField 做一个方法的拓展
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <param name="Equals"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static RepeatedField<T> MergeProtoArray<T>(this RepeatedField<T> arr1, RepeatedField<T> arr2, Func<T, T, bool> Equals)
        {
            RepeatedField<T> results = new RepeatedField<T>();
            results.AddRange(arr1);

            foreach (T item in arr2)
            {
                bool include = false;
                foreach (T result in results)
                {
                    if (Equals(result, item))
                    {
                        include = true;
                        break;
                    }
                }
                if (!include)
                {
                    results.Add(item);
                }
            }
            return results;
        }

        public static RepeatedField<T> Append<T>(this RepeatedField<T> arr1, RepeatedField<T> arr2)
        {
            foreach (T item in arr2)
            {
                arr1.Add(item);
            }
            return arr1;
        }

        /// <summary>
        /// 集合相减 A [1,2,3] , B [2,3,4] , A - B = [1]
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        // public static RepeatedField<T> Subtract<T>(this RepeatedField<T> arr1, RepeatedField<T> arr2, Func<T, T, bool> equalFunc)
        // {
        //     var CreateSetComparer = HashSet<T>.CreateSetComparer();
        //     CreateSetComparer.Equals(equalFunc);

        //     HashSet<T> tempHashSet = new HashSet<T>();
        //     for (int i = 0; i < arr1.Count; i++)
        //     {
        //         tempHashSet.Add(arr1[i]);
        //     }

        //     for (int i = 0; i < arr2.Count; i++)
        //     {
        //         tempHashSet.Comparer()
        //     }

        // }
        #endregion

        /// <summary>
        /// 集合相减 A [1,2,3] , B [2,3,4] , A - B = [1]
        /// TODO : DL
        ///     临时做的接口,上面 set 的CreateSetComparer 不太会用
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <param name="equalFunc"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static RepeatedField<T> Subtract<T>(RepeatedField<T> arr1, RepeatedField<T> arr2, Func<T, T, bool> equalFunc)
        {
            RepeatedField<T> result = new RepeatedField<T>();

            List<T> arr2CopyList = arr2.KToList();
            for (int i = 0; i < arr1.Count; i++)
            {
                int idx = arr2CopyList.FindIndex((T value) =>
                {
                    return equalFunc.Invoke(arr1[i], value);
                });
                // 集合相减, 如果arr2中 找不到 arr1的数据,则保留
                if (idx == -1)
                {
                    result.Add(arr1[i]);
                }
            }

            return result;
        }


        #region BaseBinary 拓展

        public static int BaseBinaryContainKey(BaseBinary baseBinary, int index)
        {
            byte[] datas = baseBinary.Data.ToByteArray();
            int dlen = baseBinary.OneDataBitNum;
            int ai = index / (8 / dlen);
            byte bitnum = (byte)(index % (8 / dlen) * dlen);
            if (ai >= baseBinary.ArrayLen)
            {
                return 0;
            }
            byte d = datas[ai];
            if (dlen == 8)
            {
                return d;
            }
            byte mask = (byte)((1 << dlen) - 1);
            return (d & (mask << bitnum)) >> bitnum;
        }

        public static bool BaseBinaryUpData(BaseBinary baseBinary, int index, int val)
        {
            byte bval = (byte)val;
            byte[] datas = baseBinary.Data.ToByteArray();
            int dlen = baseBinary.OneDataBitNum;
            int ai = index / (8 / dlen);
            byte bitnum = (byte)(index % (8 / dlen) * dlen);
            if (ai >= baseBinary.ArrayLen)
            {
                return false;
            }
            byte key = datas[ai];
            if (dlen == 8)
            {
                key = bval;
            }
            else
            {
                int tmp = (1 << dlen) - 1;
                bval = (byte)(bval & tmp);
                tmp = (byte)(tmp << bitnum);
                tmp = key & ~tmp;
                key = (byte)(tmp + (bval << bitnum));
            }
            datas[ai] = key;
            baseBinary.Data = ByteString.CopyFrom(datas);
            return true;
        }

        #endregion

    }


}