using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class GhostShip
	{
		private const string FactionName = "ghostship";
		private const string PlayerName = "Ghost Ship";
		private const string PlayerAvatar = "\\base\\factions\\ghostship\\avatars\\Ghostship_Avatar.tga";
		private const string FleetName = "Ghost Ship";
		private const string designFile = "lv_SFS.section";
		private int PlayerId = -1;
		public static bool ForceEncounter;
		private int _designId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int DesignId
		{
			get
			{
				return this._designId;
			}
		}
		public static GhostShip InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			GhostShip ghostShip = new GhostShip();
			ghostShip.PlayerId = gamedb.InsertPlayer("Ghost Ship", "ghostship", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\ghostship\\avatars\\Ghostship_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			List<DesignInfo> designs = gamedb.GetDesignInfosForPlayer(ghostShip.PlayerId).ToList<DesignInfo>();
			ghostShip._designId = gamedb.AddOrGetEncounterDesignInfo(designs, ghostShip.PlayerId, "The Flying Dutchman", "ghostship", new string[]
			{
				"lv_SFS.section"
			});
			int fleetID = gamedb.InsertFleet(ghostShip.PlayerId, 0, 0, 0, "Ghost Ship", FleetType.FL_NORMAL);
			gamedb.InsertShip(fleetID, ghostShip._designId, null, (ShipParams)0, null, 0);
			return ghostShip;
		}
		public static GhostShip ResumeEncounter(GameDatabase gamedb)
		{
			GhostShip ghostShip = new GhostShip();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Ghost Ship"));
			if (playerInfo != null)
			{
				ghostShip.PlayerId = playerInfo.ID;
			}
			else
			{
				ghostShip.PlayerId = gamedb.InsertPlayer("Ghost Ship", "ghostship", null, new Vector3(0f), new Vector3(0f), "", "\\base\\factions\\ghostship\\avatars\\Ghostship_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			List<DesignInfo> designs = gamedb.GetDesignInfosForPlayer(ghostShip.PlayerId).ToList<DesignInfo>();
			ghostShip._designId = gamedb.AddOrGetEncounterDesignInfo(designs, ghostShip.PlayerId, "The Flying Dutchman", "ghostship", new string[]
			{
				"lv_SFS.section"
			});
			return ghostShip;
		}
		public void UpdateTurn(GameSession game)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				List<ShipInfo> list2 = game.GameDatabase.GetShipInfoByFleetID(current.ID, false).ToList<ShipInfo>();
				if (list2.Count > 0)
				{
					current.SystemID = 0;
					game.GameDatabase.UpdateFleetInfo(current);
				}
				else
				{
					game.GameDatabase.RemoveFleet(current.ID);
				}
			}
		}
		public void ExecuteEncounter(GameSession game, PlayerInfo targetPlayer, int targetSystem)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			if (list.Count > 0)
			{
				FleetInfo fleetInfo = list.FirstOrDefault<FleetInfo>();
				fleetInfo.SystemID = targetSystem;
				game.GameDatabase.UpdateFleetInfo(fleetInfo);
			}
		}
		public void AddEncounter(GameSession game, PlayerInfo targetPlayer)
		{
			Random safeRandom = App.GetSafeRandom();
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			List<int> list2 = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayer.ID).ToList<int>();
			if (list2.Count > 0 && list.Count > 0)
			{
				int num = safeRandom.Choose(list2);
				game.GameDatabase.InsertIncomingRandom(targetPlayer.ID, num, RandomEncounter.RE_GHOST_SHIP, game.GameDatabase.GetTurnCount() + 1);
				if (game.DetectedIncomingRandom(targetPlayer.ID, num))
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_GHOST_SHIP,
						EventMessage = TurnEventMessage.EM_INCOMING_GHOST_SHIP,
						PlayerID = targetPlayer.ID,
						SystemID = num,
						TurnNumber = game.GameDatabase.GetTurnCount()
					});
				}
			}
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			return GhostShip.GetSpawnTransform(app, starSystem);
		}
		public static Matrix GetSpawnTransform(App app, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			float num = 0f;
			float num2 = 5000f;
			Vector3 v = Vector3.Zero;
			Vector3 vector = -Vector3.UnitZ;
			List<StellarBody> list = new List<StellarBody>();
			foreach (StellarBody current in starSystem.GetPlanetsInSystem())
			{
				List<Ship> stationsAroundPlanet = starSystem.GetStationsAroundPlanet(current.Parameters.OrbitalID);
				list.Add(current);
				foreach (Ship current2 in stationsAroundPlanet)
				{
					float lengthSquared = current2.Position.LengthSquared;
					if (lengthSquared > num)
					{
						num = lengthSquared;
						v = current2.Position;
						num2 = current2.ShipSphere.radius;
						vector = current.Parameters.Position - current2.Position;
					}
				}
			}
			if (num <= 0f)
			{
				foreach (StellarBody current3 in list)
				{
					if (current3.Population > 0.0)
					{
						Vector3 position = current3.Parameters.Position;
						float lengthSquared2 = position.LengthSquared;
						if (lengthSquared2 > num)
						{
							num = lengthSquared2;
							v = current3.Parameters.Position;
							num2 = current3.Parameters.Radius;
							int arg_163_0 = current3.Parameters.OrbitalID;
							vector = -v;
						}
					}
				}
			}
			vector.Normalize();
			Vector3 position2 = v - vector * (num2 + 20000f);
			return Matrix.CreateWorld(position2, vector, Vector3.UnitY);
		}
	}
}
