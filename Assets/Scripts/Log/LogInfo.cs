using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LogModule
{
    public class LogInfo
    {
        static string logFormat = "{0} [{1}] {2} {3}";
        public string LogString;
        public string StackTrace;

        public LogType LogInfoType;

        // 是否已读
        public bool Readed;

        public void Update(string logString, string stackTrace, LogType type)
        {
            LogString = logString;
            StackTrace = stackTrace;
            LogInfoType = type;
            Readed = false;
        }

        public bool Equals(string str)
        {
            return LogString == str;
        }

        public override string ToString()
        {
            if (LogInfoType != LogType.Log)
            {
                return string.Format(logFormat, LogInfoType, LogReport.GetLogKey(), LogString, StackTrace);
            }
            else
            {
                return string.Format(logFormat, LogInfoType, LogReport.GetLogKey(), LogString, "");
            }
        }

        public void Read()
        {
            Readed = true;
        }
    }


    public class LogViewInfo
    {
        static string logFormat = "{0} [{1}] {2} {3}";
        public LogType LogInfoType;

        public string LogString;


        public void Update(LogInfo logInfo)
        {
            LogInfoType = logInfo.LogInfoType;

            if (LogInfoType != LogType.Log)
            {
                LogString = string.Format(logFormat, LogInfoType, LogReport.GetLogKey(), logInfo.LogString, logInfo.StackTrace);
            }
            else
            {
                LogString = string.Format(logFormat, LogInfoType, LogReport.GetLogKey(), logInfo.LogString, "");
            }
        }
    }
}
