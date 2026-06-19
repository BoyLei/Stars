using Sirenix.OdinInspector;
namespace Task
{
    [System.Serializable]
    public class EffectTriggerGuide : BaseEffect
    {
        [LabelText("引导ID")]
        public int GuideID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = GuideID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            GuideID = ToInt(effectJson.Args1);
        }
    }
}
