using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDLL.Hdg
{
	[DisallowMultipleComponent]
	public class RemoteDebugServer : MonoBehaviour, rdtTcpEnqueue
	{
		private enum State
		{
			None,
			Waiting,
			Connecting,
			Connected,
			Disconnected,
			Max
		}

		private static RemoteDebugServer s_instance;
		public static RemoteDebugServer Instance
		{
			get
			{
				if (s_instance == null)
					s_instance = FindObjectOfType<RemoteDebugServer>();
				return s_instance;
			}
			set
			{
				s_instance = value;
			}
		}

		internal Action<bool> pauseGame;

		public Action onDisConnectAction;

		internal bool createFromServerFactory = false;

		private ServerBroadcaster m_broadcaster;

		private State m_state;

		private TcpListener m_listener;

		private TcpClient m_client;

		private IAsyncResult m_currentAsyncResult;

		private Action[] m_stateDelegates;

		public event Action<bool> ConnectedChanged;

		private WriteMessageThread m_writeThread;

		private ReadMessageThread m_readThread;

		private rdtDispatcher m_dispatcher = new rdtDispatcher();

		private Dictionary<Type, Action<rdtTcpMessage>> m_messageCallbacks = new Dictionary<Type, Action<rdtTcpMessage>>();

		private rdtSerializerRegistry m_serializerRegistry = new rdtSerializerRegistry();

		private List<rdtTcpMessage> m_messagesToProcess;

		private List<GameObject> m_dontDestroyOnLoadObjects = new List<GameObject>();

		private rdtMessageOperationHandler m_operationHandler;

		public const string Operation_PauseGame = "PauseGame";

		public const string Operation_RestartGame = "RestartGame";

		public const string Operation_HardReload = "HardReload";

		public List<GameObject> DontDestroyOnLoadObjects
		{
			get
			{
				return m_dontDestroyOnLoadObjects;
			}
		}

		public rdtSerializerRegistry SerializerRegistry
		{
			get
			{
				if (m_serializerRegistry == null)
					m_serializerRegistry = new rdtSerializerRegistry();
				return m_serializerRegistry;
			}
		}

		public string ClientIP
		{
			get
			{
				if (m_client == null)
					return "";
				return m_client.Client.RemoteEndPoint.ToString();
			}
		}

		public void AddCallback(Type type, Action<rdtTcpMessage> callback)
		{
			if (!m_messageCallbacks.ContainsKey(type))
				m_messageCallbacks[type] = null;
			//else
				m_messageCallbacks[type] += callback;
			//m_messageCallbacks[type] = (Action<rdtTcpMessage>)Delegate.Combine(m_messageCallbacks[type], callback);
		}

		public static void AddDontDestroyOnLoadObject(GameObject gob)
		{
			if (Instance == null)
				return;
			if (!Instance.m_dontDestroyOnLoadObjects.Contains(gob))
				Instance.m_dontDestroyOnLoadObjects.Add(gob);
		}

		public static void RemoveDontDestroyOnLoadObject(GameObject gob)
		{
			if (Instance == null)
				return;
			Instance.m_dontDestroyOnLoadObjects.Remove(gob);
		}

		public void EnqueueMessage(rdtTcpMessage message)
		{
			if (m_writeThread != null)
				m_writeThread.EnqueueMessage(message);
		}

		[RemoteDebugAction]
		public void ToggleWorldPaused()
		{
			Time.timeScale = ((Time.timeScale == 0f) ? 1f : 0f);
		}

		bool PauseGame(bool pause)
		{
			if (pauseGame != null)
				pauseGame(pause);
			else
			{
				Time.timeScale = (pause ? 0f : 1f);
			}
			return true;
        }

		void RestartGame()
		{
			//TerminalCMD.RunCommand("restart");
        }

		void HardReload()
		{
			//TerminalCMD.RunCommand("hardreload");
		}

		private void Init()
		{
			 RegisterCallbacks();
			if (Application.platform == RuntimePlatform.IPhonePlayer)
				Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
			m_messagesToProcess = new List<rdtTcpMessage>(16);
			m_stateDelegates = new Action[5]
			{
				null,
				OnWaiting,
				OnConnecting,
				OnConnected,
				OnDisconnected,
			};
			m_broadcaster = new ServerBroadcaster();
		}

		private void RegisterCallbacks()
		{
			if (!Application.isEditor && createFromServerFactory)
				//Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
			m_messageCallbacks.Clear();
			new rdtMessageFileSystemInfosHandler(this);
			new rdtMessageGameObjectsHandler(this);
			m_operationHandler = new rdtMessageOperationHandler(this);

			m_operationHandler.AddOperation(Operation_PauseGame, typeof(RemoteDebugServer).GetMethod("PauseGame", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(bool)}, null), this, true);
			m_operationHandler.AddOperation(Operation_RestartGame, typeof(RemoteDebugServer).GetMethod("RestartGame", BindingFlags.Instance | BindingFlags.NonPublic), this, true);
			m_operationHandler.AddOperation(Operation_HardReload, typeof(RemoteDebugServer).GetMethod("HardReload", BindingFlags.Instance | BindingFlags.NonPublic), this, true);
		}

		internal void OnLogMessageReceivedThreaded(string message, string stackTrace, LogType type)
		{
			rdtTcpMessageLog msg = new rdtTcpMessageLog (message, stackTrace, type);
			EnqueueMessage(msg);
		}

		private void OnEnable()
		{
			rdtDebug.Debug(this, "OnEnable");
			if (s_instance != null && s_instance.GetInstanceID() != GetInstanceID())
			{
				Object.Destroy(gameObject);
				RemoveDontDestroyOnLoadObject(gameObject);
				return;
			}
			Instance = this;
			rdtDebug.Debug(this, "{0} {1}", s_instance.GetInstanceID(), gameObject.GetInstanceID());
			Object.DontDestroyOnLoad(gameObject);
			AddDontDestroyOnLoadObject(gameObject);
			Init();
			StartListening();
		}

		private void OnDisable()
		{
			rdtDebug.Debug(this, "OnDisable");
			if (m_broadcaster == null)
				return;
			m_broadcaster.Stop();
			if (m_listener != null)
				m_listener.Stop();
			m_listener = null;
			Stop();
			SetState(State.None);
		}

		private void SetState(State state)
		{
			rdtDebug.Debug(this, "State is {0}", state);
			m_state = state;
			ConnectedChanged?.Invoke(state == State.Connected);
		}

		private void StartListening()
		{
			try
			{
				rdtDebug.Debug(this, "StartListening()");
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Settings.DEFAULT_SERVER_PORT);
				m_listener = new TcpListener(endPoint);
				m_listener.Start(1);
				SetState(State.Waiting);
			}
			catch (Exception ex)
			{
				rdtDebug.Error(this, "Failed to listen to server port ({0})", ex.Message);
			}
		}

		private void Stop()
		{
			if (m_client != null)
			{
				m_client.Close();
				m_client = null;
			}
			if (m_readThread != null)
			{
				m_readThread.Stop();
				m_readThread = null;
			}
			if (m_writeThread != null)
			{
				m_writeThread.Stop();
				m_writeThread = null;
			}
			m_dispatcher.Clear();
			if(onDisConnectAction!=null)
				onDisConnectAction();
        }

		private void Update()
		{
			try
			{
				if (m_stateDelegates[(int)m_state] != null)
					m_stateDelegates[(int)m_state]();
			}
			catch (SocketException)
			{
				Stop();
				StartListening();
			}
		}

		private void OnWaiting()
		{
			if (m_listener.Pending())
			{
				SetState(State.Connecting);
				m_currentAsyncResult = m_listener.BeginAcceptTcpClient(null, null);
			}
		}

		private void OnConnecting()
		{
			if (m_currentAsyncResult.IsCompleted)
			{
				try
				{
					m_client = m_listener.EndAcceptTcpClient(m_currentAsyncResult);
					rdtDebug.Debug(this, "Client connected from " + m_client.Client.RemoteEndPoint);
					m_readThread = new ReadMessageThread(m_client.GetStream(), m_dispatcher, OnReadMessage, "Server");
					m_writeThread = new WriteMessageThread(m_client.GetStream(), "Server");
					SetState(State.Connected);
				}
				catch (SocketException se)
				{
					rdtDebug.Error(this, "Socket exception while client was connecting (error code {0})", se.ErrorCode);
					StartListening();
				}
				catch (ObjectDisposedException)
				{
					rdtDebug.Error(this, "Tcp listener was disposed");
					StartListening();
				}
			}
		}

		private void OnConnected()
		{
			if (m_listener.Pending())
				m_listener.AcceptTcpClient().Close();
			if (!m_readThread.IsConnected || !m_writeThread.IsConnected || !m_client.Connected)
			{
				rdtDebug.Debug(this, "Read/write thread or the client disconnected");
				SetState(State.Disconnected);
				return;
			}
			m_dispatcher.Update();
			bool foundGetGameObjects = false;
			bool foundGetComponents = false;
			for (int i = 0; i < m_messagesToProcess.Count; i++)
			{
				Type msgType = m_messagesToProcess[i].GetType();
				if (msgType == typeof(rdtTcpMessageGetGameObjects))
				{
					if (foundGetGameObjects)
					{
						m_messagesToProcess.RemoveAt(i);
						i--;
					}
					else
						foundGetGameObjects = true;
				}
				else if (msgType == typeof(rdtTcpMessageGetComponents))
				{
					if (foundGetComponents)
					{
						m_messagesToProcess.RemoveAt(i);
						i--;
					}
					else
						foundGetComponents = true;
				}
			}
			while (m_messagesToProcess.Count > 0)
			{
				rdtTcpMessage msg = m_messagesToProcess[0];
				m_messagesToProcess.RemoveAt(0);
                m_messageCallbacks[msg.GetType()](msg);
			}
		}

		private void OnDisconnected()
		{
			rdtDebug.Debug(this, "Client disconnected", new object[0]);
            Stop();
			SetState(State.Waiting);
		}

		private void OnReadMessage(rdtTcpMessage message)
		{
			if (m_messageCallbacks.ContainsKey(message.GetType()))
				m_messagesToProcess.Add(message);
		}

		private void OnApplicationPause(bool pause)
		{
			if (!Application.isMobilePlatform)
				return;
			if (pause)
			{
				rdtDebug.Debug(this, "OnApplicationPause: Is pausing");
				if (m_listener != null)
				{
					m_listener.Stop();
					m_listener = null;
				}
				Stop();
				SetState(State.None);
				return;
			}
			if (m_listener == null)
			{
				rdtDebug.Debug(this, "OnApplicationPause: Is resuming");
				StartListening();
			}
		}
	}
}
