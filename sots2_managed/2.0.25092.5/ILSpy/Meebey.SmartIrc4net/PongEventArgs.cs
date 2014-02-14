using System;
namespace Meebey.SmartIrc4net
{
	public class PongEventArgs : IrcEventArgs
	{
		private TimeSpan _Lag;
		public TimeSpan Lag
		{
			get
			{
				return this._Lag;
			}
		}
		internal PongEventArgs(IrcMessageData data, TimeSpan lag) : base(data)
		{
			this._Lag = lag;
		}
	}
}
