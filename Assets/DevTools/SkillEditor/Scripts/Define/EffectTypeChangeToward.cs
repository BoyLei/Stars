
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChangeToward
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
    /// 修改朝向
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChangeToward:BaseEffectType 
    {
        /// <summary>
        /// 修改者Key
        /// <summary>
        [LabelText("修改者Key")]
        [HideReferenceObjectPicker]
        public InputKey EffectTargetArray= new InputKey();

        /// <summary>
        /// 朝向点
        /// <summary>
        [LabelText("朝向点")]
        [HideReferenceObjectPicker]
        public SingleCenterPos SingleCenterPos= new SingleCenterPos();

        /// <summary>
        /// 朝向偏移
        /// <summary>
        [LabelText("朝向偏移")]
        public int TowardOffset;

        /// <summary>
        /// 持续时间
        /// <summary>
        [LabelText("持续时间")]
        public int DurningTime;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}