using System.IO;
using UnityEngine;

namespace GameDLL.Hdg
{
	public class rdtSerializerRectInt : rdtSerializerInterface
	{
		private int x;

		private int y;

		private int width;

		private int height;

		public rdtSerializerRectInt()
		{
		}

		public rdtSerializerRectInt(RectInt r)
		{
			x = r.x;
			y = r.y;
			width = r.width;
			height = r.height;
		}

		public RectInt ToUnityType()
		{
			return new RectInt(x, y, width, height);
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(x);
			w.Write(y);
			w.Write(width);
			w.Write(height);
		}

		public void Read(BinaryReader r)
		{
			x = r.ReadInt32();
			y = r.ReadInt32();
			width = r.ReadInt32();
			height = r.ReadInt32();
		}
	}
}
