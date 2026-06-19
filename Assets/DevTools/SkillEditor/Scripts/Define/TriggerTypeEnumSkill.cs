
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumSkill
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
    /// 当前技能标签
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumSkill:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 要检查的技能标签
        /// <summary>
        [LabelText("要检查的技能标签")]
        [ValueDropdown("_skilllabel")]
        public SkillLabel SkillLabel= new SkillLabel();

        public IEnumerable _skilllabel()
        {
            return EnumDefineMap._skilllabel;
        }

    }

}