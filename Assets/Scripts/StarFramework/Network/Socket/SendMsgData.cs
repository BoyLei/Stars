using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SGF.Network
{

    public class SendMsgData : SocketData
    {
        public int cmd;
        public object data;

        public byte[] bytes;

        public bool isEncrypt;

        public int messageCmd;

        public bool result = false;

        public SendMsgData()
        {

        }

        public void Init(int _cmd, object _data, bool _isEncrypt, int _messageCmd = 0)
        {
            cmd = _cmd;
            data = _data;
            isEncrypt = _isEncrypt;
            messageCmd = _messageCmd;

            ByteStream byteStream = new();
            result = byteStream.Serialize(_data);
            bytes = byteStream.data;

            // 如果 字节数为 0, 那 说明这个 数据为空, result 也是 false 
            result = result && bytes.Length != 0;
        }

        /// <summary>
        /// 直接 发送的 byte[] 数据,不需要要 再次 Serialize.
        /// </summary>
        /// <param name="_cmd"></param>
        /// <param name="_data"></param>
        /// <param name="_isEncrypt"></param>
        /// <param name="_messageCmd"></param>
        public void Init(int _cmd, byte[] _data, bool _isEncrypt, int _messageCmd = 0)
        {
            cmd = _cmd;
            data = _data;
            isEncrypt = _isEncrypt;
            messageCmd = _messageCmd;

            result = true;
            bytes = _data;

            // 如果 字节数为 0, 那 说明这个 数据为空, result 也是 false 
            result = result && bytes.Length != 0;
        }

        protected override void Release()
        {
            cmd = 0;
            data = null;
            isEncrypt = false;
            messageCmd = 0;
        }
    }
}
