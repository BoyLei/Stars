/*
 * @Description: 公共接口
 */
using System.IO;

namespace GameTechTools.CommonLibs.CommonExtends
{
    public class GTUtils
    {
        /// <summary>
        /// 是否是图片，字体和psd文件在unity中也或当作Texture来load，类似鲜花项目会将psd文件放在asset内，需要特殊注意
        ///unity引擎TextureImporter	jpg、jpeg、tif、tiff、tga、gif、png、psd、bmp、iff、pict、pic、pct、exr、hdr，这里我们只需要定义常规属于图片的类型
        /// </summary>
        public static bool IsImage(string path)
        {
            string ext = Path.GetExtension(path);
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga";
        }

        /// <summary>
        /// 是否是音频文件，官方音频文件格式https://docs.unity3d.com/cn/2019.4/Manual/AssetDatabaseRefreshing.html
        /// </summary>
        public static bool IsAudio(string path)
        {
            string ext = Path.GetExtension(path);
            return ext == ".mp3" || ext == ".ogg" || ext == ".wav" || ext == ".aiff" || ext == ".aif" 
            || ext == ".mod" || ext == ".it" || ext == ".s3m" || ext == ".xm" || ext == ".flac";
        }

        /// <summary>
        /// 是否是图集
        /// </summary>
        public static bool IsSpriteAtlas(string path)
        {
            string ext = Path.GetExtension(path);
            return ext == PathExtensionDefine.spriteatlas;
        }
    }
}