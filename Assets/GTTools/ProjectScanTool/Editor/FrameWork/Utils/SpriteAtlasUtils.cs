/*
 * @Description: 封装一些SpriteAtlas处理接口
 */
using System;
using GameTechTools.CommonLibs.CommonExtends;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace CasualEngine.ProjectScanTool
{
    public class SpriteAtlasUtils
    {
        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(SpriteAtlas atlas, string platform, int maxTextureSize, TextureImporterFormat textureFormat)
        {
            TextureImporterPlatformSettings settings = atlas.GetPlatformSettings(platform);
            settings.maxTextureSize = maxTextureSize;
            settings.format = textureFormat;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            atlas.SetPlatformSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(SpriteAtlas atlas, string platform, TextureImporterFormat textureFormat)
        {
            TextureImporterPlatformSettings settings = atlas.GetPlatformSettings(platform);
            settings.format = textureFormat;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            atlas.SetPlatformSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(SpriteAtlas atlas, string platform, int maxTextureSize)
        {
            TextureImporterPlatformSettings settings = atlas.GetPlatformSettings(platform);
            settings.maxTextureSize = maxTextureSize;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            atlas.SetPlatformSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(SpriteAtlas atlas, string platform, TextureImporterCompression textureCompression)
        {
            TextureImporterPlatformSettings settings = atlas.GetPlatformSettings(platform);
            settings.textureCompression = textureCompression;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            atlas.SetPlatformSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(SpriteAtlas atlas, string platform, UnityEditor.TextureCompressionQuality compressionQuality)
        {
            TextureImporterPlatformSettings settings = atlas.GetPlatformSettings(platform);
            settings.compressionQuality = Convert.ToInt32(compressionQuality);
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            atlas.SetPlatformSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(SpriteAtlas atlas, string platform,
        AndroidETC2FallbackOverride androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings)
        {
            if (platform != DeviceTypeDefine.Android)
            {
                return;
            }
            TextureImporterPlatformSettings settings = atlas.GetPlatformSettings(platform);
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            settings.androidETC2FallbackOverride = androidETC2FallbackOverride;
            atlas.SetPlatformSettings(settings);
        }
    }
}