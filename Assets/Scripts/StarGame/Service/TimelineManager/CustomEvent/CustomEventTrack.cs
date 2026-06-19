///--------------------------------------------------------------------
/// 文件名   :   StarWWiseTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 11:00:45
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisplayName("自定义事件轨道")]
[TrackColor(0.1f, 1, 0.5f)]
[TrackBindingType(typeof(CustomEventNotificationReceiver))]
public class CustomEventTrack : TrackAsset, INotification
{
    /// <summary>
    /// 轨道名称
    /// </summary>
    public string trackName;

    public bool isRight;
    public PropertyName id { get; }
}
