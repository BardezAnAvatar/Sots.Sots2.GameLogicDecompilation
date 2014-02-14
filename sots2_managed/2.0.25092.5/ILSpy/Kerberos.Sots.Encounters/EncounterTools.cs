using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Encounters
{
	internal static class EncounterTools
	{
		public static int AddOrGetEncounterDesignInfo(this GameDatabase db, IEnumerable<DesignInfo> designs, int playerID, string name, string factionName, params string[] sectionFilename)
		{
			for (int i = 0; i < sectionFilename.Count<string>(); i++)
			{
				sectionFilename[i] = string.Format("factions\\{0}\\sections\\{1}", factionName, sectionFilename[i]);
			}
			DesignInfo designInfo = designs.FirstOrDefault((DesignInfo x) => x.Name == name);
			if (designInfo == null)
			{
				designInfo = new DesignInfo(playerID, name, sectionFilename);
				if (string.IsNullOrEmpty(name))
				{
					designInfo.Name = DesignLab.GenerateDesignName(db.AssetDatabase, db, null, designInfo, DesignLab.NameGenerators.FactionRandom);
				}
				return db.InsertDesignByDesignInfo(designInfo);
			}
			return designInfo.ID;
		}
		public static List<StarSystemInfo> GetClosestStars(GameDatabase game, Vector3 origin)
		{
			return (
				from x in game.GetStarSystemInfos().ToList<StarSystemInfo>()
				orderby (x.Origin - origin).LengthSquared
				select x).ToList<StarSystemInfo>();
		}
		public static List<StarSystemInfo> GetClosestStars(GameDatabase game, StarSystemInfo systemInfo)
		{
			List<StarSystemInfo> list = (
				from x in game.GetStarSystemInfos().ToList<StarSystemInfo>()
				orderby (x.Origin - systemInfo.Origin).LengthSquared
				select x).ToList<StarSystemInfo>();
			list.RemoveAll((StarSystemInfo x) => x.ID == systemInfo.ID);
			return list;
		}
		public static List<StarSystemInfo> GetClosestStars(GameDatabase game, int SystemId)
		{
			return EncounterTools.GetClosestStars(game, game.GetStarSystemInfo(SystemId));
		}
		public static float DistanceBetween(StarSystemInfo a, StarSystemInfo b)
		{
			return (b.Origin - a.Origin).Length;
		}
		public static bool IsSystemInhabited(GameDatabase gamedb, int SystemId)
		{
			List<OrbitalObjectInfo> list = gamedb.GetStarSystemOrbitalObjectInfos(SystemId).ToList<OrbitalObjectInfo>();
			foreach (OrbitalObjectInfo current in list)
			{
				if (gamedb.GetColonyInfoForPlanet(current.ID) != null)
				{
					return true;
				}
			}
			return false;
		}
		public static List<KeyValuePair<StarSystemInfo, Vector3>> GetOutlyingStars(GameDatabase gamedb)
		{
			App.GetSafeRandom();
			float num = 10f;
			int num2 = 20;
			List<StarSystemInfo> list = gamedb.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<StarSystemInfo> list2 = (
				from x in list
				where EncounterTools.IsSystemInhabited(gamedb, x.ID)
				select x).ToList<StarSystemInfo>();
			List<StarSystemInfo> list3 = new List<StarSystemInfo>();
			int count = list.Count;
			foreach (StarSystemInfo si in list)
			{
				if (!list2.Contains(si))
				{
					if (num < list2.Min((StarSystemInfo x) => (x.Origin - si.Origin).Length))
					{
						list3.Add(si);
					}
				}
			}
			Dictionary<StarSystemInfo, Vector3> dictionary = new Dictionary<StarSystemInfo, Vector3>();
			foreach (StarSystemInfo si in list3)
			{
				list.Sort((StarSystemInfo x, StarSystemInfo y) => EncounterTools.DistanceBetween(x, si).CompareTo(EncounterTools.DistanceBetween(y, si)));
				Vector3 vector = new Vector3(0f);
				int num3 = 0;
				while (num3 < num2 && num3 < count)
				{
					if (si != list[num3])
					{
						vector += Vector3.Normalize(si.Origin - list[num3].Origin);
					}
					num3++;
				}
				dictionary.Add(si, vector);
			}
			return (
				from x in dictionary
				orderby -x.Value.Length
				select x).ToList<KeyValuePair<StarSystemInfo, Vector3>>();
		}
	}
}
