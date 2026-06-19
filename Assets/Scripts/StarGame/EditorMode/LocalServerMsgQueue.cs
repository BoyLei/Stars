using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Sirenix.Utilities;
using UnityEngine;

namespace EditorModeTest
{
    /// <summary>
    /// 消息的类型
    /// </summary>
    public enum MsgType
    {
        /// <summary>
        /// 本帧立即推送的消息队列
        /// </summary>
        Post,
        /// <summary>
        /// AOI 消息队列, 在 post 队列后面推送
        /// </summary>
        AOI,
        /// <summary>
        /// 延迟 消息队列。 孔磊看代码服务器的 rpc 消息 都是延迟消息队列
        /// </summary>
        Delay
    }

    /// <summary>
    /// IMessage 消息的 数据类
    /// </summary>
    public class MsgData
    {
        public ByteString Bytes;

        public MessageParser Parser;

        public Action<object, object> MsgAction;

        public object OtherData;

        public void Init(ByteString bytes, MessageParser parser, Action<object, object> msgAction, object otherData)
        {
            Bytes = bytes;
            Parser = parser;
            MsgAction = msgAction;
            OtherData = otherData;
        }

        public void Post()
        {
            object data = null;
            if (Parser != null)
            {
                data = Parser.ParseFrom(Bytes);
            }
            MsgAction.Invoke(data, OtherData);
            Reset();
        }

        public void Reset()
        {
            Bytes = null;
            Parser = null;
            MsgAction = null;
            OtherData = null;
        }
    }

    public class MsgDataPool
    {

        int maxCount = 50;
        Queue<MsgData> pool = new();

        public MsgDataPool(int count = 50)
        {
            maxCount = count;
        }

        public MsgData Get()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            return new MsgData();
        }
        public void Put(MsgData msgData)
        {
            if (pool.Count > maxCount)
            {
                msgData.Reset();
                return;
            }
            pool.Enqueue(msgData);
        }
    }

    /// <summary>
    /// 本地服的 post 消息队列， 用来模拟 服务器的 消息队列给客户端发送消息
    /// 
    /// </summary>
    public class LocalServerMsgQueue
    {
        Queue<MsgData> PostMsgQueue = new();
        Queue<MsgData> AOIMsgQueue = new();
        Queue<MsgData> DelayMsgQueue = new();

        MsgDataPool msgDataPool = new MsgDataPool(100);

        public void SendMsg(Action<object, object> msgAc, IMessage data, MsgType msgType, object otherData = null)
        {
            MsgData msgData = msgDataPool.Get();

            if (data != null)
            {
                msgData.Init(data.ToByteString(), data.Descriptor.Parser, msgAc, otherData);
            }
            else
            {
                msgData.Init(null, null, msgAc, otherData);
            }

            switch (msgType)
            {
                case MsgType.Post:
                    {
                        PostMsgQueue.Enqueue(msgData);
                    }
                    break;
                case MsgType.AOI:
                    {
                        AOIMsgQueue.Enqueue(msgData);
                    }
                    break;
                case MsgType.Delay:
                    {
                        DelayMsgQueue.Enqueue(msgData);
                    }
                    break;

                default: break;
            }
        }

        private void PostMsg(Queue<MsgData> msgQueue)
        {
            while (msgQueue.Count > 0)
            {
                var msgAc = msgQueue.Dequeue();
                msgAc.Post();
            }
        }

        public void OnTick()
        {
            PostMsg(PostMsgQueue);
            PostMsg(AOIMsgQueue);
            PostMsg(DelayMsgQueue);
        }

    }
}
