
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeLaunchBullet
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
    /// 发射子弹
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeLaunchBullet:BaseEffectType 
    {
        /// <summary>
        /// 接收者
        /// <summary>
        [LabelText("接收者")]
        [HideReferenceObjectPicker]
        public InputKey ToTarget= new InputKey();

        /// <summary>
        /// 接收者挂点
        /// <summary>
        [LabelText("接收者挂点")]
        [JsonConverter(typeof(StringEnumConverter))]
        [ValueDropdown("_totargethangpoint")]
        public HangPoint ToTargetHangPoint=HangPoint.Root;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        public IEnumerable _totargethangpoint()
        {
            return EnumDefineMap._hangpoint;
        }

    }

}