using Sirenix.OdinInspector;

namespace Task
{
    [System.Serializable]
    public class EffectPlayEffectFormPlayerToPoint : BaseEffect
    {
        [LabelText("特效路径")]
        public string EffectPath;
        
        [LabelText("X")]
        public float X;

        [LabelText("Y")]
        public float Y;
        
        [LabelText("Z")]
        public float Z;
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = EffectPath;
            effectJson.Args2 = X.ToString();
            effectJson.Args3 = Y.ToString();
            effectJson.Args4 = Z.ToString();

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            EffectPath = effectJson.Args1;
            X = ToFloat(effectJson.Args2);
            Y = ToFloat(effectJson.Args3);
            Z = ToFloat(effectJson.Args4);

        }
    }
}