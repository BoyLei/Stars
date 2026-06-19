///--------------------------------------------------------------------
/// 文件名   :   EffectSceneControlFlag.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/24 16:43:01
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
    public class EffectSceneControlFlag : BaseEffect
    {
        [LabelText("开关(1开启2关闭)")]
        public int Open;

        [LabelText("标记位")]
        public int Flag;
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = Open.ToString();
            effectJson.Args2 = Flag.ToString();

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            Open = ToInt(effectJson.Args1);
            Flag = ToInt(effectJson.Args2);

        }
    }
}