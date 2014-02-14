using System;
namespace Meebey.SmartIrc4net
{
	public class PartEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Who;
		private string _PartMessage;
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
		public string PartMessage
		{
			get
			{
				return this._PartMessage;
			}
		}
		internal PartEventArgs(IrcMessageData data, string channel, string who, string partmessage) : base(data)
		{
			this._Channel = channel;
			this._Who = who;
			this._PartMessage = partmessage;
		}
	}
}
