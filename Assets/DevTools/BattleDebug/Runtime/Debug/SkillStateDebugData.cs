///--------------------------------------------------------------------
/// 文件名   :   SkillStateDebugData.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/08 11:34:24
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDebug
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SkillStateDebugData : DebugData
    {
        public bool IsClient { get; private set; }

        public bool IsEnd { get; private set; }
        public override DEBUGTYPE DeBugType
        {
            get
            {
                return DEBUGTYPE.DEBUG_SKILL_STATE_EVENT;
            }
        }

        public SkillStateDebugData(string skillUID, string UniqueID, bool start,bool isClient) : base(skillUID)
        {
            this.UniqueID = UniqueID;
            this.IsEnd = !start;
            this.IsStart = true;
            this.IsClient = isClient;
        }
    }
}