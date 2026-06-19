
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeSetBuffTime
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
    /// 重设BUFF剩余持续时间
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeSetBuffTime:BaseEffectType 
    {
        /// <summary>
        /// 输入Key
        /// <summary>
        [LabelText("输入Key")]
        [HideReferenceObjectPicker]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 设置方式
        /// <summary>
        [LabelText("设置方式")]
        [ValueDropdown("_type")]
        public OPEnum Type= new OPEnum();

        /// <summary>
        /// 值
        /// <summary>
        [LabelText("值")]
        public int Value;

        /// <summary>
        /// 输入Key列表
        /// <summary>
        [LabelText("输入Key列表")]
        [HideReferenceObjectPicker]
        public InputKey[] InputKeyList;

        /// <summary>
        /// 数值组ID
        /// <summary>
        [LabelText("数值组ID")]
        public int GroupID;

        /// <summary>
        /// 数值组Key
        /// <summary>
        [LabelText("数值组Key")]
        [HideReferenceObjectPicker]
        public InputKey[] GroupIDKey;

        public IEnumerable _type()
        {
            return EnumDefineMap._openum;
        }

    }

}