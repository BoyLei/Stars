/*
 * @Description: 自定义后处理规则
 */
using System;
using System.Collections.Generic;
using GameTechTools.CommonLibs.CommonExtends;
using UnityEditor;
using UnityEngine;

namespace GameTechTools.UIHelpsTool
{
    [InitializeOnLoad]
    internal class CustomPostProccesser : AssetPostprocessor
    {
        static bool open = false; //是否开放
        static CustomPostProccesser()
        {
            //后台命令行启动的unity禁用后处理
            if (GTHelper.LoadWithBatchmode)
            {
                open = false;
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!open)
            {
                return;
            }
            OnDeleteAsset(deletedAssets);
        }

        //当删除资源时，将对应的预览图也删除
        static void OnDeleteAsset(string[] deletedAssets)
        {
            foreach (string path in deletedAssets)
            {
                if (!path.EndsWith(".prefab"))
                {
                    continue;
                }
                string guid = AssetDatabase.AssetPathToGUID(path);
                UIHelpsToolUtils.DeletePreviewDataByGoRemove(guid);
            }
        }

    }

}
