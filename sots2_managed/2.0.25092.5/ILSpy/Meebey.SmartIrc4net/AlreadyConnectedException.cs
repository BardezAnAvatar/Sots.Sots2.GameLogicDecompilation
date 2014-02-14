using System;
using System.Runtime.Serialization;
namespace Meebey.SmartIrc4net
{
	[Serializable]
	public class AlreadyConnectedException : ConnectionException
	{
		public AlreadyConnectedException()
		{
		}
		public AlreadyConnectedException(string message) : base(message)
		{
		}
		public AlreadyConnectedException(string message, Exception e) : base(message, e)
		{
		}
		protected AlreadyConnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
