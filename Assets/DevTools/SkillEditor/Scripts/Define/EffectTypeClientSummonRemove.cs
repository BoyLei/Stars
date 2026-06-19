
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeClientSummonRemove
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
    /// 客户端召唤物移除
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeClientSummonRemove:BaseEffectType 
    {
        /// <summary>
        /// 召唤物索引
        /// <summary>
        [LabelText("召唤物索引")]
        [HideReferenceObjectPicker]
        public List<int> Index= new List<int>();

    }

}