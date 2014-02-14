using System;
namespace Kerberos.Sots.UI
{
	internal class GenericTextDialog : Dialog
	{
		public const string OKButton = "okButton";
		private string _title;
		private string _text;
		public GenericTextDialog(App game, string title, string text, string template = "dialogGenericMessage") : base(game, template)
		{
			this._title = title;
			this._text = text;
		}
		public override void Initialize()
		{
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"title"
			}), "text", this._title);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"textbox"
			}), "text", this._text);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "okButton")
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
