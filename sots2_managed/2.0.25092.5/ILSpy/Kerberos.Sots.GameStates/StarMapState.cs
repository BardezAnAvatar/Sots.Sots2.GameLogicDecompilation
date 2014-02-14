using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using Kerberos.Sots.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class StarMapState : GameState, IKeyBindListener
	{
		public delegate void ObjectSelectionChangedDelegate(App app, int SelectedObject);
		public enum StarMapRefreshType
		{
			REFRESH_NORMAL,
			REFRESH_ALL
		}
		public const string StarSystemPopupPanelID = "StarSystemPopup";
		private const string UISystemDetailsWidget = "systemDetailsWidget";
		private const string UIPlanetDetailsWidget = "planetDetailsWidget";
		private const string UIFleetAndPlanetDetailsWidget = "fleetAndPlanetDetailsWidget";
		public const string UITurnCount = "turn_count";
		private const string DebugTestCombatButton = "debugTestCombatButton";
		private const string UIEmpireBar = "gameEmpireBar";
		private const string UIColonyDetailsWidget = "colonyDetailsWidget";
		private const string UIColonyEstablished = "colony_event_dialog";
		private const string UIExitButton = "gameExitButton";
		private const string UIOptionsButton = "gameOptionsButton";
		private const string UISaveGameButton = "gameSaveGameButton";
		private const string UIEndTurnButton = "gameEndTurnButton";
		private const string UIEmpireSummaryButton = "gameEmpireSummaryButton";
		private const string UIResearchButton = "gameResearchButton";
		private const string UIDiplomacyButton = "gameDiplomacyButton";
		private const string UIAbandonColony = "btnAbandon";
		private const string UIHardenStructuresButton = "partHardenedStructure";
		private const string UIDesignButton = "gameDesignButton";
		private const string UIRepairButton = "gameRepairButton";
		private const string UIBuildButton = "gameBuildButton";
		private const string UISystemButton = "gameSystemButton";
		private const string UISotspediaButton = "gameSotspediaButton";
		private const string UIProvinceModeButton = "gameProvinceModeButton";
		private const string UIEventHistoryButton = "gameEventHistoryButton";
		private const string UIBattleRiderManagerButton = "gameBattleRiderManagerButton";
		private const string UITutorialButton = "gameTutorialButton";
		private const string UICloseTutorialButton = "starMapTutImage";
		private const string UIOpenSystemButton = "btnSystemOpen";
		private const string UIEventImageButton = "btnturnEventImageButton";
		private const string UIFleetCancelMissionButton = "fleetCancelButton";
		private const string UIFleetInterceptButton = "fleetInterceptButton";
		private const string UIFleetBuildStationButton = "fleetBuildStationButton";
		private const string UISurveyButton = "gameSurveyButton";
		private const string UIColonizeButton = "gameColonizeButton";
		private const string UIEvacuateButton = "gameEvacuateButton";
		private const string UIRelocateButton = "gameRelocateButton";
		private const string UIPatrolButton = "gamePatrolButton";
		private const string UIInterdictButton = "gameInterdictButton";
		private const string UIStrikeButton = "gameStrikeButton";
		private const string UIInvadeButton = "gameInvadeButton";
		private const string UISupportButton = "gameSupportButton";
		private const string UIConstructStationButton = "gameConstructStationButton";
		private const string UIUpgradeStationButton = "gameUpgradeStationButton";
		private const string UIStationManagerButton = "gameStationManagerButton";
		private const string UIContextStationManagerButton = "gameContextStationManagerButton";
		private const string UIPlanetManagerButton = "gamePlanetSummaryButton";
		private const string UIPopulationManagerButton = "gamePopulationManagerButton";
		private const string UIComparativeAnalysysButton = "gameComparativeAnalysysButton";
		private const string UIFleetSummaryButton = "gameFleetSummaryButton";
		private const string UIFleetManagerButton = "gameFleetManagerButton";
		private const string UIContextFleetManagerButton = "gameContextFleetManagerButton";
		private const string UIDefenseManagerButton = "gameDefenseManagerButton";
		private const string UIGateButton = "gameGateButton";
		private const string UIPiracyButton = "gamePiracyButton";
		private const string UINPGButton = "gameNPGButton";
		private StarMapStateMode _mode;
		private GameObjectSet _crits;
		private ArrowPainter _painter;
		private Sky _sky;
		private StarMap _starmap;
		private PlanetView _planetView;
		private StellarBody _cachedPlanet;
		private PlanetInfo _cachedPlanetInfo;
		private bool _cachedPlanetReady;
		private StarModel _cachedStar;
		private StarSystemInfo _cachedStarInfo;
		private bool _cachedStarReady;
		private string _contextMenuID;
		private string _researchContextID;
		private string _fleetContextMenuID;
		private string _enemyContextMenuID;
		private string _enemyGMStationContextMenuID;
		private bool _isProvinceMode;
		private FleetWidget _fleetWidget;
		private BudgetPiechart _piechart;
		private ColonizeDialog _colonizeDialog;
		private ColonySelfSufficientDialog _selfSufficientDialog;
		private RequestRequestedDialog _requestRequestedDialog;
		private DemandRequestedDialog _demandRequestedDialog;
		private Dialog _treatyRequestedDialog;
		private GenericTextDialog _requestAcceptedDialog;
		private GenericTextDialog _requestDeclinedDialog;
		private GenericTextDialog _demandAcceptedDialog;
		private GenericTextDialog _demandDeclinedDialog;
		private GenericTextDialog _treatyAcceptedDialog;
		private GenericTextDialog _treatyDeclinedDialog;
		private GenericTextDialog _treatyExpiredDialog;
		private LimitationTreatyBrokenDialog _treatyBrokenDialogOffender;
		private LimitationTreatyBrokenDialog _treatyBrokenDialogVictim;
		private bool _eventDialogShown;
		private PlayerWidget _playerWidget;
		private StationBuiltDialog _stationDialog;
		internal StarMapViewFilter _lastFilterSelection;
		private bool _uiEnabled = true;
		private bool _showInterface = true;
		private int _simNewTurnTick;
		private TurnEvent _currentEvent;
		private StarMapSystem _contextsystem;
		private bool _rightClickEnabled = true;
		private PlanetWidget _planetWidget;
		private TechCube _techCube;
		private string _surveyDialog;
		private string _researchCompleteDialog;
		private string _feasibilityCompleteDialog;
		private string _superWorldDialog;
		private string _confirmAbandon;
		private string _suulkaArrivalDialog;
		private string _endTurnConfirmDialog;
		public StarMapState.ObjectSelectionChangedDelegate OnObjectSelectionChanged;
		private ESMDialogState _dialogState;
		private string _enteredColonyName;
		private int _selectedPlanet;
		private int _colonyEstablishedPlanet;
		private int _colonyEstablishedSystem;
		private int _contextMenuSystem;
		private int _fleetContextFleet;
		private int _lastSelectedFleet;
		private string DeleteFrieghterConfirm;
		private int _selectedfreighter;
		private int _prevNumStations;
		private int _TurnLastUpdated = -1;
		private Dictionary<string, OverlayMission> _missionOverlays;
		private OverlayMission _reactionOverlay;
		private bool _initialized;
		public bool EnableFleetCheck
		{
			get;
			set;
		}
		public bool ShowInterface
		{
			get
			{
				return this._showInterface;
			}
			set
			{
				if (this._showInterface == value)
				{
					return;
				}
				this._showInterface = value;
				base.App.UI.SetVisible("bottomBarWidget", this._showInterface);
				base.App.UI.SetVisible("topLeftWidget", this._showInterface);
				base.App.UI.SetVisible("leftSideWidget", this._showInterface);
				if (this._showInterface)
				{
					this.RefreshSystemInterface();
				}
			}
		}
		public StarMap StarMap
		{
			get
			{
				return this._starmap;
			}
		}
		public StarMapSystem ContextSystem
		{
			get
			{
				return this._contextsystem;
			}
			set
			{
			}
		}
		public bool RightClickEnabled
		{
			get
			{
				return this._rightClickEnabled;
			}
			set
			{
				if (this._rightClickEnabled == value)
				{
					return;
				}
				this._rightClickEnabled = value;
			}
		}
		private string EnteredColonyName
		{
			get
			{
				return this._enteredColonyName;
			}
		}
		private int SelectedPlanet
		{
			get
			{
				return this._selectedPlanet;
			}
			set
			{
				this._selectedPlanet = value;
			}
		}
		private int ColonyEstablishedPlanet
		{
			get
			{
				return this._colonyEstablishedPlanet;
			}
			set
			{
				this._colonyEstablishedPlanet = value;
			}
		}
		private int ColonyEstablishedSystem
		{
			get
			{
				return this._colonyEstablishedSystem;
			}
			set
			{
				this._colonyEstablishedSystem = value;
			}
		}
		private int SelectedSystem
		{
			get
			{
				StarSystemInfo starSystemInfo = this.SelectedObject as StarSystemInfo;
				if (!(starSystemInfo != null))
				{
					return 0;
				}
				return starSystemInfo.ID;
			}
		}
		private int SelectedFleet
		{
			get
			{
				FleetInfo fleetInfo = this.SelectedObject as FleetInfo;
				if (fleetInfo == null)
				{
					return 0;
				}
				return fleetInfo.ID;
			}
		}
		private object SelectedObject
		{
			get
			{
				return base.App.Game.StarMapSelectedObject;
			}
			set
			{
				if (base.App.Game.StarMapSelectedObject == value)
				{
					return;
				}
				this._selectedPlanet = 0;
				base.App.Game.StarMapSelectedObject = value;
				if (this.OnObjectSelectionChanged != null)
				{
					StarSystemInfo starSystemInfo = this.SelectedObject as StarSystemInfo;
					this.OnObjectSelectionChanged(base.App, (starSystemInfo != null) ? starSystemInfo.ID : 0);
				}
				this.RefreshSystemInterface();
			}
		}
		public void ShowOverlay(MissionType missionType, int targetSystem)
		{
			OverlayMission overlayMission = this._missionOverlays.Values.FirstOrDefault((OverlayMission x) => x.MissionType == missionType);
			if (overlayMission != null)
			{
				overlayMission.Show(targetSystem);
			}
		}
		public void ShowColonizePlanetOverlay(int targetSystem, int targetPlanet)
		{
			OverlayColonizeMission overlayColonizeMission = (OverlayColonizeMission)this._missionOverlays.Values.FirstOrDefault((OverlayMission x) => x.MissionType == MissionType.COLONIZATION);
			if (overlayColonizeMission != null)
			{
				overlayColonizeMission.SetSelectedPlanet(targetPlanet);
				overlayColonizeMission.Show(targetSystem);
			}
		}
		public void ShowFleetCentricOverlay(MissionType missionType, int fleetid)
		{
			OverlayMission overlayMission = this._missionOverlays.Values.FirstOrDefault((OverlayMission x) => x.MissionType == missionType);
			if (overlayMission != null)
			{
				overlayMission.ShowFleetCentric(fleetid);
			}
		}
		public void ShowReactionOverlay(int targetsystem)
		{
			this._reactionOverlay.Show(targetsystem);
		}
		public bool OverlayActive()
		{
			foreach (OverlayMission current in this._missionOverlays.Values)
			{
				if (current.GetShown())
				{
					return true;
				}
			}
			return this._reactionOverlay.GetShown();
		}
		public T GetOverlay<T>() where T : OverlayMission
		{
			return this._missionOverlays.Values.FirstOrDefault((OverlayMission x) => x is T) as T;
		}
		internal void SyncFreighterInterface()
		{
			base.App.UI.ClearSelection("freighterList");
			List<FreighterInfo> list = (
				from x in base.App.GameDatabase.GetFreighterInfosForSystem(this.SelectedSystem)
				where x.PlayerId == base.App.LocalPlayer.ID && x.IsPlayerBuilt
				select x).ToList<FreighterInfo>();
			List<BuildOrderInfo> list2 = base.App.GameDatabase.GetBuildOrdersForSystem(this.SelectedSystem).Where(delegate(BuildOrderInfo x)
			{
				if (base.App.GameDatabase.GetDesignInfo(x.DesignID).PlayerID == base.App.LocalPlayer.ID)
				{
					return base.App.GameDatabase.GetDesignInfo(x.DesignID).DesignSections.Any((DesignSectionInfo j) => j.ShipSectionAsset.FreighterSpace > 0);
				}
				return false;
			}).ToList<BuildOrderInfo>();
			base.App.UI.ClearItems("freighterList");
			foreach (FreighterInfo current in list)
			{
				base.App.UI.AddItem("freighterList", "", current.ShipId, "");
				string itemGlobalID = base.App.UI.GetItemGlobalID("freighterList", "", current.ShipId, "");
				base.App.UI.SetText(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"designName"
				}), current.Design.Name);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"designDeleteButton"
				}), true);
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"designDeleteButton"
				}), "id", "freighterdeletebtn|" + current.ShipId.ToString());
				base.App.UI.SetItemPropertyColor("freighterList", string.Empty, current.ShipId, "designName", "color", new Vector3(11f, 157f, 194f));
			}
			int num = 1000000;
			foreach (BuildOrderInfo current2 in list2)
			{
				base.App.UI.AddItem("freighterList", "", num, "");
				string itemGlobalID2 = base.App.UI.GetItemGlobalID("freighterList", "", num, "");
				base.App.UI.SetText(base.App.UI.Path(new string[]
				{
					itemGlobalID2,
					"designName"
				}), base.App.GameDatabase.GetDesignInfo(current2.DesignID).Name);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					itemGlobalID2,
					"designDeleteButton"
				}), false);
				base.App.UI.SetItemPropertyColor("freighterList", string.Empty, num, "designName", "color", new Vector3(200f, 150f, 40f));
				num++;
			}
		}
		internal void RefreshSystemInterface()
		{
			this.OnFleetWidgetFleetSelected(base.App, 0);
			if (!this._showInterface)
			{
				return;
			}
			base.App.UI.ClearSelection("freighterList");
			if (this._lastFilterSelection == StarMapViewFilter.VF_TRADE)
			{
				this.SyncFreighterInterface();
				base.App.UI.SetVisible("fleetAndPlanetDetailsWidget", false);
				base.App.UI.SetVisible("colonyDetailsWidget", false);
				base.App.UI.SetVisible("planetDetailsWidget", false);
				base.App.UI.SetVisible("systemDetailsWidget", false);
				IEnumerable<ColonyInfo> enumerable = 
					from x in base.App.GameDatabase.GetColonyInfosForSystem(this.SelectedSystem)
					where x.PlayerID == base.App.LocalPlayer.ID
					select x;
				if (enumerable.Count<ColonyInfo>() > 0)
				{
					base.App.UI.SetVisible("tradePopup", true);
					base.App.UI.ClearItems("tradePlanetList");
					using (IEnumerator<ColonyInfo> enumerator = enumerable.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ColonyInfo current = enumerator.Current;
							OrbitalObjectInfo orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(current.OrbitalObjectID);
							if (orbitalObjectInfo != null)
							{
								base.App.UI.AddItem("tradePlanetList", "", current.ID, "");
								string itemGlobalID = base.App.UI.GetItemGlobalID("tradePlanetList", "", current.ID, "");
								FleetUI.SyncExistingPlanet(base.App.Game, base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"planetCard"
								}), orbitalObjectInfo);
								base.App.UI.SetSliderValue(base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"partTradeSlider"
								}), (int)(current.TradeRate * 100f));
								base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"partTradeSlider"
								}), "id", "meTradeSlider|" + current.OrbitalObjectID.ToString());
								base.App.UI.ClearSliderNotches(base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"meTradeSlider|" + current.OrbitalObjectID.ToString()
								}));
								foreach (double num in base.App.Game.GetTradeRatesForWholeExportsForColony(current.ID))
								{
									base.App.UI.AddSliderNotch(base.App.UI.Path(new string[]
									{
										itemGlobalID,
										"meTradeSlider|" + current.OrbitalObjectID.ToString()
									}), (int)Math.Ceiling(num * 100.0));
								}
								base.App.UI.ForceLayout("tradePopup");
								base.App.UI.ForceLayout("tradePopup");
							}
						}
						goto IL_621;
					}
				}
				base.App.UI.SetVisible("tradePopup", false);
			}
			else
			{
				if (this.SelectedFleet != 0)
				{
					base.App.UI.SetVisible("colonyDetailsWidget", false);
					base.App.UI.SetVisible("planetDetailsWidget", false);
					base.App.UI.SetVisible("systemDetailsWidget", false);
					base.App.UI.SetEnabled("planetsTab", false);
					base.App.UI.SetVisible("partSystemPlanets", false);
					base.App.UI.SetChecked("planetsTab", false);
					base.App.UI.SetChecked("fleetsTab", true);
					base.App.UI.SetVisible("partSystemFleets", true);
					base.App.UI.SetVisible("fleetAndPlanetDetailsWidget", true);
					this._fleetWidget.ListStations = false;
					this._fleetWidget.SetSyncedStations(new List<StationInfo>());
					this._fleetWidget.SetSyncedFleets(this.SelectedFleet);
				}
				else
				{
					StarSystemUI.SyncSystemDetailsWidget(base.App, "systemDetailsWidget", this.SelectedSystem, true, true);
					StarSystemUI.SyncPlanetDetailsWidget(base.App.Game, "planetDetailsWidget", this.SelectedSystem, this.SelectedPlanet, this.GetPlanetViewGameObject(this.SelectedSystem, this.SelectedPlanet), this._planetView);
					StarSystemUI.SyncColonyDetailsWidget(base.App.Game, "colonyDetailsWidget", this.SelectedPlanet, "");
					FleetUI.SyncFleetAndPlanetListWidget(base.App.Game, "fleetAndPlanetDetailsWidget", this.SelectedSystem, true);
					base.App.UI.SetVisible("planetDetailsWidget", true);
					if (this.SelectedSystem != 0 && this.IsSystemVisible(this.SelectedSystem))
					{
						this._fleetWidget.SetSyncedFleets((
							from x in base.App.GameDatabase.GetFleetInfoBySystemID(this.SelectedSystem, FleetType.FL_ALL)
							where x.Type != FleetType.FL_RESERVE || x.PlayerID == base.App.LocalPlayer.ID
							select x).ToList<FleetInfo>());
						this._fleetWidget.ListStations = true;
						this._fleetWidget.SetSyncedStations(base.App.GameDatabase.GetStationForSystemAndPlayer(this.SelectedSystem, base.App.LocalPlayer.ID).ToList<StationInfo>());
					}
					else
					{
						this._fleetWidget.SetSyncedFleets(new List<int>());
						this._fleetWidget.ListStations = true;
						this._fleetWidget.SetSyncedStations(new List<int>());
					}
				}
				base.App.UI.SetVisible("tradePopup", false);
			}
			IL_621:
			base.App.UI.SetVisible("gameComparativeAnalysysButton", base.App.GameDatabase.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, base.App.LocalPlayer.ID));
			bool arg_6C7_0;
			if (base.App.GameDatabase.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, base.App.LocalPlayer.ID))
			{
				arg_6C7_0 = base.App.GameDatabase.GetDesignsEncountered(base.App.LocalPlayer.ID).Any((DesignInfo x) => x.Class != ShipClass.Station);
			}
			else
			{
				arg_6C7_0 = false;
			}
			bool flag = arg_6C7_0;
			base.App.UI.SetEnabled("gameComparativeAnalysysButton", flag);
			base.App.UI.SetPropertyBool("gameComparativeAnalysysButton", "lockout_button", flag);
			base.App.UI.SetVisible("gameBattleRiderManagerButton", base.App.GameDatabase.PlayerHasTech(base.App.LocalPlayer.ID, "BRD_BattleRiders"));
			if (this.SelectedSystem != 0)
			{
				this._fleetWidget.ListStations = (this.SelectedFleet == 0);
				List<StationInfo> syncedStations = new List<StationInfo>();
				if (this._fleetWidget.ListStations)
				{
					syncedStations = base.App.GameDatabase.GetStationForSystemAndPlayer(this.SelectedSystem, base.App.LocalPlayer.ID).ToList<StationInfo>();
				}
				this._fleetWidget.SetSyncedStations(syncedStations);
			}
		}
		private bool IsSystemVisible(int systemId)
		{
			bool flag = base.App.GameDatabase.IsSurveyed(base.App.LocalPlayer.ID, systemId);
			List<FleetInfo> list = base.App.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_GATE).ToList<FleetInfo>();
			List<StationInfo> list2 = base.App.GameDatabase.GetStationForSystemAndPlayer(systemId, base.App.LocalPlayer.ID).ToList<StationInfo>();
			bool flag2 = true;
			if (!base.App.Game.SystemHasPlayerColony(systemId, base.App.LocalPlayer.ID) && !StarMap.IsInRange(base.App.GameDatabase, base.App.LocalPlayer.ID, base.App.GameDatabase.GetStarSystemInfo(systemId), null))
			{
				flag2 = false;
			}
			return (list.Count != 0 || list2.Count != 0 || flag) && (flag2 || flag);
		}
		public int GetSelectedSystem()
		{
			StarSystemInfo starSystemInfo = this.SelectedObject as StarSystemInfo;
			if (!(starSystemInfo != null))
			{
				return 0;
			}
			return starSystemInfo.ID;
		}
		public void SetSelectedSystem(int systemId, bool fleetTab = false)
		{
			if (this._starmap.Systems.Reverse.ContainsKey(systemId))
			{
				StarMapSystem starMapSystem = this._starmap.Systems.Reverse[systemId];
				this.SelectObject(starMapSystem);
				this.StarMap.SetFocus(starMapSystem);
				StarSystemUI.SyncColonyDetailsWidget(base.App.Game, "colonyDetailsWidget", this._selectedPlanet, "");
				if (fleetTab)
				{
					base.App.UI.SetVisible("partSystemPlanets", !FleetUI.ShowFleetListDefault);
					base.App.UI.SetChecked("planetsTab", !FleetUI.ShowFleetListDefault);
					base.App.UI.SetChecked("fleetsTab", FleetUI.ShowFleetListDefault);
					base.App.UI.SetVisible("partSystemFleets", FleetUI.ShowFleetListDefault);
				}
			}
		}
		public void SetSelectedFleet(int fleetId)
		{
			if (this._starmap.Fleets.Reverse.ContainsKey(fleetId))
			{
				StarMapFleet starMapFleet = this._starmap.Fleets.Reverse[fleetId];
				this.StarMap.SetFocus(starMapFleet);
				this.StarMap.Select(starMapFleet);
			}
		}
		public static bool Load(App game)
		{
			return game.SwitchGameStateViaLoadingScreen(null, null, game.GetGameState<StarMapState>(), null);
		}
		public StarMapState(App game) : base(game)
		{
		}
		public void ClearStarmapFleetArrows()
		{
			this._painter.ClearSections();
		}
		public void AddMissionFleetArrow(Vector3 Origin, Vector3 Target, Vector3 Color)
		{
			List<Vector3> list = new List<Vector3>();
			list.Add(Origin);
			list.Add(Target);
			this._painter.AddSection(list, APStyle.FLEET_ARROW, 0, new Vector3?(Color));
		}
		public void AddMissionFleetArrow(List<Vector3> path, Vector3 Color)
		{
			this._painter.AddSection(path, APStyle.FLEET_ARROW, 0, new Vector3?(Color));
		}
		protected void SyncFleetArrows()
		{
			this._painter.ClearSections();
			bool flag = base.App.GameDatabase.PlayerHasTech(base.App.LocalPlayer.ID, "CCC_Node_Tracking:_Zuul");
			bool flag2 = base.App.GameDatabase.PlayerHasTech(base.App.LocalPlayer.ID, "CCC_Node_Tracking:_Human");
			IEnumerable<NodeLineInfo> nodeLines = base.App.GameDatabase.GetNodeLines();
			IEnumerable<NodeLineInfo> exploredNodeLines = base.App.GameDatabase.GetExploredNodeLines(base.App.LocalPlayer.ID);
			foreach (NodeLineInfo nodeLine in nodeLines)
			{
				if (!nodeLine.IsLoaLine)
				{
					bool flag3 = false;
					if ((base.App.LocalPlayer.Faction.Name != "zuul" && flag) || (base.App.LocalPlayer.Faction.Name != "human" && flag2))
					{
						List<MoveOrderInfo> moveOrdersBetweenSystems = base.App.Game.GetMoveOrdersBetweenSystems(nodeLine.System1ID, nodeLine.System2ID);
						foreach (MoveOrderInfo current in moveOrdersBetweenSystems)
						{
							FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(current.FleetID);
							if (fleetInfo.PlayerID != base.App.LocalPlayer.ID && StarMap.IsInRange(base.App.Game.GameDatabase, base.App.LocalPlayer.ID, base.App.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords, 1f, null))
							{
								flag3 = true;
								break;
							}
						}
					}
					StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(nodeLine.System1ID);
					StarSystemInfo starSystemInfo2 = base.App.GameDatabase.GetStarSystemInfo(nodeLine.System2ID);
					if (!nodeLine.IsPermenant)
					{
						if ((flag3 && flag) || base.App.LocalPlayer.Faction.Name == "zuul")
						{
							List<Vector3> list = new List<Vector3>();
							list.Add(starSystemInfo.Origin);
							list.Add(starSystemInfo2.Origin);
							this._painter.AddSection(list, APStyle.BORE_LINE, nodeLine.Health, null);
						}
					}
					else
					{
						if (!flag3 || !flag2)
						{
							if (!(base.App.LocalPlayer.Faction.Name == "human"))
							{
								continue;
							}
							if (!exploredNodeLines.Any((NodeLineInfo x) => x.ID == nodeLine.ID && x.Health == -1))
							{
								continue;
							}
						}
						List<Vector3> list2 = new List<Vector3>();
						list2.Add(starSystemInfo.Origin);
						list2.Add(starSystemInfo2.Origin);
						this._painter.AddSection(list2, APStyle.NODE_LINE, nodeLine.Health, null);
					}
				}
			}
			IEnumerable<FleetInfo> enumerable = base.App.GameDatabase.GetFleetInfosByPlayerID(base.App.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_CARAVAN);
			foreach (FleetInfo current2 in enumerable)
			{
				MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(current2.ID);
				if (missionByFleetID != null && (missionByFleetID.Type != MissionType.PIRACY || base.App.GameDatabase.PirateFleetVisibleToPlayer(current2.ID, base.App.LocalPlayer.ID)))
				{
					FleetLocation fleetLocation = base.App.GameDatabase.GetFleetLocation(current2.ID, true);
					if (GameSession.FleetHasBore(base.App.GameDatabase, current2.ID) && base.App.LocalPlayer.Faction.CanUseNodeLine(new bool?(false)))
					{
						List<Vector3> list3 = new List<Vector3>();
						MoveOrderInfo moveOrderInfoByFleetID = base.App.GameDatabase.GetMoveOrderInfoByFleetID(current2.ID);
						if (moveOrderInfoByFleetID != null && moveOrderInfoByFleetID.FromSystemID != 0 && moveOrderInfoByFleetID.ToSystemID != 0 && !GameSession.SystemsLinkedByNonPermenantNodes(base.App.Game, moveOrderInfoByFleetID.FromSystemID, moveOrderInfoByFleetID.ToSystemID))
						{
							StarSystemInfo starSystemInfo3 = base.App.GameDatabase.GetStarSystemInfo(moveOrderInfoByFleetID.FromSystemID);
							if (!starSystemInfo3.IsDeepSpace)
							{
								list3.Add(base.App.GameDatabase.GetStarSystemOrigin(moveOrderInfoByFleetID.FromSystemID));
								list3.Add(fleetLocation.Coords);
								this._painter.AddSection(list3, APStyle.BORE_LINE, 1000, null);
							}
						}
					}
					base.App.GameDatabase.GetNodeLines();
					WaypointInfo nextWaypointForMission = base.App.GameDatabase.GetNextWaypointForMission(missionByFleetID.ID);
					if (nextWaypointForMission != null)
					{
						PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
						if ((missionByFleetID.Type == MissionType.INTERCEPT || missionByFleetID.Type == MissionType.SPECIAL_CONSTRUCT_STN) && nextWaypointForMission.Type == WaypointType.DoMission && missionByFleetID.TargetFleetID != 0)
						{
							List<Vector3> list4 = new List<Vector3>();
							list4.Add(base.App.GameDatabase.GetFleetLocation(current2.ID, true).Coords);
							list4.Add(base.App.GameDatabase.GetFleetLocation(missionByFleetID.TargetFleetID, true).Coords);
							if (StarMap.IsInRange(base.App.GameDatabase, current2.PlayerID, list4[1], 1f, null))
							{
								this._painter.AddSection(list4, APStyle.FLEET_ARROW, 0, new Vector3?(playerInfo.PrimaryColor));
							}
						}
						else
						{
							if (nextWaypointForMission.Type == WaypointType.TravelTo || nextWaypointForMission.Type == WaypointType.ReturnHome)
							{
								List<Vector3> list5 = new List<Vector3>();
								int num = nextWaypointForMission.SystemID.HasValue ? nextWaypointForMission.SystemID.Value : base.App.Game.GameDatabase.GetHomeSystem(base.App.Game, nextWaypointForMission.MissionID, current2);
								int num2;
								if (current2.SystemID == 0)
								{
									MoveOrderInfo moveOrderInfoByFleetID2 = base.App.GameDatabase.GetMoveOrderInfoByFleetID(current2.ID);
									list5.Add(fleetLocation.Coords);
									if (moveOrderInfoByFleetID2.ToSystemID == 0)
									{
										list5.Add(moveOrderInfoByFleetID2.ToCoords);
									}
									else
									{
										list5.Add(base.App.GameDatabase.GetStarSystemOrigin(moveOrderInfoByFleetID2.ToSystemID));
									}
									num2 = moveOrderInfoByFleetID2.ToSystemID;
								}
								else
								{
									num2 = current2.SystemID;
								}
								if (num2 != num && num2 != 0)
								{
									int num3;
									float num4;
                                    List<int> bestTravelPath = Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(base.App.Game, current2.ID, num2, num, out num3, out num4, missionByFleetID.UseDirectRoute, null, null);
									foreach (int current3 in bestTravelPath)
									{
										list5.Add(base.App.GameDatabase.GetStarSystemOrigin(current3));
									}
								}
								this._painter.AddSection(list5, APStyle.FLEET_ARROW, 0, new Vector3?(playerInfo.PrimaryColor));
							}
						}
					}
				}
			}
			enumerable = (
				from x in base.App.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL).ToList<FleetInfo>()
				where x.PlayerID != base.App.LocalPlayer.ID && StarMap.IsInRange(base.App.GameDatabase, base.App.LocalPlayer.ID, base.App.GameDatabase.GetFleetLocation(x.ID, false).Coords, 1f, null)
				select x).ToList<FleetInfo>();
			foreach (FleetInfo current4 in enumerable)
			{
				MissionInfo missionByFleetID2 = base.App.GameDatabase.GetMissionByFleetID(current4.ID);
				if (missionByFleetID2 == null || missionByFleetID2.Type != MissionType.PIRACY || base.App.GameDatabase.PirateFleetVisibleToPlayer(current4.ID, base.App.LocalPlayer.ID))
				{
					MoveOrderInfo moveOrderInfoByFleetID3 = base.App.GameDatabase.GetMoveOrderInfoByFleetID(current4.ID);
					PlayerInfo playerInfo2 = base.App.GameDatabase.GetPlayerInfo(current4.PlayerID);
					if (moveOrderInfoByFleetID3 != null)
					{
						if (moveOrderInfoByFleetID3.ToSystemID != 0)
						{
							if (StarMap.IsInRange(base.App.GameDatabase, base.App.LocalPlayer.ID, moveOrderInfoByFleetID3.ToSystemID))
							{
								this._painter.AddSection(new List<Vector3>
								{
									base.App.GameDatabase.GetFleetLocation(current4.ID, true).Coords,
									base.App.GameDatabase.GetStarSystemOrigin(moveOrderInfoByFleetID3.ToSystemID)
								}, APStyle.FLEET_ARROW, 0, new Vector3?(playerInfo2.PrimaryColor));
							}
						}
						else
						{
							if (StarMap.IsInRange(base.App.GameDatabase, base.App.LocalPlayer.ID, moveOrderInfoByFleetID3.ToCoords, 1f, null))
							{
								this._painter.AddSection(new List<Vector3>
								{
									base.App.GameDatabase.GetFleetLocation(current4.ID, true).Coords,
									moveOrderInfoByFleetID3.ToCoords
								}, APStyle.FLEET_ARROW, 0, new Vector3?(playerInfo2.PrimaryColor));
							}
						}
					}
				}
			}
		}
		public void RefreshCameraControl()
		{
			this._starmap.SetCamera(base.App.Game.StarMapCamera);
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (prev == base.App.GetGameState<LoadingScreenState>())
			{
				prev = base.App.GetGameState<LoadingScreenState>().PreviousState;
			}
			if (this._initialized && (prev == base.App.GetGameState<MainMenuState>() || prev == base.App.GetGameState<StarMapLobbyState>() || prev == null))
			{
				this.Reset();
			}
			this._simNewTurnTick = 200;
			if (!this._initialized)
			{
				if (base.App.GameDatabase == null)
				{
					base.App.NewGame();
				}
				this._crits = new GameObjectSet(base.App);
				this._sky = new Sky(base.App, SkyUsage.StarMap, 0);
				this._crits.Add(this._sky);
				this._planetView = this._crits.Add<PlanetView>(new object[0]);
				this._starmap = new StarMap(base.App, base.App.Game, this._sky);
				this._starmap.SetCamera(base.App.Game.StarMapCamera);
				this._contextMenuID = base.App.UI.CreatePanelFromTemplate("StarMapContextMenu", null);
				this._researchContextID = base.App.UI.CreatePanelFromTemplate("ResearchContextMenu", null);
				this._fleetContextMenuID = base.App.UI.CreatePanelFromTemplate("StarMapFleetContextMenu", null);
				this._enemyContextMenuID = base.App.UI.CreatePanelFromTemplate("StarMapEnemyContextMenu", null);
				this._enemyGMStationContextMenuID = base.App.UI.CreatePanelFromTemplate("StarMapGMStationTargetContextMenu", null);
				this._painter = new ArrowPainter(base.App);
				this._contextsystem = null;
				this.SyncFleetArrows();
				base.App.UI.LoadScreen("StarMap");
				this._starmap.Initialize(this._crits, new object[0]);
				this._crits.Add(this._starmap);
				this._dialogState = ESMDialogState.ESMD_None;
				this._fleetWidget = new FleetWidget(base.App, "StarMap.partSystemFleets");
				this._fleetWidget.EnableCreateFleetButton = true;
				this._fleetWidget.ScrapEnabled = true;
				this._fleetWidget.ShipSelectionEnabled = false;
				this._fleetWidget.SeparateDefenseFleet = false;
				this._fleetWidget.EnableMissionButtons = true;
				FleetWidget expr_24A = this._fleetWidget;
				expr_24A.OnFleetSelectionChanged = (FleetWidget.FleetSelectionChangedDelegate)Delegate.Combine(expr_24A.OnFleetSelectionChanged, new FleetWidget.FleetSelectionChangedDelegate(this.OnFleetWidgetFleetSelected));
				this._playerWidget = new PlayerWidget(base.App, base.App.UI, "playerDropdown");
				this._piechart = new BudgetPiechart(base.App.UI, "piechart", base.App.AssetDatabase);
				this._planetWidget = new PlanetWidget(base.App, "systemDetailsWidget");
				this._techCube = new TechCube(base.App);
				this._crits.Add(this._techCube);
				this._missionOverlays = new Dictionary<string, OverlayMission>();
				this._missionOverlays["gameSurveyButton"] = new OverlaySurveyMission(base.App, this, this._starmap, "OverlaySurveyMission");
				this._missionOverlays["gameColonizeButton"] = new OverlayColonizeMission(base.App, this, this._starmap, "OverlayColonizeMission");
				this._missionOverlays["gameEvacuateButton"] = new OverlayEvacuationMission(base.App, this, this._starmap, "OverlayEvacuationMission");
				this._missionOverlays["gameStrikeButton"] = new OverlayStrikeMission(base.App, this, this._starmap, "OverlaySurveyMission");
				this._missionOverlays["gameRelocateButton"] = new OverlayRelocateMission(base.App, this, this._starmap, "OverlayRelocateMission");
				this._missionOverlays["gameGateButton"] = new OverlayGateMission(base.App, this, this._starmap, "OverlaySurveyMission");
				this._missionOverlays["gamePatrolButton"] = new OverlayPatrolMission(base.App, this, this._starmap, "OverlaySurveyMission");
				this._missionOverlays["gameInterdictButton"] = new OverlayInterdictMission(base.App, this, this._starmap, "OverlaySurveyMission");
				this._missionOverlays["fleetInterceptButton"] = new OverlayInterceptMission(base.App, this, this._starmap, "OverlayInterceptMission");
				this._missionOverlays["fleetBuildStationButton"] = new OverlaySpecialConstructionMission(base.App, this, this._starmap, null, "OverlayGMBuildStationMission");
				this._missionOverlays["gameInvadeButton"] = new OverlayInvasionMission(base.App, this, this._starmap, "OverlayColonizeMission");
				this._missionOverlays["gameSupportButton"] = new OverlaySupportMission(base.App, this, this._starmap, "OverlaySupportMission");
				this._missionOverlays["gameConstructStationButton"] = new OverlayConstructionMission(base.App, this, this._starmap, null, "OverlayStationMission", MissionType.CONSTRUCT_STN);
				this._missionOverlays["gameUpgradeStationButton"] = new OverlayUpgradeMission(base.App, this, this._starmap);
				this._missionOverlays["gamePiracyButton"] = new OverlayPiracyMission(base.App, this, this._starmap, "OverlaySurveyMission");
				this._missionOverlays["gameNPGButton"] = new OverlayDeployAccelMission(base.App, this, this._starmap, "OverlayAcceleratorMission");
				this._reactionOverlay = new OverlayReactionlMission(base.App, this, this._starmap, "OverlayReaction");
				this._initialized = true;
			}
		}
		public void OnFleetWidgetFleetSelected(App game, int selectedFleet)
		{
			if (this._lastSelectedFleet == selectedFleet)
			{
				return;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(selectedFleet);
			if (fleetInfo != null && fleetInfo.PlayerID == game.LocalPlayer.ID && fleetInfo.Type == FleetType.FL_NORMAL)
			{
                List<int> list = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.SURVEY, false).ToList<int>();
				using (Dictionary<int, StarMapSystem>.KeyCollection.Enumerator enumerator = this._starmap.Systems.Reverse.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						int current = enumerator.Current;
						this._starmap.Systems.Reverse[current].SetIsEnabled(list.Contains(current));
					}
					goto IL_124;
				}
			}
			foreach (int current2 in this._starmap.Systems.Reverse.Keys)
			{
				this._starmap.Systems.Reverse[current2].SetIsEnabled(true);
			}
			IL_124:
			this._lastSelectedFleet = selectedFleet;
		}
		private void UIHandleCoreEventBehaviour(string eventName, string[] eventParams)
		{
			if (eventName == "ObjectClicked")
			{
				this.ProcessGameEvent_ObjectClicked(eventParams);
				return;
			}
			if (eventName == "MouseOver")
			{
				this.ProcessGameEvent_MouseOver(eventParams);
			}
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			this.UIHandleCoreEventBehaviour(eventName, eventParams);
			if (!this._uiEnabled)
			{
				return;
			}
			this._piechart.TryGameEvent(eventName, eventParams);
			if (eventName == "ContextMenu" && this.RightClickEnabled)
			{
				this.ProcessGameEvent_ContextMenu(eventParams);
			}
		}
		private void SetSelectedPlanet(int value, string trigger)
		{
			if (this._selectedPlanet == value)
			{
				return;
			}
			this._selectedPlanet = value;
			StarSystemUI.SyncPlanetDetailsWidget(base.App.Game, "planetDetailsWidget", this.SelectedSystem, this._selectedPlanet, this.GetPlanetViewGameObject(this.SelectedSystem, this._selectedPlanet), this._planetView);
			this._planetWidget.Sync(value, false, false);
			if (this._uiEnabled)
			{
				StarSystemUI.SyncColonyDetailsWidget(base.App.Game, "colonyDetailsWidget", this._selectedPlanet, "");
			}
		}
		public GameObjectSet GetCrits()
		{
			return this._crits;
		}
		public void SetProvinceMode(bool isProvinceMode)
		{
			this._isProvinceMode = isProvinceMode;
			if (this._isProvinceMode)
			{
				base.App.UI.SetChecked("gameProvinceModeButton", false);
				this._mode = new ProvinceEditStarMapStateMode(base.App.Game, this, this._starmap);
				this._mode.Initialize();
				return;
			}
			base.App.UI.SetChecked("gameProvinceModeButton", true);
			if (this._mode != null)
			{
				this._mode.Terminate();
				this._mode = null;
			}
		}
		private object EngineObjectToDatabaseObject(IGameObject obj)
		{
			int systemId;
			if (obj is StarMapSystem && this._starmap.Systems.Forward.TryGetValue((StarMapSystem)obj, out systemId))
			{
				return base.App.GameDatabase.GetStarSystemInfo(systemId);
			}
			int fleetID;
			if (obj is StarMapFleet && this._starmap.Fleets.Forward.TryGetValue((StarMapFleet)obj, out fleetID))
			{
				return base.App.GameDatabase.GetFleetInfo(fleetID);
			}
			return null;
		}
		private FleetInfo EngineObjectToSystem(IGameObject obj)
		{
			return null;
		}
		private void SelectObject(IGameObject o)
		{
			this.SelectedObject = this.EngineObjectToDatabaseObject(o);
			if (this.SelectedSystemHasFriendlyColonyScreen())
			{
				base.App.PostRequestGuiSound("starmap_selectcolonysystem");
			}
			else
			{
				base.App.PostRequestGuiSound("starmap_selectsystem");
			}
			if (!base.App.GameSettings.SeperateStarMapFocus)
			{
				this._starmap.SetFocus(o);
			}
			if (this._uiEnabled)
			{
				base.App.UI.SetEnabled("gameRepairButton", this.CanOpenRepairScreen());
				base.App.UI.SetEnabled("gameBuildButton", this.CanOpenBuildScreen());
				base.App.UI.SetEnabled("gameFleetManagerButton", this.CanOpenFleetManager(0));
				base.App.UI.SetEnabled("gameBattleRiderManagerButton", this.CanOpenRiderManager());
			}
		}
		private void ProcessGameEvent_ContextMenu(string[] eventParams)
		{
			if (!this._uiEnabled)
			{
				return;
			}
			int num = int.Parse(eventParams[0]);
			if (num == 0)
			{
				return;
			}
			IGameObject gameObject = base.App.GetGameObject(num);
			if (gameObject is StarMapFleet)
			{
				StarMapFleet fleet = (StarMapFleet)gameObject;
				if (fleet.PlayerID != base.App.LocalPlayer.ID)
				{
					bool flag = base.App.Game.ScriptModules.NeutronStar != null && base.App.Game.ScriptModules.NeutronStar.PlayerID == fleet.PlayerID;
					bool flag2 = base.App.Game.ScriptModules.Gardeners != null && base.App.Game.ScriptModules.Gardeners.PlayerID == fleet.PlayerID;
					if (flag || flag2)
					{
						int systemID = 0;
						if (flag)
						{
							NeutronStarInfo neutronStarInfo = base.App.GameDatabase.GetNeutronStarInfos().FirstOrDefault((NeutronStarInfo x) => x.FleetId == fleet.FleetID);
							if (neutronStarInfo != null && neutronStarInfo.DeepSpaceSystemId.HasValue)
							{
								systemID = neutronStarInfo.DeepSpaceSystemId.Value;
							}
						}
						else
						{
							GardenerInfo gardenerInfo = base.App.GameDatabase.GetGardenerInfos().FirstOrDefault((GardenerInfo x) => x.FleetId == fleet.FleetID);
							if (gardenerInfo != null && gardenerInfo.DeepSpaceSystemId.HasValue)
							{
								systemID = gardenerInfo.DeepSpaceSystemId.Value;
							}
						}
						List<StationInfo> list = base.App.GameDatabase.GetStationForSystem(systemID).ToList<StationInfo>();
						if (list.Count == 0)
						{
							this._fleetContextFleet = fleet.FleetID;
							base.App.UI.AutoSize(this._enemyGMStationContextMenuID);
							FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(fleet.FleetID);
							Faction faction = base.App.AssetDatabase.GetFaction(base.App.GameDatabase.GetPlayerFactionID(fleet.PlayerID));
							base.App.UI.SetEnabled(base.App.UI.Path(new string[]
							{
								this._enemyGMStationContextMenuID,
								"fleetBuildStationButton"
							}), !faction.CanUseNodeLine(null) && fleetInfo.Type != FleetType.FL_CARAVAN);
							base.App.UI.ShowTooltip(this._enemyGMStationContextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
							return;
						}
						if (list.Any((StationInfo x) => x.PlayerID == base.App.LocalPlayer.ID))
						{
							return;
						}
					}
					this._fleetContextFleet = fleet.FleetID;
					base.App.UI.AutoSize(this._enemyContextMenuID);
					FleetInfo fleetInfo2 = base.App.GameDatabase.GetFleetInfo(fleet.FleetID);
					Faction faction2 = base.App.AssetDatabase.GetFaction(base.App.GameDatabase.GetPlayerFactionID(fleet.PlayerID));
					base.App.UI.SetEnabled(base.App.UI.Path(new string[]
					{
						this._enemyContextMenuID,
						"fleetInterceptButton"
					}), (fleetInfo2.IsAcceleratorFleet || !faction2.CanUseNodeLine(null)) && fleetInfo2.Type != FleetType.FL_CARAVAN);
					base.App.UI.ShowTooltip(this._enemyContextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
					return;
				}
				this._fleetContextFleet = fleet.FleetID;
				base.App.UI.AutoSize(this._fleetContextMenuID);
				MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(this._fleetContextFleet);
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._fleetContextMenuID,
					"fleetCancelButton"
				}), missionByFleetID != null && missionByFleetID.Type != MissionType.RETURN && missionByFleetID.Type != MissionType.RETREAT && base.App.GameDatabase.GetFleetInfo(this._fleetContextFleet).Type != FleetType.FL_CARAVAN);
				base.App.UI.ShowTooltip(this._fleetContextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
				return;
			}
			else
			{
				if (!(gameObject is StarMapSystem))
				{
					return;
				}
				int num2 = this._starmap.Systems.Forward[(StarMapSystem)gameObject];
				this._contextMenuSystem = num2;
				StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(num2);
				string panelId = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameSurveyButton"
				});
				string panelId2 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameColonizeButton"
				});
				string panelId3 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameEvacuateButton"
				});
				string panelId4 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameRelocateButton"
				});
				string panelId5 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gamePatrolButton"
				});
				string panelId6 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameInterdictButton"
				});
				string panelId7 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameStrikeButton"
				});
				string panelId8 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameInvadeButton"
				});
				string panelId9 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameSupportButton"
				});
				string panelId10 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameConstructStationButton"
				});
				string panelId11 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameUpgradeStationButton"
				});
				string panelId12 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameContextFleetManagerButton"
				});
				string panelId13 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameDefenseManagerButton"
				});
				string panelId14 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameContextStationManagerButton"
				});
				string panelId15 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameGateButton"
				});
				string panelId16 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gamePiracyButton"
				});
				string panelId17 = base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameNPGButton"
				});
				bool flag3 = base.App.Game.SystemHasPlayerColony(num2, base.App.LocalPlayer.ID);
				bool flag4 = base.App.GameDatabase.GetNavalStationForSystemAndPlayer(num2, base.App.LocalPlayer.ID) != null;
                bool value = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.SURVEY, true).Any<FleetInfo>();
                bool value2 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.COLONIZATION, true).Any<FleetInfo>() && StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(base.App, num2, base.App.LocalPlayer.ID).Count<int>() > 0;
                bool value3 = base.App.GameDatabase.HasEndOfFleshExpansion() && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.EVACUATE, true).Any<FleetInfo>() && StarSystemDetailsUI.CollectPlanetListItemsForEvacuateMission(base.App, num2, base.App.LocalPlayer.ID).Count<int>() > 0;
                bool value4 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.RELOCATION, true).Any<FleetInfo>() || Kerberos.Sots.StarFleet.StarFleet.HasRelocatableDefResAssetsInRange(base.App.Game, num2);
                bool value5 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.PATROL, true).Any<FleetInfo>();
                bool value6 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.INTERDICTION, true).Any<FleetInfo>();
                bool value7 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.STRIKE, true).Any<FleetInfo>();
                bool value8 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.INVASION, true).Any<FleetInfo>();
                bool value9 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.SUPPORT, true).Any<FleetInfo>();
				bool arg_B60_0;
                if (Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.CONSTRUCT_STN, true).Any<FleetInfo>())
				{
					if ((
						from j in StarSystem.GetSystemCanSupportStations(base.App.Game, num2, base.App.LocalPlayer.ID)
						where j != StationType.DEFENCE
						select j).Count<StationType>() > 0)
					{
						arg_B60_0 = (!base.App.GameDatabase.GetSystemOwningPlayer(num2).HasValue || base.App.GameDatabase.GetSystemOwningPlayer(num2) == base.App.LocalPlayer.ID || StarMapState.SystemHasIndependentColony(base.App.Game, num2));
						goto IL_B60;
					}
				}
				arg_B60_0 = false;
				IL_B60:
				bool value10 = arg_B60_0;
                bool value11 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.CONSTRUCT_STN, true).Any<FleetInfo>() && base.App.Game.GetUpgradableStations(base.App.GameDatabase.GetStationForSystemAndPlayer(num2, base.App.LocalPlayer.ID).ToList<StationInfo>()).Count > 0;
                bool value12 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.GATE, true).Any<FleetInfo>();
				bool value13 = (base.App.GameDatabase.GetFleetInfoBySystemID(num2, FleetType.FL_NORMAL).Any<FleetInfo>() && flag3) || flag4;
				bool value14 = base.App.GameDatabase.GetStationForSystemAndPlayer(num2, base.App.LocalPlayer.ID).Any<StationInfo>() && (flag3 || flag4);
				bool value15 = GameSession.PlayerPresentInSystem(base.App.GameDatabase, base.App.LocalPlayer.ID, num2);
				bool arg_CE3_0;
                if (Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.PIRACY, true).Any<FleetInfo>())
				{
					arg_CE3_0 = !base.App.GameDatabase.GetMissionsBySystemDest(num2).Any((MissionInfo x) => x.Type == MissionType.PIRACY);
				}
				else
				{
					arg_CE3_0 = false;
				}
				bool value16 = arg_CE3_0;
                bool value17 = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(base.App.Game, base.App.LocalPlayer.ID, num2, MissionType.DEPLOY_NPG, true).Any<FleetInfo>();
				base.App.UI.SetEnabled(panelId, value);
				base.App.UI.SetEnabled(panelId2, value2);
				base.App.UI.SetEnabled(panelId3, value3);
				base.App.UI.SetEnabled(panelId4, value4);
				base.App.UI.SetEnabled(panelId5, value5);
				base.App.UI.SetEnabled(panelId6, value6);
				base.App.UI.SetEnabled(panelId7, value7);
				base.App.UI.SetEnabled(panelId8, value8);
				base.App.UI.SetEnabled(panelId9, value9);
				base.App.UI.SetEnabled(panelId10, value10);
				base.App.UI.SetEnabled(panelId11, value11);
				base.App.UI.SetEnabled(panelId12, value13);
				base.App.UI.SetEnabled(panelId13, value15);
				base.App.UI.SetEnabled(panelId14, value14);
				base.App.UI.SetEnabled(panelId16, value16);
				base.App.UI.SetVisible(panelId15, value12);
				base.App.UI.SetEnabled(panelId15, value12);
				base.App.UI.SetVisible(panelId17, value17);
				base.App.UI.SetEnabled(panelId17, value17);
				base.App.UI.SetVisible(panelId3, base.App.GameDatabase.HasEndOfFleshExpansion());
				base.App.UI.SetEnabled(panelId3, value3);
				base.App.UI.SetPropertyBool(panelId, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId2, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId3, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId4, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId5, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId6, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId7, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId9, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId8, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId10, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId12, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId13, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId14, "lockout_button", true);
				base.App.UI.SetPropertyBool(panelId16, "lockout_button", true);
				base.App.UI.AutoSize(this._contextMenuID);
				base.App.UI.ShowTooltip(this._contextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
				this._starmap.PostSetProp("ContextMenu", new object[]
				{
					this._contextMenuID,
					this._starmap.Systems.Reverse[this._contextMenuSystem]
				});
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"system_name"
				}), "text", starSystemInfo.Name);
				base.App.UI.ForceLayout(this._contextMenuID);
				base.App.UI.ForceLayout(this._contextMenuID);
				return;
			}
		}
		private void ShowResearchContext()
		{
			if (!this._uiEnabled)
			{
				return;
			}
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				this._researchContextID,
				"gameResearchButton"
			}), "lockout_button", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				this._researchContextID,
				"gameSalvageProjectsButton"
			}), "lockout_button", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				this._researchContextID,
				"gameSpecialProjectsButton"
			}), "lockout_button", true);
			base.App.UI.AutoSize(this._researchContextID);
			base.App.UI.ShowTooltip(this._researchContextID, 100f, 100f);
		}
		private void ProcessGameEvent_ObjectClicked(string[] eventParams)
		{
			int id = int.Parse(eventParams[0]);
			if (this._mode == null || !this._mode.OnGameObjectClicked(base.App.GetGameObject(id)))
			{
				this.SelectObject(base.App.GetGameObject(id));
				return;
			}
			this.SelectedObject = null;
		}
		internal static void UpdateGateUI(GameSession game, string panelName)
		{
			int totalGateCapacity = GameSession.GetTotalGateCapacity(game, game.LocalPlayer.ID);
			if (totalGateCapacity == 0)
			{
				game.UI.SetVisible(panelName, false);
				return;
			}
			game.UI.SetVisible(panelName, true);
			string panelId = game.UI.Path(new string[]
			{
				panelName,
				"gateTotalPower"
			});
			game.UI.SetPropertyString(panelId, "text", totalGateCapacity.ToString());
			string panelId2 = game.UI.Path(new string[]
			{
				panelName,
				"gateUsedPower"
			});
			game.UI.SetPropertyString(panelId2, "text", GameSession.GetTotalGateUsage(game, game.LocalPlayer.ID).ToString());
		}
		internal static void UpdateNPGUI(GameSession game, string panelName)
		{
            int maxLoaFleetCubeMassForTransit = Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(game, game.LocalPlayer.ID);
			if (maxLoaFleetCubeMassForTransit == 0)
			{
				game.UI.SetVisible(panelName, false);
				return;
			}
			game.UI.SetVisible(panelName, true);
			string panelId = game.UI.Path(new string[]
			{
				panelName,
				"gateCapacity"
			});
			game.UI.SetPropertyString(panelId, "text", maxLoaFleetCubeMassForTransit.ToString("N0"));
		}
		private static void ShowStarSystemPopup(string tooltipPanelId, App game, StarMap starmap, int gameObjectId)
		{
			StarMapSystem starMapSystem = null;
			if (gameObjectId != 0)
			{
				starMapSystem = game.GetGameObject<StarMapSystem>(gameObjectId);
			}
			if (starMapSystem == null)
			{
				return;
			}
			if (!starmap.Systems.Forward.ContainsKey(starMapSystem))
			{
				return;
			}
			int num = starmap.Systems.Forward[starMapSystem];
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(num);
			string panelId = game.UI.Path(new string[]
			{
				tooltipPanelId,
				"partSystemName"
			});
			game.UI.SetText(panelId, starSystemInfo.Name);
			if (!game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, num))
			{
				game.UI.SetPropertyColor(panelId, "color", 100f, 100f, 100f);
			}
			else
			{
				game.UI.SetPropertyColor(panelId, "color", 255f, 255f, 255f);
			}
			string mapPanelId = game.UI.Path(new string[]
			{
				tooltipPanelId,
				"partMiniSystem"
			});
			StarSystemMapUI.Sync(game, num, mapPanelId, false);
			game.UI.SetVisible(tooltipPanelId, true);
		}
		private void ProcessGameEvent_MouseOver(string[] eventParams)
		{
			int num = int.Parse(eventParams[0]);
			if ((this._mode == null || !this._mode.OnGameObjectMouseOver(base.App.GetGameObject(num))) && this._colonizeDialog == null)
			{
				if (num == 0)
				{
					base.App.UI.SetVisible("StarSystemPopup", false);
					return;
				}
				StarMapState.ShowStarSystemPopup("StarSystemPopup", base.App, this._starmap, num);
			}
		}
		private bool CanOpenFleetManager(int systemid = 0)
		{
			if (systemid == 0)
			{
				return FleetManagerState.CanOpen(base.App.Game, this.SelectedSystem);
			}
			return FleetManagerState.CanOpen(base.App.Game, systemid);
		}
		private bool CanOpenRiderManager()
		{
			return RiderManagerState.CanOpen(base.App.Game, this.SelectedSystem);
		}
		private void OpenFleetManager(int systemid = 0)
		{
			if (systemid == 0)
			{
				base.App.SwitchGameState<FleetManagerState>(new object[]
				{
					this.SelectedSystem
				});
				return;
			}
			base.App.SwitchGameState<FleetManagerState>(new object[]
			{
				systemid
			});
		}
		private bool CanOpenStationManager()
		{
			return StationManagerDialog.CanOpen(base.App.Game, this.SelectedSystem);
		}
		private bool CanOpenPlanetManager()
		{
			return PlanetManagerState.CanOpen(base.App.Game, this.SelectedSystem);
		}
		private void OpenPlanetManager()
		{
			base.App.SwitchGameState<PlanetManagerState>(new object[]
			{
				this.SelectedSystem
			});
		}
		private void PopulateViewFilterList()
		{
			base.App.UI.ClearItems("viewModeDropdown");
			base.App.UI.AddItem("viewModeDropdown", "", 0, App.Localize("@UI_STARMAPVIEW_NORMAL_VIEW"));
			base.App.UI.AddItem("viewModeDropdown", "", 1, App.Localize("@UI_STARMAPVIEW_SURVEY_DISPLAY"));
			base.App.UI.AddItem("viewModeDropdown", "", 2, App.Localize("@UI_STARMAPVIEW_PROVINCE_DISPLAY"));
			base.App.UI.AddItem("viewModeDropdown", "", 3, App.Localize("@UI_STARMAPVIEW_SUPPORT_RANGE_DISPLAY"));
			base.App.UI.AddItem("viewModeDropdown", "", 4, App.Localize("@UI_STARMAPVIEW_SENSOR_RANGE_DISPLAY"));
			base.App.UI.AddItem("viewModeDropdown", "", 5, App.Localize("@UI_STARMAPVIEW_TERRAIN_DISPLAY"));
			if (base.App.GetStratModifier<bool>(StratModifiers.EnableTrade, base.App.LocalPlayer.ID))
			{
				base.App.UI.AddItem("viewModeDropdown", "", 6, App.Localize("@UI_STARMAPVIEW_TRADE_DISPLAY"));
			}
		}
		private void UIHandleCoreBehaviour(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "gameEndTurnButton")
				{
					this.SetUIEnabled(!this._uiEnabled);
					return;
				}
				if (panelName == "gameExitButton")
				{
					base.App.UI.CreateDialog(new MainMenuDialog(base.App), null);
					return;
				}
				if (panelName == "gameTutorialButton")
				{
					if (this._lastFilterSelection == StarMapViewFilter.VF_TRADE)
					{
						base.App.UI.SetVisible("TradeTutorial", true);
						return;
					}
					base.App.UI.SetVisible("StarMapTutorial", true);
					return;
				}
				else
				{
					if (panelName == "starMapTutImage")
					{
						base.App.UI.SetVisible("StarMapTutorial", false);
						base.App.UI.SetVisible("TradeTutorial", false);
						return;
					}
					if (FleetUI.HandleFleetAndPlanetWidgetInput(base.App, "fleetAndPlanetDetailsWidget", panelName))
					{
						return;
					}
					if (panelName == "gameEventHistoryButton")
					{
						base.App.UI.CreateDialog(new EventHistoryDialog(base.App), null);
						return;
					}
					if (panelName == "turnEventNext")
					{
						this.ShowNextEvent(false);
						return;
					}
					if (panelName == "turnEventPrevious")
					{
						this.ShowNextEvent(true);
						return;
					}
					if (panelName == "btnturnEventImageButton")
					{
						this.FocusLastEvent();
						return;
					}
				}
			}
			else
			{
				if (msgType == "list_sel_changed")
				{
					if (panelName == "partSystemPlanets")
					{
						int value = 0;
						if (msgParams.Length > 0 && !string.IsNullOrEmpty(msgParams[0]))
						{
							value = int.Parse(msgParams[0]);
						}
						this.SetSelectedPlanet(value, panelName);
						return;
					}
					if (panelName == "viewModeDropdown")
					{
						StarMapViewFilter starMapViewFilter = (StarMapViewFilter)int.Parse(msgParams[0]);
						this._starmap.ViewFilter = starMapViewFilter;
						this._lastFilterSelection = starMapViewFilter;
						this.RefreshSystemInterface();
						return;
					}
				}
				else
				{
					if (msgType == "endturn_activated")
					{
						this.EndTurn(false);
					}
				}
			}
		}
		public void FocusLastEvent()
		{
			if (this._currentEvent != null && this._currentEvent.SystemID != 0)
			{
				StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(this._currentEvent.SystemID);
				if (starSystemInfo == null || starSystemInfo.IsDeepSpace)
				{
					return;
				}
				StarMapSystem starMapSystem = this._starmap.Systems.Reverse[this._currentEvent.SystemID];
				this.SelectObject(starMapSystem);
				this.StarMap.SetFocus(starMapSystem);
				this.StarMap.Select(starMapSystem);
				this.SelectedObject = base.App.GameDatabase.GetStarSystemInfo(this._currentEvent.SystemID);
				if (this._currentEvent.ColonyID != 0)
				{
					ColonyInfo colonyInfo = base.App.GameDatabase.GetColonyInfo(this._currentEvent.ColonyID);
					if (colonyInfo != null)
					{
						this.SetSelectedPlanet(colonyInfo.OrbitalObjectID, "");
					}
				}
			}
		}
		public void ShowUpgradeMissionOverlay(int targetSystem)
		{
			this._missionOverlays["gameUpgradeStationButton"].Show(targetSystem);
		}
		public OverlayUpgradeMission GetUpgradeMissionOverlay()
		{
			return (OverlayUpgradeMission)this._missionOverlays["gameUpgradeStationButton"];
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			this.UIHandleCoreBehaviour(panelName, msgType, msgParams);
			if (!this._uiEnabled)
			{
				return;
			}
			if (this._piechart != null && this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
			{
				return;
			}
			if (msgType == "mapicon_clicked")
			{
				if (panelName == "partMiniSystem" && base.App.GameDatabase.IsSurveyed(base.App.LocalPlayer.ID, this.SelectedSystem))
				{
					int value = int.Parse(msgParams[0]);
					this.SetSelectedPlanet(value, panelName);
					return;
				}
			}
			else
			{
				if (msgType == "mapicon_clicked_station")
				{
					if (panelName == "partMiniSystem")
					{
						int.Parse(msgParams[0]);
						int.Parse(msgParams[1]);
						return;
					}
				}
				else
				{
					if (msgType == "mapicon_dblclicked")
					{
						if (panelName == "partMiniSystem" && !this.OverlayActive())
						{
							int.Parse(msgParams[0]);
							if (base.App.GameDatabase.IsSurveyed(base.App.LocalPlayer.ID, this.SelectedSystem))
							{
								this.OpenSystemView();
								return;
							}
						}
					}
					else
					{
						if (msgType == "button_rclicked")
						{
							if (panelName == "gameResearchButton")
							{
								this.ShowResearchContext();
								return;
							}
						}
						else
						{
							if (msgType == "button_clicked")
							{
								if (this._mode == null || !this._mode.OnUIButtonPressed(panelName))
								{
									if (panelName == "gameResearchButton" || panelName == "researchCubeButton")
									{
										base.App.SwitchGameState("ResearchScreenState");
										return;
									}
									if (panelName == "gameSalvageProjectsButton")
									{
										base.App.UI.CreateDialog(new SalvageProjectDialog(base.App, "dialogSpecialProjects"), null);
										return;
									}
									if (panelName == "gameSpecialProjectsButton")
									{
										base.App.UI.CreateDialog(new SpecialProjectDialog(base.App, "dialogSpecialProjects"), null);
										return;
									}
									if (panelName == "btnAbandon")
									{
										if (base.App.GameDatabase.GetColonyInfoForPlanet(this._selectedPlanet) != null)
										{
											this._confirmAbandon = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, "@UI_DIALOGCONFIRMABANDON_TITLE", "@UI_DIALOGCONFIRMABANDON_DESC", "dialogGenericQuestion"), null);
											return;
										}
									}
									else
									{
										if (panelName == "gameBattleRiderManagerButton")
										{
											base.App.UI.LockUI();
											this.OpenBattleRiderManagerScreen();
											return;
										}
										if (panelName == "gameDiplomacyButton")
										{
											base.App.UI.LockUI();
											this.OpenDiplomacyScreen();
											return;
										}
										if (panelName == "gameDesignButton")
										{
											base.App.UI.LockUI();
											this.OpenDesignScreen();
											return;
										}
										if (panelName == "gameBuildButton")
										{
											base.App.UI.LockUI();
											this.OpenBuildScreen();
											return;
										}
										if (panelName == "gameRepairButton")
										{
											if (this.SelectedSystem != 0)
											{
												base.App.UI.CreateDialog(new RepairShipsDialog(base.App, this.SelectedSystem, base.App.GameDatabase.GetFleetInfoBySystemID(this.SelectedSystem, FleetType.FL_ALL).ToList<FleetInfo>(), "dialogRepairShips"), null);
												return;
											}
											List<FleetInfo> list = new List<FleetInfo>();
											list.Add(base.App.GameDatabase.GetFleetInfo(this.SelectedFleet));
											base.App.UI.CreateDialog(new RepairShipsDialog(base.App, this.SelectedSystem, list, "dialogRepairShips"), null);
											return;
										}
										else
										{
											if (panelName == "btnSystemOpen")
											{
												bool flag = !base.App.GameDatabase.GetStarSystemInfo(this.SelectedSystem).IsOpen;
												base.App.GameDatabase.UpdateStarSystemOpen(this.SelectedSystem, flag);
												base.App.UI.SetVisible("SystemDetailsWidget.ClosedSystem", !flag);
												base.App.Game.OCSystemToggleData.SystemToggled(base.App.LocalPlayer.ID, this.SelectedSystem, flag);
												return;
											}
											if (panelName == "gameSystemButton")
											{
												base.App.UI.LockUI();
												this.OpenSystemView();
												return;
											}
											if (panelName == "gameSotspediaButton")
											{
												base.App.UI.LockUI();
												base.App.SwitchGameState("SotspediaState");
												return;
											}
											if (panelName == "gameProvinceModeButton")
											{
												this.SetProvinceMode(!this._isProvinceMode);
												return;
											}
											if (panelName == "gameEmpireSummaryButton")
											{
												base.App.UI.LockUI();
												base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
												return;
											}
											if (panelName == "debugTestCombatButton")
											{
												this.DebugTestCombat();
												return;
											}
											if (panelName == "gameUpgradeStationButton")
											{
												this._missionOverlays["gameUpgradeStationButton"].Show(this._contextMenuSystem);
												return;
											}
											if (panelName == "partHardenedStructure")
											{
												string panelName2 = (this._dialogState == ESMDialogState.ESMD_ColonyEstablished) ? "colony_event_dialog" : "colonyDetailsWidget";
												int num = (this._dialogState == ESMDialogState.ESMD_ColonyEstablished) ? this._colonyEstablishedPlanet : this.SelectedPlanet;
												ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(num);
												colonyInfoForPlanet.isHardenedStructures = !colonyInfoForPlanet.isHardenedStructures;
												base.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
												StarSystemUI.SyncColonyDetailsWidget(base.App.Game, panelName2, num, panelName);
												return;
											}
											if (panelName == "gameStationManagerButton")
											{
												base.App.UI.CreateDialog(new StationManagerDialog(base.App, this, 0, "dialogStationManager"), null);
												return;
											}
											if (panelName == "gameContextStationManagerButton")
											{
												base.App.UI.CreateDialog(new StationManagerDialog(base.App, this, this._contextMenuSystem, "dialogStationManager"), null);
												return;
											}
											if (panelName == "gamePlanetSummaryButton")
											{
												base.App.UI.CreateDialog(new PlanetManagerDialog(base.App, "dialogPlanetManager"), null);
												return;
											}
											if (panelName == "gamePopulationManagerButton")
											{
												base.App.UI.CreateDialog(new PopulationManagerDialog(base.App, this.SelectedSystem, "dialogPopulationManager"), null);
												return;
											}
											if (panelName == "gameComparativeAnalysysButton")
											{
												base.App.UI.LockUI();
												base.App.SwitchGameState<ComparativeAnalysysState>(new object[]
												{
													false,
													"StarMapState"
												});
												return;
											}
											if (panelName == "gameFleetSummaryButton")
											{
												base.App.UI.CreateDialog(new FleetSummaryDialog(base.App, "FleetSummaryDialog"), null);
												return;
											}
											if (panelName == "gameFleetManagerButton")
											{
												if (this.CanOpenFleetManager(0))
												{
													base.App.UI.LockUI();
													this.OpenFleetManager(0);
													return;
												}
											}
											else
											{
												if (panelName == "gameContextFleetManagerButton")
												{
													if (this.CanOpenFleetManager(this._contextMenuSystem))
													{
														base.App.UI.LockUI();
														this.OpenFleetManager(this._contextMenuSystem);
														return;
													}
												}
												else
												{
													if (panelName == "gameDefenseManagerButton")
													{
														base.App.UI.LockUI();
														base.App.SwitchGameState<DefenseManagerState>(new object[]
														{
															this._contextMenuSystem
														});
														return;
													}
													if (panelName == "fleetCancelButton")
													{
														FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(this._fleetContextFleet);
														AdmiralInfo admiralInfo = base.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
														if (admiralInfo != null)
														{
															string cueName = string.Format("STRAT_008-01_{0}_{1}UniversalMissionNegation", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)), admiralInfo.GetAdmiralSoundCueContext(base.App.AssetDatabase));
															base.App.PostRequestSpeech(cueName, 50, 120, 0f);
														}
                                                        Kerberos.Sots.StarFleet.StarFleet.CancelMission(base.App.Game, fleetInfo, true);
														StarMapState.UpdateGateUI(base.App.Game, "gameGateInfo");
														return;
													}
													if (panelName == "fleetInterceptButton")
													{
														((OverlayInterceptMission)this._missionOverlays["fleetInterceptButton"]).TargetFleet = this._fleetContextFleet;
														this._missionOverlays["fleetInterceptButton"].Show(this._contextMenuSystem);
														return;
													}
													if (panelName == "fleetBuildStationButton")
													{
														((OverlaySpecialConstructionMission)this._missionOverlays["fleetBuildStationButton"]).TargetFleet = this._fleetContextFleet;
														this._missionOverlays["fleetBuildStationButton"].Show(this._contextMenuSystem);
														return;
													}
													if (this._missionOverlays.ContainsKey(panelName))
													{
														this._missionOverlays[panelName].Show(this._contextMenuSystem);
														return;
													}
													if (panelName == "Build_Freighter")
													{
														List<DesignInfo> list2 = (
															from x in base.App.GameDatabase.GetDesignInfosForPlayer(base.App.LocalPlayer.ID, RealShipClasses.Cruiser, true)
															where x.DesignSections.Any((DesignSectionInfo j) => j.ShipSectionAsset.FreighterSpace > 0) && x.isPrototyped
															select x).ToList<DesignInfo>();
														DesignInfo designInfo = null;
														foreach (DesignInfo current in list2)
														{
															if (designInfo == null)
															{
																designInfo = current;
															}
															else
															{
																if (current.DesignDate > designInfo.DesignDate)
																{
																	designInfo = current;
																}
															}
														}
														if (designInfo != null)
														{
															int invoiceId = base.App.GameDatabase.InsertInvoice("Freighter", base.App.LocalPlayer.ID, false);
															int value2 = base.App.GameDatabase.InsertInvoiceInstance(base.App.LocalPlayer.ID, this.SelectedSystem, "Freighter");
															base.App.GameDatabase.InsertInvoiceBuildOrder(invoiceId, designInfo.ID, designInfo.Name, 0);
															base.App.GameDatabase.InsertBuildOrder(this.SelectedSystem, designInfo.ID, 0, 0, designInfo.Name, designInfo.GetPlayerProductionCost(base.App.GameDatabase, base.App.LocalPlayer.ID, false, null), new int?(value2), null, 0);
															this.SyncFreighterInterface();
															return;
														}
													}
													else
													{
														if (panelName.StartsWith("tickerEventButton"))
														{
															string[] array = panelName.Split(new char[]
															{
																'|'
															});
															if (array.Count<string>() == 2)
															{
																int eventid = int.Parse(array[1]);
																if (base.App.Game.TurnEvents.Any((TurnEvent x) => x.ID == eventid))
																{
																	this.ShowEvent(eventid);
																	return;
																}
															}
														}
														else
														{
															if (panelName.StartsWith("freighterdeletebtn"))
															{
																string[] array2 = panelName.Split(new char[]
																{
																	'|'
																});
																int shipID = int.Parse(array2[1]);
																ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(shipID, false);
																if (shipInfo != null)
																{
																	this.DeleteFrieghterConfirm = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, "Scrap Freighter", "Confirm Scrap Freighter.", "dialogGenericQuestion"), null);
																	this._selectedfreighter = shipInfo.ID;
																	return;
																}
															}
															else
															{
																if (panelName == "btnChat")
																{
																	base.App.Network.SetChatWidgetVisibility(null);
																	base.App.UI.SetPropertyBool("btnChat", "flashing", false);
																	return;
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
							else
							{
								if (msgType == "ChatMessage")
								{
									base.App.UI.SetPropertyBool("btnChat", "flashing", true);
									return;
								}
								if (msgType == "mouse_enter")
								{
									if (panelName == "moraleeventtooltipover")
									{
										int x2 = int.Parse(msgParams[0]);
										int y = int.Parse(msgParams[1]);
										ColonyInfo colonyInfoForPlanet2 = base.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
										if (colonyInfoForPlanet2 != null && base.App.LocalPlayer.ID == colonyInfoForPlanet2.PlayerID)
										{
											StarSystemUI.ShowMoraleEventToolTip(base.App.Game, colonyInfoForPlanet2.ID, x2, y);
											return;
										}
									}
								}
								else
								{
									if (msgType == "mouse_leave")
									{
										return;
									}
									if (msgType == "slider_value")
									{
										if (panelName.Contains("meTradeSlider"))
										{
											string[] array3 = panelName.Split(new char[]
											{
												'|'
											});
											int num2 = int.Parse(array3[1]);
											OrbitalObjectInfo orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(num2);
											if (orbitalObjectInfo != null)
											{
												StarSystemDetailsUI.SetOutputRate(base.App, num2, panelName, msgParams[0]);
												this._starmap.UpdateSystemTrade(orbitalObjectInfo.StarSystemID);
												return;
											}
										}
										else
										{
											if (panelName == "gameEmpireResearchSlider")
											{
												StarMapState.SetEmpireResearchRate(base.App.Game, msgParams[0], this._piechart);
												this._techCube.SpinSpeed = (float)int.Parse(msgParams[0]) * 0.002f;
												this.UpdateTechCubeToolTip();
												return;
											}
											if (this.SelectedPlanet != StarSystemDetailsUI.StarItemID && this.SelectedPlanet != 0)
											{
												string panelName3 = (this._dialogState == ESMDialogState.ESMD_ColonyEstablished) ? "colony_event_dialog" : "colonyDetailsWidget";
												if (StarSystemDetailsUI.IsOutputRateSlider(panelName))
												{
													StarSystemDetailsUI.SetOutputRate(base.App, this.SelectedPlanet, panelName, msgParams[0]);
													StarSystemUI.SyncColonyDetailsWidget(base.App.Game, panelName3, this.SelectedPlanet, panelName);
													this._starmap.UpdateSystemTrade(base.App.GameDatabase.GetOrbitalObjectInfo(this.SelectedPlanet).StarSystemID);
													return;
												}
												if (panelName == "partOverharvestSlider")
												{
													ColonyInfo colonyInfoForPlanet3 = base.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
													colonyInfoForPlanet3.OverharvestRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
													base.App.GameDatabase.UpdateColony(colonyInfoForPlanet3);
													StarSystemUI.SyncColonyDetailsWidget(base.App.Game, panelName3, this.SelectedPlanet, panelName);
													return;
												}
												if (panelName == "partCivSlider")
												{
													ColonyInfo colonyInfoForPlanet4 = base.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
													colonyInfoForPlanet4.CivilianWeight = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
													base.App.GameDatabase.UpdateColony(colonyInfoForPlanet4);
													StarSystemUI.SyncColonyDetailsWidget(base.App.Game, panelName3, this.SelectedPlanet, panelName);
													return;
												}
												if (panelName == "partWorkRate")
												{
													ColonyInfo colonyInfoForPlanet5 = base.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
													colonyInfoForPlanet5.SlaveWorkRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
													base.App.GameDatabase.UpdateColony(colonyInfoForPlanet5);
													StarSystemUI.SyncColonyDetailsWidget(base.App.Game, panelName3, this.SelectedPlanet, panelName);
													return;
												}
											}
										}
									}
									else
									{
										if (msgType == "slider_notched")
										{
											if (panelName.Contains("meTradeSlider"))
											{
												string[] array4 = panelName.Split(new char[]
												{
													'|'
												});
												int orbitalObjectID = int.Parse(array4[1]);
												OrbitalObjectInfo orbitalObjectInfo2 = base.App.GameDatabase.GetOrbitalObjectInfo(orbitalObjectID);
												if (orbitalObjectInfo2 != null)
												{
													ColonyInfo colonyInfoForPlanet6 = base.App.GameDatabase.GetColonyInfoForPlanet(orbitalObjectID);
													if (colonyInfoForPlanet6 != null)
													{
														PlanetWidget.UpdateTradeSliderNotchInfo(base.App, colonyInfoForPlanet6.ID, int.Parse(msgParams[0]));
													}
												}
											}
											if (panelName.Contains("partTradeSlider"))
											{
												ColonyInfo colonyInfoForPlanet7 = base.App.GameDatabase.GetColonyInfoForPlanet(this.SelectedPlanet);
												if (colonyInfoForPlanet7 != null && colonyInfoForPlanet7.PlayerID == base.App.LocalPlayer.ID)
												{
													PlanetWidget.UpdateTradeSliderNotchInfo(base.App, colonyInfoForPlanet7.ID, int.Parse(msgParams[0]));
													return;
												}
											}
										}
										else
										{
											if (msgType == "list_item_dblclk")
											{
												if (panelName == "partSystemPlanets" && base.App.GameDatabase.IsSurveyed(base.App.LocalPlayer.ID, this.SelectedSystem))
												{
													this.OpenSystemView();
													return;
												}
											}
											else
											{
												if (msgType == "text_changed")
												{
													if (panelName.StartsWith("gameColonyName"))
													{
														this._enteredColonyName = msgParams[0];
														string panelId = base.App.UI.Path(new string[]
														{
															"colony_event_dialog",
															"event_dialog_close"
														});
														base.App.UI.SetEnabled(panelId, !string.IsNullOrWhiteSpace(this._enteredColonyName) && this._enteredColonyName.Length > 0);
														return;
													}
												}
												else
												{
													if (msgType == "dialog_closed" && (this._mode == null || !this._mode.OnUIDialogClosed(panelName, msgParams)))
													{
														if (this._eventDialogShown)
														{
															this._eventDialogShown = false;
															this._colonizeDialog = null;
															this._stationDialog = null;
															this.ShowNextEvent(false);
															this._dialogState = ESMDialogState.ESMD_None;
															if (panelName == this._feasibilityCompleteDialog)
															{
																this.RefreshTechCube();
																return;
															}
														}
														else
														{
															if (panelName == this._confirmAbandon)
															{
																if (bool.Parse(msgParams[0]))
																{
																	ColonyInfo colonyInfoForPlanet8 = base.App.GameDatabase.GetColonyInfoForPlanet(this._selectedPlanet);
																	if (colonyInfoForPlanet8 != null)
																	{
																		GameSession.AbandonColony(base.App, colonyInfoForPlanet8.ID);
																	}
																	this.RefreshSystemInterface();
																	return;
																}
															}
															else
															{
																if (panelName == this.DeleteFrieghterConfirm)
																{
																	if (bool.Parse(msgParams[0]))
																	{
																		ShipInfo shipInfo2 = base.App.GameDatabase.GetShipInfo(this._selectedfreighter, false);
																		if (shipInfo2 != null)
																		{
																			base.App.GameDatabase.RemoveShip(this._selectedfreighter);
																		}
																		this.DeleteFrieghterConfirm = null;
																		this.SyncFreighterInterface();
																		return;
																	}
																}
																else
																{
																	if (panelName == this._endTurnConfirmDialog)
																	{
																		if (bool.Parse(msgParams[0]))
																		{
																			this.SetUIEnabled(false);
																			this.EndTurn(true);
																			return;
																		}
																		this.EnableNextTurnButton(true);
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		public static void SetEmpireResearchRate(GameSession game, string value, BudgetPiechart piechart)
		{
			float num = (float)int.Parse(value) / 100f;
			num = num.Clamp(0f, 1f);
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			playerInfo.RateGovernmentResearch = 1f - num;
			if (game.GameDatabase.GetSliderNotchSettingInfo(playerInfo.ID, UISlidertype.SecuritySlider) != null)
			{
				Budget budget = Budget.GenerateBudget(game, playerInfo, null, BudgetProjection.Pessimistic);
				EmpireSummaryState.DistributeGovernmentSpending(game, EmpireSummaryState.GovernmentSpendings.Security, (float)Math.Min((double)((float)budget.RequiredSecurity / 100f), 1.0), playerInfo);
			}
			else
			{
				game.GameDatabase.UpdatePlayerSliders(game, playerInfo);
			}
			if (piechart != null)
			{
				Budget slices = Budget.GenerateBudget(game, playerInfo, null, BudgetProjection.Pessimistic);
				piechart.SetSlices(slices);
			}
		}
		private void DebugTestCombat()
		{
			if (this.SelectedSystem != 0)
			{
				base.App.LaunchCombat(base.App.Game, new PendingCombat
				{
					SystemID = this.SelectedSystem
				}, true, false, true);
			}
		}
		public void EndTurn(bool forceConfirm = false)
		{
			if (!forceConfirm && this.EnableFleetCheck)
			{
				List<FleetInfo> list = base.App.GameDatabase.GetFleetInfosByPlayerID(base.App.LocalPlayer.ID, FleetType.FL_NORMAL).ToList<FleetInfo>();
				foreach (FleetInfo current in list)
				{
					if (base.App.GameDatabase.GetMissionByFleetID(current.ID) == null)
					{
						this._endTurnConfirmDialog = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, "Inactive fleets", "You have inactive fleets, are you sure you want to end turn?", "dialogGenericQuestion"), null);
						this.SetUIEnabled(true);
						this.SetSelectedSystem(current.SystemID, true);
						return;
					}
				}
			}
			if (this.SelectedFleet != 0)
			{
				this._starmap.PostSetProp("SelectEnabled", false);
				this._starmap.PostSetProp("SelectEnabled", true);
				this.SelectedObject = null;
			}
			this.EnableNextTurnButton(false);
			base.App.Game.UpdateOpenCloseSystemToggle();
			App.Commands.EndTurn.Trigger();
			if (base.App.GameSetup.IsMultiplayer)
			{
				base.App.Network.EndTurn();
			}
			if (GameSession.SimAITurns > 0)
			{
				GameSession.SimAITurns--;
			}
		}
		public void ShowProcessingFlash()
		{
			base.App.UI.SetVisible("TurnUpdate", true);
			base.App.UI.SetVisible("TurnUpdate2", true);
		}
		private void EnableNextTurnButton(bool val)
		{
			base.App.UI.SetEnabled("gameEndTurnButton", val);
			this.SetUIEnabled(val);
		}
		public void TurnStarted()
		{
			base.App.UI.UnlockUI();
			this.EnableNextTurnButton(true);
			this.RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
			this.ShowNextEvent(false);
			TurnEventUI.SyncTurnEventTicker(base.App.Game, "event_list");
			base.App.UI.SetVisible("TurnUpdateText", false);
			base.App.UI.SetText("UpdateLabel", App.Localize("@TURN") + " " + base.App.GameDatabase.GetTurnCount());
			this.ShowProcessingFlash();
			this._simNewTurnTick = 200;
		}
		public void TurnEnded()
		{
			base.App.UserProfile.SaveProfile();
			base.App.TurnEvents.Clear();
			this._currentEvent = null;
			if (base.App.CurrentState == this)
			{
				base.App.UI.SetVisible("TurnUpdateText", true);
				base.App.UI.SetText("UpdateLabel", "PROCESSING...");
				this.ShowProcessingFlash();
				base.App.UI.LockUI();
				base.App.UI.Update();
				this.RefreshSystemInterface();
				this.PopulateViewFilterList();
			}
		}
		public void SetUIEnabled(bool val)
		{
			this._uiEnabled = val;
			base.App.UI.SetEnabled("gameEmpireSummaryButton", val);
			base.App.UI.SetEnabled("gameResearchButton", val);
			base.App.UI.SetEnabled("gameDiplomacyButton", val);
			base.App.UI.SetEnabled("btnAbandon", val);
			base.App.UI.SetEnabled("partHardenedStructure", val);
			base.App.UI.SetEnabled("gameDesignButton", val);
			base.App.UI.SetEnabled("gameRepairButton", val);
			base.App.UI.SetEnabled("gameBuildButton", val);
			base.App.UI.SetEnabled("gameSystemButton", val);
			base.App.UI.SetEnabled("gameSotspediaButton", val);
			base.App.UI.SetEnabled("gameProvinceModeButton", val);
			base.App.UI.SetEnabled("gameBattleRiderManagerButton", val);
			base.App.UI.SetEnabled("fleetCancelButton", val);
			base.App.UI.SetEnabled("gameSurveyButton", val);
			base.App.UI.SetEnabled("gameColonizeButton", val);
			base.App.UI.SetEnabled("gameEvacuateButton", val);
			base.App.UI.SetEnabled("gameRelocateButton", val);
			base.App.UI.SetEnabled("gamePatrolButton", val);
			base.App.UI.SetEnabled("gameInterdictButton", val);
			base.App.UI.SetEnabled("gameStrikeButton", val);
			base.App.UI.SetEnabled("gameInvadeButton", val);
			base.App.UI.SetEnabled("gameConstructStationButton", val);
			base.App.UI.SetEnabled("gameUpgradeStationButton", val);
			base.App.UI.SetEnabled("gameStationManagerButton", val);
			base.App.UI.SetEnabled("gameFleetManagerButton", val);
			base.App.UI.SetEnabled("gameFleetSummaryButton", val);
			base.App.UI.SetEnabled("gameDefenseManagerButton", val);
			base.App.UI.SetEnabled("gameGateButton", val);
			base.App.UI.SetEnabled("gamePiracyButton", val);
			base.App.UI.SetEnabled("gameNPGButton", val);
			base.App.UI.SetEnabled("gameEventHistoryButton", val);
			base.App.UI.SetEnabled("gameEmpireResearchSlider", val);
			base.App.UI.SetEnabled("gamePlanetSummaryButton", val);
			base.App.UI.SetEnabled("gamePopulationManagerButton", val);
			bool arg_35F_0;
			if (base.App.GameDatabase.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, base.App.LocalPlayer.ID))
			{
				arg_35F_0 = base.App.GameDatabase.GetDesignsEncountered(base.App.LocalPlayer.ID).Any((DesignInfo x) => x.Class != ShipClass.Station);
			}
			else
			{
				arg_35F_0 = false;
			}
			bool flag = arg_35F_0;
			base.App.UI.SetEnabled("gameComparativeAnalysysButton", val && flag);
			base.App.UI.SetVisible("colonyDetailsWidget", false);
			this._fleetWidget.SetEnabled(val);
			base.App.HotKeyManager.SetEnabled(true);
			if (val)
			{
				base.App.UI.SetEnabled("gameRepairButton", this.CanOpenRepairScreen());
				base.App.UI.SetEnabled("gameBuildButton", this.CanOpenBuildScreen());
				base.App.UI.SetEnabled("gameFleetManagerButton", this.CanOpenFleetManager(0));
				base.App.UI.SetEnabled("gameStationManagerButton", this.CanOpenStationManager());
			}
		}
		public void ShowEvent(int eventid)
		{
			if (base.App.TurnEvents.Count > 0 && base.App.TurnEvents.Any((TurnEvent x) => x.ID == eventid))
			{
				this._currentEvent = base.App.TurnEvents.FirstOrDefault((TurnEvent x) => x.ID == eventid);
				if (this._currentEvent.ShowsDialog && !this._currentEvent.dialogShown)
				{
					this.ShowEventDialog(this._currentEvent);
					this._eventDialogShown = true;
					this._currentEvent.dialogShown = true;
					base.App.GameDatabase.UpdateTurnEventDialogShown(this._currentEvent.ID, true);
				}
				TurnEventUI.SyncTurnEventWidget(base.App.Game, "turnEventWidget", this._currentEvent);
				base.App.PostRequestGuiSound("starmap_messagealert");
				this._currentEvent.EventViewed = true;
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					"turnEventNext",
					"newEventFlash"
				}), base.App.Game.TurnEvents.Any((TurnEvent x) => !x.EventViewed));
			}
		}
		public void ShowNextEvent(bool reverse = false)
		{
			if (base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).isDefeated)
			{
				base.App.UI.CreateDialog(new EndGameDialog(base.App, App.Localize("@UI_ENDGAME_DEFEAT"), string.Format(App.Localize("@UI_ENDGAME_DEFEAT_CONQUEST"), base.App.GameSetup.Players[base.App.Game.LocalPlayer.ID - 1].EmpireName), "loseScreen"), null);
			}
			else
			{
				if (base.App.TurnEvents.Count > 0)
				{
					if (base.App.TurnEvents.Any<TurnEvent>())
					{
						if (this._currentEvent != null)
						{
							int num = base.App.Game.TurnEvents.FindIndex((TurnEvent te) => te.ID == this._currentEvent.ID) + (reverse ? -1 : 1);
							if (num >= 0 && num <= base.App.Game.TurnEvents.Count - 1)
							{
								this._currentEvent = base.App.Game.TurnEvents[num];
							}
						}
						else
						{
							this._currentEvent = base.App.Game.TurnEvents[0];
						}
					}
					if (this._currentEvent.ShowsDialog && !this._currentEvent.dialogShown)
					{
						this.ShowEventDialog(this._currentEvent);
						this._eventDialogShown = true;
						this._currentEvent.dialogShown = true;
						base.App.GameDatabase.UpdateTurnEventDialogShown(this._currentEvent.ID, true);
					}
					TurnEventUI.SyncTurnEventWidget(base.App.Game, "turnEventWidget", this._currentEvent);
					base.App.PostRequestGuiSound("starmap_messagealert");
					if (!string.IsNullOrEmpty(this._currentEvent.EventSoundCueName) && this._currentEvent.PlayerID == base.App.LocalPlayer.ID && !this._currentEvent.EventViewed)
					{
						base.App.PostRequestSpeech(this._currentEvent.EventSoundCueName, 50, 120, 0f);
						this._currentEvent.EventSoundCueName = string.Empty;
						base.App.GameDatabase.UpdateTurnEventSoundQue(this._currentEvent.ID, this._currentEvent.EventSoundCueName);
					}
					this._currentEvent.EventViewed = true;
				}
				else
				{
					TurnEventUI.SyncTurnEventWidget(base.App.Game, "turnEventWidget", null);
					this._currentEvent = null;
				}
			}
			base.App.UI.SetVisible(base.App.UI.Path(new string[]
			{
				"turnEventNext",
				"newEventFlash"
			}), base.App.Game.TurnEvents.Any((TurnEvent x) => !x.EventViewed));
		}
		private void ShowEventDialog(TurnEvent turnEvent)
		{
			if (GameSession.SimAITurns > 0)
			{
				return;
			}
			if (turnEvent.EventType == TurnEventType.EV_COMBAT_WIN || turnEvent.EventType == TurnEventType.EV_COMBAT_LOSS || turnEvent.EventType == TurnEventType.EV_COMBAT_DRAW)
			{
				base.App.UI.CreateDialog(new DialogPostCombat(base.App, turnEvent.SystemID, turnEvent.CombatID, turnEvent.TurnNumber, "postCombat"), null);
			}
			if (turnEvent.EventType == TurnEventType.EV_SUULKA_ARRIVES)
			{
				this._suulkaArrivalDialog = base.App.UI.CreateDialog(new SuulkaArrivalDialog(base.App, turnEvent.SystemID), null);
				return;
			}
			if (turnEvent.EventType == TurnEventType.EV_REQUEST_REQUESTED)
			{
				this._requestRequestedDialog = new RequestRequestedDialog(base.App, base.App.GameDatabase.GetRequestInfo(turnEvent.TreatyID), "dialogRequested");
				base.App.UI.CreateDialog(this._requestRequestedDialog, null);
				return;
			}
			if (turnEvent.EventType == TurnEventType.EV_REQUEST_ACCEPTED)
			{
				this._requestAcceptedDialog = new GenericTextDialog(base.App, App.Localize("@UI_DIPLOMACY_REQUEST_ACCEPTED_TITLE"), turnEvent.GetEventMessage(base.App.Game), "dialogGenericMessage");
				base.App.UI.CreateDialog(this._requestAcceptedDialog, null);
				return;
			}
			if (turnEvent.EventType == TurnEventType.EV_REQUEST_DECLINED)
			{
				this._requestDeclinedDialog = new GenericTextDialog(base.App, App.Localize("@UI_DIPLOMACY_REQUEST_DECLINED_TITLE"), turnEvent.GetEventMessage(base.App.Game), "dialogGenericMessage");
				base.App.UI.CreateDialog(this._requestDeclinedDialog, null);
				return;
			}
			if (turnEvent.EventType == TurnEventType.EV_DEMAND_REQUESTED)
			{
				this._demandRequestedDialog = new DemandRequestedDialog(base.App, base.App.GameDatabase.GetDemandInfo(turnEvent.TreatyID), "dialogRequested");
				base.App.UI.CreateDialog(this._demandRequestedDialog, null);
				return;
			}
			if (turnEvent.EventType == TurnEventType.EV_DEMAND_ACCEPTED)
			{
				this._demandAcceptedDialog = new GenericTextDialog(base.App, App.Localize("@UI_DIPLOMACY_DEMAND_ACCEPTED_TITLE"), turnEvent.GetEventMessage(base.App.Game), "dialogGenericMessage");
				base.App.UI.CreateDialog(this._demandAcceptedDialog, null);
				return;
			}
			if (turnEvent.EventType == TurnEventType.EV_DEMAND_DECLINED)
			{
				this._demandDeclinedDialog = new GenericTextDialog(base.App, App.Localize("@UI_DIPLOMACY_DEMAND_DECLINED_TITLE"), turnEvent.GetEventMessage(base.App.Game), "dialogGenericMessage");
				base.App.UI.CreateDialog(this._demandDeclinedDialog, null);
				return;
			}
			if (turnEvent.EventType == TurnEventType.EV_TREATY_REQUESTED)
			{
				List<TreatyInfo> source = base.App.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>();
				TreatyInfo treatyInfo = source.FirstOrDefault((TreatyInfo x) => x.ID == turnEvent.TreatyID);
				if (treatyInfo != null)
				{
					if (treatyInfo.Type == TreatyType.Limitation)
					{
						this._treatyRequestedDialog = new LimitationTreatyRequestedDialog(base.App, turnEvent.TreatyID);
						base.App.UI.CreateDialog(this._treatyRequestedDialog, null);
						return;
					}
					this._treatyRequestedDialog = new TreatyRequestedDialog(base.App, turnEvent.TreatyID);
					base.App.UI.CreateDialog(this._treatyRequestedDialog, null);
					return;
				}
			}
			else
			{
				if (turnEvent.EventType == TurnEventType.EV_TREATY_ACCEPTED)
				{
					this._treatyAcceptedDialog = new GenericTextDialog(base.App, turnEvent.GetEventName(base.App.Game), turnEvent.GetEventMessage(base.App.Game), "dialogGenericMessage");
					base.App.UI.CreateDialog(this._treatyAcceptedDialog, null);
					return;
				}
				if (turnEvent.EventType == TurnEventType.EV_TREATY_DECLINED)
				{
					this._treatyDeclinedDialog = new GenericTextDialog(base.App, turnEvent.GetEventName(base.App.Game), turnEvent.GetEventMessage(base.App.Game), "dialogGenericMessage");
					base.App.UI.CreateDialog(this._treatyDeclinedDialog, null);
					return;
				}
				if (turnEvent.EventType == TurnEventType.EV_TREATY_EXPIRED)
				{
					this._treatyExpiredDialog = new GenericTextDialog(base.App, turnEvent.GetEventName(base.App.Game), turnEvent.GetEventMessage(base.App.Game), "dialogGenericMessage");
					base.App.UI.CreateDialog(this._treatyExpiredDialog, null);
					return;
				}
				if (turnEvent.EventType == TurnEventType.EV_TREATY_BROKEN_OFFENDER)
				{
					this._treatyBrokenDialogOffender = new LimitationTreatyBrokenDialog(base.App, turnEvent.TreatyID, true);
					base.App.UI.CreateDialog(this._treatyBrokenDialogOffender, null);
					return;
				}
				if (turnEvent.EventType == TurnEventType.EV_TREATY_BROKEN_VICTIM)
				{
					this._treatyBrokenDialogVictim = new LimitationTreatyBrokenDialog(base.App, turnEvent.TreatyID, false);
					base.App.UI.CreateDialog(this._treatyBrokenDialogVictim, null);
					return;
				}
				if (turnEvent.EventType == TurnEventType.EV_COLONY_SELF_SUFFICIENT)
				{
					FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(turnEvent.FleetID);
					if (fleetInfo != null)
					{
						this._selfSufficientDialog = new ColonySelfSufficientDialog(base.App, turnEvent.OrbitalID, turnEvent.MissionID);
						base.App.UI.CreateDialog(this._selfSufficientDialog, null);
						return;
					}
					this.ShowNextEvent(false);
					return;
				}
				else
				{
					if (turnEvent.EventType == TurnEventType.EV_COLONY_ESTABLISHED)
					{
						this._colonyEstablishedPlanet = turnEvent.OrbitalID;
						this._colonyEstablishedSystem = turnEvent.SystemID;
						this._dialogState = ESMDialogState.ESMD_ColonyEstablished;
						this._colonizeDialog = new ColonizeDialog(base.App, turnEvent.OrbitalID, false);
						base.App.UI.CreateDialog(this._colonizeDialog, null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_STATION_BUILT && base.App.GameDatabase.GetOrbitalObjectInfo(turnEvent.OrbitalID) != null)
					{
						this._stationDialog = new StationBuiltDialog(base.App, turnEvent.OrbitalID);
						base.App.UI.CreateDialog(this._stationDialog, null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_RESEARCH_COMPLETE)
					{
						this._researchCompleteDialog = base.App.UI.CreateDialog(new ResearchCompleteDialog(base.App, turnEvent.TechID), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_SALVAGE_PROJECT_COMPLETE)
					{
						this._researchCompleteDialog = base.App.UI.CreateDialog(new SalvageCompleteDialog(base.App, turnEvent.TechID), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_FEASIBILITY_STUDY_COMPLETE)
					{
						this._feasibilityCompleteDialog = base.App.UI.CreateDialog(new FeasibilityCompleteDialog(base.App, turnEvent), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_SYSTEM_SURVEYED)
					{
						this._surveyDialog = base.App.UI.CreateDialog(new SystemSurveyDialog(base.App, turnEvent.SystemID, turnEvent.FleetID), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_SURVEY_INDEPENDENT_RACE_FOUND)
					{
						base.App.UI.CreateDialog(new IndependentFoundDialog(base.App, turnEvent.SystemID, turnEvent.ColonyID, turnEvent.TargetPlayerID), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_SCRIPT_MESSAGE)
					{
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_NO_RESEARCH)
					{
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_SUPERWORLD_COMPLETE)
					{
						this._superWorldDialog = base.App.UI.CreateDialog(new SuperWorldDialog(base.App, turnEvent.ColonyID), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_HIGHEST_TRADE_SYSTEM)
					{
						base.App.UI.CreateDialog(new DialogSystemIntel(base.App, turnEvent.SystemID, base.App.GameDatabase.GetPlayerInfo(turnEvent.TargetPlayerID), turnEvent.GetEventMessage(base.App.Game)), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_NEWEST_COLONY_SYSTEM)
					{
						base.App.UI.CreateDialog(new DialogSystemIntel(base.App, turnEvent.SystemID, base.App.GameDatabase.GetPlayerInfo(turnEvent.TargetPlayerID), turnEvent.GetEventMessage(base.App.Game)), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_RANDOM_SYSTEM)
					{
						base.App.UI.CreateDialog(new DialogSystemIntel(base.App, turnEvent.SystemID, base.App.GameDatabase.GetPlayerInfo(turnEvent.TargetPlayerID), turnEvent.GetEventMessage(base.App.Game)), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_CRITICAL_SUCCESS)
					{
						base.App.UI.CreateDialog(new IntelCriticalSuccessDialog(base.App.Game, turnEvent.TargetPlayerID), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_INTEL_MISSION_CURRENT_TECH)
					{
						base.App.UI.CreateDialog(new DialogTechIntel(base.App, turnEvent.TechID, base.App.GameDatabase.GetPlayerInfo(turnEvent.TargetPlayerID)), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_VN_HW_DEFEATED)
					{
						base.App.UI.CreateDialog(new GenericTextDialog(base.App, App.Localize("@UI_VN_HOMEWORLD_DEFEATED"), turnEvent.GetEventMessage(base.App.Game), "dialogGenericMessage"), null);
						return;
					}
					if (turnEvent.EventType == TurnEventType.EV_COUNTER_INTEL_CRITICAL_SUCCESS || turnEvent.EventType == TurnEventType.EV_COUNTER_INTEL_SUCCESS)
					{
						IntelMissionInfo intelInfo = base.App.GameDatabase.GetIntelInfo(int.Parse(turnEvent.Param1));
						if (intelInfo.MissionType == IntelMission.CurrentResearch)
						{
							base.App.UI.CreateDialog(new CounterIntelSelectTechDialog(base.App.Game, intelInfo.ID), null);
							return;
						}
						if (intelInfo.MissionType == IntelMission.HighestTradeSystem || intelInfo.MissionType == IntelMission.NewestColonySystem || intelInfo.MissionType == IntelMission.RandomSystem)
						{
							base.App.UI.CreateDialog(new CounterIntelSelectSystemDialog(base.App.Game, intelInfo.ID), null);
							return;
						}
					}
					else
					{
						if (turnEvent.EventType == TurnEventType.EV_SUPER_NOVA_TURN || turnEvent.EventType == TurnEventType.EV_SUPER_NOVA_DESTROYED_SYSTEM)
						{
							base.App.UI.CreateDialog(new DialogSuperNova(base.App, turnEvent.Param1, turnEvent.ArrivalTurns, turnEvent.NumShips), null);
							return;
						}
						if (turnEvent.EventType == TurnEventType.EV_NEUTRON_STAR_NEARBY)
						{
							base.App.UI.CreateDialog(new DialogNeutronStar(base.App, turnEvent.NumShips), null);
						}
					}
				}
			}
		}
		private bool SelectedSystemHasFriendlyColonyScreen()
		{
			int selectedSystem = this.SelectedSystem;
			return selectedSystem != 0 && base.App.GameDatabase.GetPlayerColonySystemIDs(base.App.LocalPlayer.ID).Contains(selectedSystem);
		}
		public static bool SystemHasIndependentColony(GameSession App, int system)
		{
			if (system == 0 || !App.GameDatabase.IsSurveyed(App.LocalPlayer.ID, system))
			{
				return false;
			}
			List<ColonyInfo> list = App.GameDatabase.GetColonyInfosForSystem(system).ToList<ColonyInfo>();
			foreach (ColonyInfo current in list)
			{
				PlayerInfo playerInfo = App.GameDatabase.GetPlayerInfo(current.PlayerID);
				Faction faction = App.AssetDatabase.GetFaction(playerInfo.FactionID);
				if (faction.IsIndependent())
				{
					return true;
				}
			}
			return false;
		}
		private bool CanOpenBuildScreen()
		{
			return this.SelectedSystemHasFriendlyColonyScreen();
		}
		private bool CanOpenRepairScreen()
		{
			return true;
		}
		private void OpenBuildScreen()
		{
			if (!this.CanOpenBuildScreen())
			{
				return;
			}
			base.App.SwitchGameState<BuildScreenState>(new object[]
			{
				this.SelectedSystem
			});
		}
		private void OpenDesignScreen()
		{
			base.App.SwitchGameState<DesignScreenState>(new object[]
			{
				false,
				"StarMapState"
			});
		}
		private void OpenBattleRiderManagerScreen()
		{
			base.App.SwitchGameState<RiderManagerState>(new object[]
			{
				this.SelectedSystem
			});
		}
		private void OpenDiplomacyScreen()
		{
			base.App.SwitchGameState<DiplomacyScreenState>(new object[0]);
		}
		private void OpenSystemView()
		{
			int selectedSystem = this.SelectedSystem;
			int selectedPlanet = this.SelectedPlanet;
			base.App.SwitchGameState<StarSystemState>(new object[]
			{
				selectedSystem,
				selectedPlanet
			});
		}
		private void CacheStar(StarSystemInfo systemInfo)
		{
			if (this._cachedStar != null)
			{
				if (systemInfo == this._cachedStarInfo)
				{
					return;
				}
				base.App.ReleaseObject(this._cachedStar);
				this._cachedStar = null;
			}
			this._cachedStarInfo = systemInfo;
			this._cachedStarReady = false;
			this._cachedStar = StarSystem.CreateStar(base.App, Vector3.Zero, systemInfo, 1f, false);
			this._cachedStar.PostSetProp("AutoDraw", false);
		}
		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (this._cachedPlanet != null)
			{
				if (PlanetInfo.AreSame(planetInfo, this._cachedPlanetInfo))
				{
					return;
				}
				base.App.ReleaseObject(this._cachedPlanet);
				this._cachedPlanet = null;
			}
			this._cachedPlanetInfo = planetInfo;
			this._cachedPlanetReady = false;
			this._cachedPlanet = StarSystem.CreatePlanet(base.App.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, StarSystem.TerrestrialPlanetQuality.High);
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
					this.CachePlanet(base.App.GameDatabase.GetPlanetInfo(orbitId));
					result = this._cachedPlanet;
				}
				else
				{
					this.CacheStar(base.App.GameDatabase.GetStarSystemInfo(systemId));
					result = this._cachedStar;
				}
			}
			return result;
		}
		public void ClearSelectedObject()
		{
			this.SelectedObject = null;
		}
		public void RefreshMission()
		{
			this.SyncFleetArrows();
		}
		public void RefreshStarmap(StarMapState.StarMapRefreshType refreshtype = StarMapState.StarMapRefreshType.REFRESH_NORMAL)
		{
			EmpireBarUI.SyncTitleBar(base.App, "gameEmpireBar", this._piechart);
			base.App.UI.SetPropertyString("turn_count", "text", string.Format("{0} {1}", App.Localize("@UI_GENERAL_TURN"), base.App.GameDatabase.GetTurnCount()));
			if (this._TurnLastUpdated != base.App.GameDatabase.GetTurnCount() || refreshtype == StarMapState.StarMapRefreshType.REFRESH_ALL)
			{
				this._starmap.Sync(this._crits);
				this.SyncFleetArrows();
				this._TurnLastUpdated = base.App.GameDatabase.GetTurnCount();
			}
			this._planetWidget.Sync(this._selectedPlanet, false, false);
			StarMapState.UpdateGateUI(base.App.Game, "gameGateInfo");
			StarMapState.UpdateNPGUI(base.App.Game, "gameNGPInfo");
			this.RefreshEmpireBarColor();
			this.RefreshTechCube();
			int count = base.App.Game.GetUpgradableStations(base.App.GameDatabase.GetStationInfosByPlayerID(base.App.LocalPlayer.ID).ToList<StationInfo>()).Count;
			base.App.UI.SetText("numStationsReady", count.ToString());
			base.App.UI.SetVisible("numStationsReady", count > 0);
			if (count > this._prevNumStations)
			{
				base.App.UI.SetPropertyBool("gameStationManagerButton", "flashing", true);
			}
			else
			{
				base.App.UI.SetPropertyBool("gameStationManagerButton", "flashing", false);
			}
			this._prevNumStations = count;
			this.RefreshSystemInterface();
			App.Log.Warn("Starmap refreshed.", "state");
		}
		private void RefreshTechCube()
		{
			if (this._techCube != null)
			{
				float spinSpeed = (1f - base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).RateGovernmentResearch) * 100f * 0.002f;
				this._techCube.SpinSpeed = spinSpeed;
				this._techCube.UpdateResearchProgress();
				this._techCube.RefreshResearchingTech();
			}
			if (base.App.GameDatabase.GetPlayerResearchingTechID(base.App.LocalPlayer.ID) == 0 && base.App.GameDatabase.GetPlayerFeasibilityStudyTechId(base.App.LocalPlayer.ID) == 0)
			{
				base.App.UI.SetPropertyBool("gameResearchButton", "flashing", true);
			}
			else
			{
				base.App.UI.SetPropertyBool("gameResearchButton", "flashing", false);
			}
			if (base.App.LocalPlayer.Faction.Name == "loa" && base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).Savings < 0.0)
			{
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					"gameResearchButton",
					"warning"
				}), true);
				base.App.UI.SetEnabled("gameEmpireResearchSlider", false);
			}
			else
			{
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					"gameResearchButton",
					"warning"
				}), false);
				base.App.UI.SetEnabled("gameEmpireResearchSlider", true);
			}
			this.UpdateTechCubeToolTip();
		}
		private void UpdateTechCubeToolTip()
		{
			string str = App.Localize("@UI_RESEARCH_RESEARCHING");
			bool flag = true;
			int num = base.App.GameDatabase.GetPlayerResearchingTechID(base.App.LocalPlayer.ID);
			if (num == 0)
			{
				num = base.App.GameDatabase.GetPlayerFeasibilityStudyTechId(base.App.LocalPlayer.ID);
				str = App.Localize("@UI_RESEARCH_STUDYING");
				flag = false;
			}
			string techIdStr = base.App.GameDatabase.GetTechFileID(num);
			PlayerTechInfo playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, num);
			Tech tech = base.App.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Tech x) => x.Id == techIdStr);
			if (tech != null && playerTechInfo != null)
			{
				string str2 = "";
				if (flag)
				{
					str2 = " -  " + ResearchScreenState.GetTurnsToCompleteString(base.App, base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID), playerTechInfo);
				}
				base.App.UI.SetTooltip("researchCubeButton", str + " " + tech.Name + str2);
				return;
			}
			base.App.UI.SetTooltip("researchCubeButton", App.Localize("@UI_TOOLTIP_RESEARCHCUBE"));
		}
		private void RefreshEmpireBarColor()
		{
			Vector3 primaryColor = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).PrimaryColor;
			Vector3 secondaryColor = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).SecondaryColor;
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"LC"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"RC"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"BG"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"RC2"
			}), "color", secondaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.TLC"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.TRC"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BLC"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BRC"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BG1"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BG2"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BG3"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BG4"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BG5"
			}), "color", primaryColor * 0.5f);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BOL1"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BOL2"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BOL3"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BOL4"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BOL5"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BOL6"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BOL7"
			}), "color", primaryColor);
			base.App.UI.SetPropertyColorNormalized(base.App.UI.Path(new string[]
			{
				"gameEmpireBar",
				"boxback.BOL8"
			}), "color", primaryColor);
		}
		public void SetEndTurnTimeout(int delay)
		{
			base.App.UI.SetPropertyInt(base.App.UI.Path(new string[]
			{
				"gameEndTurnButton"
			}), "timeout", delay);
		}
		public void SetOffsetViewMode(bool enabled)
		{
			base.App.UI.SetVisible("OH_StarMap", !enabled);
			base.App.UI.SetVisible("OH_StarMap_Offset", enabled);
		}
		protected override void OnEnter()
		{
			base.App.UI.UnlockUI();
			base.App.UI.SetScreen("StarMap");
			base.App.UI.Send(new object[]
			{
				"SetGameObject",
				"OH_StarMap",
				this._starmap.ObjectID
			});
			base.App.UI.Send(new object[]
			{
				"SetGameObject",
				"OH_StarMap_Offset",
				this._starmap.ObjectID
			});
			this.RefreshCameraControl();
			this.SetProvinceMode(false);
			this.PopulateViewFilterList();
			if (base.App.UserProfile.AutoPlaceDefenseAssets != base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).AutoPlaceDefenseAssets)
			{
				base.App.GameDatabase.UpdatePlayerAutoPlaceDefenses(base.App.LocalPlayer.ID, base.App.UserProfile.AutoPlaceDefenseAssets);
			}
			if (base.App.UserProfile.AutoRepairFleets != base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).AutoRepairShips)
			{
				base.App.GameDatabase.UpdatePlayerAutoRepairFleets(base.App.LocalPlayer.ID, base.App.UserProfile.AutoRepairFleets);
			}
			if (base.App.UserProfile.AutoUseGoop != base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).AutoUseGoopModules)
			{
				base.App.GameDatabase.UpdatePlayerAutoUseGoop(base.App.LocalPlayer.ID, base.App.UserProfile.AutoUseGoop);
			}
			if (base.App.UserProfile.AutoUseJoker != base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).AutoUseJokerModules)
			{
				base.App.GameDatabase.UpdatePlayerAutoUseJoker(base.App.LocalPlayer.ID, base.App.UserProfile.AutoUseJoker);
			}
			if (base.App.UserProfile.AutoAOE != base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).AutoAoe)
			{
				base.App.GameDatabase.UpdatePlayerAutoUseAOE(base.App.LocalPlayer.ID, base.App.UserProfile.AutoAOE);
			}
			if (base.App.UserProfile.AutoPatrol != base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).AutoPatrol)
			{
				base.App.GameDatabase.UpdatePlayerAutoPatrol(base.App.LocalPlayer.ID, base.App.UserProfile.AutoPatrol);
			}
			if (!base.App.Game.HomeworldNamed && base.App.GameDatabase.GetTurnCount() == 1)
			{
				int? num = new int?(base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).Homeworld.Value);
				if (num.HasValue)
				{
					this._eventDialogShown = true;
					this._colonizeDialog = new ColonizeDialog(base.App, num.Value, true);
					base.App.UI.CreateDialog(this._colonizeDialog, null);
					base.App.Game.HomeworldNamed = true;
					this._dialogState = ESMDialogState.ESMD_None;
					this.SetSelectedSystem(base.App.GameDatabase.GetPlayerHomeworld(base.App.LocalPlayer.ID).SystemID, false);
				}
			}
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			EmpireBarUI.SyncTitleBar(base.App, "gameEmpireBar", this._piechart);
			EmpireBarUI.SyncTitleFrame(base.App);
			base.App.UI.SetPropertyBool("gameDesignButton", "lockout_button", true);
			base.App.UI.SetPropertyBool("gameResearchButton", "lockout_button", true);
			base.App.UI.SetPropertyBool("gameStationManagerButton", "lockout_button", true);
			base.App.UI.SetPropertyBool("gamePlanetSummaryButton", "lockout_button", true);
			base.App.UI.SetPropertyBool("gamePopulationManagerButton", "lockout_button", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyDetailsWidget",
				"partTradeSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyDetailsWidget",
				"partTerraSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyDetailsWidget",
				"partInfraSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyDetailsWidget",
				"partShipConSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyDetailsWidget",
				"partCivSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colonyDetailsWidget",
				"partOverDevelopment"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colony_event_dialog",
				"partTradeSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colony_event_dialog",
				"partTerraSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colony_event_dialog",
				"partInfraSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colony_event_dialog",
				"partShipConSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colony_event_dialog",
				"partCivSlider"
			}), "only_user_events", true);
			base.App.UI.SetPropertyBool(base.App.UI.Path(new string[]
			{
				"colony_event_dialog",
				"partOverDevelopment"
			}), "only_user_events", true);
			this.SetEndTurnTimeout(base.App.GameSettings.EndTurnDelay);
			this.EnableFleetCheck = base.App.GameSettings.CheckForInactiveFleets;
			PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
			if (this.SelectedSystem == 0 && playerInfo.Homeworld.HasValue)
			{
				OrbitalObjectInfo orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(playerInfo.Homeworld.Value);
				this.SelectedObject = base.App.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID);
				this.SetSelectedSystem(orbitalObjectInfo.StarSystemID, false);
			}
			this._crits.Activate();
			EmpireBarUI.SyncTitleBar(base.App, "gameEmpireBar", this._piechart);
			base.App.PostPlayMusic(string.Format("Ambient_{0}", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID))));
			if (ScriptHost.AllowConsole)
			{
				base.App.UI.SetVisible("debugTestCombatButton", true);
			}
			else
			{
				base.App.UI.SetVisible("debugTestCombatButton", false);
			}
			base.App.Network.EnableChatWidgetPlayerList(true);
			base.App.UI.SetSelection("viewModeDropdown", (int)this._lastFilterSelection);
			this._starmap.ViewFilter = this._lastFilterSelection;
			if (!this._playerWidget.Initialized)
			{
				this._playerWidget.Initialize();
			}
			if (base.App.PreviousState == base.App.GetGameState<CombatState>() || base.App.PreviousState == base.App.GetGameState<SimCombatState>())
			{
				PendingCombat currentCombat = base.App.Game.GetCurrentCombat();
				if (base.App.GameSetup.IsMultiplayer && currentCombat != null)
				{
					base.App.Network.CombatComplete(currentCombat.ConflictID);
				}
				base.App.Game.CombatComplete();
			}
			if (base.App.Game.TurnEvents.Count == 0)
			{
				base.App.Game.TurnEvents = base.App.GameDatabase.GetTurnEventsByTurnNumber(base.App.GameDatabase.GetTurnCount() - 1, base.App.LocalPlayer.ID).OrderByDescending(delegate(TurnEvent x)
				{
					if (!x.ShowsDialog)
					{
						return 0;
					}
					if (!x.IsCombatEvent)
					{
						return 1;
					}
					return 2;
				}).ToList<TurnEvent>();
				for (int i = 0; i < base.App.Game.TurnEvents.Count<TurnEvent>() - 1; i++)
				{
					base.App.Game.TurnEvents[i].dialogShown = true;
					if (!base.App.Game.TurnEvents[i].ShowsDialog && base.App.Game.TurnEvents[i + 1].ShowsDialog)
					{
						TurnEvent value = base.App.Game.TurnEvents[i + 1];
						base.App.Game.TurnEvents[i + 1] = base.App.Game.TurnEvents[i];
						base.App.Game.TurnEvents[i] = value;
						i = -1;
					}
				}
			}
			if (base.App.Game.State == SimState.SS_PLAYER)
			{
				this.TurnStarted();
			}
			else
			{
				this.EnableNextTurnButton(false);
			}
			this.RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
			FleetUI.SyncFleetAndPlanetListWidget(base.App.Game, "fleetAndPlanetDetailsWidget", this.SelectedSystem, true);
			EncounterDialog._starmap = this._starmap;
			base.App.HotKeyManager.AddListener(this);
		}
		public void Reset()
		{
			base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._piechart = null;
			FleetWidget expr_29 = this._fleetWidget;
			expr_29.OnFleetSelectionChanged = (FleetWidget.FleetSelectionChangedDelegate)Delegate.Remove(expr_29.OnFleetSelectionChanged, new FleetWidget.FleetSelectionChangedDelegate(this.OnFleetWidgetFleetSelected));
			this._fleetWidget.Dispose();
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
			}
			this._painter.Dispose();
			this._starmap = null;
			this._planetView = null;
			if (this._cachedPlanet != null)
			{
				base.App.ReleaseObject(this._cachedPlanet);
			}
			this._cachedPlanet = null;
			this._cachedPlanetReady = false;
			if (this._cachedStar != null)
			{
				base.App.ReleaseObject(this._cachedStar);
			}
			this._cachedStar = null;
			this._cachedStarReady = false;
			this._playerWidget.Terminate();
			this._planetWidget.Terminate();
			foreach (OverlayMission current in this._missionOverlays.Values)
			{
				base.App.UI.CloseDialog(current, true);
			}
			this._missionOverlays = null;
			base.App.UI.CloseDialog(this._reactionOverlay, true);
			this._reactionOverlay = null;
			if (this._starmap != null)
			{
				this._starmap.ViewFilter = StarMapViewFilter.VF_STANDARD;
			}
			this._lastFilterSelection = StarMapViewFilter.VF_STANDARD;
			this._TurnLastUpdated = -1;
			base.App.UI.DestroyPanel(this._contextMenuID);
			base.App.UI.DestroyPanel(this._researchContextID);
			base.App.UI.DestroyPanel(this._fleetContextMenuID);
			base.App.UI.DestroyPanel(this._enemyContextMenuID);
			base.App.UI.DestroyPanel(this._enemyGMStationContextMenuID);
			base.App.UI.DeleteScreen("StarMap");
			base.App.UI.PurgeFleetWidgetCache();
			this._initialized = false;
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			if (this._initialized)
			{
				EncounterDialog._starmap = null;
				this._crits.Deactivate();
				base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
				if (this._cachedPlanet != null)
				{
					this._cachedPlanet.Active = false;
				}
				this._cachedPlanetReady = false;
				if (this._cachedStar != null)
				{
					this._cachedStar.Active = false;
				}
				this._cachedStarReady = false;
				foreach (OverlayMission current in this._missionOverlays.Values)
				{
					base.App.UI.CloseDialog(current, true);
				}
			}
		}
		protected override void OnUpdate()
		{
			if (this._colonizeDialog != null)
			{
				this._colonizeDialog.Update();
			}
			this._planetWidget.Update();
			this.UpdateCachedPlanet();
			this.UpdateCachedStar();
			this._playerWidget.Sync();
			this._fleetWidget.OnUpdate();
			if (this._painter.ObjectStatus == GameObjectStatus.Ready && !this._painter.Active)
			{
				this._painter.Active = true;
				this._starmap.PostObjectAddObjects(new IGameObject[]
				{
					this._painter
				});
			}
			if (GameSession.SimAITurns > 0 && this._simNewTurnTick > 0)
			{
				this._simNewTurnTick--;
				if (this._simNewTurnTick <= 0)
				{
					this.EndTurn(false);
				}
			}
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(base.Name))
			{
				if (!this._missionOverlays.Any((KeyValuePair<string, OverlayMission> x) => x.Value.GetShown()) && base.App.UI.GetTopDialog() == null)
				{
					switch (action)
					{
					case HotKeyManager.HotKeyActions.State_Starmap:
						base.App.UI.LockUI();
						base.App.SwitchGameState<StarMapState>(new object[0]);
						return true;
					case HotKeyManager.HotKeyActions.State_BuildScreen:
						if (this.SelectedSystem != 0 && this.CanOpenBuildScreen())
						{
							base.App.UI.LockUI();
							base.App.SwitchGameState<BuildScreenState>(new object[]
							{
								this.SelectedSystem
							});
							return true;
						}
						return false;
					case HotKeyManager.HotKeyActions.State_DesignScreen:
						base.App.UI.LockUI();
						this.OpenDesignScreen();
						return true;
					case HotKeyManager.HotKeyActions.State_ResearchScreen:
						base.App.UI.LockUI();
						base.App.SwitchGameState<ResearchScreenState>(new object[0]);
						return true;
					case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
						if (base.App.GameDatabase.GetStratModifier<bool>(StratModifiers.ComparativeAnalysys, base.App.LocalPlayer.ID))
						{
							if (base.App.GameDatabase.GetDesignsEncountered(base.App.LocalPlayer.ID).Any((DesignInfo x) => x.Class != ShipClass.Station))
							{
								base.App.UI.LockUI();
								base.App.SwitchGameState<ComparativeAnalysysState>(new object[]
								{
									false,
									"StarMapState"
								});
							}
						}
						return true;
					case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
						base.App.UI.LockUI();
						base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
						return true;
					case HotKeyManager.HotKeyActions.State_SotspediaScreen:
						base.App.UI.LockUI();
						base.App.SwitchGameState<SotspediaState>(new object[0]);
						return true;
					case HotKeyManager.HotKeyActions.State_StarSystemScreen:
						if (this.SelectedSystem != 0)
						{
							base.App.UI.LockUI();
							base.App.SwitchGameState<StarSystemState>(new object[]
							{
								this.SelectedSystem,
								this.SelectedPlanet
							});
							return true;
						}
						return false;
					case HotKeyManager.HotKeyActions.State_FleetManagerScreen:
						if (this.SelectedSystem != 0 && this.CanOpenFleetManager(this.SelectedSystem))
						{
							base.App.UI.LockUI();
							base.App.SwitchGameState<FleetManagerState>(new object[]
							{
								this.SelectedSystem
							});
							return true;
						}
						return false;
					case HotKeyManager.HotKeyActions.State_DefenseManagerScreen:
						if (this.SelectedSystem != 0 && GameSession.PlayerPresentInSystem(base.App.GameDatabase, base.App.LocalPlayer.ID, this.SelectedSystem))
						{
							base.App.UI.LockUI();
							base.App.SwitchGameState<DefenseManagerState>(new object[]
							{
								this.SelectedSystem
							});
							return true;
						}
						return false;
					case HotKeyManager.HotKeyActions.State_BattleRiderScreen:
						if (this.SelectedSystem != 0 && this.CanOpenRiderManager() && base.App.GameDatabase.PlayerHasTech(base.App.LocalPlayer.ID, "BRD_BattleRiders"))
						{
							base.App.UI.LockUI();
							base.App.SwitchGameState<RiderManagerState>(new object[]
							{
								this.SelectedSystem
							});
							return true;
						}
						return false;
					case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
						base.App.UI.LockUI();
						base.App.SwitchGameState<DiplomacyScreenState>(new object[0]);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_EndTurn:
						this.EndTurn(false);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NextFleet:
					{
						List<FleetInfo> list = (
							from x in base.App.GameDatabase.GetFleetInfosByPlayerID(base.App.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_CARAVAN)
							where x.SystemID == 0
							select x).ToList<FleetInfo>();
						list = (
							from x in list
							orderby x.Name
							select x).ToList<FleetInfo>();
						if (list.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							if (!list.Any((FleetInfo x) => x.ID == sel) || list.Last<FleetInfo>().ID == sel)
							{
								sel = list.First<FleetInfo>().ID;
							}
							else
							{
								sel = list[list.IndexOf(list.FirstOrDefault((FleetInfo x) => x.ID == sel)) + 1].ID;
							}
							this.SetSelectedFleet(sel);
						}
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_LastFleet:
					{
						List<FleetInfo> list2 = (
							from x in base.App.GameDatabase.GetFleetInfosByPlayerID(base.App.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_CARAVAN)
							where x.SystemID == 0
							select x).ToList<FleetInfo>();
						list2 = (
							from x in list2
							orderby x.Name descending
							select x).ToList<FleetInfo>();
						if (list2.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							if (!list2.Any((FleetInfo x) => x.ID == sel) || list2.Last<FleetInfo>().ID == sel)
							{
								sel = list2.First<FleetInfo>().ID;
							}
							else
							{
								sel = list2[list2.IndexOf(list2.FirstOrDefault((FleetInfo x) => x.ID == sel)) + 1].ID;
							}
							this.SetSelectedFleet(sel);
						}
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_NextIdleFleet:
					{
						List<FleetInfo> list3 = (
							from x in base.App.GameDatabase.GetFleetInfosByPlayerID(base.App.LocalPlayer.ID, FleetType.FL_NORMAL)
							where x.SystemID != 0 && base.App.GameDatabase.GetMissionByFleetID(x.ID) == null
							select x).ToList<FleetInfo>();
						list3 = (
							from x in list3
							orderby x.Name
							select x).ToList<FleetInfo>();
						if (list3.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							if (!list3.Any((FleetInfo x) => x.ID == sel) || list3.Last<FleetInfo>().ID == sel)
							{
								sel = list3.First<FleetInfo>().ID;
							}
							else
							{
								sel = list3[list3.IndexOf(list3.FirstOrDefault((FleetInfo x) => x.ID == sel)) + 1].ID;
							}
							this.SetSelectedFleet(sel);
						}
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_LastIdleFleet:
					{
						List<FleetInfo> list4 = (
							from x in base.App.GameDatabase.GetFleetInfosByPlayerID(base.App.LocalPlayer.ID, FleetType.FL_NORMAL)
							where x.SystemID != 0 && base.App.GameDatabase.GetMissionByFleetID(x.ID) == null
							select x).ToList<FleetInfo>();
						list4 = (
							from x in list4
							orderby x.Name descending
							select x).ToList<FleetInfo>();
						if (list4.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							if (!list4.Any((FleetInfo x) => x.ID == sel) || list4.Last<FleetInfo>().ID == sel)
							{
								sel = list4.First<FleetInfo>().ID;
							}
							else
							{
								sel = list4[list4.IndexOf(list4.FirstOrDefault((FleetInfo x) => x.ID == sel)) + 1].ID;
							}
							this.SetSelectedFleet(sel);
						}
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_NextSystem:
					{
						List<StarSystemInfo> list5 = (
							from x in base.App.GameDatabase.GetStarSystemInfos()
							where base.App.GameDatabase.GetSystemOwningPlayer(x.ID) == base.App.LocalPlayer.ID
							select x).ToList<StarSystemInfo>();
						list5 = (
							from x in list5
							orderby x.Name
							select x).ToList<StarSystemInfo>();
						if (list5.Any<StarSystemInfo>())
						{
							int sel = this.SelectedSystem;
							if (!list5.Any((StarSystemInfo x) => x.ID == sel) || list5.Last<StarSystemInfo>().ID == sel)
							{
								sel = list5.First<StarSystemInfo>().ID;
							}
							else
							{
								sel = list5[list5.IndexOf(list5.FirstOrDefault((StarSystemInfo x) => x.ID == sel)) + 1].ID;
							}
							this.SetSelectedSystem(sel, false);
						}
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_LastSystem:
					{
						List<StarSystemInfo> list6 = (
							from x in base.App.GameDatabase.GetStarSystemInfos()
							where base.App.GameDatabase.GetSystemOwningPlayer(x.ID) == base.App.LocalPlayer.ID
							select x).ToList<StarSystemInfo>();
						list6 = (
							from x in list6
							orderby x.Name descending
							select x).ToList<StarSystemInfo>();
						if (list6.Any<StarSystemInfo>())
						{
							int sel = this.SelectedSystem;
							if (!list6.Any((StarSystemInfo x) => x.ID == sel) || list6.Last<StarSystemInfo>().ID == sel)
							{
								sel = list6.First<StarSystemInfo>().ID;
							}
							else
							{
								sel = list6[list6.IndexOf(list6.FirstOrDefault((StarSystemInfo x) => x.ID == sel)) + 1].ID;
							}
							this.SetSelectedSystem(sel, false);
						}
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_NextIncomingFleet:
					{
						List<int> list7 = (
							from x in base.App.GameDatabase.GetColonyInfos()
							where x.PlayerID == base.App.LocalPlayer.ID
							select x.CachedStarSystemID).ToList<int>();
						List<FleetInfo> list8 = (
							from x in base.App.GameDatabase.GetFleetInfos(FleetType.FL_NORMAL)
							where x.SystemID == 0 && x.PlayerID != base.App.LocalPlayer.ID && base.App.GameDatabase.GetDiplomacyStateBetweenPlayers(x.PlayerID, base.App.LocalPlayer.ID) == DiplomacyState.WAR
							select x).ToList<FleetInfo>();
						List<FleetInfo> list9 = new List<FleetInfo>();
						foreach (FleetInfo current in list8)
						{
							MoveOrderInfo moveOrderInfoByFleetID = base.App.GameDatabase.GetMoveOrderInfoByFleetID(current.ID);
							if (moveOrderInfoByFleetID != null && list7.Contains(moveOrderInfoByFleetID.ToSystemID))
							{
								list9.Add(current);
							}
						}
						list9 = (
							from x in list9
							orderby x.Name descending
							select x).ToList<FleetInfo>();
						if (list9.Any<FleetInfo>())
						{
							int sel = this.SelectedFleet;
							if (!list9.Any((FleetInfo x) => x.ID == sel) || list9.Last<FleetInfo>().ID == sel)
							{
								sel = list9.First<FleetInfo>().ID;
							}
							else
							{
								sel = list9[list9.IndexOf(list9.FirstOrDefault((FleetInfo x) => x.ID == sel)) + 1].ID;
							}
							this.SetSelectedFleet(sel);
						}
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_OpenFleetManager:
						if (base.App.UI.GetTopDialog() == null)
						{
							base.App.UI.CreateDialog(new FleetSummaryDialog(base.App, "FleetSummaryDialog"), null);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenPlanetManager:
						if (base.App.UI.GetTopDialog() == null)
						{
							base.App.UI.CreateDialog(new PlanetManagerDialog(base.App, "dialogPlanetManager"), null);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenStationManager:
						if (base.App.UI.GetTopDialog() == null)
						{
							base.App.UI.CreateDialog(new StationManagerDialog(base.App, this, 0, "dialogStationManager"), null);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenRepairScreen:
						if (base.App.UI.GetTopDialog() == null)
						{
							if (this.SelectedSystem != 0)
							{
								base.App.UI.CreateDialog(new RepairShipsDialog(base.App, this.SelectedSystem, base.App.GameDatabase.GetFleetInfoBySystemID(this.SelectedSystem, FleetType.FL_ALL).ToList<FleetInfo>(), "dialogRepairShips"), null);
							}
							else
							{
								if (this.SelectedFleet != 0)
								{
									List<FleetInfo> list10 = new List<FleetInfo>();
									list10.Add(base.App.GameDatabase.GetFleetInfo(this.SelectedFleet));
									base.App.UI.CreateDialog(new RepairShipsDialog(base.App, this.SelectedSystem, list10, "dialogRepairShips"), null);
								}
							}
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenPopulationManager:
						if (base.App.UI.GetTopDialog() == null)
						{
							base.App.UI.CreateDialog(new PopulationManagerDialog(base.App, this.SelectedSystem, "dialogPopulationManager"), null);
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NormalViewFilter:
					{
						StarMapViewFilter starMapViewFilter = StarMapViewFilter.VF_STANDARD;
						this._starmap.ViewFilter = starMapViewFilter;
						this._lastFilterSelection = starMapViewFilter;
						this.RefreshSystemInterface();
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_SurveyViewFilter:
					{
						StarMapViewFilter starMapViewFilter = StarMapViewFilter.VF_SURVEY;
						this._starmap.ViewFilter = starMapViewFilter;
						this._lastFilterSelection = starMapViewFilter;
						this.RefreshSystemInterface();
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_ProvinceFilter:
					{
						StarMapViewFilter starMapViewFilter = StarMapViewFilter.VF_PROVINCE;
						this._starmap.ViewFilter = starMapViewFilter;
						this._lastFilterSelection = starMapViewFilter;
						this.RefreshSystemInterface();
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_SupportRangeFilter:
					{
						StarMapViewFilter starMapViewFilter = StarMapViewFilter.VF_SUPPORT_RANGE;
						this._starmap.ViewFilter = starMapViewFilter;
						this._lastFilterSelection = starMapViewFilter;
						this.RefreshSystemInterface();
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_SensorRangeFilter:
					{
						StarMapViewFilter starMapViewFilter = StarMapViewFilter.VF_SENSOR_RANGE;
						this._starmap.ViewFilter = starMapViewFilter;
						this._lastFilterSelection = starMapViewFilter;
						this.RefreshSystemInterface();
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_TerrainFilter:
					{
						StarMapViewFilter starMapViewFilter = StarMapViewFilter.VF_TERRAIN;
						this._starmap.ViewFilter = starMapViewFilter;
						this._lastFilterSelection = starMapViewFilter;
						this.RefreshSystemInterface();
						return true;
					}
					case HotKeyManager.HotKeyActions.Starmap_TradeViewFilter:
						if (base.App.GetStratModifier<bool>(StratModifiers.EnableTrade, base.App.LocalPlayer.ID))
						{
							StarMapViewFilter starMapViewFilter = StarMapViewFilter.VF_TRADE;
							this._starmap.ViewFilter = starMapViewFilter;
							this._lastFilterSelection = starMapViewFilter;
							this.RefreshSystemInterface();
						}
						return true;
					case HotKeyManager.HotKeyActions.Starmap_NextNewsEvent:
						this.ShowNextEvent(false);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_LastNewsEvent:
						this.ShowNextEvent(true);
						return true;
					case HotKeyManager.HotKeyActions.Starmap_OpenMenu:
						if (base.App.UI.GetTopDialog() == null)
						{
							base.App.UI.CreateDialog(new MainMenuDialog(base.App), null);
						}
						return true;
					}
				}
			}
			return false;
		}
	}
}
