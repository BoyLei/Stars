///--------------------------------------------------------------------
/// 文件名   :   EffectPlayTimeline.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:29:38
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
namespace Task
{
    [System.Serializable]
    public class EffectPlayTimeline : BaseEffect
    {
        [LabelText("TimelineID")]
        public int TimelineID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = TimelineID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            TimelineID = ToInt(effectJson.Args1);
        }
    }
}
