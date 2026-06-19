#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public static class PlayModeChangeDetector
{
    static PlayModeChangeDetector()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        if (_DataPath == null)
        {
            string projectPath = Directory.GetCurrentDirectory();
            _DataPath = Path.Combine(projectPath, "Assets");
        }
    }

    private static bool _IsPlaying = false;
    public static bool IsPlaying => _IsPlaying;

    private static string _DataPath;
    /// <summary>
    /// DataPath = Application.dataPath
    /// </summary>
    public static string DataPath => _DataPath;

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredEditMode:
                // 退出播放模式时的逻辑代码
                _IsPlaying = false;
                //Debug.Log("Exited Play Mode");
                break;
            case PlayModeStateChange.ExitingEditMode:
                // 进入播放模式前的逻辑代码
                break;
            case PlayModeStateChange.EnteredPlayMode:
                // 进入播放模式时的逻辑代码
                _IsPlaying = true;
                //Debug.Log("Entered Play Mode");
                break;
            case PlayModeStateChange.ExitingPlayMode:
                // 退出播放模式前的逻辑代码
                break;
        }
    }
}
#endif
