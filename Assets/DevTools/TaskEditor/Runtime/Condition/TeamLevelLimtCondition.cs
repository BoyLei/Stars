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
    public class TeamLevelLimtCondition : CMCondition
    {        
        [LabelText("等级")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int Level;
        
        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = Level.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            Level = ToInt(conditon.Args1);
        }
    }
}

