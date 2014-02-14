using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class OverlayInterceptMission : OverlayMission
	{
		private const string UIFleetDetailsWidget = "fleetDetailsWidget";
		public int TargetFleet
		{
			get;
			set;
		}
		public OverlayInterceptMission(App game, StarMapState state, StarMap starmap, string template = "OverlayInterceptMission") : base(game, state, starmap, MissionType.INTERCEPT, template)
		{
		}
		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return new List<int>();
		}
		protected override bool CanConfirmMission()
		{
			return base.IsValidFleetID(base.SelectedFleet);
		}
		protected override void OnCommitMission()
		{
            Kerberos.Sots.StarFleet.StarFleet.SetFleetInterceptMission(this.App.Game, base.SelectedFleet, this.TargetFleet, this._useDirectRoute, base.GetDesignsToBuild());
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			if (admiralInfo != null)
			{
				string cueName = string.Format("STRAT_010-01_{0}_{1}InterceptMissionConfirmation", this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase));
				this.App.PostRequestSpeech(cueName, 50, 120, 0f);
			}
			this.App.GetGameState<StarMapState>().RefreshMission();
		}
		protected override string GetMissionDetailsTitle()
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this.TargetFleet);
			return string.Format(App.Localize("@MISSIONWIDGET_INTERCEPT_FLEET_NAME"), fleetInfo.Name);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (FleetUI.HandleFleetAndPlanetWidgetInput(this.App, "fleetDetailsWidget", panelName))
			{
				base.UpdateCanConfirmMission();
				return;
			}
			if (msgType == "list_sel_changed")
			{
				if (panelName == "gameFleetList" && msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
				{
					base.SelectedFleet = int.Parse(msgParams[0]);
				}
				base.UpdateCanConfirmMission();
				return;
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}
		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
			if (newValue)
			{
				this.App.UI.SetEnabled("gameConfirmMissionButton", true);
				this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
				return;
			}
			this.App.UI.SetEnabled("gameConfirmMissionButton", false);
		}
		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			base.AddCommonMissionTimes(estimate);
			base.AddMissionCost(estimate);
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetEnabled("gameConfirmMissionButton", false);
		}
		protected override void OnUpdate()
		{
			base.OnUpdate();
		}
		protected override void OnExit()
		{
			base.OnExit();
		}
	}
}
