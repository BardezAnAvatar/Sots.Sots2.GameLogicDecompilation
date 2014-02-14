using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class GiveTypeDialog : Dialog
	{
		private const string GiveMoneyButton = "btnMoney";
		private const string GiveResearchMoneyButton = "btnResearch";
		private const string CancelButton = "btnCancel";
		public static Dictionary<GiveType, string> GiveTypeLocMap = new Dictionary<GiveType, string>
		{

			{
				GiveType.GiveResearchPoints,
				"@UI_DIPLOMACY_GIVE_RESEARCH_MONEY"
			},

			{
				GiveType.GiveSavings,
				"@UI_DIPLOMACY_GIVE_SAVINGS"
			}
		};
		private int _otherPlayer;
		public GiveTypeDialog(App game, int otherPlayer, string template = "dialogGiveType") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
		}
		public override void Initialize()
		{
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
			this._app.GameDatabase.GetPlayerInfo(this._otherPlayer);
			DiplomacyUI.SyncDiplomacyPopup(this._app, base.ID, this._otherPlayer);
			this._app.UI.SetEnabled("btnMoney", playerInfo.Savings > 0.0);
			this._app.UI.SetEnabled("btnResearch", playerInfo.Savings > 0.0);
			this._app.UI.SetButtonText("btnMoney", string.Format(App.Localize(GiveTypeDialog.GiveTypeLocMap[GiveType.GiveSavings]), new object[0]));
			this._app.UI.SetButtonText("btnResearch", string.Format(App.Localize(GiveTypeDialog.GiveTypeLocMap[GiveType.GiveResearchPoints]), new object[0]));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnMoney")
				{
					this._app.UI.CreateDialog(new GiveScalarDialog(this._app, GiveType.GiveSavings, this._otherPlayer, "dialogGiveScalar"), null);
					return;
				}
				if (panelName == "btnResearch")
				{
					this._app.UI.CreateDialog(new GiveScalarDialog(this._app, GiveType.GiveResearchPoints, this._otherPlayer, "dialogGiveScalar"), null);
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
