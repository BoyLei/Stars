using System;
using System.Collections.Generic;
using Yoka.UnityString.Core;

namespace Yoka.UnityString.Examples
{
    public class HtmlTagParser
    {
        public const int MinHtmlTagLength = 5;
        public delegate string HtmlTagHandler(HtmlTag tag);
        public HtmlTagHandler htmlTagHandler;

        public delegate void HtmlTagCustomHandler(UString tag, ref HtmlTag htmlTag);
        public HtmlTagCustomHandler htmlTagCustomHandler;

        public delegate string HtmlTagSuffixHandler(HtmlTag tag);
        public HtmlTagSuffixHandler htmlTagSuffixHandler;

        private Dictionary<string, string> customTags = new Dictionary<string, string>(5);
        private Dictionary<string, int> customTagSpaces = new Dictionary<string, int>(10);
        private Dictionary<string, string> includeTags = new Dictionary<string, string>(10);
        private Dictionary<int, int> tempEndIndex = new Dictionary<int, int>(50);

        private List<int> tmpValiedPos = new List<int>(20);
        private List<int> realValiedPos = new List<int>(50);
        private HtmlTag htmlTag = new HtmlTag() { attrs = new Dictionary<string, string>(10) };

        /// <summary>
        /// 添加占位标记
        /// </summary>
        public void AddOrUpdateCustomTagSpace(string tag, int space)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                customTagSpaces[tag] = space;
            }
        }

        /// <summary>
        /// 移除占位标记
        /// </summary>
        public bool RemoveCustomTagSpace(string tag)
        {
            return customTagSpaces.Remove(tag);
        }

        public void AddCustomTag(string srcTag, string destTag)
        {
            customTags[srcTag] = destTag;
        }

        public bool RemoveCustomTag(string srcTag)
        {
            return customTags.Remove(srcTag);
        }

        /// <summary>
        /// 添加自定义包含占位标记
        /// </summary>
        public void AddCustomIncludeTag(string tag1, string tag2)
        {
            if (!string.IsNullOrEmpty(tag1) && !string.IsNullOrEmpty(tag2))
            {
                includeTags[tag1] = tag2;
            }
        }

        public void RemoveCustomIncludeTag(string tag1, string tag2)
        {
            foreach (var tag in includeTags)
            {
                if (tag.Key == tag1 && tag.Value == tag2)
                {
                    includeTags.Remove(tag1);
                    break;
                }
            }
        }

        public string StartParseHtmlTag(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }
            using (UString.Block())
            {
                UString uString = source;
                var trimStr = uString.Trim();
                if (trimStr.Length <= MinHtmlTagLength)     //最小完整合法标记
                {
                    return source;
                }

                ///custom tag
                foreach (var tag in customTags)
                {
                    if (uString.Contains(tag.Key))
                    {
                        uString = uString.Replace(tag.Key, tag.Value);
                    }
                }
                ValiedTagPos(uString);

                if (realValiedPos.Count <= 1)   //至少成对的tag
                {
                    return source;
                }
                tempEndIndex.Clear();
                var result = ParseHtmlTag(uString);
                return string.IsNullOrEmpty(result) ? source : result;
            }
        }

        string ParseHtmlTag(UString source)
        {
            var vsb = new ValueStringBuilder(stackalloc char[1024 * 2]);
            var txtIndex = 0;
            var lastIndex = 0;
            while (true)
            {
                var startIndex = source.IndexOf('<', lastIndex);
                if (startIndex == -1)
                {
                    lastIndex = source.LastIndexOf('>');
                    if (realValiedPos.Count > 0 && lastIndex > -1)
                    {
                        while (true)
                        {
                            if (!realValiedPos.Contains(lastIndex) && lastIndex > -1)
                            {
                                lastIndex--;
                                lastIndex = source.LastIndexOf('>', lastIndex);
                                continue;
                            }
                            break;
                        }
                    }
                    if (lastIndex < source.Length - 1)
                    {
                        var lastStr = source.Substring(lastIndex + 1);
                        vsb.Append(lastStr.Value);
                    }
                    break;
                }
                lastIndex = startIndex + 1;

                if (!realValiedPos.Contains(startIndex)) //不是合法tag
                {
                    continue;
                }

                var endIndex = source.IndexOf('>', lastIndex);
                lastIndex = endIndex + 1;

                if (!realValiedPos.Contains(endIndex)) //不是合法tag
                {
                    continue;
                }

                if ((startIndex - txtIndex) > 0)    //文字内容
                {
                    var len = startIndex - txtIndex;
                    var txtStr = source.Substring(txtIndex, len);

                    vsb.Append(txtStr.ToString());
                }
                txtIndex = endIndex + 1;


                htmlTag.Reset();
                var subStr = source.Substring(startIndex, endIndex - startIndex + 1);
                if (subStr.StartsWith("</"))
                {
                    if (htmlTagSuffixHandler != null)
                    {
                        htmlTag.tagName = subStr.Value;

                        if (tempEndIndex.TryGetValue(startIndex, out var endPos))
                        {
                            htmlTag.startIndex = 0;
                            htmlTag.endIndex = endPos;
                        }
                        var returnStr = htmlTagSuffixHandler(htmlTag);
                        vsb.Append(returnStr);
                    }
                }
                else
                {
                    var customTag = htmlTag.ParseTag(subStr);
                    if (customTag != null && htmlTagCustomHandler != null)  //正常解析
                    {
                        htmlTagCustomHandler(customTag, ref htmlTag);    //自定义解析
                    }
                    var endTagPos = 0;
                    (htmlTag.startIndex, htmlTag.endIndex, endTagPos) = FindTextIndexPos(source, htmlTag.tagName, txtIndex);

                    if (endTagPos > txtIndex)
                    {
                        tempEndIndex.Add(endTagPos, htmlTag.endIndex);
                    }

                    if (htmlTagHandler != null)
                    {
                        var returnStr = htmlTagHandler(htmlTag);
                        vsb.Append(returnStr);
                    }
                }
            }
            return vsb.ToString();      //have gc
        }

        void ValiedTagPos(UString source)
        {
            tmpValiedPos.Clear();
            realValiedPos.Clear();
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Value[i] == '<' && isValiedTag(source, i, source.Value[i]) ||
                    source.Value[i] == '>' && isValiedTag(source, i, source.Value[i]))
                {
                    realValiedPos.Add(i);
                }
            }
        }

        bool isValiedTag(UString source, int p, char c)
        {
            if (c == '<')
            {
                if (p == source.Length - 1) return false;

                var n1 = source.IndexOf('<', p + 1);
                var n2 = source.IndexOf('>', p + 1);

                if (n2 == -1 || (n1 > -1 && n1 < n2)) return false;

                UString tagName = null;
                var n3 = source.IndexOf("/>", p + 1);
                if (n3 > -1 && (n1 == -1 || n3 < n1))         //<a />
                {
                    tmpValiedPos.Add(n3 + 1);  //mark pos

                    tagName = source.Substring(p + 1, n3 - p);
                    var result = IsVerifiedChar(tagName.Value);    //有特殊字符

                    if (!result)
                    {
                        result = tagName.Contains(" ");
                    }
                    return result;
                }

                if (source.Value[p + 1] == '/')     //</b>
                {
                    tagName = source.Substring(p + 2, n2 - p - 2);
                    if (!IsVerifiedChar(tagName.Value)) return false;

                    var beginTag1 = UString.Concat("<", tagName.Value, ">");    //<a>
                    var beginTag2 = UString.Concat("<", tagName.Value, "=");    //<a=
                    var beginTag3 = UString.Concat("<", tagName.Value, " ");    //<a 
                    if (source.LastIndexOf(beginTag1.Value, p - 1) == -1 && 
                        source.LastIndexOf(beginTag2.Value, p - 1) == -1 &&
                        source.LastIndexOf(beginTag3.Value, p - 1) == -1)
                        return false;

                    tmpValiedPos.Add(n2);  //mark pos
                    return true;
                }

                tagName = source.Substring(p + 1, n2 - p - 1);     //<a>xxx

                var blankIndex = tagName.IndexOf(' ');
                var eqIndex = tagName.IndexOf('=');

                if (blankIndex > -1 && eqIndex > -1)
                {
                    if (blankIndex < eqIndex)
                    {
                        tagName = tagName.Substring(0, blankIndex);
                    }
                }
                else if (eqIndex > 0)
                {
                    tagName = tagName.Substring(0, eqIndex);
                }

                if (!IsVerifiedChar(tagName.Value)) return false;

                UString endTag = UString.Concat("</", tagName.Value, ">");
                if (source.IndexOf(endTag.Value, p + 1) > -1)
                {
                    tmpValiedPos.Add(n2);  //mark pos
                    return true;
                }
                return false;
            }
            else
            {
                if (p == 0) return false;

                var n1 = source.LastIndexOf('<', p - 1);
                var n2 = source.LastIndexOf('>', p - 1);

                if (n1 < n2) return false;

                if (!tmpValiedPos.Contains(p))
                    return false;
            }
            return true;
        }

        bool IsVerifiedChar(string str)
        {
            if (str == null || str.Length == 0) return false;
            for (int i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                var result = ch >= 'a' && ch <= 'z' || (ch >= 'A' && ch <= 'Z');
                if (!result) return false;
            }
            return true;
        }

        (int, int, int) FindTextIndexPos(UString text, string tagName, int indexPos)
        {
            var indexCount = 0;
            var lastPos = indexPos;
            var startTag = UString.StrConcat("<", tagName);
            var endTag = UString.StrConcat("</", tagName, ">");

            while (true)
            {
                var endIndex = text.IndexOf(endTag.Value, lastPos);
                if (endIndex == -1)
                {
                    break;
                }
                var startIndex = text.IndexOf(startTag.Value, lastPos);
                if (startIndex > -1 && startIndex < endIndex)  //tag 嵌套
                {
                    indexCount++;
                    lastPos = text.IndexOf(">", startIndex) + 1;
                }
                else
                {
                    if (indexCount <= 0)
                    {
                        lastPos = Math.Max(lastPos, endIndex);
                        break;
                    }
                    lastPos = text.IndexOf(endTag.Value, lastPos) + endTag.Length;
                    indexCount--;
                }
            }
            var startPos = SearchIndexPos(text, tagName, 0, indexPos - 1);
            var endPos = startPos;
            if (lastPos > indexPos)
            {
                endPos += SearchIndexPos(text, tagName, indexPos, lastPos);
            }
            return (startPos, endPos, lastPos);
        }

        private int SearchIndexPos(UString text, string tagName, int indexPos, int endPos)
        {
            var pos = 0;
            var beginPos = 0;
            var lastPos = indexPos;
            var whiteSpaceCount = 0;

            var tagWhiteSpace = false;
            var isIncTagMode = false;
            includeTags.TryGetValue(tagName, out string value);
            isIncTagMode = !string.IsNullOrEmpty(value);

            for (int i = indexPos; i <= endPos; i++)
            {
                if (char.IsWhiteSpace(text.Value[i]))
                {
                    if (!tagWhiteSpace)
                        whiteSpaceCount++;
                    continue;
                }
                if (!realValiedPos.Contains(i))
                {
                    continue;   //
                }
                if (text.Value[i] == '<')
                {
                    beginPos = i;
                    pos += i - lastPos - 1;
                    tagWhiteSpace = true;
                }
                else if (text.Value[i] == '>')
                {
                    lastPos = i;

                    tagWhiteSpace = false;

                    if (!isIncTagMode)
                        pos += GetCustomTagSpace(text, beginPos);
                }
            }
            if (pos <= 0) return 0;

            var isCustomTagValue = customTagSpaces.ContainsKey(tagName);
            if (isIncTagMode)
                pos = isCustomTagValue ? 0 : pos;
            var resultPos = indexPos > 0 || isCustomTagValue ? pos : pos + 1;
            return resultPos - whiteSpaceCount;
        }

        int GetCustomTagSpace(UString text, int index)
        {
            int tagSpace = 0;
            if (customTagSpaces.Count > 0)
            {
                foreach (var de in customTagSpaces)
                {
                    var subTagStr = text.Substring(index + 1, de.Key.Length);
                    if (subTagStr == de.Key)
                    {
                        tagSpace = customTagSpaces[de.Key];
                        break;
                    }
                }
            }
            return tagSpace;
        }
    }
}