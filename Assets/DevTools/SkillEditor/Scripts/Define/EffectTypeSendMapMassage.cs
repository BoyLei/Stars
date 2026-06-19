
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeSendMapMassage
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
    /// 触发场景消息
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeSendMapMassage:BaseEffectType 
    {
        /// <summary>
        /// 场景事件名
        /// <summary>
        [LabelText("场景事件名")]
        [HideReferenceObjectPicker]
        public string EventKey;

    }

}