///--------------------------------------------------------------------
/// 文件名   :   ITrigger.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/05 09:49:41
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Task;

namespace Trigger
{
    public interface ITrigger
    {
        TrrigerType M_TriggerType { get; }
        ulong M_EntityID { get; }
        void EnterFrame(int frameIndex);
    }
}