
///--------------------------------------------------------------------
/// 文件名   :   ConditionBuff
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
    /// 技能条件Buff层数
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ConditionBuff:BaseSkillCondition 
    {
        /// <summary>
        /// BuffID
        /// <summary>
        [LabelText("BuffID")]
        public int ID;

        /// <summary>
        /// 最小值
        /// <summary>
        [LabelText("最小值")]
        public int Min;

        /// <summary>
        /// 最大值
        /// <summary>
        [LabelText("最大值")]
        public int Max;

    }

}