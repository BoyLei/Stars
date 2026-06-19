using MessagePack;
using Newtonsoft.Json;
using SGF.UI.Framework;
using SkillEditor;
using StarProjectDef;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class UIQueueToDefine
{
    private static string LOG_TAG = "[UIQueueToDefine]";

    private static readonly string m_PrefabFilePath = "Assets/Res/Config/UIQueue/";
    private static readonly string m_PrefabFilePath2 = "Assets/Res/UI";


    private static string mTemplate;


    [MenuItem("Build/UI界面/导出配置", false, 0)]
    public static void GenerateUIQueueToDefine()
    {
        // 2.写成json+messagepack
        {
            ExportPrefabText();
        }

    }


    public static void ExportPrefabText()
    {
        string[] prefabFiles = Directory.GetFiles(m_PrefabFilePath2, "*.prefab", SearchOption.AllDirectories);
        StarProjectDef.UIQueueToDefine configs = new();
        foreach (string prefabPath in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                UIPanel uIPanelComp = prefab.GetComponent<UIPanel>();
                if (uIPanelComp != null)
                {
                    string path = prefabPath.Replace("Assets/Res/UI\\", string.Empty).Replace(".prefab", string.Empty).Replace("\\", "/").ToLower();
                    UIQueueCfgData uIQueueCfgData = new(path, uIPanelComp.UISeatType, (int)uIPanelComp.QueuePriorityType, uIPanelComp.IsLongTime);
                    configs.UIQueueCfgDatas.Add(path, uIQueueCfgData);
                    Debug.Log($"路径 path={path}");
                }
            }
        }

        JsonSerializerSettings setting = new();
        setting.NullValueHandling = NullValueHandling.Ignore;
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(configs, setting);
        //WriteJson(content, m_TargetFilePath2);

        WriteMessagePackBytes(configs, m_PrefabFilePath);
        UnityEditor.AssetDatabase.Refresh();
    }

    #region 方案二

    private static void WriteJson(string content, string path)
    {
        path += "AssetsDefine.json";
#if UNITY_EDITOR

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        FileStream fs = new(path, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(SkillEditorUtils.ConvertJsonString(content));
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
#endif
    }

    private static void WriteMessagePackBytes(StarProjectDef.UIQueueToDefine configs, string path)
    {
        path += "UIQueueToDefine.bytes";

#if UNITY_EDITOR

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        byte[] byteArrary = MessagePackSerializer.Serialize<StarProjectDef.UIQueueToDefine>(configs);
        System.IO.File.WriteAllBytes(path, byteArrary);
#endif
    }

    #endregion

    private static void Release()
    {
        mTemplate = null;
    }
}




