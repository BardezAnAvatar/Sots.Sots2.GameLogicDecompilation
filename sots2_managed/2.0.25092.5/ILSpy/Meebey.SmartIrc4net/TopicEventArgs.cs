using System;
namespace Meebey.SmartIrc4net
{
	public class TopicEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Topic;
		public string Channel
		{
			get
			{
				return this._Channel;
			}
		}
		public string Topic
		{
			get
			{
				return this._Topic;
			}
		}
		internal TopicEventArgs(IrcMessageData data, string channel, string topic) : base(data)
		{
			this._Channel = channel;
			this._Topic = topic;
		}
	}
}
