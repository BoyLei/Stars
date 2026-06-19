///--------------------------------------------------------------------
/// 文件名   :   PassiveEndStageAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 16:11:52
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [DisplayName("被动结束阶段")]
    [HideMonoScript]
    public class PassiveEndStageAsset : PlayableAsset, ITimelineClipAsset
    {
        [LabelText("被动结束阶段")]
        [ShowInInspector]
        public PassiveEndStageBehaviour template = new PassiveEndStageBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.data.StageType = (int)StageType.EndPassiveStage;
            var playable = ScriptPlayable<PassiveEndStageBehaviour>.Create(graph, template);
            PassiveEndStageBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;
            return playable;
        }
    }
}