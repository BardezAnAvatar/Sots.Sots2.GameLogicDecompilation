using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class DemandScalarDialog : Dialog
	{
		private const string HeaderLabel = "lblHeader";
		private const string MinValueLabel = "lblMinValue";
		private const string MaxValueLabel = "lblMaxValue";
		private const string ValueSlider = "sldValue";
		private const string ValueEditBox = "txtValue";
		private const string RequestButton = "btnFinishRequest";
		private const string CancelButton = "btnCancel";
		private int _otherPlayer;
		private DemandType _type;
		private DemandInfo _demand;
		public DemandScalarDialog(App game, DemandType type, int otherPlayer, string template = "dialogRequestScalar") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
			this._type = type;
			this._demand = new DemandInfo();
			this._demand.InitiatingPlayer = game.LocalPlayer.ID;
			this._demand.ReceivingPlayer = this._otherPlayer;
			this._demand.State = AgreementState.Unrequested;
			this._demand.Type = type;
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
			case DemandType.SavingsDemand:
				num = 2.5E+07f;
				num2 = 1f;
				break;
			case DemandType.ResearchPointsDemand:
				num = 2.5E+07f;
				num2 = 1f;
				break;
			case DemandType.SlavesDemand:
				num = 2.5E+07f;
				num2 = 1f;
				break;
			}
			this._app.UI.SetText("lblHeader", string.Format(App.Localize(DemandTypeDialog.DemandTypeLocMap[this._type]), this._app.AssetDatabase.GetDiplomaticDemandPointCost(this._type)));
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
					this._app.GameDatabase.SpendDiplomacyPoints(playerInfo, this._app.GameDatabase.GetPlayerFactionID(this._otherPlayer), this._app.AssetDatabase.GetDiplomaticDemandPointCost(this._type));
					this._app.GameDatabase.InsertDemand(this._demand);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnCancel")
				{
					this._demand = null;
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
						this._demand.DemandValue = float.Parse(msgParams[0]);
						this._app.UI.SetText("txtValue", msgParams[0]);
						return;
					}
				}
				else
				{
					int num;
					if (msgType == "text_changed" && int.TryParse(msgParams[0], out num))
					{
						this._demand.DemandValue = (float)num;
						this._app.UI.SetSliderValue("sldValue", num);
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			List<string> list = new List<string>();
			if (this._demand == null)
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
