using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace GameDLL.Hdg
{
	public class ReadMessageThread
	{
		//private enum State
		//{
		//	Reading,
		//	LostConnection,
		//	Max
		//}

		public const int STATE_READING = 0;
		public const int STATE_LOST_CONNECTION = 1;


		private Action[] m_stateDelegates;

		private int m_state;

		private rdtDispatcher m_dispatcher;

		private Stream m_stream;

		private BinaryReader m_reader;

		private Thread m_thread;

		private bool m_run;

		private Action<rdtTcpMessage> m_callback;

		private string m_name;

		public bool IsConnected
		{
			get
			{
				return m_state != STATE_LOST_CONNECTION;
			}
		}

		public ReadMessageThread(Stream stream, rdtDispatcher dispatcher, Action<rdtTcpMessage> callback, string name)
		{
			m_name = name;
			m_stateDelegates = new Action[2]
			{
				OnReading,
				OnLostConnection,
			};
			m_stream = stream;
			m_reader = new BinaryReader(m_stream);
			m_dispatcher = dispatcher;
			m_callback = callback;
			m_run = true;
			m_thread = new Thread(new ThreadStart(ThreadFunc));
			m_thread.Name = m_name + " rdtReadMessageThread";
			m_thread.Start();
		}

		public void Stop()
		{
			m_run = false;
			m_thread.Join();
		}

		private void ThreadFunc()
		{
			while (m_run)
			{
				if (m_stateDelegates[m_state] != null)
					m_stateDelegates[m_state]();
			}
			rdtDebug.Debug(this, "Exited");
		}

		private void OnReading()
		{
			try
			{
				bool disconnected = true;
				int msgLen = m_reader.ReadInt32();
				if (msgLen == 0)
					disconnected = true;
				else
				{
					byte[] buffer = m_reader.ReadBytes(msgLen);
					disconnected = false;
					using (MemoryStream ms = new MemoryStream(buffer))
					{
						using (BinaryReader br = new BinaryReader(ms))
						{
							Type type = Type.GetType(br.ReadString());
							rdtTcpMessage message = Activator.CreateInstance(type) as rdtTcpMessage;
							if (message != null)
							{
								message.Read(br);
								m_dispatcher.Enqueue(() =>
								{
									m_callback(message);
								});
							}
							else
							{
								rdtDebug.Error(this, "Ignoring invalid message");
							}
						}
					}
				}
				if (disconnected)
				{
					m_state = STATE_LOST_CONNECTION;
				}
			}
			catch (SocketException se)
			{
				rdtDebug.Log(this, se, rdtDebug.LogLevel.Debug, "{0} socket exception", m_name);
				rdtDebug.Debug(this, "{3} ErrorCode={0} SocketErrorCode={1} NativeErrorCode={2}", se.ErrorCode, se.SocketErrorCode, se.NativeErrorCode, m_name);
				m_state = STATE_LOST_CONNECTION;
			}
			catch (IOException ioe)
			{
				SocketException se2 = ioe.InnerException as SocketException;
				if (se2 != null)
				{
					rdtDebug.Log(this, se2, rdtDebug.LogLevel.Debug, "{0} socket exception", m_name);
					rdtDebug.Debug(this, "{3} ErrorCode={0} SocketErrorCode={1} NativeErrorCode={2}", se2.ErrorCode, se2.SocketErrorCode, se2.NativeErrorCode, m_name);
				}
				else
				{
					rdtDebug.Log(this, ioe, rdtDebug.LogLevel.Debug, "{0} thread lost connection", m_name);
				}
				m_state = STATE_LOST_CONNECTION;
			}
			catch (ObjectDisposedException)
			{
				rdtDebug.Debug(this, "{0} thread object disposed, lost connection", m_name);
				m_state = STATE_LOST_CONNECTION;
			}
			catch (Exception e)
			{
				rdtDebug.Error(this, e, "{0} thread unknown exception", m_name);
				m_state = STATE_LOST_CONNECTION;
			}
		}

		private void OnLostConnection()
		{
			m_run = false;
		}
	}
}
