
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeTriggerStage
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
    /// 触发自定义阶段
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeTriggerStage:BaseEffectType 
    {
        /// <summary>
        /// 输入Key
        /// <summary>
        [LabelText("输入Key")]
        [HideReferenceObjectPicker]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 触发目标
        /// <summary>
        [LabelText("触发目标")]
        [HideReferenceObjectPicker]
        [ValueDropdown("_targettype")]
        public TriggerTargetType TargetType=TriggerTargetType.Onwer;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _targettype()
        {
            return EnumDefineMap._triggertargettype;
        }

    }

}