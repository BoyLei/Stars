///--------------------------------------------------------------------
/// 文件名   :   ClipInputEffectAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/09 15:02:00
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
    [DisplayName("用户输入")]
    [HideMonoScript]
    public class ClipInputEffectAsset : PlayableAsset, ITimelineClipAsset
    {
        //[LabelText("索引")]
        //[Sirenix.OdinInspector.ReadOnly]
        //public int Index;
        [HideLabel] public ClipInputEffectBehaviour template = new ClipInputEffectBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
#if UNITY_EDITOR
            // if (template.frameEffect.EffectID == 0)
            // {
            //     var skill = SkillEditorGlobal.Instance.GetCurrentSkill();
            //     if (skill != null)
            //     {
            //         template.frameEffect.EffectID = skill.GenneraEffectID();
            //     }
            // }
#endif
            var playable = ScriptPlayable<ClipInputEffectBehaviour>.Create(graph, template);
            ClipInputEffectBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;
            return playable;
        }
    }
}