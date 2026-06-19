/*
 * @Description: Sprite(2d ui) 基本参数设置检测
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
    public class TextureCompressedForPlatform
    {
        [NonSerialized]
        public string deviceType;

        public IntVal maxTextureSize = new IntVal("Max Size", 2048)
        {
            dropDownMethod = "@TextureCompressedSetting.EnumTextureMaxSize",
            enable = false //默认关闭检查
        };

        public EnumVal resizeAlgorithm = new EnumVal(Convert.ToInt32(TextureResizeAlgorithm.Mitchell), typeof(TextureResizeAlgorithm))
        {
            enable = false //默认关闭检查
        };

        public IntVal format = new IntVal(Convert.ToInt32(TextureImporterFormat.Automatic));

        [ShowIf("@deviceType == DeviceTypeDefine.Default")]
        public IntVal compression = new IntVal(Convert.ToInt32(TextureImporterCompression.Compressed))
        {
            dropDownMethod = "@TextureCompressedSetting.EnumTextureImporterCompression",
            enable = false //默认关闭检查
        };

        [ShowIf("IsIosOrAndroid")]
        public EnumVal compresserQuality = new EnumVal(Convert.ToInt32(UnityEditor.TextureCompressionQuality.Normal), typeof(UnityEditor.TextureCompressionQuality))
        {
            enable = false //默认关闭检查
        };
        private bool IsIosOrAndroid()
        {
            return deviceType == DeviceTypeDefine.IOS || deviceType == DeviceTypeDefine.Android;
        }


        [ShowIf("@deviceType == DeviceTypeDefine.Android")]
        public EnumVal androidETC2FallbackOverride = new EnumVal("Override ETC2 fallback", Convert.ToInt32(AndroidETC2FallbackOverride.UseBuildSettings), typeof(AndroidETC2FallbackOverride))
        {
            enable = false //默认关闭检查
        };

        [LabelText("当是以下压缩格式时，忽略检测。表示是特殊需要设置，不进行转换设置")]
        [ValueDropdown("@ProjectScanGlobalConfig.all_formats", DropdownHeight = 200)]
        public List<int> noNeedHandle = new List<int>() { };

        //压缩格式列表
        public TextureCompressedForPlatform(string platform, TextureImporterFormat formatValue, string formatValueList)
        {
            deviceType = platform;
            format.value = Convert.ToInt32(formatValue);
            format.dropDownMethod = formatValueList;
        }

    }

    //纹理压缩设置
    [Serializable]
    public class TextureCompressedSetting
    {
        public static ValueDropdownList<int> EnumTextureMaxSize = new ValueDropdownList<int>() {
            {"32", 32},
            {"64", 64},
            {"128", 128},
            {"256", 256},
            {"512", 512},
            {"1024", 1024},
            {"2048", 2048},
            {"4096", 4096},
            {"8192", 8192}
        };

        public static ValueDropdownList<int> EnumTextureImporterCompression = new ValueDropdownList<int>() {
            {"None", Convert.ToInt32(TextureImporterCompression.Uncompressed)},
            {"Low Quality", Convert.ToInt32(TextureImporterCompression.CompressedLQ)},
            {"Normal Quality", Convert.ToInt32(TextureImporterCompression.Compressed)},
            {"High Quality", Convert.ToInt32(TextureImporterCompression.CompressedHQ)},
        };

        [TabGroup("Defalut")]
        public TextureCompressedForPlatform default_fmt = new TextureCompressedForPlatform(DeviceTypeDefine.Default, TextureImporterFormat.Automatic, "@ProjectScanGlobalConfig.default_formats");

        [TabGroup("PC,Mac & Linux Standalone")]
        public TextureCompressedForPlatform win_fmt = new TextureCompressedForPlatform(DeviceTypeDefine.Win, TextureImporterFormat.DXT5, "@ProjectScanGlobalConfig.win_formats");

        [TabGroup("iOS")]
#if UNITY_2018
        public TextureCompressedForPlatform iOS_fmt = new TextureCompressedForPlatform(DeviceTypeDefine.IOS, TextureImporterFormat.ASTC_RGBA_5x5, "@ProjectScanGlobalConfig.iOS_formats");
#else
        public TextureCompressedForPlatform iOS_fmt = new TextureCompressedForPlatform(DeviceTypeDefine.IOS, TextureImporterFormat.ASTC_5x5, "@ProjectScanGlobalConfig.iOS_formats");
#endif
        [TabGroup("Android")]
        public TextureCompressedForPlatform android_fmt = new TextureCompressedForPlatform(DeviceTypeDefine.Android, TextureImporterFormat.ETC2_RGBA8, "@ProjectScanGlobalConfig.android_formats");
    }

    [Serializable, HideLabel]
    public class SpriteUIFormatDetail : CustomCheckDetail
    {
        [InfoBox("指定目录必须都是Sprite格式,如果不是给出警告或修正。如果未勾选，则当贴图格式不是Sprite时跳过检查。所以当我们需要后处理时必须开启当前项")]
        public BoolVal mustbeSprite = new BoolVal("指定目录必须都是Sprite格式", true);

        public EnumVal spriteMode = new EnumVal(Convert.ToInt32(SpriteImportMode.Single), typeof(SpriteImportMode))
        {
            enable = false
        };

        [Indent]
        [ShowIf("@spriteMode.value != 0")]
        public IntVal pixelsPerUnit = new IntVal(100)
        {
            enable = false
        };

        [Indent]
        [ShowIf("@spriteMode.value != 0")]
        public UIntVal spriteExtrude = new UIntVal("spriteExtrude限制必须大于等于", 2, 0, 32);

        [Indent]
        [ShowIf("@spriteMode.value == 1")]
        public EnumVal spriteAlignment = new EnumVal("Pivot", Convert.ToInt32(SpriteAlignment.Center), typeof(SpriteAlignment)){
            enable = false
        };

        [Indent, LabelText("")]
        [ShowIf("SpritePivotIsVisible")]
        public Vector2 spritePivot = new Vector2(0.5f, 0.5f);
        private bool SpritePivotIsVisible()
        {
            return spriteMode.value == Convert.ToInt32(SpriteImportMode.Single) 
            && spriteAlignment.enable
            && spriteAlignment.value == Convert.ToInt32(SpriteAlignment.Custom);
        } 

        [Indent]
        [ShowIf("@spriteMode.value == 1 || spriteMode.value == 3")]
        public BoolVal generatePhysicsShape = new BoolVal(false);

        [TitleGroup("Advanced", HorizontalLine = false), Indent]
        public BoolVal sRGBTexture = new BoolVal("sRGB (Color Texture)", false);

        [TitleGroup("Advanced", HorizontalLine = false), Indent]
        public EnumVal alphaSource = new EnumVal(Convert.ToInt32(TextureImporterAlphaSource.FromInput), typeof(TextureImporterAlphaSource));

        [TitleGroup("Advanced", HorizontalLine = false), Indent]
        public BoolVal alphaIsTransparency = new BoolVal(true);

        [TitleGroup("Advanced", HorizontalLine = false), Indent]
        public BoolVal isReadable = new BoolVal("Read/Write Enabled", false);

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

    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查, priority: 1)]
    public class SpriteUIFormatCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Sprite-UI基本参数设置";
            ruleDescription = "[说明]：Sprite-UI基本的一些参数设置检测，如sRGBTexture、isReadable是否关闭，Ios和Android的材质压缩参数设置等。可能和其他Texture的设置功能重复，请按需设置或关闭。此检查项提供自动修正和后处理功能。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<SpriteUIFormatDetail> checkDetailList = new List<SpriteUIFormatDetail>() { new SpriteUIFormatDetail() };

        /// <summary>
        /// assetPostprocessorPath 后处理文件路径
        /// </summary>
        [CustomScanAction]
        public void Do_SpriteUIFormatCheck(string[] assetPostprocessorPath = null)
        {
            //添加检测逻辑
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_SpriteUIFormatCheck, this, assetPostprocessorPath);
        }
    }
}
