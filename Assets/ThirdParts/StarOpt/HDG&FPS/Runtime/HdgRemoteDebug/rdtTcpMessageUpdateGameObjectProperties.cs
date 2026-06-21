using System;
using System.IO;

namespace GameDLL.Hdg
{
	public struct rdtTcpMessageUpdateGameObjectProperties : rdtTcpMessage
	{
		[Flags]
		public enum Flags
		{
			UpdateEnabled = 1,
			UpdateTag = 2,
			UpdateLayer = 4,
			UpdateFlags = 8,
		}

		public int m_instanceId;

		public Flags m_flags;

		public bool m_enabled;

		public string m_tag;

		public int m_layer;

		public byte m_hideFlags;

		public void Write(BinaryWriter w)
		{
			w.Write(m_instanceId);
			w.Write(m_enabled);
			w.Write(m_tag);
			w.Write(m_layer);
			w.Write(m_hideFlags);
			w.Write((int)m_flags);
		}

		public void Read(BinaryReader r)
		{
			m_instanceId = r.ReadInt32();
			m_enabled = r.ReadBoolean();
			m_tag = r.ReadString();
			m_layer = r.ReadInt32();
			m_hideFlags = r.ReadByte();
			m_flags = (Flags)r.ReadInt32();
		}

		public void SetFlag(Flags flag, bool enabled)
		{
			if (enabled)
				m_flags |= flag;
			else
				m_flags &= ~flag;
		}

		public bool HasFlag(Flags flag)
		{
			return (m_flags & flag) > 0;
		}
	}
}
