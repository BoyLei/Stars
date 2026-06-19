///--------------------------------------------------------------------
/// 文件名   :   ResCheck.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/13 18:30:20
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using System.IO;
using OfficeOpenXml;

public class ResCheck
{
    public static List<string> SuffixFiles = new List<string>() { ".xbm", ".tif", ".pjp", ".svgz", ".jpg", ".jpeg", ".ico", ".tiff", ".gif", ".svg", ".jfif", ".webp", ".png", ".bmp", ".avif" };

    public static List<BigFileRecord> Records = new List<BigFileRecord>();

    //[MenuItem("Res/Check")]
    public static void CheckRes()
    {
        Records.Clear();
        EditorCoroutineUtility.StartCoroutineOwnerless(DoCheck());
        EditorUtility.ClearProgressBar();
    }

    static IEnumerator DoCheck()
    {
        yield return EditorCoroutineUtility.StartCoroutineOwnerless(CheckDirectory("Assets/ArtWorkPlace"));
        yield return EditorCoroutineUtility.StartCoroutineOwnerless(CheckDirectory("Assets/Res"));
        yield return EditorCoroutineUtility.StartCoroutineOwnerless(CheckDirectory("Assets/Resources"));
        yield return EditorCoroutineUtility.StartCoroutineOwnerless(CheckDirectory("Assets/Resources_moved"));
        WriteExcel();
        AssetDatabase.Refresh();
        Debug.LogError("导出成功");
    }

    public static IEnumerator CheckDirectory(string DirectoryName)
    {
        string[] files = System.IO.Directory.GetFiles(DirectoryName);
        int len = files.Length;
        string filePath = string.Empty;
        for (int i = 0; i < files.Length; i++)
        {
            filePath = files[i];
            EditorUtility.DisplayProgressBar($"检测目录{DirectoryName}", $"当前检测文件{filePath}", i * 1.0f / (len - 1));

            string file = System.IO.Path.GetExtension(filePath);
            if (file == ".cs")
            {
                continue;
            }
            if (file == ".meta")
            {
                continue;
            }
            if (file == ".prefab")
            {
                continue;
            }
            if (file == ".asset")
            {
                continue;
            }
            if (file == ".txt")
            {
                continue;
            }
            if (file == ".json")
            {
                continue;
            }
            if (file == ".xml")
            {
                continue;
            }
            if (file == ".mat")
            {
                continue;
            }
            if (IsPicture(file))
            {

                //byte[] datas = System.IO.File.ReadAllBytes(filePath);

                //Texture2D texture2D = new Texture2D(0, 0);
                //texture2D.LoadImage(datas);

                TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;
                int width = 0;
                int height = 0;
                GetTextureOriginalSize(textureImporter, out width, out height);
                if (IsBigImage(width, height))
                {
                    Records.Add(new BigFileRecord() { Path = filePath, Width = width, Height = height });
                    Debug.LogError($"{filePath} width:{width} height:{height}");

                    TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                    platformSettings.format = TextureImporterFormat.ASTC_6x6;
                    platformSettings.name = "Android";
                    platformSettings.overridden = true;
                    platformSettings.textureCompression = TextureImporterCompression.Compressed;
                    textureImporter.SetPlatformTextureSettings(platformSettings);

                    textureImporter.SaveAndReimport();
                    AssetDatabase.ImportAsset(filePath);

                }

            }
            yield return null;
        }
        EditorUtility.ClearProgressBar();

        string[] dirs = System.IO.Directory.GetDirectories(DirectoryName);
        if (dirs != null)
        {
            foreach (var item in dirs)
            {
                yield return EditorCoroutineUtility.StartCoroutineOwnerless(CheckDirectory(item));
            }
        }
    }

    private static void WriteExcel()
    {
        //string excelName, string sheetName
        string dir = Application.dataPath + $"/../";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string path = dir + "/资源检测.xlsx";
        FileInfo newFile = new FileInfo(path);
        if (newFile.Exists)
        {
            //创建一个新的excel文件
            newFile.Delete();
            newFile = new FileInfo(path);
        }

        //通过ExcelPackage打开文件
        using (ExcelPackage package = new ExcelPackage(newFile))
        {

            //在excel空文件添加新sheet
            ExcelWorksheet monstersheet = package.Workbook.Worksheets.Add("布怪");
            int index = 1;
            monstersheet.Cells[1, index++].Value = "序号";
            monstersheet.Cells[1, index++].Value = "路径";
            monstersheet.Cells[1, index++].Value = "宽";
            monstersheet.Cells[1, index++].Value = "高";

            if (Records != null && Records.Count > 0)
            {
                int row = 2;
                foreach (var item in Records)
                {
                    int indexx = 1;
                    monstersheet.Cells[row, indexx++].Value = row - 1;
                    monstersheet.Cells[row, indexx++].Value = item.Path;
                    monstersheet.Cells[row, indexx++].Value = item.Width;
                    monstersheet.Cells[row, indexx++].Value = item.Height;
                  //  monstersheet.Cells[row, 3].Style = new OfficeOpenXml.Style.ExcelStyle() { Font };
                    row++;
                }
            }
            monstersheet.Cells.AutoFitColumns();
            //保存excel
            package.Save();
        }
    }


    static string GetDownloadSize(long size)
    {
        int cnt = 0;
        float temp = size;
        while (temp > 1024)
        {
            temp /= 1024;
            ++cnt;
        }

        if (cnt >= 3)
        {
            return $"{temp.ToString("F2")}GB";
        }
        else if (cnt >= 2)
        {
            return $"{temp.ToString("F2")}MB";
        }
        else if (cnt >= 1)
        {
            return $"{temp.ToString("F2")}KB";
        }
        else
        {
            return $"{temp.ToString("F2")}B";
        }
    }
    public static bool IsBigImage(int width, int height)
    {
        if (width >= 1024)
        {
            return true;
        }

        if (height >= 1024)
        {
            return true;
        }

        return false;
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



    public static bool IsPicture(string suffix)
    {
        if (SuffixFiles.Contains(suffix))
        {
            return true;
        }
        return false;
    }

    public class BigFileRecord
    {
        public string Path;
        public int Width;
        public int Height;
    }
}
