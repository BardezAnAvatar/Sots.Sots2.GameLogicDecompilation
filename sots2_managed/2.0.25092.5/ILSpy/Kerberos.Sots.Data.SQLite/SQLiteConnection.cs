using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
namespace Kerberos.Sots.Data.SQLite
{
	internal class SQLiteConnection : IDisposable
	{
		public const string NullValue = "NULL";
		internal const int SQLITE_OK = 0;
		internal const int SQLITE_ROW = 100;
		internal const int SQLITE_DONE = 101;
		internal const int SQLITE_INTEGER = 1;
		internal const int SQLITE_FLOAT = 2;
		internal const int SQLITE_TEXT = 3;
		internal const int SQLITE_BLOB = 4;
		internal const int SQLITE_NULL = 5;
		private string QueryStack = "BEGIN TRANSACTION;";
		public bool LogQueries = true;
		private static readonly string[] StatementsToLog = new string[]
		{
			"INSERT",
			"UPDATE",
			"DELETE",
			"REPLACE"
		};
		private IntPtr _db;
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_open(string filename, out IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_open(byte[] filename, out IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_open16(byte[] filename, out IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_close(IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_prepare_v2(IntPtr db, byte[] zSql, int nByte, out IntPtr ppStmpt, IntPtr pzTail);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_prepare16_v2(IntPtr db, string zSql, int nByte, out IntPtr ppStmpt, IntPtr pzTail);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_step(IntPtr stmHandle);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_finalize(IntPtr stmHandle);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_errcode(IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_errmsg16")]
		internal static extern IntPtr _sqlite3_errmsg16(IntPtr db);
		internal static string sqlite3_errmsg16(IntPtr db)
		{
			return Marshal.PtrToStringUni(SQLiteConnection._sqlite3_errmsg16(db));
		}
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_column_count(IntPtr stmHandle);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_origin_name16")]
		private static extern IntPtr _sqlite3_column_origin_name16(IntPtr stmHandle, int iCol);
		internal static string sqlite3_column_origin_name16(IntPtr stmHandle, int iCol)
		{
			return Marshal.PtrToStringUni(SQLiteConnection._sqlite3_column_origin_name16(stmHandle, iCol));
		}
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_column_type(IntPtr stmHandle, int iCol);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_trigger_rowid(IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sqlite3_clear_trigger_rowid(IntPtr db);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_column_int(IntPtr stmHandle, int iCol);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_text16")]
		private static extern IntPtr _sqlite3_column_text16(IntPtr stmHandle, int iCol);
		internal static string sqlite3_column_text16(IntPtr stmHandle, int iCol)
		{
			return Marshal.PtrToStringAuto(SQLiteConnection._sqlite3_column_text16(stmHandle, iCol));
		}
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		internal static extern int sqlite3_exec(IntPtr dbHandle, byte[] statement, IntPtr callbackPtr, IntPtr callbackArg, out string errmsg);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern double sqlite3_column_double(IntPtr stmHandle, int iCol);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sqlite3_backup_init(IntPtr to, string todbname, IntPtr from, string fromdbname);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_backup_step(IntPtr backupHandle, int page);
		[DllImport("sqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sqlite3_backup_finish(IntPtr backupHandle);
		public IntPtr GetDbPointer()
		{
			return this._db;
		}
		public void ExecuteNonQueryReferenceUTF8(string statement)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(statement);
			string text;
			if (SQLiteConnection.sqlite3_exec(this._db, bytes, IntPtr.Zero, IntPtr.Zero, out text) != 0)
			{
				throw new SQLiteException("Could not execute query: " + this.GetErrorMessage());
			}
		}
		private static string GetResultCodeDescription(int value)
		{
			switch (value)
			{
			case 0:
				return "Successful result";
			case 1:
				return "SQL error or missing database";
			case 2:
				return "Internal logic error in SQLite";
			case 3:
				return "Access permission denied";
			case 4:
				return "Callback routine requested an abort";
			case 5:
				return "The database file is locked";
			case 6:
				return "A table in the database is locked";
			case 7:
				return "A malloc() failed";
			case 8:
				return "Attempt to write a readonly database";
			case 9:
				return "Operation terminated by sqlite3_interrupt()";
			case 10:
				return "Some kind of disk I/O error occurred";
			case 11:
				return "The database disk image is malformed";
			case 12:
				return "Unknown opcode in sqlite3_file_control()";
			case 13:
				return "Insertion failed because database is full";
			case 14:
				return "Unable to open the database file";
			case 15:
				return "Database lock protocol error";
			case 16:
				return "Database is empty";
			case 17:
				return "The database schema changed";
			case 18:
				return "String or BLOB exceeds size limit";
			case 19:
				return "Abort due to constraint violation";
			case 20:
				return "Data type mismatch";
			case 21:
				return "Library used incorrectly";
			case 22:
				return "Uses OS features not supported on host";
			case 23:
				return "Authorization denied";
			case 24:
				return "Auxiliary database format error";
			case 25:
				return "2nd parameter to sqlite3_bind out of range";
			case 26:
				return "File opened that is not a database file";
			default:
				switch (value)
				{
				case 100:
					return "sqlite3_step() has another row ready";
				case 101:
					return "sqlite3_step() has finished executing";
				default:
					return "Unrecognized result code " + value;
				}
				break;
			}
		}
		private string GetErrorMessage()
		{
			return SQLiteConnection.GetResultCodeDescription(SQLiteConnection.sqlite3_errcode(this._db)) + " > " + SQLiteConnection.sqlite3_errmsg16(this._db);
		}
		public SQLiteConnection(string path)
		{
			this.OpenDatabase(path);
		}
		private void PerformBackup(IntPtr from, IntPtr to)
		{
			IntPtr intPtr = SQLiteConnection.sqlite3_backup_init(to, "main", from, "main");
			if (intPtr == IntPtr.Zero)
			{
				throw new SQLiteException("Could not initialize db backup: " + this.GetErrorMessage());
			}
			int num;
			for (num = 0; num == 0; num = SQLiteConnection.sqlite3_backup_step(intPtr, -1))
			{
			}
			if (num != 101)
			{
				throw new SQLiteException("Could not perform backup step: " + this.GetErrorMessage());
			}
			SQLiteConnection.sqlite3_backup_finish(intPtr);
		}
		private static IntPtr OpenDatabaseCore(string path)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(path);
			IntPtr result;
			int num = SQLiteConnection.sqlite3_open(bytes, out result);
			if (num != 0)
			{
				throw new SQLiteException("Could not open database '" + path + "'");
			}
			return result;
		}
		private void CloseDatabaseCore(IntPtr dbPtr)
		{
			int num = SQLiteConnection.sqlite3_close(dbPtr);
			if (num != 0)
			{
				throw new SQLiteException("Could not close database: " + this.GetErrorMessage());
			}
		}
		public void LoadBackup(string path)
		{
			IntPtr intPtr = SQLiteConnection.OpenDatabaseCore(path);
			this.PerformBackup(intPtr, this._db);
			this.CloseDatabaseCore(intPtr);
		}
		public void SaveBackup(string path)
		{
			IntPtr intPtr = SQLiteConnection.OpenDatabaseCore(path);
			this.PerformBackup(this._db, intPtr);
			this.CloseDatabaseCore(intPtr);
		}
		public void Reload()
		{
			IntPtr intPtr = SQLiteConnection.OpenDatabaseCore(":memory:");
			this.PerformBackup(this._db, intPtr);
			this.CloseDatabaseCore(this._db);
			this._db = intPtr;
		}
		private void OpenDatabase(string path)
		{
			this._db = SQLiteConnection.OpenDatabaseCore(path);
			if (path == ":memory:")
			{
				this.ExecuteNonQueryReferenceUTF8("PRAGMA page_size = 8192");
				this.ExecuteNonQueryReferenceUTF8("PRAGMA synchronous = OFF");
				this.ExecuteNonQueryReferenceUTF8("PRAGMA journal_mode = OFF");
				this.ExecuteNonQueryReferenceUTF8("PRAGMA temp_store = 2");
				this.ExecuteNonQueryReferenceUTF8("PRAGMA cache_size = 50000");
				this.ExecuteNonQueryReferenceUTF8("PRAGMA wal_autocheckpoint = 100000");
			}
		}
		private void CloseDatabase()
		{
			this.CloseDatabaseCore(this._db);
			this._db = IntPtr.Zero;
		}
		private static string[] SplitQuery(string query)
		{
			return (
				from x in query.Split(new char[]
				{
					';'
				}, StringSplitOptions.RemoveEmptyEntries)
				select x.Trim() into y
				where y.Length > 0
				select y).ToArray<string>();
		}
		private IntPtr PrepareStatement(string statement)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(statement);
			IntPtr result;
			int num = SQLiteConnection.sqlite3_prepare_v2(this._db, bytes, bytes.Length + 1, out result, IntPtr.Zero);
			if (num != 0)
			{
				string errorMessage = this.GetErrorMessage();
				throw new SQLiteException("Could not prepare SQL statement '" + statement + "': " + errorMessage);
			}
			return result;
		}
		private void FinalizeStatement(IntPtr statementPtr)
		{
			int num = SQLiteConnection.sqlite3_finalize(statementPtr);
			if (num != 0)
			{
				throw new SQLiteException("Could not finalize SQL statement: " + this.GetErrorMessage());
			}
		}
		private static bool StepStatement(IntPtr statementPtr)
		{
			int num = SQLiteConnection.sqlite3_step(statementPtr);
			if (num == 100)
			{
				return true;
			}
			if (num == 101)
			{
				return false;
			}
			throw new SQLiteException("SQL statement step failed.");
		}
		private void ExecuteNonQueryStatement(string statement)
		{
			if (SQLiteConnection.IsCommentStatement(statement))
			{
				return;
			}
			IntPtr statementPtr = this.PrepareStatement(statement);
			try
			{
				SQLiteConnection.StepStatement(statementPtr);
			}
			catch (SQLiteException)
			{
				throw new SQLiteException("SQL statement failed: \n" + statement + "\n-- " + this.GetErrorMessage());
			}
			finally
			{
				try
				{
					this.FinalizeStatement(statementPtr);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
		}
		private IEnumerable<Row> StepRows(IntPtr statementPtr)
		{
			while (SQLiteConnection.StepStatement(statementPtr))
			{
				int num = SQLiteConnection.sqlite3_column_count(statementPtr);
				Row row = new Row
				{
					Values = new string[num]
				};
				for (int i = 0; i < num; i++)
				{
					row.Values[i] = SQLiteConnection.sqlite3_column_text16(statementPtr, i);
				}
				yield return row;
			}
			yield break;
		}
		private Table ExecuteTableQueryStatement(string statement)
		{
			IntPtr statementPtr = this.PrepareStatement(statement);
			Table result;
			try
			{
				result = new Table
				{
					Rows = this.StepRows(statementPtr).ToArray<Row>()
				};
			}
			finally
			{
				try
				{
					this.FinalizeStatement(statementPtr);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
			return result;
		}
		private bool TryLogQuery(string query)
		{
			if (!this.LogQueries)
			{
				return false;
			}
			string[] statementsToLog = SQLiteConnection.StatementsToLog;
			for (int i = 0; i < statementsToLog.Length; i++)
			{
				string value = statementsToLog[i];
				if (query.Contains(value) || SQLiteConnection.IsCommentStatement(query))
				{
					this.ExecuteNonQueryStatement(string.Format(Queries.LogTransaction, query.Replace("'", "''").Replace("\r\n", "")));
					return true;
				}
			}
			return false;
		}
		public void LogComment(string comment)
		{
			if (ScriptHost.AllowConsole)
			{
				App.Log.Trace("db.LogComment: " + comment, "data");
			}
			if (!this.LogQueries)
			{
				return;
			}
			comment = comment.Insert(0, "--");
			this.ExecuteNonQueryStatement(string.Format(Queries.LogTransaction, comment.Replace("'", "''").Replace("\r\n", "")));
		}
		public Table ExecuteTableQuery(string query, bool splitQuery = true)
		{
			this.TryLogQuery(query);
			List<string> list = new List<string>();
			if (splitQuery)
			{
				list.AddRange(SQLiteConnection.SplitQuery(query));
			}
			else
			{
				list.Add(query);
			}
			int i;
			for (i = 0; i < list.Count - 1; i++)
			{
				this.ExecuteNonQueryStatement(list[i]);
			}
			Table result;
			if (i < list.Count)
			{
				result = this.ExecuteTableQueryStatement(list[i]);
			}
			else
			{
				result = new Table
				{
					Rows = new Row[0]
				};
			}
			return result;
		}
		public void StackQuery(string query)
		{
			this.QueryStack += query;
		}
		public void ExecuteQueryStack(bool ignore = false)
		{
			this.QueryStack += "COMMIT TRANSACTION;";
			this.ExecuteNonQueryStatement(this.QueryStack);
			this.QueryStack = "BEGIN DEFERRED TRANSACTION;";
		}
		public static bool IsCommentStatement(string value)
		{
			return value != null && value.StartsWith("--");
		}
		public void ExecuteNonQuery(string query, bool ignore = false, bool split = true)
		{
			if (!ignore)
			{
				this.TryLogQuery(query);
			}
			if (split)
			{
				string[] array = SQLiteConnection.SplitQuery(query);
				for (int i = 0; i < array.Length; i++)
				{
					string statement = array[i];
					this.ExecuteNonQueryStatement(statement);
				}
				return;
			}
			this.ExecuteNonQueryStatement(query);
		}
		private int ExecuteIntegerQueryStatement(string statement)
		{
			IntPtr intPtr = this.PrepareStatement(statement);
			int result;
			try
			{
				SQLiteConnection.StepStatement(intPtr);
				int num = SQLiteConnection.sqlite3_trigger_rowid(this._db);
				if (num > 0)
				{
					result = num;
				}
				else
				{
					result = SQLiteConnection.sqlite3_column_int(intPtr, 0);
				}
			}
			finally
			{
				try
				{
					this.FinalizeStatement(intPtr);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
			return result;
		}
		private int? ExecuteIntegerQueryStatementDefault(string statement, int? defaultValue)
		{
			IntPtr intPtr = this.PrepareStatement(statement);
			int? result;
			try
			{
				if (!SQLiteConnection.StepStatement(intPtr))
				{
					result = defaultValue;
				}
				else
				{
					int num = SQLiteConnection.sqlite3_trigger_rowid(this._db);
					if (num > 0)
					{
						result = new int?(num);
					}
					else
					{
						result = new int?(SQLiteConnection.sqlite3_column_int(intPtr, 0));
					}
				}
			}
			finally
			{
				try
				{
					this.FinalizeStatement(intPtr);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
			return result;
		}
		public int ExecuteIntegerQuery(string query)
		{
			this.TryLogQuery(query);
			SQLiteConnection.sqlite3_clear_trigger_rowid(this._db);
			string[] array = SQLiteConnection.SplitQuery(query);
			int i;
			for (i = 0; i < array.Length - 1; i++)
			{
				this.ExecuteNonQueryStatement(array[i]);
			}
			if (i < array.Length)
			{
				return this.ExecuteIntegerQueryStatement(array[i]);
			}
			return 0;
		}
		public int? ExecuteIntegerQueryDefault(string query, int? defaultValue)
		{
			this.TryLogQuery(query);
			SQLiteConnection.sqlite3_clear_trigger_rowid(this._db);
			string[] array = SQLiteConnection.SplitQuery(query);
			int i;
			for (i = 0; i < array.Length - 1; i++)
			{
				this.ExecuteNonQueryStatement(array[i]);
			}
			if (i < array.Length)
			{
				return this.ExecuteIntegerQueryStatementDefault(array[i], defaultValue);
			}
			return new int?(0);
		}
		private string ExecuteStringQueryStatement(string statement)
		{
			IntPtr intPtr = this.PrepareStatement(statement);
			string result;
			try
			{
				SQLiteConnection.StepStatement(intPtr);
				result = SQLiteConnection.sqlite3_column_text16(intPtr, 0);
			}
			finally
			{
				try
				{
					this.FinalizeStatement(intPtr);
				}
				catch (SQLiteException ex)
				{
					throw new SQLiteException(ex.Message + "\nin: " + statement);
				}
			}
			return result;
		}
		public string ExecuteStringQuery(string query)
		{
			this.TryLogQuery(query);
			string[] array = SQLiteConnection.SplitQuery(query);
			int i;
			for (i = 0; i < array.Length - 1; i++)
			{
				this.ExecuteNonQueryStatement(array[i]);
			}
			if (i < array.Length)
			{
				return this.ExecuteStringQueryStatement(array[i]);
			}
			return null;
		}
		public void VacuumDatabase()
		{
			this.ExecuteNonQueryReferenceUTF8("VACUUM;");
		}
		private IEnumerable<int> ExecuteIntegerArrayQueryStatement(string statement)
		{
			IntPtr intPtr = this.PrepareStatement(statement);
			try
			{
				while (SQLiteConnection.StepStatement(intPtr))
				{
					yield return SQLiteConnection.sqlite3_column_int(intPtr, 0);
				}
			}
			finally
			{
				this.FinalizeStatement(intPtr);
			}
			yield break;
		}
		public int[] ExecuteIntegerArrayQuery(string query)
		{
			this.TryLogQuery(query);
			string[] array = SQLiteConnection.SplitQuery(query);
			int i;
			for (i = 0; i < array.Length - 1; i++)
			{
				this.ExecuteNonQueryStatement(array[i]);
			}
			if (i < array.Length)
			{
				return this.ExecuteIntegerArrayQueryStatement(array[i]).ToArray<int>();
			}
			return new int[0];
		}
		public void Dispose()
		{
			this.CloseDatabase();
		}
	}
}
