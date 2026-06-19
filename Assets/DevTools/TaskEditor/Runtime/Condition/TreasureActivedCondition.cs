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
    public class TreasureActivedCondition : CMCondition
    {        
        [LabelText("交互物唯一Index")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int Index;
        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = Index.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            Index = ToInt(conditon.Args1);
        }
    }
}