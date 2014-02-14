using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class OverlayGateMission : OverlayMission
	{
		public OverlayGateMission(App game, StarMapState state, StarMap starmap, string template = "OverlaySurveyMission") : base(game, state, starmap, MissionType.GATE, template)
		{
		}
		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}
		protected override bool CanConfirmMission()
		{
            return base.IsValidFleetID(base.SelectedFleet) && base.TargetSystem != 0 && Kerberos.Sots.StarFleet.StarFleet.CanDoGateMissionToTarget(this.App.Game, base.TargetSystem, base.SelectedFleet, null, null, null);
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", base.TargetSystem, false, true);
		}
		protected override void OnCommitMission()
		{
            Kerberos.Sots.StarFleet.StarFleet.SetGateMission(this.App.Game, base.SelectedFleet, base.TargetSystem, this._useDirectRoute, base.GetDesignsToBuild(), null);
			this.App.GetGameState<StarMapState>().RefreshMission();
		}
		protected override string GetMissionDetailsTitle()
		{
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(base.TargetSystem);
			return string.Format(App.Localize("@UI_GATE_OVERLAY_MISSION_TITLE"), starSystemInfo.Name.ToUpperInvariant());
		}
		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			base.AddCommonMissionTimes(estimate);
			string hint = App.Localize("@UI_GATE_OVERLAY_MISSION_HINT");
			base.AddMissionTime(2, App.Localize("@UI_MISSION_GATE"), estimate.TurnsAtTarget, hint);
			base.AddMissionCost(estimate);
			base.UpdateCanConfirmMission();
		}
		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}
	}
}
