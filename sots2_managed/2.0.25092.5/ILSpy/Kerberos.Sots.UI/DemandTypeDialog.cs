using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.UI
{
	internal class DemandTypeDialog : Dialog
	{
		private const string DemandPanel = "pnlDemand";
		private const string BoxPanel = "pnlBoxPanel";
		private const string DemMoneyButton = "btnDemMoney";
		private const string DemSystemInfoButton = "btnDemSystemInfo";
		private const string DemResearchPointsButton = "btnDemResearchPoints";
		private const string DemSlavesButton = "btnDemSlaves";
		private const string DemWorldButton = "btnDemWorld";
		private const string DemProvinceButton = "btnDemProvince";
		private const string DemSurrenderButton = "btnDemSurrender";
		private const string CancelButton = "btnCancel";
		public static Dictionary<DemandType, string> DemandTypeLocMap = new Dictionary<DemandType, string>
		{

			{
				DemandType.ProvinceDemand,
				"@UI_DIPLOMACY_DEMAND_PROVINCE"
			},

			{
				DemandType.ResearchPointsDemand,
				"@UI_DIPLOMACY_DEMAND_RESEARCHPOINTS"
			},

			{
				DemandType.SavingsDemand,
				"@UI_DIPLOMACY_DEMAND_SAVINGS"
			},

			{
				DemandType.SlavesDemand,
				"@UI_DIPLOMACY_DEMAND_SLAVES"
			},

			{
				DemandType.SurrenderDemand,
				"@UI_DIPLOMACY_DEMAND_SURRENDER"
			},

			{
				DemandType.SystemInfoDemand,
				"@UI_DIPLOMACY_DEMAND_SYSTEMINFO"
			},

			{
				DemandType.WorldDemand,
				"@UI_DIPLOMACY_DEMAND_WORLD"
			}
		};
		private int _otherPlayer;
		public DemandTypeDialog(App game, int otherPlayer, string template = "dialogDemandType") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
		}
		public override void Initialize()
		{
			DiplomacyUI.SyncDiplomacyPopup(this._app, base.ID, this._otherPlayer);
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.Game.LocalPlayer.ID);
			PlayerInfo playerInfo2 = this._app.GameDatabase.GetPlayerInfo(this._otherPlayer);
			this._app.UI.SetEnabled("btnDemMoney", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, null, new DemandType?(DemandType.SavingsDemand)));
			this._app.UI.SetEnabled("btnDemSystemInfo", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, null, new DemandType?(DemandType.SystemInfoDemand)));
			this._app.UI.SetEnabled("btnDemResearchPoints", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, null, new DemandType?(DemandType.ResearchPointsDemand)));
			this._app.UI.SetEnabled("btnDemSlaves", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, null, new DemandType?(DemandType.SlavesDemand)));
			this._app.UI.SetEnabled("btnDemWorld", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, null, new DemandType?(DemandType.WorldDemand)));
			this._app.UI.SetEnabled("btnDemProvince", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, null, new DemandType?(DemandType.ProvinceDemand)));
			this._app.UI.SetEnabled("btnDemSurrender", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, null, new DemandType?(DemandType.SurrenderDemand)));
			this._app.UI.SetButtonText("btnDemMoney", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SAVINGS"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, null, new DemandType?(DemandType.SavingsDemand))));
			this._app.UI.SetButtonText("btnDemSystemInfo", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SYSTEMINFO"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, null, new DemandType?(DemandType.SystemInfoDemand))));
			this._app.UI.SetButtonText("btnDemResearchPoints", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_RESEARCHPOINTS"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, null, new DemandType?(DemandType.ResearchPointsDemand))));
			this._app.UI.SetButtonText("btnDemSlaves", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SLAVES"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, null, new DemandType?(DemandType.SlavesDemand))));
			this._app.UI.SetButtonText("btnDemWorld", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_WORLD"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, null, new DemandType?(DemandType.WorldDemand))));
			this._app.UI.SetButtonText("btnDemProvince", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_PROVINCE"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, null, new DemandType?(DemandType.ProvinceDemand))));
			this._app.UI.SetButtonText("btnDemSurrender", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SURRENDER"), this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, null, new DemandType?(DemandType.SurrenderDemand))));
			if (this._app.AssetDatabase.GetFaction(playerInfo.FactionID).HasSlaves())
			{
				this._app.UI.SetPropertyInt("pnlDemand", "height", 210);
				this._app.UI.SetPropertyInt(this._app.UI.Path(new string[]
				{
					"pnlDemand",
					"pnlBoxPanel"
				}), "height", 160);
				this._app.UI.SetVisible("btnDemSlaves", true);
				return;
			}
			this._app.UI.SetPropertyInt("pnlDemand", "height", 190);
			this._app.UI.SetPropertyInt(this._app.UI.Path(new string[]
			{
				"pnlDemand",
				"pnlBoxPanel"
			}), "height", 140);
			this._app.UI.SetVisible("btnDemSlaves", false);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnDemMoney")
				{
					this._app.UI.CreateDialog(new DemandScalarDialog(this._app, DemandType.SavingsDemand, this._otherPlayer, "dialogRequestScalar"), null);
					return;
				}
				if (panelName == "btnDemSystemInfo")
				{
					this._app.UI.CreateDialog(new DemandSystemSelectDialog(this._app, DemandType.SystemInfoDemand, this._otherPlayer, "dialogRequestSystemSelect"), null);
					return;
				}
				if (panelName == "btnDemResearchPoints")
				{
					this._app.UI.CreateDialog(new DemandScalarDialog(this._app, DemandType.ResearchPointsDemand, this._otherPlayer, "dialogRequestScalar"), null);
					return;
				}
				if (panelName == "btnDemSlaves")
				{
					this._app.UI.CreateDialog(new DemandScalarDialog(this._app, DemandType.SlavesDemand, this._otherPlayer, "dialogRequestScalar"), null);
					return;
				}
				if (panelName == "btnDemWorld")
				{
					this._app.UI.CreateDialog(new DemandSystemSelectDialog(this._app, DemandType.WorldDemand, this._otherPlayer, "dialogRequestSystemSelect"), null);
					return;
				}
				if (panelName == "btnDemProvince")
				{
					this._app.UI.CreateDialog(new DemandSystemSelectDialog(this._app, DemandType.ProvinceDemand, this._otherPlayer, "dialogRequestSystemSelect"), null);
					return;
				}
				if (panelName == "btnDemSurrender")
				{
					this._app.UI.CreateDialog(new DemandSystemSelectDialog(this._app, DemandType.SurrenderDemand, this._otherPlayer, "dialogRequestSystemSelect"), null);
					return;
				}
				if (panelName == "btnCancel")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				if (msgType == "dialog_closed" && msgParams[0] != "true")
				{
					this._app.UI.CloseDialog(this, true);
				}
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
