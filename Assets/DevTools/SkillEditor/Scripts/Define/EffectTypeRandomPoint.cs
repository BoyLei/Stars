
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeRandomPoint
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
    /// 随机多个点位
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeRandomPoint:BaseEffectType 
    {
        /// <summary>
        /// 中心点
        /// <summary>
        [LabelText("中心点")]
        [HideReferenceObjectPicker]
        public CenterPosArray CenterPosArray= new CenterPosArray();

        /// <summary>
        /// 距离中心点最小距离
        /// <summary>
        [LabelText("距离中心点最小距离")]
        public int MinDistance;

        /// <summary>
        /// 距离中心点最大距离
        /// <summary>
        [LabelText("距离中心点最大距离")]
        public int MaxDistance;

        /// <summary>
        /// 每个中心点附近点位数量
        /// <summary>
        [LabelText("每个中心点附近点位数量")]
        public int PointCount;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}