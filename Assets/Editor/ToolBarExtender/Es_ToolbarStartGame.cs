using System.Collections.Generic;
using UnityToolbarExtender;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

//基于Toolbar拓展插件制作 https://github.com/marijnz/unity-toolbar-extender
[InitializeOnLoad]
public class Es_ToolbarStartGame
{
    public static void OnToolbarGUI()
    {
        //设置按钮底色
        var backgroundoldColor = GUI.backgroundColor;
        GUI.backgroundColor = Es_ToolbarStyle.EstoolbarStartGameButtonBGColor;

        var rect = GUILayoutUtility.GetRect(new GUIContent("开始游戏"), Es_ToolbarStyle.EstoolbarStartGameButton);
        if (GUI.Button(rect, new GUIContent("开始游戏"), Es_ToolbarStyle.EstoolbarStartGameButton))
        {
            if (Application.isPlaying)
                return;
            if (EditorSceneManager.GetActiveScene().name == "Startup")
            {
                EditorApplication.isPlaying = true;
            }
            else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                //ChangeGameViewResolution(3);
                EditorSceneManager.OpenScene("Assets/Res/Map/Prepare.unity");
                EditorApplication.isPlaying = true;
            }
        }

        GUI.backgroundColor = backgroundoldColor;
    }

    public static void ChangeGameViewResolution(int sizeIndex)
    {
        var assembly = typeof(Editor).Assembly;
        var gameView = assembly.GetType("UnityEditor.GameView");
        var instance = EditorWindow.GetWindow(gameView);
        gameView.GetMethod("SizeSelectionCallback")!.Invoke(instance, new object[] { sizeIndex, null });
    }

}
