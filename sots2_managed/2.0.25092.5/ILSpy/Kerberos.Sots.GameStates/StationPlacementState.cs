using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class StationPlacementState : BasicStarSystemState
	{
		private const string UIMissionAdmiralName = "gameAdmiralName";
		private const string UIMissionAdmiralFleet = "gameAdmiralFleet";
		private const string UIMissionAdmiralSkills = "gameAdmiralSkills";
		private const string UIMissionAdmiralAvatar = "gameAdmiralAvatar";
		protected const int UIItemMissionTotalTime = 0;
		protected const int UIItemMissionTravelTime = 1;
		protected const int UIItemMissionTime = 2;
		protected const int UIItemMissionBuildTime = 3;
		protected const int UIItemMissionCostSeparator = 4;
		protected const int UIItemMissionCost = 5;
		protected const int UIItemMissionSupportTime = 6;
		private int _targetSystemID;
		private int _selectedPlanetID;
		private GameSession _sim;
		private int _selectedFleetID;
		private List<int> _designsToBuild;
		private StationType _stationType;
		private GameObjectSet _crits;
		private GameObjectSet _dummies;
		private MissionEstimate _missionEstimate;
		private StationPlacement _manager;
		private BudgetPiechart _piechart;
		private bool _useDirectRoute;
		private int? _rebase;
		protected static readonly string UICancelButton = "gameCancelMissionButton";
		protected static readonly string UICommitButton = "gameConfirmMissionButton";
		public StationPlacementState(App game) : base(game)
		{
		}
		protected override void OnBack()
		{
			StarMapState gameState = base.App.GetGameState<StarMapState>();
			base.App.SwitchGameState(gameState, new object[0]);
			gameState.ShowOverlay(MissionType.CONSTRUCT_STN, this._targetSystemID);
		}
		public static string GetAdmiralTraitText(AdmiralInfo.TraitType trait)
		{
			return App.Localize(string.Format("@ADMIRALTRAITS_{0}", trait.ToString().ToUpper()));
		}
		public static string GetAdmiralTraitsString(App game, int admiralId)
		{
			string text = string.Empty;
			AdmiralInfo.TraitType[] array = (AdmiralInfo.TraitType[])Enum.GetValues(typeof(AdmiralInfo.TraitType));
			AdmiralInfo.TraitType[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				AdmiralInfo.TraitType traitType = array2[i];
				int levelForAdmiralTrait = game.GameDatabase.GetLevelForAdmiralTrait(admiralId, traitType);
				if (levelForAdmiralTrait > 0)
				{
					if (!string.IsNullOrEmpty(text))
					{
						text += ", ";
					}
					text += StationPlacementState.GetAdmiralTraitText(traitType);
				}
			}
			return text;
		}
		public static void RefreshFleetAdmiralDetails(App game, int fleetId)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetId);
			string arg = game.GameDatabase.GetFactionName(game.GameDatabase.GetFleetFaction(fleetId));
			string arg2 = string.Empty;
			string arg3 = string.Empty;
			if (fleetInfo.AdmiralID != 0)
			{
				AdmiralInfo admiralInfo = game.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
				arg2 = admiralInfo.Name;
				arg3 = StationPlacementState.GetAdmiralTraitsString(game, fleetInfo.AdmiralID);
				arg = admiralInfo.Race;
			}
			string text = string.Format(App.Localize("@MISSIONWIDGET_ADMIRAL"), arg2).ToUpperInvariant();
			game.UI.SetText("gameAdmiralName", text);
			string text2 = string.Format(App.Localize("@MISSIONWIDGET_FLEET"), fleetInfo.Name).ToUpperInvariant();
			game.UI.SetText("gameAdmiralFleet", text2);
			string text3 = string.Format(App.Localize("@MISSIONWIDGET_ADMIRAL_TRAITS"), arg3);
			game.UI.SetText("gameAdmiralSkills", text3);
			string propertyValue = string.Format("admiral_{0}", arg);
			if (fleetInfo.AdmiralID != 0)
			{
                propertyValue = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(game, fleetInfo.AdmiralID);
			}
			game.UI.SetPropertyString("gameAdmiralAvatar", "sprite", propertyValue);
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			base.OnPrepare(prev, stateParams);
			this._targetSystemID = (int)stateParams[0];
			this._selectedPlanetID = (int)stateParams[1];
			this._sim = (GameSession)stateParams[2];
			this._selectedFleetID = (int)stateParams[3];
			this._designsToBuild = (List<int>)stateParams[4];
			this._stationType = (StationType)stateParams[5];
			this._missionEstimate = (MissionEstimate)stateParams[6];
			this._useDirectRoute = (bool)stateParams[7];
			this._rebase = (int?)stateParams[8];
			this._crits = new GameObjectSet(base.App);
			this._dummies = new GameObjectSet(base.App);
			DesignInfo di = DesignLab.CreateStationDesignInfo(base.App.AssetDatabase, base.App.GameDatabase, base.App.LocalPlayer.ID, this._stationType, 1, false);
			ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.ShipSections.First((ShipSectionAsset x) => x.FileName == di.DesignSections[0].FilePath);
			StarSystemDummyOccupant starSystemDummyOccupant = new StarSystemDummyOccupant(base.App, shipSectionAsset.ModelName, this._stationType);
			this._dummies.Add(starSystemDummyOccupant);
			this._starsystem.PostObjectAddObjects(new IGameObject[]
			{
				starSystemDummyOccupant
			});
			this._manager = new StationPlacement(base.App, base.App.LocalPlayer.Faction.Name == "zuul");
			this._manager.PostSetProp("SetStarSystem", this._starsystem);
			this._manager.PostSetProp("SetPlacementStamp", starSystemDummyOccupant);
			this._manager.PostSetProp("SetMissionType", new object[]
			{
				this._stationType.ToFlags()
			});
			base.App.UI.LoadScreen("StationPlacement");
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			this._crits.Activate();
			this._dummies.Activate();
			base.App.UI.UnlockUI();
			base.App.UI.SetScreen("StationPlacement");
			base.App.UI.SetPropertyBool("gameCancelMissionButton", "lockout_button", true);
			base.App.UI.SetPropertyBool("gameConfirmMissionButton", "lockout_button", true);
			base.App.UI.SetPropertyBool("gameExitButton", "lockout_button", true);
			this._piechart = new BudgetPiechart(base.App.UI, "piechart", base.App.AssetDatabase);
			EmpireBarUI.SyncTitleFrame(base.App);
			EmpireBarUI.SyncTitleBar(base.App, "gameEmpireBar", this._piechart);
			base.Camera.DesiredPitch = MathHelper.DegreesToRadians(-40f);
			base.Camera.DesiredDistance = 80000f;
			base.Camera.MinPitch = MathHelper.DegreesToRadians(-60f);
			base.Camera.MaxPitch = MathHelper.DegreesToRadians(-20f);
			this._manager.Active = true;
			this._starsystem.SetAutoDrawEnabled(false);
			bool flag = false;
			if (this._stationType == StationType.MINING)
			{
				int? suitablePlanetForStation = StarSystem.GetSuitablePlanetForStation(base.App.Game, base.App.LocalPlayer.ID, this._targetSystemID, this._stationType);
				if (suitablePlanetForStation.HasValue)
				{
					base.SetSelectedObject(suitablePlanetForStation.Value, "");
					flag = true;
				}
			}
			else
			{
				IEnumerable<PlanetInfo> planetInfosOrbitingStar = base.App.GameDatabase.GetPlanetInfosOrbitingStar(this._targetSystemID);
				foreach (PlanetInfo current in planetInfosOrbitingStar)
				{
					if (current != null)
					{
						ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(current.ID);
						if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == base.App.LocalPlayer.ID)
						{
							base.SetSelectedObject(current.ID, "");
							flag = true;
							break;
						}
					}
				}
			}
			if (!flag)
			{
				base.SetSelectedObject(-1, "");
			}
			StationPlacementState.RefreshFleetAdmiralDetails(base.App, this._selectedFleetID);
			OverlayConstructionMission.RefreshMissionUI(base.App, this._selectedPlanetID, this._targetSystemID);
			OverlayConstructionMission.ReRefreshMissionDetails(base.App, this._missionEstimate);
			base.App.UI.AutoSizeContents("gameMissionDetails");
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this._piechart = null;
			this._manager.Dispose();
			this._crits.Dispose();
			this._crits = null;
			this._dummies.Dispose();
			this._dummies = null;
			this._sim = null;
			this._designsToBuild = null;
			base.OnExit(prev, reason);
		}
		public override bool IsReady()
		{
			return this._crits.IsReady() && this._dummies.IsReady() && base.IsReady() && base.IsReady();
		}
		protected override void OnUIGameEvent(string eventName, string[] eventParams)
		{
			this._piechart.TryGameEvent(eventName, eventParams);
			if (eventName == "StationSpotSelected")
			{
				this._selectedPlanetID = int.Parse(eventParams[0]);
				base.Camera.DesiredDistance = 4000f;
				base.Camera.PostSetProp("TargetID", 0);
				base.Camera.TargetPosition = new Vector3(float.Parse(eventParams[1]), float.Parse(eventParams[2]), float.Parse(eventParams[3]));
			}
			if (eventName == "StationSpotUnselected")
			{
				this._selectedPlanetID = int.Parse(eventParams[0]);
				base.Camera.DesiredDistance = 30000f;
				base.Camera.PostSetProp("TargetID", 0);
				base.Camera.TargetPosition = new Vector3(float.Parse(eventParams[1]), float.Parse(eventParams[2]), float.Parse(eventParams[3]));
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
			{
				return;
			}
			if (msgType == "button_clicked")
			{
				if (panelName == StationPlacementState.UICancelButton)
				{
					base.App.GetGameState<StarMapState>().ShowInterface = true;
					base.App.GetGameState<StarMapState>().RightClickEnabled = true;
					base.App.UI.Send(new object[]
					{
						"SetWidthProp",
						"OH_StarMap",
						"parent:width"
					});
					base.App.SwitchGameState<StarMapState>(new object[0]);
					return;
				}
				if (panelName == StationPlacementState.UICommitButton)
				{
					OverlayConstructionMission.OnConstructionPlaced(this._sim, this._selectedFleetID, this._targetSystemID, this._useDirectRoute, this._selectedPlanetID, this._designsToBuild, this._stationType, this._rebase, true);
					base.App.GetGameState<StarMapState>().ShowInterface = true;
					base.App.GetGameState<StarMapState>().RightClickEnabled = true;
					base.App.UI.Send(new object[]
					{
						"SetWidthProp",
						"OH_StarMap",
						"parent:width"
					});
					base.App.SwitchGameState<StarMapState>(new object[0]);
				}
			}
		}
	}
}
