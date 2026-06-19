using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Task
{
    [System.Serializable]
    public class EffectShowMessageBox : BaseEffect
    {
        [LabelText("弹窗ID")]
        [SuffixLabel("弹窗配置表_Window.xlsm")]
        public int ConfigID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = ConfigID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            ConfigID = ToInt(effectJson.Args1);
        }
    }
}