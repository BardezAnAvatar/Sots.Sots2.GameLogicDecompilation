using System;
using System.Collections.Generic;

using Kerberos.Sots.Data;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Steam;
using Source = Kerberos.Sots.Strategy.InhabitedPlanet;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Strategy.InhabitedPanet
{
    /// <summary>This class contains code fixes to general SotS2 logic in Kerberos.Sots.Strategy.InhabitedPanet.Colony</summary>
	public class Colony
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
	}
}