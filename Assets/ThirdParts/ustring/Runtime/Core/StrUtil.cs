using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Yoka.UnityString.Core
{
    public static class StrUtil
    {
        public static unsafe void StrCpy(string dst, string src)
        {
            StrCpy(dst, src.AsSpan());
        }

        public static unsafe void StrCpy(string dst, ReadOnlySpan<char> src, int count = 0)
        {
            fixed (char* source = src)
            fixed (char* target = dst)
            {
                UnsafeUtility.MemCpy(target, source, (uint)src.Length * sizeof(char));
            }
        }

        public static unsafe void StrCpy(string dst, string src, int offset_dst, int offset_src, int size)
        {
            fixed (char* source = src)
            fixed (char* target = dst)
            {
                UnsafeUtility.MemCpy(target + offset_dst, source + offset_src, size * sizeof(char));
            }
        }

        public static unsafe string ToLower(this string str)
        {
            fixed (char* dest = str)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    dest[i] = char.ToLower(dest[i]);
                }
            }
            return str;
        }

        public static unsafe string ToUpper(this string str)
        {
            fixed (char* dest = str)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    dest[i] = char.ToUpper(dest[i]);
                }
            }
            return str;
        }

        public static UString ToUString(string src, int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex < 0 || startIndex > endIndex)
            {
                throw new ArgumentOutOfRangeException();
            }
            var length = endIndex - startIndex + 1;
            var result = UString.Get(length);
            StrCpy(result.Value, src, 0, startIndex, length);
            return result;
        }

        public static int GetDigitCount(int value)
        {
            int cnt;
            for (cnt = 1; (value /= 10) > 0; cnt++) ;
            return cnt;
        }

        public static int GetDigitCount(uint value)
        {
            int cnt;
            for (cnt = 1; (value /= 10) > 0; cnt++) ;
            return cnt;
        }

        public static int GetDigitCount(float value)
        {
            int cnt;
            long mul = (long)Math.Pow(10, StrDefine.DecimalAccuracy);
            long number = (long)(value * mul); // gets the number as a whole, e.g. 3148
            int left_num = (int)(number / mul); // left part of the decimal point, e.g. 3
            int right_num = (int)(number % mul); // right part of the decimal pnt, e.g. 148
            int left_digit_count = GetDigitCount(left_num); // e.g. 1
            int right_digit_count = GetDigitCount(right_num); // e.g. 3
            cnt = left_digit_count + right_digit_count + 1;
            return cnt;
        }

        public static int GetDigitCount(long value)
        {
            int cnt;
            for (cnt = 1; (value /= 10) > 0; cnt++) ;
            return cnt;
        }

        public static unsafe string IntToStr(string dst, int v)
        {
            bool negative = v < 0;
            v = Math.Abs(v);
            int count = GetDigitCount(v);
            if (negative)
            {
                fixed (char* ptr = dst)
                {
                    *ptr = '-';
                    Intcpy(ptr, v, 0, count);
                }
            }
            else
            {
                fixed (char* ptr = dst)
                    Intcpy(ptr, v, 0, count);
            }
            return dst;
        }

        internal unsafe static void Intcpy(char* dst, int value, int start, int count)
        {
            int end = start + count;
            for (int i = end - 1; i >= start; i--, value /= 10)
                *(dst + i) = (char)(value % 10 + 48);
        }

        public static unsafe string UIntToStr(string dst, uint v)
        {
            int count = GetDigitCount(v);
            fixed (char* ptr = dst)
                UIntcpy(ptr, v, 0, count);
            return dst;
        }

        internal unsafe static void UIntcpy(char* dst, uint value, int start, int count)
        {
            int end = start + count;
            for (int i = end - 1; i >= start; i--, value /= 10)
                *(dst + i) = (char)(value % 10 + 48);
        }

        public static unsafe string FloatToStr(string dst, float value)
        {
            bool negative = value < 0;
            if (negative) value = -value;
            long mul = (long)Math.Pow(10, StrDefine.DecimalAccuracy);
            long number = (long)(value * mul); // gets the number as a whole, e.g. 3148
            int left_num = (int)(number / mul); // left part of the decimal point, e.g. 3
            int right_num = (int)(number % mul); // right part of the decimal pnt, e.g. 148
            int left_digit_count = GetDigitCount(left_num); // e.g. 1
            int right_digit_count = GetDigitCount(right_num); // e.g. 3

            if (negative)
            {
                fixed (char* ptr = dst)
                {
                    *ptr = '-';
                    Intcpy(ptr, left_num, 1, left_digit_count);
                    *(ptr + left_digit_count + 1) = '.';
                    int offest = (int)StrDefine.DecimalAccuracy - right_digit_count;
                    for (int i = 0; i < offest; i++)
                        *(ptr + left_digit_count + i + 1) = '0';
                    Intcpy(ptr, right_num, left_digit_count + 2 + offest, right_digit_count);
                }
            }
            else
            {
                fixed (char* ptr = dst)
                {
                    Intcpy(ptr, left_num, 0, left_digit_count);
                    *(ptr + left_digit_count) = '.';
                    int offest = (int)StrDefine.DecimalAccuracy - right_digit_count;
                    for (int i = 0; i < offest; i++)
                        *(ptr + left_digit_count + i + 1) = '0';
                    Intcpy(ptr, right_num, left_digit_count + 1 + offest, right_digit_count);
                }
            }
            return dst;
        }

        public static unsafe string LongToStr(string dst, long v)
        {
            bool negative = v < 0;
            v = Math.Abs(v);
            int num_digits = GetDigitCount(v);
            if (negative)
            {
                fixed (char* ptr = dst)
                {
                    *ptr = '-';
                    Longcpy(ptr, v, 1, num_digits);
                }
            }
            else
            {
                fixed (char* ptr = dst)
                    Longcpy(ptr, v, 0, num_digits);
            }
            return dst;
        }

        private unsafe static void Longcpy(char* dst, long value, int start, int count)
        {
            int end = start + count;
            for (int i = end - 1; i >= start; i--, value /= 10)
                *(dst + i) = (char)(value % 10 + 48);
        }
    }
}
