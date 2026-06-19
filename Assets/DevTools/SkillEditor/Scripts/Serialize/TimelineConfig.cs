///--------------------------------------------------------------------
/// 文件名   :   TimelineConfig.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/22 17:44:49
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor
{

    [System.Serializable]
    public class TimelineConfig
    {
        [HideLabel]
        public NormalGroupConfig NormalGroup = new NormalGroupConfig();

        [HideLabel]
        public BulletGroupConfig BulletGroup = new BulletGroupConfig();

        [HideLabel]
        public OtherGroupConfig OtherGroup = new OtherGroupConfig();

    }

    [System.Serializable]
    public class TimelineConfigList
    {
        [LabelText("Skill配置")]
        public TimelineConfig SkillConfig;

        [LabelText("Buff配置")]
        public TimelineConfig BuffConfig;

        [LabelText("子弹配置")]
        public TimelineConfig BulletConfig;

        [LabelText("被动配置")]
        public TimelineConfig PassiveConfig;
    }


    [System.Serializable]
    public class TimelineGroupConfig
    {
        public IEnumerable _TimelineTracks = new ValueDropdownList<string>()
        {
            {"动画轴","SkillEditor.StarAnimatorTrack"},
            {"特效轴","SkillEditor.StarControlTrack"},
            {"相机轴","SkillEditor.StarCinemachineTrack"},
            {"声音轴","SkillEditor.StarWWiseTrack"},
            {"效果轴","SkillEditor.EffectTrack"},
            {"用户输入轴","SkillEditor.InputEffectTrack"},
            {"阶段轴","SkillEditor.StageTrack"}
        };

        public Dictionary<string, string> TrackDisplayName = new Dictionary<string, string>()
        {
            {"SkillEditor.StarAnimatorTrack","动画轴"},
            {"SkillEditor.StarControlTrack","特效轴"},
            {"SkillEditor.StarCinemachineTrack","相机轴"},
            {"SkillEditor.StarWWiseTrack","声音轴"},
            {"SkillEditor.EffectTrack","效果轴"},
            {"SkillEditor.InputEffectTrack","用户输入轴"},
            {"SkillEditor.StageTrack","阶段轴"}
        };

        public virtual List<string> Tracks { get; }

        public virtual string DisplayName { get; }
    }

    [System.Serializable]
    public class NormalGroupConfig : TimelineGroupConfig
    {
        public override string DisplayName => "NormalGroup";

        [LabelText("普通组")]
        [ValueDropdown("_TimelineTracks")]
        [ShowInInspector]
        private List<string> tracks = new List<string> {

             "SkillEditor.StarAnimatorTrack",
            "SkillEditor.StarControlTrack",
            "SkillEditor.StarCinemachineTrack",
             "SkillEditor.StarWWiseTrack",
             "SkillEditor.EffectTrack",
             "SkillEditor.InputEffectTrack",
             "SkillEditor.StageTrack"
        };

        public override List<string> Tracks
        {
            get
            {
                return tracks;
            }
        }
    }

    [System.Serializable]
    public class BulletGroupConfig : TimelineGroupConfig
    {
        public override string DisplayName => "BulletGroup";

        [LabelText("子弹组")]
        [ValueDropdown("_TimelineTracks")]
        [ShowInInspector]
        private List<string> tracks = new List<string> {

            "SkillEditor.StarControlTrack",
             "SkillEditor.EffectTrack",
             "SkillEditor.StageTrack"
        };

        public override List<string> Tracks
        {
            get
            {
                return tracks;
            }
        }
    }

    [System.Serializable]
    public class OtherGroupConfig : TimelineGroupConfig
    {
        public override string DisplayName => "OtherGroup";

        [LabelText("其他组")]
        [ValueDropdown("_TimelineTracks")]
        [ShowInInspector]
        private List<string> tracks = new List<string> {

            "SkillEditor.StarControlTrack",
             "SkillEditor.EffectTrack",
             "SkillEditor.StageTrack"
        };

        public override List<string> Tracks
        {
            get
            {
                return tracks;
            }
        }
    }
}
