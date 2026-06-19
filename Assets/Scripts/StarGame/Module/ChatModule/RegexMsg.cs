using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.Chat
{
    [XLua.LuaCallCSharp]
    public static class RegexMsg
    {
        public enum MSG_TYPE
        {
            MSG_TYPE_TEXT,      //文本
            MSG_TYPE_EMOJI,     //小表情
            MSG_TYPE_LOCATION,  //定位
            MSG_TYPE_ITEM,      //装备
            MSG_TYPE_BIGEMOJI   //大表情
        };

        private static readonly Regex _inputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#(.+?)\]", RegexOptions.Singleline);
        private static readonly Regex _inputTagRegex2 = new Regex(@"\[(\d+)#(\(\-?\d+(\.\d+)?,\s?\-?\d+(\.\d+)?,\s?\-?\d+(\.\d+)?\))\]", RegexOptions.Singleline);
        //private static readonly Regex _inputPosRegex = new Regex(@"\[(\-{0,1}\d{0,})#\\(\\d+,\\d+,\\d+\\)\]", RegexOptions.Singleline);

        private static StringBuilder _textBuilder = new StringBuilder();

        public static int TranslateBigEmoji(string msg)
        {
            foreach (Match match in _inputTagRegex.Matches(msg))
            {
                MSG_TYPE msgType = (MSG_TYPE)int.Parse(match.Groups[1].Value);
                if (msgType != MSG_TYPE.MSG_TYPE_BIGEMOJI)
                {
                    return -1;
                }

                try
                {
                    string b = match.Groups[2].Value;
                    return int.Parse(b);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    return -1;
                }

            }
            return -1;
        }

        public static string TranslateMsg(string msg, GameObject go)
        {
            _textBuilder.Remove(0, _textBuilder.Length);
            int textIndex = 0;
            string part = "";
            //int linkIndex = 0;
            LinkOpener lo = go.GetComponent<LinkOpener>();
            foreach (Match match in _inputTagRegex.Matches(msg))
            {
                part = msg.Substring(textIndex, match.Index - textIndex);
                _textBuilder.Append(part);

                try
                {
                    MSG_TYPE msgType = (MSG_TYPE)int.Parse(match.Groups[1].Value);
                    string b = match.Groups[2].Value;

                    switch (msgType)
                    {
                        case MSG_TYPE.MSG_TYPE_EMOJI:
                            int emojiID = int.Parse(b);
                            _textBuilder.AppendFormat("<sprite={0}>", emojiID);
                            break;
                        case MSG_TYPE.MSG_TYPE_LOCATION:
                            var v = Parse(b);
                            _textBuilder.Append("<link><color=#ffff00>cxgd</color></link>");
                            // lo.AddPosLink(linkIndex++, v); 
                            break;
                        case MSG_TYPE.MSG_TYPE_ITEM:
                            int itemID = int.Parse(b);
                            _textBuilder.AppendFormat("<link><color=#0000ff>{0}</color></link>", itemID);
                            // lo.AddItemLink(linkIndex++, itemID);
                            break;
                        default:
                            throw new Exception("no type matches!!!");
                    }

                    textIndex = match.Index + match.Length;
                }
                catch(Exception e)
                {
                    Debug.Log(e.Message);
                }
            }

            part = msg.Substring(textIndex, msg.Length - textIndex);
            _textBuilder.Append(part);

            return _textBuilder.ToString();
        }

        public static Vector3 Parse(string str)
        {
            str = str.Replace("(", "").Replace(")", "").Replace(" ","");
            string[] s = str.Split(',');
            return new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
        }
    }
}