////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
* 描述：
* 工程 ：StarProject
*/
using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
/// <summary>
/// 【参数组，方法名】和 网络的单一参数，的封装
/// </summary>
namespace SGF.Network.RPCLite
{

    //网络调用名，参数S 的 组合
    [ProtoContract]//序列化类标记
    public class RPCMessage
    {
        [ProtoMember(1)]//序列化成员，以及顺序
        public string methodName;//方法名字
        [ProtoMember(2)]
        public List<RPCRawArg> raw_args_cache = new List<RPCRawArg>();//网络参数，缓存组

        /// <summary>
        /// raw_args 参数缓存组
        /// </summary>
        public object[] args
        {
            get
            {
                ///List 2 Array
                List<object> list = new List<object>();
                for (int i = 0; i < raw_args_cache.Count; i++)
                {
                    list.Add(raw_args_cache[i].value);
                }
                return list.ToArray();
            }

            set
            {
                
                raw_args_cache = new List<RPCRawArg>();
                //new array
                object[] list = value;
                for (int i = 0; i < list.Length; i++)
                {
                    ///值缓存同步到List
                    RPCRawArg raw_arg = new RPCRawArg();
                    raw_arg.value = list[i];
                    raw_args_cache.Add(raw_arg);
                }
            }
        }

    }

    /// <summary>
    /// 网络[参数]类
    /// </summary>
    [ProtoContract]
    public class RPCRawArg
    {
        [ProtoMember(1)]
        public E_RPCArgType type;///数据类型，传递枚举
        [ProtoMember(2)]
        public byte[] raw_value;//数据

        public object value
        {
            get
            {
                if (raw_value == null || raw_value.Length == 0)
                {
                    return null;
                }

                NetBufferReader reader = new NetBufferReader(raw_value);
                switch (type)
                {
                    case E_RPCArgType.Int: return reader.ReadInt();
                    case E_RPCArgType.UInt: return reader.ReadUInt();
                    case E_RPCArgType.Long: return reader.ReadLong();
                    case E_RPCArgType.ULong: return reader.ReadULong();
                    case E_RPCArgType.Short: return reader.ReadShort();
                    case E_RPCArgType.UShort: return reader.ReadUShort();
                    case E_RPCArgType.Double: return reader.ReadDouble();
                    case E_RPCArgType.Float: return reader.ReadFloat();
                    case E_RPCArgType.String: return Encoding.UTF8.GetString(raw_value);
                    case E_RPCArgType.Byte: return reader.ReadByte();
                    case E_RPCArgType.Bool: return reader.ReadByte() != 0;
                    case E_RPCArgType.ByteArray: return raw_value;
                    default: return raw_value;
                }
            }

            set
            {
                NetBuffer writer;
                object v = value;
                if (v is int)
                {
                    type = E_RPCArgType.Int;
                    raw_value = BitConverter.GetBytes((int)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is uint)
                {
                    type = E_RPCArgType.UInt;
                    raw_value = BitConverter.GetBytes((uint)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is long)
                {
                    type = E_RPCArgType.Long;
                    raw_value = BitConverter.GetBytes((long)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is ulong)
                {
                    type = E_RPCArgType.ULong;
                    raw_value = BitConverter.GetBytes((ulong)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is short)
                {
                    type = E_RPCArgType.Short;
                    raw_value = BitConverter.GetBytes((short)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is ushort)
                {
                    type = E_RPCArgType.UShort;
                    raw_value = BitConverter.GetBytes((ushort)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is double)
                {
                    type = E_RPCArgType.Double;
                    raw_value = BitConverter.GetBytes((double)v);
                }
                else if (v is float)
                {
                    type = E_RPCArgType.Float;
                    raw_value = BitConverter.GetBytes((float)v);
                    NetBuffer.ReverseOrder(raw_value);
                }
                else if (v is string)
                {
                    type = E_RPCArgType.String;
                    raw_value = Encoding.UTF8.GetBytes((string)v);
                }
                else if (v is byte)
                {
                    type = E_RPCArgType.Byte;
                    raw_value = new[] { (byte)v };
                }
                else if (v is bool)
                {
                    type = E_RPCArgType.Bool;
                    raw_value = new[] { (bool)v ? (byte)1 : (byte)0 };
                }
                else if (v is byte[])
                {
                    type = E_RPCArgType.ByteArray;
                    raw_value = new byte[((byte[])v).Length];
                    Buffer.BlockCopy((byte[])v, 0, raw_value, 0, raw_value.Length);
                }
            }
        }
    }



    public enum E_RPCArgType
    {
        Unkown = 0,
        Int = 1,
        UInt = 2,
        Long = 3,
        ULong = 4,
        Short = 5,
        UShort = 6,
        Double = 8,
        Float = 9,
        String = 10,
        Byte = 11,
        Bool = 12,
        ByteArray = 31
    }

}
