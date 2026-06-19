///--------------------------------------------------------------------
/// 文件名   :   EffectPlayPv.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/12 09:55:52
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
    public class EffectSystemGuide : BaseEffect
    {
        [LabelText("guideID")]
        public int guideID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = guideID.ToString();

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            guideID = ToInt(effectJson.Args1);
        }
    }
}
