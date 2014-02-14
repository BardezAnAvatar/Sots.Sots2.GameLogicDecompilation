using System;
namespace Kerberos.Sots.Engine
{
	public class LogMessageInfo
	{
		public readonly LogLevel Level;
		public readonly LogSeverity Severity;
		public readonly char SeverityGlyph;
		public readonly string TimeStamp;
		public readonly string Category;
		public readonly string Message;
		public LogMessageInfo()
		{
		}
		public LogMessageInfo(LogLevel level, LogSeverity severity, char severityGlyph, string timeStamp, string category, string message)
		{
			this.Level = level;
			this.Severity = severity;
			this.SeverityGlyph = severityGlyph;
			this.TimeStamp = timeStamp;
			this.Category = category;
			this.Message = message;
		}
	}
}
