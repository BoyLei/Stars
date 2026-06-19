
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeGetGroupIDToKey
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
    /// 组数值产生器
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeGetGroupIDToKey:BaseEffectType 
    {
        /// <summary>
        /// 数值组
        /// <summary>
        [LabelText("数值组")]
        [HideReferenceObjectPicker]
        public InputKey GroupID= new InputKey();

        /// <summary>
        /// 数值组Key
        /// <summary>
        [LabelText("数值组Key")]
        [HideReferenceObjectPicker]
        public InputKey[] GroupIDKey;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}