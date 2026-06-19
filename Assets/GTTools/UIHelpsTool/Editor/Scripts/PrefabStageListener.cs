/*
 * @Description: 监听prefab状态改变，当修改了prefab时，如果时预览的通用件，则视图重新生成
 */
using UnityEngine;
using UnityEditor;

using System.IO;
using System;

namespace GameTechTools.UIHelpsTool
{
    [InitializeOnLoad]
    internal static class PrefabStageListener
    {
        static PrefabStageListener()
        {
            //Prefab auto save之后回调
            UnityEditor.SceneManagement.PrefabStage.prefabSaved -= OnPrefabSaved;
            UnityEditor.SceneManagement.PrefabStage.prefabSaved += OnPrefabSaved;
        }

        static void OnPrefabSaved(GameObject prefab)
        {
            //后台命令行启动的unity禁用此操作
            if (Environment.CommandLine.IndexOf("-batchmode") >= 0)
            {
                return;
            }

            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null || prefabStage.prefabContentsRoot == null)
                return;

            //auto save拿到的prefab竟然无法获取path，需要通过当前stage拿到实际编辑的prefab
            string path = prefabStage.prefabAssetPath;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }
            string preview_path = UIHelpsToolConfigure.PreviewPath + "/" + guid + ".png";
            if (File.Exists(preview_path))
            {
                //重新生成
                Texture Tex = UIHelpsToolUtils.ExportBasicCompImg(prefabStage.prefabContentsRoot, guid);
                if (Tex != null)
                {
                    AssetDatabase.ImportAsset(preview_path, ImportAssetOptions.ForceUpdate);
                    if (GeneralCompWindow.mainWindow)
                    {
                        GeneralCompWindow.mainWindow.Repaint();
                    }
                }
            }
        }

    }
}