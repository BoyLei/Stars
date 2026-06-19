
///--------------------------------------------------------------------
/// 文件名   :   SpecEffect
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
    /// SpecEffect
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class SpecEffect 
    {
        /// <summary>
        /// 挂点
        /// <summary>
        [LabelText("挂点")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_hangpoint")]
        public HangPoint HangPoint=HangPoint.Root;

        /// <summary>
        /// 是否全局可见
        /// <summary>
        [LabelText("是否全局可见")]
        public bool IsAll=true;

        /// <summary>
        /// 是否跟随技能结束
        /// <summary>
        [LabelText("是否跟随技能结束")]
        public bool IsSyncBreak=true;

        /// <summary>
        /// 是否跟随阶段结束
        /// <summary>
        [LabelText("是否跟随阶段结束")]
        public bool IsChangeCancel;

        /// <summary>
        /// 是否面向施法者
        /// <summary>
        [LabelText("是否面向施法者")]
        public bool IsFaceToBuilder;

        /// <summary>
        /// 是否面向摄像机
        /// <summary>
        [LabelText("是否面向摄像机")]
        public bool IsFaceToCamera;

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
        /// 是否跟随循环
        /// <summary>
        [LabelText("是否跟随循环")]
        public bool IsFollowLoop;

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
        /// 是否自身循环
        /// <summary>
        [LabelText("是否自身循环")]
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
        /// Scale: X
        /// <summary>
        [LabelText("Scale: X")]
        [HorizontalGroup("scale", 0.1f)][LabelWidth(70f)]
        public int XScale=100;

        /// <summary>
        /// Y
        /// <summary>
        [LabelText("Y")]
        [HorizontalGroup("scale", 0.1f)][LabelWidth(20f)]
        public int YScale=100;

        /// <summary>
        /// Z
        /// <summary>
        [LabelText("Z")]
        [HorizontalGroup("scale", 0.1f)][LabelWidth(20f)]
        public int ZScale=100;

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