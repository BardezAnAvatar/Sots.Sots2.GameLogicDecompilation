using System;
using System.Net.Sockets;
namespace Meebey.SmartIrc4net
{
	internal class IrcTcpClient : TcpClient
	{
		public Socket Socket
		{
			get
			{
				return base.Client;
			}
		}
	}
}
