///--------------------------------------------------------------------
/// 文件名   :   EffectSameSceneTranslate.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/24 16:43:36
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
    public class EffectSameSceneTranslate : BaseEffect
    {
        [LabelText("X")]
        public int X;

        [LabelText("Y")]
        public int Y;

        [LabelText("Z")]
        public int Z;

        [LabelText("角度")]
        public int Angle;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = X.ToString();
            effectJson.Args2 = Y.ToString();
            effectJson.Args3 = Z.ToString();
            effectJson.Args4 = Angle.ToString();

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            X = ToInt(effectJson.Args1);
            Y = ToInt(effectJson.Args2);
            Z = ToInt(effectJson.Args3);
            Angle = ToInt(effectJson.Args4);
        }
    }
}