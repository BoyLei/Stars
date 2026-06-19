/*
 * @Description: 贴图的透明像数占比过高检测
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{

    [Serializable, HideLabel]
    public class TransparentPixelsCheckDetail : CustomCheckDetail
    {
        [LabelText("alpha为0占比阈值")]
        [PropertyRange(0, 100), SuffixLabel("%", true)]
        public float alphaPixRange = 50;

        [ToggleGroup(toggleMemberName: "limitImgSize", ToggleGroupTitle = "限定只检查大于一定宽高的贴图")]
        public bool limitImgSize = false;

        [ToggleGroup("limitImgSize"), LabelText("贴图Width >=")]
        public uint widthLimit = 256;

        [ToggleGroup("limitImgSize"), LabelText("贴图Height >=")]
        public uint heightLimit = 256;
    }

    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class TextureTooManyTransparentPixels : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图的透明像数占比过高";
            ruleDescription = "[说明]：贴图中alpha为0的区域占比过大，建议减少贴图大小以充分运用贴图区域。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<TransparentPixelsCheckDetail> checkDetailList = new List<TransparentPixelsCheckDetail>() { new TransparentPixelsCheckDetail() };

        [CustomScanAction]
        public void Do_TextureTooManyTransparentPixels()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_TextureTooManyTransparentPixels, this);
        }
    }
}
