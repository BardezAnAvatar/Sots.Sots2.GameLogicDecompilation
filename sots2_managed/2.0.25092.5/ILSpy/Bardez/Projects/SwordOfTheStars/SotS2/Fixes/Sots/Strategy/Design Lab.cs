using System;
using System.Collections.Generic;
using System.Linq;

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.StarFleet;

using PerformanceStarFleet = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.StarFleet;
using PerformanceData = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Strategy
{
    /// <summary>This class contains various performance enhancements for Kerberos.Sots.Strategy.DesignLab</summary>
	public static class DesignLab
    {
        /// <summary>Retrieves the collection of designs for the player that meet the criteria for the specified mission type</summary>
        /// <param name="game">GameSession to utilize</param>
        /// <param name="missionType">Type of mission to use as criteria</param>
        /// <param name="playerId">Unique ID of the player to get designs for</param>
        /// <returns>The collection of the player's designs' IDs fitting the specified criteria</returns>
        internal static List<Int32> GetMissionRequiredDesigns(GameSession game, MissionType missionType, Int32 playerId)
        {
            List<Int32> designIDs = new List<Int32>();

            Func<GameSession, IList<String>> criteria = null;

            switch (missionType)
            {
                case MissionType.COLONIZATION:
                case MissionType.SUPPORT:
                    criteria = PerformanceStarFleet.StarFleet.GetColonizationSectionAssetNames;
                    break;

                case MissionType.CONSTRUCT_STN:
                    criteria = PerformanceStarFleet.StarFleet.GetConstructionSectionAssetNames;
                    break;
            }

            if (criteria != null)
            {
                IList<String> matchingAssets = criteria(game);
                designIDs = PerformanceData.GameDatabase.GetDesignsMatchingAssetsForPlayer(game.GameDatabase, playerId, matchingAssets);
            }

            return designIDs;
        }
	}
}