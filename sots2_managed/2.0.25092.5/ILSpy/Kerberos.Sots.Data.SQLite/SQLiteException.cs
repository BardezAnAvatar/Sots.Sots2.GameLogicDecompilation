using System;
namespace Kerberos.Sots.Data.SQLite
{
	internal class SQLiteException : Exception
	{
		public SQLiteException(string message) : base(message)
		{
		}
	}
}
