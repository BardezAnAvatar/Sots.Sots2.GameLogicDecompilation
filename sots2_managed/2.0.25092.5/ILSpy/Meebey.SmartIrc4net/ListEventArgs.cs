using System;
namespace Meebey.SmartIrc4net
{
	public class ListEventArgs : IrcEventArgs
	{
		private ChannelInfo f_ListInfo;
		public ChannelInfo ListInfo
		{
			get
			{
				return this.f_ListInfo;
			}
		}
		internal ListEventArgs(IrcMessageData data, ChannelInfo listInfo) : base(data)
		{
			this.f_ListInfo = listInfo;
		}
	}
}
