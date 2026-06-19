using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public static class LogCollect
{
    public static  string filePath = "";
    public static void Init()
    {
        //崩溃问题查询，本地问题查询时使用
        /*filePath = Application.persistentDataPath + "/UnityLog.txt";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        Application.logMessageReceivedThreaded += HandleLogMessage;*/
    }

    private static  void AppendToFile(string content)
    {
        using (StreamWriter writer = File.AppendText(filePath))
        {
            writer.WriteLine(content);
        }
    }
    
    private static void HandleLogMessage(string condition, string stacktrace, LogType type)
    {
        AppendToFile($"[{System.DateTime.Now.ToString("HH:mm:ss.fff")}] [{type}] {condition} {stacktrace}");
    }
}
