using System;
using System.IO;

namespace GameDLL.Hdg
{
	public interface rdtSerializerInterface
	{
		object Deserialize(rdtSerializerRegistry registry);

		void Write(BinaryWriter w);

		void Read(BinaryReader r);
	}
}
