
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeStealth
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
    /// 隐身
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeStealth:BaseEffectType 
    {
        /// <summary>
        /// 持续时间
        /// <summary>
        [LabelText("持续时间")]
        public int DurningTime;

        /// <summary>
        /// 技能状态切换是否同步取消播放配置
        /// <summary>
        [LabelText("技能状态切换是否同步取消播放配置")]
        public bool IsChangeCancel=true;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}