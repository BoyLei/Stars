///--------------------------------------------------------------------
/// 文件名   :   TextureImporterSetting.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/14 16:53:52
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class TextureImporterSetting
{
    [FolderPath]
    [LabelText("目录")]
    [OnValueChanged("OnFolderPathChange")]
    public string FolderPath;

    [LabelText("文件深度")]
    [ReadOnly]
    public int Deep = 0;


    [LabelText("备注")]
    public string Desc;

    [LabelText("本次刷新是否生效")]
    public bool IsThisUSE = false;

    [LabelText("是否开启读/写")]
    [FoldoutGroup("设置", Expanded = false)]
    public bool isReadable = false;

    [LabelText("是否开启minMap")]
    [FoldoutGroup("设置", Expanded = false)]
    public bool mipmapEnabled = true;

    [LabelText("是否开启流式miniMap")]
    [FoldoutGroup("设置", Expanded = false)]
    public bool streamMipmapEnabled = true;

    [LabelText("是否强刷stream优先级")]//默认，没开流加载的开启他并且设置优先级；开了根据true才强刷
    [FoldoutGroup("设置", Expanded = false)]
    public bool ForceRefStreamPro = false;

    [LabelText("加载等级")]
    [FoldoutGroup("设置", Expanded = false)]
    public int streamMipmapLevel = 0;

    [LabelText("是否使用图片颜色SRGB，一般都有")]
    [FoldoutGroup("设置", Expanded = false)]
    public bool sRGBTexture = true;

    [LabelText("图片格式")]
    [FoldoutGroup("设置", Expanded = false)]
    public TextureImporterType TextureType = TextureImporterType.Default;

    [LabelText("图片拉伸规则")]
    [FoldoutGroup("设置", Expanded = false)]
    public TextureImporterNPOTScale TextureImporterNPOTScale = TextureImporterNPOTScale.None;

    [LabelText("是否开启Alpha")]
    [FoldoutGroup("设置", Expanded = false)]
    public bool alphaIsTransparency = true;

    [LabelText("是否强制更新Alpha")]
    [FoldoutGroup("设置", Expanded = false)]
    public bool alphaIsTransparencyForce = false;//false就是保持原来的样子

    [LabelText("WrapMode")]
    [FoldoutGroup("设置", Expanded = false)]
    public TextureWrapMode WrapMode = TextureWrapMode.Repeat;

    [LabelText("FilterMode")]
    [FoldoutGroup("设置", Expanded = false)]
    public FilterMode FilterMode = FilterMode.Bilinear;

    [LabelText("抗锯齿等级")]
    [FoldoutGroup("设置", Expanded = false)]
    [PropertyRange(0, 16)]
    public int AnisoLevel = 2;

    [LabelText("Android平台设置")]
    [FoldoutGroup("设置", Expanded = false)]
    public AndroidPlatformSettings AndroidSettings;

    [LabelText("IPhone平台设置")]
    [FoldoutGroup("设置", Expanded = false)]
    public IPhonePlatformSettings IPhoneSettings;

    [LabelText("Standalone平台设置")]
    [FoldoutGroup("设置", Expanded = false)]
    public StandalonePlatformSettings StandaloneSettings;

    [Button("应用设置")]
    public void Apply()
    {
        ModifySetting(FolderPath);
    }


    private void ModifySetting(string Directory)
    {
        if (!string.IsNullOrEmpty(Directory))
        {
            string[] files = System.IO.Directory.GetFiles(Directory);
            if (files != null && files.Length > 0)
            {
                foreach (var item in files)
                {
                    if (item.EndsWith(".meta"))
                    {
                        continue;
                    }
                    string fileName = System.IO.Path.GetFileName(item);
                    if(AutoAssetPost.InBlockList(fileName))
                    {
                        continue;
                    }
                    TextureImporter textureImporter = AssetImporter.GetAtPath(item) as TextureImporter;
                    if (textureImporter != null)
                    {
                        //尔东的编辑器功能设定
                        AutoAssetPost.SetTextureFormat(textureImporter, this);
                    }

                }
            }
            //递归
            string[] childs = System.IO.Directory.GetDirectories(Directory);
            if(childs!=null && childs.Length>0)
            {
                Debug.LogError(childs.Length);
                foreach (var item in childs)
                {
                    ModifySetting(item);
                }
            }
        }
    }

    private void OnFolderPathChange()
    {
        Deep = 0;
        if (!string.IsNullOrEmpty(FolderPath))
        {
            Deep = FolderPath.Split('/').Length;
        }
    }
}

[System.Serializable]
public abstract class PlatformSettings
{
    public abstract string PlatformName { get; }

    [LabelText("图片格式")]
    public TextureImporterFormat ImporterFormat = TextureImporterFormat.ARGB32;

    [LabelText("最大尺寸")]
    public int maxTextureSize=2048;

    [LabelText("ResizeAlgorithm")]
    public TextureResizeAlgorithm resizeAlgorithm;

    [LabelText("压缩等级")]
    public TextureImporterCompression textureCompression;

    [LabelText("是否覆盖Default设置")]
    public bool overridden;
}


[System.Serializable]
public class AndroidPlatformSettings : PlatformSettings { public override string PlatformName { get { return "Android"; } } }

[System.Serializable]
public class IPhonePlatformSettings : PlatformSettings { public override string PlatformName { get { return "iPhone"; } } }


[System.Serializable]
public class StandalonePlatformSettings : PlatformSettings { public override string PlatformName { get { return "Standalone"; } } }