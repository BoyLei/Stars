
///--------------------------------------------------------------------
/// 文件名   :   TalentAndSlot
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
    /// 槽位技能
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TalentAndSlot 
    {
        /// <summary>
        /// 槽位
        /// <summary>
        [LabelText("槽位")]
        public int SlotID;

        /// <summary>
        /// 职业技能
        /// <summary>
        [LabelText("职业技能")]
        public int TalentID;

    }

}