using System;
namespace Kerberos.Sots.UI
{
	internal class DialogNeutronStar : Dialog
	{
		private static readonly string panelID = "dialogSuperNova";
		private int NumColonies;
		public DialogNeutronStar(App app, int numColonies) : base(app, DialogNeutronStar.panelID)
		{
			this.NumColonies = numColonies;
		}
		public override void Initialize()
		{
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"title"
			}), App.Localize("@UI_NEUTRON_STAR_PRESENT"));
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"subTitle"
			}), string.Format(App.Localize("@UI_NEUTRON_STAR_SYSTEMS_IN_RANGE"), this.NumColonies));
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"description"
			}), App.Localize("@UI_NEUTRON_STAR_IN_RANGE_DESC"));
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
