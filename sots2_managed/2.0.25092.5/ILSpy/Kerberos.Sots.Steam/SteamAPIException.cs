using System;
namespace Kerberos.Sots.Steam
{
	public class SteamAPIException : Exception
	{
		public SteamAPIException(string message) : base(message)
		{
		}
	}
}
