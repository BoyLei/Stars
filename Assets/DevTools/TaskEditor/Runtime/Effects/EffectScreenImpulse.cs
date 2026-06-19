///--------------------------------------------------------------------
/// 文件名   :   EffectScreenImpulse.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/03/11 15:06:41
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    public class EffectScreenImpulse : BaseEffect
    {
        [LabelText("资源路径")]
        public string Path;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = Path;
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            Path = effectJson.Args1;
        }
    }
}