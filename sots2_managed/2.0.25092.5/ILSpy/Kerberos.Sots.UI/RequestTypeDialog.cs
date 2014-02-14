using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class RequestTypeDialog : Dialog
	{
		private const string ReqMoneyButton = "btnReqMoney";
		private const string ReqSystemInfoButton = "btnReqSystemInfo";
		private const string ReqResearchPointsButton = "btnReqResearchPoints";
		private const string ReqMilitaryAssistanceButton = "btnReqMilitaryAssistance";
		private const string ReqGatePermissionButton = "btnReqGatePermission";
		private const string ReqEstablishEnclaveButton = "btnReqEstablishEnclave";
		private const string CancelButton = "btnCancel";
		public static Dictionary<RequestType, string> RequestTypeLocMap = new Dictionary<RequestType, string>
		{

			{
				RequestType.EstablishEnclaveRequest,
				"@UI_DIPLOMACY_REQUEST_ESTABLISHENCLAVE"
			},

			{
				RequestType.GatePermissionRequest,
				"@UI_DIPLOMACY_REQUEST_GATEPERMISSION"
			},

			{
				RequestType.MilitaryAssistanceRequest,
				"@UI_DIPLOMACY_REQUEST_MILITARYASSISTANCE"
			},

			{
				RequestType.ResearchPointsRequest,
				"@UI_DIPLOMACY_REQUEST_RESEARCHPOINTS"
			},

			{
				RequestType.SavingsRequest,
				"@UI_DIPLOMACY_REQUEST_SAVINGS"
			},

			{
				RequestType.SystemInfoRequest,
				"@UI_DIPLOMACY_REQUEST_SYSTEMINFO"
			}
		};
		private int _otherPlayer;
		public RequestTypeDialog(App game, int otherPlayer, string template = "dialogRequestType") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
		}
		public override void Initialize()
		{
			this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._otherPlayer);
			DiplomacyUI.SyncDiplomacyPopup(this._app, base.ID, this._otherPlayer);
			this._app.UI.SetEnabled("btnReqMoney", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.SavingsRequest), null));
			this._app.UI.SetEnabled("btnReqSystemInfo", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.SystemInfoRequest), null));
			this._app.UI.SetEnabled("btnReqResearchPoints", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.ResearchPointsRequest), null));
			this._app.UI.SetEnabled("btnReqMilitaryAssistance", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.MilitaryAssistanceRequest), null));
			this._app.UI.SetEnabled("btnReqGatePermission", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.GatePermissionRequest), null));
			this._app.UI.SetEnabled("btnReqEstablishEnclave", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.EstablishEnclaveRequest), null));
			this._app.UI.SetButtonText("btnReqMoney", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_SAVINGS"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.SavingsRequest), null)));
			this._app.UI.SetButtonText("btnReqSystemInfo", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_SYSTEMINFO"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.SystemInfoRequest), null)));
			this._app.UI.SetButtonText("btnReqResearchPoints", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_RESEARCHPOINTS"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.ResearchPointsRequest), null)));
			this._app.UI.SetButtonText("btnReqMilitaryAssistance", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_MILITARYASSISTANCE"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.MilitaryAssistanceRequest), null)));
			this._app.UI.SetButtonText("btnReqGatePermission", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_GATEPERMISSION"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.GatePermissionRequest), null)));
			this._app.UI.SetButtonText("btnReqEstablishEnclave", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_ESTABLISHENCLAVE"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.EstablishEnclaveRequest), null)));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnReqMoney")
				{
					this._app.UI.CreateDialog(new RequestScalarDialog(this._app, RequestType.SavingsRequest, this._otherPlayer, "dialogRequestScalar"), null);
					return;
				}
				if (panelName == "btnReqSystemInfo")
				{
					this._app.UI.CreateDialog(new RequestSystemSelectDialog(this._app, RequestType.SystemInfoRequest, this._otherPlayer, "dialogRequestSystemSelect"), null);
					return;
				}
				if (panelName == "btnReqResearchPoints")
				{
					this._app.UI.CreateDialog(new RequestScalarDialog(this._app, RequestType.ResearchPointsRequest, this._otherPlayer, "dialogRequestScalar"), null);
					return;
				}
				if (panelName == "btnReqMilitaryAssistance")
				{
					this._app.UI.CreateDialog(new RequestSystemSelectDialog(this._app, RequestType.MilitaryAssistanceRequest, this._otherPlayer, "dialogRequestSystemSelect"), null);
					return;
				}
				if (panelName == "btnReqGatePermission")
				{
					this._app.UI.CreateDialog(new RequestSystemSelectDialog(this._app, RequestType.GatePermissionRequest, this._otherPlayer, "dialogRequestSystemSelect"), null);
					return;
				}
				if (panelName == "btnReqEstablishEnclave")
				{
					this._app.UI.CreateDialog(new RequestSystemSelectDialog(this._app, RequestType.EstablishEnclaveRequest, this._otherPlayer, "dialogRequestSystemSelect"), null);
					return;
				}
				if (panelName == "btnCancel")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				if (msgType == "dialog_closed" && msgParams[0] != "true")
				{
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
