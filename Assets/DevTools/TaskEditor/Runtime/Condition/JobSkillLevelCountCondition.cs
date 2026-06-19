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
    public class JobSkillLevelCountCondition : CMCondition
    {
        [LabelText("技能等级总量")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int LevelCount;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = LevelCount.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            LevelCount = ToInt(conditon.Args1);
        }
    }
}
