using System.IO;

namespace GameDLL.Hdg
{
    public struct rdtTcpMessageReturnResult : rdtTcpMessage
    {
        public const string RETURN_SUCCESS = "success";
        public const string RETURN_FAIL = "fail";


        public string m_operation;
        public rdtTcpMessageComponents.Property m_result;
        public void Read(BinaryReader r)
        {
            m_operation = r.ReadString();
            m_result.Read(r);
        }

        public void Write(BinaryWriter w)
        {
            w.Write(m_operation);
            m_result.Write(w);
        }
    }
}
