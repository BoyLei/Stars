/*
 * @Description: 音频资源一些基本属性设置要求
 */
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    // [HideLabel]
    [Serializable]
    public class AudioFileSizeCheckDetail : CustomCheckDetail
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

        [HorizontalGroup("limit"), LabelText("音频文件大于")]
        public float fileSizeLimit = 512;

        [HorizontalGroup("limit", Width = 20), HideLabel]
        public EnumStorageMemoryType storageMemoryType = EnumStorageMemoryType.KB;

    }

    [Serializable]
    [CustomScanType(EnumScanModes.音频资源检查, priority: 1)]
    public class AudioFileSizeCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "音频文件大小检查";
            ruleDescription = "[说明]：统计较大的音频文件。";
        }

        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<AudioFileSizeCheckDetail> checkDetailList = new List<AudioFileSizeCheckDetail>() { new AudioFileSizeCheckDetail() };


        [CustomScanAction]
        public void Do_AudioFileSizeCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunAudioAssetsCheck.Do_AudioFileSizeCheck, this);
        }
    }
}
