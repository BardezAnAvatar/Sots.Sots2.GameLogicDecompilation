using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class StationManagerDialog : Dialog
	{
		public const string OKButton = "okButton";
		private App App;
		private StarMapState _starmap;
		private StationInfo _selectedStation;
		private StationType _currentFilterMode;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private int _systemID;
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private static Dictionary<StationType, bool> StationViewFilter = new Dictionary<StationType, bool>
		{

			{
				StationType.CIVILIAN,
				true
			},

			{
				StationType.DEFENCE,
				true
			},

			{
				StationType.DIPLOMATIC,
				true
			},

			{
				StationType.GATE,
				true
			},

			{
				StationType.MINING,
				true
			},

			{
				StationType.NAVAL,
				true
			},

			{
				StationType.SCIENCE,
				true
			}
		};
		public StationManagerDialog(App game, StarMapState starmap, int systemid = 0, string template = "dialogStationManager") : base(game, template)
		{
			this._starmap = starmap;
			this.App = game;
			this._systemID = systemid;
		}
		public override void Initialize()
		{
			this.App.UI.UnlockUI();
			this.App.UI.AddItem("filterDropdown", "", 0, App.Localize("@UI_STATION_MANAGER_ALL_STATIONS"));
			this.App.UI.AddItem("filterDropdown", "", 1, App.Localize("@UI_STATION_MANAGER_NAVAL"));
			this.App.UI.AddItem("filterDropdown", "", 2, App.Localize("@UI_STATION_MANAGER_SCIENCE"));
			this.App.UI.AddItem("filterDropdown", "", 3, App.Localize("@UI_STATION_MANAGER_CIVILIAN"));
			this.App.UI.AddItem("filterDropdown", "", 4, App.Localize("@UI_STATION_MANAGER_DIPLOMATIC"));
			this.App.UI.AddItem("filterDropdown", "", 5, App.Localize("@UI_STATION_MANAGER_GATE"));
			this.App.UI.AddItem("filterDropdown", "", 6, App.Localize("@UI_STATION_MANAGER_MINING"));
			this.App.UI.SetSelection("filterDropdown", 0);
			this._currentFilterMode = StationType.INVALID_TYPE;
		}
		public static bool CanOpen(GameSession game, int targetSystemId)
		{
			return game.GameDatabase.GetStationInfosByPlayerID(game.LocalPlayer.ID).Count<StationInfo>() > 0;
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "okButton")
			{
				this._app.UI.CloseDialog(this, true);
			}
			if (msgType == "mouse_enter")
			{
				string[] array = panelName.Split(new char[]
				{
					'|'
				});
				int orbitalObjectID = int.Parse(array[0]);
				ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), array[1]);
				StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(orbitalObjectID);
				IEnumerable<StationModules.StationModule> source = 
					from x in StationModules.Modules
					where x.SMType == type
					select x;
				if (source.Count<StationModules.StationModule>() > 0)
				{
					string arg = stationInfo.DesignInfo.StationType.ToDisplayText(this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(stationInfo.PlayerID))).ToUpper();
					this.App.UI.SetPropertyString("moduleDescriptionText", "text", App.Localize(string.Format(source.ElementAt(0).Description, arg)));
				}
			}
			else
			{
                bool flag1 = msgType == "mouse_leave";
			}
			if (msgType == "list_sel_changed")
			{
				if (panelName == "station_list")
				{
					StationInfo stationInfo2 = this.App.GameDatabase.GetStationInfo(int.Parse(msgParams[0]));
					this.PopulateModulesList(stationInfo2);
				}
				else
				{
					if (panelName == "filterDropdown")
					{
						int currentFilterMode = int.Parse(msgParams[0]);
						this._currentFilterMode = (StationType)currentFilterMode;
						this.SyncStationList();
					}
				}
			}
			if (msgType == "button_clicked")
			{
				if (panelName == "upgradeButton")
				{
					OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._selectedStation.OrbitalObjectID);
					this._starmap.GetUpgradeMissionOverlay().StartSelect = orbitalObjectInfo.ID;
					this._app.UI.CloseDialog(this, true);
					this._starmap.ShowUpgradeMissionOverlay(orbitalObjectInfo.StarSystemID);
					return;
				}
				if (panelName.StartsWith("modque"))
				{
					string[] array2 = panelName.Split(new char[]
					{
						'|'
					});
					ModuleEnums.StationModuleType moduleID = (ModuleEnums.StationModuleType)int.Parse(array2[1]);
					StationInfo stationInfo3 = this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID);
					List<LogicalModuleMount> availableStationModuleMounts = this.App.Game.GetAvailableStationModuleMounts(stationInfo3);
					List<DesignModuleInfo> queuedModules = this.App.GameDatabase.GetQueuedStationModules(this._selectedStation.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
					(
						from x in availableStationModuleMounts
						where queuedModules.Any((DesignModuleInfo y) => y.MountNodeName == x.NodeName)
						select x).ToList<LogicalModuleMount>();
					DesignModuleInfo designModuleInfo = queuedModules.FirstOrDefault((DesignModuleInfo x) => x.StationModuleType == moduleID);
					if (designModuleInfo != null)
					{
						this.App.GameDatabase.RemoveQueuedStationModule(designModuleInfo.ID);
					}
					this.SyncModuleItems();
					this.SyncBuildQueue();
					return;
				}
				if (panelName == "filterDiplomatic")
				{
					StationManagerDialog.StationViewFilter[StationType.DIPLOMATIC] = !StationManagerDialog.StationViewFilter[StationType.DIPLOMATIC];
					this.SyncStationList();
					return;
				}
				if (panelName == "filterScience")
				{
					StationManagerDialog.StationViewFilter[StationType.SCIENCE] = !StationManagerDialog.StationViewFilter[StationType.SCIENCE];
					this.SyncStationList();
					return;
				}
				if (panelName == "filterCivilian")
				{
					StationManagerDialog.StationViewFilter[StationType.CIVILIAN] = !StationManagerDialog.StationViewFilter[StationType.CIVILIAN];
					this.SyncStationList();
					return;
				}
				if (panelName == "filterNaval")
				{
					StationManagerDialog.StationViewFilter[StationType.NAVAL] = !StationManagerDialog.StationViewFilter[StationType.NAVAL];
					this.SyncStationList();
					return;
				}
				if (panelName == "filterMining")
				{
					StationManagerDialog.StationViewFilter[StationType.MINING] = !StationManagerDialog.StationViewFilter[StationType.MINING];
					this.SyncStationList();
					return;
				}
				if (panelName == "filterSDS")
				{
					StationManagerDialog.StationViewFilter[StationType.DEFENCE] = !StationManagerDialog.StationViewFilter[StationType.DEFENCE];
					this.SyncStationList();
					return;
				}
				if (panelName == "filterGate")
				{
					StationManagerDialog.StationViewFilter[StationType.GATE] = !StationManagerDialog.StationViewFilter[StationType.GATE];
					this.SyncStationList();
					return;
				}
				if (panelName == "confirmOrderButton")
				{
					StationInfo stationInfo4 = this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID);
					StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
					StationModuleQueue.ConfirmStationQueuedItems(this.App.Game, stationInfo4, this._queuedItemMap);
					this.SyncModuleItems();
					this.SyncBuildQueue();
					return;
				}
				if (panelName.EndsWith("module_up"))
				{
					string[] array3 = panelName.Split(new char[]
					{
						'|'
					});
					int orbitalObjectID2 = int.Parse(array3[0]);
					ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), array3[1]);
					StationInfo stationInfo5 = this.App.GameDatabase.GetStationInfo(orbitalObjectID2);
					this.App.GameDatabase.GetModuleID(this.App.AssetDatabase.GetStationModuleAsset(type, this.App.Game.LocalPlayer.Faction.Name), this.App.Game.LocalPlayer.ID);
					List<LogicalModuleMount> availableStationModuleMounts2 = this.App.Game.GetAvailableStationModuleMounts(stationInfo5);
					List<DesignModuleInfo> source2 = this.App.GameDatabase.GetQueuedStationModules(this._selectedStation.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
					(
						from x in source2
						where x.StationModuleType == type
						select x).ToList<DesignModuleInfo>();
					int num = (
						from x in source2
						where AssetDatabase.StationModuleTypeToMountTypeMap[x.StationModuleType.Value] == AssetDatabase.StationModuleTypeToMountTypeMap[type]
						select x).Count<DesignModuleInfo>();
					int num2 = (
						from x in availableStationModuleMounts2
						where x.ModuleType == AssetDatabase.StationModuleTypeToMountTypeMap[type].ToString()
						select x).Count<LogicalModuleMount>();
					int num3 = (
						from x in this._queuedItemMap
						where AssetDatabase.StationModuleTypeToMountTypeMap[x.Key] == AssetDatabase.StationModuleTypeToMountTypeMap[type]
						select x).Sum((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Value);
					if (num3 < num2 - num)
					{
						Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap;
						ModuleEnums.StationModuleType type3;
						(queuedItemMap = this._queuedItemMap)[type3 = type] = queuedItemMap[type3] + 1;
						this.SyncModuleItems();
						return;
					}
				}
				else
				{
					if (panelName.EndsWith("module_down"))
					{
						string[] array4 = panelName.Split(new char[]
						{
							'|'
						});
						int.Parse(array4[0]);
						ModuleEnums.StationModuleType stationModuleType = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), array4[1]);
						if (this._queuedItemMap[stationModuleType] > 0)
						{
							Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap2;
							ModuleEnums.StationModuleType key;
							(queuedItemMap2 = this._queuedItemMap)[key = stationModuleType] = queuedItemMap2[key] - 1;
							this.SyncModuleItems();
							return;
						}
					}
					else
					{
						if (panelName.EndsWith("module_max"))
						{
							string[] array5 = panelName.Split(new char[]
							{
								'|'
							});
							int orbitalObjectID3 = int.Parse(array5[0]);
							ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), array5[1]);
							StationInfo stationInfo6 = this.App.GameDatabase.GetStationInfo(orbitalObjectID3);
							this.App.GameDatabase.GetModuleID(this.App.AssetDatabase.GetStationModuleAsset(type, this.App.Game.LocalPlayer.Faction.Name), this.App.Game.LocalPlayer.ID);
							List<LogicalModuleMount> availableStationModuleMounts3 = this.App.Game.GetAvailableStationModuleMounts(stationInfo6);
							List<DesignModuleInfo> source3 = this.App.GameDatabase.GetQueuedStationModules(this._selectedStation.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
							(
								from x in source3
								where x.StationModuleType == type
								select x).ToList<DesignModuleInfo>();
							int num4 = (
								from x in source3
								where AssetDatabase.StationModuleTypeToMountTypeMap[x.StationModuleType.Value] == AssetDatabase.StationModuleTypeToMountTypeMap[type]
								select x).Count<DesignModuleInfo>();
							int num5 = (
								from x in availableStationModuleMounts3
								where x.ModuleType == AssetDatabase.StationModuleTypeToMountTypeMap[type].ToString()
								select x).Count<LogicalModuleMount>();
							int num6 = (
								from x in this._queuedItemMap
								where AssetDatabase.StationModuleTypeToMountTypeMap[x.Key] == AssetDatabase.StationModuleTypeToMountTypeMap[type]
								select x).Sum((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Value);
							if (num6 < num5 - num4)
							{
								Dictionary<ModuleEnums.StationModuleType, int> queuedItemMap3;
								ModuleEnums.StationModuleType type2;
								(queuedItemMap3 = this._queuedItemMap)[type2 = type] = queuedItemMap3[type2] + (num5 - num4 - num6);
								this.SyncModuleItems();
								return;
							}
						}
						else
						{
							if (panelName == "autoUpgradeButton")
							{
								this.AutoFillModules();
							}
						}
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Terminate();
			}
			return null;
		}
		protected void SyncStationList()
		{
			this.App.UI.ClearItems("station_list");
			this.App.UI.ClearItems("stationModules");
			this.App.UI.ClearItems("moduleQue");
			this.App.UI.SetText("queueCost", "");
			this.App.UI.SetText("turnsToComplete", "");
			this.App.UI.SetPropertyString("moduleDescriptionText", "text", "");
			List<StationInfo> list;
			if (this._systemID != 0)
			{
				list = this.App.GameDatabase.GetStationForSystemAndPlayer(this._systemID, this.App.Game.LocalPlayer.ID).ToList<StationInfo>();
			}
			else
			{
				list = this.App.GameDatabase.GetStationInfosByPlayerID(this.App.Game.LocalPlayer.ID).ToList<StationInfo>();
			}
			List<StationInfo> list2 = new List<StationInfo>(list);
			foreach (StationInfo current in list2)
			{
				if (!StationManagerDialog.StationViewFilter[current.DesignInfo.StationType])
				{
					list.Remove(current);
				}
			}
			foreach (SystemWidget current2 in this._systemWidgets)
			{
				current2.Terminate();
			}
			this._systemWidgets.Clear();
			List<int> list3 = new List<int>();
			list.RemoveAll((StationInfo x) => this.App.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID) == null);
			list = (
				from x in list
				orderby this.App.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetOrbitalObjectInfo(x.OrbitalObjectID).StarSystemID).Name
				select x).ToList<StationInfo>();
			foreach (StationInfo current3 in list)
			{
				if (current3.DesignInfo.StationLevel > 0 && (current3.DesignInfo.StationType == this._currentFilterMode || this._currentFilterMode == StationType.INVALID_TYPE))
				{
					int starSystemID = this.App.GameDatabase.GetOrbitalObjectInfo(current3.OrbitalObjectID).StarSystemID;
					if (!list3.Contains(starSystemID))
					{
						StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(starSystemID);
						if (starSystemInfo == null || starSystemInfo.IsDeepSpace)
						{
							continue;
						}
						this.App.UI.AddItem("station_list", "", starSystemID + 999999, "", "systemTitleCard");
						string itemGlobalID = this.App.UI.GetItemGlobalID("station_list", "", starSystemID + 999999, "");
						list3.Add(starSystemID);
						this._systemWidgets.Add(new SystemWidget(this.App, itemGlobalID));
						this._systemWidgets.Last<SystemWidget>().Sync(starSystemID);
					}
					this.App.UI.AddItem("station_list", string.Empty, current3.OrbitalObjectID, string.Empty, "navalStation_DetailsCard");
					this._selectedStation = current3;
					this.SyncStationProgress();
					StationUI.SyncStationDetailsWidget(this.App.Game, this.App.UI.GetItemGlobalID("station_list", string.Empty, current3.OrbitalObjectID, string.Empty), current3.OrbitalObjectID, true);
				}
			}
		}
		protected override void OnUpdate()
		{
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Update();
			}
		}
		protected void SyncModuleItems()
		{
			ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == this._selectedStation.DesignInfo.DesignSections[0].FilePath);
			List<LogicalModuleMount> source = shipSectionAsset.Modules.ToList<LogicalModuleMount>();
			List<DesignModuleInfo> builtModules = this._selectedStation.DesignInfo.DesignSections[0].Modules;
			List<DesignModuleInfo> queuedModules = this.App.GameDatabase.GetQueuedStationModules(this._selectedStation.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
			StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID);
			Dictionary<ModuleEnums.StationModuleType, int> source2 = new Dictionary<ModuleEnums.StationModuleType, int>();
			this.App.Game.GetStationUpgradeProgress(stationInfo, out source2);
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < 70; i++)
			{
				ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)i;
				if (this._queuedItemMap.ContainsKey(type))
				{
					List<StationModules.StationModule> matchingModules = (
						from val in StationModules.Modules
						where val.SMType == type
						select val).ToList<StationModules.StationModule>();
					if (matchingModules.Count > 0)
					{
						List<LogicalModuleMount> list = (
							from x in source
							where x.ModuleType == matchingModules[0].SlotType
							select x).ToList<LogicalModuleMount>();
						if (list.Count > 0)
						{
							num += list.Count<LogicalModuleMount>();
							List<LogicalModuleMount> list2 = (
								from x in list
								where builtModules.Any((DesignModuleInfo y) => y.MountNodeName == x.NodeName)
								select x).ToList<LogicalModuleMount>();
							List<LogicalModuleMount> list3 = (
								from x in list
								where queuedModules.Any((DesignModuleInfo y) => y.MountNodeName == x.NodeName)
								select x).ToList<LogicalModuleMount>();
							List<DesignModuleInfo> list4 = (
								from x in builtModules
								where x.StationModuleType == type
								select x).ToList<DesignModuleInfo>();
							List<DesignModuleInfo> list5 = (
								from x in queuedModules
								where x.StationModuleType == type
								select x).ToList<DesignModuleInfo>();
							int num3 = (
								from x in this._queuedItemMap
								where AssetDatabase.StationModuleTypeToMountTypeMap[x.Key] == AssetDatabase.StationModuleTypeToMountTypeMap[type] && x.Key != type
								select x).Sum((KeyValuePair<ModuleEnums.StationModuleType, int> x) => x.Value);
							int count = list4.Count;
							num2 += count;
							int count2 = list5.Count;
							int num4 = list.Count - list2.Count - list3.Count - num3;
							if (count2 + count + num4 >= 0)
							{
								string text = count2.ToString();
								if (this._queuedItemMap[type] > 0)
								{
									text = text + "~0,255,0,255|+" + this._queuedItemMap[type].ToString() + "~";
									this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
									{
										"module" + ((ModuleEnums.StationModuleType)i).ToString(),
										"module_que_plus"
									}), "text", text);
									this.App.UI.SetVisible(this.App.UI.Path(new string[]
									{
										"module" + ((ModuleEnums.StationModuleType)i).ToString(),
										"module_que_plus"
									}), true);
									this.App.UI.SetVisible(this.App.UI.Path(new string[]
									{
										"module" + ((ModuleEnums.StationModuleType)i).ToString(),
										"module_que"
									}), false);
								}
								else
								{
									this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
									{
										"module" + ((ModuleEnums.StationModuleType)i).ToString(),
										"module_que"
									}), "text", text);
									this.App.UI.SetVisible(this.App.UI.Path(new string[]
									{
										"module" + ((ModuleEnums.StationModuleType)i).ToString(),
										"module_que_plus"
									}), false);
									this.App.UI.SetVisible(this.App.UI.Path(new string[]
									{
										"module" + ((ModuleEnums.StationModuleType)i).ToString(),
										"module_que"
									}), true);
								}
								this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
								{
									"module" + ((ModuleEnums.StationModuleType)i).ToString(),
									"module_built"
								}), "text", string.Format("{0}/{1}", count, count2 + count + num4));
								string propertyValue = "";
								if (count2 + count + num4 > 0)
								{
									List<KeyValuePair<ModuleEnums.StationModuleType, int>> source3 = (
										from x in source2
										where x.Key.ToString() == AssetDatabase.StationModuleTypeToMountTypeMap[type].ToString()
										select x).ToList<KeyValuePair<ModuleEnums.StationModuleType, int>>();
									if (source3.Count<KeyValuePair<ModuleEnums.StationModuleType, int>>() > 0)
									{
										propertyValue = string.Format("{0} req.", source3.ElementAt(0).Value);
									}
								}
								this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
								{
									"module" + ((ModuleEnums.StationModuleType)i).ToString(),
									"module_req"
								}), "text", propertyValue);
							}
						}
					}
				}
			}
			this.SyncBuildQueue();
		}
		private void AutoFillModules()
		{
			StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
			StationModuleQueue.AutoFillModules(this.App.Game, this._selectedStation, this._queuedItemMap);
			if (this._queuedItemMap.Count > 0)
			{
				this.SyncModuleItems();
			}
		}
		protected void SyncStationProgress()
		{
			StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID);
			string itemGlobalID = this.App.UI.GetItemGlobalID("station_list", "", stationInfo.OrbitalObjectID, "");
            bool value = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, this.App.GameDatabase.GetOrbitalObjectInfo(this._selectedStation.OrbitalObjectID).StarSystemID, MissionType.CONSTRUCT_STN, true).Any<FleetInfo>();
			this.App.UI.SetEnabled(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"upgradeButton"
			}), value);
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"upgradeIndicator"
			}), false);
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"upgradeButton"
			}), false);
			this.App.UI.SetPropertyBool(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"upgradeButton"
			}), "lockout_button", true);
			if (this.App.Game.StationIsUpgrading(this._selectedStation))
			{
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"upgradeIndicator"
				}), true);
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"upgradeButton"
				}), false);
				return;
			}
			if (this.App.Game.StationIsUpgradable(this._selectedStation))
			{
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"upgradeButton"
				}), true);
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"upgradingIndicator"
				}), false);
			}
		}
		protected void SyncBuildQueue()
		{
			this.App.UI.ClearItems("moduleQue");
			StationInfo si = this.App.GameDatabase.GetStationInfo(this._selectedStation.OrbitalObjectID);
			List<DesignModuleInfo> list = this.App.GameDatabase.GetQueuedStationModules(si.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
			int num = 0;
			foreach (DesignModuleInfo module in list)
			{
				this.App.UI.AddItem("moduleQue", "", module.ID, "");
				string itemGlobalID = this.App.UI.GetItemGlobalID("moduleQue", "", module.ID, "");
				StationModules.StationModule stationModule = (
					from x in StationModules.Modules
					where x.SMType == module.StationModuleType
					select x).First<StationModules.StationModule>();
				StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
				LogicalModule logicalModule = this.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == this.App.AssetDatabase.GetStationModuleAsset(module.StationModuleType.Value, StationModuleQueue.GetModuleFactionDefault(module.StationModuleType.Value, this.App.Game.GetPlayerObject(si.PlayerID).Faction)));
				num += logicalModule.SavingsCost;
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"moduleName"
				}), "text", stationModule.Name + " - $" + logicalModule.SavingsCost.ToString("N0"));
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"deleteButton"
				}), true);
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"deleteButton"
				}), "id", "modque|" + ((int)module.StationModuleType.Value).ToString());
				this.App.UI.SetPropertyColor(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"moduleName"
				}), "color", new Vector3(255f, 255f, 255f));
			}
			int num2 = 999000;
			foreach (KeyValuePair<ModuleEnums.StationModuleType, int> thing in 
				from x in this._queuedItemMap.ToList<KeyValuePair<ModuleEnums.StationModuleType, int>>()
				where x.Value > 0
				select x)
			{
				int num3 = 0;
				while (true)
				{
					int arg_503_0 = num3;
					KeyValuePair<ModuleEnums.StationModuleType, int> thing4 = thing;
					if (arg_503_0 >= thing4.Value)
					{
						break;
					}
					this.App.UI.AddItem("moduleQue", "", num2, "");
					string itemGlobalID2 = this.App.UI.GetItemGlobalID("moduleQue", "", num2, "");
					StationModules.StationModule stationModule2 = StationModules.Modules.Where(delegate(StationModules.StationModule x)
					{
						ModuleEnums.StationModuleType arg_14_0 = x.SMType;
						KeyValuePair<ModuleEnums.StationModuleType, int> thing2 = thing;
						return arg_14_0 == thing2.Key;
					}).First<StationModules.StationModule>();
					StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
					LogicalModule logicalModule2 = this.App.AssetDatabase.Modules.First(delegate(LogicalModule x)
					{
						string arg_70_0 = x.ModulePath;
						AssetDatabase arg_6B_0 = this.App.AssetDatabase;
						KeyValuePair<ModuleEnums.StationModuleType, int> thing2 = thing;
						ModuleEnums.StationModuleType arg_6B_1 = thing2.Key;
						KeyValuePair<ModuleEnums.StationModuleType, int> thing3 = thing;
						return arg_70_0 == arg_6B_0.GetStationModuleAsset(arg_6B_1, StationModuleQueue.GetModuleFactionDefault(thing3.Key, this.App.Game.GetPlayerObject(si.PlayerID).Faction));
					});
					num += logicalModule2.SavingsCost;
					this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
					{
						itemGlobalID2,
						"moduleName"
					}), "text", stationModule2.Name + " - $" + logicalModule2.SavingsCost.ToString("N0"));
					this.App.UI.SetPropertyColor(this.App.UI.Path(new string[]
					{
						itemGlobalID2,
						"moduleName"
					}), "color", new Vector3(255f, 200f, 50f));
					this.App.UI.SetVisible(this.App.UI.Path(new string[]
					{
						itemGlobalID2,
						"deleteButton"
					}), false);
					num2++;
					num3++;
				}
			}
			this.App.UI.SetText("queueCost", "$" + num.ToString("N0"));
			this.App.UI.SetText("turnsToComplete", list.Count.ToString() + " " + App.Localize("@UI_GENERAL_TURNS"));
		}
		protected void PopulateModulesList(StationInfo station)
		{
			this.App.UI.SetPropertyString("moduleDescriptionText", "text", "");
			this._selectedStation = station;
			StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
			List<LogicalModuleMount> stationModuleMounts = this.App.Game.GetStationModuleMounts(station);
			this.App.UI.ClearItems("stationModules");
			StationModuleQueue.UpdateStationMapsForFaction(this.App.LocalPlayer.Faction.Name);
			StationModuleQueue.InitializeQueuedItemMap(this.App.Game, station, this._queuedItemMap);
			int num = 0;
			foreach (StationModules.StationModule current in StationModuleQueue.EnumerateUniqueStationModules(this.App.Game, station))
			{
				this.App.UI.AddItem("stationModules", "", num, "");
				string itemGlobalID = this.App.UI.GetItemGlobalID("stationModules", "", num, "");
				this.App.UI.SetPropertyString(itemGlobalID, "id", "module" + current.SMType);
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"huverbuttin"
				}), "id", station.OrbitalObjectID.ToString() + "|" + current.SMType);
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"module_label"
				}), "text", current.Name);
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"module_up"
				}), true);
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"module_down"
				}), true);
				this.App.UI.SetVisible(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"module_max"
				}), true);
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"module_up"
				}), "id", station.OrbitalObjectID.ToString() + "|" + current.SMType.ToString() + "|module_up");
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"module_down"
				}), "id", station.OrbitalObjectID.ToString() + "|" + current.SMType.ToString() + "|module_down");
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"module_max"
				}), "id", station.OrbitalObjectID.ToString() + "|" + current.SMType.ToString() + "|module_max");
				num++;
			}
			if (stationModuleMounts.FirstOrDefault((LogicalModuleMount x) => x.ModuleType == "AlienHabitation") != null)
			{
				if ((
					from x in this._queuedItemMap
					where AssetDatabase.StationModuleTypeToMountTypeMap[x.Key].ToString() == "AlienHabitation"
					select x).Count<KeyValuePair<ModuleEnums.StationModuleType, int>>() == 0)
				{
					this.AddBlankModule(num++, station, ModuleEnums.StationModuleType.AlienHabitation);
				}
			}
			if (stationModuleMounts.FirstOrDefault((LogicalModuleMount x) => x.ModuleType == "LargeAlienHabitation") != null)
			{
				if ((
					from x in this._queuedItemMap
					where AssetDatabase.StationModuleTypeToMountTypeMap[x.Key].ToString() == "LargeAlienHabitation"
					select x).Count<KeyValuePair<ModuleEnums.StationModuleType, int>>() == 0)
				{
					this.AddBlankModule(num++, station, ModuleEnums.StationModuleType.LargeAlienHabitation);
				}
			}
			this.SyncModuleItems();
			this.SyncBuildQueue();
		}
		public void AddBlankModule(int cur, StationInfo station, ModuleEnums.StationModuleType type)
		{
			this.App.UI.AddItem("stationModules", "", cur, "");
			string itemGlobalID = this.App.UI.GetItemGlobalID("stationModules", "", cur, "");
			this.App.UI.SetPropertyString(itemGlobalID, "id", "nullfun");
			this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"huverbuttin"
			}), "id", station.OrbitalObjectID.ToString() + "|" + type);
			if (type == ModuleEnums.StationModuleType.AlienHabitation)
			{
				this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"module_label"
				}), "text", App.Localize("@UI_STATIONDETAILS_ALIENHAB"));
			}
			else
			{
				if (type == ModuleEnums.StationModuleType.LargeAlienHabitation)
				{
					this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
					{
						itemGlobalID,
						"module_label"
					}), "text", App.Localize("@UI_STATIONDETAILS_LGALIENHAB"));
				}
			}
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"module_que_plus"
			}), false);
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"module_que"
			}), false);
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"module_up"
			}), false);
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"module_down"
			}), false);
			this.App.UI.SetVisible(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"module_max"
			}), false);
			this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"module_built"
			}), "text", "");
			Dictionary<ModuleEnums.StationModuleType, int> source = new Dictionary<ModuleEnums.StationModuleType, int>();
			this.App.Game.GetStationUpgradeProgress(station, out source);
			string propertyValue = "";
			List<KeyValuePair<ModuleEnums.StationModuleType, int>> source2 = (
				from x in source
				where x.Key.ToString() == type.ToString()
				select x).ToList<KeyValuePair<ModuleEnums.StationModuleType, int>>();
			if (source2.Count<KeyValuePair<ModuleEnums.StationModuleType, int>>() > 0)
			{
				propertyValue = string.Format("{0} req.", source2.ElementAt(0).Value);
			}
			this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
			{
				itemGlobalID,
				"module_req"
			}), "text", propertyValue);
		}
	}
}
