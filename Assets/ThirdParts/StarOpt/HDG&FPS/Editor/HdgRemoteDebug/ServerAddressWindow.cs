using System;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Hdg
{
	public class ServerAddressWindow : EditorWindow
	{
		private string m_address;

		public Action<string> Callback { get; set; }

		private void OnGUI()
		{
			Color32 border = EditorGUIUtility.isProSkin ? new Color32(99, 99, 99, byte.MaxValue) : new Color32(130, 130, 130, byte.MaxValue);
			EditorGUI.DrawRect(new Rect(0f, 0f, maxSize.x, maxSize.y), border);
			Color32 fill = EditorGUIUtility.isProSkin ? new Color32(49, 49, 49, byte.MaxValue) : new Color32(193, 193, 193, byte.MaxValue);
			EditorGUI.DrawRect(new Rect(1f, 1f, maxSize.x - 2f, maxSize.y - 2f), fill);
			GUILayout.Space(8f);
			m_address = EditorGUILayout.TextField("Server IP:port", m_address, new GUILayoutOption[0]);
			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Cancel", GUILayout.Width(100f)))
				Close(false);
			if (GUILayout.Button("Connect", GUILayout.Width(100f)))
				Close(true);
			EditorGUILayout.EndHorizontal();
		}

		private void Close(bool connect)
		{
			if (Callback != null && connect)
				Callback(m_address);
			Close();
		}
	}
}
