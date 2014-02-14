using System;
using System.Runtime.Serialization;
namespace Meebey.SmartIrc4net
{
	[Serializable]
	public class SmartIrc4netException : ApplicationException
	{
		public SmartIrc4netException()
		{
		}
		public SmartIrc4netException(string message) : base(message)
		{
		}
		public SmartIrc4netException(string message, Exception e) : base(message, e)
		{
		}
		protected SmartIrc4netException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
