using System.IO;

namespace GameDLL.Hdg
{
    public struct rdtTcpMessageDownloadFile : rdtTcpMessage
    {
        public int m_hash;

        public void Write(BinaryWriter w)
        {
            w.Write(m_hash);
        }
        public void Read(BinaryReader r)
        {
            m_hash = r.ReadInt32();
        }
    }
}
