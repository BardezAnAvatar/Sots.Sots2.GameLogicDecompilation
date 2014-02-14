using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	public class StationUI
	{
		public static Dictionary<string, ModuleEnums.StationModuleType> ModuleToStationModuleTypeMap = new Dictionary<string, ModuleEnums.StationModuleType>
		{

			{
				"moduleEWPLab",
				ModuleEnums.StationModuleType.EWPLab
			},

			{
				"moduleTRPLab",
				ModuleEnums.StationModuleType.TRPLab
			},

			{
				"moduleNRGLab",
				ModuleEnums.StationModuleType.NRGLab
			},

			{
				"moduleWARLab",
				ModuleEnums.StationModuleType.WARLab
			},

			{
				"moduleBALLab",
				ModuleEnums.StationModuleType.BALLab
			},

			{
				"moduleBIOLab",
				ModuleEnums.StationModuleType.BIOLab
			},

			{
				"moduleINDLab",
				ModuleEnums.StationModuleType.INDLab
			},

			{
				"moduleCCCLab",
				ModuleEnums.StationModuleType.CCCLab
			},

			{
				"moduleDRVLab",
				ModuleEnums.StationModuleType.DRVLab
			},

			{
				"modulePOLLab",
				ModuleEnums.StationModuleType.POLLab
			},

			{
				"modulePSILab",
				ModuleEnums.StationModuleType.PSILab
			},

			{
				"moduleENGLab",
				ModuleEnums.StationModuleType.ENGLab
			},

			{
				"moduleBRDLab",
				ModuleEnums.StationModuleType.BRDLab
			},

			{
				"moduleSLDLab",
				ModuleEnums.StationModuleType.SLDLab
			},

			{
				"moduleCYBLab",
				ModuleEnums.StationModuleType.CYBLab
			},

			{
				"moduleSensor",
				ModuleEnums.StationModuleType.Sensor
			},

			{
				"moduleCustoms",
				ModuleEnums.StationModuleType.Customs
			},

			{
				"moduleCombat",
				ModuleEnums.StationModuleType.Combat
			},

			{
				"moduleRepair",
				ModuleEnums.StationModuleType.Repair
			},

			{
				"moduleWarehouse",
				ModuleEnums.StationModuleType.Warehouse
			},

			{
				"moduleCommand",
				ModuleEnums.StationModuleType.Command
			},

			{
				"moduleDock",
				ModuleEnums.StationModuleType.Dock
			},

			{
				"moduleHumanHabitation",
				ModuleEnums.StationModuleType.HumanHabitation
			},

			{
				"moduleTarkasHabitation",
				ModuleEnums.StationModuleType.TarkasHabitation
			},

			{
				"moduleLiirHabitation",
				ModuleEnums.StationModuleType.LiirHabitation
			},

			{
				"moduleHiverHabitation",
				ModuleEnums.StationModuleType.HiverHabitation
			},

			{
				"moduleMorrigiHabitation",
				ModuleEnums.StationModuleType.MorrigiHabitation
			},

			{
				"moduleZuulHabitation",
				ModuleEnums.StationModuleType.ZuulHabitation
			},

			{
				"moduleLoaHabitation",
				ModuleEnums.StationModuleType.LoaHabitation
			},

			{
				"moduleHumanTrade",
				ModuleEnums.StationModuleType.HumanTradeModule
			},

			{
				"moduleTarkaTrade",
				ModuleEnums.StationModuleType.TarkasTradeModule
			},

			{
				"moduleLiirTrade",
				ModuleEnums.StationModuleType.LiirTradeModule
			},

			{
				"moduleHiverTrade",
				ModuleEnums.StationModuleType.HiverTradeModule
			},

			{
				"moduleMorrigiTrade",
				ModuleEnums.StationModuleType.MorrigiTradeModule
			},

			{
				"moduleZuulTrade",
				ModuleEnums.StationModuleType.ZuulTradeModule
			},

			{
				"moduleLoaTrade",
				ModuleEnums.StationModuleType.LoaTradeModule
			},

			{
				"moduleTerraform",
				ModuleEnums.StationModuleType.Terraform
			},

			{
				"moduleHumanLargeHabitation",
				ModuleEnums.StationModuleType.HumanLargeHabitation
			},

			{
				"moduleTarkasLargeHabitation",
				ModuleEnums.StationModuleType.TarkasLargeHabitation
			},

			{
				"moduleLiirLargeHabitation",
				ModuleEnums.StationModuleType.LiirLargeHabitation
			},

			{
				"moduleHiverLargeHabitation",
				ModuleEnums.StationModuleType.HiverLargeHabitation
			},

			{
				"moduleMorrigiLargeHabitation",
				ModuleEnums.StationModuleType.MorrigiLargeHabitation
			},

			{
				"moduleZuulLargeHabitation",
				ModuleEnums.StationModuleType.ZuulLargeHabitation
			},

			{
				"moduleLoaLargeHabitation",
				ModuleEnums.StationModuleType.LoaLargeHabitation
			},

			{
				"moduleBastion",
				ModuleEnums.StationModuleType.Bastion
			},

			{
				"moduleAmp",
				ModuleEnums.StationModuleType.Amp
			},

			{
				"moduleDefence",
				ModuleEnums.StationModuleType.Defence
			},

			{
				"moduleGateLab",
				ModuleEnums.StationModuleType.GateLab
			}
		};
		public static Dictionary<string, string> ModuleTypeMap = new Dictionary<string, string>
		{

			{
				"moduleHiverTrade",
				"Trade"
			},

			{
				"moduleTarkaTrade",
				"Trade"
			},

			{
				"moduleHumanTrade",
				"Trade"
			},

			{
				"moduleMorrigiTrade",
				"Trade"
			},

			{
				"moduleLiirTrade",
				"Trade"
			},

			{
				"moduleLoaTrade",
				"Trade"
			},

			{
				"moduleEWPLab",
				"Lab"
			},

			{
				"moduleTRPLab",
				"Lab"
			},

			{
				"moduleNRGLab",
				"Lab"
			},

			{
				"moduleWARLab",
				"Lab"
			},

			{
				"moduleBALLab",
				"Lab"
			},

			{
				"moduleBIOLab",
				"Lab"
			},

			{
				"moduleINDLab",
				"Lab"
			},

			{
				"moduleCCCLab",
				"Lab"
			},

			{
				"moduleDRVLab",
				"Lab"
			},

			{
				"modulePOLLab",
				"Lab"
			},

			{
				"modulePSILab",
				"Lab"
			},

			{
				"moduleENGLab",
				"Lab"
			},

			{
				"moduleBRDLab",
				"Lab"
			},

			{
				"moduleSLDLab",
				"Lab"
			},

			{
				"moduleCYBLab",
				"Lab"
			},

			{
				"moduleSensor",
				"Sensor"
			},

			{
				"moduleCustoms",
				"Customs"
			},

			{
				"moduleCombat",
				"Combat"
			},

			{
				"moduleRepair",
				"Repair"
			},

			{
				"moduleWarehouse",
				"Warehouse"
			},

			{
				"moduleCommand",
				"Command"
			},

			{
				"moduleDock",
				"Dock"
			},

			{
				"moduleHumanHabitation",
				"Habitation"
			},

			{
				"moduleTarkasHabitation",
				"Habitation"
			},

			{
				"moduleLiirHabitation",
				"Habitation"
			},

			{
				"moduleHiverHabitation",
				"Habitation"
			},

			{
				"moduleMorrigiHabitation",
				"Habitation"
			},

			{
				"moduleZuulHabitation",
				"Habitation"
			},

			{
				"moduleLoaHabitation",
				"Habitation"
			},

			{
				"moduleTerraform",
				"Terraform"
			},

			{
				"moduleHumanLargeHabitation",
				"LargeHabitation"
			},

			{
				"moduleTarkasLargeHabitation",
				"LargeHabitation"
			},

			{
				"moduleLiirLargeHabitation",
				"LargeHabitation"
			},

			{
				"moduleHiverLargeHabitation",
				"LargeHabitation"
			},

			{
				"moduleMorrigiLargeHabitation",
				"LargeHabitation"
			},

			{
				"moduleZuulLargeHabitation",
				"LargeHabitation"
			},

			{
				"moduleLoaLargeHabitation",
				"LargeHabitation"
			},

			{
				"moduleBastion",
				"Bastion"
			},

			{
				"moduleAmp",
				"Gate"
			},

			{
				"moduleDefence",
				"Defence"
			},

			{
				"moduleGateLab",
				"GateLab"
			}
		};
		private static readonly string[] UIDefenceStationModules = new string[]
		{
			"moduleDefence"
		};
		private static readonly string[] UIMiningStationModules = new string[]
		{
			"moduleMining"
		};
		private static readonly string[] UINavalStationModules = new string[]
		{
			"moduleSensor",
			"moduleWarehouse",
			"moduleRepair",
			"moduleCommand",
			"moduleDock",
			"moduleCombat"
		};
		private static readonly string[] UIScienceStationModules = new string[]
		{
			"moduleHumanHabitation",
			"moduleTarkasHabitation",
			"moduleLiirHabitation",
			"moduleHiverHabitation",
			"moduleMorrigiHabitation",
			"moduleZuulHabitation",
			"moduleLoaHabitation",
			"moduleSensor",
			"moduleDock",
			"moduleWarehouse",
			"moduleEWPLab",
			"moduleTRPLab",
			"moduleNRGLab",
			"moduleWARLab",
			"moduleBALLab",
			"moduleBIOLab",
			"moduleINDLab",
			"moduleCCCLab",
			"moduleDRVLab",
			"modulePOLLab",
			"modulePSILab",
			"moduleENGLab",
			"moduleBRDLab",
			"moduleSLDLab",
			"moduleCYBLab"
		};
		private static readonly string[] UIDiplomaticStationModules = new string[]
		{
			"moduleSensor",
			"moduleDock",
			"moduleCustoms",
			"moduleHumanHabitation",
			"moduleTarkasHabitation",
			"moduleLiirHabitation",
			"moduleHiverHabitation",
			"moduleMorrigiHabitation",
			"moduleZuulHabitation",
			"moduleLoaHabitation",
			"moduleHumanLargeHabitation",
			"moduleTarkasLargeHabitation",
			"moduleLiirLargeHabitation",
			"moduleHiverLargeHabitation",
			"moduleMorrigiLargeHabitation",
			"moduleZuulLargeHabitation",
			"moduleLoaLargeHabitation"
		};
		private static readonly string[] UICivilianStationModules = new string[]
		{
			"moduleTerraform",
			"moduleDock",
			"moduleWarehouse",
			"moduleSensor",
			"moduleHiverTrade",
			"moduleTarkaTrade",
			"moduleHumanTrade",
			"moduleMorrigiTrade",
			"moduleLiirTrade",
			"moduleLoaTrade",
			"moduleHumanHabitation",
			"moduleTarkasHabitation",
			"moduleLiirHabitation",
			"moduleHiverHabitation",
			"moduleMorrigiHabitation",
			"moduleZuulHabitation",
			"moduleLoaHabitation",
			"moduleHumanLargeHabitation",
			"moduleTarkasLargeHabitation",
			"moduleLiirLargeHabitation",
			"moduleHiverLargeHabitation",
			"moduleMorrigiLargeHabitation",
			"moduleZuulLargeHabitation",
			"moduleLoaLargeHabitation"
		};
		private static readonly string[] UIGateStationModules = new string[]
		{
			"moduleHiverHabitation",
			"moduleDock",
			"moduleBastion",
			"moduleSensor",
			"moduleAmp",
			"moduleDefence",
			"moduleGateLab"
		};
		public static Dictionary<StationType, string[]> StationModuleMap = new Dictionary<StationType, string[]>
		{

			{
				StationType.CIVILIAN,
				StationUI.UICivilianStationModules
			},

			{
				StationType.DEFENCE,
				StationUI.UIDefenceStationModules
			},

			{
				StationType.DIPLOMATIC,
				StationUI.UIDiplomaticStationModules
			},

			{
				StationType.GATE,
				StationUI.UIGateStationModules
			},

			{
				StationType.MINING,
				StationUI.UIMiningStationModules
			},

			{
				StationType.NAVAL,
				StationUI.UINavalStationModules
			},

			{
				StationType.SCIENCE,
				StationUI.UIScienceStationModules
			}
		};
		private static Dictionary<StationType, string> StationModulePanelMap = new Dictionary<StationType, string>
		{

			{
				StationType.CIVILIAN,
				"civilianModules"
			},

			{
				StationType.DEFENCE,
				""
			},

			{
				StationType.DIPLOMATIC,
				"diplomaticModules"
			},

			{
				StationType.GATE,
				"gateModules"
			},

			{
				StationType.MINING,
				""
			},

			{
				StationType.NAVAL,
				"navalModules"
			},

			{
				StationType.SCIENCE,
				"scienceModules"
			}
		};
		private static Dictionary<StationType, string> StationIconMap = new Dictionary<StationType, string>
		{

			{
				StationType.CIVILIAN,
				"stationicon_civilian"
			},

			{
				StationType.DEFENCE,
				"stationicon_SDB"
			},

			{
				StationType.DIPLOMATIC,
				"stationicon_diplomatic"
			},

			{
				StationType.GATE,
				"stationicon_gate"
			},

			{
				StationType.MINING,
				"stationicon_mining"
			},

			{
				StationType.NAVAL,
				"stationicon_naval"
			},

			{
				StationType.SCIENCE,
				"stationicon_science"
			}
		};
		private static string GetStationIcon(StationType type, bool zuul)
		{
			string result = StationUI.StationIconMap[type];
			if (zuul && type == StationType.CIVILIAN)
			{
				result = "stationicon_slave";
			}
			else
			{
				if (zuul && type == StationType.DIPLOMATIC)
				{
					result = "stationicon_tribute";
				}
			}
			return result;
		}
		private static void SyncModuleItemControl(GameSession game, string panelName, int modulesBuilt, int modulesQueued, int modulesAvailable, string itemId)
		{
			if (itemId != null)
			{
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"module_up"
				}), "id", string.Format("{0}|{1}|module_up", itemId, panelName.Split(new char[]
				{
					'.'
				}).Last<string>()));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"module_down"
				}), "id", string.Format("{0}|{1}|module_down", itemId, panelName.Split(new char[]
				{
					'.'
				}).Last<string>()));
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"module_que"
			}), "text", modulesQueued.ToString());
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"module_built"
			}), "text", string.Format("{0}/{1}", modulesBuilt, modulesQueued + modulesBuilt + modulesAvailable));
		}
		private static void SyncStationDetailsControl(GameSession game, string panelName, StationInfo station)
		{
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == station.DesignInfo.DesignSections[0].FilePath);
			int num = shipSectionAsset.Crew;
			float structure = station.DesignInfo.Structure;
			int num2 = GameSession.CalculateStationUpkeepCost(game.GameDatabase, game.AssetDatabase, station);
			float num3 = shipSectionAsset.TacticalSensorRange;
			float strategicSensorRange = shipSectionAsset.StrategicSensorRange;
			List<DesignModuleInfo> modules = station.DesignInfo.DesignSections[0].Modules;
			foreach (DesignModuleInfo dmi in modules)
			{
				LogicalModule logicalModule = game.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == game.GameDatabase.GetModuleAsset(dmi.ModuleID));
				num += logicalModule.Crew;
				num3 += logicalModule.SensorBonus;
			}
			string propertyValue = num.ToString();
			string propertyValue2 = strategicSensorRange.ToString();
			string propertyValue3 = num3.ToString();
			string propertyValue4 = num2.ToString();
			string propertyValue5 = structure.ToString();
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"popVal"
			}), "text", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"stratVal"
			}), "text", propertyValue2);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"tactVal"
			}), "text", propertyValue3);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"maintVal"
			}), "text", propertyValue4);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"structVal"
			}), "text", propertyValue5);
		}
        internal static void SyncStationModulesControl(GameSession game, string panelName, StationInfo station, string itemId = null)
        {
            Func<LogicalModuleMount, bool> predicate = null;
            Func<LogicalModuleMount, bool> func5 = null;
            foreach (KeyValuePair<StationType, string> pair in StationModulePanelMap)
            {
                string panelId = game.UI.Path(new string[] { panelName, pair.Value });
                game.UI.SetVisible(panelId, pair.Key == station.DesignInfo.StationType);
            }
            string str = game.UI.Path(new string[] { panelName, StationModulePanelMap[station.DesignInfo.StationType] });
            List<LogicalModuleMount> source = game.AssetDatabase.ShipSections.First<ShipSectionAsset>(x => (x.FileName == station.DesignInfo.DesignSections[0].FilePath)).Modules.ToList<LogicalModuleMount>();
            List<DesignModuleInfo> builtModules = station.DesignInfo.DesignSections[0].Modules;
            List<DesignModuleInfo> queuedModules = game.GameDatabase.GetQueuedStationModules(station.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
            Func<LogicalModuleMount, bool> func = null;
            Func<DesignModuleInfo, bool> func2 = null;
            Func<DesignModuleInfo, bool> func3 = null;
            foreach (string s in StationModuleMap[station.DesignInfo.StationType])
            {
                if (func == null)
                {
                    func = x => x.ModuleType == ModuleTypeMap[s];
                }
                List<LogicalModuleMount> list2 = source.Where<LogicalModuleMount>(func).ToList<LogicalModuleMount>();
                if (predicate == null)
                {
                    predicate = x => builtModules.Any<DesignModuleInfo>(y => y.MountNodeName == x.NodeName);
                }
                List<LogicalModuleMount> list3 = list2.Where<LogicalModuleMount>(predicate).ToList<LogicalModuleMount>();
                if (func5 == null)
                {
                    func5 = x => queuedModules.Any<DesignModuleInfo>(y => y.MountNodeName == x.NodeName);
                }
                List<LogicalModuleMount> list4 = list2.Where<LogicalModuleMount>(func5).ToList<LogicalModuleMount>();
                if (func2 == null)
                {
                    func2 = delegate(DesignModuleInfo x)
                    {
                        ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
                        ModuleEnums.StationModuleType type = ModuleToStationModuleTypeMap[s];
                        return (stationModuleType.GetValueOrDefault() == type) && stationModuleType.HasValue;
                    };
                }
                List<DesignModuleInfo> list5 = builtModules.Where<DesignModuleInfo>(func2).ToList<DesignModuleInfo>();
                if (func3 == null)
                {
                    func3 = delegate(DesignModuleInfo x)
                    {
                        ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
                        ModuleEnums.StationModuleType type = ModuleToStationModuleTypeMap[s];
                        return (stationModuleType.GetValueOrDefault() == type) && stationModuleType.HasValue;
                    };
                }
                List<DesignModuleInfo> list6 = queuedModules.Where<DesignModuleInfo>(func3).ToList<DesignModuleInfo>();
                int count = list5.Count;
                int modulesQueued = list6.Count;
                int modulesAvailable = (list2.Count - list3.Count) - list4.Count;
                SyncModuleItemControl(game, game.UI.Path(new string[] { str, s }), count, modulesQueued, modulesAvailable, itemId);
            }
        }
        internal static void SyncStationDetailsWidget(GameSession game, string panelName, int stationID, bool updateButtonIds)
		{
			StationInfo station = game.GameDatabase.GetStationInfo(stationID);
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(game.GameDatabase.GetOrbitalObjectInfo(station.OrbitalObjectID).StarSystemID);
			Dictionary<ModuleEnums.StationModuleType, int> dictionary;
			float stationUpgradeProgress = game.GetStationUpgradeProgress(station, out dictionary);
			string text = string.Format("{0}|station_upgrade", station.OrbitalObjectID);
			if (updateButtonIds)
			{
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"station_upgrade"
				}), "id", text);
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"station_system"
			}), "text", starSystemInfo.Name);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"station_name"
			}), "text", game.GameDatabase.GetOrbitalObjectInfo(station.OrbitalObjectID).Name);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"station_preview"
			}), "sprite", StationUI.GetStationIcon(station.DesignInfo.StationType, game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(station.PlayerID)) == "zuul"));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"station_level"
			}), "text", string.Format("{0} {1}", App.Localize("@UI_STATIONMANAGER_STAGE"), station.DesignInfo.StationLevel));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"stationLevel"
			}), "text", string.Format("{0}", station.DesignInfo.StationLevel));
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				text
			}), stationUpgradeProgress == 1f);
			game.UI.SetPropertyInt(game.UI.Path(new string[]
			{
				panelName,
				"station_progress"
			}), "value", (int)(stationUpgradeProgress * 100f));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"itemSubTitle"
			}), "text", station.DesignInfo.StationType.ToDisplayText(game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(station.PlayerID))) + " | " + station.DesignInfo.Name);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"stationPopulation"
			}), "text", station.DesignInfo.CrewRequired.ToString("N0"));
            int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(game, station.DesignInfo, station.ShipID);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"stationStructure"
			}), "text", healthAndHealthMax[0] + "/" + healthAndHealthMax[1]);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"stationUpkeep"
			}), "text", GameSession.CalculateStationUpkeepCost(game.GameDatabase, game.AssetDatabase, station).ToString("N0"));
			string str = station.GetBaseStratSensorRange().ToString();
			float stationAdditionalStratSensorRange = game.GameDatabase.GetStationAdditionalStratSensorRange(station);
			if (stationAdditionalStratSensorRange > 0f)
			{
				str += "(+";
				str += stationAdditionalStratSensorRange.ToString();
				str += ")";
			}
			string str2 = GameSession.GetStationBaseTacSensorRange(game, station).ToString("N0");
			float stationAdditionalTacSensorRange = GameSession.GetStationAdditionalTacSensorRange(game, station);
			if (stationAdditionalTacSensorRange > 0f)
			{
				str2 += "(+";
				str2 += stationAdditionalTacSensorRange.ToString("N0");
				str2 += ")";
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"stationStratSensorRange"
			}), "text", str + " ly");
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"stationTacSensorRange"
			}), "text", str2 + " km");
			StationUI.SyncStationDetailsControl(game, game.UI.Path(new string[]
			{
				panelName,
				"generalstats"
			}), station);
			if (updateButtonIds)
			{
				StationUI.SyncStationModulesControl(game, game.UI.Path(new string[]
				{
					panelName,
					"module_details"
				}), station, stationID.ToString());
			}
			else
			{
				StationUI.SyncStationModulesControl(game, game.UI.Path(new string[]
				{
					panelName,
					"module_details"
				}), station, null);
			}
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == station.DesignInfo.DesignSections[0].FilePath);
			shipSectionAsset.Modules.ToList<LogicalModuleMount>();
			List<DesignModuleInfo> arg_680_0 = station.DesignInfo.DesignSections[0].Modules;
			if (station.DesignInfo.StationLevel == 5)
			{
				game.UI.SetPropertyInt(game.UI.Path(new string[]
				{
					panelName,
					"upgradeProgress"
				}), "value", 100);
				game.UI.SetPropertyColorNormalized(game.UI.Path(new string[]
				{
					panelName,
					"upgradeProgress.overlay_idle.image"
				}), "color", 0.8f, 0.7f, 0f);
				return;
			}
			Dictionary<ModuleEnums.StationModuleType, int> dictionary2 = new Dictionary<ModuleEnums.StationModuleType, int>();
			float stationUpgradeProgress2 = game.GetStationUpgradeProgress(station, out dictionary2);
			game.UI.SetPropertyInt(game.UI.Path(new string[]
			{
				panelName,
				"upgradeProgress"
			}), "value", (int)(stationUpgradeProgress2 * 100f));
			game.UI.SetPropertyColorNormalized(game.UI.Path(new string[]
			{
				panelName,
				"upgradeProgress.overlay_idle.image"
			}), "color", 0f, 0.4f, 0.9f);
		}
	}
}
