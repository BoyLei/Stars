
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowSkill_SkillSlotSpShow
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
    /// 技能槽特殊表现
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowSkill_SkillSlotSpShow:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 技能槽特殊表现
        /// <summary>
        [LabelText("技能槽特殊表现")]
        [ValueDropdown("_spshow")]
        public List<EnumSkillSlotSpShow> SpShow= new List<EnumSkillSlotSpShow>();

        public IEnumerable _spshow()
        {
            return EnumDefineMap._enumskillslotspshow;
        }

    }

}