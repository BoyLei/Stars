
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeClientSummonTurnTo
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
    /// 客户端召唤物锁定目标
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeClientSummonTurnTo:BaseEffectType 
    {
        /// <summary>
        /// 朝向目标
        /// <summary>
        [LabelText("朝向目标")]
        [HideReferenceObjectPicker]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 召唤物索引
        /// <summary>
        [LabelText("召唤物索引")]
        [HideReferenceObjectPicker]
        public List<int> Index= new List<int>();

        /// <summary>
        /// 多久内朝向目标
        /// <summary>
        [LabelText("多久内朝向目标")]
        public int TurnTime;

        /// <summary>
        /// 朝向目标时间
        /// <summary>
        [LabelText("朝向目标时间")]
        public int LookTime;

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

    }

}