using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using GameDLL.Hdg;

namespace GameEditor.Hdg
{
	public class rdtClient : rdtTcpEnqueue
	{
		private enum State
		{
			None,
			Connecting,
			Connected,
			Disconnected,
			Max
		}

		private State m_state;

		private TcpClient m_client;

		private IAsyncResult m_currentAsyncResult;

		private Action<double>[] m_stateDelegates;

		private WriteMessageThread m_writeThread;

		private ReadMessageThread m_readThread;

		private rdtDispatcher m_dispatcher = new rdtDispatcher();

		private Dictionary<Type, Action<rdtTcpMessage>> m_messageCallbacks = new Dictionary<Type, Action<rdtTcpMessage>>();

		public bool IsConnected
		{
			get
			{
				return m_state == State.Connected;
			}
		}

		public bool IsConnecting
		{
			get
			{
				return m_state == State.Connecting;
			}
		}

		public rdtClient()
		{
			m_stateDelegates = new Action<double>[4]
			{
				null,
				OnConnecting,
				OnConnected,
				OnDisconnected,
			};
		}

		public void AddCallback(Type type, Action<rdtTcpMessage> callback)
		{
			if (!m_messageCallbacks.ContainsKey(type))
				m_messageCallbacks[type] = null;
			//else
				m_messageCallbacks[type] += callback;
			//m_messageCallbacks[type] = (Action<rdtTcpMessage>)Delegate.Combine(m_messageCallbacks[type], callback);
		}

		private void SetState(rdtClient.State state)
		{
			rdtDebug.Debug(this, "State is {0}", state);
			m_state = state;
		}

		public void Connect(IPAddress address, int port)
		{
			m_client = new TcpClient();
			m_currentAsyncResult = m_client.BeginConnect(address, port, null, null);
			SetState(State.Connecting);
		}

		public void EnqueueMessage(rdtTcpMessage message)
		{
			if (m_writeThread != null)
				m_writeThread.EnqueueMessage(message);
		}

		public void Stop()
		{
			if (m_client != null)
			{
				rdtDebug.Debug(this, "Stopping connection");
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
            rdtTcpMessageTransferFile.ClearTransferFile();
            SetState(State.None);
		}

		public void Update(double delta)
		{
			if (m_stateDelegates[(int)m_state] != null)
				m_stateDelegates[(int)m_state](delta);
		}

		private void OnConnecting(double delta)
		{
			if (m_currentAsyncResult.IsCompleted)
			{
				try
				{
					m_client.EndConnect(m_currentAsyncResult);
					rdtDebug.Debug(this, "Connected to server");
					m_writeThread = new WriteMessageThread(m_client.GetStream(), "rdtClient");
					m_readThread = new ReadMessageThread(m_client.GetStream(), m_dispatcher, OnReadMessage, "rdtClient");
					SetState(State.Connected);
				}
				catch (SocketException se)
				{
					int errorCode = se.ErrorCode;
					if (errorCode != 10060)
					{
						if (errorCode == 10061)
							rdtDebug.Info("RemoteDebug: Connection was refused");
						else
							rdtDebug.Info("RemoteDebug: Failed to connect to server (error code {0})", se.ErrorCode);
					}
					else
						rdtDebug.Info("RemoteDebug: Connection timed out");
					Stop();
				}
				catch (ObjectDisposedException)
				{
					rdtDebug.Error(this, "Client was disposed");
					Stop();
				}
			}
		}

		private void OnConnected(double delta)
		{
			if (!m_client.Connected || !m_writeThread.IsConnected || !m_readThread.IsConnected)
			{
				SetState(State.Disconnected);
				return;
			}
			m_dispatcher.Update();
		}

		private void OnDisconnected(double delta)
		{
			rdtDebug.Debug(this, "Server disconnected");
			Stop();
		}

		private void OnReadMessage(rdtTcpMessage message)
		{
			if (message is rdtTcpMessageLog)
			{
				rdtTcpMessageLog logMsg = (rdtTcpMessageLog)message;
				switch (logMsg.m_logType)
				{
				case LogType.Error:
					Debug.LogError(logMsg);
					break;
				case LogType.Assert:
					Debug.LogError(logMsg);
					break;
				case LogType.Warning:
					Debug.LogWarning(logMsg);
					break;
				case LogType.Log:
					Debug.Log(logMsg);
					break;
				case LogType.Exception:
					Debug.LogError(logMsg);
					break;
				}
			}
			if (m_messageCallbacks.ContainsKey(message.GetType()))
				m_messageCallbacks[message.GetType()](message);
		}
	}
}
