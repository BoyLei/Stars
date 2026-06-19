/*
 * @Description: Texture本地磁盘文件大小扫描
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class TextureStorageSizeCheckDetail : CustomCheckDetail
    {
        [HorizontalGroup("limit"), LabelText("贴图文件大于")]
        public float storageSizeLimit = 1;

        [HorizontalGroup("limit", Width = 20), HideLabel]
        public EnumStorageMemoryType storageMemoryType = EnumStorageMemoryType.M;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class TextureStorageSizeCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图文件大小检查";
            ruleDescription = "[说明]：检查贴图本地磁盘文件大小超过一定值的资源";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<TextureStorageSizeCheckDetail> checkDetailList = new List<TextureStorageSizeCheckDetail>() { new TextureStorageSizeCheckDetail() };

        [CustomScanAction]
        public void Do_ArtTextureStorageSizeCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_TextureStorageSizeCheck, this);
        }
    }
}
