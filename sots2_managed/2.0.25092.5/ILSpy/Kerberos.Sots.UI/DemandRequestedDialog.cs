using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.UI
{
	internal class DemandRequestedDialog : Dialog
	{
		private const string RequestHeader = "lblRequestHeader";
		private const string AcceptButton = "btnAccept";
		private const string DeclineButton = "btnDecline";
		private DemandInfo _demand;
		public DemandRequestedDialog(App game, DemandInfo di, string template = "dialogRequested") : base(game, template)
		{
			this._demand = di;
		}
		public override void Initialize()
		{
			this.SyncDetails();
		}
		private void SyncDetails()
		{
			DiplomacyCard diplomacyCard = new DiplomacyCard(this._app, this._demand.InitiatingPlayer, base.UI, "pnlRequest");
			diplomacyCard.Initialize();
			this._app.UI.SetText("lblTitle", "@EV_DEMAND_REQUESTED");
			this._app.UI.SetText("lblRequestHeader", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_REQUESTED"), this._app.GameDatabase.GetPlayerInfo(this._demand.InitiatingPlayer).Name, this._demand.ToString(this._app.GameDatabase)));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnAccept")
				{
					this._app.Game.AcceptDemand(this._demand);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnDecline")
				{
					this._app.Game.DeclineDemand(this._demand);
					this._app.UI.CloseDialog(this, true);
				}
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
