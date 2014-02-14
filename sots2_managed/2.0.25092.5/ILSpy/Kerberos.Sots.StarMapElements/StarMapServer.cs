using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPSERVER)]
	internal class StarMapServer : StarMapObject
	{
		public StarMapServer(App game, Vector3 position, string name, string map, string version, int players, int maxPlayers, int ping, bool passworded)
		{
			game.AddExistingObject(this, new object[]
			{
				maxPlayers,
				passworded
			});
			this.PostSetPosition(position);
			string text = string.Concat(new object[]
			{
				"Players: ",
				players,
				"/",
				maxPlayers
			});
			string text2 = "Map: " + map;
			string text3 = "Ping: " + ping;
			string text4 = "Version: " + version;
			this.PostSetProp("MultiLabel", new object[]
			{
				name,
				4,
				text2,
				text4,
				text,
				text3
			});
		}
	}
}
