using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class Gardeners
	{
		private const string FactionName = "protean";
		private const string PlayerName = "Protean";
		private const string PlayerAvatar = "\\base\\factions\\protean\\avatars\\Protean_Avatar.tga";
		private const string FleetName = "Protean Pod";
		private const string FollowFleetName = "Protean Follow";
		private const string GardenerFleetName = "Gardener";
		private const string ProteanDesignFile = "protean.section";
		private const string GardenerDesignFile = "Rama.section";
		private int PlayerId = -1;
		private int _proteanDesignId;
		private int _gardenerDesignId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int ProteanDesignId
		{
			get
			{
				return this._proteanDesignId;
			}
		}
		public int GardenerDesignId
		{
			get
			{
				return this._gardenerDesignId;
			}
		}
		public static Gardeners InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Gardeners gardeners = new Gardeners();
			gardeners.PlayerId = gamedb.InsertPlayer("Protean", "protean", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\protean\\avatars\\Protean_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(gardeners.PlayerId, "Protean", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "protean", "protean.section")
			});
			gardeners._proteanDesignId = gamedb.InsertDesignByDesignInfo(design);
			if (gamedb.HasEndOfFleshExpansion())
			{
				DesignInfo design2 = new DesignInfo(gardeners.PlayerId, "Gardener", new string[]
				{
					string.Format("factions\\{0}\\sections\\{1}", "protean", "Rama.section")
				});
				gardeners._gardenerDesignId = gamedb.InsertDesignByDesignInfo(design2);
			}
			return gardeners;
		}
		public static Gardeners ResumeEncounter(GameDatabase gamedb)
		{
			Gardeners gardeners = new Gardeners();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Protean"));
			gardeners.PlayerId = playerInfo.ID;
			List<DesignInfo> source2 = gamedb.GetDesignInfosForPlayer(gardeners.PlayerId).ToList<DesignInfo>();
			gardeners._proteanDesignId = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("protean.section")).ID;
			DesignInfo designInfo = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("Rama.section"));
			if (designInfo != null && gamedb.HasEndOfFleshExpansion())
			{
				gardeners._gardenerDesignId = designInfo.ID;
			}
			return gardeners;
		}
		public static int GetPlayerID(GameDatabase gamedb)
		{
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Protean"));
			if (playerInfo == null)
			{
				return 0;
			}
			return playerInfo.ID;
		}
        public static Matrix GetBaseEnemyFleetTrans(App app, int systemId, OrbitalObjectInfo[] objects)
        {
            if (objects.Count<OrbitalObjectInfo>() == 0)
            {
                return Matrix.Identity;
            }
            GardenerInfo info = app.GameDatabase.GetGardenerInfos().ToList<GardenerInfo>().FirstOrDefault<GardenerInfo>(x => x.SystemId == systemId);
            float num = 0f;
            Matrix identity = Matrix.Identity;
            Func<KeyValuePair<int, int>, bool> predicate = null;
            foreach (OrbitalObjectInfo oo in objects)
            {
                PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(oo.ID);
                if (((planetInfo != null) && (planetInfo.Type != "barren")) && (planetInfo.Type != "gaseous"))
                {
                    if (info != null)
                    {
                        if (predicate == null)
                        {
                            predicate = x => x.Value == oo.ID;
                        }
                        if ((info.ShipOrbitMap.Where<KeyValuePair<int, int>>(predicate).Count<KeyValuePair<int, int>>() == 0) && (num > 0f))
                        {
                            continue;
                        }
                    }
                    Vector3 position = app.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position;
                    float lengthSquared = position.LengthSquared;
                    if (lengthSquared > num)
                    {
                        num = lengthSquared;
                        Vector3 forward = position;
                        forward.Normalize();
                        identity = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
                        float num4 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 3000f;
                        identity.Position = position - ((Vector3)(identity.Forward * num4));
                    }
                }
            }
            return identity;
        }
        public static Matrix GetSpawnTransform(App app, int databaseId, int fleetId, int shipIndex, int systemId, OrbitalObjectInfo[] objects)
		{
			GardenerInfo gardenerInfo = app.GameDatabase.GetGardenerInfos().FirstOrDefault((GardenerInfo x) => x.FleetId == fleetId);
			if (gardenerInfo == null)
			{
				return Matrix.Identity;
			}
			if (gardenerInfo.IsGardener)
			{
				return Gardeners.GetSpawnMatrixForGardener(app, systemId, objects);
			}
			if (gardenerInfo.GardenerFleetId != 0)
			{
				return Gardeners.GetSpawnMatrixForProteansWithGardener(app, databaseId, systemId, objects);
			}
			return Gardeners.GetSpawnMatrixForProteansAtHome(app, databaseId, shipIndex, systemId, objects);
		}
		private static Matrix GetSpawnMatrixForGardener(App app, int systemId, OrbitalObjectInfo[] objects)
		{
			StarSystemInfo starSystemInfo = app.GameDatabase.GetStarSystemInfo(systemId);
			if (starSystemInfo == null || starSystemInfo.IsDeepSpace)
			{
				return Matrix.Identity;
			}
			float num = 0f;
			float s = 5000f;
			Vector3 vector = Vector3.Zero;
			bool flag = false;
			for (int i = 0; i < objects.Length; i++)
			{
				OrbitalObjectInfo orbitalObjectInfo = objects[i];
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
				if (planetInfo != null)
				{
					ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo.ID);
					Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
					float lengthSquared = orbitalTransform.Position.LengthSquared;
					if (lengthSquared > num || (!flag && colonyInfoForPlanet != null))
					{
						num = lengthSquared;
						vector = orbitalTransform.Position;
						s = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 1500f;
						flag = (colonyInfoForPlanet != null);
					}
				}
			}
			if (num <= 0f)
			{
				return Matrix.Identity;
			}
			Vector3 vector2 = Vector3.Normalize(vector);
			return Matrix.CreateWorld(vector + vector2 * s, -vector2, Vector3.UnitY);
		}
		private static Matrix GetSpawnMatrixForProteansWithGardener(App app, int databaseId, int systemId, OrbitalObjectInfo[] objects)
		{
			Matrix spawnMatrixForGardener = Gardeners.GetSpawnMatrixForGardener(app, systemId, objects);
			StarSystemInfo starSystemInfo = app.GameDatabase.GetStarSystemInfo(systemId);
			float s = (starSystemInfo == null || starSystemInfo.IsDeepSpace) ? 1000f : 10000f;
			Vector3 vector = -spawnMatrixForGardener.Forward;
			Matrix mat = Matrix.CreateWorld(spawnMatrixForGardener.Position + vector * s, -vector, Vector3.UnitY);
			Vector3 position = mat.Position;
			Vector3? shipFleetPosition = app.GameDatabase.GetShipFleetPosition(databaseId);
			if (shipFleetPosition.HasValue)
			{
				position = Vector3.Transform(shipFleetPosition.Value, mat);
			}
			return Matrix.CreateWorld(position, mat.Forward, Vector3.UnitY);
		}
		private static Matrix GetSpawnMatrixForProteansAtHome(App app, int databaseId, int shipIndex, int systemId, OrbitalObjectInfo[] objects)
		{
			if (objects.Count<OrbitalObjectInfo>() == 0)
			{
				return Matrix.Identity;
			}
			GardenerInfo gardenerInfo = app.GameDatabase.GetGardenerInfos().FirstOrDefault((GardenerInfo x) => x.SystemId == systemId);
			int orbitalId = 0;
			if (gardenerInfo != null && gardenerInfo.ShipOrbitMap.ContainsKey(databaseId))
			{
				orbitalId = gardenerInfo.ShipOrbitMap[databaseId];
			}
			Matrix result = Matrix.Identity;
			if (orbitalId == 0)
			{
				int num = 0;
				for (int i = 0; i < objects.Length; i++)
				{
					OrbitalObjectInfo orbitalObjectInfo = objects[i];
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
					if (planetInfo != null && !(planetInfo.Type == "barren") && !(planetInfo.Type == "gaseous"))
					{
						num += (int)planetInfo.Size;
						if (shipIndex < num)
						{
							int num2 = num - shipIndex;
							float num3 = 360f / planetInfo.Size;
							result = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(num3 * (float)num2), 0f, 0f);
							float s = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 3000f;
							result.Position = app.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position + result.Forward * s;
							if (gardenerInfo != null)
							{
								app.Game.GameDatabase.InsertProteanShipOrbitMap(gardenerInfo.Id, databaseId, planetInfo.ID);
								break;
							}
							break;
						}
					}
				}
			}
			else
			{
				PlanetInfo planetInfo2 = app.GameDatabase.GetPlanetInfo(orbitalId);
				IEnumerable<KeyValuePair<int, int>> source = 
					from x in gardenerInfo.ShipOrbitMap
					where x.Value == orbitalId
					select x;
				source.Count<KeyValuePair<int, int>>();
				int num4 = 0;
				foreach (KeyValuePair<int, int> current in 
					from x in gardenerInfo.ShipOrbitMap
					where x.Value == orbitalId
					select x)
				{
					if (current.Key == databaseId)
					{
						break;
					}
					num4++;
				}
				float num5 = 360f / planetInfo2.Size;
				result = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(num5 * (float)num4), 0f, 0f);
				float s2 = StarSystemVars.Instance.SizeToRadius(planetInfo2.Size) + 3000f;
				result.Position = app.GameDatabase.GetOrbitalTransform(planetInfo2.ID).Position + result.Forward * s2;
			}
			return result;
		}
		public static List<PlanetInfo> GetGardenerPlanetsFromList(App app, int systemId)
		{
			GardenerInfo gardenerInfo = app.GameDatabase.GetGardenerInfos().FirstOrDefault((GardenerInfo x) => x.SystemId == systemId);
			List<PlanetInfo> list = new List<PlanetInfo>();
			if (gardenerInfo != null)
			{
				foreach (KeyValuePair<int, int> ship in gardenerInfo.ShipOrbitMap)
				{
					if (gardenerInfo.ShipOrbitMap.Where(delegate(KeyValuePair<int, int> x)
					{
						int arg_15_0 = x.Value;
						KeyValuePair<int, int> ship2 = ship;
						return arg_15_0 == ship2.Value;
					}).Count<KeyValuePair<int, int>>() != 0)
					{
						GameDatabase arg_A2_0 = app.GameDatabase;
						KeyValuePair<int, int> ship3 = ship;
						PlanetInfo planetInfo = arg_A2_0.GetPlanetInfo(ship3.Value);
						if (planetInfo != null && !list.Contains(planetInfo))
						{
							list.Add(planetInfo);
						}
					}
				}
			}
			return list;
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId)
		{
			if (gamedb.HasEndOfFleshExpansion() && SystemId == 0)
			{
				this.AddGardenerInstance(gamedb, assetdb);
				return;
			}
			this.AddProteanSystemInstance(gamedb, assetdb, SystemId);
		}
		private void AddProteanSystemInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId)
		{
			GardenerInfo gardenerInfo = new GardenerInfo();
			gardenerInfo.SystemId = SystemId;
			List<PlanetInfo> list = gamedb.GetStarSystemPlanetInfos(SystemId).ToList<PlanetInfo>();
			int num = 0;
			int num2 = 0;
			float num3 = gamedb.GetFactions().Average((FactionInfo x) => x.IdealSuitability);
			Random safeRandom = App.GetSafeRandom();
			GardenerGlobalData globalGardenerData = assetdb.GlobalGardenerData;
			foreach (PlanetInfo current in list)
			{
				if (!gamedb.GetOrbitalObjectInfo(current.ID).ParentID.HasValue && current.Type != "barren" && current.Type != "gaseous")
				{
					current.Biosphere = safeRandom.Next(globalGardenerData.MinBiosphere, globalGardenerData.MaxBiosphere);
					current.Suitability = num3;
					num++;
					gamedb.UpdatePlanet(current);
				}
			}
			if (num < globalGardenerData.MinPlanets)
			{
				int num4 = globalGardenerData.MinPlanets - num;
				int num5 = (
					from x in gamedb.GetStarSystemOrbitalObjectInfos(SystemId)
					where !x.ParentID.HasValue
					select x).Count<OrbitalObjectInfo>() + 1;
				for (int i = 0; i < num4; i++)
				{
					PlanetInfo pi = StarSystemHelper.InferPlanetInfo(new PlanetOrbit
					{
						OrbitNumber = num5 + i,
						Biosphere = new int?(safeRandom.Next(globalGardenerData.MinBiosphere, globalGardenerData.MaxBiosphere)),
						Suitability = new float?(num3)
					});
					gamedb.AddPlanetToSystem(SystemId, null, null, pi, new int?(num5 + i));
				}
			}
			List<PlanetInfo> list2 = new List<PlanetInfo>();
			foreach (PlanetInfo current2 in list)
			{
				if (current2.Type != "barren" && current2.Type != "gaseous")
				{
					num2 += (int)current2.Size;
					list2.Add(current2);
				}
			}
			int num6 = gamedb.InsertFleet(this.PlayerId, 0, SystemId, SystemId, "Protean Pod", FleetType.FL_NORMAL);
			gardenerInfo.FleetId = num6;
			gamedb.InsertGardenerInfo(gardenerInfo);
			int encounterIDAtSystem = gamedb.GetEncounterIDAtSystem(EasterEgg.EE_GARDENERS, gardenerInfo.SystemId);
			for (int j = 0; j < num2; j++)
			{
				int shipId = gamedb.InsertShip(num6, this._proteanDesignId, null, (ShipParams)0, null, 0);
				int num7 = 0;
				foreach (PlanetInfo current3 in list2)
				{
					num7 += (int)current3.Size;
					if (j < num7)
					{
						gamedb.InsertProteanShipOrbitMap(encounterIDAtSystem, shipId, current3.ID);
						break;
					}
				}
			}
		}
		private void AddGardenerInstance(GameDatabase gamedb, AssetDatabase assetdb)
		{
			GardenerInfo gardenerInfo = new GardenerInfo();
			gardenerInfo.FleetId = gamedb.InsertFleet(this.PlayerId, 0, 0, 0, "Gardener", FleetType.FL_NORMAL);
			gamedb.InsertShip(gardenerInfo.FleetId, this.GardenerDesignId, null, (ShipParams)0, null, 0);
			gardenerInfo.IsGardener = true;
			Vector3 v = default(Vector3);
			v.X = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(1E-06f, 1f);
			v.Y = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(1E-06f, 1f);
			v.Z = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(1E-06f, 1f);
			v.Normalize();
			Vector3 vector = v * -10f;
			Vector3 toCoords = v * 10f;
			int missionID = gamedb.InsertMission(gardenerInfo.FleetId, MissionType.RELOCATION, 0, 0, 0, 0, true, null);
			gamedb.InsertWaypoint(missionID, WaypointType.TravelTo, null);
			gamedb.InsertMoveOrder(gardenerInfo.FleetId, 0, vector, 0, toCoords);
			gardenerInfo.DeepSpaceSystemId = new int?(gamedb.InsertStarSystem(null, App.Localize("@UI_STARMAP_ENCOUNTER_DEEPSPACE"), null, "Deepspace", vector, false, false, null));
			OrbitalPath path = default(OrbitalPath);
			path.Scale = new Vector2(20000f, Ellipse.CalcSemiMinorAxis(20000f, 0f));
			path.InitialAngle = 0f;
			gardenerInfo.DeepSpaceOrbitalId = new int?(gamedb.InsertOrbitalObject(null, gardenerInfo.DeepSpaceSystemId.Value, path, "space"));
			gamedb.GetOrbitalTransform(gardenerInfo.DeepSpaceOrbitalId.Value);
			gamedb.InsertGardenerInfo(gardenerInfo);
			List<PlayerInfo> list = gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, current.ID) > 0)
				{
					gamedb.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_GARDENER,
						EventMessage = TurnEventMessage.EM_INCOMING_GARDENER,
						PlayerID = current.ID,
						TurnNumber = gamedb.GetTurnCount()
					});
				}
			}
		}
		public void UpdateTurn(GameSession game, int id)
		{
			GardenerInfo gi = game.GameDatabase.GetGardenerInfo(id);
			if (gi == null)
			{
				game.GameDatabase.RemoveEncounter(id);
				return;
			}
			if (gi.IsGardener)
			{
				FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(gi.FleetId);
				if (fleetInfo != null && fleetInfo.PlayerID != this.PlayerID)
				{
					return;
				}
				if (game.GameDatabase.GetMoveOrderInfoByFleetID(gi.FleetId) == null)
				{
					if (gi.DeepSpaceSystemId.HasValue)
					{
						game.GameDatabase.DestroyStarSystem(game, gi.DeepSpaceSystemId.Value);
					}
					if (gi.DeepSpaceOrbitalId.HasValue)
					{
						game.GameDatabase.RemoveOrbitalObject(gi.DeepSpaceOrbitalId.Value);
					}
					game.GameDatabase.RemoveEncounter(id);
					return;
				}
				FleetLocation fl = game.GameDatabase.GetFleetLocation(fleetInfo.ID, false);
				if (gi.DeepSpaceSystemId.HasValue)
				{
					if (!game.GameDatabase.GetStarSystemInfos().Any((StarSystemInfo x) => x.ID != gi.DeepSpaceSystemId.Value && (game.GameDatabase.GetStarSystemOrigin(x.ID) - fl.Coords).LengthSquared < 0.0001f))
					{
						game.GameDatabase.UpdateStarSystemOrigin(gi.DeepSpaceSystemId.Value, fl.Coords);
						return;
					}
				}
			}
			else
			{
				if (gi.SystemId == 0 && gi.GardenerFleetId != 0)
				{
					FleetInfo gardenerFleet = game.GameDatabase.GetFleetInfo(gi.GardenerFleetId);
					bool flag = gardenerFleet == null;
					if (!flag)
					{
						if (gi.TurnsToWait <= 0)
						{
							FleetInfo fleetInfo2 = game.GameDatabase.GetFleetInfo(gi.FleetId);
							if (fleetInfo2 != null)
							{
								if (game.GameDatabase.GetMoveOrderInfoByFleetID(gardenerFleet.ID) == null)
								{
									if (gardenerFleet.SystemID != fleetInfo2.SystemID)
									{
										game.GameDatabase.UpdateFleetLocation(fleetInfo2.ID, gardenerFleet.SystemID, null);
									}
								}
								else
								{
									GardenerInfo gardenerInfo = game.GameDatabase.GetGardenerInfos().FirstOrDefault((GardenerInfo x) => x.FleetId == gardenerFleet.ID);
									if (gardenerInfo != null && gardenerInfo.DeepSpaceSystemId.HasValue)
									{
										game.GameDatabase.UpdateFleetLocation(fleetInfo2.ID, gardenerInfo.DeepSpaceSystemId.Value, null);
									}
									else
									{
										game.GameDatabase.UpdateFleetLocation(fleetInfo2.ID, 0, null);
									}
								}
							}
						}
						else
						{
							if (game.GameDatabase.GetMoveOrderInfoByFleetID(gardenerFleet.ID) == null)
							{
								gi.TurnsToWait--;
								game.GameDatabase.UpdateGardenerInfo(gi);
							}
						}
					}
					if (flag)
					{
						game.GameDatabase.RemoveEncounter(id);
					}
				}
			}
		}
		public void SpawnProteanChaser(GameSession game, GardenerInfo gardener)
		{
			GardenerInfo gardenerInfo = new GardenerInfo();
			gardenerInfo.TurnsToWait = game.AssetDatabase.GlobalGardenerData.CatchUpDelay;
			gardenerInfo.GardenerFleetId = gardener.FleetId;
			gardenerInfo.FleetId = game.GameDatabase.InsertFleet(this.PlayerID, 0, 0, 0, "Protean Follow", FleetType.FL_NORMAL);
			game.GameDatabase.InsertGardenerInfo(gardenerInfo);
			int num = App.GetSafeRandom().NextInclusive(game.AssetDatabase.GlobalGardenerData.ProteanMobMin, game.AssetDatabase.GlobalGardenerData.ProteanMobMax);
			float num2 = 250000f;
			float maxValue = 2000f;
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < num; i++)
			{
				int shipID = game.GameDatabase.InsertShip(gardenerInfo.FleetId, this.ProteanDesignId, null, (ShipParams)0, null, 0);
				Vector3 vector = default(Vector3);
				bool flag = false;
				while (!flag)
				{
					flag = true;
					vector.Y = 0f;
					vector.X = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(0f, maxValue);
					vector.Z = (App.GetSafeRandom().CoinToss(50) ? -1f : 1f) * App.GetSafeRandom().NextInclusive(0f, maxValue);
					foreach (Vector3 current in list)
					{
						if ((current - vector).LengthSquared < num2)
						{
							flag = false;
							break;
						}
					}
				}
				list.Add(vector);
				game.GameDatabase.UpdateShipFleetPosition(shipID, new Vector3?(vector));
			}
		}
		public void HandleGardenerCaptured(GameSession game, GameDatabase gamedb, int playerId, int gardenerId)
		{
			GardenerInfo gardenerInfo = gamedb.GetGardenerInfo(gardenerId);
			if (gardenerInfo == null)
			{
				return;
			}
			FleetInfo fleetInfo = gamedb.GetFleetInfo(gardenerInfo.FleetId);
			if (fleetInfo == null)
			{
				return;
			}
			if (gardenerInfo.DeepSpaceSystemId.HasValue)
			{
				List<StationInfo> list = game.GameDatabase.GetStationForSystem(gardenerInfo.DeepSpaceSystemId.Value).ToList<StationInfo>();
				foreach (StationInfo current in list)
				{
					game.GameDatabase.DestroyStation(game, current.ID, 0);
				}
			}
			fleetInfo.PlayerID = playerId;
			gamedb.UpdateFleetInfo(fleetInfo);
			FleetLocation fleetLocation = gamedb.GetFleetLocation(fleetInfo.ID, false);
			List<StarSystemInfo> closestStars = EncounterTools.GetClosestStars(gamedb, fleetLocation.Coords);
			StarSystemInfo starSystemInfo = null;
			foreach (StarSystemInfo current2 in closestStars)
			{
				int? systemOwningPlayer = gamedb.GetSystemOwningPlayer(current2.ID);
				if (systemOwningPlayer.HasValue && systemOwningPlayer.Value == playerId)
				{
					starSystemInfo = current2;
					break;
				}
			}
			if (starSystemInfo != null)
			{
				MissionInfo missionByFleetID = gamedb.GetMissionByFleetID(fleetInfo.ID);
				if (missionByFleetID != null)
				{
					gamedb.RemoveMission(missionByFleetID.ID);
				}
				MoveOrderInfo moveOrderInfoByFleetID = gamedb.GetMoveOrderInfoByFleetID(fleetInfo.ID);
				if (moveOrderInfoByFleetID != null)
				{
					gamedb.RemoveMoveOrder(moveOrderInfoByFleetID.ID);
				}
				int missionID = gamedb.InsertMission(fleetInfo.ID, MissionType.RETURN, starSystemInfo.ID, 0, 0, 0, false, null);
				gamedb.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(starSystemInfo.ID));
				gamedb.InsertMoveOrder(fleetInfo.ID, 0, fleetLocation.Coords, starSystemInfo.ID, Vector3.Zero);
				this.SpawnProteanChaser(game, gardenerInfo);
			}
		}
	}
}
