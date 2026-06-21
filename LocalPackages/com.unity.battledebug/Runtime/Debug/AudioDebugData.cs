///--------------------------------------------------------------------
/// 文件名   :   AudioDebugData.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/06 10:02:02
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace BattleDebug
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AudioDebugData : DebugData
    {
        public override DEBUGTYPE DeBugType
        {
            get
            {
                return DEBUGTYPE.DEBUG_AUDIO;
            }
        }

        public AudioDebugData(int skillID, string uniqueid, bool start) : base(skillID)
        {
            this.UniqueID = uniqueid;
            this.IsStart = start;
        }
    }
}
