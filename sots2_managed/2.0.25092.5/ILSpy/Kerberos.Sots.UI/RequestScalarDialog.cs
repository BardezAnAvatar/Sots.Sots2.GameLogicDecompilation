using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class RequestScalarDialog : Dialog
	{
		private const string HeaderLabel = "lblHeader";
		private const string MinValueLabel = "lblMinValue";
		private const string MaxValueLabel = "lblMaxValue";
		private const string ValueSlider = "sldValue";
		private const string ValueEditBox = "txtValue";
		private const string RequestButton = "btnFinishRequest";
		private const string CancelButton = "btnCancel";
		private int _otherPlayer;
		private RequestType _type;
		private RequestInfo _request;
		public RequestScalarDialog(App game, RequestType type, int otherPlayer, string template = "dialogRequestScalar") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
			this._type = type;
			this._request = new RequestInfo();
			this._request.InitiatingPlayer = game.LocalPlayer.ID;
			this._request.ReceivingPlayer = this._otherPlayer;
			this._request.State = AgreementState.Unrequested;
			this._request.Type = type;
		}
		public override void Initialize()
		{
			DiplomacyUI.SyncDiplomacyPopup(this._app, base.ID, this._otherPlayer);
			this.SyncScalar();
		}
		private void SyncScalar()
		{
			float num = 0f;
			float num2 = 0f;
			switch (this._type)
			{
			case RequestType.SavingsRequest:
				num = 2.5E+07f;
				num2 = 1f;
				break;
			case RequestType.ResearchPointsRequest:
				num = 2.5E+07f;
				num2 = 1f;
				break;
			}
			this._app.UI.SetText("lblHeader", string.Format(App.Localize(RequestTypeDialog.RequestTypeLocMap[this._type]), this._app.AssetDatabase.GetDiplomaticRequestPointCost(this._type)));
			this._app.UI.SetSliderRange("sldValue", (int)num2, (int)num);
			this._app.UI.SetText("lblMinValue", num2.ToString("N0"));
			this._app.UI.SetText("lblMaxValue", num.ToString("N0"));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnFinishRequest")
				{
					PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
					this._app.GameDatabase.SpendDiplomacyPoints(playerInfo, this._app.GameDatabase.GetPlayerFactionID(this._otherPlayer), this._app.AssetDatabase.GetDiplomaticRequestPointCost(this._type));
					this._app.GameDatabase.InsertRequest(this._request);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnCancel")
				{
					this._request = null;
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				if (msgType == "slider_value")
				{
					if (panelName == "sldValue")
					{
						this._request.RequestValue = float.Parse(msgParams[0]);
						this._app.UI.SetText("txtValue", msgParams[0]);
						return;
					}
				}
				else
				{
					int num;
					if (msgType == "text_changed" && int.TryParse(msgParams[0], out num))
					{
						this._request.RequestValue = (float)num;
						this._app.UI.SetSliderValue("sldValue", num);
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			List<string> list = new List<string>();
			if (this._request == null)
			{
				list.Add("true");
			}
			else
			{
				list.Add("false");
			}
			return list.ToArray();
		}
	}
}
