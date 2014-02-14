using System;
namespace Kerberos.Sots.UI
{
	internal class NetProcessNetworkDialog : Dialog
	{
		public const string OKButton = "buttonProcess";
		public const string TextArea = "process_text";
		private string _text = "";
		public NetProcessNetworkDialog(App game) : base(game, "dialogNetworkProcess")
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "buttonProcess")
			{
				this._app.UI.CloseDialog(this, true);
			}
		}
		public void AddDialogString(string val)
		{
			this._text = this._text + val + "\n";
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"process_text"
			}), this._text);
		}
		public void ShowButton(bool val)
		{
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				"buttonProcess"
			}), val);
		}
		public override void Initialize()
		{
			base.Initialize();
		}
		public override string[] CloseDialog()
		{
			return new string[0];
		}
	}
}
