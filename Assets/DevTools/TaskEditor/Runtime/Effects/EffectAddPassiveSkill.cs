using Sirenix.OdinInspector;
using System.Collections;

namespace Task
{
    /// <summary>
    /// 添加被动
    /// 1.被动等级
    /// 2.技能等级
    /// 3.被动源：0固定1秘境随机被动【不填默认0】
    /// </summary>
    [System.Serializable]
    public class EffectAddPassiveSkill : BaseEffect
    {
        [LabelText("被动等级")]
        public int PassiveSkillID;

        [LabelText("技能等级")]
        public int PassiveSkillLevel;

        [LabelText("被动源")]
        [ValueDropdown("GetPassiveSkillSourceType")]
        public TaskPassiveSkillSourceType Source = TaskPassiveSkillSourceType.Default;

        public IEnumerable GetPassiveSkillSourceType()
        {
            return TaskEnumUtils._taskpassiveskillsourcetypes;
        }

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = PassiveSkillID.ToString();
            effectJson.Args2 = PassiveSkillLevel.ToString();
            effectJson.Args3 = ((int)Source).ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            PassiveSkillID = ToInt(effectJson.Args1);
            PassiveSkillLevel = ToInt(effectJson.Args2);
            Source = (TaskPassiveSkillSourceType)ToInt(effectJson.Args3);
        }
    }
}
