///--------------------------------------------------------------------
/// 文件名   :   EffectWait.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/03/06 16:31:19
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
    public class EffectWait : BaseEffect
    {
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
        }
    }
}
