
///--------------------------------------------------------------------
/// 文件名   :   CenterPosArray
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
    /// 中心点
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class CenterPosArray 
    {
        /// <summary>
        /// 中心目标Key
        /// <summary>
        [LabelText("中心目标Key")]
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
        /// 坐标Key
        /// <summary>
        [LabelText("坐标Key")]
        [HideReferenceObjectPicker]
        public InputKey PosKey= new InputKey();

        /// <summary>
        /// 角度
        /// <summary>
        [LabelText("角度")]
        public int Angle=0;

        /// <summary>
        /// 距离
        /// <summary>
        [LabelText("距离")]
        public int Distance=0;

        public IEnumerable _transtargetishit()
        {
            return EnumDefineMap._transtargetishit;
        }

    }

}