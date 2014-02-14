using Kerberos.Sots.Data;
using System;
namespace Kerberos.Sots.UI
{
	internal class RequestRequestedDialog : Dialog
	{
		private const string RequestHeader = "lblRequestHeader";
		private const string AcceptButton = "btnAccept";
		private const string DeclineButton = "btnDecline";
		private RequestInfo _request;
		public RequestRequestedDialog(App game, RequestInfo ri, string template = "dialogRequested") : base(game, template)
		{
			this._request = ri;
		}
		public override void Initialize()
		{
			this.SyncDetails();
		}
		private void SyncDetails()
		{
			DiplomacyCard diplomacyCard = new DiplomacyCard(this._app, this._request.InitiatingPlayer, base.UI, "pnlRequest");
			diplomacyCard.Initialize();
			this._app.UI.SetText("lblTitle", "@EV_REQUEST_REQUESTED");
			this._app.UI.SetText("lblRequestHeader", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_REQUESTED"), this._app.GameDatabase.GetPlayerInfo(this._request.InitiatingPlayer).Name, this._request.ToString(this._app.GameDatabase)));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnAccept")
				{
					this._app.Game.AcceptRequest(this._request);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnDecline")
				{
					this._app.Game.DeclineRequest(this._request);
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
