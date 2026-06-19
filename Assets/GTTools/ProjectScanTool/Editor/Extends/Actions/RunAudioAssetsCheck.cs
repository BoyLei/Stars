/*
 * @Description:音频资源检查 
 */
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using System.IO;
using GameTechTools.CommonLibs.CommonExtends;

namespace CasualEngine.ProjectScanTool
{
    public class RunAudioAssetsCheck : MethodHelper
    {
        #region 音频资源一些基本属性设置要求
        public static bool Do_AudioFormatCheck(AudioFormatCheck customRule, string[] assetPostprocessorPath = null)
        {
            foreach (var checkDetail in customRule.checkDetailList)
            {
                if (!checkDetail.audioNameReq.enable)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(checkDetail.audioNameReq.value) || string.IsNullOrEmpty(checkDetail.audioNameReq.value.Trim()))
                {
                    Debug.LogErrorFormat("{0}----检查规则中命名规范不能为空", customRule.ruleTitle);
                    return false;
                }
            }

            foreach (var checkDetail in customRule.checkDetailList)
            {
                string methodName = MethodBase.GetCurrentMethod().Name;
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.audioclip, assetPostprocessorPath);
                foreach (var path in paths)
                {
                    var audioName = Path.GetFileNameWithoutExtension(path);
                    //根据名称匹配类型
                    if (!checkDetail.audioNameReq.enable
                        || (checkDetail.audioNameMatch == EnumAudioNameMatchRule.前缀匹配 && audioName.StartsWith(checkDetail.audioNameReq.value.Trim()))
                        || (checkDetail.audioNameMatch == EnumAudioNameMatchRule.后缀匹配 && audioName.EndsWith(checkDetail.audioNameReq.value.Trim()))
                        || (checkDetail.audioNameMatch == EnumAudioNameMatchRule.包含即可 && audioName.IndexOf(checkDetail.audioNameReq.value.Trim()) > -1))
                    {
                        //音频文件
                        AudioClip audio = AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;
                        if (audio == null)
                            continue;

                        //获得音频文件信息
                        AudioImporter audioImport = AssetImporter.GetAtPath(path) as AudioImporter;
                        if (audioImport == null)
                            continue;

                        AudioImporterSampleSettings settings = audioImport.defaultSampleSettings;

                        if (checkDetail.forceToMono.enable)
                            Comparator.CompareForObject(methodName, audio, audioImport, GetVarName(() => audioImport.forceToMono), checkDetail.forceToMono.value, customRule);
                        //normalize属性不直接开放，走序列化设置，并且是m_Normalize
                        SerializedObject ob = new SerializedObject(audioImport);
                        SerializedProperty normalize = ob.FindProperty("m_Normalize");
                        if (checkDetail.normalize.enable)
                            Comparator.CompareForSerializedProp(methodName, audio, normalize, GetVarName(() => normalize.boolValue), checkDetail.normalize.value, customRule);

                        if (checkDetail.loadType.enable)
                            Comparator.CompareForStruct(methodName, audio, ref settings, GetVarName(() => settings.loadType), checkDetail.loadType.value, customRule);

                        if (checkDetail.compressionFormat.enable)
                            Comparator.CompareForStruct(methodName, audio, ref settings, GetVarName(() => settings.compressionFormat), checkDetail.compressionFormat.value, customRule);

                        if (checkDetail.quality.enable)
                            Comparator.CompareForStruct(methodName, audio, ref settings, GetVarName(() => settings.quality), 1.0f * checkDetail.quality.value / 100, customRule);

                        if (checkDetail.sampleRateSetting.enable)
                            Comparator.CompareForStruct(methodName, audio, ref settings, GetVarName(() => settings.sampleRateSetting), checkDetail.sampleRateSetting.value, customRule);

                        if (checkDetail.sampleRateOverride.enable)
                            Comparator.CompareForStruct(methodName, audio, ref settings, GetVarName(() => settings.sampleRateOverride), checkDetail.sampleRateOverride.value, customRule);

                        if (customRule.AssetsModifyed)
                        {
                            //不知道为什么，settings用的是引用，数据不会回写，需要重新设置下defalut.
                            audioImport.defaultSampleSettings = settings;
                            //应用属性修改
                            ob.ApplyModifiedProperties();
                            audioImport.SaveAndReimport();
                            //audioImport一定要标记dirty
                            EditorUtility.SetDirty(audioImport);
                        }
                    }
                }
            }

            return true;
        }
        #endregion

        #region 音频文件大小统计
        public static bool Do_AudioFileSizeCheck(AudioFileSizeCheck customRule)
        {
            foreach (var checkDetail in customRule.checkDetailList)
            {
                if (!checkDetail.audioNameReq.enable)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(checkDetail.audioNameReq.value) || string.IsNullOrEmpty(checkDetail.audioNameReq.value.Trim()))
                {
                    Debug.LogErrorFormat("{0}----检查规则中命名规范不能为空", customRule.ruleTitle);
                    return false;
                }
            }

            foreach (var checkDetail in customRule.checkDetailList)
            {
                string methodName = MethodBase.GetCurrentMethod().Name;
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.audioclip);
                foreach (var path in paths)
                {
                    var audioName = Path.GetFileNameWithoutExtension(path);
                    //根据名称匹配类型
                    if (!checkDetail.audioNameReq.enable
                        || (checkDetail.audioNameMatch == EnumAudioNameMatchRule.前缀匹配 && audioName.StartsWith(checkDetail.audioNameReq.value.Trim()))
                        || (checkDetail.audioNameMatch == EnumAudioNameMatchRule.后缀匹配 && audioName.EndsWith(checkDetail.audioNameReq.value.Trim()))
                        || (checkDetail.audioNameMatch == EnumAudioNameMatchRule.包含即可 && audioName.IndexOf(checkDetail.audioNameReq.value.Trim()) > -1))
                    {
                        //音频文件
                        AudioClip audio = AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;
                        if (audio == null)
                            continue;

                        //获得音频文件磁盘文件大小
                        System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
                        if (fileInfo == null)
                            continue;

                        if (1.0f * fileInfo.Length > checkDetail.fileSizeLimit * Convert.ToInt32(checkDetail.storageMemoryType))
                        {
                            var formatVal = EditorUtility.FormatBytes(fileInfo.Length);
                            customRule.Record(audio, methodName, $"音频文件大于{checkDetail.fileSizeLimit}{checkDetail.storageMemoryType.ToString()} (实际大小{formatVal})", path);
                        }
                    }
                }
            }
            return true;
        }
        #endregion
    }
}