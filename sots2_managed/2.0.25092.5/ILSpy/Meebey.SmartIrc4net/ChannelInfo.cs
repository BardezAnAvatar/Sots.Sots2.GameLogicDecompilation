using System;
namespace Meebey.SmartIrc4net
{
	public class ChannelInfo
	{
		private string f_Channel;
		private int f_UserCount;
		private string f_Topic;
		public string Channel
		{
			get
			{
				return this.f_Channel;
			}
		}
		public int UserCount
		{
			get
			{
				return this.f_UserCount;
			}
		}
		public string Topic
		{
			get
			{
				return this.f_Topic;
			}
		}
		internal ChannelInfo(string channel, int userCount, string topic)
		{
			this.f_Channel = channel;
			this.f_UserCount = userCount;
			this.f_Topic = topic;
		}
	}
}
