///--------------------------------------------------------------------
/// 文件名   :   EffectAddBuff.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:24:56
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
    public class EffectAddBuff : BaseEffect
    {
        [LabelText("BuffID")]
        public int BuffID;

        [LabelText("Buff层数")]
        public int BuffCount;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = BuffID.ToString();
            effectJson.Args2 = BuffCount.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            BuffID = ToInt(effectJson.Args1);
            BuffCount = ToInt(effectJson.Args2);
        }
    }
}