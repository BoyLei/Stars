
///--------------------------------------------------------------------
/// 文件名   :   ShapeArrow
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
    /// 箭头
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ShapeArrow:BaseShap 
    {
        /// <summary>
        /// 长度
        /// <summary>
        [LabelText("长度")]
        public int Length;

        /// <summary>
        /// 宽度
        /// <summary>
        [LabelText("宽度")]
        public int Width;

    }

}