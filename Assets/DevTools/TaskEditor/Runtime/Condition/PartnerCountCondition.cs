using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 伙伴数量
    /// </summary>
    [System.Serializable]
    public class PartnerCountCondition : CMCondition
    {
        [LabelText("x级伙伴")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int PartnerLevel;

        [LabelText("x数量")]
        [Newtonsoft.Json.JsonProperty("Args2")]
        public int Count;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = PartnerLevel.ToString();
            conditon.Args2 = Count.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            PartnerLevel = ToInt(conditon.Args1);
            Count = ToInt(conditon.Args2);
        }
    }
}
