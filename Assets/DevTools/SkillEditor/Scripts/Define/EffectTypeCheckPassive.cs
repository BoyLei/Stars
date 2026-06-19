
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeCheckPassive
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
    /// 目标是否有指定被动检查器
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeCheckPassive:BaseEffectType 
    {
        /// <summary>
        /// 目标
        /// <summary>
        [LabelText("目标")]
        [HideReferenceObjectPicker]
        public InputKey Target= new InputKey();

        /// <summary>
        /// 被动配置ID
        /// <summary>
        [LabelText("被动配置ID")]
        [HideReferenceObjectPicker]
        public InputKey PassiveConfigID= new InputKey();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}