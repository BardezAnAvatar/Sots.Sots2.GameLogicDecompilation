using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;

using Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.StarFleet
{
    /// <summary>Contains performance fixes for Kerberos.Sots.StarFleet.StarFleet</summary>
	public static class StarFleet
    {
        /// <summary>Lists all constructor ship section assets' file names</summary>
        /// <param name="game">Game Session to utilize</param>
        /// <returns>The collection of all constructor ship section assets' file names</returns>
        internal static IList<String> GetConstructionSectionAssetNames(GameSession game)
        {
            return game.AssetDatabase.ShipSections.Where(section => section != null && section.isConstructor).Select(section => section.FileName).ToList();
        }

        /// <summary>Lists all colonization ship section assets' file names</summary>
        /// <param name="game">Game Session to utilize</param>
        /// <returns>The collection of all colonization ship section assets' file names</returns>
        internal static IList<String> GetColonizationSectionAssetNames(GameSession game)
        {
            return game.AssetDatabase.ShipSections.Where(section => section != null && section.ColonizationSpace > 0).Select(section => section.FileName).ToList();
        }
	}
}