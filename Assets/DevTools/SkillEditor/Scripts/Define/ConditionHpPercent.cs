
///--------------------------------------------------------------------
/// 文件名   :   ConditionHpPercent
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
    /// 技能条件血量万分比
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ConditionHpPercent:BaseSkillCondition 
    {
        /// <summary>
        /// 最小万分比
        /// <summary>
        [LabelText("最小万分比")]
        public int Min;

        /// <summary>
        /// 最大万分比
        /// <summary>
        [LabelText("最大万分比")]
        public int Max;

    }

}