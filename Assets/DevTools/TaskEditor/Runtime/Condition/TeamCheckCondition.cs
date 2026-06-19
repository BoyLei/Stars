using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class TeamCheckCondition : CMCondition
    {
        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
        }
    }
}
