using System;
namespace Kerberos.Sots.UI
{
	internal class GenericQuestionDialog : GenericTextDialog
	{
		public const string CancelButton = "cancelButton";
		public bool _choice;
		public GenericQuestionDialog(App game, string title, string text, string template = "dialogGenericQuestion") : base(game, title, text, template)
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._choice = true;
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "cancelButton")
				{
					this._choice = false;
					this._app.UI.CloseDialog(this, true);
				}
			}
		}
		public override void Initialize()
		{
			base.Initialize();
		}
		public override string[] CloseDialog()
		{
			return new string[]
			{
				this._choice.ToString()
			};
		}
	}
}
