using System;
namespace Meebey.SmartIrc4net
{
	public class QuitEventArgs : IrcEventArgs
	{
		private string _Who;
		private string _QuitMessage;
		public string Who
		{
			get
			{
				return this._Who;
			}
		}
		public string QuitMessage
		{
			get
			{
				return this._QuitMessage;
			}
		}
		internal QuitEventArgs(IrcMessageData data, string who, string quitmessage) : base(data)
		{
			this._Who = who;
			this._QuitMessage = quitmessage;
		}
	}
}
