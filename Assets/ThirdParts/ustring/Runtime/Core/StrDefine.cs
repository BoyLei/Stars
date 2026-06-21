using System;
using System.Collections.Generic;

namespace Yoka.UnityString.Core
{
    public static class StrDefine
    {
        public const string FM_ARGV_1 = "{0}";
        public const string FM_ARGV_2 = "{1}";
        public const string FM_ARGV_3 = "{2}";
        public const string FM_ARGV_4 = "{3}";
        public const string FM_ARGV_5 = "{4}";
        public const string FM_ARGV_6 = "{5}";
        public const string FM_ARGV_7 = "{6}";
        public const string FM_ARGV_8 = "{7}";
        public const string FM_ARGV_9 = "{8}";

        public const string STR_TRUE = "True";
        public const string STR_FALSE = "False";

        public const char NEW_ALLOC_CHAR = (char)0xCC;

        public const uint DecimalAccuracy = 3; //小数点后精度位数
        public const int MAX_STRING_SIZE = 256;

        public const int InitCacheNum = 128;
        public const int InitSingleCacheNum = 48;

        public const int InitStringBlockNum = 5;   //block num

        public const char BlankChar = ' ';  //空格字符


        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        const string str1 = "222Assets/";
        const string str2 = "/solider我的222.prefab";

        const string str3 = "111Assets/";
        const string str4 = "/solider111.prefab";

        const string str5 = "   中国    的 ";

        const string str6 = "aaatbbscctdd";
        const string str7 = "222tsse{0}ts{0}//As{1}/As{2}/xcv";
        static char[] splitChars = new char[] { 't', 's' };

        static List<string> strs = new List<string> { "aaa", "bbb", "ccc" };

        public static void Warmup()
        {
            UString externalStr = null;
            using (UString.Block())
            {
                UString ustr1 = "222Assets/As/As/";
                var ustr2 = ustr1.Replace("As", "A中国") + str4;

                ustr2.ToLower();
                ustr2.ToUpper();
                ustr2 = ustr2.Append(100).Append(3.14f).AppendLine("200");
                var index = ustr2.IndexOf("A");
                var lastIndex = ustr2.LastIndexOf("A");

                var result0 = !(ustr2 == null);
                var result1 = ustr2.Contains("A");
                var result2 = ustr2.StartsWith("222a", StringComparison.OrdinalIgnoreCase);
                var result3 = ustr2.EndsWith(".PREFAB");

                var result4 = ustr1 != ustr2 && !ustr1.Equals(ustr2) && !ustr1.Equals(str3);
                var result5 = ustr1 == ustr1.Value && ustr1 != str1;

                ustr2.PadLeft(5, '&').PadRight(5, '*').ToString();
            }

            using (UString.Block())
            {
                UString.Concat(str1, str2, str1, str2, str1, str2).ToString();

                UString.Format("v1:{0},v2:{1},v3:{2}", 200, str3, 300).ToString();

                externalStr = UString.Format("v1:{0},v2:{1},v3:{2}", 100, str3, 300).Intern();
            }

            using (UString.Block())
            {
                UString ustr3 = str5;
                ustr3.Trim().ToString();
                UString.Join('&', strs).Substring(1).ToString();

                UString ustr4 = str7;
                ustr4.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            }
            externalStr?.Dispose();
        }
    }
}
