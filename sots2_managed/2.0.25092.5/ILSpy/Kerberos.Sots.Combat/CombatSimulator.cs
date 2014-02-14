using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.Combat
{
	internal static class CombatSimulator
	{
		private static Random _rand = new Random();
		private static void ApplyWeaponStats(ShipCombatInfo sci, LogicalWeapon lw, int totalMounts)
		{
			if (lw != null && lw.RechargeTime > 0f)
			{
				float num = ((lw.Duration > 0f) ? lw.Duration : 1f) / lw.RechargeTime * 60f * (float)totalMounts;
				if (lw.PayloadType == WeaponEnums.PayloadTypes.Missile || lw.PayloadType == WeaponEnums.PayloadTypes.Torpedo)
				{
					sci.trackingFireFactor += lw.RangeTable.Effective.Damage * num;
				}
				if (lw.Traits.Contains(WeaponEnums.WeaponTraits.PointDefence))
				{
					sci.pdFactor += lw.RangeTable.Effective.Damage * num;
				}
				sci.directFireFactor = lw.RangeTable.PointBlank.Damage * num;
				sci.bombFactorPopulation += lw.PopDamage * num;
				sci.bombFactorInfrastructure += lw.InfraDamage * num;
				sci.bombFactorHazard += lw.TerraDamage * num;
			}
		}
		private static void ApplyDeaths(Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo)
		{
			foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> current in shipCombatInfo)
			{
				foreach (ShipCombatInfo current2 in current.Value)
				{
					if (current2.structureFactor <= 0f)
					{
						current2.shipDead = true;
					}
				}
			}
		}
		public static void Simulate(GameSession game, int systemId, List<FleetInfo> fleets)
		{
			if (ScriptHost.AllowConsole)
			{
				App.Log.Trace(string.Format("Simulating AI combat at: {0}", systemId), "combat");
			}
			List<PlanetCombatInfo> list = new List<PlanetCombatInfo>();
			PlanetInfo[] starSystemPlanetInfos = game.GameDatabase.GetStarSystemPlanetInfos(systemId);
			if (starSystemPlanetInfos != null)
			{
				PlanetInfo[] array = starSystemPlanetInfos;
				for (int i = 0; i < array.Length; i++)
				{
					PlanetInfo planetInfo = array[i];
					list.Add(new PlanetCombatInfo
					{
						planetInfo = planetInfo,
						colonyInfo = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID)
					});
				}
			}
			Dictionary<FleetInfo, List<ShipCombatInfo>> dictionary = new Dictionary<FleetInfo, List<ShipCombatInfo>>();
			foreach (FleetInfo current in fleets)
			{
				List<ShipCombatInfo> list2 = new List<ShipCombatInfo>();
				List<ShipInfo> list3 = game.GameDatabase.GetShipInfoByFleetID(current.ID, true).ToList<ShipInfo>();
				foreach (ShipInfo current2 in list3)
				{
					if (current2.DesignInfo.Class != ShipClass.BattleRider)
					{
						ShipCombatInfo shipCombatInfo = new ShipCombatInfo();
						shipCombatInfo.shipInfo = current2;
						float num = 1f;
						if (current2.DesignInfo.Class == ShipClass.Cruiser || current2.DesignInfo.Class == ShipClass.Dreadnought)
						{
							num = 3f;
						}
						shipCombatInfo.armorFactor = (float)current2.DesignInfo.Armour / num;
						shipCombatInfo.structureFactor = current2.DesignInfo.Structure / num;
						DesignSectionInfo[] designSections = current2.DesignInfo.DesignSections;
						for (int j = 0; j < designSections.Length; j++)
						{
							DesignSectionInfo designSectionInfo = designSections[j];
							ShipSectionAsset shipSectionAsset = game.AssetDatabase.GetShipSectionAsset(designSectionInfo.FilePath);
							foreach (WeaponBankInfo wbi in designSectionInfo.WeaponBanks)
							{
								if (wbi.WeaponID.HasValue)
								{
									string weaponName = Path.GetFileNameWithoutExtension(game.GameDatabase.GetWeaponAsset(wbi.WeaponID.Value));
									LogicalWeapon lw = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon weapon) => string.Equals(weapon.WeaponName, weaponName, StringComparison.InvariantCultureIgnoreCase));
									List<LogicalMount> list4 = (
										from x in shipSectionAsset.Mounts
										where x.Bank.GUID == wbi.BankGUID
										select x).ToList<LogicalMount>();
									int totalMounts = (list4.Count<LogicalMount>() > 0) ? list4.Count<LogicalMount>() : 1;
									foreach (LogicalMount current3 in list4)
									{
										switch (current3.Bank.TurretClass)
										{
										case WeaponEnums.TurretClasses.Drone:
											shipCombatInfo.drones++;
											continue;
										case WeaponEnums.TurretClasses.DestroyerRider:
										case WeaponEnums.TurretClasses.CruiserRider:
										case WeaponEnums.TurretClasses.DreadnoughtRider:
											shipCombatInfo.battleRiders++;
											continue;
										}
										CombatSimulator.ApplyWeaponStats(shipCombatInfo, lw, totalMounts);
									}
								}
							}
							foreach (DesignModuleInfo mod in designSectionInfo.Modules)
							{
								LogicalModule logicalModule = game.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == game.GameDatabase.GetModuleAsset(mod.ModuleID));
								if (logicalModule != null && mod.WeaponID.HasValue)
								{
									LogicalBank[] banks = logicalModule.Banks;
									LogicalBank lb;
									for (int k = 0; k < banks.Length; k++)
									{
										lb = banks[k];
										string weaponName = Path.GetFileNameWithoutExtension(game.GameDatabase.GetWeaponAsset(mod.WeaponID.Value));
										LogicalWeapon lw2 = game.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon weapon) => string.Equals(weapon.WeaponName, weaponName, StringComparison.InvariantCultureIgnoreCase));
										List<LogicalMount> list5 = (
											from x in shipSectionAsset.Mounts
											where x.Bank.GUID == lb.GUID
											select x).ToList<LogicalMount>();
										int totalMounts2 = (list5.Count<LogicalMount>() > 0) ? list5.Count<LogicalMount>() : 1;
										foreach (LogicalMount arg_4BE_0 in list5)
										{
											CombatSimulator.ApplyWeaponStats(shipCombatInfo, lw2, totalMounts2);
										}
									}
								}
							}
						}
						list2.Add(shipCombatInfo);
					}
				}
				dictionary.Add(current, list2);
			}
			if (fleets.Count<FleetInfo>() > 1)
			{
				CombatSimulator.TrackingPhase(dictionary, 4f);
				CombatSimulator.DirectPhase(dictionary, 4f);
				CombatSimulator.TrackingPhase(dictionary, 1f);
				CombatSimulator.DirectPhase(dictionary, 2f);
				CombatSimulator.BombardmentPhase(game.GameDatabase, dictionary, list, 2f);
			}
			else
			{
				CombatSimulator.BombardmentPhase(game.GameDatabase, dictionary, list, 5f);
			}
			CombatSimulator.CompleteSimulation(game, systemId, dictionary, list);
		}
		private static void CompleteSimulation(GameSession game, int systemId, Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo, List<PlanetCombatInfo> planets)
		{
			CombatData combatData = game.CombatData.AddCombat(GameSession.GetNextUniqueCombatID(), systemId, game.GameDatabase.GetTurnCount());
			foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> current in shipCombatInfo)
			{
				PlayerCombatData orAddPlayer = combatData.GetOrAddPlayer(current.Key.PlayerID);
				orAddPlayer.VictoryStatus = GameSession.VictoryStatus.Draw;
				foreach (ShipCombatInfo sci in current.Value)
				{
					if (sci.structureFactor == 0f)
					{
						if (sci.shipInfo.DesignInfo.IsSuulka())
						{
							TurnEvent turnEvent = game.GameDatabase.GetTurnEventsByTurnNumber(game.GameDatabase.GetTurnCount(), orAddPlayer.PlayerID).FirstOrDefault((TurnEvent x) => x.ShipID == sci.shipInfo.ID);
							if (turnEvent != null)
							{
								game.GameDatabase.RemoveTurnEvent(turnEvent.ID);
							}
							List<int> list = new List<int>();
							List<int> list2 = new List<int>();
							foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> current2 in shipCombatInfo)
							{
								if (orAddPlayer.PlayerID != current2.Key.PlayerID && !list.Contains(current2.Key.PlayerID) && !list2.Contains(current2.Key.PlayerID))
								{
									DiplomacyState diplomacyStateBetweenPlayers = game.GameDatabase.GetDiplomacyStateBetweenPlayers(orAddPlayer.PlayerID, current2.Key.PlayerID);
									if (diplomacyStateBetweenPlayers == DiplomacyState.WAR)
									{
										list.Add(current2.Key.PlayerID);
									}
									else
									{
										if (diplomacyStateBetweenPlayers == DiplomacyState.NEUTRAL)
										{
											list2.Add(current2.Key.PlayerID);
										}
									}
								}
							}
							int playerID = 0;
							if (list.Count > 0)
							{
								playerID = App.GetSafeRandom().Choose(list);
							}
							else
							{
								if (list2.Count > 0)
								{
									playerID = App.GetSafeRandom().Choose(list2);
								}
							}
							game.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_SUULKA_DIES,
								EventMessage = TurnEventMessage.EM_SUULKA_DIES,
								PlayerID = playerID,
								SystemID = systemId,
								ShipID = sci.shipInfo.ID,
								DesignID = sci.shipInfo.DesignID,
								TurnNumber = game.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
							SuulkaInfo suulkaByShipID = game.GameDatabase.GetSuulkaByShipID(sci.shipInfo.ID);
							if (suulkaByShipID != null)
							{
								game.GameDatabase.RemoveSuulka(suulkaByShipID.ID);
							}
						}
						game.GameDatabase.RemoveShip(sci.shipInfo.ID);
						GameTrigger.PushEvent(EventType.EVNT_SHIPDIED, sci.shipInfo.DesignInfo.Class, game);
						orAddPlayer.AddShipData(sci.shipInfo.DesignID, 0f, 0f, 0, true);
						if (ScriptHost.AllowConsole)
						{
							App.Log.Trace(string.Format("Ship destroyed: {0} ({1})", sci.shipInfo.ID, sci.shipInfo.ShipName), "combat");
						}
					}
					else
					{
						if (sci.shipInfo.DesignInfo == null)
						{
							sci.shipInfo.DesignInfo = game.GameDatabase.GetDesignInfo(sci.shipInfo.DesignID);
						}
						List<SectionInstanceInfo> list3 = game.GameDatabase.GetShipSectionInstances(sci.shipInfo.ID).ToList<SectionInstanceInfo>();
						foreach (SectionInstanceInfo sii in list3)
						{
							DesignSectionInfo designSectionInfo = sci.shipInfo.DesignInfo.DesignSections.First((DesignSectionInfo x) => x.ID == sii.SectionID);
							int minStructure = designSectionInfo.GetMinStructure(game.GameDatabase, game.AssetDatabase);
							sii.Structure -= sii.Structure - (int)Math.Round((double)sci.structureFactor);
							sii.Structure = Math.Max(sii.Structure, minStructure);
							game.GameDatabase.UpdateSectionInstance(sii);
							if (sii.Structure == minStructure)
							{
								List<ModuleInstanceInfo> list4 = game.GameDatabase.GetModuleInstances(sii.ID).ToList<ModuleInstanceInfo>();
								foreach (ModuleInstanceInfo current3 in list4)
								{
									current3.Structure = 0;
									game.GameDatabase.UpdateModuleInstance(current3);
								}
								List<WeaponInstanceInfo> list5 = game.GameDatabase.GetWeaponInstances(sii.ID).ToList<WeaponInstanceInfo>();
								foreach (WeaponInstanceInfo current4 in list5)
								{
									current4.Structure = 0f;
									game.GameDatabase.UpdateWeaponInstance(current4);
								}
							}
						}
					}
				}
				if (!CombatSimulator.IsFleetAlive(current.Value))
				{
					game.GameDatabase.RemoveFleet(current.Key.ID);
					GameTrigger.PushEvent(EventType.EVNT_FLEETDIED, current.Key.Name, game);
					if (ScriptHost.AllowConsole)
					{
						App.Log.Trace(string.Format("Fleet destroyed: {0} ({1})", current.Key.ID, current.Key.Name), "combat");
					}
				}
				else
				{
					CombatSimulator.CheckFleetCommandPoints(game, current.Key);
				}
			}
			foreach (PlanetCombatInfo current5 in planets)
			{
				game.GameDatabase.UpdatePlanet(current5.planetInfo);
				if (current5.colonyInfo != null)
				{
					if (current5.colonyInfo.ImperialPop <= 0.0)
					{
						game.GameDatabase.RemoveColonyOnPlanet(current5.planetInfo.ID);
						if (ScriptHost.AllowConsole)
						{
							App.Log.Trace(string.Format("Colony defeated: planetid={0}", current5.planetInfo.ID), "combat");
						}
					}
					else
					{
						current5.colonyInfo.DamagedLastTurn = true;
						game.GameDatabase.UpdateColony(current5.colonyInfo);
						ColonyFactionInfo[] factions = current5.colonyInfo.Factions;
						for (int i = 0; i < factions.Length; i++)
						{
							ColonyFactionInfo civPop = factions[i];
							game.GameDatabase.UpdateCivilianPopulation(civPop);
						}
					}
				}
			}
			game.GameDatabase.InsertCombatData(systemId, combatData.CombatID, combatData.Turn, combatData.ToByteArray());
		}
		private static bool IsFleetAlive(List<ShipCombatInfo> ships)
		{
			bool result = false;
			foreach (ShipCombatInfo current in ships)
			{
				if (current.structureFactor > 0f)
				{
					result = true;
					break;
				}
			}
			return result;
		}
		private static void CheckFleetCommandPoints(GameSession game, FleetInfo fleet)
		{
			if (game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)) == "loa")
			{
				return;
			}
			List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
			if (list.Count == 0 || fleet.Type != FleetType.FL_NORMAL)
			{
				return;
			}
			if (list.Max((ShipInfo x) => game.GameDatabase.GetShipCommandPointQuota(x.ID)) == 0)
			{
				int num = game.GameDatabase.InsertFleet(fleet.PlayerID, 0, fleet.SystemID, fleet.SupportingSystemID, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL);
				int missionID = game.GameDatabase.InsertMission(num, MissionType.RETREAT, 0, 0, 0, 0, false, null);
				game.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, null);
				game.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, null);
				foreach (ShipInfo current in list)
				{
					game.GameDatabase.TransferShip(current.ID, num);
				}
				game.GameDatabase.RemoveFleet(fleet.ID);
			}
		}
		private static ShipCombatInfo SelectTargetShip(FleetInfo currentFleet, Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo)
		{
			IEnumerable<KeyValuePair<FleetInfo, List<ShipCombatInfo>>> source = 
				from x in shipCombatInfo
				where x.Key != currentFleet && CombatSimulator.IsFleetAlive(x.Value)
				select x;
			if (source.Count<KeyValuePair<FleetInfo, List<ShipCombatInfo>>>() == 0)
			{
				return null;
			}
			int index = CombatSimulator._rand.Next(0, source.Count<KeyValuePair<FleetInfo, List<ShipCombatInfo>>>() - 1);
			KeyValuePair<FleetInfo, List<ShipCombatInfo>> keyValuePair = source.ElementAt(index);
			ShipCombatInfo shipCombatInfo2 = null;
			foreach (ShipCombatInfo current in keyValuePair.Value)
			{
				if (current.shipInfo.DesignInfo.Role == ShipRole.COMMAND && current.structureFactor > 0f)
				{
					shipCombatInfo2 = current;
					break;
				}
			}
			if (shipCombatInfo2 == null)
			{
				foreach (ShipCombatInfo current2 in keyValuePair.Value)
				{
					if (current2.structureFactor > 0f)
					{
						shipCombatInfo2 = current2;
						break;
					}
				}
			}
			return shipCombatInfo2;
		}
		private static void ApplyShipDamage(ShipCombatInfo targetShip, float damage)
		{
			if (damage <= 0f)
			{
				return;
			}
			if (targetShip.armorFactor > damage)
			{
				float num = damage * 0.25f;
				damage -= damage * 0.25f;
				targetShip.armorFactor -= damage;
				targetShip.structureFactor -= num;
				damage = 0f;
			}
			else
			{
				float armorFactor = targetShip.armorFactor;
				targetShip.armorFactor = 0f;
				damage -= armorFactor;
			}
			if (damage <= 0f)
			{
				return;
			}
			if (targetShip.structureFactor > damage)
			{
				targetShip.structureFactor -= damage;
				damage = 0f;
				return;
			}
			targetShip.structureFactor = 0f;
		}
		private static bool TrackingPhase(Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo, float damageMultiplier = 1f)
		{
			foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> current in shipCombatInfo)
			{
				ShipCombatInfo shipCombatInfo2 = CombatSimulator.SelectTargetShip(current.Key, shipCombatInfo);
				if (shipCombatInfo2 != null)
				{
					foreach (ShipCombatInfo current2 in current.Value)
					{
						if (!current2.shipDead)
						{
							if (shipCombatInfo2.structureFactor <= 0f)
							{
								shipCombatInfo2 = CombatSimulator.SelectTargetShip(current.Key, shipCombatInfo);
							}
							if (shipCombatInfo2 == null)
							{
								break;
							}
							float num = current2.trackingFireFactor * (float)(0.75 + CombatSimulator._rand.NextDouble() * 1.25) * damageMultiplier;
							num -= shipCombatInfo2.pdFactor;
							CombatSimulator.ApplyShipDamage(shipCombatInfo2, num);
						}
					}
				}
			}
			CombatSimulator.ApplyDeaths(shipCombatInfo);
			return true;
		}
		private static bool DirectPhase(Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo, float damageMultiplier = 1f)
		{
			foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> current in shipCombatInfo)
			{
				ShipCombatInfo shipCombatInfo2 = CombatSimulator.SelectTargetShip(current.Key, shipCombatInfo);
				if (shipCombatInfo2 != null)
				{
					foreach (ShipCombatInfo current2 in current.Value)
					{
						if (!current2.shipDead)
						{
							if (shipCombatInfo2.structureFactor <= 0f)
							{
								shipCombatInfo2 = CombatSimulator.SelectTargetShip(current.Key, shipCombatInfo);
								if (shipCombatInfo2 == null)
								{
									break;
								}
							}
							float damage = current2.directFireFactor * (float)(0.5 + CombatSimulator._rand.NextDouble() * 1.5) * damageMultiplier * (1f + (float)current2.battleRiders * 0.15f) * (1f + (float)current2.drones * 0.05f);
							CombatSimulator.ApplyShipDamage(shipCombatInfo2, damage);
						}
					}
				}
			}
			CombatSimulator.ApplyDeaths(shipCombatInfo);
			return true;
		}
		private static bool BombardmentPhase(GameDatabase db, Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo, List<PlanetCombatInfo> planets, float damageMultiplier = 1f)
		{
			foreach (PlanetCombatInfo pci in planets)
			{
				if (pci.colonyInfo != null && pci.colonyInfo.ImperialPop != 0.0)
				{
					IEnumerable<KeyValuePair<FleetInfo, List<ShipCombatInfo>>> enumerable = 
						from x in shipCombatInfo
						where x.Key.PlayerID != pci.colonyInfo.PlayerID
						select x;
					float num = CombatSimulator._rand.CoinToss(50) ? 1f : -1f;
					foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> current in enumerable)
					{
						foreach (ShipCombatInfo current2 in current.Value)
						{
							pci.planetInfo.Infrastructure -= current2.bombFactorInfrastructure * damageMultiplier;
							pci.planetInfo.Suitability += num * current2.bombFactorHazard * damageMultiplier * 0.1f;
							pci.planetInfo.Infrastructure = Math.Max(0f, pci.planetInfo.Infrastructure);
							pci.planetInfo.Suitability = Math.Max(Math.Min(Constants.MaxSuitability, pci.planetInfo.Suitability), Constants.MinSuitability);
							float num2 = current2.bombFactorPopulation / ((float)pci.colonyInfo.Factions.Count<ColonyFactionInfo>() + 1f) * damageMultiplier;
							pci.colonyInfo.ImperialPop = Math.Max(0.0, pci.colonyInfo.ImperialPop - Math.Ceiling((double)num2));
							ColonyFactionInfo[] factions = pci.colonyInfo.Factions;
							for (int i = 0; i < factions.Length; i++)
							{
								ColonyFactionInfo colonyFactionInfo = factions[i];
								colonyFactionInfo.CivilianPop = Math.Max(0.0, colonyFactionInfo.CivilianPop - Math.Ceiling((double)num2));
							}
						}
					}
				}
			}
			bool result = false;
			foreach (PlanetCombatInfo current3 in planets)
			{
				if (current3.colonyInfo != null && current3.colonyInfo.ImperialPop > 0.0)
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
