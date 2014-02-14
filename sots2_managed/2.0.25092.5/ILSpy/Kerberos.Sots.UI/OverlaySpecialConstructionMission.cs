using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class OverlaySpecialConstructionMission : OverlayConstructionMission
	{
		public int TargetFleet
		{
			get;
			set;
		}
		public OverlaySpecialConstructionMission(App game, StarMapState state, StarMap starmap, SpecialConstructionMission smission = null, string template = "OverlayGMBuildStationMission") : base(game, state, starmap, smission, template, MissionType.SPECIAL_CONSTRUCT_STN)
		{
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetEnabled(this.App.UI.Path(new string[]
			{
				base.ID,
				"gamePlaceStationMissionButton"
			}), false);
			base.SyncAvailableStationTypes();
			this.SelectADefaultStation();
		}
		protected override void OnExit()
		{
			this.TargetFleet = 0;
			base.OnExit();
		}
		protected override bool CanConfirmMission()
		{
			return base.IsValidFleetID(base.SelectedFleet);
		}
		protected override void OnCommitMission()
		{
			OverlaySpecialConstructionMission.OnSpecialConstructionPlaced(this._app.Game, base.SelectedFleet, this.TargetFleet, this._useDirectRoute, base.GetDesignsToBuild(), this.SelectedStationType);
		}
		protected override List<StationType> GetAvailableTypes()
		{
			return new List<StationType>
			{
				StationType.SCIENCE
			};
		}
		protected override void SelectADefaultStation()
		{
			base.SetSelectedStationType(StationType.SCIENCE);
		}
		public static void OnSpecialConstructionPlaced(GameSession sim, int selectedFleet, int targetFleet, bool useDirectRoute, List<int> designsToBuild, StationType stationType)
		{
            Kerberos.Sots.StarFleet.StarFleet.SetSpecialConstructionMission(sim, selectedFleet, targetFleet, useDirectRoute, designsToBuild, stationType);
			FleetInfo fleetInfo = sim.GameDatabase.GetFleetInfo(selectedFleet);
			AdmiralInfo admiralInfo = sim.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			if (admiralInfo != null)
			{
				string cueName = string.Format("STRAT_007-01_{0}_{1}ConstructionMissionConfirmation", sim.GameDatabase.GetFactionName(sim.GameDatabase.GetPlayerFactionID(sim.LocalPlayer.ID)), admiralInfo.GetAdmiralSoundCueContext(sim.AssetDatabase));
				sim.App.PostRequestSpeech(cueName, 50, 120, 0f);
			}
			sim.App.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
		}
	}
}
