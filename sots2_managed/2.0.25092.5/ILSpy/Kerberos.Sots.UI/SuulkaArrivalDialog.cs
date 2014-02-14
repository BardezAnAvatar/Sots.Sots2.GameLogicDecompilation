using System;
namespace Kerberos.Sots.UI
{
	internal class SuulkaArrivalDialog : Dialog
	{
		private int _systemID;
		public SuulkaArrivalDialog(App game, int systemID) : base(game, "dialogSuulkaArrival")
		{
			this._systemID = systemID;
		}
		public override void Initialize()
		{
			this._app.UI.SetPropertyString("system_name", "text", App.Localize("@DIALOG_SUULKA_ARRIVES") + " - " + this._app.GameDatabase.GetStarSystemInfo(this._systemID).Name);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "event_dialog_close")
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
