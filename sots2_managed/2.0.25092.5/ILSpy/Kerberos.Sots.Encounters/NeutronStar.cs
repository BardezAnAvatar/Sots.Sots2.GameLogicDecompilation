using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal class NeutronStar
	{
		private const string FactionName = "grandmenaces";
		private const string PlayerName = "NetronStar";
		private const string FleetName = "NetronStar";
		private const string PlayerAvatar = "\\base\\factions\\grandmenaces\\avatars\\Neutron_Star_Avatar.tga";
		private const string NeutronDesignFile = "neutron_star.section";
		private int PlayerId = -1;
		public static bool ForceEncounter;
		private int _neutronDesignId;
		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}
		public int NeutronDesignId
		{
			get
			{
				return this._neutronDesignId;
			}
		}
		public static NeutronStar InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			if (assetdb.GetFaction("grandmenaces") == null)
			{
				return null;
			}
			NeutronStar neutronStar = new NeutronStar();
			neutronStar.PlayerId = gamedb.InsertPlayer("NetronStar", "grandmenaces", null, assetdb.RandomEncounterPrimaryColor, new Vector3(0f), "", "\\base\\factions\\grandmenaces\\avatars\\Neutron_Star_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(neutronStar.PlayerId, "NeutronStar", new string[]
			{
				string.Format("factions\\{0}\\sections\\{1}", "grandmenaces", "neutron_star.section")
			});
			neutronStar._neutronDesignId = gamedb.InsertDesignByDesignInfo(design);
			foreach (int current in gamedb.GetPlayerIDs().ToList<int>())
			{
				gamedb.UpdateDiplomacyState(neutronStar.PlayerId, current, DiplomacyState.NEUTRAL, 1000, true);
			}
			return neutronStar;
		}
		public static NeutronStar ResumeEncounter(GameDatabase gamedb)
		{
			if (gamedb.AssetDatabase.GetFaction("grandmenaces") == null)
			{
				return null;
			}
			NeutronStar neutronStar = new NeutronStar();
			List<PlayerInfo> source = gamedb.GetPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = source.FirstOrDefault((PlayerInfo x) => !x.isStandardPlayer && x.Name.Contains("NetronStar"));
			if (playerInfo != null)
			{
				neutronStar.PlayerId = playerInfo.ID;
			}
			else
			{
				neutronStar.PlayerId = gamedb.InsertPlayer("NetronStar", "grandmenaces", null, new Vector3(0f), new Vector3(0f), "", "\\base\\factions\\grandmenaces\\avatars\\Neutron_Star_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			}
			List<DesignInfo> source2 = gamedb.GetDesignInfosForPlayer(neutronStar.PlayerId).ToList<DesignInfo>();
			DesignInfo designInfo = source2.FirstOrDefault((DesignInfo x) => x.DesignSections[0].FilePath.EndsWith("neutron_star.section"));
			if (designInfo == null)
			{
				DesignInfo design = new DesignInfo(neutronStar.PlayerId, "NeutronStar", new string[]
				{
					string.Format("factions\\{0}\\sections\\{1}", "grandmenaces", "neutron_star.section")
				});
				neutronStar._neutronDesignId = gamedb.InsertDesignByDesignInfo(design);
			}
			else
			{
				neutronStar._neutronDesignId = designInfo.ID;
			}
			return neutronStar;
		}
		public void UpdateTurn(GameSession game, int id)
		{
			NeutronStarInfo nsi = game.GameDatabase.GetNeutronStarInfo(id);
			FleetInfo fleetInfo = (nsi != null) ? game.GameDatabase.GetFleetInfo(nsi.FleetId) : null;
			if (fleetInfo == null)
			{
				game.GameDatabase.RemoveEncounter(id);
				return;
			}
			MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
			if (game.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID) != null)
			{
				FleetLocation fl = game.GameDatabase.GetFleetLocation(fleetInfo.ID, false);
				if (nsi.DeepSpaceSystemId.HasValue)
				{
					if (!game.GameDatabase.GetStarSystemInfos().Any((StarSystemInfo x) => x.ID != nsi.DeepSpaceSystemId.Value && (game.GameDatabase.GetStarSystemOrigin(x.ID) - fl.Coords).LengthSquared < 0.0001f))
					{
						game.GameDatabase.UpdateStarSystemOrigin(nsi.DeepSpaceSystemId.Value, fl.Coords);
					}
				}
				return;
			}
			if (missionByFleetID != null)
			{
				game.GameDatabase.RemoveMission(missionByFleetID.ID);
			}
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
			foreach (int current in game.GameDatabase.GetStandardPlayerIDs())
			{
				if (StarMap.IsInRange(game.GameDatabase, current, game.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, null))
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_NEUTRON_STAR_DESTROYED_SYSTEM,
						EventMessage = TurnEventMessage.EM_NEUTRON_STAR_DESTROYED_SYSTEM,
						SystemID = starSystemInfo.ID,
						PlayerID = this.PlayerID,
						TurnNumber = game.GameDatabase.GetTurnCount()
					});
				}
			}
			if (nsi.DeepSpaceSystemId.HasValue)
			{
				game.GameDatabase.RemoveFleet(fleetInfo.ID);
				game.GameDatabase.RemoveEncounter(id);
				game.GameDatabase.DestroyStarSystem(game, nsi.DeepSpaceSystemId.Value);
			}
			if (fleetInfo.SystemID != 0)
			{
				game.GameDatabase.DestroyStarSystem(game, starSystemInfo.ID);
			}
			if (game.App.CurrentState is StarMapState)
			{
				((StarMapState)game.App.CurrentState).ClearSelectedObject();
				((StarMapState)game.App.CurrentState).RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
			}
		}
		public void ExecuteInstance(GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			NeutronStarInfo neutronStarInfo = new NeutronStarInfo();
			neutronStarInfo.FleetId = gamedb.InsertFleet(this.PlayerId, 0, 0, 0, "NetronStar", FleetType.FL_NORMAL);
			neutronStarInfo.TargetSystemId = systemid;
			Vector3 starSystemOrigin = gamedb.GetStarSystemOrigin(neutronStarInfo.TargetSystemId);
			Vector3 travelDirection = this.GetTravelDirection(gamedb, starSystemOrigin, neutronStarInfo.TargetSystemId);
			int shipID = gamedb.InsertShip(neutronStarInfo.FleetId, this._neutronDesignId, null, (ShipParams)0, null, 0);
			gamedb.UpdateShipSystemPosition(shipID, new Matrix?(Matrix.Identity));
			Vector3 vector = starSystemOrigin - travelDirection * 20f;
			int missionID = gamedb.InsertMission(neutronStarInfo.FleetId, MissionType.STRIKE, neutronStarInfo.TargetSystemId, 0, 0, 0, true, null);
			gamedb.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(neutronStarInfo.TargetSystemId));
			gamedb.InsertMoveOrder(neutronStarInfo.FleetId, 0, vector, neutronStarInfo.TargetSystemId, Vector3.Zero);
			neutronStarInfo.DeepSpaceSystemId = new int?(gamedb.InsertStarSystem(null, App.Localize("@UI_STARMAP_ENCOUNTER_DEEPSPACE"), null, "Deepspace", vector, false, false, null));
			OrbitalPath path = default(OrbitalPath);
			path.Scale = new Vector2(20000f, Ellipse.CalcSemiMinorAxis(20000f, 0f));
			path.InitialAngle = 0f;
			neutronStarInfo.DeepSpaceOrbitalId = new int?(gamedb.InsertOrbitalObject(null, neutronStarInfo.DeepSpaceSystemId.Value, path, "space"));
			gamedb.GetOrbitalTransform(neutronStarInfo.DeepSpaceOrbitalId.Value);
			gamedb.InsertNeutronStarInfo(neutronStarInfo);
		}
		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			if (!targetSystem.HasValue)
			{
				List<ColonyInfo> source = gamedb.GetColonyInfos().ToList<ColonyInfo>();
				List<int> list = (
					from x in source
					select gamedb.GetOrbitalObjectInfo(x.OrbitalObjectID).StarSystemID).ToList<int>();
				List<NeutronStarInfo> neutronStars = new List<NeutronStarInfo>();
				list.RemoveAll((int x) => neutronStars.Any((NeutronStarInfo y) => y.TargetSystemId == x));
				if (list.Count == 0)
				{
					return;
				}
				targetSystem = new int?(safeRandom.Choose(list));
			}
			gamedb.InsertIncomingGM(targetSystem.Value, EasterEgg.GM_NEUTRON_STAR, gamedb.GetTurnCount() + 1);
			List<PlayerInfo> list2 = gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list2)
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, current.ID) > 0)
				{
					gamedb.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_INCOMING_NEUTRON_STAR,
						EventMessage = TurnEventMessage.EM_INCOMING_NEUTRON_STAR,
						PlayerID = current.ID,
						SystemID = targetSystem.Value,
						TurnNumber = gamedb.GetTurnCount()
					});
				}
			}
		}
		private Vector3 GetTravelDirection(GameDatabase gamedb, Vector3 systemOrigin, int targetSystemId)
		{
			return Vector3.Normalize(systemOrigin);
		}
		public static void GenerateMeteorAndCometEncounters(App game)
		{
			List<NeutronStarInfo> list = game.GameDatabase.GetNeutronStarInfos().ToList<NeutronStarInfo>();
			if (list.Count == 0)
			{
				return;
			}
            float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game.Game, list.First<NeutronStarInfo>().FleetId, false);
			List<StarSystemInfo> list2 = game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			float num = game.AssetDatabase.GlobalNeutronStarData.AffectRange * game.AssetDatabase.GlobalNeutronStarData.AffectRange;
			Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
			Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
			List<int> list3 = new List<int>();
			foreach (NeutronStarInfo current in list)
			{
				FleetLocation fleetLocation = game.GameDatabase.GetFleetLocation(current.FleetId, true);
				if (fleetLocation != null)
				{
					Vector3 coords = fleetLocation.Coords;
					Vector3 v = fleetLocation.Direction.HasValue ? (fleetLocation.Direction.Value * fleetTravelSpeed + fleetLocation.Coords) : fleetLocation.Coords;
					foreach (StarSystemInfo current2 in list2)
					{
						float lengthSquared = (coords - current2.Origin).LengthSquared;
						if (lengthSquared <= num)
						{
							if (dictionary2.ContainsKey(current2.ID))
							{
								dictionary2[current2.ID] = Math.Min(dictionary2[current2.ID], lengthSquared);
							}
							else
							{
								dictionary2.Add(current2.ID, lengthSquared);
							}
						}
						float lengthSquared2 = (v - current2.Origin).LengthSquared;
						if (lengthSquared2 <= num)
						{
							if (!list3.Contains(current2.ID))
							{
								list3.Add(current2.ID);
							}
							if (dictionary2.ContainsKey(current2.ID))
							{
								dictionary2[current2.ID] = Math.Min(dictionary2[current2.ID], lengthSquared);
							}
							else
							{
								dictionary2.Add(current2.ID, lengthSquared);
							}
						}
					}
				}
			}
			Random safeRandom = App.GetSafeRandom();
			foreach (KeyValuePair<int, float> inRangeSys in dictionary2)
			{
				GameDatabase arg_2A3_0 = game.GameDatabase;
				KeyValuePair<int, float> inRangeSys7 = inRangeSys;
				List<ColonyInfo> list4 = arg_2A3_0.GetColonyInfosForSystem(inRangeSys7.Key).ToList<ColonyInfo>();
				List<int> list5 = new List<int>();
				foreach (ColonyInfo current3 in list4)
				{
					Player player = game.GetPlayer(current3.PlayerID);
					if (player != null && player.IsStandardPlayer)
					{
						if (list3.Contains(current3.CachedStarSystemID))
						{
							if (!dictionary.ContainsKey(player.ID))
							{
								dictionary.Add(current3.PlayerID, new List<int>());
							}
							if (!dictionary[current3.PlayerID].Contains(current3.CachedStarSystemID))
							{
								dictionary[current3.PlayerID].Add(current3.CachedStarSystemID);
							}
						}
						if (!player.IsAI() && !list5.Contains(player.ID))
						{
							list5.Add(player.ID);
						}
					}
				}
				Dictionary<int, float> arg_3B3_0 = dictionary2;
				KeyValuePair<int, float> inRangeSys2 = inRangeSys;
				if (arg_3B3_0[inRangeSys2.Key] <= num && list5.Count != 0)
				{
					if (!list5.Any((int x) => game.GameDatabase.GetIncomingRandomsForPlayerThisTurn(x).Any(delegate(IncomingRandomInfo y)
					{
						int arg_14_0 = y.SystemId;
						KeyValuePair<int, float> inRangeSys6 = inRangeSys;
						return arg_14_0 == inRangeSys6.Key;
					})))
					{
						KeyValuePair<int, float> inRangeSys3 = inRangeSys;
						float num2 = (float)Math.Sqrt((double)inRangeSys3.Value);
						float num3 = Math.Min(num2 / game.AssetDatabase.GlobalNeutronStarData.AffectRange, 1f);
						int odds = (int)(90f * num3) + 10;
						if (safeRandom.CoinToss(odds))
						{
							if (safeRandom.CoinToss(game.AssetDatabase.GlobalNeutronStarData.MeteorRatio))
							{
								float intensity = (game.AssetDatabase.GlobalNeutronStarData.MaxMeteorIntensity - 1f) * num3 + 1f;
								if (game.Game.ScriptModules.MeteorShower != null)
								{
									MeteorShower arg_4DB_0 = game.Game.ScriptModules.MeteorShower;
									GameSession arg_4DB_1 = game.Game;
									KeyValuePair<int, float> inRangeSys4 = inRangeSys;
									arg_4DB_0.ExecuteEncounter(arg_4DB_1, inRangeSys4.Key, intensity, true);
								}
							}
							else
							{
								if (game.Game.ScriptModules.Comet != null)
								{
									Comet arg_538_0 = game.Game.ScriptModules.Comet;
									GameDatabase arg_538_1 = game.GameDatabase;
									AssetDatabase arg_538_2 = game.AssetDatabase;
									KeyValuePair<int, float> inRangeSys5 = inRangeSys;
									arg_538_0.ExecuteInstance(arg_538_1, arg_538_2, inRangeSys5.Key);
								}
							}
						}
					}
				}
			}
			foreach (KeyValuePair<int, List<int>> current4 in dictionary)
			{
				if (current4.Value.Count != 0)
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_NEUTRON_STAR_NEARBY,
						EventMessage = TurnEventMessage.EM_NEUTRON_STAR_NEARBY,
						PlayerID = current4.Key,
						NumShips = current4.Value.Count,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = true
					});
				}
			}
		}
		public static Matrix GetBaseEnemyFleetTrans()
		{
			return Matrix.Identity;
		}
		public static Matrix GetSpawnTransform()
		{
			return Matrix.Identity;
		}
		public static bool IsPlayerSystemsInNeutronStarEffectRanges(GameSession game, int playerId, int systemId)
		{
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			if (starSystemInfo == null)
			{
				return false;
			}
			List<NeutronStarInfo> list = game.GameDatabase.GetNeutronStarInfos().ToList<NeutronStarInfo>();
			if (list.Count == 0)
			{
				return false;
			}
			bool flag = false;
			List<ColonyInfo> list2 = game.GameDatabase.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list2)
			{
				if (current.PlayerID == playerId)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
            float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, list.First<NeutronStarInfo>().FleetId, false);
			float num = game.GameDatabase.AssetDatabase.GlobalNeutronStarData.AffectRange * game.GameDatabase.AssetDatabase.GlobalNeutronStarData.AffectRange;
			foreach (NeutronStarInfo current2 in list)
			{
				FleetLocation fleetLocation = game.GameDatabase.GetFleetLocation(current2.FleetId, true);
				if (fleetLocation != null)
				{
					Vector3 v = fleetLocation.Direction.HasValue ? (fleetLocation.Direction.Value * fleetTravelSpeed + fleetLocation.Coords) : fleetLocation.Coords;
					if ((v - starSystemInfo.Origin).LengthSquared <= num)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
