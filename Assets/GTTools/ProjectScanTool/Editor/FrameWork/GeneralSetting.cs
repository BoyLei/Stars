/*
 * @Description: 通用设置
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    /// <summary>
    /// 开启模块选择记录
    /// </summary>
    [Serializable]
    public class RunCustomMode
    {
        [HideInInspector]
        public EnumScanModes scanMode;

        public CustomSelectBox status = new CustomSelectBox();
    }

    [Serializable]
    [CustomScanModeAttribute(EnumScanModes.通用设置)]
    public class GeneralSetting : CustomScanMode
    {
        void OnEnable()
        {
            scanMode = EnumScanModes.通用设置;
            logSetting.Init();

            InitModeList();
        }

        /// <summary>
        /// 初始化模块选项
        /// </summary>
        private void InitModeList()
        {
            addRuleTips = File.ReadAllText(ProjectScanGlobalConfig.scan_tool_dir + "Example.txt"); ;
            foreach (var item in ProjectScanGlobalConfig.allScanModes)
            {
                bool find = false;
                foreach (var mode in modes)
                {
                    if (mode.scanMode == item.scanMode)
                    {
                        find = true;
                        //描述字段为了方便随时更改时没有序列化的，所以每次重新赋值
                        mode.status.description = item.scanMode.ToString();
                        break;
                    }
                }

                //把新规则添加进去
                if (!find && item.scanMode != EnumScanModes.通用设置)
                {
                    RunCustomMode mode = new RunCustomMode();
                    mode.scanMode = item.scanMode;
                    mode.status.enable = true;
                    mode.status.description = item.scanMode.ToString();
                    modes.Add(mode);
                }
            }

            for (var i = modes.Count - 1; i >= 0; i--)
            {
                bool find = false;
                foreach (var item in ProjectScanGlobalConfig.allScanModes)
                {
                    if (modes[i].scanMode == item.scanMode)
                    {
                        find = true;
                        break;
                    }
                }
                //某个规则可能已经删除了，这里的数据也需要删除
                if (!find)
                {
                    modes.Remove(modes[i]);
                }
            }
        }

        /// <summary>
        /// 检查某个模块是否开启
        /// </summary>
        public bool CheckCustomModeIsEnable(EnumScanModes type)
        {
            foreach (var item in modes)
            {
                if (item.scanMode == type)
                {
                    return item.status.enable;
                }
            }
            return false;
        }

        [Title("说明")]
        [DisableContextMenu(true, true), HideLabel, DisplayAsString(false), NonSerialized, ShowInInspector]
        //注意，说明类的不序列化存储，因为可能会经出变化，但是还是要显示出来的，根据自己代码中改动
        public string desc = "项目资源检测\n选择下方要扫描的模块，没有勾选的将直接跳过模块内的所有检测\n" +
        "当然，还可以在每一个检查模块里面单独开启和关闭某一个检查项\n注意，扫描规则的标题会作为日志输出的文件名称，所以尽量不要使用特殊字符";


        //单独的目录
        [TitleGroup("公共的目标文件夹配置")]
        [LabelText("目标文件夹     右侧+号添加     x号移除")]
        [DisableContextMenu(true, true), DisplayAsString]
        [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddTargetPath")]
        public List<string> targetDirs = new List<string>() { "Assets" };
        public void AddTargetPath()
        {
            string file = EditorUtility.OpenFolderPanel("添加目录", Application.dataPath, "");
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            targetDirs.Add(file.Substring(file.IndexOf("Assets")));
        }

        //单独的目录
        [TitleGroup("公共的目标文件夹配置")]
        [PropertySpace(SpaceBefore = 5)]
        [LabelText("忽略文件夹     右侧+号添加     x号移除")]
        [DisableContextMenu(true, true), DisplayAsString]
        [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddIgnorePath")]
        public List<string> ignoreDirs = new List<string>() { "Assets/GTTools" };
        public void AddIgnorePath()
        {
            string file = EditorUtility.OpenFolderPanel("添加目录", Application.dataPath, "");
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            ignoreDirs.Add(file.Substring(file.IndexOf("Assets")));
        }


        [TitleGroup("更多设置")]
        [TabGroup("更多设置/A", "模块选择"), PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
        [DisableContextMenu(true, true), LabelText("选择要扫描的模块")]
        [ListDrawerSettings(HideRemoveButton = true, HideAddButton = true, DraggableItems = false, ShowItemCount = false, Expanded = true)]
        public List<RunCustomMode> modes = new List<RunCustomMode>();

        [TabGroup("更多设置/A", "日志设置"), DisableContextMenu(true, true)]
        public LogSetting logSetting = new LogSetting();

        [TabGroup("更多设置/A", "规则样例"), DisableContextMenu(true, true)]
        [HideLabel, TextArea(10, 20), NonSerialized, ShowInInspector]
        public string addRuleTips;

    }

}
