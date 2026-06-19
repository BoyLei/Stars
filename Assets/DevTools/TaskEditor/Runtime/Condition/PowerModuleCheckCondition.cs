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
    public class PowerModuleCheckCondition : CMCondition
    {
        [LabelText("战力模块")] //（0总，1装备，2伙伴，3角色，4技能）
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int moduleID;

        [LabelText("战力下限")]//（需求的战力数字）
        [Newtonsoft.Json.JsonProperty("Args2")]
        public int limit;
        [LabelText("历史还是当前")]//（0历史，1当前）
        [Newtonsoft.Json.JsonProperty("Args3")]
        public int type;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = moduleID.ToString();
            conditon.Args2 = limit.ToString();
            conditon.Args3 = type.ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            moduleID = Convert.ToInt32(conditon.Args1);
            limit = Convert.ToInt32(conditon.Args2);
            type = Convert.ToInt32(conditon.Args3);
        }
    }
}
