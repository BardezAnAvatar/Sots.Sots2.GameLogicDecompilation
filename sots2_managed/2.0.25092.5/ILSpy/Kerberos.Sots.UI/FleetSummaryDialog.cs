using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class FleetSummaryDialog : Dialog
	{
		public enum FilterFleets
		{
			All,
			NormalFleet,
			Name,
			Admiral,
			BaseSystem,
			UnitCount,
			Mission,
			NoMission
		}
		public const string OKButton = "okButton";
		private App App;
		private FleetWidget _mainWidget;
		private List<FleetInfo> _fleets;
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private Dictionary<ShipRole, bool> _interestedRoles = new Dictionary<ShipRole, bool>();
		private FleetSummaryDialog.FilterFleets _currentFilterMode;
		public FleetSummaryDialog.FilterFleets CurrentFilterMode
		{
			get
			{
				return this._currentFilterMode;
			}
			set
			{
				if (this._currentFilterMode == value)
				{
					return;
				}
				this._currentFilterMode = value;
				this.RefreshFleets();
			}
		}
		public FleetSummaryDialog(App game, string template = "FleetSummaryDialog") : base(game, template)
		{
			this.App = game;
		}
		private void RefreshFleets()
		{
			if (this._fleets == null)
			{
				this._fleets = this._app.GameDatabase.GetFleetInfosByPlayerID(this.App.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).ToList<FleetInfo>();
			}
			List<FleetInfo> list = (
				from x in this._fleets
				where this.IsKeeperFleet(x)
				select x).ToList<FleetInfo>();
			switch (this._currentFilterMode)
			{
			case FleetSummaryDialog.FilterFleets.NormalFleet:
				list = (
					from x in list
					where x.Type == FleetType.FL_NORMAL
					select x).ToList<FleetInfo>();
				break;
			case FleetSummaryDialog.FilterFleets.Name:
				list = (
					from x in list
					orderby x.Name
					select x).ToList<FleetInfo>();
				break;
			case FleetSummaryDialog.FilterFleets.Admiral:
				list = list.OrderBy(delegate(FleetInfo x)
				{
					if (x.AdmiralID == 0)
					{
						return "";
					}
					return this._app.GameDatabase.GetAdmiralInfo(x.AdmiralID).Name;
				}).ToList<FleetInfo>();
				break;
			case FleetSummaryDialog.FilterFleets.BaseSystem:
				list = list.OrderBy(delegate(FleetInfo x)
				{
					if (x.SupportingSystemID == 0)
					{
						return "";
					}
					return this._app.GameDatabase.GetStarSystemInfo(x.SupportingSystemID).Name;
				}).ToList<FleetInfo>();
				break;
			case FleetSummaryDialog.FilterFleets.UnitCount:
				list = (
					from x in list
					orderby this._app.GameDatabase.GetShipsByFleetID(x.ID).Count<int>() descending
					select x).ToList<FleetInfo>();
				break;
			case FleetSummaryDialog.FilterFleets.Mission:
				list = (
					from x in list
					where x.Type == FleetType.FL_NORMAL
					orderby this._app.GameDatabase.GetMissionByFleetID(x.ID) != null descending
					select x).ToList<FleetInfo>();
				break;
			case FleetSummaryDialog.FilterFleets.NoMission:
				list = (
					from x in list
					where x.Type == FleetType.FL_NORMAL && this._app.GameDatabase.GetMissionByFleetID(x.ID) == null
					select x).ToList<FleetInfo>();
				break;
			}
			this._mainWidget.SetSyncedFleets(list);
		}
		private bool IsKeeperFleet(FleetInfo fleet)
		{
			List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>();
			if (list.Count == 0)
			{
				return false;
			}
			if (!this._interestedRoles.ContainsValue(true))
			{
				return true;
			}
			foreach (ShipInfo current in list)
			{
				bool flag;
				if (!this._interestedRoles.TryGetValue(current.DesignInfo.Role, out flag))
				{
					flag = false;
				}
				if (flag)
				{
					return true;
				}
			}
			return false;
		}
		public override void Initialize()
		{
			this.App.UI.UnlockUI();
			this._mainWidget = new FleetWidget(this.App, base.UI.Path(new string[]
			{
				base.ID,
				"fleetList"
			}));
			this._mainWidget.JumboMode = true;
			this.App.UI.AddItem("filterDropdown", "", 0, App.Localize("@UI_GENERAL_ALL"));
			this.App.UI.AddItem("filterDropdown", "", 1, App.Localize("@UI_GENERAL_NORMAL_FLEETS"));
			this.App.UI.AddItem("filterDropdown", "", 2, App.Localize("@UI_GENERAL_NAME"));
			this.App.UI.AddItem("filterDropdown", "", 3, App.Localize("@UI_GENERAL_ADMIRAL"));
			this.App.UI.AddItem("filterDropdown", "", 4, App.Localize("@UI_GENERAL_BASE_SYSTEM"));
			this.App.UI.AddItem("filterDropdown", "", 5, App.Localize("@UI_GENERAL_UNITCOUNT"));
			this.App.UI.AddItem("filterDropdown", "", 6, App.Localize("@UI_GENERAL_MISSION"));
			this.App.UI.AddItem("filterDropdown", "", 7, App.Localize("@UI_MISSIONFLEET_NO_MISSION"));
			this.App.UI.SetSelection("filterDropdown", 0);
			this._currentFilterMode = FleetSummaryDialog.FilterFleets.All;
			this._mainWidget.OnFleetSelectionChanged = new FleetWidget.FleetSelectionChangedDelegate(FleetSummaryDialog.FleetSelectionChanged);
			this.RefreshFleets();
		}
		public static void FleetSelectionChanged(App App, int fleetid)
		{
			StarMapState starMapState = (StarMapState)App.CurrentState;
			if (starMapState != null && starMapState.StarMap != null && starMapState.StarMap.Fleets.Reverse.ContainsKey(fleetid))
			{
				StarMapFleet starMapFleet = starMapState.StarMap.Fleets.Reverse[fleetid];
				if (starMapFleet != null)
				{
					starMapState.StarMap.SetFocus(starMapFleet);
					starMapState.StarMap.Select(starMapFleet);
				}
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				if (msgType == "list_item_dblclk")
				{
					return;
				}
				if (msgType == "list_sel_changed")
				{
					if (panelName == "filterDropdown")
					{
						int currentFilterMode = int.Parse(msgParams[0]);
						this.CurrentFilterMode = (FleetSummaryDialog.FilterFleets)currentFilterMode;
						return;
					}
				}
				else
				{
					if (msgType == "checkbox_clicked")
					{
						bool value = int.Parse(msgParams[0]) > 0;
						if (panelName == "hasColonizer")
						{
							this._interestedRoles[ShipRole.COLONIZER] = value;
							this.RefreshFleets();
							return;
						}
						if (panelName == "hasConstructor")
						{
							this._interestedRoles[ShipRole.CONSTRUCTOR] = value;
							this.RefreshFleets();
							return;
						}
						if (panelName == "hasSupply")
						{
							this._interestedRoles[ShipRole.SUPPLY] = value;
							this.RefreshFleets();
						}
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			this._mainWidget.Dispose();
			this._mainWidget = null;
			return null;
		}
	}
}
