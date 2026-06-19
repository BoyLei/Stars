
///--------------------------------------------------------------------
/// 文件名   :   ConditionSkill
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
    /// 技能条件技能时间
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ConditionSkill:BaseSkillCondition 
    {
        /// <summary>
        /// 技能ID
        /// <summary>
        [LabelText("技能ID")]
        public int ID;

        /// <summary>
        /// 最小时间
        /// <summary>
        [LabelText("最小时间")]
        public int Min;

        /// <summary>
        /// 最大时间
        /// <summary>
        [LabelText("最大时间")]
        public int Max;

    }

}