using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 装备强化
    /// </summary>
    [System.Serializable]
    public class EquipQualityCondition : CMCondition
    {
        [LabelText("x品质装备")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int Quality;

        [LabelText("x件")]
        [Newtonsoft.Json.JsonProperty("Args2")]
        public int Count;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = Quality.ToString();
            conditon.Args2 = Count.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            Quality = ToInt(conditon.Args1);
            Count = ToInt(conditon.Args2);
        }
    }
}
