//--------------------------------------------------------------------
// 文件名:		Assets/Scripts/StarGame/Utlis/Archive.cs
// 内  容:		 
// 创建日期:	    
// 创建人:		 
// 版权所有:	    
//--------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SysUtils
{
    [XLua.LuaCallCSharp]
    public static class ArchiveHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Serialize(object data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream rems = new MemoryStream();
            formatter.Serialize(rems, data);
            return rems.GetBuffer();
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data">数据缓冲区</param>
        /// <returns>对象</returns>
        public static object Deserialize(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream rems = new MemoryStream(data);
            data = null;
            return formatter.Deserialize(rems);
        }
    }

    // 读取字节数据流
    [XLua.LuaCallCSharp]
    public class LoadArchive
    {
        private byte[] m_pData;
        private int m_nLength;
        private int m_nCurPosi;
        private static LoadArchive ar = new LoadArchive();

        //
        public LoadArchive(byte[] pdata, int start, int len)
        {
            m_pData = new byte[len];
            m_nLength = len;
            m_nCurPosi = 0;

            Array.Copy(pdata, start, m_pData, 0, len);
        }

        public LoadArchive(byte[] pdata, int len)
        {
            m_pData = pdata;
            m_nLength = len;
            m_nCurPosi = 0;
        }

        public LoadArchive(IntPtr pdata, int len)
        {
            m_pData = new byte[len];
            m_nLength = len;
            m_nCurPosi = 0;

            Marshal.Copy(pdata, m_pData, 0, len);
        }

        protected LoadArchive()
        {
        }

        public static LoadArchive Load(byte[] pdata, int len)
        {
            ar.m_pData = pdata;
            ar.m_nLength = len;
            ar.m_nCurPosi = 0;
            return ar;
        }

        // 返回字节数组
        public byte[] GetData()
        {
            return m_pData;
        }

        // 返回长度
        public int GetLength()
        {
            return m_nCurPosi;
        }

        // 移动到指定位置
        public bool Seek(int posi)
        {
            if ((posi < 0) || (posi > m_nLength))
            {
                return false;
            }

            m_nCurPosi = posi;

            return true;
        }

        public int GetRemain()
        {
            return m_nLength - m_nCurPosi;
        }

        // 增加当前读取位置
        public void IncPosi(int length)
        {
            m_nCurPosi += length;

            if (m_nCurPosi > m_nLength)
            {
                throw new Exception();
            }
        }

        // 读取数据
        public bool ReadUInt8(ref uint val)
        {
            if ((m_nCurPosi + 1) > m_nLength)
            {
                return false;
            }

            val = m_pData[m_nCurPosi];

            IncPosi(1);

            return true;
        }

        public bool ReadInt8(ref int val)
        {
            if ((m_nCurPosi + 1) > m_nLength)
            {
                return false;
            }

            val = (int) m_pData[m_nCurPosi];

            IncPosi(1);

            return true;
        }

        public bool ReadUInt16(ref uint val)
        {
            if ((m_nCurPosi + 2) > m_nLength)
            {
                return false;
            }

            val = (uint) m_pData[m_nCurPosi + 1] << 8 | m_pData[m_nCurPosi];

            // val = BitConverter.ToUInt16(m_pData, m_nCurPosi);

            IncPosi(2);

            return true;
        }

        public bool ReadInt16(ref int val)
        {
            if ((m_nCurPosi + 2) > m_nLength)
            {
                return false;
            }

            val = (int) m_pData[m_nCurPosi + 1] << 8 | m_pData[m_nCurPosi];
            //val = BitConverter.ToInt16(m_pData, m_nCurPosi);

            IncPosi(2);

            return true;
        }

        public bool ReadUInt32(ref uint val)
        {
            if ((m_nCurPosi + 4) > m_nLength)
            {
                return false;
            }

            val = (uint) m_pData[m_nCurPosi + 3] << 24 | (uint) m_pData[m_nCurPosi + 2] << 16 |
                  (uint) m_pData[m_nCurPosi + 1] << 8 | m_pData[m_nCurPosi];
            //val = BitConverter.ToUInt32(m_pData, m_nCurPosi);

            IncPosi(4);

            return true;
        }

        public bool ReadInt32(ref int val)
        {
            if ((m_nCurPosi + 4) > m_nLength)
            {
                return false;
            }

            val = (int) m_pData[m_nCurPosi + 3] << 24 | (int) m_pData[m_nCurPosi + 2] << 16 |
                  (int) m_pData[m_nCurPosi + 1] << 8 | m_pData[m_nCurPosi];

            //val = BitConverter.ToInt32(m_pData, m_nCurPosi);

            IncPosi(4);

            return true;
        }

        public bool ReadUInt64(ref ulong val)
        {
            if ((m_nCurPosi + 8) > m_nLength)
            {
                return false;
            }

            val = BitConverter.ToUInt64(m_pData, m_nCurPosi);

            IncPosi(8);

            return true;
        }

        public bool ReadInt64(ref long val)
        {
            if ((m_nCurPosi + 8) > m_nLength)
            {
                return false;
            }

            val = BitConverter.ToInt64(m_pData, m_nCurPosi);

            IncPosi(8);

            return true;
        }

        public bool ReadFloat(ref float val)
        {
            if ((m_nCurPosi + 4) > m_nLength)
            {
                return false;
            }

            val = BitConverter.ToSingle(m_pData, m_nCurPosi);

            IncPosi(4);

            return true;
        }

        public bool ReadDouble(ref double val)
        {
            if ((m_nCurPosi + 8) > m_nLength)
            {
                return false;
            }

            val = BitConverter.ToDouble(m_pData, m_nCurPosi);

            IncPosi(8);

            return true;
        }

        /// <summary>
        /// 读取带前缀长度的UTF8字符串
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool ReadUTF8String(ref string val)
        {
            // 读取长度
            uint len = 0;

            if (!ReadUInt32(ref len))
            {
                return false;
            }

            // 字符串
            if ((m_nCurPosi + len) > m_nLength)
            {
                return false;
            }

            if (len < 1)
            {
                return false;
            }

            if (len >= 1024)
            {
                val = Encoding.UTF8.GetString(m_pData, m_nCurPosi, (int) (len - 1));
            }
            else
            {
                int charCount = Encoding.UTF8.GetChars(m_pData, m_nCurPosi, (int) (len - 1), tempChars, 0);
                val = new string(tempChars, 0, charCount);
            }

            IncPosi((int) len);

            return true;
        }

        // 读取带前缀长度的字符串
        public bool ReadString(ref string val)
        {
            // 读取长度
            uint len = 0;

            if (!ReadUInt32(ref len))
            {
                return false;
            }

            // 字符串
            if ((m_nCurPosi + len) > m_nLength)
            {
                return false;
            }

            if (len < 1)
            {
                return false;
            }

            if (len >= 1024)
            {
                val = Encoding.UTF8.GetString(m_pData, m_nCurPosi, (int) (len - 1));
            }
            else
            {
                int charCount = Encoding.UTF8.GetChars(m_pData, m_nCurPosi, (int) (len - 1), tempChars, 0);
                val = new string(tempChars, 0, charCount);
            }

            IncPosi((int) len);

            return true;
        }

        //add begin
        // 读取不带前缀长度的UTF8字符串
        public bool ReadUTF8StringNoLen(ref string val, int length = 0)
        {
            // 读取长度
            uint len = 0;

            for (int i = m_nCurPosi; i < m_nLength; i++)
            {
                if (m_pData[i] == '\0')
                {
                    len = (uint) (i - m_nCurPosi + 1);
                    break;
                }
            }

            // 字符串
            if ((m_nCurPosi + len) > m_nLength)
            {
                return false;
            }

            if (len < 1)
            {
                return false;
            }

            if (len >= 1024)
            {
                val = Encoding.UTF8.GetString(m_pData, m_nCurPosi, (int) (len - 1));
            }
            else
            {
                int charCount = Encoding.UTF8.GetChars(m_pData, m_nCurPosi, (int) (len - 1), tempChars, 0);
                val = new string(tempChars, 0, charCount);
            }

            if (length == 0)
                IncPosi((int) len);
            else
                IncPosi(length + 1);

            return true;
        }
        //add end

        //add begin
        // 读取不带前缀长度的字符串
        public bool ReadStringNoLen(ref string val)
        {
            // 读取长度
            uint len = 0;

            for (int i = m_nCurPosi; i < m_nLength; i++)
            {
                if (m_pData[i] == '\0')
                {
                    len = (uint) (i - m_nCurPosi + 1);
                    break;
                }
            }

            // 字符串
            if ((m_nCurPosi + len) > m_nLength)
            {
                return false;
            }

            if (len < 1)
            {
                return false;
            }


            if (len >= 1024)
            {
                val = Encoding.UTF8.GetString(m_pData, m_nCurPosi, (int) (len - 1));
            }
            else
            {
                int charCount = Encoding.UTF8.GetChars(m_pData, m_nCurPosi, (int) (len - 1), tempChars, 0);
                val = new string(tempChars, 0, charCount);
            }

            IncPosi((int) len);

            return true;
        }
        //add end


        // 读取带前缀长度的宽字符串
        public bool ReadWideStr(ref string val)
        {
            uint len = 0;

            if (!ReadUInt32(ref len))
            {
                return false;
            }

            if (len < 2)
            {
                return false;
            }

            return ReadUnicodeLen(ref val, (int) len);
        }

        // 读取不带前缀长度的宽字符串
        public bool ReadWideStrNoLen(ref string val)
        {
            if (m_nCurPosi > m_nLength)
            {
                return false;
            }

            //int size = 0;

            int count = 0;
            bool bHasEndSuffix = false;
            for (int c = 0; c < m_nLength; c = c + 2)
            {
                char v = BitConverter.ToChar(m_pData, m_nCurPosi + c);

                if (v == '\0')
                {
                    bHasEndSuffix = true;
                    count++;
                    break;
                }

                count++;
                // data[count++] = v;
            }

            if (!bHasEndSuffix)
            {
                return false;
            }


            if (count < 1)
            {
                return false;
            }

            char[] data;
            if (count >= 1024)
            {
                data = new char[count]; 
            }
            else
            {
                Array.Clear(tempChars, 0, tempChars.Length);
                data = tempChars;
            }

            data[count] = '\0'; ///保护不会溢出
            for (int c = 0; c < count; c++)
            {
                char v = BitConverter.ToChar(m_pData, m_nCurPosi + c * 2);

                if (v == '\0')
                {
                    break;
                }

                data[c] = v;
            }

            val = new string(data, 0, count);

            IncPosi(count * 2);

            return true;
        }

        private static char[] tempChars = new char[1024];

        // 从指定长度中读取宽字符串
        public bool ReadUnicodeLen(ref string val, int len)
        {
            if ((m_nCurPosi + len) > m_nLength)
            {
                return false;
            }

            int size = len / 2;
            char[] data;
            if (size >= 1024)
            {
                data = new char[size];
            }
            else
            {
                Array.Clear(tempChars, 0, tempChars.Length);
                data = tempChars;
            }

            int count = 0;

            for (int c = 0; c < size; c++)
            {
                char v = BitConverter.ToChar(m_pData, m_nCurPosi + c * 2);

                if (v == '\0')
                {
                    break;
                }

                data[count++] = v;
            }

            val = new string(data, 0, count);

            IncPosi((int) len);

            return true;
        }

        // 二进制数据
        public bool ReadUserData(ref byte[] val)
        {
            uint size = 0;

            if (!ReadUInt32(ref size))
            {
                return false;
            }

            if ((m_nCurPosi + size) > m_nLength)
            {
                return false;
            }

            val =new byte [size];

            Array.Copy(m_pData, m_nCurPosi, val, 0, size);

            IncPosi((int) size);

            return true;
        }

        //add begin
        // 二进制数据 不带长度
        public bool ReadUserDataNoLen(ref byte[] val, int len)
        {
            uint size = (uint) len;

            if ((m_nCurPosi + size) > m_nLength)
            {
                return false;
            }

            val = new byte[size];

            Array.Copy(m_pData, m_nCurPosi, val, 0, size);

            IncPosi((int) size);

            return true;
        }

        //add end
    }

    // 写入字节数据流
    [XLua.LuaCallCSharp]
    public class StoreArchive
    {
        private byte[] m_pData;
        private int m_nLength;
        private int m_nCurPosi;
        public static StoreArchive ar = new StoreArchive();

        public StoreArchive(int size)
        {
            m_pData = new byte[size];
            m_nLength = size;
            m_nCurPosi = 0;

            Array.Clear(m_pData, 0, size);
        }

        public StoreArchive(byte[] pdata, int size)
        {
            m_pData = pdata;
            m_nLength = size;
            m_nCurPosi = 0;

            Array.Clear(m_pData, 0, size);
        }

        protected StoreArchive()
        {
        }

        public static StoreArchive Load(byte[] pdata, int size)
        {
            ar.m_pData = pdata;
            ar.m_nLength = size;
            ar.m_nCurPosi = 0;

            Array.Clear(ar.m_pData, 0, size);
            return ar;
        }

        // 返回字节数组
        public byte[] GetData()
        {
            return m_pData;
        }

        // 返回长度
        public int GetLength()
        {
            return m_nCurPosi;
        }

        // 移动到指定位置
        public bool Seek(int posi)
        {
            if ((posi < 0) || (posi > m_nLength))
            {
                return false;
            }

            m_nCurPosi = posi;

            return true;
        }

        // 增加当前写入位置
        public void IncPosi(int length)
        {
            m_nCurPosi += length;

            if (m_nCurPosi > m_nLength)
            {
                throw new Exception();
            }
        }

        // 申请必要的写入空间
        private void RequireLength(int length)
        {
            int need_len = m_nCurPosi + length;

            if (need_len <= m_nLength)
            {
                return;
            }

            int new_len = m_nLength * 2;

            if (new_len < need_len)
            {
                new_len = need_len;
            }

            byte[] new_buf = new byte[new_len];

            Array.Copy(m_pData, new_buf, m_nLength);

            m_pData = new_buf;
            m_nLength = new_len;
        }

        // 写入数据
        public bool WriteUInt8(uint val)
        {
            RequireLength(1);

            m_pData[m_nCurPosi] = (byte) (val & 0xFF);

            IncPosi(1);

            return true;
        }

        public bool WriteInt8(int val)
        {
            return WriteUInt8((uint) val);
        }

        public bool WriteUInt16(uint val)
        {
            RequireLength(2);

            m_pData[m_nCurPosi] = (byte) (val & 0xff);
            m_pData[m_nCurPosi + 1] = (byte) ((val >> 8) & 0xFF);

            //BitConverter.GetBytes((UInt16)val).CopyTo(m_pData, m_nCurPosi);

            IncPosi(2);

            return true;
        }

        public bool WriteInt16(int val)
        {
            return WriteUInt16((uint) val);
        }

        public bool WriteUInt32(uint val)
        {
            RequireLength(4);

            m_pData[m_nCurPosi] = (byte) (val & 0xff);
            m_pData[m_nCurPosi + 1] = (byte) ((val >> 8) & 0xFF);
            m_pData[m_nCurPosi + 2] = (byte) ((val >> 16) & 0xFF);
            m_pData[m_nCurPosi + 3] = (byte) ((val >> 24) & 0xFF);

            // BitConverter.GetBytes(val).CopyTo(m_pData, m_nCurPosi);

            IncPosi(4);

            return true;
        }

        public bool WriteInt32(int val)
        {
            return WriteUInt32((uint) val);
        }

        public bool WriteUInt64(ulong val)
        {
            RequireLength(8);

            m_pData[m_nCurPosi] = (byte) (val & 0xff);
            m_pData[m_nCurPosi + 1] = (byte) ((val >> 8) & 0xFF);
            m_pData[m_nCurPosi + 2] = (byte) ((val >> 16) & 0xFF);
            m_pData[m_nCurPosi + 3] = (byte) ((val >> 24) & 0xFF);
            m_pData[m_nCurPosi + 4] = (byte) ((val >> 32) & 0xff);
            m_pData[m_nCurPosi + 5] = (byte) ((val >> 40) & 0xFF);
            m_pData[m_nCurPosi + 6] = (byte) ((val >> 48) & 0xFF);
            m_pData[m_nCurPosi + 7] = (byte) ((val >> 56) & 0xFF);
            //BitConverter.GetBytes(val).CopyTo(m_pData, m_nCurPosi);

            IncPosi(8);

            return true;
        }

        public bool WriteInt64(long val)
        {
            return WriteUInt64((ulong) val);
        }

        public bool WriteFloat(float val)
        {
            RequireLength(4);
            BitConverter.GetBytes(val).CopyTo(m_pData, m_nCurPosi);

            IncPosi(4);

            return true;
        }

        public bool WriteDouble(double val)
        {
            RequireLength(8);

            BitConverter.GetBytes(val).CopyTo(m_pData, m_nCurPosi);

            IncPosi(8);

            return true;
        }

        // 写入带前缀长度的UTF8字符串
        public bool WriteUTF8String(string val)
        {
            // 包含结束符
            uint len = (uint) Encoding.UTF8.GetByteCount(val) + 1;

            RequireLength(4 + (int) len);

            m_pData[m_nCurPosi] = (byte) (len & 0xFF);
            m_pData[m_nCurPosi + 1] = (byte) ((len >> 8) & 0xFF);
            m_pData[m_nCurPosi + 2] = (byte) ((len >> 16) & 0xFF);
            m_pData[m_nCurPosi + 3] = (byte) ((len >> 24) & 0xFF);

            IncPosi(4);

            try
            {
                Encoding.UTF8.GetBytes(val, 0, val.Length, m_pData, m_nCurPosi);
            }
            catch (Exception)
            {
                return false;
            }

            // 结束符
            m_pData[m_nCurPosi + len - 1] = 0;

            IncPosi((int) len);

            return true;
        }

        // 写入带前缀长度的字符串
        public bool WriteString(string val)
        {
            if (val==null)
            {
                val = "";
            }
            // 包含结束符
            uint len = (uint) Encoding.Default.GetByteCount(val) + 1;

            RequireLength(4 + (int) len);

            m_pData[m_nCurPosi] = (byte) (len & 0xFF);
            m_pData[m_nCurPosi + 1] = (byte) ((len >> 8) & 0xFF);
            m_pData[m_nCurPosi + 2] = (byte) ((len >> 16) & 0xFF);
            m_pData[m_nCurPosi + 3] = (byte) ((len >> 24) & 0xFF);

            IncPosi(4);

            try
            {
                Encoding.Default.GetBytes(val, 0, val.Length, m_pData, m_nCurPosi);
            }
            catch (Exception)
            {
                return false;
            }

            // 结束符
            m_pData[m_nCurPosi + len - 1] = 0;

            IncPosi((int) len);

            return true;
        }

        // 写入不带前缀长度的字符串
        public bool WriteStringNoLen(string val)
        {
            // 包含结束符
            uint len = (uint) Encoding.Default.GetByteCount(val) + 1;

            RequireLength((int) len);

            try
            {
                Encoding.Default.GetBytes(val, 0, val.Length, m_pData, m_nCurPosi);
            }
            catch (Exception)
            {
                return false;
            }

            // 结束符
            m_pData[m_nCurPosi + len - 1] = 0;

            IncPosi((int) len);

            return true;
        }

        // 写入带前缀长度的宽字符串
        public bool WriteWideStr(string val)
        {
            // 包含结束符
            int len = (val.Length + 1) * 2;

            RequireLength(4 + len);

            m_pData[m_nCurPosi] = (byte) (len & 0xFF);
            m_pData[m_nCurPosi + 1] = (byte) ((len >> 8) & 0xFF);
            m_pData[m_nCurPosi + 2] = (byte) ((len >> 16) & 0xFF);
            m_pData[m_nCurPosi + 3] = (byte) ((len >> 24) & 0xFF);

            IncPosi(4);

            return WriteUnicodeLen(val, len);
        }

        // 写入不带前缀长度的宽字符串
        public bool WriteWideStrNoLen(string val)
        {
            // 包含结束符
            int len = (val.Length + 1) * 2;

            RequireLength(len);

            return WriteUnicodeLen(val, len);
        }

        /// <summary>
        /// 写入一个 utf8格式的数据
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool WriteUtf8Str(string val)
        {
            // 包含结束符
            int len = Encoding.UTF8.GetByteCount(val) + 1;

            RequireLength(len);

            try
            {
                Encoding.UTF8.GetBytes(val, 0, val.Length, m_pData, m_nCurPosi);
            }
            catch (Exception)
            {
                return false;
            }

            // 结束符
            m_pData[m_nCurPosi + len - 1] = 0;

            IncPosi(len);

            return true;
        }

        // 写入不超过指定长度的宽字符串
        public bool WriteUnicodeLen(string val, int len)
        {
            RequireLength(len);

            // 截断
            int val_len = val.Length;

            if (val_len >= (len / 2))
            {
                val_len = (len / 2) - 1;
            }

            for (int c = 0; c < len; c++)
            {
                m_pData[m_nCurPosi + c] = 0;
            }

            for (int c = 0; c < val_len; c++)
            {
                BitConverter.GetBytes(val[c]).CopyTo(m_pData,
                    m_nCurPosi + c * 2);
            }

            IncPosi(len);

            return true;
        }

        // 二进制数据
        public bool WriteUserData(byte[] val)
        {
            RequireLength(val.Length + 4);

            WriteUInt32((uint) val.Length);

            Array.Copy(val, 0, m_pData, m_nCurPosi, val.Length);

            IncPosi(val.Length);

            return true;
        }

        //add begin
        // 二进制数据 不带长度
        public bool WriteUserDataNoLen(byte[] val)
        {
            RequireLength(val.Length);

            Array.Copy(val, 0, m_pData, m_nCurPosi, val.Length);

            IncPosi(val.Length);

            return true;
        }

        //add end
    }
}