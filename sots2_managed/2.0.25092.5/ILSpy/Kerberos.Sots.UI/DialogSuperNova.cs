using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.UI
{
	internal class DialogSuperNova : Dialog
	{
		private static readonly string panelID = "dialogSuperNova";
		private string SystemName;
		private int TurnsRemaining;
		private int NumColonies;
		public DialogSuperNova(App app, string systemName, int turnsRemaing, int numColonies) : base(app, DialogSuperNova.panelID)
		{
			this.SystemName = systemName;
			this.TurnsRemaining = turnsRemaing;
			this.NumColonies = numColonies;
		}
		public override void Initialize()
		{
			if (this.TurnsRemaining > 0)
			{
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"title"
				}), string.Format(App.Localize("@UI_SUPER_NOVA_COUNTDOWN_TITLE"), this.TurnsRemaining));
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"subTitle"
				}), string.Format(App.Localize("@UI_SUPER_NOVA_COUNTDOWN_SUBTITLE"), this.NumColonies));
				if (this._app.GameDatabase.GetHasPlayerStudiedSpecialProject(this._app.LocalPlayer.ID, SpecialProjectType.RadiationShielding))
				{
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						base.ID,
						"description"
					}), string.Format(App.Localize("@UI_SUPER_NOVA_COUNTDOWN_DESC_RESEARCHED"), this.SystemName, this.NumColonies, this.TurnsRemaining));
					return;
				}
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"description"
				}), string.Format(App.Localize("@UI_SUPER_NOVA_COUNTDOWN_DESC_NOT_RESEARCHED"), this.SystemName, this.NumColonies, this.TurnsRemaining));
				return;
			}
			else
			{
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"title"
				}), string.Format(App.Localize("@UI_SUPER_NOVA_EXPLODE_TITLE"), this.SystemName));
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"subTitle"
				}), string.Format(App.Localize("@UI_SUPER_NOVA_EXPLODE_SUBTITLE"), this.NumColonies));
				if (this._app.GameDatabase.GetHasPlayerStudiedSpecialProject(this._app.LocalPlayer.ID, SpecialProjectType.RadiationShielding))
				{
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						base.ID,
						"description"
					}), string.Format(App.Localize("@UI_SUPER_NOVA_EXPLODE_DESC_RESEARCHED"), this.SystemName));
					return;
				}
				this._app.UI.SetText(this._app.UI.Path(new string[]
				{
					base.ID,
					"description"
				}), string.Format(App.Localize("@UI_SUPER_NOVA_EXPLODE_DESC_NOT_RESEARCHED"), this.SystemName, this.NumColonies));
				return;
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "btnOK")
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
