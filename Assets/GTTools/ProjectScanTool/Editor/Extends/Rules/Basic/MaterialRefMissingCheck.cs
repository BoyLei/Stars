/*
 * @Description: 材质纹理引用丢失检查
 */

using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    [CustomScanType(EnumScanModes.基本资源检查, priority: 5)]
    public class MaterialRefMissingCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "材质纹理数据异常检查";
            ruleDescription = "[说明]：在进行资源删除等操作时可能出现材质纹理数据的丢失，导致打包失败或运行时出错。具体表现在序列化文件中Texture仍旧记录着fileID，但是guid已经为0";
        }

        [BoxGroup("检查规则")]
        public CustomCheckDetail checkDetail = new CustomCheckDetail();

        [CustomScanAction]
        public void Do_MaterialRefMissingCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunBasicAssetsCheck.Do_MaterialRefMissingCheck, this);
        }

    }
}
