using SGF.Unity;
using StarProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ConsoleLog : MonoBehaviour
{

    public Text txt;
    public List<string> cachedStrings;
    public int lineLimit = 10;

    private bool dirty = false;

    void OnEnable()
    {
        //A主线程
        //资源加载,资源下载,     B网络，多一个线程的Socket没问题
        //本地开服务器：开一个线程
        //UnityLog本身是线程C


        //线程截取需要的。手柄控制器-A，我的框架-A，
        //其他的：Lua，reporter，Logger


        //【B进程要打印，被手柄截取了】，并想通过C，处理打印在A中
        //这没问题
        //问题在于本类只做过，主进程逻辑A，日志C进程调用，回到主A，所以这里特殊设计了【文本】
        //但如果这截C取了 其他进程B，B是无法获得A中的文本并且处理文本的 ，B不知道A状态（A和C是一家，其他都不知道）
        //所以B这种特殊情况，想要处理所以A中的东西都可能出现问题
        //所以必须处理，所有开线程的地方通过Loom，所有拦截的地方（调用unityA主线程都通过Loom协调）
        if (AppConfig.IsDev())
        {
            Application.logMessageReceivedThreaded += HandleLog;
        }
   
    }

    void OnDisable()
    {
        if (AppConfig.IsDev())
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 用Loom的方法在Unity主线程中调用Text组件
        Loom.QueueOnMainThread((param) =>
        {
            cachedStrings.Add(logString);


            if (cachedStrings.Count > lineLimit)
            {
                cachedStrings.RemoveAt(0);
            }

            dirty = true;
            this.RefreshText();

        }, null);

    }

    void RefreshText()
    {

        if (!dirty)
        {
            return;
        }

        dirty = false;
        txt.text = "";

        for (int i = 0; i < cachedStrings.Count; i++)
        {
            txt.text += cachedStrings[i] + "\n";
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        // this.RefreshText();
    }
}
