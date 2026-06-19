
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeHitEffect
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
    /// 受击特效
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class EffectTypeHitEffect 
    {
        /// <summary>
        /// 特效路径
        /// <summary>
        [LabelText("特效路径")]
        [FilePath]
        public string PrefabPathInEditor;

        /// <summary>
        /// 挂点
        /// <summary>
        [LabelText("挂点")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_hangpoint")]
        public HangPoint HangPoint=HangPoint.Root;

        /// <summary>
        /// 是否朝向施法者
        /// <summary>
        [LabelText("是否朝向施法者")]
        public bool IsFaceToBuilder;

        /// <summary>
        /// 是否跟随位置
        /// <summary>
        [LabelText("是否跟随位置")]
        public bool IsFollowMove;

        /// <summary>
        /// 是否跟随旋转
        /// <summary>
        [LabelText("是否跟随旋转")]
        public bool IsFollowRot;

        /// <summary>
        /// 是否跟随主人隐藏
        /// <summary>
        [LabelText("是否跟随主人隐藏")]
        public bool IsFollowOwnerHide;

        /// <summary>
        /// 是否跟随主人放缩
        /// <summary>
        [LabelText("是否跟随主人放缩")]
        [ValueDropdown("_isfollowownerscale")]
        public SpecEffectScale IsFollowOwnerScale= new SpecEffectScale();

        /// <summary>
        /// 是否循环
        /// <summary>
        [LabelText("是否循环")]
        public bool IsLoop;

        /// <summary>
        /// Position: X
        /// <summary>
        [LabelText("Position: X")]
        [HorizontalGroup("offset", 0.1f)][LabelWidth(70f)]
        public int XOffset;

        /// <summary>
        /// Y
        /// <summary>
        [LabelText("Y")]
        [HorizontalGroup("offset", 0.1f)][LabelWidth(20f)]
        public int YOffset;

        /// <summary>
        /// Z
        /// <summary>
        [LabelText("Z")]
        [HorizontalGroup("offset", 0.1f)][LabelWidth(20f)]
        public int ZOffset;

        /// <summary>
        /// Rotation: X
        /// <summary>
        [LabelText("Rotation: X")]
        [HorizontalGroup("offsetToward", 0.1f)][LabelWidth(70f)]
        public int XOffsetTowards;

        /// <summary>
        /// Y
        /// <summary>
        [LabelText("Y")]
        [HorizontalGroup("offsetToward", 0.1f)][LabelWidth(20f)]
        public int YOffsetTowards;

        /// <summary>
        /// Z
        /// <summary>
        [LabelText("Z")]
        [HorizontalGroup("offsetToward", 0.1f)][LabelWidth(20f)]
        public int ZOffsetTowards;

        /// <summary>
        /// 特效参数
        /// <summary>
        [LabelText("特效参数")]
        [HideInInspector]
        public FXJson fxJson= new FXJson();

        public IEnumerable _hangpoint()
        {
            return EnumDefineMap._hangpoint;
        }

        public IEnumerable _isfollowownerscale()
        {
            return EnumDefineMap._speceffectscale;
        }

    }

}