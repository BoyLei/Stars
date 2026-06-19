/*
 * @Description: 自定义扫描规则基类，包括是否开放扫描，标题描述，基础说明，目标文件夹、忽略文件夹、白名单等，所有其他扫描规则都应当继承此基类
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    /// <summary>
    /// 自定义规则基类，外部所有的自定义检查类型都需要继承此类
    /// </summary>
    [Serializable]
    // [DisableContextMenu(true, true)]
    public class CustomRule : ScriptableObject
    {
        /// <summary>
        /// 是否展开
        /// </summary>
        [NonSerialized]
        public bool _expend = false;

        public bool expend
        {
            get { return _expend; }
            set
            {
                this._expend = value;
                // ProjectScanWindow.GetWindow().SetLastSelectCustomRule(this);
            }
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        [HideInInspector]
        public bool _enable = false;

        public bool enable
        {
            get { return _enable; }
            set
            {
                this._enable = value;
                if (menuItem != null)
                {
                    EditorUtility.SetDirty(menuItem);
                }
            }
        }

        [DisableContextMenu(true, true), HideInEditorMode, DisplayAsString, NonSerialized, ShowInInspector, HideLabel]
        public string ruleTitle = "标题";

        [DisplayAsString(false), HideLabel, NonSerialized, ShowInInspector]
        [HideIf("@string.IsNullOrEmpty(ruleDescription)")]
        [CustomContextSimpleMenu("1.打开 [调试结果日志]", "OpenLog")]
        [CustomContextSimpleMenu("2.显示或隐藏 [自动修正]功 能", "ModifyShowAutoCorrection")]
        [CustomContextSimpleMenu("3.显示或隐藏 [应用后处理] 功能", "ModifyShowAutoPostProcessor")]
        public string ruleDescription = "";
        /// <summary>
        /// 打开执行日志，前提是在通用设置里面打开了记录日志功能
        /// </summary>
        private void OpenLog()
        {
            string logPath = ProjectScanGlobalConfig.EditorLogDir + "/" + this.ruleTitle.Trim().Replace("/", "&") + ".log";
            if (!File.Exists(logPath))
            {
                EditorUtility.DisplayDialog("提示", "检查日志不存在，请先执行检查或调试操作", "确定");
            }
            else
            {
                System.Diagnostics.Process.Start(logPath);
            }
        }

        /// <summary>
        /// 显示或隐藏自动修正功能
        /// </summary>
        private void ModifyShowAutoCorrection()
        {
            showAutoCorrection = !showAutoCorrection;
            if (!showAutoCorrection)
            {
                _autoCorrection.enable = false;
            }
            if (menuItem)
            {
                EditorUtility.SetDirty(menuItem);
            }
        }

        /// <summary>
        /// 显示或隐藏应用后处理功能
        /// </summary>
        private void ModifyShowAutoPostProcessor()
        {
            showAutoPostProcessor = !showAutoPostProcessor;
            if (!showAutoPostProcessor)
            {
                autoPostProcessor.enable = false;
            }
            if (menuItem)
            {
                EditorUtility.SetDirty(menuItem);
            }
        }


        /// <summary>
        /// 是否显示自动修正功能，默认false，要开启的在外部规则设置
        /// </summary>
        [DisableContextMenu(true, true), HideInInspector, HideInPlayMode]
        public bool showAutoCorrection = false;

        /// <summary>
        /// 是否开启自动修正功能，前提是先开启显示自动修正功能，因为有些检查逻辑不需要自动修正，显示出来容易误导
        /// </summary>
        [PropertyTooltip("注意，开启自动修正别忘了在扫描逻辑里面加上修正的逻辑")]
        [DisableContextMenu(true, true), ShowIf("$showAutoCorrection")]
        public CustomSelectBox _autoCorrection = new CustomSelectBox("是否自动修正");

        public bool autoCorrection
        {
            get
            {
                return ProjectScanGlobalConfig.openAutoCorrection && _autoCorrection.enable;
            }
        }

        /// <summary>
        /// 是否显示将当前规则应用到后处理，默认false，要开启的在外部规则设置
        /// </summary>
        [DisableContextMenu(true, true), HideInInspector, HideInPlayMode]
        public bool showAutoPostProcessor = false;

        /// <summary>
        /// 是否将当前规则应用到后处理，前提是先开启显示将当前规则应用到后处理，因为有些检查逻辑不应该应用到后处理，显示出来容易误导
        /// </summary>
        [PropertyTooltip("注意，开启应用到后处理别忘了在扫描逻辑里面添加路径传参")]
        [DisableContextMenu(true, true), ShowIf("$showAutoPostProcessor")]
        [OnValueChanged("AutoPostProcessorChanged", true)]
        [InfoBox("注意，应用后处理时需[启用单独的目标文件夹配置]选项并配置相关目录，让资源精准定位到后处理类型")]
        public CustomSelectBox autoPostProcessor = new CustomSelectBox("将检查规则应用到后处理");
        private void AutoPostProcessorChanged()
        {
            if (autoPostProcessor.enable && !showAutoCorrection)
            {
                autoPostProcessor.enable = false;
                EditorUtility.DisplayDialog("提示", "当前检查规则开启了应用后处理，但是并没有开启自动修正功能，请开启。", "确定");
                return;
            }
            //开启后处理默认开启自动修正和启用单独的目标文件夹配置
            if (autoPostProcessor.enable)
            {
                _autoCorrection.enable = true;
            }
            //更新缓存的后处理规则
            ProjectScanGlobalConfig.InitOrUpdateAutoPostProcessorRule(true);
        }

        /// <summary>
        /// 检测结果详细列表，当在通用设置界面开启日志显示时会自动展示出来
        /// </summary>
        [NonSerialized]
        // [PropertySpace(SpaceBefore = 5)]
        // [DisableContextMenu(true, true), LabelText("检查结果"), NonSerialized, ShowInInspector, PropertyOrder(999)]
        // [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false, HideRemoveButton = true, HideAddButton = true, IsReadOnly = true)]
        // [ShowIf("@logDetails.Count > 0 && ProjectScanGlobalConfig.generalSetting.logSetting.pringLogToList.enable")]
        // [TableList(IsReadOnly = true, ShowIndexLabels = true, DrawScrollView = true, MinScrollViewHeight = 200, MaxScrollViewHeight = 400, AlwaysExpanded = true)]
        public List<LogDetail> logDetails = new List<LogDetail>();


        [NonSerialized]
        public bool _isScaning = false;
        public bool isScaning
        {
            get { return _isScaning; }
            set
            {
                this._isScaning = value;
            }
        }





















        /*--------------------------------------------------------------华丽的分割线--------------------------------------------------------------------*/
        /*--------------------------------------------------------------以下部分都是检测过程中的一些动态逻辑，不会序列化，序列化数据部分请填加在分割线前面--------------------------------------------------------------------*/

        /// <summary>
        /// 绑定的模块对象
        /// </summary>
        private UnityEngine.Object menuItem = null;

        /// <summary>
        /// 归属模块
        /// </summary>
        [NonSerialized]
        public EnumScanModes bindMode;

        /// <summary>
        /// 扫描资源总数
        /// </summary>
        [NonSerialized]
        public int scanAssetsCnt = 0;

        /// <summary>
        /// 注册绑定的模块对象，方便程序修改数据进行SetDirty操作
        /// </summary>
        public void RegisterMenuItem(UnityEngine.Object item)
        {
            menuItem = item;
        }

        /// <summary>
        /// 开始检查
        /// </summary>
        public void BeginScan()
        {
            logDetails.Clear();
            scanAssetsCnt = 0;
            isScaning = true;
        }

        /// <summary>
        /// 记录检查日志，如果外部实现了自动修正的逻辑则hasRealizeAutoCorrectionMethod传参为true
        /// </summary>
        /// <param name="context">异常对象</param>
        /// <param name="method">对应的函数检测名称</param>
        /// <param name="logDetail">说明细节</param>
        /// <param name="path">资源路径</param>
        /// <param name="hasRealizeAutoCorrectionMethod">是否有实现自动修正的方法，外部实现了则记得标记为true</param>
        public void Record(UnityEngine.Object context, string method, string logDetail, string path, bool hasRealizeAutoCorrectionMethod = false)
        {
            if (this.autoCorrection)
            {
                logDetail += " (已自动修正)";
                // 逻辑体验不好，废弃不用
                // if (hasRealizeAutoCorrectionMethod)
                // {
                //     logDetail += " (已自动修正)";
                // }
                // else
                // {
                //     logDetail += " (未实现自动修正细节，或当前检查项不适合自动修正)";
                // }
            }

            ProjectScanHelper.PrintProjectScanLog(context, this.ruleTitle, method, logDetail, path);
            // ProjectScanHelper.PrintLogToCustomView(this, context, logDetail, path);
            this.logDetails.Add(new LogDetail()
            {
                warnObj = context,
                warnInfo = logDetail,
                warnPath = path
            });
            ProjectScanLogsMgr.AddLog(context, this, logDetail, path);
            // builder.AppendLine($"{path}\t{logDetail}");
        }

        /// <summary>
        /// 结束检查
        /// </summary>
        public void EndScan()
        {
            StringBuilder builder = new StringBuilder();
            if (this.logDetails.Count == 0)
            {
                builder.AppendLine($"{ruleTitle} 检查完毕，无异常");
            }
            else
            {
                foreach (var item in logDetails)
                {
                    builder.AppendLine($"{item.warnPath}\t{item.warnInfo}");
                }
            }


            //表示有异常资源被自动化修正了
            if (this.autoCorrection && this.logDetails.Count > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            ProjectScanLogsMgr.Statisticsing(this);
            ProjectScanHelper.WriteAndOpenFile(builder, this);
            isScaning = false;
        }

        /// <summary>
        /// 检测结果表示资源是否已经被修改了
        /// </summary>
        public bool AssetsModifyed
        {
            get
            {
                return this.autoCorrection && this.logDetails.Count > 0;
            }
        }

    }
}
#endif