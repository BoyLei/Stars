using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    public class EffectActiveSceneCam : BaseEffect
    {
        [LabelText("索引")]
        public int Index;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = Index.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            Index = ToInt(effectJson.Args1);
        }
    }
}