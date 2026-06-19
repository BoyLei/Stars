using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 角色累计获得技能点数
    /// </summary>
    [System.Serializable]
    public class SkillDotTotalCondition : CMCondition
    {
        [LabelText("技能点数")] 
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int value;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = value.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            value = Convert.ToInt32(conditon.Args1);
        }
    }
}
