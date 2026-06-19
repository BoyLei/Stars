
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeGetMirrorPoint
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
    /// 获得镜像点
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeGetMirrorPoint:BaseEffectType 
    {
        /// <summary>
        /// 起点
        /// <summary>
        [LabelText("起点")]
        [HideReferenceObjectPicker]
        public SingleCenterPos StartCenterPos= new SingleCenterPos();

        /// <summary>
        /// 目标点
        /// <summary>
        [LabelText("目标点")]
        [HideReferenceObjectPicker]
        public SingleCenterPos TargetCenterPos= new SingleCenterPos();

        /// <summary>
        /// 数量
        /// <summary>
        [LabelText("数量")]
        public int Count=1;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}