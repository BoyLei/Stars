/*
 * @Description: Animation资源Legacy设置检查
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class AnimationLegacyCheckDetail : CustomCheckDetail
    {
        public BoolVal legacy = new BoolVal(true);
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 10)]
    public class AnimationLegacyCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Animation资源Legacy设置检查";
            ruleDescription = "[说明]：扫描被Prefab中使用的Animation组件挂载的Animation资源的Legacy参数设置是否符合要求。此检查项提供自动修正功能，但不提供后处理功能，避免扫描大量Prefab消耗时间。";
        }


        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<AnimationLegacyCheckDetail> checkDetailList = new List<AnimationLegacyCheckDetail>() { new AnimationLegacyCheckDetail() };

        [CustomScanAction]
        public void Do_AnimationLegacyCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_AnimationLegacyCheck, this);
        }
    }
}
