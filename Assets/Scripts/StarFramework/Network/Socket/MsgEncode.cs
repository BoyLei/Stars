using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SGF.Network
{
    public class MsgEncode
    {
        /// <summary>
        /// 消息的加解密是否开启
        /// </summary>
        public static bool isOpenEncrypt = false;
        public static byte[] encryptKey = new byte[] { 253, 1, 56, 52, 62, 176, 42, 138 };
        public static byte[] decryptKey = new byte[] { 41, 253, 1, 56, 52, 62, 176, 42 };

        // EncryptData 加密算法
        public static byte[] EncryptData(byte[] buf)
        {
            int bufLen = buf.Length;
            byte[] key = encryptKey;
            int keyLen = key.Length;

            for (int i = 0; i < bufLen; i++)
            {
                byte n = (byte)(i % 7 + 1);
                int b = (buf[i] << n) | (buf[i] >> (8 - n));

                buf[i] = (byte)(b ^ key[i % keyLen]);
            }

            return buf;
        }

        // DecryptData 解密算法
        public static byte[] DecryptData(byte[] buf)
        {
            int bufLen = buf.Length;
            byte[] key = encryptKey;
            int keyLen = key.Length;

            for (int i = 0; i < bufLen; i++)
            {
                int b = buf[i] ^ key[i % keyLen];
                byte n = (byte)(i % 7 + 1);

                buf[i] = (byte)((b >> n) | (b << (8 - n)));
            }

            return buf;
        }

    }
}