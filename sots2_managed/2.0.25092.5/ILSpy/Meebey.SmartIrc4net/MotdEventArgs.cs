using System;
namespace Meebey.SmartIrc4net
{
	public class MotdEventArgs : IrcEventArgs
	{
		private string _MotdMessage;
		public string MotdMessage
		{
			get
			{
				return this._MotdMessage;
			}
		}
		internal MotdEventArgs(IrcMessageData data, string motdmsg) : base(data)
		{
			this._MotdMessage = motdmsg;
		}
	}
}
