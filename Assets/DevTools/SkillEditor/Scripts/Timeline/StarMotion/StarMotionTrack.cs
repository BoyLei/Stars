///--------------------------------------------------------------------
/// 文件名   :   StarMotionTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/20 16:28:41
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [DisplayName("运动轨道")]
    [TrackColor(0.15f, 0.25f, 0.25f)]
    [TrackClipType(typeof(ClipStarMotionAsset))]
    public class StarMotionTrack : TrackAsset, INotification
    {
        /// <summary>
        /// 轨道名称
        /// </summary>
        public string trackName;

        public PropertyName id { get; }


        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.duration = 1;
            clip.displayName = "移动";
        }
    }
}