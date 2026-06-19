///--------------------------------------------------------------------
/// 文件名   :   ClipStageAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 18:34:53
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using UnityEngine.Timeline;

namespace SkillEditor
{

    [DisplayName("一般阶段")]
    [HideMonoScript]
    public class ClipStageAsset : PlayableAsset, ITimelineClipAsset  
    {
        [LabelText("一般阶段")]
        [ShowInInspector]
        public ClipStageBehaviour template = new ClipStageBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.data.StageType = (int)StageType.NormalStage;
            var playable = ScriptPlayable<ClipStageBehaviour>.Create(graph, template);
            ClipStageBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;
            behaviour.Asset = this;
            return playable;
        }
    }
}
