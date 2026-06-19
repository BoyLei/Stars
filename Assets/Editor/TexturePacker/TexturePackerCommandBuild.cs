///--------------------------------------------------------------------
/// 文件名   :   TexturePackerCommandBuild.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/24 18:01:04
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using MapEditor;
using System.Reflection;
using System.Collections.Generic;

public class TexturePackerCommandBuild
{
    private const string KEY_WIDTH = "{-width-}";
    private const string KEY_HEIGHT = "{-height-}";
    private const string KEY_FILE_PATH = "{-filename-}";
    private const string KEY_SHAPE_PADDING = "{-shapePadding-}";
    private const string KEY_BORDER_PADDING = "{-borderPadding-}";
    private const string KEY_TRIM_MODE = "{-TrimMode-}";

    private const string KEY_FILE_NAME = "{-FileName-}";
    private const string KEY_MAX_SIZE = "{-maxTextureSize-}";
    private static string m_strTemplateTPS = "Assets/Editor/TexturePacker/template.tps";
    private static string m_strTexturePackerExe = "C:/Program Files (x86)/CodeAndWeb/TexturePacker/bin/TexturePacker.exe";
    public enum TrimMode
    {
        None,
        Trim,
    }
    public static void MakeTPS(string strPath, int nWidth, int nHeight, string[] filePaths,
        int shapePadding = 2, int borderPadding = 2, TrimMode trimMode = TrimMode.None)
    {
        string strTargetPath = m_strTemplateTPS.Replace("Assets", Application.dataPath);
        if (!File.Exists(strTargetPath))
        {
            UnityEngine.Debug.Log("TexturePackerCommandBuild MakeTPS, not exit file : " + strTargetPath);
            return;
        }
        string strData = File.ReadAllText(strTargetPath, Encoding.UTF8);
        strData = strData.Replace(KEY_WIDTH, nWidth.ToString());
        strData = strData.Replace(KEY_HEIGHT, nHeight.ToString());
        strData = strData.Replace(KEY_MAX_SIZE, "2048");

        StringBuilder strBuilder = new StringBuilder();
        for (int i = 0; i < filePaths.Length; i++)
        {
            strBuilder.AppendLine("<filename>" + filePaths[i] + "</filename>");
        }
        strData = strData.Replace(KEY_FILE_PATH, strBuilder.ToString());

        strData = strData.Replace(KEY_SHAPE_PADDING, shapePadding.ToString());
        strData = strData.Replace(KEY_BORDER_PADDING, borderPadding.ToString());
        strData = strData.Replace(KEY_TRIM_MODE, trimMode.ToString());

        string strFileNameKey = System.IO.Path.GetFileNameWithoutExtension(strPath);
        strData = strData.Replace(KEY_FILE_NAME, strFileNameKey);

        Encoding eutf8 = new UTF8Encoding(false);   // 不带bom
        File.WriteAllText(strPath, strData, eutf8);
        AssetDatabase.ImportAsset(strPath);
    }


    [MenuItem("Assets/图集工具/生成TP文件和图集", false, 1)]
    public static void BuildAtlas()
    {
        //文件路径
        string AssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string FolderName = System.IO.Path.GetFileName(AssetPath);
        //是不是 文件目录
        if (System.IO.Path.HasExtension(AssetPath))
        {
            UnityEngine.Debug.LogError("请选中文件目录");
            return;
        }

        string[] files = System.IO.Directory.GetFiles(AssetPath, "*.png");
        if (files == null || files.Length < 1)
        {
            UnityEngine.Debug.LogError("文件目录没找到图片");
            return;
        }

        List<string> inputs = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            TextureImporter texImp = AssetImporter.GetAtPath(files[i]) as TextureImporter;
            if (texImp != null)
            {
                string filename = System.IO.Path.GetFileName(files[i]);
                if (filename.Replace(".png", "") == FolderName)
                {
                    Debug.LogError($"filename{filename}");
                    continue;
                }
                inputs.Add(filename);
            }
        }

        // png 文件不删除 因为有meta 文件要读取， 记录的有Border 等信息
        //if (System.IO.File.Exists($"{AssetPath}/{FolderName}.png"))
        //{
        //    AssetDatabase.DeleteAsset($"{AssetPath}/{FolderName}.png");
        //}

        if (System.IO.File.Exists($"{AssetPath}/{FolderName}.tps"))
        {
            System.IO.File.Delete($"{AssetPath}/{FolderName}.tps");
        }

        if (System.IO.File.Exists($"{AssetPath}/{FolderName}.txt"))
        {
            System.IO.File.Delete($"{AssetPath}/{FolderName}.txt");
        }
        if (System.IO.File.Exists($"{AssetPath}/{FolderName}.asset"))
        {
            System.IO.File.Delete($"{AssetPath}/{FolderName}.asset");
        }
        MakeTPS($"{AssetPath}/{FolderName}.tps", 0, 0, inputs.ToArray());
        BuildTexturePack($"{AssetPath}/{FolderName}.tps");
        AssetDatabase.ImportAsset($"{AssetPath}/{FolderName}.png");
        AssetDatabase.ImportAsset($"{AssetPath}/{FolderName}.txt");
        AtlasGenera.ProcessToSprite(AssetDatabase.LoadAssetAtPath<TextAsset>($"{AssetPath}/{FolderName}.txt"));
        AssetDatabase.Refresh();
    }


    public static int CalMaxminSize(int tolsize, int NowMax)
    {
        if (tolsize > NowMax * NowMax)
        {
            NowMax *= 2;
            return CalMaxminSize(tolsize, NowMax);
        }
        return NowMax;
    }

    public static void GetTextureOriginalSize(TextureImporter ti, out int width, out int height)
    {
        if (ti == null)
        {
            width = 0;
            height = 0;
            return;
        }
        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(ti, args);
        width = (int)args[0];
        height = (int)args[1];
    }

    public static void BuildTexturePack(string strTPSPath)
    {
        MapEditorUtils.RunBat(m_strTexturePackerExe, strTPSPath);
    }
}

