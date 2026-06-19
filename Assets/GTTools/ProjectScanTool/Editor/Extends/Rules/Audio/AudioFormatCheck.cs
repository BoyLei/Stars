/*
 * @Description: 音频资源一些基本属性设置要求
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
    //源码参考 https://github.com/Unity-Technologies/UnityCsReference/blob/667dd8bc523315986521ee31df35cb9e584049b9/Modules/AssetPipelineEditor/ImportSettings/AudioImporterInspector.cs
    // [HideLabel]
    [Serializable]
    public class AudioFormatDetail : CustomCheckDetail
    {
        [InfoBox("必须指定命名规范要求，用于区分音乐还是音效。没有指定将不会进行检测。", InfoMessageType.Error, "AudioNameIsEmpty")]
        public StringVal audioNameReq = new StringVal("命名规范要求", "MUS_");
        private bool AudioNameIsEmpty()
        {
            if (!audioNameReq.enable)
                return false;
            return string.IsNullOrEmpty(audioNameReq.value) || string.IsNullOrEmpty(audioNameReq.value.Trim());
        }

        [LabelText("文件命名匹配方式"), Indent, ShowIf("@audioNameReq.enable")]
        public EnumAudioNameMatchRule audioNameMatch = EnumAudioNameMatchRule.前缀匹配;

        //单声道true
        public BoolVal forceToMono = new BoolVal(true);

        //优化声道true
        [Indent, ShowIf("ForceToMonoIsEnable")]
        public BoolVal normalize = new BoolVal(false);
        private bool ForceToMonoIsEnable()
        {
            return forceToMono.enable && forceToMono.value;
        }

        //加载音频资源的方法
        [BoxGroup("Default"), EnumPaging]
        public EnumVal loadType = new EnumVal(Convert.ToInt32(AudioClipLoadType.Streaming), typeof(AudioClipLoadType));

        [NonSerialized]
        public static IEnumerable kCompressionFormats = new ValueDropdownList<int>()
        {
            { "PCM", Convert.ToInt32(AudioCompressionFormat.PCM) },
            { "Vorbis", Convert.ToInt32(AudioCompressionFormat.Vorbis) },
            { "ADPCM", Convert.ToInt32(AudioCompressionFormat.ADPCM) },
        };

        //压缩状态
        [BoxGroup("Default")]
        public IntVal compressionFormat = new IntVal(Convert.ToInt32(AudioCompressionFormat.Vorbis))
        {
            dropDownMethod = "@AudioFormatDetail.kCompressionFormats"
        };

        [BoxGroup("Default"), SuffixLabel("%", true)]
        public IntVal quality = new IntVal(70, 1, 100);

        //PCM 和 ADPCM 压缩格式允许自动优化或手动降低采样率
        [BoxGroup("Default")]
        public EnumVal sampleRateSetting = new EnumVal(Convert.ToInt32(AudioSampleRateSetting.OverrideSampleRate), typeof(AudioSampleRateSetting));

        [NonSerialized]
        public static IEnumerable kSampleRateValues = new ValueDropdownList<uint>()
        {
            { "8,000 Hz", 8000 },
            { "11,025 Hz", 11025 },
            { "22,050 Hz", 22050 },
            { "44,100 Hz", 44100 },
            { "48,000 Hz", 48000 },
            { "96,000 Hz", 96000 },
            { "192,000 Hz", 192000 },
        };

        [BoxGroup("Default"), ShowIf("OverrideSampleRate")]
        public UIntVal sampleRateOverride = new UIntVal(22050)
        {
            dropDownMethod = "@AudioFormatDetail.kSampleRateValues"
        };
        private bool OverrideSampleRate()
        {
            return sampleRateSetting.enable && sampleRateSetting.value == Convert.ToInt32(AudioSampleRateSetting.OverrideSampleRate);
        }
    }

    [Serializable]
    [CustomScanType(EnumScanModes.音频资源检查, priority: 0)]
    public class AudioFormatCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "音频资源基本参数设置检查";
            ruleDescription = "[说明]：主要针对音乐和音效有不同的设置需求。这也就意味着项目中对音乐音效需要有不同的命名规范需求，比如音乐统一命名前缀MUS_，音效统一命名前缀SFX_，两个参数下面会开放配置，默认填充的命名各项目按需求更改。此检查项提供自动修正和后处理功能。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<AudioFormatDetail> checkDetailList = new List<AudioFormatDetail>() { new AudioFormatDetail() };


        [CustomScanAction]
        public void Do_AudioFormatCheck(string[] assetPostprocessorPath = null)
        {
            ProjectScanHelper.DoCustomRuleCheck(RunAudioAssetsCheck.Do_AudioFormatCheck, this, assetPostprocessorPath);
        }
    }
}
