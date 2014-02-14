using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class PopulationManagerDialog : Dialog
	{
		private static readonly string UIExitButton = "okButton";
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private List<PlanetWidget> _planetWidgets;
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private App App;
		private int initialsystemid;
		public PopulationManagerDialog(App game, int systemid = 0, string template = "dialogPopulationManager") : base(game, template)
		{
			this.App = game;
			this.initialsystemid = systemid;
		}
		public override void Initialize()
		{
			this.App.UI.UnlockUI();
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
					this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "planetDetailsPop_Card");
					string itemGlobalID2 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID2));
					this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, true, false);
				}
				else
				{
					if (this.App.AssetDatabase.IsGasGiant(current3.Type))
					{
						this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "gasgiantDetailsM_Card");
						string itemGlobalID3 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
						this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID3));
						this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
					}
					else
					{
						if (this.App.AssetDatabase.IsMoon(current3.Type))
						{
							this.App.UI.AddItem("system_list", "", current3.ID + 999999, "", "moonDetailsM_Card");
							string itemGlobalID4 = this.App.UI.GetItemGlobalID("system_list", "", current3.ID + 999999, "");
							this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalID4));
							this._planetWidgets.Last<PlanetWidget>().Sync(current3.ID, false, false);
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
					AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, current.ID);
					if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
					{
						list2.Add(current);
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
					if (current.ID == this.initialsystemid || (flag && this.initialsystemid == 0))
					{
						this.SetSyncedSystem(current);
						this.App.UI.SetSelection("sys_list_left", current.ID);
						flag = false;
					}
				}
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed" && panelName == "sys_list_left")
			{
				int systemId = int.Parse(msgParams[0]);
				StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(systemId);
				if (starSystemInfo != null)
				{
					this.SetSyncedSystem(starSystemInfo);
				}
			}
			if (msgType == "button_clicked" && panelName == PopulationManagerDialog.UIExitButton)
			{
				this.App.UI.CloseDialog(this, true);
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
