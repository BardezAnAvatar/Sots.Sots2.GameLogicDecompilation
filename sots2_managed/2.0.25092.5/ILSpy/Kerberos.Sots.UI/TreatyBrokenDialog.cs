using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class TreatyBrokenDialog : Dialog
	{
		private const string DoneButton = "btnDone";
		private const string ConsequenceList = "lstConsequences";
		private const string DescLabel = "lblDesc";
		private TreatyInfo _treatyInfo;
		private bool _isVictim;
		public TreatyBrokenDialog(App game, int treatyId, bool isVictim) : base(game, "dialogTreatyBroken")
		{
			List<TreatyInfo> source = game.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>();
			this._treatyInfo = source.FirstOrDefault((TreatyInfo x) => x.ID == treatyId);
			this._isVictim = isVictim;
			if (this._treatyInfo == null)
			{
				this._app.UI.CloseDialog(this, true);
			}
		}
		public override void Initialize()
		{
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo((this._treatyInfo.InitiatingPlayerId == this._app.LocalPlayer.ID) ? this._treatyInfo.ReceivingPlayerId : this._treatyInfo.InitiatingPlayerId);
			if (this._isVictim)
			{
				this._app.UI.SetPropertyString("lblDesc", "text", string.Format(App.Localize("@UI_TREATY_BROKEN_VICTIM"), playerInfo.Name));
			}
			else
			{
				this._app.UI.SetPropertyString("lblDesc", "text", string.Format(App.Localize("@UI_TREATY_BROKEN_OFFENDER"), playerInfo.Name));
			}
			this._app.UI.ClearItems("lstConsequences");
			if (this._treatyInfo != null)
			{
				foreach (TreatyConsequenceInfo current in this._treatyInfo.Consequences)
				{
					this._app.UI.AddItem("lstConsequences", string.Empty, current.ID, string.Format("{0} {1}", App.Localize(TreatyEditDialog.ConsequenceTypeLocMap[current.Type]), current.ConsequenceValue));
				}
			}
			if (!this._isVictim)
			{
				this._app.GameDatabase.RemoveTreatyInfo(this._treatyInfo.ID);
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "btnDone")
			{
				this._app.UI.CloseDialog(this, true);
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
