///--------------------------------------------------------------------
/// 文件名   :   BulletStageAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 16:13:06
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
    [DisplayName("子弹移动阶段")]
    [HideMonoScript]
    public class BulletStageAsset : PlayableAsset  , ITimelineClipAsset  
    {
        [LabelText("子弹移动阶段")]
        [ShowInInspector]
        public BulletStageBehaviour template = new BulletStageBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.data.StageType = (int)StageType.BulletStage;
            var playable = ScriptPlayable<BulletStageBehaviour>.Create(graph, template);
            BulletStageBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;
            return playable;
        }
    }
}