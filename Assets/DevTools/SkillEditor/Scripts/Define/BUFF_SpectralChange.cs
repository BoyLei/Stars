
///--------------------------------------------------------------------
/// 文件名   :   BUFF_SpectralChange
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
    /// 量谱显示方式修改
    /// </summary>
    [System.Serializable]
    public  class BUFF_SpectralChange:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 量谱显示类型
        /// <summary>
        [LabelText("量谱显示类型")]
        public int SpectralType= new int();

    }

}