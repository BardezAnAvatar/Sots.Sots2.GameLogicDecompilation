using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Data
{
	internal class ServerInfo : IIDProvider
	{
		public ulong serverID;
		public string name;
		public string map;
		public string version;
		public int players;
		public int maxPlayers;
		public int ping;
		public bool passworded;
		public List<PlayerSetup> playerInfo;
		public Vector3 Origin;
		public int ID
		{
			get;
			set;
		}
	}
}
