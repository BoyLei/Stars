
///--------------------------------------------------------------------
/// 文件名   :   EffectTypePlayEffectAtTarget
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
    /// 在目标播放特效
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypePlayEffectAtTarget:BaseEffectType 
    {
        /// <summary>
        /// 目标Key
        /// <summary>
        [LabelText("目标Key")]
        [HideReferenceObjectPicker]
        public InputKey CheckKey= new InputKey();

        /// <summary>
        /// 特效
        /// <summary>
        [LabelText("特效")]
        [HideReferenceObjectPicker]
        public EffectTypeHitEffect[] HitEffect;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}