using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Text;
namespace Kerberos.Sots.UI.Dialogs
{
	internal class CounterIntelMissionDialog : Dialog
	{
		private readonly int _targetPlayer;
		private readonly GameSession _game;
		private readonly Label _descLabel;
		private readonly Button _okButton;
		private readonly Button _cancelButton;
		private readonly EspionagePlayerHeader _playerHeader;
		public CounterIntelMissionDialog(GameSession game, int targetPlayer) : base(game.App, "dialogCounterIntelSummary")
		{
			this._targetPlayer = targetPlayer;
			this._game = game;
			this._descLabel = new Label(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"lblIntelDesc"
			}));
			this._okButton = new Button(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"btnOk"
			}), null);
			this._okButton.Clicked += new EventHandler(this._okButton_Clicked);
			this._cancelButton = new Button(base.UI, base.UI.Path(new string[]
			{
				base.ID,
				"btnCancel"
			}), null);
			this._cancelButton.Clicked += new EventHandler(this._cancelButton_Clicked);
			this._playerHeader = new EspionagePlayerHeader(this._game, base.ID);
			base.AddPanels(new PanelBinding[]
			{
				this._descLabel,
				this._okButton,
				this._cancelButton,
				this._playerHeader
			});
		}
		public override void Initialize()
		{
			PlayerInfo playerInfo = this._game.GameDatabase.GetPlayerInfo(this._game.LocalPlayer.ID);
			PlayerInfo playerInfo2 = this._game.GameDatabase.GetPlayerInfo(this._targetPlayer);
			DiplomacyUI.SyncPanelColor(this._app, base.ID, playerInfo2.PrimaryColor);
			this._playerHeader.UpdateFromPlayerInfo(playerInfo.ID, playerInfo2);
			int counterIntelPoints = playerInfo.CounterIntelPoints;
			int requiredCounterIntelPointsForMission = this._game.AssetDatabase.RequiredCounterIntelPointsForMission;
			bool enabled = counterIntelPoints >= requiredCounterIntelPointsForMission;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(string.Format(App.Localize("@UI_DIPLOMACY_COUNTER_INTEL_INFO_DESC_TARGET") + "\n", playerInfo2.Name));
			stringBuilder.Append(string.Format(App.Localize("@UI_DIPLOMACY_COUNTER_INTEL_INFO_DESC_POINTS_REQUIRED") + "\n", requiredCounterIntelPointsForMission, counterIntelPoints));
			this._descLabel.SetText(stringBuilder.ToString());
			this._okButton.SetEnabled(enabled);
		}
		private void _cancelButton_Clicked(object sender, EventArgs e)
		{
			this._app.UI.CloseDialog(this, true);
		}
		private void _okButton_Clicked(object sender, EventArgs e)
		{
			PlayerInfo playerInfo = this._game.GameDatabase.GetPlayerInfo(this._game.LocalPlayer.ID);
			playerInfo.CounterIntelPoints = Math.Max(playerInfo.CounterIntelPoints - this._game.AssetDatabase.RequiredCounterIntelPointsForMission, 0);
			this._game.GameDatabase.UpdatePlayerCounterintelPoints(this._game.LocalPlayer.ID, playerInfo.CounterIntelPoints);
			this._game.GameDatabase.InsertCounterIntelSting(this._game.LocalPlayer.ID, this._targetPlayer);
			this._app.UI.CloseDialog(this, true);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			base.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Recursive);
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
