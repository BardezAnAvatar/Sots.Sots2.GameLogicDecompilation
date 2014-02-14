using System;
namespace Meebey.SmartIrc4net
{
	public class IrcEventArgs : EventArgs
	{
		private readonly IrcMessageData _Data;
		public IrcMessageData Data
		{
			get
			{
				return this._Data;
			}
		}
		internal IrcEventArgs(IrcMessageData data)
		{
			this._Data = data;
		}
	}
}
