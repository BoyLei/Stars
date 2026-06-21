using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameDLL.Hdg
{
    public struct rdtTcpMessageMoveFiles : rdtTcpMessage
    {
        public int m_directorHash;

        public List<int> m_childrenHashs;

		public void Write(BinaryWriter w)
		{
			w.Write(m_directorHash);
			int num = m_childrenHashs.Count;
			w.Write(num);
			for (int i = 0; i < num; i++)
				w.Write(m_childrenHashs[i]);
		}

		public void Read(BinaryReader r)
		{
			m_directorHash = r.ReadInt32();
			m_childrenHashs = new List<int>();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
				m_childrenHashs.Add(r.ReadInt32());
		}

	}
}
