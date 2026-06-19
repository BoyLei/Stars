///--------------------------------------------------------------------
/// 文件名   :   EffectScenePlaysControl
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/31/22 2:31:39
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
    public class EffectScenePlaysControl:BaseEffect
    {
        [LabelText("开关")]
        public bool Open;

        [LabelText("PlayID")]
        public int PlayID;
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = PlayID.ToString();
            effectJson.Args2 = Open?"1":"0";

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            PlayID = ToInt(effectJson.Args1);
            Open = ToBoolean(effectJson.Args2);

        }
    }
}