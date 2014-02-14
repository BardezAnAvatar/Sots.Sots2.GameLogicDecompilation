using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class SystemKiller
	{
		private const string FactionName = "grandmenaces";
		private const string PlayerName = "System Killer";
		private const string PlayerAvatar = "\\base\\factions\\grandmenaces\\avatars\\Systemkiller_Avatar.tga";
		private const string FleetName = "System Killer";
		private const string SystemKillerDesignFile = "systemkiller.section";
		private int PlayerId = -1;
		private int _systemKillerDesignId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int SystemKillerDesignId
		{
			get
			{
				return this._systemKillerDesignId;
			}
		}
		public static SystemKiller InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			SystemKiller systemKiller = new SystemKiller();
			systemKiller.PlayerId = gamedb.InsertPlayer("System Killer", "grandmenaces", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\grandmenaces\\avatars\\Systemkiller_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(systemKiller.PlayerId, "System Killer", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "grandmenaces", "systemkiller.section")
			});
			systemKiller._systemKillerDesignId = gamedb.InsertDesignByDesignInfo(design);
			return systemKiller;
		}
		public static SystemKiller ResumeEncounter(GameDatabase gamedb)
		{
			SystemKiller systemKiller = new SystemKiller();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("System Killer"));
			systemKiller.PlayerId = playerInfo.ID;
			List<DesignInfo> source2 = gamedb.GetDesignInfosForPlayer(systemKiller.PlayerId).ToList<DesignInfo>();
			systemKiller._systemKillerDesignId = source2.First((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("systemkiller.section")).ID;
			return systemKiller;
		}
		public void ExecuteInstance(GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(systemid);
			StarSystemInfo starSystemInfo2 = EncounterTools.GetClosestStars(gamedb, starSystemInfo).Last<StarSystemInfo>();
			int num = gamedb.InsertFleet(this.PlayerId, 0, 0, 0, "System Killer", FleetType.FL_NORMAL);
			gamedb.InsertShip(num, this._systemKillerDesignId, null, (ShipParams)0, null, 0);
			int missionID = gamedb.InsertMission(num, MissionType.STRIKE, starSystemInfo.ID, 0, 0, 0, false, null);
			gamedb.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(starSystemInfo.ID));
			gamedb.InsertMoveOrder(num, 0, starSystemInfo.Origin - Vector3.Normalize(starSystemInfo2.Origin - starSystemInfo.Origin) * 10f, starSystemInfo.ID, Vector3.Zero);
			gamedb.InsertSystemKillerInfo(new SystemKillerInfo
			{
				Target = starSystemInfo2.Origin,
				FleetId = new int?(num)
			});
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			List<KeyValuePair<StarSystemInfo, Vector3>> outlyingStars = EncounterTools.GetOutlyingStars(gamedb);
			StarSystemInfo starSystemInfo;
			if (targetSystem.HasValue)
			{
				starSystemInfo = gamedb.GetStarSystemInfo(targetSystem.Value);
			}
			else
			{
				int num = outlyingStars.Count / 3;
				if (num <= 0)
				{
					return;
				}
				List<KeyValuePair<StarSystemInfo, Vector3>> range = outlyingStars.GetRange(0, num);
				starSystemInfo = safeRandom.Choose(range).Key;
			}
			gamedb.InsertIncomingGM(starSystemInfo.ID, EasterEgg.GM_SYSTEM_KILLER, gamedb.GetTurnCount() + 1);
			List<PlayerInfo> list = gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, current.ID) > 0)
				{
					gamedb.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_SYSTEMKILLER,
						EventMessage = TurnEventMessage.EM_INCOMING_SYSTEMKILLER,
						PlayerID = current.ID,
						TurnNumber = gamedb.GetTurnCount()
					});
				}
			}
		}
		public void UpdateTurn(GameSession game, int id)
		{
			SystemKillerInfo si = game.GameDatabase.GetSystemKillerInfo(id);
			FleetInfo fleetInfo = si.FleetId.HasValue ? game.GameDatabase.GetFleetInfo(si.FleetId.Value) : null;
			if (fleetInfo == null)
			{
				game.GameDatabase.RemoveEncounter(si.Id);
				return;
			}
			MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
			StarSystemInfo systemInfo = game.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
			if (game.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID) != null)
			{
				return;
			}
			List<OrbitalObjectInfo> list = game.GameDatabase.GetStarSystemOrbitalObjectInfos(fleetInfo.SystemID).ToList<OrbitalObjectInfo>();
			list.RemoveAll((OrbitalObjectInfo x) => game.GameDatabase.GetAsteroidBeltInfo(x.ID) != null || game.GameDatabase.GetLargeAsteroidInfo(x.ID) != null || game.GameDatabase.GetStationInfo(x.ID) != null);
			if (list.Any<OrbitalObjectInfo>())
			{
				if (!game.isHostilesAtSystem(this.PlayerId, fleetInfo.SystemID))
				{
					OrbitalObjectInfo orbitalObjectInfo = (
						from x in list
						orderby x.OrbitalPath.Scale.Length
						select x).First<OrbitalObjectInfo>();
					game.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_PLANET_DESTROYED,
						EventMessage = TurnEventMessage.EM_PLANET_DESTROYED,
						PlayerID = this.PlayerId,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = orbitalObjectInfo.ID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					game.GameDatabase.DestroyOrbitalObject(game, orbitalObjectInfo.ID);
					return;
				}
			}
			else
			{
				if (missionByFleetID != null)
				{
					game.GameDatabase.RemoveMission(missionByFleetID.ID);
				}
				List<StarSystemInfo> closestStars = EncounterTools.GetClosestStars(game.GameDatabase, fleetInfo.SystemID);
				double maxCos = Math.Cos(1.0471975511965976);
				StarSystemInfo starSystemInfo = closestStars.FirstOrDefault((StarSystemInfo x) => (double)Vector3.Dot(Vector3.Normalize(si.Target - systemInfo.Origin), Vector3.Normalize(x.Origin - systemInfo.Origin)) > maxCos);
				if (starSystemInfo == null)
				{
					foreach (int current in game.GameDatabase.GetStandardPlayerIDs())
					{
						if (StarMap.IsInRange(game.GameDatabase, current, game.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, null))
						{
							game.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_SYS_KILLER_LEAVING,
								EventMessage = TurnEventMessage.EM_SYS_KILLER_LEAVING,
								PlayerID = this.PlayerID,
								TurnNumber = game.GameDatabase.GetTurnCount()
							});
						}
					}
					game.GameDatabase.RemoveFleet(fleetInfo.ID);
					game.GameDatabase.RemoveEncounter(si.Id);
				}
				else
				{
					int missionID = game.GameDatabase.InsertMission(fleetInfo.ID, MissionType.STRIKE, starSystemInfo.ID, 0, 0, 0, false, null);
					game.GameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(starSystemInfo.ID));
					game.GameDatabase.InsertMoveOrder(fleetInfo.ID, 0, game.GameDatabase.GetStarSystemOrigin(fleetInfo.SystemID), starSystemInfo.ID, Vector3.Zero);
					game.GameDatabase.UpdateFleetLocation(fleetInfo.ID, 0, null);
				}
				if (systemInfo != null)
				{
					game.GameDatabase.DestroyStarSystem(game, systemInfo.ID);
				}
				if (game.App.CurrentState is StarMapState)
				{
					((StarMapState)game.App.CurrentState).ClearSelectedObject();
					((StarMapState)game.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
				}
			}
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, int skFleetId, Kerberos.Sots.GameStates.StarSystem starSystem, OrbitalObjectInfo[] orbitalObjects)
		{
			return SystemKiller.GetSpawnTransform(app, skFleetId, starSystem, orbitalObjects);
		}
		public static Matrix GetSpawnTransform(App app, int fleetID, Kerberos.Sots.GameStates.StarSystem starSystem, OrbitalObjectInfo[] orbitalObjects)
		{
			DesignInfo designInfo = app.GameDatabase.GetDesignInfo(app.Game.ScriptModules.SystemKiller.SystemKillerDesignId);
			float num = 50000f;
			if (designInfo != null)
			{
				float num2 = designInfo.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.Maneuvering.LinearSpeed);
				num = num2 * 120f;
			}
			float num3 = 0f;
			float num4 = 5000f;
			Vector3 vector = Vector3.Zero;
			Vector3 vector2 = -Vector3.UnitZ;
			int num5 = 0;
			for (int i = 0; i < orbitalObjects.Length; i++)
			{
				OrbitalObjectInfo orbitalObjectInfo = orbitalObjects[i];
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
				if (planetInfo != null)
				{
					Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
					float lengthSquared = orbitalTransform.Position.LengthSquared;
					if (lengthSquared > num3)
					{
						num3 = lengthSquared;
						vector = orbitalTransform.Position;
						vector2 = -orbitalTransform.Position;
						num4 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + num;
						num5 = orbitalObjectInfo.ID;
					}
				}
			}
			if (num5 != 0)
			{
				vector2.Normalize();
				IEnumerable<OrbitalObjectInfo> moons = app.GameDatabase.GetMoons(num5);
				if (moons.Count<OrbitalObjectInfo>() > 0)
				{
					float num6 = 5000f;
					Vector3 v = Vector3.Zero;
					Vector3 vector3 = -Vector3.UnitZ;
					int num7 = 0;
					num3 = 0f;
					foreach (OrbitalObjectInfo current in moons)
					{
						PlanetInfo planetInfo2 = app.GameDatabase.GetPlanetInfo(current.ID);
						if (planetInfo2 != null)
						{
							Matrix orbitalTransform2 = app.GameDatabase.GetOrbitalTransform(current.ID);
							float lengthSquared2 = orbitalTransform2.Position.LengthSquared;
							if (lengthSquared2 > num3)
							{
								num3 = lengthSquared2;
								v = orbitalTransform2.Position;
								vector3 = -orbitalTransform2.Position;
								num6 = StarSystemVars.Instance.SizeToRadius(planetInfo2.Size) + num;
								num7 = current.ID;
							}
						}
					}
					if (num7 != 0)
					{
						Vector3 vector4 = Vector3.Normalize(v - vector);
						if (Vector3.Dot(vector4, vector2) > 0f)
						{
							vector3.Normalize();
							Vector3 position = v - vector3 * (num6 + num);
							return Matrix.CreateWorld(position, vector3, Vector3.UnitY);
						}
						Vector3 vector5 = Vector3.Cross(vector2, Vector3.UnitY);
						float num8 = (Vector3.Dot(vector5, vector4) > 0f) ? 1f : -1f;
						Vector3 position2 = v + vector5 * ((num6 + num) * num8);
						return Matrix.CreateWorld(position2, vector3, Vector3.UnitY);
					}
				}
				Vector3 vector6 = vector - vector2 * num4;
				if (starSystem != null)
				{
					Vector3 vector7 = -Vector3.UnitZ;
					float num9 = -1f;
					List<Ship> stationsAroundPlanet = starSystem.GetStationsAroundPlanet(num5);
					foreach (Ship current2 in stationsAroundPlanet)
					{
						Vector3 vector8 = vector - current2.Maneuvering.Position;
						vector8.Normalize();
						float num10 = Vector3.Dot(vector8, vector2);
						if (num10 > 0.75f && num10 > num9)
						{
							num9 = num10;
							vector7 = vector8;
						}
					}
					if (num9 > 0f)
					{
						Matrix matrix = Matrix.CreateWorld(Vector3.Zero, vector7, Vector3.UnitY);
						Vector3 v2 = vector + matrix.Right * num4;
						Vector3 v3 = vector - matrix.Right * num4;
						if ((vector6 - v2).LengthSquared < (vector6 - v3).LengthSquared)
						{
							vector2 = (vector7 + matrix.Right) * 0.5f;
						}
						else
						{
							vector2 = (vector7 - matrix.Right) * 0.5f;
						}
						vector2.Normalize();
						vector6 = vector - vector2 * num4;
					}
				}
				return Matrix.CreateWorld(vector6, vector2, Vector3.UnitY);
			}
			MoveOrderInfo moveOrderInfoByFleetID = app.GameDatabase.GetMoveOrderInfoByFleetID(fleetID);
			if (moveOrderInfoByFleetID != null)
			{
				Vector3 vector9 = moveOrderInfoByFleetID.ToCoords - moveOrderInfoByFleetID.FromCoords;
				vector9.Y = 0f;
				float num11 = vector9.Normalize();
				if (num11 < 1.401298E-45f)
				{
					vector9 = -Vector3.UnitZ;
				}
				num4 = 0f;
				if (starSystem != null)
				{
					num4 = Math.Min((starSystem.GetBaseOffset() + Kerberos.Sots.GameStates.StarSystem.CombatZoneMapRadii[Math.Max(starSystem.GetFurthestRing() - 1, 1)]) * 5700f, num);
				}
				Vector3 position3 = vector9 * -num4;
				return Matrix.CreateWorld(position3, vector9, Vector3.UnitY);
			}
			num4 = ((starSystem != null) ? starSystem.GetStarRadius() : 5000f);
			Vector3 vector10 = Vector3.UnitZ * -(num4 + num);
			return Matrix.CreateWorld(vector10, Vector3.Normalize(-vector10), Vector3.UnitY);
		}
	}
}
