
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_ChangeColorShader
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
    /// 修改整体颜色Shader
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class GlobalShowBUFF_ChangeColorShader:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 主色调
        /// <summary>
        [LabelText("主色调")]
        [Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]
        public Color MainColor= new Color();

        /// <summary>
        /// 主色调红
        /// <summary>
        [LabelText("主色调红")]
        [HideInInspector]
        public int MainColorR;

        /// <summary>
        /// 主色调绿
        /// <summary>
        [LabelText("主色调绿")]
        [HideInInspector]
        public int MainColorG;

        /// <summary>
        /// 主色调蓝
        /// <summary>
        [LabelText("主色调蓝")]
        [HideInInspector]
        public int MainColorB;

        /// <summary>
        /// 主色调透
        /// <summary>
        [LabelText("主色调透")]
        [HideInInspector]
        public int MainColorA;

        /// <summary>
        /// 边缘色
        /// <summary>
        [LabelText("边缘色")]
        [Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]
        public Color EdgeColor= new Color();

        /// <summary>
        /// 边缘色红
        /// <summary>
        [LabelText("边缘色红")]
        [HideInInspector]
        public int EdgeColorR;

        /// <summary>
        /// 边缘色绿
        /// <summary>
        [LabelText("边缘色绿")]
        [HideInInspector]
        public int EdgeColorG;

        /// <summary>
        /// 边缘色蓝
        /// <summary>
        [LabelText("边缘色蓝")]
        [HideInInspector]
        public int EdgeColorB;

        /// <summary>
        /// 边缘色透
        /// <summary>
        [LabelText("边缘色透")]
        [HideInInspector]
        public int EdgeColorA;

        /// <summary>
        /// 主色调
        /// <summary>
        [LabelText("主色调")]
        [JsonIgnore][HideInInspector][MessagePack.IgnoreMember]
        public Color mainColor=Color.clear;

        /// <summary>
        /// 边缘色
        /// <summary>
        [LabelText("边缘色")]
        [JsonIgnore][HideInInspector][MessagePack.IgnoreMember]
        public Color edgeColor=Color.clear;

        /// <summary>
        /// 边缘比例
        /// <summary>
        [LabelText("边缘比例")]
        public int EdgePer;

    }

}