using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class OverlayColonizeMission : OverlayMission
	{
		private const string UIPlanetDetailsWidget = "planetDetailsWidget";
		private const string UIFleetDetailsWidget = "fleetDetailsWidget";
		private const string UIPlanetListWidget = "gamePlanetList";
		private PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private bool _cachedPlanetReady;
		private StarModel _cachedStar;
		private bool _cachedStarReady;
		private int _selectedPlanetForColonize;
		private PlanetWidget _planetWidget;
		public OverlayColonizeMission(App game, StarMapState state, StarMap starmap, string template = "OverlayColonizeMission") : base(game, state, starmap, MissionType.COLONIZATION, template)
		{
		}
		protected override bool CanConfirmMission()
		{
			List<MissionInfo> source = (
				from x in this._app.GameDatabase.GetMissionsByPlanetDest(this._selectedPlanetForColonize)
				where x.Type == MissionType.COLONIZATION && this._app.GameDatabase.GetFleetInfo(x.FleetID).PlayerID == this.App.LocalPlayer.ID
				select x).ToList<MissionInfo>();
			return !source.Any<MissionInfo>() && this.App.GameDatabase.CanColonizePlanet(this.App.LocalPlayer.ID, this._selectedPlanetForColonize, this.App.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this.App.LocalPlayer.ID)) && base.IsValidFleetID(base.SelectedFleet);
		}
		protected override void OnCommitMission()
		{
			if (this._selectedPlanetForColonize == 0)
			{
				IEnumerable<int> starSystemPlanets = this.App.GameDatabase.GetStarSystemPlanets(base.TargetSystem);
				foreach (int current in starSystemPlanets)
				{
					bool flag = this.App.GameDatabase.CanColonizePlanet(this.App.LocalPlayer.ID, current, this.App.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, this.App.LocalPlayer.ID));
					if (flag)
					{
						this._selectedPlanetForColonize = current;
						break;
					}
				}
			}
			if (this._selectedPlanetForColonize == 0)
			{
				return;
			}
			if (this._app.LocalPlayer.Faction.Name == "loa")
			{
                Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, this._selectedFleet);
                Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, this._selectedFleet, MissionType.COLONIZATION);
				base.RebuildShipLists(base.SelectedFleet);
			}
            Kerberos.Sots.StarFleet.StarFleet.SetColonizationMission(this.App.Game, base.SelectedFleet, base.TargetSystem, this._useDirectRoute, this._selectedPlanetForColonize, base.GetDesignsToBuild(), base.RebaseTarget);
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			if (admiralInfo != null)
			{
				string cueName = string.Format("STRAT_002-01_{0}_{1}ColonizationMissionConfirmation", this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase));
				this.App.PostRequestSpeech(cueName, 50, 120, 0f);
			}
			IEnumerable<int> missionTargetPlanets = this.GetMissionTargetPlanets();
			FleetUI.SyncPlanetListControl(this.App.Game, this.App.UI.Path(new string[]
			{
				base.ID,
				"overlayPlanetList"
			}), missionTargetPlanets);
			this.App.GetGameState<StarMapState>().RefreshMission();
		}
		protected override string GetMissionDetailsTitle()
		{
			string name;
			if (this._selectedPlanetForColonize != 0)
			{
				name = this.App.GameDatabase.GetOrbitalObjectInfo(this._selectedPlanetForColonize).Name;
			}
			else
			{
				name = this.App.GameDatabase.GetStarSystemInfo(base.TargetSystem).Name;
			}
			return string.Format(App.Localize("@MISSIONWIDGET_COLONIZE_PLANET_NAME"), name.ToUpperInvariant());
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (FleetUI.HandleFleetAndPlanetWidgetInput(this.App, "fleetDetailsWidget", panelName))
			{
				base.UpdateCanConfirmMission();
				return;
			}
			if (msgType == "mapicon_clicked")
			{
				if (panelName == "partMiniSystem" && this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, base.TargetSystem))
				{
					int value = int.Parse(msgParams[0]);
					this.SetSelectedPlanet(value, panelName);
					return;
				}
			}
			else
			{
				if (msgType == "list_sel_changed")
				{
					if (panelName == "overlayPlanetList")
					{
						int value2 = 0;
						if (msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
						{
							value2 = int.Parse(msgParams[0]);
						}
						this.SetSelectedPlanet(value2, panelName);
					}
					else
					{
						if (panelName == "gameFleetList" && msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
						{
							base.SelectedFleet = int.Parse(msgParams[0]);
						}
					}
					base.UpdateCanConfirmMission();
					return;
				}
				base.OnPanelMessage(panelName, msgType, msgParams);
			}
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
		public void SetSelectedPlanet(int value)
		{
			this._selectedPlanetForColonize = value;
		}
		private void SetSelectedPlanet(int value, string trigger)
		{
			if (this._selectedPlanetForColonize == value && trigger != "onenter")
			{
				return;
			}
			this._selectedPlanetForColonize = value;
			this._planetWidget.Sync(this._selectedPlanetForColonize, false, false);
			StarSystemUI.SyncPlanetDetailsWidget(this.App.Game, "planetDetailsWidget", base.TargetSystem, this._selectedPlanetForColonize, this.GetPlanetViewGameObject(base.TargetSystem, this._selectedPlanetForColonize), this._planetView);
			this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
			base.UpdateCanConfirmMission();
		}
		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			base.AddCommonMissionTimes(estimate);
			string hint = App.Localize("@MISSIONWIDGET_TOOLTIP_COLONIZATION_TIME");
			base.AddMissionTime(2, App.Localize("@MISSIONWIDGET_COLONIZATION_TIME"), estimate.TurnsAtTarget, hint);
			string hint2 = App.Localize("@COLONIZEMISSION_HINT");
			base.AddMissionTime(6, App.Localize("@MISSIONWIDGET_SUPPORT_TIME"), estimate.TurnsColonySupport, hint2);
			base.AddMissionCost(estimate);
			int num = estimate.TurnsTillPhase1Completion;
			if (num < 1)
			{
				num = 1;
			}
			string arg = (num == 1) ? App.Localize("@UI_GENERAL_TURN") : App.Localize("@UI_GENERAL_TURNS");
			string note = string.Format(App.Localize("@MISSIONWIDGET_PHASE_ONE"), num, arg);
			base.AddMissionNote(note);
			if (this._fleetCentric)
			{
				IEnumerable<int> missionTargetPlanets = this.GetMissionTargetPlanets();
				FleetUI.SyncPlanetListControl(this.App.Game, this.App.UI.Path(new string[]
				{
					base.ID,
					"overlayPlanetList"
				}), missionTargetPlanets);
			}
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			this._planetWidget = new PlanetWidget(this.App, this.App.UI.Path(new string[]
			{
				base.ID,
				"planetDetailsCard"
			}));
			IEnumerable<int> missionTargetPlanets = this.GetMissionTargetPlanets();
			FleetUI.SyncPlanetListControl(this.App.Game, this.App.UI.Path(new string[]
			{
				base.ID,
				"overlayPlanetList"
			}), missionTargetPlanets);
			this.App.UI.SetEnabled("gameConfirmMissionButton", false);
			if (this._selectedPlanetForColonize != 0)
			{
				this.SetSelectedPlanet(this._selectedPlanetForColonize, "onenter");
				return;
			}
			if (missionTargetPlanets.Count<int>() > 0)
			{
				this.SetSelectedPlanet(missionTargetPlanets.First<int>(), "onenter");
			}
		}
		protected override void OnUpdate()
		{
			if (this._planetWidget != null)
			{
				this._planetWidget.Update();
			}
			base.OnUpdate();
		}
		protected override void OnExit()
		{
			this._planetView = null;
			this._planetWidget.Terminate();
			if (this._cachedPlanet != null)
			{
				this.App.ReleaseObject(this._cachedPlanet);
			}
			this._cachedPlanet = null;
			this._cachedPlanetReady = false;
			if (this._cachedStar != null)
			{
				this.App.ReleaseObject(this._cachedStar);
			}
			this._cachedStar = null;
			this._cachedStarReady = false;
			this._selectedPlanetForColonize = 0;
			base.OnExit();
		}
		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this.App, base.TargetSystem, this.App.LocalPlayer.ID);
		}
		private void CacheStar(StarSystemInfo systemInfo)
		{
			if (this._cachedStar != null)
			{
				this.App.ReleaseObject(this._cachedStar);
				this._cachedStar = null;
			}
			this._cachedStarReady = false;
			this._cachedStar = Kerberos.Sots.GameStates.StarSystem.CreateStar(this.App, Vector3.Zero, systemInfo, 1f, false);
			this._cachedStar.PostSetProp("AutoDraw", false);
		}
		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (this._cachedPlanet != null)
			{
				this.App.ReleaseObject(this._cachedPlanet);
				this._cachedPlanet = null;
			}
			this._cachedPlanetReady = false;
			this._cachedPlanet = Kerberos.Sots.GameStates.StarSystem.CreatePlanet(this.App.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, Kerberos.Sots.GameStates.StarSystem.TerrestrialPlanetQuality.High);
			this._cachedPlanet.PostSetProp("AutoDraw", false);
		}
		private void UpdateCachedPlanet()
		{
			if (this._cachedPlanet != null && !this._cachedPlanetReady && this._cachedPlanet.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedPlanetReady = true;
				this._cachedPlanet.Active = true;
			}
		}
		private void UpdateCachedStar()
		{
			if (this._cachedStar != null && !this._cachedStarReady && this._cachedStar.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedStarReady = true;
				this._cachedStar.Active = true;
			}
		}
		private IGameObject GetPlanetViewGameObject(int systemId, int orbitId)
		{
			IGameObject result = null;
			if (systemId != 0)
			{
				if (orbitId > 0)
				{
					this.CachePlanet(this.App.GameDatabase.GetPlanetInfo(orbitId));
					result = this._cachedPlanet;
				}
				else
				{
					this.CacheStar(this.App.GameDatabase.GetStarSystemInfo(systemId));
					result = this._cachedStar;
				}
			}
			return result;
		}
	}
}
