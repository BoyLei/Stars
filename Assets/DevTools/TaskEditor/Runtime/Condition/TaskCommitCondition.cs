///--------------------------------------------------------------------
/// 文件名   :   TaskCommitCondition.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 10:48:36
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.Serialization;

namespace Task
{
    [System.Serializable]
    public class TaskCommitCondition : CMCondition
    {
        [LabelText("任务ID")]
        public uint TaskID;


        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = TaskID.ToString();

        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            TaskID = (uint)ToInt(conditon.Args1);
        }
    }
}