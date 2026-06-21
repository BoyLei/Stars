using System;
using System.Collections.Generic;
using System.IO;

namespace GameDLL.Hdg
{
    public struct rdtTcpMessageSetParent : rdtTcpMessage
    {
		public int m_parentId;

		public int m_startIndex;

		public List<int> m_childrenIds;

		public void Write(BinaryWriter w)
		{
			w.Write(m_parentId);
			w.Write(m_startIndex);
			int num = m_childrenIds.Count;
			w.Write(num);
			for (int i = 0; i < num; i++)
				w.Write(m_childrenIds[i]);
		}

		public void Read(BinaryReader r)
		{
			m_parentId = r.ReadInt32();
			m_startIndex = r.ReadInt32();
			m_childrenIds = new List<int>();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
				m_childrenIds.Add(r.ReadInt32());
		}

	}
}
