using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class SalvageProjectDialog : Dialog
	{
		private SpecialProjectInfo _contextProject;
		private string _confirmProjectChangeDialog;
		private List<SpecialProjectInfo> _rates = new List<SpecialProjectInfo>();
		private int _researchPoints;
		private int _salvagePoints;
		private BudgetPiechart _piechart;
		private BudgetPiechart _behindPiechart;
		private static readonly string UIGovernmentResearchSlider = "research_sliderD";
		private static readonly string UICurrentProjectSlider = "currentProjectSliderD";
		private static readonly string UISpecialProjectSlider = "specialProjectSliderD";
		private static readonly string UISalvageResearchSlider = "salvageResearchSliderD";
		public SalvageProjectDialog(App game, string template = "dialogSpecialProjects") : base(game, template)
		{
		}
		public override void Initialize()
		{
			this._app.UI.UnlockUI();
			this._piechart = new BudgetPiechart(this._app.UI, this._app.UI.Path(new string[]
			{
				base.ID,
				"pnlScreenLeft",
				"piechartD"
			}), this._app.AssetDatabase);
			this._behindPiechart = new BudgetPiechart(this._app.UI, "piechart", this._app.AssetDatabase);
			this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
			{
				base.ID,
				"specialProjectsHeader"
			}), "text", "Salvage Projects");
			this.RefreshProjects();
			EmpireBarUI.SyncResearchSlider(this._app, SalvageProjectDialog.UIGovernmentResearchSlider, this._piechart);
		}
		private void SyncRates()
		{
			foreach (SpecialProjectInfo current in this._rates)
			{
				this._app.GameDatabase.UpdateSpecialProjectRate(current.ID, current.Rate);
			}
		}
		private void RefreshProjects()
		{
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				base.ID,
				"specialList"
			}));
			this.SyncRates();
			this._rates.Clear();
			List<SpecialProjectInfo> list = (
				from x in this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(this._app.LocalPlayer.ID, true)
				where x.Type == SpecialProjectType.Salvage
				select x).ToList<SpecialProjectInfo>();
			list = (
				from x in list
				orderby x.Progress < 0
				select x).ToList<SpecialProjectInfo>();
			float num = list.Sum((SpecialProjectInfo x) => x.Rate);
			if ((double)num < 0.99)
			{
				SpecialProjectInfo specialProjectInfo = list.FirstOrDefault((SpecialProjectInfo x) => x.Progress >= 0);
				if (specialProjectInfo != null)
				{
					specialProjectInfo.Rate = 1f;
				}
			}
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
			Budget budget = Budget.GenerateBudget(this._app.Game, playerInfo, null, BudgetProjection.Actual);
			this._researchPoints = this._app.Game.ConvertToResearchPoints(this._app.LocalPlayer.ID, budget.ResearchSpending.RequestedTotal);
			this._salvagePoints = (int)((float)this._researchPoints * playerInfo.RateResearchSalvageResearch);
			foreach (SpecialProjectInfo current in list)
			{
				this._app.UI.AddItem(this._app.UI.Path(new string[]
				{
					base.ID,
					"specialList"
				}), "", current.ID, "");
				string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
				{
					base.ID,
					"specialList"
				}), "", current.ID, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"projectName"
				}), "text", current.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"description"
				}), "text", GameSession.GetSpecialProjectDescription(current.Type));
				if (current.Progress >= 0)
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"startButton"
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"cancelButton"
					}), true);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"activeIndicator"
					}), true);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"projectRate"
					}), true);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"startButton"
					}), true);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"cancelButton"
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"activeIndicator"
					}), false);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID,
						"projectRate"
					}), false);
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"startButton"
				}), "id", "startButton|" + current.ID.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"cancelButton"
				}), "id", "cancelButton|" + current.ID.ToString());
				this._app.UI.SetSliderValue(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"projectProgress"
				}), (int)((float)current.Progress / (float)current.Cost * 100f));
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"projectRate"
				}), "id", "projectRate|" + current.ID.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(new string[]
				{
					itemGlobalID,
					"projectTurnCount"
				}), "id", "projectTurnCount|" + current.ID.ToString());
				this._rates.Add(current);
			}
			this.RefreshSliders();
			this.UpdateResearchSliders(playerInfo, "");
		}
		private void SetRate(int id, float value)
		{
			SpecialProjectInfo specialProjectInfo = this._rates.FirstOrDefault((SpecialProjectInfo x) => x.ID == id);
			List<SpecialProjectInfo> list = (
				from x in this._rates
				where x.Progress >= 0
				select x).ToList<SpecialProjectInfo>();
			if (specialProjectInfo != null)
			{
				float num = specialProjectInfo.Rate - value;
				float num2 = 0f;
				float num3 = this._rates.Sum((SpecialProjectInfo x) => x.Rate);
				if (this._rates.Count > 1 && (double)num3 >= 0.999)
				{
					int num4 = 100;
					do
					{
						num4--;
						foreach (SpecialProjectInfo current in list)
						{
							if (current.ID != id)
							{
								float num5 = Math.Abs(num / (float)(this._rates.Count - 1));
								if (num < 0f)
								{
									if (current.Rate - num5 > 0f)
									{
										current.Rate -= num5;
									}
									else
									{
										num5 = current.Rate;
										current.Rate = 0f;
									}
									num2 += num5;
									if (num2 >= Math.Abs(num))
									{
										current.Rate += num2 - Math.Abs(num);
										num2 = Math.Abs(num);
										break;
									}
								}
								else
								{
									if (current.Rate + num5 < 1f)
									{
										current.Rate += num5;
									}
									else
									{
										num5 = 1f - current.Rate;
										current.Rate = 1f;
									}
									num2 += num5;
									if (num2 >= num)
									{
										current.Rate -= num2 - num;
										num2 = num;
										break;
									}
								}
							}
						}
					}
					while (num2 < Math.Abs(num) - 0.0001f && num4 > 0);
					specialProjectInfo.Rate -= num;
				}
				else
				{
					specialProjectInfo.Rate = 1f;
				}
			}
			this.RefreshSliders();
		}
		private void RefreshSliders()
		{
			foreach (SpecialProjectInfo current in this._rates)
			{
				this._app.UI.SetSliderValue("projectRate|" + current.ID.ToString(), (int)(current.Rate * 100f));
				int num = current.Cost - current.Progress;
				int num2 = (this._salvagePoints > 0) ? ((int)((float)num / (current.Rate * (float)this._salvagePoints))) : 10000;
				if (current.Progress == -1)
				{
					num2 = ((this._salvagePoints > 0) ? (num / this._salvagePoints) : 10000);
				}
				num2++;
				if (num2 < 0)
				{
					num2 = 10000;
				}
				if (num2 < 5000)
				{
					this._app.UI.SetPropertyString("projectTurnCount|" + current.ID.ToString(), "text", num2.ToString() + " Turns");
				}
				else
				{
					this._app.UI.SetPropertyString("projectTurnCount|" + current.ID.ToString(), "text", "∞ Turns");
				}
				if (current.Progress >= 0)
				{
					if (num2 > 100)
					{
						num2 = 100;
					}
					float num3 = (float)num2 / 100f;
					this._app.UI.SetPropertyColorNormalized("projectTurnCount|" + current.ID.ToString(), "color", new Vector3(Math.Min(num3 * (num3 + 1f), 1f), Math.Min((1f - num3) * (num3 + 1f), 1f), 0f));
				}
				else
				{
					this._app.UI.SetPropertyColorNormalized("projectTurnCount|" + current.ID.ToString(), "color", new Vector3(0.8f, 0.8f, 0.8f));
				}
			}
		}
		private void UpdateResearchSliders(PlayerInfo playerInfo, string iChanged)
		{
			double num = (double)((1f - playerInfo.RateGovernmentResearch) * 100f);
			double num2 = (double)(playerInfo.RateResearchCurrentProject * 100f);
			double num3 = (double)(playerInfo.RateResearchSpecialProject * 100f);
			double num4 = (double)(playerInfo.RateResearchSalvageResearch * 100f);
			if (iChanged != SalvageProjectDialog.UIGovernmentResearchSlider)
			{
				this._app.UI.SetSliderValue(SalvageProjectDialog.UIGovernmentResearchSlider, (int)num);
			}
			if (iChanged != SalvageProjectDialog.UICurrentProjectSlider)
			{
				this._app.UI.SetSliderValue(SalvageProjectDialog.UICurrentProjectSlider, (int)num2);
			}
			if (iChanged != SalvageProjectDialog.UISpecialProjectSlider)
			{
				this._app.UI.SetSliderValue(SalvageProjectDialog.UISpecialProjectSlider, (int)num3);
			}
			if (iChanged != SalvageProjectDialog.UISalvageResearchSlider)
			{
				this._app.UI.SetSliderValue(SalvageProjectDialog.UISalvageResearchSlider, (int)num4);
			}
			this._app.UI.SetSliderValue("research_slider", (int)num);
			this._app.UI.SetSliderValue("gameEmpireResearchSlider", (int)num);
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
				if (panelName.Contains("startButton"))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					int id = int.Parse(array[1]);
					SpecialProjectInfo specialProjectInfo = this._app.GameDatabase.GetSpecialProjectInfo(id);
					if (specialProjectInfo != null && specialProjectInfo.Progress == -1)
					{
						this._contextProject = specialProjectInfo;
						this._confirmProjectChangeDialog = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@UI_SALVAGE_RESEARCH_START_TITLE"), App.Localize("@UI_SALVAGE_RESEARCH_START_DESC"), "dialogGenericQuestion"), null);
						return;
					}
				}
				else
				{
					if (panelName.Contains("cancelButton"))
					{
						string[] array2 = panelName.Split(new char[]
						{
							'|'
						});
						int id2 = int.Parse(array2[1]);
						SpecialProjectInfo specialProjectInfo2 = this._app.GameDatabase.GetSpecialProjectInfo(id2);
						if (specialProjectInfo2 != null && specialProjectInfo2.Progress > -1)
						{
							this._contextProject = specialProjectInfo2;
							this._confirmProjectChangeDialog = this._app.UI.CreateDialog(new GenericQuestionDialog(this._app, App.Localize("@UI_SALVAGE_RESEARCH_CANCEL_TITLE"), App.Localize("@UI_SALVAGE_RESEARCH_CANCEL_DESC"), "dialogGenericQuestion"), null);
							return;
						}
					}
				}
			}
			else
			{
				if (msgType == "slider_value")
				{
					if (panelName.Contains("projectRate"))
					{
						string[] array3 = panelName.Split(new char[]
						{
							'|'
						});
						int id3 = int.Parse(array3[1]);
						this.SetRate(id3, (float)int.Parse(msgParams[0]) / 100f);
						return;
					}
					PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
					if (panelName == SalvageProjectDialog.UIGovernmentResearchSlider)
					{
						float num = (float)int.Parse(msgParams[0]) / 100f;
						num = num.Clamp(0f, 1f);
						playerInfo.RateGovernmentResearch = 1f - num;
						if (this._app.GameDatabase.GetSliderNotchSettingInfo(playerInfo.ID, UISlidertype.SecuritySlider) != null)
						{
							Budget budget = Budget.GenerateBudget(this._app.Game, playerInfo, null, BudgetProjection.Pessimistic);
							EmpireSummaryState.DistributeGovernmentSpending(this._app.Game, EmpireSummaryState.GovernmentSpendings.Security, (float)Math.Min((double)((float)budget.RequiredSecurity / 100f), 1.0), playerInfo);
						}
						else
						{
							this._app.GameDatabase.UpdatePlayerSliders(this._app.Game, playerInfo);
						}
						Budget budget2 = Budget.GenerateBudget(this._app.Game, playerInfo, null, BudgetProjection.Actual);
						this._researchPoints = this._app.Game.ConvertToResearchPoints(this._app.LocalPlayer.ID, budget2.ResearchSpending.RequestedTotal);
						this._salvagePoints = (int)((float)this._researchPoints * playerInfo.RateResearchSalvageResearch);
						Budget slices = Budget.GenerateBudget(this._app.Game, playerInfo, null, BudgetProjection.Pessimistic);
						this._piechart.SetSlices(slices);
						this._behindPiechart.SetSlices(slices);
						this._app.UI.ShakeViolently("piechartD");
					}
					else
					{
						if (panelName == SalvageProjectDialog.UICurrentProjectSlider)
						{
							EmpireSummaryState.DistibuteResearchSpending(this._app.Game, this._app.GameDatabase, EmpireSummaryState.ResearchSpendings.CurrentProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
							this._salvagePoints = (int)((float)this._researchPoints * playerInfo.RateResearchSalvageResearch);
						}
						else
						{
							if (panelName == SalvageProjectDialog.UISpecialProjectSlider)
							{
								EmpireSummaryState.DistibuteResearchSpending(this._app.Game, this._app.GameDatabase, EmpireSummaryState.ResearchSpendings.SpecialProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this._salvagePoints = (int)((float)this._researchPoints * playerInfo.RateResearchSalvageResearch);
							}
							else
							{
								if (panelName == SalvageProjectDialog.UISalvageResearchSlider)
								{
									EmpireSummaryState.DistibuteResearchSpending(this._app.Game, this._app.GameDatabase, EmpireSummaryState.ResearchSpendings.SalvageResearch, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
									this._salvagePoints = (int)((float)this._researchPoints * playerInfo.RateResearchSalvageResearch);
								}
							}
						}
					}
					this.UpdateResearchSliders(playerInfo, panelName);
					this.RefreshSliders();
					return;
				}
				else
				{
					if (msgType == "dialog_closed" && panelName == this._confirmProjectChangeDialog)
					{
						bool flag = bool.Parse(msgParams[0]);
						if (flag && this._contextProject != null)
						{
							if (this._contextProject.Progress == -1)
							{
								this._app.GameDatabase.UpdateSpecialProjectProgress(this._contextProject.ID, 0);
								this.RefreshProjects();
							}
							else
							{
								this._app.GameDatabase.RemoveSpecialProject(this._contextProject.ID);
								this.RefreshProjects();
							}
						}
						this._contextProject = null;
						this._confirmProjectChangeDialog = "";
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			this.SyncRates();
			this._piechart = null;
			this._behindPiechart = null;
			return null;
		}
	}
}
