using UnityEditor;
using UnityEngine;

public class Es_ToolbarStyle
{
    public static GUIStyle EstoolbarStartGameButton
    {
        get
        {
            var style = new GUIStyle(EditorStyles.toolbarButton);
            if (EditorGUIUtility.isProSkin)
            {
                style.normal.textColor = new Color(187f/255f, 207f/255f, 156f/255f);
            }
            else
            {
                style.normal.textColor = new Color(73f/255f, 117f/255f, 69f/255f);
            }
            return style;
        }
    }
    public static GUIStyle EstoolbarGraphicSettingButton
    {
        get
        {
            var style = new GUIStyle(EditorStyles.toolbarButton);
            if (EditorGUIUtility.isProSkin)
            {
                style.normal.textColor = new Color(156f/255f, 190f/255f, 211f/255f);
            }
            else
            {
                style.normal.textColor = new Color(38f/255f, 79f/255f, 104f/255f);
            }
            return style;
        }
    }
    public static GUIStyle EstoolbarSceneListButton
    {
        get
        {
            var style = new GUIStyle(EditorStyles.toolbarButton);
            if (EditorGUIUtility.isProSkin)
            {
                style.normal.textColor = new Color(227f/255f, 181f/255f, 161f/255f);
            }
            else
            {
                style.normal.textColor = new Color(109f/255f, 73f/255f, 58f/255f);
            }
            return style;
        }
    }

    public static GUIStyle EstoolbarOtherUtilsButton
    {
        get
        {
            var style = new GUIStyle(EditorStyles.toolbarButton);
            if (EditorGUIUtility.isProSkin)
            {
                style.normal.textColor = new Color(86f / 255f, 70f / 255f, 211f / 255f);
            }
            else
            {
                style.normal.textColor = new Color(128f / 255f, 129f / 255f, 104f / 255f);
            }
            return style;
        }
    }




    private const float proSkinColorParameter = 60f / 255f;
    private const float defaultSkinColorParameter = 203f / 255f;
    public static Color EstoolbarStartGameButtonBGColor
    {
        get
        {
            if (EditorGUIUtility.isProSkin)
            {
                // Color startgameButtonBGColor = new Color(39f/255f, 144f/255f, 33f/255f)/proSkinColorParameter;
                Color startgameButtonBGColor = new Color(39f/255f, 114f/255f, 33f/255f)/proSkinColorParameter;
                return startgameButtonBGColor;
            }
            else
            {
                Color startgameButtonBGColor = new Color(153f/255f, 194f/255f, 113f/255f)/defaultSkinColorParameter;
                return startgameButtonBGColor;
            }
        }
    }
    public static Color EstoolbarGraphicSettingButtonBGColor
    {
        get
        {
            if (EditorGUIUtility.isProSkin)
            {
                Color graphicsettingButtonBGColor = new Color(33f/255f, 83f/255f, 114f/255f)/proSkinColorParameter;
                return graphicsettingButtonBGColor;
            }
            else
            {
                Color graphicsettingButtonBGColor = new Color(112f/255f, 181f/255f, 204f/255f)/defaultSkinColorParameter;
                return graphicsettingButtonBGColor;
            }
        }
    }
    public static Color EstoolbarSceneListButtonBGColor
    {
        get
        {
            if (EditorGUIUtility.isProSkin)
            {
                Color scenelistButtonBGColor = new Color(115f/255f, 58f/255f, 33f/255f)/proSkinColorParameter;
                return scenelistButtonBGColor;
            }
            else
            {
                Color scenelistButtonBGColor = new Color(221f/255f, 148f/255f, 113f/255f)/defaultSkinColorParameter;
                return scenelistButtonBGColor;
            }
        }
    }
}
