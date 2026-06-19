using Sirenix.OdinInspector;

namespace Task
{
    /// <summary>
    /// תְ转职
    /// </summary>
    [System.Serializable]
    public class EffectTransJob : BaseEffect
    {
        [LabelText("转职后的ID")]
        public int TransJobID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = TransJobID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            TransJobID = ToInt(effectJson.Args1);
        }
    }
}

