
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeChangeToAbsoluteToward
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
    /// 目标集合修改至绝对朝向
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeChangeToAbsoluteToward:BaseEffectType 
    {
        /// <summary>
        /// 被修改者
        /// <summary>
        [LabelText("被修改者")]
        [HideReferenceObjectPicker]
        public InputKey Target= new InputKey();

        /// <summary>
        /// 转向时间
        /// <summary>
        [LabelText("转向时间")]
        [HideReferenceObjectPicker]
        public InputKey ChangeTime= new InputKey();

        /// <summary>
        /// 最大转向角度
        /// <summary>
        [LabelText("最大转向角度")]
        [HideReferenceObjectPicker]
        public InputKey MaxAngle= new InputKey();

        /// <summary>
        /// 目标朝向
        /// <summary>
        [LabelText("目标朝向")]
        [HideReferenceObjectPicker]
        public InputKey TargetToward= new InputKey();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}