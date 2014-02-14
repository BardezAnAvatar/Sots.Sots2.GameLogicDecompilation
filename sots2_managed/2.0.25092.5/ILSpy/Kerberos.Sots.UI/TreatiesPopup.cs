using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class TreatiesPopup : Dialog
	{
		public static string DoneButton = "btnTreatyOk";
		public static string TreatyList = "lstTreaties";
		public static string AddTreatyButton = "btnAddTreaty";
		public static string RemoveTreatyButton = "btnRemoveTreaty";
		private int _otherPlayer;
		private string _treatyEditDialog;
		private int? _selectedTreaty;
		private List<TreatyInfo> _treaties;
		public TreatiesPopup(App game, int otherPlayer, string template = "TreatiesPopup") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
		}
		public override void Initialize()
		{
			DiplomacyUI.SyncDiplomacyPopup(this._app, base.ID, this._otherPlayer);
			this.SyncTreaties();
		}
		private void SyncTreaties()
		{
			this._app.UI.ClearItems(TreatiesPopup.TreatyList);
			this._treaties = (
				from x in this._app.GameDatabase.GetTreatyInfos()
				where (x.ReceivingPlayerId == this._otherPlayer && x.InitiatingPlayerId == this._app.LocalPlayer.ID) || (x.ReceivingPlayerId == this._app.LocalPlayer.ID && x.InitiatingPlayerId == this._otherPlayer)
				select x).ToList<TreatyInfo>();
			foreach (TreatyInfo current in this._treaties)
			{
				if (current.Type == TreatyType.Limitation)
				{
					LimitationTreatyInfo limitationTreatyInfo = (LimitationTreatyInfo)current;
					this._app.UI.AddItem(TreatiesPopup.TreatyList, string.Empty, current.ID, string.Empty);
					string itemGlobalID = this._app.UI.GetItemGlobalID(TreatiesPopup.TreatyList, string.Empty, current.ID, string.Empty);
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"lblHeader"
					}), string.Format("{0}", App.Localize(TreatyEditDialog.LimitationTreatyTypeLocMap[limitationTreatyInfo.LimitationType])));
				}
				else
				{
					this._app.UI.AddItem(TreatiesPopup.TreatyList, string.Empty, current.ID, string.Empty);
					string itemGlobalID2 = this._app.UI.GetItemGlobalID(TreatiesPopup.TreatyList, string.Empty, current.ID, string.Empty);
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"lblHeader"
					}), App.Localize(TreatyEditDialog.TreatyTypeLocMap[current.Type]));
				}
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == TreatiesPopup.TreatyList && !string.IsNullOrEmpty(msgParams[0]))
				{
					this._selectedTreaty = new int?(int.Parse(msgParams[0]));
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == TreatiesPopup.DoneButton)
					{
						this._app.UI.CloseDialog(this, true);
						return;
					}
					if (panelName == TreatiesPopup.AddTreatyButton)
					{
						this._treatyEditDialog = this._app.UI.CreateDialog(new TreatyEditDialog(this._app, this._otherPlayer, "TreatyConfigurationPopup"), null);
						return;
					}
					if (panelName == TreatiesPopup.RemoveTreatyButton && this._selectedTreaty.HasValue)
					{
						TreatyInfo treatyInfo = this._treaties.First((TreatyInfo x) => x.ID == this._selectedTreaty.Value);
						if (treatyInfo.StartingTurn > this._app.GameDatabase.GetTurnCount())
						{
							this._app.GameDatabase.RemoveTreatyInfo(this._selectedTreaty.Value);
							this._app.UI.ClearSelection(TreatiesPopup.TreatyList);
							this._selectedTreaty = null;
							this.SyncTreaties();
							return;
						}
					}
				}
				else
				{
					if (msgType == "dialog_closed" && panelName == this._treatyEditDialog)
					{
						this.SyncTreaties();
						this._app.UI.ClearSelection(TreatiesPopup.TreatyList);
						this._selectedTreaty = null;
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			List<string> list = new List<string>();
			return list.ToArray();
		}
	}
}
