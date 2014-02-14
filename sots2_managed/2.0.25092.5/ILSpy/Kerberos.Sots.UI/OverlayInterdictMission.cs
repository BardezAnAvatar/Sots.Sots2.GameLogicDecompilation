using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class OverlayInterdictMission : OverlayMission
	{
		public OverlayInterdictMission(App game, StarMapState state, StarMap starmap, string template = "OverlaySurveyMission") : base(game, state, starmap, MissionType.INTERDICTION, template)
		{
		}
		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}
		protected override bool CanConfirmMission()
		{
			return base.IsValidFleetID(base.SelectedFleet) && base.TargetSystem != 0;
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", base.TargetSystem, false, true);
		}
		protected override void OnCommitMission()
		{
            Kerberos.Sots.StarFleet.StarFleet.SetInterdictionMission(this.App.Game, base.SelectedFleet, base.TargetSystem, this._useDirectRoute, 0, base.GetDesignsToBuild());
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			if (admiralInfo != null)
			{
				string cueName = string.Format("STRAT_005-01_{0}_{1}InterdictMissionConfirmation", this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase));
				this.App.PostRequestSpeech(cueName, 50, 120, 0f);
			}
			this.App.GetGameState<StarMapState>().RefreshMission();
		}
		protected override string GetMissionDetailsTitle()
		{
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(base.TargetSystem);
			return string.Format(App.Localize("@UI_INTERDICT_OVERLAY_MISSION_TITLE"), starSystemInfo.Name.ToUpperInvariant());
		}
		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			base.AddCommonMissionTimes(estimate);
			string hint = App.Localize("@UI_INTERDICT_OVERLAY_MISSION_HINT");
			base.AddMissionTime(2, App.Localize("@MISSIONWIDGET_INTERDICTION_TIME"), estimate.TurnsAtTarget, hint);
			base.AddMissionCost(estimate);
		}
		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}
	}
}
