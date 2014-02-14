using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class VictoryConditionDialog : Dialog
	{
		private GameMode _selectedMode;
		public VictoryConditionDialog(App game, string template = "dialogVictoryCondition") : base(game, template)
		{
		}
		public override void Initialize()
		{
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "modeLastSideStanding")
				{
					this._selectedMode = GameMode.LastSideStanding;
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "modeLastCapitalStanding")
				{
					this._selectedMode = GameMode.LastCapitalStanding;
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "modeFiveStarChambers")
				{
					this._selectedMode = GameMode.StarChamberLimit;
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "modeFiveGemWorlds")
				{
					this._selectedMode = GameMode.GemWorldLimit;
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "modeFiveProvinces")
				{
					this._selectedMode = GameMode.ProvinceLimit;
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "modeTenLeviathans")
				{
					this._selectedMode = GameMode.LeviathanLimit;
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "modeLandGrab")
				{
					this._selectedMode = GameMode.LandGrab;
					this._app.UI.CloseDialog(this, true);
				}
			}
		}
		public override string[] CloseDialog()
		{
			List<string> list = new List<string>();
			List<string> arg_15_0 = list;
			int selectedMode = (int)this._selectedMode;
			arg_15_0.Add(selectedMode.ToString());
			return list.ToArray();
		}
	}
}
