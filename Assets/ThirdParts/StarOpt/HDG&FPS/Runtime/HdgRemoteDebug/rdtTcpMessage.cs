using System;
using System.IO;

namespace GameDLL.Hdg
{
	public interface rdtTcpMessage
	{
		void Write(BinaryWriter w);

		void Read(BinaryReader r);
	}

	public interface rdtTcpEnqueue
    {
		void EnqueueMessage(rdtTcpMessage message);

	}

	public interface IModifiable
    {
		bool editable { get; }

		bool canBeParent { get; }
	}
}
