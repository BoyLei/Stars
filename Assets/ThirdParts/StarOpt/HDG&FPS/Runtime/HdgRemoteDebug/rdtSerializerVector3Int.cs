using System.IO;
using UnityEngine;

namespace GameDLL.Hdg
{
	public class rdtSerializerVector3Int : rdtSerializerInterface
	{
		public int x;

		public int y;

		public int z;

		public rdtSerializerVector3Int()
		{
		}

		public rdtSerializerVector3Int(Vector3Int v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}

		public Vector3Int ToUnityType()
		{
			return new Vector3Int(x, y, z);
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(x);
			w.Write(y);
			w.Write(z);
		}

		public void Read(BinaryReader r)
		{
			x = r.ReadInt32();
			y = r.ReadInt32();
			z = r.ReadInt32();
		}
	}
}
