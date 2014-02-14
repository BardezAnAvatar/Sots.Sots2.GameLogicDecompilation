using System;
namespace Meebey.SmartIrc4net
{
	public class NamesEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string[] _UserList;
		public string Channel
		{
			get
			{
				return this._Channel;
			}
		}
		public string[] UserList
		{
			get
			{
				return this._UserList;
			}
		}
		internal NamesEventArgs(IrcMessageData data, string channel, string[] userlist) : base(data)
		{
			this._Channel = channel;
			this._UserList = userlist;
		}
	}
}
