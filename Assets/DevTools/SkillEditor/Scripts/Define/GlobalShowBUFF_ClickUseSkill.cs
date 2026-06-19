
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_ClickUseSkill
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
    /// 点击屏幕释放技能
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowBUFF_ClickUseSkill:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 指定技能ID
        /// <summary>
        [LabelText("指定技能ID")]
        public int UseSkill;

    }

}