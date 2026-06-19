
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeCreateInteract
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
    /// 创建交互物
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeCreateInteract:BaseEffectType 
    {
        /// <summary>
        /// 创建坐标
        /// <summary>
        [LabelText("创建坐标")]
        [HideReferenceObjectPicker]
        public InputKey BornPos= new InputKey();

        /// <summary>
        /// 创建朝向
        /// <summary>
        [LabelText("创建朝向")]
        [HideReferenceObjectPicker]
        public InputKey BornToward= new InputKey();

        /// <summary>
        /// 交互物配置ID
        /// <summary>
        [LabelText("交互物配置ID")]
        [HideReferenceObjectPicker]
        public InputKey InteractConfigID= new InputKey();

    }

}