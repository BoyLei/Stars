
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumCurrEffectID
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
    /// 当前效果id
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumCurrEffectID:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 要检查的效果ID
        /// <summary>
        [LabelText("要检查的效果ID")]
        public int EffectID;

    }

}