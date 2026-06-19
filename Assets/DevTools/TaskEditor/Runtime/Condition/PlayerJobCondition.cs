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
    public class PlayerJobCondition : CMCondition
    {        
        [LabelText("玩家职业")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int Job;
        
        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = Job.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            Job = ToInt(conditon.Args1);
        }
    }
}

