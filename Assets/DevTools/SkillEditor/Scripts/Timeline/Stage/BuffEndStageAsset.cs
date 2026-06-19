///--------------------------------------------------------------------
/// 文件名   :   BuffEndStageAsset.cs
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
    [DisplayName("Buff结束阶段")]
    [HideMonoScript]
    public class BuffEndStageAsset : PlayableAsset  , ITimelineClipAsset  
    {
        [LabelText("Buff结束阶段")]
        [ShowInInspector]
        public BuffEndStageBehaviour template = new BuffEndStageBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.data.StageType = (int)StageType.EndBuffStage;
            var playable = ScriptPlayable<BuffEndStageBehaviour>.Create(graph, template);
            BuffEndStageBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;
            return playable;
        }
    }
}