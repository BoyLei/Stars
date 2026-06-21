using System;
using System.Collections.Generic;
using System.IO;

namespace GameDLL.Hdg
{
	public struct rdtTcpMessageComponents : rdtTcpMessage
	{
		public List<Component> m_components;

		public int m_layer;

		public string m_tag;

		public byte m_hideFlags;

		public bool m_enabled;

		public int m_instanceId;

		static List<Component> s_writeList = new List<Component>();

		public void Write(BinaryWriter w)
		{
			//写入时缓存当前的数据，避免同时出现Read的情况List发生变化
			s_writeList.Clear();
			s_writeList.AddRange(m_components);

			int num = s_writeList.Count;
			w.Write(num);
			for (int i = 0; i < num; i++)
				s_writeList[i].Write(w);
			w.Write(m_layer);
			w.Write(m_tag);
			w.Write(m_enabled);
			w.Write(m_instanceId);
			w.Write(m_hideFlags);
		}

		public void Read(BinaryReader r)
		{
			m_components = new List<Component>();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Component c = default(Component);
				c.Read(r);
				m_components.Add(c);
			}
			m_layer = r.ReadInt32();
			m_tag = r.ReadString();
			m_enabled = r.ReadBoolean();
			m_instanceId = r.ReadInt32();
			m_hideFlags = r.ReadByte();
		}

		public struct Property
		{
			public enum Type
			{
				Field,
				Property,
				Method
			}

			public string m_name;

			public object m_value;

			public Type m_type;

			public bool m_isArray;

			public void Deserialise(rdtSerializerRegistry registry)
			{
				List<Property> subProperties = m_value as List<Property>;
				if (subProperties != null)
				{
					for (int i = 0; i < subProperties.Count; i++)
					{
						Property p = subProperties[i];
						p.Deserialise(registry);
						subProperties[i] = p;
					}
					return;
				}
				m_value = registry.Deserialize(m_value);
			}

			public void Write(BinaryWriter w)
			{
				w.Write(m_name);
				w.Write((int)m_type);
				w.Write(m_isArray);
				if (m_value == null)
				{
					w.Write(true);
					return;
				}
				w.Write(false);
				List<Property> subProperties = m_value as List<Property>;
				w.Write(subProperties != null);
				if (subProperties != null)
				{
					Component.WriteProperties(w, subProperties);
					return;
				}
				System.Type type = m_value.GetType();
				System.Type stringType = typeof(string);
				bool isPrimitive = type.IsPrimitive || type.IsAssignableFrom(stringType) || type.IsEnum;
				w.Write(isPrimitive);
				if (isPrimitive)
				{
					SerialisationHelpers.WritePrimitive(w, m_value);
					return;
				}
				rdtSerializerInterface rdtSerializerInterface = m_value as rdtSerializerInterface;
				string fullName = rdtSerializerInterface.GetType().FullName;
				w.Write(fullName);
				rdtSerializerInterface.Write(w);
			}

			public void Read(BinaryReader r)
			{
				m_name = r.ReadString();
				m_type = (Type)r.ReadInt32();
				m_isArray = r.ReadBoolean();
				if (r.ReadBoolean())
					return;
				if (r.ReadBoolean())
				{
					m_value = Component.ReadProperties(r);
					return;
				}
				if (r.ReadBoolean())
				{
					m_value = SerialisationHelpers.ReadPrimitive(r);
					return;
				}
				rdtSerializerInterface s = Activator.CreateInstance(System.Type.GetType(r.ReadString())) as rdtSerializerInterface;
				s.Read(r);
				m_value = s;
			}

			public override string ToString()
			{
				return string.Format("{0} = {1}", m_name, m_value);
			}

			public Property Clone()
			{
				return new Property
				{
					m_name = m_name,
					m_type = m_type,
					m_isArray = m_isArray
				};
			}
		}

		public struct Component
		{
			public bool m_canBeDisabled;

			public bool m_enabled;

			public string m_name;

			public string m_assemblyName;

			public int m_instanceId;

			public List<Property> m_properties;

			public void Write(BinaryWriter w)
			{
				w.Write(m_canBeDisabled);
				w.Write(m_enabled);
				w.Write(m_name);
				w.Write(m_assemblyName);
				w.Write(m_instanceId);
				WriteProperties(w, m_properties);
			}

			public static void WriteProperties(BinaryWriter w, List<Property> properties)
			{
				int num = (properties != null) ? properties.Count : 0;
				w.Write(num);
				for (int i = 0; i < num; i++)
					properties[i].Write(w);
			}

			public void Read(BinaryReader r)
			{
				m_canBeDisabled = r.ReadBoolean();
				m_enabled = r.ReadBoolean();
				m_name = r.ReadString();
				m_assemblyName = r.ReadString();
				m_instanceId = r.ReadInt32();
				m_properties = ReadProperties(r);
			}

			public static List<Property> ReadProperties(BinaryReader r)
			{
				List<Property> properties = new List<Property>();
				int num = r.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					Property p = default(Property);
					p.Read(r);
					properties.Add(p);
				}
				return properties;
			}
			
			public override string ToString()
			{
				return string.Format("Component {0}", m_name);
			}
		}
	}
}
