
///--------------------------------------------------------------------
/// 文件名   :   BUFF_ChangeColorShader
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
using Newtonsoft.Json.Converters;
namespace SkillEditor
{
    /// <summary>
    /// 修改整体颜色Shader
    /// </summary>
    [System.Serializable]
    public  class BUFF_ChangeColorShader:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 比较符
        /// <summary>
        [LabelText("比较符")]
        [ValueDropdown("_compoperator")]
        public Color CompOperator= new Color();

        /// <summary>
        /// 隐藏标签
        /// <summary>
        [LabelText("隐藏标签")]
        [ValueDropdown("_hidelabels")]
        public Color HideLabels= new Color();

        
        public void ColorTos() {
            UnityEngine.ColorUtility.ToHtmlStringRGB(CompOperator);

            } 
    }

}