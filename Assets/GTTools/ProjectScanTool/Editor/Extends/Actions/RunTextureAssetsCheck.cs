/*
 * @Description: 纹理资源检查
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameTechTools.CommonLibs.CommonExtends;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CasualEngine.ProjectScanTool
{
    public class RunTextureAssetsCheck : MethodHelper
    {
        #region Texture基本参数设置检测
        public static bool Do_DefaultTextureFormatCheck(DefaultTextureFormatCheck customRule, string[] assetPostprocessorPath = null)
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
                    TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!texImporter)
                        continue;

                    //check textureType
                    if (checkDetail.texCheck.mustbeDefault.enable && checkDetail.texCheck.mustbeDefault.value == false && texImporter.textureType != TextureImporterType.Default)
                    {
                        continue;
                    }

                    if (checkDetail.texCheck.mustbeDefault.enable && checkDetail.texCheck.mustbeDefault.value && texImporter.textureType != TextureImporterType.Default)
                    {
                        Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.textureType), TextureImporterType.Default, customRule);
                    }

                    CheckDefaultTextureFormatParam(customRule, texture, methodName, path, texImporter, checkDetail.texCheck);
                    CheckTextureCompressionFormat(customRule, texture, methodName, path, texImporter, checkDetail.texCheck.textureFormat);

                    //一定要记得将texImporter重新save
                    //表示有更改
                    if (customRule.AssetsModifyed)
                    {
                        EditorUtility.SetDirty(texImporter);
                        texImporter.SaveAndReimport();
                        EditorUtility.SetDirty(texture);
                    }
                }
            }

            return true;
        }
        #endregion

        public static void CheckDefaultTextureFormatParam<T>(T customRule, Texture texture, string methodName, string path, TextureImporter texImporter, DefaultTextureFormatDetail checkDetail) where T : CustomRule
        {
            // check sRGBTexture
            if (checkDetail.sRGBTexture.enable)
                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.sRGBTexture), checkDetail.sRGBTexture.value, customRule);

            //check alphaSource
            if (checkDetail.alphaSource.enable)
                Comparator.CompareForClass(methodName, texture, texImporter, GetVarName(() => texImporter.alphaSource), checkDetail.alphaSource.value, customRule);

            //check alphaSource
            if (checkDetail.alphaSource.enable)
                Comparator.CompareForClass(methodName, texture, texImporter, GetVarName(() => texImporter.alphaSource), checkDetail.alphaSource.value, customRule);

            // check npotScale
            if (checkDetail.npotScale.enable)
                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.npotScale), checkDetail.npotScale.value, customRule);

            // check isReadable
            if (checkDetail.isReadable.enable)
                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.isReadable), checkDetail.isReadable.value, customRule);

            // check streamingMipmaps
            if (checkDetail.streamingMipmaps.enable)
                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.streamingMipmaps), checkDetail.streamingMipmaps.value, customRule);

            // check mipmapEnabled
            if (checkDetail.mipmapEnabled.enable)
                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.mipmapEnabled), checkDetail.mipmapEnabled.value, customRule);

            // check WrapMode
            if (checkDetail.wrapMode.enable)
                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.wrapMode), checkDetail.wrapMode.value, customRule);

            // check filterMode
            if (checkDetail.filterMode.enable)
                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.filterMode), checkDetail.filterMode.value, customRule);
        }

        #region Texture-Sprite基本参数设置检测
        public static bool Do_SpriteUIFormatCheck(SpriteUIFormatCheck customRule, string[] assetPostprocessorPath = null)
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
                    TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!texImporter)
                        continue;

                    if (checkDetail.mustbeSprite.enable && checkDetail.mustbeSprite.value == false && texImporter.textureType != TextureImporterType.Sprite)
                    {
                        continue;
                    }

                    //check textureType
                    if (checkDetail.mustbeSprite.enable && checkDetail.mustbeSprite.value && texImporter.textureType != TextureImporterType.Sprite)
                    {
                        Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.textureType), TextureImporterType.Sprite, customRule);
                    }

                    TextureImporterSettings texSetting = new TextureImporterSettings();
                    texImporter.ReadTextureSettings(texSetting);
                    int flag = 0;

                    //check sprite mode
                    if (checkDetail.spriteMode.enable && texSetting.spriteMode != checkDetail.spriteMode.value)
                    {
                        flag += (int)Comparator.CompareForClass(methodName, texture, texSetting, GetVarName(() => texSetting.spriteMode), checkDetail.spriteMode.value, customRule);
                    }

                    //check pixels per unit
                    if (checkDetail.pixelsPerUnit.enable && texImporter.spritePixelsPerUnit != (float)checkDetail.pixelsPerUnit.value)
                    {
                        Comparator.CompareForClass(methodName, texture, texImporter, GetVarName(() => texImporter.spritePixelsPerUnit), (float)checkDetail.pixelsPerUnit.value, customRule);
                    }

                    //check spriteExtrude
                    if (checkDetail.spriteExtrude.enable && texSetting.spriteExtrude < checkDetail.spriteExtrude.value)
                    {
                        flag += (int)Comparator.CompareForClass(methodName, texture, texSetting, GetVarName(() => texSetting.spriteExtrude), checkDetail.spriteExtrude.value, customRule, EnumCompareType.大于等于);
                    }

                    //check pivot
                    if (checkDetail.spriteAlignment.enable)
                    {
                        if (texSetting.spriteAlignment != checkDetail.spriteAlignment.value)
                        {
                            flag += (int)Comparator.CompareForClass(methodName, texture, texSetting, GetVarName(() => texSetting.spriteAlignment), checkDetail.spriteAlignment.value, customRule);
                        }
                        if (checkDetail.spriteAlignment.value == Convert.ToInt32(SpriteAlignment.Custom))
                        {
                            if (texSetting.spritePivot != checkDetail.spritePivot)
                            {
                                //开启了自动修正
                                if (customRule.autoCorrection)
                                {
                                    flag += 1;
                                    texSetting.spritePivot = new Vector2(checkDetail.spritePivot.x, checkDetail.spritePivot.y);
                                }
                                customRule.Record(texture, methodName, $"spritePivot参数设置错误", path);
                            }
                        }
                    }

                    //check generatePhysicsShape
                    if (checkDetail.generatePhysicsShape.enable && texSetting.spriteGenerateFallbackPhysicsShape != checkDetail.generatePhysicsShape.value)
                    {
                        flag += (int)Comparator.CompareForClass(methodName, texture, texSetting, GetVarName(() => texSetting.spriteGenerateFallbackPhysicsShape), checkDetail.generatePhysicsShape.value, customRule);
                    }
                    if (flag > 0)
                    {
                        texImporter.SetTextureSettings(texSetting);
                    }

                    // check sRGBTexture
                    if (checkDetail.sRGBTexture.enable)
                        Comparator.CompareForClass(methodName, texture, texImporter, GetVarName(() => texImporter.sRGBTexture), checkDetail.sRGBTexture.value, customRule);

                    //check alphaSource
                    if (checkDetail.alphaSource.enable)
                        Comparator.CompareForClass(methodName, texture, texImporter, GetVarName(() => texImporter.alphaSource), checkDetail.alphaSource.value, customRule);

                    //check alphaIsTransparency
                    if (checkDetail.alphaIsTransparency.enable)
                        Comparator.CompareForClass(methodName, texture, texImporter, GetVarName(() => texImporter.alphaIsTransparency), checkDetail.alphaIsTransparency.value, customRule);

                    // check isReadable
                    if (checkDetail.isReadable.enable)
                        Comparator.CompareForClass(methodName, texture, texImporter, GetVarName(() => texImporter.isReadable), checkDetail.isReadable.value, customRule);

                    // check mipmapEnabled
                    if (checkDetail.mipmapEnabled.enable)
                        Comparator.CompareForClass(methodName, texture, texImporter, GetVarName(() => texImporter.mipmapEnabled), checkDetail.mipmapEnabled.value, customRule);

                    // check WrapMode
                    if (checkDetail.wrapMode.enable)
                        Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.wrapMode), checkDetail.wrapMode.value, customRule);

                    // check filterMode
                    if (checkDetail.filterMode.enable)
                        Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.filterMode), checkDetail.filterMode.value, customRule);


                    CheckTextureCompressionFormat(customRule, texture, methodName, path, texImporter, checkDetail.textureFormat);

                    //一定要记得将texImporter重新save
                    if (customRule.AssetsModifyed)
                    {
                        EditorUtility.SetDirty(texImporter);
                        texImporter.SaveAndReimport();
                    }
                }
            }

            return true;
        }
        #endregion

        public static void CheckTextureCompressionFormat<T>(T customRule, Texture texture, string methodName, string path, TextureImporter texImporter, TextureCompressedSetting compressedSetting) where T : CustomRule
        {
            Func<TextureCompressedForPlatform, int, bool> IsNoNeedHandle = (detail, fmt) =>
            {
                foreach (var item in detail.noNeedHandle)
                {
                    if (item == fmt)
                    {
                        return true;
                    }
                }
                return false;
            };

            List<TextureCompressedForPlatform> settingList = new List<TextureCompressedForPlatform>(){
                compressedSetting.default_fmt,
                compressedSetting.win_fmt,
                compressedSetting.iOS_fmt,
                compressedSetting.android_fmt
            };

            foreach (var settingItem in settingList)
            {
                TextureImporterPlatformSettings curSetting = settingItem.deviceType == DeviceTypeDefine.Default ? texImporter.GetDefaultPlatformTextureSettings() : texImporter.GetPlatformTextureSettings(settingItem.deviceType);

                //check maxSize
                if (settingItem.maxTextureSize.enable && curSetting.maxTextureSize != settingItem.maxTextureSize.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        TextureUtils.SetPlatformTextureSettings(texImporter, settingItem.deviceType, settingItem.maxTextureSize.value);
                        EditorUtility.SetDirty(texImporter);
                    }
                    customRule.Record(texture, methodName, $"{settingItem.deviceType}纹理压缩Max Size参数设置错误，应该为{settingItem.maxTextureSize.value}", path, true);
                }

                //check resizeAlgorithm
                if (settingItem.resizeAlgorithm.enable && Convert.ToInt32(curSetting.resizeAlgorithm) != settingItem.resizeAlgorithm.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        TextureUtils.SetPlatformTextureSettings(texImporter, settingItem.deviceType, (TextureResizeAlgorithm)settingItem.resizeAlgorithm.value);
                        EditorUtility.SetDirty(texImporter);
                    }
                    customRule.Record(texture, methodName, $"{settingItem.deviceType}纹理压缩Resize Algorithm参数设置错误，应该为{settingItem.resizeAlgorithm.value}", path, true);
                }

                //check format
                int curFormat = Convert.ToInt32(curSetting.format);
                var setDefaultFmt = settingItem.format;
                if (setDefaultFmt.enable && curFormat != setDefaultFmt.value && !IsNoNeedHandle(settingItem, curFormat))
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        TextureUtils.SetPlatformTextureSettings(texImporter, settingItem.deviceType, (TextureImporterFormat)setDefaultFmt.value);
                        EditorUtility.SetDirty(texImporter);
                    }

                    TextureImporterFormat setFmt = (TextureImporterFormat)setDefaultFmt.value;
                    customRule.Record(texture, methodName, $"{settingItem.deviceType}纹理压缩Format参数设置错误，应该为{setFmt.ToString()}", path, true);
                }

                //check compression
                if (settingItem.compression.enable && Convert.ToInt32(curSetting.textureCompression) != settingItem.compression.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        TextureUtils.SetPlatformTextureSettings(texImporter, settingItem.deviceType, (TextureImporterCompression)settingItem.compression.value);
                        EditorUtility.SetDirty(texImporter);
                    }
                    customRule.Record(texture, methodName, $"{settingItem.deviceType}纹理压缩Compression参数设置错误，应该为{settingItem.compression.value}", path, true);
                }


                //check compresserQuality
                if (settingItem.compresserQuality.enable && curSetting.compressionQuality != settingItem.compresserQuality.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        TextureUtils.SetPlatformTextureSettings(texImporter, settingItem.deviceType, (UnityEditor.TextureCompressionQuality)settingItem.compresserQuality.value);
                        EditorUtility.SetDirty(texImporter);
                    }
                    customRule.Record(texture, methodName, $"{settingItem.deviceType}纹理压缩Compresser Quality参数设置错误，应该为{settingItem.compresserQuality.value}", path, true);
                }

                //check textureCompression
                if (settingItem.androidETC2FallbackOverride.enable && Convert.ToInt32(curSetting.androidETC2FallbackOverride) != settingItem.androidETC2FallbackOverride.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        TextureUtils.SetPlatformTextureSettings(texImporter, settingItem.deviceType, (AndroidETC2FallbackOverride)settingItem.androidETC2FallbackOverride.value);
                        EditorUtility.SetDirty(texImporter);
                    }
                    customRule.Record(texture, methodName, $"{settingItem.deviceType}纹理压缩Override ETC2 fallback参数设置错误，应该为{settingItem.androidETC2FallbackOverride.value}", path, true);
                }

            }
        }

        #region 开启Read/Write选项的纹理检查
        public static bool Do_TextureRWCheck(TextureRWCheck customRule, string[] assetPostprocessorPath = null)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.texture, assetPostprocessorPath);
            foreach (var path in paths)
            {
                Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                if (texture == null)
                    continue;
                TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!texImporter)
                    continue;


                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.isReadable), false, customRule);
                if (customRule.AssetsModifyed)
                {
                    EditorUtility.SetDirty(texImporter);
                    texImporter.SaveAndReimport();
                    EditorUtility.SetDirty(texture);
                }
                // Resources.UnloadAsset(texture);
            }

            return true;
        }
        #endregion

        #region 开启Mipmap选项的Sprite纹理检查
        public static bool Do_TextureMipmapCheck(TextureMipmapCheck customRule, string[] assetPostprocessorPath = null)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.texture);
            foreach (var path in paths)
            {
                Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                if (texture == null)
                    continue;
                TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!texImporter)
                    continue;

                Comparator.CompareForObject(methodName, texture, texImporter, GetVarName(() => texImporter.mipmapEnabled), false, customRule);
                if (customRule.AssetsModifyed)
                {
                    EditorUtility.SetDirty(texImporter);
                    texImporter.SaveAndReimport();
                }
            }

            return true;
        }
        #endregion

        #region 贴图尺寸规范扫描
        public static bool Do_TextureSizeCheck(TextureSizeCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
                foreach (var path in paths)
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                    if (texture == null)
                        continue;
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!textureImporter)
                        continue;

                    TextureUtils.GetTexWidthAndHeight(textureImporter, out int width, out int height);

                    if (checkDetail.scanType == EnumImgSizeScanType.eScan_w_h)
                    {
                        if (checkDetail.compareType == EnumCompareType.等于 || checkDetail.compareType == EnumCompareType.小于 || checkDetail.compareType == EnumCompareType.小于等于)
                        {
                            if (Comparator.CompareTowValue(width, checkDetail.imageWidthLimit, checkDetail.compareType)
                                && Comparator.CompareTowValue(height, checkDetail.imageHeightLimit, checkDetail.compareType))
                            {
                                customRule.Record(texture, methodName, $"贴图尺寸{checkDetail.compareType.ToString()}{checkDetail.imageWidthLimit}x{checkDetail.imageHeightLimit}, 当前尺寸{width}x{height}", path);
                            }
                        }
                        else
                        {
                            if (Comparator.CompareTowValue(width, checkDetail.imageWidthLimit, checkDetail.compareType)
                                || Comparator.CompareTowValue(height, checkDetail.imageHeightLimit, checkDetail.compareType))
                            {
                                customRule.Record(texture, methodName, $"贴图尺寸{checkDetail.compareType.ToString()}{checkDetail.imageWidthLimit}x{checkDetail.imageHeightLimit}, 当前尺寸{width}x{height}", path);
                            }
                        }
                    }
                    else if (checkDetail.scanType == EnumImgSizeScanType.eScan_fixed_mul)
                    {
                        if (width % checkDetail.sizeMul != 0 || height % checkDetail.sizeMul != 0)
                        {
                            customRule.Record(texture, methodName, $"贴图尺寸不符合要求, 当前尺寸{width}x{height}，实际要求是{checkDetail.sizeMul}的倍数", path);
                        }
                    }
                    else if (checkDetail.scanType == EnumImgSizeScanType.eScan_square)
                    {
                        if (width != height)
                        {
                            customRule.Record(texture, methodName, $"贴图尺寸不符合要求, 当前尺寸{width}x{height}，实际要求是正方形", path);
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        #region 包含无效透明通道的纹理检测
        public static bool Do_TextureAlphaAllOneCheck(TextureAlphaAllOneCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.texture);
            foreach (var path in paths)
            {
                Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                if (texture == null)
                    continue;
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!textureImporter)
                    continue;

                //由于要读取贴图数据，如果不开启readable无法读取纹理数据缓存。但是如果将原图启用readable，需要重新save和refresh才能获取数据
                //之后再改回来非常耗时。所以这里采用直接copy一份数据出来，这样就可以不修改readable，并且可以读取贴图的信息
                Texture2D texCopy = TextureUtils.CloneNewTextureFile(path, textureImporter);
                Color[] colors = texCopy.GetPixels();
                bool bAllOne = true;
                for (var i = 0; i < colors.Length; i++)
                {
                    if (colors[i] != null && colors[i].a == 0)
                    {
                        bAllOne = false;
                        break;
                    }
                }
                if (bAllOne && textureImporter.alphaIsTransparency)
                {
                    customRule.Record(texture, methodName, "包含无效透明通道的纹理，建议不勾选Alpha is Transparency选项", path);
                }
                GameObject.DestroyImmediate(texCopy);
            }
            return true;
        }
        #endregion

        #region 贴图的透明像数占比过高检测
        public static bool Do_TextureTooManyTransparentPixels(TextureTooManyTransparentPixels customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
                foreach (var path in paths)
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                    if (texture == null)
                        continue;
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!textureImporter)
                        continue;

                    TextureUtils.GetTexWidthAndHeight(textureImporter, out int width, out int height);
                    if (checkDetail.limitImgSize)
                    {
                        if (width < checkDetail.widthLimit || height < checkDetail.heightLimit)
                        {
                            continue;
                        }
                    }
                    //由于要读取贴图数据，如果不开启readable无法读取纹理数据缓存。但是如果将原图启用readable，需要重新save和refresh才能获取数据
                    //之后再改回来非常耗时。所以这里采用直接copy一份数据出来，这样就可以不修改readable，并且可以读取贴图的信息
                    Texture2D texCopy = TextureUtils.CloneNewTextureFile(path, textureImporter);

                    Color[] colors = texCopy.GetPixels();
                    int alphaPix = 0;
                    for (var i = 0; i < colors.Length; i++)
                    {
                        if (colors[i] != null && colors[i].a == 0)
                        {
                            alphaPix++;
                        }
                    }

                    var pRange = Math.Round((100.0f * alphaPix / (width * height)), 2);
                    if (pRange > checkDetail.alphaPixRange)
                    {
                        customRule.Record(texture, methodName, $"贴图的透明像数占比过高({pRange}%)", path);
                    }
                    GameObject.DestroyImmediate(texCopy);
                }
            }
            return true;
        }
        #endregion

        #region 贴图文件内存占用检查
        public static bool Do_TextureStorageMemoryCheck(TextureStorageMemoryCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
                foreach (var path in paths)
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                    if (texture == null)
                        continue;

                    var value = TextureUtils.GetTextureStorageMemorySize(texture);
                    if (1.0f * value > checkDetail.storageMemoryLimit * Convert.ToInt32(checkDetail.storageMemoryType))
                    {
                        var formatVal = EditorUtility.FormatBytes(value);
                        customRule.Record(texture, methodName, $"贴图内存大于{checkDetail.storageMemoryLimit}{checkDetail.storageMemoryType.ToString()} (实际大小{formatVal})", path);
                    }
                }
            }
            return true;
        }
        #endregion

        #region 图集检查
        public static bool Do_SpriteAtlasFormatCheck(SpriteAtlasFormatCheck customRule, string[] assetPostprocessorPath = null)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.spriteAtlas, assetPostprocessorPath);
                foreach (var path in paths)
                {
                    SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath(path, typeof(SpriteAtlas)) as SpriteAtlas;
                    if (atlas == null)
                        continue;

                    //图集的某些属性并没有直接暴露出来，需要使用读取序列化文件获取
                    SerializedObject ob = new SerializedObject(atlas);
                    SerializedProperty isVariant = ob.FindProperty("m_IsVariant");
                    SerializedProperty atlasEditorData = ob.FindProperty("m_EditorData");

                    int intVariant = isVariant.boolValue ? 1 : 0;

                    if (checkDetail.atlasType.enable && checkDetail.atlasType.value != intVariant)
                    {
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            atlas.SetIsVariant(checkDetail.atlasType.value == 1);
                            //应用属性修改
                            ob.ApplyModifiedProperties();
                            ob.Update();
                        }
                        string typeTip = "";
                        foreach (var item in SpriteAtlasFormatCheck.kAtlasTypes)
                        {
                            if (item.Value == checkDetail.atlasType.value)
                            {
                                typeTip = item.Text;
                            }
                        }
                        customRule.Record(atlas, methodName, $"图集的Type参数设置错误，应该为{typeTip}", path);
                    }

                    // //防止自动修改改动，重新拿一下
                    isVariant = ob.FindProperty("m_IsVariant");

                    //检查 MasterAtlas
                    if (isVariant.boolValue && checkDetail.masterAtlasCheck.enable && checkDetail.masterAtlasCheck.value)
                    {
                        SerializedProperty m_MasterAtlas = ob.FindProperty("m_MasterAtlas");
                        if (m_MasterAtlas == null || m_MasterAtlas.objectReferenceValue == null)
                        {
                            customRule.Record(atlas, methodName, $"图集Master Atlas引用丢失", path);
                        }
                    }

                    //检查Scale
                    if (isVariant.boolValue && checkDetail.variantMultiplier.enable)
                    {
                        checkDetail.variantMultiplier.value = Math.Min(1, checkDetail.variantMultiplier.value);
                        checkDetail.variantMultiplier.value = Math.Max(0, checkDetail.variantMultiplier.value);
                        SerializedProperty variantMultiplier = atlasEditorData.FindPropertyRelative("variantMultiplier");
                        if (variantMultiplier.floatValue != checkDetail.variantMultiplier.value)
                        {
                            //开启了自动修正
                            if (customRule.autoCorrection)
                            {
                                atlas.SetVariantScale(checkDetail.variantMultiplier.value);
                                //应用属性修改
                                ob.ApplyModifiedProperties();
                                ob.Update();
                            }
                            customRule.Record(atlas, methodName, $"图集的Variant Scale参数设置错误，应该为{checkDetail.variantMultiplier.value}", path);
                        }
                    }

                    //检查 include in build属性
                    SerializedProperty bindAsDefault = atlasEditorData.FindPropertyRelative("bindAsDefault");
                    bool includeInBuild = bindAsDefault.boolValue;
                    if (checkDetail.includeInBuild.enable && checkDetail.includeInBuild.value != includeInBuild)
                    {
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            atlas.SetIncludeInBuild(checkDetail.includeInBuild.value);
                            //应用属性修改
                            ob.ApplyModifiedProperties();
                            ob.Update();
                        }
                        customRule.Record(atlas, methodName, $"图集的Include in Build参数设置错误，应该为{checkDetail.includeInBuild.value}", path);
                    }

                    //检查Packing一系列设置
                    int flag = 0;
                    if (!isVariant.boolValue)
                    {
                        flag = 0;
                        SpriteAtlasPackingSettings packingSettings = atlas.GetPackingSettings();
                        if (checkDetail.enableRotation.enable)
                            flag += (int)Comparator.CompareForStruct(methodName, atlas, ref packingSettings, GetVarName(() => packingSettings.enableRotation), checkDetail.enableRotation.value, customRule);

                        if (checkDetail.enableTightPacking.enable)
                            flag += (int)Comparator.CompareForStruct(methodName, atlas, ref packingSettings, GetVarName(() => packingSettings.enableTightPacking), checkDetail.enableTightPacking.value, customRule);

                        if (checkDetail.padding.enable)
                            flag += (int)Comparator.CompareForStruct(methodName, atlas, ref packingSettings, GetVarName(() => packingSettings.padding), checkDetail.padding.value, customRule);


                        if (flag > 0)
                        {
                            atlas.SetPackingSettings(packingSettings);
                        }

                        if (checkDetail.packObjectsCheck.enable && checkDetail.packObjectsCheck.value)
                        {
                            var packObjs = atlas.GetPackables();
                            if (packObjs == null || packObjs.Length == 0)
                            {
                                customRule.Record(atlas, methodName, $"图集{Path.GetFileName(path)}Objects for Packing引用丢失", path);
                            }
                            else
                            {
                                int i = 0;
                                for (i = 0; i < packObjs.Length; i++)
                                {
                                    if (packObjs[i] != null)
                                    {
                                        break;
                                    }
                                }
                                if (i >= packObjs.Length)
                                {
                                    customRule.Record(atlas, methodName, $"图集{Path.GetFileName(path)}Objects for Packing引用丢失", path);
                                }
                            }
                        }
                    }

                    //检查texturesettings一些列设置
                    SpriteAtlasTextureSettings textureSettings = atlas.GetTextureSettings();

                    flag = 0;
                    // check isReadable
                    if (checkDetail.readable.enable)
                        flag += (int)Comparator.CompareForStruct(methodName, atlas, ref textureSettings, GetVarName(() => textureSettings.readable), checkDetail.readable.value, customRule);

                    // check mipmapEnabled
                    if (checkDetail.generateMipMaps.enable)
                        flag += (int)Comparator.CompareForStruct(methodName, atlas, ref textureSettings, GetVarName(() => textureSettings.generateMipMaps), checkDetail.generateMipMaps.value, customRule);

                    // check sRGBTexture
                    if (checkDetail.sRGBTexture.enable)
                        flag += (int)Comparator.CompareForStruct(methodName, atlas, ref textureSettings, GetVarName(() => textureSettings.sRGB), checkDetail.sRGBTexture.value, customRule);

                    // check filterMode
                    if (checkDetail.filterMode.enable)
                        flag += (int)Comparator.CompareForStruct(methodName, atlas, ref textureSettings, GetVarName(() => textureSettings.filterMode), checkDetail.filterMode.value, customRule);

                    if (flag > 0)
                    {
                        atlas.SetTextureSettings(textureSettings);
                    }

                    CheckSpriteAtlasCompressionFormat(customRule, atlas, methodName, path, checkDetail.textureFormat);

                    if (customRule.AssetsModifyed)
                    {
                        EditorUtility.SetDirty(atlas);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 图集的压缩格式检查
        /// </summary>
        public static void CheckSpriteAtlasCompressionFormat<T>(T customRule, SpriteAtlas atlas, string methodName, string path, TextureCompressedSetting compressedSetting) where T : CustomRule
        {
            Func<TextureCompressedForPlatform, int, bool> IsNoNeedHandle = (detail, fmt) =>
            {
                foreach (var item in detail.noNeedHandle)
                {
                    if (item == fmt)
                    {
                        return true;
                    }
                }
                return false;
            };

            List<TextureCompressedForPlatform> settingList = new List<TextureCompressedForPlatform>(){
                compressedSetting.default_fmt,
                compressedSetting.win_fmt,
                compressedSetting.iOS_fmt,
                compressedSetting.android_fmt
            };

            foreach (var settingItem in settingList)
            {
                //坑，图集里面的机型名称和普通贴图不一样，一个是IOS，一个是iPhone
                if (settingItem.deviceType == DeviceTypeDefine.IOS)
                {
                    settingItem.deviceType = DeviceTypeDefine.iPhone;
                }
                else if (settingItem.deviceType == DeviceTypeDefine.Default)
                {
                    settingItem.deviceType = DeviceTypeDefine.DefaultTexturePlatform;
                }
                TextureImporterPlatformSettings curSetting = atlas.GetPlatformSettings(settingItem.deviceType);

                //check maxSize
                if (settingItem.maxTextureSize.enable && curSetting.maxTextureSize != settingItem.maxTextureSize.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        SpriteAtlasUtils.SetPlatformTextureSettings(atlas, settingItem.deviceType, settingItem.maxTextureSize.value);
                        EditorUtility.SetDirty(atlas);
                    }
                    customRule.Record(atlas, methodName, $"{settingItem.deviceType}纹理压缩Max Size参数设置错误，应该为{settingItem.maxTextureSize.value}", path, true);
                }

                //check format
                int curFormat = Convert.ToInt32(curSetting.format);
                var setDefaultFmt = settingItem.format;
                if (setDefaultFmt.enable && curFormat != setDefaultFmt.value && !IsNoNeedHandle(settingItem, curFormat))
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        SpriteAtlasUtils.SetPlatformTextureSettings(atlas, settingItem.deviceType, (TextureImporterFormat)setDefaultFmt.value);
                        EditorUtility.SetDirty(atlas);
                    }

                    TextureImporterFormat setFmt = (TextureImporterFormat)setDefaultFmt.value;
                    customRule.Record(atlas, methodName, $"{settingItem.deviceType}纹理压缩Format参数设置错误，应该为{setFmt.ToString()}", path, true);
                }

                //check compression
                if (settingItem.compression.enable && Convert.ToInt32(curSetting.textureCompression) != settingItem.compression.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        SpriteAtlasUtils.SetPlatformTextureSettings(atlas, settingItem.deviceType, (TextureImporterCompression)settingItem.compression.value);
                        EditorUtility.SetDirty(atlas);
                    }
                    customRule.Record(atlas, methodName, $"{settingItem.deviceType}纹理压缩Compression参数设置错误，应该为{settingItem.compression.value}", path, true);
                }

                //check compresserQuality
                if (settingItem.compresserQuality.enable && curSetting.compressionQuality != settingItem.compresserQuality.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        SpriteAtlasUtils.SetPlatformTextureSettings(atlas, settingItem.deviceType, (UnityEditor.TextureCompressionQuality)settingItem.compresserQuality.value);
                        EditorUtility.SetDirty(atlas);
                    }
                    customRule.Record(atlas, methodName, $"{settingItem.deviceType}纹理压缩Compresser Quality参数设置错误，应该为{settingItem.compresserQuality.value}", path, true);
                }

                //check textureCompression
                if (settingItem.androidETC2FallbackOverride.enable && Convert.ToInt32(curSetting.androidETC2FallbackOverride) != settingItem.androidETC2FallbackOverride.value)
                {
                    //开启了自动修正
                    if (customRule.autoCorrection)
                    {
                        SpriteAtlasUtils.SetPlatformTextureSettings(atlas, settingItem.deviceType, (AndroidETC2FallbackOverride)settingItem.androidETC2FallbackOverride.value);
                        EditorUtility.SetDirty(atlas);
                    }
                    customRule.Record(atlas, methodName, $"{settingItem.deviceType}纹理压缩Override ETC2 fallback参数设置错误，应该为{settingItem.androidETC2FallbackOverride.value}", path, true);
                }

            }
        }
        #endregion

        #region 贴图文件大小检查
        public static bool Do_TextureStorageSizeCheck(TextureStorageSizeCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
                foreach (var path in paths)
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                    if (texture == null)
                        continue;

                    //获得文件磁盘文件大小
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
                    if (fileInfo == null)
                        continue;

                    if (1.0f * fileInfo.Length > checkDetail.storageSizeLimit * Convert.ToInt32(checkDetail.storageMemoryType))
                    {
                        var formatVal = EditorUtility.FormatBytes(fileInfo.Length);
                        customRule.Record(texture, methodName, $"贴图文件大于{checkDetail.storageSizeLimit}{checkDetail.storageMemoryType.ToString()} (实际大小{formatVal})", path);
                    }
                }
            }
            return true;
        }
        #endregion

        #region AndroidETC2必须要求贴图尺寸是4的倍数
        public static bool Do_AndroidETC2_TextureSizeCheck(AndroidETC2_TextureSizeCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.texture);
            foreach (var path in paths)
            {
                Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                if (texture == null)
                    continue;
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!textureImporter)
                    continue;

                TextureImporterPlatformSettings settingsAndroid = textureImporter.GetPlatformTextureSettings(DeviceTypeDefine.Android);
                if (settingsAndroid.format != TextureImporterFormat.ETC2_RGBA8
                && settingsAndroid.format != TextureImporterFormat.ETC2_RGBA8Crunched
                && settingsAndroid.format != TextureImporterFormat.ETC2_RGB4
                && settingsAndroid.format != TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA)
                {
                    continue;
                }

                TextureUtils.GetTexWidthAndHeight(textureImporter, out int width, out int height);
                if (width % 4 != 0 || height % 4 != 0)
                {
                    customRule.Record(texture, methodName, $"Android ETC2贴图尺寸必须是4的倍数, 当前尺寸{width}x{height}", path);
                }
            }
            return true;
        }
        #endregion

        #region AndroidETC2必须要求贴图尺寸是4的倍数
        public static bool Do_IOSPVRTC_TextureSizeCheck(IOSPVRTC_TextureSizeCheck customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, customRule.checkDetail, AssetType.texture);
            foreach (var path in paths)
            {
                Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                if (texture == null)
                    continue;
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!textureImporter)
                    continue;

                TextureImporterPlatformSettings settingsIOS = textureImporter.GetPlatformTextureSettings(DeviceTypeDefine.IOS);
                if (settingsIOS.format != TextureImporterFormat.PVRTC_RGB2
                && settingsIOS.format != TextureImporterFormat.PVRTC_RGB4
                && settingsIOS.format != TextureImporterFormat.PVRTC_RGBA2
                && settingsIOS.format != TextureImporterFormat.PVRTC_RGBA4)
                {
                    continue;
                }

                TextureUtils.GetTexWidthAndHeight(textureImporter, out int width, out int height);
                if (width != height || !TextureUtils.IsPowerOfTwo((ulong)width))
                {
                    customRule.Record(texture, methodName, $"IOS PVRTC的贴图必须是正方, 当前尺寸{width}x{height}", path);
                }
            }
            return true;
        }
        #endregion
    }
}