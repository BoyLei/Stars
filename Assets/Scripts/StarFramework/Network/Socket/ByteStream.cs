using System;
using System.Reflection;
using UnityEngine;

namespace SGF.Network
{
    class ByteStream
    {
        string flag_key = "[ByteStream]";
        public byte[] data;
        int readPos = 0;
        int writePos = 0;

        /// <summary>
        /// 构建一个空的byteStream
        /// </summary>
        public ByteStream()
        {
            readPos = 0;
            writePos = 0;
        }
        /// <summary>
        /// V:结构化数据结果
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <param name="buf">字节数据</param>
        /// <param name="result">V:结构化数据结果</param>
        /// <returns></returns>
        public object DeSerializeType(Type type, byte[] buf, out bool result)
        {
            data = buf;
            object v = Activator.CreateInstance(type);//某个类型实例，得结构化结果||| //Type v = new Type();//new object() as Type;//是类型得值；不是表达类型得Type类 （的类类形的值）
            result = DeSerialize(ref v);
         
            return v;
        }

        private bool ReadCheck(int count)
        {
            if (data == null)
            {
                return false;
            }
            if (readPos + count > data.Length)
            {
                return false;
            }
            return true;
        }

        private bool ReadEnd()
        {
            return readPos != data.Length;
        }

        private byte ReadByte(out bool result)
        {
            result = ReadCheck(1);
            if (!result)
            {
                return 0;
            }
            byte value = data[readPos];
            readPos++;
            // Debug.Log($"{flag_key} ReadByte readPos : {readPos}");
            return value;
        }

        private byte[] ReadBytes(out bool result)
        {
            ushort len = ReadUShort(out result);
            //如果读取失败，直接返回长度为[]
            if (!result)
            {
                return new byte[0];
            }

            if (len == 0)
            {
                return new byte[0];
            }

            result = ReadCheck(len);
            if (!result)
            {
                return new byte[0];
            }

            byte[] buf = new byte[len];
            Array.Copy(data, readPos, buf, 0, len);
            readPos += len;
            // Debug.Log($"{flag_key} ReadByte readPos : {readPos}");

            Debug.Log($"{flag_key} ReadBytes : buf {buf} , readPos : {readPos}");

            return buf;
        }

        private sbyte ReadSByte(out bool result)
        {
            sbyte value = (sbyte)ReadByte(out result);
            // Debug.Log($"{flag_key} ReadSByte readPos : {readPos}");
            return value;
        }


        private bool ReadBool(out bool result)
        {
            byte v = ReadByte(out result);
            if (!result)
            {
                return false;
            }
            return v != 0;
        }

        private ushort ReadUShort(out bool result)
        {
            result = ReadCheck(2);
            if (!result)
            {
                return 0;
            }

            ushort v = BitConverter.ToUInt16(data, readPos);
            readPos += 2;
            // Debug.Log($"{flag_key} ReadUShort readPos : {readPos}");

            return v;
        }

        private short ReadShort(out bool result)
        {
            result = ReadCheck(2);
            if (!result)
            {
                return 0;
            }

            short v = BitConverter.ToInt16(data, readPos);
            readPos += 2;
            // Debug.Log($"{flag_key} ReadShort readPos : {readPos}");

            return v;
        }

        private uint ReadUInt(out bool result)
        {
            result = ReadCheck(4);
            if (!result)
            {
                return 0;
            }

            uint v = BitConverter.ToUInt32(data, readPos);
            readPos += 4;
            // Debug.Log($"{flag_key} ReadUInt readPos : {readPos}");

            return v;
        }

        private int ReadInt(out bool result)
        {
            result = ReadCheck(4);
            if (!result)
            {
                return 0;
            }

            int v = BitConverter.ToInt32(data, readPos);
            readPos += 4;
            // Debug.Log($"{flag_key} ReadInt readPos : {readPos}");

            return v;
        }

        private ulong ReadULong(out bool result)
        {
            result = ReadCheck(8);
            if (!result)
            {
                return 0;
            }

            ulong v = BitConverter.ToUInt64(data, readPos);
            readPos += 8;
            // Debug.Log($"{flag_key} ReadULong readPos : {readPos}");

            return v;
        }

        private long ReadLong(out bool result)
        {
            result = ReadCheck(8);
            if (!result)
            {
                return 0;
            }

            long v = BitConverter.ToInt64(data, readPos);
            readPos += 8;
            // Debug.Log($"{flag_key} ReadLong readPos : {readPos}");

            return v;
        }

        private string ReadString(out bool result)
        {
            ushort len = ReadUShort(out result);
            if (!result)
            {
                return "";
            }
            result = ReadCheck((int)len);
            if (!result)
            {
                return "";
            }

            string v = System.Text.Encoding.UTF8.GetString(data, readPos, len);
            readPos += len;
            // Debug.Log($"{flag_key} ReadStr readPos : {readPos}");

            return v;
        }

        //-------------------------Write---------------------------------
        //TODO
        // 目前不知道改如何用泛型的方式，去减少代码，主要的愿意是 Sizeof（T） ，T为泛型的时候，编译会报错
        private bool writeCheck(int count)
        {
            if (data == null)
            {
                return false;
            }
            if (writePos + count > data.Length)
            {
                return false;
            }
            return true;
        }
        private bool WriteBase(byte[] buf, int len)
        {
            bool result = writeCheck(len);
            if (!result)
            {
                return result;
            }
            Array.Copy(buf, 0, data, writePos, len);
            writePos += len;
            return result;
        }

        private bool WriteByte(byte v)
        {
            bool result = WriteBase(new byte[] { v }, 1);
            // Debug.Log($"{flag_key} WriteByte writePos : {writePos}");
            return result;
        }

        private bool WriteBytes(byte[] v)
        {
            ushort len = (ushort)v.Length;
            if (len == 0)
            {
                return WriteUShort(0);
            }

            bool result = writeCheck(len + 2);
            if (!result)
            {
                return false;
            }
            //写入长度
            WriteUShort(len);

            result = WriteBase(v, len);

            // Debug.Log($"{flag_key} WriteByte writePos : {writePos}");

            //Debug.Log($"{flag_key} WriteBytes : buf {v} , writePos : {writePos}");

            return result;
        }

        private bool WriteSByte(sbyte v)
        {
            return WriteByte((byte)v);
        }

        private bool WriteBool(bool v)
        {
            // Debug.Log($"{flag_key} WriteBool writePos : {writePos}");

            if (v)
            {
                return WriteByte(1);
            }
            return WriteByte(0);
        }



        private bool WriteUShort(ushort v)
        {
            byte[] vBuf = BitConverter.GetBytes(v);
            bool result = WriteBase(vBuf, 2);

            // Debug.Log($"{flag_key} WrithUShort writePos : {writePos} result : {result} ");

            return result;
        }

        private bool WriteShort(short v)
        {
            byte[] vBuf = BitConverter.GetBytes(v);
            bool result = WriteBase(vBuf, 2);

            // Debug.Log($"{flag_key} WriteShort writePos : {writePos} result : {result} ");

            return result;
        }

        private bool WriteUInt(uint v)
        {
            byte[] vBuf = BitConverter.GetBytes(v);
            bool result = WriteBase(vBuf, 4);


            // Debug.Log($"{flag_key} WriteUInt writePos : {writePos} result : {result} ");

            return result;
        }

        private bool WriteInt(int v)
        {
            byte[] vBuf = BitConverter.GetBytes(v);
            bool result = WriteBase(vBuf, 4);

            // Debug.Log($"{flag_key} WriteInt writePos : {writePos} result : {result} ");

            return result;
        }

        private bool WriteuLong(ulong v)
        {
            byte[] vBuf = BitConverter.GetBytes(v);
            bool result = WriteBase(vBuf, 8);

            // Debug.Log($"{flag_key} WriteuLong writePos : {writePos} result : {result} ");

            return result;
        }

        private bool WriteLong(long v)
        {
            byte[] vBuf = BitConverter.GetBytes(v);
            bool result = WriteBase(vBuf, 8);

            // Debug.Log($"{flag_key} WriteLong writePos : {writePos} result : {result} ");

            return result;
        }

        private bool WriteString(string v)
        {
            int len = v.Length;
            bool result = writeCheck(len + 2);
            if (!result)
            {
                return result;
            }
            WriteUShort((ushort)len);
            if (len > 0)
            {
                byte[] vBuf = System.Text.Encoding.UTF8.GetBytes(v);
                result = WriteBase(vBuf, len);
            }

            // Debug.Log($"{flag_key} WriteStr writePos : {writePos} result : {result} ");

            return result;
        }

        // CalcSize 计算序列化所需长度
        public int CalcSize(object v)
        {
            int size = 0;
            Type type = v.GetType();
            FieldInfo[] fieldInfos = type.GetFields();


            int itemSize = 0;
            foreach (var item in fieldInfos)
            {
                string itemTypeStr = item.FieldType.Name;

                switch (itemTypeStr)
                {
                    case "SByte":
                    case "Byte":
                    case "Boolean":
                        {
                            itemSize = 1;
                        }
                        break;
                    case "UInt16":
                    case "Int16":
                        {
                            itemSize = 2;
                        }
                        break;
                    case "Int32":
                    case "UInt32":
                        {
                            itemSize = 4;
                        }
                        break;

                    case "Int64":
                    case "UInt64":
                        {
                            itemSize = 8;
                        }
                        break;
                    case "String":
                        {
                            object itemValue = item.GetValue(v);
                            string str = FormatStr(itemValue);
                            itemSize = 2 + str.Length;
                        }
                        break;
                    case "Byte[]":
                        {
                            byte[] itemValue = (byte[])item.GetValue(v);
                            itemSize = 2 + itemValue.Length;
                        }
                        break;
                    default:
                        {
                            itemSize = 0;
                            Debug.LogWarning($"{flag_key} file : {item.Name} , type : {itemTypeStr} ， 暂不支持，需要的时候扩展");
                        }
                        break;
                }
                size += itemSize;
                // Debug.Log($"{flag_key} file : {item.Name} , type : {itemTypeStr} ， size : {itemSize}");
            }


            // Debug.Log($"{flag_key} CalcSize T : {type.Name} total size : {size}");

            return size;
        }

        // Serialize 序列化content
        public bool Serialize(object v)
        {
            bool result = false;

            int size = CalcSize(v);
            data = new byte[size];

            // Debug.Log($"{flag_key} Serialize init size : {size}");

            Type type = v.GetType();
            FieldInfo[] fieldInfos = type.GetFields();

            foreach (var item in fieldInfos)
            {
                string itemTypeStr = item.FieldType.Name;
                object value = item.GetValue(v);
                switch (itemTypeStr)
                {
                    case "SByte":
                        {
                            result = WriteSByte((sbyte)value);
                        }
                        break;
                    case "Byte":
                        {
                            result = WriteByte((byte)value);
                        }
                        break;

                    case "Boolean":
                        {
                            result = WriteBool((bool)value);
                        }
                        break;
                    case "UInt16":
                        {
                            result = WriteUShort((ushort)value);
                        }
                        break;
                    case "Int16":
                        {
                            result = WriteShort((short)value);
                        }
                        break;
                    case "Int32":
                        {
                            result = WriteInt((int)value);
                        }
                        break;
                    case "UInt32":
                        {
                            result = WriteUInt((uint)value);
                        }
                        break;
                    case "Int64":
                        {
                            result = WriteLong((long)value);
                        }
                        break;
                    case "UInt64":
                        {
                            result = WriteuLong((ulong)value);
                        }
                        break;
                    case "String":
                        {
                            string str = FormatStr(value);
                            result = WriteString(str);
                        }
                        break;
                    case "Byte[]":
                        {
                            byte[] itemValue = (byte[])item.GetValue(v);
                            result = WriteBytes(itemValue);
                        }
                        break;
                    default:
                        Debug.LogWarning($"{flag_key} Serialize at  item : {item.Name} , 缺少 {itemTypeStr} 处理，需要的时候再加");
                        break;
                }
                if (!result)
                {
                    Debug.LogError($"{flag_key} Serialize error at  item : {item.Name} , value : {value} writePos : {writePos} buf.len : {data.Length} ");
                    return result;
                }
                // Debug.Log($"{flag_key} Serialize item : {item.Name} , value : {value} writePos : {writePos} ");

            }
            return result;
        }

        // DeSerialize 反序列化
        /// <summary>
        //[类中：字段值实例的获值过程]
        //1类：字段集合
        //2字段：[*字段类型（*类型名称，类型Type/Class）] 字段实例（类型名称）  *字段值（*Get方式）
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool DeSerialize(ref object v)
        {
            bool result = false;

            // Debug.Log($"{flag_key} DeSerialize buf.len : {data.Length}");
            //类型合集
            Type type = v.GetType();
            //字段合集
            FieldInfo[] fieldInfos = type.GetFields();
            if (fieldInfos.Length == 0)
            {
                return true;
            }
            //字段合集
            foreach (var item in fieldInfos)
            {
                //字段类型，简名
                string itemTypeStr = item.FieldType.Name;
                //获取，原值
                object value = item.GetValue(v);
                //字段，类型名称
                switch (itemTypeStr)
                {
                    //类型决定类型赋值方式[Byte -》 Target ValueType]
                    case "SByte":
                        {
                            //[字段类型] - [字段] -  [字段值取值方式]
                            //类中（！类实例！字段：！类实例！，值）。
                            //Reflection，帮你找到实例结构中本类型的 字段指针的头部地址
                            item.SetValue(v, ReadSByte(out result));
                        }
                        break;
                    case "Byte":
                        {
                            item.SetValue(v, ReadByte(out result));
                        }
                        break;

                    case "Boolean":
                        {
                            item.SetValue(v, ReadBool(out result));
                        }
                        break;
                    case "UInt16":
                        {
                            item.SetValue(v, ReadUShort(out result));
                        }
                        break;
                    case "Int16":
                        {
                            item.SetValue(v, ReadShort(out result));
                        }
                        break;
                    case "Int32":
                        {
                            item.SetValue(v, ReadInt(out result));
                        }
                        break;
                    case "UInt32":
                        {
                            item.SetValue(v, ReadUInt(out result));
                        }
                        break;
                    case "Int64":
                        {
                            item.SetValue(v, ReadLong(out result));
                        }
                        break;
                    case "UInt64":
                        {
                            item.SetValue(v, ReadULong(out result));
                        }
                        break;
                    case "String":
                        {
                            item.SetValue(v, ReadString(out result));
                        }
                        break;
                    case "Byte[]":
                        {
                            item.SetValue(v, ReadBytes(out result));
                        }
                        break;
                    default:
                        Debug.LogWarning($"{flag_key} DeSerialize at  item : {item.Name} , 缺少 {itemTypeStr} 处理，需要的时候再加");
                        break;
                }
                if (!result)
                {
                    Debug.LogError($"{flag_key} DeSerialize error at  item : {item.Name} , value : {value} readPos : {readPos} buf.len : {data.Length} ");
                    return result;
                }
                // Debug.Log($"{flag_key} DeSerialize item : {item.Name} , value : {value} readPos : {readPos} ");

            }
            return result;
        }

        public static string FormatStr(object str)
        {
            string formatStr = (string)(str == null ? "" : str);
            return formatStr;
        }

    }

    public enum EnumType
    {
        SByte,
        Byte,
        Boolean,
    }

}
