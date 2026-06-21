using System.IO;

namespace GameDLL.Hdg
{
    public struct rdtTcpMessageRenameFile : rdtTcpMessage
    {
        public int m_hash;

        public string m_name;

        public void Write(BinaryWriter w)
        {
            w.Write(m_hash);
            w.Write(m_name);
        }

        public void Read(BinaryReader r)
        {
            m_hash = r.ReadInt32();
            m_name = r.ReadString();
        }
    }
}
