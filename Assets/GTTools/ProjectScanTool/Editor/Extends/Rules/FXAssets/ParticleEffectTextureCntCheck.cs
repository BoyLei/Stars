/*
 * @Description: 特效总贴图数量检查
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class ParticleEffectTextureCntCheckDetail : CustomCheckDetail
    {
        [LabelText("总贴图数量阈值")]
        public int textureCntLimit = 5;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 4)]
    public class ParticleEffectTextureCntCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "特效总贴图数量检查";
            ruleDescription = "[说明]：统计特效中包含的贴图总数。该值较高时，容易导致较高的渲染相关指标。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<ParticleEffectTextureCntCheckDetail> checkDetailList = new List<ParticleEffectTextureCntCheckDetail>() { new ParticleEffectTextureCntCheckDetail() };

        [CustomScanAction]
        public void Do_ParticleEffectMemoryCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_ParticleEffectTextureCntCheck, this);
        }

    }
}
