
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChangeToInputSpectral
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
    /// 目标集合量谱值修改
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChangeToInputSpectral:BaseEffectType 
    {
        /// <summary>
        /// 被修改者
        /// <summary>
        [LabelText("被修改者")]
        [HideReferenceObjectPicker]
        public InputKey Target= new InputKey();

        /// <summary>
        /// 量谱值
        /// <summary>
        [LabelText("量谱值")]
        [HideReferenceObjectPicker]
        [ValueDropdown("_spectral")]
        public Spectral Spectral= new Spectral();

        /// <summary>
        /// 目标值
        /// <summary>
        [LabelText("目标值")]
        [HideReferenceObjectPicker]
        public InputKey TargetValue= new InputKey();

        public IEnumerable _spectral()
        {
            return EnumDefineMap._spectral;
        }

    }

}