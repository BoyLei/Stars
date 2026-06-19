using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 藏宝图是否激活
    /// </summary>
    [System.Serializable]
    public class CheckBuffCondition : CMCondition
    {        
        [LabelText("通关指定关卡")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int BuffID;
        
        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = BuffID.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            BuffID = ToInt(conditon.Args1);
        }
    }
}