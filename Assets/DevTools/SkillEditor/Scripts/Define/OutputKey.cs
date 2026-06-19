
///--------------------------------------------------------------------
/// 文件名   :   OutputKey
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
    /// 输出KEY
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class OutputKey 
    {
        /// <summary>
        /// Guid
        /// <summary>
        [LabelText("Guid")]
        [HideInInspector][Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]
        public string Guid;

        /// <summary>
        /// 是否保存在技能中
        /// <summary>
        [LabelText("是否保存在技能中")]
        public bool SaveSkill;

        /// <summary>
        /// 输出值
        /// <summary>
        [LabelText("输出值")]
        [OnValueChanged("OnResultChange")]
        public string Result="";

    }

}