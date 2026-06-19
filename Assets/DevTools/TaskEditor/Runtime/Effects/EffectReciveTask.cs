///--------------------------------------------------------------------
/// 文件名   :   EffectReciveTask.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:25:57
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
    public class EffectReciveTask : BaseEffect
    {
        [LabelText("任务ID")]
        public int TaskID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = TaskID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            TaskID = ToInt(effectJson.Args1);
        }
    }
}