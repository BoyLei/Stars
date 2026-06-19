/*
 * @Description: 特效误引用图集
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class AnimatorAssetCheckDetail : CustomCheckDetail
    {
        [InfoBox("检查Animator的transition中使用的条件是否合法，比如使用的变量已经被删除就不合法")]
        [LabelText("transition中使用的条件合法性检查")]
        public bool transitionConditionCheck = false;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 9)]
    public class AnimatorAssetCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Animator资源检查";
            ruleDescription = "[说明]：检查Animator的一些基本使用规范，如transition中使用的条件是否合法等。";
        }


        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<AnimatorAssetCheckDetail> checkDetailList = new List<AnimatorAssetCheckDetail>() { new AnimatorAssetCheckDetail() };

        [CustomScanAction]
        public void Do_AnimatorAssetCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_AnimatorAssetCheck, this);
        }
    }
}
