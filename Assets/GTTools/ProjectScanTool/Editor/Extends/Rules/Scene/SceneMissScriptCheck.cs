/*
 * @Description: 检查手动删除脚本之后出现的脚本丢失问题
 */

using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    [CustomScanType(EnumScanModes.场景检查, priority: 1)]
    public class SceneMissScriptCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "场景Missing Script检查";
            ruleDescription = "[说明]：手动删除一些脚本后，某些场景可能引用了删除的脚本没有进行Remove";
        }

        [BoxGroup("检查规则")]
        public CustomCheckDetail checkDetail = new CustomCheckDetail();

        [CustomScanAction]
        public void Do_SceneMissScriptCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunSceneCheck.Do_SceneMissScriptCheck, this);
        }

    }
}
