using System;
using System.IO;
using System.Threading;
using UnityEngine;
namespace GameDLL.Hdg
{
    public struct rdtTcpMessageTransferFile : rdtTcpMessage
    {
		public int m_hash;
        public int m_fileLength;
		public int m_offset;
		public int m_length;
		public byte[] m_bytes;
        public void Write(BinaryWriter w)
		{
			w.Write(m_hash);
			w.Write(m_fileLength);
			w.Write(m_offset);
			w.Write(m_length);
			w.Write(m_bytes, 0, m_length);
			m_isSending = false;
		}
        public void Read(BinaryReader r)
        {
			m_hash = r.ReadInt32();
			m_fileLength = r.ReadInt32();
			m_offset = r.ReadInt32();
			m_length = r.ReadInt32();
			if (m_length > 0)
                m_bytes = r.ReadBytes(m_length);
            else
                m_bytes = new byte[0];
        }

        private static Thread m_thread;

        private static bool m_run;

		/// <summary>
		/// 单次最大传输100K字节
		/// </summary>
		private const int FileBlockMaxSize = 1024 * 100;

		private static FileRequestInfo m_requestInfo;

		private static bool m_isSending;
		class FileRequestInfo
		{
			public rdtTcpEnqueue m_enqueue;

			public string m_path;

			public int m_hash;

			public int m_fileLength;

			public Action<float> m_progressAction;
		}

		public static int StartTransferFile(rdtTcpEnqueue enqueue, string path, int hash, Action<float> progress = null)
        {
			FileInfo file = new FileInfo(path);
			long fileLength = file.Length;
			if (fileLength <= FileBlockMaxSize)
			{
				rdtTcpMessageTransferFile ret = default(rdtTcpMessageTransferFile);
				byte[] bytes = File.ReadAllBytes(path);
				ret.m_hash = hash;
				ret.m_fileLength = bytes.Length;
				ret.m_offset = 0;
				ret.m_length = bytes.Length;
				ret.m_bytes = bytes;
				enqueue.EnqueueMessage(ret);
				return 1;
			}
			else if (!m_run)
			{
				if (m_requestInfo == null)
					m_requestInfo = new FileRequestInfo();
				m_requestInfo.m_fileLength = (int)fileLength;
				m_requestInfo.m_enqueue = enqueue;
				m_requestInfo.m_path = path;
				m_requestInfo.m_hash = hash;
				m_requestInfo.m_progressAction = progress;
				m_run = true;
				m_thread = new Thread(new ThreadStart(ThreadFunc));
				m_thread.Name = "rdtTcpMessageTransferFile";
				m_thread.Start();
				return 0;
			}
			else if(m_requestInfo != null) {
				rdtDebug.Error(m_requestInfo.m_enqueue, "File {0} is transfering", m_requestInfo.m_path);
			}
				
			else
				rdtDebug.Error("File is transfering");
			return -1;
		}

		private static void ThreadFunc()
		{
			if (m_requestInfo == null || m_requestInfo.m_enqueue == null || string.IsNullOrEmpty(m_requestInfo.m_path))
				return;
			using (FileStream stream = new FileStream(m_requestInfo.m_path, FileMode.Open))
			{
				byte[] bytes = new byte[FileBlockMaxSize];
				int offset = 0;
				int length = 0;
				while (m_run)
				{
					if (!m_isSending)
					{
						length = stream.Read(bytes, 0, FileBlockMaxSize);
						if (length <= 0)
						{
							m_run = false;
							break;
						}
                        rdtTcpMessageTransferFile ret = default(rdtTcpMessageTransferFile);
						ret.m_hash = m_requestInfo.m_hash;
						ret.m_fileLength = m_requestInfo.m_fileLength;
						ret.m_offset = offset;
						ret.m_length = length;
						ret.m_bytes = bytes;
						m_isSending = true;
						m_requestInfo.m_enqueue.EnqueueMessage(ret);
						offset += length;
						if (m_requestInfo.m_progressAction != null)
							m_requestInfo.m_progressAction((float) offset / m_requestInfo.m_fileLength);
						Thread.Sleep(20);
						if (offset >= m_requestInfo.m_fileLength)
							m_run = false;
					}
                }
				if(m_requestInfo.m_progressAction!=null)
					m_requestInfo.m_progressAction(-1);
                rdtDebug.Debug("Finishing transfer file thread");
            }
		}

		public static void ClearTransferFile()
		{
			m_run = false;
			m_isSending = false;
		}
	}
}
