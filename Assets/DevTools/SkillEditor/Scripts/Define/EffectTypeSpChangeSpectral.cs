
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeSpChangeSpectral
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
    /// 昧光增加影能量
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeSpChangeSpectral:BaseEffectType 
    {
        /// <summary>
        /// 判断碰撞盒
        /// <summary>
        [LabelText("判断碰撞盒")]
        [HideReferenceObjectPicker]
        public InputKey CheckKey= new InputKey();

        /// <summary>
        /// 判断BUFF
        /// <summary>
        [LabelText("判断BUFF")]
        public List<int> CheckBuff= new List<int>();

        /// <summary>
        /// 增加值
        /// <summary>
        [LabelText("增加值")]
        public int AddValue;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}