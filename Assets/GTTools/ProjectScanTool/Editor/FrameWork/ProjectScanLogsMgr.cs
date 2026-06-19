/*
 * @Description: 项目扫描日志统计
 */
using System.Collections.Generic;
using System.Text;
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    public class ProjectScanLogsMgr
    {
        public const string ALL_TYPE = "------------------------------所有------------------------------";
        //执行列表
        public static ValueDropdownList<string> scanTypes = new ValueDropdownList<string>() { };
        //统计结果
        public static List<LogStatistics> logStatistics = new List<LogStatistics>();

        //清理日志
        private static void ClearLog()
        {
            logStatistics.Clear();
            scanTypes.Clear();
            scanTypes.Add(ALL_TYPE, ALL_TYPE);
        }

        //增加日志
        public static void AddLog(UnityEngine.Object context, CustomRule rule, string logDetail, string path)
        {
            LogStatistics logItem = null;
            foreach (var item in logStatistics)
            {
                if (item.customRule.bindMode == rule.bindMode && item.customRule.ruleTitle == rule.ruleTitle)
                {
                    logItem = item;
                    break;
                }
            }
            if (logItem == null)
            {
                logItem = new LogStatistics();
                logItem.customRule = rule;
                logItem.description = rule.ruleTitle;
                logStatistics.Add(logItem);
            }

            logItem.InsertLog(context, logDetail, path);

            // if (!logItem.abnormalObjs.Contains(context))
            // {
            //     logItem.abnormalObjs.Add(context);
            // }

        }

        //开始统计
        public static void BeginStatistics()
        {
            ClearLog();
            if (ProjectScanLogsWindow.IsOpen())
            {
                ProjectScanLogsWindow.Get().Clear();
            }
        }

        //统计
        public static void Statisticsing(CustomRule rule)
        {
            LogStatistics logItem = null;
            foreach (var item in logStatistics)
            {
                if (item.customRule.bindMode == rule.bindMode && item.customRule.ruleTitle == rule.ruleTitle)
                {
                    logItem = item;
                    break;
                }
            }
            if (logItem == null)
            {
                logItem = new LogStatistics();
                logItem.customRule = rule;
                logItem.description = rule.ruleTitle;
                logStatistics.Add(logItem);
            }
            logItem.Statistics();

            var scanType = GetSearchTitle(rule);
            scanTypes.Add(scanType, scanType);
        }

        //组装搜索信息
        public static string GetSearchTitle(CustomRule rule)
        {
            return rule.bindMode + "/" + rule.ruleTitle.Replace("/", "&");
        }

        //结束统计
        public static void EndStatistics()
        {
            if (ProjectScanLogsWindow.IsOpen())
            {
                ProjectScanLogsWindow.Get().UpdateStatistics();
            }
        }

        public static string FormatBuilderLog(string log)
        {
            return log.Replace("</color>", "").Replace("<color=#ff0000>", "").Replace("<color=#00ff00>", "");
        }
    }

    //日志统计显示界面
    public class ProjectScanLogsWindow : OdinEditorWindow
    {
        private static ProjectScanLogsWindow MainWindow = null;
        private bool isOpen = false;
        public static void OpenLogsWindow()
        {
            ProjectScanLogsWindow window = GetWindow<ProjectScanLogsWindow>("项目扫描日志统计");
            window.Show();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(800, 600f);
            window.minSize = new Vector2(800, 600f);
            window.maxSize = new Vector2(800, 600f);
            MainWindow.isOpen = true;
            window.UpdateStatistics();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            MainWindow = this;
            GTEditorWindowFocusMgr.Instance.PushEditorWindow(this);
        }

        protected override void OnDestroy()
        {
            MainWindow.isOpen = false;
            MainWindow = null;
            GTEditorWindowFocusMgr.Instance.PopEditorWindow(this);
            base.OnDestroy();
        }

        public static bool IsOpen()
        {
            return MainWindow != null && MainWindow.isOpen;
        }

        public static ProjectScanLogsWindow Get()
        {
            if (MainWindow == null)
            {
                MainWindow = GetWindow<ProjectScanLogsWindow>("项目扫描日志统计");
            }

            return MainWindow;
        }

        //显示部分
        [ValueDropdown("@ProjectScanLogsMgr.scanTypes")]
        [LabelText("检查类型:"), LabelWidth(60)]
        [HorizontalGroup("search", Width = 440)]
        [PropertySpace(SpaceBefore = 8, SpaceAfter = 8)]
        [OnValueChanged("OnSearchClick")]
        public string searchStr = ProjectScanLogsMgr.ALL_TYPE;

        private void OnSearchClick()
        {
            UpdateStatistics();
            GUIUtility.ExitGUI();
        }

        [HorizontalGroup("search", Width = 100, PaddingLeft = 5)]
        [PropertySpace(SpaceBefore = 8, SpaceAfter = 8)]
        [Button("汇总输出文件")]
        public void ExportData()
        {
            StringBuilder builder = new StringBuilder();
            var statisticsMgr = ProjectScanLogsMgr.logStatistics;
            foreach (var item in statisticsMgr)
            {
                builder.AppendLine("-------------------------------------------------------------------------------------------------");
                builder.AppendLine($"\t\t\t\t\t检查规则 [{ProjectScanLogsMgr.FormatBuilderLog(item.description)}]");
                builder.AppendLine("\t\t\t\t\t" + ProjectScanLogsMgr.FormatBuilderLog(item.scanTotalDsc) + "\t" +
                ProjectScanLogsMgr.FormatBuilderLog(item.abnormalDsc));
                builder.AppendLine("-------------------------------------------------------------------------------------------------");
                if (item.logDetails.Count > 0)
                {
                    foreach (var child in item.logDetails)
                    {
                        builder.AppendLine(child.warnPath);
                        builder.AppendLine(child.warnInfo);
                        builder.Append("\n");
                    }
                }
                builder.Append("\n\n\n");
            }
            ProjectScanHelper.WriteAndOpenFile(builder, "项目扫描结果汇总");
        }

        [LabelText("检查结果")]
        [TableList(IsReadOnly = true, DrawScrollView = true, MinScrollViewHeight = 530, MaxScrollViewHeight = 530, AlwaysExpanded = true, HideToolbar = true)]
        public List<LogStatistics> logStatistics = new List<LogStatistics>();

        public void Clear()
        {
            logStatistics.Clear();
            searchStr = ProjectScanLogsMgr.ALL_TYPE;
        }

        public void UpdateStatistics()
        {
            logStatistics.Clear();
            var statisticsMgr = ProjectScanLogsMgr.logStatistics;
            foreach (var item in statisticsMgr)
            {
                if (searchStr == ProjectScanLogsMgr.ALL_TYPE || searchStr == ProjectScanLogsMgr.GetSearchTitle(item.customRule))
                {
                    logStatistics.Add(item);
                }
            }
        }

    }
}