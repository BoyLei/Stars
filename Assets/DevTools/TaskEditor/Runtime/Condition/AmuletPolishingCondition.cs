using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 护符打磨
    /// </summary>
    [System.Serializable]
    public class AmuletPolishingCondition : CMCondition
    {
        [LabelText("护符数量")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int AmuletCount;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = AmuletCount.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            AmuletCount = ToInt(conditon.Args1);
        }
    }
}
