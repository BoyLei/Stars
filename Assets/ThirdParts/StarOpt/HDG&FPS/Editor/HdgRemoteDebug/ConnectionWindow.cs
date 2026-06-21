using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
//using GameExtensions;
using UnityEngine;
using GameDLL.Hdg;
using GUI = UnityEngine.GUI;

namespace GameEditor.Hdg
{
	public partial class ConnectionWindow : EditorWindow
	{
		private bool m_debug;
		public bool debug
        {
			get
            {
				return m_debug;
            }
            set
            {
				m_debug = value;
            }
        }

		private bool m_showInstanceID;

		private bool m_automaticRefresh = true;

		private bool m_forceRepaint = true;

		private bool m_isPlaying;

		[NonSerialized]
		private bool m_waitingForPlayingChanged;

		private rdtClient m_client;

		private double m_lastTime;

		private rdtServerAddress m_currentServer;

		private rdtGuiSplit m_split;

		private bool m_updatingTree;

		private int m_pageType;

		internal const int PAGE_TYPE_HIERARCHY = 0;
		internal const int PAGE_TYPE_FILES = 1;

		private rdtExpandedCache m_expandedCache = new rdtExpandedCache();

		private rdtOperationCallback m_operationCallback = new rdtOperationCallback();

		private const float WIDE_MODE_SIZE_THRESHOLD = 330f;

		private const float LABEL_ADJUST_SIZE_THRESHOLD = 350f;

		private GUIContent m_automaticRefreshOnContent;
		private GUIContent m_automaticRefreshOffContent;

		private GUIContent m_refreshContent;

		private GUIContent m_settingContent;

		private GUIContent m_hardReloadIconContent;

		private GUIContent m_closeIconContent;

		private GUIContent m_showInstanceContent;
		private GUIContent m_hideInstanceContent;

		private GUIContent[] m_pageContents;

		private GUIContent m_hierarchyIconContent;

		private GUIContent m_filesIconContent;

		private GUIStyle m_normalFoldoutStyle;

		private GUIStyle m_toggleStyle;

		private GUIStyle m_iconButtonStyle;

		private bool m_isProSkin;

		private string m_filter;

		public string Filter
        {
			get
            {
				return m_filter;
			}
            set
			{
				if (m_filter != value)
				{
					m_filter = value;
					switch (m_pageType)
                    {
						case PAGE_TYPE_HIERARCHY:
							m_hierarchyTree.Filter = value;
							break;
						case PAGE_TYPE_FILES:
							m_filesTree.Filter = value;
							break;
					}
				}
			}
        }

		private rdtClientEnumerateServers m_serverEnum;

		private ServersMenu m_serversMenu;

		[NonSerialized]
		private bool m_clearFocus;

		private static ConnectionWindow s_instance;

		public static ConnectionWindow Instance
		{
			get
			{
				return s_instance;
			}
		}

		[MenuItem("Tools/Console/Hdg Remote Debug")]
		public static void ShowWindow()
		{
			GetWindow<ConnectionWindow>(false, "Remote Debug", true);
		}

		public ConnectionWindow()
		{
			minSize = new Vector2(400f, 100f);
		}

		public void RestartServerEnumerator()
		{
			if (m_serverEnum != null)
			{
				rdtDebug.Debug("Stopping server enumerator");
				m_serverEnum.Stop();
			}
			m_serverEnum = new rdtClientEnumerateServers();
		}

		public void Connect(rdtServerAddress address)
		{
			Disconnect(true);
			m_expandedCache.Clear();
			m_currentServer = address;
			m_client = new rdtClient();
			m_client.Connect(address.IPAddress, address.m_port);
			m_client.AddCallback(typeof(rdtTcpMessageReturnResult), OnMessageOperationReturn);
			ConnectHierarchy();
			ConnectFiles();
		}

		private void Disconnect(bool resetServer = true)
		{
			m_pageType = PAGE_TYPE_HIERARCHY;
			if (resetServer)
				m_currentServer = null;
			if (m_client != null)
				m_client.Stop();
			m_waitingForPlayingChanged = false;
			DisconnectHierarchy();
			DisconnectFiles();
		}

		private void OnEnable()
		{
			m_debug = EditorPrefs.GetBool("Hdg.RemoteDebug.Debug", false);
			rdtDebug.s_logLevel = (m_debug ? rdtDebug.LogLevel.Debug : rdtDebug.LogLevel.Info);
			m_showInstanceID = EditorPrefs.GetBool("Hdg.RemoteDebug.ShowInstanceID", false);
			rdtDebug.s_showInstanceID = m_showInstanceID;

			rdtDebug.Debug("OnEnable");
			s_instance = this;
			m_split = new rdtGuiSplit(200f, 100f, this);
			EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
			RestartServerEnumerator();
			m_automaticRefresh = EditorPrefs.GetBool("Hdg.RemoteDebug.AutomaticRefresh", false);
			m_serversMenu = new ServersMenu(OnServerSelected, this);
			m_isProSkin = EditorGUIUtility.isProSkin;
			m_expandedCache.Clear();
			OnEnableHierarchy();
			OnEnableFiles();
		}

		private void OnDisable()
		{
			rdtDebug.Debug("OnDisable");
			EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
			s_instance = null;
			m_serversMenu.Destroy();
			Disconnect(false);
			if (m_serverEnum != null)
				m_serverEnum.Stop();
			OnDisableHierarchy();
			OnDisableFiles();
		}

		private void Update()
		{
			if (EditorApplication.isCompiling && m_client != null && m_client.IsConnected)
				Disconnect(false);
			double time = EditorApplication.timeSinceStartup;
			double delta = time - m_lastTime;
			m_lastTime = time;
			UpdateServers(delta);
			if (m_client != null)
			{
				bool connected = m_client.IsConnected;
				bool connecting = m_client.IsConnecting;
				m_client.Update(delta);
				if (m_client.IsConnected != connected || (!m_client.IsConnected && !m_client.IsConnecting && connecting))
				{
					m_isPlaying = m_client.IsConnected;
					OnConnectionStatusChanged();
				}
				if (m_automaticRefresh || m_forceRefreshHierarchy)
				{
					UpdateHierarchy(delta);
				}
				if (m_forceRefreshFile)
				{
					UpdateFiles(delta);
				}
				if (m_forceRepaint)
					ForceRepaint();
			}
		}

		private void HardReload()
        {
			if (m_client == null || !m_client.IsConnected || m_waitingForPlayingChanged)
				return;
			AddOperationCallback(RemoteDebugServer.Operation_HardReload, OnMessageOperationCallback);
			rdtTcpMessageOperation msg = new rdtTcpMessageOperation()
			{
				m_operation = RemoteDebugServer.Operation_HardReload,
				m_properties = new List<rdtTcpMessageComponents.Property>(),
			};
			m_client.EnqueueMessage(msg);
			m_waitingForPlayingChanged = true;
		}

		private void RestartOrStopGame()
        {
			if (m_client == null || !m_client.IsConnected || m_waitingForPlayingChanged)
				return;
			if (EditorUtility.DisplayDialog("Restart Confirm", "Do you want to restart the game ?", "OK", "Cancel"))
			{
				AddOperationCallback(RemoteDebugServer.Operation_RestartGame, OnMessageOperationCallback);
				rdtTcpMessageOperation msg = new rdtTcpMessageOperation()
				{
					m_operation = RemoteDebugServer.Operation_RestartGame,
					m_properties = new List<rdtTcpMessageComponents.Property>(),
				};
				m_client.EnqueueMessage(msg);
				m_waitingForPlayingChanged = true;
			}
		}

		private void PlayOrPauseGame()
        {
			if (m_client == null || !m_client.IsConnected || m_waitingForPlayingChanged)
				return;
			rdtDebug.Debug(this, m_isPlaying ? "Play Game" : "Pause Game");

			AddOperationCallback(RemoteDebugServer.Operation_PauseGame, OnMessageOperationCallback);

			List<rdtTcpMessageComponents.Property> list = new List<rdtTcpMessageComponents.Property>();
			m_serializerRegistry.AddField(list, "param1", !m_isPlaying, rdtTcpMessageComponents.Property.Type.Property, null, false);
			rdtTcpMessageOperation msg = new rdtTcpMessageOperation()
			{
				m_operation = RemoteDebugServer.Operation_PauseGame,
				m_properties = list,
			};
			m_client.EnqueueMessage(msg);
			m_waitingForPlayingChanged = true;

		}

		private void OnMessageOperationCallback(object value)
		{
			m_waitingForPlayingChanged = false;
		}

		private void OnGUI()
		{
			if (m_clearFocus)
			{
				m_clearFocus = false;
				GUI.FocusControl(null);
			}
			InitStylesAndContent();
			DrawToolbar();
			Draw();
			ProcessInput();
		}

		private void InitStylesAndContent()
		{
			if (m_iconButtonStyle == null)
			{
				m_iconButtonStyle = new GUIStyle("IconButton");
			}
			if (m_isProSkin == EditorGUIUtility.isProSkin && m_toggleStyle != null && m_normalFoldoutStyle != null && m_toggleStyle.normal.background != null)
				return;
			m_isProSkin = EditorGUIUtility.isProSkin;
			m_toggleStyle = new GUIStyle(EditorStyles.toggle);
			m_toggleStyle.overflow.top = -2;
			m_normalFoldoutStyle = new GUIStyle(EditorStyles.foldout);
			m_normalFoldoutStyle.overflow.top = -2;
			m_normalFoldoutStyle.active.textColor = EditorStyles.foldout.normal.textColor;
			m_normalFoldoutStyle.onActive.textColor = EditorStyles.foldout.normal.textColor;
			m_normalFoldoutStyle.onFocused.textColor = EditorStyles.foldout.normal.textColor;
			m_normalFoldoutStyle.onFocused.background = EditorStyles.foldout.onNormal.background;
			m_normalFoldoutStyle.focused.textColor = EditorStyles.foldout.normal.textColor;
			m_normalFoldoutStyle.focused.background = EditorStyles.foldout.normal.background;
			m_automaticRefreshOnContent = EditorGUIUtility.TrIconContent("d_AutoLightbakingOn", "Automatic Refresh On");
			m_automaticRefreshOffContent = EditorGUIUtility.TrIconContent("d_AutoLightbakingOff", "Automatic Refresh Off");
			m_refreshContent = EditorGUIUtility.TrIconContent("d_RotateTool On", "Refresh");
			m_settingContent = EditorGUIUtility.TrIconContent("d__Popup", "Setting");
			m_hardReloadIconContent = EditorGUIUtility.TrIconContent("d_Refresh", "Hard reload lua files");
			m_closeIconContent = EditorGUIUtility.TrIconContent("Close", "Restart game");
			m_showInstanceContent = EditorGUIUtility.TrIconContent("d_animationvisibilitytoggleon", "Show Instance ID");
			m_hideInstanceContent = EditorGUIUtility.TrIconContent("d_animationvisibilitytoggleoff", "Hide Instance ID");
			m_hierarchyIconContent = EditorGUIUtility.TrTextContent("Hierarchy", "GameObjects in game hierarchy", "d_UnityEditor.SceneHierarchyWindow");
			m_filesIconContent = EditorGUIUtility.TrTextContent("Persistent", "Files in persistentDataPath", "d_Project");
			m_pageContents = new GUIContent[]
			{
				m_hierarchyIconContent,
				m_filesIconContent,
			};
			InitHierarchyStylesAndContent();
			InitFilesStylesAndContent();

		}

		private void DrawToolbar()
        {
			GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            m_serversMenu.Show(m_currentServer);
			bool flag = m_client != null && m_client.IsConnecting;
			bool isConnected = m_client != null && m_client.IsConnected;
			string serverName = flag ? "Connecting" : ((isConnected && m_currentServer != null) ? m_currentServer.ToString() : "Not connected");
			if (isConnected && m_currentServer != null && m_debug)
				serverName = serverName + " - Server Version " + m_currentServer.m_serverVersion;
			GUILayout.Label(serverName, EditorStyles.toolbarButton, GUILayout.MinWidth(64));
			GUILayout.FlexibleSpace();
			bool showInstanceID = GUILayout.Toggle(m_showInstanceID, m_showInstanceID ? m_showInstanceContent : m_hideInstanceContent, EditorStyles.toolbarButton);
			if (showInstanceID != m_showInstanceID)
			{
				m_showInstanceID = showInstanceID;
				rdtDebug.s_showInstanceID = showInstanceID;
				EditorPrefs.SetBool("Hdg.RemoteDebug.ShowInstanceID", m_showInstanceID);
			}
			bool prevEnabled = GUI.enabled;
			bool autoRefresh = GUILayout.Toggle(m_automaticRefresh, m_automaticRefresh ? m_automaticRefreshOnContent : m_automaticRefreshOffContent, EditorStyles.toolbarButton);
			if (autoRefresh != m_automaticRefresh)
			{
				m_automaticRefresh = autoRefresh;
				EditorPrefs.SetBool("Hdg.RemoteDebug.AutomaticRefresh", m_automaticRefresh);
			}
			if (m_pageType == PAGE_TYPE_HIERARCHY)
				GUI.enabled = isConnected && !m_waitingForGameObjects && !m_automaticRefresh;
			else if (m_pageType == PAGE_TYPE_FILES)
				GUI.enabled = isConnected && !m_waitingForFiles;
			if (GUILayout.Button(m_refreshContent, EditorStyles.toolbarButton))
			{
				if (m_pageType == PAGE_TYPE_HIERARCHY)
				{
					RefreshGameObjects();
					RefreshComponents();
				}
				else if (m_pageType == PAGE_TYPE_FILES)
					RefreshFiles();
			}
			GUI.enabled = prevEnabled;
			if (GUILayout.Button(m_settingContent, EditorStyles.toolbarButton))
            {
				//SettingsWindowHelper.Show(SettingsScope.User, "Preferences/Remote Debug");
			}
            //if (GUILayout.Button("About", EditorStyles.toolbarButton))
            //	ShowAbout();
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			GUI.enabled = isConnected && !m_waitingForPlayingChanged;
			// int pageType = EditorGUILayoutHelper.CycleButton(m_pageType, m_pageContents, EditorStyles.toolbarButton);
			// if (pageType != m_pageType)
			// 	OnPageChanged(pageType);
			GUI.enabled = true;
			GUILayout.Space(5);
			//Filter = EditorGUILayoutHelper.ToolbarSearchField(Filter, GUILayout.Width(250f));
			GUILayout.FlexibleSpace();
			GUI.enabled = isConnected && !m_waitingForPlayingChanged;
			if (GUILayout.Button(m_hardReloadIconContent, m_iconButtonStyle))
			{
				HardReload();
			}
			if (GUILayout.Button(m_closeIconContent, m_iconButtonStyle))
			{
				RestartOrStopGame();
			}
			if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_preAudioPlay" + (isConnected && m_isPlaying ? "On" : "Off")), m_iconButtonStyle))
			{
				m_isPlaying = !m_isPlaying;
				PlayOrPauseGame();
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		private void ShowAbout()
		{
			string msg = "\r\n    Hdg Remote Debug\r\n    Version {0}.{1}.{2} {3} {4}\r\n\r\n    http://www.horsedrawngames.com\r\n    info@horsedrawngames.com\r\n\r\n    (c) 2017 Horse Drawn Games Pty Ltd";
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Version version = executingAssembly.GetName().Version;
			object[] assemblyInfoVersionAttr = executingAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
			string beta = "";
			if (assemblyInfoVersionAttr.Length != 0)
				beta = ((AssemblyInformationalVersionAttribute)assemblyInfoVersionAttr[0]).InformationalVersion;
			object[] assemblyConfigurationAttr = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
			string configuration = "";
			if (assemblyConfigurationAttr.Length != 0)
				configuration = ((AssemblyConfigurationAttribute)assemblyConfigurationAttr[0]).Configuration;
			EditorUtility.DisplayDialog("About", string.Format(msg, new object[]
			{
				version.Major,
				version.Minor,
				version.Build,
				beta,
				configuration
			}), "Ok");
		}

		private void Draw()
		{
			EditorGUILayout.BeginHorizontal();
			bool windowHasFocus = focusedWindow == this;
			switch(m_pageType)
            {
				case PAGE_TYPE_HIERARCHY:
					DrawHierarchy(windowHasFocus);
					break;
				case PAGE_TYPE_FILES:
					DrawFiles(windowHasFocus);
					break;
            }
			EditorGUILayout.EndHorizontal();
		}

		private void ProcessInput()
		{
			Event evt = Event.current;
			if (evt.isMouse && evt.type == EventType.MouseUp && m_pendingExpandComponent != null)
			{
				GUI.FocusControl(null);
				bool expanded = m_expandedCache.IsExpanded(m_pendingExpandComponent.Value, null);
				m_expandedCache.SetExpanded(!expanded, m_pendingExpandComponent.Value, null);
				m_pendingExpandComponent = null;
				Repaint();
			}
		}

		private void ForceRepaint()
        {
			m_forceRepaint = false;
			Repaint();
		}

		private void OnPageChanged(int pageType)
        {
			m_pageType = pageType;
			switch (m_pageType)
			{
				case PAGE_TYPE_HIERARCHY:
					m_hierarchyTree.Filter = Filter;
					m_gameObjectRefreshTimer = 0.10000000149011612;//=0.1f
					m_forceRefreshHierarchy = true;
					break;
				case PAGE_TYPE_FILES:
					m_filesTree.Filter = Filter;
					m_fileRefreshTimer = 0.10000000149011612;//=0.1f
					m_forceRefreshFile = true;
					break;
			}
		}

		private void OnMessageOperationReturn(rdtTcpMessage message)
        {
			rdtTcpMessageReturnResult msg = (rdtTcpMessageReturnResult)message;
			if (msg.m_result.m_name == rdtTcpMessageReturnResult.RETURN_SUCCESS)
				m_operationCallback.ReturnCallback(msg.m_operation, msg.m_result.m_value);
			else
				rdtDebug.Error(this, "Operation {0} return excetion {1}", msg.m_operation, msg.m_result.m_value);
		}

		public void AddOperationCallback(string operation, Action<object> callback)
        {
			m_operationCallback.SetCallback(operation, callback);
		}

		private void OnConnectionStatusChanged()
		{
			rdtDebug.Debug(this, "OnConnectionStatusChanged");
			if (m_client == null || !m_client.IsConnected)
				m_currentServer = null;

			m_expandedCache.Clear();
			ConnectionStatusChangedHierarchy();
			ConnectionStatusChangedFiles();
			Repaint();
		}

		private void UpdateServers(double delta)
		{
			m_serverEnum.Update(delta);
			m_serversMenu.Servers = m_serverEnum.Servers;
		}

		private void OnServerSelected(rdtServerAddress server)
		{
			if (server != null)
			{
				Connect(server);
				return;
			}
			Disconnect(true);
		}

		[DidReloadScripts]
		private static void OnUnityReloadedAssemblies()
		{
			if (s_instance == null)
				return;
			s_instance.OnUnityReloadedAssembliesImp();
		}

		private void OnUnityReloadedAssembliesImp()
		{
			rdtDebug.Debug(this, "OnUnityReloadedAssemblies");
			rdtDebug.s_logLevel = (m_debug ? rdtDebug.LogLevel.Debug : rdtDebug.LogLevel.Info);
			rdtDebug.s_showInstanceID = m_showInstanceID;
			if (m_currentServer != null)
			{
				if (m_currentServer.IPAddress == null)
				{
					m_currentServer = null;
					return;
				}
				Connect(m_currentServer);
			}
		}

		private void OnPlaymodeStateChanged(PlayModeStateChange state)
		{
			Disconnect(true);
		}
	}
}
