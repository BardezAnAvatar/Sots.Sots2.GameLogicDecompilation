using System;
namespace Kerberos.Sots.Engine
{
	public interface IMessageQueue
	{
		int IncomingCapacity
		{
			get;
		}
		int OutgoingCapacity
		{
			get;
		}
		int IncomingSize
		{
			get;
		}
		int OutgoingSize
		{
			get;
		}
		void PrepareIncoming();
		void PrepareOutgoing();
		void Update();
		int GetNextMessage(byte[] data);
		int GetNextMessageSize();
		int GetNextMessageData(byte[] data, int size);
		void PutMessage(byte[] data, int count);
	}
}
