using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class ReactionDialog : Dialog
	{
		public static readonly string UITemplate = "ReactionPopup";
		public static readonly string UIConfirmButton = "buttonIntercept";
		public static readonly string UICancelButton = "buttonCancel";
		public static readonly string UIAdmiralLabel = "lblAdmiral";
		public static readonly string UIFleetWidget = "gameFleetList";
		public static readonly string UIObjectHost = "reactionStarMapViewport";
		public static readonly string UILeftPanel = "pnlLeft";
		public static readonly string UIRightPanel = "pnlRight";
		private ReactionInfo _currentReaction;
		private FleetWidget _interceptFleetWidget;
		private string _hostileActionDialog;
		private OrbitCameraController _cameraReduced;
		private StarMap _starmapReduced;
		private Sky _sky;
		private GameObjectSet _crits;
		public ReactionDialog(App game, ReactionInfo reaction) : base(game, "ReactionPopup")
		{
			this._currentReaction = reaction;
			this._interceptFleetWidget = new FleetWidget(game, game.UI.Path(new string[]
			{
				base.ID,
				ReactionDialog.UILeftPanel,
				ReactionDialog.UIFleetWidget
			}));
			this._interceptFleetWidget.EnemySelectionEnabled = true;
			this._interceptFleetWidget.SetSyncedFleets(reaction.fleetsInRange);
		}
		public override void Initialize()
		{
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				ReactionDialog.UIConfirmButton
			}), false);
			AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(this._currentReaction.fleet.AdmiralID);
			if (admiralInfo != null)
			{
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					ReactionDialog.UIAdmiralLabel
				}), admiralInfo.Name);
			}
			this._sky = new Sky(this._app, SkyUsage.StarMap, 0);
			this._crits = new GameObjectSet(this._app);
			this._crits.Add(this._sky);
			this._cameraReduced = new OrbitCameraController(this._app);
			this._cameraReduced.MinDistance = 2.5f;
			this._cameraReduced.MaxDistance = 100f;
			this._cameraReduced.DesiredDistance = 50f;
			this._cameraReduced.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this._cameraReduced.DesiredPitch = -MathHelper.DegreesToRadians(25f);
			this._cameraReduced.SnapToDesiredPosition();
			this._starmapReduced = new StarMap(this._app, this._app.Game, this._sky);
			this._starmapReduced.Initialize(this._crits, new object[0]);
			this._starmapReduced.SetCamera(this._cameraReduced);
			this._starmapReduced.FocusEnabled = false;
			this._starmapReduced.SetFocus(this._starmapReduced.Systems.Reverse[this._currentReaction.fleet.SystemID]);
			this._starmapReduced.Select(this._starmapReduced.Systems.Reverse[this._currentReaction.fleet.SystemID]);
			this._starmapReduced.SelectEnabled = false;
			this._starmapReduced.PostSetProp("MissionTarget", new object[]
			{
				this._starmapReduced.Systems.Reverse[this._currentReaction.fleet.SystemID].ObjectID,
				true
			});
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._currentReaction.fleet.SystemID);
			this._starmapReduced.PostSetProp("CullCenter", starSystemInfo.Origin);
			this._starmapReduced.PostSetProp("CullRadius", 15f);
			string text = this._app.UI.Path(new string[]
			{
				base.ID,
				"gameStarMapViewport"
			});
			this._app.UI.Send(new object[]
			{
				"SetGameObject",
				text,
				this._starmapReduced.ObjectID
			});
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"gameStarMapViewport"
			}), true);
			string cueName = string.Format("STRAT_014-01_{0}_{1}AdmiralGetsReactionPhase", this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)), admiralInfo.GetAdmiralSoundCueContext(this._app.AssetDatabase));
			this._app.PostRequestSpeech(cueName, 50, 120, 0f);
			this._crits.Activate();
		}
		protected override void OnUpdate()
		{
			if (this._starmapReduced != null && !this._starmapReduced.Active && this._starmapReduced.ObjectStatus != GameObjectStatus.Pending)
			{
				this._starmapReduced.Active = true;
			}
			if (this._cameraReduced != null && !this._cameraReduced.Active && this._cameraReduced.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cameraReduced.Active = true;
			}
		}
		public override void HandleScriptMessage(ScriptMessageReader mr)
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == ReactionDialog.UIConfirmButton)
				{
					FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._interceptFleetWidget.SelectedFleet);
					if (this._app.GameDatabase.GetDiplomacyStateBetweenPlayers(this._currentReaction.fleet.PlayerID, fleetInfo.PlayerID) != DiplomacyState.WAR)
					{
						this._hostileActionDialog = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@UI_TARGETMISSION_CONFIRM_HOSTILE_ACTION_TITLE"), App.Localize("@UI_TARGETMISSION_CONFIRM_HOSTILE_ACTION_DESCRIPTION"), "dialogGenericQuestion"), null);
						return;
					}
					this.CommitMission();
					return;
				}
				else
				{
					if (panelName == ReactionDialog.UICancelButton)
					{
						this.GetNextReaction();
						return;
					}
				}
			}
			else
			{
				if (msgType == "list_sel_changed")
				{
					if (panelName == ReactionDialog.UIFleetWidget)
					{
						int num = int.Parse(msgParams[0]);
						FleetInfo fleetInfo2 = this._app.GameDatabase.GetFleetInfo(this._interceptFleetWidget.SelectedFleet);
						StarMapSystem target = this._starmapReduced.Systems.Reverse[fleetInfo2.SystemID];
						this._starmapReduced.SetFocus(target, 10f);
						if (num > -1)
						{
							this._app.UI.SetEnabled(this._app.UI.Path(new string[]
							{
								base.ID,
								ReactionDialog.UIConfirmButton
							}), true);
							return;
						}
						this._app.UI.SetEnabled(this._app.UI.Path(new string[]
						{
							base.ID,
							ReactionDialog.UIConfirmButton
						}), false);
						return;
					}
				}
				else
				{
					if (msgType == "dialog_closed" && panelName == this._hostileActionDialog && bool.Parse(msgParams[0]))
					{
						this.CommitMission();
					}
				}
			}
		}
		private void GetNextReaction()
		{
			this._app.UI.CloseDialog(this, true);
			this._app.Game.RemoveReaction(this._currentReaction);
			ReactionInfo nextReactionForPlayer = this._app.Game.GetNextReactionForPlayer(this._app.LocalPlayer.ID);
			if (nextReactionForPlayer != null)
			{
				this._app.UI.CreateDialog(new ReactionDialog(this._app, nextReactionForPlayer), null);
				return;
			}
			if (!this._app.GameSetup.IsMultiplayer)
			{
				this._app.Game.Phase4_Combat();
				return;
			}
			if (this._app.Network.IsJoined)
			{
				this._app.GameDatabase.LogComment("SYNC (ReactionDialog.GetNextReaction)");
				this._app.Network.SendHistory(this._app.GameDatabase.GetTurnCount());
				return;
			}
			if (this._app.Network.IsHosting)
			{
				this._app.Network.ReactionComplete();
			}
		}
		private void CommitMission()
		{
			FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this._interceptFleetWidget.SelectedFleet);
			int systemID = fleetInfo.SystemID;
			int systemID2 = this._currentReaction.fleet.SystemID;
			this._app.GameDatabase.ChangeDiplomacyState(this._currentReaction.fleet.PlayerID, fleetInfo.PlayerID, DiplomacyState.WAR);
			this._app.GameDatabase.UpdateFleetLocation(this._currentReaction.fleet.ID, systemID, null);
			MissionInfo missionByFleetID = this._app.GameDatabase.GetMissionByFleetID(this._currentReaction.fleet.ID);
			if (missionByFleetID == null)
			{
                Kerberos.Sots.StarFleet.StarFleet.SetRelocationMission(this._app.Game, this._currentReaction.fleet.ID, systemID2, false, null);
			}
			else
			{
				List<WaypointInfo> list = this._app.GameDatabase.GetWaypointsByMissionID(missionByFleetID.ID).ToList<WaypointInfo>();
				foreach (WaypointInfo current in list)
				{
					this._app.GameDatabase.RemoveWaypoint(current.ID);
				}
				this._app.GameDatabase.InsertWaypoint(missionByFleetID.ID, WaypointType.TravelTo, new int?(systemID2));
				foreach (WaypointInfo current2 in list)
				{
					this._app.GameDatabase.InsertWaypoint(missionByFleetID.ID, current2.Type, current2.SystemID);
				}
			}
			this.GetNextReaction();
		}
		public override string[] CloseDialog()
		{
			this._crits.Dispose();
			this._starmapReduced.Dispose();
			this._cameraReduced.Dispose();
			this._interceptFleetWidget.Dispose();
			return null;
		}
	}
}
