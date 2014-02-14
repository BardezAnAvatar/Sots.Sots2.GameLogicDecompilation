using System;
namespace Meebey.SmartIrc4net
{
	public class AwayEventArgs : IrcEventArgs
	{
		private string _Who;
		private string _AwayMessage;
		public string Who
		{
			get
			{
				return this._Who;
			}
		}
		public string AwayMessage
		{
			get
			{
				return this._AwayMessage;
			}
		}
		internal AwayEventArgs(IrcMessageData data, string who, string awaymessage) : base(data)
		{
			this._Who = who;
			this._AwayMessage = awaymessage;
		}
	}
}
