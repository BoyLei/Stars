using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Yoka.UnityString.Interface;

namespace Yoka.UnityString.Core
{
    public class UString : IDisposable
    {
        static UStringBlock currentBlock = null;
        static Stack<UStringBlock> stack = new Stack<UStringBlock>(32);
        static Queue<UStringBlock> blocks = new Queue<UStringBlock>(32);
        static Dictionary<int, UString> externalCache = new Dictionary<int, UString>(64);

        static readonly UString emptyStr = new UString();

        private int _length;
        private string _value;
        private List<UString> _splitList = new List<UString>(48);

        public string Value => _value;
        public int Length => _length;

        public static void Initialize()
        {
            StringPool.InitPools();
            for (int i = 0; i < StrDefine.InitStringBlockNum; i++)
            {
                blocks.Enqueue(new UStringBlock());
            }
            StrDefine.Warmup();
        }

        public UString()
        {
            this._length = 0;
            this._value = string.Empty;
        }

        /// <summary>
        /// 不受block管理，受StringPool管理
        /// </summary>
        public UString(char v, int size)
        {
            this._length = size;
            this._value = new string(v, size);
        }

        public char this[int i]
        {
            get { return _value[i]; }
        }

        public UString Append<T>(T arg1)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            return this + strArg1;
        }

        public UString AppendLine<T>(T arg1)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            return this + strArg1 + "\n";
        }

        public UString Substring(int startIndex, int len = 0)
        {
            if (startIndex < 0 || len < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (len == 0)
            {
                len = _length - startIndex;
            }
            var srcSpan = _value.AsSpan(startIndex, len);
            return NewUString(srcSpan);
        }

        public UString Remove(int start)
        {
            return Remove(start, Length - start);
        }

        public UString Remove(int startIndex, int len)
        {
            if (startIndex < 0 || len < 0 || startIndex > _length - len)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (startIndex == 0 && len == _length)
            {
                return emptyStr;
            }
            var newLen = _length - len;
            var newStr = Get(newLen);

            if (startIndex > 0)
            {
                StrUtil.StrCpy(newStr.Value, _value, 0, 0, startIndex);
            }
            StrUtil.StrCpy(newStr.Value, _value, startIndex, startIndex + len, newLen - startIndex);
            return newStr;
        }

        public string ToLower()
        {
            StrUtil.ToLower(_value);
            return _value;
        }

        public unsafe string ToUpper()
        {
            StrUtil.ToUpper(_value);
            return _value;
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(_value);
        }

        public bool IsNullOrWhiteSpace()
        {
            return string.IsNullOrWhiteSpace(_value);
        }

        public string Reverse()
        {
            var vsb = new ValueStringBuilder(stackalloc char[_length]);
            vsb.Append(_value);
            vsb.Reverse();

            StrUtil.StrCpy(_value, vsb.AsSpan());
            return _value;
        }

        public UString Replace(char c, char d)
        {
            var vsb = new ValueStringBuilder(stackalloc char[_length]);
            vsb.Append(_value);
            vsb.Replace(c, d, 0, _value.Length);

            return NewUString(vsb.AsSpan());
        }

        public UString Replace(string str, string dest)
        {
            if (string.IsNullOrEmpty(str) || dest == null)
            {
                throw new ArgumentOutOfRangeException();
            }
            int matchCount = 0;
            int startIndex = 0;
            int endIndex = startIndex + _value.Length;
            var oldValue = str.AsSpan();
            var newValue = dest.AsSpan();

            var readOnlySpan = _value.AsSpan();
            for (int i = startIndex; i < endIndex; i += oldValue.Length)
            {
                var span = readOnlySpan.Slice(i, endIndex - i);
                var pos = span.IndexOf(oldValue/*, StringComparison.Ordinal*/);
                if (pos == -1)
                {
                    break;
                }
                i += pos;
                matchCount++;
            }

            if (matchCount == 0)
                return this;

            var newBufferIndex = startIndex;
            var newlen = readOnlySpan.Length + (newValue.Length - oldValue.Length) * matchCount;
            Span<char> newBuffer = stackalloc char[newlen];

            for (int i = startIndex; i < endIndex; i += oldValue.Length)
            {
                var span = readOnlySpan.Slice(i, endIndex - i);
                var pos = span.IndexOf(oldValue);
                if (pos == -1)
                {
                    var remain = readOnlySpan.Slice(i);
                    remain.CopyTo(newBuffer.Slice(newBufferIndex));
                    newBufferIndex += remain.Length;
                    break;
                }
                readOnlySpan.Slice(i, pos).CopyTo(newBuffer.Slice(newBufferIndex));
                newValue.CopyTo(newBuffer.Slice(newBufferIndex + pos));
                newBufferIndex += pos + newValue.Length;
                i += pos;
            }
            return NewUString(newBuffer);
        }

        public List<UString> Split(char c, StringSplitOptions options = StringSplitOptions.None)
        {
            var last = 0;
            _splitList.Clear();
            while (true)
            {
                var index = _value.IndexOf(c, last);
                if (index == -1)
                {
                    if (last <= _value.Length - 1)
                    {
                        TryAddToSplitList(last, _value.Length - 1);
                    }
                    break;
                }
                if (index == last)
                {
                    if (options == StringSplitOptions.None)
                        _splitList.Add(emptyStr);
                }
                else
                {
                    TryAddToSplitList(last, index - 1);
                }
                last = index + 1;
            }
            return _splitList;
        }

        public List<UString> Split(string str, StringSplitOptions options = StringSplitOptions.None)
        {
            var last = 0;
            _splitList.Clear();
            while (true)
            {
                var index = _value.IndexOf(str, last);
                if (index == -1)
                {
                    if (last <= _value.Length - 1)
                    {
                        TryAddToSplitList(last, _value.Length - 1);
                    }
                    break;
                }
                if (index == last)
                {
                    if (options == StringSplitOptions.None)
                        _splitList.Add(emptyStr);
                }
                else
                {
                    TryAddToSplitList(last, index - 1);
                }
                last = index + str.Length;
            }
            return _splitList;
        }

        public List<UString> Split(char[] separators, StringSplitOptions options = StringSplitOptions.None)
        {
            if (separators == null || separators.Length == 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            var last = 0;
            _splitList.Clear();
            while (true)
            {
                var index = -1;
                foreach (var c in separators)
                {
                    var pos = _value.IndexOf(c, last);
                    if (index == -1 || (pos < index && pos > -1))
                    {
                        index = pos;
                    }
                }
                if (index == -1)
                {
                    if (last <= _value.Length - 1)
                    {
                        TryAddToSplitList(last, _value.Length - 1);
                    }
                    break;
                }
                if (index == last)
                {
                    if (options == StringSplitOptions.None)
                        _splitList.Add(emptyStr);
                }
                else
                {
                    TryAddToSplitList(last, index - 1);
                }
                last = index + 1;
            }
            return _splitList;
        }

        void TryAddToSplitList(int startIndex, int endIndex, StringSplitOptions options = StringSplitOptions.None)
        {
            var subStr = StrUtil.ToUString(_value, startIndex, endIndex);
            if (subStr.IsNullOrEmpty())
            {
                if (options == StringSplitOptions.None)
                {
                    _splitList.Add(subStr);
                }
            }
            else
            {
                _splitList.Add(subStr);
            }
        }

        public UString Trim()
        {
            var startIndex = 0;
            var endIndex = 0;
            for (int i = 0; i < _length; i++)
            {
                if (_value[i] != StrDefine.BlankChar)
                {
                    startIndex = i; break;
                }
            }
            for (int i = _length - 1; i >= 0; i--)
            {
                if (_value[i] != StrDefine.BlankChar)
                {
                    endIndex = i; break;
                }
            }
            var newlen = endIndex - startIndex + 1;
            var srcSpan = _value.AsSpan(startIndex, newlen);
            return NewUString(srcSpan);
        }

        public UString TrimStart()
        {
            var index = 0;
            for (int i = 0; i < _length; i++)
            {
                if (!char.IsWhiteSpace(_value[i]))
                {
                    index = i; break;
                }
            }
            var newlen = index + 1;
            var srcSpan = _value.AsSpan(index, newlen);
            return NewUString(srcSpan);
        }

        public UString TrimEnd()
        {
            var index = 0;
            for (int i = _length - 1; i >= 0; i--)
            {
                if (!char.IsWhiteSpace(_value[i]))
                {
                    index = i; break;
                }
            }
            var newlen = index + 1;
            var srcSpan = _value.AsSpan(0, newlen);
            return NewUString(srcSpan);
        }

        public UString Insert(int startPos, string str)
        {
            if (startPos < 0 || string.IsNullOrEmpty(str))
            {
                throw new ArgumentOutOfRangeException();
            }
            var newlen = _length + str.Length;
            var vsb = new ValueStringBuilder(stackalloc char[newlen]);
            vsb.Append(_value);
            vsb.Insert(startPos, str);
            return NewUString(vsb.AsSpan());
        }

        public bool StartsWith(string str, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentOutOfRangeException();
            }
            return _value.StartsWith(str, comparisonType);
        }

        public bool StartsWith(UString str, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return _value.StartsWith(str.Value, comparisonType);
        }

        public bool EndsWith(UString str, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return _value.EndsWith(str.Value, comparisonType);
        }

        public UString PadLeft(int num)
        {
            return PadLeft(num, StrDefine.BlankChar);
        }

        public UString PadLeft(int num, char paddingChar)
        {
            var newlen = _length + num;
            var vsb = new ValueStringBuilder(stackalloc char[newlen]);
            for (int i = 0; i < num; i++)
            {
                vsb.Append(paddingChar);
            }
            vsb.Append(_value);
            return NewUString(vsb.AsSpan());
        }

        public UString PadRight(int num)
        {
            return PadRight(num, StrDefine.BlankChar);
        }

        public UString PadRight(int num, char paddingChar)
        {
            var newlen = _length + num;
            var vsb = new ValueStringBuilder(stackalloc char[newlen]);
            vsb.Append(_value);
            for (int i = 0; i < num; i++)
            {
                vsb.Append(paddingChar);
            }
            return NewUString(vsb.AsSpan());
        }

        public int IndexOf(char c)
        {
            return _value.IndexOf(c);
        }

        public int IndexOf(char c, int index)
        {
            return _value.IndexOf(c, index);
        }

        public int IndexOf(string str)
        {
            return _value.IndexOf(str);
        }

        public int IndexOf(string str, int index)
        {
            return _value.IndexOf(str, index);
        }

        public int IndexOfAny(char[] chars)
        {
            return _value.IndexOfAny(chars);
        }

        public int IndexOfAny(char[] chars, int index)
        {
            return _value.IndexOfAny(chars, index);
        }

        public int IndexOfAny(char[] chars, int index, int count)
        {
            return _value.IndexOfAny(chars, index, count);
        }

        public int LastIndexOf(char c)
        {
            return _value.LastIndexOf(c);
        }

        public int LastIndexOf(char c, int index)
        {
            return _value.LastIndexOf(c, index);
        }

        public int LastIndexOf(string str)
        {
            return _value.LastIndexOf(str);
        }

        public int LastIndexOf(string str, int index)
        {
            return _value.LastIndexOf(str, index);
        }

        public int LastIndexOf(char[] anyOf)
        {
            return _value.LastIndexOfAny(anyOf);
        }

        public int LastIndexOfAny(char[] anyOf, int startIndex)
        {
            return _value.LastIndexOfAny(anyOf, startIndex);
        }

        public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
        {
            return _value.LastIndexOfAny(anyOf, startIndex, count);
        }

        public bool Contains(char c)
        {
            return _value.IndexOf(c) > -1;
        }

        public bool Contains(string str)
        {
            return _value.Contains(str);
        }

        public bool Equals(UString value)
        {
            return Equals(this, value);
        }

        public bool Equals(string value)
        {
            return Equals(_value, value);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(_value);
        }

        /// <summary>
        /// 需要自己手动Dispose
        /// </summary>
        public UString Intern()
        {
            var hashCode = GetHashCode();
            if (externalCache.ContainsKey(hashCode))
            {
                return this;
            }
            var newStr = StringPool.Get(_length);
            StrUtil.StrCpy(newStr.Value, Value);
            externalCache.Add(newStr.GetHashCode(), newStr);   //放入外部缓存池
            return newStr;
        }

        void CopyFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            StrUtil.StrCpy(_value, str);
            _length = str.Length;
        }

        public void Dispose()
        {
            if (this == emptyStr) return;
            var hashCode = GetHashCode();
            externalCache.Remove(hashCode);
            Release(this);
        }

        public string Clone()   //gc
        {
            if (_length > 0)
            {
                var newStr = new string(StrDefine.NEW_ALLOC_CHAR, _length);
                StrUtil.StrCpy(newStr, _value);
                return newStr;
            }
            return string.Empty;
        }

        public override string ToString()
        {
            return _value;
        }

        /////////////////////////////////////////////////////////////Static//////////////////////////////////////////////////////////////

        public static implicit operator UString(string str)
        {
            if (str.Length == 0)
            {
                return emptyStr;
            }
            var temp = Get(str.Length);
            temp.CopyFromString(str);
            return temp;
        }

        public static UString operator +(UString left, UString right)
        {
            if (left == null || right == null)
            {
                throw new ArgumentException("Argument was null!!~~");
            }
            return StrConcat(left.Value, right.Value);
        }

        public static UString operator +(UString left, string right)
        {
            if (left == null || string.IsNullOrEmpty(right))
            {
                throw new ArgumentException("Argument was null!!~~");
            }
            return StrConcat(left.Value, right);
        }

        public static bool operator ==(UString left, UString right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);
            if (ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        public static bool operator ==(UString left, string right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);
            return left.Value == right;
        }

        public static bool operator !=(UString left, UString right)
        {
            if (ReferenceEquals(left, null))
                return !ReferenceEquals(right, null);
            if (ReferenceEquals(right, null))
                return true;
            return left.Value != right.Value;
        }

        public static bool operator !=(UString left, string right)
        {
            if (ReferenceEquals(left, null))
                return !ReferenceEquals(right, null);
            return left.Value != right;
        }

        public static bool Equals(UString a, UString b)
        {
            if (a != null && b != null)
            {
                return Equals(a.Value, b.Value);
            }
            return a == b;
        }

        private static bool Equals(string a, string b)
        {
            return a == b;
        }

        public static UString Get(int size)
        {
            if (size == 0) return emptyStr;

            var newStr = StringPool.Get(size);
            if (currentBlock != null)
            {
                currentBlock.Push(newStr);
            }
            else
            {
                throw new InvalidOperationException(nameof(currentBlock));
            }
            return newStr;
        }

        public static void Release(UString strobj)
        {
            if (strobj != null)
            {
                StringPool.Release(strobj);
            }
        }

        public static int GetCacheCapacity(int length)
        {
            return StringPool.Count(length);
        }

        static UString NewUString(string value)
        {
            return NewUString(value.AsSpan());
        }

        static UString NewUString(ReadOnlySpan<char> span)
        {
            var usingStr = Get(span.Length);
            StrUtil.StrCpy(usingStr.Value, span);
            return usingStr;
        }

        static bool IsNeedAppendChar(string path)
        {
            char c = path[path.Length - 1];
            if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar && c != Path.VolumeSeparatorChar)
            {
                return true;
            }
            return false;
        }

        static void CheckInvalidPathChars(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
        }

        public static UString Combine(string path1, string path2)
        {
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);

            var maxLength = path1.Length + path2.Length + 1;
            var vsb = new ValueStringBuilder(stackalloc char[maxLength]);
            vsb.Append(path1);
            if (IsNeedAppendChar(path1))
                vsb.Append("\\");
            vsb.Append(path2);
            return NewUString(vsb.AsSpan());
        }

        public static UString Combine(string path1, string path2, string path3)
        {
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);
            CheckInvalidPathChars(path3);

            var maxLength = path1.Length + path2.Length + path3.Length + 2;
            var vsb = new ValueStringBuilder(stackalloc char[maxLength]);
            vsb.Append(path1);
            if (IsNeedAppendChar(path1))
                vsb.Append("\\");
            vsb.Append(path2);
            if (IsNeedAppendChar(path2))
                vsb.Append("\\");
            vsb.Append(path3);
            return NewUString(vsb.AsSpan());
        }

        public static UString Combine(string path1, string path2, string path3, string path4)
        {
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);
            CheckInvalidPathChars(path3);
            CheckInvalidPathChars(path4);

            var maxLength = path1.Length + path2.Length + path3.Length + path4.Length + 3;
            var vsb = new ValueStringBuilder(stackalloc char[maxLength]);
            vsb.Append(path1);
            if (IsNeedAppendChar(path1))
                vsb.Append("\\");
            vsb.Append(path2);
            if (IsNeedAppendChar(path2))
                vsb.Append("\\");
            vsb.Append(path3);
            if (IsNeedAppendChar(path3))
                vsb.Append("\\");
            vsb.Append(path4);
            return NewUString(vsb.AsSpan());
        }

        public static UString Join<T>(char separator, List<T> values)
        {
            var vsb = new ValueStringBuilder(stackalloc char[StrDefine.MAX_STRING_SIZE]);
            int index = 0;
            int length = 0;
            int count = values.Count;
            foreach (var arg in values)
            {
                var strArg = ConvertArgvToUString(arg);
                vsb.Append(strArg.Value);
                length += strArg.Length;
                if (index < count - 1)
                {
                    vsb.Append(separator);
                    length++;
                }
                index++;
            }
            return NewUString(vsb.AsSpan());
        }

        /// <summary>
        /// 这个版本效率最高，因为只有一次内存copy
        /// </summary>
        public static UString StrConcat(string arg1, string arg2, string arg3 = null, string arg4 = null, string arg5 = null,
            string arg6 = null, string arg7 = null, string arg8 = null, string arg9 = null)
        {
            if (arg1 == null || arg2 == null)
            {
                throw new Exception("Concat arguments cannot be null!!!");
            }
            var length = arg1.Length + arg2.Length;
            if (arg3 != null) length += arg3.Length;
            if (arg4 != null) length += arg4.Length;
            if (arg5 != null) length += arg5.Length;
            if (arg6 != null) length += arg6.Length;
            if (arg7 != null) length += arg7.Length;
            if (arg8 != null) length += arg8.Length;
            if (arg9 != null) length += arg9.Length;

            if (length > StrDefine.MAX_STRING_SIZE)
            {
                throw new IndexOutOfRangeException(nameof(length));
            }
            var vsb = new ValueStringBuilder(stackalloc char[length]);
            vsb.Append(arg1);
            vsb.Append(arg2);

            if (arg3 != null) vsb.Append(arg3);
            if (arg4 != null) vsb.Append(arg4);
            if (arg5 != null) vsb.Append(arg5);
            if (arg6 != null) vsb.Append(arg6);
            if (arg7 != null) vsb.Append(arg7);
            if (arg8 != null) vsb.Append(arg8);
            if (arg9 != null) vsb.Append(arg9);

            return NewUString(vsb.AsSpan());
        }

        public static UString Concat<T1, T2>(T1 arg1, T2 arg2)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            var strArg2 = ConvertArgvToUString(arg2);
            return StrConcat(strArg1.Value, strArg2.Value);
        }
        public static UString Concat<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            var strArg2 = ConvertArgvToUString(arg2);
            var strArg3 = ConvertArgvToUString(arg3);
            return StrConcat(strArg1.Value, strArg2.Value, strArg3.Value);
        }
        public static UString Concat<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            var strArg2 = ConvertArgvToUString(arg2);
            var strArg3 = ConvertArgvToUString(arg3);
            var strArg4 = ConvertArgvToUString(arg4);
            return StrConcat(strArg1.Value, strArg2.Value, strArg3.Value, strArg4.Value);
        }
        public static UString Concat<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            var strArg2 = ConvertArgvToUString(arg2);
            var strArg3 = ConvertArgvToUString(arg3);
            var strArg4 = ConvertArgvToUString(arg4);
            var strArg5 = ConvertArgvToUString(arg5);
            return StrConcat(strArg1.Value, strArg2.Value, strArg3.Value, strArg4.Value, strArg5.Value);
        }
        public static UString Concat<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            var strArg2 = ConvertArgvToUString(arg2);
            var strArg3 = ConvertArgvToUString(arg3);
            var strArg4 = ConvertArgvToUString(arg4);
            var strArg5 = ConvertArgvToUString(arg5);
            var strArg6 = ConvertArgvToUString(arg6);
            return StrConcat(strArg1.Value, strArg2.Value, strArg3.Value, strArg4.Value, strArg5.Value, strArg6.Value);
        }
        public static UString Concat<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            var strArg2 = ConvertArgvToUString(arg2);
            var strArg3 = ConvertArgvToUString(arg3);
            var strArg4 = ConvertArgvToUString(arg4);
            var strArg5 = ConvertArgvToUString(arg5);
            var strArg6 = ConvertArgvToUString(arg6);
            var strArg7 = ConvertArgvToUString(arg7);
            return StrConcat(strArg1.Value, strArg2.Value, strArg3.Value, strArg4.Value, strArg5.Value, strArg6.Value, strArg7.Value);
        }
        public static UString Concat<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            var strArg2 = ConvertArgvToUString(arg2);
            var strArg3 = ConvertArgvToUString(arg3);
            var strArg4 = ConvertArgvToUString(arg4);
            var strArg5 = ConvertArgvToUString(arg5);
            var strArg6 = ConvertArgvToUString(arg6);
            var strArg7 = ConvertArgvToUString(arg7);
            var strArg8 = ConvertArgvToUString(arg8);
            return StrConcat(strArg1.Value, strArg2.Value, strArg3.Value, strArg4.Value, strArg5.Value, strArg6.Value, strArg7.Value, strArg8.Value);
        }
        public static UString Concat<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            var strArg1 = ConvertArgvToUString(arg1);
            var strArg2 = ConvertArgvToUString(arg2);
            var strArg3 = ConvertArgvToUString(arg3);
            var strArg4 = ConvertArgvToUString(arg4);
            var strArg5 = ConvertArgvToUString(arg5);
            var strArg6 = ConvertArgvToUString(arg6);
            var strArg7 = ConvertArgvToUString(arg7);
            var strArg8 = ConvertArgvToUString(arg8);
            var strArg9 = ConvertArgvToUString(arg9);
            return StrConcat(strArg1.Value, strArg2.Value, strArg3.Value, strArg4.Value, strArg5.Value, strArg6.Value, strArg7.Value, strArg8.Value, strArg9.Value);
        }

        public static UString Format<T1>(string format, T1 arg1)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);

            return NewUString(input.Value);
        }

        public static UString Format<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);
            input = ProcFormatArgv(arg2, input, StrDefine.FM_ARGV_2);

            return NewUString(input.Value);
        }

        public static UString Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);
            input = ProcFormatArgv(arg2, input, StrDefine.FM_ARGV_2);
            input = ProcFormatArgv(arg3, input, StrDefine.FM_ARGV_3);

            return NewUString(input.Value);
        }

        public static UString Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);
            input = ProcFormatArgv(arg2, input, StrDefine.FM_ARGV_2);
            input = ProcFormatArgv(arg3, input, StrDefine.FM_ARGV_3);
            input = ProcFormatArgv(arg4, input, StrDefine.FM_ARGV_4);

            return NewUString(input.Value);
        }

        public static UString Format<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);
            input = ProcFormatArgv(arg2, input, StrDefine.FM_ARGV_2);
            input = ProcFormatArgv(arg3, input, StrDefine.FM_ARGV_3);
            input = ProcFormatArgv(arg4, input, StrDefine.FM_ARGV_4);
            input = ProcFormatArgv(arg5, input, StrDefine.FM_ARGV_5);

            return NewUString(input.Value);
        }

        public static UString Format<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);
            input = ProcFormatArgv(arg2, input, StrDefine.FM_ARGV_2);
            input = ProcFormatArgv(arg3, input, StrDefine.FM_ARGV_3);
            input = ProcFormatArgv(arg4, input, StrDefine.FM_ARGV_4);
            input = ProcFormatArgv(arg5, input, StrDefine.FM_ARGV_5);
            input = ProcFormatArgv(arg6, input, StrDefine.FM_ARGV_6);

            return NewUString(input.Value);
        }

        public static UString Format<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);
            input = ProcFormatArgv(arg2, input, StrDefine.FM_ARGV_2);
            input = ProcFormatArgv(arg3, input, StrDefine.FM_ARGV_3);
            input = ProcFormatArgv(arg4, input, StrDefine.FM_ARGV_4);
            input = ProcFormatArgv(arg5, input, StrDefine.FM_ARGV_5);
            input = ProcFormatArgv(arg6, input, StrDefine.FM_ARGV_6);
            input = ProcFormatArgv(arg7, input, StrDefine.FM_ARGV_7);

            return NewUString(input.Value);
        }

        public static UString Format<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);
            input = ProcFormatArgv(arg2, input, StrDefine.FM_ARGV_2);
            input = ProcFormatArgv(arg3, input, StrDefine.FM_ARGV_3);
            input = ProcFormatArgv(arg4, input, StrDefine.FM_ARGV_4);
            input = ProcFormatArgv(arg5, input, StrDefine.FM_ARGV_5);
            input = ProcFormatArgv(arg6, input, StrDefine.FM_ARGV_6);
            input = ProcFormatArgv(arg7, input, StrDefine.FM_ARGV_7);
            input = ProcFormatArgv(arg8, input, StrDefine.FM_ARGV_8);

            return NewUString(input.Value);
        }

        public static UString Format<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            UString input = format;     //init
            input = ProcFormatArgv(arg1, input, StrDefine.FM_ARGV_1);
            input = ProcFormatArgv(arg2, input, StrDefine.FM_ARGV_2);
            input = ProcFormatArgv(arg3, input, StrDefine.FM_ARGV_3);
            input = ProcFormatArgv(arg4, input, StrDefine.FM_ARGV_4);
            input = ProcFormatArgv(arg5, input, StrDefine.FM_ARGV_5);
            input = ProcFormatArgv(arg6, input, StrDefine.FM_ARGV_6);
            input = ProcFormatArgv(arg7, input, StrDefine.FM_ARGV_7);
            input = ProcFormatArgv(arg8, input, StrDefine.FM_ARGV_8);
            input = ProcFormatArgv(arg9, input, StrDefine.FM_ARGV_9);

            return NewUString(input.Value);
        }

        static UString ProcFormatArgv<T>(T arg, UString input, string mask = null)
        {
            UString result = ConvertArgvToUString(arg);
            if (!string.IsNullOrEmpty(mask))
            {
                if (result != null && input.Contains(mask))
                {
                    return input.Replace(mask, result.Value);
                }
            }
            return emptyStr;
        }

        static UString ConvertArgvToUString<T>(T arg)
        {
            UString result = emptyStr;
            if (typeof(T) == typeof(string))
            {
                result = FormatString(Unsafe.As<T, string>(ref arg));
            }
            else if (typeof(T) == typeof(int))
            {
                result = FormatInt(Unsafe.As<T, int>(ref arg));
            }
            else if (typeof(T) == typeof(long))
            {
                result = FormatLong(Unsafe.As<T, long>(ref arg));
            }
            else if (typeof(T) == typeof(float))
            {
                result = FormatFloat(Unsafe.As<T, float>(ref arg));
            }
            else if (typeof(T) == typeof(uint))
            {
                result = FormatUInt(Unsafe.As<T, uint>(ref arg));
            }
            else if(typeof(T) == typeof(bool))
            {
                result = FormatBool(Unsafe.As<T, bool>(ref arg));
            }
            else
            {
                throw new NotSupportedException(nameof(arg));
            }
            return result;
        }

        static UString FormatBool(bool value)
        {
            var str = value ? StrDefine.STR_TRUE : StrDefine.STR_FALSE;
            var newStr = Get(str.Length);
            StrUtil.StrCpy(newStr.Value, str);
            return newStr;
        }

        static UString FormatString(string v)
        {
            if (!string.IsNullOrEmpty(v))
            {
                return NewUString(v);
            }
            return emptyStr;
        }

        static UString FormatInt(int v)
        {
            var len = StrUtil.GetDigitCount(v);
            var newStr = Get(len);
            StrUtil.IntToStr(newStr.Value, v);
            return newStr;
        }

        static UString FormatUInt(uint v)
        {
            var len = StrUtil.GetDigitCount(v);
            var newStr = Get(len);
            StrUtil.UIntToStr(newStr.Value, v);
            return newStr;
        }

        private static UString FormatFloat(float v)
        {
            var len = StrUtil.GetDigitCount(v);
            var newStr = Get(len);
            StrUtil.FloatToStr(newStr.Value, v);
            return newStr;
        }

        private static UString FormatLong(long v)
        {
            var len = StrUtil.GetDigitCount(v);
            var newStr = Get(len);
            StrUtil.LongToStr(newStr.Value, v);
            return newStr;
        }

        ////////////////////////////////////////////////////////////////StringBlock//////////////////////////////////////////////////////////////////

        public static IStringBlock Block()   //block模式
        {
            UStringBlock block = null;
            if (blocks.Count > 0)
            {
                block = blocks.Dequeue();
            }
            else
            {
                block = new UStringBlock();
            }
            block.Init();
            stack.Push(block);
            currentBlock = block;
            return block;
        }

        public static bool IsBlocking()
        {
            return currentBlock != null;
        }

        internal class UStringBlock : IStringBlock
        {
            private List<UString> list = new List<UString>(48);
            private bool beDisposed = false;

            public UStringBlock()
            {
            }

            public void Init()
            {
                beDisposed = false;
            }

            public void Push(UString str)
            {
                list.Add(str);
            }

            public bool Remove(UString str)
            {
                return list.Remove(str);
            }

            public void Dispose()
            {
                if (beDisposed)
                {
                    return;
                }
                if (currentBlock != this)
                {
                    throw new Exception("Dispose in it's own block");
                }
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Dispose();
                }
                list.Clear();
                blocks.Enqueue(this);
                stack.Pop();
                currentBlock = stack.Count > 0 ? stack.Peek() : null;
                beDisposed = true;
            }
        }
    }
}