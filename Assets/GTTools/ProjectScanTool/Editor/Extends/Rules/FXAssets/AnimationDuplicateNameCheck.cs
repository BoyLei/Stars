/*
 * @Description: 同一目录下Animator与Animation同名的检查
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{

    [Serializable, HideLabel]
    public class AnimationDuplicateNameCheckDetail : CustomCheckDetail
    {
        [LabelText("是否同时检查Animation被多个Animator引用")]
        public bool mulRefCheck = false;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 11)]
    public class AnimationDuplicateNameCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "同一目录下Animation与Animator同名的检查";
            ruleDescription = "[说明]：同一个目录下，Animation和Animator文件同名检查。\n提供联合检查————同名且这个Animation被多个Animator引用时，该Animation会被单独打AB，最终Animation的AB会覆盖Animator的AB。";
        }


        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<AnimationDuplicateNameCheckDetail> checkDetailList = new List<AnimationDuplicateNameCheckDetail>() { new AnimationDuplicateNameCheckDetail() };

        [CustomScanAction]
        public void Do_AnimationDuplicateNameCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_AnimationDuplicateNameCheck, this);
        }
    }
}
