using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using System;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class FleetCompositorDialog : Dialog
	{
		private int _systemid;
		private App App;
		private int fleetid;
		private FleetWidget fleetlist;
		private FleetWidget workingfleet;
		public FleetCompositorDialog(App game, int systemID, int fleetID, string template = "dialogFleetCompositor") : base(game, template)
		{
			this._systemid = systemID;
			this.App = game;
			this.fleetid = fleetID;
		}
		public override void Initialize()
		{
			this.fleetlist = new FleetWidget(this._app, base.UI.Path(new string[]
			{
				base.ID,
				"gameFleetList"
			}));
			this.workingfleet = new FleetWidget(this._app, base.UI.Path(new string[]
			{
				base.ID,
				"gameWorkingFleet"
			}));
			this.fleetlist.DisableTooltips = true;
			this.workingfleet.DisableTooltips = true;
			this.fleetlist.LinkWidget(this.workingfleet);
			this.workingfleet.LinkWidget(this.fleetlist);
			if (this._app.CurrentState == this._app.GetGameState<FleetManagerState>())
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"FleetManager"
				}), false);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"FleetManager"
				}), true);
			}
			this.fleetlist.SetSyncedFleets((
				from x in this._app.GameDatabase.GetFleetsByPlayerAndSystem(this.App.LocalPlayer.ID, this._systemid, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_DEFENSE)
				where x.ID != this.fleetid
				select x).ToList<FleetInfo>());
			this.workingfleet.SetSyncedFleets(this.fleetid);
			OverlayMission.RefreshFleetAdmiralDetails(this.App, base.ID, this.fleetid, "admiralDetails");
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "OkButton")
				{
					if (this._app.CurrentState == this._app.GetGameState<StarMapState>())
					{
						this._app.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
					}
					if (this._app.CurrentState == this._app.GetGameState<FleetManagerState>())
					{
						this._app.GetGameState<FleetManagerState>().Refresh();
					}
					this.App.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "FleetManager")
				{
					this.App.UI.CloseDialog(this, true);
					this.App.SwitchGameState<FleetManagerState>(new object[]
					{
						this._systemid
					});
				}
			}
		}
		public override string[] CloseDialog()
		{
			this.fleetlist.Dispose();
			this.workingfleet.Dispose();
			return null;
		}
	}
}
