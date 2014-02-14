using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class ColonySelfSufficientDialog : Dialog
	{
		private const string OkButton = "btnOk";
		private const string SupportButton = "btnSupport";
		private OrbitalObjectInfo orbitalObject;
		private PlanetInfo planet;
		private ColonyInfo colony;
		private MissionInfo mission;
		private PlanetWidget planetwidget;
		public ColonySelfSufficientDialog(App game, int planetId, int missionId) : base(game, "dialogColonySelfSufficientEvent")
		{
			this.orbitalObject = game.GameDatabase.GetOrbitalObjectInfo(planetId);
			this.planet = game.GameDatabase.GetPlanetInfo(planetId);
			this.colony = game.GameDatabase.GetColonyInfoForPlanet(planetId);
			this.mission = game.GameDatabase.GetMissionInfo(missionId);
		}
		public override void Initialize()
		{
			if (this.mission != null)
			{
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblDesc"
				}), "text", string.Format(App.Localize("@UI_DIALOGSELFSUFFICIENT_DESC"), new object[]
				{
					this.orbitalObject.Name,
					this._app.GameDatabase.GetStarSystemInfo(this.orbitalObject.StarSystemID).Name,
					this._app.Game.GetNumSupportTrips(this.mission),
					this._app.GameDatabase.GetFleetInfo(this.mission.FleetID).Name
				}));
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"btnSupport"
				}), true);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblDesc"
				}), true);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"btnSupport"
				}), false);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"lblDesc"
				}), false);
			}
			this.planetwidget = new PlanetWidget(this._app, this._app.UI.Path(new string[]
			{
				base.ID,
				"planetcard"
			}));
			this.planetwidget.Sync(this.planet.ID, false, false);
		}
		protected override void OnUpdate()
		{
			if (this.planetwidget != null)
			{
				this.planetwidget.Update();
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnOk")
				{
					if (this.mission != null)
					{
						FleetInfo fleetInfo = this._app.GameDatabase.GetFleetInfo(this.mission.FleetID);
						if (fleetInfo == null)
						{
							this._app.GameDatabase.RemoveMission(this.mission.ID);
							return;
						}
						AdmiralInfo admiralInfo = this._app.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
						if (admiralInfo != null)
						{
							string cueName = string.Format("STRAT_009-01_{0}_{1}UniversalMissionComplete", this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)), admiralInfo.GetAdmiralSoundCueContext(this._app.AssetDatabase));
							this._app.PostRequestSpeech(cueName, 50, 120, 0f);
						}
					}
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnSupport")
				{
					List<WaypointInfo> list = this._app.GameDatabase.GetWaypointsByMissionID(this.mission.ID).ToList<WaypointInfo>();
					foreach (WaypointInfo current in list)
					{
						this._app.GameDatabase.RemoveWaypoint(current.ID);
					}
					FleetInfo fleetInfo2 = this._app.GameDatabase.GetFleetInfo(this.mission.FleetID);
					int numSupportTrips = this._app.Game.GetNumSupportTrips(this.mission);
					for (int i = 0; i < numSupportTrips; i++)
					{
						if (this.mission.TargetSystemID != fleetInfo2.SupportingSystemID)
						{
							this._app.GameDatabase.InsertWaypoint(this.mission.ID, WaypointType.TravelTo, new int?(this.mission.TargetSystemID));
						}
						this._app.GameDatabase.InsertWaypoint(this.mission.ID, WaypointType.DoMission, null);
						if (this.mission.TargetSystemID != fleetInfo2.SupportingSystemID)
						{
							this._app.GameDatabase.InsertWaypoint(this.mission.ID, WaypointType.ReturnHome, null);
						}
					}
					this._app.GameDatabase.InsertWaypoint(this.mission.ID, WaypointType.CheckSupportColony, null);
					foreach (WaypointInfo current2 in list)
					{
						this._app.GameDatabase.InsertWaypoint(this.mission.ID, current2.Type, current2.SystemID);
					}
					this._app.UI.CloseDialog(this, true);
				}
			}
		}
		public override string[] CloseDialog()
		{
			this.planetwidget.Terminate();
			return null;
		}
	}
}
