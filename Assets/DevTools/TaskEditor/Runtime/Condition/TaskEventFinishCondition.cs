///--------------------------------------------------------------------
/// 文件名   :   TaskEventFinishCondition.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 10:27:38
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
namespace Task
{
    [System.Serializable]
    public class TaskEventFinishCondition : CMCondition
    {
        [LabelText("任务ID")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public uint TaskID;

        [LabelText("任务目标ID")]
        [Newtonsoft.Json.JsonProperty("Args2")]
        public int EventID;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = TaskID.ToString();
            conditon.Args2 = EventID.ToString();

        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            TaskID = (uint)ToInt(conditon.Args1);
            EventID = ToInt(conditon.Args2);
        }
    }
}