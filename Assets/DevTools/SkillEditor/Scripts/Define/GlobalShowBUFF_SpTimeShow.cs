
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_SpTimeShow
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
    /// 特殊时间展示
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowBUFF_SpTimeShow:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 时间展示方式
        /// <summary>
        [LabelText("时间展示方式")]
        [ValueDropdown("_sptimeshowtype")]
        public List<SpTimeShowTypeEnum> SpTimeShowType= new List<SpTimeShowTypeEnum>();

        public IEnumerable _sptimeshowtype()
        {
            return EnumDefineMap._sptimeshowtypeenum;
        }

    }

}