///--------------------------------------------------------------------
/// 文件名   :   EffectTaskProcess.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:29:06
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
    public class EffectTaskProcess : BaseEffect
    {
        [LabelText("任务ID")]
        public int TaskID;

        [LabelText("任务目标ID")]
        public int EventID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = TaskID.ToString();
            effectJson.Args2 = EventID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            TaskID = ToInt(effectJson.Args1);
            EventID = ToInt(effectJson.Args2);
        }
    }
}
