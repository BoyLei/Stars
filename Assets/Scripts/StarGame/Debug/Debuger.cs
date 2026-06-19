
using System;
using System.Collections.Generic;
using System.IO;
using GSSDK;
using StarProject.Service.SDK;
using StarProject.Service.Time;
using UnityEngine;

namespace SGF
{
    public class Debuger
    {
        public static bool EnableLog;

        public static bool EnableTime = true;

        public static bool EnableSave = false;

        public static bool EnableStack = false;

        public static string LogFileDir = "";

        public static string LogFileName = "";

        public static string Prefix = SystemInfo.deviceName + "> ";

        public static StreamWriter LogFileWriter = null;

        public static bool UseUnityEngine = true;

        private static void Internal_Log(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                Debug.Log(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        private static void Internal_LogWarning(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                Debug.LogWarning(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }


        private static void Internal_LogError(string msg, object context = null)
        {
            if (UseUnityEngine)
            {
                Debug.LogError(msg, (UnityEngine.Object)context);
            }
            else
            {
                Console.WriteLine(msg);
            }
            //Log正常打，这里只是一层封装


        }

        public static void Log(object message)
        {
            if (EnableLog)
            {
                string text = GetLogTime() + message;
                Debug.Log(Prefix + text, null);
                LogToFile("[I]" + text);
            }
        }
        private static Dictionary<string, object> postEventString = new();
        private static string histroyString = "";
        /// <summary>
        /// prepare的时候早就准备好了Debuger
        /// 会拦unity,Error;
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        public static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            //Internal_LogError
            /* 两边流程：unity特殊的比如丢失引用的，这里上报bugly;   比如我们报错error的也会被unity收集到bugly上报；
              系统错误和非错误的，也可以通过这里上报！！*/

            switch (type)
            {

                case LogType.Assert:
                    break;
                case LogType.Warning:
                    break;
                case LogType.Log:
                    break;
                case LogType.Error:
                case LogType.Exception:

                    // 加个 简单的日志上报过滤
                    if (histroyString != condition)
                    {
                        histroyString = condition;

                        postEventString.Clear();
                        postEventString.Add("category", 4);     //日志来源，默认 4 Unity 日志
                        postEventString.Add("name", 6);         //日志类型，Log.ERROR = 6

                        postEventString.Add("reason",
                            "msg:[E_Bugly_Foo]: " +
                            "FPS:" + ShowFps.FPS +
                            "LAG:" + TimeManager.Instance.AveragePing + "ms;" +
                            Prefix + GetLogTime() + type.ToString() +
                            "condition:" + condition + "stackTrace:" + stackTrace);

                        postEventString.Add("stackTrace:", StackTraceUtility.ExtractStackTrace());

                        // SGF.Debuger.Log($"[Log] : {condition} , stackTrace: {stackTrace} , type: {type} ");
                        SDKManager.Instance.ReportBuglyLog(postEventString);
                    }

                    break;
                default:
                    break;
            }
        }

        public static void Log(object message, object context)
        {
            if (EnableLog)
            {
                string text = GetLogTime() + message;
                Internal_Log(Prefix + text, context);
                LogToFile("[I]" + text);
            }
        }

        public static void LogError(object message)
        {
            string text = GetLogTime() + message;
            Internal_LogError(Prefix + text);
            LogToFile("[E]" + text, EnableStack: true);
        }

        public static void LogError(object message, object context)
        {
            string text = GetLogTime() + message;
            Internal_LogError(Prefix + text, context);
            LogToFile("[E]" + text, EnableStack: true);
        }

        public static void LogWarning(object message)
        {
            string text = GetLogTime() + message;
            Internal_LogWarning(Prefix + text);
            LogToFile("[W]" + text);
        }

        public static void LogWarning(object message, object context)
        {
            string text = GetLogTime() + message;
            Internal_LogWarning(Prefix + text, context);
            LogToFile("[W]" + text);
        }

        public static void Log(string tag, string message)
        {
            if (EnableLog)
            {
                message = GetLogText(tag, message);
                Internal_Log(Prefix + message);
                LogToFile("[I]" + message);
            }
        }

        public static void Log(string tag, string format, params object[] args)
        {
            if (EnableLog)
            {
                string logText = GetLogText(tag, string.Format(format, args));
                Internal_Log(Prefix + logText);
                LogToFile("[I]" + logText);
            }
        }

        public static void LogError(string tag, string message)
        {
            message = GetLogText(tag, message);
            Internal_LogError(Prefix + message);
            LogToFile("[E]" + message, EnableStack: true);
        }

        public static void LogError(string tag, string format, params object[] args)
        {
            string logText = GetLogText(tag, string.Format(format, args));
            Internal_LogError(Prefix + logText);
            LogToFile("[E]" + logText, EnableStack: true);
        }

        public static void LogWarning(string tag, string message)
        {
            message = GetLogText(tag, message);
            Internal_LogWarning(Prefix + message);
            LogToFile("[W]" + message);
        }

        public static void LogWarning(string tag, string format, params object[] args)
        {
            string logText = GetLogText(tag, string.Format(format, args));
            Internal_LogWarning(Prefix + logText);
            LogToFile("[W]" + logText);
        }

        private static string GetLogText(string tag, string message)
        {
            string text = "";
            if (EnableTime)
            {
                text = DateTime.Now.ToString("HH:mm:ss.fff") + " ";
            }

            return text + tag + "::" + message;
        }

        private static string GetLogTime()
        {
            string result = "";
            if (EnableTime)
            {
                result = DateTime.Now.ToString("HH:mm:ss.fff") + " ";
            }

            return result;
        }

        private static void LogToFile(string message, bool EnableStack = false)
        {
            if (!EnableSave)
            {
                return;
            }

            if (LogFileWriter == null)
            {
                LogFileName = DateTime.Now.GetDateTimeFormats('s')[0].ToString();
                LogFileName = LogFileName.Replace("-", "_");
                LogFileName = LogFileName.Replace(":", "_");
                LogFileName = LogFileName.Replace(" ", "");
                LogFileName += ".log";
                if (string.IsNullOrEmpty(LogFileDir))
                {
                    try
                    {
                        if (UseUnityEngine)
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
                        Internal_LogError(Prefix + "获取 Application.persistentDataPath 报错！" + ex.Message);
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
                }
                catch (Exception ex2)
                {
                    LogFileWriter = null;
                    Internal_LogError("LogToCache() " + ex2.Message + ex2.StackTrace);
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
                if ((EnableStack || Debuger.EnableStack) && UseUnityEngine)
                {
                    LogFileWriter.WriteLine(StackTraceUtility.ExtractStackTrace());
                }
            }
            catch (Exception)
            {
            }
        }
    }

}

