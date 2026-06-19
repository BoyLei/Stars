
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChangeMana
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
    /// 修改蓝量
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChangeMana:BaseEffectType 
    {
        /// <summary>
        /// 效果目标Key
        /// <summary>
        [LabelText("效果目标Key")]
        [HideReferenceObjectPicker]
        public InputKey EffectTargetArray= new InputKey();

        /// <summary>
        /// 数值组ID
        /// <summary>
        [LabelText("数值组ID")]
        public int GroupID;

        /// <summary>
        /// 数值组Key
        /// <summary>
        [LabelText("数值组Key")]
        [HideReferenceObjectPicker]
        public InputKey[] GroupIDKey;

        /// <summary>
        /// 飘字类型（DamageText）
        /// <summary>
        [LabelText("飘字类型（DamageText）")]
        public int DamageTextID;

        /// <summary>
        /// 回蓝特效
        /// <summary>
        [LabelText("回蓝特效")]
        [HideReferenceObjectPicker]
        public List<EffectTypeHitEffect> HitEffect= new List<EffectTypeHitEffect>();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}