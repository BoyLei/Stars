///--------------------------------------------------------------------
/// 文件名   :   EffectSendCustomEvent.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/24 16:42:10
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
    public class EffectSendCustomEvent : BaseEffect
    {
        [LabelText("自定义Key")]
        public string CustomKey;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = CustomKey;

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            CustomKey =effectJson.Args1;
        }
    }
}
