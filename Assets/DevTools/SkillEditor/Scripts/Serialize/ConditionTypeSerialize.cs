
///--------------------------------------------------------------------
/// 文件名   :   ConditionTypeSerialize
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
    /// 条件序列化
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ConditionTypeSerialize 
    {
        /// <summary>
        /// 条件类型
        /// <summary>
        [LabelText("条件类型")]
        [HideReferenceObjectPicker]
        [ValueDropdown("_conditiontype")]
        public ConditionType ConditionType= new ConditionType();

        /// <summary>
        /// BUFF层数参数
        /// <summary>
        [LabelText("BUFF层数参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeCond_Buff")]
        public ConditionBuff Cond_Buff= new ConditionBuff();

        /// <summary>
        /// 属性值参数
        /// <summary>
        [LabelText("属性值参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeCond_Attr")]
        public ConditionAttr Cond_Attr= new ConditionAttr();

        /// <summary>
        /// 前置技能时间参数
        /// <summary>
        [LabelText("前置技能时间参数")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeCond_SkillTime")]
        public ConditionSkill Cond_SkillTime= new ConditionSkill();

        /// <summary>
        /// 血量万分比
        /// <summary>
        [LabelText("血量万分比")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeCond_HpPercent")]
        public ConditionHpPercent Cond_HpPercent= new ConditionHpPercent();

        /// <summary>
        /// 末位量谱值
        /// <summary>
        [LabelText("末位量谱值")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeCondition_CurSpectralElem")]
        public ConditionCurSpectralElem Condition_CurSpectralElem= new ConditionCurSpectralElem();

        /// <summary>
        /// 敌人阵营
        /// <summary>
        [LabelText("敌人阵营")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeCond_TargetFaction")]
        public Cond_TargetFaction Cond_TargetFaction= new Cond_TargetFaction();

        public IEnumerable _conditiontype()
        {
            return EnumDefineMap._conditiontype;
        }

        public bool ShouldSerializeCond_Buff()
        {
            return this.ConditionType == ConditionType.Cond_Buff;
        }

        public bool ShouldSerializeCond_Attr()
        {
            return this.ConditionType == ConditionType.Cond_Attr;
        }

        public bool ShouldSerializeCond_SkillTime()
        {
            return this.ConditionType == ConditionType.Cond_SkillTime;
        }

        public bool ShouldSerializeCond_HpPercent()
        {
            return this.ConditionType == ConditionType.Cond_HpPercent;
        }

        public bool ShouldSerializeCondition_CurSpectralElem()
        {
            return this.ConditionType == ConditionType.Condition_CurSpectralElem;
        }

        public bool ShouldSerializeCond_TargetFaction()
        {
            return this.ConditionType == ConditionType.Cond_TargetFaction;
        }

    }

}