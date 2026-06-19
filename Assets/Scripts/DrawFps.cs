using UnityEngine;
using System.Collections;
using UnityEngine.Profiling;
using System.Text;

public class DrawFps : MonoBehaviour {

	private float updateInterval = 1.0f;
	private double lastInterval; // Last interval end time
	private int frames = 0; // Frames over current interval
	private float fps;
	private float ms;
    public bool IsShow = true;
    private static string customText = "";
    private static DrawFps mDrawFps = null;
    public static void SetShow(bool b)
    {
        if (mDrawFps != null)
        {
            mDrawFps.IsShow = b;
        }
    }
    void Awake()
    {
        mDrawFps = this;
    }
	// Use this for initialization
	void Start () {
	    lastInterval = Time.realtimeSinceStartup;
    	frames = 0;
	}

    //string memoryInfo = "";
    string fpsStr = "";
	// Update is called once per frame
	void Update () {
		
	    ++frames;
	    float timeNow = Time.realtimeSinceStartup;
	    if (timeNow > lastInterval + updateInterval)
	    {
			float f = float.Parse(lastInterval.ToString());
			fps = frames / (timeNow - f);
			ms = 1000.0f / Mathf.Max (fps, 0.00001f);
			
			frames = 0;
        	lastInterval = timeNow;
            if(IsShow)
            {
                //memoryInfo = string.Format("总: {0:F2}MB\n已用: {1:F2}MB\n空闲: {2:F2}MB\n总Mono堆: {3:F2}MB\n已用Mono堆: {4:F2}MB",
                //   UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f),
                //     UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f),
                //      UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / (1024f * 1024f),
                //       UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / (1024f * 1024f),
                //        UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / (1024f * 1024f)
                //);
                fpsStr = "FPS:" + fps.ToString("f0");
                //if (FOWSystem.Instance.setting != null && FOWSystem.Instance.setting.enableSystem)
                //{
                //    fpsStr += "\nFOW:" + FOWSystem.Instance.setting.elapsed.ToString("f0");
                //}
            }
            //memoryInfo.Append(fps.ToString("f2") + "FPS");
            // memoryInfo += Frame.StringX.Join("\n", Profiler.GetTotalAllocatedMemoryLong() * 0.000001, Profiler.GetTotalUnusedReservedMemoryLong() * 0.000001, Profiler.GetTotalReservedMemoryLong() * 0.000001, SystemInfo.systemMemorySize);
        }
	}
	
	void OnGUI()
	{
        if (IsShow)
        {
            //GUILayout.BeginVertical();//GUILayout.Width(300)
            GUI.color = Color.white;
            GUI.backgroundColor = Color.black;
            //  GUI.Box(new Rect(0, 1, 160, 80), memoryInfo);
            GUI.skin.box.fontSize = 20;
            GUI.Box(new Rect(0, 5, 90, 30), fpsStr);
            
            //GUILayout.Box(memoryInfo);
           // GUILayout.Box(fpsStr);
            //GUILayout.EndVertical();
        }
    }
    public static void SetCustomText(string aText)
    {
        customText = aText;
    }

    public static void AddCustomText(string aText)
    {
        customText += aText;
    }

    public static string GetCustomText()
    {
        return customText;
    }
}
