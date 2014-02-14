using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.Framework
{
	internal class Log
	{
		private ILogHost _logHost;
		public event MessageLoggedEventHandler MessageLogged
		{
			add
			{
				this._logHost.MessageLogged += value;
			}
			remove
			{
				this._logHost.MessageLogged -= value;
			}
		}
		public string FilePath
		{
			get
			{
				return this._logHost.FilePath;
			}
		}
		public LogLevel Level
		{
			get
			{
				return this._logHost.Level;
			}
			set
			{
				this._logHost.Level = value;
			}
		}
		public Log(ILogHost loghost)
		{
			this._logHost = loghost;
		}
		public void Trace(string message, string category, LogLevel level)
		{
			this._logHost.LogMessage(level, LogSeverity.Trace, category, message);
		}
		public void Trace(string message, string category)
		{
			this._logHost.LogMessage(LogLevel.Normal, LogSeverity.Trace, category, message);
		}
		public void Warn(string message, string category)
		{
			this._logHost.LogMessage(LogLevel.Normal, LogSeverity.Warn, category, message);
		}
	}
}
