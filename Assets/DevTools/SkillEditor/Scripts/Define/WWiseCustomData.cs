
///--------------------------------------------------------------------
/// 文件名   :   WWiseCustomData
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
    /// WWise自定义数据
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class WWiseCustomData:BaseCustomData 
    {
        /// <summary>
        /// WWise
        /// <summary>
        [LabelText("WWise")]
        public string WWiseName;

    }

}