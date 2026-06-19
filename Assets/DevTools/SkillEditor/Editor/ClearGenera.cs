///--------------------------------------------------------------------
/// 文件名   :   ClearGenera.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/23 13:57:13
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public static class ClearGenera
{
    /// <summary>
    /// 剔除文件，手写的代码排除外,生成的枚举文件
    /// </summary>
    private static List<string> Filters = new List<string>() {

        "SkillEditorDefine",
        "DefualtKey",
        "InputKeySerialize",
        "NullToEmptyStringResolver",
        "TimelineConfig"
    };

    private static string DefineDir = "DevTools/SkillEditor/Scripts/Define";
    private static string SerializeDir = "DevTools/SkillEditor/Scripts/Serialize";
    private static string BaseDataConfigPath = "Assets/DevTools/SkillEditor/Config/BaseDataConfig.xml";
    private static List<string> ClassNames = new List<string>();

    private static List<string> ClearFiles = new List<string>();

    public static void ExecuteClear()
    {
        ClassNames.Clear();
        ClearFiles.Clear();
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(BaseDataConfigPath);
        if (textAsset != null && textAsset.text != null)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(textAsset.text);

            XmlNode xmlNode = document.SelectSingleNode("config");
            if (xmlNode != null && xmlNode.ChildNodes.Count > 0)
            {


                foreach (var item in xmlNode.ChildNodes)
                {

                    XmlElement node = item as XmlElement;
                    if (node == null || node.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }
                    string ClassName = node.GetAttribute("name");
                    if(string.IsNullOrEmpty(ClassName))
                    {
                        continue;
                    }
                    ClassNames.Add(ClassName);

                }
            }
        }
        ClearDefine();
        ClearSerialize();
        DeleateFile();
        ClassNames.Clear();
        ClearFiles.Clear();
    }


    private static void ClearDefine()
    {
        string dirPath = UnityEngine.Application.dataPath + "/"+ DefineDir;
        string[] files= System.IO.Directory.GetFiles(dirPath);
        foreach (var item in files)
        {
            if(item.EndsWith(".cs"))
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(item);

                if (Filters.Contains(fileName))
                {
                    continue;
                }

                if (ClassNames.Contains(fileName))
                {
                    continue;
                }
                ClearFiles.Add(item);

            }
        }
    }
    private static void ClearSerialize()
    {
        string dirPath = UnityEngine.Application.dataPath + "/" + SerializeDir;
        string[] files = System.IO.Directory.GetFiles(dirPath);
        foreach (var item in files)
        {
            if (item.EndsWith(".cs"))
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(item);

                if (Filters.Contains(fileName))
                {
                    continue;
                }

                if (ClassNames.Contains(fileName))
                {
                    continue;
                }
                ClearFiles.Add(item);

            }
        }
    }

    private static void DeleateFile()
    {
        if(ClearFiles==null || ClearFiles.Count<1)
        {
            return;
        }
        foreach (var item in ClearFiles)
        {
            System.IO.File.Delete(item);
        }

        UnityEditor.AssetDatabase.Refresh();
    }
}
