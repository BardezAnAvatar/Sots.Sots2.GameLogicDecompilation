using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.IO;
namespace Kerberos.Sots.GameStates
{
	internal static class EmpireBarUI
	{
		public const string UISavings = "gameEmpireSavings";
		public const string UIPsiLevel = "lblPsiValue";
		public const string UIResearchSlider = "gameEmpireResearchSlider";
		public const string UITitleFrame = "gameScreenFrame";
		public const string UIPsiPanel = "pnlPsiLevel";
		public static void SyncTitleFrame(App game)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			Vector3 primaryColor = playerInfo.PrimaryColor;
			game.UI.SetPropertyColorNormalized("gameScreenFrame", "empire_color", primaryColor);
		}
		public static void SyncTitleBar(App game, string panelId, BudgetPiechart piechart)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			Vector3 primaryColor = playerInfo.PrimaryColor;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath);
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath);
			game.UI.SetPropertyString(panelId, "avatar", fileNameWithoutExtension2);
			game.UI.SetPropertyString(panelId, "badge", fileNameWithoutExtension);
			game.UI.SetPropertyString(panelId, "name", playerInfo.Name.ToUpper());
			game.UI.SetPropertyColorNormalized(panelId, "empire_color", primaryColor);
			string propertyValue = string.Format("{0} {1}", App.Localize("@UI_GENERAL_TURN"), game.GameDatabase.GetTurnCount());
			game.UI.SetPropertyString("turn_count", "text", propertyValue);
			long num = (long)playerInfo.Savings;
			long num2 = (long)playerInfo.PsionicPotential;
			game.UI.SetPropertyString("gameEmpireSavings", "text", num.ToString("N0"));
			game.UI.SetVisible("pnlPsiLevel", (game.GameDatabase.PlayerHasTech(game.LocalPlayer.ID, "PSI_Clairvoyance") | game.GameDatabase.PlayerHasTech(game.LocalPlayer.ID, "PSI_Empathy") | game.GameDatabase.PlayerHasTech(game.LocalPlayer.ID, "PSI_Telekinesis")) && game.LocalPlayer.Faction.Name != "loa");
			game.UI.SetPropertyString("lblPsiValue", "text", num2.ToString("N0"));
			EmpireHistoryData lastEmpireHistoryForPlayer = game.GameDatabase.GetLastEmpireHistoryForPlayer(game.LocalPlayer.ID);
			Vector3 value = new Vector3(255f, 255f, 255f);
			Vector3 value2 = new Vector3(0f, 255f, 0f);
			Vector3 value3 = new Vector3(255f, 0f, 0f);
			if (lastEmpireHistoryForPlayer != null)
			{
				if (num2 > (long)lastEmpireHistoryForPlayer.psi_potential)
				{
					game.UI.SetPropertyColor("lblPsiValue", "color", value2);
				}
				else
				{
					if (num2 < (long)lastEmpireHistoryForPlayer.psi_potential)
					{
						game.UI.SetPropertyColor("lblPsiValue", "color", value3);
					}
					else
					{
						game.UI.SetPropertyColor("lblPsiValue", "color", value);
					}
				}
			}
			EmpireBarUI.SyncResearchSlider(game, "gameEmpireResearchSlider", playerInfo, piechart);
		}
		public static void SyncResearchSlider(App game, string sliderId, BudgetPiechart piechart)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			EmpireBarUI.SyncResearchSlider(game, sliderId, playerInfo, piechart);
		}
		private static void SyncResearchSlider(App game, string sliderId, PlayerInfo playerInfo, BudgetPiechart piechart)
		{
			int num = (int)(playerInfo.RateGovernmentResearch * 100f);
			game.UI.SetSliderValue(sliderId, 100 - num);
			if (piechart != null)
			{
				Budget slices = Budget.GenerateBudget(game.Game, playerInfo, null, BudgetProjection.Pessimistic);
				piechart.SetSlices(slices);
			}
		}
	}
}
