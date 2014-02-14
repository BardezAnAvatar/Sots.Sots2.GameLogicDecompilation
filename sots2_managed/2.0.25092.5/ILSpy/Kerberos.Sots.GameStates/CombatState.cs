using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
namespace Kerberos.Sots.GameStates
{
	internal class CombatState : CommonCombatState
	{
		private static readonly string UIExitButton = "gameExitButton";
		public CombatState(App game) : base(game)
		{
		}
		protected override GameState GetExitState()
		{
			return base.App.GetGameState<StarMapState>();
		}
		protected override void UICommChannel_OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked" && panelName == CombatState.UIExitButton && base.EndCombat())
			{
				base.App.UI.SetEnabled(CombatState.UIExitButton, false);
			}
		}
		protected override void OnCombatEnding()
		{
			base.App.UI.SetEnabled(CombatState.UIExitButton, false);
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			base.SimMode = false;
			base.OnPrepare(prev, stateParams);
			base.App.UI.LoadScreen("Combat");
		}
		protected override void OnEnter()
		{
			base.OnEnter();
			base.App.UI.SetScreen("Combat");
			base.App.UI.SetEnabled(CombatState.UIExitButton, true);
			base.App.UI.SetPropertyInt("gameWeaponsPanel", "combat_input", base.Input.ObjectID);
			base.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_COMBAT_CONNECT_UI,
				base.Combat.ObjectID,
				"Combat"
			});
			this.SyncPlayerList();
		}
		protected override void SyncPlayerList()
		{
			base.App.UI.ClearItems("combatPlayers");
			base.App.UI.ClearItems("remainingPlayers");
			List<Player> list = new List<Player>();
			List<Player> list2 = new List<Player>();
			List<Player> list3 = new List<Player>();
			foreach (Player current in this._playersInCombat)
			{
				if (current.ID == base.App.LocalPlayer.ID)
				{
					list.Add(current);
				}
				else
				{
					DiplomacyState diplomacyState = base.GetDiplomacyState(base.App.LocalPlayer.ID, current.ID);
					if (diplomacyState == DiplomacyState.CEASE_FIRE || diplomacyState == DiplomacyState.NON_AGGRESSION || diplomacyState == DiplomacyState.NEUTRAL)
					{
						list2.Add(current);
					}
					else
					{
						if (diplomacyState == DiplomacyState.WAR)
						{
							list3.Add(current);
						}
						else
						{
							if (diplomacyState == DiplomacyState.PEACE || diplomacyState == DiplomacyState.ALLIED)
							{
								list.Add(current);
							}
						}
					}
				}
			}
			if (this._systemId > 0)
			{
				base.App.UI.SetPropertyString("systemName", "text", base.App.GameDatabase.GetStarSystemInfo(this._systemId).Name);
			}
			else
			{
				base.App.UI.SetPropertyString("systemName", "text", App.Localize("@ADMIRAL_LOCATION_DEEP_SPACE"));
			}
			foreach (Player current2 in list)
			{
				if (current2.ID != base.App.LocalPlayer.ID)
				{
					base.App.UI.AddItem("remainingPlayers", "", current2.ID, "", "smallPlayerCard");
					string itemGlobalID = base.App.UI.GetItemGlobalID("remainingPlayers", "", current2.ID, "");
					this.SyncPlayerCard(itemGlobalID, current2.ID, true, false);
				}
			}
			if (list2.Count > 0)
			{
				foreach (Player current3 in list2)
				{
					base.App.UI.AddItem("remainingPlayers", "", current3.ID, "", "smallPlayerCard");
					string itemGlobalID2 = base.App.UI.GetItemGlobalID("remainingPlayers", "", current3.ID, "");
					this.SyncPlayerCard(itemGlobalID2, current3.ID, false, false);
				}
			}
			if (list3.Count > 0)
			{
				foreach (Player current4 in list3)
				{
					base.App.UI.AddItem("combatPlayers", "", current4.ID, "", "smallPlayerCard");
					string itemGlobalID3 = base.App.UI.GetItemGlobalID("combatPlayers", "", current4.ID, "");
					this.SyncPlayerCard(itemGlobalID3, current4.ID, false, false);
				}
				base.App.UI.AddItem("combatPlayers", "", 99999999, "", "smallPlayerCard");
				string itemGlobalID4 = base.App.UI.GetItemGlobalID("combatPlayers", "", 99999999, "");
				this.SyncPlayerCard(itemGlobalID4, 99999999, false, true);
			}
			base.App.UI.AddItem("combatPlayers", "", base.App.LocalPlayer.ID, "", "smallPlayerCard");
			string itemGlobalID5 = base.App.UI.GetItemGlobalID("combatPlayers", "", base.App.LocalPlayer.ID, "");
			this.SyncPlayerCard(itemGlobalID5, base.App.LocalPlayer.ID, false, false);
		}
		protected void SyncPlayerCard(string card, int playerID, bool isally, bool vscard = false)
		{
			if (!vscard)
			{
				PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(playerID);
				base.App.UI.SetPropertyString(base.App.UI.Path(new string[]
				{
					card,
					"smallAvatar"
				}), "sprite", Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath));
				base.App.UI.SetVisible(base.App.UI.Path(new string[]
				{
					card,
					"ally"
				}), isally);
				CombatState.SetPlayerCardOutlineColor(base.App, base.App.UI.Path(new string[]
				{
					card,
					"bgPlayerColor"
				}), playerInfo.PrimaryColor);
				return;
			}
			base.App.UI.SetVisible(base.App.UI.Path(new string[]
			{
				card,
				"ally"
			}), false);
			base.App.UI.SetVisible(base.App.UI.Path(new string[]
			{
				card,
				"smallAvatar"
			}), false);
			base.App.UI.SetVisible(base.App.UI.Path(new string[]
			{
				card,
				"vsText"
			}), true);
			base.App.UI.SetVisible(base.App.UI.Path(new string[]
			{
				card,
				"bgPlayerColor"
			}), false);
		}
		public static void SetPlayerCardOutlineColor(App game, string panelName, Vector3 color)
		{
			List<string> list = new List<string>
			{
				"BOL1",
				"BOL2",
				"BOL3",
				"BOL4",
				"BOL5",
				"BOL6",
				"BOL7",
				"BOL8",
				"L_Cap",
				"R_Cap",
				"PC_OWNER_S"
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
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.OnExit(prev, reason);
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			if (messageID == InteropMessageID.IMID_SCRIPT_MOVE_ORDER)
			{
				int playerID = mr.ReadInteger();
				CombatAI commanderForPlayerID = base.GetCommanderForPlayerID(playerID);
				if (commanderForPlayerID != null)
				{
					return;
				}
			}
			else
			{
				base.OnEngineMessage(messageID, mr);
			}
		}
	}
}
