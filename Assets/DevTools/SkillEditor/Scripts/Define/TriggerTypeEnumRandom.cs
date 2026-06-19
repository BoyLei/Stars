
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumRandom
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
    /// 临时随机数值
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumRandom:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 随机值最大10000
        /// <summary>
        [LabelText("随机值最大10000")]
        public int Value;

    }

}