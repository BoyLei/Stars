
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeRamdonValueToKey
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
    /// 随机数值产生器
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeRamdonValueToKey:BaseEffectType 
    {
        /// <summary>
        /// 下限值
        /// <summary>
        [LabelText("下限值")]
        [HideReferenceObjectPicker]
        public InputKey MinValue= new InputKey();

        /// <summary>
        /// 上限值
        /// <summary>
        [LabelText("上限值")]
        [HideReferenceObjectPicker]
        public InputKey MaxValue= new InputKey();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}