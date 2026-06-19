/*
 * @Description: Animation被多个Animator引用的检查
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{

    [Serializable, HideLabel]
    public class AnimationBeRefMulTimesCheckDetail : CustomCheckDetail
    {
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 12)]
    public class AnimationBeRefMulTimesCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Animation被多个Animator引用的检查";
            ruleDescription = "[说明]：检查某个Animation是否被多个Animator引用。";
        }


        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<AnimationBeRefMulTimesCheckDetail> checkDetailList = new List<AnimationBeRefMulTimesCheckDetail>() { new AnimationBeRefMulTimesCheckDetail() };

        [CustomScanAction]
        public void Do_AnimationBeRefMulTimesCheckCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_AnimationBeRefMulTimesCheck, this);
        }
    }
}
