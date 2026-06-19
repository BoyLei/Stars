///--------------------------------------------------------------------
/// 文件名   :   AddBuffStageAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 16:11:00
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
    [DisplayName("Buff添加阶段")]
    [HideMonoScript]
    public class AddBuffStageAsset : PlayableAsset  , ITimelineClipAsset  
    {
        [LabelText("Buff添加阶段")]
        [ShowInInspector]
        public AddBuffStageBehaviour template = new AddBuffStageBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.data.StageType = (int)StageType.AddBuffStage;
            var playable = ScriptPlayable<AddBuffStageBehaviour>.Create(graph, template);
            AddBuffStageBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;
            return playable;
        }
    }
}