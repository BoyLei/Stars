/*
 * @Description: IOS PVRTC的贴图要求是正方形
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class IOSPVRTC_TextureSizeCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "IOS PVRTC贴图必须是正方";
            ruleDescription = "[说明]：纹理压缩格式设置中IOS PVRTC的贴图必须是长宽相等且是2的幂";
        }

        [BoxGroup("检查规则"), HideLabel]
        public CustomCheckDetail checkDetail = new CustomCheckDetail();

        [CustomScanAction]
        public void Do_IOSPVRTC_TextureSizeCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_IOSPVRTC_TextureSizeCheck, this);
        }
    }
}
