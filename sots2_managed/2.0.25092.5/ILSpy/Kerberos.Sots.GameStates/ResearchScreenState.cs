using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class ResearchScreenState : GameState, IKeyBindListener
	{
		private class TechBranch
		{
			public float Feasibility = 1f;
			public Allowable Allows;
		}
		private const string UIFeasibilityDetails = "feasibility_details";
		private const string UIFeasibilityTechName = "feasibility_tech_name";
		private const string UIFeasibilityTechIcon = "feasibility_tech_icon";
		private const string UIResearchingDetails = "researching_details";
		private const string UIResearchingTechName = "researching_tech_name";
		private const string UIResearchingTechIcon = "researching_tech_icon";
		private const string UIResearchingTechProgress = "researching_progress";
		private const string UISelectResearchingTechButton = "select_researching_tech";
		private const string UISelectFeasibilityTechButton = "select_feasibility_tech";
		private const string UISelectedTechName = "selected_tech_name";
		private const string UISelectedTechStatus = "selected_tech_status";
		private const string UISelectedTechIcon = "selected_tech_icon";
		private const string UISelectedTechFamilyName = "selected_tech_family_name";
		private const string UISelectedTechFamilyIcon = "discipline_icon";
		private const string UISelectedTechDesc = "selected_tech_desc";
		private const string UICancelResearchButton = "cancel_research_button";
		private const string UICancelFeasibilityButton = "cancel_feasibility_button";
		private const string UIStartResearchButton = "start_research_button";
		private const string UIStartFeasibilityButton = "start_feasibility_button";
		private const string UIStartResearchPanel = "start_research";
		private const string UIStartFeasibilityPanel = "start_feasibility";
		private const string UIResearchSlider = "research_slider";
		private const string UIBoostResearchButton = "boost_research_button";
		private const string UIBoostResearchPanel = "BoostResearchPanel";
		private const string UIBoostValue = "boost_value";
		private const string UIBoostSlider = "boost_slider";
		private const string UIProjectList = "research_projects";
		private const string UIStartProjectButton = "start_project_button";
		private const string UICancelProjectButton = "cancel_project_button";
		private const string UIModuleCountLabel = "moduleCountLabel";
		private const string UIModuleBonusLabel = "moduleBonusLabel";
		private const string UISelectedTechDetailsButton = "selectedTechDetailsButton";
		private const string UISelectedTechInfoButton = "selectedTechInfoButton";
		private const string UIFamilyList = "familyList";
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private ResearchState _research;
		private bool _showDebugControls;
		private BudgetPiechart _piechart;
		private WeaponInfoPanel _weaponInfoPanel;
		private static readonly string UICurrentProjectSlider = "currentProjectSliderR";
		private static readonly string UISpecialProjectSlider = "specialProjectSliderR";
		private static readonly string UISalvageResearchSlider = "salvageResearchSliderR";
		private bool _inState;
		private bool _selectedTechDetailsVisible;
		private int _leftButton;
		private bool _enteredButton;
		private bool _showBoostDialog;
		private int _selectedProject;
		private string _selectedTech = string.Empty;
		private Dictionary<string, int> FamilyIndex = new Dictionary<string, int>();
		private Random _rand = new Random();
		private bool _budgetChanged;
		private int SelectedProject
		{
			get
			{
				return this._selectedProject;
			}
		}
		private string SelectedTech
		{
			get
			{
				return this._selectedTech;
			}
		}
		private static void AddTechTreeBranches(TechTree tree, Kerberos.Sots.Data.TechnologyFramework.Tech tech, Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>> acquired, string faction, Random rng, bool forceBranches)
		{
			if (!acquired.ContainsKey(tech))
			{
				acquired.Add(tech, new Dictionary<Allowable, ResearchScreenState.TechBranch>());
			}
			foreach (Allowable current in tech.Allows)
			{
				if (!acquired[tech].ContainsKey(current))
				{
					float num = current.GetFactionProbabilityPercentage(faction) / 100f;
					if (num != 0f)
					{
						bool flag = forceBranches || rng.CoinToss((double)num);
						acquired[tech][current] = new ResearchScreenState.TechBranch
						{
							Feasibility = flag ? num : 0f,
							Allows = current
						};
						ResearchScreenState.AddTechTreeBranches(tree, tree[current.Id], acquired, faction, rng, forceBranches);
					}
				}
			}
		}
		public static void BuildPlayerTechTree(App game, AssetDatabase assetdb, GameDatabase gamedb, int playerId)
		{
			List<Kerberos.Sots.Data.ScenarioFramework.Tech> list = new List<Kerberos.Sots.Data.ScenarioFramework.Tech>();
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech current in assetdb.MasterTechTree.Technologies)
			{
				list.Add(new Kerberos.Sots.Data.ScenarioFramework.Tech
				{
					Name = current.Id
				});
			}
			ResearchScreenState.BuildPlayerTechTree(game, assetdb, gamedb, playerId, list);
		}
		private static void BuildFactionTechTree(AssetDatabase assetdb, GameDatabase gamedb, string faction, out Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>> acquired)
		{
			Random rng = new Random();
			acquired = new Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>>();
			Kerberos.Sots.Data.TechnologyFramework.Tech[] masterTechTreeRoots = assetdb.MasterTechTreeRoots;
			for (int i = 0; i < masterTechTreeRoots.Length; i++)
			{
				Kerberos.Sots.Data.TechnologyFramework.Tech tech = masterTechTreeRoots[i];
				ResearchScreenState.AddTechTreeBranches(assetdb.MasterTechTree, tech, acquired, faction, rng, true);
			}
		}
		public static void BuildPlayerTechTree(App game, AssetDatabase assetdb, GameDatabase gamedb, int playerId, List<Kerberos.Sots.Data.ScenarioFramework.Tech> AvailableList)
		{
			Random rng = new Random();
			string factionName = gamedb.GetFactionName(gamedb.GetPlayerFactionID(playerId));
			TechTree masterTechTree = assetdb.MasterTechTree;
			Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>> dictionary = new Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>>();
			Kerberos.Sots.Data.TechnologyFramework.Tech[] masterTechTreeRoots = assetdb.MasterTechTreeRoots;
			for (int i = 0; i < masterTechTreeRoots.Length; i++)
			{
				Kerberos.Sots.Data.TechnologyFramework.Tech tech = masterTechTreeRoots[i];
				ResearchScreenState.AddTechTreeBranches(masterTechTree, tech, dictionary, factionName, rng, false);
			}
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech current in dictionary.Keys)
			{
				gamedb.InsertPlayerTech(playerId, current.Id, TechStates.Locked, 0.0, 0.0, null);
				foreach (ResearchScreenState.TechBranch current2 in dictionary[current].Values)
				{
					gamedb.InsertPlayerTechBranch(playerId, gamedb.GetTechID(current.Id), gamedb.GetTechID(current2.Allows.Id), current2.Allows.ResearchPoints, current2.Feasibility);
				}
			}
			gamedb.UpdateLockedTechs(assetdb, playerId);
		}
		public static void AcquireAllTechs(GameSession game, int playerId)
		{
			IEnumerable<PlayerTechInfo> playerTechInfos = game.GameDatabase.GetPlayerTechInfos(playerId);
			foreach (PlayerTechInfo current in playerTechInfos)
			{
				if (current.State != TechStates.Researched)
				{
					game.GameDatabase.UpdatePlayerTechState(playerId, current.TechID, TechStates.Researched);
					App.UpdateStratModifiers(game, playerId, current.TechID);
				}
			}
		}
		public ResearchScreenState(App game) : base(game)
		{
		}
		private static List<string> GetTechTreeModels(AssetDatabase assetdb, Faction faction)
		{
			List<string> list = new List<string>();
			list.AddRange(assetdb.TechTreeModels);
			if (faction != null)
			{
				list.AddRange(faction.TechTreeModels);
			}
			else
			{
				foreach (Faction current in assetdb.Factions)
				{
					list.AddRange(current.TechTreeModels);
				}
			}
			return list;
		}
		private static List<string> GetTechTreeRoots(AssetDatabase assetdb, Faction faction)
		{
			List<string> list = new List<string>();
			list.AddRange(assetdb.TechTreeRoots);
			if (faction != null)
			{
				list.AddRange(faction.TechTreeRoots);
			}
			else
			{
				foreach (Faction current in assetdb.Factions)
				{
					list.AddRange(current.TechTreeRoots);
				}
			}
			return list;
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame();
				this._showDebugControls = ScriptHost.AllowConsole;
			}
			List<string> techTreeModels = ResearchScreenState.GetTechTreeModels(base.App.AssetDatabase, base.App.LocalPlayer.Faction);
			this._crits = new GameObjectSet(base.App);
			this._camera = this._crits.Add<OrbitCameraController>(new object[0]);
			this._research = this._crits.Add<ResearchState>(new object[]
			{
				techTreeModels.Count
			}.Concat(techTreeModels.Cast<object>()).ToArray<object>());
			this._research.PostSetProp("CameraController", this._camera.ObjectID);
			this._camera.MinDistance = 15f;
			this._camera.DesiredDistance = 80f;
			this._camera.MaxDistance = 600f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this._camera.DesiredPitch = -MathHelper.DegreesToRadians(45f);
			this.SyncTechTree(base.App.LocalPlayer.ID);
			base.App.UI.LoadScreen("Research");
		}
		public void ShowDebugControls()
		{
			this._showDebugControls = true;
			if (this._inState)
			{
				base.App.UI.SetVisible("debugControls", true);
			}
		}
		protected override void OnEnter()
		{
			base.App.UI.UnlockUI();
			base.App.UI.SetScreen("Research");
			base.App.UI.SetPropertyBool("gameExitButton", "lockout_button", true);
			base.App.UI.SetVisible("selectedTechDetailsButton", false);
			base.App.UI.SetVisible("selectedTechInfoButton", false);
			base.App.UI.SetPropertyBool("familyList", "only_user_events", true);
			this.PopulateFamilyList();
			this._piechart = new BudgetPiechart(base.App.UI, "piechart", base.App.AssetDatabase);
			base.App.UI.Send(new object[]
			{
				"SetGameObject",
				"Research",
				this._research.ObjectID
			});
			base.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			if (this._showDebugControls)
			{
				base.App.UI.SetVisible("debugControls", true);
			}
			this._crits.Activate();
			EmpireBarUI.SyncTitleFrame(base.App);
			EmpireBarUI.SyncResearchSlider(base.App, "research_slider", this._piechart);
			this.RefreshResearchButton();
			this.RefreshResearchingTech();
			this.RefreshFeasibilityStudy();
			this.RefreshProjectList();
			this._weaponInfoPanel = new WeaponInfoPanel(base.App.UI, "selectedTechWeaponDetails");
			this.SetSelectedTechDetailsVisible(false);
			this.HideSelectedTechDetails();
			PlayerTechInfo[] source = base.App.GameDatabase.GetPlayerTechInfos(base.App.LocalPlayer.ID).ToArray<PlayerTechInfo>();
			PlayerTechInfo playerTechInfo = source.FirstOrDefault((PlayerTechInfo x) => (x.State == TechStates.Researching || x.State == TechStates.PendingFeasibility) && !x.TechFileID.StartsWith("PSI") && !x.TechFileID.StartsWith("CYB"));
			if (playerTechInfo == null)
			{
				playerTechInfo = source.FirstOrDefault((PlayerTechInfo x) => x.State == TechStates.HighFeasibility && !x.TechFileID.StartsWith("PSI") && !x.TechFileID.StartsWith("CYB"));
			}
			if (playerTechInfo == null)
			{
				playerTechInfo = source.FirstOrDefault((PlayerTechInfo x) => x.State == TechStates.Core && !x.TechFileID.StartsWith("PSI") && !x.TechFileID.StartsWith("CYB"));
			}
			if (playerTechInfo == null)
			{
				playerTechInfo = source.FirstOrDefault((PlayerTechInfo x) => x.State == TechStates.Researched && !x.TechFileID.StartsWith("PSI") && !x.TechFileID.StartsWith("CYB"));
			}
			if (playerTechInfo == null)
			{
				playerTechInfo = source.First((PlayerTechInfo x) => !x.TechFileID.StartsWith("PSI") && !x.TechFileID.StartsWith("CYB"));
			}
			if (playerTechInfo != null)
			{
				this._research.PostSetProp("Select", playerTechInfo.TechFileID);
			}
			if (base.App.GameSettings.AudioEnabled)
			{
				base.App.PostEnableAllSounds();
			}
			FeasibilityStudyInfo feasibilityStudyInfoByPlayer = base.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(base.App.Game.LocalPlayer.ID);
			if (feasibilityStudyInfoByPlayer != null)
			{
				base.App.PostRequestGuiSound("research_feasibilitystudyloop");
			}
			int playerResearchingTechID = base.App.GameDatabase.GetPlayerResearchingTechID(base.App.Game.LocalPlayer.ID);
			if (playerResearchingTechID != 0)
			{
				base.App.PostRequestGuiSound("research_researchingloop");
			}
			this._research.PostSetProp("SetTechFloater", "techFloater");
			this._inState = true;
			this.UpdateResearchSliders(base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID), "");
			PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
			float ratio = ((float)(playerInfo.ResearchBoostFunds / Math.Max(0.0, playerInfo.Savings))).Clamp(0f, 1f);
			this.SetBoost(ratio, false);
			base.App.HotKeyManager.AddListener(this);
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
			this._inState = false;
			this.SetSelectedTechDetailsVisible(false);
			this._piechart = null;
			base.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._crits.Dispose();
			this._crits = null;
		}
		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			this._piechart.TryGameEvent(eventName, eventParams);
			if (eventName == "TechClicked")
			{
				this.ProcessGameEvent_TechClicked(eventParams);
				return;
			}
			if (eventName == "TechMouseOver")
			{
				base.App.PostRequestGuiSound("research_mouseover");
			}
		}
		private void ProcessGameEvent_TechClicked(string[] eventParams)
		{
			this.SetSelectedTech(eventParams[0], "TechClicked");
			base.App.PostRequestGuiSound(this.GetSelectedTechFamilySound(eventParams[0].Substring(0, 3)));
		}
		private string GetSelectedTechFamilySound(string familyId)
		{
			return "research_selectballistic";
		}
		private void DoResearchWhooshAnimation()
		{
			base.App.UI.SetVisible("research_mask", false);
			base.App.UI.SetVisible("research_mask", true);
		}
		private void ShowSelectedTechDetails()
		{
			if (string.IsNullOrEmpty(this._selectedTech))
			{
				return;
			}
			base.App.UI.SetVisible("weaponFader", true);
		}
		private void HideSelectedTechDetails()
		{
			base.App.UI.SetVisible("weaponFader", false);
		}
		private void SetSelectedTechDetailsVisible(bool value)
		{
			if (this._selectedTechDetailsVisible == value)
			{
				return;
			}
			this._selectedTechDetailsVisible = value;
			if (this._selectedTechDetailsVisible)
			{
				this.ShowSelectedTechDetails();
				return;
			}
			this.HideSelectedTechDetails();
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
			{
				return;
			}
			if (msgType == "mouse_enter")
			{
				if (panelName.Contains("techButton"))
				{
					string[] array = panelName.Split(new char[]
					{
						'|'
					});
					int id = int.Parse(array[1]);
					this.SetSelectedFamily(this.FamilyIndex.FirstOrDefault((KeyValuePair<string, int> x) => x.Value == id).Key);
					this._enteredButton = true;
					return;
				}
			}
			else
			{
				if (msgType == "mouse_leave")
				{
					this._leftButton = 3;
					return;
				}
				if (msgType == "list_sel_changed")
				{
					if (panelName == "familyList")
					{
						int index = int.Parse(msgParams[0]);
						string key = this.FamilyIndex.FirstOrDefault((KeyValuePair<string, int> x) => x.Value == index).Key;
						this._research.PostSetProp("SelectFamily", key);
						return;
					}
				}
				else
				{
					if (msgType == "button_clicked")
					{
						if (panelName == "gameExitButton")
						{
							base.App.SwitchGameState<StarMapState>(new object[0]);
						}
						else
						{
							if (panelName == "gameTutorialButton")
							{
								base.App.UI.SetVisible("ResearchScreenTutorial", true);
							}
							else
							{
								if (panelName == "researchScreenTutImage")
								{
									base.App.UI.SetVisible("ResearchScreenTutorial", false);
								}
								else
								{
									if (panelName == "selectedTechDetailsButton")
									{
										this.SetSelectedTechDetailsVisible(!this._selectedTechDetailsVisible);
									}
									else
									{
										if (panelName == "selectedTechInfoButton")
										{
											if (this.SelectedTech != null)
											{
												SotspediaState.NavigateToLink(base.App, "#" + this.SelectedTech);
											}
										}
										else
										{
											if (panelName == "gameSalvageProjectsButton")
											{
												base.App.UI.CreateDialog(new SalvageProjectDialog(base.App, "dialogSpecialProjects"), null);
											}
											else
											{
												if (panelName == "gameSpecialProjectsButton")
												{
													base.App.UI.CreateDialog(new SpecialProjectDialog(base.App, "dialogSpecialProjects"), null);
												}
												else
												{
													if (panelName == "start_research_button")
													{
														this.StartResearch(this.SelectedTech);
													}
													else
													{
														if (panelName == "cancel_research_button")
														{
															this.CancelResearch();
														}
														else
														{
															if (panelName == "boost_research_button")
															{
																this._showBoostDialog = !this._showBoostDialog;
																base.App.UI.SetVisible("BoostResearchPanel", this._showBoostDialog);
															}
															else
															{
																if (panelName == "cancel_feasibility_button")
																{
																	this.CancelFeasibility();
																}
																else
																{
																	if (panelName == "select_researching_tech")
																	{
																		this.SelectResearchingTech();
																	}
																	else
																	{
																		if (panelName == "select_feasibility_tech")
																		{
																			this.SelectFeasibilityTech();
																		}
																		else
																		{
																			if (panelName == "start_feasibility_button")
																			{
																				this.StartFeasibilityStudy(this.SelectedTech);
																			}
																			else
																			{
																				if (panelName == "start_project_button")
																				{
																					this.StartProject(this.SelectedProject);
																				}
																				else
																				{
																					if (panelName == "cancel_project_button")
																					{
																						this.CancelProject(this.SelectedProject);
																					}
																					else
																					{
																						if (panelName == "prevTechFamily")
																						{
																							this._research.NextTechFamily();
																						}
																						else
																						{
																							if (panelName == "nextTechFamily")
																							{
																								this._research.PrevTechFamily();
																							}
																							else
																							{
																								if (panelName == "game_budget_pie")
																								{
																									base.App.UI.LockUI();
																									base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
																								}
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
						if (this._showDebugControls)
						{
							if (panelName == "debugShowNormal")
							{
								this.SyncTechTree(base.App.LocalPlayer.ID);
								return;
							}
							if (panelName == "debugShowAll")
							{
								this.DebugShowAllTechs();
								return;
							}
							if (panelName == "debugShowHuman")
							{
								this.SyncTechTree("human");
								return;
							}
							if (panelName == "debugShowHiver")
							{
								this.SyncTechTree("hiver");
								return;
							}
							if (panelName == "debugShowTarkas")
							{
								this.SyncTechTree("tarkas");
								return;
							}
							if (panelName == "debugShowLiir")
							{
								this.SyncTechTree("liir_zuul");
								return;
							}
							if (panelName == "debugShowZuul")
							{
								this.SyncTechTree("zuul");
								return;
							}
							if (panelName == "debugShowMorrigi")
							{
								this.SyncTechTree("morrigi");
								return;
							}
							if (panelName == "debugShowLoa")
							{
								this.SyncTechTree("loa");
								return;
							}
						}
					}
					else
					{
						if (msgType == "slider_value")
						{
							PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
							if (panelName == "research_slider")
							{
								StarMapState.SetEmpireResearchRate(base.App.Game, msgParams[0], this._piechart);
								this._budgetChanged = true;
								return;
							}
							if (panelName == ResearchScreenState.UICurrentProjectSlider)
							{
								EmpireSummaryState.DistibuteResearchSpending(base.App.Game, base.App.GameDatabase, EmpireSummaryState.ResearchSpendings.CurrentProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this._budgetChanged = true;
								this.UpdateResearchSliders(playerInfo, panelName);
								return;
							}
							if (panelName == ResearchScreenState.UISpecialProjectSlider)
							{
								EmpireSummaryState.DistibuteResearchSpending(base.App.Game, base.App.GameDatabase, EmpireSummaryState.ResearchSpendings.SpecialProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this._budgetChanged = true;
								this.UpdateResearchSliders(playerInfo, panelName);
								return;
							}
							if (panelName == ResearchScreenState.UISalvageResearchSlider)
							{
								EmpireSummaryState.DistibuteResearchSpending(base.App.Game, base.App.GameDatabase, EmpireSummaryState.ResearchSpendings.SalvageResearch, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
								this._budgetChanged = true;
								this.UpdateResearchSliders(playerInfo, panelName);
								return;
							}
							if (panelName == "boost_slider")
							{
								float ratio = (float)int.Parse(msgParams[0]) / 100f;
								this.SetBoost(ratio, true);
								return;
							}
						}
						else
						{
							if (msgType == "list_sel_changed" && panelName == "research_projects")
							{
								int projectId = 0;
								base.App.UI.ParseListItemId(msgParams[0], out projectId);
								this.SetSelectedProject(projectId, "research_projects");
							}
						}
					}
				}
			}
		}
		private void SetBoost(float ratio, bool setboostvalue)
		{
			PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
			if (setboostvalue)
			{
				playerInfo.ResearchBoostFunds = Math.Max(0.0, playerInfo.Savings) * (double)ratio;
				base.App.GameDatabase.UpdatePlayerResearchBoost(playerInfo.ID, playerInfo.ResearchBoostFunds);
			}
			else
			{
				base.App.UI.SetSliderValue("boost_slider", (int)(100f * ratio));
			}
			base.App.UI.SetText("boost_value", playerInfo.ResearchBoostFunds.ToString("N0"));
			float num = 1f - ratio;
			base.App.UI.SetPropertyColor("boost_value", "color", 255f, 255f * num, 255f * num);
			base.App.UI.SetPropertyColor("boost_title", "color", 255f, 255f * num, 255f * num);
			this.RefreshResearchingTech();
		}
		private void SelectResearchingTech()
		{
			int playerResearchingTechID = base.App.GameDatabase.GetPlayerResearchingTechID(base.App.LocalPlayer.ID);
			string techFileID = base.App.GameDatabase.GetTechFileID(playerResearchingTechID);
			this.SetSelectedTech(techFileID, string.Empty);
		}
		private void SelectFeasibilityTech()
		{
			FeasibilityStudyInfo feasibilityStudyInfoByPlayer = base.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(base.App.LocalPlayer.ID);
			if (feasibilityStudyInfoByPlayer == null)
			{
				return;
			}
			string techFileID = base.App.GameDatabase.GetTechFileID(feasibilityStudyInfoByPlayer.TechID);
			this.SetSelectedTech(techFileID, string.Empty);
		}
		private void RefreshProjectList()
		{
			IEnumerable<ResearchProjectInfo> researchProjectInfos = base.App.GameDatabase.GetResearchProjectInfos(base.App.LocalPlayer.ID);
			FeasibilityStudyInfo feasibilityStudyInfoByPlayer = base.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(base.App.LocalPlayer.ID);
			base.App.UI.ClearItems("research_projects");
			foreach (ResearchProjectInfo current in researchProjectInfos)
			{
				if (feasibilityStudyInfoByPlayer == null || feasibilityStudyInfoByPlayer.ProjectID != current.ID)
				{
					string text = current.Name;
					switch (current.State)
					{
					case ProjectStates.InProgress:
						text = text + " (" + App.Localize("@UI_RESEARCH_FEASIBILITY_IN_PROGRESS") + ")";
						break;
					case ProjectStates.Paused:
						text = text + " (" + App.Localize("@UI_RESEARCH_FEASIBILITY_PAUSED") + ")";
						break;
					}
					base.App.UI.AddItem("research_projects", string.Empty, current.ID, text);
				}
			}
			base.App.UI.SetEnabled("start_project_button", false);
			base.App.UI.SetEnabled("cancel_project_button", false);
		}
		public static int? GetTurnsToCompleteResearch(App App, PlayerInfo player, PlayerTechInfo tech)
		{
			if (tech == null)
			{
				return null;
			}
			if (tech.State == TechStates.Researched)
			{
				return new int?(0);
			}
			string techFamily = tech.TechFileID.Substring(0, 3);
			int num2;
			float num = 1f + App.Game.GetFamilySpecificResearchModifier(player.ID, techFamily, out num2);
			float num3 = (float)App.Game.GetAvailableResearchPoints(player);
			num3 *= App.GameDatabase.GetNameValue<float>("ResearchEfficiency") / 100f * num;
			return GameSession.CalculateTurnsToCompleteResearch(tech.ResearchCost, tech.Progress, (int)num3);
		}
		public static string GetTurnsToCompleteString(App App, PlayerInfo player, PlayerTechInfo tech)
		{
			int? turnsToCompleteResearch = ResearchScreenState.GetTurnsToCompleteResearch(App, player, tech);
			if (turnsToCompleteResearch.HasValue && turnsToCompleteResearch.Value > 0)
			{
				string arg = turnsToCompleteResearch.Value.ToString("N0");
				string format = (turnsToCompleteResearch == 1) ? AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_ONE_TURN") : AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_MANY_TURNS");
				return string.Format(format, arg);
			}
			return string.Empty;
		}
		private void RefreshResearchButton()
		{
			PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
			PlayerTechInfo playerTechInfo = null;
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = null;
			int num = 0;
			if (!string.IsNullOrEmpty(this.SelectedTech))
			{
				num = base.App.GameDatabase.GetTechID(this.SelectedTech);
				playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, num);
				tech = base.App.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == this.SelectedTech);
			}
			bool value = false;
			bool value2 = false;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			string text = string.Empty;
			if (tech != null)
			{
				text = base.App.AssetDatabase.GetLocalizedTechnologyName(tech.Id);
			}
			string text2 = string.Empty;
			if (playerTechInfo != null)
			{
				switch (playerTechInfo.State)
				{
				case TechStates.Core:
					flag2 = true;
					flag3 = true;
					flag = true;
					break;
				case TechStates.Branch:
					value2 = true;
					value = true;
					text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_FEASIBILITY_UNKNOWN");
					break;
				case TechStates.LowFeasibility:
					flag2 = true;
					flag3 = true;
					flag = true;
					text2 = string.Format(AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_LOW_FEASIBILITY_PERCENT"), (int)(playerTechInfo.PlayerFeasibility * 100f));
					break;
				case TechStates.HighFeasibility:
					flag2 = true;
					flag3 = true;
					flag = true;
					text2 = string.Format(AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_HIGH_FEASIBILITY_PERCENT"), (int)(playerTechInfo.PlayerFeasibility * 100f));
					break;
				case TechStates.PendingFeasibility:
					value2 = true;
					value = false;
					text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_FEASIBILITY_PENDING");
					break;
				case TechStates.Researching:
					flag2 = true;
					flag = false;
					text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_RESEARCHING");
					break;
				case TechStates.Researched:
					text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_RESEARCHED");
					break;
				case TechStates.Locked:
					text2 = AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_TECHSTATUS_LOCKED");
					break;
				}
				if (flag2 && flag3)
				{
					string turnsToCompleteString = ResearchScreenState.GetTurnsToCompleteString(base.App, playerInfo, playerTechInfo);
					if (string.IsNullOrEmpty(text2))
					{
						text2 = turnsToCompleteString;
					}
					else
					{
						if (!string.IsNullOrEmpty(turnsToCompleteString))
						{
							text2 = text2 + ", " + turnsToCompleteString;
						}
					}
				}
			}
			int playerResearchingTechID = base.App.GameDatabase.GetPlayerResearchingTechID(base.App.LocalPlayer.ID);
			int playerFeasibilityStudyTechId = base.App.GameDatabase.GetPlayerFeasibilityStudyTechId(base.App.LocalPlayer.ID);
			if (num != 0 && (playerResearchingTechID != 0 || playerFeasibilityStudyTechId != 0))
			{
				value = false;
				flag = false;
			}
			base.App.UI.SetVisible("start_feasibility", value2);
			base.App.UI.SetVisible("start_research", flag2);
			base.App.UI.SetEnabled("start_feasibility_button", value);
			base.App.UI.SetEnabled("start_research_button", flag);
			if (flag)
			{
				text += string.Format(" ( {0}% )", (int)Math.Round((double)(playerTechInfo.PlayerFeasibility * 100f)));
			}
			base.App.UI.SetText("selected_tech_name", text);
			base.App.UI.SetText("selected_tech_status", text2);
		}
		private void RefreshFeasibilityStudy()
		{
			FeasibilityStudyInfo feasibilityStudyInfoByPlayer = base.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(base.App.LocalPlayer.ID);
			if (feasibilityStudyInfoByPlayer == null)
			{
				base.App.UI.SetVisible("feasibility_details", false);
				return;
			}
			int techID = feasibilityStudyInfoByPlayer.TechID;
			string techIdStr = base.App.GameDatabase.GetTechFileID(techID);
			base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, techID);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = base.App.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techIdStr);
			string text = ResearchScreenState.IconTextureToSpriteName(tech.Icon);
			base.App.UI.SetVisible("feasibility_details", true);
			base.App.UI.SetVisible("feasibility_tech_icon", !string.IsNullOrEmpty(text));
			base.App.UI.SetPropertyString("feasibility_tech_name", "text", App.Localize("@UI_RESEARCH_STUDYING") + " " + base.App.AssetDatabase.GetLocalizedTechnologyName(tech.Id));
			base.App.UI.SetPropertyString("feasibility_tech_icon", "sprite", text);
			base.App.UI.AutoSize("feasibility_details");
		}
		private void RefreshResearchingTech()
		{
			int playerResearchingTechID = base.App.GameDatabase.GetPlayerResearchingTechID(base.App.LocalPlayer.ID);
			if (playerResearchingTechID == 0)
			{
				base.App.UI.SetVisible("researching_details", false);
				return;
			}
			PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID);
			string techIdStr = base.App.GameDatabase.GetTechFileID(playerResearchingTechID);
			PlayerTechInfo playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, playerResearchingTechID);
			float num = 1f;
			if (playerTechInfo.ResearchCost > 0)
			{
				num = (float)playerTechInfo.Progress / (float)playerTechInfo.ResearchCost;
			}
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = base.App.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techIdStr);
			string str = string.Format(": {0}% ({1})", (int)Math.Ceiling((double)(num * 100f)), ResearchScreenState.GetTurnsToCompleteString(base.App, playerInfo, playerTechInfo));
			string text = ResearchScreenState.IconTextureToSpriteName(tech.Icon);
			base.App.UI.SetVisible("researching_details", true);
			base.App.UI.SetVisible("researching_tech_icon", !string.IsNullOrEmpty(text));
			base.App.UI.SetPropertyString("researching_tech_name", "text", AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_RESEARCHING") + " " + base.App.AssetDatabase.GetLocalizedTechnologyName(tech.Id));
			base.App.UI.SetPropertyString("researching_progress", "text", AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_PROGRESS") + str);
			base.App.UI.SetPropertyString("researching_tech_icon", "sprite", text);
			base.App.UI.SetSliderValue("researchProgress", (int)Math.Ceiling((double)(num * 100f)));
			base.App.UI.AutoSize("researching_details");
		}
		private static string IconTextureToSpriteName(string texture)
		{
			return Path.GetFileNameWithoutExtension(texture);
		}
		private void SyncTech(Kerberos.Sots.Data.TechnologyFramework.Tech tech, TechStates state, PlayerTechInfo techInf = null)
		{
			TechFamily techFamily = base.App.AssetDatabase.MasterTechTree.TechFamilies.First((TechFamily x) => x.Id == tech.Family);
			string text = string.Empty;
			if (!string.IsNullOrEmpty(tech.Icon))
			{
				text = tech.GetProperIconPath();
			}
			string text2 = string.Empty;
			if (!string.IsNullOrEmpty(techFamily.Icon))
			{
				text2 = techFamily.GetProperIconPath();
			}
			bool flag = base.App.AssetDatabase.MasterTechTreeRoots.Contains(tech);
			int num = (state == TechStates.Researched) ? 1 : 0;
			int num2 = 1;
			if (state != TechStates.Researched && techInf != null)
			{
				num = techInf.Progress;
				num2 = techInf.ResearchCost;
			}
			this._research.PostSetProp("Tech", new object[]
			{
				tech.Id,
				flag,
				text,
				text2,
				state,
				num,
				num2,
				tech.Family,
				ResearchScreenState.GetTechTreeRoots(base.App.AssetDatabase, base.App.LocalPlayer.Faction).Contains(tech.Id)
			});
		}
		private void SyncBranch(string fromTechId, string toTechId, TechStates state)
		{
			this._research.PostSetProp("Branch", new object[]
			{
				fromTechId,
				toTechId,
				state
			});
		}
		private void DebugShowAllTechs()
		{
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech current in base.App.AssetDatabase.MasterTechTree.Technologies)
			{
				this.SyncTech(current, TechStates.Researched, null);
				foreach (Allowable current2 in current.Allows)
				{
					this.SyncBranch(current.Id, current2.Id, TechStates.Researched);
				}
			}
			this._research.RebindModels();
		}
		private void SyncTechTree(int playerId)
		{
			this._research.Clear();
			IEnumerable<PlayerTechInfo> playerTechInfos = base.App.GameDatabase.GetPlayerTechInfos(base.App.LocalPlayer.ID);
			foreach (PlayerTechInfo current in playerTechInfos)
			{
				if (current.State != TechStates.Locked)
				{
					this.SyncTechState(current.TechID);
				}
			}
			this._research.RebindModels();
		}
		private void SyncTechTree(string faction)
		{
			this._research.Clear();
			Dictionary<Kerberos.Sots.Data.TechnologyFramework.Tech, Dictionary<Allowable, ResearchScreenState.TechBranch>> dictionary;
			ResearchScreenState.BuildFactionTechTree(base.App.AssetDatabase, base.App.GameDatabase, faction, out dictionary);
			foreach (Kerberos.Sots.Data.TechnologyFramework.Tech current in dictionary.Keys)
			{
				this.SyncTech(current, TechStates.Researched, null);
				foreach (ResearchScreenState.TechBranch current2 in dictionary[current].Values)
				{
					this.SyncBranch(current.Id, current2.Allows.Id, TechStates.Researched);
				}
			}
			this._research.RebindModels();
		}
		private void SyncTechState(int techId)
		{
			PlayerTechInfo playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, techId);
			string techFileId = playerTechInfo.TechFileID;
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = base.App.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techFileId);
			this.SyncTech(tech, playerTechInfo.State, playerTechInfo);
			IEnumerable<PlayerBranchInfo> unlockedBranchesToTech = base.App.GameDatabase.GetUnlockedBranchesToTech(base.App.LocalPlayer.ID, playerTechInfo.TechID);
			foreach (PlayerBranchInfo current in unlockedBranchesToTech)
			{
				string techFileID = base.App.GameDatabase.GetTechFileID(current.FromTechID);
				this.SyncBranch(techFileID, techFileId, playerTechInfo.State);
			}
		}
		private void SetTechState(string techIdStr, TechStates techState)
		{
			int techID = base.App.GameDatabase.GetTechID(techIdStr);
			if (techID == 0)
			{
				return;
			}
			if (base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, techID).State == TechStates.Locked)
			{
				return;
			}
			base.App.GameDatabase.UpdatePlayerTechState(base.App.LocalPlayer.ID, techID, techState);
			this.SyncTechState(techID);
		}
		private void CancelFeasibility()
		{
			FeasibilityStudyInfo feasibilityStudyInfoByPlayer = base.App.GameDatabase.GetFeasibilityStudyInfoByPlayer(base.App.LocalPlayer.ID);
			if (feasibilityStudyInfoByPlayer != null)
			{
				this.CancelProject(feasibilityStudyInfoByPlayer.ProjectID);
			}
			string cueName = string.Format("STRAT_031-01_{0}_CancelFeasibilityStudy", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)));
			base.App.PostRequestSpeech(cueName, 50, 120, 0f);
			base.App.PostRequestStopSound("research_feasibilitystudyloop");
		}
		private void CancelResearchCore()
		{
			int playerResearchingTechID = base.App.GameDatabase.GetPlayerResearchingTechID(base.App.LocalPlayer.ID);
			if (playerResearchingTechID == 0)
			{
				return;
			}
			string techFileID = base.App.GameDatabase.GetTechFileID(playerResearchingTechID);
			PlayerTechInfo playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, playerResearchingTechID);
			if (playerTechInfo.Feasibility >= 1f)
			{
				this.SetTechState(techFileID, TechStates.Core);
			}
			else
			{
				if (playerTechInfo.PlayerFeasibility < 0.3f)
				{
					this.SetTechState(techFileID, TechStates.LowFeasibility);
				}
				else
				{
					this.SetTechState(techFileID, TechStates.HighFeasibility);
				}
			}
			base.App.PostRequestStopSound("research_researchstudyloop");
		}
		public static int StartFeasibilityStudy(GameDatabase db, int playerId, int techId)
		{
			if (db.GetFeasibilityStudyInfo(playerId, techId) != null)
			{
				return 0;
			}
			PlayerTechInfo playerTechInfo = db.GetPlayerTechInfo(playerId, techId);
			if (playerTechInfo.State != TechStates.Branch)
			{
				return 0;
			}
			string localizedTechnologyName = db.AssetDatabase.GetLocalizedTechnologyName(playerTechInfo.TechFileID);
			string projectName = string.Format(App.Localize("@UI_RESEARCH_FEASABILITY_STUDY"), localizedTechnologyName);
			int result = db.InsertFeasibilityStudy(playerId, techId, projectName);
			db.UpdatePlayerTechState(playerId, techId, TechStates.PendingFeasibility);
			return result;
		}
		public static int StartFeasibilityStudy(GameDatabase db, int playerId, string techIdStr)
		{
			int techID = db.GetTechID(techIdStr);
			return ResearchScreenState.StartFeasibilityStudy(db, playerId, techID);
		}
		private void StartFeasibilityStudy(string techIdStr)
		{
			if (this.isPlayerBusyResearching(base.App.Game.LocalPlayer.ID))
			{
				return;
			}
			int projectId = ResearchScreenState.StartFeasibilityStudy(base.App.GameDatabase, base.App.LocalPlayer.ID, techIdStr);
			this.SyncTechState(base.App.GameDatabase.GetTechID(techIdStr));
			this.RefreshResearchButton();
			this.RefreshFeasibilityStudy();
			this.RefreshProjectList();
			this.SetSelectedProject(projectId, string.Empty);
			string cueName = string.Format("STRAT_028-01_{0}_StartFeasibilityStudy", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)));
			base.App.PostRequestSpeech(cueName, 50, 120, 0f);
			base.App.PostRequestGuiSound("research_feasibilitystudyloop");
			this.DoResearchWhooshAnimation();
		}
		private void SetSelectedProject(int projectId, string trigger)
		{
			ResearchProjectInfo researchProjectInfo = null;
			if (projectId != 0)
			{
				researchProjectInfo = base.App.GameDatabase.GetResearchProjectInfo(base.App.LocalPlayer.ID, projectId);
			}
			this._selectedProject = projectId;
			if (trigger != "research_projects")
			{
				base.App.UI.SetSelection("research_projects", projectId);
			}
			bool value = researchProjectInfo != null && researchProjectInfo.State == ProjectStates.Available;
			base.App.UI.SetEnabled("start_project_button", value);
			bool value2 = researchProjectInfo != null && researchProjectInfo.State == ProjectStates.InProgress;
			base.App.UI.SetEnabled("cancel_project_button", value2);
			if (projectId != 0)
			{
				FeasibilityStudyInfo feasibilityStudyInfo = base.App.GameDatabase.GetFeasibilityStudyInfo(projectId);
				if (feasibilityStudyInfo != null)
				{
					string techFileID = base.App.GameDatabase.GetTechFileID(feasibilityStudyInfo.TechID);
					this.SetSelectedTech(techFileID, string.Empty);
				}
			}
		}
		public bool isPlayerBusyResearching(int playerid)
		{
			int playerResearchingTechID = base.App.GameDatabase.GetPlayerResearchingTechID(playerid);
			return playerResearchingTechID != 0 || base.App.GameDatabase.GetPlayerFeasibilityStudyTechId(playerid) != 0;
		}
		private void StartProject(int projectId)
		{
			if (projectId == 0)
			{
				return;
			}
			if (this.isPlayerBusyResearching(base.App.Game.LocalPlayer.ID))
			{
				return;
			}
			base.App.GameDatabase.UpdateResearchProjectState(projectId, ProjectStates.InProgress);
			this.DoResearchWhooshAnimation();
			base.App.PostRequestGuiSound("research_researchstudyloop");
		}
		public static void CancelResearchProject(App game, int playerId, int projectId)
		{
			FeasibilityStudyInfo feasibilityStudyInfo = game.GameDatabase.GetFeasibilityStudyInfo(projectId);
			if (feasibilityStudyInfo != null)
			{
				game.GameDatabase.RemoveFeasibilityStudy(projectId);
				game.GameDatabase.UpdatePlayerTechState(playerId, feasibilityStudyInfo.TechID, TechStates.Branch);
				return;
			}
			ResearchProjectInfo researchProjectInfo = game.GameDatabase.GetResearchProjectInfo(playerId, projectId);
			ProjectStates state = ProjectStates.Available;
			if (researchProjectInfo.Progress > 0f)
			{
				state = ProjectStates.Paused;
			}
			game.GameDatabase.UpdateResearchProjectState(projectId, state);
		}
		private void CancelProject(int projectId)
		{
			if (projectId == 0)
			{
				return;
			}
			FeasibilityStudyInfo feasibilityStudyInfo = base.App.GameDatabase.GetFeasibilityStudyInfo(projectId);
			ResearchScreenState.CancelResearchProject(base.App, base.App.LocalPlayer.ID, projectId);
			if (feasibilityStudyInfo != null)
			{
				this.SyncTechState(feasibilityStudyInfo.TechID);
				this.RefreshResearchButton();
				this.RefreshFeasibilityStudy();
			}
			this.RefreshProjectList();
			this.SetSelectedProject(0, string.Empty);
		}
		private void StartResearch(string techIdStr)
		{
			int techID = base.App.GameDatabase.GetTechID(techIdStr);
			if (techID == 0)
			{
				return;
			}
			PlayerTechInfo playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, techID);
			if (playerTechInfo.State == TechStates.Branch)
			{
				return;
			}
			this.CancelResearchCore();
			this.SetTechState(techIdStr, TechStates.Researching);
			this.RefreshResearchButton();
			this.RefreshResearchingTech();
			string cueName = string.Format("STRAT_029-01_{0}_StartResearch", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)));
			base.App.PostRequestSpeech(cueName, 50, 120, 0f);
			this.DoResearchWhooshAnimation();
		}
		private void CancelResearch()
		{
			this.CancelResearchCore();
			this.RefreshResearchButton();
			this.RefreshResearchingTech();
			string cueName = string.Format("STRAT_030-01_{0}_CancelResearch", base.App.GameDatabase.GetFactionName(base.App.GameDatabase.GetPlayerFactionID(base.App.LocalPlayer.ID)));
			base.App.PostRequestSpeech(cueName, 50, 120, 0f);
		}
		private void PopulateFamilyList()
		{
			base.App.UI.ClearItems("familyList");
			this.FamilyIndex.Clear();
			int num = 0;
			foreach (TechFamily family in base.App.AssetDatabase.MasterTechTree.TechFamilies)
			{
				if (family.FactionDefined)
				{
					if (!base.App.LocalPlayer.Faction.TechTreeRoots.Any((string x) => x.StartsWith(family.Id)))
					{
						continue;
					}
				}
				base.App.UI.AddItem("familyList", "", num, "");
				string itemGlobalID = base.App.UI.GetItemGlobalID("familyList", "", num, "");
				this.FamilyIndex[family.Id] = num;
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_idle.idle"
				}), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_idle.mouse_over"
				}), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_idle.pressed"
				}), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_idle.disabled"
				}), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_idle"
				}), "id", "techButton|" + num.ToString());
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_sel.idle"
				}), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_sel.mouse_over"
				}), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_sel.pressed"
				}), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_sel.disabled"
				}), "sprite", ResearchScreenState.IconTextureToSpriteName(family.Icon));
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					itemGlobalID,
					"header_sel"
				}), "id", "techButton|" + num.ToString());
				num++;
			}
		}
		private void UpdateSelectedTech()
		{
			int techID = base.App.GameDatabase.GetTechID(this._selectedTech);
			base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, techID);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = base.App.AssetDatabase.MasterTechTree.Technologies.First((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == this._selectedTech);
			ResearchScreenState.IconTextureToSpriteName(tech.Icon);
			TechFamily techFamily = base.App.AssetDatabase.MasterTechTree.TechFamilies.First((TechFamily x) => x.Id == tech.Family);
			ResearchScreenState.IconTextureToSpriteName(techFamily.Icon);
			base.App.UI.SetSelection("familyList", this.FamilyIndex[techFamily.Id]);
			PlayerTechInfo playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, techID);
			if (playerTechInfo != null)
			{
				if (ResearchScreenState.GetTurnsToCompleteResearch(base.App, base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID), playerTechInfo).HasValue)
				{
					base.App.UI.SetPropertyString("selected_tech_time", "text", ResearchScreenState.GetTurnsToCompleteString(base.App, base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID), playerTechInfo));
					return;
				}
				base.App.UI.SetPropertyString("selected_tech_time", "text", "∞ Turns");
			}
		}
		private void SetSelectedTech(string techIdStr, string trigger)
		{
			this.SetSelectedTechDetailsVisible(false);
			this._selectedTech = techIdStr;
			int techID = base.App.GameDatabase.GetTechID(techIdStr);
			base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, techID);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = base.App.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == techIdStr);
			if (tech == null)
			{
				return;
			}
			string text = ResearchScreenState.IconTextureToSpriteName(tech.Icon);
			TechFamily techFamily = base.App.AssetDatabase.MasterTechTree.TechFamilies.First((TechFamily x) => x.Id == tech.Family);
			string text2 = ResearchScreenState.IconTextureToSpriteName(techFamily.Icon);
			base.App.UI.SetSelection("familyList", this.FamilyIndex[techFamily.Id]);
			if (trigger != "TechClicked")
			{
				this._research.PostSetProp("Select", techIdStr);
			}
			base.App.UI.SetVisible("selected_tech_icon", !string.IsNullOrEmpty(text));
			base.App.UI.SetVisible("discipline_icon", !string.IsNullOrEmpty(text2));
			if (techFamily == null)
			{
				base.App.UI.SetVisible("moduleCountLabel", false);
				base.App.UI.SetVisible("moduleBonusLabel", false);
			}
			else
			{
				int num = 0;
				int percentBonusToResearch = this.GetPercentBonusToResearch(techFamily, out num);
				if (num == 1)
				{
					base.App.UI.SetText("moduleCountLabel", "1 " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_MODULE_INSTALLED"));
				}
				else
				{
					base.App.UI.SetText("moduleCountLabel", num.ToString() + " " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_MODULES_INSTALLED"));
				}
				base.App.UI.SetText("moduleBonusLabel", percentBonusToResearch.ToString() + "% " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_BONUS_TO_RESEARCH_PROJECTS"));
			}
			base.App.UI.SetPropertyString("selected_tech_icon", "sprite", text);
			base.App.UI.SetPropertyString("selected_tech_family_name", "text", AssetDatabase.CommonStrings.Localize("@TECH_FAMILY_" + techFamily.Id));
			base.App.UI.SetPropertyString("discipline_icon", "sprite", text2);
			base.App.UI.SetPropertyString("selected_tech_desc", "text", App.Localize("@TECH_DESC_" + tech.Id));
			PlayerTechInfo playerTechInfo = base.App.GameDatabase.GetPlayerTechInfo(base.App.LocalPlayer.ID, techID);
			if (playerTechInfo != null)
			{
				if (ResearchScreenState.GetTurnsToCompleteResearch(base.App, base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID), playerTechInfo).HasValue)
				{
					base.App.UI.SetPropertyString("selected_tech_time", "text", ResearchScreenState.GetTurnsToCompleteString(base.App, base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID), playerTechInfo));
					base.App.UI.SetPropertyString("selected_tech_time_right", "text", ResearchScreenState.GetTurnsToCompleteString(base.App, base.App.GameDatabase.GetPlayerInfo(base.App.LocalPlayer.ID), playerTechInfo));
				}
				else
				{
					base.App.UI.SetPropertyString("selected_tech_time", "text", "∞ Turns");
					base.App.UI.SetPropertyString("selected_tech_time_right", "text", "∞ Turns");
				}
			}
			if (tech != null)
			{
				LogicalWeapon weaponUnlockedByTech = this.GetWeaponUnlockedByTech(tech);
				if (weaponUnlockedByTech != null)
				{
					base.App.UI.SetVisible("selectedTechDetailsButton", true);
					this._weaponInfoPanel.SetWeapons(weaponUnlockedByTech, null);
				}
				else
				{
					base.App.UI.SetVisible("selectedTechDetailsButton", false);
				}
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				{
					base.App.UI.SetVisible("selectedTechInfoButton", ScriptHost.AllowConsole);
				}
			}
			else
			{
				base.App.UI.SetVisible("selectedTechDetailsButton", false);
				base.App.UI.SetVisible("selectedTechInfoButton", false);
			}
			this.RefreshResearchButton();
		}
		private void SetSelectedFamily(string familyId)
		{
			TechFamily techFamily = base.App.AssetDatabase.MasterTechTree.TechFamilies.First((TechFamily x) => x.Id == familyId);
			string text = ResearchScreenState.IconTextureToSpriteName(techFamily.Icon);
			base.App.UI.SetVisible("discipline_icon", !string.IsNullOrEmpty(text));
			if (techFamily == null)
			{
				base.App.UI.SetVisible("moduleCountLabel", false);
				base.App.UI.SetVisible("moduleBonusLabel", false);
			}
			else
			{
				int num = 0;
				int percentBonusToResearch = this.GetPercentBonusToResearch(techFamily, out num);
				if (num == 1)
				{
					base.App.UI.SetText("moduleCountLabel", "1 " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_MODULE_INSTALLED"));
				}
				else
				{
					base.App.UI.SetText("moduleCountLabel", num.ToString() + " " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_MODULES_INSTALLED"));
				}
				base.App.UI.SetText("moduleBonusLabel", percentBonusToResearch.ToString() + "% " + AssetDatabase.CommonStrings.Localize("@UI_RESEARCH_BONUS_TO_RESEARCH_PROJECTS"));
			}
			base.App.UI.SetPropertyString("selected_tech_family_name", "text", AssetDatabase.CommonStrings.Localize("@TECH_FAMILY_" + techFamily.Id));
			base.App.UI.SetPropertyString("discipline_icon", "sprite", text);
			base.App.UI.SetSelection("familyList", this.FamilyIndex[familyId]);
		}
		private LogicalWeapon GetWeaponUnlockedByTech(Kerberos.Sots.Data.TechnologyFramework.Tech tech)
		{
			return base.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.RequiredTechs.Any((Kerberos.Sots.Data.WeaponFramework.Tech y) => y.Name == tech.Id));
		}
		private ShipSectionAsset GetShipSectionUnlockedByTech(Kerberos.Sots.Data.TechnologyFramework.Tech tech)
		{
			return base.App.AssetDatabase.ShipSections.FirstOrDefault((ShipSectionAsset x) => x.Faction == this.App.LocalPlayer.Faction.Name && x.RequiredTechs.Any((string y) => y == tech.Id));
		}
		private ShipClass? GetShipClassUnlockedByTech(Kerberos.Sots.Data.TechnologyFramework.Tech tech)
		{
			if (tech.Id == "ENG_Leviathian_Construction")
			{
				return new ShipClass?(ShipClass.Leviathan);
			}
			if (tech.Id == "ENG_Dreadnought_Construction")
			{
				return new ShipClass?(ShipClass.Dreadnought);
			}
			if (tech.Id == "ENG_Cruiser_Construction")
			{
				return new ShipClass?(ShipClass.Cruiser);
			}
			if (tech.Id == "BRD_BattleRiders")
			{
				return new ShipClass?(ShipClass.BattleRider);
			}
			return null;
		}
		private int GetPercentBonusToResearch(TechFamily family, out int modulesInstalled)
		{
			modulesInstalled = 0;
			float num = base.App.Game.GetFamilySpecificResearchModifier(base.App.LocalPlayer.ID, family.Id, out modulesInstalled);
			num += base.App.Game.GetGeneralResearchModifier(base.App.LocalPlayer.ID, true) - 1f;
			return (int)Math.Round((double)(num * 100f));
		}
		protected override void OnUpdate()
		{
			if (this._budgetChanged)
			{
				this._budgetChanged = false;
				this.RefreshResearchButton();
				this.RefreshResearchingTech();
				this.UpdateSelectedTech();
			}
			if (this._leftButton > 0 && this._enteredButton)
			{
				this._leftButton = 0;
			}
			if (this._leftButton == 1 && !this._enteredButton)
			{
				base.App.GameDatabase.GetTechID(this.SelectedTech);
				Kerberos.Sots.Data.TechnologyFramework.Tech tech = base.App.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault((Kerberos.Sots.Data.TechnologyFramework.Tech x) => x.Id == this.SelectedTech);
				if (tech != null)
				{
					this.SetSelectedFamily(tech.Family);
				}
			}
			this._enteredButton = false;
			if (this._leftButton > 0)
			{
				this._leftButton--;
			}
		}
		public override bool IsReady()
		{
			return this._crits.IsReady() && base.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		private void UpdateResearchSliders(PlayerInfo playerInfo, string iChanged)
		{
			double num = (double)((1f - playerInfo.RateGovernmentResearch) * 100f);
			double num2 = (double)(playerInfo.RateResearchCurrentProject * 100f);
			double num3 = (double)(playerInfo.RateResearchSpecialProject * 100f);
			double num4 = (double)(playerInfo.RateResearchSalvageResearch * 100f);
			if (iChanged != "research_slider")
			{
				base.App.UI.SetSliderValue("research_slider", (int)num);
			}
			if (iChanged != ResearchScreenState.UICurrentProjectSlider)
			{
				base.App.UI.SetSliderValue(ResearchScreenState.UICurrentProjectSlider, (int)num2);
			}
			if (iChanged != ResearchScreenState.UISpecialProjectSlider)
			{
				base.App.UI.SetSliderValue(ResearchScreenState.UISpecialProjectSlider, (int)num3);
			}
			if (iChanged != ResearchScreenState.UISalvageResearchSlider)
			{
				base.App.UI.SetSliderValue(ResearchScreenState.UISalvageResearchSlider, (int)num4);
			}
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
					return false;
				case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
					return false;
				case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_SotspediaScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<SotspediaState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_StarSystemScreen:
				case HotKeyManager.HotKeyActions.State_FleetManagerScreen:
				case HotKeyManager.HotKeyActions.State_DefenseManagerScreen:
				case HotKeyManager.HotKeyActions.State_BattleRiderScreen:
					break;
				case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<DiplomacyScreenState>(new object[0]);
					return true;
				default:
					switch (action)
					{
					case HotKeyManager.HotKeyActions.Research_NextTree:
						this._research.PrevTechFamily();
						return true;
					case HotKeyManager.HotKeyActions.Research_LastTree:
						this._research.NextTechFamily();
						return true;
					}
					break;
				}
			}
			return false;
		}
	}
}
