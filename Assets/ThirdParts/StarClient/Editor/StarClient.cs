///--------------------------------------------------------------------
/// 文件名   :   StarClient.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/12/09 14:42:41
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class StarClient
{
    public static string AssetConfigPath = "Assets/ThirdParts/StarClient/StarConfig.asset";
    public static void UpdateExcel()
    {
        var assestConfig = AssetDatabase.LoadAssetAtPath<StarConfigSetting>(AssetConfigPath);
        if (assestConfig != null)
        {
            assestConfig.UpdateExcel();
        }
    }

    public static void ExecuteExcel()
    {
        var assestConfig = AssetDatabase.LoadAssetAtPath<StarConfigSetting>(AssetConfigPath);
        if (assestConfig != null)
        {
            assestConfig.ExecuteExcel();
        }
    }

    public static void UpdateProtoBuff()
    {
        var assestConfig = AssetDatabase.LoadAssetAtPath<StarConfigSetting>(AssetConfigPath);
        if (assestConfig != null)
        {
            assestConfig.UpdateProtoBuff();
        }
    }

    public static void ExecuteProtoBuff()
    {
        var assestConfig = AssetDatabase.LoadAssetAtPath<StarConfigSetting>(AssetConfigPath);
        if (assestConfig != null)
        {
            assestConfig.ExecuteProtoBuff();
        }
    }
}
