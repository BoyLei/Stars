///--------------------------------------------------------------------
/// 文件名   :   ExcelToCsharp
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   #CREATETIME#
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Data;
using System.IO;
using Excel;
using System;
using System.Text;

public class ExcelToCsharp : EditorWindow
{
    [MenuItem("Tools/Excel/ExcelToCS", false, 100)]
    static void OpenWindow()
    {
        ExcelToCsharp excel = GetWindow<ExcelToCsharp>();
        excel.Show();
    }

    public string FilePath;

    public Dictionary<string, FileData> FileClass = new Dictionary<string, FileData>();

    public string mTemplate;

    public string FileName;

    public string OutputDir;

    public void SelectDir()
    {
        string path = EditorUtility.SaveFolderPanel("数据输出文件目录", "", "");
        OutputDir = path;
        PlayerPrefs.SetString("OutputDir", path);
    }

    public void SelectFile()
    {
        FilePath = EditorUtility.OpenFilePanel("选择Excel文件", Application.dataPath, "xlsx");
        FileName = System.IO.Path.GetFileNameWithoutExtension(FilePath);
        Read();
    }

    public void Read()
    {
        FileClass.Clear();
        DataSet data = ReadBook(FilePath);
        if (data != null)
        {
            for (int i = 0; i < data.Tables.Count; i++)
            {
                DataTable sheet = data.Tables[i];
                FileData fileData = ReadTable(sheet);
                if (fileData != null)
                {
                    FileClass.Add(fileData.FileName, fileData);
                }
            }
        }
    }

    private static FileData ReadTable(DataTable sheet)
    {
        if (sheet == null)
        {
            return null;
        }

        var colCount = sheet.Columns.Count;
        var rowCount = sheet.Rows.Count;
        if (colCount < 1)
        {
            return null;
        }

        if (rowCount < 3)
        {
            return null;
        }

        FileData fileData = new FileData();
        fileData.FileName = sheet.TableName;
        fileData.fields = new List<FieldData>();
        fileData.fields.Clear();
        for (int i = 0; i < colCount; i++)
        {
            FieldData fieldData = new FieldData();
            for (int j = 0; j < 3; j++)
            {
                DataRow dataRow = sheet.Rows[j];
                if (j == 0)
                {
                    fieldData.FieldDesc = dataRow[i].ToString();
                }
                else if (j == 1)
                {
                    fieldData.FieldName = dataRow[i].ToString();
                }
                else if (j == 2)
                {
                    fieldData.FieldType = dataRow[i].ToString();
                }
            }

            fileData.fields.Add(fieldData);
        }

        fileData.Key = fileData.fields[0].FieldType;
        return fileData;
    }

    public void Execute()
    {
        if (FileClass != null && FileClass.Count > 0)
        {
            foreach (var item in FileClass)
            {
                Write(item.Value);
            }
        }

        AssetDatabase.Refresh();
    }

    public void ToXml()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("<config>\n");
        if (FileClass != null && FileClass.Count > 0)
        {
            foreach (var item in FileClass)
            {
                stringBuilder.Append(WriteXml(item.Value));
            }
        }

        stringBuilder.Append("</config>\n");

        string filePath = OutputDir + "/" + "FileName.xml";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        FileStream fs = new FileStream(filePath, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
        AssetDatabase.Refresh();
    }

    private string WriteXml(FileData fileData)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string className = fileData.FileName == "data" ? FileName : fileData.FileName;
        stringBuilder.Append(string.Format("<enum name=\"{0}\" desc=\"{1}\"> \n", className, className));
        stringBuilder.Append(fileData.FieldToXml());
        stringBuilder.Append("</enum>\n");
        return stringBuilder.ToString();
    }


    private void Write(FileData fileData)
    {
        string mContent = mTemplate;
        string className = fileData.FileName == "data" ? FileName : fileData.FileName;
        mContent = mContent.Replace("#CLASSNAME#", className);
        mContent = mContent.Replace("#FILEDS#", fileData.FieldToString());
        mContent = mContent.Replace("#KEY#", fileData.Key);
        mContent = mContent.Replace("#CREATETIME#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        mContent = mContent.Replace("#AUTHOR#", System.Environment.UserName);
        string filePath = OutputDir + "/" + className + "Data.cs";
        FileStream fs = new FileStream(filePath, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(mContent);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
    }

    public void OnEnable()
    {
        OutputDir = PlayerPrefs.GetString("OutputDir", Application.dataPath);
        mTemplate = File.ReadAllText("Assets/Editor/ExcelToData/Template.cs.txt", System.Text.Encoding.UTF8);
    }

    public void OnDestroy()
    {
        FilePath = string.Empty;
        FileClass.Clear();
        mTemplate = string.Empty;
        FileName = string.Empty;
    }


    public static DataSet ReadBook(string path)
    {
        DataSet dataSet = null;
        try
        {
            FileStream mStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(mStream);
            dataSet = mExcelReader.AsDataSet();
        }
        catch (Exception e)
        {
            Debug.LogError("文件读取失败，" + e.ToString());
        }

        return dataSet;
    }


    public void OnGUI()
    {
        GUILayout.BeginHorizontal("box");
        EditorGUILayout.TextField("文件路径", FilePath);
        if (GUILayout.Button("选择"))
        {
            SelectFile();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        EditorGUILayout.TextField("输出路径", OutputDir);
        if (GUILayout.Button("选择"))
        {
            SelectDir();
        }

        GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal("box");
        //GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("执行"))
        {
            Execute();
        }

        if (GUILayout.Button("输出XML"))
        {
            ToXml();
        }

        GUILayout.EndHorizontal();
    }
}

public class FileData
{
    public string FileName;
    public string Key;
    public List<FieldData> fields;

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

    public string FieldToXml()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var item in fields)
        {
            if (item.IsNull())
            {
                continue;
            }

            stringBuilder.Append(item.FiledToXml());
        }

        return stringBuilder.ToString();
    }
}

public class FieldData
{
    public string FieldName;
    public string FieldType;
    public string FieldDesc;

    public bool IsNull()
    {
        return string.IsNullOrEmpty(FieldName) || string.IsNullOrEmpty(FieldType) || string.IsNullOrEmpty(FieldDesc);
    }

    public string FiledToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("        /// <summary>\n");
        sb.Append("        /// " + FieldDesc + "\n");
        sb.Append("        /// <summary>\n");
        sb.Append(string.Format("        public {0} {1} {2}\n", GetFileType(), FieldName, "{get; set;}"));
        return sb.ToString();
    }

    public string FiledToXml()
    {
        return
            string.Format(" <item name=\"{0}\" desc=\"{1}\"  type=\"{2}\"/> \n", FieldName, FieldDesc,
                GetFileType());
    }


    public string GetFileType()
    {
        string type = FieldType.ToLower();
        if (type.Contains("json"))
        {
            string _type = type.Replace("json", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            return string.Format("List<{0}>", _type);
        }

        return type;
    }
}