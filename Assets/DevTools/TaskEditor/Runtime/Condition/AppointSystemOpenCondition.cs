using Sirenix.OdinInspector;
using StarProjectDef;
using System;
using System.Collections;

namespace Task
{
    /// <summary>
    /// 指定系统解锁
    /// </summary>
    [System.Serializable]
    public class AppointSystemOpenCondition : CMCondition
    {
        [LabelText("系统")]
        [ValueDropdown("GetSystemOpenEnums")]
        public SystemOpenType systemID;
        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = ((int)systemID).ToString();
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            systemID=SystemOpenType.None;
            if (Enum.TryParse<SystemOpenType>(conditon.Args1,out SystemOpenType res))
            {
                systemID = res;
            }
        }

        public IEnumerable GetSystemOpenEnums()
        {
            return TaskEnumUtils.e_systemopentypes;
        }
    }
}
