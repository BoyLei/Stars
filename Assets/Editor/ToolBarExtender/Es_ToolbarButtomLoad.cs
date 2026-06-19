using UnityToolbarExtender;
using UnityEditor;
using UnityEngine;

//基于Toolbar拓展插件制作 https://github.com/marijnz/unity-toolbar-extender
[InitializeOnLoad]
public class Es_ToolbarLoad
{
    static Es_ToolbarLoad()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);
        ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
    }

    private static void OnLeftToolbarGUI()
    {
        GUILayout.FlexibleSpace();
        Es_ToolbarStartGame.OnToolbarGUI();
    }

    private static void OnRightToolbarGUI()
    {
        GUILayout.FlexibleSpace();
        Es_ToolbarSceneList.OnToolbarGUI();
        GUILayout.Space(20);
        Es_ToolbarGameQualityPreview.OnToolbarGUI();
        GUILayout.Space(20);
        Es_ToolbarOtherUtils.OnToolbarGUI();
        GUILayout.FlexibleSpace();
    }
}