using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 冒险旅程阶段检
    /// </summary>
    [System.Serializable]
    public class AdventureCheckCondition : CMCondition
    {
        [LabelText("阶段ID")] 
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int stage;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = stage.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            stage = Convert.ToInt32(conditon.Args1);
        }
    }
}
