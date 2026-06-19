
///--------------------------------------------------------------------
/// 文件名   :   TriggerTypeEnumCoolTime
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
    /// 对象的技能冷却
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TriggerTypeEnumCoolTime:TriggerTypeEnumBaseData 
    {
        /// <summary>
        /// 检查对象
        /// <summary>
        [LabelText("检查对象")]
        public InputKey Tar= new InputKey();

        /// <summary>
        /// CDKey
        /// <summary>
        [LabelText("CDKey")]
        public InputKey Value= new InputKey();

    }

}