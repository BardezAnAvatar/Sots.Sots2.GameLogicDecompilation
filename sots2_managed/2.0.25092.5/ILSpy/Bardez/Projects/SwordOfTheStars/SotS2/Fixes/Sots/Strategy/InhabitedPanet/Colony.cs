using System;
using System.Collections.Generic;

using Kerberos.Sots;
using Kerberos.Sots.Data;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Steam;
using Source = Kerberos.Sots.Strategy.InhabitedPlanet;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Strategy.InhabitedPanet
{
    /// <summary>This class contains performance enhancements to general SotS2 logic in Kerberos.Sots.Strategy.InhabitedPanet.Colony</summary>
	public static class Colony
    {
        /// <summary>Summarizes the empire-wide support cost of colonize without the recursive Linq summarizing that hits the DB redundantly</summary>
        /// <param name="db">GameDatabase to query</param>
        /// <param name="playerId">Player ID to get costs for</param>
        /// <param name="playerColonies">Collection of a player-owned colonies</param>
        /// <returns>The support cost of the empire's colonies</returns>
        internal static Double GetEmpireColonySupportCost(GameDatabase db, Int32 playerId, IEnumerable<ColonyInfo> playerColonies)
        {
            Double supportCosts;

            PlayerInfo playerInfo = db.GetPlayerInfo(playerId);
            FactionInfo factionInfo = db.GetFactionInfo(playerInfo.FactionID);

            //Loa have no support cost
            if (factionInfo.Name == "loa")  //short circuit
            {
                supportCosts = 0.0;
            }
            else
            {
                supportCosts = 0.0; //start with no base cost

                //get dictionaries of each Planet/OrbitalObject/StarSystem
                Dictionary<int, PlanetInfo> planetInfoMapForColonies = Source.Colony.GetPlanetInfoMapForColonies(db, playerColonies);
                Dictionary<int, OrbitalObjectInfo> orbitalObjectInfoMapForColonies = Source.Colony.GetOrbitalObjectInfoMapForColonies(db, playerColonies);
                Dictionary<int, StarSystemInfo> starSystemInfoMapForOrbitalObjects = Source.Colony.GetStarSystemInfoMapForOrbitalObjects(db, orbitalObjectInfoMapForColonies.Values);
                double playerDriveSpeed = (double)db.FindCurrentDriveSpeedForPlayer(playerInfo.ID);

                foreach (ColonyInfo colony in playerColonies)   //calculate the support cost for each individual colony
                {
                    OrbitalObjectInfo orbitalObjectInfo = db.GetOrbitalObjectInfo(colony.OrbitalObjectID);
                    PlanetInfo planetInfo = db.GetPlanetInfo(colony.OrbitalObjectID);
                    StarSystemInfo starSystemInfo = db.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
                    Boolean gateAtSystem = db.SystemHasGate(starSystemInfo.ID, playerInfo.ID);

                    double climateHazard = Source.Colony.GetClimateHazard(factionInfo, planetInfo);
                    if (climateHazard == 0.0)   //no support cost. continue
                        continue;

                    //this needs to be public within the code base
                    double? num = Source.Colony.FindSupportDistanceForColony(playerInfo, factionInfo, orbitalObjectInfo, planetInfo, starSystemInfo, orbitalObjectInfoMapForColonies, planetInfoMapForColonies, starSystemInfoMapForOrbitalObjects);
                    if (!num.HasValue)  //no support cost, continue
                    {
                        //The source method signature (later) returns -1 for some reason. It retuns a space buck; bug. source: GetColonySupportCost(GameDatabase gamedb, AssetDatabase assetdb, PlayerInfo playerInfo, FactionInfo playerFactionInfo, OrbitalObjectInfo targetOrbitalObjectInfo, PlanetInfo targetPlanetInfo, StarSystemInfo targetStarSystemInfo, Dictionary<int, OrbitalObjectInfo> playerOrbitalObjectInfos, Dictionary<int, PlanetInfo> playerPlanetInfos, Dictionary<int, StarSystemInfo> playerStarSystemInfos, bool gateAtSystem, double playerDriveSpeed)
                        continue;
                    }

                    double colonyDistanceModifier = Source.Colony.GetColonyDistanceModifier(num.Value, playerDriveSpeed, gateAtSystem);
                    double colonyStageModifier = Source.Colony.GetColonyStageModifier(db, playerInfo.ID, climateHazard);
                    double num2 = Math.Pow(climateHazard / 1.5, 1.8999999761581421) * colonyDistanceModifier * colonyStageModifier;
                    supportCosts += num2 * (double)db.AssetDatabase.ColonySupportCostFactor;
                }
            }

            return supportCosts;
        }

        /// <summary>Gets the tax revenue for a collection of ColonyInfo colonies</summary>
        /// <param name="game">App to query</param>
        /// <param name="player">Owning player to search on and apply info for</param>
        /// <param name="colony">Colony whose tax revenue is being calculated</param>
        /// <returns>The tax revenue for the specified colonies</returns>
        internal static Double GetTaxRevenue(App game, PlayerInfo player, IEnumerable<ColonyInfo> colonies)
        {
            Double taxRevenue = 0.0;
            Double taxRate = (player.RateTax != 0f) ? (player.RateTax * 100f / game.AssetDatabase.TaxDivider) : 0f;
            Boolean playerRaceHasSlaves = game.AssetDatabase.GetFaction(player.FactionID).HasSlaves();

            //pre-collect strat modifiers, outside of the loop
            Single alienCivilianTaxRate = game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AlienCivilianTaxRate, player.ID);
            Single aiTaxBonuses = game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIRevenueBonus, player.ID) * game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.AIBenefitBonus, player.ID);
            Single taxRevenueModifier = game.GameDatabase.GetStratModifierFloatToApply(StratModifiers.TaxRevenue, player.ID) - 1f;
            Single difficultyTaxRevenueBonus = game.AssetDatabase.GetAIModifier(game, DifficultyModifiers.RevenueBonus, player.ID);

            //calculate this above so that it can be applied for each colony. I would prefer to factor this out, but rebellions disallow that.
            Single consolidatedBonuses = aiTaxBonuses + taxRevenueModifier + difficultyTaxRevenueBonus;

            //This change might be considered possibly dangerous, though I doubt it.
            //The homeworld for protectorates doesn't exist, and that should be the only case of player colony combinations for tax calculations.
            //HomeworldInfo playerHomeworld = game.GameDatabase.GetPlayerHomeworld(colony.PlayerID);
            HomeworldInfo playerHomeworld = game.GameDatabase.GetPlayerHomeworld(player.ID);

            foreach (ColonyInfo colony in colonies)
            {
                Double colonyTaxRevenue = 0.0;

                if (colony.RebellionType != RebellionType.None) //if colony is rebelling
                    continue;

                colonyTaxRevenue = colony.ImperialPop * taxRate;
                ColonyFactionInfo[] factions = colony.Factions;
                for (Int32 i = 0; i < factions.Length; i++)
                {
                    ColonyFactionInfo colonyFactionInfo = factions[i];
                    if (!playerRaceHasSlaves || colonyFactionInfo.FactionID == player.FactionID)
                    {
                        Double civillianTax = colonyFactionInfo.CivilianPop * taxRate * 2.0;
                        if (colonyFactionInfo.FactionID != player.FactionID)
                            civillianTax *= alienCivilianTaxRate;

                        colonyTaxRevenue += civillianTax;
                    }
                }

                //Here the AI, tax modifier and difficulty tax bonus are pulled. I precomputed this.
                colonyTaxRevenue += consolidatedBonuses;

                if (!game.GameDatabase.GetStarSystemInfo(colony.CachedStarSystemID).IsOpen)
                {
                    colonyTaxRevenue *= 0.89999997615814209;    //Floating point error? I assume this is *= 0.9
                }

                //pulled the homeworld retrieval out to alleviate a DB call.

                if (playerHomeworld != null && playerHomeworld.ColonyID == colony.ID)
                    colonyTaxRevenue *= game.AssetDatabase.HomeworldTaxBonusMod;

                taxRevenue += colonyTaxRevenue;
            }

            return taxRevenue;
        }
	}
}