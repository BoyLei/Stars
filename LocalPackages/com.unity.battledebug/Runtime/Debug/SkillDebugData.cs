///--------------------------------------------------------------------
/// 文件名   :   SkillDebugData.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/06 09:58:54
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace BattleDebug
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SkillDebugData : DebugData
    {
        public override DEBUGTYPE DeBugType
        {
            get
            {
                return DEBUGTYPE.DEBUG_SKILL;
            }
        }

        public SkillDebugData(int skillID, bool start) : base(skillID)
        {
            this.UniqueID = skillID.ToString();
            this.IsStart = start;
        }
    }
}