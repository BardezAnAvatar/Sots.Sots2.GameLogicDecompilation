using Kerberos.Sots.Data;
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
	internal class OverlayReactionlMission : OverlayMission
	{
		private class ReactionUIContainer
		{
			public string buttonID;
			public string ListItemID;
			public ReactionInfo Reaction;
			public int? TargetFleet;
		}
		private List<OverlayReactionlMission.ReactionUIContainer> _containers;
		private OverlayReactionlMission.ReactionUIContainer _selectedReaction;
		private List<ReactionInfo> _reactions;
		protected FleetWidget _reactionfleet;
		private static string UI_REACTIONLIST_PANEL = "ReactionList_Panel";
		private static string UI_CURRENTFLEETLIST = "gameReactionCurrentFleet";
		public OverlayReactionlMission(App game, StarMapState state, StarMap starmap, string template = "OverlayReaction") : base(game, state, starmap, MissionType.REACTION, template)
		{
			this._containers = new List<OverlayReactionlMission.ReactionUIContainer>();
		}
		protected override void OnCanConfirmMissionChanged(bool newValue)
		{
		}
		protected override bool CanConfirmMission()
		{
			return true;
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			this._reactionfleet = new FleetWidget(this.App, this.App.UI.Path(new string[]
			{
				base.ID,
				OverlayReactionlMission.UI_CURRENTFLEETLIST
			}));
			this._reactionfleet.ScrapEnabled = false;
			this._reactionfleet.SetEnabled(false);
			this.PathDrawEnabled = false;
			this._containers.Clear();
			this._reactions = (
				from x in this._app.Game.GetPendingReactions()
				where x.fleet.PlayerID == this._app.LocalPlayer.ID
				select x).ToList<ReactionInfo>();
			foreach (ReactionInfo current in this._reactions)
			{
				this._app.UI.AddItem(OverlayReactionlMission.UI_REACTIONLIST_PANEL, "", current.fleet.ID, "");
				string itemGlobalID = this._app.UI.GetItemGlobalID(OverlayReactionlMission.UI_REACTIONLIST_PANEL, "", current.fleet.ID, "");
				this._app.UI.SetText(this.App.UI.Path(new string[]
				{
					itemGlobalID,
					"fleetname"
				}), current.fleet.Name);
				string text = "ReactionButton|" + current.fleet.ID.ToString();
				this._app.UI.SetPropertyString(base.UI.Path(new string[]
				{
					itemGlobalID,
					"reaction_button"
				}), "id", text);
				OverlayReactionlMission.ReactionUIContainer reactionUIContainer = new OverlayReactionlMission.ReactionUIContainer();
				reactionUIContainer.Reaction = current;
				reactionUIContainer.ListItemID = itemGlobalID;
				reactionUIContainer.buttonID = text;
				reactionUIContainer.TargetFleet = null;
				this._containers.Add(reactionUIContainer);
			}
			this._fleetWidget.EnemySelectionEnabled = true;
			FleetWidget expr_205 = this._fleetWidget;
			expr_205.OnFleetSelectionChanged = (FleetWidget.FleetSelectionChangedDelegate)Delegate.Combine(expr_205.OnFleetSelectionChanged, new FleetWidget.FleetSelectionChangedDelegate(this.OnFleetSelectionChanged));
			this._fleetWidget.MissionMode = MissionType.NO_MISSION;
			this._starMap.FocusEnabled = true;
			this._starMap.SelectEnabled = true;
			foreach (int current2 in this._starMap.Systems.Reverse.Keys)
			{
				bool flag = false;
				foreach (OverlayReactionlMission.ReactionUIContainer current3 in this._containers)
				{
					foreach (FleetInfo current4 in current3.Reaction.fleetsInRange)
					{
						if (current4.SystemID == current2)
						{
							flag = true;
							break;
						}
					}
					if (current3.Reaction.fleet.SystemID == current2)
					{
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				this._starMap.Systems.Reverse[current2].SetIsEnabled(flag);
				this._starMap.Systems.Reverse[current2].SetIsSelectable(flag);
			}
			this.SelectReaction(this._containers.First<OverlayReactionlMission.ReactionUIContainer>());
		}
		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (base.GetShown() && this._selectedReaction != null && this._fleetWidget.SelectedFleet == 0 && this._selectedReaction.TargetFleet.HasValue)
			{
				this.OnFleetSelectionChanged(this._app, 0);
			}
		}
		protected override void RefreshMissionDetails(StationType type = StationType.INVALID_TYPE, int stationLevel = 1)
		{
			if (base.TargetSystem == 0)
			{
				return;
			}
			string text = this.GetMissionDetailsTitle();
			if (this._selectedReaction != null && this._selectedReaction.Reaction != null && this._selectedReaction.TargetFleet.HasValue)
			{
				int systemID = this._selectedReaction.Reaction.fleetsInRange.FirstOrDefault((FleetInfo x) => x.ID == this._selectedReaction.TargetFleet.Value).SystemID;
				this.App.UI.ClearItems("gameMissionTimes");
				this.App.UI.ClearItems("gameMissionNotes");
                this._missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(this.App.Game, MissionType.REACTION, type, this._selectedReaction.Reaction.fleet.ID, systemID, base.SelectedPlanet, base.GetDesignsToBuild(), stationLevel, false, null, null);
				text += string.Format(App.Localize("@UI_MISSION_ETA_TURNS"), this._missionEstimate.TurnsToTarget);
				this.OnRefreshMissionDetails(this._missionEstimate);
				this.App.UI.AutoSizeContents("gameMissionDetails");
			}
			this.App.UI.SetText(this.App.UI.Path(new string[]
			{
				base.ID,
				"gameMissionTitle"
			}), text);
		}
		public void OnFleetSelectionChanged(App game, int selectedFleet)
		{
			if ((selectedFleet == 0 && this._selectedReaction != null) || selectedFleet == this._selectedReaction.Reaction.fleet.ID)
			{
				base.SelectedFleet = this._selectedReaction.Reaction.fleet.ID;
				this._selectedReaction.TargetFleet = null;
				base.FocusOnStarSystem(this._selectedReaction.Reaction.fleet.SystemID);
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					this._selectedReaction.ListItemID,
					"reaction_anim"
				}), false);
				return;
			}
			if (this._selectedReaction != null)
			{
				this._selectedReaction.TargetFleet = new int?(selectedFleet);
				base.SelectedFleet = this._selectedReaction.TargetFleet.Value;
				base.FocusOnStarSystem(this._selectedReaction.Reaction.fleetsInRange.FirstOrDefault((FleetInfo x) => x.ID == selectedFleet).SystemID);
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					this._selectedReaction.ListItemID,
					"reaction_anim"
				}), true);
			}
		}
		private void SelectReaction(OverlayReactionlMission.ReactionUIContainer reaction)
		{
			if (this._selectedReaction != null)
			{
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					this._selectedReaction.ListItemID,
					"reaction_selection"
				}), false);
			}
			this._selectedReaction = reaction;
			if (this._selectedReaction != null)
			{
				base.SelectedFleet = this._selectedReaction.Reaction.fleet.ID;
				this._app.UI.SetVisible(base.UI.Path(new string[]
				{
					this._selectedReaction.ListItemID,
					"reaction_selection"
				}), true);
				this._reactionfleet.SetSyncedFleets(this._selectedReaction.Reaction.fleet.ID);
				OverlayMission.RefreshFleetAdmiralDetails(this.App, base.ID, this._selectedReaction.Reaction.fleet.ID, "admiralDetails1");
				base.SelectedFleet = 0;
				base.SelectedPlanet = 0;
				this._fleetWidget.Selected = -1;
				this._fleetWidget.SelectedFleet = 0;
				this._fleetWidget.SetSyncedFleets(this._selectedReaction.Reaction.fleetsInRange);
				if (this._selectedReaction.TargetFleet.HasValue)
				{
					this._fleetWidget.Selected = this._selectedReaction.TargetFleet.Value;
					this._fleetWidget.SelectedFleet = this._selectedReaction.TargetFleet.Value;
				}
				else
				{
					base.SelectedFleet = 0;
					base.SelectedPlanet = 0;
					this._fleetWidget.Selected = -1;
					this._fleetWidget.SelectedFleet = 0;
				}
				base.FocusOnStarSystem(this._selectedReaction.Reaction.fleet.SystemID);
				this._systemWidget.Sync(this._selectedReaction.Reaction.fleet.SystemID);
			}
		}
		protected override void OnExit()
		{
			if (this._reactionfleet != null)
			{
				this._reactionfleet.Dispose();
			}
			base.OnExit();
		}
		protected override void OnCommitMission()
		{
			foreach (OverlayReactionlMission.ReactionUIContainer rui in this._containers)
			{
				if (rui.TargetFleet.HasValue)
				{
					FleetInfo fleetInfo = rui.Reaction.fleetsInRange.FirstOrDefault((FleetInfo x) => x.ID == rui.TargetFleet);
					int systemID = rui.Reaction.fleet.SystemID;
					this._app.GameDatabase.ChangeDiplomacyState(rui.Reaction.fleet.PlayerID, fleetInfo.PlayerID, DiplomacyState.WAR);
					this._app.GameDatabase.UpdateFleetLocation(rui.Reaction.fleet.ID, fleetInfo.SystemID, null);
					MissionInfo missionByFleetID = this._app.GameDatabase.GetMissionByFleetID(rui.Reaction.fleet.ID);
					if (missionByFleetID == null)
					{
                        Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._app.Game, rui.Reaction.fleet.ID, systemID, false, null);
					}
					else
					{
						List<WaypointInfo> list = this._app.GameDatabase.GetWaypointsByMissionID(missionByFleetID.ID).ToList<WaypointInfo>();
						foreach (WaypointInfo current in list)
						{
							this._app.GameDatabase.RemoveWaypoint(current.ID);
						}
						this._app.GameDatabase.InsertWaypoint(missionByFleetID.ID, WaypointType.TravelTo, new int?(systemID));
						foreach (WaypointInfo current2 in list)
						{
							this._app.GameDatabase.InsertWaypoint(missionByFleetID.ID, current2.Type, current2.SystemID);
						}
					}
				}
				this._app.Game.RemoveReaction(rui.Reaction);
			}
			if (!this._app.GameSetup.IsMultiplayer)
			{
				this._app.Game.Phase4_Combat();
				return;
			}
			if (this._app.Network.IsJoined)
			{
				this._app.GameDatabase.LogComment("SYNC REACTIONS");
				this._app.Network.SendHistory(this._app.GameDatabase.GetTurnCount());
				return;
			}
			if (this._app.Network.IsHosting)
			{
				this._app.Network.ReactionComplete();
			}
		}
		protected override string GetMissionDetailsTitle()
		{
			return string.Format(App.Localize("@UI_REACTIONS_PENDING"), new object[0]);
		}
		protected override void OnRefreshMissionDetails(MissionEstimate estimate)
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (this._containers.Any((OverlayReactionlMission.ReactionUIContainer x) => x.buttonID == panelName))
				{
					this.SelectReaction(this._containers.FirstOrDefault((OverlayReactionlMission.ReactionUIContainer x) => x.buttonID == panelName));
				}
				else
				{
					if (panelName == "selectionClear")
					{
						base.SelectedFleet = 0;
						base.SelectedPlanet = 0;
						this._fleetWidget.Selected = -1;
						this._fleetWidget.SelectedFleet = 0;
					}
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}
		protected override IEnumerable<int> GetMissionTargetPlanets()
		{
			return Enumerable.Empty<int>();
		}
	}
}
