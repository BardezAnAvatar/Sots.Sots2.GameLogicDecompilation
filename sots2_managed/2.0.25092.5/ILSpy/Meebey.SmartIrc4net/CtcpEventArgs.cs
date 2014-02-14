using System;
namespace Meebey.SmartIrc4net
{
	public class CtcpEventArgs : IrcEventArgs
	{
		private string _CtcpCommand;
		private string _CtcpParameter;
		public string CtcpCommand
		{
			get
			{
				return this._CtcpCommand;
			}
		}
		public string CtcpParameter
		{
			get
			{
				return this._CtcpParameter;
			}
		}
		internal CtcpEventArgs(IrcMessageData data, string ctcpcmd, string ctcpparam) : base(data)
		{
			this._CtcpCommand = ctcpcmd;
			this._CtcpParameter = ctcpparam;
		}
	}
}
