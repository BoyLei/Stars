
///--------------------------------------------------------------------
/// 文件名   :   StageTrigger
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
    /// 触发阶段参数
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class StageTrigger 
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
        public int StageType=1;

        /// <summary>
        /// 触发次数
        /// <summary>
        [LabelText("触发次数")]
        public int TriggerCount;

        /// <summary>
        /// 触发阶段
        /// <summary>
        [LabelText("触发阶段")]
        [ValueDropdown("_stageenum")]
        public TriggerStageEnum StageEnum= new TriggerStageEnum();

        /// <summary>
        /// 触发条件组
        /// <summary>
        [LabelText("触发条件组")]
        [Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]
        public List<TriggerTypeEnumSerialize> ConditionGroupInEditor= new List<TriggerTypeEnumSerialize>();

        /// <summary>
        /// 是否不通知客户端
        /// <summary>
        [LabelText("是否不通知客户端")]
        [HideInInspector]
        public bool IsMsgClient;

        /// <summary>
        /// 触发条件组
        /// <summary>
        [LabelText("触发条件组")]
        [HideInInspector]
        public List<TriggerCondition> ConditionGroup= new List<TriggerCondition>();

        public IEnumerable _stageenum()
        {
            return EnumDefineMap._triggerstageenum;
        }

    }

}