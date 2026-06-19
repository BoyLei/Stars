///--------------------------------------------------------------------
/// 文件名   :   ExcelToJson
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/20 15:46:34
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

public class ExcelToJson : EditorWindow
{
    [MenuItem("Tools/Excel/Model", false, 100)]
    static void OpenWindow()
    {
        ExcelToJson excel = GetWindow<ExcelToJson>();
        excel.Show();
    }

    public string FilePath;


    static Models models=new Models();


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
        models.ModelDatas.Clear();
        DataSet data = ReadBook(FilePath);
        if (data != null)
        {
            for (int i = 0; i < data.Tables.Count; i++)
            {
                DataTable sheet = data.Tables[i];
                if (sheet.TableName == "data")
                {
                    continue;
                }
                ReadTable(sheet);
            }
        }
    }

    private static void ReadTable(DataTable sheet)
    {
        if (sheet == null)
        {
            return;
        }
        var colCount = sheet.Columns.Count;
        var rowCount = sheet.Rows.Count;
        if (rowCount < 6)
        {
            return;
        }


        for (int i = 5; i < rowCount; i++)
        {
            DataRow data = sheet.Rows[i];
            if (data.IsNull(0))
            {
                continue;
            }
            ModelJson model = new ModelJson();
            model.Init(data);
            models.ModelDatas.Add(model);
        }
    }

    public void Execute()
    {
        string mContent = JsonUtility.ToJson(models);
        string filePath = OutputDir + "/" + FileName + ".json";
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileStream fs = new FileStream(filePath, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(mContent);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
        AssetDatabase.Refresh();
    }

    private void Write(FileData fileData)
    {
        string mContent = string.Empty;
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
    }

    public void OnDestroy()
    {
        FilePath = string.Empty;
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
        GUILayout.EndHorizontal();

    }
}

[System.Serializable]
public class ModelJson
{
    public int ID;
    public string Desc;
    public string AssetPath;

    public void Init(DataRow data)
    {
        int id = 0;
        int.TryParse(data[0].ToString(), out id);
        ID = id;

        Desc = data[1].ToString();
        AssetPath = data[2].ToString();
    }
}

public class Models
{
    public List<ModelJson> ModelDatas = new List<ModelJson>();
}