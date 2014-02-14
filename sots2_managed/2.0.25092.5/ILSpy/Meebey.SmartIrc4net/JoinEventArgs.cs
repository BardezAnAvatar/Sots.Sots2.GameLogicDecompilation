using System;
namespace Meebey.SmartIrc4net
{
	public class JoinEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Who;
		public string Channel
		{
			get
			{
				return this._Channel;
			}
		}
		public string Who
		{
			get
			{
				return this._Who;
			}
		}
		internal JoinEventArgs(IrcMessageData data, string channel, string who) : base(data)
		{
			this._Channel = channel;
			this._Who = who;
		}
	}
}
