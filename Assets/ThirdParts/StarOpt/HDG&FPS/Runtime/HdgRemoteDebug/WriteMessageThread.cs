using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GameDLL.Hdg
{
	public class WriteMessageThread
	{
		//private enum State
		//{
		//	Idle,
		//	Writing,
		//	LostConnection,
		//	Max
		//}

		public const int STATE_IDLE = 0;
		public const int STATE_WRITING = 1;
		public const int STATE_LOST_CONNECTION = 2;


		private Stream m_stream;

		private BinaryWriter m_writer;

		private Queue<rdtTcpMessage> m_messageQueue = new Queue<rdtTcpMessage>();

		private bool m_run;

		private Action[] m_stateDelegates;

		private int m_state;

		private rdtTcpMessage m_currentMessage;

		private AutoResetEvent m_event = new AutoResetEvent(false);

		private Thread m_thread;

		private string m_name;

		public bool IsConnected
		{
			get
			{
				return m_state != STATE_LOST_CONNECTION;
			}
		}

		public WriteMessageThread(Stream stream, string name)
		{
			m_name = name;
			m_stateDelegates = new Action[3]
			{
				OnIdle,
				OnWriting,
				OnLostConnection,
			};
			m_stream = stream;
			m_writer = new BinaryWriter(m_stream);
			m_run = true;
			m_thread = new Thread(new ThreadStart(ThreadFunc));
			m_thread.Name = m_name + " rdtWriteMessageThread";
			m_thread.Start();
		}

		public void Stop()
		{
			m_run = false;
			m_event.Set();
			m_thread.Join();
		}

		public void EnqueueMessage(rdtTcpMessage message)
		{
			lock (m_messageQueue)
			{
				m_messageQueue.Enqueue(message);
			}
			m_event.Set();
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

		private void OnIdle()
		{
			m_currentMessage = null;
			Queue<rdtTcpMessage> messageQueue = m_messageQueue;
			lock (messageQueue)
			{
				if (m_messageQueue.Count > 0)
					m_currentMessage = m_messageQueue.Dequeue();
			}
			if (m_currentMessage != null)
			{
				m_state = STATE_WRITING;
				return;
			}
			m_event.WaitOne();
		}

		private void OnWriting()
		{
			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (BinaryWriter bw = new BinaryWriter(ms))
					{
                       
						string name = m_currentMessage.GetType().FullName;
                        bw.Write(name);
						m_currentMessage.Write(bw);
						byte[] bytes = ms.ToArray();
						m_writer.Write(bytes.Length);
						m_writer.Write(bytes);
						m_writer.Flush();
                        
					}
				}
				m_state = STATE_IDLE;
			}
			catch (IOException ioe)
			{
				rdtDebug.Log(this, ioe, rdtDebug.LogLevel.Debug, "{0} lost connection", m_name);
				m_state = STATE_LOST_CONNECTION;
			}
			catch (ObjectDisposedException)
			{
				rdtDebug.Debug(this, "{0} object disposed, lost connection", m_name);
				m_state = STATE_LOST_CONNECTION;
			}
			catch (Exception e)
			{
				rdtDebug.Error(this, e, "{0} Unknown exception", m_name);
				m_state = STATE_LOST_CONNECTION;
			}
		}

		private void OnLostConnection()
		{
			m_run = false;
		}
	}
}
