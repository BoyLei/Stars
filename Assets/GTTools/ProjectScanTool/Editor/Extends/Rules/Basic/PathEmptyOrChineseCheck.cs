/*
 * @Description: 资源名称是否含有中文或空格
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class PathEmptyOrChineseCheckDetail : CustomCheckDetail
    {
        [LabelText("是否检查资源内部节点")]
        public bool checkChild = true;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.基本资源检查, priority: 0)]
    public class PathEmptyOrChineseCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "资源名称是否含有中文或空格";
            ruleDescription = "[说明]：资源包含中文或空格可能导致加载AB时报空的情况";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<PathEmptyOrChineseCheckDetail> checkDetailList = new List<PathEmptyOrChineseCheckDetail>() { new PathEmptyOrChineseCheckDetail() };

        [CustomScanAction]
        public void Do_PathEmptyOrChineseCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunBasicAssetsCheck.Do_PathEmptyOrChineseCheck, this);
        }

    }
}
