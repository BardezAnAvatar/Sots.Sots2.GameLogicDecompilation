using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class EncounterDialog : Dialog
	{
		public static readonly string UIPostCombatSummaryPanel = "pnlPostCombatSummary";
		public static readonly string UIEncounterSummaryPanel = "pnlEncounterSummary";
		public static readonly string UIEncounterListPanel = "pnlPendingCombats";
		public static readonly string UIPreviousAlly = "btnPreviousAlly";
		public static readonly string UINextAlly = "btnNextAlly";
		public static readonly string UIPreviousEnemy = "btnPreviousEnemy";
		public static readonly string UINextEnemy = "btnNextEnemy";
		public static readonly string UICombatHeaderLabel = "lblHeader";
		public static readonly string UIInfoPanel = "pnlInfo";
		public static readonly string UIInfoList = "lstInfo";
		public static readonly string UIPostCombatImage = "imgPostCombatEvent";
		public static readonly string UIAlliedPlayerCard = "pnlAlliedPlayer";
		public static readonly string UIEnemyPlayerCard = "pnlEnemyPlayer";
		public static readonly string UIZoneRender = "pnlZoneRender";
		public static readonly string UIAlliedDamagePanel = "pnlAlliedDamage";
		public static readonly string UIAlliedDamageList = "lstAlliedDamage";
		public static readonly string UIEnemyDamagePanel = "pnlEnemyDamage";
		public static readonly string UIEnemyDamageList = "lstEnemyDamage";
		public static readonly string UIWeaponIcon = "imgWeaponIcon";
		public static readonly string UIWeaponDamage = "lblWeaponDamage";
		public static readonly string UIEncounterTitle = "lblEncounterTitle";
		public static readonly string UIMiniMapPanel = "pnlMiniMap";
		public static readonly string UIMiniMapPart = "partMiniSystem";
		public static readonly string UIInhabitantsPanel = "pnlPendingCombats";
		public static readonly string UIInhabitantsList = "lstCombats";
		public static readonly string UILocalPlayer = "pnlLocalPlayer";
		public static readonly string UIEncounterPlayer = "pnlEncounterPlayer";
		public static readonly string UILocalFleets = "pnlLocalFleets";
		public static readonly string UIEncounterFleets = "pnlEncounterFleets";
		public static readonly string UILocalFleetList = "lstLocalFleets";
		public static readonly string UIEncounterFleetList = "lstEncounterFleets";
		public static readonly string UIFleetList = "lstFleets";
		public static readonly string UIManualResolveButton = "btnManualResolve";
		public static readonly string UIAutoResolveButton = "btnAutoResolve";
		public static readonly string UIResponseResolveButton = "btnResponseResolve";
		public static readonly string UIAggressiveStanceButton = "btnAggressive";
		public static readonly string UIPassiveStanceButton = "btnPassive";
		public static readonly Dictionary<string, string> ResolutionIcons = new Dictionary<string, string>
		{

			{
				EncounterDialog.UIManualResolveButton,
				"imgManualResolve"
			},

			{
				EncounterDialog.UIAutoResolveButton,
				"imgAutoResolve"
			},

			{
				EncounterDialog.UIResponseResolveButton,
				"imgResponseResolve"
			}
		};
		public static readonly string UIEncounters = "lstEncounters";
		public static readonly string UIEncounterSystemName = "lblSystemName";
		public static readonly string UIStartCombatButton = "btnStartCombat";
		public static StarMap _starmap = null;
		private List<PendingCombat> _pendingCombats = new List<PendingCombat>();
		private PendingCombat _selectedCombat;
		private FleetWidget _preCombatLocalFleetWidget;
		private FleetWidget _preCombatEncounterFleetWidget;
		private FleetWidget _postCombatLocalFleetWidget;
		private FleetWidget _postCombatEncounterFleetWidget;
		private List<EncounterUIContainer> _EncounterUI = new List<EncounterUIContainer>();
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private int _allyIndex;
		private int _enemyIndex;
		private bool _visible = true;
		public EncounterDialog(App game, List<PendingCombat> PendingCombats) : base(game, "EncounterPopup")
		{
			this._pendingCombats.Clear();
			this._pendingCombats.AddRange(PendingCombats);
			if (this._pendingCombats.Any<PendingCombat>() && EncounterDialog._starmap.Systems != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._pendingCombats.First<PendingCombat>().SystemID))
			{
				this._app.GetGameState<StarMapState>().StarMap.SetFocus(EncounterDialog._starmap.Systems.Reverse[this._pendingCombats.First<PendingCombat>().SystemID]);
			}
			game.HotKeyManager.SetEnabled(false);
		}
		private void SetResolveType(string panel, ResolutionType type)
		{
			this.HideResolveImages(panel);
			if (!this._selectedCombat.CombatResolutionSelections.ContainsKey(this._app.LocalPlayer.ID))
			{
				this._selectedCombat.CombatResolutionSelections.Add(this._app.LocalPlayer.ID, type);
			}
			else
			{
				this._selectedCombat.CombatResolutionSelections[this._app.LocalPlayer.ID] = type;
			}
			switch (type)
			{
			case ResolutionType.FIGHT:
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					panel,
					EncounterDialog.ResolutionIcons[EncounterDialog.UIManualResolveButton]
				}), true);
				break;
			case ResolutionType.AUTO_RESOLVE:
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					panel,
					EncounterDialog.ResolutionIcons[EncounterDialog.UIAutoResolveButton]
				}), true);
				break;
			case ResolutionType.FIGHT_ON_FIGHT:
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					panel,
					EncounterDialog.ResolutionIcons[EncounterDialog.UIResponseResolveButton]
				}), true);
				break;
			}
			EncounterUIContainer cont = this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x._combat.ConflictID == this._selectedCombat.ConflictID);
			this.UpdateCombatListResolveButtons(cont, type);
		}
		private void HideResolveImages(string panel)
		{
			foreach (string current in EncounterDialog.ResolutionIcons.Values)
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					panel,
					current
				}), false);
			}
		}
		private void SetStanceType(string panel, AutoResolveStance stance)
		{
			if (!this._selectedCombat.CombatStanceSelections.ContainsKey(this._app.LocalPlayer.ID))
			{
				this._selectedCombat.CombatStanceSelections.Add(this._app.LocalPlayer.ID, stance);
			}
			else
			{
				this._selectedCombat.CombatStanceSelections[this._app.LocalPlayer.ID] = stance;
			}
			switch (stance)
			{
			case AutoResolveStance.PASSIVE:
				this._app.UI.SetChecked(this._app.UI.Path(new string[]
				{
					panel,
					EncounterDialog.UIAggressiveStanceButton
				}), false);
				this._app.UI.SetChecked(this._app.UI.Path(new string[]
				{
					panel,
					EncounterDialog.UIPassiveStanceButton
				}), true);
				return;
			case AutoResolveStance.AGGRESSIVE:
				this._app.UI.SetChecked(this._app.UI.Path(new string[]
				{
					panel,
					EncounterDialog.UIPassiveStanceButton
				}), false);
				this._app.UI.SetChecked(this._app.UI.Path(new string[]
				{
					panel,
					EncounterDialog.UIAggressiveStanceButton
				}), true);
				return;
			default:
				return;
			}
		}
		protected override void OnUpdate()
		{
			this._app.HotKeyManager.SetEnabled(false);
			if (this._app.CurrentState.Name != "StarMapState")
			{
				if (this._visible)
				{
					base.SetVisible(false);
					this._app.UI.Send(new object[]
					{
						"PopFocus",
						base.ID
					});
					this._visible = false;
				}
			}
			else
			{
				if (!this._visible)
				{
					base.SetVisible(true);
					this._app.UI.Send(new object[]
					{
						"PushFocus",
						base.ID
					});
					this._visible = true;
				}
			}
			this._app.GetGameState<StarMapState>().ShowInterface = !this._visible;
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Update();
			}
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "dialog_opened"))
			{
				if (msgType == "list_sel_changed")
				{
					if (panelName == EncounterDialog.UIEncounters)
					{
						this._selectedCombat = this._pendingCombats.First((PendingCombat x) => x.SystemID == int.Parse(msgParams[0]));
						if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
						{
							EncounterDialog._starmap.SetFocus(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
							EncounterDialog._starmap.Select(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
						}
						this._allyIndex = 0;
						this._enemyIndex = 0;
						this.SyncEncounterPopup(false, false);
					}
				}
				else
				{
					if (msgType == "button_clicked")
					{
						if (panelName == EncounterDialog.UIManualResolveButton)
						{
							this.SetResolveType(base.ID, ResolutionType.FIGHT);
						}
						else
						{
							if (panelName == EncounterDialog.UIAutoResolveButton)
							{
								this.SetResolveType(base.ID, ResolutionType.AUTO_RESOLVE);
							}
							else
							{
								if (panelName == EncounterDialog.UIResponseResolveButton)
								{
									this.SetResolveType(base.ID, ResolutionType.FIGHT_ON_FIGHT);
								}
								else
								{
									if (panelName == EncounterDialog.UIAggressiveStanceButton)
									{
										this.SetStanceType(base.ID, AutoResolveStance.AGGRESSIVE);
									}
									else
									{
										if (panelName == EncounterDialog.UIPassiveStanceButton)
										{
											this.SetStanceType(base.ID, AutoResolveStance.PASSIVE);
										}
										else
										{
											if (panelName == "btnCombatManager")
											{
												this._app.SwitchGameState<DefenseManagerState>(new object[]
												{
													this._selectedCombat.SystemID
												});
											}
											else
											{
												if (panelName == EncounterDialog.UIStartCombatButton)
												{
													this._app.GetGameState<StarMapState>().ShowInterface = true;
													this._app.UI.CloseDialog(this, true);
													bool flag = this._app.Game.GetPendingCombats().Any((PendingCombat x) => x.CombatResults == null);
													if (flag)
													{
														this._app.Game.OrderCombatsByResponse();
														if (this._app.GameSetup.IsMultiplayer)
														{
															this._app.Network.SendCombatResponses(this._pendingCombats, this._app.LocalPlayer.ID);
														}
														else
														{
															this._app.Game.LaunchNextCombat();
														}
													}
													else
													{
														if (!this._app.GameSetup.IsMultiplayer)
														{
															this._app.Game.GetPendingCombats().Clear();
															this._app.Game.NextTurn();
														}
													}
												}
												else
												{
													if (panelName == EncounterDialog.UIPreviousAlly)
													{
														this._allyIndex--;
														if (this._selectedCombat.CombatResults != null)
														{
															this.SyncPostCombatPanel(this._app, EncounterDialog.UIPostCombatSummaryPanel, this._selectedCombat, false, false);
														}
														else
														{
															this.SyncEncounterPanel(this._app, EncounterDialog.UIEncounterSummaryPanel, this._selectedCombat);
														}
													}
													else
													{
														if (panelName == EncounterDialog.UINextAlly)
														{
															this._allyIndex++;
															if (this._selectedCombat.CombatResults != null)
															{
																this.SyncPostCombatPanel(this._app, EncounterDialog.UIPostCombatSummaryPanel, this._selectedCombat, false, false);
															}
															else
															{
																this.SyncEncounterPanel(this._app, EncounterDialog.UIEncounterSummaryPanel, this._selectedCombat);
															}
														}
														else
														{
															if (panelName == EncounterDialog.UIPreviousEnemy)
															{
																this._enemyIndex--;
																if (this._selectedCombat.CombatResults != null)
																{
																	this.SyncPostCombatPanel(this._app, EncounterDialog.UIPostCombatSummaryPanel, this._selectedCombat, false, false);
																}
																else
																{
																	this.SyncEncounterPanel(this._app, EncounterDialog.UIEncounterSummaryPanel, this._selectedCombat);
																}
															}
															else
															{
																if (panelName == EncounterDialog.UINextEnemy)
																{
																	this._enemyIndex++;
																	if (this._selectedCombat.CombatResults != null)
																	{
																		this.SyncPostCombatPanel(this._app, EncounterDialog.UIPostCombatSummaryPanel, this._selectedCombat, false, false);
																	}
																	else
																	{
																		this.SyncEncounterPanel(this._app, EncounterDialog.UIEncounterSummaryPanel, this._selectedCombat);
																	}
																}
																else
																{
																	if (this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x.SystemButtonID == panelName)._panels != null)
																	{
																		EncounterUIContainer encounterUIContainer = this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x.SystemButtonID == panelName && x.NumEnounters == 1);
																		if (this._selectedCombat.SystemID == encounterUIContainer._combat.SystemID)
																		{
																			int targetcombat = this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x._combat == this._selectedCombat).NumEnounters + 1;
																			if (targetcombat > 3)
																			{
																				targetcombat = 1;
																			}
																			EncounterUIContainer encounterUIContainer2 = this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x._combat.SystemID == this._selectedCombat.SystemID && x.NumEnounters == targetcombat);
																			if (encounterUIContainer2._panels == null)
																			{
																				encounterUIContainer2 = encounterUIContainer;
																			}
																			if (this._selectedCombat != encounterUIContainer2._combat)
																			{
																				this._selectedCombat = encounterUIContainer2._combat;
																				if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
																				{
																					EncounterDialog._starmap.SetFocus(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
																					EncounterDialog._starmap.Select(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
																				}
																				this._allyIndex = 0;
																				this._enemyIndex = 0;
																				this.SyncEncounterPopup(false, false);
																			}
																		}
																		else
																		{
																			if (this._selectedCombat != encounterUIContainer._combat)
																			{
																				this._selectedCombat = encounterUIContainer._combat;
																				if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
																				{
																					EncounterDialog._starmap.SetFocus(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
																					EncounterDialog._starmap.Select(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
																				}
																				this._allyIndex = 0;
																				this._enemyIndex = 0;
																				this.SyncEncounterPopup(false, false);
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
			foreach (EncounterUIContainer current in this._EncounterUI)
			{
				if (current._panels != null)
				{
					PanelBinding.TryPanelMessage(current._panels, panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self);
				}
			}
		}
		public override void Initialize()
		{
			base.Initialize();
			foreach (PendingCombat current in this._pendingCombats)
			{
				if (!current.CombatResolutionSelections.ContainsKey(this._app.LocalPlayer.ID))
				{
					current.CombatResolutionSelections.Add(this._app.LocalPlayer.ID, ResolutionType.FIGHT);
				}
				if (!current.CombatStanceSelections.ContainsKey(this._app.LocalPlayer.ID))
				{
					current.CombatStanceSelections.Add(this._app.LocalPlayer.ID, AutoResolveStance.AGGRESSIVE);
				}
				if (!current.SelectedPlayerFleets.ContainsKey(this._app.LocalPlayer.ID))
				{
					int value = 0;
					List<FleetInfo> list = this._app.GameDatabase.GetFleetsByPlayerAndSystem(this._app.LocalPlayer.ID, current.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
					if (list != null && list.Count > 0)
					{
						value = list.First<FleetInfo>().ID;
					}
					current.SelectedPlayerFleets.Add(this._app.LocalPlayer.ID, value);
				}
			}
			this._preCombatLocalFleetWidget = new FleetWidget(this._app, this._app.UI.Path(new string[]
			{
				EncounterDialog.UIEncounterSummaryPanel,
				EncounterDialog.UILocalFleets,
				EncounterDialog.UILocalFleetList
			}));
			this._preCombatLocalFleetWidget.EnableAdmiralButton = false;
			this._preCombatLocalFleetWidget.EnableRightClick = false;
			this._preCombatEncounterFleetWidget = new FleetWidget(this._app, this._app.UI.Path(new string[]
			{
				EncounterDialog.UIEncounterSummaryPanel,
				EncounterDialog.UIEncounterFleets,
				EncounterDialog.UIEncounterFleetList
			}));
			this._preCombatEncounterFleetWidget.SetEnabled(false);
			this._preCombatEncounterFleetWidget.ShowPiracyFleets = true;
			this._preCombatEncounterFleetWidget.EnableRightClick = false;
			this._preCombatEncounterFleetWidget.EnableAdmiralButton = false;
			this._postCombatLocalFleetWidget = new FleetWidget(this._app, this._app.UI.Path(new string[]
			{
				EncounterDialog.UIPostCombatSummaryPanel,
				EncounterDialog.UILocalFleets,
				EncounterDialog.UIFleetList
			}));
			this._postCombatLocalFleetWidget.EnableAdmiralButton = false;
			this._postCombatLocalFleetWidget.EnableRightClick = false;
			this._postCombatEncounterFleetWidget = new FleetWidget(this._app, this._app.UI.Path(new string[]
			{
				EncounterDialog.UIPostCombatSummaryPanel,
				EncounterDialog.UIEncounterFleets,
				EncounterDialog.UIFleetList
			}));
			this._postCombatEncounterFleetWidget.SetEnabled(false);
			this._postCombatEncounterFleetWidget.ShowPiracyFleets = true;
			this._postCombatEncounterFleetWidget.EnableRightClick = false;
			this._postCombatEncounterFleetWidget.EnableAdmiralButton = false;
			this.SyncEncounterPopup(true, true);
		}
		public override string[] CloseDialog()
		{
			foreach (SystemWidget current in this._systemWidgets)
			{
				current.Terminate();
			}
			if (this._selectedCombat.FleetIDs.Contains(this._preCombatLocalFleetWidget.SelectedFleet))
			{
				this._selectedCombat.SelectedPlayerFleets[this._app.LocalPlayer.ID] = this._preCombatLocalFleetWidget.SelectedFleet;
			}
			this._postCombatEncounterFleetWidget.Dispose();
			this._postCombatLocalFleetWidget.Dispose();
			this._preCombatEncounterFleetWidget.Dispose();
			this._preCombatLocalFleetWidget.Dispose();
			return null;
		}
		public void UpdateCombatListResolveButtons(EncounterUIContainer cont, ResolutionType type)
		{
			cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID] = type;
			PanelBinding[] panels = cont._panels;
			for (int i = 0; i < panels.Length; i++)
			{
				Button button = (Button)panels[i];
				if (button.ID.Contains("Stance"))
				{
					this._app.UI.SetVisible(button.ID, false);
				}
			}
			switch (cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID])
			{
			case ResolutionType.FIGHT:
				this._app.UI.SetVisible(cont._panels.FirstOrDefault((PanelBinding x) => x.ID.Contains("Stancem")).ID, true);
				return;
			case ResolutionType.AUTO_RESOLVE:
				this._app.UI.SetVisible(cont._panels.FirstOrDefault((PanelBinding x) => x.ID.Contains("Stancea")).ID, true);
				return;
			case ResolutionType.FIGHT_ON_FIGHT:
				this._app.UI.SetVisible(cont._panels.FirstOrDefault((PanelBinding x) => x.ID.Contains("Stancer")).ID, true);
				return;
			default:
				return;
			}
		}
		private void CycleStanceOnCombat(object sender, EventArgs e)
		{
			EncounterUIContainer cont = this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x._panels.FirstOrDefault((PanelBinding k) => k.ID == ((Button)sender).ID) != null);
			if (!cont._combat.CombatResolutionSelections.ContainsKey(this._app.LocalPlayer.ID))
			{
				cont._combat.CombatResolutionSelections.Add(this._app.LocalPlayer.ID, ResolutionType.FIGHT);
			}
			else
			{
				switch (cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID])
				{
				case ResolutionType.FIGHT:
					cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID] = ResolutionType.AUTO_RESOLVE;
					break;
				case ResolutionType.AUTO_RESOLVE:
					cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID] = ResolutionType.FIGHT_ON_FIGHT;
					break;
				case ResolutionType.FIGHT_ON_FIGHT:
					cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID] = ResolutionType.FIGHT;
					break;
				}
			}
			this.UpdateCombatListResolveButtons(cont, cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID]);
			if (this._selectedCombat == cont._combat)
			{
				this.SetResolveType(base.ID, cont._combat.CombatResolutionSelections[this._app.LocalPlayer.ID]);
			}
		}
		private void SelectCombat(object sender, EventArgs e)
		{
			this._selectedCombat = this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x._panels.FirstOrDefault((PanelBinding k) => k.ID == ((Button)sender).ID) != null)._combat;
			if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
			{
				EncounterDialog._starmap.SetFocus(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
				EncounterDialog._starmap.Select(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
			}
			this._allyIndex = 0;
			this._enemyIndex = 0;
			this.SyncEncounterPopup(false, false);
		}
		public void SyncEncounterList(App game, string panelName, List<PendingCombat> pendingCombats)
		{
			game.UI.ClearItems(EncounterDialog.UIEncounters);
			foreach (PendingCombat pc in pendingCombats)
			{
				if (pc.PlayersInCombat.Contains(this._app.LocalPlayer.ID))
				{
					EncounterUIContainer item = default(EncounterUIContainer);
					EncounterUIContainer encounterUIContainer = this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x._combat.SystemID == pc.SystemID);
					if (encounterUIContainer._panels == null)
					{
						game.UI.AddItem(game.UI.Path(new string[]
						{
							panelName,
							EncounterDialog.UIInhabitantsPanel,
							EncounterDialog.UIInhabitantsList
						}), "", pc.SystemID, "");
						string itemGlobalID = this._app.UI.GetItemGlobalID(game.UI.Path(new string[]
						{
							panelName,
							EncounterDialog.UIInhabitantsPanel,
							EncounterDialog.UIInhabitantsList
						}), "", pc.SystemID, "");
						StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(pc.SystemID);
						if (starSystemInfo == null || starSystemInfo.IsDeepSpace)
						{
							string globalID = this._app.UI.GetGlobalID(game.UI.Path(new string[]
							{
								itemGlobalID,
								"systemDeepspace"
							}));
							this._app.UI.SetVisible(globalID, true);
							this._app.UI.SetText(game.UI.Path(new string[]
							{
								itemGlobalID,
								"title"
							}), starSystemInfo.Name);
						}
						else
						{
							this._systemWidgets.Add(new SystemWidget(this._app, itemGlobalID));
							this._systemWidgets.Last<SystemWidget>().Sync(pc.SystemID);
							List<HomeworldInfo> source = this._app.GameDatabase.GetHomeworlds().ToList<HomeworldInfo>();
							HomeworldInfo homeworldInfo = source.FirstOrDefault((HomeworldInfo x) => x.SystemID == pc.SystemID);
							int? systemOwningPlayer = this._app.GameDatabase.GetSystemOwningPlayer(pc.SystemID);
							PlayerInfo Owner = this._app.GameDatabase.GetPlayerInfo(systemOwningPlayer.HasValue ? systemOwningPlayer.Value : 0);
							if (homeworldInfo != null && homeworldInfo.SystemID != 0)
							{
								string globalID2 = this._app.UI.GetGlobalID(game.UI.Path(new string[]
								{
									itemGlobalID,
									"systemHome"
								}));
								this._app.UI.SetVisible(globalID2, true);
								this._app.UI.SetPropertyColor(globalID2, "color", this._app.GameDatabase.GetPlayerInfo(homeworldInfo.PlayerID).PrimaryColor * 255f);
							}
							else
							{
								if (Owner != null && (
									from x in game.GameDatabase.GetProvinceInfos()
									where x.CapitalSystemID == pc.SystemID && x.PlayerID == Owner.ID && x.CapitalSystemID != Owner.Homeworld
									select x).Any<ProvinceInfo>())
								{
									string globalID3 = this._app.UI.GetGlobalID(game.UI.Path(new string[]
									{
										itemGlobalID,
										"systemCapital"
									}));
									this._app.UI.SetVisible(globalID3, true);
									this._app.UI.SetPropertyColor(globalID3, "color", Owner.PrimaryColor * 255f);
								}
								else
								{
									string globalID4 = this._app.UI.GetGlobalID(game.UI.Path(new string[]
									{
										itemGlobalID,
										"systemOwnership"
									}));
									this._app.UI.SetVisible(globalID4, true);
									if (Owner != null)
									{
										this._app.UI.SetPropertyColor(globalID4, "color", Owner.PrimaryColor * 255f);
									}
								}
							}
						}
						item.CombatListID = game.UI.Path(new string[]
						{
							itemGlobalID,
							"lstcombatinstances"
						});
						item.SystemItemID = itemGlobalID;
						item.NumEnounters = pc.CardID;
						string panelId = game.UI.Path(new string[]
						{
							itemGlobalID,
							"systemButton"
						});
						this._app.UI.SetPropertyString(panelId, "id", "SelectSystem:" + pc.SystemID.ToString());
						item.SystemButtonID = "SelectSystem:" + pc.SystemID.ToString();
					}
					else
					{
						item.CombatListID = encounterUIContainer.CombatListID;
						item.SystemItemID = encounterUIContainer.SystemItemID;
						item.SystemButtonID = encounterUIContainer.SystemItemID;
						item.NumEnounters = pc.CardID;
					}
					game.UI.AddItem(item.CombatListID, "", 999 * item.NumEnounters, "");
					string itemGlobalID2 = this._app.UI.GetItemGlobalID(game.UI.Path(new string[]
					{
						item.CombatListID,
						"lstcombatinstances"
					}), "", 999 * item.NumEnounters, "");
					string globalID5 = this._app.UI.GetGlobalID(game.UI.Path(new string[]
					{
						itemGlobalID2,
						"btnManualResolve"
					}));
					string globalID6 = this._app.UI.GetGlobalID(game.UI.Path(new string[]
					{
						itemGlobalID2,
						"btnAutoResolve"
					}));
					string globalID7 = this._app.UI.GetGlobalID(game.UI.Path(new string[]
					{
						itemGlobalID2,
						"btnEnemyResolve"
					}));
					item.InstancePanel = itemGlobalID2;
					Button button = new Button(game.UI, globalID5, null);
					button.SetID(string.Concat(new string[]
					{
						"Stancem(sid:",
						pc.SystemID.ToString(),
						"xcid:",
						item.NumEnounters.ToString(),
						")"
					}));
					button.Clicked += new EventHandler(this.CycleStanceOnCombat);
					this._app.UI.SetPropertyString(globalID5, "id", string.Concat(new string[]
					{
						"Stancem(sid:",
						pc.SystemID.ToString(),
						"xcid:",
						item.NumEnounters.ToString(),
						")"
					}));
					Button button2 = new Button(game.UI, globalID6, null);
					button2.SetID(string.Concat(new string[]
					{
						"Stancea(sid:",
						pc.SystemID.ToString(),
						"xcid:",
						item.NumEnounters.ToString(),
						")"
					}));
					button2.Clicked += new EventHandler(this.CycleStanceOnCombat);
					this._app.UI.SetPropertyString(globalID6, "id", string.Concat(new string[]
					{
						"Stancea(sid:",
						pc.SystemID.ToString(),
						"xcid:",
						item.NumEnounters.ToString(),
						")"
					}));
					Button button3 = new Button(game.UI, globalID7, null);
					button3.SetID(string.Concat(new string[]
					{
						"Stancer(sid:",
						pc.SystemID.ToString(),
						"xcid:",
						item.NumEnounters.ToString(),
						")"
					}));
					button3.Clicked += new EventHandler(this.CycleStanceOnCombat);
					this._app.UI.SetPropertyString(globalID7, "id", string.Concat(new string[]
					{
						"Stancer(sid:",
						pc.SystemID.ToString(),
						"xcid:",
						item.NumEnounters.ToString(),
						")"
					}));
					string globalID8 = this._app.UI.GetGlobalID(game.UI.Path(new string[]
					{
						itemGlobalID2,
						"CombatSelButton"
					}));
					Button button4 = new Button(game.UI, globalID8, null);
					button4.SetID(string.Concat(new string[]
					{
						"Combat(sid:",
						pc.SystemID.ToString(),
						"xcid:",
						item.NumEnounters.ToString(),
						")"
					}));
					button4.Clicked += new EventHandler(this.SelectCombat);
					this._app.UI.SetPropertyString(globalID8, "id", string.Concat(new string[]
					{
						"Combat(sid:",
						pc.SystemID.ToString(),
						"xcid:",
						item.NumEnounters.ToString(),
						")"
					}));
					game.UI.SetText(game.UI.Path(new string[]
					{
						itemGlobalID2,
						"combatnum"
					}), item.NumEnounters.ToString());
					item._combat = pc;
					item._panels = new PanelBinding[]
					{
						button,
						button2,
						button3,
						button4
					};
					this._EncounterUI.Insert(this._EncounterUI.Count, item);
				}
			}
			this._selectedCombat = pendingCombats.FirstOrDefault((PendingCombat x) => x.CombatResults == null);
			if (this._selectedCombat == null)
			{
				this._selectedCombat = pendingCombats.Last<PendingCombat>();
			}
			game.UI.SetSelection(EncounterDialog.UIEncounters, this._selectedCombat.SystemID);
		}
		public void OnLocalFleetChanged(App game, int selectedFleet)
		{
			List<int> list = (
				from x in this._selectedCombat.PlayersInCombat
				where x == game.Game.LocalPlayer.ID || game.GameDatabase.GetDiplomacyStateBetweenPlayers(game.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED
				select x).ToList<int>();
			int? num = null;
			if (list.Count > 1)
			{
				this._allyIndex = Math.Min(Math.Max(this._allyIndex, 0), list.Count);
				num = ((this._allyIndex == 0) ? null : (new int?(this._allyIndex) - 1));
			}
			else
			{
				num = new int?(0);
			}
			if (!num.HasValue || list[num.Value] == game.LocalPlayer.ID)
			{
				this._selectedCombat.SelectedPlayerFleets[game.LocalPlayer.ID] = selectedFleet;
			}
		}
		public void SyncEncounterPanel(App game, string panelName, PendingCombat pendingCombat)
		{
			if (!pendingCombat.PlayersInCombat.Contains(this._app.LocalPlayer.ID))
			{
				this._selectedCombat = this._pendingCombats.First((PendingCombat x) => x.PlayersInCombat.Contains(this._app.LocalPlayer.ID));
				pendingCombat = this._selectedCombat;
			}
			foreach (EncounterUIContainer current in this._EncounterUI)
			{
				this._app.UI.SetVisible(game.UI.Path(new string[]
				{
					current.SystemItemID,
					"selectionoverlay"
				}), false);
				this._app.UI.SetVisible(game.UI.Path(new string[]
				{
					current.InstancePanel,
					"selectionoverlay"
				}), false);
			}
			EncounterUIContainer encounterUIContainer = this._EncounterUI.FirstOrDefault((EncounterUIContainer x) => x._combat == pendingCombat);
			if (encounterUIContainer._panels != null)
			{
				this._app.UI.SetVisible(game.UI.Path(new string[]
				{
					encounterUIContainer.SystemItemID,
					"selectionoverlay"
				}), true);
				this._app.UI.SetVisible(game.UI.Path(new string[]
				{
					encounterUIContainer.InstancePanel,
					"selectionoverlay"
				}), true);
			}
			List<int> AlliedPlayers = (
				from x in pendingCombat.PlayersInCombat
				where x == game.Game.LocalPlayer.ID || game.GameDatabase.GetDiplomacyStateBetweenPlayers(game.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED
				select x).ToList<int>();
			if (pendingCombat.Type == CombatType.CT_Piracy)
			{
				foreach (int current2 in pendingCombat.FleetIDs)
				{
					FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(current2);
					MissionInfo missionByFleetID = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
					if (missionByFleetID != null && missionByFleetID.Type == MissionType.PIRACY)
					{
						if (fleetInfo.PlayerID == game.LocalPlayer.ID)
						{
							AlliedPlayers.Clear();
							AlliedPlayers.Add(game.Game.LocalPlayer.ID);
						}
						else
						{
							AlliedPlayers.Remove(fleetInfo.PlayerID);
						}
					}
				}
			}
			List<int> EnemyPlayers = (
				from x in pendingCombat.PlayersInCombat
				where !AlliedPlayers.Contains(x)
				select x).ToList<int>();
			int? selectedAlly = null;
			int? selectedEnemy = null;
			if (AlliedPlayers.Count > 1)
			{
				this._allyIndex = Math.Min(Math.Max(this._allyIndex, 0), AlliedPlayers.Count);
				selectedAlly = ((this._allyIndex == 0) ? null : (new int?(this._allyIndex) - 1));
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UINextAlly
				}), this._allyIndex < AlliedPlayers.Count);
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIPreviousAlly
				}), this._allyIndex >= 1);
			}
			else
			{
				selectedAlly = new int?(0);
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UINextAlly
				}), false);
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIPreviousAlly
				}), false);
			}
			this._app.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				EncounterDialog.UINextAlly
			}), AlliedPlayers.Count > 0);
			this._app.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				EncounterDialog.UIPreviousAlly
			}), AlliedPlayers.Count > 0);
			if (EnemyPlayers.Count > 0)
			{
				if (EnemyPlayers.Count > 1)
				{
					this._enemyIndex = Math.Min(Math.Max(this._enemyIndex, 0), EnemyPlayers.Count);
					selectedEnemy = ((this._enemyIndex == 0) ? null : (new int?(this._enemyIndex) - 1));
					this._app.UI.SetEnabled(game.UI.Path(new string[]
					{
						panelName,
						EncounterDialog.UINextEnemy
					}), this._enemyIndex < EnemyPlayers.Count);
					this._app.UI.SetEnabled(game.UI.Path(new string[]
					{
						panelName,
						EncounterDialog.UIPreviousEnemy
					}), this._enemyIndex >= 1);
				}
				else
				{
					selectedEnemy = new int?(0);
					this._app.UI.SetEnabled(game.UI.Path(new string[]
					{
						panelName,
						EncounterDialog.UINextEnemy
					}), false);
					this._app.UI.SetEnabled(game.UI.Path(new string[]
					{
						panelName,
						EncounterDialog.UIPreviousEnemy
					}), false);
				}
			}
			this._app.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				EncounterDialog.UINextEnemy
			}), EnemyPlayers.Count > 0);
			this._app.UI.SetVisible(game.UI.Path(new string[]
			{
				panelName,
				EncounterDialog.UIPreviousEnemy
			}), EnemyPlayers.Count > 0);
			if (!selectedAlly.HasValue || AlliedPlayers[selectedAlly.Value] == game.LocalPlayer.ID)
			{
				FleetWidget expr_7D9 = this._preCombatLocalFleetWidget;
				expr_7D9.OnFleetSelectionChanged = (FleetWidget.FleetSelectionChangedDelegate)Delegate.Combine(expr_7D9.OnFleetSelectionChanged, new FleetWidget.FleetSelectionChangedDelegate(this.OnLocalFleetChanged));
				this._preCombatLocalFleetWidget.Selected = pendingCombat.SelectedPlayerFleets[this._app.LocalPlayer.ID];
			}
			else
			{
				FleetWidget expr_82E = this._preCombatLocalFleetWidget;
				expr_82E.OnFleetSelectionChanged = (FleetWidget.FleetSelectionChangedDelegate)Delegate.Remove(expr_82E.OnFleetSelectionChanged, new FleetWidget.FleetSelectionChangedDelegate(this.OnLocalFleetChanged));
				this._preCombatLocalFleetWidget.Selected = -1;
			}
			this.SetResolveType(base.ID, pendingCombat.CombatResolutionSelections[game.LocalPlayer.ID]);
			this.SetStanceType(base.ID, pendingCombat.CombatStanceSelections[game.LocalPlayer.ID]);
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(pendingCombat.SystemID);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				base.ID,
				"systemHomemain"
			}), false);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				base.ID,
				"systemCapitalmain"
			}), false);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				base.ID,
				"systemOwnershipmain"
			}), false);
			game.UI.SetVisible(game.UI.Path(new string[]
			{
				base.ID,
				"systemDeepspacemain"
			}), false);
			if (starSystemInfo == null || starSystemInfo.IsDeepSpace)
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					base.ID,
					"btnCombatManager"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					base.ID,
					"systemDeepspacemain"
				}), true);
			}
			else
			{
				int? systemOwningPlayer = game.GameDatabase.GetSystemOwningPlayer(pendingCombat.SystemID);
				PlayerInfo SystemOwner = game.GameDatabase.GetPlayerInfo(systemOwningPlayer.HasValue ? systemOwningPlayer.Value : 0);
				if (SystemOwner != null)
				{
					HomeworldInfo homeworldInfo = this._app.GameDatabase.GetHomeworlds().FirstOrDefault((HomeworldInfo x) => x.SystemID == pendingCombat.SystemID && x.PlayerID == SystemOwner.ID);
					Vector3 value = SystemOwner.PrimaryColor * 255f;
					if (homeworldInfo != null && homeworldInfo.SystemID == pendingCombat.SystemID)
					{
						game.UI.SetVisible(game.UI.Path(new string[]
						{
							base.ID,
							"systemHomemain"
						}), true);
						game.UI.SetPropertyColor(game.UI.Path(new string[]
						{
							base.ID,
							"systemHomemain"
						}), "color", value);
					}
					else
					{
						if ((
							from x in game.GameDatabase.GetProvinceInfos()
							where x.PlayerID == SystemOwner.ID && x.CapitalSystemID == pendingCombat.SystemID && x.CapitalSystemID != SystemOwner.Homeworld
							select x).Any<ProvinceInfo>())
						{
							game.UI.SetVisible(game.UI.Path(new string[]
							{
								base.ID,
								"systemCapitalmain"
							}), true);
							game.UI.SetPropertyColor(game.UI.Path(new string[]
							{
								base.ID,
								"systemCapitalmain"
							}), "color", value);
						}
						else
						{
							game.UI.SetVisible(game.UI.Path(new string[]
							{
								base.ID,
								"systemOwnershipmain"
							}), true);
							game.UI.SetPropertyColor(game.UI.Path(new string[]
							{
								base.ID,
								"systemOwnershipmain"
							}), "color", value);
						}
					}
				}
				if (game.Game.IsMultiplayer)
				{
					game.UI.SetEnabled(game.UI.Path(new string[]
					{
						base.ID,
						"btnCombatManager"
					}), false);
					game.UI.SetTooltip(game.UI.Path(new string[]
					{
						base.ID,
						"btnCombatManager"
					}), "Combat Manager Disabled in Multiplayer");
				}
				else
				{
					game.UI.SetEnabled(game.UI.Path(new string[]
					{
						base.ID,
						"btnCombatManager"
					}), true);
					game.UI.SetTooltip(game.UI.Path(new string[]
					{
						base.ID,
						"btnCombatManager"
					}), "");
				}
			}
			List<ColonyInfo> list = game.GameDatabase.GetColonyInfosForSystem(pendingCombat.SystemID).ToList<ColonyInfo>();
			List<StationInfo> list2 = game.GameDatabase.GetStationForSystem(pendingCombat.SystemID).ToList<StationInfo>();
			List<int> list3 = new List<int>();
			foreach (StationInfo current3 in list2)
			{
				if (!list3.Contains(current3.PlayerID))
				{
					list3.Add(current3.PlayerID);
				}
			}
			foreach (ColonyInfo current4 in list)
			{
				if (!list3.Contains(current4.PlayerID))
				{
					list3.Add(current4.PlayerID);
				}
			}
			List<FleetInfo> source = game.GameDatabase.GetFleetInfoBySystemID(pendingCombat.SystemID, FleetType.FL_NORMAL).ToList<FleetInfo>();
			StarSystemMapUI.Sync(game, pendingCombat.SystemID, game.UI.Path(new string[]
			{
				panelName,
				EncounterDialog.UIMiniMapPanel,
				EncounterDialog.UIMiniMapPart
			}), false);
			if (AlliedPlayers.Count > 1 && !selectedAlly.HasValue)
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UILocalPlayer
				}), App.Localize("@UI_POST_COMBAT_ALLIANCE"), App.Localize("@DIPLO_REACTION_LOVE"), "", "");
				this._preCombatLocalFleetWidget.SetSyncedFleets((
					from x in source
                    where AlliedPlayers.Contains(x.PlayerID) && this._selectedCombat.FleetIDs.Contains(x.ID) && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game.Game, x)
					select x).ToList<FleetInfo>());
			}
			else
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UILocalPlayer
				}), AlliedPlayers[selectedAlly.Value]);
				this._preCombatLocalFleetWidget.SetSyncedFleets((
					from x in source
                    where x.PlayerID == AlliedPlayers[selectedAlly.Value] && this._selectedCombat.FleetIDs.Contains(x.ID) && !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game.Game, x)
					select x).ToList<FleetInfo>());
			}
			if (EnemyPlayers.Count > 0)
			{
				if (EnemyPlayers.Count > 1 && !selectedEnemy.HasValue)
				{
					StarmapUI.SyncPlayerCard(game, game.UI.Path(new string[]
					{
						panelName,
						EncounterDialog.UIEncounterPlayer
					}), App.Localize("@UI_POST_COMBAT_ENEMIES"), App.Localize("@DIPLO_REACTION_HATE"), "", "");
					this._preCombatEncounterFleetWidget.SetSyncedFleets((
						from x in source
						where EnemyPlayers.Contains(x.PlayerID) && this._selectedCombat.FleetIDs.Contains(x.ID)
						select x).ToList<FleetInfo>());
				}
				else
				{
					int num = EnemyPlayers[selectedEnemy.Value];
					List<int> piracyfleet = new List<int>();
					if (this._selectedCombat.Type == CombatType.CT_Piracy)
					{
						foreach (int current5 in this._selectedCombat.FleetIDs)
						{
							FleetInfo fleetInfo2 = game.GameDatabase.GetFleetInfo(current5);
							if (fleetInfo2.PlayerID == num)
							{
								MissionInfo missionByFleetID2 = game.GameDatabase.GetMissionByFleetID(fleetInfo2.ID);
								if (missionByFleetID2 != null && missionByFleetID2.Type == MissionType.PIRACY && !game.GameDatabase.PirateFleetVisibleToPlayer(current5, game.LocalPlayer.ID))
								{
									piracyfleet.Add(current5);
									break;
								}
							}
						}
					}
					StarmapUI.SyncPlayerCard(game, game.UI.Path(new string[]
					{
						panelName,
						EncounterDialog.UIEncounterPlayer
					}), (!piracyfleet.Any<int>()) ? EnemyPlayers[selectedEnemy.Value] : game.Game.ScriptModules.Pirates.PlayerID);
					this._preCombatEncounterFleetWidget.SetSyncedFleets(source.Where(delegate(FleetInfo x)
					{
						if (x.PlayerID != EnemyPlayers[selectedEnemy.Value])
						{
							return false;
						}
						if (piracyfleet.Any<int>())
						{
							return piracyfleet.Contains(x.ID);
						}
						return this._selectedCombat.FleetIDs.Contains(x.ID);
					}).ToList<FleetInfo>());
				}
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				EncounterDialog.UIEncounterTitle
			}), "text", string.Format("{0} {1}", App.Localize("@UI_STARMAP_ENCOUNTER_AT"), starSystemInfo.Name));
		}
		public void SyncEncounterPopup(bool playSpeech, bool refreshList = true)
		{
			if (refreshList)
			{
				this.SyncEncounterList(this._app, this._app.UI.Path(new string[]
				{
					base.ID,
					EncounterDialog.UIEncounterListPanel
				}), this._pendingCombats);
			}
			if (this._selectedCombat == null)
			{
				this._selectedCombat = this._pendingCombats.First((PendingCombat x) => x.PlayersInCombat.Contains(this._app.LocalPlayer.ID));
				if (EncounterDialog._starmap != null && EncounterDialog._starmap.Systems.Reverse.ContainsKey(this._selectedCombat.SystemID))
				{
					EncounterDialog._starmap.SetFocus(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
					EncounterDialog._starmap.Select(EncounterDialog._starmap.Systems.Reverse[this._selectedCombat.SystemID]);
				}
				this._allyIndex = 0;
				this._enemyIndex = 0;
				this.SyncEncounterPopup(false, false);
			}
			if (this._selectedCombat.CombatResults == null)
			{
				string text = this._app.UI.Path(new string[]
				{
					base.ID,
					EncounterDialog.UIEncounterSummaryPanel
				});
				this._app.UI.SetVisible(text, true);
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					EncounterDialog.UIPostCombatSummaryPanel
				}), false);
				this.SyncEncounterPanel(this._app, text, this._selectedCombat);
				return;
			}
			string text2 = this._app.UI.Path(new string[]
			{
				base.ID,
				EncounterDialog.UIPostCombatSummaryPanel
			});
			this._app.UI.SetVisible(text2, true);
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				base.ID,
				EncounterDialog.UIEncounterSummaryPanel
			}), false);
			this.SyncPostCombatPanel(this._app, text2, this._selectedCombat, playSpeech, refreshList);
		}
		public void SyncWeaponDamageList(App game, string panelName, Dictionary<int, float> damageTable)
		{
			game.UI.ClearItems(panelName);
			int num = 0;
			foreach (KeyValuePair<int, float> kvp in damageTable)
			{
				LogicalWeapon logicalWeapon = game.AssetDatabase.Weapons.First(delegate(LogicalWeapon x)
				{
					int arg_14_0 = x.UniqueWeaponID;
					KeyValuePair<int, float> kvp2 = kvp;
					return arg_14_0 == kvp2.Key;
				});
				string iconSpriteName = logicalWeapon.IconSpriteName;
				game.UI.AddItem(panelName, string.Empty, num, string.Empty);
				string itemGlobalID = game.UI.GetItemGlobalID(panelName, string.Empty, num, string.Empty);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID,
					EncounterDialog.UIWeaponIcon
				}), "sprite", iconSpriteName);
				UICommChannel arg_115_0 = game.UI;
				string arg_115_1 = game.UI.Path(new string[]
				{
					itemGlobalID,
					EncounterDialog.UIWeaponDamage
				});
				string arg_115_2 = "text";
				KeyValuePair<int, float> kvp3 = kvp;
				arg_115_0.SetPropertyString(arg_115_1, arg_115_2, Math.Floor((double)kvp3.Value).ToString("N0"));
				num++;
			}
		}
		public void SyncPostCombatPanel(App game, string panelName, PendingCombat pendingCombat, bool playSpeech, bool postGameEvents = false)
		{
			List<int> AlliedPlayers = (
				from x in pendingCombat.PlayersInCombat
				where x == game.Game.LocalPlayer.ID || game.GameDatabase.GetDiplomacyStateBetweenPlayers(game.Game.LocalPlayer.ID, x) == DiplomacyState.ALLIED
				select x).ToList<int>();
			List<int> EnemyPlayers = (
				from x in pendingCombat.PlayersInCombat
				where !AlliedPlayers.Contains(x)
				select x).ToList<int>();
			PostCombatData combatResults = pendingCombat.CombatResults;
			combatResults.SystemId = new int?(pendingCombat.SystemID);
			StarSystemMapUI.Sync(game, pendingCombat.SystemID, game.UI.Path(new string[]
			{
				panelName,
				"partMiniSystem"
			}), false);
			List<Player> source = GameSession.GetPlayersWithCombatAssets(game, pendingCombat.SystemID).ToList<Player>();
			bool flag = source.Any((Player x) => game.GameDatabase.GetDiplomacyStateBetweenPlayers(x.ID, game.LocalPlayer.ID) == DiplomacyState.WAR);
			bool flag2 = source.Any((Player x) => x.ID == game.LocalPlayer.ID || game.GameDatabase.GetDiplomacyStateBetweenPlayers(x.ID, game.LocalPlayer.ID) == DiplomacyState.ALLIED);
			string propertyValue = "ui\\events\\event_combat_draw.tga";
			string arg = App.Localize("@UI_POST_COMBAT_DRAW");
			if (flag && flag2)
			{
				if (playSpeech)
				{
					string cueName = string.Format("COMBAT_045-01_{0}_BattleIsADraw", this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)));
					this._app.PostRequestSpeech(cueName, 50, 120, 0f);
				}
				if (postGameEvents)
				{
					game.GameDatabase.InsertTurnEvent(new TurnEvent
					{
						EventType = TurnEventType.EV_COMBAT_DRAW,
						EventMessage = TurnEventMessage.EM_COMBAT_DRAW,
						FleetID = pendingCombat.SelectedPlayerFleets[game.LocalPlayer.ID],
						PlayerID = game.LocalPlayer.ID,
						SystemID = pendingCombat.SystemID,
						CombatID = pendingCombat.ConflictID,
						TurnNumber = game.GameDatabase.GetTurnCount(),
						ShowsDialog = false
					});
				}
			}
			else
			{
				if (flag2)
				{
					if (playSpeech)
					{
						FleetInfo fleetInfo = this._selectedCombat.CombatResults.FleetsInCombat.FirstOrDefault((FleetInfo x) => x.PlayerID == game.LocalPlayer.ID);
						if (fleetInfo != null)
						{
							this._app.GameDatabase.GetAdmiralInfo(fleetInfo.AdmiralID);
							string cueName2 = string.Format("COMBAT_043-01_{0}_WinningABattle", this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)));
							this._app.PostRequestSpeech(cueName2, 50, 120, 0f);
						}
					}
					propertyValue = "ui\\events\\event_combat_win.tga";
					arg = App.Localize("@UI_POST_COMBAT_WIN");
					if (postGameEvents)
					{
						game.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_COMBAT_WIN,
							EventMessage = TurnEventMessage.EM_COMBAT_WIN,
							FleetID = pendingCombat.SelectedPlayerFleets[game.LocalPlayer.ID],
							PlayerID = game.LocalPlayer.ID,
							SystemID = pendingCombat.SystemID,
							CombatID = pendingCombat.ConflictID,
							TurnNumber = game.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
				else
				{
					propertyValue = "ui\\events\\event_combat_lose.tga";
					arg = App.Localize("@UI_POST_COMBAT_LOSS");
					if (postGameEvents)
					{
						game.GameDatabase.InsertTurnEvent(new TurnEvent
						{
							EventType = TurnEventType.EV_COMBAT_LOSS,
							EventMessage = TurnEventMessage.EM_COMBAT_LOSS,
							FleetID = pendingCombat.SelectedPlayerFleets[game.LocalPlayer.ID],
							PlayerID = game.LocalPlayer.ID,
							SystemID = pendingCombat.SystemID,
							CombatID = pendingCombat.ConflictID,
							TurnNumber = game.GameDatabase.GetTurnCount(),
							ShowsDialog = false
						});
					}
				}
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"imgPostCombatEvent"
			}), "sprite", propertyValue);
			string arg2 = combatResults.SystemId.HasValue ? game.GameDatabase.GetStarSystemInfo(combatResults.SystemId.Value).Name : App.Localize("@UI_POST_COMBAT_DEEP_SPACE");
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				EncounterDialog.UICombatHeaderLabel
			}), "text", string.Format("{0} {1} - {2}", App.Localize("@UI_POST_COMBAT_COMBAT_AT"), arg2, arg));
			game.UI.ClearItems(game.UI.Path(new string[]
			{
				panelName,
				EncounterDialog.UIInfoPanel,
				EncounterDialog.UIInfoList
			}));
			for (int i = 0; i < combatResults.AdditionalInfo.Count; i++)
			{
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIInfoPanel,
					EncounterDialog.UIInfoList
				}), string.Empty, i, string.Empty);
				string itemGlobalID = game.UI.GetItemGlobalID(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIInfoPanel,
					EncounterDialog.UIInfoList
				}), string.Empty, i, string.Empty);
				game.UI.SetPropertyString(itemGlobalID, "text", combatResults.AdditionalInfo[i]);
			}
			int? selectedAlly;
			if (AlliedPlayers.Count > 1)
			{
				this._allyIndex = Math.Min(Math.Max(this._allyIndex, 0), AlliedPlayers.Count);
				selectedAlly = ((this._allyIndex == 0) ? null : new int?(AlliedPlayers[this._allyIndex - 1]));
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UINextAlly
				}), this._allyIndex < AlliedPlayers.Count);
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIPreviousAlly
				}), this._allyIndex >= 1);
			}
			else
			{
				selectedAlly = new int?(AlliedPlayers.First<int>());
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UINextAlly
				}), false);
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIPreviousAlly
				}), false);
			}
			int? selectedEnemy;
			if (EnemyPlayers.Count > 1)
			{
				this._enemyIndex = Math.Min(Math.Max(this._enemyIndex, 0), EnemyPlayers.Count);
				selectedEnemy = ((this._enemyIndex == 0) ? null : new int?(EnemyPlayers[this._enemyIndex - 1]));
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UINextEnemy
				}), this._enemyIndex < EnemyPlayers.Count);
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIPreviousEnemy
				}), this._enemyIndex >= 1);
			}
			else
			{
				selectedEnemy = new int?(EnemyPlayers.First<int>());
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UINextEnemy
				}), false);
				this._app.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIPreviousEnemy
				}), false);
			}
			if (AlliedPlayers.Count > 1 && !selectedAlly.HasValue)
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIAlliedPlayerCard
				}), App.Localize("@UI_POST_COMBAT_ALLIANCE"), App.Localize("@DIPLO_RELATION_LOVE"), "", "");
				this._postCombatLocalFleetWidget.SetSyncedFleets((
					from x in combatResults.FleetsInCombat
					where AlliedPlayers.Contains(x.PlayerID)
					select x).ToList<FleetInfo>());
				Dictionary<int, float> dictionary = new Dictionary<int, float>();
				foreach (KeyValuePair<int, Dictionary<int, float>> current in combatResults.WeaponDamageTable)
				{
					if (AlliedPlayers.Contains(current.Key))
					{
						foreach (KeyValuePair<int, float> current2 in current.Value)
						{
							if (!dictionary.ContainsKey(current2.Key))
							{
								dictionary.Add(current2.Key, 0f);
							}
							Dictionary<int, float> dictionary2;
							int key;
							(dictionary2 = dictionary)[key = current2.Key] = dictionary2[key] + current2.Value;
						}
					}
				}
				this.SyncWeaponDamageList(game, game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIAlliedDamagePanel,
					EncounterDialog.UIAlliedDamageList
				}), dictionary);
			}
			else
			{
				if (selectedAlly != 0)
				{
					StarmapUI.SyncPlayerCard(game, game.UI.Path(new string[]
					{
						panelName,
						EncounterDialog.UIAlliedPlayerCard
					}), selectedAlly.Value);
					this._postCombatLocalFleetWidget.SetSyncedFleets((
						from x in combatResults.FleetsInCombat
						where x.PlayerID == selectedAlly
						select x).ToList<FleetInfo>());
					this.SyncWeaponDamageList(game, game.UI.Path(new string[]
					{
						panelName,
						EncounterDialog.UIAlliedDamagePanel,
						EncounterDialog.UIAlliedDamageList
					}), combatResults.WeaponDamageTable[selectedAlly.Value]);
				}
			}
			if (EnemyPlayers.Count > 1 && !selectedEnemy.HasValue)
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIEnemyPlayerCard
				}), App.Localize("@UI_POST_COMBAT_ENEMIES"), App.Localize("@DIPLO_RELATION_HATE"), "", "");
				this._postCombatEncounterFleetWidget.SetSyncedFleets((
					from x in combatResults.FleetsInCombat
					where EnemyPlayers.Contains(x.PlayerID)
					select x).ToList<FleetInfo>());
				Dictionary<int, float> dictionary3 = new Dictionary<int, float>();
				foreach (KeyValuePair<int, Dictionary<int, float>> current3 in combatResults.WeaponDamageTable)
				{
					if (EnemyPlayers.Contains(current3.Key))
					{
						foreach (KeyValuePair<int, float> current4 in current3.Value)
						{
							if (!dictionary3.ContainsKey(current4.Key))
							{
								dictionary3.Add(current4.Key, 0f);
							}
							Dictionary<int, float> dictionary4;
							int key2;
							(dictionary4 = dictionary3)[key2 = current4.Key] = dictionary4[key2] + current4.Value;
						}
					}
				}
				this.SyncWeaponDamageList(game, game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIEnemyDamagePanel,
					EncounterDialog.UIEnemyDamageList
				}), dictionary3);
				return;
			}
			if (selectedEnemy != 0)
			{
				StarmapUI.SyncPlayerCard(game, game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIEnemyPlayerCard
				}), selectedEnemy.Value);
				this._postCombatEncounterFleetWidget.SetSyncedFleets((
					from x in combatResults.FleetsInCombat
					where x.PlayerID == selectedEnemy
					select x).ToList<FleetInfo>());
				this.SyncWeaponDamageList(game, game.UI.Path(new string[]
				{
					panelName,
					EncounterDialog.UIEnemyDamagePanel,
					EncounterDialog.UIEnemyDamageList
				}), combatResults.WeaponDamageTable[selectedEnemy.Value]);
			}
		}
	}
}
