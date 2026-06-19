///--------------------------------------------------------------------
/// 文件名   :   EffectPassiveRemove.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/03/08 10:32:10
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
    public class EffectPassiveRemove : BaseEffect
    {
        [LabelText("被动技能ID")]
        public int PassiveID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = PassiveID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            PassiveID = ToInt(effectJson.Args1);
        }
    }
}