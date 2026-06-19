/*
 * @Description: Texture文件内存大小扫描
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class TextureStorageMemoryCheckDetail : CustomCheckDetail
    {
        [HorizontalGroup("limit"), LabelText("贴图内存大于")]
        public float storageMemoryLimit = 4;

        [HorizontalGroup("limit", Width = 20), HideLabel]
        public EnumStorageMemoryType storageMemoryType = EnumStorageMemoryType.M;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class TextureStorageMemoryCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图文件内存占用检查";
            ruleDescription = "[说明]：检查贴图文件内存占用大于一定值的资源，标准1024*1024=4M检查";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<TextureStorageMemoryCheckDetail> checkDetailList = new List<TextureStorageMemoryCheckDetail>() { new TextureStorageMemoryCheckDetail() };

        [CustomScanAction]
        public void Do_ArtTextureStorageMemoryCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_TextureStorageMemoryCheck, this);
        }
    }
}
