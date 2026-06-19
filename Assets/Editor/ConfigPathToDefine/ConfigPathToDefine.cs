using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
/// <summary>
/// AOI\EXCEL\引导\任务-》配置的路径 导出在txt，预热配置使用
/// Lua-》文本的路径 导出在txt，预热配置使用
/// </summary>
public static class ConfigPathToDefine
{
    private static string LOG_TAG = "[ConfigPathToDefine]";

    private static readonly string m_TargetFilePath = "Assets/Res/Config/ConfigPathToDefine.txt";
    private static string mTemplate;

    private static List<string> m_AOIJsonPathList = new();
    private static List<string> m_ExcelPathList = new();
    private static List<string> m_GuidePathList = new();
    private static List<string> m_TaskPathList = new();
    //private static List<string> m_LuaModulePathList = new();
    private static List<string> m_LuaPathList = new();



    [MenuItem("自动化工具/配置路径导出", false, 2)]
    public static void GenerateConfigPathToDefine()
    {
        mTemplate = File.ReadAllText("Assets/Editor/ConfigPathToDefine/ConfigPathToDefineTemp.txt", System.Text.Encoding.UTF8);

        CacheAssets();

        UpdateFile(m_GuidePathList, "#Guide#", true);
        UpdateFile(m_TaskPathList, "#Task#", true);
        UpdateFile(m_ExcelPathList, "#Excel#", true);
        UpdateFile(m_AOIJsonPathList, "#AOIJson#"); // 最后一个配置不加|

        //UpdateFile(m_LuaModulePathList, "#LuaModule#");
        UpdateFile(m_LuaPathList, "#Lua#");

        WriteAllText();

        Release();
    }

    #region 【缓存配置文件路径】
    private static void CacheAssets()
    {
        {
            // 缓存所有 【AOI】 的配置路径
            CacheFileName(m_AOIJsonPathList, "Assets/Res/Config/AOIJson", "*.json");
            // 缓存所有 【Excel】 的配置路径
            CacheFileName(m_ExcelPathList, "Assets/Res/Config/Excel", "*.json");
            // 缓存所有 【引导】 的配置路径
            CacheFileName(m_GuidePathList, "Assets/Res/Config/Guide", "*.json");
            // 缓存所有 【任务】 的配置路径
            CacheFileName(m_TaskPathList, "Assets/Res/Config/Task", "*.json");
        }

        {
            // 缓存所有 【LuaModule】 的配置路径
            //m_LuaModulePathList.Add("LuaScripts/Global.lua");
            //CacheFileName(m_LuaModulePathList, "Assets/Res/LuaScripts/LuaModule", "*.txt");
            // 缓存所有 【Lua】 的配置路径
            CacheFileName(m_LuaPathList, "Assets/Res/LuaScripts", "*.txt");
        }
    }

    /// 获得指定路径下所有文件名
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="suffix">文件后缀</param>
    private static void CacheFileName(List<string> list, string path, string suffix)
    {
        //string prefixPath = path.Replace("Assets/Res/", string.Empty) + "/";

        DirectoryInfo directory = new(path);
        FileInfo[] files = directory.GetFiles(suffix, SearchOption.AllDirectories);
        foreach (FileInfo f in files)
        {
            string filename = f.Name;
            Debug.Log($"{LOG_TAG} GetFileName filename={filename}");
            string fullName = f.FullName;
            var itemPath = fullName.Split("Assets\\Res\\");
            //if (filename.EndsWith(suffix))//判断文件后缀，并获取指定格式的文件全路径增添至fileList
            if (itemPath.Length >= 1 && itemPath[1] != null)
            {
                //string filename2 = filename.Replace(".json", string.Empty).Replace(".txt", string.Empty);
                //list.Add(prefixPath + filename2);
                string filename2 = itemPath[1].Replace("\\", "/").Replace(".json", string.Empty).Replace(".txt", string.Empty);
                list.Add(filename2);

            }
        }
        Debug.LogWarning($"{LOG_TAG} GetFileName path={path},count={list.Count}");
    }

    #endregion

    /// <summary>
    /// 修改文件中的部分
    /// </summary>
    /// <param name="variateName">变量名</param>
    /// <param name="target">要修改的值</param>
    private static void UpdateFile(List<string> list, string target, bool isForceAddSeparator = false)
    {
        Debug.Log($"{LOG_TAG} ------------------------------UpdateFile-start--path={m_TargetFilePath},target={target}");
        // 处理要修改的文件
        string results = "";

        for (int i = 0; i < list.Count; i++)
        {
            string str = list[i];
            results += $"{str}";
            if (i + 1 < list.Count || isForceAddSeparator)
            {
                results += "|";
            }
        }

        mTemplate = Regex.Replace(mTemplate, target, results);

        Debug.Log($"{LOG_TAG} ------------------------------UpdateFile-end--path={m_TargetFilePath},target={target}");
    }

    private static void WriteAllText()
    {
        // 保存并关闭文件
        // 创建一个新文件，向其中写入指定的字符串，然后关闭文件。 如果目标文件已存在，则覆盖该文件。
        File.WriteAllText(m_TargetFilePath, mTemplate);
    }

    private static void Release()
    {
        mTemplate = null;
        m_AOIJsonPathList.Clear();
        m_ExcelPathList.Clear();
        m_GuidePathList.Clear();
        m_TaskPathList.Clear();
        //m_LuaModulePathList.Clear();
        m_LuaPathList.Clear();
    }
}




