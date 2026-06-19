using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

namespace SGF.Network
{
    /// <summary>
    /// 静态的 socket 工具类, 从SocketBase中独立出来的一些 可以公用的方法
    /// </summary>
    public static class SocketUtils
    {
        /// <summary>
        /// 数据转网络结构
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static sSocketData BytesToSocketData(int cmd, byte[] data, Boolean isEncrypt)
        {
            sSocketData socketData = new();
            socketData.buffLength = Constants.HEAD_SUM_LEN + data.Length;
            socketData.dataLength = data.Length;
            socketData.cmd = cmd;
            socketData.data = data;
            socketData.serializeType = (byte)(isEncrypt ? 0x02 : 0x00);

            return socketData;
        }

        /// <summary>
        /// 将发送的数据格式化
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <param name="isEncrypt"></param>
        /// <returns></returns>
        public static byte[] FormateData(int cmd, byte[] data, Boolean isEncrypt)
        {
            byte[] buf = data;
            //如果加密的话，将数据执行加密操作
            //Debug.Log($"{TagFlag} FormateData cmd : {cmd}  isEncrypt : {isEncrypt} before : {KTool.BytesArrToString(data) }");
            if (isEncrypt)
            {
                buf = MsgEncode.EncryptData(buf);
                //Debug.Log($"{TagFlag} FormateData cmd : {cmd}  isEncrypt : {isEncrypt} after : {KTool.BytesArrToString(buf) }");

            }
            return BytesToSocketData(cmd, data, isEncrypt).Serialize();
        }

        /// <summary>
        /// 将rpc的数据在做一次封装，结构 为 1个字节 协议类型 + 2字节 cmd + 2字节 长度
        /// 注意： 协议类型写死 为 14 （服务器说的，表示prototype）
        /// 加注释是因为排除数据问题的时候，很麻烦，服务器自己都忘了为啥是14了

        /// </summary>
        /// <param name="data"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static byte[] RPCMsgSerializer(byte[] data, int cmd)
        {
            int len = data.Length;
            byte[] buf = new byte[len + 1 + 2 + 2];
            //rpc  14 服务器让固定写死，表示proto类型,data 用proto来解开
            buf[0] = (byte)14;


            byte[] cmdBuf = BitConverter.GetBytes(cmd);
            Array.Copy(cmdBuf, 0, buf, 1, 2);

            byte[] lenBuf = BitConverter.GetBytes(len);
            Array.Copy(lenBuf, 0, buf, 3, 2);

            Array.Copy(data, 0, buf, 5, len);
            //Debug.Log($"{TagFlag} RPCMsgSerializer cmd : {cmd}  , buf : {KTool.BytesArrToString(buf)}");

            return buf;
        }

        /// <summary>
        /// 从 DataBuffer 中取出 完整的 MessageData
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <returns></returns>
        public static MessageData GetMsgData(DataBuffer dataBuffer)
        {
            //取出一条完整数据
            sSocketData? socketData = dataBuffer.GetData();
            if (socketData.HasValue)
            {
                sSocketData value = socketData.Value;

                MessageData msgData = new();
                // msgData.socketName = socketName;
                msgData.cmd = value.cmd;
                msgData.data = value.data;
                msgData.serializeType = value.serializeType;
                msgData.allData = value.allData;
                msgData.buffLength = value.buffLength;
                msgData.dataLength = value.dataLength;
                msgData.curBuffPosition = value.curBuffPosition;
                msgData.minBuffLen = value.minBuffLen;

                return msgData;
            }

            return null;
        }

        public static MessageHandleData GetMsgHandleData(DataBuffer dataBuffer, string socketName)
        {
            MessageData msgData = GetMsgData(dataBuffer);
            if (msgData == null)
            {
                return null;
            }
            msgData.socketName = socketName;
            return DeserializeMessageData(msgData);
        }


        public static MessageHandleData DeserializeMessageData(MessageData messageData)
        {
            byte[] buf = messageData.data;

            int encryptFlag = messageData.serializeType & 0x02;
            //表示字段已经加密，则需要解密
            if (encryptFlag > 0)
            {
                // Debug.Log($"[SocketUtils] handleMessageCS encryptFlag：{encryptFlag} need DecryptData: { KTool.BytesArrToString(buf)}");
                buf = MsgEncode.DecryptData(buf);
                // Debug.Log($"[SocketUtils] handleMessageCS encryptFlag：{encryptFlag} after DecryptData ---> {KTool.BytesArrToString(buf)}");
            }
            messageData.data = buf;

            int compressFlag = messageData.serializeType & 0x01;

            // Debug.Log($"[SocketUtils] handleMessageCS CMD : {messageData.cmd} compressFlag : {compressFlag} ");
            // Debug.Log($"[SocketUtils] handleMessageCS data : buf { KTool.BytesArrToString(buf)} ");

            MessageHandleData messageHandleData = new MessageHandleData();
            messageHandleData.socketName = messageData.socketName;

            //compressFlag == 0 有两层，一个走pb，一个走自定义。
            //服务器通过统一接口的方式，通过一个msgContent.Unmarshal 做到了统一解析
            //我这边跟服务器沟通出现了偏差，以为pb 和自定义会走两个 compressFlag 值
            //所以先通过cmd 是否在自定义协议集合，先把协议调通。

            //TODO: dl
            //服务器的通过多态来统一解的方式，我不太确实是否最优。
            //按我的想法，其实就应该根据 compressFlag 的不同值，来做不同的解析方式
            //但服务器的方式，代码更加整洁

            int cmd = messageData.cmd;
            bool isCustomMsg = CustomMsg.Instance.Contain(cmd);


            bool result = false;

            //表示自定义的msg 结构
            if (compressFlag == 0)
            {
                if (isCustomMsg)
                {
                    result = DeserializeCustomMsg(messageData, messageHandleData);
                }
                else
                {
                    result = ProtoUtils.DeserializePbMsg(messageData, messageHandleData);

                    if (result)
                    {
                        QASDKMsgUtils.SendServerMessage2QASDK(messageHandleData.isRPC, true, false, messageHandleData.messageCmd, (IMessage)messageHandleData.data);
                    }
                }
            }
            else if (compressFlag == 1)
            {
                //听海星的说法，此处服务器为冗余代码，serializeType只可能为0 或者 2
                //compressFlag 不可能为1
            }
            else
            {
                Debug.LogError($"[SocketUtils] handleMessageCS CMD : {messageData.cmd} has error compression flag");
                return null;
            }

            if (!result)
            {
                Debug.LogError($"[SocketUtils] handleMessageCS CMD : {messageData.cmd} 解析失败: {messageData.allData}");

                return null;
            }

            return messageHandleData;
        }

        public static bool DeserializeCustomMsg(MessageData messageData, MessageHandleData messageHandleData)
        {
            int cmd = messageData.cmd;

            Type type = CustomMsg.Instance.GetStructByCmd(cmd);
            ByteStream byteStream = new ByteStream();

            bool result = false;
            object data = byteStream.DeSerializeType(type, messageData.data, out result);

            if (result)
            {
                messageHandleData.data = data;
                messageHandleData.messageCmd = cmd;
                // 对于自定义的数据结构, 目前应该都是一层数据结构
                messageHandleData.DeserializedData = data;
            }
            else
            {
                Debug.LogError($"[SocketUtils] DeserializeCustomMsg cmd : {cmd} error!!");
            }
            return result;
        }
    }
}
