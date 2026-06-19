using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class Json2List
{
    private const string LOG_TAG = "[Json2List]";

    private static string TargetFolderPath = "Assets/Scripts/StarGame/Service/LocalDataManager/JsonConfigList";

    private static string templateStr =
@"using System.Collections.Generic;
/// 注意,此脚本为 Json2List.cs 动态生成
public static class #CLASSNAME#List
{
    public static List<string> infos = new List<string>();

    public static string basePath = #BASEPATH#;

    static #CLASSNAME#List()
    {
        #FILEDS#
    }
}
";


    [MenuItem("Assets/GenerateJsonList")]
    public static void LogSelect()
    {
        string path = GetCurrentAssetDirectory();
        GenerateJsonList(path);
    }

    public static void GenerateJsonList(string path)
    {
        Debug.LogError($"{LOG_TAG} GenerateJsonList [path] : {path}");
        ReadFolderFiles(path, "json");
        Debug.LogError($"{LOG_TAG} GenerateJsonList success !!!");

    }

    [System.Obsolete]//标记该方法已弃用
    public static void GenerateSkillJsonList()
    {
        string path = SkillEditorGlobal.Instance.SkillConfigPath;
        GenerateJsonList(path);
    }

    public static string GetCurrentAssetDirectory()
    {
        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                continue;

            if (System.IO.Directory.Exists(path))
                return path;
            else if (System.IO.File.Exists(path))
                return System.IO.Path.GetDirectoryName(path);
        }

        return "Assets";
    }

    public static void ReadFolderFiles(string path, string suffix)
    {
        DirectoryInfo root = new DirectoryInfo(path);
        FileInfo[] fileInfos = root.GetFiles();

        List<string> fileNameList = new List<string>();

        foreach (FileInfo f in fileInfos)
        {
            string filename = f.Name;

            if (filename.EndsWith(suffix))//判断文件后缀，并获取指定格式的文件全路径增添至fileList
            {
                fileNameList.Add(filename);
                Debug.Log($"{LOG_TAG} ReadFolderFiles [file] : {filename}");
            }
        }

        if (fileNameList.Count == 0)
        {
            return;
        }
        string FolderName = root.Name;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < fileNameList.Count; i++)
        {
            string[] fileStr = fileNameList[i].Split('.');
            if (i == 0)
            {
                sb.Append(string.Format("   infos.Add(\"{0}\");\n", fileStr[0]));
            }
            else
            {
                sb.Append(string.Format("           infos.Add(\"{0}\");\n", fileStr[0]));
            }
        }
        StringBuilder strSb = new StringBuilder();
        strSb.Append(templateStr);
        strSb.Replace("#CLASSNAME#", FolderName);
        strSb.Replace("#FILEDS#", sb.ToString());

        string basePath = "";
        int startIndex = path.IndexOf('/') + 1;
        int gangIndex = path.IndexOf('/', startIndex) + 1;
        //Debug.LogWarning($"{LOG_TAG} ReadFolderFiles path={path}");
        //Debug.LogWarning($"{LOG_TAG} ReadFolderFiles startIndex={startIndex},gangIndex={gangIndex},Length={path.Length - 1}");
        basePath = path.Substring(gangIndex);
        //Debug.LogWarning($"{LOG_TAG} ReadFolderFiles basePath={basePath}");
        basePath = $"\"{basePath}/\"";
        strSb.Replace("#BASEPATH#", basePath);
        string filePath = $"{TargetFolderPath}/{FolderName}List.cs";
        // 保存并关闭文件
        // 创建一个新文件，向其中写入指定的字符串，然后关闭文件。 如果目标文件已存在，则覆盖该文件。
        File.WriteAllText(filePath, strSb.ToString());
        // Debug.Log($"{LOG_TAG} ReadFolderFiles export success,path={path}");
    }
}
