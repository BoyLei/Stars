/*
 * @Description: Texture尺寸规范扫描
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class TextureSizeCheckDetail : CustomCheckDetail
    {
        [HideInInspector] //隐藏选择，固定倍数、正方形的扫描单独拆到平台要求里面
        [ValueDropdown("kImgSizeScans"), LabelText("扫描类型")]
        public EnumImgSizeScanType scanType = EnumImgSizeScanType.eScan_w_h;
        public static IEnumerable kImgSizeScans = new ValueDropdownList<EnumImgSizeScanType>()
        {
            { "贴图普通宽高扫描", EnumImgSizeScanType.eScan_w_h },
            { "指定目录检测贴图尺寸必须是固定倍数", EnumImgSizeScanType.eScan_fixed_mul },
            { "指定目录检测贴图必须是正方形", EnumImgSizeScanType.eScan_square },
        };

        [LabelText("检查贴图Width"), MinValue(1)]
        [ShowIf("@scanType == EnumImgSizeScanType.eScan_w_h")]
        public int imageWidthLimit = 512;

        [LabelText("检查贴图Height"), MinValue(1)]
        [ShowIf("@scanType == EnumImgSizeScanType.eScan_w_h")]
        public int imageHeightLimit = 512;

        [ShowIf("@scanType == EnumImgSizeScanType.eScan_fixed_mul")]
        [LabelText("倍数要求"), MinValue(1)]
        public int sizeMul = 4;

        [LabelText("比较级")]
        [ShowIf("@scanType == EnumImgSizeScanType.eScan_w_h")]
        public EnumCompareType compareType = EnumCompareType.大于;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class TextureSizeCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图尺寸扫描";
            ruleDescription = "[说明]：按指定的规则扫描贴图，如检测宽高都大于1024的贴图等";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<TextureSizeCheckDetail> checkDetailList = new List<TextureSizeCheckDetail>() { new TextureSizeCheckDetail() };

        [CustomScanAction]
        public void Do_ArtTextureSizeCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_TextureSizeCheck, this);
        }
    }
}
