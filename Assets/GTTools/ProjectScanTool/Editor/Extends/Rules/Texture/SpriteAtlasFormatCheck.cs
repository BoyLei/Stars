/*
 * @Description: 图集检查
 */
using System;
using System.Collections;
using System.Collections.Generic;
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class SpriteAtlasFormatDetail : CustomCheckDetail
    {

        public IntVal atlasType = new IntVal("Type", 0)
        {
            dropDownMethod = "@SpriteAtlasFormatCheck.kAtlasTypes"
        };

        [ShowIf("@atlasType.value == 1")]
        [InfoBox("当MasterAtlas引用丢失时给出警告")]
        public BoolVal masterAtlasCheck = new BoolVal("MasterAtlas为空检查", false)
        {
            enable = false
        };

        public BoolVal includeInBuild = new BoolVal(true);

        [ShowIf("@atlasType.value == 0")]
        public BoolVal enableRotation = new BoolVal("AllowRotation", true);

        [ShowIf("@atlasType.value == 0")]
        public BoolVal enableTightPacking = new BoolVal("Tight Packing", false);

        [ShowIf("@atlasType.value == 0")]
        public IntVal padding = new IntVal(4)
        {
            dropDownMethod = "@SpriteAtlasFormatCheck.kAtlasPaddings"
        };

        [ShowIf("@atlasType.value == 1")]
        [InfoBox("Scale的值必须介于0-1之间", InfoMessageType.Error, "CheckVariantMultiplier")]
        public FloatVal variantMultiplier = new FloatVal("Scale", 1f);
        private bool CheckVariantMultiplier()
        {
            return !(variantMultiplier.value > 0 && variantMultiplier.value <= 1);
        }

        [TitleGroup("Texture", HorizontalLine = false)]
        public BoolVal readable = new BoolVal("Read/Write Enabled", false);

        [TitleGroup("Texture")]
        public BoolVal generateMipMaps = new BoolVal(false);

        [TitleGroup("Texture")]
        public BoolVal sRGBTexture = new BoolVal("sRGB", true);

        [TitleGroup("Texture")]
        public EnumVal filterMode = new EnumVal(Convert.ToInt32(FilterMode.Bilinear), typeof(FilterMode))
        {
            enable = false
        };

        [HideLabel]
        public TextureCompressedSetting textureFormat = new TextureCompressedSetting();

        [ShowIf("@atlasType.value == 0")]
        [InfoBox("当Objects for Packing引用丢失时给出警告")]
        public BoolVal packObjectsCheck = new BoolVal("Objects for Packing为空检查", false)
        {
            enable = false
        };
    }

    [Serializable]
    [CustomScanType(EnumScanModes.贴图资源检查, priority: 2)]
    public class SpriteAtlasFormatCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "SpriteAtlas-图集基本参数设置";
            ruleDescription = "[说明]：SpriteAtlas基本的一些参数设置检测。此检查项提供自动修正和后处理功能。";
        }

        public static ValueDropdownList<int> kAtlasTypes = new ValueDropdownList<int>()
        {
            { "Master", 0 },
            { "Variant", 1 },
        };

        public static IEnumerable kAtlasPaddings = new ValueDropdownList<int>()
        {
            { "2", 2 },
            { "4", 4 },
            { "8", 8 },
        };

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<SpriteAtlasFormatDetail> checkDetailList = new List<SpriteAtlasFormatDetail>() { new SpriteAtlasFormatDetail() };

        /// <summary>
        /// assetPostprocessorPath 后处理文件路径
        /// </summary>
        [CustomScanAction]
        public void Do_SpriteAtlasFormatCheck(string[] assetPostprocessorPath = null)
        {
            //添加检测逻辑
            ProjectScanHelper.DoCustomRuleCheck(RunTextureAssetsCheck.Do_SpriteAtlasFormatCheck, this, assetPostprocessorPath);
        }
    }
}
