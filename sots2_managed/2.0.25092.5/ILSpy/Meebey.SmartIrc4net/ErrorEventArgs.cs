using System;
namespace Meebey.SmartIrc4net
{
	public class ErrorEventArgs : IrcEventArgs
	{
		private string _ErrorMessage;
		public string ErrorMessage
		{
			get
			{
				return this._ErrorMessage;
			}
		}
		internal ErrorEventArgs(IrcMessageData data, string errormsg) : base(data)
		{
			this._ErrorMessage = errormsg;
		}
	}
}
