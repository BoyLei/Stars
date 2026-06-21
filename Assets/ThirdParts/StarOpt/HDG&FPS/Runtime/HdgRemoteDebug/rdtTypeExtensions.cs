using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameDLL.Hdg
{
	public static class rdtTypeExtensions
	{
		private static List<FieldInfo> s_fields = new List<FieldInfo>(256);

		public static bool IsUserStruct(this Type type)
		{
			return !type.IsPrimitive && !type.IsEnum && type.IsValueType;
		}

		public static bool IsGenericList(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
		}

		public static bool IsReference(this Type type)
		{
			return !type.IsValueType && type != typeof(string) && !type.IsArray && !type.IsGenericList();
		}

		public static Type GetListElementType(this Type type)
		{
			if (type.IsArray)
				return type.GetElementType();
			if (!type.IsGenericList())
				return null;
			return type.GetGenericArguments()[0];
		}

		public static List<FieldInfo> GetAllFields(this Type t)
		{
			s_fields.Clear();
			GetAllFieldsImp(t);
			return s_fields;
		}

		public static FieldInfo GetFieldInHierarchy(this Type t, string name)
		{
			if (t == null)
				return null;
			BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = t.GetField(name, flags);
			if (field == null)
				field = t.BaseType.GetFieldInHierarchy(name);
			return field;
		}

		private static void GetAllFieldsImp(Type t)
		{
			if (t == null)
				return;
			BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo[] fields = t.GetFields(flags);
			s_fields.AddRange(fields);
			GetAllFieldsImp(t.BaseType);
		}
	}
}
