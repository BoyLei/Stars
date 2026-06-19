using Sirenix.OdinInspector;
using System.Collections;

namespace Task
{
    /// <summary>
    /// 玩家自身显隐状态	
    /// "0未定义
    ///1不能看到别人
    ///2别人看不到我"	
    ///"0：false
    ///1：true"
    /// </summary>
    [System.Serializable]
    public class EffectChanageHide : BaseEffect
    {
        [LabelText("对象类型")]
        [ValueDropdown("GetEntityHideType")]
        public TaskEntityHideType HideType;

        [LabelText("开关")]
        public int State;


        public IEnumerable GetEntityHideType()
        {
            return TaskEnumUtils._taskentityhidetypes;
        }

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = ((int)HideType).ToString();
            effectJson.Args2 = State.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            HideType = (TaskEntityHideType)ToInt(effectJson.Args1);
            State = ToInt(effectJson.Args2);
        }
    }
}
