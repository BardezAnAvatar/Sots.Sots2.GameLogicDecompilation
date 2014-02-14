using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class EmpireSummaryState : GameState, IKeyBindListener
	{
		public enum GovernmentSpendings
		{
			Security,
			Stimulus,
			Savings
		}
		private enum SecuritySpendings
		{
			Operations,
			Intel,
			CounterIntel
		}
		private enum StimulusSpendings
		{
			Mining,
			Colonization,
			Trade
		}
		public enum ResearchSpendings
		{
			CurrentProject,
			SpecialProject,
			SalvageResearch
		}
		private BudgetPiechart _piechart;
		private GameState _prev;
		private GameObjectSet _crits;
		private TechCube _techCube;
		private static readonly string UIExitButton = "gameExitButton";
		private static readonly string UIGovernmentType = "governmenttype_title";
		private static readonly string UIGovernmentResearchSlider = "governmentResearchSlider";
		private static readonly string UISecuritySlider = "securitySlider";
		private static readonly string UIOperationsSlider = "operationsSlider";
		private static readonly string UIIntelSlider = "intelSlider";
		private static readonly string UICounterIntelSlider = "counterIntelSlider";
		private static readonly string UIStimulusSlider = "stimulusSlider";
		private static readonly string UIMiningSlider = "miningSlider";
		private static readonly string UIColonizationSlider = "colonizationSlider";
		private static readonly string UITradeSlider = "tradeSlider";
		private static readonly string UISavingsSlider = "savingsSlider";
		private static readonly string UICurrentProjectSlider = "currentProjectSlider";
		private static readonly string UISpecialProjectSlider = "specialProjectSlider";
		private static readonly string UISalvageResearchSlider = "salvageResearchSlider";
		private static readonly string UIEmpireButton = "btnEmpire";
		private static readonly string UIGovernmentButton = "btnGovernment";
		private static readonly string UIGOVScreenPanel = "governmentScreen";
		private static readonly string UIShipInvoicesList = "shipinvoicelist";
		private static readonly string UIShipInvoicesCost = "shipinvoicecost";
		private static readonly string UIStationInvoicesList = "stationinvoicelist";
		private static readonly string UIStationInvoicesCost = "stationinvoicecost";
		private static readonly string UIColonyDevList = "colonydevlist";
		private static readonly string UIColonyDevCost = "colonydevcost";
		private static readonly string UIColonyIncomeList = "colonyincomelist";
		private static readonly string UIColonyIncome = "colonyincome";
		private static readonly string UIFinancialDue = "financialDue_Amount";
		private static readonly string UIFinancialDueTotal = "financialCommit_Value";
		private static readonly string[] UIFactionRelation = new string[]
		{
			"factionRelation1",
			"factionRelation2",
			"factionRelation3",
			"factionRelation4",
			"factionRelation5",
			"factionRelation6",
			"factionRelation7"
		};
		private static readonly string[] UIFactionImage = new string[]
		{
			"factionImage1",
			"factionImage2",
			"factionImage3",
			"factionImage4",
			"factionImage5",
			"factionImage6",
			"factionImage7"
		};
		private static readonly string[] UIFactionBadge = new string[]
		{
			"factionBadge1",
			"factionBadge2",
			"factionBadge3",
			"factionBadge4",
			"factionBadge5",
			"factionBadge6",
			"factionBadge7"
		};
		private static readonly string UITaxesSlider = "taxslider";
		private static readonly string UIImmigrationSlider = "immigrationslider";
		private static readonly string UIImmigrationValue = "immigrationvalue";
		private static readonly string UIImmigrationLabel = "immigrationlabel";
		public EmpireSummaryState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame();
			}
			this._prev = prev;
			base.App.UI.LoadScreen("EmpireSummary");
			this._crits = new GameObjectSet(base.App);
			this._techCube = new TechCube(base.App);
			this._crits.Add(this._techCube);
		}
		protected override void OnEnter()
		{
			if (base.App.LocalPlayer == null)
			{
				base.App.NewGame();
			}
			base.App.UI.SetScreen("EmpireSummary");
			base.App.UI.SetVisible(EmpireSummaryState.UIGOVScreenPanel, false);
			this._piechart = new BudgetPiechart(base.App.UI, "piechart", base.App.AssetDatabase);
			EmpireBarUI.SyncTitleFrame(base.App);
			base.App.UI.InitializeSlider(EmpireSummaryState.UIGovernmentResearchSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UISecuritySlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UIOperationsSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UIIntelSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UICounterIntelSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UIStimulusSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UIMiningSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UIColonizationSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UITradeSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UISavingsSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UICurrentProjectSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UISpecialProjectSlider, 0, 100, 0);
			base.App.UI.InitializeSlider(EmpireSummaryState.UISalvageResearchSlider, 0, 100, 0);
			List<PlayerInfo> list = base.App.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
			for (int i = 0; i < EmpireSummaryState.UIFactionImage.Count<string>(); i++)
			{
				if (list.Count > i)
				{
					PlayerInfo playerInfo2 = list[i];
					if (playerInfo2.ID == base.App.LocalPlayer.ID)
					{
						list.Remove(playerInfo2);
						i--;
					}
					else
					{
						DiplomacyInfo diplomacyInfo = base.App.GameDatabase.GetDiplomacyInfo(base.App.LocalPlayer.ID, playerInfo2.ID);
						DiplomaticMood diplomaticMood = diplomacyInfo.GetDiplomaticMood();
						base.App.UI.SetVisible(EmpireSummaryState.UIFactionRelation[i], true);
						if (diplomaticMood == DiplomaticMood.Love)
						{
							base.App.UI.SetPropertyString(EmpireSummaryState.UIFactionRelation[i], "sprite", "Love");
						}
						else
						{
							if (diplomaticMood == DiplomaticMood.Hatred)
							{
								base.App.UI.SetPropertyString(EmpireSummaryState.UIFactionRelation[i], "sprite", "Hate");
							}
							else
							{
								base.App.UI.SetVisible(EmpireSummaryState.UIFactionRelation[i], false);
							}
						}
						base.App.UI.SetVisible(EmpireSummaryState.UIFactionImage[i], true);
						base.App.UI.SetVisible(EmpireSummaryState.UIFactionBadge[i], true);
						base.App.UI.SetPropertyString(EmpireSummaryState.UIFactionImage[i], "sprite", Path.GetFileNameWithoutExtension(playerInfo2.AvatarAssetPath));
						base.App.UI.SetPropertyString(EmpireSummaryState.UIFactionBadge[i], "sprite", Path.GetFileNameWithoutExtension(playerInfo2.BadgeAssetPath));
					}
				}
				else
				{
					base.App.UI.SetVisible(EmpireSummaryState.UIFactionRelation[i], false);
					base.App.UI.SetVisible(EmpireSummaryState.UIFactionImage[i], false);
					base.App.UI.SetVisible(EmpireSummaryState.UIFactionBadge[i], false);
				}
			}
			int maxValue = (base.App.GameDatabase.GetFactionName(playerInfo.FactionID) == "zuul") ? 7 : 10;
			base.App.UI.InitializeSlider(EmpireSummaryState.UITaxesSlider, 0, maxValue, (int)Math.Round((double)(playerInfo.RateTax * 100f)));
			base.App.UI.InitializeSlider(EmpireSummaryState.UIImmigrationSlider, 0, 10, (int)Math.Round((double)(playerInfo.RateImmigration * 100f)));
			base.App.UI.SetVisible(EmpireSummaryState.UIImmigrationSlider, base.App.GetStratModifier<bool>(StratModifiers.AllowAlienImmigration, playerInfo.ID));
			base.App.UI.SetVisible(EmpireSummaryState.UIImmigrationLabel, base.App.GetStratModifier<bool>(StratModifiers.AllowAlienImmigration, playerInfo.ID));
			base.App.UI.SetVisible(EmpireSummaryState.UIImmigrationValue, base.App.GetStratModifier<bool>(StratModifiers.AllowAlienImmigration, playerInfo.ID));
			List<GovernmentActionInfo> list2 = base.App.GameDatabase.GetGovernmentActions(base.App.LocalPlayer.ID).ToList<GovernmentActionInfo>();
			base.App.UI.ClearItems("eventlist");
			int num = base.App.GameDatabase.GetTurnCount() - 30;
			for (int j = list2.Count - 1; j >= 0; j--)
			{
				GovernmentActionInfo governmentActionInfo = list2[j];
				if (governmentActionInfo.Turn >= num)
				{
					string text = "";
					if (governmentActionInfo.AuthoritarianismChange > 0)
					{
						text = string.Format("+" + App.Localize("@UI_GOVERNMENT_AUTHORITARIANISM"), governmentActionInfo.AuthoritarianismChange);
					}
					else
					{
						if (governmentActionInfo.AuthoritarianismChange < 0)
						{
							text = string.Format(App.Localize("@UI_GOVERNMENT_AUTHORITARIANISM"), governmentActionInfo.AuthoritarianismChange);
						}
					}
					string text2 = "";
					if (governmentActionInfo.EconLiberalismChange > 0)
					{
						text2 = string.Format("+" + App.Localize("@UI_GOVERNMENT_ECON_LIBERALISM"), governmentActionInfo.EconLiberalismChange);
					}
					else
					{
						if (governmentActionInfo.EconLiberalismChange < 0)
						{
							text2 = string.Format(App.Localize("@UI_GOVERNMENT_ECON_LIBERALISM"), governmentActionInfo.EconLiberalismChange);
						}
					}
					if (!string.IsNullOrEmpty(text))
					{
						string text3 = string.Format("{0} - {1}", governmentActionInfo.Description, text);
						base.App.UI.AddItem("eventlist", "", governmentActionInfo.ID, "");
						string itemGlobalID = base.App.UI.GetItemGlobalID("eventlist", "", governmentActionInfo.ID, "");
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							itemGlobalID,
							"lblHeader"
						}), text3);
					}
					if (!string.IsNullOrEmpty(text2))
					{
						string text4 = string.Format("{0} - {1}", governmentActionInfo.Description, text2);
						base.App.UI.AddItem("eventlist", "", -governmentActionInfo.ID, "");
						string itemGlobalID2 = base.App.UI.GetItemGlobalID("eventlist", "", -governmentActionInfo.ID, "");
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							itemGlobalID2,
							"lblHeader"
						}), text4);
					}
				}
			}
			GovernmentInfo governmentInfo = base.App.GameDatabase.GetGovernmentInfo(base.App.LocalPlayer.ID);
			base.App.UI.SetPropertyString(EmpireSummaryState.UIGovernmentType, "text", App.Localize(string.Format("@UI_EMPIRESUMMARY_{0}", governmentInfo.CurrentType.ToString().ToUpper())));
			base.App.UI.SetPropertyString("govDescrip", "text", App.Localize("@GOV_DESC_" + governmentInfo.CurrentType.ToString().ToUpper()));
			base.App.UI.Send(new object[]
			{
				"SetMarkerPosition",
				"pol_spectrum",
				governmentInfo.EconomicLiberalism / (float)base.App.AssetDatabase.MaxGovernmentShift,
				-governmentInfo.Authoritarianism / (float)base.App.AssetDatabase.MaxGovernmentShift
			});
			base.App.UI.SetPropertyBool(EmpireSummaryState.UIGovernmentResearchSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UISecuritySlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UIOperationsSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UIIntelSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UICounterIntelSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UIStimulusSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UIMiningSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UIColonizationSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UITradeSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UISavingsSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UICurrentProjectSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UISpecialProjectSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UISalvageResearchSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UITaxesSlider, "only_user_events", true);
			base.App.UI.SetPropertyBool(EmpireSummaryState.UIImmigrationSlider, "only_user_events", true);
			base.App.UI.SetEnabled(EmpireSummaryState.UIOperationsSlider, false);
			this.RefreshAll(string.Empty, true);
			this._crits.Activate();
			float spinSpeed = (1f - base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).RateGovernmentResearch) * 100f * 0.002f;
			this._techCube.SpinSpeed = spinSpeed;
			this._techCube.UpdateResearchProgress();
			this._techCube.RefreshResearchingTech();
			this.UpdateTechCubeToolTip();
			base.App.HotKeyManager.AddListener(this);
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			if (this._crits != null)
			{
				this._crits.Deactivate();
				this._crits.Dispose();
				this._crits = null;
			}
			this._piechart = null;
		}
		public static void DistributeGovernmentSpending(GameSession App, EmpireSummaryState.GovernmentSpendings lockedBar, float newValue, PlayerInfo pi)
		{
			Dictionary<EmpireSummaryState.GovernmentSpendings, float> dictionary = new Dictionary<EmpireSummaryState.GovernmentSpendings, float>
			{

				{
					EmpireSummaryState.GovernmentSpendings.Savings,
					pi.RateGovernmentSavings
				},

				{
					EmpireSummaryState.GovernmentSpendings.Security,
					pi.RateGovernmentSecurity
				},

				{
					EmpireSummaryState.GovernmentSpendings.Stimulus,
					pi.RateGovernmentStimulus
				}
			};
			AlgorithmExtensions.DistributePercentages<EmpireSummaryState.GovernmentSpendings>(ref dictionary, lockedBar, newValue);
			pi.RateGovernmentSavings = dictionary[EmpireSummaryState.GovernmentSpendings.Savings];
			pi.RateGovernmentSecurity = dictionary[EmpireSummaryState.GovernmentSpendings.Security];
			pi.RateGovernmentStimulus = dictionary[EmpireSummaryState.GovernmentSpendings.Stimulus];
			App.GameDatabase.UpdatePlayerSliders(App, pi);
		}
		private void DistributeSecuritySpending(EmpireSummaryState.SecuritySpendings lockedBar, float newValue, PlayerInfo pi)
		{
			Dictionary<EmpireSummaryState.SecuritySpendings, float> dictionary = new Dictionary<EmpireSummaryState.SecuritySpendings, float>
			{

				{
					EmpireSummaryState.SecuritySpendings.CounterIntel,
					pi.RateSecurityCounterIntelligence
				},

				{
					EmpireSummaryState.SecuritySpendings.Intel,
					pi.RateSecurityIntelligence
				},

				{
					EmpireSummaryState.SecuritySpendings.Operations,
					pi.RateSecurityOperations
				}
			};
			dictionary.Remove(EmpireSummaryState.SecuritySpendings.Operations);
			AlgorithmExtensions.DistributePercentages<EmpireSummaryState.SecuritySpendings>(ref dictionary, lockedBar, newValue);
			pi.RateSecurityCounterIntelligence = dictionary[EmpireSummaryState.SecuritySpendings.CounterIntel];
			pi.RateSecurityIntelligence = dictionary[EmpireSummaryState.SecuritySpendings.Intel];
			base.App.GameDatabase.UpdatePlayerSliders(base.App.Game, pi);
		}
		private void DistributeStimulusSpending(EmpireSummaryState.StimulusSpendings lockedBar, float newValue, PlayerInfo pi, bool enableTrade)
		{
			Dictionary<EmpireSummaryState.StimulusSpendings, float> dictionary = new Dictionary<EmpireSummaryState.StimulusSpendings, float>
			{

				{
					EmpireSummaryState.StimulusSpendings.Colonization,
					pi.RateStimulusColonization
				},

				{
					EmpireSummaryState.StimulusSpendings.Mining,
					pi.RateStimulusMining
				},

				{
					EmpireSummaryState.StimulusSpendings.Trade,
					pi.RateStimulusTrade
				}
			};
			if (!enableTrade)
			{
				dictionary.Remove(EmpireSummaryState.StimulusSpendings.Trade);
			}
			if (!base.App.GameDatabase.PlayerHasTech(base.App.LocalPlayer.ID, "IND_Mega-Strip_Mining"))
			{
				dictionary.Remove(EmpireSummaryState.StimulusSpendings.Mining);
			}
			AlgorithmExtensions.DistributePercentages<EmpireSummaryState.StimulusSpendings>(ref dictionary, lockedBar, newValue);
			if (base.App.GameDatabase.PlayerHasTech(base.App.LocalPlayer.ID, "IND_Mega-Strip_Mining"))
			{
				pi.RateStimulusMining = dictionary[EmpireSummaryState.StimulusSpendings.Mining];
			}
			pi.RateStimulusColonization = dictionary[EmpireSummaryState.StimulusSpendings.Colonization];
			if (enableTrade)
			{
				pi.RateStimulusTrade = dictionary[EmpireSummaryState.StimulusSpendings.Trade];
			}
			base.App.GameDatabase.UpdatePlayerSliders(base.App.Game, pi);
		}
		public static void DistibuteResearchSpending(GameSession game, GameDatabase db, EmpireSummaryState.ResearchSpendings lockedBar, float newValue, PlayerInfo pi)
		{
			Dictionary<EmpireSummaryState.ResearchSpendings, float> dictionary = new Dictionary<EmpireSummaryState.ResearchSpendings, float>
			{

				{
					EmpireSummaryState.ResearchSpendings.CurrentProject,
					pi.RateResearchCurrentProject
				},

				{
					EmpireSummaryState.ResearchSpendings.SalvageResearch,
					pi.RateResearchSalvageResearch
				},

				{
					EmpireSummaryState.ResearchSpendings.SpecialProject,
					pi.RateResearchSpecialProject
				}
			};
			AlgorithmExtensions.DistributePercentages<EmpireSummaryState.ResearchSpendings>(ref dictionary, lockedBar, newValue);
			pi.RateResearchCurrentProject = dictionary[EmpireSummaryState.ResearchSpendings.CurrentProject];
			pi.RateResearchSalvageResearch = dictionary[EmpireSummaryState.ResearchSpendings.SalvageResearch];
			pi.RateResearchSpecialProject = dictionary[EmpireSummaryState.ResearchSpendings.SpecialProject];
			db.UpdatePlayerSliders(game, pi);
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
			{
				return;
			}
			if (msgType == "button_clicked")
			{
				if (panelName == EmpireSummaryState.UIExitButton)
				{
					if (this._prev != null)
					{
						object[] parms = new object[0];
						if (this._prev.Name == "StarSystemState")
						{
							parms = new object[]
							{
								((StarSystemState)this._prev).CurrentSystem,
								((StarSystemState)this._prev).SelectedObject
							};
						}
						base.App.SwitchGameState(this._prev, parms);
						return;
					}
					base.App.SwitchGameState<StarMapState>(new object[0]);
					return;
				}
				else
				{
					if (panelName == EmpireSummaryState.UIEmpireButton)
					{
						base.App.UI.SetVisible(EmpireSummaryState.UIGOVScreenPanel, false);
						return;
					}
					if (panelName == EmpireSummaryState.UIGovernmentButton)
					{
						base.App.UI.SetVisible(EmpireSummaryState.UIGOVScreenPanel, true);
						return;
					}
				}
			}
			else
			{
				if (msgType == "mouse_enter")
				{
					if (panelName.StartsWith("government_"))
					{
						string[] array = panelName.Split(new char[]
						{
							'_'
						});
						string text = array[1].ToUpper();
						base.App.UI.SetPropertyString(EmpireSummaryState.UIGovernmentType, "text", App.Localize(string.Format("@UI_EMPIRESUMMARY_{0}", text)));
						base.App.UI.SetPropertyString("govDescrip", "text", App.Localize("@GOV_DESC_" + text));
						return;
					}
				}
				else
				{
					if (msgType == "mouse_leave")
					{
						if (panelName.StartsWith("government_"))
						{
							GovernmentInfo governmentInfo = base.App.GameDatabase.GetGovernmentInfo(base.App.LocalPlayer.ID);
							base.App.UI.SetPropertyString(EmpireSummaryState.UIGovernmentType, "text", App.Localize(string.Format("@UI_EMPIRESUMMARY_{0}", governmentInfo.CurrentType.ToString().ToUpper())));
							base.App.UI.SetPropertyString("govDescrip", "text", App.Localize("@GOV_DESC_" + governmentInfo.CurrentType.ToString().ToUpper()));
							return;
						}
					}
					else
					{
						if (msgType == "slider_value")
						{
							PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
							if (panelName == EmpireSummaryState.UITaxesSlider)
							{
								float num = float.Parse(msgParams[0]) / 100f;
								base.App.GameDatabase.UpdateTaxRate(playerInfo.ID, (num != 0f) ? num : 0f);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UIImmigrationSlider)
							{
								float num2 = float.Parse(msgParams[0]);
								base.App.GameDatabase.UpdateImmigrationRate(playerInfo.ID, (num2 != 0f) ? (num2 / 100f) : 0f);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UIGovernmentResearchSlider)
							{
								float num3 = (float)int.Parse(msgParams[0]) / 100f;
								num3 = num3.Clamp(0f, 1f);
								playerInfo.RateGovernmentResearch = 1f - num3;
								base.App.GameDatabase.UpdatePlayerSliders(base.App.Game, playerInfo);
								this.RefreshAll(panelName, false);
								this._techCube.SpinSpeed = (float)int.Parse(msgParams[0]) * 0.002f;
								this.UpdateTechCubeToolTip();
								return;
							}
							if (panelName == EmpireSummaryState.UISecuritySlider)
							{
								EmpireSummaryState.DistributeGovernmentSpending(base.App.Game, EmpireSummaryState.GovernmentSpendings.Security, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UIStimulusSlider)
							{
								EmpireSummaryState.DistributeGovernmentSpending(base.App.Game, EmpireSummaryState.GovernmentSpendings.Stimulus, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UISavingsSlider)
							{
								EmpireSummaryState.DistributeGovernmentSpending(base.App.Game, EmpireSummaryState.GovernmentSpendings.Savings, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UIOperationsSlider)
							{
								this.DistributeSecuritySpending(EmpireSummaryState.SecuritySpendings.Operations, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UIIntelSlider)
							{
								this.DistributeSecuritySpending(EmpireSummaryState.SecuritySpendings.Intel, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UICounterIntelSlider)
							{
								this.DistributeSecuritySpending(EmpireSummaryState.SecuritySpendings.CounterIntel, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UIMiningSlider)
							{
								this.DistributeStimulusSpending(EmpireSummaryState.StimulusSpendings.Mining, (float)int.Parse(msgParams[0]) / 100f, playerInfo, base.App.GetStratModifier<bool>(StratModifiers.EnableTrade, base.App.LocalPlayer.ID));
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UIColonizationSlider)
							{
								this.DistributeStimulusSpending(EmpireSummaryState.StimulusSpendings.Colonization, (float)int.Parse(msgParams[0]) / 100f, playerInfo, base.App.GetStratModifier<bool>(StratModifiers.EnableTrade, base.App.LocalPlayer.ID));
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UITradeSlider)
							{
								this.DistributeStimulusSpending(EmpireSummaryState.StimulusSpendings.Trade, (float)int.Parse(msgParams[0]) / 100f, playerInfo, base.App.GetStratModifier<bool>(StratModifiers.EnableTrade, base.App.LocalPlayer.ID));
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UICurrentProjectSlider)
							{
								EmpireSummaryState.DistibuteResearchSpending(base.App.Game, base.App.GameDatabase, EmpireSummaryState.ResearchSpendings.CurrentProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UISpecialProjectSlider)
							{
								EmpireSummaryState.DistibuteResearchSpending(base.App.Game, base.App.GameDatabase, EmpireSummaryState.ResearchSpendings.SpecialProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
							if (panelName == EmpireSummaryState.UISalvageResearchSlider)
							{
								EmpireSummaryState.DistibuteResearchSpending(base.App.Game, base.App.GameDatabase, EmpireSummaryState.ResearchSpendings.SalvageResearch, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this.RefreshAll(panelName, false);
								return;
							}
						}
						else
						{
							if (msgType == "slider_notched")
							{
								int num4 = int.Parse(msgParams[0]);
								if (panelName == EmpireSummaryState.UISecuritySlider)
								{
									if (num4 != -1)
									{
										base.App.GameDatabase.InsertUISliderNotchSetting(base.App.LocalPlayer.ID, UISlidertype.SecuritySlider, 0.0, 0);
										return;
									}
									base.App.GameDatabase.DeleteUISliderNotchSetting(base.App.LocalPlayer.ID, UISlidertype.SecuritySlider);
								}
							}
						}
					}
				}
			}
		}
		private void RefreshAll(string panelName, bool BuildLists = false)
		{
			bool stratModifier = base.App.GetStratModifier<bool>(StratModifiers.EnableTrade, base.App.LocalPlayer.ID);
			PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
			Budget budget = Budget.GenerateBudget(base.App.Game, playerInfo, null, BudgetProjection.Pessimistic);
			int numColonies = base.App.GameDatabase.GetNumColonies(base.App.LocalPlayer.ID);
			int numProvinces = base.App.GameDatabase.GetNumProvinces(base.App.LocalPlayer.ID);
			int num = base.App.GameDatabase.GetStationInfosByPlayerID(base.App.LocalPlayer.ID).Count<StationInfo>();
			int num2 = base.App.GameDatabase.GetFleetInfosByPlayerID(base.App.LocalPlayer.ID, FleetType.FL_NORMAL).Count<FleetInfo>();
			int numShips = base.App.GameDatabase.GetNumShips(base.App.LocalPlayer.ID);
			double empirePopulation = base.App.GameDatabase.GetEmpirePopulation(base.App.LocalPlayer.ID);
			float empireEconomy = base.App.GameDatabase.GetEmpireEconomy(base.App.LocalPlayer.ID);
			int empireBiosphere = base.App.GameDatabase.GetEmpireBiosphere(base.App.LocalPlayer.ID);
			double tradeRevenue = budget.TradeRevenue;
			int? empireMorale = base.App.GameDatabase.GetEmpireMorale(base.App.LocalPlayer.ID);
			double num3 = (double)((1f - playerInfo.RateGovernmentResearch) * 100f);
			double num4 = (double)(playerInfo.RateGovernmentSecurity * 100f);
			double num5 = (double)(playerInfo.RateSecurityOperations * 100f);
			double num6 = (double)(playerInfo.RateSecurityIntelligence * 100f);
			double num7 = (double)(playerInfo.RateSecurityCounterIntelligence * 100f);
			double num8 = (double)(playerInfo.RateGovernmentStimulus * 100f);
			double num9 = (double)(playerInfo.RateStimulusMining * 100f);
			double num10 = (double)(playerInfo.RateStimulusColonization * 100f);
			double num11 = (double)(playerInfo.RateStimulusTrade * 100f);
			double num12 = (double)(playerInfo.RateGovernmentSavings * 100f);
			float arg_29B_0 = playerInfo.RateGovernmentResearch;
			double num13 = (double)(playerInfo.RateResearchCurrentProject * 100f);
			double num14 = (double)(playerInfo.RateResearchSpecialProject * 100f);
			double num15 = (double)(playerInfo.RateResearchSalvageResearch * 100f);
			string text = budget.TotalRevenue.ToString("N0");
			string text2 = budget.ProjectedGovernmentSpending.ToString("N0");
			string text3 = budget.ResearchSpending.ProjectedTotal.ToString("N0");
			string text4 = budget.SecuritySpending.ProjectedTotal.ToString("N0");
			double num16 = budget.SecuritySpending.ProjectedOperations;
			string text5 = num16.ToString("N0");
			num16 = budget.SecuritySpending.ProjectedIntelligence;
			string text6 = num16.ToString("N0");
			num16 = budget.SecuritySpending.ProjectedCounterIntelligence;
			string text7 = num16.ToString("N0");
			string text8 = budget.StimulusSpending.ProjectedTotal.ToString("N0");
			num16 = budget.StimulusSpending.ProjectedMining;
			string text9 = num16.ToString("N0");
			num16 = budget.StimulusSpending.ProjectedColonization;
			string text10 = num16.ToString("N0");
			num16 = budget.StimulusSpending.ProjectedTrade;
			string text11 = num16.ToString("N0");
			string text12 = budget.NetSavingsIncome.ToString("N0");
			string text13 = budget.CurrentSavings.ToString("N0");
			string text14 = budget.SavingsInterest.ToString("N0");
			string text15 = budget.ProjectedSavings.ToString("N0");
			num16 = budget.ResearchSpending.ProjectedCurrentProject;
			string text16 = num16.ToString("N0");
			num16 = budget.ResearchSpending.ProjectedSpecialProject;
			string text17 = num16.ToString("N0");
			num16 = budget.ResearchSpending.ProjectedSalvageResearch;
			string text18 = num16.ToString("N0");
			string text19 = (budget.ColonySupportExpenses + budget.CurrentShipUpkeepExpenses + budget.CurrentStationUpkeepExpenses + budget.CorruptionExpenses + budget.DebtInterest).ToString("N0");
			string text20 = (budget.TotalExpenses + budget.PendingBuildStationsCost + budget.PendingStationsModulesCost + budget.PendingBuildShipsCost).ToString("N0");
			string text21 = budget.ColonySupportExpenses.ToString("N0");
			string text22 = budget.CurrentShipUpkeepExpenses.ToString("N0");
			string text23 = budget.CurrentStationUpkeepExpenses.ToString("N0");
			string text24 = budget.CorruptionExpenses.ToString("N0");
			string text25 = budget.DebtInterest.ToString("N0");
			string text26 = (budget.IORevenue + budget.TaxRevenue).ToString("N0");
			string text27 = text2;
			string text28 = text20;
			string text29 = text3;
			string text30 = text13;
			string text31 = text15;
			string str = numColonies.ToString("N0");
			string str2 = numProvinces.ToString("N0");
			string text32 = num.ToString("N0");
			string text33 = num2.ToString("N0");
			string text34 = numShips.ToString("N0");
			string text35 = empirePopulation.ToString("N0");
			string text36 = (empireEconomy * 100f).ToString("N0");
			string text37 = empireBiosphere.ToString("N0");
			string text38 = tradeRevenue.ToString("N0");
			string text39 = empireMorale.HasValue ? empireMorale.Value.ToString("N0") : "n/a";
			string text40 = budget.PendingBuildShipsCost.ToString("N0");
			string text41 = (budget.PendingBuildStationsCost + budget.PendingStationsModulesCost).ToString("N0");
			string text42 = (budget.TotalExpenses + budget.TotalBuildShipCosts + budget.TotalBuildStationsCost + budget.TotalStationsModulesCost).ToString("N0");
			string text43 = "Turn " + base.App.GameDatabase.GetTurnCount().ToString("N0");
			base.App.UI.SetEnabled(EmpireSummaryState.UITradeSlider, stratModifier);
			base.App.UI.SetEnabled(EmpireSummaryState.UIMiningSlider, base.App.GameDatabase.PlayerHasTech(base.App.LocalPlayer.ID, "IND_Mega-Strip_Mining"));
			if (base.App.LocalPlayer.Faction.Name == "loa" && base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID).Savings < 0.0)
			{
				base.App.UI.SetEnabled(EmpireSummaryState.UIGovernmentResearchSlider, false);
			}
			else
			{
				base.App.UI.SetEnabled(EmpireSummaryState.UIGovernmentResearchSlider, true);
			}
			if (panelName != EmpireSummaryState.UIGovernmentResearchSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UIGovernmentResearchSlider, (int)num3);
			}
			if (panelName != EmpireSummaryState.UISecuritySlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UISecuritySlider, (int)num4);
			}
			if (budget.ProjectedGovernmentSpending > 0.0)
			{
				base.App.UI.ClearSliderNotches(EmpireSummaryState.UISecuritySlider);
				base.App.UI.AddSliderNotch(EmpireSummaryState.UISecuritySlider, budget.RequiredSecurity);
				if (base.App.GameDatabase.GetSliderNotchSettingInfo(base.App.LocalPlayer.ID, UISlidertype.SecuritySlider) != null)
				{
					base.App.UI.SetSliderValue(EmpireSummaryState.UISecuritySlider, budget.RequiredSecurity);
					EmpireSummaryState.DistributeGovernmentSpending(base.App.Game, EmpireSummaryState.GovernmentSpendings.Security, (float)Math.Min((double)((float)budget.RequiredSecurity / 100f), 1.0), playerInfo);
				}
			}
			if (panelName != EmpireSummaryState.UIOperationsSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UIOperationsSlider, (int)num5);
			}
			if (panelName != EmpireSummaryState.UIIntelSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UIIntelSlider, (int)num6);
			}
			if (panelName != EmpireSummaryState.UICounterIntelSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UICounterIntelSlider, (int)num7);
			}
			if (panelName != EmpireSummaryState.UIStimulusSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UIStimulusSlider, (int)num8);
			}
			if (panelName != EmpireSummaryState.UIMiningSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UIMiningSlider, (int)num9);
			}
			if (panelName != EmpireSummaryState.UIColonizationSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UIColonizationSlider, (int)num10);
			}
			if (panelName != EmpireSummaryState.UITradeSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UITradeSlider, (int)num11);
			}
			if (panelName != EmpireSummaryState.UISavingsSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UISavingsSlider, (int)num12);
			}
			if (panelName != EmpireSummaryState.UICurrentProjectSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UICurrentProjectSlider, (int)num13);
			}
			if (panelName != EmpireSummaryState.UISpecialProjectSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UISpecialProjectSlider, (int)num14);
			}
			if (panelName != EmpireSummaryState.UISalvageResearchSlider)
			{
				base.App.UI.SetSliderValue(EmpireSummaryState.UISalvageResearchSlider, (int)num15);
			}
			base.App.UI.SetText("incomeValue", text);
			base.App.UI.SetText("governmentValue", text2);
			base.App.UI.SetText("researchValue", text3);
			base.App.UI.SetText("researchTotalValue", text3);
			base.App.UI.SetText("securityValue", text4);
			base.App.UI.SetText("operationsValue", text5);
			base.App.UI.SetText("intelValue", text6);
			base.App.UI.SetText("counterIntelValue", text7);
			base.App.UI.SetText("stimulusValue", text8);
			base.App.UI.SetText("miningValue", text9);
			base.App.UI.SetText("colonizationValue", text10);
			base.App.UI.SetText("tradeValue", text11);
			base.App.UI.SetText("savingsValue", text12);
			base.App.UI.SetText("interestValue", text14);
			base.App.UI.SetText("treasuryValue", text13);
			base.App.UI.SetText("projectedTreasuryValue", text15);
			base.App.UI.SetText("currentProjectValue", text16);
			base.App.UI.SetText("specialProjectValue", text17);
			base.App.UI.SetText("salvageResearchValue", text18);
			base.App.UI.SetText("expensesValue", text19);
			base.App.UI.SetText("colonyDevelopmentValue", text21);
			base.App.UI.SetText("fleetMaintenanceValue", text22);
			base.App.UI.SetText("stationMaintenanceValue", text23);
			base.App.UI.SetText("embezzlementValue", text24);
			base.App.UI.SetText("debtInterestValue", text25);
			base.App.UI.SetVisible("embezzlementValue", base.App.LocalPlayer.Faction.Name != "loa");
			base.App.UI.SetVisible("debtInterestValue", base.App.LocalPlayer.Faction.Name != "loa");
			base.App.UI.SetVisible("interestValue", base.App.LocalPlayer.Faction.Name != "loa");
			base.App.UI.SetVisible("InterestLabel", base.App.LocalPlayer.Faction.Name != "loa");
			base.App.UI.SetVisible("embezzlementString", base.App.LocalPlayer.Faction.Name != "loa");
			base.App.UI.SetVisible("debtString", base.App.LocalPlayer.Faction.Name != "loa");
			base.App.UI.SetText("summaryGovernmentValue", text27);
			base.App.UI.SetText("summaryExpensesValue", text28);
			base.App.UI.SetText("summaryResearchValue", text29);
			base.App.UI.SetText("summarySavingsValue", text30);
			base.App.UI.SetText("summaryProjectedValue", text31);
			base.App.UI.SetText("turn_label", text43);
			base.App.UI.SetText(EmpireSummaryState.UIShipInvoicesCost, text40);
			base.App.UI.SetText(EmpireSummaryState.UIStationInvoicesCost, text41);
			base.App.UI.SetText(EmpireSummaryState.UIColonyDevCost, text21);
			base.App.UI.SetText(EmpireSummaryState.UIColonyIncome, text26);
			base.App.UI.SetText(EmpireSummaryState.UIFinancialDue, text20);
			base.App.UI.SetText(EmpireSummaryState.UIFinancialDueTotal, text42);
			base.App.UI.SetPropertyColor("summaryProjectedValue", "color", 255f, 255f, 255f);
			base.App.UI.SetText("coloniesvalue", str + " " + App.Localize("@UI_PLANET_MANAGER_OWNED_PLANETS"));
			base.App.UI.SetText("provincesvalue", str2 + " " + App.Localize("@UI_STARMAPVIEW_PROVINCE_DISPLAY"));
			base.App.UI.SetText("basesvalue", text32);
			base.App.UI.SetText("fleetsvalue", text33);
			base.App.UI.SetText("shipsvalue", text34);
			base.App.UI.SetText("populationvalue", text35);
			base.App.UI.SetText("economyvalue", text36);
			base.App.UI.SetText("biospherevalue", text37);
			base.App.UI.SetText("tradeunitsvalue", text38);
			base.App.UI.SetText("averagemoralevalue", text39);
			if (BuildLists)
			{
				List<int> list = base.App.GameDatabase.GetPlayerColonySystemIDs(playerInfo.ID).ToList<int>();
				base.App.UI.ClearItems(EmpireSummaryState.UIShipInvoicesList);
				List<int> ShipOrdersDueNextTurn = new List<int>();
				foreach (int current in list)
				{
					List<BuildOrderInfo> list2 = base.App.GameDatabase.GetBuildOrdersForSystem(current).ToList<BuildOrderInfo>();
					float num17 = 0f;
					List<ColonyInfo> list3 = base.App.GameDatabase.GetColonyInfosForSystem(current).ToList<ColonyInfo>();
					foreach (ColonyInfo current2 in list3)
					{
						if (current2.PlayerID == playerInfo.ID)
						{
							num17 += Colony.GetConstructionPoints(base.App.Game, current2);
						}
					}
					num17 *= base.App.Game.GetStationBuildModifierForSystem(current, playerInfo.ID);
					foreach (BuildOrderInfo current3 in list2)
					{
						DesignInfo designInfo = base.App.GameDatabase.GetDesignInfo(current3.DesignID);
						if (designInfo.PlayerID == playerInfo.ID)
						{
							int num18 = designInfo.SavingsCost;
							if (designInfo.IsLoaCube())
							{
								num18 = current3.LoaCubes * base.App.AssetDatabase.LoaCostPerCube;
							}
							int num19 = current3.ProductionTarget - current3.Progress;
							if ((float)num19 <= num17)
							{
								ShipOrdersDueNextTurn.Add(current3.ID);
								base.App.UI.AddItem(EmpireSummaryState.UIShipInvoicesList, "", current3.ID, "");
								string itemGlobalID = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIShipInvoicesList, "", current3.ID, "");
								base.App.UI.SetText(base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"designName"
								}), designInfo.Name + " - " + current3.ShipName);
								base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"designName"
								}), "color", 50f, 202f, 240f);
								base.App.UI.SetTooltip(base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"header_idle"
								}), num18.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current).Name);
								base.App.UI.SetTooltip(base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"header_sel"
								}), num18.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current).Name);
								base.App.UI.SetVisible(base.App.UI.Path(new string[]
								{
									itemGlobalID,
									"designDeleteButton"
								}), false);
								num17 -= (float)num19;
							}
						}
					}
				}
				foreach (int current4 in list)
				{
					List<BuildOrderInfo> list4 = (
						from x in base.App.GameDatabase.GetBuildOrdersForSystem(current4)
						where !ShipOrdersDueNextTurn.Contains(x.ID)
						select x).ToList<BuildOrderInfo>();
					foreach (BuildOrderInfo current5 in list4)
					{
						DesignInfo designInfo2 = base.App.GameDatabase.GetDesignInfo(current5.DesignID);
						if (designInfo2.PlayerID == playerInfo.ID)
						{
							int num20 = designInfo2.SavingsCost;
							if (designInfo2.IsLoaCube())
							{
								num20 = current5.LoaCubes * base.App.AssetDatabase.LoaCostPerCube;
							}
							base.App.UI.AddItem(EmpireSummaryState.UIShipInvoicesList, "", current5.ID, "");
							string itemGlobalID2 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIShipInvoicesList, "", current5.ID, "");
							base.App.UI.SetText(base.App.UI.Path(new string[]
							{
								itemGlobalID2,
								"designName"
							}), designInfo2.Name + " - " + current5.ShipName);
							base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
							{
								itemGlobalID2,
								"designName"
							}), "color", 250f, 170f, 50f);
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID2,
								"header_idle"
							}), num20.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current4).Name);
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID2,
								"header_sel"
							}), num20.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current4).Name);
							base.App.UI.SetVisible(base.App.UI.Path(new string[]
							{
								itemGlobalID2,
								"designDeleteButton"
							}), false);
						}
					}
				}
				base.App.UI.ClearItems(EmpireSummaryState.UIStationInvoicesList);
				List<int> ConstMissionsDue = new List<int>();
				List<int> ConstModulesDue = new List<int>();
				foreach (MissionInfo current6 in 
					from x in base.App.GameDatabase.GetMissionInfos()
					where x.Type == MissionType.CONSTRUCT_STN
					select x)
				{
					FleetInfo fleetInfo = base.App.GameDatabase.GetFleetInfo(current6.FleetID);
					if (fleetInfo.PlayerID == playerInfo.ID)
					{
                        MissionEstimate missionEstimate = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(base.App.Game, current6.Type, (StationType)current6.StationType.Value, fleetInfo.ID, current6.TargetSystemID, current6.TargetOrbitalObjectID, null, 1, false, null, null);
						if (missionEstimate.TotalTurns - 1 - missionEstimate.TurnsToReturn <= 1)
						{
							ConstMissionsDue.Add(current6.ID);
							base.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", current6.ID, "");
							string itemGlobalID3 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", current6.ID, "");
							base.App.UI.SetText(base.App.UI.Path(new string[]
							{
								itemGlobalID3,
								"designName"
							}), "Build " + ((StationType)current6.StationType.Value).ToDisplayText(base.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name));
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID3,
								"header_idle"
							}), missionEstimate.ConstructionCost.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current6.TargetSystemID).Name);
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID3,
								"header_sel"
							}), missionEstimate.ConstructionCost.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current6.TargetSystemID).Name);
							base.App.UI.SetVisible(base.App.UI.Path(new string[]
							{
								itemGlobalID3,
								"designDeleteButton"
							}), false);
							base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
							{
								itemGlobalID3,
								"designName"
							}), "color", 50f, 202f, 240f);
						}
					}
				}
				foreach (MissionInfo current7 in 
					from x in base.App.GameDatabase.GetMissionInfos()
					where x.Type == MissionType.UPGRADE_STN && x.Duration > 0
					select x)
				{
					FleetInfo fleetInfo2 = base.App.GameDatabase.GetFleetInfo(current7.FleetID);
					if (base.App.GameDatabase.GetStationInfo(current7.TargetOrbitalObjectID) != null && fleetInfo2.PlayerID == playerInfo.ID && current7.Duration > 0)
					{
						StationInfo stationInfo = base.App.GameDatabase.GetStationInfo(current7.TargetOrbitalObjectID);
						if (stationInfo.DesignInfo.StationLevel + 1 <= 5)
						{
                            MissionEstimate missionEstimate2 = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(base.App.Game, current7.Type, stationInfo.DesignInfo.StationType, fleetInfo2.ID, current7.TargetSystemID, current7.TargetOrbitalObjectID, null, stationInfo.DesignInfo.StationLevel + 1, false, null, null);
							if (missionEstimate2.TotalTurns - 1 - missionEstimate2.TurnsToReturn <= 1)
							{
								DesignInfo designInfo3 = DesignLab.CreateStationDesignInfo(base.App.AssetDatabase, base.App.GameDatabase, fleetInfo2.PlayerID, stationInfo.DesignInfo.StationType, stationInfo.DesignInfo.StationLevel + 1, false);
								ConstMissionsDue.Add(current7.ID);
								base.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", current7.ID, "");
								string itemGlobalID4 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", current7.ID, "");
								base.App.UI.SetText(base.App.UI.Path(new string[]
								{
									itemGlobalID4,
									"designName"
								}), "Upgrade " + stationInfo.DesignInfo.Name);
								base.App.UI.SetTooltip(base.App.UI.Path(new string[]
								{
									itemGlobalID4,
									"header_idle"
								}), designInfo3.SavingsCost.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current7.TargetSystemID).Name);
								base.App.UI.SetTooltip(base.App.UI.Path(new string[]
								{
									itemGlobalID4,
									"header_sel"
								}), designInfo3.SavingsCost.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current7.TargetSystemID).Name);
								base.App.UI.SetVisible(base.App.UI.Path(new string[]
								{
									itemGlobalID4,
									"designDeleteButton"
								}), false);
								base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
								{
									itemGlobalID4,
									"designName"
								}), "color", 50f, 202f, 240f);
							}
						}
					}
				}
				foreach (StationInfo current8 in base.App.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID))
				{
					List<DesignModuleInfo> queuedModules = base.App.GameDatabase.GetQueuedStationModules(current8.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>();
					if (queuedModules.Count > 0)
					{
						LogicalModule logicalModule = base.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == this.App.GameDatabase.GetModuleAsset(queuedModules.First<DesignModuleInfo>().ModuleID));
						if (logicalModule != null)
						{
							StationModules.StationModule stationModule = (
								from x in StationModules.Modules
								where x.SMType == queuedModules.First<DesignModuleInfo>().StationModuleType
								select x).First<StationModules.StationModule>();
							ConstModulesDue.Add(queuedModules.First<DesignModuleInfo>().ID);
							base.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", 100000 + queuedModules.First<DesignModuleInfo>().ID, "");
							string itemGlobalID5 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", 100000 + queuedModules.First<DesignModuleInfo>().ID, "");
							base.App.UI.SetText(base.App.UI.Path(new string[]
							{
								itemGlobalID5,
								"designName"
							}), current8.DesignInfo.Name + " - " + stationModule.Name);
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID5,
								"header_idle"
							}), logicalModule.SavingsCost.ToString("N0"));
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID5,
								"header_sel"
							}), logicalModule.SavingsCost.ToString("N0"));
							base.App.UI.SetVisible(base.App.UI.Path(new string[]
							{
								itemGlobalID5,
								"designDeleteButton"
							}), false);
							base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
							{
								itemGlobalID5,
								"designName"
							}), "color", 50f, 202f, 240f);
						}
					}
				}
				foreach (MissionInfo current9 in 
					from x in base.App.GameDatabase.GetMissionInfos()
					where x.Type == MissionType.CONSTRUCT_STN && !ConstMissionsDue.Contains(x.ID)
					select x)
				{
					FleetInfo fleetInfo3 = base.App.GameDatabase.GetFleetInfo(current9.FleetID);
					if (fleetInfo3.PlayerID == playerInfo.ID)
					{
                        MissionEstimate missionEstimate3 = Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(base.App.Game, current9.Type, (StationType)current9.StationType.Value, fleetInfo3.ID, current9.TargetSystemID, current9.TargetOrbitalObjectID, null, 1, false, null, null);
						base.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", current9.ID, "");
						string itemGlobalID6 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", current9.ID, "");
						base.App.UI.SetText(base.App.UI.Path(new string[]
						{
							itemGlobalID6,
							"designName"
						}), "Build " + ((StationType)current9.StationType.Value).ToDisplayText(base.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name));
						base.App.UI.SetTooltip(base.App.UI.Path(new string[]
						{
							itemGlobalID6,
							"header_idle"
						}), missionEstimate3.ConstructionCost.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current9.TargetSystemID).Name);
						base.App.UI.SetTooltip(base.App.UI.Path(new string[]
						{
							itemGlobalID6,
							"header_sel"
						}), missionEstimate3.ConstructionCost.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current9.TargetSystemID).Name);
						base.App.UI.SetVisible(base.App.UI.Path(new string[]
						{
							itemGlobalID6,
							"designDeleteButton"
						}), false);
						base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
						{
							itemGlobalID6,
							"designName"
						}), "color", 250f, 170f, 50f);
					}
				}
				foreach (MissionInfo current10 in 
					from x in base.App.GameDatabase.GetMissionInfos()
					where x.Type == MissionType.UPGRADE_STN && !ConstMissionsDue.Contains(x.ID)
					select x)
				{
					FleetInfo fleetInfo4 = base.App.GameDatabase.GetFleetInfo(current10.FleetID);
					if (base.App.GameDatabase.GetStationInfo(current10.TargetOrbitalObjectID) != null && fleetInfo4.PlayerID == playerInfo.ID && current10.Duration > 0)
					{
						StationInfo stationInfo2 = base.App.GameDatabase.GetStationInfo(current10.TargetOrbitalObjectID);
						if (stationInfo2.DesignInfo.StationLevel + 1 <= 5)
						{
                            Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(base.App.Game, current10.Type, stationInfo2.DesignInfo.StationType, fleetInfo4.ID, current10.TargetSystemID, current10.TargetOrbitalObjectID, null, stationInfo2.DesignInfo.StationLevel + 1, false, null, null);
							DesignInfo designInfo4 = DesignLab.CreateStationDesignInfo(base.App.AssetDatabase, base.App.GameDatabase, fleetInfo4.PlayerID, stationInfo2.DesignInfo.StationType, stationInfo2.DesignInfo.StationLevel + 1, false);
							base.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", current10.ID, "");
							string itemGlobalID7 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", current10.ID, "");
							base.App.UI.SetText(base.App.UI.Path(new string[]
							{
								itemGlobalID7,
								"designName"
							}), "Upgrade " + stationInfo2.DesignInfo.Name);
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID7,
								"header_idle"
							}), designInfo4.SavingsCost.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current10.TargetSystemID).Name);
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID7,
								"header_sel"
							}), designInfo4.SavingsCost.ToString("N0") + " | " + base.App.GameDatabase.GetStarSystemInfo(current10.TargetSystemID).Name);
							base.App.UI.SetVisible(base.App.UI.Path(new string[]
							{
								itemGlobalID7,
								"designDeleteButton"
							}), false);
							base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
							{
								itemGlobalID7,
								"designName"
							}), "color", 250f, 170f, 50f);
						}
					}
				}
				foreach (StationInfo current11 in base.App.GameDatabase.GetStationInfosByPlayerID(playerInfo.ID))
				{
					List<DesignModuleInfo> list5 = (
						from x in base.App.GameDatabase.GetQueuedStationModules(current11.DesignInfo.DesignSections[0])
						where !ConstModulesDue.Contains(x.ID)
						select x).ToList<DesignModuleInfo>();
					foreach (DesignModuleInfo smod in list5)
					{
						LogicalModule logicalModule2 = base.App.AssetDatabase.Modules.First((LogicalModule x) => x.ModulePath == this.App.GameDatabase.GetModuleAsset(smod.ModuleID));
						if (logicalModule2 != null)
						{
							StationModules.StationModule stationModule2 = (
								from x in StationModules.Modules
								where x.SMType == smod.StationModuleType
								select x).First<StationModules.StationModule>();
							base.App.UI.AddItem(EmpireSummaryState.UIStationInvoicesList, "", 100000 + smod.ID, "");
							string itemGlobalID8 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIStationInvoicesList, "", 100000 + smod.ID, "");
							base.App.UI.SetText(base.App.UI.Path(new string[]
							{
								itemGlobalID8,
								"designName"
							}), current11.DesignInfo.Name + " - " + stationModule2.Name);
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID8,
								"header_idle"
							}), logicalModule2.SavingsCost.ToString("N0"));
							base.App.UI.SetTooltip(base.App.UI.Path(new string[]
							{
								itemGlobalID8,
								"header_sel"
							}), logicalModule2.SavingsCost.ToString("N0"));
							base.App.UI.SetVisible(base.App.UI.Path(new string[]
							{
								itemGlobalID8,
								"designDeleteButton"
							}), false);
							base.App.UI.SetPropertyColor(base.App.UI.Path(new string[]
							{
								itemGlobalID8,
								"designName"
							}), "color", 250f, 170f, 50f);
						}
					}
				}
				List<ColonyInfo> list6 = (
					from x in base.App.GameDatabase.GetColonyInfos()
					where x.PlayerID == base.App.LocalPlayer.ID && Colony.GetColonySupportCost(base.App.AssetDatabase, base.App.GameDatabase, x) > 0.0
					select x).ToList<ColonyInfo>();
				FleetUI.SyncPlanetListControl(base.App.Game, EmpireSummaryState.UIColonyDevList, 
					from x in list6
					select x.OrbitalObjectID);
				foreach (ColonyInfo current12 in list6)
				{
					string itemGlobalID9 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIColonyDevList, "", current12.OrbitalObjectID, "");
					base.App.UI.SetText(base.App.UI.Path(new string[]
					{
						itemGlobalID9,
						"colonycostlbl"
					}), Colony.GetColonySupportCost(base.App.AssetDatabase, base.App.GameDatabase, current12).ToString("N0"));
					StarSystemInfo starSystemInfo = base.App.GameDatabase.GetStarSystemInfo(current12.CachedStarSystemID);
					base.App.UI.SetTooltip(base.App.UI.Path(new string[]
					{
						itemGlobalID9,
						"header_sel"
					}), starSystemInfo.Name);
					base.App.UI.SetTooltip(base.App.UI.Path(new string[]
					{
						itemGlobalID9,
						"header_idle"
					}), starSystemInfo.Name);
				}
				List<ColonyInfo> list7 = (
					from x in base.App.GameDatabase.GetColonyInfos()
					where x.PlayerID == this.App.LocalPlayer.ID && Colony.GetTaxRevenue(this.App, playerInfo, x) >= 1.0
					select x).ToList<ColonyInfo>();
				FleetUI.SyncPlanetListControl(base.App.Game, EmpireSummaryState.UIColonyIncomeList, 
					from x in list7
					select x.OrbitalObjectID);
				foreach (ColonyInfo current13 in list7)
				{
					string itemGlobalID10 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIColonyIncomeList, "", current13.OrbitalObjectID, "");
					base.App.UI.SetText(base.App.UI.Path(new string[]
					{
						itemGlobalID10,
						"colonycostlbl"
					}), Colony.GetTaxRevenue(base.App, playerInfo, current13).ToString("N0"));
					StarSystemInfo starSystemInfo2 = base.App.GameDatabase.GetStarSystemInfo(current13.CachedStarSystemID);
					base.App.UI.SetTooltip(base.App.UI.Path(new string[]
					{
						itemGlobalID10,
						"header_sel"
					}), starSystemInfo2.Name);
					base.App.UI.SetTooltip(base.App.UI.Path(new string[]
					{
						itemGlobalID10,
						"header_idle"
					}), starSystemInfo2.Name);
				}
			}
			List<ColonyInfo> list8 = (
				from x in base.App.GameDatabase.GetColonyInfos()
				where x.PlayerID == this.App.LocalPlayer.ID && Colony.GetTaxRevenue(this.App, playerInfo, x) >= 1.0
				select x).ToList<ColonyInfo>();
			foreach (ColonyInfo current14 in list8)
			{
				string itemGlobalID11 = base.App.UI.GetItemGlobalID(EmpireSummaryState.UIColonyIncomeList, "", current14.OrbitalObjectID, "");
				base.App.UI.SetText(base.App.UI.Path(new string[]
				{
					itemGlobalID11,
					"colonycostlbl"
				}), Colony.GetTaxRevenue(base.App, playerInfo, current14).ToString("N0"));
			}
			EmpireHistoryData lastEmpireHistoryForPlayer = base.App.GameDatabase.GetLastEmpireHistoryForPlayer(base.App.LocalPlayer.ID);
			Vector3 value = new Vector3(255f, 255f, 255f);
			Vector3 value2 = new Vector3(0f, 255f, 0f);
			Vector3 value3 = new Vector3(255f, 0f, 0f);
			if (lastEmpireHistoryForPlayer != null)
			{
				if (numColonies > lastEmpireHistoryForPlayer.colonies)
				{
					base.App.UI.SetPropertyColor("coloniesvalue", "color", value2);
				}
				else
				{
					if (numColonies < lastEmpireHistoryForPlayer.colonies)
					{
						base.App.UI.SetPropertyColor("coloniesvalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("coloniesvalue", "color", value);
					}
				}
				if (numProvinces > lastEmpireHistoryForPlayer.provinces)
				{
					base.App.UI.SetPropertyColor("provincesvalue", "color", value2);
				}
				else
				{
					if (numProvinces < lastEmpireHistoryForPlayer.provinces)
					{
						base.App.UI.SetPropertyColor("provincesvalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("provincesvalue", "color", value);
					}
				}
				if (num > lastEmpireHistoryForPlayer.bases)
				{
					base.App.UI.SetPropertyColor("basesvalue", "color", value2);
				}
				else
				{
					if (num < lastEmpireHistoryForPlayer.bases)
					{
						base.App.UI.SetPropertyColor("basesvalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("basesvalue", "color", value);
					}
				}
				if (num2 > lastEmpireHistoryForPlayer.fleets)
				{
					base.App.UI.SetPropertyColor("fleetsvalue", "color", value2);
				}
				else
				{
					if (num2 < lastEmpireHistoryForPlayer.fleets)
					{
						base.App.UI.SetPropertyColor("fleetsvalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("fleetsvalue", "color", value);
					}
				}
				if (numShips > lastEmpireHistoryForPlayer.ships)
				{
					base.App.UI.SetPropertyColor("shipsvalue", "color", value2);
				}
				else
				{
					if (numShips < lastEmpireHistoryForPlayer.ships)
					{
						base.App.UI.SetPropertyColor("shipsvalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("shipsvalue", "color", value);
					}
				}
				if (empirePopulation > lastEmpireHistoryForPlayer.empire_pop)
				{
					base.App.UI.SetPropertyColor("populationvalue", "color", value2);
				}
				else
				{
					if (empirePopulation < lastEmpireHistoryForPlayer.empire_pop)
					{
						base.App.UI.SetPropertyColor("populationvalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("populationvalue", "color", value);
					}
				}
				if (empireEconomy > lastEmpireHistoryForPlayer.empire_economy)
				{
					base.App.UI.SetPropertyColor("economyvalue", "color", value2);
				}
				else
				{
					if (empireEconomy < lastEmpireHistoryForPlayer.empire_economy)
					{
						base.App.UI.SetPropertyColor("economyvalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("economyvalue", "color", value);
					}
				}
				if (empireBiosphere > lastEmpireHistoryForPlayer.empire_biosphere)
				{
					base.App.UI.SetPropertyColor("biospherevalue", "color", value2);
				}
				else
				{
					if (empireBiosphere < lastEmpireHistoryForPlayer.empire_biosphere)
					{
						base.App.UI.SetPropertyColor("biospherevalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("biospherevalue", "color", value);
					}
				}
				if (tradeRevenue > lastEmpireHistoryForPlayer.empire_trade)
				{
					base.App.UI.SetPropertyColor("tradeunitsvalue", "color", value2);
				}
				else
				{
					if (tradeRevenue < lastEmpireHistoryForPlayer.empire_trade)
					{
						base.App.UI.SetPropertyColor("tradeunitsvalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("tradeunitsvalue", "color", value);
					}
				}
				if (empireMorale > lastEmpireHistoryForPlayer.empire_morale)
				{
					base.App.UI.SetPropertyColor("averagemoralevalue", "color", value2);
				}
				else
				{
					if (empireMorale < lastEmpireHistoryForPlayer.empire_morale)
					{
						base.App.UI.SetPropertyColor("averagemoralevalue", "color", value3);
					}
					else
					{
						base.App.UI.SetPropertyColor("averagemoralevalue", "color", value);
					}
				}
			}
			this._piechart.SetSlices(budget);
		}
		protected override void OnUpdate()
		{
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}
		private void UpdateTechCubeToolTip()
		{
			string str = App.Localize("@UI_RESEARCH_RESEARCHING");
			bool flag = true;
			int num = base.App.GameDatabase.GetPlayerResearchingTechID(base.App.LocalPlayer.ID);
			if (num == 0)
			{
				num = base.App.GameDatabase.GetPlayerFeasibilityStudyTechId(base.App.LocalPlayer.ID);
				str = App.Localize("@UI_RESEARCH_STUDYING");
				flag = false;
			}
			string techIdStr = base.App.GameDatabase.GetTechFileID(num);
			PlayerTechInfo playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, num);
			Tech tech = base.App.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Tech x) => x.Id == techIdStr);
			if (tech != null && playerTechInfo != null)
			{
				string str2 = "";
				if (flag)
				{
					str2 = " -  " + ResearchScreenState.GetTurnsToCompleteString(base.App, base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID), playerTechInfo);
				}
				base.App.UI.SetTooltip("researchCubeButton", str + " " + tech.Name + str2);
				return;
			}
			base.App.UI.SetTooltip("researchCubeButton", App.Localize("@UI_TOOLTIP_RESEARCHCUBE"));
		}
		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(base.Name))
			{
				switch (action)
				{
				case HotKeyManager.HotKeyActions.State_Starmap:
					base.App.UI.LockUI();
					base.App.SwitchGameState<StarMapState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_BuildScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_DesignScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<DesignScreenState>(new object[]
					{
						false,
						base.Name
					});
					return true;
				case HotKeyManager.HotKeyActions.State_ResearchScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<ResearchScreenState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_SotspediaScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<SotspediaState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<DiplomacyScreenState>(new object[0]);
					return true;
				}
			}
			return false;
		}
	}
}
