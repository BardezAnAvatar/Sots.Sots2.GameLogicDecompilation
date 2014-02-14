using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class PlanetManagerDialog : Dialog
	{
		private enum PlanetFilterMode
		{
			AllPlanets,
			SurveyedPlanets,
			OwnedPlanets,
			EnemyPlanets
		}
		private enum PlanetOrderMode
		{
			SystemOrder,
			PlanetHazard,
			PlanetSize,
			PlanetResources,
			PlanetBioSphere,
			PlanetDevCost,
			PlanetInfra
		}
		public class sortPlanetHazard : IComparer<PlanetInfo>
		{
			private App _App;
			public sortPlanetHazard(App _app)
			{
				this._App = _app;
			}
			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float planetHazardRating = this._App.GameDatabase.GetPlanetHazardRating(this._App.LocalPlayer.ID, a.ID, false);
				float planetHazardRating2 = this._App.GameDatabase.GetPlanetHazardRating(this._App.LocalPlayer.ID, b.ID, false);
				if (planetHazardRating > planetHazardRating2)
				{
					return 1;
				}
				if (planetHazardRating < planetHazardRating2)
				{
					return -1;
				}
				return 0;
			}
		}
		public class sortPlanetDevCost : IComparer<PlanetInfo>
		{
			private App _App;
			public sortPlanetDevCost(App _app)
			{
				this._App = _app;
			}
			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				double num = Colony.EstimateColonyDevelopmentCost(this._App.Game, a.ID, this._App.LocalPlayer.ID);
				double num2 = Colony.EstimateColonyDevelopmentCost(this._App.Game, b.ID, this._App.LocalPlayer.ID);
				if (num > num2)
				{
					return 1;
				}
				if (num < num2)
				{
					return -1;
				}
				return 0;
			}
		}
		public class sortPlanetSize : IComparer<PlanetInfo>
		{
			private App _App;
			public sortPlanetSize(App _app)
			{
				this._App = _app;
			}
			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float size = a.Size;
				float size2 = b.Size;
				if (size < size2)
				{
					return 1;
				}
				if (size > size2)
				{
					return -1;
				}
				return 0;
			}
		}
		public class sortPlanetResources : IComparer<PlanetInfo>
		{
			private App _App;
			public sortPlanetResources(App _app)
			{
				this._App = _app;
			}
			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float num = (float)a.Resources;
				float num2 = (float)b.Resources;
				if (num < num2)
				{
					return 1;
				}
				if (num > num2)
				{
					return -1;
				}
				return 0;
			}
		}
		public class sortPlanetBiosphere : IComparer<PlanetInfo>
		{
			private App _App;
			public sortPlanetBiosphere(App _app)
			{
				this._App = _app;
			}
			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float num = (float)a.Biosphere;
				float num2 = (float)b.Biosphere;
				if (num < num2)
				{
					return 1;
				}
				if (num > num2)
				{
					return -1;
				}
				return 0;
			}
		}
		public class sortPlanetInfra : IComparer<PlanetInfo>
		{
			private App _App;
			public sortPlanetInfra(App _app)
			{
				this._App = _app;
			}
			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float infrastructure = a.Infrastructure;
				float infrastructure2 = b.Infrastructure;
				if (infrastructure < infrastructure2)
				{
					return 1;
				}
				if (infrastructure > infrastructure2)
				{
					return -1;
				}
				return 0;
			}
		}
		private static readonly string UIExitButton = "okButton";
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private List<PlanetWidget> _planetWidgets;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private App App;
		private StarSystemInfo _selectedSystem;
		private PlanetManagerDialog.PlanetFilterMode _currentFilterMode;
		private PlanetManagerDialog.PlanetOrderMode _currentOrderMode;
		public PlanetManagerDialog(App game, string template = "dialogPlanetManager") : base(game, template)
		{
			this.App = game;
		}
		public override void Initialize()
		{
			this.App.UI.UnlockUI();
			this.App.UI.AddItem("filterDropdown", "", 0, App.Localize("@UI_PLANET_MANAGER_ALL_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 1, App.Localize("@UI_PLANET_MANAGER_SURVEYED_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 2, App.Localize("@UI_PLANET_MANAGER_OWNED_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 3, App.Localize("@UI_PLANET_MANAGER_ENEMY_PLANETS"));
			this.App.UI.SetSelection("filterDropdown", 0);
			this._currentFilterMode = PlanetManagerDialog.PlanetFilterMode.AllPlanets;
			this.App.UI.AddItem("orderDropdown", "", 0, App.Localize("@UI_PLANET_MANAGER_ORDERBY_POS"));
			this.App.UI.AddItem("orderDropdown", "", 1, App.Localize("@UI_PLANET_MANAGER_ORDERBY_HAZARD"));
			this.App.UI.AddItem("orderDropdown", "", 2, App.Localize("@UI_PLANET_MANAGER_ORDERBY_SIZE"));
			this.App.UI.AddItem("orderDropdown", "", 3, App.Localize("@UI_PLANET_MANAGER_ORDERBY_RESOURCES"));
			this.App.UI.AddItem("orderDropdown", "", 4, App.Localize("@UI_PLANET_MANAGER_ORDERBY_BIOSPHERE"));
			this.App.UI.AddItem("orderDropdown", "", 5, App.Localize("@UI_PLANET_MANAGER_ORDERBY_DEVCOST"));
			this.App.UI.AddItem("orderDropdown", "", 6, App.Localize("@UI_PLANET_MANAGER_ORDERBY_INFRA"));
			this.App.UI.SetSelection("orderDropdown", 0);
			this._currentOrderMode = PlanetManagerDialog.PlanetOrderMode.SystemOrder;
			this.App.UI.SetListCleanClear("system_list", true);
			EmpireBarUI.SyncTitleFrame(this.App);
			this._planetWidgets = new List<PlanetWidget>();
			this.SyncPlanetList();
		}
		protected override void OnUpdate()
		{
			foreach (PlanetWidget current in this._planetWidgets)
			{
				current.Update();
			}
			foreach (SystemWidget current2 in this._systemWidgets)
			{
				current2.Update();
			}
		}
		protected void SetSyncedSystem(StarSystemInfo system)
		{
			this._selectedSystem = system;
			if (this._app.CurrentState.Name == "StarMapState")
			{
				StarMapState starMapState = (StarMapState)this._app.CurrentState;
				starMapState.StarMap.SetFocus(starMapState.StarMap.Systems.Reverse[system.ID]);
				starMapState.StarMap.Select(starMapState.StarMap.Systems.Reverse[system.ID]);
			}
			this.App.UI.ClearItems("system_list");
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Terminate();
			}
			this._systemWidgets.Clear();
			foreach (PlanetWidget current2 in this._planetWidgets)
			{
				current2.Terminate();
			}
			this._planetWidgets.Clear();
			this.App.UI.ClearItems("system_list");
			List<PlanetInfo> list = this.FilteredPlanetList(system);
			this.App.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			string itemGlobalID = this.App.UI.GetItemGlobalID("system_list", "", system.ID, "");
			this._systemWidgets.Add(new SystemWidget(this.App, itemGlobalID));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo current3 in list)
			{
				if (this.App.AssetDatabase.IsPotentialyHabitable(current3.Type))
				{
					this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "planetDetailsM_Card");
					string itemGlobalID2 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID2));
					this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, true);
					this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
					{
						itemGlobalID2,
						"MoraleRow"
					}), "id", "MoraleRow|" + current3.ID);
				}
				else
				{
					if (this.App.AssetDatabase.IsGasGiant(current3.Type))
					{
						this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "gasgiantDetailsM_Card");
						string itemGlobalID3 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
						this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID3));
						this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
						this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
						{
							itemGlobalID3,
							"MoraleRow"
						}), "id", "MoraleRow|" + current3.ID);
					}
					else
					{
						if (this.App.AssetDatabase.IsMoon(current3.Type))
						{
							this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "moonDetailsM_Card");
							string itemGlobalID4 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
							this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID4));
							this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
							this.App.UI.SetPropertyString(this.App.UI.Path(new string[]
							{
								itemGlobalID4,
								"MoraleRow"
							}), "id", "MoraleRow|" + current3.ID);
						}
					}
				}
			}
		}
		private List<PlanetInfo> FilteredPlanetList(StarSystemInfo system)
		{
			List<PlanetInfo> list = this.App.GameDatabase.GetStarSystemPlanetInfos(system.ID).ToList<PlanetInfo>();
			List<PlanetInfo> list2 = new List<PlanetInfo>();
			foreach (PlanetInfo current in list)
			{
				if (this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, system.ID))
				{
					if (this._currentFilterMode == PlanetManagerDialog.PlanetFilterMode.AllPlanets)
					{
						list2.Add(current);
					}
					else
					{
						if (this._currentFilterMode == PlanetManagerDialog.PlanetFilterMode.SurveyedPlanets)
						{
							if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID) == null)
							{
								list2.Add(current);
							}
						}
						else
						{
							if (this._currentFilterMode == PlanetManagerDialog.PlanetFilterMode.OwnedPlanets)
							{
								AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID);
								if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
								{
									list2.Add(current);
								}
							}
							else
							{
								if (this._currentFilterMode == PlanetManagerDialog.PlanetFilterMode.EnemyPlanets)
								{
									AIColonyIntel colonyIntelForPlanet2 = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID);
									if (colonyIntelForPlanet2 != null && colonyIntelForPlanet2.OwningPlayerID != this.App.LocalPlayer.ID)
									{
										list2.Add(current);
									}
								}
							}
						}
					}
				}
			}
			if (list2.Any<PlanetInfo>())
			{
				if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetHazard)
				{
					PlanetManagerDialog.sortPlanetHazard comparer = new PlanetManagerDialog.sortPlanetHazard(this._app);
					list2.Sort(comparer);
				}
				else
				{
					if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetDevCost)
					{
						PlanetManagerDialog.sortPlanetDevCost comparer2 = new PlanetManagerDialog.sortPlanetDevCost(this._app);
						list2.Sort(comparer2);
					}
					else
					{
						if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetSize)
						{
							PlanetManagerDialog.sortPlanetSize comparer3 = new PlanetManagerDialog.sortPlanetSize(this._app);
							list2.Sort(comparer3);
						}
						else
						{
							if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetResources)
							{
								PlanetManagerDialog.sortPlanetResources comparer4 = new PlanetManagerDialog.sortPlanetResources(this._app);
								list2.Sort(comparer4);
							}
							else
							{
								if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetBioSphere)
								{
									PlanetManagerDialog.sortPlanetBiosphere comparer5 = new PlanetManagerDialog.sortPlanetBiosphere(this._app);
									list2.Sort(comparer5);
								}
								else
								{
									if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetInfra)
									{
										PlanetManagerDialog.sortPlanetInfra comparer6 = new PlanetManagerDialog.sortPlanetInfra(this._app);
										list2.Sort(comparer6);
									}
								}
							}
						}
					}
				}
			}
			return list2;
		}
		protected void SyncPlanetList()
		{
			this.App.UI.ClearItems("sys_list_left");
			List<StarSystemInfo> list = this.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			list.Sort((StarSystemInfo x, StarSystemInfo y) => string.Compare(x.Name, y.Name));
			bool flag = true;
			foreach (StarSystemInfo current in list)
			{
				if (this.FilteredPlanetList(current).Count > 0)
				{
					this.App.UI.AddItem("sys_list_left", "", current.ID, current.Name);
					if (flag)
					{
						this.SetSyncedSystem(current);
						flag = false;
					}
				}
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "filterDropdown")
				{
					int currentFilterMode = int.Parse(msgParams[0]);
					this._currentFilterMode = (PlanetManagerDialog.PlanetFilterMode)currentFilterMode;
					this.SyncPlanetList();
				}
				else
				{
					if (panelName == "orderDropdown")
					{
						int currentOrderMode = int.Parse(msgParams[0]);
						this._currentOrderMode = (PlanetManagerDialog.PlanetOrderMode)currentOrderMode;
						if (this._selectedSystem != null)
						{
							this.SetSyncedSystem(this._selectedSystem);
						}
					}
					else
					{
						if (panelName == "sys_list_left")
						{
							int systemId = int.Parse(msgParams[0]);
							StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(systemId);
							if (starSystemInfo != null)
							{
								this.SetSyncedSystem(starSystemInfo);
							}
						}
					}
				}
			}
			if (msgType == "button_clicked")
			{
				if (panelName == PlanetManagerDialog.UIExitButton)
				{
					this.App.UI.CloseDialog(this, true);
				}
				else
				{
					if (panelName.StartsWith("btnColoninzePlanet"))
					{
						string[] array = panelName.Split(new char[]
						{
							'|'
						});
						int targetSystem = int.Parse(array[1]);
						int targetPlanet = int.Parse(array[2]);
						if (this.App.CurrentState.GetType() == typeof(StarMapState))
						{
							((StarMapState)this.App.CurrentState).ShowColonizePlanetOverlay(targetSystem, targetPlanet);
						}
						this.App.UI.CloseDialog(this, true);
					}
				}
			}
			if (msgType == "mouse_enter" && panelName.StartsWith("MoraleRow"))
			{
				string[] array2 = panelName.Split(new char[]
				{
					'|'
				});
				int orbitalObjectID = int.Parse(array2[0]);
				int x = int.Parse(msgParams[0]);
				int y = int.Parse(msgParams[1]);
				ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(orbitalObjectID);
				if (colonyInfoForPlanet != null && this.App.LocalPlayer.ID == colonyInfoForPlanet.PlayerID)
				{
					StarSystemUI.ShowMoraleEventToolTip(this.App.Game, colonyInfoForPlanet.ID, x, y);
				}
			}
		}
		public override string[] CloseDialog()
		{
			foreach (PlanetWidget current in this._planetWidgets)
			{
				current.Terminate();
			}
			foreach (SystemWidget current2 in this._systemWidgets)
			{
				current2.Terminate();
			}
			return null;
		}
	}
}
