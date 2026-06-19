using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    public class EffectPlayFx : BaseEffect
    {
        [LabelText("对象类型")]
        [ValueDropdown("GetEntityType")]
        public TaskEntityType EntityType;
        
        [LabelText("配置ID")]
        public int ConffigID;
        
        [LabelText("特效路径")]
        public string FxName;

        [LabelText("时长（毫秒）")]
        public int DurationInMilliseconds;

        public IEnumerable GetEntityType()
        {
            return TaskEnumUtils._taskentitytypes;
        }
        
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = ((int)EntityType).ToString();
            effectJson.Args2 = ConffigID.ToString();
            effectJson.Args3 = FxName;
            effectJson.Args4 = DurationInMilliseconds .ToString(); // Convert milliseconds to seconds
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            EntityType = (TaskEntityType)(ToInt(effectJson.Args1));
            ConffigID = ToInt(effectJson.Args2);
            FxName = effectJson.Args3;
            DurationInMilliseconds = ToInt(effectJson.Args4); // Convert seconds to milliseconds
        }
    }
}
