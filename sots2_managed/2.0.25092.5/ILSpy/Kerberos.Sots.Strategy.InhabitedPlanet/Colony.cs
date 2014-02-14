using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy.InhabitedPlanet
{
	internal class Colony
	{
		public enum OutputRate
		{
			Trade,
			Infra,
			Terra,
			ShipCon,
			OverDev
		}
		public const float INDSYS_IMPERIAL_POPULATION_MOD = 0.2f;
		public const long BLEEDPOP_MIN_REMAINING = 10000L;
		public const float POPULATION_GROWTH_MOD = 1f;
		public const float POPULATION_GROWTH_EXP = 2f;
		public static float GetConstructionPoints(GameSession sim, ColonyInfo colony)
		{
			if (GameSession.InstaBuildHackEnabled)
			{
				return 1E+09f;
			}
			return (float)Colony.GetIndustrialOutput(sim, colony, sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID)) * colony.ShipConRate;
		}
		public static double GetTaxRevenue(App game, ColonyInfo colony)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colony.PlayerID);
			return Colony.GetTaxRevenue(game, playerInfo, colony);
		}
		public static double GetTaxRevenue(App game, PlayerInfo player, ColonyInfo colony)
		{
			double num = (double)((player.RateTax != 0f) ? (player.RateTax * 100f / game.AssetDatabase.TaxDivider) : 0f);
			double num2 = 0.0;
			bool flag = game.AssetDatabase.GetFaction(player.FactionID).HasSlaves();
			if (colony.RebellionType != RebellionType.None)
			{
				return num2;
			}
			num2 += colony.ImperialPop * num;
			ColonyFactionInfo[] factions = colony.Factions;
			for (int i = 0; i < factions.Length; i++)
			{
				ColonyFactionInfo colonyFactionInfo = factions[i];
				if (!flag || colonyFactionInfo.FactionID == player.FactionID)
				{
					double num3 = colonyFactionInfo.CivilianPop * num * 2.0;
					if (colonyFactionInfo.FactionID != player.FactionID)
					{
						num3 *= (double)game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AlienCivilianTaxRate, player.ID);
					}
					num2 += num3;
				}
			}
			float num4 = game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIRevenueBonus, player.ID) * game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIBenefitBonus, player.ID);
			num4 += game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TaxRevenue, player.ID) - 1f;
			num4 += game.AssetDatabase.GetAIModifier(game, DifficultyModifiers.RevenueBonus, player.ID);
			num2 += num2 * (double)num4;
			if (!game.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID).IsOpen)
			{
				num2 *= 0.89999997615814209;
			}
			HomeworldInfo playerHomeworld = game.GameDatabase.GetPlayerHomeworld(colony.PlayerID);
			if (playerHomeworld != null && playerHomeworld.ColonyID == colony.ID)
			{
				num2 *= (double)game.AssetDatabase.HomeworldTaxBonusMod;
			}
			return num2;
		}
		public static double CalcOverharvestRate(AssetDatabase assetdb, GameDatabase gamedb, ColonyInfo colony, PlanetInfo planet)
		{
			double num = (double)planet.MaxResources * (double)(assetdb.MaxOverharvestRate * gamedb.GetStratModifier<float>(StratModifiers.StripMiningMaximum, colony.PlayerID));
			num = Math.Min(num, (double)planet.Resources);
			double num2 = 0.0;
			if (colony.OverharvestRate > 0f)
			{
				num2 = (double)Math.Max(gamedb.GetStratModifier<float>(StratModifiers.MinOverharvestRate, colony.PlayerID), colony.OverharvestRate) * num;
				double num3 = Math.Max(Math.Min(gamedb.GetTotalPopulation(colony) * 1E-05, 1.0), 0.0);
				num2 *= num3;
				num2 = Math.Max(1.0, num2);
			}
			num2 = Math.Min(num2, num);
			if (assetdb.GetFaction(gamedb.GetPlayerInfo(colony.PlayerID).FactionID).Name == "zuul" && num >= 5.0 && num2 < 5.0)
			{
				num2 = 5.0;
			}
			return num2;
		}
		private static double CalcOverharvestFromOverpopulation(GameDatabase db, ColonyInfo colony, PlanetInfo planet)
		{
			Faction faction = db.AssetDatabase.GetFaction(db.GetPlayerFactionID(colony.PlayerID));
			double result = 0.0;
			double maxCivilianPop = Colony.GetMaxCivilianPop(db, db.GetPlanetInfo(colony.OrbitalObjectID));
			double num = (double)db.GetStratModifierFloatToApply(StratModifiers.OverPopulationPercentage, colony.PlayerID) * maxCivilianPop;
			double civilianPopulation = db.GetCivilianPopulation(colony.OrbitalObjectID, faction.ID, faction.HasSlaves());
			if (civilianPopulation > num)
			{
				double num2 = (civilianPopulation - num) / (maxCivilianPop - num);
				num2 = Math.Max(Math.Min(num2, 1.0), 0.0);
				double val = (double)db.GetStratModifierFloatToApply(StratModifiers.OverharvestFromPopulationModifier, colony.PlayerID) * num2;
				result = Math.Min((double)planet.Resources, val);
			}
			return result;
		}
		public static bool CanBeOverdeveloped(AssetDatabase assetdb, GameDatabase gamedb, ColonyInfo colony, PlanetInfo planet)
		{
			return colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.Developed && planet.Size >= (float)assetdb.SuperWorldSizeConstraint && gamedb.GetStratModifier<bool>(StratModifiers.AllowSuperWorlds, colony.PlayerID);
		}
		public static double GetCivilianIndustrialOutput(GameSession sim, ColonyInfo colony)
		{
			double num = 0.0;
			ColonyFactionInfo[] factions = colony.Factions;
			for (int i = 0; i < factions.Length; i++)
			{
				ColonyFactionInfo colonyFactionInfo = factions[i];
				double num2 = 1.0;
				if (colonyFactionInfo.Morale > 80)
				{
					num2 = 1.5;
				}
				else
				{
					if (colonyFactionInfo.Morale < 20)
					{
						num2 = 0.5;
					}
				}
				num += colonyFactionInfo.CivilianPop / 1000000.0 * num2 * (double)sim.AssetDatabase.CivilianProductionMultiplier;
			}
			return num;
		}
		public static double GetSlaveIndustrialOutput(GameSession sim, ColonyInfo colony)
		{
			double num = 0.0;
			ColonyFactionInfo[] factions = colony.Factions;
			for (int i = 0; i < factions.Length; i++)
			{
				ColonyFactionInfo colonyFactionInfo = factions[i];
				if (colonyFactionInfo.FactionID != sim.GameDatabase.GetPlayerFactionID(colony.PlayerID))
				{
					num += colonyFactionInfo.CivilianPop / 1000000.0 * (double)sim.AssetDatabase.SlaveProductionMultiplier * (double)(1f + colony.SlaveWorkRate);
				}
				else
				{
					double num2 = 1.0;
					if (colonyFactionInfo.Morale > 80)
					{
						num2 = 1.5;
					}
					else
					{
						if (colonyFactionInfo.Morale < 20)
						{
							num2 = 0.5;
						}
					}
					num += colonyFactionInfo.CivilianPop / 1000000.0 * num2 * (double)sim.AssetDatabase.CivilianProductionMultiplier;
				}
			}
			return num;
		}
		public static double GetIndustrialOutput(GameSession sim, ColonyInfo colony, PlanetInfo planet)
		{
			if (colony.RebellionType != RebellionType.None)
			{
				return 0.0;
			}
			int arg_2D_0 = planet.Resources;
			int playerFactionID = sim.GameDatabase.GetPlayerFactionID(colony.PlayerID);
			bool flag = sim.AssetDatabase.GetFaction(playerFactionID).HasSlaves();
			double num = colony.ImperialPop / 1000000.0 * (double)sim.AssetDatabase.ImperialProductionMultiplier;
			num += (flag ? Colony.GetSlaveIndustrialOutput(sim, colony) : Colony.GetCivilianIndustrialOutput(sim, colony));
			if (num < 1.0)
			{
				num = 1.0;
			}
			double num2 = (double)(planet.Infrastructure * (float)planet.Resources);
			double num3 = num + num2;
			OrbitalObjectInfo orbitalObjectInfo = sim.GameDatabase.GetOrbitalObjectInfo(planet.ID);
			int num4 = 0;
			List<StationInfo> list = (
				from x in sim.GameDatabase.GetStationForSystem(orbitalObjectInfo.StarSystemID)
				where x.DesignInfo.StationType == StationType.MINING
				select x).ToList<StationInfo>();
			foreach (StationInfo current in list)
			{
				num4 += current.DesignInfo.StationLevel * sim.AssetDatabase.MiningStationIOBonus;
			}
			int num5 = sim.GameDatabase.GetColonyInfosForSystem(orbitalObjectInfo.StarSystemID).Count((ColonyInfo x) => x.PlayerID == colony.PlayerID);
			if (num5 > 0)
			{
				num3 += (double)(num4 / num5);
			}
			num3 += Colony.CalcOverharvestRate(sim.AssetDatabase, sim.GameDatabase, colony, planet) * (double)sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.OverharvestModifier, colony.PlayerID);
			num3 *= (double)(sim.GameDatabase.GetStratModifier<float>(StratModifiers.IndustrialOutputModifier, colony.PlayerID) * sim.AssetDatabase.GlobalProductionModifier);
			float num6 = sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIProductionBonus, colony.PlayerID) * sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIBenefitBonus, colony.PlayerID);
			num6 += sim.AssetDatabase.GetAIModifier(sim.App, DifficultyModifiers.ProductionBonus, colony.PlayerID);
			num3 += num3 * (double)num6;
			num3 += (double)((colony.isHardenedStructures && !sim.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID).IsOpen) ? 0.9f : 1f);
			num3 = (colony.DamagedLastTurn ? (num3 / 2.0) : num3);
			if (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld)
			{
				num3 *= (double)sim.AssetDatabase.ForgeWorldIOBonus;
			}
			return num3;
		}
		public static float GetOverdevelopmentTarget(GameSession sim, PlanetInfo planet)
		{
			return (float)planet.Resources * planet.Size * sim.AssetDatabase.SuperWorldModifier;
		}
		public static float GetOverdevelopmentDelta(GameSession sim, ColonyInfo colony, PlanetInfo planet)
		{
			return (float)Colony.GetIndustrialOutput(sim, colony, planet) * colony.OverdevelopRate;
		}
		public static float GetBiosphereDelta(GameSession sim, ColonyInfo colony, PlanetInfo planet, double terraformingBonus)
		{
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			string factionName = sim.GameDatabase.GetFactionName(playerInfo.FactionID);
			float factionSuitability = sim.GameDatabase.GetFactionSuitability(factionName);
			float num = Math.Abs(planet.Suitability - factionSuitability);
			float num2 = Colony.GetTerraformingDelta(sim, colony, planet, terraformingBonus);
			if (num2 > num)
			{
				num2 = num;
			}
			float num3 = sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.BiosphereDestructionModifier, colony.PlayerID);
			List<FleetInfo> source = (
				from x in sim.GameDatabase.GetGardenerInfos()
				where x.IsGardener
				select sim.GameDatabase.GetFleetInfo(x.FleetId)).ToList<FleetInfo>();
			if (source.Any((FleetInfo x) => x != null && x.SystemID == colony.CachedStarSystemID))
			{
				num3 += sim.AssetDatabase.GlobalGardenerData.BiosphereDamage;
			}
			float num4 = (float)Math.Min((int)Math.Abs(num2) * 10, planet.Biosphere);
			num4 = (float)((int)(num4 * num3));
			num4 += (float)((int)Math.Max(0.0, (colony.Factions.Sum((ColonyFactionInfo x) => x.CivilianPop) / Colony.GetMaxCivilianPop(sim.GameDatabase, planet) * (double)planet.Biosphere - (double)planet.Biosphere) / 10.0));
			return -num4;
		}
		public static double GetInfrastructureDelta(GameSession sim, ColonyInfo colony, PlanetInfo planet)
		{
			double industrialOutput = Colony.GetIndustrialOutput(sim, colony, planet);
			double num = industrialOutput / 50000.0 * (double)colony.InfraRate;
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			if (!playerInfo.isStandardPlayer)
			{
				num += 0.02;
			}
			return Math.Min(num, (double)(1f - planet.Infrastructure));
		}
		public static float GetBiosphereBurnDelta(GameSession sim, ColonyInfo colony, PlanetInfo planet, FleetInfo fleet)
		{
            int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(sim, fleet.ID);
			return (float)(fleetLoaCubeValue / 100);
		}
		public static float GetTerraformingDelta(GameSession sim, ColonyInfo colony, PlanetInfo planet, double terraformingSpace)
		{
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			string factionName = sim.GameDatabase.GetFactionName(playerInfo.FactionID);
			float factionSuitability = sim.GameDatabase.GetFactionSuitability(factionName);
			float num = Math.Abs(planet.Suitability - factionSuitability);
			double industrialOutput = Colony.GetIndustrialOutput(sim, colony, planet);
			float num2 = sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TerraformingModifier, colony.PlayerID);
			List<FleetInfo> source = (
				from x in sim.GameDatabase.GetGardenerInfos()
				where x.IsGardener
				select sim.GameDatabase.GetFleetInfo(x.FleetId)).ToList<FleetInfo>();
			if (source.Any((FleetInfo x) => x != null && x.SystemID == colony.CachedStarSystemID))
			{
				num2 += sim.AssetDatabase.GlobalGardenerData.Terrforming;
			}
			float num3 = (float)(industrialOutput * 0.01) * colony.TerraRate * num2;
			float num4 = 0f;
			StationInfo stationForSystemPlayerAndType = sim.GameDatabase.GetStationForSystemPlayerAndType(sim.GameDatabase.GetOrbitalObjectInfo(planet.ID).StarSystemID, colony.PlayerID, StationType.CIVILIAN);
			if (stationForSystemPlayerAndType != null)
			{
				foreach (DesignModuleInfo current in stationForSystemPlayerAndType.DesignInfo.DesignSections[0].Modules)
				{
					if (current.StationModuleType.Value == ModuleEnums.StationModuleType.Terraform)
					{
						num4 += num3 * 0.25f;
					}
				}
			}
			num3 += num4 + (float)terraformingSpace;
			if (!playerInfo.isStandardPlayer)
			{
				num3 += 10f;
			}
			if (num3 > num)
			{
				num3 = num;
			}
			if (planet.Suitability > factionSuitability)
			{
				num3 *= -1f;
			}
			return num3;
		}
		public static float GetShipConstResources(GameSession sim, ColonyInfo colony, PlanetInfo planet)
		{
			double industrialOutput = Colony.GetIndustrialOutput(sim, colony, planet);
			return (float)industrialOutput * colony.ShipConRate;
		}
		public static double EstimateColonyDevelopmentCost(GameSession game, int planetId, int playerId)
		{
			ColonyInfo colonyInfo = new ColonyInfo
			{
				ID = 0,
				PlayerID = playerId,
				OrbitalObjectID = planetId,
				ImperialPop = 100.0,
				CivilianWeight = 0f,
				TurnEstablished = game.GameDatabase.GetTurnCount(),
				TerraRate = 0.5f,
				InfraRate = 0.5f,
				ShipConRate = 0f,
				TradeRate = 0f,
				DamagedLastTurn = false
			};
			return Colony.GetColonySupportCost(game.AssetDatabase, game.GameDatabase, colonyInfo);
		}
		public static double GetClimateHazard(FactionInfo factionInfo, PlanetInfo planetInfo)
		{
			return (double)Math.Abs(factionInfo.IdealSuitability - planetInfo.Suitability);
		}
		public static ColonyStage GetColonyStage(GameDatabase gamedb, int playerId, double hazard)
		{
			if (hazard < 100.0)
			{
				return ColonyStage.Open;
			}
			if (hazard < (double)(300 + gamedb.GetStratModifier<int>(StratModifiers.DomeStageModifier, playerId)))
			{
				return ColonyStage.Domed;
			}
			return ColonyStage.Underground;
		}
		public static double GetColonyStageModifier(GameDatabase gamedb, int playerId, double hazard)
		{
			switch (Colony.GetColonyStage(gamedb, playerId, hazard))
			{
			case ColonyStage.Open:
				return 1.0;
			case ColonyStage.Domed:
				return 2.0;
			}
			return (double)(3f + gamedb.GetStratModifier<float>(StratModifiers.CavernDmodModifier, playerId));
		}
		public static double GetColonyDistanceModifier(double supportDistance, double driveSpeed, bool gatePresent)
		{
			if (gatePresent)
			{
				return 1.0;
			}
			driveSpeed = Math.Max(driveSpeed, 0.001);
			return Math.Max(supportDistance / driveSpeed, 1.0);
		}
		public static double GetColonySupportCost(GameDatabase gamedb, AssetDatabase assetdb, PlayerInfo playerInfo, FactionInfo playerFactionInfo, OrbitalObjectInfo targetOrbitalObjectInfo, PlanetInfo targetPlanetInfo, StarSystemInfo targetStarSystemInfo, Dictionary<int, OrbitalObjectInfo> playerOrbitalObjectInfos, Dictionary<int, PlanetInfo> playerPlanetInfos, Dictionary<int, StarSystemInfo> playerStarSystemInfos, bool gateAtSystem, double playerDriveSpeed)
		{
			if (playerFactionInfo.Name == "loa")
			{
				return 0.0;
			}
			float arg_22_0 = playerFactionInfo.IdealSuitability;
			double climateHazard = Colony.GetClimateHazard(playerFactionInfo, targetPlanetInfo);
			if (climateHazard == 0.0)
			{
				return 0.0;
			}
			double? num = Colony.FindSupportDistanceForColony(playerInfo, playerFactionInfo, targetOrbitalObjectInfo, targetPlanetInfo, targetStarSystemInfo, playerOrbitalObjectInfos, playerPlanetInfos, playerStarSystemInfos);
			if (!num.HasValue)
			{
				return -1.0;
			}
			double colonyDistanceModifier = Colony.GetColonyDistanceModifier(num.Value, playerDriveSpeed, gateAtSystem);
			double colonyStageModifier = Colony.GetColonyStageModifier(gamedb, playerInfo.ID, climateHazard);
			double num2 = Math.Pow(climateHazard / 1.5, 1.8999999761581421) * colonyDistanceModifier * colonyStageModifier;
			return num2 * (double)assetdb.ColonySupportCostFactor;
		}
		public static double GetColonySupportCost(AssetDatabase assetdb, GameDatabase db, ColonyInfo colonyInfo)
		{
			PlayerInfo playerInfo = db.GetPlayerInfo(colonyInfo.PlayerID);
			OrbitalObjectInfo orbitalObjectInfo = db.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID);
			PlanetInfo planetInfo = db.GetPlanetInfo(colonyInfo.OrbitalObjectID);
			StarSystemInfo starSystemInfo = db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
			return Colony.GetColonySupportCost(assetdb, db, playerInfo, orbitalObjectInfo, planetInfo, starSystemInfo);
		}
		public static double GetColonySupportCost(AssetDatabase assetdb, GameDatabase db, PlayerInfo playerInfo, OrbitalObjectInfo targetOrbitalObjectInfo, PlanetInfo targetPlanetInfo, StarSystemInfo targetStarSystemInfo)
		{
			FactionInfo factionInfo = db.GetFactionInfo(playerInfo.FactionID);
			IEnumerable<ColonyInfo> playerColoniesByPlayerId = db.GetPlayerColoniesByPlayerId(playerInfo.ID);
			Dictionary<int, PlanetInfo> planetInfoMapForColonies = Colony.GetPlanetInfoMapForColonies(db, playerColoniesByPlayerId);
			Dictionary<int, OrbitalObjectInfo> orbitalObjectInfoMapForColonies = Colony.GetOrbitalObjectInfoMapForColonies(db, playerColoniesByPlayerId);
			Dictionary<int, StarSystemInfo> starSystemInfoMapForOrbitalObjects = Colony.GetStarSystemInfoMapForOrbitalObjects(db, orbitalObjectInfoMapForColonies.Values);
			bool gateAtSystem = db.SystemHasGate(targetStarSystemInfo.ID, playerInfo.ID);
			double playerDriveSpeed = (double)db.FindCurrentDriveSpeedForPlayer(playerInfo.ID);
			return Colony.GetColonySupportCost(db, assetdb, playerInfo, factionInfo, targetOrbitalObjectInfo, targetPlanetInfo, targetStarSystemInfo, orbitalObjectInfoMapForColonies, planetInfoMapForColonies, starSystemInfoMapForOrbitalObjects, gateAtSystem, playerDriveSpeed) * (double)db.GetStratModifier<float>(StratModifiers.ColonySupportCostModifier, playerInfo.ID);
		}
		public static double GetColonySupportCost(GameDatabase db, int playerId, int planetId)
		{
			PlayerInfo playerInfo = db.GetPlayerInfo(playerId);
			OrbitalObjectInfo orbitalObjectInfo = db.GetOrbitalObjectInfo(planetId);
			PlanetInfo planetInfo = db.GetPlanetInfo(planetId);
			StarSystemInfo starSystemInfo = db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
			return Colony.GetColonySupportCost(db.AssetDatabase, db, playerInfo, orbitalObjectInfo, planetInfo, starSystemInfo);
		}
		private static bool IsColonyDeveloped(FactionInfo playerFactionInfo, PlanetInfo planetInfo)
		{
			return planetInfo.Suitability == playerFactionInfo.IdealSuitability && (double)planetInfo.Infrastructure >= 1.0;
		}
		internal static double? FindSupportDistanceForColony(PlayerInfo playerInfo, FactionInfo playerFactionInfo, OrbitalObjectInfo targetOrbitalObjectInfo, PlanetInfo targetPlanetInfo, StarSystemInfo targetStarSystemInfo, Dictionary<int, OrbitalObjectInfo> playerOrbitalObjectInfos, Dictionary<int, PlanetInfo> playerPlanetInfos, Dictionary<int, StarSystemInfo> playerStarSystemInfos)
		{
			if (Colony.IsColonyDeveloped(playerFactionInfo, targetPlanetInfo))
			{
				return new double?(0.0);
			}
			double? num = null;
			foreach (PlanetInfo current in playerPlanetInfos.Values)
			{
				if (Colony.IsColonyDeveloped(playerFactionInfo, current))
				{
					OrbitalObjectInfo orbitalObjectInfo = playerOrbitalObjectInfos[current.ID];
					StarSystemInfo starSystemInfo = playerStarSystemInfos[orbitalObjectInfo.StarSystemID];
					double num2 = (double)(targetStarSystemInfo.Origin - starSystemInfo.Origin).Length;
					if (num.HasValue)
					{
						double num3 = num2;
						double? num4 = num;
						if (num3 >= num4.GetValueOrDefault() || !num4.HasValue)
						{
							continue;
						}
					}
					num = new double?(num2);
				}
			}
			return num;
		}
		public static Dictionary<int, PlanetInfo> GetPlanetInfoMapForColonies(GameDatabase db, IEnumerable<ColonyInfo> colonyInfos)
		{
			Dictionary<int, PlanetInfo> dictionary = new Dictionary<int, PlanetInfo>();
			List<PlanetInfo> list = (
				from x in colonyInfos
				select db.GetPlanetInfo(x.OrbitalObjectID)).ToList<PlanetInfo>();
			foreach (PlanetInfo current in list)
			{
				dictionary[current.ID] = current;
			}
			return dictionary;
		}
		public static Dictionary<int, OrbitalObjectInfo> GetOrbitalObjectInfoMapForColonies(GameDatabase db, IEnumerable<ColonyInfo> colonyInfos)
		{
			Dictionary<int, OrbitalObjectInfo> dictionary = new Dictionary<int, OrbitalObjectInfo>();
			List<OrbitalObjectInfo> list = (
				from x in colonyInfos
				select db.GetOrbitalObjectInfo(x.OrbitalObjectID)).ToList<OrbitalObjectInfo>();
			foreach (OrbitalObjectInfo current in list)
			{
				dictionary[current.ID] = current;
			}
			return dictionary;
		}
		public static Dictionary<int, StarSystemInfo> GetStarSystemInfoMapForOrbitalObjects(GameDatabase db, IEnumerable<OrbitalObjectInfo> orbitalObjectInfos)
		{
			Dictionary<int, StarSystemInfo> dictionary = new Dictionary<int, StarSystemInfo>();
			foreach (OrbitalObjectInfo current in orbitalObjectInfos)
			{
				if (!dictionary.ContainsKey(current.StarSystemID))
				{
					dictionary[current.StarSystemID] = db.GetStarSystemInfo(current.StarSystemID);
				}
			}
			return dictionary;
		}
		public static bool IsColonySelfSufficient(GameSession sim, ColonyInfo colony, PlanetInfo planet)
		{
			if (!sim.GameDatabase.GetPlayerInfo(colony.PlayerID).isStandardPlayer)
			{
				return true;
			}
			ColonyInfo colonyInfo = new ColonyInfo();
			colonyInfo.OrbitalObjectID = colony.OrbitalObjectID;
			colonyInfo.ImperialPop = colony.ImperialPop;
			colonyInfo.CivilianWeight = colony.CivilianWeight;
			colonyInfo.PlayerID = colony.PlayerID;
			colonyInfo.DamagedLastTurn = false;
			colonyInfo.CachedStarSystemID = sim.GameDatabase.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID;
			PlanetInfo planetInfo = new PlanetInfo();
			planetInfo.Biosphere = planet.Biosphere;
			planetInfo.Size = planet.Size;
			planetInfo.Suitability = planet.Suitability;
			planetInfo.Resources = planet.Resources;
			planetInfo.Infrastructure = planet.Infrastructure;
			planetInfo.ID = planet.ID;
			List<PlagueInfo> list = new List<PlagueInfo>();
			List<ColonyFactionInfo> list2;
			bool flag;
			return Colony.MaintainColony(sim, ref colonyInfo, ref planetInfo, ref list, 0.0, 0.0, null, out list2, out flag, true);
		}
		public static int SupportTripsTillSelfSufficient(GameSession sim, ColonyInfo colony, PlanetInfo planet, double colSpace, double terSpace, FleetInfo fleet)
		{
			List<PlagueInfo> list = new List<PlagueInfo>();
            int numColonizationShips = Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(sim, fleet.ID);
			if (colSpace > 0.0 && numColonizationShips > 0)
			{
				int num = 0;
				while (!Colony.IsColonySelfSufficient(sim, colony, planet))
				{
					num++;
					List<ColonyFactionInfo> list2;
					bool flag;
					Colony.MaintainColony(sim, ref colony, ref planet, ref list, colSpace, terSpace, fleet, out list2, out flag, false);
					if (num > 50)
					{
						return -1;
					}
				}
				return num;
			}
			return -1;
		}
		public static bool IsColonyPhase1Complete(GameSession sim, ColonyInfo colony, PlanetInfo planet)
		{
			if (planet.Infrastructure >= 1f && colony.ImperialPop > 0.0)
			{
				PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
				string factionName = sim.GameDatabase.GetFactionName(playerInfo.FactionID);
				float factionSuitability = sim.GameDatabase.GetFactionSuitability(factionName);
				if (planet.Suitability == factionSuitability)
				{
					return true;
				}
			}
			return false;
		}
		public static int PredictTurnsToPhase1Completion(GameSession sim, ColonyInfo colony, PlanetInfo planet)
		{
			List<PlagueInfo> list = new List<PlagueInfo>();
			if (!Colony.IsColonySelfSufficient(sim, colony, planet))
			{
				return -1;
			}
			int num = 0;
			while (!Colony.IsColonyPhase1Complete(sim, colony, planet))
			{
				List<ColonyFactionInfo> list2;
				bool flag;
				Colony.MaintainColony(sim, ref colony, ref planet, ref list, 0.0, 0.0, null, out list2, out flag, false);
				num++;
				if (num > 500)
				{
					return -1;
				}
			}
			return num;
		}
		public static void SetOutputRate(GameDatabase gamedb, AssetDatabase assetdb, ref ColonyInfo colony, PlanetInfo planet, Colony.OutputRate rate, float value)
		{
			Dictionary<Colony.OutputRate, float> dictionary = new Dictionary<Colony.OutputRate, float>
			{

				{
					Colony.OutputRate.Trade,
					colony.TradeRate
				},

				{
					Colony.OutputRate.Infra,
					colony.InfraRate
				},

				{
					Colony.OutputRate.ShipCon,
					colony.ShipConRate
				},

				{
					Colony.OutputRate.Terra,
					colony.TerraRate
				},

				{
					Colony.OutputRate.OverDev,
					colony.OverdevelopRate
				}
			};
			if (planet.Infrastructure >= 1f)
			{
				dictionary.Remove(Colony.OutputRate.Infra);
			}
			if (gamedb.GetPlanetHazardRating(colony.PlayerID, planet.ID, false) == 0f)
			{
				dictionary.Remove(Colony.OutputRate.Terra);
			}
			if (!Colony.CanBeOverdeveloped(assetdb, gamedb, colony, planet))
			{
				dictionary.Remove(Colony.OutputRate.OverDev);
			}
			AlgorithmExtensions.DistributePercentages<Colony.OutputRate>(ref dictionary, rate, value);
			colony.InfraRate = (dictionary.Keys.Contains(Colony.OutputRate.Infra) ? dictionary[Colony.OutputRate.Infra] : 0f);
			colony.ShipConRate = (dictionary.Keys.Contains(Colony.OutputRate.ShipCon) ? dictionary[Colony.OutputRate.ShipCon] : 0f);
			colony.TerraRate = (dictionary.Keys.Contains(Colony.OutputRate.Terra) ? dictionary[Colony.OutputRate.Terra] : 0f);
			colony.TradeRate = (dictionary.Keys.Contains(Colony.OutputRate.Trade) ? dictionary[Colony.OutputRate.Trade] : 0f);
			colony.OverdevelopRate = (dictionary.Keys.Contains(Colony.OutputRate.OverDev) ? dictionary[Colony.OutputRate.OverDev] : 0f);
		}
		public static double CalcSuitMod(FactionInfo faction, double suitability)
		{
			double val = 0.0;
			double val2 = 20.0;
			suitability = Math.Min(val2, Math.Max(suitability, val));
			double num = (double)faction.IdealSuitability;
			double suitabilityTolerance = Player.GetSuitabilityTolerance();
			double val3 = Math.Abs(num - suitability);
			return Math.Min(val3, suitabilityTolerance);
		}
		public static double CalcPopulationGrowth(GameSession sim, ColonyInfo colony, double curPopulation, double growthModifier, int populationSupply, FactionInfo populationFaction)
		{
			if (colony.DamagedLastTurn)
			{
				colony.DamagedLastTurn = false;
				return 0.0;
			}
			if (sim.GameDatabase.GetMissionInfos().Any((MissionInfo x) => x.TargetSystemID == colony.CachedStarSystemID && x.Type == MissionType.EVACUATE))
			{
				return 0.0;
			}
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID);
			FactionInfo factionInfo = sim.GameDatabase.GetFactionInfo(sim.GameDatabase.GetPlayerFactionID(colony.PlayerID));
			double num = (double)Math.Abs(planetInfo.Suitability - populationFaction.IdealSuitability);
			double num2 = 1.0;
			if (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowIdealAlienGrowthRate, colony.PlayerID) && populationFaction.ID != sim.GameDatabase.GetPlayerFactionID(colony.PlayerID))
			{
				num = 0.0;
			}
			if (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, colony.PlayerID))
			{
				num = 0.0;
				num2 = (double)Colony.GetLoaGrowthPotential(sim, planetInfo.ID, colony.CachedStarSystemID, colony.PlayerID);
			}
			double num3 = (curPopulation * ((num > 0.0) ? Math.Min(20.0 / num, 1.0) : 1.0) + (double)populationSupply) * num2;
			num3 *= growthModifier;
			StarSystemInfo starSystemInfo = sim.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID);
			if (starSystemInfo != null)
			{
				if (starSystemInfo.IsOpen && populationFaction.ID == factionInfo.ID)
				{
					num3 *= sim.AssetDatabase.GetGlobal<double>("OpenSystemNativeModifier");
				}
				else
				{
					if (starSystemInfo.IsOpen && populationFaction.ID != factionInfo.ID)
					{
						num3 *= sim.AssetDatabase.GetGlobal<double>("OpenSystemAlienModifier");
					}
				}
			}
			float num4 = sim.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PopulationGrowthModifier, colony.PlayerID) - 1f;
			num4 += sim.AssetDatabase.GetAIModifier(sim.App, DifficultyModifiers.PopulationGrowthBonus, colony.PlayerID);
			if (colony.ReplicantsOn && curPopulation < sim.AssetDatabase.GetGlobal<double>("ReplicantGrowthLimit"))
			{
				PlayerTechInfo playerTechInfo = sim.GameDatabase.GetPlayerTechInfos(colony.PlayerID).FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "CYB_Replicant");
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				{
					num4 += sim.AssetDatabase.GetTechBonus<float>(playerTechInfo.TechFileID, "popgrowth_u100m");
				}
			}
			num3 += num3 * (double)num4;
			num3 = Math.Round(num3, MidpointRounding.AwayFromZero);
			if (sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerInfo(colony.PlayerID).FactionID).Name == "loa" && populationFaction.Name == "loa")
			{
				float num5 = 1f;
				if (starSystemInfo != null)
				{
					StellarClass stellarClass = new StellarClass(starSystemInfo.StellarClass);
					num5 = 1f / Math.Max((float)stellarClass.GetInterference() - 4f, 1f);
				}
				num3 *= (double)((1f - sim.GameDatabase.GetPlayerInfo(colony.PlayerID).RateTax * 10f) * num5);
			}
			return num3;
		}
		public static float GetLoaGrowthPotential(GameSession sim, int Planetid, int starsystemid, int playerid)
		{
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(Planetid);
			StarSystemInfo starSystemInfo = sim.GameDatabase.GetStarSystemInfo(starsystemid);
			double num = Math.Max(1.0 - (double)planetInfo.Biosphere / sim.AssetDatabase.GetGlobal<double>("SterilizationIndex"), 0.01);
			if (sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerInfo(playerid).FactionID).Name == "loa")
			{
				float num2 = 1f;
				if (starSystemInfo != null)
				{
					StellarClass stellarClass = new StellarClass(starSystemInfo.StellarClass);
					num2 = 1f / Math.Max((float)stellarClass.GetAverageInterference() - 4f, 1f);
				}
				num *= (double)((1f - sim.GameDatabase.GetPlayerInfo(playerid).RateTax * 10f) * num2);
			}
			return (float)num;
		}
		public static double DoPopulationChange(GameSession sim, double value, ColonyInfo colony, double maxPop, double maxDelta, double minDelta, int populationSupply, FactionInfo populationFaction, double growthModifier = 1.0)
		{
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			double val = playerInfo.isAIRebellionPlayer ? 50000000.0 : Math.Min(value + Math.Min(Colony.CalcPopulationGrowth(sim, colony, value, growthModifier, populationSupply, populationFaction), maxDelta), maxPop);
			return Math.Max(0.0, val);
		}
		public static double GetMaxImperialPop(GameDatabase gamedb, PlanetInfo pi)
		{
			double num = 0.0;
			ColonyInfo colonyInfoForPlanet = gamedb.GetColonyInfoForPlanet(pi.ID);
			if (colonyInfoForPlanet != null)
			{
				num = (double)gamedb.GetStratModifier<int>(StratModifiers.AdditionalMaxImperialPopulation, colonyInfoForPlanet.PlayerID) * 1000000.0;
			}
			return (double)pi.Size * 100000000.0 + num;
		}
		public static double GetMaxCivilianPop(GameDatabase gamedb, PlanetInfo pi)
		{
			double num = 0.0;
			ColonyInfo colonyInfoForPlanet = gamedb.GetColonyInfoForPlanet(pi.ID);
			if (colonyInfoForPlanet != null)
			{
				num = (double)gamedb.GetStratModifier<int>(StratModifiers.AdditionalMaxCivilianPopulation, colonyInfoForPlanet.PlayerID) * 1000000.0;
			}
			return (double)pi.Size * 200000000.0 + num;
		}
		public static double GetMaxSlavePop(GameDatabase gamedb, PlanetInfo pi)
		{
			return (double)pi.Size * 50000000.0;
		}
		public static double GetKilledSlavePopulation(GameSession sim, ColonyInfo colony, double currentSlaves)
		{
			return Math.Floor(Math.Min(currentSlaves, Math.Max(1000.0, currentSlaves * (double)(sim.AssetDatabase.MinSlaveDeathRate + (sim.AssetDatabase.MaxSlaveDeathRate - sim.AssetDatabase.MinSlaveDeathRate) * colony.SlaveWorkRate))));
		}
		public static void UpdatePopulation(GameSession sim, ref ColonyInfo colony, int populationSupply, ref List<ColonyFactionInfo> civPopulation, bool useSlaveRules)
		{
			int playerFactionID = sim.GameDatabase.GetPlayerFactionID(colony.PlayerID);
			FactionInfo faction = sim.GameDatabase.GetFactionInfo(playerFactionID);
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID);
			if (colony.ImperialPop > 0.0)
			{
				colony.ImperialPop = Colony.DoPopulationChange(sim, colony.ImperialPop, colony, (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.ForgeWorld) ? (Colony.GetMaxImperialPop(sim.GameDatabase, planetInfo) * (double)sim.AssetDatabase.ForgeWorldImpMaxBonus) : Colony.GetMaxImperialPop(sim.GameDatabase, planetInfo), 50000000.0, 50000000.0, populationSupply, faction, 1.0);
			}
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			if ((
				from x in civPopulation
				where !useSlaveRules || x.FactionID == faction.ID
				select x).Sum((ColonyFactionInfo y) => y.CivilianPop) == 0.0 && colony.ImperialPop > (double)sim.AssetDatabase.CivilianPopulationTriggerAmount && !playerInfo.isAIRebellionPlayer)
			{
				if (!civPopulation.Any((ColonyFactionInfo x) => x.FactionID == faction.ID))
				{
					civPopulation.Add(new ColonyFactionInfo
					{
						CivilianPop = (double)sim.AssetDatabase.CivilianPopulationStartAmount,
						FactionID = faction.ID,
						CivPopWeight = 1f,
						TurnEstablished = sim.GameDatabase.GetTurnCount(),
						OrbitalObjectID = planetInfo.ID,
						Morale = sim.AssetDatabase.CivilianPopulationStartMoral
					});
				}
				else
				{
					civPopulation.First((ColonyFactionInfo x) => x.FactionID == faction.ID).CivilianPop = (double)sim.AssetDatabase.CivilianPopulationStartAmount;
				}
				colony.ImperialPop -= (double)sim.AssetDatabase.CivilianPopulationStartAmount;
			}
			List<int> list = new List<int>();
			List<PlayerInfo> list2 = sim.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			foreach (PlayerInfo current in list2)
			{
				if (!list.Contains(current.FactionID) && sim.AssetDatabase.GetFaction(current.FactionID).Name != "zuul")
				{
					list.Add(current.FactionID);
				}
			}
			double num = (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld) ? (Colony.GetMaxCivilianPop(sim.GameDatabase, planetInfo) * (double)sim.AssetDatabase.GemWorldCivMaxBonus) : Colony.GetMaxCivilianPop(sim.GameDatabase, planetInfo);
			num *= (double)colony.CivilianWeight;
			if (useSlaveRules)
			{
				foreach (ColonyFactionInfo current2 in civPopulation)
				{
					if (current2.FactionID != faction.ID && current2.CivilianPop > 0.0)
					{
						current2.CivilianPop -= Colony.GetKilledSlavePopulation(sim, colony, current2.CivilianPop);
					}
				}
				OrbitalObjectInfo orbitalObjectInfo = sim.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID);
				StationInfo stationForSystemPlayerAndType = sim.GameDatabase.GetStationForSystemPlayerAndType(orbitalObjectInfo.StarSystemID, colony.PlayerID, StationType.CIVILIAN);
				if (stationForSystemPlayerAndType != null)
				{
					StarSystemInfo starSystemInfo = sim.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
					float num2 = sim.GameDatabase.FindCurrentDriveSpeedForPlayer(colony.PlayerID) * 2f;
					List<StarSystemInfo> list3 = sim.GameDatabase.GetSystemsInRange(starSystemInfo.Origin, num2).ToList<StarSystemInfo>();
					List<SlaveNode> list4 = new List<SlaveNode>();
					foreach (StarSystemInfo current3 in list3)
					{
						float distanceMod = (current3.Origin - starSystemInfo.Origin).Length / num2;
						if (distanceMod != 0f)
						{
							List<ColonyInfo> list5 = sim.GameDatabase.GetColonyInfosForSystem(current3.ID).ToList<ColonyInfo>();
							foreach (ColonyInfo current4 in list5)
							{
								int targetFactionId = sim.GameDatabase.GetPlayerFactionID(current4.PlayerID);
								if (targetFactionId != sim.GameDatabase.GetPlayerFactionID(colony.PlayerID) && !list4.Any((SlaveNode x) => x.DistanceMod == distanceMod && x.FactionId == targetFactionId))
								{
									list4.Add(new SlaveNode
									{
										DistanceMod = distanceMod,
										FactionId = targetFactionId
									});
								}
							}
						}
					}
					if (list4.Count > 0)
					{
						SlaveNode chosenTarget = App.GetSafeRandom().Choose(list4);
						double num3 = Math.Floor((double)((float)(stationForSystemPlayerAndType.DesignInfo.StationLevel * 50000) / chosenTarget.DistanceMod));
						double val = Math.Max(Math.Floor(num - civPopulation.Sum((ColonyFactionInfo x) => x.CivilianPop)), 0.0);
						num3 = Math.Min(num3, val);
						if (num3 > 0.0)
						{
							if (civPopulation.Any((ColonyFactionInfo x) => x.FactionID == chosenTarget.FactionId))
							{
								civPopulation.First((ColonyFactionInfo x) => x.FactionID == chosenTarget.FactionId).CivilianPop += num3;
							}
							else
							{
								civPopulation.Add(new ColonyFactionInfo
								{
									FactionID = chosenTarget.FactionId,
									Morale = 100,
									OrbitalObjectID = colony.OrbitalObjectID,
									TurnEstablished = sim.GameDatabase.GetTurnCount(),
									CivPopWeight = 0f,
									CivilianPop = num3
								});
							}
						}
					}
				}
				double maxSlavePop = Colony.GetMaxSlavePop(sim.GameDatabase, planetInfo);
				double slavePopulation = sim.GameDatabase.GetSlavePopulation(planetInfo.ID, faction.ID);
				if (slavePopulation <= maxSlavePop)
				{
					goto IL_91E;
				}
				using (List<ColonyFactionInfo>.Enumerator enumerator5 = civPopulation.GetEnumerator())
				{
					while (enumerator5.MoveNext())
					{
						ColonyFactionInfo current5 = enumerator5.Current;
						if (current5.FactionID == playerFactionID)
						{
							current5.CivilianPop = Math.Min(current5.CivilianPop, maxSlavePop * (double)current5.CivPopWeight);
						}
					}
					goto IL_91E;
				}
			}
			if (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowAlienImmigration, colony.PlayerID) && !playerInfo.isAIRebellionPlayer && sim.GameDatabase.GetStarSystemInfo(sim.GameDatabase.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID).IsOpen)
			{
				float num4 = sim.GameDatabase.GetPlayerInfo(colony.PlayerID).RateImmigration * 100f * sim.AssetDatabase.CitizensPerImmigrationPoint;
				foreach (int i in list)
				{
					if (!(sim.AssetDatabase.GetFaction(i).Name == "zuul"))
					{
						if (!civPopulation.Any((ColonyFactionInfo x) => x.FactionID == i))
						{
							civPopulation.Add(new ColonyFactionInfo
							{
								FactionID = i,
								CivilianPop = 0.0,
								CivPopWeight = 1f,
								TurnEstablished = sim.GameDatabase.GetTurnCount(),
								OrbitalObjectID = planetInfo.ID,
								Morale = sim.AssetDatabase.CivilianPopulationStartMoral
							});
						}
						double num5 = Math.Floor(num - sim.GameDatabase.GetCivilianPopulations(colony.OrbitalObjectID).Sum((ColonyFactionInfo x) => x.CivilianPop));
						if (num5 > 0.0)
						{
							ColonyFactionInfo colonyFactionInfo = civPopulation.First((ColonyFactionInfo x) => x.FactionID == i);
							colonyFactionInfo.CivilianPop += Math.Min((double)(num4 / (float)list.Count), num5);
						}
					}
				}
			}
			IL_91E:
			foreach (ColonyFactionInfo current6 in civPopulation)
			{
				if ((!useSlaveRules || current6.FactionID == faction.ID) && current6.CivilianPop > 0.0)
				{
					float num6 = current6.CivPopWeight;
					if (useSlaveRules && current6.FactionID != faction.ID)
					{
						num6 = 1f;
					}
					double num7 = current6.CivilianPop;
					double num8 = Math.Max(Math.Floor(num - sim.GameDatabase.GetCivilianPopulations(colony.OrbitalObjectID).Sum((ColonyFactionInfo x) => x.CivilianPop)), 0.0);
					if (num8 > 0.0)
					{
						num7 = num * (double)num6 * (double)sim.AssetDatabase.GetFaction(playerFactionID).GetImmigrationPopBonusValueForFaction(sim.AssetDatabase.GetFaction(current6.FactionID));
					}
					num7 = Math.Max(num7, current6.CivilianPop);
					num7 = Math.Min(num7, Math.Min(num, current6.CivilianPop + num8));
					double civilianPop = Colony.DoPopulationChange(sim, current6.CivilianPop, colony, num7, 20000000.0, 50000000.0, 0, sim.GameDatabase.GetFactionInfo(current6.FactionID), (double)sim.AssetDatabase.CivilianPopulationGrowthRateMod);
					current6.CivilianPop = civilianPop;
				}
			}
		}
		public static int CalcColonyRepairPoints(GameSession sim, ColonyInfo colony)
		{
			return (int)(colony.ImperialPop / 100000000.0 * (0.20000000298023224 * Colony.GetIndustrialOutput(sim, colony, sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID))) * (double)sim.GetStationRepairModifierForSystem(sim.GameDatabase.GetOrbitalObjectInfo(colony.OrbitalObjectID).StarSystemID, colony.PlayerID));
		}
		public static void SimulatePlagues(GameSession sim, ref ColonyInfo ci, ref PlanetInfo pi, ref List<PlagueInfo> plagues)
		{
			Random safeRandom = App.GetSafeRandom();
			double totalPopulation = sim.GameDatabase.GetTotalPopulation(ci);
			foreach (PlagueInfo current in plagues)
			{
				if (!sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(ci.PlayerID)).IsPlagueImmune(current.PlagueType))
				{
					double num = 0.0;
					double num2 = 0.0;
					float num3 = 0f;
					float minValue = 0f;
					float maxValue = 0f;
					float num4 = 0f;
					bool flag = false;
					switch (current.PlagueType)
					{
					case WeaponEnums.PlagueType.BASIC:
						num4 = 0f;
						minValue = 0.2f;
						maxValue = 0.4f;
						flag = false;
						break;
					case WeaponEnums.PlagueType.RETRO:
						num4 = 0f;
						minValue = 0.1f;
						maxValue = 0.3f;
						flag = false;
						break;
					case WeaponEnums.PlagueType.BEAST:
						num4 = 0f;
						minValue = 0.05f;
						maxValue = 0.2f;
						flag = true;
						break;
					case WeaponEnums.PlagueType.ASSIM:
						num4 = -0.1f;
						minValue = 0f;
						maxValue = 0f;
						flag = true;
						break;
					case WeaponEnums.PlagueType.NANO:
						num4 = 0f;
						current.InfectionRate += 1f;
						pi.Infrastructure = Math.Max(current.InfectionRate - current.InfectionRate, 0f);
						if (safeRandom.CoinToss(0.20000000298023224))
						{
							current.InfectedPopulationImperial = 0.0;
							current.InfectedPopulationCivilian = 0.0;
							continue;
						}
						continue;
					case WeaponEnums.PlagueType.XOMBIE:
						num4 = -0.1f;
						minValue = 0f;
						maxValue = 0f;
						flag = true;
						break;
					case WeaponEnums.PlagueType.ZUUL:
						num4 = 0f;
						minValue = 0f;
						maxValue = 0f;
						flag = false;
						break;
					}
					current.InfectionRate = Math.Max(current.InfectionRate - num4, 1f);
					current.InfectedPopulationCivilian *= (double)current.InfectionRate;
					current.InfectedPopulationImperial *= (double)current.InfectionRate;
					float num5 = safeRandom.NextInclusive(minValue, maxValue);
					double num6 = current.InfectedPopulationCivilian * (double)num5;
					double num7 = current.InfectedPopulationImperial * (double)num5;
					current.InfectedPopulationCivilian = Math.Max(current.InfectedPopulationCivilian - num6, 0.0);
					current.InfectedPopulationImperial = Math.Max(current.InfectedPopulationImperial - num7, 0.0);
					num2 += num6;
					num += num7;
					bool flag2 = false;
					if (flag)
					{
						double num8;
						double num9;
						switch (current.PlagueType)
						{
						case WeaponEnums.PlagueType.BEAST:
							num8 = current.InfectedPopulation * 3.0;
							num2 += num8 * 0.949999988079071;
							num += num8 * 0.05000000074505806;
							num9 = Math.Floor((totalPopulation - num2 - num - current.InfectedPopulation) / 100000.0);
							current.InfectedPopulationCivilian = Math.Max(current.InfectedPopulationCivilian - Math.Floor(num9 / 2.0), 0.0);
							current.InfectedPopulationImperial = Math.Max(current.InfectedPopulationImperial - Math.Floor(num9 / 2.0), 0.0);
							num2 += Math.Floor(num9 / 2.0);
							num += Math.Floor(num9 / 2.0);
							num3 += safeRandom.NextInclusive(1f, 3f) * (float)Math.Ceiling(current.InfectedPopulation / 100000.0);
							goto IL_80C;
						case WeaponEnums.PlagueType.ASSIM:
						{
							num8 = current.InfectedPopulation * 2.0;
							num2 += num8 * 0.75;
							num += num8 * 0.25;
							num9 = Math.Floor((totalPopulation - num2 - num - current.InfectedPopulation) / 50000.0);
							current.InfectedPopulationCivilian = Math.Max(current.InfectedPopulationCivilian - Math.Floor(num9 / 2.0), 0.0);
							current.InfectedPopulationImperial = Math.Max(current.InfectedPopulationImperial - Math.Floor(num9 / 2.0), 0.0);
							num2 += Math.Floor(num9 / 2.0);
							num += Math.Floor(num9 / 2.0);
							num3 += safeRandom.NextInclusive(1f, 2f) * (float)Math.Ceiling(current.InfectedPopulation / 500000.0);
							if (current.InfectedPopulationCivilian <= 0.0)
							{
								goto IL_80C;
							}
							if (current.InfectedPopulationCivilian <= ci.Factions.Sum((ColonyFactionInfo x) => x.CivilianPop) - num2 || current.InfectedPopulationImperial <= ci.ImperialPop - num)
							{
								goto IL_80C;
							}
							sim.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_ASSIMILATION_PLAGUE_PLANET_GAINED,
								EventMessage = TurnEventMessage.EM_ASSIMILATION_PLAGUE_PLANET_GAINED,
								ColonyID = ci.ID,
								PlayerID = current.LaunchingPlayer,
								TurnNumber = sim.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
							sim.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_ASSIMILATION_PLAGUE_PLANET_LOST,
								EventMessage = TurnEventMessage.EM_ASSIMILATION_PLAGUE_PLANET_LOST,
								ColonyID = ci.ID,
								PlayerID = ci.PlayerID,
								TargetPlayerID = current.LaunchingPlayer,
								TurnNumber = sim.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
							sim.GameDatabase.GetPlayerFactionID(ci.PlayerID);
							ci.Factions.First<ColonyFactionInfo>().CivilianPop += ci.ImperialPop;
							ci.ImperialPop = 1000.0;
							ci.PlayerID = current.LaunchingPlayer;
							num2 = 0.0;
							num = 0.0;
							num3 = 0f;
							flag2 = true;
							List<StationInfo> list = sim.GameDatabase.GetStationForSystem(ci.CachedStarSystemID).ToList<StationInfo>();
							using (List<StationInfo>.Enumerator enumerator2 = list.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									StationInfo current2 = enumerator2.Current;
									OrbitalObjectInfo orbitalObjectInfo = sim.GameDatabase.GetOrbitalObjectInfo(current2.OrbitalObjectID);
									if (orbitalObjectInfo != null && orbitalObjectInfo.ParentID == ci.OrbitalObjectID)
									{
										sim.GameDatabase.DestroyStation(sim, current2.ID, 0);
									}
								}
								goto IL_80C;
							}
							break;
						}
						case WeaponEnums.PlagueType.NANO:
							goto IL_80C;
						case WeaponEnums.PlagueType.XOMBIE:
							break;
						default:
							goto IL_80C;
						}
						num8 = current.InfectedPopulation * 10.0;
						current.InfectedPopulationCivilian += num8 * 0.75;
						current.InfectedPopulationImperial += num8 * 0.25;
						num9 = Math.Floor((totalPopulation - num2 - num - current.InfectedPopulation) / 100000.0);
						current.InfectedPopulationCivilian = Math.Max(current.InfectedPopulationCivilian - Math.Floor(num9 / 2.0), 0.0);
						current.InfectedPopulationImperial = Math.Max(current.InfectedPopulationImperial - Math.Floor(num9 / 2.0), 0.0);
						num2 += Math.Floor(num9 / 2.0);
						num += Math.Floor(num9 / 2.0);
						num3 += safeRandom.NextInclusive(1f, 5f) * (float)Math.Ceiling(current.InfectedPopulation / 50000.0);
					}
					IL_80C:
					ci.ImperialPop = Math.Max(ci.ImperialPop - num, 0.0);
					double num10 = ci.Factions.Sum((ColonyFactionInfo x) => x.CivilianPop);
					if (num10 > 0.0)
					{
						ColonyFactionInfo[] factions = ci.Factions;
						for (int i = 0; i < factions.Length; i++)
						{
							ColonyFactionInfo colonyFactionInfo = factions[i];
							colonyFactionInfo.CivilianPop = Math.Max(colonyFactionInfo.CivilianPop - num2 * (colonyFactionInfo.CivilianPop / num10), 0.0);
						}
					}
					num10 = ci.Factions.Sum((ColonyFactionInfo x) => x.CivilianPop);
					pi.Infrastructure = Math.Max(pi.Infrastructure - num3, 0f);
					current.InfectedPopulationImperial = Math.Min(current.InfectedPopulationImperial, ci.ImperialPop);
					current.InfectedPopulationCivilian = Math.Min(current.InfectedPopulationCivilian, num10);
					current.InfectionRate += num4;
					if (!flag2)
					{
						TurnEventMessage eventMessage;
						if ((num != 0.0 || num2 != 0.0) && num3 != 0f)
						{
							eventMessage = TurnEventMessage.EM_PLAGUE_DAMAGE_POP_STRUCT;
						}
						else
						{
							if (num != 0.0 || num2 != 0.0)
							{
								eventMessage = TurnEventMessage.EM_PLAGUE_DAMAGE_POP;
							}
							else
							{
								eventMessage = TurnEventMessage.EM_PLAGUE_DAMAGE_STRUCT;
							}
						}
						sim.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_PLAGUE_DAMAGE,
							EventMessage = eventMessage,
							PlagueType = current.PlagueType,
							ImperialPop = (float)num,
							CivilianPop = (float)num2,
							ColonyID = current.ColonyId,
							PlayerID = ci.PlayerID,
							TurnNumber = sim.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
		}
		public static bool MaintainColony(GameSession sim, ref ColonyInfo colony, ref PlanetInfo planet, ref List<PlagueInfo> plagues, double fleetCapacity, double terraformingBonus, FleetInfo fleet, out List<ColonyFactionInfo> civPopulation, out bool achievedSuperWorld, bool isSupplyRun = false)
		{
			OrbitalObjectInfo orbitalObjectInfo = sim.GameDatabase.GetOrbitalObjectInfo(planet.ID);
			if (!isSupplyRun && colony.OverharvestRate > sim.GameDatabase.GetStratModifier<float>(StratModifiers.MinOverharvestRate, colony.PlayerID))
			{
				if (colony.TurnsOverharvested > 5)
				{
					sim.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_OVERHARVEST_WARNING,
						EventMessage = TurnEventMessage.EM_OVERHARVEST_WARNING,
						PlayerID = colony.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = colony.OrbitalObjectID,
						ColonyID = colony.ID,
						TurnNumber = sim.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					colony.TurnsOverharvested = 0;
				}
				else
				{
					colony.TurnsOverharvested++;
				}
			}
			Random safeRandom = App.GetSafeRandom();
			achievedSuperWorld = false;
			bool result = false;
			civPopulation = sim.GameDatabase.GetCivilianPopulations(planet.ID).ToList<ColonyFactionInfo>();
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colony.PlayerID);
			bool flag = sim.AssetDatabase.GetFaction(playerInfo.FactionID).HasSlaves();
			if (!playerInfo.isAIRebellionPlayer)
			{
				Colony.SimulatePlagues(sim, ref colony, ref planet, ref plagues);
				civPopulation = colony.Factions.ToList<ColonyFactionInfo>();
				if (colony.RebellionType != RebellionType.None)
				{
					double civilianPopulation = sim.GameDatabase.GetCivilianPopulation(planet.ID, playerInfo.FactionID, flag);
					double num = Math.Min(colony.ImperialPop, 50000000.0);
					double num2 = Math.Min(civilianPopulation, (double)(5E+07f * colony.CivilianWeight));
					double num3 = 0.0;
					double num4 = 0.0;
					while (num > 0.0 && num2 > 0.0)
					{
						if (safeRandom.CoinToss(0.75))
						{
							int num5 = (int)Math.Min(num2, 1000000.0);
							num4 += (double)num5;
							num2 -= (double)num5;
						}
						else
						{
							int num6 = (int)Math.Min(num, 1000000.0);
							num3 += (double)num6;
							num -= (double)num6;
						}
					}
					int num7 = 0;
					int num8 = 0;
					List<FleetInfo> list = sim.GameDatabase.GetFleetInfoBySystemID(orbitalObjectInfo.StarSystemID, FleetType.FL_ALL).ToList<FleetInfo>();
					foreach (FleetInfo current in list)
					{
						if (current.PlayerID == colony.PlayerID && !current.IsReserveFleet)
						{
							num7 += sim.GameDatabase.GetFleetCruiserEquivalent(current.ID);
						}
						List<ShipInfo> list2 = sim.GameDatabase.GetShipInfoByFleetID(current.ID, false).ToList<ShipInfo>();
						foreach (ShipInfo current2 in list2)
						{
							if (current2.IsPoliceShip())
							{
								num8++;
							}
						}
					}
					num4 += (double)(1000000 * num7);
					colony.ImperialPop -= num3;
					colony.ImperialPop = Math.Max(0.0, colony.ImperialPop);
					foreach (ColonyFactionInfo current3 in civPopulation)
					{
						if (civilianPopulation == 0.0)
						{
							current3.CivilianPop = 0.0;
						}
						else
						{
							current3.CivilianPop -= Math.Ceiling(num4 * (current3.CivilianPop / civilianPopulation));
							current3.CivilianPop = Math.Max(0.0, current3.CivilianPop);
						}
					}
					planet.Infrastructure -= (float)Math.Floor((num4 + num3) / 1000000000.0);
					double num9 = civPopulation.Sum((ColonyFactionInfo x) => x.CivilianPop);
					float num10;
					if (num9 == 0.0)
					{
						num10 = 1f;
					}
					else
					{
						num10 = (float)((num4 - num3) / num9);
						num10 += (float)num7 * 0.0025f;
						num10 += (float)num8 * 0.02f;
					}
					if (safeRandom.CoinToss((double)num10))
					{
						sim.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_REBELLION_ENDED_WIN,
							EventMessage = TurnEventMessage.EM_REBELLION_ENDED_WIN,
							PlayerID = colony.PlayerID,
							SystemID = orbitalObjectInfo.StarSystemID,
							OrbitalID = colony.OrbitalObjectID,
							ColonyID = colony.ID,
							TurnNumber = sim.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						colony.RebellionType = RebellionType.None;
						using (List<ColonyFactionInfo>.Enumerator enumerator3 = civPopulation.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								ColonyFactionInfo current4 = enumerator3.Current;
								current4.Morale = 50;
							}
							goto IL_7F6;
						}
					}
					if (colony.ImperialPop <= 0.0)
					{
						sim.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_REBELLION_ENDED_LOSS,
							EventMessage = TurnEventMessage.EM_REBELLION_ENDED_LOSS,
							PlayerID = colony.PlayerID,
							SystemID = orbitalObjectInfo.StarSystemID,
							OrbitalID = colony.OrbitalObjectID,
							ColonyID = colony.ID,
							TurnNumber = sim.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
						string factionName = string.Format("{0} {1}", orbitalObjectInfo.Name, App.Localize("@UI_PLAYER_NAME_REBELLION_COLONY"));
						Faction faction = sim.AssetDatabase.GetFaction(playerInfo.FactionID);
						int orInsertIndyPlayerId = sim.GameDatabase.GetOrInsertIndyPlayerId(sim, playerInfo.FactionID, factionName, faction.SplinterAvatarPath());
						sim.GameDatabase.UpdateDiplomacyState(orInsertIndyPlayerId, playerInfo.ID, DiplomacyState.WAR, 500, true);
						List<PlayerInfo> list3 = sim.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>();
						foreach (PlayerInfo current5 in list3)
						{
							DiplomacyInfo diplomacyInfo = sim.GameDatabase.GetDiplomacyInfo(playerInfo.ID, current5.ID);
							if (sim.GameDatabase.GetFactionName(current5.FactionID) == "morrigi")
							{
								sim.GameDatabase.UpdateDiplomacyState(orInsertIndyPlayerId, current5.ID, diplomacyInfo.State, diplomacyInfo.Relations + 300, true);
							}
							else
							{
								sim.GameDatabase.UpdateDiplomacyState(orInsertIndyPlayerId, current5.ID, diplomacyInfo.State, diplomacyInfo.Relations, true);
							}
						}
						sim.GameDatabase.DuplicateStratModifiers(orInsertIndyPlayerId, playerInfo.ID);
						colony.ShipConRate = 0f;
						colony.TerraRate = 0.75f;
						colony.InfraRate = 0.25f;
						colony.TradeRate = 0f;
						colony.PlayerID = orInsertIndyPlayerId;
						colony.ImperialPop = 500.0;
						colony.RebellionType = RebellionType.None;
						using (List<ColonyFactionInfo>.Enumerator enumerator3 = civPopulation.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								ColonyFactionInfo current6 = enumerator3.Current;
								current6.Morale = 100;
							}
							goto IL_7F6;
						}
					}
					sim.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_REBELLION_ONGOING,
						EventMessage = TurnEventMessage.EM_REBELLION_ONGOING,
						PlayerID = colony.PlayerID,
						SystemID = orbitalObjectInfo.StarSystemID,
						OrbitalID = colony.OrbitalObjectID,
						ColonyID = colony.ID,
						TurnNumber = sim.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
					colony.RebellionTurns++;
					IL_7F6:
					colony.RepairPointsMax = Colony.CalcColonyRepairPoints(sim, colony);
					colony.RepairPoints = 0;
					return false;
				}
			}
			string factionName2 = sim.GameDatabase.GetFactionName(playerInfo.FactionID);
			float factionSuitability = sim.GameDatabase.GetFactionSuitability(factionName2);
			float num11 = (playerInfo.isAIRebellionPlayer || factionName2 == "loa") ? 0f : Math.Abs(planet.Suitability - factionSuitability);
			float num12 = (float)planet.Biosphere;
			int arg_872_0 = planet.Resources;
			if (isSupplyRun || Colony.IsColonySelfSufficient(sim, colony, planet))
			{
				double num13 = colony.ImperialPop + civPopulation.Sum((ColonyFactionInfo x) => x.CivilianPop);
				double num14 = num13 * (double)(2f / Math.Max(num11, 1f)) * (double)(100f + num12 / 100f);
				double num15 = Math.Truncate(Math.Max(0.0, num13 - num14));
				if (num15 > 0.0)
				{
					result = false;
					if (fleetCapacity > 0.0)
					{
						double num16 = Math.Min(fleetCapacity * 1.5, num15);
						fleetCapacity -= num16 / 1.5;
						num15 -= num16;
					}
				}
				else
				{
					result = true;
					if (colony.CurrentStage == Kerberos.Sots.Data.ColonyStage.Colony)
					{
						colony.CurrentStage = Kerberos.Sots.Data.ColonyStage.Developed;
					}
				}
				if (num15 > 0.0)
				{
					double num17 = num15 / 5.0;
					bool flag2 = playerInfo.isStandardPlayer && sim.GameDatabase.GetStratModifier<bool>(StratModifiers.ColonyStarvation, playerInfo.ID);
					if (flag2)
					{
						if (num13 != 0.0)
						{
							foreach (ColonyFactionInfo current7 in civPopulation)
							{
								double num18 = Math.Truncate(current7.CivilianPop / num13 * num17 * 2.0);
								current7.CivilianPop -= num18;
								num17 -= num18;
								current7.CivilianPop = Math.Max(current7.CivilianPop, 0.0);
							}
							colony.ImperialPop -= colony.ImperialPop / num13 * num17;
						}
					}
					else
					{
						if (plagues.Count == 0)
						{
							Colony.UpdatePopulation(sim, ref colony, (int)(fleetCapacity * (double)(1f - sim.AssetDatabase.InfrastructureSupplyRatio)) * 10, ref civPopulation, flag);
						}
					}
				}
				else
				{
					if (plagues.Count == 0)
					{
						Colony.UpdatePopulation(sim, ref colony, (int)(fleetCapacity * (double)(1f - sim.AssetDatabase.InfrastructureSupplyRatio)) * 10, ref civPopulation, flag);
					}
				}
				colony.ImperialPop = Math.Truncate(colony.ImperialPop);
			}
			if (isSupplyRun && fleet != null)
			{
                float num19 = (float)Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(sim, fleet.ID);
				double num20 = fleetCapacity * (double)sim.AssetDatabase.InfrastructureSupplyRatio / 2.0 + (double)(num19 * 1f);
				planet.Infrastructure += (float)(num20 * 0.001);
				planet.Infrastructure = Math.Min(planet.Infrastructure, 1f);
				if (sim.App.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerInfo.ID))
				{
					planet.Biosphere -= (int)Colony.GetBiosphereBurnDelta(sim, colony, planet, fleet);
					planet.Biosphere = Math.Max(planet.Biosphere, 0);
				}
				else
				{
					float num21 = Colony.GetTerraformingDelta(sim, colony, planet, terraformingBonus);
					if (num21 > num11)
					{
						num21 = num11;
					}
					planet.Suitability += num21;
				}
			}
			else
			{
				double infrastructureDelta = Colony.GetInfrastructureDelta(sim, colony, planet);
				float arg_BB7_0 = planet.Infrastructure;
				planet.Infrastructure += (float)infrastructureDelta;
				planet.Infrastructure = Math.Min(planet.Infrastructure, 1f);
				if (planet.Infrastructure == 1f)
				{
					Colony.SetOutputRate(sim.GameDatabase, sim.AssetDatabase, ref colony, planet, Colony.OutputRate.Infra, 0f);
				}
				if (Colony.CanBeOverdeveloped(sim.AssetDatabase, sim.GameDatabase, colony, planet))
				{
					colony.OverdevelopProgress += Colony.GetOverdevelopmentDelta(sim, colony, planet);
					if (colony.OverdevelopProgress >= Colony.GetOverdevelopmentTarget(sim, planet))
					{
						achievedSuperWorld = true;
					}
				}
				else
				{
					colony.OverdevelopProgress = 0f;
					colony.OverdevelopRate = 0f;
				}
				if (!sim.App.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerInfo.ID))
				{
					float num22 = Colony.GetTerraformingDelta(sim, colony, planet, terraformingBonus);
					if (num22 > num11)
					{
						num22 = num11;
					}
					planet.Suitability += num22;
					if (num11 > 0f && num11 - num22 <= 0f)
					{
						Colony.SetOutputRate(sim.GameDatabase, sim.AssetDatabase, ref colony, planet, Colony.OutputRate.Terra, 0f);
					}
					if (num11 > 0f && num11 - num22 <= 0f && playerInfo.ID == sim.LocalPlayer.ID)
					{
						sim.App.SteamHelper.DoAchievement(AchievementType.SOTS2_HOT_COLD);
					}
					planet.Biosphere += (int)Colony.GetBiosphereDelta(sim, colony, planet, terraformingBonus);
					planet.Biosphere = Math.Max(0, planet.Biosphere);
				}
				planet.Resources -= (int)Colony.CalcOverharvestRate(sim.AssetDatabase, sim.GameDatabase, colony, planet);
				planet.Resources -= (int)Colony.CalcOverharvestFromOverpopulation(sim.GameDatabase, colony, planet);
				if (planet.Resources == 0 && playerInfo.ID == sim.LocalPlayer.ID)
				{
					sim.App.SteamHelper.DoAchievement(AchievementType.SOTS2_NOTHIN);
				}
				int num23 = 100;
				ColonyFactionInfo[] factions = colony.Factions;
				for (int i = 0; i < factions.Length; i++)
				{
					ColonyFactionInfo colonyFactionInfo = factions[i];
					num23 = Math.Min(num23, colonyFactionInfo.Morale);
				}
				if (!flag)
				{
					int num24 = 20;
					StarSystemInfo ssi = sim.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID);
					if (ssi != null && ssi.ProvinceID.HasValue)
					{
						int num25 = sim.GameDatabase.GetStarSystemInfos().Count((StarSystemInfo x) => x.ProvinceID.HasValue && ssi.ProvinceID.Value == x.ProvinceID.Value);
						num24 = Math.Max(num24 - 2 * num25, 0);
					}
					if (num23 < num24)
					{
						if (safeRandom.CoinToss((double)((float)(100 - num23) * 0.75f)))
						{
							colony.RebellionType = RebellionType.Civilian;
							colony.RebellionTurns = 0;
							sim.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_REBELLION_STARTED,
								EventMessage = TurnEventMessage.EM_REBELLION_STARTED,
								PlayerID = colony.PlayerID,
								SystemID = orbitalObjectInfo.StarSystemID,
								OrbitalID = colony.OrbitalObjectID,
								ColonyID = colony.ID,
								TurnNumber = sim.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
						else
						{
							sim.GameDatabase.InsertTurnEvent(new TurnEvent
							{
								EventType = TurnEventType.EV_REBELLION_STARTING,
								EventMessage = TurnEventMessage.EM_REBELLION_STARTING,
								PlayerID = colony.PlayerID,
								SystemID = orbitalObjectInfo.StarSystemID,
								OrbitalID = colony.OrbitalObjectID,
								ColonyID = colony.ID,
								TurnNumber = sim.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
						}
					}
				}
			}
			colony.RepairPoints = Colony.CalcColonyRepairPoints(sim, colony);
			colony.RepairPointsMax = colony.RepairPoints;
			if (sim.App.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerInfo.ID))
			{
				result = (planet.Biosphere == 0);
			}
			return result;
		}
	}
}
