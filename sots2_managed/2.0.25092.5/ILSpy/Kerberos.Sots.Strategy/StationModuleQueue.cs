using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class StationModuleQueue
	{
		private class ModuleRequirements : Dictionary<ModuleEnums.StationModuleType, KeyValuePair<IEnumerable<ModuleEnums.StationModuleType>, int>>
		{
			public ModuleRequirements(GameSession game, StationInfo station, Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap)
			{
				Dictionary<ModuleEnums.StationModuleType, int> dictionary = new Dictionary<ModuleEnums.StationModuleType, int>();
				game.GetStationUpgradeProgress(station, out dictionary);
				string name = game.GetPlayerObject(station.PlayerID).Faction.Name;
				foreach (KeyValuePair<ModuleEnums.StationModuleType, int> current in dictionary)
				{
					base.Add(current.Key, new KeyValuePair<IEnumerable<ModuleEnums.StationModuleType>, int>(
						from x in AssetDatabase.ResolveSpecificStationModuleTypes(name, current.Key)
						where queuedItemMap.ContainsKey(x)
						select x, current.Value));
				}
			}
		}
		public static string GetModuleFactionDefault(ModuleEnums.StationModuleType type, Faction defaultFaction)
		{
			string text = AssetDatabase.GetModuleFactionName(type);
			if (text.Length == 0)
			{
				text = defaultFaction.Name;
			}
			return text;
		}
		internal static IEnumerable<StationModules.StationModule> EnumerateUniqueStationModules(GameSession game, StationInfo station)
		{
			HashSet<ModuleEnums.StationModuleType> hashSet = new HashSet<ModuleEnums.StationModuleType>();
			foreach (LogicalModuleMount mount in game.GetStationModuleMounts(station))
			{
				List<StationModules.StationModule> list = (
					from val in StationModules.Modules
					where val.SlotType == mount.ModuleType
					select val).ToList<StationModules.StationModule>();
				Player stationPlayer = game.GetPlayerObject(station.DesignInfo.PlayerID);
				foreach (StationModules.StationModule current in list)
				{
					if (!hashSet.Contains(current.SMType) && current.SMType != ModuleEnums.StationModuleType.AlienHabitation && current.SMType != ModuleEnums.StationModuleType.LargeAlienHabitation)
					{
						hashSet.Add(current.SMType);
						string faction = AssetDatabase.GetModuleFactionName(current.SMType);
						if (faction.Length > 0)
						{
							if (faction == "zuul" && current.SlotType.ToString().ToUpper().Contains("ALIEN"))
							{
								continue;
							}
							IEnumerable<PlayerInfo> source = game.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
							if ((
								from x in source
								where game.GameDatabase.GetFactionName(x.FactionID) == faction
								select x).Count<PlayerInfo>() <= 0)
							{
								continue;
							}
							bool flag = false;
							IEnumerable<PlayerInfo> enumerable = 
								from x in source
								where game.GameDatabase.GetFactionName(x.FactionID) == faction && x.ID != stationPlayer.ID
								select x;
							foreach (PlayerInfo current2 in enumerable)
							{
								flag = game.GameDatabase.GetDiplomacyInfo(stationPlayer.ID, current2.ID).isEncountered;
								if (flag)
								{
									break;
								}
							}
							if ((faction != stationPlayer.Faction.Name && !flag) || (faction != stationPlayer.Faction.Name && flag && current.SMType.ToString().ToUpper().Contains("FOREIGN")) || (faction == stationPlayer.Faction.Name && !flag && current.SMType.ToString().ToUpper().Contains("FOREIGN")))
							{
								continue;
							}
						}
						yield return current;
					}
				}
			}
			yield break;
		}
		internal static void InitializeQueuedItemMap(GameSession game, StationInfo station, Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap)
		{
			queuedItemMap.Clear();
			foreach (StationModules.StationModule current in StationModuleQueue.EnumerateUniqueStationModules(game, station))
			{
				queuedItemMap.Add(current.SMType, 0);
			}
		}
		internal static void AutoFillModules(GameSession game, StationInfo station, Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap)
		{
			if (queuedItemMap.Count > 0)
			{
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == station.DesignInfo.DesignSections[0].FilePath);
				shipSectionAsset.Modules.ToList<LogicalModuleMount>();
				List<DesignModuleInfo> arg_68_0 = station.DesignInfo.DesignSections[0].Modules;
				List<DesignModuleInfo> source = game.GameDatabase.GetQueuedStationModules(station.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
				StationInfo stationInfo = game.GameDatabase.GetStationInfo(station.OrbitalObjectID);
				StationModuleQueue.ModuleRequirements source2 = new StationModuleQueue.ModuleRequirements(game, stationInfo, queuedItemMap);
				Player playerObject = game.GetPlayerObject(stationInfo.PlayerID);
				foreach (KeyValuePair<ModuleEnums.StationModuleType, KeyValuePair<IEnumerable<ModuleEnums.StationModuleType>, int>> current in 
					from x in source2
					where x.Value.Value > 0
					select x)
				{
					KeyValuePair<IEnumerable<ModuleEnums.StationModuleType>, int> req = current.Value;
					int i = req.Value - source.Where((DesignModuleInfo x) => req.Key.Any((ModuleEnums.StationModuleType y) => y == x.StationModuleType.Value)).Count<DesignModuleInfo>() - queuedItemMap.Where((KeyValuePair<ModuleEnums.StationModuleType, int> x) => req.Key.Any((ModuleEnums.StationModuleType y) => y == x.Key)).Sum((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Value);
					while (i > 0)
					{
						int num = i;
						List<ModuleEnums.StationModuleType> list = req.Key.ToList<ModuleEnums.StationModuleType>();
						if (list.Count == 0)
						{
							break;
						}
						list.Shuffle<ModuleEnums.StationModuleType>();
						if (current.Key == ModuleEnums.StationModuleType.Lab)
						{
							int playerResearchingTechID = game.GameDatabase.GetPlayerResearchingTechID(playerObject.ID);
							if (playerResearchingTechID != 0)
							{
								string stringTechId = game.GameDatabase.GetTechFileID(playerResearchingTechID);
								Tech tech = game.AssetDatabase.MasterTechTree.Technologies.First((Tech x) => x.Id == stringTechId);
								ModuleEnums.StationModuleType item;
								if (list.ExistsFirst((ModuleEnums.StationModuleType x) => x.ToString().Contains(tech.Family), out item))
								{
									list.Remove(item);
									list.Insert(0, item);
								}
							}
						}
						foreach (ModuleEnums.StationModuleType moduleType in list)
						{
							int num2 = req.Value - source.Where((DesignModuleInfo x) => moduleType == x.StationModuleType.Value).Count<DesignModuleInfo>() - queuedItemMap.Where((KeyValuePair<ModuleEnums.StationModuleType, int> x) => moduleType == x.Key).Sum((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Value);
							if (num2 > 0)
							{
								ModuleEnums.StationModuleType moduleType2;
								queuedItemMap[moduleType2 = moduleType] = queuedItemMap[moduleType2] + 1;
								i--;
								if (i <= 0)
								{
									break;
								}
							}
						}
						if (i >= num)
						{
							break;
						}
					}
				}
			}
		}
        internal static void ConfirmStationQueuedItems(GameSession game, StationInfo station, Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap)
        {
            List<LogicalModuleMount> availableStationModuleMounts = game.GetAvailableStationModuleMounts(station);
            using (List<KeyValuePair<ModuleEnums.StationModuleType, int>>.Enumerator enumerator = queuedItemMap.ToList<KeyValuePair<ModuleEnums.StationModuleType, int>>().GetEnumerator())
            {
                Func<LogicalModuleMount, bool> predicate = null;
                KeyValuePair<ModuleEnums.StationModuleType, int> thing;
                while (enumerator.MoveNext())
                {
                    thing = enumerator.Current;
                    int num = thing.Value;
                    while (num > 0)
                    {
                        List<DesignModuleInfo> source = game.GameDatabase.GetQueuedStationModules(station.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
                        List<DesignModuleInfo> modules = station.DesignInfo.DesignSections[0].Modules;
                        if (predicate == null)
                        {
                            predicate = x => x.ModuleType == AssetDatabase.StationModuleTypeToMountTypeMap[thing.Key].ToString();
                        }
                        List<LogicalModuleMount> list4 = availableStationModuleMounts.Where<LogicalModuleMount>(predicate).ToList<LogicalModuleMount>();
                        bool flag = false;
                        using (List<LogicalModuleMount>.Enumerator enumerator2 = list4.GetEnumerator())
                        {
                            Func<DesignModuleInfo, bool> func = null;
                            Func<DesignModuleInfo, bool> func2 = null;
                            LogicalModuleMount mount;
                            while (enumerator2.MoveNext())
                            {
                                mount = enumerator2.Current;
                                if (func == null)
                                {
                                    func = x => x.MountNodeName == mount.NodeName;
                                }
                                if (source.Where<DesignModuleInfo>(func).Count<DesignModuleInfo>() == 0)
                                {
                                    if (func2 == null)
                                    {
                                        func2 = x => x.MountNodeName == mount.NodeName;
                                    }
                                    if (modules.Where<DesignModuleInfo>(func2).Count<DesignModuleInfo>() == 0)
                                    {
                                        List<PlayerInfo> list5 = game.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>();
                                        Player playerObject = game.GetPlayerObject(station.PlayerID);
                                        string moduleFactionDefault = GetModuleFactionDefault(thing.Key, playerObject.Faction);
                                        int moduleID = game.GameDatabase.GetModuleID(game.AssetDatabase.GetStationModuleAsset(thing.Key, moduleFactionDefault), list5.First<PlayerInfo>(x => (game.GameDatabase.GetFactionName(x.FactionID) == moduleFactionDefault)).ID);
                                        int? weaponId = null;
                                        game.GameDatabase.InsertQueuedStationModule(station.DesignInfo.DesignSections[0].ID, moduleID, weaponId, mount.NodeName, thing.Key);
                                        num--;
                                        flag = true;
                                        goto Label_027D;
                                    }
                                }
                            }
                        }
                    Label_027D:
                        if (!flag)
                        {
                            break;
                        }
                    }
                    queuedItemMap[thing.Key] = 0;
                }
            }
        }
        internal static void UpdateStationMapsForFaction(string faction)
		{
			StationModules.StationModule[] modules = StationModules.Modules;
			for (int i = 0; i < modules.Length; i++)
			{
				StationModules.StationModule stationModule = modules[i];
				if (stationModule.SMType == ModuleEnums.StationModuleType.HumanHabitation)
				{
					stationModule.SlotType = ((faction == "human") ? "Habitation" : "AlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.HiverHabitation)
				{
					stationModule.SlotType = ((faction == "hiver") ? "Habitation" : "AlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.TarkasHabitation)
				{
					stationModule.SlotType = ((faction == "tarkas") ? "Habitation" : "AlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.MorrigiHabitation)
				{
					stationModule.SlotType = ((faction == "morrigi") ? "Habitation" : "AlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.LiirHabitation)
				{
					stationModule.SlotType = ((faction == "liir_zuul") ? "Habitation" : "AlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.ZuulHabitation)
				{
					stationModule.SlotType = ((faction == "zuul") ? "Habitation" : "AlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.LoaHabitation)
				{
					stationModule.SlotType = ((faction == "loa") ? "Habitation" : "AlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.HumanLargeHabitation)
				{
					stationModule.SlotType = ((faction == "human") ? "LargeHabitation" : "LargeAlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.HiverLargeHabitation)
				{
					stationModule.SlotType = ((faction == "hiver") ? "LargeHabitation" : "LargeAlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.TarkasLargeHabitation)
				{
					stationModule.SlotType = ((faction == "tarkas") ? "LargeHabitation" : "LargeAlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.MorrigiLargeHabitation)
				{
					stationModule.SlotType = ((faction == "morrigi") ? "LargeHabitation" : "LargeAlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.LiirLargeHabitation)
				{
					stationModule.SlotType = ((faction == "liir_zuul") ? "LargeHabitation" : "LargeAlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.ZuulLargeHabitation)
				{
					stationModule.SlotType = ((faction == "zuul") ? "LargeHabitation" : "LargeAlienHabitation");
				}
				if (stationModule.SMType == ModuleEnums.StationModuleType.LoaLargeHabitation)
				{
					stationModule.SlotType = ((faction == "loa") ? "LargeHabitation" : "LargeAlienHabitation");
				}
			}
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.HumanHabitation] = ((faction == "human") ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.HiverHabitation] = ((faction == "hiver") ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.TarkasHabitation] = ((faction == "tarkas") ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.MorrigiHabitation] = ((faction == "morrigi") ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.LiirHabitation] = ((faction == "liir_zuul") ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.ZuulHabitation] = ((faction == "zuul") ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.LoaHabitation] = ((faction == "loa") ? ModuleEnums.ModuleSlotTypes.Habitation : ModuleEnums.ModuleSlotTypes.AlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.HumanLargeHabitation] = ((faction == "human") ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.HiverLargeHabitation] = ((faction == "hiver") ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.TarkasLargeHabitation] = ((faction == "tarkas") ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.MorrigiLargeHabitation] = ((faction == "morrigi") ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.LiirLargeHabitation] = ((faction == "liir_zuul") ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.ZuulLargeHabitation] = ((faction == "zuul") ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
			AssetDatabase.StationModuleTypeToMountTypeMap[ModuleEnums.StationModuleType.LoaLargeHabitation] = ((faction == "loa") ? ModuleEnums.ModuleSlotTypes.LargeHabitation : ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
		}
	}
}
