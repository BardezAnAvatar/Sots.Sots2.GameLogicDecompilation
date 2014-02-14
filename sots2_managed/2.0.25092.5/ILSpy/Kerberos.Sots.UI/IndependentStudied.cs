using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using System;
namespace Kerberos.Sots.UI
{
	internal class IndependentStudied : Dialog
	{
		public const string OkButton = "okButton";
		public const string DiploButton = "DiplomacyButton";
		private int _playerID;
		public IndependentStudied(App game, int playerid) : base(game, "dialogIndependentSurvalenceComplete")
		{
			this._playerID = playerid;
		}
		public override void Initialize()
		{
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._playerID);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"tech_desc"
			}), "text", App.Localize("@UI_INDEPENDENT_DESCRIPTION_" + playerInfo.Name.ToUpper()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"radimage"
			}), "sprite", "Independent_Splash_" + playerInfo.Name.ToUpper());
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"faction_name"
			}), "text", App.Localize("@UI_INDEPENDENT_COMPLETE_" + playerInfo.Name.ToUpper()) + "  -  " + App.Localize("@UI_INDEPENDENT_TECH_" + playerInfo.Name.ToUpper()));
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"advantage"
			}), "text", App.Localize("@UI_INDEPENDENT_BONUS_" + playerInfo.Name.ToUpper()));
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._app.UI.CloseDialog(this, true);
				}
				if (panelName == "DiplomacyButton")
				{
					this._app.SwitchGameState<DiplomacyScreenState>(new object[0]);
					this._app.UI.CloseDialog(this, true);
				}
			}
		}
		protected override void OnUpdate()
		{
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
