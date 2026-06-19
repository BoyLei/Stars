///--------------------------------------------------------------------
/// 文件名   :   WWiseEventMarker.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 10:57:51
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;
using System.Linq;
using Sirenix.Utilities;

[DisplayName("自定义事件")]
[CustomStyle("LockMarker")]
[System.Serializable]
public class CustomEventMarker : Marker, INotification, INotificationOptionProvider
{
    [SerializeField] public bool emitOnce;
    [SerializeField] public bool emitInEditor;

    public PropertyName id { get; }

    [Header("触发者")]
    public string Trigger;

    [Header("事件")]
    public List<TimelineEventData> Events;

    NotificationFlags INotificationOptionProvider.flags =>
        (emitOnce ? NotificationFlags.TriggerOnce : default) |
        (emitInEditor ? NotificationFlags.TriggerInEditMode : default);
}