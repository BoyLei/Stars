/*
 * @Description: AndroidETC2必须要求贴图尺寸是4的倍数
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
    public class AndroidETC2_TextureSizeCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Android ETC2贴图尺寸必须是4的倍数";
            ruleDescription = "[说明]：纹理压缩格式设置中Android ETC2的贴图尺寸必须是4的倍数";
        }

        [BoxGroup("检查规则"), HideLabel]
        public CustomCheckDetail checkDetail = new CustomCheckDetail();

        [CustomScanAction]
        public void Do_AndroidETC2_TextureSizeCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_AndroidETC2_TextureSizeCheck, this);
        }
    }
}
