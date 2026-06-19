
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeDamageSecond
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
    /// 二次伤害
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeDamageSecond:BaseEffectType 
    {
        /// <summary>
        /// 飘字类型（DamageText）
        /// <summary>
        [LabelText("飘字类型（DamageText）")]
        public int FlutteringWordsID;

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
        /// 是否是特殊攻击
        /// <summary>
        [LabelText("是否是特殊攻击")]
        [PropertyTooltip("如果该攻击为特殊攻击则不会受到流血、反伤等效果的影响")]
        public bool IsSpecial=false;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}