///--------------------------------------------------------------------
/// 文件名   :   EffectReleaseSkill.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:24:31
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
    public class EffectReleaseSkill : BaseEffect
    {
        [LabelText("技能ID")]
        public int SkillID;

        [LabelText("技能等级")]
        public int SkillLevel;


        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = SkillID.ToString();
            effectJson.Args2 = SkillLevel.ToString();
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            SkillID = ToInt(effectJson.Args1);
            SkillLevel = ToInt(effectJson.Args2);
        }
    }
}