using System;
using System.Runtime.Serialization;
namespace Meebey.SmartIrc4net
{
	[Serializable]
	public class ConnectionException : SmartIrc4netException
	{
		public ConnectionException()
		{
		}
		public ConnectionException(string message) : base(message)
		{
		}
		public ConnectionException(string message, Exception e) : base(message, e)
		{
		}
		protected ConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
