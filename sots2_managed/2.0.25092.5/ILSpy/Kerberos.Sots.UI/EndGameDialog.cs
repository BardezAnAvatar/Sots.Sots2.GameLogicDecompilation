using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.UI
{
	internal class EndGameDialog : Dialog
	{
		private const string UIOkButton = "btnOk";
		private const string UITitleLabel = "lblTitle";
		private const string UIDescriptionLabel = "lblDescription";
		private const string UIEndGameImage = "imgEndGame";
		private string _title = "";
		private string _message = "";
		private string _graphic = "";
		public EndGameDialog(App game, string title, string message, string graphic) : base(game, "dialogEndGame")
		{
			this._title = title;
			this._message = message;
			this._graphic = graphic;
		}
		public override void Initialize()
		{
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"lblTitle"
			}), this._title);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"lblDescription"
			}), this._message);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"imgEndGame"
			}), "sprite", this._graphic);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == "btnOk")
			{
				if (this._app.GameSetup.IsMultiplayer)
				{
					this._app.Network.Disconnect();
				}
				this._app.GetGameState<StarMapState>().Reset();
				this._app.UI.CloseDialog(this, true);
				this._app.SwitchGameStateViaLoadingScreen(null, null, this._app.GetGameState<MainMenuState>(), null);
				this._app.UI.CloseDialog(this, true);
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
