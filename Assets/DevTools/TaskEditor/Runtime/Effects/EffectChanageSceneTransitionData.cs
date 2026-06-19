///--------------------------------------------------------------------
/// 文件名   :   EffectPlayPv.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/12 09:55:52
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
    public class EffectChanageSceneTransitionData : BaseEffect
    {
        [LabelText("过渡类型")]
        public int TransitionType;

        [LabelText("地图基础信息id")]
        public int SceneBaseDataID;

        [LabelText("PV表ID")]
        public int PVCfgID;

        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = TransitionType.ToString();
            effectJson.Args2 = SceneBaseDataID.ToString();
            effectJson.Args3 = PVCfgID.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            TransitionType = ToInt(effectJson.Args1);
            SceneBaseDataID = ToInt(effectJson.Args2);
            PVCfgID = ToInt(effectJson.Args3);
        }
    }
}
