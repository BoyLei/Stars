using MessagePack;
using Newtonsoft.Json;
using SkillEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Task;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
/// <summary>
/// Addressables中动画，模型，特效，路径导出到AssetsToDefine中
/// </summary>
public static class AssetsToDefine
{
    private static string LOG_TAG = "[AssetsToDefine]";

    private static readonly string m_TargetFilePath = "Assets/Scripts/StarGame/AssetsToDefine.cs";
    private static readonly string m_TargetFilePath2 = "Assets/Res/Config/AssetsDefine/";

    private static string mTemplate;

    private static List<string> m_AnimationPathList = new();
    private static List<string> m_RolesPathList = new();
    private static List<string> m_EffectPathList = new();


    [MenuItem("Build/资源/动态资源路径导出", false, 0)]
    public static void GenerateAssetsToDefine()
    {
        mTemplate = File.ReadAllText("Assets/Editor/AssetsToData/AssetsToDefineTemp.cs.txt", System.Text.Encoding.UTF8);

        CacheAssets();

        //// 1.写到静态的cs类里
        //{
        //    UpdateFile(m_AnimationPathList, "AnimationPathList", "#动画#");

        //    UpdateFile(m_RolesPathList, "RolePathList", "#模型#");

        //    UpdateFile(m_EffectPathList, "EffectPathList", "#特效#");
            
        //    WriteAllText();
        //}

        // 2.写成json+messagepack
        {
            StarProjectDef.AssetsToDefine configs = new();

            configs.AnimationPathList.Clear();
            foreach (var item in m_AnimationPathList)
            {
                configs.AnimationPathList.Add(item);
            }

            configs.RolePathList.Clear();
            foreach (var item in m_RolesPathList)
            {
                configs.RolePathList.Add(item);
            }

            configs.EffectPathList.Clear();
            foreach (var item in m_EffectPathList)
            {
                configs.EffectPathList.Add(item);
            }

            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string content = Newtonsoft.Json.JsonConvert.SerializeObject(configs, setting);
            //WriteJson(content, m_TargetFilePath2);

            WriteMessagePackBytes(configs, m_TargetFilePath2);

            UnityEditor.AssetDatabase.Refresh();

        }

        Release();
    }

    #region 【缓存资源文件路径】
    private static void CacheAssets()
    {
        // 缓存所有 【animation】 的资源路径
        InitCacheAnimation();
        // 缓存所有 【roles】 的资源路径
        InitCacheRoles();
        // 缓存所有 【effect】 的资源路径
        InitCacheEffect();
    }

    private static void InitCacheAnimation()
    {
        m_AnimationPathList = new List<string>();
        List<string> keys = new() { "animation" };
        var list = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Intersection, null).WaitForCompletion();
        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string path = list[i].ToString();
                path = path.Replace("Assets/Res/", string.Empty);
                path = path.Replace(".anim", string.Empty);
                path = path.ToLower();
                m_AnimationPathList.Add(path);
            }
        }
    }

    // 缓存所有 【roles】 的资源路径
    private static void InitCacheRoles()
    {
        m_RolesPathList = new List<string>();
        List<string> keys = new() { "roles" };
        var list = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Intersection, null).WaitForCompletion();
        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string path = list[i].ToString();
                path = path.Replace("Assets/Res/", string.Empty);
                path = path.Replace(".prefab", string.Empty);
                path = path.ToLower();
                m_RolesPathList.Add(path);
            }
        }
    }

    // 缓存所有 【effect】 的资源路径
    private static void InitCacheEffect()
    {
        m_EffectPathList = new List<string>();
        List<string> keys = new() { "effect" };
        var list = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Intersection, null).WaitForCompletion();
        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string path = list[i].ToString();
                path = path.Replace("Assets/Res/", string.Empty);
                path = path.Replace(".prefab", string.Empty);
                path = path.ToLower();
                m_EffectPathList.Add(path);
            }
        }
    }

    #endregion

    /// <summary>
    /// 修改文件中的部分
    /// </summary>
    /// <param name="variateName">变量名</param>
    /// <param name="target">要修改的值</param>
    private static void UpdateFile(List<string> list,string variateName, string target)
    {
        Debug.Log($"{LOG_TAG} ------------------------------UpdateFile-start--path={m_TargetFilePath},target={target},count={list.Count}");
        // 处理要修改的文件
        string results = "";

        results = $"// {target} 路径";
        results += $"\n        public static readonly HashSet<string> {variateName} = new HashSet<string>";
        results += "\n        {";

        for (int i = 0; i < list.Count; i++)
        {
            string str = list[i];
            results += $"\n            \"{str}\",";
        }

        results += "\n        };";

        mTemplate = Regex.Replace(mTemplate, target, results);

        Debug.Log($"{LOG_TAG} ------------------------------UpdateFile-end--path={m_TargetFilePath},target={target},count={list.Count}");
    }

    private static void WriteAllText()
    {
        // 保存并关闭文件
        // 创建一个新文件，向其中写入指定的字符串，然后关闭文件。 如果目标文件已存在，则覆盖该文件。
        File.WriteAllText(m_TargetFilePath, mTemplate);
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

        FileStream fs = new FileStream(path, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(SkillEditorUtils.ConvertJsonString(content));
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
#endif
    }

    private static void WriteMessagePackBytes(StarProjectDef.AssetsToDefine configs, string path)
    {
        path += "AssetsDefine.bytes";

#if UNITY_EDITOR

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        byte[] byteArrary = MessagePackSerializer.Serialize<StarProjectDef.AssetsToDefine>(configs);
        System.IO.File.WriteAllBytes(path, byteArrary);
#endif
    }



    #endregion

    private static void Release()
    {
        mTemplate = null;
        m_AnimationPathList.Clear();
        m_RolesPathList.Clear();
        m_EffectPathList.Clear();
    }
}



