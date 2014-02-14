using System;
namespace Meebey.SmartIrc4net
{
	public class WriteLineEventArgs : EventArgs
	{
		private string _Line;
		public string Line
		{
			get
			{
				return this._Line;
			}
		}
		internal WriteLineEventArgs(string line)
		{
			this._Line = line;
		}
	}
}
