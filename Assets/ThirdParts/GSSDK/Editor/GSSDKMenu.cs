using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using GSSDKEditor;

[InitializeOnLoad]
public class GSSDKMenu : ScriptableObject
{

    [MenuItem("GSSDK/查看GSSDK文档")]
    public static void OpenGSSDKWiki()
    {
        string url = "https://docs.dobest.cn/docs/sdk-service-docs/sdk-service-docs-2faj2p4um3apl";

        Application.OpenURL(url);
    }

    [MenuItem("GSSDK/编辑GSSDK参数")]
    public static void EditGSSDKParams()
    {
        SettingsService.OpenProjectSettings(GSSDKParams.SettingsWindowPath);
    }

    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }


    static string GetBuildPath()
    {
        string dirPath = Application.dataPath.Replace("/Assets", "") + "/iOSProject/";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        return dirPath;
    }
}
