using System;
using System.Collections.Generic;
using System.IO;
using AillieoUtils;
using SGF.Unity;
using UnityEngine;
using UnityEngine.UI;


namespace LogModule
{

    public class LogReportView : MonoBehaviour
    {
        public ScrollViewEx SCView;

        public LogInfoItem logInfoItemPrefab;

        List<LogViewInfo> showLogInfos = new();
        List<LogViewInfo> logInfos = new();

        bool isMoveDown = true;

        bool dirty = false;

        public Button MoveDown;
        public Button CloseBtn;

        public Button ShowBtn;

        public GameObject panel;


        public List<Toggle> fillterToggles;
        List<LogType> defaultLogTypes = new() { LogType.Log, LogType.Warning, LogType.Exception, LogType.Assert, LogType.Error };
        HashSet<LogType> filterLogTypes = new();


        public List<Toggle> toggles_XIAOSHAN;

        List<LogType> LogTypes_XIAOSHAN = new() { LogType.Warning, LogType.Exception, LogType.Assert, LogType.Error };

        public List<Toggle> toggles_HTML;
        List<LogType> LogTypes_HTML = new() { LogType.Log, LogType.Warning, LogType.Exception, LogType.Assert, LogType.Error };

        public Button HtmlBtn;

        public Button OutputLogToFileBtn;

        private bool isOutputToFile = false;

        void Start()
        {

            SCView.SetUpdateFunc(OnRefesh);
            SCView.SetItemSizeFunc(OnItemSizeFunc);
            SCView.SetItemCountFunc(OnItemCountFunc);

            SCView.OnActionDrag += OnActionDrag;

            MoveDown.onClick.AddListener(OnMoveDownClick);
            CloseBtn.onClick.AddListener(OnCloseClick);
            ShowBtn.onClick.AddListener(OnShowClick);

            OutputLogToFileBtn.onClick.AddListener(OnOutputLogToFlieClick);


            for (int i = 0; i < fillterToggles.Count; i++)
            {
                var logType = defaultLogTypes[i];
                filterLogTypes.Add(logType);

                fillterToggles[i].onValueChanged.AddListener((isOn) =>
                {
                    // Debug.Log($"click ----> {i} , type: {logType} , isOn: {isOn}");
                    if (isOn)
                    {
                        if (!filterLogTypes.Contains(logType))
                        {
                            filterLogTypes.Add(logType);
                            OnFilterTypeChange();
                        }
                    }
                    else
                    {
                        if (filterLogTypes.Contains(logType))
                        {
                            filterLogTypes.Remove(logType);
                            OnFilterTypeChange();
                        }
                    }
                });
            }

            // 小闪上报 类型
            for (int i = 0; i < toggles_XIAOSHAN.Count; i++)
            {
                var logType = LogTypes_XIAOSHAN[i];
                //var idx = i;

                toggles_XIAOSHAN[i].isOn = LogReport.Instance.HasReportType(logType, LogReport.E_PostLogType.XIAOSHAN);
                toggles_XIAOSHAN[i].onValueChanged.AddListener((isOn) =>
                {
                    //Debug.Log($"[XiaoShan] click ----> {i} , idx: {idx}, type: {logType} , isOn: {isOn}");

                    LogReport.Instance.OnReportTypeChange(logType, isOn, LogReport.E_PostLogType.XIAOSHAN);
                }); ;
            }

            // html 上报类型
            for (int i = 0; i < toggles_HTML.Count; i++)
            {
                var logType = LogTypes_HTML[i];

                toggles_HTML[i].isOn = LogReport.Instance.HasReportType(logType, LogReport.E_PostLogType.HTML);

                toggles_HTML[i].onValueChanged.AddListener((isOn) =>
                {
                    //Debug.Log($"[html] click ----> {i} , type: {logType} , isOn: {isOn}");

                    LogReport.Instance.OnReportTypeChange(logType, isOn, LogReport.E_PostLogType.HTML);
                }); ;
            }

            HtmlBtn.onClick.AddListener(OnHtmlClick);

            LogReport.Instance.OnCatchErrorLog += OnCatchErrorLog;
        }

        void OnRefesh(int id, RectTransform tr)
        {
            if (showLogInfos.Count > id)
            {
                tr.GetComponent<LogInfoItem>().OnRefresh(showLogInfos[id]);
            }
        }

        Vector2 OnItemSizeFunc(int id)
        {
            if (showLogInfos.Count > id)
            {

                logInfoItemPrefab.GetComponent<LogInfoItem>().OnRefresh(showLogInfos[id]);
                RectTransform tr = logInfoItemPrefab.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(tr);
                return new Vector2(SCView.defaultItemSize.x, LayoutUtility.GetPreferredHeight(tr));
            }
            return SCView.defaultItemSize;
        }

        int OnItemCountFunc()
        {
            return showLogInfos.Count;
        }

        void OnActionDrag(bool isDrag)
        {
            if (isDrag)
            {
                isMoveDown = false;
            }
        }

        private void OnDisable()
        {
            dirty = false;
            DelayInvoker.CancelInvoke(this);
        }

        // 不是 编译器 并且是 start_dev 或者 gm 包才显示日志
#if (STAR_DEV || GM) && !UNITY_EDITOR
        // #if (STAR_DEV || GM)

        private void Update()
        {
        return;

            // 超过5000， 留一半, 下一帧再来读
            if (logInfos.Count >= 5000)
            {
                for (int i = 0; i < 2500; i++)
                {
                    LogReportPool.Release<LogViewInfo>(logInfos[i]);
                }
                logInfos = logInfos.GetRange(2499, 2500);

                RefreshShowLogInfos();
                return;
            }

            for (int i = 0; i < LogReport.Instance.GetFrameReadCount(); i++)
            {
                LogInfo logInfo = LogReport.Instance.ReadOne();

                if (logInfo == null || logInfo.LogString == string.Empty)
                {
                    break;
                }
                var logViewInfo = LogReportPool.Acquire<LogViewInfo>();
                logViewInfo.Update(logInfo);

                logInfos.Add(logViewInfo);

                AddShowLogInfo(logViewInfo);
            }

            if (!panel.activeInHierarchy)
            {
                return;
            }


            // 导航到 最下面
            if (isMoveDown)
            {
                MarkDirty();
            }
        }
#endif

        private bool CheckIsInFiltterType(LogViewInfo logViewInfo)
        {
            // 所有类型都有
            if (filterLogTypes.Count == 5)
            {
                return true;
            }

            if (filterLogTypes.Contains(logViewInfo.LogInfoType))
            {
                return true;
            }

            return false;
        }

        private void OnFilterTypeChange()
        {
            RefreshShowLogInfos();

            isMoveDown = true;

            MarkDirty();
        }

        private void RefreshShowLogInfos()
        {
            showLogInfos.Clear();

            logInfos.ForEach((logInfo) =>
            {
                AddShowLogInfo(logInfo);
            });

        }


        private void AddShowLogInfo(LogViewInfo logViewInfo)
        {
            if (CheckIsInFiltterType(logViewInfo))
            {
                showLogInfos.Add(logViewInfo);
            }

            OutputToFile(logViewInfo);
        }

        private void MarkDirty()
        {
            if (dirty)
            {
                return;
            }
            dirty = true;
            DelayInvoker.DelayInvoke(this, 0.1f, (args) =>
            {
                dirty = false;
                if (!isMoveDown)
                {
                    return;
                }
                if (showLogInfos.Count > 0)
                {
                    SCView.ScrollTo(showLogInfos.Count - 1);
                }
                else
                {
                    SCView.ScrollTo(0);
                }
            });
        }

        void OnMoveDownClick()
        {
            isMoveDown = true;

            MarkDirty();
        }

        void OnCloseClick()
        {
            isMoveDown = false;
            dirty = false;
            panel.SetActive(false);
        }

        void OnShowClick()
        {
            isMoveDown = true;
            panel.SetActive(true);

            MarkDirty();
        }

        void OnCatchErrorLog()
        {
            ShowBtn.gameObject.SetActive(true);
        }

        void OnHtmlClick()
        {
            LogReport.Instance.OpenHtmlReport();
            Application.OpenURL("http://10.225.254.180:9001/logtail/MyPanic");
        }

        void OnOutputLogToFlieClick()
        {
            if (isOutputToFile)
            {
                return;
            }
            isOutputToFile = true;

            foreach (var item in logInfos)
            {
                OutputToFile(item);
            }

        }

        void OutputToFile(LogViewInfo logViewInfo)
        {
            if (!isOutputToFile)
            {
                return;
            }

            LogToFile(logViewInfo.LogString);

        }
        public StreamWriter LogFileWriter = null;

        private string LogFileName = "";

        private string LogFileDir = "";
        // debuger 日志 是实时开启的, 所以此处加个 reportLog. 在需要保存的时候 写文件
        private void LogToFile(string message)
        {
            // 采用 Debuger  的日志路径
            LogFileDir = SGF.Debuger.LogFileDir;

            if (LogFileWriter == null)
            {
                LogFileName = DateTime.Now.GetDateTimeFormats('s')[0].ToString();
                LogFileName = LogFileName.Replace("-", "_");
                LogFileName = LogFileName.Replace(":", "_");
                LogFileName = LogFileName.Replace(" ", "");
                LogFileName += "_reportLog.log";
                if (string.IsNullOrEmpty(LogFileDir))
                {
                    try
                    {
                        if (SGF.Debuger.UseUnityEngine)
                        {
                            LogFileDir = Application.persistentDataPath + "/DebugerLog/";
                        }
                        else
                        {
                            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            LogFileDir = baseDirectory + "/DebugerLog/";
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(SGF.Debuger.Prefix + "获取 Application.persistentDataPath 报错！" + ex.Message);
                        return;
                    }
                }

                string path = LogFileDir + LogFileName;
                try
                {
                    if (!Directory.Exists(LogFileDir))
                    {
                        Directory.CreateDirectory(LogFileDir);
                    }

                    LogFileWriter = File.AppendText(path);
                    LogFileWriter.AutoFlush = true;
                    UnityEngine.Debug.Log("LogTo path " + path);

                }
                catch (Exception ex2)
                {
                    LogFileWriter = null;
                    UnityEngine.Debug.LogError("LogToCache() " + ex2.Message + ex2.StackTrace);
                    return;
                }
            }

            if (LogFileWriter == null)
            {
                return;
            }

            try
            {
                LogFileWriter.WriteLine(message);
            }
            catch (Exception)
            {
            }
        }


    }
}
