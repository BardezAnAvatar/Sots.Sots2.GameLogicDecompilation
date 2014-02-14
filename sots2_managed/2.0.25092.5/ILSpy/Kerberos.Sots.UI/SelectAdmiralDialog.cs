using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class SelectAdmiralDialog : Dialog
	{
		private int _systemid;
		private App App;
		private int _currentAdmiralID;
		private int _currentShipID;
		private int _currentDesignID;
		private string _nameFleetDialog;
		private string _transfercubesDialog;
		public SelectAdmiralDialog(App game, int systemID, string template = "dialogSelectAdmiral") : base(game, template)
		{
			this._systemid = systemID;
			this.App = game;
		}
		public override void Initialize()
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "selectadmiralbtn")
				{
					this.App.UI.CloseDialog(this, true);
					this.App.UI.CreateDialog(new AdmiralManagerDialog(this.App, this.App.LocalPlayer.ID, this._systemid, true, "AdmiralManagerDialog"), null);
				}
				else
				{
					if (panelName == "autoselectadmiralbtn")
					{
						this.App.UI.SetVisible(base.ID, false);
						this.AutoChooseAdmiral();
					}
					else
					{
						if (panelName == "cancelbtn")
						{
							this.App.UI.CloseDialog(this, true);
						}
					}
				}
			}
			if (msgType == "dialog_closed")
			{
				if (panelName == this._nameFleetDialog)
				{
					if (!bool.Parse(msgParams[0]))
					{
						this.App.UI.SetVisible(base.ID, true);
						return;
					}
					int num = this._app.GameDatabase.InsertFleet(this.App.LocalPlayer.ID, this._currentAdmiralID, this._systemid, this._systemid, this._app.GameDatabase.ResolveNewFleetName(this._app, this.App.LocalPlayer.ID, msgParams[1]), FleetType.FL_NORMAL);
					if (!(this._app.LocalPlayer.Faction.Name == "loa"))
					{
						this._app.GameDatabase.TransferShip(this._currentShipID, num);
						this.App.UI.CreateDialog(new FleetCompositorDialog(this.App, this._systemid, num, "dialogFleetCompositor"), null);
						this.App.UI.CloseDialog(this, true);
						return;
					}
					FleetInfo fleetInfo = this._app.GameDatabase.GetFleetsByPlayerAndSystem(this.App.LocalPlayer.ID, this._systemid, FleetType.FL_RESERVE).First<FleetInfo>();
					if (fleetInfo != null)
					{
						ShipInfo shipInfo = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
						DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(this._currentDesignID);
						if (shipInfo != null && designInfo != null)
						{
							this._transfercubesDialog = this._app.UI.CreateDialog(new DialogLoaShipTransfer(this._app, num, fleetInfo.ID, shipInfo.ID, designInfo.ProductionCost), null);
							return;
						}
					}
				}
				else
				{
					if (panelName == this._transfercubesDialog && msgParams.Count<string>() == 4)
					{
						int fleetID = int.Parse(msgParams[0]);
						int.Parse(msgParams[1]);
						int shipID = int.Parse(msgParams[2]);
						int num2 = int.Parse(msgParams[3]);
						ShipInfo shipInfo2 = this._app.GameDatabase.GetShipInfo(shipID, true);
						ShipInfo shipInfo3 = this._app.GameDatabase.GetShipInfoByFleetID(fleetID, false).FirstOrDefault((ShipInfo x) => x.DesignInfo.IsLoaCube());
						if (shipInfo3 == null)
						{
							this._app.GameDatabase.InsertShip(fleetID, shipInfo2.DesignInfo.ID, "Cube", (ShipParams)0, null, num2);
						}
						else
						{
							this._app.GameDatabase.UpdateShipLoaCubes(shipInfo3.ID, shipInfo3.LoaCubes + num2);
						}
						if (shipInfo2.LoaCubes <= num2)
						{
							this._app.GameDatabase.RemoveShip(shipInfo2.ID);
						}
						else
						{
							this._app.GameDatabase.UpdateShipLoaCubes(shipInfo2.ID, shipInfo2.LoaCubes - num2);
						}
						if (this._app.CurrentState == this._app.GetGameState<StarMapState>())
						{
							this._app.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
						}
						if (this._app.CurrentState == this._app.GetGameState<FleetManagerState>())
						{
							this._app.GetGameState<FleetManagerState>().Refresh();
						}
						this.App.UI.CloseDialog(this, true);
					}
				}
			}
		}
		public void AutoChooseAdmiral()
		{
			List<AdmiralInfo> source = (
				from x in this._app.GameDatabase.GetAdmiralInfosForPlayer(this._app.LocalPlayer.ID)
				where this._app.GameDatabase.GetFleetInfoByAdmiralID(x.ID, FleetType.FL_NORMAL) == null
				select x).ToList<AdmiralInfo>();
			AdmiralInfo admiralInfo = source.FirstOrDefault((AdmiralInfo x) => this.App.GameDatabase.GetAdmiralTraits(x.ID).Any((AdmiralInfo.TraitType j) => AdmiralInfo.IsGoodTrait(j)));
			if (admiralInfo == null)
			{
				admiralInfo = source.First<AdmiralInfo>();
			}
			ShipInfo shipInfo = null;
			DesignInfo designInfo = null;
			int? reserveFleetID = this._app.GameDatabase.GetReserveFleetID(this._app.LocalPlayer.ID, this._systemid);
			if (reserveFleetID.HasValue)
			{
				if (this._app.LocalPlayer.Faction.Name == "loa")
				{
                    Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, reserveFleetID.Value);
                    int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, reserveFleetID.Value);
					List<DesignInfo> list = (
						from x in this._app.GameDatabase.GetDesignInfosForPlayer(this._app.LocalPlayer.ID)
						where x.Class == ShipClass.Cruiser && x.GetCommandPoints() > 0
						select x).ToList<DesignInfo>();
					foreach (DesignInfo current in list)
					{
						if (designInfo == null)
						{
							designInfo = current;
						}
						else
						{
							if (designInfo.ProductionCost > current.ProductionCost)
							{
								designInfo = current;
							}
						}
					}
					if (designInfo != null && designInfo.ProductionCost > fleetLoaCubeValue)
					{
						designInfo = null;
					}
				}
				else
				{
					IEnumerable<ShipInfo> shipInfoByFleetID = this._app.GameDatabase.GetShipInfoByFleetID(reserveFleetID.Value, false);
					foreach (ShipInfo current2 in shipInfoByFleetID)
					{
						if (this._app.GameDatabase.GetShipCommandPointQuota(current2.ID) > 0)
						{
							shipInfo = current2;
							break;
						}
					}
				}
			}
			if (shipInfo == null && designInfo == null)
			{
				this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_DESC"), "dialogGenericMessage"), null);
				this._app.UI.CloseDialog(this, true);
				return;
			}
			this._currentAdmiralID = admiralInfo.ID;
			if (shipInfo != null)
			{
				this._currentShipID = shipInfo.ID;
			}
			if (designInfo != null)
			{
				this._currentDesignID = designInfo.ID;
			}
			this._nameFleetDialog = this._app.UI.CreateDialog(new GenericTextEntryDialog(this._app, App.Localize("@UI_FLEET_DIALOG_FLEETNAME_TITLE"), App.Localize("@UI_FLEET_DIALOG_FLEETNAME_DESC"), this._app.GameDatabase.ResolveNewFleetName(this._app, this._app.LocalPlayer.ID, this._app.Game.NamesPool.GetFleetName(this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)))), 24, 1, true, EditBoxFilterMode.None), null);
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
