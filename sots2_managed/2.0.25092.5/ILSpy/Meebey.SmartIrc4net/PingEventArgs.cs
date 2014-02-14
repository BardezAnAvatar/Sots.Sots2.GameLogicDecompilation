using System;
namespace Meebey.SmartIrc4net
{
	public class PingEventArgs : IrcEventArgs
	{
		private string _PingData;
		public string PingData
		{
			get
			{
				return this._PingData;
			}
		}
		internal PingEventArgs(IrcMessageData data, string pingdata) : base(data)
		{
			this._PingData = pingdata;
		}
	}
}
