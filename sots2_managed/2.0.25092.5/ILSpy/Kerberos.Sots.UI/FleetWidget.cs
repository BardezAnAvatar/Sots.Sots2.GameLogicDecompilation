using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Performance = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.UI;

namespace Kerberos.Sots.UI
{
	[GameObjectType(InteropGameObjectType.IGOT_FLEETWIDGET)]
	internal class FleetWidget : GameObject, IDisposable
	{
		public delegate void FleetSelectionChangedDelegate(App app, int SelectedFleetId);
		public delegate void FleetsModifiedDelegate(App app, int[] modifiedFleetIds);
		public enum FilterShips
		{
			Enable,
			Disable,
			Ignore
		}
		public delegate FleetWidget.FilterShips FleetWidgetShipFilter(ShipInfo ship, DesignInfo design);
		public delegate FleetWidget.FilterShips FleetWidgetFleetFilter(FleetInfo fleet);
		private App _game;
		public FleetWidget.FleetSelectionChangedDelegate OnFleetSelectionChanged;
		public FleetWidget.FleetsModifiedDelegate OnFleetsModified;
		private ShipTooltip _ShipToolTip;
		private bool _contentChanged;
		private bool _fleetsChanged;
		private bool _planetsChanged;
		private bool _confirmRepairs;
		private bool _repairModeChanged;
		private List<int> _syncedFleets;
		private List<int> _syncedStations;
		private List<int> _syncedPlanets;
		private string _rootName;
		private int _widgetID;
		private bool _ready;
		private bool _expandAll;
		private bool _enabled = true;
		private bool _hasSuulka;
		private List<FleetWidget> _linkedWidgets;
		private bool _ridersEnabled;
		private bool _showColonies;
		private bool _onlyLocalPlayer;
		private bool _listStations;
		private bool _suulkaMode;
		private bool _ShowPiracyFleets;
		private bool _enableAdmiralButton = true;
		private bool _enableRightClick = true;
		private bool _enableMissionButtons;
		private bool _enemySelectionEnabled;
		private bool _separateDefenseFleet = true;
		private bool _DefenseFleetUpdated;
		private int _selectedFleet;
		private bool _shipSelectionEnabled;
		private MissionType _missionMode;
		private bool _showEmptyFleets = true;
		private bool _showFleetInfo = true;
		private bool _showRepairPoints;
		private bool _repairMode;
		private bool _jumboMode;
		private bool _preferredSelectMode;
		private bool _scrapEnabled;
		private bool _repairAll;
		private bool _undoAll;
		private int _selected;
		private FleetWidget _repairWidget;
		private string _contextMenuID;
		private string _shipcontextMenuID;
		private int _contextSlot;
		private string _fleetNameDialog;
		private string _dissolveFleetDialog;
		private string _cancelMissionDialog;
		private string _retrofitShipDialog;
		private static int _nextWidgetID;
		private List<int> AccountedSystems = new List<int>();
		public bool EnableCreateFleetButton;
		private List<int> _shipsToScrap = new List<int>();
		private string _scrapDialog;
		public bool DisableTooltips;
		private string _LoaCubeTransferDialog;
		private static string createfleetpanel;
		private static string admiralPanel;
		private static string SelectCompositionPanel;
		public event FleetWidget.FleetWidgetShipFilter ShipFilter;
		public event FleetWidget.FleetWidgetFleetFilter FleetFilter;
		public bool ContentChanged
		{
			get
			{
				return this._contentChanged;
			}
		}
		public List<int> SyncedFleets
		{
			get
			{
				return this._syncedFleets;
			}
		}
		public List<int> SyncedStations
		{
			get
			{
				return this._syncedStations;
			}
		}
		public List<int> SyncedPlanets
		{
			get
			{
				return this._syncedPlanets;
			}
		}
		public bool RidersEnabled
		{
			get
			{
				return this._ridersEnabled;
			}
			set
			{
				this._ridersEnabled = value;
			}
		}
		public bool ShowColonies
		{
			get
			{
				return this._showColonies;
			}
			set
			{
				this._showColonies = value;
			}
		}
		public bool OnlyLocalPlayer
		{
			get
			{
				return this._onlyLocalPlayer;
			}
			set
			{
				this._onlyLocalPlayer = value;
			}
		}
		public bool ListStations
		{
			get
			{
				return this._listStations;
			}
			set
			{
				this._listStations = value;
			}
		}
		public bool SuulkaMode
		{
			get
			{
				return this._suulkaMode;
			}
			set
			{
				this._suulkaMode = value;
			}
		}
		public bool ShowPiracyFleets
		{
			get
			{
				return this._ShowPiracyFleets;
			}
			set
			{
				this._ShowPiracyFleets = value;
			}
		}
		public bool EnableAdmiralButton
		{
			get
			{
				return this._enableAdmiralButton;
			}
			set
			{
				this._enableAdmiralButton = value;
			}
		}
		public bool EnableRightClick
		{
			get
			{
				return this._enableRightClick;
			}
			set
			{
				this._enableRightClick = value;
			}
		}
		public bool EnableMissionButtons
		{
			get
			{
				return this._enableMissionButtons;
			}
			set
			{
				this._enableMissionButtons = value;
			}
		}
		public bool EnemySelectionEnabled
		{
			get
			{
				return this._enemySelectionEnabled;
			}
			set
			{
				this._enemySelectionEnabled = value;
			}
		}
		public bool SeparateDefenseFleet
		{
			get
			{
				return this._separateDefenseFleet;
			}
			set
			{
				this._separateDefenseFleet = value;
				this.PostSetProp("SeparateDefenseFleet", value);
			}
		}
		public bool DefenseFleetUpdated
		{
			get
			{
				return this._DefenseFleetUpdated;
			}
			set
			{
				this._DefenseFleetUpdated = value;
			}
		}
		public int SelectedFleet
		{
			get
			{
				return this._selectedFleet;
			}
			set
			{
				this._selectedFleet = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public bool ShipSelectionEnabled
		{
			get
			{
				return this._shipSelectionEnabled;
			}
			set
			{
				this._shipSelectionEnabled = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public MissionType MissionMode
		{
			get
			{
				return this._missionMode;
			}
			set
			{
				this._missionMode = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public bool ShowEmptyFleets
		{
			get
			{
				return this._showEmptyFleets;
			}
			set
			{
				this._showEmptyFleets = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public bool ShowFleetInfo
		{
			get
			{
				return this._showFleetInfo;
			}
			set
			{
				this._showFleetInfo = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public bool ShowRepairPoints
		{
			get
			{
				return this._showRepairPoints;
			}
			set
			{
				this._showRepairPoints = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public bool RepairMode
		{
			get
			{
				return this._repairMode;
			}
			set
			{
				this._repairMode = value;
				this._repairModeChanged = true;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public bool JumboMode
		{
			get
			{
				return this._jumboMode;
			}
			set
			{
				this._jumboMode = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public bool PreferredSelectMode
		{
			get
			{
				return this._preferredSelectMode;
			}
			set
			{
				this._preferredSelectMode = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public bool ScrapEnabled
		{
			get
			{
				return this._scrapEnabled;
			}
			set
			{
				this._scrapEnabled = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public int Selected
		{
			get
			{
				return this._selected;
			}
			set
			{
				this._selected = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public FleetWidget RepairWidget
		{
			get
			{
				return this._repairWidget;
			}
			set
			{
				this._repairWidget = value;
				this._contentChanged = true;
				this.Refresh();
			}
		}
		public FleetWidget(App game, string rootList)
		{
			this._game = game;
			game.AddExistingObject(this, new object[]
			{
				rootList,
				FleetWidget._nextWidgetID,
				this._game.LocalPlayer.ID
			});
			this._widgetID = FleetWidget._nextWidgetID;
			FleetWidget._nextWidgetID++;
			this._rootName = rootList;
			this._game.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._game.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			this.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.DefaultShipFilter);
			this._linkedWidgets = new List<FleetWidget>();
			this._contextMenuID = base.App.UI.CreatePanelFromTemplate("FleetContextMenu", null);
			this._ShipToolTip = new ShipTooltip(base.App);
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameRenameFleetButton"
			}), "id", "gameRenameFleetButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameDissolveFleetButton"
			}), "id", "gameDissolveFleetButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameCancelMissionButton"
			}), "id", "gameCancelMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetLoaDissolveToCube"
			}), "id", "gameFleetLoaDissolveToCube|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetSetLoaComposition"
			}), "id", "gameFleetSetLoaComposition|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetSurveyMissionButton"
			}), "id", "gameFleetSurveyMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetColonizeMissionButton"
			}), "id", "gameFleetColonizeMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetEvacuateButton"
			}), "id", "gameFleetEvacuateButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetSuportMissionButton"
			}), "id", "gameFleetSuportMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetRelocateMissionButton"
			}), "id", "gameFleetRelocateMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetPatrolMissionButton"
			}), "id", "gameFleetPatrolMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetInterdictMissionButton"
			}), "id", "gameFleetInterdictMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetInvadeMissionButton"
			}), "id", "gameFleetInvadeMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetStrikeMissionButton"
			}), "id", "gameFleetStrikeMissionButton|" + this._widgetID.ToString());
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._contextMenuID,
				"gameFleetPiracyButton"
			}), "id", "gameFleetPiracyButton|" + this._widgetID.ToString());
			this._shipcontextMenuID = base.App.UI.CreatePanelFromTemplate("FleetShipContextMenu", null);
			base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
			{
				this._shipcontextMenuID,
				"gameRetrofitShip"
			}), "id", "gameRetrofitShip|" + this._widgetID.ToString());
		}
		public void Refresh()
		{
            Performance.FleetWidget perfWidget = new Performance.FleetWidget(this);
            perfWidget.Refresh();

            //if (this._contentChanged && this._ready)
            //{
            //    this.PostSetProp("RefreshEnabled", false);
            //    this.PostSetProp("SetJumboMode", this._jumboMode);
            //    this.PostSetProp("EnemySelectionEnabled", this._enemySelectionEnabled);
            //    this.PostSetProp("SetScrapEnabled", this._scrapEnabled);
            //    if (this._missionMode != MissionType.NO_MISSION)
            //    {
            //        this.SetMissionMode(true);
            //    }
            //    else
            //    {
            //        this.SetMissionMode(false);
            //    }
            //    if (this._fleetsChanged)
            //    {
            //        this.ClearFleets();
            //        this.AccountedSystems.Clear();
            //        List<FleetInfo> list = new List<FleetInfo>();
            //        foreach (int current in this._syncedFleets)
            //        {
            //            FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(current);
            //            if (fleetInfo != null)
            //            {
            //                list.Add(fleetInfo);
            //            }
            //        }
            //        list = (
            //            from x in list
            //            orderby x.SystemID
            //            select x).ToList<FleetInfo>();
            //        FleetInfo fleetInfo2 = list.FirstOrDefault((FleetInfo x) => x.IsReserveFleet && x.PlayerID == base.App.LocalPlayer.ID);
            //        if (fleetInfo2 != null)
            //        {
            //            if (!this.AccountedSystems.Contains(fleetInfo2.SystemID))
            //            {
            //                this.AccountedSystems.Add(fleetInfo2.SystemID);
            //                this.SyncSystem(base.App.GameDatabase.GetStarSystemInfo(fleetInfo2.SystemID));
            //            }
            //            this.SyncFleet(fleetInfo2);
            //        }
            //        FleetInfo fleetInfo3 = list.FirstOrDefault((FleetInfo x) => x.IsDefenseFleet && x.PlayerID == base.App.LocalPlayer.ID);
            //        if (fleetInfo3 != null)
            //        {
            //            if (!this.AccountedSystems.Contains(fleetInfo3.SystemID))
            //            {
            //                this.AccountedSystems.Add(fleetInfo3.SystemID);
            //                this.SyncSystem(base.App.GameDatabase.GetStarSystemInfo(fleetInfo3.SystemID));
            //            }
            //            this.SyncFleet(fleetInfo3);
            //        }
            //        if (this._listStations)
            //        {
            //            List<StationInfo> list2 = new List<StationInfo>();
            //            foreach (int current2 in this._syncedStations)
            //            {
            //                StationInfo stationInfo = base.App.GameDatabase.GetStationInfo(current2);
            //                if (stationInfo != null)
            //                {
            //                    list2.Add(stationInfo);
            //                }
            //            }
            //            this.SyncStations(list2);
            //        }
            //        foreach (FleetInfo current3 in list)
            //        {
            //            if (current3 != fleetInfo2 && (current3.Type != FleetType.FL_RESERVE || current3.PlayerID == base.App.LocalPlayer.ID) && current3 != fleetInfo3 && (current3.Type != FleetType.FL_DEFENSE || current3.PlayerID == base.App.LocalPlayer.ID))
            //            {
            //                if (!this.AccountedSystems.Contains(current3.SystemID))
            //                {
            //                    this.AccountedSystems.Add(current3.SystemID);
            //                    this.SyncSystem(base.App.GameDatabase.GetStarSystemInfo(current3.SystemID));
            //                }
            //                this.SyncFleet(current3);
            //            }
            //        }
            //        this._fleetsChanged = false;
            //    }
            //    if (this._planetsChanged)
            //    {
            //        this.ClearPlanets();
            //        this.SyncPlanets();
            //        this._planetsChanged = false;
            //    }
            //    this.PostSetProp("SetSelected", this._selected);
            //    this.PostSetProp("ShowFleetInfo", this._showFleetInfo);
            //    this.PostSetProp("ShipSelectionEnabled", this._shipSelectionEnabled);
            //    this.PostSetProp("SetPreferredSelectMode", this._preferredSelectMode);
            //    if (this._hasSuulka)
            //    {
            //        base.App.UI.SetVisible("gameRepairSuulkasButton", true);
            //    }
            //    if (this._repairModeChanged)
            //    {
            //        this.PostSetProp("SetRepairMode", this._repairMode);
            //        this._repairModeChanged = false;
            //    }
            //    this.PostSetProp("ShowRepairPoints", this._showRepairPoints);
            //    this.PostSetProp("RepairWidget", this._repairWidget);
            //    if (this._expandAll)
            //    {
            //        this.PostSetProp("ExpandAll", new object[0]);
            //        this._expandAll = false;
            //    }
            //    this.PostSetProp("Enabled", this._enabled);
            //    if (this._repairAll)
            //    {
            //        this.PostSetProp("RepairAll", new object[0]);
            //        this._repairAll = false;
            //    }
            //    if (this._undoAll)
            //    {
            //        this.PostSetProp("UndoAll", new object[0]);
            //        this._undoAll = false;
            //    }
            //    if (this._confirmRepairs)
            //    {
            //        this.PostSetProp("ConfirmRepairs", new object[0]);
            //        this._confirmRepairs = false;
            //    }
            //    this._contentChanged = false;
            //    this.PostSetProp("RefreshEnabled", true);
            //    this.SetShowEmptyFleets(this._showEmptyFleets);
            //}
		}
		public void SetVisibleFleets(Dictionary<int, bool> values)
		{
			if (values.Count <= 0)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(values.Count);
			foreach (int current in values.Keys)
			{
				list.Add(current);
				list.Add(values[current]);
			}
			this.PostSetProp("SetFleetsVisible", list.ToArray());
		}
		public void SetSyncedFleets(List<FleetInfo> fleets)
		{
			List<int> list = new List<int>();
			foreach (FleetInfo current in fleets)
			{
				list.Add(current.ID);
			}
			this.SetSyncedFleets(list);
		}
		public void SetSyncedFleets(List<int> fleets)
		{
			this._syncedFleets = fleets;
			this._contentChanged = true;
			this._fleetsChanged = true;
			this.Refresh();
		}
		public void SetSyncedFleets(int fleet)
		{
			this.SetSyncedFleets(new List<int>
			{
				fleet
			});
		}
		public void SetSyncedStations(List<StationInfo> stations)
		{
			List<int> list = new List<int>();
			foreach (StationInfo current in stations)
			{
				list.Add(current.OrbitalObjectID);
			}
			this.SetSyncedStations(list);
		}
		public void SetSyncedStations(List<int> stations)
		{
			this._syncedStations = stations;
			this._contentChanged = true;
			this._fleetsChanged = true;
			this.Refresh();
		}
		public void SetSyncedPlanets(List<PlanetInfo> planets)
		{
			List<int> list = new List<int>();
			foreach (PlanetInfo current in planets)
			{
				list.Add(current.ID);
			}
			this.SetSyncedPlanets(list);
		}
		public void SetSyncedPlanets(List<int> planets)
		{
			this._syncedPlanets = planets;
			this._contentChanged = true;
			this._planetsChanged = true;
			this.Refresh();
		}
		public void SyncFleetInfo(FleetInfo fleet)
		{
			if (fleet == null)
			{
				return;
			}
			if (this.FleetFilter != null && this.FleetFilter(fleet) != FleetWidget.FilterShips.Enable)
			{
				return;
			}
			if (this._separateDefenseFleet && fleet.IsDefenseFleet)
			{
				this._DefenseFleetUpdated = true;
			}
			bool flag = fleet.PlayerID != base.App.LocalPlayer.ID;
			IEnumerable<ShipInfo> shipInfoByFleetID = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true);
			MissionInfo missionByFleetID = this._game.GameDatabase.GetMissionByFleetID(fleet.ID);
			string text = "";
			int num = 0;
			if (fleet.AdmiralID != 0)
			{
				AdmiralInfo admiralInfo = this._game.GameDatabase.GetAdmiralInfo(fleet.AdmiralID);
				text = admiralInfo.Name;
				num = admiralInfo.ID;
			}
			int num2 = 0;
			if (this._game.AssetDatabase.GetFaction(this._game.GameDatabase.GetPlayerInfo(fleet.PlayerID).FactionID).CanUseAccelerators())
			{
                num2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._game.Game, fleet.ID);
			}
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			foreach (ShipInfo current in shipInfoByFleetID)
			{
				DesignInfo designInfo = current.DesignInfo;
				switch (designInfo.Class)
				{
				case ShipClass.Cruiser:
					num5++;
					break;
				case ShipClass.Dreadnought:
					num6++;
					break;
				case ShipClass.Leviathan:
					num7++;
					break;
				case ShipClass.BattleRider:
					num3++;
					break;
				case ShipClass.Station:
					num4++;
					break;
				}
			}
			string text2 = "CR";
			if (num5 > 0)
			{
				text2 = "CR_CMD";
			}
			if (num6 > 0)
			{
				text2 = "DN_CMD";
			}
			if (num7 > 0)
			{
				text2 = "LV_CMD";
			}
			int num8 = fleet.SystemID;
			int num9 = 0;
			MoveOrderInfo moveOrderInfoByFleetID = this._game.GameDatabase.GetMoveOrderInfoByFleetID(fleet.ID);
			if (moveOrderInfoByFleetID != null)
			{
				if (moveOrderInfoByFleetID.Progress == 0f)
				{
					num8 = moveOrderInfoByFleetID.FromSystemID;
				}
				else
				{
					num8 = 0;
				}
				num9 = moveOrderInfoByFleetID.ToSystemID;
			}
			if (missionByFleetID != null)
			{
				WaypointInfo nextWaypointForMission = this._game.GameDatabase.GetNextWaypointForMission(missionByFleetID.ID);
				if (nextWaypointForMission != null)
				{
					if (nextWaypointForMission.Type == WaypointType.TravelTo)
					{
						num9 = (nextWaypointForMission.SystemID.HasValue ? nextWaypointForMission.SystemID.Value : 0);
					}
					else
					{
						if (nextWaypointForMission.Type == WaypointType.ReturnHome)
						{
							num9 = fleet.SupportingSystemID;
						}
					}
				}
			}
			string text3 = "Deep Space";
			if (num8 != 0)
			{
				StarSystemInfo starSystemInfo = this._game.GameDatabase.GetStarSystemInfo(num8);
				text3 = string.Format("{0}", starSystemInfo.Name);
			}
			string text4 = "None";
			if (num9 != 0)
			{
				StarSystemInfo starSystemInfo2 = this._game.GameDatabase.GetStarSystemInfo(num9);
				text4 = string.Format("{0}", starSystemInfo2.Name);
			}
			string text5 = "None";
			if (fleet.SupportingSystemID != 0)
			{
				StarSystemInfo starSystemInfo3 = this._game.GameDatabase.GetStarSystemInfo(fleet.SupportingSystemID);
				text5 = string.Format("{0}", starSystemInfo3.Name);
			}
            int num10 = Kerberos.Sots.StarFleet.StarFleet.GetFleetEndurance(this._game.Game, fleet.ID);
			if (!this._game.Game.IsFleetInSupplyRange(fleet.ID))
			{
				num10 *= -1;
			}
			int num11 = -1;
			if (this._game.GameDatabase.GetFactionName(this._game.GameDatabase.GetPlayerInfo(fleet.PlayerID).FactionID) == "hiver")
			{
				num11 = this._game.GameDatabase.GetFleetCruiserEquivalent(fleet.ID);
			}
			int arg_390_0 = fleet.PlayerID;
			int arg_3A1_0 = base.App.LocalPlayer.ID;
			bool flag2 = false;
			string text6 = fleet.Name;
			string text7 = "";
			int num12 = 0;
			int num13 = 0;
			if (missionByFleetID != null)
			{
				text7 = GameSession.GetMissionDesc(base.App.GameDatabase, missionByFleetID);
				num12 = missionByFleetID.Duration;
                num13 = Kerberos.Sots.StarFleet.StarFleet.GetTurnsRemainingForMissionFleet(base.App.Game, fleet);
				if (this._game.GameDatabase.GetMissionByFleetID(fleet.ID) != null && this._game.GameDatabase.GetMissionByFleetID(fleet.ID).Type == MissionType.PIRACY && !this._game.GameDatabase.PirateFleetVisibleToPlayer(fleet.ID, this._game.LocalPlayer.ID))
				{
					text6 = "Pirate Raiders";
					flag2 = true;
				}
			}
            float fleetTravelSpeed = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game.Game, fleet.ID, shipInfoByFleetID, false);
            float fleetTravelSpeed2 = Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(this._game.Game, fleet.ID, shipInfoByFleetID, true);
			float num14;
			if (fleetTravelSpeed2 > 0f)
			{
				num14 = fleetTravelSpeed2;
			}
			else
			{
				num14 = fleetTravelSpeed;
			}
			this.PostSetProp("SyncFleetInfo", new object[]
			{
				fleet.Type,
				fleet.ID,
				fleet.PlayerID,
				text6,
				flag2 ? "" : text,
				flag2 ? 0 : num,
				text2,
				text5,
				text3,
				num8,
				text4,
				num10,
				num11,
				flag2 ? "" : text7,
				num12,
				num14,
				flag2 ? this._game.GameDatabase.GetPlayerInfo(this._game.Game.ScriptModules.Pirates.PlayerID).PrimaryColor : this._game.GameDatabase.GetPlayerInfo(fleet.PlayerID).PrimaryColor,
				this._enabled && (!flag || this._enemySelectionEnabled),
				fleet.Preferred,
				num2,
				num13
			});
		}
		private void SyncStationListInfo(int systemID, int playerID)
		{
			if (systemID == 0)
			{
				return;
			}
			StarSystemInfo starSystemInfo = this._game.GameDatabase.GetStarSystemInfo(systemID);
			string text = string.Format("{0}", starSystemInfo.Name);
			this.PostSetProp("SyncStationListInfo", new object[]
			{
				FleetType.FL_STATION,
				systemID,
				playerID,
				App.Localize("@FLEET_STATION_NAME"),
				"",
				text,
				systemID,
				this._game.GameDatabase.GetPlayerInfo(playerID).PrimaryColor,
				this._enabled,
				false
			});
		}
		public static bool IsBattleRider(DesignInfo design)
		{
			if (design == null)
			{
				return false;
			}
			DesignSectionInfo designSectionInfo = design.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission);
			return designSectionInfo != null && designSectionInfo.ShipSectionAsset.IsBattleRider;
		}
		public static bool IsWeaponBattleRider(DesignInfo design)
		{
			if (design == null)
			{
				return false;
			}
			DesignSectionInfo designSectionInfo = design.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ShipSectionAsset.Type == ShipSectionType.Mission);
			return designSectionInfo != null && designSectionInfo.ShipSectionAsset.IsBattleRider && ShipSectionAsset.IsWeaponBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass);
		}
		public FleetWidget.FilterShips DefaultShipFilter(ShipInfo ship, DesignInfo design)
		{
			if (FleetWidget.IsBattleRider(design) && (!this._ridersEnabled || FleetWidget.IsWeaponBattleRider(design)))
			{
				return FleetWidget.FilterShips.Ignore;
			}
			return FleetWidget.FilterShips.Enable;
		}
		public void LinkWidget(FleetWidget FleetWidget)
		{
			this._linkedWidgets.Add(FleetWidget);
			this.PostSetProp("LinkWidget", FleetWidget);
		}
		public void UnLinkWidget(FleetWidget fleetwidget)
		{
			this._linkedWidgets.Remove(fleetwidget);
			this.PostSetProp("UnlinkWidget", fleetwidget);
		}
		public void UnlinkWidgets()
		{
			this._linkedWidgets.Clear();
			this.PostSetProp("UnlinkWidgets", new object[0]);
		}
		private void ClearFleets()
		{
			this.PostSetProp("ClearFleets", new object[0]);
		}
		private void ClearPlanets()
		{
			this.PostSetProp("ClearPlanets", new object[0]);
		}
		private void SetMissionMode(bool val)
		{
			this.PostSetProp("MissionMode", val);
		}
		private void SetShowEmptyFleets(bool val)
		{
			this.PostSetProp("ShowEmptyFleets", val);
		}
		public void ExpandAll()
		{
			this._expandAll = true;
			this._contentChanged = true;
			this.Refresh();
		}
		public void SetEnabled(bool enabled)
		{
			this._enabled = enabled;
			this._contentChanged = true;
			this.Refresh();
		}
		public void RepairAll()
		{
			this._repairAll = true;
			this._contentChanged = true;
			this.Refresh();
		}
		public void UndoAll()
		{
			this._undoAll = true;
			this._contentChanged = true;
			this.Refresh();
		}
		public void ConfirmRepairs()
		{
			this._confirmRepairs = true;
			this._contentChanged = true;
			this.Refresh();
		}
		private void SyncPlanets()
		{
			if (this._syncedPlanets == null)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(this._syncedPlanets.Count);
			foreach (int current in this._syncedPlanets)
			{
				PlanetInfo planetInfo = base.App.GameDatabase.GetPlanetInfo(current);
				ColonyInfo colonyInfo = base.App.GameDatabase.GetColonyInfo(current);
				double num = 0.0;
				if (colonyInfo != null)
				{
					num = colonyInfo.ImperialPop;
				}
				list.Add(current);
				list.Add(base.App.GameDatabase.GetOrbitalObjectInfo(current).Name);
				list.Add(base.App.GameDatabase.GetCivilianPopulation(current, 0, false) + num);
				list.Add(planetInfo.Biosphere);
			}
			this.PostSetProp("SyncPlanets", list.ToArray());
		}
        private void SyncSuulkas(FleetInfo fleet)
        {
            IEnumerable<ShipInfo> shipInfoByFleetID = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false);
            List<object> list = new List<object>
            {
                fleet.Type,
                fleet.ID
            };
            int count = list.Count;
            int item = 0;
            foreach (ShipInfo info in shipInfoByFleetID)
            {
                DesignInfo designInfo = info.DesignInfo;
                if (Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(base.App, designInfo))
                {
                    this._hasSuulka = true;
                }
                if ((this.ShipFilter == null) || (this.ShipFilter(info, designInfo) != FilterShips.Ignore))
                {
                    item += 2;
                    list.Add(true);
                    list.Add(info.DesignID);
                    list.Add(info.ID);
                    list.Add(designInfo.Name + " Psionics");
                    list.Add(info.ShipName);
                    list.Add(false);
                    list.Add(false);
                    list.Add("");
                    list.Add("");
                    list.Add(0);
                    list.Add(false);
                    list.Add(false);
                    list.Add(0);
                    list.Add(0);
                    list.Add(true);
                    list.Add(info.PsionicPower);
                    list.Add((int)designInfo.DesignSections[0].ShipSectionAsset.PsionicPowerLevel);
                    list.Add(0);
                    list.Add(0);
                    list.Add(0);
                    list.Add(0);
                    list.Add(0);
                    list.Add(2);
                    list.Add(0);
                    list.Add(0);
                    list.Add(true);
                    list.Add(info.DesignID + 1);
                    list.Add(info.ID);
                    list.Add(designInfo.Name + " Structure");
                    list.Add(info.ShipName);
                    list.Add(false);
                    list.Add(false);
                    list.Add("");
                    list.Add("");
                    list.Add(0);
                    list.Add(false);
                    list.Add(false);
                    list.Add(0);
                    list.Add(0);
                    list.Add(true);
                    int num3 = 0;
                    int num4 = 0;
                    int num5 = 0;
                    int num6 = 0;
                    ShipClass leviathan = ShipClass.Leviathan;
                    BattleRiderTypes unspecified = BattleRiderTypes.Unspecified;
                    List<SectionInstanceInfo> list2 = base.App.GameDatabase.GetShipSectionInstances(info.ID).ToList<SectionInstanceInfo>();
                    if (list2.Count != designInfo.DesignSections.Length)
                    {
                        throw new InvalidDataException(string.Format("Mismatched design section vs ship section instance count for designId={0} and shipId={1}.", designInfo.ID, info.ID));
                    }
                    for (int i = 0; i < designInfo.DesignSections.Count<DesignSectionInfo>(); i++)
                    {
                        ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.GetShipSectionAsset(designInfo.DesignSections[i].FilePath);
                        List<string> techs = new List<string>();
                        if (designInfo.DesignSections[i].Techs.Count > 0)
                        {
                            foreach (int num8 in designInfo.DesignSections[i].Techs)
                            {
                                techs.Add(base.App.GameDatabase.GetTechFileID(num8));
                            }
                        }
                        num4 += Ship.GetStructureWithTech(this._game.AssetDatabase, techs, shipSectionAsset.Structure);
                        num3 += list2[i].Structure;
                        if (shipSectionAsset.Type == ShipSectionType.Mission)
                        {
                            leviathan = shipSectionAsset.Class;
                            unspecified = shipSectionAsset.BattleRiderType;
                        }
                        Dictionary<ArmorSide, DamagePattern> armorInstances = base.App.GameDatabase.GetArmorInstances(list2[i].ID);
                        if (armorInstances.Count > 0)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                num4 += (armorInstances[(ArmorSide)j].Width * armorInstances[(ArmorSide)j].Height) * 3;
                                for (int k = 0; k < armorInstances[(ArmorSide)j].Width; k++)
                                {
                                    for (int m = 0; m < armorInstances[(ArmorSide)j].Height; m++)
                                    {
                                        if (!armorInstances[(ArmorSide)j].GetValue(k, m))
                                        {
                                            num3 += 3;
                                        }
                                    }
                                }
                            }
                        }
                        Func<WeaponBankInfo, bool> predicate = null;
                        foreach (LogicalMount mount in shipSectionAsset.Mounts)
                        {
                            if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                            {
                                if (num5 == 0)
                                {
                                    if (predicate == null)
                                    {
                                        predicate = delegate(WeaponBankInfo x)
                                        {
                                            if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
                                            {
                                                return false;
                                            }
                                            int? designID = x.DesignID;
                                            if (designID.GetValueOrDefault() == 0)
                                            {
                                                return !designID.HasValue;
                                            }
                                            return true;
                                        };
                                    }
                                    WeaponBankInfo info3 = designInfo.DesignSections[i].WeaponBanks.FirstOrDefault<WeaponBankInfo>(predicate);
                                    num5 = ((info3 != null) && info3.DesignID.HasValue) ? info3.DesignID.Value : 0;
                                }
                                num6++;
                            }
                        }
                        List<ModuleInstanceInfo> source = base.App.GameDatabase.GetModuleInstances(list2[i].ID).ToList<ModuleInstanceInfo>();
                        List<DesignModuleInfo> module = designInfo.DesignSections[i].Modules;
                        if (source.Count == module.Count)
                        {
                            Func<ModuleInstanceInfo, bool> func2 = null;
                            for (int mod = 0; mod < module.Count; mod++)
                            {
                                if (func2 == null)
                                {
                                    func2 = x => x.ModuleNodeID == module[mod].MountNodeName;
                                }
                                ModuleInstanceInfo info4 = source.FirstOrDefault<ModuleInstanceInfo>(func2);
                                string modAsset = base.App.GameDatabase.GetModuleAsset(module[mod].ModuleID);
                                LogicalModule logicalModule = (from x in base.App.AssetDatabase.Modules
                                                        where x.ModulePath == modAsset
                                                        select x).First<LogicalModule>();
                                num4 += (int)logicalModule.Structure;
                                num3 += (info4 != null) ? info4.Structure : ((int)logicalModule.Structure);
                                if (module[mod].DesignID.HasValue)
                                {
                                    foreach (LogicalMount mount in logicalModule.Mounts)
                                    {
                                        if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                                        {
                                            if (num5 == 0)
                                            {
                                                num5 = module[mod].DesignID.Value;
                                            }
                                            num6++;
                                        }
                                    }
                                }
                            }
                        }
                        foreach (WeaponInstanceInfo info5 in base.App.GameDatabase.GetWeaponInstances(list2[i].ID).ToList<WeaponInstanceInfo>())
                        {
                            num4 += (int)info5.MaxStructure;
                            num3 += (int)info5.Structure;
                        }
                        List<ShipInfo> list6 = base.App.GameDatabase.GetBattleRidersByParentID(info.ID).ToList<ShipInfo>();
                        if (num6 > 0)
                        {
                            int num12 = num6;
                            foreach (ShipInfo info6 in list6)
                            {
                                DesignInfo info7 = base.App.GameDatabase.GetDesignInfo(info6.DesignID);
                                if (info7 != null)
                                {
                                    DesignSectionInfo info8 = info7.DesignSections.FirstOrDefault<DesignSectionInfo>(x => x.ShipSectionAsset.Type == ShipSectionType.Mission);
                                    if ((info8 != null) && ShipSectionAsset.IsBattleRiderClass(info8.ShipSectionAsset.RealClass))
                                    {
                                        num12--;
                                    }
                                }
                            }
                            int structure = 0;
                            if (num5 != 0)
                            {
                                foreach (DesignSectionInfo info10 in base.App.GameDatabase.GetDesignInfo(num5).DesignSections)
                                {
                                    ShipSectionAsset asset2 = base.App.AssetDatabase.GetShipSectionAsset(info10.FilePath);
                                    structure = asset2.Structure;
                                    int repairPoints = asset2.RepairPoints;
                                    if (asset2.Armor.Length > 0)
                                    {
                                        for (int n = 0; n < 4; n++)
                                        {
                                            structure += (asset2.Armor[n].X * asset2.Armor[n].Y) * 3;
                                        }
                                    }
                                }
                            }
                            num4 += structure * num6;
                            num3 += structure * (num6 - num12);
                        }
                    }
                    list.Add(num3);
                    list.Add(num4);
                    list.Add(0);
                    list.Add(0);
                    list.Add(0);
                    list.Add(0);
                    list.Add(0);
                    list.Add(3);
                    list.Add((int)leviathan);
                    list.Add((int)unspecified);
                }
            }
            list.Insert(count, item);
            list.Add(0);
            this.PostSetProp("SyncShips", list.ToArray());
        }
        private void SyncSystem(StarSystemInfo system)
		{
			if (system != null && !system.IsDeepSpace)
			{
				StellarClass stellarClass = new StellarClass(system.StellarClass);
				Vector4 vector = StarHelper.CalcModelColor(stellarClass);
				bool flag = this.EnableCreateFleetButton && base.App.GameDatabase.GetRemainingSupportPoints(base.App.Game, system.ID, base.App.LocalPlayer.ID) > 0 && this.AdmiralAvailable(base.App.LocalPlayer.ID, system.ID) && this.CommandShipAvailable(base.App.LocalPlayer.ID, system.ID) && (base.App.CurrentState == base.App.GetGameState<StarMapState>() || base.App.CurrentState == base.App.GetGameState<FleetManagerState>());
				this.PostSetProp("SyncSystem", new object[]
				{
					system.ID,
					system.Name,
					vector.X,
					vector.Y,
					vector.Z,
					flag
				});
			}
		}
        private void SyncFleet(FleetInfo fleet)
        {
            Func<ColonyInfo, bool> predicate = null;
            if ((fleet != null) && (((this._game.GameDatabase.GetMissionByFleetID(fleet.ID) == null) || (this._game.GameDatabase.GetMissionByFleetID(fleet.ID).Type != MissionType.PIRACY)) || (this._game.GameDatabase.PirateFleetVisibleToPlayer(fleet.ID, this._game.LocalPlayer.ID) || this.ShowPiracyFleets)))
            {
                this.SyncFleetInfo(fleet);
                if (this.SuulkaMode)
                {
                    this.SyncSuulkas(fleet);
                }
                else
                {
                    List<ShipInfo> list = this._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
                    List<object> list2 = new List<object>
                    {
                        fleet.Type,
                        fleet.ID
                    };
                    int count = list2.Count;
                    int item = 0;
                    List<int> list3 = null;
                    if ((this._missionMode != MissionType.NO_MISSION) && (fleet.Type != FleetType.FL_RESERVE))
                    {
                        list3 = Kerberos.Sots.StarFleet.StarFleet.GetMissionCapableShips(this._game.Game, fleet.ID, this._missionMode);
                        List<int> list4 = new List<int>();
                        if (list3.Count == 0)
                        {
                            if (!this._game.GetStratModifier<bool>(StratModifiers.MutableFleets, fleet.PlayerID))
                            {
                                list4 = DesignLab.GetMissionRequiredDesigns(this._game.Game, this._missionMode, this._game.LocalPlayer.ID);
                            }
                            foreach (int num3 in list4)
                            {
                                item++;
                                list2.Add(true);
                                DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(num3);
                                list2.Add(num3);
                                list2.Add(-1);
                                list2.Add(designInfo.Name);
                                list2.Add(designInfo.Name);
                                list2.Add(false);
                                list2.Add(false);
                                list2.Add("");
                                list2.Add("");
                                list2.Add(0);
                                list2.Add(false);
                                list2.Add(false);
                                list2.Add(base.App.GameDatabase.GetCommandPointCost(designInfo.ID));
                                list2.Add(base.App.GameDatabase.GetDesignCommandPointQuota(base.App.AssetDatabase, designInfo.ID));
                                list2.Add(true);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(designInfo.Class);
                                list2.Add(designInfo.DesignSections.First<DesignSectionInfo>(x => (x.ShipSectionAsset.Type == ShipSectionType.Mission)).ShipSectionAsset.BattleRiderType);
                            }
                        }
                    }
                    foreach (ShipInfo info2 in list)
                    {
                        DesignInfo design = base.App.GameDatabase.GetDesignInfo(info2.DesignID);
                        bool flag = true;
                        if (this.ShipFilter != null)
                        {
                            switch (this.ShipFilter(info2, design))
                            {
                                case FilterShips.Ignore:
                                    {
                                        continue;
                                    }
                                case FilterShips.Disable:
                                    flag = false;
                                    break;
                            }
                        }
                        item++;
                        if ((list3 != null) && (this._missionMode != MissionType.NO_MISSION))
                        {
                            list2.Add(list3.Contains(info2.ID));
                        }
                        else
                        {
                            list2.Add(true);
                        }
                        list2.Add(info2.DesignID);
                        list2.Add(info2.ID);
                        list2.Add(design.Name);
                        list2.Add(info2.ShipName);
                        bool flag2 = false;
                        string iconSpriteName = "";
                        string str2 = "";
                        bool flag3 = false;
                        bool flag4 = info2.IsPoliceShip();
                        bool flag5 = false;
                        foreach (DesignSectionInfo info4 in design.DesignSections)
                        {
                            if (info4.ShipSectionAsset.GetPlatformType().HasValue)
                            {
                                str2 = info4.ShipSectionAsset.GetPlatformType().ToString();
                            }
                            if (info4.FilePath.Contains("minelayer"))
                            {
                                flag2 = true;
                                foreach (WeaponBankInfo info5 in info4.WeaponBanks)
                                {
                                    Func<LogicalWeapon, bool> func = null;
                                    string wasset = base.App.GameDatabase.GetWeaponAsset(info5.WeaponID.Value);
                                    if (wasset.Contains("Min_"))
                                    {
                                        if (func == null)
                                        {
                                            func = x => x.FileName == wasset;
                                        }
                                        LogicalWeapon weapon = base.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>(func);
                                        if (weapon != null)
                                        {
                                            iconSpriteName = weapon.IconSpriteName;
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            if (info4.FilePath.ToLower().Contains("_sdb"))
                            {
                                flag3 = true;
                                break;
                            }
                            if (info4.ShipSectionAsset.isPolice)
                            {
                                flag4 = true;
                                break;
                            }
                            if (info4.ShipSectionAsset.IsSuperTransport)
                            {
                                flag5 = true;
                                break;
                            }
                        }
                        list2.Add(flag2);
                        list2.Add(flag3);
                        list2.Add(iconSpriteName);
                        list2.Add(str2);
                        list2.Add(info2.LoaCubes);
                        list2.Add(flag5);
                        list2.Add(flag4);
                        list2.Add(base.App.GameDatabase.GetShipCommandPointCost(info2.ID, true));
                        list2.Add(base.App.GameDatabase.GetDesignCommandPointQuota(base.App.AssetDatabase, design.ID));
                        list2.Add(flag);
                        int num4 = 0;
                        int num5 = 0;
                        int num6 = 0;
                        int num7 = 0;
                        int num8 = 0;
                        int num9 = 0;
                        int num10 = 0;
                        int num11 = 0;
                        int num12 = 0;
                        BattleRiderTypes unspecified = BattleRiderTypes.Unspecified;
                        List<SectionInstanceInfo> list5 = base.App.GameDatabase.GetShipSectionInstances(info2.ID).ToList<SectionInstanceInfo>();
                        for (int i = 0; i < design.DesignSections.Count<DesignSectionInfo>(); i++)
                        {
                            ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.GetShipSectionAsset(design.DesignSections[i].FilePath);
                            List<string> techs = new List<string>();
                            if (design.DesignSections[i].Techs.Count > 0)
                            {
                                foreach (int num14 in design.DesignSections[i].Techs)
                                {
                                    techs.Add(base.App.GameDatabase.GetTechFileID(num14));
                                }
                            }
                            float structure = list5[i].Structure;
                            num7 += Ship.GetStructureWithTech(this._game.AssetDatabase, techs, shipSectionAsset.Structure);
                            num6 += list5[i].Structure;
                            num8 += shipSectionAsset.ConstructionPoints;
                            num9 += shipSectionAsset.ColonizationSpace;
                            if (shipSectionAsset.Type == ShipSectionType.Mission)
                            {
                                unspecified = shipSectionAsset.BattleRiderType;
                            }
                            Dictionary<ArmorSide, DamagePattern> armorInstances = base.App.GameDatabase.GetArmorInstances(list5[i].ID);
                            if (armorInstances.Count > 0)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    num7 += (armorInstances[(ArmorSide)j].Width * armorInstances[(ArmorSide)j].Height) * 3;
                                    for (int k = 0; k < armorInstances[(ArmorSide)j].Width; k++)
                                    {
                                        for (int m = 0; m < armorInstances[(ArmorSide)j].Height; m++)
                                        {
                                            if (!armorInstances[(ArmorSide)j].GetValue(k, m))
                                            {
                                                num6 += 3;
                                            }
                                        }
                                    }
                                }
                            }
                            Func<WeaponBankInfo, bool> func2 = null;
                            foreach (LogicalMount mount in shipSectionAsset.Mounts)
                            {
                                if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                                {
                                    if (num12 == 0)
                                    {
                                        if (func2 == null)
                                        {
                                            func2 = delegate(WeaponBankInfo x)
                                            {
                                                if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
                                                {
                                                    return false;
                                                }
                                                int? designID = x.DesignID;
                                                if (designID.GetValueOrDefault() == 0)
                                                {
                                                    return !designID.HasValue;
                                                }
                                                return true;
                                            };
                                        }
                                        WeaponBankInfo info6 = design.DesignSections[i].WeaponBanks.FirstOrDefault<WeaponBankInfo>(func2);
                                        num12 = ((info6 != null) && info6.DesignID.HasValue) ? info6.DesignID.Value : 0;
                                    }
                                    num10++;
                                }
                            }
                            List<ModuleInstanceInfo> list7 = base.App.GameDatabase.GetModuleInstances(list5[i].ID).ToList<ModuleInstanceInfo>();
                            List<DesignModuleInfo> module = design.DesignSections[i].Modules;
                            if (list7.Count == module.Count)
                            {
                                Func<ModuleInstanceInfo, bool> func3 = null;
                                for (int mod = 0; mod < module.Count; mod++)
                                {
                                    string modAsset;
                                    if (func3 == null)
                                    {
                                        func3 = x => x.ModuleNodeID == module[mod].MountNodeName;
                                    }
                                    ModuleInstanceInfo info7 = list7.FirstOrDefault<ModuleInstanceInfo>(func3);
                                    if (info7 != null)
                                    {
                                        modAsset = base.App.GameDatabase.GetModuleAsset(module[mod].ModuleID);
                                        LogicalModule logicalModule = (from x in base.App.AssetDatabase.Modules
                                                                where x.ModulePath == modAsset
                                                                select x).First<LogicalModule>();
                                        num7 += (int)logicalModule.Structure;
                                        num6 += (info7 != null) ? info7.Structure : ((int)logicalModule.Structure);
                                        num5 += logicalModule.RepairPointsBonus;
                                        if (info7.Structure > 0f)
                                        {
                                            num4 += info7.RepairPoints;
                                            structure += logicalModule.StructureBonus;
                                        }
                                        if (module[mod].DesignID.HasValue)
                                        {
                                            foreach (LogicalMount mount in logicalModule.Mounts)
                                            {
                                                if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                                                {
                                                    if (num12 == 0)
                                                    {
                                                        num12 = module[mod].DesignID.Value;
                                                    }
                                                    num10++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            num5 += shipSectionAsset.RepairPoints;
                            if (structure > 0f)
                            {
                                num4 += list5[i].RepairPoints;
                            }
                            foreach (WeaponInstanceInfo info8 in base.App.GameDatabase.GetWeaponInstances(list5[i].ID).ToList<WeaponInstanceInfo>())
                            {
                                num7 += (int)info8.MaxStructure;
                                num6 += (int)info8.Structure;
                            }
                        }
                        List<ShipInfo> source = base.App.GameDatabase.GetBattleRidersByParentID(info2.ID).ToList<ShipInfo>();
                        if (num10 > 0)
                        {
                            num11 = num10;
                            foreach (ShipInfo info9 in source)
                            {
                                DesignInfo info10 = base.App.GameDatabase.GetDesignInfo(info9.DesignID);
                                if (info10 != null)
                                {
                                    DesignSectionInfo info11 = info10.DesignSections.FirstOrDefault<DesignSectionInfo>(x => x.ShipSectionAsset.Type == ShipSectionType.Mission);
                                    if ((info11 != null) && ShipSectionAsset.IsBattleRiderClass(info11.ShipSectionAsset.RealClass))
                                    {
                                        num11--;
                                    }
                                }
                            }
                            int num19 = 0;
                            int repairPoints = 0;
                            if (num12 != 0)
                            {
                                foreach (DesignSectionInfo info13 in base.App.GameDatabase.GetDesignInfo(num12).DesignSections)
                                {
                                    ShipSectionAsset asset2 = base.App.AssetDatabase.GetShipSectionAsset(info13.FilePath);
                                    num19 = asset2.Structure;
                                    repairPoints = asset2.RepairPoints;
                                    if (asset2.Armor.Length > 0)
                                    {
                                        for (int n = 0; n < 4; n++)
                                        {
                                            num19 += (asset2.Armor[n].X * asset2.Armor[n].Y) * 3;
                                        }
                                    }
                                }
                            }
                            num7 += num19 * num10;
                            num5 += repairPoints * num10;
                            num6 += num19 * (num10 - num11);
                            num4 += repairPoints * (num10 - num11);
                        }
                        if (list5.Count != design.DesignSections.Length)
                        {
                            throw new InvalidDataException(string.Format("Mismatched design section vs ship section instance count for designId={0} and shipId={1}.", design.ID, info2.ID));
                        }
                        list2.Add(num6);
                        list2.Add(num7);
                        list2.Add(num4);
                        list2.Add(num5);
                        list2.Add(num8);
                        list2.Add(num9);
                        list2.Add(source.Count<ShipInfo>());
                        foreach (ShipInfo info14 in source)
                        {
                            list2.Add(info14.ID);
                        }
                        list2.Add(0);
                        list2.Add(design.Class);
                        list2.Add((int)unspecified);
                    }
                    list2.Insert(count, item);
                    bool flag6 = fleet.Type == FleetType.FL_RESERVE;
                    count = list2.Count;
                    int num22 = 0;
                    if (flag6 && this._showColonies)
                    {
                        List<ColonyInfo> list10;
                        if (this._onlyLocalPlayer)
                        {
                            if (predicate == null)
                            {
                                predicate = x => x.PlayerID == base.App.LocalPlayer.ID;
                            }
                            list10 = base.App.GameDatabase.GetColonyInfosForSystem(fleet.SystemID).ToList<ColonyInfo>().Where<ColonyInfo>(predicate).ToList<ColonyInfo>();
                        }
                        else
                        {
                            list10 = base.App.GameDatabase.GetColonyInfosForSystem(fleet.SystemID).ToList<ColonyInfo>();
                        }
                        foreach (ColonyInfo info15 in list10)
                        {
                            list2.Add(info15.ID);
                            list2.Add(base.App.GameDatabase.GetOrbitalObjectInfo(info15.OrbitalObjectID).Name);
                            list2.Add(info15.RepairPoints);
                            list2.Add(info15.RepairPointsMax);
                            num22++;
                        }
                    }
                    list2.Insert(count, num22);
                    this.PostSetProp("SyncShips", list2.ToArray());
                }
            }
        }
        private void SyncStations(List<StationInfo> stationInfos)
		{
			StationInfo stationInfo = stationInfos.FirstOrDefault<StationInfo>();
			if (stationInfo == null)
			{
				return;
			}
			int playerID = stationInfo.PlayerID;
			int num = 0;
			OrbitalObjectInfo orbitalObjectInfo = base.App.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
			if (orbitalObjectInfo != null)
			{
				num = orbitalObjectInfo.StarSystemID;
			}
			this.SyncStationListInfo(num, playerID);
			List<object> list = new List<object>();
			list.Add(FleetType.FL_STATION);
			list.Add(num);
			int count = list.Count;
			int num2 = 0;
			foreach (StationInfo current in stationInfos)
			{
				DesignInfo designInfo = current.DesignInfo;
				list.Add(true);
				list.Add(current.DesignInfo.ID);
				list.Add(current.ShipID);
				list.Add(designInfo.Name);
				list.Add(designInfo.Name);
				list.Add(false);
				list.Add(false);
				list.Add("");
				list.Add("");
				list.Add(0);
				list.Add(false);
				list.Add(false);
				list.Add(0);
				list.Add(0);
				list.Add(true);
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				List<SectionInstanceInfo> list2 = base.App.GameDatabase.GetShipSectionInstances(current.ShipID).ToList<SectionInstanceInfo>();
				for (int i = 0; i < designInfo.DesignSections.Length; i++)
				{
					ShipSectionAsset shipSectionAsset = base.App.AssetDatabase.GetShipSectionAsset(designInfo.DesignSections[i].FilePath);
					List<string> list3 = new List<string>();
					if (designInfo.DesignSections[i].Techs.Count > 0)
					{
						foreach (int current2 in designInfo.DesignSections[i].Techs)
						{
							list3.Add(base.App.GameDatabase.GetTechFileID(current2));
						}
					}
					num6 += Ship.GetStructureWithTech(this._game.AssetDatabase, list3, shipSectionAsset.Structure);
					bool flag = designInfo.DesignSections.Length == list2.Count;
					int num7 = flag ? list2[i].Structure : designInfo.DesignSections[i].ShipSectionAsset.Structure;
					num5 += num7;
					if (num5 > num6)
					{
						num5 = num6;
					}
					Dictionary<ArmorSide, DamagePattern> dictionary = flag ? base.App.GameDatabase.GetArmorInstances(list2[i].ID) : new Dictionary<ArmorSide, DamagePattern>();
					if (dictionary.Count > 0)
					{
						for (int j = 0; j < 4; j++)
						{
							num6 += dictionary[(ArmorSide)j].Width * dictionary[(ArmorSide)j].Height * 3;
							for (int k = 0; k < dictionary[(ArmorSide)j].Width; k++)
							{
								for (int l = 0; l < dictionary[(ArmorSide)j].Height; l++)
								{
									if (!dictionary[(ArmorSide)j].GetValue(k, l))
									{
										num5 += 3;
									}
								}
							}
						}
					}
					List<ModuleInstanceInfo> list4 = flag ? base.App.GameDatabase.GetModuleInstances(list2[i].ID).ToList<ModuleInstanceInfo>() : new List<ModuleInstanceInfo>();
					List<DesignModuleInfo> modules = designInfo.DesignSections[i].Modules;
					if (list4.Count == modules.Count)
					{
						for (int m = 0; m < modules.Count; m++)
						{
							string modAsset = base.App.GameDatabase.GetModuleAsset(modules[m].ModuleID);
							LogicalModule logicalModule = (
								from x in base.App.AssetDatabase.Modules
								where x.ModulePath == modAsset
								select x).First<LogicalModule>();
							num6 += (int)logicalModule.Structure;
							num4 += logicalModule.RepairPointsBonus;
							num5 += list4[m].Structure;
							if ((float)list4[m].Structure > 0f)
							{
								num3 += list4[m].RepairPoints;
								num7 += (int)logicalModule.StructureBonus;
							}
						}
					}
					List<WeaponInstanceInfo> list5 = base.App.GameDatabase.GetWeaponInstances(list2[i].ID).ToList<WeaponInstanceInfo>();
					foreach (WeaponInstanceInfo current3 in list5)
					{
						num6 += (int)current3.MaxStructure;
						num5 += (int)current3.Structure;
					}
					num4 += shipSectionAsset.RepairPoints;
					if (num7 > 0)
					{
						num3 += (flag ? list2[i].RepairPoints : designInfo.DesignSections[i].ShipSectionAsset.RepairPoints);
					}
				}
				list.Add(num5);
				list.Add(num6);
				list.Add(num3);
				list.Add(num4);
				list.Add(0);
				list.Add(0);
				list.Add(0);
				list.Add(false);
				list.Add(4);
				list.Add(0);
				num2++;
			}
			list.Insert(count, num2);
			list.Add(0);
			this.PostSetProp("SyncShips", list.ToArray());
		}
		private void ShowFleetPopup(string[] eventParams)
		{
			base.App.UI.AutoSize(this._contextMenuID);
			this._contextSlot = int.Parse(eventParams[1]);
			FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
			if (fleetInfo != null)
			{
				MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameRenameFleetButton|" + this._widgetID.ToString()
				}), !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, fleetInfo));
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameCancelMissionButton|" + this._widgetID.ToString()
				}), fleetInfo.IsNormalFleet && (missionByFleetID != null && missionByFleetID.Type != MissionType.RETURN) && missionByFleetID.Type != MissionType.RETREAT);
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameDissolveFleetButton|" + this._widgetID.ToString()
				}), (fleetInfo.IsNormalFleet || fleetInfo.IsAcceleratorFleet || fleetInfo.IsGateFleet) && missionByFleetID == null && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(base.App.Game, fleetInfo));
                bool value = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.SURVEY, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetSurveyMissionButton|" + this._widgetID.ToString()
				}), value);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetSurveyMissionButton|" + this._widgetID.ToString()
				}), value);
                bool value2 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.COLONIZATION, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetColonizeMissionButton|" + this._widgetID.ToString()
				}), value2);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetColonizeMissionButton|" + this._widgetID.ToString()
				}), value2);
                bool value3 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.EVACUATE, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetEvacuateButton|" + this._widgetID.ToString()
				}), value3);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetEvacuateButton|" + this._widgetID.ToString()
				}), value3);
                bool value4 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.SUPPORT, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetSuportMissionButton|" + this._widgetID.ToString()
				}), value4);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetSuportMissionButton|" + this._widgetID.ToString()
				}), value4);
                bool value5 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.RELOCATION, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetRelocateMissionButton|" + this._widgetID.ToString()
				}), value5);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetRelocateMissionButton|" + this._widgetID.ToString()
				}), value5);
                bool value6 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.PATROL, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetPatrolMissionButton|" + this._widgetID.ToString()
				}), value6);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetPatrolMissionButton|" + this._widgetID.ToString()
				}), value6);
                bool value7 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.INTERDICTION, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetInterdictMissionButton|" + this._widgetID.ToString()
				}), value7);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetInterdictMissionButton|" + this._widgetID.ToString()
				}), value7);
                bool value8 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.INVASION, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetInvadeMissionButton|" + this._widgetID.ToString()
				}), value8);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetInvadeMissionButton|" + this._widgetID.ToString()
				}), value8);
                bool value9 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.STRIKE, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetStrikeMissionButton|" + this._widgetID.ToString()
				}), value9);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetStrikeMissionButton|" + this._widgetID.ToString()
				}), value9);
                bool value10 = fleetInfo.IsNormalFleet && this._enableMissionButtons && missionByFleetID == null && Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(base.App.GameDatabase, base.App.Game, fleetInfo.ID, MissionType.PIRACY, true).Any<int>() && base.App.CurrentState.Name == "StarMapState" && this._missionMode == MissionType.NO_MISSION && this._game.GameDatabase.HasEndOfFleshExpansion();
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetPiracyButton|" + this._widgetID.ToString()
				}), value10);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetPiracyButton|" + this._widgetID.ToString()
				}), value10);
				bool value11 = base.App.AssetDatabase.GetFaction(base.App.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID).FactionID).Name == "loa" && this._game.GameDatabase.HasEndOfFleshExpansion() && fleetInfo.Type != FleetType.FL_ACCELERATOR;
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetLoaDissolveToCube|" + this._widgetID.ToString()
				}), value11);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetLoaDissolveToCube|" + this._widgetID.ToString()
				}), value11);
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetSetLoaComposition|" + this._widgetID.ToString()
				}), value11);
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					this._contextMenuID,
					"gameFleetSetLoaComposition|" + this._widgetID.ToString()
				}), value11);
				base.App.UI.AutoSize(this._contextMenuID);
				base.App.UI.ForceLayout(this._contextMenuID);
				base.App.UI.ShowTooltip(this._contextMenuID, float.Parse(eventParams[2]), float.Parse(eventParams[3]));
			}
		}
		private void ShowShipPopup(string[] eventParams)
		{
			base.App.UI.AutoSize(this._shipcontextMenuID);
			this._contextSlot = int.Parse(eventParams[3]);
			ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(this._contextSlot, true);
			if (shipInfo != null && shipInfo.DesignInfo.PlayerID == base.App.LocalPlayer.ID)
			{
				bool flag = false;
				if (shipInfo.DesignInfo.Class == ShipClass.Station)
				{
					flag = true;
				}
				FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(shipInfo.FleetID);
				MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(shipInfo.FleetID);
				if (fleetInfo != null)
				{
					base.App.UI.SetEnabled(base.App.UI.Path(new string[]
					{
						this._shipcontextMenuID,
						"gameRetrofitShip|" + this._widgetID.ToString()
					}), !Kerberos.Sots.StarFleet.StarFleet.IsNewestRetrofit(shipInfo.DesignInfo, base.App.GameDatabase.GetVisibleDesignInfosForPlayer(base.App.LocalPlayer.ID)) && Kerberos.Sots.StarFleet.StarFleet.SystemSupportsRetrofitting(base.App, fleetInfo.SystemID, base.App.LocalPlayer.ID) && Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShip(base.App, fleetInfo.ID, shipInfo.ID) && missionByFleetID == null);
					base.App.UI.ShowTooltip(this._shipcontextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
					return;
				}
				if (flag)
				{
					base.App.UI.SetEnabled(base.App.UI.Path(new string[]
					{
						this._shipcontextMenuID,
						"gameRetrofitShip|" + this._widgetID.ToString()
					}), Kerberos.Sots.StarFleet.StarFleet.CanRetrofitStation(base.App, shipInfo.ID));
					base.App.UI.ShowTooltip(this._shipcontextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
				}
			}
		}
		private void DissolveFleet(int src, int tgt)
		{
			FleetInfo fleetInfo = this._game.GameDatabase.GetFleetInfo(src);
			FleetInfo fleetInfo2 = this._game.GameDatabase.GetFleetInfo(tgt);
			if (fleetInfo2 != null)
			{
				IEnumerable<ShipInfo> enumerable = this._game.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>();
				foreach (ShipInfo current in enumerable)
				{
					this._game.GameDatabase.TransferShip(current.ID, fleetInfo2.ID);
				}
				if (!fleetInfo.IsReserveFleet)
				{
					this._game.GameDatabase.RemoveFleet(src);
				}
			}
			this._contentChanged = true;
			this._fleetsChanged = true;
			this._syncedFleets.Remove(src);
			this.Refresh();
		}
		public void SetFleetsChanged()
		{
			this._fleetsChanged = true;
		}
		public void OnUpdate()
		{
			this._ShipToolTip.Update();
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (eventName == "FleetWidgetReady" && int.Parse(eventParams[0]) == this._widgetID)
			{
				this._ready = true;
				this.Refresh();
			}
			if (!this._enabled)
			{
				return;
			}
			if (eventName == "ShipHovered" && int.Parse(eventParams[0]) == this._widgetID)
			{
				if (this.DisableTooltips)
				{
					return;
				}
				int num = int.Parse(eventParams[1]);
				string a = eventParams[4];
				if (a != this._shipcontextMenuID && num != -1 && (this._ShipToolTip.GetShipID() != num || !this._ShipToolTip.isvalid()))
				{
					ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(num, false);
					if (shipInfo != null)
					{
						FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(shipInfo.FleetID);
						if (fleetInfo.PlayerID == base.App.LocalPlayer.ID)
						{
							this._ShipToolTip.Initialize();
							base.App.UI.ShowTooltip(this._ShipToolTip.GetPanelID(), float.Parse(eventParams[2]), float.Parse(eventParams[3]) - (float)Math.Floor(77.5));
							this._ShipToolTip.SyncShipTooltip(num);
							return;
						}
					}
				}
			}
			else
			{
				if (eventName == "ShipLeft")
				{
					string a2 = eventParams[1];
					if (a2 != this._shipcontextMenuID)
					{
						base.App.UI.HideTooltip();
					}
					this._ShipToolTip.Dispose(true);
					return;
				}
				if (eventName == "ScrapShipsEvent" && int.Parse(eventParams[0]) == this._widgetID)
				{
					this._shipsToScrap.Clear();
					int num2 = int.Parse(eventParams[1]);
					for (int i = 0; i < num2; i++)
					{
						int item = int.Parse(eventParams[2 + i]);
						this._shipsToScrap.Add(item);
					}
					this._scrapDialog = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, App.Localize("@UI_FLEET_DIALOG_SCRAPSHIPS_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_SCRAPSHIPS_DESC"), num2.ToString()), "dialogGenericQuestion"), null);
					return;
				}
				if (eventName == "FleetContextMenu" && int.Parse(eventParams[0]) == this._widgetID)
				{
					if (this._enableRightClick)
					{
						this.ShowFleetPopup(eventParams);
						return;
					}
				}
				else
				{
					if (eventName == "FleetShipContextMenu" && int.Parse(eventParams[0]) == this._widgetID)
					{
						return;
					}
					if (eventName == "ListContextMenu" && eventParams[4] == "shipList")
					{
						this.ShowShipPopup(eventParams);
						return;
					}
					if (eventName == "FleetTransferEvent" && int.Parse(eventParams[0]) == this._widgetID)
					{
						int num3 = int.Parse(eventParams[1]);
						int num4 = int.Parse(eventParams[2]);
						int num5 = int.Parse(eventParams[3]);
						ShipInfo shipInfo2 = this._game.GameDatabase.GetShipInfo(num3, true);
						if (shipInfo2.DesignInfo.IsLoaCube())
						{
							this._LoaCubeTransferDialog = base.App.UI.CreateDialog(new DialogLoaShipTransfer(base.App, num4, num5, num3, 1), null);
							return;
						}
						this._game.GameDatabase.TransferShip(num3, num4);
						if (this.SyncedFleets.Contains(num4))
						{
							this.SyncFleetInfo(base.App.GameDatabase.GetFleetInfo(num4));
						}
						if (this.SyncedFleets.Contains(num5))
						{
							this.SyncFleetInfo(base.App.GameDatabase.GetFleetInfo(num5));
						}
						foreach (FleetWidget current in this._linkedWidgets)
						{
							if (current.SyncedFleets != null && current.SyncedFleets.Count<int>() >= 0)
							{
								if (current.SyncedFleets.Contains(num4))
								{
									current.SyncFleetInfo(base.App.GameDatabase.GetFleetInfo(num4));
								}
								if (current.SyncedFleets.Contains(num5))
								{
									current.SyncFleetInfo(base.App.GameDatabase.GetFleetInfo(num5));
								}
							}
						}
						if (this.OnFleetsModified != null)
						{
							this.OnFleetsModified(base.App, new int[]
							{
								num4,
								num5
							});
							return;
						}
					}
					else
					{
						if (eventName == "FleetDissolveEvent" && int.Parse(eventParams[0]) == this._widgetID)
						{
							int src = int.Parse(eventParams[1]);
							int tgt = int.Parse(eventParams[2]);
							this.DissolveFleet(src, tgt);
							return;
						}
						if (eventName == "CannotModifyEvent" && int.Parse(eventParams[0]) == this._widgetID)
						{
							int fleetID = int.Parse(eventParams[1]);
							FleetInfo fleetInfo2 = base.App.GameDatabase.GetFleetInfo(fleetID);
							if (fleetInfo2 != null)
							{
								base.App.UI.CreateDialog(new GenericTextDialog(base.App, App.Localize("@UI_FLEET_DIALOG_CANNOT_MODIFY_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_CANNOT_MODIFY_DESC"), fleetInfo2.Name), "dialogGenericMessage"), null);
								return;
							}
						}
						else
						{
							if (eventName == "SupportLimitViolationEvent" && int.Parse(eventParams[0]) == this._widgetID)
							{
								int fleetID2 = int.Parse(eventParams[1]);
								FleetInfo fleetInfo3 = base.App.GameDatabase.GetFleetInfo(fleetID2);
								if (fleetInfo3 != null)
								{
									base.App.UI.CreateDialog(new GenericTextDialog(base.App, App.Localize("@UI_FLEET_DIALOG_CANNOT_COMPLY_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOT_COMPLY_DESC"), "dialogGenericMessage"), null);
									return;
								}
							}
							else
							{
								if (eventName == "DifferentSystemsEvent" && int.Parse(eventParams[0]) == this._widgetID)
								{
									int fleetID3 = int.Parse(eventParams[1]);
									int fleetID4 = int.Parse(eventParams[2]);
									FleetInfo fleetInfo4 = base.App.GameDatabase.GetFleetInfo(fleetID3);
									FleetInfo fleetInfo5 = base.App.GameDatabase.GetFleetInfo(fleetID4);
									if (fleetInfo4 != null && fleetInfo5 != null)
									{
										base.App.UI.CreateDialog(new GenericTextDialog(base.App, App.Localize("@UI_FLEET_DIALOG_CANNOT_TRANSFER_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_CANNOT_TRANSFER_DESC"), fleetInfo4.Name, fleetInfo5.Name), "dialogGenericMessage"), null);
										return;
									}
								}
								else
								{
									if (eventName == "SelectionChanged" && int.Parse(eventParams[0]) == this._widgetID)
									{
										this._selectedFleet = int.Parse(eventParams[1]);
										if (this.OnFleetSelectionChanged != null)
										{
											this.OnFleetSelectionChanged(this._game, this._selectedFleet);
											return;
										}
									}
									else
									{
										if (eventName == "SetPreferred" && int.Parse(eventParams[0]) == this._widgetID)
										{
											this._selectedFleet = int.Parse(eventParams[1]);
											FleetInfo fleetInfo6 = base.App.GameDatabase.GetFleetInfo(this._selectedFleet);
											if (fleetInfo6 == null)
											{
												return;
											}
											List<FleetInfo> list = base.App.GameDatabase.GetFleetsByPlayerAndSystem(fleetInfo6.PlayerID, fleetInfo6.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
											using (List<FleetInfo>.Enumerator enumerator2 = list.GetEnumerator())
											{
												while (enumerator2.MoveNext())
												{
													FleetInfo current2 = enumerator2.Current;
													if (current2.ID != this._selectedFleet)
													{
														base.App.GameDatabase.UpdateFleetPreferred(current2.ID, false);
													}
													else
													{
														base.App.GameDatabase.UpdateFleetPreferred(current2.ID, true);
													}
												}
												return;
											}
										}
										if (eventName == "ConfirmRepairs" && int.Parse(eventParams[0]) == this._widgetID)
										{
											int num6 = 1;
											int num7 = int.Parse(eventParams[num6]);
											num6++;
											for (int j = 0; j < num7; j++)
											{
												int shipID = int.Parse(eventParams[num6]);
												int points = int.Parse(eventParams[num6 + 1]);
												ShipInfo shipInfo3 = base.App.GameDatabase.GetShipInfo(shipID, true);
                                                Kerberos.Sots.StarFleet.StarFleet.RepairShip(base.App, shipInfo3, points);
												num6 += 2;
											}
											num7 = int.Parse(eventParams[num6]);
											num6++;
											for (int k = 0; k < num7; k++)
											{
												bool flag = int.Parse(eventParams[num6]) > 0;
												int num8 = int.Parse(eventParams[num6 + 1]);
												int num9 = int.Parse(eventParams[num6 + 2]);
												num6 += 3;
												if (flag)
												{
													ColonyInfo colonyInfo = base.App.GameDatabase.GetColonyInfo(num8);
													if (colonyInfo != null)
													{
														colonyInfo.RepairPoints = num9;
														base.App.GameDatabase.UpdateColony(colonyInfo);
													}
												}
												else
												{
													int num10 = 0;
													ShipInfo shipInfo4 = base.App.GameDatabase.GetShipInfo(num8, true);
													List<SectionInstanceInfo> list2 = base.App.GameDatabase.GetShipSectionInstances(num8).ToList<SectionInstanceInfo>();
													Dictionary<SectionInstanceInfo, List<ModuleInstanceInfo>> dictionary = new Dictionary<SectionInstanceInfo, List<ModuleInstanceInfo>>();
													Dictionary<SectionInstanceInfo, int> dictionary2 = new Dictionary<SectionInstanceInfo, int>();
													int num11 = shipInfo4.DesignInfo.DesignSections.Count<DesignSectionInfo>();
													for (int l = 0; l < num11; l++)
													{
														dictionary2.Add(list2[l], list2[l].Structure);
														num10 += list2[l].RepairPoints;
														dictionary.Add(list2[l], base.App.GameDatabase.GetModuleInstances(list2[l].ID).ToList<ModuleInstanceInfo>());
														if (dictionary[list2[l]].Count == shipInfo4.DesignInfo.DesignSections[l].Modules.Count<DesignModuleInfo>())
														{
															for (int m = 0; m < dictionary[list2[l]].Count; m++)
															{
																if (dictionary[list2[l]][m].Structure > 0)
																{
																	num10 += dictionary[list2[l]][m].RepairPoints;
																	string modAsset = base.App.GameDatabase.GetModuleAsset(shipInfo4.DesignInfo.DesignSections[l].Modules[m].ModuleID);
																	LogicalModule logicalModule = (
																		from x in base.App.AssetDatabase.Modules
																		where x.ModulePath == modAsset
																		select x).First<LogicalModule>();
																	Dictionary<SectionInstanceInfo, int> dictionary3;
																	SectionInstanceInfo key;
																	(dictionary3 = dictionary2)[key = list2[l]] = dictionary3[key] + (int)logicalModule.StructureBonus;
																}
															}
														}
													}
													int num12 = num10 - num9;
													if (num12 > 0)
													{
														foreach (SectionInstanceInfo current3 in list2)
														{
															if (dictionary2[current3] > 0)
															{
																if (current3.RepairPoints > 0)
																{
																	int num13 = current3.RepairPoints - num12;
																	if (num13 > 0)
																	{
																		current3.RepairPoints -= num12;
																		num12 = 0;
																	}
																	else
																	{
																		num12 -= current3.RepairPoints;
																		current3.RepairPoints = 0;
																	}
																	this._game.GameDatabase.UpdateSectionInstance(current3);
																}
																if (num12 > 0)
																{
																	foreach (ModuleInstanceInfo current4 in dictionary[current3])
																	{
																		if (current4.Structure > 0 && current4.RepairPoints > 0)
																		{
																			int num14 = current4.RepairPoints - num12;
																			if (num14 > 0)
																			{
																				current4.RepairPoints -= num12;
																				num12 = 0;
																			}
																			else
																			{
																				num12 -= current4.RepairPoints;
																				current4.RepairPoints = 0;
																			}
																			this._game.GameDatabase.UpdateModuleInstance(current4);
																		}
																		if (num12 <= 0)
																		{
																			break;
																		}
																	}
																}
																if (num12 <= 0)
																{
																	break;
																}
															}
														}
													}
												}
											}
											return;
										}
										if (eventName == "ConfirmSuulkaStructure" && int.Parse(eventParams[0]) == this._widgetID)
										{
											int shipID2 = int.Parse(eventParams[1]);
											int points2 = int.Parse(eventParams[2]);
											int orbitalObjectID = int.Parse(eventParams[3]);
											double num15 = double.Parse(eventParams[4]);
											double num16 = double.Parse(eventParams[5]);
											double num17 = num15 - num16;
											ShipInfo shipInfo5 = base.App.GameDatabase.GetShipInfo(shipID2, true);
											PlanetInfo planetInfo = base.App.GameDatabase.GetPlanetInfo(orbitalObjectID);
											if (shipInfo5 != null && planetInfo != null)
											{
                                                Kerberos.Sots.StarFleet.StarFleet.RepairShip(base.App, shipInfo5, points2);
												ColonyInfo colonyInfoForPlanet = base.App.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
												if (colonyInfoForPlanet == null)
												{
													return;
												}
												double num18 = base.App.GameDatabase.GetCivilianPopulation(planetInfo.ID, 0, false);
												double num19 = 0.0;
												if (colonyInfoForPlanet != null)
												{
													num19 = colonyInfoForPlanet.ImperialPop;
												}
												if (num17 > num18)
												{
													num17 -= num18;
													num18 = 0.0;
												}
												else
												{
													num18 -= num17;
													num17 = 0.0;
												}
												if (num17 > num19)
												{
													num17 -= num19;
													num19 = 0.0;
												}
												else
												{
													num19 -= num17;
												}
												IEnumerable<ColonyFactionInfo> civilianPopulations = base.App.GameDatabase.GetCivilianPopulations(planetInfo.ID);
												double civilianPop = num18 / (double)civilianPopulations.Count<ColonyFactionInfo>();
												foreach (ColonyFactionInfo current5 in civilianPopulations)
												{
													current5.CivilianPop = civilianPop;
													base.App.GameDatabase.UpdateCivilianPopulation(current5);
												}
												colonyInfoForPlanet.ImperialPop = num19;
												base.App.GameDatabase.UpdateColony(colonyInfoForPlanet);
												SuulkaType suulkaType = shipInfo5.DesignInfo.DesignSections[0].ShipSectionAsset.SuulkaType;
												int num20 = (int)((suulkaType == SuulkaType.TheBlack) ? SuulkaType.TheHidden : suulkaType);
												string cueName = string.Format("STRAT_118-0{0}_{1}_SuulkaLifeDrain", num20, base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)));
												base.App.PostRequestSpeech(cueName, 50, 120, 0f);
												return;
											}
										}
										else
										{
											if (eventName == "ConfirmSuulkaPsi" && int.Parse(eventParams[0]) == this._widgetID)
											{
												int shipID3 = int.Parse(eventParams[1]);
												int num21 = int.Parse(eventParams[2]);
												int orbitalObjectID2 = int.Parse(eventParams[3]);
												int biosphere = int.Parse(eventParams[4]);
												ShipInfo shipInfo6 = base.App.GameDatabase.GetShipInfo(shipID3, true);
												PlanetInfo planetInfo2 = base.App.GameDatabase.GetPlanetInfo(orbitalObjectID2);
												if (shipInfo6 != null && planetInfo2 != null)
												{
													shipInfo6.PsionicPower += num21;
													base.App.GameDatabase.UpdateShipPsionicPower(shipInfo6.ID, shipInfo6.PsionicPower);
													planetInfo2.Biosphere = biosphere;
													base.App.GameDatabase.UpdatePlanet(planetInfo2);
													SuulkaType suulkaType2 = shipInfo6.DesignInfo.DesignSections[0].ShipSectionAsset.SuulkaType;
													int num22 = (int)((suulkaType2 == SuulkaType.TheBlack) ? SuulkaType.TheHidden : suulkaType2);
													string cueName2 = string.Format("STRAT_119-0{0}_{1}_SuulkaPsiDrain", num22, base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)));
													base.App.PostRequestSpeech(cueName2, 50, 120, 0f);
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
		private bool AdmiralAvailable(int playerid, int systemid)
		{
			return (
				from x in base.App.GameDatabase.GetAdmiralInfosForPlayer(playerid)
				where base.App.GameDatabase.GetFleetInfoByAdmiralID(x.ID, FleetType.FL_NORMAL) == null
				select x).Any<AdmiralInfo>();
		}
		private bool CommandShipAvailable(int playerid, int systemid)
		{
			ShipInfo result = null;
			int? reserveFleetID = base.App.GameDatabase.GetReserveFleetID(base.App.LocalPlayer.ID, systemid);
			if (reserveFleetID.HasValue)
			{
				IEnumerable<ShipInfo> shipInfoByFleetID = base.App.GameDatabase.GetShipInfoByFleetID(reserveFleetID.Value, false);
				foreach (ShipInfo current in shipInfoByFleetID)
				{
					if (base.App.LocalPlayer.Faction.Name == "loa")
					{
						result = current;
						break;
					}
					if (base.App.GameDatabase.GetShipCommandPointQuota(current.ID) > 0)
					{
						result = current;
						break;
					}
				}
			}
			return result != null;
		}
		protected void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!this._enabled)
			{
				return;
			}
			if (msgType == "button_clicked")
			{
				if (panelName.StartsWith("gameRenameFleetButton"))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					if (array.Count<string>() > 1 && int.Parse(array[1]) == this._widgetID)
					{
						FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
						if (fleetInfo != null)
						{
							this._fleetNameDialog = base.App.UI.CreateDialog(new GenericTextEntryDialog(base.App, "ENTER FLEET NAME", "Enter a name for your fleet:", fleetInfo.Name, 40, 0, true, EditBoxFilterMode.None), null);
							return;
						}
					}
				}
				else
				{
					if (panelName.StartsWith("admiralbutton"))
					{
						string[] array2 = panelName.Split(new char[]
						{
							'|'
						});
						if (array2.Count<string>() > 1 && this._enableAdmiralButton)
						{
							int admiralID = int.Parse(array2[1]);
							AdmiralInfo admiralInfo = this._game.GameDatabase.GetAdmiralInfo(admiralID);
							if (admiralInfo != null && FleetWidget.admiralPanel == null)
							{
								FleetWidget.admiralPanel = this._game.UI.CreateDialog(new AdmiralInfoDialog(this._game, admiralID, "admiralPopUp"), null);
								return;
							}
						}
					}
					else
					{
						if (panelName.StartsWith("createfleetbutton"))
						{
							string[] array3 = panelName.Split(new char[]
							{
								'|'
							});
							if (array3.Count<string>() > 1)
							{
								bool flag = this.AdmiralAvailable(base.App.LocalPlayer.ID, int.Parse(array3[1]));
								bool flag2 = this.CommandShipAvailable(base.App.LocalPlayer.ID, int.Parse(array3[1]));
								if (!flag || !flag2)
								{
									base.App.UI.CreateDialog(new GenericTextDialog(base.App, App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_DESC"), "dialogGenericMessage"), null);
									return;
								}
								if (FleetWidget.createfleetpanel == null)
								{
									FleetWidget.createfleetpanel = base.App.UI.CreateDialog(new SelectAdmiralDialog(base.App, int.Parse(array3[1]), "dialogSelectAdmiral"), null);
									return;
								}
							}
						}
						else
						{
							if (panelName.StartsWith("gameDissolveFleetButton"))
							{
								string[] array4 = panelName.Split(new char[]
								{
									'|'
								});
								if (array4.Count<string>() > 1 && int.Parse(array4[1]) == this._widgetID)
								{
									FleetInfo fleetInfo2 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
									if (fleetInfo2 != null)
									{
										this._dissolveFleetDialog = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, App.Localize("@UI_FLEET_DIALOG_DISSOLVEFLEET_TITLE"), string.Format(App.Localize("@UI_FLEET_DIALOG_DISSOLVEFLEET_DESC"), fleetInfo2.Name), "dialogGenericQuestion"), null);
										return;
									}
								}
							}
							else
							{
								if (panelName.StartsWith("gameCancelMissionButton"))
								{
									string[] array5 = panelName.Split(new char[]
									{
										'|'
									});
									if (array5.Count<string>() > 1 && int.Parse(array5[1]) == this._widgetID)
									{
										FleetInfo fleetInfo3 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
										if (fleetInfo3 != null)
										{
											this._cancelMissionDialog = base.App.UI.CreateDialog(new GenericQuestionDialog(base.App, App.Localize("@UI_FLEET_DIALOG_CANCELMISSION_TITLE"), App.Localize("@CANCELMISSIONTEXT"), "dialogGenericQuestion"), null);
											return;
										}
									}
								}
								else
								{
									if (panelName.StartsWith("gameFleetSurveyMissionButton"))
									{
										string[] array6 = panelName.Split(new char[]
										{
											'|'
										});
										if (array6.Count<string>() > 1 && int.Parse(array6[1]) == this._widgetID)
										{
											FleetInfo fleetInfo4 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
											if (fleetInfo4 != null && base.App.CurrentState.Name == "StarMapState")
											{
												StarMapState starMapState = (StarMapState)base.App.CurrentState;
												starMapState.ShowFleetCentricOverlay(MissionType.SURVEY, fleetInfo4.ID);
												return;
											}
										}
									}
									else
									{
										if (panelName.StartsWith("gameFleetLoaDissolveToCube"))
										{
											FleetInfo fleetInfo5 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
											if (fleetInfo5 != null)
											{
                                                Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(base.App.Game, fleetInfo5.ID);
												this._fleetsChanged = true;
												this._contentChanged = true;
												this.Refresh();
												return;
											}
										}
										else
										{
											if (panelName.StartsWith("gameFleetSetLoaComposition"))
											{
												FleetInfo fleetInfo6 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
												if (fleetInfo6 != null)
												{
													MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(fleetInfo6.ID);
													FleetWidget.SelectCompositionPanel = base.App.UI.CreateDialog(new DialogLoaFleetSelector(base.App, (missionByFleetID != null) ? missionByFleetID.Type : MissionType.NO_MISSION, fleetInfo6, false), null);
													return;
												}
											}
											else
											{
												if (panelName.StartsWith("gameFleetColonizeMissionButton"))
												{
													string[] array7 = panelName.Split(new char[]
													{
														'|'
													});
													if (array7.Count<string>() > 1 && int.Parse(array7[1]) == this._widgetID)
													{
														FleetInfo fleetInfo7 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
														if (fleetInfo7 != null && base.App.CurrentState.Name == "StarMapState")
														{
															StarMapState starMapState2 = (StarMapState)base.App.CurrentState;
															starMapState2.ShowFleetCentricOverlay(MissionType.COLONIZATION, fleetInfo7.ID);
															return;
														}
													}
												}
												else
												{
													if (panelName.StartsWith("gameFleetEvacuateButton"))
													{
														string[] array8 = panelName.Split(new char[]
														{
															'|'
														});
														if (array8.Count<string>() > 1 && int.Parse(array8[1]) == this._widgetID)
														{
															FleetInfo fleetInfo8 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
															if (fleetInfo8 != null && base.App.CurrentState.Name == "StarMapState")
															{
																StarMapState starMapState3 = (StarMapState)base.App.CurrentState;
																starMapState3.ShowFleetCentricOverlay(MissionType.EVACUATE, fleetInfo8.ID);
																return;
															}
														}
													}
													else
													{
														if (panelName.StartsWith("gameFleetSuportMissionButton"))
														{
															string[] array9 = panelName.Split(new char[]
															{
																'|'
															});
															if (array9.Count<string>() > 1 && int.Parse(array9[1]) == this._widgetID)
															{
																FleetInfo fleetInfo9 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
																if (fleetInfo9 != null && base.App.CurrentState.Name == "StarMapState")
																{
																	StarMapState starMapState4 = (StarMapState)base.App.CurrentState;
																	starMapState4.ShowFleetCentricOverlay(MissionType.SUPPORT, fleetInfo9.ID);
																	return;
																}
															}
														}
														else
														{
															if (panelName.StartsWith("gameFleetRelocateMissionButton"))
															{
																string[] array10 = panelName.Split(new char[]
																{
																	'|'
																});
																if (array10.Count<string>() > 1 && int.Parse(array10[1]) == this._widgetID)
																{
																	FleetInfo fleetInfo10 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
																	if (fleetInfo10 != null && base.App.CurrentState.Name == "StarMapState")
																	{
																		StarMapState starMapState5 = (StarMapState)base.App.CurrentState;
																		starMapState5.ShowFleetCentricOverlay(MissionType.RELOCATION, fleetInfo10.ID);
																		return;
																	}
																}
															}
															else
															{
																if (panelName.StartsWith("gameFleetPatrolMissionButton"))
																{
																	string[] array11 = panelName.Split(new char[]
																	{
																		'|'
																	});
																	if (array11.Count<string>() > 1 && int.Parse(array11[1]) == this._widgetID)
																	{
																		FleetInfo fleetInfo11 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
																		if (fleetInfo11 != null && base.App.CurrentState.Name == "StarMapState")
																		{
																			StarMapState starMapState6 = (StarMapState)base.App.CurrentState;
																			starMapState6.ShowFleetCentricOverlay(MissionType.PATROL, fleetInfo11.ID);
																			return;
																		}
																	}
																}
																else
																{
																	if (panelName.StartsWith("gameFleetInterdictMissionButton"))
																	{
																		string[] array12 = panelName.Split(new char[]
																		{
																			'|'
																		});
																		if (array12.Count<string>() > 1 && int.Parse(array12[1]) == this._widgetID)
																		{
																			FleetInfo fleetInfo12 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
																			if (fleetInfo12 != null && base.App.CurrentState.Name == "StarMapState")
																			{
																				StarMapState starMapState7 = (StarMapState)base.App.CurrentState;
																				starMapState7.ShowFleetCentricOverlay(MissionType.INTERDICTION, fleetInfo12.ID);
																				return;
																			}
																		}
																	}
																	else
																	{
																		if (panelName.StartsWith("gameFleetInvadeMissionButton"))
																		{
																			string[] array13 = panelName.Split(new char[]
																			{
																				'|'
																			});
																			if (array13.Count<string>() > 1 && int.Parse(array13[1]) == this._widgetID)
																			{
																				FleetInfo fleetInfo13 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
																				if (fleetInfo13 != null && base.App.CurrentState.Name == "StarMapState")
																				{
																					StarMapState starMapState8 = (StarMapState)base.App.CurrentState;
																					starMapState8.ShowFleetCentricOverlay(MissionType.INVASION, fleetInfo13.ID);
																					return;
																				}
																			}
																		}
																		else
																		{
																			if (panelName.StartsWith("gameFleetStrikeMissionButton"))
																			{
																				string[] array14 = panelName.Split(new char[]
																				{
																					'|'
																				});
																				if (array14.Count<string>() > 1 && int.Parse(array14[1]) == this._widgetID)
																				{
																					FleetInfo fleetInfo14 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
																					if (fleetInfo14 != null && base.App.CurrentState.Name == "StarMapState")
																					{
																						StarMapState starMapState9 = (StarMapState)base.App.CurrentState;
																						starMapState9.ShowFleetCentricOverlay(MissionType.STRIKE, fleetInfo14.ID);
																						return;
																					}
																				}
																			}
																			else
																			{
																				if (panelName.StartsWith("gameFleetPiracyButton"))
																				{
																					string[] array15 = panelName.Split(new char[]
																					{
																						'|'
																					});
																					if (array15.Count<string>() > 1 && int.Parse(array15[1]) == this._widgetID)
																					{
																						FleetInfo fleetInfo15 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
																						if (fleetInfo15 != null && base.App.CurrentState.Name == "StarMapState")
																						{
																							StarMapState starMapState10 = (StarMapState)base.App.CurrentState;
																							starMapState10.ShowFleetCentricOverlay(MissionType.PIRACY, fleetInfo15.ID);
																							return;
																						}
																					}
																				}
																				else
																				{
																					if (panelName.StartsWith("gameRetrofitShip"))
																					{
																						string[] array16 = panelName.Split(new char[]
																						{
																							'|'
																						});
																						if (array16.Count<string>() > 1 && int.Parse(array16[1]) == this._widgetID)
																						{
																							ShipInfo shipInfo = base.App.GameDatabase.GetShipInfo(this._contextSlot, true);
																							if (shipInfo != null)
																							{
																								if (shipInfo.DesignInfo.Class == ShipClass.Station && shipInfo.DesignInfo.StationType != StationType.INVALID_TYPE)
																								{
																									this._retrofitShipDialog = base.App.UI.CreateDialog(new RetrofitStationDialog(base.App, shipInfo), null);
																									return;
																								}
																								this._retrofitShipDialog = base.App.UI.CreateDialog(new RetrofitShipDialog(base.App, shipInfo), null);
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
			else
			{
				if (msgType == "dialog_closed")
				{
					if (panelName == this._fleetNameDialog)
					{
						if (bool.Parse(msgParams[0]) && msgParams[1].Length > 0)
						{
							FleetInfo fleetInfo16 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
							if (fleetInfo16 != null)
							{
								fleetInfo16.Name = msgParams[1];
								base.App.GameDatabase.UpdateFleetInfo(fleetInfo16);
								this._fleetsChanged = true;
								this._contentChanged = true;
								this.Refresh();
								return;
							}
						}
					}
					else
					{
						if (panelName == this._LoaCubeTransferDialog)
						{
							if (msgParams.Count<string>() == 4)
							{
								int num = int.Parse(msgParams[0]);
								int num2 = int.Parse(msgParams[1]);
								int shipID = int.Parse(msgParams[2]);
								int num3 = int.Parse(msgParams[3]);
								ShipInfo shipInfo2 = base.App.GameDatabase.GetShipInfo(shipID, true);
								ShipInfo shipInfo3 = base.App.GameDatabase.GetShipInfoByFleetID(num, false).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
								if (shipInfo3 == null)
								{
									base.App.GameDatabase.InsertShip(num, shipInfo2.DesignInfo.ID, "Cube", (ShipParams)0, null, num3);
								}
								else
								{
									base.App.GameDatabase.UpdateShipLoaCubes(shipInfo3.ID, shipInfo3.LoaCubes + num3);
								}
								if (shipInfo2.LoaCubes <= num3)
								{
									int fleetID = shipInfo2.FleetID;
									base.App.GameDatabase.RemoveShip(shipInfo2.ID);
									if (base.App.GameDatabase.GetShipsByFleetID(fleetID).Count<int>() == 0 && base.App.GameDatabase.GetFleetInfo(fleetID).Type != FleetType.FL_RESERVE)
									{
										base.App.GameDatabase.RemoveFleet(fleetID);
									}
								}
								else
								{
									base.App.GameDatabase.UpdateShipLoaCubes(shipInfo2.ID, shipInfo2.LoaCubes - num3);
								}
								if (this.SyncedFleets.Contains(num))
								{
									this.SyncFleetInfo(base.App.GameDatabase.GetFleetInfo(num));
								}
								if (this.SyncedFleets.Contains(num2))
								{
									this.SyncFleetInfo(base.App.GameDatabase.GetFleetInfo(num2));
								}
								foreach (FleetWidget current in this._linkedWidgets)
								{
									if (current.SyncedFleets != null && current.SyncedFleets.Count<int>() >= 0)
									{
										if (current.SyncedFleets.Contains(num))
										{
											current.SyncFleetInfo(base.App.GameDatabase.GetFleetInfo(num));
										}
										if (current.SyncedFleets.Contains(num2))
										{
											current.SyncFleetInfo(base.App.GameDatabase.GetFleetInfo(num2));
										}
									}
								}
								if (this.OnFleetsModified != null)
								{
									this.OnFleetsModified(base.App, new int[]
									{
										num,
										num2
									});
								}
								this._contentChanged = true;
								this._fleetsChanged = true;
								this.Refresh();
								return;
							}
						}
						else
						{
							if (panelName == FleetWidget.SelectCompositionPanel)
							{
								if (msgParams[0] != "")
								{
									int num4 = int.Parse(msgParams[0]);
									if (num4 != 0)
									{
										FleetInfo fleetInfo17 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
										if (fleetInfo17 != null)
										{
                                            Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(base.App.Game, fleetInfo17.ID);
											base.App.GameDatabase.UpdateFleetCompositionID(fleetInfo17.ID, new int?(num4));
                                            Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(base.App.Game, fleetInfo17.ID, MissionType.NO_MISSION);
											this._fleetsChanged = true;
											this._contentChanged = true;
											this.Refresh();
											return;
										}
									}
								}
							}
							else
							{
								if (panelName == this._retrofitShipDialog)
								{
									this._fleetsChanged = true;
									this._contentChanged = true;
									this.Refresh();
									return;
								}
								if (panelName == FleetWidget.admiralPanel)
								{
									FleetWidget.admiralPanel = null;
									return;
								}
								if (panelName == FleetWidget.createfleetpanel)
								{
									FleetWidget.createfleetpanel = null;
									return;
								}
								if (panelName == this._scrapDialog)
								{
									if (bool.Parse(msgParams[0]))
									{
										List<string> list = new List<string>();
										int num5 = 0;
										bool flag3 = false;
										foreach (int ship in this._shipsToScrap)
										{
											list.Add(base.App.GameDatabase.GetShipInfo(ship, false).ShipName);
											num5 = base.App.GameDatabase.GetShipInfo(ship, false).FleetID;
											if (base.App.GameDatabase.GetShipInfo(ship, true).DesignInfo.Class == ShipClass.Station && !base.App.GameDatabase.GetShipInfo(ship, true).IsPlatform())
											{
												StationInfo stationInfo = base.App.GameDatabase.GetStationInfos().FirstOrDefault((StationInfo x) => x.ShipID == ship);
												if (stationInfo != null)
												{
													base.App.GameDatabase.DestroyStation(base.App.Game, stationInfo.OrbitalObjectID, 0);
													flag3 = true;
												}
											}
											else
											{
												base.App.GameDatabase.RemoveShip(ship);
											}
											FleetInfo fleetInfo18 = base.App.GameDatabase.GetFleetInfo(num5);
											if (fleetInfo18 != null && fleetInfo18.Type != FleetType.FL_DEFENSE && fleetInfo18.Type != FleetType.FL_RESERVE && fleetInfo18.Type != FleetType.FL_STATION && !base.App.GameDatabase.GetShipsByFleetID(num5).Any<int>())
											{
												base.App.GameDatabase.RemoveFleet(num5);
												this._syncedFleets.Remove(num5);
											}
										}
										this.SetSyncedFleets(this._syncedFleets);
										if (flag3 && base.App.CurrentState.Name == "StarMapState")
										{
											StarMapState starMapState11 = (StarMapState)base.App.CurrentState;
											starMapState11.RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
										}
										if (list.Count > 0)
										{
											string text = "";
											for (int i = 0; i < list.Count - 1; i++)
											{
												text = text + list[i] + ", ";
											}
											text += list[list.Count - 1];
											base.App.GameDatabase.InsertTurnEvent(new TurnEvent
											{
												EventType = TurnEventType.EV_SHIPS_RECYCLED,
												EventMessage = TurnEventMessage.EM_SHIPS_RECYCLED,
												PlayerID = base.App.LocalPlayer.ID,
												SystemID = (num5 != 0) ? ((base.App.GameDatabase.GetFleetInfo(num5) != null) ? base.App.GameDatabase.GetFleetInfo(num5).SystemID : 0) : 0,
												NamesList = text,
												TurnNumber = base.App.GameDatabase.GetTurnCount(),
												ShowsDialog = false
											});
											return;
										}
									}
								}
								else
								{
									if (panelName == this._dissolveFleetDialog)
									{
										if (bool.Parse(msgParams[0]))
										{
											FleetInfo fleetInfo19 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
											if (fleetInfo19 != null)
											{
												int? reserveFleetID = base.App.GameDatabase.GetReserveFleetID(base.App.LocalPlayer.ID, fleetInfo19.SystemID);
												if (reserveFleetID.HasValue)
												{
													this.DissolveFleet(fleetInfo19.ID, reserveFleetID.Value);
													if (base.App.CurrentState.Name == "StarMapState")
													{
														StarMapState starMapState12 = (StarMapState)base.App.CurrentState;
														starMapState12.RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
													}
												}
												if ((fleetInfo19.IsAcceleratorFleet || fleetInfo19.IsGateFleet) && !reserveFleetID.HasValue)
												{
													base.App.GameDatabase.RemoveFleet(fleetInfo19.ID);
													if (base.App.CurrentState.Name == "StarMapState")
													{
														StarMapState starMapState13 = (StarMapState)base.App.CurrentState;
														starMapState13.RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_ALL);
														return;
													}
												}
											}
										}
									}
									else
									{
										if (panelName == this._cancelMissionDialog && bool.Parse(msgParams[0]))
										{
											FleetInfo fleetInfo20 = base.App.GameDatabase.GetFleetInfo(this._contextSlot);
											if (fleetInfo20 != null)
											{
												AdmiralInfo admiralInfo2 = base.App.GameDatabase.GetAdmiralInfo(fleetInfo20.AdmiralID);
												if (admiralInfo2 != null)
												{
													string cueName = string.Format("STRAT_008-01_{0}_{1}UniversalMissionNegation", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)), admiralInfo2.GetAdmiralSoundCueContext(base.App.AssetDatabase));
													base.App.PostRequestSpeech(cueName, 50, 120, 0f);
												}
                                                Kerberos.Sots.StarFleet.StarFleet.CancelMission(base.App.Game, fleetInfo20, true);
												this._fleetsChanged = true;
												this._contentChanged = true;
												this.Refresh();
												if (typeof(StarMapState).IsAssignableFrom(base.App.CurrentState.GetType()))
												{
													StarMapState.UpdateGateUI(base.App.Game, "gameGateInfo");
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
		public void Dispose()
		{
			base.App.UI.DestroyPanel(this._contextMenuID);
			base.App.UI.DestroyPanel(this._shipcontextMenuID);
			this._ShipToolTip.Dispose(false);
			this._game.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._game.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			if (this._game != null)
			{
				this._game.ReleaseObject(this);
			}
		}
	}
}
