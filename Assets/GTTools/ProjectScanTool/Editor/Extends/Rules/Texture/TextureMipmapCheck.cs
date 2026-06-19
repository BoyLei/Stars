/*
 * @Description: texture开启Mipmap选项的Sprite纹理检测
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{

    [Serializable]
    // [CustomScanType(EnumScanModes.贴图资源检查)]
    public class TextureMipmapCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "开启Mipmap选项的Sprite纹理";
            ruleDescription = "[说明]：Mipmap开启后，内存回事未开启Mipmap的1.33倍，因为Mipmap会生成一组长宽依次减少一倍的纹理序列，一直生成到1*1。Mipmap提升GPU效率，一般用于3D场景或角色，UI不建议开启。此检查项提供自动修正和后处理功能。";
            enable = false;
        }

        [BoxGroup("检查规则")]
        public CustomCheckDetail checkDetail = new CustomCheckDetail();

        [CustomScanAction]
        public void Do_TextureMipmapCheck(string[] assetPostprocessorPath = null)
        {
            //添加检测逻辑
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_TextureMipmapCheck, this, assetPostprocessorPath);
        }
    }
}
