using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal abstract class OverlayMission : Dialog
	{
		protected const string UIFleetList = "gameFleetList";
		protected const string UISystemList = "gameSystemList";
		protected const string UIRelocateFleetList = "gameRelocateFleet";
		protected const string UISystemMap = "partMiniSystem";
		protected const string UIAvailableFleetsList = "invalid";
		protected const string UIFleetVitalsList = "invalid";
		protected const string UIFleetEscortsList = "invalid";
		protected const string UISystemDetails = "systemDetailsWidget";
		protected const string UIMissionTimes = "gameMissionTimes";
		protected const string UIMissionAdmiralName = "gameAdmiralName";
		protected const string UIMissionAdmiralFleet = "gameAdmiralFleet";
		protected const string UIMissionAdmiralSkills = "gameAdmiralSkills";
		protected const string UIMissionAdmiralAvatar = "gameAdmiralAvatar";
		protected const string UIMissionTitle = "gameMissionTitle";
		protected const string UIConfirmMissionButton = "gameConfirmMissionButton";
		protected const string UIConfirmMissionAndContinueButton = "gameConfirmAndContinueMissionButton";
		protected const string UICancelMissionButton = "gameCancelMissionButton";
		protected const string UIMissionPlanetDetails = "gameMissionPlanet";
		protected const string UIMissionNotes = "gameMissionNotes";
		protected const string UIToggleRebase = "gameRebaseToggle";
		protected const string UIToggleRebaseStn = "gameRebaseToggleStn";
		protected const string UI2bbackground = "2B_Background";
		protected const string UI3bbackground = "3B_Background";
		protected const string UILYSlider = "LYSlider";
		protected const string UILYLabel = "right_label";
		protected const string UIAccelListBox = "AccelListBox";
		protected const string UIAccelList = "AccelList";
		protected const string UIPlaceAccelButton = "gamePlaceAccelerator";
		protected const int UIItemMissionTotalTime = 0;
		protected const int UIItemMissionTravelTime = 1;
		protected const int UIItemMissionTime = 2;
		protected const int UIItemMissionBuildTime = 3;
		protected const int UIItemMissionCostSeparator = 4;
		protected const int UIItemMissionCost = 5;
		protected const int UIItemMissionSupportTime = 6;
		protected App App;
		private readonly string SystemButton = "systembtn";
		private IGameObject _selectedObject;
		protected int _selectedFleet;
		protected int _selectedPlanet;
		private int _nextMissionNoteItemId = 1;
		protected MissionEstimate _missionEstimate;
		protected FleetWidget _fleetWidget;
		private bool _canConfirm;
		public bool BlindEnter;
		protected bool _useDirectRoute;
		private string _zuulConfirm;
		private string _zuulBoreMissing;
		private string _zuulBoreRoute;
		private string _loaDirectRoute;
		private string _loaShearFleetConfirm;
		private string _loaFleetCompoSel;
		protected SystemWidget _systemWidget;
		private MissionType _missionType;
		private List<int> _ActiveSystems;
		protected bool _fleetCentric;
		protected bool _canExit;
		private int _TargetSystem;
		private int _rebasetarget;
		private readonly List<ShipItem> _vitals = new List<ShipItem>();
		private readonly List<ShipItem> _escorts = new List<ShipItem>();
		private readonly List<ShipItem> _additionalShips = new List<ShipItem>();
		protected bool PathDrawEnabled;
		private bool _rebaseToggle;
		protected StarMap _starMap;
		private StarMapState _starMapState;
		private StarMapViewFilter _lastFilter;
		private bool _shown;
		private string MissionScreen
		{
			get;
			set;
		}
		public int TargetSystem
		{
			get
			{
				return this._TargetSystem;
			}
			set
			{
				if (this._rebaseToggle)
				{
					this.RebaseTarget = new int?(value);
					this.UpdateRebaseUI();
					return;
				}
				this._TargetSystem = value;
			}
		}
		protected int? RebaseTarget
		{
			get
			{
				if (this._rebasetarget != 0)
				{
					return new int?(this._rebasetarget);
				}
				return null;
			}
			set
			{
				this._rebasetarget = value.Value;
			}
		}
		public bool CanConfirm
		{
			get
			{
				return this._canConfirm;
			}
		}
		public MissionType MissionType
		{
			get
			{
				return this._missionType;
			}
		}
		protected bool ReBaseToggle
		{
			get
			{
				return this._rebaseToggle;
			}
			set
			{
				if (this._rebaseToggle == value)
				{
					return;
				}
				this.RebaseMode(value);
			}
		}
		public int SelectedPlanet
		{
			get
			{
				return this._selectedPlanet;
			}
			set
			{
				if (this._selectedPlanet == value)
				{
					return;
				}
				this._selectedPlanet = value;
				bool value2 = false;
				this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, this.SelectedSystem);
				if (this.SelectedPlanet != StarSystemDetailsUI.StarItemID)
				{
					PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(this.SelectedPlanet);
					if (planetInfo != null && StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
					{
						value2 = true;
					}
				}
				this.App.UI.SetVisible("gameMissionPlanet", value2);
				this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
			}
		}
		private int SelectedSystem
		{
			get
			{
				StarMapSystem starMapSystem = this._selectedObject as StarMapSystem;
				if (starMapSystem == null)
				{
					return 0;
				}
				StarMapState gameState = this.App.GetGameState<StarMapState>();
				int result;
				if (gameState.StarMap.Systems.Forward.TryGetValue(starMapSystem, out result))
				{
					return result;
				}
				return 0;
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
				if (this._selectedFleet != value)
				{
					this._selectedFleet = value;
					this.RebuildShipLists(this.SelectedFleet);
					this.RefreshMissionDetails(this.GetSelectedStationtype(), 1);
					this.UpdateCanConfirmMission();
					this._starMapState.ClearStarmapFleetArrows();
					this._starMap.ClearSystemEffects();
					FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._selectedFleet);
					List<Vector3> list = new List<Vector3>();
					if (fleetInfo != null)
					{
						PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
						StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
						Vector3? vector = null;
						if (this.MissionType == MissionType.INTERCEPT || this.MissionType == MissionType.SPECIAL_CONSTRUCT_STN)
						{
							int target = (this.MissionType == MissionType.SPECIAL_CONSTRUCT_STN) ? ((OverlaySpecialConstructionMission)this).TargetFleet : ((OverlayInterceptMission)this).TargetFleet;
							if (this._starMap.Fleets.Reverse.ContainsKey(target))
							{
								StarMapFleet value2 = this._starMap.Fleets.Reverse.FirstOrDefault((KeyValuePair<int, StarMapFleet> x) => x.Key == target).Value;
								if (value2 != null)
								{
									vector = new Vector3?(value2.Position);
								}
							}
						}
						else
						{
							StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(this.TargetSystem);
							if (starSystemInfo2 != null && starSystemInfo != null)
							{
								this._starMap.SetMissionEffectTarget(starSystemInfo2, true);
								vector = new Vector3?(starSystemInfo2.Origin);
								int num;
								float num2;
                                List<int> bestTravelPath = Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this.App.Game, fleetInfo.ID, starSystemInfo.ID, starSystemInfo2.ID, out num, out num2, false, null, null);
								foreach (int current in bestTravelPath)
								{
									list.Add(this.App.GameDatabase.GetStarSystemOrigin(current));
								}
							}
						}
						if (starSystemInfo != null && vector.HasValue && this.PathDrawEnabled)
						{
							if (list.Count == 0)
							{
								this._starMapState.AddMissionFleetArrow(starSystemInfo.Origin, vector.Value, playerInfo.PrimaryColor);
							}
							else
							{
								this._starMapState.AddMissionFleetArrow(list, playerInfo.PrimaryColor);
							}
						}
					}
					if (this.ReBaseToggle)
					{
						this.ReBaseToggle = false;
					}
					this.RebaseTarget = new int?(0);
					this.UpdateRebaseUI();
				}
			}
		}
		protected abstract bool CanConfirmMission();
		protected abstract void OnCommitMission();
		protected abstract void OnCanConfirmMissionChanged(bool newValue);
		protected abstract string GetMissionDetailsTitle();
		protected abstract void OnRefreshMissionDetails(MissionEstimate estimate);
		protected abstract IEnumerable<int> GetMissionTargetPlanets();
		protected bool IsValidFleetID(int fleetID)
		{
			return fleetID != 0 && !this.App.GameDatabase.GetFleetInfo(fleetID).IsReserveFleet;
		}
		public static string GetAdmiralTraitText(AdmiralInfo.TraitType trait)
		{
			switch (trait)
			{
			case AdmiralInfo.TraitType.Thrifty:
				return App.Localize("@ADMIRALTRAITS_THRIFTY");
			case AdmiralInfo.TraitType.Wastrel:
				return App.Localize("@ADMIRALTRAITS_WASTREL");
			case AdmiralInfo.TraitType.Pathfinder:
				return App.Localize("@ADMIRALTRAITS_PATHFINDER");
			case AdmiralInfo.TraitType.Slippery:
				return App.Localize("@ADMIRALTRAITS_SLIPPERY");
			case AdmiralInfo.TraitType.Livingstone:
				return App.Localize("@ADMIRALTRAITS_LIVINGSTONE");
			case AdmiralInfo.TraitType.Conscript:
				return App.Localize("@ADMIRALTRAITS_CONSCRIPT");
			case AdmiralInfo.TraitType.TrueBeliever:
				return App.Localize("@ADMIRALTRAITS_TRUEBELIEVER");
			case AdmiralInfo.TraitType.GoodShepherd:
				return App.Localize("@ADMIRALTRAITS_GOODSHEPHERD");
			case AdmiralInfo.TraitType.BadShepherd:
				return App.Localize("@ADMIRALTRAITS_BADSHEPHERD");
			case AdmiralInfo.TraitType.GreenThumb:
				return App.Localize("@ADMIRALTRAITS_GREENTHUMB");
			case AdmiralInfo.TraitType.BlackThumb:
				return App.Localize("@ADMIRALTRAITS_BLACKTHUMB");
			case AdmiralInfo.TraitType.DrillSergeant:
				return App.Localize("@ADMIRALTRAITS_DRILLSERGEANT");
			case AdmiralInfo.TraitType.Vigilant:
				return App.Localize("@ADMIRALTRAITS_VIGILANT");
			case AdmiralInfo.TraitType.Architect:
				return App.Localize("@ADMIRALTRAITS_ARCHITECT");
			case AdmiralInfo.TraitType.Inquisitor:
				return App.Localize("@ADMIRALTRAITS_INQUISITOR");
			case AdmiralInfo.TraitType.Evangelist:
				return App.Localize("@ADMIRALTRAITS_EVANGELIST");
			case AdmiralInfo.TraitType.HeadHunter:
				return App.Localize("@ADMIRALTRAITS_HEAD_HUNTER");
			case AdmiralInfo.TraitType.TrueGrit:
				return App.Localize("@ADMIRALTRAITS_TRUEGRIT");
			case AdmiralInfo.TraitType.Hunter:
				return App.Localize("@ADMIRALTRAITS_HUNTER");
			case AdmiralInfo.TraitType.Defender:
				return App.Localize("@ADMIRALTRAITS_DEFENDER");
			case AdmiralInfo.TraitType.Attacker:
				return App.Localize("@ADMIRALTRAITS_ATTACKER");
			case AdmiralInfo.TraitType.ArtilleryExpert:
				return App.Localize("@ADMIRALTRAITS_ARTILLERYEXPERT");
			case AdmiralInfo.TraitType.Psion:
				return App.Localize("@ADMIRALTRAITS_PSION");
			case AdmiralInfo.TraitType.Skeptic:
				return App.Localize("@ADMIRALTRAITS_SKEPTIC");
			case AdmiralInfo.TraitType.MediaHero:
				return App.Localize("@ADMIRALTRAITS_MEDIAHERO");
			case AdmiralInfo.TraitType.GloryHound:
				return App.Localize("@ADMIRALTRAITS_GLORYHOUND");
			case AdmiralInfo.TraitType.Sherman:
				return App.Localize("@ADMIRALTRAITS_SHERMAN");
			case AdmiralInfo.TraitType.Technophobe:
				return App.Localize("@ADMIRALTRAITS_TECHNOPHOBE");
			case AdmiralInfo.TraitType.Elite:
				return App.Localize("@ADMIRALTRAITS_ELITE");
			default:
				throw new ArgumentOutOfRangeException("trait");
			}
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
					text += OverlayMission.GetAdmiralTraitText(traitType);
				}
			}
			return text;
		}
		public void UpdateRebaseUI()
		{
			if (this.SelectedFleet == 0)
			{
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gameRebaseTarget"
				}), false);
				this.App.UI.SetText(base.UI.Path(new string[]
				{
					base.ID,
					"gameEnterRebase"
				}), App.Localize("@UI_REBASE_TARGET"));
				return;
			}
			if (this.RebaseTarget.HasValue)
			{
				StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(this.RebaseTarget.Value);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gameRebaseTarget"
				}), true);
				this.App.UI.SetText(base.UI.Path(new string[]
				{
					base.ID,
					"gameRebaseTarget"
				}), string.Format(App.Localize("@UI_FLEET_REBASE_DESC"), starSystemInfo.Name));
				if (this._rebaseToggle)
				{
					this.App.UI.SetText(base.UI.Path(new string[]
					{
						base.ID,
						"gameEnterRebase"
					}), App.Localize("@UI_DONE_REBASE"));
					return;
				}
				this.App.UI.SetText(base.UI.Path(new string[]
				{
					base.ID,
					"gameEnterRebase"
				}), App.Localize("@UI_REBASE_TARGET"));
				return;
			}
			else
			{
				if (this._rebaseToggle)
				{
					this.App.UI.SetVisible(base.UI.Path(new string[]
					{
						base.ID,
						"gameRebaseTarget"
					}), true);
					this.App.UI.SetText(base.UI.Path(new string[]
					{
						base.ID,
						"gameRebaseTarget"
					}), App.Localize("@UI_SELECT_REBASE_SYSTEM"));
					this.App.UI.SetText(base.UI.Path(new string[]
					{
						base.ID,
						"gameEnterRebase"
					}), App.Localize("@UI_DONE_REBASE"));
					return;
				}
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gameRebaseTarget"
				}), false);
				this.App.UI.SetText(base.UI.Path(new string[]
				{
					base.ID,
					"gameEnterRebase"
				}), App.Localize("@UI_REBASE_TARGET"));
				return;
			}
		}
		public void RebaseMode(bool mode)
		{
			if (mode)
			{
				this.CompileListOfRelocatableSystemsForFleet(this.SelectedFleet, true);
				this._starMap.SelectEnabled = true;
				this._starMap.FocusEnabled = true;
                if (Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(this._app.Game, this.TargetSystem, this.SelectedFleet) || this.MissionType == MissionType.COLONIZATION)
				{
					this.RebaseTarget = new int?(this.TargetSystem);
				}
			}
			else
			{
				if (this._starMap.Systems.Reverse.ContainsKey(this.TargetSystem))
				{
					StarMapSystem value = this._starMap.Systems.Reverse.FirstOrDefault((KeyValuePair<int, StarMapSystem> x) => x.Key == this.TargetSystem).Value;
					this._starMap.Select(value);
					this._starMap.SetFocus(value);
				}
				this._starMap.SelectEnabled = false;
				this._starMap.FocusEnabled = false;
				this.SystemCentricSystemUIRefresh();
			}
			this._rebaseToggle = mode;
			this.UpdateRebaseUI();
		}
		public List<int> CompileListOfRelocatableSystemsForFleet(int fleetid, bool updatestarmap = true)
		{
			if (updatestarmap)
			{
				foreach (int current in this._starMap.Systems.Reverse.Keys)
				{
					this._starMap.Systems.Reverse[current].SetIsEnabled(false);
					this._starMap.Systems.Reverse[current].SetIsSelectable(false);
				}
			}
			List<StarSystemInfo> source = this.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(fleetid);
			List<int> list = new List<int>();
			foreach (int current2 in 
				from x in source
				select x.ID)
			{
                if (Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(this._app.Game, current2, fleetid))
				{
					list.Add(current2);
				}
			}
			if (!list.Contains(fleetInfo.SupportingSystemID))
			{
				list.Add(fleetInfo.SupportingSystemID);
			}
			if (updatestarmap)
			{
				foreach (int current3 in this._starMap.Systems.Reverse.Keys)
				{
					this._starMap.Systems.Reverse[current3].SetIsEnabled(list.Contains(current3));
					this._starMap.Systems.Reverse[current3].SetIsSelectable(list.Contains(current3));
				}
			}
			return list;
		}
		public static void RefreshFleetAdmiralDetails(App _app, string ID, int fleetId, string Element = "admiralDetails")
		{
			FleetInfo fleetInfo = _app.GameDatabase.GetFleetInfo(fleetId);
			_app.GameDatabase.GetFactionName(_app.GameDatabase.GetFleetFaction(fleetId));
			string arg_2A_0 = string.Empty;
			string arg_30_0 = string.Empty;
			if (fleetInfo.AdmiralID != 0)
			{
				_app.UI.SetVisible(_app.UI.Path(new string[]
				{
					ID,
					Element,
					"traitsLabel"
				}), true);
				AdmiralInfo admiralInfo = _app.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
				string arg_88_0 = admiralInfo.Name;
				OverlayMission.GetAdmiralTraitsString(_app, fleetInfo.AdmiralID);
				string arg_9C_0 = admiralInfo.Race;
				string arg = "Deep Space";
				if (admiralInfo.HomeworldID.HasValue)
				{
					OrbitalObjectInfo orbitalObjectInfo = _app.GameDatabase.GetOrbitalObjectInfo(admiralInfo.HomeworldID.Value);
					if (orbitalObjectInfo != null)
					{
						FleetInfo fleetInfoByAdmiralID = _app.GameDatabase.GetFleetInfoByAdmiralID(admiralInfo.ID, FleetType.FL_NORMAL);
						if (fleetInfoByAdmiralID != null)
						{
							StarSystemInfo starSystemInfo = _app.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
							if (starSystemInfo != null)
							{
								arg = starSystemInfo.Name;
							}
						}
						else
						{
							arg = _app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo.StarSystemID).Name;
						}
					}
				}
				_app.UI.SetPropertyString(_app.UI.Path(new string[]
				{
					ID,
					"admiralName"
				}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_NAME_COLON"), admiralInfo.Name));
				_app.UI.SetPropertyString(_app.UI.Path(new string[]
				{
					ID,
					"admiralLocation"
				}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_LOCATION_COLON"), arg));
				_app.UI.SetPropertyString(_app.UI.Path(new string[]
				{
					ID,
					"admiralAge"
				}), "text", string.Format(App.Localize("@ADMIRAL_DIALOG_AGE_COLON"), ((int)admiralInfo.Age).ToString()));
                string admiralAvatar = Kerberos.Sots.StarFleet.StarFleet.GetAdmiralAvatar(_app, admiralInfo.ID);
				_app.UI.SetPropertyString(_app.UI.Path(new string[]
				{
					ID,
					"admiralImage"
				}), "sprite", admiralAvatar);
				IEnumerable<AdmiralInfo.TraitType> admiralTraits = _app.GameDatabase.GetAdmiralTraits(admiralInfo.ID);
				_app.UI.ClearItems(_app.UI.Path(new string[]
				{
					ID,
					"admiralTraits"
				}));
				int num = 0;
				foreach (AdmiralInfo.TraitType current in admiralTraits)
				{
					string text = OverlayMission.GetAdmiralTraitText(current);
					if (current != admiralTraits.Last<AdmiralInfo.TraitType>())
					{
						text += ", ";
					}
					_app.UI.AddItem(_app.UI.Path(new string[]
					{
						ID,
						"admiralTraits"
					}), "", num, "");
					string itemGlobalID = _app.UI.GetItemGlobalID(_app.UI.Path(new string[]
					{
						ID,
						"admiralTraits"
					}), "", num, "");
					num++;
					_app.UI.SetPropertyString(itemGlobalID, "text", text);
					if (AdmiralInfo.IsGoodTrait(current))
					{
						_app.UI.SetPropertyColorNormalized(itemGlobalID, "color", new Vector3(0f, 1f, 0f));
					}
					else
					{
						_app.UI.SetPropertyColorNormalized(itemGlobalID, "color", new Vector3(1f, 0f, 0f));
					}
					_app.UI.SetTooltip(itemGlobalID, AdmiralInfo.GetTraitDescription(current, _app.GameDatabase.GetLevelForAdmiralTrait(admiralInfo.ID, current)));
				}
			}
		}
		protected void RebuildShipLists(int fleetId)
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(fleetId);
			if (fleetInfo != null)
			{
				IEnumerable<ShipInfo> shipInfoByFleetID = this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false);
                List<int> missionCapableShips = Kerberos.Sots.StarFleet.StarFleet.GetMissionCapableShips(this.App.Game, fleetInfo.ID, this._missionType);
				this._vitals.Clear();
				this._escorts.Clear();
				List<int> list = null;
				if (missionCapableShips.Count == 0)
				{
					list = DesignLab.GetMissionRequiredDesigns(this.App.Game, this._missionType, this.App.LocalPlayer.ID);
				}
				if (list != null)
				{
					foreach (int current in list)
					{
						ShipItem shipItem = new ShipItem(new ShipInfo
						{
							DesignID = current,
							DesignInfo = this.App.GameDatabase.GetDesignInfo(current)
						});
						shipItem.NumAdded++;
						this._vitals.Add(shipItem);
					}
				}
				foreach (ShipInfo current2 in shipInfoByFleetID)
				{
					if (missionCapableShips.Contains(current2.ID))
					{
						this._vitals.Add(new ShipItem(current2));
					}
					else
					{
						this._escorts.Add(new ShipItem(current2));
					}
				}
			}
		}
		protected void SyncShipListEscorts()
		{
			this.App.UI.ClearItems("invalid");
			foreach (ShipItem current in this._escorts)
			{
				this.App.UI.AddItem("invalid", "name", current.ShipID, current.Name);
			}
		}
		protected void AddCommonMissionTimes(MissionEstimate estimate)
		{
			OverlayMission.AddCommonMissionTimes(this.App, estimate);
		}
		public static void AddCommonMissionTimes(App game, MissionEstimate estimate)
		{
			OverlayMission.AddMissionTime(game, 0, App.Localize("@MISSIONWIDGET_TOTAL_MISSION_TIME"), estimate.TotalTurns, string.Empty);
			if (estimate.TurnsForConstruction > 0)
			{
				OverlayMission.AddMissionTime(game, 3, App.Localize("@MISSIONWIDGET_BUILD_TIME"), estimate.TurnsForConstruction, string.Empty);
			}
			if (estimate.TotalTravelTurns > 0)
			{
				string hint = App.Localize("@SURVEYMISSION_HINT");
				OverlayMission.AddMissionTime(game, 1, App.Localize("@MISSIONWIDGET_TRAVEL_TIME"), estimate.TotalTravelTurns, hint);
			}
		}
		public static void AddMissionEstimate(App game, int itemId, string label, string value, string units, string incButtonId, string decButtonId, string hint)
		{
			game.UI.AddItem("gameMissionTimes", string.Empty, itemId, string.Empty);
			game.UI.SetItemPropertyString("gameMissionTimes", string.Empty, itemId, "label", "text", label);
			game.UI.SetItemPropertyString("gameMissionTimes", string.Empty, itemId, "value", "text", value);
			game.UI.SetItemPropertyString("gameMissionTimes", string.Empty, itemId, "units", "text", units);
			if (!string.IsNullOrEmpty(hint))
			{
				game.UI.SetItemPropertyString("gameMissionTimes", string.Empty, itemId, "hint", "text", hint);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					game.UI.GetItemGlobalID("gameMissionTimes", string.Empty, itemId, string.Empty),
					"hint"
				}), true);
				return;
			}
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				game.UI.GetItemGlobalID("gameMissionTimes", string.Empty, itemId, string.Empty),
				"hint"
			}), false);
		}
		protected void AddMissionTime(int itemId, string label, int numTurns, string hint)
		{
			OverlayMission.AddMissionTime(this.App, itemId, label, numTurns, hint);
		}
		public static void AddMissionTime(App game, int itemId, string label, int numTurns, string hint)
		{
			string value = string.Format("{0}", numTurns);
			string units = (numTurns == 1) ? App.Localize("@TURN") : App.Localize("@TURNS");
			OverlayMission.AddMissionEstimate(game, itemId, label, value, units, string.Empty, string.Empty, hint);
		}
		protected void AddMissionNote(string note)
		{
			this.App.UI.AddItem("gameMissionNotes", string.Empty, this._nextMissionNoteItemId++, note);
		}
		protected void AddMissionCost(MissionEstimate estimate)
		{
			OverlayMission.AddMissionCost(this.App, estimate);
		}
		public static void AddMissionCost(App game, MissionEstimate estimate)
		{
			string value = string.Format("{0}", estimate.ConstructionCost.ToString("N0"));
			OverlayMission.AddMissionEstimate(game, 4, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			OverlayMission.AddMissionEstimate(game, 5, App.Localize("@MISSIONWIDGET_COST"), value, App.Localize("@MISSIONWIDGET_CREDITS"), string.Empty, string.Empty, string.Empty);
		}
		protected List<int> GetDesignsToBuild()
		{
			List<int> designs = new List<int>();
			this._vitals.ForEach(delegate(ShipItem x)
			{
				designs.AddRange(Enumerable.Repeat<int>(x.DesignID, x.NumAdded));
			});
			this._escorts.ForEach(delegate(ShipItem x)
			{
				designs.AddRange(Enumerable.Repeat<int>(x.DesignID, x.NumAdded));
			});
			this._additionalShips.ForEach(delegate(ShipItem x)
			{
				designs.AddRange(Enumerable.Repeat<int>(x.DesignID, x.NumAdded));
			});
			return designs;
		}
		protected virtual void RefreshMissionDetails(StationType type = StationType.INVALID_TYPE, int stationLevel = 1)
		{
			if (this.TargetSystem == 0)
			{
				return;
			}
			string text = this.GetMissionDetailsTitle();
			if (this.SelectedFleet != 0)
			{
				if (this.MissionType != MissionType.REACTION)
				{
					OverlayMission.RefreshFleetAdmiralDetails(this.App, base.ID, this.SelectedFleet, "admiralDetails");
				}
				this.App.UI.ClearItems("gameMissionTimes");
				this.App.UI.ClearItems("gameMissionNotes");
                this._missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this.App.Game, this._missionType, type, this.SelectedFleet, this.TargetSystem, this.SelectedPlanet, this.GetDesignsToBuild(), stationLevel, this.ReBaseToggle, null, null);
				text += string.Format(App.Localize("@UI_MISSION_ETA_TURNS"), this._missionEstimate.TotalTurns);
				this.OnRefreshMissionDetails(this._missionEstimate);
				this.App.UI.AutoSizeContents("gameMissionDetails");
			}
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				"gameMissionTitle"
			}), text);
		}
		private void RefreshConstructionCount()
		{
			IEnumerable<FleetInfo> enumerable = this.CollectAvailableFleets(this._missionType, this.TargetSystem);
			this.App.GameDatabase.GetStarSystemInfo(this.TargetSystem);
			foreach (FleetInfo current in enumerable)
			{
				string text = string.Format("FleetItem{0}", current.ID);
				IEnumerable<ShipInfo> shipInfoByFleetID = this.App.GameDatabase.GetShipInfoByFleetID(current.ID, false);
				string listId = this.App.UI.Path(new string[]
				{
					"invalid",
					text,
					"invalid"
				});
				foreach (ShipItem item in this._vitals)
				{
					int num = shipInfoByFleetID.Count((ShipInfo x) => x.DesignID == item.DesignID);
					this.App.UI.SetItemPropertyString(listId, "count", item.ShipID, string.Empty, "text", string.Concat(num + item.NumAdded));
				}
			}
		}
		public OverlayMission(App game, StarMapState state, StarMap starmap, MissionType missionType, string template = "OverlaySurveyMission") : base(game, template)
		{
			this._starMapState = state;
			this._starMap = starmap;
			StarMapState expr_50 = this._starMapState;
			expr_50.OnObjectSelectionChanged = (StarMapState.ObjectSelectionChangedDelegate)Delegate.Combine(expr_50.OnObjectSelectionChanged, new StarMapState.ObjectSelectionChangedDelegate(this.OnStarmapSelectedObjectChanged));
			this.App = game;
			this._missionType = missionType;
		}
		public void Show(int system)
		{
			if (base.ID != "")
			{
				this.App.UI.DestroyPanel(base.ID);
			}
			base.SetID(Guid.NewGuid().ToString());
			this.App.UI.CreateOverlay(this, null);
			this._fleetCentric = false;
			this.TargetSystem = system;
			this.App.UI.ShowOverlay(this);
		}
		public void ShowFleetCentric(int fleetid)
		{
			if (base.ID != "")
			{
				this.App.UI.DestroyPanel(base.ID);
			}
			base.SetID(Guid.NewGuid().ToString());
			this.App.UI.CreateOverlay(this, null);
			this._fleetCentric = true;
			this.TargetSystem = 0;
			this.SelectedFleet = fleetid;
			this.App.UI.ShowOverlay(this);
		}
		public bool GetShown()
		{
			return this._shown;
		}
		public void Hide()
		{
			if (!this._canExit)
			{
				return;
			}
			this.App.UI.Send(new object[]
			{
				"SetWidthProp",
				"OH_StarMap",
				"parent:width"
			});
			this.App.GetGameState<StarMapState>().ShowInterface = true;
			this.App.GetGameState<StarMapState>().RightClickEnabled = true;
			this.App.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
			this.OnExit();
			this._shown = false;
			this.App.UI.CloseDialog(this, false);
		}
		public override void Initialize()
		{
			this._fleetWidget = new FleetWidget(this.App, this.App.UI.Path(new string[]
			{
				base.ID,
				"gameFleetList"
			}));
			this._systemWidget = new SystemWidget(this.App, this.App.UI.Path(new string[]
			{
				base.ID,
				"starDetailsCard"
			}));
		}
		protected virtual void OnEnter()
		{
			this._canConfirm = false;
			this._useDirectRoute = false;
			this._canExit = true;
			this.ReBaseToggle = false;
			if (!this._fleetCentric)
			{
				this.SelectedFleet = 0;
			}
			this.PathDrawEnabled = true;
			if (this.MissionType == MissionType.SURVEY || this.MissionType == MissionType.INTERCEPT || this.MissionType == MissionType.PIRACY || this.MissionType == MissionType.DEPLOY_NPG || this._fleetCentric)
			{
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gameConfirmAndContinueMissionButton"
				}), false);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"2B_Background"
				}), true);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"3B_Background"
				}), false);
			}
			else
			{
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gameConfirmAndContinueMissionButton"
				}), true);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"2B_Background"
				}), false);
				this.App.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"3B_Background"
				}), true);
			}
			this.App.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"LYSlider"
			}), this.MissionType == MissionType.DEPLOY_NPG);
			if (this.MissionType == MissionType.INTERCEPT)
			{
				int target = ((OverlayInterceptMission)this).TargetFleet;
				if (this._starMap.Fleets.Reverse.ContainsKey(target))
				{
					StarMapFleet value = this._starMap.Fleets.Reverse.FirstOrDefault((KeyValuePair<int, StarMapFleet> x) => x.Key == target).Value;
					this._starMap.Select(value);
				}
			}
			else
			{
				if (this._fleetCentric && this.TargetSystem == 0)
				{
					this.TargetSystem = this._app.GameDatabase.GetFleetInfo(this.SelectedFleet).SystemID;
				}
				if (this._starMap.Systems.Reverse.ContainsKey(this.TargetSystem))
				{
					StarMapSystem value2 = this._starMap.Systems.Reverse.FirstOrDefault((KeyValuePair<int, StarMapSystem> x) => x.Key == this.TargetSystem).Value;
					this._starMap.Select(value2);
				}
			}
			this._starMap.SelectEnabled = this._fleetCentric;
			this._starMap.FocusEnabled = this._fleetCentric;
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._fleetWidget.MissionMode = this._missionType;
			this._fleetWidget.EnemySelectionEnabled = false;
            List<FleetInfo> list = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, this.TargetSystem, this._missionType, true).ToList<FleetInfo>();
			this._fleetWidget.SetSyncedFleets(list);
			List<int> list2 = new List<int>();
			List<int> list3 = new List<int>();
			list2.Add(this.TargetSystem);
			foreach (FleetInfo current in list)
			{
				if (!list2.Contains(current.SystemID))
				{
					list2.Add(current.SystemID);
				}
			}
			if (this._fleetCentric)
			{
                foreach (int current2 in Kerberos.Sots.StarFleet.StarFleet.CollectAvailableSystemsForFleetMission(this.App.Game.GameDatabase, this.App.Game, this.SelectedFleet, this._fleetWidget.MissionMode, false))
				{
					if (!list2.Contains(current2))
					{
						list2.Add(current2);
					}
					if (!list3.Contains(current2))
					{
						list3.Add(current2);
					}
				}
				if (list3.Any<int>())
				{
					this.TargetSystem = list3.First<int>();
					this.FocusOnStarSystem(this.TargetSystem);
				}
				if (this._systemWidget != null)
				{
					this._systemWidget.Sync(this.TargetSystem);
				}
				this._fleetWidget.SetSyncedFleets(this.SelectedFleet);
				this._fleetWidget.SelectedFleet = this.SelectedFleet;
			}
			foreach (int current3 in this._starMap.Systems.Reverse.Keys)
			{
				this._starMap.Systems.Reverse[current3].SetIsEnabled(list2.Contains(current3));
				this._starMap.Systems.Reverse[current3].SetIsSelectable(list3.Contains(current3));
			}
			foreach (int current4 in this._starMap.Fleets.Reverse.Keys)
			{
				this._starMap.Fleets.Reverse[current4].SetIsSelectable(false);
			}
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"fleetMission"
			}), this._fleetCentric);
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"systemMission"
			}), !this._fleetCentric);
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"gameFleetList"
			}), !this._fleetCentric);
			this.BuildSystemList(list3);
			this.UIListSelectSystem(this.TargetSystem);
			this._lastFilter = this._starMap.ViewFilter;
			this._starMap.ViewFilter = StarMapViewFilter.VF_MISSION;
			this._starMapState.ClearStarmapFleetArrows();
			this._starMap.ClearSystemEffects();
			this.UpdateCanConfirmMission();
			this.UpdateRebaseUI();
		}
		private void BuildSystemList(List<int> selectablesystems)
		{
			foreach (int current in selectablesystems)
			{
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(current);
				this.App.UI.AddItem(this.App.UI.Path(new string[]
				{
					base.ID,
					"gameSystemList"
				}), "", current, "A sys", "TinySystemCard_Button");
				string itemGlobalID = this._app.UI.GetItemGlobalID(this.App.UI.Path(new string[]
				{
					base.ID,
					"gameSystemList"
				}), "", starSystemInfo.ID, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					this.SystemButton
				}), "id", this.SystemButton + "|" + starSystemInfo.ID);
				this._app.UI.GetGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"unselected"
				}));
				this._app.UI.GetGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"selected"
				}));
				this._app.UI.GetGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"contentselected"
				}));
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"itemName"
				}), starSystemInfo.Name);
				StellarClass stellarClass = new StellarClass(starSystemInfo.StellarClass);
				Vector4 vector = StarHelper.CalcModelColor(stellarClass);
				this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"colorGradient"
				}), "color", new Vector3(vector.X, vector.Y, vector.Z) * 255f);
			}
			this._ActiveSystems = selectablesystems;
		}
		private void UIListSelectSystem(int selectedsystem)
		{
			foreach (int current in this._ActiveSystems)
			{
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(current);
				string itemGlobalID = this._app.UI.GetItemGlobalID(this.App.UI.Path(new string[]
				{
					base.ID,
					"gameSystemList"
				}), "", starSystemInfo.ID, "");
				string globalID = this._app.UI.GetGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"unselected"
				}));
				string globalID2 = this._app.UI.GetGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"selected"
				}));
				this._app.UI.GetGlobalID(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"contentselected"
				}));
				this._app.UI.SetVisible(globalID, current != selectedsystem);
				this._app.UI.SetVisible(globalID2, current == selectedsystem);
			}
		}
		public void FocusOnStarSystem(int TargetSystem)
		{
			if (this._starMap.Systems.Reverse.ContainsKey(TargetSystem))
			{
				StarMapSystem value = this._starMap.Systems.Reverse.FirstOrDefault((KeyValuePair<int, StarMapSystem> x) => x.Key == TargetSystem).Value;
				this._starMap.Select(value);
			}
		}
		protected virtual void SystemCentricSystemUIRefresh()
		{
			foreach (int current in this._starMap.Systems.Reverse.Keys)
			{
				this._starMap.Systems.Reverse[current].SetIsEnabled(false);
				this._starMap.Systems.Reverse[current].SetIsSelectable(false);
			}
            List<FleetInfo> list = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, this.TargetSystem, this._missionType, true).ToList<FleetInfo>();
			List<int> list2 = new List<int>();
			list2.Add(this.TargetSystem);
			foreach (FleetInfo current2 in list)
			{
				if (!list2.Contains(current2.SystemID))
				{
					list2.Add(current2.SystemID);
				}
			}
			foreach (int current3 in this._starMap.Systems.Reverse.Keys)
			{
				this._starMap.Systems.Reverse[current3].SetIsEnabled(list2.Contains(current3));
			}
		}
		protected virtual void OnExit()
		{
			foreach (int current in this._starMap.Systems.Reverse.Keys)
			{
				this._starMap.Systems.Reverse[current].SetIsEnabled(true);
				this._starMap.Systems.Reverse[current].SetIsSelectable(true);
			}
			foreach (int current2 in this._starMap.Fleets.Reverse.Keys)
			{
				this._starMap.Fleets.Reverse[current2].SetIsSelectable(true);
			}
			this._starMap.ViewFilter = this._lastFilter;
			this._starMapState._lastFilterSelection = this._lastFilter;
			this._selectedObject = null;
			this.SelectedFleet = 0;
			this.SelectedPlanet = 0;
			this.TargetSystem = 0;
			this._fleetWidget.SelectedFleet = 0;
			this._starMap.SelectEnabled = true;
			this._starMap.FocusEnabled = true;
			this._starMapState.RefreshMission();
		}
		protected virtual StationType GetSelectedStationtype()
		{
			return StationType.INVALID_TYPE;
		}
		protected bool CheckZuulHasBore()
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
            return !GameSession.FleetHasBore(this.App.GameDatabase, this._selectedFleet) && Kerberos.Sots.StarFleet.StarFleet.GetNodeTravelPath(this.App.GameDatabase, fleetInfo.SystemID, this.TargetSystem, fleetInfo.PlayerID, false, true, false).Count<int>() == 0;
		}
		protected bool CheckZuulDirectRoute()
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
			int num = 0;
			int num2 = 0;
			float num3 = 0f;
			float num4 = 0f;
            Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this.App.Game, this._selectedFleet, fleetInfo.SystemID, this.TargetSystem, out num, out num3, true, null, null);
            Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(this.App.Game, this._selectedFleet, fleetInfo.SystemID, this.TargetSystem, out num2, out num4, false, null, null);
			return num3 != num4;
		}
		protected bool CheckZuulNodeHealth()
		{
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).SystemID);
			if (this._useDirectRoute)
			{
				return (
					from x in this.App.GameDatabase.GetExploredNodeLinesFromSystem(this.App.LocalPlayer.ID, starSystemInfo.ID)
					where x.Health > -1
					select x).Count<NodeLineInfo>() >= GameSession.GetPlayerSystemBoreLineLimit(this.App.GameDatabase, this.App.LocalPlayer.ID);
			}
			return false;
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "gameCancelMissionButton")
				{
					this._canExit = true;
					this.Hide();
					return;
				}
				if (panelName == "gameConfirmMissionButton")
				{
					this._canExit = true;
					this.TryCommitMission();
					return;
				}
				if (panelName == "gameConfirmAndContinueMissionButton")
				{
					this._canExit = false;
					this.TryCommitMission();
					return;
				}
				if (panelName == "gameEnterRebase")
				{
					this.ReBaseToggle = !this.ReBaseToggle;
					this.UpdateCanConfirmMission();
					return;
				}
				if (panelName.StartsWith(this.SystemButton))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					int sysid = int.Parse(array[1]);
					if (this._starMap.Systems.Reverse.ContainsKey(sysid))
					{
						StarMapSystem value = this._starMap.Systems.Reverse.FirstOrDefault((KeyValuePair<int, StarMapSystem> x) => x.Key == sysid).Value;
						this._starMap.Select(value);
						this._starMap.SetFocus(value);
						return;
					}
				}
				else
				{
					if (panelName == "planetsTab")
					{
						this.App.UI.SetVisible(this.App.UI.Path(new string[]
						{
							base.ID,
							"gameSystemList"
						}), true);
						this.App.UI.SetVisible(this.App.UI.Path(new string[]
						{
							base.ID,
							"gameFleetList"
						}), false);
						this.App.UI.SetChecked(this.App.UI.Path(new string[]
						{
							base.ID,
							"fleetMission",
							"planetsTab"
						}), true);
						this.App.UI.SetChecked(this.App.UI.Path(new string[]
						{
							base.ID,
							"fleetMission",
							"fleetsTab"
						}), false);
						return;
					}
					if (panelName == "fleetsTab")
					{
						this.App.UI.SetVisible(this.App.UI.Path(new string[]
						{
							base.ID,
							"gameSystemList"
						}), false);
						this.App.UI.SetVisible(this.App.UI.Path(new string[]
						{
							base.ID,
							"gameFleetList"
						}), true);
						this.App.UI.SetChecked(this.App.UI.Path(new string[]
						{
							base.ID,
							"fleetMission",
							"planetsTab"
						}), false);
						this.App.UI.SetChecked(this.App.UI.Path(new string[]
						{
							base.ID,
							"fleetMission",
							"fleetsTab"
						}), true);
						return;
					}
				}
			}
			else
			{
				if (msgType == "list_sel_changed")
				{
					return;
				}
				if (msgType == "dialog_closed")
				{
					if (panelName == this._loaDirectRoute)
					{
						this._useDirectRoute = (msgParams[0] == "True");
						this.LoaTryCommitMission();
						return;
					}
					if (panelName == this._zuulBoreMissing)
					{
						GenericYesNoDialog.YesNoDialogResult yesNoDialogResult = (GenericYesNoDialog.YesNoDialogResult)Enum.Parse(typeof(GenericYesNoDialog.YesNoDialogResult), msgParams[0]);
						if (yesNoDialogResult == GenericYesNoDialog.YesNoDialogResult.No)
						{
							DesignInfo designInfo = null;
							IEnumerable<DesignInfo> designInfosForPlayer = this.App.GameDatabase.GetDesignInfosForPlayer(this.App.LocalPlayer.ID);
							foreach (DesignInfo current in designInfosForPlayer)
							{
								DesignSectionInfo[] designSections = current.DesignSections;
								for (int i = 0; i < designSections.Length; i++)
								{
									DesignSectionInfo designSectionInfo = designSections[i];
									ShipSectionAsset shipSectionAsset = this.App.GameDatabase.AssetDatabase.GetShipSectionAsset(designSectionInfo.FilePath);
									if (shipSectionAsset.IsBoreShip)
									{
										designInfo = current;
										break;
									}
								}
								if (designInfo != null)
								{
									break;
								}
							}
							if (designInfo != null)
							{
								ShipItem shipItem = new ShipItem(new ShipInfo
								{
									DesignID = designInfo.ID,
									DesignInfo = designInfo
								});
								shipItem.NumAdded++;
								this._additionalShips.Add(shipItem);
							}
							StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).SystemID);
							if (this.CheckZuulDirectRoute())
							{
								this._zuulBoreRoute = this.App.UI.CreateDialog(new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), App.Localize("@UI_CONFIRM_BORE_LINE_MISSION"), "dialogBoreRouteQuestion"), null);
								return;
							}
							if (this.CheckZuulNodeHealth())
							{
								this._zuulConfirm = this.App.UI.CreateDialog(new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), string.Format(App.Localize("@UI_CONFIRM_BORE_LINE_MISSION_MAX"), starSystemInfo.Name), "dialogGenericQuestion"), null);
								return;
							}
							this.CommitMission();
							return;
						}
						else
						{
							if (yesNoDialogResult == GenericYesNoDialog.YesNoDialogResult.Yes)
							{
								this.CommitMission();
								return;
							}
						}
					}
					else
					{
						if (panelName == this._zuulBoreRoute)
						{
							this._useDirectRoute = (msgParams[0] == "True");
							StarSystemInfo starSystemInfo2 = this._app.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetFleetInfo(this._selectedFleet).SystemID);
							if (this.CheckZuulNodeHealth())
							{
								this._zuulConfirm = this.App.UI.CreateDialog(new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), string.Format(App.Localize("@UI_CONFIRM_BORE_LINE_MISSION_MAX"), starSystemInfo2.Name), "dialogGenericQuestion"), null);
								return;
							}
							this.CommitMission();
							return;
						}
						else
						{
							if (panelName == this._loaShearFleetConfirm)
							{
								bool flag = msgParams[0] == "True";
								if (flag)
								{
									this._app.Game.CheckLoaFleetGateCompliancy(this._app.GameDatabase.GetFleetInfo(this._selectedFleet));
									this._fleetWidget.Refresh();
								}
								this._loaFleetCompoSel = this.App.UI.CreateDialog(new DialogLoaFleetSelector(this.App, this._missionType, this._app.GameDatabase.GetFleetInfo(this._selectedFleet), false), null);
								return;
							}
							if (panelName == this._loaFleetCompoSel)
							{
								if (msgParams[0] != "")
								{
									int value2 = int.Parse(msgParams[0]);
									this._app.GameDatabase.UpdateFleetCompositionID(this._selectedFleet, new int?(value2));
									this.CommitMission();
									return;
								}
							}
							else
							{
								if (panelName == this._zuulConfirm && msgParams[0] == "True" && this.CanConfirmMission())
								{
									this.CommitMission();
									return;
								}
							}
						}
					}
				}
				else
				{
					if (msgType == "checkbox_clicked")
					{
						bool reBaseToggle = int.Parse(msgParams[0]) > 0;
						if (panelName == "gameRebaseToggle")
						{
							this.ReBaseToggle = reBaseToggle;
							this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
							return;
						}
					}
					else
					{
						if (msgType == "dialog_opened" && panelName == base.ID)
						{
							this.App.UI.SetVisible(base.ID, true);
							this.App.UI.UnlockUI();
							this.App.GetGameState<StarMapState>().ShowInterface = false;
							this.App.GetGameState<StarMapState>().RightClickEnabled = false;
							this.App.UI.Send(new object[]
							{
								"SetWidthProp",
								"OH_StarMap",
								"parent:width+280"
							});
							this.TargetSystem = this.TargetSystem;
							StarSystemUI.SyncSystemDetailsWidget(this.App, this.App.UI.Path(new string[]
							{
								base.ID,
								"systemDetailsWidget"
							}), this.TargetSystem, false, true);
							if (this._systemWidget != null)
							{
								this._systemWidget.Sync(this.TargetSystem);
							}
							this.App.UI.SetVisible(this.App.UI.Path(new string[]
							{
								base.ID,
								"admiralDetails",
								"traitsLabel"
							}), false);
							this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
							{
								base.ID,
								"admiralImage"
							}), "sprite", this.GetDefaultFactionAdmiralSprite(this._app.LocalPlayer.Faction));
							this.OnEnter();
							this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
							this._shown = true;
						}
					}
				}
			}
		}
		private string GetDefaultFactionAdmiralSprite(Faction faction)
		{
			string arg = string.Empty;
			string name;
			if ((name = faction.Name) != null)
			{
				if (name == "zuul")
				{
					arg = "hordezuul";
					goto IL_43;
				}
				if (name == "liir_zuul")
				{
					arg = "liir";
					goto IL_43;
				}
			}
			arg = faction.Name;
			IL_43:
			return string.Format("admiral_{0}", arg);
		}
		private void CommitMission()
		{
			MissionInfo missionByFleetID = this.App.GameDatabase.GetMissionByFleetID(this._selectedFleet);
			if (missionByFleetID != null)
			{
				IEnumerable<WaypointInfo> waypointsByMissionID = this.App.GameDatabase.GetWaypointsByMissionID(missionByFleetID.ID);
				foreach (WaypointInfo current in waypointsByMissionID)
				{
					this.App.GameDatabase.RemoveWaypoint(current.ID);
				}
				this.App.GameDatabase.RemoveMission(missionByFleetID.ID);
			}
			this.OnCommitMission();
			this.RefreshUI(this.TargetSystem);
			this.Hide();
		}
		public override string[] CloseDialog()
		{
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._fleetWidget.UnlinkWidgets();
			this._fleetWidget.Dispose();
			this._escorts.Clear();
			this._vitals.Clear();
			this._systemWidget.Terminate();
			return null;
		}
		protected override void OnUpdate()
		{
			if (this._shown)
			{
				this.SelectedFleet = this._fleetWidget.SelectedFleet;
				this._systemWidget.Update();
			}
		}
		public void OnStarmapSelectedObjectChanged(App game, int objectid)
		{
			if (this._shown && this.TargetSystem != objectid && objectid != 0 && this.MissionType != MissionType.REACTION)
			{
				this.RefreshUI(objectid);
			}
		}
		public void RefreshUI(int targetsystem)
		{
			this.TargetSystem = targetsystem;
			if (this.ReBaseToggle)
			{
				return;
			}
			if (this._fleetCentric)
			{
				this._fleetWidget.SetSyncedFleets(this.SelectedFleet);
				this.UIListSelectSystem(this.TargetSystem);
			}
			else
			{
                List<FleetInfo> list = Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, this.TargetSystem, this._missionType, true).ToList<FleetInfo>();
				List<int> list2 = (
					from x in list
					select x.ID).ToList<int>();
				if (list2.Except(this._fleetWidget.SyncedFleets).Count<int>() != 0 || this._fleetWidget.SyncedFleets.Except(list2).Count<int>() != 0)
				{
					this._fleetWidget.SetSyncedFleets(list);
				}
			}
			if (!this._fleetCentric)
			{
				this.SelectedFleet = 0;
			}
			this.SelectedPlanet = 0;
			this._fleetWidget.SelectedFleet = this.SelectedFleet;
			if (this._systemWidget != null)
			{
				this._systemWidget.Sync(this.TargetSystem);
			}
			this.RefreshMissionDetails(StationType.INVALID_TYPE, 1);
			this.UpdateCanConfirmMission();
		}
		public void UpdateCanRebaseToTarget()
		{
			if (this.MissionType == MissionType.GATE || this.MissionType == MissionType.INTERCEPT || this.MissionType == MissionType.INTERDICTION || this.MissionType == MissionType.INVASION || this.MissionType == MissionType.RELOCATION || this.MissionType == MissionType.STRIKE || this.MissionType == MissionType.PIRACY)
			{
				this.ReBaseToggle = false;
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gameRebaseToggle"
				}), false);
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gameRebaseToggleStn"
				}), false);
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"gameEnterRebase"
				}), false);
				return;
			}
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"gameRebaseToggle"
			}), true);
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"gameRebaseToggleStn"
			}), true);
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"gameEnterRebase"
			}), true);
			bool flag = false;
			if (this.SelectedFleet != 0)
			{
				flag = this.CompileListOfRelocatableSystemsForFleet(this.SelectedFleet, false).Any<int>();
			}
			this._app.UI.SetEnabled(base.UI.Path(new string[]
			{
				base.ID,
				"gameEnterRebase"
			}), flag);
			if (!flag)
			{
				this._rebaseToggle = false;
			}
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
		}
		protected bool LoaTryCommitMission()
		{
			if (this.App.Game.LocalPlayer.Faction == this.App.AssetDatabase.GetFaction("loa"))
			{
				FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._selectedFleet);
                int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this.App.Game, this._selectedFleet);
                if (fleetInfo.SystemID != this._TargetSystem && fleetLoaCubeValue > Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this.App.Game, this.App.LocalPlayer.ID))
				{
                    this._loaShearFleetConfirm = this.App.UI.CreateDialog(new GenericQuestionDialog(this.App, App.Localize("@UI_LOA_CUBE_GATE_VIOLATION"), string.Format(App.Localize("@UI_LOA_CUBE_GATE_VIOLATION_DESC"), fleetLoaCubeValue.ToString("N0"), Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this.App.Game, this.App.LocalPlayer.ID).ToString("N0")), "dialogShearFleetQuestion"), null);
				}
				else
				{
					this._loaFleetCompoSel = this.App.UI.CreateDialog(new DialogLoaFleetSelector(this.App, this._missionType, this._app.GameDatabase.GetFleetInfo(this._selectedFleet), false), null);
				}
				return false;
			}
			this.CommitMission();
			return true;
		}
		protected bool TryCommitMission()
		{
			this._additionalShips.Clear();
			if (this.CanConfirmMission() && this.MissionType != MissionType.REACTION)
			{
				if (this.App.Game.LocalPlayer.Faction != this.App.AssetDatabase.GetFaction("loa"))
				{
                    if ((this.App.Game.LocalPlayer.Faction == this.App.AssetDatabase.GetFaction("zuul") || this.App.Game.LocalPlayer.Faction == this.App.AssetDatabase.GetFaction("loa")) && this._selectedFleet != 0 && !Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, this.App.GameDatabase.GetFleetInfo(this._selectedFleet)))
					{
						if (this.App.GameDatabase.GetMissionByFleetID(this._selectedFleet) == null && this.CheckZuulHasBore())
						{
							this.App.UI.UnlockUI();
							this._zuulBoreMissing = this.App.UI.CreateDialog(new GenericYesNoDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), App.Localize("@UI_CONFIRM_BORE_LINE_MISSION_NOBORE"), "dialogNoBoreQuestion"), null);
							return false;
						}
						if (this.CheckZuulDirectRoute() && GameSession.FleetHasBore(this.App.GameDatabase, this._selectedFleet))
						{
							this._zuulBoreRoute = this.App.UI.CreateDialog(new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), App.Localize("@UI_CONFIRM_BORE_LINE_MISSION"), "dialogBoreRouteQuestion"), null);
							return false;
						}
					}
					this.CommitMission();
					return true;
				}
				if (this.CheckZuulDirectRoute() && this._missionType != MissionType.DEPLOY_NPG)
				{
					this._loaDirectRoute = this.App.UI.CreateDialog(new GenericQuestionDialog(this.App, App.Localize("@UI_MISSION_CONFIRMATION"), App.Localize("@UI_CONFIRM_DIRECT_PATH"), "dialogDirectRouteQuestion"), null);
					return false;
				}
				return this.LoaTryCommitMission();
			}
			else
			{
				if (this.MissionType == MissionType.REACTION)
				{
					this.OnCommitMission();
					this.Hide();
					return true;
				}
				return false;
			}
		}
		protected void UpdateCanConfirmMission()
		{
			bool newValue = this.CanConfirmMission() && !this.ReBaseToggle;
			this.CanConfirmMissionChanged(newValue);
			this.UpdateCanRebaseToTarget();
		}
		private void CanConfirmMissionChanged(bool newValue)
		{
			this._canConfirm = newValue;
			this.App.UI.SetEnabled("gameConfirmMissionButton", newValue);
			this.App.UI.SetEnabled("gameConfirmAndContinueMissionButton", newValue);
			this.OnCanConfirmMissionChanged(newValue);
		}
		private FleetInfo GetBestFleet(MissionType missionType, int systemId, IEnumerable<FleetInfo> fleets)
		{
			return fleets.FirstOrDefault<FleetInfo>();
		}
		private IEnumerable<FleetInfo> CollectAvailableFleets(MissionType missionType, int systemId)
		{
            return Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, systemId, missionType, true);
		}
	}
}
