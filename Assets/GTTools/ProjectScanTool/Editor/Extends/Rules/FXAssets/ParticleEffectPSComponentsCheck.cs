/*
 * @Description: 特效包含的ParticleSystem组件数量检查
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class ParticleEffectPSComponentsCheckDetail : CustomCheckDetail
    {
        [LabelText("粒子系统组件总数阈值")]
        public double psComponentCntLimit = 5;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 6)]
    public class ParticleEffectPSComponentsCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "特效包含的ParticleSystem组件数量检查";
            ruleDescription = "[说明]：统计特效中包含的ParticleSystem组件数。该值较高时，容易导致较高的渲染相关指标，以及较高的序列化耗时等。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<ParticleEffectPSComponentsCheckDetail> checkDetailList = new List<ParticleEffectPSComponentsCheckDetail>() { new ParticleEffectPSComponentsCheckDetail() };

        [CustomScanAction]
        public void Do_ParticleEffectPSComponentsCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_ParticleEffectPSComponentsCheck, this);
        }

    }
}
