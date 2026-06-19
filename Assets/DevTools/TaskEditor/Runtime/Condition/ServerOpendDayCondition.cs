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
    public class ServerOpendDayCondition : CMCondition
    {        
        [LabelText("开服天数")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int Day;
        
        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = Day.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            Day = ToInt(conditon.Args1);
        }
    }
}
