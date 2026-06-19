///--------------------------------------------------------------------
/// 文件名   :   EffectPlayPlot.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:30:00
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class EffectPlayPlot : BaseEffect
    {
        [LabelText("章节ID")]
        public int PlotID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = PlotID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            PlotID = ToInt(effectJson.Args1);
        }
    }
}