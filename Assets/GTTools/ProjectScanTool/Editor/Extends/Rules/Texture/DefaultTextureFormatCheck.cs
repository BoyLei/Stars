/*
 * @Description: Defalut-Texture基本参数设置检测
 */

using System;
using System.Collections.Generic;
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class DefaultTextureFormatDetail
    {
        [InfoBox("指定目录必须都是Default格式,如果不是给出警告或修正。如果未勾选，则当贴图格式不是Default时跳过检查。所以当我们需要后处理时必须开启当前项")]
        public BoolVal mustbeDefault = new BoolVal("指定目录必须都是Default Texture格式", false)
        {
            enable = false
        };

        public BoolVal sRGBTexture = new BoolVal("sRGB (Color Texture)", false);

        public EnumVal alphaSource = new EnumVal(Convert.ToInt32(TextureImporterAlphaSource.FromInput), typeof(TextureImporterAlphaSource));

        public BoolVal alphaIsTransparency = new BoolVal(true);

        [TitleGroup("Advanced", HorizontalLine = false), Indent]
        public EnumVal npotScale = new EnumVal("Non-Power of 2", Convert.ToInt32(TextureImporterNPOTScale.ToNearest), typeof(TextureImporterNPOTScale));

        [TitleGroup("Advanced", HorizontalLine = false), Indent]
        public BoolVal isReadable = new BoolVal("Read/Write Enabled", false);

        [TitleGroup("Advanced", HorizontalLine = false), Indent]
        public BoolVal streamingMipmaps = new BoolVal(false) { enable = false };

        [TitleGroup("Advanced", HorizontalLine = false), Indent]
        public BoolVal mipmapEnabled = new BoolVal("Generate Mip Maps", false);

        public EnumVal wrapMode = new EnumVal(Convert.ToInt32(TextureWrapMode.Clamp), typeof(TextureWrapMode))
        {
            enable = false
        };

        public EnumVal filterMode = new EnumVal(Convert.ToInt32(FilterMode.Bilinear), typeof(FilterMode))
        {
            enable = false
        };

        [HideLabel]
        public TextureCompressedSetting textureFormat = new TextureCompressedSetting();

    }

    [Serializable, HideLabel]
    public class DefaultTextureFormatCheckDetail : CustomCheckDetail
    {
        public DefaultTextureFormatDetail texCheck = new DefaultTextureFormatDetail();
    }

    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查, priority: 0)]
    public class DefaultTextureFormatCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Defalut-Texture基本参数设置";
            ruleDescription = "[说明]：Defalut-Texture基本的一些参数设置检测，如sRGBTexture、isReadable是否关闭，Ios和Android的材质压缩参数设置等。可能和其他Texture的设置功能重复，请按需设置或关闭。此检查项提供自动修正和后处理功能。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<DefaultTextureFormatCheckDetail> checkDetailList = new List<DefaultTextureFormatCheckDetail>() { new DefaultTextureFormatCheckDetail() };

        [CustomScanAction]
        public void Do_DefaultTextureFormatCheck(string[] assetPostprocessorPath = null)
        {
            //添加检测逻辑
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_DefaultTextureFormatCheck, this, assetPostprocessorPath);
        }
    }
}
