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
    public class CheckBlackBoardCondition : CMCondition
    {
        [LabelText("黑板Key")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public string key;

        [LabelText("黑板值")]
        [Newtonsoft.Json.JsonProperty("Args2")]
        public int value;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = key.ToString();
            conditon.Args2 = value.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            key = conditon.Args1.ToString();
            value = Convert.ToInt32(conditon.Args2);
        }
    }
}
