
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChangeToCDKeyCD
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
    /// 目标集合冷却时间修改修改
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChangeToCDKeyCD:BaseEffectType 
    {
        /// <summary>
        /// 被修改者
        /// <summary>
        [LabelText("被修改者")]
        [HideReferenceObjectPicker]
        public InputKey Target= new InputKey();

        /// <summary>
        /// CDKey
        /// <summary>
        [LabelText("CDKey")]
        [HideReferenceObjectPicker]
        public InputKey CDKey= new InputKey();

        /// <summary>
        /// 目标值
        /// <summary>
        [LabelText("目标值")]
        [HideReferenceObjectPicker]
        public InputKey TargetValue= new InputKey();

    }

}