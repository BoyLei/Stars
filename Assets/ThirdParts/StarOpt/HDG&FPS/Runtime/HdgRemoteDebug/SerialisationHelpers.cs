using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GameDLL.Hdg
{
    internal static class SerialisationHelpers
	{
		public enum ArrayElementType
		{
			Primitive,
			UserStruct,
			SerialiserInterface
		}

		private enum PrimitiveType
		{
			Byte,
			SByte,
			Int,
			UInt,
			Short,
			UShort,
			Long,
			ULong,
			Float,
			Double,
			Char,
			Bool,
			String,
			Decimal,
            Enum,
            Null
		}

		private static Type[] PrimitiveTypes;

		static SerialisationHelpers()
		{
			PrimitiveTypes = new Type[16]
			{
				typeof(byte),
				typeof(sbyte),
				typeof(int),
				typeof(uint),
				typeof(short),
				typeof(ushort),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(char),
				typeof(bool),
				typeof(string),
				typeof(decimal),
                typeof(Enum),		//枚举类型统合
				null,
			};
		}

		public static void WriteList(BinaryWriter bw, IList list, ArrayElementType type)
		{
			bw.Write((int)type);
			if (list == null || list.Count == 0)
			{
				bw.Write(0);
				return;
			}
			int num = list.Count;
			bw.Write(num);
			switch (type)
			{
			case ArrayElementType.Primitive:
				WritePrimitiveList(bw, list);
				return;
			case ArrayElementType.UserStruct:
				WriteUserStructList(bw, list);
				return;
			case ArrayElementType.SerialiserInterface:
				WriteSerialiserList(bw, list);
				return;
			default:
				return;
			}
		}

		static void WriteByteList(BinaryWriter bw, IList array, int num)
        {
			for (int i = 0; i < num; i++)
				bw.Write((byte)array[i]);
		}

		public static void WritePrimitiveList(BinaryWriter bw, IList array)
		{
			int num = array.Count;
			Type elementType = array.GetType().GetListElementType();
			if (elementType.IsEnum)
			{
				bw.Write((int)PrimitiveType.Enum);
				bw.Write(elementType.FullName);
				for (int i = 0; i < num; i++)
					WriteEnum(bw, elementType, array[i]);
				return;
			}
			if (elementType == typeof(byte))
			{
				bw.Write((int)PrimitiveType.Byte);
				for (int i = 0; i < num; i++)
					bw.Write((byte)array[i]);
				return;
			}
			if (elementType == typeof(sbyte))
			{
				bw.Write((int)PrimitiveType.SByte);
				for (int i = 0; i < num; i++)
					bw.Write((sbyte)array[i]);
				return;
			}
			if (elementType == typeof(int))
			{
				bw.Write((int)PrimitiveType.Int);
				for (int i = 0; i < num; i++)
					bw.Write((int)array[i]);
				return;
			}
			if (elementType == typeof(uint))
			{
				bw.Write((int)PrimitiveType.UInt);
				for (int i = 0; i < num; i++)
					bw.Write((uint)array[i]);
				return;
			}
			if (elementType == typeof(short))
			{
				bw.Write((int)PrimitiveType.Short);
				for (int i = 0; i < num; i++)
					bw.Write((short)array[i]);
				return;
			}
			if (elementType == typeof(ushort))
			{
				bw.Write((int)PrimitiveType.UShort);
				for (int i = 0; i < num; i++)
					bw.Write((ushort)array[i]);
				return;
			}
			if (elementType == typeof(long))
			{
				bw.Write((int)PrimitiveType.Long);
				for (int i = 0; i < num; i++)
					bw.Write((long)array[i]);
				return;
			}
			if (elementType == typeof(ulong))
			{
				bw.Write((int)PrimitiveType.ULong);
				for (int i = 0; i < num; i++)
					bw.Write((ulong)array[i]);
				return;
			}
			if (elementType == typeof(float))
			{
				bw.Write((int)PrimitiveType.Float);
				for (int i = 0; i < num; i++)
					bw.Write((float)array[i]);
				return;
			}
			if (elementType == typeof(double))
			{
				bw.Write((int)PrimitiveType.Double);
				for (int i = 0; i < num; i++)
					bw.Write((double)array[i]);
				return;
			}
			if (elementType == typeof(char))
			{
				bw.Write((int)PrimitiveType.Char);
				for (int i = 0; i < num; i++)
					bw.Write((char)array[i]);
				return;
			}
			if (elementType == typeof(bool))
			{
				bw.Write((int)PrimitiveType.Bool);
				for (int i = 0; i < num; i++)
					bw.Write((bool)array[i]);
				return;
			}
			if (elementType == typeof(string))
			{
				bw.Write((int)PrimitiveType.String);
				for (int i = 0; i < num; i++)
					bw.Write((string)array[i]);
				return;
			}
			if (elementType == typeof(decimal))
			{
				bw.Write((int)PrimitiveType.Decimal);
				for (int i = 0; i < num; i++)
					bw.Write((decimal)array[i]);
				return;
			}
			RemoteDebugServer.Instance.SerializerRegistry.AddUnknownPrimitive(elementType);
			bw.Write((int)PrimitiveType.Null);
		}

		public static void WriteSerialiserList(BinaryWriter bw, IList array)
		{
			string fullName = array[0].GetType().FullName;
			bw.Write(fullName);
			for (int i = 0; i < array.Count; i++)
			{
				(array[i] as rdtSerializerInterface).Write(bw);
			}
		}

		public static void WriteUserStructList(BinaryWriter bw, IList array)
		{
			for (int i = 0; i < array.Count; i++)
			{
				List<rdtTcpMessageComponents.Property> subProperties = array[i] as List<rdtTcpMessageComponents.Property>;
				rdtTcpMessageComponents.Component.WriteProperties(bw, subProperties);
			}
		}

		static void WriteEnum(BinaryWriter bw, Type type, object value)
        {
			type = Enum.GetUnderlyingType(type);
			bw.Write(Convert.ToInt32(Convert.ChangeType(value, type)));
		}

		public static void WritePrimitive(BinaryWriter bw, object value)
		{
			if (value == null)
			{
				bw.Write((int)PrimitiveType.Null);
				return;
			}
			Type type = value.GetType();
			if (type.IsEnum)
			{
				bw.Write((int)PrimitiveType.Enum);
				bw.Write(type.FullName);
				WriteEnum(bw, type, value);
				return;
			}
			if (type == typeof(byte))
			{
				bw.Write((int)PrimitiveType.Byte);
				bw.Write((byte)value);
				return;
			}
			if (type == typeof(sbyte))
			{
				bw.Write((int)PrimitiveType.SByte);
				bw.Write((sbyte)value);
				return;
			}
			if (type == typeof(int))
			{
				bw.Write((int)PrimitiveType.Int);
				bw.Write((int)value);
				return;
			}
			if (type == typeof(uint))
			{
				bw.Write((int)PrimitiveType.UInt);
				bw.Write((uint)value);
				return;
			}
			if (type == typeof(short))
			{
				bw.Write((int)PrimitiveType.Short);
				bw.Write((short)value);
				return;
			}
			if (type == typeof(ushort))
			{
				bw.Write((int)PrimitiveType.UShort);
				bw.Write((ushort)value);
				return;
			}
			if (type == typeof(long))
			{
				bw.Write((int)PrimitiveType.Long);
				bw.Write((long)value);
				return;
			}
			if (type == typeof(ulong))
			{
				bw.Write((int)PrimitiveType.ULong);
				bw.Write((ulong)value);
				return;
			}
			if (type == typeof(float))
			{
				bw.Write((int)PrimitiveType.Float);
				bw.Write((float)value);
				return;
			}
			if (type == typeof(double))
			{
				bw.Write((int)PrimitiveType.Double);
				bw.Write((double)value);
				return;
			}
			if (type == typeof(char))
			{
				bw.Write((int)PrimitiveType.Char);
				bw.Write((char)value);
				return;
			}
			if (type == typeof(bool))
			{
				bw.Write((int)PrimitiveType.Bool);
				bw.Write((bool)value);
				return;
			}
			if (type == typeof(string))
			{
				bw.Write((int)PrimitiveType.String);
				bw.Write((string)value);
				return;
			}
			if (type == typeof(decimal))
			{
				bw.Write((int)PrimitiveType.Decimal);
				bw.Write((decimal)value);
				return;
			}
			RemoteDebugServer.Instance.SerializerRegistry.AddUnknownPrimitive(type);
			bw.Write((int)PrimitiveType.Null);
		}

		public static void ReadList(BinaryReader br, out IList list, out ArrayElementType type)
		{
			type = (ArrayElementType)br.ReadInt32();
			int count = br.ReadInt32();
			list = null;
			if (count == 0)
			{
				return;
			}
			switch (type)
			{
			case ArrayElementType.Primitive:
				list = ReadPrimitiveArray(br, count);
				return;
			case ArrayElementType.UserStruct:
				list = ReadUserStructArray(br, count);
				return;
			case ArrayElementType.SerialiserInterface:
				list = ReadSerialiserArray(br, count);
				return;
			default:
				return;
			}
		}

		public static Array ReadPrimitiveArray(BinaryReader r, int count)
		{
			PrimitiveType primitiveType = (PrimitiveType)r.ReadInt32();
			Array array = Array.CreateInstance(typeof(object), count);
			ReadPrimitives(r, array, count, primitiveType);
			return array;
		}

		public static IList ReadPrimitiveList(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num == 0)
			{
				return null;
			}
			PrimitiveType primitiveType = (PrimitiveType)r.ReadInt32();
			Type type = PrimitiveTypes[(int)primitiveType];
			IList list = (IList)typeof(List<>).MakeGenericType(new Type[]
			{
				type
			}).GetConstructor(Type.EmptyTypes).Invoke(null);
			ReadPrimitives(r, list, num, primitiveType);
			return list;
		}

		public static IList ReadSerialiserArray(BinaryReader br, int count)
		{
			Type type = Type.GetType(br.ReadString());
			IList array = new object[count];
			for (int i = 0; i < count; i++)
			{
				rdtSerializerInterface s = Activator.CreateInstance(type) as rdtSerializerInterface;
				s.Read(br);
				array[i] = s;
			}
			return array;
		}

		public static IList ReadUserStructArray(BinaryReader br, int count)
		{
			List<rdtTcpMessageComponents.Property>[] array = new List<rdtTcpMessageComponents.Property>[count];
			for (int i = 0; i < count; i++)
				array[i] = rdtTcpMessageComponents.Component.ReadProperties(br);
			return array;
		}

		private static void ReadPrimitives(BinaryReader r, IList array, int count, PrimitiveType primitiveType)
		{
			switch(primitiveType)
			{
				case PrimitiveType.Enum:
					Type enumType = Type.GetType(r.ReadString());
					for (int i = 0; i < count; i++)
					{
						if (enumType != null)
						{
							if (array.IsFixedSize)
								array[i] = ReadEnum(r, enumType);
							else
								array.Add(ReadEnum(r, enumType));
						}
						else
						{
							int value = r.ReadInt32();
							if (array.IsFixedSize)
								array[i] = value;
							else
								array.Add(value);
						}
					}
					break;
				case PrimitiveType.Byte:
					for (int i = 0; i < count; i++)
					{
						byte value = r.ReadByte();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.SByte:
					for (int i = 0; i < count; i++)
					{
						sbyte value = r.ReadSByte();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.Int:
					for (int i = 0; i < count; i++)
					{
						int value = r.ReadInt32();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.UInt:
					for (int i = 0; i < count; i++)
					{
						uint value = r.ReadUInt32();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.Short:
					for (int i = 0; i < count; i++)
					{
						short value = r.ReadInt16();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.UShort:
					for (int i = 0; i < count; i++)
					{
						ushort value = r.ReadUInt16();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.Long:
					for (int i = 0; i < count; i++)
					{
						long value = r.ReadInt64();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.ULong:
					for (int i = 0; i < count; i++)
					{
						ulong value = r.ReadUInt64();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.Float:
					for (int i = 0; i < count; i++)
					{
						float value = r.ReadSingle();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.Double:
					for (int i = 0; i < count; i++)
					{
						double value = r.ReadDouble();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.Char:
					for (int i = 0; i < count; i++)
					{
						char value = r.ReadChar();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.Bool:
					for (int i = 0; i < count; i++)
					{
						bool value = r.ReadBoolean();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.String:
					for (int i = 0; i < count; i++)
					{
						string value = r.ReadString();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
				case PrimitiveType.Decimal:
					for (int i = 0; i < count; i++)
					{
						decimal value = r.ReadDecimal();
						if (array.IsFixedSize)
							array[i] = value;
						else
							array.Add(value);
					}
					return;
			}
		}

		static object ReadEnum(BinaryReader r, Type type)
        {
			object value = r.ReadInt32();
			if (type == null)
				return value;
			value = Convert.ChangeType(value, type.GetEnumUnderlyingType());
			return Enum.ToObject(type, value);
		}

		public static object ReadPrimitive(BinaryReader r)
		{
			switch ((PrimitiveType)r.ReadInt32())
			{
				case PrimitiveType.Byte:
					return r.ReadByte();
				case PrimitiveType.SByte:
                    return r.ReadSByte();
				case PrimitiveType.Int:
                    return r.ReadInt32();
				case PrimitiveType.UInt:
                    return r.ReadUInt32();
				case PrimitiveType.Short:
                    return r.ReadInt16();
				case PrimitiveType.UShort:
                    return r.ReadUInt16();
				case PrimitiveType.Long:
                    return r.ReadInt64();
				case PrimitiveType.ULong:
                    return r.ReadUInt64();
				case PrimitiveType.Float:
                    return r.ReadSingle();
				case PrimitiveType.Double:
                    return r.ReadDouble();
				case PrimitiveType.Char:
                    return r.ReadChar();
				case PrimitiveType.Bool:
                    return r.ReadBoolean();
				case PrimitiveType.String:
					return r.ReadString();
				case PrimitiveType.Decimal:
					return r.ReadDecimal();
				case PrimitiveType.Enum:
					Type type = GetType(r.ReadString());
					return ReadEnum(r, type);
			}
			return null;
		}

		static List<Assembly> assemblies;

		static Dictionary<string, Type> typeDic = new Dictionary<string, Type>();

		static void addAssemblieByName(IEnumerable<Assembly> assemblies_usorted, string name)
		{
			foreach (var assemblie in assemblies_usorted)
			{
				if (assemblie.FullName.StartsWith(name) && !assemblies.Contains(assemblie))
				{
					assemblies.Add(assemblie);
					break;
				}
			}
		}

		static void InitAssemblies()
        {
			if(assemblies == null)
            {
				assemblies = new List<Assembly>();

#if (UNITY_WSA && !ENABLE_IL2CPP) && !UNITY_EDITOR
				var assemblies_usorted = Utils.GetAssemblies();
#else
				assemblies.Add(Assembly.GetExecutingAssembly());
				var assemblies_usorted = AppDomain.CurrentDomain.GetAssemblies();
#endif
				addAssemblieByName(assemblies_usorted, "mscorlib,");
				addAssemblieByName(assemblies_usorted, "System,");
				addAssemblieByName(assemblies_usorted, "System.Core,");
				foreach (Assembly assembly in assemblies_usorted)
				{
					if (!assemblies.Contains(assembly))
					{
						assemblies.Add(assembly);
					}
				}
			}
        }

		static Type GetType(string className)
        {
			Type type;
			if (!typeDic.TryGetValue(className, out type))
			{
				InitAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					type = assembly.GetType(className);
					if (type != null)
					{
						typeDic.Add(className, type);
						return type;
					}
				}
			}
			return type;
		}
	}
}
