using UnityEditor;
using UnityEngine;

namespace GameEditor.Hdg
{
	public class rdtGuiSplit
	{
		private bool m_resize;

		private float m_resizeInitPos;

		private float m_minimumSize;

		private float m_separatorPosition = 150f;

		private float m_rightMargin;

		private EditorWindow m_parentWindow;

		private GUIStyle m_style;

		public rdtGuiSplit(float initPos, float rightMargin, EditorWindow parentWindow)
		{
			m_separatorPosition = initPos;
			m_minimumSize = initPos;
			m_rightMargin = rightMargin;
			m_parentWindow = parentWindow;
		}

		public float SeparatorPosition
		{
			get
			{
				return m_separatorPosition;
			}
		}

		public void Draw()
		{
			Event evt = Event.current;
			if (m_style == null)
			{
				m_style = new GUIStyle("EyeDropperVerticalLine");
			}
			EditorGUILayout.BeginVertical(GUILayout.Width(1f));
			GUILayout.Label("", m_style, GUILayout.MaxHeight(m_parentWindow.position.height));
			EditorGUILayout.EndVertical();
			Rect r = GUILayoutUtility.GetLastRect();
			r.width += 4f;
			EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeHorizontal);
			switch(evt.type)
            {
				case EventType.MouseDown:
                case EventType.MouseDrag:
                    if (r.Contains(evt.mousePosition))
					{
						m_resizeInitPos = evt.mousePosition.x;
						m_resize = true;
						evt.Use();
					}
					break;
				case EventType.MouseUp:
					if (m_resize)
					{
						m_resize = false;
						evt.Use();
					}
					break;
			}
			//if (evt.type == EventType.MouseDown && r.Contains(evt.mousePosition))
			//{
			//	m_resizeInitPos = evt.mousePosition.x;
			//	m_resize = true;
			//	evt.Use();
			//}
			//else if (m_resize && (evt.type == EventType.MouseDrag || evt.rawType == EventType.MouseUp))
			//{
			//	m_resize = false;
			//	evt.Use();
			//}
			if (m_resize)
			{
				float deltax = evt.mousePosition.x - m_resizeInitPos;
				float parentWidth = m_parentWindow.position.width;
				if (parentWidth > m_rightMargin)
					parentWidth -= m_rightMargin;
                m_separatorPosition = Mathf.Clamp(m_separatorPosition + deltax, m_minimumSize, parentWidth);
                m_resizeInitPos = Mathf.Clamp(evt.mousePosition.x, m_minimumSize, parentWidth);
				m_parentWindow.Repaint();
			}
		}
	}
}
