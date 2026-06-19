
///--------------------------------------------------------------------
/// 文件名   :   AnimatorCustomData
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
    /// 动画自定义数据
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class AnimatorCustomData:BaseCustomData 
    {
        /// <summary>
        /// 动画名称
        /// <summary>
        [LabelText("动画名称")]
        public string AnimatorName;

        /// <summary>
        /// 融合时间
        /// <summary>
        [LabelText("融合时间")]
        public int FadeDuration=100;

        /// <summary>
        /// 特殊条件动画
        /// <summary>
        [LabelText("特殊条件动画")]
        public List<AnimAndCondition> AnimatorSp= new List<AnimAndCondition>();

    }

}