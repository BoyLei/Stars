
///--------------------------------------------------------------------
/// 文件名   :   ShapeRoute
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
    /// 路径
    /// </summary>
    [System.Serializable]
    public  class ShapeRoute:BaseShap 
    {
        /// <summary>
        /// 宽度
        /// <summary>
        [LabelText("宽度")]
        public int Width= new int();

    }

}