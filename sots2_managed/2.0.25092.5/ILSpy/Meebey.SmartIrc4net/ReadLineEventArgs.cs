using System;
namespace Meebey.SmartIrc4net
{
	public class ReadLineEventArgs : EventArgs
	{
		private string _Line;
		public string Line
		{
			get
			{
				return this._Line;
			}
		}
		internal ReadLineEventArgs(string line)
		{
			this._Line = line;
		}
	}
}
