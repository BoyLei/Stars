
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeGetPointWithBuffTime
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
    /// 随BUFF时间计算点位
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeGetPointWithBuffTime:BaseEffectType 
    {
        /// <summary>
        /// 初始角度
        /// <summary>
        [LabelText("初始角度")]
        public int StartRot;

        /// <summary>
        /// 随时间旋转速度(度/秒)
        /// <summary>
        [LabelText("随时间旋转速度(度/秒)")]
        public int RotSpeed;

        /// <summary>
        /// 与中心点距离
        /// <summary>
        [LabelText("与中心点距离")]
        public int Distance;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}