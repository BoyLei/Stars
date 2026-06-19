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
    public class TeamDailyPassCondition : CMCondition
    {
        [LabelText("副本id")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int id;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = id.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            id = ToInt(conditon.Args1);
        }
    }
}
