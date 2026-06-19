///--------------------------------------------------------------------
/// 文件名   :   ClipStarMotionAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/20 16:29:31
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEngine;
using System.ComponentModel;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [DisplayName("运动")]
    [HideMonoScript]
    public class ClipStarMotionAsset : PlayableAsset, ITimelineClipAsset
    {
        [HideLabel] public ClipStarMotionBehaviour template = new ClipStarMotionBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ClipStarMotionBehaviour>.Create(graph, template);
            ClipStarMotionBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;
            return playable;
        }
    }
}
