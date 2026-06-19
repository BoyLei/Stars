
///--------------------------------------------------------------------
/// 文件名   :   BUFF_JobSkillChange
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
using Newtonsoft.Json.Converters;
namespace SkillEditor
{
    /// <summary>
    /// 新增职业技能
    /// </summary>
    [System.Serializable]
    public  class BUFF_JobSkillChange:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 槽位切换技能
        /// <summary>
        [LabelText("槽位切换技能")]
        public List<TalentAndSlot> TalentAndSlot= new List<TalentAndSlot>();

    }

}