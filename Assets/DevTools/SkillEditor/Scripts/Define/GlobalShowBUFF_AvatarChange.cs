
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_AvatarChange
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
    /// 修改化身
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowBUFF_AvatarChange:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 化身ID
        /// <summary>
        [LabelText("化身ID")]
        public int AvatarID;

    }

}