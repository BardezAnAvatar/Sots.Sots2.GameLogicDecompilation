using System;
namespace Meebey.SmartIrc4net
{
	public class NickChangeEventArgs : IrcEventArgs
	{
		private string _OldNickname;
		private string _NewNickname;
		public string OldNickname
		{
			get
			{
				return this._OldNickname;
			}
		}
		public string NewNickname
		{
			get
			{
				return this._NewNickname;
			}
		}
		internal NickChangeEventArgs(IrcMessageData data, string oldnick, string newnick) : base(data)
		{
			this._OldNickname = oldnick;
			this._NewNickname = newnick;
		}
	}
}
