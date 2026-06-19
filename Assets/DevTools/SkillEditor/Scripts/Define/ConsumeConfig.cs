
///--------------------------------------------------------------------
/// 文件名   :   ConsumeConfig
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
    /// 技能消耗
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ConsumeConfig 
    {
        /// <summary>
        /// 消耗类型
        /// <summary>
        [LabelText("消耗类型")]
        [ValueDropdown("_costtype")]
        public ConsumeType CostType= new ConsumeType();

        /// <summary>
        /// 消耗
        /// <summary>
        [LabelText("消耗")]
        public int Cost;

        public IEnumerable _costtype()
        {
            return EnumDefineMap._consumetype;
        }

    }

}