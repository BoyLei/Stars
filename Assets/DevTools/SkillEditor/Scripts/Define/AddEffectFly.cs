
///--------------------------------------------------------------------
/// 文件名   :   AddEffectFly
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
    /// BUFF飘字
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class AddEffectFly 
    {
        /// <summary>
        /// 类型
        /// <summary>
        [LabelText("类型")]
        public int Type;

        /// <summary>
        /// 飘字
        /// <summary>
        [LabelText("飘字")]
        [HideInInspector]
        public string val;

        /// <summary>
        /// 飘字
        /// <summary>
        [LabelText("飘字")]
        [HideInInspector]
        public string Value_Key;

        /// <summary>
        /// 飘字
        /// <summary>
        [LabelText("飘字")]
        [ShowInInspector]
        public string Value 
        {
           get{return OnValueGet();}set{OnValueSet(value);}
        }

    }

}