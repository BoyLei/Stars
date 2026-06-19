/*
 * @Description: 工具的主管理还是在ProjectScanWindow相关里面。这里主要提供暴露给Jenkins后台调度各模块检查接口
 */
using UnityEditor;
using System;
using Sirenix.Utilities;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace CasualEngine.ProjectScanTool
{
    public class ProjectScanner
    {
        // [MenuItem("Tools/ProjectScan_DebugRunAll")]
        /// <summary>
        /// jenkins等后台调度检查全部规则
        /// </summary>
        public static void RunAll()
        {
            //先删除所有日志文件
            ProjectScanHelper.ClearScanLogFils();
            //

            try
            {
                string EditorLogDir = ProjectScanGlobalConfig.EditorLogDir;
                if (!Directory.Exists(EditorLogDir))
                {
                    Directory.CreateDirectory(EditorLogDir);
                }
                //后台扫描时关闭自动修正功能
                ProjectScanGlobalConfig.openAutoCorrection = false;
                ProjectScanGlobalConfig.InitCustomData();

                GeneralSetting generalSetting = AssetDatabase.LoadAssetAtPath<GeneralSetting>(ProjectScanGlobalConfig.scanTypeConfig_dir + typeof(GeneralSetting).Name + ".asset");
                if (generalSetting == null)
                {
                    ProjectScanHelper.PrintProjectScanLog("ProjectScanner.RunAll", "找不到" + typeof(GeneralSetting).Name + ".asset", ProjectScanGlobalConfig.scanTypeConfig_dir);
                    return;
                }
                foreach (var item in ProjectScanGlobalConfig.allScanChildRules)
                {
                    if (item != null && generalSetting.CheckCustomModeIsEnable(item.bindScanMode))
                    {
                        var customRule = AssetDatabase.LoadAssetAtPath(ProjectScanGlobalConfig.scanTypeConfig_dir + item.assetType.Name + ".asset", item.assetType);
                        if (customRule == null)
                        {
                            ProjectScanHelper.PrintProjectScanLog("ProjectScanner.RunAll", $"找不到{item.assetType.Name}.asset", ProjectScanGlobalConfig.scanTypeConfig_dir);
                            continue;
                        }
                        Scan(customRule);
                    }
                }
            }
            finally
            {
                ProjectScanGlobalConfig.openAutoCorrection = true;
            }
        }

        /// <summary>
        /// 检查某一模块
        /// </summary>
        private static void Scan(System.Object customRule)
        {
            if (customRule.GetType().BaseType != typeof(CustomRule))
            {
                return;
            }
            if (customRule.GetType().IsDefined<CustomScanTypeAttribute>())
            {
                CustomRule rule = (CustomRule)customRule;
                if (rule.enable)
                {
                    var methods = customRule.GetType().GetMethods();
                    foreach (var method in methods)
                    {
                        if (method.IsDefined<CustomScanActionAttribute>())
                        {
                            int parameCnt = 0;
                            if (method.GetParameters() != null)
                                parameCnt = method.GetParameters().Length;
                            List<object> args = new List<object>();
                            //点击执行的操作都是默认的检查规则，不传入任何参数
                            while (parameCnt-- > 0)
                            {
                                args.Add(null);
                            }
                            method.Invoke(customRule, args.ToArray());
                        }
                    }
                }
            }
        }

    }
}
