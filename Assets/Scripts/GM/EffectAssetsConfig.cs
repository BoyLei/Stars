
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Assets/Create EffectAssestConfig")]
#endif
[System.Serializable]
public class EffectAssetsConfig : ScriptableObject
{
    public List<GameObject> effects = new List<GameObject>();
    public static string AssetConfigPath = "Assets/ThirdParts/ProfilerTest/RuntimeGlobalProfiler/FxAssets.asset";

    private static EffectAssetsConfig assestConfig;
    public static EffectAssetsConfig Instance
    {
        get
        {
            if (assestConfig == null)
            {
//#if UNITY_EDITOR
                //assestConfig = AssetDatabase.LoadAssetAtPath<EffectAssetsConfig>(AssetConfigPath);
                //#else
                //#endif

                assestConfig = Resources.Load<EffectAssetsConfig>("TestAddress/FxAssets/FxAssets");
                if (assestConfig != null)
                {
                    Debug.LogError("fxasssets 资源加载成功");
                }
                else
                {
                    Debug.LogError("fxasssets 资源加载失败");
                }
            }
            return assestConfig;
        }
    }

#if UNITY_EDITOR
    [LabelText("路径")]
    [FolderPath]
    //[OnValueChanged("OnsetPath")]
    public string assetPath;

    [Button("刷新")]
    public void Refresh()
    {
        effects.Clear();
        //effects = new List<GameObject>();
        if (Directory.Exists(assetPath))
        {
            DirectoryInfo direction = new DirectoryInfo(assetPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            Debug.Log(files.Length);

            
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Name.EndsWith(".prefab"))
                {
                    continue;
                }
                Debug.Log("Name:" + files[i].Name);
                //Debug.Log( "FullName:" + files[i].FullName );
                //Debug.Log( "DirectoryName:" + files[i].DirectoryName );
            }



            Dictionary<string, GameObject> dic = new Dictionary<string, GameObject>();
            List<string> allPathes = new List<string>();
            var assetsGuid = AssetDatabase.FindAssets("t:prefab", new string[]{assetPath});
            for (var i = 0; i < assetsGuid.Length; i++)
            {
                var guid = assetsGuid[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                //var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                //dic[Path.GetFileName(path)] = obj;

                allPathes.Add(path);
                
                // var lastIndex = path.LastIndexOf('/');
                // if (lastIndex >= 0)
                // {
                //     var name = (path.Substring(lastIndex + 1)).Replace(".asset", "");
                //     names.Add(name, name);
                // }
            }

            var sortedPaths = allPathes.OrderBy(s => Path.GetFileName(s)).ToList();

            //var dicOrderby = string.Join("&", dic.OrderBy(p => p.Key[0]).Select(p => $"{p.Key}{p.Value}"));
            foreach(var item in sortedPaths)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(item);
                effects.Add(obj);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
#endif
}