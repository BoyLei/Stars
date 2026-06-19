using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using SGF.Module.Framework;
using StarProject.Service.User;
using Unity.Mathematics;
using UnityEngine;
namespace LogModule
{
    public class LogReport : ServiceModule<LogReport>
    {


        // static string timeFormat = "HH:mm:ss,fff";
        public enum E_PostLogType
        {
            XIAOSHAN,
            HTML
        }


        LinkedList<LogInfo> logInfos = new();

        LinkedListNode<LogInfo> cur = null;
        public int lineLimit = 200;

        // html 上报日志类型
        public HashSet<LogType> LogReportTypes_HTML = new() { LogType.Error, LogType.Assert, LogType.Exception, LogType.Log };

        // 小闪 上报日志类型
        public HashSet<LogType> LogReportTypes_XIAOSHAN = new() { LogType.Error, LogType.Assert, LogType.Exception };

        public Action OnCatchErrorLog;

        /// <summary>
        /// 是否上报 html 的 标签
        /// </summary> 
        bool FLAG_HTML = false;

        public void Init()
        {

/*#if (STAR_DEV || GM) && !UNITY_EDITOR
// #if (STAR_DEV || GM)

            Application.logMessageReceivedThreaded += HandleLog;

#endif
            cur = logInfos.First;*/
        }

        private static StringBuilder sb = new();

        public static string GetLogKey()
        {
            sb.Clear();
            if (StarProject.Service.SDK.SDKManager.Instance.IsInit)
            {
                sb.Append(StarProject.Service.SDK.SDKManager.Instance.SdkUserID);
            }
            sb.Append("_");
            if (UserManager.Instance.MainUserData != null)
            {
                sb.Append(UserManager.Instance.MainUserData?.playerRoleId);
            }

            return sb.ToString();
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            // 用Loom的方法在Unity主线程中调用Text组件
            SGF.Unity.Loom.QueueOnMainThread((param) =>
            {
                // 相同的日志过滤
                if (logInfos.Count > 0 && logInfos.Last.Equals(logString))
                {
                    return;
                }

                LogInfo logInfo;

                bool curIsFirstLogInfo = false;

                if (logInfos.Count > lineLimit)
                {
                    if (cur == logInfos.First)
                    {
                        curIsFirstLogInfo = true;
                    }
                    logInfo = logInfos.First.Value;
                    logInfos.RemoveFirst();
                }
                else
                {
                    logInfo = new LogInfo();
                }

                logInfo.Update(logString, stackTrace, type);

                logInfos.AddLast(logInfo);

                // 如果当前指针指向了第一个日志节点， 并且这个节点被后续日志信息给刷掉了, 那就将这个指针指向 当前的首节点
                if (curIsFirstLogInfo || cur == null)
                {
                    cur = logInfos.First;
                }

                ReportLog(logInfo);


            }, null);
        }

        public int GetFrameReadCount()
        {
            if (logInfos.Count == 0)
            {
                return 0;
            }

            if (logInfos.Count < 10)
            {
                return 1;
            }

            if (logInfos.Count < 500)
            {
                return logInfos.Count / 10;
            }

            if (logInfos.Count < 2000)
            {
                return logInfos.Count / 20;
            }

            return logInfos.Count / 30;
        }

        public LogInfo ReadOne()
        {
            LogInfo logInfo = null;

            do
            {
                if (logInfos.Count == 0)
                {
                    break;
                }

                if (cur.Value.Readed && cur.Next == null)
                {
                    break;
                }

                // 如果已经读过 , 根据上一个 判定, cur.Next 一定不为null
                if (cur.Value.Readed)
                {
                    cur = cur.Next;
                }
                else
                {
                    // 如果没有读过,那就 把此次读到的数据 输出
                    logInfo = cur.Value;
                    logInfo.Read();
                }

                // 指针指向下一个
                if (cur.Next != null)
                {
                    cur = cur.Next;
                }

                break;
            } while (true);

            return logInfo;
        }

        public bool HasReportType(LogType logType, E_PostLogType e_PostLogType)
        {
            switch (e_PostLogType)
            {
                case E_PostLogType.XIAOSHAN:
                    {
                        return HasReportType(LogReportTypes_XIAOSHAN, logType);
                    }
                case E_PostLogType.HTML:
                    {
                        return HasReportType(LogReportTypes_HTML, logType);
                    }
            }

            return true;
        }

        bool HasReportType(HashSet<LogType> logTypes, LogType logType)
        {
            return logTypes.Contains(logType);
        }

        public void OnReportTypeChange(LogType logType, bool isOn, E_PostLogType e_PostLogType)
        {
            switch (e_PostLogType)
            {
                case E_PostLogType.XIAOSHAN:
                    {
                        ChangeReportType(LogReportTypes_XIAOSHAN, logType, isOn);
                    }
                    break;
                case E_PostLogType.HTML:
                    {
                        ChangeReportType(LogReportTypes_HTML, logType, isOn);
                    }
                    break;
            }
        }

        void ChangeReportType(HashSet<LogType> target, LogType logType, bool isOn)
        {
            if (isOn)
            {
                if (!target.Contains(logType))
                {
                    target.Add(logType);
                }
            }
            else
            {
                if (target.Contains(logType))
                {
                    target.Remove(logType);
                }
            }
        }

        void ReportLog(LogInfo logInfo)
        {

            ReportHtml(logInfo);

            ReportXiaoShan(logInfo);

            // if ((int)logInfo.LogInfoType > (int)LogType.Warning || logInfo.LogInfoType == LogType.Exception)
            // {

            // }

            // 当捕获到错误日志的时候, 通知外面
            if (logInfo.LogInfoType == LogType.Error)
            {
                OnCatchErrorLog?.Invoke();
            }
        }

        public void OpenHtmlReport()
        {
            FLAG_HTML = true;
        }

        void ReportHtml(LogInfo logInfo)
        {
            if (!FLAG_HTML)
            {
                return;
            }

            if (!LogReportTypes_HTML.Contains(logInfo.LogInfoType))
            {
                return;
            }
            string url = "http://10.225.254.180:7081/client";

            HttpUtils.SendPostHttp(url, logInfo);

        }


        /// <summary>
        /// 日志上报小闪
        /// 小闪日志 单独上报 error 日志.
        /// </summary>
        void ReportXiaoShan(LogInfo logInfo)
        {

            if (!LogReportTypes_XIAOSHAN.Contains(logInfo.LogInfoType))
            {
                return;
            }

            string url = "http://10.225.254.180:7081/panic";

            HttpUtils.SendPostHttp(url, FormateXiaoShanLog(logInfo));
        }


        ClientPanicMD FormateXiaoShanLog(LogInfo logInfo)
        {
            string pattern = @"\b\d{2}:\d{2}:\d{2}\.\d{3}\b";
            string result = Regex.Replace(logInfo.ToString(), pattern, "");

            clientPanicMD.CrashInfo = result;
            clientPanicMD.OpenID = GetLogKey();
            clientPanicMD.Platform = Application.platform.ToString();

            clientPanicMD.Memory = SystemInfo.systemMemorySize + "KB";

            clientPanicMD.BuildVersion = GameApp.Instance.LocalVersion.String();


            return clientPanicMD;
        }

        ClientPanicMD clientPanicMD = new();
        /// <summary>
        /// 上报小闪需要的数据结构
        /// </summary>
        class ClientPanicMD
        {
            public string MD5;
            //版本号
            public string BuildVersion;
            //渲染设置等级
            public string RenderLevel;
            //运营平台
            public string Platform;
            //机器型号
            public string DeviceModel;
            //运行时信息-CPU
            public string Cpu;
            //运行时信息-GPU
            public string Gpu;
            //运行时信息-内存
            public string Memory;
            //用户ID
            public string OpenID;
            //用户名
            public string UserName;
            //崩溃信息
            public string CrashInfo;
            //系统平台
            public string SystemPlatform;
            //设备系统号
            public string SystemVersion;
        }


    }
}
