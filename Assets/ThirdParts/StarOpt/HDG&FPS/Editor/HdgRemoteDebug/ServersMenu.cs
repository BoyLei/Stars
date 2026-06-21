using GameDLL;
using GameDLL.Hdg;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Hdg
{
	internal class ServersMenu
	{
		private List<rdtServerAddress> m_servers;

		private Action<rdtServerAddress> m_onSelected;

		private ServerAddressWindow m_popup;

		private Rect m_buttonRect;

		private EditorWindow m_owner;

		private LinkedList<rdtServerAddress> m_customServers;

		private const int SERVER_MRU_COUNT = 5;

		public List<rdtServerAddress> Servers
		{
			set
			{
				m_servers = value;
				m_servers.Sort();
			}
		}

		public ServersMenu(Action<rdtServerAddress> onSelected, EditorWindow owner)
		{
			m_customServers = new LinkedList<rdtServerAddress>();
			for (int i = 4; i >= 0; i--)
			{
				string key = "Hdg.RemoteDebug.RecentServer" + i;
				if (EditorPrefs.HasKey(key))
				{
					string address = EditorPrefs.GetString(key);
					AddServer(address);
				}
			}
			m_servers = new List<rdtServerAddress>();
			m_onSelected = onSelected;
			m_owner = owner;
		}

		public void Show(rdtServerAddress currentServer)
		{
			GUIContent content = new GUIContent("Active Player");
			Rect rect = GUILayoutUtility.GetRect(content, EditorStyles.toolbarDropDown);
			GUI.Label(rect, content, EditorStyles.toolbarDropDown);
			Event evt = Event.current;
			if (evt.type == EventType.Repaint)
			{
				Rect pos = m_owner.position;
				m_buttonRect = GUILayoutUtility.GetLastRect();
				m_buttonRect.x = m_buttonRect.x + pos.x;
				m_buttonRect.y = m_buttonRect.y + pos.y;
			}
			if (evt.isMouse && evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
			{
				evt.Use();
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("None"), currentServer == null, new GenericMenu.MenuFunction2(OnContextMenu), null);
				AddServers(m_servers, menu, currentServer);
				AddServers(m_customServers, menu, currentServer);
				menu.AddItem(new GUIContent("<Enter IP>"), false, delegate()
				{
					OnEnterIP(m_buttonRect);
				});
				menu.DropDown(rect);
			}
		}

		public void Destroy()
		{
			if (m_popup != null)
				m_popup.Close();
			m_popup = null;
		}

		private void AddServers(IEnumerable<rdtServerAddress> servers, GenericMenu menu, rdtServerAddress currentServer)
		{
			foreach (rdtServerAddress server in servers)
			{
				menu.AddItem(new GUIContent(server.ToString()), server.Equals(currentServer), new GenericMenu.MenuFunction2(OnContextMenu), server);
			}
		}

		private void OnContextMenu(object userdata)
		{
			m_onSelected(userdata as rdtServerAddress);
		}

		private void OnEnterIP(Rect rect)
		{
			if (m_popup != null)
				return;
			m_popup = ScriptableObject.CreateInstance<ServerAddressWindow>();
			m_popup.Callback = new Action<string>(OnWindowDismissed);
			m_popup.ShowAsDropDown(rect, new Vector2(344f, 54f));
		}

		private void OnWindowDismissed(string server)
		{
			m_popup = null;
			rdtServerAddress serverAddress = AddServer(server);
			if (serverAddress == null)
				return;
			int count = m_customServers.Count;
			if (count > SERVER_MRU_COUNT)
			{
				int extra = count - SERVER_MRU_COUNT;
				for (int i = 0; i < extra; i++)
					m_customServers.RemoveLast();
			}
			int index = 0;
			foreach (rdtServerAddress s in m_customServers)
				EditorPrefs.SetString("Hdg.RemoteDebug.RecentServer" + index++, s.IPAddress.ToString() + ":" + s.m_port);
			m_onSelected(serverAddress);
		}

		private rdtServerAddress AddServer(string server)
		{
			if (string.IsNullOrEmpty(server))
				return null;
			int portIndex = server.IndexOf(':');
			string ipString = (portIndex != -1) ? server.Substring(0, portIndex) : server;
			int port = Settings.DEFAULT_SERVER_PORT;
			if (portIndex != -1)
			{
				string portString = server.Substring(portIndex + 1);
				if (!string.IsNullOrEmpty(portString) && !int.TryParse(portString, out port))
				{
					Debug.LogErrorFormat("Invalid IP address '{0}'", server);
					return null;
				}
			}
			IPAddress address = null;
			if (!IPAddress.TryParse(ipString, out address))
			{
				Debug.LogErrorFormat("Invalid IP address '{0}'", server);
				return null;
			}
			rdtServerAddress serverAddress = new rdtServerAddress(address, port, server, "Custom", "Custom", "Unknown");
			m_customServers.AddFirst(serverAddress);
			return serverAddress;
		}
	}
}
