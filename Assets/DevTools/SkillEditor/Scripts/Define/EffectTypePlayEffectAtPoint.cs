
///--------------------------------------------------------------------
/// 文件名   :   EffectTypePlayEffectAtPoint
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
    /// 在点位播放特效
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class EffectTypePlayEffectAtPoint:BaseEffectType 
    {
        /// <summary>
        /// 中心点
        /// <summary>
        [LabelText("中心点")]
        [HideReferenceObjectPicker]
        public CenterPosArray CenterPosArray= new CenterPosArray();

        /// <summary>
        /// 朝向
        /// <summary>
        [LabelText("朝向")]
        [HideReferenceObjectPicker]
        public TowardArray TowardArray= new TowardArray();

        /// <summary>
        /// 特效
        /// <summary>
        [LabelText("特效")]
        [HideReferenceObjectPicker]
        public EffectTypeHitEffect PrefabPathInEditor= new EffectTypeHitEffect();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}