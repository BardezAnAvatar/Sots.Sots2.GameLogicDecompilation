using System;
namespace Kerberos.Sots.UI
{
	internal class LoadGameDialog : SaveGameDialog
	{
		public const string OKButton = "buttonOk";
		private bool _choice;
		public LoadGameDialog(App game, string defaultName) : base(game, defaultName, "dialogLoadGame")
		{
		}
		protected override void OnSelectionCleared()
		{
			base.OnSelectionCleared();
			base.UI.SetEnabled("buttonOk", false);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "buttonOk")
				{
					this.Confirm();
					return;
				}
				if (panelName == "buttonCancel")
				{
					this._choice = false;
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
			if (this._selectedIndex != -1)
			{
				this._app.UI.SetEnabled(this._app.UI.Path(new string[]
				{
					base.ID,
					"buttonOk"
				}), true);
			}
		}
		public override void Confirm()
		{
			if (this._selectedIndex == -1)
			{
				return;
			}
			this._choice = true;
			string fileToLoad = this._selectionFileNames[this._selectedIndex];
			this._app.UILoadGame(fileToLoad);
			this._app.UI.CloseDialog(this, true);
		}
		public override void Initialize()
		{
			this._app.UI.SetEnabled(this._app.UI.Path(new string[]
			{
				base.ID,
				"buttonOk"
			}), false);
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
