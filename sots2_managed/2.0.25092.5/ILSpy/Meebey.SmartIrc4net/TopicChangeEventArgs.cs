using System;
namespace Meebey.SmartIrc4net
{
	public class TopicChangeEventArgs : IrcEventArgs
	{
		private string _Channel;
		private string _Who;
		private string _NewTopic;
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
		public string NewTopic
		{
			get
			{
				return this._NewTopic;
			}
		}
		internal TopicChangeEventArgs(IrcMessageData data, string channel, string who, string newtopic) : base(data)
		{
			this._Channel = channel;
			this._Who = who;
			this._NewTopic = newtopic;
		}
	}
}
