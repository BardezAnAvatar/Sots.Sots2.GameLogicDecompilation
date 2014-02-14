using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class OverlayRelocateMission : OverlayMission
	{
		private FleetWidget _RelocatefleetWidget;
		private bool CaravanMode;
		private int? CaravanFleet;
		private int? SelectedCaravanSourceSystem;
		public OverlayRelocateMission(App game, StarMapState state, StarMap starmap, string template = "OverlayRelocateMission") : base(game, state, starmap, MissionType.RELOCATION, template)
		{
		}
		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}
		protected override bool CanConfirmMission()
		{
			if (this.CaravanMode && this.CaravanFleet.HasValue)
			{
				return this.App.GameDatabase.GetShipInfoByFleetID(this.CaravanFleet.Value, false).Any<ShipInfo>();
			}
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(base.SelectedFleet);
			if (fleetInfo == null)
			{
				return false;
			}
            if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(this.App.GameDatabase, fleetInfo))
			{
				return true;
			}
            if (Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, fleetInfo))
			{
				return true;
			}
			if (fleetInfo.AdmiralID == 0)
			{
				return false;
			}
			bool flag = fleetInfo.SupportingSystemID == base.TargetSystem;
            return base.IsValidFleetID(base.SelectedFleet) && Kerberos.Sots.StarFleet.StarFleet.CanSystemSupportFleet(this.App.Game, base.TargetSystem, base.SelectedFleet) && !flag;
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			StarSystemUI.SyncSystemDetailsWidget(this.App, "systemDetailsWidget", base.TargetSystem, false, true);
			if (base.MissionType == MissionType.RELOCATION)
			{
				this._RelocatefleetWidget = new FleetWidget(this.App, this.App.UI.Path(new string[]
				{
					base.ID,
					"gameRelocateFleet"
				}));
			}
			if (base.MissionType == MissionType.RELOCATION)
			{
				this._RelocatefleetWidget.MissionMode = MissionType.NO_MISSION;
				this._fleetWidget.LinkWidget(this._RelocatefleetWidget);
				this._RelocatefleetWidget.LinkWidget(this._fleetWidget);
				FleetWidget expr_9E = this._RelocatefleetWidget;
				expr_9E.OnFleetsModified = (FleetWidget.FleetsModifiedDelegate)Delegate.Combine(expr_9E.OnFleetsModified, new FleetWidget.FleetsModifiedDelegate(this.FleetsModified));
			}
			this.CaravanMode = false;
			this.CaravanFleet = null;
			this.SelectedCaravanSourceSystem = null;
			if (this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).FactionID).Name == "loa" || this._fleetCentric)
			{
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"RelocateAssetsBTN"
				}), false);
				this._app.UI.SetEnabled(base.UI.Path(new string[]
				{
					base.ID,
					"RelocateAssetsBTN"
				}), false);
			}
			else
			{
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"RelocateAssetsBTN"
				}), true);
				if (this.CanDoCaravan())
				{
					this._app.UI.SetEnabled(base.UI.Path(new string[]
					{
						base.ID,
						"RelocateAssetsBTN"
					}), true);
				}
				else
				{
					this._app.UI.SetEnabled(base.UI.Path(new string[]
					{
						base.ID,
						"RelocateAssetsBTN"
					}), false);
				}
			}
			this._app.UI.SetVisible(base.UI.Path(new string[]
			{
				base.ID,
				"caravanFade"
			}), false);
		}
		private void FleetsModified(App app, int[] modifiedFleetIds)
		{
			new int?(modifiedFleetIds[0]);
			int? num = new int?(modifiedFleetIds[1]);
			FleetInfo fleetInfo = null;
			if (num.HasValue)
			{
				fleetInfo = this._app.GameDatabase.GetFleetInfo(num.Value);
			}
			if (fleetInfo != null && fleetInfo.Type == FleetType.FL_CARAVAN)
			{
				return;
			}
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(fleetInfo.SystemID);
			if (starSystemInfo != null && this.SelectedCaravanSourceSystem != starSystemInfo.ID)
			{
				this.SelectedCaravanSourceSystem = new int?(starSystemInfo.ID);
				if (this.SelectedCaravanSourceSystem.HasValue)
				{
					List<int> fleets = (
						from x in this._app.GameDatabase.GetFleetsByPlayerAndSystem(this._app.LocalPlayer.ID, this.SelectedCaravanSourceSystem.Value, FleetType.FL_RESERVE)
						select x.ID).ToList<int>();
					Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
					foreach (int current in 
						from x in this._fleetWidget.SyncedFleets
						where !fleets.Contains(x)
						select x)
					{
						dictionary.Add(current, false);
					}
					this._fleetWidget.SetVisibleFleets(dictionary);
					FleetInfo fleetInfo2 = this._app.GameDatabase.GetFleetInfo(this.CaravanFleet.Value);
					if (fleetInfo2 != null)
					{
						fleetInfo2.SystemID = this.SelectedCaravanSourceSystem.Value;
						fleetInfo2.SupportingSystemID = this.SelectedCaravanSourceSystem.Value;
						this._app.GameDatabase.UpdateFleetInfo(fleetInfo2);
						this._app.GameDatabase.UpdateFleetLocation(fleetInfo2.ID, this.SelectedCaravanSourceSystem.Value, null);
					}
				}
				else
				{
					this._fleetWidget.SetSyncedFleets(0);
				}
			}
			base.UpdateCanConfirmMission();
		}
		protected override void OnCommitMission()
		{
			if (this.CaravanMode && this.CaravanFleet.HasValue)
			{
				base.SelectedFleet = this.CaravanFleet.Value;
			}
			if (this._app.LocalPlayer.Faction.Name == "loa")
			{
                Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, this._selectedFleet);
                Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, this._selectedFleet, MissionType.NO_MISSION);
				base.RebuildShipLists(base.SelectedFleet);
			}
            Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this.App.Game, base.SelectedFleet, base.TargetSystem, this._useDirectRoute, base.GetDesignsToBuild());
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._selectedFleet);
			AdmiralInfo admiralInfo = this.App.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
			if (admiralInfo != null)
			{
				string cueName = string.Format("STRAT_011-01_{0}_{1}TransferMissionConfirmation", this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID)), admiralInfo.GetAdmiralSoundCueContext(this.App.AssetDatabase));
				this.App.PostRequestSpeech(cueName, 50, 120, 0f);
			}
			if (this.CaravanMode && this.CaravanFleet.HasValue)
			{
				List<FreighterInfo> source = (
					from x in this.App.GameDatabase.GetFreighterInfosForSystem(fleetInfo.SystemID)
					where x.PlayerId == this.App.LocalPlayer.ID && x.IsPlayerBuilt
					select x).ToList<FreighterInfo>();
				List<ShipInfo> list = this.App.GameDatabase.GetShipInfoByFleetID(this.CaravanFleet.Value, false).ToList<ShipInfo>();
				foreach (ShipInfo ship in list)
				{
					if (source.Any((FreighterInfo x) => x.ShipId == ship.ID))
					{
						this.App.GameDatabase.RemoveFreighterInfo(source.First((FreighterInfo x) => x.ShipId == ship.ID).ID);
					}
				}
			}
			this.App.GetGameState<StarMapState>().RefreshMission();
			this.CaravanFleet = null;
			this.SelectedCaravanSourceSystem = null;
			if (this._fleetWidget.SyncedFleets != null)
			{
				this._fleetWidget.SetSyncedFleets(this._fleetWidget.SyncedFleets);
			}
			if (this._RelocatefleetWidget.SyncedFleets != null)
			{
				this._RelocatefleetWidget.SetSyncedFleets(this._RelocatefleetWidget.SyncedFleets);
			}
			this.EnterAssetRelocateMode(false);
		}
		protected override string GetMissionDetailsTitle()
		{
			string name = this.App.GameDatabase.GetStarSystemInfo(base.TargetSystem).Name;
			return string.Format(App.Localize("@UI_RELOCATE_OVERLAY_MISSION_TITLE"), name.ToUpperInvariant());
		}
		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
			string hint = string.Empty;
			if (!base.CanConfirm)
			{
				hint = App.Localize("@UI_RELOCATION_INSUFFICIENT_SUPPORT");
			}
			else
			{
				hint = App.Localize("@RELOCATIONMISSION_HINT");
			}
			base.AddMissionTime(2, App.Localize("@MISSIONWIDGET_RELOCATION_TIME"), estimate.TurnsToTarget, hint);
		}
		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "RelocateAssetsBTN")
				{
					this.EnterAssetRelocateMode(!this.CaravanMode);
				}
				base.UpdateCanConfirmMission();
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}
		private bool CanDoCaravan()
		{
            if (Kerberos.Sots.StarFleet.StarFleet.HasRelocatableDefResAssetsInRange(this._app.Game, base.TargetSystem))
			{
				return true;
			}
			if (this.App.GameDatabase.GetFreighterInfosBuiltByPlayer(this.App.LocalPlayer.ID).Any((FreighterInfo x) => x.SystemId != base.TargetSystem))
			{
				return this.App.GameDatabase.GetStationForSystem(base.TargetSystem).Any((StationInfo x) => x.DesignInfo.StationType == StationType.CIVILIAN);
			}
			return false;
		}
		private void EnterAssetRelocateMode(bool value)
		{
			this.CaravanMode = value;
			if (this.CaravanMode)
			{
				this._fleetWidget.DisableTooltips = true;
				List<StarSystemInfo> list = this._app.GameDatabase.GetVisibleStarSystemInfos(this._app.LocalPlayer.ID).ToList<StarSystemInfo>();
				if (list.Any<StarSystemInfo>())
				{
					int admiralID = GameSession.GenerateNewAdmiral(this._app.AssetDatabase, this._app.LocalPlayer.ID, this._app.GameDatabase, null, this._app.Game.NamesPool);
					int systemID = this._app.GameDatabase.GetHomeworlds().FirstOrDefault((HomeworldInfo x) => x.PlayerID == this._app.LocalPlayer.ID).SystemID;
					this.CaravanFleet = new int?(this._app.GameDatabase.InsertFleet(this._app.LocalPlayer.ID, admiralID, systemID, systemID, App.Localize("@FLEET_CARAVAN_NAME"), FleetType.FL_CARAVAN));
					this._RelocatefleetWidget.SetSyncedFleets(this.CaravanFleet.Value);
					List<int> list2 = new List<int>();
					foreach (StarSystemInfo sysinf in list)
					{
						if (sysinf.ID != base.TargetSystem)
						{
							int? reserveFleetID = this._app.GameDatabase.GetReserveFleetID(this._app.LocalPlayer.ID, sysinf.ID);
							if (reserveFleetID.HasValue)
							{
								if (this.App.GameDatabase.GetStationForSystem(base.TargetSystem).Any((StationInfo x) => x.DesignInfo.StationType == StationType.CIVILIAN))
								{
									List<FreighterInfo> list3 = (
										from x in this._app.GameDatabase.GetFreighterInfosBuiltByPlayer(this._app.LocalPlayer.ID)
										where x.SystemId == sysinf.ID && x.IsPlayerBuilt
										select x).ToList<FreighterInfo>();
									foreach (FreighterInfo current in list3)
									{
										this._app.GameDatabase.TransferShip(current.ShipId, reserveFleetID.Value);
									}
								}
							}
							int? defenseFleetID = this._app.GameDatabase.GetDefenseFleetID(sysinf.ID, this._app.LocalPlayer.ID);
							if (reserveFleetID.HasValue && this._app.GameDatabase.GetShipsByFleetID(reserveFleetID.Value).Any<int>())
							{
								list2.Add(reserveFleetID.Value);
							}
							if (defenseFleetID.HasValue && this._app.GameDatabase.GetShipsByFleetID(defenseFleetID.Value).Any<int>())
							{
								list2.Add(defenseFleetID.Value);
							}
						}
					}
					this._fleetWidget.MissionMode = MissionType.NO_MISSION;
					this._fleetWidget.SetSyncedFleets(list2);
					this._app.UI.SetVisible(base.UI.Path(new string[]
					{
						base.ID,
						"caravanFade"
					}), true);
					return;
				}
			}
			else
			{
				this._fleetWidget.DisableTooltips = false;
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					base.ID,
					"caravanFade"
				}), false);
				this.clearCaravanFleet();
				this._fleetWidget.MissionMode = MissionType.RELOCATION;
				base.RefreshUI(base.TargetSystem);
			}
		}
		private void clearCaravanFleet()
		{
			if (this.CaravanFleet.HasValue)
			{
				FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this.CaravanFleet.Value);
				List<ShipInfo> list = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
				List<FreighterInfo> list2 = (
					from x in this._app.GameDatabase.GetFreighterInfosBuiltByPlayer(this._app.LocalPlayer.ID)
					where x.IsPlayerBuilt
					select x).ToList<FreighterInfo>();
				foreach (ShipInfo ship in list)
				{
					if (list2.Any((FreighterInfo x) => x.ShipId == ship.ID))
					{
						this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetLimboFleetID(fleetInfo.SystemID, this._app.LocalPlayer.ID));
					}
					else
					{
						if (ship.IsSDB() || ship.IsPlatform())
						{
							this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetDefenseFleetInfo(fleetInfo.SystemID, this._app.LocalPlayer.ID).ID);
						}
						else
						{
							this._app.GameDatabase.TransferShip(ship.ID, this._app.GameDatabase.InsertOrGetReserveFleetInfo(fleetInfo.SystemID, this._app.LocalPlayer.ID).ID);
						}
					}
				}
				foreach (FreighterInfo current in list2)
				{
					ShipInfo shipInfo = this._app.GameDatabase.GetShipInfo(current.ShipId, false);
					FleetInfo fleetInfo2 = this._app.GameDatabase.GetFleetInfo(shipInfo.FleetID);
					if (fleetInfo2 != null && fleetInfo2.IsReserveFleet && !fleetInfo2.IsLimboFleet)
					{
						this._app.GameDatabase.TransferShip(shipInfo.ID, this._app.GameDatabase.InsertOrGetLimboFleetID(current.SystemId, current.PlayerId));
					}
				}
				this._app.GameDatabase.RemoveAdmiral(fleetInfo.AdmiralID);
				this._app.GameDatabase.RemoveFleet(fleetInfo.ID);
				fleetInfo = null;
				this._RelocatefleetWidget.SetSyncedFleets(0);
				this.CaravanFleet = null;
			}
			else
			{
				List<FreighterInfo> list3 = (
					from x in this._app.GameDatabase.GetFreighterInfosBuiltByPlayer(this._app.LocalPlayer.ID)
					where x.IsPlayerBuilt
					select x).ToList<FreighterInfo>();
				foreach (FreighterInfo current2 in list3)
				{
					ShipInfo shipInfo2 = this._app.GameDatabase.GetShipInfo(current2.ShipId, false);
					FleetInfo fleetInfo3 = this._app.GameDatabase.GetFleetInfo(shipInfo2.FleetID);
					if (fleetInfo3 != null && fleetInfo3.IsReserveFleet && !fleetInfo3.IsLimboFleet)
					{
						this._app.GameDatabase.TransferShip(shipInfo2.ID, this._app.GameDatabase.InsertOrGetLimboFleetID(current2.SystemId, current2.PlayerId));
					}
				}
			}
			this.SelectedCaravanSourceSystem = null;
		}
		protected override void OnExit()
		{
			this.clearCaravanFleet();
			this.CaravanMode = false;
			base.OnExit();
		}
		public override string[] CloseDialog()
		{
			if (this._RelocatefleetWidget != null)
			{
				this._fleetWidget.UnlinkWidgets();
				this._RelocatefleetWidget.UnlinkWidgets();
				this._RelocatefleetWidget.SetSyncedFleets(0);
				this._RelocatefleetWidget.SelectedFleet = 0;
				FleetWidget expr_3C = this._RelocatefleetWidget;
				expr_3C.OnFleetsModified = (FleetWidget.FleetsModifiedDelegate)Delegate.Remove(expr_3C.OnFleetsModified, new FleetWidget.FleetsModifiedDelegate(this.FleetsModified));
				this._RelocatefleetWidget.Dispose();
			}
			this.App.UI.PurgeFleetWidgetCache();
			this._RelocatefleetWidget = null;
			return base.CloseDialog();
		}
	}
}
