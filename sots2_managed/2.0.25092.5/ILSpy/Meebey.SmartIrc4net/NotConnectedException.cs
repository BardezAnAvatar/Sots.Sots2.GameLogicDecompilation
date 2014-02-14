using System;
using System.Runtime.Serialization;
namespace Meebey.SmartIrc4net
{
	[Serializable]
	public class NotConnectedException : ConnectionException
	{
		public NotConnectedException()
		{
		}
		public NotConnectedException(string message) : base(message)
		{
		}
		public NotConnectedException(string message, Exception e) : base(message, e)
		{
		}
		protected NotConnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
