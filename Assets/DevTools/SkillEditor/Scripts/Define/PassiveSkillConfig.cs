
///--------------------------------------------------------------------
/// 文件名   :   PassiveSkillConfig
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By BaseDataConfig.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using MessagePack;
using Newtonsoft.Json.Converters;
namespace SkillEditor
{
    /// <summary>
    /// 被动配置
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class PassiveSkillConfig:BaseConfig 
    {
        /// <summary>
        /// 被动技能ID
        /// <summary>
        [LabelText("被动技能ID")]
        [ReadOnly]
        public int ID;

        /// <summary>
        /// 被动备注
        /// <summary>
        [LabelText("被动备注")]
        public string PassiveSkillDesc;

        /// <summary>
        /// 脱战是否刷新被动内触发器次数
        /// <summary>
        [LabelText("脱战是否刷新被动内触发器次数")]
        public bool OutBattleFreshTimes;

        /// <summary>
        /// 运行时优先级
        /// <summary>
        [LabelText("运行时优先级")]
        public int RuntimePriority;

        /// <summary>
        /// 被动时间
        /// <summary>
        [LabelText("被动时间")]
        public int Time;

        /// <summary>
        /// 循环特效
        /// <summary>
        [LabelText("循环特效")]
        public List<EffectTypeHitEffect> LoopEffectInEditors= new List<EffectTypeHitEffect>();

        /// <summary>
        /// 表现标签
        /// <summary>
        [LabelText("表现标签")]
        public List<GlobalShowSerialize> GlobalShows= new List<GlobalShowSerialize>();

        /// <summary>
        /// 属性修改
        /// <summary>
        [LabelText("属性修改")]
        public List<BattleProp> BattleProp= new List<BattleProp>();

        /// <summary>
        /// 属性组ID
        /// <summary>
        [LabelText("属性组ID")]
        [PropertyTooltip("如果有这个值就覆盖上面的属性修改数组")]
        public int BattleGroupID;

        /// <summary>
        /// 原子状态
        /// <summary>
        [LabelText("原子状态")]
        [ValueDropdown("_battlestate")]
        public List<BattleState> States= new List<BattleState>();

        /// <summary>
        /// Passive拷贝数据
        /// <summary>
        [LabelText("Passive拷贝数据")]
        public List<OutputKey> PassiveCopyData= new List<OutputKey>();

    }

}