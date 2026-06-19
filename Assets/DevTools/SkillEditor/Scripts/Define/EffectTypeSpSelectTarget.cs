
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeSpSelectTarget
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
    /// 选择特殊目标
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeSpSelectTarget:BaseEffectType 
    {
        /// <summary>
        /// 目标组
        /// <summary>
        [LabelText("目标组")]
        [HideReferenceObjectPicker]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 选择方式
        /// <summary>
        [LabelText("选择方式")]
        [ValueDropdown("_targettype")]
        public TargetType TargetType= new TargetType();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _targettype()
        {
            return EnumDefineMap._targettype;
        }

    }

}