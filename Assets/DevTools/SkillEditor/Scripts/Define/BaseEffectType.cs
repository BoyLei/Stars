
///--------------------------------------------------------------------
/// 文件名   :   BaseEffectType
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
    /// 效果抽象
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class BaseEffectType 
    {
        /// <summary>
        /// 获取输出Key
        /// <summary>
        [LabelText("获取输出Key")]
        public virtual OutputKey GetOutputKey 
        {
           get{return null;}
        }

    }

}