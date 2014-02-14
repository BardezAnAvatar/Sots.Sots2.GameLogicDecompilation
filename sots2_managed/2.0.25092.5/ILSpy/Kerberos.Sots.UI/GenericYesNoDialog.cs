using System;
namespace Kerberos.Sots.UI
{
	internal class GenericYesNoDialog : GenericTextDialog
	{
		public enum YesNoDialogResult
		{
			Yes,
			No,
			Cancel
		}
		public const string CancelButton = "cancelButton";
		public const string NoButton = "noButton";
		public GenericYesNoDialog.YesNoDialogResult Result;
		public GenericYesNoDialog(App game, string title, string text, string template) : base(game, title, text, template)
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this.Result = GenericYesNoDialog.YesNoDialogResult.Yes;
				}
				else
				{
					if (panelName == "cancelButton")
					{
						this.Result = GenericYesNoDialog.YesNoDialogResult.Cancel;
					}
					else
					{
						if (panelName == "noButton")
						{
							this.Result = GenericYesNoDialog.YesNoDialogResult.No;
						}
					}
				}
				this._app.UI.CloseDialog(this, true);
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
				this.Result.ToString()
			};
		}
	}
}
