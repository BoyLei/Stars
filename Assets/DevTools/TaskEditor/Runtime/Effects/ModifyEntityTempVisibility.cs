using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class ModifyEntityTempVisibility : BaseEffect
    {
        [LabelText("对象类型")]
        [ValueDropdown("GetEntityType")]
        public TaskEntityType EntityType;

        [LabelText("配置ID")]
        public int ConfigID;

        [LabelText("显示、隐藏")]
        [SuffixLabel("false- 隐藏 ,true- 显示")]
        public bool Visibility;
        
        public IEnumerable GetEntityType()
        {
            return TaskEnumUtils._taskentitytypes;
        }
        
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = ((int)EntityType).ToString();
            effectJson.Args2 = ConfigID.ToString();
            effectJson.Args3 = Visibility?"1":"0";
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            EntityType = (TaskEntityType)ToInt(effectJson.Args1);
            ConfigID = ToInt(effectJson.Args2);
            Visibility = ToBoolean(effectJson.Args3);
        }
    }
}