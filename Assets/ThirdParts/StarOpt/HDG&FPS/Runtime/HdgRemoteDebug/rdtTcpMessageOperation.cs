using System.Collections.Generic;
using System.IO;

namespace GameDLL.Hdg
{
	public struct rdtTcpMessageOperation : rdtTcpMessage
	{
		public string m_operation;

		public List<rdtTcpMessageComponents.Property> m_properties;

		public void Read(BinaryReader r)
		{
			m_operation = r.ReadString();
			m_properties = rdtTcpMessageComponents.Component.ReadProperties(r);
		}

		public void Write(BinaryWriter w)
		{
			w.Write(m_operation);
			rdtTcpMessageComponents.Component.WriteProperties(w, m_properties);
		}

		public object[] GetParameters()
        {
			object[] parameters = null;
			if(m_properties != null && m_properties.Count > 0)
            {
				parameters = new object[m_properties.Count];
				for (int i = 0; i < parameters.Length; i++)
					parameters[i] = m_properties[i].m_value;
			}
			return parameters;

		}
	}
}
