
///--------------------------------------------------------------------
/// 文件名   :   EffectTypePlayEffectBetweenPoints
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
    /// 在两点间播放特效
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class EffectTypePlayEffectBetweenPoints:BaseEffectType 
    {
        /// <summary>
        /// 发射中心点
        /// <summary>
        [LabelText("发射中心点")]
        [HideReferenceObjectPicker]
        public CenterPosArray FromCenterPosArray= new CenterPosArray();

        /// <summary>
        /// 接收中心点
        /// <summary>
        [LabelText("接收中心点")]
        [HideReferenceObjectPicker]
        public CenterPosArray ToCenterPosArray= new CenterPosArray();

        /// <summary>
        /// 单位长度
        /// <summary>
        [LabelText("单位长度")]
        public int UnitLength;

        /// <summary>
        /// 是否修改X
        /// <summary>
        [LabelText("是否修改X")]
        public bool IsChangeX;

        /// <summary>
        /// 是否修改Y
        /// <summary>
        [LabelText("是否修改Y")]
        public bool IsChangeY;

        /// <summary>
        /// 是否修改Z
        /// <summary>
        [LabelText("是否修改Z")]
        public bool IsChangeZ;

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