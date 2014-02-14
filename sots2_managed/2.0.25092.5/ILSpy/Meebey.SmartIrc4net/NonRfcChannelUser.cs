using System;
namespace Meebey.SmartIrc4net
{
	public class NonRfcChannelUser : ChannelUser
	{
		private bool _IsHalfop;
		private bool _IsOwner;
		private bool _IsAdmin;
		public bool IsHalfop
		{
			get
			{
				return this._IsHalfop;
			}
			set
			{
				this._IsHalfop = value;
			}
		}
		internal NonRfcChannelUser(string channel, IrcUser ircuser) : base(channel, ircuser)
		{
		}
	}
}
