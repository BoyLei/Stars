#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/Create AssestConfig")]
public class AssestConfig : ScriptableObject
{
    public const string RemoteLoadPath = "Remote.LoadPath";                    //远程加载目录
    public const string RemoteBuildPath = "Remote.BuildPath";                  //远程构建目录


    public const string LocalLoadPath = "Local.LoadPath";                    //远程资源加载目录
    public const string LocalBuildPath = "Local.BuildPath";                  //远程资源构建目录

    public static string AssetConfigPath = "Assets/DevTools/AssestEditor/AssetsConfig.asset";


    private static AssestConfig assestConfig;
    public static AssestConfig Instance
    {
        get
        {
            if (assestConfig == null)
            {
                assestConfig = AssetDatabase.LoadAssetAtPath<AssestConfig>(AssetConfigPath);
            }
            return assestConfig;
        }
    }


    [FoldoutGroup("设置")]
    [LabelText("是否热更")]
    public bool IsUseUpdate;

    [FoldoutGroup("设置")]
    [LabelText("隐藏unity版本信息")]
    public bool EnableUnityVersion;

    [FoldoutGroup("设置")]
    [LabelText("是否编译到包体")]
    public bool IncludeInBuild;

    [FoldoutGroup("设置")]
    [LabelText("是否首包")]
    public bool IsFirstPack;

    public string CurrentProfileName
    {
        get
        {
            string profileInUse = "Default";
            //  if (IsUseUpdate && !IsFirstPack)
            // {
            //     // profileInUse = "Hotfix";
            //  }
            return profileInUse;
        }
    }

    [FoldoutGroup("配置")]
    public string[] m_GroupsSepartately = new string[] { "texture-item", "texture-bg", /*"scene",*/  "texture-poster", "scene-terrain", "Map" };

    public Dictionary<string, string> m_AddressableGroupLabelLookUpDic = new Dictionary<string, string>()
    {
        { "LuaScripts",   "lua"},
        { "ConfigJson",   "config"},
        { "SkillJson",   "skillconfig"},
        { "MapData",   "config"},
        { "shader",   "preload"},
        {"preload-prefab","preload"},
        {"preload-texture","preload"},
        {"preload-renderTexture","preload"},
        {"Animation","animation"},
        {"Effects","effect"},
        //{"Roles","roles"},
        {"RolesDisplay","roles"},
        {"RolesTemplate","roles"},
        {"RolesWorld","roles"},
        {"InteractObject","roles"},

    };

    /// <summary>
    /// Shader 黑名单
    /// </summary>
    [LabelText("Shader黑名单")]
    public List<string> BlockShader = new List<string>()
    {

    };
    /// <summary>
    /// 设置成本地模式
    /// </summary>
    public void SetAddressableLocal()
    {
        IsUseUpdate = false;
        IncludeInBuild = true;

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.RemoteCatalogLoadPath.SetVariableByName(settings, LocalLoadPath);
        settings.RemoteCatalogBuildPath.SetVariableByName(settings, LocalBuildPath);
        //  UnityEditor.EditorUtility.SetDirty(settings);
        // UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
        // UnityEditor.AssetDatabase.SaveAssets();
        FreshAssetSetting(false);
    }



    [FoldoutGroup("配置")]
    [LabelText("资源分组")]
    [TableList]
    public AssetGroup[] Asstes;


    [FoldoutGroup("Addressable")]
    [Button("刷新Addressable分组")]
    [PropertySpace(SpaceBefore = 5, SpaceAfter = 10)]
    public void FreshAssetGroup()
    {
        //设置分组
        foreach (var config in Asstes)
        {
            if (config == null)
            { continue; }
            if (string.IsNullOrEmpty(config.GroupName))
            {
                continue;
            }
            var label = GetAddressableLabelFromGroup(config.GroupName);

            //仅出补丁包时才设置cs-patch分组，平时工程内没有该组对应的文件夹
            if (config.GroupName == "cs-patch")
            {
                if (IsUseUpdate)
                {
                    SetAddressable(config.GroupName, config.Path, GetFilter(config.Suffix, config.Suffix_Extens), label);
                }
            }
            else
            {
                SetAddressable(config.GroupName, config.Path, GetFilter(config.Suffix, config.Suffix_Extens), label);
            }
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.SaveAssets();

        Debug.LogError("刷新分组结束");
    }


    [FoldoutGroup("Addressable")]
    [Button("异步刷新Addressable分组")]
    public void FreshGroupAync()
    {
        EditorCoroutineUtility.StartCoroutine(FreshAssetGroupAsync(),this);
    }
    
    IEnumerator SetAddressableAync(string groupName, string dir, string fliter, bool isCheckDependences, string label = "")
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup((a) => { return a.name == groupName; });

        //更新时不重新创建Group
        if (!IsUseUpdate)
        {
            if (group != null)
            {
                settings.RemoveGroup(group);
            }
        }
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);
        }

        List<string> allPathList = new List<string>();

        if (Directory.Exists(dir))
        {
            string[] fliters = fliter.Split('|');
            foreach (var item in fliters)
            {
                string[] files = System.IO.Directory.GetFiles(dir, item, System.IO.SearchOption.AllDirectories);
                if (isCheckDependences)//打包勾选 并且是打包调用才检测依赖
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        var dependencies = AssetDatabase.GetDependencies(files[i], true); // 获取资源的所有依赖项，包括间接依赖项
                        for (int j = 0; j < dependencies.Length; j++)
                        {
                            allPathList.Add(dependencies[j]);
                        }
                    }
                }
                else
                {
                    allPathList.AddRange(files);
                }
            }
        }

        var allPath = allPathList.ToArray();

        Debug.LogError($"设置Group {groupName}    总数量:{allPath.Length}");
        EditorUtility.ClearProgressBar();
        var entriesAdded = new List<AddressableAssetEntry>();
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = allPath[i];
            path = path.Replace("\\", "/");
            EditorUtility.DisplayProgressBar($"设置Group {groupName}", path, (float)i / allPath.Length);


            var guid = AssetDatabase.AssetPathToGUID(path);
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string suffix = System.IO.Path.GetExtension(assetPath);
            assetPath = assetPath.Replace(suffix, "");

            try
            {
                var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
                if (groupName.Equals("art-scene") || groupName.Equals("scene"))
                {
                    entry.SetAddress(System.IO.Path.GetFileName(assetPath).ToLower().Replace(" ", ""));       //资源索引地址统一小写，删除空格
                }
                else
                {
                    entry.SetAddress(assetPath.ToLower().Replace("assets/res/", "").Replace(" ", ""));       //资源索引地址统一小写，删除空格
                }

                entry.labels.Clear();

                if (!string.IsNullOrEmpty(label))
                {
                    entry.labels.Add(label);
                }


                if (!group.entries.Contains(entry))
                {
                    entriesAdded.Add(entry);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        //删除空分组
        if (group.entries.Count <= 0)
        {
            settings.RemoveGroup(group);
        }
        EditorUtility.ClearProgressBar();
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true);
        yield return null;
    }

 

    private IEnumerator FreshAssetGroupAsync()
    {
        foreach (var config in Asstes)
        {
            if (config == null)
            { continue; }
            if (string.IsNullOrEmpty(config.GroupName))
            {
                continue;
            }
            var label = GetAddressableLabelFromGroup(config.GroupName);

            //仅出补丁包时才设置cs-patch分组，平时工程内没有该组对应的文件夹
            if (config.GroupName == "cs-patch")
            {
                if (IsUseUpdate)
                {
                    
                  EditorCoroutineUtility.StartCoroutine( SetAddressableAync(config.GroupName, config.Path, GetFilter(config.Suffix, config.Suffix_Extens), false, label),this);
                }
            }
            else
            {
                EditorCoroutineUtility.StartCoroutine(SetAddressableAync(config.GroupName, config.Path, GetFilter(config.Suffix, config.Suffix_Extens), false, label),this);
            }
        }
        yield return null;
    }
    
    public void SetAddressableRemote()
    {
        string ContentStatePath = GameApp.Instance.BuildDir;
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.RemoteCatalogLoadPath.SetVariableByName(settings, RemoteLoadPath);
        settings.RemoteCatalogBuildPath.SetVariableByName(settings, RemoteBuildPath);
        settings.ContentStateBuildPath = ContentStatePath;
        settings.BuildRemoteCatalog = true;
        settings.DisableCatalogUpdateOnStartup = true;
        settings.BundleLocalCatalog = false;
        settings.OptimizeCatalogSize = true;
        //ab包中是否隐藏unity版本信息
        //settings.OverridePlayerVersion = GameApp.Instance.Version;

        UnityEditor.EditorUtility.SetDirty(settings);
        UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
        UnityEditor.AssetDatabase.SaveAssets();
        FreshAssetSetting(true);

    }

    [FoldoutGroup("Addressable")]
    [Button("刷新Addressable设置")]
    [PropertySpace(SpaceBefore = 5, SpaceAfter = 10)]
    public void FreshAssetSetting(bool remote = false)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string id = settings.profileSettings.GetProfileId(CurrentProfileName);
        settings.activeProfileId = id;

        Debug.Log($"==========@@ Profile in Use : {settings.profileSettings.GetProfileName(settings.activeProfileId)}");

        //清理有可能为空或miss的组
        int idx = 0;
        while (idx < settings.groups.Count)
        {
            var group = settings.groups[idx];
            if (group == null)
            {
                settings.groups.RemoveAt(idx);
            }
            else
            {
                ++idx;
            }
        }

        foreach (var group in settings.groups)
        {
            var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
            if (updateSchema == null)
            {
                updateSchema = group.AddSchema<ContentUpdateGroupSchema>();
            }
            if (updateSchema != null)
            {
                updateSchema.StaticContent = true;
            }

            var packLoadSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (packLoadSchema == null)
            {
                packLoadSchema = group.AddSchema<BundledAssetGroupSchema>();
            }
            if (packLoadSchema != null)
            {
                //热更新超时时间
                packLoadSchema.Timeout = 0;
                //重试次数
                packLoadSchema.RetryCount = 0;
                //独立Provider
                packLoadSchema.ForceUniqueProvider = false;
                //是否跟包
                packLoadSchema.IncludeInBuild = BuildInPack(group.name);// !IsUseUpdate || (IncludeInBuild && BuildInPack(group.name));// !IsGroupDontIncludeInBuild(group.name);
                //该group资源是否分开打bundle
                packLoadSchema.BundleMode = IsGroupPackSepartately(group.name) ? BundledAssetGroupSchema.BundlePackingMode.PackSeparately : BundledAssetGroupSchema.BundlePackingMode.PackTogether;
                packLoadSchema.UseAssetBundleCache = true;
                //是否静态资源
                if (!remote || IsFirstPack)
                {

                    packLoadSchema.UseAssetBundleCrc = remote;
                    packLoadSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
                    //静态资源默认跟包
                    packLoadSchema.BuildPath.SetVariableByName(settings, LocalBuildPath);
                    packLoadSchema.LoadPath.SetVariableByName(settings, LocalLoadPath);
                }
                else
                {
                    packLoadSchema.UseAssetBundleCrc = true;
                    packLoadSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;

                    //动态资源出包时不跟包，若存在直接放到资源服务器
                    packLoadSchema.BuildPath.SetVariableByName(settings, RemoteBuildPath);
                    packLoadSchema.LoadPath.SetVariableByName(settings, RemoteLoadPath);
                }
            }
            EditorUtility.SetDirty(group);
        }
        //EditorUtility.SetDirty(settings);
        // AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        //AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    }

    [FoldoutGroup("Addressable")]
    [PropertySpace(SpaceBefore = 5, SpaceAfter = 10)]
    [Button("清理Addressable分组")]
    public void ClearAddressablesGroup()
    {
        //清理Addressable分组
        Debug.LogError("清理Addressable分组");

        //清理Addressable
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings.groups != null)
        {
            for (int i = settings.groups.Count - 1; i >= 0; i--)
            {
                var group = settings.groups[i];
                if (group != null)
                {
                    settings.RemoveGroup(group);
                }
            }
        }
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, null, true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.SaveAssets();
    }

    bool IsDynamicGroup(string group)
    {
        //动态资源打包时不进包，静态资源跟随出包
        var Groups = new string[] { "cs-patch", "update_patch" };

        foreach (var str in Groups)
        {
            if (group.ToLower().Contains(str.ToLower()))
            {
                return true;
            }
        }

        return false;
    }

    bool IsGroupDontIncludeInBuild(string group)
    {
        // 此处配置的资源不包含在包内，使用时直接资源服务器下载
        var Groups = new string[] { "" };
        return Groups.KContains(group);
    }
    //是否跟包
    public bool BuildInPack(string group)
    {
        if (Asstes != null && Asstes.Length > 0)
        {
            foreach (var item in Asstes)
            {
                if (item.GroupName == group)
                {
                    return item.IncludeInBuild;
                }
            }
        }
        return false;
    }

    bool IsGroupPackSepartately(string group)
    {
        return m_GroupsSepartately.KContains(group);
    }

    /// <summary>
    /// 获取指定组的label标签(如果有)
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    private string GetAddressableLabelFromGroup(string groupName)
    {
        if (m_AddressableGroupLabelLookUpDic.TryGetValue(groupName, out string ans))
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                bool isHasLabel = false;
                foreach (var label in settings.GetLabels())
                {
                    if (label == ans)
                    {
                        isHasLabel = true;
                        break;
                    }
                }
                if (!isHasLabel)
                {
                    settings.AddLabel(ans);
                }
            }
            return ans;
        }
        return string.Empty;
    }
    void SetAddressable(string groupName, string dir, string fliter, string label = "")
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup((a) => { return a.name == groupName; });

        //更新时不重新创建Group
        if (!IsUseUpdate)
        {
            if (group != null)
            {
                settings.RemoveGroup(group);
            }
        }
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);
        }

    
        
        List<string> allPathList = new List<string>();

        if (Directory.Exists(dir))
        {
            string[] fliters = fliter.Split('|');
            foreach (var item in fliters)
            {
                string[] files = System.IO.Directory.GetFiles(dir, item, System.IO.SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var path = Path.GetExtension(file);
                    if (path == ".unity")
                    {
                        var fileinfo = Path.GetFileName(file);

                        if(CheckIsUnuseScene(fileinfo))
                        {
                            continue;
                        }
                    }
                    if (groupName == "Shader")
                    {
                        //黑名单检查
                        if (InBlockShader(file))
                        {
                            continue;   
                        }
                        
                        allPathList.Add(file);
                    }
                    else
                    {
                        allPathList.Add(file);
                    }
                }
               // allPathList.AddRange(files);
            }
        }

        var allPath = allPathList.ToArray();

        Debug.LogError($"设置Group {groupName}    总数量:{allPath.Length}");
        EditorUtility.ClearProgressBar();
        var entriesAdded = new List<AddressableAssetEntry>();
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = allPath[i];
            path = path.Replace("\\", "/");
            EditorUtility.DisplayProgressBar($"设置Group {groupName}", path, (float)i / allPath.Length);


            var guid = AssetDatabase.AssetPathToGUID(path);
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string suffix = System.IO.Path.GetExtension(assetPath);
            assetPath = assetPath.Replace(suffix, "");

            try
            {
                var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
                if (groupName.Equals("art-scene") || groupName.Equals("scene"))
                {
                    entry.SetAddress(System.IO.Path.GetFileName(assetPath).ToLower().Replace(" ", ""));       //资源索引地址统一小写，删除空格
                }
                else
                {
                    entry.SetAddress(assetPath.ToLower().Replace("assets/res/", "").Replace(" ", ""));       //资源索引地址统一小写，删除空格
                }

                entry.labels.Clear();

                if (!string.IsNullOrEmpty(label))
                {
                    entry.labels.Add(label);
                }


                if (!group.entries.Contains(entry))
                {
                    entriesAdded.Add(entry);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        //删除空分组
        if (group.entries.Count <= 0)
        {
            settings.RemoveGroup(group);
        }
        EditorUtility.ClearProgressBar();
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true);
    }

    private static bool CheckIsUnuseScene(string scenename)
    {
        UnuseMapsThisVersion unuseMapsThisVersion;
        unuseMapsThisVersion = Resources.Load<UnuseMapsThisVersion>("UnuseMapsThisVersion");
        for (int i = 0; i < unuseMapsThisVersion.removeName.Length; i++)
        {
            if (scenename.Equals(unuseMapsThisVersion.removeName[i]+".unity"))
            {
                return true;
            }
        }
        return false;
    }

    private bool InBlockShader(string shaderPath)
    {
        if (shaderPath.StartsWith("Assets/Res/Shader"))
        {
            foreach (var item in BlockShader)
            {
                string path = item.Replace("\\", "/");
                shaderPath = shaderPath.Replace("\\", "/");
                if (path == shaderPath)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private string GetFilter(SuffixEnum suffix, SuffixEnum_Extennds enumExtennds)
    {
        string filter = string.Empty;
        string _suffix = suffix.ToString();
        string[] array = _suffix.Split(',');

        for (int i = 0; i < array.Length; i++)
        {
            /*if (i == array.Length - 1)
            {
                filter += "*." + array[i].Trim();
            }
            else
            {
                filter += "*." + array[i].Trim() + "|";
            }*/
            filter += "*." + array[i].Trim() + "|";
        }

        string _suffixex = enumExtennds.ToString();
        string[] arrayex = _suffixex.Split(',');

        for (int i = 0; i < arrayex.Length; i++)
        {
            if (arrayex[i] == "None")
            {
                continue;
            }
            filter += "*." + arrayex[i].Trim() + "|";
            /*if (i == arrayex.Length - 1)
            {
                filter += "*." + arrayex[i].Trim();
            }
            else
            {
                filter += "*." + arrayex[i].Trim() + "|";
            }*/
        }

        filter = filter.Remove(filter.Length - 1, 1);
        return filter;
    }
}

[System.Serializable]
public class AssetGroup
{
    [LabelText("组名")]
    public string GroupName;

    [LabelText("是否跟首包")]
    public bool IncludeInBuild = true;

    [LabelText("路径")]
    [FolderPath]
    [OnValueChanged("OnsetPath")]
    public string Path;

    [LabelText("后缀名")]
    [EnumPaging]
    public SuffixEnum Suffix = SuffixEnum.prefab;

    [LabelText("后缀名")]
    [EnumPaging]
    public SuffixEnum_Extennds Suffix_Extens;

    public void OnsetPath()
    {
        if (string.IsNullOrEmpty(GroupName))
        {
            if (!string.IsNullOrEmpty(Path))
            {
                int index = Path.LastIndexOf("/") + 1;
                GroupName = Path.Substring(index);
            }
        }
    }
}


[System.Flags]
public enum SuffixEnum
{
    prefab = 1 << 1,
    asset = 1 << 2,
    txt = 1 << 3,
    json = 1 << 4,
    png = 1 << 5,
    jpg = 1 << 6,
    renderTexture = 1 << 7,
    spriteatlasv2 = 1 << 8,
    bytes = 1 << 9,
    otf = 1 << 10,
    ttf = 1 << 11,
    anim = 1 << 12,
    controller = 1 << 13,
    mask = 1 << 14,
    mat = 1 << 15,
    shader = 1 << 16,
    compute = 1 << 17,
    shadervariants = 1 << 18,
    shadergraph = 1 << 19,
    mp3 = 1 << 20,
    wav = 1 << 21,
    ogg = 1 << 22,
    mp4 = 1 << 23,
    unity = 1 << 24,
    hlsl = 1 << 25,
    exr = 1 << 26,
    mtl = 1 << 27,
    obj = 1 << 28,
    rar = 1 << 29,
    fbx = 1 << 30,
    tga = 1 << 31,
};

[System.Flags]
public enum SuffixEnum_Extennds
{
    tif = 1 << 1,
    cginc = 1 << 2,
    unitypackage = 1 << 3,
    playable = 1 << 4,
    lighting = 1 << 5,
    spriteatlas = 1 << 6
}
#endif