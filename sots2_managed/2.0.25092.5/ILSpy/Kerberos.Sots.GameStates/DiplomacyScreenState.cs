using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using Kerberos.Sots.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class DiplomacyScreenState : GameState, IKeyBindListener
	{
		private enum DiplomacyMode
		{
			Standard,
			Independent
		}
		private const string UIBackButton = "btnBackButton";
		private const string UIEmpiresButton = "btnEmpiresButton";
		private const string UIIndependentsButton = "btnIndependentsButton";
		private Dictionary<int, DiplomacyCardState> LastDiplomacyCardState = new Dictionary<int, DiplomacyCardState>();
		private List<int> PlayerSlots = new List<int>();
		private int _playerId;
		private int _selectedIndy;
		private int _selectedLobbyPlayer;
		private bool _lobbyimprove = true;
		private DiplomacyScreenState.DiplomacyMode _mode;
		public DiplomacyScreenState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame();
			}
			base.App.UI.LoadScreen("Diplomacy");
		}
		public void SyncPlayerDiplomacyCard(int playerId, bool updateButtonIds)
		{
			DiplomacyUI.SyncPlayerDiplomacyCard(base.App, "Player" + this.PlayerSlots.IndexOf(playerId), playerId, this.LastDiplomacyCardState[playerId], updateButtonIds);
		}
		protected override void OnEnter()
		{
			if (base.App.LocalPlayer == null)
			{
				base.App.NewGame();
			}
			base.App.UI.SetScreen("Diplomacy");
			base.App.UI.SetVisible("noDiploText", false);
			this.PlayerSlots.Clear();
			List<PlayerInfo> list = base.App.GameDatabase.GetPlayerInfos().ToList<PlayerInfo>();
			list.RemoveAll((PlayerInfo x) => x.ID == base.App.Game.LocalPlayer.ID);
			int num = 0;
			DiplomacyUI.HideAllPlayerDiplomacyCards(base.App);
			base.App.UI.ClearItems("pnlIndyDiplomacy.pnlFactionsList.factionList");
			foreach (PlayerInfo current in list)
			{
				if (current.isStandardPlayer)
				{
					if (!this.LastDiplomacyCardState.ContainsKey(current.ID))
					{
						this.LastDiplomacyCardState.Add(current.ID, DiplomacyCardState.PlayerStats);
					}
					this.PlayerSlots.Add(current.ID);
					this.SyncPlayerDiplomacyCard(current.ID, true);
					bool isEncountered = base.App.GameDatabase.GetDiplomacyInfo(base.App.LocalPlayer.ID, current.ID).isEncountered;
					base.App.UI.SetVisible("Player" + this.PlayerSlots.IndexOf(current.ID), isEncountered);
					if (isEncountered)
					{
						num++;
					}
				}
				else
				{
					if (!current.isDefeated && current.includeInDiplomacy && (!base.App.AssetDatabase.GetFaction(current.FactionID).IsIndependent() || base.App.GameDatabase.GetHasPlayerStudiedIndependentRace(base.App.LocalPlayer.ID, current.ID)))
					{
						bool isEncountered2 = base.App.GameDatabase.GetDiplomacyInfo(base.App.LocalPlayer.ID, current.ID).isEncountered;
						if (isEncountered2)
						{
							base.App.UI.AddItem("pnlIndyDiplomacy.pnlFactionsList.factionList", string.Empty, current.ID, string.Empty);
							string itemGlobalID = base.App.UI.GetItemGlobalID("pnlIndyDiplomacy.pnlFactionsList.factionList", string.Empty, current.ID, string.Empty);
							base.App.UI.SetEnabled(itemGlobalID, false);
							base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
							{
								itemGlobalID,
								"txtInteraction"
							}), "text", current.Name);
						}
					}
				}
			}
			if (num == 0)
			{
				base.App.UI.SetVisible("noDiploText", true);
			}
			this._selectedIndy = 0;
			base.App.UI.SetVisible("pnlIndyPlayerSummary", false);
			base.App.UI.ClearSelection("factionList");
			PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(base.App.Game.LocalPlayer.ID);
			base.App.UI.SetPropertyString("Screen_Title", "text", string.Format(App.Localize("@UI_DIPLOMACY_DIPLOMACY"), playerInfo.GenericDiplomacyPoints));
			base.App.HotKeyManager.AddListener(this);
		}
		private int GetPlayerId(string panelName)
		{
			if (this._mode == DiplomacyScreenState.DiplomacyMode.Standard)
			{
				string text = panelName.Split(new char[]
				{
					'|'
				})[0];
				return this.PlayerSlots[int.Parse(text.Replace("Player", ""))];
			}
			return this._selectedIndy;
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.App.HotKeyManager.RemoveListener(this);
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_closed")
			{
				if (this._mode == DiplomacyScreenState.DiplomacyMode.Independent)
				{
					DiplomacyUI.SyncIndyDiplomacyCard(base.App, "pnlIndyPlayerSummary", this._selectedIndy);
				}
				else
				{
					this.SyncPlayerDiplomacyCard(this._playerId, false);
				}
			}
			else
			{
				if (msgType == "list_sel_changed")
				{
					if (panelName == "factionList" && !string.IsNullOrEmpty(msgParams[0]))
					{
						this._selectedIndy = int.Parse(msgParams[0]);
						base.App.UI.SetVisible("pnlIndyPlayerSummary", true);
						DiplomacyUI.SyncIndyDiplomacyCard(base.App, "pnlIndyPlayerSummary", this._selectedIndy);
					}
					else
					{
						if (panelName == DiplomacyUI.UILobbyPlayerList)
						{
							int num = int.Parse(msgParams[0]);
							this._selectedLobbyPlayer = num;
							PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(num);
							DiplomacyInfo diplomacyInfo = base.App.GameDatabase.GetDiplomacyInfo(this._playerId, playerInfo.ID);
							DiplomaticMood diplomaticMood = diplomacyInfo.GetDiplomaticMood();
							base.App.UI.SetVisible(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								"imgOtherRelation"
							}), true);
							if (diplomaticMood == DiplomaticMood.Love)
							{
								base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
								{
									DiplomacyUI.UILobbyPanel,
									"imgOtherRelation"
								}), "sprite", "Love");
							}
							else
							{
								if (diplomaticMood == DiplomaticMood.Hatred)
								{
									base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
									{
										DiplomacyUI.UILobbyPanel,
										"imgOtherRelation"
									}), "sprite", "Hate");
								}
								else
								{
									base.App.UI.SetVisible(base.App.UI.Path(new string[]
									{
										DiplomacyUI.UILobbyPanel,
										"imgOtherRelation"
									}), false);
								}
							}
							base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								"imgOtherAvatar"
							}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
							base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								"imgOtherBadge"
							}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
						}
					}
				}
			}
			if (msgType == "button_clicked")
			{
				if (panelName == "gameTutorialButton")
				{
					base.App.UI.SetVisible("DiplomacyScreenTutorial", true);
					return;
				}
				if (panelName == "diplomacyScreenTutImage")
				{
					base.App.UI.SetVisible("DiplomacyScreenTutorial", false);
					return;
				}
				if (panelName == "btnEmpiresButton")
				{
					this._mode = DiplomacyScreenState.DiplomacyMode.Standard;
					base.App.UI.SetVisible("pnlStandardDiplomacy", true);
					base.App.UI.SetVisible("pnlIndyDiplomacy", false);
					return;
				}
				if (panelName == "btnIndependentsButton")
				{
					this._mode = DiplomacyScreenState.DiplomacyMode.Independent;
					base.App.UI.SetVisible("pnlIndyDiplomacy", true);
					base.App.UI.SetVisible("pnlStandardDiplomacy", false);
					return;
				}
				if (panelName == "btnBackButton")
				{
					base.App.SwitchGameState<StarMapState>(new object[0]);
					return;
				}
				if (panelName == DiplomacyUI.UISurpriseAttackOk)
				{
					base.App.Game.DeclareWarInformally(base.App.Game.LocalPlayer.ID, this._playerId);
					base.App.UI.SetVisible(DiplomacyUI.UISurpriseAttackPanel, false);
					if (this._mode == DiplomacyScreenState.DiplomacyMode.Independent)
					{
						DiplomacyUI.SyncIndyDiplomacyCard(base.App, "pnlIndyPlayerSummary", this._selectedIndy);
						return;
					}
					this.SyncPlayerDiplomacyCard(this._playerId, true);
					return;
				}
				else
				{
					if (panelName == DiplomacyUI.UISurpriseAttackCancel)
					{
						base.App.UI.SetVisible(DiplomacyUI.UISurpriseAttackPanel, false);
						return;
					}
					if (panelName == DiplomacyUI.UIDeclareWarOk)
					{
						base.App.Game.DeclareWarFormally(base.App.Game.LocalPlayer.ID, this._playerId);
						base.App.UI.SetVisible(DiplomacyUI.UIDeclareWarPanel, false);
						base.App.GameDatabase.SpendDiplomacyPoints(base.App.GameDatabase.GetPlayerInfo(base.App.Game.LocalPlayer.ID), base.App.GameDatabase.GetPlayerFactionID(this._playerId), base.App.Game.GetDiplomacyActionCost(DiplomacyAction.DECLARATION, null, null).Value);
						if (this._mode == DiplomacyScreenState.DiplomacyMode.Independent)
						{
							DiplomacyUI.SyncIndyDiplomacyCard(base.App, "pnlIndyPlayerSummary", this._selectedIndy);
							return;
						}
						this.SyncPlayerDiplomacyCard(this._playerId, true);
						return;
					}
					else
					{
						if (panelName == DiplomacyUI.UIDeclareWarCancel)
						{
							base.App.UI.SetVisible(DiplomacyUI.UIDeclareWarPanel, false);
							return;
						}
						if (panelName == DiplomacyUI.UIDemandOk)
						{
							base.App.UI.SetVisible(DiplomacyUI.UIDemandPanel, false);
							return;
						}
						if (panelName == DiplomacyUI.UIDemandCancel)
						{
							base.App.UI.SetVisible(DiplomacyUI.UIDemandPanel, false);
							return;
						}
						if (panelName == DiplomacyUI.UIRequestOk)
						{
							base.App.UI.SetVisible(DiplomacyUI.UIRequestPanel, false);
							return;
						}
						if (panelName == DiplomacyUI.UIRequestCancel)
						{
							base.App.UI.SetVisible(DiplomacyUI.UIRequestPanel, false);
							return;
						}
						if (panelName == DiplomacyUI.UITreatyOk)
						{
							base.App.UI.SetVisible(DiplomacyUI.UITreatyPanel, false);
							return;
						}
						if (panelName == DiplomacyUI.UITreatyCancel)
						{
							base.App.UI.SetVisible(DiplomacyUI.UITreatyPanel, false);
							return;
						}
						if (panelName == DiplomacyUI.UILobbyRelationImprovebtn)
						{
							this._lobbyimprove = true;
							base.App.UI.SetChecked(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								DiplomacyUI.UILobbyRelationDegradebtn
							}), false);
							base.App.UI.SetChecked(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								DiplomacyUI.UILobbyRelationImprovebtn
							}), true);
							return;
						}
						if (panelName == DiplomacyUI.UILobbyRelationDegradebtn)
						{
							this._lobbyimprove = false;
							base.App.UI.SetChecked(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								DiplomacyUI.UILobbyRelationDegradebtn
							}), true);
							base.App.UI.SetChecked(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								DiplomacyUI.UILobbyRelationImprovebtn
							}), false);
							return;
						}
						if (panelName == DiplomacyUI.UILobbyOk)
						{
							base.App.Game.DoLobbyAction(base.App.LocalPlayer.ID, this._playerId, this._selectedLobbyPlayer, this._lobbyimprove);
							base.App.UI.SetVisible(DiplomacyUI.UILobbyPanel, false);
							return;
						}
						if (panelName == DiplomacyUI.UILobbyCancel)
						{
							base.App.UI.SetVisible(DiplomacyUI.UILobbyPanel, false);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UIIntelButton))
						{
							string text = panelName.Split(new char[]
							{
								'|'
							})[0];
							int targetPlayer = this.PlayerSlots[int.Parse(text.Replace("Player", ""))];
							IntelMissionDialog dialog = new IntelMissionDialog(base.App.Game, targetPlayer);
							this._playerId = this.GetPlayerId(panelName);
							base.App.UI.CreateDialog(dialog, null);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UICounterIntelButton))
						{
							string text2 = panelName.Split(new char[]
							{
								'|'
							})[0];
							int targetPlayer2 = this.PlayerSlots[int.Parse(text2.Replace("Player", ""))];
							this._playerId = this.GetPlayerId(panelName);
							base.App.UI.CreateDialog(new CounterIntelMissionDialog(base.App.Game, targetPlayer2), null);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UIOperationsButton))
						{
							string text3 = panelName.Split(new char[]
							{
								'|'
							})[0];
							int arg_91B_0 = this.PlayerSlots[int.Parse(text3.Replace("Player", ""))];
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UICardPreviousState))
						{
							string text4 = panelName.Split(new char[]
							{
								'|'
							})[0];
							this._playerId = this.PlayerSlots[int.Parse(text4.Replace("Player", ""))];
							this.LastDiplomacyCardState[this._playerId] = DiplomacyUI.GetPreviousDiplomacyCardState(this.LastDiplomacyCardState[this._playerId]);
							DiplomacyUI.SyncPlayerDiplomacyCard(base.App, text4, this._playerId, this.LastDiplomacyCardState[this._playerId], false);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UICardNextState))
						{
							string text5 = panelName.Split(new char[]
							{
								'|'
							})[0];
							this._playerId = this.PlayerSlots[int.Parse(text5.Replace("Player", ""))];
							this.LastDiplomacyCardState[this._playerId] = DiplomacyUI.GetNextDiplomacyCardState(this.LastDiplomacyCardState[this._playerId]);
							DiplomacyUI.SyncPlayerDiplomacyCard(base.App, text5, this._playerId, this.LastDiplomacyCardState[this._playerId], false);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UISurpriseAttackButton))
						{
							this._playerId = this.GetPlayerId(panelName);
							DiplomacyUI.SyncDiplomacyPopup(base.App, DiplomacyUI.UISurpriseAttackPanel, this._playerId);
							base.App.UI.SetVisible(DiplomacyUI.UISurpriseAttackPanel, true);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UIDeclareButton))
						{
							this._playerId = this.GetPlayerId(panelName);
							DiplomacyUI.SyncDiplomacyPopup(base.App, DiplomacyUI.UIDeclareWarPanel, this._playerId);
							base.App.UI.SetVisible(DiplomacyUI.UIDeclareWarPanel, true);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UIDemandButton))
						{
							this._playerId = this.GetPlayerId(panelName);
							base.App.UI.CreateDialog(new DemandTypeDialog(base.App, this._playerId, "dialogDemandType"), null);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UIRequestButton))
						{
							this._playerId = this.GetPlayerId(panelName);
							base.App.UI.CreateDialog(new RequestTypeDialog(base.App, this._playerId, "dialogRequestType"), null);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UITreatyButton))
						{
							this._playerId = this.GetPlayerId(panelName);
							base.App.UI.CreateDialog(new TreatiesPopup(base.App, this._playerId, "TreatiesPopup"), null);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UILobbyButton))
						{
							this._playerId = this.GetPlayerId(panelName);
							DiplomacyUI.SyncDiplomacyPopup(base.App, DiplomacyUI.UILobbyPanel, this._playerId);
							this._lobbyimprove = true;
							base.App.UI.SetChecked(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								DiplomacyUI.UILobbyRelationDegradebtn
							}), false);
							base.App.UI.SetChecked(base.App.UI.Path(new string[]
							{
								DiplomacyUI.UILobbyPanel,
								DiplomacyUI.UILobbyRelationImprovebtn
							}), true);
							base.App.UI.SetVisible(DiplomacyUI.UILobbyPanel, true);
							return;
						}
						if (panelName.EndsWith(DiplomacyUI.UIGiveButton))
						{
							this._playerId = this.GetPlayerId(panelName);
							base.App.UI.CreateDialog(new GiveTypeDialog(base.App, this._playerId, "dialogGiveType"), null);
						}
					}
				}
			}
		}
		protected override void OnUpdate()
		{
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
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
					base.App.UI.LockUI();
					base.App.SwitchGameState<EmpireSummaryState>(new object[0]);
					return true;
				case HotKeyManager.HotKeyActions.State_SotspediaScreen:
					base.App.UI.LockUI();
					base.App.SwitchGameState<SotspediaState>(new object[0]);
					return true;
				}
			}
			return false;
		}
	}
}
