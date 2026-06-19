
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum TimelineEventDefine
{
    None = 0,
    Dialogue = 1,           //对话
    PlaySound = 2,          //播放声音
    RoleBindEffect=3,           //特效绑定
    Blur=4,                 //模糊
    ShakeScreen=5,          //屏幕震动
    BlackScreen=6,          //黑慕
    FlowScreen=7,           //报幕
    SetScreenEffect=8,      //屏幕特效
    ModifySpeed=9,         //修改速度
    QuickTimeEvent = 10,    //
    ChangeModelShader=11,   //改变模型shader
    CtrMainLight=12,        //控制主灯光
    PlayPlot=13,            //播放章节报幕
    PlayWWiseBGM=14,        //播放wwise背景音乐
}

[System.Serializable]
public class TimelineEventData
{
   // [ValueDropdown("GetDefines")]
    [Header("事件类型")]
    //[OnValueChanged("OnEventTypeChanged")]
    public TimelineEventDefine EventType;
    
    [Header("事件参数")]
    public List<string> EventArgs;

    /*public void OnEventTypeChanged()
    {
        Debug.LogError(EventType);
    }
    
    public IEnumerable GetDefines()
    {
        return TimelineEventUtils.timelineevents;
    }*/
}



/*public static class TimelineEventUtils
{
     public static IEnumerable timelineevents = new ValueDropdownList<TimelineEventDefine>()
    {
        { "无", TimelineEventDefine.None },
        { "对话", TimelineEventDefine.Dialogue },
        { "播放声音", TimelineEventDefine.PlaySound },
        { "特效绑定", TimelineEventDefine.BindEffect },
        { "模糊", TimelineEventDefine.Blur },
        { "屏幕震动", TimelineEventDefine.ShakeScreen },
        { "黑慕", TimelineEventDefine.BlackScreen },
        { "报幕", TimelineEventDefine.FlowScreen },
        { "屏幕特效", TimelineEventDefine.SetScreenEffect }
    };
}*/