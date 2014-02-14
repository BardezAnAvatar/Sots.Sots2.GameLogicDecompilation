using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class GameSetupUI
	{
		private static void SyncPlayerSlotControl(App game, string panelName, int itemId, PlayerSetup player, bool rebuildIDs)
		{
			Vector4 value = new Vector4(0f, 0f, 0f, 0f);
			Vector4 value2 = new Vector4(0f, 0f, 0f, 0f);
			if (player != null)
			{
				string text = null;
				if (!string.IsNullOrEmpty(player.Faction))
				{
					Faction faction = game.AssetDatabase.GetFaction(player.Faction);
					text = Path.GetFileNameWithoutExtension(faction.NoAvatar);
				}
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"pnlPlayer"
				}), true);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"pnlEmpty"
				}), false);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"btnPlayer"
				}), "id", string.Format("{0}|btnPlayer", itemId));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"lblEmpireName"
				}), "text", player.EmpireName ?? string.Empty);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"lblName"
				}), "text", player.Name ?? string.Empty);
				if (player.Faction != null)
				{
					game.UI.SetPropertyString(game.UI.Path(new string[]
					{
						panelName,
						"lblFaction"
					}), "text", (!string.IsNullOrEmpty(player.Faction)) ? (App.GetLocalizedFactionName(player.Faction) ?? string.Empty) : string.Empty);
				}
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"imgAvatar"
				}), "sprite", (!string.IsNullOrEmpty(player.Avatar)) ? player.Avatar : (text ?? string.Empty));
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"imgBadge"
				}), "sprite", (!string.IsNullOrEmpty(player.Badge)) ? player.Badge : string.Empty);
				if (player.EmpireColor.HasValue && player.EmpireColor.Value >= 0 && player.EmpireColor.Value < Player.DefaultPrimaryPlayerColors.Count<Vector3>())
				{
					value = new Vector4(Player.DefaultPrimaryPlayerColors[player.EmpireColor.Value] * 255f, 255f);
				}
				value2 = new Vector4(player.ShipColor * 255f, 255f);
				if (player.Ready)
				{
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						panelName,
						"imgReady"
					}), true);
				}
				else
				{
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						panelName,
						"imgReady"
					}), false);
				}
				if (player.Locked)
				{
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						panelName,
						"eliminatedState"
					}), true);
				}
				else
				{
					game.UI.SetVisible(game.UI.Path(new string[]
					{
						panelName,
						"eliminatedState"
					}), false);
				}
				if (rebuildIDs)
				{
					game.UI.SetPropertyString(game.UI.Path(new string[]
					{
						panelName,
						"team_button"
					}), "id", "team_button|" + player.slot.ToString());
				}
				game.UI.SetText(game.UI.Path(new string[]
				{
					panelName,
					"team_button|" + player.slot.ToString(),
					"team_label"
				}), (player.Team != 0) ? (App.Localize("@UI_GAMESETUP_TEAM") + " " + player.Team) : App.Localize("@UI_GAMESETUP_NOTEAM"));
				game.UI.SetPropertyColor(game.UI.Path(new string[]
				{
					panelName,
					"team_button|" + player.slot.ToString(),
					"team_label"
				}), "color", (player.Team != 0) ? (Player.DefaultPrimaryTeamColors[player.Team - 1] * 255f) : new Vector3(255f, 255f, 255f));
				List<string> list = new List<string>
				{
					"TC",
					"BC",
					"BOL1",
					"BOL2",
					"BOL3",
					"BOL4",
					"BOL5",
					"BOL6",
					"BOL7",
					"BOL8"
				};
				foreach (string current in list)
				{
					game.UI.SetPropertyColor(game.UI.Path(new string[]
					{
						panelName,
						"team_button|" + player.slot.ToString(),
						"idle",
						current
					}), "color", (player.Team != 0) ? (Player.DefaultPrimaryTeamColors[player.Team - 1] * 255f) : new Vector3(255f, 255f, 255f));
					game.UI.SetPropertyColor(game.UI.Path(new string[]
					{
						panelName,
						"team_button|" + player.slot.ToString(),
						"mouse_over",
						current
					}), "color", (player.Team != 0) ? (Player.DefaultPrimaryTeamColors[player.Team - 1] * 255f) : new Vector3(255f, 255f, 255f));
					game.UI.SetPropertyColor(game.UI.Path(new string[]
					{
						panelName,
						"team_button|" + player.slot.ToString(),
						"pressed",
						current
					}), "color", (player.Team != 0) ? (Player.DefaultPrimaryTeamColors[player.Team - 1] * 255f) : new Vector3(255f, 255f, 255f));
					game.UI.SetPropertyColor(game.UI.Path(new string[]
					{
						panelName,
						"team_button|" + player.slot.ToString(),
						"disabled",
						current
					}), "color", (player.Team != 0) ? (Player.DefaultPrimaryTeamColors[player.Team - 1] * 255f) : new Vector3(255f, 255f, 255f));
				}
				game.UI.SetEnabled(game.UI.Path(new string[]
				{
					panelName,
					"team_button|" + player.slot.ToString()
				}), ((game.GameSetup.IsMultiplayer && game.Network.IsHosting) || !game.GameSetup.IsMultiplayer) && !player.Fixed);
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"pnlPlayer"
				}), false);
				game.UI.SetVisible(game.UI.Path(new string[]
				{
					panelName,
					"pnlEmpty"
				}), true);
				game.UI.SetPropertyString(game.UI.Path(new string[]
				{
					panelName,
					"btnPlayer"
				}), "id", string.Format("{0}|btnPlayer", itemId));
			}
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"LC"
			}), "color", value);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"RC"
			}), "color", value);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"BG"
			}), "color", value);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"RC"
			}), "color", value2);
		}
		internal static void SyncPlayerSetupWidget(App game, string panelName, PlayerSetup player)
		{
			string text = null;
			if (!string.IsNullOrEmpty(player.Faction))
			{
				Faction faction = game.AssetDatabase.GetFaction(player.Faction);
				text = Path.GetFileNameWithoutExtension(faction.NoAvatar);
			}
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"imgFaction"
			}), "sprite", App.GetFactionIcon(player.Faction));
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"imgAvatar"
			}), "sprite", string.IsNullOrEmpty(player.Avatar) ? (text ?? string.Empty) : player.Avatar);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"imgBadge"
			}), "sprite", player.Badge ?? string.Empty);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"lblPlayerName"
			}), "text", player.Name ?? string.Empty);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"lblEmpireName"
			}), "text", string.IsNullOrEmpty(player.EmpireName) ? App.Localize("@GAMESETUP_RANDOM_EMPIRE_NAME") : player.EmpireName);
			game.UI.SetPropertyString(game.UI.Path(new string[]
			{
				panelName,
				"lblFactionDescription"
			}), "text", App.GetFactionDescription(player.Faction));
			Vector4 value = new Vector4(0f, 0f, 0f, 0f);
			Vector4 value2 = new Vector4(0f, 0f, 0f, 0f);
			if (player.EmpireColor.HasValue)
			{
				value = new Vector4(Player.DefaultPrimaryPlayerColors[player.EmpireColor.Value] * 255f, 255f);
			}
			value2 = new Vector4(player.ShipColor * 255f, 255f);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"imgEmpireColor"
			}), "color", value);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"sample"
			}), "color", value2);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"LC"
			}), "color", value);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"RC"
			}), "color", value);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"BG"
			}), "color", value);
			game.UI.SetPropertyColor(game.UI.Path(new string[]
			{
				panelName,
				"RC"
			}), "color", value2);
		}
		internal static void SyncPlayerListWidget(App game, string panelName, List<PlayerSetup> players, bool rebuildPlayerList = true)
		{
			if (rebuildPlayerList)
			{
				game.UI.ClearItems(panelName);
			}
			foreach (PlayerSetup current in players)
			{
				if (rebuildPlayerList)
				{
					game.UI.AddItem(panelName, string.Empty, current.slot, string.Empty);
				}
				string itemGlobalID = game.UI.GetItemGlobalID(panelName, string.Empty, current.slot, string.Empty);
				GameSetupUI.SyncPlayerSlotControl(game, itemGlobalID, current.slot, current, rebuildPlayerList);
			}
		}
		internal static void ClearPlayerListWidget(App game, string panelName)
		{
			game.UI.ClearItems(panelName);
		}
	}
}
