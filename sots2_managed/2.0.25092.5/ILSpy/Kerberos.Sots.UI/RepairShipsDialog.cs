using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class RepairShipsDialog : Dialog
	{
		private FleetWidget _leftWidget;
		private FleetWidget _rightWidget;
		private FleetWidget _colonyWidget;
		private FleetWidget _suulkaWidget;
		private FleetWidget _suulkaDrainWidget;
		private int _systemID;
		private List<FleetInfo> _fleets;
		public RepairShipsDialog(App game, int systemID, List<FleetInfo> fleets, string template = "dialogRepairShips") : base(game, template)
		{
			this._fleets = fleets;
			this._systemID = systemID;
		}
		public override void Initialize()
		{
			this._colonyWidget = new FleetWidget(this._app, base.UI.Path(new string[]
			{
				base.ID,
				"repairWidgetColonyList"
			}));
			List<PlanetInfo> list = this._app.GameDatabase.GetStarSystemPlanetInfos(this._systemID).ToList<PlanetInfo>();
			List<PlanetInfo> list2 = new List<PlanetInfo>();
			List<ColonyInfo> list3 = new List<ColonyInfo>();
			foreach (PlanetInfo current in list)
			{
				ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(current.ID);
				if (colonyInfoForPlanet != null)
				{
					list3.Add(colonyInfoForPlanet);
					if (colonyInfoForPlanet.PlayerID == this._app.LocalPlayer.ID)
					{
						list2.Add(current);
					}
				}
			}
			this._colonyWidget.SetSyncedPlanets(list2);
			this._suulkaWidget = new FleetWidget(this._app, base.UI.Path(new string[]
			{
				base.ID,
				"gameRightListS"
			}));
			this._suulkaWidget.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.SuulkaListFilter);
			this._suulkaWidget.SuulkaMode = true;
			this._suulkaWidget.DisableTooltips = true;
			this._suulkaDrainWidget = new FleetWidget(this._app, base.UI.Path(new string[]
			{
				base.ID,
				"repairWidgetColonyList"
			}));
			List<PlanetInfo> list4 = new List<PlanetInfo>();
			bool flag = this._fleets.Any((FleetInfo x) => x.IsNormalFleet && x.PlayerID != this._app.LocalPlayer.ID);
			foreach (PlanetInfo pi in list)
			{
				ColonyInfo colonyInfo = list3.FirstOrDefault((ColonyInfo x) => x.OrbitalObjectID == pi.ID);
				if ((!flag || colonyInfo == null || colonyInfo.PlayerID == this._app.LocalPlayer.ID) && (pi.Biosphere > 0 || colonyInfo != null))
				{
					list4.Add(pi);
				}
			}
			this._suulkaDrainWidget.SetSyncedPlanets(list4);
			this._leftWidget = new FleetWidget(this._app, base.UI.Path(new string[]
			{
				base.ID,
				"gameLeftList"
			}));
			this._rightWidget = new FleetWidget(this._app, base.UI.Path(new string[]
			{
				base.ID,
				"gameRightList"
			}));
			this._leftWidget.DisableTooltips = true;
			this._rightWidget.RidersEnabled = true;
			this._rightWidget.SeparateDefenseFleet = false;
			this._rightWidget.DisableTooltips = true;
			this._leftWidget.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.LeftListFilter);
			this._rightWidget.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.RightListFilter);
			this._leftWidget.ShowColonies = true;
			this._leftWidget.OnlyLocalPlayer = true;
			this._leftWidget.ListStations = true;
			this._rightWidget.ListStations = true;
			List<StationInfo> syncedStations = this._app.GameDatabase.GetStationForSystemAndPlayer(this._systemID, this._app.LocalPlayer.ID).ToList<StationInfo>();
			this._leftWidget.SetSyncedFleets(this._fleets);
			this._rightWidget.SetSyncedFleets(this._fleets);
			this._leftWidget.SetSyncedStations(syncedStations);
			this._rightWidget.SetSyncedStations(syncedStations);
			this._suulkaWidget.SetSyncedFleets(this._fleets);
			this._leftWidget.ShowEmptyFleets = false;
			this._rightWidget.ShowEmptyFleets = false;
			this._leftWidget.ShowFleetInfo = false;
			this._rightWidget.ShowFleetInfo = false;
			this._suulkaWidget.ShowEmptyFleets = false;
			this._suulkaWidget.ShowFleetInfo = false;
			this._rightWidget.RepairWidget = this._leftWidget;
			this._suulkaWidget.RepairWidget = this._suulkaDrainWidget;
			this._leftWidget.ShowRepairPoints = true;
			this._rightWidget.RepairMode = true;
			this._leftWidget.ExpandAll();
			this._rightWidget.ExpandAll();
			this._suulkaWidget.ExpandAll();
		}
		public FleetWidget.FilterShips LeftListFilter(ShipInfo ship, DesignInfo design)
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(ship.FleetID);
			int num = (fleetInfo != null) ? fleetInfo.PlayerID : design.PlayerID;
			if (num != this._app.LocalPlayer.ID)
			{
				return FleetWidget.FilterShips.Ignore;
			}
			int num2 = 0;
			DesignSectionInfo[] designSections = design.DesignSections;
			for (int i = 0; i < designSections.Length; i++)
			{
				DesignSectionInfo designSectionInfo = designSections[i];
				ShipSectionAsset shipSectionAsset = this._app.AssetDatabase.GetShipSectionAsset(designSectionInfo.FilePath);
				num2 += shipSectionAsset.RepairPoints;
			}
			if (num2 > 0)
			{
				return FleetWidget.FilterShips.Enable;
			}
			return FleetWidget.FilterShips.Ignore;
		}
		public FleetWidget.FilterShips RightListFilter(ShipInfo ship, DesignInfo design)
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(ship.FleetID);
			int num = (fleetInfo != null) ? fleetInfo.PlayerID : design.PlayerID;
            if (num != this._app.LocalPlayer.ID || Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._app, design) || FleetWidget.IsWeaponBattleRider(design))
			{
				return FleetWidget.FilterShips.Ignore;
			}
            Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(this._app.Game, design, ship.ID);
			return FleetWidget.FilterShips.Enable;
		}
		public FleetWidget.FilterShips SuulkaListFilter(ShipInfo ship, DesignInfo design)
		{
            if (Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._app, design))
			{
				return FleetWidget.FilterShips.Enable;
			}
			return FleetWidget.FilterShips.Ignore;
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "gameDoneButton")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "gameRepairSuulkasButton")
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						base.ID,
						"repairDialog"
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						base.ID,
						"repairSuulkaDialog"
					}), true);
					return;
				}
				if (panelName == "gameBackButton")
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						base.ID,
						"repairDialog"
					}), true);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						base.ID,
						"repairSuulkaDialog"
					}), false);
					return;
				}
				if (panelName == "gameUndoAllButton")
				{
					this._rightWidget.UndoAll();
					return;
				}
				if (panelName == "gameRepairAllButton")
				{
					this._rightWidget.RepairAll();
					return;
				}
				if (panelName == "gameConfirmRepairsButton")
				{
					this._rightWidget.ConfirmRepairs();
				}
			}
		}
		public override string[] CloseDialog()
		{
			this._leftWidget.Dispose();
			this._rightWidget.Dispose();
			this._suulkaWidget.Dispose();
			this._colonyWidget.Dispose();
			this._suulkaDrainWidget.Dispose();
			StarMapState gameState = this._app.GetGameState<StarMapState>();
			if (gameState != null)
			{
				gameState.RefreshSystemInterface();
			}
			return null;
		}
	}
}
