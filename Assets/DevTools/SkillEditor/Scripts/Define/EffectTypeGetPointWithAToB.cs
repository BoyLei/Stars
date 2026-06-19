
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeGetPointWithAToB
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
    /// 从A指向B计算可达点
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeGetPointWithAToB:BaseEffectType 
    {
        /// <summary>
        /// 中心点
        /// <summary>
        [LabelText("中心点")]
        public SingleCenterPos StartCenterPos= new SingleCenterPos();

        /// <summary>
        /// 中点
        /// <summary>
        [LabelText("中点")]
        public SingleCenterPos EndCenterPos= new SingleCenterPos();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        public OutputKey OutputKey= new OutputKey();

    }

}