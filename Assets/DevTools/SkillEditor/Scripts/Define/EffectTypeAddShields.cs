
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeAddShields
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
    /// 增加护盾值
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeAddShields:BaseEffectType 
    {
        /// <summary>
        /// 输入Key列表
        /// <summary>
        [LabelText("输入Key列表")]
        [HideReferenceObjectPicker]
        public InputKey[] InputKeyList;

        /// <summary>
        /// 固定值
        /// <summary>
        [LabelText("固定值")]
        public int Value;

        /// <summary>
        /// 数值组ID
        /// <summary>
        [LabelText("数值组ID")]
        public int GroupID;

        /// <summary>
        /// 数值组Key
        /// <summary>
        [LabelText("数值组Key")]
        [HideReferenceObjectPicker]
        public InputKey[] GroupIDKey;

    }

}