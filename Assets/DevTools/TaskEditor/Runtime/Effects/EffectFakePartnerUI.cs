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
    public class EffectFakePartnerUI : BaseEffect
    {
        [LabelText("伙伴id")]
        public int id;

        [LabelText("是否显示")]//0关闭1显示
        public int isShow;


        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = id.ToString();
            effectJson.Args2 = isShow.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            id = ToInt(effectJson.Args1);
            isShow = ToInt(effectJson.Args2);
        }
    }
}
