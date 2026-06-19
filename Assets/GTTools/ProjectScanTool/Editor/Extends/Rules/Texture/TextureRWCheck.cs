/*
 * @Description: 开启read/write选项的纹理检测
 */

using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    // [CustomScanType(EnumScanModes.贴图资源检查)]
    public class TextureRWCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "开启Read&Write选项的纹理";
            ruleDescription = "[说明]：Read/Write选项起用后，将会允许从脚本来访问纹理数据，所以在系统内存中会保留纹理数据的副本，占用额外内存，等同于一个纹理数据会有双倍的内存消耗。此检查项提供自动修正和后处理功能。";
            enable = false;
        }

        [BoxGroup("检查规则")]
        public CustomCheckDetail checkDetail = new CustomCheckDetail();

        [CustomScanAction]
        public void Do_TextureRWCheck(string[] assetPostprocessorPath = null)
        {
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_TextureRWCheck, this, assetPostprocessorPath);
        }
    }
}
