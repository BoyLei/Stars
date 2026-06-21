using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GameDLL.Hdg
{
	public class rdtSerializerContainerArray : rdtSerializerInterface
	{
		private enum ListType
		{
			Array,
			List
		}

		private ListType m_listType;

		private IList m_array;

		private SerialisationHelpers.ArrayElementType m_arrayElementType;


		public static object Serialize(object objIn, rdtSerializerRegistry registry)
		{
			rdtSerializerContainerArray rdtSerializerContainerArray = new rdtSerializerContainerArray();
			IList array = (IList)objIn;
			rdtSerializerContainerArray.SerializeImp(array, registry);
			return rdtSerializerContainerArray;
		}

		private void SerializeImp(IList array, rdtSerializerRegistry registry)
		{
			Type type = array.GetType();
			Type elementType = type.GetListElementType();
			bool isSerialisable = elementType.IsSerializable;
			m_listType = (type.IsGenericList() ? ListType.List : ListType.Array);
			if (!isSerialisable || elementType == typeof(object) || elementType.IsUserStruct() || elementType.IsReference())
			{
				int count = array.Count;
				m_array = new object[count];
				for (int i = 0; i < count; i++)
				{
					object element = array[i];
					element = registry.Serialize(element);
					m_array[i] = element;
				}
				m_arrayElementType = ((m_array.Count > 0 && m_array[0] is rdtSerializerInterface) ? SerialisationHelpers.ArrayElementType.SerialiserInterface : SerialisationHelpers.ArrayElementType.UserStruct);
				return;
			}
			m_array = array;
			m_arrayElementType = SerialisationHelpers.ArrayElementType.Primitive;
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			if (m_array == null || m_array.Count == 0)
				return null;
			Type elementType = registry.Deserialize(m_array[0]).GetType();
			return DeserializeArray(elementType, registry);
		}

		public object DeserializeArray(Type elementType, rdtSerializerRegistry registry)
		{
			object obj = null;
			ListType listType = m_listType;
			if (listType != ListType.Array)
			{
				if (listType == ListType.List)
				{
					IList newList = (IList)typeof(List<>).MakeGenericType(new Type[]
					{
						elementType
					}).GetConstructor(Type.EmptyTypes).Invoke(null);
					for (int i = 0; i < m_array.Count; i++)
					{
						object value = registry.Deserialize(m_array[i]);
						newList.Add(value);
					}
					obj = newList;
				}
			}
			else
			{
				Array newArray = Array.CreateInstance(elementType, m_array.Count);
				for (int j = 0; j < m_array.Count; j++)
				{
					object value2 = registry.Deserialize(m_array[j]);
					newArray.SetValue(value2, j);
				}
				obj = newArray;
			}
			return obj;
		}

		public void Write(BinaryWriter w)
		{
			w.Write((int)m_listType);
			SerialisationHelpers.WriteList(w, m_array, m_arrayElementType);
		}

		public void Read(BinaryReader r)
		{
			m_listType = (ListType)r.ReadInt32();
			SerialisationHelpers.ReadList(r, out m_array, out m_arrayElementType);
		}
	}
}
