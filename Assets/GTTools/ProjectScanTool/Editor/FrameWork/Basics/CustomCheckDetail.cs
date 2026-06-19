/*
 * @Description: 自定义扫描细节，包括目标文件夹、忽略文件夹、白名单等，所有其他扫描细节都应当继承此基类
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    /// <summary>
    /// 自定义扫描细节基类，外部所有的自定义检查细节都需要继承此类
    /// </summary>
    [Serializable, HideLabel]
    [DisableContextMenu(true, true)]
    public class CustomCheckDetail
    {
        /// <summary>
        /// 额外说明，可以补充自己子配置项的说明
        /// </summary>
        [LabelText("额外说明"), LabelWidth(60)]
        public string descExt = "";

        /// <summary>
        /// 只有包含这个文件路径才去检测
        /// </summary>
        [LabelText("包含这个路径才会去检测 默认为空 都检测"), LabelWidth(300)]
        public List<string> checkPath = new List<string>();

        /// <summary>
        /// 是否启用单独的目录配置
        /// </summary>
        [DisableContextMenu(true, true)]
        public CustomSelectBox useSingleDir = new CustomSelectBox("是否启用单独的扫描目标文件夹配置");

        /// <summary>
        /// 单独的目标目录
        /// </summary>
        [PropertySpace(SpaceBefore = 5)]
        [DisableContextMenu(true, true), LabelText("目标文件夹"), DisplayAsString]
        [ShowIf("@useSingleDir.enable")]
        [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddTargetPath")]
        public List<string> targetDirs = new List<string>() { };
        public void AddTargetPath()
        {
            string file = EditorUtility.OpenFolderPanel("添加目录", Application.dataPath, "");
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            targetDirs.Add(file.Substring(file.IndexOf("Assets")));
        }

        /// <summary>
        /// 单独的忽略目录
        /// </summary>
        [LabelText("忽略文件夹"), DisplayAsString]
        [DisableContextMenu(true, true), ShowIf("@useSingleDir.enable")]
        [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddIgnorePath")]
        public List<string> ignoreDirs = new List<string>() { };
        public void AddIgnorePath()
        {
            string file = EditorUtility.OpenFolderPanel("添加目录", Application.dataPath, "");
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            ignoreDirs.Add(file.Substring(file.IndexOf("Assets")));
        }


        /// <summary>
        /// 白名单路径
        /// </summary>
        [PropertySpace(SpaceBefore = 5)]
        [DisableContextMenu(true, true), LabelText("选择资源白名单路径"), DisplayAsString]
        [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddWhiteListPath")]
        public List<string> whiteListPath = new List<string>() { };
        public void AddWhiteListPath()
        {
            string file = EditorUtility.OpenFilePanel("添加文件", Application.dataPath, "");
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            whiteListPath.Add(file.Substring(file.IndexOf("Assets")));
        }

    }
}
