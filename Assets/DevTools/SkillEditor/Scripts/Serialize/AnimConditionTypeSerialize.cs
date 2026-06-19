
///--------------------------------------------------------------------
/// 文件名   :   AnimConditionTypeSerialize
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
    /// 动作条件序列化
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class AnimConditionTypeSerialize 
    {
        /// <summary>
        /// 条件类型
        /// <summary>
        [LabelText("条件类型")]
        [HideReferenceObjectPicker]
        [ValueDropdown("_animconditiontype")]
        public AnimConditionType AnimConditionType= new AnimConditionType();

        /// <summary>
        /// 移动
        /// <summary>
        [LabelText("移动")]
        [HideReferenceObjectPicker]
        [SerializeField]
        [ShowIf("ShouldSerializeMove")]
        public AnimConditionTypeMove Move= new AnimConditionTypeMove();

        public IEnumerable _animconditiontype()
        {
            return EnumDefineMap._animconditiontype;
        }

        public bool ShouldSerializeMove()
        {
            return this.AnimConditionType == AnimConditionType.Move;
        }

    }

}