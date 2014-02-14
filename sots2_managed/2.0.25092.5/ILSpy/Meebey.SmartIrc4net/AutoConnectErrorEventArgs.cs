using System;
namespace Meebey.SmartIrc4net
{
	public class AutoConnectErrorEventArgs : EventArgs
	{
		private Exception _Exception;
		private string _Address;
		private int _Port;
		public Exception Exception
		{
			get
			{
				return this._Exception;
			}
		}
		public string Address
		{
			get
			{
				return this._Address;
			}
		}
		public int Port
		{
			get
			{
				return this._Port;
			}
		}
		internal AutoConnectErrorEventArgs(string address, int port, Exception ex)
		{
			this._Address = address;
			this._Port = port;
			this._Exception = ex;
		}
	}
}
