using System;
using System.Resources;
using System.Text;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data
{
    /// <summary>This class exposes any new database queries</summary>
	public static class Queries
    {
        #region Fields
        /// <summary>Resource manager to retrieve strings from Queries.cs</summary>
        private static ResourceManager resourceManager = new ResourceManager("Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data.Queries", typeof(Queries).Assembly);

        /// <summary>Query to retrieve the entire collection of module psionics</summary>
        private static String getModulePsionics = @"SELECT [id], [design_module_id],  [psionic]
FROM [module_psionics]";

        /// <summary>Query to retrieve the entire collection of weapon banks</summary>
        private static String getWeaponBanks = @"SELECT [id], [weapon_id], [design_id], [bank_guid], [firing_mode], [filter_mode], [design_section_id]
FROM [design_section_weapon_banks]";

        /// <summary>Query to retrieve the entire collection of design modules</summary>
        private static String getDesignModules = @"SELECT [id], [design_section_id], [module_id], [weapon_id], [mount_node], [station_module_type], [design_id]
FROM [design_modules]";

        /// <summary>Query to retrieve the entire collection of design sections</summary>
        private static String getDesignSections = @"SELECT [ds].[id], [ds].[design_id], [a].[path]
FROM
    [design_sections] AS [ds]
    INNER JOIN [assets] as [a]
        ON [a].[id] = [ds].[asset_id]";

        /// <summary>Query to retrieve the entire collection of design sections' techs</summary>
        private static String getDesignSectionTechs = @"SELECT [id], [tech_id]
FROM [design_section_techs]";

        /// <summary>Query to retrieve the collection of design IDs that match a player and certain assets</summary>
        private static String getDesignsForPlayerMatchingAssets = @"SELECT [d].[id]
FROM
	[design_sections] AS [ds]
	INNER JOIN [designs] AS [d]
		ON [ds].[design_id] = [d].[id]
	INNER JOIN [assets] AS [a]
		ON [a].[id] = [ds].[asset_id]
WHERE
	[d].[player_id] = {0}
	AND [a].[path] IN ( {1} )";
        #endregion


        #region Properties
        /// <summary>Retrieves the query to retrieve the entire collection of module psionics</summary>
        public static String GetModulePsionics
        {
            get { return Queries.getDesignModules; }
        }

        /// <summary>Retrieves the query to retrieve the entire collection of weapon banks</summary>
        public static String GetWeaponBanks
        {
            get { return Queries.getWeaponBanks; }
        }

        /// <summary>Retrieves the query to retrieve the entire collection of design modules</summary>
        public static String GetDesignModules
        {
            get { return Queries.getDesignModules; }
        }

        /// <summary>Retrieves the query to retrieve the entire collection of design sections</summary>
        public static String GetDesignSections
        {
            get { return Queries.getDesignSections; }
        }

        /// <summary>Retrieves the query to retrieve the entire collection of design sections' techs</summary>
        public static String GetDesignSectionTechs
        {
            get { return Queries.getDesignSectionTechs; }
        }

        /// <summary>Retrieves the query to retrieve the collection of design IDs that match a player and certain assets</summary>
        public static String GetDesignsForPlayerMatchingAssets
        {
            get { return Queries.getDesignsForPlayerMatchingAssets; }
        }
        #endregion
    }
}