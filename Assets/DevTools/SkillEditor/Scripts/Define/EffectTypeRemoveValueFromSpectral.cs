
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeRemoveValueFromSpectral
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
    /// 特殊移除量谱值
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeRemoveValueFromSpectral:BaseEffectType 
    {
        /// <summary>
        /// 量谱值
        /// <summary>
        [LabelText("量谱值")]
        [ValueDropdown("_spectral")]
        public Spectral Spectral= new Spectral();

        public IEnumerable _spectral()
        {
            return EnumDefineMap._spectral;
        }

    }

}