using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class FeasibilityCompleteDialog : Dialog
	{
		public const string OKButton = "okButton";
		private TurnEvent _event;
		private ResearchInfoPanel _researchInfo;
		public FeasibilityCompleteDialog(App game, TurnEvent ev) : base(game, "dialogFeasibilityEvent")
		{
			this._event = ev;
		}
		public override void Initialize()
		{
			PlayerTechInfo pti = this._app.GameDatabase.GetPlayerTechInfo(this._app.LocalPlayer.ID, this._event.TechID);
			Tech tech = this._app.AssetDatabase.MasterTechTree.Technologies.First((Tech x) => x.Id == pti.TechFileID);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"feasible_title"
			}), string.Format(App.Localize("@FEASIBILITY_RESULT"), this._app.AssetDatabase.GetLocalizedTechnologyName(tech.Id), this._event.FeasibilityPercent));
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				base.ID,
				"feasible_details"
			}), this._event.GetEventMessage(this._app.Game));
			this._researchInfo = new ResearchInfoPanel(this._app.UI, this._app.UI.Path(new string[]
			{
				base.ID,
				"research_details"
			}));
			this._researchInfo.SetTech(this._app, this._event.TechID);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "researchButton")
				{
					int playerResearchingTechID = this._app.GameDatabase.GetPlayerResearchingTechID(this._app.LocalPlayer.ID);
					if (playerResearchingTechID != 0)
					{
						this._app.GameDatabase.UpdatePlayerTechState(this._app.LocalPlayer.ID, playerResearchingTechID, TechStates.Core);
					}
					int playerFeasibilityStudyTechId = this._app.GameDatabase.GetPlayerFeasibilityStudyTechId(this._app.LocalPlayer.ID);
					if (playerFeasibilityStudyTechId != 0)
					{
						ResearchScreenState.CancelResearchProject(this._app, this._app.LocalPlayer.ID, playerFeasibilityStudyTechId);
					}
					this._app.GameDatabase.UpdatePlayerTechState(this._app.LocalPlayer.ID, this._event.TechID, TechStates.Researching);
					string cueName = string.Format("STRAT_029-01_{0}_StartResearch", this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)));
					this._app.PostRequestSpeech(cueName, 50, 120, 0f);
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
