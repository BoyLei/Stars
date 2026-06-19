/*
 * @Description: UI子节点数量检测
 * @Description: 检查手动删除脚本之后出现的脚本丢失问题
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class UIChildsCountCheckDetail : CustomCheckDetail
    {
        [LabelText("子节点数阈值")]
        public int numChildren = 50;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.基本资源检查, priority: 3)]
    public class UIChildsCountCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "UI子节点数量检查";
            ruleDescription = "[说明]：UI节点太多，会导致序列化数据重建耗时会比较高。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<UIChildsCountCheckDetail> checkDetailList = new List<UIChildsCountCheckDetail>() { new UIChildsCountCheckDetail() };

        [CustomScanAction]
        public void Do_UIChildsCountCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunBasicAssetsCheck.Do_UIChildsCountCheck, this);
        }

    }
}
