using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 任务编辑器配置
/// </summary>
public partial class TaskWindow
{

    #region static

    public static string settingsPath = "task.Settings";

    public static string taskTypeName = "TaskTypeList";

    public static TaskSettings taskSettings;


    public static void OpenSettings()
    {
        SettingsService.OpenUserPreferences("Preferences/任务编辑器");
    }

#if UNITY_2019_1_OR_NEWER
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        SettingsProvider provider = new SettingsProvider("Preferences/任务编辑器", SettingsScope.User)
        {
            guiHandler = (searchContext) => { TaskWindow.PreferencesGUI(); },
            keywords = new HashSet<string>(new[] { "task" })
        };
        return provider;
    }
#endif

#if !UNITY_2019_1_OR_NEWER
        [PreferenceItem("Node Editor")]
#endif
    private static void PreferencesGUI()
    {
        LoadPrefs();
        //JsonPathDir
        taskSettings.JsonPathDir = EditorGUILayout.TextField(new GUIContent("配置目录", "配置目录"), taskSettings.JsonPathDir);
        if (GUILayout.Button(new GUIContent("浏览..."), GUILayout.Width(120)))
        {
            var tmp = toPath(EditorUtility.SaveFolderPanel("选择配置目录", taskSettings.JsonPathDir, ""));
            if (tmp != "")
            {
                taskSettings.JsonPathDir = tmp;
                taskSettings.LogPath = tmp + "/TaskLog.txt";
            }
        }
        taskSettings.ClientPath = EditorGUILayout.TextField(new GUIContent("客户端任务数据", "客户端运行时的数据目录"), taskSettings.ClientPath);
        if (GUILayout.Button(new GUIContent("浏览..."), GUILayout.Width(120)))
        {
            var tmp = toPath(EditorUtility.SaveFolderPanel("选择客户端所在目录", taskSettings.ClientPath, ""));
            if (tmp != "")
            {
                taskSettings.ClientPath = tmp;
            }
        }
        taskSettings.ServerPath = EditorGUILayout.TextField(new GUIContent("服务器任务数据", "服务器运行时的数据目录"), taskSettings.ServerPath);
        if (GUILayout.Button(new GUIContent("浏览..."), GUILayout.Width(120)))
        {
            var tmp = toPath(EditorUtility.SaveFolderPanel("选择服务器所在目录", taskSettings.ServerPath, ""));
            if (tmp != "")
            {
                taskSettings.ServerPath = tmp;
            }
        }
        taskSettings.DesignPath = EditorGUILayout.TextField(new GUIContent("策划任务数据", "策划数据目录"), taskSettings.DesignPath);
        if (GUILayout.Button(new GUIContent("浏览..."), GUILayout.Width(120)))
        {
            var tmp = toPath(EditorUtility.SaveFolderPanel("选择策划所在目录", taskSettings.DesignPath, ""));
            if (tmp != "")
            {
                taskSettings.DesignPath = tmp;
            }
        }

        if (GUI.changed) EditorPrefs.SetString(settingsPath, JsonUtility.ToJson(taskSettings));
    }
    private static TaskSettings LoadPrefs()
    {
        // Create settings if it doesn't exist      
        if (!EditorPrefs.HasKey(settingsPath))
        {
            EditorPrefs.SetString(settingsPath, JsonUtility.ToJson(new TaskSettings()));
        }
        taskSettings = JsonUtility.FromJson<TaskSettings>(EditorPrefs.GetString(settingsPath));
        return taskSettings;
    }

    private static string toPath(string path)
    {
        if (path == string.Empty)
        {
            return string.Empty;
        }
        string a = Path.GetDirectoryName(Application.dataPath);
        path = Path.GetFullPath(path);
        if (path.Contains(a))
        {
            return path.Replace(a, "").Replace("\\", "/").Remove(0, 1) + "/";
        }
        else
        {
            path = path.Replace("\\", "/");
            if (path.Length > 0 && path[path.Length - 1] == '/')
                return path;
            else
                return path + "/";
        }
    }
    #endregion
}

public class TaskSettings
{
    public string JsonPath = "Assets/DevTools/TaskEditor/Export/";
    public string JsonPathDir = "Assets/DevTools/TaskEditor/Export";
    public string LogPath = "Assets/DevTools/TaskEditor/Export/TaskLog.txt";
    public string ClientPath = "Assets/Res/Config/Task";
    public string ServerPath = "";
    public string DesignPath = "";
}
