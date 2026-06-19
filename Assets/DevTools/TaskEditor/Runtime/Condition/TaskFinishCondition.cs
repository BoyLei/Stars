///--------------------------------------------------------------------
/// 文件名   :   TaskFinishCondition.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/17 17:08:32
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class TaskFinishCondition : CMCondition
    {
        [LabelText("任务ID")]
        [Newtonsoft.Json.JsonProperty("Args1")]
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