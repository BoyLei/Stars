using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using SGF.Time;
using StarProject.Service.Time;
using StarProject;
using System;
using SGF.Network;

public class ShowFps : MonoBehaviour
{
    private float updateInterval = 1.0f;
    private float lastInterval; // Last interval end time
    private int frames = 0; // Frames over current interval
    public float fps = 0;
    private string lag;
    private string Describe = "早期版本,非最终品质";
    private GUIStyle gUIStyle;
    private GUIStyle gUIStyle2;
    private GUIStyle gUIStyle3;
    public static ShowFps Instance;

    public static string FPS
    {
        get
        {
            if (Instance != null)
            {
                return Instance.fps.ToString("f2");
            }
            else
            {
                return String.Empty;
            }
        }
    }
    private int ping = 0;
    private int waitPing = 0;
    private int waitPingCount = 0;
    private int preWeakCount = 0;
    private int totalWeakCount = 0;

    void Start()
    {
        Instance = this;
        //Application.targetFrameRate = 60;
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
        gUIStyle = new GUIStyle(); gUIStyle2 = new GUIStyle();
        gUIStyle3 = new GUIStyle();
        SocketBase battleSocket = NetworkManager.Instance.gameSocket;

    }

    void Update()
    {
        //#if STAR_DEV

        ++frames;
        var timeNow = Time.realtimeSinceStartup;
        lag = TimeUtils.LagMillTime + "ms";
        if (timeNow > lastInterval + updateInterval)
        {
            fps = frames / (timeNow - lastInterval);
            frames = 0;
            lastInterval = timeNow;
        }

        ping = TimeManager.Instance.AveragePing;
        SocketBase battleSocket = NetworkManager.Instance.gameSocket;

        waitPing = (int)battleSocket.CurPing.CurWaitTime;
        waitPingCount = battleSocket.CurPing.CurWaitWeakPingTimes;
        preWeakCount = battleSocket.CurPing.PreContinueWeakCount;
        totalWeakCount = battleSocket.CurPing.CurTotalWeakPingCount;
        //#endif

    }

    StringBuilder temp = new StringBuilder(128);
    StringBuilder temp1 = new StringBuilder(128);

    void OnGUI()
    {
#if STAR_DEV

        if (lastInterval != 0)
        {
            temp.Length = 0;
            temp.Append("FPS：").Append(fps.ToString("f2")).Append("w:" + Screen.width + "h:" + Screen.height);
            //temp.Append("___LAG：").Append(lag);
            temp.Append("__Ping").Append(ping);
            temp.Append("__TotalWeakCount").Append(totalWeakCount);
            temp.Append("__PreWeakCount").Append(preWeakCount);
            temp.Append("__WaitPingCount").Append(waitPingCount);
            temp.Append("__WaitPing").Append(waitPing);

            // temp.Append(SystemInfo.graphicsDeviceName);
            // temp.Append("GMemory:"+SystemInfo.graphicsMemorySize);
            // temp.Append('(').Append(Screen.width).Append('x').Append(Screen.height).Append(')');
            gUIStyle.fontSize = 20;
            gUIStyle.normal.background = null;
            gUIStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(200, Screen.height - 60, Screen.width, 20), temp.ToString(), gUIStyle);

            temp1.Clear();
            temp1.Append(TimeUtils.ServerNow);
            gUIStyle2.fontSize = 32;
            gUIStyle2.normal.background = null;
            gUIStyle2.normal.textColor = Color.green;
            GUI.Label(new Rect(200, Screen.height - 100, Screen.width, 40), temp1.ToString(), gUIStyle2);
            //temp1.Clear();
            //// temp1.Append($"C_{TimeUtils.ClientUtcNow.ToString("HH:mm:ss.fff")} ,S_{TimeUtils.ServerNow.ToString("HH:mm:ss.fff")} ,P_{TimeUtils.AveragePing}, SCB_{TimeUtils.SyncClientBaseTime}, SSB_{TimeManager.Instance.ServerSyncTimeStamp}");
            //temp1.Append($"C_{TimeUtils.ClientUtcNow.ToString("HH:mm:ss.fff")} ,S_{TimeUtils.ServerNow.ToString("HH:mm:ss.fff")} ,P_{TimeUtils.AveragePing}");
            //gUIStyle3.fontSize = 40;
            //gUIStyle3.normal.textColor = Color.red;
            //GUI.Label(new Rect(0, Screen.height - 80, Screen.width, 80), temp1.ToString(), gUIStyle3);
            //SGF.Debuger.Log(temp.ToString());

            //gUIStyle2.fontSize = 25;
            //gUIStyle2.normal.background = null;
            //gUIStyle2.normal.textColor = Color.green;
            //gUIStyle2.alignment = TextAnchor.LowerCenter;
            //GUI.Label(new Rect(0, 20, Screen.width, 20), Describe.ToString(), gUIStyle2);// SGF.Debuger.Log("---------------------------------------fps--------------------------");

        }
#endif

    }

}
