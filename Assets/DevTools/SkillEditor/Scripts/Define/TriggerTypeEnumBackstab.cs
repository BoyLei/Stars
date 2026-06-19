
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumBackstab
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
    /// 是否触发背刺
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumBackstab:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 获取我和目标的相对朝向差
        /// <summary>
        [LabelText("获取我和目标的相对朝向差")]
        public int Value;

    }

}