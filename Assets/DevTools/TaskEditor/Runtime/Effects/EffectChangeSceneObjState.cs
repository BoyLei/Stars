///--------------------------------------------------------------------
/// 文件名   :   EffectChangeSceneObjState.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/03/04 15:46:14
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    public class EffectChangeSceneObjState : BaseEffect
    {
        [LabelText("物件ID")]
        public int ID;

        [LabelText("物件状态")]
        public SObjStateEnum State;
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = ID.ToString();
            effectJson.Args2 = ((int)State).ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
           ID = ToInt(effectJson.Args1);
           State=(SObjStateEnum)ToInt(effectJson.Args2);
        }
    }
}