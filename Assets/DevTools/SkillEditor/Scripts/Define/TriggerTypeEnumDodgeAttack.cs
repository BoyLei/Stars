
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumDodgeAttack
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
    /// 临时闪避结果
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumDodgeAttack:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 比较符
        /// <summary>
        [LabelText("比较符")]
        [ValueDropdown("_compoperator")]
        public CompOperator CompOperator= new CompOperator();

        /// <summary>
        /// 是否闪避
        /// <summary>
        [LabelText("是否闪避")]
        public bool Value;

        public IEnumerable _compoperator()
        {
            return EnumDefineMap._compoperator;
        }

    }

}