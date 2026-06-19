//在ProjectScanDef.cs中添加新的模块类型 (如果需要添加新模块的话，否则直接添加检查项即可)

//扫描类型唯一标识
//public enum EnumScanModes
//{
//    通用设置 = 0,
//    基本资源检查,
//    贴图资源检查,
//    音频资源检查,
//    动效资源检查,
//    场景检查,
//    eExampleCheck
//}

//自定义某个检测项
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    public class Texture_Size_SpineCheckDetail : CustomCheckDetail
    {
        [LabelText("检查贴图Width")]
        public int imageWidthLimit = 1024;

        [LabelText("检查贴图Height")]
        public int imageHeightLimit = 1024;
    }

    [Serializable]
    //注册特性，并绑定对应模块类型
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class Texture_Size_Spine : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图尺寸规范：Spine";
            ruleDescription = "[说 明]: Spine贴图尺寸";
        }

        //定义检查规则列表
        [ListDrawerSettings(Expanded = true), LabelText("检查Spine贴图尺寸")]
        public List<Texture_Size_SpineCheckDetail> checkDetailList = new List<Texture_Size_SpineCheckDetail>() { new Texture_Size_SpineCheckDetail() };

        //注册扫描的执行逻辑
        [CustomScanAction]
        public void Do_CustomExampleCheck(string[] assetPostprocessorPath = null)
        {
            //添加逻辑
            ProjectScanHelper.DoCustomRuleCheck(Do_Texture_Size_SpineCheck, this, assetPostprocessorPath);
        }

        #region Spine贴图尺寸
        public static bool Do_Texture_Size_SpineCheck(Texture_Size_Spine customRule, string[] assetPostprocessorPath = null)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture, assetPostprocessorPath);
                foreach (var path in paths)
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                    if (texture == null)
                        continue;
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!textureImporter)
                        continue;

                    TextureUtils.GetTexWidthAndHeight(textureImporter, out int width, out int height);
                    if (width > checkDetail.imageWidthLimit || height > checkDetail.imageHeightLimit)
                    {
                        TextureImporterPlatformSettings settingsAndroid = textureImporter.GetPlatformTextureSettings("Android");
                        TextureImporterPlatformSettings settingsIOS = textureImporter.GetPlatformTextureSettings("iPhone");
                        if (textureImporter.maxTextureSize > checkDetail.imageWidthLimit
                            || settingsAndroid.maxTextureSize > checkDetail.imageWidthLimit
                            || settingsIOS.maxTextureSize > checkDetail.imageWidthLimit)
                        {
                            customRule.Record(texture, methodName, $"Spine贴图不规范，原图尺寸{width}x{height}，最大尺寸应该设置为1024", path, customRule.autoCorrection);

                            if (customRule.autoCorrection)
                            {
                                if (textureImporter.maxTextureSize > checkDetail.imageWidthLimit)
                                {
                                    textureImporter.maxTextureSize = checkDetail.imageWidthLimit;
                                }
                                if (settingsAndroid.maxTextureSize > checkDetail.imageWidthLimit)
                                {
                                    settingsAndroid.maxTextureSize = checkDetail.imageWidthLimit;
                                    textureImporter.SetPlatformTextureSettings(settingsAndroid);
                                }
                                if (settingsIOS.maxTextureSize > checkDetail.imageWidthLimit)
                                {
                                    settingsIOS.maxTextureSize = checkDetail.imageWidthLimit;
                                    textureImporter.SetPlatformTextureSettings(settingsIOS);
                                }

                                EditorUtility.SetDirty(textureImporter);
                                textureImporter.SaveAndReimport();
                                EditorUtility.SetDirty(texture);
                            }
                        }
                    }
                }
            }
            return true;
        }
        #endregion
    }
}