/*
 * @Description: 粒子系统开启了预热且LifeTime值过大
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class ParticleLifeTimeCheckDetail : CustomCheckDetail
    {
        [LabelText("LifeTime阈值")]
        public float lifeTimeLimit = 60f;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 2)]
    public class ParticleLifeTimeCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "粒子系统开启了预热且LifeTime值过大";
            ruleDescription = "[说明]：当粒子特效开启PreWarm且LifeTime值过大时，在运行时对性能有所影响，建议开启PreWarn的粒子特效LifeTime值设定在60以内";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<ParticleLifeTimeCheckDetail> checkDetailList = new List<ParticleLifeTimeCheckDetail>() { new ParticleLifeTimeCheckDetail() };

        [CustomScanAction]
        public void Do_ArtParticleLifeTimeCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_ArtParticleLifeTimeCheck, this);
        }
    }
}
