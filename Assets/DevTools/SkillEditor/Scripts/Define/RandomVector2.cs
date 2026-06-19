
///--------------------------------------------------------------------
/// 文件名   :   RandomVector2
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
    /// 随机二维点位
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class RandomVector2 
    {
        /// <summary>
        /// X最小值
        /// <summary>
        [LabelText("X最小值")]
        [HideReferenceObjectPicker]
        public int MinX;

        /// <summary>
        /// X最大值
        /// <summary>
        [LabelText("X最大值")]
        [HideReferenceObjectPicker]
        public int MaxX;

        /// <summary>
        /// Y最小值
        /// <summary>
        [LabelText("Y最小值")]
        [HideReferenceObjectPicker]
        public int MinY;

        /// <summary>
        /// Y最大值
        /// <summary>
        [LabelText("Y最大值")]
        [HideReferenceObjectPicker]
        public int MaxY;

    }

}