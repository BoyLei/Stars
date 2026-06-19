using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class UpLoadBuglySo
{

    //要解压缩
    private static string soCmdTemplate = "java -jar D://pppppp//buglyqq-upload-symbol.jar -appid #ID# -appkey #KEY# -bundleid #PACKAGE# -version #VERSION# -platform Android -inputSymbol #SOPATH#";

    /*
    java -jar D:\StarsProject_Client\GamePack\AndroidPack\buglyqq-upload-symbol.jar -appid 5ff6561361 -appkey e46326c6-c338-4176-89f3-236665524eae -bundleid com.Yoka.Stars -version 100.0.3.5 -platform Android -inputSymbol D:\StarsProject_Client\GamePack\AndroidPack\202403141516-100.0.3.5-v1.symbols\java -jar buglyqq-upload-symbol.jar -appid 5ff6561361 -appkey e46326c6-c338-4176-89f3-236665524eae -bundleid com.Yoka.Stars -version 100.0.3.5 -platform Android -inputSymbol D:\StarsProject_Client\GamePack\AndroidPack\202403141516-100.0.3.5-v1.symbols\
    */

    /// <summary>
    /// 打完包后调用此方法 自动上传符号表文件
    /// </summary>
    [MenuItem("Build/自动上传符号表")]
    public static void UploadBuglyso()
    {
        var datatime = ""; //"202411181654";
        var buildpath = ""; //"F:\\Proj\\StarsProject_Client\\GamePack\\AndroidPack\\TrunkPack";
        EditAndRunCommand(datatime, buildpath);
    }

    public static void EditAndRunCommand(string datatime = null,string buildpath = null)
    {       
        //1，还差自动解压
        //2，名字填充BuildPack
        //3，自动上传
        StringBuilder sb = new StringBuilder();
        sb.Append(soCmdTemplate);
        //4，目前这里每次都要手动改一下，并且包要发给运维。！！！！！！！！！！！！！！！！！！！！
        sb.Replace("#SOPATH#", "D://pppppp//202411170116-0.3.5.204-v200.symbols//");
        sb.Replace("#ID#", "5ff6561361");
        sb.Replace("#KEY#", "e46326c6-c338-4176-89f3-236665524eae");
        sb.Replace("#PACKAGE#", "com.jrtt.stars"/*PlayerSettings.applicationIdentifier*/);
        sb.Replace("#VERSION#", "0.3.5.204"/*PlayerSettings.bundleVersion*/);//version   bugly:0.3.5.138 bug      后台：0.3.5.153   



        ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/k " + sb.ToString());
        processInfo.CreateNoWindow = false;
        processInfo.UseShellExecute = true;

        Process process = Process.Start(processInfo);
        Console.Write(" ");
        process.WaitForExit();
        process.Close();
        Debug.LogError("Upload Finished");
    }


}