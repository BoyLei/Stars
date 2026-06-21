///--------------------------------------------------------------------
/// 文件名   :   DebugData.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/06 09:38:00
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace BattleDebug
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class DebugData
    {
        public abstract DEBUGTYPE DeBugType { get; }
        public int SkillID;
        public string UniqueID;
        public bool IsStart;
        public string DisplayName;
        public List<string> Args = new List<string>();

        public DebugData(int skillID)
        {
            this.SkillID = skillID;
        }
    }
}