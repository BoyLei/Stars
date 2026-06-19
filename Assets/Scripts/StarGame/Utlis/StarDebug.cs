///--------------------------------------------------------------------
/// 文件名   :   StarDebug.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/14 14:48:16
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XLua;

public class StarDebug
{
    public const string Green = "00FF00";
    public const string Blue = "0000FF";
    public const string Orange = "FFA500";
    public const string Purple = "A020F0";
    public const string SteelBlue = "63B8FF";
    public const string White = "FFFFFF";

    public const string StarLog_Tag = "[star_error] ";

    public static Dictionary<LogTagEnum, string> TagDic = new Dictionary<LogTagEnum, string>() {

        {LogTagEnum.Battle,"FFA500" },
        {LogTagEnum.Skill,"A020F0" },
        {LogTagEnum.Buff,"63B8FF" },
        {LogTagEnum.UI,"00FF00" },
    };



    public enum LogTagEnum
    {
        Battle = 0,
        Skill = 1,
        Buff = 2,
        UI = 3,
    }

    public static StringBuilder stringBuilder = new StringBuilder();

    public static void Log(string Color = White, params object[] content)
    {
        stringBuilder.Clear();
        if (content != null && content.Length > 0)
        {
            foreach (var item in content)
            {
                stringBuilder.Append(item);
            }
        }
        SGF.Debuger.Log($"<color=#{Color}>{stringBuilder.ToString()}</color>");
    }

    public static void LogError(params object[] content)
    {
        stringBuilder.Clear();
        stringBuilder.Append(StarLog_Tag);
        if (content != null && content.Length > 0)
        {
            foreach (var item in content)
            {
                stringBuilder.Append(item);
            }
        }
        SGF.Debuger.LogError($"<color=#FF0000>{stringBuilder.ToString()}</color>");
    }

    public static void LogTag(LogTagEnum tag, params object[] content)
    {
        stringBuilder.Clear();
        stringBuilder.Append("[star_");
        stringBuilder.Append(tag.ToString());
        stringBuilder.Append("]");
        if (content != null && content.Length > 0)
        {
            foreach (var item in content)
            {
                stringBuilder.Append(item);
            }
        }
        SGF.Debuger.Log($"<color=#{TagDic[tag]}>{stringBuilder.ToString()}</color>");
    }

}
