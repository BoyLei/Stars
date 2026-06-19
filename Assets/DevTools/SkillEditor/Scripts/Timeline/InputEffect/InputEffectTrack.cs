///--------------------------------------------------------------------
/// 文件名   :   InputEffectTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/09 15:00:57
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEngine;
using System.ComponentModel;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

namespace SkillEditor
{
    [DisplayName("输入效果轨道")]
    [TrackColor(0.95f, 0.25f, 0.25f)]
    [TrackClipType(typeof(ClipInputEffectAsset))]
    public class InputEffectTrack : TrackAsset, INotification
    {
        /// <summary>
        /// 轨道名称
        /// </summary>
        public string trackName;
        public bool isRight;

        public PropertyName id { get; }


        protected override void OnCreateClip(TimelineClip clip)
        {
#if UNITY_EDITOR
            clip.duration = 1;
            clip.displayName = "用户输入";
            //ClipInputEffectAsset effectAsset = clip.asset as ClipInputEffectAsset;
            //if (effectAsset != null)
            //{
            //    effectAsset.template.inputEffect.EffectArgs.EffectType = EffectType.UserInput;
            //    effectAsset.template.inputEffect.EffectArgs.InitEffect();
            //    effectAsset.template.inputEffect.SaveSkill = true;
            //    List<int> ints = new List<int>();
            //    foreach (var track in UnityEditor.Timeline.TimelineEditor.timelineAsset.GetOutputTracks())
            //    {
            //        foreach (var curclip in track.GetClips())
            //        {
            //            if (curclip.displayName == "效果")
            //            {
            //                ClipEffectAsset clipEffectAsset = (ClipEffectAsset)curclip.asset;
            //                ints.Add(clipEffectAsset.Index);
            //            }
            //            if (curclip.displayName == "用户输入")
            //            {
            //                ClipInputEffectAsset clipInputEffectAsset = (ClipInputEffectAsset)curclip.asset;
            //                ints.Add(clipInputEffectAsset.Index);
            //            }
            //        }
            //    }
            //    effectAsset.Index = ints.Max() + 1;
            //}
#endif
        }
    }
}