using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace GameDLL
{
    public class GLog
    {
        public enum Type
        {
            Error = LogType.Error,
            Assert = LogType.Assert,
            Warning = LogType.Warning,
            Log = LogType.Log,
            Exception = LogType.Exception,

            Message,
            Others = -1,
        }

        public const int LogTypeError = 0;
        public const int LogTypeAssert = 1;
        public const int LogTypeWarning = 2;
        public const int LogTypeInfo = 3;
        public const int LogTypeException = 4;
        public const int LogTypeMessage = 5;
        public const int LogTypeGameDLL = 6;
        public const int LogTypeStatistics = 7;
        public const int LogTypeUGUI = 8;

        const int LogTypeCustomFirst = 9;


        public const ulong LogErrorFlag = 1 << LogTypeError;
        public const ulong LogAssertFlag = 1 << LogTypeAssert;
        public const ulong LogWarningFlag = 1 << LogTypeWarning;
        public const ulong LogInfoFlag = 1 << LogTypeInfo;
        public const ulong LogExceptionFlag = 1 << LogTypeException;
        public const ulong LogMessageFlag = 1 << LogTypeMessage;
        public const ulong LogBasicFlag = LogErrorFlag | LogAssertFlag | LogWarningFlag | LogInfoFlag | LogExceptionFlag | LogMessageFlag;
        public const ulong LogAllFlag = ulong.MaxValue;

        private static ulong logFlag = LogBasicFlag;
        private static string[] logNames = new string[64];

        private const string CustomTagFormat = "[{0}]";
        private const string CustomTagColorFormat = "<color=#{0}>[{1}]</color> ";
        private static string[] customColorTag = new string[64];

        private static IConsole defaultConsole = new UnityConsole();

        private static bool assertionsOpened = false;

        private static bool checkAssertions = true;

        public static bool CheckAssertions
        {
            get
            {
                return checkAssertions;
            }
            set
            {
                checkAssertions = value;
            }
        }

        public static IConsole customConsole;

        static IConsole console
        {
            get
            {
                if (customConsole != null)
                    return customConsole;
                return defaultConsole;
            }
        }

        static GLog()
        {
            logNames[0] = "Error";
            customColorTag[0] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(Color.red), logNames[0]);
            logNames[1] = "Assert";
            customColorTag[1] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(new Color(1f, 0.5f, 0f)), logNames[1]);
            logNames[2] = "Warning";
            customColorTag[2] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(new Color(1f, 0.84f, 0.02f)), logNames[2]);
            logNames[3] = "Info";
            customColorTag[3] = string.Format(CustomTagFormat, logNames[3]);
            logNames[4] = "Exception";
            customColorTag[4] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(new Color(0.616f, 0f, 1f)), logNames[4]);
            logNames[5] = "Message";
            customColorTag[5] = string.Format(CustomTagFormat, logNames[5]);
            logNames[6] = "GameDLL";
            customColorTag[6] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(Color.magenta), logNames[6]);
            logNames[7] = "Statistics";
            customColorTag[7] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(Color.cyan), logNames[7]);
            logNames[8] = "UGUI";
            customColorTag[8] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(Color.yellow), logNames[8]);

            AssertionsCheck();
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        private static void AssertionsCheck()
        {
            assertionsOpened = true;
        }

        /// <summary>
        /// 设置Unity打印级别
        /// <para>级别Error Error(True) Assert(False) Warning(False) Log(False) Exception(True)</para>
        /// <para>级别Assert Error(True) Assert(True) Warning(False) Log(False) Exception(True)</para>
        /// <para>级别Warning Error(True) Assert(True) Warning(True) Log(False) Exception(True)</para>
        /// <para>级别Log Error(True) Assert(True) Warning(True) Log(True) Exception(True)</para>
        /// <para>级别Exception Error(False) Assert(False) Warning(False) Log(False) Exception(True)</para>
        /// </summary>
        public static void SetUnityLogLevel(LogType type)
        {
            Debug.unityLogger.filterLogType = type;
        }

        /// <summary>
        /// 设置Unity打印开关
        /// </summary>
        public static bool LogEnable
        {
            get
            {
                return console.LogEnable;
                //return iLog.logEnabled;
            }
            set
            {
                console.LogEnable = value;
                //iLog.logEnabled = value;
            }
        }

        /// <summary>
        /// 添加本地打印信息类型
        /// </summary>
        public static void AddLogType(int flag)
        {
            logFlag |= 1ul << flag;
        }

        /// <summary>
        /// 移除本地打印信息类型
        /// </summary>
        public static void RemoveLogType(int flag)
        {
            logFlag &= ~(1ul << flag);
        }

        /// <summary>
        /// 本地打印信息类型是否打开
        /// </summary>
        public static bool IsLogOpened(int flag)
        {
            return (logFlag & (1ul << flag)) != 0;
        }

        /// <summary>
        /// 设置本地打印信息类型
        /// </summary>
        public static void SetLogFlag(ulong flag)
        {
            logFlag = flag;
        }

        /// <summary>
        /// 获取本地打印信息类型
        /// </summary>
        public static ulong GetLogFlag()
        {
            return logFlag;
        }

        public static int AddLogName(string name, bool addName = true)
        {
            if (string.IsNullOrEmpty(name))
                return -1;
            int i = 0;
            for (; i < logNames.Length; i++)
            {
                if (logNames[i] == name)
                    break;
                if (addName && logNames[i] == null)
                    break;
            }
            if (logNames.Length == i)
                return -1;
            if (addName)
            {
                logNames[i] = name;
                customColorTag[i] = "[" + name + "] ";
            }
            AddLogType(i);
            return i;
        }

        public static int AddLogName(string name, Color color)
        {
            if (string.IsNullOrEmpty(name))
                return -1;
            int i = 0;
            for (; i < logNames.Length; i++)
            {
                if (logNames[i] == name)
                    break;
                if (logNames[i] == null)
                    break;
            }
            if (logNames.Length == i)
                return -1;
            logNames[i] = name;
            customColorTag[i] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(color), name);
            AddLogType(i);
            return i;
        }

        public static bool AddLogName(string name, int type, bool replace = false)
        {
            if (RegisterLog(name, type, replace))
            {
                AddLogType(type);
                return true;
            }
            return false;
        }

        public static bool RegisterLog(string name, int type, bool replace = false)
        {
            if (!string.IsNullOrEmpty(name) && type >= LogTypeCustomFirst && type < logNames.Length)
            {
                if (logNames[type] == null)
                {
                    logNames[type] = name;
                    customColorTag[type] = string.Format(CustomTagFormat, name);
                    return true;
                }
                else if (replace)
                {
                    if (string.IsNullOrEmpty(customColorTag[type]))
                        customColorTag[type] = string.Format(CustomTagFormat, name);
                    else
                        customColorTag[type] = customColorTag[type].Replace("[" + logNames[type] + "]", "[" + name + "]");
                    logNames[type] = name;
                    return true;
                }
                LogFormat("Log [{0}], Type = {1} has been registered", name, type);
                return false;
            }
            if (string.IsNullOrEmpty(name))
                Log("Log name is Empty!");
            if (string.IsNullOrEmpty(name))
                LogFormat("Type {0} is outrange({1}~{2})!", type, LogTypeCustomFirst, logNames.Length - 1);
            return false;
        }

        public static bool AddLogName(string name, int type, Color color, bool replace = false)
        {
            if (RegisterLog(name, type, color, replace))
            {
                AddLogType(type);
                return true;
            }
            return false;
        }

        public static bool RegisterLog(string name, int type, Color color, bool replace = false)
        {
            if (!string.IsNullOrEmpty(name) && type >= LogTypeCustomFirst && type < logNames.Length)
            {
                if (logNames[type] == null || replace)
                {
                    logNames[type] = name;
                    customColorTag[type] = string.Format(CustomTagColorFormat, ColorUtility.ToHtmlStringRGB(color), name);
                    return true;
                }
                LogFormat("Log [{0}], Type = {1} has been registered", name, type);
                return false;
            }
            if (string.IsNullOrEmpty(name))
                Log("Log name is Empty!");
            if (string.IsNullOrEmpty(name))
                LogFormat("Type {0} is outrange({1}~{2})!", type, LogTypeCustomFirst, logNames.Length - 1);
            return false;
        }

        public static int RemoveLogName(string name, bool clearName = false)
        {
            if (string.IsNullOrEmpty(name))
                return -1;
            int i = 0;
            for (; i < logNames.Length; i++)
            {
                if (logNames[i] == name)
                    break;
            }
            if (logNames.Length == i)
                return -1;
            if (clearName && i >= LogTypeCustomFirst)
                logNames[i] = null;
            if (clearName)
                customColorTag[i] = null;
            RemoveLogType(i);
            return i;
        }

        public static int GetLogType(string name, bool addName = false)
        {
            if (string.IsNullOrEmpty(name))
                return -1;
            for (int i = 0; i < logNames.Length; i++)
            {
                if (logNames[i] == name)
                    return i;
                if (addName && logNames[i] == null)
                {
                    logNames[i] = name;
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 本地打印信息类型是否打开
        /// </summary>
        public static bool IsLogOpened(string name)
        {
            for (int i = 0; i < logNames.Length; i++)
            {
                if (logNames[i] == name)
                    return IsLogOpened(i);
            }
            return false;
        }

        /// <summary>
        /// 打印错误信息
        /// </summary>
        public static void Error(object message)
        {
            if (IsLogOpened(LogTypeError))
                console.Log(Type.Error, message);
        }

        /// <summary>
        /// 打印错误信息及消息对象
        /// </summary>
        public static void Error(object message, Object context)
        {
            if (IsLogOpened(LogTypeError))
                console.Log(Type.Error, message, context);
        }

        /// <summary>
        /// 打印格式化错误信息
        /// </summary>
        public static void ErrorFormat(string message, object arg1)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, message, arg1);
        }

        /// <summary>
        /// 打印格式化错误信息
        /// </summary>
        public static void ErrorFormat(string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, message, arg1, arg2);
        }

        /// <summary>
        /// 打印格式化错误信息
        /// </summary>
        public static void ErrorFormat(string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印格式化错误信息
        /// </summary>
        public static void ErrorFormat(string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印格式化错误信息
        /// </summary>
        public static void ErrorFormat(string message, params object[] args)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, message, args);
        }

        /// <summary>
        /// 打印格式化错误信息及消息对象
        /// </summary>
        public static void ErrorFormat(Object context, string message, object arg1)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, message, arg1);
        }

        /// <summary>
        /// 打印格式化错误信息及消息对象
        /// </summary>
        public static void ErrorFormat(Object context, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, message, arg1, arg2);
        }

        /// <summary>
        /// 打印格式化错误信息及消息对象
        /// </summary>
        public static void ErrorFormat(Object context, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印格式化错误信息及消息对象
        /// </summary>
        public static void ErrorFormat(Object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印格式化错误信息及消息对象
        /// </summary>
        public static void ErrorFormat(Object context, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, message, args);
        }

        /// <summary>
        /// 打印带tag错误信息
        /// </summary>
        public static void TagError(object message, string tag)
        {
            if (IsLogOpened(LogTypeError))
                console.LogTag(Type.Error, message, tag);
        }

        /// <summary>
        /// 打印带tag错误信息
        /// </summary>
        public static void TagError(Object context, object message, string tag)
        {
            if (IsLogOpened(LogTypeError))
                console.LogTag(Type.Error, context, message, tag);
        }

        /// <summary>
        /// 打印断言信息
        /// </summary>
        public static void Assertion(object message)
        {
            if (IsLogOpened(LogTypeAssert))
                console.Log(Type.Assert, message);
        }

        /// <summary>
        /// 打印断言信息及消息对象
        /// </summary>
        public static void Assertion(object message, Object context)
        {
            if (IsLogOpened(LogTypeAssert))
                console.Log(Type.Assert, message, context);
        }

        /// <summary>
        /// 打印格式化断言信息
        /// </summary>
        public static void AssertionFormat(string message, object arg1)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, message, arg1);
        }

        /// <summary>
        /// 打印格式化断言信息
        /// </summary>
        public static void AssertionFormat(string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, message, arg1, arg2);
        }

        /// <summary>
        /// 打印格式化断言信息
        /// </summary>
        public static void AssertionFormat(string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印格式化断言信息
        /// </summary>
        public static void AssertionFormat(string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印格式化断言信息
        /// </summary>
        public static void AssertionFormat(string message, params object[] args)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, message, args);
        }

        /// <summary>
        /// 打印格式化断言信息及消息对象
        /// </summary>
        public static void AssertionFormat(Object context, string message, object arg1)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, context, message, arg1);
        }

        /// <summary>
        /// 打印格式化断言信息及消息对象
        /// </summary>
        public static void AssertionFormat(Object context, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, context, message, arg1, arg2);
        }

        /// <summary>
        /// 打印格式化断言信息及消息对象
        /// </summary>
        public static void AssertionFormat(Object context, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, context, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印格式化断言信息及消息对象
        /// </summary>
        public static void AssertionFormat(Object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, context, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印格式化断言信息及消息对象
        /// </summary>
        public static void AssertionFormat(Object context, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeAssert))
                console.LogFormat(Type.Assert, context, message, args);
        }

        /// <summary>
        /// 打印警告信息
        /// </summary>
        public static void Warning(object message)
        {
            if (IsLogOpened(LogTypeWarning))
                console.Log(Type.Warning, message);
        }

        /// <summary>
        /// 打印警告信息及消息对象
        /// </summary>
        public static void Warning(object message, Object context)
        {
            if (IsLogOpened(LogTypeWarning))
                console.Log(Type.Warning, message, context);
        }

        /// <summary>
        /// 打印格式化警告信息
        /// </summary>
        public static void WarningFormat(string message, object arg1)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, message, arg1);
        }

        /// <summary>
        /// 打印格式化警告信息
        /// </summary>
        public static void WarningFormat(string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, message, arg1, arg2);
        }

        /// <summary>
        /// 打印格式化警告信息
        /// </summary>
        public static void WarningFormat(string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印格式化警告信息
        /// </summary>
        public static void WarningFormat(string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印格式化警告信息
        /// </summary>
        public static void WarningFormat(string message, params object[] args)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, message, args);
        }

        /// <summary>
        /// 打印格式化警告信息及消息对象
        /// </summary>
        public static void WarningFormat(Object context, string message, object arg1)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, message, arg1);
        }

        /// <summary>
        /// 打印格式化警告信息及消息对象
        /// </summary>
        public static void WarningFormat(Object context, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, message, arg1, arg2);
        }

        /// <summary>
        /// 打印格式化警告信息及消息对象
        /// </summary>
        public static void WarningFormat(Object context, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印格式化警告信息及消息对象
        /// </summary>
        public static void WarningFormat(Object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印格式化警告信息及消息对象
        /// </summary>
        public static void WarningFormat(Object context, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, message, args);
        }

        /// <summary>
        /// 打印带tag警告信息
        /// </summary>
        public static void TagWarning(object message, string tag)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogTag(Type.Warning, message, tag);
        }

        /// <summary>
        /// 打印带tag警告信息
        /// </summary>
        public static void TagWarning(Object context, object message, string tag)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogTag(Type.Warning, context, message, tag);
        }

        /// <summary>
        /// 打印日志信息
        /// </summary>
        public static void Log(object message)
        {
            if (IsLogOpened(LogTypeInfo))
                console.Log(Type.Log, message);
        }

        /// <summary>
        /// 打印日志信息及消息对象
        /// </summary>
        public static void Log(object message, Object context)
        {
            if (IsLogOpened(LogTypeInfo))
                console.Log(Type.Log, message, context);
        }

        /// <summary>
        /// 打印格式化日志信息
        /// </summary>
        public static void LogFormat(string message, object arg1)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, message, arg1);
        }

        /// <summary>
        /// 打印格式化日志信息
        /// </summary>
        public static void LogFormat(string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, message, arg1, arg2);
        }

        /// <summary>
        /// 打印格式化日志信息
        /// </summary>
        public static void LogFormat(string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印格式化日志信息
        /// </summary>
        public static void LogFormat(string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印格式化日志信息
        /// </summary>
        public static void LogFormat(string message, params object[] args)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, message, args);
        }

        /// <summary>
        /// 打印格式化日志信息及消息对象
        /// </summary>
        public static void LogFormat(Object context, string message, object arg1)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, context, message, arg1);
        }

        /// <summary>
        /// 打印格式化日志信息及消息对象
        /// </summary>
        public static void LogFormat(Object context, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, context, message, arg1, arg2);
        }

        /// <summary>
        /// 打印格式化日志信息及消息对象
        /// </summary>
        public static void LogFormat(Object context, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, context, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印格式化日志信息及消息对象
        /// </summary>
        public static void LogFormat(Object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, context, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印格式化日志信息及消息对象
        /// </summary>
        public static void LogFormat(Object context, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogFormat(Type.Log, context, message, args);
        }

        /// <summary>
        /// 打印带颜色格式化日志信息
        /// </summary>
        public static void LogColor(Color color, string message, object arg1)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogColor(color, message, arg1);
        }

        /// <summary>
        /// 打印带颜色格式化日志信息
        /// </summary>
        public static void LogColor(Color color, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogColor(color, message, arg1, arg2);
        }

        /// <summary>
        /// 打印带颜色格式化日志信息
        /// </summary>
        public static void LogColor(Color color, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogColor(color, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印带颜色格式化日志信息
        /// </summary>
        public static void LogColor(Color color, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogColor(color, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印带颜色格式化日志信息
        /// </summary>
        public static void LogColor(Color color, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogColor(color, message, args);
        }

        /// <summary>
        /// 打印带tag日志信息
        /// </summary>
        public static void TagInfo(object message, string tag)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogTag(Type.Log, message, tag);
        }

        /// <summary>
        /// 打印带tag日志信息
        /// </summary>
        public static void TagInfo(Object context, object message, string tag)
        {
            if (IsLogOpened(LogTypeInfo))
                console.LogTag(Type.Log, context, message, tag);
        }

        /// <summary>
        /// 打印异常信息
        /// </summary>
        public static void Exception(Exception exc)
        {
            if (IsLogOpened(LogTypeException))
                console.LogException(exc);
            //Debug.LogException(exc);
        }

        /// <summary>
        /// 打印异常信息及消息对象
        /// </summary>
        public static void Exception(Exception exc, Object context)
        {
            if (IsLogOpened(LogTypeException))
                console.LogException(exc, context);
        }

        const string WatchStr = "<watch>{0}</watch>\n{1}";
        public static void Watch(string name, object message)
        {
            console.LogFormat(Type.Log, WatchStr, name, message);
        }

        public static void Watch(Object context, string name, object message)
        {
            console.LogFormat(Type.Log, context, WatchStr, name, message);
        }

        public static void Print(string message, Type type = Type.Log)
        {
            console.Log(type, message);
        }

        /// <summary>
        /// 断言设置开关，raise为true将中断后续代码执行
        /// </summary>
        public static bool RaiseAssert
        {
            get
            {
                return Assert.raiseExceptions;
            }
            set
            {
                Assert.raiseExceptions = value;
            }
        }

        /// <summary>
        /// 检测是否打开了UNITY_ASSERTIONS宏并打印提示
        /// </summary>
        static void CheckAssert()
        {
            if (checkAssertions && !assertionsOpened)
                Warning("Plase set the symbol 'UNITY_ASSERTIONS' for script compilation");
        }

        /// <summary>
        /// 如果obj不为空触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void IsNullObject(Object obj, string message)
        {
            CheckAssert();
            Assert.IsNull(obj, message);
        }

        /// <summary>
        /// 如果value不为空触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void IsNull<T>(T value, string message) where T : class
        {
            CheckAssert();
            Assert.IsNull(value, message);
        }

        /// <summary>
        /// 如果obj为空触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void IsNotNullObject(Object obj, string message)
        {
            CheckAssert();
            Assert.IsNotNull(obj, message);
        }

        /// <summary>
        /// 如果value为空触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void IsNotNull<T>(T value, string message) where T : class
        {
            CheckAssert();
            Assert.IsNotNull(value, message);
        }

        /// <summary>
        /// 如果condition为false触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void IsTrue(bool condition, string message)
        {
            CheckAssert();
            Assert.IsTrue(condition, message);
        }

        /// <summary>
        /// 如果condition为true触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void IsFalse(bool condition, string message)
        {
            CheckAssert();
            Assert.IsFalse(condition, message);
        }

        /// <summary>
        /// 如果expected与actual对象不相等触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreEqualObject(Object expected, Object actual, string message)
        {
            CheckAssert();
            Assert.AreEqual(expected, actual, message);
        }

        /// <summary>
        /// 如果expected与actual不相等触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreEqual<T>(T expected, T actual, string message)
        {
            CheckAssert();
            Assert.AreEqual(expected, actual, message);
        }

        /// <summary>
        /// 自定义比较器，如果expected与actual不相等触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreEqualComparer<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            CheckAssert();
            Assert.AreEqual(expected, actual, message, comparer);
        }

        /// <summary>
        /// 如果expected与actual对象相等触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreNotEqualObject(Object expected, Object actual, string message)
        {
            CheckAssert();
            Assert.AreNotEqual(expected, actual, message);
        }

        /// <summary>
        /// 如果expected与actual相等触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreNotEqual<T>(T expected, T actual, string message)
        {
            CheckAssert();
            Assert.AreNotEqual(expected, actual, message);
        }

        /// <summary>
        /// 自定义比较器，如果expected与actual相等触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreNotEqualComparer<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            CheckAssert();
            Assert.AreNotEqual(expected, actual, message, comparer);
        }

        /// <summary>
        /// 如果expected与actual误差大等于0.00001f触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreApproximatelyEqual(float expected, float actual, string message)
        {
            CheckAssert();
            Assert.AreApproximatelyEqual(expected, actual, message);
        }

        /// <summary>
        /// 如果expected与actual误差大等于tolerance触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            CheckAssert();
            Assert.AreApproximatelyEqual(expected, actual, tolerance, message);
        }

        /// <summary>
        /// 如果expected与actual误差小等于0.00001f触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreNotApproximatelyEqual(float expected, float actual, string message)
        {
            CheckAssert();
            Assert.AreNotApproximatelyEqual(expected, actual, message);
        }

        /// <summary>
        /// 如果expected与actual误差小等于tolerance触发断言
        /// 编译时需要加入宏UNITY_ASSERTIONS
        /// </summary>
        public static void AreNotApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            CheckAssert();
            Assert.AreNotApproximatelyEqual(expected, actual, tolerance, message);
        }

        /// <summary>
        /// 导出Int值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int IntValue(LogType type)
        {
            return (int)type;
        }

        /// <summary>
        /// 打印特殊标识的日志信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void Log(int type, object message)
        {
            if (IsLogOpened(type))
                console.Log(Type.Log, message);
        }

        public static void LogTag(int type, object message)
        {
            if (IsLogOpened(type))
                console.Log(Type.Log, customColorTag[type] + message);
        }

        /// <summary>
        /// 打印特殊标识的日志信息及消息对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void Log(int type, object message, Object context)
        {
            if (IsLogOpened(type))
                console.Log(Type.Log, message, context);
        }

        public static void LogTag(int type, object message, Object context)
        {
            if (IsLogOpened(type))
                console.Log(Type.Log, customColorTag[type] + message, context);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void LogFormat(int type, string message, object arg1)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, message, arg1);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void LogFormat(int type, string message, object arg1, object arg2)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, message, arg1, arg2);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void LogFormat(int type, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void LogFormat(int type, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogFormat(int type, string message, params object[] args)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, message, args);
        }

        public static void LogTagFormat(int type, string message, object arg1)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, customColorTag[type] + message, arg1);
        }

        public static void LogTagFormat(int type, string message, object arg1, object arg2)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, customColorTag[type] + message, arg1, arg2);
        }

        public static void LogTagFormat(int type, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, customColorTag[type] + message, arg1, arg2, arg3);
        }

        public static void LogTagFormat(int type, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, customColorTag[type] + message, arg1, arg2, arg3, arg4);
        }

        public static void LogTagFormat(int type, string message, params object[] args)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, customColorTag[type] + message, args);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息及消息对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void LogFormat(int type, Object context, string message, object arg1)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, message, arg1);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息及消息对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void LogFormat(int type, Object context, string message, object arg1, object arg2)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, message, arg1, arg2);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息及消息对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void LogFormat(int type, Object context, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, message, arg1, arg2, arg3);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息及消息对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void LogFormat(int type, Object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, message, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 打印特殊标识的格式化日志信息及消息对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogFormat(int type, Object context, string message, params object[] args)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, message, args);
        }

        public static void LogTagFormat(int type, Object context, string message, object arg1)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, customColorTag[type] + message, arg1);
        }

        public static void LogTagFormat(int type, Object context, string message, object arg1, object arg2)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, customColorTag[type] + message, arg1, arg2);
        }

        public static void LogTagFormat(int type, Object context, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, customColorTag[type] + message, arg1, arg2, arg3);
        }

        public static void LogTagFormat(int type, Object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, customColorTag[type] + message, arg1, arg2, arg3, arg4);
        }

        public static void LogTagFormat(int type, Object context, string message, params object[] args)
        {
            if (IsLogOpened(type))
                console.LogFormat(Type.Log, context, customColorTag[type] + message, args);
        }

        /// <summary>
        /// 打印特殊标识的带tag日志信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="tag"></param>
        public static void TagInfo(int type, object message, string tag)
        {
            if (IsLogOpened(type))
                console.LogTag(Type.Log, message, tag);
        }

        /// <summary>
        /// 打印特殊标识的带tag日志信息及消息对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <param name="tag"></param>
        public static void TagInfo(int type, object message, Object context, string tag)
        {
            if (IsLogOpened(type))
                console.LogTag(Type.Log, context, message, tag);
        }

        public static void ErrorTag(int type, object message)
        {
            if (IsLogOpened(LogTypeError))
                console.Log(Type.Error, customColorTag[type] + message);
        }

        public static void ErrorTag(int type, object message, Object context)
        {
            if (IsLogOpened(LogTypeError))
                console.Log(Type.Error, customColorTag[type] + message, context);
        }

        public static void ErrorTagFormat(int type, string message, object arg1)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, customColorTag[type] + message, arg1);
        }

        public static void ErrorTagFormat(int type, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, customColorTag[type] + message, arg1, arg2);
        }

        public static void ErrorTagFormat(int type, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, customColorTag[type] + message, arg1, arg2, arg3);
        }

        public static void ErrorTagFormat(int type, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, customColorTag[type] + message, arg1, arg2, arg3, arg4);
        }

        public static void ErrorTagFormat(int type, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, customColorTag[type] + message, args);
        }

        public static void ErrorTagFormat(int type, Object context, string message, object arg1)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, customColorTag[type] + message, arg1);
        }

        public static void ErrorTagFormat(int type, Object context, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, customColorTag[type] + message, arg1, arg2);
        }

        public static void ErrorTagFormat(int type, Object context, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, customColorTag[type] + message, arg1, arg2, arg3);
        }

        public static void ErrorTagFormat(int type, Object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, customColorTag[type] + message, arg1, arg2, arg3, arg4);
        }

        public static void ErrorTagFormat(int type, Object context, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeError))
                console.LogFormat(Type.Error, context, customColorTag[type] + message, args);
        }

        public static void WarningTag(int type, object message)
        {
            if (IsLogOpened(LogTypeWarning))
                console.Log(Type.Warning, customColorTag[type] + message);
        }

        public static void WarningTag(int type, object message, Object context)
        {
            if (IsLogOpened(LogTypeWarning))
                console.Log(Type.Warning, customColorTag[type] + message, context);
        }

        public static void WarningTagFormat(int type, string message, object arg1)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, customColorTag[type] + message, arg1);
        }

        public static void WarningTagFormat(int type, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, customColorTag[type] + message, arg1, arg2);
        }

        public static void WarningTagFormat(int type, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, customColorTag[type] + message, arg1, arg2, arg3);
        }

        public static void WarningTagFormat(int type, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, customColorTag[type] + message, arg1, arg2, arg3, arg4);
        }

        public static void WarningTagFormat(int type, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, customColorTag[type] + message, args);
        }

        public static void WarningTagFormat(int type, Object context, string message, object arg1)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, customColorTag[type] + message, arg1);
        }

        public static void WarningTagFormat(int type, Object context, string message, object arg1, object arg2)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, customColorTag[type] + message, arg1, arg2);
        }

        public static void WarningTagFormat(int type, Object context, string message, object arg1, object arg2, object arg3)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, customColorTag[type] + message, arg1, arg2, arg3);
        }

        public static void WarningTagFormat(int type, Object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, customColorTag[type] + message, arg1, arg2, arg3, arg4);
        }

        public static void WarningTagFormat(int type, Object context, string message, params object[] args)
        {
            if (IsLogOpened(LogTypeWarning))
                console.LogFormat(Type.Warning, context, customColorTag[type] + message, args);
        }

        #region 输出Log至文件
        private static bool outputLogToFile = false;

        public static bool OutputLogToFile
        {
            get
            {
                return outputLogToFile;
            }
            set
            {
                outputLogToFile = value;
                AddLogReceived();
            }
        }

        private static bool outputStackToFile = false;
        public static bool OutputStackToFile
        {
            get
            {
                return outputStackToFile;
            }
            set
            {
                if (value)
                    OutputLogToFile = value;
                outputStackToFile = value;
            }
        }

        private static bool singleLogFile = true;
        public static bool SingleLogFile
        {
            get
            {
                return singleLogFile;
            }
            set
            {
                singleLogFile = value;
                UpdateFilePath();
            }
        }

        const string logFormat = "{0} [{1}] {2}";

        const string timeFormat = "HH:mm:ss,fff";

        const string fileFormat = "game{0:yyyyMMdd.HHmmss}.log";

        private static readonly char[] logTypeString =
        {
            'E', 'A', 'W', 'I', 'X'
        };

        static string outputFilePath;

        static void HandleLog(string message, string stackTrace, LogType type)
        {
            if (outputFilePath != null)
            {
                using (StreamWriter writer = new StreamWriter(outputFilePath, true, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(logFormat, DateTime.Now.ToString(timeFormat), logTypeString[(int)type], message);
                    if (outputStackToFile)
                        writer.WriteLine(stackTrace);
                }
            }
        }

        static void UpdateFilePath()
        {
            string path = Application.persistentDataPath + "/logs/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (singleLogFile)
            {
                path += "game.log";
                if (outputFilePath != path)
                {
                    //备份记录文件，只留一个
                    if (File.Exists(path))
                    {
                        if (File.Exists(path + ".bak"))
                            File.Delete(path + ".bak");
                        File.Move(path, path + ".bak");
                    }
                }
            }
            else
            {
                path += string.Format(fileFormat, DateTime.Now);
            }
            outputFilePath = path;
        }

        static void AddLogReceived()
        {
            if (outputFilePath == null)
                UpdateFilePath();

            Application.logMessageReceivedThreaded -= HandleLog;
            if (outputLogToFile)
                Application.logMessageReceivedThreaded += HandleLog;
        }

        #endregion
    }
}
