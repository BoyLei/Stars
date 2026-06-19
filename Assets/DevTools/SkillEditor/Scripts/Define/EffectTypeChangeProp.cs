
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChangeProp
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
    /// 修改属性
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChangeProp:BaseEffectType 
    {
        /// <summary>
        /// 效果目标
        /// <summary>
        [LabelText("效果目标")]
        [HideReferenceObjectPicker]
        public InputKey EffectTargetArray= new InputKey();

        /// <summary>
        /// 修改方式
        /// <summary>
        [LabelText("修改方式")]
        [ValueDropdown("_propchangetype")]
        public PropChangeType PropChangeType= new PropChangeType();

        /// <summary>
        /// 修改属性
        /// <summary>
        [LabelText("修改属性")]
        public BattleProp[] ChangeProp;

        public IEnumerable _propchangetype()
        {
            return EnumDefineMap._propchangetype;
        }

    }

}