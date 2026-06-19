using System;
using System.Buffers;

namespace SGF.Network
{

    //常量数据
    public class Constants
    {
        //总数据 = int32（数据总长度） + 描述后续数据的类型 + 数据(N byte)
        //消息：包头数据（6字节）[数据总长度(4byte) + 数据类型(2byte)]  +  [数据(N byte)]
        //PS：32位 = 服务器描述长度 = （含字符类型2字 + Ndata长度）
        //32位 + 16 + N
        public static int HEAD_DATA_LEN = 4;
        public static int HEAD_TYPE_LEN = 2;
        public static int HEAD_SUM_LEN//6byte
        {
            get { return HEAD_DATA_LEN + HEAD_TYPE_LEN; }
        }

    }

    /// <summary>
    /// 网络数据结构
    /// </summary>
    [System.Serializable]
    public struct sSocketData
    {
        public byte[] data;
        public int cmd;

        /// <summary>
        /// 数据序列化的方式，用0表示默认的 pb 
        /// </summary>
        public byte serializeType;
        /// <summary>
        /// socket 的总数据长度
        /// </summary>
        public int buffLength;
        /// <summary>
        /// socket 传送的数据长度
        /// </summary>
        public int dataLength;

        public byte[] allData;
        public int curBuffPosition;
        //自动大小数据缓存器
        public int minBuffLen;

        public byte[] Serialize()
        {

            byte[] buf = new byte[buffLength];
            byte[] buffLenBytes = BitConverter.GetBytes(buffLength - Constants.HEAD_DATA_LEN);

            //buff 的前3个字节代表长度，第四个字节代表是否加密，0x00表示不加密
            buffLenBytes[3] = serializeType;

            byte[] cmdBytes = BitConverter.GetBytes((UInt16)cmd);

            Array.Copy(buffLenBytes, 0, buf, 0, Constants.HEAD_DATA_LEN);
            Array.Copy(cmdBytes, 0, buf, Constants.HEAD_DATA_LEN, Constants.HEAD_TYPE_LEN);
            Array.Copy(data, 0, buf, Constants.HEAD_SUM_LEN, dataLength);

            //Debug.Log($"[DataBuff] origin data {KTool.BytesArrToString(data)}");
            //Debug.Log($"[DataBuff] after Serialize buf {KTool.BytesArrToString(buf)}");

            return buf;
        }
    }

    /// <summary>
    /// 网络数据缓存器，
    /// </summary>
    [System.Serializable]
    public class DataBuffer
    {
        //自动大小数据缓存器
        private int _minBuffLen;
        public byte[] _buff;
        private int _curBuffPosition;
        private int _buffLength = 0;
        private int _dataLength;
        private int _cmd;

        private byte _serializeType = 0;



        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_minBuffLen">最小缓冲区大小</param>
        public DataBuffer(int _minBuffLen = 1024)
        {
            if (_minBuffLen <= 0)
            {
                this._minBuffLen = 1024;
            }
            else
            {
                this._minBuffLen = _minBuffLen;
            }
            // _buff = new byte[this._minBuffLen];
            _buff = ShareArrayPool.Rent<byte>(this._minBuffLen);
        }

        /// <summary>
        /// 添加缓存数据
        /// 拼接，续接缓存数据
        /// </summary>
        /// <param name="_data">数据</param>
        /// <param name="_dataByteLen">几个Byte字节</param>
        public void AddBuffer(byte[] _data, int _dataByteLen)
        {
            if (_dataByteLen > _buff.Length - _curBuffPosition)//超过当前缓存
            {
                // byte[] _newBuff = new byte[_curBuffPosition + _dataByteLen];
                // 申请一片新的 共享内存
                byte[] _newBuff = ShareArrayPool.Rent<byte>(_curBuffPosition + _dataByteLen);

                Array.Copy(_buff, 0, _newBuff, 0, _curBuffPosition);
                Array.Copy(_data, 0, _newBuff, _curBuffPosition, _dataByteLen);

                // 先返还 旧的 _buff 内存
                ShareArrayPool.Return(_buff);

                // 重新 将 _buff 指向 _newBuff
                _buff = _newBuff;
                _newBuff = null;
            }
            else
            {
                Array.Copy(_data, 0, _buff, _curBuffPosition, _dataByteLen);
            }
            _curBuffPosition += _dataByteLen;//修改当前数据标记
        }

        /// <summary>
        /// 更新数据长度
        /// </summary>
        public void UpdateDataLength()
        {
            if (_dataLength == 0 && _curBuffPosition >= Constants.HEAD_SUM_LEN)
            {
                // byte[] headDataBuf = new byte[Constants.HEAD_DATA_LEN];

                var shared = System.Buffers.ArrayPool<byte>.Shared;
                byte[] headDataBuf = shared.Rent(Constants.HEAD_DATA_LEN);

                //note:
                //数据头数据的前3个字节代表数据包的长度，第四个字节代表编码格式
                //所以，手动的填充第四个字节 0x00;作为本地编码
                //0~（3+1）
                Array.Copy(_buff, 0, headDataBuf, 0, Constants.HEAD_DATA_LEN - 1);                                                                   //X，第几位，塞到Y，第几位开始，取n长
                headDataBuf[3] = 0x00;

                //buf 的总长度 = （服务器4字节描述长度）headBuf.length + （四字节本身）headDataBuf.length;
                //在byte[]中，从指定的索引开始，读取4字节，转换int32
                _buffLength = BitConverter.ToInt32(headDataBuf, 0) + Constants.HEAD_DATA_LEN;



                //note
                //数据头的第三个字节代表Remote数据编码格式
                _serializeType = _buff[3];

                //4~6
                // byte[] tmpProtocalType = new byte[Constants.HEAD_TYPE_LEN];

                byte[] tmpProtocalType = shared.Rent(Constants.HEAD_TYPE_LEN);
                Array.Copy(_buff, Constants.HEAD_DATA_LEN, tmpProtocalType, 0, Constants.HEAD_TYPE_LEN);
                _cmd = BitConverter.ToUInt16(tmpProtocalType, 0);

                //有效数据长度：服务器描述长度 - 头包（6字节）非数据描述长度
                _dataLength = _buffLength - Constants.HEAD_SUM_LEN;
                //服务器描述的长度实际上 = 含数据描述2字节 + NData数据长度

                // 归还 租赁的 内存
                shared.Return(headDataBuf);
                shared.Return(tmpProtocalType);
            }
        }

        public bool ContainData()
        {
            if (_buffLength <= 0)
            {
                UpdateDataLength();
            }

            if (_buffLength > 0 && _curBuffPosition >= _buffLength)
            {
                return true;
            }
            return false;

        }


        public sSocketData? GetData()
        {
            if (!ContainData())
            {
                return null;
            }
            sSocketData socketData = new sSocketData();
            socketData.buffLength = _buffLength;
            socketData.dataLength = _dataLength;
            socketData.cmd = _cmd;
            socketData.data = new byte[_dataLength];
            socketData.serializeType = _serializeType;

            Array.Copy(_buff, Constants.HEAD_SUM_LEN, socketData.data, 0, _dataLength);
            _curBuffPosition -= _buffLength;


            // byte[] _tmpBuff = new byte[_curBuffPosition < _minBuffLen ? _minBuffLen : _curBuffPosition];

            /// 获取完一条数据后, 重新申请一片新的 buff, 将buff 位置 提前
            byte[] _newBuff = ShareArrayPool.Rent<byte>(_curBuffPosition < _minBuffLen ? _minBuffLen : _curBuffPosition);

            // 将剩余的 数据 拷入 新的 buff 中
            Array.Copy(_buff, _buffLength, _newBuff, 0, _curBuffPosition);

            ShareArrayPool.Return(_buff);

            _buff = _newBuff;
            _newBuff = null;

            socketData.allData = _buff;
            socketData.curBuffPosition = _curBuffPosition;
            socketData.minBuffLen = _minBuffLen;

            _buffLength = 0;
            _dataLength = 0;
            _cmd = 0;
            _serializeType = 0;
            return socketData;
        }

    }

}