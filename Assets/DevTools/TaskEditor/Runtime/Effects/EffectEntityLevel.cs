///--------------------------------------------------------------------
/// 文件名   :   EffectEntityLevel.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:27:09
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
    public class EffectEntityLevel : BaseEffect
    {
        [LabelText("副本ID")]
        public int MapID;

        [LabelText("副本类型 1大场景  2副本")]
        public int MapType;
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = MapID.ToString();
            effectJson.Args2 = MapType.ToString();

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            MapID = ToInt(effectJson.Args1);
            MapType = ToInt(effectJson.Args2);

        }
    }
}