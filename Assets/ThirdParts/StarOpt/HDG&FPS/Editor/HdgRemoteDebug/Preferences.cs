using System;
using UnityEditor;
using UnityEngine;
using GameDLL.Hdg;

namespace GameEditor.Hdg
{
	public static class Preferences
	{
#if UNITY_2018_3_OR_NEWER
		[SettingsProvider]
		static SettingsProvider PreferenceGUI()
		{
			return new SettingsProvider("Preferences/Remote Debug", SettingsScope.User)
			{
				guiHandler = searchContext => OnGUI()
			};
		}
#else
        [PreferenceItem("Remote Debug")]
        static void PreferenceGUI()
        {
            OnGUI();
        }
#endif

		static void OnGUI()
		{
			EditorGUILayout.Space();
			int broadcastPort = EditorPrefs.GetInt("Hdg.RemoteDebug.BroadcastPort", 12000);
			broadcastPort = EditorGUILayout.IntField("Server broadcast port", broadcastPort);
			if (GUI.changed)
			{
				EditorPrefs.SetInt("Hdg.RemoteDebug.BroadcastPort", broadcastPort);
				if (ConnectionWindow.Instance)
				{
					ConnectionWindow.Instance.RestartServerEnumerator();
				}
			}
			float gameObjectUpdateTime = EditorPrefs.GetFloat("Hdg.RemoteDebug.GameobjectUpdateTime", 1f);
			gameObjectUpdateTime = EditorGUILayout.FloatField("Gameobject Auto Refresh Time", gameObjectUpdateTime);
			if (GUI.changed)
			{
				EditorPrefs.SetFloat("Hdg.RemoteDebug.GameobjectUpdateTime", gameObjectUpdateTime);
                if (ConnectionWindow.Instance)
                {
                    ConnectionWindow.Instance.gameobjectUpdateTime = gameObjectUpdateTime;
                }
            }
			float componentUpdateTime = EditorPrefs.GetFloat("Hdg.RemoteDebug.ComponentUpdateTime", 0.1f);
			componentUpdateTime = EditorGUILayout.FloatField("Component Auto Refresh Time", componentUpdateTime);
			if (GUI.changed)
			{
				EditorPrefs.SetFloat("Hdg.RemoteDebug.ComponentUpdateTime", componentUpdateTime);
                if (ConnectionWindow.Instance)
                {
                    ConnectionWindow.Instance.componentUpdateTime = componentUpdateTime;
                }
            }
			bool debug = EditorPrefs.GetBool("Hdg.RemoteDebug.Debug", false);
			debug = EditorGUILayout.Toggle("Debug mode", debug);
			if (GUI.changed)
			{
				EditorPrefs.SetBool("Hdg.RemoteDebug.Debug", debug);
				rdtDebug.s_logLevel = (debug ? rdtDebug.LogLevel.Debug : rdtDebug.LogLevel.Info);
				if (ConnectionWindow.Instance)
					ConnectionWindow.Instance.debug = debug;
			}
		}
	}
}
