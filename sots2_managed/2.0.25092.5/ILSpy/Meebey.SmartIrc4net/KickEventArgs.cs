using System;
namespace Meebey.SmartIrc4net
{
	public class KickEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Who;
		private string _Whom;
		private string _KickReason;
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
		public string KickReason
		{
			get
			{
				return this._KickReason;
			}
		}
		internal KickEventArgs(IrcMessageData data, string channel, string who, string whom, string kickreason) : base(data)
		{
			this._Channel = channel;
			this._Who = who;
			this._Whom = whom;
			this._KickReason = kickreason;
		}
	}
}
