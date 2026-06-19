
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumDamageType
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
    /// 伤害节点-检查伤害类型
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumDamageType:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 检查类型
        /// <summary>
        [LabelText("检查类型")]
        public List<DamageType> TypeList= new List<DamageType>();

    }

}