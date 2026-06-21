using System;
using System.Collections.Generic;
using Yoka.UnityString.Core;

namespace Yoka.UnityString.Examples
{
    public class HtmlTag
    {
        public string tagName;
        public string tagValue;
        public int startIndex;
        public int endIndex;
        public Dictionary<string, string> attrs;
        public string rawData;

        public UString ParseTag(UString tag)
        {
            rawData = tag.ToString();
            return ParseInternal(tag);
        }

        private UString ParseInternal(UString tagStr)
        {
            var newStr = tagStr.Remove(tagStr.Length - 1, 1).Remove(0, 1);

            var index = newStr.IndexOf("/");
            if (index > -1)
            {
                newStr = newStr.Substring(0, index);
            }
            if (newStr.Contains(" ") && newStr.Contains("="))   //quad width=4
            {
                var eqIndex = newStr.IndexOf(' ');
                tagName = newStr.Substring(0, eqIndex).ToString();

                var subStr = newStr.Substring(eqIndex, newStr.Length - eqIndex);
                var strAttrs = subStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in strAttrs)
                {
                    var strs = item.Split('=');
                    var value = strs.Count == 2 ? strs[1].ToString() : string.Empty;
                    attrs.Add(strs[0].ToString(), value);
                }
            }
            else if (newStr.Contains("="))  //pc=quality,5     color=#6be48b
            {
                var strs = newStr.Split('=');
                tagName = strs[0].ToString();
                tagValue = strs[1].ToString();
            }
            else if (!newStr.Contains(" ") && !newStr.Contains("="))    //<b> <i> <u> <xxx>
            {
                tagName = newStr.ToString();
            }
            else
            {
                return newStr;  //return custom parse
            }
            return null;
        }

        public void Reset()
        {
            tagName = string.Empty;
            tagValue = string.Empty;
            startIndex = -1;
            endIndex = -1;
            attrs.Clear();
            rawData = string.Empty;
        }

        public void CopyTo(HtmlTag tag)
        {
            tag.tagName = tagName;
            tag.tagValue = tagValue;
            foreach (var item in attrs)
            {
                tag.attrs.Add(item.Key, item.Value);
            }
        }

        public override string ToString()
        {
            UString result = UString.StrConcat("\n(tagName：", tagName).Value;
            if (tagValue != null && tagValue.Length > 0)
            {
                result += UString.StrConcat(" tagValue：", tagValue).Value;
            }
            result += UString.Format(" startIndex：{0} endIndex：{1}", startIndex, endIndex);
            if (attrs.Count > 0)
            {
                foreach (var item in attrs)
                {
                    var attrStr = UString.StrConcat(" AttrName：", item.Key, " AttrValue：", item.Value);
                    result += attrStr.Value;
                }
            }
            result += ")  ";
            return result.ToString();
        }
    }
}