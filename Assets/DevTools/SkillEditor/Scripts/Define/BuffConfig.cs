
///--------------------------------------------------------------------
/// 文件名   :   BuffConfig
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
    /// Buff配置
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class BuffConfig:BaseConfig 
    {
        /// <summary>
        /// BUFFID
        /// <summary>
        [LabelText("BUFFID")]
        [ReadOnly]
        public int ID;

        /// <summary>
        /// Buff备注
        /// <summary>
        [LabelText("Buff备注")]
        public string BuffDesc;

        /// <summary>
        /// 运行时优先级
        /// <summary>
        [LabelText("运行时优先级")]
        public int RuntimePriority;

        /// <summary>
        /// 是否是减益
        /// <summary>
        [LabelText("是否是减益")]
        public bool IsDebuff=true;

        /// <summary>
        /// BUFF基础类型
        /// <summary>
        [LabelText("BUFF基础类型")]
        [ValueDropdown("_bufftags")]
        public BuffTag BuffTags= new BuffTag();

        /// <summary>
        /// 表现标签
        /// <summary>
        [LabelText("表现标签")]
        public List<GlobalShowSerialize> GlobalShows= new List<GlobalShowSerialize>();

        /// <summary>
        /// BUFF时间
        /// <summary>
        [LabelText("BUFF时间")]
        public int Time;

        /// <summary>
        /// 是否显示持续时间
        /// <summary>
        [LabelText("是否显示持续时间")]
        public bool IsTimeShow=true;

        /// <summary>
        /// 循环特效
        /// <summary>
        [LabelText("循环特效")]
        public List<EffectTypeHitEffect> LoopEffectInEditors= new List<EffectTypeHitEffect>();

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
        /// 离线是否计时生效
        /// <summary>
        [LabelText("离线是否计时生效")]
        public bool IsLeaveSave;

        /// <summary>
        /// 同buff替换方式
        /// <summary>
        [LabelText("同buff替换方式")]
        [ValueDropdown("_replace")]
        public BuffReplace Replace=BuffReplace.Refresh;

        /// <summary>
        /// BUFF组ID
        /// <summary>
        [LabelText("BUFF组ID")]
        public int GroupID;

        /// <summary>
        /// 是否分对象计算替换规则
        /// <summary>
        [LabelText("是否分对象计算替换规则")]
        public bool IsCretDiffer;

        /// <summary>
        /// 最大层数
        /// <summary>
        [LabelText("最大层数")]
        public int MaxFloor=1;

        /// <summary>
        /// 添加BUFF时飘字
        /// <summary>
        [LabelText("添加BUFF时飘字")]
        public List<AddEffectFly> AddEffectFlys= new List<AddEffectFly>();

        /// <summary>
        /// Buff拷贝数据
        /// <summary>
        [LabelText("Buff拷贝数据")]
        public List<OutputKey> BuffCopyData= new List<OutputKey>();

        /// <summary>
        /// Buff标签
        /// <summary>
        [LabelText("Buff标签")]
        [ValueDropdown("_bufflabels")]
        public List<BuffLabel> BuffLabels= new List<BuffLabel>();

        public IEnumerable _bufftags()
        {
            return EnumDefineMap._bufftag;
        }

        public IEnumerable _replace()
        {
            return EnumDefineMap._buffreplace;
        }

        public IEnumerable _bufflabels()
        {
            return EnumDefineMap._bufflabel;
        }

    }

}