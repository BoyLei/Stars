using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System;
using StarProjectDef;

[InitializeOnLoad]
public class Es_ToolbarGameQualityPreview
{
    public static void OnToolbarGUI()
    {
        //设置按钮底色
        var backgroundoldColor = GUI.backgroundColor;
        GUI.backgroundColor = Es_ToolbarStyle.EstoolbarGraphicSettingButtonBGColor;
        var rect = GUILayoutUtility.GetRect(new GUIContent("性能设置"), Es_ToolbarStyle.EstoolbarGraphicSettingButton);
        if (GUI.Button(rect, new GUIContent("性能设置"), Es_ToolbarStyle.EstoolbarGraphicSettingButton))
        {
            PopupWindow.Show(rect, new MachineQulatitySettingPopup());
        }
        GUI.backgroundColor = backgroundoldColor;
    }
}

public class MachineQulatitySettingPopup : PopupWindowContent
{
    static MachineQualityLevel editorCurrentLevel = MachineQualityLevel.TopLevel;
    static class Style
    {
        private static GUIStyle s_MenuItem;
        public static GUIStyle sMenuItem => s_MenuItem ?? (s_MenuItem = "MenuItem");
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(300, 120);
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Space(5);
        for (MachineQualityLevel level = MachineQualityLevel.TopLevel; level <= MachineQualityLevel.TopestLevel; ++level)
        {
            doQualitySetting(level);
        }
    }

    /// <summary>
    /// 意图，EDITOR下，可以修改表现，随意调整来调整渲染结果；
    /// 来源上，前置已经设置
    /// 后续上其他设置会刷新他
    /// 冲突上手机无法操作
    /// </summary>
    /// <param name="level"></param>
    private void doQualitySetting(MachineQualityLevel level)
    {
        EditorGUI.BeginChangeCheck();
        var levelDescription = GetEnumDescription(level);
        GUILayout.Toggle(editorCurrentLevel == level, new GUIContent(levelDescription), Style.sMenuItem);
        if (EditorGUI.EndChangeCheck())
        {
            editorCurrentLevel = level;
            if (level == MachineQualityLevel.LowestLevel)
            {
                AppMain.Instance.UseLowestLevelSetting();
            }
            else if (level == MachineQualityLevel.LowerLevel)
            {
                AppMain.Instance.UseLowLevelSetting();
            }
            else if (level == MachineQualityLevel.MiddleLevel)
            {
                AppMain.Instance.UseMiddleLevelSetting();
            }
            else if (level == MachineQualityLevel.TopLevel)
            {
                AppMain.Instance.UseHighLevelSetting();
            }
            else if (level == MachineQualityLevel.TopestLevel)
            {
                AppMain.Instance.UseHighHighHighLevelSetting();
            }
            GameConfig.MachineQualityLevel = editorCurrentLevel;//使其可以和设置相互设定（Editor下）,其他模块也生效
            

           GUIUtility.ExitGUI();
        }
    }

    public static string GetEnumDescription(Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());

        DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        if (attributes != null && attributes.Any())
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }
}