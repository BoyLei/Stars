///--------------------------------------------------------------------
/// 文件名   :   BuildRes.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/13 09:48:48
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets;

public class BuildRes
{
    public static string BuildScript = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
    public static string AssetDataBase = "Assets/AddressableAssetsData/DataBuilders/BuildScriptFastMode.asset";

    static void setProfile(string profile)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string profileId = settings.profileSettings.GetProfileId(profile);
        if (string.IsNullOrEmpty(profileId))
            Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
                             $"using current profile instead.");
        else
            settings.activeProfileId = profileId;
    }

    static void setBuilder(IDataBuilder builder)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;


        int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);
        if (index >= 0 && index < settings.DataBuilders.Count)
        {
            settings.ActivePlayerDataBuilderIndex = index;
        }
        else
        {
            Debug.LogWarning($"{builder} {index} must be added to the " +
                             $"DataBuilders list before it can be made " +
                             $"active. Using last run builder instead.");
        }
        UnityEditor.EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    static bool buildAddressableContent()
    {
        AddressableAssetSettings
            .BuildPlayerContent(out AddressablesPlayerBuildResult result);
        bool success = string.IsNullOrEmpty(result.Error);

        if (!success)
        {
            Debug.LogError("Addressables build error encountered: " + result.Error);
        }

        return success;
    }


    public static void SetUseAssetDataBase()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        IDataBuilder builderScript
            = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDataBase) as IDataBuilder;
        if (builderScript == null)
        {
            Debug.LogError(AssetDataBase + " couldn't be found or isn't a build AssetDataBase.");
            return;
        }

        int index = settings.DataBuilders.IndexOf((ScriptableObject)builderScript);
        if (index >= 0 && index < settings.DataBuilders.Count)
            settings.ActivePlayModeDataBuilderIndex = index;
        else
            Debug.LogWarning($"{builderScript} {index} must be added to the " +
                             $"DataBuilders list before it can be made " +
                             $"active. Using last run builder instead.");

        UnityEditor.EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    public static bool BuildAddressables()
    {
        setProfile(AssestConfig.Instance.CurrentProfileName);
        RunBeforBuildCommand();
        IDataBuilder builderScript
            = AssetDatabase.LoadAssetAtPath<ScriptableObject>(BuildScript) as IDataBuilder;

        if (builderScript == null)
        {
            Debug.LogError(BuildScript + " couldn't be found or isn't a build script.");
            return false;
        }

        setBuilder(builderScript);
        return buildAddressableContent();
    }

    public static void SetBuildScript()
    {
        setProfile(AssestConfig.Instance.CurrentProfileName);
        RunBeforBuildCommand();
        IDataBuilder builderScript
            = AssetDatabase.LoadAssetAtPath<ScriptableObject>(BuildScript) as IDataBuilder;

        if (builderScript == null)
        {
            Debug.LogError(BuildScript + " couldn't be found or isn't a build script.");
        }

        setBuilder(builderScript);
    }


    public static void FreshAssetGroup()
    {
        AssestConfig.Instance.FreshAssetGroup();
    }

    public static void FreshAssetGroupAync()
    {
        AssestConfig.Instance.FreshGroupAync();
    }

    public static void FreshAssetSetting()
    {
        AssestConfig.Instance.FreshAssetSetting();
    }


    public static void ClearAddressablesGroup()
    {
        AssestConfig.Instance.ClearAddressablesGroup();
    }


    /// <summary>
    /// 执行一些 build 之前 需要执行的指令
    /// </summary>
    public static void RunBeforBuildCommand()
    {
        Debug.Log("开始 生成 fxDetails.json : ");
        // 生成 一份 所有特效 参数 配置细节 的json
        SkillEditor.FxGenera.GeneraFxDetails();
        Debug.Log(" 生成 fxDetails.json 成功");
        Debug.Log(" ===================================== ");
        Debug.Log(" 开始 生成 AssetsToDefine : ");
        AssetsToDefine.GenerateAssetsToDefine();
        Debug.Log(" 生成 AssetsToDefine  成功 ");
        Debug.Log(" ===================================== ");
        Debug.Log(" 开始 生成 UIQueueToDefine : ");
        UIQueueToDefine.GenerateUIQueueToDefine();
        Debug.Log(" 生成 UIQueueToDefine  成功 ");
        Debug.Log(" ===================================== ");

    }
}