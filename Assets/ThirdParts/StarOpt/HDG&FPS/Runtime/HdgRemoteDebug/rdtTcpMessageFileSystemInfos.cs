using System.Collections.Generic;
using System.IO;

namespace GameDLL.Hdg
{
    public struct rdtTcpMessageFileSystemInfos : rdtTcpMessage
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
			public string m_name;

            public int m_hash;

            public int m_attributes;

            public int m_parentHash;

			//public int m_scene;

			public FileAttributes fileAttributes
			{
				get
				{
                    return (FileAttributes)m_attributes;
                }
			}

			public bool editable
			{
				get
                {
					return !fileAttributes.HasFlag(FileAttributes.ReadOnly);

				}
            }

            public bool canBeParent
            {
                get
                {
                    return isDirectory;
                }
            }

            public bool isDirectory
            {
                get
                {
                    return fileAttributes.HasFlag(FileAttributes.Directory);
                }
            }


            public override string ToString()
			{
				string name = m_name;
				//if (m_instanceId != m_scene && (rdtDebug.s_logLevel == rdtDebug.LogLevel.Debug || rdtDebug.s_showInstanceID))
				//	name = name + ":" + m_instanceId;
				return name;
			}

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                Gob o = (Gob)obj;
                return m_hash == o.m_hash;
            }

            public override int GetHashCode()
            {
                return m_hash;
            }

            public void Write(BinaryWriter w)
            {
                w.Write(m_name);
                w.Write(m_hash);
                w.Write(m_attributes);
                w.Write(m_parentHash);
            }

            public void Read(BinaryReader r)
            {
                m_name = r.ReadString();
                m_hash = r.ReadInt32();
                m_attributes = r.ReadInt32();
                m_parentHash = r.ReadInt32();
            }
        }
    }
}
