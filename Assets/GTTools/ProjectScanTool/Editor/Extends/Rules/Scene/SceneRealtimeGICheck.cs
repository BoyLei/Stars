/*
 * @Description: texture开启Mipmap选项的Sprite纹理检测
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class RealtimeGICheckDetail : CustomCheckDetail
    {
        [LabelText("是否启用Realtime GI")]
        public bool realtimeLighting = false; 
    }

    [Serializable]
    [CustomScanType(EnumScanModes.场景检查, priority: 0)]
    public class SceneRealtimeGICheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "场景开启了RealtimeGI";
            ruleDescription = "[说明]：启用实时光照技术，运行时开销较大，一般不建议开启。可以根据需要自行设置。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<RealtimeGICheckDetail> checkDetailList = new List<RealtimeGICheckDetail>() { new RealtimeGICheckDetail() };

        [CustomScanAction]
        public void Do_SceneRealtimeGICheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunSceneCheck.Do_SceneRealtimeGICheck, this);
        }
    }
}
