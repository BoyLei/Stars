/*
 * @Description: 项目检测工具主界面
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{

    public class ProjectScanWindow : OdinMenuEditorWindow
    {
        private static ProjectScanWindow MainWindow = null;
        public OdinMenuTree mTree;
        public OdinMenuItem mCurSelectItem; //当前左侧选中的对象
        public static string checkLog = "";
        GUIStyle logStyle = new GUIStyle(); //工作台日志风格设置

        /// <summary>
        /// 主入口，各项目可以根据自己需要更改
        /// </summary>
        [MenuItem("自动化工具/ProjectScan (v2.0.6)", priority = 100)]
        public static void OpenCommonEditorWindow()
        {
            try
            {
                EditorUtility.DisplayProgressBar("提示", "加载数据中", 0);
                ProjectScanGlobalConfig.Init();
                foreach (var ruleData in ProjectScanGlobalConfig.allScanChildRules)
                {
                    AssetDatabase.LoadAssetAtPath(ProjectScanGlobalConfig.scanTypeConfig_dir + ruleData.assetType.Name + ".asset", ruleData.assetType);
                }
                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogErrorFormat("打开项目扫描工具失败, {0}", e.ToString());
                return;
            }

            ProjectScanWindow window = GetWindow<ProjectScanWindow>("ProjectScan (v2.0.6)");
            window.Show();
            window.MenuWidth = 200f;
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(1100f, 840f);
            window.minSize = new Vector2(1100f, 840f);
            window.maxSize = new Vector2(1100f, 840f);
            window.logStyle.richText = true;
        }


        public static ProjectScanWindow GetWindow()
        {
            if (MainWindow == null)
                MainWindow = GetWindow<ProjectScanWindow>();
            return MainWindow;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            MainWindow = this;
            GTEditorWindowFocusMgr.Instance.PushEditorWindow(this);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            mTree = new OdinMenuTree();
            mTree.Selection.SupportsMultiSelect = false;
            mTree.Selection.SelectionChanged -= this.SelectionChanged;
            mTree.Selection.SelectionChanged += this.SelectionChanged;
            mTree.Config.DrawSearchToolbar = true;
            mTree.Config.DefaultMenuStyle.Height = 30;
            mTree.Config.DefaultMenuStyle.DefaultLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            AddTreeItems(mTree);
            return mTree;
        }

        private void SelectionChanged(SelectionChangedType type = SelectionChangedType.SelectionCleared)
        {
            try
            {
                if (MainWindow == null || MainWindow.mTree == null)
                {
                    return;
                }
                IEnumerable<OdinMenuItem> IE = MainWindow.mTree.EnumerateTree().Where(x => x.IsSelected);
                if (IE == null)
                {
                    return;
                }
                var item = IE.FirstOrDefault();
                if (item == null)
                {
                    return;
                }
                if (mCurSelectItem != null && item == mCurSelectItem)
                {
                    return;
                }
                mCurSelectItem = item;
                // SelectionChanged(item);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

        }

        public void AddTreeItems(OdinMenuTree tree)
        {
            tree.Add("通用设置", AssetDatabase.LoadAssetAtPath(ProjectScanGlobalConfig.scanTypeConfig_dir + "GeneralSetting.asset", typeof(GeneralSetting)));
            foreach (var item in ProjectScanGlobalConfig.allScanModes)
            {
                tree.Add(item.description, new ScanModeDrawItem(item.scanMode));
            }

            tree.MenuItems.Sort((x, y) =>
            {
                if (x == null || y == null || x.Value == null || y.Value == null)
                    return 0;
                //模块id从小到大排序
                return (ProjectScanHelper.GetScanModeForMenuItem(x.Value) - ProjectScanHelper.GetScanModeForMenuItem(y.Value));
            });
            if (tree.MenuItems.Count > 0)
            {
                //首次打开默认打开通用设置
                if (mCurSelectItem == null)
                {
                    mCurSelectItem = tree.MenuItems[0];
                    tree.MenuItems[0].Select();
                }
            }

            InitTreeItemsInfo(tree);
        }

        /// <summary>
        /// 将模块、检测规则标题等封装成检索信息
        /// </summary>
        private void InitTreeItemsInfo(OdinMenuTree tree)
        {
            foreach (var menuItem in tree.MenuItems)
            {
                if (menuItem == null || menuItem.Value == null)
                {
                    continue;
                }
                //给栏目添加图标
                menuItem.IconGetter = () => ProjectScanDef.GetScanModeIconTex(ProjectScanHelper.GetScanModeForMenuItem(menuItem.Value));
                menuItem.SearchString = menuItem.Name;

                if (menuItem.Value.GetType() == typeof(ScanModeDrawItem))
                {
                    var rulesPreview = (menuItem.Value as ScanModeDrawItem).RulesPreview;
                    foreach (var preview in rulesPreview)
                    {
                        menuItem.SearchString += ((CustomRule)preview.customRule).ruleTitle;
                    }
                }

            }
        }

        /// <summary>
        /// 最近一次查看的规则
        /// </summary>
        // private UnityEngine.Object lastSelectCustomRule;
        // public void SetLastSelectCustomRule(UnityEngine.Object obj){
        //     lastSelectCustomRule = obj;
        // }

        /// <summary>
        /// 当编辑器中数据由代码逻辑进行改动时，设置当前选择的扫描模块为Dirty状态
        /// </summary>
        public void SetCurrentScanModeDirty()
        {
            if (mCurSelectItem == null || mCurSelectItem.Value == null || mCurSelectItem.Value.GetType() != typeof(ScanModeDrawItem))
            {
                return;
            }
            var rulesPreview = (mCurSelectItem.Value as ScanModeDrawItem).RulesPreview;
            foreach (var item in rulesPreview)
            {
                if ((item.customRule as CustomRule).expend)
                {
                    EditorUtility.SetDirty(item.customRule);
                }
            }
        }

        // private void SetupSeperator(int index = 1)
        // {
        //     seperator = new GUIStyle();
        //     float weight = options1.Length * 25;
        //     seperator.fixedWidth = weight;
        //     seperator.fontSize = 14;
        //     seperator.fontStyle = FontStyle.Normal;
        //     seperator.hover.background = (Texture2D)ProjectScanGlobalConfig.menuhover_tex;

        //     seperator.alignment = TextAnchor.MiddleCenter;
        //     seperator.focused.background = null;
        //     seperator.onNormal.background = null;
        //     seperator.normal.background = selectindex == index ? (Texture2D)ProjectScanGlobalConfig.menuhover_tex : null;
        //     seperator.normal.textColor = selectindex == index ? new Color(0, 0, 0) : new Color(0.7f, 0.7f, 0.7f);
        // }

        // GUIStyle style = new GUIStyle();
        // protected override void OnGUI()
        // {
        //     EditorStyles.popup.fontSize = 12;
        //     EditorStyles.popup.fixedHeight = 18;
        //     EditorStyles.popup.alignment = TextAnchor.MiddleCenter;
        //     GUILayout.BeginHorizontal();
        //     {
        //         GUILayout.BeginVertical("box");
        //         {
        //             GUILayout.Space(5);
        //             GUILayout.BeginHorizontal("box");
        //             {
        //                 SetupSeperator();
        //                 if (EditorGUILayout.DropdownButton(new GUIContent(options1), FocusType.Passive, seperator, GUILayout.Width(options1.Length * 25), GUILayout.Height(24)))
        //                 {
        //                     CreatMenuFile();
        //                 }

        //             }
        //             GUILayout.EndHorizontal();
        //         }
        //         GUILayout.EndVertical();
        //     }
        //     GUILayout.EndHorizontal();
        //     base.OnGUI();
        // }

        // public CommonMenuWindow menu = null;
        // //绘制操作选项
        // private GUIStyle seperator;
        // protected string options1 = "操作";
        // protected Rect buttonRect1 = new Rect(0, -5, "操作".Length * 25, 24);
        // protected int selectindex = 0;
        // //菜单栏
        // protected void CreatMenuFile()
        // {
        //     menu = new CommonMenuWindow();
        //     menu.AddItem("打开日志目录", 0, this.OpenLogDir);
        //     selectindex = 1;
        //     menu.DropDown(buttonRect1, position, delegate ()
        //     {
        //         selectindex = 0;
        //         menu = null;
        //     });
        // }

        // //打开日志目录
        // protected void OpenLogDir()
        // {
        //     System.Diagnostics.Process.Start(ProjectScanGlobalConfig.EditorLogDir);
        // }


        protected override void OnDestroy()
        {
            checkLog = "";
            mCurSelectItem = null;
            AssetDatabase.SaveAssets();

            MainWindow = null;
            GTEditorWindowFocusMgr.Instance.PopEditorWindow(this);

            base.OnDestroy();
        }

        /// <summary>
        /// 状态开启或关闭
        /// </summary>
        protected void NotifyOpenOrCloseAllScan(OdinMenuItem selected, EnumScanEnable status)
        {
            if (selected.Value == null || selected.Value.GetType() != typeof(ScanModeDrawItem))
            {
                return;
            }
            var rulesPreview = (selected.Value as ScanModeDrawItem).RulesPreview;
            foreach (var item in rulesPreview)
            {
                (item.customRule as CustomRule).enable = (status == EnumScanEnable.eOpen ? true : false);
            }
        }

        /// <summary>
        /// 执行扫描全部
        /// </summary>
        protected bool NotifyRunAllScanType(OdinMenuItem selected)
        {
            if (selected.Value == null || selected.Value.GetType() != typeof(ScanModeDrawItem))
            {
                return false;
            }
            ProjectScanGlobalConfig.openAutoCorrection = true;
            bool atLeastOneWasChecked = false;
            var rulesPreview = (selected.Value as ScanModeDrawItem).RulesPreview;
            foreach (var item in rulesPreview)
            {
                if ((item.customRule as CustomRule).enable)
                {
                    atLeastOneWasChecked = true;
                    ProjectScanHelper.DoScanCustomRuleByEntry(item.customRule);
                }
            }

            return atLeastOneWasChecked;
        }

        /// <summary>
        /// 绘制主界面toolbar
        /// </summary>
        protected override void OnBeginDrawEditors()
        {
            var selected = this.MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight + 10);
            {
                if (selected != null && selected.Value != null)
                {
                    var style = new GUIStyle();
                    style.fontSize = 18;
                    style.fixedHeight = 35;
                    style.normal.textColor = Color.white;
                    style.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Space(5);
                    GUILayout.Label(selected.Name, style);

                    if (selected.Value.GetType() == typeof(ScanModeDrawItem))
                    {
                        if (SirenixEditorGUI.ToolbarButton(new GUIContent(" 开启全部 ", "点击开启所有检查规则")))
                        {
                            NotifyOpenOrCloseAllScan(selected, EnumScanEnable.eOpen);
                        }

                        if (SirenixEditorGUI.ToolbarButton(new GUIContent(" 关闭全部 ", "点击关闭所有检查规则")))
                        {
                            NotifyOpenOrCloseAllScan(selected, EnumScanEnable.eClose);
                        }
                    }

                    if (selected.Value.GetType().BaseType == typeof(CustomScanMode))
                    {
                        //打开confluence文档
                        if (SirenixEditorGUI.ToolbarButton(new GUIContent(" 说明文档 ", "打开说明文档")))
                        {
                            Application.OpenURL("https://www.baidu.com");
                        }

                        if (SirenixEditorGUI.ToolbarButton(new GUIContent(" 日志目录 ", "打开检查后生成的日志目录\n需要勾选日志设置中的[将检查结果输出到文本中]")))
                        {
                            System.Diagnostics.Process.Start(ProjectScanGlobalConfig.EditorLogDir);
                        }

                        if (SirenixEditorGUI.ToolbarButton(new GUIContent(" 日志统计 ", "统计当前执行模块日志")))
                        {
                            ProjectScanLogsWindow.OpenLogsWindow();
                        }
                    }

                    if (SirenixEditorGUI.ToolbarButton(new GUIContent(" 执行全部 ", "点击执行所有检查规则")))
                    {
                        ProjectScanLogsMgr.BeginStatistics();
                        var customRuleClass = selected.Value;
                        //如果是通用设置里面的检查全部
                        if (customRuleClass.GetType().BaseType == typeof(CustomScanMode))
                        {
                            //先删除所有日志文件
                            ProjectScanHelper.ClearScanLogFils();
                            //
                            GeneralSetting generalSetting = (GeneralSetting)customRuleClass;
                            var menuItems = this.MenuTree.MenuItems;
                            bool flag = false;
                            foreach (var item in menuItems)
                            {
                                if (item != null && item.Value != null
                                && item.Value.GetType() == typeof(ScanModeDrawItem)
                                && generalSetting.CheckCustomModeIsEnable(((ScanModeDrawItem)item.Value).scanMode))
                                {
                                    if (NotifyRunAllScanType(item))
                                    {
                                        flag = true;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                EditorUtility.DisplayDialog("提示", "没有开启任何要扫描的模块", "确定");
                            }
                        }
                        else
                        {
                            //如果是单个检查全部
                            if (!NotifyRunAllScanType(selected))
                            {
                                EditorUtility.DisplayDialog("提示", "没有开启任何要扫描的模块", "确定");
                            }
                        }
                        ProjectScanLogsMgr.EndStatistics();
                        GUIUtility.ExitGUI();
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            GUILayout.Space(8);
            GUILayout.Label(checkLog, logStyle);
            GUILayout.Space(8);
        }

    }
}