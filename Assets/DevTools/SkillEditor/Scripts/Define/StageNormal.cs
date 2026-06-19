
///--------------------------------------------------------------------
/// 文件名   :   StageNormal
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
    /// 普通阶段参数
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public partial class StageNormal 
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
        public int StageType=0;

        /// <summary>
        /// 激活
        /// <summary>
        [LabelText("激活")]
        public bool Active=true;

        /// <summary>
        /// 循环阶段
        /// <summary>
        [LabelText("循环阶段")]
        public int LoopCount=1;

        /// <summary>
        /// 进入阶段是否进入CD
        /// <summary>
        [LabelText("进入阶段是否进入CD")]
        public bool IsEntryCD=true;

        /// <summary>
        /// 原子状态
        /// <summary>
        [LabelText("原子状态")]
        [ValueDropdown("_battlestate")]
        public List<BattleState> States= new List<BattleState>();

        /// <summary>
        /// 打断技能时
        /// <summary>
        [LabelText("打断技能时")]
        [ValueDropdown("_interrupt")]
        public InterruptEvent Interrupt=InterruptEvent.KillStage;

        /// <summary>
        /// 属性修改
        /// <summary>
        [LabelText("属性修改")]
        public List<BattleProp> BattleProp= new List<BattleProp>();

        /// <summary>
        /// 属性组ID
        /// <summary>
        [LabelText("属性组ID")]
        [PropertyTooltip("如果有这个值就覆盖上面的属性修改数组")]
        public int BattleGroupID;

        /// <summary>
        /// 是否不通知客户端
        /// <summary>
        [LabelText("是否不通知客户端")]
        [HideInInspector]
        public bool IsMsgClient;

        public IEnumerable _interrupt()
        {
            return EnumDefineMap._interruptevent;
        }

    }

}