using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class Swarmers
	{
		private const string FactionName = "swarm";
		private const string PlayerName = "Swarm";
		private const string PlayerAvatar = "\\base\\factions\\swarm\\avatars\\Swarm_Avatar.tga";
		private const int SpawnHiveDelay = 1;
		public const string SwarmHiveFleetName = "Swarm";
		public const string SwarmQueenFleetName = "Swarm Queen";
		private const string GuardianSectionName = "guardian.section";
		private const string SwarmerSectionName = "swarmer.section";
		private const string HiveStage1SectionName = "hive_stage1.section";
		private const string LarvalQueenSectionName = "larval_queen.section";
		private const string SwarmQueenSectionName = "swarm_queen.section";
		private int PlayerId = -1;
		private int _guardianDesignId;
		private int _swarmerDesignId;
		private int _hiveStage1DesignId;
		private int _larvalQueenDesignId;
		private int _swarmQueenDesignId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int HiveDesignID
		{
			get
			{
				return this._hiveStage1DesignId;
			}
		}
		public int GuardianDesignID
		{
			get
			{
				return this._guardianDesignId;
			}
		}
		public int SwarmerDesignID
		{
			get
			{
				return this._swarmerDesignId;
			}
		}
		public int LarvalQueenDesignID
		{
			get
			{
				return this._larvalQueenDesignId;
			}
		}
		public int SwarmQueenDesignID
		{
			get
			{
				return this._swarmQueenDesignId;
			}
		}
		private void InitDesigns(GameDatabase db)
		{
			List<DesignInfo> designs = db.GetDesignInfosForPlayer(this.PlayerId).ToList<DesignInfo>();
			this._guardianDesignId = db.AddOrGetEncounterDesignInfo(designs, this.PlayerId, "Swarm Guardian", "swarm", new string[]
			{
				"guardian.section"
			});
			this._swarmerDesignId = db.AddOrGetEncounterDesignInfo(designs, this.PlayerId, "Swarm Swarmer", "swarm", new string[]
			{
				"swarmer.section"
			});
			this._hiveStage1DesignId = db.AddOrGetEncounterDesignInfo(designs, this.PlayerId, "Swarm Hive", "swarm", new string[]
			{
				"hive_stage1.section"
			});
			this._larvalQueenDesignId = db.AddOrGetEncounterDesignInfo(designs, this.PlayerId, "Swarm Larval Queen", "swarm", new string[]
			{
				"larval_queen.section"
			});
			this._swarmQueenDesignId = db.AddOrGetEncounterDesignInfo(designs, this.PlayerId, "Swarm Queen", "swarm", new string[]
			{
				"swarm_queen.section"
			});
		}
		public static Swarmers InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Swarmers swarmers = new Swarmers();
			swarmers.PlayerId = gamedb.InsertPlayer("Swarm", "swarm", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\swarm\\avatars\\Swarm_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			List<PlayerInfo> list = gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list)
			{
				gamedb.ChangeDiplomacyState(current.ID, swarmers.PlayerId, DiplomacyState.WAR);
			}
			swarmers.InitDesigns(gamedb);
			return swarmers;
		}
		public static Swarmers ResumeEncounter(GameDatabase gamedb)
		{
			Swarmers swarmers = new Swarmers();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("Swarm"));
			swarmers.PlayerId = playerInfo.ID;
			swarmers.InitDesigns(gamedb);
			return swarmers;
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId, int OrbitId)
		{
			int orbitalId = OrbitId;
			if (gamedb.GetLargeAsteroidInfo(OrbitId) == null && gamedb.GetAsteroidBeltInfo(OrbitId) == null)
			{
				OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(OrbitId);
				StarSystemInfo starSystemInfo = gamedb.GetStarSystemInfo(SystemId);
				gamedb.RemoveOrbitalObject(orbitalObjectInfo.ID);
				if (!orbitalObjectInfo.ParentID.HasValue)
				{
					orbitalId = gamedb.InsertAsteroidBelt(null, orbitalObjectInfo.StarSystemID, orbitalObjectInfo.OrbitalPath, "Swarmed Asteroid Belt", App.GetSafeRandom().Next());
				}
				else
				{
					int num = (
						from x in gamedb.GetStarSystemOrbitalObjectInfos(SystemId)
						where !x.ParentID.HasValue
						select x).Count<OrbitalObjectInfo>();
					OrbitalPath path = gamedb.OrbitNumberToOrbitalPath(num + 1, StellarClass.Parse(starSystemInfo.StellarClass).Size, null);
					orbitalId = gamedb.InsertAsteroidBelt(null, orbitalObjectInfo.StarSystemID, path, "Swarmed Asteroid Belt", App.GetSafeRandom().Next());
				}
			}
			SwarmerInfo swarmerInfo = new SwarmerInfo();
			swarmerInfo.GrowthStage = 0;
			swarmerInfo.SystemId = SystemId;
			swarmerInfo.OrbitalId = orbitalId;
			swarmerInfo.HiveFleetId = new int?(gamedb.InsertFleet(this.PlayerId, 0, SystemId, SystemId, "Swarm", FleetType.FL_NORMAL));
			swarmerInfo.QueenFleetId = null;
			swarmerInfo.SpawnHiveDelay = 1;
			gamedb.InsertSwarmerInfo(swarmerInfo);
			gamedb.InsertShip(swarmerInfo.HiveFleetId.Value, this._hiveStage1DesignId, null, (ShipParams)0, null, 0);
		}
		public void UpdateTurn(GameSession game, int id)
		{
			Random safeRandom = App.GetSafeRandom();
			ShipInfo shipInfo = null;
			SwarmerInfo si = game.GameDatabase.GetSwarmerInfo(id);
			if (si == null)
			{
				game.GameDatabase.RemoveEncounter(id);
				return;
			}
			if (si.QueenFleetId.HasValue)
			{
				FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(si.QueenFleetId.Value);
				if (fleetInfo == null)
				{
					si.QueenFleetId = null;
					si.SpawnHiveDelay = 1;
				}
				else
				{
					if (fleetInfo.SystemID != 0 && fleetInfo.SystemID != si.SystemId)
					{
						if (si.SpawnHiveDelay <= 0)
						{
							List<OrbitalObjectInfo> source = game.GameDatabase.GetStarSystemOrbitalObjectInfos(fleetInfo.SystemID).ToList<OrbitalObjectInfo>();
							int iD = source.First((OrbitalObjectInfo x) => game.GameDatabase.GetAsteroidBeltInfo(x.ID) != null).ID;
							this.AddInstance(game.GameDatabase, game.AssetDatabase, fleetInfo.SystemID, iD);
							game.GameDatabase.RemoveFleet(fleetInfo.ID);
							si.QueenFleetId = null;
							si.SpawnHiveDelay = 1;
							List<int> list = game.GameDatabase.GetStandardPlayerIDs().ToList<int>();
							using (List<int>.Enumerator enumerator = list.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									int current = enumerator.Current;
									if (StarMap.IsInRange(game.GameDatabase, current, fleetInfo.SystemID))
									{
										game.GameDatabase.InsertTurnEvent(new TurnEvent
										{
											EventType = TurnEventType.EV_SWARM_INFESTATION,
											EventMessage = TurnEventMessage.EM_SWARM_INFESTATION,
											PlayerID = current,
											SystemID = fleetInfo.SystemID,
											TurnNumber = game.GameDatabase.GetTurnCount()
										});
									}
								}
								goto IL_260;
							}
						}
						si.SpawnHiveDelay--;
					}
				}
			}
			IL_260:
			if (si.HiveFleetId.HasValue)
			{
				if (game.GameDatabase.GetFleetInfo(si.HiveFleetId.Value) == null)
				{
					si.HiveFleetId = null;
				}
				else
				{
					List<ShipInfo> source2 = game.GameDatabase.GetShipInfoByFleetID(si.HiveFleetId.Value, true).ToList<ShipInfo>();
					shipInfo = source2.FirstOrDefault((ShipInfo x) => x.DesignInfo.ID == this._larvalQueenDesignId);
					int num = si.GrowthStage - game.AssetDatabase.GlobalSwarmerData.GrowthRateLarvaSpawn;
					if (num > 0 && !si.QueenFleetId.HasValue && shipInfo == null && safeRandom.CoinToss(Math.Max(0, num * 10)))
					{
						game.GameDatabase.InsertShip(si.HiveFleetId.Value, this._larvalQueenDesignId, "Swarm Larval Queen", (ShipParams)0, null, 0);
						si.GrowthStage = 0;
					}
				}
			}
			if (si.GrowthStage > game.AssetDatabase.GlobalSwarmerData.GrowthRateQueenSpawn && shipInfo != null)
			{
				List<StarSystemInfo> list2 = game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
				list2.RemoveAll((StarSystemInfo x) => !game.GameDatabase.GetStarSystemOrbitalObjectInfos(x.ID).Any((OrbitalObjectInfo y) => game.GameDatabase.GetAsteroidBeltInfo(y.ID) != null));
				List<SwarmerInfo> list3 = game.GameDatabase.GetSwarmerInfos().ToList<SwarmerInfo>();
				foreach (SwarmerInfo swarmer in list3)
				{
					int targetID = 0;
					int fleetSystem = 0;
					if (swarmer.QueenFleetId.HasValue)
					{
						MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(swarmer.QueenFleetId.Value);
						if (missionByFleetID != null)
						{
							targetID = missionByFleetID.TargetSystemID;
						}
						FleetInfo fleetInfo2 = game.GameDatabase.GetFleetInfo(swarmer.QueenFleetId.Value);
						if (fleetInfo2 != null)
						{
							fleetSystem = fleetInfo2.SystemID;
						}
					}
					list2.RemoveAll((StarSystemInfo x) => x.ID == swarmer.SystemId || x.ID == targetID || x.ID == fleetSystem);
				}
				list2 = (
					from x in list2
					orderby (game.GameDatabase.GetStarSystemOrigin(si.SystemId) - x.Origin).Length
					select x).ToList<StarSystemInfo>();
				if (list2.Count > 0)
				{
					StarSystemInfo starSystemInfo = list2[safeRandom.Next(0, Math.Min(Math.Max(0, list2.Count - 1), 3))];
					si.QueenFleetId = new int?(game.GameDatabase.InsertFleet(this.PlayerId, 0, si.SystemId, si.SystemId, "Swarm Queen", FleetType.FL_NORMAL));
					game.GameDatabase.RemoveShip(shipInfo.ID);
					game.GameDatabase.InsertShip(si.QueenFleetId.Value, this._swarmQueenDesignId, "Swarm Queen", (ShipParams)0, null, 0);
					int missionID = game.GameDatabase.InsertMission(si.QueenFleetId.Value, MissionType.STRIKE, starSystemInfo.ID, 0, 0, 0, true, null);
					game.GameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(starSystemInfo.ID));
				}
				si.GrowthStage = 0;
			}
			if (si.HiveFleetId.HasValue && !si.QueenFleetId.HasValue)
			{
				si.GrowthStage++;
			}
			if (!si.HiveFleetId.HasValue && !si.QueenFleetId.HasValue)
			{
				game.GameDatabase.RemoveEncounter(si.Id);
				return;
			}
			game.GameDatabase.UpdateSwarmerInfo(si);
		}
		public static void SetInitialSwarmerPosition(GameSession sim, SwarmerInfo si, int systemId)
		{
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(sim, systemId, 1f);
			if (combatZonesForSystem.Count == 0)
			{
				return;
			}
			FleetInfo fleetInfo = si.QueenFleetId.HasValue ? sim.GameDatabase.GetFleetInfo(si.QueenFleetId.Value) : null;
			if (fleetInfo == null)
			{
				return;
			}
			if (combatZonesForSystem.Count > 0)
			{
				CombatZonePositionInfo combatZonePositionInfo = combatZonesForSystem.Last<CombatZonePositionInfo>();
				Vector3 vector = combatZonePositionInfo.Center;
				float num = 0f;
				int num2 = (si != null) ? si.OrbitalId : 0;
				if (num2 == 0)
				{
					float num3 = 0f;
					List<OrbitalObjectInfo> list = sim.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId).ToList<OrbitalObjectInfo>();
					foreach (OrbitalObjectInfo current in list)
					{
						if (sim.GameDatabase.GetAsteroidBeltInfo(current.ID) != null)
						{
							Math.Max(num3, sim.GameDatabase.GetOrbitalTransform(current.ID).Position.LengthSquared);
						}
					}
					if (num3 > 0f)
					{
						num = (float)Math.Sqrt((double)num3);
					}
				}
				if (num2 != 0 && num == 0f)
				{
					Vector3 arg_124_0 = Vector3.Zero;
					Matrix orbitalTransform = sim.GameDatabase.GetOrbitalTransform(num2);
					List<LargeAsteroidInfo> list2 = sim.GameDatabase.GetLargeAsteroidsInAsteroidBelt(num2).ToList<LargeAsteroidInfo>();
					if (list2.Count > 0)
					{
						num2 = list2.First<LargeAsteroidInfo>().ID;
						orbitalTransform = sim.GameDatabase.GetOrbitalTransform(num2);
					}
					num = orbitalTransform.Position.Length;
				}
				Vector3 v = Vector3.UnitZ;
				if (fleetInfo.PreviousSystemID.HasValue && fleetInfo.PreviousSystemID != systemId)
				{
					Vector3 starSystemOrigin = sim.GameDatabase.GetStarSystemOrigin(systemId);
					Vector3 starSystemOrigin2 = sim.GameDatabase.GetStarSystemOrigin(fleetInfo.PreviousSystemID.Value);
					v = starSystemOrigin2 - starSystemOrigin;
					v.Y = 0f;
					v.Normalize();
				}
				vector = v * num;
				Vector3 forward = -vector;
				forward.Normalize();
				List<int> list3 = sim.GameDatabase.GetShipsByFleetID(fleetInfo.ID).ToList<int>();
				if (list3.Count > 0)
				{
					sim.GameDatabase.UpdateShipSystemPosition(list3.First<int>(), new Matrix?(Matrix.CreateWorld(vector, forward, Vector3.UnitY)));
				}
			}
		}
		public static void ClearTransform(GameDatabase db, SwarmerInfo si)
		{
			if (si != null && si.QueenFleetId.HasValue)
			{
				ShipInfo shipInfo = db.GetShipInfoByFleetID(si.QueenFleetId.Value, false).FirstOrDefault<ShipInfo>();
				if (shipInfo != null && db.GetShipSystemPosition(shipInfo.ID).HasValue)
				{
					db.UpdateShipSystemPosition(shipInfo.ID, null);
				}
			}
		}
		public static Matrix GetBaseEnemyFleetTrans(App app, int systemID)
		{
			return Swarmers.GetSpawnTransform(app, systemID);
		}
		public static Matrix GetSpawnTransform(App app, int systemID)
		{
			StarSystemInfo starSystemInfo = app.GameDatabase.GetStarSystemInfo(systemID);
			if (starSystemInfo == null || starSystemInfo.IsDeepSpace)
			{
				return Matrix.Identity;
			}
			SwarmerInfo swarmerInfo = app.GameDatabase.GetSwarmerInfos().FirstOrDefault((SwarmerInfo x) => x.SystemId == systemID);
			if (swarmerInfo != null && swarmerInfo.QueenFleetId.HasValue)
			{
				ShipInfo shipInfo = app.GameDatabase.GetShipInfoByFleetID(swarmerInfo.QueenFleetId.Value, false).FirstOrDefault<ShipInfo>();
				if (shipInfo != null)
				{
					Matrix? shipSystemPosition = app.GameDatabase.GetShipSystemPosition(shipInfo.ID);
					if (shipSystemPosition.HasValue)
					{
						return shipSystemPosition.Value;
					}
				}
			}
			int num = (swarmerInfo != null) ? swarmerInfo.OrbitalId : 0;
			if (num == 0)
			{
				Matrix result = Matrix.Identity;
				float num2 = 0f;
				List<OrbitalObjectInfo> list = app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemID).ToList<OrbitalObjectInfo>();
				foreach (OrbitalObjectInfo current in list)
				{
					if (app.GameDatabase.GetAsteroidBeltInfo(current.ID) != null)
					{
						Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(current.ID);
						float lengthSquared = orbitalTransform.Position.LengthSquared;
						if (lengthSquared > num2)
						{
							num2 = lengthSquared;
							result = orbitalTransform;
							num = current.ID;
						}
					}
				}
				if (num2 > 0f)
				{
					return result;
				}
			}
			if (num != 0)
			{
				Vector3 arg_17C_0 = Vector3.Zero;
				Matrix orbitalTransform2 = app.GameDatabase.GetOrbitalTransform(num);
				List<LargeAsteroidInfo> list2 = app.GameDatabase.GetLargeAsteroidsInAsteroidBelt(num).ToList<LargeAsteroidInfo>();
				if (list2.Count > 0)
				{
					num = list2.First<LargeAsteroidInfo>().ID;
					orbitalTransform2 = app.GameDatabase.GetOrbitalTransform(num);
					Vector3 position = orbitalTransform2.Position;
					float s = position.Normalize();
					Vector3 value = Vector3.Cross(position, Vector3.UnitY) * 1000f;
					orbitalTransform2.Position = Vector3.Normalize(value) * s;
				}
				return orbitalTransform2;
			}
			Random random = new Random();
			float yawRadians = MathHelper.DegreesToRadians(random.NextInclusive(0f, 360f));
			Matrix matrix = Matrix.CreateRotationYPR(yawRadians, 0f, 0f);
			return Matrix.CreateWorld(matrix.Forward * 50000f, -matrix.Forward, Vector3.UnitY);
		}
	}
}
