using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 个人副本通关
    /// </summary>
    [System.Serializable]
    public class PersonDailyPassCondition : CMCondition
    {
        [LabelText("副本id")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public ulong id;

        [LabelText("难度")]
        [Newtonsoft.Json.JsonProperty("Args2")]
        public ulong diff;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = id.ToString();
            conditon.Args2 = diff.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            id = Convert.ToUInt64(conditon.Args1);
            diff = Convert.ToUInt64(conditon.Args2);
        }
    }
}
