using System.IO;
using UnityEngine;

namespace GameDLL.Hdg
{
	public class rdtSerializerBoundsInt : rdtSerializerInterface
	{
		private rdtSerializerVector3Int position;

		private rdtSerializerVector3Int size;

		public rdtSerializerBoundsInt()
		{
		}

		public rdtSerializerBoundsInt(BoundsInt b)
		{
			position = new rdtSerializerVector3Int(b.position);
			size = new rdtSerializerVector3Int(b.size);
		}

		public BoundsInt ToUnityType()
		{
			return new BoundsInt(position.ToUnityType(), size.ToUnityType());
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			position.Write(w);
			size.Write(w);
		}

		public void Read(BinaryReader r)
		{
			position = new rdtSerializerVector3Int();
			position.Read(r);
			size = new rdtSerializerVector3Int();
			size.Read(r);
		}
	}
}
