using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 所有AOI.json中props属性导入到AOIAttrDefine中
/// </summary>
public static class AOIJsonToDefine
{
    private static string LOG_TAG = "[AOIJsonToDefine]";

    private static readonly string m_TargetFilePath = "Assets/Scripts/StarGame/AOIAttrDefine.cs";
    private static string mTemplate;
    private static List<string> m_PropAttrs = new() { };


    [MenuItem("自动化工具/AOI JSON/AOIJsonToDefine", false, 2)]
    public static void GenerateAOIJsonToDefine()
    {
        LocalDataManager.Instance.Init();
        mTemplate = File.ReadAllText("Assets/Editor/AOIJsonToData/AOIJsonToDefineTemp.cs.txt", System.Text.Encoding.UTF8);
        // -----------遍历缓存：Player\Battle\GameStatus\TinyEntity
        CacheAOIJsons();

        UpdateFile(m_TargetFilePath, "#FILEDS#");

        Release();
    }

    #region 【缓存AOI Json文件】
    private static void CacheAOIJsons()
    {
        m_PropAttrs.Clear();
        CachePlayerJson();
        CacheItemJson();
        CacheBattleJson();
        CacheTinyEntityJson();
        CacheGameStatusJson();
    }

    private static void CachePlayerJson()
    {
        PlayerPropInfo props = LocalDataManager.Instance.M_PlayerInfoData.props;
        PropertyInfo[] propList = props.GetType().GetProperties();
        CacheProp(propList);
    }

    private static void CacheItemJson()
    {
        ItemPropInfo props = LocalDataManager.Instance.M_ItemInfoData.props;
        PropertyInfo[] propList = props.GetType().GetProperties();
        CacheProp(propList);
    }

    private static void CacheBattleJson()
    {
        BattlePropInfo props = LocalDataManager.Instance.M_BattleInfoData.props;
        PropertyInfo[] propList = props.GetType().GetProperties();
        CacheProp(propList);
    }

    private static void CacheTinyEntityJson()
    {
        TinyEntityPropInfo props = LocalDataManager.Instance.M_TinyEntityInfoData.props;
        PropertyInfo[] propList = props.GetType().GetProperties();
        CacheProp(propList);
    }

    private static void CacheGameStatusJson()
    {
        GameStatePropInfo props = LocalDataManager.Instance.M_GameStateInfoData.props;
        PropertyInfo[] propList = props.GetType().GetProperties();
        CacheProp(propList);
    }

    private static void CacheProp(PropertyInfo[] propList)
    {
        foreach (PropertyInfo p in propList)
        {
            if (!m_PropAttrs.Contains(p.Name))
            {
                m_PropAttrs.Add(p.Name);
            }
        }
    }

    #endregion
    /// <summary>
    /// 修改文件中的部分
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="target">要修改的值</param>
    private static void UpdateFile(string path, string target)
    {
        Debug.Log($"{LOG_TAG} ------------------------------UpdateFile-start--path={path}");
        // 处理要修改的文件
        string results = "";
        for (int i = 0; i < m_PropAttrs.Count; i++)
        {
            string str = m_PropAttrs[i];
            VitalSignAOIClientAttrs attrsPropertie = LocalDataManager.Instance.GetAttrPropByName(str);
            if (attrsPropertie == null)
            {
                Debug.LogError($"{LOG_TAG} ------------------------------UpdateFile-str={str},attrsPropertie=null err!!!");
                continue;
            }
            if (i == 0)
            {
                results = $" // {attrsPropertie.Name}:{attrsPropertie.index}:{attrsPropertie.type}:{attrsPropertie.desc}";
            }
            else
            {
                results += $"\n         // {attrsPropertie.Name}:{attrsPropertie.index}:{attrsPropertie.type}:{attrsPropertie.desc}";
            }
            results += $"\n         public const string {str} = \"{str}\";\n";
        }

        results += $"\n         public const string Error = \"Error\";";

        mTemplate = Regex.Replace(mTemplate, target, results);
        // 保存并关闭文件
        // 创建一个新文件，向其中写入指定的字符串，然后关闭文件。 如果目标文件已存在，则覆盖该文件。
        File.WriteAllText(path, mTemplate);
        Debug.Log($"{LOG_TAG} ------------------------------UpdateFile-end--path={path}");
    }

    private static void Release()
    {
        mTemplate = null;
        m_PropAttrs.Clear();
    }
}


