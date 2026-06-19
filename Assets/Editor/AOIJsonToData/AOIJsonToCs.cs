using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 所有AOI的json转换成cs类
/// </summary>
public static class AOIJsonToCs
{
    private static string LOG_TAG = "[AOIJsonToCs]";

    private static readonly string m_TargetDir = "Assets/Res/Config/AOIJson";
    private static readonly string m_OutputDir = "Assets/Scripts/StarGame/Service/LocalDataManager/AOIData";
    private static string mTemplate;
    public static Dictionary<string, FileData2> FileClass = new Dictionary<string, FileData2>();

    [MenuItem("自动化工具/AOI JSON/AOIJsonToCs", false, 3)]

    public static void GenerateAOIJsonToCs()
    {
        mTemplate = File.ReadAllText("Assets/Editor/AOIJsonToData/AOIJsonToCsTemp.cs.txt", System.Text.Encoding.UTF8);

        GetFileName(m_TargetDir, "json");

        Execute();

        Release();
    }

    /// 获得指定路径下所有文件名
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="suffix">文件后缀</param>
    private static void GetFileName(string path, string suffix)
    {
        FileClass.Clear();
        DirectoryInfo root = new DirectoryInfo(path);
        foreach (FileInfo f in root.GetFiles())
        {
            string filename = f.Name;
            if (filename.EndsWith(suffix))//判断文件后缀，并获取指定格式的文件全路径增添至fileList
            {
                Debug.Log($"{LOG_TAG} GetFileName filename={filename}");
                FileData2 fileData = ReadTable(f);
                if (fileData != null)
                {
                    FileClass.Add(fileData.FileName, fileData);
                }
            }
        }
    }

    private static FileData2 ReadTable(FileInfo fileInfo)
    {
        if (fileInfo == null)
        {
            return null;
        }
        FileData2 fileData = new FileData2();
        string filename = fileInfo.Name;
        filename = filename.Replace(".json", string.Empty);
        fileData.FileName = filename;
        fileData.fields = new List<FieldData2>();
        fileData.fields.Clear();
        //--------------------- 遍历props
        //逐行读取文件处理至文件结束
        bool isBegin = false;   // 是否开始遍历
        bool isFieldBegin = false;  // 是否段落开始
        StreamReader streamReader = fileInfo.OpenText();
        string str = string.Empty;
        while ((str = streamReader.ReadLine()) != null)
        {
            // 标记开始
            if (str.Contains("\"props\":") && !isBegin)
            {
                isBegin = true;
                //Debug.Log($"{LOG_TAG} filename={filename},---------------------isBegin={isBegin}");
            }
            else if (isBegin)
            {
                if (str.Contains("{") && !isFieldBegin)
                {
                    string attrStr = SubStringMiddle(str, "\"", "\"");
                    FieldData2 fieldData2 = new FieldData2();
                    fieldData2.FieldName = attrStr;
                    fileData.fields.Add(fieldData2);
                    isFieldBegin = true;
                    //Debug.Log($"{LOG_TAG} filename={filename},---------------------attrStr={attrStr}");
                }
                else if (str.Contains("},") && isFieldBegin)
                {
                    isFieldBegin = false;
                }
                else if (str.Contains("}") && isFieldBegin)
                {
                    break;
                }
            }
        }
        return fileData;
    }

    /// <summary>
    /// 截取字符串中两个指定字符串中间部分
    /// </summary>
    /// <param name="origin">字符</param>
    /// <param name="startString">开始的特定字符</param>
    /// <param name="endString">结束的特定字符</param>
    /// <returns></returns>
    private static string SubStringMiddle(string origin, string startString, string endString)
    {
        int start, end;
        start = origin.IndexOf(startString, 0) + startString.Length;
        end = origin.IndexOf(endString, start);
        return origin.Substring(start, end - start);
    }

    private static void Execute()
    {
        if (FileClass != null && FileClass.Count > 0)
        {
            Debug.Log($"{LOG_TAG} ------------------------------Execute-start--Count={FileClass.Count}");
            foreach (var item in FileClass)
            {
                Write(item.Value);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"{LOG_TAG} ------------------------------Execute-end");
    }

    private static void Write(FileData2 fileData)
    {
        string mContent = mTemplate;
        string className = fileData.FileName;
        mContent = mContent.Replace("#CLASSNAME#", className);
        mContent = mContent.Replace("#FILEDS#", fileData.FieldToString());
        string filePath = m_OutputDir + "/" + className + "Info.cs";

        // 保存并关闭文件
        // 创建一个新文件，向其中写入指定的字符串，然后关闭文件。 如果目标文件已存在，则覆盖该文件。
        File.WriteAllText(filePath, mContent);
    }

    private static void Release()
    {
        mTemplate = null;
        FileClass.Clear();
    }

}

public class FileData2
{
    public string FileName;
    public List<FieldData2> fields;

    public string FieldToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var item in fields)
        {
            if (item.IsNull())
            {
                continue;
            }
            stringBuilder.Append("\n");
            stringBuilder.Append(item.FiledToString());
        }

        return stringBuilder.ToString();
    }
}

public class FieldData2
{
    public string FieldName;

    public bool IsNull()
    {
        return string.IsNullOrEmpty(FieldName);
    }
    public string FiledToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Format("        public VitalSignAOIAttrs {0} {1}\n", FieldName, "{get; set;}"));
        return sb.ToString();
    }
}

