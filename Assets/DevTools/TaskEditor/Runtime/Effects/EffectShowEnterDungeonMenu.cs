using Sirenix.OdinInspector;

namespace Task
{
    [System.Serializable]
    public class EffectShowEnterDungeonMenu : BaseEffect
    {

        [LabelText("配置表ID")]
        public int ID;


        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);

            effectJson.Args1 = ID.ToString();

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            ID = ToInt(effectJson.Args1);

        }
    }
}