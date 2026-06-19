
///--------------------------------------------------------------------
/// 文件名   :   RandomAngle
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
    /// 随机角度
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class RandomAngle 
    {
        /// <summary>
        /// 最小角度
        /// <summary>
        [LabelText("最小角度")]
        [HideReferenceObjectPicker]
        public int MinAngle;

        /// <summary>
        /// 最大角度
        /// <summary>
        [LabelText("最大角度")]
        [HideReferenceObjectPicker]
        public int MaxAngle;

    }

}