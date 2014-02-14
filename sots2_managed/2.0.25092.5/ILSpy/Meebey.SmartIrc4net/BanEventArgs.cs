using System;
namespace Meebey.SmartIrc4net
{
	public class BanEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Who;
		private string _Hostmask;
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
		public string Hostmask
		{
			get
			{
				return this._Hostmask;
			}
		}
		internal BanEventArgs(IrcMessageData data, string channel, string who, string hostmask) : base(data)
		{
			this._Channel = channel;
			this._Who = who;
			this._Hostmask = hostmask;
		}
	}
}
