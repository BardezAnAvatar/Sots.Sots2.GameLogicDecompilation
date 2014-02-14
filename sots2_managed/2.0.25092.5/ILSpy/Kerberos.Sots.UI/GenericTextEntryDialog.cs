using System;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class GenericTextEntryDialog : GenericQuestionDialog
	{
		public const string EditBoxPanel = "edit_text";
		private string _enteredText;
		private int _maxChars;
		private int _minChars;
		private bool _cancelEnabled;
		private EditBoxFilterMode filterMode;
		public GenericTextEntryDialog(App game, string title, string text, string defaultText = "", int maxChars = 1024, int minChars = 0, bool cancelEnabled = true, EditBoxFilterMode filterMode = EditBoxFilterMode.None) : base(game, title, text, "dialogGenericTextEntry")
		{
			this._maxChars = maxChars;
			this._minChars = minChars;
			this._enteredText = defaultText;
			this._cancelEnabled = cancelEnabled;
			this.filterMode = filterMode;
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "okButton")
			{
				this.Confirm();
				return;
			}
			if (msgType == "edit_confirmed")
			{
				if (panelName == "edit_text")
				{
					this.Confirm();
					return;
				}
			}
			else
			{
				if (msgType == "text_changed" && panelName == "edit_text")
				{
					this._enteredText = msgParams[0];
					return;
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}
		public void Confirm()
		{
			if (this._enteredText.Count<char>() < this._minChars)
			{
				this._app.UI.CreateDialog(new GenericTextDialog(this._app, App.Localize("@INVALID_NAME"), string.Format(App.Localize("@INVALID_NAME_TEXT"), this._minChars), "dialogGenericMessage"), null);
				return;
			}
			this._choice = true;
			this._app.UI.CloseDialog(this, true);
		}
		public override void Initialize()
		{
			base.Initialize();
			this._app.UI.Send(new object[]
			{
				"SetMaxChars",
				"edit_text",
				this._maxChars
			});
			this._app.UI.SetPropertyString("edit_text", "text", this._enteredText);
			this._app.UI.Send(new object[]
			{
				"SetFilterMode",
				"edit_text",
				this.filterMode.ToString()
			});
			if (!this._cancelEnabled)
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"cancelButton"
				}), false);
			}
		}
		public override string[] CloseDialog()
		{
			return new string[]
			{
				this._choice.ToString(),
				this._enteredText
			};
		}
	}
}
