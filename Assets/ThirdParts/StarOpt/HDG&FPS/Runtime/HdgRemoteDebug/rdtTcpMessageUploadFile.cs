using System.IO;

namespace GameDLL.Hdg
{
    public struct rdtTcpMessageUploadFile : rdtTcpMessage
    {
        public int m_hash;
        public string m_name;
        public int m_fileLength;
        public bool m_overwrite;
        public void Write(BinaryWriter w)
        {
            w.Write(m_hash);
            w.Write(m_fileLength);
            w.Write(m_overwrite);
            w.Write(m_name);
        }
        public void Read(BinaryReader r)
        {
            m_hash = r.ReadInt32();
            m_fileLength = r.ReadInt32();
            m_overwrite = r.ReadBoolean();
            m_name = r.ReadString();
        }
    }
}
