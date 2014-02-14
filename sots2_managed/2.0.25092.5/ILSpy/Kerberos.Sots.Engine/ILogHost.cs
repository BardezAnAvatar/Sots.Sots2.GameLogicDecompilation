using System;
namespace Kerberos.Sots.Engine
{
	public interface ILogHost
	{
		event MessageLoggedEventHandler MessageLogged;
		string FilePath
		{
			get;
		}
		LogLevel Level
		{
			get;
			set;
		}
		void LogMessage(LogLevel level, LogSeverity severity, string category, string message);
	}
}
