using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DiplomacyUI
	{
		internal delegate void SyncCardStateDelegate(App game, string panelName, PlayerInfo playerId, bool updateButtonIds);
		public const int MaxPlayerCards = 7;
		public static readonly string UIPanelBackground = "pnlBackground";
		public static readonly string UISurpriseAttackPanel = "pnlSurpriseAttack";
		public static readonly string UISurpriseAttackOk = "btnSurpriseAttackOk";
		public static readonly string UISurpriseAttackCancel = "btnSurpriseAttackCancel";
		public static readonly string UIDeclareWarPanel = "pnlDeclareWar";
		public static readonly string UIDeclareWarOk = "btnDeclareWarOk";
		public static readonly string UIDeclareWarCancel = "btnDeclareWarCancel";
		public static readonly string UIRequestPanel = "pnlRequest";
		public static readonly string UIRequestOk = "btnRequestOk";
		public static readonly string UIRequestCancel = "btnRequestCancel";
		public static readonly string UIDemandPanel = "pnlDemand";
		public static readonly string UIDemandOk = "btnDemandOk";
		public static readonly string UIDemandCancel = "btnDemandCancel";
		public static readonly string UITreatyPanel = "pnlTreaty";
		public static readonly string UITreatyOk = "btnTreatyOk";
		public static readonly string UITreatyCancel = "btnTreatyCancel";
		public static readonly string UILobbyPanel = "pnlLobby";
		public static readonly string UILobbyOk = "btnLobbyOk";
		public static readonly string UILobbyCancel = "btnLobbyCancel";
		public static readonly string UILobbyPlayerList = "LobbyselectEmpire";
		public static readonly string UILobbyRelationImprovebtn = "LobbyrelationsImprove";
		public static readonly string UILobbyRelationDegradebtn = "LobbyrelationsDegrade";
		public static readonly string UICardPreviousState = "btnPreviousState";
		public static readonly string UICardNextState = "btnNextState";
		public static readonly string UICardPlayerName = "lblPlayerName";
		public static readonly string UIAvatar = "imgAvatar";
		public static readonly string UIBadge = "imgBadge";
		public static readonly string UIRelation = "imgRelation";
		public static readonly string UIMood = "imgMood";
		public static readonly string UIRelationText = "lblPlayerRelation";
		public static readonly string UIRelationsGraph = "grphRelation";
		public static readonly string UIHazardRating = "lblHazardValue";
		public static readonly string UIDriveTech = "lblDriveTechValue";
		public static readonly string UIDriveSpecial = "lblDriveSpecialValue";
		public static readonly string UIStatRpdValue = "lblStatRpdValue";
		public static readonly string UIGovernmentType = "governmentType";
		public static readonly string UIActionRdpValue = "lblActionRpdValue";
		public static readonly string UIPendingActions = "lstPendingActions";
		public static readonly string UISurpriseAttackButton = "btnSurpriseAttack";
		public static readonly string UIDeclareButton = "btnDeclare";
		public static readonly string UIRequestButton = "btnRequest";
		public static readonly string UIDemandButton = "btnDemand";
		public static readonly string UITreatyButton = "btnTreaty";
		public static readonly string UILobbyButton = "btnLobby";
		public static readonly string UIGiveButton = "btnGive";
		public static readonly string UINewsList = "lstNews";
		public static readonly string UIInteractionsList = "lstInteractions";
		public static readonly string UIIntelButton = "btnIntel";
		public static readonly string UICounterIntelButton = "btnCounterIntel";
		public static readonly string UIOperationsButton = "btnOperations";
		public static readonly string UIIntelList = "listIntel";
		public static readonly string UICounterIntelList = "listCounterIntel";
		public static readonly string UIOperationsList = "listOperations";
		internal static Dictionary<DiplomacyCardState, DiplomacyUI.SyncCardStateDelegate> CardStateFunctionMap = new Dictionary<DiplomacyCardState, DiplomacyUI.SyncCardStateDelegate>
		{

			{
				DiplomacyCardState.PlayerStats,
				new DiplomacyUI.SyncCardStateDelegate(DiplomacyUI.SyncPlayerStatsState)
			},

			{
				DiplomacyCardState.DiplomacyActions,
				new DiplomacyUI.SyncCardStateDelegate(DiplomacyUI.SyncDiplomacyActionsState)
			},

			{
				DiplomacyCardState.PlayerHistory,
				new DiplomacyUI.SyncCardStateDelegate(DiplomacyUI.SyncPlayerHistoryState)
			},

			{
				DiplomacyCardState.Espionage,
				new DiplomacyUI.SyncCardStateDelegate(DiplomacyUI.SyncEspionageState)
			}
		};
		private static bool EnableSupriseButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.SURPRISEATTACK, null, null);
		}
		private static bool EnableDeclareButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.DECLARATION, null, null);
		}
		private static bool EnableRequestButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.REQUEST, null, null);
		}
		private static bool EnableDemandButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.DEMAND, null, null);
		}
		private static bool EnableTreatyButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.TREATY, null, null);
		}
		private static bool EnableLobbyButton(GameSession game, PlayerInfo target)
		{
			return game.CanPerformLocalDiplomacyAction(target, DiplomacyAction.LOBBY, null, null);
		}
		private static string GetDiplomacyActionString(App game, DiplomacyActionHistoryEntryInfo actionInfo)
		{
			string text = "";
			switch (actionInfo.Action)
			{
			case DiplomacyAction.DECLARATION:
				return App.Localize("@DIPLOMACY_DECLARE_WAR");
			case DiplomacyAction.REQUEST:
			{
				text = App.Localize("@UI_DIPLOMACY_REQUEST") + " ";
				int? actionSubType = actionInfo.ActionSubType;
				int valueOrDefault = actionSubType.GetValueOrDefault();
				if (actionSubType.HasValue)
				{
					switch (valueOrDefault)
					{
					case 0:
						text += App.Localize("@DIPLOMACY_SUBTYPE_MONEY");
						break;
					case 1:
						text += App.Localize("@DIPLOMACY_SUBTYPE_SYSTEM_INFORMATION");
						break;
					case 2:
						text += App.Localize("@DIPLOMACY_SUBTYPE_RESEARCH_POINTS");
						break;
					case 3:
						text += App.Localize("@DIPLOMACY_SUBTYPE_MILITARY_ASSISTANCE");
						break;
					case 4:
						text += App.Localize("@DIPLOMACY_SUBTYPE_BUILD_GATE");
						break;
					case 5:
						text += App.Localize("@DIPLOMACY_SUBTYPE_BUILD_WORLD");
						break;
					case 6:
						text += App.Localize("@DIPLOMACY_SUBTYPE_BUILD_ENCLAVE");
						break;
					}
				}
				break;
			}
			case DiplomacyAction.DEMAND:
			{
				text = App.Localize("@DIPLOMACY_DEMAND") + " ";
				int? actionSubType2 = actionInfo.ActionSubType;
				int valueOrDefault2 = actionSubType2.GetValueOrDefault();
				if (actionSubType2.HasValue)
				{
					switch (valueOrDefault2)
					{
					case 0:
						text += App.Localize("@DIPLOMACY_SUBTYPE_MONEY");
						break;
					case 1:
						text += App.Localize("@DIPLOMACY_SUBTYPE_SYSTEM_INFORMATION");
						break;
					case 2:
						text += App.Localize("@DIPLOMACY_SUBTYPE_RESEARCH_POINTS");
						break;
					case 3:
						text += App.Localize("@DIPLOMACY_SUBTYPE_SLAVES");
						break;
					case 4:
						text += App.Localize("@DIPLOMACY_SUBTYPE_SYSTEM");
						break;
					case 5:
						text += App.Localize("@DIPLOMACY_SUBTYPE_SURRENDER");
						break;
					}
				}
				break;
			}
			case DiplomacyAction.TREATY:
			{
				text = App.Localize("@UI_DIPLOMACY_TREATY") + " ";
				int? actionSubType3 = actionInfo.ActionSubType;
				int valueOrDefault3 = actionSubType3.GetValueOrDefault();
				if (actionSubType3.HasValue)
				{
					switch (valueOrDefault3)
					{
					case 0:
						text += App.Localize("@UI_TREATY_ARMISTICE");
						break;
					case 1:
						text += App.Localize("@UI_TREATY_TRADE");
						break;
					case 2:
						text += App.Localize("@UI_TREATY_LIMITATION");
						break;
					case 3:
						text += App.Localize("@UI_TREATY_PROTECTORATE");
						break;
					case 4:
						text += App.Localize("@UI_TREATY_INCORPORATE");
						break;
					}
				}
				break;
			}
			case DiplomacyAction.LOBBY:
				return App.Localize("@DIPLOMACY_LOBBY");
			case DiplomacyAction.SPIN:
				return App.Localize("@DIPLOMACY_SPIN");
			case DiplomacyAction.SURPRISEATTACK:
				return App.Localize("@DIPLOMACY_SURPRISE_ATTACK");
			case DiplomacyAction.GIVE:
			{
				int? actionSubType4 = actionInfo.ActionSubType;
				int valueOrDefault4 = actionSubType4.GetValueOrDefault();
				if (actionSubType4.HasValue)
				{
					switch (valueOrDefault4)
					{
					case 0:
						text = App.Localize("@UI_DIPLOMACY_GIVE_SAVINGS");
						break;
					case 1:
						text = App.Localize("@UI_DIPLOMACY_GIVE_RESEARCH_MONEY");
						break;
					}
				}
				break;
			}
			}
			return text;
		}
		public static void SyncPanelColor(App game, string panelName, Vector3 color)
		{
			List<string> list = new List<string>
			{
				"TLC",
				"TRC",
				"BLC",
				"BRC",
				"TC",
				"BC",
				"FILL"
			};
			foreach (string current in list)
			{
				game.UI.SetPropertyColorNormalized(game.UI.Path(new string[]
				{
					panelName,
					current
				}), "color", color);
			}
		}
		private static string GetRelationText(DiplomacyState state)
		{
			switch (state)
			{
			case DiplomacyState.CEASE_FIRE:
				return "C/F";
			case DiplomacyState.UNKNOWN:
				return string.Empty;
			case DiplomacyState.NON_AGGRESSION:
				return "NAP";
			case DiplomacyState.WAR:
				return "War";
			case DiplomacyState.ALLIED:
				return "Ally";
			case DiplomacyState.NEUTRAL:
				return "Neutral";
			case DiplomacyState.PEACE:
				return "Peace";
			default:
				return string.Empty;
			}
		}
        private static void SyncPlayerStatsState(App game, string panelName, PlayerInfo playerInfo, bool updateButtonIds)
        {
            FactionInfo factionInfo = game.GameDatabase.GetFactionInfo(playerInfo.FactionID);
            float num2 = Math.Abs((float)(game.GameDatabase.GetFactionInfo(game.GameDatabase.GetPlayerFactionID(game.Game.LocalPlayer.ID)).IdealSuitability - factionInfo.IdealSuitability));
            string propertyValue = string.Format("{0:000}", num2);
            string bestEnginePlantTechString = GameSession.GetBestEnginePlantTechString(game, playerInfo.ID);
            string bestEngineTechString = GameSession.GetBestEngineTechString(game, playerInfo.ID);
            string str4 = game.GameDatabase.GetPlayerInfo(game.Game.LocalPlayer.ID).FactionDiplomacyPoints[factionInfo.ID].ToString();
            DiplomacyInfo diplomacyInfo = game.GameDatabase.GetDiplomacyInfo(game.Game.LocalPlayer.ID, playerInfo.ID);
            int numHistoryTurns = 10;
            int currentTurn = game.GameDatabase.GetTurnCount();
            List<DiplomacyReactionHistoryEntryInfo> source = game.GameDatabase.GetDiplomacyReactionHistory(game.Game.LocalPlayer.ID, playerInfo.ID, currentTurn, 10).ToList<DiplomacyReactionHistoryEntryInfo>();
            int[] numArray = new int[numHistoryTurns];
            numArray[numHistoryTurns - 1] = diplomacyInfo.Relations;
            string str5 = numArray[numHistoryTurns - 1].ToString();
            Func<DiplomacyReactionHistoryEntryInfo, bool> predicate = null;
            for (int i = numHistoryTurns - 2; i >= 0; i--)
            {
                if (predicate == null)
                {
                    predicate = delegate(DiplomacyReactionHistoryEntryInfo x)
                    {
                        int? turnCount = x.TurnCount;
                        int num = currentTurn - ((numHistoryTurns - 1) - i);
                        return (turnCount.GetValueOrDefault() == num) && turnCount.HasValue;
                    };
                }
                numArray[i] = numArray[i + 1] + source.Where<DiplomacyReactionHistoryEntryInfo>(predicate).Sum<DiplomacyReactionHistoryEntryInfo>(((Func<DiplomacyReactionHistoryEntryInfo, int>)(y => y.Difference)));
                str5 = str5 + "|" + numArray[i].ToString();
            }
            game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIRelationsGraph }), "data", str5);
            string diplomaticMoodSprite = diplomacyInfo.GetDiplomaticMoodSprite();
            if (!string.IsNullOrEmpty(diplomaticMoodSprite))
            {
                game.UI.SetVisible(UIRelation, true);
                game.UI.SetPropertyString(UIRelation, "sprite", diplomaticMoodSprite);
            }
            else
            {
                game.UI.SetVisible(UIRelation, false);
            }
            game.UI.SetText(game.UI.Path(new string[] { panelName, UIRelationText }), GetRelationText(diplomacyInfo.State));
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath);
            if (fileNameWithoutExtension == "")
            {
                fileNameWithoutExtension = game.AssetDatabase.GetFaction(playerInfo.FactionID).SplinterAvatarPath();
            }
            game.UI.SetVisible(game.UI.Path(new string[] { panelName, "eliminated" }), playerInfo.isDefeated);
            game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIAvatar }), "sprite", fileNameWithoutExtension);
            game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIBadge }), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
            game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIHazardRating }), "text", propertyValue);
            game.UI.SetVisible(game.UI.Path(new string[] { panelName, UIHazardRating }), factionInfo.Name != "loa");
            game.UI.SetVisible(game.UI.Path(new string[] { panelName, "hazardtitle" }), factionInfo.Name != "loa");
            if (playerInfo.isStandardPlayer)
            {
                game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIDriveTech }), "text", bestEnginePlantTechString);
                game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIDriveSpecial }), "text", bestEngineTechString);
            }
            else
            {
                game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIDriveTech }), "text", App.Localize("@UI_DIPLOMACY_TECHLEVEL_1"));
                game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIDriveSpecial }), "text", "");
            }
            game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIStatRpdValue }), "text", str4);
            GovernmentInfo governmentInfo = game.GameDatabase.GetGovernmentInfo(playerInfo.ID);
            game.UI.SetPropertyString(game.UI.Path(new string[] { panelName, UIGovernmentType }), "text", App.Localize(string.Format("@UI_EMPIRESUMMARY_{0}", governmentInfo.CurrentType.ToString().ToUpper())));
        }
        private static void SyncDiplomacyActionsState(App game, string panelName, PlayerInfo playerInfo, bool updateButtonIds)
		{
			int turnCount = game.GameDatabase.GetTurnCount();
			FactionInfo factionInfo = game.GameDatabase.GetFactionInfo(playerInfo.FactionID);
			string propertyValue = game.GameDatabase.GetPlayerInfo(game.Game.LocalPlayer.ID).FactionDiplomacyPoints[factionInfo.ID].ToString();
			List<DiplomacyActionHistoryEntryInfo> list = game.GameDatabase.GetDiplomacyActionHistory(game.Game.LocalPlayer.ID, playerInfo.ID, turnCount, 1).ToList<DiplomacyActionHistoryEntryInfo>();
			game.UI.ClearItems(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIPendingActions
			}));
			foreach (DiplomacyActionHistoryEntryInfo current in list)
			{
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UIPendingActions
				}), string.Empty, current.ID, string.Empty);
				string itemGlobalID = game.UI.GetItemGlobalID(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UIPendingActions
				}), string.Empty, current.ID, string.Empty);
				game.UI.SetEnabled(itemGlobalID, false);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID,
					"txtTurn"
				}), "text", current.TurnCount.ToString());
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID,
					"txtInteraction"
				}), "text", DiplomacyUI.GetDiplomacyActionString(game, current));
			}
			string arg = panelName.Split(new char[]
			{
				'.'
			}).First<string>();
			bool isStandardPlayer = playerInfo.isStandardPlayer;
			game.UI.SetButtonText(isStandardPlayer ? string.Format("{0}|{1}", arg, DiplomacyUI.UIDeclareButton) : game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIDeclareButton
			}), string.Format(App.Localize("@UI_DIPLOMACY_DECLARE"), game.Game.GetDiplomacyActionCost(DiplomacyAction.DECLARATION, null, null)));
			game.UI.SetEnabled(isStandardPlayer ? string.Format("{0}|{1}", arg, DiplomacyUI.UISurpriseAttackButton) : game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UISurpriseAttackButton
			}), DiplomacyUI.EnableSupriseButton(game.Game, playerInfo));
			game.UI.SetEnabled(isStandardPlayer ? string.Format("{0}|{1}", arg, DiplomacyUI.UIDeclareButton) : game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIDeclareButton
			}), DiplomacyUI.EnableDeclareButton(game.Game, playerInfo));
			game.UI.SetEnabled(isStandardPlayer ? string.Format("{0}|{1}", arg, DiplomacyUI.UIRequestButton) : game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIRequestButton
			}), DiplomacyUI.EnableRequestButton(game.Game, playerInfo));
			game.UI.SetEnabled(isStandardPlayer ? string.Format("{0}|{1}", arg, DiplomacyUI.UIDemandButton) : game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIDemandButton
			}), DiplomacyUI.EnableDemandButton(game.Game, playerInfo));
			game.UI.SetEnabled(isStandardPlayer ? string.Format("{0}|{1}", arg, DiplomacyUI.UITreatyButton) : game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UITreatyButton
			}), DiplomacyUI.EnableTreatyButton(game.Game, playerInfo));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIActionRdpValue
			}), "text", propertyValue);
		}
		private static void SyncPlayerHistoryState(App game, string panelName, PlayerInfo playerInfo, bool updateButtonIds)
		{
			int turnCount = game.GameDatabase.GetTurnCount();
			game.UI.ClearItems(DiplomacyUI.UINewsList);
			List<DiplomacyReactionHistoryEntryInfo> list = game.GameDatabase.GetDiplomacyReactionHistory(game.Game.LocalPlayer.ID, playerInfo.ID, turnCount, 10).ToList<DiplomacyReactionHistoryEntryInfo>();
			Vector3 vector = new Vector3(0.2f, 0.8f, 0.2f);
			Vector3 vector2 = new Vector3(0.8f, 0.2f, 0.2f);
			foreach (DiplomacyReactionHistoryEntryInfo current in list)
			{
				Vector3 value = (current.Difference > 0) ? vector : vector2;
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UINewsList
				}), string.Empty, current.ID, string.Empty);
				string itemGlobalID = game.UI.GetItemGlobalID(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UINewsList
				}), string.Empty, current.ID, string.Empty);
				game.UI.SetEnabled(itemGlobalID, false);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID,
					"txtTurn"
				}), "text", current.TurnCount.ToString());
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID,
					"txtType"
				}), "text", App.Localize("@" + current.Reaction.ToString()));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID,
					"txtChange"
				}), "text", (current.Difference > 0) ? string.Format("+{0}", current.Difference) : current.Difference.ToString());
				game.UI.SetPropertyColorNormalized(game.UI.Path(new string[]
				{
					itemGlobalID,
					"txtChange"
				}), "color", value);
			}
			game.UI.ClearItems(DiplomacyUI.UIInteractionsList);
			List<DiplomacyActionHistoryEntryInfo> list2 = game.GameDatabase.GetDiplomacyActionHistory(game.Game.LocalPlayer.ID, playerInfo.ID, turnCount - 1, 10).ToList<DiplomacyActionHistoryEntryInfo>();
			foreach (DiplomacyActionHistoryEntryInfo current2 in list2)
			{
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UIInteractionsList
				}), string.Empty, current2.ID, string.Empty);
				string itemGlobalID2 = game.UI.GetItemGlobalID(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UIInteractionsList
				}), string.Empty, current2.ID, string.Empty);
				game.UI.SetEnabled(itemGlobalID2, false);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID2,
					"txtTurn"
				}), "text", current2.TurnCount.ToString());
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					itemGlobalID2,
					"txtInteraction"
				}), "text", DiplomacyUI.GetDiplomacyActionString(game, current2));
			}
		}
		private static void AddIntelListItem(App game, string panelName, string entry, int itemid)
		{
			game.UI.AddItem(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIIntelList
			}), string.Empty, itemid, entry);
			game.UI.GetItemGlobalID(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIIntelList
			}), string.Empty, itemid, string.Empty);
		}
		public static void ClearIntelList(App game, string panelName)
		{
			game.UI.ClearItems(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIIntelList
			}));
		}
		private static void SyncEspionageState(App game, string panelName, PlayerInfo playerInfo, bool updateButtonIds)
		{
			PlayerInfo playerInfo2 = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				panelName.Split(new char[]
				{
					'.'
				})[0] + "|btnIntel",
				"lblIntelLabel"
			}), "text", string.Format(App.Localize("@UI_DIPLOMACY_INTEL"), playerInfo2.IntelPoints));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				panelName.Split(new char[]
				{
					'.'
				})[0] + "|btnCounterIntel",
				"lblCounterIntelLabel"
			}), "text", string.Format(App.Localize("@UI_DIPLOMACY_COUNTER_INTEL"), playerInfo2.CounterIntelPoints));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				panelName.Split(new char[]
				{
					'.'
				})[0] + "|btnOperations",
				"lblOperationsLabel"
			}), "text", string.Format(App.Localize("@UI_DIPLOMACY_OPERATIONS"), playerInfo2.OperationsPoints));
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				panelName.Split(new char[]
				{
					'.'
				})[0] + "|btnCounterIntel"
			}), playerInfo2.CounterIntelPoints >= game.AssetDatabase.RequiredCounterIntelPointsForMission);
			DiplomacyUI.ClearIntelList(game, panelName);
			int num = 0;
			List<IntelMissionInfo> list = (
				from x in game.GameDatabase.GetIntelInfosForPlayer(game.LocalPlayer.ID)
				where x.TargetPlayerId == playerInfo.ID
				select x).ToList<IntelMissionInfo>();
			foreach (IntelMissionInfo current in list)
			{
				DiplomacyUI.AddIntelListItem(game, panelName, "Pending Intel", current.ID);
				num = ((num > current.ID) ? num : current.ID);
			}
			num++;
			List<TurnEvent> list2 = (
				from x in game.GameDatabase.GetTurnEventsByTurnNumber(game.GameDatabase.GetTurnCount(), game.LocalPlayer.ID)
				where x.EventType == TurnEventType.EV_INTEL_MISSION_CRITICAL_FAILED || x.EventType == TurnEventType.EV_INTEL_MISSION_FAILED
				select x).ToList<TurnEvent>();
			foreach (TurnEvent arg_2C7_0 in list2)
			{
				DiplomacyUI.AddIntelListItem(game, panelName, "Pending Intel", num);
				num++;
			}
			game.UI.ClearItems(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UICounterIntelList
			}));
			List<CounterIntelStingMission> list3 = game.GameDatabase.GetCountIntelStingsForPlayerAgainstPlayer(game.LocalPlayer.ID, playerInfo.ID).ToList<CounterIntelStingMission>();
			foreach (CounterIntelStingMission current2 in list3)
			{
				game.UI.AddItem(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UICounterIntelList
				}), string.Empty, current2.ID, string.Format(App.Localize("@UI_COUNTER_INTEL_LIST_ITEM"), new object[0]));
			}
		}
		public static DiplomacyCardState GetPreviousDiplomacyCardState(DiplomacyCardState cardState)
		{
			if (cardState == DiplomacyCardState.PlayerStats)
			{
				DiplomacyCardState[] array = (DiplomacyCardState[])Enum.GetValues(typeof(DiplomacyCardState));
				return array[array.Count<DiplomacyCardState>() - 1];
			}
			return cardState - 1;
		}
		public static DiplomacyCardState GetNextDiplomacyCardState(DiplomacyCardState cardState)
		{
			DiplomacyCardState[] array = (DiplomacyCardState[])Enum.GetValues(typeof(DiplomacyCardState));
			if (cardState == (DiplomacyCardState)(array.Count<DiplomacyCardState>() - 1))
			{
				return array[0];
			}
			return cardState + 1;
		}
		public static void SyncDiplomacyPopup(App game, string panelName, int playerId)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(playerId);
			DiplomacyInfo diplomacyInfo = game.GameDatabase.GetDiplomacyInfo(game.Game.LocalPlayer.ID, playerInfo.ID);
			DiplomaticMood diplomaticMood = diplomacyInfo.GetDiplomaticMood();
			game.UI.SetVisible(DiplomacyUI.UIRelation, true);
			if (diplomaticMood == DiplomaticMood.Love)
			{
				game.UI.SetPropertyString(DiplomacyUI.UIRelation, "sprite", "Love");
			}
			else
			{
				if (diplomaticMood == DiplomaticMood.Hatred)
				{
					game.UI.SetPropertyString(DiplomacyUI.UIRelation, "sprite", "Hate");
				}
				else
				{
					game.UI.SetVisible(DiplomacyUI.UIRelation, false);
				}
			}
			DiplomacyUI.SyncPanelColor(game, game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIPanelBackground
			}), playerInfo.PrimaryColor);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIAvatar
			}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIBadge
			}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UICardPlayerName
			}), "text", playerInfo.Name);
			if (panelName == DiplomacyUI.UILobbyPanel)
			{
				foreach (PlayerInfo current in game.GameDatabase.GetStandardPlayerInfos())
				{
					if (current.ID != playerId)
					{
						game.UI.AddItem(DiplomacyUI.UILobbyPlayerList, "", current.ID, current.Name);
					}
				}
				game.UI.SetSelection(DiplomacyUI.UILobbyPlayerList, game.LocalPlayer.ID);
			}
		}
		public static void HideAllPlayerDiplomacyCards(App game)
		{
			for (int i = 0; i < 7; i++)
			{
				game.UI.SetVisible("Player" + i, false);
			}
		}
		public static void SyncIndyDiplomacyCard(App game, string panelName, int playerId)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(playerId);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UICardPlayerName
			}), "text", playerInfo.Name);
			DiplomacyUI.SyncPanelColor(game, game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIPanelBackground
			}), playerInfo.PrimaryColor);
			DiplomacyUI.CardStateFunctionMap[DiplomacyCardState.DiplomacyActions](game, game.UI.Path(new string[]
			{
				panelName,
				"stateDiplomacyActions"
			}), playerInfo, false);
			DiplomacyUI.CardStateFunctionMap[DiplomacyCardState.PlayerHistory](game, game.UI.Path(new string[]
			{
				panelName,
				"statePlayerHistory"
			}), playerInfo, false);
			DiplomacyUI.CardStateFunctionMap[DiplomacyCardState.PlayerStats](game, game.UI.Path(new string[]
			{
				panelName,
				"statePlayerStats"
			}), playerInfo, false);
		}
		public static void SyncPlayerDiplomacyCard(App game, string panelName, int playerId, DiplomacyCardState cardState, bool updateButtonIds)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(playerId);
			if (updateButtonIds)
			{
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UICardPreviousState
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UICardPreviousState));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					DiplomacyUI.UICardNextState
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UICardNextState));
				game.UI.SetButtonText(game.UI.Path(new string[]
				{
					panelName,
					"stateDiplomacyActions",
					DiplomacyUI.UIDeclareButton
				}), string.Format(App.Localize("@UI_DIPLOMACY_DECLARE"), game.AssetDatabase.DeclareWarPointCost));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateDiplomacyActions",
					DiplomacyUI.UISurpriseAttackButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UISurpriseAttackButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateDiplomacyActions",
					DiplomacyUI.UIDeclareButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UIDeclareButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateDiplomacyActions",
					DiplomacyUI.UIRequestButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UIRequestButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateDiplomacyActions",
					DiplomacyUI.UIDemandButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UIDemandButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateDiplomacyActions",
					DiplomacyUI.UITreatyButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UITreatyButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateDiplomacyActions",
					DiplomacyUI.UILobbyButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UILobbyButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateDiplomacyActions",
					DiplomacyUI.UIGiveButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UIGiveButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateEspionage",
					DiplomacyUI.UIIntelButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UIIntelButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateEspionage",
					DiplomacyUI.UICounterIntelButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UICounterIntelButton));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"stateEspionage",
					DiplomacyUI.UIOperationsButton
				}), "id", string.Format("{0}|{1}", panelName, DiplomacyUI.UIOperationsButton));
			}
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				string.Format("{0}|{1}", panelName, DiplomacyUI.UICardPreviousState)
			}), !playerInfo.isDefeated);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				string.Format("{0}|{1}", panelName, DiplomacyUI.UICardNextState)
			}), !playerInfo.isDefeated);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateDiplomacyActions",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UISurpriseAttackButton)
			}), !playerInfo.isDefeated);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateDiplomacyActions",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UIDeclareButton)
			}), !playerInfo.isDefeated);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateDiplomacyActions",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UIRequestButton)
			}), !playerInfo.isDefeated);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateDiplomacyActions",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UIDemandButton)
			}), !playerInfo.isDefeated);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateDiplomacyActions",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UITreatyButton)
			}), !playerInfo.isDefeated);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateDiplomacyActions",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UIGiveButton)
			}), !playerInfo.isDefeated && game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID).Savings > 0.0);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateDiplomacyActions",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UILobbyButton)
			}), !playerInfo.isDefeated && game.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.LOBBY, null, null));
			bool flag = (game.AssetDatabase.GetFaction(playerInfo.FactionID).Name == "loa" && game.GameDatabase.PlayerHasTech(game.LocalPlayer.ID, "CCC_Artificial_Intelligence")) || game.AssetDatabase.GetFaction(playerInfo.FactionID).Name != "loa";
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateEspionage",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UIIntelButton)
			}), !playerInfo.isDefeated && flag);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateEspionage",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UICounterIntelButton)
			}), !playerInfo.isDefeated && flag);
			game.UI.SetEnabled(game.UI.Path(new string[]
			{
				panelName,
				"stateEspionage",
				string.Format("{0}|{1}", panelName, DiplomacyUI.UIOperationsButton)
			}), !playerInfo.isDefeated && flag);
			foreach (DiplomacyCardState diplomacyCardState in Enum.GetValues(typeof(DiplomacyCardState)))
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					string.Format("state{0}", diplomacyCardState.ToString())
				}), playerInfo.isDefeated ? (diplomacyCardState == DiplomacyCardState.PlayerStats) : (diplomacyCardState == cardState));
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UICardPlayerName
			}), "text", playerInfo.Name);
			DiplomacyUI.SyncEspionageState(game, panelName, playerInfo, updateButtonIds);
			DiplomacyUI.SyncPanelColor(game, game.UI.Path(new string[]
			{
				panelName,
				DiplomacyUI.UIPanelBackground
			}), playerInfo.PrimaryColor);
			DiplomacyUI.CardStateFunctionMap[cardState](game, game.UI.Path(new string[]
			{
				panelName,
				string.Format("state{0}", cardState.ToString())
			}), playerInfo, updateButtonIds);
		}
	}
}
