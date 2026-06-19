/*
 * @Description: 封装一些Texture处理接口
 */
using System;
using System.IO;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using GameTechTools.CommonLibs.CommonExtends;

namespace CasualEngine.ProjectScanTool
{
    public class TextureUtils
    {
        /// <summary>
        /// 是否是图片，字体和psd文件在unity中也或当作Texture来load，类似鲜花项目会将psd文件放在asset内，需要特殊注意
        /// </summary>
        public static bool IsImage(string path)
        {
            string ext = Path.GetExtension(path);
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga";
        }

        /// <summary>
        /// 获得默认平台所支持的纹理压缩格式
        /// </summary>
        public static void GetDefaultTextureFormatValuesAndStrings(TextureImporterType textureType, ref ValueDropdownList<int> formats)
        {
            //利用反射获取defalut、ios、android实际支持的纹理压缩格式，如果不这样处理，直接TextureImporterFormat作为枚举选择，会有很多非当前平台的压缩参数
            Assembly asm = Assembly.Load("UnityEditor");
            Type textureImportValidFormats = asm.GetType("UnityEditor.TextureImportValidFormats");//参数必须是类的全名
            var method = textureImportValidFormats.GetMethod("GetDefaultTextureFormatValuesAndStrings");
            object[] args = new object[3] { TextureImporterType.Default, null, null };
            method.Invoke(null, args);
            int[] formatValues = (int[])args[1];
            string[] formatStrings = (string[])args[2];
            formats.Clear();
            for (var i = 0; i < formatStrings.Length; i++)
            {
                formats.Add(formatStrings[i], formatValues[i]);
            }
        }

        /// <summary>
        /// 获得对应平台所支持的纹理压缩格式
        /// </summary>
        public static void GetPlatformTextureFormatValuesAndStrings(TextureImporterType textureType, BuildTarget target, ref ValueDropdownList<int> formats)
        {
            //利用反射获取defalut、ios、android实际支持的纹理压缩格式，如果不这样处理，直接TextureImporterFormat作为枚举选择，会有很多非当前平台的压缩参数
            Assembly asm = Assembly.Load("UnityEditor");
            Type textureImportValidFormats = asm.GetType("UnityEditor.TextureImportValidFormats");//参数必须是类的全名

            var method = textureImportValidFormats.GetMethod("GetPlatformTextureFormatValuesAndStrings");
            //第3、4参数是out，详细查看TextureImportValidFormats.cs
            //源码 https://searchcode.com/file/309300982/Editor/Mono/ImportSettings/TextureImportValidFormats.cs/
            object[] args = new object[4] { TextureImporterType.Default, target, null, null };
            method.Invoke(null, args);
            int[] formatValues = (int[])args[2];
            string[] formatStrings = (string[])args[3];
            formats.Clear();
            for (var i = 0; i < formatStrings.Length; i++)
            {
                formats.Add(formatStrings[i], formatValues[i]);
            }
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(TextureImporter texImporter, string platform, int maxTextureSize, TextureImporterFormat textureFormat)
        {
            TextureImporterPlatformSettings settings = platform == DeviceTypeDefine.Default ? texImporter.GetDefaultPlatformTextureSettings() : texImporter.GetPlatformTextureSettings(platform);
            settings.maxTextureSize = maxTextureSize;
            settings.format = textureFormat;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            texImporter.SetPlatformTextureSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(TextureImporter texImporter, string platform, TextureImporterFormat textureFormat)
        {
            TextureImporterPlatformSettings settings = platform == DeviceTypeDefine.Default ? texImporter.GetDefaultPlatformTextureSettings() : texImporter.GetPlatformTextureSettings(platform);
            settings.format = textureFormat;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            texImporter.SetPlatformTextureSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(TextureImporter texImporter, string platform, int maxTextureSize)
        {
            TextureImporterPlatformSettings settings = platform == DeviceTypeDefine.Default ? texImporter.GetDefaultPlatformTextureSettings() : texImporter.GetPlatformTextureSettings(platform);
            settings.maxTextureSize = maxTextureSize;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            texImporter.SetPlatformTextureSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(TextureImporter texImporter, string platform, TextureResizeAlgorithm resizeAlgorithm)
        {
            TextureImporterPlatformSettings settings = platform == DeviceTypeDefine.Default ? texImporter.GetDefaultPlatformTextureSettings() : texImporter.GetPlatformTextureSettings(platform);
            settings.resizeAlgorithm = resizeAlgorithm;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            texImporter.SetPlatformTextureSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(TextureImporter texImporter, string platform, TextureImporterCompression textureCompression)
        {
            TextureImporterPlatformSettings settings = platform == DeviceTypeDefine.Default ? texImporter.GetDefaultPlatformTextureSettings() : texImporter.GetPlatformTextureSettings(platform);
            settings.textureCompression = textureCompression;
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            texImporter.SetPlatformTextureSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(TextureImporter texImporter, string platform, UnityEditor.TextureCompressionQuality compressionQuality)
        {
            TextureImporterPlatformSettings settings = platform == DeviceTypeDefine.Default ? texImporter.GetDefaultPlatformTextureSettings() : texImporter.GetPlatformTextureSettings(platform);
            settings.compressionQuality = Convert.ToInt32(compressionQuality);
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            texImporter.SetPlatformTextureSettings(settings);
        }

        /// <summary>
        /// 设置材质压缩参数
        /// </summary>
        public static void SetPlatformTextureSettings(TextureImporter texImporter, string platform,
        AndroidETC2FallbackOverride androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings)
        {
            if (platform != DeviceTypeDefine.Android)
            {
                return;
            }
            TextureImporterPlatformSettings settings = platform == DeviceTypeDefine.Default ? texImporter.GetDefaultPlatformTextureSettings() : texImporter.GetPlatformTextureSettings(platform);
            //default一定不要设置这个为true，否则会出现纹理数据保存出错
            if (platform != DeviceTypeDefine.Default)
            {
                settings.overridden = true;
            }
            settings.androidETC2FallbackOverride = androidETC2FallbackOverride;
            texImporter.SetPlatformTextureSettings(settings);
        }

        /// <summary>
        /// 获得贴图的原始宽高
        /// </summary>
        public static void GetTexWidthAndHeight(TextureImporter texImporter, out int width, out int height)
        {
            object[] args = new object[2] { 0, 0 };
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(texImporter, args);

            width = (int)args[0];
            height = (int)args[1];
        }

        /// <summary>
        /// copy一份Texture数据
        /// </summary>
        public static Texture2D CloneNewTextureFile(string path, TextureImporter texImporter)
        {
            FileStream fs = File.OpenRead(Path.GetFullPath(path));
            fs.Seek(0, SeekOrigin.Begin);
            byte[] image = new byte[(int)fs.Length];
            fs.Read(image, 0, (int)fs.Length);

            TextureUtils.GetTexWidthAndHeight(texImporter, out int width, out int height);
            var texCopy = new Texture2D(width, height);
            texCopy.LoadImage(image);
            return texCopy;
        }

        /// <summary>
        /// 获得目标平台占用内存大小
        /// </summary>
        public static int GetTextureStorageMemorySize(Texture texture)
        {
            return (int)InvokeInternalAPI("UnityEditor.TextureUtil", "GetStorageMemorySize", texture);
        }

        private static object InvokeInternalAPI(string type, string method, params object[] parameters)
        {
            var assembly = typeof(AssetDatabase).Assembly;
            var custom = assembly.GetType(type);
            var methodInfo = custom.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            return methodInfo != null ? methodInfo.Invoke(null, parameters) : 0;
        }

        /// <summary>
        /// 获得贴图运行时占用内存大小，根据纹理压缩计算
        /// </summary>
        public static long GetTextureRuntimeMemorySize(Texture texture)
        {
            if (texture == null)
            {
                return 0;
            }
            return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
        }

        /// <summary>
        /// 判断数字是否是2次幂
        /// </summary>
        public static bool IsPowerOfTwo(ulong x)
        {
            return x > 0 && (x & (x - 1)) == 0;
        }

    }

}
