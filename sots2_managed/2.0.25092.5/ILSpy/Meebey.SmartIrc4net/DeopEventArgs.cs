using System;
namespace Meebey.SmartIrc4net
{
	public class DeopEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Who;
		private string _Whom;
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
		public string Whom
		{
			get
			{
				return this._Whom;
			}
		}
		internal DeopEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data)
		{
			this._Channel = channel;
			this._Who = who;
			this._Whom = whom;
		}
	}
}
