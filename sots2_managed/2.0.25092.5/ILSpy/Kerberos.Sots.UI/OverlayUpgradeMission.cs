using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class OverlayUpgradeMission : OverlayMission
	{
		private int _startSelect;
		private int _selectedStation;
		public int StartSelect
		{
			get
			{
				return this._startSelect;
			}
			set
			{
				this._startSelect = value;
			}
		}
		public int SelectedStation
		{
			get
			{
				return this._selectedStation;
			}
			set
			{
				this._selectedStation = value;
				this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
				base.UpdateCanConfirmMission();
			}
		}
		protected override bool CanConfirmMission()
		{
			return base.IsValidFleetID(base.SelectedFleet) && this.SelectedStation != 0;
		}
		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}
		public OverlayUpgradeMission(App game, StarMapState state, StarMap starmap) : base(game, state, starmap, MissionType.CONSTRUCT_STN, "OverlayUpgradeStationMission")
		{
		}
		protected override void OnCommitMission()
		{
			StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(this.SelectedStation);
			if (this._app.LocalPlayer.Faction.Name == "loa")
			{
                Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, this._selectedFleet);
                Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, this._selectedFleet, MissionType.UPGRADE_STN);
				base.RebuildShipLists(base.SelectedFleet);
			}
            Kerberos.Sots.StarFleet.StarFleet.SetUpgradeStationMission(this.App.Game, base.SelectedFleet, base.TargetSystem, false, stationInfo.OrbitalObjectID, base.GetDesignsToBuild(), stationInfo.DesignInfo.StationType, base.RebaseTarget);
			this.App.GetGameState<StarMapState>().RefreshMission();
		}
		public static void RefreshMissionUI(App game, int selectedPlanet, int targetSystem)
		{
			game.UI.SetText("gameMissionTitle", App.Localize("@MISSIONTITLE_CONSTRUCTION_MISSION"));
			game.UI.ClearItems("gameMissionTimes");
			game.UI.ClearItems("gameMissionNotes");
		}
		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			OverlayUpgradeMission.ReRefreshMissionDetails(this.App, estimate);
		}
		public static void ReRefreshMissionDetails(App game, MissionEstimate estimate)
		{
			OverlayMission.AddCommonMissionTimes(game, estimate);
			string hint = App.Localize("@MISSIONWIDGET_TOOLTIP_UPGRADE_TIME");
			OverlayMission.AddMissionTime(game, 2, App.Localize("@MISSIONWIDGET_CONSTRUCTION_TIME"), estimate.TurnsAtTarget, hint);
			OverlayMission.AddMissionCost(game, estimate);
		}
		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return StarSystemDetailsUI.CollectPlanetListItemsForConstructionMission(this.App, base.TargetSystem);
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			base.UI.SetVisible("overlayStationList", false);
			base.UI.SetVisible("stationTypes", true);
			EmpireBarUI.SyncTitleFrame(this.App);
			this.App.UI.ClearItems("station_list");
			IEnumerable<StationInfo> stationForSystemAndPlayer = this.App.GameDatabase.GetStationForSystemAndPlayer(base.TargetSystem, this.App.LocalPlayer.ID);
			List<StationInfo> upgradableStations = this.App.Game.GetUpgradableStations(stationForSystemAndPlayer.ToList<StationInfo>());
			foreach (StationInfo current in upgradableStations)
			{
				this.App.UI.AddItem("station_list", string.Empty, current.OrbitalObjectID, string.Empty);
				StationUI.SyncStationDetailsWidget(this.App.Game, this.App.UI.GetItemGlobalID("station_list", string.Empty, current.OrbitalObjectID, string.Empty), current.OrbitalObjectID, true);
			}
			OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this.StartSelect);
			if (orbitalObjectInfo != null && orbitalObjectInfo.StarSystemID == base.TargetSystem)
			{
				this.SelectedStation = this.StartSelect;
			}
			else
			{
				this.SelectedStation = upgradableStations[0].OrbitalObjectID;
			}
			this.App.UI.SetSelection("station_list", this.SelectedStation);
		}
		protected override string GetMissionDetailsTitle()
		{
			return App.Localize("@MISSIONTITLE_STATION_UPGRADE_MISSION");
		}
		protected override void RefreshMissionDetails(StationType stationType = StationType.INVALID_TYPE, int stationLevel = 1)
		{
			StationInfo stationInfo = this.App.GameDatabase.GetStationInfo(this.SelectedStation);
			if (stationInfo != null)
			{
				base.RefreshMissionDetails(stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1);
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
			{
				if (msgType == "list_sel_changed")
				{
					if (panelName == "station_list" && msgParams[0] != string.Empty)
					{
						this.SelectedStation = int.Parse(msgParams[0]);
					}
				}
				else
				{
					if (msgType == "checkbox_clicked")
					{
						bool reBaseToggle = int.Parse(msgParams[0]) > 0;
						if (panelName == "gameRebaseToggleStn")
						{
							base.ReBaseToggle = reBaseToggle;
							this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
						}
					}
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}
	}
}
