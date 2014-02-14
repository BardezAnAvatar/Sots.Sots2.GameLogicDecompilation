using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class Budget
	{
		public readonly double CurrentSavings;
		public readonly double SavingsInterest;
		public readonly double DebtInterest;
		public readonly double TradeRevenue;
		public readonly double TaxRevenue;
		public readonly double IORevenue;
		public readonly double CurrentShipUpkeepExpenses;
		public readonly double CurrentStationUpkeepExpenses;
		public readonly double AdditionalUpkeepExpenses;
		public readonly double ColonySupportExpenses;
		public readonly double CorruptionExpenses;
		public readonly int RequiredSecurity;
		public readonly double TotalRevenue;
		public readonly double UpkeepExpenses;
		public readonly double TotalExpenses;
		public readonly double OperatingBudget;
		public readonly double DisposableIncome;
		public readonly double NetSavingsLoss;
		public readonly double RequestedGovernmentSpending;
		public readonly ResearchSpending ResearchSpending;
		public readonly SecuritySpending SecuritySpending;
		public readonly StimulusSpending StimulusSpending;
		public readonly double ProjectedGovernmentSpending;
		public readonly double UnspentIncome;
		public readonly double NetSavingsIncome;
		public readonly double ProjectedSavings;
		public readonly double PendingBuildShipsCost;
		public readonly double PendingBuildStationsCost;
		public readonly double PendingStationsModulesCost;
		public readonly double TotalBuildShipCosts;
		public readonly double TotalBuildStationsCost;
		public readonly double TotalStationsModulesCost;
		private Budget(GameDatabase gamedb, AssetDatabase assetdb, GameSession game, PlayerInfo playerInfo, FactionInfo playerFactionInfo, double maxDriveSpeed, double incomeFromTrade, SpendingCaps spendingCaps, IEnumerable<ColonyInfo> colonyInfos, Dictionary<int, PlanetInfo> planetInfos, Dictionary<int, OrbitalObjectInfo> orbitalObjectInfos, Dictionary<int, StarSystemInfo> starSystemInfos, HashSet<int> starSystemsWithGates, IEnumerable<StationInfo> stationInfos, IEnumerable<DesignInfo> reserveShipDesignInfos, IEnumerable<DesignInfo> shipDesignInfos, IEnumerable<DesignInfo> eliteShipDesignInfos, IEnumerable<DesignInfo> additionalShipDesignInfos)
		{
			this.CurrentSavings = playerInfo.Savings;
			this.SavingsInterest = ((playerFactionInfo.Name != "loa") ? GameSession.CalculateSavingsInterest(game, playerInfo) : 0.0);
			this.DebtInterest = ((playerFactionInfo.Name != "loa") ? GameSession.CalculateDebtInterest(game, playerInfo) : 0.0);
			this.TradeRevenue = incomeFromTrade;
			this.TaxRevenue = colonyInfos.Sum((ColonyInfo x) => Colony.GetTaxRevenue(game.App, playerInfo, x));
			float num = gamedb.GetNameValue<float>("EconomicEfficiency") / 100f;
			this.TradeRevenue *= (double)num;
			this.TradeRevenue *= (double)game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TradeRevenue, playerInfo.ID);
			this.TaxRevenue *= (double)num;
			Player playerObject = game.GetPlayerObject(playerInfo.ID);
			if (playerObject == null || !playerObject.IsAI())
			{
				this.CurrentStationUpkeepExpenses = GameSession.CalculateStationUpkeepCosts(gamedb, assetdb, stationInfos);
				this.CurrentShipUpkeepExpenses = GameSession.CalculateFleetUpkeepCosts(assetdb, reserveShipDesignInfos, shipDesignInfos, eliteShipDesignInfos);
				this.AdditionalUpkeepExpenses = GameSession.CalculateShipUpkeepCosts(assetdb, additionalShipDesignInfos, 1f, false);
			}
			this.ColonySupportExpenses = colonyInfos.Sum(delegate(ColonyInfo x)
			{
				OrbitalObjectInfo orbitalObjectInfo = orbitalObjectInfos[x.OrbitalObjectID];
				PlanetInfo arg_23_0 = planetInfos[x.OrbitalObjectID];
				StarSystemInfo arg_35_0 = starSystemInfos[orbitalObjectInfo.StarSystemID];
				return Colony.GetColonySupportCost(gamedb, assetdb, playerInfo, playerFactionInfo, orbitalObjectInfos[x.OrbitalObjectID], planetInfos[x.OrbitalObjectID], starSystemInfos[orbitalObjectInfos[x.OrbitalObjectID].StarSystemID], orbitalObjectInfos, planetInfos, starSystemInfos, starSystemsWithGates.Contains(orbitalObjectInfos[x.OrbitalObjectID].StarSystemID), maxDriveSpeed);
			});
			this.IORevenue = 0.0;
			List<int> list = gamedb.GetPlayerColonySystemIDs(playerInfo.ID).ToList<int>();
			int num2 = (
				from x in list
				where gamedb.GetStarSystemInfo(x).IsOpen
				select x).Count<int>();
			Player playerObject2 = game.GetPlayerObject(playerInfo.ID);
			float num3 = 0f;
			foreach (int current in list)
			{
				List<BuildOrderInfo> list2 = gamedb.GetBuildOrdersForSystem(current).ToList<BuildOrderInfo>();
				float num4 = 0f;
				List<ColonyInfo> list3 = gamedb.GetColonyInfosForSystem(current).ToList<ColonyInfo>();
				foreach (ColonyInfo current2 in list3)
				{
					if (current2.PlayerID == playerInfo.ID)
					{
						num4 += Colony.GetConstructionPoints(game, current2);
					}
				}
				num4 *= game.GetStationBuildModifierForSystem(current, playerInfo.ID);
				foreach (BuildOrderInfo current3 in list2)
				{
					DesignInfo designInfo = gamedb.GetDesignInfo(current3.DesignID);
					if (designInfo.PlayerID == playerInfo.ID)
					{
						int num5 = designInfo.SavingsCost;
						if (designInfo.IsLoaCube())
						{
							num5 = current3.LoaCubes * assetdb.LoaCostPerCube;
						}
						int num6 = current3.ProductionTarget - current3.Progress;
						float num7 = 0f;
						if (!designInfo.isPrototyped)
						{
							num7 = (float)((int)(num4 * (gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeTimeModifier, playerInfo.ID) - 1f)));
							switch (designInfo.Class)
							{
							case ShipClass.Cruiser:
								num5 = (int)((float)designInfo.SavingsCost * gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierCR, playerInfo.ID));
								break;
							case ShipClass.Dreadnought:
								num5 = (int)((float)designInfo.SavingsCost * gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierDN, playerInfo.ID));
								break;
							case ShipClass.Leviathan:
								num5 = (int)((float)designInfo.SavingsCost * gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierLV, playerInfo.ID));
								break;
							case ShipClass.Station:
								if (designInfo.GetRealShipClass() == RealShipClasses.Platform)
								{
									num5 = (int)((float)designInfo.SavingsCost * gamedb.GetStratModifierFloatToApply(StratModifiers.PrototypeSavingsCostModifierPF, playerInfo.ID));
								}
								break;
							}
						}
						if (playerInfo.isStandardPlayer && playerObject2.IsAI() && playerObject2.Faction.Name == "loa")
						{
							num5 = (int)((float)num5 * 0.5f);
						}
						if ((float)num6 <= num4 - num7)
						{
							num3 += (float)num5;
							num4 -= (float)num6;
						}
						this.TotalBuildShipCosts += (double)num5;
					}
				}
				this.IORevenue += (double)num4;
			}
			this.IORevenue *= (double)game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.IORevenue, playerInfo.ID);
			this.PendingBuildShipsCost = (double)num3;
			foreach (MissionInfo current4 in 
				from x in game.GameDatabase.GetMissionInfos()
				where x.Type == MissionType.CONSTRUCT_STN
				select x)
			{
				FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(current4.FleetID);
				if (fleetInfo.PlayerID == playerInfo.ID)
				{
                    MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(game, current4.Type, (StationType)current4.StationType.Value, fleetInfo.ID, current4.TargetSystemID, current4.TargetOrbitalObjectID, null, 1, false, null, null);
					this.TotalBuildStationsCost += (double)missionEstimate.ConstructionCost;
					if (missionEstimate.TotalTurns - 1 - missionEstimate.TurnsToReturn <= 1)
					{
						this.PendingBuildStationsCost += (double)missionEstimate.ConstructionCost;
					}
				}
			}
			foreach (MissionInfo current5 in 
				from x in game.GameDatabase.GetMissionInfos()
				where x.Type == MissionType.UPGRADE_STN && x.Duration > 0
				select x)
			{
				FleetInfo fleetInfo2 = game.GameDatabase.GetFleetInfo(current5.FleetID);
				if (game.GameDatabase.GetStationInfo(current5.TargetOrbitalObjectID) != null && fleetInfo2.PlayerID == playerInfo.ID && game.GameDatabase.GetWaypointsByMissionID(current5.ID).First<WaypointInfo>().Type != WaypointType.ReturnHome)
				{
					StationInfo stationInfo = game.GameDatabase.GetStationInfo(current5.TargetOrbitalObjectID);
					if (stationInfo.DesignInfo.StationLevel + 1 <= 5)
					{
                        MissionEstimate missionEstimate2 = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(game, current5.Type, stationInfo.DesignInfo.StationType, fleetInfo2.ID, current5.TargetSystemID, current5.TargetOrbitalObjectID, null, stationInfo.DesignInfo.StationLevel + 1, false, null, null);
						DesignInfo designInfo2 = DesignLab.CreateStationDesignInfo(game.AssetDatabase, game.GameDatabase, fleetInfo2.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, false);
						this.TotalBuildStationsCost += (double)designInfo2.SavingsCost;
						if (missionEstimate2.TotalTurns - 1 - missionEstimate2.TurnsToReturn <= 1)
						{
							this.PendingBuildStationsCost += (double)designInfo2.SavingsCost;
						}
					}
				}
			}
			foreach (StationInfo current6 in game.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID))
			{
				List<DesignModuleInfo> queuedModules = game.GameDatabase.GetQueuedStationModules(current6.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
				if (queuedModules.Count > 0)
				{
					LogicalModule logicalModule = game.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == game.GameDatabase.GetModuleAsset(queuedModules.First<DesignModuleInfo>().ModuleID));
					if (logicalModule != null)
					{
						this.PendingStationsModulesCost += (double)logicalModule.SavingsCost;
					}
					foreach (DesignModuleInfo dmi in queuedModules)
					{
						logicalModule = game.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == game.GameDatabase.GetModuleAsset(dmi.ModuleID));
						if (logicalModule != null)
						{
							this.TotalStationsModulesCost += (double)logicalModule.SavingsCost;
						}
					}
				}
			}
			this.TotalRevenue = this.TradeRevenue + this.TaxRevenue + this.IORevenue + this.SavingsInterest;
			double num8 = (double)((float)num2 / (float)list.Count);
			double num9 = (double)((num8 != 0.0) ? ((assetdb.BaseCorruptionRate + 0.02f * (playerInfo.RateImmigration * 10f)) / (2f * playerInfo.RateGovernmentResearch)) : 0f);
			this.RequiredSecurity = (int)Math.Ceiling(num9 * 100.0);
			if (playerFactionInfo.Name == "loa")
			{
				this.RequiredSecurity = 0;
			}
			float num10 = Math.Max(0f, (assetdb.BaseCorruptionRate + 0.02f * (playerInfo.RateImmigration * 10f) - 2f * (playerInfo.RateGovernmentResearch * playerInfo.RateGovernmentSecurity)) * ((float)num2 / (float)list.Count));
			this.CorruptionExpenses = this.TotalRevenue * (double)num10;
			if (playerFactionInfo.Name == "loa")
			{
				this.CorruptionExpenses = 0.0;
			}
			this.UpkeepExpenses = this.CurrentShipUpkeepExpenses + this.CurrentStationUpkeepExpenses + this.AdditionalUpkeepExpenses;
			this.TotalExpenses = this.UpkeepExpenses + this.ColonySupportExpenses + this.DebtInterest + this.CorruptionExpenses;
			this.OperatingBudget = this.TotalRevenue - this.TotalExpenses;
			this.DisposableIncome = Math.Max(this.OperatingBudget, 0.0);
			this.NetSavingsLoss = Math.Max(-this.OperatingBudget, 0.0) + this.PendingBuildShipsCost + this.PendingBuildStationsCost + this.PendingStationsModulesCost;
			this.RequestedGovernmentSpending = this.DisposableIncome * (double)playerInfo.RateGovernmentResearch;
			SpendingPool spendingPool = new SpendingPool();
			this.ResearchSpending = new ResearchSpending(playerInfo, this.DisposableIncome - this.RequestedGovernmentSpending, spendingPool, spendingCaps);
			this.SecuritySpending = new SecuritySpending(playerInfo, this.RequestedGovernmentSpending * (double)playerInfo.RateGovernmentSecurity, spendingPool, spendingCaps);
			this.StimulusSpending = new StimulusSpending(playerInfo, this.RequestedGovernmentSpending * (double)playerInfo.RateGovernmentStimulus, spendingPool, spendingCaps);
			this.ProjectedGovernmentSpending = this.SecuritySpending.ProjectedTotal + this.StimulusSpending.ProjectedTotal;
			this.UnspentIncome = spendingPool.Excess;
			this.NetSavingsIncome = this.DisposableIncome - this.ResearchSpending.RequestedTotal - this.SecuritySpending.RequestedTotal - this.StimulusSpending.RequestedTotal + this.UnspentIncome;
			this.ProjectedSavings = this.CurrentSavings + this.NetSavingsIncome - this.NetSavingsLoss;
		}
		public static Budget GenerateBudget(GameSession sim, PlayerInfo playerInfo, IEnumerable<DesignInfo> additionalShipDesignInfos, BudgetProjection budgetProjection)
		{
			FactionInfo factionInfo = sim.GameDatabase.GetFactionInfo(playerInfo.FactionID);
			double maxDriveSpeed = (double)sim.GameDatabase.FindCurrentDriveSpeedForPlayer(playerInfo.ID);
			double playerIncomeFromTrade = sim.GetPlayerIncomeFromTrade(playerInfo.ID);
			SpendingCaps spendingCaps = new SpendingCaps(sim.GameDatabase, playerInfo, budgetProjection);
			List<StationInfo> stationInfos = sim.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID).ToList<StationInfo>();
			List<ColonyInfo> list = sim.GameDatabase.GetPlayerColoniesByPlayerId(playerInfo.ID).ToList<ColonyInfo>();
			List<TreatyInfo> list2 = (
				from x in sim.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>()
				where x.Type == TreatyType.Protectorate && x.InitiatingPlayerId == playerInfo.ID
				select x).ToList<TreatyInfo>();
			foreach (TreatyInfo current in list2)
			{
				if (current.Active)
				{
					list.AddRange(sim.GameDatabase.GetPlayerColoniesByPlayerId(current.ReceivingPlayerId));
				}
			}
			Dictionary<int, PlanetInfo> planetInfoMapForColonies = Colony.GetPlanetInfoMapForColonies(sim.GameDatabase, list);
			Dictionary<int, OrbitalObjectInfo> orbitalObjectInfoMapForColonies = Colony.GetOrbitalObjectInfoMapForColonies(sim.GameDatabase, list);
			Dictionary<int, StarSystemInfo> starSystemInfoMapForOrbitalObjects = Colony.GetStarSystemInfoMapForOrbitalObjects(sim.GameDatabase, orbitalObjectInfoMapForColonies.Values);
			HashSet<int> starSystemsWithGates = GameSession.GetStarSystemsWithGates(sim.GameDatabase, playerInfo.ID, starSystemInfoMapForOrbitalObjects.Keys);
			List<FleetInfo> list3 = sim.GameDatabase.GetFleetInfosByPlayerID(playerInfo.ID, FleetType.FL_ALL).ToList<FleetInfo>();
			List<FleetInfo> eliteFleetInfos = (
				from x in list3
				where sim.GameDatabase.GetAdmiralTraits(x.AdmiralID).Contains(AdmiralInfo.TraitType.Elite)
				select x).ToList<FleetInfo>();
			if (eliteFleetInfos.Count > 0)
			{
				list3.RemoveAll((FleetInfo x) => eliteFleetInfos.Contains(x));
			}
			List<DesignInfo> shipDesignInfos = GameSession.MergeShipDesignInfos(sim.GameDatabase, list3, false);
			List<DesignInfo> eliteShipDesignInfos = GameSession.MergeShipDesignInfos(sim.GameDatabase, eliteFleetInfos, false);
			List<DesignInfo> reserveShipDesignInfos = GameSession.MergeShipDesignInfos(sim.GameDatabase, list3, true);
			return new Budget(sim.GameDatabase, sim.AssetDatabase, sim, playerInfo, factionInfo, maxDriveSpeed, playerIncomeFromTrade, spendingCaps, list, planetInfoMapForColonies, orbitalObjectInfoMapForColonies, starSystemInfoMapForOrbitalObjects, starSystemsWithGates, stationInfos, reserveShipDesignInfos, shipDesignInfos, eliteShipDesignInfos, additionalShipDesignInfos);
		}
	}
}
