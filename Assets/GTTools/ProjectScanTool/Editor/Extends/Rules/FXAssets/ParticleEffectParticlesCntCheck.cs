/*
 * @Description: 特效播放时最大粒子数量检查
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class ParticleEffectParticlesCntCheckDetail : CustomCheckDetail
    {
        [LabelText("单帧粒子数总和阈值 (建议 <50)")]
        public int ParticlesCntLimit = 50;
    }

    [Serializable]
    // [CustomScanType(EnumScanModes.动效资源检查, priority: 3)]
    public class ParticleEffectParticlesCntCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "特效播放时最大粒子数量检查";
            ruleDescription = "[说明]：统计特效每帧的粒子数总和，并取过程中最高的值。该值越大，特效的更新开销可能也越大。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<ParticleEffectParticlesCntCheckDetail> checkDetailList = new List<ParticleEffectParticlesCntCheckDetail>() { new ParticleEffectParticlesCntCheckDetail() };

        [CustomScanAction]
        public void Do_ParticleEffectParticlesCntCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_ParticleEffectParticlesCntCheck, this);
        }

    }
}
