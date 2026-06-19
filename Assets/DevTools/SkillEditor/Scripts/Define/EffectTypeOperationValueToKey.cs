
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeOperationValueToKey
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
    /// 数值运算产生器
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeOperationValueToKey:BaseEffectType 
    {
        /// <summary>
        /// 数值Key列表
        /// <summary>
        [LabelText("数值Key列表")]
        [HideReferenceObjectPicker]
        public List<InputKey> ValueList= new List<InputKey>();

        /// <summary>
        /// 运算过程（a+b-c*d/e）
        /// <summary>
        [LabelText("运算过程（a+b-c*d/e）")]
        [HideReferenceObjectPicker]
        public string OperationProcess;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}