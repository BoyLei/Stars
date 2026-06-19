
///--------------------------------------------------------------------
/// 文件名   :   TowardArray
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
    /// 朝向
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class TowardArray 
    {
        /// <summary>
        /// 朝向目标Key
        /// <summary>
        [LabelText("朝向目标Key")]
        [HideReferenceObjectPicker]
        public InputKey TargetKey= new InputKey();

        /// <summary>
        /// 是否取命中
        /// <summary>
        [LabelText("是否取命中")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_transtargetishit")]
        public TransTargetIsHit TransTargetIsHit=TransTargetIsHit.All;

        /// <summary>
        /// 朝向Key
        /// <summary>
        [LabelText("朝向Key")]
        [HideReferenceObjectPicker]
        public InputKey TowardKey= new InputKey();

        /// <summary>
        /// 朝向偏移
        /// <summary>
        [LabelText("朝向偏移")]
        public int TowardOffset;

        public IEnumerable _transtargetishit()
        {
            return EnumDefineMap._transtargetishit;
        }

    }

}