using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Diagnostics;
using System;
using System.Net.Http;
/*using Qiniu.Storage;*/
using System.Web;
using System;
using System.Net;
using System.Text;

public class BuildProcess : Editor
{
    static public List<string> FileNameList = new List<string>();

    [PostProcessBuild(1)]
    public static void AfterBuild(BuildTarget target, string pathtoBuildProject)
    {
    }
    static string assetPaht = Application.dataPath;

    public static void RunBat(string program, string parm)
    {
        try
        {
            Process proc = new Process();
            proc.StartInfo.FileName = program;
            proc.StartInfo.Arguments = string.Format(parm);//this is argument
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            proc.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
        }
    }


    static string buildPath = Directory.GetParent(assetPaht).FullName;

    private static void SvnUpdate()
    {
        RunBat("TortoiseProc", string.Format($"/command:update /path:{buildPath} /closeonend:3"));
        //svn commit
        // RunBat("TortoiseProc", string.Format($"/command:commit /path:{"更新的路径(绝对路径|相对路径都可)"} /closeonend:3"));
    }

    // [MenuItem("Build/Android")]
    public static List<string> BuildScene()
    {
        //环境设置
        List<string> scenes = new List<string>();
        foreach (var item in EditorBuildSettings.scenes)
        {
            if (item.enabled)
            {
                scenes.Add(item.path);
            }
        }

        return scenes;
    }


    public static void Build()
    {
#if UNITY_ANDROID
        BuildAndroid();
#elif UNITY_IOS
            BuildIOS();
#else
                BuildWin();
#endif
    }

    public static void BuildIOS() { }

    public static void BuildWin()
    {
        string path = Application.dataPath + "/../../../GamePack/AndroidPack/";
        UnityEngine.Debug.LogError(path);
    }

    private static void Commit(string Filepath)
    {
        RunBat("TortoiseProc", string.Format($"/command:add /path:{Filepath} /closeonend:3"));
        RunBat("TortoiseProc", string.Format($"/command:commit /path:{Filepath} -m\"【新增】：包体提交{Filepath}\"  /closeonend:3"));
    }

    private static Dictionary<string, string> CommadLines = new Dictionary<string, string>();
    private static void ReadCommadLineArgs()
    {
        CommadLines.Clear();
        List<string> lines = System.Environment.GetCommandLineArgs().KToList();
        if (lines != null && lines.Count > 0)
        {
            int index1 = lines.IndexOf("-datetime");
            if (index1 > -1 && index1 < lines.Count)
            {
                string datatime_value = string.Empty;
                if (index1 < lines.Count - 1)
                {
                    datatime_value = lines[index1 + 1];
                }
                CommadLines.Add("datatime", datatime_value);
                Log("datatime:"+datatime_value);
            }
        }
    }
    //  [MenuItem("Build/Android")]
    public static void BuildAndroid()
    {
        stringBuilder.Clear();
        ReadCommadLineArgs();
        Save();
        var scenes = BuildScene();
        PlayerSettings.productName = "Stars";
        PlayerSettings.companyName = "Yoka";
        /*PlayerSettings.bundleVersion = "1.0.0";*/
        PlayerSettings.keystorePass = "ykgame123";
        PlayerSettings.keyaliasPass = "ykgame123";
        PlayerSettings.Android.keyaliasName = "yoka";
        PlayerSettings.Android.keyaliasPass = "ykgame123";
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 1);

        string Filepath = Application.dataPath + "/../../../GamePack/AndroidPack/" + CommadLines["datatime"]+ ".apk";
        //打包
        BuildPipeline.BuildPlayer(new BuildPlayerOptions()
        {
            scenes = scenes.ToArray(),
            locationPathName = Filepath,
            target = BuildTarget.Android,
        });

      //  RunBat("TortoiseProc", string.Format($"/command:add /path:{Filepath} "));

      //  RunBat("TortoiseProc", string.Format($"/command:commit /path:{Filepath} -m'【新增】：包体提交{Filepath}' "));

    }

    public static StringBuilder stringBuilder = new StringBuilder();

    public static void Log(string message)
    {
        stringBuilder.Append(message);
        stringBuilder.Append("\n");
    }

    public static void Save()
    {
        string filePath = Application.dataPath + "/../BuildLog.txt";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        File.WriteAllText(filePath, stringBuilder.ToString());

    }
}
