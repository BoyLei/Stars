///--------------------------------------------------------------------
/// 文件名   :   DataTime
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 14:18:35
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class DataTime 
{
    [BoxGroup("日期")]
    [LabelText("年")]
    public int Year;

    [BoxGroup("日期")]
    [LabelText("月")]
    [Range(0, 12)]
    public int Month;

    [BoxGroup("日期")]
    [LabelText("日")]
    [Range(0, 31)]
    public int Day;

    [BoxGroup("周")]
    [LabelText("星期")]
    [Range(0,7)]
    public int Week;

    [BoxGroup("时间")]
    [LabelText("时")]
    [Range(0,23)]
    public int Hour;

    [BoxGroup("时间")]
    [LabelText("分")]
    [Range(0, 59)]
    public int Minute;

    [BoxGroup("时间")]
    [LabelText("秒")]
    [Range(0, 59)]
    public int Second;

      
    public const string pattern = @"(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})";
    
    public DataTime(string datatime)
    {
        if(!string.IsNullOrEmpty(datatime))
        {
            string[] datas = datatime.Split('/');
            if (datas != null && datas.Length == 2)
            {
                string dateString = datas[0];
                // 匹配字符串
                Match match = Regex.Match(dateString, pattern);
                if (match.Groups != null && match.Groups.Count == 7)
                {
                    // 提取年、月、日、小时、分钟和秒数
                    Year = int.Parse(match.Groups[1].Value);
                    Month = int.Parse(match.Groups[2].Value);
                    Day = int.Parse(match.Groups[3].Value);
                    Hour = int.Parse(match.Groups[4].Value);
                    Minute = int.Parse(match.Groups[5].Value);
                    Second = int.Parse(match.Groups[6].Value);
                }
                Week = int.Parse(datas[1]);
            }

        }
    }
    
      
    public string DataString()
    {
       //return string.Format("{d4}-{Month}-{Day} {Hour}:{Minute}:{Second}/{Week}");
       return $"{Year:0000}-{Month:00}-{Day:00} {Hour:00}:{Minute:00}:{Second:00}/{Week:00}";
    }
}
#endif