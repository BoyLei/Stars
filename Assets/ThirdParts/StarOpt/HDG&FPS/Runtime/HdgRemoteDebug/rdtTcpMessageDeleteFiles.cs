using System.Collections.Generic;
using System.IO;

namespace GameDLL.Hdg
{
    public struct rdtTcpMessageDeleteFiles : rdtTcpMessage
	{
		public List<int> m_hashs;

		public void Write(BinaryWriter w)
		{
			int count = m_hashs.Count;
			w.Write(count);
			for (int i = 0; i < count; i++)
				w.Write(m_hashs[i]);
		}

		public void Read(BinaryReader r)
		{
			int count = r.ReadInt32();
			m_hashs = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				int id = r.ReadInt32();
				m_hashs.Add(id);
			}
		}
	}
}
