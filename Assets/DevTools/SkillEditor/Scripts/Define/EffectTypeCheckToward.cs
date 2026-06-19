
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeCheckToward
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
    /// 1是否在2的角度范围内检查器
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeCheckToward:BaseEffectType 
    {
        /// <summary>
        /// 朝向1
        /// <summary>
        [LabelText("朝向1")]
        [HideReferenceObjectPicker]
        public InputKey Toward1= new InputKey();

        /// <summary>
        /// 朝向2
        /// <summary>
        [LabelText("朝向2")]
        [HideReferenceObjectPicker]
        public InputKey Toward2= new InputKey();

        /// <summary>
        /// 角度范围
        /// <summary>
        [LabelText("角度范围")]
        [HideReferenceObjectPicker]
        public InputKey CheckRange= new InputKey();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}