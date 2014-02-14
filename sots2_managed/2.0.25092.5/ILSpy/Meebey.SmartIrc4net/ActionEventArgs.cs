using System;
namespace Meebey.SmartIrc4net
{
	public class ActionEventArgs : CtcpEventArgs
	{
		private string _ActionMessage;
		public string ActionMessage
		{
			get
			{
				return this._ActionMessage;
			}
		}
		internal ActionEventArgs(IrcMessageData data, string actionmsg) : base(data, "ACTION", actionmsg)
		{
			this._ActionMessage = actionmsg;
		}
	}
}
