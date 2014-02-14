using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class MeteorShower
	{
		private const string FactionName = "asteroids";
		private const string PlayerName = "Meteors";
		private const string PlayerAvatar = "\\base\\factions\\asteroids\\avatars\\Asteroids_Avatar.tga";
		private const string FleetName = "Meteors";
		private static Random MSRandom = new Random();
		private static string[] MeteorDesignFiles = new string[]
		{
			"Small_01.section",
			"Small_02.section",
			"Small_03.section",
			"Small_04.section",
			"Small_05.section",
			"Medium_01.section",
			"Medium_02.section",
			"Large_01.section",
			"Large_02.section",
			"Large_03.section",
			"Large_04.section",
			"Large_05.section"
		};
		private int PlayerId = -1;
		public static bool ForceEncounter = false;
		private int[] _meteorDesignIds;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int[] MeteorDesignIds
		{
			get
			{
				return this._meteorDesignIds;
			}
		}
		public static MeteorShower InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			if (assetdb.GetFaction("asteroids") == null)
			{
				return null;
			}
			MeteorShower meteorShower = new MeteorShower();
			meteorShower.PlayerId = gamedb.InsertPlayer("Meteors", "asteroids", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\asteroids\\avatars\\Asteroids_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			int num = MeteorShower.MeteorDesignFiles.Count<string>();
			meteorShower._meteorDesignIds = new int[num];
			for (int i = 0; i < num; i++)
			{
				DesignInfo design = new DesignInfo(meteorShower.PlayerId, "Huge Meteor " + i, new string[]
				{
					string.Format("factions\\{0}\\sections\\{1}", "asteroids", MeteorShower.MeteorDesignFiles[i])
				});
				meteorShower._meteorDesignIds[i] = gamedb.InsertDesignByDesignInfo(design);
			}
			return meteorShower;
		}
		public static MeteorShower ResumeEncounter(GameDatabase gamedb)
		{
			if (gamedb.AssetDatabase.GetFaction("asteroids") == null)
			{
				return null;
			}
			MeteorShower meteorShower = new MeteorShower();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Meteors"));
			meteorShower.PlayerId = playerInfo.ID;
			List<DesignInfo> source2 = gamedb.GetDesignInfosForPlayer(meteorShower.PlayerId).ToList<DesignInfo>();
			int num = MeteorShower.MeteorDesignFiles.Count<string>();
			meteorShower._meteorDesignIds = new int[num];
			int i;
			for (i = 0; i < num; i++)
			{
				DesignInfo designInfo = source2.FirstOrDefault((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith(MeteorShower.MeteorDesignFiles[i]));
				if (designInfo == null)
				{
					DesignInfo design = new DesignInfo(meteorShower.PlayerId, "Huge Meteor " + i, new string[]
					{
						string.Format("factions\\{0}\\sections\\{1}", "asteroids", MeteorShower.MeteorDesignFiles[i])
					});
					meteorShower._meteorDesignIds[i] = gamedb.InsertDesignByDesignInfo(design);
				}
				else
				{
					meteorShower._meteorDesignIds[i] = designInfo.ID;
				}
			}
			return meteorShower;
		}
		public void UpdateTurn(GameSession game)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				game.GameDatabase.RemoveFleet(current.ID);
			}
		}
		public void ExecuteEncounter(GameSession game, int targetSystem, float intensity = 1f, bool multiPlanets = false)
		{
			Random safeRandom = App.GetSafeRandom();
			int minValue = (int)((float)game.AssetDatabase.GlobalMeteorShowerData.MinMeteors * intensity);
			int maxValue = (int)((float)game.AssetDatabase.GlobalMeteorShowerData.MaxMeteors * intensity);
			int num = safeRandom.Next(minValue, maxValue);
			int fleetID = game.GameDatabase.InsertFleet(this.PlayerId, 0, targetSystem, targetSystem, "Meteors", FleetType.FL_NORMAL);
			List<int> list = new List<int>();
			for (int i = 0; i < num; i++)
			{
				list.Add(game.GameDatabase.InsertShip(fleetID, safeRandom.Choose(this.MeteorDesignIds), null, (ShipParams)0, null, 0));
			}
			List<ColonyInfo> list2 = game.GameDatabase.GetColonyInfosForSystem(targetSystem).ToList<ColonyInfo>();
			foreach (int current in list)
			{
				int planetId = 0;
				if (multiPlanets && list2.Count > 0)
				{
					ColonyInfo colonyInfo = safeRandom.Choose(list2);
					planetId = colonyInfo.OrbitalObjectID;
				}
				game.GameDatabase.UpdateShipSystemPosition(current, new Matrix?(MeteorShower.GetSpawnTransform(game.App, targetSystem, planetId)));
			}
		}
		public void AddEncounter(GameSession game, PlayerInfo targetPlayer, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			List<int> list = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayer.ID).ToList<int>();
			if (targetSystem.HasValue || list.Count > 0)
			{
				if (!targetSystem.HasValue)
				{
					List<int> noAsteroidBelt = (
						from x in list
						where !game.GameDatabase.GetStarSystemOrbitalObjectInfos(x).Any((OrbitalObjectInfo y) => game.GameDatabase.GetAsteroidBeltInfo(y.ID) != null)
						select x).ToList<int>();
					list.RemoveAll((int x) => noAsteroidBelt.Contains(x));
					if (list.Count > 0)
					{
						targetSystem = new int?(safeRandom.Choose(list));
					}
					else
					{
						targetSystem = new int?(safeRandom.Choose(noAsteroidBelt));
					}
				}
				game.GameDatabase.InsertIncomingRandom(targetPlayer.ID, targetSystem.Value, RandomEncounter.RE_ASTEROID_SHOWER, game.GameDatabase.GetTurnCount() + 1);
				if (game.DetectedIncomingRandom(targetPlayer.ID, targetSystem.Value))
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_ASTEROID_SHOWER,
						EventMessage = TurnEventMessage.EM_INCOMING_ASTEROID_SHOWER,
						PlayerID = targetPlayer.ID,
						SystemID = targetSystem.Value,
						TurnNumber = game.GameDatabase.GetTurnCount()
					});
				}
			}
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, int systemId)
		{
			float num = 0f;
			Matrix result = Matrix.Identity;
			foreach (OrbitalObjectInfo current in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
			{
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(current.ID);
				if (planetInfo != null)
				{
					ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(current.ID);
					if (colonyInfoForPlanet != null)
					{
						Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(current.ID);
						float lengthSquared = orbitalTransform.Position.LengthSquared;
						if (lengthSquared > num)
						{
							num = lengthSquared;
							result = orbitalTransform;
						}
					}
				}
			}
			if (num <= 0f)
			{
				return Matrix.Identity;
			}
			return result;
		}
		public static Matrix GetSpawnTransform(App app, int systemId, int planetId = 0)
		{
			float num = 0f;
			float num2 = 5000f;
			Matrix? matrix = null;
			if (planetId != 0)
			{
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(planetId);
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(planetId);
				if (planetInfo != null && colonyInfoForPlanet != null)
				{
					num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 15000f;
					matrix = new Matrix?(app.GameDatabase.GetOrbitalTransform(planetId));
				}
			}
			if (!matrix.HasValue)
			{
				foreach (OrbitalObjectInfo current in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
				{
					PlanetInfo planetInfo2 = app.GameDatabase.GetPlanetInfo(current.ID);
					if (planetInfo2 != null)
					{
						ColonyInfo colonyInfoForPlanet2 = app.GameDatabase.GetColonyInfoForPlanet(current.ID);
						if (colonyInfoForPlanet2 != null)
						{
							Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(current.ID);
							float lengthSquared = orbitalTransform.Position.LengthSquared;
							if (lengthSquared > num)
							{
								num = lengthSquared;
								matrix = new Matrix?(orbitalTransform);
								num2 = StarSystemVars.Instance.SizeToRadius(planetInfo2.Size) + 15000f;
							}
						}
					}
				}
			}
			if (!matrix.HasValue)
			{
				return Matrix.Identity;
			}
			Vector3 forward = matrix.Value.Position;
			forward.Normalize();
			Matrix rhs = Matrix.CreateWorld(matrix.Value.Position, forward, Vector3.UnitY);
			Vector3 vector = Matrix.PolarDeviation(MeteorShower.MSRandom, 80f).Forward * num2;
			vector.Y = Math.Max(Math.Min(vector.Y, 300f), -300f);
			float num3 = MeteorShower.MSRandom.NextInclusive(-1500f, 1500f);
			vector.Normalize();
			forward = -vector;
			Matrix lhs = Matrix.CreateWorld(vector * (num2 + num3), forward, Vector3.UnitY);
			return lhs * rhs;
		}
	}
}
