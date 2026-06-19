/*
 * @Description: 粒子系统开启了预热
 * @Description: 粒子系统开启了预热且LifeTime值过大
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class ParticlePreWarmCheckDetail : CustomCheckDetail
    {
        [LabelText("是否需要开启预热")]
        public bool preWarm = false;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 1)]
    public class ParticlePreWarmCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "粒子系统是否开启预热检查";
            ruleDescription = "[说明]：粒子系统的Prewarn操作会在使用时的第一帧中造成相对集中的CPU耗时，很可能会造成运行时的局部卡顿，建议考虑是否确实需要开启该选项。项目可以根据不同需要来设置是否开启。此检查项提供自动修正和后处理功能。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<ParticlePreWarmCheckDetail> checkDetailList = new List<ParticlePreWarmCheckDetail>() { new ParticlePreWarmCheckDetail() };

        [CustomScanAction]
        public void Do_ArtParticlePreWarmCheck(string[] assetPostprocessorPath = null)
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_ArtParticlePreWarmCheck, this, assetPostprocessorPath);
        }
    }
}
