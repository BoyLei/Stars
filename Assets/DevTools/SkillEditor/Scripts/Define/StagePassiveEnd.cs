
///--------------------------------------------------------------------
/// 文件名   :   StagePassiveEnd
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
    /// 被动结束阶段参数
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class StagePassiveEnd 
    {
        /// <summary>
        /// 阶段ID
        /// <summary>
        [LabelText("阶段ID")]
        [Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]
        public int StageID;

        /// <summary>
        /// 阶段类型
        /// <summary>
        [LabelText("阶段类型")]
        [HideInInspector]   [Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]
        public int StageType=5;

        /// <summary>
        /// 是否不通知客户端
        /// <summary>
        [LabelText("是否不通知客户端")]
        [HideInInspector]
        public bool IsMsgClient;

    }

}