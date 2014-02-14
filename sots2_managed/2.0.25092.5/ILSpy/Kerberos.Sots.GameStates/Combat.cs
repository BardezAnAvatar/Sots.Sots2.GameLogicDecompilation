using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_COMBAT)]
	internal class Combat : GameObject
	{
		private Combat()
		{
		}
		static Combat()
		{
		}
		public static Combat Create(App game, OrbitCameraController cameraController, CombatInput input, CombatSensor sensor, StarSystem system, CombatGrid grid, Vector3 origin, float radius, int duration, Player[] players, Dictionary<Player, Dictionary<Player, PlayerCombatDiplomacy>> diplomacyStates, bool simulateOnly = false)
		{
			List<int> list = (
				from x in players
				where !x.IsStandardPlayer
				select x.ObjectID).ToList<int>();
			Combat combat = new Combat();
			List<object> list2 = new List<object>();
			list2.Add(cameraController.GetObjectID());
			list2.Add(input.GetObjectID());
			list2.Add(sensor.GetObjectID());
			list2.Add(system.GetObjectID());
			list2.Add(grid.GetObjectID());
			list2.Add(origin);
			list2.Add(radius);
			list2.Add(duration);
			list2.Add(game.LocalPlayer.GetObjectID());
			list2.Add(simulateOnly);
			list2.Add(players.Length);
			for (int i = 0; i < players.Length; i++)
			{
				Player player = players[i];
				list2.Add(player.ObjectID);
			}
			list2.Add(diplomacyStates.Count);
			foreach (KeyValuePair<Player, Dictionary<Player, PlayerCombatDiplomacy>> current in diplomacyStates)
			{
				list2.Add(current.Key.GetObjectID());
				list2.Add(current.Value.Count);
				foreach (KeyValuePair<Player, PlayerCombatDiplomacy> current2 in current.Value)
				{
					list2.Add(current2.Key.GetObjectID());
					list2.Add(current2.Value);
				}
			}
			list2.Add(list.Count);
			foreach (int current3 in list)
			{
				list2.Add(current3);
			}
			game.AddExistingObject(combat, list2.ToArray());
			return combat;
		}
	}
}
