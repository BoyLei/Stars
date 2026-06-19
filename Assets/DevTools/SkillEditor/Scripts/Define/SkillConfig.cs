
///--------------------------------------------------------------------
/// 文件名   :   SkillConfig
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
    /// 技能配置
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class SkillConfig:BaseConfig 
    {
        /// <summary>
        /// 技能ID
        /// <summary>
        [LabelText("技能ID")]
        [ReadOnly]
        public int ID;

        /// <summary>
        /// 主动打断优先级
        /// <summary>
        [LabelText("主动打断优先级")]
        public int Priority;

        /// <summary>
        /// 运行时优先级
        /// <summary>
        [LabelText("运行时优先级")]
        public int RuntimePriority;

        /// <summary>
        /// 技能备注
        /// <summary>
        [LabelText("技能备注")]
        public string SkillDesc;

        /// <summary>
        /// 技能类型
        /// <summary>
        [LabelText("技能类型")]
        [ValueDropdown("_skilltag")]
        public SkillTag SkillTag=SkillTag.Skill;

        /// <summary>
        /// 技能最大转身时间
        /// <summary>
        [LabelText("技能最大转身时间")]
        public int SkillTurnAroundTime;

        /// <summary>
        /// 表现标签
        /// <summary>
        [LabelText("表现标签")]
        public List<GlobalShowSerialize> GlobalShows= new List<GlobalShowSerialize>();

        /// <summary>
        /// 技能CD
        /// <summary>
        [LabelText("技能CD")]
        public int CD;

        /// <summary>
        /// 技能CDKey
        /// <summary>
        [LabelText("技能CDKey")]
        public int SkillCDKey;

        /// <summary>
        /// 是否不显示技能CD
        /// <summary>
        [LabelText("是否不显示技能CD")]
        public bool IsNotShowCD;

        /// <summary>
        /// 技能按钮CD
        /// <summary>
        [LabelText("技能按钮CD")]
        public int ButtonCD;

        /// <summary>
        /// 技能消耗
        /// <summary>
        [LabelText("技能消耗")]
        public List<ConsumeConfig> Consume= new List<ConsumeConfig>();

        /// <summary>
        /// 不显示在技能框中
        /// <summary>
        [LabelText("不显示在技能框中")]
        public bool IsHidden;

        /// <summary>
        /// 技能施法条件
        /// <summary>
        [LabelText("技能施法条件")]
        public List<SkillCondition> Conditions= new List<SkillCondition>();

        /// <summary>
        /// 最小释放距离(怪物)
        /// <summary>
        [LabelText("最小释放距离(怪物)")]
        public int MinCastDistance;

        /// <summary>
        /// 最大释放距离(怪物)
        /// <summary>
        [LabelText("最大释放距离(怪物)")]
        public int AICastDistance;

        /// <summary>
        /// 施法方式
        /// <summary>
        [LabelText("施法方式")]
        [ValueDropdown("_castmethod")]
        public CastMethodType CastMethod=CastMethodType.DirectCast;

        /// <summary>
        /// 进入CD是否抬起按钮
        /// <summary>
        [LabelText("进入CD是否抬起按钮")]
        public bool IsCDCloseTouch=true;

        /// <summary>
        /// 技能输入类型
        /// <summary>
        [LabelText("技能输入类型")]
        [ValueDropdown("_skillinputtype")]
        public SkillInputType SkillInputType=SkillInputType.DirInput;

        /// <summary>
        /// 指示器修改方式
        /// <summary>
        [LabelText("指示器修改方式")]
        [ValueDropdown("_dynamicrangetype")]
        public DynamicRangeType DynamicRangeType=DynamicRangeType.NoChange;

        /// <summary>
        /// 轮盘配置
        /// <summary>
        [LabelText("轮盘配置")]
        public WheelConfig WheelCfg= new WheelConfig();

        /// <summary>
        /// 轮盘可选范围
        /// <summary>
        [LabelText("轮盘可选范围")]
        public ShapeRingFan WheelRange= new ShapeRingFan();

        /// <summary>
        /// 释放自动转向
        /// <summary>
        [LabelText("释放自动转向")]
        public bool IsAutoTurnToTarget;

        /// <summary>
        /// 技能标签
        /// <summary>
        [LabelText("技能标签")]
        [ValueDropdown("_skilllabels")]
        public List<SkillLabel> SkillLabels= new List<SkillLabel>();

        /// <summary>
        /// 技能图标文字
        /// <summary>
        [LabelText("技能图标文字")]
        [HideInInspector]
        public string skillIconDesc;

        /// <summary>
        /// 技能图标文字
        /// <summary>
        [LabelText("技能图标文字")]
        [HideInInspector]
        public string SkillIconDesc_Key;

        public IEnumerable _skilltag()
        {
            return EnumDefineMap._skilltag;
        }

        public IEnumerable _castmethod()
        {
            return EnumDefineMap._castmethodtype;
        }

        public IEnumerable _skillinputtype()
        {
            return EnumDefineMap._skillinputtype;
        }

        public IEnumerable _dynamicrangetype()
        {
            return EnumDefineMap._dynamicrangetype;
        }

        public IEnumerable _skilllabels()
        {
            return EnumDefineMap._skilllabel;
        }

        /// <summary>
        /// 技能图标文字
        /// <summary>
        [LabelText("技能图标文字")]
        [ShowInInspector]
        public string SkillIconDesc 
        {
           get{return OnSkillIconDescGet();}set{OnSkillIconDescSet(value);}
        }

    }

}