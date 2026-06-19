
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeTarGroup
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
    /// 目标集合的操作
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeTarGroup:BaseEffectType 
    {
        /// <summary>
        /// 集合1
        /// <summary>
        [LabelText("集合1")]
        [HideReferenceObjectPicker]
        public InputKey Tar1= new InputKey();

        /// <summary>
        /// 是否取命中
        /// <summary>
        [LabelText("是否取命中")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_transtargetishit1")]
        public TransTargetIsHit TransTargetIsHit1=TransTargetIsHit.All;

        /// <summary>
        /// 集合2
        /// <summary>
        [LabelText("集合2")]
        [HideReferenceObjectPicker]
        public InputKey Tar2= new InputKey();

        /// <summary>
        /// 是否取命中
        /// <summary>
        [LabelText("是否取命中")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_transtargetishit2")]
        public TransTargetIsHit TransTargetIsHit2=TransTargetIsHit.All;

        /// <summary>
        /// 操作枚举
        /// <summary>
        [LabelText("操作枚举")]
        [ValueDropdown("_operation")]
        public TarGroupOperType Operation= new TarGroupOperType();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _transtargetishit1()
        {
            return EnumDefineMap._transtargetishit;
        }

        public IEnumerable _transtargetishit2()
        {
            return EnumDefineMap._transtargetishit;
        }

        public IEnumerable _operation()
        {
            return EnumDefineMap._targroupopertype;
        }

    }

}