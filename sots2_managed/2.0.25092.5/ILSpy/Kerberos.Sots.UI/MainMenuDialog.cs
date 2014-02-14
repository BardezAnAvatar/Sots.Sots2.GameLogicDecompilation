using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.UI
{
	internal class MainMenuDialog : Dialog
	{
		public const string UIMainMenuDialog = "gameMainMenu";
		public const string UIMainMenuDialogBack = "gameMainDialogBack";
		public const string UIMainMenuDialogQuit = "gameMainDialogQuit";
		public const string UIMainMenuDialogOptions = "gameOptionsButton";
		public const string UIMainMenuDialogSave = "gameSaveGameButton";
		public const string UIMainMenuDialogAutoMenu = "gameAutoMenuButton";
		public const string UIMainMenuDialogkeybinds = "gameKeyBindButton";
		private string _confirmExitToMenu;
		public MainMenuDialog(App game) : base(game, "dialogMainMenu")
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "gameMainDialogQuit")
				{
					this._confirmExitToMenu = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@UI_CONFIRM_MENU_RETURN_TITLE"), App.Localize("@UI_CONFIRM_MENU_RETURN_DESCRIPTION"), "dialogGenericQuestion"), null);
					return;
				}
				if (panelName == "gameMainDialogBack")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "gameOptionsButton")
				{
					this._app.UI.CreateDialog(new OptionsDialog(this._app, "OptionsDialog"), null);
					return;
				}
				if (panelName == "gameSaveGameButton")
				{
					if (this._app.Game != null)
					{
						this._app.UI.CreateDialog(new SaveGameDialog(this._app, this._app.Game.SaveGameName, "dialogSaveGame"), null);
						return;
					}
				}
				else
				{
					if (panelName == "gameAutoMenuButton")
					{
						this._app.UI.CreateDialog(new AutoMenuDialog(this._app), null);
						return;
					}
					if (panelName == "gameKeyBindButton")
					{
						this._app.UI.CreateDialog(new HotKeyDialog(this._app), null);
						return;
					}
				}
			}
			else
			{
				if (msgType == "dialog_closed" && panelName == this._confirmExitToMenu)
				{
					bool flag = bool.Parse(msgParams[0]);
					if (flag)
					{
						if (this._app.GameSetup.IsMultiplayer)
						{
							this._app.Network.Disconnect();
						}
						this._app.GetGameState<StarMapState>().Reset();
						this._app.UI.CloseDialog(this, true);
						this._app.UI.SetVisible("gameMainMenu", false);
						this._app.SwitchGameStateViaLoadingScreen(null, null, this._app.GetGameState<MainMenuState>(), null);
					}
					this._confirmExitToMenu = "";
				}
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
