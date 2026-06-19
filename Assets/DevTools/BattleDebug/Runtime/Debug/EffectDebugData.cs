///--------------------------------------------------------------------
/// 文件名   :   EffectDebugData.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/06 10:00:58
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace BattleDebug
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class EffectDebugData : DebugData
    {
        public override DEBUGTYPE DeBugType
        {
            get
            {
                return DEBUGTYPE.DEBUG_EFFECT;
            }
        }
        public EffectDebugData(string skillUID, string uniqueid, bool start) : base(skillUID)
        {
            this.UniqueID = uniqueid;
            this.IsStart = start;
        }
    }
}