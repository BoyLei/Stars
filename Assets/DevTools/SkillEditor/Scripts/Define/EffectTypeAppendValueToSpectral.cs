
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeAppendValueToSpectral
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
    /// 特殊增加量谱值
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeAppendValueToSpectral:BaseEffectType 
    {
        /// <summary>
        /// 量谱值
        /// <summary>
        [LabelText("量谱值")]
        [ValueDropdown("_spectral")]
        public Spectral Spectral= new Spectral();

        /// <summary>
        /// 新增量谱值
        /// <summary>
        [LabelText("新增量谱值")]
        public int InsertVal;

        /// <summary>
        /// 复合量谱值
        /// <summary>
        [LabelText("复合量谱值")]
        public int CombineVal;

        public IEnumerable _spectral()
        {
            return EnumDefineMap._spectral;
        }

    }

}