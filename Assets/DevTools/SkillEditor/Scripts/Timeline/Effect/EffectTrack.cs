///--------------------------------------------------------------------
/// 文件名   :   EffectTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 18:35:47
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
#if UNITY_EDITOR
    [DisplayName("效果轨道")]
    [TrackColor(0.8f, 0.8f, 0.2f)]
    [TrackClipType(typeof(ClipEffectAsset))]
    public class EffectTrack : TrackAsset, INotification
    {
        /// <summary>
        /// 轨道名称
        /// </summary>
        public string trackName;
        public bool isRight;

        public PropertyName id { get; }

        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.duration = 0.5f;
            clip.displayName = "效果";
            //ClipEffectAsset effectAsset = clip.asset as ClipEffectAsset;
            //List<int> ints= new List<int>();
            //foreach (var track in UnityEditor.Timeline.TimelineEditor.timelineAsset.GetOutputTracks())
            //{
            //    foreach (var curclip in track.GetClips())
            //    {
            //        if (curclip.displayName == "效果")
            //        {
            //            ClipEffectAsset clipEffectAsset = (ClipEffectAsset)curclip.asset;
            //            ints.Add(clipEffectAsset.Index);
            //        }
            //        if (curclip.displayName == "用户输入")
            //        {
            //            ClipInputEffectAsset clipInputEffectAsset = (ClipInputEffectAsset)curclip.asset;
            //            ints.Add(clipInputEffectAsset.Index);
            //        }
            //    }
            //}
            //effectAsset.Index = ints.Max() + 1;
        }

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            return base.CreatePlayable(graph, gameObject, clip);
        }
    }
#endif
}