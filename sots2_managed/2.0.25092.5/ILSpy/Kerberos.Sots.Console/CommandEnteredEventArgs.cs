using System;
namespace Kerberos.Sots.Console
{
	public class CommandEnteredEventArgs : EventArgs
	{
		private string command;
		public string Command
		{
			get
			{
				return this.command;
			}
		}
		public CommandEnteredEventArgs(string command)
		{
			this.command = command;
		}
	}
}
