using Kerberos.Sots.Data.ScenarioFramework;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class GameSetupState : GameState
	{
		private const string UIBackButton = "gameBackButton";
		private const string UIPlayerSetupButton = "gamePlayerSetupButton";
		private const string UIStarMapList = "gameStarMapList";
		private const string UIScenarioList = "gameScenarioList";
		private const string UIMapsCheckbox = "gameMapsCheckbox";
		private const string UIScenariosCheckbox = "gameScenariosCheckbox";
		private Dictionary<int, Starmap.StarmapInfo> _starmapIdMap;
		private Dictionary<int, Scenario.ScenarioInfo> _scenarioIdMap;
		private StarMapPreview _starmapPreview;
		private TimeSlider _strategicTurnLengthSlider;
		private TimeSlider _combatTurnLengthSlider;
		private PercentageSlider _economicEfficiencySlider;
		private PercentageSlider _researchEfficiencySlider;
		private ValueBoundSlider _planetResourcesSlider;
		private ValueBoundSlider _planetSizeSlider;
		private TreasurySlider _initialTreasurySlider;
		private ValueBoundSpinner _numPlayersSpinner;
		private ValueBoundSpinner _initialSystemsSpinner;
		private ValueBoundSpinner _initialTechnologiesSpinner;
		private PercentageSlider _randomEncounterFrequencySlider;
		private ValueBoundSlider _grandMenaceSlider;
		private bool _creatingGame;
		private int _maxPlayers;
		private GameSetup _tempGameSetup;
		private string _victoryConditionDialog;
		private int _modeSliderVal;
		private GameMode _gameMode;
		public GameSetupState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._creatingGame = false;
			if (parms.Count<object>() > 0)
			{
				this._creatingGame = (bool)parms[0];
			}
			if (this._tempGameSetup == null)
			{
				this._tempGameSetup = new GameSetup(base.App);
			}
			base.App.UI.LoadScreen("GameSetup");
			this.RecreateStarmapPreview();
		}
		protected override void OnEnter()
		{
			base.App.PostPlayMusic("Ambient_GameSetup");
			base.App.UI.SetScreen("GameSetup");
			base.App.UI.SetEnabled("gamePlayerSetupButton", false);
			this.RecreateStarmapPreview();
			if (this._creatingGame)
			{
				base.App.UI.SetText("gamePlayerSetupButton", App.Localize("@UI_GAMESETUPSTATE_CONFIRM"));
				base.App.UI.SetVisible("gameBackButton", true);
			}
			else
			{
				base.App.UI.SetText("gamePlayerSetupButton", "Done");
				base.App.UI.SetVisible("gameBackButton", false);
			}
			this.CreateUI();
			this.SetGameMode(GameMode.LastSideStanding);
		}
		private void CreateUI()
		{
			this._strategicTurnLengthSlider = new TimeSlider(base.App.UI, "gameStrategyTurnLength", base.App.GameSetup.StrategicTurnLength, 0.25f, 15f, 0.25f, true);
			this._combatTurnLengthSlider = new TimeSlider(base.App.UI, "gameCombatTurnLength", base.App.GameSetup.CombatTurnLength, 3f, 12f, 1f, false);
			this._economicEfficiencySlider = new PercentageSlider(base.App.UI, "gameEconomicEfficiency", base.App.GameSetup.EconomicEfficiency, 50, 200);
			this._researchEfficiencySlider = new PercentageSlider(base.App.UI, "gameResearchEfficiency", base.App.GameSetup.ResearchEfficiency, 50, 200);
			this._planetResourcesSlider = new PercentageSlider(base.App.UI, "gameStarMapPlanetResSlider", base.App.GameSetup.PlanetResources, 50, 150);
			this._planetSizeSlider = new PercentageSlider(base.App.UI, "gameStarMapPlanetSizeSlider", base.App.GameSetup.PlanetSize, 50, 150);
			this._initialTreasurySlider = new TreasurySlider(base.App.UI, "gameInitialTreasury", base.App.GameSetup.InitialTreasury, 0, 1000000);
			this._numPlayersSpinner = new ValueBoundSpinner(base.App.UI, "gameNumPlayers", 2.0, 8.0, (double)base.App.GameSetup.Players.Count, 1.0);
			this._initialSystemsSpinner = new ValueBoundSpinner(base.App.UI, "gameInitialSystems", 3.0, 9.0, (double)base.App.GameSetup.InitialSystems, 1.0);
			this._initialTechnologiesSpinner = new ValueBoundSpinner(base.App.UI, "gameInitialTechs", 0.0, 10.0, (double)base.App.GameSetup.InitialTechnologies, 1.0);
			this._randomEncounterFrequencySlider = new PercentageSlider(base.App.UI, "gameRandomEncounterFrequency", base.App.GameSetup.RandomEncounterFrequency, 0, 200);
			this._grandMenaceSlider = new ValueBoundSlider(base.App.UI, "gameGrandMenaces", 0, 5, base.App.GameSetup.GrandMenaceCount);
			foreach (Faction current in base.App.AssetDatabase.Factions)
			{
				if (current.IsPlayable)
				{
					string panelId = GameSetupState.UIAvailableFactionCheckBox(current);
					bool isChecked = base.App.GameSetup.AvailablePlayerFeatures.Factions.ContainsKey(current);
					base.App.UI.SetChecked(panelId, isChecked);
				}
			}
			string[] array = new string[]
			{
				"loa"
			};
			string[] array2 = array;
			string expansionFactionName;
			for (int i = 0; i < array2.Length; i++)
			{
				expansionFactionName = array2[i];
				if (!base.App.AssetDatabase.Factions.Any((Faction x) => x.Name == expansionFactionName))
				{
					base.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(expansionFactionName), false);
					base.App.UI.SetVisible(GameSetupState.UIAvailableFactionFrame(expansionFactionName), false);
				}
			}
			base.App.UI.SetVisible("gameScenarioList", false);
			base.App.UI.SetVisible("gameStarMapList", true);
			base.App.UI.SetChecked("gameScenariosCheckbox", false);
			base.App.UI.SetChecked("gameMapsCheckbox", true);
			this.PopulateScenarioList();
			this.PopulateStarMapList();
			if (this._starmapIdMap.Count > 0)
			{
				if (ScriptHost.AllowConsole)
				{
					if (this._starmapIdMap.Any((KeyValuePair<int, Starmap.StarmapInfo> x) => x.Value.Title == "@STARMAP_TITLE_FIGHT"))
					{
						base.App.UI.SetSelection("gameStarMapList", this._starmapIdMap.FirstOrDefault((KeyValuePair<int, Starmap.StarmapInfo> x) => x.Value.Title == "@STARMAP_TITLE_FIGHT").Key);
						return;
					}
				}
				base.App.UI.SetSelection("gameStarMapList", this._starmapIdMap.Keys.First<int>());
			}
		}
		private void PopulateStarMapList()
		{
			base.App.UI.ClearItems("gameStarMapList");
			this._starmapIdMap = new Dictionary<int, Starmap.StarmapInfo>();
			string[] array = (
				from y in ScriptHost.FileSystem.FindFiles("starmaps\\*.starmap")
				orderby Path.GetFileNameWithoutExtension(y)
				select y).ToArray<string>();
			for (int i = 0; i < array.Length; i++)
			{
				this._starmapIdMap[i] = new Starmap.StarmapInfo(array[i]);
				base.App.UI.AddItem("gameStarMapList", string.Empty, i, string.Format("{0} [{1}]", App.Localize(this._starmapIdMap[i].GetFallbackTitle()), this._starmapIdMap[i].NumPlayers));
			}
		}
		private void PopulateScenarioList()
		{
			base.App.UI.ClearItems("gameScenarioList");
			this._scenarioIdMap = new Dictionary<int, Scenario.ScenarioInfo>();
			string[] array = (
				from y in ScriptHost.FileSystem.FindFiles("scenarios\\*.scenario")
				orderby Path.GetFileNameWithoutExtension(y)
				select y).ToArray<string>();
			for (int i = 0; i < array.Length; i++)
			{
				this._scenarioIdMap[i] = new Scenario.ScenarioInfo(array[i]);
				base.App.UI.AddItem("gameScenarioList", string.Empty, i, string.Format("{0} [?]", App.Localize(this._scenarioIdMap[i].GetFallbackTitle())));
			}
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.UI.DeleteScreen("GameSetup");
			if (this._starmapPreview != null)
			{
				this._starmapPreview.Dispose();
				this._starmapPreview = null;
			}
			this._strategicTurnLengthSlider = null;
			this._combatTurnLengthSlider = null;
			this._economicEfficiencySlider = null;
			this._researchEfficiencySlider = null;
			this._planetResourcesSlider = null;
			this._planetSizeSlider = null;
			this._initialTreasurySlider = null;
			this._numPlayersSpinner = null;
			this._initialSystemsSpinner = null;
			this._initialTechnologiesSpinner = null;
			this._randomEncounterFrequencySlider = null;
			this._grandMenaceSlider = null;
			this._starmapIdMap = null;
			this._scenarioIdMap = null;
		}
		private static string UIAvailableFactionFrame(string factionName)
		{
			return string.Format("gameFaction_{0}_frame", factionName);
		}
		private static string UIAvailableFactionCheckBox(string factionName)
		{
			return string.Format("gameFaction_{0}", factionName);
		}
		private static string UIAvailableFactionCheckBox(Faction faction)
		{
			return GameSetupState.UIAvailableFactionCheckBox(faction.Name);
		}
		private void UpdateData()
		{
			base.App.GameSetup.StrategicTurnLength = this._strategicTurnLengthSlider.TimeInMinutes;
			base.App.GameSetup.CombatTurnLength = this._combatTurnLengthSlider.TimeInMinutes;
			base.App.GameSetup.EconomicEfficiency = this._economicEfficiencySlider.Value;
			base.App.GameSetup.ResearchEfficiency = this._researchEfficiencySlider.Value;
			base.App.GameSetup.PlanetResources = this._planetResourcesSlider.Value;
			base.App.GameSetup.PlanetSize = this._planetSizeSlider.Value;
			base.App.GameSetup.InitialTreasury = this._initialTreasurySlider.Value;
			base.App.GameSetup.InitialSystems = (int)this._initialSystemsSpinner.Value;
			base.App.GameSetup.InitialTechnologies = (int)this._initialTechnologiesSpinner.Value;
			base.App.GameSetup.RandomEncounterFrequency = this._randomEncounterFrequencySlider.Value;
			base.App.GameSetup.GrandMenaceCount = this._grandMenaceSlider.Value;
			base.App.GameSetup.SetPlayerCount((int)this._numPlayersSpinner.Value);
		}
		private void SelectStarMapOrScenario(int? nullableId, bool isMapFile)
		{
			if (!nullableId.HasValue)
			{
				this._tempGameSetup.StarMapFile = string.Empty;
				this._tempGameSetup.ScenarioFile = string.Empty;
				if (this._starmapPreview != null)
				{
					this._starmapPreview.Dispose();
					this._starmapPreview = null;
					return;
				}
			}
			else
			{
				int value = nullableId.Value;
				string fallbackTitle;
				string strId;
				if (isMapFile)
				{
					fallbackTitle = this._starmapIdMap[value].GetFallbackTitle();
					strId = this._starmapIdMap[value].Description;
				}
				else
				{
					fallbackTitle = this._scenarioIdMap[value].GetFallbackTitle();
					strId = string.Empty;
				}
				base.App.UI.SetPropertyString("mapLabel", "text", App.Localize(fallbackTitle));
				base.App.UI.SetText("mapsummary_Content", App.Localize(strId));
				if (isMapFile)
				{
					foreach (Faction current in base.App.AssetDatabase.Factions)
					{
						base.App.GameSetup.AvailablePlayerFeatures.TryAddFaction(current);
						if (current.IsPlayable)
						{
							base.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(current), true);
							base.App.UI.SetEnabled(GameSetupState.UIAvailableFactionCheckBox(current), true);
						}
					}
					this._numPlayersSpinner.SetEnabled(true);
					this._initialSystemsSpinner.SetEnabled(true);
					this._initialTechnologiesSpinner.SetEnabled(true);
					this._initialTreasurySlider.SetEnabled(true);
					base.App.UI.SetEnabled("victoryToggle", true);
					Starmap.StarmapInfo starmapInfo = this._starmapIdMap[value];
					this._tempGameSetup.StarMapFile = starmapInfo.FileName;
					this._tempGameSetup.ScenarioFile = string.Empty;
					this._numPlayersSpinner.SetValue((double)starmapInfo.NumPlayers);
					this._maxPlayers = starmapInfo.NumPlayers;
				}
				else
				{
					Scenario.ScenarioInfo scenarioInfo = this._scenarioIdMap[value];
					this._tempGameSetup.ScenarioFile = scenarioInfo.FileName;
					this._tempGameSetup.StarMapFile = scenarioInfo.StarmapInfo.FileName;
					this._numPlayersSpinner.SetValue((double)scenarioInfo.StarmapInfo.NumPlayers);
					Scenario scenario = new Scenario();
					ScenarioXmlUtility.LoadScenarioFromXml(scenarioInfo.FileName, ref scenario);
					foreach (Faction current2 in base.App.AssetDatabase.Factions)
					{
						base.App.GameSetup.AvailablePlayerFeatures.TryRemoveFaction(current2);
						if (current2.IsPlayable)
						{
							base.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(current2), false);
							base.App.UI.SetEnabled(GameSetupState.UIAvailableFactionCheckBox(current2), false);
						}
					}
					foreach (Kerberos.Sots.Data.ScenarioFramework.Player current3 in scenario.PlayerStartConditions)
					{
						Faction faction = base.App.AssetDatabase.GetFaction(current3.Faction);
						base.App.GameSetup.AvailablePlayerFeatures.TryAddFaction(faction);
						if (faction.IsPlayable)
						{
							base.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(faction), true);
						}
					}
					this._numPlayersSpinner.SetEnabled(false);
					this._initialSystemsSpinner.SetEnabled(false);
					this._initialTechnologiesSpinner.SetEnabled(false);
					this._initialTreasurySlider.SetEnabled(false);
					base.App.UI.SetEnabled("victoryToggle", false);
				}
				this.RecreateStarmapPreview();
			}
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._numPlayersSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
			{
				this._numPlayersSpinner.SetValue(Math.Max(0.0, Math.Min((double)this._maxPlayers, this._numPlayersSpinner.Value)));
				return;
			}
			if (this._strategicTurnLengthSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._combatTurnLengthSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._economicEfficiencySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._researchEfficiencySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._planetResourcesSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._planetSizeSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._initialTreasurySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._initialSystemsSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive) || this._initialTechnologiesSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive) || this._randomEncounterFrequencySlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self) || this._grandMenaceSlider.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
			{
				return;
			}
			if (msgType == "slider_value")
			{
				if (panelName == "gameModeSlider")
				{
					this._modeSliderVal = int.Parse(msgParams[0]);
					this.SetGameMode(this._gameMode);
					return;
				}
			}
			else
			{
				if (msgType == "button_clicked")
				{
					if (panelName == "gameBackButton")
					{
						this.GoBack();
						return;
					}
					if (panelName == "gamePlayerSetupButton")
					{
						base.App.GameSetup._mode = this._tempGameSetup._mode;
						base.App.GameSetup._modeValue = this._tempGameSetup._modeValue;
						base.App.GameSetup.StarMapFile = this._tempGameSetup.StarMapFile;
						base.App.GameSetup.ScenarioFile = this._tempGameSetup.ScenarioFile;
						if (base.App.GameSetup.HasScenarioFile())
						{
							Scenario scenario = new Scenario();
							ScenarioXmlUtility.LoadScenarioFromXml(base.App.GameSetup.ScenarioFile, ref scenario);
							int num = 0;
							foreach (Kerberos.Sots.Data.ScenarioFramework.Player current in scenario.PlayerStartConditions)
							{
								PlayerSetup playerSetup = base.App.GameSetup.Players[num];
								playerSetup.EmpireName = current.Name;
								playerSetup.Avatar = current.Avatar;
								playerSetup.Badge = current.Badge;
								playerSetup.ShipColor = current.ShipColor;
								playerSetup.Faction = current.Faction;
								playerSetup.AI = current.isAI;
								playerSetup.Fixed = true;
								playerSetup.InitialColonies = current.Colonies.Count;
								playerSetup.InitialTechs = current.StartingTechs.Count;
								playerSetup.InitialTreasury = (int)current.Treasury;
								num++;
							}
						}
						this.UpdateData();
						if (base.App.GameSetup.IsMultiplayer)
						{
							base.App.SwitchGameState(base.App.PreviousState, new object[]
							{
								LobbyEntranceState.Multiplayer
							});
							return;
						}
						base.App.SwitchGameState<StarMapLobbyState>(new object[]
						{
							LobbyEntranceState.SinglePlayer
						});
						return;
					}
					else
					{
						if (panelName == "victoryToggle")
						{
							this._victoryConditionDialog = base.App.UI.CreateDialog(new VictoryConditionDialog(base.App, "dialogVictoryCondition"), null);
							return;
						}
						if (panelName == "gameMapsCheckbox")
						{
							base.App.UI.SetChecked("gameMapsCheckbox", true);
							base.App.UI.SetChecked("gameScenariosCheckbox", false);
							base.App.UI.SetVisible("gameScenarioList", false);
							base.App.UI.SetVisible("gameStarMapList", true);
							this._tempGameSetup.ScenarioFile = string.Empty;
							this._tempGameSetup.StarMapFile = string.Empty;
							if (this._starmapIdMap.Count > 0)
							{
								if (ScriptHost.AllowConsole)
								{
									if (this._starmapIdMap.Any((KeyValuePair<int, Starmap.StarmapInfo> x) => x.Value.FileName == "FIGHT.starmap"))
									{
										base.App.UI.SetSelection("gameStarMapList", this._starmapIdMap.FirstOrDefault((KeyValuePair<int, Starmap.StarmapInfo> x) => x.Value.FileName == "FIGHT.starmap").Key);
										return;
									}
								}
								base.App.UI.SetSelection("gameStarMapList", this._starmapIdMap.Keys.First<int>());
								return;
							}
							base.App.UI.ClearSelection("gameStarMapList");
							return;
						}
						else
						{
							if (panelName == "gameScenariosCheckbox")
							{
								return;
							}
						}
					}
				}
				else
				{
					if (msgType == "checkbox_clicked")
					{
						using (IEnumerator<Faction> enumerator2 = base.App.AssetDatabase.Factions.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								Faction current2 = enumerator2.Current;
								if (panelName == GameSetupState.UIAvailableFactionCheckBox(current2))
								{
									bool flag = int.Parse(msgParams[0]) != 0;
									if (flag)
									{
										base.App.GameSetup.AvailablePlayerFeatures.TryAddFaction(current2);
									}
									else
									{
										if (base.App.GameSetup.AvailablePlayerFeatures.Factions.Keys.Count<Faction>() >= 2)
										{
											base.App.GameSetup.AvailablePlayerFeatures.TryRemoveFaction(current2);
										}
										else
										{
											base.App.UI.SetChecked(GameSetupState.UIAvailableFactionCheckBox(current2), true);
										}
									}
								}
							}
							return;
						}
					}
					if (msgType == "list_sel_changed")
					{
						if (panelName == "gameStarMapList" || panelName == "gameScenarioList")
						{
							bool isMapFile = panelName == "gameStarMapList";
							if (msgParams.Length == 1)
							{
								int value = int.Parse(msgParams[0]);
								this.SelectStarMapOrScenario(new int?(value), isMapFile);
							}
							else
							{
								this.SelectStarMapOrScenario(null, false);
							}
							base.App.UI.SetEnabled("gamePlayerSetupButton", this._tempGameSetup.HasStarMapFile() || this._tempGameSetup.HasScenarioFile());
							return;
						}
					}
					else
					{
						if (msgType == "dialog_closed" && panelName == this._victoryConditionDialog)
						{
							GameMode gameMode = (GameMode)int.Parse(msgParams[0]);
							if (gameMode == GameMode.LandGrab)
							{
								base.App.UI.SetPropertyInt("gameModeSlider", "value", 67);
								this._modeSliderVal = 67;
							}
							else
							{
								base.App.UI.SetPropertyInt("gameModeSlider", "value", 100);
								this._modeSliderVal = 100;
							}
							this.SetGameMode(gameMode);
						}
					}
				}
			}
		}
		private int GetModeSliderValue(GameMode mode)
		{
			switch (mode)
			{
			case GameMode.LastSideStanding:
				return -1;
			case GameMode.LastCapitalStanding:
				return -1;
			case GameMode.StarChamberLimit:
				return (int)((float)this._modeSliderVal / 100f * 4f) + 1;
			case GameMode.GemWorldLimit:
				return (int)((float)this._modeSliderVal / 100f * 4f) + 1;
			case GameMode.ProvinceLimit:
				return (int)((float)this._modeSliderVal / 100f * 4f) + 1;
			case GameMode.LeviathanLimit:
				return (int)((float)this._modeSliderVal / 100f * 9f) + 1;
			case GameMode.LandGrab:
				return (int)((float)this._modeSliderVal / 100f * 60f) + 20;
			default:
				return -1;
			}
		}
		private void SetGameMode(GameMode mode)
		{
			this._gameMode = mode;
			int modeSliderValue = this.GetModeSliderValue(this._gameMode);
			this._tempGameSetup._mode = this._gameMode;
			this._tempGameSetup._modeValue = modeSliderValue;
			if (modeSliderValue < 0)
			{
				base.App.UI.SetVisible("gameModeSlider", false);
			}
			else
			{
				base.App.UI.SetVisible("gameModeSlider", true);
			}
			switch (mode)
			{
			case GameMode.LastSideStanding:
				base.App.UI.SetPropertyString("victoryLabel", "text", App.Localize("@UI_GAMESETUP_LASTSIDESTANDING"));
				return;
			case GameMode.LastCapitalStanding:
				base.App.UI.SetPropertyString("victoryLabel", "text", App.Localize("@UI_GAMESETUP_LASTCAPITALSTANDING"));
				return;
			case GameMode.StarChamberLimit:
				base.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XSTARCHAMBERS"), modeSliderValue));
				return;
			case GameMode.GemWorldLimit:
				base.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XGEMWORLDS"), modeSliderValue));
				return;
			case GameMode.ProvinceLimit:
				base.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XPROVINCES"), modeSliderValue));
				return;
			case GameMode.LeviathanLimit:
				base.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XLEVIATHANS"), modeSliderValue));
				return;
			case GameMode.LandGrab:
				base.App.UI.SetPropertyString("victoryLabel", "text", string.Format(App.Localize("@UI_GAMESETUP_XLANDGRAB"), modeSliderValue));
				return;
			default:
				return;
			}
		}
		private void RecreateStarmapPreview()
		{
			if (this._starmapPreview != null)
			{
				this._starmapPreview.Dispose();
				base.App.UI.Send(new object[]
				{
					"SetGameObject",
					"starmapPreviewImage",
					0
				});
			}
			if (string.IsNullOrEmpty(this._tempGameSetup.StarMapFile))
			{
				return;
			}
			this._starmapPreview = new StarMapPreview(base.App, this._tempGameSetup);
			base.App.UI.Send(new object[]
			{
				"SetGameObject",
				"starmapPreviewImage",
				this._starmapPreview.StarMap.ObjectID
			});
		}
		private void GoBack()
		{
			if (base.App.GameSetup.IsMultiplayer)
			{
				base.App.SwitchGameState(base.App.PreviousState, new object[]
				{
					LobbyEntranceState.Browser
				});
				return;
			}
			base.App.SwitchGameState(base.App.GetGameState<MainMenuState>(), new object[0]);
		}
		protected override void OnUpdate()
		{
			if (this._numPlayersSpinner.Value > (double)this._maxPlayers)
			{
				this._numPlayersSpinner.SetValue((double)this._maxPlayers);
			}
			if (this._starmapPreview != null)
			{
				this._starmapPreview.Update();
			}
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
