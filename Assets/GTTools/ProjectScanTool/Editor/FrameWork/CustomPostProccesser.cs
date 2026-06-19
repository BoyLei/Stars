/*
 * @Description: 自定义后处理规则
 */
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [InitializeOnLoad]
    public class CustomPostProccesser : AssetPostprocessor
    {
        static bool open = false; //是否开放
        static bool canLog = false;
        static CustomPostProccesser()
        {
            //后台命令行启动的unity禁用后处理
            if (Environment.CommandLine.IndexOf("-batchmode") >= 0)
            {
                open = false;
            }
            Debug.LogFormat("CustomPostProccesser.InitializeOnLoad");
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!open || !ProjectScanGlobalConfig.openCustomPostProccesser)
            {
                return;
            }
            OnImportAsset(importedAssets);
            OnDeleteAsset(deletedAssets);
            OnMoveAsset(movedAssets, movedFromAssetPaths);
        }

        static void OnImportAsset(string[] importedAssets)
        {
            foreach (var str in importedAssets)
            {
                if (canLog) Debug.LogFormat("Imported Asset:{0}", str);
            }

            if (canLog) Debug.Log("Imported Asset Completed!");
            ProjectScanHelper.DoCustomRuleForPostProccesser(importedAssets);
        }

        static void OnDeleteAsset(string[] deletedAssets)
        {
            foreach (string str in deletedAssets)
            {
                if (canLog) Debug.LogFormat("Deleted Asset:{0}", str);
            }
            if (canLog) Debug.Log("Deleted Asset Completed!");
        }

        static void OnMoveAsset(string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < movedAssets.Length; i++)
            {
                var str = movedAssets[i];
                if (canLog) Debug.LogFormat("Moved Asset:{0}, from:{1}", str, movedFromAssetPaths[i]);
            }

            if (canLog) Debug.Log("Moved Asset Completed!");
            ProjectScanHelper.DoCustomRuleForPostProccesser(movedAssets);
        }


    }

}
