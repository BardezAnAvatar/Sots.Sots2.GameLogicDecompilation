using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal static class CombatSimulatorRandoms
	{
		private static Random _rand = new Random();
		public static bool Simulate(GameSession game, int systemId, List<FleetInfo> fleets)
		{
			if (ScriptHost.AllowConsole)
			{
				App.Log.Trace(string.Format("Simulating RANDOM AI combat at: {0}", systemId), "combat");
			}
			bool flag = true;
			Dictionary<PlayerInfo, List<FleetInfo>> dictionary = new Dictionary<PlayerInfo, List<FleetInfo>>();
			foreach (FleetInfo current in fleets)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(current.PlayerID);
				if (playerInfo != null)
				{
					if (!dictionary.ContainsKey(playerInfo))
					{
						dictionary.Add(playerInfo, new List<FleetInfo>());
						if (!playerInfo.isStandardPlayer && !CombatSimulatorRandoms.IsValidSimulateEncounterPlayer(game, playerInfo.ID))
						{
							flag = false;
						}
					}
					dictionary[playerInfo].Add(current);
				}
			}
			List<ColonyInfo> list = new List<ColonyInfo>();
			PlanetInfo[] starSystemPlanetInfos = game.GameDatabase.GetStarSystemPlanetInfos(systemId);
			if (starSystemPlanetInfos != null)
			{
				PlanetInfo[] array = starSystemPlanetInfos;
				for (int i = 0; i < array.Length; i++)
				{
					PlanetInfo planetInfo = array[i];
					ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
					if (colonyInfoForPlanet != null && colonyInfoForPlanet.IsIndependentColony(game.App))
					{
						list.Add(colonyInfoForPlanet);
					}
				}
			}
			if (flag || list.Count != 0)
			{
				if (dictionary.Keys.Count((PlayerInfo x) => x.isStandardPlayer) == 1)
				{
					bool result = false;
					PlayerInfo playerInfo2 = dictionary.Keys.First((PlayerInfo x) => x.isStandardPlayer);
					List<FleetInfo> aiPlayerFleets = dictionary[playerInfo2];
					if (game.ScriptModules.Swarmers != null)
					{
						PlayerInfo playerInfo3 = dictionary.Keys.FirstOrDefault((PlayerInfo x) => x.ID == game.ScriptModules.Swarmers.PlayerID);
						if (playerInfo3 != null)
						{
							FleetInfo fleetInfo = null;
							FleetInfo fleetInfo2 = null;
							foreach (FleetInfo current2 in dictionary[playerInfo3])
							{
								List<ShipInfo> list2 = game.GameDatabase.GetShipInfoByFleetID(current2.ID, false).ToList<ShipInfo>();
								if (list2.Count != 0)
								{
									if (list2.Any((ShipInfo x) => x.DesignID == game.ScriptModules.Swarmers.SwarmQueenDesignID))
									{
										fleetInfo = current2;
									}
									else
									{
										fleetInfo2 = current2;
									}
								}
							}
							if (fleetInfo != null)
							{
								CombatSimulatorRandoms.SimulateSwarmerQueen(CombatSimulatorRandoms._rand, game, systemId, fleetInfo, playerInfo2.ID, aiPlayerFleets);
							}
							if (fleetInfo2 != null)
							{
								CombatSimulatorRandoms.SimulateSwarmerNest(CombatSimulatorRandoms._rand, game, systemId, fleetInfo2, playerInfo2.ID, aiPlayerFleets);
							}
							result = true;
						}
					}
					if (game.ScriptModules.Gardeners != null)
					{
						PlayerInfo playerInfo4 = dictionary.Keys.FirstOrDefault((PlayerInfo x) => x.ID == game.ScriptModules.Gardeners.PlayerID);
						if (playerInfo4 != null)
						{
							FleetInfo fleetInfo3 = dictionary[playerInfo4].FirstOrDefault<FleetInfo>();
							if (fleetInfo3 != null)
							{
								CombatSimulatorRandoms.SimulateProteans(CombatSimulatorRandoms._rand, game, systemId, fleetInfo3, playerInfo2.ID, aiPlayerFleets);
							}
							result = true;
						}
					}
					if (game.ScriptModules.AsteroidMonitor != null)
					{
						PlayerInfo playerInfo5 = dictionary.Keys.FirstOrDefault((PlayerInfo x) => x.ID == game.ScriptModules.AsteroidMonitor.PlayerID);
						if (playerInfo5 != null)
						{
							FleetInfo fleetInfo4 = dictionary[playerInfo5].FirstOrDefault<FleetInfo>();
							if (fleetInfo4 != null)
							{
								CombatSimulatorRandoms.SimulateAsteroidMonitors(CombatSimulatorRandoms._rand, game, systemId, fleetInfo4, playerInfo2.ID, aiPlayerFleets);
							}
							result = true;
						}
					}
					if (game.ScriptModules.MorrigiRelic != null)
					{
						PlayerInfo playerInfo6 = dictionary.Keys.FirstOrDefault((PlayerInfo x) => x.ID == game.ScriptModules.MorrigiRelic.PlayerID);
						if (playerInfo6 != null)
						{
							FleetInfo fleetInfo5 = dictionary[playerInfo6].FirstOrDefault<FleetInfo>();
							if (fleetInfo5 != null)
							{
								CombatSimulatorRandoms.SimulateMorrigiRelics(CombatSimulatorRandoms._rand, game, systemId, fleetInfo5, playerInfo2.ID, aiPlayerFleets);
							}
							result = true;
						}
					}
					if (game.ScriptModules.Pirates != null)
					{
						PlayerInfo playerInfo7 = dictionary.Keys.FirstOrDefault((PlayerInfo x) => x.ID == game.ScriptModules.Pirates.PlayerID);
						if (playerInfo7 != null)
						{
							FleetInfo fleetInfo6 = dictionary[playerInfo7].FirstOrDefault<FleetInfo>();
							if (fleetInfo6 != null)
							{
								CombatSimulatorRandoms.SimulatePirateBase(CombatSimulatorRandoms._rand, game, systemId, fleetInfo6, playerInfo2.ID, aiPlayerFleets);
							}
							result = true;
						}
					}
					foreach (ColonyInfo current3 in list)
					{
						Dictionary<PlayerInfo, List<FleetInfo>> dictionary2 = new Dictionary<PlayerInfo, List<FleetInfo>>();
						foreach (KeyValuePair<PlayerInfo, List<FleetInfo>> current4 in dictionary)
						{
							if (current4.Key.isStandardPlayer && game.GameDatabase.GetDiplomacyStateBetweenPlayers(current3.PlayerID, current4.Key.ID) == DiplomacyState.WAR)
							{
								dictionary2.Add(current4.Key, current4.Value);
							}
						}
						if (dictionary2.Keys.Count != 0)
						{
							CombatSimulatorRandoms.SimulateIndyColony(CombatSimulatorRandoms._rand, game, systemId, current3, dictionary2);
							result = true;
						}
					}
					return result;
				}
			}
			return false;
		}
		private static bool IsValidSimulateEncounterPlayer(GameSession game, int playerId)
		{
			return game.ScriptModules != null && ((game.ScriptModules.Swarmers != null && game.ScriptModules.Swarmers.PlayerID == playerId) || (game.ScriptModules.Gardeners != null && game.ScriptModules.Gardeners.PlayerID == playerId) || (game.ScriptModules.AsteroidMonitor != null && game.ScriptModules.AsteroidMonitor.PlayerID == playerId) || (game.ScriptModules.MorrigiRelic != null && game.ScriptModules.MorrigiRelic.PlayerID == playerId) || (game.ScriptModules.Pirates != null && game.ScriptModules.Pirates.PlayerID == playerId));
		}
		private static Dictionary<FleetInfo, List<ShipInfo>> GetShipsInFleets(GameSession game, List<FleetInfo> playerFleets)
		{
			Dictionary<FleetInfo, List<ShipInfo>> dictionary = new Dictionary<FleetInfo, List<ShipInfo>>();
			foreach (FleetInfo current in playerFleets)
			{
				if (current.Type == FleetType.FL_NORMAL || (current.Type & FleetType.FL_ALL_COMBAT) != (FleetType)0)
				{
					dictionary.Add(current, new List<ShipInfo>());
					List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(current.ID, true).ToList<ShipInfo>();
					foreach (ShipInfo current2 in list)
					{
						if (!ShipSectionAsset.IsBattleRiderClass(current2.DesignInfo.GetRealShipClass().Value))
						{
							dictionary[current].Add(current2);
						}
					}
				}
			}
			return dictionary;
		}
		private static void SimulateSwarmerQueen(Random rand, GameSession game, int systemId, FleetInfo randomsFleet, int aiPlayerID, List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(4, 6);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			shipsInFleets.Sum((KeyValuePair<FleetInfo, List<ShipInfo>> x) => x.Value.Count);
			if (shipsInFleets.Sum(delegate(KeyValuePair<FleetInfo, List<ShipInfo>> x)
			{
				if (x.Value.Count <= 0)
				{
					return 0;
				}
				return x.Value.Sum(delegate(ShipInfo y)
				{
					if (y.DesignInfo == null)
					{
						return 0;
					}
					return CombatAI.GetShipStrength(y.DesignInfo.Class) / 3;
				});
			}) <= 4)
			{
				foreach (KeyValuePair<FleetInfo, List<ShipInfo>> current in shipsInFleets)
				{
					foreach (ShipInfo current2 in current.Value)
					{
						game.GameDatabase.RemoveShip(current2.ID);
					}
					CombatSimulatorRandoms.FleetDestroyed(game, randomsFleet.PlayerID, current.Key, null);
					game.GameDatabase.RemoveFleet(current.Key.ID);
				}
				List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
				if (list.Count == 0)
				{
					game.GameDatabase.RemoveFleet(randomsFleet.ID);
				}
			}
			else
			{
				CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
				List<ShipInfo> list2 = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
				if (list2.Count > 0)
				{
					CombatSimulatorRandoms.FleetDestroyed(game, aiPlayerID, randomsFleet, list2.First<ShipInfo>());
				}
				game.GameDatabase.RemoveFleet(randomsFleet.ID);
			}
			List<SwarmerInfo> list3 = game.GameDatabase.GetSwarmerInfos().ToList<SwarmerInfo>();
			foreach (SwarmerInfo current3 in list3)
			{
				if (current3.QueenFleetId.HasValue)
				{
					FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(current3.QueenFleetId.Value);
					if (fleetInfo != null && fleetInfo.SystemID == systemId)
					{
						Swarmers.ClearTransform(game.GameDatabase, current3);
					}
				}
			}
		}
		private static void SimulateSwarmerNest(Random rand, GameSession game, int systemId, FleetInfo randomsFleet, int aiPlayerID, List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(2, 4);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			shipsInFleets.Sum((KeyValuePair<FleetInfo, List<ShipInfo>> x) => x.Value.Count);
			if (shipsInFleets.Sum(delegate(KeyValuePair<FleetInfo, List<ShipInfo>> x)
			{
				if (x.Value.Count <= 0)
				{
					return 0;
				}
				return x.Value.Sum(delegate(ShipInfo y)
				{
					if (y.DesignInfo == null)
					{
						return 0;
					}
					return CombatAI.GetShipStrength(y.DesignInfo.Class) / 3;
				});
			}) <= 2)
			{
				foreach (KeyValuePair<FleetInfo, List<ShipInfo>> current in shipsInFleets)
				{
					foreach (ShipInfo current2 in current.Value)
					{
						game.GameDatabase.RemoveShip(current2.ID);
					}
					CombatSimulatorRandoms.FleetDestroyed(game, randomsFleet.PlayerID, current.Key, null);
					game.GameDatabase.RemoveFleet(current.Key.ID);
				}
				List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
				if (list.Count == 0)
				{
					game.GameDatabase.RemoveFleet(randomsFleet.ID);
					return;
				}
			}
			else
			{
				CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
				List<ShipInfo> list2 = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
				if (list2.Count > 0)
				{
					CombatSimulatorRandoms.FleetDestroyed(game, aiPlayerID, randomsFleet, list2.First<ShipInfo>());
				}
				game.GameDatabase.RemoveFleet(randomsFleet.ID);
			}
		}
		private static void SimulateProteans(Random rand, GameSession game, int systemId, FleetInfo randomsFleet, int aiPlayerID, List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(3, 5);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			if (shipsInFleets.Sum((KeyValuePair<FleetInfo, List<ShipInfo>> x) => x.Value.Count) == 0)
			{
				return;
			}
			CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
			CombatSimulatorRandoms.FleetDestroyed(game, aiPlayerID, randomsFleet, null);
			game.GameDatabase.RemoveFleet(randomsFleet.ID);
		}
		private static void SimulateAsteroidMonitors(Random rand, GameSession game, int systemId, FleetInfo randomsFleet, int aiPlayerID, List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(1, 3);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			if (shipsInFleets.Sum((KeyValuePair<FleetInfo, List<ShipInfo>> x) => x.Value.Count) == 0)
			{
				return;
			}
			CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
			int encounterIDAtSystem = game.GameDatabase.GetEncounterIDAtSystem(EasterEgg.EE_ASTEROID_MONITOR, systemId);
			List<ShipInfo> source = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
			ShipInfo shipInfo = source.FirstOrDefault((ShipInfo x) => x.DesignID == game.ScriptModules.AsteroidMonitor.MonitorCommandDesignId);
			if (shipInfo == null)
			{
				game.GameDatabase.RemoveFleet(randomsFleet.ID);
				game.GameDatabase.RemoveEncounter(encounterIDAtSystem);
				return;
			}
			AsteroidMonitorInfo asteroidMonitorInfo = game.GameDatabase.GetAsteroidMonitorInfo(encounterIDAtSystem);
			if (asteroidMonitorInfo != null)
			{
				asteroidMonitorInfo.IsAggressive = false;
				game.GameDatabase.UpdateAsteroidMonitorInfo(asteroidMonitorInfo);
			}
			shipInfo.DesignInfo = game.GameDatabase.GetDesignInfo(shipInfo.DesignID);
			List<DesignModuleInfo> list = new List<DesignModuleInfo>();
			DesignSectionInfo[] designSections = shipInfo.DesignInfo.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				list.AddRange(designSectionInfo.Modules.ToList<DesignModuleInfo>());
			}
			if (list.Any<DesignModuleInfo>())
			{
				List<SectionInstanceInfo> list2 = game.GameDatabase.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>();
				foreach (SectionInstanceInfo current in list2)
				{
					List<ModuleInstanceInfo> list3 = game.GameDatabase.GetModuleInstances(current.ID).ToList<ModuleInstanceInfo>();
					foreach (ModuleInstanceInfo current2 in list3)
					{
						current2.Structure = 0;
						game.GameDatabase.UpdateModuleInstance(current2);
					}
				}
				game.InsertNewMonitorSpecialProject(aiPlayerID, encounterIDAtSystem, randomsFleet.ID);
				return;
			}
			game.GameDatabase.RemoveFleet(randomsFleet.ID);
			game.GameDatabase.RemoveEncounter(encounterIDAtSystem);
		}
		private static void SimulateIndyColony(Random rand, GameSession game, int systemId, ColonyInfo indyColony, Dictionary<PlayerInfo, List<FleetInfo>> enemyPlayers)
		{
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(indyColony.OrbitalObjectID);
			List<PlayerInfo> standardPlayers = game.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			List<FleetInfo> list = game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (FleetInfo current in list)
			{
				MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(current.ID);
				if (missionByFleetID != null && missionByFleetID.Type == MissionType.INVASION && missionByFleetID.TargetOrbitalObjectID == planetInfo.ID)
				{
					game.GameDatabase.InsertGovernmentAction(current.PlayerID, App.Localize("@GA_INDEPENDANTCONQUERED"), "IndependantConquered", 0, 0);
					foreach (PlayerInfo current2 in standardPlayers)
					{
						if (game.GameDatabase.GetDiplomacyInfo(current.PlayerID, current2.ID).isEncountered)
						{
							game.GameDatabase.ApplyDiplomacyReaction(current.PlayerID, current2.ID, StratModifiers.DiplomacyReactionInvadeIndependentWorld, 1);
						}
					}
				}
			}
			game.GameDatabase.InsertTurnEvent(new TurnEvent
			{
				EventType = TurnEventType.EV_COLONY_DESTROYED,
				EventMessage = TurnEventMessage.EM_COLONY_DESTROYED,
				PlayerID = indyColony.PlayerID,
				ColonyID = indyColony.ID,
				SystemID = systemId,
				TurnNumber = game.GameDatabase.GetTurnCount(),
				ShowsDialog = false
			});
			StationTypeFlags stationTypeFlags = StationTypeFlags.CIVILIAN | StationTypeFlags.DIPLOMATIC | StationTypeFlags.DEFENCE;
			List<StationInfo> list2 = game.GameDatabase.GetStationForSystemAndPlayer(systemId, indyColony.PlayerID).ToList<StationInfo>();
			foreach (StationInfo current3 in list2)
			{
				if (current3.OrbitalObjectInfo.ParentID == planetInfo.ID && (1 << (int)current3.DesignInfo.StationType & (int)stationTypeFlags) != 0)
				{
					game.GameDatabase.DestroyStation(game, current3.ID, 0);
				}
			}
			game.GameDatabase.RemoveColonyOnPlanet(planetInfo.ID);
			if (enemyPlayers.Keys.Any((PlayerInfo x) => standardPlayers.Any((PlayerInfo y) => y.ID == x.ID)))
			{
				GameSession.ApplyMoralEvent(game.App, MoralEvent.ME_LOSE_WORLD_ENEMY, indyColony.PlayerID, null, starSystemInfo.ProvinceID, null);
			}
			foreach (int i in (
				from x in enemyPlayers.Keys
				select x.ID).ToList<int>())
			{
				game.App.GameDatabase.ApplyDiplomacyReaction(i, indyColony.PlayerID, StratModifiers.DiplomacyReactionKillColony, 1);
				int factionId = game.App.GameDatabase.GetPlayerFactionID(indyColony.PlayerID);
				List<PlayerInfo> list3 = (
					from x in standardPlayers
					where x.FactionID == factionId && x.ID != i
					select x).ToList<PlayerInfo>();
				foreach (PlayerInfo current4 in list3)
				{
					if (game.App.GameDatabase.GetDiplomacyInfo(i, current4.ID).isEncountered)
					{
						game.App.GameDatabase.ApplyDiplomacyReaction(i, current4.ID, StratModifiers.DiplomacyReactionKillRaceWorld, 1);
					}
				}
			}
			if (indyColony.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld)
			{
				GameSession.ApplyMoralEvent(game.App, MoralEvent.ME_LOSE_GEM, indyColony.PlayerID, null, starSystemInfo.ProvinceID, null);
				return;
			}
			if (indyColony.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld)
			{
				GameSession.ApplyMoralEvent(game.App, MoralEvent.ME_LOSE_FORGE, indyColony.PlayerID, null, starSystemInfo.ProvinceID, null);
			}
		}
		private static void SimulateMorrigiRelics(Random rand, GameSession game, int systemId, FleetInfo randomsFleet, int aiPlayerID, List<FleetInfo> aiPlayerFleets)
		{
			int numToKill = rand.NextInclusive(5, 6);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			if (shipsInFleets.Sum((KeyValuePair<FleetInfo, List<ShipInfo>> x) => x.Value.Count) == 0)
			{
				return;
			}
			CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, numToKill);
			List<MorrigiRelicInfo> source = game.GameDatabase.GetMorrigiRelicInfos().ToList<MorrigiRelicInfo>();
			MorrigiRelicInfo morrigiRelicInfo = source.FirstOrDefault((MorrigiRelicInfo x) => x.FleetId == randomsFleet.ID);
			if (morrigiRelicInfo != null && morrigiRelicInfo.IsAggressive)
			{
				List<ShipInfo> aliveRelicShips = game.GameDatabase.GetShipInfoByFleetID(randomsFleet.ID, false).ToList<ShipInfo>();
				game.ScriptModules.MorrigiRelic.ApplyRewardsToPlayers(game.App, morrigiRelicInfo, aliveRelicShips, new List<Player>
				{
					game.GetPlayerObject(aiPlayerID)
				});
				return;
			}
			CombatSimulatorRandoms.FleetDestroyed(game, aiPlayerID, randomsFleet, null);
			game.GameDatabase.RemoveFleet(randomsFleet.ID);
		}
		private static void SimulatePirateBase(Random rand, GameSession game, int systemId, FleetInfo randomsFleet, int aiPlayerID, List<FleetInfo> aiPlayerFleets)
		{
			int num = rand.NextInclusive(5, 6);
			Dictionary<FleetInfo, List<ShipInfo>> shipsInFleets = CombatSimulatorRandoms.GetShipsInFleets(game, aiPlayerFleets);
			int num2 = shipsInFleets.Sum((KeyValuePair<FleetInfo, List<ShipInfo>> x) => x.Value.Count);
			if (num2 == 0)
			{
				return;
			}
			PirateBaseInfo pirateBaseInfo = game.GameDatabase.GetPirateBaseInfos().FirstOrDefault((PirateBaseInfo x) => x.SystemId == systemId);
			if (pirateBaseInfo == null)
			{
				return;
			}
			CombatSimulatorRandoms.UpdateShipsKilled(game, rand, shipsInFleets, randomsFleet.PlayerID, num);
			if (num2 >= num)
			{
				int num3 = game.AssetDatabase.GlobalPiracyData.Bounties[0];
				List<FleetInfo> list = game.GameDatabase.GetFleetsByPlayerAndSystem(randomsFleet.PlayerID, systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
				int num4 = 0;
				foreach (FleetInfo current in list)
				{
					num4 = game.GameDatabase.GetShipInfoByFleetID(current.ID, true).Count((ShipInfo x) => x.DesignInfo.Class != ShipClass.Station);
				}
				if (num4 > 0)
				{
					int num5 = rand.NextInclusive(0, num4);
					num3 += num5 * game.AssetDatabase.GlobalPiracyData.Bounties[1];
				}
				List<int> list2 = game.GameDatabase.GetStandardPlayerIDs().ToList<int>();
				foreach (int current2 in list2)
				{
					if (current2 != aiPlayerID)
					{
						string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(current2));
						int reactionAmount = 0;
						game.AssetDatabase.GlobalPiracyData.ReactionBonuses.TryGetValue(factionName, out reactionAmount);
						game.GameDatabase.ApplyDiplomacyReaction(aiPlayerID, current2, reactionAmount, null, 1);
					}
				}
				game.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_PIRATE_BASE_DESTROYED,
					EventMessage = TurnEventMessage.EM_PIRATE_BASE_DESTROYED,
					PlayerID = aiPlayerID,
					SystemID = systemId,
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
				game.GameDatabase.DestroyStation(game, pirateBaseInfo.BaseStationId, 0);
			}
		}
		private static void UpdateShipsKilled(GameSession game, Random rand, Dictionary<FleetInfo, List<ShipInfo>> aiPlayerShips, int randomsPlayerID, int numToKill)
		{
			int num = numToKill;
			int num2 = 0;
			while (num2 < numToKill && num > 0 && aiPlayerShips.Keys.Count > 0)
			{
				bool flag = false;
				while (!flag && aiPlayerShips.Keys.Count > 0)
				{
					int num3 = 0;
					foreach (KeyValuePair<FleetInfo, List<ShipInfo>> current in aiPlayerShips)
					{
						num3 += current.Value.Count;
						foreach (ShipInfo ship in current.Value)
						{
							if (rand.CoinToss(50))
							{
								num -= CombatAI.GetShipStrength(ship.DesignInfo.Class) / 3;
								if (ship.DesignInfo.IsSuulka())
								{
									TurnEvent turnEvent = game.GameDatabase.GetTurnEventsByTurnNumber(game.GameDatabase.GetTurnCount(), current.Key.PlayerID).FirstOrDefault((TurnEvent x) => x.ShipID == ship.ID);
									if (turnEvent != null)
									{
										game.GameDatabase.RemoveTurnEvent(turnEvent.ID);
									}
									game.GameDatabase.InsertTurnEvent(new TurnEvent
									{
										EventType = TurnEventType.EV_SUULKA_DIES,
										EventMessage = TurnEventMessage.EM_SUULKA_DIES,
										PlayerID = randomsPlayerID,
										SystemID = current.Key.SystemID,
										ShipID = ship.ID,
										DesignID = ship.DesignID,
										TurnNumber = game.GameDatabase.GetTurnCount(),
										ShowsDialog = false
									});
									SuulkaInfo suulkaByShipID = game.GameDatabase.GetSuulkaByShipID(ship.ID);
									if (suulkaByShipID != null)
									{
										game.GameDatabase.RemoveSuulka(suulkaByShipID.ID);
									}
								}
								game.GameDatabase.RemoveShip(ship.ID);
								current.Value.Remove(ship);
								flag = true;
								break;
							}
						}
						if (flag)
						{
							if (current.Value.Count == 0)
							{
								CombatSimulatorRandoms.FleetDestroyed(game, randomsPlayerID, current.Key, null);
								game.GameDatabase.RemoveFleet(current.Key.ID);
								aiPlayerShips.Remove(current.Key);
								break;
							}
							break;
						}
					}
					if (num3 == 0)
					{
						break;
					}
				}
				num2++;
			}
			foreach (KeyValuePair<FleetInfo, List<ShipInfo>> current2 in aiPlayerShips)
			{
				if (current2.Value.Count > 0)
				{
					CombatSimulatorRandoms.CheckFleetCommandPoints(game, current2.Key, current2.Value);
				}
			}
		}
		private static void FleetDestroyed(GameSession game, int killerPlayerID, FleetInfo fleet, ShipInfo fleetCommander = null)
		{
			if (fleet.PlayerID == game.ScriptModules.Gardeners.PlayerID)
			{
				game.GameDatabase.InsertTurnEvent(new TurnEvent
				{
					EventType = TurnEventType.EV_PROTEANS_REMOVED,
					EventMessage = TurnEventMessage.EM_PROTEANS_REMOVED,
					PlayerID = killerPlayerID,
					SystemID = fleet.SystemID,
					TurnNumber = game.GameDatabase.GetTurnCount(),
					ShowsDialog = false
				});
			}
			else
			{
				if (fleet.PlayerID == game.ScriptModules.Swarmers.PlayerID)
				{
					if (fleetCommander != null && fleetCommander.DesignID == game.ScriptModules.Swarmers.SwarmQueenDesignID)
					{
						game.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_SWARM_QUEEN_DESTROYED,
							EventMessage = TurnEventMessage.EM_SWARM_QUEEN_DESTROYED,
							PlayerID = killerPlayerID,
							SystemID = fleet.SystemID,
							TurnNumber = game.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					else
					{
						game.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_SWARM_DESTROYED,
							EventMessage = TurnEventMessage.EM_SWARM_DESTROYED,
							PlayerID = killerPlayerID,
							SystemID = fleet.SystemID,
							TurnNumber = game.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
				else
				{
					if (fleet.PlayerID == game.ScriptModules.MorrigiRelic.PlayerID)
					{
						game.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_TOMB_DESTROYED,
							EventMessage = TurnEventMessage.EM_TOMB_DESTROYED,
							PlayerID = killerPlayerID,
							SystemID = fleet.SystemID,
							TurnNumber = game.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
					else
					{
						game.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_FLEET_DESTROYED,
							EventMessage = TurnEventMessage.EM_FLEET_DESTROYED,
							PlayerID = killerPlayerID,
							SystemID = fleet.SystemID,
							FleetID = fleet.ID,
							TurnNumber = game.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
			GameTrigger.PushEvent(EventType.EVNT_FLEETDIED, fleet.Name, game);
		}
		private static void CheckFleetCommandPoints(GameSession game, FleetInfo fleet, List<ShipInfo> ships)
		{
			if (game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)) == "loa")
			{
				return;
			}
			if (ships.Count == 0 || fleet.Type != FleetType.FL_NORMAL)
			{
				return;
			}
			if (ships.Max((ShipInfo x) => game.GameDatabase.GetShipCommandPointQuota(x.ID)) == 0)
			{
				int num = game.GameDatabase.InsertFleet(fleet.PlayerID, 0, fleet.SystemID, fleet.SupportingSystemID, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL);
				int missionID = game.GameDatabase.InsertMission(num, MissionType.RETREAT, 0, 0, 0, 0, false, null);
				game.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, null);
				game.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, null);
				foreach (ShipInfo current in ships)
				{
					game.GameDatabase.TransferShip(current.ID, num);
				}
				game.GameDatabase.RemoveFleet(fleet.ID);
			}
		}
	}
}
