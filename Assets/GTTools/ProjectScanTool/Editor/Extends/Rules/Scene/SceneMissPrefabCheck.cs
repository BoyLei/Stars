/*
 * @Description: 检查手动删除prefab之后，嵌套的prefab出现missing prefab的问题
 */

using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    [CustomScanType(EnumScanModes.场景检查, priority: 2)]
    public class SceneMissPrefabCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "场景Missing Prefab检查";
            ruleDescription = "[说明]：unity手动删除prefab后，对嵌套了被删除prefab的prefab进行打包Assetbundle可能出现Crash";
        }

        [BoxGroup("检查规则")]
        public CustomCheckDetail checkDetail = new CustomCheckDetail();

        [CustomScanAction]
        public void Do_SceneMissPrefabCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunSceneCheck.Do_SceneMissPrefabCheck, this);
        }

    }
}
