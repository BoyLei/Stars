
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_BUFFUIDateils
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
    /// BUFFUI显示详情
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class GlobalShowBUFF_BUFFUIDateils:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// BUFFUI显示详情
        /// <summary>
        [LabelText("BUFFUI显示详情")]
        [HideReferenceObjectPicker]
        public List<BUFFUIShow> BUFFUIShowDetailsList= new List<BUFFUIShow>();

    }

}