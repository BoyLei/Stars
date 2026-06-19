using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class EffectClientSendModuleMsg : BaseEffect
    {
        [LabelText("Module名-客户端程序填写")]
        public string ModuleName;

        [LabelText("事件名-客户端程序填写")]
        public string EventName;
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = ModuleName;
            effectJson.Args2 = EventName;
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            ModuleName = effectJson.Args1;
            EventName = effectJson.Args2;

        }
    }
}