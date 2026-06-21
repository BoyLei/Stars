using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameDLL.Hdg
{
	public struct rdtTcpMessageGameObjects : rdtTcpMessage
	{
		public List<Gob> m_allGobs;

		public void Write(BinaryWriter w)
		{
			int num = m_allGobs.Count;
			w.Write(num);
			for (int i = 0; i < num; i++)
				m_allGobs[i].Write(w);
		}

		public void Read(BinaryReader r)
		{
			m_allGobs = new List<Gob>();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Gob gob = default(Gob);
				gob.Read(r);
				m_allGobs.Add(gob);
			}
		}

		public struct Gob : IModifiable
		{
			public bool m_enabled;

			public string m_name;

			public int m_instanceId;

			public byte m_hideFlags;

			//public bool m_hasParent;

			public int m_parentInstanceId;

			//public string m_scene;
			public int m_scene;

            public HideFlags hideFlags
            {
				get
                {
					return (HideFlags)m_hideFlags;
				}
            }

			public bool editable
            {
				get
                {
					return (hideFlags & HideFlags.NotEditable) == 0;
				}
            }

			public bool canBeParent
			{
				get
				{
					return true;
				}
			}

			public override string ToString()
			{
				string name = m_name;
				if (m_instanceId != m_scene && (rdtDebug.s_logLevel == rdtDebug.LogLevel.Debug || rdtDebug.s_showInstanceID))
					name = name + ":" + m_instanceId;
				return name;
			}

			public override bool Equals(object obj)
			{
				Gob o = (Gob)obj;
				return m_instanceId == o.m_instanceId;
			}

			public override int GetHashCode()
			{
				return m_instanceId;
			}

			public void Write(BinaryWriter w)
			{
				w.Write(m_enabled);
				w.Write(m_name);
				w.Write(m_instanceId);
				w.Write(m_hideFlags);
				//w.Write(m_hasParent);
				w.Write(m_parentInstanceId);
				w.Write(m_scene);
			}

			public void Read(BinaryReader r)
			{
				m_enabled = r.ReadBoolean();
				m_name = r.ReadString();
				m_instanceId = r.ReadInt32();
				m_hideFlags = r.ReadByte();
				//m_hasParent = r.ReadBoolean();
				m_parentInstanceId = r.ReadInt32();
				//m_scene = r.ReadString();
				m_scene = r.ReadInt32();
			}
		}
	}
}
