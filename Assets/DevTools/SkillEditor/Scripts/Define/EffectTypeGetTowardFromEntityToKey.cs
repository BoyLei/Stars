
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeGetTowardFromEntityToKey
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
    /// 单位朝向产生器
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeGetTowardFromEntityToKey:BaseEffectType 
    {
        /// <summary>
        /// 单位
        /// <summary>
        [LabelText("单位")]
        [HideReferenceObjectPicker]
        public InputKey MainTarget= new InputKey();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}