using System;
using System.Collections;
namespace Kerberos.Sots.Console
{
	internal class CommandHistory
	{
		private int currentPosn;
		private string lastCommand;
		private ArrayList commandHistory = new ArrayList();
		internal string LastCommand
		{
			get
			{
				return this.lastCommand;
			}
		}
		internal CommandHistory()
		{
		}
		internal void Add(string command)
		{
			if (command != this.lastCommand)
			{
				this.commandHistory.Add(command);
				this.lastCommand = command;
				this.currentPosn = this.commandHistory.Count;
			}
		}
		internal bool DoesPreviousCommandExist()
		{
			return this.currentPosn > 0;
		}
		internal bool DoesNextCommandExist()
		{
			return this.currentPosn < this.commandHistory.Count - 1;
		}
		internal string GetPreviousCommand()
		{
			this.lastCommand = (string)this.commandHistory[--this.currentPosn];
			return this.lastCommand;
		}
		internal string GetNextCommand()
		{
			this.lastCommand = (string)this.commandHistory[++this.currentPosn];
			return this.LastCommand;
		}
		internal string[] GetCommandHistory()
		{
			return (string[])this.commandHistory.ToArray(typeof(string));
		}
	}
}
