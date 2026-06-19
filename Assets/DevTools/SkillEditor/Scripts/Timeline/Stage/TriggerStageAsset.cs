///--------------------------------------------------------------------
/// 文件名   :   TriggerStageAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 15:35:19
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [DisplayName("触发阶段")]
    [HideMonoScript]
    public class TriggerStageAsset : PlayableAsset, ITimelineClipAsset  
    {
        [LabelText("触发阶段")] [ShowInInspector] public TriggerStageBehaviour template = new TriggerStageBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.data.StageType = (int)StageType.TriggerStage;
            var playable = ScriptPlayable<TriggerStageBehaviour>.Create(graph, template);
            TriggerStageBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;
            return playable;
        }
    }
}