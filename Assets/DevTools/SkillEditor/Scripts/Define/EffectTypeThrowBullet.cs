
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeThrowBullet
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
    /// 抛出子弹
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeThrowBullet:BaseEffectType 
    {
        /// <summary>
        /// 目标点
        /// <summary>
        [LabelText("目标点")]
        [HideReferenceObjectPicker]
        public InputKey TargetPoint= new InputKey();

        /// <summary>
        /// 最大高度
        /// <summary>
        [LabelText("最大高度")]
        public int MaxHight;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}