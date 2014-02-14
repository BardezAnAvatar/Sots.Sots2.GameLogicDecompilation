using Kerberos.Sots.PlayerFramework;
using System;
namespace Kerberos.Sots.Steam
{
	internal static class SteamDLCHelpers
	{
		public static SteamDLCIdentifiers? GetDLCIdentifierFromFaction(Faction faction)
		{
			if (faction.Name == "human")
			{
				return new SteamDLCIdentifiers?(SteamDLCIdentifiers.SolForceImmersionPack);
			}
			if (faction.Name == "morrigi" || faction.Name == "liir_zuul")
			{
				return new SteamDLCIdentifiers?(SteamDLCIdentifiers.LiirAndMorrigiImmersionPack);
			}
			if (faction.Name == "hiver" || faction.Name == "tarkas")
			{
				return new SteamDLCIdentifiers?(SteamDLCIdentifiers.HiverAndTarkasImmersionPack);
			}
			if (faction.Name == "zuul")
			{
				return new SteamDLCIdentifiers?(SteamDLCIdentifiers.TheHordeImmersionPack);
			}
			return null;
		}
		public static void LogAvailableDLC(App app)
		{
			App.Log.Trace("DLC content available:", "steam");
			SteamDLCIdentifiers[] array = (SteamDLCIdentifiers[])Enum.GetValues(typeof(SteamDLCIdentifiers));
			for (int i = 0; i < array.Length; i++)
			{
				SteamDLCIdentifiers steamDLCIdentifiers = array[i];
				if (app.Steam.HasDLC((int)steamDLCIdentifiers))
				{
					App.Log.Trace(string.Format("   {0}", steamDLCIdentifiers.ToString()), "steam");
				}
			}
			App.Log.Trace("End.", "steam");
		}
	}
}
