using UnityEditor;
using UnityEngine;

namespace GameEditor.Hdg
{
	public static class rdtGuiLine
	{
		public static void DrawHorizontalSplitLine()
		{
            DrawLine(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.MaxHeight(1f), GUILayout.ExpandWidth(true)), EditorGUIUtility.isProSkin ? new Color(0.2784314f, 0.2784314f, 0.2784314f, 1f) : new Color(0.3647059f, 0.3647059f, 0.3647059f, 255f));
        }

        public static void DrawLine(Rect rect, Color color)
        {
            Color prevCol = GUI.color;
            GUI.color = Color.white;
            Color colour = color;
            EditorGUI.DrawRect(rect, colour);
            GUI.color = prevCol;

        }
    }
}
