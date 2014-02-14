using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class GiveScalarDialog : Dialog
	{
		private const string HeaderLabel = "lblHeader";
		private const string MinValueLabel = "lblMinValue";
		private const string MaxValueLabel = "lblMaxValue";
		private const string ValueSlider = "sldValue";
		private const string ValueEditBox = "txtValue";
		private const string RequestButton = "btnFinishRequest";
		private const string CancelButton = "btnCancel";
		private int _otherPlayer;
		private GiveType _type;
		private GiveInfo _give;
		private float MaxValue;
		private float MinValue;
		public GiveScalarDialog(App game, GiveType type, int otherPlayer, string template = "dialogGiveScalar") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
			this._type = type;
			this._give = new GiveInfo();
			this._give.InitiatingPlayer = game.LocalPlayer.ID;
			this._give.ReceivingPlayer = this._otherPlayer;
			this._give.Type = type;
		}
		public override void Initialize()
		{
			DiplomacyUI.SyncDiplomacyPopup(this._app, base.ID, this._otherPlayer);
			this.SyncScalar();
		}
		private void SyncScalar()
		{
			double num = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).Savings;
			if (num < 0.0)
			{
				num = (double)this.MaxValue;
			}
			switch (this._type)
			{
			case GiveType.GiveSavings:
				this.MaxValue = (float)num;
				this.MinValue = 1f;
				break;
			case GiveType.GiveResearchPoints:
				this.MaxValue = (float)num;
				this.MinValue = 1f;
				break;
			}
			this._app.UI.SetText("lblHeader", string.Format(App.Localize(GiveTypeDialog.GiveTypeLocMap[this._type]), 0));
			this._app.UI.SetSliderRange("sldValue", (int)this.MinValue, (int)this.MaxValue);
			this._app.UI.SetText("lblMinValue", this.MinValue.ToString("N0"));
			this._app.UI.SetText("lblMaxValue", this.MaxValue.ToString("N0"));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnFinishRequest")
				{
					this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
					PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
					this._app.GameDatabase.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings - (double)this._give.GiveValue);
					this._app.GameDatabase.InsertGive(this._give);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnCancel")
				{
					this._give = null;
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
						this._give.GiveValue = float.Parse(msgParams[0]);
						this._app.UI.SetText("txtValue", msgParams[0]);
						return;
					}
				}
				else
				{
					if (msgType == "text_changed")
					{
						int num;
						if (int.TryParse(msgParams[0], out num))
						{
							this._give.GiveValue = Math.Max(this.MinValue, Math.Min((float)num, this.MaxValue));
							this._app.UI.SetSliderValue("sldValue", (int)this._give.GiveValue);
							this._app.UI.SetText("txtValue", ((int)this._give.GiveValue).ToString());
							return;
						}
						if (msgParams[0] == string.Empty)
						{
							this._give.GiveValue = this.MinValue;
							this._app.UI.SetSliderValue("sldValue", (int)this._give.GiveValue);
						}
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			List<string> list = new List<string>();
			if (this._give == null)
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
