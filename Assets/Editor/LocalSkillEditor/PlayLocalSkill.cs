using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlayLocalSkill
{
    [MenuItem("Tools/离线/PlayLocal")]
    public static void Play()
    {

        Application.quitting -= OnQuit;
        Application.quitting += OnQuit;

        if (!EditorApplication.isPlaying)
        {
            EditorPrefs.SetBool("LocalPlay", true);

            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scene/DevScene/Init.unity");

            EditorApplication.isPlaying = true;
        }

    }
    [MenuItem("Tools/离线/Clean")]
    public static void Clean()
    {
        EditorPrefs.SetBool("LocalPlay", false);
    }

    public static void OnQuit()
    {
        Application.quitting -= OnQuit;
        EditorPrefs.SetBool("LocalPlay", false);
    }

}
