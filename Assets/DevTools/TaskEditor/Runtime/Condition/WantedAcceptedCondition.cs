using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 通缉任务是否激活
    /// </summary>
    [System.Serializable]
    public class WantedAcceptedCondition : CMCondition
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
