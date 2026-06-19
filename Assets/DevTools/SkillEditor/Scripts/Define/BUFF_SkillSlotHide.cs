
///--------------------------------------------------------------------
/// 文件名   :   BUFF_SkillSlotHide
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
    /// 技能槽隐藏
    /// </summary>
    [System.Serializable]
    public  class BUFF_SkillSlotHide:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 隐藏槽位ID
        /// <summary>
        [LabelText("隐藏槽位ID")]
        public List<int> HideIndex= new List<int>();

    }

}