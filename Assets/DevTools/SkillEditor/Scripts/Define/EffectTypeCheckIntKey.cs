
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeCheckIntKey
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
    /// 检查目标Key的int值
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeCheckIntKey:BaseEffectType 
    {
        /// <summary>
        /// 目标Key
        /// <summary>
        [LabelText("目标Key")]
        [HideReferenceObjectPicker]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 值
        /// <summary>
        [LabelText("值")]
        public int Value;

        /// <summary>
        /// 比较方式
        /// <summary>
        [LabelText("比较方式")]
        [ValueDropdown("_type")]
        public CompOperator Type= new CompOperator();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _type()
        {
            return EnumDefineMap._compoperator;
        }

    }

}