using System.IO;
using UnityEngine;

namespace GameDLL.Hdg
{
	public class rdtSerializerVector2Int : rdtSerializerInterface
	{
		public int x;

		public int y;

		public rdtSerializerVector2Int()
		{
		}

		public rdtSerializerVector2Int(Vector2Int v)
		{
			x = v.x;
			y = v.y;
		}

		public Vector2Int ToUnityType()
		{
			return new Vector2Int(x, y);
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(x);
			w.Write(y);
		}

		public void Read(BinaryReader r)
		{
			x = r.ReadInt32();
			y = r.ReadInt32();
		}
	}
}
