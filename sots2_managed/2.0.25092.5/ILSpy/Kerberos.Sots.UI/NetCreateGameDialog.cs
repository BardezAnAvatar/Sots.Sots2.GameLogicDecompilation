using System;
namespace Kerberos.Sots.UI
{
	internal class NetCreateGameDialog : Dialog
	{
		public const string OKButton = "buttonGameSetup";
		public const string LoadButton = "btnLoad";
		public const string CancelButton = "btnGameDlgCancel";
		public const string GameNameBox = "game_name";
		public const string PassBox = "game_password";
		private string _enteredGameName;
		private string _enteredPass;
		private LoadGameDialog _loadGameDlg;
		private bool _cancelled;
		private bool _loaded;
		public NetCreateGameDialog(App game) : base(game, "dialogNetCreateGame")
		{
			this._enteredGameName = "";
			this._enteredPass = "";
		}
		protected bool ValidateGameName()
		{
			if (string.IsNullOrEmpty(this._enteredGameName) || this._enteredGameName.Length > 30 || this._enteredGameName.Length < 3)
			{
				this._app.UI.CreateDialog(new GenericTextDialog(this._app, "@ERROR", "@GAMENAMEERROR", "dialogGenericMessage"), null);
				return false;
			}
			return true;
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_opened")
			{
				if (panelName == base.ID)
				{
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						base.ID,
						"lblUser"
					}), "text", App.Localize("@UI_GAMELOBBY_USERNAME_COLON"));
					this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
					{
						base.ID,
						"lblPass"
					}), "text", App.Localize("@UI_GAMELOBBY_PASSWORD_COLON"));
					return;
				}
			}
			else
			{
				if (msgType == "dialog_closed")
				{
					if (this._loadGameDlg != null && panelName == this._loadGameDlg.ID)
					{
						this._loaded = (msgParams[0] == "True");
						if (this._loaded)
						{
							this._cancelled = false;
							this._app.UI.CloseDialog(this, true);
							return;
						}
					}
				}
				else
				{
					if (msgType == "button_clicked")
					{
						if (panelName == "btnGameDlgCancel")
						{
							this._cancelled = true;
							this._app.UI.CloseDialog(this, true);
							return;
						}
						if (panelName == "buttonGameSetup")
						{
							if (this.ValidateGameName())
							{
								this._cancelled = false;
								this._app.UI.CloseDialog(this, true);
								return;
							}
						}
						else
						{
							if (panelName == "btnLoad" && this.ValidateGameName())
							{
								this._app.GameSetup.IsMultiplayer = true;
								this._loadGameDlg = new LoadGameDialog(this._app, null);
								this._app.UI.CreateDialog(this._loadGameDlg, null);
								return;
							}
						}
					}
					else
					{
						if (msgType == "text_changed")
						{
							if (panelName == "game_name")
							{
								this._enteredGameName = msgParams[0];
								return;
							}
							if (panelName == "game_password")
							{
								this._enteredPass = msgParams[0];
							}
						}
					}
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
				this._cancelled.ToString(),
				this._loaded.ToString(),
				this._enteredGameName,
				this._enteredPass
			};
		}
	}
}
