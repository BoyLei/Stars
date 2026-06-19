/*
 * @Description: 特效总贴图内存检测
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class ParticleEffectMemoryCheckDetail : CustomCheckDetail
    {
        [LabelText("总贴图内存阈值 (建议 <1M)"), SuffixLabel("M", true)]
        public double memoryLimit = 1f;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 5)]
    public class ParticleEffectMemoryCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "特效总贴图内存检查";
            ruleDescription = "[说明]：统计每个特效中包含的贴图总内存。该值较高时，可能是纹理使用过量。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<ParticleEffectMemoryCheckDetail> checkDetailList = new List<ParticleEffectMemoryCheckDetail>() { new ParticleEffectMemoryCheckDetail() };

        [CustomScanAction]
        public void Do_ParticleEffectMemoryCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_ParticleEffectMemoryCheck, this);
        }

    }
}
