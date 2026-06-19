
///--------------------------------------------------------------------
/// 文件名   :   ConditionCurSpectralElem
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
    /// 技能条件末位量谱
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ConditionCurSpectralElem:BaseSkillCondition 
    {
        /// <summary>
        /// 量谱值
        /// <summary>
        [LabelText("量谱值")]
        public Spectral Spectral= new Spectral();

        /// <summary>
        /// 检查值
        /// <summary>
        [LabelText("检查值")]
        public int CheckValue;

    }

}