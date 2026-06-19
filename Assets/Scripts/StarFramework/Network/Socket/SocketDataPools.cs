using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
using UnityEngine;
namespace SGF.Network
{
    public abstract class SocketData : SimpleDataObject
    {
    }

    /// <summary>
    /// socket 数据层相关的 数据池
    ///     目的是减少 socket 收发过程中反复 new 的各种数据结构
    /// </summary>
    public class SocketDataPools
    {
        private static readonly object _lock = new object();
        private static SocketDataPools _instance = null;

        /// <summary>
        /// socket 存在不同线程调用的情况, 所以此处需要考虑 锁的问题
        /// </summary>
        public static SocketDataPools Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SocketDataPools();

                        }
                    }
                }
                return _instance;
            }
        }

        private Dictionary<Type, System.Buffers.ArrayPool<SocketData>> _pools = new Dictionary<Type, System.Buffers.ArrayPool<SocketData>>();

        public SendMsgData GetMsgData(int cmd, object msg, bool isEncrypt, int messageCmd = 0)
        {
            // 默认取一个
            SendMsgData sendData = SocketDataPools.Instance.GetData<SendMsgData>();
            sendData.Init(cmd, msg, isEncrypt, messageCmd);

            return sendData;
        }

        public SendMsgData GetMsgData(int cmd, byte[] data, bool isEncrypt, int messageCmd = 0)
        {
            // 默认取一个
            SendMsgData sendData = SocketDataPools.Instance.GetData<SendMsgData>();
            sendData.Init(cmd, data, isEncrypt, messageCmd);

            return sendData;
        }

        public byte[] FormateRetMsgData(SendMsgData msg)
        {
            byte[] buf = SocketUtils.FormateData(msg.cmd, msg.bytes, msg.isEncrypt);
            Loom.QueueOnMainThread((v) =>
            {
                Return(msg);
            }, null);
            return buf;
        }
        private T GetData<T>() where T : SocketData, new()
        {

            T data = SimpleDataFactory.InstanceData<T>();

            return data;
        }



        public void Return<T>(T data) where T : SocketData, new()
        {

            data.ReleaseInFactory();
        }

    }
}