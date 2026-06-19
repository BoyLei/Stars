/*
 * @Description: 包含无效透明通道的纹理检测
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{

    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class TextureAlphaAllOneCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "包含无效透明通道的纹理";
            ruleDescription = "[说明]：检测到alpha通道，但数值都是1，建议在导入选项时去掉Alpha is Transparency选项，避免造成内存的浪费。";
        }

        [BoxGroup("检查规则")]
        public CustomCheckDetail checkDetail = new CustomCheckDetail();

        [CustomScanAction]
        public void Do_TextureAlphaAllOneCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_TextureAlphaAllOneCheck, this);
        }
    }
}
