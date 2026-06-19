///--------------------------------------------------------------------
/// 文件名   :   EffectTranslate.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/03/06 16:31:48
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
    public class EffectTranslate : BaseEffect
    {
        [LabelText("地图ID")]
        public int MapID;
        
        [LabelText("SpwanerID")]
        public int Spid;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = MapID.ToString();
            effectJson.Args2 = Spid.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            MapID = ToInt(effectJson.Args1);
            Spid = ToInt(effectJson.Args2);
        }
    }
}
