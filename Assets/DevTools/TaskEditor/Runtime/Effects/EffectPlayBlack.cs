///--------------------------------------------------------------------
/// 文件名   :   EffectPlayBlack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:27:57
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
    public class EffectPlayBlack : BaseEffect
    {
        [LabelText("黑幕ID")]
        public int BlackID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = BlackID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            BlackID = ToInt(effectJson.Args1);
        }
    }
}
